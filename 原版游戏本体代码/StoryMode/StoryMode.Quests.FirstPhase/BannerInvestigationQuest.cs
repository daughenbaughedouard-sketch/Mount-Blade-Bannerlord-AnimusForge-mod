using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.FirstPhase;

public class BannerInvestigationQuest : StoryModeQuestBase
{
	private const int NotablesToTalkAmount = 10;

	private const string MonchugStringId = "lord_6_1";

	private const string MesuiStringId = "lord_6_4";

	private const string HurunagStringId = "lord_6_16";

	private const string CaladogStringId = "lord_5_1";

	private const string ErgeonStringId = "lord_5_3";

	private const string MelidirStringId = "lord_5_5";

	private const string DerthertStringId = "lord_4_1";

	private const string UntheryStringId = "lord_4_5";

	private const string IngaltherStringId = "lord_4_16";

	private const string UnqidStringId = "lord_3_1";

	private const string AdramStringId = "lord_3_3";

	private const string TaisStringId = "lord_3_5";

	private const string RaganvadStringId = "lord_2_1";

	private const string OlekStringId = "lord_2_3";

	private const string GodunStringId = "lord_2_5";

	private const string LuconStringId = "lord_1_1";

	private const string PentonStringId = "lord_1_5";

	private const string GariosStringId = "lord_1_7";

	private const string RhagaeaStringId = "lord_1_14";

	[SaveableField(1)]
	private Dictionary<Hero, bool> _noblesToTalk;

	[SaveableField(2)]
	private bool _allNoblesDead;

	[SaveableField(3)]
	private bool _battleSummarized;

	[SaveableField(4)]
	private JournalLog _talkedNotablesQuestLog;

	private TextObject _startQuestLog => new TextObject("{=ysRVZiA6}As you explore Calradia, you can learn more about your artifact and its importance by asking any lord or lady about the Empire's recent history.", (Dictionary<string, object>)null);

	private TextObject _endQuestLog => new TextObject("{=oA4iTPyV}You have collected enough information.", (Dictionary<string, object>)null);

	public override TextObject Title => new TextObject("{=zLRlmitp}Investigate 'Neretzes' Folly'", (Dictionary<string, object>)null);

	public override bool IsRemainingTimeHidden => false;

