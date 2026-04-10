using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using StoryMode.Quests.TutorialPhase;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.ActivitySystem;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class TutorialPhaseCampaignBehavior : CampaignBehaviorBase
{
	private bool _controlledByBrother;

	private bool _notifyPlayerAboutPosition;

	private Equipment[] _mainHeroEquipmentBackup = (Equipment[])(object)new Equipment[2];

	private Equipment[] _brotherEquipmentBackup = (Equipment[])(object)new Equipment[2];

	private float _distanceThresholdForQuestFocusTarget;

	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter, int>)OnNewGameCreatedPartialFollowUp);
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)Tick);
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnGameLoaded);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener((object)this, (Action)DailyTick);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
		CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener((object)this, (Action)OnCharacterCreationIsOver);
		CampaignEvents.CanHaveCampaignIssuesEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanHaveCampaignIssuesInfoIsRequested);
		CampaignEvents.CanHeroMarryEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanHeroMarry);
	}

	private void AddDialogAndGameMenus(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_007e: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_00f4: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0125: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		//IL_0156: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Expected O, but got Unknown
		//IL_01ad: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("storymode_conversation_blocker", "start", "close_window", "{=9XnFlRR0}Interaction with this person is disabled during tutorial stage.", new OnConditionDelegate(storymode_conversation_blocker_on_condition), (OnConsequenceDelegate)null, 1000000, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddGameMenu("storymode_game_menu_blocker", "{=pVKkclVk}Interactions are limited during tutorial phase. This interaction is disabled.", new OnInitDelegate(storymode_game_menu_blocker_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("storymode_game_menu_blocker", "game_menu_blocker_leave", "{=3sRdGQou}Leave", new OnConditionDelegate(game_menu_leave_condition), new OnConsequenceDelegate(game_menu_leave_on_consequence), true, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("storymode_tutorial_village_game_menu", "{=7VFLb3Qj}You have arrived at the village.", new OnInitDelegate(storymode_tutorial_village_game_menu_on_init), (MenuOverlayType)3, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("storymode_tutorial_village_game_menu", "storymode_tutorial_village_hostile_action", "{=GM3tAYMr}Take a hostile action", new OnConditionDelegate(raid_village_menu_option_condition), (OnConsequenceDelegate)null, false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("storymode_tutorial_village_game_menu", "storymode_tutorial_village_recruit", "{=E31IJyqs}Recruit troops", new OnConditionDelegate(recruit_troops_village_menu_option_condition), new OnConsequenceDelegate(storymode_recruit_volunteers_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("storymode_tutorial_village_game_menu", "storymode_tutorial_village_buy", "{=VN4ctHIU}Buy products", new OnConditionDelegate(buy_products_village_menu_option_condition), new OnConsequenceDelegate(storymode_ui_village_buy_good_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("storymode_tutorial_village_game_menu", "storymode_tutorial_village_enter", "{=Xrz05hYE}Take a walk around", new OnConditionDelegate(storymode_tutorial_village_enter_on_condition), new OnConsequenceDelegate(storymode_tutorial_village_enter_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("storymode_tutorial_village_game_menu", "storymode_tutorial_village_wait", "{=zEoHYEUS}Wait here for some time", new OnConditionDelegate(wait_village_menu_option_condition), (OnConsequenceDelegate)null, false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("storymode_tutorial_village_game_menu", "storymode_tutorial_village_leave", "{=3sRdGQou}Leave", new OnConditionDelegate(game_menu_leave_on_condition), new OnConsequenceDelegate(game_menu_leave_on_consequence), true, -1, false, (object)null);
	}

	private void InitializeTutorial()
	{
		Hero elderBrother = StoryModeHeroes.ElderBrother;
		elderBrother.ChangeState((CharacterStates)1);
		AddHeroToPartyAction.Apply(elderBrother, MobileParty.MainParty, false);
		elderBrother.SetHasMet();
		DisableHeroAction.Apply(StoryModeHeroes.Tacitus);
		DisableHeroAction.Apply(StoryModeHeroes.LittleBrother);
		DisableHeroAction.Apply(StoryModeHeroes.LittleSister);
		DisableHeroAction.Apply(StoryModeHeroes.Radagos);
		DisableHeroAction.Apply(StoryModeHeroes.ImperialMentor);
		DisableHeroAction.Apply(StoryModeHeroes.AntiImperialMentor);
		DisableHeroAction.Apply(StoryModeHeroes.RadagosHenchman);
		Settlement settlement = Settlement.Find("village_ES3_2");
		CreateHeadman(settlement);
		PartyBase.MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, 1);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<Equipment[]>("_mainHeroEquipmentBackup", ref _mainHeroEquipmentBackup);
		dataStore.SyncData<Equipment[]>("_brotherEquipmentBackup", ref _brotherEquipmentBackup);
	}

	private void Tick(float dt)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		if (TutorialPhase.Instance.TutorialFocusSettlement != null || TutorialPhase.Instance.TutorialFocusMobileParty != null)
		{
			float num = -1f;
			CampaignVec2 val = CampaignVec2.Invalid;
			CampaignVec2 val2;
			if (TutorialPhase.Instance.TutorialFocusSettlement != null)
			{
				val2 = TutorialPhase.Instance.TutorialFocusSettlement.GatePosition;
				num = ((CampaignVec2)(ref val2)).Distance(MobileParty.MainParty.Position);
				val = TutorialPhase.Instance.TutorialFocusSettlement.GatePosition;
			}
			else if (TutorialPhase.Instance.TutorialFocusMobileParty != null)
			{
				val2 = TutorialPhase.Instance.TutorialFocusMobileParty.Position;
				num = ((CampaignVec2)(ref val2)).Distance(MobileParty.MainParty.Position);
				val = TutorialPhase.Instance.TutorialFocusMobileParty.Position;
			}
			if (num > _distanceThresholdForQuestFocusTarget)
			{
				_controlledByBrother = true;
				MobileParty.MainParty.SetMoveGoToPoint(val, (NavigationType)1);
			}
			if (_controlledByBrother && !_notifyPlayerAboutPosition)
			{
				_notifyPlayerAboutPosition = true;
				MBInformationManager.AddQuickInformation(new TextObject("{=hadftxlO}We have strayed too far from our path. I'll take the lead for some time. You follow me.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)(object)StoryModeHeroes.ElderBrother.CharacterObject, (Equipment)null, "");
				Campaign.Current.TimeControlMode = (CampaignTimeControlMode)3;
			}
			if (_controlledByBrother && num < MobileParty.MainParty.SeeingRange)
			{
				_controlledByBrother = false;
				_notifyPlayerAboutPosition = false;
				MobileParty.MainParty.SetMoveModeHold();
				MobileParty.MainParty.SetMoveGoToPoint(MobileParty.MainParty.Position, (NavigationType)1);
				MBInformationManager.AddQuickInformation(new TextObject("{=4vsvniPd}I think we are on the right path now. You are the better rider so you should take the lead.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)(object)StoryModeHeroes.ElderBrother.CharacterObject, (Equipment)null, "");
			}
		}
	}

	private void OnGameLoadFinished()
	{
		if (Settlement.CurrentSettlement != null && ((MBObjectBase)Settlement.CurrentSettlement).StringId == "village_ES3_2" && !TutorialPhase.Instance.IsCompleted)
		{
			SpawnYourBrotherInLocation(StoryModeHeroes.ElderBrother, "village_center");
		}
	}

	private void DailyTick()
	{
		Campaign.Current.IssueManager.ToggleAllIssueTracks(false);
		CheckIfMainPartyStarving();
	}

	private void CanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
	{
		Settlement val = Settlement.Find("village_ES3_2");
		if (!StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted && ((List<Hero>)(object)val.Notables).Contains(hero))
		{
			result = false;
		}
	}

	private void CanHeroMarry(Hero hero, ref bool result)
	{
		if (!TutorialPhase.Instance.IsCompleted && hero.Clan == Clan.PlayerClan)
		{
			result = false;
		}
	}

	private void OnCharacterCreationIsOver()
	{
		ActivityManager.SetActivityAvailability("CompleteMainQuest", true);
		ActivityManager.StartActivity("CompleteMainQuest");
		_mainHeroEquipmentBackup[0] = Hero.MainHero.BattleEquipment.Clone(false);
		_mainHeroEquipmentBackup[1] = Hero.MainHero.CivilianEquipment.Clone(false);
		_brotherEquipmentBackup[0] = StoryModeHeroes.ElderBrother.BattleEquipment.Clone(false);
		_brotherEquipmentBackup[1] = StoryModeHeroes.ElderBrother.CivilianEquipment.Clone(false);
		Settlement val = Settlement.Find("village_ES3_2");
		StoryModeHeroes.LittleBrother.UpdateLastKnownClosestSettlement(val);
		StoryModeHeroes.LittleSister.UpdateLastKnownClosestSettlement(val);
		StoryModeHeroes.MainHeroMother.UpdateLastKnownClosestSettlement(val);
		StoryModeHeroes.MainHeroFather.UpdateLastKnownClosestSettlement(val);
	}

	private void OnNewGameCreatedPartialFollowUp(CampaignGameStarter campaignGameStarter, int i)
	{
		if (i == 99)
		{
			PartyBase.MainParty.ItemRoster.Clear();
			_distanceThresholdForQuestFocusTarget = Campaign.Current.GetAverageDistanceBetweenClosestTwoTownsWithNavigationType((NavigationType)1) / 1.5f;
			AddDialogAndGameMenus(campaignGameStarter);
			InitializeTutorial();
		}
	}

	private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		_distanceThresholdForQuestFocusTarget = Campaign.Current.GetAverageDistanceBetweenClosestTwoTownsWithNavigationType((NavigationType)1) / 1.5f;
		AddDialogAndGameMenus(campaignGameStarter);
		Settlement val = Settlement.Find("village_ES3_2");
		if (Extensions.IsEmpty<Hero>((IEnumerable<Hero>)val.Notables))
		{
			CreateHeadman(val);
			return;
		}
		TutorialPhase.Instance.TutorialVillageHeadman = ((List<Hero>)(object)val.Notables)[0];
		if (!TutorialPhase.Instance.TutorialVillageHeadman.FirstName.Equals(new TextObject("{=Sb46O8WO}Orthos", (Dictionary<string, object>)null)))
		{
			TextObject val2 = new TextObject("{=JWLBKIkR}Headman {HEADMAN.FIRSTNAME}", (Dictionary<string, object>)null);
			TextObject val3 = new TextObject("{=Sb46O8WO}Orthos", (Dictionary<string, object>)null);
			TutorialPhase.Instance.TutorialVillageHeadman.SetName(val2, val3);
			StringHelpers.SetCharacterProperties("HEADMAN", TutorialPhase.Instance.TutorialVillageHeadman.CharacterObject, val2, false);
		}
	}

	public void FinalizeTutorialPhase()
	{
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Expected O, but got Unknown
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Expected O, but got Unknown
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Expected O, but got Unknown
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Expected O, but got Unknown
		Settlement val = Settlement.Find("village_ES3_2");
		if (((List<Hero>)(object)val.Notables).Count > 1)
		{
			Debug.FailedAssert("There are more than one notable in tutorial phase, control it.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\GameComponents\\CampaignBehaviors\\TutorialPhaseCampaignBehavior.cs", "FinalizeTutorialPhase", 265);
			foreach (Hero item in (List<Hero>)(object)val.Notables)
			{
				item.SetPersonalRelation(Hero.MainHero, 0);
			}
		}
		else
		{
			Hero obj = ((List<Hero>)(object)val.Notables)[0];
			obj.SetPersonalRelation(Hero.MainHero, 0);
			KillCharacterAction.ApplyByRemove(obj, false, true);
		}
		SpawnAllNotablesForVillage(val.Village);
		VolunteerModel volunteerModel = Campaign.Current.Models.VolunteerModel;
		foreach (Hero item2 in (List<Hero>)(object)val.Notables)
		{
			if (!item2.IsAlive || !volunteerModel.CanHaveRecruits(item2))
			{
				continue;
			}
			CharacterObject basicVolunteer = volunteerModel.GetBasicVolunteer(item2);
			for (int i = 0; i < item2.VolunteerTypes.Length; i++)
			{
				if (item2.VolunteerTypes[i] == null && MBRandom.RandomFloat < 0.5f)
				{
					item2.VolunteerTypes[i] = basicVolunteer;
				}
			}
		}
		DisableHeroAction.Apply(StoryModeHeroes.ElderBrother);
		StoryModeHeroes.ElderBrother.Clan = null;
		foreach (TroopRosterElement item3 in (List<TroopRosterElement>)(object)PartyBase.MainParty.MemberRoster.GetTroopRoster())
		{
			if (!((BasicCharacterObject)item3.Character).IsPlayerCharacter)
			{
				PartyBase.MainParty.MemberRoster.RemoveTroop(item3.Character, PartyBase.MainParty.MemberRoster.GetTroopCount(item3.Character), default(UniqueTroopDescriptor), 0);
			}
		}
		foreach (TroopRosterElement item4 in (List<TroopRosterElement>)(object)PartyBase.MainParty.PrisonRoster.GetTroopRoster())
		{
			if (((BasicCharacterObject)item4.Character).IsHero)
			{
				DisableHeroAction.Apply(item4.Character.HeroObject);
			}
			else
			{
				PartyBase.MainParty.PrisonRoster.RemoveTroop(item4.Character, PartyBase.MainParty.PrisonRoster.GetTroopCount(item4.Character), default(UniqueTroopDescriptor), 0);
			}
		}
		TutorialPhase.Instance.RemoveTutorialFocusSettlement();
		PartyBase.MainParty.ItemRoster.Clear();
		Hero.MainHero.BattleEquipment.FillFrom(_mainHeroEquipmentBackup[0], true);
		Hero.MainHero.CivilianEquipment.FillFrom(_mainHeroEquipmentBackup[1], true);
		StoryModeHeroes.ElderBrother.BattleEquipment.FillFrom(_brotherEquipmentBackup[0], true);
		StoryModeHeroes.ElderBrother.CivilianEquipment.FillFrom(_brotherEquipmentBackup[1], true);
		PartyBase.MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, 2);
		Hero.MainHero.Heal(Hero.MainHero.MaxHitPoints, false);
		Hero.MainHero.Gold = 1000;
		if (TutorialPhase.Instance.TutorialQuestPhase == TutorialQuestPhase.Finalized && !TutorialPhase.Instance.IsSkipped)
		{
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=EWD4Op6d}Notification", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=GCbqpeDs}Tutorial is over. You are now free to explore Calradia.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)delegate
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Expected O, but got Unknown
				MBInformationManager.ShowSceneNotification((SceneNotificationData)new FindingFirstBannerPieceSceneNotificationItem(Hero.MainHero, (Action)ShowStealthTutorialInquiry));
				((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
			}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
	}

	private void ShowStealthTutorialInquiry()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001c: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		TextObject val = new TextObject("{=DhMge68x}Stealth Tutorial", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=lVfxJkYb}You and your brother part ways. As he rides over the crest of a hill, he lifts his arm in salute, then disappears from view. A few days ago you were a family of six. Now, you are alone, and you realize that despite your courage and determination you and your brother may never see each other again.{newline}However, you are not left long in your solitude. As you make the final preparations to set out, a young boy whom you recognize from Tevea staggers into your camp. Once he regains his breath, he explains: a small band of Radagos's men escaped the showdown at his hideout, returned to the village, and seized the headman as a hostage. He begs you to come back with him and rescue their elder.", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)GameTexts.FindText("str_continue", (string)null)).ToString(), string.Empty, (Action)StartStealthTutorial, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void StartStealthTutorial()
	{
		((QuestBase)new VillagersInNeed()).StartQuest();
		StoryModeEvents.Instance.OnStealthTutorialActivated();
	}

	private void CreateHeadman(Settlement settlement)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Hero val = HeroCreator.CreateNotable((Occupation)20, settlement);
		TextObject val2 = new TextObject("{=JWLBKIkR}Headman {HEADMAN.FIRSTNAME}", (Dictionary<string, object>)null);
		TextObject val3 = new TextObject("{=Sb46O8WO}Orthos", (Dictionary<string, object>)null);
		val.SetName(val2, val3);
		StringHelpers.SetCharacterProperties("HEADMAN", val.CharacterObject, val2, false);
		val.AddPower((float)(Campaign.Current.Models.NotablePowerModel.NotableDisappearPowerLimit * 2));
		TutorialPhase.Instance.TutorialVillageHeadman = val;
	}

	private bool recruit_troops_village_menu_option_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)13;
		args.Tooltip = ((args.IsEnabled = TutorialPhase.Instance.TutorialQuestPhase >= TutorialQuestPhase.RecruitAndPurchaseStarted && (Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(RecruitTroopsTutorialQuest)) || !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(PurchaseGrainTutorialQuest)))) ? ((TextObject)null) : new TextObject("{=TeMExjrH}This option is disabled during current active quest.", (Dictionary<string, object>)null));
		return true;
	}

	private bool buy_products_village_menu_option_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)14;
		args.Tooltip = ((args.IsEnabled = TutorialPhase.Instance.TutorialQuestPhase >= TutorialQuestPhase.RecruitAndPurchaseStarted && (!Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(PurchaseGrainTutorialQuest)) || !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(RecruitTroopsTutorialQuest)))) ? ((TextObject)null) : new TextObject("{=TeMExjrH}This option is disabled during current active quest.", (Dictionary<string, object>)null));
		return true;
	}

	private bool raid_village_menu_option_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)12;
		return PlaceholderOptionsClickableCondition(args);
	}

	private bool wait_village_menu_option_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)15;
		return PlaceholderOptionsClickableCondition(args);
	}

	private bool PlaceholderOptionsClickableCondition(MenuCallbackArgs args)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		args.IsEnabled = false;
		args.Tooltip = new TextObject("{=F7VxtCSd}This option is disabled during tutorial phase.", (Dictionary<string, object>)null);
		return true;
	}

	private void storymode_recruit_volunteers_on_consequence(MenuCallbackArgs args)
	{
		TutorialPhase.Instance.PrepareRecruitOptionForTutorial();
		args.MenuContext.OpenRecruitVolunteers();
	}

	private void storymode_ui_village_buy_good_on_consequence(MenuCallbackArgs args)
	{
		InventoryScreenHelper.OpenScreenAsTrade(TutorialPhase.Instance.GetAndPrepareBuyProductsOptionForTutorial(Settlement.CurrentSettlement.Village), (SettlementComponent)(object)Settlement.CurrentSettlement.Village, (InventoryCategoryType)(-1), (Action)null);
	}

	[GameMenuInitializationHandler("storymode_tutorial_village_game_menu")]
	private static void storymode_tutorial_village_game_menu_on_init_background(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName(((SettlementComponent)Settlement.CurrentSettlement.Village).WaitMeshName);
	}

	[GameMenuInitializationHandler("storymode_game_menu_blocker")]
	private static void storymode_tutorial_blocker_game_menu_on_init_background(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName(((SettlementComponent)SettlementHelper.FindNearestVillageToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)null)).WaitMeshName);
	}

	private void storymode_game_menu_blocker_on_init(MenuCallbackArgs args)
	{
		if (Settlement.CurrentSettlement != null && ((MBObjectBase)Settlement.CurrentSettlement).StringId == "village_ES3_2")
		{
			GameMenu.SwitchToMenu("storymode_tutorial_village_game_menu");
		}
	}

	private void storymode_tutorial_village_game_menu_on_init(MenuCallbackArgs args)
	{
		if (!StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted)
		{
			GameMenu.SwitchToMenu("village_outside");
			return;
		}
		Settlement currentSettlement = Settlement.CurrentSettlement;
		Campaign.Current.GameMenuManager.MenuLocations.Clear();
		Campaign.Current.GameMenuManager.MenuLocations.AddRange(currentSettlement.LocationComplex.GetListOfLocations());
	}

	private bool storymode_conversation_blocker_on_condition()
	{
		return StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted;
	}

	private bool storymode_tutorial_village_enter_on_condition(MenuCallbackArgs args)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		List<Location> list = Settlement.CurrentSettlement.LocationComplex.GetListOfLocations().ToList();
		IssueQuestFlags val = Campaign.Current.IssueManager.CheckIssueForMenuLocations(list, true);
		args.OptionQuestData |= val;
		args.OptionQuestData |= Campaign.Current.QuestManager.CheckQuestForMenuLocations(list);
		args.optionLeaveType = (LeaveType)1;
		args.IsEnabled = !TutorialPhase.Instance.LockTutorialVillageEnter;
		if (!args.IsEnabled)
		{
			args.Tooltip = new TextObject("{=tWwXEWh6}Use the portrait to talk and enter the mission.", (Dictionary<string, object>)null);
		}
		return true;
	}

	private void storymode_tutorial_village_enter_on_consequence(MenuCallbackArgs args)
	{
		if (MobileParty.MainParty.CurrentSettlement == null)
		{
			PlayerEncounter.EnterSettlement();
		}
		LocationEncounter locationEncounter = PlayerEncounter.LocationEncounter;
		VillageEncounter val = (VillageEncounter)(object)((locationEncounter is VillageEncounter) ? locationEncounter : null);
		if (TutorialPhase.Instance.TutorialQuestPhase == TutorialQuestPhase.TravelToVillageStarted)
		{
			((LocationEncounter)val).CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("village_center"), (Location)null, StoryModeHeroes.ElderBrother.CharacterObject, (string)null);
		}
		else
		{
			((LocationEncounter)val).CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("village_center"), (Location)null, (CharacterObject)null, (string)null);
		}
	}

	private bool game_menu_leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private bool game_menu_leave_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void game_menu_leave_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Finish(true);
	}

	private void SpawnYourBrotherInLocation(Hero hero, string locationId)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if (LocationComplex.Current != null)
		{
			Location locationWithId = LocationComplex.Current.GetLocationWithId(locationId);
			Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(((BasicCharacterObject)hero.CharacterObject).Race);
			AgentData val = new AgentData((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, hero.CharacterObject, -1, default(UniqueTroopDescriptor), false, false)).Monster(baseMonsterFromRace).NoHorses(true);
			IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
			locationWithId.AddCharacter(new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddFixedCharacterBehaviors), (string)null, true, (CharacterRelations)1, (string)null, true, false, (ItemObject)null, false, true, true, (AfterAgentCreatedDelegate)null, false));
		}
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		if (quest is TravelToVillageTutorialQuest)
		{
			((QuestBase)new TalkToTheHeadmanTutorialQuest(((IEnumerable<Hero>)Settlement.CurrentSettlement.Notables).First((Hero n) => n.IsHeadman))).StartQuest();
			TutorialPhase.Instance.SetTutorialQuestPhase(TutorialQuestPhase.TalkToTheHeadmanStarted);
		}
		else if (quest is TalkToTheHeadmanTutorialQuest)
		{
			((QuestBase)new LocateAndRescueTravellerTutorialQuest()).StartQuest();
			TutorialPhase.Instance.SetTutorialQuestPhase(TutorialQuestPhase.LocateAndRescueTravellerStarted);
		}
		else if (quest is LocateAndRescueTravellerTutorialQuest)
		{
			((QuestBase)new FindHideoutTutorialQuest(((SettlementComponent)SettlementHelper.FindNearestHideoutToSettlement(Settlement.Find("village_ES3_2"), (NavigationType)1, (Func<Settlement, bool>)null)).Settlement)).StartQuest();
			TutorialPhase.Instance.SetTutorialQuestPhase(TutorialQuestPhase.FindHideoutStarted);
		}
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!(((MBObjectBase)settlement).StringId == "village_ES3_2") || TutorialPhase.Instance.IsCompleted)
		{
			return;
		}
		if (party != null)
		{
			if (party.IsMainParty)
			{
				SpawnYourBrotherInLocation(StoryModeHeroes.ElderBrother, "village_center");
			}
			else if (!party.IsMilitia)
			{
				party.SetMoveGoToSettlement(SettlementHelper.FindNearestSettlementToMobileParty(party, party.NavigationCapability, (Func<Settlement, bool>)((Settlement s) => s != settlement && (s.IsFortification || s.IsVillage) && settlement != s && settlement.MapFaction == s.MapFaction)), (NavigationType)1, false);
			}
		}
		if (party == null && hero != null && !hero.IsNotable)
		{
			TeleportHeroAction.ApplyImmediateTeleportToSettlement(hero, SettlementHelper.FindNearestSettlementToSettlement(settlement, (NavigationType)1, (Func<Settlement, bool>)((Settlement s) => s != settlement && (s.IsFortification || s.IsVillage) && settlement != s && settlement.MapFaction == s.MapFaction)));
		}
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (((MBObjectBase)settlement).StringId == "tutorial_training_field" && party == MobileParty.MainParty && TutorialPhase.Instance.TutorialQuestPhase == TutorialQuestPhase.None)
		{
			((QuestBase)new TravelToVillageTutorialQuest()).StartQuest();
			TutorialPhase.Instance.SetTutorialQuestPhase(TutorialQuestPhase.TravelToVillageStarted);
			Campaign.Current.IssueManager.ToggleAllIssueTracks(false);
		}
		if (party == MobileParty.MainParty)
		{
			CheckIfMainPartyStarving();
		}
	}

	private void CheckIfMainPartyStarving()
	{
		if (!TutorialPhase.Instance.IsCompleted && PartyBase.MainParty.IsStarving)
		{
			PartyBase.MainParty.ItemRoster.AddToCounts(DefaultItems.Grain, 1);
		}
	}

	private void SpawnAllNotablesForVillage(Village village)
	{
		int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(((SettlementComponent)village).Settlement, (Occupation)22);
		for (int i = 0; i < targetNotableCountForSettlement; i++)
		{
			HeroCreator.CreateNotable((Occupation)22, ((SettlementComponent)village).Settlement);
		}
	}
}
