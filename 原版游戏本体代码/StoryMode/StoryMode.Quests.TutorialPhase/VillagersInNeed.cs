using System;
using System.Collections.Generic;
using Helpers;
using StoryMode.Missions;
using StoryMode.StoryModePhases;
using Storymode.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.TutorialPhase;

public class VillagersInNeed : StoryModeQuestBase
{
	public const string StealthEquipmentId = "stealth_tutorial_set_player";

	public const string VillaSceneId = "villa_singular_c";

	public const string HeadmanId = "tutorial_npc_captive_headman";

	private const string VillagerId = "tutorial_npc_questgiver_villager";

	[SaveableField(1)]
	private bool _talkedToVillagers;

	[SaveableField(2)]
	private bool _failedTheMission;

	private bool _startVillaMission;

	private bool _isHeadmanFollowing;

	private bool _rescuedHeadman;

	private static int SettlementBusyPriority => 400;

	public override TextObject Title => new TextObject("{=Cv2W7aFu}Villagers in Need", (Dictionary<string, object>)null);

	private TextObject _startQuestLogTutorialNotSkipped
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=sbX4fQ0R}A boy came to your camp and told you some of Radagos' men returned to {VILLAGE_LINK} and took the headman hostage.", (Dictionary<string, object>)null);
			val.SetTextVariable("VILLAGE_LINK", _village.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _startQuestLogTutorialSkipped
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=Iu7tpHsO}A boy came to your camp and told you the villagers of {VILLAGE_LINK} need your help rescuing their headman from a group of bandits.", (Dictionary<string, object>)null);
			val.SetTextVariable("VILLAGE_LINK", _village.EncyclopediaLinkWithName);
			return val;
		}
	}

	private Settlement _village => Settlement.Find("village_ES3_2");

	public CharacterObject Headman => MBObjectManager.Instance.GetObject<CharacterObject>("tutorial_npc_captive_headman");

	private CharacterObject _villager => MBObjectManager.Instance.GetObject<CharacterObject>("tutorial_npc_questgiver_villager");

	public VillagersInNeed()
		: base("talk_to_villagers_in_village_quest", null, CampaignTime.Never)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_village);
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		((QuestBase)this).InitializeQuestOnCreation();
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
	}

	protected override void OnStartQuest()
	{
		((QuestBase)this).AddLog(StoryMode.StoryModePhases.TutorialPhase.Instance.IsSkipped ? _startQuestLogTutorialSkipped : _startQuestLogTutorialNotSkipped, false);
		Extensions.GetRandomElementWithPredicate<Hero>(Hero.AllAliveHeroes, (Func<Hero, bool>)((Hero t) => (int)t.Occupation == 20 && t.Culture == _village.Culture));
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)GameMenuOpened);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		CampaignEvents.IsSettlementBusyEvent.AddNonSerializedListener((object)this, (ReferenceAction<Settlement, object, int>)IsSettlementBusy);
	}

	private void IsSettlementBusy(Settlement settlement, object asker, ref int priority)
	{
		if (settlement == _village && asker != this)
		{
			priority = Math.Max(priority, SettlementBusyPriority);
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		_isHeadmanFollowing = false;
	}

	private void GameMenuOpened(MenuCallbackArgs args)
	{
		if (Settlement.CurrentSettlement == _village)
		{
			if (args.MenuContext.GameMenu.StringId == "village" && _startVillaMission)
			{
				StartVillaMission();
			}
			if (args.MenuContext.GameMenu.StringId == "village" && _rescuedHeadman)
			{
				OpenConversationWithHeadman();
			}
		}
	}

	private void OnGameLoadFinished()
	{
		AddGameMenus();
	}

	private void AddGameMenus()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0035: Expected O, but got Unknown
		//IL_0035: Expected O, but got Unknown
		((QuestBase)this).AddGameMenuOption("village", "talk_to_villager", new TextObject("{=Q5jUW8Oa}Talk to the villager", (Dictionary<string, object>)null), new OnConditionDelegate(village_talk_to_villager_on_condition), new OnConsequenceDelegate(village_talk_to_villager_on_consequence), false, 4);
	}

	private void village_talk_to_villager_on_consequence(MenuCallbackArgs args)
	{
		OpenConversationWithVillager();
	}

	private bool village_talk_to_villager_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		args.OptionQuestData = (IssueQuestFlags)4;
		args.optionLeaveType = (LeaveType)22;
		if (Hero.MainHero.IsWounded)
		{
			args.IsEnabled = false;
			args.Tooltip = new TextObject("{=yNMrF2QF}You are wounded", (Dictionary<string, object>)null);
		}
		if (Settlement.CurrentSettlement == _village)
		{
			return _talkedToVillagers;
		}
		return false;
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		if (!_talkedToVillagers && settlement == _village && party == MobileParty.MainParty)
		{
			OpenConversationWithVillager();
		}
	}

	private void OpenConversationWithVillager()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		CampaignMission.OpenConversationMission(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, false, false, false, false, false), new ConversationCharacterData(_villager, (PartyBase)null, true, true, false, false, false, false), "", "", false);
	}

	private void OpenConversationWithHeadman()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		CampaignMission.OpenConversationMission(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, false, false, false, false, false), new ConversationCharacterData(Headman, (PartyBase)null, true, true, false, false, false, false), "", "", false);
	}

	protected override void SetDialogs()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Expected O, but got Unknown
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Expected O, but got Unknown
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Expected O, but got Unknown
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Expected O, but got Unknown
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Expected O, but got Unknown
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Expected O, but got Unknown
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Expected O, but got Unknown
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Expected O, but got Unknown
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Expected O, but got Unknown
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Expected O, but got Unknown
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Expected O, but got Unknown
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Expected O, but got Unknown
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Expected O, but got Unknown
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Expected O, but got Unknown
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Expected O, but got Unknown
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Expected O, but got Unknown
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Expected O, but got Unknown
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Expected O, but got Unknown
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Expected O, but got Unknown
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Expected O, but got Unknown
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Expected O, but got Unknown
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Expected O, but got Unknown
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Expected O, but got Unknown
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Expected O, but got Unknown
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Expected O, but got Unknown
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Expected O, but got Unknown
		//IL_0502: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Expected O, but got Unknown
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Expected O, but got Unknown
		//IL_052a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Expected O, but got Unknown
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Expected O, but got Unknown
		string text = default(string);
		string text2 = default(string);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=!}{VILLAGER_DIALOGUE_1}", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(talk_to_villagers_on_condition))
			.NpcLine(new TextObject("{=!}{VILLAGER_DIALOGUE_2}", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=vxYaxWwC}They've holed up in a ruined villa a short distance from here, and say that if we try to rescue the headman they'll cut his throat then and there. But surely you could save him? You could sneak in there and get him out?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GenerateToken(ref text)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=3sI7nPbF}Hmm. I guess it's mostly a matter of waiting until their back is turned, then moving quickly from cover to cover.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, text)
			.PlayerOption(new TextObject("{=mFTTWH03}I shall pass through them unseen, cloaked in silence and shadow.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, text)
			.EndPlayerOptions()
			.NpcLine(new TextObject("{=n5fELaJd}We can give you some things that might help you. We have some special, softer boots, that our hunters use when they go out at night - when you walk, you'll barely make any noise at all. And some darkened clothes. We have some that would fit you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, text, (string)null)
			.NpcLine(new TextObject("{=3ee3I8WX}Also, this dagger... It's probably safest just to get to the headman as stealthily as you can, and then sneak back out. But if there's just no getting around one of them, you can take this and come up behind him, and that would make a lot less noise than a straight-out fight.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GenerateToken(ref text2)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=HpYpZEt3}Right. I'll don those hunting clothes and see what I can do.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, text2)
			.PlayerOption(new TextObject("{=q4bjsoKj}Let this dagger be the hand of the angel of death.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, text2)
			.EndPlayerOptions()
			.NpcLine(new TextObject("{=z9prKkWu}Thank you, my {?PLAYER.GENDER}lady{?}lord{\\?}. I'll take you right now to the villa. This is a good time - they took some wine when they raided us, and I doubt they'll be on their guard.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, text2, (string)null)
			.Consequence(new OnConsequenceDelegate(talk_to_villagers_not_skipped_on_consequence))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=glarczej}Lead on.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_startVillaMission = true;
			})
			.CloseDialog()
			.PlayerOption(new TextObject("{=nhSLTzHk}I have to take care of something else, first.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=AZ30Q0nM}Heaven bless you, {?PLAYER.GENDER}madame{?}sir{\\?}. Shall I take you to where the bandits are holding the headman?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(talk_to_villagers_later_on_condition))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=glarczej}Lead on.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_startVillaMission = true;
			})
			.CloseDialog()
			.PlayerOption(new TextObject("{=nhSLTzHk}I have to take care of something else, first.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=IGdpap9P}We saw what happened, {?PLAYER.GENDER}madame{?}sir{\\?}, but we think that drunken lot have all gone to sleep again. Maybe you could try again, {?PLAYER.GENDER}madame{?}sir{\\?}? We would be forever in your debt.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(talk_to_villagers_failed_on_condition))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=glarczej}Lead on.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_startVillaMission = true;
				_failedTheMission = false;
			})
			.NpcLine(new TextObject("{=76acv5m2}Whatever happens, we're forever in your debt.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.PlayerOption(new TextObject("{=nhSLTzHk}I have to take care of something else, first.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=krsbwYax}Come find us here in Tevea when you're ready, {?PLAYER.GENDER}madame{?}sir{\\?}.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=7aAFEx7e}You're not one of them! Who are you? What's happening?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(talk_to_headman_in_villa_skipped_on_condition))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=bcZaWOZM}I'll find a way out. Follow me as soon as it’s safe.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(talk_to_headman_in_villa_on_consequence))
			.CloseDialog()
			.PlayerOption(new TextObject("{=nfMWzDbw}Be silent! I shall clear a path past them.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(talk_to_headman_in_villa_on_consequence))
			.CloseDialog()
			.EndPlayerOptions(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=dykrJl5v}{PLAYER.NAME}! What's happening?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(talk_to_headman_in_villa_not_skipped_on_condition))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=bcZaWOZM}I'll find a way out. Follow me as soon as it’s safe.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(talk_to_headman_in_villa_on_consequence))
			.CloseDialog()
			.PlayerOption(new TextObject("{=nfMWzDbw}Be silent! I shall clear a path past them.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(talk_to_headman_in_villa_on_consequence))
			.CloseDialog()
			.EndPlayerOptions(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=bN3zJKz5}As soon as you find an escape route, I will follow.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(talk_to_headman_in_villa_after_talking_on_condition))
			.CloseDialog(), (object)this);
		string text3 = default(string);
		string text4 = default(string);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=!}{HEADMAN_DIALOGUE_1}", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(talk_to_headman_after_rescue_on_condition))
			.GenerateToken(ref text3)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=xJUJmrTb}I'm always glad to help honest folk like yourselves.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, text3)
			.PlayerOption(new TextObject("{=y3gl2ada}Perhaps you would like to express a more tangible form of gratitude.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, text3)
			.EndPlayerOptions()
			.NpcLine(new TextObject("{=!}{HEADMAN_DIALOGUE_2}", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, text3, (string)null)
			.GenerateToken(ref text4)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=gLVaQeAL}I'll take it. I need whatever I can get.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, text4)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += TakeRewards;
			})
			.PlayerOption(new TextObject("{=xj5dlLXa}Keep your money, my good man. You've lost too much already.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, text4)
			.EndPlayerOptions()
			.NpcLine(new TextObject("{=x3vZ8iQC}Then thank you again. We here in Tevea won't forget what you've done for us.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, text4, (string)null)
			.Consequence(new OnConsequenceDelegate(((QuestBase)this).CompleteQuestWithSuccess))
			.CloseDialog(), (object)this);
	}

	protected override void OnCompleteWithSuccess()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		TextObject val = new TextObject("{=LcPHw4m2}You rescued the headman from the bandits, and returned him to the village of {VILLAGE_LINK}.", (Dictionary<string, object>)null);
		val.SetTextVariable("VILLAGE_LINK", _village.EncyclopediaLinkWithName);
		((QuestBase)this).AddLog(val, false);
		MBEquipmentRoster val2 = MBObjectManager.Instance.GetObject<MBEquipmentRoster>("stealth_tutorial_set_player");
		for (int i = 0; i < 12; i++)
		{
			EquipmentElement val3 = val2.DefaultEquipment[i];
			if (!((EquipmentElement)(ref val3)).IsEmpty)
			{
				MobileParty.MainParty.ItemRoster.AddToCounts(val2.DefaultEquipment[i], 1);
			}
		}
	}

	private void talk_to_headman_in_villa_on_consequence()
	{
		Mission.Current.GetMissionBehavior<SneakIntoTheVillaMissionController>().OnAfterTalkingToPrisoner();
		_isHeadmanFollowing = true;
	}

	private bool talk_to_headman_in_villa_on_condition()
	{
		int num;
		if (CharacterObject.OneToOneConversationCharacter == Headman && _talkedToVillagers && !_isHeadmanFollowing)
		{
			num = ((!_rescuedHeadman) ? 1 : 0);
			if (num != 0)
			{
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private bool talk_to_headman_in_villa_after_talking_on_condition()
	{
		int num;
		if (CharacterObject.OneToOneConversationCharacter == Headman && _talkedToVillagers && _isHeadmanFollowing)
		{
			num = ((!_rescuedHeadman) ? 1 : 0);
			if (num != 0)
			{
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private bool talk_to_headman_in_villa_skipped_on_condition()
	{
		if (talk_to_headman_in_villa_on_condition())
		{
			return StoryMode.StoryModePhases.TutorialPhase.Instance.IsSkipped;
		}
		return false;
	}

	private bool talk_to_headman_in_villa_not_skipped_on_condition()
	{
		if (talk_to_headman_in_villa_on_condition())
		{
			return !StoryMode.StoryModePhases.TutorialPhase.Instance.IsSkipped;
		}
		return false;
	}

	private void TakeRewards()
	{
		GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, 100, false);
	}

	private bool talk_to_headman_after_rescue_on_condition()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		int num;
		if (CharacterObject.OneToOneConversationCharacter == Headman && _talkedToVillagers)
		{
			num = (_rescuedHeadman ? 1 : 0);
			if (num != 0)
			{
				if (StoryMode.StoryModePhases.TutorialPhase.Instance.IsSkipped)
				{
					MBTextManager.SetTextVariable("HEADMAN_DIALOGUE_1", new TextObject("{=sbqpaU64}Thank you, {?PLAYER.GENDER}madame{?}sir{\\?}! You saved my life, and put the fear of Heaven into those villains, I'm sure!  We can handle things from here. There's just a few of them left, and we won't let them take us unaware again. With all our hearts, thank you.", (Dictionary<string, object>)null), false);
				}
				else
				{
					MBTextManager.SetTextVariable("HEADMAN_DIALOGUE_1", new TextObject("{=L5KshziU}{PLAYER.NAME}... Once again, you've helped us fend off those villains. We can handle things from here, I'm sure. There's just a few of them left, and we won't let them take us unaware again. Thank you. With all our hearts, thank you.", (Dictionary<string, object>)null), false);
				}
				if (StoryMode.StoryModePhases.TutorialPhase.Instance.IsSkipped)
				{
					MBTextManager.SetTextVariable("HEADMAN_DIALOGUE_2", new TextObject("{=9IHnjuXb}We'd heard that you're trying to find your family. Our heart goes out to you, {?PLAYER.GENDER}madame{?}sir{\\?}. That dagger and those hunting clothes - please take them. We pray they can be of use. And I have 100 denars that I'd been saving, but I want you to have it. If it helps you at all, I'd be glad.", (Dictionary<string, object>)null), false);
				}
				else
				{
					MBTextManager.SetTextVariable("HEADMAN_DIALOGUE_2", new TextObject("{=LRkKxcmX}We know you've got a long road ahead of you, trying to find your family. That dagger and those hunting clothes - please take them. We pray they can be of use. And I have 100 denars that I'd been saving, but I want you to have it. If it helps you at all to find your poor brother and sister, I'd be glad.", (Dictionary<string, object>)null), false);
				}
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private void talk_to_villagers_not_skipped_on_consequence()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		TextObject val = new TextObject("{=4ezrToWI}You agreed to help the villagers and try to save their headman from a nearby villa.", (Dictionary<string, object>)null);
		((QuestBase)this).AddLog(val, false);
		_talkedToVillagers = true;
	}

	private void StartVillaMission()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		StoryModeMissions.OpenSneakIntoTheVillaMission("villa_singular_c", CampaignTime.Now);
		_startVillaMission = false;
	}

	private bool talk_to_villagers_later_on_condition()
	{
		int num;
		if (Mission.Current != null && CharacterObject.OneToOneConversationCharacter != null && Settlement.CurrentSettlement == _village && !_failedTheMission && _talkedToVillagers)
		{
			num = ((CharacterObject.OneToOneConversationCharacter == _villager) ? 1 : 0);
			if (num != 0)
			{
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private bool talk_to_villagers_failed_on_condition()
	{
		int num;
		if (Mission.Current != null && CharacterObject.OneToOneConversationCharacter != null && Settlement.CurrentSettlement == _village && _talkedToVillagers && _failedTheMission)
		{
			num = ((CharacterObject.OneToOneConversationCharacter == _villager) ? 1 : 0);
			if (num != 0)
			{
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private bool talk_to_villagers_on_condition()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		int num;
		if (Mission.Current != null && CharacterObject.OneToOneConversationCharacter != null && Settlement.CurrentSettlement == _village && !_talkedToVillagers)
		{
			num = ((CharacterObject.OneToOneConversationCharacter == _villager) ? 1 : 0);
			if (num != 0)
			{
				if (StoryMode.StoryModePhases.TutorialPhase.Instance.IsSkipped)
				{
					MBTextManager.SetTextVariable("VILLAGER_DIALOGUE_1", new TextObject("{=toszqdCj}Thank Heaven our lad found you! Please, {?PLAYER.GENDER}madame{?}sir{\\?}, we'd heard about that terrible affair at the inn, and that a couple of warriors were planning to track those killers down. Are you one of them? We thought you could help us.", (Dictionary<string, object>)null), false);
				}
				else
				{
					MBTextManager.SetTextVariable("VILLAGER_DIALOGUE_1", new TextObject("{=PkoWqYPD}Thank Heaven our lad found you! Please, {?PLAYER.GENDER}madame{?}sir{\\?}, you've done so much for us, but we beg you not to forsake us now.", (Dictionary<string, object>)null), false);
				}
				if (StoryMode.StoryModePhases.TutorialPhase.Instance.IsSkipped)
				{
					MBTextManager.SetTextVariable("VILLAGER_DIALOGUE_2", new TextObject("{=55avOQ4k}Listen, {?PLAYER.GENDER}madame{?}sir{\\?}... It seems like a small group of bandits broke off from the main group, and now they have our headman. They're demanding a ransom - a half-dozen horses and ten sacks of grain. After all their theft and villainy we have no horses at all, sirs, and the grain would leave us nothing to plant!", (Dictionary<string, object>)null), false);
				}
				else
				{
					MBTextManager.SetTextVariable("VILLAGER_DIALOGUE_2", new TextObject("{=datALLCZ}When our lads came back, and said you'd led them to victory over Radagos and his gang, we thought the danger had passed. But it looks like we rejoiced too soon. A few desperate bandits got away, and now they have our headman. They're demanding a ransom - a half-dozen horses and ten sacks of grain. After all their theft and villainy we have no horses at all, sirs, and the grain would leave us nothing to plant!", (Dictionary<string, object>)null), false);
				}
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	public void OnRescueMissionFailed()
	{
		_failedTheMission = true;
	}

	public void OnHeadmanRescued()
	{
		_rescuedHeadman = true;
	}

	internal static void AutoGeneratedStaticCollectObjectsVillagersInNeed(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(VillagersInNeed)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	internal static object AutoGeneratedGetMemberValue_talkedToVillagers(object o)
	{
		return ((VillagersInNeed)o)._talkedToVillagers;
	}

	internal static object AutoGeneratedGetMemberValue_failedTheMission(object o)
	{
		return ((VillagersInNeed)o)._failedTheMission;
	}
}
