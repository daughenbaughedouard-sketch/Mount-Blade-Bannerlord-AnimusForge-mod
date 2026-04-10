using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Helpers;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.FirstPhase;

public class SupportKingdomQuest : StoryModeQuestBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConditionDelegate _003C_003E9__19_0;

		public static OnConditionDelegate _003C_003E9__20_0;

		internal bool _003CGetImperialKingDialogueFlow_003Eb__19_0()
		{
			if (Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Kingdom != null && Hero.OneToOneConversationHero.Clan.Kingdom.Leader == Hero.OneToOneConversationHero)
			{
				return StoryModeData.IsKingdomImperial(Hero.OneToOneConversationHero.Clan.Kingdom);
			}
			return false;
		}

		internal bool _003CGetAntiImperialKingDialogueFlow_003Eb__20_0()
		{
			if (Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Kingdom != null && Hero.OneToOneConversationHero.Clan.Kingdom.Leader == Hero.OneToOneConversationHero)
			{
				return !StoryModeData.IsKingdomImperial(Hero.OneToOneConversationHero.Clan.Kingdom);
			}
			return false;
		}
	}

	[SaveableField(1)]
	private bool _isImperial;

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			TextObject val = new TextObject("{=XtC0hXhr}Support {?IS_IMPERIAL}an Imperial Faction{?}a Non-Imperial Kingdom{\\?}", (Dictionary<string, object>)null);
			val.SetTextVariable("IS_IMPERIAL", _isImperial ? 1 : 0);
			return val;
		}
	}

	private TextObject _onQuestStartedImperialLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=TZZX9kWf}{MENTOR.LINK} suggested that you should support an imperial faction by offering them the Dragon Banner.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _onQuestStartedAntiImperialLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=4d5SP6B6}{MENTOR.LINK} suggested that you should support an anti-imperial kingdom by offering them the Dragon Banner.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _onImperialKingdomSupportedLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=atUTLABh}You have chosen to support the {KINGDOM} by presenting them the Dragon Banner, taking the advice of {MENTOR.LINK}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			val.SetTextVariable("KINGDOM", Clan.PlayerClan.Kingdom.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _onAntiImperialKingdomSupportedLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=atUTLABh}You have chosen to support the {KINGDOM} by presenting them the Dragon Banner, taking the advice of {MENTOR.LINK}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			val.SetTextVariable("KINGDOM", Clan.PlayerClan.Kingdom.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _onPlayerRuledKingdomSupportedLogText => new TextObject("{=kqj1Wp0f}You have decided to keep the Dragon Banner within the kingdom you are ruling.", (Dictionary<string, object>)null);

	private TextObject _questFailedLogText => new TextObject("{=tVlZTOst}You have chosen a different path.", (Dictionary<string, object>)null);

	public override bool IsRemainingTimeHidden => false;

	public SupportKingdomQuest(Hero questGiver)
		: base("main_storyline_support_kingdom_quest_" + ((StoryModeHeroes.ImperialMentor == questGiver) ? "1" : "0"), questGiver, StoryModeManager.Current.MainStoryLine.FirstPhase.FirstPhaseEndTime)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_isImperial = StoryModeHeroes.ImperialMentor == questGiver;
		((QuestBase)this).SetDialogs();
		if (_isImperial)
		{
			((QuestBase)this).AddLog(_onQuestStartedImperialLogText, false);
			Campaign.Current.ConversationManager.AddDialogFlow(GetImperialKingDialogueFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetImperialMentorDialogueFlow(), (object)this);
		}
		else
		{
			((QuestBase)this).AddLog(_onQuestStartedAntiImperialLogText, false);
			Campaign.Current.ConversationManager.AddDialogFlow(GetAntiImperialKingDialogueFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetAntiImperialMentorDialogueFlow(), (object)this);
		}
		((QuestBase)this).InitializeQuestOnCreation();
	}

	protected override void SetDialogs()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		((QuestBase)this).DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("{=9tpTkKdY}Tell me which path you choose when you've made progress.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver))
			.CloseDialog();
	}

	private DialogFlow GetImperialKingDialogueFlow()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		DialogFlow obj = DialogFlow.CreateDialogFlow("hero_main_options", 300).BeginPlayerOptions((string)null, false).PlayerSpecialOption(new TextObject("{=Ke7f4XSC}I present you with the Dragon Banner of Calradios.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(CheckConditionToSupportKingdom));
		object obj2 = _003C_003Ec._003C_003E9__19_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = () => Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Kingdom != null && Hero.OneToOneConversationHero.Clan.Kingdom.Leader == Hero.OneToOneConversationHero && StoryModeData.IsKingdomImperial(Hero.OneToOneConversationHero.Clan.Kingdom);
			_003C_003Ec._003C_003E9__19_0 = val;
			obj2 = (object)val;
		}
		return obj.Condition((OnConditionDelegate)obj2).NpcLine("{=PQgzfHLk}Well now. I had heard rumors that you had obtained this great artifact.[if:convo_nonchalant]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).NpcLine("{=ULn7iWlz}It will be a powerful tool in our hands. People will believe that the Heavens intend us to restore the Empire of Calradia.[if:convo_pondering]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=S1yCTPrL}This is one of the most valuable services anyone has ever done for me. I am very grateful.[if:convo_grateful]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_005b: Expected O, but got Unknown
				OnKingdomSupported(Hero.OneToOneConversationHero.Clan.Kingdom, isImperial: true);
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.LeaveEncounter = true;
				}
				TextObject val2 = new TextObject("{=IL4FcHXv}You've pledged your allegiance to the {KINGDOM_NAME}!", (Dictionary<string, object>)null);
				val2.SetTextVariable("KINGDOM_NAME", Hero.OneToOneConversationHero.Clan.Kingdom.Name);
				MBInformationManager.AddQuickInformation(val2, 0, (BasicCharacterObject)null, (Equipment)null, "");
			})
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog();
	}

	private DialogFlow GetAntiImperialKingDialogueFlow()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		DialogFlow obj = DialogFlow.CreateDialogFlow("hero_main_options", 300).BeginPlayerOptions((string)null, false).PlayerSpecialOption(new TextObject("{=Ke7f4XSC}I present you with the Dragon Banner of Calradios.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(CheckConditionToSupportKingdom));
		object obj2 = _003C_003Ec._003C_003E9__20_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = () => Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Kingdom != null && Hero.OneToOneConversationHero.Clan.Kingdom.Leader == Hero.OneToOneConversationHero && !StoryModeData.IsKingdomImperial(Hero.OneToOneConversationHero.Clan.Kingdom);
			_003C_003Ec._003C_003E9__20_0 = val;
			obj2 = (object)val;
		}
		return obj.Condition((OnConditionDelegate)obj2).NpcLine("{=PQgzfHLk}Well now. I had heard rumors that you had obtained this great artifact.[if:convo_nonchalant]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).NpcLine("{=4olAbDTq}It will be a powerful tool in our hands. People will believe that the Heavens have transferred dominion over Calradia from the Empire to us.[if:convo_pondering]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=S1yCTPrL}This is one of the most valuable services anyone has ever done for me. I am very grateful.[if:convo_grateful]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_005b: Expected O, but got Unknown
				OnKingdomSupported(Hero.OneToOneConversationHero.Clan.Kingdom, isImperial: false);
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.LeaveEncounter = true;
				}
				TextObject val2 = new TextObject("{=IL4FcHXv}You've pledged your allegiance to the {KINGDOM_NAME}!", (Dictionary<string, object>)null);
				val2.SetTextVariable("KINGDOM_NAME", Hero.OneToOneConversationHero.Clan.Kingdom.Name);
				MBInformationManager.AddQuickInformation(val2, 0, (BasicCharacterObject)null, (Equipment)null, "");
			})
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog();
	}

	private DialogFlow GetImperialMentorDialogueFlow()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		return DialogFlow.CreateDialogFlow("hero_main_options", 300).BeginPlayerOptions((string)null, false).PlayerSpecialOption(new TextObject("{=O2BAcMNO}As the legitimate {?PLAYER.GENDER}Empress{?}Emperor{\\?} of Calradia, I am ready to declare my ownership of the Dragon Banner.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => ((QuestBase)this).IsOngoing && Hero.OneToOneConversationHero == StoryModeHeroes.ImperialMentor))
			.NpcLine("{=ATduKfHu}This will make a great impression. It will attract allies, but also probably make you new enemies. Are you sure you're ready?[if:convo_undecided_closed]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=n8pmVHNn}Yes, I am ready.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(CheckPlayerCanDeclareBannerOwnershipClickableCondition))
			.NpcLine("{=gL241Hoz}Very nice. Superstitious twaddle, of course, but people will believe you. Very well, oh heir to Calradios, go forth![if:convo_nonchalant]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				OnKingdomSupported(Clan.PlayerClan.Kingdom, isImperial: true);
			})
			.CloseDialog()
			.PlayerOption("{=fRMIoPUK}Give me more time.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=KH07mJ5k}Very well, come back when you are ready.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.EndPlayerOptions()
			.CloseDialog()
			.PlayerOption("{=eYXLYgsC}I still am not sure what I will do with it.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => ((QuestBase)this).IsOngoing && Hero.OneToOneConversationHero == StoryModeHeroes.ImperialMentor))
			.NpcLine("{=UCoOMWaj}As I said before, there's a case for all of the claimants. When this war began, I thought Rhagaea understood best how to rule, Garios was the strongest warrior, and Lucon had the firmest grasp of our traditions.[if:convo_empathic_voice]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=uFsMzAuR}Speak to whichever one you choose, or come back to me if you wish to claim the banner for yourself.[if:convo_normal]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions()
			.NpcLine("{=Z54ZrDG9}Until next time, then.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog();
	}

	private DialogFlow GetAntiImperialMentorDialogueFlow()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		return DialogFlow.CreateDialogFlow("hero_main_options", 300).BeginPlayerOptions((string)null, false).PlayerSpecialOption(new TextObject("{=N5jJtZyr}As the Empire's nemesis, I am ready to declare my ownership of the Dragon Banner.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => ((QuestBase)this).IsOngoing && Hero.OneToOneConversationHero == StoryModeHeroes.AntiImperialMentor))
			.NpcLine("{=BXMKgTXl}This will make a great impression. It will attract allies, but also probably make you new enemies. Are you sure you're ready?[if:convo_astonished]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=ALWqXMiP}Yes, I am sure.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(CheckPlayerCanDeclareBannerOwnershipClickableCondition))
			.NpcLine("{=exoZygYL}Very well. The Dragon Banner in your hands proclaims you the avenger of the Empire's crimes and its successor. Now go forth and claim your destiny![if:convo_calm_friendly]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				OnKingdomSupported(Clan.PlayerClan.Kingdom, isImperial: false);
			})
			.CloseDialog()
			.PlayerOption("{=fRMIoPUK}Give me more time.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=YgoxFJSz}Very well, come back when you are ready.[if:convo_nonchalant]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.EndPlayerOptions()
			.CloseDialog()
			.PlayerOption("{=tzsZTcWd}I wonder which kingdom should I support..", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => ((QuestBase)this).IsOngoing && Hero.OneToOneConversationHero == StoryModeHeroes.AntiImperialMentor))
			.NpcLine("{=1v6aYpDx}You must choose, but choose wisely. Or you can claim it yourself. I have no preference.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("hero_main_options")
			.EndPlayerOptions()
			.NpcLine("{=Z54ZrDG9}Until next time, then.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog();
	}

	private bool IsPlayerTheRulerOfAKingdom()
	{
		int num;
		if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader == Hero.MainHero)
		{
			num = ((StoryModeData.IsKingdomImperial(Clan.PlayerClan.Kingdom) == _isImperial) ? 1 : 0);
			if (num != 0)
			{
				MBTextManager.SetTextVariable("FACTION", Clan.PlayerClan.Kingdom.Name, false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private bool CheckPlayerCanDeclareBannerOwnershipClickableCondition(out TextObject explanation)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		if (IsPlayerTheRulerOfAKingdom())
		{
			explanation = null;
			return true;
		}
		explanation = (_isImperial ? new TextObject("{=mziMNKm2}You should be ruling a kingdom of the imperial culture.", (Dictionary<string, object>)null) : new TextObject("{=HCA9xOOo}You should be ruling a kingdom of non-imperial culture.", (Dictionary<string, object>)null));
		return false;
	}

	private bool CheckConditionToSupportKingdom(out TextObject explanation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		explanation = new TextObject("{=qNR8WKcX}You should join a kingdom before supporting it with the Dragon Banner.", (Dictionary<string, object>)null);
		if (Clan.PlayerClan.Kingdom == null || Clan.PlayerClan.Kingdom != Hero.OneToOneConversationHero.Clan.Kingdom)
		{
			return false;
		}
		return true;
	}

	private void OnKingdomSupported(Kingdom kingdom, bool isImperial)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		if (isImperial)
		{
			if (kingdom.RulingClan == Clan.PlayerClan)
			{
				((QuestBase)this).AddLog(_onPlayerRuledKingdomSupportedLogText, false);
				StoryModeManager.Current.MainStoryLine.SetStoryLineSide(MainStoryLineSide.CreateImperialKingdom);
				MBInformationManager.ShowSceneNotification((SceneNotificationData)new DeclareDragonBannerSceneNotificationItem(true));
			}
			else
			{
				((QuestBase)this).AddLog(_onImperialKingdomSupportedLogText, false);
				StoryModeManager.Current.MainStoryLine.SetStoryLineSide(MainStoryLineSide.SupportImperialKingdom);
				MBInformationManager.ShowSceneNotification((SceneNotificationData)new PledgeAllegianceSceneNotificationItem(Hero.MainHero, true));
			}
		}
		else if (kingdom.RulingClan == Clan.PlayerClan)
		{
			((QuestBase)this).AddLog(_onPlayerRuledKingdomSupportedLogText, false);
			StoryModeManager.Current.MainStoryLine.SetStoryLineSide(MainStoryLineSide.CreateAntiImperialKingdom);
			MBInformationManager.ShowSceneNotification((SceneNotificationData)new DeclareDragonBannerSceneNotificationItem(false));
		}
		else
		{
			((QuestBase)this).AddLog(_onAntiImperialKingdomSupportedLogText, false);
			StoryModeManager.Current.MainStoryLine.SetStoryLineSide(MainStoryLineSide.SupportAntiImperialKingdom);
			MBInformationManager.ShowSceneNotification((SceneNotificationData)new PledgeAllegianceSceneNotificationItem(Hero.MainHero, false));
		}
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private void MainStoryLineChosen(MainStoryLineSide chosenSide)
	{
		if ((_isImperial && chosenSide != MainStoryLineSide.SupportImperialKingdom && chosenSide != MainStoryLineSide.CreateImperialKingdom) || (!_isImperial && chosenSide != MainStoryLineSide.SupportAntiImperialKingdom && chosenSide != MainStoryLineSide.CreateAntiImperialKingdom))
		{
			((QuestBase)this).CompleteQuestWithCancel(_questFailedLogText);
		}
	}

	protected override void HourlyTick()
	{
	}

	protected override void RegisterEvents()
	{
		StoryModeEvents.OnMainStoryLineSideChosenEvent.AddNonSerializedListener((object)this, (Action<MainStoryLineSide>)MainStoryLineChosen);
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
		if (_isImperial)
		{
			Campaign.Current.ConversationManager.AddDialogFlow(GetImperialKingDialogueFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetImperialMentorDialogueFlow(), (object)this);
		}
		else
		{
			Campaign.Current.ConversationManager.AddDialogFlow(GetAntiImperialKingDialogueFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetAntiImperialMentorDialogueFlow(), (object)this);
		}
	}

	internal static void AutoGeneratedStaticCollectObjectsSupportKingdomQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(SupportKingdomQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	internal static object AutoGeneratedGetMemberValue_isImperial(object o)
	{
		return ((SupportKingdomQuest)o)._isImperial;
	}
}
