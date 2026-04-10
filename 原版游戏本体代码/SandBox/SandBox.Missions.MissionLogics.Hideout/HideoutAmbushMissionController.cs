using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.MissionLogics.Hideout.Objectives;
using SandBox.Objects.AreaMarkers;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.Missions.Objectives;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Missions.MissionLogics.Hideout;

public class HideoutAmbushMissionController : MissionLogic
{
	public class TroopData
	{
		public CharacterObject Troop;

		public int Number;

		public int Level;

		public TroopData(CharacterObject troop, int number)
		{
			Troop = troop;
			Number = number;
		}
	}

	private enum HideoutMissionState
	{
		NotDecided,
		StealthState,
		CallTroopsCutSceneState,
		BattleBeforeBossFight,
		CutSceneBeforeBossFight,
		ConversationBetweenLeaders,
		BossFightWithDuel,
		BossFightWithAll
	}

	private const int FirstPhaseEndInSeconds = 4;

	private int _initialHideoutPopulation;

	private bool _troopsInitialized;

	private bool _isMissionInitialized;

	private bool _battleResolved;

	private readonly BattleSideEnum _playerSide;

	private HideoutMissionState _currentHideoutMissionState;

	private List<Agent> _duelPhaseAllyAgents;

	private List<Agent> _duelPhaseBanditAgents;

	private List<IAgentOriginBase> _allEnemyTroops;

	private List<IAgentOriginBase> _playerPriorTroops;

	private List<IAgentOriginBase> _allEnemyTroopTypesCache;

	private List<StealthAreaMissionLogic.StealthAreaData> _stealthAreaData;

	private Timer _waitTimerToChangeStealthModeIntoBattle;

	private Timer _firstPhaseEndTimer;

	private int _sentryCount;

	private int _remainingSentryCount;

	private bool _isClearedAsGhost = true;

	private BattleAgentLogic _battleAgentLogic;

	private BattleEndLogic _battleEndLogic;

	private HideoutAmbushBossFightCinematicController _hideoutAmbushBossFightCinematicController;

	private StealthAreaMissionLogic _stealthAreaMissionLogic;

	private MissionObjectiveLogic _missionObjectiveLogic;

	private Agent _bossAgent;

	private Team _enemyTeam;

	private CharacterObject _overriddenHideoutBossCharacterObject;

	private IAgentOriginBase _overriddenHideoutBossAgentOrigin;

	private int _playerTroopCount;

	private LocateTheMainCampObjective _locateTheMainCampObjective;

	private ClearTheMainCampObjective _clearTheMainCampObjective;

	private DefeatHideoutBossObjective _defeatHideoutBossObjective;

	private readonly List<Agent> _clearObjectiveTargetAgents = new List<Agent>();

	private IMissionTroopSupplier[] _suppliers;

	public bool IsReadyForCallTroopsCinematic => _currentHideoutMissionState == HideoutMissionState.CallTroopsCutSceneState;

