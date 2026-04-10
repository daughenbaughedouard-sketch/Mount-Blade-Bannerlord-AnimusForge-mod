using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class CaptureTheImperialMerchantPrusas : NavalStorylineQuestBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<QuestBase, bool> _003C_003E9__55_2;

		public static OnConsequenceDelegate _003C_003E9__55_1;

		public static Func<Clan, bool> _003C_003E9__66_0;

		internal void _003CAddDialogsForFinalFight_003Eb__55_1()
		{
			(((IEnumerable<QuestBase>)Campaign.Current.QuestManager.Quests).FirstOrDefault((Func<QuestBase, bool>)((QuestBase x) => x is CaptureTheImperialMerchantPrusas)) as CaptureTheImperialMerchantPrusas)._shouldRunMission = true;
		}

		internal bool _003CAddDialogsForFinalFight_003Eb__55_2(QuestBase x)
		{
			return x is CaptureTheImperialMerchantPrusas;
		}

		internal bool _003CSpawnMainCorsairParty_003Eb__66_0(Clan x)
		{
			return ((MBObjectBase)x).StringId == "southern_pirates";
		}
	}

	private const int NumberOfCorsairParties = 2;

	private const int CalculatingBonusAmount = 50;

	private const int HonorBonusAmount = 50;

	private const int CorsairShipAiDisableTimeAsHours = 3;

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
	private int _selectedOption;

	[SaveableField(9)]
	private bool _checkpointReached;

	[SaveableField(10)]
	private bool _hasRanMissionBefore;

	private bool _shouldRunMission;

	private const string Act3Quest4CorsairPartyTemplateStringId = "storyline_act3_quest_4_corsair_generic_template";

	private const string Act3Quest4BossCorsairPartyTemplateStringId = "storyline_act3_quest_4_boss_corsair_template";

	public int SelectedOption => _selectedOption;

	public override bool WillProgressStoryline => _willProgressStoryline;

	public override TextObject Title => new TextObject("{=2eXHN7v8}Capture the Merchant Crusas", (Dictionary<string, object>)null);

	private TextObject DescriptionLogText => new TextObject("{=uGTU4k9w}Defeat Crusas' fleet and take him prisoner.", (Dictionary<string, object>)null);

	private TextObject MainCorsairShipSpawnedLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=6HCOzjBt}The way is now clear to attack {HERO.NAME}'s fleet. Destroy it!", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.Prusas.CharacterObject, false);
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
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_0039: Expected O, but got Unknown
			TextObject val = new TextObject("{=vgnaNH9O}You've learned that Purig's ally, the merchant {HERO.NAME}, is anchored in the Skatria islands. You should sail there and defeat him, along with any other Sea Hounds you find there.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.Prusas.CharacterObject, false);
			TextObjectExtensions.SetCharacterProperties(val, "ISSUE_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, false);
			return val;
		}
	}

	private TextObject QuestSucceededWithHonorableOptionLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			TextObject val = new TextObject("{=GFERb4SK}You promised {HERO.NAME} his life if he helped you capture Purig's prisoner ship.  (+{HONOR_BONUS_AMOUNT} honor bonus)", (Dictionary<string, object>)null);
			val.SetTextVariable("HONOR_BONUS_AMOUNT", 50);
			return val;
		}
	}

	private TextObject QuestSucceededWithCalculatingOptionLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			TextObject val = new TextObject("{=4wJCXVb4}You forced {HERO.NAME} to help you capture Purig's prisoner ship, promising him nothing. (+{CALCULATING_BONUS_AMOUNT} calculating bonus)", (Dictionary<string, object>)null);
			val.SetTextVariable("CALCULATING_BONUS_AMOUNT", 50);
			return val;
		}
	}

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act3Quest4;

	protected override string MainPartyTemplateStringId => "storyline_act3_quest_4_main_party_template";

	public CaptureTheImperialMerchantPrusas(string questId, Hero questGiver, CampaignVec2 corsairSpawnPosition)
		: base(questId, questGiver, CampaignTime.Never, 0)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		_willProgressStoryline = false;
		_numberOfDefeatedCorsairParties = 0;
		_corsairParties = new List<MobileParty>();
		_bossCorsairParty = null;
		_corsairSpawnPosition = corsairSpawnPosition;
		((QuestBase)this).AddLog(DescriptionLogText, false);
	}

	protected override void OnFinalizeInternal()
	{
		_playerStartsQuestLog = null;
		DestroyCorsairParties();
	}

	protected override void InitializeQuestOnGameLoadInternal()
	{
		((QuestBase)this).SetDialogs();
		AddGameMenus();
	}

	protected override void SetDialogs()
	{
		AddDialogsForFinalFight();
	}

	protected override void OnStartQuestInternal()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		_numberOfDefeatedCorsairParties = 2;
		SpawnMainCorsairParty();
		_willProgressStoryline = true;
		MBInformationManager.AddQuickInformation(new TextObject("{=vbrXtMyM}Feel that hot fetid air? It means we’re in the Skatrias, now. The foe is near…", (Dictionary<string, object>)null), 200, (BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, "");
	}

	protected override void HourlyTick()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		foreach (MobileParty corsairParty in _corsairParties)
		{
			if (corsairParty.IsActive && !corsairParty.IsMoving && !corsairParty.Ai.IsDisabled)
			{
				CampaignVec2 val = NavigationHelper.FindReachablePointAroundPosition(_corsairSpawnPosition, (NavigationType)2, 20f, 5f, false);
				corsairParty.SetMoveGoToPoint(val, (NavigationType)2);
			}
		}
	}

	protected override void IsNavalQuestPartyInternal(PartyBase party, NavalStorylinePartyData data)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber maxPartySizeLimitFromTemplate;
		if (_corsairParties.Any((MobileParty c) => c.Party == party))
		{
			PartyTemplateObject templateObject = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_4_corsair_generic_template");
			maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(templateObject);
			data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber;
			data.IsQuestParty = true;
		}
		else if (_bossCorsairParty != null && _bossCorsairParty.Party == party)
		{
			PartyTemplateObject templateObject2 = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_4_boss_corsair_template");
			maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(templateObject2);
			data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber;
			data.IsQuestParty = true;
		}
	}

	protected override void OnCompleteWithSuccessInternal()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		MobileParty.MainParty.MemberRoster.RemoveTroop(NavalStorylineData.Bjolgur.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
		NavalStorylineData.Bjolgur.ChangeState((CharacterStates)6);
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest4Succeeded);
	}

	protected override void OnFailedInternal()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		MobileParty.MainParty.MemberRoster.RemoveTroop(NavalStorylineData.Bjolgur.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
		NavalStorylineData.Bjolgur.ChangeState((CharacterStates)6);
	}

	public void OnCheckPointReached()
	{
		_checkpointReached = true;
	}

	protected override void RegisterEventsInternal()
	{
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
		CampaignEvents.MapEventStarted.AddNonSerializedListener((object)this, (Action<MapEvent, PartyBase, PartyBase>)OnMapEventStarted);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.OnShipOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Ship, PartyBase, ShipOwnerChangeDetail>)OnShipOwnerChanged);
		CampaignEvents.BeforeGameMenuOpenedEvent.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnBeforeGameMenuOpened);
		CampaignEvents.ConversationEnded.AddNonSerializedListener((object)this, (Action<IEnumerable<CharacterObject>>)OnConversationEnded);
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

	private void OnShipOwnerChanged(Ship ship, PartyBase partyBase, ShipOwnerChangeDetail shipOwnerChangeDetail)
	{
		if (partyBase == PartyBase.MainParty && ship.IsInvulnerable)
		{
			ship.IsInvulnerable = false;
		}
	}

	private void OnConversationEnded(IEnumerable<CharacterObject> conversationCharacters)
	{
		if (NavalStorylineData.IsNavalStoryLineActive() && _battleWon && conversationCharacters.Contains(NavalStorylineData.Prusas.CharacterObject))
		{
			switch (_selectedOption)
			{
			case 1:
				OnPlayerSelectsOption1();
				break;
			case 2:
				OnPlayerSelectsOption2();
				break;
			default:
				Debug.FailedAssert("Quest selected option is wrong!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\Quests\\CaptureTheImperialMerchantPrusas.cs", "OnConversationEnded", 255);
				break;
			}
		}
	}

	private void OnBeforeGameMenuOpened(MenuCallbackArgs args)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (!NavalStorylineData.IsNavalStoryLineActive() || PlayerEncounter.EncounteredParty == null || !PlayerEncounter.EncounteredParty.IsMobile || !PlayerEncounter.EncounteredParty.IsNavalStorylineQuestParty())
		{
			return;
		}
		if (!_corsairParties.Contains(PlayerEncounter.EncounteredParty.MobileParty))
		{
			PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
			MobileParty bossCorsairParty = _bossCorsairParty;
			if (encounteredParty != ((bossCorsairParty != null) ? bossCorsairParty.Party : null))
			{
				return;
			}
		}
		string stringId = args.MenuContext.GameMenu.StringId;
		if (stringId == "naval_storyline_encounter_meeting")
		{
			PlayerEncounter.SetMeetingDone();
		}
		else if (stringId == "naval_storyline_encounter")
		{
			TextObject val = new TextObject("{=7b05ZaVm}You are in the Skatrias. The jagged silhouettes of small rocky islands, streaked with gull dung, stretch southwest to the horizon.{NEW_LINE}{NEW_LINE}Through the hazy air you make out the outline of a sail. It’s still quite distant, but closing fast. They are clearly Sea Hounds, ready to pounce on anyone who ventures into their hunting grounds in the Skatrias.", (Dictionary<string, object>)null).SetTextVariable("NEW_LINE", "\n");
			MBTextManager.SetTextVariable("ENCOUNTER_TEXT", val, false);
		}
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
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
		if ((string?)obj == "naval_storyline_encounter" && PlayerEncounter.EncounteredParty != null && NavalStorylineData.IsNavalStoryLineActive())
		{
			MobileParty bossCorsairParty = _bossCorsairParty;
			if (((bossCorsairParty != null) ? bossCorsairParty.Party : null) == PlayerEncounter.EncounteredParty)
			{
				NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest4EncounterMenu);
				GameMenu.ActivateGameMenu("naval_storyline_act_3_quest_4_encounter_menu");
			}
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		if (PlayerEncounter.Current == null)
		{
			return;
		}
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		MobileParty bossCorsairParty = _bossCorsairParty;
		if (encounteredParty != ((bossCorsairParty != null) ? bossCorsairParty.Party : null))
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
				_battleWon = true;
			}
		}
		else if ((int)PlayerEncounter.WinningSide == -1)
		{
			_battleWon = false;
		}
		else
		{
			Debug.FailedAssert("unhandled case", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\Quests\\CaptureTheImperialMerchantPrusas.cs", "OnMissionEnded", 319);
		}
	}

	private void OnMobilePartyDestroyed(MobileParty party, PartyBase partyBase)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (NavalStorylineData.IsNavalStoryLineActive() && _playerStartsQuestLog != null && _corsairParties.Contains(party))
		{
			_numberOfDefeatedCorsairParties++;
			_corsairParties.Remove(party);
			((QuestBase)this).UpdateQuestTaskStage(_playerStartsQuestLog, _numberOfDefeatedCorsairParties);
			if (2 == _numberOfDefeatedCorsairParties)
			{
				SpawnMainCorsairParty();
				((QuestBase)this).AddLog(MainCorsairShipSpawnedLogText, false);
				_bossCorsairParty.SetMoveGoToPoint(MobileParty.MainParty.Position, (NavigationType)2);
			}
			else
			{
				MBInformationManager.AddQuickInformation(new TextObject("{=Kal82TKK}There may be more Sea Hounds patrolling these islands. Let's keep searching.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, "");
			}
		}
	}

	private void AddDialogsForFinalFight()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_00e9: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		//IL_014d: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		//IL_016e: Expected O, but got Unknown
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected O, but got Unknown
		//IL_01d4: Expected O, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Expected O, but got Unknown
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Expected O, but got Unknown
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Expected O, but got Unknown
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Expected O, but got Unknown
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Expected O, but got Unknown
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Expected O, but got Unknown
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Expected O, but got Unknown
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Expected O, but got Unknown
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Expected O, but got Unknown
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Expected O, but got Unknown
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Expected O, but got Unknown
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Expected O, but got Unknown
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Expected O, but got Unknown
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Expected O, but got Unknown
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Expected O, but got Unknown
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Expected O, but got Unknown
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Expected O, but got Unknown
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Expected O, but got Unknown
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Expected O, but got Unknown
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Expected O, but got Unknown
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Expected O, but got Unknown
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Expected O, but got Unknown
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Expected O, but got Unknown
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Expected O, but got Unknown
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Expected O, but got Unknown
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Expected O, but got Unknown
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Expected O, but got Unknown
		//IL_0418: Expected O, but got Unknown
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Expected O, but got Unknown
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Expected O, but got Unknown
		//IL_044a: Expected O, but got Unknown
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Expected O, but got Unknown
		//IL_046b: Expected O, but got Unknown
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Expected O, but got Unknown
		//IL_048c: Expected O, but got Unknown
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Expected O, but got Unknown
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Expected O, but got Unknown
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Expected O, but got Unknown
		//IL_04f2: Expected O, but got Unknown
		//IL_0502: Unknown result type (might be due to invalid IL or missing references)
		//IL_050e: Expected O, but got Unknown
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_052a: Expected O, but got Unknown
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_054b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Expected O, but got Unknown
		//IL_0558: Expected O, but got Unknown
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Expected O, but got Unknown
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Expected O, but got Unknown
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Expected O, but got Unknown
		//IL_05be: Expected O, but got Unknown
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Expected O, but got Unknown
		//IL_05df: Expected O, but got Unknown
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Expected O, but got Unknown
		//IL_0600: Expected O, but got Unknown
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Expected O, but got Unknown
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0638: Expected O, but got Unknown
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0659: Unknown result type (might be due to invalid IL or missing references)
		//IL_0666: Expected O, but got Unknown
		//IL_0666: Expected O, but got Unknown
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0687: Expected O, but got Unknown
		//IL_0687: Expected O, but got Unknown
		//IL_0690: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a8: Expected O, but got Unknown
		//IL_06a8: Expected O, but got Unknown
		//IL_06b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c9: Expected O, but got Unknown
		//IL_06c9: Expected O, but got Unknown
		//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06de: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ea: Expected O, but got Unknown
		//IL_06ea: Expected O, but got Unknown
		//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_070b: Expected O, but got Unknown
		//IL_070b: Expected O, but got Unknown
		//IL_0714: Unknown result type (might be due to invalid IL or missing references)
		//IL_0720: Unknown result type (might be due to invalid IL or missing references)
		//IL_072c: Expected O, but got Unknown
		//IL_072c: Expected O, but got Unknown
		//IL_0735: Unknown result type (might be due to invalid IL or missing references)
		//IL_0741: Unknown result type (might be due to invalid IL or missing references)
		//IL_074d: Expected O, but got Unknown
		//IL_074d: Expected O, but got Unknown
		//IL_075d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0769: Expected O, but got Unknown
		//IL_0770: Unknown result type (might be due to invalid IL or missing references)
		//IL_077a: Expected O, but got Unknown
		//IL_078a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0796: Expected O, but got Unknown
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a7: Expected O, but got Unknown
		//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d5: Expected O, but got Unknown
		//IL_07d5: Expected O, but got Unknown
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Expected O, but got Unknown
		TextObject val = new TextObject("{=A1e4qar9}Did you see that big fiery ball? Not very accurate, I’ll warrant, but if one of our ships gets hit by one of those… Those who don’t jump into the sea in time will die a nasty death.", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=sawnbWQP}I’ve heard Crusas does this… He doesn’t try to maneuver or run, but lashes his ships together, building himself a floating fortress. He mounts mangonels on them, and peppers any attackers with flaming pitch. Not a bad tactic, if you’ve got the time to prepare and you just want to be left alone. Most attackers will keep their distance and look for easier prey.", (Dictionary<string, object>)null);
		TextObject val3 = new TextObject("{=Rc2iUkN2}No fortress is invulnerable.", (Dictionary<string, object>)null);
		TextObject val4 = new TextObject("{=ZYheTO7N}How do we counter this?", (Dictionary<string, object>)null);
		TextObject val5 = new TextObject("{=G5gTXNKi}If all our ships row in together, we’d be presenting enough targets that we’re bound to get hit. So let’s not do that. Here’s another idea…", (Dictionary<string, object>)null);
		TextObject val6 = new TextObject("{=0AWyunPW}Our captured ship, the Golden Wasp, is fast and maneuverable and has that ballista. If we make it as light as possible by removing all cargo and move in our strongest rowers to man the oars, we can dart within range while avoiding that flaming pitch. Then we can use the ballista to take out the mangonels one by one, and when they’re all down, the rest of us will storm in and clear their decks.", (Dictionary<string, object>)null);
		TextObject val7 = new TextObject("{=b8XvnNSs}Sounds like good fun. I’ll do it.", (Dictionary<string, object>)null);
		TextObject val8 = new TextObject("{=PUxIpByI}I’m not sure about this. Maybe you can command the Golden Wasp.", (Dictionary<string, object>)null);
		TextObject val9 = new TextObject("{=kW7yU5CE}I saw you handle that fireship at Omor, and I think you’re the one to take the helm. I’ll come with you though, to keep my men rowing briskly.", (Dictionary<string, object>)null);
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		string text = default(string);
		string text2 = default(string);
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200).GenerateToken(ref text).GenerateToken(ref text2)
			.NpcLine(val, new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(CaptureTheImperialMerchantPrusas)) && Hero.OneToOneConversationHero == NavalStorylineData.Bjolgur && !_hasRanMissionBefore))
			.NpcLine(val2, new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(val3, new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption(val4, new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.GotoDialogState(text)
			.EndPlayerOptions()
			.NpcLine(val5, new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainHero), text, (string)null)
			.NpcLine(val6, new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(val7, new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.GotoDialogState(text2)
			.PlayerOption(val8, new OnMultipleConversationConsequenceDelegate(IsBjolgur), (string)null, (string)null)
			.GotoDialogState(text2)
			.EndPlayerOptions()
			.NpcLine(val9, new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainHero), text2, (string)null)
			.CloseDialog();
		object obj2 = _003C_003Ec._003C_003E9__55_1;
		if (obj2 == null)
		{
			OnConsequenceDelegate val10 = delegate
			{
				(((IEnumerable<QuestBase>)Campaign.Current.QuestManager.Quests).FirstOrDefault((Func<QuestBase, bool>)((QuestBase x) => x is CaptureTheImperialMerchantPrusas)) as CaptureTheImperialMerchantPrusas)._shouldRunMission = true;
			};
			_003C_003Ec._003C_003E9__55_1 = val10;
			obj2 = (object)val10;
		}
		conversationManager.AddDialogFlow(obj.Consequence((OnConsequenceDelegate)obj2), (object)this);
		TextObject val11 = new TextObject("{=DaYQ2dm8}That was a good fight! You did a fine job taking out those mangonels.", (Dictionary<string, object>)null);
		TextObject val12 = new TextObject("{=0x6OBWqY}Now then… I wish to present you with my old acquaintance, Salautas Crusas, who gave himself up when the last of his men fell to our swords. He seems very sure of himself for a man in his circumstances, and will no doubt try to bluster his way out of trouble.", (Dictionary<string, object>)null);
		TextObject val13 = new TextObject("{=1L0smluY}Crusas! Step forward.", (Dictionary<string, object>)null);
		TextObject val14 = new TextObject("{=1aHDn1cc}I am Salautas Crusas. I sail under the protection of the Sea Hounds. If you kill me, it will not go well for you.", (Dictionary<string, object>)null);
		TextObject val15 = new TextObject("{=Y2hbEtJN}Your threats mean nothing to me. Tell me about your deals with Purig.", (Dictionary<string, object>)null);
		TextObject val16 = new TextObject("{=zIBIcnNa}You’re a slaver, the scum of the seas. Talk fast if you value your life.", (Dictionary<string, object>)null);
		TextObject val17 = new TextObject("{=edUrD21k}Yes, I buy slaves. They work my sulfur mines. Sulfur is valuable, and if I did not mine it another would. Anyway, these islands are part of no kingdom and I am violating no law. Since when does a pirate like yourself care about such things?", (Dictionary<string, object>)null);
		TextObject val18 = new TextObject("{=H5iCH92M}I am no pirate, but a liberator. I intend to free your captives.", (Dictionary<string, object>)null);
		TextObject val19 = new TextObject("{=kEBVuiUY}I have reason to believe that one of you slaving bastards has my sister.", (Dictionary<string, object>)null);
		TextObject val20 = new TextObject("{=hp67Xmzj}So then… I believe I have heard of you. {PLAYER.NAME}? Purig spoke of you. From what I know, I think I can be of use to you. Do we have a bargain? I tell you what I know, and you give my freedom.", (Dictionary<string, object>)null);
		TextObjectExtensions.SetCharacterProperties(val20, "PLAYER", Hero.MainHero.CharacterObject, false);
		TextObject val21 = new TextObject("{=v3NQFt1b}We might, if you speak truthfully.", (Dictionary<string, object>)null);
		TextObject val22 = new TextObject("{=J0j7IGno}You are in no place to speak of bargains.", (Dictionary<string, object>)null);
		TextObject val23 = new TextObject("{=l8XH22F7}So then. When I last spoke to Purig, I saw your sister among his captives, and tried to buy her. ‘Not that one,’ he said. ‘That’s my insurance against a pair of avenging furies.’ I think he grudgingly admired how persistently you pursued him.", (Dictionary<string, object>)null);
		TextObject val24 = new TextObject("{=JYSOmwV8}He told me the whole story. Apparently, you had taken passage with him on some voyage to the north, hoping to find and free your sister from pirates. Then, you stole his ship - or so he said. Realizing that you were a dangerous enemy, he made inquiries among his Sea Hound allies to find her. Now he keeps her as a hostage on a ship in his fleet.", (Dictionary<string, object>)null);
		TextObject val25 = new TextObject("{=blWf6oTJ}So.. I can tell you how to find Purig, which means you’ve found your sister as well. But if you harm me, it’s likely you’ll never have such a chance again. So I repeat - do we have a bargain? ", (Dictionary<string, object>)null);
		TextObject val26 = new TextObject("{=jdBPxHZQ}And I repeat: speak the full truth, and we might.", (Dictionary<string, object>)null);
		TextObject val27 = new TextObject("{=MebLhJmj}You try my patience. Speak if you value your life.", (Dictionary<string, object>)null);
		TextObject val28 = new TextObject("{=udKuGe2a}Indeed… So then, Purig has run a bit short of money, and has arranged to sell off some of his captives in Angranfjord, his hideaway in the north. He will be anchored there for the next several weeks, doing business with his favored buyers. You may be able to get close to him without him suspecting that anything is amiss. He will not sell your sister, though, as I explained.", (Dictionary<string, object>)null);
		TextObject val29 = new TextObject("{=Yj4RhLbo}Were you to be one of these buyers?", (Dictionary<string, object>)null);
		TextObject val30 = new TextObject("{=kh5HVkT1}Among others, yes.", (Dictionary<string, object>)null);
		TextObject val31 = new TextObject("{=G7ekdQvI}Good. Then we will take your ship. It has fine lines and expensive fittings, and I have no doubt that Purig, who has an eye for costly things, would recognize it instantly", (Dictionary<string, object>)null);
		TextObject val32 = new TextObject("{=zDG4dNbj}{PLAYER.NAME}... If Purig is holding your sister as a hostage, then capturing his roundship will be a very delicate affair. If he sees Crusas’ ship and believes that we are Crusas, we may be able to allay his suspicions while we sneak aboard and turn things to our advantage.", (Dictionary<string, object>)null);
		TextObjectExtensions.SetCharacterProperties(val32, "PLAYER", Hero.MainHero.CharacterObject, false);
		TextObject val33 = new TextObject("{=0L1ZKRk4}We shall need to think on this, but it might even be good to keep Crusas with us, to converse with Purig or his crew.", (Dictionary<string, object>)null);
		TextObject val34 = new TextObject("{=QmIfTGw4}Good news, Crusas! You are indeed worth more to us alive than dead, for now.", (Dictionary<string, object>)null);
		TextObject val35 = new TextObject("{=SsAit4jx}For now, you say. What, might I ask, is to be my fate?", (Dictionary<string, object>)null);
		TextObject val36 = new TextObject("{=ijvIIOfv}If you don’t play us false, we’ll have mercy on you. (+{HONOR_BONUS_AMOUNT} Honor Bonus)", (Dictionary<string, object>)null);
		val36.SetTextVariable("HONOR_BONUS_AMOUNT", 50);
		TextObject val37 = new TextObject("{=zkYn0OKb}I will make you no promises. (+{CALCULATING_BONUS_AMOUNT} Calculating Bonus)", (Dictionary<string, object>)null);
		val37.SetTextVariable("CALCULATING_BONUS_AMOUNT", 50);
		TextObject val38 = new TextObject("{=uUrrMnad}Well, that’s decided then. We should return to Ostican to refit and gather our allies, then prepare to sail for Angranfjord.", (Dictionary<string, object>)null);
		string text3 = default(string);
		string text4 = default(string);
		string text5 = default(string);
		string text6 = default(string);
		string text7 = default(string);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1200).GenerateToken(ref text3).GenerateToken(ref text4)
			.GenerateToken(ref text5)
			.GenerateToken(ref text6)
			.GenerateToken(ref text7)
			.NpcLine(val11, new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.Condition(new OnConditionDelegate(MultiAgentConversationCondition))
			.NpcLine(val12, new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(val13, new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.NpcLine(val14, new OnMultipleConversationConsequenceDelegate(IsCrusas), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(val15, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.GotoDialogState(text3)
			.PlayerOption(val16, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.GotoDialogState(text3)
			.EndPlayerOptions()
			.NpcLine(val17, new OnMultipleConversationConsequenceDelegate(IsCrusas), new OnMultipleConversationConsequenceDelegate(IsMainHero), text3, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(val18, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.GotoDialogState(text4)
			.PlayerOption(val19, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.GotoDialogState(text4)
			.EndPlayerOptions()
			.NpcLine(val20, new OnMultipleConversationConsequenceDelegate(IsCrusas), new OnMultipleConversationConsequenceDelegate(IsMainHero), text4, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(val21, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.GotoDialogState(text5)
			.PlayerOption(val22, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.GotoDialogState(text5)
			.EndPlayerOptions()
			.NpcLine(val23, new OnMultipleConversationConsequenceDelegate(IsCrusas), new OnMultipleConversationConsequenceDelegate(IsMainHero), text5, (string)null)
			.NpcLine(val24, new OnMultipleConversationConsequenceDelegate(IsCrusas), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(val25, new OnMultipleConversationConsequenceDelegate(IsCrusas), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(val26, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.GotoDialogState(text6)
			.PlayerOption(val27, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.GotoDialogState(text6)
			.EndPlayerOptions()
			.NpcLine(val28, new OnMultipleConversationConsequenceDelegate(IsCrusas), new OnMultipleConversationConsequenceDelegate(IsMainHero), text6, (string)null)
			.NpcLine(val29, new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.NpcLine(val30, new OnMultipleConversationConsequenceDelegate(IsCrusas), new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null)
			.NpcLine(val31, new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.NpcLine(val32, new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(val33, new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
			.NpcLine(val34, new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.NpcLine(val35, new OnMultipleConversationConsequenceDelegate(IsCrusas), new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(val36, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_selectedOption = 1;
			})
			.GotoDialogState(text7)
			.PlayerOption(val37, new OnMultipleConversationConsequenceDelegate(IsCrusas), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_selectedOption = 2;
			})
			.GotoDialogState(text7)
			.EndPlayerOptions()
			.NpcLine(val38, new OnMultipleConversationConsequenceDelegate(IsBjolgur), new OnMultipleConversationConsequenceDelegate(IsMainHero), text7, (string)null)
			.CloseDialog(), (object)this);
	}

	private void OnPlayerSelectsOption1()
	{
		TraitLevelingHelper.OnIssueSolvedThroughQuest(((QuestBase)this).QuestGiver, new Tuple<TraitObject, int>[1]
		{
			new Tuple<TraitObject, int>(DefaultTraits.Honor, 50)
		});
		((QuestBase)this).AddLog(QuestSucceededWithHonorableOptionLogText, false);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private void OnPlayerSelectsOption2()
	{
		TraitLevelingHelper.OnIssueSolvedThroughQuest(((QuestBase)this).QuestGiver, new Tuple<TraitObject, int>[1]
		{
			new Tuple<TraitObject, int>(DefaultTraits.Calculating, 50)
		});
		((QuestBase)this).AddLog(QuestSucceededWithCalculatingOptionLogText, false);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private bool IsMainHero(IAgent agent)
	{
		return (object)agent.Character == CharacterObject.PlayerCharacter;
	}

	private bool IsCrusas(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Prusas.CharacterObject;
	}

	private bool IsBjolgur(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Bjolgur.CharacterObject;
	}

	private bool IsGunnar(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
	}

	private bool MultiAgentConversationCondition()
	{
		if (Hero.OneToOneConversationHero == NavalStorylineData.Prusas && MobileParty.MainParty.IsCurrentlyAtSea && Mission.Current != null)
		{
			Agent item = SpawnBjolgur();
			Agent item2 = SpawnGunnar();
			Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<Agent> { item, item2 }, true);
			return true;
		}
		return false;
	}

	private Agent SpawnBjolgur()
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
		AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Bjolgur.CharacterObject);
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

	private void StartBattle(bool startFromCheckpoint)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected I4, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		_battleWon = false;
		_hasRanMissionBefore = true;
		if (Hero.MainHero.IsWounded)
		{
			Hero.MainHero.Heal(Hero.MainHero.WoundedHealthLimit - Hero.MainHero.HitPoints + 1, false);
		}
		PlayerEncounter.Finish(true);
		PlayerEncounter.Start();
		PlayerEncounter.Current.SetupFields(PartyBase.MainParty, _bossCorsairParty.Party);
		PlayerEncounter.StartBattle();
		MissionInitializerRecord navalMissionInitializerTemplate = NavalStorylineData.GetNavalMissionInitializerTemplate("naval_storyline_act_3_quest_4");
		TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
		navalMissionInitializerTemplate.TerrainType = (int)faceTerrainType;
		navalMissionInitializerTemplate.NeedsRandomTerrain = false;
		navalMissionInitializerTemplate.PlayingInCampaignMode = true;
		navalMissionInitializerTemplate.RandomTerrainSeed = MBRandom.RandomInt(10000);
		navalMissionInitializerTemplate.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position);
		navalMissionInitializerTemplate.SceneHasMapPatch = false;
		NavalMissions.OpenFloatingFortressSetPieceBattleMission(navalMissionInitializerTemplate, startFromCheckpoint);
	}

	private void SpawnMainCorsairParty()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		NavalStorylineData.Prusas.ChangeState((CharacterStates)1);
		PartyTemplateObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_4_boss_corsair_template");
		_bossCorsairParty = BanditPartyComponent.CreateLooterParty("naval_corsair_boss", Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "southern_pirates")), NavalStorylineData.Act3Quest2TargetSettlement, false, val, _corsairSpawnPosition);
		MobilePartyHelper.FillPartyManuallyAfterCreation(_bossCorsairParty, val, val.GetUpperTroopLimit());
		foreach (ShipTemplateStack item in (List<ShipTemplateStack>)(object)val.ShipHulls)
		{
			for (int num = 0; num < item.MaxValue; num++)
			{
				new Ship(item.ShipHull).Owner = _bossCorsairParty.Party;
			}
		}
		TextObject val2 = GameTexts.FindText("str_lord_party_name", (string)null);
		TextObjectExtensions.SetCharacterProperties(val2, "TROOP", NavalStorylineData.Prusas.CharacterObject, false);
		_bossCorsairParty.Party.SetCustomName(val2);
		_bossCorsairParty.Party.SetCustomBanner(NavalStorylineData.CorsairBanner);
		_bossCorsairParty.IsInfoHidden = true;
		SetupCorsairParty(_bossCorsairParty);
	}

	private void SetupCorsairParty(MobileParty corsairParty)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		corsairParty.SetPartyUsedByQuest(true);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)corsairParty);
		corsairParty.IsCurrentlyAtSea = true;
		CampaignVec2 position = MobileParty.MainParty.Position;
		corsairParty.IsVisible = ((CampaignVec2)(ref position)).Distance(corsairParty.Position) <= MobileParty.MainParty.SeeingRange;
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
		//IL_0072: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00dc: Expected O, but got Unknown
		//IL_00dc: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_0111: Expected O, but got Unknown
		//IL_0111: Expected O, but got Unknown
		((QuestBase)this).AddGameMenu("naval_storyline_act_3_quest_4_encounter_menu", new TextObject("{=KBe6oPWy}You see the silhouette of a larger ship on the horizon, but its details are hard to make out. At first, you attribute this to the shimmering heat coming off of the sea, but as you close you can see that it is not one ship but several lashed together.\n\nSuddenly a flaming ball arcs out of the cluster of ships, tracing a line of smoke in the sky, before impacting a few arrow-shots from your prow and scattering fire across the water.", (Dictionary<string, object>)null), new OnInitDelegate(naval_storyline_act_3_quest_4_encounter_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("naval_storyline_act_3_quest_4_encounter_menu", "naval_storyline_act_3_quest_4_encounter_menu_continue_option", new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null), new OnConditionDelegate(naval_storyline_act_3_quest_4_encounter_menu_continue_option_on_condition), new OnConsequenceDelegate(naval_storyline_act_3_quest_4_encounter_menu_continue_option_on_consequence), false, -1);
		((QuestBase)this).AddGameMenu("naval_storyline_act_3_quest_4_encounter_retry", new TextObject("{=etH1IHNZ}You manage to put some distance between you and your enemies, and you have a moment to consider how to proceed.", (Dictionary<string, object>)null), (OnInitDelegate)null, (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("naval_storyline_act_3_quest_4_encounter_retry", "naval_storyline_act_3_quest_4_encounter_retry_continue", new TextObject("{=YHMDy3lQ}Try again", (Dictionary<string, object>)null), new OnConditionDelegate(game_menu_encounter_retry_attack_on_condition), new OnConsequenceDelegate(game_menu_encounter_retry_attack_on_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("naval_storyline_act_3_quest_4_encounter_retry", "naval_storyline_act_3_quest_4_encounter_retry_continue_from_checkpoint", new TextObject("{=rHlzkNFL}Try again from checkpoint", (Dictionary<string, object>)null), new OnConditionDelegate(game_menu_encounter_retry_continue_from_checkpoint_on_condition), new OnConsequenceDelegate(game_menu_encounter_retry_continue_from_checkpoint_on_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("naval_storyline_act_3_quest_4_encounter_retry", "naval_storyline_act_3_quest_4_encounter_retry_leave", new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null), new OnConditionDelegate(game_menu_encounter_retry_leave_on_condition), new OnConsequenceDelegate(game_menu_encounter_retry_leave_on_consequence), true, -1);
	}

	private bool game_menu_encounter_retry_attack_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		return true;
	}

	private void game_menu_encounter_retry_attack_on_consequence(MenuCallbackArgs args)
	{
		CharacterObject.PlayerCharacter.HeroObject.Heal(CharacterObject.PlayerCharacter.HeroObject.MaxHitPoints, false);
		StartBattle(startFromCheckpoint: false);
	}

	private bool game_menu_encounter_retry_continue_from_checkpoint_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		return _checkpointReached;
	}

	private void game_menu_encounter_retry_continue_from_checkpoint_on_consequence(MenuCallbackArgs args)
	{
		CharacterObject.PlayerCharacter.HeroObject.Heal(CharacterObject.PlayerCharacter.HeroObject.MaxHitPoints, false);
		StartBattle(startFromCheckpoint: true);
	}

	private bool game_menu_encounter_retry_leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		args.Tooltip = new TextObject("{=wmTjX28f}This will exit story mode and return you to the Sandbox. You can continue the storyline later by talking to Gunnar in the port again.", (Dictionary<string, object>)null);
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void game_menu_encounter_retry_leave_on_consequence(MenuCallbackArgs args)
	{
		((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
		NavalStorylineData.DeactivateNavalStoryline();
	}

	private void naval_storyline_act_3_quest_4_encounter_menu_on_init(MenuCallbackArgs args)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (_shouldRunMission)
		{
			_shouldRunMission = false;
			StartBattle(startFromCheckpoint: false);
		}
		else if (_battleWon)
		{
			PlayerEncounter.Finish(true);
			NavalStorylineData.Prusas.SetHasMet();
			ConversationCharacterData val = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, false, false, false, false, true);
			ConversationCharacterData val2 = default(ConversationCharacterData);
			((ConversationCharacterData)(ref val2))._002Ector(NavalStorylineData.Prusas.CharacterObject, (PartyBase)null, true, true, true, false, false, true);
			CampaignMission.OpenConversationMission(val, val2, "conversation_scene_sea_multi_agent", "", true);
		}
		else if (_hasRanMissionBefore)
		{
			GameMenu.SwitchToMenu("naval_storyline_act_3_quest_4_encounter_retry");
		}
	}

	private bool naval_storyline_act_3_quest_4_encounter_menu_continue_option_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		return true;
	}

	private void naval_storyline_act_3_quest_4_encounter_menu_continue_option_on_consequence(MenuCallbackArgs args)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		ConversationCharacterData val = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, false, false, false, false, false);
		ConversationCharacterData val2 = default(ConversationCharacterData);
		((ConversationCharacterData)(ref val2))._002Ector(NavalStorylineData.Bjolgur.CharacterObject, PartyBase.MainParty, true, false, false, false, false, false);
		CampaignMission.OpenConversationMission(val, val2, "", "", false);
	}

	[GameMenuInitializationHandler("naval_storyline_act_3_quest_4_encounter_menu")]
	[GameMenuInitializationHandler("naval_storyline_act_3_quest_4_encounter_retry")]
	private static void quest_game_menus_on_init_background(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName(((SettlementComponent)SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)null)).WaitMeshName);
	}

	public bool IsCrusasVisible()
	{
		if (_bossCorsairParty != null && _bossCorsairParty.IsActive)
		{
			return _bossCorsairParty.IsVisible;
		}
		return false;
	}
}
