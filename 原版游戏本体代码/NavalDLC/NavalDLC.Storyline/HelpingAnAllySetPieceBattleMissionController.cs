using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions;
using NavalDLC.Missions.AI.Tactics;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipControl;
using NavalDLC.Missions.ShipInput;
using NavalDLC.Storyline.Objectives.Captivity;
using SandBox;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.Missions.Objectives;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline;

public class HelpingAnAllySetPieceBattleMissionController : MissionLogic, IMissionAgentSpawnLogic, IMissionBehavior
{
	private const string PlayerShipId = "longship_storyline_q1";

	private const string AllyShipId = "ship_trade_cog_q1";

	private const string EnemyShip1Id = "northern_medium_ship";

	private const string EnemyShip2Id = "ship_lightlongship_q1";

	private const string AllyShipTroopType = "vlandian_fortune_seekers";

	private const int AllyShipTroopCount = 12;

	private const int PlayerShipTroopType1Count = 32;

	private const int PlayerShipTroopType2Count = 1;

	private const int EnemyShip1TroopType1Count = 28;

	private const string EnemyShip1TroopType2 = "sea_hounds";

	private const int EnemyShip1TroopType2Count = 2;

	private const int EnemyShip2TroopType1Count = 16;

	private const int EnemyShip2TroopType2Count = 2;

	private const string PlayerShipTroopType1 = "gangradirs_kin_melee";

	private const string PlayerShipTroopType2 = "gangradirs_kin_ranged";

	private const string EnemyShip1TroopType1 = "sea_hounds_pups";

	private MissionShip _playerShip;

	private const string EnemyShip2TroopType1 = "sea_hounds_pups";

	private MissionShip _allyShip;

	private const string EnemyShip2TroopType2 = "sea_hounds";

	private MissionShip _pursuerShip1;

	private const float WindStrength = 2f;

	private const int WayPointCount = 6;

	private const float AiPlayerEngagementDistance = 10f;

	private MissionObjectiveLogic _missionObjectiveLogic;

	private NavalAgentsLogic _agentsLogic;

	private const float ShipAgentsAlarmDistance = 30f;

	private const float DefeatFadeOutDelayDuration = 2f;

	private const float DefeatFadeOutDuration = 1f;

	private const float DefeatBlackScreenDuration = 2f;

	private static readonly Dictionary<string, string> PlayerShipUpgradePieces = new Dictionary<string, string>
	{
		{ "oars", "oars_wide_lvl3" },
		{ "sail", "sails_lvl2" },
		{ "side", "side_northern_shields_lvl2" }
	};

	private static readonly Dictionary<string, string> AllyShipUpgradePieces = new Dictionary<string, string>
	{
		{ "oars", "oars_wide_lvl3" },
		{ "sail", "sails_lvl2" }
	};

	private static readonly Dictionary<string, string> Enemy1ShipUpgradePieces = new Dictionary<string, string>
	{
		{ "sail", "sails_lvl2" },
		{ "side", "side_northern_shields_lvl1" }
	};

	private static readonly Dictionary<string, string> Enemy2ShipUpgradePieces = new Dictionary<string, string>
	{
		{ "sail", "sails_lvl2" },
		{ "side", "side_northern_shields_lvl1" }
	};

	private List<GameEntity> _entities = new List<GameEntity>();

	private MobileParty _merchantParty;

	private MobileParty _seaHoundsParty;

	private MissionShip _pursuerShip2;

	private List<GameEntity> _waypoints = new List<GameEntity>();

	private bool _isAllyBoardedNotificationGiven;

	private int _currentWaypointIndex;

	private bool _isMissionInitialized;

	private bool _isMissionSuccessful;

	private bool _isAllyAboutToBeBoardedNotificationGiven;

	private bool _hasPlayerEngagedEnemyNotificationGiven;

	private bool _hasPlayerClearedFirstEnemyNotificationGiven;

	private bool _hasPlayerClearedSecondEnemyNotificationGiven;

	private bool _isPursuer1ShipEngaged;

	private bool _isMissionFailed;

	private bool _isPursuer2ShipEngaged;

	private float _drownCheckTimer;

	private float _drownCheckDuration = 3f;

	private bool _isVictoryQueued;

	private float _victoryPopUpTimer;

	private float _victoryPopUpDelay = 3f;

	private bool _isDefeatQueued;

	private bool _isFadeOutTriggered;

	private float _defeatTimer;

	private float _notificationTimer;

	public Action OnShipsInitializedEvent;

