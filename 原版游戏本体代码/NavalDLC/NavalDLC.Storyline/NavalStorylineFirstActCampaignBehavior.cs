using System;
using System.Collections.Generic;
using NavalDLC.Missions;
using NavalDLC.Storyline.Quests;
using StoryMode;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace NavalDLC.Storyline;

public class NavalStorylineFirstActCampaignBehavior : CampaignBehaviorBase
{
	private enum PortFightState
	{
		None,
		FightMissionStarted,
		FightMissionWon,
		FightShouldContinue,
		ReadyToBeFinalized
	}

	public class NavalStorylineFirstActCampaignBehaviorTypeDefiner : SaveableTypeDefiner
	{
		public NavalStorylineFirstActCampaignBehaviorTypeDefiner()
			: base(370000)
		{
		}

		protected override void DefineEnumTypes()
		{
			((SaveableTypeDefiner)this).AddEnumDefinition(typeof(PortFightState), 1, (IEnumResolver)null);
		}
	}

	private const string PortFightEnemyTroopStringId = "gangster_3";

	private PortFightState _portFightState;

	private bool _initialPortFightSuccessDialogPlayerOption1Selected;

	private bool _initialPortFightSuccessDialogPlayerOption2Selected;

	private bool _initialPortFightSuccessDialogPlayerOption4Selected;

