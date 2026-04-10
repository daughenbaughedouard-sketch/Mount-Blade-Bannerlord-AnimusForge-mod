using System;
using System.Collections.Generic;
using System.Linq;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.TutorialPhase;

public class TravelToVillageTutorialQuest : StoryModeQuestBase
{
	private const int RefugePartyCount = 4;

	[SaveableField(1)]
	private Settlement _questVillage;

	[SaveableField(2)]
	private readonly MobileParty[] _refugeeParties;

	private TextObject _startQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=bNqLQKQS}You are out of food. There is a village called {VILLAGE_NAME} north of here where you can buy provisions and find some help.", (Dictionary<string, object>)null);
			val.SetTextVariable("VILLAGE_NAME", _questVillage.Name);
			return val;
		}
	}

	private TextObject _endQuestLog => new TextObject("{=7VFLb3Qj}You have arrived at the village.", (Dictionary<string, object>)null);

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=oa4XFhve}Travel to Village {VILLAGE_NAME}", (Dictionary<string, object>)null);
			val.SetTextVariable("VILLAGE_NAME", _questVillage.Name);
			return val;
		}
	}

	public TravelToVillageTutorialQuest()
		: base("travel_to_village_tutorial_quest", null, CampaignTime.Never)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_0096: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		_questVillage = Settlement.Find("village_ES3_2");
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_questVillage);
		Hero val = ((IEnumerable<Hero>)_questVillage.Notables).First((Hero x) => x.IsHeadman);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)val);
		_refugeeParties = (MobileParty[])(object)new MobileParty[4];
		TextObject val2 = new TextObject("{=3YHL3wpM}{BROTHER.NAME}:", (Dictionary<string, object>)null);
		TextObjectExtensions.SetCharacterProperties(val2, "BROTHER", StoryModeHeroes.ElderBrother.CharacterObject, false);
		InformationManager.ShowInquiry(new InquiryData(((object)val2).ToString(), ((object)new TextObject("{=dE2ufxte}Before we do anything else... We're low on food. There's a village north of here where we can buy provisions and find some help. You're a better rider than I am so I'll let you lead the way...", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=JOJ09cLW}Let's go.", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)delegate
		{
			StoryModeEvents.Instance.OnTravelToVillageTutorialQuestStarted();
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		((QuestBase)this).SetDialogs();
		((QuestBase)this).InitializeQuestOnCreation();
		((QuestBase)this).AddLog(_startQuestLog, false);
		StoryMode.StoryModePhases.TutorialPhase.Instance.SetTutorialFocusSettlement(_questVillage);
		CreateRefugeeParties();
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
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=MDtTC5j5}Don't hurt us![ib:nervous][if:convo_nervous]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(news_about_raiders_condition))
			.Consequence(new OnConsequenceDelegate(news_about_raiders_consequence))
			.PlayerLine(new TextObject("{=pX5cx3b4}I mean you no harm. We're hunting a group of raiders who took our brother and sister.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=ajBBFq1D}Aii... Those devils. They raided our village. Took whoever they could catch. Slavers, I'll bet.[if:convo_nervous][ib:nervous2]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=AhthUkMu}People say they're still about. We're sleeping in the woods, not going back until they're gone. You hunt them down and kill every one, you hear! Heaven protect you! Heaven guide your swords![if:convo_nervous2][ib:nervous]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000020).NpcLine(new TextObject("{=pa9LrHln}We're here, I guess. So... We need food, and after that, maybe some men to come with us.[if:convo_thinking]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Settlement.CurrentSettlement != null && Settlement.CurrentSettlement == _questVillage && Hero.OneToOneConversationHero == StoryModeHeroes.ElderBrother))
			.NpcLine(new TextObject("{=p0fmZY5r}The headman here can probably help us. Let's try to find him...[if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(talk_with_brother_consequence))
			.CloseDialog(), (object)this);
	}

	private bool news_about_raiders_condition()
	{
		if (Settlement.CurrentSettlement == null && MobileParty.ConversationParty != null)
		{
			return _refugeeParties.Contains(MobileParty.ConversationParty);
		}
		return false;
	}

	private void news_about_raiders_consequence()
	{
		PlayerEncounter.LeaveEncounter = true;
	}

	private void talk_with_brother_consequence()
	{
		Campaign.Current.ConversationManager.ConversationEndOneShot += ((QuestBase)this).CompleteQuestWithSuccess;
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener((object)this, (Action)OnBeforeMissionOpened);
		StoryModeEvents.OnTravelToVillageTutorialQuestStartedEvent.AddNonSerializedListener((object)this, (Action)OnTravelToVillageTutorialQuestStarted);
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		if (!StoryMode.StoryModePhases.TutorialPhase.Instance.IsCompleted && Settlement.CurrentSettlement == null && PlayerEncounter.EncounteredParty != null && args.MenuContext.GameMenu.StringId != "encounter_meeting" && args.MenuContext.GameMenu.StringId != "encounter")
		{
			if (_refugeeParties.Contains(PlayerEncounter.EncounteredMobileParty))
			{
				GameMenu.SwitchToMenu("encounter_meeting");
				return;
			}
			PlayerEncounter.Finish(true);
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=EWD4Op6d}Notification", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=pVKkclVk}Interactions are limited during tutorial phase. This interaction is disabled.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
	}

	private void OnBeforeMissionOpened()
	{
		if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement == Settlement.Find("village_ES3_2"))
		{
			int hitPoints = StoryModeHeroes.ElderBrother.HitPoints;
			int num = 50;
			if (hitPoints < num)
			{
				int num2 = num - hitPoints;
				StoryModeHeroes.ElderBrother.Heal(num2, false);
			}
			LocationCharacter locationCharacterOfHero = LocationComplex.Current.GetLocationCharacterOfHero(StoryModeHeroes.ElderBrother);
			PlayerEncounter.LocationEncounter.AddAccompanyingCharacter(locationCharacterOfHero, true);
		}
	}

	protected override void DailyTick()
	{
		for (int i = 0; i < _refugeeParties.Length; i++)
		{
			if (_refugeeParties[i].Party.IsStarving)
			{
				_refugeeParties[i].Party.ItemRoster.AddToCounts(DefaultItems.Grain, 2);
			}
		}
	}

	private void OnTravelToVillageTutorialQuestStarted()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		GameState activeState = GameStateManager.Current.ActiveState;
		MapState val;
		if ((val = (MapState)(object)((activeState is MapState) ? activeState : null)) != null)
		{
			val.Handler.StartCameraAnimation(_questVillage.GatePosition, 1f);
		}
	}

	private void CreateRefugeeParties()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			MobileParty val = CustomPartyComponent.CreateCustomPartyWithTroopRoster(_questVillage.GatePosition, MobileParty.MainParty.SeeingRange, _questVillage, new TextObject("{=7FWF01bW}Refugees", (Dictionary<string, object>)null), (Clan)null, TroopRoster.CreateDummyTroopRoster(), TroopRoster.CreateDummyTroopRoster(), _questVillage.OwnerClan.Leader, "", "", 0f, true);
			val.InitializePartyTrade(200);
			SetPartyAiAction.GetActionForPatrollingAroundSettlement(val, _questVillage, (NavigationType)1, false, false);
			val.Ai.SetDoNotMakeNewDecisions(true);
			val.IgnoreByOtherPartiesTill(CampaignTime.Never);
			val.SetPartyUsedByQuest(true);
			val.Party.ItemRoster.AddToCounts(DefaultItems.Grain, 2);
			CharacterObject val2 = MBObjectManager.Instance.GetObject<CharacterObject>("storymode_quest_refugee_female");
			CharacterObject val3 = MBObjectManager.Instance.GetObject<CharacterObject>("storymode_quest_refugee_male");
			int num = MBRandom.RandomInt(6, 12);
			for (int j = 0; j < num; j++)
			{
				val.MemberRoster.AddToCounts((MBRandom.RandomFloat < 0.5f) ? val2 : val3, 1, false, 0, 0, true, -1);
			}
			_refugeeParties[i] = val;
		}
	}

	protected override void OnCompleteWithSuccess()
	{
		foreach (MobileParty item in _refugeeParties.ToList())
		{
			DestroyPartyAction.Apply((PartyBase)null, item);
		}
		((QuestBase)this).AddLog(_endQuestLog, false);
		StoryMode.StoryModePhases.TutorialPhase.Instance.RemoveTutorialFocusSettlement();
	}

	internal static void AutoGeneratedStaticCollectObjectsTravelToVillageTutorialQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(TravelToVillageTutorialQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_questVillage);
		collectedObjects.Add(_refugeeParties);
	}

	internal static object AutoGeneratedGetMemberValue_questVillage(object o)
	{
		return ((TravelToVillageTutorialQuest)o)._questVillage;
	}

	internal static object AutoGeneratedGetMemberValue_refugeeParties(object o)
	{
		return ((TravelToVillageTutorialQuest)o)._refugeeParties;
	}
}
