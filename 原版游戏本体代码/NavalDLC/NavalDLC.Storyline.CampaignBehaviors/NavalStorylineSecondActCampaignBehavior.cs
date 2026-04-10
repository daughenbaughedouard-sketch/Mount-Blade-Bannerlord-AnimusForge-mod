using System;
using System.Runtime.CompilerServices;
using NavalDLC.Storyline.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Storyline.CampaignBehaviors;

public class NavalStorylineSecondActCampaignBehavior : CampaignBehaviorBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConsequenceDelegate _003C_003E9__12_7;

		public static OnConsequenceDelegate _003C_003E9__12_10;

		internal void _003CAddGunnarDialogFlow_003Eb__12_7()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}

		internal void _003CAddGunnarDialogFlow_003Eb__12_10()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}
	}

	private const string _questConversationMenuId = "naval_storyline_act_2_conversation_menu";

	private bool _isQuestAcceptedThroughMission;

	private bool _isIntroGiven;

	private bool _isOption1Selected;

	private bool _isOption2Selected;

	private bool _isOption3Selected;

	public override void RegisterEvents()
	{
		if (!NavalStorylineData.IsNavalStorylineCanceled())
		{
			NavalDLCEvents.OnNavalStorylineCanceledEvent.AddNonSerializedListener((object)this, (Action<NavalStorylineData.StorylineCancelDetail>)OnNavalStorylineCanceled);
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		}
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameSystemStarter)
	{
		AddDialogs();
		AddGameMenus(campaignGameSystemStarter);
	}

	private void AddGameMenus(CampaignGameStarter starter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		starter.AddGameMenu("naval_storyline_act_2_conversation_menu", string.Empty, new OnInitDelegate(naval_storyline_act_2_conversation_menu_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
	}

	private void naval_storyline_act_2_conversation_menu_on_init(MenuCallbackArgs args)
	{
		if (_isQuestAcceptedThroughMission && Mission.Current == null)
		{
			StartQuest();
			_isQuestAcceptedThroughMission = false;
		}
	}

	private void AddDialogs()
	{
		AddGunnarDialogFlow();
	}

	private void OnNavalStorylineCanceled(NavalStorylineData.StorylineCancelDetail detail)
	{
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	private void AddGunnarDialogFlow()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Expected O, but got Unknown
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Expected O, but got Unknown
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Expected O, but got Unknown
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Expected O, but got Unknown
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Expected O, but got Unknown
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Expected O, but got Unknown
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		string text = default(string);
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=LI75U7wB}Well, I sent off my letter, and I spoke to some men from my homeland who have just arrived. They are sick of Sea Hound raids, and ready to join us in our hunt. I am afraid we cannot take your own companions, however. There's not enough room on the ship, and my men aren't willing to trust their lives to any other vessel in these northern seas.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => IsAct2ReadyToStart(NavalStorylineData.Gunnar) && !_isIntroGiven))
			.GetOutputToken(ref text)
			.NpcLine("{=sKumwPNF}I recommend you make ready to sail again soon, but we have a bit of time.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=Vk0n2AHp}Who are these allies? And to whom were you writing your letter?", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(option_1_clickable_condition))
			.Consequence((OnConsequenceDelegate)delegate
			{
				_isOption1Selected = true;
			})
			.NpcLine("{=jzVnKuMC}The ones with us now are farmers from the island of Beinland, near my village at Lagshofn. They are good men, and well-motivated. That coast has suffered greatly at the hands of the Sea Hounds. But they are not warriors who live for battle.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=suM1ocUh}The letter, though, was addressed to one of my very old friends, Bjolgur of Agilting. When he left the rebellion he chose neither peace nor banditry. He chose, instead, to join the Skolderbroda, the Shield Brothers. Rather than fight for a king, he said, he would fight for whoever pays him. A king may turn out to be unworthy of his warriors' valor, but gold is never unworthy.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=Z7FadHk0}I do not pretend that I see eye-to-eye with Bjolgur on all things. But he, like me, prefers to match his skill against other warriors, and he has kin in Beinland. He will not come immediately, as he must take permission from his brotherhood, but when he does come we will be very glad of his help.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption("{=kAFsfSda}Can you tell me more about your past with the Sea Hounds?", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(option_2_clickable_condition))
			.Consequence((OnConsequenceDelegate)delegate
			{
				_isOption2Selected = true;
			})
			.NpcLine("{=n6VFFFoU}I suppose I have time for an old war story or two, if you're truly of a mind to hear…", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=GW2Qa6Iq}As I told you, we fought together against old king Volbjorn. Many of us were from Beinland and other parts of the Nordvyg where even the jarls tread lightly. A man who called himself our 'king' - well, we weren't having any of that. We didn't call ourselves the Sea Hounds back then, but we flew the dogs-head banner, to show our loyalty to each other and to our cause.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=YkjnLIGV}We fought, but we lost, in a great battle in Hvalvik Bay. I and a hundred other captives were led before Volbjorn, and the king ordered his men to take our heads. A dozen or so were killed before me, so I had time to think up something to say. As they held me down to receive the blow, I told Volbjorn that it was a good thing I'd eaten not long ago, so I could let him know directly from the High Hall what sort of feast they'd prepared for him there.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=ImiGnP0a}I thought he'd order me cut open, or flayed, but instead Volbjorn laughed and told me that he liked my mettle. He'd spare my life, and the lives of the rest of my comrades, so long as I swore an oath not to take up arms against him again. Volbjorn had made his point with a dozen headless bodies at his feet, and men paying the land-tax or serving in his armies were of more value to him than corpses. If I swore, a hundred of my comrades could return to their families. So I swore.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=hnSqEnxu}Some of our number never showed up at the battle, however. I can imagine how it went for them after that - mocked in taverns or in scraps of songs sung by children.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=ZOV8GaTO}The north is not forgiving of those who throw away their good name. Their old life, where they were feared rather than scorned, must have seemed much sweeter in comparison. And of course they conceived a great resentment against those of us who faced death and showed them up as cowards. These are the ones who became the Sea Hounds, who aimed to steal the wealth and glory that they failed to win in battle.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption("{=oKRiNpUR}Why did Purig turn on you? ", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(option_3_clickable_condition))
			.Consequence((OnConsequenceDelegate)delegate
			{
				_isOption3Selected = true;
			})
			.NpcLine("{=IV6FiJW4}I do not know for sure. If I had ever knowingly given him insult, I never would have sailed on his ship.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=1Aq78OZl}A true warrior, to my mind, knows his own mettle. He does not need others to remind of his honor. He knows that sometimes in war you suffer bad luck - your ship arrived late, you were carried away in the rush of a rout, you were defeated by a king with forces far greater than your own.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=L4EokSgE}Other men, on the other hand, crave glory. They must hear the cheers of townsfolk and wear gilded armor given to them by kings. They must be envied by other men. And if they aren't, they may hear a little voice in your head telling them that they can steal this glory, that they can slay the weak and take their wealth and buy the respect that they crave.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=l3DKrfv0}That voice spoke to Purig, he gave into it, and it twisted his heart into something truly dark. He is a husk of a man filled only with ambition and wounded pride, all concealed beneath a fair face and a friendly laugh.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption("{=sOtCi0WH}So, that's all I have to ask. What would be our next move?", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => _isOption1Selected && _isOption2Selected && _isOption3Selected))
			.NpcLine("{=oMRN2H6T}Well… I do not think we will have to go very far to start our hunt. A pair of ships have been patrolling off our coast, robbing passing fishermen, and I'd wager one of my eyes to a bale of herring that they're Sea Hounds. They scatter like minnows whenever a real warship sets out, but I think that my men's little vessel would be prey much to their liking. Hopefully, the fight will come to us.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_isIntroGiven = true;
			})
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=qhmolZly}Let's sail out, see if they're Sea Hounds, and sink or take them if they are.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=eLCcTeAX}Right. To the ship, then!", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				if (Mission.Current == null)
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += StartQuest;
				}
				else
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
				}
			})
			.CloseDialog()
			.PlayerOption("{=R7KiYpab}I have things to do on shore.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__12_7;
		if (obj2 == null)
		{
			OnConsequenceDelegate val = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__12_7 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Consequence((OnConsequenceDelegate)obj2).NpcLine("{=Bss21RWb}Very well. Come back here when you're ready.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
			.EndPlayerOptions()
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)null);
		ConversationManager conversationManager2 = Campaign.Current.ConversationManager;
		DialogFlow obj3 = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=7NQsM70B}Are you ready to deal with those two Sea Hound vessels?", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => IsAct2ReadyToStart(NavalStorylineData.Gunnar) && _isIntroGiven))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=NzMX0s21}I am ready.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=eLCcTeAX}Right. To the ship, then!", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				if (Mission.Current == null)
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += StartQuest;
				}
				else
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
				}
			})
			.CloseDialog()
			.PlayerOption("{=8MFLb4X6}I still have things to do on shore.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj4 = _003C_003Ec._003C_003E9__12_10;
		if (obj4 == null)
		{
			OnConsequenceDelegate val2 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__12_10 = val2;
			obj4 = (object)val2;
		}
		conversationManager2.AddDialogFlow(obj3.Consequence((OnConsequenceDelegate)obj4).NpcLine("{=Bss21RWb}Very well. Come back here when you're ready.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)null);
	}

	private bool option_1_clickable_condition(out TextObject explanation)
	{
		explanation = TextObject.GetEmpty();
		return !_isOption1Selected;
	}

	private bool option_2_clickable_condition(out TextObject explanation)
	{
		explanation = TextObject.GetEmpty();
		return !_isOption2Selected;
	}

	private bool option_3_clickable_condition(out TextObject explanation)
	{
		explanation = TextObject.GetEmpty();
		return !_isOption3Selected;
	}

	private bool IsAct2ReadyToStart(Hero conversationHero)
	{
		if (NavalStorylineData.IsStorylineActivationPossible() && NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act1) && Hero.OneToOneConversationHero == conversationHero && conversationHero.HasMet && Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement)
		{
			return !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(DefeatThePiratesQuest));
		}
		return false;
	}

	private void StartQuest()
	{
		((QuestBase)new DefeatThePiratesQuest("naval_storyline_defeat_the_pirates_quest", NavalStorylineData.Gunnar)).StartQuest();
	}

	private void OnPlayerAcceptsQuestThroughMission()
	{
		_isQuestAcceptedThroughMission = true;
		OpenQuestMenu();
		Mission.Current.EndMission();
	}

	private void OpenQuestMenu()
	{
		GameMenu.ActivateGameMenu("naval_storyline_act_2_conversation_menu");
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<bool>("_isIntroGiven", ref _isIntroGiven);
	}
}
