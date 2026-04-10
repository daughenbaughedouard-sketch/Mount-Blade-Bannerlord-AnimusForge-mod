using System;
using System.Collections.Generic;
using SandBox.Missions.MissionLogics.Hideout;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.CampaignBehaviors;

public class HideoutConversationsCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
	}

	private void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_008d: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("bandit_hideout_start_defender", "start", "bandit_hideout_defender", "{=nYCXzAYH}You! You've cut quite a swathe through my men there, damn you. How about we settle this, one-on-one?", new OnConditionDelegate(bandit_hideout_start_defender_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("bandit_hideout_start_defender_1", "bandit_hideout_defender", "close_window", "{=dzXaXKaC}Very well.", (OnConditionDelegate)null, new OnConsequenceDelegate(bandit_hideout_start_duel_fight_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("bandit_hideout_start_defender_2", "bandit_hideout_defender", "close_window", "{=ukRZd2AA}I don't fight duels with brigands.", (OnConditionDelegate)null, new OnConsequenceDelegate(bandit_hideout_continue_battle_on_consequence), 100, new OnClickableConditionDelegate(bandit_hideout_continue_battle_on_clickable_condition), (OnPersuasionOptionDelegate)null);
	}

	private bool bandit_hideout_start_defender_on_condition()
	{
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		if (encounteredParty != null)
		{
			IFaction mapFaction = encounteredParty.MapFaction;
			if (((mapFaction != null) ? new bool?(mapFaction.IsBanditFaction) : ((bool?)null)) == true && encounteredParty != null)
			{
				Settlement settlement = encounteredParty.Settlement;
				if (((settlement != null) ? new bool?(settlement.IsHideout) : ((bool?)null)) == true)
				{
					Mission current = Mission.Current;
					if (((current != null) ? current.GetMissionBehavior<HideoutMissionController>() : null) == null)
					{
						Mission current2 = Mission.Current;
						return ((current2 != null) ? current2.GetMissionBehavior<HideoutAmbushMissionController>() : null) != null;
					}
					return true;
				}
			}
		}
		return false;
	}

	private void bandit_hideout_start_duel_fight_on_consequence()
	{
		if (Mission.Current.GetMissionBehavior<HideoutMissionController>() != null)
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutMissionController.StartBossFightDuelMode;
		}
		else if (Mission.Current.GetMissionBehavior<HideoutAmbushMissionController>() != null)
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutAmbushMissionController.StartBossFightDuelMode;
		}
	}

	private void bandit_hideout_continue_battle_on_consequence()
	{
		if (Mission.Current.GetMissionBehavior<HideoutMissionController>() != null)
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutMissionController.StartBossFightBattleMode;
		}
		else if (Mission.Current.GetMissionBehavior<HideoutAmbushMissionController>() != null)
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutAmbushMissionController.StartBossFightBattleMode;
		}
	}

	private bool bandit_hideout_continue_battle_on_clickable_condition(out TextObject explanation)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		bool flag = false;
		foreach (Agent item in (List<Agent>)(object)Mission.Current.PlayerTeam.ActiveAgents)
		{
			if (!item.IsMount && (object)item.Character != CharacterObject.PlayerCharacter)
			{
				flag = true;
				break;
			}
		}
		explanation = TextObject.GetEmpty();
		if (!flag)
		{
			explanation = new TextObject("{=F9HxO1iS}You don't have any men.", (Dictionary<string, object>)null);
		}
		return flag;
	}
}
