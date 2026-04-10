using System;
using Helpers;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Localization;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class LordConversationsStoryModeBehavior : CampaignBehaviorBase
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

	private void AddDialogs(CampaignGameStarter starter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		starter.AddDialogLine("anti_imperial_mentor_introduction", "lord_introduction", "lord_start", "{=TB20aFsf}You probably are aware that I am {CONVERSATION_HERO.FIRSTNAME}. I am not sure why you have sought me out, but know that my old life, as imperial lap-dog, is over.", new OnConditionDelegate(conversation_anti_imperial_mentor_introduction_on_condition), (OnConsequenceDelegate)null, 150, (OnClickableConditionDelegate)null);
		starter.AddDialogLine("imperial_mentor_introduction", "lord_introduction", "lord_start", "{=6aDiS9eP}I am {CONVERSATION_HERO.FIRSTNAME}. You probably already know that, though. Once I wielded great power, but now... Anyway, I am most curious what you might want with me.", new OnConditionDelegate(conversation_imperial_mentor_introduction_on_condition), (OnConsequenceDelegate)null, 150, (OnClickableConditionDelegate)null);
		starter.AddDialogLine("start_default_for_mentors", "start", "lord_start", "{=!}{PLAYER.NAME}...", new OnConditionDelegate(start_default_for_mentors_on_condition), (OnConsequenceDelegate)null, 150, (OnClickableConditionDelegate)null);
	}

	private bool conversation_imperial_mentor_introduction_on_condition()
	{
		if (Campaign.Current.ConversationManager.CurrentConversationIsFirst && Hero.OneToOneConversationHero == StoryModeHeroes.ImperialMentor)
		{
			StringHelpers.SetCharacterProperties("CONVERSATION_HERO", CharacterObject.OneToOneConversationCharacter, (TextObject)null, false);
			return true;
		}
		return false;
	}

	private bool conversation_anti_imperial_mentor_introduction_on_condition()
	{
		if (Campaign.Current.ConversationManager.CurrentConversationIsFirst && Hero.OneToOneConversationHero == StoryModeHeroes.AntiImperialMentor)
		{
			StringHelpers.SetCharacterProperties("CONVERSATION_HERO", CharacterObject.OneToOneConversationCharacter, (TextObject)null, false);
			return true;
		}
		return false;
	}

	private bool start_default_for_mentors_on_condition()
	{
		if (Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.HasMet && (Hero.OneToOneConversationHero == StoryModeHeroes.AntiImperialMentor || Hero.OneToOneConversationHero == StoryModeHeroes.ImperialMentor))
		{
			return true;
		}
		return false;
	}
}
