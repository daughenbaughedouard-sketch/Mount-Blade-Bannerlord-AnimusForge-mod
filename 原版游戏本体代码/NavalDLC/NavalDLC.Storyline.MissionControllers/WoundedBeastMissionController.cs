using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipInput;
using NavalDLC.Storyline.Objectives.WoundedBeast;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.Missions.Objectives;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.MissionControllers;

public class WoundedBeastMissionController : MissionLogic
{
	private struct StorylineTroop
	{
		public string TroopId { get; }

		public int TroopCount { get; }

		public StorylineTroop(string troopId, int troopCount)
		{
			TroopCount = troopCount;
			TroopId = troopId;
		}
	}

	private const string WindDirection = "sp_wind_direction";

	private const string TargetEntityTag = "targeting_entity";

	private const string GunnarInitialDestination = "sp_gangradir_ship_destination";

	private const string LaharShipSpawnId = "sp_lahar_ship";

	private const string GunnarShipSpawnId = "sp_gangradir_ship";

	private const string LaharShipHullId = "ship_liburna_q2_storyline";

	private const string GunnarShipHullId = "northern_medium_ship";

	private const string LaharMeleeTroopId = "southern_pirates_raider";

	private const string LaharRangedTroopId = "aserai_marine_t5";

	private const string GunnarMeleeTroopId = "gangradirs_kin_melee";

	private const string GunnarRangedTroopId = "gangradirs_kin_ranged";

	private readonly List<StorylineTroop> _laharShipTroops = new List<StorylineTroop>();

	private readonly List<StorylineTroop> _gunnarShipTroops = new List<StorylineTroop>();

	private readonly List<Ship> _playerShips = new List<Ship>();

	private readonly MBList<MissionShip> _playerMissionShips = new MBList<MissionShip>();

	private Ship _gunnarShip;

	private Ship _laharShip;

	private MissionShip _laharMissionShip;

	private MissionShip _gunnarMissionShip;

