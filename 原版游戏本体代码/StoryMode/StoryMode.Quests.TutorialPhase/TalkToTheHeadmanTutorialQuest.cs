using System;
using System.Collections.Generic;
using Helpers;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.TutorialPhase;

public class TalkToTheHeadmanTutorialQuest : StoryModeQuestBase
{
	[SaveableField(1)]
	private readonly Hero _headman;

	[SaveableField(2)]
	private RecruitTroopsTutorialQuest _recruitTroopsQuest;

	[SaveableField(3)]
	private PurchaseGrainTutorialQuest _purchaseGrainQuest;

	private TextObject _startQuestLog => new TextObject("{=rinefpgo}You have arrived at the village. You can buy some food and hire some men to help hunt for the raiders. First go into the village and talk to the headman.", (Dictionary<string, object>)null);

	private TextObject _readyToGoLog => new TextObject("{=KhL2ctsi}You're ready to leave now. Talk to the headman again. He had said he has a task for you.", (Dictionary<string, object>)null);

	private TextObject _goBackToVillageMenuLog => new TextObject("{=awgBkdXx}You should go back to the village menu and make your preparations to go after the raiders, then find out about the task that the headman has for you.", (Dictionary<string, object>)null);

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=HqlXdzcv}Talk with Headman {HEADMAN.FIRSTNAME}", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("HEADMAN", _headman.CharacterObject, val, false);
			return val;
		}
	}

	public TalkToTheHeadmanTutorialQuest(Hero headman)
		: base("talk_to_the_headman_tutorial_quest", null, CampaignTime.Never)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_headman = headman;
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_headman);
		((QuestBase)this).SetDialogs();
		((QuestBase)this).InitializeQuestOnCreation();
		((QuestBase)this).AddLog(_startQuestLog, false);
		StoryMode.StoryModePhases.TutorialPhase.Instance.SetTutorialFocusSettlement(Settlement.CurrentSettlement);
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
	}

	protected override void HourlyTick()
	{
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener((object)this, (Action)OnBeforeMissionOpened);
	}

	protected override void SetDialogs()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected O, but got Unknown
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Expected O, but got Unknown
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=YEeb0B1V}I am {HEADMAN.FIRSTNAME}, headman of this village. What brings you here?[ib:normal][if:convo_shocked]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(headman_quest_conversation_start_on_condition))
			.PlayerLine(new TextObject("{=StLYbEQZ}We need help. Some raiders have taken our younger brother and sister captive. We think they may have passed this way.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=uNgu02FH}They got your people too? Sorry to hear that. Those bastards have done a bit of killing and looting in these parts as well.[ib:normal2][if:convo_dismayed]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=bNcGO33Q}We think they've gone north. I reckon there are a few folk around here who'll join you in going after them if you'll pay for their gear.[if:convo_thinking]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=5Mw4trfs}Once you've made your preparations, come and talk to me again. I may have a task for you if you are going after the raiders.[if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(headman_quest_conversation_end_on_consequence))
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000009).NpcLine(new TextObject("{=uhYXopnJ}Have you finished your preparations?[if:convo_undecided_open]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == _headman && (!((QuestBase)_recruitTroopsQuest).IsFinalized || !((QuestBase)_purchaseGrainQuest).IsFinalized)))
			.PlayerLine(new TextObject("{=elJCacQO}I am working on it.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=TIaVyqMx}Glad to see you found what you needed. Now, about that matter I mentioned earlier...[if:convo_grave]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(headman_quest_end_conversation_start_on_condition))
			.NpcLine(new TextObject("{=lnAhXvbo}There's this wandering doctor who comes through here from time to time. Name of Tacteos. Treats people for free... We're fond of him.[if:convo_thinking]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=xGdoz9Pn}Well, we last saw him a few days ago. He was carrying some sort of chest, which he was very mysterious about. He was on some sort of 'quest', he said, though wouldn't tell us more.[if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=WDylM3dx}He set off on the road just a few hours before the raiders came through here. Well, he's not really a worldly type, just the kind of fellow who'd stumble into a trap and let himself be captured. We're worried about him.[if:convo_dismayed]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=MREvo37b}If you can keep an eye out for him, this Tacteos, we'd be very grateful. Maybe, if he's alive and well, he'll tell you a little more about his 'quest.'[if:convo_normal]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(headman_quest_end_conversation_start_on_consequence))
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000009).NpcLine(new TextObject("{=gX0RzZoT}Let's just go speak to the headman.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(headman_quest_conversation_talk_with_brother_on_condition))
			.CloseDialog(), (object)this);
	}

	private bool headman_quest_conversation_start_on_condition()
	{
		StringHelpers.SetCharacterProperties("HEADMAN", _headman.CharacterObject, (TextObject)null, false);
		if (Hero.OneToOneConversationHero == _headman)
		{
			return !_headman.HasMet;
		}
		return false;
	}

	private bool headman_quest_conversation_talk_with_brother_on_condition()
	{
		StringHelpers.SetCharacterProperties("BROTHER", StoryModeHeroes.ElderBrother.CharacterObject, (TextObject)null, false);
		if (Hero.OneToOneConversationHero == StoryModeHeroes.ElderBrother)
		{
			return !_headman.HasMet;
		}
		return false;
	}

	private void headman_quest_conversation_end_on_consequence()
	{
		_headman.SetHasMet();
		_headman.SetPersonalRelation(Hero.MainHero, 100);
		_recruitTroopsQuest = new RecruitTroopsTutorialQuest(_headman);
		((QuestBase)_recruitTroopsQuest).StartQuest();
		_purchaseGrainQuest = new PurchaseGrainTutorialQuest(_headman);
		((QuestBase)_purchaseGrainQuest).StartQuest();
		StoryMode.StoryModePhases.TutorialPhase.Instance.SetTutorialQuestPhase(TutorialQuestPhase.RecruitAndPurchaseStarted);
		((QuestBase)this).AddLog(_goBackToVillageMenuLog, false);
	}

	private bool headman_quest_end_conversation_start_on_condition()
	{
		if (Hero.OneToOneConversationHero == _headman && ((QuestBase)_recruitTroopsQuest).IsFinalized)
		{
			return ((QuestBase)_purchaseGrainQuest).IsFinalized;
		}
		return false;
	}

	private void headman_quest_end_conversation_start_on_consequence()
	{
		StoryMode.StoryModePhases.TutorialPhase.Instance.SetLockTutorialVillageEnter(value: false);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	protected override void OnCompleteWithSuccess()
	{
		StoryMode.StoryModePhases.TutorialPhase.Instance.RemoveTutorialFocusSettlement();
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_004e: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		if (((QuestBase)_recruitTroopsQuest).IsFinalized && ((QuestBase)_purchaseGrainQuest).IsFinalized)
		{
			StoryMode.StoryModePhases.TutorialPhase.Instance.SetLockTutorialVillageEnter(value: true);
			TextObject val = new TextObject("{=3YHL3wpM}{BROTHER.NAME}:", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "BROTHER", StoryModeHeroes.ElderBrother.CharacterObject, false);
			InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)new TextObject("{=1xqmoDvS}We have finished our preparations. Let's talk to the headman again. He had said he may have a task for us. We could use his friendship.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=lmG7uRK2}Okay", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			((QuestBase)this).AddLog(_readyToGoLog, false);
		}
	}

	private void OnBeforeMissionOpened()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (Settlement.CurrentSettlement != null && ((MBObjectBase)Settlement.CurrentSettlement).StringId == "village_ES3_2")
		{
			int hitPoints = StoryModeHeroes.ElderBrother.HitPoints;
			int num = 50;
			if (hitPoints < num)
			{
				int num2 = num - hitPoints;
				StoryModeHeroes.ElderBrother.Heal(num2, false);
			}
			LocationCharacter locationCharacterOfHero = LocationComplex.Current.GetLocationCharacterOfHero(StoryModeHeroes.ElderBrother);
			locationCharacterOfHero.CharacterRelation = (CharacterRelations)0;
			PlayerEncounter.LocationEncounter.AddAccompanyingCharacter(locationCharacterOfHero, true);
		}
	}

	internal static void AutoGeneratedStaticCollectObjectsTalkToTheHeadmanTutorialQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(TalkToTheHeadmanTutorialQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_headman);
		collectedObjects.Add(_recruitTroopsQuest);
		collectedObjects.Add(_purchaseGrainQuest);
	}

	internal static object AutoGeneratedGetMemberValue_headman(object o)
	{
		return ((TalkToTheHeadmanTutorialQuest)o)._headman;
	}

	internal static object AutoGeneratedGetMemberValue_recruitTroopsQuest(object o)
	{
		return ((TalkToTheHeadmanTutorialQuest)o)._recruitTroopsQuest;
	}

	internal static object AutoGeneratedGetMemberValue_purchaseGrainQuest(object o)
	{
		return ((TalkToTheHeadmanTutorialQuest)o)._purchaseGrainQuest;
	}
}
