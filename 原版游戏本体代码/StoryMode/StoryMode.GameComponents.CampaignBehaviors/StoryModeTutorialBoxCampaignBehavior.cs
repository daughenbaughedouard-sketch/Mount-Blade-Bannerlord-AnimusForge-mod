using System;
using System.Collections.Generic;
using StoryMode.Quests.TutorialPhase;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class StoryModeTutorialBoxCampaignBehavior : CampaignBehaviorBase
{
	private List<string> _shownTutorials;

	private readonly MBList<CampaignTutorial> _availableTutorials;

	private Dictionary<string, int> _tutorialBackup;

	public MBReadOnlyList<CampaignTutorial> AvailableTutorials => (MBReadOnlyList<CampaignTutorial>)(object)_availableTutorials;

	public StoryModeTutorialBoxCampaignBehavior()
	{
		_shownTutorials = new List<string>();
		_availableTutorials = new MBList<CampaignTutorial>();
		_tutorialBackup = new Dictionary<string, int>();
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.OnTutorialCompletedEvent.AddNonSerializedListener((object)this, (Action<string>)OnTutorialCompleted);
		CampaignEvents.CollectAvailableTutorialsEvent.AddNonSerializedListener((object)this, (Action<List<CampaignTutorial>>)OnTutorialListRequested);
		CampaignEvents.OnQuestStartedEvent.AddNonSerializedListener((object)this, (Action<QuestBase>)OnQuestStarted);
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		StoryModeEvents.OnTravelToVillageTutorialQuestStartedEvent.AddNonSerializedListener((object)this, (Action)OnTravelToVillageTutorialQuestStarted);
		Game.Current.EventManager.RegisterEvent<ResetAllTutorialsEvent>((Action<ResetAllTutorialsEvent>)OnResetAllTutorials);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<List<string>>("_shownTutorials", ref _shownTutorials);
		dataStore.SyncData<Dictionary<string, int>>("_tutorialBackup", ref _tutorialBackup);
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		BackupTutorial("MovementInMissionTutorial", 5);
		int num = 100;
		BackupTutorial("EncyclopediaHomeTutorial", num++);
		BackupTutorial("EncyclopediaSettlementsTutorial", num++);
		BackupTutorial("EncyclopediaTroopsTutorial", num++);
		BackupTutorial("EncyclopediaKingdomsTutorial", num++);
		BackupTutorial("EncyclopediaClansTutorial", num++);
		BackupTutorial("EncyclopediaConceptsTutorial", num++);
		BackupTutorial("EncyclopediaTrackTutorial", num++);
		BackupTutorial("EncyclopediaSearchTutorial", num++);
		BackupTutorial("EncyclopediaFiltersTutorial", num++);
		BackupTutorial("EncyclopediaSortTutorial", num++);
		BackupTutorial("EncyclopediaFogOfWarTutorial", num++);
		BackupTutorial("RaidVillageStep1", num++);
		BackupTutorial("UpgradingTroopsStep1", num++);
		BackupTutorial("UpgradingTroopsStep2", num++);
		BackupTutorial("UpgradingTroopsStep3", num++);
		BackupTutorial("ChoosingPerkUpgradesStep1", num++);
		BackupTutorial("ChoosingPerkUpgradesStep2", num++);
		BackupTutorial("ChoosingPerkUpgradesStep3", num++);
		BackupTutorial("ChoosingSkillFocusStep1", num++);
		BackupTutorial("ChoosingSkillFocusStep2", num++);
		BackupTutorial("GettingCompanionsStep1", num++);
		BackupTutorial("GettingCompanionsStep2", num++);
		BackupTutorial("GettingCompanionsStep3", num++);
		BackupTutorial("RansomingPrisonersStep1", num++);
		BackupTutorial("RansomingPrisonersStep2", num++);
		BackupTutorial("EquipmentSets", num++);
		BackupTutorial("PartySpeed", num++);
		BackupTutorial("ArmyCohesionStep1", num++);
		BackupTutorial("ArmyCohesionStep2", num++);
		BackupTutorial("CreateArmyStep2", num++);
		BackupTutorial("CreateArmyStep3", num++);
		BackupTutorial("OrderOfBattleTutorialStep1", num++);
		BackupTutorial("OrderOfBattleTutorialStep2", num++);
		BackupTutorial("OrderOfBattleTutorialStep3", num++);
		BackupTutorial("CraftingStep1Tutorial", num++);
		BackupTutorial("CraftingOrdersTutorial", num++);
		BackupTutorial("InventoryBannerItemTutorial", num++);
		BackupTutorial("CrimeTutorial", num++);
		BackupTutorial("AssignRolesTutorial", num++);
		BackupTutorial("BombardmentStep1", num++);
		BackupTutorial("KingdomDecisionVotingTutorial", num++);
		foreach (KeyValuePair<string, int> item in _tutorialBackup)
		{
			AddTutorial(item.Key, item.Value);
		}
	}

	private void OnTravelToVillageTutorialQuestStarted()
	{
		AddTutorial("SeeMarkersInMissionTutorial", 1);
		AddTutorial("NavigateOnMapTutorialStep1", 2);
		AddTutorial("NavigateOnMapTutorialStep2", 3);
		AddTutorial("EnterVillageTutorial", 4);
	}

	private void OnQuestStarted(QuestBase quest)
	{
		if (quest is PurchaseGrainTutorialQuest)
		{
			AddTutorial("PressLeaveToReturnFromMissionType1", 10);
			AddTutorial("GetSuppliesTutorialStep1", 20);
			AddTutorial("GetSuppliesTutorialStep3", 22);
		}
		else if (quest is RecruitTroopsTutorialQuest)
		{
			AddTutorial("RecruitmentTutorialStep1", 11);
			AddTutorial("RecruitmentTutorialStep2", 12);
		}
		else if (quest is LocateAndRescueTravellerTutorialQuest)
		{
			AddTutorial("PressLeaveToReturnFromMissionType2", 30);
			AddTutorial("OrderTutorial1TutorialStep2", 33);
			AddTutorial("TakeAndRescuePrisonerTutorial", 34);
			AddTutorial("OrderTutorial2Tutorial", 35);
		}
		else if (quest is VillagersInNeed)
		{
			AddTutorial("StealthCrouchTutorial", 36);
			AddTutorial("StealthWalkSlowTutorial", 37);
			AddTutorial("StealthHideInBushesTutorial", 38);
			AddTutorial("StealthDistractionTutorial", 39);
			AddTutorial("StealthDarkZoneTutorial", 40);
			AddTutorial("StealthStealthKillTutorial", 41);
			AddTutorial("StealthHideCorpseTutorial", 42);
		}
		((List<CampaignTutorial>)(object)_availableTutorials).Sort((Comparison<CampaignTutorial>)delegate(CampaignTutorial x, CampaignTutorial y)
		{
			int priority = x.Priority;
			return priority.CompareTo(y.Priority);
		});
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		if (TutorialPhase.Instance.TutorialQuestPhase == TutorialQuestPhase.RecruitAndPurchaseStarted && ((quest is RecruitTroopsTutorialQuest && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(PurchaseGrainTutorialQuest))) || (quest is PurchaseGrainTutorialQuest && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(RecruitTroopsTutorialQuest)))))
		{
			AddTutorial("TalkToNotableTutorialStep1", 40);
			AddTutorial("TalkToNotableTutorialStep2", 41);
		}
		((List<CampaignTutorial>)(object)_availableTutorials).Sort((Comparison<CampaignTutorial>)delegate(CampaignTutorial x, CampaignTutorial y)
		{
			int priority = x.Priority;
			return priority.CompareTo(y.Priority);
		});
	}

	private void OnTutorialCompleted(string completedTutorialType)
	{
		CampaignTutorial val = ((List<CampaignTutorial>)(object)_availableTutorials).Find((Predicate<CampaignTutorial>)((CampaignTutorial t) => t.TutorialTypeId == completedTutorialType));
		if (val != null)
		{
			((List<CampaignTutorial>)(object)_availableTutorials).Remove(val);
			_shownTutorials.Add(completedTutorialType);
			_tutorialBackup.Remove(completedTutorialType);
		}
	}

	private void OnTutorialListRequested(List<CampaignTutorial> campaignTutorials)
	{
		if (!BannerlordConfig.EnableTutorialHints)
		{
			return;
		}
		MBTextManager.SetTextVariable("TUTORIAL_SETTLEMENT_NAME", MBObjectManager.Instance.GetObject<Settlement>("village_ES3_2").Name, false);
		foreach (CampaignTutorial item in (List<CampaignTutorial>)(object)AvailableTutorials)
		{
			campaignTutorials.Add(item);
		}
	}

	private void BackupTutorial(string tutorialTypeId, int priority)
	{
		if (!_shownTutorials.Contains(tutorialTypeId) && !_tutorialBackup.ContainsKey(tutorialTypeId))
		{
			_tutorialBackup.Add(tutorialTypeId, priority);
		}
	}

	private void AddTutorial(string tutorialTypeId, int priority)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (!_shownTutorials.Contains(tutorialTypeId))
		{
			CampaignTutorial item = new CampaignTutorial(tutorialTypeId, priority);
			((List<CampaignTutorial>)(object)_availableTutorials).Add(item);
			if (!_tutorialBackup.ContainsKey(tutorialTypeId))
			{
				_tutorialBackup.Add(tutorialTypeId, priority);
			}
		}
	}

	public void OnResetAllTutorials(ResetAllTutorialsEvent obj)
	{
		_shownTutorials.Clear();
	}
}
