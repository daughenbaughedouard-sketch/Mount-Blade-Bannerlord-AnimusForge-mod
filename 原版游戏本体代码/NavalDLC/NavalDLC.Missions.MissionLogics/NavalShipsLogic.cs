using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.AI.Behaviors;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipControl;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

public class NavalShipsLogic : MissionLogic, IVehicleHandler, IMissionBehavior
{
	public enum NavalBoundaryCheckType
	{
		HardBoundary,
		DeploymentBoundary
	}

	public delegate bool ShipFilter(ShipAssignment assignment);

	public const int MaxTeamShipDeploymentLimit = 8;

	private readonly Dictionary<int, Missile> _shipSiegeEngineMissileDictionary;

	private readonly Dictionary<int, float> _missileHittingSailDictionary;

	private readonly ShipAssignment[,] _shipAssignments;

	private readonly MBList<MissionShip> _allShips;

	private readonly NavalShipDeploymentLimit[] _teamDeploymentLimits;

	private readonly MBList<MissionShip> _removedShipsPool;

	private int _shipIndexGenerator;

	private readonly PriorityQueue<int, (int distance, Vec2i index)> _tmpCollisionFreeFrameSearchQueue;

	public MissionShip PlayerControlledShip { get; private set; }

	public bool SceneHasNavMeshForPathFinding { get; private set; }

	public bool IsTeleportingShips { get; private set; }

	public bool IsMissionEnding { get; private set; }

	public bool IsDeploymentMode { get; private set; }

	public MBReadOnlyList<MissionShip> AllShips => (MBReadOnlyList<MissionShip>)(object)_allShips;

	public bool CanHaveConnectionCooldown { get; private set; } = true;

	public event Action<MissionShip> ShipSpawnedEvent;

	public event Action<MissionShip, Formation> BeforeShipTransferredToFormationEvent;

	public event Action<MissionShip, Formation> ShipTransferredToFormationEvent;

	public event Action<MissionShip, Team, Formation> BeforeShipTransferredToTeamEvent;

	public event Action<MissionShip, Team, Formation> ShipTransferredToTeamEvent;

	public event Action<MissionShip, MissionShip, Formation, Formation> ShipCapturedEvent;

	public event Action<MissionShip> ShipSunkEvent;

	public event Action<ShipAttachmentMachine, ShipAttachmentPointMachine> ShipAttachmentBrokenEvent;

	public event Action<MissionShip, MatrixFrame, MatrixFrame> ShipTeleportedEvent;

	public event Action<MissionShip> BeforeShipRemovedEvent;

	public event Action<MissionShip> ShipRemovedEvent;

	public event Action<MissionShip> ShipControllerChanged;

	public event Action<MissionShip, MissionShip, float, bool, CapsuleData, int> ShipRammingEvent;

	public event Action<MissionShip, MissionShip> ShipsConnectedEvent;

	public event Action<MissionShip, WeakGameEntity, Vec3, Vec3, bool> ShipCollisionEvent;

	public event Action<MissionShip, MissionShip> ShipHookThrowEvent;

	public event Action MissionEndEvent;

	public event Action<MissionShip, Agent, int, Vec3, Vec3, MissionWeapon, int> ShipHitEvent;

	public event Action<MissionShip> ShipBurnedEvent;

	public event Action<MissionShip> SailsDeadEvent;

	public event Action<MissionShip> ShipLowHealthEvent;

	public event Action<MissionShip, MissionShip, float, float> ShipAboutToBeRammedEvent;

	public event Action<MissionShip, MissionShip> ShipAttachmentLostEvent;

	public event Action<MissionShip, MissionShip> BridgeConnectedEvent;

	public event Action<MissionShip> CutLooseOrderEvent;

	public event Action<MissionShip, MissionShip> BoardingOrderEvent;

	public event Action<Missile> AddShipSiegeEngineMissileEvent;

