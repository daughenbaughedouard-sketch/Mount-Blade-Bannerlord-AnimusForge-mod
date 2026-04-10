using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Conversation;
using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionLogics;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.CampaignBehaviors;

public class GuardsCampaignBehavior : CampaignBehaviorBase
{
	public const float UnarmedTownGuardSpawnRate = 0.4f;

	private readonly List<(CharacterObject, int)> _garrisonTroops = new List<(CharacterObject, int)>();

	public override void RegisterEvents()
	{
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

	private float GetProsperityMultiplier(SettlementComponent settlement)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ((float)settlement.GetProsperityLevel() + 1f) / 3f;
	}

	private void AddGarrisonAndPrisonCharacters(Settlement settlement)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		InitializeGarrisonCharacters(settlement);
		Location locationWithId = settlement.LocationComplex.GetLocationWithId("center");
		CultureObject val = (((int)Campaign.Current.GameMode == 1) ? settlement.MapFaction.Culture : settlement.Culture);
		locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreatePrisonGuard), val, (CharacterRelations)0, 1);
	}

	private void InitializeGarrisonCharacters(Settlement settlement)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		_garrisonTroops.Clear();
		if ((int)Campaign.Current.GameMode != 1)
		{
			return;
		}
		MobileParty garrisonParty = ((Fief)settlement.Town).GarrisonParty;
		if (garrisonParty == null)
		{
			return;
		}
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)garrisonParty.MemberRoster.GetTroopRoster())
		{
			TroopRosterElement current = item;
			if ((int)current.Character.Occupation == 7)
			{
				_garrisonTroops.Add((current.Character, ((TroopRosterElement)(ref current)).Number - ((TroopRosterElement)(ref current)).WoundedNumber));
			}
		}
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		if (settlement.IsFortification)
		{
			AddGarrisonAndPrisonCharacters(settlement);
			if ((settlement.IsTown || settlement.IsCastle) && CampaignMission.Current != null)
			{
				Location location = CampaignMission.Current.Location;
				AddGuardsFromGarrison(settlement, unusedUsablePointCount, location);
			}
		}
	}

	private void AddGuardsFromGarrison(Settlement settlement, Dictionary<string, int> unusedUsablePointCount, Location location)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Expected O, but got Unknown
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Expected O, but got Unknown
		unusedUsablePointCount.TryGetValue("sp_guard", out var value);
		unusedUsablePointCount.TryGetValue("sp_guard_with_spear", out var value2);
		unusedUsablePointCount.TryGetValue("sp_guard_patrol", out var value3);
		unusedUsablePointCount.TryGetValue("sp_guard_unarmed", out var value4);
		unusedUsablePointCount.TryGetValue("sp_guard_castle", out var value5);
		float prosperityMultiplier = GetProsperityMultiplier(settlement.SettlementComponent);
		float num = (settlement.IsCastle ? 1.6f : 0.4f);
		value = (int)((float)value * prosperityMultiplier);
		value2 = (int)((float)value2 * prosperityMultiplier);
		value3 = (int)((float)value3 * prosperityMultiplier);
		value4 = (int)((float)value4 * prosperityMultiplier * num);
		if (value5 > 0)
		{
			location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateCastleGuard), settlement.Culture, (CharacterRelations)0, value5);
		}
		if (value > 0)
		{
			location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateStandGuard), settlement.Culture, (CharacterRelations)0, value);
		}
		if (value2 > 0)
		{
			location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateStandGuardWithSpear), settlement.Culture, (CharacterRelations)0, value2);
		}
		if (value3 > 0)
		{
			location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreatePatrollingGuard), settlement.Culture, (CharacterRelations)0, value3);
		}
		if (value4 > 0 && location != settlement.LocationComplex.GetLocationWithId("lordshall"))
		{
			location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateUnarmedGuard), settlement.Culture, (CharacterRelations)0, value4);
		}
		if (location.StringId == "prison")
		{
			if (unusedUsablePointCount.TryGetValue("area_marker_1", out var value6) && value6 > 0)
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateStandGuard), settlement.Culture, (CharacterRelations)0, 1);
			}
			if (unusedUsablePointCount.TryGetValue("area_marker_2", out value6) && value6 > 0)
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateStandGuard), settlement.Culture, (CharacterRelations)0, 1);
			}
			if (unusedUsablePointCount.TryGetValue("area_marker_3", out value6) && value6 > 0)
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateStandGuard), settlement.Culture, (CharacterRelations)0, 1);
			}
		}
	}

	private static ItemObject GetSuitableSpear(CultureObject culture)
	{
		string text = ((((MBObjectBase)culture).StringId == "battania") ? "northern_spear_2_t3" : "western_spear_3_t3");
		return MBObjectManager.Instance.GetObject<ItemObject>(text);
	}

	private AgentData TakeGuardAgentDataFromGarrisonTroopList(CultureObject culture, bool overrideWeaponWithSpear = false, bool unarmed = false)
	{
		CharacterObject guardRosterElement;
		if (_garrisonTroops.Count > 0)
		{
			List<((CharacterObject, int), float)> list = new List<((CharacterObject, int), float)>();
			foreach (var garrisonTroop in _garrisonTroops)
			{
				list.Add(((garrisonTroop.Item1, garrisonTroop.Item2), ((BasicCharacterObject)garrisonTroop.Item1).Level));
			}
			int index = default(int);
			(CharacterObject, int) tuple = MBRandom.ChooseWeighted<(CharacterObject, int)>((IReadOnlyList<ValueTuple<(CharacterObject, int), float>>)list, ref index);
			(guardRosterElement, _) = tuple;
			if (tuple.Item2 <= 1)
			{
				_garrisonTroops.RemoveAt(index);
			}
			else
			{
				_garrisonTroops[index] = (tuple.Item1, tuple.Item2 - 1);
			}
		}
		else
		{
			guardRosterElement = culture.Guard;
		}
		return PrepareGuardAgentDataFromGarrison(guardRosterElement, overrideWeaponWithSpear, unarmed);
	}

	private static AgentData PrepareGuardAgentDataFromGarrison(CharacterObject guardRosterElement, bool overrideWeaponWithSpear = false, bool unarmed = false)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Expected O, but got Unknown
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		Banner val = (((int)Campaign.Current.GameMode == 1) ? PlayerEncounter.LocationEncounter.Settlement.OwnerClan.Banner : null);
		Equipment randomEquipmentElements = Equipment.GetRandomEquipmentElements((BasicCharacterObject)(object)guardRosterElement, false, (EquipmentType)0, -1);
		Dictionary<ItemTypeEnum, int> dictionary = new Dictionary<ItemTypeEnum, int>
		{
			{
				(ItemTypeEnum)4,
				0
			},
			{
				(ItemTypeEnum)10,
				0
			},
			{
				(ItemTypeEnum)6,
				0
			},
			{
				(ItemTypeEnum)9,
				0
			},
			{
				(ItemTypeEnum)5,
				0
			},
			{
				(ItemTypeEnum)11,
				0
			},
			{
				(ItemTypeEnum)7,
				0
			},
			{
				(ItemTypeEnum)12,
				0
			},
			{
				(ItemTypeEnum)8,
				0
			}
		};
		int num = 0;
		EquipmentElement val2;
		for (int i = 0; i <= 4; i++)
		{
			val2 = randomEquipmentElements[i];
			if (((EquipmentElement)(ref val2)).Item != null)
			{
				val2 = randomEquipmentElements[i];
				if (dictionary.ContainsKey(((EquipmentElement)(ref val2)).Item.ItemType))
				{
					val2 = randomEquipmentElements[i];
					dictionary[((EquipmentElement)(ref val2)).Item.ItemType]++;
				}
				else
				{
					num++;
				}
			}
		}
		if (overrideWeaponWithSpear && dictionary[(ItemTypeEnum)4] > 0)
		{
			dictionary[(ItemTypeEnum)4]--;
		}
		if (num > 0)
		{
			num--;
		}
		else if (dictionary[(ItemTypeEnum)4] > 0)
		{
			dictionary[(ItemTypeEnum)4]--;
		}
		else if (dictionary[(ItemTypeEnum)9] > 0)
		{
			dictionary[(ItemTypeEnum)5]--;
			dictionary[(ItemTypeEnum)9]--;
		}
		else if (dictionary[(ItemTypeEnum)10] > 0)
		{
			dictionary[(ItemTypeEnum)10]--;
			dictionary[(ItemTypeEnum)6]--;
		}
		else if (dictionary[(ItemTypeEnum)11] > 0)
		{
			dictionary[(ItemTypeEnum)11]--;
			dictionary[(ItemTypeEnum)7]--;
		}
		for (int num2 = 4; num2 >= 0; num2--)
		{
			val2 = randomEquipmentElements[num2];
			if (((EquipmentElement)(ref val2)).Item != null)
			{
				bool flag = false;
				val2 = randomEquipmentElements[num2];
				if (dictionary.TryGetValue(((EquipmentElement)(ref val2)).Item.ItemType, out var value))
				{
					if (value > 0)
					{
						flag = true;
						val2 = randomEquipmentElements[num2];
						dictionary[((EquipmentElement)(ref val2)).Item.ItemType]--;
					}
				}
				else if (num > 0)
				{
					flag = true;
					num--;
				}
				if (flag)
				{
					int num3 = num2;
					val2 = default(EquipmentElement);
					randomEquipmentElements.AddEquipmentToSlotWithoutAgent((EquipmentIndex)num3, val2);
				}
			}
		}
		if (overrideWeaponWithSpear)
		{
			if (!IfEquipmentHasSpearSwapSlots(randomEquipmentElements))
			{
				ItemObject suitableSpear = GetSuitableSpear(guardRosterElement.Culture);
				randomEquipmentElements.AddEquipmentToSlotWithoutAgent((EquipmentIndex)3, new EquipmentElement(suitableSpear, (ItemModifier)null, (ItemObject)null, false));
				IfEquipmentHasSpearSwapSlots(randomEquipmentElements);
			}
		}
		else if (unarmed)
		{
			val2 = default(EquipmentElement);
			randomEquipmentElements.AddEquipmentToSlotWithoutAgent((EquipmentIndex)0, val2);
			val2 = default(EquipmentElement);
			randomEquipmentElements.AddEquipmentToSlotWithoutAgent((EquipmentIndex)1, val2);
			val2 = default(EquipmentElement);
			randomEquipmentElements.AddEquipmentToSlotWithoutAgent((EquipmentIndex)2, val2);
			val2 = default(EquipmentElement);
			randomEquipmentElements.AddEquipmentToSlotWithoutAgent((EquipmentIndex)3, val2);
			val2 = default(EquipmentElement);
			randomEquipmentElements.AddEquipmentToSlotWithoutAgent((EquipmentIndex)4, val2);
			val2 = default(EquipmentElement);
			randomEquipmentElements.AddEquipmentToSlotWithoutAgent((EquipmentIndex)5, val2);
			val2 = default(EquipmentElement);
			randomEquipmentElements.AddEquipmentToSlotWithoutAgent((EquipmentIndex)8, val2);
		}
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)guardRosterElement).Race, "_settlement");
		return new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)guardRosterElement, -1, val, default(UniqueTroopDescriptor))).Equipment(randomEquipmentElements).Monster(monsterWithSuffix).NoHorses(true);
	}

	private static bool IfEquipmentHasSpearSwapSlots(Equipment equipment)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			EquipmentElement val2 = equipment[val];
			ItemObject item = ((EquipmentElement)(ref val2)).Item;
			if (item != null && item.WeaponComponent.PrimaryWeapon.IsPolearm)
			{
				Equipment.SwapWeapons(equipment, val, (EquipmentIndex)0);
				return true;
			}
		}
		return false;
	}

	private void RemoveShields(Equipment equipment)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			EquipmentElement val2 = equipment[val];
			ItemObject item = ((EquipmentElement)(ref val2)).Item;
			if (item != null && item.WeaponComponent.PrimaryWeapon.IsShield)
			{
				EquipmentIndex val3 = val;
				val2 = default(EquipmentElement);
				equipment.AddEquipmentToSlotWithoutAgent(val3, val2);
			}
		}
	}

	private LocationCharacter CreateCastleGuard(CultureObject culture, CharacterRelations relation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		AgentData val = TakeGuardAgentDataFromGarrisonTroopList(culture, overrideWeaponWithSpear: true);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddStandGuardBehaviors), "sp_guard_castle", true, (CharacterRelations)0, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_guard"), false, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreateStandGuard(CultureObject culture, CharacterRelations relation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		AgentData val = TakeGuardAgentDataFromGarrisonTroopList(culture);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddStandGuardBehaviors), "sp_guard", true, (CharacterRelations)0, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_guard"), false, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreateStandGuardWithSpear(CultureObject culture, CharacterRelations relation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		AgentData val = TakeGuardAgentDataFromGarrisonTroopList(culture, overrideWeaponWithSpear: true);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddStandGuardBehaviors), "sp_guard_with_spear", true, (CharacterRelations)0, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_guard"), false, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreateUnarmedGuard(CultureObject culture, CharacterRelations relation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		AgentData val = TakeGuardAgentDataFromGarrisonTroopList(culture, overrideWeaponWithSpear: false, unarmed: true);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "sp_guard_unarmed", true, (CharacterRelations)0, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_unarmed_guard"), false, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreatePatrollingGuard(CultureObject culture, CharacterRelations relation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		AgentData val = TakeGuardAgentDataFromGarrisonTroopList(culture);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddPatrollingGuardBehaviors), "sp_guard_patrol", true, (CharacterRelations)0, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_guard"), false, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private LocationCharacter CreatePrisonGuard(CultureObject culture, CharacterRelations relation)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		CharacterObject prisonGuard = culture.PrisonGuard;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)prisonGuard).Race, "_settlement");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)prisonGuard, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddStandGuardBehaviors), "sp_prison_guard", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_guard"), false, true, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	protected void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0123: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Expected O, but got Unknown
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Expected O, but got Unknown
		//IL_025e: Expected O, but got Unknown
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Expected O, but got Unknown
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Expected O, but got Unknown
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Expected O, but got Unknown
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Expected O, but got Unknown
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Expected O, but got Unknown
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Expected O, but got Unknown
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Expected O, but got Unknown
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Expected O, but got Unknown
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Expected O, but got Unknown
		//IL_0515: Expected O, but got Unknown
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Expected O, but got Unknown
		//IL_0577: Expected O, but got Unknown
		//IL_0577: Expected O, but got Unknown
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c3: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("disguise_start_conversation_alt", "start", "close_window", "{=uTycGRdI}You need to move along. I'm on duty right now and I can't spare any coin. May Heaven provide.", new OnConditionDelegate(conversation_disguised_start_on_condition_alt), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("disguise_start_conversation", "start", "close_window", "{=P98iCLjl}Get out of my face, you vile beggar.[if:convo_angry]", new OnConditionDelegate(conversation_disguised_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_start_criminal", "start", "prison_guard_talk_criminal", "{=0UUCTaEj}We hear a lot of complaints about you lately. You better start behaving or you'll get yourself a good flogging.[if:convo_mocking_revenge]", new OnConditionDelegate(conversation_prison_guard_criminal_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_ask_criminal", "prison_guard_talk_criminal", "prison_guard_talk", "{=XqTa0iQZ}What do you want, you degenerate?[if:convo_stern]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_start", "start", "prison_guard_talk", "{=6SppoTum}Yes? What do you want?", new OnConditionDelegate(conversation_prison_guard_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_ask_prisoners", "prison_guard_talk", "prison_guard_ask_prisoners", "{=av0bRae8}Who is imprisoned here?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_ask_prisoner_talk", "prison_guard_talk", "close_window", "{=QxIXbHai}I want to speak with a prisoner (Cheat).", new OnConditionDelegate(conversation_prison_guard_visit_prison_cheat_on_condition), new OnConsequenceDelegate(conversation_prison_guard_visit_prison_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_ask_prisoner_talk_2", "prison_guard_talk", "prison_guard_visit_prison", "{=EGI6ztlH}I want to speak with a prisoner.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_talk_end", "prison_guard_talk", "close_window", "{=D33fIGQe}Never mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_talk_about_prisoners", "prison_guard_ask_prisoners", "prison_guard_talk", "{=2eydhtcz}Currently, {PRISONER_NAMES} {?IS_PLURAL}are{?}is{\\?} imprisoned here.", new OnConditionDelegate(conversation_prison_guard_talk_about_prisoners_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_visit_prison_ask_for_permission", "prison_guard_visit_prison", "prison_guard_visit_prison_ask_for_permission_answer", "{=XN0XZAkI}I can't let you in. My {?SETTLEMENT_OWNER.GENDER}Lady{?}Lord{\\?} {SETTLEMENT_OWNER.NAME} would be furious.", new OnConditionDelegate(conversation_prison_guard_reject_visit_prison_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_visit_prison", "prison_guard_visit_prison", "close_window", "{=XWpEpaQ4}Of course, {?PLAYER.GENDER}madam{?}sir{\\?}. Go in.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_prison_guard_visit_prison_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_visit_prison_ask_answer", "prison_guard_visit_prison_ask_for_permission_answer", "prison_guard_visit_prison_ask_for_permission_guard_answer", "{=k3b5KqSc}Come on now. I thought you were the boss here.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_visit_prison_ask_answer_3", "prison_guard_visit_prison_ask_for_permission_guard_answer", "prison_guard_visit_prison_ask_for_permission_answer_options", "{=JaAltoKP}Um... What are you saying?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_visit_permission_try_bribe", "prison_guard_visit_prison_ask_for_permission_answer_options", "prison_guard_bribe_answer_satisfied", "{=dY3Vazug}I found a purse with {AMOUNT}{GOLD_ICON} a few paces away. I reckon it belongs to you.", new OnConditionDelegate(prison_guard_visit_permission_try_bribe_on_condition), (OnConsequenceDelegate)null, 100, new OnClickableConditionDelegate(can_player_bribe_to_prison_guard_clickable), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_visit_prison_ask_answer_3_2", "prison_guard_visit_prison_ask_for_permission_answer_options", "close_window", "{=D33fIGQe}Never mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_visit_prison_nobody_inside", "prison_guard_visit_prison", "prison_guard_talk", "{=rVHbbrCQ}We're not holding anyone in here right now. There's no reason for you to go in.[ib:closed]", new OnConditionDelegate(conversation_prison_guard_visit_prison_nobody_inside_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_visit_empty_prison", "prison_guard_visit_prison_nobody_1", "close_window", "{=b3KFoJJ8}All right then. I'll have a look at the prison.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_prison_guard_visit_prison_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_visit_empty_prison_2", "prison_guard_visit_prison_nobody_2", "close_window", "{=b3KFoJJ8}All right then. I'll have a look at the prison.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_not_visit_empty_prison", "prison_guard_visit_prison_nobody_1", "close_window", "{=L5vAhxhO}I have more important business to do.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_not_visit_empty_prison_2", "prison_guard_visit_prison_nobody_2", "close_window", "{=L5vAhxhO}I have more important business to do.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_visit_permission_leave", "prison_guard_visit_prison_2", "close_window", "{=qPRl07mD}All right then. I'll try that.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_visit_permission_bribe", "prison_guard_bribe_answer_satisfied", "close_window", "{=fCrVeHP3}Ah! I was looking for this all day. How good of you to bring it back {?PLAYER.GENDER}madam{?}sir{\\?}. Well, now that I know what an honest {?PLAYER.GENDER}lady{?}man{\\?} you are, there can be no harm in letting you inside for a look. Go in.... Just so you know, though -- I'll be hanging onto the keys, in case you were thinking about undoing anyone's chains.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_prison_guard_visit_permission_bribe_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("prison_guard_visit_permission_try_break", "prison_guard_visit_prison_4", "prison_guard_visit_break", "{=htfLEQlf}Give me the keys to the cells -- now!", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("prison_guard_visit_break", "prison_guard_visit_break", "close_window", "{=Kto7RWKE}Help! Help! Prison break!", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_prison_guard_visit_break_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("castle_guard_start_criminal", "start", "castle_guard_talk_criminal", "{=0UUCTaEj}We hear a lot of complaints about you lately. You better start behaving or you'll get yourself a good flogging.[if:convo_mocking_revenge]", new OnConditionDelegate(conversation_castle_guard_criminal_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("castle_guard_ask_criminal", "castle_guard_talk_criminal", "castle_guard_talk", "{=XqTa0iQZ}What do you want, you degenerate?[if:convo_stern]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("castle_guard_start", "start", "castle_guard_talk", "{=6SppoTum}Yes? What do you want?", new OnConditionDelegate(conversation_castle_guard_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("guard_start", "start", "close_window", "{=!}{GUARD_COMMENT}", new OnConditionDelegate(conversation_guard_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_ask_for_permission_to_enter_lords_hall", "castle_guard_talk", "player_ask_permission_to_lords_hall", "{=b2h3r1kL}I want to visit the lord's hall.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_ask_for_permission_to_enter_lords_hall_2", "castle_guard_talk", "close_window", "{=never_mind}Never mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("castle_guard_no_permission_nobody_inside", "player_ask_permission_to_lords_hall", "permisson_for_lords_hall", "{=RJtCakaG}There is nobody inside to receive you right now.", new OnConditionDelegate(conversation_castle_guard_nobody_inside_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("castle_guard_player_can_enter", "player_ask_permission_to_lords_hall", "close_window", "{=bbroVUrD}Of course, my {?PLAYER.GENDER}lady{?}lord{\\?}.", new OnConditionDelegate(conversation_castle_guard_player_can_enter_lordshall_condition), (OnConsequenceDelegate)delegate
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += OpenLordsHallMission;
		}, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("castle_guard_no_permission", "player_ask_permission_to_lords_hall", "permisson_for_lords_hall", "{=rcoESVVz}Sorry, but we don't know you. We can't just let anyone in. (Not enough renown)", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_bribe_to_enter_lords_hall", "permisson_for_lords_hall", "player_bribe_to_castle_guard", "{=7wkHMnNM}Maybe {AMOUNT}{GOLD_ICON} will help you to remember me.", new OnConditionDelegate(conversation_player_bribe_to_enter_lords_hall_on_condition), new OnConsequenceDelegate(conversation_player_bribe_to_enter_lords_hall_on_consequence), 100, new OnClickableConditionDelegate(conversation_player_bribe_to_enter_lords_hall_on_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_not_bribe_to_enter_lords_hall", "permisson_for_lords_hall", "close_window", "{=xatWDriV}Never mind then.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("castle_guard_let_player_in", "player_bribe_to_castle_guard", "close_window", "{=g5ofoKa8}Yeah... Now I remember you.", (OnConditionDelegate)null, (OnConsequenceDelegate)delegate
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += OpenLordsHallMission;
		}, 100, (OnClickableConditionDelegate)null);
	}

	private bool conversation_prison_guard_criminal_start_on_condition()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		if (!Campaign.Current.IsMainHeroDisguised && (int)CharacterObject.OneToOneConversationCharacter.Occupation == 23 && Settlement.CurrentSettlement.MapFaction != Hero.MainHero.MapFaction)
		{
			if (!Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingModerate(Settlement.CurrentSettlement.MapFaction) && !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingSevere(Settlement.CurrentSettlement.MapFaction))
			{
				return Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingMild(Settlement.CurrentSettlement.MapFaction);
			}
			return true;
		}
		return false;
	}

	private bool conversation_prison_guard_start_on_condition()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		if (!Campaign.Current.IsMainHeroDisguised && (int)CharacterObject.OneToOneConversationCharacter.Occupation == 23)
		{
			if (Settlement.CurrentSettlement.MapFaction != Hero.MainHero.MapFaction)
			{
				if (Settlement.CurrentSettlement.MapFaction != Hero.MainHero.MapFaction && !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingModerate(Settlement.CurrentSettlement.MapFaction) && !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingSevere(Settlement.CurrentSettlement.MapFaction))
				{
					return !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingMild(Settlement.CurrentSettlement.MapFaction);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool conversation_prison_guard_talk_about_prisoners_on_condition()
	{
		List<CharacterObject> prisonerHeroes = Settlement.CurrentSettlement.SettlementComponent.GetPrisonerHeroes();
		if (prisonerHeroes.Count == 0)
		{
			MBTextManager.SetTextVariable("PRISONER_NAMES", GameTexts.FindText("str_nobody", (string)null), false);
			MBTextManager.SetTextVariable("IS_PLURAL", "0", false);
		}
		else
		{
			for (int i = 0; i < prisonerHeroes.Count; i++)
			{
				if (i == 0)
				{
					MBTextManager.SetTextVariable("LEFT", ((BasicCharacterObject)prisonerHeroes[i]).Name, false);
					continue;
				}
				MBTextManager.SetTextVariable("RIGHT", ((BasicCharacterObject)prisonerHeroes[i]).Name, false);
				MBTextManager.SetTextVariable("LEFT", ((object)GameTexts.FindText("str_LEFT_comma_RIGHT", (string)null)).ToString(), false);
			}
			MBTextManager.SetTextVariable("IS_PLURAL", (prisonerHeroes.Count > 1) ? 1 : 0);
			MBTextManager.SetTextVariable("PRISONER_NAMES", ((object)GameTexts.FindText("str_LEFT_ONLY", (string)null)).ToString(), false);
		}
		return true;
	}

	private bool conversation_prison_guard_visit_prison_cheat_on_condition()
	{
		return Game.Current.IsDevelopmentMode;
	}

	private bool can_player_bribe_to_prison_guard_clickable(out TextObject explanation)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		int bribeToEnterDungeon = Campaign.Current.Models.BribeCalculationModel.GetBribeToEnterDungeon(Settlement.CurrentSettlement);
		if (Hero.MainHero.Gold < bribeToEnterDungeon)
		{
			explanation = new TextObject("{=TP7rZTKs}You don't have {DENAR_AMOUNT}{GOLD_ICON} denars.", (Dictionary<string, object>)null);
			explanation.SetTextVariable("DENAR_AMOUNT", bribeToEnterDungeon);
			explanation.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
			return false;
		}
		explanation = new TextObject("{=hCavIm4G}You will pay {AMOUNT}{GOLD_ICON} denars.", (Dictionary<string, object>)null);
		explanation.SetTextVariable("AMOUNT", bribeToEnterDungeon);
		explanation.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
		return true;
	}

	private bool conversation_prison_guard_reject_visit_prison_on_condition()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Invalid comparison between Unknown and I4
		bool num = Settlement.CurrentSettlement.BribePaid >= Campaign.Current.Models.BribeCalculationModel.GetBribeToEnterDungeon(Settlement.CurrentSettlement);
		StringHelpers.SetCharacterProperties("SETTLEMENT_OWNER", Settlement.CurrentSettlement.OwnerClan.Leader.CharacterObject, (TextObject)null, false);
		AccessDetails val = default(AccessDetails);
		Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterDungeon(Settlement.CurrentSettlement, ref val);
		if (!num)
		{
			return (int)val.AccessLevel != 2;
		}
		return false;
	}

	private void conversation_prison_guard_visit_prison_on_consequence()
	{
		if (Settlement.CurrentSettlement.IsFortification)
		{
			Campaign.Current.GameMenuManager.NextLocation = LocationComplex.Current.GetLocationWithId("prison");
			Campaign.Current.GameMenuManager.PreviousLocation = LocationComplex.Current.GetLocationWithId("center");
		}
		Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
		{
			Mission.Current.EndMission();
		};
	}

	private bool conversation_guard_start_on_condition()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		if (Campaign.Current.ConversationManager.OneToOneConversationAgent == null || CheckIfConversationAgentIsEscortingTheMainAgent())
		{
			return false;
		}
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 7 && PlayerEncounter.Current != null && PlayerEncounter.InsideSettlement)
		{
			TextObject val = new TextObject("{=6JL4GyKC}Can't talk right now. Got to keep my eye on things around here.", (Dictionary<string, object>)null);
			if (Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan || Settlement.CurrentSettlement.MapFaction.Leader == Hero.MainHero)
			{
				val = new TextObject("{=xizHRti3}Nothing to report, your lordship.", (Dictionary<string, object>)null);
				if (Hero.MainHero.IsFemale)
				{
					val = new TextObject("{=sIfL5Vnx}Nothing to report, your ladyship.", (Dictionary<string, object>)null);
				}
			}
			else if (Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.Town.Security <= 20f)
			{
				val = new TextObject("{=3sfjBnaJ}It's quiet. Too quiet. Things never stay quiet around here for long.", (Dictionary<string, object>)null);
			}
			else if (Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.Town.Security <= 40f)
			{
				val = new TextObject("{=jjkOBPkY}Can't let down your guard around here. Too many bastards up to no good.", (Dictionary<string, object>)null);
			}
			else if (Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.Town.Security >= 70f)
			{
				val = new TextObject("{=AHg5k9q2}Welcome to {SETTLEMENT_NAME}. I think you'll find these are good, law-abiding folk, for the most part.", (Dictionary<string, object>)null);
				val.SetTextVariable("SETTLEMENT_NAME", Settlement.CurrentSettlement.Name);
			}
			MBTextManager.SetTextVariable("GUARD_COMMENT", val, false);
			return true;
		}
		return false;
	}

	private bool CheckIfConversationAgentIsEscortingTheMainAgent()
	{
		if (Agent.Main != null && Agent.Main.IsActive() && Settlement.CurrentSettlement != null && ConversationMission.OneToOneConversationAgent != null)
		{
			return EscortAgentBehavior.CheckIfAgentIsEscortedBy(ConversationMission.OneToOneConversationAgent, Agent.Main);
		}
		return false;
	}

	private bool conversation_prison_guard_visit_prison_nobody_inside_condition()
	{
		return Settlement.CurrentSettlement.SettlementComponent.GetPrisonerHeroes().Count == 0;
	}

	private bool prison_guard_visit_permission_try_bribe_on_condition()
	{
		int bribeToEnterDungeon = Campaign.Current.Models.BribeCalculationModel.GetBribeToEnterDungeon(Settlement.CurrentSettlement);
		MBTextManager.SetTextVariable("AMOUNT", bribeToEnterDungeon);
		MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">", false);
		return !Campaign.Current.IsMainHeroDisguised;
	}

	private void conversation_prison_guard_visit_permission_bribe_on_consequence()
	{
		int bribeToEnterDungeon = Campaign.Current.Models.BribeCalculationModel.GetBribeToEnterDungeon(Settlement.CurrentSettlement);
		BribeGuardsAction.Apply(Settlement.CurrentSettlement, bribeToEnterDungeon);
		conversation_prison_guard_visit_prison_on_consequence();
	}

	private void conversation_prison_guard_visit_break_on_consequence()
	{
	}

	private bool IsCastleGuard()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		Agent oneToOneConversationAgent = ConversationMission.OneToOneConversationAgent;
		AgentNavigator agentNavigator = ((oneToOneConversationAgent != null) ? oneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator : null);
		bool flag = false;
		if (agentNavigator != null)
		{
			WeakGameEntity gameEntity;
			int num;
			if (agentNavigator.TargetUsableMachine != null && oneToOneConversationAgent.IsUsingGameObject)
			{
				gameEntity = ((ScriptComponentBehavior)agentNavigator.TargetUsableMachine).GameEntity;
				num = (((WeakGameEntity)(ref gameEntity)).HasTag("sp_guard_castle") ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			flag = (byte)num != 0;
			if (!flag && (agentNavigator.SpecialTargetTag == "sp_guard_castle" || agentNavigator.SpecialTargetTag == "sp_guard"))
			{
				Location lordsHallLocation = LocationComplex.Current.GetLocationWithId("lordshall");
				MissionAgentHandler missionBehavior = Mission.Current.GetMissionBehavior<MissionAgentHandler>();
				if (missionBehavior != null && missionBehavior.TownPassageProps != null)
				{
					UsableMachine val = missionBehavior.TownPassageProps.Find((UsableMachine x) => ((Passage)(object)x).ToLocation == lordsHallLocation);
					if (val != null)
					{
						gameEntity = ((ScriptComponentBehavior)val).GameEntity;
						Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
						if (((Vec3)(ref globalPosition)).DistanceSquared(oneToOneConversationAgent.Position) < 100f)
						{
							flag = true;
						}
					}
				}
			}
		}
		return flag;
	}

	private bool conversation_castle_guard_start_on_condition()
	{
		if (!Campaign.Current.IsMainHeroDisguised && IsCastleGuard())
		{
			if (Settlement.CurrentSettlement.MapFaction != Hero.MainHero.MapFaction)
			{
				if (Settlement.CurrentSettlement.MapFaction != Hero.MainHero.MapFaction && !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingModerate(Settlement.CurrentSettlement.MapFaction) && !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingSevere(Settlement.CurrentSettlement.MapFaction))
				{
					return !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingMild(Settlement.CurrentSettlement.MapFaction);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool conversation_castle_guard_criminal_start_on_condition()
	{
		if (!Campaign.Current.IsMainHeroDisguised && IsCastleGuard() && Settlement.CurrentSettlement.MapFaction != Hero.MainHero.MapFaction)
		{
			if (!Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingModerate(Settlement.CurrentSettlement.MapFaction) && !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingSevere(Settlement.CurrentSettlement.MapFaction))
			{
				return Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingMild(Settlement.CurrentSettlement.MapFaction);
			}
			return true;
		}
		return false;
	}

	private bool conversation_castle_guard_nobody_inside_condition()
	{
		if (!LocationComplex.Current.GetLocationWithId("lordshall").GetCharacterList().Any((LocationCharacter c) => ((BasicCharacterObject)c.Character).IsHero && c.Character.HeroObject.IsLord) && Campaign.Current.Models.BribeCalculationModel.GetBribeToEnterLordsHall(Settlement.CurrentSettlement) > 0)
		{
			return Settlement.CurrentSettlement.OwnerClan != Clan.PlayerClan;
		}
		return false;
	}

	private bool conversation_castle_guard_player_can_enter_lordshall_condition()
	{
		bool flag = default(bool);
		TextObject val = default(TextObject);
		return Campaign.Current.Models.SettlementAccessModel.CanMainHeroAccessLocation(Settlement.CurrentSettlement, "lordshall", ref flag, ref val);
	}

	private bool conversation_player_bribe_to_enter_lords_hall_on_condition()
	{
		int bribeToEnterLordsHall = Campaign.Current.Models.BribeCalculationModel.GetBribeToEnterLordsHall(Settlement.CurrentSettlement);
		MBTextManager.SetTextVariable("AMOUNT", bribeToEnterLordsHall);
		MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">", false);
		if (bribeToEnterLordsHall > 0 && !Campaign.Current.IsMainHeroDisguised)
		{
			return !conversation_castle_guard_nobody_inside_condition();
		}
		return false;
	}

	private void conversation_player_bribe_to_enter_lords_hall_on_consequence()
	{
		int bribeToEnterLordsHall = Campaign.Current.Models.BribeCalculationModel.GetBribeToEnterLordsHall(Settlement.CurrentSettlement);
		BribeGuardsAction.Apply(Settlement.CurrentSettlement, bribeToEnterLordsHall);
	}

	private bool conversation_player_bribe_to_enter_lords_hall_on_clickable_condition(out TextObject explanation)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		int bribeToEnterLordsHall = Campaign.Current.Models.BribeCalculationModel.GetBribeToEnterLordsHall(Settlement.CurrentSettlement);
		if (Hero.MainHero.Gold < bribeToEnterLordsHall)
		{
			explanation = new TextObject("{=TP7rZTKs}You don't have {DENAR_AMOUNT}{GOLD_ICON} denars.", (Dictionary<string, object>)null);
			explanation.SetTextVariable("DENAR_AMOUNT", bribeToEnterLordsHall);
			explanation.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
			return false;
		}
		explanation = new TextObject("{=hCavIm4G}You will pay {AMOUNT}{GOLD_ICON} denars.", (Dictionary<string, object>)null);
		explanation.SetTextVariable("AMOUNT", bribeToEnterLordsHall);
		explanation.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
		return true;
	}

	private void OpenLordsHallMission()
	{
		Campaign.Current.GameMenuManager.NextLocation = LocationComplex.Current.GetLocationWithId("lordshall");
		Campaign.Current.GameMenuManager.PreviousLocation = LocationComplex.Current.GetLocationWithId("center");
		Mission.Current.EndMission();
	}

	private bool conversation_disguised_start_on_condition()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		if (Campaign.Current.IsMainHeroDisguised)
		{
			if (!IsCastleGuard() && (int)CharacterObject.OneToOneConversationCharacter.Occupation != 23 && (int)CharacterObject.OneToOneConversationCharacter.Occupation != 24 && (int)CharacterObject.OneToOneConversationCharacter.Occupation != 5)
			{
				return (int)CharacterObject.OneToOneConversationCharacter.Occupation == 7;
			}
			return true;
		}
		return false;
	}

	private bool conversation_disguised_start_on_condition_alt()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		if (Campaign.Current.IsMainHeroDisguised && MBRandom.RandomInt(2) == 0)
		{
			if (!IsCastleGuard() && (int)CharacterObject.OneToOneConversationCharacter.Occupation != 23 && (int)CharacterObject.OneToOneConversationCharacter.Occupation != 24 && (int)CharacterObject.OneToOneConversationCharacter.Occupation != 5)
			{
				return (int)CharacterObject.OneToOneConversationCharacter.Occupation == 7;
			}
			return true;
		}
		return false;
	}
}
