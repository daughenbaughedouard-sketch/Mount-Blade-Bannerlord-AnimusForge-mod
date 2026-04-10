using System;
using System.Collections.Generic;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;

namespace NavalDLC.CampaignBehaviors;

public class PortCharactersCampaignBehavior : CampaignBehaviorBase
{
	private const float PortTownsmanCarryingStuffSpawnPercentage = 0.6f;

	private const float PortTownsmanSpawnPercentageMale = 0.2f;

	private const float PortTownsmanSpawnPercentageFemale = 0.1f;

	private const float ShipyardWorkerSpawnPercentage = 1f;

	private const float MarketWorkerSpawnPercentage = 0.75f;

	private const float CarpenterSpawnPercentage = 0.35f;

	private static List<(string, bool)> _itemToCarryAndIsMainHandData = new List<(string, bool)>
	{
		("wood_load", true),
		("bucket_filled", false),
		("carry_fish_stick", false)
	};

	public override void RegisterEvents()
	{
		CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnAfterSessionLaunched);
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnAfterSessionLaunched(CampaignGameStarter campaignGameSystemStarter)
	{
		AddDialogs(campaignGameSystemStarter);
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Expected O, but got Unknown
		Settlement currentSettlement = Settlement.CurrentSettlement;
		Location val = ((currentSettlement != null) ? currentSettlement.LocationComplex.GetLocationWithId("port") : null);
		if (val != null && !NavalStorylineData.IsNavalStoryLineActive())
		{
			if (unusedUsablePointCount.TryGetValue("sp_shipwright", out var value))
			{
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateShipWright), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, 1);
			}
			if (unusedUsablePointCount.TryGetValue("merchant_carpenter", out value))
			{
				int num = 1 + (int)((float)value * 0.35f);
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreatePortMerchant), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, num);
			}
			if (unusedUsablePointCount.TryGetValue("npc_common", out value))
			{
				float num2 = (float)value * 0.2f;
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsPeopleMale), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, (int)num2);
				float num3 = (float)value * 0.1f;
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsPeopleFemale), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, (int)num3);
			}
			if (unusedUsablePointCount.TryGetValue("npc_common_limited", out value))
			{
				float num4 = (float)value * 0.6f;
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsManCarryingStuff), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, (int)num4);
			}
			if (unusedUsablePointCount.TryGetValue("shipyard_worker", out value))
			{
				float num5 = (float)value * 1f;
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateShipyardWorker), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, (int)num5);
			}
			if (unusedUsablePointCount.TryGetValue("market_worker", out value))
			{
				float num6 = (float)value * 0.75f;
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreatePortMarketWorker), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, (int)num6);
			}
			if (unusedUsablePointCount.TryGetValue("static_npc", out value))
			{
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateStaticTownsPeopleMale), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, value);
			}
			if (unusedUsablePointCount.TryGetValue("musician", out value) && value > 0)
			{
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMusician), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, value);
			}
		}
	}

	private static LocationCharacter CreateTownsManCarryingStuff(CultureObject culture, CharacterRelations relation)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		CharacterObject townsman = culture.Townsman;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townsman).Race, "_settlement_slow");
		var (text, text2, flag) = GetRandomActionSetSuffixAndItem();
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townsman, ref num, ref num2, "TownsfolkCarryingStuff");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townsman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		ItemObject val2 = Game.Current.ObjectManager.GetObject<ItemObject>(text2);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		LocationCharacter val3 = new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)townsman).IsFemale, text), true, false, val2, false, false, true, (AfterAgentCreatedDelegate)null, false);
		if (val2 == null)
		{
			val3.PrefabNamesForBones.Add(flag ? val.AgentMonster.MainHandItemBoneIndex : val.AgentMonster.OffHandItemBoneIndex, text2);
		}
		return val3;
	}

	private static LocationCharacter CreateTownsPeopleMale(CultureObject culture, CharacterRelations relation)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		CharacterObject townsman = culture.Townsman;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townsman).Race, "_settlement_slow");
		Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager_2"), monsterWithSuffix);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townsman, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townsman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddIndoorWandererBehaviors), "npc_common", false, relation, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateStaticTownsPeopleMale(CultureObject culture, CharacterRelations relation)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		CharacterObject townsman = culture.Townsman;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townsman).Race, "_settlement_slow");
		Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager_2"), monsterWithSuffix);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townsman, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townsman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddIndoorWandererBehaviors), "static_npc", false, relation, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateTownsPeopleFemale(CultureObject culture, CharacterRelations relation)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		CharacterObject townswoman = culture.Townswoman;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townswoman).Race, "_settlement_slow");
		Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, true, "_villager_2"), monsterWithSuffix);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townswoman, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townswoman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2).Age(MBRandom.RandomInt(num, num2)).IsFemale(true);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddIndoorWandererBehaviors), "npc_common", false, relation, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateShipyardWorker(CultureObject culture, CharacterRelations relation)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		CharacterObject shipyardWorker = culture.ShipyardWorker;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)shipyardWorker).Race, "_settlement_slow");
		Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager_2"), monsterWithSuffix);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(shipyardWorker, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)shipyardWorker, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddIndoorWandererBehaviors), "shipyard_worker", true, relation, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreatePortMarketWorker(CultureObject culture, CharacterRelations relation)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		CharacterObject shipyardWorker = culture.ShipyardWorker;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)shipyardWorker).Race, "_settlement_slow");
		Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager_2"), monsterWithSuffix);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(shipyardWorker, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)shipyardWorker, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddIndoorWandererBehaviors), "market_worker", true, relation, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreatePortMerchant(CultureObject culture, CharacterRelations relation)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		CharacterObject merchant = culture.Merchant;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)merchant).Race, "_settlement_slow");
		Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager_2"), monsterWithSuffix);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(merchant, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)merchant, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddFixedCharacterBehaviors), "shipyard_shop_worker", false, relation, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
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

	private static LocationCharacter CreateShipWright(CultureObject culture, CharacterRelations relation)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		CharacterObject shipwright = culture.Shipwright;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)shipwright).Race, "_settlement_slow");
		Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager_2"), monsterWithSuffix);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(shipwright, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)shipwright, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddFixedCharacterBehaviors), "sp_shipwright", true, relation, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	public static (string, string, bool) GetRandomActionSetSuffixAndItem()
	{
		string item = Extensions.GetRandomElement<(string, bool)>((IReadOnlyList<(string, bool)>)_itemToCarryAndIsMainHandData).Item1;
		return item switch
		{
			"wood_load" => ("_worker_carry_wood_on_shoulder", item, true), 
			"bucket_filled" => ("_villager_carry_bucket_on_lefthand", item, false), 
			"carry_fish_stick" => ("_villager_carry_fish_buckets", item, false), 
			_ => ("_worker_carry_wood_on_shoulder", item, true), 
		};
	}

	private static void AddDialogs(CampaignGameStarter campaignGameSystemStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		campaignGameSystemStarter.AddDialogLine("shipwright_dialog_start", "start", "close_window", "{=PZk5f99h}Greetings, {?PLAYER.GENDER}madam{?}sir{\\?}. This is where we lay the keels, fit the planks, and nail them all together.", new OnConditionDelegate(shipwright_default_dialog_start), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameSystemStarter.AddDialogLine("shipyard_market_worker", "start", "close_window", "{=!}Greetings, {?PLAYER.GENDER}madam{?}sir{\\?}. This is where we pack the stores for all those sailors and travelers about to put to sea.", new OnConditionDelegate(shipyard_marker_worker_default_dialog_start), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
	}

	private static (Occupation ConversationCharacterOccupation, string ConversationCharacterSpecialTag) GetConversationCharacterInfo()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (Campaign.Current.ConversationManager.OneToOneConversationCharacter != null && Campaign.Current.ConversationManager.OneToOneConversationAgent != null && Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.LocationComplex != null)
		{
			CharacterObject oneToOneConversationCharacter = Campaign.Current.ConversationManager.OneToOneConversationCharacter;
			IAgent oneToOneConversationAgent = Campaign.Current.ConversationManager.OneToOneConversationAgent;
			LocationCharacter obj = Settlement.CurrentSettlement.LocationComplex.FindCharacter(oneToOneConversationAgent);
			return new ValueTuple<Occupation, string>(item2: (obj != null) ? obj.SpecialTargetTag : null, item1: oneToOneConversationCharacter.Occupation);
		}
		return (ConversationCharacterOccupation: (Occupation)0, ConversationCharacterSpecialTag: string.Empty);
	}

	private static bool shipwright_default_dialog_start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		(Occupation, string) conversationCharacterInfo = GetConversationCharacterInfo();
		if ((int)conversationCharacterInfo.Item1 == 32)
		{
			if (!(conversationCharacterInfo.Item2 == "shipyard_worker"))
			{
				return conversationCharacterInfo.Item2 == "sp_shipwright";
			}
			return true;
		}
		return false;
	}

	private static bool shipyard_marker_worker_default_dialog_start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		(Occupation, string) conversationCharacterInfo = GetConversationCharacterInfo();
		if ((int)conversationCharacterInfo.Item1 == 32)
		{
			return conversationCharacterInfo.Item2 == "market_worker";
		}
		return false;
	}
}