	public NavalShipsLogic()
	{
		_shipIndexGenerator = 0;
		_allShips = new MBList<MissionShip>();
		_tmpCollisionFreeFrameSearchQueue = new PriorityQueue<int, (int, Vec2i)>();
		_removedShipsPool = new MBList<MissionShip>();
		_shipAssignments = new ShipAssignment[3, 11];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 11; j++)
			{
				_shipAssignments[i, j] = ShipAssignment.Create((TeamSideEnum)i, (FormationClass)j);
			}
		}
		int num = 3;
		_teamDeploymentLimits = new NavalShipDeploymentLimit[num];
		for (int k = 0; k < num; k++)
		{
			_teamDeploymentLimits[k] = NavalShipDeploymentLimit.Invalid();
		}
		_shipSiegeEngineMissileDictionary = new Dictionary<int, Missile>();
		_missileHittingSailDictionary = new Dictionary<int, float>();
	}

	public bool IsMissileFromShipSiegeEngine(int missileIndex)
	{
		return _shipSiegeEngineMissileDictionary.ContainsKey(missileIndex);
	}

	public void AddShipSiegeEngineMissile(Missile missile)
	{
		if (!_shipSiegeEngineMissileDictionary.ContainsKey(((MBMissile)missile).Index))
		{
			_shipSiegeEngineMissileDictionary.Add(((MBMissile)missile).Index, missile);
			this.AddShipSiegeEngineMissileEvent?.Invoke(missile);
		}
	}

	private void AddMissileHittingSail(Missile missile)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!_missileHittingSailDictionary.ContainsKey(((MBMissile)missile).Index))
		{
			Dictionary<int, float> missileHittingSailDictionary = _missileHittingSailDictionary;
			int index = ((MBMissile)missile).Index;
			Vec3 velocity = ((MBMissile)missile).GetVelocity();
			missileHittingSailDictionary.Add(index, ((Vec3)(ref velocity)).Length);
		}
	}

	public float GetMissileVelocityLengthOnFirstSailHit(int missileIndex)
	{
		if (_missileHittingSailDictionary.TryGetValue(missileIndex, out var value))
		{
			return value;
		}
		return -1f;
	}

	public override void OnMissileRemoved(int MissileIndex)
	{
		((MissionBehavior)this).OnMissileRemoved(MissileIndex);
		if (_shipSiegeEngineMissileDictionary.ContainsKey(MissileIndex))
		{
			_shipSiegeEngineMissileDictionary.Remove(MissileIndex);
		}
		else if (_missileHittingSailDictionary.ContainsKey(MissileIndex))
		{
			_missileHittingSailDictionary.Remove(MissileIndex);
		}
	}

	public override void OnDeploymentFinished()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_allShips)
		{
			((MissionObject)item).OnDeploymentFinished();
		}
	}

	public override void OnBehaviorInitialize()
	{
		Mission.Current.OnMissileRemovedEvent += ((MissionBehavior)this).OnMissileRemoved;
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		MissionShipFactory.ResetShipUniqueBitwiseIDNext();
		SceneHasNavMeshForPathFinding = ((MissionBehavior)this).Mission.Scene.SetAbilityOfFacesWithId(1, false) > 0;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		foreach (Missile item in (List<Missile>)(object)((MissionBehavior)this).Mission.MissilesList)
		{
			MissionWeapon missileWeapon = item.Weapon;
			WeaponComponentData currentUsageItem = ((MissionWeapon)(ref missileWeapon)).CurrentUsageItem;
			if (currentUsageItem == null || !Extensions.HasAnyFlag<WeaponFlags>(currentUsageItem.WeaponFlags, (WeaponFlags)32768))
			{
				continue;
			}
			Vec3 missileOldPosition = ((MBMissile)item).GetOldPosition();
			Vec3 missilePosition = ((MBMissile)item).GetPosition();
			MissionSail missionSail = null;
			foreach (MissionShip item2 in (List<MissionShip>)(object)AllShips)
			{
				Agent shooterAgent = item.ShooterAgent;
				missileWeapon = item.Weapon;
				missionSail = item2.CheckHitSails(shooterAgent, item, in missileOldPosition, in missilePosition, in missileWeapon);
				if (missionSail != null)
				{
					HandleSailsHit(item2, missionSail, item.ShooterAgent, item, item.Weapon);
					item.PassThroughEntity(missionSail.SailEntity);
				}
			}
		}
	}

	protected override void OnEndMission()
	{
		((MissionBehavior)this).OnEndMission();
		IsMissionEnding = true;
		this.MissionEndEvent?.Invoke();
		foreach (MissionShip item in ((IEnumerable<MissionShip>)_allShips).ToList())
		{
			RemoveShip(item);
		}
		SetDeploymentMode(value: false);
		Mission.Current.OnMissileRemovedEvent -= ((MissionBehavior)this).OnMissileRemoved;
	}

	public void OnShipControllerChanged(MissionShip ship)
	{
		if (ship.IsPlayerControlled && PlayerControlledShip != ship)
		{
			PlayerControlledShip = ship;
		}
		else if (!ship.IsPlayerControlled && PlayerControlledShip == ship)
		{
			PlayerControlledShip = null;
		}
		this.ShipControllerChanged?.Invoke(ship);
	}

	public void OnShipSunk(MissionShip ship)
	{
		this.ShipSunkEvent?.Invoke(ship);
	}

	public void OnAttachmentBroken(ShipAttachmentMachine attachmentMachine, ShipAttachmentPointMachine attachmentPointMachine)
	{
		this.ShipAttachmentBrokenEvent?.Invoke(attachmentMachine, attachmentPointMachine);
	}

	public void OnShipCollision(MissionShip ship, WeakGameEntity targetEntity, Vec3 averageContactPoint, Vec3 totalImpulseOnShip, bool isFirstImpact)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		this.ShipCollisionEvent?.Invoke(ship, targetEntity, averageContactPoint, totalImpulseOnShip, isFirstImpact);
	}

	public void OnShipRamming(MissionShip rammingShip, MissionShip rammedShip, float damagePercent, bool isFirstImpact, CapsuleData capsuleData, int ramQuality)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		this.ShipRammingEvent?.Invoke(rammingShip, rammedShip, damagePercent, isFirstImpact, capsuleData, ramQuality);
	}

	public void OnShipsConnected(MissionShip ownerShip, MissionShip targetShip)
	{
		this.ShipsConnectedEvent?.Invoke(ownerShip, targetShip);
	}

	public void OnSuccessfulHookThrow(MissionShip hookingShip, MissionShip hookedShip)
	{
		this.ShipHookThrowEvent?.Invoke(hookingShip, hookedShip);
	}

	public void OnShipHit(MissionShip ship, Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		this.ShipHitEvent?.Invoke(ship, attackerAgent, damage, impactPosition, impactDirection, weapon, affectorWeaponSlotOrMissileIndex);
	}

	public void OnShipBurned(MissionShip ship)
	{
		this.ShipBurnedEvent?.Invoke(ship);
	}

	public void OnShipAttachmentLost(MissionShip hookingShip, MissionShip hookedShip)
	{
		this.ShipAttachmentLostEvent?.Invoke(hookingShip, hookedShip);
	}

	public void OnSailsDead(MissionShip ship)
	{
		this.SailsDeadEvent?.Invoke(ship);
	}

	public void OnShipLowHealth(MissionShip ship)
	{
		this.ShipLowHealthEvent?.Invoke(ship);
	}

	public void OnShipAboutToBeRammed(MissionShip rammingShip, MissionShip rammedShip, float distance, float speedInRamDirection)
	{
		this.ShipAboutToBeRammedEvent?.Invoke(rammingShip, rammedShip, distance, speedInRamDirection);
	}

	public void OnCutLooseOrder(MissionShip ship)
	{
		this.CutLooseOrderEvent?.Invoke(ship);
	}

	public void OnBoardingOrder(MissionShip boardingShip, MissionShip boardedShip)
	{
		this.BoardingOrderEvent?.Invoke(boardingShip, boardedShip);
	}

	public void OnBridgeConnected(MissionShip sourceShip, MissionShip targetShip)
	{
		this.BridgeConnectedEvent?.Invoke(sourceShip, targetShip);
	}

	public void SetDeploymentMode(bool value)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (value == IsDeploymentMode)
		{
			return;
		}
		IsDeploymentMode = value;
		if (value)
		{
			return;
		}
		foreach (MissionShip item in (List<MissionShip>)(object)_removedShipsPool)
		{
			((MissionBehavior)this).Mission.Scene.RemoveEntity(((ScriptComponentBehavior)item).GameEntity, 121);
		}
		((List<MissionShip>)(object)_removedShipsPool).Clear();
	}

	public int GetNumTeamShips(TeamSideEnum teamSide)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected I4, but got Unknown
		int num = 0;
		for (int i = 0; i < 11; i++)
		{
			if (_shipAssignments[(int)teamSide, i].HasMissionShip)
			{
				num++;
			}
		}
		return num;
	}

	public bool GetShip(Formation formation, out MissionShip ship)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return GetShip(formation.Team.TeamSide, formation.FormationIndex, out ship);
	}

	public bool GetShip(TeamSideEnum teamSide, FormationClass formationIndex, out MissionShip ship)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ShipAssignment shipAssignment = GetShipAssignment(teamSide, formationIndex);
		if (shipAssignment.HasMissionShip)
		{
			ship = shipAssignment.MissionShip;
			return true;
		}
		ship = null;
		return false;
	}

	public void FillTeamShips(TeamSideEnum teamSide, MBList<MissionShip> teamShips)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected I4, but got Unknown
		for (int i = 0; i < 11; i++)
		{
			ShipAssignment shipAssignment = _shipAssignments[(int)teamSide, i];
			if (shipAssignment.HasMissionShip)
			{
				((List<MissionShip>)(object)teamShips).Add(shipAssignment.MissionShip);
			}
		}
	}

	public ShipAssignment GetShipAssignment(TeamSideEnum teamSide, FormationClass formationIndex)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected I4, but got Unknown
		//IL_000d: Expected I4, but got Unknown
		return _shipAssignments[(int)teamSide, (int)formationIndex];
	}

	public bool GetShipAssignmentWithShipIndex(int shipIndex, out ShipAssignment shipAssignment)
	{
		ShipAssignment[,] shipAssignments = _shipAssignments;
		foreach (ShipAssignment shipAssignment2 in shipAssignments)
		{
			if (shipAssignment2.HasMissionShip && shipAssignment2.MissionShip.Index == shipIndex)
			{
				shipAssignment = shipAssignment2;
				return true;
			}
		}
		shipAssignment = null;
		return false;
	}

	public int GetCountOfSetShipAssignments(TeamSideEnum teamSide)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		ShipAssignment[,] shipAssignments = _shipAssignments;
		foreach (ShipAssignment shipAssignment in shipAssignments)
		{
			if (shipAssignment.TeamSide == teamSide && shipAssignment.IsSet)
			{
				num++;
			}
		}
		return num;
	}

	public bool GetShipWithShipIndex(int shipIndex, out MissionShip missionShip)
	{
		if (GetShipAssignmentWithShipIndex(shipIndex, out var shipAssignment))
		{
			missionShip = shipAssignment.MissionShip;
			return true;
		}
		missionShip = null;
		return false;
	}

	internal MissionShip GetConnectedTeamShip(TeamSideEnum teamSide, ulong shipUniqueBitwiseID)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected I4, but got Unknown
		MissionShip result = null;
		for (int i = 0; i < 11; i++)
		{
			ShipAssignment shipAssignment = _shipAssignments[(int)teamSide, i];
			if (!shipAssignment.HasMissionShip)
			{
				continue;
			}
			MissionShip missionShip = shipAssignment.MissionShip;
			if ((missionShip.ShipIslandCombinedID & shipUniqueBitwiseID) != 0L)
			{
				if (missionShip.ShipUniqueBitwiseID == shipUniqueBitwiseID)
				{
					return missionShip;
				}
				result = missionShip;
			}
		}
		return result;
	}

	internal MissionShip GetNearestTeamShip(TeamSideEnum teamSide, in Vec3 position, float maxDistance = float.MaxValue, Func<MissionShip, bool> shipFilter = null)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected I4, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		MissionShip result = null;
		float num = maxDistance;
		for (int i = 0; i < 11; i++)
		{
			ShipAssignment shipAssignment = _shipAssignments[(int)teamSide, i];
			if (!shipAssignment.HasMissionShip)
			{
				continue;
			}
			MissionShip missionShip = shipAssignment.MissionShip;
			if (shipFilter == null || shipFilter(missionShip))
			{
				MatrixFrame globalFrame = missionShip.GlobalFrame;
				float num2 = ((Vec3)(ref globalFrame.origin)).DistanceSquared(position);
				if (num2 <= num)
				{
					num = num2;
					result = missionShip;
				}
			}
		}
		return result;
	}

	public void FillClosestShips(in MatrixFrame shipEntityGlobalFrame, float distanceLimit, MBList<(MissionShip ship, OarSidePhaseController.OarSide shipSide)> closestShips, MissionShip ignoreShip = null)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		foreach (MissionShip item2 in (List<MissionShip>)(object)_allShips)
		{
			if (item2 != null && item2 != ignoreShip)
			{
				BoundingBox physicsBoundingBoxWithChildren = item2.Physics.PhysicsBoundingBoxWithChildren;
				Vec2 val = Vec2.Abs(((Vec3)(ref physicsBoundingBoxWithChildren.max)).AsVec2);
				physicsBoundingBoxWithChildren = item2.Physics.PhysicsBoundingBoxWithChildren;
				Vec2 val2 = Vec2.Max(val, Vec2.Abs(((Vec3)(ref physicsBoundingBoxWithChildren.min)).AsVec2));
				float num = distanceLimit + ((Vec2)(ref val2)).Length;
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
				Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().origin;
				Vec3 origin2 = shipEntityGlobalFrame.origin;
				if (((Vec3)(ref origin2)).DistanceSquared(origin) <= num * num)
				{
					Vec3 val3 = origin - shipEntityGlobalFrame.origin;
					OarSidePhaseController.OarSide item = ((!(Vec3.CrossProduct(shipEntityGlobalFrame.rotation.f, val3).z >= 0f)) ? OarSidePhaseController.OarSide.Right : OarSidePhaseController.OarSide.Left);
					((List<(MissionShip, OarSidePhaseController.OarSide)>)(object)closestShips).Add((item2, item));
				}
			}
		}
	}

	public MatrixFrame GetMeanFrameOfTeamShips(TeamSideEnum teamSide)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.Zero;
		Vec3 val2 = Vec3.Forward;
		int num = 0;
		ShipAssignment[,] shipAssignments = _shipAssignments;
		foreach (ShipAssignment shipAssignment in shipAssignments)
		{
			if (shipAssignment.HasMissionShip && shipAssignment.TeamSide == teamSide)
			{
				MatrixFrame globalFrame = shipAssignment.MissionShip.GlobalFrame;
				val += globalFrame.origin;
				float num2 = 1f / ((float)num + 1f);
				val2 = Vec3.Slerp(val2, globalFrame.rotation.f, num2);
				num++;
			}
		}
		Mat3 identity = Mat3.Identity;
		identity.f = val2;
		((Mat3)(ref identity)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		return new MatrixFrame(ref identity, ref val);
	}

	public bool GetCollisionFreeShipFrame(in Vec2 globalPosition, in Vec2 forward, in Vec2 localDimensions, out MatrixFrame collisionFreeFrame, bool checkBoundaries = true, NavalBoundaryCheckType boundaryToCheck = NavalBoundaryCheckType.HardBoundary, Team team = null, float searchCellWidth = 10f, int searchCellDistance = 10, float clearanceMargin = 1f, int ignoreShipIndex = -1)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		searchCellWidth = MathF.Max(1f, searchCellWidth);
		searchCellDistance = MathF.Max(0, searchCellDistance);
		clearanceMargin = MathF.Max(0f, clearanceMargin);
		_tmpCollisionFreeFrameSearchQueue.Clear();
		_tmpCollisionFreeFrameSearchQueue.Enqueue(0, (0, Vec2i.Zero));
		bool flag = false;
		Oriented2DArea area = new Oriented2DArea(ref Vec2.Zero, ref forward, ref localDimensions);
		Vec2 val2;
		do
		{
			(int, Vec2i) value = _tmpCollisionFreeFrameSearchQueue.Dequeue().Value;
			Vec2i item = value.Item2;
			Vec2 val = globalPosition;
			val2 = ((Oriented2DArea)(ref area)).GlobalForward;
			Vec2 position = val + ((Vec2)(ref val2)).RightVec() * searchCellWidth * (float)item.X + ((Oriented2DArea)(ref area)).GlobalForward * searchCellWidth * (float)item.Y;
			((Oriented2DArea)(ref area)).SetGlobalCenter(ref position);
			bool flag2 = !checkBoundaries || IsPositionInsideBoundaries(in position, boundaryToCheck, team);
			flag = flag2 && IsAreaFreeOfShipCollision(in area, clearanceMargin, ignoreShipIndex);
			if (flag || value.Item1 >= searchCellDistance)
			{
				continue;
			}
			int num = value.Item1 + 1;
			int num2 = num + ((!flag2) ? searchCellDistance : 0);
			if (item.X <= 0)
			{
				_tmpCollisionFreeFrameSearchQueue.Enqueue(num2, (num, new Vec2i(item.X - 1, item.Y)));
			}
			if (item.X >= 0)
			{
				_tmpCollisionFreeFrameSearchQueue.Enqueue(num2, (num, new Vec2i(item.X + 1, item.Y)));
			}
			if (item.X == 0)
			{
				if (item.Y >= 0)
				{
					_tmpCollisionFreeFrameSearchQueue.Enqueue(num2, (num, new Vec2i(item.X, item.Y + 1)));
				}
				if (item.Y <= 0)
				{
					_tmpCollisionFreeFrameSearchQueue.Enqueue(num2, (num, new Vec2i(item.X, item.Y - 1)));
				}
			}
		}
		while (!Extensions.IsEmpty<KeyValuePair<int, (int, Vec2i)>>((IEnumerable<KeyValuePair<int, (int, Vec2i)>>)_tmpCollisionFreeFrameSearchQueue) && !flag);
		collisionFreeFrame = MatrixFrame.Identity;
		if (flag)
		{
			val2 = ((Oriented2DArea)(ref area)).GlobalCenter;
			Vec3 origin = ((Vec2)(ref val2)).ToVec3(0f);
			origin.z = ((MissionBehavior)this).Mission.Scene.GetWaterLevelAtPosition(((Oriented2DArea)(ref area)).GlobalCenter, true, false);
			collisionFreeFrame.origin = origin;
			ref Mat3 rotation = ref collisionFreeFrame.rotation;
			val2 = forward;
			rotation.f = ((Vec2)(ref val2)).ToVec3(0f);
			((Mat3)(ref collisionFreeFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		}
		return flag;
	}

	public float ComputeSpawnPathDeploymentOffset(Path path)
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 11; j++)
			{
				if (_shipAssignments[i, j].IsSet)
				{
					num++;
				}
			}
		}
		float num2 = 400f;
		if (num > 2)
		{
			int num3 = 14;
			float num4 = (float)Math.Min(num - 2, num3) / (float)num3;
			num2 += 460f * MathF.Pow(num4, 0.6f);
		}
		num2 = MathF.Min(path.GetTotalLength(), num2);
		return (0f - num2) / 2f + 1f;
	}

	public bool FindAssignmentOfShipOrigin(IShipOrigin shipOrigin, out ShipAssignment shipAssignment)
	{
		shipAssignment = null;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 11; j++)
			{
				ShipAssignment shipAssignment2 = _shipAssignments[i, j];
				if (shipAssignment2.HasMissionShip && shipAssignment2.MissionShip.ShipOrigin == shipOrigin)
				{
					shipAssignment = shipAssignment2;
					return true;
				}
			}
		}
		return false;
	}

	public void SetTeleportShips(bool value)
	{
		IsTeleportingShips = value;
	}

	public void TeleportShip(MissionShip ship, MatrixFrame targetFrame, bool checkFreeArea, bool anchorShip = false, bool snapToWater = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		TeleportShipAux(ship, ref targetFrame, checkFreeArea, anchorShip, snapToWater);
		this.ShipTeleportedEvent?.Invoke(ship, globalFrame, targetFrame);
	}

	public bool IsAreaFreeOfShipCollision(in Oriented2DArea area, float clearanceMargin = 1f, int ignoreShipIndex = -1)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		foreach (MissionShip item in (List<MissionShip>)(object)_allShips)
		{
			if (item.Index != ignoreShipIndex && item.Physics.NavalSinkingState != NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sunk)
			{
				Oriented2DArea globalMaximal2DArea = item.Physics.GetGlobalMaximal2DArea();
				Oriented2DArea val = area;
				if (((Oriented2DArea)(ref val)).Overlaps(ref globalMaximal2DArea, clearanceMargin))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsAShipAssignedToFormation(Formation formation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return GetShipAssignment(formation.Team.TeamSide, formation.FormationIndex).HasMissionShip;
	}

	public bool IsShipAssignedToFormation(MissionShip ship, Formation formation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		ShipAssignment shipAssignment = GetShipAssignment(formation.Team.TeamSide, formation.FormationIndex);
		if (shipAssignment.HasMissionShip)
		{
			return shipAssignment.MissionShip == ship;
		}
		return false;
	}

	public void ClearShipAssignments()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 11; j++)
			{
				TeamSideEnum teamSide = (TeamSideEnum)i;
				ClearShipAssignment(teamSide, j);
			}
		}
	}

	public ShipAssignment SetShipAssignment(TeamSideEnum teamSide, FormationClass formationIndex, IShipOrigin shipOrigin)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected I4, but got Unknown
		//IL_000d: Expected I4, but got Unknown
		ShipAssignment shipAssignment = _shipAssignments[(int)teamSide, (int)formationIndex];
		shipAssignment.Set(shipOrigin);
		return shipAssignment;
	}

	public MissionShip SpawnShip(IShipOrigin shipOrigin, in MatrixFrame shipFrame, Team team, Formation formation = null, bool spawnAnchored = false, FormationClass formationSearchRange = (FormationClass)8, bool checkForFreeArea = true)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (formation == null)
		{
			formation = FindFirstFormationWithoutShip(team, formationSearchRange);
		}
		TeamSideEnum teamSide = team.TeamSide;
		FormationClass formationIndex = formation.FormationIndex;
		GetShipAssignment(teamSide, formationIndex).Set(shipOrigin);
		return SpawnShip(formation, in shipFrame, spawnAnchored, checkForFreeArea);
	}

	public MissionShip SpawnShip(Formation formation, in MatrixFrame spawnFrame, bool spawnAnchored = true, bool checkForFreeArea = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected I4, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected I4, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = formation.Team.TeamSide;
		FormationClass formationIndex = formation.FormationIndex;
		int num = (int)teamSide;
		ShipAssignment shipAssignment = _shipAssignments[num, (int)formationIndex];
		MatrixFrame initialFrame = spawnFrame;
		IMissionDeploymentPlan deploymentPlan = ((MissionBehavior)this).Mission.DeploymentPlan;
		bool flag = deploymentPlan.IsPlanMade(formation.Team);
		if (((MatrixFrame)(ref initialFrame)).IsZero && flag)
		{
			initialFrame = deploymentPlan.GetDeploymentFrame(formation.Team);
		}
		if (((MatrixFrame)(ref initialFrame)).IsZero)
		{
			initialFrame = GetMeanFrameOfTeamShips(teamSide);
		}
		if (checkForFreeArea)
		{
			Vec2 globalPosition = ((Vec3)(ref initialFrame.origin)).AsVec2;
			Vec2 forward = ((Vec3)(ref initialFrame.rotation.f)).AsVec2;
			forward = ((Vec2)(ref forward)).Normalized();
			if (GetCollisionFreeShipFrame(in globalPosition, in forward, shipAssignment.MissionShipObject.DeploymentArea, out var collisionFreeFrame, checkBoundaries: true, flag ? NavalBoundaryCheckType.DeploymentBoundary : NavalBoundaryCheckType.HardBoundary, formation.Team, 1f, 400))
			{
				initialFrame = collisionFreeFrame;
			}
		}
		float waterLevelAtPosition = ((MissionBehavior)this).Mission.Scene.GetWaterLevelAtPosition(((Vec3)(ref initialFrame.origin)).AsVec2, true, true);
		initialFrame.origin.z = waterLevelAtPosition;
		MissionShip missionShip = null;
		MissionShip missionShip2 = LinQuick.FirstOrDefaultQ<MissionShip>((List<MissionShip>)(object)_removedShipsPool, (Func<MissionShip, bool>)((MissionShip ship) => ship.ShipOrigin == shipAssignment.ShipOrigin));
		if (missionShip2 != null)
		{
			((List<MissionShip>)(object)_removedShipsPool).Remove(missionShip2);
			missionShip2.SetFormation(formation);
			missionShip2.SetRemoved(value: false);
			((MissionObject)missionShip2).SetEnabledAndMakeVisible(true, true);
			shipAssignment.SetMissionShip(missionShip2);
			missionShip = missionShip2;
			missionShip.ResetFormationPositioning();
		}
		else
		{
			MissionShipFactory.CreateMissionShip(_shipIndexGenerator, shipAssignment, this, in initialFrame);
			_shipIndexGenerator++;
			missionShip = shipAssignment.MissionShip;
		}
		missionShip.SetOriginalTeamSide(formation.Team.TeamSide);
		((List<MissionShip>)(object)_allShips).Add(missionShip);
		if (missionShip2 != null)
		{
			TeleportShipAux(missionShip, ref initialFrame, checkFreeArea: false);
		}
		if (spawnAnchored)
		{
			missionShip.SetAnchor(isAnchored: true, anchorInPlace: true);
		}
		this.ShipSpawnedEvent?.Invoke(missionShip);
		return missionShip;
	}

	public void RemoveShip(MissionShip ship)
	{
		FindAssignmentOfShipOrigin(ship.ShipOrigin, out var shipAssignment);
		if (shipAssignment != null)
		{
			RemoveShipAux(shipAssignment);
		}
	}

	public void RemoveShip(Formation formation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = formation.Team.TeamSide;
		ShipAssignment shipAssignment = GetShipAssignment(teamSide, formation.FormationIndex);
		RemoveShipAux(shipAssignment);
	}

	public void TransferShipToFormation(MissionShip ship, Formation toFormation)
	{
		FindAssignmentOfShipOrigin(ship.ShipOrigin, out var shipAssignment);
		TransferShipToFormation(ship.ShipOrigin, shipAssignment.Formation, toFormation);
	}

	public void TransferShipToTeam(MissionShip ship, Team targetTeam, Formation targetFormation = null, FormationClass searchRange = (FormationClass)8)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		ShipAssignment shipAssignment;
		bool flag = FindAssignmentOfShipOrigin(ship.ShipOrigin, out shipAssignment);
		if (targetFormation == null)
		{
			targetFormation = FindFirstFormationWithoutShip(targetTeam, searchRange);
		}
		this.BeforeShipTransferredToTeamEvent?.Invoke(ship, targetTeam, targetFormation);
		Team team = ship.Team;
		Formation formation = ship.Formation;
		ShipAssignment shipAssignment2 = GetShipAssignment(targetFormation.Team.TeamSide, targetFormation.FormationIndex);
		if (flag)
		{
			shipAssignment.RemoveShip();
			shipAssignment.Clear();
		}
		shipAssignment2.Set(ship.ShipOrigin);
		shipAssignment2.SetMissionShip(ship);
		ship.SetFormation(targetFormation);
		this.ShipTransferredToTeamEvent?.Invoke(ship, team, formation);
	}

	private void RefreshTeamAIBehaviorShipReferences(Team team)
	{
		foreach (Formation item in (List<Formation>)(object)team.FormationsIncludingSpecialAndEmpty)
		{
			if (item.AI == null)
			{
				continue;
			}
			for (int i = 0; i < item.AI.BehaviorCount; i++)
			{
				if (item.AI.GetBehaviorAtIndex(i) is NavalBehaviorComponent navalBehaviorComponent)
				{
					navalBehaviorComponent.RefreshShipReferences();
				}
			}
		}
	}

	public void OnShipCaptured(MissionShip ship, Formation targetFormation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Formation formation = ship.Formation;
		ShipAssignment shipAssignment;
		bool num = FindAssignmentOfShipOrigin(ship.ShipOrigin, out shipAssignment);
		ShipAssignment shipAssignment2 = GetShipAssignment(targetFormation.Team.TeamSide, targetFormation.FormationIndex);
		MissionShip missionShip = shipAssignment2.MissionShip;
		shipAssignment2.SetMissionShip(ship);
		ship.SetFormation(null);
		missionShip.SetFormation(null);
		if (num)
		{
			shipAssignment.RemoveShip();
			shipAssignment.Clear();
		}
		ship.SetFormation(targetFormation);
		ship.SetController(missionShip.Controller?.ControllerType ?? ShipControllerType.None);
		missionShip.SetController(ShipControllerType.None);
		foreach (MissionShip item in (List<MissionShip>)(object)AllShips)
		{
			item.ShipOrder.OnShipCaptured(ship, missionShip);
		}
		((TeamAINavalComponent)(object)((formation != null) ? formation.Team.TeamAI : null))?.TeamNavalQuerySystem.ForceExpireAll();
		((TeamAINavalComponent)(object)targetFormation.Team.TeamAI).TeamNavalQuerySystem.ForceExpireAll();
		this.ShipCapturedEvent?.Invoke(ship, missionShip, formation, targetFormation);
		if (formation != null)
		{
			RefreshTeamAIBehaviorShipReferences(formation.Team);
		}
		RefreshTeamAIBehaviorShipReferences(targetFormation.Team);
	}

	public Formation FindFirstFormationWithoutShip(Team team, FormationClass searchRange = (FormationClass)8)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between I4 and Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)team.TeamSide;
		FormationClass val = (FormationClass)11;
		for (int i = 0; i < (int)searchRange; i++)
		{
			ShipAssignment shipAssignment = _shipAssignments[num, i];
			if (!shipAssignment.IsSet && !shipAssignment.HasMissionShip)
			{
				val = shipAssignment.FormationIndex;
				break;
			}
		}
		if ((int)val != 11)
		{
			return team.GetFormation(val);
		}
		return null;
	}

	public void SetTeamShipDeploymentLimit(TeamSideEnum teamSide, NavalShipDeploymentLimit deploymentLimit)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_teamDeploymentLimits[teamSide] = deploymentLimit;
	}

	public void TransferShipToFormation(IShipOrigin shipOrigin, Formation fromFormation, Formation toFormation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected I4, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected I4, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected I4, but got Unknown
		int num = (int)fromFormation.Team.TeamSide;
		int num2 = (int)fromFormation.FormationIndex;
		int num3 = (int)toFormation.FormationIndex;
		ShipAssignment shipAssignment = _shipAssignments[num, num2];
		ShipAssignment shipAssignment2 = _shipAssignments[num, num3];
		MissionShip missionShip = shipAssignment.MissionShip;
		this.BeforeShipTransferredToFormationEvent?.Invoke(missionShip, toFormation);
		if (!shipAssignment2.IsSet)
		{
			shipAssignment2.Set(missionShip.ShipOrigin);
		}
		shipAssignment2.SetMissionShip(missionShip);
		shipAssignment.RemoveShip();
		shipAssignment.Clear();
		missionShip.SetFormation(toFormation);
		missionShip.ResetFormationPositioning();
		this.ShipTransferredToFormationEvent?.Invoke(missionShip, fromFormation);
	}

	public int GetShipDeploymentLimit(TeamSideEnum teamSide, out NavalShipDeploymentLimit deploymentLimit)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		deploymentLimit = _teamDeploymentLimits[teamSide];
		return deploymentLimit.NetDeploymentLimit;
	}

	public void SetCanHaveConnectionCooldown(bool value)
	{
		CanHaveConnectionCooldown = value;
	}

	private void ClearShipAssignment(TeamSideEnum teamSide, int formationIndex)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		int num = (int)teamSide;
		_ = _shipAssignments[num, formationIndex];
		_shipAssignments[num, formationIndex].Clear();
	}

	bool IVehicleHandler.IsAgentInVehicle(Agent agent, out WeakGameEntity vehicleEntity)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		foreach (MissionShip item in (List<MissionShip>)(object)AllShips)
		{
			if (item.GetIsAgentOnShip(agent))
			{
				vehicleEntity = ((ScriptComponentBehavior)item).GameEntity;
				return true;
			}
		}
		vehicleEntity = WeakGameEntity.Invalid;
		return false;
	}

	private void RemoveShipAux(ShipAssignment shipAssignment)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		MissionShip shipToRemove = shipAssignment.MissionShip;
		this.BeforeShipRemovedEvent?.Invoke(shipToRemove);
		shipAssignment.RemoveShip();
		shipAssignment.Clear();
		((List<MissionShip>)(object)_allShips).RemoveAll((Predicate<MissionShip>)((MissionShip s) => s == shipToRemove));
		this.ShipRemovedEvent?.Invoke(shipToRemove);
		shipToRemove.SetFormation(null);
		shipToRemove.SetRemoved(value: true);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)shipToRemove).GameEntity;
		if (IsDeploymentMode && !IsMissionEnding)
		{
			MatrixFrame globalFrame = shipToRemove.GlobalFrame;
			((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			((WeakGameEntity)(ref gameEntity)).SetGlobalFrame(ref globalFrame, true);
			GameEntityPhysicsExtensions.SetAngularVelocity(gameEntity, Vec3.Zero);
			GameEntityPhysicsExtensions.SetLinearVelocity(gameEntity, Vec3.Zero);
			((MissionObject)shipToRemove).SetDisabledAndMakeInvisible(true, true);
			((List<MissionShip>)(object)_removedShipsPool).Add(shipToRemove);
		}
		else
		{
			((MissionBehavior)this).Mission.Scene.RemoveEntity(gameEntity, 121);
		}
		foreach (MissionShip item in (List<MissionShip>)(object)AllShips)
		{
			if (item != shipToRemove && item.HasController && item.Controller.IsAIControlled && item.AIController.TargetShip == shipToRemove)
			{
				item.AIController.ClearTarget();
			}
		}
	}

	private void TeleportShipAux(MissionShip ship, ref MatrixFrame targetFrame, bool checkFreeArea, bool anchorShip = false, bool snapToWater = true)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (checkFreeArea)
		{
			Vec2 globalPosition = ((Vec3)(ref targetFrame.origin)).AsVec2;
			Vec2 forward = ((Vec3)(ref targetFrame.rotation.f)).AsVec2;
			forward = ((Vec2)(ref forward)).Normalized();
			if (GetCollisionFreeShipFrame(in globalPosition, in forward, ship.MissionShipObject.DeploymentArea, out var collisionFreeFrame, checkBoundaries: true, ((int)((MissionBehavior)this).Mission.Mode == 6) ? NavalBoundaryCheckType.DeploymentBoundary : NavalBoundaryCheckType.HardBoundary, ship.Team, 1f, 400, 1f, ship.Index))
			{
				targetFrame = collisionFreeFrame;
			}
		}
		if (snapToWater)
		{
			targetFrame.origin.z = ((MissionBehavior)this).Mission.Scene.GetWaterLevelAtPosition(new Vec2(targetFrame.origin.x, targetFrame.origin.y), true, false);
			targetFrame.origin.z -= ship.Physics.StabilitySubmergedHeightOfShip;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetGlobalFrame(ref targetFrame, true);
		gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
		((WeakGameEntity)(ref gameEntity)).UpdateAttachedNavigationMeshFaces();
		ship.ResetFormationPositioning();
		if (anchorShip)
		{
			ship.SetAnchor(isAnchored: true, anchorInPlace: true);
		}
		if (ship.ShipOrder != null)
		{
			ship.ShipOrder.RefreshOrders();
		}
	}

	internal void HandleSailsHit(MissionShip ship, MissionSail sail, Agent attackerAgent, Missile missile, MissionWeapon missileWeapon)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = ((MissionWeapon)(ref missileWeapon)).CurrentUsageItem.FireDamage;
		float missileVelocityLengthOnFirstSailHit = GetMissileVelocityLengthOnFirstSailHit(((MBMissile)missile).Index);
		if (missileVelocityLengthOnFirstSailHit < 0f)
		{
			AddMissileHittingSail(missile);
		}
		else
		{
			Vec3 velocity = ((MBMissile)missile).GetVelocity();
			float length = ((Vec3)(ref velocity)).Length;
			num *= length / missileVelocityLengthOnFirstSailHit;
		}
		float inflictedDamage = MissionGameModels.Current.AgentApplyDamageModel.CalculateSailFireDamage(attackerAgent, ship.ShipOrigin, num, IsMissileFromShipSiegeEngine(((MBMissile)missile).Index));
		ship.DealDamageToSails(attackerAgent, num, inflictedDamage, sail);
	}

	public static bool IsPositionInsideBoundaries(in Vec2 position, NavalBoundaryCheckType boundaryType = NavalBoundaryCheckType.HardBoundary, Team team = null)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return boundaryType switch
		{
			NavalBoundaryCheckType.HardBoundary => Mission.Current.IsPositionInsideHardBoundaries(position), 
			NavalBoundaryCheckType.DeploymentBoundary => Mission.Current.DeploymentPlan.IsPositionInsideDeploymentBoundaries(team, ref position), 
			_ => false, 
		};
	}

	private static bool FindAndRemoveClosestDeckFrameToPosition(MBList<MatrixFrame> deckFrames, in Vec3 position, out MatrixFrame foundFrame)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < ((List<MatrixFrame>)(object)deckFrames).Count; i++)
		{
			Vec3 val = ((List<MatrixFrame>)(object)deckFrames)[i].origin - position;
			float lengthSquared = ((Vec3)(ref val)).LengthSquared;
			if (lengthSquared <= num2)
			{
				num = i;
				num2 = lengthSquared;
			}
		}
		if (num >= 0)
		{
			foundFrame = ((List<MatrixFrame>)(object)deckFrames)[num];
			Vec3 val2 = 1f * Vec3.Up;
			ref Vec3 origin = ref foundFrame.origin;
			origin += val2;
			((List<MatrixFrame>)(object)deckFrames).RemoveAt(num);
			return true;
		}
		foundFrame = MatrixFrame.Identity;
		return false;
	}
}
