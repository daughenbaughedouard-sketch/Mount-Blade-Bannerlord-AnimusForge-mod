using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace NavalDLC.CampaignBehaviors;

public class NavalDLCTutorialBoxCampaignBehavior : CampaignBehaviorBase
{
	private List<string> _shownTutorials = new List<string>();

	private readonly MBList<CampaignTutorial> _availableTutorials = new MBList<CampaignTutorial>();

	private Dictionary<string, int> _tutorialBackup = new Dictionary<string, int>();

	private List<CampaignTutorial> _tutorialsToResetAfterMission = new List<CampaignTutorial>();

	public MBReadOnlyList<CampaignTutorial> AvailableTutorials => (MBReadOnlyList<CampaignTutorial>)(object)_availableTutorials;

	public override void RegisterEvents()
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.OnTutorialCompletedEvent.AddNonSerializedListener((object)this, (Action<string>)OnTutorialCompleted);
		CampaignEvents.CollectAvailableTutorialsEvent.AddNonSerializedListener((object)this, (Action<List<CampaignTutorial>>)OnTutorialListRequested);
		CampaignEvents.OnQuestStartedEvent.AddNonSerializedListener((object)this, (Action<QuestBase>)OnQuestStarted);
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		Game.Current.EventManager.RegisterEvent<ResetAllTutorialsEvent>((Action<ResetAllTutorialsEvent>)OnResetAllTutorials);
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(Input.OnGamepadActiveStateChanged, new Action(UpdateKeyTexts));
		HotKeyManager.OnKeybindsChanged += new OnKeybindsChangedEvent(UpdateKeyTexts);
		UpdateKeyTexts();
	}

	private void OnMissionEnded(IMission obj)
	{
		if (_tutorialsToResetAfterMission.Count <= 0)
		{
			return;
		}
		foreach (CampaignTutorial item in _tutorialsToResetAfterMission)
		{
			((List<CampaignTutorial>)(object)_availableTutorials).Add(item);
			_shownTutorials.Remove(item.TutorialTypeId);
			if (!_tutorialBackup.ContainsKey(item.TutorialTypeId))
			{
				_tutorialBackup.Add(item.TutorialTypeId, item.Priority);
			}
		}
		((List<CampaignTutorial>)(object)_availableTutorials).Sort((Comparison<CampaignTutorial>)delegate(CampaignTutorial x, CampaignTutorial y)
		{
			int priority = x.Priority;
			return priority.CompareTo(y.Priority);
		});
		_tutorialsToResetAfterMission.Clear();
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<List<string>>("_shownTutorials", ref _shownTutorials);
		dataStore.SyncData<Dictionary<string, int>>("_tutorialBackup", ref _tutorialBackup);
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddTutorial("ShipControlTutorial", 1);
		AddTutorial("ShipOarsmanTutorial", 2);
		AddTutorial("ShipCameraTutorial", 3);
		AddTutorial("ShipSailTutorial", 4);
		AddTutorial("ShipCloseSailTutorial", 5);
		AddTutorial("ShipBoardingApproachTutorial", 6);
		AddTutorial("ShipBoardingAttemptBoardingTutorial", 7);
		AddTutorial("ShipBoardingTroopChargeTutorial", 8);
		AddTutorial("ShipCutLooseTutorial", 9);
		AddTutorial("ShipCommandingShipsTutorial", 10);
		((List<CampaignTutorial>)(object)_availableTutorials).Sort((Comparison<CampaignTutorial>)delegate(CampaignTutorial x, CampaignTutorial y)
		{
			int priority = x.Priority;
			return priority.CompareTo(y.Priority);
		});
	}

	private void OnQuestStarted(QuestBase quest)
	{
		((List<CampaignTutorial>)(object)_availableTutorials).Sort((Comparison<CampaignTutorial>)delegate(CampaignTutorial x, CampaignTutorial y)
		{
			int priority = x.Priority;
			return priority.CompareTo(y.Priority);
		});
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
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
			if (val.TutorialTypeId == "ShipControlTutorial" || val.TutorialTypeId == "ShipSailTutorial" || val.TutorialTypeId == "ShipOarsmanTutorial" || val.TutorialTypeId == "ShipBoardingApproachTutorial" || val.TutorialTypeId == "ShipBoardingAttemptBoardingTutorial" || val.TutorialTypeId == "ShipBoardingTroopChargeTutorial" || val.TutorialTypeId == "ShipCutLooseTutorial" || val.TutorialTypeId == "ShipCommandingShipsTutorial" || val.TutorialTypeId == "ShipCameraTutorial" || val.TutorialTypeId == "ShipCloseSailTutorial")
			{
				_tutorialsToResetAfterMission.Add(val);
			}
			((List<CampaignTutorial>)(object)_availableTutorials).Remove(val);
			_shownTutorials.Add(completedTutorialType);
			_tutorialBackup.Remove(completedTutorialType);
		}
	}

	private void OnTutorialListRequested(List<CampaignTutorial> campaignTutorials)
	{
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

	private static void UpdateKeyTexts()
	{
		string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("NavalShipControlsHotKeyCategory", 110), 1f);
		GameTexts.SetVariable("TOGGLE_SAIL_KEY", keyHyperlinkText);
		string keyHyperlinkText2 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("NavalShipControlsHotKeyCategory", 111), 1f);
		GameTexts.SetVariable("TOGGLE_OARSMEN_KEY", keyHyperlinkText2);
		string keyHyperlinkText3 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("NavalShipControlsHotKeyCategory", 112), 1f);
		GameTexts.SetVariable("TOGGLE_CAMERA_KEY", keyHyperlinkText3);
		string keyHyperlinkText4 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("NavalShipControlsHotKeyCategory", 111), 1f);
		GameTexts.SetVariable("CUT_LOOSE_KEY", keyHyperlinkText4);
		string keyHyperlinkText5 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("NavalShipControlsHotKeyCategory", 114), 1f);
		GameTexts.SetVariable("ATTEMPT_BOARDING_KEY", keyHyperlinkText5);
	}
}
