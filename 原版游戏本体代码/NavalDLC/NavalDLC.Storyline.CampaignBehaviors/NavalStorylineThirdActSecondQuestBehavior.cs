using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Storyline.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Storyline.CampaignBehaviors;

public class NavalStorylineThirdActSecondQuestBehavior : CampaignBehaviorBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConsequenceDelegate _003C_003E9__7_1;

		public static OnConsequenceDelegate _003C_003E9__7_4;

		internal void _003CAddDialogs_003Eb__7_1()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}

		internal void _003CAddDialogs_003Eb__7_4()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}
	}

	private const string _questConversationMenuId = "naval_storyline_act_3_quest_2_conversation_menu";

	private bool _isQuestAcceptedThroughMission;

	public override void RegisterEvents()
	{
		if (!NavalStorylineData.IsNavalStorylineCanceled())
		{
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
			NavalDLCEvents.OnNavalStorylineCanceledEvent.AddNonSerializedListener((object)this, (Action<NavalStorylineData.StorylineCancelDetail>)OnNavalStorylineCanceled);
		}
	}

	private void OnNavalStorylineCanceled(NavalStorylineData.StorylineCancelDetail detail)
	{
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	private void OnSessionLaunched(CampaignGameStarter starter)
	{
		AddGameMenus(starter);
		AddDialogs(starter);
	}

	private void AddGameMenus(CampaignGameStarter starter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		starter.AddGameMenu("naval_storyline_act_3_quest_2_conversation_menu", string.Empty, new OnInitDelegate(naval_storyline_act_3_quest_2_conversation_menu_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
	}

	private void naval_storyline_act_3_quest_2_conversation_menu_on_init(MenuCallbackArgs args)
	{
		if (_isQuestAcceptedThroughMission && Mission.Current == null)
		{
			StartQuest();
			_isQuestAcceptedThroughMission = false;
		}
	}

	private void AddDialogs(CampaignGameStarter starter)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_0184: Expected O, but got Unknown
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Expected O, but got Unknown
		//IL_01b5: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Expected O, but got Unknown
		//IL_01d5: Expected O, but got Unknown
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Expected O, but got Unknown
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Expected O, but got Unknown
		//IL_0219: Expected O, but got Unknown
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Expected O, but got Unknown
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_0256: Expected O, but got Unknown
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Expected O, but got Unknown
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Expected O, but got Unknown
		//IL_029f: Expected O, but got Unknown
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Expected O, but got Unknown
		//IL_02c0: Expected O, but got Unknown
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Expected O, but got Unknown
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Expected O, but got Unknown
		//IL_030a: Expected O, but got Unknown
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Expected O, but got Unknown
		//IL_032b: Expected O, but got Unknown
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Expected O, but got Unknown
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Expected O, but got Unknown
		//IL_0375: Expected O, but got Unknown
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Expected O, but got Unknown
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Expected O, but got Unknown
		//IL_03b2: Expected O, but got Unknown
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Expected O, but got Unknown
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Expected O, but got Unknown
		//IL_03fc: Expected O, but got Unknown
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Expected O, but got Unknown
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Expected O, but got Unknown
		//IL_0446: Expected O, but got Unknown
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Expected O, but got Unknown
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Expected O, but got Unknown
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Expected O, but got Unknown
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Expected O, but got Unknown
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Expected O, but got Unknown
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Expected O, but got Unknown
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0544: Expected O, but got Unknown
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Expected O, but got Unknown
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Expected O, but got Unknown
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Expected O, but got Unknown
		//IL_05d5: Expected O, but got Unknown
		//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Expected O, but got Unknown
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Expected O, but got Unknown
		TextObject val = new TextObject("{=TlgUi5Sh}{PLAYER.NAME}... Word spreads fast among sailors. We seem to have made a bit of a name for ourselves with that victory off of Hvalvik. I have someone for you to meet.", (Dictionary<string, object>)null);
		TextObjectExtensions.SetCharacterProperties(val, "PLAYER", CharacterObject.PlayerCharacter, false);
		TextObject val2 = new TextObject("{=AGY68GQE}So… You are the captain who thrashed those so-called Sea Hounds up north. I have a proposal that I hope would be of interest.", (Dictionary<string, object>)null);
		TextObject val3 = new TextObject("{=pUZTxrEy}I am Lahar, of Quyaz, on the Jade Sea. I am here because one of the great families of our city has been having some troubles. The head of one branch, the lady Fahda, has quarreled over her inheritance with her uncles. The elders of the town backed the uncles, so she took to the sea with her retainers and vowed to ravage their shipping.", (Dictionary<string, object>)null);
		val3.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest2TargetSettlement.EncyclopediaLinkWithName);
		TextObject val4 = new TextObject("{=MM0mXw6o}How formidable a foe is this Fahda?", (Dictionary<string, object>)null);
		TextObject val5 = new TextObject("{=x3EgmkF8}The lady is good at her craft. Fahda has been sailing since she was a child. She always wears a sailor’s cap, and underneath she is as bald as an egg. She persuaded her late father to take her to sea, so the story goes, by cutting off all of her long shining hair lest it catch in the rigging. She has taken several Quyazi ships, and I would be reluctant to fight her alone.", (Dictionary<string, object>)null);
		val5.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest2TargetSettlement.EncyclopediaLinkWithName);
		TextObject val6 = new TextObject("{=s7CSGwZ5}What does this have to do with our quarrel with the Sea Hounds?", (Dictionary<string, object>)null);
		TextObject val7 = new TextObject("{=JBOE2x1a}The lady Fahda has reportedly joined up with these Sea Hounds, as pirates often band together. She has been prowling about the Gulf of Charas, taking Quyazi vessels. You wish to continue hunting Sea Hounds, do you not? Those would be good waters in which to hunt, and if you are going there, I would like to come with you and lend my assistance.", (Dictionary<string, object>)null);
		TextObject val8 = new TextObject("{=pUZPt8Po}Fahda also traffics in captives with the Sea Hounds. She may have bought or held your sister at some point, or if not, at least she may be able to tell us more about the Sea Hounds' trade in slaves.", (Dictionary<string, object>)null);
		TextObject val9 = new TextObject("{=TUmPKK8P}Lahar - what will we gain by helping you catch her?", (Dictionary<string, object>)null);
		TextObject val10 = new TextObject("{=fbKlKR0v}If you wish to weaken these Sea Hounds, you may want to strike at their allies first. And of course the elders of Quyaz will be most happy to pay a handsome reward, of which you and Gunnar would receive your fair share.", (Dictionary<string, object>)null);
		TextObject val11 = new TextObject("{=jo3s90PF}What will you bring on our hunt?", (Dictionary<string, object>)null);
		TextObject val12 = new TextObject("{=w9ar5Ldc}I have my loyal crew and a swift liburna, outfitted with a ram, which I think you might put to good purpose. It would be especially useful if we encounter any slow but powerful ships that would be costly to take by boarding.", (Dictionary<string, object>)null);
		TextObject val13 = new TextObject("{=jSaUTBbW}I am ready to set out.", (Dictionary<string, object>)null);
		TextObject val14 = new TextObject("{=ZUAvYPpg}That sounds promising, but I am not yet ready to depart.", (Dictionary<string, object>)null);
		TextObject val15 = new TextObject("{=8T2uf1ay}Can I tell Lahar that we are ready to sail? The tide and winds are with us, and it would be a pity if someone else were to hunt down Fahda and claim the bounty.", (Dictionary<string, object>)null);
		TextObject val16 = new TextObject("{=hcm7PZLK}Order the men to their ships. We sail at once.", (Dictionary<string, object>)null);
		TextObject val17 = new TextObject("{=vxLowgvR}I am not quite ready. Let us pray that the good winds last a little longer.", (Dictionary<string, object>)null);
		TextObject val18 = new TextObject("{=OSZozYIR}Talk with Gunnar when you're ready to depart.", (Dictionary<string, object>)null);
		string text = default(string);
		string text2 = default(string);
		string text3 = default(string);
		string text4 = default(string);
		string text5 = default(string);
		string text6 = default(string);
		string text7 = default(string);
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200).GenerateToken(ref text).GenerateToken(ref text2)
			.GenerateToken(ref text3)
			.GenerateToken(ref text4)
			.GenerateToken(ref text5)
			.GenerateToken(ref text6)
			.GenerateToken(ref text7)
			.NpcLine(val, new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.Condition(new OnConditionDelegate(MultiAgentConversationCondition))
			.NpcLine(val2, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(val3, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.GotoDialogState(text)
			.BeginPlayerOptions(text, false)
			.PlayerOption(val4, new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(val5, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.GotoDialogState(text3)
			.PlayerOption(val6, new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(val7, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.GotoDialogState(text2)
			.EndPlayerOptions()
			.BeginPlayerOptions(text2, false)
			.PlayerOption(val4, new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(val5, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(val8, new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.GotoDialogState(text4)
			.EndPlayerOptions()
			.BeginPlayerOptions(text3, false)
			.PlayerOption(val6, new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(val7, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(val8, new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.GotoDialogState(text4)
			.EndPlayerOptions()
			.BeginPlayerOptions(text4, false)
			.PlayerOption(val9, new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(val10, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.GotoDialogState(text6)
			.PlayerOption(val11, new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(val12, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.GotoDialogState(text5)
			.EndPlayerOptions()
			.BeginPlayerOptions(text5, false)
			.PlayerOption(val9, new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(val10, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.GotoDialogState(text7)
			.EndPlayerOptions()
			.BeginPlayerOptions(text6, false)
			.PlayerOption(val11, new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(val12, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.GotoDialogState(text7)
			.EndPlayerOptions()
			.BeginPlayerOptions(text7, false)
			.PlayerOption(val13, new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
			})
			.CloseDialog()
			.PlayerOption(val14, new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__7_1;
		if (obj2 == null)
		{
			OnConsequenceDelegate val19 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__7_1 = val19;
			obj2 = (object)val19;
		}
		DialogFlow val20 = obj.Consequence((OnConsequenceDelegate)obj2).CloseDialog().PlayerOption("{=aEKNUI45}This war on the Sea Hounds is too risky. There must be another way to get my sister back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_ransom_sister")
			.EndPlayerOptions();
		DialogFlow obj3 = DialogFlow.CreateDialogFlow("start", 1200).NpcLine(val15, new OnMultipleConversationConsequenceDelegate(IsGunnar), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => NavalStorylineData.Lahar.HasMet && IsAct3Quest2ReadyToStart(NavalStorylineData.Gunnar)))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(val16, new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
			})
			.CloseDialog()
			.PlayerOption(val17, new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null);
		object obj4 = _003C_003Ec._003C_003E9__7_4;
		if (obj4 == null)
		{
			OnConsequenceDelegate val21 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__7_4 = val21;
			obj4 = (object)val21;
		}
		DialogFlow val22 = obj3.Consequence((OnConsequenceDelegate)obj4).CloseDialog().PlayerOption("{=aEKNUI45}This war on the Sea Hounds is too risky. There must be another way to get my sister back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_ransom_sister")
			.EndPlayerOptions();
		DialogFlow val23 = DialogFlow.CreateDialogFlow("start", 1200).NpcLine(val18, new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null).Condition((OnConditionDelegate)(() => NavalStorylineData.Lahar.HasMet && IsAct3Quest2ReadyToStart(NavalStorylineData.Lahar)))
			.CloseDialog();
		Campaign.Current.ConversationManager.AddDialogFlow(val20, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val22, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val23, (object)null);
	}

	private bool MultiAgentConversationCondition()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
		if (IsAct3Quest2ReadyToStart(NavalStorylineData.Gunnar) && Mission.Current != null && !NavalStorylineData.Lahar.HasMet)
		{
			NavalStorylineData.Lahar.SetHasMet();
			Agent val = null;
			Mission current = Mission.Current;
			Vec3 position = Agent.Main.Position;
			foreach (Agent item in (List<Agent>)(object)current.GetNearbyAgents(((Vec3)(ref position)).AsVec2, 100f, new MBList<Agent>()))
			{
				if ((object)item.Character == NavalStorylineData.Gunnar.CharacterObject)
				{
					val = item;
					break;
				}
			}
			if (val != null)
			{
				Agent val2 = SpawnLahar(val);
				val2.SetLookAgent(Agent.Main);
				Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<Agent> { val2 }, true);
			}
			return true;
		}
		return false;
	}

	private bool IsAct3Quest2ReadyToStart(Hero conversationHero)
	{
		if (NavalStorylineData.IsStorylineActivationPossible() && NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest1) && Hero.OneToOneConversationHero == conversationHero && Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SailToTheGulfOfCharasQuest)))
		{
			return !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(HuntDownTheEmiraAlFahdaAndTheCorsairsQuest));
		}
		return false;
	}

	private bool IsGunnar(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
	}

	private bool IsLahar(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Lahar.CharacterObject;
	}

	private bool IsMainHero(IAgent agent)
	{
		return (object)agent.Character == CharacterObject.PlayerCharacter;
	}

	private Agent SpawnLahar(Agent gunnar)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Lahar.CharacterObject);
		val.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
		Vec3 val2 = gunnar.Position - Agent.Main.Position;
		((Vec3)(ref val2)).RotateAboutZ(0.34906584f);
		val2 += Agent.Main.Position;
		int num = 250;
		while (true)
		{
			Mission current = Mission.Current;
			UIntPtr? obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				Scene scene = current.Scene;
				obj = ((scene != null) ? new UIntPtr?(scene.GetNavigationMeshForPosition(ref val2)) : ((UIntPtr?)null));
			}
			UIntPtr? uIntPtr = obj;
			UIntPtr zero = UIntPtr.Zero;
			if (!uIntPtr.HasValue || (uIntPtr.HasValue && !(uIntPtr.GetValueOrDefault() == zero)) || num == 0)
			{
				break;
			}
			if (MBRandom.RandomFloat > 0.5f)
			{
				((Vec3)(ref val2)).RotateAboutZ(MathF.PI / 180f * (float)MBRandom.RandomInt(20, 45));
			}
			else
			{
				((Vec3)(ref val2)).RotateAboutZ(MathF.PI / 180f * (float)MBRandom.RandomInt(-45, -20));
			}
			num--;
		}
		if (num == 0)
		{
			Debug.FailedAssert("Couldn't find a valid position for Lahar around Gunnar", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\CampaignBehaviors\\NavalStorylineThirdActSecondQuestBehavior.cs", "SpawnLahar", 284);
			val2 = Mission.Current.GetRandomPositionAroundPoint(gunnar.Position, 1f, 3f, true);
		}
		val.InitialPosition(ref val2);
		Vec3 lookDirection = Agent.Main.LookDirection;
		Vec2 val3 = ((Vec3)(ref lookDirection)).AsVec2;
		val3 = -((Vec2)(ref val3)).Normalized();
		val.InitialDirection(ref val3);
		val.NoHorses(true);
		val.CivilianEquipment(true);
		return Mission.Current.SpawnAgent(val, false);
	}

	private void OnPlayerAcceptsQuestThroughMission()
	{
		_isQuestAcceptedThroughMission = true;
		GameMenu.ActivateGameMenu("naval_storyline_act_3_quest_2_conversation_menu");
		Mission.Current.EndMission();
	}

	private void StartQuest()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SailToTheGulfOfCharasQuest)))
		{
			CampaignVec2 val = default(CampaignVec2);
			((CampaignVec2)(ref val))._002Ector(new Vec2(194.4578f, 359.8387f), false);
			if (!NavigationHelper.IsPositionValidForNavigationType(val, (NavigationType)2))
			{
				val = NavigationHelper.FindReachablePointAroundPosition(val, (NavigationType)2, 10f, 0f, false);
			}
			((QuestBase)new SailToTheGulfOfCharasQuest("naval_storyline_act3_quest2_1", NavalStorylineData.Gunnar, val)).StartQuest();
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
