using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.CampaignBehaviors;
using StoryMode.Quests.ThirdPhase;
using TaleWorlds.AchievementSystem;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class AchievementsCampaignBehavior : CampaignBehaviorBase
{
	private class AchievementMissionLogic : MissionLogic
	{
		private Action<Agent, Agent> OnAgentRemovedAction;

		private Action<Agent, WeaponComponentData, BoneBodyPartType, int> OnAgentHitAction;

		public AchievementMissionLogic(Action<Agent, Agent> onAgentRemoved, Action<Agent, WeaponComponentData, BoneBodyPartType, int> onAgentHitAction)
		{
			OnAgentRemovedAction = onAgentRemoved;
			OnAgentHitAction = onAgentHitAction;
		}

		public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
		{
			OnAgentRemovedAction?.Invoke(affectedAgent, affectorAgent);
		}

		public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			OnAgentHitAction?.Invoke(affectorAgent, attackerWeapon, blow.VictimBodyPart, (int)hitDistance);
		}
	}

	private const float SettlementCountStoredInIntegerSet = 30f;

	private const string CreatedKingdomCountStatID = "CreatedKingdomCount";

	private const string ClearedHideoutCountStatID = "ClearedHideoutCount";

	private const string RepelledSiegeAssaultStatID = "RepelledSiegeAssaultCount";

	private const string KingOrQueenKilledInBattleStatID = "KingOrQueenKilledInBattle";

	private const string SuccessfulSiegeCountStatID = "SuccessfulSiegeCount";

	private const string WonTournamentCountStatID = "WonTournamentCount";

	private const string HighestTierSwordCraftedStatID = "HighestTierSwordCrafted";

	private const string SuccessfulBattlesAgainstArmyCountStatID = "SuccessfulBattlesAgainstArmyCount";

	private const string DefeatedArmyWhileAloneCountStatID = "DefeatedArmyWhileAloneCount";

	private const string TotalTradeProfitStatID = "TotalTradeProfit";

	private const string MaxDailyTributeGainStatID = "MaxDailyTributeGain";

	private const string MaxDailyIncomeStatID = "MaxDailyIncome";

	private const string CapturedATownAloneCountStatID = "CapturedATownAloneCount";

	private const string DefeatedTroopCountStatID = "DefeatedTroopCount";

	private const string FarthestHeadStatID = "FarthestHeadShot";

	private const string ButtersInInventoryStatID = "ButtersInInventoryCount";

	private const string ReachedClanTierSixStatID = "ReachedClanTierSix";

	private const string OwnedFortificationCountStatID = "OwnedFortificationCount";

	private const string HasOwnedCaravanAndWorkshopStatID = "HasOwnedCaravanAndWorkshop";

	private const string ExecutedLordWithMinus100RelationStatID = "ExecutedLordRelation100";

	private const string HighestSkillValueStatID = "HighestSkillValue";

	private const string LeaderOfTournamentStatID = "LeaderOfTournament";

	private const string FinishedTutorialStatID = "FinishedTutorial";

	private const string DefeatedSuperiorForceStatID = "DefeatedSuperiorForce";

	private const string BarbarianVictoryStatID = "BarbarianVictory";

	private const string ImperialVictoryStatID = "ImperialVictory";

	private const string AssembledDragonBannerStatID = "AssembledDragonBanner";

	private const string CompletedAllProjectsStatID = "CompletedAllProjects";

	private const string ClansUnderPlayerKingdomCountStatID = "ClansUnderPlayerKingdomCount";

	private const string HearthBreakerStatID = "Hearthbreaker";

	private const string ProposedAndWonAPolicyStatID = "ProposedAndWonAPolicy";

	private const string BestServedColdStatID = "BestServedCold";

	private const string DefeatedRadagosInDUelStatID = "RadagosDefeatedInDuel";

	private const string GreatGrannyStatID = "GreatGranny";

	private const string NumberOfChildrenStatID = "NumberOfChildrenBorn";

	private const string UndercoverStatID = "CompletedAnIssueInHostileTown";

	private const string EnteredEverySettlemenStatID = "EnteredEverySettlement";

	private bool _deactivateAchievements;

	private int _cachedCreatedKingdomCount;

	private int _cachedHideoutClearedCount;

	private int _cachedHighestSkillValue = -1;

	private int _cachedRepelledSiegeAssaultCount;

	private int _cachedCapturedTownAloneCount;

	private int _cachedKingOrQueenKilledInBattle;

	private int _cachedSuccessfulSiegeCount;

	private int _cachedWonTournamentCount;

	private int _cachedSuccessfulBattlesAgainstArmyCount;

	private int _cachedSuccessfulBattlesAgainstArmyAloneCount;

	private int _cachedTotalTradeProfit;

	private int _cachedMaxDailyIncome;

	private int _cachedDefeatedTroopCount;

	private int _cachedFarthestHeadShot;

	private ItemObject _butter;

	private List<Settlement> _orderedSettlementList = new List<Settlement>();

	private int[] _settlementIntegerSetList;

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<bool>("_deactivateAchievements", ref _deactivateAchievements);
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener((object)this, (Action)CacheHighestSkillValue);
		CampaignEvents.WorkshopOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Workshop, Hero>)ProgressOwnedWorkshopCount);
		CampaignEvents.MobilePartyCreated.AddNonSerializedListener((object)this, (Action<MobileParty>)ProgressOwnedCaravanCount);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
		CampaignEvents.KingdomCreatedEvent.AddNonSerializedListener((object)this, (Action<Kingdom>)ProgressCreatedKingdomCount);
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
		CampaignEvents.BeforeHeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnBeforeHeroKilled);
		CampaignEvents.ClanTierIncrease.AddNonSerializedListener((object)this, (Action<Clan, bool>)ProgressClanTier);
		CampaignEvents.OnHideoutBattleCompletedEvent.AddNonSerializedListener((object)this, (Action<BattleSideEnum, HideoutEventComponent, HideoutBattleEndState>)OnHideoutBattleCompleted);
		CampaignEvents.HeroGainedSkill.AddNonSerializedListener((object)this, (Action<Hero, SkillObject, int, bool>)ProgressHeroSkillValue);
		CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener((object)this, (Action<List<(ItemRosterElement, int)>, List<(ItemRosterElement, int)>, bool>)PlayerInventoryExchange);
		CampaignEvents.TournamentFinished.AddNonSerializedListener((object)this, (Action<CharacterObject, MBReadOnlyList<CharacterObject>, Town, ItemObject>)OnTournamentFinish);
		CampaignEvents.SiegeCompletedEvent.AddNonSerializedListener((object)this, (Action<Settlement, MobileParty, bool, BattleTypes>)OnSiegeCompleted);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		CampaignEvents.OnBuildingLevelChangedEvent.AddNonSerializedListener((object)this, (Action<Town, Building, int>)OnBuildingLevelChanged);
		CampaignEvents.OnNewItemCraftedEvent.AddNonSerializedListener((object)this, (Action<ItemObject, ItemModifier, bool>)OnNewItemCrafted);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
		CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener((object)this, (Action<Clan>)OnClanDestroyed);
		CampaignEvents.OnPlayerTradeProfitEvent.AddNonSerializedListener((object)this, (Action<int>)ProgressTotalTradeProfit);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener((object)this, (Action)OnDailyTick);
		CampaignEvents.BeforeHeroesMarried.AddNonSerializedListener((object)this, (Action<Hero, Hero, bool>)CheckHeroMarriage);
		CampaignEvents.KingdomDecisionConcluded.AddNonSerializedListener((object)this, (Action<KingdomDecision, DecisionOutcome, bool>)CheckKingdomDecisionConcluded);
		CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionStarted);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEnter);
		CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnNewGameCreatedPartialFollowUpEnd);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
		CampaignEvents.HeroCreated.AddNonSerializedListener((object)this, (Action<Hero, bool>)OnHeroCreated);
		CampaignEvents.OnIssueUpdatedEvent.AddNonSerializedListener((object)this, (Action<IssueBase, IssueUpdateDetails, Hero>)OnIssueUpdated);
		CampaignEvents.RulingClanChanged.AddNonSerializedListener((object)this, (Action<Kingdom, Clan>)OnRulingClanChanged);
		CampaignEvents.OnConfigChangedEvent.AddNonSerializedListener((object)this, (Action)OnConfigChanged);
		CampaignEvents.CollectMetadataEntriesEvent.AddNonSerializedListener((object)this, (Action<List<KeyValuePair<string, string>>>)CollectMetadataEntries);
		StoryModeEvents.OnStoryModeTutorialEndedEvent.AddNonSerializedListener((object)this, (Action)CheckTutorialFinished);
		StoryModeEvents.OnBannerPieceCollectedEvent.AddNonSerializedListener((object)this, (Action)ProgressAssembledDragonBanner);
	}

	private void OnRulingClanChanged(Kingdom kingdom, Clan newRulingCLan)
	{
		ProgressOwnedFortificationCount();
	}

	private void OnIssueUpdated(IssueBase issueBase, IssueUpdateDetails detail, Hero issueSolver)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (issueSolver == Hero.MainHero && !issueBase.IsSolvingWithAlternative && (int)detail == 5 && issueBase.IssueOwner.MapFaction != null && issueBase.IssueOwner.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
		{
			SetStatInternal("CompletedAnIssueInHostileTown", 1);
		}
	}

	private void OnHideoutBattleCompleted(BattleSideEnum winnerSide, HideoutEventComponent hideoutEventComponent, HideoutBattleEndState battleEndState)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (((MapEventComponent)hideoutEventComponent).MapEvent.InvolvedParties.Contains(PartyBase.MainParty) && winnerSide == ((MapEventComponent)hideoutEventComponent).MapEvent.PlayerSide)
		{
			ProgressHideoutClearedCount();
		}
	}

	private void OnBeforeHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		ProgressKingOrQueenKilledInBattle(victim, killer, detail);
	}

	private void OnConfigChanged()
	{
		if (!CheckAchievementSystemActivity(out var reason))
		{
			DeactivateAchievements(reason);
		}
	}

	private void OnHeroCreated(Hero hero, bool isBornNaturally)
	{
		if (isBornNaturally)
		{
			if (hero.Father == Hero.MainHero || hero.Mother == Hero.MainHero)
			{
				ProgressChildCount();
			}
			CheckGrandparent();
		}
	}

	private void OnGameLoadFinished()
	{
		if (CheckAchievementSystemActivity(out var reason))
		{
			CacheAndInitializeAchievementVariables();
			CacheHighestSkillValue();
		}
		else
		{
			DeactivateAchievements(reason);
		}
	}

	private async void CacheAndInitializeAchievementVariables()
	{
		_butter = MBObjectManager.Instance.GetObject<ItemObject>("butter");
		List<string> list = new List<string>
		{
			"CreatedKingdomCount", "ClearedHideoutCount", "RepelledSiegeAssaultCount", "KingOrQueenKilledInBattle", "SuccessfulSiegeCount", "WonTournamentCount", "SuccessfulBattlesAgainstArmyCount", "DefeatedArmyWhileAloneCount", "TotalTradeProfit", "MaxDailyIncome",
			"CapturedATownAloneCount", "DefeatedTroopCount", "FarthestHeadShot"
		};
		_orderedSettlementList = (from x in (IEnumerable<Settlement>)Settlement.All
			where x.IsFortification
			orderby ((MBObjectBase)x).StringId descending
			select x).ToList();
		int neededIntegerCount = MathF.Ceiling((float)_orderedSettlementList.Count / 30f);
		_settlementIntegerSetList = new int[neededIntegerCount];
		for (int num = 0; num < neededIntegerCount; num++)
		{
			list.Add("SettlementSet" + num);
		}
		int[] array = await AchievementManager.GetStats(list.ToArray());
		if (array != null)
		{
			int num2 = 0;
			_cachedCreatedKingdomCount = array[num2++];
			_cachedHideoutClearedCount = array[num2++];
			_cachedRepelledSiegeAssaultCount = array[num2++];
			_cachedKingOrQueenKilledInBattle = array[num2++];
			_cachedSuccessfulSiegeCount = array[num2++];
			_cachedWonTournamentCount = array[num2++];
			_cachedSuccessfulBattlesAgainstArmyCount = array[num2++];
			_cachedSuccessfulBattlesAgainstArmyAloneCount = array[num2++];
			_cachedTotalTradeProfit = array[num2++];
			_cachedMaxDailyIncome = array[num2++];
			_cachedCapturedTownAloneCount = array[num2++];
			_cachedDefeatedTroopCount = array[num2++];
			_cachedFarthestHeadShot = array[num2++];
			for (int num3 = 0; num3 < neededIntegerCount; num3++)
			{
				int num4 = array[num2++];
				if (num4 == -1)
				{
					_settlementIntegerSetList[num3] = 0;
					SetStatInternal("SettlementSet" + num3, 0);
				}
				else
				{
					_settlementIntegerSetList[num3] = num4;
				}
			}
		}
		else
		{
			TextObject reason = new TextObject("{=4wS8eYYe}Achievements are disabled temporarily for this session due to service disconnection.", (Dictionary<string, object>)null);
			DeactivateAchievements(reason, showMessage: true, temporarily: true);
			Debug.Print("Achievements are disabled because current platform does not support achievements!", 0, (DebugColor)0, 17592186044416uL);
		}
	}

	private void OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter starter)
	{
		if (CheckAchievementSystemActivity(out var reason))
		{
			CacheAndInitializeAchievementVariables();
		}
		else
		{
			DeactivateAchievements(reason);
		}
	}

	private void OnDailyTick()
	{
		ProgressDailyTribute();
		ProgressDailyIncome();
	}

	private void OnClanDestroyed(Clan clan)
	{
		ProgressClansUnderKingdomCount();
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
	{
		ProgressDailyIncome();
		ProgressClansUnderKingdomCount();
		ProgressOwnedFortificationCount();
	}

	private void OnNewItemCrafted(ItemObject itemObject, ItemModifier overriddenItemModifier, bool isCraftingOrderItem)
	{
		ProgressHighestTierSwordCrafted(itemObject);
	}

	private void OnBuildingLevelChanged(Town town, Building building, int levelChange)
	{
		ProgressDailyIncome();
		CheckProjectsInSettlement(town);
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ProgressImperialBarbarianVictory(quest, detail);
	}

	private void OnTournamentFinish(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town, ItemObject prize)
	{
		ProgressTournamentWonCount(winner);
		ProgressTournamentRank(winner);
	}

	private void OnMapEventEnded(MapEvent mapEvent)
	{
		ProgressRepelSiegeAssaultCount(mapEvent);
		CheckDefeatedSuperiorForce(mapEvent);
		ProgressSuccessfulBattlesAgainstArmyCount(mapEvent);
		ProgressSuccessfulBattlesAgainstArmyAloneCount(mapEvent);
	}

	private void OnSiegeCompleted(Settlement siegeSettlement, MobileParty attackerParty, bool isWin, BattleTypes battleType)
	{
		ProgressRepelSiegeAssaultCount(siegeSettlement, isWin);
		ProgressSuccessfulSiegeCount(attackerParty, isWin);
		ProgressCapturedATownAlone(attackerParty, isWin);
	}

	private void PlayerInventoryExchange(List<(ItemRosterElement, int)> purchasedItems, List<(ItemRosterElement, int)> soldItems, bool isTrading)
	{
		if (_butter != null)
		{
			int itemNumber = PartyBase.MainParty.ItemRoster.GetItemNumber(_butter);
			if (itemNumber > 0)
			{
				SetStatInternal("ButtersInInventoryCount", itemNumber);
			}
		}
	}

	public bool CheckAchievementSystemActivity(out TextObject reason)
	{
		bool flag = DumpIntegrityCampaignBehavior.IsGameIntegrityAchieved(ref reason);
		DumpIntegrityCampaignBehavior behavior = Campaign.Current.CampaignBehaviorManager.GetBehavior<DumpIntegrityCampaignBehavior>();
		if (!(!_deactivateAchievements && behavior != null && flag))
		{
			return MBDebug.IsTestMode();
		}
		return true;
	}

	private void OnSettlementEnter(MobileParty party, Settlement settlement, Hero hero)
	{
		if (party == MobileParty.MainParty && settlement.IsFortification)
		{
			int num = _orderedSettlementList.IndexOf(settlement);
			int num2 = MathF.Floor((float)num / 30f);
			int num3 = _settlementIntegerSetList[num2];
			int num4 = 1 << (int)(30f - ((float)num % 30f + 1f));
			int num5 = num3 | num4;
			SetStatInternal("SettlementSet" + num2, num5);
			if (_settlementIntegerSetList[num2] != num5)
			{
				_settlementIntegerSetList[num2] = num5;
				CheckEnteredEverySettlement();
			}
		}
	}

	private void CheckEnteredEverySettlement()
	{
		int num = 0;
		for (int i = 0; i < _settlementIntegerSetList.Length; i++)
		{
			for (int num2 = _settlementIntegerSetList[i]; num2 > 0; num2 >>= 1)
			{
				if (num2 % 2 == 1)
				{
					num++;
				}
			}
		}
		if (num == _orderedSettlementList.Count)
		{
			SetStatInternal("EnteredEverySettlement", 1);
		}
	}

	private void CacheHighestSkillValue()
	{
		int num = 0;
		foreach (SkillObject item in (List<SkillObject>)(object)Skills.All)
		{
			int skillValue = Hero.MainHero.GetSkillValue(item);
			if (skillValue > num)
			{
				num = skillValue;
			}
		}
		_cachedHighestSkillValue = num;
	}

	private void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		CheckExecutedLordRelation(victim, killer, detail);
		CheckBestServedCold(victim, killer, detail);
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
	{
		ProgressDailyIncome();
		if (settlement.IsFortification)
		{
			ProgressOwnedFortificationCount();
		}
	}

	private void OnMissionStarted(IMission obj)
	{
		AchievementMissionLogic achievementMissionLogic = new AchievementMissionLogic(OnAgentRemoved, OnAgentHit);
		Mission.Current.AddMissionBehavior((MissionBehavior)(object)achievementMissionLogic);
	}

	private void OnAgentHit(Agent affectorAgent, WeaponComponentData attackerWeapon, BoneBodyPartType victimBoneBodyPartType, int hitDistance)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (affectorAgent != null && affectorAgent == Agent.Main && attackerWeapon != null && !attackerWeapon.IsMeleeWeapon && (int)victimBoneBodyPartType == 0 && hitDistance > _cachedFarthestHeadShot)
		{
			SetStatInternal("FarthestHeadShot", hitDistance);
			_cachedFarthestHeadShot = hitDistance;
		}
	}

	private void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent)
	{
		if (affectorAgent != null && affectorAgent == Agent.Main && affectedAgent.IsHuman)
		{
			SetStatInternal("DefeatedTroopCount", ++_cachedDefeatedTroopCount);
		}
	}

	private void ProgressChildCount()
	{
		int num = ((List<Hero>)(object)Hero.MainHero.Children).Count;
		foreach (LogEntry item in (List<LogEntry>)(object)Campaign.Current.LogEntryHistory.GameActionLogs)
		{
			PlayerCharacterChangedLogEntry val;
			if ((val = (PlayerCharacterChangedLogEntry)(object)((item is PlayerCharacterChangedLogEntry) ? item : null)) != null)
			{
				num += ((List<Hero>)(object)val.OldPlayerHero.Children).Count;
			}
		}
		SetStatInternal("NumberOfChildrenBorn", num);
	}

	private void CheckGrandparent()
	{
		if (((IEnumerable<Hero>)Hero.MainHero.Children).Any((Hero x) => ((IEnumerable<Hero>)x.Children).Any((Hero y) => ((IEnumerable<Hero>)y.Children).Any())))
		{
			SetStatInternal("GreatGranny", 1);
		}
	}

	public void OnRadagosDuelWon()
	{
		SetStatInternal("RadagosDefeatedInDuel", 1);
	}

	private void CheckBestServedCold(Hero victim, Hero killer, KillCharacterActionDetail detail)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if (killer != Hero.MainHero || ((int)detail != 6 && (int)detail != 7 && (int)detail != 1 && (int)detail != 4 && (int)detail != 5))
		{
			return;
		}
		foreach (LogEntry item in (List<LogEntry>)(object)Campaign.Current.LogEntryHistory.GameActionLogs)
		{
			CharacterKilledLogEntry val;
			if ((val = (CharacterKilledLogEntry)(object)((item is CharacterKilledLogEntry) ? item : null)) != null && val.Killer == victim && val.VictimClan == Clan.PlayerClan)
			{
				SetStatInternal("BestServedCold", 1);
				break;
			}
		}
	}

	private void CheckProposedAndWonPolicy(KingdomDecision decision, DecisionOutcome chosenOutcome)
	{
		if (decision.ProposerClan == Clan.PlayerClan)
		{
			MBList<DecisionOutcome> obj = new MBList<DecisionOutcome>();
			((List<DecisionOutcome>)(object)obj).Add(chosenOutcome);
			if (decision.GetQueriedDecisionOutcome((MBReadOnlyList<DecisionOutcome>)(object)obj) != null)
			{
				SetStatInternal("ProposedAndWonAPolicy", 1);
			}
		}
	}

	private void CheckKingdomDecisionConcluded(KingdomDecision decision, DecisionOutcome chosenOutcome, bool isPlayerInvolved)
	{
		CheckProposedAndWonPolicy(decision, chosenOutcome);
		ProgressOwnedFortificationCount();
		ProgressClansUnderKingdomCount();
	}

	private void CheckHeroMarriage(Hero hero1, Hero hero2, bool showNotification = true)
	{
		if (hero1 != Hero.MainHero && hero2 != Hero.MainHero)
		{
			return;
		}
		Hero val = ((hero1 == Hero.MainHero) ? hero2 : hero1);
		foreach (LogEntry item in (List<LogEntry>)(object)Campaign.Current.LogEntryHistory.GameActionLogs)
		{
			CharacterKilledLogEntry val2;
			if ((val2 = (CharacterKilledLogEntry)(object)((item is CharacterKilledLogEntry) ? item : null)) != null && val2.Killer == Hero.MainHero && ((List<Hero>)(object)val.ExSpouses).Contains(val2.Victim))
			{
				SetStatInternal("Hearthbreaker", 1);
			}
		}
	}

	private void ProgressClansUnderKingdomCount()
	{
		if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader == Hero.MainHero)
		{
			SetStatInternal("ClansUnderPlayerKingdomCount", ((List<Clan>)(object)Clan.PlayerClan.Kingdom.Clans).Count);
		}
	}

	private void ProgressSuccessfulBattlesAgainstArmyCount(MapEvent mapEvent)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (mapEvent.IsPlayerMapEvent && mapEvent.Winner == mapEvent.GetMapEventSide(mapEvent.PlayerSide) && ((IEnumerable<MapEventParty>)mapEvent.GetMapEventSide(mapEvent.DefeatedSide).Parties).Any((MapEventParty x) => x.Party.MobileParty != null && x.Party.MobileParty.AttachedTo != null))
		{
			SetStatInternal("SuccessfulBattlesAgainstArmyCount", ++_cachedSuccessfulBattlesAgainstArmyCount);
		}
	}

	private void ProgressSuccessfulBattlesAgainstArmyAloneCount(MapEvent mapEvent)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (mapEvent.IsPlayerMapEvent && mapEvent.Winner == mapEvent.GetMapEventSide(mapEvent.PlayerSide) && ((IEnumerable<MapEventParty>)mapEvent.GetMapEventSide(mapEvent.DefeatedSide).Parties).Any((MapEventParty x) => x.Party.MobileParty != null && x.Party.MobileParty.AttachedTo != null) && ((List<MapEventParty>)(object)mapEvent.GetMapEventSide(mapEvent.PlayerSide).Parties).Count == 1)
		{
			SetStatInternal("DefeatedArmyWhileAloneCount", ++_cachedSuccessfulBattlesAgainstArmyAloneCount);
		}
	}

	private void ProgressDailyTribute()
	{
		IFaction mapFaction = Clan.PlayerClan.MapFaction;
		float num = 1f;
		int num2 = 0;
		if (Clan.PlayerClan.Kingdom != null)
		{
			num = CalculateTributeShareFactor(Clan.PlayerClan);
		}
		foreach (StanceLink stance in FactionHelper.GetStances(mapFaction))
		{
			int dailyTributeToPay = stance.GetDailyTributeToPay(mapFaction);
			if (stance.IsNeutral && dailyTributeToPay < 0)
			{
				int num3 = (int)((float)dailyTributeToPay * num);
				num2 += num3;
			}
		}
		SetStatInternal("MaxDailyTributeGain", MathF.Abs(num2));
	}

	private static float CalculateTributeShareFactor(Clan clan)
	{
		Kingdom kingdom = clan.Kingdom;
		int num = ((IEnumerable<Town>)kingdom.Fiefs).Sum((Town x) => ((SettlementComponent)x).IsCastle ? 1 : 3) + 1 + ((List<Clan>)(object)kingdom.Clans).Count;
		return (float)(((IEnumerable<Town>)clan.Fiefs).Sum((Town x) => ((SettlementComponent)x).IsCastle ? 1 : 3) + ((clan == kingdom.RulingClan) ? 1 : 0) + 1) / (float)num;
	}

	private void ProgressDailyIncome()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber val = Campaign.Current.Models.ClanFinanceModel.CalculateClanIncome(Clan.PlayerClan, false, false, false);
		int num = (int)((ExplainedNumber)(ref val)).ResultNumber;
		if (num > _cachedMaxDailyIncome)
		{
			SetStatInternal("MaxDailyIncome", num);
			_cachedMaxDailyIncome = num;
		}
	}

	private void ProgressTotalTradeProfit(int profit)
	{
		_cachedTotalTradeProfit += profit;
		SetStatInternal("TotalTradeProfit", _cachedTotalTradeProfit);
	}

	private void CheckProjectsInSettlement(Town town)
	{
		if (town.OwnerClan != Clan.PlayerClan)
		{
			return;
		}
		foreach (Settlement item in ((IEnumerable<Settlement>)Clan.PlayerClan.Settlements).Where((Settlement x) => x.IsFortification))
		{
			bool flag = true;
			foreach (Building item2 in (List<Building>)(object)item.Town.Buildings)
			{
				if (item2.CurrentLevel != 3 && !item2.BuildingType.IsDailyProject)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				SetStatInternal("CompletedAllProjects", 1);
			}
		}
	}

	private void ProgressHighestTierSwordCrafted(ItemObject itemObject)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected I4, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		WeaponComponentData primaryWeapon = itemObject.WeaponComponent.PrimaryWeapon;
		if ((int)primaryWeapon.WeaponClass == 2 || (int)primaryWeapon.WeaponClass == 3)
		{
			SetStatInternal("HighestTierSwordCrafted", itemObject.Tier + 1);
		}
	}

	private void ProgressAssembledDragonBanner()
	{
		if (StoryModeManager.Current.MainStoryLine.FirstPhase != null && StoryModeManager.Current.MainStoryLine.FirstPhase.AllPiecesCollected)
		{
			SetStatInternal("AssembledDragonBanner", 1);
		}
	}

	private void ProgressImperialBarbarianVictory(QuestBase quest, QuestCompleteDetails detail)
	{
		if (quest.IsSpecialQuest && ((object)quest).GetType() == typeof(DefeatTheConspiracyQuestBehavior.DefeatTheConspiracyQuest))
		{
			if (StoryModeManager.Current.MainStoryLine.MainStoryLineSide == MainStoryLineSide.CreateAntiImperialKingdom || StoryModeManager.Current.MainStoryLine.MainStoryLineSide == MainStoryLineSide.SupportAntiImperialKingdom)
			{
				SetStatInternal("BarbarianVictory", 1);
			}
			else
			{
				SetStatInternal("ImperialVictory", 1);
			}
		}
	}

	private void CheckDefeatedSuperiorForce(MapEvent mapEvent)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (mapEvent.IsPlayerMapEvent && mapEvent.Winner == mapEvent.GetMapEventSide(mapEvent.PlayerSide))
		{
			int num = ((IEnumerable<MapEventParty>)mapEvent.GetMapEventSide(mapEvent.DefeatedSide).Parties).Sum((MapEventParty x) => x.HealthyManCountAtStart);
			int num2 = ((IEnumerable<MapEventParty>)mapEvent.GetMapEventSide(mapEvent.WinningSide).Parties).Sum((MapEventParty x) => x.HealthyManCountAtStart);
			if (num - num2 >= 500)
			{
				SetStatInternal("DefeatedSuperiorForce", 1);
			}
		}
	}

	private void CheckTutorialFinished()
	{
		if (!StoryModeManager.Current.MainStoryLine.TutorialPhase.IsSkipped)
		{
			SetStatInternal("FinishedTutorial", 1);
		}
	}

	private void ProgressSuccessfulSiegeCount(MobileParty attackerParty, bool isWin)
	{
		if (attackerParty == MobileParty.MainParty && isWin)
		{
			SetStatInternal("SuccessfulSiegeCount", ++_cachedSuccessfulSiegeCount);
		}
	}

	private void ProgressCapturedATownAlone(MobileParty attackerParty, bool isWin)
	{
		if (attackerParty == MobileParty.MainParty && isWin && attackerParty.Army == null)
		{
			SetStatInternal("CapturedATownAloneCount", ++_cachedCapturedTownAloneCount);
		}
	}

	private void ProgressRepelSiegeAssaultCount(Settlement siegeSettlement, bool isWin)
	{
		if (siegeSettlement.OwnerClan == Clan.PlayerClan && !isWin)
		{
			SetStatInternal("RepelledSiegeAssaultCount", ++_cachedRepelledSiegeAssaultCount);
		}
	}

	private void ProgressRepelSiegeAssaultCount(MapEvent mapEvent)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (mapEvent.MapEventSettlement != null && mapEvent.MapEventSettlement.OwnerClan == Clan.PlayerClan && (int)mapEvent.EventType == 5 && (int)mapEvent.BattleState == 0 && PlayerEncounter.Battle != null && PlayerEncounter.CampaignBattleResult != null && PlayerEncounter.CampaignBattleResult.PlayerVictory)
		{
			SetStatInternal("RepelledSiegeAssaultCount", ++_cachedRepelledSiegeAssaultCount);
		}
	}

	private void ProgressTournamentRank(CharacterObject winner)
	{
		if (winner == CharacterObject.PlayerCharacter && Campaign.Current.TournamentManager.GetLeaderboard()[0].Key == Hero.MainHero)
		{
			SetStatInternal("LeaderOfTournament", 1);
		}
	}

	private void ProgressHeroSkillValue(Hero hero, SkillObject skill, int change = 1, bool shouldNotify = true)
	{
		if (hero == Hero.MainHero && _cachedHighestSkillValue > -1)
		{
			int skillValue = hero.GetSkillValue(skill);
			if (skillValue > _cachedHighestSkillValue)
			{
				SetStatInternal("HighestSkillValue", skillValue);
				_cachedHighestSkillValue = skillValue;
			}
		}
	}

	private void ProgressHideoutClearedCount()
	{
		SetStatInternal("ClearedHideoutCount", ++_cachedHideoutClearedCount);
	}

	private void CheckExecutedLordRelation(Hero victim, Hero killer, KillCharacterActionDetail detail)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if (((killer == Hero.MainHero && (int)detail == 6) || (int)detail == 7) && (int)victim.GetRelationWithPlayer() <= -100)
		{
			SetStatInternal("ExecutedLordRelation100", 1);
		}
	}

	private void ProgressKingOrQueenKilledInBattle(Hero victim, Hero killer, KillCharacterActionDetail detail)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (killer == Hero.MainHero && victim.IsKingdomLeader && (int)detail == 4)
		{
			SetStatInternal("KingOrQueenKilledInBattle", ++_cachedKingOrQueenKilledInBattle);
		}
	}

	private void ProgressTournamentWonCount(CharacterObject winner)
	{
		if (winner == CharacterObject.PlayerCharacter)
		{
			SetStatInternal("WonTournamentCount", ++_cachedWonTournamentCount);
		}
	}

	private void ProgressOwnedWorkshopCount(Workshop workshop, Hero oldOwner)
	{
		if (((SettlementArea)workshop).Owner == Hero.MainHero)
		{
			ProgressHasOwnedCaravanAndWorkshop();
		}
	}

	private void ProgressOwnedCaravanCount(MobileParty party)
	{
		if (party.IsCaravan && party.MapFaction == Hero.MainHero.MapFaction)
		{
			ProgressHasOwnedCaravanAndWorkshop();
		}
	}

	private void ProgressHasOwnedCaravanAndWorkshop()
	{
		if (((List<Workshop>)(object)Hero.MainHero.OwnedWorkshops).Count > 0 && Hero.MainHero.OwnedCaravans.Count > 0)
		{
			SetStatInternal("HasOwnedCaravanAndWorkshop", 1);
		}
	}

	private void ProgressOwnedFortificationCount()
	{
		int num = 0;
		num = ((!Hero.MainHero.IsKingdomLeader) ? ((List<Town>)(object)Hero.MainHero.Clan.Fiefs).Count : ((List<Town>)(object)Hero.MainHero.MapFaction.Fiefs).Count);
		SetStatInternal("OwnedFortificationCount", num);
	}

	private void ProgressCreatedKingdomCount(Kingdom kingdom)
	{
		if (kingdom.Leader == Hero.MainHero)
		{
			SetStatInternal("CreatedKingdomCount", ++_cachedCreatedKingdomCount);
		}
	}

	private void ProgressClanTier(Clan clan, bool shouldNotify)
	{
		if (clan == Clan.PlayerClan && clan.Tier == 6)
		{
			SetStatInternal("ReachedClanTierSix", 1);
		}
	}

	public void DeactivateAchievements(TextObject reason = null, bool showMessage = true, bool temporarily = false)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		_deactivateAchievements = !temporarily || _deactivateAchievements;
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
		CampaignEvents.CollectMetadataEntriesEvent.AddNonSerializedListener((object)this, (Action<List<KeyValuePair<string, string>>>)CollectMetadataEntries);
		if (showMessage)
		{
			if (TextObject.IsNullOrEmpty(reason))
			{
				reason = new TextObject("{=Z9mcDuDi}Achievements are disabled!", (Dictionary<string, object>)null);
			}
			MBInformationManager.AddQuickInformation(reason, 4000, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void CollectMetadataEntries(List<KeyValuePair<string, string>> list)
	{
		list.Add(new KeyValuePair<string, string>("AchievementsDisabled", _deactivateAchievements ? "1" : "0"));
	}

	private void SetStatInternal(string statId, int value)
	{
		if (!_deactivateAchievements)
		{
			AchievementManager.SetStat(statId, value);
		}
	}
}
