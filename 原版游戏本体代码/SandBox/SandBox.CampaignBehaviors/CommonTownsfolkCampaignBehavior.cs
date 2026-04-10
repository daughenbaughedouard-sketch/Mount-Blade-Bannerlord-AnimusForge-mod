using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.CampaignBehaviors;

public class CommonTownsfolkCampaignBehavior : CampaignBehaviorBase
{
	public const float TownsmanSpawnPercentageMale = 0.2f;

	public const float TownsmanSpawnPercentageFemale = 0.15f;

	public const float TownsmanSpawnPercentageLimitedMale = 0.15f;

	public const float TownsmanSpawnPercentageLimitedFemale = 0.1f;

	public const float TownOtherPeopleSpawnPercentage = 0.05f;

	public const float TownsmanSpawnPercentageTavernMale = 0.3f;

	public const float TownsmanSpawnPercentageTavernFemale = 0.1f;

	public const float BeggarSpawnPercentage = 0.33f;

	private float GetSpawnRate(Settlement settlement)
	{
		return TimeOfDayPercentage() * GetProsperityMultiplier(settlement.SettlementComponent) * GetWeatherEffectMultiplier(settlement);
	}

	private float GetConfigValue()
	{
		return BannerlordConfig.CivilianAgentCount;
	}

