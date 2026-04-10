using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.Missions;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class HuntDownTheEmiraAlFahdaAndTheCorsairsQuest : NavalStorylineQuestBase
{
	private const int NumberOfCorsairParties = 2;

	private const int GoldReward = 1000;

	private const int RelationshipReward = 10;

	private const int CorsairShipAiDisableTime = 3;

	private const string QuestSetPieceEncounterMenuId = "naval_storyline_act_3_quest_2_encounter_menu";

	private const string QuestSetPieceRetryMenuId = "naval_storyline_act_3_quest_2_retry_menu";

	private const string Act3Quest2CorsairPartyTemplateStringIdBase = "storyline_act3_quest_2_corsair_generic_template_";

	private const string Act3Quest2BossCorsairPartyTemplateStringId = "storyline_act3_quest_2_boss_corsair_template";

	private const string FahdaShipHullId = "ship_meditheavy_storyline";

	private const string MediumReinforcementShipHullId = "ship_liburna_storyline";

	private const string LightReinforcementShipHullId = "ship_meditlight_storyline";

	private static readonly Dictionary<string, string> FahdaShipUpgradePieces = new Dictionary<string, string> { { "side", "side_southern_shields_lvl2" } };

	private static readonly Dictionary<string, string> MediumReinforcementShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl2" },
		{ "sail", "sails_lvl2" }
	};

	private static readonly Dictionary<string, string> FirstLightReinforcementShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl2" },
		{ "sail", "sails_lvl2" }
	};

	private static readonly Dictionary<string, string> SecondLightReinforcementShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl2" },
		{ "sail", "sails_lvl2" }
	};

	private const string LaharShipHullId = "ship_liburna_q2_storyline";

	private static readonly Dictionary<string, string> LaharShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl3" },
		{ "sail", "sails_lvl2" },
		{ "bow", "bow_northern_reinforced_ram_lvl3" }
	};

	private const string GunnarShipHullId = "northern_medium_ship";

	private static readonly Dictionary<string, string> GunnarShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl2" },
		{ "sail", "sails_lvl2" }
	};

	private GameEntity _stormEntity;

	[SaveableField(1)]
	private List<MobileParty> _corsairParties;

	[SaveableField(2)]
	private JournalLog _playerStartsQuestLog;

	[SaveableField(3)]
	private CampaignVec2 _corsairSpawnPosition;

	[SaveableField(4)]
	private int _numberOfDefeatedCorsairParties;

	[SaveableField(5)]
	private MobileParty _bossCorsairParty;

	[SaveableField(6)]
	private bool _battleWon;

	[SaveableField(7)]
	private bool _willProgressStoryline;

	[SaveableField(8)]
	private bool _battleStarted;

	[SaveableField(9)]
	private readonly MapMarker _corsairHuntingGroundMarker;

	public override bool WillProgressStoryline => _willProgressStoryline;

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=kEyCQWh1}Hunt Down {HERO.NAME}", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.EmiraAlFahda.CharacterObject, false);
			return val;
		}
	}

	private TextObject DescriptionLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=ezctGj6M}Find the corsair {HERO.NAME} and defeat her.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.EmiraAlFahda.CharacterObject, false);
			return val;
		}
	}

	private TextObject MainCorsairShipSpawnedLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=BKlHaMZ6}Overtake and defeat {HERO.NAME} and her fleet.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.EmiraAlFahda.CharacterObject, false);
			return val;
		}
	}

	private TextObject QuestSucceededWithRansomLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			TextObject val = new TextObject("{=UvFN0bf1}You decided to accept {HERO.NAME}'s ransom money. ({GOLD_REWARD}{GOLD_ICON}).", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.EmiraAlFahda.CharacterObject, false);
			val.SetTextVariable("GOLD_REWARD", 1000);
			return val;
		}
	}

	private TextObject QuestSucceededWithReturnOfEmiraLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			TextObject val = new TextObject("{=DKA4tOwq}You decided to return {HERO.NAME} to her uncles alive.(+{RELATIONSHIP_REWARD} relationship with all notables in {SETTLEMENT_LINK}).", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.EmiraAlFahda.CharacterObject, false);
			val.SetTextVariable("RELATIONSHIP_REWARD", 10);
			val.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.Act3Quest2TargetSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject PlayerStartsQuestLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=pfIWdGnV}The corsairs appear to be scattered. Find them and take them, until you sight {HERO.NAME}.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.EmiraAlFahda.CharacterObject, false);
			return val;
		}
	}

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act3Quest2;

	protected override string MainPartyTemplateStringId => "storyline_act3_quest_2_main_party_template";

	public HuntDownTheEmiraAlFahdaAndTheCorsairsQuest(string questId, Hero questGiver, CampaignVec2 corsairSpawnPosition)
		: base(questId, questGiver, CampaignTime.Never, 0)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		_willProgressStoryline = false;
		_numberOfDefeatedCorsairParties = 0;
		_corsairParties = new List<MobileParty>();
		_bossCorsairParty = null;
		_corsairSpawnPosition = corsairSpawnPosition;
		_corsairHuntingGroundMarker = Campaign.Current.MapMarkerManager.CreateMapMarker(NavalStorylineData.CorsairBanner, new TextObject("{=QLrwlirp}Corsair Hunting Grounds", (Dictionary<string, object>)null), ((CampaignVec2)(ref _corsairSpawnPosition)).AsVec3(), false, ((MBObjectBase)this).StringId);
		((QuestBase)this).AddLog(DescriptionLogText, false);
	}

	protected override void OnFinalizeInternal()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		_playerStartsQuestLog = null;
		DestroyCorsairParties();
		Scene scene = ((MapScene)Campaign.Current.MapSceneWrapper).Scene;
		List<GameEntity> list = new List<GameEntity>();
		scene.GetAllEntitiesWithScriptComponent<CampaignMapAmbientOccluder>(ref list);
		foreach (GameEntity item in list)
		{
			item.GetFirstScriptOfType<CampaignMapAmbientOccluder>().UnregisterQuestStorm(_stormEntity);
		}
		GameEntity stormEntity = _stormEntity;
		if (stormEntity != null)
		{
			stormEntity.Remove(111);
		}
	}

	protected override void InitializeQuestOnGameLoadInternal()
	{
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		if (_numberOfDefeatedCorsairParties == 2)
		{
			SpawnStormEntity();
		}
	}

	protected override void SetDialogs()
	{
		AddDialogsForFinalFight();
	}

	protected override void OnStartQuestInternal()
	{
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		_numberOfDefeatedCorsairParties = 2;
		SpawnMainCorsairParty();
		SpawnStormEntity();
		_willProgressStoryline = true;
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_corsairHuntingGroundMarker);
	}

	protected override void HourlyTick()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = _corsairHuntingGroundMarker.Position;
		CampaignVec2 position2 = MobileParty.MainParty.Position;
		if (((Vec3)(ref position)).Distance(((CampaignVec2)(ref position2)).AsVec3()) > 15f)
		{
			_corsairHuntingGroundMarker.IsVisibleOnMap = true;
		}
		else
		{
			_corsairHuntingGroundMarker.IsVisibleOnMap = false;
		}
		foreach (MobileParty corsairParty in _corsairParties)
		{
			if (MBRandom.RandomFloat < 0.25f && corsairParty.IsActive && !corsairParty.IsMoving && !corsairParty.Ai.IsDisabled)
			{
				CampaignVec2 val = NavigationHelper.FindReachablePointAroundPosition(_corsairSpawnPosition, (NavigationType)2, 10f, 3f, false);
				corsairParty.SetMoveGoToPoint(val, (NavigationType)2);
			}
		}
	}

	protected override void IsNavalQuestPartyInternal(PartyBase party, NavalStorylinePartyData data)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber maxPartySizeLimitFromTemplate;
		if (_corsairParties.Any((MobileParty c) => c.Party == party))
		{
			PartyTemplateObject templateObject = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_2_corsair_generic_template_" + ((!party.Id.Contains("0")) ? 1 : 0));
			maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(templateObject);
			data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber;
			data.IsQuestParty = true;
		}
		else if (_bossCorsairParty != null && _bossCorsairParty.Party == party)
		{
			PartyTemplateObject templateObject2 = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_2_boss_corsair_template");
			maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(templateObject2);
			data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber + 1;
			data.IsQuestParty = true;
		}
		if (party == PartyBase.MainParty)
		{
			data.PartySize++;
		}
	}

	protected override void OnCompleteWithSuccessInternal()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		MobileParty.MainParty.MemberRoster.RemoveTroop(NavalStorylineData.Lahar.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
		NavalStorylineData.Lahar.ChangeState((CharacterStates)6);
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest2Succeeded);
	}

	protected override void OnFailedInternal()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		MobileParty.MainParty.MemberRoster.RemoveTroop(NavalStorylineData.Lahar.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
		NavalStorylineData.Lahar.ChangeState((CharacterStates)6);
	}

	protected override void RegisterEventsInternal()
	{
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
		CampaignEvents.MapEventStarted.AddNonSerializedListener((object)this, (Action<MapEvent, PartyBase, PartyBase>)OnMapEventStarted);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.OnShipOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Ship, PartyBase, ShipOwnerChangeDetail>)OnShipOwnerChanged);
		CampaignEvents.BeforeGameMenuOpenedEvent.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnBeforeGameMenuOpened);
	}

	private void OnMapEventStarted(MapEvent mapEvent, PartyBase partyBase1, PartyBase partyBase2)
	{
		if (partyBase1.IsNavalStorylineQuestParty())
		{
			foreach (Ship item in (List<Ship>)(object)partyBase1.Ships)
			{
				item.IsInvulnerable = false;
			}
		}
		if (!partyBase2.IsNavalStorylineQuestParty())
		{
			return;
		}
		foreach (Ship item2 in (List<Ship>)(object)partyBase2.Ships)
		{
			item2.IsInvulnerable = false;
		}
	}

	private void OnShipOwnerChanged(Ship ship, PartyBase partyBase, ShipOwnerChangeDetail detail)
	{
		if (partyBase == PartyBase.MainParty && ship.IsInvulnerable)
		{
			ship.IsInvulnerable = false;
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
		((QuestBase)this).AddGameMenu("naval_storyline_act_3_quest_2_encounter_menu", new TextObject("{=YjcPI4pT}An east wind sweeps across the sea, bearing desert dust, and briefly obscures your vision. Soon after it lifts, you hear your lookouts shouting excitedly to you. They have spotted Fahda’s fleet, which appears to have been damaged by the gale. If you attack now, you may be able to sink the flagship before it can escape.", (Dictionary<string, object>)null), new OnInitDelegate(naval_storyline_act_3_quest_2_set_piece_encounter_menu_on_init), (MenuOverlayType)4, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("naval_storyline_act_3_quest_2_encounter_menu", "naval_storyline_act_3_quest_2_encounter_menu_continue", new TextObject("{=1r0tDsrR}Attack!", (Dictionary<string, object>)null), new OnConditionDelegate(naval_storyline_act_3_quest_2_set_piece_encounter_menu_attack_on_condition), new OnConsequenceDelegate(naval_storyline_act_3_quest_2_set_piece_encounter_menu_attack_on_consequence), false, -1);
		((QuestBase)this).AddGameMenu("naval_storyline_act_3_quest_2_retry_menu", new TextObject("{=etH1IHNZ}You manage to put some distance between you and your enemies, and you have a moment to consider how to proceed.", (Dictionary<string, object>)null), new OnInitDelegate(naval_storyline_act_3_quest_2_set_piece_retry_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("naval_storyline_act_3_quest_2_retry_menu", "try_again_option", new TextObject("{=YHMDy3lQ}Try again", (Dictionary<string, object>)null), new OnConditionDelegate(naval_storyline_act_3_quest_2_set_piece_retry_menu_retry_on_condition), new OnConsequenceDelegate(naval_storyline_act_3_quest_2_set_piece_retry_menu_retry_on_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("naval_storyline_act_3_quest_2_retry_menu", "leave_option", new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null), new OnConditionDelegate(naval_storyline_act_3_quest_2_set_piece_retry_menu_leave_on_condition), new OnConsequenceDelegate(naval_storyline_act_3_quest_2_set_piece_retry_menu_leave_on_consequence), true, -1);
	}

	private void naval_storyline_act_3_quest_2_set_piece_retry_menu_leave_on_consequence(MenuCallbackArgs args)
	{
		((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
		NavalStorylineData.DeactivateNavalStoryline();
	}

	private bool naval_storyline_act_3_quest_2_set_piece_retry_menu_retry_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		if (!_battleWon)
		{
			return _battleStarted;
		}
		return false;
	}

	private bool naval_storyline_act_3_quest_2_set_piece_encounter_menu_attack_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		if (!_battleStarted)
		{
			return !_battleWon;
		}
		return false;
	}

	private void OnBeforeGameMenuOpened(MenuCallbackArgs args)
	{
		MenuContext menuContext = args.MenuContext;
		object obj;
		if (menuContext == null)
		{
			obj = null;
		}
		else
		{
			GameMenu gameMenu = menuContext.GameMenu;
			obj = ((gameMenu != null) ? gameMenu.StringId : null);
		}
		if ((string?)obj == "naval_storyline_encounter_meeting" && NavalStorylineData.IsNavalStoryLineActive() && PlayerEncounter.EncounteredParty != null && PlayerEncounter.EncounteredParty.IsNavalStorylineQuestParty())
		{
			PlayerEncounter.SetMeetingDone();
		}
	}

	private bool naval_storyline_act_3_quest_2_set_piece_retry_menu_leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		args.Tooltip = new TextObject("{=wmTjX28f}This will exit story mode and return you to the Sandbox. You can continue the storyline later by talking to Gunnar in the port again.", (Dictionary<string, object>)null);
		args.optionLeaveType = (LeaveType)16;
		if (!_battleWon)
		{
			return _battleStarted;
		}
		return false;
	}

	private void naval_storyline_act_3_quest_2_set_piece_encounter_menu_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest2EncounterMenu);
	}

	private void naval_storyline_act_3_quest_2_set_piece_retry_menu_on_init(MenuCallbackArgs args)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
		if (_battleWon)
		{
			PlayerEncounter.Finish(true);
			RefreshShips(MobileParty.MainParty, ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>(MainPartyTemplateStringId));
			AddShipUpgradesForMainParty();
			NavalStorylineData.EmiraAlFahda.SetHasMet();
			NavalStorylineData.EmiraAlFahda.MakeWounded((Hero)null, (KillCharacterActionDetail)0);
			ConversationCharacterData val = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, false, false, false, false, true);
			ConversationCharacterData val2 = default(ConversationCharacterData);
			((ConversationCharacterData)(ref val2))._002Ector(NavalStorylineData.EmiraAlFahda.CharacterObject, (PartyBase)null, true, true, true, false, false, true);
			CampaignMission.OpenConversationMission(val, val2, "conversation_scene_sea_multi_agent", "", true);
		}
		else
		{
			RefreshParty(_bossCorsairParty, ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_2_boss_corsair_template"));
			AddShipUpgradesForMainCorsairParty();
			RefreshParty(MobileParty.MainParty, ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>(MainPartyTemplateStringId));
			AddShipUpgradesForMainParty();
		}
	}

	private void naval_storyline_act_3_quest_2_set_piece_encounter_menu_attack_on_consequence(MenuCallbackArgs args)
	{
		StartBattle();
	}

	private void naval_storyline_act_3_quest_2_set_piece_retry_menu_retry_on_consequence(MenuCallbackArgs args)
	{
		StartBattle();
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		if (!NavalStorylineData.IsNavalStoryLineActive() || PlayerEncounter.EncounteredParty == null || !PlayerEncounter.EncounteredParty.IsNavalStorylineQuestParty())
		{
			return;
		}
		MenuContext menuContext = args.MenuContext;
		object obj;
		if (menuContext == null)
		{
			obj = null;
		}
		else
		{
			GameMenu gameMenu = menuContext.GameMenu;
			obj = ((gameMenu != null) ? gameMenu.StringId : null);
		}
		string text = (string)obj;
		MobileParty bossCorsairParty = _bossCorsairParty;
		if (((bossCorsairParty != null) ? bossCorsairParty.Party : null) == PlayerEncounter.EncounteredParty)
		{
			if (text == "naval_storyline_encounter")
			{
				GameMenu.ActivateGameMenu("naval_storyline_act_3_quest_2_encounter_menu");
			}
		}
		else
		{
			MBTextManager.SetTextVariable("ENCOUNTER_TEXT", new TextObject("{=XVCdua8m}One of your sharper-eyed sailors thinks he sees a ship. You stare at the horizon, and though at first it's hard to make out shapes against the choppy waves of the gulf, you eventually distinguish the unmistakable outline of a lateen sail. It's a corsair, and it's heading directly towards you. ", (Dictionary<string, object>)null), true);
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Invalid comparison between Unknown and I4
		if (!Mission.Current.IsNavalBattle || PlayerEncounter.Current == null || PlayerEncounter.EncounteredParty == null)
		{
			return;
		}
		MobileParty bossCorsairParty = _bossCorsairParty;
		if (((bossCorsairParty != null) ? bossCorsairParty.Party : null) != PlayerEncounter.EncounteredParty)
		{
			return;
		}
		if (PlayerEncounter.CampaignBattleResult != null && PlayerEncounter.CampaignBattleResult.BattleResolved)
		{
			if (PlayerEncounter.CampaignBattleResult.PlayerDefeat)
			{
				_battleWon = false;
			}
			else if (PlayerEncounter.CampaignBattleResult.PlayerVictory)
			{
				MobileParty bossCorsairParty2 = _bossCorsairParty;
				if (((bossCorsairParty2 != null) ? bossCorsairParty2.Party : null) == PlayerEncounter.EncounteredParty)
				{
					_battleWon = true;
				}
			}
		}
		else if ((int)PlayerEncounter.WinningSide == -1)
		{
			_battleWon = false;
		}
		else
		{
			Debug.FailedAssert("unhandled case", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\Quests\\HuntDownTheEmiraAlFahdaAndTheCorsairsQuest.cs", "OnMissionEnded", 475);
		}
	}

	private void OnMobilePartyDestroyed(MobileParty party, PartyBase partyBase)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		if (NavalStorylineData.IsNavalStoryLineActive() && _playerStartsQuestLog != null && _corsairParties.Contains(party) && partyBase == PartyBase.MainParty)
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=MRX4gImP}So far so good, but there are still enemies about.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, "");
			_numberOfDefeatedCorsairParties++;
			_corsairParties.Remove(party);
			((QuestBase)this).UpdateQuestTaskStage(_playerStartsQuestLog, _numberOfDefeatedCorsairParties);
			if (2 == _numberOfDefeatedCorsairParties)
			{
				SpawnStormEntity();
				SpawnMainCorsairParty();
				((QuestBase)this).AddLog(MainCorsairShipSpawnedLogText, false);
			}
		}
	}

	private void AddDialogsForFinalFight()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0043: Expected O, but got Unknown
		//IL_0043: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00a1: Expected O, but got Unknown
		//IL_00a1: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_00cb: Expected O, but got Unknown
		//IL_00cb: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_00f5: Expected O, but got Unknown
		//IL_00f5: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		//IL_011f: Expected O, but got Unknown
		//IL_011f: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_0149: Expected O, but got Unknown
		//IL_0149: Expected O, but got Unknown
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		//IL_0174: Expected O, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected O, but got Unknown
		//IL_0198: Expected O, but got Unknown
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		//IL_01cd: Expected O, but got Unknown
		//IL_01cd: Expected O, but got Unknown
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Expected O, but got Unknown
		//IL_01f7: Expected O, but got Unknown
		//IL_01f7: Expected O, but got Unknown
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Expected O, but got Unknown
		//IL_0221: Expected O, but got Unknown
		//IL_0221: Expected O, but got Unknown
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Expected O, but got Unknown
		//IL_0246: Expected O, but got Unknown
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Expected O, but got Unknown
		//IL_026a: Expected O, but got Unknown
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Expected O, but got Unknown
		//IL_029f: Expected O, but got Unknown
		//IL_029f: Expected O, but got Unknown
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Expected O, but got Unknown
		//IL_02c9: Expected O, but got Unknown
		//IL_02c9: Expected O, but got Unknown
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Expected O, but got Unknown
		//IL_02f3: Expected O, but got Unknown
		//IL_02f3: Expected O, but got Unknown
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Expected O, but got Unknown
		//IL_031d: Expected O, but got Unknown
		//IL_031d: Expected O, but got Unknown
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Expected O, but got Unknown
		//IL_0347: Expected O, but got Unknown
		//IL_0347: Expected O, but got Unknown
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Expected O, but got Unknown
		//IL_0372: Expected O, but got Unknown
		//IL_0372: Expected O, but got Unknown
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Expected O, but got Unknown
		//IL_0398: Expected O, but got Unknown
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Expected O, but got Unknown
		//IL_03c2: Expected O, but got Unknown
		//IL_03c2: Expected O, but got Unknown
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Expected O, but got Unknown
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Expected O, but got Unknown
		//IL_03f6: Expected O, but got Unknown
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Expected O, but got Unknown
		//IL_0420: Expected O, but got Unknown
		//IL_0420: Expected O, but got Unknown
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Expected O, but got Unknown
		//IL_044a: Expected O, but got Unknown
		//IL_044a: Expected O, but got Unknown
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Expected O, but got Unknown
		string text = default(string);
		string text2 = default(string);
		string text3 = default(string);
		string text4 = default(string);
		string text5 = default(string);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1200).NpcLine(new TextObject("{=unOIbuqz}What have you done? Do you know who I am? I have allies who'll unthread your entrails from your guts and hang you with them from your own yardarm. I am queen of these waters, you fools, and those who practice piracy here without my permission end up chum to attract the sharks.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null).Condition(new OnConditionDelegate(MultiAgentConversationCondition))
			.GenerateToken(ref text)
			.GenerateToken(ref text2)
			.GenerateToken(ref text3)
			.GenerateToken(ref text4)
			.GenerateToken(ref text5)
			.NpcLine(new TextObject("{=xQunuNT9}My lady, we are not pirates. Rather, I am a man who has done many services for families such as your own in Quyaz. At present I am working for your uncles. I do not know what they intend to do with you, although I do not expect that a town that lives on trade will deal leniently with piracy.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), (string)null, (string)null)
			.NpcLine(new TextObject("{=nyOUdUQI}Before we sail, however, I would like you to have a chat with my friend here.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), (string)null, (string)null)
			.NpcLine(new TextObject("{=LFLn7SJc}So you are on contract to deliver me alive to Quyaz, are you? I can tell you this, then - my lineage goes back to the founding of that city, and if you spill so much as a drop of my blood, your own shall be drained from your body like that of a horse-fish. As for the Sea Hounds, they are my allies and servants, and I shall not betray them to you.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(new TextObject("{=C88poDCA}How much are my uncles paying you, anyway? I have a chest of silver set aside for occasions such as this, and I suspect I could pay you more than they will. They are stingy men.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(new TextObject("{=v2664Qeo}...", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.GotoDialogState(text)
			.BeginPlayerOptions(text, true)
			.PlayerOption(new TextObject("{=q3uOXLEO}I am here too, and I have no contract to deliver you anywhere alive.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), (string)null, (string)null)
			.GotoDialogState(text2)
			.PlayerOption(new TextObject("{=XfIbjoH8}You tell me all you know about the Sea Hounds and their dealings in slaves.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), (string)null, (string)null)
			.GotoDialogState(text2)
			.EndPlayerOptions()
			.NpcLine(new TextObject("{=06AGZvSg}Are you threatening me? You won't get a single coin from my uncles if you harm me.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), new OnMultipleConversationConsequenceDelegate(IsMainHero), text2, (string)null)
			.NpcLine(new TextObject("{=T0a3QpjV}Unlike Lahar, here, we have not shed our blood today merely for a part-share of a ransom, or to boost our standing with the merchants of Quyaz. You are an ally of the Sea Hounds, and it serves us well to make an example of you. Your life is forfeit unless you tell us something we can use.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), (string)null, (string)null)
			.NpcLine(new TextObject("{=IPq1hnUG}How do I know that telling you about the Sea Hounds will save my life?", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, text3)
			.BeginPlayerOptions(text3, false)
			.PlayerOption(new TextObject("{=Su0h3ZMC}If you speak truthfully, you will live.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), (string)null, (string)null)
			.GotoDialogState(text4)
			.PlayerOption(new TextObject("{=9tmYkhb1}You'll just have to try and see.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), (string)null, (string)null)
			.GotoDialogState(text4)
			.EndPlayerOptions()
			.NpcLine(new TextObject("{=tlXQV9mO}I can tell you this – I don’t have your sister. I used to buy captives from the Sea Hounds. But now they have this new leader named Purig, who keeps them all for his own purposes. Apparently he has some anchorage up in the north, where he intends to use slaves to build larger and stronger ships.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), new OnMultipleConversationConsequenceDelegate(IsMainHero), text4, (string)null)
			.NpcLine(new TextObject("{=w5GbjHDG}Purig, a leader among the Sea Hounds! I'll speak straight here - it gnaws at my gut to hear that he is prospering from his treachery.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(new TextObject("{=C2OtgWn0}He acts as though the Sea Hounds have already crowned him their king. He demanded that I hunt for captives here in the south and sell them to him, promising to pay me with a huge store of silver that some new partners of his, a vile-looking gang of Vlandian pirates, hoped to steal from the merchants of Omor.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(new TextObject("{=Ex5CzHBt}We should get more information on this Omor silver. If we can stop these Vlandians it would deal a great blow to Purig, and we could possibly find out more about this northern anchorage, his captives, and maybe your sister.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(new TextObject("{=2bzElv6k}So that information is worth something to you, is it not? If we add in that ransom I mentioned, is it enough to buy my life and my freedom?", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsEmiraAlFahda), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(new TextObject("{=M24S1pEI}You know my preference, {PLAYER.NAME}. If I bring her back to Quyaz, I will ensure that you get some of the credit, but perhaps you prefer good cold silver to goodwill.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, text5)
			.BeginPlayerOptions(text5, false)
			.PlayerOption(new TextObject("{=VHbGnf4W}Return her to her uncles alive, as per your original understanding.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(new TextObject("{=7f9yXAvI}I accept your decision. Very well then…", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				OnPlayerSelectsOption1();
			})
			.CloseDialog()
			.PlayerOption(new TextObject("{=xpz9JFGK}The lady offers a fair ransom. Let us accept.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsLahar), (string)null, (string)null)
			.NpcLine(new TextObject("{=7f9yXAvI}I accept your decision. Very well then…", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(new TextObject("{=cxG2qhbv}Listen. I have enjoyed our excursion, and hunting pirates is always good business. Though I must depart now to Quyaz, I would like to go hunting with you again. Gunnar tells me that you will be sailing from Ostican once you locate your next quarry. Hopefully I will see you there soon.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsLahar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				OnPlayerSelectsOption2();
			})
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
	}

	private void OnPlayerSelectsOption1()
	{
		foreach (Hero item in (List<Hero>)(object)NavalStorylineData.Act3Quest2TargetSettlement.Notables)
		{
			ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, item, 10, true);
		}
		((QuestBase)this).AddLog(QuestSucceededWithReturnOfEmiraLogText, false);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private void OnPlayerSelectsOption2()
	{
		GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, 1000, false);
		((QuestBase)this).AddLog(QuestSucceededWithRansomLogText, false);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private bool IsLahar(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Lahar.CharacterObject;
	}

	private bool IsGunnar(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
	}

	private bool IsMainHero(IAgent agent)
	{
		return (object)agent.Character == CharacterObject.PlayerCharacter;
	}

	private bool IsEmiraAlFahda(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.EmiraAlFahda.CharacterObject;
	}

	private Agent SpawnGunnar()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject);
		val.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
		Vec3 globalPosition = Mission.Current.Scene.FindEntityWithName("free_infantry_spawn_point_1").GlobalPosition;
		val.InitialPosition(ref globalPosition);
		Vec3 lookDirection = Agent.Main.LookDirection;
		Vec2 val2 = ((Vec3)(ref lookDirection)).AsVec2;
		val2 = ((Vec2)(ref val2)).Normalized();
		val.InitialDirection(ref val2);
		val.NoHorses(true);
		return Mission.Current.SpawnAgent(val, false);
	}

	private bool MultiAgentConversationCondition()
	{
		if (Hero.OneToOneConversationHero == NavalStorylineData.EmiraAlFahda && MobileParty.MainParty.IsCurrentlyAtSea && Mission.Current != null)
		{
			Agent item = SpawnLahar();
			Agent item2 = SpawnGunnar();
			Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<Agent> { item, item2 }, true);
			return true;
		}
		return false;
	}

	private Agent SpawnLahar()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Lahar.CharacterObject);
		val.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
		Vec3 globalPosition = Mission.Current.Scene.FindEntityWithName("free_infantry_spawn_point_0").GlobalPosition;
		val.InitialPosition(ref globalPosition);
		Vec3 lookDirection = Agent.Main.LookDirection;
		Vec2 val2 = ((Vec3)(ref lookDirection)).AsVec2;
		val2 = ((Vec2)(ref val2)).Normalized();
		val.InitialDirection(ref val2);
		val.NoHorses(true);
		return Mission.Current.SpawnAgent(val, false);
	}

	private void StartBattle()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		_battleWon = false;
		_battleStarted = true;
		foreach (TroopRosterElement item in ((IEnumerable<TroopRosterElement>)PartyBase.MainParty.MemberRoster.GetTroopRoster()).Where((TroopRosterElement troop) => ((BasicCharacterObject)troop.Character).IsHero && troop.Character.HeroObject.IsWounded))
		{
			item.Character.HeroObject.Heal(item.Character.HeroObject.WoundedHealthLimit - item.Character.HeroObject.HitPoints + 1, false);
		}
		PlayerEncounter.Finish(true);
		PlayerEncounter.Start();
		PlayerEncounter.Current.SetupFields(_bossCorsairParty.Party, PartyBase.MainParty);
		PlayerEncounter.StartBattle();
		MissionInitializerRecord navalMissionInitializerTemplate = NavalStorylineData.GetNavalMissionInitializerTemplate("naval_storyline_act_3_quest_2");
		navalMissionInitializerTemplate.NeedsRandomTerrain = false;
		navalMissionInitializerTemplate.PlayingInCampaignMode = false;
		navalMissionInitializerTemplate.SceneHasMapPatch = false;
		navalMissionInitializerTemplate.AtmosphereOnCampaign.NauticalInfo.UsesNavalSimulatedWater = 1;
		NavalMissions.OpenNavalStorylineWoundedBeastBattleMission(navalMissionInitializerTemplate);
		GameMenu.ActivateGameMenu("naval_storyline_act_3_quest_2_retry_menu");
	}

	private void SpawnMainCorsairParty()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		NavalStorylineData.EmiraAlFahda.ChangeState((CharacterStates)1);
		_bossCorsairParty = CustomPartyComponent.CreateCustomPartyWithPartyTemplate(_corsairSpawnPosition, 1f, NavalStorylineData.HomeSettlement, new TextObject("{=j7h8QfsE}Fahda's Corsairs", (Dictionary<string, object>)null), Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "southern_pirates")), ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_2_boss_corsair_template"), NavalStorylineData.EmiraAlFahda, NavalStorylineData.EmiraAlFahda, "", "", 1f, false);
		AddShipUpgradesForMainCorsairParty();
		SetupCorsairParty(_bossCorsairParty);
		_bossCorsairParty.IsInfoHidden = true;
	}

	private void AddShipUpgradesForMainCorsairParty()
	{
		bool flag = false;
		foreach (Ship item in (List<Ship>)(object)_bossCorsairParty.Ships)
		{
			if (((MBObjectBase)item.ShipHull).StringId == "ship_meditheavy_storyline")
			{
				item.ChangeFigurehead(DefaultFigureheads.Viper);
				AddShipUpgradePieces(item, FahdaShipUpgradePieces);
			}
			else if (((MBObjectBase)item.ShipHull).StringId == "ship_liburna_storyline")
			{
				item.ChangeFigurehead(DefaultFigureheads.Hawk);
				AddShipUpgradePieces(item, MediumReinforcementShipUpgradePieces);
			}
			else if (((MBObjectBase)item.ShipHull).StringId == "ship_meditlight_storyline")
			{
				if (flag)
				{
					AddShipUpgradePieces(item, SecondLightReinforcementShipUpgradePieces);
					continue;
				}
				AddShipUpgradePieces(item, FirstLightReinforcementShipUpgradePieces);
				flag = true;
			}
		}
	}

	private void AddShipUpgradesForMainParty()
	{
		foreach (Ship item in (List<Ship>)(object)MobileParty.MainParty.Ships)
		{
			if (((MBObjectBase)item.ShipHull).StringId == "ship_liburna_q2_storyline")
			{
				item.ChangeFigurehead(DefaultFigureheads.Hawk);
				AddShipUpgradePieces(item, LaharShipUpgradePieces);
			}
			else if (((MBObjectBase)item.ShipHull).StringId == "northern_medium_ship")
			{
				item.ChangeFigurehead(DefaultFigureheads.Dragon);
				AddShipUpgradePieces(item, GunnarShipUpgradePieces);
			}
		}
	}

	private void SetupCorsairParty(MobileParty corsairParty)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		corsairParty.SetPartyUsedByQuest(true);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)corsairParty);
		corsairParty.IsCurrentlyAtSea = true;
		corsairParty.IsVisible = true;
		corsairParty.Party.SetCustomBanner(NavalStorylineData.CorsairBanner);
		foreach (Ship item in (List<Ship>)(object)corsairParty.Ships)
		{
			item.IsInvulnerable = true;
		}
		corsairParty.Ai.SetDoNotMakeNewDecisions(true);
		corsairParty.Ai.DisableForHours(3);
		corsairParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
		corsairParty.Party.SetVisualAsDirty();
	}

	private void DestroyCorsairParties()
	{
		foreach (MobileParty item in _corsairParties.ToList())
		{
			if (item != null && item.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, item);
			}
		}
		if (_bossCorsairParty != null && _bossCorsairParty.IsActive)
		{
			DestroyPartyAction.Apply((PartyBase)null, _bossCorsairParty);
		}
	}

	private void SpawnStormEntity()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (_stormEntity == (GameEntity)null)
		{
			MatrixFrame identity = MatrixFrame.Identity;
			Scene scene = ((MapScene)Campaign.Current.MapSceneWrapper).Scene;
			List<GameEntity> list = new List<GameEntity>();
			identity.origin = new Vec3(((CampaignVec2)(ref _corsairSpawnPosition)).X, ((CampaignVec2)(ref _corsairSpawnPosition)).Y, 0f, -1f);
			_stormEntity = GameEntity.Instantiate(scene, "psys_mapicon_darkclouds", identity, true, "");
			scene.GetAllEntitiesWithScriptComponent<CampaignMapAmbientOccluder>(ref list);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].GetFirstScriptOfType<CampaignMapAmbientOccluder>().RegisterQuestStorm(_stormEntity);
			}
		}
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
		RefreshShips(mobileParty, pt);
	}

	private void RefreshShips(MobileParty mobileParty, PartyTemplateObject pt)
	{
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			item.HitPoints = item.MaxHitPoints;
		}
		List<Ship> list = Campaign.Current.Models.PartySizeLimitModel.FindAppropriateInitialShipsForMobileParty(mobileParty, pt);
		if (((List<Ship>)(object)mobileParty.Ships).Count == list.Count)
		{
			return;
		}
		foreach (Ship ship in (List<Ship>)(object)mobileParty.Ships)
		{
			Ship val = ((IEnumerable<Ship>)list).FirstOrDefault((Func<Ship, bool>)((Ship x) => x.ShipHull == ship.ShipHull));
			if (val != null)
			{
				list.Remove(val);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		foreach (Ship item2 in list)
		{
			ChangeShipOwnerAction.ApplyByMobilePartyCreation(mobileParty.Party, item2);
			if (mobileParty != MobileParty.MainParty)
			{
				item2.IsInvulnerable = true;
			}
		}
	}

	private void AddShipUpgradePieces(Ship ship, Dictionary<string, string> upgradePieces)
	{
		foreach (KeyValuePair<string, string> kv in upgradePieces)
		{
			ShipUpgradePiece val = MBObjectManager.Instance.GetObject<ShipUpgradePiece>(kv.Value);
			if (((IEnumerable<KeyValuePair<string, ShipSlot>>)ship.ShipHull.AvailableSlots).Any((KeyValuePair<string, ShipSlot> slot) => slot.Key == kv.Key))
			{
				ship.EquipUpgradePiece(kv.Key, val);
			}
		}
	}

	public bool IsFahdaVisible()
	{
		if (_bossCorsairParty != null && _bossCorsairParty.IsActive)
		{
			return _bossCorsairParty.IsVisible;
		}
		return false;
	}
}
