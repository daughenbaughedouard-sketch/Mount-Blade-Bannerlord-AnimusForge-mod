using System.Collections.Generic;
using System.Linq;
using Helpers;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.FirstPhase;

public class MeetWithIstianaQuest : StoryModeQuestBase
{
	[SaveableField(1)]
	private bool _metImperialMentor;

	private TextObject _startQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=h9VP4ypW}Find and meet {HERO.LINK} to learn more about Neretzes' Banner. She is currently in {SETTLEMENT}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("HERO", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			val.SetTextVariable("SETTLEMENT", StoryModeHeroes.ImperialMentor.CurrentSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _endQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=kTaYz2mo}You talked with {HERO.NAME}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("HERO", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=Y6SqyQwn}Meet with {HERO.NAME}", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("HERO", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	public override bool IsRemainingTimeHidden => false;

	public MeetWithIstianaQuest(Settlement settlement)
		: base("meet_with_istiana_story_mode_quest", null, StoryModeManager.Current.MainStoryLine.FirstPhase.FirstPhaseEndTime)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_metImperialMentor = false;
		((QuestBase)this).SetDialogs();
		HeroHelper.SpawnHeroForTheFirstTime(StoryModeHeroes.ImperialMentor, settlement);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)settlement);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.ImperialMentor);
		((QuestBase)this).AddLog(_startQuestLog, false);
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
	}

	protected override void HourlyTick()
	{
	}

	protected override void SetDialogs()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Expected O, but got Unknown
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Expected O, but got Unknown
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Expected O, but got Unknown
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Expected O, but got Unknown
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Expected O, but got Unknown
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Expected O, but got Unknown
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Expected O, but got Unknown
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Expected O, but got Unknown
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Expected O, but got Unknown
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Expected O, but got Unknown
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("lord_start", 110).NpcLine(new TextObject("{=5UHbg6D0}So. What brings you to me?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == StoryModeHeroes.ImperialMentor && !_metImperialMentor))
			.PlayerLine(new TextObject("{=tfm5hcks}I believe I have a piece of the Dragon Banner of Calradios.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=0P4HqZiB}Is that true?[ib:normal][if:convo_shocked]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=ZDEcFXIm}You may have one piece of the banner, but it's of little use in itself. You'll have to find the other parts. But once you can bring together the pieces, you'll have something of tremendous value.[if:convo_undecided_open]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=t71lPdyb}How so?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=40oa2Nav}The banner of Calradios is part of a legend. They say it was carried by Calradios the Great as he led his small band of exiles into this land to make a new home for themselves. They say that, so long as it is carried by a true son of Calradios, he shall never be defeated in battle. Or a daughter, I imagine, although that has never come up.[ib:confident][if:convo_undecided_closed]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=xjduipCO}Of course our glorious armies have been defeated many times, but I guess those commanders and emperors were not 'true sons.' Clever little legend. A child could see through it, if she tried, but of course people never try to see through the noble lies that bind us together. Thank Heaven for that.[ib:closed][if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=FBp2ranI}So, can you help me find a buyer for it?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=WWcG7kPr}A 'buyer'? Think bigger than that. Let me just say that, if you can find the missing pieces, I am sure I can help you put it to good use.[ib:confident2][if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=MnmblprY}So, where can I find the other pieces?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=jnOa3cbK}Before I reveal that information to you, I need to know more about your intentions. One could use the banner to restore the empire, but one could also use the banner to destroy it.[ib:closed][if:convo_angry_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=DuUhVWaV}Let me tell you about myself... I was a confidant of the old emperor Neretzes. Officially I was not his spymaster, as I am a woman, but that was the role I played nonetheless. I liked Neretzes, and was very grateful for his trust, but he was not a good emperor. Too stubborn and principled. I probably should have poisoned him.[ib:demure2][if:convo_empathic_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=bWdsH2Ls}This is what I learned from a lifetime in politics: There is nothing worse than disorder. Suffice to say that I know better than anyone about the lies and cruelty that kept the Empire alive. But all the murders I ever committed in 10 years of serving Neretzes do not amount to the death toll in a single hour when an army storms a town.[if:convo_snide_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=GDNXavAl}There's nothing special about our Empire. [if:convo_calm_friendly]Any one of these petty kings and khans and sultans could probably get lucky and conquer Calradia and do as good a job ruling it as we did. But the point is - we already did it. Our greatest crimes are in the past. Let us not undo what has already been done.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=KXj8bsao}So... If you intend to use the banner to save the Empire, I'll tell you what I know. But if you want to go backward, not forward, then I will not help you.[ib:closed][if:convo_undecided_closed]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(ActivateAssembleTheBannerQuest))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=IavuL9KI}Of course. I intend to use the banner to help save the Empire.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=JRQ7qRO6}Good. Then I will tell you what I know. I heard about one other piece.[ib:normal2][if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=4WZ9zJbF}I do not know where the other pieces are, you may need to keep searching for them.[if:convo_confused_normal]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=kIDbW8fP}When you have recovered all pieces, return to me and I'll help you put them to use.[if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += ((QuestBase)this).CompleteQuestWithSuccess;
			})
			.CloseDialog()
			.PlayerOption(new TextObject("{=EitTbGvB}I am not sure. I haven't made up my mind about this.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_metImperialMentor = true;
				Hero.OneToOneConversationHero.SetHasMet();
			})
			.NpcLine(new TextObject("{=TH6L7OXu}Then you can come back when you have made up your mind.[ib:demure][if:convo_snide_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("lord_start", 110).NpcLine(new TextObject("{=oaSTbNwz}So have you made up your mind now?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == StoryModeHeroes.ImperialMentor && _metImperialMentor))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=Lwdkj0hG}Yes, I intend to use the banner to help save the Empire.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=JRQ7qRO6}Good. Then I will tell you what I know. I heard about one other piece.[ib:normal2][if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=ijyROgb4}I do not know where the other pieces are, you may need to keep searching for them.[if:convo_thinking]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=kIDbW8fP}When you have recovered all pieces, return to me and I'll help you put them to use.[if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += ((QuestBase)this).CompleteQuestWithSuccess;
			})
			.CloseDialog()
			.PlayerOption(new TextObject("{=ibm9EEPa}No, I need more time to decide.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=PknruSY5}Then you can come back when you have made up your mind.[ib:demure][if:convo_nonchalant]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
	}

	private void ActivateAssembleTheBannerQuest()
	{
		if (!((IEnumerable<QuestBase>)Campaign.Current.QuestManager.Quests).Any((QuestBase q) => q is AssembleTheBannerQuest))
		{
			((QuestBase)new AssembleTheBannerQuest()).StartQuest();
		}
	}

	protected override void OnCompleteWithSuccess()
	{
		((QuestBase)this).OnCompleteWithSuccess();
		((QuestBase)this).AddLog(_endQuestLog, false);
	}

	internal static void AutoGeneratedStaticCollectObjectsMeetWithIstianaQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(MeetWithIstianaQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	internal static object AutoGeneratedGetMemberValue_metImperialMentor(object o)
	{
		return ((MeetWithIstianaQuest)o)._metImperialMentor;
	}
}
