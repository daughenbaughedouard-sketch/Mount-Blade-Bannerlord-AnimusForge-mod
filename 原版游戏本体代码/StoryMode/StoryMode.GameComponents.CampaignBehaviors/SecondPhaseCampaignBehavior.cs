using System;
using System.Collections.Generic;
using System.Linq;
using StoryMode.Quests.SecondPhase;
using StoryMode.Quests.SecondPhase.ConspiracyQuests;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class SecondPhaseCampaignBehavior : CampaignBehaviorBase
{
	private int _conspiracyQuestTriggerDayCounter;

	private bool _isConspiracySetUpStarted;

	public SecondPhaseCampaignBehavior()
	{
		_conspiracyQuestTriggerDayCounter = 0;
		_isConspiracySetUpStarted = false;
	}

	public override void RegisterEvents()
	{
		CampaignEvents.WeeklyTickEvent.AddNonSerializedListener((object)this, (Action)WeeklyTick);
		CampaignEvents.OnQuestStartedEvent.AddNonSerializedListener((object)this, (Action<QuestBase>)OnQuestStarted);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener((object)this, (Action)DailyTick);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnGameLoaded);
		StoryModeEvents.OnConspiracyActivatedEvent.AddNonSerializedListener((object)this, (Action)OnConspiracyActivated);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<int>("_conspiracyQuestTriggerDayCounter", ref _conspiracyQuestTriggerDayCounter);
		dataStore.SyncData<bool>("_isConspiracySetUpStarted", ref _isConspiracySetUpStarted);
	}

	private void WeeklyTick()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		SecondPhase instance = SecondPhase.Instance;
		double num;
		CampaignTime lastConspiracyQuestCreationTime;
		if (instance == null)
		{
			num = 53.0;
		}
		else
		{
			lastConspiracyQuestCreationTime = instance.LastConspiracyQuestCreationTime;
			num = ((CampaignTime)(ref lastConspiracyQuestCreationTime)).ToMilliseconds;
		}
		int num2 = 14 + MBRandom.RandomIntWithSeed((uint)num, 2000u) % 8;
		if (_isConspiracySetUpStarted && StoryModeManager.Current.MainStoryLine.ThirdPhase == null && SecondPhase.Instance.ConspiracyStrength < 2000f)
		{
			lastConspiracyQuestCreationTime = SecondPhase.Instance.LastConspiracyQuestCreationTime;
			if (((CampaignTime)(ref lastConspiracyQuestCreationTime)).ElapsedDaysUntilNow >= (float)num2 && !IsThereActiveConspiracyQuest())
			{
				SecondPhase.Instance.CreateNextConspiracyQuest();
			}
		}
	}

	private void OnQuestStarted(QuestBase quest)
	{
		if (quest is AssembleEmpireQuestBehavior.AssembleEmpireQuest || quest is WeakenEmpireQuestBehavior.WeakenEmpireQuest)
		{
			StoryModeManager.Current.MainStoryLine.CompleteFirstPhase();
			_isConspiracySetUpStarted = true;
		}
	}

	private void DailyTick()
	{
		if (_isConspiracySetUpStarted && _conspiracyQuestTriggerDayCounter < 10)
		{
			_conspiracyQuestTriggerDayCounter++;
			if (_conspiracyQuestTriggerDayCounter >= 10)
			{
				((QuestBase)new ConspiracyProgressQuest()).StartQuest();
			}
		}
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		SecondPhase.Instance?.OnSessionLaunched();
	}

	private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		foreach (MobileParty item in ((IEnumerable<MobileParty>)Campaign.Current.CustomParties).ToList())
		{
			if (!item.Name.HasSameValue(new TextObject("{=eVzg5Mtl}Conspiracy Caravan", (Dictionary<string, object>)null)))
			{
				continue;
			}
			bool flag = true;
			foreach (QuestBase item2 in (List<QuestBase>)(object)Campaign.Current.QuestManager.Quests)
			{
				if (((object)item2).GetType() == typeof(DisruptSupplyLinesConspiracyQuest) && ((DisruptSupplyLinesConspiracyQuest)(object)item2).ConspiracyCaravan == item)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				DestroyPartyAction.Apply((PartyBase)null, item);
			}
		}
	}

	private void OnConspiracyActivated()
	{
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	private bool IsThereActiveConspiracyQuest()
	{
		foreach (QuestBase item in (List<QuestBase>)(object)Campaign.Current.QuestManager.Quests)
		{
			if (item.IsOngoing && typeof(ConspiracyQuestBase) == ((object)item).GetType().BaseType)
			{
				return true;
			}
		}
		return false;
	}
}
