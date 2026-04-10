using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.DWA;
using NavalDLC.Missions.AI.UsableMachineAIs;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipControl;
using NavalDLC.Missions.ShipInput;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects.Usables;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Missions.Objects;

public class MissionShip : MissionObject
{
	public enum ShipInstanceType
	{
		None,
		MissionShip,
		EditorShip
	}

	public enum SailState : byte
	{
		Intact,
		Burning,
		Destroyed
	}

	public struct ShipCollisionData
	{
		public MissionShip CollidingShip;

		public Vec3 ContactPosAvg;

		public float Damage;

		public ShipCollisionData(MissionShip collidingShip, Vec3 contactPosAvg, float damage)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			CollidingShip = collidingShip;
			ContactPosAvg = contactPosAvg;
			Damage = damage;
		}
	}

	private const int DetanglingDuration = 6;

	private const float DamageCooldownForShipInSeconds = 2f;

	private const float CollisionDirectionSpeedThresholdToDamage = 3f;

	private const float MaxSoundPositionUpdateDistanceSquared = 10000f;

	public const string OuterDeckTroopSpTag = "sp_troop_outer_deck";

	public const string InnerDeckTroopSpTag = "sp_troop_inner_deck";

	public const string CaptainTroopSpTag = "sp_troop_captain";

	public const string CrewTroopSpTag = "sp_troop_crew_spawn";

	public const string RallyPointTag = "rally_point";

	public const string BannerTag = "banner_with_faction_color";

	public const string SailMeshTag = "sail_mesh_entity";

	public const float NavmeshDisableLimit = 0.35f;

	private static TextObject PlayerSideShipSinkingText = new TextObject("{=jX6yqP3T}A friendly ship has started to sink!", (Dictionary<string, object>)null);

	private static TextObject EnemySideShipSinkingText = new TextObject("{=nvTWWBib}An enemy ship has started to sink!", (Dictionary<string, object>)null);

	private readonly MBList<MissionShip> _temporaryMissionShipContainer = new MBList<MissionShip>();

	private readonly MBQueue<MissionShip> _temporaryMissionShipQueue = new MBQueue<MissionShip>();

	private static readonly int _scrapeSoundEventID = SoundEvent.GetEventIdFromString("event:/physics/vessel/ship_scraping");

	private readonly QueryData<bool> _anyActiveFormationTroopOnShip;

	private SailInput _customSailSetting;

	private MissionShipObject _missionShipObject;

	private MBList<(int, SoundEvent)> _scrapeSoundEvents;

	public bool ShouldUpdateSoundPos;

	private NavalAgentMoraleInteractionLogic _moraleInteractionLogic;

	private MBList<MatrixFrame> _outerDeckLocalFrames;

	private MBList<MatrixFrame> _innerDeckLocalFrames;

	private MBList<MatrixFrame> _crewSpawnLocalFrames;

	private int _nextDeckSpawnFrameIndex;

	private bool _autoUpdateController = true;

	private int _nextCrewSpawnFrameIndex;

	private MBList<ShipAttachmentMachine> _attachmentMachines;

	private MBList<IShipEventListener> _shipEventListeners;

	private bool _isCapsized;

	private MBList<ShipAttachmentPointMachine> _attachmentPointMachines;

	private MBList<ShipShieldComponent> _shields;

	private Timer _capsizeDamageTimer;

	private MBList<GameEntity> _bannerEntities;

	private MBList<GameEntity> _sailMeshEntities;

	private WorldPosition _cachedWorldPositionOnDeck;

	private bool _isCachedWorldPositionOnDeckDirty = true;

	private bool _isRemoved;

	private bool _foldSailsOnBridgeConnection = true;

	private HashSet<MissionShip> _visitedMissionShips;

	private float _nextPermanentBurnDamageTime;

	private float _nextFireHitPointRestoreTime;

	private Vec2[] _localPhysicsBoundingBoxXYPlaneVertices;

	private Vec2[] _scaledLocalPhysicsBoundingBoxXYPlaneVertices;

	private Vec2[] _physicsBoundingBoxXYPlaneVertices;

	private Vec2[] _criticalZoneVertices;

	private MissionShip _detanglingMissionShip;

	private Vec3 _detanglingMissionShipAverageContactPosition;

	private MissionTimer _detanglingMissionShipTimer;

	private ShipInputProcessor _inputProcessor;

	private NavalDLC.Missions.ShipActuators.ShipActuators _actuators;

	private ShipInputRecord _inputRecord;

	private NavalDLC.Missions.NavalPhysics.NavalPhysics _physics;

	private float[] _partialHitPoints;

	private MBList<ShipAttachmentMachine> _shipAttachmentMachines;

	private MBList<ShipOarMachine> _leftSideShipOarMachines;

	private MBList<ShipOarMachine> _rightSideShipOarMachines;

	private MBList<ShipOarMachine> _shipOarMachines;

	private MBList<ShipUnmannedOar> _shipUnmannedOars;

	private MBList<ClimbingMachine> _climbingMachines;

	private MBList<DestructableComponent> _allDestructibleComponents;

	private ShipDWAAgentDelegate _dwaAgentDelegate;

	private MissionShipRam _ram;

	private MBList<AmmoBarrelBase> _ammoBarrels;

	private float _connectionBlockedShipTime;

	private float _disconnectionBlockedShipTime;

	private MBList<SailVisual> _sailVisuals;

	private BoundingBox _localBoundingBoxCached;

	private bool _localBoundingBoxCacheInvalid = true;

	private List<(GameEntity, PhysicsEventType)> _currentCollisionStatesToShips = new List<(GameEntity, PhysicsEventType)>();

	private PhysicsEventType _currentCollisionState = (PhysicsEventType)2;

	private readonly Dictionary<MissionShip, float> _shipDamageCooldowns = new Dictionary<MissionShip, float>();

	private readonly ConcurrentQueue<ShipCollisionData> _shipCollisionData = new ConcurrentQueue<ShipCollisionData>();

	private static uint _missionShipScriptNameHash = Managed.GetStringHashCode("MissionShip");

	public static int MaxShipIndex { get; private set; }

	public bool AnyActiveFormationTroopOnShip => _anyActiveFormationTroopOnShip.Value;

	public int Index { get; private set; }

	public bool IsRemoved => _isRemoved;

	public MatrixFrame GlobalFrame
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			return ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		}
	}

	public MBReadOnlyList<MatrixFrame> OuterDeckLocalFrames => (MBReadOnlyList<MatrixFrame>)(object)_outerDeckLocalFrames;

	public MBReadOnlyList<MatrixFrame> InnerDeckLocalFrames => (MBReadOnlyList<MatrixFrame>)(object)_innerDeckLocalFrames;

	public MBReadOnlyList<MatrixFrame> CrewSpawnLocalFrames => (MBReadOnlyList<MatrixFrame>)(object)_crewSpawnLocalFrames;

	public int DeckFrameCount => ((List<MatrixFrame>)(object)_innerDeckLocalFrames).Count + ((List<MatrixFrame>)(object)_outerDeckLocalFrames).Count;

	public MBReadOnlyList<GameEntity> BannerEntities => (MBReadOnlyList<GameEntity>)(object)_bannerEntities;

	public MBReadOnlyList<GameEntity> SailMeshEntities => (MBReadOnlyList<GameEntity>)(object)_sailMeshEntities;

	public Banner Banner
	{
		get
		{
			Banner obj = ShipHelper.GetShipBanner(ShipOrigin, (IAgent)(object)Captain);
			if (obj == null)
			{
				Team team = Team;
				if (team == null)
				{
					return null;
				}
				obj = team.Banner;
			}
			return obj;
		}
	}

	public (uint sailColor1, uint sailColor2) SailColors => ShipHelper.GetSailColors(ShipOrigin, (IAgent)(object)Captain);

	public NavalDLC.Missions.NavalPhysics.NavalPhysics Physics => _physics;

	public float MaxHealth => ShipOrigin.MaxHitPoints;

	public float MaxFireHealth => ShipOrigin.MaxFireHitPoints;

	public float MaxPartialHealth => MaxHealth * _missionShipObject.PartialHitPointsRatio;

	public int TotalCrewCapacity => ShipOrigin.TotalCrewCapacity;

	public int CrewSizeOnMainDeck { get; private set; }

	public int CrewSizeOnLowerDeck => ShipOrigin.TotalCrewCapacity - CrewSizeOnMainDeck;

	public bool HasController => Controller != null;

	public AIShipController AIController => (AIShipController)Controller;

	public bool IsAIControlled
	{
		get
		{
			if (HasController)
			{
				return Controller.IsAIControlled;
			}
			return false;
		}
	}

	public bool IsPlayerControlled
	{
		get
		{
			if (HasController)
			{
				return Controller.IsPlayerControlled;
			}
			return false;
		}
	}

	public bool IsFormationAndShipAIControlled
	{
		get
		{
			if (Formation != null && Formation.IsAIControlled)
			{
				return IsAIControlled;
			}
			return false;
		}
	}

	public PlayerShipController PlayerController => (PlayerShipController)Controller;

	public FormationClass FormationIndex => (FormationClass)(((_003F?)Formation?.FormationIndex) ?? 0);

	public BattleSideEnum BattleSide
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Team team = Team;
			if (team == null)
			{
				return (BattleSideEnum)(-1);
			}
			return team.Side;
		}
	}

	public MissionShipObject MissionShipObject => _missionShipObject;

	public NavalShipsLogic ShipsLogic { get; private set; }

	public Team Team => Formation?.Team;

	public Formation Formation { get; private set; }

	public Agent Captain
	{
		get
		{
			Formation formation = Formation;
			if (formation == null)
			{
				return null;
			}
			return formation.Captain;
		}
	}

	public bool IsInitialized => _missionShipObject != null;

	public bool IsRetreating => false;

	public SailState ShipSailState { get; private set; }

	public bool HasCustomSailSetting { get; private set; }

	public bool IsSinking
	{
		get
		{
			if (_physics.NavalSinkingState != NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sinking)
			{
				return _physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sunk;
			}
			return true;
		}
	}

	public bool IsSunk => _physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sunk;

	public ShipOrder ShipOrder { get; private set; }

	public IShipOrigin ShipOrigin { get; private set; }

	public bool IsPlayerShip
	{
		get
		{
			Agent main = Agent.Main;
			return ((main == null) ? null : main.GetComponent<AgentNavalComponent>()?.FormationShip) == this;
		}
	}

	public MatrixFrame RallyFrame { get; private set; }

	public float HitPoints => ShipOrigin.HitPoints;

	public float FireHitPoints { get; private set; }

	public float BurntHullDamageTotal { get; private set; }

	public float VisualRudderRotationPercentage => _actuators.VisualRudderLocalRotation / MissionShipObject.RudderRotationMax;

	public float VisualRudderRotation => _actuators.VisualRudderLocalRotation;

	public float VisualRudderPullDirection => _actuators.VisualRudderPullDirection;

	public float SailTargetSetting => ((IEnumerable<MissionSail>)_actuators.Sails).FirstOrDefault()?.TargetSailSetting ?? 0f;

	public MBReadOnlyList<MissionSail> Sails => _actuators.Sails;

	public ulong ShipUniqueBitwiseID { get; private set; }

	public ulong ShipIslandCombinedID { get; private set; }

	public bool IsShipOrderActive { get; private set; } = true;

	public bool IsClimbingMachineStandAloneTickingActive { get; private set; }

	public MBReadOnlyList<ShipAttachmentMachine> AttachmentMachines => (MBReadOnlyList<ShipAttachmentMachine>)(object)_attachmentMachines;

	public MBReadOnlyList<ShipAttachmentPointMachine> AttachmentPointMachines => (MBReadOnlyList<ShipAttachmentPointMachine>)(object)_attachmentPointMachines;

	public MBReadOnlyList<ShipShieldComponent> Shields => (MBReadOnlyList<ShipShieldComponent>)(object)_shields;

	public ClimbingMachineDetachment ClimbingMachineDetachment { get; private set; }

	public MBReadOnlyList<ShipAttachmentMachine> ShipAttachmentMachines => (MBReadOnlyList<ShipAttachmentMachine>)(object)_shipAttachmentMachines;

	public MBReadOnlyList<ShipOarMachine> LeftSideShipOarMachines => (MBReadOnlyList<ShipOarMachine>)(object)_leftSideShipOarMachines;

	public MBReadOnlyList<ShipOarMachine> RightSideShipOarMachines => (MBReadOnlyList<ShipOarMachine>)(object)_rightSideShipOarMachines;

	public MBReadOnlyList<ShipOarMachine> ShipOarMachines => (MBReadOnlyList<ShipOarMachine>)(object)_shipOarMachines;

	public MBReadOnlyList<ClimbingMachine> ClimbingMachines => (MBReadOnlyList<ClimbingMachine>)(object)_climbingMachines;

	public MBReadOnlyList<ShipUnmannedOar> ShipUnmannedOars => (MBReadOnlyList<ShipUnmannedOar>)(object)_shipUnmannedOars;

	public MBReadOnlyList<DestructableComponent> AllDestructableComponents => (MBReadOnlyList<DestructableComponent>)(object)_allDestructibleComponents;

	public ShipControllerMachine ShipControllerMachine { get; private set; }

	public float MaxSailHitPoints => ShipOrigin.MaxSailHitPoints;

	public float SailHitPoints => ShipOrigin.SailHitPoints;

	public bool IsDeployed { get; private set; }

	public bool CanBeTakenOver { get; private set; } = true;

	public TeamSideEnum OriginalTeamSide { get; private set; } = (TeamSideEnum)(-1);

	public Agent SailBurnerAgent { get; private set; }

	public SoundEvent SailBurningSoundEvent { get; private set; }

	public ShipController Controller { get; private set; }

	public RangedSiegeWeapon ShipSiegeWeapon { get; private set; }

	public bool IsShipNavmeshDisabled { get; private set; }

	public bool HasDWAAgent => _dwaAgentDelegate != null;

	public int DWAAgentId => _dwaAgentDelegate.Id;

	public ref readonly DWAAgentState DWAAgentState => ref _dwaAgentDelegate.State;

	public ShipPlacementDetachment ShipPlacementDetachment { get; private set; }

	public override TextObject HitObjectName => new TextObject("{=1nbU1tV5}Ship", (Dictionary<string, object>)null);

	public static uint MissionShipScriptNameHash => _missionShipScriptNameHash;

	public MissionShip()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		_anyActiveFormationTroopOnShip = new QueryData<bool>((Func<bool>)delegate
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Invalid comparison between Unknown and I4
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Invalid comparison between Unknown and I4
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Invalid comparison between Unknown and I4
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Invalid comparison between Unknown and I4
			Formation formation = Formation;
			if (formation != null && formation.CountOfUnits > 0)
			{
				foreach (IFormationUnit item in (List<IFormationUnit>)(object)Formation.Arrangement.GetAllUnits())
				{
					Agent val;
					if ((val = (Agent)(object)((item is Agent) ? item : null)) != null)
					{
						AgentMovementMode val2 = (AgentMovementMode)(val.MovementMode & 3);
						if ((int)val2 != 2 && (int)val2 != 3)
						{
							return true;
						}
					}
				}
				foreach (Agent item2 in (List<Agent>)(object)Formation.DetachedUnits)
				{
					Agent current2;
					if ((current2 = item2) != null)
					{
						AgentMovementMode val3 = (AgentMovementMode)(current2.MovementMode & 3);
						if ((int)val3 != 2 && (int)val3 != 3)
						{
							return true;
						}
					}
				}
			}
			return false;
		}, 5f);
	}

	public void BreakAllExistingConnections()
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_shipAttachmentMachines)
		{
			if (item.CurrentAttachment != null)
			{
				item.CurrentAttachment.Destroy();
				item.CheckCurrentAttachmentAndInitializeRopeBoundingBox();
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.CurrentAttachment != null)
			{
				item2.CurrentAttachment.AttachmentSource?.CheckCurrentAttachmentAndInitializeRopeBoundingBox();
				item2.CurrentAttachment.Destroy();
			}
		}
	}

	public bool IsConnectionBlocked()
	{
		return _connectionBlockedShipTime > Mission.Current.CurrentTime;
	}

	public bool IsConnectionPermanentlyBlocked()
	{
		return MBMath.ApproximatelyEqualsTo(_connectionBlockedShipTime, float.MaxValue, 1E-05f);
	}

	public bool IsDisconnectionBlocked()
	{
		return _disconnectionBlockedShipTime > Mission.Current.CurrentTime;
	}

	public void BlockConnection()
	{
		_connectionBlockedShipTime = float.MaxValue;
	}

	public void ResetDisconnectionBlock()
	{
		_disconnectionBlockedShipTime = 0f;
	}

	public void ResetConnectionBlock()
	{
		_connectionBlockedShipTime = 0f;
	}

	public void SetShipOrderActive(bool isOrderActive)
	{
		IsShipOrderActive = isOrderActive;
	}

	public void SetShipClimbingOrderStandAloneTickingActive(bool isShipClimbingMachineStandaloneTickingActive)
	{
		IsClimbingMachineStandAloneTickingActive = isShipClimbingMachineStandaloneTickingActive;
	}

	public void SetFoldSailsOnBridgeConnection(bool value)
	{
		_foldSailsOnBridgeConnection = value;
	}

	public void SetOriginalTeamSide(TeamSideEnum teamSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		OriginalTeamSide = teamSide;
	}

	public void OnShipConnected(ShipAttachmentMachine.ShipAttachment currentAttachment)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (currentAttachment.AttachmentTarget.OwnerShip != this || currentAttachment.AttachmentSource.OwnerShip.BattleSide == BattleSide)
		{
			return;
		}
		bool flag = true;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ShipAttachmentMachines)
		{
			if (item.CurrentAttachment != currentAttachment && item.CurrentAttachment?.AttachmentTarget?.OwnerShip == this && item.CurrentAttachment.AttachmentSource.OwnerShip.BattleSide != BattleSide)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			_disconnectionBlockedShipTime = Mission.Current.CurrentTime + 30f;
			_connectionBlockedShipTime = 0f;
		}
	}

	public void OnShipDisconnected(ShipAttachmentMachine.ShipAttachment currentAttachment)
	{
		if (ShipsLogic.CanHaveConnectionCooldown && currentAttachment.AttachmentTarget.OwnerShip == this && _connectionBlockedShipTime <= 0f)
		{
			_connectionBlockedShipTime = Mission.Current.CurrentTime + 30f;
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (_isRemoved)
		{
			return (TickRequirement)0;
		}
		TickRequirement val = (TickRequirement)52;
		if (Mission.Current != null)
		{
			val = (TickRequirement)(val | 2);
		}
		return val;
	}

	public override void OnDeploymentFinished()
	{
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			((MissionObject)item).OnDeploymentFinished();
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			((MissionObject)item2).OnDeploymentFinished();
		}
		foreach (ShipOarMachine item3 in (List<ShipOarMachine>)(object)_leftSideShipOarMachines)
		{
			((MissionObject)item3).OnDeploymentFinished();
		}
		foreach (ShipOarMachine item4 in (List<ShipOarMachine>)(object)_rightSideShipOarMachines)
		{
			((MissionObject)item4).OnDeploymentFinished();
		}
		foreach (ClimbingMachine item5 in (List<ClimbingMachine>)(object)_climbingMachines)
		{
			((MissionObject)item5).OnDeploymentFinished();
		}
		((MissionObject)ShipControllerMachine).OnDeploymentFinished();
		RangedSiegeWeapon shipSiegeWeapon = ShipSiegeWeapon;
		if (shipSiegeWeapon != null)
		{
			((MissionObject)shipSiegeWeapon).OnDeploymentFinished();
		}
		foreach (AmmoBarrelBase item6 in (List<AmmoBarrelBase>)(object)_ammoBarrels)
		{
			((MissionObject)item6).OnDeploymentFinished();
		}
		MissionShipRam ram = _ram;
		if (ram != null)
		{
			((MissionObject)ram).OnDeploymentFinished();
		}
		SetSiegeWeaponsInitialAmmoCount();
		CrewSizeOnMainDeck = MissionGameModels.Current.MissionShipParametersModel.CalculateMainDeckCrewSize(ShipOrigin, Formation.GetFirstUnit());
		SetAnchor(isAnchored: false);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfTypeRecursive<ShipWaterEffects>()?.EnableWakeAndParticles();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfTypeRecursive<ShipFloatsamManager>()?.EnableFloatsamSystem();
		IsDeployed = true;
	}

	private void SetSiegeWeaponsInitialAmmoCount()
	{
		if (ShipSiegeWeapon != null)
		{
			int startAmmo = MissionGameModels.Current.MissionSiegeEngineCalculationModel.CalculateShipSiegeWeaponAmmoCount(ShipOrigin, Captain, ShipSiegeWeapon);
			ShipSiegeWeapon.SetStartAmmo(startAmmo);
		}
	}

	public override void SetAbilityOfFaces(bool enabled)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (base.DynamicNavmeshIdStart > 0)
		{
			for (int i = 0; i < 50; i++)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).Scene.SetAbilityOfFacesWithId(base.DynamicNavmeshIdStart + i, enabled);
			}
		}
	}

	public bool IsAgentOnShipNavmesh(int testedNavmeshID)
	{
		if (testedNavmeshID >= base.DynamicNavmeshIdStart)
		{
			return testedNavmeshID < base.DynamicNavmeshIdStart + 50;
		}
		return false;
	}

	public float GetPartialHitPoints(int index)
	{
		return _partialHitPoints[index];
	}

	public void SetController(ShipControllerType controllerType, bool autoUpdateController = true)
	{
		_autoUpdateController = autoUpdateController;
		if ((HasController ? Controller.ControllerType : ShipControllerType.None) != controllerType)
		{
			switch (controllerType)
			{
			case ShipControllerType.Player:
				Controller = new PlayerShipController(this);
				break;
			case ShipControllerType.AI:
				Controller = new AIShipController(this);
				break;
			default:
				Controller = null;
				break;
			}
			ShipsLogic.OnShipControllerChanged(this);
		}
	}

	public void SetCanBeTakenOver(bool value)
	{
		CanBeTakenOver = value;
	}

	public MBReadOnlyList<MissionShip> GetShipsConnectedWithBridges()
	{
		((List<MissionShip>)(object)_temporaryMissionShipContainer).Clear();
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment != null && item.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected && ((List<MissionShip>)(object)_temporaryMissionShipContainer).IndexOf(item.CurrentAttachment.AttachmentTarget.OwnerShip) < 0)
			{
				((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(item.CurrentAttachment.AttachmentTarget.OwnerShip);
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.CurrentAttachment != null && item2.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected && ((List<MissionShip>)(object)_temporaryMissionShipContainer).IndexOf(item2.CurrentAttachment.AttachmentSource.OwnerShip) < 0)
			{
				((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(item2.CurrentAttachment.AttachmentSource.OwnerShip);
			}
		}
		return (MBReadOnlyList<MissionShip>)(object)_temporaryMissionShipContainer;
	}

	public void SetInputRecord(in ShipInputRecord record)
	{
		_inputRecord = record;
	}

	public void SetOarAppliedForceMultiplierForStoryMission(float forceMultiplier)
	{
		_actuators.SetOarAppliedForceMultiplierForStoryMission(forceMultiplier);
	}

	public bool SearchShipConnection(MissionShip soughtShip, bool isDirect, bool findEnemy, bool enforceActive, bool acceptNotBridgedConnections)
	{
		((Queue<MissionShip>)(object)_temporaryMissionShipQueue).Clear();
		_visitedMissionShips.Clear();
		bool flag = false;
		foreach (MissionShip item in (List<MissionShip>)(object)(acceptNotBridgedConnections ? GetConnectedShips() : GetShipsConnectedWithBridges()))
		{
			if (item == this || item.Team == null)
			{
				continue;
			}
			if (item == soughtShip)
			{
				flag = true;
			}
			if (isDirect)
			{
				if (item != soughtShip)
				{
					Team team = Team;
					if (findEnemy != ((team != null) ? new bool?(team.IsEnemyOf(item.Team)) : ((bool?)null)) || (enforceActive && !item.AnyActiveFormationTroopOnShip))
					{
						goto IL_00be;
					}
				}
				((Queue<MissionShip>)(object)_temporaryMissionShipQueue).Clear();
				_visitedMissionShips.Clear();
				return true;
			}
			goto IL_00be;
			IL_00be:
			((Queue<MissionShip>)(object)_temporaryMissionShipQueue).Enqueue(item);
		}
		_visitedMissionShips.Add(this);
		while (((Queue<MissionShip>)(object)_temporaryMissionShipQueue).Count > 0)
		{
			MissionShip missionShip = ((Queue<MissionShip>)(object)_temporaryMissionShipQueue).Dequeue();
			_visitedMissionShips.Add(missionShip);
			if (flag || missionShip != soughtShip)
			{
				if (missionShip.Team != null)
				{
					Team team2 = Team;
					if (findEnemy == ((team2 != null) ? new bool?(team2.IsEnemyOf(missionShip.Team)) : ((bool?)null)) && (!enforceActive || missionShip.AnyActiveFormationTroopOnShip))
					{
						goto IL_016d;
					}
				}
				foreach (MissionShip item2 in (List<MissionShip>)(object)(acceptNotBridgedConnections ? missionShip.GetConnectedShips() : missionShip.GetShipsConnectedWithBridges()))
				{
					if (!_visitedMissionShips.Contains(item2))
					{
						((Queue<MissionShip>)(object)_temporaryMissionShipQueue).Enqueue(item2);
					}
				}
				continue;
			}
			goto IL_016d;
			IL_016d:
			((Queue<MissionShip>)(object)_temporaryMissionShipQueue).Clear();
			_visitedMissionShips.Clear();
			return true;
		}
		((Queue<MissionShip>)(object)_temporaryMissionShipQueue).Clear();
		_visitedMissionShips.Clear();
		return false;
	}

	public void SetFormation(Formation newFormation)
	{
		if (Formation != newFormation)
		{
			if (Formation != null)
			{
				ShipOrder.FormationLeaveShip();
				Formation.OnUnitAttached -= OnUnitAttached;
			}
			Formation = newFormation;
			if (newFormation != null)
			{
				ShipOrder.FormationJoinShip(Formation);
				Formation.OnUnitAttached += OnUnitAttached;
			}
		}
	}

	private void ProcessDetanglingShips()
	{
		if (_detanglingMissionShip != null)
		{
			float remainingTimeInSeconds = _detanglingMissionShipTimer.GetRemainingTimeInSeconds(false);
			if (remainingTimeInSeconds <= 3f)
			{
				float detanglingHarshness = 1f - remainingTimeInSeconds / 3f;
				DetangleShip(detanglingHarshness);
			}
		}
	}

	private void AddDetanglingShip(MissionShip ship, Vec3 contactPosAvg)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (_detanglingMissionShip == null || _detanglingMissionShip == ship)
		{
			if (_detanglingMissionShip == null)
			{
				_detanglingMissionShipTimer.Reset();
			}
			_detanglingMissionShip = ship;
			_detanglingMissionShipAverageContactPosition = contactPosAvg;
		}
	}

	private void RemoveDetanglingShip(MissionShip ship)
	{
		if (ship == _detanglingMissionShip)
		{
			_detanglingMissionShip = null;
			_detanglingMissionShipTimer.Reset();
		}
	}

	public static float CalculateShipAlignWithVectorZTorque(MissionShip ship, Vec3 alignVector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
		Mat3 rotation = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().rotation;
		float num = MathF.Atan2(rotation.f.y, rotation.f.x);
		float num2 = MathF.Atan2(alignVector.y, alignVector.x) - num;
		num2 = MBMath.WrapAngle(num2);
		if (MathF.Abs(num2) > MathF.PI / 2f)
		{
			num2 = ((!(num2 > 0f)) ? (num2 + MathF.PI) : (num2 - MathF.PI));
		}
		return num2 * 0.5f * ship.Physics.Mass * 50f - ship.Physics.AngularVelocity.z * ship.Physics.Mass * 60f;
	}

	private void DetangleShip(float detanglingHarshness)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_detanglingMissionShip).GameEntity;
		MatrixFrame shipFrame = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		if (!(((MatrixFrame)(ref shipFrame)).TransformToLocal(ref _detanglingMissionShipAverageContactPosition).z < 0f))
		{
			return;
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame shipFrame2 = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec2[] array = CalculateBoundingXYGlobalPlaneFromLocal(in shipFrame2, 0.9f);
		Vec2[] array2 = _detanglingMissionShip.CalculateBoundingXYGlobalPlaneFromLocal(in shipFrame, 0.9f);
		Vec3 globalForceVec;
		if (MBMath.CheckPolygonIntersection(array, array2))
		{
			globalForceVec = shipFrame.origin - _detanglingMissionShipAverageContactPosition;
			Vec3 val = ((Vec3)(ref globalForceVec)).NormalizedCopy();
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			if (((Vec2)(ref asVec)).LengthSquared < 0.01f)
			{
				val.x = shipFrame.rotation.f.x;
				val.y = shipFrame.rotation.f.y;
				((Vec3)(ref val)).Normalize();
			}
			float num = 2f * detanglingHarshness;
			float mass = _detanglingMissionShip.Physics.Mass;
			float num2 = MathF.Min(_detanglingMissionShip.Physics.Mass, Physics.Mass);
			Vec3 globalForceVec2 = val * mass * num;
			Vec3 localPos = ((MatrixFrame)(ref shipFrame)).TransformToLocal(ref _detanglingMissionShipAverageContactPosition);
			float num3 = num2 * 5f;
			if (((Vec3)(ref globalForceVec2)).LengthSquared >= num3 * num3)
			{
				((Vec3)(ref globalForceVec2)).Normalize();
				globalForceVec2 *= num3;
			}
			_detanglingMissionShip.Physics.ApplyGlobalForceAtLocalPos(in localPos, in globalForceVec2, (ForceMode)0);
			Vec3 localPos2 = ((MatrixFrame)(ref shipFrame2)).TransformToLocal(ref _detanglingMissionShipAverageContactPosition);
			NavalDLC.Missions.NavalPhysics.NavalPhysics physics = Physics;
			globalForceVec = -globalForceVec2;
			physics.ApplyGlobalForceAtLocalPos(in localPos2, in globalForceVec, (ForceMode)0);
		}
		float num4 = CalculateShipAlignWithVectorZTorque(_detanglingMissionShip, shipFrame2.rotation.s);
		NavalDLC.Missions.NavalPhysics.NavalPhysics physics2 = _detanglingMissionShip.Physics;
		globalForceVec = new Vec3(0f, 0f, num4, -1f);
		physics2.ApplyTorque(in globalForceVec, (ForceMode)0);
	}

	private void InitializeDetanglingShipInformation()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_detanglingMissionShip = null;
		_detanglingMissionShipTimer = new MissionTimer(6f);
		_detanglingMissionShipAverageContactPosition = Vec3.Zero;
	}

	private void InitializeLocalPhysicsBoundingXYPlane()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		_localPhysicsBoundingBoxXYPlaneVertices = (Vec2[])(object)new Vec2[4];
		_scaledLocalPhysicsBoundingBoxXYPlaneVertices = (Vec2[])(object)new Vec2[4];
		Vec3 min = Physics.PhysicsBoundingBoxWithoutChildren.min;
		Vec3 max = Physics.PhysicsBoundingBoxWithoutChildren.max;
		_localPhysicsBoundingBoxXYPlaneVertices[0] = new Vec2(min.x, min.y);
		_localPhysicsBoundingBoxXYPlaneVertices[1] = new Vec2(min.x, max.y);
		_localPhysicsBoundingBoxXYPlaneVertices[2] = new Vec2(max.x, max.y);
		_localPhysicsBoundingBoxXYPlaneVertices[3] = new Vec2(max.x, min.y);
		_scaledLocalPhysicsBoundingBoxXYPlaneVertices[0] = _localPhysicsBoundingBoxXYPlaneVertices[0];
		_scaledLocalPhysicsBoundingBoxXYPlaneVertices[1] = _localPhysicsBoundingBoxXYPlaneVertices[1];
		_scaledLocalPhysicsBoundingBoxXYPlaneVertices[2] = _localPhysicsBoundingBoxXYPlaneVertices[2];
		_scaledLocalPhysicsBoundingBoxXYPlaneVertices[3] = _localPhysicsBoundingBoxXYPlaneVertices[3];
	}

	public Vec2[] CalculateBoundingXYGlobalPlaneFromLocal(in MatrixFrame shipFrame, float scale = 1f)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Vec2[] physicsBoundingBoxXYPlaneVertices = _physicsBoundingBoxXYPlaneVertices;
		MatrixFrame val = shipFrame;
		Vec2 val2 = _localPhysicsBoundingBoxXYPlaneVertices[0] * scale;
		physicsBoundingBoxXYPlaneVertices[0] = ((MatrixFrame)(ref val)).TransformToParent(ref val2);
		Vec2[] physicsBoundingBoxXYPlaneVertices2 = _physicsBoundingBoxXYPlaneVertices;
		val = shipFrame;
		val2 = _localPhysicsBoundingBoxXYPlaneVertices[1] * scale;
		physicsBoundingBoxXYPlaneVertices2[1] = ((MatrixFrame)(ref val)).TransformToParent(ref val2);
		Vec2[] physicsBoundingBoxXYPlaneVertices3 = _physicsBoundingBoxXYPlaneVertices;
		val = shipFrame;
		val2 = _localPhysicsBoundingBoxXYPlaneVertices[2] * scale;
		physicsBoundingBoxXYPlaneVertices3[2] = ((MatrixFrame)(ref val)).TransformToParent(ref val2);
		Vec2[] physicsBoundingBoxXYPlaneVertices4 = _physicsBoundingBoxXYPlaneVertices;
		val = shipFrame;
		val2 = _localPhysicsBoundingBoxXYPlaneVertices[3] * scale;
		physicsBoundingBoxXYPlaneVertices4[3] = ((MatrixFrame)(ref val)).TransformToParent(ref val2);
		return _physicsBoundingBoxXYPlaneVertices;
	}

	public Vec2[] GetLocalPhysicsBoundingBoxXYPlaneVertices(float scale = 1f)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (scale == 1f)
		{
			return _localPhysicsBoundingBoxXYPlaneVertices;
		}
		for (int i = 0; i < 4; i++)
		{
			_scaledLocalPhysicsBoundingBoxXYPlaneVertices[i] = _localPhysicsBoundingBoxXYPlaneVertices[i] * scale;
		}
		return _scaledLocalPhysicsBoundingBoxXYPlaneVertices;
	}

	public void SetSinkingState(NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState state)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		switch (state)
		{
		case NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sinking:
		{
			for (int i = 0; i < _partialHitPoints.Length; i++)
			{
				_partialHitPoints[i] = 0f;
				_physics.SetTargetDurabilityOfPart(i, 0f);
			}
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).AddBodyFlags((BodyFlags)1073741824, true);
			foreach (UsableMachine item in (List<UsableMachine>)(object)MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<UsableMachine>(((ScriptComponentBehavior)this).GameEntity))
			{
				((ScriptComponentBehavior)item).SetScriptComponentToTick(((ScriptComponentBehavior)item).GetTickRequirement());
			}
			SetController(ShipControllerType.None);
			if (Team != null)
			{
				if (Team == Mission.Current.PlayerTeam || Team == Mission.Current.PlayerAllyTeam)
				{
					MBInformationManager.AddQuickInformation(PlayerSideShipSinkingText, 0, (BasicCharacterObject)null, (Equipment)null, "");
				}
				else if (Team == Mission.Current.PlayerEnemyTeam)
				{
					MBInformationManager.AddQuickInformation(EnemySideShipSinkingText, 0, (BasicCharacterObject)null, (Equipment)null, "");
				}
			}
			ClimbingMachineDetachment.Deactivate();
			break;
		}
		case NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sunk:
		{
			((MissionObject)this).SetDisabled(true);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			SoundManager.StartOneShotEvent("event:/mission/movement/vessel/ship_sink", ref globalPosition);
			break;
		}
		}
		_physics.SetSinkingState(state);
	}

	public void SetAnchor(bool isAnchored, bool anchorInPlace = false, float forceMultiplier = 1f)
	{
		_physics.SetAnchor(isAnchored, anchorInPlace, forceMultiplier);
	}

	public void SetAnchorFrame(in Vec2 position, in Vec2 direction, float forceMultiplier = 1f)
	{
		_physics.SetAnchorFrame(in position, in direction, forceMultiplier);
	}

	public void DealCollisionDamage(MissionShip hitterShip, bool isRamDamage, Vec3 point, float damage)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		int inflictedDamage;
		int modifiedDamage;
		DamageTypes damageType;
		bool isFatalDamage;
		float num = DealDamage(damage, hitterShip, out inflictedDamage, out modifiedDamage, out damageType, out isFatalDamage);
		bool flag = hitterShip?.IsPlayerShip ?? false;
		if (Agent.Main != null && Agent.Main.IsActive() && (flag || IsPlayerShip) && inflictedDamage > 0)
		{
			CombatLogData val = default(CombatLogData);
			((CombatLogData)(ref val))._002Ector(false, flag, flag, false, false, false, IsPlayerShip, IsPlayerShip, false, false, false, false, (MissionObject)(object)this, false, false, false, 0f);
			val.InflictedDamage = inflictedDamage;
			val.ModifiedDamage = modifiedDamage;
			val.DamageType = damageType;
			val.IsFatalDamage = isFatalDamage;
			val.IsEntityToEntityCollisionDamage = true;
			if (isRamDamage)
			{
				val.IsSpecialDamage = true;
			}
			Mission.Current.AddCombatLogSafe((Agent)null, (Agent)null, val);
		}
		_moraleInteractionLogic?.OnShipRammed(hitterShip, this);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec3 val2 = ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref point);
		int partIndexAtPosition = _physics.GetPartIndexAtPosition(val2);
		if (partIndexAtPosition < 0 || partIndexAtPosition >= _partialHitPoints.Length)
		{
			Debug.FailedAssert($"DealRammingDamage: GetPartIndexAtPosition for localPos {val2} returned {partIndexAtPosition}.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\Objects\\MissionShip.cs", "DealCollisionDamage", 1161);
			return;
		}
		_partialHitPoints[partIndexAtPosition] = MathF.Max(0f, _partialHitPoints[partIndexAtPosition] - num);
		_physics.SetTargetDurabilityOfPart(partIndexAtPosition, _partialHitPoints[partIndexAtPosition] / MaxPartialHealth);
	}

	public void ResetFormationPositioning()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		GetWorldPositionOnDeck(out var worldPosition);
		Formation formation = Formation;
		WorldPosition? val = worldPosition;
		MatrixFrame globalFrame = GlobalFrame;
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		formation.SetPositioning(val, (Vec2?)((Vec2)(ref asVec)).Normalized(), (int?)null);
	}

	public float DealDamage(float rawDamage, MissionShip rammingShip, out int inflictedDamage, out int modifiedDamage, out DamageTypes damageType, out bool isFatalDamage)
	{
		float hitPoints = HitPoints;
		float num = default(float);
		ShipOrigin.OnShipDamaged(rawDamage, rammingShip?.ShipOrigin, ref num);
		modifiedDamage = (int)num;
		float result = hitPoints - HitPoints;
		damageType = (DamageTypes)2;
		isFatalDamage = false;
		if (HitPoints <= 0f && _physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Floating)
		{
			SetSinkingState(NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sinking);
			_moraleInteractionLogic?.OnShipSunk(this);
			isFatalDamage = true;
		}
		if (HitPoints / ShipOrigin.MaxHitPoints <= 0.1f && hitPoints / ShipOrigin.MaxHitPoints > 0.1f)
		{
			ShipsLogic.OnShipLowHealth(this);
		}
		inflictedDamage = (int)rawDamage;
		return result;
	}

	public float DealDamageToSails(Agent attackerAgent, float rawDamage, float inflictedDamage, MissionSail sailHit)
	{
		float sailHitPoints = SailHitPoints;
		ShipOrigin.OnSailDamaged(rawDamage, inflictedDamage);
		float result = sailHitPoints - SailHitPoints;
		sailHit?.OnSailHit(attackerAgent, rawDamage, inflictedDamage);
		if (SailHitPoints <= 0f && ShipSailState == SailState.Intact)
		{
			foreach (MissionSail item in (List<MissionSail>)(object)Sails)
			{
				item.StartFire();
			}
			SailBurnerAgent = attackerAgent;
			ShipSailState = SailState.Burning;
			if (!SailBurningSoundEvent.IsPlaying())
			{
				SailBurningSoundEvent.Play();
			}
		}
		return result;
	}

	public bool GetIsConnected()
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment?.AttachmentTarget?.OwnerShip != null)
			{
				return true;
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.CurrentAttachment?.AttachmentSource?.OwnerShip != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetIsConnectedToEnemyWithoutBridges()
	{
		bool result = false;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (Team != null && item.CurrentAttachment?.AttachmentTarget?.OwnerShip?.Team != null && item.CurrentAttachment.AttachmentTarget.OwnerShip.Team.IsEnemyOf(Team))
			{
				if (item.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected || item.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeThrown)
				{
					return false;
				}
				result = true;
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (Team != null && item2.CurrentAttachment?.AttachmentSource?.OwnerShip?.Team != null && item2.CurrentAttachment.AttachmentTarget.OwnerShip.Team.IsEnemyOf(Team))
			{
				if (item2.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected || item2.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeThrown)
				{
					return false;
				}
				result = true;
			}
		}
		return result;
	}

	public bool HasThrownOrActiveBridgeConnections()
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment?.AttachmentTarget != null)
			{
				ShipAttachmentMachine.ShipAttachment.ShipAttachmentState state = item.CurrentAttachment.State;
				if (state == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeThrown || state == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected)
				{
					return true;
				}
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.CurrentAttachment?.AttachmentSource != null)
			{
				ShipAttachmentMachine.ShipAttachment.ShipAttachmentState state2 = item2.CurrentAttachment.State;
				if (state2 == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeThrown || state2 == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasMachine(UsableMachine usableMachine)
	{
		if ((object)ShipControllerMachine == usableMachine)
		{
			return true;
		}
		if (_shipOarMachines != null && ((IEnumerable<UsableMachine>)_shipOarMachines).Contains(usableMachine))
		{
			return true;
		}
		if ((object)ShipSiegeWeapon == usableMachine)
		{
			return true;
		}
		if (_attachmentMachines != null && ((IEnumerable<UsableMachine>)_attachmentMachines).Contains(usableMachine))
		{
			return true;
		}
		if (_attachmentPointMachines != null && ((IEnumerable<UsableMachine>)_attachmentPointMachines).Contains(usableMachine))
		{
			return true;
		}
		if (_shipAttachmentMachines != null && ((IEnumerable<UsableMachine>)_shipAttachmentMachines).Contains(usableMachine))
		{
			return true;
		}
		if (_climbingMachines != null && ((IEnumerable<UsableMachine>)_climbingMachines).Contains(usableMachine))
		{
			return true;
		}
		return false;
	}

	public bool IsShipInCriticalZoneBetween(MissionShip ship2, MBReadOnlyList<MissionShip> allShips)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vec2[] criticalZoneVerticesBetween = GetCriticalZoneVerticesBetween(ship2);
		foreach (MissionShip item in (List<MissionShip>)(object)allShips)
		{
			if (item != this && item != ship2)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				Vec2[] array = item.CalculateBoundingXYGlobalPlaneFromLocal(((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform());
				if (MBMath.CheckPolygonIntersection(criticalZoneVerticesBetween, array))
				{
					return true;
				}
				if (MBMath.CheckPointInsidePolygon(ref criticalZoneVerticesBetween[0], ref criticalZoneVerticesBetween[1], ref criticalZoneVerticesBetween[2], ref criticalZoneVerticesBetween[3], ref array[0]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public Vec2[] GetCriticalZoneVerticesBetween(MissionShip otherShip)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		float num = 6.35f;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame shipFrame = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		gameEntity = ((ScriptComponentBehavior)otherShip).GameEntity;
		MatrixFrame shipFrame2 = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec2[] array = CalculateBoundingXYGlobalPlaneFromLocal(in shipFrame);
		Vec2[] array2 = otherShip.CalculateBoundingXYGlobalPlaneFromLocal(in shipFrame2);
		Vec2 val = array[0];
		Vec2 val2 = array[3];
		Vec2 val3 = array[0];
		Vec2 val4 = array[1];
		Vec2 val5 = array[3];
		Vec2 val6 = array[2];
		Vec2 val7 = array2[0];
		Vec2 val8 = array2[1];
		Vec2 val9 = array2[3];
		Vec2 val10 = array2[2];
		float distanceSquareOfPointToLineSegment = MBMath.GetDistanceSquareOfPointToLineSegment(ref val7, ref val8, val);
		float distanceSquareOfPointToLineSegment2 = MBMath.GetDistanceSquareOfPointToLineSegment(ref val7, ref val8, val2);
		Vec2 val11;
		Vec2 val12;
		if (distanceSquareOfPointToLineSegment < distanceSquareOfPointToLineSegment2)
		{
			val11 = val3;
			val12 = val4;
		}
		else
		{
			val11 = val5;
			val12 = val6;
		}
		distanceSquareOfPointToLineSegment2 = MBMath.GetDistanceSquareOfPointToLineSegment(ref val9, ref val10, val);
		Vec2 val13;
		Vec2 val14;
		if (distanceSquareOfPointToLineSegment < distanceSquareOfPointToLineSegment2)
		{
			val13 = val7;
			val14 = val8;
		}
		else
		{
			val13 = val9;
			val14 = val10;
		}
		Vec2 val15 = MBMath.ProjectPointOntoLine(val13, val11, val12);
		Vec2 val16 = MBMath.ProjectPointOntoLine(val14, val11, val12);
		Vec2 val17 = MBMath.ProjectPointOntoLine(val11, val13, val14);
		Vec2 val18 = MBMath.ProjectPointOntoLine(val12, val13, val14);
		Vec3 f = shipFrame.rotation.f;
		Vec3 f2 = shipFrame2.rotation.f;
		int num2 = ((!(Vec3.DotProduct(f, f2) < 0f)) ? 1 : (-1));
		val15 = MBMath.ClampToAxisAlignedRectangle(val15, val11, val12);
		val16 = MBMath.ClampToAxisAlignedRectangle(val16, val11, val12);
		val17 = MBMath.ClampToAxisAlignedRectangle(val17, val13, val14);
		val18 = MBMath.ClampToAxisAlignedRectangle(val18, val13, val14);
		Vec2 val19 = val12 - val11;
		Vec2 val20 = ((Vec2)(ref val19)).Normalized();
		val19 = val14 - val13;
		Vec2 val21 = ((Vec2)(ref val19)).Normalized();
		val15 += num * val20 * (float)num2;
		val16 -= num * val20 * (float)num2;
		val17 += num * val21 * (float)num2;
		val18 -= num * val21 * (float)num2;
		if (num2 > 0)
		{
			_criticalZoneVertices[0] = val15;
			_criticalZoneVertices[1] = val17;
			_criticalZoneVertices[2] = val18;
			_criticalZoneVertices[3] = val16;
		}
		else
		{
			_criticalZoneVertices[0] = val17;
			_criticalZoneVertices[1] = val16;
			_criticalZoneVertices[2] = val15;
			_criticalZoneVertices[3] = val18;
		}
		return _criticalZoneVertices;
	}

	public bool GetIsConnectedToEnemy()
	{
		Team team = Team;
		bool flag2;
		if (team != null)
		{
			foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
			{
				ShipAttachmentMachine.ShipAttachment currentAttachment = item.CurrentAttachment;
				if (currentAttachment == null)
				{
					continue;
				}
				ShipAttachmentPointMachine attachmentTarget = currentAttachment.AttachmentTarget;
				bool? obj;
				if (attachmentTarget == null)
				{
					obj = null;
				}
				else
				{
					Team team2 = attachmentTarget.OwnerShip.Team;
					obj = ((team2 != null) ? new bool?(team2.IsEnemyOf(team)) : ((bool?)null));
				}
				bool? flag = obj;
				flag2 = true;
				if (flag != flag2)
				{
					continue;
				}
				flag2 = true;
				goto IL_0123;
			}
			foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
			{
				ShipAttachmentMachine.ShipAttachment currentAttachment2 = item2.CurrentAttachment;
				if (currentAttachment2 == null)
				{
					continue;
				}
				Team team3 = currentAttachment2.AttachmentSource.OwnerShip.Team;
				if (((team3 != null) ? new bool?(team3.IsEnemyOf(team)) : ((bool?)null)) != true)
				{
					continue;
				}
				flag2 = true;
				goto IL_0123;
			}
		}
		return false;
		IL_0123:
		return flag2;
	}

	public bool GetIsConnectedToEnemy(out MissionShip connectedEnemyShip)
	{
		Team team = Team;
		bool flag2;
		if (team != null)
		{
			foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
			{
				ShipAttachmentMachine.ShipAttachment currentAttachment = item.CurrentAttachment;
				if (currentAttachment == null)
				{
					continue;
				}
				ShipAttachmentPointMachine attachmentTarget = currentAttachment.AttachmentTarget;
				bool? obj;
				if (attachmentTarget == null)
				{
					obj = null;
				}
				else
				{
					Team team2 = attachmentTarget.OwnerShip.Team;
					obj = ((team2 != null) ? new bool?(team2.IsEnemyOf(team)) : ((bool?)null));
				}
				bool? flag = obj;
				flag2 = true;
				if (flag != flag2)
				{
					continue;
				}
				connectedEnemyShip = item.CurrentAttachment.AttachmentTarget.OwnerShip;
				flag2 = true;
				goto IL_015b;
			}
			foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
			{
				ShipAttachmentMachine.ShipAttachment currentAttachment2 = item2.CurrentAttachment;
				if (currentAttachment2 == null)
				{
					continue;
				}
				Team team3 = currentAttachment2.AttachmentSource.OwnerShip.Team;
				if (((team3 != null) ? new bool?(team3.IsEnemyOf(team)) : ((bool?)null)) != true)
				{
					continue;
				}
				connectedEnemyShip = item2.CurrentAttachment.AttachmentSource.OwnerShip;
				flag2 = true;
				goto IL_015b;
			}
		}
		connectedEnemyShip = null;
		return false;
		IL_015b:
		return flag2;
	}

	public bool GetIsConnectedToEnemyWithSide(out Vec2 direction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		direction = Vec2.Zero;
		bool flag = false;
		bool flag3;
		if (Team != null)
		{
			WeakGameEntity gameEntity;
			MatrixFrame globalFrame;
			Vec3 val;
			foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
			{
				ShipAttachmentMachine.ShipAttachment currentAttachment = item.CurrentAttachment;
				if (currentAttachment == null)
				{
					continue;
				}
				ShipAttachmentPointMachine attachmentTarget = currentAttachment.AttachmentTarget;
				bool? obj;
				if (attachmentTarget == null)
				{
					obj = null;
				}
				else
				{
					Team team = attachmentTarget.OwnerShip.Team;
					obj = ((team != null) ? new bool?(team.IsEnemyOf(Team)) : ((bool?)null));
				}
				bool? flag2 = obj;
				flag3 = true;
				if (flag2 != flag3)
				{
					continue;
				}
				flag = true;
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				val = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalPosition);
				Vec2 asVec = ((Vec3)(ref val)).AsVec2;
				if (direction.x * asVec.x >= 0f)
				{
					direction += asVec;
					continue;
				}
				direction = Vec2.Zero;
				flag3 = true;
				goto IL_0231;
			}
			foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
			{
				ShipAttachmentMachine.ShipAttachment currentAttachment2 = item2.CurrentAttachment;
				if (currentAttachment2 == null)
				{
					continue;
				}
				Team team2 = currentAttachment2.AttachmentSource.OwnerShip.Team;
				if (((team2 != null) ? new bool?(team2.IsEnemyOf(Team)) : ((bool?)null)) != true)
				{
					continue;
				}
				flag = true;
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				val = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalPosition);
				Vec2 asVec = ((Vec3)(ref val)).AsVec2;
				if (direction.x * asVec.x >= 0f)
				{
					direction += asVec;
					continue;
				}
				direction = Vec2.Zero;
				flag3 = true;
				goto IL_0231;
			}
			if (flag)
			{
				((Vec2)(ref direction)).Normalize();
			}
			return flag;
		}
		return false;
		IL_0231:
		return flag3;
	}

	public void OnShipObjectUpdated()
	{
		_actuators.OnShipObjectUpdated();
		InitializeNavalPhysics();
	}

	public MBReadOnlyList<MissionShip> GetConnectedShips()
	{
		((List<MissionShip>)(object)_temporaryMissionShipContainer).Clear();
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment?.AttachmentTarget?.OwnerShip != null)
			{
				((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(item.CurrentAttachment.AttachmentTarget.OwnerShip);
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.CurrentAttachment?.AttachmentSource?.OwnerShip != null)
			{
				((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(item2.CurrentAttachment.AttachmentSource.OwnerShip);
			}
		}
		return (MBReadOnlyList<MissionShip>)(object)_temporaryMissionShipContainer;
	}

	public int GetDynamicNavmeshIdStart()
	{
		return base.DynamicNavmeshIdStart;
	}

	public bool GetBridgeWithEnemyActive()
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.IsShipAttachmentMachineBridgeWithEnemy())
			{
				return true;
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.IsShipAttachmentMachinePointBridgeWithEnemy())
			{
				return true;
			}
		}
		return false;
	}

	public bool GetIsAnyBridgeActive()
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.IsShipAttachmentMachineBridged())
			{
				return true;
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.IsShipAttachmentPointBridged())
			{
				return true;
			}
		}
		return false;
	}

	public void GetWorldPositionOnDeck(out WorldPosition worldPosition)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (_isCachedWorldPositionOnDeckDirty)
		{
			MatrixFrame globalFrame = GlobalFrame;
			MatrixFrame rallyFrame = RallyFrame;
			WorldPosition val = ModuleExtensions.ToWorldPosition(((MatrixFrame)(ref globalFrame)).TransformToParent(ref rallyFrame).origin);
			_cachedWorldPositionOnDeck = ModuleExtensions.ToWorldPosition(((WorldPosition)(ref val)).GetNavMeshVec3());
			_isCachedWorldPositionOnDeckDirty = false;
		}
		worldPosition = _cachedWorldPositionOnDeck;
	}

	public NavalState GetNavalState(in NavalVec localOffset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = GlobalFrame;
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.s)).AsVec2;
		Vec2 val = ((Vec2)(ref asVec)).Normalized();
		asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Vec2 val2 = ((Vec2)(ref asVec)).Normalized();
		MatrixFrame globalFrame2 = GlobalFrame;
		Vec2 position = ((Vec3)(ref globalFrame2.origin)).AsVec2 + localOffset.DeltaPosition.x * val + localOffset.DeltaPosition.y * val2;
		Vec2 direction = val2;
		((Vec2)(ref direction)).RotateCCW(localOffset.DeltaOrientation);
		Vec3 linearVelocity = _physics.LinearVelocity;
		float num = Vec2.DotProduct(((Vec3)(ref linearVelocity)).AsVec2, val2);
		num += localOffset.DeltaSpeed;
		return new NavalState(in position, in direction, num);
	}

	public FacingOrder GetFacingOrderToRallyPoint()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = RallyFrame;
		MatrixFrame val2;
		if (((MatrixFrame)(ref val)).IsZero)
		{
			val2 = GlobalFrame;
		}
		else
		{
			val = GlobalFrame;
			MatrixFrame rallyFrame = RallyFrame;
			val2 = ((MatrixFrame)(ref val)).TransformToParent(ref rallyFrame);
		}
		MatrixFrame val3 = val2;
		Vec2 asVec = ((Vec3)(ref val3.rotation.f)).AsVec2;
		return FacingOrder.FacingOrderLookAtDirection(((Vec2)(ref asVec)).Normalized());
	}

	public MovementOrder GetMovementOrderToRallyPoint()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = RallyFrame;
		MatrixFrame val2;
		if (((MatrixFrame)(ref val)).IsZero)
		{
			val2 = GlobalFrame;
		}
		else
		{
			val = GlobalFrame;
			MatrixFrame rallyFrame = RallyFrame;
			val2 = ((MatrixFrame)(ref val)).TransformToParent(ref rallyFrame);
		}
		return MovementOrder.MovementOrderMove(ModuleExtensions.ToWorldPosition(val2.origin));
	}

	public void SetPositioningOrdersToRallyPoint(bool applyToPlayerFormation, bool playersOrder)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (applyToPlayerFormation || Formation.PlayerOwner != Mission.Current.MainAgent || !Formation.HasPlayerControlledTroop)
		{
			MatrixFrame val = RallyFrame;
			MatrixFrame val2;
			if (((MatrixFrame)(ref val)).IsZero)
			{
				val2 = GlobalFrame;
			}
			else
			{
				val = GlobalFrame;
				MatrixFrame rallyFrame = RallyFrame;
				val2 = ((MatrixFrame)(ref val)).TransformToParent(ref rallyFrame);
			}
			MatrixFrame val3 = val2;
			WorldPosition val4 = ModuleExtensions.ToWorldPosition(val3.origin);
			Vec2 asVec = ((Vec3)(ref val3.rotation.f)).AsVec2;
			Vec2 val5 = ((Vec2)(ref asVec)).Normalized();
			Formation.SetMovementOrder(MovementOrder.MovementOrderMove(val4));
			Formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderShieldWall);
			Formation.SetFacingOrder(FacingOrder.FacingOrderLookAtDirection(val5));
		}
		if (applyToPlayerFormation)
		{
			ShipOrder.JoinPlayerFormationToPlacementDetachment(playersOrder);
		}
	}

	public MBReadOnlyList<MissionShip> GetNavmeshConnectedShips()
	{
		((List<MissionShip>)(object)_temporaryMissionShipContainer).Clear();
		ulong num = 0uL;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment == null || !item.CurrentAttachment.IsNavmeshConnected)
			{
				continue;
			}
			MissionShip ownerShip = item.CurrentAttachment.AttachmentSource.OwnerShip;
			MissionShip ownerShip2 = item.CurrentAttachment.AttachmentTarget.OwnerShip;
			if (ownerShip != this)
			{
				if ((num & ownerShip.ShipUniqueBitwiseID) == 0L)
				{
					((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(ownerShip);
					num |= ownerShip.ShipUniqueBitwiseID;
				}
			}
			else if ((num & ownerShip2.ShipUniqueBitwiseID) == 0L)
			{
				((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(ownerShip2);
				num |= ownerShip2.ShipUniqueBitwiseID;
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.CurrentAttachment == null || !item2.CurrentAttachment.IsNavmeshConnected)
			{
				continue;
			}
			MissionShip ownerShip3 = item2.CurrentAttachment.AttachmentSource.OwnerShip;
			MissionShip ownerShip4 = item2.CurrentAttachment.AttachmentTarget.OwnerShip;
			if (ownerShip3 != this)
			{
				if ((num & ownerShip3.ShipUniqueBitwiseID) == 0L)
				{
					((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(ownerShip3);
					num |= ownerShip3.ShipUniqueBitwiseID;
				}
			}
			else if ((num & ownerShip4.ShipUniqueBitwiseID) == 0L)
			{
				((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(ownerShip4);
				num |= ownerShip4.ShipUniqueBitwiseID;
			}
		}
		return (MBReadOnlyList<MissionShip>)(object)_temporaryMissionShipContainer;
	}

	public int ComputeActiveShipAttachmentCount()
	{
		int num = 0;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment?.AttachmentTarget != null)
			{
				num++;
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.CurrentAttachment != null)
			{
				num++;
			}
		}
		return num;
	}

	public void UpdateSailBurningSoundPosition()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.Zero;
		WeakGameEntity gameEntity;
		if (((List<MissionSail>)(object)Sails).Count > 0)
		{
			foreach (MissionSail item in (List<MissionSail>)(object)Sails)
			{
				val += item.CenterOfSailForceShipLocal;
			}
			val /= (float)((List<MissionSail>)(object)Sails).Count;
		}
		else
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref gameEntity)).CenterOfMass;
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 position = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		SailBurningSoundEvent.SetPosition(position);
	}

	protected override void OnSaveAsPrefab()
	{
	}

	public MissionShip GetOutermostConnectedShipFromSide(bool rightSide, out bool effectiveSideOfOutermostShip, ulong aggregateShipUniqueID)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		if ((aggregateShipUniqueID & ShipUniqueBitwiseID) != 0L)
		{
			effectiveSideOfOutermostShip = rightSide;
			return this;
		}
		aggregateShipUniqueID |= ShipUniqueBitwiseID;
		MatrixFrame globalFrame = GlobalFrame;
		WeakGameEntity gameEntity;
		MatrixFrame globalFrame2;
		Vec2 asVec;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			bool num = !rightSide;
			gameEntity = ((ScriptComponentBehavior)item).GameEntity;
			globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			if ((num ^ (((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalFrame2).origin.x > 0f)) && item.CurrentAttachment?.AttachmentTarget != null)
			{
				MissionShip ownerShip = item.CurrentAttachment.AttachmentTarget.OwnerShip;
				asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				globalFrame2 = ownerShip.GlobalFrame;
				return ownerShip.GetOutermostConnectedShipFromSide((((Vec2)(ref asVec)).DotProduct(((Vec3)(ref globalFrame2.rotation.f)).AsVec2) >= 0f) ? rightSide : (!rightSide), out effectiveSideOfOutermostShip, aggregateShipUniqueID);
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			bool num2 = !rightSide;
			gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
			globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			if ((num2 ^ (((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalFrame2).origin.x > 0f)) && item2.CurrentAttachment?.AttachmentSource != null)
			{
				MissionShip ownerShip2 = item2.CurrentAttachment.AttachmentSource.OwnerShip;
				asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				globalFrame2 = ownerShip2.GlobalFrame;
				return ownerShip2.GetOutermostConnectedShipFromSide((((Vec2)(ref asVec)).DotProduct(((Vec3)(ref globalFrame2.rotation.f)).AsVec2) >= 0f) ? rightSide : (!rightSide), out effectiveSideOfOutermostShip, aggregateShipUniqueID);
			}
		}
		effectiveSideOfOutermostShip = rightSide;
		return this;
	}

	protected override void OnFixedTick(float fixedDt)
	{
		if (!_isRemoved)
		{
			ProcessDetanglingShips();
		}
	}

	protected override void OnInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_attachmentMachines = MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<ShipAttachmentMachine>(((ScriptComponentBehavior)this).GameEntity);
		_attachmentPointMachines = MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<ShipAttachmentPointMachine>(((ScriptComponentBehavior)this).GameEntity);
		InitializeLists(isForCheckingForProblems: false);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetHasCustomBoundingBoxValidationSystem(true);
		((MissionObject)this).OnInit();
	}

	protected override void OnBoundingBoxValidate()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).RelaxLocalBoundingBox(ref _localBoundingBoxCached);
	}

	public bool GetIsAgentOnShip(Agent agent, bool bypassSteppedShipCheck = false)
	{
		if (!bypassSteppedShipCheck && (agent.IsInWater() || agent.GetComponent<AgentNavalComponent>()?.SteppedShip == null))
		{
			return false;
		}
		int currentNavigationFaceId = agent.GetCurrentNavigationFaceId();
		return IsAgentOnShipNavmesh(currentNavigationFaceId);
	}

	public bool GetNextCrewSpawnGlobalFrame(out MatrixFrame crewSpawnGlobalFrame)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (_crewSpawnLocalFrames != null && !Extensions.IsEmpty<MatrixFrame>((IEnumerable<MatrixFrame>)_crewSpawnLocalFrames))
		{
			int nextCrewSpawnFrameIndex = _nextCrewSpawnFrameIndex;
			_nextCrewSpawnFrameIndex = (_nextCrewSpawnFrameIndex + 1) % ((List<MatrixFrame>)(object)_crewSpawnLocalFrames).Count;
			MatrixFrame globalFrame = GlobalFrame;
			MatrixFrame val = ((List<MatrixFrame>)(object)_crewSpawnLocalFrames)[nextCrewSpawnFrameIndex];
			crewSpawnGlobalFrame = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
			return true;
		}
		crewSpawnGlobalFrame = MatrixFrame.Identity;
		return false;
	}

	public MatrixFrame GetNextOuterInnerSpawnGlobalFrame()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		int nextDeckSpawnFrameIndex = _nextDeckSpawnFrameIndex;
		_nextDeckSpawnFrameIndex = (_nextDeckSpawnFrameIndex + 1) % DeckFrameCount;
		MatrixFrame globalFrame = GlobalFrame;
		MatrixFrame val = ((nextDeckSpawnFrameIndex >= ((List<MatrixFrame>)(object)OuterDeckLocalFrames).Count) ? ((List<MatrixFrame>)(object)InnerDeckLocalFrames)[nextDeckSpawnFrameIndex - ((List<MatrixFrame>)(object)OuterDeckLocalFrames).Count] : ((List<MatrixFrame>)(object)OuterDeckLocalFrames)[nextDeckSpawnFrameIndex]);
		return ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
	}

	public MatrixFrame GetMiddleInnerSpawnGlobalFrame()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = GlobalFrame;
		MatrixFrame val = ((List<MatrixFrame>)(object)InnerDeckLocalFrames)[((List<MatrixFrame>)(object)InnerDeckLocalFrames).Count / 2];
		return ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
	}

	public MatrixFrame GetCaptainSpawnGlobalFrame()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = GlobalFrame;
		MatrixFrame val = ((List<MatrixFrame>)(object)InnerDeckLocalFrames)[((List<MatrixFrame>)(object)InnerDeckLocalFrames).Count - 1];
		return ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
	}

	public NavalState GetNavalState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = GlobalFrame;
		Vec2 position = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Vec2 direction = ((Vec2)(ref position)).Normalized();
		Vec3 linearVelocity = _physics.LinearVelocity;
		float speed = Vec2.DotProduct(((Vec3)(ref linearVelocity)).AsVec2, direction);
		position = ((Vec3)(ref globalFrame.origin)).AsVec2;
		return new NavalState(in position, in direction, speed);
	}

	public bool GetIsThereActiveBridgeTo(MissionShip targetShip)
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)AttachmentMachines)
		{
			if (item.IsShipAttachmentMachineBridged() && item.CurrentAttachment.AttachmentTarget.OwnerShip == targetShip)
			{
				return true;
			}
		}
		foreach (ShipAttachmentMachine item2 in (List<ShipAttachmentMachine>)(object)targetShip.AttachmentMachines)
		{
			if (item2.IsShipAttachmentMachineBridged() && item2.CurrentAttachment.AttachmentTarget.OwnerShip == this)
			{
				return true;
			}
		}
		return false;
	}

	public MBReadOnlyList<MissionShip> GetFullyConnectedShips()
	{
		((List<MissionShip>)(object)_temporaryMissionShipContainer).Clear();
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment != null && item.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected && ((List<MissionShip>)(object)_temporaryMissionShipContainer).IndexOf(item.CurrentAttachment.AttachmentTarget.OwnerShip) < 0)
			{
				((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(item.CurrentAttachment.AttachmentTarget.OwnerShip);
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			if (item2.CurrentAttachment != null && item2.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected && ((List<MissionShip>)(object)_temporaryMissionShipContainer).IndexOf(item2.CurrentAttachment.AttachmentSource.OwnerShip) < 0)
			{
				((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(item2.CurrentAttachment.AttachmentSource.OwnerShip);
			}
		}
		return (MBReadOnlyList<MissionShip>)(object)_temporaryMissionShipContainer;
	}

	protected override void AttachDynamicNavmeshToEntity()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.Current != null && base.NavMeshPrefabName.Length > 0)
		{
			base.DynamicNavmeshIdStart = Mission.Current.GetNextDynamicNavMeshIdStart();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.ImportNavigationMeshPrefab(base.NavMeshPrefabName, base.DynamicNavmeshIdStart);
			AttachDynamicNavmeshFromMachines(_attachmentMachines, _attachmentPointMachines);
		}
	}

	public void EnableBlockers()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).Scene.SetAbilityOfFacesWithId(base.DynamicNavmeshIdStart + 49, true);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Scene scene = ((WeakGameEntity)(ref gameEntity)).Scene;
		int num = base.DynamicNavmeshIdStart + 49;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		scene.SetBlockerDirectionForFacesWithId(num, ((Vec2)(ref asVec)).RotationInRadians);
	}

	public MissionSail CheckHitSails(Agent attackerAgent, Missile missile, in Vec3 missileOldPosition, in Vec3 missilePosition, in MissionWeapon missileWeapon)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (!((MissionObject)this).IsDisabled && (flag || (Team != null && Team.IsEnemyOf(attackerAgent.Team))))
		{
			MissionWeapon val = missileWeapon;
			if (((MissionWeapon)(ref val)).CurrentUsageItem != null)
			{
				val = missileWeapon;
				if (Extensions.HasAnyFlag<WeaponFlags>(((MissionWeapon)(ref val)).CurrentUsageItem.WeaponFlags, (WeaponFlags)32768))
				{
					foreach (MissionSail item in (List<MissionSail>)(object)Sails)
					{
						if (missile.AlreadyHitEntityToIgnore != item.SailEntity && item.GetVisualSailEnabled() && item.IntersectLineSegmentWithSail(in missileOldPosition, in missilePosition))
						{
							return item;
						}
					}
				}
			}
		}
		return null;
	}

	protected override bool OnHit(Agent attackerAgent, int inflictedDamage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage, out float finalDamage, out float fireDamage, out float modifiedFireDamage)
	{
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		reportDamage = false;
		finalDamage = 0f;
		fireDamage = -1f;
		modifiedFireDamage = -1f;
		bool flag = false;
		if (!Mission.Current.DisableDying && (int)Mission.Current.Mode != 1 && (int)Mission.Current.Mode != 9 && !((MissionObject)this).IsDisabled)
		{
			MissionWeapon val = weapon;
			if (((MissionWeapon)(ref val)).CurrentUsageItem != null && (flag || (Team != null && Team.IsEnemyOf(attackerAgent.Team))))
			{
				val = weapon;
				bool num = Extensions.HasAnyFlag<WeaponFlags>(((MissionWeapon)(ref val)).CurrentUsageItem.WeaponFlags, (WeaponFlags)32768);
				bool num2 = ShipsLogic.IsMissileFromShipSiegeEngine(affectorWeaponSlotOrMissileIndex);
				float missileVelocityLengthOnFirstSailHit = ShipsLogic.GetMissileVelocityLengthOnFirstSailHit(affectorWeaponSlotOrMissileIndex);
				bool flag2 = missileVelocityLengthOnFirstSailHit >= 0f;
				if (num2)
				{
					float num3 = inflictedDamage;
					inflictedDamage = MissionGameModels.Current.MissionSiegeEngineCalculationModel.CalculateDamage(attackerAgent, num3);
					finalDamage = DealDamage(inflictedDamage, null, out var _, out var _, out var _, out var _);
					reportDamage = true;
				}
				if (num)
				{
					val = weapon;
					fireDamage = ((MissionWeapon)(ref val)).CurrentUsageItem.FireDamage;
					Vec3 val2 = default(Vec3);
					if (flag2 && Mission.Current.TryGetMissileVelocityFromMissileIndex(affectorWeaponSlotOrMissileIndex, ref val2))
					{
						fireDamage *= ((Vec3)(ref val2)).Length / missileVelocityLengthOnFirstSailHit;
					}
					foreach (MissionSail item in (List<MissionSail>)(object)Sails)
					{
						if (item.GetVisualSailEnabled())
						{
							float inflictedDamage3 = MissionGameModels.Current.AgentApplyDamageModel.CalculateSailFireDamage(attackerAgent, ShipOrigin, fireDamage, true);
							DealDamageToSails(attackerAgent, fireDamage, inflictedDamage3, null);
							break;
						}
					}
					if (FireHitPoints > 0f)
					{
						float num4 = DealFireDamage(fireDamage);
						modifiedFireDamage = num4 - fireDamage;
						WeakGameEntity gameEntity;
						if (num4 > 40f)
						{
							gameEntity = ((ScriptComponentBehavior)this).GameEntity;
							((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfTypeRecursive<ShipBurningSystem>()?.RegisterBlow(impactPosition);
						}
						reportDamage = true;
						if (FireHitPoints <= 0f)
						{
							DealDamageToSails(attackerAgent, SailHitPoints, SailHitPoints, null);
							foreach (UsableMachine item2 in (List<UsableMachine>)(object)MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<UsableMachine>(((ScriptComponentBehavior)this).GameEntity))
							{
								if (!(item2 is ShipAttachmentPointMachine))
								{
									item2.SetIsDisabledForAI(true);
									((ScriptComponentBehavior)item2).SetScriptComponentToTick(((ScriptComponentBehavior)item2).GetTickRequirement());
								}
								foreach (StandingPoint item3 in (List<StandingPoint>)(object)item2.StandingPoints)
								{
									((UsableMissionObject)item3).SetIsDisabledForPlayersSynched(true);
								}
							}
							List<WeakGameEntity> list = new List<WeakGameEntity>();
							gameEntity = ((ScriptComponentBehavior)this).GameEntity;
							((WeakGameEntity)(ref gameEntity)).GetChildrenRecursive(ref list);
							foreach (WeakGameEntity item4 in list)
							{
								WeakGameEntity current2 = item4;
								if (Extensions.HasAnyFlag<BodyFlags>(((WeakGameEntity)(ref current2)).BodyFlag, (BodyFlags)512) || Extensions.HasAnyFlag<BodyFlags>(((WeakGameEntity)(ref current2)).BodyFlag, (BodyFlags)1024) || Extensions.HasAnyFlag<BodyFlags>(((WeakGameEntity)(ref current2)).BodyFlag, (BodyFlags)256))
								{
									((WeakGameEntity)(ref current2)).SetVisibilityExcludeParents(false);
								}
							}
							foreach (ShipAttachmentPointMachine item5 in (List<ShipAttachmentPointMachine>)(object)AttachmentPointMachines)
							{
								((UsableMachine)item5).SetEnemyRangeToStopUsing(0f);
								((UsableMachine)item5).SetIsDisabledForAI(false);
								foreach (StandingPoint item6 in (List<StandingPoint>)(object)((UsableMachine)item5).StandingPoints)
								{
									if (item6 == ((UsableMachine)item5).PilotStandingPoint)
									{
										((UsableMissionObject)item6).LockUserFrames = true;
									}
								}
								foreach (GameEntity item7 in (List<GameEntity>)(object)item5.RampPhysicsList)
								{
									item7.SetVisibilityExcludeParents(true);
								}
							}
							ShipsLogic.OnShipBurned(this);
							ShipOrder.StopUsingMachines(formationLeaving: false);
							IsShipOrderActive = false;
							gameEntity = ((ScriptComponentBehavior)this).GameEntity;
							((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfTypeRecursive<ShipBurningSystem>()?.StartFire();
							SetController(ShipControllerType.None);
						}
					}
				}
			}
		}
		ShipsLogic.OnShipHit(this, attackerAgent, (int)finalDamage, impactPosition, impactDirection, in weapon, affectorWeaponSlotOrMissileIndex);
		return true;
	}

	public float DealFireDamage(float fireDamage)
	{
		float num = MissionGameModels.Current.AgentApplyDamageModel.CalculateHullFireDamage(fireDamage, ShipOrigin);
		FireHitPoints -= num;
		return num;
	}

	protected override void OnTick(float dt)
	{
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Invalid comparison between Unknown and I4
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Invalid comparison between Unknown and I4
		_isCachedWorldPositionOnDeckDirty = true;
		WeakGameEntity gameEntity;
		if (!_isRemoved)
		{
			if (Mission.Current.IsDeploymentFinished)
			{
				if (_autoUpdateController)
				{
					UpdateController();
				}
				if (IsShipOrderActive)
				{
					ShipOrder.Tick();
				}
				else if (IsClimbingMachineStandAloneTickingActive)
				{
					ShipOrder.TickClimbingMachines();
				}
			}
			if (HasController)
			{
				_inputRecord = Controller.Update(dt);
			}
			if (HasCustomSailSetting)
			{
				_inputRecord.SetSail(_customSailSetting);
			}
			if (_inputRecord.Sail != SailInput.Raised && _foldSailsOnBridgeConnection && HasThrownOrActiveBridgeConnections())
			{
				_inputRecord.SetSail(SailInput.Raised);
			}
			HandleCapsizing();
			float num = MathF.Max(_physics.PhysicsBoundingBoxWithoutChildren.max.z, _physics.PhysicsBoundingBoxSizeWithoutChildren.y * 0.5f);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			if (Physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sinking && globalPosition.z + num < Mission.Current.Scene.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, true, false))
			{
				SetSinkingState(NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sunk);
				ShipSailState = SailState.Destroyed;
				if (SailBurningSoundEvent.IsPlaying())
				{
					SailBurningSoundEvent.Stop();
				}
				ShipsLogic.OnShipSunk(this);
			}
			bool flag = IsShipUpsideDown();
			if (flag != IsShipNavmeshDisabled)
			{
				SetAbilityOfShipNavmeshFaces(!flag);
				IsShipNavmeshDisabled = flag;
				ShipOrder.ManageShipDetachments();
			}
			UpdateSailBurningSoundPosition();
			if (ShipSailState == SailState.Burning)
			{
				bool flag2 = true;
				foreach (MissionSail item in (List<MissionSail>)(object)Sails)
				{
					if (!item.IsBurningFinished())
					{
						flag2 = false;
					}
				}
				if (flag2)
				{
					ShipSailState = SailState.Destroyed;
					if (SailBurningSoundEvent.IsPlaying())
					{
						SailBurningSoundEvent.Stop();
					}
					ShipsLogic.OnSailsDead(this);
				}
			}
			if (FireHitPoints <= 0f && BurntHullDamageTotal < MaxHealth * 0.5f && !IsSinking && Mission.Current.CurrentTime > _nextPermanentBurnDamageTime && !Mission.Current.DisableDying && (int)Mission.Current.Mode != 1 && (int)Mission.Current.Mode != 9)
			{
				BurntHullDamageTotal += DealDamage(MaxHealth * 0.03f, null, out var _, out var _, out var _, out var _);
				_nextPermanentBurnDamageTime = Mission.Current.CurrentTime + 1f;
			}
			if (FireHitPoints > 0f && FireHitPoints < MaxFireHealth && Mission.Current.CurrentTime > _nextFireHitPointRestoreTime)
			{
				FireHitPoints += MaxFireHealth * 0.005f;
				if (FireHitPoints > MaxFireHealth)
				{
					FireHitPoints = MaxFireHealth;
				}
				_nextFireHitPointRestoreTime = Mission.Current.CurrentTime + 1f;
			}
			if (IsDisconnectionBlocked())
			{
				bool flag3 = false;
				foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)AttachmentPointMachines)
				{
					if (item2.IsShipAttachmentPointConnectedToEnemy())
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					ResetDisconnectionBlock();
				}
			}
			HandleQueuedShipCollisions();
		}
		_actuators.Update(dt);
		if (_localBoundingBoxCacheInvalid)
		{
			ComputeStaticLocalBoundingBox();
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).RecomputeBoundingBox();
			_localBoundingBoxCacheInvalid = false;
		}
	}

	protected override void OnParallelFixedTick(float fixedDt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame listenerFrame = SoundManager.GetListenerFrame();
		ref Vec3 origin = ref listenerFrame.origin;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		ShouldUpdateSoundPos = ((Vec3)(ref origin)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().origin) < 10000f;
		ShipActuatorRecord actuatorInput = _inputProcessor.OnParallelFixedTick(fixedDt, in _inputRecord);
		ShipForceRecord record = _actuators.OnParallelFixedTick(fixedDt, in actuatorInput);
		_physics.SetShipForceRecord(in record);
	}

	protected override void OnPhysicsCollision(ref PhysicsContact contactPairList, WeakGameEntity entity0, WeakGameEntity entity1)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected I4, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Expected I4, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected I4, but got Unknown
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Expected I4, but got Unknown
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Invalid comparison between Unknown and I4
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Invalid comparison between Unknown and I4
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Invalid comparison between Unknown and I4
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0502: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Invalid comparison between Unknown and I4
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_050e: Invalid comparison between Unknown and I4
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_055b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0573: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_060b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0621: Unknown result type (might be due to invalid IL or missing references)
		//IL_0626: Unknown result type (might be due to invalid IL or missing references)
		//IL_062b: Unknown result type (might be due to invalid IL or missing references)
		//IL_062d: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_0637: Unknown result type (might be due to invalid IL or missing references)
		//IL_063c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0643: Unknown result type (might be due to invalid IL or missing references)
		//IL_0648: Unknown result type (might be due to invalid IL or missing references)
		//IL_0652: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		//IL_0659: Unknown result type (might be due to invalid IL or missing references)
		//IL_065e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		//IL_066a: Unknown result type (might be due to invalid IL or missing references)
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0673: Unknown result type (might be due to invalid IL or missing references)
		//IL_0678: Unknown result type (might be due to invalid IL or missing references)
		//IL_067a: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0681: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		SoundEvent val = null;
		Vec3 val2 = default(Vec3);
		((Vec3)(ref val2))._002Ector(0f, 0f, 0f, -1f);
		Vec3 val3 = default(Vec3);
		((Vec3)(ref val3))._002Ector(0f, 0f, 0f, -1f);
		int num = 0;
		int num2 = -1;
		bool flag = false;
		StackArray3Int val4 = default(StackArray3Int);
		Vec3 val5 = Vec3.Zero;
		MissionShip missionShip = ((entity1 != (GameEntity)null) ? (((WeakGameEntity)(ref entity1)).GetFirstScriptWithNameHash(MissionShipScriptNameHash) as MissionShip) : null);
		for (int i = 0; i < contactPairList.NumberOfContactPairs; i++)
		{
			int num3 = (int)((PhysicsContact)(ref contactPairList))[i].ContactEventType;
			int num4 = ((StackArray3Int)(ref val4))[num3];
			((StackArray3Int)(ref val4))[num3] = num4 + 1;
			for (int j = 0; j < ((PhysicsContact)(ref contactPairList))[i].NumberOfContacts; j++)
			{
				num++;
				Vec3 val6 = val2;
				PhysicsContactPair val7 = ((PhysicsContact)(ref contactPairList))[i];
				val2 = val6 + ((PhysicsContactPair)(ref val7))[j].Position;
				Vec3 val8 = val3;
				val7 = ((PhysicsContact)(ref contactPairList))[i];
				val3 = val8 + ((PhysicsContactPair)(ref val7))[j].Normal;
				Vec3 val9 = val5;
				val7 = ((PhysicsContact)(ref contactPairList))[i];
				val5 = val9 + ((PhysicsContactPair)(ref val7))[j].Impulse;
			}
		}
		int num5 = -1;
		for (int k = 0; k < _currentCollisionStatesToShips.Count; k++)
		{
			if (_currentCollisionStatesToShips[k].Item1 == entity1)
			{
				num5 = k;
				break;
			}
		}
		if (contactPairList.NumberOfContactPairs > 0)
		{
			PhysicsEventType val10 = (PhysicsEventType)((num5 < 0) ? 2 : ((int)_currentCollisionStatesToShips[num5].Item2));
			switch ((int)val10)
			{
			case 0:
				if (((StackArray3Int)(ref val4))[1] > 0)
				{
					val10 = (PhysicsEventType)1;
				}
				else if (((StackArray3Int)(ref val4))[0] == 0 && ((StackArray3Int)(ref val4))[2] > 0)
				{
					val10 = (PhysicsEventType)2;
				}
				break;
			case 1:
				if (((StackArray3Int)(ref val4))[0] == 0 && ((StackArray3Int)(ref val4))[1] == 0)
				{
					val10 = (PhysicsEventType)2;
				}
				break;
			case 2:
				if (((StackArray3Int)(ref val4))[0] > 0 || ((StackArray3Int)(ref val4))[1] > 0)
				{
					val10 = (PhysicsEventType)0;
				}
				break;
			}
			if (num5 >= 0)
			{
				_currentCollisionStatesToShips[num5] = (_currentCollisionStatesToShips[num5].Item1, val10);
			}
			else
			{
				_currentCollisionStatesToShips.Add((GameEntity.CreateFromWeakEntity(entity1), val10));
			}
			flag = (int)val10 != 2 && missionShip != null;
		}
		val2 /= (float)num;
		val3 /= (float)num;
		StackArray3Int val11 = default(StackArray3Int);
		for (int l = 0; l < _currentCollisionStatesToShips.Count; l++)
		{
			int num4 = (int)_currentCollisionStatesToShips[l].Item2;
			int num3 = ((StackArray3Int)(ref val11))[num4];
			((StackArray3Int)(ref val11))[num4] = num3 + 1;
		}
		PhysicsEventType item;
		if (num5 != -1 && missionShip != null)
		{
			item = _currentCollisionStatesToShips[num5].Item2;
			if ((int)item != 1)
			{
				if ((int)item == 2)
				{
					RemoveDetanglingShip(missionShip);
				}
			}
			else
			{
				AddDetanglingShip(missionShip, val2);
			}
		}
		PhysicsEventType currentCollisionState = _currentCollisionState;
		item = _currentCollisionState;
		switch ((int)item)
		{
		case 0:
			if (((StackArray3Int)(ref val11))[1] > 0)
			{
				_currentCollisionState = (PhysicsEventType)1;
			}
			else if (((StackArray3Int)(ref val11))[0] == 0 && ((StackArray3Int)(ref val11))[2] > 0)
			{
				_currentCollisionState = (PhysicsEventType)2;
			}
			break;
		case 1:
			if (((StackArray3Int)(ref val11))[0] == 0 && ((StackArray3Int)(ref val11))[1] == 0)
			{
				_currentCollisionState = (PhysicsEventType)2;
			}
			break;
		case 2:
			if (((StackArray3Int)(ref val11))[0] > 0 || ((StackArray3Int)(ref val11))[1] > 0)
			{
				_currentCollisionState = (PhysicsEventType)0;
			}
			break;
		}
		WeakGameEntity gameEntity;
		if (missionShip != null)
		{
			for (int m = 0; m < ((List<(int, SoundEvent)>)(object)_scrapeSoundEvents).Count; m++)
			{
				if (((List<(int, SoundEvent)>)(object)_scrapeSoundEvents)[m].Item1 == missionShip.Index)
				{
					val = ((List<(int, SoundEvent)>)(object)_scrapeSoundEvents)[m].Item2;
					num2 = m;
					break;
				}
			}
			if (flag)
			{
				if (val == null)
				{
					int scrapeSoundEventID = _scrapeSoundEventID;
					gameEntity = ((ScriptComponentBehavior)this).GameEntity;
					val = SoundEvent.CreateEvent(scrapeSoundEventID, ((WeakGameEntity)(ref gameEntity)).Scene);
					((List<(int, SoundEvent)>)(object)_scrapeSoundEvents).Add((missionShip.Index, val));
					((List<(int, SoundEvent)>)(object)missionShip._scrapeSoundEvents).Add((Index, val));
				}
				if (!val.IsPlaying())
				{
					val.Play();
				}
				Vec3 val12 = ((Vec3)(ref val3)).CrossProductWithUp();
				float num6 = Vec3.DotProduct(val12, Physics.LinearVelocity);
				float num7 = Vec3.DotProduct(val12, missionShip.Physics.LinearVelocity);
				float num8 = MathF.Min(MathF.Abs(num6 - num7) / 10f, 1f);
				val.SetParameter("ForceContinuous", num8);
				val.SetPosition(val2);
				if (!IsPlayerControlled && !missionShip.IsPlayerControlled)
				{
					val.SetParameter("VibrationSend", 0f);
				}
			}
			else
			{
				if (val != null && val.IsPlaying())
				{
					val.Stop();
					val = null;
				}
				if (num2 != -1 && num2 < ((List<(int, SoundEvent)>)(object)_scrapeSoundEvents).Count)
				{
					((List<(int, SoundEvent)>)(object)_scrapeSoundEvents).RemoveAt(num2);
				}
			}
		}
		if (num <= 0 || ShipsLogic == null)
		{
			return;
		}
		bool flag2 = (int)currentCollisionState == 2 && (int)_currentCollisionState == 0;
		if (flag2 && missionShip != null && CanDealDamage(missionShip))
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			gameEntity = ((ScriptComponentBehavior)missionShip).GameEntity;
			MatrixFrame bodyWorldTransform2 = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			BoundingBox physicsBoundingBoxWithoutChildren = Physics.PhysicsBoundingBoxWithoutChildren;
			Vec3 val13 = ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref physicsBoundingBoxWithoutChildren.center);
			Vec3 val14 = val2 - val13;
			val14.z = 0f;
			((Vec3)(ref val14)).Normalize();
			float num9 = Vec3.DotProduct(val14, bodyWorldTransform.rotation.f);
			if (num9 > 0f && MathF.Acos(num9) * (180f / MathF.PI) < MissionShipObject.BowAngleLimitFromCenterline)
			{
				Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)this).GameEntity, val2);
				Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody2 = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)missionShip).GameEntity, val2);
				_ = linearVelocityAtGlobalPointForEntityWithDynamicBody - linearVelocityAtGlobalPointForEntityWithDynamicBody2;
				Vec3 val15 = bodyWorldTransform.origin + Vec3.DotProduct(val2 - bodyWorldTransform.origin, bodyWorldTransform.rotation.f) * bodyWorldTransform.rotation.f;
				Vec3 val16 = bodyWorldTransform2.origin + Vec3.DotProduct(val2 - bodyWorldTransform2.origin, bodyWorldTransform2.rotation.f) * bodyWorldTransform2.rotation.f - val15;
				Vec3 val17 = ((Vec3)(ref val16)).NormalizedCopy();
				float num10 = Vec3.DotProduct(linearVelocityAtGlobalPointForEntityWithDynamicBody - linearVelocityAtGlobalPointForEntityWithDynamicBody2, val17);
				if (num10 >= 3f)
				{
					float num11 = 12f * (float)Math.Sqrt(Physics.Mass / 500f) * 0.8f * num10;
					missionShip.QueueShipCollision(this, val2, num11);
					QueueShipCollision(missionShip, val2, num11 * 0.2f);
					UpdateDamageCooldown(missionShip);
				}
			}
		}
		ShipsLogic.OnShipCollision(this, entity1, val2, val5, flag2);
	}

	private void HandleQueuedShipCollisions()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		ShipCollisionData result;
		while (_shipCollisionData.TryDequeue(out result))
		{
			DealCollisionDamage(result.CollidingShip, isRamDamage: false, result.ContactPosAvg, result.Damage);
		}
	}

	private void QueueShipCollision(MissionShip collidingShip, Vec3 contactPosAvg, float damage)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_shipCollisionData.Enqueue(new ShipCollisionData(collidingShip, contactPosAvg, damage));
	}

	public bool CanDealDamage(MissionShip targetShip)
	{
		float currentTime = Mission.Current.CurrentTime;
		if (_shipDamageCooldowns.TryGetValue(targetShip, out var value))
		{
			return currentTime - value >= 2f;
		}
		return true;
	}

	public void UpdateDamageCooldown(MissionShip targetShip)
	{
		float currentTime = Mission.Current.CurrentTime;
		_shipDamageCooldowns[targetShip] = currentTime;
	}

	protected override bool OnCheckForProblems()
	{
		InitializeLists(isForCheckingForProblems: true);
		return false;
	}

	internal void InitForMission(int shipIndex, ulong shipUniqueBitwiseID, ShipAssignment shipAssignment, NavalShipsLogic shipsLogic)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsInitialized)
		{
			ShipsLogic = shipsLogic;
			ValidateShipAndDescendantEntitiesAndBoundingBoxes();
			Index = shipIndex;
			MaxShipIndex = MathF.Max(Index, MaxShipIndex);
			_shields = MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<ShipShieldComponent>(((ScriptComponentBehavior)this).GameEntity);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.SetFixedTickCallbackActive(true);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.SetOnCollisionFilterCallbackActive(true);
			_missionShipObject = shipAssignment.MissionShipObject;
			ShipOrigin = shipAssignment.ShipOrigin;
			ShipSailState = ((!(ShipOrigin.SailHitPoints > 250f)) ? SailState.Destroyed : SailState.Intact);
			FireHitPoints = ShipOrigin.MaxFireHitPoints;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			GameEntity val = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("rally_point")[0]);
			MatrixFrame globalFrame = GlobalFrame;
			MatrixFrame globalFrame2 = val.GetGlobalFrame();
			RallyFrame = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalFrame2);
			LoadSpawnPoints();
			LoadShipBanners();
			_capsizeDamageTimer = new Timer(Mission.Current.CurrentTime, 0.5f, true);
			MBList<ClimbingMachine> val2 = MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<ClimbingMachine>(((ScriptComponentBehavior)this).GameEntity);
			ClimbingMachineDetachment = new ClimbingMachineDetachment(ref val2);
			Team team = Mission.GetTeam(shipAssignment.TeamSide);
			Formation = team.GetFormation(shipAssignment.FormationIndex);
			_inputRecord = ShipInputRecord.None();
			Formation.OnUnitRemoved += OnFormationUnitRemoved;
			SetController(ShipControllerType.AI);
			_inputProcessor = new ShipInputProcessor(this);
			_actuators = new NavalDLC.Missions.ShipActuators.ShipActuators(this);
			foreach (MissionSail item2 in (List<MissionSail>)(object)_actuators.Sails)
			{
				item2.ForceFold();
			}
			InitializeNavalPhysics();
			_visitedMissionShips = new HashSet<MissionShip>();
			InitializeDetanglingShipInformation();
			InitializeLocalPhysicsBoundingXYPlane();
			_physicsBoundingBoxXYPlaneVertices = (Vec2[])(object)new Vec2[4];
			_criticalZoneVertices = (Vec2[])(object)new Vec2[4];
			float element = MaxPartialHealth - (MaxHealth - HitPoints) / 6f;
			_partialHitPoints = Enumerable.Repeat(element, 6).ToArray();
			InitializePartialDurabilities();
			_moraleInteractionLogic = Mission.Current.GetMissionBehavior<NavalAgentMoraleInteractionLogic>();
			ShipsLogic.ShipSpawnedEvent += OnShipSpawned;
			ShipsLogic.ShipTransferredToFormationEvent += OnShipTransferred;
			ShipsLogic.ShipRemovedEvent += OnShipRemoved;
			ShipOrder = new ShipOrder(this, Formation);
			ResetFormationPositioning();
			_scrapeSoundEvents = new MBList<(int, SoundEvent)>();
			int eventIdFromString = SoundEvent.GetEventIdFromString("event:/mission/ambient/detail/fire/fire_big");
			SailBurningSoundEvent = SoundEvent.CreateEvent(eventIdFromString, Mission.Current.Scene);
			UpdateSailBurningSoundPosition();
			RangedSiegeWeapon shipSiegeWeapon = ShipSiegeWeapon;
			if (shipSiegeWeapon != null)
			{
				((SiegeWeapon)shipSiegeWeapon).SetForcedUse(false);
			}
			InitializeShipBoundingBox();
			_shipEventListeners = new MBList<IShipEventListener>();
			foreach (ScriptComponentBehavior item3 in (List<ScriptComponentBehavior>)(object)MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<ScriptComponentBehavior>(((ScriptComponentBehavior)this).GameEntity))
			{
				if (item3 is IShipEventListener item)
				{
					((List<IShipEventListener>)(object)_shipEventListeners).Add(item);
				}
			}
			ShipUniqueBitwiseID = shipUniqueBitwiseID;
			ShipIslandCombinedID = ShipUniqueBitwiseID;
			Formation.OnUnitAttached += OnUnitAttached;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (!((WeakGameEntity)(ref gameEntity)).IsInEditorScene())
			{
				ClearFloaterVolumes();
				WeakGameEntity firstChildEntityWithName = MBExtensions.GetFirstChildEntityWithName(((ScriptComponentBehavior)this).GameEntity, "knobs_holder");
				if (((WeakGameEntity)(ref firstChildEntityWithName)).IsValid)
				{
					((WeakGameEntity)(ref firstChildEntityWithName)).SetEntityFlags((EntityFlags)(((WeakGameEntity)(ref firstChildEntityWithName)).EntityFlags | 0x20000000));
				}
				WeakGameEntity firstChildEntityWithName2 = MBExtensions.GetFirstChildEntityWithName(((ScriptComponentBehavior)this).GameEntity, "brazier_holder");
				if (((WeakGameEntity)(ref firstChildEntityWithName2)).IsValid)
				{
					((WeakGameEntity)(ref firstChildEntityWithName2)).SetEntityFlags((EntityFlags)(((WeakGameEntity)(ref firstChildEntityWithName2)).EntityFlags | 0x20000000));
				}
				List<WeakGameEntity> list = new List<WeakGameEntity>();
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).GetChildrenRecursive(ref list);
				foreach (WeakGameEntity item4 in list)
				{
					WeakGameEntity current = item4;
					((WeakGameEntity)(ref current)).SetForceDecalsToRender(true);
					((WeakGameEntity)(ref current)).SetForceNotAffectedBySeason(true);
				}
			}
			_anyActiveFormationTroopOnShip.Expire();
		}
		else
		{
			Debug.FailedAssert("The ship is already initialized", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\Objects\\MissionShip.cs", "InitForMission", 3074);
		}
	}

	private void OnFormationUnitRemoved(Formation formation, Agent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (BattleSide != Mission.Current.PlayerTeam.Side && formation.CountOfUnits == 0)
		{
			((UsableMachine)ShipControllerMachine).PilotStandingPoint.SetUsableByPlayerOnly();
		}
	}

	private void InitializeNavalPhysics()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		ShipPhysicsReference physicsReference = _missionShipObject.PhysicsReference;
		NavalDLC.Missions.NavalPhysics.NavalPhysics.NavalPhysicsParameters physicsParameters = new NavalDLC.Missions.NavalPhysics.NavalPhysics.NavalPhysicsParameters
		{
			OverrideMass = _missionShipObject.Mass,
			MassMultiplier = 1f + ((ShipOrigin != null) ? ShipOrigin.ShipWeightFactor : 0f),
			MomentOfInertiaMultiplier = _missionShipObject.MomentOfInertiaMultiplier,
			FloatingForceMultiplier = _missionShipObject.FloatingForceMultiplier,
			MaximumSubmergedVolumeRatio = _missionShipObject.MaximumSubmergedVolumeRatio,
			ForwardDragMultiplier = 1f + ((ShipOrigin != null) ? ShipOrigin.ForwardDragFactor : 0f),
			LinearFrictionMultiplier = _missionShipObject.LinearFrictionMultiplier,
			AngularFrictionMultiplier = _missionShipObject.AngularFrictionMultiplier,
			TorqueMultiplierOfLateralBuoyantForces = _missionShipObject.TorqueMultiplierOfLateralBuoyantForces,
			TorqueMultiplierOfVerticalBuoyantForces = _missionShipObject.TorqueMultiplierOfVerticalBuoyantForces,
			UpSideDownFrictionMultiplier = 3f,
			MaxLinearSpeedForLateralDragCenterShift = _missionShipObject.MaxLinearSpeed,
			MaxLateralDragShift = _missionShipObject.MaxLateralDragShift,
			LateralDragShiftCriticalAngle = _missionShipObject.LateralDragShiftCriticalAngle,
			StepAgentWeightMultiplier = 2f,
			MakeAgentsStepToEntityEvenUnderWater = true
		};
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_physics = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<NavalDLC.Missions.NavalPhysics.NavalPhysics>();
		_physics.Initialize(physicsParameters, physicsReference);
	}

	internal void OnShipSpawned(MissionShip spawnedShip)
	{
		foreach (IShipEventListener item in (List<IShipEventListener>)(object)_shipEventListeners)
		{
			item.OnShipSpawned(spawnedShip);
		}
	}

	internal void OnShipRemoved(MissionShip removedShip)
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)AttachmentMachines)
		{
			if (item.CurrentAttachment != null)
			{
				item.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
				item.CurrentAttachment.Destroy();
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)AttachmentPointMachines)
		{
			if (item2.CurrentAttachment != null)
			{
				item2.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
				item2.CurrentAttachment.Destroy();
			}
		}
		foreach (IShipEventListener item3 in (List<IShipEventListener>)(object)_shipEventListeners)
		{
			item3.OnShipRemoved(removedShip);
		}
		if (IsAIControlled)
		{
			AIController.RemoveShipFromCollisionIgnoreList(removedShip);
		}
		_actuators.OnShipRemoved(removedShip);
	}

	protected override void OnTickParallel(float dt)
	{
		_actuators.OnTickParallel(dt);
		NavalDLC.Missions.NavalPhysics.NavalPhysics physics = _physics;
		Formation formation = Formation;
		physics.SetContinuousDriftSpeed((formation != null && formation.CountOfUnits > 0) ? 0f : 1f);
	}

	private void ClearFloaterVolumes()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = WeakGameEntity.Invalid;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child;
			if (((WeakGameEntity)(ref current)).Name == "floater_volume_holder")
			{
				val = current;
				break;
			}
		}
		if (((WeakGameEntity)(ref val)).IsValid)
		{
			((WeakGameEntity)(ref val)).RemoveAllChildren();
		}
	}

	internal void SetRemoved(bool value)
	{
		_isRemoved = value;
	}

	internal void OnShipTransferred(MissionShip ship, Formation oldFormation)
	{
		foreach (IShipEventListener item in (List<IShipEventListener>)(object)_shipEventListeners)
		{
			item.OnShipTransferred(ship, oldFormation);
		}
	}

	public IDWAAgentDelegate CreateDWAAgent(in DWASimulatorParameters parameters)
	{
		if (_dwaAgentDelegate == null)
		{
			_dwaAgentDelegate = new ShipDWAAgentDelegate(this, in parameters);
		}
		else
		{
			((IDWAAgentDelegate)_dwaAgentDelegate).SetParameters(in parameters);
		}
		return _dwaAgentDelegate;
	}

	protected override void OnRemoved(int removeReason)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		((MissionObject)this).OnRemoved(removeReason);
		ShipsLogic.ShipSpawnedEvent -= OnShipSpawned;
		ShipsLogic.ShipRemovedEvent -= OnShipRemoved;
		ShipsLogic.ShipTransferredToFormationEvent -= OnShipTransferred;
		ShipOrder.OnOwnerShipRemoved();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfTypeRecursive<ShipWaterEffects>()?.DeregisterWaterMeshMaterials();
	}

	public void MoveShipToTheTargetWithDirection(MatrixFrame currentFrame, Vec2 targetPosition, Vec2 targetDirection, float maxAcceleration, float maxAngularAcceleration, float fixedDt)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		float num = MathF.Atan2(targetDirection.y, targetDirection.x);
		Vec3 origin = currentFrame.origin;
		Vec3 linearVelocity = Physics.LinearVelocity;
		Vec3 angularVelocity = Physics.AngularVelocity;
		float mass = Physics.Mass;
		float num2 = MathF.Atan2(currentFrame.rotation.f.y, currentFrame.rotation.f.x);
		Vec2 val = (targetPosition - ((Vec3)(ref origin)).AsVec2) / fixedDt;
		float num3 = MBMath.WrapAngle(num - num2) / fixedDt;
		Vec2 val2 = (val - ((Vec3)(ref linearVelocity)).AsVec2) / fixedDt;
		((Vec2)(ref val2)).ClampMagnitude(0f, maxAcceleration);
		float num4 = (num3 - angularVelocity.z) / fixedDt;
		float num5 = MathF.Sign(num4);
		float num6 = MathF.Clamp(num4 * num5, 0f, maxAngularAcceleration);
		num4 = num5 * num6;
		Vec2 val3 = val2 * mass;
		Physics.ApplyForceToDynamicBody(((Vec2)(ref val3)).ToVec3(0f), (ForceMode)0);
		Physics.ApplyTorque(new Vec3(0f, 0f, num4, -1f), (ForceMode)3);
	}

	internal void UpdateController()
	{
		if (IsSinking)
		{
			return;
		}
		if (((UsableMachine)ShipControllerMachine).PilotAgent != null && ((UsableMachine)ShipControllerMachine).PilotAgent.IsPlayerControlled)
		{
			if (!IsPlayerControlled)
			{
				if (Formation.IsAIControlled)
				{
					Formation.SetControlledByAI(false, false);
				}
				SetController(ShipControllerType.Player);
				PlayerController.SetInput(in _inputRecord);
			}
			else if (Formation.IsAIControlled)
			{
				((UsableMachine)ShipControllerMachine).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				SetController(ShipControllerType.AI);
			}
		}
		else if (IsPlayerShip)
		{
			if (Formation.IsAIControlled && !HasController)
			{
				SetController(ShipControllerType.AI);
			}
			else if (!Formation.IsAIControlled)
			{
				if (IsAIControlled)
				{
					ShipOrder.SetShipStopOrder();
					SetController(ShipControllerType.None);
					SetInputRecord(ShipInputRecord.Stop());
				}
				else if (HasController)
				{
					SetController(ShipControllerType.None);
				}
			}
		}
		else if (!IsAIControlled)
		{
			SetController(ShipControllerType.AI);
		}
	}

	private void HandleCapsizing()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Invalid comparison between Unknown and I4
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Invalid comparison between Unknown and I4
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		bool flag = Vec3.DotProduct(((WeakGameEntity)(ref gameEntity)).GetLocalFrame().rotation.u, Vec3.Up) < -0.5f;
		if (_isCapsized != flag)
		{
			_isCapsized = flag;
			if (flag)
			{
				_capsizeDamageTimer.Reset(Mission.Current.CurrentTime);
			}
		}
		if (_isCapsized && _capsizeDamageTimer.Check(Mission.Current.CurrentTime) && !Mission.Current.DisableDying && (int)Mission.Current.Mode != 1 && (int)Mission.Current.Mode != 9)
		{
			DealDamage(MaxHealth * 0.05f, null, out var _, out var _, out var _, out var _);
		}
	}

	private void ValidateShipAndDescendantEntitiesAndBoundingBoxes()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).ValidateBoundingBox();
	}

	private void OnUnitAttached(Formation formation, Agent agent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)((MovementOrder)formation.GetReadonlyMovementOrderReference()).OrderEnum == 7)
		{
			SetPositioningOrdersToRallyPoint(applyToPlayerFormation: true, playersOrder: false);
		}
	}

	private void ComputeStaticLocalBoundingBox()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		((BoundingBox)(ref _localBoundingBoxCached)).BeginRelaxation();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child;
			((WeakGameEntity)(ref current)).ValidateBoundingBox();
			BoundingBox localBoundingBox = ((WeakGameEntity)(ref current)).GetLocalBoundingBox();
			((BoundingBox)(ref _localBoundingBoxCached)).RelaxWithBoundingBox(localBoundingBox);
		}
		_localBoundingBoxCacheInvalid = false;
	}

	private void InitializePartialDurabilities()
	{
		for (int i = 0; i < 6; i++)
		{
			_physics.SetTargetDurabilityOfPart(i, _partialHitPoints[i] / MaxPartialHealth);
		}
	}

	private void InitializeShipBoundingBox()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		foreach (ShipOarMachine item in (List<ShipOarMachine>)(object)_leftSideShipOarMachines)
		{
			item.ArrangeOarBoundingBox();
		}
		foreach (ShipOarMachine item2 in (List<ShipOarMachine>)(object)_rightSideShipOarMachines)
		{
			item2.ArrangeOarBoundingBox();
		}
		foreach (ShipUnmannedOar item3 in (List<ShipUnmannedOar>)(object)_shipUnmannedOars)
		{
			item3.ArrangeOarBoundingBox();
		}
		foreach (MissionSail item4 in (List<MissionSail>)(object)_actuators.Sails)
		{
			List<GameEntity> list = new List<GameEntity>();
			item4.SailEntity.GetChildrenRecursive(ref list);
			foreach (GameEntity item5 in list)
			{
				item5.EntityFlags = (EntityFlags)(item5.EntityFlags | 0x1000);
			}
			item4.SailEntity.SetHasCustomBoundingBoxValidationSystem(true);
			item4.SailEntity.SetBoundingboxDirty();
		}
	}

	private void RecalculateShipIsland()
	{
		ShipIslandCombinedID = 0uL;
		ulong islandMask = 0uL;
		BuildIslandMaskRecursive(this, ref islandMask);
		ulong visitedShipsMask = 0uL;
		ApplyIslandMaskRecursive(this, islandMask, ref visitedShipsMask);
	}

	private void BuildIslandMaskRecursive(MissionShip ship, ref ulong islandMask)
	{
		ulong shipUniqueBitwiseID = ship.ShipUniqueBitwiseID;
		if ((islandMask & shipUniqueBitwiseID) != 0L)
		{
			return;
		}
		islandMask |= shipUniqueBitwiseID;
		foreach (MissionShip item in (List<MissionShip>)(object)ship.GetNavmeshConnectedShips())
		{
			BuildIslandMaskRecursive(item, ref islandMask);
		}
	}

	private void ApplyIslandMaskRecursive(MissionShip ship, ulong finalIslandMask, ref ulong visitedShipsMask)
	{
		ulong shipUniqueBitwiseID = ship.ShipUniqueBitwiseID;
		if ((visitedShipsMask & shipUniqueBitwiseID) != 0L)
		{
			return;
		}
		visitedShipsMask |= shipUniqueBitwiseID;
		ship.ShipIslandCombinedID = finalIslandMask;
		foreach (MissionShip item in (List<MissionShip>)(object)ship.GetNavmeshConnectedShips())
		{
			ApplyIslandMaskRecursive(item, finalIslandMask, ref visitedShipsMask);
		}
	}

	private bool IsShipUpsideDown()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		return ((WeakGameEntity)(ref gameEntity)).GetLocalFrame().rotation.u.z <= 0.35f;
	}

	private void SetAbilityOfShipNavmeshFaces(bool enable)
	{
		Mission.Current.Scene.SetAbilityOfFacesWithId(base.DynamicNavmeshIdStart, enable);
		foreach (ShipAttachmentPointMachine item in (List<ShipAttachmentPointMachine>)(object)_attachmentPointMachines)
		{
			int num = base.DynamicNavmeshIdStart + item.RelatedShipNavmeshOffset;
			Mission.Current.Scene.SetAbilityOfFacesWithId(num, enable);
		}
	}

	private void AttachDynamicNavmeshFromMachines(MBList<ShipAttachmentMachine> shipAttachmentMachines, MBList<ShipAttachmentPointMachine> shipAttachmentPointMachines)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		((MissionObject)this).SetAbilityOfFaces(((WeakGameEntity)(ref val)).IsValid && GameEntityPhysicsExtensions.GetPhysicsState(((ScriptComponentBehavior)this).GameEntity));
		for (int i = 0; i < ((List<ShipAttachmentPointMachine>)(object)shipAttachmentPointMachines).Count; i++)
		{
			int num = base.DynamicNavmeshIdStart + ((List<ShipAttachmentPointMachine>)(object)shipAttachmentPointMachines)[i].RelatedShipNavmeshOffset;
			val = ((MissionObject)this).GetEntityToAttachNavMeshFaces();
			((WeakGameEntity)(ref val)).AttachNavigationMeshFaces(num, false, false, false, false, false);
		}
		val = ((MissionObject)this).GetEntityToAttachNavMeshFaces();
		((WeakGameEntity)(ref val)).AttachNavigationMeshFaces(base.DynamicNavmeshIdStart, false, false, false, false, true);
	}

	private bool CheckAttachedNavmeshSanity(bool isEditorMode)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		WeakGameEntity gameEntity;
		if (isEditorMode)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.ClearNavMesh();
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Scene scene = ((WeakGameEntity)(ref gameEntity)).Scene;
			string navMeshPrefabName = base.NavMeshPrefabName;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			scene.ImportNavigationMeshPrefabWithFrame(navMeshPrefabName, ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame());
		}
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		MBList<ShipAttachmentMachine> val = new MBList<ShipAttachmentMachine>();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetChildrenRecursive(ref list);
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current = item;
			foreach (ShipAttachmentMachine scriptComponent in ((WeakGameEntity)(ref current)).GetScriptComponents<ShipAttachmentMachine>())
			{
				((List<ShipAttachmentMachine>)(object)val).Add(scriptComponent);
			}
		}
		MBList<ShipAttachmentPointMachine> val2 = new MBList<ShipAttachmentPointMachine>();
		foreach (WeakGameEntity item2 in list)
		{
			WeakGameEntity current3 = item2;
			foreach (ShipAttachmentPointMachine scriptComponent2 in ((WeakGameEntity)(ref current3)).GetScriptComponents<ShipAttachmentPointMachine>())
			{
				((List<ShipAttachmentPointMachine>)(object)val2).Add(scriptComponent2);
			}
		}
		if (!CheckAttachedNavmeshSanityAux(val, val2, isEditorMode))
		{
			result = false;
		}
		if (!CheckSpawnPointsNavMeshSanityAux(isEditorMode))
		{
			result = false;
		}
		if (isEditorMode)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.ClearNavMesh();
		}
		return result;
	}

	private bool CheckPhysicsOfChildren()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetChildrenRecursive(ref list);
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current = item;
			int physicsTriangleCount = GameEntityPhysicsExtensions.GetPhysicsTriangleCount(current);
			if (physicsTriangleCount > 4000)
			{
				object arg = physicsTriangleCount;
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				string text = $"Physics body has too much polygon {arg} for ship part: '{((WeakGameEntity)(ref gameEntity)).Name}' - '{((WeakGameEntity)(ref current)).Name}'.";
				MBEditor.AddEntityWarning(((ScriptComponentBehavior)this).GameEntity, text);
			}
		}
		return result;
	}

	private bool CheckSpawnPoints(bool fromEditor)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		if (MBObjectManager.Instance == null)
		{
			return result;
		}
		MBReadOnlyList<MissionShipObject> objects = MBObjectManager.Instance.GetObjects<MissionShipObject>((Func<MissionShipObject, bool>)delegate(MissionShipObject x)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			string prefab = x.Prefab;
			WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
			return prefab == ((WeakGameEntity)(ref gameEntity2)).Name;
		});
		if (((List<MissionShipObject>)(object)objects).Count == 0)
		{
			return result;
		}
		MissionShipObject missionShipObject = ((List<MissionShipObject>)(object)objects)[0];
		MBReadOnlyList<ShipHull> objects2 = MBObjectManager.Instance.GetObjects<ShipHull>((Func<ShipHull, bool>)((ShipHull x) => x.MissionShipObjectId == ((MBObjectBase)missionShipObject).StringId));
		if (((List<ShipHull>)(object)objects2).Count == 0)
		{
			return result;
		}
		ShipHull val = ((List<ShipHull>)(object)objects2)[0];
		WeakGameEntity gameEntity;
		if (val.TotalCrewCapacity != val.MainDeckCrewCapacity)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((List<WeakGameEntity>)(object)Extensions.ToMBList<WeakGameEntity>(((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("sp_troop_crew_spawn"))).Count == 0)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				string text = "Ship with reinforcements '" + ((WeakGameEntity)(ref gameEntity)).Name + "' does not have any crew spawn point.";
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(((ScriptComponentBehavior)this).GameEntity, text);
				}
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MBList<WeakGameEntity> obj = Extensions.ToMBList<WeakGameEntity>(((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("sp_troop_outer_deck"));
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MBList<WeakGameEntity> val2 = Extensions.ToMBList<WeakGameEntity>(((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("sp_troop_inner_deck"));
		int num = ((List<WeakGameEntity>)(object)obj).Count + ((List<WeakGameEntity>)(object)val2).Count;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("sp_troop_captain").Count == 0)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			string text2 = "Ship '" + ((WeakGameEntity)(ref gameEntity)).Name + "' must have at least one captain spawn entity.";
			if (fromEditor)
			{
				MBEditor.AddEntityWarning(((ScriptComponentBehavior)this).GameEntity, text2);
			}
		}
		else
		{
			num++;
		}
		float num2 = 1f + Math.Max(NavalPerks.Boatswain.PopularCaptain.PrimaryBonus, NavalPerks.Boatswain.PopularCaptain.SecondaryBonus);
		int num3 = (int)((float)val.MainDeckCrewCapacity * num2);
		if (num < num3)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			string text3 = $"Ship '{((WeakGameEntity)(ref gameEntity)).Name}': Main deck crew spawn point count {num}" + $"should be equal or greater than the value set in ship hull xml (including perks): {num3}.";
			if (fromEditor)
			{
				MBEditor.AddEntityWarning(((ScriptComponentBehavior)this).GameEntity, text3);
			}
		}
		return result;
	}

	private bool CheckOarCount(bool fromEditor)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		if (MBObjectManager.Instance == null)
		{
			return result;
		}
		MBReadOnlyList<MissionShipObject> objects = MBObjectManager.Instance.GetObjects<MissionShipObject>((Func<MissionShipObject, bool>)delegate(MissionShipObject x)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			string prefab = x.Prefab;
			WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
			return prefab == ((WeakGameEntity)(ref gameEntity2)).Name;
		});
		if (((List<MissionShipObject>)(object)objects).Count == 0)
		{
			return result;
		}
		int oarCount = ((List<MissionShipObject>)(object)objects)[0].OarCount;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		int count = ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("oar_gate_left").Count;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		int count2 = ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("oar_gate_right").Count;
		if (count + count2 != oarCount)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			string text = "Oar count set in prefab does not match oar count set in mission ship xml for ship '" + ((WeakGameEntity)(ref gameEntity)).Name + "'.";
			if (fromEditor)
			{
				MBEditor.AddEntityWarning(((ScriptComponentBehavior)this).GameEntity, text);
			}
			result = false;
		}
		return result;
	}

	private bool CheckSpawnPointsNavMeshSanityAux(bool fromEditor)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		int num = default(int);
		foreach (WeakGameEntity item in ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("rally_point"))
		{
			WeakGameEntity current = item;
			Vec3 origin = ((WeakGameEntity)(ref current)).GetGlobalFrame().origin;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Scene.GetNavigationMeshForPosition(ref origin, ref num, 1.5f, false) == UIntPtr.Zero)
			{
				string[] obj = new string[5]
				{
					"Rally point '",
					((WeakGameEntity)(ref current)).Name,
					"' is not on any navigation mesh face in ship '",
					null,
					null
				};
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				obj[3] = ((WeakGameEntity)(ref gameEntity)).Name;
				obj[4] = "'.";
				string text = string.Concat(obj);
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(current, text);
				}
				result = false;
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity item2 in ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("sp_troop_captain"))
		{
			WeakGameEntity current2 = item2;
			Vec3 origin2 = ((WeakGameEntity)(ref current2)).GetGlobalFrame().origin;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Scene.GetNavigationMeshForPosition(ref origin2, ref num, 1.5f, false) == UIntPtr.Zero)
			{
				string[] obj2 = new string[5]
				{
					"Captain spawn point '",
					((WeakGameEntity)(ref current2)).Name,
					"' is not on any navigation mesh face in ship '",
					null,
					null
				};
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				obj2[3] = ((WeakGameEntity)(ref gameEntity)).Name;
				obj2[4] = "'.";
				string text2 = string.Concat(obj2);
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(current2, text2);
				}
				result = false;
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity item3 in ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("sp_troop_outer_deck"))
		{
			WeakGameEntity current3 = item3;
			Vec3 origin3 = ((WeakGameEntity)(ref current3)).GetGlobalFrame().origin;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Scene.GetNavigationMeshForPosition(ref origin3, ref num, 1.5f, false) == UIntPtr.Zero)
			{
				string[] obj3 = new string[5]
				{
					"Outer deck spawn point '",
					((WeakGameEntity)(ref current3)).Name,
					"' is not on any navigation mesh face in ship '",
					null,
					null
				};
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				obj3[3] = ((WeakGameEntity)(ref gameEntity)).Name;
				obj3[4] = "'.";
				string text3 = string.Concat(obj3);
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(current3, text3);
				}
				result = false;
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		int num2 = default(int);
		foreach (WeakGameEntity item4 in ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("sp_troop_inner_deck"))
		{
			WeakGameEntity current4 = item4;
			Vec3 origin4 = ((WeakGameEntity)(ref current4)).GetGlobalFrame().origin;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Scene.GetNavigationMeshForPosition(ref origin4, ref num2, 1.5f, false) == UIntPtr.Zero)
			{
				string[] obj4 = new string[5]
				{
					"Inner deck spawn point '",
					((WeakGameEntity)(ref current4)).Name,
					"' is not on any navigation mesh face in ship '",
					null,
					null
				};
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				obj4[3] = ((WeakGameEntity)(ref gameEntity)).Name;
				obj4[4] = "'.";
				string text4 = string.Concat(obj4);
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(current4, text4);
				}
				result = false;
			}
		}
		return result;
	}

	private bool CheckAttachedNavmeshSanityAux(MBList<ShipAttachmentMachine> shipAttachmentMachines, MBList<ShipAttachmentPointMachine> shipAttachmentPointMachines, bool fromEditor)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0618: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0631: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_0581: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		WeakGameEntity gameEntity;
		int num;
		if (!fromEditor)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			num = ((WeakGameEntity)(ref gameEntity)).GetAttachedNavmeshFaceCount();
		}
		else
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			num = ((WeakGameEntity)(ref gameEntity)).Scene.GetNavMeshFaceCount();
		}
		PathFaceRecord[] array = (PathFaceRecord[])(object)new PathFaceRecord[num];
		if (fromEditor)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.GetAllNavmeshFaceRecords(array);
		}
		else
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).GetAttachedNavmeshFaceRecords(array);
		}
		HashSet<int> uniqueIdsFaces = new HashSet<int>();
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> hashSet2 = new HashSet<int>();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		PathFaceRecord[] array2 = (PathFaceRecord[])(object)new PathFaceRecord[((WeakGameEntity)(ref gameEntity)).Scene.GetNavmeshFaceCountBetweenTwoIds(base.DynamicNavmeshIdStart, base.DynamicNavmeshIdStart + 50)];
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).Scene.GetNavmeshFaceRecordsBetweenTwoIds(base.DynamicNavmeshIdStart, base.DynamicNavmeshIdStart + 50, array2);
		PathFaceRecord[] array3 = array2;
		foreach (PathFaceRecord val in array3)
		{
			if (val.FaceGroupIndex < base.DynamicNavmeshIdStart || val.FaceGroupIndex > base.DynamicNavmeshIdStart + 50)
			{
				object arg = val.FaceGroupIndex - base.DynamicNavmeshIdStart;
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				string text = $"The face with id {arg} must not be attached to {((WeakGameEntity)(ref gameEntity)).Name}. Ids must be between 0 and {50}.";
				if (fromEditor)
				{
					gameEntity = ((ScriptComponentBehavior)this).GameEntity;
					MBEditor.AddNavMeshWarning(((WeakGameEntity)(ref gameEntity)).Scene, val, text);
				}
				result = false;
			}
			else if (val.FaceGroupIndex > base.DynamicNavmeshIdStart && !uniqueIdsFaces.Add(val.FaceGroupIndex))
			{
				string text2 = $"Attached navmesh must have faces with unique group ids. Id: {val.FaceGroupIndex - base.DynamicNavmeshIdStart} is not unique";
				if (fromEditor)
				{
					gameEntity = ((ScriptComponentBehavior)this).GameEntity;
					MBEditor.AddNavMeshWarning(((WeakGameEntity)(ref gameEntity)).Scene, val, text2);
				}
				result = false;
			}
		}
		array3 = array2;
		for (int i = 0; i < array3.Length; i++)
		{
			PathFaceRecord val2 = array3[i];
			if (val2.FaceGroupIndex == base.DynamicNavmeshIdStart)
			{
				continue;
			}
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (!((WeakGameEntity)(ref gameEntity)).Scene.HasNavmeshFaceUnsharedEdges(ref val2))
			{
				string text3 = $"The face with id {val2.FaceGroupIndex - base.DynamicNavmeshIdStart} must not be fully enclosed; it must have at least one unshared edge.";
				if (fromEditor)
				{
					gameEntity = ((ScriptComponentBehavior)this).GameEntity;
					MBEditor.AddNavMeshWarning(((WeakGameEntity)(ref gameEntity)).Scene, val2, text3);
				}
				result = false;
			}
		}
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		string name = ((WeakGameEntity)(ref gameEntity)).Name;
		bool flag = false;
		int num3 = default(int);
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)shipAttachmentMachines)
		{
			int num2 = base.DynamicNavmeshIdStart + item.RelatedShipNavmeshOffset;
			if (num2 <= base.DynamicNavmeshIdStart || num2 > base.DynamicNavmeshIdStart + 50)
			{
				gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				string text4 = $"{name}: Every {((WeakGameEntity)(ref gameEntity)).Name}'s RelatedShipNavmeshOffset must be between 1 and {50}.";
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(((ScriptComponentBehavior)item).GameEntity, text4);
				}
				result = false;
			}
			if (!hashSet.Add(item.RelatedShipNavmeshOffset))
			{
				flag = true;
				list.Add(((ScriptComponentBehavior)item).GameEntity);
			}
			if (uniqueIdsFaces.Contains(item.RelatedShipNavmeshOffset + base.DynamicNavmeshIdStart))
			{
				uniqueIdsFaces.Remove(item.RelatedShipNavmeshOffset + base.DynamicNavmeshIdStart);
			}
			gameEntity = ((ScriptComponentBehavior)item).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Scene.GetNavigationMeshForPosition(ref globalFrame.origin, ref num3, 1.5f, false) == UIntPtr.Zero)
			{
				string text5 = $"{name}: shipAttachmentMachine with related id {item.RelatedShipNavmeshOffset} is not on any navmesh face";
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(((ScriptComponentBehavior)item).GameEntity, text5);
				}
				result = false;
			}
			else if (num3 != num2)
			{
				string text6 = $"{name}: ShipAttachmentMachine script with nav mesh id {item.RelatedShipNavmeshOffset} is not on a face with the same id. Current face id: {num3}";
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(((ScriptComponentBehavior)item).GameEntity, text6);
				}
				result = false;
			}
		}
		if (flag)
		{
			foreach (WeakGameEntity item2 in list)
			{
				WeakGameEntity current2 = item2;
				string text7 = name + ": shipAttachmentMachine '" + ((WeakGameEntity)(ref current2)).Name + "' must have a unique RelatedShipNavmeshOffset with respect to other ShipAttachmentMachines";
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(current2, text7);
				}
				result = false;
			}
			flag = false;
			list.Clear();
		}
		int num5 = default(int);
		foreach (ShipAttachmentPointMachine item3 in (List<ShipAttachmentPointMachine>)(object)shipAttachmentPointMachines)
		{
			int num4 = base.DynamicNavmeshIdStart + item3.RelatedShipNavmeshOffset;
			if (num4 <= base.DynamicNavmeshIdStart || num4 > base.DynamicNavmeshIdStart + 50)
			{
				gameEntity = ((ScriptComponentBehavior)item3).GameEntity;
				string text8 = $"{name}: Every {((WeakGameEntity)(ref gameEntity)).Name}'s RelatedShipNavmeshOffset must be between 1 and {50}.";
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(((ScriptComponentBehavior)item3).GameEntity, text8);
				}
				result = false;
			}
			if (!hashSet2.Add(item3.RelatedShipNavmeshOffset))
			{
				flag = true;
				list.Add(((ScriptComponentBehavior)item3).GameEntity);
			}
			if (uniqueIdsFaces.Contains(item3.RelatedShipNavmeshOffset + base.DynamicNavmeshIdStart))
			{
				uniqueIdsFaces.Remove(item3.RelatedShipNavmeshOffset + base.DynamicNavmeshIdStart);
			}
			gameEntity = ((ScriptComponentBehavior)item3).GameEntity;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Scene.GetNavigationMeshForPosition(ref globalFrame2.origin, ref num5, 1.5f, false) == UIntPtr.Zero)
			{
				string text9 = $"{name}: shipAttachmentPointMachine with related id {item3.RelatedShipNavmeshOffset} is not on any navmesh face";
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(((ScriptComponentBehavior)item3).GameEntity, text9);
				}
				result = false;
			}
			else if (num5 != num4)
			{
				string text10 = $"{name}: ShipAttachmentPointMachine script with nav mesh face id {item3.RelatedShipNavmeshOffset} is not on a face with the same id. Current face id: {num5}";
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(((ScriptComponentBehavior)item3).GameEntity, text10);
				}
				result = false;
			}
		}
		foreach (PathFaceRecord item4 in array2.Where((PathFaceRecord record) => uniqueIdsFaces.Contains(record.FaceGroupIndex)).ToList())
		{
			object arg2 = item4.FaceGroupIndex - base.DynamicNavmeshIdStart;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			string text11 = $"{name}: The face with id {arg2} has not been attached to {((WeakGameEntity)(ref gameEntity)).Name}. " + $"There should be a shipAttachmentMachine or a shipAttachmentPointMachine with RelatedShipNavmeshOffset: {item4.FaceGroupIndex - base.DynamicNavmeshIdStart}";
			if (fromEditor)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MBEditor.AddNavMeshWarning(((WeakGameEntity)(ref gameEntity)).Scene, item4, text11);
			}
			result = false;
		}
		if (flag)
		{
			foreach (WeakGameEntity item5 in list)
			{
				WeakGameEntity current5 = item5;
				string text12 = name + ": ShipAttachmentPointMachine '" + ((WeakGameEntity)(ref current5)).Name + "' must have a unique RelatedShipNavmeshOffset with respect to other ShipAttachmentPoints";
				if (fromEditor)
				{
					MBEditor.AddEntityWarning(current5, text12);
				}
			}
			result = false;
		}
		return result;
	}

	private int DeckSpawnFrameSortingFunction(MatrixFrame deckFrame1, MatrixFrame deckFrame2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		float value = Vec3.DotProduct(deckFrame1.origin, Vec3.Forward);
		return -Vec3.DotProduct(deckFrame2.origin, Vec3.Forward).CompareTo(value);
	}

	private void InitializeLists(bool isForCheckingForProblems)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetChildrenRecursive(ref list);
		_rightSideShipOarMachines = new MBList<ShipOarMachine>();
		_leftSideShipOarMachines = new MBList<ShipOarMachine>();
		_shipOarMachines = new MBList<ShipOarMachine>();
		_shipUnmannedOars = new MBList<ShipUnmannedOar>();
		_climbingMachines = new MBList<ClimbingMachine>();
		ShipSiegeWeapon = null;
		_allDestructibleComponents = new MBList<DestructableComponent>();
		_ammoBarrels = new MBList<AmmoBarrelBase>();
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current = item;
			if (((WeakGameEntity)(ref current)).HasScriptOfType<ShipOarMachine>())
			{
				MatrixFrame localFrame = ((WeakGameEntity)(ref current)).GetLocalFrame();
				Vec2 asVec = ((Vec3)(ref localFrame.origin)).AsVec2;
				if (((Vec2)(ref asVec)).DotProduct(Vec2.Side) > 0f)
				{
					((List<ShipOarMachine>)(object)_rightSideShipOarMachines).Add(((WeakGameEntity)(ref current)).GetFirstScriptOfType<ShipOarMachine>());
				}
				else
				{
					((List<ShipOarMachine>)(object)_leftSideShipOarMachines).Add(((WeakGameEntity)(ref current)).GetFirstScriptOfType<ShipOarMachine>());
				}
			}
			else if (((WeakGameEntity)(ref current)).HasScriptOfType<ShipControllerMachine>())
			{
				ShipControllerMachine = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<ShipControllerMachine>();
			}
			else if (((WeakGameEntity)(ref current)).HasScriptOfType<ClimbingMachine>())
			{
				((List<ClimbingMachine>)(object)_climbingMachines).Add(((WeakGameEntity)(ref current)).GetFirstScriptOfType<ClimbingMachine>());
			}
			else if (((WeakGameEntity)(ref current)).HasScriptOfType<ShipUnmannedOar>())
			{
				((List<ShipUnmannedOar>)(object)_shipUnmannedOars).Add(((WeakGameEntity)(ref current)).GetFirstScriptOfType<ShipUnmannedOar>());
			}
			else if (((WeakGameEntity)(ref current)).HasScriptOfType<RangedSiegeWeapon>())
			{
				ShipSiegeWeapon = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<RangedSiegeWeapon>();
			}
			else if (((WeakGameEntity)(ref current)).HasScriptOfType<MissionShipRam>())
			{
				_ram = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<MissionShipRam>();
			}
			else if (((WeakGameEntity)(ref current)).HasScriptOfType<AmmoBarrelBase>())
			{
				((List<AmmoBarrelBase>)(object)_ammoBarrels).Add(((WeakGameEntity)(ref current)).GetFirstScriptOfType<AmmoBarrelBase>());
			}
			if (((WeakGameEntity)(ref current)).HasScriptOfType<DestructableComponent>())
			{
				((List<DestructableComponent>)(object)_allDestructibleComponents).Add(((WeakGameEntity)(ref current)).GetFirstScriptOfType<DestructableComponent>());
			}
		}
		((List<ShipOarMachine>)(object)_leftSideShipOarMachines).Sort((Comparison<ShipOarMachine>)delegate(ShipOarMachine oar1, ShipOarMachine oar2)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)oar1).GameEntity;
			float y3 = ((WeakGameEntity)(ref gameEntity2)).GetLocalFrame().origin.y;
			gameEntity2 = ((ScriptComponentBehavior)oar2).GameEntity;
			float y4 = ((WeakGameEntity)(ref gameEntity2)).GetLocalFrame().origin.y;
			return y4.CompareTo(y3);
		});
		((List<ShipOarMachine>)(object)_rightSideShipOarMachines).Sort((Comparison<ShipOarMachine>)delegate(ShipOarMachine oar1, ShipOarMachine oar2)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)oar1).GameEntity;
			float y3 = ((WeakGameEntity)(ref gameEntity2)).GetLocalFrame().origin.y;
			gameEntity2 = ((ScriptComponentBehavior)oar2).GameEntity;
			float y4 = ((WeakGameEntity)(ref gameEntity2)).GetLocalFrame().origin.y;
			return y4.CompareTo(y3);
		});
		for (int num = 0; num < ((List<ShipOarMachine>)(object)_leftSideShipOarMachines).Count; num++)
		{
			ShipOarMachine shipOarMachine = ((List<ShipOarMachine>)(object)_leftSideShipOarMachines)[num];
			ShipOarMachine shipOarMachine2 = ((List<ShipOarMachine>)(object)_rightSideShipOarMachines)[num];
			gameEntity = ((ScriptComponentBehavior)shipOarMachine).GameEntity;
			float y = ((WeakGameEntity)(ref gameEntity)).GetLocalFrame().origin.y;
			gameEntity = ((ScriptComponentBehavior)shipOarMachine2).GameEntity;
			float y2 = ((WeakGameEntity)(ref gameEntity)).GetLocalFrame().origin.y;
			Math.Abs(y - y2);
			((List<ShipOarMachine>)(object)_shipOarMachines).Add(shipOarMachine);
			((List<ShipOarMachine>)(object)_shipOarMachines).Add(shipOarMachine2);
		}
		MBList<ShipControllerMachine> val = MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<ShipControllerMachine>(((ScriptComponentBehavior)this).GameEntity);
		ShipControllerMachine = ((((List<ShipControllerMachine>)(object)val).Count > 0) ? ((List<ShipControllerMachine>)(object)val)[0] : null);
		_shipAttachmentMachines = Extensions.ToMBList<ShipAttachmentMachine>(from ce in list
			where ((WeakGameEntity)(ref ce)).HasScriptOfType<ShipAttachmentMachine>()
			select ((WeakGameEntity)(ref ce)).GetFirstScriptOfType<ShipAttachmentMachine>());
		_sailVisuals = MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<SailVisual>(((ScriptComponentBehavior)this).GameEntity);
	}

	private void LoadSpawnPoints()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = GameEntity.CreateFromWeakEntity(((ScriptComponentBehavior)this).GameEntity);
		_outerDeckLocalFrames = new MBList<MatrixFrame>();
		MatrixFrame globalFrame;
		MatrixFrame globalFrame2;
		foreach (GameEntity item4 in (List<GameEntity>)(object)Extensions.ToMBList<GameEntity>(MBExtensions.CollectChildrenEntitiesWithTag(val, "sp_troop_outer_deck")))
		{
			globalFrame = val.GetGlobalFrame();
			globalFrame2 = item4.GetGlobalFrame();
			MatrixFrame item = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalFrame2);
			((List<MatrixFrame>)(object)_outerDeckLocalFrames).Add(item);
		}
		_innerDeckLocalFrames = new MBList<MatrixFrame>();
		foreach (GameEntity item5 in (List<GameEntity>)(object)Extensions.ToMBList<GameEntity>(MBExtensions.CollectChildrenEntitiesWithTag(val, "sp_troop_inner_deck")))
		{
			globalFrame2 = val.GetGlobalFrame();
			globalFrame = item5.GetGlobalFrame();
			MatrixFrame item2 = ((MatrixFrame)(ref globalFrame2)).TransformToLocal(ref globalFrame);
			((List<MatrixFrame>)(object)_innerDeckLocalFrames).Add(item2);
		}
		_crewSpawnLocalFrames = new MBList<MatrixFrame>();
		foreach (GameEntity item6 in (List<GameEntity>)(object)Extensions.ToMBList<GameEntity>(MBExtensions.CollectChildrenEntitiesWithTag(val, "sp_troop_crew_spawn")))
		{
			globalFrame = val.GetGlobalFrame();
			globalFrame2 = item6.GetGlobalFrame();
			MatrixFrame item3 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalFrame2);
			((List<MatrixFrame>)(object)_crewSpawnLocalFrames).Add(item3);
		}
		((List<MatrixFrame>)(object)_outerDeckLocalFrames).Sort((Comparison<MatrixFrame>)DeckSpawnFrameSortingFunction);
		((List<MatrixFrame>)(object)_innerDeckLocalFrames).Sort((Comparison<MatrixFrame>)DeckSpawnFrameSortingFunction);
		List<GameEntity> list = MBExtensions.CollectChildrenEntitiesWithTag(val, "sp_troop_captain");
		MBList<MatrixFrame> innerDeckLocalFrames = _innerDeckLocalFrames;
		globalFrame2 = val.GetGlobalFrame();
		globalFrame = list[0].GetGlobalFrame();
		((List<MatrixFrame>)(object)innerDeckLocalFrames).Add(((MatrixFrame)(ref globalFrame2)).TransformToLocal(ref globalFrame));
		CrewSizeOnMainDeck = MathF.Min(DeckFrameCount, ShipOrigin.MainDeckCrewCapacity);
		ShipPlacementDetachment = new ShipPlacementDetachment(in this);
	}

	protected override bool CanPhysicsCollideBetweenTwoEntities(WeakGameEntity myEntity, BodyFlags myEntityBodyFlags, WeakGameEntity otherEntity, BodyFlags otherEntityBodyFlags)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return !Extensions.HasAnyFlag<BodyFlags>(otherEntityBodyFlags, (BodyFlags)16) || Extensions.HasAnyFlag<BodyFlags>(otherEntityBodyFlags, (BodyFlags)8);
	}

	private void LoadShipBanners()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = GameEntity.CreateFromWeakEntity(((ScriptComponentBehavior)this).GameEntity);
		_bannerEntities = Extensions.ToMBList<GameEntity>(MBExtensions.CollectChildrenEntitiesWithTag(val, "banner_with_faction_color"));
		_sailMeshEntities = Extensions.ToMBList<GameEntity>(MBExtensions.CollectChildrenEntitiesWithTag(val, "sail_mesh_entity"));
	}

	public static bool AreShipsConnected(MissionShip ship1, MissionShip ship2)
	{
		return (ship1.ShipIslandCombinedID & ship2.ShipIslandCombinedID) != 0;
	}

	public void OnSetRangedWeaponControlMode(bool value)
	{
		if (ShipSiegeWeapon != null)
		{
			(((UsableMachine)ShipSiegeWeapon).Ai as ShipBallistaAI).SetIsUnderDirectControl(value);
		}
		foreach (SailVisual item in (List<SailVisual>)(object)_sailVisuals)
		{
			item.SetBallistaRopeVisibility(!value);
		}
	}

	public bool IsAgentUsingSiegeWeapon(Agent agent)
	{
		if (ShipSiegeWeapon != null)
		{
			return ((UsableMachine)ShipSiegeWeapon).PilotAgent == agent;
		}
		return false;
	}

	public void SetCustomSailSetting(bool enableCustomSailSetting, SailInput customSailSetting)
	{
		HasCustomSailSetting = enableCustomSailSetting;
		_customSailSetting = customSailSetting;
	}

	public void ShootBallista()
	{
		ShipSiegeWeapon.Shoot();
	}

	public void TryToMaintainConnectionToAnotherShip(MissionShip otherShip, bool forceBridge = true, bool unbreakableBridge = false)
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment != null)
			{
				continue;
			}
			item.SetPreferredTargetShip(otherShip);
			if (item.LinkedAttachmentPointMachine.CurrentAttachment == null)
			{
				item.SetCanConnectToFriends(canConnectToFriends: true);
				ShipAttachmentPointMachine bestEnemyAttachment = item.GetBestEnemyAttachment(checkAttachmentAlreadyExists: true, checkInteractionDistance: false);
				if (bestEnemyAttachment != null)
				{
					item.ConnectWithAttachmentPointMachine(bestEnemyAttachment, forceBridge, unbreakableBridge);
				}
			}
		}
	}

	public void TryToConnectionToAttachmentMachine(ShipAttachmentMachine otherAttachmentMachine, bool forceBridge = true, bool unbreakableBridge = false)
	{
		ShipAttachmentPointMachine shipAttachmentPointMachine = null;
		if (otherAttachmentMachine.CurrentAttachment == null && otherAttachmentMachine.LinkedAttachmentPointMachine.CurrentAttachment == null)
		{
			shipAttachmentPointMachine = otherAttachmentMachine.GetBestEnemyAttachment(checkAttachmentAlreadyExists: true, checkInteractionDistance: false);
		}
		if (shipAttachmentPointMachine != null)
		{
			otherAttachmentMachine.SetPreferredTargetShip(shipAttachmentPointMachine.OwnerShip);
			otherAttachmentMachine.SetCanConnectToFriends(canConnectToFriends: true);
			otherAttachmentMachine.ConnectWithAttachmentPointMachine(shipAttachmentPointMachine, forceBridge, unbreakableBridge);
		}
	}

	public void DisconnectedWithShip(MissionShip otherShip)
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment != null && item.GetPreferredTargetShip() == otherShip)
			{
				item.SetPreferredTargetShip(null);
				if (item.CurrentAttachment.AttachmentTarget.OwnerShip == otherShip)
				{
					item.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
				}
			}
		}
	}

	public void InvalidateLocalBoundingBoxCache()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_localBoundingBoxCacheInvalid = true;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetBoundingboxDirty();
	}

	public void InvalidateActiveFormationTroopOnShipCache()
	{
		_anyActiveFormationTroopOnShip.Expire();
	}

	internal void SeparateFromShip(MissionShip otherShip)
	{
		bool flag = false;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_attachmentMachines)
		{
			if (item.CurrentAttachment != null && item.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected && item.CurrentAttachment.ShipIslandsConnected && (item.CurrentAttachment.AttachmentSource.OwnerShip == otherShip || item.CurrentAttachment.AttachmentTarget.OwnerShip == otherShip))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			RecalculateShipIsland();
			if ((ShipIslandCombinedID & otherShip.ShipUniqueBitwiseID) == 0L)
			{
				otherShip.RecalculateShipIsland();
			}
		}
	}

	internal static void MergeShipIslands(MissionShip ship1, MissionShip ship2)
	{
		if (ship1.ShipIslandCombinedID == ship2.ShipIslandCombinedID)
		{
			return;
		}
		ulong num = ship1.ShipIslandCombinedID | ship2.ShipIslandCombinedID;
		((Queue<MissionShip>)(object)ship1._temporaryMissionShipQueue).Clear();
		((Queue<MissionShip>)(object)ship1._temporaryMissionShipQueue).Enqueue(ship1);
		while (((Queue<MissionShip>)(object)ship1._temporaryMissionShipQueue).Count > 0)
		{
			MissionShip missionShip = ((Queue<MissionShip>)(object)ship1._temporaryMissionShipQueue).Dequeue();
			if (missionShip.ShipIslandCombinedID == num)
			{
				continue;
			}
			missionShip.ShipIslandCombinedID |= num;
			num = missionShip.ShipIslandCombinedID;
			foreach (MissionShip item in (List<MissionShip>)(object)missionShip.GetNavmeshConnectedShips())
			{
				if (item.ShipIslandCombinedID != num)
				{
					((Queue<MissionShip>)(object)ship1._temporaryMissionShipQueue).Enqueue(item);
				}
			}
		}
	}
}
