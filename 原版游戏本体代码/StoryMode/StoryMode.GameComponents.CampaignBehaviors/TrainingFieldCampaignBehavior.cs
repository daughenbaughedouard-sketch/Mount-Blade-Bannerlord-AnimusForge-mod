using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Helpers;
using StoryMode.Extensions;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class TrainingFieldCampaignBehavior : CampaignBehaviorBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConditionDelegate _003C_003E9__9_0;

		public static OnConditionDelegate _003C_003E9__9_1;

		public static Action _003C_003E9__9_4;

		public static OnConsequenceDelegate _003C_003E9__9_3;

		public static Action _003C_003E9__16_0;

		internal bool _003COnSessionLaunched_003Eb__9_0(MenuCallbackArgs args)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			args.optionLeaveType = (LeaveType)1;
			return true;
		}

		internal bool _003COnSessionLaunched_003Eb__9_1(MenuCallbackArgs args)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			args.optionLeaveType = (LeaveType)16;
			return true;
		}

		internal void _003COnSessionLaunched_003Eb__9_3()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
			{
				Mission.Current.EndMission();
			};
		}

		internal void _003COnSessionLaunched_003Eb__9_4()
		{
			Mission.Current.EndMission();
		}

		internal void _003Cstorymode_go_to_end_tutorial_village_consequence_003Eb__16_0()
		{
			Mission.Current.EndMission();
		}
	}

	public bool SkipTutorialMission;

	private const string TrainingFieldLocationId = "training_field";

	private bool _completeTutorial;

	private bool _askedAboutRaiders1;

	private bool _askedAboutRaiders2;

	private bool _talkedWithBrotherForTheFirstTime;

	public override void SyncData(IDataStore dataStore)
	{
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener((object)this, (Action)OnCharacterCreationIsOver);
	}

	private void OnCharacterCreationIsOver()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!SkipTutorialMission)
		{
			Settlement val = Settlement.Find("tutorial_training_field");
			MobileParty.MainParty.Position = val.Position;
			EncounterManager.StartSettlementEncounter(MobileParty.MainParty, val);
			PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("training_field"), (Location)null, (CharacterObject)null, (string)null);
		}
		SkipTutorialMission = false;
		foreach (MobileParty item in (List<MobileParty>)(object)MobileParty.All)
		{
			item.Party.UpdateVisibilityAndInspected(MobileParty.MainParty.Position, 0f);
		}
		foreach (Settlement item2 in (List<Settlement>)(object)Settlement.All)
		{
			item2.Party.UpdateVisibilityAndInspected(MobileParty.MainParty.Position, 0f);
		}
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		//IL_017b: Expected O, but got Unknown
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_01b2: Expected O, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Expected O, but got Unknown
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Expected O, but got Unknown
		//IL_0274: Expected O, but got Unknown
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Expected O, but got Unknown
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Expected O, but got Unknown
		campaignGameStarter.AddGameMenu("training_field_menu", "{=5g9ZFGrN}You are at a training field. You can learn the basics of combat here.", new OnInitDelegate(game_menu_training_field_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		object obj = _003C_003Ec._003C_003E9__9_0;
		if (obj == null)
		{
			OnConditionDelegate val = delegate(MenuCallbackArgs args)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				args.optionLeaveType = (LeaveType)1;
				return true;
			};
			_003C_003Ec._003C_003E9__9_0 = val;
			obj = (object)val;
		}
		campaignGameStarter.AddGameMenuOption("training_field_menu", "training_field_enter", "{=F0ldgio8}Go back to training.", (OnConditionDelegate)obj, new OnConsequenceDelegate(game_menu_enter_training_field_on_consequence), false, -1, false, (object)null);
		object obj2 = _003C_003Ec._003C_003E9__9_1;
		if (obj2 == null)
		{
			OnConditionDelegate val2 = delegate(MenuCallbackArgs args)
			{
				//IL_0003: Unknown result type (might be due to invalid IL or missing references)
				args.optionLeaveType = (LeaveType)16;
				return true;
			};
			_003C_003Ec._003C_003E9__9_1 = val2;
			obj2 = (object)val2;
		}
		campaignGameStarter.AddGameMenuOption("training_field_menu", "training_field_leave", "{=3sRdGQou}Leave", (OnConditionDelegate)obj2, new OnConsequenceDelegate(game_menu_settlement_leave_on_consequence), true, -1, false, (object)null);
		campaignGameStarter.AddDialogLine("brother_training_field_start_coversation", "start", "training_field_line_2", "{=4vsPD3ec}{?PLAYER.GENDER}Sister{?}Brother{\\?}... It's been three days now we've been tracking those bastards. I think we're getting close. We need to think about what happens when we catch them. How are we going to rescue {PLAYER_LITTLE_BROTHER.LINK} and {PLAYER_LITTLE_SISTER.LINK}? Are we up for a fight?[if:convo_grave]", new OnConditionDelegate(storymode_training_field_start_on_condition), (OnConsequenceDelegate)null, 1000001, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("brother_training_field_start_coversation_2", "training_field_line_2", "player_answer_training_field", "{=MfczTFxp}This looks like an old training field for the legions. Perhaps we can spare some time and brush up on our skills. The practice could come in handy when we catch up with the raiders.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 1000001, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_answer_play_training_field", "player_answer_training_field", "play_tutorial", "{=FaQDaRri}I'm going to run the course. I need to know I can fight if I have to. (Continue tutorial)", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			_talkedWithBrotherForTheFirstTime = true;
		}, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_answer_skip_tutorial", "player_answer_training_field", "skip_tutorial", "{=gYYGGflb}We have no time to lose. We can do more if we split up. (Skip tutorial)", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_answer_ask_about_raiders_1", "player_answer_training_field", "ask_about_raiders_1", "{=b7Z1OBas}So, do you think we'll catch up with the raiders soon?", (OnConditionDelegate)null, new OnConsequenceDelegate(storymode_asked_about_raiders_1_consequence), 100, new OnClickableConditionDelegate(storymode_asked_about_raiders_1_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_answer_ask_about_raiders_2", "player_answer_training_field", "ask_about_raiders_2", "{=tzkclhXs}How should we prepare for the fight?", (OnConditionDelegate)null, new OnConsequenceDelegate(storymode_asked_about_raiders_2_consequence), 100, new OnClickableConditionDelegate(storymode_asked_about_raiders_2_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("end_prolouge_conversation", "play_tutorial", "close_window", "{=hT2Hh70m}Let's go on then. (Play the combat tutorial)", (OnConditionDelegate)null, new OnConsequenceDelegate(storymode_go_to_end_tutorial_village_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ask_about_tutorial_end_confirmation", "skip_tutorial", "skip_tutorial_confirmation", "{=FUwIgcZO}Are you sure about that? (This option will finish the tutorial, which has story elements, and start the full single player campaign. It is recommended that you pick this option only if you have already played the tutorial once.)", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("explanation_about_raiders_1", "ask_about_raiders_1", "training_field_line_2", "{=YAWCkOYa}The tracks look fresh, and I've seen some smoke on the horizon. They can't move too quickly if they're still looting and raiding. No, I'm pretty sure we'll be able to rescue the little ones... or die trying.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("explanation_about_raiders_2", "ask_about_raiders_2", "training_field_line_2", "{=NItH4oL6}Well, if they're still pillaging they may have split up into smaller groups. Hopefully we won't need to take them all on at once. But it would help if we could hire or persuade some people to join us.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("end_tutorial_yes", "skip_tutorial_confirmation", "end_tutorial", "{=a4W7Gzka}Yes. Time is of the essence. (Skip tutorial)", (OnConditionDelegate)null, new OnConsequenceDelegate(storymode_skip_tutorial_from_conversation_consequence), 100, new OnClickableConditionDelegate(storymode_skip_tutorial_from_conversation_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("end_tutorial_no", "skip_tutorial_confirmation", "training_field_line_2", "{=5qhaDtef}No. Let me rethink this.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("end_tutorial_goodbye_start", "end_tutorial", "end_tutorial_goodbye", "{=QF8B6XFS}All right then. Let us split up and look for the little ones separately. I'll send you a word if I find them before you do...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("end_tutorial_select_family_name", "end_tutorial_goodbye", "close_window", "{=LbSvq3be}One other thing, {?PLAYER.GENDER}sister{?}brother{\\?}. We want people to take us seriously. We may be leading men into battle soon. Let's give our family a name and a banner, like the nobles do.", (OnConditionDelegate)null, new OnConsequenceDelegate(storymode_go_to_end_tutorial_village_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("brother_training_field_default_coversation", "start", "player_answer_training_field_default", "{=kIklPYto}Are you ready to leave here?", new OnConditionDelegate(story_mode_training_field_default_conversation_with_brother_condition), (OnConsequenceDelegate)null, 1000001, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_answer_play_training_field_2", "player_answer_training_field_default", "close_window", "{=k07wzat8}I am not ready yet.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		object obj3 = _003C_003Ec._003C_003E9__9_3;
		if (obj3 == null)
		{
			OnConsequenceDelegate val3 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
				{
					Mission.Current.EndMission();
				};
			};
			_003C_003Ec._003C_003E9__9_3 = val3;
			obj3 = (object)val3;
		}
		campaignGameStarter.AddPlayerLine("player_answer_skip_tutorial_2", "player_answer_training_field_default", "close_window", "{=bSDt8FN5}I am ready, let's go!", (OnConditionDelegate)null, (OnConsequenceDelegate)obj3, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
	}

	private void OnMissionEnded(IMission mission)
	{
		if (_completeTutorial)
		{
			StoryModeManager.Current.MainStoryLine.CompleteTutorialPhase(isSkipped: true);
			_completeTutorial = false;
		}
	}

	private void game_menu_training_field_on_init(MenuCallbackArgs args)
	{
		Settlement val = ((Settlement.CurrentSettlement == null) ? MobileParty.MainParty.CurrentSettlement : Settlement.CurrentSettlement);
		Campaign.Current.GameMenuManager.MenuLocations.Clear();
		Campaign.Current.GameMenuManager.MenuLocations.Add(val.LocationComplex.GetLocationWithId("training_field"));
		PlayerEncounter.EnterSettlement();
		PlayerEncounter.LocationEncounter = (LocationEncounter)(object)new TrainingFieldEncounter(val);
		GameState activeState = GameStateManager.Current.ActiveState;
		MapState val2;
		if ((val2 = (MapState)(object)((activeState is MapState) ? activeState : null)) != null)
		{
			val2.Handler.TeleportCameraToMainParty();
		}
	}

	private static void game_menu_enter_training_field_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("training_field"), (Location)null, (CharacterObject)null, (string)null);
	}

	[GameMenuInitializationHandler("training_field_menu")]
	private static void storymode_tutorial_training_field_game_menu_on_init_background(MenuCallbackArgs args)
	{
		TrainingField trainingField = Settlement.Find("tutorial_training_field").TrainingField();
		args.MenuContext.SetBackgroundMeshName(((SettlementComponent)trainingField).WaitMeshName);
	}

	private static void game_menu_settlement_leave_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.LeaveSettlement();
		PlayerEncounter.Finish(true);
	}

	private bool storymode_training_field_start_on_condition()
	{
		StringHelpers.SetCharacterProperties("PLAYER_LITTLE_BROTHER", StoryModeHeroes.LittleBrother.CharacterObject, (TextObject)null, false);
		StringHelpers.SetCharacterProperties("PLAYER_LITTLE_SISTER", StoryModeHeroes.LittleSister.CharacterObject, (TextObject)null, false);
		if (StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted)
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (((currentSettlement != null) ? ((MBObjectBase)currentSettlement).StringId : null) == "tutorial_training_field" && Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == StoryModeHeroes.ElderBrother)
			{
				return !_talkedWithBrotherForTheFirstTime;
			}
		}
		return false;
	}

	private void storymode_go_to_end_tutorial_village_consequence()
	{
		TutorialPhase.Instance.PlayerTalkedWithBrotherForTheFirstTime();
		if (_completeTutorial)
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
			{
				Mission.Current.EndMission();
			};
		}
	}

	private bool storymode_skip_tutorial_from_conversation_clickable_condition(out TextObject explanation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		explanation = new TextObject("{=XlSHcfsP}This option will end the tutorial!", (Dictionary<string, object>)null);
		return true;
	}

	private void storymode_skip_tutorial_from_conversation_consequence()
	{
		_completeTutorial = true;
	}

	private bool storymode_asked_about_raiders_1_clickable_condition(out TextObject explanation)
	{
		explanation = null;
		return !_askedAboutRaiders1;
	}

	private bool storymode_asked_about_raiders_2_clickable_condition(out TextObject explanation)
	{
		explanation = null;
		return !_askedAboutRaiders2;
	}

	private void storymode_asked_about_raiders_1_consequence()
	{
		_askedAboutRaiders1 = true;
	}

	private void storymode_asked_about_raiders_2_consequence()
	{
		_askedAboutRaiders2 = true;
	}

	private bool story_mode_training_field_default_conversation_with_brother_condition()
	{
		if (StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted && (Settlement.CurrentSettlement == null || ((MBObjectBase)Settlement.CurrentSettlement).StringId != "village_ES3_2") && CharacterObject.OneToOneConversationCharacter == StoryModeHeroes.ElderBrother.CharacterObject)
		{
			return _talkedWithBrotherForTheFirstTime;
		}
		return false;
	}
}
