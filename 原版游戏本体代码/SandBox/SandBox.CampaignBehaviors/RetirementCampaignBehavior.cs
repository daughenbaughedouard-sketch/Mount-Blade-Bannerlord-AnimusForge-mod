using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.CampaignBehaviors;

public class RetirementCampaignBehavior : CampaignBehaviorBase
{
	private Hero _selectedHeir;

	private bool _hasTalkedWithHermitBefore;

	private bool _playerEndedGame;

	private Settlement _retirementSettlement;

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<bool>("_hasTalkedWithHermitBefore", ref _hasTalkedWithHermitBefore);
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, (Action)HourlyTick);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)GameMenuOpened);
	}

	private void GameMenuOpened(MenuCallbackArgs args)
	{
		if (args.MenuContext.GameMenu.StringId == "retirement_place")
		{
			if (_selectedHeir != null)
			{
				PlayerEncounter.Finish(true);
				ApplyHeirSelectionAction.ApplyByRetirement(_selectedHeir);
				_selectedHeir = null;
			}
			else if (_playerEndedGame)
			{
				GameMenu.ExitToLast();
				ShowGameStatistics();
			}
		}
	}

	private void HourlyTick()
	{
		CheckRetirementSettlementVisibility();
	}

	private void OnSessionLaunched(CampaignGameStarter starter)
	{
		_retirementSettlement = Settlement.Find("retirement_retreat");
		SetupGameMenus(starter);
		SetupConversationDialogues(starter);
	}

	private void SetupGameMenus(CampaignGameStarter starter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0081: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00c6: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_00f7: Expected O, but got Unknown
		starter.AddGameMenu("retirement_place", "{=ruHt0Ub5}You are at the base of Mount Erithrys, an ancient volcano that has long been a refuge for oracles, seers and mystics. High up a steep valley, you can make out a number of caves carved into the soft volcanic rock. Coming closer, you see that some show signs of habitation. An old man in worn and tattered robes sits at the mouth of one of these caves, meditating in the cool mountain air.", new OnInitDelegate(retirement_menu_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		starter.AddGameMenuOption("retirement_place", "retirement_place_enter", "{=H3fobmyO}Approach", new OnConditionDelegate(enter_on_condition), new OnConsequenceDelegate(retirement_menu_on_enter), false, -1, false, (object)null);
		starter.AddGameMenuOption("retirement_place", "retirement_place_leave", "{=3sRdGQou}Leave", new OnConditionDelegate(leave_on_condition), new OnConsequenceDelegate(retirement_menu_on_leave), true, -1, false, (object)null);
		starter.AddGameMenu("retirement_after_player_knockedout", "{=DK3QwC68}Your men on the slopes below saw you collapse, and rushed up to your aid. They tended to your wounds as well as they could, and you will survive. They are now awaiting your next decision.", (OnInitDelegate)null, (MenuOverlayType)0, (MenuFlags)0, (object)null);
		starter.AddGameMenuOption("retirement_after_player_knockedout", "enter", "{=H3fobmyO}Approach", new OnConditionDelegate(enter_on_condition), new OnConsequenceDelegate(retirement_menu_on_enter), false, -1, false, (object)null);
		starter.AddGameMenuOption("retirement_after_player_knockedout", "leave", "{=3sRdGQou}Leave", new OnConditionDelegate(leave_on_condition), new OnConsequenceDelegate(retirement_menu_on_leave), true, -1, false, (object)null);
	}

	private void SetupConversationDialogues(CampaignGameStarter starter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected O, but got Unknown
		//IL_01cf: Expected O, but got Unknown
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected O, but got Unknown
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Expected O, but got Unknown
		starter.AddDialogLine("hermit_start_1", "start", "player_answer", "{=09PJ02x6}Hello there. You must be one of those lordly types who ride their horses about this land, scrabbling for wealth and power. I wonder what you hope to gain from such things...", new OnConditionDelegate(hermit_start_talk_first_time_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		starter.AddDialogLine("hermit_start_2", "start", "player_accept_or_decline", "{=YmNT7HJ3}Have you made up your mind? Are you ready to let go of your earthly burdens and relish a simpler life?", new OnConditionDelegate(hermit_start_talk_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		starter.AddPlayerLine("player_answer_hermit_start_1", "player_answer", "hermit_answer_1", "{=1FbydhBb}I rather enjoy wealth and power.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		starter.AddPlayerLine("player_answer_hermit_start_2", "player_answer", "hermit_answer_2", "{=TOpysYqG}Power can mean the power to do good, you know.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		starter.AddDialogLine("hermit_answer_1", "hermit_answer_1", "hermit_answer_continue", "{=v7pJerga}Ah, but the thing about wealth and power is that someone is always trying to take it from you. Perhaps even one's own children. There is no rest for the powerful.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		starter.AddDialogLine("hermit_answer_2", "hermit_answer_2", "hermit_answer_continue", "{=1MaECke2}Many people tell themselves that they only want power to do good, but still seek it for its own sake. The only way to know for sure that you do not lust after power is to give it up.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		starter.AddDialogLine("hermit_answer_continue_1", "hermit_answer_continue", "player_accept_or_decline", "{=Q2RKkISe}There are a number of us up here in our caves. We dine well on locusts and wild honey. Some of us once knew wealth and power, but what we relish now is freedom. Freedom from the worry that what we hoard will be lost. Freedom from the fear that our actions offend Heaven. The only true freedom. We welcome newcomers, like your good self.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			_hasTalkedWithHermitBefore = true;
		}, 100, (OnClickableConditionDelegate)null);
		starter.AddPlayerLine("player_accept_or_decline_2", "player_accept_or_decline", "player_accept", "{=z2duf82b}Well, perhaps I would like to know peace for a while. But I would need to think about the future of my clan...", (OnConditionDelegate)null, new OnConsequenceDelegate(hermit_player_select_heir_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		starter.AddPlayerLine("player_accept_or_decline_1", "player_accept_or_decline", "player_decline", "{=x4wnaovC}I am not sure I share your idea of freedom.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		starter.AddDialogLine("hermit_answer_player_decline", "player_decline", "close_window", "{=uXXH5GIU}Please, then, return to your wars and worry. I cannot help someone attain what they do not want. But I am not going anywhere, so if you desire my help at any time, do come again.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		starter.AddDialogLine("hermit_answer_player_accept", "player_accept", "hermit_player_select_heir", "{=6nIC6i2V}I have acolytes who seek me out from time to time. I can have one of them take a message to your kinfolk. Perhaps you would like to name an heir?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		starter.AddRepeatablePlayerLine("hermit_player_select_heir_1", "hermit_player_select_heir", "close_window", "{=!}{HEIR.NAME}", "{=epepvysB}I have someone else in mind.", "player_accept", new OnConditionDelegate(hermit_select_heir_multiple_on_condition), new OnConsequenceDelegate(hermit_select_heir_multiple_on_consequence), 100, (OnClickableConditionDelegate)null);
		starter.AddPlayerLine("hermit_player_select_heir_2", "hermit_player_select_heir", "close_window", "{=Qbp2AiRZ}I choose not to name anyone. I shall think no more of worldly things. (END GAME)", (OnConditionDelegate)null, new OnConsequenceDelegate(hermit_player_retire_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		starter.AddPlayerLine("hermit_player_select_heir_3", "hermit_player_select_heir", "close_window", "{=CH7b5LaX}I have changed my mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			_selectedHeir = null;
		}, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
	}

	private bool hermit_start_talk_first_time_on_condition()
	{
		if (!_hasTalkedWithHermitBefore)
		{
			return ((MBObjectBase)CharacterObject.OneToOneConversationCharacter).StringId == "sp_hermit";
		}
		return false;
	}

	private void hermit_player_retire_on_consequence()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		TextObject val = new TextObject("{=LmoyzsTE}{PLAYER.NAME} will retire. {HEIR_PART} Would you like to continue?", (Dictionary<string, object>)null);
		if (_selectedHeir == null)
		{
			TextObject val2 = new TextObject("{=RPgzaZeR}Your game will end.", (Dictionary<string, object>)null);
			val.SetTextVariable("HEIR_PART", val2);
		}
		else
		{
			TextObject val3 = new TextObject("{=GEvP9i5f}You will play on as {HEIR.NAME}.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val3, "HEIR", _selectedHeir.CharacterObject, false);
			val.SetTextVariable("HEIR_PART", val3);
		}
		InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_decision", (string)null)).ToString(), ((object)val).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)DecideRetirementPositively, (Action)DecideRetirementNegatively, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void DecideRetirementPositively()
	{
		if (_selectedHeir != null)
		{
			_hasTalkedWithHermitBefore = false;
		}
		else
		{
			_playerEndedGame = true;
		}
		Mission.Current.EndMission();
	}

	private void DecideRetirementNegatively()
	{
		_selectedHeir = null;
	}

	private void hermit_player_select_heir_on_consequence()
	{
		List<Hero> list = new List<Hero>();
		foreach (KeyValuePair<Hero, int> item in from x in Clan.PlayerClan.GetHeirApparents()
			orderby x.Value
			select x)
		{
			list.Add(item.Key);
		}
		ConversationSentence.SetObjectsToRepeatOver((IReadOnlyList<object>)list, 5);
	}

	private void hermit_select_heir_multiple_on_consequence()
	{
		ref Hero selectedHeir = ref _selectedHeir;
		object selectedRepeatObject = ConversationSentence.SelectedRepeatObject;
		selectedHeir = (Hero)((selectedRepeatObject is Hero) ? selectedRepeatObject : null);
		hermit_player_retire_on_consequence();
	}

	private bool hermit_select_heir_multiple_on_condition()
	{
		if (Clan.PlayerClan.GetHeirApparents().Any())
		{
			object currentProcessedRepeatObject = ConversationSentence.CurrentProcessedRepeatObject;
			Hero val = (Hero)((currentProcessedRepeatObject is Hero) ? currentProcessedRepeatObject : null);
			if (val != null)
			{
				TextObjectExtensions.SetCharacterProperties(ConversationSentence.SelectedRepeatLine, "HEIR", val.CharacterObject, false);
				return true;
			}
		}
		return false;
	}

	private bool hermit_start_talk_on_condition()
	{
		if (_hasTalkedWithHermitBefore)
		{
			return ((MBObjectBase)CharacterObject.OneToOneConversationCharacter).StringId == "sp_hermit";
		}
		return false;
	}

	private void ShowGameStatistics()
	{
		GameOverState val = Game.Current.GameStateManager.CreateState<GameOverState>(new object[1] { (object)(GameOverReason)0 });
		Game.Current.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void retirement_menu_on_init(MenuCallbackArgs args)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		Settlement currentSettlement = MobileParty.MainParty.CurrentSettlement;
		if (currentSettlement != null)
		{
			Campaign.Current.GameMenuManager.MenuLocations.Clear();
			Campaign.Current.GameMenuManager.MenuLocations.Add(currentSettlement.LocationComplex.GetLocationWithId("retirement_retreat"));
			args.MenuContext.SetBackgroundMeshName(Settlement.CurrentSettlement.SettlementComponent.WaitMeshName);
			PlayerEncounter.EnterSettlement();
			PlayerEncounter.LocationEncounter = (LocationEncounter)new RetirementEncounter(currentSettlement);
		}
	}

	private bool enter_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		return true;
	}

	private bool leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void retirement_menu_on_enter(MenuCallbackArgs args)
	{
		PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("retirement_retreat"), (Location)null, (CharacterObject)null, (string)null);
	}

	private void retirement_menu_on_leave(MenuCallbackArgs args)
	{
		PlayerEncounter.LeaveSettlement();
		PlayerEncounter.Finish(true);
	}

	private void CheckRetirementSettlementVisibility()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float hideoutSpottingDistance = Campaign.Current.Models.MapVisibilityModel.GetHideoutSpottingDistance();
		CampaignVec2 position = MobileParty.MainParty.Position;
		float num = ((CampaignVec2)(ref position)).DistanceSquared(_retirementSettlement.Position);
		if (1f - num / (hideoutSpottingDistance * hideoutSpottingDistance) > 0f)
		{
			_retirementSettlement.IsVisible = true;
			SettlementComponent settlementComponent = _retirementSettlement.SettlementComponent;
			RetirementSettlementComponent val = (RetirementSettlementComponent)(object)((settlementComponent is RetirementSettlementComponent) ? settlementComponent : null);
			if (!val.IsSpotted)
			{
				val.IsSpotted = true;
			}
		}
	}
}