	public override void RegisterEvents()
	{
		if (!NavalStorylineData.IsNavalStorylineCanceled())
		{
			CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnNewGameCreated);
			CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
			CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnAfterSessionLaunched);
			CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
			NavalDLCEvents.OnNavalStorylineCanceledEvent.AddNonSerializedListener((object)this, (Action<NavalStorylineData.StorylineCancelDetail>)OnNavalStorylineCanceled);
		}
	}

	private void OnNavalStorylineCanceled(NavalStorylineData.StorylineCancelDetail detail)
	{
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<PortFightState>("_portFightState", ref _portFightState);
	}

	private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
	{
		if (StoryModeManager.Current == null)
		{
			_portFightState = PortFightState.ReadyToBeFinalized;
		}
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		if (_portFightState != PortFightState.ReadyToBeFinalized && args.MenuContext.GameMenu.StringId == "port_menu" && Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement && Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(InquireAtOstican)))
		{
			if (_portFightState == PortFightState.FightMissionWon)
			{
				GameMenu.ActivateGameMenu("naval_storyline_after_port_fight");
			}
			else
			{
				GameMenu.ActivateGameMenu("naval_storyline_port_fight");
			}
		}
	}

	private void OnAfterSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddGameMenus(campaignGameStarter);
		AddPortFightOnSuccessDialogFlow(campaignGameStarter);
	}

	private void OnMissionEnded(IMission mission)
	{
		if (_portFightState == PortFightState.FightMissionStarted)
		{
			MissionResult missionResult = ((Mission)((mission is Mission) ? mission : null)).MissionResult;
			if (missionResult != null && missionResult.PlayerVictory)
			{
				_portFightState = PortFightState.FightMissionWon;
			}
			else
			{
				_portFightState = PortFightState.FightShouldContinue;
			}
		}
	}

	private void AddGameMenus(CampaignGameStarter campaignGameStarter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00a0: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_0102: Expected O, but got Unknown
		campaignGameStarter.AddGameMenu("naval_storyline_port_fight", "{=GhTjvwpl}You're strolling through {SETTLEMENT}{.o} streets when you hear raised voices coming from a side alley. You turn to look, and see three rough-looking men accosting an older man in a cloak. His gaze shifts quickly from one to the other and his body is tensed, as though he is going to spring into action. You sense a fight is about to start.", new OnInitDelegate(port_fight_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_port_fight", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(port_fight_condition), new OnConsequenceDelegate(port_fight_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("naval_storyline_after_port_fight", "{=!}{AFTER_PORT_FIGHT_MENU_TEXT}", new OnInitDelegate(after_port_fight_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_after_port_fight", "continue_to_dialog", "{=DM6luo3c}Continue", new OnConditionDelegate(naval_storyline_after_port_fight_continue_to_dialog_on_condition), new OnConsequenceDelegate(naval_storyline_after_port_fight_continue_to_dialog_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_after_port_fight", "return_to_fight", "{=inC6Ia5s}Return to the fight", new OnConditionDelegate(naval_storyline_after_port_fight_return_to_fight_on_condition), new OnConsequenceDelegate(naval_storyline_after_port_fight_return_to_fight_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_after_port_fight", "escape", "{=qqjRkMy9}Make good your escape", new OnConditionDelegate(naval_storyline_after_port_fight_escape_on_condition), new OnConsequenceDelegate(naval_storyline_after_port_fight_escape_on_consequence), true, -1, false, (object)null);
	}

	[GameMenuInitializationHandler("naval_storyline_port_fight")]
	[GameMenuInitializationHandler("naval_storyline_after_port_fight")]
	public static void port_menu_on_init(MenuCallbackArgs args)
	{
		string backgroundMeshName = ((MBObjectBase)Settlement.CurrentSettlement.Culture).StringId + "_port";
		args.MenuContext.SetBackgroundMeshName(backgroundMeshName);
		args.MenuContext.SetAmbientSound("event:/map/ambient/node/settlements/2d/port");
	}

	private void port_fight_on_init(MenuCallbackArgs args)
	{
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act1PortMenu);
		MBTextManager.SetTextVariable("SETTLEMENT", NavalStorylineData.HomeSettlement.EncyclopediaLinkWithName, false);
	}

	private bool port_fight_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		return true;
	}

	private void port_fight_consequence(MenuCallbackArgs args)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected I4, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		TroopRoster obj = TroopRoster.CreateDummyTroopRoster();
		obj.AddToCounts(CharacterObject.PlayerCharacter, 1, true, 0, 0, true, -1);
		obj.AddToCounts(NavalStorylineData.Gunnar.CharacterObject, 1, false, 0, 0, true, -1);
		TroopRoster obj2 = TroopRoster.CreateDummyTroopRoster();
		CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_3");
		obj2.AddToCounts(val, 3, false, 0, 0, true, -1);
		int wallLevel = Settlement.CurrentSettlement.Town.GetWallLevel();
		Settlement.CurrentSettlement.LocationComplex.GetScene("center", wallLevel);
		LocationComplex.Current.GetLocationWithId("center");
		GameMenu.ActivateGameMenu("naval_storyline_after_port_fight");
		_portFightState = PortFightState.FightMissionStarted;
		MissionInitializerRecord navalMissionInitializerTemplate = NavalStorylineData.GetNavalMissionInitializerTemplate("storyline_shipyard_alley");
		TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
		navalMissionInitializerTemplate.TerrainType = (int)faceTerrainType;
		navalMissionInitializerTemplate.NeedsRandomTerrain = false;
		navalMissionInitializerTemplate.PlayingInCampaignMode = true;
		navalMissionInitializerTemplate.RandomTerrainSeed = MBRandom.RandomInt(10000);
		navalMissionInitializerTemplate.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position);
		navalMissionInitializerTemplate.SceneHasMapPatch = false;
		navalMissionInitializerTemplate.AtmosphereOnCampaign.NauticalInfo.UsesNavalSimulatedWater = 1;
		NavalMissions.OpenNavalStorylineAlleyFightMission(navalMissionInitializerTemplate);
	}

	private void after_port_fight_on_init(MenuCallbackArgs args)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		if (_portFightState == PortFightState.None)
		{
			return;
		}
		if (_portFightState == PortFightState.FightMissionWon)
		{
			if (NavalStorylineData.Gunnar.IsWounded)
			{
				MBTextManager.SetTextVariable("AFTER_PORT_FIGHT_MENU_TEXT", new TextObject("{=3V80vvSz}You make quick work of the alley thugs, and help their victim to his feet. He seems dazed, but grateful.", (Dictionary<string, object>)null), false);
			}
			else if (Hero.MainHero.IsWounded)
			{
				MBTextManager.SetTextVariable("AFTER_PORT_FIGHT_MENU_TEXT", new TextObject("{=5NoZgdqr}The alley thugs are too many for you, and knock you to the ground. Before they can finish you off, however, you hear a rush of feet and cries of alarm. The town watch must have heard the commotion, and your assailants make a quick retreat. The watch helps you to your feet and tells you to be more careful. The thugs' victim, dazed but apparently unhurt, introduces himself.", (Dictionary<string, object>)null), false);
			}
			else
			{
				OnFightMissionFinalized();
			}
		}
		else if (_portFightState == PortFightState.FightShouldContinue)
		{
			MBTextManager.SetTextVariable("AFTER_PORT_FIGHT_MENU_TEXT", new TextObject("{=7C4JYwZp}You back out of the alley. You could easily escape, but you sense that the thugs will kill the old man.", (Dictionary<string, object>)null), false);
		}
		else
		{
			OnFightMissionFinalized();
		}
	}

	private void OnFightMissionFinalized()
	{
		_portFightState = PortFightState.ReadyToBeFinalized;
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act1PortFightSucceeded);
		GameMenu.SwitchToMenu("town");
	}

	private void OpenConversationWithGunnar()
	{
		SpawnPortQuestGiver();
		PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationOfCharacter(NavalStorylineData.Gunnar), (Location)null, NavalStorylineData.Gunnar.CharacterObject, (string)null);
	}

	private bool naval_storyline_after_port_fight_continue_to_dialog_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)12;
		return _portFightState == PortFightState.FightMissionWon;
	}

	private void naval_storyline_after_port_fight_continue_to_dialog_on_consequence(MenuCallbackArgs args)
	{
		OpenConversationWithGunnar();
	}

	private bool naval_storyline_after_port_fight_return_to_fight_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)12;
		return _portFightState == PortFightState.FightShouldContinue;
	}

	private void naval_storyline_after_port_fight_return_to_fight_on_consequence(MenuCallbackArgs args)
	{
		port_fight_consequence(args);
	}

	private bool naval_storyline_after_port_fight_escape_on_condition(MenuCallbackArgs args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		args.Tooltip = new TextObject("{=SpZEO1Rx}This option will abandon the storyline.", (Dictionary<string, object>)null);
		args.optionLeaveType = (LeaveType)16;
		return _portFightState == PortFightState.FightShouldContinue;
	}

	private void naval_storyline_after_port_fight_escape_on_consequence(MenuCallbackArgs args)
	{
		_portFightState = PortFightState.ReadyToBeFinalized;
		NavalDLCEvents.Instance.OnNavalStorylineCanceled(NavalStorylineData.StorylineCancelDetail.ByDialogue);
		GameMenu.SwitchToMenu("town_outside");
	}

	private void AddPortFightOnSuccessDialogFlow(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00cf: Expected O, but got Unknown
		//IL_00cf: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_0111: Expected O, but got Unknown
		//IL_0111: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		//IL_0153: Expected O, but got Unknown
		//IL_0153: Expected O, but got Unknown
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Expected O, but got Unknown
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Expected O, but got Unknown
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("initial_port_fight_success_dialog_start", "start", "gunnar_introduction_1", "{=!}{START_LINE}", new OnConditionDelegate(initial_port_fight_success_dialog_start_on_condition), (OnConsequenceDelegate)null, 50000, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("gunnar_introduction_1_line", "gunnar_introduction_1", "gunnar_introduction_2", "{=rbpBs3bZ}I am Gunnar of Lagshofn, from the Nordvyg lands.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("gunnar_introduction_2_line", "gunnar_introduction_2", "gunnar_introduction_3", "{=8kUr3LUi}I've come to this port seeking warriors and a ship. These men we fought were allies of a pirate gang who call themselves the Sea Hounds. They have been raiding and slaving along the Nordvyg’s shores, and I intend to go to war with them.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("gunnar_introduction_3_line", "gunnar_introduction_3", "initial_port_fight_success_dialog_player_options", "{=enXch5l7}The Sea Hounds and I have history, and nowadays they hate my guts as fiercely as I hate theirs. Somebody must have sent word of my whereabouts to their local friends as these lowlifes had a mind to do me in. Again, you have my thanks for evening the odds.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("initial_port_fight_success_dialog_player_options_1", "initial_port_fight_success_dialog_player_options", "initial_port_fight_success_dialog_player_options_1_answer", "{=Z39CjlP7}Did you say slave raids? My brother and sister were taken in one.", new OnConditionDelegate(initial_port_fight_success_dialog_player_options_1_condition), new OnConsequenceDelegate(initial_port_fight_success_dialog_player_options_1_on_consequence), 100, new OnClickableConditionDelegate(initial_port_fight_success_dialog_player_options_1_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("initial_port_fight_success_dialog_player_options_2", "initial_port_fight_success_dialog_player_options", "initial_port_fight_success_dialog_player_options_2_answer", "{=tIxXxFQU}Who are these Sea Hounds?", new OnConditionDelegate(initial_port_fight_success_dialog_player_options_2_condition), new OnConsequenceDelegate(initial_port_fight_success_dialog_player_options_2_on_consequence), 100, new OnClickableConditionDelegate(initial_port_fight_success_dialog_player_options_2_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("initial_port_fight_success_dialog_player_options_3", "initial_port_fight_success_dialog_player_options", "initial_port_fight_success_dialog_player_options_3_answer", "{=XP7g0Kiq}Why do you risk so much to hunt them?", new OnConditionDelegate(initial_port_fight_success_dialog_player_options_3_condition), new OnConsequenceDelegate(initial_port_fight_success_dialog_player_options_3_on_consequence), 100, new OnClickableConditionDelegate(initial_port_fight_success_dialog_player_options_3_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("initial_port_fight_success_dialog_player_options_4", "initial_port_fight_success_dialog_player_options", "initial_port_fight_success_dialog_player_options_4_answer", "{=ac5oq0pt}What are you doing now?", new OnConditionDelegate(initial_port_fight_success_dialog_continue_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("initial_port_fight_success_dialog_player_options_1_answer_line", "initial_port_fight_success_dialog_player_options_1_answer", "initial_port_fight_success_dialog_player_options", "{=zTr3dBd7}I know what it's like to lose family to slavers. If you're still searching, look to the Sea Hounds. They've got their hands in most of the slaving that happens along these coasts.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("initial_port_fight_success_dialog_player_options_2_answer_line", "initial_port_fight_success_dialog_player_options_2_answer", "initial_port_fight_success_dialog_player_options", "{=Vs5cNhfI}It’s hard to believe now, but they were once my brothers-in-arms. Years ago we fought side-by-side in the last great rebellion in the north. Most of the clans and many freemen like myself refused to bow to Volbjorn the usurper, as he was then called. But Volbjorn knew how to speak to men’s desires. He won over the bigger clans with promises of land and silver, and when summer came and he brought a fleet to give us battle, he had with him so many long ships that their sails covered the horizon. We still fought them of course, but their numbers were too many to beat.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("initial_port_fight_success_dialog_player_options_3_answer_line", "initial_port_fight_success_dialog_player_options_3_answer", "initial_port_fight_success_dialog_player_options", "{=lIpAlkH2}They dishonor what we fought for. I'm no stranger to battle - I'll kill when I must. But they murder for pleasure, thinking the All-Father rewards bloodthirst. He wants warriors, not hounds.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("initial_port_fight_success_dialog_player_options_4_answer_line", "initial_port_fight_success_dialog_player_options_4_answer", "next_move_explanation_1", "{=RQ0qIqGH}I mean to gather up with some of my kin and friends to go against the Sea Hounds. Just a few days ago, I ran into an old comrade of mine here in Ostican. He is called Purig and he happens to own a fast ship. He promised to help me capture a Sea Hound ship and put together a crew.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("next_move_explanation_1_line", "next_move_explanation_1", "next_move_explanation_player_options", "{=okfrRTb4}So, I'm going to make you a proposal. Perhaps you'd like to come with us? I can't guarantee we'll find your kin, but I can promise a good fight and, if we win, a bit of fine loot. And, well, if you'd ever had an interest in learning how to handle a ship, you won't find any better school than these northern seas.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("next_move_explanation_player_option_1_line", "next_move_explanation_player_options", "player_joins_gunnar_answer", "{=9buEaTHt}I will join you, and we can hunt together.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("next_move_explanation_player_option_2_line", "next_move_explanation_player_options", "player_waits_answer", "{=qFFYyNeR}Let me think this over.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("next_move_explanation_player_option_3_line", "next_move_explanation_player_options", "player_skips_tutorial", "{=JAuDUFkG}I have other obligations, and I already know how to handle a ship. (Skip tutorial)", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("player_joins_gunnar_answer_line", "player_joins_gunnar_answer", "close_window", "{=nu5vuTvX}You can find Purig in the tavern and introduce yourself. I should go get myself cleaned up and get ready to travel.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += OnQuestGiverSaved;
		}, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("player_waits_answer_line", "player_waits_answer", "close_window", "{=nyQhfz0B}The decision is of course yours. I expect you can find Purig in the tavern for the next few days, if you change your mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += OnQuestGiverSaved;
		}, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("player_skips_tutorial_line", "player_skips_tutorial", "skip_naval_tutorial_confirmation", "{=2biaAIpM}Very well. I hope you find your kin some day. Listen, whatever I manage to do near Hvalvik, I will return here and try to find other warriors to help me. If you ever reconsider, look for me here in Ostican.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("skip_tutorial_confirmation_option_1_line", "skip_naval_tutorial_confirmation", "player_joins_gunnar_answer", "{=58CsRmug}Wait, I changed my mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("skip_tutorial_confirmation_option_2_line", "skip_naval_tutorial_confirmation", "close_window", "{=1zleX968}Farewell to you too, and good luck.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += OnNavalTutorialSkipped;
		}, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
	}

	private bool initial_port_fight_success_dialog_start_on_condition()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		int num;
		if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar)
		{
			num = ((!NavalStorylineData.Gunnar.HasMet) ? 1 : 0);
			if (num != 0)
			{
				TextObject val = (Hero.MainHero.IsWounded ? new TextObject("{=h46iGLj0}Are you all right? One on three aren't the worst odds I've faced, but even so, that could have gone either way. I owe you my thanks.", (Dictionary<string, object>)null) : new TextObject("{=CvcV0DWt}By my blood… Damn, that hurts. I think I'm all right, though. Thank you.", (Dictionary<string, object>)null));
				TextObjectExtensions.SetCharacterProperties(val, "QUEST_GIVER", NavalStorylineData.Gunnar.CharacterObject, false);
				TextObjectExtensions.SetCharacterProperties(val, "PLAYER", Hero.MainHero.CharacterObject, false);
				MBTextManager.SetTextVariable("START_LINE", val, false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private bool initial_port_fight_success_dialog_player_options_1_condition()
	{
		return true;
	}

	private bool initial_port_fight_success_dialog_player_options_1_clickable_condition(out TextObject explanation)
	{
		explanation = TextObject.GetEmpty();
		return !_initialPortFightSuccessDialogPlayerOption1Selected;
	}

	private void initial_port_fight_success_dialog_player_options_1_on_consequence()
	{
		_initialPortFightSuccessDialogPlayerOption1Selected = true;
	}

	private bool initial_port_fight_success_dialog_player_options_2_condition()
	{
		return true;
	}

	private bool initial_port_fight_success_dialog_player_options_2_clickable_condition(out TextObject explanation)
	{
		explanation = TextObject.GetEmpty();
		return !_initialPortFightSuccessDialogPlayerOption2Selected;
	}

	private void initial_port_fight_success_dialog_player_options_2_on_consequence()
	{
		_initialPortFightSuccessDialogPlayerOption2Selected = true;
	}

	private bool initial_port_fight_success_dialog_player_options_3_condition()
	{
		return _initialPortFightSuccessDialogPlayerOption2Selected;
	}

	private bool initial_port_fight_success_dialog_player_options_3_clickable_condition(out TextObject explanation)
	{
		explanation = TextObject.GetEmpty();
		return !_initialPortFightSuccessDialogPlayerOption4Selected;
	}

	private void initial_port_fight_success_dialog_player_options_3_on_consequence()
	{
		_initialPortFightSuccessDialogPlayerOption4Selected = true;
	}

	private bool initial_port_fight_success_dialog_continue_condition()
	{
		if (_initialPortFightSuccessDialogPlayerOption1Selected && _initialPortFightSuccessDialogPlayerOption2Selected)
		{
			return _initialPortFightSuccessDialogPlayerOption4Selected;
		}
		return false;
	}

	private void SpawnPortQuestGiver()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)NavalStorylineData.Gunnar.CharacterObject).Race, "_settlement");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		LocationCharacter val = new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common", true, (CharacterRelations)0, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
		LocationComplex.Current.GetLocationWithId("center").AddCharacter(val);
	}

	private void OnQuestGiverSaved()
	{
		Mission.Current.GetMissionBehavior<NavalStorylineAlleyFightMissionController>().OnConversationEnded();
		LocationComplex.Current.RemoveCharacterIfExists(NavalStorylineData.Gunnar);
		NavalDLCEvents.Instance.OnGunnarSaved();
		NavalStorylineData.Gunnar.SetHasMet();
		OnFightMissionFinalized();
	}

	private void OnNavalTutorialSkipped()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		Mission current = Mission.Current;
		if (current != null)
		{
			current.GetMissionBehavior<NavalStorylineAlleyFightMissionController>()?.OnConversationEnded();
		}
		NavalStorylineData.Gunnar.SetHasMet();
		OnFightMissionFinalized();
		NavalDLCEvents.Instance.OnNavalStorylineTutorialSkipped();
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement != null && currentSettlement == NavalStorylineData.HomeSettlement && currentSettlement.HasPort && currentSettlement.LocationComplex.GetLocationOfCharacter(NavalStorylineData.Gunnar) == null)
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)NavalStorylineData.Gunnar.CharacterObject).Race, "_settlement");
			AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix);
			IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
			LocationCharacter val = new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common", true, (CharacterRelations)0, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
			LocationComplex.Current.GetLocationWithId("port").AddCharacter(val);
		}
	}
}
