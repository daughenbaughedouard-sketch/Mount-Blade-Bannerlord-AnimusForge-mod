using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Missions;
using NavalDLC.Storyline.MissionControllers;
using SandBox;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace NavalDLC.Storyline.Quests;

public class SpeakToTheSailorsQuest : NavalStorylineQuestBase
{
	public class SpeakToTheSailorsQuestTypeDefiner : SaveableTypeDefiner
	{
		public SpeakToTheSailorsQuestTypeDefiner()
			: base(312250)
		{
		}

		protected override void DefineClassTypes()
		{
		}

		protected override void DefineEnumTypes()
		{
			((SaveableTypeDefiner)this).AddEnumDefinition(typeof(QuestState), 100, (IEnumResolver)null);
		}
	}

	[Flags]
	private enum QuestState
	{
		None = 0,
		TalkedToSailors = 1,
		BattleStarted = 2,
		BattleWon = 4,
		CheckpointReached = 8,
		HadEncounterWithBjolgor = 0x10
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<Agent, bool> _003C_003E9__62_4;

		public static OnConditionDelegate _003C_003E9__62_2;

		public static OnConditionDelegate _003C_003E9__62_3;

		public static OnConditionDelegate _003C_003E9__66_0;

		public static Predicate<Clan> _003C_003E9__78_0;

		internal bool _003CAddBjolgurDialogs_003Eb__62_4(Agent x)
		{
			return (object)x.Character == NavalStorylineData.Gunnar.CharacterObject;
		}

		internal bool _003CAddBjolgurDialogs_003Eb__62_2()
		{
			MBTextManager.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest3TargetSettlement.EncyclopediaLinkWithName, false);
			return true;
		}

