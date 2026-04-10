using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.TutorialPhase;

public class LocateAndRescueTravellerTutorialQuest : StoryModeQuestBase
{
	private const int MainPartyHealHitPointLimit = 50;

	private const int PlayerPartySizeMinLimitToSpawnRaiders = 4;

	private const int RaiderPartySize = 6;

	private const int RaiderPartyCount = 3;

	private const string RaiderPartyStringId = "locate_and_rescue_traveller_quest_raider_party_";

	[SaveableField(1)]
	private int _raiderPartyCount;

	[SaveableField(2)]
	private readonly List<MobileParty> _raiderParties;

	[SaveableField(3)]
	private int _defeatedRaiderPartyCount;

	[SaveableField(4)]
	private readonly JournalLog _startQuestLog;

	private TextObject _startQuestLogText => new TextObject("{=JJo0i8an}Look around the village to find the party that captured the traveller whom the headman told you about.", (Dictionary<string, object>)null);

	public override TextObject Title => new TextObject("{=ACyYhA2s}Locate and Rescue Traveller", (Dictionary<string, object>)null);

	public LocateAndRescueTravellerTutorialQuest()
		: base("locate_and_rescue_traveler_tutorial_quest", null, CampaignTime.Never)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		_raiderParties = new List<MobileParty>();
		_defeatedRaiderPartyCount = 0;
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		((QuestBase)this).InitializeQuestOnCreation();
		_raiderPartyCount = 0;
		_startQuestLog = ((QuestBase)this).AddDiscreteLog(_startQuestLogText, new TextObject("{=UkNUuyr1}Defeated Parties", (Dictionary<string, object>)null), _defeatedRaiderPartyCount, 3, (TextObject)null, false);
		if (MobileParty.MainParty.MemberRoster.TotalManCount >= 4)
		{
			SpawnRaiderParties();
		}
		StoryMode.StoryModePhases.TutorialPhase.Instance.SetTutorialFocusSettlement(Settlement.Find("village_ES3_2"));
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
		AddGameMenus();
	}

	private MobileParty CreateRaiderParty()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		Settlement settlement = ((SettlementComponent)SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement x) => x.IsActive))).Settlement;
		Settlement val = MBObjectManager.Instance.GetObject<Settlement>("village_ES3_2");
		CampaignVec2 val2 = NavigationHelper.FindReachablePointAroundPosition(val.GatePosition, (NavigationType)1, MobileParty.MainParty.SeeingRange * 0.75f, 1f, false);
		MobileParty val3 = BanditPartyComponent.CreateBanditParty("locate_and_rescue_traveller_quest_raider_party_" + _raiderPartyCount, settlement.OwnerClan, settlement.Hideout, false, (PartyTemplateObject)null, val2);
		CharacterObject val4 = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("storymode_quest_raider");
		val3.MemberRoster.AddToCounts(val4, 6, false, 0, 0, true, -1);
		CharacterObject val5 = MBObjectManager.Instance.GetObject<CharacterObject>("tutorial_placeholder_volunteer");
		val3.PrisonRoster.AddToCounts(val5, (MBRandom.RandomFloat >= 0.5f) ? 1 : 2, false, 0, 0, true, -1);
		val3.Party.SetCustomName(new TextObject("{=u1Pkt4HC}Raiders", (Dictionary<string, object>)null));
		val3.InitializePartyTrade(200);
		val3.ActualClan = settlement.OwnerClan;
		SetPartyAiAction.GetActionForPatrollingAroundSettlement(val3, val, (NavigationType)1, false, false);
		val3.Ai.SetDoNotMakeNewDecisions(true);
		val3.IgnoreByOtherPartiesTill(CampaignTime.Never);
		val3.Party.SetVisualAsDirty();
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)val3);
		val3.IsActive = true;
		_raiderPartyCount++;
		val3.SetPartyUsedByQuest(true);
		return val3;
	}

	private void DespawnRaiderParties()
	{
		if (Extensions.IsEmpty<MobileParty>((IEnumerable<MobileParty>)_raiderParties))
		{
			return;
		}
		foreach (MobileParty item in _raiderParties.ToList())
		{
			((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)item);
			DestroyPartyAction.Apply((PartyBase)null, item);
		}
		_raiderParties.Clear();
	}

	private void SpawnRaiderParties()
	{
		if (Extensions.IsEmpty<MobileParty>((IEnumerable<MobileParty>)_raiderParties))
		{
			for (int i = _defeatedRaiderPartyCount; i < 3; i++)
			{
				_raiderParties.Add(CreateRaiderParty());
			}
		}
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
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=BdYaRvhm}I don't know who you are, but I'm in your debt. These brigands would've marched us to our deaths.[ib:nervous2][if:convo_uncomfortable_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(meeting_tacitus_on_condition))
			.NpcLine(new TextObject("{=9VxUSDQ7}My name's Tacteos. I'm a doctor by trade. I was on, well, a bit of a quest, but now I'm thinking I'm not really made for this kind of thing.[ib:nervous][if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=5LJTeOBT}I was with a caravan and they just came out of the brush. We were surrounded and outnumbered, so we gave up. I figured they'd keep us alive, if just for the ransom. But then they started flogging us along at top speed, without any water, and I was just about ready to drop.[ib:nervous2]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=XdDQdSsW}I could feel the signs of heat-stroke creeping up and I told them but they just flogged me more... If your group hadn't come along... Maybe I have a way to thank you properly.[ib:normal][if:convo_thinking]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=bkZFbCRx}We're looking for two children captured by the raiders. Can you tell us anything?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=ehnbi5yD}I am afraid I haven't seen any children. But after our caravan was attacked, the chief of the raiders, the one they call Radagos, took and rode off with our more valuable belongings, including a chest that I had.[ib:closed][if:convo_empathic_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=RF3NoR3d}He seemed to be controlling more than one band raiding around this area. If this lot has your kin, then I think he'd be the one to know.[if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=K75sH3vW}And since I have nothing of value left to repay your help, I'll tell you this. If you do catch up with and defeat that ruffian, you may be able to recover my chest. It contains a valuable ornament which I was told could be of great value, if you knew where to sell it.[if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=8GCW5IRO}I was trying to find out more about it, but, as I say, I've had all my urge for travelling flogged out of me. Right now I don't think I'd venture more than 20 paces from a well as long as I live.[ib:closed2][if:convo_shocked]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=Zyn5FrTR}We'll keep that in mind.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=vJyTsFdU}It doesn't look like much and I suspect this lot would give it away for a few coins, but I got it from a mercenary whom I treated once, and swore it was related to 'Neretzes's Folly'. I don't know what that means, except that Neretzes was, of course, the emperor who died in battle some years back. Maybe you can find out its true value.[if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=tsjQtWsO}Thanks for saving me again. I hope our paths will cross again![ib:normal2][if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(meeting_tacitus_on_consequence))
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=!}Start encounter.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(meeting_with_raider_party_on_condition))
			.CloseDialog(), (object)this);
	}

	private bool meeting_tacitus_on_condition()
	{
		if (Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == StoryModeHeroes.Tacitus)
		{
			return !Hero.OneToOneConversationHero.HasMet;
		}
		return false;
	}

	private void meeting_tacitus_on_consequence()
	{
		foreach (MobileParty raiderParty in _raiderParties)
		{
			if (raiderParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, raiderParty);
			}
		}
		DisableHeroAction.Apply(StoryModeHeroes.Tacitus);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private bool meeting_with_raider_party_on_condition()
	{
		return _raiderParties.Any((MobileParty p) => ConversationHelper.GetConversationCharacterPartyLeader(p.Party) == CharacterObject.OneToOneConversationCharacter);
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Invalid comparison between Unknown and I4
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I4
		if (Settlement.CurrentSettlement == null && PlayerEncounter.EncounteredMobileParty != null && _raiderParties.Any((MobileParty p) => p == PlayerEncounter.EncounteredMobileParty) && args.MenuContext.GameMenu.StringId != "encounter_meeting" && args.MenuContext.GameMenu.StringId != "encounter" && args.MenuContext.GameMenu.StringId != "encounter_raiders_quest")
		{
			GameMenu.SwitchToMenu("encounter_raiders_quest");
		}
		if (Hero.MainHero.HitPoints < 50)
		{
			Hero.MainHero.Heal(50 - Hero.MainHero.HitPoints, false);
		}
		Hero elderBrother = StoryModeHeroes.ElderBrother;
		if (elderBrother.HitPoints < 50)
		{
			elderBrother.Heal(50 - elderBrother.HitPoints, false);
		}
		if (!Hero.MainHero.IsPrisoner)
		{
			return;
		}
		EndCaptivityAction.ApplyByPeace(Hero.MainHero, (Hero)null);
		if (elderBrother.IsPrisoner)
		{
			EndCaptivityAction.ApplyByPeace(elderBrother, (Hero)null);
		}
		if (elderBrother.PartyBelongedTo != MobileParty.MainParty)
		{
			if ((int)elderBrother.HeroState == 2 || (int)elderBrother.HeroState == 4)
			{
				elderBrother.ChangeState((CharacterStates)1);
			}
			AddHeroToPartyAction.Apply(elderBrother, MobileParty.MainParty, false);
		}
		DisableHeroAction.Apply(StoryModeHeroes.Tacitus);
		TextObject val = new TextObject("{=ORnjaMlM}You were defeated by the raiders, but your brother saved you. It doesn't look like they're going anywhere, though, so you should attack again once you're ready.{newline}You must have at least {NUMBER} members in your party. If you don't, go back to the village and recruit some more troops.", (Dictionary<string, object>)null);
		val.SetTextVariable("NUMBER", 4);
		InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=FPhWhjq7}Defeated", (Dictionary<string, object>)null)).ToString(), ((object)val).ToString(), true, false, ((object)new TextObject("{=lmG7uRK2}Okay", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)delegate
		{
			PartyBase mainParty = PartyBase.MainParty;
			if (mainParty != null && mainParty.MemberRoster.TotalManCount >= 4)
			{
				SpawnRaiderParties();
			}
			else
			{
				Campaign current = Campaign.Current;
				if (current != null)
				{
					current.VisualTrackerManager.RegisterObject((ITrackableCampaignObject)(object)MBObjectManager.Instance.GetObject<Settlement>("village_ES3_2"));
				}
			}
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		DespawnRaiderParties();
	}

	private void AddGameMenus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0024: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0059: Expected O, but got Unknown
		//IL_0059: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_0083: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Expected O, but got Unknown
		((QuestBase)this).AddGameMenu("encounter_raiders_quest", new TextObject("{=mU1bC1mp}You encountered the raider party.", (Dictionary<string, object>)null), new OnInitDelegate(game_menu_encounter_on_init), (MenuOverlayType)4, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("encounter_raiders_quest", "encounter_raiders_quest_attack", new TextObject("{=1r0tDsrR}Attack!", (Dictionary<string, object>)null), new OnConditionDelegate(game_menu_encounter_attack_on_condition), new OnConsequenceDelegate(game_menu_encounter_attack_on_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("encounter_raiders_quest", "encounter_raiders_quest_send_troops", new TextObject("{=z3VamNrX}Send in your troops.", (Dictionary<string, object>)null), new OnConditionDelegate(game_menu_encounter_send_troops_on_condition), (OnConsequenceDelegate)null, false, -1);
		((QuestBase)this).AddGameMenuOption("encounter_raiders_quest", "encounter_raiders_quest_leave", new TextObject("{=2YYRyrOO}Leave...", (Dictionary<string, object>)null), new OnConditionDelegate(game_menu_encounter_leave_on_condition), new OnConsequenceDelegate(game_menu_encounter_leave_on_consequence), true, -1);
	}

	private void game_menu_encounter_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Battle == null)
		{
			PlayerEncounter.StartBattle();
		}
		PlayerEncounter.Update();
	}

	private bool game_menu_encounter_leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void game_menu_encounter_leave_on_consequence(MenuCallbackArgs args)
	{
		MenuHelper.EncounterLeaveConsequence();
	}

	private bool game_menu_encounter_attack_on_condition(MenuCallbackArgs args)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if (PartyBase.MainParty.MemberRoster.TotalManCount < 4)
		{
			args.IsEnabled = false;
			args.Tooltip = new TextObject("{=DyE3luNM}You need to have at least {NUMBER} member in your party to deal with the raider party. Go back to village to recruit more troops.", (Dictionary<string, object>)null);
			args.Tooltip.SetTextVariable("NUMBER", 4);
		}
		return MenuHelper.EncounterAttackCondition(args);
	}

	internal void game_menu_encounter_attack_on_consequence(MenuCallbackArgs args)
	{
		MenuHelper.EncounterAttackConsequence(args);
	}

	private bool game_menu_encounter_send_troops_on_condition(MenuCallbackArgs args)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		args.IsEnabled = false;
		args.Tooltip = new TextObject("{=hnFkhPhp}This option is disabled during tutorial stage.", (Dictionary<string, object>)null);
		args.optionLeaveType = (LeaveType)10;
		return true;
	}

	[GameMenuInitializationHandler("encounter_raiders_quest")]
	private static void game_menu_encounter_on_init_background(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_looter");
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (party == MobileParty.MainParty)
		{
			if (4 > MobileParty.MainParty.MemberRoster.TotalManCount)
			{
				DespawnRaiderParties();
				OpenRecruitMoreTroopsPopUp();
			}
			else
			{
				SpawnRaiderParties();
			}
		}
	}

	private void OpenRecruitMoreTroopsPopUp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=y3fn2vWY}Recruit Troops", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=taOCFKtZ}You need to recruit more troops to deal with the raider party. Go back to village to recruit more troops.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void OnMapEventEnded(MapEvent mapEvent)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
		if (!mapEvent.IsPlayerMapEvent)
		{
			return;
		}
		if (mapEvent.PlayerSide == mapEvent.WinningSide)
		{
			foreach (MobileParty party in _raiderParties.ToList())
			{
				if (mapEvent.InvolvedParties.Any((PartyBase p) => p == party.Party))
				{
					_defeatedRaiderPartyCount++;
					_startQuestLog.UpdateCurrentProgress(_defeatedRaiderPartyCount);
					party.MemberRoster.Clear();
					if (_raiderParties.Count > 1)
					{
						_raiderParties.Remove(party);
					}
				}
				if (party.MemberRoster.TotalManCount == 0 && _raiderParties.Count > 1)
				{
					_raiderParties.Remove(party);
				}
			}
			if (_defeatedRaiderPartyCount >= 3)
			{
				MobileParty obj = _raiderParties[0];
				Hero tacitus = StoryModeHeroes.Tacitus;
				TakePrisonerAction.Apply(obj.Party, tacitus);
				obj.PrisonRoster.AddToCounts(((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("villager_empire"), 2, false, 0, 0, true, -1);
				InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=EWD4Op6d}Notification", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=OMrnTIe0}You rescue several prisoners that the raiders had been dragging along. They look parched and exhausted. You give them a bit of water and bread, and after a short while one staggers to his feet and comes over to you.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=lmG7uRK2}Okay", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)delegate
				{
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, true, false, false, false, false), new ConversationCharacterData(StoryModeHeroes.Tacitus.CharacterObject, (PartyBase)null, true, true, false, false, false, false));
				}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
		}
		if (4 > MobileParty.MainParty.MemberRoster.TotalManCount)
		{
			DespawnRaiderParties();
			OpenRecruitMoreTroopsPopUp();
		}
	}

	protected override void HourlyTick()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (4 > MobileParty.MainParty.MemberRoster.TotalManCount)
		{
			CampaignTime campaignStartTime = Campaign.Current.Models.CampaignTimeModel.CampaignStartTime;
			if (MathF.Floor(((CampaignTime)(ref campaignStartTime)).ElapsedHoursUntilNow) % 12 == 0)
			{
				DespawnRaiderParties();
				OpenRecruitMoreTroopsPopUp();
				Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
			}
		}
	}

	private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
	{
		if (_raiderParties.Contains(mobileParty))
		{
			_raiderParties.Remove(mobileParty);
		}
	}

	protected override void OnCompleteWithSuccess()
	{
		StoryMode.StoryModePhases.TutorialPhase.Instance.RemoveTutorialFocusSettlement();
		StoryMode.StoryModePhases.TutorialPhase.Instance.RemoveTutorialFocusMobileParty();
	}

	internal static void AutoGeneratedStaticCollectObjectsLocateAndRescueTravellerTutorialQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(LocateAndRescueTravellerTutorialQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_raiderParties);
		collectedObjects.Add(_startQuestLog);
	}

	internal static object AutoGeneratedGetMemberValue_raiderPartyCount(object o)
	{
		return ((LocateAndRescueTravellerTutorialQuest)o)._raiderPartyCount;
	}

	internal static object AutoGeneratedGetMemberValue_raiderParties(object o)
	{
		return ((LocateAndRescueTravellerTutorialQuest)o)._raiderParties;
	}

	internal static object AutoGeneratedGetMemberValue_defeatedRaiderPartyCount(object o)
	{
		return ((LocateAndRescueTravellerTutorialQuest)o)._defeatedRaiderPartyCount;
	}

	internal static object AutoGeneratedGetMemberValue_startQuestLog(object o)
	{
		return ((LocateAndRescueTravellerTutorialQuest)o)._startQuestLog;
	}
}
