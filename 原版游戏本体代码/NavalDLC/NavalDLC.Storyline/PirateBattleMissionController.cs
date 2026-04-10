using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipControl;
using NavalDLC.Storyline.Objectives.PirateBattle;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.Missions.Objectives;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline;

public class PirateBattleMissionController : MissionLogic
{
	private const int InitialAllyMeleeTroopCount = 10;

	private const int InitialAllyRangedTroopCount = 10;

	private const int SecondPhaseMinTotalAllyTroopCount = 14;

	private const int SecondPhasePrisonerMeleeTroopCount = 7;

	private const int SecondPhasePrisonerRangedTroopCount = 7;

	private const float AfterFightShipChangeDuration = 0.5f;

	private const float BoardingImminentRadiusSqr = 2500f;

	private const float NotificationRepeatDuration = 27f;

	private const string AllyMeleeTroopStringId = "gangradirs_kin_melee";

	private const string AllyRangedTroopStringId = "gangradirs_kin_ranged";

	private const string EnemyTroopStringId = "sea_hounds_pups";

	private const float MissionStateChangeTimer = 3f;

	private const float WindStrength = 1.5f;

	private const float FadeDuration = 0.5f;

	private const float BlackScreenDuration = 0.75f;

	private static readonly Dictionary<string, string> PlayerShipUpgradePieces = new Dictionary<string, string> { { "sail", "sails_lvl2" } };

	private static readonly Dictionary<string, string> SecondShipUpgradePieces = new Dictionary<string, string> { { "sail", "sails_lvl2" } };

	private static readonly Dictionary<string, string> ReinforcementShipUpgradePieces = new Dictionary<string, string> { { "sail", "sails_lvl2" } };

	private bool _isMissionInitialized;

	private List<GameEntity> _entities = new List<GameEntity>();

	private Agent _gunnarAgent;

	private MissionShip _playerShip;

	private MissionShip _secondShip;

	private MissionShip _reinforcementShip;

	private readonly MobileParty _pirateParty;

	private MissionTimer _victoryTimer;

	private MissionTimer _defeatTimer;

	private float _notificationTimer = 15f;

	private TextObject _currentNotificationText;

	private bool _isInSecondPhase;

	private bool _isMissionSuccessful;

	private bool _isMissionFailed;

	private bool _hasShownChargeNotification;

	private bool _hasShownSecondPhaseChargeNotification;

	private bool _hasShownBoardImminentNotification;

	private bool _hasIncreasedMusicIntensityForSecondPhase;

	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	private MissionObjectiveLogic _missionObjectiveLogic;

	private bool _isGunnarAfterFightFirstNotificationShown;

	private bool _isGunnarAfterFightSecondNotificationShown;

	private float _afterFightShipChangeTimer;

	private bool _isShipTransferQueued;

	private bool _isSecondShipSelected;

	private readonly int _pirateTroopCount;

	private bool _isDialogueQueued;

	private bool _isSecondPhaseSetup;

	private float _dialogueTimer;

	public bool IsFirstShipCleared { get; private set; }

	public bool HasSelectedShip { get; private set; }

	public event Action<float, float> OnBeginScreenFadeEvent;

	public event Action<float> OnCameraBearingNeedsUpdateEvent;

	public event Action OnShipsInitializedEvent;