	public Action<float> OnDefeatedEvent;

	public HelpingAnAllySetPieceBattleMissionController(MobileParty merchantParty, MobileParty seaHoundsParty)
	{
		_merchantParty = merchantParty;
		_seaHoundsParty = seaHoundsParty;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Expected O, but got Unknown
		//IL_065a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0671: Expected O, but got Unknown
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Expected O, but got Unknown
		if (!_isMissionInitialized)
		{
			_isMissionInitialized = true;
			UpdateEntityReferences();
			_agentsLogic = Mission.Current.GetMissionBehavior<NavalAgentsLogic>();
			NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
			_agentsLogic.SetDeploymentMode(value: true);
			missionBehavior.SetDeploymentMode(value: true);
			missionBehavior.SetTeamShipDeploymentLimit((TeamSideEnum)0, NavalShipDeploymentLimit.Max());
			missionBehavior.SetTeamShipDeploymentLimit((TeamSideEnum)1, NavalShipDeploymentLimit.Max());
			missionBehavior.SetTeamShipDeploymentLimit((TeamSideEnum)2, NavalShipDeploymentLimit.Max());
			Team team = Mission.GetTeam((TeamSideEnum)0);
			Formation formation = team.GetFormation((FormationClass)0);
			team.SetPlayerRole(true, true);
			Formation formation2 = Mission.GetTeam((TeamSideEnum)1).GetFormation((FormationClass)0);
			Team team2 = Mission.GetTeam((TeamSideEnum)2);
			Formation formation3 = team2.GetFormation((FormationClass)0);
			Formation formation4 = team2.GetFormation((FormationClass)1);
			_playerShip = CreateShip("longship_storyline_q1", "player_ship_sp", formation, PartyBase.MainParty, "generated_square__h4_09", DefaultFigureheads.Dragon, PlayerShipUpgradePieces);
			missionBehavior?.TeleportShip(_playerShip, _playerShip.GlobalFrame, checkFreeArea: false);
			Scene scene = Mission.Current.Scene;
			MatrixFrame globalFrame = _playerShip.GlobalFrame;
			Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			scene.SetGlobalWindVelocity(ref val);
			_allyShip = CreateShip("ship_trade_cog_q1", "ally_ship_sp", formation2, _merchantParty.Party, "generated_square_l1_h4_04", null, AllyShipUpgradePieces);
			missionBehavior?.TeleportShip(_allyShip, _allyShip.GlobalFrame, checkFreeArea: false);
			_pursuerShip1 = CreateShip("northern_medium_ship", "sea_hound_ship_1_sp", formation3, _seaHoundsParty.Party, "generated_square_l1_h4_10", DefaultFigureheads.Viper, Enemy1ShipUpgradePieces);
			_pursuerShip2 = CreateShip("ship_lightlongship_q1", "sea_hound_ship_2_sp", formation4, _seaHoundsParty.Party, "generated_square_l1_h4_10", DefaultFigureheads.Ram, Enemy2ShipUpgradePieces);
			missionBehavior?.TeleportShip(_pursuerShip1, _pursuerShip1.GlobalFrame, checkFreeArea: false);
			missionBehavior?.TeleportShip(_pursuerShip2, _pursuerShip2.GlobalFrame, checkFreeArea: false);
			((MissionBehavior)this).Mission.DefenderTeam.TeamAI.ClearTacticOptions();
			((MissionBehavior)this).Mission.DefenderTeam.AddTacticOption((TacticComponent)(object)new TacticNavalLineDefense(((MissionBehavior)this).Mission.DefenderTeam));
			((MissionBehavior)this).Mission.AttackerTeam.TeamAI.ClearTacticOptions();
			((MissionBehavior)this).Mission.AttackerTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(((MissionBehavior)this).Mission.AttackerTeam));
			_playerShip.SetController(ShipControllerType.Player);
			_playerShip.SetAnchor(isAnchored: false);
			missionBehavior?.SetCanHaveConnectionCooldown(value: false);
			_pursuerShip1.SetController(ShipControllerType.AI);
			_pursuerShip2.SetController(ShipControllerType.AI);
			_pursuerShip1.ShipOrder.SetShipEngageOrder(_allyShip);
			_pursuerShip2.ShipOrder.SetShipEngageOrder(_allyShip);
			_pursuerShip1.SetShipOrderActive(isOrderActive: true);
			_pursuerShip2.SetShipOrderActive(isOrderActive: true);
			_pursuerShip1.SetCanBeTakenOver(value: false);
			_pursuerShip2.SetCanBeTakenOver(value: false);
			_allyShip.SetShipOrderActive(isOrderActive: true);
			UpdateEntityReferences();
			SpawnPlayer();
			formation.PlayerOwner = Agent.Main;
			SpawnPlayerShipAgents();
			SpawnAllyShipAgents(_allyShip);
			SpawnEnemyAgents(_pursuerShip1, "sea_hounds_pups", 28, "sea_hounds", 2);
			SpawnEnemyAgents(_pursuerShip2, "sea_hounds_pups", 16, "sea_hounds", 2);
			team.SetPlayerRole(true, true);
			foreach (Team item2 in (List<Team>)(object)Mission.Current.Teams)
			{
				item2.MasterOrderController.SelectAllFormations(false);
				item2.MasterOrderController.SetOrder((OrderType)34);
				item2.MasterOrderController.ClearSelectedFormations();
			}
			int i;
			for (i = 1; i <= 6; i++)
			{
				GameEntity item = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("volume_box_" + i)));
				_waypoints.Add(item);
			}
			_agentsLogic.AssignCaptainToShipForDeploymentMode(Agent.Main, _playerShip);
			_agentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)0);
			_agentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)1);
			_agentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)2);
			Mission.Current.OnDeploymentFinished();
			_agentsLogic.SetDeploymentMode(value: false);
			missionBehavior.SetDeploymentMode(value: false);
			Scene scene2 = Mission.Current.Scene;
			globalFrame = _entities.First((GameEntity t) => t.HasTag("sp_wind")).GetGlobalFrame();
			val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			val = ((Vec2)(ref val)).Normalized() * 2f;
			scene2.SetGlobalWindStrengthVector(ref val);
			CampaignInformationManager.AddDialogLine(new TextObject("{=FkFpeYSI}Look - there's two of them giving chase. We'll have to take one down quickly, and hope the Vlandians can hold the other off until we reach them.", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
			HelpingAnAllyMissionObjective helpingAnAllyMissionObjective = new HelpingAnAllyMissionObjective(Mission.Current);
			_missionObjectiveLogic.StartObjective((MissionObjective)(object)helpingAnAllyMissionObjective);
			_playerShip.SetCustomSailSetting(enableCustomSailSetting: false, SailInput.Raised);
			OnShipsInitializedEvent();
		}
		HandleShipOrders();
		_drownCheckTimer += dt;
		if (_drownCheckTimer >= _drownCheckDuration)
		{
			_drownCheckTimer = 0f;
			CheckDrowningAgents(_pursuerShip1);
			CheckDrowningAgents(_pursuerShip2);
		}
		if (_isVictoryQueued)
		{
			_victoryPopUpTimer += dt;
			if (_victoryPopUpTimer >= _victoryPopUpDelay)
			{
				_isVictoryQueued = false;
				OpenVictoryPopUp();
			}
		}
		if (_isDefeatQueued)
		{
			_defeatTimer += dt;
			if (!_isFadeOutTriggered && _defeatTimer >= 2f)
			{
				_isFadeOutTriggered = true;
				StartDefeatFadeOut();
			}
			if (_defeatTimer >= 5f)
			{
				_isDefeatQueued = false;
				OnMissionFailed();
			}
		}
		if (!_playerShip.GetIsConnected())
		{
			_notificationTimer += dt;
			if (!(_notificationTimer > 10f))
			{
				return;
			}
			_notificationTimer = 0f;
			if (HasSailThrust())
			{
				if (_playerShip.SailTargetSetting < 1f)
				{
					CampaignInformationManager.AddDialogLine(new TextObject("{=cGay4oWJ}The wind is with us. Should we unfurl the sail?", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
				}
			}
			else if (_playerShip.SailTargetSetting > 0f)
			{
				CampaignInformationManager.AddDialogLine(new TextObject("{=IpjMuSVa}The wind is blowing against us. Best furl the sail.", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
			}
		}
		else
		{
			_notificationTimer = 0f;
		}
	}

	public override void OnBehaviorInitialize()
	{
		if (!SailWindProfile.IsSailWindProfileInitialized)
		{
			SailWindProfile.InitializeProfile();
		}
		_missionObjectiveLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionObjectiveLogic>();
	}

	private void UpdateEntityReferences()
	{
		((MissionBehavior)this).Mission.Scene.GetEntities(ref _entities);
	}

	private MissionShip CreateShip(string shipHullId, string spawnPointId, Formation formation, PartyBase owner, string materialName, Figurehead figurehead = null, Dictionary<string, string> upgradePieces = null)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		Ship val = new Ship(((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>(shipHullId));
		if (figurehead != null)
		{
			val.ChangeFigurehead(figurehead);
		}
		if (upgradePieces != null)
		{
			foreach (KeyValuePair<string, string> upgradePiece in upgradePieces)
			{
				if (val.HasSlot(upgradePiece.Key))
				{
					val.EquipUpgradePiece(upgradePiece.Key, MBObjectManager.Instance.GetObject<ShipUpgradePiece>(upgradePiece.Value));
				}
			}
		}
		MissionShip missionShip = CreateMissionShip(val, spawnPointId, formation);
		if (owner.MobileParty.IsBandit)
		{
			ChangeShipColors(missionShip, NavalStorylineData.CorsairBanner.GetPrimaryColor(), NavalStorylineData.CorsairBanner.GetSecondaryColor(), materialName);
		}
		else
		{
			ChangeShipColors(missionShip, owner.MapFaction.Color, owner.MapFaction.Color2, materialName);
		}
		return missionShip;
	}

	private void ChangeShipColors(MissionShip missionShip, uint color1, uint color2, string materialName)
	{
		foreach (GameEntity item in (List<GameEntity>)(object)missionShip.SailMeshEntities)
		{
			SetSailColors(item, color1, color2, materialName);
		}
	}

	private void SetSailColors(GameEntity sailEntity, uint sailColor1, uint sailColor2, string materialName)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)sailEntity.Skeleton != (NativeObject)null)
		{
			foreach (Mesh allMesh in sailEntity.Skeleton.GetAllMeshes())
			{
				if (allMesh.HasTag("faction_color"))
				{
					Material fromResource = Material.GetFromResource(materialName);
					if ((NativeObject)(object)fromResource != (NativeObject)null)
					{
						allMesh.SetMaterial(fromResource);
					}
					allMesh.Color = sailColor1;
					allMesh.Color2 = sailColor2;
				}
			}
		}
		WeakGameEntity weakEntity = sailEntity.WeakEntity;
		foreach (Mesh item in ((WeakGameEntity)(ref weakEntity)).GetAllMeshesWithTag("faction_color"))
		{
			item.Color = sailColor1;
			item.Color2 = sailColor2;
		}
	}

	private void OnShipsEngaged(MissionShip ship1, MissionShip ship2)
	{
		int activeAgentCountOfShip = _agentsLogic.GetActiveAgentCountOfShip(ship1);
		int activeAgentCountOfShip2 = _agentsLogic.GetActiveAgentCountOfShip(ship2);
		if (activeAgentCountOfShip > 0 && activeAgentCountOfShip2 > 0)
		{
			ship1.ShipOrder.SetShipEngageOrder(ship2);
			ship2.ShipOrder.SetShipEngageOrder(ship1);
			AddFightBehaviors(ship1);
			AddFightBehaviors(ship2);
		}
	}

	private void HandleShipOrders()
	{
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Expected O, but got Unknown
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Expected O, but got Unknown
		if (AreShipsWithinDistance(_pursuerShip1, _playerShip, 30f))
		{
			OnShipsEngaged(_pursuerShip1, _playerShip);
			_isPursuer1ShipEngaged = true;
		}
		else if (_isPursuer1ShipEngaged)
		{
			CalmAgentsOfShip(_playerShip);
			CalmAgentsOfShip(_pursuerShip1);
			_isPursuer1ShipEngaged = false;
		}
		if (AreShipsWithinDistance(_pursuerShip2, _playerShip, 30f))
		{
			OnShipsEngaged(_pursuerShip2, _playerShip);
			_isPursuer2ShipEngaged = true;
		}
		else if (_isPursuer2ShipEngaged)
		{
			CalmAgentsOfShip(_playerShip);
			CalmAgentsOfShip(_pursuerShip2);
			_isPursuer2ShipEngaged = false;
		}
		if (AreShipsWithinDistance(_pursuerShip1, _allyShip, 30f))
		{
			OnShipsEngaged(_pursuerShip1, _allyShip);
			OnMerchantsAboutToBeBoarded();
		}
		if (AreShipsWithinDistance(_pursuerShip2, _allyShip, 30f))
		{
			OnShipsEngaged(_pursuerShip2, _allyShip);
			OnMerchantsAboutToBeBoarded();
		}
		if (AreShipsWithinDistance(_pursuerShip1, _playerShip, 10f))
		{
			_pursuerShip1.ShipOrder.SetShipEngageOrder(_playerShip);
			_pursuerShip1.ShipOrder.SetBoardingTargetShip(_playerShip);
		}
		else if (!_pursuerShip1.GetIsConnected())
		{
			_pursuerShip1.ShipOrder.SetShipEngageOrder(_allyShip);
			_pursuerShip1.ShipOrder.SetBoardingTargetShip(_allyShip);
		}
		if (AreShipsWithinDistance(_pursuerShip2, _playerShip, 10f))
		{
			_pursuerShip2.ShipOrder.SetShipEngageOrder(_playerShip);
			_pursuerShip2.ShipOrder.SetBoardingTargetShip(_playerShip);
		}
		else if (!_pursuerShip2.GetIsConnected())
		{
			_pursuerShip2.ShipOrder.SetShipEngageOrder(_allyShip);
			_pursuerShip2.ShipOrder.SetBoardingTargetShip(_allyShip);
		}
		GameEntity val = _waypoints[_currentWaypointIndex];
		Vec3 val2 = val.GlobalPosition - _allyShip.GlobalFrame.origin;
		if (((Vec3)(ref val2)).LengthSquared <= 10000f)
		{
			_currentWaypointIndex = (_currentWaypointIndex + 1) % 6;
		}
		ShipOrder shipOrder = _allyShip.ShipOrder;
		val2 = val.GlobalPosition;
		shipOrder.SetShipMovementOrder(((Vec3)(ref val2)).AsVec2);
		if (!_isAllyBoardedNotificationGiven && (_allyShip.GetIsThereActiveBridgeTo(_pursuerShip1) || _allyShip.GetIsThereActiveBridgeTo(_pursuerShip2)))
		{
			_isAllyBoardedNotificationGiven = true;
			CampaignInformationManager.AddDialogLine(new TextObject("{=J83UkY9F}They’re boarding the Vlandians!", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}
		if (!_hasPlayerEngagedEnemyNotificationGiven && (_playerShip.GetIsThereActiveBridgeTo(_pursuerShip1) || _playerShip.GetIsThereActiveBridgeTo(_pursuerShip2)))
		{
			_hasPlayerEngagedEnemyNotificationGiven = true;
			CampaignInformationManager.AddDialogLine(new TextObject("{=LABFnNwV}The grapples have caught. Cut them down!", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}
	}

	private void OnMerchantsAboutToBeBoarded()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		if (!_isAllyAboutToBeBoardedNotificationGiven)
		{
			_isAllyAboutToBeBoardedNotificationGiven = true;
			CampaignInformationManager.AddDialogLine(new TextObject("{=Iy0a0ucw}I think the Vlandians are about to be overtaken and boarded.", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}
	}

	private MissionShip CreateMissionShip(Ship ship, string spawnPointId, Formation formation)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag(spawnPointId)));
		MatrixFrame shipFrame = val.GetGlobalFrame();
		Scene scene = Mission.Current.Scene;
		Vec3 globalPosition = val.GlobalPosition;
		float waterLevelAtPosition = scene.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, true, false);
		shipFrame.origin = new Vec3(val.GlobalPosition.x, val.GlobalPosition.y, waterLevelAtPosition, -1f);
		return missionBehavior.SpawnShip((IShipOrigin)(object)ship, in shipFrame, formation.Team, formation, spawnAnchored: false, (FormationClass)8);
	}

	private void SpawnPlayer()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_playerShip).GameEntity;
		WeakGameEntity val = ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("sp_troop_captain").FirstOrDefault();
		Formation formation = ((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)0);
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)Hero.MainHero.CharacterObject).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)Hero.MainHero.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = ((WeakGameEntity)(ref val)).GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val2 = obj2.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false)
			.Formation(formation);
		Mission.Current.SpawnAgent(val2, false).Controller = (AgentControllerType)2;
		_agentsLogic.AddAgentToShip(Agent.Main, _playerShip);
	}

	private void SpawnPlayerShipAgents()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		NavalAgentsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		int num = 33;
		missionBehavior.SetDesiredTroopCountOfShip(_playerShip, num + 1);
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("gangradirs_kin_melee");
		CharacterObject val2 = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("gangradirs_kin_ranged");
		int deckFrameCount = _playerShip.DeckFrameCount;
		for (int i = 0; i < deckFrameCount && i < num; i++)
		{
			CharacterObject val3 = val;
			if (i >= 32)
			{
				val3 = val2;
			}
			MatrixFrame nextOuterInnerSpawnGlobalFrame = _playerShip.GetNextOuterInnerSpawnGlobalFrame();
			AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val3).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val3, -1, default(UniqueTroopDescriptor), false, true)).Team(((MissionBehavior)this).Mission.PlayerTeam).InitialPosition(ref nextOuterInnerSpawnGlobalFrame.origin);
			Vec2 val4 = ((Vec3)(ref nextOuterInnerSpawnGlobalFrame.rotation.f)).AsVec2;
			val4 = ((Vec2)(ref val4)).Normalized();
			AgentBuildData val5 = obj.InitialDirection(ref val4).NoHorses(true).NoWeapons(false);
			Agent agent = Mission.Current.SpawnAgent(val5, false);
			missionBehavior.AddAgentToShip(agent, _playerShip);
		}
	}

	private void SpawnEnemyAgents(MissionShip ship, string troopType1, int troopType1Count, string troopType2, int troopType2Count)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		int num = troopType1Count + troopType2Count;
		NavalAgentsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		missionBehavior.SetDesiredTroopCountOfShip(ship, num);
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(troopType1);
		CharacterObject val2 = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(troopType2);
		int deckFrameCount = ship.DeckFrameCount;
		for (int i = 0; i < deckFrameCount; i++)
		{
			CharacterObject val3 = val;
			if (i < num)
			{
				if (i >= troopType1Count)
				{
					val3 = val2;
				}
				MatrixFrame nextOuterInnerSpawnGlobalFrame = ship.GetNextOuterInnerSpawnGlobalFrame();
				AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val3).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(_seaHoundsParty.Party, val3, -1, default(UniqueTroopDescriptor), false, true)).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam).InitialPosition(ref nextOuterInnerSpawnGlobalFrame.origin);
				Vec2 val4 = ((Vec3)(ref nextOuterInnerSpawnGlobalFrame.rotation.f)).AsVec2;
				val4 = ((Vec2)(ref val4)).Normalized();
				AgentBuildData val5 = obj.InitialDirection(ref val4).NoHorses(true).NoWeapons(false);
				Agent agent = Mission.Current.SpawnAgent(val5, false);
				missionBehavior.AddAgentToShip(agent, ship);
				continue;
			}
			break;
		}
	}

	private void SpawnAllyShipAgents(MissionShip ship)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		NavalAgentsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		missionBehavior.SetDesiredTroopCountOfShip(ship, 12);
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("vlandian_fortune_seekers");
		int deckFrameCount = ship.DeckFrameCount;
		for (int i = 0; i < deckFrameCount && i < 12; i++)
		{
			MatrixFrame nextOuterInnerSpawnGlobalFrame = ship.GetNextOuterInnerSpawnGlobalFrame();
			AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(_merchantParty.Party, val, -1, default(UniqueTroopDescriptor), false, true)).Team(((MissionBehavior)this).Mission.PlayerAllyTeam).InitialPosition(ref nextOuterInnerSpawnGlobalFrame.origin);
			Vec2 val2 = ((Vec3)(ref nextOuterInnerSpawnGlobalFrame.rotation.f)).AsVec2;
			val2 = ((Vec2)(ref val2)).Normalized();
			AgentBuildData val3 = obj.InitialDirection(ref val2).NoHorses(true).NoWeapons(false);
			Agent agent = Mission.Current.SpawnAgent(val3, false);
			missionBehavior.AddAgentToShip(agent, ship);
			ship.Formation.PlayerOwner = Agent.Main;
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		if (_isMissionFailed || _isMissionSuccessful || (int)Mission.Current.CurrentState != 2)
		{
			return;
		}
		if (_isPursuer1ShipEngaged && _agentsLogic.GetActiveAgentCountOfShip(_pursuerShip1) == 0)
		{
			CalmAgentsOfShip(_playerShip);
			_isPursuer1ShipEngaged = false;
			if (!_hasPlayerClearedFirstEnemyNotificationGiven)
			{
				_hasPlayerClearedFirstEnemyNotificationGiven = true;
				CampaignInformationManager.AddDialogLine(new TextObject("{=Xjm7x5vu}Hah! That's the end of them! Now, about the other one…", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
			}
		}
		if (_isPursuer2ShipEngaged && _agentsLogic.GetActiveAgentCountOfShip(_pursuerShip2) == 0)
		{
			CalmAgentsOfShip(_playerShip);
			_isPursuer2ShipEngaged = false;
			if (!_hasPlayerClearedSecondEnemyNotificationGiven)
			{
				_hasPlayerClearedSecondEnemyNotificationGiven = true;
				CampaignInformationManager.AddDialogLine(new TextObject("{=2lX2bIwy}That's the last of them!", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
			}
		}
		MBReadOnlyList<Agent> activeAgents = ((MissionBehavior)this).Mission.PlayerAllyTeam.ActiveAgents;
		if (activeAgents != null && !_isDefeatQueued && !_isVictoryQueued)
		{
			if ((float)((List<Agent>)(object)activeAgents).Count <= 3.6000001f || Extensions.IsEmpty<Agent>((IEnumerable<Agent>)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents))
			{
				StartDefeatSequence();
			}
			else if (((List<Agent>)(object)activeAgents).Count == 6)
			{
				CampaignInformationManager.AddDialogLine(new TextObject("{=zdQoMBZd}Most of the Vlandians are down! We haven't much time!", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
			}
		}
		if (_isMissionSuccessful)
		{
			return;
		}
		MBReadOnlyList<Agent> activeAgentsOfShip = _agentsLogic.GetActiveAgentsOfShip(_pursuerShip1);
		MBReadOnlyList<Agent> activeAgentsOfShip2 = _agentsLogic.GetActiveAgentsOfShip(_pursuerShip2);
		if (activeAgentsOfShip != null && activeAgentsOfShip2 != null)
		{
			IEnumerable<Agent> enumerable = ((IEnumerable<Agent>)activeAgentsOfShip).Where((Agent t) => t.Team == ((MissionBehavior)this).Mission.PlayerEnemyTeam);
			IEnumerable<Agent> enumerable2 = ((IEnumerable<Agent>)activeAgentsOfShip2).Where((Agent t) => t.Team == ((MissionBehavior)this).Mission.PlayerEnemyTeam);
			if (Extensions.IsEmpty<Agent>(enumerable) && Extensions.IsEmpty<Agent>(enumerable2))
			{
				OnAllPursuingShipsDefeated();
			}
		}
	}

	private void CheckDrowningAgents(MissionShip ship)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent item in ((IEnumerable<Agent>)_agentsLogic.GetActiveAgentsOfShip(ship)).ToList())
		{
			if (!item.IsMainAgent && item.Team != ((MissionBehavior)this).Mission.PlayerTeam && (int)item.CurrentMortalityState == 0 && item.IsInWater())
			{
				item.GetComponent<AgentNavalComponent>()?.DrownAgent();
			}
		}
	}

	private void CalmAgentsOfShip(MissionShip targetShip)
	{
		foreach (Agent item in (List<Agent>)(object)_agentsLogic.GetActiveAgentsOfShip(targetShip))
		{
			item.SetAlarmState((AIStateFlag)0);
			AgentNavigator agentNavigator = item.GetComponent<CampaignAgentComponent>().AgentNavigator;
			if (agentNavigator != null)
			{
				agentNavigator.RemoveBehaviorGroup<AlarmedBehaviorGroup>();
			}
		}
	}

	private bool AreShipsWithinDistance(MissionShip ship1, MissionShip ship2, float distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = ship1.GlobalFrame.origin - ship2.GlobalFrame.origin;
		return ((Vec3)(ref val)).LengthSquared <= distance * distance;
	}

	private void OnAllPursuingShipsDefeated()
	{
		_playerShip.ShipOrder.SetShipStopOrder();
		_allyShip.ShipOrder.SetShipStopOrder();
		_isVictoryQueued = true;
	}

	private void OpenVictoryPopUp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0028: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		TextObject val = new TextObject("{=R4Gqskgq}Victory", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=p0HTLZzH}After the last Sea Hound is defeated, the merchants approach you...", (Dictionary<string, object>)null);
		TextObject val3 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)val3).ToString(), (string)null, (Action)OnVictoryPopUpClosed, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void OnVictoryPopUpClosed()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_isMissionSuccessful = true;
		PlayerEncounter.Battle.SetOverrideWinner(PlayerEncounter.Battle.PlayerSide);
		((MissionBehavior)this).Mission.EndMission();
	}

	private void StartDefeatSequence()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		_isDefeatQueued = true;
		MBInformationManager.AddQuickInformation(new TextObject("{=fhEaEedK}Vlandian merchants have been destroyed.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
	}

	private void StartDefeatFadeOut()
	{
		OnDefeatedEvent(1f);
	}

	private void OnMissionFailed()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		_isMissionFailed = true;
		PlayerEncounter.Battle.SetOverrideWinner(PlayerEncounter.Battle.GetOtherSide(PlayerEncounter.Battle.PlayerSide));
		ScreenFadeController.BeginFadeIn(0.5f);
		((MissionBehavior)this).Mission.EndMission();
	}

	private void AddFightBehaviors(MissionShip ship)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent item in (List<Agent>)(object)_agentsLogic.GetActiveAgentsOfShip(ship))
		{
			AgentFlag agentFlags = item.GetAgentFlags();
			item.SetAgentFlags((AgentFlag)(agentFlags | 0x10000));
			CampaignAgentComponent component = item.GetComponent<CampaignAgentComponent>();
			AgentNavigator val = component.AgentNavigator;
			if (val == null)
			{
				val = component.CreateAgentNavigator();
			}
			AlarmedBehaviorGroup val2 = val.GetBehaviorGroup<AlarmedBehaviorGroup>();
			if (val2 == null)
			{
				val2 = val.AddBehaviorGroup<AlarmedBehaviorGroup>();
				((AgentBehaviorGroup)val2).AddBehavior<FightBehavior>();
			}
			((AgentBehaviorGroup)val2).SetScriptedBehavior<FightBehavior>();
			item.SetAlarmState((AIStateFlag)3);
		}
	}

	private bool HasSailThrust()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		Vec2 globalWindVelocity = ((MissionBehavior)this).Mission.Scene.GetGlobalWindVelocity();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_playerShip).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		ref Mat3 rotation = ref globalFrame.rotation;
		Vec3 val = ((Vec2)(ref globalWindVelocity)).ToVec3(0f);
		Vec3 val2 = ((Mat3)(ref rotation)).TransformToLocal(ref val);
		Vec2 asVec = ((Vec3)(ref val2)).AsVec2;
		Vec2 windDir = ((Vec2)(ref asVec)).Normalized();
		MBReadOnlyList<MissionSail> sails = _playerShip.Sails;
		float num = 0f;
		foreach (MissionSail item in (List<MissionSail>)(object)sails)
		{
			float num2 = 0f - item.SailObject.RightRotationLimit;
			float leftRotationLimit = item.SailObject.LeftRotationLimit;
			float num3 = (leftRotationLimit - num2) * 0.01f;
			for (float num4 = num2; num4 <= leftRotationLimit; num4 += num3)
			{
				Vec2 forward = Vec2.Forward;
				((Vec2)(ref forward)).RotateCCW(num4);
				num += SailWindProfile.Instance.ComputeSailThrustValue(item.SailObject.Type, forward, Vec2.Forward, windDir);
			}
		}
		return num > 0.1f;
	}

	public override bool MissionEnded(ref MissionResult missionResult)
	{
		bool result = false;
		if (_isMissionSuccessful)
		{
			missionResult = MissionResult.CreateSuccessful((IMission)(object)((MissionBehavior)this).Mission, true);
			result = true;
		}
		else if (_isMissionFailed)
		{
			missionResult = MissionResult.CreateDefeated((IMission)(object)((MissionBehavior)this).Mission);
			result = true;
		}
		return result;
	}

	public void StartSpawner(BattleSideEnum side)
	{
	}

	public void StopSpawner(BattleSideEnum side)
	{
	}

	public bool IsSideSpawnEnabled(BattleSideEnum side)
	{
		return false;
	}

	public float GetReinforcementInterval()
	{
		return 0f;
	}

	public bool IsSideDepleted(BattleSideEnum side)
	{
		return false;
	}

	public IEnumerable<IAgentOriginBase> GetAllTroopsForSide(BattleSideEnum side)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if ((int)side == 1)
		{
			return ((IEnumerable<Agent>)Mission.Current.PlayerEnemyTeam.ActiveAgents).Select((Agent t) => t.Origin);
		}
		if ((int)side == 0)
		{
			List<IAgentOriginBase> list = new List<IAgentOriginBase>();
			list.AddRange(((IEnumerable<Agent>)Mission.Current.PlayerTeam.ActiveAgents).Select((Agent t) => t.Origin));
			list.AddRange(((IEnumerable<Agent>)Mission.Current.PlayerAllyTeam.ActiveAgents).Select((Agent t) => t.Origin));
			return list;
		}
		return null;
	}

	public int GetNumberOfPlayerControllableTroops()
	{
		return 1;
	}

	public bool GetSpawnHorses(BattleSideEnum side)
	{
		return false;
	}
}