		internal bool _003CAddBjolgurDialogs_003Eb__62_3()
		{
			MBTextManager.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest3TargetSettlement.EncyclopediaLinkWithName, false);
			return true;
		}

		internal bool _003CAddBjolgurDialogsEndBattle_003Eb__66_0()
		{
			MBTextManager.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest3TargetSettlement.EncyclopediaLinkWithName, false);
			return true;
		}

		internal bool _003CCreateHoundsParty_003Eb__78_0(Clan x)
		{
			return ((MBObjectBase)x).StringId == "northern_pirates";
		}
	}

	private const string SeaHoundsTemplateStringId = "storyline_act3_quest_3_sea_hounds_template";

	private const string MerchantsTemplateStringId = "storyline_act3_quest_3_merchants_template";

	private const string InterceptedMenuId = "hounds_3_intercepted";

	private const string EncounterMenuId = "quest3_encounter_invisible_menu";

	private const string BattleScene = "naval_storyline_act_3_quest_3";

	private const string ShipBallistaSlotId = "fore";

	private const string ShipSailSlotId = "sail";

	private const string BurningShipBallistaId = "fore_heavy_ballista_pot";

	private const string ExplosiveShipBallistaId = "fore_heavy_ballista_pot";

	private const string GalleySailId = "sails_lvl2";

	public const string FishingShipId = "burning_fishing_ship";

	public const string BurningTradeCogId = "burning_cog_ship";

	public const string TradeCogId = "ship_trade_cog_q3";

	private PartyTemplateObject _houndsTemplate;

	private PartyTemplateObject _merchantsTemplate;

	[SaveableField(0)]
	private Settlement _settlement;

	[SaveableField(1)]
	private MobileParty _houndsParty;

	private MobileParty _merchantParty;

	[SaveableField(2)]
	private QuestState _state;

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=ebFg8V9z}Speak to the Sailors in {SETTLEMENT_NAME}", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT_NAME", _settlement.Name);
			return val;
		}
	}

	protected override string MainPartyTemplateStringId => "storyline_act3_quest_3_main_party_template";

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act3SpeakToSailors;

	public override bool WillProgressStoryline => true;

	public SpeakToTheSailorsQuest(string questId, Settlement targetSettlement)
		: base(questId, NavalStorylineData.Gunnar, CampaignTime.Never, 0)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_settlement = targetSettlement;
	}

	protected override void InitializeQuestOnGameLoadInternal()
	{
		InitializeTemplates();
		((QuestBase)this).SetDialogs();
		AddGameMenus();
	}

	protected override void SetDialogs()
	{
		AddTalkToGangradirDialogue();
		AddBjolgurDialogs();
		AddBjolgurSecondConversationDialogs();
		AddGunnarHorsebackDialogs();
		AddBjolgurDialogsEndBattle();
	}

	protected override void OnStartQuestInternal()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		InitializeTemplates();
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		TextObject val = new TextObject("{=ZDDXZcMW}Gunnar has learned that the Sea Hounds will be targeting a ship that sails from the estuary near {SETTLEMENT_LINK}, bringing Sturgian silver to the Skolderbroda.", (Dictionary<string, object>)null);
		val.SetTextVariable("SETTLEMENT_LINK", _settlement.EncyclopediaLinkWithName);
		NavalStorylineData.Bjolgur.ChangeState((CharacterStates)1);
		TeleportHeroAction.ApplyImmediateTeleportToSettlement(NavalStorylineData.Bjolgur, _settlement);
		((QuestBase)this).AddLog(val, false);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_settlement);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)NavalStorylineData.Bjolgur);
	}

	private void InitializeTemplates()
	{
		_houndsTemplate = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_3_sea_hounds_template");
		_merchantsTemplate = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_3_merchants_template");
	}

	protected override void RegisterEventsInternal()
	{
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		if (party == MobileParty.MainParty && settlement == NavalStorylineData.Act3Quest3TargetSettlement && !HadEncounterWithBjolgur())
		{
			StartConversationOnSettlementEntered();
		}
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (args.MenuContext.GameMenu.StringId == "naval_storyline_virtualport" && ((QuestBase)this).IsOngoing && Settlement.CurrentSettlement == _settlement)
		{
			if (!HasTalkedToSailors())
			{
				TextObject val = new TextObject("{=4PUz4yQv}You have arrived in {SETTLEMENT_LINK}. As you sail up the estuary into the harbor, you spot several large ships at anchor in a cove. They look like Vlandian craft, probably the pirates that Fahda told you about. They do not try to give chase, however, possibly because they saw you too late to raise sail, or perhaps because they are lying in wait for more lucrative prey.", (Dictionary<string, object>)null);
				val.SetTextVariable("SETTLEMENT_LINK", _settlement.EncyclopediaLinkWithName);
				MBTextManager.SetTextVariable("VIRTUAL_PORT_TEXT", val, false);
			}
			else
			{
				MobileParty.MainParty.SetSailAtPosition(Settlement.CurrentSettlement.PortPosition);
				PlayerEncounter.Finish(true);
			}
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		if (PlayerEncounter.Current == null)
		{
			return;
		}
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		MobileParty houndsParty = _houndsParty;
		if (encounteredParty != ((houndsParty != null) ? houndsParty.Party : null))
		{
			return;
		}
		if (PlayerEncounter.CampaignBattleResult != null && PlayerEncounter.CampaignBattleResult.BattleResolved)
		{
			if (!PlayerEncounter.CampaignBattleResult.PlayerDefeat && PlayerEncounter.CampaignBattleResult.PlayerVictory)
			{
				((QuestBase)this).AddLog(new TextObject("{=bWqvK0iY}You were able to run the Sea Hound blockade.", (Dictionary<string, object>)null), false);
				AddState(QuestState.BattleWon);
			}
		}
		else if ((int)PlayerEncounter.WinningSide != -1)
		{
			Debug.FailedAssert("unhandled case", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\Quests\\SpeakToTheSailorsQuest.cs", "OnMissionEnded", 213);
		}
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (party.IsMainParty && HasTalkedToSailors() && NavalStorylineData.IsNavalStoryLineActive() && !HasBattleStarted() && MobileParty.MainParty.IsCurrentlyAtSea)
		{
			GameMenu.ActivateGameMenu("hounds_3_intercepted");
		}
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
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_007d: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00b2: Expected O, but got Unknown
		//IL_00b2: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_011c: Expected O, but got Unknown
		//IL_011c: Expected O, but got Unknown
		((QuestBase)this).AddGameMenu("hounds_3_intercepted", new TextObject("{=lbLABNVY}You row out of {SETTLEMENT_LINK} harbor, with the Sturgian merchantmen following close behind you, and make your way toward the sea. But as you reach the estuary mouth, you see several ominous squat shapes blocking your passage to the open sea. Clearly it is the Sea Hounds, and you will either have to defeat them or hold them off long enough for your allies to make good their escape.", (Dictionary<string, object>)null), new OnInitDelegate(intercepted_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("hounds_3_intercepted", "continue", new TextObject("{=1r0tDsrR}Attack!", (Dictionary<string, object>)null), new OnConditionDelegate(intercepted_menu_on_condition), new OnConsequenceDelegate(intercepted_menu_on_consequence), false, -1);
		((QuestBase)this).AddGameMenu("quest3_encounter_invisible_menu", new TextObject("{=!}{RETRY_DESC}", (Dictionary<string, object>)null), new OnInitDelegate(quest3_encounter_invisible_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("quest3_encounter_invisible_menu", "retry", new TextObject("{=YHMDy3lQ}Try again", (Dictionary<string, object>)null), new OnConditionDelegate(on_retry_condition), new OnConsequenceDelegate(on_retry_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("quest3_encounter_invisible_menu", "retry_checkpoint", new TextObject("{=rHlzkNFL}Try again from checkpoint", (Dictionary<string, object>)null), new OnConditionDelegate(on_retry_from_checkpoint_condition), new OnConsequenceDelegate(on_retry_from_checkpoint_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("quest3_encounter_invisible_menu", "leave", new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null), new OnConditionDelegate(on_leave_condition), new OnConsequenceDelegate(on_leave_consequence), true, -1);
	}

	private void StartConversationOnSettlementEntered()
	{
		PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("port"), (Location)null, NavalStorylineData.Bjolgur.CharacterObject, (string)null);
	}

	private void on_leave_consequence(MenuCallbackArgs args)
	{
		((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
	}

	private bool on_leave_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return HasBattleStarted();
	}

	private bool on_retry_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		if (HasBattleStarted())
		{
			return !CheckPointReached();
		}
		return false;
	}

	private bool on_retry_from_checkpoint_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		if (HasBattleStarted())
		{
			return CheckPointReached();
		}
		return false;
	}

	private void on_retry_consequence(MenuCallbackArgs args)
	{
		StartBattle(fromCheckPoint: false);
	}

	private void on_retry_from_checkpoint_consequence(MenuCallbackArgs args)
	{
		StartBattle(fromCheckPoint: true);
	}

	private void quest3_encounter_invisible_menu_on_init(MenuCallbackArgs args)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		MBTextManager.SetTextVariable("RETRY_DESC", new TextObject("{=etH1IHNZ}You manage to put some distance between you and your enemies, and you have a moment to consider how to proceed.", (Dictionary<string, object>)null), false);
		DestroyParty(ref _merchantParty);
		if (!HasBattleWon())
		{
			RefreshParty(_houndsParty, _houndsTemplate);
			RefreshParty(MobileParty.MainParty, base.Template);
			AddBurningTradeShipsToParties();
		}
		if (((QuestBase)this).IsOngoing)
		{
			if (NavalStorylineData.IsNavalStoryLineActive() && HasBattleWon())
			{
				TalkToBjolgur();
			}
			else if (!HasBattleStarted())
			{
				StartBattle(fromCheckPoint: false);
			}
		}
		else
		{
			GameMenu.ExitToLast();
		}
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest3EncounterMenu);
	}

	private void RefreshParty(MobileParty mobileParty, PartyTemplateObject pt)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		MBList<TroopRosterElement> troopRoster = mobileParty.MemberRoster.GetTroopRoster();
		for (int i = 0; i < ((List<TroopRosterElement>)(object)troopRoster).Count; i++)
		{
			if (((BasicCharacterObject)((List<TroopRosterElement>)(object)troopRoster)[i].Character).IsHero)
			{
				((List<TroopRosterElement>)(object)troopRoster)[i].Character.HeroObject.Heal(((List<TroopRosterElement>)(object)troopRoster)[i].Character.HeroObject.MaxHitPoints, false);
				continue;
			}
			TroopRoster memberRoster = mobileParty.MemberRoster;
			CharacterObject character = ((List<TroopRosterElement>)(object)troopRoster)[i].Character;
			TroopRosterElement val = ((List<TroopRosterElement>)(object)troopRoster)[i];
			memberRoster.RemoveTroop(character, ((TroopRosterElement)(ref val)).Number, default(UniqueTroopDescriptor), 0);
		}
		TroopRoster val2 = Campaign.Current.Models.PartySizeLimitModel.FindAppropriateInitialRosterForMobileParty(mobileParty, pt);
		mobileParty.MemberRoster.Add(val2);
		HealShips(mobileParty);
	}

	private void HealShips(MobileParty mobileParty)
	{
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			item.HitPoints = item.MaxHitPoints;
		}
	}

	private void intercepted_menu_on_init(MenuCallbackArgs args)
	{
		MBTextManager.SetTextVariable("SETTLEMENT_LINK", _settlement.EncyclopediaLinkWithName, false);
		if (_houndsParty == null)
		{
			CreateHoundsParty();
		}
	}

	[GameMenuInitializationHandler("hounds_3_intercepted")]
	private static void intercepted_menu_background_on_init(MenuCallbackArgs args)
	{
		Settlement val = Settlement.CurrentSettlement ?? MobileParty.MainParty.LastVisitedSettlement;
		args.MenuContext.SetBackgroundMeshName(((MBObjectBase)val.Culture).StringId + "_port");
	}

	[GameMenuInitializationHandler("quest3_encounter_invisible_menu")]
	private static void encounter_menu_background_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
	}

	private void intercepted_menu_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.ActivateGameMenu("quest3_encounter_invisible_menu");
	}

	private void AddBurningTradeShipsToParties()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		ShipHull tradeCogHull = MBObjectManager.Instance.GetObject<ShipHull>("burning_cog_ship");
		ShipHull normalCogHull = MBObjectManager.Instance.GetObject<ShipHull>("ship_trade_cog_q3");
		ShipHull fishingShipHull = MBObjectManager.Instance.GetObject<ShipHull>("burning_fishing_ship");
		if (!((IEnumerable<Ship>)MobileParty.MainParty.Ships).Any((Ship x) => x.ShipHull == normalCogHull))
		{
			Ship val = new Ship(normalCogHull);
			ChangeShipOwnerAction.ApplyByLooting(PartyBase.MainParty, val);
		}
		if (!((IEnumerable<Ship>)MobileParty.MainParty.Ships).Any((Ship x) => x.ShipHull == fishingShipHull))
		{
			Ship val2 = new Ship(fishingShipHull);
			ChangeShipOwnerAction.ApplyByLooting(PartyBase.MainParty, val2);
		}
		if (!((IEnumerable<Ship>)_houndsParty.Ships).Any((Ship x) => x.ShipHull == tradeCogHull))
		{
			Ship val3 = new Ship(tradeCogHull);
			val3.EquipUpgradePiece("fore", MBObjectManager.Instance.GetObject<ShipUpgradePiece>("fore_heavy_ballista_pot"));
			ChangeShipOwnerAction.ApplyByLooting(_houndsParty.Party, val3);
		}
	}

	private bool intercepted_menu_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		return true;
	}

	protected override void HourlyTick()
	{
	}

	protected override void OnFinalizeInternal()
	{
		if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)_settlement))
		{
			((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)_settlement);
		}
		int num;
		if (PlayerEncounter.EncounteredParty != null)
		{
			PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
			MobileParty houndsParty = _houndsParty;
			num = ((encounteredParty == ((houndsParty != null) ? houndsParty.Party : null)) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		DestroyParty(ref _houndsParty);
		DestroyParty(ref _merchantParty);
		if (NavalStorylineData.Bjolgur.IsActive)
		{
			RemoveHero(NavalStorylineData.Bjolgur);
		}
		if (num != 0)
		{
			PlayerEncounter.Finish(true);
		}
		for (int num2 = ((List<Ship>)(object)MobileParty.MainParty.Ships).Count - 1; num2 >= 0; num2--)
		{
			if (((MBObjectBase)((List<Ship>)(object)MobileParty.MainParty.Ships)[num2].ShipHull).StringId == "burning_fishing_ship")
			{
				DestroyShipAction.Apply(((List<Ship>)(object)MobileParty.MainParty.Ships)[num2]);
			}
		}
	}

	protected override void OnCompleteWithSuccessInternal()
	{
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest3Succeeded);
	}

	protected override void IsNavalQuestPartyInternal(PartyBase party, NavalStorylinePartyData data)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		MobileParty houndsParty = _houndsParty;
		ExplainedNumber maxPartySizeLimitFromTemplate;
		if (party == ((houndsParty != null) ? houndsParty.Party : null))
		{
			maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(_houndsTemplate);
			data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber;
			data.Template = _houndsTemplate;
			data.IsQuestParty = true;
			return;
		}
		MobileParty merchantParty = _merchantParty;
		if (party == ((merchantParty != null) ? merchantParty.Party : null))
		{
			maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(_merchantsTemplate);
			data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber;
			data.Template = _merchantsTemplate;
			data.IsQuestParty = true;
		}
	}

	private void AddTalkToGangradirDialogue()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 250).NpcLine("{=O0qBJmSS}Talk with Bjolgur when you're ready to depart.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => ((QuestBase)this).IsOngoing && HadEncounterWithBjolgur() && Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && !HasTalkedToSailors()))
			.CloseDialog(), (object)this);
	}

	private void AddBjolgurSecondConversationDialogs()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1300).NpcLine("{=GkaEhSwJ}{PLAYER.NAME}...", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => ((QuestBase)this).IsOngoing && HadEncounterWithBjolgur() && Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == NavalStorylineData.Bjolgur && !HasTalkedToSailors()))
			.NpcLine("{=zNaWTBin}Are you ready to take command of the fireship and break the blockade?", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=anANUCFV}I am as ready as I will ever be, I suppose.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(OnTalkedToSailors))
			.CloseDialog()
			.PlayerOption("{=6c2bHHHj}No, not yet.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog(), (object)this);
	}

	private void AddBjolgurDialogs()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_007b: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00c3: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		//IL_0168: Expected O, but got Unknown
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		//IL_018c: Expected O, but got Unknown
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_01b0: Expected O, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected O, but got Unknown
		//IL_01d4: Expected O, but got Unknown
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Expected O, but got Unknown
		//IL_01f8: Expected O, but got Unknown
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1300).NpcLine("{=J6QLFwbb}Welcome to {SETTLEMENT_LINK}, friend. Is that grizzled fellow with you, coming up now, is that my old comrade Gunnar of Lagshofn? A bit greyer than I remember from the days when we stood together in the shield wall facing Volbjorn's host, but, well, aren't we all…", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			MBTextManager.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest3TargetSettlement.EncyclopediaLinkWithName, false);
			int num;
			if (((QuestBase)this).IsOngoing && !HadEncounterWithBjolgur() && Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == NavalStorylineData.Bjolgur)
			{
				num = ((!HasTalkedToSailors()) ? 1 : 0);
				if (num != 0)
				{
					Agent val3 = ((IEnumerable<Agent>)Mission.Current.Agents).FirstOrDefault((Func<Agent, bool>)((Agent x) => (object)x.Character == NavalStorylineData.Gunnar.CharacterObject));
					if (!Campaign.Current.ConversationManager.ConversationAgents.Contains((IAgent)(object)val3))
					{
						AddGunnarToConversation(isAgentSpawned: true);
					}
					val3.TeleportToPosition(GetGunnarTeleportPosition());
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		})
			.Consequence((OnConsequenceDelegate)delegate
			{
				AddState(QuestState.HadEncounterWithBjolgor);
			})
			.NpcLine("{=KYqqVZh1}We received his letter a while back, about your run-in with Purig. Hah! That worm must have cursed like an old woman when he learned that his captives stole his ship. You two are making quite a name for yourselves.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=4bsY9noo}Bjolgur of Gauksdal! Well met! Are the Skolderbroda working for the merchants of {SETTLEMENT_LINK} now?", new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__62_2;
		if (obj2 == null)
		{
			OnConditionDelegate val = delegate
			{
				MBTextManager.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest3TargetSettlement.EncyclopediaLinkWithName, false);
				return true;
			};
			_003C_003Ec._003C_003E9__62_2 = val;
			obj2 = (object)val;
		}
		DialogFlow obj3 = obj.Condition((OnConditionDelegate)obj2).NpcLine("{=lTjvOdoX}Not yet. As you know, our brotherhood does not fight before it's paid.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null).NpcLine("{=iSKIBXnj}See, the {SETTLEMENT_LINK} merchants promised us a hoard of silver to protect their ships from the Sea Hounds, but it never arrived. I was sent down to learn what was going on, and I find the silver just sitting here, loaded onto a ship in the harbor, and the Sturgians are burning through it paying their men double wages not to run off. Some Vlandian pirates were sighted in the estuary, and the Sturgians refuse to venture out.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null);
		object obj4 = _003C_003Ec._003C_003E9__62_3;
		if (obj4 == null)
		{
			OnConditionDelegate val2 = delegate
			{
				MBTextManager.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest3TargetSettlement.EncyclopediaLinkWithName, false);
				return true;
			};
			_003C_003Ec._003C_003E9__62_3 = val2;
			obj4 = (object)val2;
		}
		string text = default(string);
		conversationManager.AddDialogFlow(obj3.Condition((OnConditionDelegate)obj4).GenerateToken(ref text).BeginPlayerOptions((string)null, false)
			.PlayerOption("{=325GxBag}With so much wealth at stake, the Sturgians are right to be cautious.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption("{=2YEmSZq1}Pirates are scum. Let's just sail out and crush them.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.EndPlayerOptions()
			.NpcLine("{=kbug6MQB}Much as I would like to simply sail forth and bathe my sword in Sea Hound blood, my brotherhood has commanded me to do my best to ensure that the silver gets through safely.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), text, (string)null)
			.NpcLine("{=rlpVWadN}Listen. I've been watching these Vlandian blockaders, and mulling over a plan. Their flagship has a lofty deck and it would be hard to board, but it doesn't seem very maneuverable. I think we can hit them with a trick that can be deadly in estuaries.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null)
			.NpcLine("{=K3B52zD6}We will be upstream of them. I'll have the merchants here donate some leaky old vessel that they are about to scrap. We load it up with oil and pitch. Then we steer it towards the pirates, throw a torch in the hull, and jump.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null)
			.NpcLine("{=8PmocyQy}Good, very good. With luck, the current shall carry it right into them, and they shall all merrily blaze up like a bonfire at a midwinter feast. The silver ship will make for the open sea, while the rest of us can have it out with any surviving Sea Hounds.", new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.NpcLine("{=867iaibq}Listen, though… We need someone to steer the fireship. I'd do it myself, but my order wants me to stay close to the silver. I'd found a few volunteers who've offered to do it, but they keep sobering up.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=ybDSa8Xr}I'll steer the fireship. Let us sail forth.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(OnTalkedToSailors))
			.CloseDialog()
			.PlayerOption("{=brMsnacx}I need a little while here in port first.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
	}

	private void AddGunnarHorsebackDialogs()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1300).NpcLine("{=GkaEhSwJ}{PLAYER.NAME}...", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(gunnar_horseback_dialog_on_condition))
			.NpcLine("{=ypTUg9xC}There may be some Hound patrols about. Keep a wary eye.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += gunnar_horseback_dialog_on_consequence;
			})
			.CloseDialog(), (object)this);
	}

	private bool gunnar_horseback_dialog_on_condition()
	{
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
		if (((QuestBase)this).IsOngoing && Mission.Current != null && Hero.OneToOneConversationHero == NavalStorylineData.Gunnar)
		{
			BlockedEstuaryMissionController missionBehavior = Mission.Current.GetMissionBehavior<BlockedEstuaryMissionController>();
			if (missionBehavior != null && missionBehavior.CurrentPhase == BlockedEstuaryMissionController.BattlePhase.Phase2)
			{
				return true;
			}
		}
		return false;
	}

	private void gunnar_horseback_dialog_on_consequence()
	{
		Mission.Current.GetMissionBehavior<BlockedEstuaryMissionController>().OnTalkedToGunnarPhase2();
	}

	private void AddBjolgurDialogsEndBattle()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_003d: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0072: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_012c: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		//IL_0150: Expected O, but got Unknown
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		//IL_0174: Expected O, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected O, but got Unknown
		//IL_0198: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Expected O, but got Unknown
		//IL_01bc: Expected O, but got Unknown
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Expected O, but got Unknown
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Expected O, but got Unknown
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Expected O, but got Unknown
		//IL_022b: Expected O, but got Unknown
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Expected O, but got Unknown
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Expected O, but got Unknown
		//IL_0267: Expected O, but got Unknown
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Expected O, but got Unknown
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_02a3: Expected O, but got Unknown
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Expected O, but got Unknown
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Expected O, but got Unknown
		//IL_02de: Expected O, but got Unknown
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Expected O, but got Unknown
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Expected O, but got Unknown
		//IL_031a: Expected O, but got Unknown
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Expected O, but got Unknown
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Expected O, but got Unknown
		//IL_0356: Expected O, but got Unknown
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1300).NpcLine("{=8OtmPWCK}So! {PLAYER.NAME}... You did well with that fireship! The silver is on its way to my order, and that bastard Purig will no doubt be much discomfitted. You helped me out there, so let me see if I can now help you.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null).Condition(new OnConditionDelegate(MultiAgentConversationCondition))
			.NpcLine("{=5GMbKn4x}Just before I set sail for {SETTLEMENT_LINK}, my brothers and I had a visitor, a merchant named Salautas Crusas who said he was acting as an “ambassador” for Purig. He wanted us to break our contract with Balgard and ally with the Sea Hounds instead. He offered a great deal of money, too, and more - we could share in Purig's grand plan of conquest.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__66_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = delegate
			{
				MBTextManager.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest3TargetSettlement.EncyclopediaLinkWithName, false);
				return true;
			};
			_003C_003Ec._003C_003E9__66_0 = val;
			obj2 = (object)val;
		}
		string text = default(string);
		string text2 = default(string);
		string text3 = default(string);
		string text4 = default(string);
		string text5 = default(string);
		string text6 = default(string);
		conversationManager.AddDialogFlow(obj.Condition((OnConditionDelegate)obj2).GenerateToken(ref text).GenerateToken(ref text2)
			.GenerateToken(ref text3)
			.GenerateToken(ref text4)
			.GenerateToken(ref text5)
			.GenerateToken(ref text6)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=0EVkbp01}What grand plans?", new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption("{=jce9rAAu}I'm not interested in Purig's lies, just how to find him.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.GotoDialogState(text2)
			.EndPlayerOptions()
			.NpcLine("{=n4bIAwNN}Well, first we would join the Sea Hounds in ravaging the coasts of Sturgia and Vlandia, so that no ship would dare sail on the Byalic Sea without paying us our due. Then Purig would raise an army out of the king's old enemies and take the Nordvyg, and crown himself in Thronderlag, and shower upon us lands, and titles, and anything else we might want.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), text, (string)null)
			.NpcLine("{=2oEhDTjU}Well, some of the brothers listened to him, men who had fought against Volbjorn to whom a fine meal of wealth seasoned with revenge sounded rather tasty. But the rest of us… We'd heard such promises before, and we had no wish to serve any king. Better to fight for gold… and if you want the gold to flow, you honor your contracts, even if some fancy Calradian merchant comes along offering you the riches of the seven seas.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, text3)
			.NpcLine("{=3mxtyo2y}Here's the detail that would interest you…. In addition to all the other delights that Crusas dangled before us, he also offered to build us ships. Purig was going to construct them in some northern anchorage called Angranfjord, where he had brought a large number of captives to work in a shipyard.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), text2, text3)
			.NpcLine("{=GlV3EsEv}This must be the slave colony that Fahda mentioned. Pirates value safe havens to build new ships. With an anchorage like that, Purig can have the Sea Hounds out of his hands.", new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainAgent), text3, text4)
			.NpcLine("{=v2664Qeo}...", new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainAgent), text4, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=WtODG7Mc}Bjolgur... you've known this for some time, you say?", new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.GotoDialogState(text5)
			.PlayerOption("{=X14bPFvN}Why didn't you tell us this before the battle?", new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.GotoDialogState(text6)
			.EndPlayerOptions()
			.NpcLine("{=7UNOf0DZ}Come now, I couldn't have you dash off to hunt Crusas before the silver got past the Sea Hounds. My brothers named me their emissary, you see, and we diplomats need to be crafty.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), text5, (string)null)
			.PlayerLine("{=l8Rbjazw}It sounds as though, if we find Crusas, we can find Purig.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.NpcLine("{=vhr55efV}So… I need to get this silver safely to harbor, but after that, I shall request permission from my order to fit out a ship and sail to Ostican to join your hunt. I'm not saying I owe you anything, mind you - but those bastards did try to take our money, and all Crusas' talk about gold and riches made me think that I wouldn't mind taking one of his ships and having a rummage through his holds.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null)
			.PlayerLine("{=JEpBDamz}We are grateful for your help. We shall meet you back in Ostican.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.NpcLine("{=Sl45Pmxg}I shall see you shortly in Ostican, then.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += FinishQuest;
			})
			.CloseDialog()
			.NpcLine("{=7UNOf0DZ}Come now, I couldn't have you dash off to hunt Crusas before the silver got past the Sea Hounds. My brothers named me their emissary, you see, and we diplomats need to be crafty.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), text6, (string)null)
			.PlayerLine("{=U9e7WbOS}I piloted a fireship. I think you owe us more than just information.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.NpcLine("{=vhr55efV}So… I need to get this silver safely to harbor, but after that, I shall request permission from my order to fit out a ship and sail to Ostican to join your hunt. I'm not saying I owe you anything, mind you - but those bastards did try to take our money, and all Crusas' talk about gold and riches made me think that I wouldn't mind taking one of his ships and having a rummage through his holds.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null)
			.PlayerLine("{=8zxLaxKn}You'll get your share of Crusas' ill-gained wealth, never fear.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.NpcLine("{=Sl45Pmxg}I shall see you shortly in Ostican, then.", new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += FinishQuest;
			})
			.CloseDialog()
			.CloseDialog(), (object)this);
	}

	private void TalkToBjolgur()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Campaign.Current.CampaignMissionManager.OpenConversationMission(new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, true, false, false, false, true), new ConversationCharacterData(NavalStorylineData.Bjolgur.CharacterObject, PartyBase.MainParty, true, true, false, false, false, true), "conversation_scene_sea_multi_agent", "", true);
	}

	private bool MultiAgentConversationCondition()
	{
		if (((QuestBase)this).IsOngoing && Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == NavalStorylineData.Bjolgur && HasBattleWon() && HasTalkedToSailors())
		{
			AddGunnarToConversation(isAgentSpawned: false);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
			return true;
		}
		return false;
	}

	private Vec3 GetGunnarTeleportPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Vec3 randomPositionAroundPoint = Mission.Current.GetRandomPositionAroundPoint(Agent.Main.Position + Agent.Main.LookRotation.s * 3f, 1f, 1.5f, false);
		int num = 20;
		while (Mission.Current.Scene.GetNavigationMeshForPosition(ref randomPositionAroundPoint) == UIntPtr.Zero && num > 0)
		{
			randomPositionAroundPoint = Mission.Current.GetRandomPositionAroundPoint(Agent.Main.Position + Agent.Main.LookRotation.s * 3f, 1f, 1.5f, false);
			num--;
		}
		return randomPositionAroundPoint;
	}

	private void AddGunnarToConversation(bool isAgentSpawned)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Agent item;
		if (!isAgentSpawned)
		{
			AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject);
			val.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
			Vec3 globalPosition = Mission.Current.Scene.FindEntityWithName("free_infantry_spawn_point_0").GlobalPosition;
			val.InitialPosition(ref globalPosition);
			Vec3 lookDirection = Agent.Main.LookDirection;
			Vec2 val2 = ((Vec3)(ref lookDirection)).AsVec2;
			val2 = ((Vec2)(ref val2)).Normalized();
			val.InitialDirection(ref val2);
			val.NoHorses(true);
			item = Mission.Current.SpawnAgent(val, false);
		}
		else
		{
			item = ((IEnumerable<Agent>)Mission.Current.Agents).FirstOrDefault((Func<Agent, bool>)((Agent x) => IsGunnar((IAgent)(object)x)));
			RemoveWalkingBehavior(NavalStorylineData.Gunnar.CharacterObject);
			RemoveWalkingBehavior(NavalStorylineData.Bjolgur.CharacterObject);
		}
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		MBList<IAgent> obj = new MBList<IAgent>();
		((List<IAgent>)(object)obj).Add((IAgent)(object)item);
		conversationManager.AddConversationAgents((IEnumerable<IAgent>)obj, true);
	}

	private void RemoveWalkingBehavior(CharacterObject character)
	{
		Agent? obj = ((IEnumerable<Agent>)Mission.Current.Agents).FirstOrDefault((Func<Agent, bool>)((Agent x) => (object)x.Character == character));
		CampaignAgentComponent component = obj.GetComponent<CampaignAgentComponent>();
		obj.ClearTargetFrame();
		AgentNavigator agentNavigator = component.AgentNavigator;
		if (agentNavigator != null)
		{
			DailyBehaviorGroup behaviorGroup = agentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
			if (behaviorGroup != null)
			{
				((AgentBehaviorGroup)behaviorGroup).RemoveBehavior<WalkingBehavior>();
			}
		}
	}

	private void FinishQuest()
	{
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private bool IsGunnar(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
	}

	private bool IsBjolgur(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
	}

	private bool IsMainAgent(IAgent agent)
	{
		return (object)agent == Agent.Main;
	}

	private void RemoveHero(Hero hero)
	{
		hero.ChangeState((CharacterStates)6);
		LocationComplex current = LocationComplex.Current;
		if (current != null)
		{
			current.RemoveCharacterIfExists(hero);
		}
		LeaveSettlementAction.ApplyForCharacterOnly(hero);
	}

	private void OnTalkedToSailors()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		AddState(QuestState.TalkedToSailors);
		TextObject val = new TextObject("{=FOQ5YOWH}You talked to {HERO.NAME}, and agreed to pilot a fireship and help the Sturgians run the Sea Hound blockade.", (Dictionary<string, object>)null);
		TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.Bjolgur.CharacterObject, false);
		((QuestBase)this).AddLog(val, false);
		Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
		{
			RemoveHero(NavalStorylineData.Bjolgur);
			Mission.Current.EndMission();
		};
	}

	private void CreateHoundsParty()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 val = NavigationHelper.FindPointAroundPosition(MobileParty.MainParty.Position, (NavigationType)2, 3f, 1f, true, false);
		TextObject val2 = new TextObject("{=27QTvW27}Vlandian Pirates", (Dictionary<string, object>)null);
		_houndsParty = CustomPartyComponent.CreateCustomPartyWithPartyTemplate(val, 1f, _settlement, val2, Clan.FindFirst((Predicate<Clan>)((Clan x) => ((MBObjectBase)x).StringId == "northern_pirates")), _houndsTemplate, (Hero)null, "", "", 0f, false);
		_houndsParty.SetPartyUsedByQuest(true);
		_houndsParty.IsInfoHidden = true;
		_houndsParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
		_houndsParty.Party.SetCustomBanner(NavalStorylineData.CorsairBanner);
		ShipUpgradePiece val3 = MBObjectManager.Instance.GetObject<ShipUpgradePiece>("fore_heavy_ballista_pot");
		ShipUpgradePiece val4 = MBObjectManager.Instance.GetObject<ShipUpgradePiece>("sails_lvl2");
		ShipUpgradePiece val5 = MBObjectManager.Instance.GetObject<ShipUpgradePiece>("fore_heavy_ballista_pot");
		foreach (Ship item in (List<Ship>)(object)_houndsParty.Ships)
		{
			if (item.HasSlot("fore"))
			{
				if (((MBObjectBase)item.ShipHull).StringId == "burning_cog_ship")
				{
					item.EquipUpgradePiece("fore", val5);
				}
				else
				{
					item.EquipUpgradePiece("fore", val3);
				}
			}
			if (item.HasSlot("sail") && ((MBObjectBase)item.ShipHull).StringId != "burning_cog_ship")
			{
				item.EquipUpgradePiece("sail", val4);
			}
		}
	}

	private void CreateMerchantsParty()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 val = NavigationHelper.FindPointAroundPosition(MobileParty.MainParty.Position, (NavigationType)2, 3f, 1f, true, false);
		TextObject val2 = new TextObject("{=CElcGl2R}Sturgian Merchants", (Dictionary<string, object>)null);
		_merchantParty = CustomPartyComponent.CreateCustomPartyWithPartyTemplate(val, 3f, _settlement, val2, _settlement.OwnerClan, _merchantsTemplate, (Hero)null, "", "", 0f, false);
		_merchantParty.SetPartyUsedByQuest(true);
		_merchantParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
	}

	private void StartBattle(bool fromCheckPoint)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		AddState(QuestState.BattleStarted);
		if (PartyBase.MainParty.MapEventSide == null)
		{
			PlayerEncounter.Start();
			PlayerEncounter.Current.SetupFields(_houndsParty.Party, PartyBase.MainParty);
			PlayerEncounter.StartBattle();
		}
		CreateMerchantsParty();
		_merchantParty.MapEventSide = PartyBase.MainParty.MapEventSide;
		NavalMissions.OpenBlockedEstuaryMission(GetMissionInitializerRecord(), _houndsParty, fromCheckPoint);
	}

	public void OnCheckPointReached()
	{
		AddState(QuestState.CheckpointReached);
	}

	private MissionInitializerRecord GetMissionInitializerRecord()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected I4, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		MissionInitializerRecord navalMissionInitializerTemplate = NavalStorylineData.GetNavalMissionInitializerTemplate("naval_storyline_act_3_quest_3");
		TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
		navalMissionInitializerTemplate.TerrainType = (int)faceTerrainType;
		navalMissionInitializerTemplate.NeedsRandomTerrain = false;
		navalMissionInitializerTemplate.PlayingInCampaignMode = true;
		navalMissionInitializerTemplate.RandomTerrainSeed = MBRandom.RandomInt(10000);
		navalMissionInitializerTemplate.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position);
		navalMissionInitializerTemplate.SceneHasMapPatch = false;
		navalMissionInitializerTemplate.AtmosphereOnCampaign.NauticalInfo.UsesNavalSimulatedWater = 1;
		return navalMissionInitializerTemplate;
	}

	private void DestroyParty(ref MobileParty mobileParty)
	{
		if (mobileParty != null && mobileParty.IsActive)
		{
			if (mobileParty.MapEventSide != null)
			{
				mobileParty.MapEventSide = null;
			}
			DestroyPartyAction.Apply((PartyBase)null, mobileParty);
			mobileParty = null;
		}
	}

	private bool HasTalkedToSailors()
	{
		return (_state & QuestState.TalkedToSailors) == QuestState.TalkedToSailors;
	}

	private bool HasBattleStarted()
	{
		return (_state & QuestState.BattleStarted) == QuestState.BattleStarted;
	}

	private bool HasBattleWon()
	{
		return (_state & QuestState.BattleWon) == QuestState.BattleWon;
	}

	private bool CheckPointReached()
	{
		return (_state & QuestState.CheckpointReached) == QuestState.CheckpointReached;
	}

	private bool HadEncounterWithBjolgur()
	{
		return (_state & QuestState.HadEncounterWithBjolgor) == QuestState.HadEncounterWithBjolgor;
	}

	private void AddState(QuestState state)
	{
		_state |= state;
	}
}
