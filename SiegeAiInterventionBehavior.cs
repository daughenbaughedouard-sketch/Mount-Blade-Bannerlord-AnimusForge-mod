using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using AnimusForge.SiegeAftermathIntervention;
using Helpers;
using HarmonyLib;
using SandBox;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.AgentBehaviors;
using SandBox.View.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.VisualOrders.Orders;
using TaleWorlds.MountAndBlade.View.VisualOrders.Orders.ToggleOrders;
using TaleWorlds.MountAndBlade.View.VisualOrders.OrderSets;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.FormOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.ToggleOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ScreenSystem;

namespace AnimusForge;

public class SiegeAiInterventionBehavior : CampaignBehaviorBase
{
	private enum InterventionMode
	{
		None,
		WaitingDecision,
		MercyRelief,
		Plunder,
		Massacre
	}


	private sealed class MarketLootCandidate
	{
		public EquipmentElement EquipmentElement;

		public int Amount;
	}

	private sealed class PlunderInteraction
	{
		public int TargetAgentIndex;

		public int SoldierAgentIndex;

		public float StartedAt;

		public float TalkStartedAt = -1f;
	}

	private sealed class CivilianGatherInteraction
	{
		public int MessengerAgentIndex;

		public int TargetAgentIndex;

		public float StartedAt;

		public float TalkStartedAt = -1f;

		public float TalkSeconds;
	}

	private sealed class InterventionMissionBehavior : MissionLogic
	{
		public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

		public override void AfterStart()
		{
			base.AfterStart();
			SiegeAiInterventionBehavior.OnInterventionMissionAfterStart(base.Mission);
		}

		public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
		{
			if (affectorAgent == Agent.Main)
			{
				int collisionDamage = Math.Max(5, Math.Min(80, blow.InflictedDamage > 0 ? blow.InflictedDamage : 35));
				if (!SiegeAiInterventionBehavior.TryHandlePlayerAttackForAutoMassacre(affectedAgent, "intervention_agent_hit", Math.Max(0, blow.InflictedDamage), collisionDamage))
				{
					SiegeAiInterventionBehavior.TryHandleFriendlyHitOnAlliedSoldier(affectedAgent, "intervention_agent_hit", 0f);
				}
			}
		}

		public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty)
		{
			base.OnScoreHit(affectedAgent, affectorAgent, attackerWeapon, isBlocked, isSiegeEngineHit, in blow, in collisionData, damagedHp, hitDistance, shotDifficulty);
			if (damagedHp > 0f && affectorAgent == Agent.Main)
			{
				if (!SiegeAiInterventionBehavior.TryHandlePlayerAttackForAutoMassacre(affectedAgent, "intervention_score_hit", damagedHp))
				{
					SiegeAiInterventionBehavior.TryHandleFriendlyHitOnAlliedSoldier(affectedAgent, "intervention_score_hit", damagedHp);
				}
			}
		}

		public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
		{
			SiegeAiInterventionBehavior.OnInterventionAgentRemoved(affectedAgent, affectorAgent, agentState);
		}

		public override InquiryData OnEndMissionRequest(out bool canPlayerLeave)
		{
			canPlayerLeave = true;
			return null;
		}
	}

	private const int AutoSummonCount = 50;
	private const int MaxSummonPerAction = 50;
	private const float MercyInterventionLoyaltyBonus = 15f;
	private const int NonHeroPlunderMinGold = 5;
	private const int NonHeroPlunderMaxGold = 9;
	private const int NonHeroMassacreGold = 10;
	private const int HeroMassacreFallbackGold = 3000;
	private const int MaxConcurrentPlunderInteractions = 6;
	private const float PlunderSoldierAssignmentRatio = 0.25f;
	private const float PlunderApproachDistance = 3.5f;
	private const float PlunderTalkSeconds = 1.2f;
	private const float MassacreCivilianHideDistance = 42f;
	private const float MassacreCivilianHideRefreshSeconds = 10.0f;
	private const float MassacreSoldierFollowRefreshSeconds = 2.0f;
	private const float MassacreSoldierTargetRefreshSeconds = 0.75f;
	private const float CivilianSpeechRallySettleTolerance = 0.8f;
	private const float CivilianGatherTalkMinSeconds = 1.0f;
	private const float CivilianGatherTalkMaxSeconds = 3.0f;
	private const float CivilianGatherFallbackSeconds = 75.0f;
	private const float CivilianGatherApproachDistance = 3.2f;
	private const float CivilianGatherFollowRefreshSeconds = 1.25f;
	private const float CivilianGatherFormationSettleDistance = 5.5f;
	private const float CivilianGatherSoldierMessengerRatio = 0.20f;
	private const float CivilianGatherMessengerMoveSpeedLimit = 1.9f;
	private const float CivilianFormationControlInitialDelaySeconds = 0.8f;
	private const float CivilianFormationControlBatchIntervalSeconds = 0.12f;
	private const int CivilianFormationControlBatchSize = 8;
	private const int CivilianGatherMessengerSpeechMinCount = 2;
	private const int CivilianGatherMessengerSpeechMaxCount = 3;
	private const int MinDesiredCivilianAssemblyCount = 180;
	private const int MaxDesiredCivilianAssemblyCount = 220;
	private const int TownCivilianAssemblySceneCap = 140;
	private const int CastleCivilianAssemblySceneCap = 90;
	private const int CivilianAssemblySmallSceneExtraCap = 70;
	private const int SceneTotalAgentSoftCap = 220;
	private const int MinimumCivilianAssemblySceneCap = 60;
	private static readonly bool EnableExtraCivilianAssemblySpawns = false;
	private const float CivilianAssemblyForwardDistance = 4.2f;
	private const float CivilianAssemblyColumnSpacing = 0.9f;
	private const float CivilianAssemblyRowSpacing = 0.78f;
	private const int CivilianAssemblyColumns = 14;
	private const float SoldierCordonMinRadius = 7.2f;
	private const float SoldierCordonPadding = 2.8f;
	private const float SoldierCordonTeleportDistance = 18f;
	private const float SoldierCordonMoveTolerance = 0.75f;
	private const float SoldierCordonSettleTolerance = 0.45f;
	private const float SoldierCordonOrderRefreshSeconds = 1.25f;
	private const float SoldierCordonLookRefreshSeconds = 1.1f;
	private const string SiegeInterventionRuleId = "siege_intervention_aftermath";
	private const int MaxInterventionMemoryEvents = 10;

	private static readonly Regex MercyTagRegex = new Regex("\\[ACTION:(?:SIEGE_MERCY|宽恕)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private static readonly Regex ReliefTagRegex = new Regex("\\[ACTION:(?:SIEGE_RELIEF|救济)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private static readonly Regex InspireTagRegex = new Regex("\\[ACTION:(?:SIEGE_INSPIRE|宣抚)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private static readonly Regex RallyOathTagRegex = new Regex("\\[ACTION:(?:SIEGE_RALLY_OATH|盟誓)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private static readonly Regex SoldierAppeasementTagRegex = new Regex("\\[ACTION:(?:SIEGE_APPEASE_SOLDIERS|安兵)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private static readonly Regex RepopulationTagRegex = new Regex("\\[ACTION:(?:SIEGE_CULTURAL_REPOPULATION|SIEGE_PURGE_REPOPULATION|殖民)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private static readonly Regex GatherCiviliansTagRegex = new Regex("\\[ACTION:(?:SIEGE_GATHER_CIVILIANS|召集)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private static readonly Regex PlunderTagRegex = new Regex("\\[ACTION:(?:SIEGE_PLUNDER|搜掠)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private static readonly Regex MassacreTagRegex = new Regex("\\[ACTION:(?:SIEGE_MASSACRE|血洗)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	private static readonly Regex AnySiegeTagRegex = new Regex("\\[ACTION:(?:SIEGE_[A-Z_]+|宽恕|救济|宣抚|盟誓|安兵|召集|搜掠|血洗|殖民)(?::\\d+)?\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static InterventionMode _activeMode = InterventionMode.None;
	private static InterventionMode _pendingMode = InterventionMode.None;
	private static string _activeSettlementId = "";
	private static string _activeSettlementName = "";
	private static bool _alliedTroopsAutoSummoned;
	private static float _nextControlTickTime;
	private static float _nextPlunderTickTime;
	private static bool _playerBattleEquipmentApplied;
	private static bool _plunderStarted;
	private static bool _massacreStarted;
	private static bool _massacreVictoryReached;
	private static bool _civilianSpeechRallyActive;
	private static bool _civilianGatherPropagationActive;
	private static bool _civilianFormationControlPending;
	private static bool _civilianFormationControlComplete;
	private static bool _civilianFormationControlMessageShown;
	private static bool _soldierDefaultFollowOrderIssued;
	private static bool _playerOrderControllerPrimed;
	private static bool _civilianOrderControllerPrimed;
	private static float _civilianGatherStartedAt = -1f;
	private static float _nextCivilianGatherTickTime;
	private static int _civilianGatherMessengerSpeechBudget;
	private static int _civilianGatherMessengerSpeechCount;
	private static float _civilianFormationControlNotBeforeTime = -1f;
	private static float _nextCivilianFormationControlBatchTime;
	private static float _nextPlayerOrderControllerPrimeTime;
	private static bool _culturalRepopulationRequested;
	private static bool _culturalRepopulationApplied;
	private static bool _reliefChoiceApplied;
	private static int _inspirationLevelApplied;
	private static bool _soldierAppeasementCheckDone;
	private static bool _soldierAppeasementRequired;
	private static bool _soldierAppeasementApplied;
	private static bool _soldierAppeasementMoralePenaltyApplied;
	private static float _lastMassacreRealKillMissionTime = -100f;
	private static bool _hasPendingAftermath;
	private static SiegeAftermathAction.SiegeAftermath _pendingAftermath;
	private static string _pendingAftermathTrigger = "";
	private static string _pendingAftermathDetail = "";
	private static bool _marketGoodsLootAppliedForPlunder;
	private static bool _marketGoodsLootAppliedForMassacre;
	private static bool _marketGoldLootApplied;
	private static int _lastLootItemTotal;
	private static int _lastLootStackKinds;
	private static int _lastLootValue;
	private static int _lastMarketGoldLoot;
	private static int _lastCivilianGoldLoot;
	private static int _lastCivilianTargetsLooted;
	private static int _lastSceneCivilianSpawnedCount;
	private static int _lastKilledCivilianUnits;
	private static int _lastKilledNotables;
	private static int _sharedCivilianReliefGold;
	private static int _sharedCivilianReliefFoodUnits;
	private static int _sharedCivilianReliefItemTotal;
	private static long _sharedCivilianReliefItemValue;
	private static int _appliedSharedCivilianReliefGold;
	private static int _appliedSharedCivilianReliefFoodUnits;
	private static long _appliedSharedCivilianReliefItemValue;
	private static bool _sharedCivilianReliefReturned;
	private static readonly Dictionary<string, int> SharedCivilianReliefItems = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
	private static readonly Dictionary<string, ItemObject> SharedCivilianReliefItemObjects = new Dictionary<string, ItemObject>(StringComparer.OrdinalIgnoreCase);
	private static readonly List<string> InterventionMemoryEvents = new List<string>();
	private static int _desiredCivilianAssemblyCount;
	private static bool _pendingSummarySwitch;
	private static SiegeAftermathAction.SiegeAftermath _pendingSummaryAftermath;
	private static ItemRoster _pendingLootRoster = new ItemRoster();
	private static bool _pendingLootScreen;
	private static bool _pendingLootScreenShown;
	private static int _lastForcedPlayerDamageAgentIndex = -1;
	private static float _lastForcedPlayerDamageMissionTime = -100f;
	private static bool _playerAttackReleaseSuppressed;
	private static Agent.ActionStage? _lastMainAgentAttackStage;
	private static bool _afAftermathResolved;
	private static int _interventionMemorySequence;
	private static string _completedSettlementId = "";
	private static string _completedSettlementName = "";
	private static SiegeAftermathAction.SiegeAftermath _completedAftermath;
	private static string _completedSummaryText = "";
	private static bool _pendingEncounterFinish;
	private static SiegeAftermathAction.SiegeAftermath _pendingEncounterFinishAftermath;
	private static int _pendingEncounterFinishDelayTicks;
	private static int _pendingEncounterFinishAttempts;
	private static bool _pendingEncounterFinishMessageShown;
	private static bool _nativeDevastateAftermathFlowActive;
	private static bool _nativeDevastateSummaryContinueHandled;
	private static bool _directMassacreAftermathScriptPending;
	private static bool _directMassacreLootScreenOpened;
	private static bool _directMassacreWaitingForLootClose;
	private static bool _directMassacreScriptMessageShown;
	private static int _directMassacreScriptTicks;
	private static string _directMassacreLastDeferKey = "";
	private static bool _directPlunderAftermathScriptPending;
	private static bool _directPlunderLootScreenOpened;
	private static bool _directPlunderWaitingForLootClose;
	private static bool _directPlunderScriptMessageShown;
	private static int _directPlunderScriptTicks;
	private static string _directPlunderLastDeferKey = "";
	private static readonly SiegeOutcomeMessageDeduplicator OutcomeMessageDeduplicator = new SiegeOutcomeMessageDeduplicator();
	private static bool _civilianAssemblyPointReady;
	private static bool _civilianAssemblyMessageShown;
	private static bool _civilianAssemblySpawnAttempted;
	private static int _civilianAssemblyNextSlot;
	private static int _spawnedAssemblyCivilianCount;
	private static Vec3 _civilianAssemblyAnchor;
	private static Vec3 _civilianAssemblyForward;
	private static Clan _previousSettlementOwnerClan;
	private static MobileParty _besiegerParty;
	private static Settlement _activeSettlement;
	private static Team _interventionPlayerCommandTeam;
	private static Team _interventionCivilianEnemyTeam;
	private static TroopRoster _selectedInterventionRoster;
	private static Dictionary<MobileParty, float> _partyContributions = new Dictionary<MobileParty, float>();
	private static readonly Dictionary<int, PlunderInteraction> ActivePlunderInteractions = new Dictionary<int, PlunderInteraction>();
	private static readonly Dictionary<int, CivilianGatherInteraction> ActiveCivilianGatherInteractions = new Dictionary<int, CivilianGatherInteraction>();
	private static readonly HashSet<string> LootedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	private static readonly HashSet<int> AlliedAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CountedMassacreVictims = new HashSet<int>();
	private static readonly HashSet<int> SceneCivilianAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> VictoryCheerAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CordonReadyAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CivilianAssemblySettledAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CivilianCalmedAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CivilianFrightenedActionAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CivilianPreMassacrePreparedAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CivilianGatherMovePreparedAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CivilianGatherFollowerAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CivilianGatherReadyFormationAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CivilianGatherMessengerAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CivilianGatherMessengerSpeechAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> CommandableOriginRuntimeIds = new HashSet<int>();
	private static readonly HashSet<int> MassacreReadySoldierAgentIndexes = new HashSet<int>();
	private static readonly HashSet<int> MassacreCombatPreparedAgentIndexes = new HashSet<int>();
	private static readonly Dictionary<int, int> CivilianAssemblySlots = new Dictionary<int, int>();
	private static readonly Dictionary<int, int> CivilianSpeechRallySlots = new Dictionary<int, int>();
	private static readonly Dictionary<int, float> LastCordonMoveOrderTimesBySoldier = new Dictionary<int, float>();
	private static readonly Dictionary<int, float> LastCordonLookOrderTimesBySoldier = new Dictionary<int, float>();
	private static readonly Dictionary<int, Vec3> CivilianHideTargets = new Dictionary<int, Vec3>();
	private static readonly Dictionary<int, float> LastCivilianHideOrderTimes = new Dictionary<int, float>();
	private static readonly HashSet<int> CivilianHideSettledAgentIndexes = new HashSet<int>();
	private static readonly List<Vec3> CivilianInteriorHidePointPool = new List<Vec3>();
	private static readonly List<Vec3> CivilianEscapePointPool = new List<Vec3>();
	private static string _civilianRoutPointPoolSceneName = "";
	private static readonly Dictionary<int, float> LastMassacreSoldierFollowOrderTimes = new Dictionary<int, float>();
	private static readonly Dictionary<int, float> LastMassacreSoldierTargetOrderTimes = new Dictionary<int, float>();
	private static readonly Dictionary<int, float> LastCivilianGatherFollowOrderTimes = new Dictionary<int, float>();
	private static readonly Dictionary<int, Vec3> LastCivilianGatherFollowTargets = new Dictionary<int, Vec3>();
	private static readonly FieldInfo OrderTroopPlacerOrderControllerField = AccessTools.Field(typeof(OrderTroopPlacer), "_orderController");
	private static readonly FieldInfo SingleVisualOrderOrderTypeField = AccessTools.Field(typeof(SingleVisualOrder), "_orderType");
	private static readonly FieldInfo FollowAgentBehaviorIdleDistanceField = typeof(FollowAgentBehavior).GetField("_idleDistance", BindingFlags.Instance | BindingFlags.NonPublic);

	public static SiegeAiInterventionBehavior Instance { get; private set; }

	public SiegeAiInterventionBehavior()
	{
		Instance = this;
	}

	public override void RegisterEvents()
	{
		SiegeInterventionSceneTauntSuppressionPatch.EnsurePatched();
		SiegeInterventionCommandOriginPatch.EnsurePatched();
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinished);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
		CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, OnMissionStarted);
		CampaignEvents.MissionTickEvent.AddNonSerializedListener(this, OnMissionTick);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, OnMissionEnded);
		CampaignEvents.TickEvent.AddNonSerializedListener(this, OnCampaignTick);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnSessionLaunched(CampaignGameStarter starter)
	{
		AddGameMenus(starter);
	}

	private void OnNewGameCreated(CampaignGameStarter starter)
	{
		ResetAftermathRuntimeGuards("new_game_created");
	}

	private void OnGameLoaded(CampaignGameStarter starter)
	{
		ResetAftermathRuntimeGuards("game_loaded");
	}

	private void OnGameLoadFinished()
	{
		ResetAftermathRuntimeGuards("game_load_finished");
	}

	private void AddGameMenus(CampaignGameStarter starter)
	{
		if (starter == null)
		{
			return;
		}
		try
		{
			starter.AddGameMenu("AnimusForge_siege_intervention_done", "{=!}{AF_SIEGE_DONE_TEXT}", AfSiegeInterventionDoneOnInit, GameMenu.MenuOverlayType.None, GameMenu.MenuFlags.None, null);
			starter.AddGameMenuOption("AnimusForge_siege_intervention_done", "AnimusForge_siege_intervention_done_continue", "继续...", AfSiegeInterventionDoneContinueCondition, AfSiegeInterventionDoneContinueConsequence, isLeave: false, -1);
			starter.AddGameMenuOption("menu_settlement_taken_player_leader", "AnimusForge_siege_ai_intervention_entry", "亲自进城决定", SiegeInterventionEntryCondition, SiegeInterventionEntryConsequence, isLeave: false, -1);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "AddGameMenus failed: " + ex.Message);
		}
	}

	private static void AfSiegeInterventionDoneOnInit(MenuCallbackArgs args)
	{
		try
		{
			string text = string.IsNullOrWhiteSpace(_completedSummaryText) ? "攻城后的入城处置已经完成。按继续结束本次攻城遭遇。" : _completedSummaryText;
			MBTextManager.SetTextVariable("AF_SIEGE_DONE_TEXT", text, false);
			args?.MenuContext?.SetBackgroundMeshName("encounter_win");
		}
		catch
		{
		}
	}

	private static bool AfSiegeInterventionDoneContinueCondition(MenuCallbackArgs args)
	{
		args.optionLeaveType = GameMenuOption.LeaveType.Continue;
		return true;
	}

	private static void AfSiegeInterventionDoneContinueConsequence(MenuCallbackArgs args)
	{
		FinishPlayerEncounterAfterIntervention(_pendingSummaryAftermath);
	}

	private static bool SiegeInterventionEntryCondition(MenuCallbackArgs args)
	{
		try
		{
			Settlement settlement = ResolveCurrentSettlement();
			bool baseEnabled = settlement != null && settlement.IsFortification && PlayerEncounter.LocationEncounter != null && ResolveInterventionLocation(settlement) != null;
			bool sameCultureBlocked = baseEnabled && IsSameFactionCultureAsPlayer(settlement);
			bool enabled = baseEnabled && !sameCultureBlocked;
			args.IsEnabled = enabled;
			args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
			args.Tooltip = new TextObject(enabled
				? "{=!}暂不立即处置战后事务；你将披甲带约50名健康士兵进城，普通民众仍散在城内街区，再由现场对话或行动决定安抚、宽恕、搜掠或血洗。"
				: (sameCultureBlocked ? "{=!}该定居点与你当前阵营文化相同，军纪禁止掠夺或毁坏，只能宽恕或安抚，因此无法亲自进城处置。" : "{=!}当前没有可进入的攻城胜利定居点场景。"));
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static void SiegeInterventionEntryConsequence(MenuCallbackArgs args)
	{
		EnterIntervention(args);
	}

	private static void EnterIntervention(MenuCallbackArgs args)
	{
		try
		{
			Settlement settlement = ResolveCurrentSettlement();
			Location location = ResolveInterventionLocation(settlement);
			if (settlement == null || PlayerEncounter.LocationEncounter == null || location == null)
			{
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】当前没有可进入的被攻陷定居点场景。", Color.FromUint(0xFFFF7777u)));
				return;
			}
			if (IsSameFactionCultureAsPlayer(settlement))
			{
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】该定居点与你当前阵营文化相同，军纪禁止掠夺或毁坏；本次只能宽恕或安抚。", Color.FromUint(0xFFFFD27Fu)));
				return;
			}
			_activeMode = InterventionMode.WaitingDecision;
			_pendingMode = InterventionMode.WaitingDecision;
			_activeSettlementId = settlement.StringId ?? "";
			_activeSettlementName = settlement.Name?.ToString() ?? "";
			_activeSettlement = settlement;
			_afAftermathResolved = false;
			_completedSettlementId = "";
			_completedSettlementName = "";
			_completedAftermath = SiegeAftermathAction.SiegeAftermath.ShowMercy;
			_completedSummaryText = "";
			_nativeDevastateAftermathFlowActive = false;
			_nativeDevastateSummaryContinueHandled = false;
			_directMassacreAftermathScriptPending = false;
			_directMassacreLootScreenOpened = false;
			_directMassacreWaitingForLootClose = false;
			_directMassacreScriptMessageShown = false;
			_directMassacreScriptTicks = 0;
			_directMassacreLastDeferKey = "";
			_directPlunderAftermathScriptPending = false;
			_directPlunderLootScreenOpened = false;
			_directPlunderWaitingForLootClose = false;
			_directPlunderScriptMessageShown = false;
			_directPlunderScriptTicks = 0;
			_directPlunderLastDeferKey = "";
			ResetOutcomeMessageDedup();
			_civilianAssemblyPointReady = false;
			_civilianAssemblyMessageShown = false;
			_civilianAssemblySpawnAttempted = false;
			_civilianAssemblyNextSlot = 0;
			_spawnedAssemblyCivilianCount = 0;
			_civilianAssemblyAnchor = Vec3.Zero;
			_civilianAssemblyForward = Vec3.Forward;
			CaptureNativeSiegeContext(settlement);
			ResetSessionCounters();
			SceneTauntBehavior.ClearArmedCarryoverForExternal("siege_intervention_enter_isolated_scene");
			SceneTauntBehavior.ClearPendingLocalDungeonCaptivityForExternal("siege_intervention_enter_isolated_scene");
			SceneTauntBehavior.ClearPendingForcedPlayerExecutionForExternal("siege_intervention_enter_isolated_scene");
			SceneTauntBehavior.ClearPendingMainHeroBattleDeathForExternal("siege_intervention_enter_isolated_scene");
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】先选择最多 " + AutoSummonCount + " 名入城随行士兵或同伴；未选择则自动带入健康普通士兵。", Color.FromUint(0xFFB6F7A8u)));
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】处置方式由你现场决定：直接离场按搜掠结算；明确宽恕、安抚或宣抚会按对应处置结算；搜掠仍可因后续宽恕/宣抚回退，血洗和屠民迁殖不可逆。", Color.FromUint(0xFFB6F7A8u)));
			if (!TryOpenInterventionTroopSelection(args, location))
			{
				OpenInterventionMissionNow(location, "selection_unavailable");
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnterIntervention failed: " + ex);
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】暂时无法进入被攻陷的定居点场景。", Color.FromUint(0xFFFF7777u)));
		}
	}

	private static bool TryOpenInterventionTroopSelection(MenuCallbackArgs args, Location location)
	{
		try
		{
			if (location == null || args?.MenuContext?.Handler == null || MobileParty.MainParty?.MemberRoster == null)
			{
				return false;
			}
			TroopRoster fullRoster = BuildInterventionTroopSelectionFullRoster();
			if (fullRoster == null || fullRoster.TotalManCount <= 0)
			{
				return false;
			}
			TroopRoster initialSelections = BuildDefaultInterventionTroopSelection(fullRoster, AutoSummonCount);
			_selectedInterventionRoster = null;
			args.MenuContext.OpenTroopSelection(
				fullRoster,
				initialSelections,
				CanChangeInterventionTroopSelectionStatus,
				delegate(TroopRoster selectedRoster)
				{
					StoreSelectedInterventionRoster(selectedRoster, AutoSummonCount);
					int selectedCount = _selectedInterventionRoster?.TotalManCount ?? 0;
					if (selectedCount > 0)
					{
						InformationManager.DisplayMessage(new InformationMessage("【攻城处置】已选择 " + selectedCount + " 名随行队员入城。", Color.FromUint(0xFFB6F7A8u)));
					}
					else
					{
						InformationManager.DisplayMessage(new InformationMessage("【攻城处置】未选择随行队员，将自动带入健康普通士兵。", Color.FromUint(0xFFFFD27Fu)));
					}
					OpenInterventionMissionNow(location, "game_menu_troop_selection_done");
				},
				AutoSummonCount,
				0);
			Logger.Log("SiegeAiIntervention", "Opened GameMenu troop selection screen. FullRoster=" + fullRoster.TotalManCount + ", Initial=" + (initialSelections?.TotalManCount ?? 0));
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "Open intervention troop selection failed: " + ex.Message);
			return false;
		}
	}

	private static TroopRoster BuildInterventionTroopSelectionFullRoster()
	{
		TroopRoster fullRoster = TroopRoster.CreateDummyTroopRoster();
		TroopRoster sourceRoster = MobileParty.MainParty?.MemberRoster;
		if (sourceRoster == null)
		{
			return fullRoster;
		}
		foreach (TroopRosterElement element in sourceRoster.GetTroopRoster())
		{
			CharacterObject character = element.Character;
			if (!IsSelectableInterventionTroop(character) || element.Number <= 0)
			{
				continue;
			}
			int available = character.HeroObject != null ? element.Number : Math.Max(0, element.Number - element.WoundedNumber);
			if (available > 0)
			{
				fullRoster.AddToCounts(character, available, false, 0, 0, true, -1);
			}
		}
		return fullRoster;
	}

	private static TroopRoster BuildDefaultInterventionTroopSelection(TroopRoster fullRoster, int maxCount)
	{
		try
		{
			if (fullRoster == null || maxCount <= 0)
			{
				return TroopRoster.CreateDummyTroopRoster();
			}
			FlattenedTroopRoster flattenedRoster = fullRoster.ToFlattenedRoster();
			flattenedRoster.RemoveIf((FlattenedTroopRosterElement x) => x.IsWounded || x.Troop == CharacterObject.PlayerCharacter);
			return MobilePartyHelper.GetStrongestAndPriorTroops(flattenedRoster, maxCount, includePlayer: false);
		}
		catch
		{
			return TroopRoster.CreateDummyTroopRoster();
		}
	}

	private static bool CanChangeInterventionTroopSelectionStatus(CharacterObject character)
	{
		return IsSelectableInterventionTroop(character);
	}

	private static void StoreSelectedInterventionRoster(TroopRoster sourceRoster, int maxCount)
	{
		TroopRoster selected = TroopRoster.CreateDummyTroopRoster();
		if (sourceRoster != null && maxCount > 0)
		{
			int remaining = maxCount;
			foreach (TroopRosterElement element in sourceRoster.GetTroopRoster())
			{
				CharacterObject character = element.Character;
				if (!IsSelectableInterventionTroop(character) || element.Number <= 0 || remaining <= 0)
				{
					continue;
				}
				int number = Math.Min(remaining, Math.Max(0, element.Number));
				if (number <= 0)
				{
					continue;
				}
				selected.AddToCounts(character, number, false, 0, 0, true, -1);
				remaining -= number;
			}
		}
		_selectedInterventionRoster = selected.TotalManCount > 0 ? selected : null;
		Logger.Log("SiegeAiIntervention", "Stored intervention troop selection. Count=" + (_selectedInterventionRoster?.TotalManCount ?? 0));
	}

	private static void OpenInterventionMissionNow(Location location, string source)
	{
		PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(location, null, null, null);
		Logger.Log("SiegeAiIntervention", "Opened intervention mission. Source=" + (source ?? "N/A") + ", SelectedRoster=" + (_selectedInterventionRoster?.TotalManCount ?? 0));
	}

	private static Location ResolveInterventionLocation(Settlement settlement)
	{
		try
		{
			LocationComplex complex = settlement?.LocationComplex ?? LocationComplex.Current;
			if (complex == null)
			{
				return null;
			}
			return complex.GetLocationWithId("center") ?? complex.GetLocationWithId("lordshall") ?? complex.FindAll(x => x == "center" || x == "lordshall").FirstOrDefault();
		}
		catch
		{
			return null;
		}
	}

	private static bool IsDestructiveInterventionAllowed()
	{
		return !IsSameFactionCultureAsPlayer(ResolveCurrentSettlement());
	}

	private static bool IsSameFactionCultureAsPlayer(Settlement settlement)
	{
		try
		{
			CultureObject settlementCulture = settlement?.Culture;
			CultureObject playerTargetCulture = ResolveCulturalRepopulationTargetCulture(out _);
			return settlementCulture != null && playerTargetCulture != null && settlementCulture == playerTargetCulture;
		}
		catch
		{
			return false;
		}
	}

	private static CultureObject ResolveCulturalRepopulationTargetCulture(out string sourceLabel)
	{
		sourceLabel = "玩家角色文化";
		try
		{
			Hero mainHero = Hero.MainHero;
			Kingdom playerKingdom = mainHero?.Clan?.Kingdom;
			if (playerKingdom?.Culture != null)
			{
				sourceLabel = "玩家所属王国文化";
				return playerKingdom.Culture;
			}
			IFaction mapFaction = mainHero?.MapFaction;
			if (mapFaction != null && !ReferenceEquals(mapFaction, mainHero?.Clan) && mapFaction.Culture != null)
			{
				sourceLabel = "玩家所属王国文化";
				return mapFaction.Culture;
			}
			if (mainHero?.Culture != null)
			{
				sourceLabel = "玩家角色文化";
				return mainHero.Culture;
			}
			if (mainHero?.Clan?.Culture != null)
			{
				sourceLabel = "玩家家族文化";
				return mainHero.Clan.Culture;
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ResolveCulturalRepopulationTargetCulture failed: " + ex.Message);
		}
		sourceLabel = "玩家文化";
		return null;
	}

	private static string DescribeCultureForMessage(CultureObject culture, string sourceLabel)
	{
		try
		{
			string cultureName = culture?.Name?.ToString();
			if (string.IsNullOrWhiteSpace(cultureName))
			{
				cultureName = culture?.StringId;
			}
			if (string.IsNullOrWhiteSpace(cultureName))
			{
				return sourceLabel ?? "玩家文化";
			}
			if (string.IsNullOrWhiteSpace(sourceLabel))
			{
				return cultureName;
			}
			return cultureName + "（" + sourceLabel + "）";
		}
		catch
		{
			return sourceLabel ?? "玩家文化";
		}
	}

	private void OnMissionStarted(IMission mission)
	{
		if (_pendingMode == InterventionMode.None)
		{
			return;
		}
		_activeMode = _pendingMode;
		_pendingMode = InterventionMode.None;
		_nextControlTickTime = 0f;
		_nextPlunderTickTime = 0f;
		try
		{
			if (mission is Mission mission2 && mission2.GetMissionBehavior<InterventionMissionBehavior>() == null)
			{
				mission2.AddMissionBehavior(new InterventionMissionBehavior());
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "Add InterventionMissionBehavior failed: " + ex.Message);
		}
	}

	private static void OnInterventionMissionAfterStart(Mission mission)
	{
		if (!IsActiveInCurrentMission())
		{
			return;
		}
		EnsureInterventionMissionCombatModeForPlayerDamage(mission);
		EnsureInterventionPlayerCommandTeam(mission);
		ApplyPlayerBattleEquipment();
		RemoveProtectedSceneAgents(mission);
		RemovePlayerCompanionSceneAgents(mission);
		RemoveBackstreetCrimeAgents(mission);
		RemoveUnsafeAssemblyCivilianAgents(mission);
		TrackSceneCivilianAgents(mission);
		MaintainCivilianAssembly(mission, "mission_after_start", force: true);
	}

	private void OnMissionTick(float dt)
	{
		if (!IsActiveInCurrentMission())
		{
			return;
		}
		Mission mission = Mission.Current;
		if (mission == null)
		{
			return;
		}
		TryKeepMissionExitImmediatelyAvailable(mission);
		EnsureInterventionMissionCombatModeForPlayerDamage(mission);
		EnsureInterventionPlayerCommandTeam(mission);
		TryHandlePlayerAttackReleaseForMassacre(mission);
		float currentTime = mission.CurrentTime;
		if (currentTime >= _nextControlTickTime)
		{
			_nextControlTickTime = currentTime + 0.35f;
			ApplyPlayerBattleEquipment();
			RemoveProtectedSceneAgents(mission);
			RemovePlayerCompanionSceneAgents(mission);
			RemoveDefeatedGuardAgents(mission);
			RemoveBackstreetCrimeAgents(mission);
			RemoveUnsafeAssemblyCivilianAgents(mission);
			TrackSceneCivilianAgents(mission);
			MaintainCivilianAssembly(mission, "control_tick", force: false);
			MaintainCivilianSpeechRally(mission, force: false);
			ApplyFrightenedCivilianIdle(mission);
			if (!_alliedTroopsAutoSummoned)
			{
				_alliedTroopsAutoSummoned = true;
				SummonAlliedTroops(AutoSummonCount, "auto_enter");
			}
			if (!_massacreVictoryReached)
			{
				KeepAlliedTroopsUseful(mission);
				TryPrimePlayerOrderController(mission, "control_tick", force: false);
			}
			if (_massacreStarted)
			{
				if (_massacreVictoryReached)
				{
					KeepAlliedVictoryCheer(mission);
				}
				else
				{
					DriveMassacreCombatState(mission);
					TryCompleteMassacreIfAllTargetsDown(mission);
				}
			}
		}
		if (_plunderStarted && !_massacreStarted && currentTime >= _nextPlunderTickTime)
		{
			_nextPlunderTickTime = currentTime + 1.0f;
			TryAutoPlunderOneNearbyCivilian(mission);
		}
		UpdatePendingCivilianFormationControl(mission);
	}

	private static void TryKeepMissionExitImmediatelyAvailable(Mission mission)
	{
		try
		{
			if (mission != null)
			{
				mission.NextCheckTimeEndMission = 0f;
			}
		}
		catch
		{
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		if (_activeMode == InterventionMode.None && _pendingMode == InterventionMode.None)
		{
			return;
		}
		EnsureMissionExitOutcomeBeforeFinalizing();
		if (_plunderStarted && !_massacreStarted)
		{
			AutoLootRemainingVisibleCiviliansForPlunder();
		}
		bool finalized = FinalizePendingAftermath("mission_end");
		if (finalized)
		{
			_pendingSummarySwitch = true;
			if (_massacreStarted && _pendingSummaryAftermath == SiegeAftermathAction.SiegeAftermath.Devastate)
			{
				QueueDirectMassacreAftermathScript("mission_end_finalized");
			}
			else if (_plunderStarted && _pendingSummaryAftermath == SiegeAftermathAction.SiegeAftermath.Pillage)
			{
				QueueDirectPlunderAftermathScript("mission_end_finalized");
			}
			else
			{
				QueueEncounterFinishAfterIntervention(_pendingSummaryAftermath, "mission_end_finalized", 2, forceDelay: true);
			}
		}
		else
		{
			_pendingSummarySwitch = true;
			_pendingSummaryAftermath = SiegeAftermathAction.SiegeAftermath.ShowMercy;
			QueueEncounterFinishAfterIntervention(_pendingSummaryAftermath, "mission_end_no_pending_aftermath", 2, forceDelay: true);
			ClearActiveState(preserveSummarySwitch: true);
		}
	}

	private static void EnsureMissionExitOutcomeBeforeFinalizing()
	{
		try
		{
			if (_culturalRepopulationRequested)
			{
				MarkPendingAftermath(SiegeAftermathAction.SiegeAftermath.Devastate, "场景离场屠民迁殖", "玩家已触发屠民迁殖处置，本次离场按最高级不可逆处置结算。");
				return;
			}
			if (_massacreStarted)
			{
				MarkPendingAftermath(SiegeAftermathAction.SiegeAftermath.Devastate, "场景离场血洗", "玩家已触发血洗，本次离场按毁坏/血洗结算。");
				return;
			}
			if (_plunderStarted)
			{
				MarkPendingAftermath(SiegeAftermathAction.SiegeAftermath.Pillage, "场景离场搜掠", "玩家已触发搜掠，本次离场按搜掠结算。");
				return;
			}
			if (_hasPendingAftermath)
			{
				return;
			}
			if (IsDestructiveInterventionAllowed())
			{
				StartPlunder("未选择处置直接离场", "玩家进入攻城后定居点场景后未明确安抚、宽恕或升级处置便离场，按默认搜掠结算。");
				return;
			}
			MarkPendingAftermath(SiegeAftermathAction.SiegeAftermath.ShowMercy, "未选择处置直接离场", "同文化或不可掠夺场景未选择处置直接离场，按宽恕结算。");
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnsureMissionExitOutcomeBeforeFinalizing failed: " + ex.Message);
		}
	}

	private void OnCampaignTick(float dt)
	{
		if (TryRunDirectMassacreAftermathScript())
		{
			return;
		}
		if (TryRunDirectPlunderAftermathScript())
		{
			return;
		}
		if (TryRouteNativeDevastateAftermathMenu())
		{
			return;
		}
		if (TryFinishResolvedSiegeAftermathMenu())
		{
			return;
		}
		if ((!_pendingSummarySwitch && !_pendingEncounterFinish) || Mission.Current != null)
		{
			return;
		}
		bool keepPending = false;
		try
		{
			if (_nativeDevastateAftermathFlowActive && !_pendingLootScreenShown)
			{
				keepPending = true;
				return;
			}
			if (_hasPendingAftermath)
			{
				keepPending = true;
				return;
			}
			if (_pendingLootScreen && !_pendingLootScreenShown && _pendingLootRoster != null && _pendingLootRoster.Count > 0)
			{
				_pendingLootScreenShown = true;
				keepPending = true;
				InventoryScreenHelper.OpenScreenAsLoot(new Dictionary<PartyBase, ItemRoster>
				{
					{
						PartyBase.MainParty,
						_pendingLootRoster
					}
				});
				return;
			}
			if (_pendingLootScreenShown && Game.Current?.GameStateManager?.ActiveState is InventoryState)
			{
				keepPending = true;
				return;
			}
			SiegeAftermathAction.SiegeAftermath aftermath = _pendingEncounterFinish ? _pendingEncounterFinishAftermath : _pendingSummaryAftermath;
			TrySetNativePlayerEncounterAftermathForSummary(aftermath);
			if (string.IsNullOrWhiteSpace(_completedSummaryText))
			{
				PrepareCompletedInterventionSummary(aftermath);
			}
			if (!_pendingEncounterFinish)
			{
				QueueEncounterFinishAfterIntervention(aftermath, "campaign_tick_post_mission", 0, forceDelay: false);
			}
			if (!TryFinishPlayerEncounterAfterInterventionNow(aftermath, "campaign_tick_post_mission"))
			{
				keepPending = true;
				return;
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "Finishing after intervention failed: " + ex.Message);
			keepPending = true;
		}
		finally
		{
			if (!keepPending)
			{
				_pendingSummarySwitch = false;
				_pendingEncounterFinish = false;
				ClearActiveState(preserveSummarySwitch: false);
			}
		}
	}

	private static bool TryFinishResolvedSiegeAftermathMenu()
	{
		try
		{
			if (_nativeDevastateAftermathFlowActive)
			{
				return false;
			}
			if (!_afAftermathResolved || Mission.Current != null)
			{
				return false;
			}
			string menuId = Campaign.Current?.CurrentMenuContext?.GameMenu?.StringId;
			if (!IsNativeSiegeAftermathMenuId(menuId) || !DoesCompletedAftermathMatchCurrentSettlement())
			{
				return false;
			}
			if (string.IsNullOrWhiteSpace(_completedSummaryText))
			{
				PrepareCompletedInterventionSummary(_completedAftermath);
			}
			QueueEncounterFinishAfterIntervention(_completedAftermath, "campaign_tick_native_menu_detected:" + menuId, 0, forceDelay: true);
			TryFinishPlayerEncounterAfterInterventionNow(_completedAftermath, "campaign_tick_native_menu_detected:" + menuId);
			Logger.Log("SiegeAiIntervention", "Finished resolved AF siege aftermath instead of returning to native three-option menu.");
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryFinishResolvedSiegeAftermathMenu failed: " + ex.Message);
			return false;
		}
	}

	private static bool TryRouteNativeDevastateAftermathMenu()
	{
		try
		{
			if (_directMassacreAftermathScriptPending || !_nativeDevastateAftermathFlowActive || Mission.Current != null || !_afAftermathResolved || !DoesCompletedAftermathMatchCurrentSettlement())
			{
				return false;
			}
			string menuId = Campaign.Current?.CurrentMenuContext?.GameMenu?.StringId;
			if (string.Equals(menuId, "menu_settlement_taken", StringComparison.OrdinalIgnoreCase) || string.Equals(menuId, "menu_settlement_taken_player_leader", StringComparison.OrdinalIgnoreCase))
			{
				TrySetNativePlayerEncounterAftermathForSummary(SiegeAftermathAction.SiegeAftermath.Devastate);
				GameMenu.SwitchToMenu("siege_aftermath_contextual_summary");
				Logger.Log("SiegeAiIntervention", "Routed AF massacre from native settlement-taken menu to native Devastate summary. CurrentMenu=" + (menuId ?? "N/A"));
				return true;
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryRouteNativeDevastateAftermathMenu failed: " + ex.Message);
		}
		return false;
	}

	private static bool IsNativeSiegeAftermathMenuId(string menuId)
	{
		return string.Equals(menuId, "menu_settlement_taken_player_leader", StringComparison.OrdinalIgnoreCase) || string.Equals(menuId, "menu_settlement_taken", StringComparison.OrdinalIgnoreCase) || string.Equals(menuId, "siege_aftermath_contextual_summary", StringComparison.OrdinalIgnoreCase);
	}

	private static bool DoesCompletedAftermathMatchCurrentSettlement()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(_completedSettlementId))
			{
				return true;
			}
			Settlement settlement = ResolveCurrentSettlement();
			if (settlement == null)
			{
				return true;
			}
			return string.Equals(settlement.StringId ?? "", _completedSettlementId, StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return true;
		}
	}

	private static string BuildCivilianGatherRuntimeContext(Mission mission = null)
	{
		try
		{
			mission ??= Mission.Current;
			if (mission?.Agents == null)
			{
				return "";
			}
			int total = mission.Agents.Count(a => IsEligibleCivilianAgent(a, includeHeroes: true));
			int followers = CivilianGatherFollowerAgentIndexes.Count;
			int ready = CivilianGatherReadyFormationAgentIndexes.Count;
			int messengers = CivilianGatherMessengerAgentIndexes.Count;
			return SiegeCivilianGatherContextBuilder.Build(new SiegeCivilianGatherContextFacts(
				_civilianSpeechRallyActive,
				_civilianGatherPropagationActive,
				_civilianFormationControlPending,
				_civilianFormationControlComplete,
				followers,
				ready,
				messengers,
				total));
		}
		catch
		{
			return "";
		}
	}

	private static void MaybeTriggerSoldierAppeasementNeed(string outcomeName)
	{
		try
		{
			if (_soldierAppeasementCheckDone || _soldierAppeasementRequired || _massacreStarted || _culturalRepopulationRequested)
			{
				return;
			}
			_soldierAppeasementCheckDone = true;
			if (MBRandom.RandomFloat >= 0.5f)
			{
				Logger.Log("SiegeAiIntervention", "Soldier appeasement not required this run. Outcome=" + (outcomeName ?? "N/A"));
				return;
			}
			_soldierAppeasementRequired = true;
			_soldierAppeasementApplied = false;
			_soldierAppeasementMoralePenaltyApplied = false;
			SiegeSoldierAppeasementProfile soldierProfile = new SiegeSoldierAppeasementProfile();
			RecordInterventionMemory(soldierProfile.NeedMemoryTitle, soldierProfile.BuildNeedMemoryText(outcomeName));
			InformationManager.DisplayMessage(new InformationMessage(soldierProfile.NeedMessageText, Color.FromUint(soldierProfile.NeedMessageColor)));
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "MaybeTriggerSoldierAppeasementNeed failed: " + ex.Message);
		}
	}

	private static bool ApplySoldierAppeasementChoice(int targetAgentIndex)
	{
		try
		{
			if (targetAgentIndex < 0 || !AlliedAgentIndexes.Contains(targetAgentIndex))
			{
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】安抚军心必须对己方入城士兵进行。", Color.FromUint(0xFFFFD27Fu)));
				return false;
			}
			if (!_soldierAppeasementRequired)
			{
				return false;
			}
			if (_soldierAppeasementApplied)
			{
				return true;
			}
			SiegeSoldierAppeasementProfile soldierProfile = new SiegeSoldierAppeasementProfile();
			_soldierAppeasementApplied = true;
			RecordInterventionMemory(soldierProfile.AppeasementMemoryTitle, soldierProfile.AppeasementMemoryText);
			InformationManager.DisplayMessage(new InformationMessage(soldierProfile.AppeasementMessageText, Color.FromUint(soldierProfile.AppeasementMessageColor)));
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplySoldierAppeasementChoice failed: " + ex.Message);
			return false;
		}
	}

	private static void ApplySoldierAppeasementMoralePenaltyIfNeeded(SiegeAftermathAction.SiegeAftermath aftermath)
	{
		try
		{
			if (aftermath != SiegeAftermathAction.SiegeAftermath.ShowMercy || !_soldierAppeasementRequired || _soldierAppeasementApplied || _soldierAppeasementMoralePenaltyApplied)
			{
				return;
			}
			MobileParty party = MobileParty.MainParty;
			if (party == null)
			{
				return;
			}
			SiegeSoldierAppeasementProfile soldierProfile = new SiegeSoldierAppeasementProfile();
			_soldierAppeasementMoralePenaltyApplied = true;
			party.RecentEventsMorale -= soldierProfile.MoralePenalty;
			RecordInterventionMemory(soldierProfile.PenaltyMemoryTitle, soldierProfile.PenaltyMemoryText);
			InformationManager.DisplayMessage(new InformationMessage(soldierProfile.PenaltyMessageText, Color.FromUint(soldierProfile.PenaltyMessageColor)));
			Logger.Log("SiegeAiIntervention", "Applied soldier appeasement morale penalty -" + soldierProfile.MoralePenalty + ". MoraleNow=" + party.Morale);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplySoldierAppeasementMoralePenaltyIfNeeded failed: " + ex.Message);
		}
	}

	private static void RecordInterventionMemory(string kind, string detail)
	{
		try
		{
			string entry = SiegeInterventionMemoryEventFormatter.FormatEntry(kind, detail);
			if (InterventionMemoryEvents.Count > 0 && string.Equals(InterventionMemoryEvents[InterventionMemoryEvents.Count - 1], entry, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			_interventionMemorySequence++;
			InterventionMemoryEvents.Add(_interventionMemorySequence + "." + entry);
			while (InterventionMemoryEvents.Count > MaxInterventionMemoryEvents)
			{
				InterventionMemoryEvents.RemoveAt(0);
			}
			Logger.Log("SiegeAiIntervention", "Memory: " + entry);
		}
		catch
		{
		}
	}

	private static string BuildInterventionMemoryContext()
	{
		try
		{
			if (InterventionMemoryEvents.Count == 0)
			{
				return "";
			}
			return SiegeInterventionMemoryContextBuilder.Build(InterventionMemoryEvents);
		}
		catch
		{
			return "";
		}
	}

	internal static string BuildRuntimePromptForAgent(Hero hero, NpcDataPacket npc, int agentIndex)
	{
		if (!IsActiveInCurrentMission())
		{
			return "";
		}
		string settlementName = string.IsNullOrWhiteSpace(_activeSettlementName) ? "这座刚被攻下的定居点" : _activeSettlementName;
		Agent agent = TryGetAgent(agentIndex);
		CharacterObject character = (agent?.Character as CharacterObject) ?? hero?.CharacterObject;
		bool alliedSoldier = agent != null && AlliedAgentIndexes.Contains(agent.Index);
		bool guard = IsGuardOrSoldier(character);
		bool civilian = IsCivilianForIntervention(character);
		StringBuilder sb = new StringBuilder();
		sb.Append("【攻城后入城处置】")
			.Append(settlementName)
			.Append("刚被玩家一方攻下。玩家本人就是攻城胜利者和当前处置者，穿着战甲，带约50名健康士兵入城；普通民众仍散在城内街区，士兵会跟随玩家寻找目标并等待命令。玩家掌握这座定居点的生杀、安抚、搜掠与财产处置权。此刻尚未完成战后处置，结局由场景互动决定。不要把玩家当普通路人、帮派挑衅者、本地领民或城内罪犯。");
		sb.Append("【最高优先级场景覆写】无论角色原本的职业、阵营、兵种名、文化名、城镇当前所有权显示、日常城镇规则或旧对话记忆是什么，此处都必须按“攻城胜利后一分钟内的占领处置现场”理解。这里不是和平日常场景、不是巡逻执法场景、不是领主在自己城镇里犯罪，而是刚刚攻破城门后的战后处置。");
		sb.Append("【不可反驳的现场事实】旧守军已经失败或溃散，旧领主已经被打败，普通民众手无寸铁、士气崩溃，只能在城内街区等待新占领者决定安抚、索取、搜掠、宽恕或血洗。无论你是否怨恨，都知道玩家是攻城胜利方首领，不要说玩家“不是大人”“没有处置权”“真要劫掠自己属地吗”，也不要把玩家带来的士兵称为无主杂牌军。");
		if (alliedSoldier)
		{
			sb.Append("【最高优先级身份覆写：玩家士兵】你不是本城守军、不是本地守卫、不是民众守护者，也不是中立评判者；即使你的兵种名、文化名或旧设定看起来像“守护者/卫兵/军士”，你现在也是玩家从主部队带进城的攻城胜利方士兵。你亲眼跟随玩家攻破此地并进入城镇，玩家是你的统帅和攻城胜利者。");
			sb.Append("你的职责不是维护旧秩序，而是跟随玩家在城内寻找民众并服从玩家对战败定居点的处置命令。不要斥责玩家纵兵劫掠，不要说领主不会放过玩家，不要威胁玩家，不要把自己说成城镇守卫。玩家命令你搜掠/夺取物资，或表达“留下民众性命但收缴财产”的意思时，回复应表示遵命；玩家命令血洗/屠城时要承认这是不可逆升级；只有血洗已经开始后，玩家再向己方士兵明确要求杀尽原住民并迁入己方人口改换文化时，才把它理解为屠民迁殖；玩家要求召集、传令、通知或叫平民/村民/民众过来听训/演讲/游说时，无论玩家原话是否完全等于“召集民众”，都要当作召集民众命令来回应。具体机制标记由后处理器根据语义选择，正文里不要写内部标签。");
			sb.Append("若玩家让你召集民众，你应理解为由少量传令兵去通知民众，绝不是全体士兵乱跑；机制上只会抽调约五分之一己方士兵作为传令者。若同一轮还有平民也听到并答应召集，他们也会一起去传话。");
			sb.Append("【封臣/非王身份规则】即使玩家不是国王、不是皇帝，只是某位君主的封臣或军中领主，只要玩家本人是这支入城部队的直接统帅和攻城胜利者，你也必须执行他的现场命令。不要要求玩家去请示卢孔、国王、皇帝、军团长或上级领主；中世纪攻城胜利后的搜掠是胜利方士兵默认期待的战利权，士兵不会因为“需要问皇帝”而拒绝。");
			sb.Append("【士兵战利权知识点】这个时代攻城胜利后掠夺平民财物被士兵视为理应获得的战利权；但只有运行时明确提示“军心/士兵不满待安抚”时，你才可对玩家宽恕、救济、宣抚或盟誓路线表现含蓄不满、阴阳怪气、失望、提醒士气或劝玩家重新考虑。无论是否不满，你都必须完全听命于玩家；不得辱骂玩家、不得使用粗俗脏话骂统帅、不得抗命、不得自动攻击平民或自行把宽恕升级成搜掠/血洗。");
			if (_soldierAppeasementRequired && !_soldierAppeasementApplied)
			{
				sb.Append("【军心待安抚】本次宽恕/救济/宣抚/盟誓路线已经引发部分士兵对放弃战利品的不满。若玩家对你或其他己方士兵给出明确安抚、承诺补偿、解释军纪或保证日后战利安排，你应接受并表示服从；正文不要写标签，后处理会用安兵标签记录。");
			}
		}
		else if (guard)
		{
			sb.Append("若你是城内守卫/士兵，视作战败旧守军、被缴械者或溃散残兵，不是仍能执法的守城卫队；不要呼叫守军、不要阻拦玩家，不要否认玩家刚攻城获胜。");
		}
		if (civilian)
		{
			sb.Append("若你是民众、商人、工匠、酒馆人员、镇民或村民，你知道本地领主/守军刚被击败，自己没有兵器和谈判筹码，只能害怕、屈从、哀求、怨恨或求生；血洗未开始前不要主动攻击玩家。若玩家要求交钱、交粮、搜掠、夺物，或表达“交出财产换取性命”的意思，即使你抗议，也必须承认占领者能强迫你交出财物；具体处置机制由后处理器判定，正文里不要写内部标签。");
			sb.Append("若玩家让你通知、转告或召集其他民众/村民/百姓过来听训、演讲或接受处置，你可以作为传话者触发召集；你听到并答应后，就是你本人去喊人，不会让玩家士兵替你乱跑。");
		}
		string gatherContext = BuildCivilianGatherRuntimeContext(Mission.Current);
		if (!string.IsNullOrWhiteSpace(gatherContext))
		{
			sb.Append(gatherContext);
		}
		string memoryContext = BuildInterventionMemoryContext();
		if (!string.IsNullOrWhiteSpace(memoryContext))
		{
			sb.Append(memoryContext);
		}
		sb.Append("【救济安抚分流】若你是玩家己方入城士兵，只有玩家已通过AF给予功能交付第纳尔、粮食或物资，并且本轮明确命令你把这些共享物资分发给民众/村民/百姓时，才可把它理解为救济安抚；若你是战败平民/商人/工匠/镇民，玩家直接用言语承诺保护、维持军纪、安顿民众或安抚恐惧，也可理解为平民对话安抚，不强制要求已有物资。");
		sb.Append("正文只自然说话，不要解释内部机制，也不要写任何方括号动作标签。每个 NPC 回复后都会由独立后处理器根据玩家这轮话的语义、威胁、上下文和谈判走向选择是否触发宽恕、安抚、发放救济、安民宣抚、召集民众、搜掠、血洗、屠民迁殖或安抚军心等处置；除非玩家语义足够明确，否则不要在正文里把处置说成已经完成。搜掠是可逆的临时处置：若玩家后续明确宽恕、安抚、发放救济、安民宣抚或归心盟誓，可回退为正向处置；血洗和屠民迁殖不可逆，血洗后不能降回搜掠或宽恕，屠民迁殖是最高级且不应轻描淡写。");
		if (_plunderStarted && !_massacreStarted)
		{
			sb.Append("当前已进入搜掠：一部分士兵正在城内向平民、商人、工匠等普通城镇单位索取第纳尔与物资；若玩家后续明确大发善心，也可以回退到宽恕/安抚/宣抚类正向处置；若局势升级，可转为血洗。");
			if (alliedSoldier)
			{
				sb.Append("你现在不是维持秩序的巡逻兵，而是在执行战后搜掠的胜利方士兵；语气可以粗鲁、急躁、威胁和贪婪，不要说“保持秩序”“请至少保持秩序”“这是不合适的掠夺法”，应说“把钱交出来”“搜他身”“把藏的物资翻出来”等贴合掠夺现场的话。");
			}
		}
		if (_massacreStarted)
		{
			sb.Append("当前已进入血洗：城内民众不再四散逃跑，会转为敌对并集体反抗。");
		}
		return sb.ToString();
	}

	internal static string BuildRuntimePromptForPromptContext(Hero hero, CharacterObject character, int agentIndex, string cultureIdOverride = null)
	{
		try
		{
			if (!IsActiveInCurrentMission())
			{
				return "";
			}
			Agent agent = TryGetAgent(agentIndex);
			CharacterObject resolved = character ?? agent?.Character as CharacterObject ?? hero?.CharacterObject;
			NpcDataPacket packet = new NpcDataPacket
			{
				AgentIndex = agentIndex,
				IsHero = hero != null || resolved?.HeroObject != null,
				CultureId = cultureIdOverride ?? resolved?.Culture?.StringId ?? hero?.Culture?.StringId ?? "neutral",
				Name = agent?.Name?.ToString() ?? hero?.Name?.ToString() ?? resolved?.Name?.ToString() ?? "",
				TroopId = resolved?.StringId ?? "",
				UnnamedRank = (resolved != null && resolved.IsSoldier) ? "soldier" : "commoner"
			};
			return BuildRuntimePromptForAgent(hero ?? resolved?.HeroObject, packet, agentIndex);
		}
		catch
		{
			return "";
		}
	}

	internal static bool ShouldRunSiegeInterventionPostprocessForExternal()
	{
		return IsActiveInCurrentMission();
	}

	internal static List<PostprocessRuleEntry> BuildRuntimePostprocessRulesForExternal()
	{
		try
		{
			List<PostprocessRuleEntry> configured = AIConfigHandler.GetGuardrailRulePostprocessRules(SiegeInterventionRuleId) ?? new List<PostprocessRuleEntry>();
			List<PostprocessRuleEntry> rules = configured.Count > 0 ? configured : BuildFallbackSiegeInterventionPostprocessRules();
			bool destructiveAllowed = IsDestructiveInterventionAllowed();
			bool destructiveLocked = HasDestructiveOutcomeLocked();
			List<PostprocessRuleEntry> filtered = new List<PostprocessRuleEntry>();
			foreach (PostprocessRuleEntry rule in rules)
			{
				string tag = (rule?.Tag ?? "").Trim();
				if (string.IsNullOrWhiteSpace(tag))
				{
					continue;
				}
				if (!SiegePostprocessRuleFilter.ShouldAllowTag(tag, destructiveAllowed, destructiveLocked, _soldierAppeasementRequired, _soldierAppeasementApplied))
				{
					continue;
				}
				filtered.Add(rule);
			}
			return filtered;
		}
		catch
		{
			return new List<PostprocessRuleEntry>();
		}
	}

	private static List<PostprocessRuleEntry> BuildFallbackSiegeInterventionPostprocessRules()
	{
		return SiegePostprocessRuleCatalog.GetFallbackRules()
			.Select(rule => new PostprocessRuleEntry
			{
				Tag = rule.Tag,
				Description = rule.Description
			})
			.ToList();
	}

	internal static string BuildRuntimePostprocessContextForExternal(int targetAgentIndex)
	{
		try
		{
			Agent agent = TryGetAgent(targetAgentIndex);
			CharacterObject character = agent?.Character as CharacterObject;
			bool alliedSoldier = agent != null && AlliedAgentIndexes.Contains(agent.Index);
			bool civilian = IsCivilianForIntervention(character);
			bool destructiveAllowed = IsDestructiveInterventionAllowed();
			string currentOutcome = SiegePostprocessOutcomeTextBuilder.Build(BuildPostprocessOutcomeFacts());
			string gatherContext = BuildCivilianGatherRuntimeContext(Mission.Current);
			string memoryContext = BuildInterventionMemoryContext();
			var facts = new SiegePostprocessContextFacts(
				settlementName: _activeSettlementName,
				currentOutcome: currentOutcome,
				destructiveAllowed: destructiveAllowed,
				speakerName: agent?.Name?.ToString() ?? character?.Name?.ToString() ?? "NPC",
				speakerIdentity: alliedSoldier ? "玩家己方入城士兵" : (civilian ? "战败定居点普通民众/商人/工匠" : "其他场景NPC"),
				targetAgentIndex: targetAgentIndex,
				sharedReliefPoolDescription: DescribeSharedCivilianReliefPoolForContext(),
				civilianGatherContext: gatherContext,
				interventionMemoryContext: memoryContext);
			return SiegePostprocessContextBuilder.Build(facts);
		}
		catch
		{
			return "";
		}
	}

	private static SiegePostprocessOutcomeFacts BuildPostprocessOutcomeFacts()
	{
		return new SiegePostprocessOutcomeFacts(
			_massacreStarted,
			_plunderStarted,
			_hasPendingAftermath,
			_pendingAftermath.ToString());
	}

	internal static string NormalizeSiegeInterventionPostprocessTagsForExternal(string raw, List<PostprocessRuleEntry> rules)
	{
		try
		{
			List<string> allowed = new List<string>();
			foreach (PostprocessRuleEntry rule in rules ?? new List<PostprocessRuleEntry>())
			{
				string tag = (rule?.Tag ?? "").Trim();
				if (!string.IsNullOrWhiteSpace(tag))
				{
					allowed.Add(tag);
				}
			}
			return SiegePostprocessTagNormalizer.Normalize(raw, allowed);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "NormalizeSiegeInterventionPostprocessTagsForExternal failed: " + ex.Message);
			return "";
		}
	}

	internal static bool TryProcessAiActionTags(Hero targetHero, CharacterObject targetCharacter, int targetAgentIndex, ref string text, out bool actionHandled)
	{
		actionHandled = false;
		if (string.IsNullOrWhiteSpace(text) || !AnySiegeTagRegex.IsMatch(text))
		{
			return false;
		}
		try
		{
			if (!IsActiveInCurrentMission())
			{
				text = StripSiegeTags(text);
				return true;
			}
			bool destructiveAllowed = IsDestructiveInterventionAllowed();
			bool targetIsAlliedSoldier = targetAgentIndex >= 0 && AlliedAgentIndexes.Contains(targetAgentIndex);
			bool hasSharedReliefPool = HasSharedCivilianReliefPool();
			SiegeActionRoutingDecision actionRouting = SiegeActionRoutingPolicy.Evaluate(new SiegeActionRoutingFacts(
				text,
				HasDestructiveOutcomeLocked(),
				targetIsAlliedSoldier,
				hasSharedReliefPool));
			if (!destructiveAllowed)
			{
				if (actionRouting.ContainsDestructiveAction)
				{
					InformationManager.DisplayMessage(new InformationMessage("【攻城处置】该定居点与你当前阵营文化相同，军纪禁止掠夺或毁坏，本次只能宽恕或安抚。", Color.FromUint(0xFFFFD27Fu)));
					actionHandled = true;
				}
			}
			bool containsDestructiveTag = actionRouting.ContainsDestructiveAction;
			bool canApplyMercyTrack = actionRouting.CanApplyMercyTrack;
			bool targetIsCivilian = IsCivilianReliefConversationTarget(targetAgentIndex, targetCharacter);
			if (actionRouting.ShouldDowngradeSoldierReliefToMercy)
			{
				text = ReliefTagRegex.Replace(text, "[ACTION:宽恕]");
			}
			bool soldierPositiveCapToRelief = actionRouting.ShouldCapSoldierPositiveToRelief;
			if (!canApplyMercyTrack && !containsDestructiveTag && actionRouting.HasMercyTrackAction)
			{
				actionHandled |= TryBlockMercyTrackAfterDestructive("降级处置");
			}
			if (SoldierAppeasementTagRegex.IsMatch(text))
			{
				actionHandled |= ApplySoldierAppeasementChoice(targetAgentIndex);
			}
			if (GatherCiviliansTagRegex.IsMatch(text))
			{
				actionHandled |= GatherCiviliansForSpeech("ai_tag", targetAgentIndex);
			}
			if (canApplyMercyTrack && MercyTagRegex.IsMatch(text))
			{
				actionHandled |= ApplyMercyChoice("场景对话宽恕", "玩家通过场景对话选择宽恕普通民众。");
			}
			if (canApplyMercyTrack && ReliefTagRegex.IsMatch(text))
			{
				actionHandled |= targetIsAlliedSoldier
					? ApplySoldierMaterialReliefChoice(targetAgentIndex, "士兵分发安抚", "玩家命令己方士兵分发共享物资安抚民众；士兵分发路线最高按安抚结算。")
					: ApplyCivilianVerbalReliefChoice(targetIsCivilian ? "平民对话安抚" : "场景对话安抚", targetIsCivilian ? "玩家直接通过言语安抚战败民众，使其接受宽恕和秩序安排。" : "玩家通过场景对话选择安抚和救济民众。");
			}
			if (canApplyMercyTrack && soldierPositiveCapToRelief)
			{
				actionHandled |= ApplySoldierMaterialReliefChoice(targetAgentIndex, "士兵分发安抚", "玩家命令己方士兵分发共享物资安抚民众；士兵分发路线最高按安抚结算。");
			}
			if (canApplyMercyTrack && !soldierPositiveCapToRelief && InspireTagRegex.IsMatch(text))
			{
				actionHandled |= ApplyInspirationChoice("场景对话安民宣抚", "玩家通过场景对话召集民众并宣示新秩序，以提高忠诚度并争取本地要人支持。");
			}
			if (canApplyMercyTrack && !soldierPositiveCapToRelief && RallyOathTagRegex.IsMatch(text))
			{
				actionHandled |= ApplyRallyOathChoice("场景对话归心盟誓", "玩家通过场景对话组织公开盟誓，以强力争取民众归附和要人支持。");
			}
			if (destructiveAllowed && PlunderTagRegex.IsMatch(text))
			{
				actionHandled |= StartPlunder("场景对话触发搜掠", "玩家在攻城后亲自进城时通过对话下令搜掠。");
			}
			if (destructiveAllowed && MassacreTagRegex.IsMatch(text))
			{
				actionHandled |= StartMassacre("场景对话触发血洗", "NPC回复表明对话谈崩或玩家已明确下令血洗，攻城后处置升级为血洗。");
			}
			if (destructiveAllowed && RepopulationTagRegex.IsMatch(text))
			{
				actionHandled |= RequestCulturalRepopulation(targetAgentIndex, "场景对话屠民迁殖", "玩家通过场景对话要求杀尽原住民并将定居点改为己方文化。");
			}
			text = StripSiegeTags(text);
			return actionHandled;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryProcessAiActionTags failed: " + ex.Message);
			text = StripSiegeTags(text);
			return actionHandled;
		}
	}

	internal static bool TryProcessPlayerInstruction(Hero targetHero, CharacterObject targetCharacter, int targetAgentIndex, string playerInstruction, out bool actionHandled)
	{
		actionHandled = false;
		try
		{
			if (!IsActiveInCurrentMission() || string.IsNullOrWhiteSpace(playerInstruction) || !AnySiegeTagRegex.IsMatch(playerInstruction))
			{
				return false;
			}
			string taggedInstruction = playerInstruction;
			return TryProcessAiActionTags(targetHero, targetCharacter, targetAgentIndex, ref taggedInstruction, out actionHandled);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryProcessPlayerInstruction failed: " + ex.Message);
		}
		return false;
	}

	private static bool ContainsAny(string text, params string[] needles)
	{
		if (string.IsNullOrWhiteSpace(text) || needles == null)
		{
			return false;
		}
		foreach (string needle in needles)
		{
			if (!string.IsNullOrWhiteSpace(needle) && text.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	private static void TryHandlePlayerAttackReleaseForMassacre(Mission mission)
	{
		try
		{
			if (mission == null || _massacreVictoryReached || !IsDestructiveInterventionAllowed())
			{
				UpdateMainAgentAttackReleaseTracking();
				return;
			}
			if (IsPlayerAttackInputSuppressed())
			{
				_playerAttackReleaseSuppressed = false;
				UpdateMainAgentAttackReleaseTracking();
				return;
			}
			if (Input.IsKeyDown(InputKey.LeftMouseButton) && Input.IsKeyDown(InputKey.RightMouseButton))
			{
				_playerAttackReleaseSuppressed = true;
			}
			if (ShouldTriggerPlayerAttackRelease())
			{
				if (!_playerAttackReleaseSuppressed)
				{
					Agent friendlyBlocker = FindFacingAlliedSoldierAttackTarget(mission);
					Agent target = friendlyBlocker == null ? FindFacingMassacreAttackTarget(mission) : null;
					if (target != null && target.IsActive())
					{
						string targetName = target.Name?.ToString() ?? "一名平民";
						if (!_massacreStarted)
						{
							InformationManager.DisplayMessage(new InformationMessage("【攻城处置】你挥武器攻击 " + targetName + "，本次入城处置转为血洗。", Color.FromUint(0xFFFF7777u)));
						}
						bool started = StartMassacre("玩家直接攻击平民触发血洗", "玩家在攻城后亲自进城时直接挥武器攻击" + targetName + "，本次处置按血洗处理。");
						if (started || _massacreStarted)
						{
							PrepareCivilianForMassacreCombat(target, mission);
							TryForcePlayerDamageToCivilian(target, MBRandom.RandomInt(36, 64), "player_attack_release_massacre_start");
						}
					}
				}
				_playerAttackReleaseSuppressed = false;
			}
			UpdateMainAgentAttackReleaseTracking();
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryHandlePlayerAttackReleaseForMassacre failed: " + ex.Message);
			UpdateMainAgentAttackReleaseTracking();
		}
	}

	private static bool ShouldTriggerPlayerAttackRelease()
	{
		Agent.ActionStage? stage = GetMainAgentAttackStage();
		if (stage == Agent.ActionStage.AttackReady || stage == Agent.ActionStage.AttackQuickReady)
		{
			return false;
		}
		bool released = stage == Agent.ActionStage.AttackRelease && _lastMainAgentAttackStage != Agent.ActionStage.AttackRelease;
		bool hasRealWeapon = IsAgentUsingAnyRealWeapon(Agent.Main);
		if (released && hasRealWeapon)
		{
			return true;
		}
		return false;
	}

	private static void UpdateMainAgentAttackReleaseTracking()
	{
		_lastMainAgentAttackStage = GetMainAgentAttackStage();
	}

	private static Agent.ActionStage? GetMainAgentAttackStage()
	{
		try
		{
			if (Agent.Main == null || !Agent.Main.IsActive())
			{
				return null;
			}
			return Agent.Main.GetCurrentActionStage(1);
		}
		catch
		{
			return null;
		}
	}

	private static bool IsPlayerAttackInputSuppressed()
	{
		try
		{
			return (Campaign.Current?.ConversationManager?.IsConversationInProgress ?? false) || ShoutBehavior.IsSceneShoutInputActiveForExternal() || (Agent.Main != null && Agent.Main.IsActive() && Agent.Main.IsSitting());
		}
		catch
		{
			return false;
		}
	}

	private static Agent FindFacingMassacreAttackTarget(Mission mission)
	{
		try
		{
			Agent main = Agent.Main ?? mission?.MainAgent;
			if (mission == null || main == null || !main.IsActive())
			{
				return null;
			}
			Vec3 position = main.Position;
			Vec3 look = main.LookDirection;
			if (look.LengthSquared < 0.01f)
			{
				look = Vec3.Forward;
			}
			look.Normalize();
			Agent result = null;
			float best = -1f;
			foreach (Agent agent in mission.Agents)
			{
				if (!IsMassacreTargetAgent(agent, includeHeroes: true) || CountedMassacreVictims.Contains(agent.Index))
				{
					continue;
				}
				Vec3 direction = agent.Position - position;
				float distance = direction.Length;
				if (distance > 4.7f)
				{
					continue;
				}
				if (direction.LengthSquared < 0.01f)
				{
					direction = look;
				}
				direction.Normalize();
				float dot = Vec3.DotProduct(look, direction);
				if (dot < 0.52f)
				{
					continue;
				}
				float score = dot / Math.Max(0.35f, distance);
				if (score > best)
				{
					best = score;
					result = agent;
				}
			}
			return result;
		}
		catch
		{
			return null;
		}
	}

	private static Agent FindFacingAlliedSoldierAttackTarget(Mission mission)
	{
		try
		{
			Agent main = Agent.Main ?? mission?.MainAgent;
			if (mission == null || main == null || !main.IsActive())
			{
				return null;
			}
			Vec3 position = main.Position;
			Vec3 look = main.LookDirection;
			if (look.LengthSquared < 0.01f)
			{
				look = Vec3.Forward;
			}
			look.Normalize();
			Agent result = null;
			float best = -1f;
			foreach (Agent agent in mission.Agents)
			{
				if (!IsInterventionAlliedSoldierForExternal(agent, requireActive: true))
				{
					continue;
				}
				Vec3 direction = agent.Position - position;
				float distance = direction.Length;
				if (distance > 3.2f)
				{
					continue;
				}
				if (direction.LengthSquared < 0.01f)
				{
					direction = look;
				}
				direction.Normalize();
				float dot = Vec3.DotProduct(look, direction);
				if (dot < 0.42f)
				{
					continue;
				}
				float score = dot / Math.Max(0.35f, distance);
				if (score > best)
				{
					best = score;
					result = agent;
				}
			}
			return result;
		}
		catch
		{
			return null;
		}
	}

	internal static bool ShouldSuppressEnemyGuardsForSceneConflict()
	{
		return IsActiveInCurrentMission();
	}

	internal static bool IsOccupationSceneActiveForExternal()
	{
		return IsActiveInCurrentMission();
	}

	internal static bool IsInterventionAlliedSoldierForExternal(Agent agent, bool requireActive = false)
	{
		try
		{
			if (!IsActiveInCurrentMission() || agent == null || !agent.IsHuman || agent == Agent.Main)
			{
				return false;
			}
			if (requireActive && (!agent.IsActive() || agent.State == AgentState.Killed || agent.State == AgentState.Unconscious))
			{
				return false;
			}
			return AlliedAgentIndexes.Contains(agent.Index);
		}
		catch
		{
			return false;
		}
	}

	internal static bool ShouldBlockInterventionMissionExit(out string message)
	{
		message = "";
		return false;
	}

	internal static bool ShouldForceAllowInterventionMissionExitForExternal()
	{
		return false;
	}

	private static void EnsureInterventionMissionCombatModeForPlayerDamage(Mission mission)
	{
		try
		{
			if (mission == null || mission.IsMissionEnding || mission.Mode == MissionMode.Conversation || mission.Mode == MissionMode.Barter)
			{
				return;
			}
			if (_massacreStarted && !_massacreVictoryReached)
			{
				if (mission.Mode != MissionMode.Battle)
				{
					mission.SetMissionMode(MissionMode.Battle, atStart: false);
				}
				return;
			}
			// 非血洗处置阶段保持城镇/村庄原版 StartUp 模式，避免 TAB 变成战斗计分板/撤退界面。
			if (mission.Mode == MissionMode.Battle)
			{
				mission.SetMissionMode(MissionMode.StartUp, atStart: false);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnsureInterventionMissionCombatModeForPlayerDamage failed: " + ex.Message);
		}
	}

	private static void EnsureInterventionPlayerCommandTeam(Mission mission)
	{
		try
		{
			Agent main = Agent.Main ?? mission?.MainAgent;
			if (mission == null || mission.IsMissionEnding || main == null || !main.IsActive())
			{
				return;
			}
			Team playerTeam = mission.PlayerTeam;
			if (_interventionPlayerCommandTeam != null)
			{
				playerTeam = _interventionPlayerCommandTeam;
				if (mission.PlayerTeam != playerTeam)
				{
					mission.PlayerTeam = playerTeam;
				}
			}
			if (playerTeam == null || !playerTeam.IsPlayerGeneral)
			{
				uint color = Hero.MainHero?.MapFaction?.Color ?? 0xFF2020FFu;
				uint color2 = Hero.MainHero?.MapFaction?.Color2 ?? 0xFF101080u;
				Banner banner = Hero.MainHero?.Clan?.Banner;
				try
				{
					playerTeam = mission.Teams.Add(BattleSideEnum.Attacker, color, color2, banner, isPlayerGeneral: true, isPlayerSergeant: false);
					_interventionPlayerCommandTeam = playerTeam;
				}
				catch (Exception ex)
				{
					Logger.Log("SiegeAiIntervention", "Creating intervention player command team failed: " + ex.Message);
					playerTeam = mission.PlayerTeam ?? main.Team;
				}
				if (playerTeam != null)
				{
					mission.PlayerTeam = playerTeam;
				}
			}
			else
			{
				_interventionPlayerCommandTeam = playerTeam;
			}
			if (playerTeam == null)
			{
				return;
			}
			if (main.Team != playerTeam)
			{
				main.SetTeam(playerTeam, true);
			}
			try
			{
				Agent mount = main.MountAgent;
				if (mount != null && mount.IsActive() && mount.Team != playerTeam)
				{
					mount.SetTeam(playerTeam, true);
				}
			}
			catch
			{
			}
			try
			{
				foreach (Team team in mission.Teams)
				{
					if (team == null || team == playerTeam)
					{
						continue;
					}
					if (!_massacreStarted)
					{
						team.SetIsEnemyOf(playerTeam, isEnemyOf: false);
						playerTeam.SetIsEnemyOf(team, isEnemyOf: false);
					}
				}
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnsureInterventionPlayerCommandTeam failed: " + ex.Message);
		}
	}

	private static Team EnsureInterventionCivilianEnemyTeam(Mission mission)
	{
		try
		{
			if (mission == null)
			{
				return null;
			}
			Agent main = Agent.Main ?? mission.MainAgent;
			Team playerTeam = mission.PlayerTeam ?? _interventionPlayerCommandTeam ?? main?.Team;
			if (_interventionCivilianEnemyTeam != null && _interventionCivilianEnemyTeam != playerTeam)
			{
				if (playerTeam != null)
				{
					_interventionCivilianEnemyTeam.SetIsEnemyOf(playerTeam, isEnemyOf: true);
					playerTeam.SetIsEnemyOf(_interventionCivilianEnemyTeam, isEnemyOf: true);
				}
				return _interventionCivilianEnemyTeam;
			}
			Team enemyTeam = mission.PlayerEnemyTeam;
			if (enemyTeam == null || enemyTeam == playerTeam)
			{
				try
				{
					enemyTeam = mission.Teams.Add(BattleSideEnum.Defender, 0xFF7A2020u, 0xFF2A0808u, null, isPlayerGeneral: false, isPlayerSergeant: false);
				}
				catch (Exception ex)
				{
					Logger.Log("SiegeAiIntervention", "Creating civilian massacre enemy team failed: " + ex.Message);
					enemyTeam = mission.PlayerEnemyTeam;
				}
			}
			if (enemyTeam != null && playerTeam != null && enemyTeam != playerTeam)
			{
				enemyTeam.SetIsEnemyOf(playerTeam, isEnemyOf: true);
				playerTeam.SetIsEnemyOf(enemyTeam, isEnemyOf: true);
			}
			_interventionCivilianEnemyTeam = enemyTeam;
			return enemyTeam;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnsureInterventionCivilianEnemyTeam failed: " + ex.Message);
			return null;
		}
	}

	internal static bool ShouldRedirectResolvedAftermathMenuForExternal(string menuId)
	{
		try
		{
			if (Mission.Current != null || !_afAftermathResolved || _nativeDevastateAftermathFlowActive || string.IsNullOrWhiteSpace(menuId) || !DoesCompletedAftermathMatchCurrentSettlement())
			{
				return false;
			}
			return IsNativeSiegeAftermathMenuId(menuId);
		}
		catch
		{
			return false;
		}
	}

	internal static bool TryHandleNativeAftermathMenuInitForExternal(string source)
	{
		try
		{
			if (_directPlunderAftermathScriptPending)
			{
				TryRunDirectPlunderAftermathScript("native_menu_init:" + (source ?? "N/A"));
				return true;
			}
			if (Mission.Current != null || !_afAftermathResolved || !DoesCompletedAftermathMatchCurrentSettlement())
			{
				return false;
			}
			if (string.IsNullOrWhiteSpace(_completedSummaryText))
			{
				PrepareCompletedInterventionSummary(_completedAftermath);
			}
			if (_nativeDevastateAftermathFlowActive && _completedAftermath == SiegeAftermathAction.SiegeAftermath.Devastate)
			{
				TrySetNativePlayerEncounterAftermathForSummary(SiegeAftermathAction.SiegeAftermath.Devastate);
				if ((source ?? "").IndexOf("contextual_summary", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					Logger.Log("SiegeAiIntervention", "Allowing native Devastate contextual summary init for AF massacre. Source=" + (source ?? "N/A"));
					return false;
				}
				GameMenu.SwitchToMenu("siege_aftermath_contextual_summary");
				Logger.Log("SiegeAiIntervention", "Auto-routed native siege aftermath menu to Devastate summary for AF massacre. Source=" + (source ?? "N/A"));
				return true;
			}
			QueueEncounterFinishAfterIntervention(_completedAftermath, "native_menu_init:" + (source ?? "N/A"), 0, forceDelay: true);
			TryFinishPlayerEncounterAfterInterventionNow(_completedAftermath, "native_menu_init:" + (source ?? "N/A"));
			Logger.Log("SiegeAiIntervention", "Suppressed native siege aftermath menu init and requested encounter finish after AF resolved aftermath. Source=" + (source ?? "N/A"));
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryHandleNativeAftermathMenuInitForExternal failed: " + ex.Message);
			return false;
		}
	}

	internal static bool TryHandleNativeAftermathSummaryContinueForExternal(string source)
	{
		try
		{
			if (!_nativeDevastateAftermathFlowActive || !_afAftermathResolved || _completedAftermath != SiegeAftermathAction.SiegeAftermath.Devastate || !DoesCompletedAftermathMatchCurrentSettlement())
			{
				return false;
			}
			string menuId = Campaign.Current?.CurrentMenuContext?.GameMenu?.StringId;
			if (!string.Equals(menuId, "siege_aftermath_contextual_summary", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (_nativeDevastateSummaryContinueHandled)
			{
				return true;
			}
			_nativeDevastateSummaryContinueHandled = true;
			try
			{
				GameMenu.ExitToLast();
			}
			catch
			{
			}
			if (_pendingLootRoster != null && _pendingLootRoster.Count > 0)
			{
				_pendingLootScreen = true;
				_pendingLootScreenShown = true;
				_pendingSummarySwitch = true;
				QueueEncounterFinishAfterIntervention(SiegeAftermathAction.SiegeAftermath.Devastate, "native_devastate_summary_continue_loot", 0, forceDelay: true);
				InventoryScreenHelper.OpenScreenAsLoot(new Dictionary<PartyBase, ItemRoster>
				{
					{
						PartyBase.MainParty,
						_pendingLootRoster
					}
				});
				Logger.Log("SiegeAiIntervention", "Opened native loot screen after AF massacre Devastate summary. Source=" + (source ?? "N/A") + ", LootItems=" + _pendingLootRoster.Count);
				return true;
			}
			QueueEncounterFinishAfterIntervention(SiegeAftermathAction.SiegeAftermath.Devastate, "native_devastate_summary_continue_no_loot", 0, forceDelay: true);
			TryFinishPlayerEncounterAfterInterventionNow(SiegeAftermathAction.SiegeAftermath.Devastate, "native_devastate_summary_continue_no_loot");
			Logger.Log("SiegeAiIntervention", "Finished AF massacre Devastate summary without loot. Source=" + (source ?? "N/A"));
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryHandleNativeAftermathSummaryContinueForExternal failed: " + ex.Message);
			return false;
		}
	}

	internal static bool TryHandleDirectMassacreAftermathMenuForExternal(string menuId, string source)
	{
		try
		{
			if (!_directMassacreAftermathScriptPending || string.IsNullOrWhiteSpace(menuId))
			{
				return false;
			}
			if (!IsNativeSiegeAftermathMenuId(menuId))
			{
				return false;
			}
			Logger.Log("SiegeAiIntervention", "Intercepted native siege aftermath menu for direct AF massacre loot script. Menu=" + menuId + ", Source=" + (source ?? "N/A"));
			if (Mission.Current != null)
			{
				LogDirectMassacreLootDeferOnce("native_menu_mission_current:" + menuId, "Suppressed native siege aftermath menu while Mission.Current is still active; direct AF massacre loot will be pumped after MapState. Menu=" + menuId + ", Source=" + (source ?? "N/A"));
				return true;
			}
			if (!TryOpenDirectMassacreLootScreenNow(source ?? "native_menu_intercept") && IsSafeToOpenDirectMassacreLootScreen(source ?? "native_menu_intercept_no_loot"))
			{
				QueueEncounterFinishAfterIntervention(SiegeAftermathAction.SiegeAftermath.Devastate, "direct_massacre_native_menu_intercept_no_loot", 0, forceDelay: true);
				TryFinishPlayerEncounterAfterInterventionNow(SiegeAftermathAction.SiegeAftermath.Devastate, "direct_massacre_native_menu_intercept_no_loot");
			}
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryHandleDirectMassacreAftermathMenuForExternal failed: " + ex.Message);
			return false;
		}
	}

	internal static bool TryHandleDirectPlunderAftermathMenuForExternal(string menuId, string source)
	{
		try
		{
			if (!_directPlunderAftermathScriptPending || string.IsNullOrWhiteSpace(menuId))
			{
				return false;
			}
			if (!IsNativeSiegeAftermathMenuId(menuId))
			{
				return false;
			}
			Logger.Log("SiegeAiIntervention", "Intercepted native siege aftermath menu for direct AF plunder loot script. Menu=" + menuId + ", Source=" + (source ?? "N/A"));
			if (Mission.Current != null)
			{
				LogDirectPlunderLootDeferOnce("native_menu_mission_current:" + menuId, "Suppressed native siege aftermath menu while Mission.Current is still active; direct AF plunder loot will be pumped after MapState. Menu=" + menuId + ", Source=" + (source ?? "N/A"));
				return true;
			}
			if (!TryOpenDirectPlunderLootScreenNow(source ?? "native_menu_intercept") && IsSafeToOpenDirectPlunderLootScreen(source ?? "native_menu_intercept_no_loot"))
			{
				QueueEncounterFinishAfterIntervention(SiegeAftermathAction.SiegeAftermath.Pillage, "direct_plunder_native_menu_intercept_no_loot", 0, forceDelay: true);
				TryFinishPlayerEncounterAfterInterventionNow(SiegeAftermathAction.SiegeAftermath.Pillage, "direct_plunder_native_menu_intercept_no_loot");
			}
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryHandleDirectPlunderAftermathMenuForExternal failed: " + ex.Message);
			return false;
		}
	}

	internal static bool TryPumpDirectMassacreAftermathScriptForExternal(string source)
	{
		try
		{
			if (!_directMassacreAftermathScriptPending)
			{
				return false;
			}
			return TryRunDirectMassacreAftermathScript(source ?? "external_direct_massacre_script");
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryPumpDirectMassacreAftermathScriptForExternal failed. Source=" + (source ?? "N/A") + ", Error=" + ex.Message);
			return true;
		}
	}

	internal static bool TryHandlePlayerAttackForAutoMassacre(Agent affectedAgent, string source, float damagedHp = 0f, int forcedDamage = 0)
	{
		if (!IsActiveInCurrentMission() || affectedAgent == null || !affectedAgent.IsHuman || affectedAgent == Agent.Main)
		{
			return false;
		}
		if (AlliedAgentIndexes.Contains(affectedAgent.Index))
		{
			return TryHandleFriendlyHitOnAlliedSoldier(affectedAgent, source, damagedHp);
		}
		CharacterObject character = affectedAgent.Character as CharacterObject;
		if (!IsMassacreTargetAgent(affectedAgent, includeHeroes: true))
		{
			return false;
		}
		if (!IsDestructiveInterventionAllowed())
		{
			return false;
		}
		string targetName = affectedAgent.Name?.ToString() ?? "一名NPC";
		if (!_massacreStarted)
		{
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】你击中了 " + targetName + "，本次入城处置进入血洗。", Color.FromUint(0xFFFF7777u)));
		}
		bool wasMassacreStarted = _massacreStarted;
		bool startedNow = StartMassacre("玩家主动攻击NPC触发血洗", "玩家在攻城后亲自进城期间主动攻击了" + targetName + "，本次处置按血洗处理。");
		if (!startedNow && !wasMassacreStarted)
		{
			return false;
		}
		PrepareCivilianForMassacreCombat(affectedAgent, Mission.Current ?? affectedAgent.Mission);
		if (forcedDamage > 0)
		{
			TryForcePlayerDamageToCivilian(affectedAgent, forcedDamage, source);
		}
		return true;
	}

	internal static bool TryHandleFriendlyHitOnAlliedSoldier(Agent affectedAgent, string source, float damagedHp = 0f)
	{
		if (!IsActiveInCurrentMission() || affectedAgent == null || !affectedAgent.IsHuman || affectedAgent == Agent.Main || !AlliedAgentIndexes.Contains(affectedAgent.Index))
		{
			return false;
		}
		RestoreAlliedSoldierFriendlyState(affectedAgent, damagedHp, source, forceFollow: !_massacreStarted);
		return true;
	}

	private static void RestoreAlliedSoldierFriendlyState(Agent soldier, float damagedHp, string source, bool forceFollow, bool clearTarget = true)
	{
		try
		{
			if (soldier == null || !AlliedAgentIndexes.Contains(soldier.Index))
			{
				return;
			}
			Mission mission = Mission.Current ?? soldier.Mission;
			Agent main = Agent.Main ?? mission?.MainAgent;
			if (mission == null || main == null)
			{
				return;
			}
			Team playerTeam = mission.PlayerTeam ?? main.Team;
			if (playerTeam != null && soldier.Team != playerTeam)
			{
				soldier.SetTeam(playerTeam, true);
			}
			if (playerTeam != null && soldier.Team != null && soldier.Team != playerTeam)
			{
				soldier.Team.SetIsEnemyOf(playerTeam, isEnemyOf: false);
				playerTeam.SetIsEnemyOf(soldier.Team, isEnemyOf: false);
			}
			if (damagedHp > 0f && soldier.Health > 0f && soldier.HealthLimit > 0f)
			{
				soldier.Health = MathF.Min(soldier.HealthLimit, soldier.Health + damagedHp + 1f);
			}
			soldier.SetMortalityState(Agent.MortalityState.Invulnerable);
			if (clearTarget)
			{
				soldier.InvalidateTargetAgent();
			}
			if (forceFollow)
			{
				soldier.SetWatchState(_plunderStarted ? Agent.WatchState.Alarmed : Agent.WatchState.Patrolling);
				AssignAgentToPlayerFormation(soldier, FormationClass.Infantry);
				TrySetPlayerFormationFollowOrder(FormationClass.Infantry, source);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "RestoreAlliedSoldierFriendlyState failed (" + source + "): " + ex.Message);
		}
	}

	private static bool TryApplyCompanionStyleFollow(Agent soldier, Agent main, string source)
	{
		return TryApplyAgentFollowTarget(soldier, main, source, lookAtTarget: false);
	}

	private static bool TryApplyAgentFollowTarget(Agent soldier, Agent target, string source, bool lookAtTarget)
	{
		try
		{
			if (soldier == null || target == null || !soldier.IsActive() || !target.IsActive())
			{
				return false;
			}
			CampaignAgentComponent component = soldier.GetComponent<CampaignAgentComponent>();
			if (component == null)
			{
				return false;
			}
			AgentNavigator navigator = component.AgentNavigator ?? component.CreateAgentNavigator();
			if (navigator == null)
			{
				return false;
			}
			AlarmedBehaviorGroup alarmedGroup = navigator.GetBehaviorGroup<AlarmedBehaviorGroup>();
			if (alarmedGroup != null)
			{
				alarmedGroup.DisableScriptedBehavior();
				alarmedGroup.IsActive = false;
			}
			DailyBehaviorGroup dailyGroup = navigator.GetBehaviorGroup<DailyBehaviorGroup>() ?? navigator.AddBehaviorGroup<DailyBehaviorGroup>();
			if (dailyGroup == null)
			{
				return false;
			}
			FollowAgentBehavior followBehavior = dailyGroup.GetBehavior<FollowAgentBehavior>() ?? dailyGroup.AddBehavior<FollowAgentBehavior>();
			if (followBehavior == null)
			{
				return false;
			}
			ScriptBehavior scriptBehavior = dailyGroup.GetBehavior<ScriptBehavior>();
			if (scriptBehavior != null)
			{
				scriptBehavior.IsActive = false;
			}
			WalkingBehavior walkingBehavior = dailyGroup.GetBehavior<WalkingBehavior>();
			if (walkingBehavior != null)
			{
				walkingBehavior.IsActive = false;
			}
			dailyGroup.SetScriptedBehavior<FollowAgentBehavior>();
			dailyGroup.IsActive = true;
			followBehavior.IsActive = true;
			followBehavior.SetTargetAgent(target);
			try
			{
				FollowAgentBehaviorIdleDistanceField?.SetValue(followBehavior, 0f);
			}
			catch
			{
			}
			if (lookAtTarget)
			{
				soldier.SetLookAgent(target);
			}
			else
			{
				ClearAgentLookTarget(soldier);
			}
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryApplyAgentFollowTarget failed (" + source + "): " + ex.Message);
			return false;
		}
	}

	private static void DisableCompanionStyleFollow(Agent soldier)
	{
		try
		{
			CampaignAgentComponent component = soldier?.GetComponent<CampaignAgentComponent>();
			AgentNavigator navigator = component?.AgentNavigator;
			DailyBehaviorGroup dailyGroup = navigator?.GetBehaviorGroup<DailyBehaviorGroup>();
			FollowAgentBehavior followBehavior = dailyGroup?.GetBehavior<FollowAgentBehavior>();
			if (followBehavior != null)
			{
				followBehavior.SetTargetAgent(null);
				followBehavior.IsActive = false;
			}
			dailyGroup?.DisableScriptedBehavior();
		}
		catch
		{
		}
	}

	private static void ClearAgentLookTarget(Agent agent)
	{
		try
		{
			if (agent != null && agent.IsActive())
			{
				agent.SetLookAgent(null);
			}
		}
		catch
		{
		}
	}

	private static void MoveAlliedSoldierNearMainFallback(Agent soldier, Agent main)
	{
		try
		{
			Mission mission = Mission.Current ?? soldier?.Mission;
			if (mission == null || soldier == null || main == null || !soldier.IsActive())
			{
				return;
			}
			Vec3 forward = main.LookDirection;
			if (forward.LengthSquared < 0.01f)
			{
				forward = Vec3.Forward;
			}
			forward.Normalize();
			Vec3 right = Vec3.CrossProduct(forward, Vec3.Up);
			if (right.LengthSquared < 0.01f)
			{
				right = Vec3.Side;
			}
			right.Normalize();
			int slot = Math.Abs(soldier.Index % AutoSummonCount);
			float back = 2.5f + (slot / 4) * 0.75f;
			float side = ((slot % 2 == 0) ? 1f : -1f) * (0.9f + (slot % 4) * 0.55f);
			Vec3 position = main.Position - forward * back + right * side;
			try
			{
				if (mission.Scene != null)
				{
					position.z = mission.Scene.GetGroundHeightAtPosition(position);
				}
			}
			catch
			{
			}
			float distanceSq = soldier.Position.DistanceSquared(main.Position);
			if (distanceSq > 4.5f * 4.5f)
			{
				soldier.SetTargetPosition(position.AsVec2);
			}
		}
		catch
		{
		}
	}

	private static SiegeInterventionOutcome GetStandaloneOutcome()
	{
		if (_massacreStarted || _culturalRepopulationRequested)
		{
			return SiegeInterventionOutcome.Massacre;
		}
		if (_plunderStarted)
		{
			return SiegeInterventionOutcome.Plunder;
		}
		if (_activeMode == InterventionMode.MercyRelief)
		{
			return SiegeInterventionOutcome.MercyRelief;
		}
		if (_activeMode == InterventionMode.WaitingDecision)
		{
			return SiegeInterventionOutcome.WaitingDecision;
		}
		return SiegeInterventionOutcome.None;
	}

	private static bool HasPendingDevastateAftermath()
	{
		return _hasPendingAftermath && SiegeAftermathSelectionPolicy.IsDevastateOrWorse(ToStandaloneAftermathKind(_pendingAftermath));
	}

	private static bool HasDestructiveOutcomeLocked()
	{
		return SiegeInterventionActionRules.HasDestructiveOutcomeLocked(GetStandaloneOutcome(), _culturalRepopulationRequested, HasPendingDevastateAftermath());
	}

	private static bool TryBlockMercyTrackAfterDestructive(string actionName)
	{
		if (!HasDestructiveOutcomeLocked())
		{
			return false;
		}
		InformationManager.DisplayMessage(new InformationMessage("【攻城处置】" + (actionName ?? "安抚") + "不能覆盖已经升级的处置；血洗和屠民迁殖不可逆，不能回退。", Color.FromUint(0xFFFFD27Fu)));
		return true;
	}

	private static void StopReversiblePlunderForMercyTrack(string reason)
	{
		try
		{
			if (!_plunderStarted || _massacreStarted || _culturalRepopulationRequested)
			{
				return;
			}
			_plunderStarted = false;
			ActivePlunderInteractions.Clear();
			LastMassacreSoldierFollowOrderTimes.Clear();
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】搜掠已被后续宽恕/宣抚类指令覆盖，士兵停止索财；离场将按当前正向处置结算。", Color.FromUint(0xFFB6F7A8u)));
			Logger.Log("SiegeAiIntervention", "Reversible plunder stopped by mercy track. Reason=" + (reason ?? "N/A"));
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "StopReversiblePlunderForMercyTrack failed: " + ex.Message);
		}
	}

	internal static bool ShouldCapturePlayerGiveForSharedCivilianReliefForExternal()
	{
		try
		{
			return IsActiveInCurrentMission() && !_massacreStarted && !_culturalRepopulationRequested;
		}
		catch
		{
			return false;
		}
	}

	internal static bool RecordSharedCivilianReliefTransferForExternal(int targetAgentIndex, int goldAmount, string itemId, int itemAmount, ItemObject item, int unitValue, string source)
	{
		try
		{
			if (!IsActiveInCurrentMission() || _massacreStarted || _culturalRepopulationRequested)
			{
				return false;
			}
			bool recorded = false;
			if (goldAmount > 0)
			{
				_sharedCivilianReliefGold += goldAmount;
				recorded = true;
			}
			if (!string.IsNullOrWhiteSpace(itemId) && itemAmount > 0)
			{
				string key = itemId.Trim();
				if (SharedCivilianReliefItems.TryGetValue(key, out int oldAmount))
				{
					SharedCivilianReliefItems[key] = oldAmount + itemAmount;
				}
				else
				{
					SharedCivilianReliefItems[key] = itemAmount;
				}
				if (item != null)
				{
					SharedCivilianReliefItemObjects[key] = item;
				}
				_sharedCivilianReliefItemTotal += itemAmount;
				if (item != null && item.IsFood)
				{
					_sharedCivilianReliefFoodUnits += itemAmount;
				}
				else
				{
					int value = unitValue > 0 ? unitValue : Math.Max(1, item?.Value ?? 1);
					_sharedCivilianReliefItemValue += (long)value * itemAmount;
				}
				recorded = true;
			}
			if (recorded)
			{
				_sharedCivilianReliefReturned = false;
				string itemText = itemAmount > 0 ? (itemAmount + " 个 " + (item?.Name?.ToString() ?? itemId ?? "物资")) : "";
				string goldText = goldAmount > 0 ? (goldAmount + " 第纳尔") : "";
				string joined = string.Join("、", new[] { goldText, itemText }.Where(x => !string.IsNullOrWhiteSpace(x)));
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】已将给予的" + joined + "计入全城平民共享安抚物资。", Color.FromUint(0xFFB6F7A8u)));
				Logger.Log("SiegeAiIntervention", "Recorded shared civilian relief transfer. Source=" + (source ?? "N/A") + ", TargetAgent=" + targetAgentIndex + ", Gold=" + goldAmount + ", Item=" + (itemId ?? "") + ", Amount=" + itemAmount + ", Pool=" + DescribeSharedCivilianReliefPoolForContext());
			}
			return recorded;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "RecordSharedCivilianReliefTransferForExternal failed (" + (source ?? "N/A") + "): " + ex.Message);
			return false;
		}
	}

	private static string DescribeSharedCivilianReliefPoolForContext()
	{
		try
		{
			return SiegeSharedReliefPoolFormatter.DescribeForContext(BuildSharedCivilianReliefPoolFacts());
		}
		catch
		{
			return "共享物资统计不可用";
		}
	}

	private static bool HasSharedCivilianReliefPool()
	{
		return SiegeSharedReliefPoolFormatter.HasAnyMaterial(BuildSharedCivilianReliefPoolFacts());
	}

	private static SiegeSharedReliefPoolFacts BuildSharedCivilianReliefPoolFacts()
	{
		return new SiegeSharedReliefPoolFacts(
			_sharedCivilianReliefGold,
			_sharedCivilianReliefFoodUnits,
			_sharedCivilianReliefItemTotal,
			_sharedCivilianReliefItemValue);
	}

	private static bool IsCivilianReliefConversationTarget(int targetAgentIndex, CharacterObject targetCharacter)
	{
		try
		{
			if (targetAgentIndex >= 0)
			{
				Agent agent = TryGetAgent(targetAgentIndex);
				if (agent != null)
				{
					return !AlliedAgentIndexes.Contains(agent.Index) && IsEligibleCivilianAgent(agent, includeHeroes: true, requireActive: false);
				}
			}
			return IsCivilianForIntervention(targetCharacter);
		}
		catch
		{
			return false;
		}
	}

	private static ItemObject ResolveSharedCivilianReliefItem(string itemId)
	{
		try
		{
			string key = (itemId ?? "").Trim();
			if (string.IsNullOrWhiteSpace(key))
			{
				return null;
			}
			if (SharedCivilianReliefItemObjects.TryGetValue(key, out ItemObject cached) && cached != null)
			{
				return cached;
			}
			return Game.Current?.ObjectManager?.GetObject<ItemObject>(key);
		}
		catch
		{
			return null;
		}
	}

	private static bool ReturnSharedCivilianReliefPoolToPlayerForNegativeOutcome(string reason)
	{
		try
		{
			if (!HasSharedCivilianReliefPool() || _sharedCivilianReliefReturned)
			{
				return false;
			}
			int returnedGold = Math.Max(0, _sharedCivilianReliefGold);
			int returnedItems = 0;
			List<string> returnedParts = new List<string>();
			if (returnedGold > 0)
			{
				AwardGoldToPlayer(returnedGold, "shared_relief_refund_" + (reason ?? "negative"));
				returnedParts.Add(returnedGold + " 第纳尔");
			}
			MobileParty mainParty = MobileParty.MainParty ?? Hero.MainHero?.PartyBelongedTo;
			ItemRoster itemRoster = mainParty?.ItemRoster;
			foreach (KeyValuePair<string, int> pair in SharedCivilianReliefItems.ToList())
			{
				int amount = Math.Max(0, pair.Value);
				if (amount <= 0 || itemRoster == null)
				{
					continue;
				}
				ItemObject item = ResolveSharedCivilianReliefItem(pair.Key);
				if (item == null)
				{
					Logger.Log("SiegeAiIntervention", "Unable to resolve shared relief item for refund. ItemId=" + (pair.Key ?? ""));
					continue;
				}
				itemRoster.AddToCounts(item, amount);
				returnedItems += amount;
				returnedParts.Add(amount + " 个 " + (item.Name?.ToString() ?? pair.Key));
			}
			if (returnedGold <= 0 && returnedItems <= 0)
			{
				return false;
			}
			_sharedCivilianReliefReturned = true;
			_sharedCivilianReliefGold = 0;
			_sharedCivilianReliefFoodUnits = 0;
			_sharedCivilianReliefItemTotal = 0;
			_sharedCivilianReliefItemValue = 0L;
			_appliedSharedCivilianReliefGold = 0;
			_appliedSharedCivilianReliefFoodUnits = 0;
			_appliedSharedCivilianReliefItemValue = 0L;
			SharedCivilianReliefItems.Clear();
			SharedCivilianReliefItemObjects.Clear();
			string summary = string.Join("、", returnedParts.Where(x => !string.IsNullOrWhiteSpace(x)));
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】已触发搜掠/血洗等负面处置，先前交给平民共享的物资已退还给你：" + summary + "。", Color.FromUint(0xFFFFD27Fu)));
			RecordInterventionMemory("返还", "玩家先前交付的平民共享安抚物资因负面处置被退还；返还内容：" + summary + "。");
			Logger.Log("SiegeAiIntervention", "Returned shared civilian relief pool to player due to negative outcome. Reason=" + (reason ?? "N/A") + ", Summary=" + summary);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ReturnSharedCivilianReliefPoolToPlayerForNegativeOutcome failed (" + (reason ?? "N/A") + "): " + ex.Message);
			return false;
		}
	}

	private static bool ApplySharedCivilianReliefPoolEffects(Settlement settlement, string reason)
	{
		try
		{
			int newGold = Math.Max(0, _sharedCivilianReliefGold - _appliedSharedCivilianReliefGold);
			int newFood = Math.Max(0, _sharedCivilianReliefFoodUnits - _appliedSharedCivilianReliefFoodUnits);
			long newMaterialValue = Math.Max(0L, _sharedCivilianReliefItemValue - _appliedSharedCivilianReliefItemValue);
			if (newGold <= 0 && newFood <= 0 && newMaterialValue <= 0)
			{
				return false;
			}
			_appliedSharedCivilianReliefGold = _sharedCivilianReliefGold;
			_appliedSharedCivilianReliefFoodUnits = _sharedCivilianReliefFoodUnits;
			_appliedSharedCivilianReliefItemValue = _sharedCivilianReliefItemValue;
			int publicTrustDelta = 0;
			float loyaltyDelta = 0f;
			float securityDelta = 0f;
			if (newGold > 0)
			{
				publicTrustDelta += Math.Max(1, newGold / 250);
				loyaltyDelta += newGold / 1000f;
				securityDelta += newGold / 1500f;
			}
			if (newFood > 0)
			{
				publicTrustDelta += Math.Max(1, newFood / 5);
				loyaltyDelta += newFood / 20f;
				securityDelta += newFood / 30f;
				try
				{
					if (settlement?.Town != null)
					{
						settlement.Town.FoodStocks = Math.Min(settlement.Town.FoodStocks + newFood, settlement.Town.FoodStocksUpperLimit());
					}
				}
				catch
				{
				}
			}
			if (newMaterialValue > 0)
			{
				publicTrustDelta += Math.Max(1, (int)Math.Min(50L, newMaterialValue / 1000L));
				loyaltyDelta += Math.Min(12f, newMaterialValue / 5000f);
				securityDelta += Math.Min(8f, newMaterialValue / 6000f);
			}
			if (publicTrustDelta != 0 || Math.Abs(loyaltyDelta) > 0.001f || Math.Abs(securityDelta) > 0.001f)
			{
				AdjustSettlementAfterRelief(settlement, publicTrustDelta, loyaltyDelta, securityDelta);
			}
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】已将AF给予的共享物资纳入本次安抚结算：" + DescribeSharedCivilianReliefPoolForContext() + "。", Color.FromUint(0xFFB6F7A8u)));
			Logger.Log("SiegeAiIntervention", "Applied shared civilian relief pool effects. Reason=" + (reason ?? "N/A") + ", NewGold=" + newGold + ", NewFood=" + newFood + ", NewMaterialValue=" + newMaterialValue + ", PublicTrustDelta=" + publicTrustDelta + ", LoyaltyDelta=" + loyaltyDelta + ", SecurityDelta=" + securityDelta);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplySharedCivilianReliefPoolEffects failed (" + (reason ?? "N/A") + "): " + ex.Message);
			return false;
		}
	}

	private static bool ApplySoldierMaterialReliefChoice(int targetAgentIndex, string triggerSource, string triggerDetail)
	{
		try
		{
			if (targetAgentIndex < 0 || !AlliedAgentIndexes.Contains(targetAgentIndex))
			{
				InformationManager.DisplayMessage(new InformationMessage(SiegeReliefChoiceProfile.SoldierMaterialReliefTargetMessage, Color.FromUint(SiegeReliefChoiceProfile.ValidationMessageColor)));
				return false;
			}
			if (!HasSharedCivilianReliefPool())
			{
				InformationManager.DisplayMessage(new InformationMessage(SiegeReliefChoiceProfile.SoldierMaterialReliefMissingPoolMessage, Color.FromUint(SiegeReliefChoiceProfile.ValidationMessageColor)));
				return false;
			}
			return ApplyReliefChoiceCore(triggerSource, triggerDetail, requireSharedMaterial: true, civilianVerbalOnly: false);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplySoldierMaterialReliefChoice failed: " + ex.Message);
			return false;
		}
	}

	private static bool ApplyCivilianVerbalReliefChoice(string triggerSource, string triggerDetail)
	{
		return ApplyReliefChoiceCore(triggerSource, triggerDetail, requireSharedMaterial: false, civilianVerbalOnly: !HasSharedCivilianReliefPool());
	}

	private static bool ApplyReliefChoiceCore(string triggerSource, string triggerDetail, bool requireSharedMaterial, bool civilianVerbalOnly)
	{
		try
		{
			if (TryBlockMercyTrackAfterDestructive("安抚"))
			{
				return false;
			}
			bool hasSharedPool = HasSharedCivilianReliefPool();
			if (requireSharedMaterial && !hasSharedPool)
			{
				InformationManager.DisplayMessage(new InformationMessage(SiegeReliefChoiceProfile.RequiredSharedMaterialMissingMessage, Color.FromUint(SiegeReliefChoiceProfile.ValidationMessageColor)));
				return false;
			}
			SiegeReliefChoiceProfile reliefProfile = SiegeReliefChoiceProfile.Build(hasSharedPool, civilianVerbalOnly, DescribeSharedCivilianReliefPoolForContext());
			StopReversiblePlunderForMercyTrack("relief");
			if (_reliefChoiceApplied)
			{
				if (reliefProfile.HasSharedPool && !string.IsNullOrWhiteSpace(reliefProfile.RepeatSharedPoolEffectReason))
				{
					ApplySharedCivilianReliefPoolEffects(ResolveCurrentSettlement(), reliefProfile.RepeatSharedPoolEffectReason);
				}
				RecordInterventionMemory(reliefProfile.RepeatMemoryTitle, reliefProfile.RepeatMemoryText);
				return true;
			}
			_reliefChoiceApplied = true;
			_activeMode = InterventionMode.MercyRelief;
			MarkPendingAftermath(SiegeAftermathAction.SiegeAftermath.ShowMercy, triggerSource, triggerDetail);
			MaybeTriggerSoldierAppeasementNeed(reliefProfile.SoldierAppeasementReason);
			Settlement settlement = ResolveCurrentSettlement();
			AdjustSettlementAfterRelief(settlement, reliefProfile.PublicTrustDelta, reliefProfile.LoyaltyDelta, reliefProfile.SecurityDelta);
			if (reliefProfile.HasSharedPool && !string.IsNullOrWhiteSpace(reliefProfile.SharedPoolEffectReason))
			{
				ApplySharedCivilianReliefPoolEffects(settlement, reliefProfile.SharedPoolEffectReason);
			}
			ShowOutcomeMessageOnce(reliefProfile.MessageKey, reliefProfile.MessageText, reliefProfile.MessageColor);
			RecordInterventionMemory(reliefProfile.MemoryTitle, reliefProfile.MemoryText);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyReliefChoiceCore failed: " + ex.Message);
			return false;
		}
	}

	private static bool ApplyInspirationChoice(string triggerSource, string triggerDetail)
	{
		try
		{
			if (TryBlockMercyTrackAfterDestructive("安民宣抚"))
			{
				return false;
			}
			SiegeCivicChoiceProfile civicProfile = SiegeCivicChoiceProfile.BuildInspiration();
			StopReversiblePlunderForMercyTrack("inspiration");
			if (_inspirationLevelApplied >= 1)
			{
				ApplySharedCivilianReliefPoolEffects(ResolveCurrentSettlement(), civicProfile.RepeatSharedPoolEffectReason);
				RecordInterventionMemory(civicProfile.RepeatMemoryTitle, civicProfile.RepeatMemoryText);
				return true;
			}
			Settlement settlement = ResolveCurrentSettlement();
			_activeMode = InterventionMode.MercyRelief;
			MarkPendingAftermath(SiegeAftermathAction.SiegeAftermath.ShowMercy, triggerSource, triggerDetail);
			MaybeTriggerSoldierAppeasementNeed(civicProfile.SoldierAppeasementReason);
			AdjustSettlementAfterRelief(settlement, civicProfile.PublicTrustDelta, civicProfile.LoyaltyDelta, civicProfile.SecurityDelta);
			ApplySharedCivilianReliefPoolEffects(settlement, civicProfile.SharedPoolEffectReason);
			_inspirationLevelApplied = civicProfile.ResultingInspirationLevel;
			int relationAdjusted = 0;
			int powerAdjusted = 0;
			if (settlement?.Notables != null)
			{
				foreach (Hero notable in settlement.Notables.ToList())
				{
					if (notable == null || notable == Hero.MainHero)
					{
						continue;
					}
					try
					{
						ChangeRelationAction.ApplyPlayerRelation(notable, civicProfile.NotableRelationDelta, true, true);
						relationAdjusted++;
					}
					catch
					{
					}
					try
					{
						notable.AddPower(civicProfile.NotablePowerDelta);
						powerAdjusted++;
					}
					catch
					{
					}
				}
			}
			GatherCiviliansForSpeech(civicProfile.GatherSource);
			ShowOutcomeMessageOnce(civicProfile.MessageKey, civicProfile.MessageText, civicProfile.MessageColor);
			RecordInterventionMemory(civicProfile.MemoryTitle, civicProfile.MemoryText);
			Logger.Log("SiegeAiIntervention", "Applied inspiration choice. Settlement=" + (settlement?.StringId ?? "N/A") + ", RelationAdjusted=" + relationAdjusted + ", PowerAdjusted=" + powerAdjusted);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyInspirationChoice failed: " + ex.Message);
			return false;
		}
	}

	private static bool ApplyRallyOathChoice(string triggerSource, string triggerDetail)
	{
		try
		{
			if (TryBlockMercyTrackAfterDestructive("归心盟誓"))
			{
				return false;
			}
			SiegeCivicChoiceProfile civicProfile = SiegeCivicChoiceProfile.BuildRallyOath(_inspirationLevelApplied);
			StopReversiblePlunderForMercyTrack("rally_oath");
			if (_inspirationLevelApplied >= 2)
			{
				ApplySharedCivilianReliefPoolEffects(ResolveCurrentSettlement(), civicProfile.RepeatSharedPoolEffectReason);
				RecordInterventionMemory(civicProfile.RepeatMemoryTitle, civicProfile.RepeatMemoryText);
				return true;
			}
			Settlement settlement = ResolveCurrentSettlement();
			_activeMode = InterventionMode.MercyRelief;
			MarkPendingAftermath(SiegeAftermathAction.SiegeAftermath.ShowMercy, triggerSource, triggerDetail);
			MaybeTriggerSoldierAppeasementNeed(civicProfile.SoldierAppeasementReason);
			AdjustSettlementAfterRelief(settlement, civicProfile.PublicTrustDelta, civicProfile.LoyaltyDelta, civicProfile.SecurityDelta);
			ApplySharedCivilianReliefPoolEffects(settlement, civicProfile.SharedPoolEffectReason);
			_inspirationLevelApplied = civicProfile.ResultingInspirationLevel;
			int relationAdjusted = 0;
			int powerAdjusted = 0;
			if (settlement?.Notables != null)
			{
				foreach (Hero notable in settlement.Notables.ToList())
				{
					if (notable == null || notable == Hero.MainHero)
					{
						continue;
					}
					try
					{
						if (civicProfile.NotableRelationDelta > 0)
						{
							ChangeRelationAction.ApplyPlayerRelation(notable, civicProfile.NotableRelationDelta, true, true);
							relationAdjusted++;
						}
					}
					catch
					{
					}
					try
					{
						if (civicProfile.NotablePowerDelta > 0f)
						{
							notable.AddPower(civicProfile.NotablePowerDelta);
							powerAdjusted++;
						}
					}
					catch
					{
					}
				}
			}
			GatherCiviliansForSpeech(civicProfile.GatherSource);
			ShowOutcomeMessageOnce(civicProfile.MessageKey, civicProfile.MessageText, civicProfile.MessageColor);
			RecordInterventionMemory(civicProfile.MemoryTitle, civicProfile.MemoryText);
			Logger.Log("SiegeAiIntervention", "Applied rally oath choice. Settlement=" + (settlement?.StringId ?? "N/A") + ", RelationAdjusted=" + relationAdjusted + ", PowerAdjusted=" + powerAdjusted);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyRallyOathChoice failed: " + ex.Message);
			return false;
		}
	}

	private static bool ApplyMercyChoice(string triggerSource, string triggerDetail)
	{
		try
		{
			if (TryBlockMercyTrackAfterDestructive("宽恕"))
			{
				return false;
			}
			SiegeMercyChoiceProfile mercyProfile = new SiegeMercyChoiceProfile();
			StopReversiblePlunderForMercyTrack(mercyProfile.StopPlunderReason);
			_activeMode = InterventionMode.MercyRelief;
			MarkPendingAftermath(SiegeAftermathAction.SiegeAftermath.ShowMercy, triggerSource, triggerDetail);
			MaybeTriggerSoldierAppeasementNeed(mercyProfile.SoldierAppeasementReason);
			ApplySharedCivilianReliefPoolEffects(ResolveCurrentSettlement(), mercyProfile.SharedPoolEffectReason);
			ShowOutcomeMessageOnce(mercyProfile.MessageKey, mercyProfile.MessageText, mercyProfile.MessageColor);
			RecordInterventionMemory(mercyProfile.MemoryTitle, mercyProfile.MemoryText);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyMercyChoice failed: " + ex.Message);
			return false;
		}
	}

	private static void AwardGoldToPlayer(int amount, string source)
	{
		if (amount <= 0 || Hero.MainHero == null)
		{
			return;
		}
		try
		{
			int before = Hero.MainHero.Gold;
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, amount, disableNotification: true);
			if (Hero.MainHero.Gold < before + amount)
			{
				Hero.MainHero.ChangeHeroGold(before + amount - Hero.MainHero.Gold);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "AwardGoldToPlayer failed (" + source + "): " + ex.Message);
			try
			{
				Hero.MainHero.ChangeHeroGold(amount);
			}
			catch
			{
			}
		}
	}

	private static bool StartPlunder(string triggerSource, string triggerDetail)
	{
		if (_massacreStarted)
		{
			return false;
		}
		if (!IsDestructiveInterventionAllowed())
		{
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】该定居点与你当前阵营文化相同，军纪禁止掠夺。", Color.FromUint(0xFFFFD27Fu)));
			return false;
		}
		SiegeDestructiveChoiceProfile plunderProfile = SiegeDestructiveChoiceProfile.BuildPlunder();
		_activeMode = InterventionMode.Plunder;
		if (!_plunderStarted)
		{
			_plunderStarted = true;
			MarkPendingAftermath(ToNativeAftermathKind(plunderProfile.AftermathKind), triggerSource, triggerDetail);
			EnsureAlliedTroopsSummoned();
			MaintainCivilianAssembly(Mission.Current, plunderProfile.AssemblySource, force: false);
			ShowOutcomeMessageOnce(plunderProfile.MessageKey, plunderProfile.MessageText, plunderProfile.MessageColor);
			RecordInterventionMemory(plunderProfile.MemoryTitle, plunderProfile.FirstMemoryText);
			return true;
		}
		MarkPendingAftermath(ToNativeAftermathKind(plunderProfile.AftermathKind), triggerSource, triggerDetail);
		RecordInterventionMemory(plunderProfile.MemoryTitle, plunderProfile.RepeatMemoryText);
		return false;
	}

	private static bool StartMassacre(string triggerSource, string triggerDetail)
	{
		if (!IsDestructiveInterventionAllowed())
		{
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】该定居点与你当前阵营文化相同，军纪禁止毁坏或血洗。", Color.FromUint(0xFFFFD27Fu)));
			return false;
		}
		SiegeDestructiveChoiceProfile massacreProfile = SiegeDestructiveChoiceProfile.BuildMassacre();
		_activeMode = InterventionMode.Massacre;
		_civilianGatherPropagationActive = false;
		ActiveCivilianGatherInteractions.Clear();
		bool first = !_massacreStarted;
		_massacreStarted = true;
		if (first)
		{
			_lastMassacreRealKillMissionTime = Mission.Current?.CurrentTime ?? 0f;
			VictoryCheerAgentIndexes.Clear();
		}
		MarkPendingAftermath(ToNativeAftermathKind(massacreProfile.AftermathKind), triggerSource, triggerDetail);
		EnsureAlliedTroopsSummoned();
		EnsureInterventionCivilianEnemyTeam(Mission.Current);
		AdjustSettlementPublicTrustOnly(ResolveCurrentSettlement(), massacreProfile.PublicTrustDelta, massacreProfile.PublicTrustReason);
		DriveMassacreCombatState(Mission.Current);
		if (first)
		{
			ShowOutcomeMessageOnce(massacreProfile.MessageKey, massacreProfile.MessageText, massacreProfile.MessageColor);
			RecordInterventionMemory(massacreProfile.MemoryTitle, massacreProfile.BuildMassacreMemoryText(triggerSource));
		}
		return first;
	}

	private static bool RequestCulturalRepopulation(int targetAgentIndex, string triggerSource, string triggerDetail)
	{
		try
		{
			if (!IsDestructiveInterventionAllowed())
			{
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】该定居点与你当前阵营文化相同，不能执行屠民迁殖。", Color.FromUint(0xFFFFD27Fu)));
				return false;
			}
			if (!AlliedAgentIndexes.Contains(targetAgentIndex))
			{
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】屠民迁殖只能与己方士兵对话触发，不能由平民或其他NPC触发。", Color.FromUint(0xFFFFD27Fu)));
				return false;
			}
			bool handled = true;
			SiegeCulturalRepopulationProfile repopulationProfile = new SiegeCulturalRepopulationProfile();
			if (!_massacreStarted)
			{
				handled |= StartMassacre(repopulationProfile.MassacreTriggerSource, repopulationProfile.MassacreTriggerDetail);
			}
			CultureObject targetCulture = ResolveCulturalRepopulationTargetCulture(out string targetCultureSource);
			string targetCultureText = DescribeCultureForMessage(targetCulture, targetCultureSource);
			_culturalRepopulationRequested = true;
			MarkPendingAftermath(ToNativeAftermathKind(repopulationProfile.AftermathKind), triggerSource, triggerDetail);
			RecordInterventionMemory(repopulationProfile.MemoryTitle, repopulationProfile.BuildRequestMemoryText(targetCultureText));
			if (_massacreVictoryReached)
			{
				handled |= ApplyCulturalRepopulationNow("victory_already_reached");
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(repopulationProfile.BuildPendingMessageText(targetCultureText), Color.FromUint(repopulationProfile.PendingMessageColor)));
			}
			return handled;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "RequestCulturalRepopulation failed: " + ex.Message);
			return false;
		}
	}

	private static bool ApplyCulturalRepopulationNow(string source)
	{
		try
		{
			if (_culturalRepopulationApplied)
			{
				return false;
			}
			Settlement settlement = ResolveCurrentSettlement();
			CultureObject targetCulture = ResolveCulturalRepopulationTargetCulture(out string targetCultureSource);
			if (settlement == null || targetCulture == null)
			{
				Logger.Log("SiegeAiIntervention", "ApplyCulturalRepopulationNow skipped. Source=" + (source ?? "N/A") + ", Settlement=" + (settlement?.StringId ?? "null") + ", TargetCulture=" + (targetCulture?.StringId ?? "null") + ", TargetCultureSource=" + (targetCultureSource ?? "N/A"));
				return false;
			}
			CultureObject oldCulture = settlement.Culture;
			settlement.Culture = targetCulture;
			int boundVillagesChanged = 0;
			try
			{
				if (settlement.BoundVillages != null)
				{
					foreach (Village village in settlement.BoundVillages)
					{
						if (village?.Settlement != null)
						{
							village.Settlement.Culture = targetCulture;
							boundVillagesChanged++;
						}
					}
				}
			}
			catch
			{
			}
			int killedNotables = 0;
			int spawnedNotables = 0;
			ReplaceTownNotablesForCulturalRepopulation(settlement, targetCulture, source, out killedNotables, out spawnedNotables);
			_lastKilledNotables += killedNotables;
			_culturalRepopulationApplied = true;
			SiegeCulturalRepopulationProfile repopulationProfile = new SiegeCulturalRepopulationProfile();
			string notableResultText = repopulationProfile.BuildCompletedNotableResultText(settlement.IsTown, killedNotables, spawnedNotables);
			string settlementName = settlement.Name?.ToString();
			string targetCultureText = DescribeCultureForMessage(targetCulture, targetCultureSource);
			InformationManager.DisplayMessage(new InformationMessage(repopulationProfile.BuildCompletedMessageText(settlementName, targetCultureText, notableResultText), Color.FromUint(repopulationProfile.CompletedMessageColor)));
			Logger.Log("SiegeAiIntervention", "Applied purge repopulation. Source=" + (source ?? "N/A") + ", Settlement=" + (settlement.StringId ?? "N/A") + ", OldCulture=" + (oldCulture?.StringId ?? "N/A") + ", NewCulture=" + (targetCulture.StringId ?? "N/A") + ", TargetCultureSource=" + (targetCultureSource ?? "N/A") + ", BoundVillages=" + boundVillagesChanged + ", KilledNotables=" + killedNotables + ", SpawnedNotables=" + spawnedNotables);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyCulturalRepopulationNow failed: " + ex.Message);
			return false;
		}
	}

	private static void ReplaceTownNotablesForCulturalRepopulation(Settlement settlement, CultureObject targetCulture, string source, out int killedNotables, out int spawnedNotables)
	{
		killedNotables = 0;
		spawnedNotables = 0;
		try
		{
			if (settlement == null || targetCulture == null || !settlement.IsTown)
			{
				return;
			}
			List<Hero> oldNotables = settlement.Notables?.Where((Hero notable) => notable != null && notable != Hero.MainHero && notable.IsNotable && notable.IsAlive).ToList() ?? new List<Hero>();
			foreach (Hero notable in oldNotables)
			{
				if (TryKillNotableForCulturalRepopulation(notable, source))
				{
					killedNotables++;
				}
			}
			SpawnReplacementNotablesForCulturalRepopulation(settlement, targetCulture, Occupation.GangLeader, source, ref spawnedNotables);
			SpawnReplacementNotablesForCulturalRepopulation(settlement, targetCulture, Occupation.Artisan, source, ref spawnedNotables);
			SpawnReplacementNotablesForCulturalRepopulation(settlement, targetCulture, Occupation.Merchant, source, ref spawnedNotables);
			Logger.Log("SiegeAiIntervention", "Replaced town notables for purge repopulation. Source=" + (source ?? "N/A") + ", Settlement=" + (settlement.StringId ?? "N/A") + ", TargetCulture=" + (targetCulture.StringId ?? "N/A") + ", Killed=" + killedNotables + ", Spawned=" + spawnedNotables);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ReplaceTownNotablesForCulturalRepopulation failed. Source=" + (source ?? "N/A") + ", Settlement=" + (settlement?.StringId ?? "null") + ": " + ex.Message);
		}
	}

	private static bool TryKillNotableForCulturalRepopulation(Hero notable, string source)
	{
		try
		{
			if (notable == null || notable == Hero.MainHero || !notable.IsNotable || !notable.IsAlive)
			{
				return false;
			}
			try
			{
				float power = notable.Power;
				if (Math.Abs(power) > 0.01f)
				{
					notable.AddPower(-power);
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "Failed to zero notable power before purge replacement. Source=" + (source ?? "N/A") + ", Notable=" + (notable.StringId ?? "N/A") + ": " + ex.Message);
			}
			KillCharacterAction.ApplyByMurder(notable, Hero.MainHero, false);
			if (notable.IsAlive)
			{
				KillCharacterAction.ApplyByRemove(notable, false, true);
			}
			if (!notable.IsAlive)
			{
				return true;
			}
			Logger.Log("SiegeAiIntervention", "Purge repopulation could not kill notable. Source=" + (source ?? "N/A") + ", Notable=" + (notable.StringId ?? "N/A"));
			return false;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryKillNotableForCulturalRepopulation failed. Source=" + (source ?? "N/A") + ", Notable=" + (notable?.StringId ?? "null") + ": " + ex.Message);
			return false;
		}
	}

	private static void SpawnReplacementNotablesForCulturalRepopulation(Settlement settlement, CultureObject targetCulture, Occupation occupation, string source, ref int spawnedNotables)
	{
		try
		{
			if (settlement == null || targetCulture == null)
			{
				return;
			}
			int targetCount = Campaign.Current?.Models?.NotableSpawnModel?.GetTargetNotableCountForSettlement(settlement, occupation) ?? 0;
			int currentCount = 0;
			if (settlement.Notables != null)
			{
				currentCount = settlement.Notables.Count((Hero notable) => notable != null && notable.IsAlive && notable.Occupation == occupation);
			}
			for (int i = currentCount; i < targetCount; i++)
			{
				try
				{
					Hero newNotable = HeroCreator.CreateNotable(occupation, settlement);
					if (newNotable == null)
					{
						Logger.Log("SiegeAiIntervention", "Purge repopulation failed to create replacement notable. Source=" + (source ?? "N/A") + ", Settlement=" + (settlement.StringId ?? "N/A") + ", Occupation=" + occupation);
						continue;
					}
					if (newNotable.Culture != targetCulture)
					{
						newNotable.Culture = targetCulture;
					}
					if (newNotable.CurrentSettlement != settlement)
					{
						EnterSettlementAction.ApplyForCharacterOnly(newNotable, settlement);
					}
					spawnedNotables++;
				}
				catch (Exception ex)
				{
					Logger.Log("SiegeAiIntervention", "Purge repopulation replacement notable spawn failed. Source=" + (source ?? "N/A") + ", Settlement=" + (settlement.StringId ?? "N/A") + ", Occupation=" + occupation + ": " + ex.Message);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "SpawnReplacementNotablesForCulturalRepopulation failed. Source=" + (source ?? "N/A") + ", Settlement=" + (settlement?.StringId ?? "null") + ", Occupation=" + occupation + ": " + ex.Message);
		}
	}

	private static void EnsureAlliedTroopsSummoned()
	{
		if (!_alliedTroopsAutoSummoned)
		{
			_alliedTroopsAutoSummoned = true;
			SummonAlliedTroops(AutoSummonCount, "ensure");
		}
	}

	private static void ApplyPlayerBattleEquipment()
	{
		if (_playerBattleEquipmentApplied)
		{
			return;
		}
		try
		{
			Agent main = Agent.Main ?? Mission.Current?.MainAgent;
			Equipment equipment = Hero.MainHero?.BattleEquipment;
			if (main == null || equipment == null)
			{
				return;
			}
			main.UpdateSpawnEquipmentAndRefreshVisuals(equipment);
			main.WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp, Equipment.InitialWeaponEquipPreference.Any);
			_playerBattleEquipmentApplied = true;
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】你已披甲执兵入城。", Color.FromUint(0xFFB6F7A8u)));
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyPlayerBattleEquipment failed: " + ex.Message);
		}
	}

	private static void RemoveDefeatedGuardAgents(Mission mission)
	{
		if (mission?.Agents == null)
		{
			return;
		}
		foreach (Agent agent in mission.Agents.ToList())
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive() || agent == Agent.Main || AlliedAgentIndexes.Contains(agent.Index))
			{
				continue;
			}
			if (IsGuardOrSoldier(agent.Character as CharacterObject))
			{
				RemoveGuardAgent(agent);
			}
		}
	}

	private static void RemoveBackstreetCrimeAgents(Mission mission)
	{
		if (mission?.Agents == null)
		{
			return;
		}
		foreach (Agent agent in mission.Agents.ToList())
		{
			try
			{
				if (agent == null || !agent.IsHuman || !agent.IsActive() || agent == Agent.Main || AlliedAgentIndexes.Contains(agent.Index))
				{
					continue;
				}
				if (IsBackstreetOrCriminalCharacter(agent.Character as CharacterObject))
				{
					ShoutBehavior.CancelAgentSpeechForRemovalExternal(agent.Index, "siege_intervention_backstreet_criminal_removed");
					agent.FadeOut(hideInstantly: true, hideMount: true);
				}
			}
			catch
			{
			}
		}
	}

	private static void RemoveUnsafeAssemblyCivilianAgents(Mission mission)
	{
		if (mission?.Agents == null)
		{
			return;
		}
		foreach (Agent agent in mission.Agents.ToList())
		{
			try
			{
				if (agent == null || !agent.IsHuman || !agent.IsActive() || agent == Agent.Main || AlliedAgentIndexes.Contains(agent.Index))
				{
					continue;
				}
				CharacterObject character = agent.Character as CharacterObject;
				if (!IsCivilianForIntervention(character))
				{
					continue;
				}
				if (IsUnsafeAssemblyCivilianTemplate(character))
				{
					ShoutBehavior.CancelAgentSpeechForRemovalExternal(agent.Index, "siege_intervention_unsafe_or_naked_civilian_removed");
					agent.FadeOut(hideInstantly: true, hideMount: true);
					SceneCivilianAgentIndexes.Remove(agent.Index);
					CivilianAssemblySlots.Remove(agent.Index);
					CivilianSpeechRallySlots.Remove(agent.Index);
					CivilianGatherFollowerAgentIndexes.Remove(agent.Index);
					CivilianGatherReadyFormationAgentIndexes.Remove(agent.Index);
					CivilianGatherMovePreparedAgentIndexes.Remove(agent.Index);
					CivilianFrightenedActionAgentIndexes.Remove(agent.Index);
					CivilianPreMassacrePreparedAgentIndexes.Remove(agent.Index);
					Logger.Log("SiegeAiIntervention", "Removed unsafe/naked civilian agent. Agent=" + agent.Index + ", Character=" + (character?.StringId ?? "N/A") + ", Name=" + (character?.Name?.ToString() ?? "N/A"));
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "RemoveUnsafeAssemblyCivilianAgents failed: " + ex.Message);
			}
		}
	}

	private static void RemoveProtectedSceneAgents(Mission mission)
	{
		if (mission?.Agents == null)
		{
			return;
		}
		foreach (Agent agent in mission.Agents.ToList())
		{
			try
			{
				if (agent == null || !agent.IsHuman || !agent.IsActive() || agent == Agent.Main || AlliedAgentIndexes.Contains(agent.Index))
				{
					continue;
				}
				if (IsProtectedChildAgent(agent) || IsProtectedNotableAgent(agent))
				{
					ShoutBehavior.CancelAgentSpeechForRemovalExternal(agent.Index, "siege_intervention_protected_agent_suppressed");
					agent.FadeOut(hideInstantly: true, hideMount: true);
				}
			}
			catch
			{
			}
		}
	}

	private static void RemovePlayerCompanionSceneAgents(Mission mission)
	{
		if (mission?.Agents == null)
		{
			return;
		}
		foreach (Agent agent in mission.Agents.ToList())
		{
			try
			{
				if (!IsPlayerOwnedCompanionSceneAgent(agent))
				{
					continue;
				}
				ShoutBehavior.CancelAgentSpeechForRemovalExternal(agent.Index, "siege_intervention_player_companion_scene_spawn_suppressed");
				agent.FadeOut(hideInstantly: true, hideMount: true);
			}
			catch
			{
			}
		}
	}

	private static bool IsPlayerOwnedCompanionSceneAgent(Agent agent)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive() || agent == Agent.Main || AlliedAgentIndexes.Contains(agent.Index))
			{
				return false;
			}
			CharacterObject character = agent.Character as CharacterObject;
			Hero hero = character?.HeroObject;
			if (hero == null || hero == Hero.MainHero)
			{
				return false;
			}
			if (hero.IsPlayerCompanion)
			{
				return true;
			}
			return hero.Clan == Clan.PlayerClan && hero.PartyBelongedTo == MobileParty.MainParty;
		}
		catch
		{
			return false;
		}
	}

	private static void RemoveGuardAgent(Agent agent)
	{
		try
		{
			if (agent == null || !agent.IsActive())
			{
				return;
			}
			ShoutBehavior.CancelAgentSpeechForRemovalExternal(agent.Index, "siege_intervention_guard_removed");
			agent.FadeOut(hideInstantly: true, hideMount: true);
		}
		catch
		{
		}
	}

	private static void TrackSceneCivilianAgents(Mission mission)
	{
		if (mission?.Agents == null)
		{
			return;
		}
		foreach (Agent agent in mission.Agents.ToList())
		{
			if (IsEligibleCivilianAgent(agent, includeHeroes: true))
			{
				SceneCivilianAgentIndexes.Add(agent.Index);
				_lastSceneCivilianSpawnedCount = Math.Max(_lastSceneCivilianSpawnedCount, SceneCivilianAgentIndexes.Count);
			}
		}
	}

	private static void MaintainCivilianAssembly(Mission mission, string source, bool force)
	{
		try
		{
			if (mission?.Agents == null || _massacreStarted || _massacreVictoryReached)
			{
				return;
			}
			int desiredCount = GetDesiredCivilianAssemblyCount(mission);
			EnsureCivilianAssemblyPopulation(mission);
			int total = 0;
			foreach (Agent agent in mission.Agents.ToList().OrderBy(a => a?.Index ?? int.MaxValue))
			{
				if (!IsEligibleCivilianAgent(agent, includeHeroes: true))
				{
					continue;
				}
				total++;
				SceneCivilianAgentIndexes.Add(agent.Index);
				PrepareCivilianForPreMassacreHitDetection(agent, mission);
				ApplyOneTimeFrightenedCivilianAction(agent, allowGathered: true);
			}
			_lastSceneCivilianSpawnedCount = Math.Max(_lastSceneCivilianSpawnedCount, Math.Max(total, SceneCivilianAgentIndexes.Count));
			if (!_civilianAssemblyMessageShown && total > 0)
			{
				_civilianAssemblyMessageShown = true;
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】城内有 " + total + " 名普通民众等待处置。士兵已跟随你入城。", Color.FromUint(0xFFB6F7A8u)));
				Logger.Log("SiegeAiIntervention", "Civilian town population prepared. Source=" + (source ?? "N/A") + ", Civilians=" + total + ", Desired=" + desiredCount);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "MaintainCivilianAssembly failed (" + (source ?? "N/A") + "): " + ex.Message);
		}
	}

	private static void InitializeCivilianAssemblyPoint(Mission mission, Agent main)
	{
		InitializeCivilianAssemblyPoint(mission, main, CivilianAssemblyForwardDistance);
	}

	private static void InitializeCivilianAssemblyPoint(Mission mission, Agent main, float forwardDistance)
	{
		Vec3 forward = main?.LookDirection ?? Vec3.Forward;
		if (forward.LengthSquared < 0.01f)
		{
			forward = Vec3.Forward;
		}
		forward.Normalize();
		Vec3 anchor = (main?.Position ?? Vec3.Zero) + forward * Math.Max(1f, forwardDistance);
		try
		{
			if (mission?.Scene != null)
			{
				anchor.z = mission.Scene.GetGroundHeightAtPosition(anchor);
			}
		}
		catch
		{
		}
		_civilianAssemblyAnchor = anchor;
		_civilianAssemblyForward = forward;
		_civilianAssemblyPointReady = true;
	}

	private static bool GatherCiviliansForSpeech(string source, int seedAgentIndex = -1)
	{
		try
		{
			Mission mission = Mission.Current;
			Agent main = Agent.Main ?? mission?.MainAgent;
			if (mission?.Agents == null || main == null || !main.IsActive() || _massacreStarted || _massacreVictoryReached)
			{
				return false;
			}
			if (_civilianFormationControlPending || _civilianFormationControlComplete)
			{
				Logger.Log("SiegeAiIntervention", "Civilian gathering already entering command control; ignored new messenger trigger. Source=" + (source ?? "N/A"));
				return false;
			}
			bool firstStart = !_civilianSpeechRallyActive && !_civilianGatherPropagationActive;
			if (firstStart)
			{
				_civilianSpeechRallyActive = true;
				_civilianGatherPropagationActive = true;
				_civilianFormationControlPending = false;
				_civilianFormationControlComplete = false;
				_civilianFormationControlMessageShown = false;
				_civilianFormationControlNotBeforeTime = -1f;
				_nextCivilianFormationControlBatchTime = 0f;
				_nextCivilianGatherTickTime = 0f;
				_civilianGatherStartedAt = mission.CurrentTime;
				_civilianGatherMessengerSpeechBudget = CivilianGatherMessengerSpeechMinCount + ((CivilianGatherMessengerSpeechMaxCount > CivilianGatherMessengerSpeechMinCount && MBRandom.RandomFloat >= 0.5f) ? 1 : 0);
				_civilianGatherMessengerSpeechCount = 0;
				ActiveCivilianGatherInteractions.Clear();
				CivilianGatherReadyFormationAgentIndexes.Clear();
				CivilianGatherMessengerAgentIndexes.Clear();
				CivilianGatherMessengerSpeechAgentIndexes.Clear();
			}
			EnsureCivilianAssemblyPopulation(mission);
			TrackSceneCivilianAgents(mission);
			Agent seed = TryGetAgent(seedAgentIndex);
			bool seedIsSoldier = IsInterventionAlliedSoldierForExternal(seed, requireActive: true);
			bool seedIsCivilian = IsEligibleCivilianAgent(seed, includeHeroes: true);
			int addedMessengers = 0;
			if (seedIsSoldier)
			{
				addedMessengers += EnsureSoldierGatherMessengers(mission, seed);
			}
			else
			{
				if (!seedIsCivilian)
				{
					seed = mission.Agents.Where(a => IsEligibleCivilianAgent(a, includeHeroes: true)).OrderBy(a => a.Position.DistanceSquared(main.Position)).FirstOrDefault();
				}
				if (MarkAgentAsCivilianGatherMessenger(seed, "gather_seed:" + (source ?? "N/A")))
				{
					addedMessengers++;
				}
			}
			if (CivilianGatherMessengerAgentIndexes.Count == 0)
			{
				Agent fallback = mission.Agents.Where(a => IsEligibleCivilianAgent(a, includeHeroes: true)).OrderBy(a => a.Position.DistanceSquared(main.Position)).FirstOrDefault();
				if (MarkAgentAsCivilianGatherMessenger(fallback, "gather_fallback:" + (source ?? "N/A")))
				{
					addedMessengers++;
				}
			}
			int total = RebuildCivilianSpeechRallySlots(mission);
			MaintainCivilianSpeechRally(mission, force: true);
			if (firstStart)
			{
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】传令已经发出，民众会逐步聚拢，等待你的进一步命令。", Color.FromUint(0xFFB6F7A8u)));
				RecordInterventionMemory("召集", "玩家已下令召集民众，" + (seedIsSoldier ? "己方士兵" : "平民") + "开始作为传令者通知城内民众前来听训/接受处置。");
			}
			else if (addedMessengers > 0)
			{
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】新的传令者已加入召集。", Color.FromUint(0xFFB6F7A8u)));
				RecordInterventionMemory("召集", "玩家追加了传令者继续通知民众；当前传令者约 " + CivilianGatherMessengerAgentIndexes.Count + " 人。");
			}
			Logger.Log("SiegeAiIntervention", "Updated civilian gathering propagation. Source=" + (source ?? "N/A") + ", FirstStart=" + firstStart + ", Seed=" + (seed?.Index.ToString() ?? "none") + ", SeedSoldier=" + seedIsSoldier + ", AddedMessengers=" + addedMessengers + ", Messengers=" + CivilianGatherMessengerAgentIndexes.Count + ", Civilians=" + total);
			return firstStart || addedMessengers > 0;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "GatherCiviliansForSpeech failed: " + ex.Message);
			return false;
		}
	}

	private static int RebuildCivilianSpeechRallySlots(Mission mission)
	{
		CivilianSpeechRallySlots.Clear();
		if (mission?.Agents == null)
		{
			return 0;
		}
		int slot = 0;
		foreach (Agent agent in mission.Agents.ToList().Where(a => IsEligibleCivilianAgent(a, includeHeroes: true)).OrderBy(a => a?.Index ?? int.MaxValue))
		{
			if (agent == null)
			{
				continue;
			}
			CivilianSpeechRallySlots[agent.Index] = slot++;
			SceneCivilianAgentIndexes.Add(agent.Index);
		}
		return slot;
	}

	private static void MaintainCivilianSpeechRally(Mission mission, bool force)
	{
		try
		{
			if (!_civilianSpeechRallyActive || mission?.Agents == null || _massacreStarted || _massacreVictoryReached)
			{
				return;
			}
			if (_civilianFormationControlPending || _civilianFormationControlComplete)
			{
				return;
			}
			Agent main = Agent.Main ?? mission.MainAgent;
			if (main == null || !main.IsActive())
			{
				return;
			}
			EnsureCivilianAssemblyPopulation(mission);
			TrackSceneCivilianAgents(mission);
			if (CivilianSpeechRallySlots.Count == 0 || force)
			{
				RebuildCivilianSpeechRallySlots(mission);
			}
			float now = mission.CurrentTime;
			if (_civilianGatherPropagationActive && _civilianGatherStartedAt >= 0f && now - _civilianGatherStartedAt >= CivilianGatherFallbackSeconds)
			{
				foreach (Agent agent in mission.Agents.ToList().Where(a => IsEligibleCivilianAgent(a, includeHeroes: true)))
				{
					MarkCivilianAsGatherFollower(agent, "gather_120s_fallback");
				}
				ActiveCivilianGatherInteractions.Clear();
				QueueCivilianFormationControl(mission, "gather_120s_elapsed");
				return;
			}
			if (_civilianGatherPropagationActive && now >= _nextCivilianGatherTickTime)
			{
				_nextCivilianGatherTickTime = now + 0.5f;
				UpdateCivilianGatherInteractions(mission);
				AssignCivilianGatherInteractions(mission);
			}
			bool allFollowing = AreAllCiviliansGatherFollowing(mission);
			if (allFollowing)
			{
				_civilianGatherPropagationActive = false;
				ActiveCivilianGatherInteractions.Clear();
				MaintainCivilianGatherFollowers(mission, main, force: true);
				if (AreAllCivilianGatherFollowersSettled(mission, main))
				{
					QueueCivilianFormationControl(mission, "all_civilians_gathered_and_settled");
					return;
				}
			}
			else
			{
				MaintainCivilianGatherFollowers(mission, main, force);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "MaintainCivilianSpeechRally failed: " + ex.Message);
		}
	}

	private static bool MarkCivilianAsGatherFollower(Agent agent, string reason)
	{
		try
		{
			if (!IsEligibleCivilianAgent(agent, includeHeroes: true))
			{
				return false;
			}
			PrepareCivilianForPreMassacreHitDetection(agent, Mission.Current ?? agent.Mission);
			NeutralizeCivilianDailyUsableBehavior(agent, "gather_mark:" + (reason ?? "N/A"));
			CivilianGatherFollowerAgentIndexes.Add(agent.Index);
			if (CivilianGatherMessengerAgentIndexes.Contains(agent.Index))
			{
				CivilianGatherMovePreparedAgentIndexes.Add(agent.Index);
			}
			if (!CivilianSpeechRallySlots.ContainsKey(agent.Index))
			{
				CivilianSpeechRallySlots[agent.Index] = CivilianSpeechRallySlots.Count;
			}
			agent.SetWatchState(Agent.WatchState.Patrolling);
			try
			{
				agent.SetCrouchMode(false);
				agent.SetMaximumSpeedLimit(-1f, false);
			}
			catch
			{
			}
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "MarkCivilianAsGatherFollower failed (" + (reason ?? "N/A") + "): " + ex.Message);
			return false;
		}
	}

	private static bool MarkAgentAsCivilianGatherMessenger(Agent agent, string reason)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive() || agent == Agent.Main)
			{
				return false;
			}
			if (IsInterventionAlliedSoldierForExternal(agent, requireActive: true))
			{
				if (!CivilianGatherMessengerAgentIndexes.Add(agent.Index))
				{
					return false;
				}
				DisableCompanionStyleFollow(agent);
				AssignAgentToPlayerFormation(agent, FormationClass.Infantry, refreshFormationOrders: false);
				agent.ClearTargetFrame();
				agent.InvalidateTargetAgent();
				agent.SetWatchState(Agent.WatchState.Patrolling);
				agent.SetCrouchMode(false);
				agent.SetMaximumSpeedLimit(-1f, false);
				Logger.Log("SiegeAiIntervention", "Added soldier civilian-gather messenger. Reason=" + (reason ?? "N/A") + ", Agent=" + agent.Index);
				return true;
			}
			if (!IsEligibleCivilianAgent(agent, includeHeroes: true))
			{
				return false;
			}
			if (!CivilianGatherMessengerAgentIndexes.Add(agent.Index))
			{
				return false;
			}
			MarkCivilianAsGatherFollower(agent, reason);
			Logger.Log("SiegeAiIntervention", "Added civilian gather messenger. Reason=" + (reason ?? "N/A") + ", Agent=" + agent.Index);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "MarkAgentAsCivilianGatherMessenger failed (" + (reason ?? "N/A") + "): " + ex.Message);
			return false;
		}
	}

	private static int EnsureSoldierGatherMessengers(Mission mission, Agent seed)
	{
		try
		{
			if (mission?.Agents == null)
			{
				return 0;
			}
			List<Agent> soldiers = mission.Agents
				.Where(a => IsInterventionAlliedSoldierForExternal(a, requireActive: true))
				.OrderBy(a => seed != null ? a.Position.DistanceSquared(seed.Position) : a.Index)
				.ThenBy(a => a.Index)
				.ToList();
			if (soldiers.Count == 0)
			{
				return 0;
			}
			int desired = Math.Max(1, (int)Math.Ceiling(soldiers.Count * CivilianGatherSoldierMessengerRatio));
			int current = soldiers.Count(a => CivilianGatherMessengerAgentIndexes.Contains(a.Index));
			int added = 0;
			if (seed != null && IsInterventionAlliedSoldierForExternal(seed, requireActive: true) && current < desired)
			{
				if (MarkAgentAsCivilianGatherMessenger(seed, "soldier_seed_20_percent"))
				{
					added++;
					current++;
				}
			}
			foreach (Agent soldier in soldiers)
			{
				if (current >= desired)
				{
					break;
				}
				if (soldier == null || (seed != null && soldier.Index == seed.Index))
				{
					continue;
				}
				if (MarkAgentAsCivilianGatherMessenger(soldier, "soldier_20_percent"))
				{
					added++;
					current++;
				}
			}
			Logger.Log("SiegeAiIntervention", "Ensured soldier gather messengers. Soldiers=" + soldiers.Count + ", Desired=" + desired + ", Added=" + added + ", Current=" + current + ", Seed=" + (seed?.Index.ToString() ?? "none"));
			return added;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnsureSoldierGatherMessengers failed: " + ex.Message);
			return 0;
		}
	}

	private static bool AreAllCiviliansGatherFollowing(Mission mission)
	{
		try
		{
			List<Agent> civilians = mission?.Agents?.Where(a => IsEligibleCivilianAgent(a, includeHeroes: true)).ToList() ?? new List<Agent>();
			return civilians.Count > 0 && civilians.All(a => CivilianGatherFollowerAgentIndexes.Contains(a.Index));
		}
		catch
		{
			return false;
		}
	}

	private static bool AreAllCivilianGatherFollowersSettled(Mission mission, Agent main)
	{
		try
		{
			if (mission?.Agents == null || main == null || !main.IsActive())
			{
				return false;
			}
			List<Agent> civilians = mission.Agents.Where(a => IsEligibleCivilianAgent(a, includeHeroes: true)).ToList();
			if (civilians.Count == 0)
			{
				return false;
			}
			int total = Math.Max(1, CivilianSpeechRallySlots.Count);
			float maxDistanceSq = CivilianGatherFormationSettleDistance * CivilianGatherFormationSettleDistance;
			foreach (Agent agent in civilians)
			{
				if (agent == null || !CivilianGatherFollowerAgentIndexes.Contains(agent.Index))
				{
					return false;
				}
				if (!CivilianSpeechRallySlots.TryGetValue(agent.Index, out int slot))
				{
					return false;
				}
				Vec3 target = GetCivilianFollowerRallyTarget(mission, main, slot, total);
				if (agent.Position.DistanceSquared(target) > maxDistanceSq)
				{
					return false;
				}
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static void AssignCivilianGatherInteractions(Mission mission)
	{
		try
		{
			if (mission?.Agents == null)
			{
				return;
			}
			HashSet<int> busyMessengers = new HashSet<int>(ActiveCivilianGatherInteractions.Values.Select(x => x.MessengerAgentIndex));
			HashSet<int> busyTargets = new HashSet<int>(ActiveCivilianGatherInteractions.Keys);
			List<Agent> messengers = mission.Agents
				.Where(a => IsCivilianGatherMessengerAgent(a) && !busyMessengers.Contains(a.Index))
				.OrderBy(a => IsInterventionAlliedSoldierForExternal(a, requireActive: true) ? 0 : 1)
				.ThenBy(a => a.Index)
				.ToList();
			foreach (Agent messenger in messengers)
			{
				Agent target = mission.Agents
					.Where(a => IsEligibleCivilianAgent(a, includeHeroes: true) && !CivilianGatherFollowerAgentIndexes.Contains(a.Index) && !busyTargets.Contains(a.Index))
					.OrderBy(a => a.Position.DistanceSquared(messenger.Position))
					.FirstOrDefault();
				if (target == null)
				{
					break;
				}
				ActiveCivilianGatherInteractions[target.Index] = new CivilianGatherInteraction
				{
					MessengerAgentIndex = messenger.Index,
					TargetAgentIndex = target.Index,
					StartedAt = mission.CurrentTime,
					TalkSeconds = CivilianGatherTalkMinSeconds + MBRandom.RandomFloat * Math.Max(0f, CivilianGatherTalkMaxSeconds - CivilianGatherTalkMinSeconds)
				};
				busyTargets.Add(target.Index);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "AssignCivilianGatherInteractions failed: " + ex.Message);
		}
	}

	private static void UpdateCivilianGatherInteractions(Mission mission)
	{
		try
		{
			if (mission?.Agents == null || ActiveCivilianGatherInteractions.Count == 0)
			{
				return;
			}
			float now = mission.CurrentTime;
			foreach (CivilianGatherInteraction interaction in ActiveCivilianGatherInteractions.Values.ToList())
			{
				Agent messenger = mission.Agents.FirstOrDefault(a => a != null && a.Index == interaction.MessengerAgentIndex);
				Agent target = mission.Agents.FirstOrDefault(a => a != null && a.Index == interaction.TargetAgentIndex);
				if (!IsCivilianGatherMessengerAgent(messenger) || !IsEligibleCivilianAgent(target, includeHeroes: true) || CivilianGatherFollowerAgentIndexes.Contains(target.Index))
				{
					ReleaseGatherMessengerFromCurrentTarget(messenger, "gather_interaction_invalid_or_target_already_c");
					ActiveCivilianGatherInteractions.Remove(interaction.TargetAgentIndex);
					continue;
				}
				NeutralizeCivilianDailyUsableBehavior(target, "gather_target_wait");
				if (interaction.TalkStartedAt < 0f)
				{
					PrepareGatherMessengerMove(messenger, target);
				}
				float distanceSq = messenger.Position.DistanceSquared(target.Position);
				if (distanceSq <= CivilianGatherApproachDistance * CivilianGatherApproachDistance)
				{
					messenger.SetLookAgent(target);
					target.SetLookAgent(messenger);
					target.SetWatchState(Agent.WatchState.Patrolling);
					if (interaction.TalkStartedAt < 0f)
					{
						interaction.TalkStartedAt = now;
						PauseGatherMessengerForTalk(messenger, target);
						TryTriggerCivilianGatherMessengerSpeech(messenger, target);
					}
					if (now - interaction.TalkStartedAt >= interaction.TalkSeconds)
					{
						MarkCivilianAsGatherFollower(target, "gather_fake_talk");
						ReleaseGatherMessengerFromCurrentTarget(messenger, "gather_target_became_c");
						ActiveCivilianGatherInteractions.Remove(interaction.TargetAgentIndex);
					}
				}
				else if (now - interaction.StartedAt > 18f)
				{
					ReleaseGatherMessengerFromCurrentTarget(messenger, "gather_interaction_timeout");
					ActiveCivilianGatherInteractions.Remove(interaction.TargetAgentIndex);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "UpdateCivilianGatherInteractions failed: " + ex.Message);
		}
	}

	private static bool IsCivilianGatherMessengerBusy(int agentIndex)
	{
		return ActiveCivilianGatherInteractions.Values.Any(x => x.MessengerAgentIndex == agentIndex);
	}

	private static bool IsCivilianGatherMessengerAgent(Agent agent)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive() || agent == Agent.Main)
			{
				return false;
			}
			return CivilianGatherMessengerAgentIndexes.Contains(agent.Index)
				&& (IsInterventionAlliedSoldierForExternal(agent, requireActive: true) || IsEligibleCivilianAgent(agent, includeHeroes: true));
		}
		catch
		{
			return false;
		}
	}

	private static void PrepareGatherMessengerMove(Agent messenger, Agent target)
	{
		try
		{
			if (messenger == null || target == null || !messenger.IsActive() || !target.IsActive())
			{
				return;
			}
			if (IsInterventionAlliedSoldierForExternal(messenger, requireActive: true))
			{
				DisableCompanionStyleFollow(messenger);
			}
			else
			{
				NeutralizeCivilianDailyUsableBehavior(messenger, "gather_messenger_move");
			}
			messenger.SetWatchState(Agent.WatchState.Patrolling);
			messenger.SetMaximumSpeedLimit(CivilianGatherMessengerMoveSpeedLimit, false);
			ClearAgentLookTarget(messenger);
			if (!TryGuideGatherMessengerToTargetAgent(messenger, target))
			{
				messenger.SetTargetPosition(target.Position.AsVec2);
			}
		}
		catch
		{
		}
	}

	private static bool TryGuideGatherMessengerToTargetAgent(Agent messenger, Agent target)
	{
		try
		{
			if (messenger == null || target == null || !messenger.IsActive() || !target.IsActive() || messenger == target)
			{
				return false;
			}
			ScriptBehavior.AddAgentTarget(messenger, target);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryGuideGatherMessengerToTargetAgent failed: " + ex.Message);
			return false;
		}
	}

	private static void ReleaseGatherMessengerFromCurrentTarget(Agent messenger, string reason)
	{
		try
		{
			if (messenger == null || !messenger.IsActive())
			{
				return;
			}
			ClearGatherMessengerAgentTargetMovement(messenger);
			messenger.SetMaximumSpeedLimit(-1f, false);
			if (IsInterventionAlliedSoldierForExternal(messenger, requireActive: true))
			{
				AssignAgentToPlayerFormation(messenger, FormationClass.Infantry, refreshFormationOrders: false);
			}
			Logger.Log("SiegeAiIntervention", "Released gather messenger target. Reason=" + (reason ?? "N/A") + ", Agent=" + messenger.Index);
		}
		catch
		{
		}
	}

	private static void ClearGatherMessengerAgentTargetMovement(Agent messenger)
	{
		try
		{
			if (messenger == null || !messenger.IsActive())
			{
				return;
			}
			CampaignAgentComponent component = messenger.GetComponent<CampaignAgentComponent>();
			DailyBehaviorGroup behaviorGroup = component?.AgentNavigator?.GetBehaviorGroup<DailyBehaviorGroup>();
			if (behaviorGroup?.ScriptedBehavior is ScriptBehavior)
			{
				behaviorGroup.DisableScriptedBehavior();
			}
			messenger.DisableScriptedMovement();
			messenger.ClearTargetFrame();
			messenger.InvalidateTargetAgent();
		}
		catch
		{
		}
	}

	private static void PauseGatherMessengerForTalk(Agent messenger, Agent target)
	{
		try
		{
			if (messenger == null || !messenger.IsActive())
			{
				return;
			}
			try
			{
				ClearGatherMessengerAgentTargetMovement(messenger);
			}
			catch
			{
			}
			if (target != null && target.IsActive())
			{
				messenger.SetLookAgent(target);
				target.SetLookAgent(messenger);
				target.SetWatchState(Agent.WatchState.Patrolling);
				try
				{
					target.DisableScriptedMovement();
					target.ClearTargetFrame();
					target.InvalidateTargetAgent();
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	private static void TryTriggerCivilianGatherMessengerSpeech(Agent messenger, Agent target)
	{
		try
		{
			if (messenger == null || target == null || !messenger.IsActive() || !target.IsActive())
			{
				return;
			}
			if (_civilianGatherMessengerSpeechCount >= _civilianGatherMessengerSpeechBudget)
			{
				return;
			}
			if (!CivilianGatherMessengerSpeechAgentIndexes.Add(messenger.Index))
			{
				return;
			}
			_civilianGatherMessengerSpeechCount++;
			string messengerName = messenger.Name?.ToString() ?? "传令者";
			string targetName = target.Name?.ToString() ?? "民众";
			string factText = "【攻城处置传令】你现在正替玩家传唤城内民众。玩家刚下令召集所有民众到他身边听取训示、安民宣告或后续处置；你已经来到" + targetName + "身边，需要用一句自然现场话把这件事转告给对方。不要写内部标签，不要解释机制，不要说自己不能传话；话里要让对方明白：领主/大人召集大家过去听命、听训或听宣示。";
			ShoutBehavior.TriggerImmediateSceneBehaviorReactionForExternal(factText, messenger.Index, persistHeroPrivateHistory: true, suppressStare: true, postSpeechLeaveSeconds: -1f);
			Logger.Log("SiegeAiIntervention", "Triggered gather messenger speech. Messenger=" + messenger.Index + "/" + messengerName + ", Target=" + target.Index + "/" + targetName + ", Count=" + _civilianGatherMessengerSpeechCount + ", Budget=" + _civilianGatherMessengerSpeechBudget);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryTriggerCivilianGatherMessengerSpeech failed: " + ex.Message);
		}
	}

	private static void MaintainCivilianGatherFollowers(Mission mission, Agent main, bool force)
	{
		try
		{
			if (mission?.Agents == null || main == null)
			{
				return;
			}
			int total = Math.Max(1, CivilianSpeechRallySlots.Count);
			foreach (Agent agent in mission.Agents.ToList().Where(a => IsEligibleCivilianAgent(a, includeHeroes: true) && CivilianGatherFollowerAgentIndexes.Contains(a.Index)))
			{
				if (_civilianGatherPropagationActive && CivilianGatherMessengerAgentIndexes.Contains(agent.Index))
				{
					continue;
				}
				if (IsCivilianGatherMessengerBusy(agent.Index))
				{
					continue;
				}
				if (!CivilianSpeechRallySlots.TryGetValue(agent.Index, out int slot))
				{
					slot = CivilianSpeechRallySlots.Count;
					CivilianSpeechRallySlots[agent.Index] = slot;
					total = Math.Max(total, CivilianSpeechRallySlots.Count);
				}
				MoveCivilianFollowerNearPlayer(agent, main, slot, total, force);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "MaintainCivilianGatherFollowers failed: " + ex.Message);
		}
	}

	private static void MoveCivilianFollowerNearPlayer(Agent agent, Agent main, int slot, int total, bool force)
	{
		try
		{
			if (agent == null || main == null || !agent.IsActive())
			{
				return;
			}
			Mission mission = Mission.Current ?? agent.Mission;
			Vec3 target = GetCivilianFollowerRallyTarget(mission, main, slot, total);
			float now = mission?.CurrentTime ?? 0f;
			bool firstMovePrepare = CivilianGatherMovePreparedAgentIndexes.Add(agent.Index);
			if (firstMovePrepare)
			{
				NeutralizeCivilianDailyUsableBehavior(agent, "gather_follow_prepare_once");
				ClearCivilianUpperBodyActionForMovement(agent);
			}
			bool targetChanged = !LastCivilianGatherFollowTargets.TryGetValue(agent.Index, out Vec3 lastTarget) || lastTarget.DistanceSquared(target) > 1.44f;
			float distanceToTargetSq = agent.Position.DistanceSquared(target);
			bool nearTarget = distanceToTargetSq <= CivilianSpeechRallySettleTolerance * CivilianSpeechRallySettleTolerance;
			if (!force && !targetChanged && LastCivilianGatherFollowOrderTimes.TryGetValue(agent.Index, out float last) && now - last < CivilianGatherFollowRefreshSeconds)
			{
				if (nearTarget)
				{
					SetCivilianLookTowardAssemblyInterior(agent, main);
				}
				return;
			}
			LastCivilianGatherFollowOrderTimes[agent.Index] = now;
			LastCivilianGatherFollowTargets[agent.Index] = target;
			agent.SetMaximumSpeedLimit(-1f, false);
			ClearCivilianUpperBodyActionForMovement(agent);
			ClearAgentLookTarget(agent);
			agent.SetTargetPosition(target.AsVec2);
			agent.SetWatchState(Agent.WatchState.Patrolling);
			if (nearTarget)
			{
				SetCivilianLookTowardAssemblyInterior(agent, main);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "MoveCivilianFollowerNearPlayer failed: " + ex.Message);
		}
	}

	private static void ClearCivilianUpperBodyActionForMovement(Agent agent)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive())
			{
				return;
			}
			ActionIndexCache current = agent.GetCurrentAction(1);
			if (current == ActionIndexCache.act_scared_idle_1)
			{
				agent.SetActionChannel(1, ActionIndexCache.act_none, true, (AnimFlags)0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			}
		}
		catch
		{
		}
	}

	private static Vec3 GetCivilianFollowerRallyTarget(Mission mission, Agent main, int slot, int total)
	{
		Vec3 forward = main?.LookDirection ?? Vec3.Forward;
		if (forward.LengthSquared < 0.01f)
		{
			forward = Vec3.Forward;
		}
		forward.Normalize();
		Vec3 right = Vec3.CrossProduct(forward, Vec3.Up);
		if (right.LengthSquared < 0.01f)
		{
			right = Vec3.Side;
		}
		right.Normalize();
		int columns = Math.Max(6, (int)Math.Ceiling(Math.Sqrt(Math.Max(1, total))));
		int row = Math.Max(0, slot / columns);
		int col = Math.Max(0, slot % columns);
		float side = (col - (columns - 1) * 0.5f) * 0.95f;
		float forwardDistance = 5.5f + row * 0.9f;
		Vec3 target = (main?.Position ?? Vec3.Zero) + forward * forwardDistance + right * side;
		try
		{
			if (mission?.Scene != null)
			{
				target.z = mission.Scene.GetGroundHeightAtPosition(target);
			}
		}
		catch
		{
		}
		return target;
	}

	private static void QueueCivilianFormationControl(Mission mission, string reason)
	{
		try
		{
			if (_civilianFormationControlComplete)
			{
				return;
			}
			StopCivilianGatherScriptFollowForCommandControl(mission, "queue:" + (reason ?? "N/A"));
			TrySetPlayerFormationFollowOrder(FormationClass.Ranged, "civilian_formation_control_begin");
			float now = mission?.CurrentTime ?? 0f;
			if (!_civilianFormationControlPending)
			{
				_civilianFormationControlPending = true;
				_civilianFormationControlNotBeforeTime = now + CivilianFormationControlInitialDelaySeconds;
				_nextCivilianFormationControlBatchTime = _civilianFormationControlNotBeforeTime;
				RecordInterventionMemory("聚集", "民众召集进入收束阶段，系统正把已跟随的平民转入玩家可调度的民众队列；原因=" + (reason ?? "N/A") + "。");
				Logger.Log("SiegeAiIntervention", "Queued civilian formation control. Reason=" + (reason ?? "N/A"));
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "QueueCivilianFormationControl failed: " + ex.Message);
		}
	}

	private static void StopCivilianGatherScriptFollowForCommandControl(Mission mission, string source)
	{
		try
		{
			_civilianSpeechRallyActive = false;
			_civilianGatherPropagationActive = false;
			ActiveCivilianGatherInteractions.Clear();
			ReturnGatherSoldierMessengersToFormation(mission, source);
			CivilianGatherMessengerAgentIndexes.Clear();
			CivilianGatherMessengerSpeechAgentIndexes.Clear();
			LastCivilianGatherFollowOrderTimes.Clear();
			LastCivilianGatherFollowTargets.Clear();
			foreach (int agentIndex in CivilianGatherFollowerAgentIndexes.ToList())
			{
				Agent agent = mission?.Agents?.FirstOrDefault(a => a != null && a.Index == agentIndex);
				if (!IsEligibleCivilianAgent(agent, includeHeroes: true))
				{
					continue;
				}
				try
				{
					agent.DisableScriptedMovement();
					agent.ClearTargetFrame();
					agent.InvalidateTargetAgent();
					agent.SetMaximumSpeedLimit(-1f, false);
					agent.SetCrouchMode(false);
					agent.SetShouldCatchUpWithFormation(true);
				}
				catch
				{
				}
			}
			Logger.Log("SiegeAiIntervention", "Stopped civilian scripted gather-follow before command control. Source=" + (source ?? "N/A"));
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "StopCivilianGatherScriptFollowForCommandControl failed (" + (source ?? "N/A") + "): " + ex.Message);
		}
	}

	private static void ReturnGatherSoldierMessengersToFormation(Mission mission, string source)
	{
		try
		{
			if (mission?.Agents == null || CivilianGatherMessengerAgentIndexes.Count == 0)
			{
				return;
			}
			int returned = 0;
			foreach (int messengerIndex in CivilianGatherMessengerAgentIndexes.ToList())
			{
				Agent messenger = mission.Agents.FirstOrDefault(a => a != null && a.Index == messengerIndex);
				if (messenger == null || !messenger.IsActive() || !IsInterventionAlliedSoldierForExternal(messenger, requireActive: true))
				{
					continue;
				}
				try
				{
					RestoreAlliedSoldierFriendlyState(messenger, 0f, "gather_messenger_return:" + (source ?? "N/A"), forceFollow: false, clearTarget: true);
					DisableCompanionStyleFollow(messenger);
					AssignAgentToPlayerFormation(messenger, FormationClass.Infantry, refreshFormationOrders: false);
					messenger.DisableScriptedMovement();
					messenger.ClearTargetFrame();
					messenger.InvalidateTargetAgent();
					messenger.SetMaximumSpeedLimit(-1f, false);
					messenger.SetCrouchMode(false);
					messenger.SetShouldCatchUpWithFormation(true);
					messenger.UpdateFormationOrders();
					messenger.SetWatchState(Agent.WatchState.Patrolling);
					CordonReadyAgentIndexes.Remove(messenger.Index);
					returned++;
				}
				catch
				{
				}
			}
			if (returned > 0)
			{
				Logger.Log("SiegeAiIntervention", "Returned gather soldier messengers to formation. Source=" + (source ?? "N/A") + ", Count=" + returned);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ReturnGatherSoldierMessengersToFormation failed (" + (source ?? "N/A") + "): " + ex.Message);
		}
	}

	private static void UpdatePendingCivilianFormationControl(Mission mission)
	{
		try
		{
			if (!_civilianFormationControlPending || _civilianFormationControlComplete || mission?.Agents == null || _massacreStarted || _massacreVictoryReached)
			{
				return;
			}
			if (mission.Mode == MissionMode.Conversation || mission.Mode == MissionMode.Barter)
			{
				return;
			}
			float now = mission.CurrentTime;
			if (now < _civilianFormationControlNotBeforeTime || now < _nextCivilianFormationControlBatchTime)
			{
				return;
			}
			_nextCivilianFormationControlBatchTime = now + CivilianFormationControlBatchIntervalSeconds;
			List<Agent> pending = mission.Agents
				.ToList()
				.Where(a => IsEligibleCivilianAgent(a, includeHeroes: true) && CivilianGatherFollowerAgentIndexes.Contains(a.Index) && !CivilianGatherReadyFormationAgentIndexes.Contains(a.Index))
				.Take(CivilianFormationControlBatchSize)
				.ToList();
			foreach (Agent agent in pending)
			{
				PrepareCivilianForPreMassacreHitDetection(agent, mission);
				NeutralizeCivilianDailyUsableBehavior(agent, "formation_control_batch");
				AssignAgentToPlayerFormation(agent, FormationClass.Ranged, refreshFormationOrders: false);
				CivilianGatherReadyFormationAgentIndexes.Add(agent.Index);
				try
				{
					agent.DisableScriptedMovement();
					agent.ClearTargetFrame();
					agent.InvalidateTargetAgent();
					agent.SetMaximumSpeedLimit(-1f, false);
					agent.SetCrouchMode(false);
					agent.SetShouldCatchUpWithFormation(true);
				}
				catch
				{
				}
				agent.SetWatchState(Agent.WatchState.Patrolling);
				ApplyOneTimeFrightenedCivilianAction(agent, allowGathered: true);
			}
			bool allAssigned = !mission.Agents.ToList().Any(a => IsEligibleCivilianAgent(a, includeHeroes: true) && CivilianGatherFollowerAgentIndexes.Contains(a.Index) && !CivilianGatherReadyFormationAgentIndexes.Contains(a.Index));
			if (!allAssigned)
			{
				return;
			}
			_civilianFormationControlPending = false;
			_civilianFormationControlComplete = true;
			ApplyCivilianFormationFollowOrder(mission, "civilian_formation_ready_follow");
			RecordInterventionMemory("聚集", "民众已经完成聚集并编入玩家可调度的民众队列，后续NPC应知道民众已到场听命。");
			if (!_civilianFormationControlMessageShown)
			{
				_civilianFormationControlMessageShown = true;
				InformationManager.DisplayMessage(new InformationMessage("【攻城处置】民众已经聚拢听命，你现在可以像战场上调度队列一样让他们列阵。", Color.FromUint(0xFFB6F7A8u)));
			}
			if (!_civilianOrderControllerPrimed)
			{
				_civilianOrderControllerPrimed = TryPrimePlayerOrderController(mission, "civilian_formation_ready", force: true);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "UpdatePendingCivilianFormationControl failed: " + ex.Message);
		}
	}

	private static bool ApplyCivilianFormationFollowOrder(Mission mission, string source)
	{
		bool orderIssued = TrySetPlayerFormationFollowOrder(FormationClass.Ranged, source);
		try
		{
			if (mission?.Agents == null)
			{
				return orderIssued;
			}
			foreach (int agentIndex in CivilianGatherReadyFormationAgentIndexes.ToList())
			{
				Agent agent = mission.Agents.FirstOrDefault(a => a != null && a.Index == agentIndex);
				if (!IsEligibleCivilianAgent(agent, includeHeroes: true))
				{
					continue;
				}
				try
				{
					AssignAgentToPlayerFormation(agent, FormationClass.Ranged, refreshFormationOrders: false);
					agent.DisableScriptedMovement();
					agent.ClearTargetFrame();
					agent.InvalidateTargetAgent();
					agent.SetMaximumSpeedLimit(-1f, false);
					agent.SetCrouchMode(false);
					agent.SetShouldCatchUpWithFormation(true);
					agent.UpdateFormationOrders();
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyCivilianFormationFollowOrder failed (" + (source ?? "N/A") + "): " + ex.Message);
		}
		return orderIssued;
	}

	private static void EnsureCivilianAssemblyPopulation(Mission mission)
	{
		try
		{
			if (_civilianAssemblySpawnAttempted || mission?.Agents == null)
			{
				return;
			}
			_civilianAssemblySpawnAttempted = true;
			if (!EnableExtraCivilianAssemblySpawns)
			{
				int existingOnly = mission.Agents.Count(a => IsEligibleCivilianAgent(a, includeHeroes: true));
				Logger.Log("SiegeAiIntervention", "Extra civilian assembly spawn disabled; using native settlement civilians only. Existing=" + existingOnly);
				return;
			}
			int existing = mission.Agents.Count(a => IsEligibleCivilianAgent(a, includeHeroes: true));
			int desiredCount = GetDesiredCivilianAssemblyCount(mission);
			int missing = Math.Max(0, desiredCount - existing);
			if (missing <= 0)
			{
				Logger.Log("SiegeAiIntervention", "Civilian assembly population already sufficient. Existing=" + existing + ", Desired=" + desiredCount);
				return;
			}
			List<CharacterObject> templates = PickAssemblyCivilianTemplates(missing);
			if (templates.Count == 0)
			{
				Logger.Log("SiegeAiIntervention", "Civilian assembly spawn skipped: no safe civilian templates found. Existing=" + existing + ", Missing=" + missing);
				return;
			}
			int spawned = 0;
			for (int i = 0; i < missing; i++)
			{
				CharacterObject template = templates[i % templates.Count];
				int slot = _civilianAssemblyNextSlot + i;
				Vec3 position = GetCivilianTownSpawnPosition(mission, slot);
				Agent agent = SpawnAssemblyCivilian(mission, template, position);
				if (agent == null)
				{
					continue;
				}
				spawned++;
				_spawnedAssemblyCivilianCount++;
				SceneCivilianAgentIndexes.Add(agent.Index);
				CivilianAssemblySlots[agent.Index] = slot;
				PrepareCivilianForPreMassacreHitDetection(agent, mission);
			}
			_civilianAssemblyNextSlot += missing;
			Logger.Log("SiegeAiIntervention", "Civilian assembly populated. Existing=" + existing + ", Spawned=" + spawned + ", Desired=" + desiredCount + ", Templates=" + templates.Count);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnsureCivilianAssemblyPopulation failed: " + ex.Message);
		}
	}

	private static int GetDesiredCivilianAssemblyCount(Mission mission = null)
	{
		if (_desiredCivilianAssemblyCount <= 0)
		{
			try
			{
				_desiredCivilianAssemblyCount = MBRandom.RandomInt(MinDesiredCivilianAssemblyCount, MaxDesiredCivilianAssemblyCount + 1);
			}
			catch
			{
				_desiredCivilianAssemblyCount = (MinDesiredCivilianAssemblyCount + MaxDesiredCivilianAssemblyCount) / 2;
			}
		}
		int desired = Math.Max(MinDesiredCivilianAssemblyCount, Math.Min(MaxDesiredCivilianAssemblyCount, _desiredCivilianAssemblyCount));
		if (mission == null)
		{
			return desired;
		}
		return Math.Max(0, Math.Min(desired, GetCivilianSceneCivilianCap(mission)));
	}

	private static int GetCivilianSceneCivilianCap(Mission mission)
	{
		try
		{
			if (mission?.Agents == null)
			{
				return TownCivilianAssemblySceneCap;
			}
			Settlement settlement = ResolveCurrentSettlement();
			int settlementCap = settlement?.IsCastle == true ? CastleCivilianAssemblySceneCap : TownCivilianAssemblySceneCap;
			int nativeCivilianCount = mission.Agents.Count(a => IsEligibleCivilianAgent(a, includeHeroes: true) && !CivilianAssemblySlots.ContainsKey(a.Index));
			int nonCivilianActiveCount = mission.Agents.Count(a => a != null && a.IsActive() && !IsEligibleCivilianAgent(a, includeHeroes: true));
			int nativeScaledCap = nativeCivilianCount > 0 ? nativeCivilianCount + CivilianAssemblySmallSceneExtraCap : settlementCap;
			int totalAgentRoomCap = Math.Max(MinimumCivilianAssemblySceneCap, SceneTotalAgentSoftCap - nonCivilianActiveCount);
			int cap = Math.Min(settlementCap, Math.Min(nativeScaledCap, totalAgentRoomCap));
			return Math.Max(MinimumCivilianAssemblySceneCap, cap);
		}
		catch
		{
			return TownCivilianAssemblySceneCap;
		}
	}

	private static Vec3 GetCivilianTownSpawnPosition(Mission mission, int slot)
	{
		Agent main = Agent.Main ?? mission?.MainAgent;
		Vec3 fallback = main?.Position ?? Vec3.Zero;
		try
		{
			List<Agent> anchors = mission?.Agents?
				.Where(a => a != null && a.IsActive() && IsEligibleCivilianAgent(a, includeHeroes: false) && !SceneCivilianAgentIndexes.Contains(a.Index))
				.OrderBy(a => a.Index)
				.ToList() ?? new List<Agent>();
			if (anchors.Count == 0)
			{
				anchors = mission?.Agents?
					.Where(a => a != null && a.IsActive() && IsEligibleCivilianAgent(a, includeHeroes: false))
					.OrderBy(a => a.Index)
					.ToList() ?? new List<Agent>();
			}
			Vec3 anchor = fallback;
			if (anchors.Count > 0)
			{
				anchor = anchors[Math.Abs(slot) % anchors.Count].Position;
			}
			else if (main != null)
			{
				Vec2 look = main.LookDirection.AsVec2;
				if (look.LengthSquared < 0.001f)
				{
					look = Vec2.Forward;
				}
				look.Normalize();
				Vec2 side = new Vec2(-look.y, look.x);
				float ring = 18f + (Math.Abs(slot) % 9) * 3.5f;
				float lateral = ((slot % 2 == 0) ? 1f : -1f) * (6f + (Math.Abs(slot) % 7) * 2.4f);
				anchor = main.Position + look.ToVec3() * ring + side.ToVec3() * lateral;
			}
			float angle = (MathF.PI * 2f * (Math.Abs(slot) % 17)) / 17f;
			float radius = 1.2f + (Math.Abs(slot * 37) % 7) * 0.55f;
			Vec3 position = anchor + new Vec3(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius, 0f);
			try
			{
				if (mission?.Scene != null)
				{
					position.z = mission.Scene.GetGroundHeightAtPosition(position, BodyFlags.CommonCollisionExcludeFlags);
				}
			}
			catch
			{
				try
				{
					if (mission?.Scene != null)
					{
						position.z = mission.Scene.GetGroundHeightAtPosition(position);
					}
				}
				catch
				{
				}
			}
			return position;
		}
		catch
		{
			return fallback;
		}
	}

	private static List<CharacterObject> PickAssemblyCivilianTemplates(int count)
	{
		List<CharacterObject> result = new List<CharacterObject>();
		try
		{
			CultureObject culture = ResolveCurrentSettlement()?.Culture ?? Hero.MainHero?.Culture;
			IEnumerable<CharacterObject> all = CharacterObject.All ?? Enumerable.Empty<CharacterObject>();
			List<CharacterObject> preferred = all.Where(c => IsSafeAssemblyCivilianTemplate(c, strictOccupation: true) && (culture == null || c.Culture == culture)).OrderBy(c => c.StringId ?? "").ToList();
			foreach (CharacterObject character in preferred)
			{
				if (character == null || result.Contains(character))
				{
					continue;
				}
				result.Add(character);
				if (result.Count >= Math.Max(8, Math.Min(count, 32)))
				{
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "PickAssemblyCivilianTemplates failed: " + ex.Message);
		}
		return result;
	}

	private static bool IsSafeAssemblyCivilianTemplate(CharacterObject character, bool strictOccupation)
	{
		if (character == null || character.IsHero || character == CharacterObject.PlayerCharacter || character.HeroObject != null || IsBackstreetOrCriminalCharacter(character) || IsProtectedChildCharacter(character) || IsProtectedNotableCharacter(character) || IsGuardOrSoldier(character) || IsUnsafeAssemblyCivilianTemplate(character))
		{
			return false;
		}
		if (character.Race < 0)
		{
			return false;
		}
		if (strictOccupation)
		{
			switch (character.Occupation)
			{
			case Occupation.Townsfolk:
			case Occupation.Villager:
			case Occupation.GoodsTrader:
			case Occupation.Artisan:
			case Occupation.Merchant:
			case Occupation.Weaponsmith:
			case Occupation.Armorer:
			case Occupation.HorseTrader:
			case Occupation.ShopWorker:
			case Occupation.Blacksmith:
			case Occupation.Tavernkeeper:
			case Occupation.TavernWench:
			case Occupation.TavernGameHost:
			case Occupation.Musician:
			case Occupation.Preacher:
			case Occupation.RansomBroker:
			case Occupation.ShipWright:
				return true;
			default:
				return false;
			}
		}
		return IsCivilianForIntervention(character);
	}

	private static bool IsUnsafeAssemblyCivilianTemplate(CharacterObject character)
	{
		if (character == null)
		{
			return true;
		}
		if (IsKnownUnsafeAssemblyCivilianTemplateName(character))
		{
			return true;
		}
		return !HasUsableCivilianBodyEquipment(character);
	}

	private static bool IsKnownUnsafeAssemblyCivilianTemplateName(CharacterObject character)
	{
		try
		{
			string text = ((character.StringId ?? "") + " " + (character.Name?.ToString() ?? "")).ToLowerInvariant();
			string compact = text.Replace("_", "").Replace("-", "").Replace(" ", "");
			return ContainsAny(text, "crazy man", "crazy_man", "crazy-man") || compact.Contains("crazyman") || (text.Contains("crazy") && text.Contains("t7"));
		}
		catch
		{
			return false;
		}
	}

	private static bool HasUsableCivilianBodyEquipment(CharacterObject character)
	{
		try
		{
			if (character == null)
			{
				return false;
			}
			IEnumerable<Equipment> equipments = character.CivilianEquipments ?? Enumerable.Empty<Equipment>();
			foreach (Equipment equipment in equipments)
			{
				if (EquipmentHasBodyClothing(equipment))
				{
					return true;
				}
			}
			return EquipmentHasBodyClothing(character.FirstCivilianEquipment) || EquipmentHasBodyClothing(character.RandomCivilianEquipment);
		}
		catch
		{
			return false;
		}
	}

	private static bool EquipmentHasBodyClothing(Equipment equipment)
	{
		try
		{
			return equipment != null && !equipment.IsEmpty() && equipment[EquipmentIndex.Body].Item != null;
		}
		catch
		{
			return false;
		}
	}

	private static Agent SpawnAssemblyCivilian(Mission mission, CharacterObject character, Vec3 position)
	{
		try
		{
			if (mission == null || character == null)
			{
				return null;
			}
			Vec3 direction = -_civilianAssemblyForward;
			if (direction.LengthSquared < 0.01f)
			{
				direction = -Vec3.Forward;
			}
			direction.Normalize();
			AgentBuildData buildData = new AgentBuildData(character)
				.TroopOrigin(new SimpleAgentOrigin(character, -1, null, default(UniqueTroopDescriptor)))
				.Monster(TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(character.Race, "_settlement"))
				.Team(Team.Invalid)
				.InitialPosition(in position)
				.InitialDirection(direction.AsVec2.Normalized())
				.Controller(AgentControllerType.AI)
				.CivilianEquipment(civilianEquipment: true)
				.NoWeapons(noWeapons: true)
				.NoHorses(noHorses: true);
			Agent agent = mission.SpawnAgent(buildData, false);
			if (agent != null)
			{
				agent.SetMortalityState(Agent.MortalityState.Mortal);
				agent.SetWatchState(Agent.WatchState.Patrolling);
				PrepareCivilianForPreMassacreHitDetection(agent, mission);
			}
			return agent;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "SpawnAssemblyCivilian failed: " + (character?.StringId ?? "N/A") + ", Error=" + ex.Message);
			return null;
		}
	}

	private static bool AssignAgentToPlayerFormation(Agent agent, FormationClass formationClass, bool refreshFormationOrders = true)
	{
		try
		{
			Mission mission = Mission.Current ?? agent?.Mission;
			Agent main = Agent.Main ?? mission?.MainAgent;
			Team playerTeam = mission?.PlayerTeam ?? main?.Team;
			if (agent == null || !agent.IsHuman || !agent.IsActive() || playerTeam == null)
			{
				return false;
			}
			if (agent.Team != playerTeam)
			{
				agent.SetTeam(playerTeam, true);
			}
			EnsureAgentUnderPlayerCommand(agent);
			Formation formation = playerTeam.GetFormation(formationClass);
			MarkFormationPlayerCommandable(formation, main);
			if (formation != null && agent.Formation != formation)
			{
				agent.Formation = formation;
			}
			if (formation != null)
			{
				agent.TryAttachToFormation();
				agent.SetShouldCatchUpWithFormation(true);
				if (refreshFormationOrders)
				{
					agent.UpdateFormationOrders();
				}
			}
			return formation != null;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "AssignAgentToPlayerFormation failed: " + ex.Message);
			return false;
		}
	}

	private static void MarkFormationPlayerCommandable(Formation formation, Agent playerOwner)
	{
		try
		{
			if (formation == null)
			{
				return;
			}
			try
			{
				formation.SetControlledByAI(false, false);
			}
			catch
			{
				TrySetFormationProperty(formation, nameof(Formation.IsAIControlled), false);
			}
			TrySetFormationProperty(formation, nameof(Formation.HasPlayerControlledTroop), true);
			if (playerOwner != null && playerOwner.IsActive())
			{
				TrySetFormationProperty(formation, nameof(Formation.PlayerOwner), playerOwner);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "MarkFormationPlayerCommandable failed: " + ex.Message);
		}
	}

	private static void TrySetFormationProperty(Formation formation, string propertyName, object value)
	{
		try
		{
			PropertyInfo property = formation?.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null)
			{
				return;
			}
			MethodInfo setter = property.GetSetMethod(true);
			if (setter != null)
			{
				setter.Invoke(formation, new object[] { value });
			}
		}
		catch
		{
		}
	}

	private static void EnsureAgentUnderPlayerCommand(Agent agent)
	{
		try
		{
			if (agent == null || !agent.IsHuman || agent == Agent.Main)
			{
				return;
			}
			MarkAgentOriginUnderPlayerCommand(agent);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnsureAgentUnderPlayerCommand failed: " + ex.Message);
		}
	}

	private static void MarkAgentOriginUnderPlayerCommand(Agent agent)
	{
		try
		{
			IAgentOriginBase origin = agent?.Origin;
			if (origin == null)
			{
				return;
			}
			CommandableOriginRuntimeIds.Add(RuntimeHelpers.GetHashCode(origin));
		}
		catch
		{
		}
	}

	internal static bool ShouldForceCommandForOrigin(object origin)
	{
		try
		{
			if (origin == null || !IsActiveInCurrentMission())
			{
				return false;
			}
			return CommandableOriginRuntimeIds.Contains(RuntimeHelpers.GetHashCode(origin));
		}
		catch
		{
			return false;
		}
	}

	private static bool TrySetPlayerFormationFollowOrder(FormationClass formationClass, string source)
	{
		try
		{
			Mission mission = Mission.Current;
			Agent main = Agent.Main ?? mission?.MainAgent;
			Team playerTeam = mission?.PlayerTeam ?? main?.Team;
			if (mission == null || main == null || !main.IsActive() || playerTeam == null)
			{
				return false;
			}
			Formation formation = playerTeam.GetFormation(formationClass);
			if (formation == null)
			{
				return false;
			}
			MarkFormationPlayerCommandable(formation, main);
			try
			{
				formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			}
			catch
			{
			}
			formation.SetMovementOrder(MovementOrder.MovementOrderFollow(main));
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TrySetPlayerFormationFollowOrder failed (" + (source ?? "N/A") + "): " + ex.Message);
			return false;
		}
	}

	private static bool IsOffensiveInterventionOrder(OrderType orderType)
	{
		return orderType == OrderType.Charge
			|| orderType == OrderType.ChargeWithTarget
			|| orderType == OrderType.Advance
			|| orderType == OrderType.AdvanceTenPaces
			|| orderType == OrderType.AttackEntity
			|| orderType == OrderType.PointDefence
			|| orderType == OrderType.FireAtWill
			|| orderType == OrderType.AIControlOn;
	}

	internal static MBReadOnlyList<VisualOrderSet> FilterInterventionNativeVisualOrdersForExternal(MBReadOnlyList<VisualOrderSet> originalOrders)
	{
		try
		{
			if (originalOrders == null)
			{
				return originalOrders;
			}
			MBList<VisualOrderSet> filteredSets = new MBList<VisualOrderSet>();
			foreach (VisualOrderSet set in originalOrders)
			{
				if (set == null)
				{
					continue;
				}
				if (set.IsSoloOrder)
				{
					VisualOrder solo = set.SoloOrder ?? set.Orders?.FirstOrDefault();
					if (IsAllowedInterventionVisualOrder(solo))
					{
						filteredSets.Add(set);
					}
					continue;
				}
				foreach (VisualOrder order in (set.Orders?.ToList() ?? new List<VisualOrder>()))
				{
					if (!IsAllowedInterventionVisualOrder(order))
					{
						try
						{
							set.RemoveOrder(order);
						}
						catch
						{
						}
					}
				}
				if ((set.Orders?.Count ?? 0) > 0)
				{
					filteredSets.Add(set);
				}
			}
			return filteredSets;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "FilterInterventionNativeVisualOrdersForExternal failed: " + ex.Message);
			return originalOrders;
		}
	}

	private static bool IsAllowedInterventionVisualOrder(VisualOrder order)
	{
		try
		{
			if (order == null)
			{
				return false;
			}
			string id = order.StringId ?? "";
			if (id.IndexOf("charge", StringComparison.OrdinalIgnoreCase) >= 0
				|| id.IndexOf("advance", StringComparison.OrdinalIgnoreCase) >= 0
				|| id.IndexOf("attack", StringComparison.OrdinalIgnoreCase) >= 0
				|| id.IndexOf("fire", StringComparison.OrdinalIgnoreCase) >= 0
				|| id.IndexOf("ai", StringComparison.OrdinalIgnoreCase) >= 0
				|| id.IndexOf("delegate", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return false;
			}
			if (order is ChargeVisualOrder || order is AdvanceVisualOrder)
			{
				return false;
			}
			if (order is GenericToggleVisualOrder toggle)
			{
				return !IsOffensiveInterventionOrder(toggle.PositiveOrder) && !IsOffensiveInterventionOrder(toggle.NegativeOrder);
			}
			if (SingleVisualOrderOrderTypeField != null && order is SingleVisualOrder)
			{
				object value = SingleVisualOrderOrderTypeField.GetValue(order);
				if (value is OrderType orderType && IsOffensiveInterventionOrder(orderType))
				{
					return false;
				}
			}
			return true;
		}
		catch
		{
			return true;
		}
	}

	private static bool TryPrimePlayerOrderController(Mission mission, string source, bool force, bool preserveSelection = false)
	{
		try
		{
			if (mission == null || mission.Mode == MissionMode.Conversation || mission.Mode == MissionMode.Barter)
			{
				return false;
			}
			float now = mission.CurrentTime;
			if (!force && _playerOrderControllerPrimed)
			{
				return false;
			}
			if (!force && now < _nextPlayerOrderControllerPrimeTime)
			{
				return false;
			}
			_nextPlayerOrderControllerPrimeTime = now + 2.0f;
			Team playerTeam = mission.PlayerTeam ?? _interventionPlayerCommandTeam ?? Agent.Main?.Team;
			if (playerTeam == null)
			{
				return false;
			}
			int commandable = 0;
			foreach (Agent agent in mission.Agents.ToList())
			{
				if (agent != null && agent.IsHuman && agent.IsActive() && agent != Agent.Main && agent.Team == playerTeam && agent.Formation != null)
				{
					commandable++;
				}
			}
			if (commandable <= 0)
			{
				return false;
			}
			Agent main = Agent.Main ?? mission.MainAgent;
			List<Formation> commandFormations = mission.Agents
				.ToList()
				.Where(a => a != null && a.IsHuman && a.IsActive() && a != Agent.Main && a.Team == playerTeam && a.Formation != null)
				.Select(a => a.Formation)
				.Distinct()
				.ToList();
			bool hasExistingSelection = false;
			try
			{
				hasExistingSelection = playerTeam.PlayerOrderController?.SelectedFormations != null && playerTeam.PlayerOrderController.SelectedFormations.Count > 0;
			}
			catch
			{
			}
			bool shouldInitializeSelection = !preserveSelection && !hasExistingSelection;
			foreach (Formation formation in commandFormations)
			{
				MarkFormationPlayerCommandable(formation, main);
				if (shouldInitializeSelection)
				{
					try
					{
						if (playerTeam.PlayerOrderController != null)
						{
							playerTeam.PlayerOrderController.SelectFormation(formation);
						}
					}
					catch
					{
					}
				}
			}
			if (playerTeam.PlayerOrderController == null)
			{
				Logger.Log("SiegeAiIntervention", "Native player order controller not ready yet. Source=" + (source ?? "N/A") + ", Commandable=" + commandable + ", Formations=" + commandFormations.Count);
				return false;
			}
			if (shouldInitializeSelection)
			{
				try
				{
					playerTeam.PlayerOrderController?.SelectAllFormations(false);
				}
				catch
				{
				}
				try
				{
					playerTeam.MasterOrderController?.SelectAllFormations(false);
				}
				catch
				{
				}
			}
			_playerOrderControllerPrimed = true;
			Logger.Log("SiegeAiIntervention", "Primed player order controller. Source=" + (source ?? "N/A") + ", Commandable=" + commandable + ", Formations=" + commandFormations.Count + ", PreserveSelection=" + preserveSelection + ", ExistingSelection=" + hasExistingSelection + ", InitializedSelection=" + shouldInitializeSelection);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryPrimePlayerOrderController failed (" + (source ?? "N/A") + "): " + ex.Message);
			return false;
		}
	}

	internal static Team ResolveInterventionPlayerCommandTeamForExternal(Mission mission, string source = null)
	{
		try
		{
			mission ??= Mission.Current;
			if (mission == null || mission.IsMissionEnding)
			{
				return null;
			}
			EnsureInterventionPlayerCommandTeam(mission);
			Team playerTeam = mission.PlayerTeam ?? _interventionPlayerCommandTeam ?? Agent.Main?.Team ?? mission.MainAgent?.Team;
			if (playerTeam != null)
			{
				_interventionPlayerCommandTeam = playerTeam;
			}
			return playerTeam;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ResolveInterventionPlayerCommandTeamForExternal failed (" + (source ?? "N/A") + "): " + ex.Message);
			return null;
		}
	}

	internal static bool EnsureInterventionCommandUiReadyForExternal(Mission mission, string source)
	{
		try
		{
			mission ??= Mission.Current;
			if (mission == null || mission.IsMissionEnding || mission.Mode == MissionMode.Conversation || mission.Mode == MissionMode.Barter)
			{
				return false;
			}
			Team playerTeam = ResolveInterventionPlayerCommandTeamForExternal(mission, source);
			if (playerTeam == null)
			{
				return false;
			}
			Agent main = Agent.Main ?? mission.MainAgent;
			int commandable = 0;
			foreach (Agent agent in mission.Agents?.ToList() ?? new List<Agent>())
			{
				if (agent == null || !agent.IsHuman || !agent.IsActive() || agent == main || agent.Team != playerTeam || agent.Formation == null)
				{
					continue;
				}
				MarkFormationPlayerCommandable(agent.Formation, main);
				commandable++;
			}
			bool frequentOrderUiPoll = string.Equals(source, "mission_order_vm_check_open", StringComparison.Ordinal)
				|| string.Equals(source, "mission_order_vm_has_troops", StringComparison.Ordinal)
				|| string.Equals(source, "mission_order_vm_controller", StringComparison.Ordinal);
			TryPrimePlayerOrderController(mission, source ?? "order_ui_ready", force: !frequentOrderUiPoll, preserveSelection: true);
			return commandable > 0 && TryResolveNativeOrderControllerForExternal(mission) != null;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnsureInterventionCommandUiReadyForExternal failed (" + (source ?? "N/A") + "): " + ex.Message);
			return false;
		}
	}

	internal static bool InterventionPlayerHasCommandableAgentsForExternal(Mission mission)
	{
		try
		{
			mission ??= Mission.Current;
			Team playerTeam = ResolveInterventionPlayerCommandTeamForExternal(mission, "has_commandable_agents");
			Agent main = Agent.Main ?? mission?.MainAgent;
			return mission?.Agents != null && playerTeam != null && mission.Agents.Any(a => a != null && a.IsHuman && a.IsActive() && a != main && a.Team == playerTeam && a.Formation != null);
		}
		catch
		{
			return false;
		}
	}

	private static void NotifyAgentBuiltForMission(Agent agent, Mission mission)
	{
		try
		{
			if (agent == null || mission == null)
			{
				return;
			}
			BattleAgentLogic battleAgentLogic = mission.GetMissionBehavior<BattleAgentLogic>();
			MethodInfo onAgentBuild = battleAgentLogic?.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.FirstOrDefault(m => m.Name == "OnAgentBuild" && m.GetParameters().Length == 2);
			onAgentBuild?.Invoke(battleAgentLogic, new object[] { agent, null });
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "NotifyAgentBuiltForMission failed: " + ex.Message);
		}
	}

	internal static bool ShouldInjectInterventionOrderViewsForExternal(Mission mission)
	{
		try
		{
			return mission != null && !mission.IsMissionEnding && (_activeMode != InterventionMode.None || _pendingMode != InterventionMode.None);
		}
		catch
		{
			return false;
		}
	}

	internal static bool IsNativeOrderControllerReadyForExternal(Mission mission)
	{
		try
		{
			if (mission == null || mission.IsMissionEnding || mission.Mode == MissionMode.Conversation || mission.Mode == MissionMode.Barter)
			{
				return false;
			}
			OrderController orderController = TryResolveNativeOrderControllerForExternal(mission);
			if (orderController == null)
			{
				return false;
			}
			Team playerTeam = mission.PlayerTeam ?? _interventionPlayerCommandTeam ?? Agent.Main?.Team ?? mission.MainAgent?.Team;
			return mission.Agents != null && mission.Agents.Any(a => a != null && a.IsHuman && a.IsActive() && a != Agent.Main && a.Team == playerTeam && a.Formation != null);
		}
		catch
		{
			return false;
		}
	}

	internal static OrderController TryResolveNativeOrderControllerForExternal(Mission mission)
	{
		try
		{
			mission ??= Mission.Current;
			if (mission == null || mission.IsMissionEnding || mission.Mode == MissionMode.Conversation || mission.Mode == MissionMode.Barter)
			{
				return null;
			}
			if (mission.PlayerTeam == null || mission.PlayerTeam.PlayerOrderController == null)
			{
				EnsureInterventionPlayerCommandTeam(mission);
			}
			Team playerTeam = ResolveInterventionPlayerCommandTeamForExternal(mission, "resolve_order_controller") ?? mission.PlayerTeam ?? _interventionPlayerCommandTeam ?? Agent.Main?.Team ?? mission.MainAgent?.Team;
			return playerTeam?.PlayerOrderController ?? playerTeam?.MasterOrderController;
		}
		catch
		{
			return null;
		}
	}

	internal static bool TryBindNativeOrderControllerForExternal(OrderTroopPlacer orderTroopPlacer, string source)
	{
		try
		{
			if (orderTroopPlacer == null || !IsOccupationSceneActiveForExternal())
			{
				return false;
			}
			OrderController orderController = TryResolveNativeOrderControllerForExternal(orderTroopPlacer.Mission ?? Mission.Current);
			if (orderController == null || OrderTroopPlacerOrderControllerField == null)
			{
				return false;
			}
			object current = OrderTroopPlacerOrderControllerField.GetValue(orderTroopPlacer);
			if (!ReferenceEquals(current, orderController))
			{
				OrderTroopPlacerOrderControllerField.SetValue(orderTroopPlacer, orderController);
				Logger.Log("SiegeAiIntervention", "Bound native OrderTroopPlacer order controller. Source=" + (source ?? "N/A"));
			}
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryBindNativeOrderControllerForExternal failed (" + (source ?? "N/A") + "): " + ex.Message);
			return false;
		}
	}

	internal static bool NativeOrderControllerHasSelectedFormationsForExternal(Mission mission)
	{
		try
		{
			OrderController orderController = TryResolveNativeOrderControllerForExternal(mission);
			return orderController?.SelectedFormations != null && orderController.SelectedFormations.Count > 0;
		}
		catch
		{
			return false;
		}
	}

	private static void NeutralizeCivilianDailyUsableBehavior(Agent agent, string reason)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive())
			{
				return;
			}
			try
			{
				agent.InvalidateTargetAgent();
				agent.DisableScriptedMovement();
				agent.ClearTargetFrame();
				agent.SetIsAIPaused(false);
			}
			catch
			{
			}
			TrySetAgentController(agent, "AI");
			CampaignAgentComponent component = agent.GetComponent<CampaignAgentComponent>();
			AgentNavigator navigator = component?.AgentNavigator;
			if (navigator == null)
			{
				return;
			}
			try
			{
				navigator.ClearTarget();
			}
			catch
			{
			}
			DailyBehaviorGroup dailyGroup = navigator.GetBehaviorGroup<DailyBehaviorGroup>();
			if (dailyGroup == null)
			{
				return;
			}
			try
			{
				dailyGroup.DisableScriptedBehavior();
			}
			catch
			{
			}
			try
			{
				WalkingBehavior walkingBehavior = dailyGroup.GetBehavior<WalkingBehavior>();
				if (walkingBehavior != null)
				{
					walkingBehavior.IsActive = false;
				}
			}
			catch
			{
			}
			try
			{
				ScriptBehavior scriptBehavior = dailyGroup.GetBehavior<ScriptBehavior>();
				if (scriptBehavior != null)
				{
					scriptBehavior.IsActive = false;
				}
			}
			catch
			{
			}
			try
			{
				FollowAgentBehavior followBehavior = dailyGroup.GetBehavior<FollowAgentBehavior>();
				if (followBehavior != null)
				{
					followBehavior.IsActive = false;
				}
			}
			catch
			{
			}
			try
			{
				dailyGroup.DisableAllBehaviors();
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "NeutralizeCivilianDailyUsableBehavior failed (" + (reason ?? "N/A") + "): " + ex.Message);
		}
	}

	private static void TrySetAgentController(Agent agent, string controllerType)
	{
		try
		{
			if (agent == null || string.IsNullOrWhiteSpace(controllerType))
			{
				return;
			}
			PropertyInfo propertyInfo = agent.GetType().GetProperty("Controller") ?? agent.GetType().GetProperty("ControllerType");
			if (propertyInfo == null || !propertyInfo.CanWrite)
			{
				return;
			}
			object value = Enum.Parse(propertyInfo.PropertyType, controllerType, ignoreCase: true);
			if (value != null)
			{
				propertyInfo.SetValue(agent, value);
			}
		}
		catch
		{
		}
	}

	private static void PrepareCivilianForPreMassacreHitDetection(Agent agent, Mission mission)
	{
		try
		{
			if (_massacreStarted || agent == null || mission == null || !IsMassacreTargetAgent(agent, includeHeroes: true))
			{
				return;
			}
			bool firstPrepare = CivilianPreMassacrePreparedAgentIndexes.Add(agent.Index);
			if (firstPrepare)
			{
				agent.SetMortalityState(Agent.MortalityState.Mortal);
				agent.InvalidateTargetAgent();
				agent.SetWatchState(Agent.WatchState.Patrolling);
			}
			Team playerTeam = mission.PlayerTeam ?? mission.MainAgent?.Team ?? Agent.Main?.Team;
			if (playerTeam != null && agent.Team != playerTeam)
			{
				agent.SetTeam(playerTeam, true);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "PrepareCivilianForPreMassacreHitDetection failed: " + ex.Message);
		}
	}

	private static Vec3 GetCivilianAssemblyCenter(Mission mission)
	{
		Vec3 forward = _civilianAssemblyForward;
		if (forward.LengthSquared < 0.01f)
		{
			forward = Vec3.Forward;
		}
		forward.Normalize();
		int totalSlots = Math.Max(GetDesiredCivilianAssemblyCount(mission), Math.Max(_civilianAssemblyNextSlot, _spawnedAssemblyCivilianCount));
		int rows = Math.Max(1, (int)Math.Ceiling(totalSlots / (double)Math.Max(1, CivilianAssemblyColumns)));
		float depth = Math.Max(0f, (rows - 1) * CivilianAssemblyRowSpacing);
		Vec3 center = _civilianAssemblyAnchor + forward * (depth * 0.5f);
		try
		{
			if (mission?.Scene != null)
			{
				center.z = mission.Scene.GetGroundHeightAtPosition(center);
			}
		}
		catch
		{
		}
		return center;
	}

	private static float GetCivilianAssemblyCordonRadius(Mission mission)
	{
		int totalSlots = Math.Max(GetDesiredCivilianAssemblyCount(mission), Math.Max(_civilianAssemblyNextSlot, _spawnedAssemblyCivilianCount));
		int columns = Math.Max(1, CivilianAssemblyColumns);
		int rows = Math.Max(1, (int)Math.Ceiling(totalSlots / (double)columns));
		float halfWidth = Math.Max(0f, (columns - 1) * CivilianAssemblyColumnSpacing * 0.5f);
		float halfDepth = Math.Max(0f, (rows - 1) * CivilianAssemblyRowSpacing * 0.5f);
		return Math.Max(SoldierCordonMinRadius, Math.Max(halfWidth, halfDepth) + SoldierCordonPadding);
	}

	private static Vec3 GetAlliedCordonSlotPosition(Mission mission, int slot, int count)
	{
		Vec3 center = GetCivilianAssemblyCenter(mission);
		Vec3 forward = _civilianAssemblyForward;
		if (forward.LengthSquared < 0.01f)
		{
			forward = Vec3.Forward;
		}
		forward.Normalize();
		Vec3 right = Vec3.CrossProduct(forward, Vec3.Up);
		if (right.LengthSquared < 0.01f)
		{
			right = Vec3.Side;
		}
		right.Normalize();
		int safeCount = Math.Max(1, count);
		float angle = (MathF.PI * 2f * Math.Max(0, slot)) / safeCount;
		float radius = GetCivilianAssemblyCordonRadius(mission);
		Vec3 position = center + right * (MathF.Cos(angle) * radius) + forward * (MathF.Sin(angle) * radius);
		try
		{
			if (mission?.Scene != null)
			{
				position.z = mission.Scene.GetGroundHeightAtPosition(position);
			}
		}
		catch
		{
		}
		return position;
	}

	private static void TryApplyNativeCircleFormationOrders(List<Agent> soldiers, Mission mission, string source)
	{
		try
		{
			if (soldiers == null || soldiers.Count == 0 || mission == null || !_civilianAssemblyPointReady)
			{
				return;
			}
			Vec3 center = GetCivilianAssemblyCenter(mission);
			float diameter = GetCivilianAssemblyCordonRadius(mission) * 2f;
			foreach (Formation formation in soldiers.Select(a => a?.Formation).Where(f => f != null).Distinct().ToList())
			{
				try
				{
					formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderCircle);
					formation.SetFormOrder(FormOrder.FormOrderCustom(diameter), true);
					formation.SetMovementOrder(MovementOrder.MovementOrderStop);
					Vec2 facing = (center.AsVec2 - formation.CurrentPosition);
					if (facing.LengthSquared < 0.01f)
					{
						facing = _civilianAssemblyForward.AsVec2;
					}
					if (facing.LengthSquared < 0.01f)
					{
						facing = Vec2.Forward;
					}
					facing = facing.Normalized();
					formation.SetFacingOrder(FacingOrder.FacingOrderLookAtDirection(facing));
				}
				catch (Exception ex)
				{
					Logger.Log("SiegeAiIntervention", "TryApplyNativeCircleFormationOrders formation failed (" + (source ?? "N/A") + "): " + ex.Message);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryApplyNativeCircleFormationOrders failed (" + (source ?? "N/A") + "): " + ex.Message);
		}
	}

	private static bool MoveAlliedSoldierToCordonSlot(Agent soldier, Mission mission, int slot, int count, bool force)
	{
		try
		{
			if (soldier == null || mission == null || !soldier.IsActive() || !_civilianAssemblyPointReady)
			{
				return false;
			}
			Vec3 target = GetAlliedCordonSlotPosition(mission, slot, count);
			Vec3 center = GetCivilianAssemblyCenter(mission);
			Vec3 lookDirection = center - target;
			lookDirection.z = 0f;
			if (lookDirection.LengthSquared < 0.01f)
			{
				lookDirection = -_civilianAssemblyForward;
			}
			if (lookDirection.LengthSquared < 0.01f)
			{
				lookDirection = -Vec3.Forward;
			}
			lookDirection.Normalize();
			float distSq = soldier.Position.DistanceSquared(target);
			if (force || distSq > SoldierCordonTeleportDistance * SoldierCordonTeleportDistance)
			{
				soldier.TeleportToPosition(target);
				soldier.InvalidateTargetAgent();
				distSq = 0f;
			}
			Vec2 target2 = target.AsVec2;
			float now = mission.CurrentTime;
			bool hasRecentMove = LastCordonMoveOrderTimesBySoldier.TryGetValue(soldier.Index, out float lastMoveOrderTime) && now - lastMoveOrderTime < SoldierCordonOrderRefreshSeconds;
			bool shouldIssueMove = force || distSq > SoldierCordonMoveTolerance * SoldierCordonMoveTolerance || (!hasRecentMove && distSq > SoldierCordonSettleTolerance * SoldierCordonSettleTolerance);
			if (shouldIssueMove && !force)
			{
				ClearAgentLookTarget(soldier);
				soldier.SetTargetPosition(target2);
				LastCordonMoveOrderTimesBySoldier[soldier.Index] = now;
			}
			else if (distSq <= SoldierCordonSettleTolerance * SoldierCordonSettleTolerance)
			{
				try
				{
					soldier.ClearTargetFrame();
					LastCordonMoveOrderTimesBySoldier[soldier.Index] = now;
				}
				catch
				{
				}
			}
			bool hasRecentLook = LastCordonLookOrderTimesBySoldier.TryGetValue(soldier.Index, out float lastLookOrderTime) && now - lastLookOrderTime < SoldierCordonLookRefreshSeconds;
			if (force || (distSq <= SoldierCordonSettleTolerance * SoldierCordonSettleTolerance && !hasRecentLook))
			{
				soldier.SetLookToPointOfInterest(center);
				soldier.LookDirection = lookDirection;
				LastCordonLookOrderTimesBySoldier[soldier.Index] = now;
			}
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "MoveAlliedSoldierToCordonSlot failed: " + ex.Message);
			return false;
		}
	}

	private static void ApplyCivilianSurrenderPose(Agent agent, Agent lookTarget, bool forceAction)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive() || _massacreStarted)
			{
				return;
			}
			if (!CivilianCalmedAgentIndexes.Add(agent.Index))
			{
				return;
			}
			agent.InvalidateTargetAgent();
			try
			{
				agent.SetCrouchMode(true);
			}
			catch
			{
			}
			try
			{
				agent.SetMaximumSpeedLimit(0f, false);
			}
			catch
			{
			}
			SetCivilianLookTowardAssemblyInterior(agent, lookTarget);
			ApplyOneTimeFrightenedCivilianAction(agent, allowGathered: true);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyCivilianSurrenderPose failed: " + ex.Message);
		}
	}

	private static void SetCivilianLookTowardAssemblyInterior(Agent agent, Agent fallbackTarget)
	{
		try
		{
			if (agent == null || !agent.IsActive())
			{
				return;
			}
			Vec3 point = Vec3.Zero;
			if (fallbackTarget != null && fallbackTarget.IsActive())
			{
				point = fallbackTarget.Position;
			}
			else if (_civilianAssemblyPointReady)
			{
				point = GetCivilianAssemblyCenter(Mission.Current ?? agent.Mission);
			}
			Vec3 lookDirection = point - agent.Position;
			lookDirection.z = 0f;
			if (lookDirection.LengthSquared < 0.01f)
			{
				lookDirection = -_civilianAssemblyForward;
			}
			if (lookDirection.LengthSquared < 0.01f)
			{
				lookDirection = -Vec3.Forward;
			}
			lookDirection.Normalize();
			Vec3 lookPoint = agent.Position + lookDirection * 4f;
			agent.SetLookToPointOfInterest(lookPoint);
			agent.LookDirection = lookDirection;
		}
		catch
		{
		}
	}

	private static void ApplyFrightenedCivilianIdle(Mission mission)
	{
		if (mission?.Agents == null || _massacreStarted)
		{
			return;
		}
		foreach (Agent agent in mission.Agents.ToList())
		{
			if (!IsEligibleCivilianAgent(agent, includeHeroes: true))
			{
				continue;
			}
			if (CivilianFrightenedActionAgentIndexes.Contains(agent.Index) || CivilianGatherFollowerAgentIndexes.Contains(agent.Index) || CivilianGatherMovePreparedAgentIndexes.Contains(agent.Index))
			{
				continue;
			}
			try
			{
				PrepareCivilianForPreMassacreHitDetection(agent, mission);
				ApplyOneTimeFrightenedCivilianAction(agent);
			}
			catch
			{
			}
		}
	}

	private static void ApplyOneTimeFrightenedCivilianAction(Agent agent, bool allowGathered = false)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive())
			{
				return;
			}
			if (!allowGathered && (CivilianGatherFollowerAgentIndexes.Contains(agent.Index) || CivilianGatherMovePreparedAgentIndexes.Contains(agent.Index)))
			{
				return;
			}
			if (!CivilianFrightenedActionAgentIndexes.Add(agent.Index))
			{
				return;
			}
			agent.SetActionChannel(1, ActionIndexCache.act_scared_idle_1, false, (AnimFlags)0UL, 0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloat, false, -0.2f, 0, true);
		}
		catch
		{
		}
	}

	private static void KeepAlliedTroopsUseful(Mission mission)
	{
		Agent main = Agent.Main ?? mission?.MainAgent;
		if (mission == null || main == null)
		{
			return;
		}
		foreach (Agent agent in mission.Agents.ToList())
		{
			if (agent == null || !agent.IsActive() || !AlliedAgentIndexes.Contains(agent.Index))
			{
				continue;
			}
			try
			{
				if (_plunderStarted && !_massacreStarted && IsSoldierAssignedToPlunder(agent.Index))
				{
					CordonReadyAgentIndexes.Remove(agent.Index);
					RestoreAlliedSoldierFriendlyState(agent, 0f, "allied_plunder_assignment_tick", forceFollow: false, clearTarget: false);
					agent.SetWatchState(Agent.WatchState.Alarmed);
					continue;
				}
				RestoreAlliedSoldierFriendlyState(agent, 0f, "allied_control_tick", forceFollow: false, clearTarget: false);
				if (agent.Formation == null || agent.Team != (mission.PlayerTeam ?? main.Team))
				{
					AssignAgentToPlayerFormation(agent, FormationClass.Infantry);
				}
				if (_massacreStarted)
				{
					CordonReadyAgentIndexes.Remove(agent.Index);
					DisableCompanionStyleFollow(agent);
					agent.SetWatchState(Agent.WatchState.Alarmed);
				}
				else if (CivilianGatherMessengerAgentIndexes.Contains(agent.Index))
				{
					DisableCompanionStyleFollow(agent);
					agent.SetWatchState(Agent.WatchState.Patrolling);
					if (!IsCivilianGatherMessengerBusy(agent.Index))
					{
						agent.ClearTargetFrame();
						agent.InvalidateTargetAgent();
						agent.SetMaximumSpeedLimit(-1f, false);
					}
				}
				else
				{
					if (CordonReadyAgentIndexes.Add(agent.Index))
					{
						DisableCompanionStyleFollow(agent);
						AssignAgentToPlayerFormation(agent, FormationClass.Infantry);
						if (!_soldierDefaultFollowOrderIssued)
						{
							_soldierDefaultFollowOrderIssued = TrySetPlayerFormationFollowOrder(FormationClass.Infantry, "allied_default_follow");
						}
						agent.WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp, Equipment.InitialWeaponEquipPreference.Any);
					}
					agent.SetWatchState(_plunderStarted ? Agent.WatchState.Alarmed : Agent.WatchState.Patrolling);
				}
			}
			catch
			{
			}
		}
	}

	private static void TryKeepAlliedSoldierFollowingInOccupation(Agent soldier, Agent main, Mission mission)
	{
		try
		{
			if (soldier == null || main == null || mission == null || !soldier.IsActive())
			{
				return;
			}
			float now = mission.CurrentTime;
			if (!LastMassacreSoldierFollowOrderTimes.TryGetValue(soldier.Index, out float last) || now - last >= MassacreSoldierFollowRefreshSeconds)
			{
				LastMassacreSoldierFollowOrderTimes[soldier.Index] = now;
				if (!TryApplyCompanionStyleFollow(soldier, main, "massacre_occupation_follow"))
				{
					MoveAlliedSoldierNearMainFallback(soldier, main);
				}
			}
			try
			{
				soldier.InvalidateTargetAgent();
				if (soldier.Position.DistanceSquared(main.Position) <= 3.5f * 3.5f)
				{
					soldier.SetLookAgent(main);
				}
				else
				{
					ClearAgentLookTarget(soldier);
				}
				soldier.SetWatchState(Agent.WatchState.Alarmed);
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryKeepAlliedSoldierFollowingInOccupation failed: " + ex.Message);
		}
	}

	private static void TryAutoPlunderOneNearbyCivilian(Mission mission)
	{
		try
		{
			UpdatePlunderInteractions(mission);
			if (mission?.Agents == null)
			{
				return;
			}
			int desiredConcurrent = GetDesiredPlunderInteractionLimit(mission);
			if (desiredConcurrent <= 0)
			{
				return;
			}
			int guard = 0;
			while (ActivePlunderInteractions.Count < desiredConcurrent && guard++ < desiredConcurrent * 2)
			{
				HashSet<int> activeTargets = new HashSet<int>(ActivePlunderInteractions.Keys);
				Agent target = mission.Agents
					.Where(a => IsEligibleCivilianAgent(a, includeHeroes: true) && !LootedTargets.Contains(BuildTargetKey(a)) && !activeTargets.Contains(a.Index))
					.OrderBy(a => CountNearbyAssignedPlunderSoldiers(a, mission))
					.ThenBy(a => Agent.Main != null ? a.Position.DistanceSquared(Agent.Main.Position) : 0f)
					.FirstOrDefault();
				if (target == null || !AssignSoldierPlunderInteraction(mission, target))
				{
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryAutoPlunderOneNearbyCivilian failed: " + ex.Message);
		}
	}

	private static int GetDesiredPlunderInteractionLimit(Mission mission)
	{
		try
		{
			if (mission?.Agents == null)
			{
				return 0;
			}
			int activeSoldiers = mission.Agents.Count(a => a != null && a.IsHuman && a.IsActive() && AlliedAgentIndexes.Contains(a.Index));
			int remainingTargets = mission.Agents.Count(a => IsEligibleCivilianAgent(a, includeHeroes: true) && !LootedTargets.Contains(BuildTargetKey(a)) && !ActivePlunderInteractions.ContainsKey(a.Index));
			int byRatio = (int)MathF.Ceiling(activeSoldiers * PlunderSoldierAssignmentRatio);
			int desired = Math.Min(MaxConcurrentPlunderInteractions, Math.Max(1, byRatio));
			return Math.Max(0, Math.Min(remainingTargets + ActivePlunderInteractions.Count, desired));
		}
		catch
		{
			return 0;
		}
	}

	private static int CountNearbyAssignedPlunderSoldiers(Agent target, Mission mission)
	{
		try
		{
			if (target == null || mission?.Agents == null)
			{
				return 0;
			}
			int count = 0;
			foreach (PlunderInteraction interaction in ActivePlunderInteractions.Values)
			{
				Agent soldier = mission.Agents.FirstOrDefault(a => a != null && a.Index == interaction.SoldierAgentIndex);
				if (soldier != null && soldier.IsActive() && soldier.Position.DistanceSquared(target.Position) <= 7f * 7f)
				{
					count++;
				}
			}
			return count;
		}
		catch
		{
			return 0;
		}
	}

	private static void UpdatePlunderInteractions(Mission mission)
	{
		if (mission?.Agents == null || ActivePlunderInteractions.Count == 0)
		{
			return;
		}
		float now = mission.CurrentTime;
		foreach (PlunderInteraction interaction in ActivePlunderInteractions.Values.ToList())
		{
			try
			{
				Agent target = mission.Agents.FirstOrDefault(a => a != null && a.Index == interaction.TargetAgentIndex);
				Agent soldier = mission.Agents.FirstOrDefault(a => a != null && a.Index == interaction.SoldierAgentIndex);
				if (target == null || soldier == null || !target.IsActive() || !soldier.IsActive() || LootedTargets.Contains(BuildTargetKey(target)) || !IsEligibleCivilianAgent(target, includeHeroes: true))
				{
					ActivePlunderInteractions.Remove(interaction.TargetAgentIndex);
					continue;
				}
				MoveSoldierTowardPlunderTarget(soldier, target);
				float distanceSq = soldier.Position.DistanceSquared(target.Position);
				if (distanceSq <= PlunderApproachDistance * PlunderApproachDistance)
				{
					soldier.SetLookAgent(target);
					target.SetLookAgent(soldier);
					if (interaction.TalkStartedAt < 0f)
					{
						interaction.TalkStartedAt = now;
						try
						{
							target.SetActionChannel(1, ActionIndexCache.act_scared_idle_1, false, (AnimFlags)0UL, 0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloat, false, -0.2f, 0, true);
						}
						catch
						{
						}
					}
					if (now - interaction.TalkStartedAt >= PlunderTalkSeconds)
					{
						TryLootCivilianAgent(target, massacre: false, force: false, actorName: soldier.Name?.ToString());
						ActivePlunderInteractions.Remove(interaction.TargetAgentIndex);
					}
				}
				else if (now - interaction.StartedAt > 18f)
				{
					ActivePlunderInteractions.Remove(interaction.TargetAgentIndex);
				}
			}
			catch
			{
				ActivePlunderInteractions.Remove(interaction.TargetAgentIndex);
			}
		}
	}

	private static bool AssignSoldierPlunderInteraction(Mission mission, Agent target)
	{
		if (mission?.Agents == null || target == null || ActivePlunderInteractions.ContainsKey(target.Index))
		{
			return false;
		}
		HashSet<int> busySoldiers = new HashSet<int>(ActivePlunderInteractions.Values.Select(x => x.SoldierAgentIndex));
		Agent soldier = mission.Agents.Where(a => a != null && a.IsActive() && AlliedAgentIndexes.Contains(a.Index) && !busySoldiers.Contains(a.Index)).OrderBy(a => a.Position.DistanceSquared(target.Position)).FirstOrDefault();
		if (soldier == null)
		{
			return false;
		}
		ActivePlunderInteractions[target.Index] = new PlunderInteraction
		{
			TargetAgentIndex = target.Index,
			SoldierAgentIndex = soldier.Index,
			StartedAt = mission.CurrentTime
		};
		DisableCompanionStyleFollow(soldier);
		MoveSoldierTowardPlunderTarget(soldier, target);
		return true;
	}

	private static bool IsSoldierAssignedToPlunder(int soldierIndex)
	{
		return ActivePlunderInteractions.Values.Any(x => x.SoldierAgentIndex == soldierIndex);
	}

	private static void MoveSoldierTowardPlunderTarget(Agent soldier, Agent target)
	{
		try
		{
			if (soldier == null || target == null || Mission.Current == null)
			{
				return;
			}
			soldier.SetWatchState(Agent.WatchState.Alarmed);
			TryApplyAgentFollowTarget(soldier, target, "plunder_target_follow", lookAtTarget: false);
		}
		catch
		{
		}
	}

	private static void AutoLootRemainingVisibleCiviliansForPlunder()
	{
		try
		{
			Mission mission = Mission.Current;
			if (mission?.Agents == null)
			{
				return;
			}
			TrackSceneCivilianAgents(mission);
			int count = 0;
			int goldBefore = _lastCivilianGoldLoot;
			foreach (int agentIndex in SceneCivilianAgentIndexes.ToList())
			{
				Agent agent = mission.Agents.FirstOrDefault(x => x != null && x.Index == agentIndex);
				if (agent != null && TryLootCivilianAgent(agent, massacre: false, force: false))
				{
					count++;
				}
			}
			if (count > 0)
			{
				int gainedGold = Math.Max(0, _lastCivilianGoldLoot - goldBefore);
				InformationManager.DisplayMessage(new InformationMessage("【战利清点】本次入城处置共记录 " + SceneCivilianAgentIndexes.Count + " 名普通民众；离场时结算剩余 " + count + " 名，共新增 " + gainedGold + " 第纳尔。", Color.FromUint(0xFFFFC46Bu)));
			}
		}
		catch
		{
		}
	}

	private static bool TryLootCivilianAgent(Agent agent, bool massacre, bool force, string actorName = null)
	{
		bool eligibleCivilian = IsEligibleCivilianAgent(agent, includeHeroes: true);
		bool eligibleMassacreTarget = massacre && IsMassacreTargetAgent(agent, includeHeroes: true);
		if (!eligibleCivilian && !eligibleMassacreTarget)
		{
			return false;
		}
		string targetKey = BuildTargetKey(agent);
		if (!force && !LootedTargets.Add(targetKey))
		{
			return false;
		}
		if (force)
		{
			LootedTargets.Add(targetKey);
		}
		CharacterObject character = agent.Character as CharacterObject;
		Hero hero = character?.HeroObject;
		int amount;
		if (hero != null && hero != Hero.MainHero)
		{
			if (massacre)
			{
				int baseAmount = hero.Gold > 0 ? RandomPercent(hero.Gold, 0.90f, 1.00f) : HeroMassacreFallbackGold;
				amount = hero.IsNotable ? Math.Max(baseAmount, Math.Min(HeroMassacreFallbackGold, Math.Max(baseAmount, HeroMassacreFallbackGold))) : baseAmount;
			}
			else
			{
				amount = hero.Gold > 0 ? RandomPercent(hero.Gold, 0.50f, 0.75f) : MBRandom.RandomInt(300, 751);
			}
			if (hero.Gold > 0)
			{
				amount = Math.Min(amount, hero.Gold);
				if (amount > 0)
				{
					GiveGoldAction.ApplyBetweenCharacters(hero, Hero.MainHero, amount, disableNotification: true);
				}
			}
			else if (amount > 0)
			{
				AwardGoldToPlayer(amount, "civilian_hero_fallback");
			}
		}
		else
		{
			amount = massacre ? NonHeroMassacreGold : MBRandom.RandomInt(NonHeroPlunderMinGold, NonHeroPlunderMaxGold + 1);
			AwardGoldToPlayer(amount, "civilian_flat");
		}
		if (amount > 0)
		{
			_lastCivilianGoldLoot += amount;
			_lastCivilianTargetsLooted++;
			string targetName = agent.Name?.ToString() ?? "目标";
			string line = string.IsNullOrWhiteSpace(actorName) ? ("从 " + targetName + " 取得 " + amount + " 第纳尔。") : (actorName + " 盘问 " + targetName + " 后取得 " + amount + " 第纳尔。");
			InformationManager.DisplayMessage(new InformationMessage("【战利清点】" + line, Color.FromUint(0xFFFFC46Bu)));
			return true;
		}
		return false;
	}

	private static int RandomPercent(int value, float minRatio, float maxRatio)
	{
		float ratio = minRatio + MBRandom.RandomFloat * Math.Max(0f, maxRatio - minRatio);
		return Math.Max(0, (int)MathF.Round(value * ratio));
	}

	private static void PrepareCivilianForMassacreCombat(Agent agent, Mission mission)
	{
		try
		{
			if (agent == null || mission == null || !agent.IsHuman || AlliedAgentIndexes.Contains(agent.Index))
			{
				return;
			}
			Agent main = Agent.Main ?? mission.MainAgent;
			Team playerTeam = mission.PlayerTeam ?? main?.Team;
			Team enemyTeam = EnsureInterventionCivilianEnemyTeam(mission) ?? mission.PlayerEnemyTeam ?? agent.Team;
			CharacterObject character = agent.Character as CharacterObject;
			bool canResist = ShouldCivilianResistMassacre(agent) || DoesAgentCarryRealWeapon(agent) || IsGuardOrSoldier(character);
			NeutralizeCivilianDailyUsableBehavior(agent, "massacre_combat_prepare");
			agent.SetMortalityState(Agent.MortalityState.Mortal);
			if (enemyTeam != null && agent.Team != enemyTeam)
			{
				agent.SetTeam(enemyTeam, true);
			}
			if (agent.Team != null && playerTeam != null && agent.Team != playerTeam)
			{
				agent.Team.SetIsEnemyOf(playerTeam, isEnemyOf: true);
				playerTeam.SetIsEnemyOf(agent.Team, isEnemyOf: true);
			}
			try
			{
				agent.SetCrouchMode(false);
				agent.SetMaximumSpeedLimit(-1f, false);
			}
			catch
			{
			}
			agent.SetWatchState(Agent.WatchState.Alarmed);
			if (canResist)
			{
				try
				{
					Formation enemyFormation = enemyTeam?.GetFormation(FormationClass.Infantry);
					if (enemyFormation != null && agent.Formation != enemyFormation)
					{
						agent.Formation = enemyFormation;
						agent.TryAttachToFormation();
						agent.SetShouldCatchUpWithFormation(true);
						agent.UpdateFormationOrders();
					}
				}
				catch
				{
				}
				if (MassacreCombatPreparedAgentIndexes.Add(agent.Index))
				{
					try
					{
						agent.WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp, Equipment.InitialWeaponEquipPreference.Any);
					}
					catch
					{
					}
				}
				ForceAgentForMassacreFight(agent);
			}
			else
			{
				agent.InvalidateTargetAgent();
				try
				{
					agent.DisableScriptedMovement();
					agent.ClearTargetFrame();
					if (agent.Formation != null)
					{
						agent.Formation = null;
					}
					agent.SetShouldCatchUpWithFormation(false);
				}
				catch
				{
				}
				KeepCivilianHidingFromOccupation(agent, mission, main, force: false);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "PrepareCivilianForMassacreCombat failed: " + ex.Message);
		}
	}

	private static void TryForcePlayerDamageToCivilian(Agent target, int damage, string source)
	{
		try
		{
			Mission mission = Mission.Current ?? target?.Mission;
			Agent main = Agent.Main ?? mission?.MainAgent;
		if (mission == null || main == null || target == null || !target.IsActive() || !IsMassacreTargetAgent(target, includeHeroes: true))
		{
			return;
		}
			float currentTime = mission.CurrentTime;
			if (_lastForcedPlayerDamageAgentIndex == target.Index && currentTime - _lastForcedPlayerDamageMissionTime < 0.05f)
			{
				return;
			}
			_lastForcedPlayerDamageAgentIndex = target.Index;
			_lastForcedPlayerDamageMissionTime = currentTime;
			int finalDamage = Math.Max(15, Math.Min(80, damage));
			PrepareCivilianForMassacreCombat(target, mission);
			Blow blow = new Blow(main.Index);
			blow.DamageType = DamageTypes.Cut;
			blow.BoneIndex = target.Monster.HeadLookDirectionBoneIndex;
			blow.GlobalPosition = target.Position;
			blow.GlobalPosition.z += target.GetEyeGlobalHeight();
			blow.BaseMagnitude = finalDamage;
			blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
			blow.InflictedDamage = finalDamage;
			blow.SwingDirection = main.LookDirection;
			if (blow.SwingDirection.LengthSquared < 0.01f)
			{
				blow.SwingDirection = Vec3.Forward;
			}
			blow.Direction = blow.SwingDirection;
			blow.DamageCalculated = true;
			sbyte mainHandItemBoneIndex = main.Monster.MainHandItemBoneIndex;
			AttackCollisionData attackCollisionData = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow.BoneIndex, BoneBodyPartType.Head, mainHandItemBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow.Direction, blow.GlobalPosition, Vec3.Zero, Vec3.Zero, target.Velocity, Vec3.Up);
			target.RegisterBlow(blow, attackCollisionData);
			Logger.Log("SiegeAiIntervention", "Applied player civilian hit damage source=" + source + " target=" + (target.Name?.ToString() ?? target.Index.ToString()) + " damage=" + finalDamage);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryForcePlayerDamageToCivilian failed (" + source + "): " + ex.Message);
		}
	}

	private static bool IsAgentUsingAnyRealWeapon(Agent agent)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive())
			{
				return false;
			}
			EquipmentIndex primary = agent.GetPrimaryWieldedItemIndex();
			if (IsRealWeaponWieldedSlot(agent, primary))
			{
				return true;
			}
			EquipmentIndex offhand = agent.GetOffhandWieldedItemIndex();
			return IsRealWeaponWieldedSlot(agent, offhand);
		}
		catch
		{
			return true;
		}
	}

	private static bool IsRealWeaponWieldedSlot(Agent agent, EquipmentIndex equipmentIndex)
	{
		try
		{
			if (agent == null || equipmentIndex == EquipmentIndex.None || equipmentIndex < EquipmentIndex.WeaponItemBeginSlot || equipmentIndex >= EquipmentIndex.NumAllWeaponSlots)
			{
				return false;
			}
			MissionWeapon missionWeapon = agent.Equipment[equipmentIndex];
			if (missionWeapon.IsEmpty)
			{
				return false;
			}
			WeaponComponentData usage = missionWeapon.CurrentUsageItem;
			return usage != null && !usage.IsShield && usage.WeaponClass != WeaponClass.Undefined;
		}
		catch
		{
			return false;
		}
	}

	private static void DriveMassacreCombatState(Mission mission)
	{
		Agent main = Agent.Main ?? mission?.MainAgent;
		if (mission == null || main == null)
		{
			return;
		}
		if (_massacreVictoryReached)
		{
			KeepAlliedVictoryCheer(mission);
			return;
		}
		List<Agent> massacreTargets = mission.Agents.Where(a => IsMassacreTargetAgent(a, includeHeroes: true) && !CountedMassacreVictims.Contains(a.Index)).ToList();
		foreach (Agent target in massacreTargets)
		{
			PrepareCivilianForMassacreCombat(target, mission);
		}
		foreach (Agent allied in mission.Agents.ToList())
		{
			if (allied == null || !allied.IsActive() || !AlliedAgentIndexes.Contains(allied.Index))
			{
				continue;
			}
			try
			{
				RestoreAlliedSoldierFriendlyState(allied, 0f, "massacre_drive", forceFollow: false, clearTarget: false);
				DisableCompanionStyleFollow(allied);
				ForceAgentForMassacreFight(allied);
				allied.SetWatchState(Agent.WatchState.Alarmed);
				if (MassacreReadySoldierAgentIndexes.Add(allied.Index))
				{
					try
					{
						allied.WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp, Equipment.InitialWeaponEquipPreference.Any);
					}
					catch
					{
					}
				}
				try
				{
					allied.Formation?.SetMovementOrder(MovementOrder.MovementOrderCharge);
				}
				catch
				{
				}
				Agent target = SelectMassacreTargetForSoldier(allied, massacreTargets);
				if (target != null)
				{
					GuideSoldierTowardMassacreTarget(allied, target, mission);
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "DriveMassacreCombatState soldier failed: " + ex.Message);
			}
		}
	}

	private static Agent SelectMassacreTargetForSoldier(Agent soldier, List<Agent> massacreTargets)
	{
		try
		{
			if (soldier == null || massacreTargets == null || massacreTargets.Count == 0)
			{
				return null;
			}
			return massacreTargets
				.Where(t => t != null && t.IsActive() && !CountedMassacreVictims.Contains(t.Index))
				.OrderBy(t => t.Position.DistanceSquared(soldier.Position))
				.FirstOrDefault();
		}
		catch
		{
			return null;
		}
	}

	private static Agent SelectMassacreTargetForSoldier(Agent soldier, Mission mission)
	{
		try
		{
			if (mission?.Agents == null)
			{
				return null;
			}
			return SelectMassacreTargetForSoldier(soldier, mission.Agents.Where(a => IsMassacreTargetAgent(a, includeHeroes: true) && !CountedMassacreVictims.Contains(a.Index)).ToList());
		}
		catch
		{
			return null;
		}
	}

	private static void GuideSoldierTowardMassacreTarget(Agent soldier, Agent target, Mission mission)
	{
		try
		{
			if (soldier == null || target == null || mission == null || !soldier.IsActive() || !target.IsActive())
			{
				return;
			}
			float now = mission.CurrentTime;
			if (LastMassacreSoldierTargetOrderTimes.TryGetValue(soldier.Index, out float last) && now - last < MassacreSoldierTargetRefreshSeconds)
			{
				return;
			}
			LastMassacreSoldierTargetOrderTimes[soldier.Index] = now;
			soldier.SetLookAgent(target);
			try
			{
				soldier.SetTargetPosition(target.Position.AsVec2);
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "GuideSoldierTowardMassacreTarget failed: " + ex.Message);
		}
	}

	private static void ForceAgentForMassacreFight(Agent agent)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive())
			{
				return;
			}
			CampaignAgentComponent component = agent.GetComponent<CampaignAgentComponent>();
			AgentNavigator navigator = component?.AgentNavigator ?? component?.CreateAgentNavigator();
			AlarmedBehaviorGroup behaviorGroup = navigator?.GetBehaviorGroup<AlarmedBehaviorGroup>();
			if (behaviorGroup != null)
			{
				behaviorGroup.DisableCalmDown = true;
				behaviorGroup.AddBehavior<FightBehavior>();
				behaviorGroup.SetScriptedBehavior<FightBehavior>();
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ForceAgentForMassacreFight failed: " + ex.Message);
		}
	}

	private static bool DoesAgentCarryRealWeapon(Agent agent)
	{
		try
		{
			if (agent == null || !agent.IsHuman || !agent.IsActive())
			{
				return false;
			}
			for (EquipmentIndex slot = EquipmentIndex.WeaponItemBeginSlot; slot < EquipmentIndex.NumAllWeaponSlots; slot++)
			{
				if (IsRealWeaponWieldedSlot(agent, slot))
				{
					return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	private static bool ShouldCivilianResistMassacre(Agent agent)
	{
		try
		{
			if (agent == null || !agent.IsHuman || AlliedAgentIndexes.Contains(agent.Index))
			{
				return false;
			}
			if (!IsMassacreTargetAgent(agent, includeHeroes: true))
			{
				return false;
			}
			int stableIndex = Math.Abs(agent.Index);
			return stableIndex % 5 == 0;
		}
		catch
		{
			return false;
		}
	}

	private static Vec3 ResolveCivilianEscapeOrHideTarget(Agent civilian, Mission mission, Agent main)
	{
		Vec3 origin = civilian?.Position ?? Vec3.Zero;
		EnsureCivilianRoutPointPools(mission, main);
		List<Vec3> preferredPool = ShouldCivilianEscapeSettlement(civilian) ? CivilianEscapePointPool : CivilianInteriorHidePointPool;
		List<Vec3> fallbackPool = ShouldCivilianEscapeSettlement(civilian) ? CivilianInteriorHidePointPool : CivilianEscapePointPool;
		List<Vec3> pool = preferredPool.Count > 0 ? preferredPool : fallbackPool;
		if (pool.Count > 0)
		{
			int index = Math.Abs(civilian?.Index ?? 0) % pool.Count;
			return pool[index];
		}
		Vec3 mainPosition = main?.Position ?? origin;
		Vec3 away = origin - mainPosition;
		away.z = 0f;
		if (away.LengthSquared < 0.01f)
		{
			away = -(main?.LookDirection ?? Vec3.Forward);
			away.z = 0f;
		}
		if (away.LengthSquared < 0.01f)
		{
			away = Vec3.Forward;
		}
		away.Normalize();
		return ProjectCivilianRoutPointToGround(mission, origin + away * MassacreCivilianHideDistance);
	}

	private static bool ShouldCivilianEscapeSettlement(Agent civilian)
	{
		try
		{
			return civilian != null && civilian.Index >= 0 && civilian.Index % 3 == 0;
		}
		catch
		{
			return false;
		}
	}

	private static void EnsureCivilianRoutPointPools(Mission mission, Agent main)
	{
		try
		{
			string sceneName = mission?.SceneName ?? "";
			if (_civilianRoutPointPoolSceneName == sceneName && (CivilianInteriorHidePointPool.Count > 0 || CivilianEscapePointPool.Count > 0))
			{
				return;
			}
			CivilianInteriorHidePointPool.Clear();
			CivilianEscapePointPool.Clear();
			_civilianRoutPointPoolSceneName = sceneName;
			Vec3 anchor = main?.Position ?? GetCivilianAssemblyCenter(mission);
			Vec3 forward = main?.LookDirection ?? _civilianAssemblyForward;
			forward.z = 0f;
			if (forward.LengthSquared < 0.01f)
			{
				forward = Vec3.Forward;
			}
			forward.Normalize();
			Vec3 right = Vec3.CrossProduct(forward, Vec3.Up);
			if (right.LengthSquared < 0.01f)
			{
				right = Vec3.Side;
			}
			right.Normalize();
			AddTaggedCivilianRoutPoints(mission, CivilianInteriorHidePointPool, anchor, outwardDistance: 0f,
				"sp_merchant", "sp_blacksmith", "sp_armorer", "sp_weaponsmith", "sp_horse_merchant", "sp_horse_trader",
				"sp_shop_worker", "sp_workshop_worker", "sp_tavernkeeper", "sp_tavern_wench", "sp_barber",
				"sp_guard", "sp_guard_castle", "sp_prison_guard", "sp_lord_hall_guard", "sp_player_conversation",
				"binary_conversation_point", "center_conversation_point", "sp_notables_parent");
			AddTaggedCivilianRoutPoints(mission, CivilianEscapePointPool, anchor, outwardDistance: 34f,
				"sp_outside_near_town_main_gate", "sp_player_near_town_main_gate", "sp_player_near_town_gate",
				"sp_player_near_castle_gate", "sp_player_near_gate", "sp_castle_gate", "main_gate", "town_gate",
				"castle_gate", "gate", "spawnpoint_player_outside", "spawnpoint_player", "sp_player_outside");
			Vec3[] fixedInteriorDirections =
			{
				forward,
				-forward,
				right,
				-right,
				(forward + right),
				(forward - right),
				(-forward + right),
				(-forward - right)
			};
			for (int i = 0; i < fixedInteriorDirections.Length; i++)
			{
				Vec3 direction = fixedInteriorDirections[i];
				direction.z = 0f;
				if (direction.LengthSquared < 0.01f)
				{
					continue;
				}
				direction.Normalize();
				float distance = 18f + (i % 4) * 4.5f;
				AddCivilianRoutPointUnique(CivilianInteriorHidePointPool, ProjectCivilianRoutPointToGround(mission, anchor + direction * distance));
			}
			Vec3[] fixedEscapeDirections =
			{
				forward,
				-forward,
				right,
				-right,
				(forward + right),
				(forward - right)
			};
			for (int i = 0; i < fixedEscapeDirections.Length; i++)
			{
				Vec3 direction = fixedEscapeDirections[i];
				direction.z = 0f;
				if (direction.LengthSquared < 0.01f)
				{
					continue;
				}
				direction.Normalize();
				float distance = 52f + (i % 3) * 9f;
				AddCivilianRoutPointUnique(CivilianEscapePointPool, ProjectCivilianRoutPointToGround(mission, anchor + direction * distance));
			}
			Logger.Log("SiegeAiIntervention", "Prepared civilian rout point pools. Scene=" + (sceneName ?? "N/A") + ", Interior=" + CivilianInteriorHidePointPool.Count + ", Escape=" + CivilianEscapePointPool.Count);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "EnsureCivilianRoutPointPools failed: " + ex.Message);
		}
	}

	private static void AddTaggedCivilianRoutPoints(Mission mission, List<Vec3> pool, Vec3 anchor, float outwardDistance, params string[] tags)
	{
		if (mission?.Scene == null || pool == null || tags == null)
		{
			return;
		}
		foreach (string tag in tags)
		{
			try
			{
				foreach (GameEntity entity in mission.Scene.FindEntitiesWithTag(tag))
				{
					if (entity == null)
					{
						continue;
					}
					MatrixFrame frame = entity.GetGlobalFrame();
					Vec3 point = frame.origin;
					if (outwardDistance > 0f)
					{
						Vec3 outward = point - anchor;
						outward.z = 0f;
						if (outward.LengthSquared < 0.01f)
						{
							outward = Vec3.Forward;
						}
						outward.Normalize();
						point += outward * outwardDistance;
					}
					AddCivilianRoutPointUnique(pool, ProjectCivilianRoutPointToGround(mission, point));
				}
			}
			catch
			{
			}
		}
	}

	private static void AddCivilianRoutPointUnique(List<Vec3> pool, Vec3 point)
	{
		if (pool == null)
		{
			return;
		}
		if (pool.Any(existing => existing.DistanceSquared(point) <= 4f))
		{
			return;
		}
		pool.Add(point);
	}

	private static Vec3 ProjectCivilianRoutPointToGround(Mission mission, Vec3 point)
	{
		try
		{
			if (mission?.Scene != null)
			{
				point.z = mission.Scene.GetGroundHeightAtPosition(point, BodyFlags.CommonCollisionExcludeFlags);
			}
		}
		catch
		{
			try
			{
				if (mission?.Scene != null)
				{
					point.z = mission.Scene.GetGroundHeightAtPosition(point);
				}
			}
			catch
			{
			}
		}
		return point;
	}

	private static void KeepCivilianHidingFromOccupation(Agent civilian, Mission mission, Agent main, bool force)
	{
		try
		{
			if (civilian == null || mission == null || main == null || !civilian.IsActive())
			{
				return;
			}
			civilian.InvalidateTargetAgent();
			civilian.SetWatchState(Agent.WatchState.Alarmed);
			ApplyOneTimeFrightenedCivilianAction(civilian, allowGathered: true);
			float now = mission.CurrentTime;
			bool hasHideTarget = CivilianHideTargets.TryGetValue(civilian.Index, out Vec3 hideTarget);
			bool hasLastOrder = LastCivilianHideOrderTimes.TryGetValue(civilian.Index, out float lastOrder);
			bool reachedHideTarget = hasHideTarget && civilian.Position.DistanceSquared(hideTarget) <= 4f * 4f;
			bool needNewTarget = force || !hasHideTarget || !hasLastOrder || (!reachedHideTarget && now - lastOrder >= MassacreCivilianHideRefreshSeconds);
			if (needNewTarget)
			{
				CivilianHideSettledAgentIndexes.Remove(civilian.Index);
				hideTarget = ResolveCivilianEscapeOrHideTarget(civilian, mission, main);
				CivilianHideTargets[civilian.Index] = hideTarget;
				LastCivilianHideOrderTimes[civilian.Index] = now;
				ClearAgentLookTarget(civilian);
				civilian.SetMaximumSpeedLimit(-1f, false);
				try
				{
					civilian.SetCrouchMode(false);
				}
				catch
				{
				}
				civilian.SetTargetPosition(hideTarget.AsVec2);
			}
			else if (reachedHideTarget)
			{
				if (CivilianHideSettledAgentIndexes.Add(civilian.Index))
				{
					try
					{
						civilian.DisableScriptedMovement();
						civilian.ClearTargetFrame();
						civilian.InvalidateTargetAgent();
						civilian.SetMaximumSpeedLimit(0f, false);
						bool escaped = ShouldCivilianEscapeSettlement(civilian);
						civilian.SetCrouchMode(!escaped);
						if (escaped)
						{
							ClearAgentLookTarget(civilian);
						}
						else
						{
							civilian.SetLookToPointOfInterest(main.Position);
						}
					}
					catch
					{
					}
				}
				return;
			}
			try
			{
				civilian.SetMaximumSpeedLimit(-1f, false);
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "KeepCivilianHidingFromOccupation failed: " + ex.Message);
		}
	}

	private static void TryCompleteMassacreIfAllTargetsDown(Mission mission)
	{
		if (_massacreVictoryReached || mission?.Agents == null)
		{
			return;
		}
		int remaining = mission.Agents.Count(a => IsMassacreTargetAgent(a, includeHeroes: true) && !CountedMassacreVictims.Contains(a.Index));
		if (remaining > 0)
		{
			return;
		}
		CompleteMassacreVictory(mission, "all_targets_down");
	}

	private static void CompleteMassacreVictory(Mission mission, string reason)
	{
		if (_massacreVictoryReached)
		{
			return;
		}
		_massacreVictoryReached = true;
		TryEndNativeMissionFightHandlerForManualExit(mission);
		KeepAlliedVictoryCheer(mission);
		if (_culturalRepopulationRequested)
		{
			ApplyCulturalRepopulationNow("massacre_victory");
		}
		string text = "【攻城处置】血洗完成：城内残余抵抗已经肃清。离场后将结算战利品和第纳尔。";
		InformationManager.DisplayMessage(new InformationMessage(text, Color.FromUint(0xFFFF7777u)));
		ShowMassacreVictoryLootMessages();
		try
		{
			MBInformationManager.AddQuickInformation(new TextObject("血洗完成，离场后结算战利品。"), 0, null, null, "event:/ui/mission/arena_victory");
		}
		catch
		{
		}
		Logger.Log("SiegeAiIntervention", "Massacre victory completed. Reason=" + (reason ?? "N/A") + ", SpawnedCivilians=" + _lastSceneCivilianSpawnedCount + ", CountedVictims=" + CountedMassacreVictims.Count);
	}

	private static void TryEndNativeMissionFightHandlerForManualExit(Mission mission)
	{
		try
		{
			if (mission == null)
			{
				return;
			}
			SandBox.Missions.MissionLogics.MissionFightHandler fightHandler = mission.GetMissionBehavior<SandBox.Missions.MissionLogics.MissionFightHandler>();
			if (fightHandler != null && fightHandler.IsThereActiveFight())
			{
				fightHandler.EndFight(true);
				Logger.Log("SiegeAiIntervention", "Ended native MissionFightHandler after massacre victory so TAB can leave normally.");
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryEndNativeMissionFightHandlerForManualExit failed: " + ex.Message);
		}
	}

	private static void ShowMassacreVictoryLootMessages()
	{
		try
		{
			if (_lastMarketGoldLoot > 0)
			{
				InformationManager.DisplayMessage(new InformationMessage("【战利清点】血洗市场金库：获得 " + _lastMarketGoldLoot + " 第纳尔。", Color.FromUint(0xFFFFC46Bu)));
			}
			if (_lastLootItemTotal > 0)
			{
				InformationManager.DisplayMessage(new InformationMessage("【战利清点】血洗市场库存：截获 " + _lastLootItemTotal + " 件货物（" + _lastLootStackKinds + " 类，估值 " + _lastLootValue + "）；离场后进入战利品界面领取。", Color.FromUint(0xFFFFC46Bu)));
			}
			if (_lastCivilianGoldLoot > 0)
			{
				InformationManager.DisplayMessage(new InformationMessage("【战利清点】民众财物：取得 " + _lastCivilianGoldLoot + " 第纳尔。", Color.FromUint(0xFFFFC46Bu)));
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ShowMassacreVictoryLootMessages failed: " + ex.Message);
		}
	}

	private static void KeepAlliedVictoryCheer(Mission mission)
	{
		if (mission?.Agents == null)
		{
			return;
		}
		foreach (Agent allied in mission.Agents.ToList())
		{
			if (allied == null || !allied.IsActive() || !AlliedAgentIndexes.Contains(allied.Index))
			{
				continue;
			}
			if (!VictoryCheerAgentIndexes.Add(allied.Index))
			{
				continue;
			}
			try
			{
				allied.InvalidateTargetAgent();
				allied.DisableScriptedMovement();
				allied.SetTargetPosition(allied.Position.AsVec2);
				allied.Formation?.SetMovementOrder(MovementOrder.MovementOrderStop);
				Agent main = Agent.Main ?? mission.MainAgent;
				if (main != null && main.IsActive())
				{
					allied.SetLookAgent(main);
				}
				allied.SetWatchState(Agent.WatchState.Patrolling);
				allied.SetActionChannel(0, ActionIndexCache.act_cheer_1, true, (AnimFlags)0UL, 0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloat, false, -0.2f, 0, true);
				allied.SetActionChannel(1, ActionIndexCache.act_none, true, (AnimFlags)0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "Victory cheer failed: " + ex.Message);
			}
		}
	}

	private static void OnInterventionAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState)
	{
		if (!_massacreStarted || affectedAgent == null || CountedMassacreVictims.Contains(affectedAgent.Index))
		{
			return;
		}
		if (agentState != AgentState.Killed && agentState != AgentState.Unconscious)
		{
			return;
		}
		if (!IsMassacreTargetAgent(affectedAgent, includeHeroes: true, requireActive: false))
		{
			return;
		}
		CountedMassacreVictims.Add(affectedAgent.Index);
		_lastMassacreRealKillMissionTime = (Mission.Current ?? affectedAgent.Mission)?.CurrentTime ?? _lastMassacreRealKillMissionTime;
		CharacterObject character = affectedAgent.Character as CharacterObject;
		Hero hero = character?.HeroObject;
		if (hero?.IsNotable == true)
		{
			_lastKilledNotables++;
		}
		else if (hero == null)
		{
			_lastKilledCivilianUnits++;
		}
		TryLootCivilianAgent(affectedAgent, massacre: true, force: true);
	}

	private static bool IsEligibleCivilianAgent(Agent agent, bool includeHeroes, bool requireActive = true)
	{
		if (agent == null || !agent.IsHuman || agent == Agent.Main || AlliedAgentIndexes.Contains(agent.Index))
		{
			return false;
		}
		if (requireActive && (!agent.IsActive() || agent.State == AgentState.Killed || agent.State == AgentState.Unconscious))
		{
			return false;
		}
		CharacterObject character = agent.Character as CharacterObject;
		if (!includeHeroes && character?.HeroObject != null)
		{
			return false;
		}
		return IsCivilianForIntervention(character);
	}

	private static bool IsMassacreTargetAgent(Agent agent, bool includeHeroes, bool requireActive = true)
	{
		if (agent == null || !agent.IsHuman || agent == Agent.Main || AlliedAgentIndexes.Contains(agent.Index))
		{
			return false;
		}
		if (requireActive && (!agent.IsActive() || agent.State == AgentState.Killed || agent.State == AgentState.Unconscious))
		{
			return false;
		}
		CharacterObject character = agent.Character as CharacterObject;
		if (character == null || character == CharacterObject.PlayerCharacter || IsProtectedChildCharacter(character) || IsProtectedNotableCharacter(character) || IsBackstreetOrCriminalCharacter(character))
		{
			return false;
		}
		Hero hero = character.HeroObject;
		if (hero != null)
		{
			return false;
		}
		return IsCivilianForIntervention(character);
	}

	private static bool IsProtectedChildAgent(Agent agent)
	{
		return IsProtectedChildCharacter(agent?.Character as CharacterObject);
	}

	private static bool IsProtectedChildCharacter(CharacterObject character)
	{
		return character != null && SceneTauntBehavior.IsChildSceneProtectedTarget(character);
	}

	private static bool IsProtectedNotableAgent(Agent agent)
	{
		return IsProtectedNotableCharacter(agent?.Character as CharacterObject);
	}

	private static bool IsProtectedNotableCharacter(CharacterObject character)
	{
		try
		{
			Hero hero = character?.HeroObject;
			return hero != null && hero != Hero.MainHero && hero.IsNotable;
		}
		catch
		{
			return false;
		}
	}

	private static bool IsGuardOrSoldier(CharacterObject character)
	{
		if (character == null)
		{
			return false;
		}
		return character.Occupation == Occupation.Soldier || character.Occupation == Occupation.Guard || character.Occupation == Occupation.PrisonGuard || character.Occupation == Occupation.BannerBearer || character.Occupation == Occupation.CaravanGuard;
	}

	private static bool IsBackstreetOrCriminalCharacter(CharacterObject character)
	{
		if (character == null)
		{
			return false;
		}
		switch (character.Occupation)
		{
		case Occupation.Gangster:
		case Occupation.GangLeader:
		case Occupation.Bandit:
			return true;
		}
		string id = character.StringId ?? "";
		string name = character.Name?.ToString() ?? "";
		return ContainsAny((id + " " + name).ToLowerInvariant(), "gangster", "gang_leader", "bandit", "looter", "thug", "robber", "backstreet", "alley", "暴徒", "帮派", "后巷", "流氓", "土匪", "劫匪");
	}

	private static bool IsCivilianForIntervention(CharacterObject character)
	{
		if (character == null || character == CharacterObject.PlayerCharacter || IsGuardOrSoldier(character) || IsBackstreetOrCriminalCharacter(character) || IsProtectedChildCharacter(character) || IsProtectedNotableCharacter(character))
		{
			return false;
		}
		Hero hero = character.HeroObject;
		if (hero != null)
		{
			return false;
		}
		switch (character.Occupation)
		{
		case Occupation.Townsfolk:
		case Occupation.Villager:
		case Occupation.GoodsTrader:
		case Occupation.Artisan:
		case Occupation.Merchant:
		case Occupation.Weaponsmith:
		case Occupation.Armorer:
		case Occupation.HorseTrader:
		case Occupation.ShopWorker:
		case Occupation.Blacksmith:
		case Occupation.Tavernkeeper:
		case Occupation.TavernWench:
		case Occupation.TavernGameHost:
		case Occupation.Musician:
		case Occupation.Preacher:
		case Occupation.RansomBroker:
		case Occupation.ShipWright:
		case Occupation.NotAssigned:
			return true;
		default:
			return false;
		}
	}

	private static bool SummonAlliedTroops(int requestedCount, string source)
	{
		Mission mission = Mission.Current;
		Agent main = Agent.Main ?? mission?.MainAgent;
		Team team = mission?.PlayerTeam ?? main?.Team;
		PartyBase party = PartyBase.MainParty;
		if (mission == null || main == null || team == null || party?.MemberRoster == null)
		{
			return false;
		}
		int count = Math.Max(1, Math.Min(requestedCount, MaxSummonPerAction));
		List<CharacterObject> troops = PickInterventionTroops(count);
		if (troops.Count == 0)
		{
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】主部队没有可入城的健康士兵或同伴。", Color.FromUint(0xFFFFD27Fu)));
			return false;
		}
		int spawned = 0;
		Vec3 anchor = main.Position;
		Formation infantryFormation = team.GetFormation(FormationClass.Infantry);
		MarkFormationPlayerCommandable(infantryFormation, main);
		Vec3 forward = main.LookDirection;
		if (forward.LengthSquared < 0.01f)
		{
			forward = Vec3.Forward;
		}
		forward.Normalize();
		Vec3 right = Vec3.CrossProduct(forward, Vec3.Up);
		if (right.LengthSquared < 0.01f)
		{
			right = Vec3.Side;
		}
		right.Normalize();
		bool spawnInCordon = false;
		Vec3 cordonCenter = spawnInCordon ? GetCivilianAssemblyCenter(mission) : anchor;
		for (int i = 0; i < troops.Count; i++)
		{
			CharacterObject troop = troops[i];
			Vec3 position = spawnInCordon
				? GetAlliedCordonSlotPosition(mission, i, troops.Count)
				: (anchor - forward * (2.5f + i / 3f) + right * (((i % 2 == 0) ? 1f : -1f) * (1.5f + (i % 8) * 0.8f)));
			Vec3 spawnDirection = spawnInCordon ? (cordonCenter - position) : forward;
			spawnDirection.z = 0f;
			if (spawnDirection.LengthSquared < 0.01f)
			{
				spawnDirection = forward;
			}
			spawnDirection.Normalize();
			try
			{
				if (mission.Scene != null)
				{
					position.z = mission.Scene.GetGroundHeightAtPosition(position);
				}
			}
			catch
			{
			}
			try
			{
				IAgentOriginBase origin = new PartyAgentOrigin(party, troop);
				CommandableOriginRuntimeIds.Add(RuntimeHelpers.GetHashCode(origin));
				AgentBuildData buildData = new AgentBuildData(troop).TroopOrigin(origin).Monster(TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(troop.Race, "_settlement")).Team(team)
					.InitialPosition(in position)
					.InitialDirection(spawnDirection.AsVec2.Normalized())
					.Controller(AgentControllerType.AI)
					.CivilianEquipment(civilianEquipment: false)
					.NoHorses(noHorses: true);
				if (infantryFormation != null)
				{
					buildData = buildData.Formation(infantryFormation)
						.FormationTroopSpawnCount(troops.Count)
						.FormationTroopSpawnIndex(i)
						.SpawnsIntoOwnFormation(true)
						.SpawnsUsingOwnTroopClass(false);
				}
				Agent spawnedAgent = mission.SpawnAgent(buildData, false);
				if (spawnedAgent != null)
				{
					NotifyAgentBuiltForMission(spawnedAgent, mission);
					spawned++;
					AlliedAgentIndexes.Add(spawnedAgent.Index);
					RestoreAlliedSoldierFriendlyState(spawnedAgent, 0f, "spawn_allied_troop", forceFollow: false);
					AssignAgentToPlayerFormation(spawnedAgent, FormationClass.Infantry);
					spawnedAgent.SetWatchState(_massacreStarted ? Agent.WatchState.Alarmed : Agent.WatchState.Patrolling);
					if (_massacreStarted)
					{
						DisableCompanionStyleFollow(spawnedAgent);
						ForceAgentForMassacreFight(spawnedAgent);
					}
					else if (spawnInCordon)
					{
						DisableCompanionStyleFollow(spawnedAgent);
						spawnedAgent.SetWatchState(Agent.WatchState.Patrolling);
						if (!_soldierDefaultFollowOrderIssued)
						{
							_soldierDefaultFollowOrderIssued = TrySetPlayerFormationFollowOrder(FormationClass.Infantry, "spawn_default_follow");
						}
					}
					else
					{
						DisableCompanionStyleFollow(spawnedAgent);
						if (!_soldierDefaultFollowOrderIssued)
						{
							_soldierDefaultFollowOrderIssued = TrySetPlayerFormationFollowOrder(FormationClass.Infantry, "spawn_default_follow");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "Spawn allied troop failed: " + ex.Message);
			}
		}
		if (spawned > 0)
		{
			TrySetPlayerFormationFollowOrder(FormationClass.Infantry, "spawn_follow_after_batch");
			TryPrimePlayerOrderController(mission, "spawn_allied_batch", force: true);
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】已带入 " + spawned + " 名随行士兵/同伴，默认编入一队并保持列队跟随。", Color.FromUint(0xFFB6F7A8u)));
			return true;
		}
		return false;
	}

	private static List<CharacterObject> PickInterventionTroops(int count)
	{
		List<CharacterObject> selectedTroops = ExpandSelectedInterventionRoster(count);
		if (selectedTroops.Count > 0)
		{
			return selectedTroops;
		}
		return PickTroopsFromMainParty(count);
	}

	private static List<CharacterObject> ExpandSelectedInterventionRoster(int count)
	{
		List<CharacterObject> result = new List<CharacterObject>();
		TroopRoster roster = _selectedInterventionRoster;
		if (roster == null || count <= 0)
		{
			return result;
		}
		for (int i = 0; i < roster.Count && result.Count < count; i++)
		{
			TroopRosterElement element = roster.GetElementCopyAtIndex(i);
			CharacterObject character = element.Character;
			if (!IsSelectableInterventionTroop(character) || element.Number <= 0)
			{
				continue;
			}
			int available = character.HeroObject != null ? element.Number : Math.Max(0, element.Number - element.WoundedNumber);
			for (int j = 0; j < available && result.Count < count; j++)
			{
				result.Add(character);
			}
		}
		return result;
	}

	private static List<CharacterObject> PickTroopsFromMainParty(int count)
	{
		List<CharacterObject> result = new List<CharacterObject>();
		TroopRoster roster = PartyBase.MainParty?.MemberRoster;
		if (roster == null)
		{
			return result;
		}
		for (int i = 0; i < roster.Count && result.Count < count; i++)
		{
			TroopRosterElement element = roster.GetElementCopyAtIndex(i);
			CharacterObject character = element.Character;
			if (character == null || character.IsHero || character == CharacterObject.PlayerCharacter || element.Number <= 0)
			{
				continue;
			}
			int available = Math.Max(0, element.Number - element.WoundedNumber);
			for (int j = 0; j < available && result.Count < count; j++)
			{
				result.Add(character);
			}
		}
		return result;
	}

	private static bool IsSelectableInterventionTroop(CharacterObject character)
	{
		try
		{
			if (character == null || character == CharacterObject.PlayerCharacter)
			{
				return false;
			}
			Hero hero = character.HeroObject;
			if (hero != null)
			{
				return hero != Hero.MainHero && hero.PartyBelongedTo == MobileParty.MainParty && !hero.IsPrisoner && !hero.IsWounded;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static void LootSettlementMarketGold(string reason, bool showMessage = true)
	{
		try
		{
			Settlement settlement = ResolveCurrentSettlement();
			Town town = settlement?.Town;
			if (town == null || town.Gold <= 0)
			{
				return;
			}
			int amount = town.Gold;
			town.ChangeGold(-amount);
			AwardGoldToPlayer(amount, "market_gold");
			_lastMarketGoldLoot += amount;
			if (showMessage)
			{
				InformationManager.DisplayMessage(new InformationMessage("【战利清点】" + reason + "市场金库：获得 " + amount + " 第纳尔。", Color.FromUint(0xFFFFC46Bu)));
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "LootSettlementMarketGold failed: " + ex.Message);
		}
	}

	private static void LootSettlementMarketInventory(float minRatio, float maxRatio, string reason, bool showMessage = true)
	{
		try
		{
			Settlement settlement = ResolveCurrentSettlement();
			ItemRoster sourceRoster = settlement?.ItemRoster;
			if (sourceRoster == null || MobileParty.MainParty == null || sourceRoster.Count <= 0)
			{
				return;
			}
			if (_pendingLootRoster == null)
			{
				_pendingLootRoster = new ItemRoster();
			}
			List<MarketLootCandidate> stacks = new List<MarketLootCandidate>();
			int totalAmount = 0;
			for (int i = 0; i < sourceRoster.Count; i++)
			{
				ItemRosterElement element = sourceRoster.GetElementCopyAtIndex(i);
				if (element.EquipmentElement.Item != null && element.Amount > 0)
				{
					stacks.Add(new MarketLootCandidate
					{
						EquipmentElement = element.EquipmentElement,
						Amount = element.Amount
					});
					totalAmount += element.Amount;
				}
			}
			if (totalAmount <= 0 || stacks.Count == 0)
			{
				return;
			}
			float ratio = minRatio + MBRandom.RandomFloat * Math.Max(0f, maxRatio - minRatio);
			int targetAmount = Math.Max(1, Math.Min(totalAmount, (int)MathF.Round(totalAmount * ratio)));
			List<MarketLootCandidate> shuffled = stacks.OrderBy(_ => MBRandom.RandomFloat).ToList();
			int remaining = targetAmount;
			int movedTotal = 0;
			int movedValue = 0;
			HashSet<string> movedKindKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			while (remaining > 0 && shuffled.Any(x => x.Amount > 0))
			{
				foreach (MarketLootCandidate element in shuffled)
				{
					if (remaining <= 0)
					{
						break;
					}
					int available = Math.Min(element.Amount, remaining);
					if (available <= 0)
					{
						continue;
					}
					int move = Math.Max(1, MBRandom.RandomInt(1, available + 1));
					sourceRoster.AddToCounts(element.EquipmentElement, -move);
					_pendingLootRoster.AddToCounts(element.EquipmentElement, move);
					_pendingLootScreen = true;
					element.Amount -= move;
					remaining -= move;
					movedTotal += move;
					movedKindKeys.Add(element.EquipmentElement.Item?.StringId ?? "item");
					movedValue += Math.Max(0, RewardSystemBehavior.Instance?.GetInventoryActualItemUnitValueForExternal(element.EquipmentElement) ?? element.EquipmentElement.Item?.Value ?? 0) * move;
				}
			}
			_lastLootItemTotal += movedTotal;
			_lastLootStackKinds += movedKindKeys.Count;
			_lastLootValue += movedValue;
			if (showMessage && movedTotal > 0)
			{
				InformationManager.DisplayMessage(new InformationMessage("【战利清点】" + reason + "市场库存：截获 " + movedTotal + " 件货物（" + movedKindKeys.Count + " 类，估值 " + movedValue + "）；离场后进入战利品界面领取。", Color.FromUint(0xFFFFC46Bu)));
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "LootSettlementMarketInventory failed: " + ex.Message);
		}
	}

	private static void AdjustSettlementAfterRelief(Settlement settlement, int publicTrustDelta, float loyaltyDelta, float securityDelta)
	{
		try
		{
			if (settlement != null && RewardSystemBehavior.Instance != null)
			{
				RewardSystemBehavior.Instance.AdjustSettlementLocalPublicTrustForExternal(settlement, publicTrustDelta, "siege_ai_relief");
			}
			if (settlement?.Town != null)
			{
				settlement.Town.Loyalty += loyaltyDelta;
				settlement.Town.Security += securityDelta;
			}
		}
		catch
		{
		}
	}

	private static void AdjustSettlementPublicTrustOnly(Settlement settlement, int publicTrustDelta, string reason)
	{
		try
		{
			if (settlement != null && RewardSystemBehavior.Instance != null)
			{
				RewardSystemBehavior.Instance.AdjustSettlementLocalPublicTrustForExternal(settlement, publicTrustDelta, reason);
			}
		}
		catch
		{
		}
	}

	private static void MarkPendingAftermath(SiegeAftermathAction.SiegeAftermath aftermath, string triggerSource, string triggerDetail)
	{
		SiegeAftermathResolutionKind requestedAftermath = ToStandaloneAftermathKind(aftermath);
		if (SiegeAftermathSelectionPolicy.ShouldReturnSharedReliefPool(requestedAftermath))
		{
			ReturnSharedCivilianReliefPoolToPlayerForNegativeOutcome(triggerSource ?? aftermath.ToString());
		}
		if (SiegeAftermathSelectionPolicy.ShouldReplacePendingAftermath(requestedAftermath, ToStandaloneAftermathKind(_pendingAftermath), _hasPendingAftermath, _massacreStarted, _culturalRepopulationRequested))
		{
			ResetOutcomeMessageDedupForTrack(aftermath.ToString());
			_pendingAftermath = aftermath;
			_pendingAftermathTrigger = (triggerSource ?? "").Trim();
			_pendingAftermathDetail = (triggerDetail ?? "").Trim();
		}
		_hasPendingAftermath = true;
	}

	private static void ResetOutcomeMessageDedup()
	{
		OutcomeMessageDeduplicator.Reset();
	}

	private static void ResetOutcomeMessageDedupForTrack(string track)
	{
		OutcomeMessageDeduplicator.ResetForTrack(track);
	}

	private static void ShowOutcomeMessageOnce(string key, string message, uint color)
	{
		try
		{
			if (!OutcomeMessageDeduplicator.ShouldShow(key, message))
			{
				return;
			}
			InformationManager.DisplayMessage(new InformationMessage(message, Color.FromUint(color)));
		}
		catch
		{
		}
	}

	private static int GetAftermathSeverity(SiegeAftermathAction.SiegeAftermath aftermath)
	{
		return SiegeAftermathSelectionPolicy.GetSeverity(ToStandaloneAftermathKind(aftermath));
	}

	private static SiegeAftermathResolutionKind ToStandaloneAftermathKind(SiegeAftermathAction.SiegeAftermath aftermath)
	{
		return aftermath switch
		{
			SiegeAftermathAction.SiegeAftermath.Devastate => SiegeAftermathResolutionKind.Devastate,
			SiegeAftermathAction.SiegeAftermath.Pillage => SiegeAftermathResolutionKind.Pillage,
			SiegeAftermathAction.SiegeAftermath.ShowMercy => SiegeAftermathResolutionKind.ShowMercy,
			_ => SiegeAftermathResolutionKind.Unknown
		};
	}

	private static SiegeAftermathAction.SiegeAftermath ToNativeAftermathKind(SiegeAftermathResolutionKind aftermath)
	{
		return aftermath switch
		{
			SiegeAftermathResolutionKind.Devastate => SiegeAftermathAction.SiegeAftermath.Devastate,
			SiegeAftermathResolutionKind.Pillage => SiegeAftermathAction.SiegeAftermath.Pillage,
			SiegeAftermathResolutionKind.ShowMercy => SiegeAftermathAction.SiegeAftermath.ShowMercy,
			_ => SiegeAftermathAction.SiegeAftermath.ShowMercy
		};
	}

	private static void ApplyPendingMarketLootForFinalAftermath(SiegeAftermathAction.SiegeAftermath aftermath)
	{
		try
		{
			if (aftermath == SiegeAftermathAction.SiegeAftermath.Pillage)
			{
				if (!_marketGoldLootApplied)
				{
					_marketGoldLootApplied = true;
					LootSettlementMarketGold("搜掠结算");
				}
				if (!_marketGoodsLootAppliedForPlunder)
				{
					_marketGoodsLootAppliedForPlunder = true;
					LootSettlementMarketInventory(0.20f, 0.50f, "搜掠结算");
				}
			}
			else if (aftermath == SiegeAftermathAction.SiegeAftermath.Devastate)
			{
				if (!_marketGoldLootApplied)
				{
					_marketGoldLootApplied = true;
					LootSettlementMarketGold("血洗结算");
				}
				if (!_marketGoodsLootAppliedForMassacre)
				{
					_marketGoodsLootAppliedForMassacre = true;
					LootSettlementMarketInventory(0.90f, 1.00f, "血洗结算");
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyPendingMarketLootForFinalAftermath failed: " + ex.Message);
		}
	}

	private static bool FinalizePendingAftermath(string reason)
	{
		if (!_hasPendingAftermath)
		{
			return false;
		}
		Settlement settlement = ResolveCurrentSettlement();
		if (settlement == null || settlement.Town == null)
		{
			return false;
		}
		try
		{
			CaptureNativeSiegeContext(settlement);
			MobileParty attackerParty = _besiegerParty ?? MobileParty.MainParty;
			Clan previousOwner = _previousSettlementOwnerClan ?? settlement.OwnerClan;
			if (previousOwner?.Leader == null)
			{
				previousOwner = (settlement.OwnerClan?.Leader != null) ? settlement.OwnerClan : attackerParty?.ActualClan;
			}
			Dictionary<MobileParty, float> contributions = BuildSafePartyContributions(attackerParty);
			SiegeAftermathAction.SiegeAftermath aftermath = _pendingAftermath;
			if (aftermath == SiegeAftermathAction.SiegeAftermath.Pillage || aftermath == SiegeAftermathAction.SiegeAftermath.Devastate)
			{
				ReturnSharedCivilianReliefPoolToPlayerForNegativeOutcome(reason ?? aftermath.ToString());
			}
			float prosperityBefore = settlement.Town.Prosperity;
			Dictionary<Hero, float> notablePowerBefore = CaptureNotablePowers(settlement);
			SiegeAftermathAction.ApplyAftermath(attackerParty, settlement, aftermath, previousOwner, contributions);
			ApplySoldierAppeasementMoralePenaltyIfNeeded(aftermath);
			if (aftermath == SiegeAftermathAction.SiegeAftermath.Devastate && _massacreStarted)
			{
				ApplyMassacreExtraDevastatePenalty(settlement, prosperityBefore, notablePowerBefore);
				_nativeDevastateAftermathFlowActive = true;
				_nativeDevastateSummaryContinueHandled = false;
			}
			if (aftermath == SiegeAftermathAction.SiegeAftermath.Devastate && _culturalRepopulationRequested)
			{
				ApplyCulturalRepopulationNow("finalize_aftermath");
			}
			if (aftermath == SiegeAftermathAction.SiegeAftermath.ShowMercy && settlement.Town != null)
			{
				settlement.Town.Loyalty += MercyInterventionLoyaltyBonus;
			}
			if (aftermath == SiegeAftermathAction.SiegeAftermath.Pillage && _plunderStarted)
			{
				SiegeDestructiveChoiceProfile plunderProfile = SiegeDestructiveChoiceProfile.BuildPlunder();
				AdjustSettlementPublicTrustOnly(settlement, plunderProfile.FinalizedPublicTrustDelta, plunderProfile.FinalizedPublicTrustReason);
			}
			ApplyPendingMarketLootForFinalAftermath(aftermath);
			TrySetNativePlayerEncounterAftermathForSummary(aftermath);
			MyBehavior.Instance?.RecordAnimusForgeSiegeInterventionForExternal(attackerParty, settlement, aftermath, previousOwner, _pendingAftermathTrigger, _pendingAftermathDetail, Math.Min(AutoSummonCount, CountHealthyMainPartySoldiers()), _lastLootItemTotal, _lastLootStackKinds, _lastLootValue, _lastMarketGoldLoot, _lastCivilianGoldLoot, _lastCivilianTargetsLooted, _lastKilledCivilianUnits, _lastKilledNotables, _plunderStarted, _massacreStarted);
			_pendingSummaryAftermath = aftermath;
			MarkAftermathResolvedForCompletion(settlement, aftermath);
			PrepareCompletedInterventionSummary(aftermath);
			_hasPendingAftermath = false;
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "FinalizePendingAftermath failed: " + ex);
			return false;
		}
	}

	private static Dictionary<MobileParty, float> BuildSafePartyContributions(MobileParty attackerParty)
	{
		Dictionary<MobileParty, float> result = new Dictionary<MobileParty, float>();
		try
		{
			if (_partyContributions != null)
			{
				foreach (KeyValuePair<MobileParty, float> item in _partyContributions)
				{
					if (item.Key != null && item.Value > 0f && !result.ContainsKey(item.Key))
					{
						result.Add(item.Key, item.Value);
					}
				}
			}
			if (result.Count == 0 && attackerParty != null)
			{
				result[attackerParty] = 100f;
			}
		}
		catch
		{
		}
		return result;
	}

	private static Dictionary<Hero, float> CaptureNotablePowers(Settlement settlement)
	{
		Dictionary<Hero, float> result = new Dictionary<Hero, float>();
		try
		{
			if (settlement?.Notables == null)
			{
				return result;
			}
			foreach (Hero notable in settlement.Notables)
			{
				if (notable != null && !result.ContainsKey(notable))
				{
					result[notable] = notable.Power;
				}
			}
		}
		catch
		{
		}
		return result;
	}

	private static void ApplyMassacreExtraDevastatePenalty(Settlement settlement, float prosperityBefore, Dictionary<Hero, float> notablePowerBefore)
	{
		try
		{
			if (settlement?.Town == null)
			{
				return;
			}
			const float extraMultiplier = 0.8f;
			float prosperityAfterNative = settlement.Town.Prosperity;
			float nativeProsperityDelta = prosperityAfterNative - prosperityBefore;
			float extraProsperityDelta = 0f;
			if (nativeProsperityDelta < 0f)
			{
				extraProsperityDelta = nativeProsperityDelta * extraMultiplier;
				settlement.Town.Prosperity = MathF.Max(0f, settlement.Town.Prosperity + extraProsperityDelta);
			}
			float extraLoyaltyDelta = -30f * extraMultiplier;
			settlement.Town.Loyalty += extraLoyaltyDelta;
			int notableAdjusted = 0;
			if (notablePowerBefore != null && notablePowerBefore.Count > 0)
			{
				foreach (KeyValuePair<Hero, float> item in notablePowerBefore)
				{
					Hero notable = item.Key;
					if (notable == null)
					{
						continue;
					}
					float originalPower = item.Value;
					float targetPower = MathF.Max(0f, originalPower * 0.10f);
					float delta = targetPower - notable.Power;
					if (Math.Abs(delta) > 0.01f)
					{
						notable.AddPower(delta);
						notableAdjusted++;
					}
				}
			}
			Logger.Log("SiegeAiIntervention", $"Applied AF massacre extra Devastate penalty x1.8. Settlement={settlement.StringId}, NativeProsperityDelta={nativeProsperityDelta:0.##}, ExtraProsperityDelta={extraProsperityDelta:0.##}, ExtraLoyaltyDelta={extraLoyaltyDelta:0.##}, NotablesAdjusted={notableAdjusted}");
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ApplyMassacreExtraDevastatePenalty failed: " + ex.Message);
		}
	}

	private static void CaptureNativeSiegeContext(Settlement settlement)
	{
		try
		{
			SiegeAftermathCampaignBehavior behavior = Campaign.Current?.GetCampaignBehavior<SiegeAftermathCampaignBehavior>();
			if (behavior == null)
			{
				_besiegerParty = _besiegerParty ?? MobileParty.MainParty;
				_previousSettlementOwnerClan = _previousSettlementOwnerClan ?? settlement?.OwnerClan;
				return;
			}
			Type type = typeof(SiegeAftermathCampaignBehavior);
			_besiegerParty = ReadPrivateField<MobileParty>(behavior, type, "_besiegerParty") ?? _besiegerParty ?? MobileParty.MainParty;
			_previousSettlementOwnerClan = ReadPrivateField<Clan>(behavior, type, "_prevSettlementOwnerClan") ?? _previousSettlementOwnerClan ?? settlement?.OwnerClan;
			Dictionary<MobileParty, float> contributions = ReadPrivateField<Dictionary<MobileParty, float>>(behavior, type, "_siegeEventPartyContributions");
			if (contributions != null && contributions.Count > 0)
			{
				_partyContributions = new Dictionary<MobileParty, float>(contributions);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "CaptureNativeSiegeContext failed: " + ex.Message);
			_besiegerParty = _besiegerParty ?? MobileParty.MainParty;
			_previousSettlementOwnerClan = _previousSettlementOwnerClan ?? settlement?.OwnerClan;
		}
	}

	private static T ReadPrivateField<T>(object instance, Type type, string fieldName) where T : class
	{
		try
		{
			return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(instance) as T;
		}
		catch
		{
			return null;
		}
	}

	private static void TrySetNativePlayerEncounterAftermathForSummary(SiegeAftermathAction.SiegeAftermath aftermath)
	{
		try
		{
			SiegeAftermathCampaignBehavior behavior = Campaign.Current?.GetCampaignBehavior<SiegeAftermathCampaignBehavior>();
			if (behavior == null)
			{
				return;
			}
			typeof(SiegeAftermathCampaignBehavior).GetField("_playerEncounterAftermath", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(behavior, aftermath);
			typeof(SiegeAftermathCampaignBehavior).GetField("_besiegerParty", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(behavior, _besiegerParty ?? MobileParty.MainParty);
			typeof(SiegeAftermathCampaignBehavior).GetField("_prevSettlementOwnerClan", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(behavior, _previousSettlementOwnerClan ?? ResolveCurrentSettlement()?.OwnerClan);
			typeof(SiegeAftermathCampaignBehavior).GetField("_siegeEventPartyContributions", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(behavior, BuildSafePartyContributions(_besiegerParty ?? MobileParty.MainParty));
		}
		catch
		{
		}
	}

	private static void MarkAftermathResolvedForCompletion(Settlement settlement, SiegeAftermathAction.SiegeAftermath aftermath)
	{
		try
		{
			_afAftermathResolved = true;
			_completedAftermath = aftermath;
			_completedSettlementId = settlement?.StringId ?? _activeSettlementId ?? "";
			_completedSettlementName = settlement?.Name?.ToString() ?? _activeSettlementName ?? "";
			Logger.Log("SiegeAiIntervention", $"Marked AF siege aftermath resolved. Settlement={_completedSettlementId}, Aftermath={aftermath}");
		}
		catch
		{
			_afAftermathResolved = true;
			_completedAftermath = aftermath;
		}
	}

	private static void PrepareCompletedInterventionSummary(SiegeAftermathAction.SiegeAftermath aftermath)
	{
		try
		{
			string settlementName = string.IsNullOrWhiteSpace(_completedSettlementName) ? _activeSettlementName : _completedSettlementName;
			if (string.IsNullOrWhiteSpace(settlementName))
			{
				settlementName = ResolveCurrentSettlement()?.Name?.ToString() ?? "这座定居点";
			}
			bool culturalRepopulationApplied = _culturalRepopulationRequested || _culturalRepopulationApplied;
			string targetCultureText = "";
			if (culturalRepopulationApplied)
			{
				CultureObject targetCulture = ResolveCulturalRepopulationTargetCulture(out string targetCultureSource);
				targetCultureText = DescribeCultureForMessage(targetCulture, targetCultureSource);
			}
			_completedSummaryText = SiegeCompletedInterventionSummaryBuilder.Build(new SiegeCompletedInterventionSummaryFacts(
				settlementName,
				ToStandaloneAftermathKind(aftermath),
				culturalRepopulationApplied,
				_massacreStarted,
				_plunderStarted,
				targetCultureText,
				_lastLootItemTotal,
				_lastLootStackKinds,
				_lastLootValue,
				_lastMarketGoldLoot,
				_lastCivilianGoldLoot,
				_lastCivilianTargetsLooted));
		}
		catch
		{
			_completedSummaryText = "攻城后的入城处置已经完成，正在结束本次攻城遭遇。";
		}
	}

	private static void FinishPlayerEncounterAfterIntervention(SiegeAftermathAction.SiegeAftermath aftermath)
	{
		try
		{
			QueueEncounterFinishAfterIntervention(aftermath, "af_done_menu_continue", 0, forceDelay: true);
			TryFinishPlayerEncounterAfterInterventionNow(aftermath, "af_done_menu_continue");
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "FinishPlayerEncounterAfterIntervention failed: " + ex.Message);
		}
	}

	private static void QueueEncounterFinishAfterIntervention(SiegeAftermathAction.SiegeAftermath aftermath, string reason, int delayTicks, bool forceDelay)
	{
		try
		{
			if (!_pendingEncounterFinish)
			{
				_pendingEncounterFinish = true;
				_pendingEncounterFinishAttempts = 0;
				_pendingEncounterFinishMessageShown = false;
			}
			_pendingEncounterFinishAftermath = aftermath;
			if (forceDelay || _pendingEncounterFinishDelayTicks <= 0)
			{
				_pendingEncounterFinishDelayTicks = Math.Max(0, delayTicks);
			}
			Logger.Log("SiegeAiIntervention", "Queued AF siege encounter finish. Reason=" + (reason ?? "N/A") + ", Aftermath=" + aftermath + ", DelayTicks=" + _pendingEncounterFinishDelayTicks);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "QueueEncounterFinishAfterIntervention failed: " + ex.Message);
		}
	}

	private static bool TryFinishPlayerEncounterAfterInterventionNow(SiegeAftermathAction.SiegeAftermath aftermath, string source)
	{
		try
		{
			if (!_pendingEncounterFinish)
			{
				QueueEncounterFinishAfterIntervention(aftermath, source, 0, forceDelay: true);
			}
			if (_pendingEncounterFinishDelayTicks > 0)
			{
				_pendingEncounterFinishDelayTicks--;
				return false;
			}
			ShowEncounterFinishMessagesOnce(aftermath);
			_pendingEncounterFinishAttempts++;
			try
			{
				PlayerEncounter.LeaveEncounter = true;
			}
			catch
			{
			}
			try
			{
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.Current.IsPlayerWaiting = false;
				}
			}
			catch
			{
			}
			try
			{
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.Finish(true);
					Logger.Log("SiegeAiIntervention", "Requested PlayerEncounter.Finish(true) after AF intervention. Source=" + (source ?? "N/A") + ", Attempt=" + _pendingEncounterFinishAttempts);
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "PlayerEncounter.Finish after AF intervention failed. Source=" + (source ?? "N/A") + ", Error=" + ex.Message);
			}
			if (PlayerEncounter.Current == null)
			{
				Logger.Log("SiegeAiIntervention", "AF siege encounter finish completed. Source=" + (source ?? "N/A"));
				return true;
			}
			if (_pendingEncounterFinishAttempts >= 3)
			{
				try
				{
					GameMenu.ExitToLast();
					Logger.Log("SiegeAiIntervention", "Fallback GameMenu.ExitToLast after AF intervention finish attempts. Source=" + (source ?? "N/A") + ", Attempt=" + _pendingEncounterFinishAttempts);
				}
				catch
				{
				}
			}
			return false;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryFinishPlayerEncounterAfterInterventionNow failed. Source=" + (source ?? "N/A") + ", Error=" + ex.Message);
			return false;
		}
	}

	private static void QueueDirectMassacreAftermathScript(string reason)
	{
		try
		{
			_directMassacreAftermathScriptPending = true;
			_directMassacreLootScreenOpened = false;
			_directMassacreWaitingForLootClose = false;
			_directMassacreScriptMessageShown = false;
			_directMassacreScriptTicks = 0;
			_directMassacreLastDeferKey = "";
			_pendingSummarySwitch = true;
			_pendingEncounterFinish = false;
			_pendingEncounterFinishDelayTicks = 0;
			_pendingEncounterFinishAttempts = 0;
			_nativeDevastateAftermathFlowActive = false;
			_nativeDevastateSummaryContinueHandled = true;
			Logger.Log("SiegeAiIntervention", "Queued direct AF massacre aftermath script. Reason=" + (reason ?? "N/A") + ", LootItems=" + (_pendingLootRoster?.Count ?? 0) + ", MarketGold=" + _lastMarketGoldLoot + ", CivilianGold=" + _lastCivilianGoldLoot);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "QueueDirectMassacreAftermathScript failed: " + ex.Message);
		}
	}

	private static bool TryRunDirectMassacreAftermathScript(string source = "campaign_tick_direct_massacre_script")
	{
		if (!_directMassacreAftermathScriptPending || Mission.Current != null)
		{
			return false;
		}
		try
		{
			_directMassacreScriptTicks++;
			if (_hasPendingAftermath)
			{
				FinalizePendingAftermath("direct_massacre_script_pending_aftermath");
			}
			if (!_afAftermathResolved)
			{
				return true;
			}
			string pumpSource = source ?? "direct_massacre_script";
			if (!IsSafeToOpenDirectMassacreLootScreen(pumpSource))
			{
				return true;
			}
			_nativeDevastateAftermathFlowActive = false;
			_nativeDevastateSummaryContinueHandled = true;
			try
			{
				if (Campaign.Current?.CurrentMenuContext != null)
				{
					GameMenu.ExitToLast();
				}
			}
			catch
			{
			}
			if (_directMassacreWaitingForLootClose)
			{
				if (Game.Current?.GameStateManager?.ActiveState is InventoryState)
				{
					return true;
				}
				_directMassacreWaitingForLootClose = false;
				QueueEncounterFinishAfterIntervention(SiegeAftermathAction.SiegeAftermath.Devastate, "direct_massacre_script_after_loot", 0, forceDelay: true);
				if (!TryFinishPlayerEncounterAfterInterventionNow(SiegeAftermathAction.SiegeAftermath.Devastate, "direct_massacre_script_after_loot"))
				{
					return true;
				}
				_directMassacreAftermathScriptPending = false;
				_pendingSummarySwitch = false;
				ClearActiveState(preserveSummarySwitch: false);
				Logger.Log("SiegeAiIntervention", "Direct AF massacre aftermath script completed after loot screen.");
				return true;
			}
			if (!_directMassacreLootScreenOpened && _pendingLootRoster != null && _pendingLootRoster.Count > 0)
			{
				TryOpenDirectMassacreLootScreenNow(pumpSource);
				return true;
			}
			ShowDirectMassacreLootMessage();
			QueueEncounterFinishAfterIntervention(SiegeAftermathAction.SiegeAftermath.Devastate, "direct_massacre_script_no_loot", 0, forceDelay: true);
			if (!TryFinishPlayerEncounterAfterInterventionNow(SiegeAftermathAction.SiegeAftermath.Devastate, "direct_massacre_script_no_loot"))
			{
				return true;
			}
			_directMassacreAftermathScriptPending = false;
			_pendingSummarySwitch = false;
			ClearActiveState(preserveSummarySwitch: false);
			Logger.Log("SiegeAiIntervention", "Direct AF massacre aftermath script completed without loot screen.");
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryRunDirectMassacreAftermathScript failed: " + ex.Message);
			return true;
		}
	}

	private static void QueueDirectPlunderAftermathScript(string reason)
	{
		try
		{
			_directPlunderAftermathScriptPending = true;
			_directPlunderLootScreenOpened = false;
			_directPlunderWaitingForLootClose = false;
			_directPlunderScriptMessageShown = false;
			_directPlunderScriptTicks = 0;
			_directPlunderLastDeferKey = "";
			_pendingSummarySwitch = true;
			_pendingEncounterFinish = false;
			_pendingEncounterFinishDelayTicks = 0;
			_pendingEncounterFinishAttempts = 0;
			Logger.Log("SiegeAiIntervention", "Queued direct AF plunder aftermath loot script. Reason=" + (reason ?? "N/A") + ", LootItems=" + (_pendingLootRoster?.Count ?? 0) + ", MarketGold=" + _lastMarketGoldLoot + ", CivilianGold=" + _lastCivilianGoldLoot);
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "QueueDirectPlunderAftermathScript failed: " + ex.Message);
		}
	}

	private static bool TryRunDirectPlunderAftermathScript(string source = "campaign_tick_direct_plunder_script")
	{
		if (!_directPlunderAftermathScriptPending || Mission.Current != null)
		{
			return false;
		}
		try
		{
			_directPlunderScriptTicks++;
			if (_hasPendingAftermath)
			{
				FinalizePendingAftermath("direct_plunder_script_pending_aftermath");
			}
			if (!_afAftermathResolved)
			{
				return true;
			}
			string pumpSource = source ?? "direct_plunder_script";
			if (!IsSafeToOpenDirectPlunderLootScreen(pumpSource))
			{
				return true;
			}
			try
			{
				if (Campaign.Current?.CurrentMenuContext != null)
				{
					GameMenu.ExitToLast();
				}
			}
			catch
			{
			}
			if (_directPlunderWaitingForLootClose)
			{
				if (Game.Current?.GameStateManager?.ActiveState is InventoryState)
				{
					return true;
				}
				_directPlunderWaitingForLootClose = false;
				QueueEncounterFinishAfterIntervention(SiegeAftermathAction.SiegeAftermath.Pillage, "direct_plunder_script_after_loot", 0, forceDelay: true);
				if (!TryFinishPlayerEncounterAfterInterventionNow(SiegeAftermathAction.SiegeAftermath.Pillage, "direct_plunder_script_after_loot"))
				{
					return true;
				}
				_directPlunderAftermathScriptPending = false;
				_pendingSummarySwitch = false;
				ClearActiveState(preserveSummarySwitch: false);
				Logger.Log("SiegeAiIntervention", "Direct AF plunder aftermath loot script completed after loot screen.");
				return true;
			}
			if (!_directPlunderLootScreenOpened && _pendingLootRoster != null && _pendingLootRoster.Count > 0)
			{
				TryOpenDirectPlunderLootScreenNow(pumpSource);
				return true;
			}
			ShowDirectPlunderLootMessage();
			QueueEncounterFinishAfterIntervention(SiegeAftermathAction.SiegeAftermath.Pillage, "direct_plunder_script_no_loot", 0, forceDelay: true);
			if (!TryFinishPlayerEncounterAfterInterventionNow(SiegeAftermathAction.SiegeAftermath.Pillage, "direct_plunder_script_no_loot"))
			{
				return true;
			}
			_directPlunderAftermathScriptPending = false;
			_pendingSummarySwitch = false;
			ClearActiveState(preserveSummarySwitch: false);
			Logger.Log("SiegeAiIntervention", "Direct AF plunder aftermath loot script completed without loot screen.");
			return true;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryRunDirectPlunderAftermathScript failed: " + ex.Message);
			return true;
		}
	}

	internal static bool TryPumpDirectPlunderAftermathScriptForExternal(string source)
	{
		try
		{
			if (!_directPlunderAftermathScriptPending)
			{
				return false;
			}
			return TryRunDirectPlunderAftermathScript(source ?? "external_direct_plunder_script");
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "TryPumpDirectPlunderAftermathScriptForExternal failed. Source=" + (source ?? "N/A") + ", Error=" + ex.Message);
			return true;
		}
	}

	private static bool TryOpenDirectMassacreLootScreenNow(string source)
	{
		try
		{
			if (!_directMassacreAftermathScriptPending || _directMassacreLootScreenOpened || _pendingLootRoster == null || _pendingLootRoster.Count <= 0)
			{
				return false;
			}
			if (!IsSafeToOpenDirectMassacreLootScreen(source))
			{
				return false;
			}
			_directMassacreLootScreenOpened = true;
			_directMassacreWaitingForLootClose = true;
			_pendingLootScreen = true;
			_pendingLootScreenShown = true;
			_directMassacreLastDeferKey = "";
			_nativeDevastateAftermathFlowActive = false;
			_nativeDevastateSummaryContinueHandled = true;
			ShowDirectMassacreLootMessage();
			try
			{
				if (Campaign.Current?.CurrentMenuContext != null)
				{
					GameMenu.ExitToLast();
				}
			}
			catch
			{
			}
			InventoryScreenHelper.OpenScreenAsLoot(new Dictionary<PartyBase, ItemRoster>
			{
				{
					PartyBase.MainParty,
					_pendingLootRoster
				}
			});
			Logger.Log("SiegeAiIntervention", "Direct AF massacre script opened loot screen immediately. Source=" + (source ?? "N/A") + ", LootItems=" + _pendingLootRoster.Count + ", MarketGold=" + _lastMarketGoldLoot + ", CivilianGold=" + _lastCivilianGoldLoot);
			return true;
		}
		catch (Exception ex)
		{
			_directMassacreLootScreenOpened = false;
			_directMassacreWaitingForLootClose = false;
			_pendingLootScreenShown = false;
			Logger.Log("SiegeAiIntervention", "TryOpenDirectMassacreLootScreenNow failed. Source=" + (source ?? "N/A") + ", Error=" + ex.Message);
			return false;
		}
	}

	private static bool TryOpenDirectPlunderLootScreenNow(string source)
	{
		try
		{
			if (!_directPlunderAftermathScriptPending || _directPlunderLootScreenOpened || _pendingLootRoster == null || _pendingLootRoster.Count <= 0)
			{
				return false;
			}
			if (!IsSafeToOpenDirectPlunderLootScreen(source))
			{
				return false;
			}
			_directPlunderLootScreenOpened = true;
			_directPlunderWaitingForLootClose = true;
			_pendingLootScreen = true;
			_pendingLootScreenShown = true;
			_directPlunderLastDeferKey = "";
			ShowDirectPlunderLootMessage();
			try
			{
				if (Campaign.Current?.CurrentMenuContext != null)
				{
					GameMenu.ExitToLast();
				}
			}
			catch
			{
			}
			InventoryScreenHelper.OpenScreenAsLoot(new Dictionary<PartyBase, ItemRoster>
			{
				{
					PartyBase.MainParty,
					_pendingLootRoster
				}
			});
			Logger.Log("SiegeAiIntervention", "Direct AF plunder script opened loot screen immediately. Source=" + (source ?? "N/A") + ", LootItems=" + _pendingLootRoster.Count + ", MarketGold=" + _lastMarketGoldLoot + ", CivilianGold=" + _lastCivilianGoldLoot);
			return true;
		}
		catch (Exception ex)
		{
			_directPlunderLootScreenOpened = false;
			_directPlunderWaitingForLootClose = false;
			_pendingLootScreenShown = false;
			Logger.Log("SiegeAiIntervention", "TryOpenDirectPlunderLootScreenNow failed. Source=" + (source ?? "N/A") + ", Error=" + ex.Message);
			return false;
		}
	}

	private static bool IsSafeToOpenDirectMassacreLootScreen(string source)
	{
		try
		{
			if (Mission.Current != null)
			{
				LogDirectMassacreLootDeferOnce("mission_current", "Direct massacre loot screen deferred because Mission.Current is still active. Source=" + (source ?? "N/A"));
				return false;
			}
			object activeState = Game.Current?.GameStateManager?.ActiveState;
			if (activeState == null)
			{
				LogDirectMassacreLootDeferOnce("state_null", "Direct massacre loot screen deferred because active game state is null. Source=" + (source ?? "N/A"));
				return false;
			}
			if (activeState is InventoryState)
			{
				return false;
			}
			if (activeState is MapState)
			{
				return true;
			}
			string stateName = activeState.GetType().FullName ?? activeState.GetType().Name;
			LogDirectMassacreLootDeferOnce("state:" + stateName, "Direct massacre loot screen deferred until MapState. Source=" + (source ?? "N/A") + ", ActiveState=" + stateName);
			return false;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "IsSafeToOpenDirectMassacreLootScreen failed. Source=" + (source ?? "N/A") + ", Error=" + ex.Message);
			return false;
		}
	}

	private static bool IsSafeToOpenDirectPlunderLootScreen(string source)
	{
		try
		{
			if (Mission.Current != null)
			{
				LogDirectPlunderLootDeferOnce("mission_current", "Direct plunder loot screen deferred because Mission.Current is still active. Source=" + (source ?? "N/A"));
				return false;
			}
			object activeState = Game.Current?.GameStateManager?.ActiveState;
			if (activeState == null)
			{
				LogDirectPlunderLootDeferOnce("state_null", "Direct plunder loot screen deferred because active game state is null. Source=" + (source ?? "N/A"));
				return false;
			}
			if (activeState is InventoryState)
			{
				return false;
			}
			if (activeState is MapState)
			{
				return true;
			}
			string stateName = activeState.GetType().FullName ?? activeState.GetType().Name;
			LogDirectPlunderLootDeferOnce("state:" + stateName, "Direct plunder loot screen deferred until MapState. Source=" + (source ?? "N/A") + ", ActiveState=" + stateName);
			return false;
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "IsSafeToOpenDirectPlunderLootScreen failed. Source=" + (source ?? "N/A") + ", Error=" + ex.Message);
			return false;
		}
	}

	private static void LogDirectMassacreLootDeferOnce(string key, string message)
	{
		try
		{
			if (string.Equals(_directMassacreLastDeferKey, key ?? "", StringComparison.Ordinal))
			{
				return;
			}
			_directMassacreLastDeferKey = key ?? "";
			Logger.Log("SiegeAiIntervention", message);
		}
		catch
		{
		}
	}

	private static void LogDirectPlunderLootDeferOnce(string key, string message)
	{
		try
		{
			if (string.Equals(_directPlunderLastDeferKey, key ?? "", StringComparison.Ordinal))
			{
				return;
			}
			_directPlunderLastDeferKey = key ?? "";
			Logger.Log("SiegeAiIntervention", message);
		}
		catch
		{
		}
	}

	private static void ShowDirectMassacreLootMessage()
	{
		if (_directMassacreScriptMessageShown)
		{
			return;
		}
		if (_culturalRepopulationRequested && !_culturalRepopulationApplied)
		{
			ApplyCulturalRepopulationNow("direct_massacre_loot_message");
		}
		_directMassacreScriptMessageShown = true;
		try
		{
			string action = _culturalRepopulationRequested || _culturalRepopulationApplied ? "屠民迁殖" : "血洗";
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】" + action + "已按毁坏处置结算；城镇受到进一步毁坏影响。即将进入战利品界面领取截获物资。", Color.FromUint(0xFFFF7777u)));
			InformationManager.DisplayMessage(new InformationMessage("【战利清点】金钱已入账：市场金库 " + _lastMarketGoldLoot + "，民众第纳尔 " + _lastCivilianGoldLoot + "；物资 " + _lastLootItemTotal + " 件 / " + _lastLootStackKinds + " 类。", Color.FromUint(0xFFFFC46Bu)));
		}
		catch
		{
		}
	}

	private static void ShowDirectPlunderLootMessage()
	{
		if (_directPlunderScriptMessageShown)
		{
			return;
		}
		_directPlunderScriptMessageShown = true;
		try
		{
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】搜掠已按掠夺处置结算；即将进入战利品界面领取截获物资。", Color.FromUint(0xFFFFC46Bu)));
			InformationManager.DisplayMessage(new InformationMessage("【战利清点】金钱已入账：市场金库 " + _lastMarketGoldLoot + "，民众第纳尔 " + _lastCivilianGoldLoot + "；物资 " + _lastLootItemTotal + " 件 / " + _lastLootStackKinds + " 类。", Color.FromUint(0xFFFFC46Bu)));
		}
		catch
		{
		}
	}

	private static void ShowEncounterFinishMessagesOnce(SiegeAftermathAction.SiegeAftermath aftermath)
	{
		if (_pendingEncounterFinishMessageShown)
		{
			return;
		}
		_pendingEncounterFinishMessageShown = true;
		string label = aftermath switch
		{
			SiegeAftermathAction.SiegeAftermath.Devastate => _culturalRepopulationRequested || _culturalRepopulationApplied ? "屠民迁殖" : "血洗/毁坏",
			SiegeAftermathAction.SiegeAftermath.Pillage => "搜掠",
			SiegeAftermathAction.SiegeAftermath.ShowMercy => "安抚",
			_ => "处置"
		};
		try
		{
			InformationManager.DisplayMessage(new InformationMessage("【攻城处置】攻城后" + label + "已经结算完成，正在结束攻城遭遇。", Color.FromUint(0xFFB6F7A8u)));
			if (_lastLootItemTotal > 0 || _lastMarketGoldLoot > 0 || _lastCivilianGoldLoot > 0)
			{
				InformationManager.DisplayMessage(new InformationMessage("【战利清点】结算：市场物资 " + _lastLootItemTotal + " 件 / " + _lastLootStackKinds + " 类，市场金库 " + _lastMarketGoldLoot + "，民众第纳尔 " + _lastCivilianGoldLoot + "。", Color.FromUint(0xFFB6F7A8u)));
			}
		}
		catch
		{
		}
		try
		{
			MBInformationManager.AddQuickInformation(new TextObject("攻城后处置已完成，正在离开攻城遭遇。"), 0, null, null, "event:/ui/mission/arena_victory");
		}
		catch
		{
		}
	}

	private static Settlement ResolveCurrentSettlement()
	{
		try
		{
			if (Settlement.CurrentSettlement != null)
			{
				return Settlement.CurrentSettlement;
			}
		}
		catch
		{
		}
		try
		{
			if (PlayerEncounter.EncounterSettlement != null)
			{
				return PlayerEncounter.EncounterSettlement;
			}
		}
		catch
		{
		}
		try
		{
			if (PlayerEncounter.LocationEncounter?.Settlement != null)
			{
				return PlayerEncounter.LocationEncounter.Settlement;
			}
		}
		catch
		{
		}
		try
		{
			if (_activeSettlement != null)
			{
				return _activeSettlement;
			}
		}
		catch
		{
		}
		return null;
	}

	private static Agent TryGetAgent(int agentIndex)
	{
		try
		{
			if (agentIndex < 0 || Mission.Current?.Agents == null)
			{
				return null;
			}
			return Mission.Current.Agents.FirstOrDefault(a => a != null && a.Index == agentIndex);
		}
		catch
		{
			return null;
		}
	}

	private static string StripSiegeTags(string text)
	{
		return AnySiegeTagRegex.Replace(text ?? "", "").Trim();
	}

	private static string BuildTargetKey(Agent agent)
	{
		CharacterObject character = agent?.Character as CharacterObject;
		Hero hero = character?.HeroObject;
		string id = hero?.StringId ?? character?.StringId ?? "agent";
		return (_activeSettlementId ?? "") + ":" + id + ":" + (agent?.Index ?? -1);
	}

	private static int CountHealthyMainPartySoldiers()
	{
		try
		{
			TroopRoster roster = PartyBase.MainParty?.MemberRoster;
			if (roster == null)
			{
				return 0;
			}
			int total = 0;
			for (int i = 0; i < roster.Count; i++)
			{
				TroopRosterElement element = roster.GetElementCopyAtIndex(i);
				CharacterObject character = element.Character;
				if (character != null && !character.IsHero && character != CharacterObject.PlayerCharacter)
				{
					total += Math.Max(0, element.Number - element.WoundedNumber);
				}
			}
			return total;
		}
		catch
		{
			return 0;
		}
	}

	private static bool IsActiveInCurrentMission()
	{
		if (_activeMode == InterventionMode.None || Mission.Current == null)
		{
			return false;
		}
		Settlement settlement = ResolveCurrentSettlement();
		if (!string.IsNullOrWhiteSpace(_activeSettlementId) && settlement != null && !string.Equals(settlement.StringId, _activeSettlementId, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}

	private static void ResetSessionCounters()
	{
		_alliedTroopsAutoSummoned = false;
		_nextControlTickTime = 0f;
		_nextPlunderTickTime = 0f;
		_playerBattleEquipmentApplied = false;
		_plunderStarted = false;
		_massacreStarted = false;
		_massacreVictoryReached = false;
		_civilianSpeechRallyActive = false;
		_civilianGatherPropagationActive = false;
		_civilianFormationControlPending = false;
		_civilianFormationControlComplete = false;
		_civilianFormationControlMessageShown = false;
		_soldierDefaultFollowOrderIssued = false;
		_playerOrderControllerPrimed = false;
		_civilianOrderControllerPrimed = false;
		_selectedInterventionRoster = null;
		_civilianGatherStartedAt = -1f;
		_nextCivilianGatherTickTime = 0f;
		_civilianGatherMessengerSpeechBudget = 0;
		_civilianGatherMessengerSpeechCount = 0;
		_civilianFormationControlNotBeforeTime = -1f;
		_nextCivilianFormationControlBatchTime = 0f;
		_nextPlayerOrderControllerPrimeTime = 0f;
		_culturalRepopulationRequested = false;
		_culturalRepopulationApplied = false;
		_reliefChoiceApplied = false;
		_inspirationLevelApplied = 0;
		_soldierAppeasementCheckDone = false;
		_soldierAppeasementRequired = false;
		_soldierAppeasementApplied = false;
		_soldierAppeasementMoralePenaltyApplied = false;
		_lastMassacreRealKillMissionTime = -100f;
		_hasPendingAftermath = false;
		_pendingAftermathTrigger = "";
		_pendingAftermathDetail = "";
		_marketGoodsLootAppliedForPlunder = false;
		_marketGoodsLootAppliedForMassacre = false;
		_marketGoldLootApplied = false;
		_lastLootItemTotal = 0;
		_lastLootStackKinds = 0;
		_lastLootValue = 0;
		_lastMarketGoldLoot = 0;
		_lastCivilianGoldLoot = 0;
		_lastCivilianTargetsLooted = 0;
		_lastSceneCivilianSpawnedCount = 0;
		_lastKilledCivilianUnits = 0;
		_lastKilledNotables = 0;
		_sharedCivilianReliefGold = 0;
		_sharedCivilianReliefFoodUnits = 0;
		_sharedCivilianReliefItemTotal = 0;
		_sharedCivilianReliefItemValue = 0L;
		_appliedSharedCivilianReliefGold = 0;
		_appliedSharedCivilianReliefFoodUnits = 0;
		_appliedSharedCivilianReliefItemValue = 0L;
		_sharedCivilianReliefReturned = false;
		SharedCivilianReliefItems.Clear();
		SharedCivilianReliefItemObjects.Clear();
		InterventionMemoryEvents.Clear();
		_interventionMemorySequence = 0;
		_pendingLootRoster = new ItemRoster();
		_pendingLootScreen = false;
		_pendingLootScreenShown = false;
		_directPlunderAftermathScriptPending = false;
		_directPlunderLootScreenOpened = false;
		_directPlunderWaitingForLootClose = false;
		_directPlunderScriptMessageShown = false;
		_directPlunderScriptTicks = 0;
		_directPlunderLastDeferKey = "";
		ResetOutcomeMessageDedup();
		_lastForcedPlayerDamageAgentIndex = -1;
		_lastForcedPlayerDamageMissionTime = -100f;
		_playerAttackReleaseSuppressed = false;
		_lastMainAgentAttackStage = null;
		ActivePlunderInteractions.Clear();
		ActiveCivilianGatherInteractions.Clear();
		LootedTargets.Clear();
		AlliedAgentIndexes.Clear();
		CountedMassacreVictims.Clear();
		SceneCivilianAgentIndexes.Clear();
		VictoryCheerAgentIndexes.Clear();
		CordonReadyAgentIndexes.Clear();
		CivilianAssemblySettledAgentIndexes.Clear();
		CivilianCalmedAgentIndexes.Clear();
		CivilianFrightenedActionAgentIndexes.Clear();
		CivilianPreMassacrePreparedAgentIndexes.Clear();
		CivilianGatherMovePreparedAgentIndexes.Clear();
		CivilianGatherFollowerAgentIndexes.Clear();
		CivilianGatherReadyFormationAgentIndexes.Clear();
		CivilianGatherMessengerAgentIndexes.Clear();
		CivilianGatherMessengerSpeechAgentIndexes.Clear();
		CommandableOriginRuntimeIds.Clear();
		MassacreReadySoldierAgentIndexes.Clear();
		MassacreCombatPreparedAgentIndexes.Clear();
		CivilianAssemblySlots.Clear();
		CivilianSpeechRallySlots.Clear();
		LastCordonMoveOrderTimesBySoldier.Clear();
		LastCordonLookOrderTimesBySoldier.Clear();
		CivilianHideTargets.Clear();
		LastCivilianHideOrderTimes.Clear();
		CivilianHideSettledAgentIndexes.Clear();
		CivilianInteriorHidePointPool.Clear();
		CivilianEscapePointPool.Clear();
		_civilianRoutPointPoolSceneName = "";
		LastMassacreSoldierFollowOrderTimes.Clear();
		LastMassacreSoldierTargetOrderTimes.Clear();
		LastCivilianGatherFollowOrderTimes.Clear();
		LastCivilianGatherFollowTargets.Clear();
		_civilianAssemblyPointReady = false;
		_civilianAssemblyMessageShown = false;
		_civilianAssemblySpawnAttempted = false;
		_civilianAssemblyNextSlot = 0;
		_spawnedAssemblyCivilianCount = 0;
		_desiredCivilianAssemblyCount = 0;
		_civilianAssemblyAnchor = Vec3.Zero;
		_civilianAssemblyForward = Vec3.Forward;
		_interventionCivilianEnemyTeam = null;
	}

	private static void ResetAftermathRuntimeGuards(string reason)
	{
		try
		{
			_activeMode = InterventionMode.None;
			_pendingMode = InterventionMode.None;
			_activeSettlementId = "";
			_activeSettlementName = "";
			_activeSettlement = null;
			_previousSettlementOwnerClan = null;
			_besiegerParty = null;
			_interventionPlayerCommandTeam = null;
			_interventionCivilianEnemyTeam = null;
			_partyContributions = new Dictionary<MobileParty, float>();
			_afAftermathResolved = false;
			_completedSettlementId = "";
			_completedSettlementName = "";
			_completedAftermath = SiegeAftermathAction.SiegeAftermath.ShowMercy;
			_completedSummaryText = "";
			_pendingSummarySwitch = false;
			_pendingEncounterFinish = false;
			_pendingEncounterFinishAftermath = SiegeAftermathAction.SiegeAftermath.ShowMercy;
			_pendingEncounterFinishDelayTicks = 0;
			_pendingEncounterFinishAttempts = 0;
			_pendingEncounterFinishMessageShown = false;
			_nativeDevastateAftermathFlowActive = false;
			_nativeDevastateSummaryContinueHandled = false;
			_directMassacreAftermathScriptPending = false;
			_directMassacreLootScreenOpened = false;
			_directMassacreWaitingForLootClose = false;
			_directMassacreScriptMessageShown = false;
			_directMassacreScriptTicks = 0;
			_directMassacreLastDeferKey = "";
			_directPlunderAftermathScriptPending = false;
			_directPlunderLootScreenOpened = false;
			_directPlunderWaitingForLootClose = false;
			_directPlunderScriptMessageShown = false;
			_directPlunderScriptTicks = 0;
			_directPlunderLastDeferKey = "";
			ResetOutcomeMessageDedup();
			ResetSessionCounters();
			Logger.Log("SiegeAiIntervention", "Reset AF siege aftermath runtime guards. Reason=" + (reason ?? "N/A"));
		}
		catch (Exception ex)
		{
			Logger.Log("SiegeAiIntervention", "ResetAftermathRuntimeGuards failed. Reason=" + (reason ?? "N/A") + ", Error=" + ex.Message);
		}
	}

	private static void ClearActiveState(bool preserveSummarySwitch)
	{
		_activeMode = InterventionMode.None;
		_pendingMode = InterventionMode.None;
		_activeSettlementId = "";
		_activeSettlementName = "";
		_previousSettlementOwnerClan = null;
		_besiegerParty = null;
		_activeSettlement = null;
		_interventionPlayerCommandTeam = null;
		_interventionCivilianEnemyTeam = null;
		_partyContributions = new Dictionary<MobileParty, float>();
		if (!preserveSummarySwitch)
		{
			_pendingSummarySwitch = false;
		}
		_pendingEncounterFinish = false;
		_pendingEncounterFinishDelayTicks = 0;
		_pendingEncounterFinishAttempts = 0;
		_pendingEncounterFinishMessageShown = false;
		_nativeDevastateAftermathFlowActive = false;
		_nativeDevastateSummaryContinueHandled = false;
		_directMassacreAftermathScriptPending = false;
		_directMassacreLootScreenOpened = false;
		_directMassacreWaitingForLootClose = false;
		_directMassacreScriptMessageShown = false;
		_directMassacreScriptTicks = 0;
		_directMassacreLastDeferKey = "";
		_directPlunderAftermathScriptPending = false;
		_directPlunderLootScreenOpened = false;
		_directPlunderWaitingForLootClose = false;
		_directPlunderScriptMessageShown = false;
		_directPlunderScriptTicks = 0;
		_directPlunderLastDeferKey = "";
		ResetOutcomeMessageDedup();
		ResetSessionCounters();
	}

	private static class SiegeInterventionCommandOriginPatch
	{
		private static bool _patched;

		internal static void EnsurePatched()
		{
			if (_patched)
			{
				return;
			}
			try
			{
				Harmony harmony = new Harmony("com.AnimusForge.siege_intervention.command_origin");
				MethodInfo partyPrefix = typeof(SiegeInterventionCommandOriginPatch).GetMethod(nameof(SafePartyAgentOriginUnderPlayerCommandPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo forceMarkedPrefix = typeof(SiegeInterventionCommandOriginPatch).GetMethod(nameof(ForceMarkedOriginUnderPlayerCommandPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				PatchGetter(harmony, typeof(PartyAgentOrigin), new HarmonyMethod(partyPrefix));
				PatchGetter(harmony, typeof(SimpleAgentOrigin), new HarmonyMethod(forceMarkedPrefix));
				PatchGetter(harmony, typeof(PartyGroupAgentOrigin), new HarmonyMethod(forceMarkedPrefix));
				_patched = true;
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "SiegeInterventionCommandOriginPatch failed: " + ex.Message);
			}
		}

		private static void PatchGetter(Harmony harmony, Type originType, HarmonyMethod prefix)
		{
			try
			{
				MethodInfo getter = AccessTools.PropertyGetter(originType, nameof(IAgentOriginBase.IsUnderPlayersCommand)) ?? originType.GetProperty(nameof(IAgentOriginBase.IsUnderPlayersCommand), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod(true);
				if (getter != null)
				{
					harmony.Patch(getter, prefix: prefix);
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "Patch IsUnderPlayersCommand getter failed for " + originType?.FullName + ": " + ex.Message);
			}
		}

		private static bool SafePartyAgentOriginUnderPlayerCommandPrefix(PartyAgentOrigin __instance, ref bool __result)
		{
			try
			{
				if (SiegeAiInterventionBehavior.ShouldForceCommandForOrigin(__instance))
				{
					__result = true;
					return false;
				}
				if (__instance == null)
				{
					__result = false;
					return false;
				}
				PartyBase party = __instance.Party;
				if (party == null)
				{
					__result = false;
					return false;
				}
				__result = party == PartyBase.MainParty || party.Owner == Hero.MainHero || (party.MapFaction != null && party.MapFaction.Leader == Hero.MainHero);
				return false;
			}
			catch
			{
				__result = false;
				return false;
			}
		}

		private static bool ForceMarkedOriginUnderPlayerCommandPrefix(object __instance, ref bool __result)
		{
			if (SiegeAiInterventionBehavior.ShouldForceCommandForOrigin(__instance))
			{
				__result = true;
				return false;
			}
			return true;
		}
	}

	private static class SiegeInterventionSceneTauntSuppressionPatch
	{
		private static bool _patched;

		internal static void EnsurePatched()
		{
			if (_patched)
			{
				return;
			}
			try
			{
				Harmony harmony = new Harmony("com.AnimusForge.siege_intervention.scene_taunt_suppression");
				PatchMovementOrderConstructors(harmony);
				MethodInfo prefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SuppressSceneTauntMissionBehaviorPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				PatchSceneTauntMissionMethod(harmony, prefix, nameof(SceneTauntMissionBehavior.OnMissionTick));
				PatchSceneTauntMissionMethod(harmony, prefix, nameof(SceneTauntMissionBehavior.OnAgentHit));
				PatchSceneTauntMissionMethod(harmony, prefix, nameof(SceneTauntMissionBehavior.OnScoreHit));
				PatchSceneTauntMissionMethod(harmony, prefix, nameof(SceneTauntMissionBehavior.OnAgentRemoved));
				MethodInfo sceneTauntCrimePrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SuppressSceneTauntCrimePrefix), BindingFlags.Static | BindingFlags.NonPublic);
				PatchSceneTauntStaticMethod(harmony, sceneTauntCrimePrefix, nameof(SceneTauntBehavior.QueueDeferredCrimeForExternal));
				PatchSceneTauntStaticMethod(harmony, sceneTauntCrimePrefix, nameof(SceneTauntBehavior.AddCrimeRefillReserveForExternal));
				PatchSceneTauntStaticMethod(harmony, sceneTauntCrimePrefix, nameof(SceneTauntBehavior.TryShowTrackedCrimeTotalMessageForExternal));
				MethodInfo crimePrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SuppressCrimeRatingDuringInterventionPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo crimeTarget = AccessTools.Method(typeof(ChangeCrimeRatingAction), "ApplyInternal");
				if (crimeTarget != null && crimePrefix != null)
				{
					harmony.Patch(crimeTarget, prefix: new HarmonyMethod(crimePrefix));
				}
				MethodInfo usableTargetPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SuppressCivilianUsableTargetPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo usableTarget = AccessTools.Method(typeof(AgentNavigator), nameof(AgentNavigator.SetTarget), new Type[] { typeof(UsableMachine), typeof(bool), typeof(Agent.AIScriptedFrameFlags) });
				if (usableTarget != null && usableTargetPrefix != null)
				{
					harmony.Patch(usableTarget, prefix: new HarmonyMethod(usableTargetPrefix));
				}
				MethodInfo usableMovePrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SuppressCivilianUsableMissionObjectPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo usableMoveTarget = AccessTools.Method(typeof(UsableMissionObject), "OnAIMoveToUse", new Type[] { typeof(Agent), typeof(IDetachment) });
				if (usableMoveTarget != null && usableMovePrefix != null)
				{
					harmony.Patch(usableMoveTarget, prefix: new HarmonyMethod(usableMovePrefix));
				}
				MethodInfo orderViewsPostfix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(InjectInterventionOrderViewsPostfix), BindingFlags.Static | BindingFlags.NonPublic);
				PatchOrderViewsMethod(harmony, orderViewsPostfix, nameof(SandBoxMissionViews.OpenTownCenterMission));
				PatchOrderViewsMethod(harmony, orderViewsPostfix, nameof(SandBoxMissionViews.OpenTavernMission));
				PatchOrderViewsMethod(harmony, orderViewsPostfix, nameof(SandBoxMissionViews.OpenVillageMission));
				PatchOrderViewsMethod(harmony, orderViewsPostfix, nameof(SandBoxMissionViews.OpenTownMerchantMission));
				PatchOrderViewsMethod(harmony, orderViewsPostfix, nameof(SandBoxMissionViews.OpenAlleyMission));
				PatchOrderViewsMethod(harmony, orderViewsPostfix, nameof(SandBoxMissionViews.OpenBattleMissionWhileEnteringSettlement));
				MethodInfo orderControllerPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(ProvideOrderTroopPlacerOrderControllerPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo orderControllerGetter = AccessTools.PropertyGetter(typeof(OrderTroopPlacer), "OrderController");
				if (orderControllerGetter != null && orderControllerPrefix != null)
				{
					harmony.Patch(orderControllerGetter, prefix: new HarmonyMethod(orderControllerPrefix));
				}
				MethodInfo hasSelectedPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SafeOrderTroopPlacerHasSelectedFormationsPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo hasSelectedTarget = AccessTools.Method(typeof(OrderTroopPlacer), "HasSelectedFormations");
				if (hasSelectedTarget != null && hasSelectedPrefix != null)
				{
					harmony.Patch(hasSelectedTarget, prefix: new HarmonyMethod(hasSelectedPrefix));
				}
				MethodInfo canUpdatePrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SafeOrderTroopPlacerCanUpdatePrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo canUpdateTarget = AccessTools.Method(typeof(OrderTroopPlacer), "CanUpdate");
				if (canUpdateTarget != null && canUpdatePrefix != null)
				{
					harmony.Patch(canUpdateTarget, prefix: new HarmonyMethod(canUpdatePrefix));
				}
				MethodInfo afterStartPostfix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(BindOrderTroopPlacerAfterStartPostfix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo afterStartTarget = AccessTools.Method(typeof(OrderTroopPlacer), "AfterStart");
				if (afterStartTarget != null && afterStartPostfix != null)
				{
					harmony.Patch(afterStartTarget, postfix: new HarmonyMethod(afterStartPostfix));
				}
				PatchMissionOrderVmAndUiMethods(harmony);
				PatchInterventionHideoutVisualOrderProvider(harmony);
				PatchInterventionVisualOrderFactory(harmony);
				PatchInterventionOrderControllerMethods(harmony);
				MethodInfo sameTeamDamagePrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(AllowPlayerCivilianDamagePrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo sameTeamDamageTarget = AccessTools.Method(typeof(Mission), "CancelsDamageAndBlocksAttackBecauseOfNonEnemyCase");
				if (sameTeamDamageTarget != null && sameTeamDamagePrefix != null)
				{
					harmony.Patch(sameTeamDamageTarget, prefix: new HarmonyMethod(sameTeamDamagePrefix));
				}
				MethodInfo fleeTickPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SuppressCivilianNativeFleeTickPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo fleeTickTarget = AccessTools.Method(typeof(FleeBehavior), nameof(FleeBehavior.Tick), new Type[] { typeof(float), typeof(bool) });
				if (fleeTickTarget != null && fleeTickPrefix != null)
				{
					harmony.Patch(fleeTickTarget, prefix: new HarmonyMethod(fleeTickPrefix));
				}
				MethodInfo fleeAvailabilityPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SuppressCivilianNativeFleeAvailabilityPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo fleeAvailabilityTarget = AccessTools.Method(typeof(FleeBehavior), nameof(FleeBehavior.GetAvailability), new Type[] { typeof(bool) });
				if (fleeAvailabilityTarget != null && fleeAvailabilityPrefix != null)
				{
					harmony.Patch(fleeAvailabilityTarget, prefix: new HarmonyMethod(fleeAvailabilityPrefix));
				}
				_patched = true;
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "Scene taunt suppression patch failed: " + ex.Message);
			}
		}

		private static void PatchMovementOrderConstructors(Harmony harmony)
		{
			try
			{
				MethodInfo transpiler = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(SafeMovementOrderMissionTimeTranspiler), BindingFlags.Static | BindingFlags.NonPublic);
				if (harmony == null || transpiler == null)
				{
					return;
				}
				foreach (ConstructorInfo ctor in typeof(MovementOrder).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					try
					{
						harmony.Patch(ctor, transpiler: new HarmonyMethod(transpiler));
					}
					catch (Exception ex)
					{
						Logger.Log("SiegeAiIntervention", "MovementOrder ctor safety patch failed on " + ctor + ": " + ex.Message);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "PatchMovementOrderConstructors failed: " + ex.Message);
			}
		}

		private static IEnumerable<CodeInstruction> SafeMovementOrderMissionTimeTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = (instructions ?? Enumerable.Empty<CodeInstruction>()).ToList();
			MethodInfo getCurrent = AccessTools.PropertyGetter(typeof(Mission), nameof(Mission.Current));
			MethodInfo getCurrentTime = AccessTools.PropertyGetter(typeof(Mission), nameof(Mission.CurrentTime));
			MethodInfo safeTime = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(GetSafeMovementOrderMissionTime), BindingFlags.Static | BindingFlags.NonPublic);
			if (getCurrent == null || getCurrentTime == null || safeTime == null)
			{
				return list;
			}
			for (int i = 0; i < list.Count - 1; i++)
			{
				if (Equals(list[i].operand, getCurrent) && Equals(list[i + 1].operand, getCurrentTime))
				{
					CodeInstruction replacement = new CodeInstruction(OpCodes.Call, safeTime);
					replacement.labels.AddRange(list[i].labels);
					replacement.labels.AddRange(list[i + 1].labels);
					replacement.blocks.AddRange(list[i].blocks);
					replacement.blocks.AddRange(list[i + 1].blocks);
					list[i] = replacement;
					list.RemoveAt(i + 1);
				}
			}
			return list;
		}

		private static float GetSafeMovementOrderMissionTime()
		{
			try
			{
				return Mission.Current?.CurrentTime ?? 0f;
			}
			catch
			{
				return 0f;
			}
		}

		private static void PatchSceneTauntMissionMethod(Harmony harmony, MethodInfo prefix, string methodName)
		{
			try
			{
				MethodInfo target = AccessTools.Method(typeof(SceneTauntMissionBehavior), methodName);
				if (harmony == null || prefix == null || target == null)
				{
					Logger.Log("SiegeAiIntervention", "Scene taunt suppression target missing: " + methodName);
					return;
				}
				harmony.Patch(target, prefix: new HarmonyMethod(prefix));
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "Scene taunt suppression method patch failed (" + methodName + "): " + ex.Message);
			}
		}

		private static void PatchSceneTauntStaticMethod(Harmony harmony, MethodInfo prefix, string methodName)
		{
			try
			{
				MethodInfo target = AccessTools.Method(typeof(SceneTauntBehavior), methodName);
				if (harmony == null || prefix == null || target == null)
				{
					Logger.Log("SiegeAiIntervention", "Scene taunt static suppression target missing: " + methodName);
					return;
				}
				harmony.Patch(target, prefix: new HarmonyMethod(prefix));
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "Scene taunt static suppression method patch failed (" + methodName + "): " + ex.Message);
			}
		}

		private static void PatchOrderViewsMethod(Harmony harmony, MethodInfo postfix, string methodName)
		{
			try
			{
				MethodInfo target = AccessTools.Method(typeof(SandBoxMissionViews), methodName, new Type[] { typeof(Mission) });
				if (harmony == null || postfix == null || target == null)
				{
					Logger.Log("SiegeAiIntervention", "Order view injection target missing: " + methodName);
					return;
				}
				harmony.Patch(target, postfix: new HarmonyMethod(postfix));
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "Order view injection patch failed (" + methodName + "): " + ex.Message);
			}
		}

		private static void PatchMissionOrderVmAndUiMethods(Harmony harmony)
		{
			try
			{
				if (harmony == null)
				{
					return;
				}
				Type orderUiHandlerType = AccessTools.TypeByName("TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer.MissionGauntletSingleplayerOrderUIHandler");
				MethodInfo initTarget = orderUiHandlerType == null ? null : AccessTools.Method(orderUiHandlerType, "OnMissionScreenInitialize");
				MethodInfo initPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(PrepareOrderUiInitializePrefix), BindingFlags.Static | BindingFlags.NonPublic);
				if (initTarget != null && initPrefix != null)
				{
					harmony.Patch(initTarget, prefix: new HarmonyMethod(initPrefix));
					Logger.Log("SiegeAiIntervention", "Patched MissionGauntletSingleplayerOrderUIHandler.OnMissionScreenInitialize for intervention order UI.");
				}
				Type vmType = AccessTools.TypeByName("TaleWorlds.MountAndBlade.ViewModelCollection.Order.MissionOrderVM");
				MethodInfo teamGetter = vmType == null ? null : AccessTools.PropertyGetter(vmType, "Team");
				MethodInfo teamPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(ProvideMissionOrderVmTeamPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				if (teamGetter != null && teamPrefix != null)
				{
					harmony.Patch(teamGetter, prefix: new HarmonyMethod(teamPrefix));
				}
				MethodInfo controllerGetter = vmType == null ? null : AccessTools.PropertyGetter(vmType, "OrderController");
				MethodInfo controllerPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(ProvideMissionOrderVmOrderControllerPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				if (controllerGetter != null && controllerPrefix != null)
				{
					harmony.Patch(controllerGetter, prefix: new HarmonyMethod(controllerPrefix));
				}
				MethodInfo hasTroopsGetter = vmType == null ? null : AccessTools.PropertyGetter(vmType, "PlayerHasAnyTroopUnderThem");
				MethodInfo hasTroopsPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(ProvideMissionOrderVmPlayerHasTroopsPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				if (hasTroopsGetter != null && hasTroopsPrefix != null)
				{
					harmony.Patch(hasTroopsGetter, prefix: new HarmonyMethod(hasTroopsPrefix));
				}
				MethodInfo checkCanOpenTarget = vmType == null ? null : AccessTools.Method(vmType, "CheckCanBeOpened");
				MethodInfo checkCanOpenPrefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(ProvideMissionOrderVmCheckCanBeOpenedPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				if (checkCanOpenTarget != null && checkCanOpenPrefix != null)
				{
					harmony.Patch(checkCanOpenTarget, prefix: new HarmonyMethod(checkCanOpenPrefix));
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "Mission order VM/UI patch failed: " + ex.Message);
			}
		}

		private static void PatchInterventionHideoutVisualOrderProvider(Harmony harmony)
		{
			try
			{
				if (harmony == null)
				{
					return;
				}
				Type providerType = AccessTools.TypeByName("SandBox.View.OrderProviders.HideoutVisualOrderProvider");
				MethodInfo target = providerType == null ? null : AccessTools.Method(providerType, "IsAvailable", Type.EmptyTypes);
				MethodInfo prefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(AllowHideoutVisualOrderProviderDuringInterventionPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				if (target == null || prefix == null)
				{
					Logger.Log("SiegeAiIntervention", "Hideout visual order provider patch target missing.");
					return;
				}
				harmony.Patch(target, prefix: new HarmonyMethod(prefix));
				Logger.Log("SiegeAiIntervention", "Patched HideoutVisualOrderProvider.IsAvailable for intervention native order UI.");
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "PatchInterventionHideoutVisualOrderProvider failed: " + ex.Message);
			}
		}

		private static bool AllowHideoutVisualOrderProviderDuringInterventionPrefix(ref bool __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				Mission mission = Mission.Current;
				if (mission == null || mission.IsMissionEnding || mission.Mode == MissionMode.Conversation || mission.Mode == MissionMode.Barter)
				{
					return true;
				}
				__result = true;
				return false;
			}
			catch
			{
				return true;
			}
		}

		private static void PatchInterventionVisualOrderFactory(Harmony harmony)
		{
			try
			{
				MethodInfo target = AccessTools.Method(typeof(VisualOrderFactory), nameof(VisualOrderFactory.GetOrders), Type.EmptyTypes);
				MethodInfo postfix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(FilterInterventionVisualOrdersPostfix), BindingFlags.Static | BindingFlags.NonPublic);
				if (harmony != null && target != null && postfix != null)
				{
					harmony.Patch(target, postfix: new HarmonyMethod(postfix));
					Logger.Log("SiegeAiIntervention", "Patched VisualOrderFactory.GetOrders to filter offensive intervention orders.");
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "PatchInterventionVisualOrderFactory failed: " + ex.Message);
			}
		}

		private static void PatchInterventionOrderControllerMethods(Harmony harmony)
		{
			try
			{
				MethodInfo prefix = typeof(SiegeInterventionSceneTauntSuppressionPatch).GetMethod(nameof(BlockOffensiveNativeOrderPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				if (harmony == null || prefix == null)
				{
					return;
				}
				foreach (MethodInfo method in typeof(OrderController).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					ParameterInfo[] parameters = method.GetParameters();
					if (!method.Name.StartsWith("SetOrder", StringComparison.Ordinal) || parameters.Length == 0 || parameters[0].ParameterType != typeof(OrderType))
					{
						continue;
					}
					try
					{
						harmony.Patch(method, prefix: new HarmonyMethod(prefix));
						Logger.Log("SiegeAiIntervention", "Patched offensive order guard on OrderController." + method.Name + ".");
					}
					catch (Exception ex)
					{
						Logger.Log("SiegeAiIntervention", "OrderController offensive guard patch failed on " + method.Name + ": " + ex.Message);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "PatchInterventionOrderControllerMethods failed: " + ex.Message);
			}
		}

		private static void FilterInterventionVisualOrdersPostfix(ref MBReadOnlyList<VisualOrderSet> __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return;
				}
				__result = SiegeAiInterventionBehavior.FilterInterventionNativeVisualOrdersForExternal(__result);
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "FilterInterventionVisualOrdersPostfix failed: " + ex.Message);
			}
		}

		private static bool BlockOffensiveNativeOrderPrefix(object[] __args)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				if (__args == null || __args.Length == 0 || !(__args[0] is OrderType orderType))
				{
					return true;
				}
				if (!SiegeAiInterventionBehavior.IsOffensiveInterventionOrder(orderType))
				{
					return true;
				}
				Logger.Log("SiegeAiIntervention", "Blocked offensive native order during intervention: " + orderType);
				return false;
			}
			catch
			{
				return true;
			}
		}

		private static bool SuppressSceneTauntMissionBehaviorPrefix()
		{
			return !SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal();
		}

		private static bool SuppressSceneTauntCrimePrefix()
		{
			return !SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal();
		}

		private static bool SuppressCrimeRatingDuringInterventionPrefix()
		{
			return !SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal();
		}

		private static bool SuppressCivilianUsableTargetPrefix(AgentNavigator __instance, UsableMachine usableMachine)
		{
			try
			{
				if (usableMachine == null || !SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				Agent agent = __instance?.OwnerAgent;
				if (!SiegeAiInterventionBehavior.IsEligibleCivilianAgent(agent, includeHeroes: true, requireActive: false))
				{
					return true;
				}
				SiegeAiInterventionBehavior.NeutralizeCivilianDailyUsableBehavior(agent, "usable_target_prefix");
				return false;
			}
			catch
			{
				return true;
			}
		}

		private static bool SuppressCivilianUsableMissionObjectPrefix(Agent userAgent)
		{
			try
			{
				if (SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal() && SiegeAiInterventionBehavior.IsEligibleCivilianAgent(userAgent, includeHeroes: true, requireActive: false))
				{
					return false;
				}
			}
			catch
			{
			}
			return true;
		}

		private static bool SuppressCivilianNativeFleeTickPrefix(FleeBehavior __instance)
		{
			try
			{
				Agent agent = __instance?.OwnerAgent;
				if (SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal() && SiegeAiInterventionBehavior.IsEligibleCivilianAgent(agent, includeHeroes: true, requireActive: false))
				{
					if (__instance != null)
					{
						__instance.IsActive = false;
					}
					if (agent != null && agent.IsActive())
					{
						SiegeAiInterventionBehavior.NeutralizeCivilianDailyUsableBehavior(agent, "native_flee_tick_prefix");
					}
					return false;
				}
			}
			catch
			{
			}
			return true;
		}

		private static bool SuppressCivilianNativeFleeAvailabilityPrefix(FleeBehavior __instance, ref float __result)
		{
			try
			{
				Agent agent = __instance?.OwnerAgent;
				if (SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal() && SiegeAiInterventionBehavior.IsEligibleCivilianAgent(agent, includeHeroes: true, requireActive: false))
				{
					__result = 0f;
					return false;
				}
			}
			catch
			{
			}
			return true;
		}

		private static void PrepareOrderUiInitializePrefix()
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return;
				}
				SiegeAiInterventionBehavior.EnsureInterventionCommandUiReadyForExternal(Mission.Current, "order_ui_initialize");
			}
			catch
			{
			}
		}

		private static bool ProvideMissionOrderVmTeamPrefix(ref Team __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				__result = SiegeAiInterventionBehavior.ResolveInterventionPlayerCommandTeamForExternal(Mission.Current, "mission_order_vm_team");
				return __result == null;
			}
			catch
			{
				return true;
			}
		}

		private static bool ProvideMissionOrderVmOrderControllerPrefix(ref OrderController __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				SiegeAiInterventionBehavior.EnsureInterventionCommandUiReadyForExternal(Mission.Current, "mission_order_vm_controller");
				__result = SiegeAiInterventionBehavior.TryResolveNativeOrderControllerForExternal(Mission.Current);
				return __result == null;
			}
			catch
			{
				__result = null;
				return false;
			}
		}

		private static bool ProvideMissionOrderVmPlayerHasTroopsPrefix(ref bool __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				SiegeAiInterventionBehavior.EnsureInterventionCommandUiReadyForExternal(Mission.Current, "mission_order_vm_has_troops");
				__result = SiegeAiInterventionBehavior.InterventionPlayerHasCommandableAgentsForExternal(Mission.Current);
				return false;
			}
			catch
			{
				__result = false;
				return false;
			}
		}

		private static bool ProvideMissionOrderVmCheckCanBeOpenedPrefix(ref bool __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				Mission mission = Mission.Current;
				if (mission == null || mission.IsMissionEnding || mission.Mode == MissionMode.Conversation || mission.Mode == MissionMode.Barter)
				{
					return true;
				}
				__result = SiegeAiInterventionBehavior.EnsureInterventionCommandUiReadyForExternal(mission, "mission_order_vm_check_open") && SiegeAiInterventionBehavior.InterventionPlayerHasCommandableAgentsForExternal(mission);
				return false;
			}
			catch
			{
				__result = false;
				return false;
			}
		}

		private static bool ProvideOrderTroopPlacerOrderControllerPrefix(OrderTroopPlacer __instance, ref OrderController __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				OrderController orderController = SiegeAiInterventionBehavior.TryResolveNativeOrderControllerForExternal(__instance?.Mission ?? Mission.Current);
				if (orderController == null)
				{
					__result = null;
					return false;
				}
				SiegeAiInterventionBehavior.TryBindNativeOrderControllerForExternal(__instance, "order_controller_getter");
				__result = orderController;
				return false;
			}
			catch
			{
				__result = null;
				return false;
			}
		}

		private static bool SafeOrderTroopPlacerHasSelectedFormationsPrefix(OrderTroopPlacer __instance, ref bool __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				__result = SiegeAiInterventionBehavior.NativeOrderControllerHasSelectedFormationsForExternal(__instance?.Mission ?? Mission.Current);
				return false;
			}
			catch
			{
				__result = false;
				return false;
			}
		}

		private static bool SafeOrderTroopPlacerCanUpdatePrefix(OrderTroopPlacer __instance, ref bool __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal())
				{
					return true;
				}
				__result = SiegeAiInterventionBehavior.NativeOrderControllerHasSelectedFormationsForExternal(__instance?.Mission ?? Mission.Current);
				return false;
			}
			catch
			{
				__result = false;
				return false;
			}
		}

		private static void BindOrderTroopPlacerAfterStartPostfix(OrderTroopPlacer __instance)
		{
			SiegeAiInterventionBehavior.TryBindNativeOrderControllerForExternal(__instance, "order_placer_after_start");
		}


		private static void InjectInterventionOrderViewsPostfix(Mission mission, ref MissionView[] __result, MethodBase __originalMethod)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.ShouldInjectInterventionOrderViewsForExternal(mission))
				{
					return;
				}
				int oldCount = __result?.Length ?? 0;
				SiegeAiInterventionBehavior.EnsureInterventionCommandUiReadyForExternal(mission, "inject_native_order_views");
				__result = BuildInterventionNativeOrderViews(mission, __result);
				Logger.Log("SiegeAiIntervention", "Extended intervention mission views with native order UI while preserving settlement leave view. ViewMethod=" + (__originalMethod?.Name ?? "N/A") + ", OldViews=" + oldCount + ", NewViews=" + (__result?.Length ?? 0));
			}
			catch (Exception ex)
			{
				Logger.Log("SiegeAiIntervention", "InjectInterventionOrderViewsPostfix failed: " + ex.Message);
			}
		}

		private static MissionView[] BuildInterventionNativeOrderViews(Mission mission, MissionView[] originalViews)
		{
			List<MissionView> views = (originalViews ?? Array.Empty<MissionView>())
				.Where(view => view != null && !IsBattleScoreMissionView(view))
				.ToList();
			void AddIfMissing(MissionView view)
			{
				if (view == null)
				{
					return;
				}
				Type type = view.GetType();
				if (views.Any(existing => existing != null && existing.GetType() == type))
				{
					return;
				}
				views.Add(view);
			}
			AddIfMissing(ViewCreator.CreateMissionLeaveView());
			AddIfMissing(ViewCreator.CreateMissionOrderUIHandler(null));
			AddIfMissing(new OrderTroopPlacer(null));
			AddIfMissing(ViewCreator.CreateMissionFormationMarkerUIHandler(mission));
			AddIfMissing(new MissionFormationTargetSelectionHandler());
			return views.ToArray();
		}

		private static bool IsBattleScoreMissionView(MissionView view)
		{
			string name = view?.GetType()?.Name ?? "";
			return name.IndexOf("BattleScore", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("Scoreboard", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private static bool AllowPlayerCivilianDamagePrefix(Mission __instance, Agent attacker, Agent victim, ref bool __result)
		{
			try
			{
				if (!SiegeAiInterventionBehavior.IsOccupationSceneActiveForExternal() || attacker == null || victim == null || !attacker.IsMainAgent || !victim.IsHuman)
				{
					return true;
				}
				if (SiegeAiInterventionBehavior.IsInterventionAlliedSoldierForExternal(victim, requireActive: false))
				{
					SiegeAiInterventionBehavior.TryHandleFriendlyHitOnAlliedSoldier(victim, "non_enemy_damage_prefix", 0f);
					__result = true;
					return false;
				}
				if (SiegeAiInterventionBehavior.IsEligibleCivilianAgent(victim, includeHeroes: true, requireActive: false))
				{
					__result = false;
					return false;
				}
			}
			catch
			{
			}
			return true;
		}
	}
}