	public PirateBattleMissionController(MobileParty pirateParty, int pirateTroopCount)
	{
		_pirateParty = pirateParty;
		_pirateTroopCount = pirateTroopCount;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Expected O, but got Unknown
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Expected O, but got Unknown
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Expected O, but got Unknown
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Expected O, but got Unknown
		Vec3 val;
		if (!_isMissionInitialized)
		{
			_isMissionInitialized = true;
			UpdateEntityReferences();
			Team team = Mission.GetTeam((TeamSideEnum)0);
			Formation formation = team.GetFormation((FormationClass)0);
			Formation formation2 = Mission.GetTeam((TeamSideEnum)2).GetFormation((FormationClass)0);
			_playerShip = CreateShip("ship_knarr_storyline_2", "spawnpoint_ship_player", formation, PartyBase.MainParty, PlayerShipUpgradePieces, "generated_square__h4_09");
			_secondShip = CreateShip("ship_lightlongship_storyline", "spawnpoint_ship_first_enemy", formation2, _pirateParty.Party, SecondShipUpgradePieces, "generated_square_l1_h4_10");
			_navalShipsLogic.TeleportShip(_playerShip, _playerShip.GlobalFrame, checkFreeArea: false);
			_navalShipsLogic.TeleportShip(_secondShip, _secondShip.GlobalFrame, checkFreeArea: false);
			UpdateEntityReferences();
			SpawnAllyTroops();
			SpawnEnemyAgents(_secondShip);
			team.SetPlayerRole(true, true);
			_navalAgentsLogic.AssignCaptainToShipForDeploymentMode(Agent.Main, _playerShip);
			_playerShip.ShipOrder.SetOrderOarsmenLevel(2);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)0);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)2);
			Mission.Current.OnDeploymentFinished();
			_secondShip.SetAnchor(isAnchored: true);
			_secondShip.ShipOrder.SetShipStopOrder();
			_secondShip.SetController(ShipControllerType.None, autoUpdateController: false);
			_secondShip.Formation.SetControlledByAI(false, false);
			_secondShip.SetCanBeTakenOver(value: false);
			TextObject text = new TextObject("{=xz5vyQlF}They must think we're just a fishing vessel. All right now, boys, let's show them that their prey has teeth of its own!", (Dictionary<string, object>)null);
			ShowNotification(text);
			PirateBattlePhase1Objective pirateBattlePhase1Objective = new PirateBattlePhase1Objective(Mission.Current, this);
			_missionObjectiveLogic.StartObjective((MissionObjective)(object)pirateBattlePhase1Objective);
			Mission.Current.PlayerTeam.PlayerOrderController.OnOrderIssued += new OnOrderIssuedDelegate(OnPlayerOrdered);
			this.OnShipsInitializedEvent();
			MatrixFrame globalFrame = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("sp_wind"))).GetGlobalFrame();
			val = ((Vec3)(ref globalFrame.rotation.f)).NormalizedCopy();
			Vec2 val2 = ((Vec3)(ref val)).AsVec2 * 1.5f;
			Mission.Current.Scene.SetGlobalWindStrengthVector(ref val2);
			MBMusicManager.Current.StartThemeWithConstantIntensity((MusicTheme)10242, false);
			MBMusicManager.Current.ChangeCurrentThemeIntensity(0.2f);
		}
		if (_defeatTimer != null && _defeatTimer.Check(false))
		{
			_defeatTimer = null;
			OnPlayerTeamDefeated();
		}
		if (_victoryTimer != null && _victoryTimer.Check(false))
		{
			_victoryTimer = null;
			OnEnemyTeamDefeated();
		}
		if (_isInSecondPhase && HasSelectedShip)
		{
			if (!_isGunnarAfterFightFirstNotificationShown)
			{
				_isGunnarAfterFightFirstNotificationShown = true;
				_currentNotificationText = new TextObject("{=Ni85tv1G}I think I see them. Untie our ships, and let’s have at it!", (Dictionary<string, object>)null);
			}
			else if (!_isGunnarAfterFightSecondNotificationShown && !_playerShip.GetIsThereActiveBridgeTo(_secondShip))
			{
				_isGunnarAfterFightSecondNotificationShown = true;
				_currentNotificationText = new TextObject("{=BfzIsraW}I’ll let you decide how to fight this one. Maneuver a bit, or just go straight at them?", (Dictionary<string, object>)null);
				PirateBattlePhase2Objective pirateBattlePhase2Objective = new PirateBattlePhase2Objective(Mission.Current, this);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)pirateBattlePhase2Objective);
			}
		}
		_notificationTimer += dt;
		if (!_isInSecondPhase)
		{
			if (_playerShip.GetIsConnectedToEnemy())
			{
				if (!_hasShownChargeNotification)
				{
					ShowChargeNotification();
				}
			}
			else
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)_playerShip).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				gameEntity = ((ScriptComponentBehavior)_secondShip).GameEntity;
				val = globalPosition - ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				if (((Vec3)(ref val)).LengthSquared >= 2500f)
				{
					_hasShownBoardImminentNotification = false;
					if (_notificationTimer > 27f)
					{
						_notificationTimer = 0f;
						_currentNotificationText = new TextObject("{=gMhrY6rz}Get us close so we can board.", (Dictionary<string, object>)null);
					}
				}
				else if (!_hasShownBoardImminentNotification)
				{
					_hasShownBoardImminentNotification = true;
					_currentNotificationText = new TextObject("{=GtSpVtOq}Get ready to board…", (Dictionary<string, object>)null);
					MBMusicManager.Current.ChangeCurrentThemeIntensity(0.5f);
				}
			}
		}
		else
		{
			if (!_hasShownSecondPhaseChargeNotification && _isGunnarAfterFightSecondNotificationShown && (_playerShip.GetIsConnectedToEnemy() || _secondShip.GetIsConnectedToEnemy()))
			{
				ShowSecondPhaseChargeNotification();
			}
			if (_playerShip.GetIsConnectedToEnemy() && !_hasIncreasedMusicIntensityForSecondPhase)
			{
				MBMusicManager.Current.ChangeCurrentThemeIntensity(0.5f);
				_hasIncreasedMusicIntensityForSecondPhase = true;
			}
		}
		if (_currentNotificationText != (TextObject)null)
		{
			ShowNotification(_currentNotificationText);
		}
		if (_isDialogueQueued)
		{
			_dialogueTimer += dt;
			if (!_isSecondPhaseSetup && _dialogueTimer > 0.5f)
			{
				SetupSecondPhase();
			}
			if (_dialogueTimer > 1.25f)
			{
				StartDialogue();
			}
		}
		if (_isShipTransferQueued)
		{
			_afterFightShipChangeTimer += dt;
			if (_afterFightShipChangeTimer >= 0.5f)
			{
				_isShipTransferQueued = false;
				HandleShipSelection(!_isSecondShipSelected);
			}
		}
	}

	private void UpdateEntityReferences()
	{
		((MissionBehavior)this).Mission.Scene.GetEntities(ref _entities);
	}

	public override void OnBehaviorInitialize()
	{
		if (!SailWindProfile.IsSailWindProfileInitialized)
		{
			SailWindProfile.InitializeProfile();
		}
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_missionObjectiveLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionObjectiveLogic>();
	}

	private void SpawnAllyTroops()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("gangradirs_kin_melee");
		CharacterObject val2 = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("gangradirs_kin_ranged");
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_playerShip, 22);
		_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, NavalStorylineData.Gunnar.CharacterObject, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
		_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, CharacterObject.PlayerCharacter, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
		for (int i = 0; i < 10; i++)
		{
			_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val, -1, default(UniqueTroopDescriptor), false, true), _playerShip);
		}
		for (int j = 0; j < 10; j++)
		{
			_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val2, -1, default(UniqueTroopDescriptor), false, true), _playerShip);
		}
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
		_gunnarAgent = ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).FirstOrDefault((Func<Agent, bool>)((Agent x) => (object)x.Character == NavalStorylineData.Gunnar.CharacterObject));
		_gunnarAgent.ToggleInvulnerable();
		NavalStorylineData.Gunnar.SetHasMet();
		_playerShip.Formation.PlayerOwner = Agent.Main;
		Mission.Current.PlayerTeam.PlayerOrderController.Owner = Agent.Main;
	}

	private Agent SpawnHero(CharacterObject character, string spawnPointTag)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag(spawnPointTag)));
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)character).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, character, -1, default(UniqueTroopDescriptor), false, true)).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = val.GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = val.GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val2 = obj2.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false);
		return Mission.Current.SpawnAgent(val2, false);
	}

	private void SpawnEnemyAgents(MissionShip ship)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("sea_hounds_pups");
		_navalAgentsLogic.SetDesiredTroopCountOfShip(ship, _pirateTroopCount);
		for (int i = 0; i < _pirateTroopCount; i++)
		{
			PartyAgentOrigin val2 = new PartyAgentOrigin(_pirateParty.Party, val, -1, default(UniqueTroopDescriptor), false, true);
			val2.SetBanner(NavalStorylineData.CorsairBanner);
			_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)(object)val2, ship);
		}
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)2);
	}

	private void SpawnAllyPrisonerAgents(MissionShip ship)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("gangradirs_kin_melee");
		CharacterObject val2 = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("gangradirs_kin_ranged");
		_navalAgentsLogic.SetDesiredTroopCountOfShip(ship, 16);
		for (int i = 0; i < 7; i++)
		{
			_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val, -1, default(UniqueTroopDescriptor), false, true), ship);
		}
		for (int j = 0; j < 7; j++)
		{
			_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val2, -1, default(UniqueTroopDescriptor), false, true), ship);
		}
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
	}

	private MissionShip CreateShip(string shipHullId, string spawnPointId, Formation formation, PartyBase owner, Dictionary<string, string> upgradePieces, string materialName)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		Ship val = new Ship(((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>(shipHullId));
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
		GameEntity val2 = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag(spawnPointId)));
		MatrixFrame shipFrame = val2.GetGlobalFrame();
		Scene scene = Mission.Current.Scene;
		Vec3 globalPosition = val2.GlobalPosition;
		float waterLevelAtPosition = scene.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, true, false);
		shipFrame.origin = new Vec3(val2.GlobalPosition.x, val2.GlobalPosition.y, waterLevelAtPosition, -1f);
		MissionShip missionShip = _navalShipsLogic.SpawnShip((IShipOrigin)(object)val, in shipFrame, formation.Team, formation, spawnAnchored: false, (FormationClass)8);
		ChangeShipColors(missionShip, owner.MapFaction.Color, owner.MapFaction.Color2, materialName);
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

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		bool num;
		if (!IsFirstShipCleared)
		{
			num = IsShipEffectivelyDepleted(_secondShip);
		}
		else
		{
			if (_reinforcementShip == null)
			{
				goto IL_0047;
			}
			num = IsShipEffectivelyDepleted(_reinforcementShip);
		}
		if (num && _defeatTimer == null)
		{
			_victoryTimer = new MissionTimer(3f);
		}
		goto IL_0047;
		IL_0047:
		if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents) || affectedAgent.IsMainAgent)
		{
			_defeatTimer = new MissionTimer(3f);
			_victoryTimer = null;
		}
	}

	private bool IsShipEffectivelyDepleted(MissionShip ship)
	{
		bool result = true;
		foreach (Agent item in (List<Agent>)(object)_navalAgentsLogic.GetActiveAgentsOfShip(ship))
		{
			if (!item.IsInWater())
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void OnEnemyTeamDefeated()
	{
		if (!IsFirstShipCleared)
		{
			IsFirstShipCleared = true;
			OnFirstEnemyShipCleared();
		}
		else
		{
			OnSecondEnemyShipCleared();
		}
	}

	private void ShowNotification(TextObject text)
	{
		CampaignInformationManager.AddDialogLine(text, NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		_currentNotificationText = null;
	}

	private void OnFirstEnemyShipCleared()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0040: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Invalid comparison between Unknown and I4
		//IL_0090: Expected O, but got Unknown
		if (Agent.Main.IsUsingGameObject)
		{
			Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		TextObject val = new TextObject("{=pn7YqjAE}Ship Cleared", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=6UauyvuX}Your men make quick work of the pirates. As the fighting dies down, you find that the Sea Hounds were carrying captives, bound and stashed beneath the rowing benches. You cut their bonds and help them to their feet as your lookouts scan the waters for any sign of the second ship.", (Dictionary<string, object>)null);
		TextObject val3 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InquiryData val4 = new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)val3).ToString(), (string)null, (Action)OnFirstFightPopUpClosed, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
		MBMusicManager.Current.ChangeCurrentThemeIntensity(-0.5f);
		InformationManager.ShowInquiry(val4, (int)Campaign.Current.GameMode == 1, false);
	}

	private void OnFirstFightPopUpClosed()
	{
		_isDialogueQueued = true;
		this.OnBeginScreenFadeEvent?.Invoke(0.5f, 0.75f);
	}

	private void SetupSecondPhase()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		_isSecondPhaseSetup = true;
		Formation formation = Mission.GetTeam((TeamSideEnum)2).GetFormation((FormationClass)1);
		_reinforcementShip = CreateShip("ship_lightlongship_storyline", "spawnpoint_ship_reinforcement", formation, _pirateParty.Party, ReinforcementShipUpgradePieces, "generated_square_l1_h4_10");
		((MissionObject)_reinforcementShip).OnDeploymentFinished();
		SpawnEnemyAgents(_reinforcementShip);
		MatrixFrame globalFrame = _playerShip.GlobalFrame;
		Vec2 position = ((Vec3)(ref globalFrame.origin)).AsVec2;
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Vec2 direction = ((Vec2)(ref val)).Normalized();
		_playerShip.SetAnchor(isAnchored: true);
		_playerShip.SetAnchorFrame(in position, in direction);
		if (_gunnarAgent == null || !_gunnarAgent.IsActive())
		{
			_gunnarAgent = SpawnHero(NavalStorylineData.Gunnar.CharacterObject, "conversation_ally");
			_gunnarAgent.ToggleInvulnerable();
		}
		_gunnarAgent.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)1);
		_gunnarAgent.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)1);
		Agent.Main.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)1);
		Agent.Main.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)1);
		_playerShip.ShipOrder.SetOrderOarsmenLevel(2);
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_playerShip);
		_navalAgentsLogic.SetDeploymentMode(value: false);
		if (Agent.Main.IsUsingGameObject)
		{
			Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		if (_gunnarAgent.IsUsingGameObject)
		{
			_gunnarAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		_gunnarAgent.TryAttachToFormation();
		_gunnarAgent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
		_gunnarAgent.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
		Agent.Main.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
		Agent.Main.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
		GameEntity val2 = _entities.Last((GameEntity t) => t.HasTag("conversation_ally"));
		_gunnarAgent.TeleportToPosition(val2.GlobalPosition);
		GameEntity val3 = _entities.Last((GameEntity t) => t.HasTag("conversation_player"));
		((MissionBehavior)this).Mission.MainAgent.TeleportToPosition(val3.GlobalPosition);
		Agent.Main.SetLookAgent(_gunnarAgent);
		Vec3 val4 = Agent.Main.Position - _gunnarAgent.Position;
		Agent gunnarAgent = _gunnarAgent;
		val = ((Vec3)(ref val4)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		gunnarAgent.SetMovementDirection(ref val);
		_gunnarAgent.SetLookAgent(Agent.Main);
		_gunnarAgent.Controller = (AgentControllerType)0;
		Action<float> action = this.OnCameraBearingNeedsUpdateEvent;
		Vec3 val5 = -val4;
		action(((Vec3)(ref val5)).RotationZ);
		_reinforcementShip.SetAnchor(isAnchored: true);
		_reinforcementShip.ShipOrder.SetShipStopOrder();
		_reinforcementShip.SetController(ShipControllerType.AI);
		_reinforcementShip.SetCanBeTakenOver(value: false);
		Agent.Main.Health = Agent.Main.HealthLimit;
		foreach (ShipAttachmentPointMachine item in MBExtensions.FindAllWithType<ShipAttachmentPointMachine>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.ActiveMissionObjects).ToList())
		{
			((UsableMissionObject)((UsableMachine)item).PilotStandingPoint).IsDisabledForPlayers = true;
		}
		foreach (ShipAttachmentMachine item2 in MBExtensions.FindAllWithType<ShipAttachmentMachine>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.ActiveMissionObjects).ToList())
		{
			((UsableMissionObject)((UsableMachine)item2).PilotStandingPoint).IsDisabledForPlayers = true;
		}
	}

	private void StartDialogue()
	{
		_isDialogueQueued = false;
		Campaign.Current.ConversationManager.SetupAndStartMissionConversation((IAgent)(object)_gunnarAgent, (IAgent)(object)((MissionBehavior)this).Mission.MainAgent, true);
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)1, true);
	}

	public void OnPlayerSelectedFirstShipToCommand()
	{
		_isSecondShipSelected = false;
		OnPlayerSelectedShipToCommand();
	}

	public void OnPlayerSelectedSecondShipToCommand()
	{
		_isSecondShipSelected = true;
		OnPlayerSelectedShipToCommand();
	}

	private void OnPlayerSelectedShipToCommand()
	{
		_isInSecondPhase = true;
		_isShipTransferQueued = true;
		PirateBattleCutLooseObjective pirateBattleCutLooseObjective = new PirateBattleCutLooseObjective(Mission.Current, this);
		_missionObjectiveLogic.StartObjective((MissionObjective)(object)pirateBattleCutLooseObjective);
		this.OnBeginScreenFadeEvent?.Invoke(0.5f, 0.75f);
	}

	private void HandleShipSelection(bool isFirstShipSelected)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		HasSelectedShip = true;
		_playerShip.SetAnchor(isAnchored: false);
		_secondShip.SetAnchor(isAnchored: false);
		_playerShip.SetController((!isFirstShipSelected) ? ShipControllerType.AI : ShipControllerType.Player);
		_secondShip.SetController(isFirstShipSelected ? ShipControllerType.AI : ShipControllerType.Player);
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)2, true);
		_playerShip.ShipOrder.SetShipStopOrder();
		_secondShip.ShipOrder.SetShipStopOrder();
		_secondShip.BreakAllExistingConnections();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_playerShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		bodyWorldTransform.rotation.u = Vec3.Up;
		ref Mat3 rotation = ref bodyWorldTransform.rotation;
		Vec3 val = ((Vec3)(ref bodyWorldTransform.rotation.s)).CrossProductWithUpAsLeftParameter();
		rotation.f = ((Vec3)(ref val)).NormalizedCopy();
		bodyWorldTransform.rotation.s = ((Vec3)(ref bodyWorldTransform.rotation.f)).CrossProductWithUp();
		ref Vec3 origin = ref bodyWorldTransform.origin;
		origin += bodyWorldTransform.rotation.s * (_playerShip.Physics.PhysicsBoundingBoxSizeWithoutChildren.x * 0.5f + _secondShip.Physics.PhysicsBoundingBoxSizeWithoutChildren.x * 0.5f + 1f);
		_navalShipsLogic.TeleportShip(_secondShip, bodyWorldTransform, checkFreeArea: false);
		_secondShip.TryToMaintainConnectionToAnotherShip(_playerShip);
		if (isFirstShipSelected)
		{
			_navalShipsLogic.TransferShipToTeam(_secondShip, ((MissionBehavior)this).Mission.PlayerTeam, null, (FormationClass)8);
		}
		else
		{
			Formation formation = _playerShip.Formation;
			Formation formation2 = ((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)1);
			_navalShipsLogic.TransferShipToFormation(_playerShip, formation2);
			_navalShipsLogic.TransferShipToTeam(_secondShip, ((MissionBehavior)this).Mission.PlayerTeam, formation, (FormationClass)8);
		}
		_playerShip.Formation.PlayerOwner = Agent.Main;
		_secondShip.Formation.PlayerOwner = Agent.Main;
		MissionShip missionShip = (isFirstShipSelected ? _secondShip : _playerShip);
		MissionShip onShip;
		bool flag = _navalAgentsLogic.IsAgentOnAnyShip(_gunnarAgent, out onShip, (TeamSideEnum)0);
		if (flag && onShip != missionShip)
		{
			_navalAgentsLogic.TransferAgentToShip(_gunnarAgent, missionShip);
		}
		else if (!flag)
		{
			_navalAgentsLogic.AddAgentToShip(_gunnarAgent, missionShip);
		}
		MissionShip missionShip2 = (isFirstShipSelected ? _playerShip : _secondShip);
		Team team = Agent.Main.Team;
		foreach (Agent item in (List<Agent>)(object)team.ActiveAgents)
		{
			if (item != _gunnarAgent)
			{
				MissionShip onShip2;
				bool flag2 = _navalAgentsLogic.IsAgentOnAnyShip(item, out onShip2, team.TeamSide);
				if (flag2 && onShip2 != missionShip2)
				{
					_navalAgentsLogic.TransferAgentToShip(item, missionShip2);
				}
				else if (!flag2)
				{
					_navalAgentsLogic.AddAgentToShip(item, missionShip2);
				}
			}
		}
		ReplenishPlayerShipTroops();
		SpawnAllyPrisonerAgents(isFirstShipSelected ? _secondShip : _playerShip);
		_navalAgentsLogic.AssignCaptainToShip(Agent.Main, missionShip2);
		_navalAgentsLogic.AssignCaptainToShip(_gunnarAgent, missionShip);
		_playerShip.Formation.SetControlledByAI(false, false);
		_secondShip.Formation.SetControlledByAI(false, false);
		_playerShip.ShipOrder.SetCutLoose(enable: false);
		_secondShip.ShipOrder.SetCutLoose(enable: false);
		_playerShip.ShipOrder.SetBoardingTargetShip(null);
		_secondShip.ShipOrder.SetBoardingTargetShip(null);
		_playerShip.ShipOrder.MakeEnemyOnShipExpire();
		_secondShip.ShipOrder.MakeEnemyOnShipExpire();
		_playerShip.ShipOrder.SetOrderOarsmenLevel(2);
		_secondShip.ShipOrder.SetOrderOarsmenLevel(2);
		_gunnarAgent.Controller = (AgentControllerType)1;
		string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 80), 1f);
		GameTexts.SetVariable("SHIP_COMMANDING_TUTORIAL_GROUP_KEY", keyHyperlinkText);
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		_playerShip.ShipOrder.Tick();
		_secondShip.ShipOrder.Tick();
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)0);
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
		((UsableMissionObject)((UsableMachine)_playerShip.ShipControllerMachine).PilotStandingPoint).IsDisabledForPlayers = false;
		((UsableMissionObject)((UsableMachine)_secondShip.ShipControllerMachine).PilotStandingPoint).IsDisabledForPlayers = false;
		gameEntity = ((ScriptComponentBehavior)_reinforcementShip).GameEntity;
		Vec3 val2 = ((WeakGameEntity)(ref gameEntity)).GlobalPosition - Agent.Main.Position;
		this.OnCameraBearingNeedsUpdateEvent(((Vec3)(ref val2)).RotationZ);
	}

	private void ReplenishPlayerShipTroops()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		int count = ((List<Agent>)(object)Agent.Main.Team.ActiveAgents).Count;
		int num = 14 - count;
		if (num > 0)
		{
			CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("gangradirs_kin_melee");
			CharacterObject val2 = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("gangradirs_kin_ranged");
			int num2 = num / 2;
			int num3 = num / 2;
			num2 += num - (num2 + num3);
			for (int i = 0; i < num2; i++)
			{
				_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val, -1, default(UniqueTroopDescriptor), false, true), _playerShip);
			}
			for (int j = 0; j < num3; j++)
			{
				_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val2, -1, default(UniqueTroopDescriptor), false, true), _playerShip);
			}
			_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
		}
	}

	private void OnSecondEnemyShipCleared()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001c: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		TextObject val = new TextObject("{=R4Gqskgq}Victory", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=tEK1RK5N}Once again, you are victorious. Gunnar, meanwhile, inspects the fallen pirates, and soon finds one who is only lightly wounded and able to speak.", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), "", (Action)OnVictoryPopUpClosed, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
		MBMusicManager.Current.ForceStopThemeWithFadeOut();
	}

	private void OnVictoryPopUpClosed()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_isMissionSuccessful = true;
		PlayerEncounter.Battle.SetOverrideWinner(PlayerEncounter.Battle.PlayerSide);
		((MissionBehavior)this).Mission.EndMission();
	}

	private void OnPlayerTeamDefeated()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		_isMissionFailed = true;
		PlayerEncounter.Battle.SetOverrideWinner(PlayerEncounter.Battle.GetOtherSide(PlayerEncounter.Battle.PlayerSide));
		((MissionBehavior)this).Mission.EndMission();
	}

	public bool HaveAllyShipsBeenCutLoose()
	{
		return !_playerShip.GetIsThereActiveBridgeTo(_secondShip);
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

	private void OnPlayerOrdered(OrderType orderType, MBReadOnlyList<Formation> appliedFormations, OrderController orderController, object[] delegateParams)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if (!_hasShownChargeNotification && !_isSecondPhaseSetup && ((int)orderType == 4 || (int)orderType == 5))
		{
			ShowChargeNotification();
		}
	}

	private void ShowChargeNotification()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		_currentNotificationText = new TextObject("{=J0O71ubZ}The lines are holding! At them, lads!", (Dictionary<string, object>)null);
		_hasShownChargeNotification = true;
	}

	private void ShowSecondPhaseChargeNotification()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		_currentNotificationText = new TextObject("{=8WDTkhc0}Strike hard, boys! Finish them!", (Dictionary<string, object>)null);
		_hasShownSecondPhaseChargeNotification = true;
	}
}
