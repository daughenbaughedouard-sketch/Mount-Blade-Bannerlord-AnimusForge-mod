using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.FirstPhase;

public class AssembleTheBannerQuest : StoryModeQuestBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConsequenceDelegate _003C_003E9__33_0;

		public static Func<QuestBase, bool> _003C_003E9__34_0;

		public static Func<QuestBase, bool> _003C_003E9__34_1;

		internal void _003CSetDialogs_003Eb__33_0()
		{
			if (PlayerEncounter.Current != null)
			{
				PlayerEncounter.LeaveEncounter = true;
			}
		}

		internal bool _003CAssembleBannerConditionDialogCondition_003Eb__34_0(QuestBase q)
		{
			if (!q.IsFinalized)
			{
				return q is MeetWithIstianaQuest;
			}
			return false;
		}

		internal bool _003CAssembleBannerConditionDialogCondition_003Eb__34_1(QuestBase q)
		{
			if (!q.IsFinalized)
			{
				return q is MeetWithArzagosQuest;
			}
			return false;
		}
	}

	[SaveableField(1)]
	private JournalLog _startLog;

	[SaveableField(2)]
	private bool _talkedWithImperialMentor;

	[SaveableField(3)]
	private bool _talkedWithAntiImperialMentor;

	private TextObject _startQuestLog => new TextObject("{=OS8YjyE5}You should collect all of the pieces of the Dragon Banner before deciding your path.", (Dictionary<string, object>)null);

	private TextObject _allPiecesCollectedQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=eV8R0SKp}Now you can decide what to do with the {DRAGON_BANNER}.", (Dictionary<string, object>)null);
			val.SetTextVariable("DRAGON_BANNER", StoryModeManager.Current.MainStoryLine.DragonBanner.Name);
			StringHelpers.SetCharacterProperties("IMPERIAL_MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			StringHelpers.SetCharacterProperties("ANTI_IMPERIAL_MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _talkedWithImperialMentorButNotWithAntiImperialMentorQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=yNcBDr9j}You talked with {IMPERIAL_MENTOR.LINK}. Now, you may want to talk with {ANTI_IMPERIAL_MENTOR.LINK} and take {?ANTI_IMPERIAL_MENTOR.GENDER}her{?}his{\\?} opinions too. {?ANTI_IMPERIAL_MENTOR.GENDER}She{?}He{\\?} is currently in {SETTLEMENT_LINK}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("IMPERIAL_MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			StringHelpers.SetCharacterProperties("ANTI_IMPERIAL_MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			val.SetTextVariable("SETTLEMENT_LINK", StoryModeHeroes.AntiImperialMentor.CurrentSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _talkedWithImperialMentorQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=RwlDeE9t}You talked with {IMPERIAL_MENTOR.LINK} too. Now you should make a decision.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("IMPERIAL_MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _talkedWithAntiImperialMentorButNotWithImperialMentorQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=yub8ZSFP}You talked with {ANTI_IMPERIAL_MENTOR.LINK}. Now, you may want to talk with {IMPERIAL_MENTOR.LINK} and take {?IMPERIAL_MENTOR.GENDER}her{?}his{\\?} opinions too. {?IMPERIAL_MENTOR.GENDER}She{?}He{\\?} is currently in {SETTLEMENT_LINK}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("ANTI_IMPERIAL_MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			StringHelpers.SetCharacterProperties("IMPERIAL_MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			val.SetTextVariable("SETTLEMENT_LINK", StoryModeHeroes.ImperialMentor.CurrentSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _talkedWithAntiImperialMentorQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=rfkKxdxp}You talked with {ANTI_IMPERIAL_MENTOR.LINK} too. Now you should make a decision.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("ANTI_IMPERIAL_MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _endQuestLog => new TextObject("{=eNJBjYG8}You successfully assembled the Dragon Banner of Calradios.", (Dictionary<string, object>)null);

	public override TextObject Title => new TextObject("{=y84UnOQX}Assemble the Dragon Banner", (Dictionary<string, object>)null);

	public override bool IsRemainingTimeHidden => false;

	public AssembleTheBannerQuest()
		: base("assemble_the_banner_story_mode_quest", null, StoryModeManager.Current.MainStoryLine.FirstPhase.FirstPhaseEndTime)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_talkedWithImperialMentor = false;
		_talkedWithAntiImperialMentor = false;
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
		StoryModeEvents.OnBannerPieceCollectedEvent.AddNonSerializedListener((object)this, (Action)OnBannerPieceCollected);
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
	}

	protected override void OnStartQuest()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		((QuestBase)this).SetDialogs();
		_startLog = ((QuestBase)this).AddDiscreteLog(_startQuestLog, new TextObject("{=xL3WGYsw}Collected Pieces", (Dictionary<string, object>)null), StoryMode.StoryModePhases.FirstPhase.Instance.CollectedBannerPieceCount, 3, (TextObject)null, false);
	}

	protected override void OnCompleteWithSuccess()
	{
		((QuestBase)this).AddLog(_endQuestLog, false);
	}

	private void OnBannerPieceCollected()
	{
		_startLog.UpdateCurrentProgress(StoryMode.StoryModePhases.FirstPhase.Instance.CollectedBannerPieceCount);
		if (StoryMode.StoryModePhases.FirstPhase.Instance.AllPiecesCollected)
		{
			((QuestBase)this).AddLog(_allPiecesCollectedQuestLog, false);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.ImperialMentor.CurrentSettlement);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.AntiImperialMentor.CurrentSettlement);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.ImperialMentor);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.AntiImperialMentor);
			StoryModeManager.Current.MainStoryLine.FirstPhase?.MergeDragonBanner();
		}
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		if (quest is CreateKingdomQuest || quest is SupportKingdomQuest)
		{
			if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)StoryModeHeroes.AntiImperialMentor.CurrentSettlement))
			{
				((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.AntiImperialMentor.CurrentSettlement);
			}
			if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)StoryModeHeroes.ImperialMentor.CurrentSettlement))
			{
				((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.ImperialMentor.CurrentSettlement);
			}
			if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)StoryModeHeroes.AntiImperialMentor))
			{
				((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.AntiImperialMentor);
			}
			if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)StoryModeHeroes.ImperialMentor))
			{
				((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.ImperialMentor);
			}
			((QuestBase)this).CompleteQuestWithSuccess();
		}
	}

	public override void OnFailed()
	{
		((QuestBase)this).OnFailed();
		RemoveRemainingBannerPieces();
	}

	public override void OnCanceled()
	{
		((QuestBase)this).OnCanceled();
		RemoveRemainingBannerPieces();
	}

	protected override void OnTimedOut()
	{
		base.OnTimedOut();
		RemoveRemainingBannerPieces();
	}

	private void RemoveRemainingBannerPieces()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		ItemObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<ItemObject>("dragon_banner_center");
		ItemObject val2 = ((GameType)Campaign.Current).ObjectManager.GetObject<ItemObject>("dragon_banner_dragonhead");
		ItemObject val3 = ((GameType)Campaign.Current).ObjectManager.GetObject<ItemObject>("dragon_banner_handle");
		foreach (ItemRosterElement item in MobileParty.MainParty.ItemRoster)
		{
			ItemRosterElement current = item;
			EquipmentElement equipmentElement = ((ItemRosterElement)(ref current)).EquipmentElement;
			if (((EquipmentElement)(ref equipmentElement)).Item != val)
			{
				equipmentElement = ((ItemRosterElement)(ref current)).EquipmentElement;
				if (((EquipmentElement)(ref equipmentElement)).Item != val2)
				{
					equipmentElement = ((ItemRosterElement)(ref current)).EquipmentElement;
					if (((EquipmentElement)(ref equipmentElement)).Item != val3)
					{
						continue;
					}
				}
			}
			MobileParty.MainParty.ItemRoster.Remove(current);
		}
	}

	protected override void SetDialogs()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(GetImperialMentorEndQuestDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(GetAntiImperialMentorEndQuestDialog(), (object)this);
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("lord_start", 150).NpcLine(new TextObject("{=AHDQffXv}Have you assembled the banner?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(AssembleBannerConditionDialogCondition))
			.PlayerLine(new TextObject("{=2h7IlBmv}Not yet, I'm working on it...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__33_0;
		if (obj2 == null)
		{
			OnConsequenceDelegate val = delegate
			{
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.LeaveEncounter = true;
				}
			};
			_003C_003Ec._003C_003E9__33_0 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Consequence((OnConsequenceDelegate)obj2).CloseDialog(), (object)this);
	}

	private bool AssembleBannerConditionDialogCondition()
	{
		if ((Hero.OneToOneConversationHero == StoryModeHeroes.ImperialMentor || Hero.OneToOneConversationHero == StoryModeHeroes.AntiImperialMentor) && !StoryMode.StoryModePhases.FirstPhase.Instance.AllPiecesCollected)
		{
			if ((Hero.OneToOneConversationHero == StoryModeHeroes.ImperialMentor && ((IEnumerable<QuestBase>)Campaign.Current.QuestManager.Quests).Any((QuestBase q) => !q.IsFinalized && q is MeetWithIstianaQuest)) || (Hero.OneToOneConversationHero == StoryModeHeroes.AntiImperialMentor && ((IEnumerable<QuestBase>)Campaign.Current.QuestManager.Quests).Any((QuestBase q) => !q.IsFinalized && q is MeetWithArzagosQuest)))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private DialogFlow GetAntiImperialMentorEndQuestDialog()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		string text = default(string);
		return DialogFlow.CreateDialogFlow("hero_main_options", 150).BeginPlayerOptions((string)null, false).PlayerSpecialOption(new TextObject("{=r8ZLabb0}I have gathered all pieces of the Dragon Banner. What now?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == StoryModeHeroes.AntiImperialMentor && StoryMode.StoryModePhases.FirstPhase.Instance.AllPiecesCollected && !_talkedWithAntiImperialMentor))
			.NpcLine(new TextObject("{=5j6qvGAF}Excellent work! When you unfurl this banner, and men see what they thought was lost, it will make a powerful impression.[ib:normal2][if:convo_astonished]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(GetAntiImperialQuests))
			.NpcLine(new TextObject("{=MOVWOyeh}Clearly you have been chosen by Heaven for a great purpose. I see the makings of a new legend here... Allow me to call you 'Bannerlord.'[ib:normal][if:convo_relaxed_happy]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=o791xRtb}Right then, to the business of bringing down this cursed Empire. As I see it, you have two options...[ib:confident2][if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GetOutputToken(ref text)
			.NpcLine(new TextObject("{=c6pDNXbb}You can create your own kingdom or support an existing one...[if:convo_normal]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=0pilmavQ}How can I create my own kingdom?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=frk7T3ue}It will not be easy, but I can explain in detail...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=yCzcfKNM}Firstly, your clan must be independent. You cannot be pledged to an existing realm.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=tJQ5oajd}Next, your clan must have won for itself considerable renown, or no one will follow you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=MJd5agS2}I would recommend that you gather a fairly large army, as you may soon be at war with more powerful and established realms.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=6YhGGJ7a}Finally, you need a capital for your realm. It can be any settlement you own, so long as they do not speak the imperial tongue. I will not help you create another Empire.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=fprOWs1E}Now, when you are ready to declare your new kingdom, instruct the governor of your capital to have a proclamation read out throughout your lands.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=Q2obAF4E}So! You have much to do. I will await news of your success. Return to me when you wish to declare your ownership of the banner to the world.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption(new TextObject("{=mtiaY2Pa}How can I support an existing kingdom?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=oKknZdXn}You should join the kingdom that you wish to support by talking to the leader. None will bring back the Palaic people, but the final victory of any one of those would be suitable vengeance.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=dPb2Vph3}My informants will tell me once you pledged your support...[ib:normal2][if:convo_nonchalant]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption(new TextObject("{=6LQUuQhV}Thank you for your precious help.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog();
	}

	private void GetAntiImperialQuests()
	{
		_talkedWithAntiImperialMentor = true;
		if (!_talkedWithImperialMentor)
		{
			((QuestBase)this).AddLog(_talkedWithAntiImperialMentorButNotWithImperialMentorQuestLog, false);
		}
		else
		{
			((QuestBase)this).AddLog(_talkedWithAntiImperialMentorQuestLog, false);
		}
		if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)StoryModeHeroes.AntiImperialMentor.CurrentSettlement))
		{
			((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.AntiImperialMentor.CurrentSettlement);
		}
		((QuestBase)new CreateKingdomQuest(StoryModeHeroes.AntiImperialMentor)).StartQuest();
		((QuestBase)new SupportKingdomQuest(StoryModeHeroes.AntiImperialMentor)).StartQuest();
	}

	private DialogFlow GetImperialMentorEndQuestDialog()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		string text = default(string);
		return DialogFlow.CreateDialogFlow("hero_main_options", 150).BeginPlayerOptions((string)null, false).PlayerSpecialOption(new TextObject("{=r8ZLabb0}I have gathered all pieces of the Dragon Banner. What now?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == StoryModeHeroes.ImperialMentor && StoryMode.StoryModePhases.FirstPhase.Instance.AllPiecesCollected && !_talkedWithImperialMentor))
			.NpcLine(new TextObject("{=UjyZ7GFk}Impressive, most impressive. Well, things will get interesting now.[ib:normal2][if:convo_astonished]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(GetImperialQuests))
			.NpcLine(new TextObject("{=9E6faNBg}I will need to embroider a proper legend about you. Divine omens at your birth, that kind of thing. For now, we can call you 'Bannerlord,' who brings down the wrath of Heaven on the impudent barbarians.[ib:confident2][if:convo_relaxed_happy]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=CnXA7oyE}Now, there are two paths that lie ahead of you, my child!", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GetOutputToken(ref text)
			.NpcLine(new TextObject("{=1GgTNRNl}You can make your own claim to the rulership of the Empire and try to win the civil war, or support an existing claimant...[if:convo_normal]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=Dgdopl1b}How can I create my own imperial kingdom?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=NdkqUnXb}To have a chance as an imperial contender, you must fullfil some conditions.[if:convo_empathic_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=yCzcfKNM}Firstly, your clan must be independent. You cannot be pledged to an existing realm.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=LLJ0oB8i}Next, your clan's renown must have spread far and wide, or no one will take you seriously.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=3XbTo6O7}Also, of course, I recommend that you have as large an army as you can gather.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=Cl4xi6Be}Finally, you need a capital. Any settlement will do, so long as the inhabitants speak the imperial language.[if:convo_focused_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=fprOWs1E}Now, when you are ready to declare your new kingdom, instruct the governor of your capital to have a proclamation read out throughout your lands.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=tkJD40hE}Well, that should keep you busy for a while. Come back when you are ready.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption(new TextObject("{=tRzjuX0E}How can I support an existing imperial claimant?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=oL9BdThD}Choose one and pledge allegiance. When this civil war began, I was a bit torn... Rhagaea was the cleverest ruler, Garios probably the best fighter, and Lucon seemed to have the best grasp of our laws and traditions. But you can make up your own mind.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=eaxOH9mb}My little birds will tell me once you pledge your support...[if:convo_nonchalant]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption(new TextObject("{=6LQUuQhV}Thank you for your precious help.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog();
	}

	private void GetImperialQuests()
	{
		_talkedWithImperialMentor = true;
		if (!_talkedWithAntiImperialMentor)
		{
			((QuestBase)this).AddLog(_talkedWithImperialMentorButNotWithAntiImperialMentorQuestLog, false);
		}
		else
		{
			((QuestBase)this).AddLog(_talkedWithImperialMentorQuestLog, false);
		}
		if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)StoryModeHeroes.ImperialMentor.CurrentSettlement))
		{
			((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)StoryModeHeroes.ImperialMentor.CurrentSettlement);
		}
		((QuestBase)new CreateKingdomQuest(StoryModeHeroes.ImperialMentor)).StartQuest();
		((QuestBase)new SupportKingdomQuest(StoryModeHeroes.ImperialMentor)).StartQuest();
	}

	internal static void AutoGeneratedStaticCollectObjectsAssembleTheBannerQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(AssembleTheBannerQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_startLog);
	}

	internal static object AutoGeneratedGetMemberValue_startLog(object o)
	{
		return ((AssembleTheBannerQuest)o)._startLog;
	}

	internal static object AutoGeneratedGetMemberValue_talkedWithImperialMentor(object o)
	{
		return ((AssembleTheBannerQuest)o)._talkedWithImperialMentor;
	}

	internal static object AutoGeneratedGetMemberValue_talkedWithAntiImperialMentor(object o)
	{
		return ((AssembleTheBannerQuest)o)._talkedWithAntiImperialMentor;
	}
}
