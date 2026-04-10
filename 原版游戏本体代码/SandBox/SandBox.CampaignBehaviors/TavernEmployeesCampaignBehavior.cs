using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace SandBox.CampaignBehaviors;

public class TavernEmployeesCampaignBehavior : CampaignBehaviorBase
{
	private enum TavernInquiryCompanionType
	{
		Scout,
		Engineer,
		Surgeon,
		Quartermaster,
		FirstMate,
		Navigator,
		CaravanLeader,
		Leader,
		Roguery
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConditionDelegate _003C_003E9__26_0;

		public static Func<Settlement, bool> _003C_003E9__36_0;

		internal bool _003CAddDialogs_003Eb__26_0()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			return (int)CharacterObject.OneToOneConversationCharacter.Occupation == 1;
		}

		internal bool _003Cconversation_talk_bard_on_condition_003Eb__36_0(Settlement x)
		{
			if (x.IsTown && x.Culture == Settlement.CurrentSettlement.Culture)
			{
				return x != Settlement.CurrentSettlement;
			}
			return false;
		}
	}

	public const int TavernCompanionInquiryCost = 2;

	private const int MinimumTavernCompanionInquirySkillLevel = 30;

	private const int BaseTunPrice = 50;

	private const int AskForClanInfoPrice = 500;

	private Settlement _orderedDrinkThisDayInSettlement;

	private bool _orderedDrinkThisVisit;

	private bool _hasMetWithRansomBroker;

	private bool _hasBoughtTunToParty;

	private Hero _inquiryCurrentCompanion;

	private TavernInquiryCompanionType _selectedCompanionType;

	private int _inquiryVariationIndex;

	private readonly Dictionary<TavernInquiryCompanionType, List<Hero>> _previouslyRecommendedCompanions = new Dictionary<TavernInquiryCompanionType, List<Hero>>();

	private float MaxTownDistanceAsDays => Campaign.Current.GetAverageDistanceBetweenClosestTwoTownsWithNavigationType((NavigationType)3) * 2f / (Campaign.Current.EstimatedAverageVillagerPartySpeed * (float)CampaignTime.HoursInDay);

	public override void RegisterEvents()
	{
		CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionStarted);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener((object)this, (Action)DailyTick);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
		CampaignEvents.WeeklyTickEvent.AddNonSerializedListener((object)this, (Action)WeeklyTick);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<Settlement>("_orderedDrinkThisDayInSettlement", ref _orderedDrinkThisDayInSettlement);
		dataStore.SyncData<bool>("_orderedDrinkThisVisit", ref _orderedDrinkThisVisit);
		dataStore.SyncData<bool>("_hasMetWithRansomBroker", ref _hasMetWithRansomBroker);
		dataStore.SyncData<bool>("_hasBoughtTunToParty", ref _hasBoughtTunToParty);
	}

	public void DailyTick()
	{
		_orderedDrinkThisDayInSettlement = null;
	}

	public void WeeklyTick()
	{
		_hasBoughtTunToParty = false;
	}

	public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
		_inquiryVariationIndex = MBRandom.NondeterministicRandomInt % 6;
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		if (!settlement.IsTown || CampaignMission.Current == null)
		{
			return;
		}
		Location location = CampaignMission.Current.Location;
		if (location != null && location.StringId == "tavern")
		{
			if (unusedUsablePointCount.TryGetValue("spawnpoint_tavernkeeper", out var value) && value > 0)
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTavernkeeper), settlement.Culture, (CharacterRelations)0, 1);
			}
			if (unusedUsablePointCount.TryGetValue("sp_tavern_wench", out value) && value > 0)
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTavernWench), settlement.Culture, (CharacterRelations)0, 1);
			}
			if (unusedUsablePointCount.TryGetValue("musician", out value) && value > 0)
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMusician), settlement.Culture, (CharacterRelations)0, value);
			}
			location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateRansomBroker), settlement.Culture, (CharacterRelations)0, 1);
		}
		else if (location != null && location.StringId == "center" && !Campaign.Current.IsNight)
		{
			if (unusedUsablePointCount.TryGetValue("spawnpoint_tavernkeeper", out var value2))
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTavernkeeper), settlement.Culture, (CharacterRelations)0, 1);
			}
			if (unusedUsablePointCount.TryGetValue("sp_tavern_wench", out value2))
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTavernWench), settlement.Culture, (CharacterRelations)0, 1);
			}
			if (unusedUsablePointCount.TryGetValue("musician", out value2))
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMusician), settlement.Culture, (CharacterRelations)0, value2);
			}
		}
	}

	public void OnMissionStarted(IMission mission)
	{
		_orderedDrinkThisVisit = false;
	}

	private static LocationCharacter CreateTavernWench(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		CharacterObject tavernWench = culture.TavernWench;
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(tavernWench, ref num, ref num2, "");
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)tavernWench).Race, "_settlement");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)tavernWench, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "sp_tavern_wench", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_barmaid"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false)
		{
			PrefabNamesForBones = { 
			{
				val.AgentMonster.OffHandItemBoneIndex,
				"kitchen_pitcher_b_tavern"
			} }
		};
	}

	private static LocationCharacter CreateTavernkeeper(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		CharacterObject tavernkeeper = culture.Tavernkeeper;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)tavernkeeper).Race, "_settlement");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(tavernkeeper, ref num, ref num2, "");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)tavernkeeper, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "spawnpoint_tavernkeeper", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_tavern_keeper"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateMusician(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		CharacterObject musician = culture.Musician;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)musician).Race, "_settlement");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(musician, ref num, ref num2, "");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)musician, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "musician", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_musician"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateRansomBroker(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		CharacterObject ransomBroker = culture.RansomBroker;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)ransomBroker).Race, "_settlement");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(ransomBroker, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)ransomBroker, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common", true, relation, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	protected void AddDialogs(CampaignGameStarter cgs)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_00ec: Expected O, but got Unknown
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_0190: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_01c7: Expected O, but got Unknown
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Expected O, but got Unknown
		//IL_01fe: Expected O, but got Unknown
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Expected O, but got Unknown
		//IL_0235: Expected O, but got Unknown
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Expected O, but got Unknown
		//IL_026c: Expected O, but got Unknown
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_02a3: Expected O, but got Unknown
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Expected O, but got Unknown
		//IL_02da: Expected O, but got Unknown
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Expected O, but got Unknown
		//IL_0331: Expected O, but got Unknown
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Expected O, but got Unknown
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Expected O, but got Unknown
		//IL_0393: Expected O, but got Unknown
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Expected O, but got Unknown
		//IL_03c9: Expected O, but got Unknown
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Expected O, but got Unknown
		//IL_03ff: Expected O, but got Unknown
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Expected O, but got Unknown
		//IL_0435: Expected O, but got Unknown
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Expected O, but got Unknown
		//IL_046b: Expected O, but got Unknown
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Expected O, but got Unknown
		//IL_04a1: Expected O, but got Unknown
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Expected O, but got Unknown
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Expected O, but got Unknown
		//IL_050f: Expected O, but got Unknown
		//IL_050f: Expected O, but got Unknown
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Expected O, but got Unknown
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Expected O, but got Unknown
		//IL_0585: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Expected O, but got Unknown
		//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Expected O, but got Unknown
		//IL_05fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Expected O, but got Unknown
		//IL_0626: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Expected O, but got Unknown
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_0660: Expected O, but got Unknown
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Expected O, but got Unknown
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b7: Expected O, but got Unknown
		//IL_06f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0703: Expected O, but got Unknown
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_072f: Unknown result type (might be due to invalid IL or missing references)
		//IL_073a: Expected O, but got Unknown
		//IL_073a: Expected O, but got Unknown
		//IL_0798: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a6: Expected O, but got Unknown
		//IL_07e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f2: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		object obj = _003C_003Ec._003C_003E9__26_0;
		if (obj == null)
		{
			OnConditionDelegate val = () => (int)CharacterObject.OneToOneConversationCharacter.Occupation == 1;
			_003C_003Ec._003C_003E9__26_0 = val;
			obj = (object)val;
		}
		cgs.AddDialogLine("talk_common_to_tavernkeeper", "start", "tavernkeeper_talk", "{=QCuxL92I}Good day, {?PLAYER.GENDER}madam{?}sir{\\?}. How can I help you?", (OnConditionDelegate)obj, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_talk_to_get_quest", "tavernkeeper_talk", "tavernkeeper_ask_quests", "{=A61ppTa6}Do you know of anyone who might have a task for someone like me?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_get_clan_info_start", "tavernkeeper_talk", "tavernkeeper_offer_clan_info", "{=shXdvd5p}I'm looking for information about the owner of this town.", new OnConditionDelegate(player_ask_information_about_the_owner_of_the_town_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_get_clan_info_answer", "tavernkeeper_offer_clan_info", "player_offer_clan_info", "{=i96KTeph}I can sell you information about {OWNER_CLAN}, who are the owners of our town {SETTLEMENT} for {PRICE}{GOLD_ICON}.", new OnConditionDelegate(tavernkeeper_offer_clan_info_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_get_clan_info_player_answer_1", "player_offer_clan_info", "tavernkeeper_pretalk", "{=VaxbQby7}That sounds like a great deal.", (OnConditionDelegate)null, new OnConsequenceDelegate(player_accepts_clan_info_offer_on_consequence), 100, new OnClickableConditionDelegate(player_accepts_clan_info_offer_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_get_clan_info_player_answer_2", "player_offer_clan_info", "tavernkeeper_pretalk", "{=CH7b5LaX}I have changed my mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_start", "tavernkeeper_talk", "tavernkeeper_companion_info_tavernkeeper_answer", "{=JPiId1fb}I am looking for some people to hire with specific skills. Would you happen to know anyone looking for work in the towns nearby? ", new OnConditionDelegate(tavernkeeper_talk_companion_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_companion_info_answer", "tavernkeeper_companion_info_tavernkeeper_answer", "tavernkeeper_list_companion_types", "{=ASVkuYHG}I know a few. What kind of a person are you looking for?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_player_select_scout", "tavernkeeper_list_companion_types", "player_selected_companion_type", "{=joCSXAQQ}I would travel much faster if I had a good scout by my side.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			FindCompanionWithType(TavernInquiryCompanionType.Scout);
		}, 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_player_select_engineer", "tavernkeeper_list_companion_types", "player_selected_companion_type", "{=NGcKLV88}A good engineer could help construct siege engines and new buildings in towns.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			FindCompanionWithType(TavernInquiryCompanionType.Engineer);
		}, 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_player_select_surgeon", "tavernkeeper_list_companion_types", "player_selected_companion_type", "{=Y5ztM8zq}My men would feel safer with a good surgeon to aid them in their time of need.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			FindCompanionWithType(TavernInquiryCompanionType.Surgeon);
		}, 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_player_select_quartermaster", "tavernkeeper_list_companion_types", "player_selected_companion_type", "{=88Fk9keT}I am sure I can do more with fewer supplies if I had a good quartermaster.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			FindCompanionWithType(TavernInquiryCompanionType.Quartermaster);
		}, 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_player_select_caravan_leader", "tavernkeeper_list_companion_types", "player_selected_companion_type", "{=kePH44eg}I am planning to sponsor my own caravans, and someone who could run them would be perfect.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			FindCompanionWithType(TavernInquiryCompanionType.CaravanLeader);
		}, 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_player_select_leader", "tavernkeeper_list_companion_types", "player_selected_companion_type", "{=9z0Yz8za}I need a lieutenant to be my right hand and help me direct my troops in battle.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			FindCompanionWithType(TavernInquiryCompanionType.Leader);
		}, 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_player_select_roguery", "tavernkeeper_list_companion_types", "player_selected_companion_type", "{=DMUsPekF}I need someone who knows a bit about the darker side of the world, who can handle villains and thieves.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			FindCompanionWithType(TavernInquiryCompanionType.Roguery);
		}, 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_player_nevermind", "tavernkeeper_list_companion_types", "tavernkeeper_pretalk", "{=tdvnKIyS}Never mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_companion_info_tavernkeeper_not_found_answer_1", "player_selected_companion_type", "tavernkeeper_talk_companion_end", "{=!}{CANNOT_THINK_OF_ANYONE_LINE}[rb:negative]", (OnConditionDelegate)(() => !FoundCompanion()), new OnConsequenceDelegate(IncreaseVariationIndex), 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_companion_info_tavernkeeper_end", "tavernkeeper_talk_companion_end", "start", "{=PDf52VCf}Thank you for your time anyway.", (OnConditionDelegate)null, new OnConsequenceDelegate(IncreaseVariationIndex), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_companion_info_tavernkeeper_found_answer_companion_is_inside_1", "player_selected_companion_type", "player_companion_response", "{=QdEGu0CW}A {?INQUIRY_COMPANION.GENDER}woman{?}man{\\?} called {INQUIRY_COMPANION.LINK} has passed through here on his way to {COMPANION_SETTLEMENT}. You may catch up to {?INQUIRY_COMPANION.GENDER}her{?}him{\\?} if you hurry.[rb:positive]", (OnConditionDelegate)(() => _inquiryCurrentCompanion.CurrentSettlement != Settlement.CurrentSettlement && _inquiryVariationIndex % 3 == 0), new OnConsequenceDelegate(IncreaseVariationIndex), 100, (OnClickableConditionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_companion_info_tavernkeeper_found_answer_companion_is_inside_2", "player_selected_companion_type", "player_companion_response", "{=WSAS3XLC}There was someone here not long ago, went by the name of {INQUIRY_COMPANION.LINK}. {?INQUIRY_COMPANION.GENDER}She{?}He{\\?} left for {COMPANION_SETTLEMENT}. Perhaps you can find them there, or on the road.[rb:positive]", (OnConditionDelegate)(() => _inquiryCurrentCompanion.CurrentSettlement != Settlement.CurrentSettlement && _inquiryVariationIndex % 3 == 1), new OnConsequenceDelegate(IncreaseVariationIndex), 100, (OnClickableConditionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_companion_info_tavernkeeper_found_answer_companion_is_inside_3", "player_selected_companion_type", "player_companion_response", "{=ahydRFKe}There was someone who called {?INQUIRY_COMPANION.GENDER}herself{?}himself{\\?} {INQUIRY_COMPANION.LINK}. Sounds like the kind of person who might interest you, no? {?INQUIRY_COMPANION.GENDER}She{?}He{\\?} was headed for {COMPANION_SETTLEMENT}.[rb:positive]", (OnConditionDelegate)(() => _inquiryCurrentCompanion.CurrentSettlement != Settlement.CurrentSettlement && _inquiryVariationIndex % 3 == 2), new OnConsequenceDelegate(IncreaseVariationIndex), 100, (OnClickableConditionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_companion_info_tavernkeeper_found_answer_companion_is_outside_1", "player_selected_companion_type", "player_companion_response", "{=gfmU4wiM}A {?INQUIRY_COMPANION.GENDER}woman{?}man{\\?} named {INQUIRY_COMPANION.LINK} is staying at my tavern. You may want to talk with him.", (OnConditionDelegate)(() => _inquiryVariationIndex % 3 == 0), new OnConsequenceDelegate(IncreaseVariationIndex), 100, (OnClickableConditionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_companion_info_tavernkeeper_found_answer_companion_is_outside_2", "player_selected_companion_type", "player_companion_response", "{=qSy4Ns1N}There's someone who might meet your needs who is in here having a drink right now. Goes by the name of {INQUIRY_COMPANION.LINK}.", (OnConditionDelegate)(() => _inquiryVariationIndex % 3 == 1), new OnConsequenceDelegate(IncreaseVariationIndex), 100, (OnClickableConditionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_companion_info_tavernkeeper_found_answer_companion_is_outside_3", "player_selected_companion_type", "player_companion_response", "{=rLcRwkqK}You might want to look around here for a {?INQUIRY_COMPANION.GENDER}lady{?}fellow{\\?} named {INQUIRY_COMPANION.LINK}. I believe {?INQUIRY_COMPANION.GENDER}she{?}he{\\?} might possess that kind of skill.", (OnConditionDelegate)(() => _inquiryVariationIndex % 3 == 2), new OnConsequenceDelegate(IncreaseVariationIndex), 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("player_companion_response_to_tavernkeeper", "player_companion_response", "tavernkeeper_companion_info_tavernkeeper_answer", "{=SqrRaIU5}I would like to ask about someone with different expertise.", new OnConditionDelegate(FoundCompanion), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("player_companion_response_to_tavernkeeper_2", "player_companion_response", "player_selected_companion_type", "{=vx4ML2gX}Is there someone else other than {INQUIRY_COMPANION.LINK}?", new OnConditionDelegate(FoundCompanion), (OnConsequenceDelegate)delegate
		{
			FindCompanionWithType(_selectedCompanionType);
		}, 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("player_companion_response_to_tavernkeeper_found", "player_companion_response", "start", "{=3FzBTb3w}Thank you for this information. It was {COMPANION_INQUIRY_COST}{GOLD_ICON} well spent.", new OnConditionDelegate(FoundCompanion), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("player_companion_response_to_tavernkeeper_not_found", "player_companion_response", "start", "{=PDf52VCf}Thank you for your time anyway.", (OnConditionDelegate)(() => !FoundCompanion()), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_talk_to_leave", "tavernkeeper_talk", "close_window", "{=fF2BdOy9}I don't need anything now.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			_previouslyRecommendedCompanions.Clear();
		}, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("1972", "tavernkeeper_pretalk", "tavernkeeper_talk", "{=ds294zxi}Anything else?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddDialogLine("talk_common_to_tavernmaid", "start", "tavernmaid_talk", "{=ddYWbO8b}The usual?", new OnConditionDelegate(conversation_tavernmaid_offers_usual_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddDialogLine("talk_common_to_tavernmaid_2", "start", "tavernmaid_talk", "{=x7k87vj3}What can I bring you, {?PLAYER.GENDER}madam{?}sir{\\?}? Would you like to taste our local speciality, {DRINK} with {FOOD}?", new OnConditionDelegate(conversation_tavernmaid_offers_food_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddDialogLine("talk_common_to_tavernmaid_3", "start", "tavernmaid_talk", "{=Tn9g83ry}Enjoying your drink, {?PLAYER.GENDER}madam{?}sir{\\?}?", new OnConditionDelegate(conversation_tavernmaid_gossips_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("tavernmaid_order_food", "tavernmaid_talk", "tavernmaid_order_acknowledge", "{=E57VFXqU}I'll have that.", new OnConditionDelegate(conversation_player_can_order_food_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("tavernmain_order_acknowledge", "tavernmaid_order_acknowledge", "close_window", "{=3wb2dCfz}It'll be right up then, {?PLAYER.GENDER}ma'am{?}sir{\\?}.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_tavernmaid_delivers_food_on_consequence), 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("tavernmaid_ask_tun", "tavernmaid_talk", "tavern_drink_morale_to_party", "{=oAaKaXEy}I really like this meal. I'd like it served to all my men.", new OnConditionDelegate(conversation_tavernmaid_buy_tun_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernmaid_leave", "tavernmaid_talk", "close_window", "{=Piq3oYmG}I'm fine, thank you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("tavernmaid_give_tun_gold", "tavern_drink_morale_to_party", "tun_give_gold", "{=bjNuUqTx}With pleasure, {?PLAYER.GENDER}madam{?}sir{\\?}. That will cost you {COST}{GOLD_ICON}.", new OnConditionDelegate(calculate_tun_cost_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("tavernmaid_give_tun_gold_2", "tun_give_gold", "tavernmaid_enjoy", "{=nAM821Fb}Here you are.", (OnConditionDelegate)null, new OnConsequenceDelegate(can_buy_tun_on_consequence), 100, new OnClickableConditionDelegate(can_buy_tun_on_clickable_condition), (OnPersuasionOptionDelegate)null);
		cgs.AddPlayerLine("tavernmaid_not_give_tun_gold", "tun_give_gold", "start", "{=2PEKd3Sz}Actually, I changed my mind. Cancel the order...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("tavernmaid_best_wishes", "tavernmaid_enjoy", "close_window", "{=ZGMfmNe0}Very generous of you, my {?PLAYER.GENDER}lady{?}lord{\\?}. Good health and fortune to all of you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddDialogLine("talk_bard", "start", "talk_bard_player", "{=!}{LYRIC_SCRAP}", new OnConditionDelegate(conversation_talk_bard_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("talk_bard_player_leave", "talk_bard_player", "close_window", "{=sbczu2VI}Play on, good man.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_quest_info", "tavernkeeper_ask_quests", "tavernkeeper_has_quest", "{=uUkCLZEo}Let's see... {ISSUE_GIVER_LIST}.", new OnConditionDelegate(tavenkeeper_has_quest_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		cgs.AddPlayerLine("tavernkeeper_player_thanks", "tavernkeeper_has_quest", "tavernkeeper_pretalk", "{=eALf5d30}Thanks!", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		cgs.AddDialogLine("tavernkeeper_turndown", "tavernkeeper_doesnot_have_quests", "tavernkeeper_talk", "{=py6Y46sa}No, I didn't hear any...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		AddRansomBrokerDialogs(cgs);
	}

	private bool player_ask_information_about_the_owner_of_the_town_condition()
	{
		return Settlement.CurrentSettlement.OwnerClan != Clan.PlayerClan;
	}

	private void player_accepts_clan_info_offer_on_consequence()
	{
		foreach (Hero item in (List<Hero>)(object)Settlement.CurrentSettlement.OwnerClan.Heroes)
		{
			item.IsKnownToPlayer = true;
		}
		GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, 500, false);
	}

	private bool player_accepts_clan_info_offer_clickable_condition(out TextObject explanation)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		bool flag = true;
		foreach (Hero item in (List<Hero>)(object)Settlement.CurrentSettlement.OwnerClan.Heroes)
		{
			if (!item.IsKnownToPlayer)
			{
				flag = false;
			}
		}
		if (flag)
		{
			explanation = new TextObject("{=LBiZ9Rie}You already possess this information.", (Dictionary<string, object>)null);
			return false;
		}
		explanation = new TextObject("{=!}{INFORMATION_COST}{GOLD_ICON}.", (Dictionary<string, object>)null);
		MBTextManager.SetTextVariable("INFORMATION_COST", 500);
		if (Hero.MainHero.Gold < 500)
		{
			explanation = new TextObject("{=xVZVYNan}You don't have enough{GOLD_ICON}.", (Dictionary<string, object>)null);
			return false;
		}
		return true;
	}

	private bool tavernkeeper_offer_clan_info_on_condition()
	{
		MBTextManager.SetTextVariable("OWNER_CLAN", Settlement.CurrentSettlement.OwnerClan.EncyclopediaLinkWithName, false);
		MBTextManager.SetTextVariable("SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName, false);
		MBTextManager.SetTextVariable("PRICE", 500);
		return true;
	}

	private void SetCannotThinkOfAnyoneLine(TavernInquiryCompanionType type)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		TextObject val = ((!_previouslyRecommendedCompanions.ContainsKey(type)) ? ((_inquiryVariationIndex % 2 == 0) ? new TextObject("{=BYpoxUEB}I haven't heard of someone with such skills looking for work for a while now.", (Dictionary<string, object>)null) : new TextObject("{=SbYlYGFA}Sorry. No one like that has passed through here for a while.", (Dictionary<string, object>)null)) : new TextObject("{=eeIX6loR}I can't think of anyone else.", (Dictionary<string, object>)null));
		MBTextManager.SetTextVariable("CANNOT_THINK_OF_ANYONE_LINE", val, false);
	}

	private void IncreaseVariationIndex()
	{
		_inquiryVariationIndex++;
	}

	private bool FoundCompanion()
	{
		return _inquiryCurrentCompanion != null;
	}

	public void FindCompanionWithType(PartyRole role)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)role == 14)
		{
			FindCompanionWithType(TavernInquiryCompanionType.FirstMate);
		}
		else if ((int)role == 15)
		{
			FindCompanionWithType(TavernInquiryCompanionType.Navigator);
		}
		else
		{
			Debug.FailedAssert("Wrong function call! This only should be called for First Mate and Navigator.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\CampaignBehaviors\\TavernEmployeesCampaignBehavior.cs", "FindCompanionWithType", 353);
		}
	}

	private void FindCompanionWithType(TavernInquiryCompanionType companionType)
	{
		int num = 30;
		Hero inquiryCurrentCompanion = null;
		float num2 = MaxTownDistanceAsDays * Campaign.Current.EstimatedAverageVillagerPartySpeed * (float)CampaignTime.HoursInDay;
		foreach (Town item in (List<Town>)(object)Town.AllTowns)
		{
			if (((List<Hero>)(object)((SettlementComponent)item).Settlement.HeroesWithoutParty).Count <= 0 || !(Campaign.Current.Models.MapDistanceModel.GetDistance(((SettlementComponent)item).Settlement, Settlement.CurrentSettlement, false, false, (NavigationType)((!Settlement.CurrentSettlement.HasPort || !((SettlementComponent)item).Settlement.HasPort) ? 1 : 3)) < num2))
			{
				continue;
			}
			foreach (Hero item2 in (List<Hero>)(object)((SettlementComponent)item).Settlement.HeroesWithoutParty)
			{
				int num3 = 0;
				_previouslyRecommendedCompanions.TryGetValue(companionType, out var value);
				if (item2.IsWanderer && (value == null || !value.Contains(item2)))
				{
					switch (companionType)
					{
					case TavernInquiryCompanionType.Scout:
						num3 = item2.GetSkillValue(Campaign.Current.Models.ClanMemberPartyRoleModel.GetRelevantSkillForPartyRole((PartyRole)9));
						break;
					case TavernInquiryCompanionType.Engineer:
						num3 = item2.GetSkillValue(Campaign.Current.Models.ClanMemberPartyRoleModel.GetRelevantSkillForPartyRole((PartyRole)8));
						break;
					case TavernInquiryCompanionType.Surgeon:
						num3 = item2.GetSkillValue(Campaign.Current.Models.ClanMemberPartyRoleModel.GetRelevantSkillForPartyRole((PartyRole)7));
						break;
					case TavernInquiryCompanionType.Quartermaster:
						num3 = item2.GetSkillValue(Campaign.Current.Models.ClanMemberPartyRoleModel.GetRelevantSkillForPartyRole((PartyRole)10));
						break;
					case TavernInquiryCompanionType.FirstMate:
						num3 = item2.GetSkillValue(Campaign.Current.Models.ClanMemberPartyRoleModel.GetRelevantSkillForPartyRole((PartyRole)14));
						break;
					case TavernInquiryCompanionType.Navigator:
						num3 = item2.GetSkillValue(Campaign.Current.Models.ClanMemberPartyRoleModel.GetRelevantSkillForPartyRole((PartyRole)15));
						break;
					case TavernInquiryCompanionType.CaravanLeader:
						num3 += item2.GetSkillValue(DefaultSkills.Trade);
						break;
					case TavernInquiryCompanionType.Leader:
						num3 = item2.GetSkillValue(DefaultSkills.Leadership);
						num3 += item2.GetSkillValue(DefaultSkills.Tactics);
						break;
					case TavernInquiryCompanionType.Roguery:
						num3 = item2.GetSkillValue(DefaultSkills.Roguery);
						break;
					}
				}
				if (num3 > num)
				{
					num = num3;
					inquiryCurrentCompanion = item2;
				}
			}
		}
		_inquiryCurrentCompanion = inquiryCurrentCompanion;
		_selectedCompanionType = companionType;
		GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, 2, false);
		if (_inquiryCurrentCompanion != null)
		{
			StringHelpers.SetCharacterProperties("INQUIRY_COMPANION", _inquiryCurrentCompanion.CharacterObject, (TextObject)null, false);
			MBTextManager.SetTextVariable("COMPANION_SETTLEMENT", _inquiryCurrentCompanion.CurrentSettlement.EncyclopediaLinkWithName, false);
			_inquiryCurrentCompanion.IsKnownToPlayer = true;
			if (_previouslyRecommendedCompanions.ContainsKey(_selectedCompanionType))
			{
				_previouslyRecommendedCompanions[_selectedCompanionType].Add(_inquiryCurrentCompanion);
			}
			else
			{
				_previouslyRecommendedCompanions.Add(_selectedCompanionType, new List<Hero> { _inquiryCurrentCompanion });
			}
		}
		SetCannotThinkOfAnyoneLine(companionType);
	}

	private bool conversation_talk_bard_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 26)
		{
			List<string> list = new List<string>();
			Settlement randomElementWithPredicate = Extensions.GetRandomElementWithPredicate<Settlement>(Settlement.All, (Func<Settlement, bool>)((Settlement x) => x.IsTown && x.Culture == Settlement.CurrentSettlement.Culture && x != Settlement.CurrentSettlement));
			MBTextManager.SetTextVariable("RANDOM_TOWN", randomElementWithPredicate.Name, false);
			list.Add("{=3n1KRLpZ}'My love is far {newline} I know not where {newline} Perhaps the winds shall tell me'");
			list.Add("{=NQOQb0C9}'And many thousand bodies lay a-rotting in the sun {newline} But things like that must be you know for kingdoms to be won'");
			list.Add("{=bs8ayCGX}'A warrior brave you might surely be {newline} With your blade and your helm and your bold fiery steed {newline} But I'll give you a warning you'd be wise to heed {newline} Don't toy with the fishwives of {RANDOM_TOWN}'");
			list.Add("{=3n1KRLpZ}'My love is far {newline} I know not where {newline} Perhaps the winds shall tell me'");
			list.Add("{=YequZz6U}'Oh the maidens of {RANDOM_TOWN} are merry and fair {newline} Plotting their mischief with flowers in their hair {newline} Were I still a young man I sure would be there {newline} But now I'll take warmth over trouble'");
			list.Add("{=CM8Tr3lL}'Oh my pocket's been picked {newline} And my shirt's drenched with sick {newline} And my head feels like it's come a fit to bursting'");
			list.Add("{=DFkzQHRQ}'For all the silks of the Padishah {newline} For all the Emperor's gold {newline} For all the spice in the distance East...'");
			list.Add("{=2fbLBXtT}'O'er the whale-road she sped {newline} She were manned by the dead  {newline} And the clouds followed black in her wake'");
			string text = list[MBRandom.RandomInt(0, list.Count)];
			MBTextManager.SetTextVariable("LYRIC_SCRAP", new TextObject(text, (Dictionary<string, object>)null), false);
			return true;
		}
		return false;
	}

	private static bool companion_type_select_clickable_condition(out TextObject explanation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		explanation = new TextObject("{=!}{COMPANION_INQUIRY_COST}{GOLD_ICON}.", (Dictionary<string, object>)null);
		MBTextManager.SetTextVariable("COMPANION_INQUIRY_COST", 2);
		if (Hero.MainHero.Gold < 2)
		{
			explanation = new TextObject("{=xVZVYNan}You don't have enough{GOLD_ICON}.", (Dictionary<string, object>)null);
			return false;
		}
		return true;
	}

	private void AddRansomBrokerDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected O, but got Unknown
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("ransom_broker_start", "start", "ransom_broker_intro", "{=!}{RANSOM_BROKER_INTRO}", new OnConditionDelegate(conversation_ransom_broker_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("ransom_broker_intro", "ransom_broker_intro", "ransom_broker_2", "{=TGYJUUn0}Go on.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_intro_2_di", "ransom_broker_2", "ransom_broker_3", "{=MFDb5duu}Splendid! I suspect that you may, in your line of work, occasionally acquire a few captives. I could possibly take them off your hands. I'd pay you, of course.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("ransom_broker_intro_2", "ransom_broker_3", "ransom_broker_4", "{=bPqwLopK}Mm. Are you a slaver?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_intro_3", "ransom_broker_4", "ransom_broker_5", "{=YCC0hPuC}Ah, no. Slavers are rare these days. It used to be that, when the Empire and its neighbors made war upon each other, the defeated were usually taken as slaves. That helped pay for the war, you see! But today, for better or for worse, that is not practical. What with the frontiers broken and raiders crossing this way and that, there are far too many opportunities for captives to escape. But that does not mean that war cannot be profitable! Indeed it still can!", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_intro_4", "ransom_broker_5", "ransom_broker_6", "{=8bfzW0np}Many captives are still taken, and their families will pay to have them back. Men such as I criss-cross the Empire and the outer kingdoms, acquiring prisoners, and contacting their kin for a suitable ransom.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_intro_5", "ransom_broker_6", "ransom_broker_info_talk", "{=rLlIVqmY}So, were you to acquire a few prisoners in one of your victorious affrays, and wish to relieve yourself of their care and feeding, I or one of my colleagues would be happy to pay for them as a sort of speculative investment.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("ransom_broker_info_talk_player_1", "ransom_broker_info_talk", "ransom_broker_families", "{=QHoCsSZX}What if their families can't pay?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("ransom_broker_info_talk_player_2", "ransom_broker_info_talk", "ransom_broker_prices", "{=btA10FML}What can I get for a prisoner?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("ransom_broker_info_talk_player_3", "ransom_broker_info_talk", "ransom_broker_ransom_me", "{=nwJPPIvn}Would you be able to ransom me if I were taken?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("ransom_broker_info_talk_player_4", "ransom_broker_info_talk", "ransom_broker_pretalk", "{=TyultuzK}That's all I need to know. Thank you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_families", "ransom_broker_families", "ransom_broker_info_talk", "{=zxonBgY2}Oh, I suppose I could sell them to the republics of Geroia, to row their galleys, although even in Geroia they prefer free oarsmen these days... But it rarely comes to that. You'd be surprised what sorts of treasures a peasant can dig out of his cowshed or wheedle out of his cousins!", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_prices", "ransom_broker_prices", "ransom_broker_info_talk", "{=PLbxHyPu}It varies. I fancy that I have a fine eye for assessing a ransom. There are a dozen little things about a man that will tell you whether he goes to bed hungry, or spices his meat with pepper and cloves from the east. The real money of course is in the aristocracy, and if you ever want to do my job you'll want to learn about every landowning family or tribal chief in Calradia, their estates, their offspring both lawful and bastard, and, of course, their credit with the merchants.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_ransom_me", "ransom_broker_ransom_me", "ransom_broker_info_talk", "{=4tY23HWb}Of course. I'm welcome in every court in Calradia. There's not many who can say that! So always be sure to keep a pot of denars buried somewhere, and a loyal servant who can find it in a hurry.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_start_has_met", "start", "ransom_broker_talk", "{=w4yxgY3F}Greetings. If you have any prisoners, I will be happy to buy them from you.", new OnConditionDelegate(conversation_ransom_broker_start_has_met_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_pretalk", "ransom_broker_pretalk", "ransom_broker_talk", "{=AQi1arUp}Anyway, if you have any prisoners, I will be happy to buy them from you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("ransom_broker_talk_player_1", "ransom_broker_talk", "ransom_broker_sell_prisoners", "{=cAVxYAdw}Then you'd better bring your purse. I have got prisoners to sell.", new OnConditionDelegate(conversation_ransom_broker_open_party_screen_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("ransom_broker_talk_player_2", "ransom_broker_talk", "ransom_broker_2", "{=Yac7bSU3}Tell me about what you do again.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("ransom_broker_talk_player_4", "ransom_broker_talk", "ransom_broker_no_prisoners", "{=CQMkh88h}I don't have any prisoners to sell, but that's good to know.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_no_prisoners", "ransom_broker_no_prisoners", "close_window", "{=mEsaiLOR}Very well then. If you happen to have any more prisoners, you know where to find me.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_sell_prisoners", "ransom_broker_sell_prisoners", "ransom_broker_sell_prisoners_3", "{=xFmYRCHs}Let me see what you have...", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_ransom_broker_sell_prisoners_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_sell_prisoners_3", "ransom_broker_sell_prisoners_3", "ransom_broker_pretalk", "{=3BvfOe1y}Very well then. You catch some more and you want me to take them off of your hands, you know where to find me...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("ransom_broker_sell_prisoners_2", "ransom_broker_sell_prisoners_2", "close_window", "{=fQaPv0Xl}I will be staying here for a few days. Let me know if you need my services.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
	}

	private bool conversation_tavernmaid_offers_usual_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 13 && !_orderedDrinkThisVisit)
		{
			return _orderedDrinkThisDayInSettlement == Settlement.CurrentSettlement;
		}
		return false;
	}

	private bool conversation_tavernmaid_offers_food_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Expected O, but got Unknown
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Expected O, but got Unknown
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Expected O, but got Unknown
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Expected O, but got Unknown
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Expected O, but got Unknown
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Expected O, but got Unknown
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 13 && _orderedDrinkThisDayInSettlement != Settlement.CurrentSettlement)
		{
			CampaignVec2 position;
			if (((MBObjectBase)MobileParty.MainParty.CurrentSettlement.Culture).StringId == "vlandia")
			{
				MBTextManager.SetTextVariable("DRINK", new TextObject("{=07qBenIW}a flagon of ale", (Dictionary<string, object>)null), false);
				MBTextManager.SetTextVariable("FOOD", new TextObject("{=uJceH1Dv}a Sunor sausage", (Dictionary<string, object>)null), false);
				position = MobileParty.MainParty.CurrentSettlement.Position;
				if (((CampaignVec2)(ref position)).Y < 150f)
				{
					MBTextManager.SetTextVariable("FOOD", new TextObject("{=07QlFlXK}a plate of herrings", (Dictionary<string, object>)null), false);
				}
			}
			if (((MBObjectBase)MobileParty.MainParty.CurrentSettlement.Culture).StringId == "empire")
			{
				MBTextManager.SetTextVariable("DRINK", new TextObject("{=ybXgaKEv}a glass of local wine", (Dictionary<string, object>)null), false);
				MBTextManager.SetTextVariable("FOOD", new TextObject("{=IBhZGxxm}a plate of olives", (Dictionary<string, object>)null), false);
				position = MobileParty.MainParty.CurrentSettlement.Position;
				if (((CampaignVec2)(ref position)).X < 300f)
				{
					MBTextManager.SetTextVariable("FOOD", new TextObject("{=d18Ul2Zl}a plate of sardines", (Dictionary<string, object>)null), false);
				}
			}
			if (((MBObjectBase)MobileParty.MainParty.CurrentSettlement.Culture).StringId == "khuzait")
			{
				MBTextManager.SetTextVariable("DRINK", new TextObject("{=WZbrxhYm}a flask of kefir", (Dictionary<string, object>)null), false);
				MBTextManager.SetTextVariable("FOOD", new TextObject("{=0qc11kmz}a plate of mutton dumplings", (Dictionary<string, object>)null), false);
			}
			if (((MBObjectBase)MobileParty.MainParty.CurrentSettlement.Culture).StringId == "aserai")
			{
				MBTextManager.SetTextVariable("DRINK", new TextObject("{=AULqrp7D}a glass of Calradian wine", (Dictionary<string, object>)null), false);
				MBTextManager.SetTextVariable("FOOD", new TextObject("{=GhPGpR90}a plate of dates", (Dictionary<string, object>)null), false);
			}
			if (((MBObjectBase)MobileParty.MainParty.CurrentSettlement.Culture).StringId == "sturgia")
			{
				MBTextManager.SetTextVariable("DRINK", new TextObject("{=bZbFrIUr}a mug of kvass", (Dictionary<string, object>)null), false);
				MBTextManager.SetTextVariable("FOOD", new TextObject("{=LPBVTiV6}a strip of bacon", (Dictionary<string, object>)null), false);
			}
			if (((MBObjectBase)MobileParty.MainParty.CurrentSettlement.Culture).StringId == "battania")
			{
				MBTextManager.SetTextVariable("DRINK", new TextObject("{=vEHaOSIT}a mug of sour beer", (Dictionary<string, object>)null), false);
				MBTextManager.SetTextVariable("FOOD", new TextObject("{=z4arML8E}a strip of dried venison", (Dictionary<string, object>)null), false);
			}
			if (((MBObjectBase)MobileParty.MainParty.CurrentSettlement.Culture).StringId == "nord")
			{
				MBTextManager.SetTextVariable("DRINK", new TextObject("{=z47FuQ3f}a horn of barley beer", (Dictionary<string, object>)null), false);
				MBTextManager.SetTextVariable("FOOD", new TextObject("{=gcB54MUl}some buttered hardfish", (Dictionary<string, object>)null), false);
			}
			return true;
		}
		return false;
	}

	private void conversation_tavernmaid_delivers_food_on_consequence()
	{
		_orderedDrinkThisDayInSettlement = Settlement.CurrentSettlement;
		_orderedDrinkThisVisit = true;
	}

	private bool conversation_tavernmaid_gossips_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 13)
		{
			return _orderedDrinkThisDayInSettlement == Settlement.CurrentSettlement;
		}
		return false;
	}

	private bool conversation_player_can_order_food_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 13)
		{
			return !_orderedDrinkThisVisit;
		}
		return false;
	}

	private static bool tavenkeeper_has_quest_on_condition()
	{
		List<IssueBase> list = IssueManager.GetIssuesInSettlement(Hero.MainHero.CurrentSettlement, true).ToList();
		if (list.Count > 0)
		{
			Extensions.Shuffle<IssueBase>((IList<IssueBase>)list);
			if (list.Count == 1)
			{
				MBTextManager.SetTextVariable("ISSUE_GIVER_LIST", "{=roTCX8S8}{ISSUE_GIVER_1.LINK} mentioned something about needing someone to do some work.", false);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER_1", list[0].IssueOwner.CharacterObject, (TextObject)null, false);
			}
			else if (list.Count == 2)
			{
				MBTextManager.SetTextVariable("ISSUE_GIVER_LIST", "{=79XElnsg}{ISSUE_GIVER_1.LINK} mentioned something about needing someone to do some work. And I think {ISSUE_GIVER_2.LINK} was looking for someone.", false);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER_1", list[0].IssueOwner.CharacterObject, (TextObject)null, false);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER_2", list[1].IssueOwner.CharacterObject, (TextObject)null, false);
			}
			else
			{
				MBTextManager.SetTextVariable("ISSUE_GIVER_LIST", "{=SIxE2LGn}{ISSUE_GIVER_1.LINK} mentioned something about needing someone to do some work. And I think {ISSUE_GIVER_2.LINK} and {ISSUE_GIVER_3.LINK} were looking for someone.", false);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER_1", list[0].IssueOwner.CharacterObject, (TextObject)null, false);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER_2", list[1].IssueOwner.CharacterObject, (TextObject)null, false);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER_3", list[2].IssueOwner.CharacterObject, (TextObject)null, false);
			}
			return true;
		}
		MBTextManager.SetTextVariable("ISSUE_GIVER_LIST", "{=RlP8aYVJ}Nobody is looking for help right now.", false);
		return true;
	}

	private bool tavernkeeper_talk_companion_on_condition()
	{
		_inquiryCurrentCompanion = null;
		return true;
	}

	private bool conversation_tavernmaid_buy_tun_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 13 && _orderedDrinkThisDayInSettlement == Settlement.CurrentSettlement && !_hasBoughtTunToParty)
		{
			return PartyBase.MainParty.MemberRoster.Count > 1;
		}
		return false;
	}

	private bool can_buy_tun_on_clickable_condition(out TextObject explanation)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (Hero.MainHero.Gold < get_tun_price())
		{
			explanation = new TextObject("{=xVZVYNan}You don't have enough{GOLD_ICON}.", (Dictionary<string, object>)null);
			return false;
		}
		explanation = null;
		return true;
	}

	private void can_buy_tun_on_consequence()
	{
		int tun_price = get_tun_price();
		GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, tun_price, false);
		MobileParty mainParty = MobileParty.MainParty;
		mainParty.RecentEventsMorale += 2f;
		_hasBoughtTunToParty = true;
	}

	private static bool calculate_tun_cost_on_condition()
	{
		int tun_price = get_tun_price();
		MBTextManager.SetTextVariable("COST", tun_price);
		return true;
	}

	private static int get_tun_price()
	{
		return (int)(50f + (float)MobileParty.MainParty.MemberRoster.TotalHealthyCount * 0.2f);
	}

	private bool conversation_ransom_broker_start_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 9 && !_hasMetWithRansomBroker)
		{
			_hasMetWithRansomBroker = true;
			MBTextManager.SetTextVariable("RANSOM_BROKER_INTRO", "{=Y7tozytM}Hello, {?PLAYER.GENDER}madam{?}sir{\\?}. You have the bearing of a warrior. Do you have a minute? We may have interests in common.", false);
			if (Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan || Settlement.CurrentSettlement.MapFaction.Leader == Hero.MainHero)
			{
				MBTextManager.SetTextVariable("RANSOM_BROKER_INTRO", "{=6zxo4AU9}This is quite the honor, your {?PLAYER.GENDER}ladyship{?}lordship{\\?}. Do you have a minute? I may be able to do you a service.", false);
			}
			return true;
		}
		return false;
	}

	private bool conversation_ransom_broker_open_party_screen_on_condition()
	{
		return MobileParty.MainParty.Party.NumberOfPrisoners > 0;
	}

	private bool conversation_ransom_broker_start_has_met_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 9)
		{
			return _hasMetWithRansomBroker;
		}
		return false;
	}

	private void conversation_ransom_broker_sell_prisoners_on_consequence()
	{
		PartyScreenHelper.OpenScreenAsRansom();
	}
}
