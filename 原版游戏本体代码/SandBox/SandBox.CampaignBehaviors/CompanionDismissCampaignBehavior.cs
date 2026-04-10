using System;
using SandBox.Conversation;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements.Locations;

namespace SandBox.CampaignBehaviors;

internal class CompanionDismissCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.CompanionRemoved.AddNonSerializedListener((object)this, (Action<Hero, RemoveCompanionDetail>)OnCompanionRemoved);
	}

	private void OnCompanionRemoved(Hero companion, RemoveCompanionDetail detail)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (LocationComplex.Current != null)
		{
			LocationComplex.Current.RemoveCharacterIfExists(companion);
		}
		if (PlayerEncounter.LocationEncounter != null)
		{
			PlayerEncounter.LocationEncounter.RemoveAccompanyingCharacter(companion);
		}
		if ((int)detail == 0 && Hero.MainHero.CurrentSettlement != null)
		{
			AgentNavigator agentNavigator = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator;
			if (agentNavigator?.GetActiveBehavior() is FollowAgentBehavior)
			{
				agentNavigator.GetBehaviorGroup<DailyBehaviorGroup>().RemoveBehavior<FollowAgentBehavior>();
			}
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
