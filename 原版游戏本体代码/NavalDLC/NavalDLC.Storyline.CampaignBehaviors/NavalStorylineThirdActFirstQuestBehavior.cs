using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Storyline.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Storyline.CampaignBehaviors;

public class NavalStorylineThirdActFirstQuestBehavior : CampaignBehaviorBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConsequenceDelegate _003C_003E9__13_3;

		public static OnConsequenceDelegate _003C_003E9__13_7;

		public static OnConsequenceDelegate _003C_003E9__13_10;

		internal void _003CAddGunnarInitialDialogFlow_003Eb__13_3()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}

		internal void _003CAddGunnarInitialDialogFlow_003Eb__13_7()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}

		internal void _003CAddGunnarInitialDialogFlow_003Eb__13_10()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}
	}

	private const string _questConversationMenuId = "naval_storyline_act_3_quest_1_conversation_menu";

	private bool _isIntroGiven;

	private bool _isQuestAcceptedThroughMission;

	private SetSailAndEscortTheFortuneSeekersQuest _cachedQuest;

	private IFaction _merchantsFaction;

	private static SetSailAndEscortTheFortuneSeekersQuest Instance
	{
		get
		{
			NavalStorylineThirdActFirstQuestBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<NavalStorylineThirdActFirstQuestBehavior>();
			if (campaignBehavior._cachedQuest != null && ((QuestBase)campaignBehavior._cachedQuest).IsOngoing)
			{
				return campaignBehavior._cachedQuest;
			}
			foreach (QuestBase item in (List<QuestBase>)(object)Campaign.Current.QuestManager.Quests)
			{
				if (item is SetSailAndEscortTheFortuneSeekersQuest cachedQuest)
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
			CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnAfterSessionLaunched);
			NavalDLCEvents.OnNavalStorylineCanceledEvent.AddNonSerializedListener((object)this, (Action<NavalStorylineData.StorylineCancelDetail>)OnNavalStorylineCanceled);
			CampaignEvents.OnQuestStartedEvent.AddNonSerializedListener((object)this, (Action<QuestBase>)OnQuestStarted);
		}
	}

	private void OnQuestStarted(QuestBase quest)
	{
		if (quest is SetSailAndEscortTheFortuneSeekersQuest)
		{
			_merchantsFaction = (IFaction)(object)NavalStorylineData.HomeSettlement.OwnerClan;
		}
	}

	private void OnNavalStorylineCanceled(NavalStorylineData.StorylineCancelDetail detail)
	{
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	private void OnAfterSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddGunnarInitialDialogFlow();
		AddMerchantsDialogueFlow(campaignGameStarter);
		AddGameMenus(campaignGameStarter);
	}

	private void AddGameMenus(CampaignGameStarter gameStarter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		gameStarter.AddGameMenu("naval_storyline_act_3_quest_1_conversation_menu", string.Empty, new OnInitDelegate(naval_storyline_act_3_quest_1_conversation_menu_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
	}

	private void naval_storyline_act_3_quest_1_conversation_menu_on_init(MenuCallbackArgs args)
	{
		if (_isQuestAcceptedThroughMission && Mission.Current == null)
		{
			OnPlayerAgreedToHelp();
			_isQuestAcceptedThroughMission = false;
		}
	}

	private void AddGunnarInitialDialogFlow()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Expected O, but got Unknown
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Expected O, but got Unknown
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Expected O, but got Unknown
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=HTEIIesY}Greetings. Listen… When we sailed with Purig, I was hoping that he would help me fight the Sea Hounds near Hvalvik. His betrayal of course has cost us time, but I think that plan is still a good one.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => IsQuest1ReadyToStart() && !NavalStorylineData.IsTutorialSkipped() && !_isIntroGiven))
			.NpcLine("{=zYEWPvl2}That captive we took, Hralgar, said that the Sea Hounds expect to find rich pickings near Beinland. I think I know what he is talking about. Every year, a Vlandian merchant ship travels to the far north, bearing hunters and other fortune-seekers. It should be returning south around this time. These men have spent the last months gathering walrus ivory, fur and whale oil, all of which are quite valuable in the southlands.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=Tn5mFdcU}Such a prize would be a great boon to the Sea Hounds. I propose that we deny it to them. We can sail to Hvalvik, meet this merchant, and escort them south, sinking or taking any Sea Hounds we encounter.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=DRRUMKFN}Our longship is ready. If you can join me, then we should set out as soon as you are ready.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=xngacVnQ}One thing - it is hard to revictual at sea, so do make sure we have plenty of supplies with us to go to Hvalvik and back. Twenty loads of grain and meat, or the equivalent, should be sufficient for our voyage.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_isIntroGiven = true;
			})
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=SdwdyDGN}I am ready to sail.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=bhUo9L89}Splendid. The tide and winds are with us. Let us go forth!", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				if (Mission.Current == null)
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAgreedToHelp;
				}
				else
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
				}
			})
			.CloseDialog()
			.PlayerOption("{=k07wzat8}I am not ready yet.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__13_3;
		if (obj2 == null)
		{
			OnConsequenceDelegate val = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__13_3 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Consequence((OnConsequenceDelegate)obj2).NpcLine("{=mw07yfTt}Very well. We can wait here for a bit longer for you.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
			.PlayerOption("{=aEKNUI45}This war on the Sea Hounds is too risky. There must be another way to get my sister back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_ransom_sister")
			.EndPlayerOptions(), (object)null);
		ConversationManager conversationManager2 = Campaign.Current.ConversationManager;
		DialogFlow obj3 = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=NSHm5s2u}{PLAYER.NAME}... It's good to see you again! Have you reconsidered joining me in my little feud? I cannot promise you that we will find your sister, but I believe the odds have increased.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
		{
			StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, (TextObject)null, false);
			return IsQuest1ReadyToStart() && NavalStorylineData.IsTutorialSkipped() && !_isIntroGiven;
		})
			.NpcLine("{=XDr67yKI}When last we met, I was intending to sail with my old friend Purig. Well, I always fancied myself a good judge of character, but I suppose fond memories of my warlike youth went to my head like ale. Purig betrayed me. Like so many of my comrades from those days, he turned Sea Hound. I escaped his clutches however, and returned here. I know a great deal more about their operations.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=zYguiNhG}Anyway, I had originally wanted to join up with a merchant returning to Vlandia from Hvalvik, and I think that plan is still a good one. His company has spent the last months hunting and whaling in the far north, and their ship is laden with valuables. I am certain that the Sea Hounds will be unable to resist such a tempting target.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_isIntroGiven = true;
			})
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=SdwdyDGN}I am ready to sail.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=bhUo9L89}Splendid. The tide and winds are with us. Let us go forth!", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				if (Mission.Current == null)
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAgreedToHelp;
				}
				else
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
				}
			})
			.CloseDialog()
			.PlayerOption("{=k07wzat8}I am not ready yet.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj4 = _003C_003Ec._003C_003E9__13_7;
		if (obj4 == null)
		{
			OnConsequenceDelegate val2 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__13_7 = val2;
			obj4 = (object)val2;
		}
		conversationManager2.AddDialogFlow(obj3.Consequence((OnConsequenceDelegate)obj4).NpcLine("{=mw07yfTt}Very well. We can wait here for a bit longer for you.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
			.PlayerOption("{=aEKNUI45}This war on the Sea Hounds is too risky. There must be another way to get my sister back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_ransom_sister")
			.EndPlayerOptions(), (object)null);
		ConversationManager conversationManager3 = Campaign.Current.ConversationManager;
		DialogFlow obj5 = DialogFlow.CreateDialogFlow("start", 1500).NpcLine("{=yJIP3tpk}Are we ready to sail to Hvalvik to escort those Vlandian merchants? They will wait as long as they can, but they cannot wait forever.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => IsQuest1ReadyToStart() && _isIntroGiven))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=qcYkbX2a}Let us sail.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				if (Mission.Current == null)
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAgreedToHelp;
				}
				else
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
				}
			})
			.CloseDialog()
			.PlayerOption("{=yCTF6YvP}I still need more time.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj6 = _003C_003Ec._003C_003E9__13_10;
		if (obj6 == null)
		{
			OnConsequenceDelegate val3 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__13_10 = val3;
			obj6 = (object)val3;
		}
		conversationManager3.AddDialogFlow(obj5.Consequence((OnConsequenceDelegate)obj6).CloseDialog().PlayerOption("{=aEKNUI45}This war on the Sea Hounds is too risky. There must be another way to get my sister back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_ransom_sister")
			.EndPlayerOptions(), (object)null);
	}

	private void AddMerchantsDialogueFlow(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("merchant_meeting_dialogue", "start", "merchant_meeting_player_options_1", "{=lV2EbD7b}Ahoy! Who are you, and what's your purpose?", new OnConditionDelegate(merchant_meeting_dialogue_on_condition), (OnConsequenceDelegate)null, 50000, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("merchant_meeting_dialogue_player_options1_1", "merchant_meeting_player_options_1", "merchant_meeting_npc_answer", "{=zjDk0evO}We're here to escort you, if you'll have us.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("merchant_meeting_dialogue_player_options1_2", "merchant_meeting_player_options_1", "merchant_meeting_npc_answer", "{=1EkgbhaB}We're here making war upon the Sea Hounds, a pirate confederation.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("merchant_meeting_npc_answer_line", "merchant_meeting_npc_answer", "merchant_meeting_player_options_2", "{=MlLDWXuR}If that's the case then we're glad to have you around. Back in Hvalvik port, we heard rumors of these pirates, and we were none too pleased that we had to venture out alone like this. Tell me then, are you asking anything for your services?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("merchant_meeting_dialogue_player_options2_1", "merchant_meeting_player_options_2", "merchant_meeting_npc_answer_2", "{=ZFONiAA3}A small share of your cargo would be customary.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("merchant_meeting_dialogue_player_options2_2", "merchant_meeting_player_options_2", "merchant_meeting_npc_answer_2", "{=ens8bc7I}Merely a chance to fight those slaving bastards.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("merchant_meeting_npc_answer_2_line", "merchant_meeting_npc_answer_2", "close_window", "{=tH5wQo81}Very well. Should we arrive safely, we will happily show our gratitude with a contribution to your cause. The wind is brisk and the waves are choppy, so try not to venture too far away… May the Heavens protect us from pirates and the perils of the sea.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += OnMerchantConversationEnded;
		}, 100, (OnClickableConditionDelegate)null);
	}

	private bool merchant_meeting_dialogue_on_condition()
	{
		if (Instance != null && !Instance.HasMetMerchants && !Instance.HasSavedMerchants)
		{
			return Instance.IsConversationHeroTheMerchant;
		}
		return false;
	}

	private void OnMerchantConversationEnded()
	{
		Instance.OnMerchantsMet();
		PlayerEncounter.Finish(true);
	}

	private bool IsQuest1ReadyToStart()
	{
		if (NavalStorylineData.IsStorylineActivationPossible() && NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act2) && Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement && Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SetSailAndMeetTheFortuneSeekersInTargetSettlementQuest)))
		{
			return !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SetSailAndEscortTheFortuneSeekersQuest));
		}
		return false;
	}

	private void OnPlayerAcceptsQuestThroughMission()
	{
		_isQuestAcceptedThroughMission = true;
		GameMenu.ActivateGameMenu("naval_storyline_act_3_quest_1_conversation_menu");
		Mission.Current.EndMission();
	}

	private void OnPlayerAgreedToHelp()
	{
		((QuestBase)new SetSailAndMeetTheFortuneSeekersInTargetSettlementQuest("naval_storyline_act3_quest1_1", NavalStorylineData.Gunnar, NavalStorylineData.Act3Quest1TargetSettlement)).StartQuest();
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<bool>("_isIntroGiven", ref _isIntroGiven);
		dataStore.SyncData<IFaction>("_merchantsFaction", ref _merchantsFaction);
	}
}
