using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace SandBox.CampaignBehaviors;

public class PrisonBreakCampaignBehavior : CampaignBehaviorBase
{
	private const int CoolDownInDays = 7;

	private const int PrisonBreakDialogPriority = 120;

	private const string DefaultPrisonGuardWeaponId = "battania_mace_1_t2";

	private Dictionary<Settlement, CampaignTime> _coolDownData = new Dictionary<Settlement, CampaignTime>();

	private Hero _prisonerHero;

	private bool _launchingPrisonBreakMission;

	private int _bribeCost;

	private string _previousMenuId;

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.CanHeroDieEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, KillCharacterActionDetail, bool>)CanHeroDie);
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> availableSpawnPoints)
	{
		if (_launchingPrisonBreakMission)
		{
			_launchingPrisonBreakMission = false;
			int num = 8;
			Location locationWithId = LocationComplex.Current.GetLocationWithId("prison");
			locationWithId.RemoveAllCharacters();
			locationWithId.AddCharacter(CreatePrisonBreakPrisoner());
			for (int i = 0; i < num; i++)
			{
				LocationCharacter val = CreatePrisonBreakGuard();
				locationWithId.AddCharacter(val);
			}
		}
	}

	private LocationCharacter CreatePrisonBreakPrisoner()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)_prisonerHero.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Age((int)((BasicCharacterObject)_prisonerHero.CharacterObject).Age).NoHorses(true);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddCompanionBehaviors), "sp_prison_break_prisoner", true, (CharacterRelations)1, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_guard"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	public LocationCharacter CreatePrisonBreakGuard()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		AgentData prisonGuardAgentData = GetPrisonGuardAgentData();
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation((CharacterObject)prisonGuardAgentData.AgentCharacter, ref num, ref num2, "");
		prisonGuardAgentData.Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(prisonGuardAgentData, new AddBehaviorsDelegate(agentBehaviorManager.AddStealthAgentBehaviors), "stealth_agent", true, (CharacterRelations)2, ActionSetCode.GenerateActionSetNameWithSuffix(prisonGuardAgentData.AgentMonster, prisonGuardAgentData.AgentIsFemale, "_guard"), false, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private AgentData GetPrisonGuardAgentData()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		List<CharacterObject> list = LinQuick.ToListQ<CharacterObject>(CharacterHelper.GetTroopTree(Settlement.CurrentSettlement.Owner.Culture.BasicTroop.Culture.BasicTroop, 2f, 3f));
		CharacterObject obj = (LinQuick.AnyQ<CharacterObject>(list, (Func<CharacterObject, bool>)((CharacterObject x) => !((BasicCharacterObject)x).IsRanged)) ? Extensions.GetRandomElementInefficiently<CharacterObject>(list.Where((CharacterObject x) => !((BasicCharacterObject)x).IsRanged)) : Extensions.GetRandomElementInefficiently<CharacterObject>((IEnumerable<CharacterObject>)list));
		Equipment val = ((BasicCharacterObject)obj).Equipment.Clone(true);
		val.AddEquipmentToSlotWithoutAgent((EquipmentIndex)0, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("battania_mace_1_t2"), (ItemModifier)null, (ItemObject)null, false));
		return new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)obj, -1, (Banner)null, default(UniqueTroopDescriptor))).Equipment(val);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<Hero>("_prisonerHero", ref _prisonerHero);
		dataStore.SyncData<Dictionary<Settlement, CampaignTime>>("_coolDownData", ref _coolDownData);
		dataStore.SyncData<string>("_previousMenuId", ref _previousMenuId);
	}

	private void CanHeroDie(Hero hero, KillCharacterActionDetail detail, ref bool result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)detail == 4 && hero == Hero.MainHero && _prisonerHero != null && CampaignMission.Current != null)
		{
			Location location = CampaignMission.Current.Location;
			Settlement currentSettlement = Settlement.CurrentSettlement;
			object obj;
			if (currentSettlement == null)
			{
				obj = null;
			}
			else
			{
				LocationComplex locationComplex = currentSettlement.LocationComplex;
				obj = ((locationComplex != null) ? locationComplex.GetLocationWithId("prison") : null);
			}
			if (location == obj)
			{
				result = false;
			}
		}
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddGameMenus(campaignGameStarter);
		AddDialogs(campaignGameStarter);
	}

	private void AddGameMenus(CampaignGameStarter campaignGameStarter)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0031: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0062: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_0093: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00e3: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0114: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_0159: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected O, but got Unknown
		//IL_01a9: Expected O, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Expected O, but got Unknown
		//IL_01f9: Expected O, but got Unknown
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Expected O, but got Unknown
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Expected O, but got Unknown
		//IL_0249: Expected O, but got Unknown
		campaignGameStarter.AddGameMenuOption("town_keep_dungeon", "town_prison_break", "{=lc0YIqby}Stage a prison break", new OnConditionDelegate(game_menu_stage_prison_break_on_condition), new OnConsequenceDelegate(game_menu_castle_prison_break_from_dungeon_on_consequence), false, 2, false, (object)null);
		campaignGameStarter.AddGameMenuOption("castle_dungeon", "town_prison_break", "{=lc0YIqby}Stage a prison break", new OnConditionDelegate(game_menu_stage_prison_break_on_condition), new OnConsequenceDelegate(game_menu_castle_prison_break_from_castle_dungeon_on_consequence), false, 2, false, (object)null);
		campaignGameStarter.AddGameMenuOption("town_enemy_town_keep", "town_prison_break", "{=lc0YIqby}Stage a prison break", new OnConditionDelegate(game_menu_stage_prison_break_on_condition), new OnConsequenceDelegate(game_menu_castle_prison_break_from_enemy_keep_on_consequence), false, 0, false, (object)null);
		campaignGameStarter.AddGameMenu("start_prison_break", "{=aZaujaHb}The guard accepts your offer. He is ready to help you break {PRISONER.NAME} out, if you're willing to pay.", new OnInitDelegate(start_prison_break_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("start_prison_break", "start", "{=N6UeziT8}Start ({COST}{GOLD_ICON})", new OnConditionDelegate(game_menu_castle_prison_break_on_condition), (OnConsequenceDelegate)delegate
		{
			OpenPrisonBreakMission();
		}, false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("start_prison_break", "leave", "{=3sRdGQou}Leave", new OnConditionDelegate(game_menu_leave_on_condition), new OnConsequenceDelegate(game_menu_cancel_prison_break), true, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("prison_break_cool_down", "{=cGSXFJ3N}Because of a recent breakout attempt in this settlement it is on high alert. The guard won't even be seen talking to you.", (OnInitDelegate)null, (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("prison_break_cool_down", "leave", "{=3sRdGQou}Leave", new OnConditionDelegate(game_menu_leave_on_condition), new OnConsequenceDelegate(game_menu_cancel_prison_break), true, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("settlement_prison_break_success", "{=TazumJGN}You emerge into the streets. No one is yet aware of what happened in the dungeons, and you hustle {PRISONER.NAME} towards the gates.{newline}You may now leave the {?SETTLEMENT_TYPE}settlement{?}castle{\\?}.", new OnInitDelegate(settlement_prison_break_success_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("settlement_prison_break_success", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(game_menu_continue_on_condition), new OnConsequenceDelegate(settlement_prison_break_success_continue_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("settlement_prison_break_fail_player_unconscious", "{=svuD2vBo}You were knocked unconscious while trying to break {PRISONER.NAME} out of the dungeon.{newline}The guards caught you both and threw you in a cell.", new OnInitDelegate(settlement_prison_break_fail_on_init), (MenuOverlayType)3, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("settlement_prison_break_fail_player_unconscious", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(game_menu_continue_on_condition), new OnConsequenceDelegate(settlement_prison_break_fail_player_unconscious_continue_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("settlement_prison_break_fail_prisoner_unconscious", "{=eKy1II3h}You made your way out but {PRISONER.NAME} was badly wounded during the escape. You had no choice but to leave {?PRISONER.GENDER}her{?}him{\\?} behind as you disappeared into the back streets and sneaked out the gate.{INFORMATION_IF_PRISONER_DEAD}", new OnInitDelegate(settlement_prison_break_fail_prisoner_injured_on_init), (MenuOverlayType)3, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("settlement_prison_break_fail_prisoner_unconscious", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(game_menu_continue_on_condition), new OnConsequenceDelegate(settlement_prison_break_fail_prisoner_unconscious_continue_on_consequence), false, -1, false, (object)null);
	}

	private void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Expected O, but got Unknown
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("prison_break_start_1", "start", "prison_break_end_already_met", "{=5RDF3aZN}{SALUTATION}... You came for me!", new OnConditionDelegate(prison_break_end_with_success_clan_member), (OnConsequenceDelegate)null, 120, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_break_start_2", "start", "prison_break_end_already_met", "{=PRadDFN5}{SALUTATION}... Well, I hadn't expected this, but I'm very grateful.", new OnConditionDelegate(prison_break_end_with_success_player_already_met), (OnConsequenceDelegate)null, 120, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_break_start_3", "start", "prison_break_end_meet", "{=zbPRul7h}Well.. I don't know you, but I'm very grateful.", new OnConditionDelegate(prison_break_end_with_success_other_on_condition), (OnConsequenceDelegate)null, 120, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_break_player_ask", "prison_break_end_already_met", "prison_break_next_move", "{=qFoMsPIf}I'm glad we made it out safe. What will you do now?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_break_player_meet", "prison_break_end_meet", "prison_break_next_move", "{=nMn63bV1}I am {PLAYER.NAME}. All I ask is that you remember that name, and what I did.{newline}Tell me, what will you do now?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_break_next_companion", "prison_break_next_move", "prison_break_next_move_player_companion", "{=aoJHP3Ud}I'm ready to rejoin you. I'm in your debt.", (OnConditionDelegate)(() => _prisonerHero.CompanionOf == Clan.PlayerClan), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_break_next_commander", "prison_break_next_move", "prison_break_next_move_player", "{=xADZi2bK}I'll go and find my men. I will remember your help...", (OnConditionDelegate)(() => _prisonerHero.IsCommander), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_break_next_noble", "prison_break_next_move", "prison_break_next_move_player", "{=W2vV5jzj}I'll go back to my family. I will remember your help...", (OnConditionDelegate)(() => _prisonerHero.IsLord), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_break_next_notable", "prison_break_next_move", "prison_break_next_move_player", "{=efdCZPw4}I'll go back to my work. I will remember your help...", (OnConditionDelegate)(() => _prisonerHero.IsNotable), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_break_next_other", "prison_break_next_move", "prison_break_next_move_player_other", "{=TWZ4abt5}I'll keep wandering about, as I've done before. I can make a living. No need to worry.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_break_end_dialog_3", "prison_break_next_move_player_companion", "close_window", "{=ncvB4XRL}You could join me.", (OnConditionDelegate)null, new OnConsequenceDelegate(prison_break_end_with_success_companion), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_break_end_dialog_1", "prison_break_next_move_player", "close_window", "{=rlAec9CM}Very well. Keep safe.", (OnConditionDelegate)null, new OnConsequenceDelegate(prison_break_end_with_success_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_break_end_dialog_2", "prison_break_next_move_player_other", "close_window", "{=dzXaXKaC}Very well.", (OnConditionDelegate)null, new OnConsequenceDelegate(prison_break_end_with_success_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
	}

	[GameMenuInitializationHandler("start_prison_break")]
	[GameMenuInitializationHandler("prison_break_cool_down")]
	[GameMenuInitializationHandler("settlement_prison_break_success")]
	[GameMenuInitializationHandler("settlement_prison_break_fail_player_unconscious")]
	[GameMenuInitializationHandler("settlement_prison_break_fail_prisoner_unconscious")]
	public static void game_menu_prison_menu_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName(Settlement.CurrentSettlement.SettlementComponent.WaitMeshName);
	}

	private bool prison_break_end_with_success_clan_member()
	{
		int num;
		if (_prisonerHero != null && _prisonerHero.CharacterObject == CharacterObject.OneToOneConversationCharacter)
		{
			if (_prisonerHero.CompanionOf != Clan.PlayerClan)
			{
				num = ((_prisonerHero.Clan == Clan.PlayerClan) ? 1 : 0);
				if (num == 0)
				{
					goto IL_006b;
				}
			}
			else
			{
				num = 1;
			}
			MBTextManager.SetTextVariable("SALUTATION", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_salutation", CharacterObject.OneToOneConversationCharacter), false);
		}
		else
		{
			num = 0;
		}
		goto IL_006b;
		IL_006b:
		return (byte)num != 0;
	}

	private bool prison_break_end_with_success_player_already_met()
	{
		int num;
		if (_prisonerHero != null && _prisonerHero.CharacterObject == CharacterObject.OneToOneConversationCharacter)
		{
			num = (_prisonerHero.HasMet ? 1 : 0);
			if (num != 0)
			{
				MBTextManager.SetTextVariable("SALUTATION", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_salutation", CharacterObject.OneToOneConversationCharacter), false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private bool prison_break_end_with_success_other_on_condition()
	{
		if (_prisonerHero != null)
		{
			return _prisonerHero.CharacterObject == CharacterObject.OneToOneConversationCharacter;
		}
		return false;
	}

	private void PrisonBreakEndedInternal()
	{
		ChangeRelationAction.ApplyPlayerRelation(_prisonerHero, Campaign.Current.Models.PrisonBreakModel.GetRelationRewardOnPrisonBreak(_prisonerHero), true, true);
		SkillLevelingManager.OnPrisonBreakEnd(_prisonerHero, true);
	}

	private void prison_break_end_with_success_on_consequence()
	{
		PrisonBreakEndedInternal();
		EndCaptivityAction.ApplyByEscape(_prisonerHero, Hero.MainHero, true);
		_prisonerHero = null;
	}

	private void prison_break_end_with_success_companion()
	{
		PrisonBreakEndedInternal();
		EndCaptivityAction.ApplyByEscape(_prisonerHero, Hero.MainHero, true);
		_prisonerHero.ChangeState((CharacterStates)1);
		AddHeroToPartyAction.Apply(_prisonerHero, MobileParty.MainParty, true);
		_prisonerHero = null;
	}

	private bool game_menu_castle_prison_break_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		_bribeCost = Campaign.Current.Models.PrisonBreakModel.GetPrisonBreakStartCost(_prisonerHero);
		MBTextManager.SetTextVariable("COST", _bribeCost);
		return true;
	}

	private void AddCoolDownForPrisonBreak(Settlement settlement)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		CampaignTime value = CampaignTime.DaysFromNow(7f);
		if (_coolDownData.ContainsKey(settlement))
		{
			_coolDownData[settlement] = value;
		}
		else
		{
			_coolDownData.Add(settlement, value);
		}
	}

	private bool CanPlayerStartPrisonBreak(Settlement settlement)
	{
		bool flag = true;
		if (_coolDownData.TryGetValue(settlement, out var value))
		{
			flag = ((CampaignTime)(ref value)).IsPast;
			if (flag)
			{
				_coolDownData.Remove(settlement);
			}
		}
		return flag;
	}

	private bool game_menu_stage_prison_break_on_condition(MenuCallbackArgs args)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		bool result = false;
		if (Campaign.Current.Models.PrisonBreakModel.CanPlayerStagePrisonBreak(Settlement.CurrentSettlement))
		{
			args.optionLeaveType = (LeaveType)30;
			if (Hero.MainHero.IsWounded)
			{
				args.IsEnabled = false;
				args.Tooltip = new TextObject("{=yNMrF2QF}You are wounded", (Dictionary<string, object>)null);
			}
			result = true;
		}
		return result;
	}

	private void game_menu_castle_prison_break_from_dungeon_on_consequence(MenuCallbackArgs args)
	{
		_previousMenuId = "town_keep_dungeon";
		game_menu_castle_prison_break_on_consequence(args);
	}

	private void game_menu_castle_prison_break_from_castle_dungeon_on_consequence(MenuCallbackArgs args)
	{
		_previousMenuId = "castle_dungeon";
		game_menu_castle_prison_break_on_consequence(args);
	}

	private void game_menu_castle_prison_break_from_enemy_keep_on_consequence(MenuCallbackArgs args)
	{
		_previousMenuId = "town_enemy_town_keep";
		game_menu_castle_prison_break_on_consequence(args);
	}

	private void game_menu_castle_prison_break_on_consequence(MenuCallbackArgs args)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected O, but got Unknown
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Expected O, but got Unknown
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Expected O, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Expected O, but got Unknown
		if (CanPlayerStartPrisonBreak(Settlement.CurrentSettlement))
		{
			FlattenedTroopRoster val = Settlement.CurrentSettlement.Party.PrisonRoster.ToFlattenedRoster();
			if (((Fief)Settlement.CurrentSettlement.Town).GarrisonParty != null)
			{
				val.Add(((Fief)Settlement.CurrentSettlement.Town).GarrisonParty.PrisonRoster.GetTroopRoster());
			}
			val.RemoveIf((Predicate<FlattenedTroopRosterElement>)((FlattenedTroopRosterElement x) => !((BasicCharacterObject)((FlattenedTroopRosterElement)(ref x)).Troop).IsHero));
			List<InquiryElement> list = new List<InquiryElement>();
			foreach (FlattenedTroopRosterElement item in val)
			{
				FlattenedTroopRosterElement current = item;
				TextObject val2 = null;
				bool flag = false;
				TextObject val3;
				if (FactionManager.IsAtWarAgainstFaction(Clan.PlayerClan.MapFaction, ((FlattenedTroopRosterElement)(ref current)).Troop.HeroObject.MapFaction))
				{
					val3 = new TextObject("{=!}{HERO.NAME}", (Dictionary<string, object>)null);
					StringHelpers.SetCharacterProperties("HERO", ((FlattenedTroopRosterElement)(ref current)).Troop, val3, false);
					val2 = new TextObject("{=VM1SGrla}{HERO.NAME} is your enemy.", (Dictionary<string, object>)null);
					TextObjectExtensions.SetCharacterProperties(val2, "HERO", ((FlattenedTroopRosterElement)(ref current)).Troop, false);
					flag = true;
				}
				else
				{
					int prisonBreakStartCost = Campaign.Current.Models.PrisonBreakModel.GetPrisonBreakStartCost(((FlattenedTroopRosterElement)(ref current)).Troop.HeroObject);
					flag = Hero.MainHero.Gold < prisonBreakStartCost;
					val3 = new TextObject("{=!}{HERO.NAME}", (Dictionary<string, object>)null);
					StringHelpers.SetCharacterProperties("HERO", ((FlattenedTroopRosterElement)(ref current)).Troop, val3, false);
					val2 = new TextObject("{=I4SjNT6Y}This will cost you {BRIBE_COST}{GOLD_ICON}.{?ENOUGH_GOLD}{?} You don't have enough money.{\\?}", (Dictionary<string, object>)null);
					val2.SetTextVariable("BRIBE_COST", prisonBreakStartCost);
					val2.SetTextVariable("ENOUGH_GOLD", (!flag) ? 1 : 0);
				}
				list.Add(new InquiryElement((object)((FlattenedTroopRosterElement)(ref current)).Troop, ((object)val3).ToString(), (ImageIdentifier)new CharacterImageIdentifier(CharacterCode.CreateFrom((BasicCharacterObject)(object)((FlattenedTroopRosterElement)(ref current)).Troop)), !flag, ((object)val2).ToString()));
			}
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(((object)new TextObject("{=oQjsShmH}PRISONERS", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=abpzOR0D}Choose a prisoner to break out", (Dictionary<string, object>)null)).ToString(), list, true, 1, 1, ((object)GameTexts.FindText("str_done", (string)null)).ToString(), string.Empty, (Action<List<InquiryElement>>)StartPrisonBreak, (Action<List<InquiryElement>>)null, "", false), false, false);
		}
		else
		{
			GameMenu.SwitchToMenu("prison_break_cool_down");
		}
	}

	private void StartPrisonBreak(List<InquiryElement> prisonerList)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (prisonerList.Count > 0)
		{
			_prisonerHero = ((CharacterObject)prisonerList[0].Identifier).HeroObject;
			GameMenu.SwitchToMenu("start_prison_break");
		}
		else
		{
			_prisonerHero = null;
		}
	}

	private void OpenPrisonBreakMission()
	{
		GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, _bribeCost, false);
		AddCoolDownForPrisonBreak(Settlement.CurrentSettlement);
		_launchingPrisonBreakMission = true;
		Location locationWithId = LocationComplex.Current.GetLocationWithId("prison");
		CampaignMission.OpenPrisonBreakMission(locationWithId.GetSceneName(Settlement.CurrentSettlement.Town.GetWallLevel()), locationWithId, _prisonerHero.CharacterObject);
	}

	private bool game_menu_leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private bool game_menu_continue_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		return true;
	}

	private void game_menu_cancel_prison_break(MenuCallbackArgs args)
	{
		_prisonerHero = null;
		GameMenu.SwitchToMenu(_previousMenuId);
	}

	private void start_prison_break_on_init(MenuCallbackArgs args)
	{
		StringHelpers.SetCharacterProperties("PRISONER", _prisonerHero.CharacterObject, (TextObject)null, false);
	}

	private void settlement_prison_break_success_on_init(MenuCallbackArgs args)
	{
		StringHelpers.SetCharacterProperties("PRISONER", _prisonerHero.CharacterObject, (TextObject)null, false);
		MBTextManager.SetTextVariable("SETTLEMENT_TYPE", Settlement.CurrentSettlement.IsTown ? 1 : 0);
	}

	private void settlement_prison_break_success_continue_on_consequence(MenuCallbackArgs args)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		PlayerEncounter.LeaveSettlement();
		PlayerEncounter.Finish(true);
		CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, false, false, false, false, false, false), new ConversationCharacterData(_prisonerHero.CharacterObject, (PartyBase)null, false, false, false, false, false, false));
	}

	private void settlement_prison_break_fail_prisoner_injured_on_init(MenuCallbackArgs args)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (_prisonerHero.IsDead)
		{
			TextObject val = new TextObject("{=GkwOyJn9}{newline}You later learn that {?PRISONER.GENDER}she{?}he{\\?} died from {?PRISONER.GENDER}her{?}his{\\?} injuries.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PRISONER", _prisonerHero.CharacterObject, val, false);
			MBTextManager.SetTextVariable("INFORMATION_IF_PRISONER_DEAD", val, false);
		}
		StringHelpers.SetCharacterProperties("PRISONER", _prisonerHero.CharacterObject, (TextObject)null, false);
	}

	private void settlement_prison_break_fail_on_init(MenuCallbackArgs args)
	{
		StringHelpers.SetCharacterProperties("PRISONER", _prisonerHero.CharacterObject, (TextObject)null, false);
	}

	private void settlement_prison_break_fail_player_unconscious_continue_on_consequence(MenuCallbackArgs args)
	{
		SkillLevelingManager.OnPrisonBreakEnd(_prisonerHero, false);
		Settlement currentSettlement = Settlement.CurrentSettlement;
		PlayerEncounter.LeaveSettlement();
		PlayerEncounter.Finish(true);
		TakePrisonerAction.Apply(currentSettlement.Party, Hero.MainHero);
		_prisonerHero = null;
	}

	private void settlement_prison_break_fail_prisoner_unconscious_continue_on_consequence(MenuCallbackArgs args)
	{
		SkillLevelingManager.OnPrisonBreakEnd(_prisonerHero, false);
		_prisonerHero = null;
		PlayerEncounter.LeaveSettlement();
		PlayerEncounter.Finish(true);
	}
}