	public HideoutAmbushMissionController(IMissionTroopSupplier[] suppliers, BattleSideEnum playerSide, int playerTroopCount)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		_playerSide = playerSide;
		_playerTroopCount = playerTroopCount;
		_stealthAreaData = new List<StealthAreaMissionLogic.StealthAreaData>();
		_waitTimerToChangeStealthModeIntoBattle = null;
		_currentHideoutMissionState = HideoutMissionState.NotDecided;
		_overriddenHideoutBossCharacterObject = null;
		_suppliers = suppliers;
		IMissionTroopSupplier val = _suppliers[Extensions.GetOppositeSide(_playerSide)];
		_initialHideoutPopulation = val.NumTroopsNotSupplied;
	}

	private void InitializeTroops()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (_overriddenHideoutBossCharacterObject == null)
		{
			_overriddenHideoutBossCharacterObject = Settlement.CurrentSettlement.Culture.BanditBoss;
		}
		IMissionTroopSupplier obj = _suppliers[Extensions.GetOppositeSide(_playerSide)];
		IEnumerable<IAgentOriginBase> source = obj.SupplyTroops(obj.NumTroopsNotSupplied);
		_overriddenHideoutBossAgentOrigin = source.FirstOrDefault((Func<IAgentOriginBase, bool>)((IAgentOriginBase x) => (object)x.Troop == _overriddenHideoutBossCharacterObject));
		CharacterObject val;
		_allEnemyTroops = source.Where((IAgentOriginBase x) => !x.Troop.IsHero && (val = (CharacterObject)/*isinst with value type is only supported in some contexts*/) != null && val.Culture.BanditBoss != val && val != _overriddenHideoutBossCharacterObject).ToList();
		_playerPriorTroops = _suppliers[_playerSide].SupplyTroops(_playerTroopCount).ToList();
		_allEnemyTroopTypesCache = Extensions.DistinctBy<IAgentOriginBase, BasicCharacterObject>((IEnumerable<IAgentOriginBase>)_allEnemyTroops, (Func<IAgentOriginBase, BasicCharacterObject>)((IAgentOriginBase x) => x.Troop)).ToList();
	}

	public override void OnCreated()
	{
		((MissionBehavior)this).OnCreated();
		((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment = false;
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		InitializeTroops();
		SandBoxHelpers.MissionHelper.SpawnPlayer(civilianEquipment: false, noHorses: true);
		Mission.Current.GetMissionBehavior<MissionAgentHandler>().SpawnLocationCharacters();
		Agent.Main.SetClothingColor1(4279111698u);
		Agent.Main.SetClothingColor2(4279111698u);
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(Hero.MainHero.StealthEquipment);
		foreach (StealthAreaMissionLogic.StealthAreaData stealthAreaDatum in _stealthAreaData)
		{
			foreach (KeyValuePair<StealthAreaMarker, List<Agent>> stealthAreaMarker in stealthAreaDatum.StealthAreaMarkers)
			{
				_sentryCount += stealthAreaMarker.Value.Count;
				_remainingSentryCount += stealthAreaMarker.Value.Count;
			}
		}
		Mission.Current.GetMissionBehavior<StealthFailCounterMissionLogic>().FailCounterSeconds = 15f;
		_locateTheMainCampObjective = new LocateTheMainCampObjective(((MissionBehavior)this).Mission);
		_missionObjectiveLogic.StartObjective((MissionObjective)(object)_locateTheMainCampObjective);
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_battleAgentLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<BattleAgentLogic>();
		_battleEndLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<BattleEndLogic>();
		_battleEndLogic.ChangeCanCheckForEndCondition(false);
		_stealthAreaMissionLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<StealthAreaMissionLogic>();
		StealthAreaMissionLogic stealthAreaMissionLogic = _stealthAreaMissionLogic;
		stealthAreaMissionLogic.SpawnReinforcementAllyTroopsEvent = (StealthAreaMissionLogic.SpawnReinforcementAllyTroopsDelegate)Delegate.Combine(stealthAreaMissionLogic.SpawnReinforcementAllyTroopsEvent, new StealthAreaMissionLogic.SpawnReinforcementAllyTroopsDelegate(SpawnReinforcementAllyTroops));
		_missionObjectiveLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionObjectiveLogic>();
		_hideoutAmbushBossFightCinematicController = ((MissionBehavior)this).Mission.GetMissionBehavior<HideoutAmbushBossFightCinematicController>();
		foreach (StealthAreaUsePoint item in MBExtensions.FindAllWithType<StealthAreaUsePoint>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.ActiveMissionObjects))
		{
			_stealthAreaData.Add(new StealthAreaMissionLogic.StealthAreaData(item));
		}
		Game.Current.EventManager.RegisterEvent<OnStealthMissionCounterFailedEvent>((Action<OnStealthMissionCounterFailedEvent>)OnStealthMissionCounterFailed);
	}

	private MBList<Agent> SpawnReinforcementAllyTroops(StealthAreaMissionLogic.StealthAreaData triggeredStealthAreaData, StealthAreaMarker stealthAreaMarker)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		int count = triggeredStealthAreaData.StealthAreaMarkers.Count;
		StealthAreaMarker[] array = triggeredStealthAreaData.StealthAreaMarkers.Keys.ToArray();
		MBList<Agent> val = new MBList<Agent>();
		for (int i = 0; i < _playerPriorTroops.Count; i++)
		{
			if (array[i % count] == stealthAreaMarker)
			{
				IAgentOriginBase character = _playerPriorTroops[i];
				Agent item = SpawnAllyAgent(character, stealthAreaMarker.ReinforcementAllyGroupSpawnPoint, stealthAreaMarker.WaitPoint.GlobalPosition);
				((List<Agent>)(object)val).Add(item);
			}
		}
		return val;
	}

	public override void OnRemoveBehavior()
	{
		((MissionBehavior)this).OnRemoveBehavior();
		StealthAreaMissionLogic stealthAreaMissionLogic = _stealthAreaMissionLogic;
		stealthAreaMissionLogic.SpawnReinforcementAllyTroopsEvent = (StealthAreaMissionLogic.SpawnReinforcementAllyTroopsDelegate)Delegate.Remove(stealthAreaMissionLogic.SpawnReinforcementAllyTroopsEvent, new StealthAreaMissionLogic.SpawnReinforcementAllyTroopsDelegate(SpawnReinforcementAllyTroops));
	}

	public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		if (!(usedObject is StealthAreaUsePoint))
		{
			return;
		}
		StealthAreaMissionLogic.StealthAreaData stealthAreaData = null;
		foreach (StealthAreaMissionLogic.StealthAreaData stealthAreaDatum in _stealthAreaData)
		{
			if ((object)stealthAreaDatum.StealthAreaUsePoint == usedObject)
			{
				stealthAreaData = stealthAreaDatum;
				break;
			}
		}
		if (stealthAreaData != null)
		{
			_currentHideoutMissionState = HideoutMissionState.CallTroopsCutSceneState;
			_waitTimerToChangeStealthModeIntoBattle = new Timer(((MissionBehavior)this).Mission.CurrentTime, 10f, true);
			_missionObjectiveLogic.CompleteCurrentObjective();
		}
		List<Agent> list = new List<Agent>();
		foreach (StealthAreaMissionLogic.StealthAreaData stealthAreaDatum2 in _stealthAreaData)
		{
			foreach (KeyValuePair<StealthAreaMarker, List<Agent>> stealthAreaMarker in stealthAreaDatum2.StealthAreaMarkers)
			{
				list.AddRange(stealthAreaMarker.Value);
			}
		}
		foreach (Agent item in list)
		{
			item.FadeOut(true, true);
			_remainingSentryCount--;
		}
		if (_isClearedAsGhost)
		{
			Campaign.Current.SkillLevelingManager.OnHideoutClearedAsGhost();
		}
	}

	public void SetOverriddenHideoutBossCharacterObject(CharacterObject characterObject)
	{
		_overriddenHideoutBossCharacterObject = characterObject;
	}

	private IAgentOriginBase GetOneEnemyTroopToSpawnInFirstPhase()
	{
		IAgentOriginBase val = null;
		if (_allEnemyTroops.Count > 0)
		{
			val = Extensions.GetRandomElement<IAgentOriginBase>((IReadOnlyList<IAgentOriginBase>)_allEnemyTroops);
			_allEnemyTroops.Remove(val);
		}
		else
		{
			val = GetNewRandomEnemyTroop();
		}
		return val;
	}

	private IAgentOriginBase GetNewRandomEnemyTroop()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		IAgentOriginBase randomElement = Extensions.GetRandomElement<IAgentOriginBase>((IReadOnlyList<IAgentOriginBase>)_allEnemyTroopTypesCache);
		CharacterObject val = (CharacterObject)randomElement.Troop;
		return (IAgentOriginBase)new PartyAgentOrigin(((PartyGroupAgentOrigin)randomElement).Party, val, -1, default(UniqueTroopDescriptor), false, true);
	}

	private void SpawnRemainingTroopsForBossFight(List<MatrixFrame> spawnFrames, int spawnCount)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		int count = _allEnemyTroops.Count;
		for (int i = 0; i < spawnCount - count; i++)
		{
			_allEnemyTroops.Add(GetNewRandomEnemyTroop());
		}
		Vec2 asVec;
		if (_overriddenHideoutBossAgentOrigin != null)
		{
			MatrixFrame val = spawnFrames.FirstOrDefault();
			((Mat3)(ref val.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			Mission current = Mission.Current;
			IAgentOriginBase overriddenHideoutBossAgentOrigin = _overriddenHideoutBossAgentOrigin;
			Vec3? val2 = val.origin;
			asVec = ((Vec3)(ref val.rotation.f)).AsVec2;
			Agent val3 = current.SpawnTroop(overriddenHideoutBossAgentOrigin, false, false, false, false, 0, 0, false, false, false, val2, (Vec2?)((Vec2)(ref asVec)).Normalized(), "_hideout_bandit", (ItemObject)null, (FormationClass)10, false);
			AgentFlag agentFlags = val3.GetAgentFlags();
			if (Extensions.HasAnyFlag<AgentFlag>(agentFlags, (AgentFlag)1048576))
			{
				val3.SetAgentFlags((AgentFlag)(agentFlags & -1048577));
			}
		}
		for (int j = 0; j < _allEnemyTroops.Count; j++)
		{
			MatrixFrame val4 = spawnFrames.FirstOrDefault();
			((Mat3)(ref val4.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			Mission current2 = Mission.Current;
			IAgentOriginBase obj = _allEnemyTroops[j];
			Vec3? val5 = val4.origin;
			asVec = ((Vec3)(ref val4.rotation.f)).AsVec2;
			Agent val6 = current2.SpawnTroop(obj, false, false, false, false, 0, 0, false, false, false, val5, (Vec2?)((Vec2)(ref asVec)).Normalized(), "_hideout_bandit", (ItemObject)null, (FormationClass)10, false);
			AgentFlag agentFlags2 = val6.GetAgentFlags();
			if (Extensions.HasAnyFlag<AgentFlag>(agentFlags2, (AgentFlag)1048576))
			{
				val6.SetAgentFlags((AgentFlag)(agentFlags2 & -1048577));
			}
		}
		foreach (Formation item in (List<Formation>)(object)Mission.Current.AttackerTeam.FormationsIncludingEmpty)
		{
			if (item.CountOfUnits > 0)
			{
				item.SetMovementOrder(MovementOrder.MovementOrderMove(item.CachedMedianPosition));
			}
			item.SetFiringOrder(FiringOrder.FiringOrderHoldYourFire);
			if (Mission.Current.AttackerTeam == Mission.Current.PlayerTeam)
			{
				item.PlayerOwner = Mission.Current.MainAgent;
			}
		}
	}

	private Agent SpawnAllyAgent(IAgentOriginBase character, GameEntity spawnPoint, Vec3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = spawnPoint.GetGlobalFrame();
		Mission current = Mission.Current;
		Vec3? val = globalFrame.origin;
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Agent obj = current.SpawnTroop(character, true, false, false, false, 0, 0, true, true, true, val, (Vec2?)((Vec2)(ref asVec)).Normalized(), (string)null, (ItemObject)null, (FormationClass)10, false);
		Vec3 randomPositionAroundPoint = Mission.Current.GetRandomPositionAroundPoint(position, 0f, 2f, true);
		WorldPosition val2 = default(WorldPosition);
		((WorldPosition)(ref val2))._002Ector(spawnPoint.Scene, randomPositionAroundPoint);
		obj.SetScriptedPosition(ref val2, true, (AIScriptedFrameFlags)514);
		return obj;
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("hideout_center");
		if (unusedUsablePointCount.TryGetValue("stealth_agent_forced", out var value))
		{
			locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateForcedSentry), Settlement.CurrentSettlement.Culture, (CharacterRelations)2, value);
		}
		if (unusedUsablePointCount.TryGetValue("stealth_agent", out value))
		{
			int num = _initialHideoutPopulation / 8;
			if (num >= 1)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateSentry), Settlement.CurrentSettlement.Culture, (CharacterRelations)2, Math.Min(num, value));
			}
		}
	}

	private LocationCharacter CreateForcedSentry(CultureObject culture, CharacterRelations relation)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		IAgentOriginBase oneEnemyTroopToSpawnInFirstPhase = GetOneEnemyTroopToSpawnInFirstPhase();
		CharacterObject val = (CharacterObject)oneEnemyTroopToSpawnInFirstPhase.Troop;
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(val, ref num, ref num2, "");
		AgentData obj = new AgentData(oneEnemyTroopToSpawnInFirstPhase).Monster(FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)val).Race, "_settlement_slow")).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddStealthAgentBehaviors), "stealth_agent_forced", true, relation, (string)null, false, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, true);
	}

	private LocationCharacter CreateSentry(CultureObject culture, CharacterRelations relation)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		IAgentOriginBase oneEnemyTroopToSpawnInFirstPhase = GetOneEnemyTroopToSpawnInFirstPhase();
		CharacterObject val = (CharacterObject)oneEnemyTroopToSpawnInFirstPhase.Troop;
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(val, ref num, ref num2, "");
		AgentData obj = new AgentData(oneEnemyTroopToSpawnInFirstPhase).Monster(FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)val).Race, "_settlement_slow")).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddStealthAgentBehaviors), "stealth_agent", true, relation, (string)null, false, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (_waitTimerToChangeStealthModeIntoBattle != null && _waitTimerToChangeStealthModeIntoBattle.Check(((MissionBehavior)this).Mission.CurrentTime))
		{
			Agent main = Agent.Main;
			if (main != null && main.IsActive())
			{
				ChangeHideoutMissionModeToBattle();
				_waitTimerToChangeStealthModeIntoBattle = null;
			}
		}
		if (!_isMissionInitialized)
		{
			Agent main2 = Agent.Main;
			if (main2 != null && main2.IsActive())
			{
				InitializeMission();
				_isMissionInitialized = true;
				return;
			}
		}
		if (!_isMissionInitialized)
		{
			return;
		}
		if (!_troopsInitialized)
		{
			_troopsInitialized = true;
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				((MissionBehavior)_battleAgentLogic).OnAgentBuild(item, (Banner)null);
			}
		}
		if (!_battleResolved)
		{
			CheckBattleResolved();
		}
		else if (!((MissionBehavior)this).Mission.ForceNoFriendlyFire)
		{
			((MissionBehavior)this).Mission.ForceNoFriendlyFire = true;
		}
	}

	private void InitializeMission()
	{
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().DisableStartConversation(isDisabled: true);
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)4, true);
		_currentHideoutMissionState = HideoutMissionState.StealthState;
		((MissionBehavior)this).Mission.DeploymentPlan.MakeDefaultDeploymentPlans();
		List<GameEntity> list = new List<GameEntity>();
		Mission.Current.Scene.GetAllEntitiesWithScriptComponent<Chair>(ref list);
		foreach (GameEntity item in list)
		{
			foreach (StandingPoint item2 in (List<StandingPoint>)(object)((UsableMachine)item.GetFirstScriptOfType<Chair>()).StandingPoints)
			{
				((UsableMissionObject)item2).IsDisabledForPlayers = true;
			}
		}
	}

	private void ChangeHideoutMissionModeToBattle()
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		_currentHideoutMissionState = HideoutMissionState.BattleBeforeBossFight;
		Mission.Current.SetMissionMode((MissionMode)2, false);
		foreach (Agent item in (List<Agent>)(object)Mission.Current.PlayerTeam.ActiveAgents)
		{
			if (!item.IsMainAgent)
			{
				item.ClearTargetFrame();
				item.DisableScriptedMovement();
			}
		}
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SetOrder((OrderType)4);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SetOrder((OrderType)4);
		foreach (Agent item2 in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents)
		{
			item2.SetAlarmState((AIStateFlag)3);
			_clearObjectiveTargetAgents.Add(item2);
		}
		Vec3 position = Agent.Main.Position;
		SoundManager.StartOneShotEvent("event:/ui/mission/horns/attack", ref position);
		_clearTheMainCampObjective = new ClearTheMainCampObjective(((MissionBehavior)this).Mission, _clearObjectiveTargetAgents);
		_missionObjectiveLogic.StartObjective((MissionObjective)(object)_clearTheMainCampObjective);
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (_currentHideoutMissionState >= HideoutMissionState.CutSceneBeforeBossFight || !agent.IsHuman || agent.Team != Mission.Current.PlayerEnemyTeam)
		{
			return;
		}
		foreach (StealthAreaMissionLogic.StealthAreaData stealthAreaDatum in _stealthAreaData)
		{
			foreach (KeyValuePair<StealthAreaMarker, List<Agent>> stealthAreaMarker in stealthAreaDatum.StealthAreaMarkers)
			{
				if (((AreaMarker)stealthAreaMarker.Key).IsPositionInRange(agent.Position))
				{
					stealthAreaDatum.AddAgentToStealthAreaMarker(stealthAreaMarker.Key, agent);
					break;
				}
			}
		}
	}

	public override void OnAgentAlarmedStateChanged(Agent agent, AIStateFlag flag)
	{
		if (agent.IsAlarmed() && _currentHideoutMissionState == HideoutMissionState.StealthState)
		{
			_isClearedAsGhost = false;
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (_clearObjectiveTargetAgents.Contains(affectedAgent))
		{
			_clearObjectiveTargetAgents.Remove(affectedAgent);
		}
		if (_currentHideoutMissionState == HideoutMissionState.StealthState)
		{
			_isClearedAsGhost = false;
		}
		if (affectorAgent != null && affectorAgent.IsMainAgent)
		{
			_remainingSentryCount = 0;
			foreach (StealthAreaMissionLogic.StealthAreaData stealthAreaDatum in _stealthAreaData)
			{
				foreach (KeyValuePair<StealthAreaMarker, List<Agent>> stealthAreaMarker in stealthAreaDatum.StealthAreaMarkers)
				{
					if (stealthAreaMarker.Value.Contains(affectedAgent) || Extensions.IsEmpty<Agent>((IEnumerable<Agent>)stealthAreaMarker.Value))
					{
						stealthAreaDatum.RemoveAgentFromStealthAreaMarker(stealthAreaMarker.Key, affectedAgent);
					}
					_remainingSentryCount += stealthAreaMarker.Value.Count;
				}
			}
		}
		if (_currentHideoutMissionState == HideoutMissionState.BossFightWithDuel)
		{
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				if (item != affectedAgent && item != affectorAgent && item.IsActive() && item.GetLookAgent() == affectedAgent)
				{
					item.SetLookAgent((Agent)null);
				}
			}
			return;
		}
		if ((_currentHideoutMissionState == HideoutMissionState.StealthState || _currentHideoutMissionState == HideoutMissionState.BattleBeforeBossFight) && affectedAgent.IsMainAgent)
		{
			((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SelectAllFormations(false);
			affectedAgent.Formation = null;
			((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SetOrder((OrderType)9);
		}
	}

	public void OnStealthMissionCounterFailed(OnStealthMissionCounterFailedEvent obj)
	{
		if (!_battleResolved)
		{
			Campaign.Current.SkillLevelingManager.OnHideoutMissionEnd(false);
		}
		Campaign.Current.GameMenuManager.SetNextMenu("hideout_after_found_by_sentries");
	}

	private void CheckBattleResolved()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		if (_currentHideoutMissionState == HideoutMissionState.NotDecided || _currentHideoutMissionState == HideoutMissionState.CutSceneBeforeBossFight || _currentHideoutMissionState == HideoutMissionState.ConversationBetweenLeaders)
		{
			return;
		}
		if (IsSideDepleted(((MissionBehavior)this).Mission.PlayerTeam.Side))
		{
			if (_currentHideoutMissionState == HideoutMissionState.BossFightWithDuel)
			{
				OnDuelOver(((MissionBehavior)this).Mission.PlayerEnemyTeam.Side);
			}
			Campaign.Current.SkillLevelingManager.OnHideoutMissionEnd(false);
			_battleEndLogic.ChangeCanCheckForEndCondition(true);
			_battleResolved = true;
			_missionObjectiveLogic.CompleteCurrentObjective();
		}
		else
		{
			if (!IsSideDepleted(((MissionBehavior)this).Mission.PlayerEnemyTeam.Side))
			{
				return;
			}
			if (_currentHideoutMissionState == HideoutMissionState.BattleBeforeBossFight || _currentHideoutMissionState == HideoutMissionState.StealthState)
			{
				Agent main = Agent.Main;
				if (main != null && main.IsActive())
				{
					if (_firstPhaseEndTimer == null)
					{
						_firstPhaseEndTimer = new Timer(((MissionBehavior)this).Mission.CurrentTime, 4f, true);
						Mission.Current.SetMissionMode((MissionMode)9, false);
					}
					else if (_firstPhaseEndTimer.Check(((MissionBehavior)this).Mission.CurrentTime))
					{
						_hideoutAmbushBossFightCinematicController.StartCinematic(OnInitialFadeOutOver, OnCutSceneOver);
						_missionObjectiveLogic.CompleteCurrentObjective();
					}
				}
			}
			else
			{
				if (_currentHideoutMissionState == HideoutMissionState.BossFightWithDuel)
				{
					OnDuelOver(((MissionBehavior)this).Mission.PlayerTeam.Side);
				}
				Campaign.Current.SkillLevelingManager.OnHideoutMissionEnd(true);
				_battleEndLogic.ChangeCanCheckForEndCondition(true);
				MapEvent.PlayerMapEvent.SetOverrideWinner(((MissionBehavior)this).Mission.PlayerTeam.Side);
				_battleResolved = true;
				_missionObjectiveLogic.CompleteCurrentObjective();
			}
		}
	}

	public bool IsSideDepleted(BattleSideEnum side)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ((List<Agent>)(object)(((int)side == 1) ? Mission.Current.Teams.Attacker : Mission.Current.Teams.Defender).ActiveAgents).Count == 0;
		if (!flag)
		{
			if (_playerSide == side)
			{
				if (Agent.Main == null || !Agent.Main.IsActive())
				{
					if (_currentHideoutMissionState == HideoutMissionState.BossFightWithDuel || _currentHideoutMissionState == HideoutMissionState.BattleBeforeBossFight)
					{
						flag = true;
					}
					else if (_currentHideoutMissionState == HideoutMissionState.BossFightWithAll)
					{
						flag = Extensions.IsEmpty<Agent>((IEnumerable<Agent>)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents) && !Extensions.IsEmpty<Agent>((IEnumerable<Agent>)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents);
					}
				}
			}
			else if (_currentHideoutMissionState == HideoutMissionState.BossFightWithDuel && (_bossAgent == null || !_bossAgent.IsActive()))
			{
				flag = true;
			}
		}
		return flag;
	}

	public void OnAgentsShouldBeEnabled()
	{
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (item.IsActive() && item.IsAIControlled)
			{
				item.SetIsAIPaused(false);
			}
		}
	}

	protected override void OnEndMission()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
		int num = 0;
		if (_currentHideoutMissionState == HideoutMissionState.BossFightWithDuel)
		{
			if (Agent.Main == null || !Agent.Main.IsActive())
			{
				num = _duelPhaseAllyAgents?.Count ?? 0;
			}
			else if (_bossAgent == null || !_bossAgent.IsActive())
			{
				PlayerEncounter.EnemySurrender = true;
			}
		}
		if (!PlayerEncounter.EnemySurrender && num <= 0 && MobileParty.MainParty.MemberRoster.TotalHealthyCount <= 0 && (int)MapEvent.PlayerMapEvent.BattleState == 0)
		{
			MapEvent.PlayerMapEvent.SetOverrideWinner(((MissionBehavior)this).Mission.PlayerEnemyTeam.Side);
		}
		Game.Current.EventManager.UnregisterEvent<OnStealthMissionCounterFailedEvent>((Action<OnStealthMissionCounterFailedEvent>)OnStealthMissionCounterFailed);
	}

	private void SpawnBossAndBodyguards()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = Agent.Main.Position + Agent.Main.LookDirection * -3f;
		int spawnCount = (int)MathF.Clamp((float)(_initialHideoutPopulation / 2), 4f, 20f);
		SpawnRemainingTroopsForBossFight(new List<MatrixFrame> { identity }, spawnCount);
		_bossAgent = SelectBossAgent();
		_bossAgent.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents)
		{
			if (item != _bossAgent)
			{
				item.WieldInitialWeapons((WeaponWieldActionType)3, (InitialWeaponEquipPreference)0);
			}
		}
	}

	private Agent SelectBossAgent()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		Agent val = null;
		Agent val2 = null;
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (!item.IsHuman || item.Team.IsPlayerAlly)
			{
				continue;
			}
			if (_overriddenHideoutBossCharacterObject == null)
			{
				if (item.IsHero)
				{
					val = item;
					val2 = item;
					break;
				}
				if (item.Character.Culture.IsBandit)
				{
					BasicCultureObject culture = item.Character.Culture;
					BasicCultureObject obj = ((culture is CultureObject) ? culture : null);
					if (((obj != null) ? ((CultureObject)obj).BanditBoss : null) != null && (object)((CultureObject)item.Character.Culture).BanditBoss == item.Character)
					{
						val = item;
					}
				}
			}
			else if ((object)item.Character == _overriddenHideoutBossCharacterObject)
			{
				val = item;
				val2 = item;
				break;
			}
			if (val2 == null || item.Character.Level > val2.Character.Level)
			{
				val2 = item;
			}
		}
		return val ?? val2;
	}

	private void OnInitialFadeOutOver(ref Agent playerAgent, ref List<Agent> playerCompanions, ref Agent bossAgent, ref List<Agent> bossCompanions, ref float placementPerturbation, ref float placementAngle)
	{
		_currentHideoutMissionState = HideoutMissionState.CutSceneBeforeBossFight;
		_enemyTeam = ((MissionBehavior)this).Mission.PlayerEnemyTeam;
		SpawnBossAndBodyguards();
		((MissionBehavior)this).Mission.PlayerTeam.SetIsEnemyOf(_enemyTeam, false);
		if (Agent.Main.IsUsingGameObject)
		{
			Agent.Main.StopUsingGameObject(false, (StopUsingGameObjectFlags)1);
		}
		playerAgent = Agent.Main;
		playerCompanions = ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).Where((Agent x) => x.IsActive() && x.Team == ((MissionBehavior)this).Mission.PlayerTeam && x.IsHuman && x.IsAIControlled).ToList();
		bossAgent = _bossAgent;
		bossCompanions = ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).Where((Agent x) => x.IsActive() && x.Team == _enemyTeam && x.IsHuman && x.IsAIControlled && x != _bossAgent).ToList();
	}

	private void OnCutSceneOver()
	{
		Mission.Current.SetMissionMode((MissionMode)2, false);
		_currentHideoutMissionState = HideoutMissionState.ConversationBetweenLeaders;
		MissionConversationLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>();
		missionBehavior.DisableStartConversation(isDisabled: false);
		missionBehavior.StartConversation(_bossAgent, setActionsInstantly: false);
	}

	private void OnDuelOver(BattleSideEnum winnerSide)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Invalid comparison between Unknown and I4
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Invalid comparison between Unknown and I4
		if (winnerSide == ((MissionBehavior)this).Mission.PlayerTeam.Side && _duelPhaseAllyAgents != null)
		{
			foreach (Agent duelPhaseAllyAgent in _duelPhaseAllyAgents)
			{
				if ((int)duelPhaseAllyAgent.State == 1)
				{
					duelPhaseAllyAgent.SetTeam(((MissionBehavior)this).Mission.PlayerTeam, true);
				}
			}
			return;
		}
		if (winnerSide != ((MissionBehavior)this).Mission.PlayerEnemyTeam.Side || _duelPhaseBanditAgents == null)
		{
			return;
		}
		foreach (Agent duelPhaseBanditAgent in _duelPhaseBanditAgents)
		{
			if ((int)duelPhaseBanditAgent.State == 1)
			{
				duelPhaseBanditAgent.SetTeam(_enemyTeam, true);
				duelPhaseBanditAgent.DisableScriptedMovement();
				duelPhaseBanditAgent.ClearTargetFrame();
			}
		}
		foreach (Agent duelPhaseAllyAgent2 in _duelPhaseAllyAgents)
		{
			if ((int)duelPhaseAllyAgent2.State == 1)
			{
				duelPhaseAllyAgent2.SetTeam(((MissionBehavior)this).Mission.PlayerTeam, true);
				duelPhaseAllyAgent2.DisableScriptedMovement();
				duelPhaseAllyAgent2.ClearTargetFrame();
			}
		}
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents)
		{
			item.SetAlarmState((AIStateFlag)3);
		}
	}

	public static void StartBossFightDuelMode()
	{
		Mission current = Mission.Current;
		((current != null) ? current.GetMissionBehavior<HideoutAmbushMissionController>() : null)?.StartBossFightDuelModeInternal();
	}

	private void StartBossFightDuelModeInternal()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().DisableStartConversation(isDisabled: true);
		((MissionBehavior)this).Mission.PlayerTeam.SetIsEnemyOf(_enemyTeam, true);
		_duelPhaseAllyAgents = ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).Where((Agent x) => x.IsActive() && x.Team == ((MissionBehavior)this).Mission.PlayerTeam && x.IsHuman && x.IsAIControlled && x != Agent.Main).ToList();
		_duelPhaseBanditAgents = ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).Where((Agent x) => x.IsActive() && x.Team == _enemyTeam && x.IsHuman && x.IsAIControlled && x != _bossAgent).ToList();
		foreach (Agent duelPhaseAllyAgent in _duelPhaseAllyAgents)
		{
			duelPhaseAllyAgent.SetTeam(Team.Invalid, true);
			WorldPosition worldPosition = duelPhaseAllyAgent.GetWorldPosition();
			duelPhaseAllyAgent.SetScriptedPosition(ref worldPosition, false, (AIScriptedFrameFlags)0);
			duelPhaseAllyAgent.SetLookAgent(Agent.Main);
		}
		foreach (Agent duelPhaseBanditAgent in _duelPhaseBanditAgents)
		{
			duelPhaseBanditAgent.SetTeam(Team.Invalid, true);
			WorldPosition worldPosition2 = duelPhaseBanditAgent.GetWorldPosition();
			duelPhaseBanditAgent.SetScriptedPosition(ref worldPosition2, false, (AIScriptedFrameFlags)0);
			duelPhaseBanditAgent.SetLookAgent(_bossAgent);
		}
		_bossAgent.SetAlarmState((AIStateFlag)3);
		_currentHideoutMissionState = HideoutMissionState.BossFightWithDuel;
		_defeatHideoutBossObjective = new DefeatHideoutBossObjective(((MissionBehavior)this).Mission, isDuel: true);
		_missionObjectiveLogic.StartObjective((MissionObjective)(object)_defeatHideoutBossObjective);
	}

	public static void StartBossFightBattleMode()
	{
		Mission current = Mission.Current;
		((current != null) ? current.GetMissionBehavior<HideoutAmbushMissionController>() : null)?.StartBossFightBattleModeInternal();
	}

	private void StartBossFightBattleModeInternal()
	{
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().DisableStartConversation(isDisabled: true);
		((MissionBehavior)this).Mission.PlayerTeam.SetIsEnemyOf(_enemyTeam, true);
		_currentHideoutMissionState = HideoutMissionState.BossFightWithAll;
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents)
		{
			item.SetAlarmState((AIStateFlag)3);
		}
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SetOrder((OrderType)4);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SetOrder((OrderType)4);
		_defeatHideoutBossObjective = new DefeatHideoutBossObjective(((MissionBehavior)this).Mission, isDuel: false);
		_missionObjectiveLogic.StartObjective((MissionObjective)(object)_defeatHideoutBossObjective);
	}

	private void KillAllSentries()
	{
		List<Agent> list = new List<Agent>();
		foreach (StealthAreaMissionLogic.StealthAreaData stealthAreaDatum in _stealthAreaData)
		{
			foreach (KeyValuePair<StealthAreaMarker, List<Agent>> stealthAreaMarker in stealthAreaDatum.StealthAreaMarkers)
			{
				list.AddRange(stealthAreaMarker.Value);
			}
		}
		foreach (Agent item in list)
		{
			((MissionBehavior)this).Mission.KillAgentCheat(item);
		}
	}

	[CommandLineArgumentFunction("kill_all_sentries", "mission")]
	public static string KillAllSentries(List<string> strings)
	{
		string empty = string.Empty;
		if (!CampaignCheats.CheckCheatUsage(ref empty))
		{
			return empty;
		}
		Mission current = Mission.Current;
		HideoutAmbushMissionController hideoutAmbushMissionController = ((current != null) ? current.GetMissionBehavior<HideoutAmbushMissionController>() : null);
		if (hideoutAmbushMissionController != null)
		{
			hideoutAmbushMissionController.KillAllSentries();
			return "Done";
		}
		return "This cheat only works in hideout ambush mission!";
	}
}
