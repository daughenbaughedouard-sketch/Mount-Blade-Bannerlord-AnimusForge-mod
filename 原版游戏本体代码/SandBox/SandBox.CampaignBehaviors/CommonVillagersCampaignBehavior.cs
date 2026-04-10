using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Conversation;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.CampaignBehaviors;

public class CommonVillagersCampaignBehavior : CampaignBehaviorBase
{
	public const float VillagerSpawnPercentageMale = 0.25f;

	public const float VillagerSpawnPercentageFemale = 0.2f;

	public const float VillagerSpawnPercentageLimited = 0.2f;

	public const float VillageOtherPeopleSpawnPercentage = 0.05f;

	private readonly Dictionary<int, string> _rumorsGiven = new Dictionary<int, string>();

	private CampaignTime _lastEnteredTime;

	private float TradeRumorDistance => Campaign.Current.EstimatedAverageCaravanPartySpeed * (float)CampaignTime.HoursInDay;

	public override void RegisterEvents()
	{
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
	}

	private float GetSpawnRate(Settlement settlement)
	{
		return TimeOfDayPercentage() * GetProsperityMultiplier(settlement.SettlementComponent);
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

	private float TimeOfDayPercentage()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		CampaignTime now = CampaignTime.Now;
		return 1f - MathF.Abs(((CampaignTime)(ref now)).CurrentHourInDay - (float)MathF.Ceiling((float)CampaignTime.HoursInDay * 0.625f)) / (float)MathF.Ceiling((float)CampaignTime.HoursInDay * 0.625f);
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		Location locationWithId = settlement.LocationComplex.GetLocationWithId("village_center");
		if (CampaignMission.Current.Location == locationWithId)
		{
			CampaignTime now = CampaignTime.Now;
			AddVillageCenterCharacters(settlement, unusedUsablePointCount, !((CampaignTime)(ref now)).IsDayTime);
		}
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

	private void AddVillageCenterCharacters(Settlement settlement, Dictionary<string, int> unusedUsablePointCount, bool isNight)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Expected O, but got Unknown
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		Location locationWithId = settlement.LocationComplex.GetLocationWithId("village_center");
		CultureObject culture = settlement.Culture;
		unusedUsablePointCount.TryGetValue("npc_common", out var value);
		unusedUsablePointCount.TryGetValue("npc_common_limited", out var value2);
		float num = (float)(value + value2) * 0.65f;
		float num2 = MBMath.ClampFloat(GetConfigValue() / num, 0f, 1f);
		float num3 = GetSpawnRate(settlement) * num2 * GetWeatherEffectMultiplier(settlement);
		if (locationWithId == null || CampaignMission.Current.Location != locationWithId)
		{
			return;
		}
		if (value > 0)
		{
			int num4 = (int)((float)value * 0.25f * num3);
			if (num4 > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateVillageMan), culture, (CharacterRelations)0, num4);
			}
			int num5 = (int)((float)value * 0.2f * num3);
			if (num5 > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateVillageWoman), culture, (CharacterRelations)0, num5);
			}
			if (!isNight)
			{
				int num6 = (int)((float)value * 0.05f * num3);
				if (num6 > 0)
				{
					locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMaleChild), culture, (CharacterRelations)0, num6);
					locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateFemaleChild), culture, (CharacterRelations)0, num6);
					locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMaleTeenager), culture, (CharacterRelations)0, num6);
					locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateFemaleTeenager), culture, (CharacterRelations)0, num6);
				}
			}
		}
		if (value2 > 0)
		{
			int num7 = (int)((float)value2 * 0.2f * num3);
			if (num7 > 0)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateVillageManCarryingStuff), culture, (CharacterRelations)0, num7 / 2);
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateVillageWomanCarryingStuff), culture, (CharacterRelations)0, num7 / 2);
			}
		}
		int value3 = 0;
		if (unusedUsablePointCount.TryGetValue("spawnpoint_cleaner", out value3))
		{
			locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CommonTownsfolkCampaignBehavior.CreateBroomsWoman), culture, (CharacterRelations)0, value3);
		}
		if (unusedUsablePointCount.TryGetValue("npc_beggar", out value3))
		{
			locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CommonTownsfolkCampaignBehavior.CreateFemaleBeggar), culture, (CharacterRelations)0, value3 / 2);
			locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CommonTownsfolkCampaignBehavior.CreateMaleBeggar), culture, (CharacterRelations)0, value3 / 2);
		}
	}

	public void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
	{
		if (settlement.IsVillage || settlement.IsFortification)
		{
			SettlementHelper.TakeEnemyVillagersOutsideSettlements(settlement);
		}
	}

	private LocationCharacter CreateVillageMan(CultureObject culture, CharacterRelations relation)
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
		CharacterObject villager = culture.Villager;
		Tuple<string, Monster> randomTownsManActionSetAndMonster = CommonTownsfolkCampaignBehavior.GetRandomTownsManActionSetAndMonster(((BasicCharacterObject)villager).Race);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(villager, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)villager, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(randomTownsManActionSetAndMonster.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common", false, relation, randomTownsManActionSetAndMonster.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreateMaleChild(CultureObject culture, CharacterRelations relation)
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
		CharacterObject villagerMaleChild = culture.VillagerMaleChild;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)villagerMaleChild).Race, "_child");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(villagerMaleChild, ref num, ref num2, "Child");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)villagerMaleChild, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)villagerMaleChild).IsFemale, "_child"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreateFemaleChild(CultureObject culture, CharacterRelations relation)
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
		CharacterObject villagerFemaleChild = culture.VillagerFemaleChild;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)villagerFemaleChild).Race, "_child");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(villagerFemaleChild, ref num, ref num2, "Child");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)villagerFemaleChild, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)villagerFemaleChild).IsFemale, "_child"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreateMaleTeenager(CultureObject culture, CharacterRelations relation)
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
		CharacterObject villagerMaleTeenager = culture.VillagerMaleTeenager;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)villagerMaleTeenager).Race, "_child");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(villagerMaleTeenager, ref num, ref num2, "Teenager");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)villagerMaleTeenager, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)villagerMaleTeenager).IsFemale, "_villager"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreateFemaleTeenager(CultureObject culture, CharacterRelations relation)
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
		CharacterObject villagerFemaleTeenager = culture.VillagerFemaleTeenager;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)villagerFemaleTeenager).Race, "_child");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(villagerFemaleTeenager, ref num, ref num2, "Teenager");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)villagerFemaleTeenager, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)villagerFemaleTeenager).IsFemale, "_villager"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreateVillageManCarryingStuff(CultureObject culture, CharacterRelations relation)
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
		CharacterObject villager = culture.Villager;
		string randomStuff = SettlementHelper.GetRandomStuff(false);
		Monster monster;
		string actionSetSuffixAndMonsterForItem = CommonTownsfolkCampaignBehavior.GetActionSetSuffixAndMonsterForItem(randomStuff, ((BasicCharacterObject)villager).Race, isFemale: false, out monster);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(villager, ref num, ref num2, "TownsfolkCarryingStuff");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)villager, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monster).Age(MBRandom.RandomInt(num, num2));
		ItemObject val2 = Game.Current.ObjectManager.GetObject<ItemObject>(randomStuff);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		LocationCharacter val3 = new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)villager).IsFemale, actionSetSuffixAndMonsterForItem), true, false, val2, false, false, true, (AfterAgentCreatedDelegate)null, false);
		if (val2 == null)
		{
			val3.PrefabNamesForBones.Add(val.AgentMonster.MainHandItemBoneIndex, randomStuff);
		}
		return val3;
	}

	private LocationCharacter CreateVillageWoman(CultureObject culture, CharacterRelations relation)
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
		CharacterObject villageWoman = culture.VillageWoman;
		Tuple<string, Monster> randomTownsWomanActionSetAndMonster = CommonTownsfolkCampaignBehavior.GetRandomTownsWomanActionSetAndMonster(((BasicCharacterObject)villageWoman).Race);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(villageWoman, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)villageWoman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(randomTownsWomanActionSetAndMonster.Item2).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common", false, relation, randomTownsWomanActionSetAndMonster.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreateVillageWomanCarryingStuff(CultureObject culture, CharacterRelations relation)
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
		CharacterObject villageWoman = culture.VillageWoman;
		string randomStuff = SettlementHelper.GetRandomStuff(true);
		Monster monster;
		string actionSetSuffixAndMonsterForItem = CommonTownsfolkCampaignBehavior.GetActionSetSuffixAndMonsterForItem(randomStuff, ((BasicCharacterObject)villageWoman).Race, isFemale: false, out monster);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(villageWoman, ref num, ref num2, "TownsfolkCarryingStuff");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)villageWoman, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monster).Age(MBRandom.RandomInt(num, num2));
		ItemObject val2 = Game.Current.ObjectManager.GetObject<ItemObject>(randomStuff);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		LocationCharacter val3 = new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common_limited", false, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, ((BasicCharacterObject)villageWoman).IsFemale, actionSetSuffixAndMonsterForItem), true, false, val2, false, false, true, (AfterAgentCreatedDelegate)null, false);
		if (val2 == null)
		{
			val3.PrefabNamesForBones.Add(val.AgentMonster.MainHandItemBoneIndex, randomStuff);
		}
		return val3;
	}

	protected void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		AddTownspersonAndVillagerDialogs(campaignGameStarter);
	}

	private void AddTownspersonAndVillagerDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Expected O, but got Unknown
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Expected O, but got Unknown
		//IL_0288: Expected O, but got Unknown
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Expected O, but got Unknown
		//IL_02e8: Expected O, but got Unknown
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Expected O, but got Unknown
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Expected O, but got Unknown
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Expected O, but got Unknown
		//IL_0417: Expected O, but got Unknown
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Expected O, but got Unknown
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("town_beggar_to_disguised_character", "start", "close_window", "{=iVJlUlOg}Look, friend, we'll both eat better tonight if you move to a different spot. Too many of us, and the masters and ladies give a wide berth.", new OnConditionDelegate(conversation_beggar_to_disguise_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_talk_to_disguised_character", "start", "close_window", "{=qGIFpahv}Can't spare any coins for you. Sorry. May Heaven provide.", new OnConditionDelegate(conversation_townsperson_to_disguise_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_start", "start", "town_or_village_talk", "{=!}{CONVERSATION_SCRAP}", new OnConditionDelegate(conversation_town_or_village_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_talk_beggar", "town_or_village_talk", "close_window", "{=kFWmcRpV}The Heavens repay kindness with kindness, my {?PLAYER.GENDER}lady{?}lord{\\?}.", new OnConditionDelegate(conversation_beggar_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_talk", "town_or_village_talk", "town_or_village_player_children_post_rhyme", "{=jZF4l0jY}Oh, sorry, {?PLAYER.GENDER}madam{?}sir{\\?}. May I be of service?", new OnConditionDelegate(conversation_children_rhymes_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_talk_children", "town_or_village_talk", "town_or_village_player", "{=KPfs7L7B}Ah, my apologies, {?PLAYER.GENDER}madam{?}sir{\\?}. May I help you with something?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_start_2", "start", "close_window", "{=IrsaIJ4u}Ay, these are hard days indeed...", new OnConditionDelegate(conversation_beggar_delivered_line_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_start_postrumor_liege", "start", "town_or_village_player", "{=eKLH9fOb}May I be of service, {?PLAYER.GENDER}your ladyship{?}your lordship{\\?}?", new OnConditionDelegate(conversation_liege_delivered_line_on_street_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_start_postrumor_children", "start", "town_or_village_children_player_no_rhyme", "{=Osaupw6M}Beg pardon, {?PLAYER.GENDER}madam{?}sir{\\?}, I need to get back to my parents. Is there anything you need?", new OnConditionDelegate(conversation_children_already_delivered_line_on_street_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_start_postrumor_tavern", "start", "town_or_village_player", "{=X1A4r7wY}Your good health, {?PLAYER.GENDER}madam{?}sir{\\?}. May I help you?", new OnConditionDelegate(conversation_already_delivered_line_in_tavern_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_start_postrumor", "start", "town_or_village_player", "{=P99OLPWU}Excuse me, {?PLAYER.GENDER}madam{?}sir{\\?}, but I must shortly go about my business. Is there anything you need?", new OnConditionDelegate(conversation_already_delivered_line_on_street_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_run_along_children_1", "town_or_village_player_children_post_rhyme", "close_window", "{=7kn4Jmdl}Ah, yes! We had such rhymes when I was young.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_run_along_children_2", "town_or_village_player_children_post_rhyme", "close_window", "{=jGxotDqF}Best not sing that to every stranger you meet, child.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_run_along_children_3", "town_or_village_player_children_post_rhyme", "close_window", "{=bhkC3kcQ}Speak respectfully about your elders, you filthy ragamuffin!", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_run_along_children", "town_or_village_children_player_no_rhyme", "close_window", "{=PV56VAFg}Run along now, child.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_ask_hero_location", "town_or_village_player", "player_ask_hero_location", "{=urieibC4}I'm looking for someone.", new OnConditionDelegate(conversation_town_or_village_player_ask_location_of_hero_on_condition), new OnConsequenceDelegate(conversation_town_or_village_player_ask_location_of_hero_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("player_ask_hero_location_di1", "player_ask_hero_location", "player_ask_hero_location_2", "{=cqlV1YLO}Whom are you looking for?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddRepeatablePlayerLine("player_ask_hero_location_2", "player_ask_hero_location_2", "player_ask_hero_location_3", "{=obY78MnQ}{HERO.LINK}", "{=4hDu8rDF}I am thinking of a different person.", "player_ask_hero_location", new OnConditionDelegate(conversation_town_or_village_player_ask_location_of_hero_2_on_condition), new OnConsequenceDelegate(conversation_town_or_village_player_ask_location_of_hero_2_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_ask_hero_location_2_2", "player_ask_hero_location_2", "town_or_village_pretalk", "{=D33fIGQe}Never mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_pretalk", "town_or_village_pretalk", "town_or_village_player", "{=ds294zxi}Anything else?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("player_ask_hero_location_3", "player_ask_hero_location_3", "player_ask_hero_location_4", "{=qN2LYVIO}Yes, I know {HERO.LINK}.", new OnConditionDelegate(conversation_town_or_village_player_ask_location_of_hero_3_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("player_ask_hero_location_4_di", "player_ask_hero_location_3", "town_or_village_pretalk", "{=woMdU4Xl}I don't know where {?HERO.GENDER}she{?}he{\\?} is.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_ask_hero_location_4", "player_ask_hero_location_4", "player_ask_hero_location_5", "{=a1FeLSbH}Can you take me to {?HERO.GENDER}her{?}him{\\?}?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_ask_hero_location_4_2", "player_ask_hero_location_4", "town_or_village_pretalk", "{=D33fIGQe}Never mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("player_ask_hero_location_5", "player_ask_hero_location_5", "close_window", "{=mhgUwwZb}Sure. Follow me.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_town_or_village_player_ask_location_of_hero_5_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_escort_complete", "start", "town_or_village_pretalk", "{=9PBA2OJz}Here {?HERO.GENDER}she{?}he{\\?} is.", new OnConditionDelegate(conversation_town_or_village_escort_complete_on_condition), new OnConsequenceDelegate(conversation_town_or_village_escort_complete_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("town_or_village_start_3", "start", "town_or_village_escorting", "{=ym6bSrNo}{STILL_ESCORTING_ANSWER}", new OnConditionDelegate(conversation_town_or_village_talk_escorting_commoner_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("town_or_village_escorting_changed_my_mind", "town_or_village_escorting", "town_or_village_pretalk", "{=fkkYatnM}Actually, I've changed my mind. I'll talk to {?HERO.GENDER}her{?}him{\\?} later...", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_town_or_village_talk_stop_escorting_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("town_or_village_escorting_keep_going", "town_or_village_escorting", "close_window", "{=QTZjjOXb}Let us keep going.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("town_or_village_player", "town_or_village_player", "close_window", "{=OlOhuO7X}No thank you. Good day to you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
	}

	private bool CheckIfConversationAgentIsEscortingTheMainAgent()
	{
		if (Agent.Main != null && Agent.Main.IsActive() && Settlement.CurrentSettlement != null && ConversationMission.OneToOneConversationAgent != null)
		{
			return EscortAgentBehavior.CheckIfAgentIsEscortedBy(ConversationMission.OneToOneConversationAgent, Agent.Main);
		}
		return false;
	}

	private bool CheckIfTheMainAgentIsBeingEscorted()
	{
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (EscortAgentBehavior.CheckIfAgentIsEscortedBy(item, Agent.Main))
			{
				return true;
			}
		}
		return false;
	}

	private bool conversation_town_or_village_player_ask_location_of_hero_on_condition()
	{
		if (!CheckIfTheMainAgentIsBeingEscorted())
		{
			return heroes_to_look_for().Count != 0;
		}
		return false;
	}

	private void conversation_town_or_village_player_ask_location_of_hero_on_consequence()
	{
		ConversationSentence.SetObjectsToRepeatOver((IReadOnlyList<object>)heroes_to_look_for(), 5);
	}

	private List<Hero> heroes_to_look_for()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		List<Hero> list = new List<Hero>();
		Vec3 position = ConversationMission.OneToOneConversationAgent.Position;
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (item.IsHuman && item.IsHero && (int)item.State == 1)
			{
				Hero heroObject = ((CharacterObject)item.Character).HeroObject;
				if (!heroObject.IsLord && ((Vec3)(ref position)).Distance(item.Position) > 6f)
				{
					list.Add(heroObject);
				}
			}
		}
		return list;
	}

	private bool conversation_town_or_village_player_ask_location_of_hero_2_on_condition()
	{
		if (!CheckIfTheMainAgentIsBeingEscorted())
		{
			object currentProcessedRepeatObject = ConversationSentence.CurrentProcessedRepeatObject;
			Hero val = (Hero)((currentProcessedRepeatObject is Hero) ? currentProcessedRepeatObject : null);
			if (val != null)
			{
				StringHelpers.SetRepeatableCharacterProperties("HERO", val.CharacterObject, false);
				return true;
			}
		}
		return false;
	}

	private void conversation_town_or_village_player_ask_location_of_hero_2_on_consequence()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ConversationHelper.AskedLord = ((Hero)ConversationSentence.SelectedRepeatObject).CharacterObject;
	}

	private bool conversation_town_or_village_player_ask_location_of_hero_3_on_condition()
	{
		Hero heroObject = ConversationHelper.AskedLord.HeroObject;
		Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(heroObject);
		StringHelpers.SetCharacterProperties("HERO", heroObject.CharacterObject, (TextObject)null, false);
		return locationOfCharacter == CampaignMission.Current.Location;
	}

	private void conversation_town_or_village_player_ask_location_of_hero_5_on_consequence()
	{
		Hero heroObject = ConversationHelper.AskedLord.HeroObject;
		Agent conversationAgent = ConversationMission.OneToOneConversationAgent;
		Agent targetAgent = null;
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if ((object)item.Character == heroObject.CharacterObject)
			{
				targetAgent = item;
				break;
			}
		}
		EscortAgentBehavior.AddEscortAgentBehavior(conversationAgent, targetAgent, delegate
		{
			if (!Campaign.Current.ConversationManager.IsConversationFlowActive)
			{
				MissionConversationLogic.Current.StartConversation(conversationAgent, setActionsInstantly: false);
			}
			return false;
		});
	}

	public bool conversation_town_or_village_escort_complete_on_condition()
	{
		if (CheckIfConversationAgentIsEscortingTheMainAgent())
		{
			EscortAgentBehavior behavior = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<InterruptingBehaviorGroup>().GetBehavior<EscortAgentBehavior>();
			if (behavior.IsEscortFinished())
			{
				MBTextManager.SetTextVariable("HERO_GENDER", behavior.TargetAgent.Character.IsFemale ? 1 : 0);
				return true;
			}
		}
		return false;
	}

	public void conversation_town_or_village_escort_complete_on_consequence()
	{
		Agent oneToOneConversationAgent = ConversationMission.OneToOneConversationAgent;
		if (((oneToOneConversationAgent != null) ? oneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator : null) != null)
		{
			EscortAgentBehavior.RemoveEscortBehaviorOfAgent(oneToOneConversationAgent);
		}
	}

	private bool conversation_town_or_village_talk_escorting_commoner_on_condition()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		if (CheckIfConversationAgentIsEscortingTheMainAgent())
		{
			EscortAgentBehavior behavior = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<InterruptingBehaviorGroup>().GetBehavior<EscortAgentBehavior>();
			float randomFloat = MBRandom.RandomFloat;
			TextObject val = ((randomFloat < 0.33f) ? new TextObject("{=Eb7KG1bi}{HERO.LINK} should be just around the corner...[ib:normal]", (Dictionary<string, object>)null) : ((!(randomFloat < 0.66f)) ? new TextObject("{=fasAZDvM}We're on our way to {HERO.LINK}...[ib:demure]", (Dictionary<string, object>)null) : new TextObject("{=uhwoWUyR}Still haven't taken you to {HERO.LINK}...[ib:demure]", (Dictionary<string, object>)null)));
			TextObjectExtensions.SetCharacterProperties(val, "HERO", (CharacterObject)behavior.TargetAgent.Character, false);
			MBTextManager.SetTextVariable("STILL_ESCORTING_ANSWER", val, false);
			return true;
		}
		return false;
	}

	private void conversation_town_or_village_talk_stop_escorting_on_consequence()
	{
		if (CheckIfConversationAgentIsEscortingTheMainAgent())
		{
			EscortAgentBehavior.RemoveEscortBehaviorOfAgent(ConversationMission.OneToOneConversationAgent);
		}
	}

	private bool conversation_liege_delivered_line_on_street_on_condition()
	{
		if (conversation_already_delivered_line_on_street_on_condition())
		{
			if (Settlement.CurrentSettlement.MapFaction.Leader != Hero.MainHero)
			{
				return Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan;
			}
			return true;
		}
		return false;
	}

	private bool conversation_children_already_delivered_line_on_street_on_condition()
	{
		_ = Campaign.Current.ConversationManager.OneToOneConversationAgent;
		if (conversation_already_delivered_line_on_street_on_condition() && conversation_children_rhymes_on_condition())
		{
			return true;
		}
		return false;
	}

	private bool conversation_already_delivered_line_on_street_on_condition()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		if (!CheckIfConversationAgentIsEscortingTheMainAgent())
		{
			if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 6 || (int)CharacterObject.OneToOneConversationCharacter.Occupation == 8)
			{
				return PlayerEncounter.InsideSettlement;
			}
			return false;
		}
		return false;
	}

	private bool conversation_already_delivered_line_in_tavern_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if (((int)CharacterObject.OneToOneConversationCharacter.Occupation == 6 || (int)CharacterObject.OneToOneConversationCharacter.Occupation == 8) && PlayerEncounter.InsideSettlement && CampaignMission.Current != null && CampaignMission.Current.Location.StringId == "tavern" && !CheckIfConversationAgentIsEscortingTheMainAgent())
		{
			return true;
		}
		return false;
	}

	private bool conversation_children_rhymes_on_condition()
	{
		return Campaign.Current.ConversationManager.OneToOneConversationAgent.Age < (float)Campaign.Current.Models.AgeModel.BecomeTeenagerAge;
	}

	private bool conversation_townsperson_to_disguise_start_on_condition()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		if (CheckIfConversationAgentIsEscortingTheMainAgent() || Campaign.Current.ConversationManager.OneToOneConversationAgent == null)
		{
			return false;
		}
		if (((int)CharacterObject.OneToOneConversationCharacter.Occupation == 6 || (int)CharacterObject.OneToOneConversationCharacter.Occupation == 8) && PlayerEncounter.Current != null && PlayerEncounter.InsideSettlement)
		{
			return Campaign.Current.IsMainHeroDisguised;
		}
		return false;
	}

	private bool conversation_beggar_to_disguise_start_on_condition()
	{
		if (conversation_beggar_start_on_condition())
		{
			return Campaign.Current.IsMainHeroDisguised;
		}
		return false;
	}

	private bool conversation_beggar_start_on_condition()
	{
		if (CheckIfConversationAgentIsEscortingTheMainAgent() || Campaign.Current.ConversationManager.OneToOneConversationAgent == null)
		{
			return false;
		}
		if (Settlement.CurrentSettlement == null)
		{
			return false;
		}
		if (Campaign.Current.ConversationManager.OneToOneConversationCharacter == Settlement.CurrentSettlement.Culture.Beggar || Campaign.Current.ConversationManager.OneToOneConversationCharacter == Settlement.CurrentSettlement.Culture.FemaleBeggar)
		{
			return true;
		}
		return false;
	}

	private bool conversation_beggar_delivered_line_on_condition()
	{
		if (CheckIfConversationAgentIsEscortingTheMainAgent() || Campaign.Current.ConversationManager.OneToOneConversationAgent == null)
		{
			return false;
		}
		if (Settlement.CurrentSettlement == null)
		{
			return false;
		}
		if (Campaign.Current.ConversationManager.OneToOneConversationCharacter == Settlement.CurrentSettlement.Culture.Beggar || Campaign.Current.ConversationManager.OneToOneConversationCharacter == Settlement.CurrentSettlement.Culture.FemaleBeggar)
		{
			return conversation_already_delivered_line_on_street_on_condition();
		}
		return false;
	}

	private bool conversation_beggar_info_on_condition()
	{
		MBTextManager.SetTextVariable("BEGGAR_INFO", "{=zJReEB8Z}Sitting in the marketplace all day one learns many things if one keeps one's ears open... But alas, I have heard nothing recently that might interest your worshipful self.", false);
		return true;
	}

	private bool conversation_town_or_village_start_on_condition()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		if (CheckIfConversationAgentIsEscortingTheMainAgent() || Campaign.Current.ConversationManager.OneToOneConversationAgent == null)
		{
			return false;
		}
		if (((int)CharacterObject.OneToOneConversationCharacter.Occupation == 6 || (int)CharacterObject.OneToOneConversationCharacter.Occupation == 8) && PlayerEncounter.Current != null && PlayerEncounter.InsideSettlement)
		{
			if (_lastEnteredTime != CampaignTime.Now)
			{
				_lastEnteredTime = CampaignTime.Now;
				_rumorsGiven.Clear();
			}
			int key = MathF.Abs(((object)Campaign.Current.ConversationManager.OneToOneConversationAgent).GetHashCode());
			if (_rumorsGiven.ContainsKey(key))
			{
				return false;
			}
			List<TextObject> list = new List<TextObject>();
			if (conversation_children_rhymes_on_condition())
			{
				list.Add(new TextObject("{=aLAyJPrI}Garios Garios brave and strong... Always right and never wrong...{newline}The men all trust him with their lives.. But not their daughters or their wives.", (Dictionary<string, object>)null));
				list.Add(new TextObject("{=5ZbsASDx}Emp'ror Arenicos feeling fine. Went to bed with a cup of wine.{newline}In the morning he was dead. Now his wife rules in his stead!", (Dictionary<string, object>)null));
				list.Add(new TextObject("{=FxKgQECI}Lucon Lucon I've been told, sir. Was born old and just got older.{newline}Lives in a palace made of gold. Where it's always dark and it's always cold.", (Dictionary<string, object>)null));
				list.Add(new TextObject("{=YDMlCaaz}Great Khan Monchug lifts his hand. We all hark to his command!{newline}Great Khan Monchug rides away. We go back to nap and play.", (Dictionary<string, object>)null));
				list.Add(new TextObject("{=wrEKw6ip}Cuckoo bird, cuckoo bird, tell me no lies. 'I snuck into another bird's tree'.{newline}Cuckoo bird, cuckoo bird, say something wise. 'There's a king in Battania who's just like me.'", (Dictionary<string, object>)null));
				list.Add(new TextObject("{=2WI066bI}Nimr Nimr he was killed. Died in the cage of the Banu Qild.{newline}All the ladies cry their grief. All the husbands sigh in relief.", (Dictionary<string, object>)null));
				list.Add(new TextObject("{=Jc8OiSZg}Olek Olek fought a bear. Took it down, fair and square.{newline}Not with his sword and not with his spear. But with his breath of garlic and beer.", (Dictionary<string, object>)null));
				list.Add(new TextObject("{=TCIaDC9X}The foes of Vlandia came a-plundering. King Derthert summoned his men to the keep!{newline}They rode to his muster with their hooves a-thundering. But Derthert had already gone to sleep.", (Dictionary<string, object>)null));
			}
			else if (conversation_beggar_start_on_condition())
			{
				list = GetBeggarStories();
			}
			else if (GetPossibleIssueRumors().Count > 0)
			{
				list = GetPossibleIssueRumors();
			}
			else
			{
				GetPossibleRumors(list);
			}
			for (int i = 0; i < list.Count; i++)
			{
				TextObject val = list[MBRandom.RandomInt(list.Count)];
				string value = RumorIdentifier(val);
				if (!_rumorsGiven.ContainsValue(value))
				{
					MBTextManager.SetTextVariable("CONVERSATION_SCRAP", val, false);
					_rumorsGiven.Add(key, value);
					return true;
				}
			}
		}
		return false;
	}

	private string RumorIdentifier(TextObject conversationScrap)
	{
		string text = ((object)conversationScrap.CopyTextObject()).ToString();
		return text.Substring(0, (text.Length < 12) ? text.Length : 12);
	}

	private List<TextObject> GetPossibleIssueRumors()
	{
		List<TextObject> list = new List<TextObject>();
		foreach (Hero item in (List<Hero>)(object)Settlement.CurrentSettlement.Notables)
		{
			IssueBase issue = item.Issue;
			if (issue != null)
			{
				TextObject issueAsRumorInSettlement = issue.IssueAsRumorInSettlement;
				if (!issueAsRumorInSettlement.IsEmpty() && !_rumorsGiven.ContainsValue(RumorIdentifier(issueAsRumorInSettlement)))
				{
					list.Add(issueAsRumorInSettlement);
				}
			}
		}
		return list;
	}

	private List<TextObject> GetBeggarStories()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Invalid comparison between Unknown and I4
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Expected O, but got Unknown
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Expected O, but got Unknown
		List<TextObject> list = new List<TextObject>();
		list.Add(new TextObject("{=EiiHoraV}Hard times... Hard times indeed.", (Dictionary<string, object>)null));
		CharacterObject val = null;
		CharacterObject val2 = null;
		foreach (Village item in (List<Village>)(object)Settlement.CurrentSettlement.BoundVillages)
		{
			foreach (Hero item2 in (List<Hero>)(object)((SettlementComponent)item).Settlement.Notables)
			{
				if ((int)item2.Occupation == 22 && item2.GetTraitLevel(DefaultTraits.Mercy) < 0 && item2.GetTraitLevel(DefaultTraits.Generosity) <= 0)
				{
					val = item2.CharacterObject;
				}
			}
		}
		foreach (Hero item3 in (List<Hero>)(object)Settlement.CurrentSettlement.Notables)
		{
			if ((int)item3.Occupation == 18 && item3.GetTraitLevel(DefaultTraits.Mercy) < 0 && item3.GetTraitLevel(DefaultTraits.Generosity) <= 0)
			{
				val2 = item3.CharacterObject;
			}
		}
		if (((BasicCharacterObject)CharacterObject.OneToOneConversationCharacter).IsFemale)
		{
			list.Add(new TextObject("{=ZVbQemrz}My husband was wounded in the wars, and now can't pull a plow. So he lost his tenancy, and now I must beg in the marketplace.", (Dictionary<string, object>)null));
			list.Add(new TextObject("{=olKsdJmv}I married bad. That was my misfortune. Drinks his wages and beats me when I say a word, so now I must beg for bread for our children. ", (Dictionary<string, object>)null));
			list.Add(new TextObject("{=3kHmtbVZ}What my man did was wrong, there's no denying. But they hanged him, and now what am I to do? Do his children bear his guilt?", (Dictionary<string, object>)null));
			list.Add(new TextObject("{=agL4RTbB}My man and I, we never wed proper. So he made a bit of money in the wars and wanted to marry a rich man's daughter. 'Cast her out,' she told him, and that's what he did.", (Dictionary<string, object>)null));
			list.Add(new TextObject("{=k18dhQA1}The plague took my parents and my uncle took their land. But I'd rather beg than be a servant in a rich man's home!", (Dictionary<string, object>)null));
		}
		else
		{
			list.Add(new TextObject("{=oaFFW2bo}The demons come at night and taunt me until dawn. What am I to do?", (Dictionary<string, object>)null));
			list.Add(new TextObject("{=b3MBZKuQ}We came here looking for work, as there was none in the village. But the masters want skilled hands only. With a few coins, I could go home.", (Dictionary<string, object>)null));
			list.Add(new TextObject("{=mrtdsccq}My own people chased me from the village. It weren't true, what they said about me. Coveted my land, I'll warrant.", (Dictionary<string, object>)null));
			list.Add(new TextObject("{=GKl8U04i}Lamed by an arrow in the leg, and now I can't work a field and I'm no use to anyone, they say.", (Dictionary<string, object>)null));
			if (val2 != null)
			{
				TextObject val3 = new TextObject("{=yj0VZ8IZ}I lost a mule loaded with wares when it slid into the river, and {HARSH_MERCHANT.NAME} said he'd take it from my wages for the next year, didn't care if I starved. If I didn't like it then I could go beg on the street, he said to me. So look at me. I stood up to him and look at me now.", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val3, "HARSH_MERCHANT", val2, false);
				list.Add(val3);
			}
			if (val != null)
			{
				TextObject val4 = new TextObject("{=SGoz7xvk}I tilled the land of {CRUEL_LANDOWNER.NAME}, and paid my rent, and earned him a pretty penny. But the times changed, the prices changed, he took my tenancy from me and said he'd raise sheep there instead. Where was I to go? What I am to do?", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val4, "CRUEL_LANDOWNER", val, false);
				list.Add(val4);
			}
		}
		return list;
	}

	private void GetPossibleRumors(List<TextObject> conversationScraps)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Invalid comparison between Unknown and I4
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Invalid comparison between Unknown and I4
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Invalid comparison between Unknown and I4
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Invalid comparison between Unknown and I4
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Invalid comparison between Unknown and I4
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Invalid comparison between Unknown and I4
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Invalid comparison between Unknown and I4
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Invalid comparison between Unknown and I4
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Invalid comparison between Unknown and I4
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Expected O, but got Unknown
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Invalid comparison between Unknown and I4
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Invalid comparison between Unknown and I4
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Expected O, but got Unknown
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Expected O, but got Unknown
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Expected O, but got Unknown
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Expected O, but got Unknown
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Expected O, but got Unknown
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Invalid comparison between Unknown and I4
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Invalid comparison between Unknown and I4
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Expected O, but got Unknown
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Invalid comparison between Unknown and I4
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Expected O, but got Unknown
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Expected O, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Expected O, but got Unknown
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Expected O, but got Unknown
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Expected O, but got Unknown
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Invalid comparison between Unknown and I4
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Expected O, but got Unknown
		//IL_0658: Unknown result type (might be due to invalid IL or missing references)
		//IL_065b: Invalid comparison between Unknown and I4
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Expected O, but got Unknown
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_066a: Expected O, but got Unknown
		//IL_052a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Expected O, but got Unknown
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Expected O, but got Unknown
		//IL_06b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06be: Expected O, but got Unknown
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Expected O, but got Unknown
		//IL_070b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Expected O, but got Unknown
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Expected O, but got Unknown
		//IL_07d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d7: Invalid comparison between Unknown and I4
		//IL_079e: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a1: Invalid comparison between Unknown and I4
		//IL_075f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0766: Expected O, but got Unknown
		//IL_07df: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e6: Expected O, but got Unknown
		//IL_07a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b0: Expected O, but got Unknown
		//IL_0851: Unknown result type (might be due to invalid IL or missing references)
		//IL_085b: Expected O, but got Unknown
		//IL_081a: Unknown result type (might be due to invalid IL or missing references)
		//IL_081d: Invalid comparison between Unknown and I4
		//IL_0825: Unknown result type (might be due to invalid IL or missing references)
		//IL_082c: Expected O, but got Unknown
		//IL_0881: Unknown result type (might be due to invalid IL or missing references)
		//IL_0886: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ab: Expected O, but got Unknown
		//IL_08b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bc: Invalid comparison between Unknown and I4
		//IL_08ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f1: Expected O, but got Unknown
		//IL_0976: Unknown result type (might be due to invalid IL or missing references)
		//IL_097d: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f3: Expected O, but got Unknown
		//IL_0a51: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a58: Expected O, but got Unknown
		//IL_0b45: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b4c: Expected O, but got Unknown
		//IL_0ba5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bac: Expected O, but got Unknown
		//IL_0c5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c61: Expected O, but got Unknown
		//IL_0c7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c85: Expected O, but got Unknown
		//IL_0b6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b76: Expected O, but got Unknown
		//IL_0cdb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce2: Expected O, but got Unknown
		//IL_0b87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8e: Expected O, but got Unknown
		//IL_0d49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d50: Expected O, but got Unknown
		//IL_0b96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9d: Expected O, but got Unknown
		//IL_0d93: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d9a: Expected O, but got Unknown
		//IL_1076: Unknown result type (might be due to invalid IL or missing references)
		//IL_1079: Invalid comparison between Unknown and I4
		//IL_10b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ba: Invalid comparison between Unknown and I4
		//IL_1087: Unknown result type (might be due to invalid IL or missing references)
		//IL_108e: Expected O, but got Unknown
		//IL_1112: Unknown result type (might be due to invalid IL or missing references)
		//IL_1119: Expected O, but got Unknown
		//IL_10c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_10cf: Expected O, but got Unknown
		//IL_1276: Unknown result type (might be due to invalid IL or missing references)
		//IL_127d: Expected O, but got Unknown
		//IL_1286: Unknown result type (might be due to invalid IL or missing references)
		//IL_1290: Expected O, but got Unknown
		//IL_1148: Unknown result type (might be due to invalid IL or missing references)
		//IL_114b: Invalid comparison between Unknown and I4
		//IL_12a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a7: Invalid comparison between Unknown and I4
		//IL_0e91: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e98: Expected O, but got Unknown
		//IL_0e48: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e4f: Expected O, but got Unknown
		//IL_1153: Unknown result type (might be due to invalid IL or missing references)
		//IL_115a: Expected O, but got Unknown
		//IL_12af: Unknown result type (might be due to invalid IL or missing references)
		//IL_12b6: Expected O, but got Unknown
		//IL_12d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_12dc: Expected O, but got Unknown
		//IL_1455: Unknown result type (might be due to invalid IL or missing references)
		//IL_145c: Expected O, but got Unknown
		//IL_1405: Unknown result type (might be due to invalid IL or missing references)
		//IL_140c: Expected O, but got Unknown
		//IL_0f2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f31: Expected O, but got Unknown
		//IL_0ed9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0edc: Invalid comparison between Unknown and I4
		//IL_119f: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a6: Expected O, but got Unknown
		//IL_14e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_14e8: Expected O, but got Unknown
		//IL_14a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_14ac: Expected O, but got Unknown
		//IL_1306: Unknown result type (might be due to invalid IL or missing references)
		//IL_130d: Expected O, but got Unknown
		//IL_0f6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f70: Invalid comparison between Unknown and I4
		//IL_0ee4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eeb: Expected O, but got Unknown
		//IL_152f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1536: Expected O, but got Unknown
		//IL_0fc1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fc8: Expected O, but got Unknown
		//IL_0f78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7f: Expected O, but got Unknown
		//IL_1229: Unknown result type (might be due to invalid IL or missing references)
		//IL_1230: Expected O, but got Unknown
		//IL_11eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_11f2: Expected O, but got Unknown
		//IL_1390: Unknown result type (might be due to invalid IL or missing references)
		//IL_1397: Expected O, but got Unknown
		//IL_1352: Unknown result type (might be due to invalid IL or missing references)
		//IL_1359: Expected O, but got Unknown
		//IL_1001: Unknown result type (might be due to invalid IL or missing references)
		//IL_1004: Invalid comparison between Unknown and I4
		//IL_15df: Unknown result type (might be due to invalid IL or missing references)
		//IL_15e6: Expected O, but got Unknown
		//IL_157f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1586: Expected O, but got Unknown
		//IL_100c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1013: Expected O, but got Unknown
		List<string> list = new List<string>();
		list.Add("{=8XM8CHIm}brother-in-law");
		list.Add("{=VDfyzs5v}nephew");
		list.Add("{=NPuHiggC}cousin");
		list.Add("{=Mf3vLIQp}uncle");
		CampaignTime val = CampaignTime.Now;
		int num = ((CampaignTime)(ref val)).GetDayOfYear / CampaignTime.DaysInSeason;
		Town town = Settlement.CurrentSettlement.Town;
		float num2 = ((town != null) ? town.Loyalty : 50f);
		bool flag = false;
		Location location = CampaignMission.Current.Location;
		Location locationWithId = LocationComplex.Current.GetLocationWithId("tavern");
		if (Settlement.CurrentSettlement != null && locationWithId == location)
		{
			flag = true;
		}
		if (flag)
		{
			if (num == 0)
			{
				conversationScraps.Add(new TextObject("{=sm6ckPnp}It's springtime, my friend. The season of the winds. The season of madness. Now then, I believe I'll have another drink.", (Dictionary<string, object>)null));
			}
			if (num == 1 && !((BasicCharacterObject)CharacterObject.OneToOneConversationCharacter).IsFemale)
			{
				conversationScraps.Add(new TextObject("{=se2EXQu8}When the heat's this bad, a fellow can build up quite a thirst.", (Dictionary<string, object>)null));
			}
			if (num == 3)
			{
				conversationScraps.Add(new TextObject("{=kDItfaaN}I'll just have one more to keep me warm on the way home.", (Dictionary<string, object>)null));
			}
			if (((MBObjectBase)Settlement.CurrentSettlement.Culture).StringId == "empire")
			{
				conversationScraps.Add(new TextObject("{=5EXL1MiE}Sometimes I feel like running off to join those hermits over on Mount Erithrys and putting my worries behind me. Even better, maybe some of these great lords would go there. They can retire and live out their days in the sun, not worrying about being beheaded or betrayed, and they wouldn't need to tax us any. Be better for everyone.", (Dictionary<string, object>)null));
			}
			conversationScraps.Add(new TextObject("{=bFjtk0Op}Well.. Sometimes when you learn a skill, you pick up a certain way of doing it that can help you in some circumstances and hurt you in others. If you wanted to retrain yourself, your companions, or members of your clan to do things a different way, you might try the tournament master at the arena. They'll help you - for a price, of course.", (Dictionary<string, object>)null));
		}
		conversationScraps.Add(new TextObject("{=U79CXBbj}Heaven watch over us. Heaven give us strength.", (Dictionary<string, object>)null));
		ProsperityLevel prosperityLevel = Settlement.CurrentSettlement.SettlementComponent.GetProsperityLevel();
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 6)
		{
			if ((int)prosperityLevel > 1)
			{
				conversationScraps.Add(new TextObject("{=KeIGtpxb}Just glad to have full bellies this year in my household. Can't say that for every year.", (Dictionary<string, object>)null));
			}
			if (num == 0 && (int)prosperityLevel < 1)
			{
				conversationScraps.Add(new TextObject("{=dlOCK6Ro}After that winter, the cow's too thin to pull the plough. Don't know where I'll get the money to rent one.[rf:convo_grave]", (Dictionary<string, object>)null));
			}
			if (num == 1 && (int)prosperityLevel < 1)
			{
				conversationScraps.Add(new TextObject("{=usMQ64pf}Hope there's enough hands for the harvest.", (Dictionary<string, object>)null));
			}
			if (num == 2 && (int)prosperityLevel < 1)
			{
				conversationScraps.Add(new TextObject("{=46Q5MQHK}It was a thin harvest. It will be a lean winter and a cruel spring. You mark my words.[rf:convo_grave]", (Dictionary<string, object>)null));
			}
			if ((int)prosperityLevel < 1)
			{
				conversationScraps.Add(new TextObject("{=MuzS4Et1}Heaven help us. If I can't pay the rent, I'll end up on the side of the road with the landless, hoping for a day's work.[rf:confused_annoyed]", (Dictionary<string, object>)null));
			}
			if (num == 0)
			{
				conversationScraps.Add(new TextObject("{=8bnlS0IV}Ploughing, sowing... Got many weeks of that ahead of us still.", (Dictionary<string, object>)null));
			}
			if (num == 1)
			{
				conversationScraps.Add(new TextObject("{=CSqHz4EG}It's almost harvesting season.", (Dictionary<string, object>)null));
			}
			if (num == 2)
			{
				conversationScraps.Add(new TextObject("{=FNLtDFbn}Time to butcher and salt our meat for the winter.", (Dictionary<string, object>)null));
			}
			if (num == 3)
			{
				conversationScraps.Add(new TextObject("{=TDgoyFBi}Not much work to be done... Sewing and mending and what.", (Dictionary<string, object>)null));
			}
		}
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 8)
		{
			if ((int)prosperityLevel <= 0)
			{
				conversationScraps.Add(new TextObject("{=Emw9ut4A}Hard times... Can't find good help these days. Half of 'em are too hungry to work. The other half are thieves. Smart ones all left weeks ago.[rf:confused_annoyed]", (Dictionary<string, object>)null));
			}
			if ((int)prosperityLevel > 1 && num2 < 50f)
			{
				conversationScraps.Add(new TextObject("{=mimt3Qph}Every day... More people in the markets... More people at the well... Where do they all come from?", (Dictionary<string, object>)null));
			}
			if ((int)prosperityLevel > 1 && num2 < 60f)
			{
				conversationScraps.Add(new TextObject("{=RUZKiW0u}Business is good. Can't deny that. But prices are high and getting higher.", (Dictionary<string, object>)null));
			}
			if ((int)prosperityLevel < 0)
			{
				conversationScraps.Add(new TextObject("{=LKGivHYp}The pittance they call a wage these days... I don't want to leave my kin, but I'm thinking I might have to try my luck in some other town.", (Dictionary<string, object>)null));
			}
			if (((MBObjectBase)Settlement.CurrentSettlement.Culture).StringId == "aserai")
			{
				if (num == 1 && (int)prosperityLevel <= 0)
				{
					conversationScraps.Add(new TextObject("{=3zZ8ZIca}Damn the summer. Can't sleep at night for the heat. And now they say the well's running low...", (Dictionary<string, object>)null));
				}
			}
			else
			{
				if (num == 3 && (int)prosperityLevel <= 0)
				{
					conversationScraps.Add(new TextObject("{=Tol5zRgG}This winter... Kids are all coughing and wheezing. My old ma's been bed-bound for a month. Can't afford coals for the stove at night. Spring thaw can't come fast enough for me.", (Dictionary<string, object>)null));
				}
				if (num == 2 && (int)prosperityLevel <= 0)
				{
					conversationScraps.Add(new TextObject("{=ZVItXrnG}It's a lean autumn, this one. Heaven help us lay in enough wood before the winter.", (Dictionary<string, object>)null));
				}
			}
		}
		Settlement val2 = ((!Settlement.CurrentSettlement.IsTown && !Settlement.CurrentSettlement.IsCastle) ? Settlement.CurrentSettlement.Village.Bound : Settlement.CurrentSettlement);
		Town town2 = val2.Town;
		if ((town2 != null && town2.Loyalty < 20f) || val2.Town.InRebelliousState)
		{
			conversationScraps.Add(new TextObject("{=ftyp2ul1}Those troublemakers - they're not from around here. I can't believe it's anyone from around here. You hear people talking foreign more and more these days - I'm sure it's them.", (Dictionary<string, object>)null));
			conversationScraps.Add(new TextObject("{=J1TykdEn}I hear there have been secret meetings in the woods. Solemn oaths, signed in blood. A great wind is going to blow soon - you mark my words.", (Dictionary<string, object>)null));
		}
		Town town3 = Settlement.CurrentSettlement.Town;
		if (town3 == null && Settlement.CurrentSettlement.IsVillage)
		{
			town3 = ((Settlement.CurrentSettlement.Village.TradeBound != null) ? Settlement.CurrentSettlement.Village.TradeBound.Town : Settlement.CurrentSettlement.Village.Bound.Town);
		}
		IOrderedEnumerable<ItemRosterElement> orderedEnumerable = ((IEnumerable<ItemRosterElement>)Settlement.CurrentSettlement.ItemRoster).OrderBy(delegate(ItemRosterElement x)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Town obj = town3;
			EquipmentElement equipmentElement2 = ((ItemRosterElement)(ref x)).EquipmentElement;
			return obj.GetItemCategoryPriceIndex(((EquipmentElement)(ref equipmentElement2)).Item.ItemCategory);
		});
		List<Town> list2 = (from settlement in (IEnumerable<Settlement>)Campaign.Current.Settlements
			where settlement.IsTown && settlement != Settlement.CurrentSettlement && Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, Settlement.CurrentSettlement, false, false, (NavigationType)3) < TradeRumorDistance
			select settlement.Town).ToList();
		int num3 = 0;
		foreach (ItemRosterElement item3 in orderedEnumerable)
		{
			ItemRosterElement current = item3;
			EquipmentElement equipmentElement = ((ItemRosterElement)(ref current)).EquipmentElement;
			ItemObject targetItem = ((EquipmentElement)(ref equipmentElement)).Item;
			if (!targetItem.IsTradeGood)
			{
				continue;
			}
			int price = town3.MarketData.GetPrice(targetItem, (MobileParty)null, false, (PartyBase)null);
			if (list2.Count <= 0)
			{
				continue;
			}
			Town val3 = Extensions.MaxBy<Town, int>((IEnumerable<Town>)list2, (Func<Town, int>)((Town t) => ((SettlementComponent)t).GetItemPrice(targetItem, (MobileParty)null, false)));
			int num4 = ((SettlementComponent)val3).GetItemPrice(new EquipmentElement(targetItem, (ItemModifier)null, (ItemObject)null, false), (MobileParty)null, true) - price;
			if (num4 > 1)
			{
				num3++;
				if (num3 > 4)
				{
					break;
				}
				TextObject val4 = (TextObject)(num3 switch
				{
					1 => (object)new TextObject("{=8NDuezga}Now, I was talking to my {RANDOM_RELATIVE}, and he tells me he bought some {._}{PLURAL(ITEM_NAME)} around here for cheap and took it to {TOWN_NAME}. Said he made {PRICE_DIFF}{GOLD_ICON} of profit on each one.", (Dictionary<string, object>)null), 
					2 => (object)new TextObject("{=ubnVca4L}So, yeah... My {RANDOM_RELATIVE} says he took some {._}{PLURAL(ITEM_NAME)} to {TOWN_NAME} and made {PRICE_DIFF}{GOLD_ICON} in profit. Course he might be talking out of his arse.", (Dictionary<string, object>)null), 
					3 => (object)new TextObject("{=4Slck7UB}So, I heard from my {RANDOM_RELATIVE} that he took some {._}{PLURAL(ITEM_NAME)} over to {TOWN_NAME}, and made a profit of {PRICE_DIFF}{GOLD_ICON}.", (Dictionary<string, object>)null), 
					4 => (object)new TextObject("{=rTrGXB1o}Yeah.. Word is that {._}{ITEM_NAME} sells well in {TOWN_NAME}. A profit of {PRICE_DIFF}{GOLD_ICON} a load, they say.", (Dictionary<string, object>)null), 
					_ => null, 
				});
				val4.SetTextVariable("RANDOM_RELATIVE", new TextObject(list[MBRandom.RandomInt(0, list.Count)], (Dictionary<string, object>)null));
				val4.SetTextVariable("ITEM_NAME", targetItem.Name);
				val4.SetTextVariable("TOWN_NAME", ((SettlementComponent)val3).Name);
				val4.SetTextVariable("PRICE_DIFF", num4);
				conversationScraps.Add(val4);
			}
		}
		Hero leader = Settlement.CurrentSettlement.OwnerClan.Leader;
		Hero leader2 = Settlement.CurrentSettlement.MapFaction.Leader;
		if (leader.GetTraitLevel(DefaultTraits.Honor) > 0 && (int)prosperityLevel >= 0)
		{
			TextObject val5 = new TextObject("{=ztiax0Sn}Say what you will about {?OWNER.GENDER}lady{?}lord{\\?} {OWNER.LINK}... {?OWNER.GENDER}She{?}He{\\?}'ll give the lowest wretch in the realm a fair hearing in {?OWNER.GENDER}her{?}his{\\?} court. Can't deny that.", (Dictionary<string, object>)null);
			HeroHelper.SetPropertiesToTextObject(leader, val5, "OWNER");
			conversationScraps.Add(val5);
		}
		if (leader.GetTraitLevel(DefaultTraits.Mercy) < 0 && leader.GetTraitLevel(DefaultTraits.Generosity) < 0 && (int)prosperityLevel <= 1)
		{
			TextObject val6 = new TextObject("{=mfVJBFUD}The tax on this, the tax on that... Now just what do we think our beloved {?OWNER.GENDER}lady{?}lord{\\?} {OWNER.LINK} does with it all?", (Dictionary<string, object>)null);
			HeroHelper.SetPropertiesToTextObject(leader, val6, "OWNER");
			conversationScraps.Add(val6);
		}
		if (leader.GetTraitLevel(DefaultTraits.Mercy) > 0 && Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.Town.Security < 40f)
		{
			TextObject val7 = new TextObject("{=ZO5RaXMW}Spare us from {?OWNER.GENDER}ladies{?}lords{\\?} like {OWNER.LINK}, whose hearts are too tender to punish the thieves and rogues who terrorize honest folk.", (Dictionary<string, object>)null);
			HeroHelper.SetPropertiesToTextObject(leader, val7, "OWNER");
			conversationScraps.Add(val7);
		}
		if (leader.GetTraitLevel(DefaultTraits.Honor) < 0 && Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.Town.Security < 40f)
		{
			TextObject val8 = new TextObject("{=bUweVtkk}Why doesn't {OWNER.LINK} do anything about the thieves plying their trade out on the street? Could it be that {?OWNER.GENDER}she{?}he{\\?} is getting a cut of their take?", (Dictionary<string, object>)null);
			HeroHelper.SetPropertiesToTextObject(leader, val8, "OWNER");
			conversationScraps.Add(val8);
		}
		if (Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.Town.Security < 40f && leader.GetTraitLevel(DefaultTraits.Honor) > 0)
		{
			TextObject val9 = new TextObject("{=OwnoJUkn}They say that {OWNER.LINK} is an honest {?OWNER.GENDER}woman{?}man{\\?}. But even {?OWNER.GENDER}she{?}he{\\?} can't stop all the thieving and corruption around here.", (Dictionary<string, object>)null);
			HeroHelper.SetPropertiesToTextObject(leader, val9, "OWNER");
			conversationScraps.Add(val9);
		}
		if (Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.Town.Security < 40f && (int)prosperityLevel >= 2)
		{
			TextObject item = new TextObject("{=wqzyHirl}Look all around you. People making money hand over fist, draped in silks, smelling of wine and perfume. But is it the honest folk? I think not.", (Dictionary<string, object>)null);
			conversationScraps.Add(item);
		}
		if (leader.GetTraitLevel(DefaultTraits.Mercy) + leader.GetTraitLevel(DefaultTraits.Generosity) < 0 && (int)prosperityLevel >= 1)
		{
			TextObject val10 = new TextObject("{=7uFFaSHv}Now some people say our {?OWNER.GENDER}lady{?}lord{\\?} {OWNER.LINK} is as mean as a scalded cat. But I say most people are villains, and it's good they feel a little fear. Anyway, there's meat on the table these days, and who can argue with that?", (Dictionary<string, object>)null);
			HeroHelper.SetPropertiesToTextObject(leader, val10, "OWNER");
			conversationScraps.Add(val10);
		}
		if (leader2.GetTraitLevel(DefaultTraits.Honor) < 0 && leader2.GetTraitLevel(DefaultTraits.Calculating) < 0 && (int)prosperityLevel <= 1)
		{
			TextObject val11 = new TextObject("{=nMFY2Pb0}We are doomed I say, doomed. A ruler who cannot control {?RULER.GENDER}her{?}his{\\?} passions brings ruin on the realm.", (Dictionary<string, object>)null);
			HeroHelper.SetPropertiesToTextObject(leader2, val11, "RULER");
			conversationScraps.Add(val11);
		}
		if (num2 < 40f)
		{
			conversationScraps.Add(new TextObject("{=RyAH9Zvk}Now the man in the market said, 'All this trouble and woe means the Heavens aren't pleased with the one on the throne.' 'That's treason talk!' I told him. Of course I did.", (Dictionary<string, object>)null));
		}
		TextObject item2 = default(TextObject);
		foreach (LogEntry item4 in (List<LogEntry>)(object)Campaign.Current.LogEntryHistory.GameActionLogs)
		{
			val = item4.GameTime;
			if (!(((CampaignTime)(ref val)).ElapsedDaysUntilNow < 60f))
			{
				continue;
			}
			if (item4 is VillageStateChangedLogEntry)
			{
				VillageStateChangedLogEntry val12 = (VillageStateChangedLogEntry)item4;
				Village village = val12.Village;
				if ((int)val12.NewState == 1 && ((SettlementComponent)village).Settlement.MapFaction == Settlement.CurrentSettlement.MapFaction && ((SettlementComponent)village).Settlement != Settlement.CurrentSettlement)
				{
					TextObject val13 = new TextObject("{=JQuCuaVv}I've heard that {ENEMY_NAME} have been raiding across the border... People around here better wake up.", (Dictionary<string, object>)null);
					val13.SetTextVariable("ENEMY_NAME", FactionHelper.GetTermUsedByOtherFaction(val12.RaiderPartyMapFaction, Settlement.CurrentSettlement.MapFaction, false));
					conversationScraps.Add(val13);
				}
			}
			if (item4.GetAsRumor(Settlement.CurrentSettlement, ref item2) > 0)
			{
				conversationScraps.Add(item2);
			}
		}
		int num5 = 0;
		foreach (MobileParty item5 in (List<MobileParty>)(object)MobileParty.AllBanditParties)
		{
			NavigationType val14 = (NavigationType)((!item5.IsCurrentlyAtSea) ? 1 : 2);
			if (DistanceHelper.FindClosestDistanceFromMobilePartyToSettlement(item5, Settlement.CurrentSettlement, val14) < Campaign.Current.Models.EncounterModel.GetEncounterJoiningRadius * 5f)
			{
				num5++;
			}
		}
		if (num5 > 1 && Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.Town.Security < 50f)
		{
			TextObject val15 = new TextObject("{=NPhnQgQS}There's bandits lurking just beyond the walls these days. What about the taxes we pay, I ask you? Why aren't {?OWNER.GENDER}Lady{?}Lord{\\?} {OWNER.LINK}'s men doing their jobs?", (Dictionary<string, object>)null);
			HeroHelper.SetPropertiesToTextObject(leader, val15, "OWNER");
			conversationScraps.Add(val15);
		}
		if (num5 > 1 && Settlement.CurrentSettlement.IsVillage && Settlement.CurrentSettlement.Village.TradeBound != null && Settlement.CurrentSettlement.Village.TradeBound.Town.Security < 50f)
		{
			TextObject val16 = new TextObject("{=64p9ULVu}There's bandits lurking just beyond the outermost fields, I hear. What about the taxes we pay, I ask you? Why aren't {?OWNER.GENDER}Lady{?}Lord{\\?} {OWNER.LINK}'s men doing their jobs?", (Dictionary<string, object>)null);
			HeroHelper.SetPropertiesToTextObject(leader, val16, "OWNER");
			conversationScraps.Add(val16);
		}
		List<Hero> list3 = ((IEnumerable<Hero>)Settlement.CurrentSettlement.HeroesWithoutParty).ToList();
		list3.Add(Settlement.CurrentSettlement.OwnerClan.Leader);
		foreach (Hero character in list3)
		{
			int traitLevel = character.GetTraitLevel(DefaultTraits.Mercy);
			int traitLevel2 = character.GetTraitLevel(DefaultTraits.Valor);
			int traitLevel3 = character.GetTraitLevel(DefaultTraits.Generosity);
			int traitLevel4 = character.GetTraitLevel(DefaultTraits.Calculating);
			int traitLevel5 = character.GetTraitLevel(DefaultTraits.Honor);
			if (MathF.Abs(traitLevel) + MathF.Abs(traitLevel2) + MathF.Abs(traitLevel3) + MathF.Abs(traitLevel5) + MathF.Abs(traitLevel4) >= 2)
			{
				TextObject val17 = new TextObject("{=Rvt11UBE}{BASIC_EVAL} {REPUTATION}", (Dictionary<string, object>)null);
				int num6 = traitLevel + traitLevel5 + traitLevel3;
				TextObject val18 = ((!character.IsLord) ? new TextObject("{=CkaSSJz1}{NOTABLE.LINK}...", (Dictionary<string, object>)null) : ((num6 > 0) ? new TextObject("{=9Box0Yj7}The {?NOTABLE.GENDER}lady{?}lord{\\?} {NOTABLE.LINK}... We're blessed.", (Dictionary<string, object>)null) : ((num6 != 0 && traitLevel < 0) ? new TextObject("{=aEL2S1PZ}Heavens protect us from the {?NOTABLE.GENDER}lady{?}lord{\\?} {NOTABLE.LINK}...", (Dictionary<string, object>)null) : new TextObject("{=DgxVa0OP}The {?NOTABLE.GENDER}lady{?}lord{\\?} {NOTABLE.LINK}...", (Dictionary<string, object>)null))));
				HeroHelper.SetPropertiesToTextObject(character, val18, "NOTABLE");
				HeroHelper.SetPropertiesToTextObject(character, val17, "NOTABLE");
				val17.SetTextVariable("BASIC_EVAL", val18);
				TextObject val19 = Campaign.Current.ConversationManager.FindMatchingTextOrNull("informal_reputation", character.CharacterObject);
				HeroHelper.SetPropertiesToTextObject(character, val19, "NOTABLE");
				val17.SetTextVariable("REPUTATION", ((object)val19).ToString());
				conversationScraps.Add(val17);
			}
			if (character.IsGangLeader && character.GetTraitLevel(DefaultTraits.Mercy) < 0)
			{
				TextObject val20 = new TextObject("{=Xqx0uZva}I told that silly {RANDOM_RELATIVE} of mine. I told him. I said, 'You take money from {NOTABLE.LINK}, {?NOTABLE.GENDER}she{?}he{\\?}'ll want back double. And if you value the bones in your hands, you'll pay.' I told him, I did.", (Dictionary<string, object>)null);
				val20.SetTextVariable("RANDOM_RELATIVE", new TextObject(list[MBRandom.RandomInt(0, list.Count)], (Dictionary<string, object>)null));
				HeroHelper.SetPropertiesToTextObject(character, val20, "NOTABLE");
				conversationScraps.Add(val20);
			}
			if (character.IsGangLeader && character.GetTraitLevel(DefaultTraits.Generosity) - character.GetTraitLevel(DefaultTraits.Mercy) >= 0)
			{
				TextObject val21 = new TextObject("{=PJ7BhyOy}I said to that good-for-nothing... I said to him, 'You leave my daughter alone, or {NOTABLE.LINK} will hear about it.' Hah! I don't see him tomcatting around outside my home any more, that's for sure.", (Dictionary<string, object>)null);
				HeroHelper.SetPropertiesToTextObject(character, val21, "NOTABLE");
				conversationScraps.Add(val21);
			}
			if (character.IsGangLeader && character.GetTraitLevel(DefaultTraits.Honor) + character.GetTraitLevel(DefaultTraits.Generosity) + character.GetTraitLevel(DefaultTraits.Mercy) < -1)
			{
				TextObject val22 = new TextObject("{=ZhCRL9mY}All those bastards walking around drunk and bothering folks.. But they work for {NOTABLE.LINK} and you can't say a thing if you value your head.", (Dictionary<string, object>)null);
				HeroHelper.SetPropertiesToTextObject(character, val22, "NOTABLE");
				conversationScraps.Add(val22);
			}
			if (character.IsGangLeader && character.GetTraitLevel(DefaultTraits.Mercy) >= 0)
			{
				TextObject val23 = new TextObject("{=GeJuBar0}Now, some will tell you that {NOTABLE.LINK} is no better than a common thief who deserves the gallows, but my cousin says {NOTABLE.FIRSTNAME} did him a good turn. So I don't know what to believe...", (Dictionary<string, object>)null);
				HeroHelper.SetPropertiesToTextObject(character, val23, "NOTABLE");
				conversationScraps.Add(val23);
			}
			if (character.IsMerchant && Settlement.CurrentSettlement.IsTown)
			{
				List<Workshop> list4 = (from x in Settlement.CurrentSettlement.Town.Workshops
					where ((SettlementArea)x).Owner == character
					orderby x.ProfitMade descending
					select x).ToList();
				if (list4.Count > 0)
				{
					Workshop val24 = list4[0];
					if (traitLevel3 > 0 && traitLevel < 0)
					{
						TextObject val25 = new TextObject("{=xP8cKZFE}They say the merchant {NOTABLE.LINK} is hiring at {?NOTABLE.GENDER}her{?}his{\\?} {.%}{SHOP_TYPE}{.%}. {?NOTABLE.GENDER}She{?}He{\\?}'s a harsh master but a fair one, they say.", (Dictionary<string, object>)null);
						val25.SetTextVariable("SHOP_TYPE", val24.WorkshopType.Name);
						HeroHelper.SetPropertiesToTextObject(character, val25, "NOTABLE");
						conversationScraps.Add(val25);
					}
					if (traitLevel3 + traitLevel > 0)
					{
						TextObject val26 = new TextObject("{=9O8a1Yz8}I heard in the market that {NOTABLE.LINK} is hiring at {?NOTABLE.GENDER}her{?}his{\\?} {.%}{SHOP_TYPE}{.%}. Treats {?NOTABLE.GENDER}her{?}his{\\?} workers well, they say.", (Dictionary<string, object>)null);
						val26.SetTextVariable("SHOP_TYPE", val24.WorkshopType.Name);
						HeroHelper.SetPropertiesToTextObject(character, val26, "NOTABLE");
						conversationScraps.Add(val26);
					}
					if (traitLevel5 < 0 && traitLevel + traitLevel3 < 0 && (int)prosperityLevel <= 0)
					{
						TextObject val27 = new TextObject("{=8N3cs42a}So... The word is that {NOTABLE.LINK} is looking to cut costs at {?NOTABLE.GENDER}her{?}his{\\?} {.%}{SHOP_TYPE}{.%}. They say {?NOTABLE.GENDER}she{?}he{\\?}'s been docking the men's wages right and left.", (Dictionary<string, object>)null);
						val27.SetTextVariable("SHOP_TYPE", val24.WorkshopType.Name);
						HeroHelper.SetPropertiesToTextObject(character, val27, "NOTABLE");
						conversationScraps.Add(val27);
					}
					if (traitLevel5 < 0)
					{
						TextObject val28 = new TextObject("{=e9bLGCQu}They say {NOTABLE.LINK} is hiring at {?NOTABLE.GENDER}her{?}his{\\?} {.%}{SHOP_TYPE}{.%}. {?NOTABLE.GENDER}She{?}He{\\?}'s a slippery one.", (Dictionary<string, object>)null);
						val28.SetTextVariable("SHOP_TYPE", val24.WorkshopType.Name);
						HeroHelper.SetPropertiesToTextObject(character, val28, "NOTABLE");
						conversationScraps.Add(val28);
					}
					if (traitLevel + traitLevel3 < 0 && (int)prosperityLevel <= 0)
					{
						TextObject val29 = new TextObject("{=LM1yg5tI}They say {NOTABLE.LINK} turned one of {?NOTABLE.GENDER}her{?}his{\\?} artisans at the {.%}{SHOP_TYPE}{.%} out on the street for not showing up to work at first light. Hard times, these.", (Dictionary<string, object>)null);
						val29.SetTextVariable("SHOP_TYPE", val24.WorkshopType.Name);
						HeroHelper.SetPropertiesToTextObject(character, val29, "NOTABLE");
						conversationScraps.Add(val29);
					}
					if (traitLevel + traitLevel3 < 0)
					{
						TextObject val30 = new TextObject("{=dmaCQda0}Yeah. About that {NOTABLE.LINK}. A hard {?NOTABLE.GENDER}one{?}master{\\?} to work for, they say. One mistake and you're out on the street.", (Dictionary<string, object>)null);
						val30.SetTextVariable("SHOP_TYPE", val24.WorkshopType.Name);
						HeroHelper.SetPropertiesToTextObject(character, val30, "NOTABLE");
						conversationScraps.Add(val30);
					}
					if (traitLevel4 > 0 && (int)prosperityLevel >= 2)
					{
						TextObject val31 = new TextObject("{=TMhUM4tz}Heard that {NOTABLE.LINK} is making money hand over fist from {?NOTABLE.GENDER}her{?}his{\\?} {.%}{SHOP_TYPE}{.%}. A cunning one, {?NOTABLE.GENDER}she{?}he{\\?} is.", (Dictionary<string, object>)null);
						val31.SetTextVariable("SHOP_TYPE", val24.WorkshopType.Name);
						HeroHelper.SetPropertiesToTextObject(character, val31, "NOTABLE");
						conversationScraps.Add(val31);
					}
				}
			}
			int relation = character.GetRelation(leader);
			if (character.IsHeadman || character.IsArtisan)
			{
				if ((int)prosperityLevel <= 0 && relation >= 10)
				{
					TextObject val32 = new TextObject("{=ko2Nb5im}I know times are hard, but {NOTABLE.LINK} says {?OWNER.GENDER}lady{?}lord{\\?} {OWNER.LINK} is doing what {?OWNER.GENDER}she{?}he{\\?} can, and I trust {NOTABLE.FIRSTNAME}.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val32, "NOTABLE");
					HeroHelper.SetPropertiesToTextObject(leader, val32, "OWNER");
					conversationScraps.Add(val32);
				}
				if ((int)prosperityLevel >= 2 && relation >= 10)
				{
					TextObject val33 = new TextObject("{=CdRtnwwX}Things are good, and {NOTABLE.LINK} says we should credit this to the wisdom of {?OWNER.GENDER}lady{?}lord{\\?} {OWNER.LINK}.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val33, "NOTABLE");
					HeroHelper.SetPropertiesToTextObject(leader, val33, "OWNER");
					conversationScraps.Add(val33);
				}
				if (character.GetTraitLevel(DefaultTraits.Honor) > 0)
				{
					TextObject val34 = new TextObject("{=Ku49Ipkf}I'll tell anyone who asks: {NOTABLE.LINK} is a righteous {?NOTABLE.GENDER}woman{?}man{\\?}, and speaks for the poor folk. The lords of this land must listen to {?NOTABLE.GENDER}her{?}him{\\?}, or misfortune will fall upon us.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val34, "NOTABLE");
					conversationScraps.Add(val34);
				}
				if (character.GetTraitLevel(DefaultTraits.Mercy) > 0 && (int)prosperityLevel <= 1)
				{
					TextObject val35 = new TextObject("{=P7aRBg2h}Thank the heavens we have {?NOTABLE.GENDER}women{?}men{\\?} like {NOTABLE.LINK} to help the poor in these hard times.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val35, "NOTABLE");
					conversationScraps.Add(val35);
				}
				if (character.GetTraitLevel(DefaultTraits.Mercy) <= 0 && Settlement.CurrentSettlement.IsTown && !flag)
				{
					TextObject val36 = new TextObject("{=Fu47uNh4}My sister's husband is in the tavern with all the layabouts, throwing away his wages on drinking and dice while his family weeps. I hope {NOTABLE.LINK} gathers some righteous folk and sets fire to it! I'd carry a torch myself, I would.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val36, "NOTABLE");
					conversationScraps.Add(val36);
				}
				if (leader.GetTraitLevel(DefaultTraits.Honor) >= 0 && relation > 0 && Settlement.CurrentSettlement.IsTown && !flag)
				{
					TextObject val37 = new TextObject("{=NOb4GcOT}{NOTABLE.LINK} says we must trust in the wisdom of the authorities, so long as they follow the laws of the Heavens.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val37, "NOTABLE");
					conversationScraps.Add(val37);
				}
				if (relation < -10 && Settlement.CurrentSettlement.IsTown && !flag)
				{
					TextObject val38 = new TextObject("{=LaBQQ7ue}{NOTABLE.LINK} says that a fish rots from the head. Well, look around this place. I think we know what he's getting at.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val38, "NOTABLE");
					conversationScraps.Add(val38);
				}
			}
			if (character.IsPreacher)
			{
				if (character.GetTraitLevel(DefaultTraits.Honor) > 0)
				{
					TextObject val39 = new TextObject("{=9XijWk1o}I'll tell anyone who asks: {NOTABLE.LINK} is a righteous {?NOTABLE.GENDER}woman{?}man{\\?}, and the Heavens speak through {?NOTABLE.GENDER}her{?}his{\\?} mouth. The lords of this land must listen to {?NOTABLE.GENDER}her{?}him{\\?}, or misfortune will fall upon us.", (Dictionary<string, object>)null);
					conversationScraps.Add(new TextObject(((object)val39).ToString(), (Dictionary<string, object>)null));
				}
				if (character.GetTraitLevel(DefaultTraits.Mercy) > 0 && (int)prosperityLevel <= 1)
				{
					TextObject val40 = new TextObject("{=P7aRBg2h}Thank the heavens we have {?NOTABLE.GENDER}women{?}men{\\?} like {NOTABLE.LINK} to help the poor in these hard times.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val40, "NOTABLE");
					conversationScraps.Add(new TextObject(((object)val40).ToString(), (Dictionary<string, object>)null));
				}
				if (character.GetTraitLevel(DefaultTraits.Mercy) <= 0 && Settlement.CurrentSettlement.IsTown && !flag)
				{
					TextObject val41 = new TextObject("{=Fu47uNh4}My sister's husband is in the tavern with all the layabouts, throwing away his wages on drinking and dice while his family weeps. I hope {NOTABLE.LINK} gathers some righteous folk and sets fire to it! I'd carry a torch myself, I would.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val41, "NOTABLE");
					conversationScraps.Add(val41);
				}
				if (leader.GetTraitLevel(DefaultTraits.Honor) >= 0 && relation > 0 && Settlement.CurrentSettlement.IsTown && !flag)
				{
					TextObject val42 = new TextObject("{=NOb4GcOT}{NOTABLE.LINK} says we must trust in the wisdom of the authorities, so long as they follow the laws of the Heavens.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val42, "NOTABLE");
					conversationScraps.Add(val42);
				}
				if (relation < -10 && Settlement.CurrentSettlement.IsTown && !flag)
				{
					TextObject val43 = new TextObject("{=LaBQQ7ue}{NOTABLE.LINK} says that a fish rots from the head. Well, look around this place. I think we know what he's getting at.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val43, "NOTABLE");
					conversationScraps.Add(val43);
				}
			}
			if (character.IsRuralNotable)
			{
				if (character.GetTraitLevel(DefaultTraits.Honor) < 0 && character.GetTraitLevel(DefaultTraits.Generosity) <= 0 && character.GetTraitLevel(DefaultTraits.Mercy) <= 0)
				{
					TextObject val44 = new TextObject("{=avk9kNn1}Funny how the boundary stones on {NOTABLE.FIRSTNAME}'s land always seem a little bit closer every time you look at them.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val44, "NOTABLE");
					conversationScraps.Add(val44);
				}
				if (character.GetTraitLevel(DefaultTraits.Generosity) < 0 && character.GetTraitLevel(DefaultTraits.Mercy) <= 0)
				{
					TextObject val45 = new TextObject("{=Ckou6bfg}Old skinflint {NOTABLE.FIRSTNAME} is my great-uncle's son, and not a blessed penny will {?NOTABLE.GENDER}she{?}he{\\?} give me in hard times.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val45, "NOTABLE");
					conversationScraps.Add(val45);
				}
				if (character.GetTraitLevel(DefaultTraits.Calculating) > 0 && character.GetTraitLevel(DefaultTraits.Generosity) <= 0)
				{
					TextObject val46 = new TextObject("{=Xo5lc6sz}If you've got more mouths in your house than your land will feed, {NOTABLE.FIRSTNAME} will let you work a bit of {?NOTABLE.GENDER}her{?}his{\\?} property - but he'll take a third of your harvest, even if you're kin.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val46, "NOTABLE");
					conversationScraps.Add(val46);
				}
				if (character.GetTraitLevel(DefaultTraits.Generosity) > 0)
				{
					TextObject val47 = new TextObject("{=DeoalRXY}One of {NOTABLE.FIRSTNAME}'s hunting hounds took down my cousin's sheep, but {NOTABLE.FIRSTNAME} paid twice the beast's value in compensation -- very fair, I say, very fair indeed.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val47, "NOTABLE");
					conversationScraps.Add(val47);
				}
				if (character.GetTraitLevel(DefaultTraits.Calculating) - character.GetTraitLevel(DefaultTraits.Mercy) < 0)
				{
					TextObject val48 = new TextObject("{=o3MDpesZ}The way {NOTABLE.FIRSTNAME} helps out {?NOTABLE.GENDER}her{?}his{\\?} good-for-nothing relatives, {?NOTABLE.GENDER}she{?}he{\\?} won't have any seed grain for the next sewing -- and I, for one, will laugh.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val48, "NOTABLE");
					conversationScraps.Add(val48);
				}
				if (character.GetTraitLevel(DefaultTraits.Honor) > 0 && character.GetTraitLevel(DefaultTraits.Generosity) < 0)
				{
					TextObject val49 = new TextObject("{=QyGQanO7}There's no doubt that {NOTABLE.FIRSTNAME} is a stiff-necked old goat, but you can't say {?NOTABLE.GENDER}she{?}he{\\?}'s not honest as the day is long.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val49, "NOTABLE");
					conversationScraps.Add(val49);
				}
				if (character.GetTraitLevel(DefaultTraits.Generosity) + character.GetTraitLevel(DefaultTraits.Mercy) + character.GetTraitLevel(DefaultTraits.Calculating) > 0)
				{
					TextObject val50 = new TextObject("{=NazK9rwF}A decent {?NOTABLE.GENDER}woman{?}man{\\?}, {NOTABLE.FIRSTNAME} is, as rich folk go. Always keeps a bit of grain to help out any of those who ate their seed over the winter. Knows the worth of a good name, {?NOTABLE.GENDER}she{?}he{\\?} does.", (Dictionary<string, object>)null);
					HeroHelper.SetPropertiesToTextObject(character, val50, "NOTABLE");
					conversationScraps.Add(val50);
				}
			}
		}
	}

	private bool conversation_villager_talk_start_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if (((int)CharacterObject.OneToOneConversationCharacter.Occupation == 6 || (int)CharacterObject.OneToOneConversationCharacter.Occupation == 8) && PlayerEncounter.Current != null)
		{
			return PlayerEncounter.InsideSettlement;
		}
		return false;
	}

	private bool conversation_townsfolk_ask_asses_prices_on_condition()
	{
		return Settlement.CurrentSettlement.IsTown;
	}

	private void conversation_townsfolk_ask_asses_prices_on_consequence()
	{
	}
}
