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

public class MeetWithArzagosQuest : StoryModeQuestBase
{
	[SaveableField(1)]
	private bool _metAntiImperialMentor;

	private TextObject _startQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=5K4wvz3w}Find and meet {HERO.LINK} to learn more about Neretzes' Banner. He is currently in {SETTLEMENT}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("HERO", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			val.SetTextVariable("SETTLEMENT", StoryModeHeroes.AntiImperialMentor.CurrentSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _endQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=qMUfOtyk}You talked with {HERO.LINK}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("HERO", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
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
			StringHelpers.SetCharacterProperties("HERO", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	public override bool IsRemainingTimeHidden => false;

	public MeetWithArzagosQuest(Settlement settlement)
		: base("meet_with_arzagos_story_mode_quest", null, StoryModeManager.Current.MainStoryLine.FirstPhase.FirstPhaseEndTime)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_metAntiImperialMentor = false;
		((QuestBase)this).SetDialogs();
		HeroHelper.SpawnHeroForTheFirstTime(StoryModeHeroes.AntiImperialMentor, settlement);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)settlement);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.AntiImperialMentor);
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
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("lord_start", 110).NpcLine(new TextObject("{=unOLk4PY}So. Who are you, and what brings you to me?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == StoryModeHeroes.AntiImperialMentor && !_metAntiImperialMentor))
			.PlayerLine(new TextObject("{=tfm5hcks}I believe I have a piece of the Dragon Banner of Calradios.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=uvbCyLiR}Is that true? Well, that is interesting.[ib:normal][if:convo_astonished]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=pOuGX9j0}You may have one piece of the banner, but it's of little use in itself. You'll have to find the other parts. But once you can bring together the pieces, you'll have something of tremendous value.[if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=t71lPdyb}How so?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=SmVwMrUM}The banner of Calradios is part of a legend. It was said to be carried by Calradios the Great, who first led his people to this land, to conquer and despoil. The legend says that no army led by a true son of Calradios shall be defeated in battle.[ib:confident2][if:convo_thinking]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=cNwejsNl}Convenient legend, eh? Of course the Calradians have been defeated many times, but I guess those were not 'true sons.' Still, you could say it represents the strength and endurance of this empire.[ib:normal][if:convo_focused_happy]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=FBp2ranI}So, can you help me find a buyer for it?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=3G64Ej64}A buyer? I can help you do far more than that.[if:convo_astonished]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=MnmblprY}So, where can I find the other pieces?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=Fgta5mF6}Before I answer, you and I need to know more about each other. I don't know what you know about me.  I was a citizen of the Empire. I was a commander in the imperial armies. But I am not imperial.[ib:confident][if:convo_thinking]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=R5kLv5kg}I am what they call Palaic. Palaic is a language that is no longer spoken, except by a few old people. Even the word 'Palaic' is imperial. We are a people who have forgotten who we are.[if:convo_focused_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=cfTiiEEM}The Empire has a genius for destruction - the destruction of languages, traditions, gods. It takes our fortresses, slaughters our men, and turns our children into its own children.[if:convo_focused_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=qoA4UPly}Nothing can bring the Palaic people back. They are now imperial. But it is an insult to our name, to our gods, to our memory, that the state which destroyed our shrines and fortresses should last and thrive.[if:convo_empathic_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=rMem50oz}I have vowed that this Empire shall not survive this civil war, if I can do anything to stop it. And believe me, if I had that banner, there is very much something I could do.[if:convo_angry_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(ActivateAssembleTheBannerQuest))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=tkXKef0Z}I too would see the empire destroyed.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=4RaspRbe}Good. Then I will tell you what I know. I heard about one other piece.[if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=4WZ9zJbF}I do not know where the other pieces are, you may need to keep searching for them.[if:convo_confused_normal]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=kIDbW8fP}When you have recovered all pieces, return to me and I'll help you put them to use.[if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += ((QuestBase)this).CompleteQuestWithSuccess;
			})
			.CloseDialog()
			.PlayerOption(new TextObject("{=gdgbaMOP}I am not sure I share your views.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_metAntiImperialMentor = true;
				Hero.OneToOneConversationHero.SetHasMet();
			})
			.NpcLine(new TextObject("{=7ULaG8aT}Then you can come back when you made your mind up.[ib:closed][if:convo_insulted]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("lord_start", 110).NpcLine(new TextObject("{=bHveKDUI}So have you made up your mind now?[ib:closed][if:convo_nonchalant]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == StoryModeHeroes.AntiImperialMentor && _metAntiImperialMentor))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=upyNhwZ9}Yes, I intend to use the banner to help destroy the empire.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=TEgoba7R}Good. Then I will tell you what I know. I heard about one other piece.[ib:confident2][if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=ijyROgb4}I do not know where the other pieces are, you may need to keep searching for them.[if:convo_thinking]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=kIDbW8fP}When you have recovered all pieces, return to me and I'll help you put them to use.[if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += ((QuestBase)this).CompleteQuestWithSuccess;
			})
			.CloseDialog()
			.PlayerOption(new TextObject("{=ibm9EEPa}No, I need more time to decide.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=7ULaG8aT}Then you can come back when you made your mind up.[ib:closed][if:convo_insulted]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
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

	internal static void AutoGeneratedStaticCollectObjectsMeetWithArzagosQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(MeetWithArzagosQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	internal static object AutoGeneratedGetMemberValue_metAntiImperialMentor(object o)
	{
		return ((MeetWithArzagosQuest)o)._metAntiImperialMentor;
	}
}
