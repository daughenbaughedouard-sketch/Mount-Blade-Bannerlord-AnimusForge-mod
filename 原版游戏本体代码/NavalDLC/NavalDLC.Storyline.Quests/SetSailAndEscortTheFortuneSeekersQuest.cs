using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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

public class SetSailAndEscortTheFortuneSeekersQuest : NavalStorylineQuestBase
{
	private const string MerchantCharacterStringId = "vlandian_fortune_seekers";

	private const string Act3Quest1CaravanPartyTemplateStringId = "storyline_act3_quest_1_caravan_party_template";

	private const string Act3Quest1GenericPartyTemplateStringId = "storyline_act3_quest_1_generic_party_template";

	private const string Act3Quest1SpecialPartyTemplateStringId = "storyline_act3_quest_1_special_party_template";

	private const int TargetSettlementArrivalRadius = 10;

	private const float MapEventInvulnerabilityDurationInHours = 8f;

	public const string PlayerPartySailPatternId = "generated_square__h4_09";

	public const string MerchantPartySailPatternId = "generated_square_l1_h4_04";

	public const string SeaHoundsPartySailPatternId = "generated_square_l1_h4_10";

	private static readonly Dictionary<string, string> MerchantShipUpgradePieces = new Dictionary<string, string> { { "sail", "sails_lvl2" } };

	private static readonly Dictionary<string, string> RegularBanditShipUpgradePieces = new Dictionary<string, string>
	{
		{ "sail", "sails_lvl2" },
		{ "side", "side_northern_shields_lvl1" }
	};

	private static readonly Dictionary<string, string> SpecialBanditShipUpgradePieces = new Dictionary<string, string>
	{
		{ "sail", "sails_lvl2" },
		{ "side", "side_northern_shields_lvl1" }
	};

	private CharacterObject _merchantCharacter;

	[SaveableField(1)]
	private bool _isMerchantPartyWaitingForEscort;

	[SaveableField(2)]
	private bool _isMerchantPartySaved;

	[SaveableField(3)]
	private bool _isAfterFightDialogDone;

	[SaveableField(4)]
	private bool _specialBattleWon;

	[SaveableField(5)]
	private MobileParty _merchantParty;

	[SaveableField(6)]
	private MobileParty _initialBanditParty;

	[SaveableField(7)]
	private MobileParty _secondBanditParty;

	[SaveableField(8)]
	private MobileParty _specialBanditParty;

	[SaveableField(9)]
	private Settlement _targetSettlement;

	[SaveableField(10)]
	private bool _willProgressStoryline;

	[SaveableField(11)]
	private bool _hasMetMerchantParty;

	private List<Vec2> _banditSpawnPositions;

	public override bool WillProgressStoryline => _willProgressStoryline;

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act3Quest1;

	public bool HasMetMerchants => _hasMetMerchantParty;

	public bool HasSavedMerchants => _isMerchantPartySaved;

	public bool IsConversationHeroTheMerchant => CharacterObject.OneToOneConversationCharacter == _merchantCharacter;

	private TextObject QuestSecondPhaseStartLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=ycq46riU}Escort the Vlandian merchants the rest of the way to {SETTLEMENT_LINK}.", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.HomeSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	protected override string MainPartyTemplateStringId => "storyline_act3_quest_1_main_party_template";

	private TextObject MerchantPartyArrivedToHomeSettlementNotification
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=7ZFbP4TO}You have successfully escorted the Vlandian merchants to {SETTLEMENT_LINK}.", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.HomeSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject FailLogText => new TextObject("{=F0bGPXyz}You failed to defend the Vlandian merchants.", (Dictionary<string, object>)null);

	public override TextObject Title => new TextObject("{=ntIGLPdc}Escort the Vlandian Merchants", (Dictionary<string, object>)null);

	private TextObject _descriptionLogText => new TextObject("{=ik68yVRc}Guard a Vlandian merchant ship sailing home from Beinland.", (Dictionary<string, object>)null);

	private TextObject _allyDefeatedText => new TextObject("{=9sfcVI0Q}Your allies were defeated. You will have to try again.", (Dictionary<string, object>)null);

	public SetSailAndEscortTheFortuneSeekersQuest(string questId, Hero questGiver, Settlement targetSettlement)
		: base(questId, questGiver, CampaignTime.Never, 0)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		_willProgressStoryline = false;
		_targetSettlement = targetSettlement;
		SetMerchantCharacterReference();
		((QuestBase)this).AddLog(_descriptionLogText, false);
	}

	protected override void SetDialogs()
	{
		AddMerchantDialogue();
	}

	protected override void InitializeQuestOnGameLoadInternal()
	{
		SetMerchantCharacterReference();
		AddGameMenus();
		((QuestBase)this).SetDialogs();
		SetBanditSpawnPositions();
		if (_merchantParty != null && _merchantParty.IsActive)
		{
			NavalDLCHelpers.SetCustomSailPatternOfPartyShips(_merchantParty, "generated_square_l1_h4_04");
		}
		MobileParty activeBanditParty = GetActiveBanditParty();
		if (activeBanditParty != null && activeBanditParty.IsActive)
		{
			NavalDLCHelpers.SetCustomSailPatternOfPartyShips(activeBanditParty, "generated_square_l1_h4_10");
		}
		if (MobileParty.MainParty.IsActive)
		{
			NavalDLCHelpers.SetCustomSailPatternOfPartyShips(MobileParty.MainParty, "generated_square__h4_09");
		}
	}

	private void SetMerchantCharacterReference()
	{
		_merchantCharacter = MBObjectManager.Instance.GetObject<CharacterObject>("vlandian_fortune_seekers");
	}

	protected override void OnStartQuestInternal()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		AddGameMenus();
		((QuestBase)this).SetDialogs();
		SpawnMerchantParty();
		SetBanditSpawnPositions();
		CampaignVec2 banditSpawnPosition = GetBanditSpawnPosition(0);
		_initialBanditParty = SpawnBanditParty("set_sail_and_escort_generic_party_1", ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_1_generic_party_template"), isSpecialParty: false, banditSpawnPosition);
		_willProgressStoryline = true;
	}

	private void SetBanditSpawnPositions()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		_banditSpawnPositions = new List<Vec2>
		{
			new Vec2(200f, 655f),
			new Vec2(202f, 615f),
			new Vec2(210f, 595f)
		};
	}

	private CampaignVec2 GetBanditSpawnPosition(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = _banditSpawnPositions[index];
		return NavigationHelper.FindReachablePointAroundPosition(new CampaignVec2(val, false), (NavigationType)2, 5f, 0f, false);
	}

	protected override void IsNavalQuestPartyInternal(PartyBase party, NavalStorylinePartyData data)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		MobileParty initialBanditParty = _initialBanditParty;
		ExplainedNumber maxPartySizeLimitFromTemplate;
		if (((initialBanditParty != null) ? initialBanditParty.Party : null) != party)
		{
			MobileParty secondBanditParty = _secondBanditParty;
			if (((secondBanditParty != null) ? secondBanditParty.Party : null) != party)
			{
				MobileParty merchantParty = _merchantParty;
				if (((merchantParty != null) ? merchantParty.Party : null) == party)
				{
					PartyTemplateObject templateObject = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_1_caravan_party_template");
					maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(templateObject);
					data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber;
					data.IsQuestParty = true;
					return;
				}
				MobileParty specialBanditParty = _specialBanditParty;
				if (((specialBanditParty != null) ? specialBanditParty.Party : null) == party)
				{
					PartyTemplateObject templateObject2 = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_1_special_party_template");
					maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(templateObject2);
					data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber;
					data.IsQuestParty = true;
				}
				return;
			}
		}
		PartyTemplateObject templateObject3 = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_1_generic_party_template");
		maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(templateObject3);
		data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber;
		data.IsQuestParty = true;
	}

	private void AddMerchantDialogue()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 100).NpcLine("{=6QkMVCgz}Ahoy! It's good to have you with us. We've seen sails, and I reckon that there are still pirates about.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _hasMetMerchantParty && !_isMerchantPartySaved && CharacterObject.OneToOneConversationCharacter == _merchantCharacter))
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 100).NpcLine("{=acz9UxsD}Thank the Heavens. And thank you. Those Sea Hound vessels would have torn us to pieces. You came just in time.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _isMerchantPartySaved && !_isAfterFightDialogDone && CharacterObject.OneToOneConversationCharacter == _merchantCharacter))
			.NpcLine("{=CowdyMzB}We would still wish to show you our gratitude. I took a collection among the men whose lives you saved today. We wish to offer you a barrel of oil and a bundle of ivory. These are the rewards of our labor over the past months, but they would mean nothing to us if our ship were seized by pirates.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				((QuestBase)this).AddLog(QuestSecondPhaseStartLog, false);
				_isAfterFightDialogDone = true;
			})
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=e69pk8m2}I accept your gift. Let us return to Ostican.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(AcceptGifts))
			.CloseDialog()
			.PlayerOption("{=sacjGtbK}You risked much for those goods. Keep them.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(RejectGifts))
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 100).NpcLine("{=acz9UxsD}Thank the Heavens. And thank you. Those Sea Hound vessels would have torn us to pieces. You came just in time.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _isMerchantPartySaved && _isAfterFightDialogDone && CharacterObject.OneToOneConversationCharacter == _merchantCharacter))
			.CloseDialog(), (object)this);
	}

	public void OnMerchantsMet()
	{
		_hasMetMerchantParty = true;
		DirectMerchantPartyToBase();
	}

	private void AcceptGifts()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		ItemRosterElement val = default(ItemRosterElement);
		((ItemRosterElement)(ref val))._002Ector(Extensions.GetRandomElementWithPredicate<ItemObject>(Items.All, (Func<ItemObject, bool>)((ItemObject x) => x.IsTradeGood && x.ItemCategory == DefaultItemCategories.Oil)), 1, (ItemModifier)null);
		PartyBase.MainParty.ItemRoster.AddToCounts(((ItemRosterElement)(ref val)).EquipmentElement, ((ItemRosterElement)(ref val)).Amount);
		ItemRosterElement val2 = default(ItemRosterElement);
		((ItemRosterElement)(ref val2))._002Ector(Extensions.GetRandomElementWithPredicate<ItemObject>(Items.All, (Func<ItemObject, bool>)((ItemObject x) => x.IsTradeGood && x.ItemCategory == NavalItemCategories.WalrusTusk)), 1, (ItemModifier)null);
		PartyBase.MainParty.ItemRoster.AddToCounts(((ItemRosterElement)(ref val2)).EquipmentElement, ((ItemRosterElement)(ref val2)).Amount);
	}

	private void RejectGifts()
	{
		TraitLevelingHelper.OnIssueSolvedThroughQuest(Hero.MainHero, new Tuple<TraitObject, int>[1]
		{
			new Tuple<TraitObject, int>(DefaultTraits.Generosity, 50)
		});
	}

	protected override void HourlyTick()
	{
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		if (_merchantParty == null || !_merchantParty.IsActive || !((QuestBase)this).IsOngoing)
		{
			return;
		}
		if (_merchantParty.MapEvent == null)
		{
			float getEncounterJoiningRadius = Campaign.Current.Models.EncounterModel.GetEncounterJoiningRadius;
			CampaignVec2 position;
			if (!_hasMetMerchantParty)
			{
				position = _merchantParty.Position;
				if (((CampaignVec2)(ref position)).DistanceSquared(MobileParty.MainParty.Position) <= getEncounterJoiningRadius * getEncounterJoiningRadius)
				{
					EncounterManager.StartPartyEncounter(MobileParty.MainParty.Party, _merchantParty.Party);
				}
			}
			if (!_isMerchantPartySaved && GetActiveBanditParty() != null)
			{
				position = _merchantParty.Position;
				if (((CampaignVec2)(ref position)).DistanceSquared(GetActiveBanditParty().Position) <= getEncounterJoiningRadius * getEncounterJoiningRadius)
				{
					MBInformationManager.AddQuickInformation(new TextObject("{=cjkHktxl}The merchant party is under attack.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_update");
					EncounterManager.StartPartyEncounter(GetActiveBanditParty().Party, _merchantParty.Party);
					return;
				}
			}
			position = _merchantParty.Position;
			if (((CampaignVec2)(ref position)).DistanceSquared(NavalStorylineData.HomeSettlement.PortPosition) <= 100f)
			{
				MBInformationManager.AddQuickInformation(MerchantPartyArrivedToHomeSettlementNotification, 0, (BasicCharacterObject)null, (Equipment)null, "");
				((QuestBase)this).CompleteQuestWithSuccess();
				return;
			}
			UtilizePartyEscortBehavior(_merchantParty, MobileParty.MainParty, ref _isMerchantPartyWaitingForEscort, 7f, 11f, new ResumePartyEscortBehaviorDelegate(DirectMerchantPartyToBase));
			MobileParty activeBanditParty = GetActiveBanditParty();
			if (activeBanditParty != null && PlayerCaptivity.CaptorParty != activeBanditParty.Party)
			{
				if (!((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)activeBanditParty))
				{
					position = activeBanditParty.Position;
					if (((CampaignVec2)(ref position)).Distance(MobileParty.MainParty.Position) < MobileParty.MainParty.SeeingRange)
					{
						((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)activeBanditParty);
					}
				}
				SetPartyAiAction.GetActionForEngagingParty(activeBanditParty, _merchantParty, (NavigationType)2, false);
				activeBanditParty.Ai.SetDoNotMakeNewDecisions(true);
			}
			AdjustMerchantPartySpeed();
		}
		else if (_merchantParty.MapEvent.IsInvulnerable)
		{
			CampaignTime battleStartTime = _merchantParty.MapEvent.BattleStartTime;
			if (((CampaignTime)(ref battleStartTime)).ElapsedHoursUntilNow > 8f)
			{
				_merchantParty.MapEvent.IsInvulnerable = false;
			}
		}
	}

	private MobileParty GetActiveBanditParty()
	{
		return _initialBanditParty ?? _secondBanditParty ?? _specialBanditParty;
	}

	private void DirectMerchantPartyToBase()
	{
		SetPartyAiAction.GetActionForVisitingSettlement(_merchantParty, NavalStorylineData.HomeSettlement, (NavigationType)2, false, true);
	}

	protected override void RegisterEventsInternal()
	{
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)MapEventEnded);
		CampaignEvents.MapEventStarted.AddNonSerializedListener((object)this, (Action<MapEvent, PartyBase, PartyBase>)MapEventStarted);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		if (NavalStorylineData.IsNavalStoryLineActive() && PlayerEncounter.Current != null && PlayerEncounter.EncounteredParty != null && PlayerEncounter.EncounteredParty.IsNavalStorylineQuestParty())
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
			if ((string?)obj == "naval_storyline_encounter_meeting")
			{
				if (PlayerEncounter.EncounteredParty == _merchantParty.Party)
				{
					if (PlayerEncounter.MeetingDone)
					{
						PlayerEncounter.LeaveEncounter = true;
					}
				}
				else
				{
					PlayerEncounter.SetMeetingDone();
				}
			}
			if ((string?)obj == "naval_storyline_encounter" && GetActiveBanditParty() != null)
			{
				MobileParty initialBanditParty = _initialBanditParty;
				if (((initialBanditParty != null) ? initialBanditParty.Party : null) != PlayerEncounter.EncounteredParty)
				{
					MobileParty secondBanditParty = _secondBanditParty;
					if (((secondBanditParty != null) ? secondBanditParty.Party : null) != PlayerEncounter.EncounteredParty)
					{
						goto IL_0131;
					}
				}
				if (PlayerEncounter.EncounteredBattle == null || !PlayerEncounter.EncounteredBattle.HasWinner)
				{
					MapEvent encounteredBattle = PlayerEncounter.EncounteredBattle;
					if (encounteredBattle == null || !encounteredBattle.InvolvedParties.Contains(_merchantParty.Party))
					{
						MBTextManager.SetTextVariable("ENCOUNTER_TEXT", new TextObject("{=Iu7TkxZo}“A ship! A ship!” calls out one of your lookouts. You can see it too - a square sail, outlined against the steel-gray northern sky. One the Sea Hounds has spotted you, and thinks to make you its prey.", (Dictionary<string, object>)null), false);
					}
					else
					{
						MBTextManager.SetTextVariable("ENCOUNTER_TEXT", new TextObject("{=XfqPvVDc}“A ship! A ship!” calls out one of your lookouts. You can see it too - a square sail, outlined against the steel-gray northern sky. One of the Sea Hounds stalking the merchant seems to be closing in on its prey.", (Dictionary<string, object>)null), false);
					}
				}
			}
		}
		goto IL_0131;
		IL_0131:
		MenuContext menuContext2 = args.MenuContext;
		object obj2;
		if (menuContext2 == null)
		{
			obj2 = null;
		}
		else
		{
			GameMenu gameMenu2 = menuContext2.GameMenu;
			obj2 = ((gameMenu2 != null) ? gameMenu2.StringId : null);
		}
		if ((string?)obj2 == "naval_storyline_encounter" && GetActiveBanditParty() != null && PlayerEncounter.Current != null)
		{
			IEnumerable<PartyBase> involvedParties = PlayerEncounter.EncounteredBattle.InvolvedParties;
			MobileParty specialBanditParty = _specialBanditParty;
			if (involvedParties.Contains((specialBanditParty != null) ? specialBanditParty.Party : null) || PlayerEncounter.EncounteredMobileParty == _specialBanditParty)
			{
				GameMenu.SwitchToMenu("naval_storyline_act3_quest1_setpiece_menu");
			}
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		if (Mission.Current.IsNavalBattle && PlayerEncounter.Current != null && PlayerEncounter.EncounteredParty != null)
		{
			MobileParty specialBanditParty = _specialBanditParty;
			if (((specialBanditParty != null) ? specialBanditParty.Party : null) == PlayerEncounter.EncounteredParty && PlayerEncounter.Battle != null && (int)PlayerEncounter.Battle.BattleState == 1)
			{
				_specialBattleWon = true;
				_isMerchantPartySaved = true;
			}
		}
	}

	private void SpawnMerchantParty()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003c: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		Clan val = new Clan();
		((MBObjectBase)val).StringId = Campaign.Current.CampaignObjectManager.FindNextUniqueStringId<Clan>("naval_storyline_vlandian_merchant_clan");
		val.ChangeClanName(new TextObject("{=FjwRsf1C}Vlandia", (Dictionary<string, object>)null), new TextObject("{=FjwRsf1C}Vlandia", (Dictionary<string, object>)null));
		val.Culture = MBObjectManager.Instance.GetObject<CultureObject>("vlandia");
		val.Banner = Banner.CreateRandomClanBanner(-1);
		val.Color = 4287441178u;
		val.Color2 = 4294426438u;
		val.Banner.ChangePrimaryColor(4287441178u);
		val.Banner.ChangeBackgroundColor(4287441178u, 4287441178u);
		val.Banner.ChangeIconColors(4294426438u);
		TextObject val2 = new TextObject("{=FyfpoKvX}Vlandian Merchants", (Dictionary<string, object>)null);
		CampaignVec2 portPosition = _targetSettlement.PortPosition;
		PartyTemplateObject val3 = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_1_caravan_party_template");
		_merchantParty = CustomPartyComponent.CreateCustomPartyWithPartyTemplate(portPosition, 0.1f, NavalStorylineData.HomeSettlement, val2, val, val3, (Hero)null, "camel", "camel_saddle_b", MobileParty.MainParty.Speed * 1.5f, false);
		NavalDLCHelpers.AddUpgradePiecesToPartyShips(_merchantParty, MerchantShipUpgradePieces);
		NavalDLCHelpers.SetCustomSailPatternOfPartyShips(_merchantParty, "generated_square_l1_h4_04");
		foreach (Ship item in (List<Ship>)(object)_merchantParty.Ships)
		{
			item.IsInvulnerable = true;
		}
		_merchantParty.MemberRoster.AddToCounts(_merchantCharacter, 1, false, 0, 0, true, -1);
		_merchantParty.ItemRoster.AddToCounts(DefaultItems.Grain, 40);
		_merchantParty.IgnoreByOtherPartiesTill(((QuestBase)this).QuestDueTime);
		SetPartyAiAction.GetActionForEngagingParty(_merchantParty, MobileParty.MainParty, (NavigationType)2, false);
		_merchantParty.Ai.SetDoNotMakeNewDecisions(true);
		_merchantParty.SetPartyUsedByQuest(true);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_merchantParty);
	}

	private void AdjustMerchantPartySpeed()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasMetMerchantParty)
		{
			return;
		}
		MobileParty activeBanditParty = GetActiveBanditParty();
		MobileParty val = MobileParty.MainParty;
		if (!val.IsActive || activeBanditParty == null || !activeBanditParty.IsActive)
		{
			return;
		}
		float num = Campaign.Current.Models.EncounterModel.GetEncounterJoiningRadius * 2.5f;
		CampaignVec2 position = activeBanditParty.Position;
		if (((CampaignVec2)(ref position)).DistanceSquared(_merchantParty.Position) <= num * num)
		{
			val = activeBanditParty;
		}
		float referencePartySpeed = GetReferencePartySpeed(val);
		float speed = _merchantParty.Speed;
		PartyComponent partyComponent = _merchantParty.PartyComponent;
		CustomPartyComponent val2 = (CustomPartyComponent)(object)((partyComponent is CustomPartyComponent) ? partyComponent : null);
		while (referencePartySpeed < speed || ShouldMerchantPartyCatchUpWithParty(val, referencePartySpeed, speed))
		{
			referencePartySpeed = GetReferencePartySpeed(val);
			if (speed > referencePartySpeed || MBMath.ApproximatelyEqualsTo(referencePartySpeed, speed, 1E-05f))
			{
				val2.SetBaseSpeed(val2.BaseSpeed - 0.05f);
			}
			else if (ShouldMerchantPartyCatchUpWithParty(val, referencePartySpeed, speed))
			{
				val2.SetBaseSpeed(val2.BaseSpeed + 0.05f);
			}
			speed = _merchantParty.Speed;
		}
	}

	private bool ShouldMerchantPartyCatchUpWithParty(MobileParty referenceParty, float cachedReferencePartySpeed, float cachedMerchantPartySpeed)
	{
		if (referenceParty.IsMainParty && cachedMerchantPartySpeed <= 5.5f)
		{
			return MathF.Abs(cachedMerchantPartySpeed - cachedReferencePartySpeed) > 0.7f;
		}
		return false;
	}

	private float GetReferencePartySpeed(MobileParty referenceParty)
	{
		float num = 1f;
		if (referenceParty.IsActive)
		{
			num = referenceParty.Speed;
			if (referenceParty == GetActiveBanditParty())
			{
				num -= 0.5f;
			}
		}
		return num;
	}

	private MobileParty SpawnBanditParty(string stringId, PartyTemplateObject partyTemplate, bool isSpecialParty, CampaignVec2 banditPartyPosition)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		Hideout val = SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement x) => x.IsActive));
		Clan val2 = ((IEnumerable<Clan>)Clan.All).FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "northern_pirates"));
		MobileParty val3 = BanditPartyComponent.CreateBanditParty(stringId, val2, ((SettlementComponent)val).Settlement.Hideout, false, partyTemplate, banditPartyPosition);
		val3.Party.SetCustomName(new TextObject("{=SKC3FeGR}Sea Hounds", (Dictionary<string, object>)null));
		val3.SetPartyUsedByQuest(true);
		val3.SetLandNavigationAccess(false);
		foreach (Ship item in (List<Ship>)(object)val3.Ships)
		{
			item.IsInvulnerable = true;
			if (isSpecialParty)
			{
				item.IsTradeable = false;
				item.IsUsedByQuest = true;
			}
		}
		NavalDLCHelpers.AddUpgradePiecesToPartyShips(val3, isSpecialParty ? SpecialBanditShipUpgradePieces : RegularBanditShipUpgradePieces);
		NavalDLCHelpers.SetCustomSailPatternOfPartyShips(val3, "generated_square_l1_h4_10");
		val3.IgnoreByOtherPartiesTill(((QuestBase)this).QuestDueTime);
		val3.Ai.SetDoNotMakeNewDecisions(true);
		val3.Party.SetCustomBanner(NavalStorylineData.CorsairBanner);
		return val3;
	}

	private void MapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
	{
		if (attackerParty.IsNavalStorylineQuestParty())
		{
			foreach (Ship item in (List<Ship>)(object)attackerParty.Ships)
			{
				item.IsInvulnerable = false;
			}
		}
		if (defenderParty.IsNavalStorylineQuestParty())
		{
			foreach (Ship item2 in (List<Ship>)(object)defenderParty.Ships)
			{
				item2.IsInvulnerable = false;
			}
		}
		if (defenderParty.MobileParty == _merchantParty && attackerParty.MobileParty == GetActiveBanditParty())
		{
			mapEvent.IsInvulnerable = true;
		}
	}

	private void MapEventEnded(MapEvent mapEvent)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Invalid comparison between Unknown and I4
		if (!_isMerchantPartySaved && (int)mapEvent.WinningSide != -1 && (int)mapEvent.DefeatedSide != -1)
		{
			MapEventSide mapEventSide = mapEvent.GetMapEventSide(mapEvent.WinningSide);
			MapEventSide mapEventSide2 = mapEvent.GetMapEventSide(mapEvent.DefeatedSide);
			MobileParty banditParty = GetActiveBanditParty();
			if (((IEnumerable<MapEventParty>)mapEventSide2.Parties).Any((MapEventParty t) => t.Party == _merchantParty.Party) && !mapEventSide2.IsMainPartyAmongParties())
			{
				OnMerchantPartyDestroyed();
			}
			else if (((IEnumerable<MapEventParty>)mapEventSide2.Parties).Any(delegate(MapEventParty t)
			{
				PartyBase party = t.Party;
				MobileParty obj = banditParty;
				return party == ((obj != null) ? obj.Party : null);
			}))
			{
				if (mapEventSide.IsMainPartyAmongParties())
				{
					if (_merchantParty.IsActive)
					{
						OnBanditPartyDestroyed();
						if (_merchantParty.MemberRoster.TotalHealthyCount == 0 && mapEvent.InvolvedParties.Contains(_merchantParty.Party))
						{
							_merchantParty.MemberRoster.Clear();
							_merchantParty.MemberRoster.AddToCounts(_merchantCharacter, 11, false, 0, 0, true, -1);
						}
					}
					else
					{
						OnMerchantPartyDestroyed();
					}
				}
				else
				{
					OnMerchantSurvivedWithoutHelp();
				}
			}
			if (banditParty != null && banditParty.IsActive && mapEvent.InvolvedParties.Contains(banditParty.Party) && (banditParty.NavigationCapability & 2) == 2)
			{
				banditParty.SetMovePatrolAroundSettlement(NavalStorylineData.HomeSettlement, (NavigationType)2, true);
			}
		}
		if (_merchantParty != null && _merchantParty.IsActive && mapEvent.InvolvedParties.Contains(_merchantParty.Party) && !_isMerchantPartySaved && _merchantParty.MemberRoster.TotalHealthyCount > 0)
		{
			DirectMerchantPartyToBase();
		}
	}

	private void OnBanditPartyDestroyed()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (GetActiveBanditParty() == _initialBanditParty || GetActiveBanditParty() == _secondBanditParty)
		{
			CampaignVec2 banditSpawnPosition = GetBanditSpawnPosition(2);
			_specialBanditParty = SpawnBanditParty("set_sail_and_escort_special_party", ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_1_special_party_template"), isSpecialParty: true, banditSpawnPosition);
			_specialBanditParty.IsInfoHidden = true;
			_initialBanditParty = null;
			_secondBanditParty = null;
		}
	}

	private void OpenConversationWithMerchants()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		ConversationCharacterData val = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, false, false, false, false, false);
		ConversationCharacterData val2 = default(ConversationCharacterData);
		((ConversationCharacterData)(ref val2))._002Ector(_merchantCharacter, _merchantParty.Party, true, false, false, false, false, false);
		CampaignMission.OpenConversationMission(val, val2, "", "", false);
	}

	private void OnMerchantPartyDestroyed()
	{
		ShowAllyDefeatedPopUp();
	}

	private void OnMerchantSurvivedWithoutHelp()
	{
		CancelQuest();
	}

	private void CancelQuest(TextObject logText = null)
	{
		((QuestBase)this).CompleteQuestWithCancel(logText);
		NavalStorylineData.DeactivateNavalStoryline();
	}

	protected override void OnFinalizeInternal()
	{
		MobileParty activeBanditParty = GetActiveBanditParty();
		if (activeBanditParty != null && activeBanditParty.IsActive)
		{
			DestroyPartyAction.Apply((PartyBase)null, activeBanditParty);
		}
		if (_merchantParty.IsActive)
		{
			if (_merchantParty.MapEventSide != null)
			{
				_merchantParty.MapEventSide = null;
			}
			DestroyPartyAction.ApplyForDisbanding(_merchantParty, NavalStorylineData.HomeSettlement);
		}
		MobileParty merchantParty = _merchantParty;
		if (((merchantParty != null) ? merchantParty.ActualClan : null) != null)
		{
			DestroyClanAction.Apply(_merchantParty.ActualClan);
		}
	}

	private void ShowAllyDefeatedPopUp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001c: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		TextObject val = new TextObject("{=cH3Kpkwg}Ally Defeated", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)_allyDefeatedText).ToString(), true, false, ((object)val2).ToString(), (string)null, (Action)OnAllyDefeatedPopUpClosed, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void OnAllyDefeatedPopUpClosed()
	{
		CancelQuest(_allyDefeatedText);
	}

	public static void UtilizePartyEscortBehavior(MobileParty escortedParty, MobileParty escortParty, ref bool isWaitingForEscortParty, float innerRadius, float outerRadius, ResumePartyEscortBehaviorDelegate onPartyEscortBehaviorResumed, bool showDebugSpheres = false)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 position;
		if (!isWaitingForEscortParty)
		{
			position = escortParty.Position;
			if (((CampaignVec2)(ref position)).DistanceSquared(escortedParty.Position) >= outerRadius * outerRadius)
			{
				escortedParty.SetMoveGoToPoint(escortedParty.Position, (NavigationType)3);
				escortedParty.Ai.CheckPartyNeedsUpdate();
				isWaitingForEscortParty = true;
			}
		}
		else
		{
			position = escortParty.Position;
			if (((CampaignVec2)(ref position)).DistanceSquared(escortedParty.Position) <= innerRadius * innerRadius)
			{
				onPartyEscortBehaviorResumed.Invoke();
				escortedParty.Ai.CheckPartyNeedsUpdate();
				isWaitingForEscortParty = false;
			}
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
		((QuestBase)this).AddGameMenu("naval_storyline_act3_quest1_setpiece_menu", new TextObject("{=tcfyZUb8}A brief squall cuts visibility to a few bowshots, and when it clears, you see that two Sea Hound vessels have snuck up upon the merchant’s ship and are in hot pursuit. They are much faster, so unless you can close and defeat them or draw them off, it is likely that your ally will be taken.", (Dictionary<string, object>)null), new OnInitDelegate(naval_storyline_act_3_quest_1_setpiece_menu_on_init), (MenuOverlayType)4, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("naval_storyline_act3_quest1_setpiece_menu", "naval_storyline_act3_quest1_setpiece_attack", new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null), new OnConditionDelegate(naval_storyline_act3_quest1_setpiece_attack_condition), new OnConsequenceDelegate(naval_storyline_act3_quest1_setpiece_attack_consequence), false, -1);
		((QuestBase)this).AddGameMenu("set_piece_retry_menu", new TextObject("{=etH1IHNZ}You manage to put some distance between you and your enemies, and you have a moment to consider how to proceed.", (Dictionary<string, object>)null), new OnInitDelegate(set_piece_retry_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("set_piece_retry_menu", "try_again_option", new TextObject("{=YHMDy3lQ}Try again", (Dictionary<string, object>)null), new OnConditionDelegate(set_piece_retry_menu_try_again_on_condition), new OnConsequenceDelegate(encounter_menu_try_again_on_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("set_piece_retry_menu", "leave_option", new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null), new OnConditionDelegate(leave_on_condition), new OnConsequenceDelegate(leave_on_consequence), true, -1);
	}

	private void naval_storyline_act_3_quest_1_setpiece_menu_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest1SetPieceEncounterMenu);
	}

	private bool naval_storyline_act3_quest1_setpiece_attack_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		return true;
	}

	private void naval_storyline_act3_quest1_setpiece_attack_consequence(MenuCallbackArgs args)
	{
		StartBattle();
	}

	private void set_piece_retry_menu_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
		if (_specialBattleWon)
		{
			DestroyPartyAction.Apply((PartyBase)null, _specialBanditParty);
			_merchantParty.Ai.SetDoNotMakeNewDecisions(true);
			DirectMerchantPartyToBase();
			PlayerEncounter.Finish(true);
			OpenConversationWithMerchants();
			_specialBanditParty = null;
			NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest1SetPieceSucceeded);
		}
	}

	private bool set_piece_retry_menu_try_again_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		return true;
	}

	private void encounter_menu_try_again_on_consequence(MenuCallbackArgs args)
	{
		StartBattle();
	}

	private bool leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void leave_on_consequence(MenuCallbackArgs args)
	{
		CancelQuest(FailLogText);
	}

	private void StartBattle()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected I4, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		_specialBattleWon = false;
		if (Hero.MainHero.IsWounded)
		{
			Hero.MainHero.Heal(Hero.MainHero.WoundedHealthLimit - Hero.MainHero.HitPoints + 1, false);
		}
		PlayerEncounter.Finish(true);
		PlayerEncounter.Start();
		PlayerEncounter.Current.SetupFields(_specialBanditParty.Party, PartyBase.MainParty);
		PlayerEncounter.StartBattle();
		_merchantParty.MapEventSide = PlayerEncounter.Battle.GetMapEventSide(PlayerEncounter.Battle.PlayerSide);
		MissionInitializerRecord navalMissionInitializerTemplate = NavalStorylineData.GetNavalMissionInitializerTemplate("naval_storyline_act_3_quest_1");
		TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
		navalMissionInitializerTemplate.TerrainType = (int)faceTerrainType;
		navalMissionInitializerTemplate.NeedsRandomTerrain = false;
		navalMissionInitializerTemplate.PlayingInCampaignMode = true;
		navalMissionInitializerTemplate.RandomTerrainSeed = MBRandom.RandomInt(10000);
		navalMissionInitializerTemplate.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position);
		navalMissionInitializerTemplate.SceneHasMapPatch = false;
		navalMissionInitializerTemplate.AtmosphereOnCampaign.NauticalInfo.UsesNavalSimulatedWater = 1;
		PartyTemplateObject template = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_1_caravan_party_template");
		new MBList<Ship>(NavalDLCHelpers.GetSetPieceBattleShips(base.Template, PartyBase.MainParty));
		new MBList<Ship>(NavalDLCHelpers.GetSetPieceBattleShips(template, _merchantParty.Party));
		new MBList<Ship>((List<Ship>)(object)_specialBanditParty.Ships);
		NavalMissions.OpenHelpingAnAllySetPieceBattleMission(navalMissionInitializerTemplate, _merchantParty, _specialBanditParty);
		GameMenu.ActivateGameMenu("set_piece_retry_menu");
	}

	public bool AreEnemiesNearby()
	{
		if (_specialBanditParty != null && _specialBanditParty.IsActive)
		{
			return _specialBanditParty.IsVisible;
		}
		return false;
	}
}