	private float GetProsperityMultiplier(SettlementComponent settlement)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ((float)settlement.GetProsperityLevel() + 1f) / 3f;
	}

	private float GetWeatherEffectMultiplier(Settlement settlement)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		MapWeatherModel mapWeatherModel = Campaign.Current.Models.MapWeatherModel;
		CampaignVec2 position = settlement.Position;
		WeatherEvent weatherEventInPosition = mapWeatherModel.GetWeatherEventInPosition(((CampaignVec2)(ref position)).ToVec2());
		if ((int)weatherEventInPosition != 2)
		{
			if ((int)weatherEventInPosition != 4)
			{
				return 1f;
			}
			return 0.4f;
		}
		return 0.15f;
	}

	private float TimeOfDayPercentage()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		int num = MathF.Ceiling((float)CampaignTime.HoursInDay * 0.625f);
		CampaignTime now = CampaignTime.Now;
		return 1f - MathF.Abs(((CampaignTime)(ref now)).CurrentHourInDay - (float)num) / (float)num;
	}

	public override void RegisterEvents()
	{
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		if (!settlement.IsCastle)
		{
			Location locationWithId = settlement.LocationComplex.GetLocationWithId("center");
			Location locationWithId2 = settlement.LocationComplex.GetLocationWithId("tavern");
			if (CampaignMission.Current.Location == locationWithId)
			{
				CampaignTime now = CampaignTime.Now;
				AddPeopleToTownCenter(settlement, unusedUsablePointCount, ((CampaignTime)(ref now)).IsDayTime);
			}
			if (CampaignMission.Current.Location == locationWithId2)
			{
				AddPeopleToTownTavern(settlement, unusedUsablePointCount);
			}
		}
	}

	private void AddPeopleToTownTavern(Settlement settlement, Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		Location locationWithId = settlement.LocationComplex.GetLocationWithId("tavern");
		unusedUsablePointCount.TryGetValue("npc_common", out var value);
		MapWeatherModel mapWeatherModel = Campaign.Current.Models.MapWeatherModel;
		CampaignVec2 position = settlement.Position;
		WeatherEvent weatherEventInPosition = mapWeatherModel.GetWeatherEventInPosition(((CampaignVec2)(ref position)).ToVec2());
		bool flag = (int)weatherEventInPosition == 2 || (int)weatherEventInPosition == 4;
		if (value > 0)
		{
			int num = (int)((float)value * (0.3f + (flag ? 0.2f : 0f)));
			if (num > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsManForTavern), settlement.Culture, (CharacterRelations)0, num);
			}
			int num2 = (int)((float)value * (0.1f + (flag ? 0.2f : 0f)));
			if (num2 > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsWomanForTavern), settlement.Culture, (CharacterRelations)0, num2);
			}
		}
	}

	private void AddPeopleToTownCenter(Settlement settlement, Dictionary<string, int> unusedUsablePointCount, bool isDayTime)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Invalid comparison between Unknown and I4
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Invalid comparison between Unknown and I4
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected O, but got Unknown
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Expected O, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Expected O, but got Unknown
		Location locationWithId = settlement.LocationComplex.GetLocationWithId("center");
		CultureObject culture = settlement.Culture;
		unusedUsablePointCount.TryGetValue("npc_common", out var value);
		unusedUsablePointCount.TryGetValue("npc_common_limited", out var value2);
		float num = (float)(value + value2) * 0.65000004f;
		if (num == 0f)
		{
			return;
		}
		float num2 = MBMath.ClampFloat(GetConfigValue() / num, 0f, 1f);
		float num3 = GetSpawnRate(settlement) * num2;
		if (value > 0)
		{
			int num4 = (int)((float)value * 0.2f * num3);
			if (num4 > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsMan), culture, (CharacterRelations)0, num4);
			}
			int num5 = (int)((float)value * 0.15f * num3);
			if (num5 > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsWoman), culture, (CharacterRelations)0, num5);
			}
		}
		MapWeatherModel mapWeatherModel = Campaign.Current.Models.MapWeatherModel;
		CampaignVec2 position = settlement.Position;
		WeatherEvent weatherEventInPosition = mapWeatherModel.GetWeatherEventInPosition(((CampaignVec2)(ref position)).ToVec2());
		bool flag = (int)weatherEventInPosition == 2 || (int)weatherEventInPosition == 4;
		if (!isDayTime || flag)
		{
			return;
		}
		if (value2 > 0)
		{
			int num6 = (int)((float)value2 * 0.15f * num3);
			if (num6 > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsManCarryingStuff), culture, (CharacterRelations)0, num6);
			}
			int num7 = (int)((float)value2 * 0.1f * num3);
			if (num7 > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsWomanCarryingStuff), culture, (CharacterRelations)0, num7);
			}
			int num8 = (int)((float)value2 * 0.05f * num3);
			if (num8 > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMaleChild), culture, (CharacterRelations)0, num8);
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateFemaleChild), culture, (CharacterRelations)0, num8);
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMaleTeenager), culture, (CharacterRelations)0, num8);
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateFemaleTeenager), culture, (CharacterRelations)0, num8);
			}
		}
		int value3 = 0;
		if (unusedUsablePointCount.TryGetValue("spawnpoint_cleaner", out value3))
		{
			locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateBroomsWoman), culture, (CharacterRelations)0, value3);
		}
		if (unusedUsablePointCount.TryGetValue("npc_dancer", out value3))
		{
			locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateDancer), culture, (CharacterRelations)0, value3);
		}
		if (settlement.IsTown && unusedUsablePointCount.TryGetValue("npc_beggar", out value3))
		{
			locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateFemaleBeggar), culture, (CharacterRelations)0, (value3 != 1) ? (value3 / 2) : 0);
			locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMaleBeggar), culture, (CharacterRelations)0, (value3 == 1) ? 1 : (value3 / 2));
		}
	}

	public static string GetActionSetSuffixAndMonsterForItem(string itemId, int race, bool isFemale, out Monster monster)
	{
		monster = FaceGen.GetMonsterWithSuffix(race, "_settlement");
		switch (itemId)
		{
		case "_to_carry_kitchen_pot_c":
			return "_villager_carry_right_hand";
		case "_to_carry_arm_kitchen_pot_c":
			return "_villager_carry_right_arm";
		case "_to_carry_kitchen_pitcher_a":
			return "_villager_carry_over_head";
		case "_to_carry_foods_basket_apple":
			return "_villager_carry_over_head_v2";
		case "_to_carry_merchandise_hides_b":
			return "_villager_with_backpack";
		case "_to_carry_foods_pumpkin_a":
			return "_villager_carry_front_v2";
		case "_to_carry_bd_fabric_c":
		case "_to_carry_foods_watermelon_a":
			return "_villager_carry_right_side";
		case "_to_carry_bed_convolute_a":
			return "_villager_carry_front";
		case "_to_carry_bed_convolute_g":
			return "_villager_carry_on_shoulder";
		case "_to_carry_bd_basket_a":
			return "_villager_with_backpack";
		case "practice_spear_t1":
			return "_villager_with_staff";
		case "simple_sparth_axe_t2":
			return "_villager_carry_axe";
		default:
			return "_villager_carry_right_hand";
		}
	}

	public static Tuple<string, Monster> GetRandomTownsManActionSetAndMonster(int race)
	{
		switch (MBRandom.RandomInt(3))
		{
		case 0:
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(race, "_settlement");
			return new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager"), monsterWithSuffix);
		}
		case 1:
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(race, "_settlement_slow");
			return new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager_2"), monsterWithSuffix);
		}
		default:
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(race, "_settlement");
			return new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, false, "_villager_3"), monsterWithSuffix);
		}
		}
	}

	public static Tuple<string, Monster> GetRandomTownsWomanActionSetAndMonster(int race)
	{
		Monster monsterWithSuffix;
		if (MBRandom.RandomInt(4) == 0)
		{
			monsterWithSuffix = FaceGen.GetMonsterWithSuffix(race, "_settlement_fast");
			return new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, true, "_villager"), monsterWithSuffix);
		}
		monsterWithSuffix = FaceGen.GetMonsterWithSuffix(race, "_settlement_slow");
		return new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, true, "_villager_2"), monsterWithSuffix);
	}

	private static LocationCharacter CreateTownsMan(CultureObject culture, CharacterRelations relation)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		CharacterObject townsman = culture.Townsman;
		Tuple<string, Monster> randomTownsManActionSetAndMonster = GetRandomTownsManActionSetAndMonster(((BasicCharacterObject)townsman).Race);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townsman, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townsman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(randomTownsManActionSetAndMonster.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common", false, relation, randomTownsManActionSetAndMonster.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateTownsManForTavern(CultureObject culture, CharacterRelations relation)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		CharacterObject townsman = culture.Townsman;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townsman).Race, "_settlement_slow");
		string text = ((!(((MBObjectBase)culture).StringId.ToLower() == "aserai") && !(((MBObjectBase)culture).StringId.ToLower() == "khuzait")) ? ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, ((BasicCharacterObject)townsman).IsFemale, "_villager_in_tavern") : ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, ((BasicCharacterObject)townsman).IsFemale, "_villager_in_aserai_tavern"));
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townsman, ref num, ref num2, "TavernVisitor");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townsman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, text, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateTownsWomanForTavern(CultureObject culture, CharacterRelations relation)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		CharacterObject townswoman = culture.Townswoman;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townswoman).Race, "_settlement_slow");
		string text = ((!(((MBObjectBase)culture).StringId.ToLower() == "aserai") && !(((MBObjectBase)culture).StringId.ToLower() == "khuzait")) ? ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, ((BasicCharacterObject)townswoman).IsFemale, "_warrior_in_tavern") : ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, ((BasicCharacterObject)townswoman).IsFemale, "_warrior_in_aserai_tavern"));
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townswoman, ref num, ref num2, "TavernVisitor");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townswoman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, text, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateTownsManCarryingStuff(CultureObject culture, CharacterRelations relation)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		CharacterObject townsman = culture.Townsman;
		string randomStuff = SettlementHelper.GetRandomStuff(false);
		Monster monster;
		string actionSetSuffixAndMonsterForItem = GetActionSetSuffixAndMonsterForItem(randomStuff, ((BasicCharacterObject)townsman).Race, isFemale: false, out monster);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townsman, ref num, ref num2, "TownsfolkCarryingStuff");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townsman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monster).Age(MBRandom.RandomInt(num, num2));
		ItemObject val2 = Game.Current.ObjectManager.GetObject<ItemObject>(randomStuff);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		LocationCharacter val3 = new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)townsman).IsFemale, actionSetSuffixAndMonsterForItem), true, false, val2, false, false, true, (AfterAgentCreatedDelegate)null, false);
		if (val2 == null)
		{
			val3.PrefabNamesForBones.Add(val.AgentMonster.MainHandItemBoneIndex, randomStuff);
		}
		return val3;
	}

	private static LocationCharacter CreateTownsWoman(CultureObject culture, CharacterRelations relation)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		CharacterObject townswoman = culture.Townswoman;
		Tuple<string, Monster> randomTownsWomanActionSetAndMonster = GetRandomTownsWomanActionSetAndMonster(((BasicCharacterObject)townswoman).Race);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townswoman, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townswoman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(randomTownsWomanActionSetAndMonster.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common", false, relation, randomTownsWomanActionSetAndMonster.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateMaleChild(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		CharacterObject townsmanChild = culture.TownsmanChild;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townsmanChild).Race, "_child");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townsmanChild, ref num, ref num2, "Child");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townsmanChild, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)townsmanChild).IsFemale, "_child"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateFemaleChild(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		CharacterObject townswomanChild = culture.TownswomanChild;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townswomanChild).Race, "_child");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townswomanChild, ref num, ref num2, "Child");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townswomanChild, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)townswomanChild).IsFemale, "_child"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateMaleTeenager(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		CharacterObject townsmanTeenager = culture.TownsmanTeenager;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townsmanTeenager).Race, "_child");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townsmanTeenager, ref num, ref num2, "Teenager");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townsmanTeenager, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)townsmanTeenager).IsFemale, "_villager"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateFemaleTeenager(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		CharacterObject townswomanTeenager = culture.TownswomanTeenager;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townswomanTeenager).Race, "_child");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townswomanTeenager, ref num, ref num2, "Teenager");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townswomanTeenager, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)townswomanTeenager).IsFemale, "_villager"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateTownsWomanCarryingStuff(CultureObject culture, CharacterRelations relation)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		CharacterObject townswoman = culture.Townswoman;
		string randomStuff = SettlementHelper.GetRandomStuff(true);
		Monster monster;
		string actionSetSuffixAndMonsterForItem = GetActionSetSuffixAndMonsterForItem(randomStuff, ((BasicCharacterObject)townswoman).Race, isFemale: false, out monster);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townswoman, ref num, ref num2, "TownsfolkCarryingStuff");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townswoman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monster).Age(MBRandom.RandomInt(num, num2));
		ItemObject val2 = Game.Current.ObjectManager.GetObject<ItemObject>(randomStuff);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		LocationCharacter val3 = new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)townswoman).IsFemale, actionSetSuffixAndMonsterForItem), true, false, val2, false, false, true, (AfterAgentCreatedDelegate)null, false);
		if (val2 == null)
		{
			val3.PrefabNamesForBones.Add(val.AgentMonster.MainHandItemBoneIndex, randomStuff);
		}
		return val3;
	}

	public static LocationCharacter CreateBroomsWoman(CultureObject culture, CharacterRelations relation)
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
		CharacterObject townswoman = culture.Townswoman;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)townswoman).Race, "_settlement");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(townswoman, ref num, ref num2, "BroomsWoman");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)townswoman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "spawnpoint_cleaner", false, relation, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private static LocationCharacter CreateDancer(CultureObject culture, CharacterRelations relation)
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
		CharacterObject femaleDancer = culture.FemaleDancer;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)femaleDancer).Race, "_settlement");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(femaleDancer, ref num, ref num2, "Dancer");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)femaleDancer, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_dancer", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_dancer"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	public static LocationCharacter CreateMaleBeggar(CultureObject culture, CharacterRelations relation)
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
		CharacterObject beggar = culture.Beggar;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)beggar).Race, "_settlement");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(beggar, ref num, ref num2, "Beggar");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)beggar, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_beggar", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_beggar"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	public static LocationCharacter CreateFemaleBeggar(CultureObject culture, CharacterRelations relation)
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
		CharacterObject femaleBeggar = culture.FemaleBeggar;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)femaleBeggar).Race, "_settlement");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(femaleBeggar, ref num, ref num2, "Beggar");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)femaleBeggar, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_beggar", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_beggar"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}
}
