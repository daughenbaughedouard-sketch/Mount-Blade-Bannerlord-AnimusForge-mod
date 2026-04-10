using System;
using System.Collections.Generic;
using NavalDLC.Storyline.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;

namespace NavalDLC.Storyline;

public class DefeatTheCaptorsQuestBehavior : CampaignBehaviorBase
{
	private DefeatTheCaptorsQuest _cachedQuest;

	private static DefeatTheCaptorsQuest Instance
	{
		get
		{
			DefeatTheCaptorsQuestBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<DefeatTheCaptorsQuestBehavior>();
			if (campaignBehavior._cachedQuest != null && ((QuestBase)campaignBehavior._cachedQuest).IsOngoing)
			{
				return campaignBehavior._cachedQuest;
			}
			foreach (QuestBase item in (List<QuestBase>)(object)Campaign.Current.QuestManager.Quests)
			{
				if (item is DefeatTheCaptorsQuest cachedQuest)
				{
					campaignBehavior._cachedQuest = cachedQuest;
					return campaignBehavior._cachedQuest;
				}
			}
			return null;
		}
	}

	public override void RegisterEvents()
	{
		if (!NavalStorylineData.IsNavalStorylineCanceled())
		{
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		}
	}

	private void OnSessionLaunched(CampaignGameStarter gameStarter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		gameStarter.AddGameMenu("defeat_the_captors_after_fight", "{=GDwBJZQr}For a brief moment, your captors seem to have forgotten about you, offering you a chance to break free from your shackles.", new OnInitDelegate(defeat_the_captors_after_fight_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		gameStarter.AddGameMenuOption("defeat_the_captors_after_fight", "defeat_the_captors_after_fight_attack", "{=zxMOqlhs}Attack", new OnConditionDelegate(defeat_the_captors_fight_on_condition), new OnConsequenceDelegate(defeat_the_captors_fight_on_consequence), false, -1, false, (object)null);
	}

	private void defeat_the_captors_after_fight_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
	}

	private bool defeat_the_captors_fight_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		return Instance != null;
	}

	private void defeat_the_captors_fight_on_consequence(MenuCallbackArgs args)
	{
		if (Instance != null)
		{
			Hero.MainHero.Heal(Hero.MainHero.MaxHitPoints, false);
			Instance.StartMission();
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
