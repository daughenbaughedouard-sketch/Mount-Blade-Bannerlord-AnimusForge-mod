using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NavalDLC.Storyline.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Storyline.CampaignBehaviors;

public class NavalStorylineThirdActFourthQuestBehavior : CampaignBehaviorBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_0;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_1;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_2;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_3;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_4;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_5;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_6;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_7;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_8;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_9;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_10;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_12;

		public static OnConsequenceDelegate _003C_003E9__8_13;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_14;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_15;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_16;

		public static OnMultipleConversationConsequenceDelegate _003C_003E9__8_18;

		public static OnConsequenceDelegate _003C_003E9__8_19;

		public static Func<Agent, bool> _003C_003E9__10_0;

		public static Func<Agent, bool> _003C_003E9__12_0;

		internal bool _003CAddDialogs_003Eb__8_0(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
		}

		internal bool _003CAddDialogs_003Eb__8_1(IAgent agent)
		{
			return (object)agent.Character == CharacterObject.PlayerCharacter;
		}

		internal bool _003CAddDialogs_003Eb__8_2(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
		}

		internal bool _003CAddDialogs_003Eb__8_3(IAgent agent)
		{
			return (object)agent.Character == CharacterObject.PlayerCharacter;
		}

		internal bool _003CAddDialogs_003Eb__8_4(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
		}

		internal bool _003CAddDialogs_003Eb__8_5(IAgent agent)
		{
			return (object)agent.Character == CharacterObject.PlayerCharacter;
		}

		internal bool _003CAddDialogs_003Eb__8_6(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
		}

		internal bool _003CAddDialogs_003Eb__8_7(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
		}

		internal bool _003CAddDialogs_003Eb__8_8(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
		}

		internal bool _003CAddDialogs_003Eb__8_9(IAgent agent)
		{
			return (object)agent.Character == CharacterObject.PlayerCharacter;
		}

		internal bool _003CAddDialogs_003Eb__8_10(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
		}

		internal bool _003CAddDialogs_003Eb__8_12(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
		}

		internal void _003CAddDialogs_003Eb__8_13()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}

		internal bool _003CAddDialogs_003Eb__8_14(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
		}

		internal bool _003CAddDialogs_003Eb__8_15(IAgent agent)
		{
			return (object)agent.Character == CharacterObject.PlayerCharacter;
		}

		internal bool _003CAddDialogs_003Eb__8_16(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
		}

		internal bool _003CAddDialogs_003Eb__8_18(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
		}

		internal void _003CAddDialogs_003Eb__8_19()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}

		internal bool _003CGunnarActivateQuestFourDialog1OnCondition_003Eb__10_0(Agent x)
		{
			return (object)x.Character == NavalStorylineData.Bjolgur.CharacterObject;
		}

		internal bool _003CSpawnBjolgur_003Eb__12_0(Agent x)
		{
			return (object)x.Character == NavalStorylineData.Gunnar.CharacterObject;
		}
	}

	private const string QuestConversationMenuId = "naval_storyline_act_3_quest_4_conversation_menu";

	private bool _isQuestAcceptedThroughMission;

	private bool _initialConversationIsDone;

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

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnSessionLaunched(CampaignGameStarter starter)
	{
		AddDialogs();
		AddGameMenus(starter);
	}

	private void AddGameMenus(CampaignGameStarter starter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		starter.AddGameMenu("naval_storyline_act_3_quest_4_conversation_menu", string.Empty, new OnInitDelegate(naval_storyline_act_3_quest_4_conversation_menu_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
	}

	private void AddDialogs()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0069: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00db: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_012b: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Expected O, but got Unknown
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Expected O, but got Unknown
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Expected O, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Expected O, but got Unknown
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Expected O, but got Unknown
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Expected O, but got Unknown
		//IL_0333: Expected O, but got Unknown
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Expected O, but got Unknown
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Expected O, but got Unknown
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Expected O, but got Unknown
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Expected O, but got Unknown
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Expected O, but got Unknown
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200);
		TextObject val = new TextObject("{=sob0plMW}Good news, {PLAYER.NAME}... Bjolgur’s order has given him permission to sail with us.", (Dictionary<string, object>)null);
		object obj2 = _003C_003Ec._003C_003E9__8_0;
		if (obj2 == null)
		{
			OnMultipleConversationConsequenceDelegate val2 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
			_003C_003Ec._003C_003E9__8_0 = val2;
			obj2 = (object)val2;
		}
		object obj3 = _003C_003Ec._003C_003E9__8_1;
		if (obj3 == null)
		{
			OnMultipleConversationConsequenceDelegate val3 = (IAgent agent) => (object)agent.Character == CharacterObject.PlayerCharacter;
			_003C_003Ec._003C_003E9__8_1 = val3;
			obj3 = (object)val3;
		}
		DialogFlow obj4 = obj.NpcLine(val, (OnMultipleConversationConsequenceDelegate)obj2, (OnMultipleConversationConsequenceDelegate)obj3, (string)null, (string)null).Condition(new OnConditionDelegate(GunnarActivateQuestFourDialog1OnCondition)).Consequence(new OnConsequenceDelegate(GunnarActivateQuestFourDialog1OnConsequence));
		TextObject val4 = new TextObject("{=eiX98VE9}Greetings, {PLAYER.NAME}... I’ve got my longship, Corpse-Maker, and more of my brothers may yet join us on the journey. We also brought a captured vessel, agile and light, which mounts a ballista. We call it the Golden Wasp. We’ve bought up most of the ale in Ostican for our voyage, as I think we’ll be heading for the sweltering seas of the south.", (Dictionary<string, object>)null);
		object obj5 = _003C_003Ec._003C_003E9__8_2;
		if (obj5 == null)
		{
			OnMultipleConversationConsequenceDelegate val5 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
			_003C_003Ec._003C_003E9__8_2 = val5;
			obj5 = (object)val5;
		}
		object obj6 = _003C_003Ec._003C_003E9__8_3;
		if (obj6 == null)
		{
			OnMultipleConversationConsequenceDelegate val6 = (IAgent agent) => (object)agent.Character == CharacterObject.PlayerCharacter;
			_003C_003Ec._003C_003E9__8_3 = val6;
			obj6 = (object)val6;
		}
		DialogFlow obj7 = obj4.NpcLine(val4, (OnMultipleConversationConsequenceDelegate)obj5, (OnMultipleConversationConsequenceDelegate)obj6, (string)null, (string)null);
		TextObject val7 = new TextObject("{=egYc68CI}I’ve been making some inquiries. Crusas is well-known and respected in the Empire and in Vlandia. He mines sulfur from islands in the Gulf of Charas. No doubt he uses some of Purig’s slaves, but I guess the grand lords and ladies don’t know that, or choose not to know.", (Dictionary<string, object>)null);
		object obj8 = _003C_003Ec._003C_003E9__8_4;
		if (obj8 == null)
		{
			OnMultipleConversationConsequenceDelegate val8 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
			_003C_003Ec._003C_003E9__8_4 = val8;
			obj8 = (object)val8;
		}
		object obj9 = _003C_003Ec._003C_003E9__8_5;
		if (obj9 == null)
		{
			OnMultipleConversationConsequenceDelegate val9 = (IAgent agent) => (object)agent.Character == CharacterObject.PlayerCharacter;
			_003C_003Ec._003C_003E9__8_5 = val9;
			obj9 = (object)val9;
		}
		DialogFlow obj10 = obj7.NpcLine(val7, (OnMultipleConversationConsequenceDelegate)obj8, (OnMultipleConversationConsequenceDelegate)obj9, (string)null, (string)null).BeginPlayerOptions((string)null, false);
		object obj11 = _003C_003Ec._003C_003E9__8_6;
		if (obj11 == null)
		{
			OnMultipleConversationConsequenceDelegate val10 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
			_003C_003Ec._003C_003E9__8_6 = val10;
			obj11 = (object)val10;
		}
		DialogFlow obj12 = obj10.PlayerOption("{=npbsJToM}I hope, then, that he should not be difficult to find.", (OnMultipleConversationConsequenceDelegate)obj11, (string)null, (string)null).GotoDialogState("q4_next_line");
		object obj13 = _003C_003Ec._003C_003E9__8_7;
		if (obj13 == null)
		{
			OnMultipleConversationConsequenceDelegate val11 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
			_003C_003Ec._003C_003E9__8_7 = val11;
			obj13 = (object)val11;
		}
		DialogFlow obj14 = obj12.PlayerOption("{=Cywj1xTj}Well respected or not, I’m ready to track him down.", (OnMultipleConversationConsequenceDelegate)obj13, (string)null, (string)null).GotoDialogState("q4_next_line").EndPlayerOptions();
		TextObject obj15 = new TextObject("{=sghtD7ov}Not hard to find at all.. On the way here I hailed some fishermen who chase tuna in the Gulf of Charas, and they say he is known to frequent a string of islands known as the Skatrias. They are said to be barren and foul-smelling. I can’t think why a merchant would want to anchor there, were they not the site of these sulfur mines where the captives are sent.{NEW_LINE}{NEW_LINE}So… I say we set out for these islands and hunt for Crusas.", (Dictionary<string, object>)null).SetTextVariable("NEW_LINE", "\n");
		object obj16 = _003C_003Ec._003C_003E9__8_8;
		if (obj16 == null)
		{
			OnMultipleConversationConsequenceDelegate val12 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
			_003C_003Ec._003C_003E9__8_8 = val12;
			obj16 = (object)val12;
		}
		object obj17 = _003C_003Ec._003C_003E9__8_9;
		if (obj17 == null)
		{
			OnMultipleConversationConsequenceDelegate val13 = (IAgent agent) => (object)agent.Character == CharacterObject.PlayerCharacter;
			_003C_003Ec._003C_003E9__8_9 = val13;
			obj17 = (object)val13;
		}
		DialogFlow obj18 = obj14.NpcLine(obj15, (OnMultipleConversationConsequenceDelegate)obj16, (OnMultipleConversationConsequenceDelegate)obj17, "q4_next_line", "q4_next_line_player_choices").BeginPlayerOptions("q4_next_line_player_choices", false);
		object obj19 = _003C_003Ec._003C_003E9__8_10;
		if (obj19 == null)
		{
			OnMultipleConversationConsequenceDelegate val14 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
			_003C_003Ec._003C_003E9__8_10 = val14;
			obj19 = (object)val14;
		}
		DialogFlow obj20 = obj18.PlayerOption("{=el44RZG4}Let us set out, then.", (OnMultipleConversationConsequenceDelegate)obj19, (string)null, (string)null).Consequence((OnConsequenceDelegate)delegate
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
		}).CloseDialog();
		object obj21 = _003C_003Ec._003C_003E9__8_12;
		if (obj21 == null)
		{
			OnMultipleConversationConsequenceDelegate val15 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
			_003C_003Ec._003C_003E9__8_12 = val15;
			obj21 = (object)val15;
		}
		DialogFlow obj22 = obj20.PlayerOption("{=a0j86F9C}I need a bit more time.", (OnMultipleConversationConsequenceDelegate)obj21, (string)null, (string)null);
		object obj23 = _003C_003Ec._003C_003E9__8_13;
		if (obj23 == null)
		{
			OnConsequenceDelegate val16 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__8_13 = val16;
			obj23 = (object)val16;
		}
		conversationManager.AddDialogFlow(obj22.Consequence((OnConsequenceDelegate)obj23).CloseDialog().PlayerOption("{=aEKNUI45}This war on the Sea Hounds is too risky. There must be another way to get my sister back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_ransom_sister")
			.EndPlayerOptions(), (object)null);
		ConversationManager conversationManager2 = Campaign.Current.ConversationManager;
		DialogFlow obj24 = DialogFlow.CreateDialogFlow("start", 1200);
		TextObject val17 = new TextObject("{=C8aEfvMM}Are we ready to set sail for the Skatrias? I imagine that Crusas will be docked there for some time, but we don’t want to miss this opportunity.", (Dictionary<string, object>)null);
		object obj25 = _003C_003Ec._003C_003E9__8_14;
		if (obj25 == null)
		{
			OnMultipleConversationConsequenceDelegate val18 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
			_003C_003Ec._003C_003E9__8_14 = val18;
			obj25 = (object)val18;
		}
		object obj26 = _003C_003Ec._003C_003E9__8_15;
		if (obj26 == null)
		{
			OnMultipleConversationConsequenceDelegate val19 = (IAgent agent) => (object)agent.Character == CharacterObject.PlayerCharacter;
			_003C_003Ec._003C_003E9__8_15 = val19;
			obj26 = (object)val19;
		}
		DialogFlow obj27 = obj24.NpcLine(val17, (OnMultipleConversationConsequenceDelegate)obj25, (OnMultipleConversationConsequenceDelegate)obj26, (string)null, (string)null).Condition(new OnConditionDelegate(GunnarActivateQuestFourDialog2OnCondition)).BeginPlayerOptions((string)null, false);
		object obj28 = _003C_003Ec._003C_003E9__8_16;
		if (obj28 == null)
		{
			OnMultipleConversationConsequenceDelegate val20 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
			_003C_003Ec._003C_003E9__8_16 = val20;
			obj28 = (object)val20;
		}
		DialogFlow obj29 = obj27.PlayerOption("{=el44RZG4}Let us set out, then.", (OnMultipleConversationConsequenceDelegate)obj28, (string)null, (string)null).Consequence((OnConsequenceDelegate)delegate
		{
			if (Mission.Current == null)
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += ActivateQuest4;
			}
			else
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
			}
		}).CloseDialog();
		object obj30 = _003C_003Ec._003C_003E9__8_18;
		if (obj30 == null)
		{
			OnMultipleConversationConsequenceDelegate val21 = (IAgent agent) => (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
			_003C_003Ec._003C_003E9__8_18 = val21;
			obj30 = (object)val21;
		}
		DialogFlow obj31 = obj29.PlayerOption("{=a0j86F9C}I need a bit more time.", (OnMultipleConversationConsequenceDelegate)obj30, (string)null, (string)null);
		object obj32 = _003C_003Ec._003C_003E9__8_19;
		if (obj32 == null)
		{
			OnConsequenceDelegate val22 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__8_19 = val22;
			obj32 = (object)val22;
		}
		conversationManager2.AddDialogFlow(obj31.Consequence((OnConsequenceDelegate)obj32).CloseDialog().PlayerOption("{=aEKNUI45}This war on the Sea Hounds is too risky. There must be another way to get my sister back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_ransom_sister")
			.EndPlayerOptions(), (object)null);
	}

	private void naval_storyline_act_3_quest_4_conversation_menu_on_init(MenuCallbackArgs args)
	{
		if (_isQuestAcceptedThroughMission && Mission.Current == null)
		{
			ActivateQuest4();
			_isQuestAcceptedThroughMission = false;
		}
	}

	private bool GunnarActivateQuestFourDialog1OnCondition()
	{
		int num;
		if (!_initialConversationIsDone && Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && !NavalStorylineData.IsNavalStoryLineActive() && NavalStorylineData.IsStorylineActivationPossible())
		{
			num = (NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3SpeakToSailors) ? 1 : 0);
			if (num != 0)
			{
				SpawnBjolgur();
				Agent item = ((IEnumerable<Agent>)Mission.Current.Agents).First((Agent x) => (object)x.Character == NavalStorylineData.Bjolgur.CharacterObject);
				ConversationManager conversationManager = Campaign.Current.ConversationManager;
				MBList<IAgent> obj = new MBList<IAgent>();
				((List<IAgent>)(object)obj).Add((IAgent)(object)item);
				conversationManager.AddConversationAgents((IEnumerable<IAgent>)obj, false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private void GunnarActivateQuestFourDialog1OnConsequence()
	{
		_initialConversationIsDone = true;
	}

	private static void SpawnBjolgur()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		Agent val = ((IEnumerable<Agent>)Mission.Current.Agents).First((Agent x) => (object)x.Character == NavalStorylineData.Gunnar.CharacterObject);
		AgentBuildData val2 = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Bjolgur.CharacterObject);
		val2.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val2.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
		Vec3 val3 = val.Position - Agent.Main.Position;
		((Vec3)(ref val3)).RotateAboutZ(0.34906584f);
		val3 += Agent.Main.Position;
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
				obj = ((scene != null) ? new UIntPtr?(scene.GetNavigationMeshForPosition(ref val3)) : ((UIntPtr?)null));
			}
			UIntPtr? uIntPtr = obj;
			UIntPtr zero = UIntPtr.Zero;
			if (!uIntPtr.HasValue || (uIntPtr.HasValue && !(uIntPtr.GetValueOrDefault() == zero)) || num == 0)
			{
				break;
			}
			if (MBRandom.RandomFloat > 0.5f)
			{
				((Vec3)(ref val3)).RotateAboutZ(MathF.PI / 180f * (float)MBRandom.RandomInt(20, 45));
			}
			else
			{
				((Vec3)(ref val3)).RotateAboutZ(MathF.PI / 180f * (float)MBRandom.RandomInt(-45, -20));
			}
			num--;
		}
		if (num == 0)
		{
			Debug.FailedAssert("Couldn't find a valid position for Bjolgur around Gunnar", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\CampaignBehaviors\\NavalStorylineThirdActFourthQuestBehavior.cs", "SpawnBjolgur", 169);
			val3 = Mission.Current.GetRandomPositionAroundPoint(val.Position, 1f, 3f, true);
		}
		val2.InitialPosition(ref val3);
		Vec3 lookDirection = Agent.Main.LookDirection;
		Vec2 val4 = ((Vec3)(ref lookDirection)).AsVec2;
		val4 = -((Vec2)(ref val4)).Normalized();
		val2.InitialDirection(ref val4);
		val2.NoHorses(true);
		val2.CivilianEquipment(true);
		Mission.Current.SpawnAgent(val2, false);
	}

	private bool GunnarActivateQuestFourDialog2OnCondition()
	{
		if (_initialConversationIsDone && Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && !NavalStorylineData.IsNavalStoryLineActive() && NavalStorylineData.IsStorylineActivationPossible())
		{
			return NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3SpeakToSailors);
		}
		return false;
	}

	private void OnPlayerAcceptsQuestThroughMission()
	{
		_isQuestAcceptedThroughMission = true;
		OpenQuestMenu();
		Mission.Current.EndMission();
	}

	private void OpenQuestMenu()
	{
		GameMenu.ActivateGameMenu("naval_storyline_act_3_quest_4_conversation_menu");
	}

	private void ActivateQuest4()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 corsairSpawnPosition = default(CampaignVec2);
		((CampaignVec2)(ref corsairSpawnPosition))._002Ector(new Vec2(285f, 300f), false);
		((QuestBase)new GoToSkatriaIslandsQuest("naval_storyline_act_3_quest_4", NavalStorylineData.Gunnar, corsairSpawnPosition)).StartQuest();
	}
}