	private static readonly Dictionary<string, string> LaharShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl3" },
		{ "sail", "sails_lvl2" },
		{ "bow", "bow_northern_reinforced_ram_lvl3" }
	};

	private static readonly Dictionary<string, string> GunnarShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl2" },
		{ "sail", "sails_lvl2" }
	};

	private const string FahdaShipSpawnId = "sp_fahda_ship";

	private const string FahdaShipHullId = "ship_meditheavy_storyline";

	private const string MediumReinforcementShipHullId = "ship_liburna_storyline";

	private const string LightReinforcementShipHullId = "ship_meditlight_storyline";

	private const string EnemyMeleeTroopId1 = "southern_pirates_raider";

	private const string EnemyMeleeTroopId2 = "aserai_footman";

	private const string EnemyRangedTroopId = "southern_pirates_bandit";

	private readonly List<StorylineTroop> _fahdaShipTroops = new List<StorylineTroop>();

	private readonly List<StorylineTroop> _enemyReinforcementFirstShipTroops = new List<StorylineTroop>();

	private readonly List<StorylineTroop> _enemyReinforcementSecondShipTroops = new List<StorylineTroop>();

	private readonly List<StorylineTroop> _enemyReinforcementThirdShipTroops = new List<StorylineTroop>();

	private readonly MBList<Agent> _enemySideAgents = new MBList<Agent>();

	private readonly List<Formation> _availailableEnemyFormations = new List<Formation>();

	private readonly MBList<MissionShip> _enemyMissionShips = new MBList<MissionShip>();

	private readonly List<Ship> _enemyShips = new List<Ship>();

	private const string EnemyReinforcementFirstShipTargetEntityTag = "targeting_entity_1";

	private const string EnemyReinforcementSecondShipTargetEntityTag = "targeting_entity_2";

	private const string EnemyReinforcementThirdShipTargetEntityTag = "targeting_entity_3";

	private WeakGameEntity EnemyReinforcementFirstShipTargetEntity;

	private WeakGameEntity EnemyReinforcementSecondShipTargetEntity;

	private WeakGameEntity EnemyReinforcementThirdShipTargetEntity;

	private Ship _fahdaShip;

	private MissionShip _fahdaMissionShip;

	private MissionShip _enemyReinforcementFirstMissionShip;

	private MissionShip _enemyReinforcementSecondMissionShip;

	private MissionShip _enemyReinforcementThirdMissionShip;

	private static readonly Dictionary<string, string> FahdaShipUpgradePieces = new Dictionary<string, string> { { "side", "side_southern_shields_lvl2" } };

	private static readonly Dictionary<string, string> MediumReinforcementShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl2" },
		{ "sail", "sails_lvl2" }
	};

	private static readonly Dictionary<string, string> FirstLightReinforcementShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl2" },
		{ "sail", "sails_lvl2" }
	};

	private static readonly Dictionary<string, string> SecondLightReinforcementShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl2" },
		{ "sail", "sails_lvl2" }
	};

	private float _drownCheckTimer;

	private float _drownCheckDuration = 2f;

	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	private MissionObjectiveLogic _missionObjectiveLogic;

	private Vec2 _fleePoint;

	private Vec2 _gunnarInitialDestination;

	private bool _initialized;

	private bool _isMissionSuccessful;

	private bool _isMissionFailed;

	private bool _inPhase1 = true;

	private MissionTimer _failingQuestTimer;

	private float _startDistanceToFleePoint;

	private bool _nearFleePoint;

	private bool _targetedSmallerVessels;

	private bool _targetedBiggerVessel;

	private bool _targetedBySmallerVessels;

	private readonly Dictionary<MissionShip, bool> _shipsToAlert = new Dictionary<MissionShip, bool>();

	private readonly Dictionary<MissionShip, bool> _alertedShips = new Dictionary<MissionShip, bool>();

	public WoundedBeastMissionController()
	{
		_gunnarShipTroops.Add(new StorylineTroop("gangradirs_kin_melee", 15));
		_gunnarShipTroops.Add(new StorylineTroop("gangradirs_kin_ranged", 18));
		_laharShipTroops.Add(new StorylineTroop("southern_pirates_raider", 25));
		_laharShipTroops.Add(new StorylineTroop("aserai_marine_t5", 18));
		_fahdaShipTroops.Add(new StorylineTroop("southern_pirates_raider", 2));
		_fahdaShipTroops.Add(new StorylineTroop("aserai_footman", 66));
		_fahdaShipTroops.Add(new StorylineTroop("southern_pirates_bandit", 0));
		_enemyReinforcementThirdShipTroops.Add(new StorylineTroop("southern_pirates_raider", 10));
		_enemyReinforcementThirdShipTroops.Add(new StorylineTroop("aserai_footman", 13));
		_enemyReinforcementThirdShipTroops.Add(new StorylineTroop("southern_pirates_bandit", 0));
		_enemyReinforcementSecondShipTroops.Add(new StorylineTroop("southern_pirates_raider", 12));
		_enemyReinforcementSecondShipTroops.Add(new StorylineTroop("aserai_footman", 7));
		_enemyReinforcementSecondShipTroops.Add(new StorylineTroop("southern_pirates_bandit", 0));
		_enemyReinforcementFirstShipTroops.Add(new StorylineTroop("southern_pirates_raider", 12));
		_enemyReinforcementFirstShipTroops.Add(new StorylineTroop("aserai_footman", 8));
		_enemyReinforcementFirstShipTroops.Add(new StorylineTroop("southern_pirates_bandit", 0));
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_missionObjectiveLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionObjectiveLogic>();
		_navalShipsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetTeamShipDeploymentLimit((TeamSideEnum)0, NavalShipDeploymentLimit.Max());
		_navalShipsLogic.SetTeamShipDeploymentLimit((TeamSideEnum)2, NavalShipDeploymentLimit.Max());
		_navalShipsLogic.SetDeploymentMode(value: false);
		if (!SailWindProfile.IsSailWindProfileInitialized)
		{
			SailWindProfile.InitializeProfile();
		}
		_navalShipsLogic.ShipRammingEvent += OnShipRammed;
	}

	public override void OnRemoveBehavior()
	{
		((MissionBehavior)this).OnRemoveBehavior();
		_navalShipsLogic.ShipRammingEvent -= OnShipRammed;
	}

	public override void OnMissionStateFinalized()
	{
		SailWindProfile.FinalizeProfile();
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		if (!_initialized)
		{
			Initialize();
		}
		if ((Agent.Main == null || !Agent.Main.IsActive()) && _failingQuestTimer == null && _inPhase1)
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=ay5y18aq}You pass out from the pain of your wounds.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
			OnFailed();
			_failingQuestTimer = new MissionTimer(5f);
		}
		if (_failingQuestTimer != null)
		{
			if (_failingQuestTimer.Check(false))
			{
				((MissionBehavior)this).Mission.EndMission();
			}
			return;
		}
		if (!_fahdaMissionShip.IsSinking)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_fahdaMissionShip).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
			if (((Vec2)(ref asVec)).Distance(_fleePoint) < 100f)
			{
				CampaignInformationManager.AddDialogLine(new TextObject("{=9Y1iHrQ4}Ach. We couldn't catch Fahda in time.", (Dictionary<string, object>)null), NavalStorylineData.Lahar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
				OnFailed();
				_failingQuestTimer = new MissionTimer(5f);
				return;
			}
		}
		if (_inPhase1)
		{
			OnPhase1Tick(dt);
		}
		if (IsShipActive(_fahdaMissionShip) && !_fahdaMissionShip.GetIsConnected())
		{
			_fahdaMissionShip.ShipOrder.SetShipMovementOrder(in _fleePoint);
		}
		TickGunnarsShip();
		CheckTargetShipNearEscapePoint();
		CheckDrowningAgents(dt);
		CheckMissionEnd();
	}

	private void CheckMissionEnd()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!_isMissionFailed && !_isMissionSuccessful)
		{
			if (GetAgentCountOfSide(((MissionBehavior)this).Mission.PlayerTeam.Side) == 0)
			{
				OnFailed();
			}
			else if (GetAgentCountOfSide(Extensions.GetOppositeSide(((MissionBehavior)this).Mission.PlayerTeam.Side)) == 0)
			{
				OnSuccess();
			}
			else if (!((IEnumerable<MissionShip>)_enemyMissionShips).Any((MissionShip x) => IsShipActive(x)))
			{
				OnSuccess();
			}
		}
	}

	private void TickGunnarsShip()
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsShipActive(_gunnarMissionShip) || _gunnarMissionShip.GetIsConnectedToEnemy())
		{
			return;
		}
		if (IsShipAlerted(_gunnarMissionShip))
		{
			if (_gunnarMissionShip.ShipOrder.TargetShip == null || (_gunnarMissionShip.ShipOrder.TargetShip == _fahdaMissionShip && IsShipActive(_fahdaMissionShip)) || !IsShipActive(_gunnarMissionShip.ShipOrder.TargetShip))
			{
				MissionShip missionShip = ((IEnumerable<MissionShip>)_enemyMissionShips).Where((MissionShip x) => x != _fahdaMissionShip && IsShipActive(x)).OrderBy(delegate(MissionShip y)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_0009: Unknown result type (might be due to invalid IL or missing references)
					//IL_000e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0017: Unknown result type (might be due to invalid IL or missing references)
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					//IL_001f: Unknown result type (might be due to invalid IL or missing references)
					WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)y).GameEntity;
					Vec3 globalPosition2 = ((WeakGameEntity)(ref gameEntity2)).GlobalPosition;
					gameEntity2 = ((ScriptComponentBehavior)_gunnarMissionShip).GameEntity;
					return ((Vec3)(ref globalPosition2)).Distance(((WeakGameEntity)(ref gameEntity2)).GlobalPosition);
				}).FirstOrDefault() ?? _gunnarMissionShip.ShipOrder.ClosestEnemyShip;
				if (missionShip == null)
				{
					_gunnarMissionShip.ShipOrder.SetShipStopOrder();
				}
				else if (_gunnarMissionShip.ShipOrder.TargetShip == null || missionShip != _gunnarMissionShip.ShipOrder.TargetShip)
				{
					_gunnarMissionShip.SetAnchor(isAnchored: false);
					_gunnarMissionShip.ShipOrder.SetShipEngageOrder(missionShip);
					_gunnarMissionShip.ShipOrder.SetBoardingTargetShip(missionShip);
					_gunnarMissionShip.ShipOrder.IsBoardingAvailable = true;
				}
			}
		}
		else
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_gunnarMissionShip).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			if (((Vec3)(ref globalPosition)).Distance(((Vec2)(ref _gunnarInitialDestination)).ToVec3(0f)) < 10f)
			{
				_gunnarMissionShip.SetAnchor(isAnchored: true, anchorInPlace: true);
				return;
			}
			_gunnarMissionShip.SetAnchor(isAnchored: false);
			_gunnarMissionShip.ShipOrder.SetShipMovementOrder(in _gunnarInitialDestination);
		}
	}

	private bool IsShipAlerted(MissionShip ship)
	{
		bool value;
		return _alertedShips.TryGetValue(ship, out value) && value;
	}

	private bool IsShipActive(MissionShip ship)
	{
		if (!((MissionObject)ship).IsDisabled && ship.Formation.CountOfUnits > 0)
		{
			return !ship.IsSinking;
		}
		return false;
	}

	private void OnPhase1Tick(float dt)
	{
		MissionShip fahdaMissionShip = _fahdaMissionShip;
		if (fahdaMissionShip != null && fahdaMissionShip.IsSinking)
		{
			OnTargetShipSunk();
			_inPhase1 = false;
			return;
		}
		foreach (MissionShip item in (List<MissionShip>)(object)_enemyMissionShips)
		{
			MissionShip connectedEnemyShip2;
			if (item != _fahdaMissionShip)
			{
				if (!IsShipActive(item))
				{
					continue;
				}
				if (!IsShipAlerted(item))
				{
					bool value;
					if (_laharMissionShip.GetIsConnectedToEnemy(out var connectedEnemyShip))
					{
						if (connectedEnemyShip == item)
						{
							AlertShip(item, _laharMissionShip);
							AlertShip(_gunnarMissionShip, item);
							TriggerSmallerShipNotifications(hasPlayerAttemptedToBoard: true);
						}
						if (connectedEnemyShip == _fahdaMissionShip)
						{
							AlertShip(item, _laharMissionShip);
							TriggerTargetShipNotifications();
						}
					}
					else if (_gunnarMissionShip.GetIsConnectedToEnemy(out connectedEnemyShip))
					{
						if (connectedEnemyShip == item)
						{
							AlertShip(item, _gunnarMissionShip);
						}
					}
					else if (_shipsToAlert.TryGetValue(item, out value) && value)
					{
						AlertShip(item);
					}
				}
				else
				{
					TickEnemyShip(item);
					if (item.ShipOrder.GetIsAttemptingBoarding() && item.ShipOrder.TargetShip == _laharMissionShip)
					{
						TriggerSmallerShipNotifications(hasPlayerAttemptedToBoard: false);
					}
				}
			}
			else if (_laharMissionShip.GetIsConnectedToEnemy(out connectedEnemyShip2) && _fahdaMissionShip == connectedEnemyShip2)
			{
				TriggerTargetShipNotifications();
				AlertShip(_gunnarMissionShip, _gunnarMissionShip.ShipOrder.ClosestEnemyShip ?? item);
			}
		}
		MoveEscortShipsToTheirDefencePositions(dt);
	}

	private void CheckDrowningAgents(float dt)
	{
		_drownCheckTimer += dt;
		if (_drownCheckTimer >= _drownCheckDuration)
		{
			_drownCheckTimer = 0f;
			for (int num = ((List<MissionShip>)(object)_enemyMissionShips).Count - 1; num >= 0; num--)
			{
				CheckDrowningAgents(((List<MissionShip>)(object)_enemyMissionShips)[num]);
			}
		}
	}

	private void CheckDrowningAgents(MissionShip ship)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent item in ((IEnumerable<Agent>)_navalAgentsLogic.GetActiveAgentsOfShip(ship)).ToList())
		{
			if (!item.IsHero && (int)item.CurrentMortalityState == 0 && item.IsActive() && item.IsInWater())
			{
				DrownAgent(item, MBRandom.RandomInt(10, 100));
			}
		}
	}

	private void DrownAgent(Agent agent, int inflictedDamage)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Invalid comparison between Unknown and I4
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		Blow val = default(Blow);
		((Blow)(ref val))._002Ector(agent.Index);
		val.DamageType = (DamageTypes)2;
		val.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
		val.BaseMagnitude = inflictedDamage;
		val.GlobalPosition = agent.Position;
		val.GlobalPosition.z += agent.GetEyeGlobalHeight();
		val.DamagedPercentage = 1f;
		((BlowWeaponRecord)(ref val.WeaponRecord)).FillAsMeleeBlow((ItemObject)null, (WeaponComponentData)null, -1, (sbyte)(-1));
		val.SwingDirection = agent.LookDirection;
		val.Direction = val.SwingDirection;
		val.InflictedDamage = inflictedDamage;
		val.DamageCalculated = true;
		sbyte mainHandItemBoneIndex = agent.Monster.MainHandItemBoneIndex;
		AttackCollisionData attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, (CombatCollisionResult)1, -1, 0, 2, val.BoneIndex, (BoneBodyPartType)0, mainHandItemBoneIndex, (UsageDirection)2, -1, (CombatHitResultFlags)0, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, val.Direction, val.GlobalPosition, Vec3.Zero, Vec3.Zero, agent.Velocity, Vec3.Up);
		agent.RegisterBlow(val, ref attackCollisionDataForDebugPurpose);
		agent.MakeVoice(VoiceType.Drown, (CombatVoiceNetworkPredictionType)2);
		if ((int)agent.Controller == 1)
		{
			Vec3 val2 = new Vec3(0f, 0f, -20f, -1f);
			agent.AddAcceleration(ref val2);
		}
	}

	private void TickEnemyShip(MissionShip ship)
	{
		if (IsShipActive(ship) && !ship.GetIsConnectedToEnemy() && IsShipAlerted(ship) && ship.ShipOrder.TargetShip == null)
		{
			MissionShip missionShip = (IsShipActive(_laharMissionShip) ? _laharMissionShip : ship.ShipOrder.ClosestEnemyShip);
			if (missionShip == null)
			{
				ship.ShipOrder.SetShipStopOrder();
				return;
			}
			ship.SetAnchor(isAnchored: false);
			ship.ShipOrder.SetShipEngageOrder(missionShip);
		}
	}

	private void CheckTargetShipNearEscapePoint()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		if (_nearFleePoint || !IsShipActive(_fahdaMissionShip) || _fahdaMissionShip.GetIsConnectedToEnemy())
		{
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_fahdaMissionShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		if (((Vec2)(ref asVec)).Distance(_fleePoint) < _startDistanceToFleePoint * 0.33f)
		{
			_nearFleePoint = true;
			if (!_fahdaMissionShip.GetIsConnectedToEnemy())
			{
				CampaignInformationManager.AddDialogLine(new TextObject("{=KMNUcHJ5}The winds are still strong and a new squall could brew up at any time. If she gets much further we might lose sight of her.", (Dictionary<string, object>)null), NavalStorylineData.Lahar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
			}
		}
	}

	private void TriggerTargetShipNotifications()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (!_targetedBiggerVessel)
		{
			CampaignInformationManager.AddDialogLine(new TextObject("{=isa8iCbC}No! No! If you board that monster we’re finished! Cut loose!", (Dictionary<string, object>)null), NavalStorylineData.Lahar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
			_targetedBiggerVessel = true;
		}
	}

	private void TriggerSmallerShipNotifications(bool hasPlayerAttemptedToBoard)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		if (hasPlayerAttemptedToBoard && !_targetedSmallerVessels && IsShipActive(_fahdaMissionShip))
		{
			CampaignInformationManager.AddDialogLine(new TextObject("{=AFdg8UHM}Go for her flagship! We don’t want it to get away! We’ll deal with the lesser vessels later.", (Dictionary<string, object>)null), NavalStorylineData.Lahar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
			_targetedSmallerVessels = true;
		}
		if (!hasPlayerAttemptedToBoard && !_targetedBySmallerVessels && IsShipActive(_fahdaMissionShip))
		{
			CampaignInformationManager.AddDialogLine(new TextObject("{=HOAwSlCQ}One of the others is going to board us! Repel them and cut loose, or we’ll never catch her!", (Dictionary<string, object>)null), NavalStorylineData.Lahar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
			_targetedBySmallerVessels = true;
		}
	}

	private void OnTargetShipSunk()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		AlertAllShips();
		List<MissionShip> list = ((IEnumerable<MissionShip>)_enemyMissionShips).Where((MissionShip x) => x != _fahdaMissionShip).ToList();
		if (list.Count > 0)
		{
			FinishOffConsortsObjective finishOffConsortsObjective = new FinishOffConsortsObjective(((MissionBehavior)this).Mission, list);
			_missionObjectiveLogic.StartObjective((MissionObjective)(object)finishOffConsortsObjective);
			CampaignInformationManager.AddDialogLine(new TextObject("{=CzYbzDM8}Good! You dealt her ship a mortal wound. It’s going down! Now, finish off its consorts.", (Dictionary<string, object>)null), NavalStorylineData.Lahar.CharacterObject, (Equipment)null, 3000, (NotificationPriority)2);
		}
	}

	private void MoveEscortShipsToTheirDefencePositions(float dt)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		_fahdaMissionShip.ShipOrder.IsBoardingAvailable = false;
		GetDefencePositionsForReinforcementShips(out var leftSide, out var rightSide, out var behind);
		foreach (MissionShip item in (List<MissionShip>)(object)_enemyMissionShips)
		{
			if (item != _fahdaMissionShip && IsShipActive(item) && !IsShipAlerted(item))
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				Vec2 targetPosition = ((Vec3)(ref globalPosition)).AsVec2;
				if (item == _enemyReinforcementFirstMissionShip)
				{
					targetPosition = behind;
				}
				else if (item == _enemyReinforcementSecondMissionShip)
				{
					targetPosition = rightSide;
				}
				else if (item == _enemyReinforcementThirdMissionShip)
				{
					targetPosition = leftSide;
				}
				item.ShipOrder.IsBoardingAvailable = false;
				item.ShipOrder.SetShipMovementOrder(in targetPosition);
			}
		}
	}

	private void OnSuccess()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		_isMissionSuccessful = true;
		PlayerEncounter.CampaignBattleResult = CampaignBattleResult.GetResult((BattleState)1, false);
		MBInformationManager.AddQuickInformation(new TextObject("{=15aPhWar}Success! You defeated Fahda's fleet.", (Dictionary<string, object>)null), 2000, (BasicCharacterObject)null, (Equipment)null, "");
	}

	private void OnFailed()
	{
		_isMissionFailed = true;
		PlayerEncounter.CampaignBattleResult = CampaignBattleResult.GetResult((BattleState)2, false);
	}

	public override bool MissionEnded(ref MissionResult missionResult)
	{
		bool result = false;
		if (_isMissionSuccessful)
		{
			missionResult = MissionResult.CreateSuccessful((IMission)(object)((MissionBehavior)this).Mission, false);
			result = true;
		}
		else if (_isMissionFailed)
		{
			missionResult = MissionResult.CreateDefeated((IMission)(object)((MissionBehavior)this).Mission);
			result = true;
		}
		return result;
	}

	private void UpdateSceneWindDirectionAndWaterStrength()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = Mission.Current.Scene;
		WeakGameEntity val = ((MissionBehavior)this).Mission.Scene.FindWeakEntityWithTag("sp_wind_direction");
		MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		Vec2 val2 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2 * 12f;
		scene.SetGlobalWindVelocity(ref val2);
		Mission.Current.Scene.SetWaterStrength(3f);
	}

	private MissionShip CreateShip(IShipOrigin ship, Team team, Formation formation, WeakGameEntity spawnEntity)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame shipFrame = ((WeakGameEntity)(ref spawnEntity)).GetGlobalFrame();
		Scene scene = Mission.Current.Scene;
		Vec3 globalPosition = ((WeakGameEntity)(ref spawnEntity)).GlobalPosition;
		float waterLevelAtPosition = scene.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, false, false);
		shipFrame.origin = new Vec3(((WeakGameEntity)(ref spawnEntity)).GlobalPosition.x, ((WeakGameEntity)(ref spawnEntity)).GlobalPosition.y, waterLevelAtPosition, -1f);
		MissionShip missionShip = _navalShipsLogic.SpawnShip(ship, in shipFrame, team, formation, spawnAnchored: false, (FormationClass)8);
		missionShip.ShipOrder.FormationJoinShip(formation);
		if (team.IsEnemyOf(((MissionBehavior)this).Mission.PlayerTeam))
		{
			((UsableMissionObject)((UsableMachine)missionShip.ShipControllerMachine).PilotStandingPoint).IsDisabledForPlayers = true;
		}
		return missionShip;
	}

	public void AlertShip(MissionShip missionShip, MissionShip target = null)
	{
		if (CanAlertShip(missionShip))
		{
			if (_shipsToAlert.TryGetValue(missionShip, out var value) && value)
			{
				_shipsToAlert[missionShip] = false;
			}
			missionShip.ShipOrder.IsBoardingAvailable = true;
			_alertedShips[missionShip] = true;
			missionShip.SetAnchor(isAnchored: false);
			target = target ?? missionShip.ShipOrder.ClosestEnemyShip;
			if (target != null)
			{
				missionShip.ShipOrder.SetShipEngageOrder(target);
			}
		}
	}

	private void AlertAllEnemyShips()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_enemyMissionShips)
		{
			if (item != _fahdaMissionShip)
			{
				AlertShip(item, _laharMissionShip);
			}
		}
	}

	private void AlertAllShips()
	{
		AlertAllEnemyShips();
		AlertShip(_gunnarMissionShip);
	}

	private bool CanAlertShip(MissionShip missionShip)
	{
		if (IsShipActive(missionShip))
		{
			return !IsShipAlerted(missionShip);
		}
		return false;
	}

	private void Initialize()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		_inPhase1 = true;
		WeakGameEntity val = ((MissionBehavior)this).Mission.Scene.FindWeakEntityWithTag("sp_flee_point");
		Vec3 globalPosition = ((WeakGameEntity)(ref val)).GlobalPosition;
		_fleePoint = ((Vec3)(ref globalPosition)).AsVec2;
		val = ((MissionBehavior)this).Mission.Scene.FindWeakEntityWithTag("sp_gangradir_ship_destination");
		globalPosition = ((WeakGameEntity)(ref val)).GlobalPosition;
		_gunnarInitialDestination = ((Vec3)(ref globalPosition)).AsVec2;
		_initialized = true;
		CampaignInformationManager.AddDialogLine(new TextObject("{=Gdaayb1y}Ha! It looks like her ship took a lot of damage. Her crew must not have furled the sails properly before the winds hit, and now she’s just limping along. Sink her!", (Dictionary<string, object>)null), NavalStorylineData.Lahar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		_availailableEnemyFormations.AddRange((IEnumerable<Formation>)((MissionBehavior)this).Mission.PlayerEnemyTeam.FormationsIncludingEmpty);
		_navalShipsLogic.SetDeploymentMode(value: true);
		SpawnPlayerSide();
		SpawnEnemySide();
		foreach (MissionShip item in (List<MissionShip>)(object)_playerMissionShips)
		{
			item.SetShipOrderActive(isOrderActive: true);
		}
		foreach (MissionShip item2 in (List<MissionShip>)(object)_enemyMissionShips)
		{
			item2.SetShipOrderActive(isOrderActive: true);
			foreach (ShipAttachmentMachine item3 in (List<ShipAttachmentMachine>)(object)item2.ShipAttachmentMachines)
			{
				((UsableMachine)item3).SetIsDisabledForAI(true);
			}
		}
		NavalShipsLogic navalShipsLogic = _navalShipsLogic;
		MissionShip laharMissionShip = _laharMissionShip;
		val = ((ScriptComponentBehavior)_laharMissionShip).GameEntity;
		navalShipsLogic.TeleportShip(laharMissionShip, ((WeakGameEntity)(ref val)).GetGlobalFrame(), checkFreeArea: true);
		NavalShipsLogic navalShipsLogic2 = _navalShipsLogic;
		MissionShip gunnarMissionShip = _gunnarMissionShip;
		val = ((ScriptComponentBehavior)_gunnarMissionShip).GameEntity;
		navalShipsLogic2.TeleportShip(gunnarMissionShip, ((WeakGameEntity)(ref val)).GetGlobalFrame(), checkFreeArea: true);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)0);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)2);
		Mission.Current.OnDeploymentFinished();
		_navalShipsLogic.SetDeploymentMode(value: false);
		UpdateSceneWindDirectionAndWaterStrength();
	}

	private void OnShipRammed(MissionShip ship1, MissionShip ship2, float damagePercent, bool isFirstImpact, CapsuleData data, int ramQuality)
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		if (ship1 == _laharMissionShip && ship2 != _fahdaMissionShip && isFirstImpact && _fahdaMissionShip.Formation.CountOfUnits > 0 && ship2.Team.IsEnemyOf(((MissionBehavior)this).Mission.PlayerTeam))
		{
			TriggerSmallerShipNotifications(hasPlayerAttemptedToBoard: true);
			if (CanAlertShip(ship2) && damagePercent < 1f)
			{
				_shipsToAlert[ship2] = true;
			}
		}
		if (!(ship1 == _laharMissionShip && ship2 == _fahdaMissionShip && isFirstImpact))
		{
			return;
		}
		foreach (MissionShip item in (List<MissionShip>)(object)_enemyMissionShips)
		{
			if (item != _fahdaMissionShip && CanAlertShip(item))
			{
				_shipsToAlert[item] = true;
			}
		}
		if (_fahdaMissionShip.Formation.CountOfUnits > 0 && damagePercent < 1f)
		{
			CampaignInformationManager.AddDialogLine(new TextObject("{=18qp71BY}Well done! Give her another one, for luck.", (Dictionary<string, object>)null), NavalStorylineData.Lahar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}
		if (ship2 == _fahdaMissionShip && isFirstImpact && damagePercent > 1f)
		{
			_navalShipsLogic.ShipRammingEvent -= OnShipRammed;
		}
	}

	private void SpawnPlayerSide()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		Team team = Mission.GetTeam((TeamSideEnum)0);
		WeakGameEntity spawnEntity = ((MissionBehavior)this).Mission.Scene.FindWeakEntityWithTag("sp_lahar_ship");
		ShipHull questShipHull = ((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>("ship_liburna_q2_storyline");
		_laharShip = (Ship)(((object)((IEnumerable<Ship>)MobileParty.MainParty.Ships).FirstOrDefault((Func<Ship, bool>)((Ship x) => x.ShipHull == questShipHull))) ?? ((object)new Ship(questShipHull)
		{
			IsTradeable = false,
			IsUsedByQuest = true,
			Owner = PartyBase.MainParty
		}));
		_laharShip.ChangeFigurehead(DefaultFigureheads.Hawk);
		AddShipUpgradePieces(_laharShip, LaharShipUpgradePieces);
		_laharMissionShip = CreateShip((IShipOrigin)(object)_laharShip, ((MissionBehavior)this).Mission.PlayerTeam, team.GetFormation((FormationClass)0), spawnEntity);
		((List<MissionShip>)(object)_playerMissionShips).Add(_laharMissionShip);
		_playerShips.Add(_laharShip);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_laharMissionShip, _laharShipTroops.Sum((StorylineTroop t) => t.TroopCount) + 2);
		SpawnNonHeroAgents(_laharMissionShip, _laharShipTroops, PartyBase.MainParty);
		SpawnHero(CharacterObject.PlayerCharacter, _laharMissionShip, PartyBase.MainParty);
		SpawnHero(NavalStorylineData.Lahar.CharacterObject, _laharMissionShip, PartyBase.MainParty);
		WeakGameEntity spawnEntity2 = ((MissionBehavior)this).Mission.Scene.FindWeakEntityWithTag("sp_gangradir_ship");
		ShipHull northernMediumShipHull = ((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>("northern_medium_ship");
		_gunnarShip = (Ship)(((object)((IEnumerable<Ship>)MobileParty.MainParty.Ships).FirstOrDefault((Func<Ship, bool>)((Ship x) => x.ShipHull == northernMediumShipHull))) ?? ((object)new Ship(((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>("northern_medium_ship"))
		{
			IsTradeable = false,
			IsUsedByQuest = true,
			Owner = PartyBase.MainParty
		}));
		_gunnarShip.ChangeFigurehead(DefaultFigureheads.Dragon);
		AddShipUpgradePieces(_gunnarShip, GunnarShipUpgradePieces);
		_gunnarMissionShip = CreateShip((IShipOrigin)(object)_gunnarShip, ((MissionBehavior)this).Mission.PlayerTeam, team.GetFormation((FormationClass)1), spawnEntity2);
		((List<MissionShip>)(object)_playerMissionShips).Add(_gunnarMissionShip);
		_playerShips.Add(_gunnarShip);
		_alertedShips[_gunnarMissionShip] = false;
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_gunnarMissionShip, _gunnarShipTroops.Sum((StorylineTroop t) => t.TroopCount) + 1);
		SpawnNonHeroAgents(_gunnarMissionShip, _gunnarShipTroops, PartyBase.MainParty);
		SpawnHero(NavalStorylineData.Gunnar.CharacterObject, _gunnarMissionShip, PartyBase.MainParty);
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
		Agent.Main.Controller = (AgentControllerType)2;
		Agent.Main.Formation.PlayerOwner = Agent.Main;
		Mission.Current.PlayerTeam.PlayerOrderController.Owner = Agent.Main;
		Agent val = ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).First((Agent x) => x.IsHero && (object)x.Character == NavalStorylineData.Gunnar.CharacterObject);
		val.Formation.PlayerOwner = val;
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.Owner = Agent.Main;
		_navalAgentsLogic.AssignCaptainToShipForDeploymentMode(Agent.Main, _laharMissionShip);
		_navalAgentsLogic.AssignCaptainToShipForDeploymentMode(val, _gunnarMissionShip);
	}

	private void SpawnEnemySide()
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Expected O, but got Unknown
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		Formation formation = _availailableEnemyFormations.First();
		_availailableEnemyFormations.RemoveAt(0);
		ShipHull fahdaShipHull = ((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>("ship_meditheavy_storyline");
		_fahdaShip = (Ship)(((object)((IEnumerable<Ship>)encounteredParty.Ships).FirstOrDefault((Func<Ship, bool>)((Ship x) => x.ShipHull == fahdaShipHull))) ?? ((object)new Ship(fahdaShipHull)
		{
			IsTradeable = false,
			IsUsedByQuest = true,
			Owner = encounteredParty
		}));
		_fahdaShip.ChangeFigurehead(DefaultFigureheads.Viper);
		AddShipUpgradePieces(_gunnarShip, FahdaShipUpgradePieces);
		_fahdaMissionShip = CreateShip((IShipOrigin)(object)_fahdaShip, ((MissionBehavior)this).Mission.PlayerEnemyTeam, formation, ((MissionBehavior)this).Mission.Scene.FindWeakEntityWithTag("sp_fahda_ship"));
		_fahdaMissionShip.SetCustomSailSetting(enableCustomSailSetting: true, SailInput.Raised);
		_fahdaMissionShip.Formation.SetControlledByAI(false, false);
		_fahdaMissionShip.SetCanBeTakenOver(value: false);
		if (_missionObjectiveLogic != null)
		{
			SinkShipObjective sinkShipObjective = new SinkShipObjective(((MissionBehavior)this).Mission, _fahdaMissionShip);
			_missionObjectiveLogic.StartObjective((MissionObjective)(object)sinkShipObjective);
		}
		_enemyShips.Add(_fahdaShip);
		((List<MissionShip>)(object)_enemyMissionShips).Add(_fahdaMissionShip);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_fahdaMissionShip).GameEntity;
		List<WeakGameEntity> source = ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("targeting_entity");
		EnemyReinforcementThirdShipTargetEntity = ((IEnumerable<WeakGameEntity>)source).FirstOrDefault((Func<WeakGameEntity, bool>)((WeakGameEntity t) => ((WeakGameEntity)(ref t)).HasTag("targeting_entity_3")));
		EnemyReinforcementSecondShipTargetEntity = ((IEnumerable<WeakGameEntity>)source).FirstOrDefault((Func<WeakGameEntity, bool>)((WeakGameEntity t) => ((WeakGameEntity)(ref t)).HasTag("targeting_entity_2")));
		EnemyReinforcementFirstShipTargetEntity = ((IEnumerable<WeakGameEntity>)source).FirstOrDefault((Func<WeakGameEntity, bool>)((WeakGameEntity t) => ((WeakGameEntity)(ref t)).HasTag("targeting_entity_1")));
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_fahdaMissionShip, _fahdaShipTroops.Sum((StorylineTroop t) => t.TroopCount) + 1);
		_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(encounteredParty, NavalStorylineData.EmiraAlFahda.CharacterObject, -1, default(UniqueTroopDescriptor), false, true), _fahdaMissionShip);
		SpawnNonHeroAgents(_fahdaMissionShip, _fahdaShipTroops, encounteredParty, NavalStorylineData.CorsairBanner);
		_enemyReinforcementFirstMissionShip = SpawnReinforcementShip(EnemyReinforcementThirdShipTargetEntity, _enemyReinforcementThirdShipTroops, "ship_liburna_storyline", MediumReinforcementShipUpgradePieces);
		_enemyReinforcementSecondMissionShip = SpawnReinforcementShip(EnemyReinforcementSecondShipTargetEntity, _enemyReinforcementSecondShipTroops, "ship_meditlight_storyline", FirstLightReinforcementShipUpgradePieces);
		_enemyReinforcementThirdMissionShip = SpawnReinforcementShip(EnemyReinforcementFirstShipTargetEntity, _enemyReinforcementFirstShipTroops, "ship_meditlight_storyline", SecondLightReinforcementShipUpgradePieces);
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)2, isReinforcement: false, _enemySideAgents);
		gameEntity = ((ScriptComponentBehavior)_fahdaMissionShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		_startDistanceToFleePoint = ((Vec2)(ref asVec)).Distance(_fleePoint);
	}

	private MissionShip SpawnReinforcementShip(WeakGameEntity targetEntity, List<StorylineTroop> troops, string shipHullId, Dictionary<string, string> upgradePieces)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		Formation formation = _availailableEnemyFormations.First();
		_availailableEnemyFormations.RemoveAt(0);
		int desiredTroopCount = troops.Sum((StorylineTroop t) => t.TroopCount);
		ShipHull reinforcementShipHull = ((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>(shipHullId);
		Ship val = (Ship)(((object)((IEnumerable<Ship>)PlayerEncounter.EncounteredParty.Ships).FirstOrDefault((Func<Ship, bool>)((Ship x) => x.ShipHull == reinforcementShipHull))) ?? ((object)new Ship(reinforcementShipHull)
		{
			IsTradeable = false,
			IsUsedByQuest = true,
			Owner = PlayerEncounter.EncounteredParty
		}));
		AddShipUpgradePieces(val, upgradePieces);
		MissionShip missionShip = CreateShip((IShipOrigin)(object)val, ((MissionBehavior)this).Mission.PlayerEnemyTeam, formation, targetEntity);
		missionShip.SetCanBeTakenOver(value: false);
		_enemyShips.Add(val);
		((List<MissionShip>)(object)_enemyMissionShips).Add(missionShip);
		_alertedShips[missionShip] = false;
		_navalAgentsLogic.SetDesiredTroopCountOfShip(missionShip, desiredTroopCount);
		SpawnNonHeroAgents(missionShip, troops, encounteredParty, NavalStorylineData.CorsairBanner);
		return missionShip;
	}

	private void SpawnHero(CharacterObject character, MissionShip ship, PartyBase party, Banner customBanner = null)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		Banner banner = customBanner ?? party.Banner;
		character.HeroObject.Heal(character.HeroObject.MaxHitPoints, false);
		PartyAgentOrigin val = new PartyAgentOrigin(party, character, -1, default(UniqueTroopDescriptor), false, true);
		val.SetBanner(banner);
		_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)(object)val, ship);
	}

	private void SpawnNonHeroAgents(MissionShip ship, List<StorylineTroop> troopTypes, PartyBase party, Banner customBanner = null)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		Banner banner = customBanner ?? party.Banner;
		foreach (StorylineTroop troopType in troopTypes)
		{
			CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(troopType.TroopId);
			for (int i = 0; i < troopType.TroopCount; i++)
			{
				PartyAgentOrigin val2 = new PartyAgentOrigin(party, val, -1, default(UniqueTroopDescriptor), false, true);
				val2.SetBanner(banner);
				_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)(object)val2, ship);
			}
		}
	}

	private int GetAgentCountOfSide(BattleSideEnum side)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		BattleSideEnum side2 = ((MissionBehavior)this).Mission.PlayerTeam.Side;
		int num = 0;
		if (side2 == side)
		{
			foreach (MissionShip item in (List<MissionShip>)(object)_playerMissionShips)
			{
				num += _navalAgentsLogic.GetActiveAgentCountOfShip(item);
			}
		}
		else
		{
			foreach (MissionShip item2 in (List<MissionShip>)(object)_enemyMissionShips)
			{
				num += _navalAgentsLogic.GetActiveAgentCountOfShip(item2);
			}
		}
		return num;
	}

	private void GetDefencePositionsForReinforcementShips(out Vec2 leftSide, out Vec2 rightSide, out Vec2 behind)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_laharMissionShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)_fahdaMissionShip).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 val = (asVec - ((Vec3)(ref globalPosition)).AsVec2) / 2f;
		gameEntity = ((ScriptComponentBehavior)_fahdaMissionShip).GameEntity;
		MatrixFrame localFrame = ((WeakGameEntity)(ref gameEntity)).GetLocalFrame();
		Vec2 asVec2 = ((Vec3)(ref localFrame.rotation.f)).AsVec2;
		float num = 300f;
		float num2 = 200f;
		float num3 = MathF.PI / 5f;
		float num4 = MathF.PI * 4f / 5f;
		globalPosition = ((WeakGameEntity)(ref EnemyReinforcementThirdShipTargetEntity)).GlobalPosition;
		behind = ((Vec3)(ref globalPosition)).AsVec2;
		if (((Vec2)(ref asVec2)).AngleBetween(val) < 0f - num3 && ((Vec2)(ref asVec2)).AngleBetween(val) > 0f - num4)
		{
			if (((Vec2)(ref val)).Length == 0f)
			{
				gameEntity = ((ScriptComponentBehavior)_fahdaMissionShip).GameEntity;
				localFrame = ((WeakGameEntity)(ref gameEntity)).GetLocalFrame();
				val = ((Vec3)(ref localFrame.rotation.f)).AsVec2 * 30f;
			}
			else if (((Vec2)(ref val)).Length < num2)
			{
				val *= num2 / ((Vec2)(ref val)).Length;
			}
			else if (((Vec2)(ref val)).Length > num)
			{
				val *= num / ((Vec2)(ref val)).Length;
			}
			gameEntity = ((ScriptComponentBehavior)_fahdaMissionShip).GameEntity;
			globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			rightSide = ((Vec3)(ref globalPosition)).AsVec2 + val;
		}
		else
		{
			globalPosition = ((WeakGameEntity)(ref EnemyReinforcementSecondShipTargetEntity)).GlobalPosition;
			rightSide = ((Vec3)(ref globalPosition)).AsVec2;
		}
		leftSide = rightSide + asVec2 * num2;
	}

	private void AddShipUpgradePieces(Ship ship, Dictionary<string, string> upgradePieces)
	{
		foreach (KeyValuePair<string, string> kv in upgradePieces)
		{
			ShipUpgradePiece val = MBObjectManager.Instance.GetObject<ShipUpgradePiece>(kv.Value);
			if (((IEnumerable<KeyValuePair<string, ShipSlot>>)ship.ShipHull.AvailableSlots).Any((KeyValuePair<string, ShipSlot> slot) => slot.Key == kv.Key))
			{
				ship.EquipUpgradePiece(kv.Key, val);
			}
		}
	}
}