	public BannerInvestigationQuest()
		: base("investigate_neretzes_banner_quest", null, StoryModeManager.Current.MainStoryLine.FirstPhase.FirstPhaseEndTime)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_allNoblesDead = false;
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
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
		CampaignEvents.OnPartyRemovedEvent.AddNonSerializedListener((object)this, (Action<PartyBase>)OnPartyRemoved);
		CampaignEvents.MobilePartyCreated.AddNonSerializedListener((object)this, (Action<MobileParty>)OnPartySpawned);
		CampaignEvents.OnPartyLeaderChangedEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Hero>)OnPartyLeaderChanged);
	}

	private void OnPartyLeaderChanged(MobileParty mobileParty, Hero oldLeader)
	{
		bool num = mobileParty.LeaderHero != null && _noblesToTalk.ContainsKey(mobileParty.LeaderHero);
		bool flag = ((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)mobileParty);
		if (num)
		{
			if (!flag)
			{
				((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)mobileParty);
			}
		}
		else if (flag)
		{
			((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)mobileParty);
		}
	}

	private void InitializeNotablesToTalkList()
	{
		_noblesToTalk = new Dictionary<Hero, bool>();
		Hero val = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_6_1");
		if (val.IsAlive)
		{
			_noblesToTalk.Add(val, value: false);
		}
		Hero val2 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_6_4");
		if (val2.IsAlive)
		{
			_noblesToTalk.Add(val2, value: false);
		}
		Hero val3 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_6_16");
		if (val3.IsAlive)
		{
			_noblesToTalk.Add(val3, value: false);
		}
		Hero val4 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_5_1");
		if (val4.IsAlive)
		{
			_noblesToTalk.Add(val4, value: false);
		}
		Hero val5 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_5_3");
		if (val5.IsAlive)
		{
			_noblesToTalk.Add(val5, value: false);
		}
		Hero val6 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_5_5");
		if (val6.IsAlive)
		{
			_noblesToTalk.Add(val6, value: false);
		}
		Hero val7 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_4_1");
		if (val7.IsAlive)
		{
			_noblesToTalk.Add(val7, value: false);
		}
		Hero val8 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_4_16");
		if (val8.IsAlive)
		{
			_noblesToTalk.Add(val8, value: false);
		}
		Hero val9 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_4_5");
		if (val9.IsAlive)
		{
			_noblesToTalk.Add(val9, value: false);
		}
		Hero val10 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_3_1");
		if (val10.IsAlive)
		{
			_noblesToTalk.Add(val10, value: false);
		}
		Hero val11 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_3_3");
		if (val11.IsAlive)
		{
			_noblesToTalk.Add(val11, value: false);
		}
		Hero val12 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_3_5");
		if (val12.IsAlive)
		{
			_noblesToTalk.Add(val12, value: false);
		}
		Hero val13 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_2_1");
		if (val13.IsAlive)
		{
			_noblesToTalk.Add(val13, value: false);
		}
		Hero val14 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_2_3");
		if (val14.IsAlive)
		{
			_noblesToTalk.Add(val14, value: false);
		}
		Hero val15 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_2_5");
		if (val15.IsAlive)
		{
			_noblesToTalk.Add(val15, value: false);
		}
		Hero val16 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_1_1");
		if (val16.IsAlive)
		{
			_noblesToTalk.Add(val16, value: false);
		}
		Hero val17 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_1_5");
		if (val17.IsAlive)
		{
			_noblesToTalk.Add(val17, value: false);
		}
		Hero val18 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_1_14");
		if (val18.IsAlive)
		{
			_noblesToTalk.Add(val18, value: false);
		}
		Hero val19 = Campaign.Current.CampaignObjectManager.Find<Hero>("lord_1_7");
		if (val19.IsAlive)
		{
			_noblesToTalk.Add(val19, value: false);
		}
		foreach (KeyValuePair<Hero, bool> item in _noblesToTalk)
		{
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)item.Key);
			if (item.Key.PartyBelongedTo != null)
			{
				((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)item.Key.PartyBelongedTo);
			}
		}
	}

	protected override void OnStartQuest()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		InitializeNotablesToTalkList();
		((QuestBase)this).SetDialogs();
		_talkedNotablesQuestLog = ((QuestBase)this).AddDiscreteLog(_startQuestLog, new TextObject("{=T8naYoGH}Nobles to Talk to", (Dictionary<string, object>)null), 0, 10, (TextObject)null, false);
	}

	private void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
	{
		if (!_allNoblesDead && _noblesToTalk.ContainsKey(victim))
		{
			if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)victim))
			{
				((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)victim);
			}
			UpdateAllNoblesDead();
		}
	}

	private void UpdateAllNoblesDead()
	{
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<Hero, bool> item in _noblesToTalk)
		{
			if (item.Value)
			{
				num++;
			}
			else if (item.Key.IsAlive)
			{
				num2++;
			}
		}
		if (num2 <= 0)
		{
			_allNoblesDead = true;
		}
		if (num >= 9)
		{
			Campaign.Current.ConversationManager.RemoveRelatedLines((object)this);
			((QuestBase)this).SetDialogs();
		}
	}

	protected override void SetDialogs()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		SetNobleDialogs();
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("hero_main_options", 150).BeginPlayerOptions((string)null, false).PlayerSpecialOption(new TextObject("{=qV1e0x8i}What is 'Neretzes' Folly'?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero != null && (int)CharacterObject.OneToOneConversationCharacter.Occupation == 3 && Hero.OneToOneConversationHero.Clan != Hero.MainHero.Clan && !_battleSummarized && !MobileParty.MainParty.IsInRaftState && (MobileParty.ConversationParty == null || !MobileParty.ConversationParty.IsInRaftState)))
			.NpcLine(new TextObject("{=hFYG3lXw}Well, that's what some people call the great Battle of Pendraic in the year 1077.[ib:normal][if:convo_normal]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=TKpFB4qN}Emperor Neretzes led an army accompanied by Khuzaits and Aserai to fight a coalition of Sturgians, Battanians and Vlandians.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=0yxEvgGf}It was a disaster for him - he died in it - but the victors didn't fare much better.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=AmBEgOyq}I see...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(BattleSummarized))
			.GotoDialogState("lord_pretalk"), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("hero_main_options", 150).BeginPlayerOptions((string)null, false).PlayerSpecialOption(new TextObject("{=6UIa4784}Can you tell me anything about the battle of Pendraic?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition(new OnConditionDelegate(talk_with_any_noble_condition))
			.NpcLine("{=HDBkwjgf}{NOBLE_ANSWER}", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition(new OnConditionDelegate(talk_with_any_noble_continue_condition))
			.Consequence(new OnConsequenceDelegate(talk_with_any_noble_consequence))
			.GotoDialogState("lord_pretalk"), (object)this);
	}

	private bool talk_with_any_noble_condition()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if (_battleSummarized && Hero.OneToOneConversationHero != null && (int)CharacterObject.OneToOneConversationCharacter.Occupation == 3 && Hero.OneToOneConversationHero.Clan != Hero.MainHero.Clan && !_noblesToTalk.ContainsKey(Hero.OneToOneConversationHero) && !MobileParty.MainParty.IsInRaftState)
		{
			if (MobileParty.ConversationParty != null)
			{
				return !MobileParty.ConversationParty.IsInRaftState;
			}
			return true;
		}
		return false;
	}

	private bool talk_with_any_noble_continue_condition()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		TextObject val;
		if (_allNoblesDead)
		{
			val = new TextObject("{=HOb07MJF}I don't know much about it. I know two who might, though. {IMPERIAL_MENTOR.LINK} - she once served as Neretez's unofficial spymaster. Then there's {ANTI_IMPERIAL_MENTOR.LINK}. He served on Neretzes' bodyguard, but people say now he's very different than he was back then.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("IMPERIAL_MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			StringHelpers.SetCharacterProperties("ANTI_IMPERIAL_MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
		}
		else
		{
			val = new TextObject("{=HddJT1Ve}I wasn't there. I know {HERO.LINK} has some thoughts about it.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("HERO", ((!_noblesToTalk.Any((KeyValuePair<Hero, bool> n) => !n.Value && n.Key.IsAlive && n.Key.Culture == Hero.OneToOneConversationHero.Culture)) ? _noblesToTalk.First((KeyValuePair<Hero, bool> n) => !n.Value && n.Key.IsAlive) : _noblesToTalk.First((KeyValuePair<Hero, bool> n) => !n.Value && n.Key.IsAlive && n.Key.Culture == Hero.OneToOneConversationHero.Culture)).Key.CharacterObject, val, false);
		}
		MBTextManager.SetTextVariable("NOBLE_ANSWER", val, false);
		return true;
	}

	private void BattleSummarized()
	{
		_battleSummarized = true;
		Campaign.Current.ConversationManager.ConversationEndOneShot += UpdateAllNoblesDead;
	}

	private void talk_with_any_noble_consequence()
	{
		if (_allNoblesDead)
		{
			((QuestBase)this).CompleteQuestWithSuccess();
		}
		Campaign.Current.ConversationManager.ConversationEndOneShot += UpdateAllNoblesDead;
	}

	private void SetNobleDialogs()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Expected O, but got Unknown
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected O, but got Unknown
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Expected O, but got Unknown
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Expected O, but got Unknown
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Expected O, but got Unknown
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Expected O, but got Unknown
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected O, but got Unknown
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Expected O, but got Unknown
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Expected O, but got Unknown
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Expected O, but got Unknown
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Expected O, but got Unknown
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Expected O, but got Unknown
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Expected O, but got Unknown
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Expected O, but got Unknown
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Expected O, but got Unknown
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Expected O, but got Unknown
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Expected O, but got Unknown
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Expected O, but got Unknown
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Expected O, but got Unknown
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Expected O, but got Unknown
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Expected O, but got Unknown
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Expected O, but got Unknown
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Expected O, but got Unknown
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Expected O, but got Unknown
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Expected O, but got Unknown
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Expected O, but got Unknown
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Expected O, but got Unknown
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Expected O, but got Unknown
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Expected O, but got Unknown
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Expected O, but got Unknown
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Expected O, but got Unknown
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Expected O, but got Unknown
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Expected O, but got Unknown
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Expected O, but got Unknown
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Expected O, but got Unknown
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Expected O, but got Unknown
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Expected O, but got Unknown
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Expected O, but got Unknown
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Expected O, but got Unknown
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Expected O, but got Unknown
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Expected O, but got Unknown
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Expected O, but got Unknown
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0503: Expected O, but got Unknown
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Expected O, but got Unknown
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Expected O, but got Unknown
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Expected O, but got Unknown
		//IL_054b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Expected O, but got Unknown
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Expected O, but got Unknown
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Expected O, but got Unknown
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Expected O, but got Unknown
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Expected O, but got Unknown
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ae: Expected O, but got Unknown
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d4: Expected O, but got Unknown
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Expected O, but got Unknown
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Expected O, but got Unknown
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Expected O, but got Unknown
		foreach (KeyValuePair<Hero, bool> item in _noblesToTalk)
		{
			if (!item.Value)
			{
				TextObject answer;
				TextObject answer2;
				TextObject answer3;
				TextObject answer4;
				if (((MBObjectBase)item.Key).StringId == "lord_6_1")
				{
					answer = new TextObject("{=L1V8L2N1}Yes. The Emperor Neretzes had offered to hire our warriors as mercenaries. I saw nothing wrong with that.[ib:confident3][if:convo_bored]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=A9fChKmz}The Empire was an old bear. Well-fed, slow-moving. It wanted to keep what it had. The Sturgians were, and are, hungry wolves. Like us. Sometimes wolves hunt in packs, and sometimes they don't. Sometimes one wolf wants the lion to kill his rival.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=tBGtavMG}Most of those who went were Khergits. They are a young clan. Their lineage is not like ours. They were always looking to prove themselves. Anyway, at the battle, their noyan, Solun, was slain alongside most of the males of his house. What can I say? A thirst for glory is dangerous, both to the thirsty one and those around him.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=UVuwofhS}Clans rise, clans fall. My duty is to all the Khuzaits. Look at it this way. Were it not for her husband's death, Lady Mesui would never have inherited the leadership of the Khergits. Death creates opportunity. The survivors of a great battle make a great show of mourning, but inside they rejoice.[ib:closed][if:convo_mocking_revenge]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_6_4")
				{
					answer = new TextObject("{=OKgsM4nr}I curse that name. It took from me my husband, two brothers, more cousins than I can count.[ib:confident2][if:convo_stern]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=KYW1UiKm}The Khergits were never the richest of the clans, but we made up for it with our valor. When word spread that the Emperor was promising silver for men to ride at his side, against the Sturgians and Battanians and others, of course our young brave boys lept at the chance. My husband, the bravest and best of them all, led them.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=kYtRsxSL}We fought the Vlandians. We won, but there was a great slaughter. My husband's horse was slain and he was ridden down, though he died amid a pile of Vlandian dead. Elsewhere on the field, the Emperor was having his head hewn off with a Sturgian axe, and thus was in no position to pay us.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=i5OOargr}Such are the fortunes of war. But what came afterwards... When word spread of what happened to our menfolk, the other clans - the Arkits in particular - knew we were weak. Our herds were raided. Anyone who protested was killed. Monchug did little to stop it. It taught us that valor will get you killed, but treachery will make you rich.[if:convo_angry]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_6_16")
				{
					answer = new TextObject("{=NGbgfPoP}I was there. Many of the Khuzaits went. Mostly the Khergit clan, who were hungry for glory, but I was also young and hungry for glory so I went along as well.[ib:demure2][if:convo_bored]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=kw9sltab}The Battanians had planned an ambush up in a wooded pass for the imperial vanguard, then the Vlandians and Sturgians were to come sweeping down on their flanks in the battle. Our scouts found the Battanian ambush, but Neretzes did not listen, and blundered into it anyway.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=Lep5YKdt}While Neretzes' vanguard was getting slaughtered, we met the Vlandians. But the Vlandians brought lots of crossbowmen, and our horse archers took heavy losses. Eventually the armored imperial cataphracts showed up, and rolled over the crossbows. But we were caught in a melee with the Vlandian knights, and that was where things got bloody.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=4Baa38ra}We won, barely, with the help of the imperials, but the Khergits were mauled. Since then, the Khergits have been rather weak - and you know what happens to the weak. Still, no one told them to put all their eggs in one basket like that.[if:convo_bored2]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_5_1")
				{
					answer = new TextObject("{=Y6OJHdMH}I am a busy man, but there is always time to talk about the blessed battle of Pendraic.[if:convo_mocking_teasing][ib:confident3]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=bmitHhDt}Our dear old beloved King Aeril, a wonderful man but with a heart perhaps just a might too tender, did not wish for us to go off to war. But then he disappeared and I, his son-in-law, ascended to the kingship. The clans cried out for war! They had a hundred years of crimes against them to avenge. I, a father to my people, gave them what they wanted.[if:convo_calm_friendly]", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=XEuhxpCX}Now, the Empire uses tricks and traps in war. No Battanian fears to meet an imperial soldier, man-to-man, but we thought it would be a good laugh to use their tricks against them. So we laid an ambush, on both sides of a wooded pass, and wouldn't you know? They marched right into it.[if:convo_happy]", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=aMBoh7gL}They turned and twisted as our arrows rained down upon them, like fish going frantic in a pond as you draw the net tighter. Then, when they were greatly discomfited, we took up our falxes and swords and reaped the harvest. Oh, there was some unpleasantness later with the Sturgians, about the spoils of war, but what a grand old day it was![if:convo_relaxed_happy]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_5_3")
				{
					answer = new TextObject("{=4Oc6MEvR}Ah... For any son of Battania, there will be no prouder moment in his life than that day. Any true son of Battania, anyway.[if:convo_calm_friendly][ib:hip]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=WUa1gnkS}Look, right before the battle, our high king, Aeril, disappears. And his adopted son Caladog becomes king. That sets tongues to wagging, you know? But let me tell you - old Aeril could never in his life have won such a victory as did Caladog, that day.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=2DZOCsy5}We waited for them, like wolves in the wood, as their vanguard came up the winding road. They came without archers to protect them. Caladog blew his horn, and our bowmen fired on them from all sides. They turned their shields one way, and were hit from the other. A glorious thing to watch...[if:convo_huge_smile]", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=x3KpQouT}When they were all good and addled like frightened sheep, running this way and that, the rest of us warriors descended upon them with our falxes and swords. I cleaved this way and that. I took 12 heads, and mine was far from the greatest catch. Ah, the grandchildren tire of me telling this story...[if:convo_calm_friendly]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_5_5")
				{
					answer = new TextObject("{=SFJFIaAD}Well... King Caladog's great victory... Who would dare say anything to tarnish its shine?[if:convo_undecided_open][ib:closed]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=T7ye4wUT}King Aeril disappeared while hunting, and Caladog becomes king. He leads the tribes to war. Oh, we were eager enough, even though Aeril had made a truce with the Emperor, sealed by oaths. When we were dazzled with the prospect of vengeance, who cares about our sacred word and honor?[ib:confident]", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=vjOHmHOH}The ambush... Masterfully planned and executed, that none can deny. But I will also not deny that the Sturgians fought the main body of the imperial forces, and the Vlandians fought their famous cavalry, so I don't think the greatest glory went to the sons of Battania.[if:convo_contemptuous]", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=kJxPtbHo}At the end of the day, what have we gained? The Sturgians hate us worse than ever. The Vlandians too. The Empire, I suppose, is shattered. What can I say... I believe that wars should have a goal. But I am a minority, it seems, among our people.[ib:closed]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_4_1")
				{
					answer = new TextObject("{=0IeByuam}It was a victory, of the kind that is almost as bad as a defeat.[if:convo_dismayed][ib:confident2]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=vaLehwRl}We had given an oath to the Empire, to join them if attacked. It seemed clear to me that we should have honored our oath, that the Battanians and Sturgians were aggressors, but, there is always room to argue details. Ultimately our barons did not wish to fight with the Empire, so they resisted coming to its help.[if:convo_undecided_closed]", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=6qgXAcZB}Neretzes, when he heard we were hesitating, sent us a message calling us cowards and traitors. And you say that to a Vlandian noble at your peril. Neretzes should have known what he was doing. We joined the Sturgians.[if:convo_undecided_closed]", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=xjk4hNXD}I did not fight in the battle. I stood on a hill telling my commanders where to go and who to attack. And we did rather well, I think you've heard. Still, we took losses - heavy losses, and gained little. And for this the barons blamed me, even though it was their idea to fight. I learned that day that a king should always lead, never follow. But it was a bitter lesson.[if:convo_contemptuous][ib:closed]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_4_16")
				{
					answer = new TextObject("{=zRZd90cZ}I was there. I was just a young squire then.", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=sejEf69A}I have heard no sweeter music than the thunder of our hooves as we bore down on the Aserai rabble. We fell on them like a falcon plunges upon a rabbit.[if:convo_bemused]", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=Og8nep1w}They had overextended themselves, chasing the imperial archers. Light foot before our knights - there was no contest. Let me tell you something - nine-tenths of victory is recognizing when your enemy has made a mistake. The rash perish as swiftly as the weak, and deserve it just as much.[ib:hip]", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=WN1tHPnB}We should have gone on to seize all the western empire. If Derthert had any manhood, we'd have done so. But his heart was never in the war. He believed he'd broken his oath to the Empire by helping the Sturgians, and it gnawed at him. He'd have made a fine lackey. Instead he's our king.[if:convo_contemptuous]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_4_5")
				{
					answer = new TextObject("{=9sAGLK6R}Yes. I was tasked by Derthert with command of the crossbowmen.[if:convo_grave][ib:confident3]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=duD8SJDb}Our knights spotted a mass of Aserai light infantry spread across a valley, and charged them. They weren't ready to meet the onslaught, and were routed. But then a bunch of Khuzaits showed up and kept their distance, giving the knights a hard time, shooting their horses out from under them. That's when we showed up.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=iQeMllr8}When it was just us fighting the horse archers, we were winning.. A man on foot can shoot as well or better than a man on horseback, all things being equal, and there were a lot more of us. So they started to go down and galloped off. The knights of course pursued, and that's when the problems started.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=BleybhsD}Imperial cataphracts showed up, armored head to foot and their horses too, so they just ignored our shooting and tore right through us. I was swept away in the retreat and saw no more of the battle. King Derthert had a good enough plan but the barons - Ingalther, Aldric, that lot - ignored him, as they always do.", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_3_1")
				{
					answer = new TextObject("{=BstbQV5Z}It was a tragedy that gnawed at the roots of all the great families of Calradia, even ours, so far away from the battle.[if:convo_annoyed][ib:normal2]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=aGX3L8mV}We heard that the Empire was making war on the Sturgians, or maybe it was the other way around. I thought that we had no stake in this quarrel but Nimr, a fiery young hero from the Bani Sarran, asked me for permission to take some young warriors, eager for glory.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=Zz6IvIH9}The Empire had left us alone for a while, and Neretzes was offering silver for men, so I thought, “Why not? Let them help the Empire.” Ah, I should have known. The best course with wars is to have as little to do with them as you possibly can.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=2NHnVb9b}So Nimr went, and fought, and won glory, but also got a number of men killed, especially those of the Banu Qild. And he became boastful, and arrogant. And then... Well, that is the beginning of the great feud between Sarranis and Qildis, but the rest of the story I should perhaps leave for someone else.[ib:closed][if:convo_undecided_closed]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_3_3")
				{
					answer = new TextObject("{=XIIISsIg}There was never a prouder moment for the Bani Sarran.[if:convo_calm_friendly][ib:hip]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=dWVsS3D8}The bravest and most valiant son of our clan, Nimr, led off a large group of Aserai warriors to fight for the Empire, for gold and glory. I went with them. When we saw the Battanian archers come down from the hills, Nimr was ready. He gave the word: we held our shields over our heads as the arrows rained down, then threw our javelins and charged. We cut them down.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=EOSyvdFG}Then the Vlandian knights came. We were attacked on two sides and the Emperor, who could have sent men to save us, took his time. Perhaps he wanted the best of the Aserai to die, lest we become too powerful later. But that betrayal was nothing compared to what we received from our fellow Aserai of the Banu Qild.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=Xg6jhfPt}Nimr returned, in well-deserved glory. A daughter of the Banu Qild took an interest in him, and they had a secret affair, as the youth sometimes do. As heroes do. But Nimr's acts wounded the Qildi's pride. They kidnapped him, slew him and hung him in a cage in their market. We will forgive the Empire and the Vlandians. The Qildis... better not ask me that.[if:convo_contemptuous][ib:confident]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_3_5")
				{
					answer = new TextObject("{=D4MuSwRB}A sad day for the Banu Qild. But we had our vengeance.[if:convo_stern][ib:hip]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=Qb0wsarP}There was a warrior named Nimr, of the Bani Sarran. He was brave, but arrogant. Of course the young people loved him. He wanted to lead men to fight with the Empire, and though there was no gain for us, Unqid let him. Unqid can be weak sometimes.[if:convo_thinking]", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=flkmzKDz}Many Qildi youth went with him. They died in their hundreds. And there was no gain - except for Nimr, who for some reason people considered a hero. It was despicable how they fawned on him.[if:convo_annoyed]", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=SF7dqoEe}Of course Nimr's arrogance doubled, and doubled again. And then he dealt us a great insult. I will not say what that insult was, because it no longer exists. We wiped it out. In the traditional way. You may ask someone else about that.", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_2_1")
				{
					answer = new TextObject("{=lIArdHty}Yes. The day my father died, thanks to Battanian treachery.[if:convo_annoyed][ib:convo_hip2]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=RtryETdA}When they pledged to support us in the battle, we believed they would stand with us in the shield wall, like men. But of course this is not the Battanian way. They sprung some woodland trickery up in the hills, killed off Neretzes' vanguard, and no doubt spent the rest of the battle whooping and boasting and chopping the heads off of men who were already dead.[if:convo_angry]", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=flGyoqa6}It was Sturgians who met Neretzes' guard face-to-face. My father ordered me to stay back as he led them into battle, but he was at their head. He forced them back, then they broke and ran for the shelter of their camp. We went and attacked their ramparts, and broke them, but my father was hit by an imperial mace at the moment of his triumph and died.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=Ri6NmpEE}I will never forget when a messenger ran to tell me that my father was dead. But I knew I must swallow my grief, because now I was king. I rode down into the ruins of the imperial camp to take their banner as a trophy, my inheritance won by my father and passed down to me. Oh, some of the boyars were insubordinate. But I have since showed them that I am master.[if:convo_contemptuous]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_2_3")
				{
					answer = new TextObject("{=X3pDRmjK}A victory, won by my father, claimed by Raganvad.[if:convo_annoyed][ib:warrior2]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=scjogyUD}Old king Vadinslav was brave enough. He led us all into battle. I stood at my father's side as we faced the imperials eye-to-eye over the tops of our shields. It was like any battle where shield walls meet - thrust and push, struggling to stay on your feet, but you can't really describe it. Let's just say it's the kind of battle that Sturgians usually win.[ib:confident2]", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=sJoO1JzN}When the imperials had had enough of us they broke and ran for the ramparts. There they threw darts and rocks and their cursed fire. We had to go up ladders, one by one. Vadinslav was hit by a mace and went down; my father then went up, cleaving as he went, and rallied us and led us to victory.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=sSaZVD2e}My father took the imperial dragon banner from dead Neretzes' hands - it's a famous story - and but then the little prince Raganvad tried to claim it. My father broke it over his knee, threw it at him, and told him to get his own toys to play with. Hah! It was a good, good day.[if:convo_huge_smile]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_2_5")
				{
					answer = new TextObject("{=jGhWOnjx}Yes. It was madness. The greatest blow struck against the Empire in a lifetime, and we squandered it squabbling among ourselves about a flag.[if:convo_stern][ib:closed2]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=ZkSthA9d}They say Olek the Old had pried the dragon banner out of the Emperor's dead hands. But then Prince Raganvad, who had not so much as drawn his sword in the battle, claimed it as a trophy. Olek, who was covered in his enemies' blood, laughed at Raganvad and told him to go find his own toy to play with. Raganvad struck him, so Olek broke the banner staff over his knee and threw it in his prince's face.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=3BjIb3CG}Or perhaps it was just Raganvad. He was stewing in his anger when up comes the Battanian king, Caladog. The Battanians had taken their time stripping the bodies of the imperial vanguard and the Sturgians were angry at them, so Raganvad called him a coward. Caladog sneers at him and walks off.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=ez4rbGmx}Insults his most powerful vassal and then insults his most valued ally. A fine day's work, wouldn't you say? But he has grown wiser since, though no more pleasant to spend time with.[ib:demure2]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_1_1")
				{
					answer = new TextObject("{=yRAl8YRL}Yes. I was a junior officer on Neretzes' staff. People say much about the battle that betrays a lack of understanding, of Neretzes and of the circumstances he faced.[if:convo_undecided_closed][ib:closed]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=bdbZLJf3}Neretzes had an obligation to avenge the Battanian attacks on our land. He marched out, with all the forces he could gather. The Vlandians betrayed us, but that's what you expect from honorless barbarians. Fortune favored the enemy. What matters is that we did what honor required.[if:convo_thinking]", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=CRmUZvaW}Perhaps Neretzes was rash, sending our infantry up into the hills to storm the Battanian fort. But he thought he could grab the pass quickly, before the enemy had time to reinforce it. If he had made the other wager and that turned out to be wrong, people would say he was hesitant.[ib:demure2]", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=wajioDaK}I stayed with Neretzes until we were forced back to our camp by the Sturgian infantry, and then fought on the battlements. Eventually we could hold them no longer. I did not see what happened to Neretzes or to our banner. Arenicos got us out of there, and got us home. I did not respect Arenicos before, but that day I saw he was worthy to be Emperor.", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_1_5")
				{
					answer = new TextObject("{=HL9lzfjY}Of course I can. You know my name, whose son I am.[if:convo_beaten][ib:closed]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=iQrwsvb1}We had no choice but to go to war. Anyone who tells you they would have done otherwise is either a liar or a coward or both. The Sturgians attacked us, and needed to be chastised. We lost an army and a banner. But we did not lose our honor, and without honor the Empire would be finished.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=aYozMy3Z}We lost because the Vlandians broke their oaths, and fought us when they should have fought with us. I was given command of the cataphracts, and we easily crushed their crossbowmen. Their knights gave us more trouble. Meanwhile, the Sturgian infantry came down and attacked our main force. That's where my father fell. ", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=cYibaPhn}The barbarians just kept coming and coming. I fought my way out with some loyal men, and made my way back to the capital. But I found that Arenicos had got there before me, and had himself declared Emperor. He always was a cunning operator.", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_1_7")
				{
					answer = new TextObject("{=kJwV17Jx}Yes. We will never forget that day... The day we learned that the old men who claimed they had the right to rule us were doddering incompetents.[if:convo_stern][ib:closed]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=Os79KwFa}I was with the vanguard. Neretzes apparently knew that the Battanians had planned an ambush - the Khuzait scouts had told him. But he never bothered to inform us. So up we went, along a lovely wooded stream, until the Battanian arrows started whooshing in from all sides.[if:convo_bored2][ib:normal2]", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=AGIPUYvy}We had our shields but you can only point them in one direction at once. So we started to drop, one by one, until the Battanian falxmen came screaming out of the trees. Ordinarily they'd be very vulnerable to archers, but, well, old Neretzes hadn't thought to send any along with us. So they came upon us, chopping and slashing, and we fought until we broke.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=4IS4Lou7}I ran too. And any man who tells you he wouldn't, in those circumstances, is a liar. When I was sitting in the cold woods later that night, hiding with other fugitives, listening to the barbarians whoop and holler as they chopped off heads as trophies - I promise them that no Calradian soldier should again be led into battle by an emperor who knows so little of war.[if:convo_thinking][ib:hip]", (Dictionary<string, object>)null);
				}
				else if (((MBObjectBase)item.Key).StringId == "lord_1_14")
				{
					answer = new TextObject("{=sn3Eabme}Of course. I did not witness the battle, but my husband Arenicos spoke frequently of it.[if:convo_calm_friendly]", (Dictionary<string, object>)null);
					answer2 = new TextObject("{=AhWyBEhd}He was one of the emperor's trusted commanders. He could not stop Neretzes from marching to defeat, but he managed to salvage something from the disaster. When the Sturgians came over our barricades, he managed to lead a group of Neretzes' guardsmen out the back.", (Dictionary<string, object>)null);
					answer3 = new TextObject("{=kdR3CeL8}My husband's small force held together, and were joined by stragglers and fugitives. He described the march back - no food, little water, marching day and night to keep ahead of the enemy's outriders. But they survived - the only organized imperial force to do so.", (Dictionary<string, object>)null);
					answer4 = new TextObject("{=9aYCfBVz}The city was in a state of panic after hearing rumors of what happened. Arenicos kept things from descending into chaos. When it came time for the Senate to choose the next Emperor, there was no question that it should be him. I loved him before as a man, but that day learned to love him as something more: what a gift he was to the people of Calradia![ib:demure2]", (Dictionary<string, object>)null);
				}
				else
				{
					Debug.FailedAssert("Unable to set notable dialogue!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Quests\\FirstPhase\\BannerInvestigationQuest.cs", "SetNobleDialogs", 579);
					answer = null;
					answer2 = null;
					answer3 = null;
					answer4 = null;
				}
				CreateNobleDialog(item.Key, answer, answer2, answer3, answer4);
			}
		}
	}

	private void CreateNobleDialog(Hero noble, TextObject answer1, TextObject answer2, TextObject answer3, TextObject answer4)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		DialogFlow val = DialogFlow.CreateDialogFlow("hero_main_options", 150).BeginPlayerOptions((string)null, false).PlayerSpecialOption(new TextObject("{=6UIa4784}Can you tell me anything about the battle of Pendraic?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => talk_with_quest_noble_condition() && Hero.OneToOneConversationHero == noble))
			.NpcLine(answer1, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(answer2, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(answer3, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(answer4, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		if (_noblesToTalk.Count((KeyValuePair<Hero, bool> kvp) => kvp.Value) >= 9)
		{
			val.NpcLine(new TextObject("{=p4qJ4KSm}If you want more information, there are two people you might try to speak to. {IMPERIAL_MENTOR.NAME} worked as a sort of unofficial spymaster for Neretzes. She lives near {IMPERIAL_MENTOR_SETTLEMENT}.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
			val.Condition(new OnConditionDelegate(talk_about_mentors_condition));
			val.NpcLine(new TextObject("{=q80FFaBj}Then there is {ANTI_IMPERIAL_MENTOR.NAME}, who was his bodyguard. He's supposed to be near {ANTI_IMPERIAL_MENTOR_SETTLEMENT} - though I hear he's changed quite a bit since then.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		}
		val.PlayerLine(new TextObject("{=ShG19Xhi}Thank you...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		val.Consequence(new OnConsequenceDelegate(talk_with_quest_noble_consequence));
		val.EndPlayerOptions();
		val.GotoDialogState("lord_pretalk");
		Campaign.Current.ConversationManager.AddDialogFlow(val, (object)this);
	}

	private bool talk_about_mentors_condition()
	{
		StringHelpers.SetCharacterProperties("IMPERIAL_MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, (TextObject)null, false);
		StringHelpers.SetCharacterProperties("ANTI_IMPERIAL_MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, (TextObject)null, false);
		MBTextManager.SetTextVariable("IMPERIAL_MENTOR_SETTLEMENT", StoryModeManager.Current.MainStoryLine.ImperialMentorSettlement.EncyclopediaLinkWithName, false);
		MBTextManager.SetTextVariable("ANTI_IMPERIAL_MENTOR_SETTLEMENT", StoryModeManager.Current.MainStoryLine.AntiImperialMentorSettlement.EncyclopediaLinkWithName, false);
		return true;
	}

	private bool talk_with_quest_noble_condition()
	{
		if (Hero.OneToOneConversationHero != null && _battleSummarized && _noblesToTalk.ContainsKey(Hero.OneToOneConversationHero))
		{
			return !_noblesToTalk[Hero.OneToOneConversationHero];
		}
		return false;
	}

	private void talk_with_quest_noble_consequence()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		_noblesToTalk[Hero.OneToOneConversationHero] = true;
		((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)Hero.OneToOneConversationHero);
		if (Hero.OneToOneConversationHero.PartyBelongedTo != null)
		{
			((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)Hero.OneToOneConversationHero.PartyBelongedTo);
		}
		_talkedNotablesQuestLog.UpdateCurrentProgress(_noblesToTalk.Count((KeyValuePair<Hero, bool> n) => n.Value));
		TextObject val = new TextObject("{=DQzEgUzu}You talked with {HERO.LINK} and got some valuable information that may help you understand the artifact.", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("HERO", CharacterObject.OneToOneConversationCharacter, val, false);
		((QuestBase)this).AddLog(val, false);
		if (_talkedNotablesQuestLog.CurrentProgress == _talkedNotablesQuestLog.Range)
		{
			((QuestBase)this).CompleteQuestWithSuccess();
		}
		else
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += UpdateAllNoblesDead;
		}
	}

	private void OnPartyRemoved(PartyBase party)
	{
		if (party.IsMobile && ((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)party.MobileParty))
		{
			((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)party.MobileParty);
		}
	}

	private void OnPartySpawned(MobileParty spawnedParty)
	{
		if (spawnedParty.IsLordParty && spawnedParty.LeaderHero != null && _noblesToTalk.ContainsKey(spawnedParty.LeaderHero) && !_noblesToTalk[spawnedParty.LeaderHero])
		{
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)spawnedParty);
		}
	}

	protected override void OnCompleteWithSuccess()
	{
		_noblesToTalk.Clear();
		((QuestBase)this).AddLog(_endQuestLog, false);
	}

	internal static void AutoGeneratedStaticCollectObjectsBannerInvestigationQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(BannerInvestigationQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_noblesToTalk);
		collectedObjects.Add(_talkedNotablesQuestLog);
	}

	internal static object AutoGeneratedGetMemberValue_noblesToTalk(object o)
	{
		return ((BannerInvestigationQuest)o)._noblesToTalk;
	}

	internal static object AutoGeneratedGetMemberValue_allNoblesDead(object o)
	{
		return ((BannerInvestigationQuest)o)._allNoblesDead;
	}

	internal static object AutoGeneratedGetMemberValue_battleSummarized(object o)
	{
		return ((BannerInvestigationQuest)o)._battleSummarized;
	}

	internal static object AutoGeneratedGetMemberValue_talkedNotablesQuestLog(object o)
	{
		return ((BannerInvestigationQuest)o)._talkedNotablesQuestLog;
	}
}
