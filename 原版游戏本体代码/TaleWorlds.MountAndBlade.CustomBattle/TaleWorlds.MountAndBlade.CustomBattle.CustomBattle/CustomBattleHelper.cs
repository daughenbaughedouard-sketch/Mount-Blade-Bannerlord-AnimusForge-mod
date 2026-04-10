using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

public static class CustomBattleHelper
{
	public const string DefaultBattleGameTypeStringId = "Battle";

	public const string DefaultSiegeGameTypeStringId = "Siege";

	public const string DefaultVillageGameTypeStringId = "Village";

	private const string EmpireInfantryTroop = "imperial_veteran_infantryman";

	private const string EmpireRangedTroop = "imperial_archer";

	private const string EmpireCavalryTroop = "imperial_heavy_horseman";

	private const string EmpireHorseArcherTroop = "bucellarii";

	private const string SturgiaInfantryTroop = "sturgian_spearman";

	private const string SturgiaRangedTroop = "sturgian_archer";

	private const string SturgiaCavalryTroop = "sturgian_hardened_brigand";

	private const string AseraiInfantryTroop = "aserai_infantry";

	private const string AseraiRangedTroop = "aserai_archer";

	private const string AseraiCavalryTroop = "aserai_mameluke_cavalry";

	private const string AseraiHorseArcherTroop = "aserai_faris";

	private const string VlandiaInfantryTroop = "vlandian_swordsman";

	private const string VlandiaRangedTroop = "vlandian_hardened_crossbowman";

	private const string VlandiaCavalryTroop = "vlandian_knight";

	private const string BattaniaInfantryTroop = "battanian_picked_warrior";

	private const string BattaniaRangedTroop = "battanian_hero";

	private const string BattaniaCavalryTroop = "battanian_scout";

	private const string KhuzaitInfantryTroop = "khuzait_spear_infantry";

	private const string KhuzaitRangedTroop = "khuzait_archer";

	private const string KhuzaitCavalryTroop = "khuzait_lancer";

	private const string KhuzaitHorseArcherTroop = "khuzait_horse_archer";

	private const string NordInfantryTroop = "nord_spear_warrior";

	private const string NordRangedTroop = "nord_marksman";

	public static int GetIndexFromGameTypeStringId(string gameTypeStringId)
	{
		switch (gameTypeStringId)
		{
		case "Battle":
			return 0;
		case "Siege":
			return 1;
		case "Village":
			return 2;
		default:
			Debug.FailedAssert("Given gameTypeStringId: \"" + gameTypeStringId + "\" is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.CustomBattle\\CustomBattle\\CustomBattleHelper.cs", "GetIndexFromGameTypeStringId", 78);
			return -1;
		}
	}

	public static void StartGame(CustomBattleData data)
	{
		Game.Current.PlayerTroop = data.PlayerCharacter;
		if (data.GameTypeStringId == "Siege")
		{
			BannerlordMissions.OpenSiegeMissionWithDeployment(data.SceneId, data.PlayerCharacter, data.PlayerParty, data.EnemyParty, data.IsPlayerGeneral, data.WallHitpointPercentages, data.HasAnySiegeTower, data.AttackerMachines, data.DefenderMachines, data.IsPlayerAttacker, data.SceneUpgradeLevel, data.SeasonId, data.IsSallyOut, data.IsReliefAttack, data.TimeOfDay);
		}
		else
		{
			BannerlordMissions.OpenCustomBattleMission(data.SceneId, data.PlayerCharacter, data.PlayerParty, data.EnemyParty, data.IsPlayerGeneral, data.PlayerSideGeneralCharacter, data.SceneLevel, data.SeasonId, data.TimeOfDay);
		}
	}

	public static int[] GetTroopCounts(int armySize, CustomBattleCompositionData compositionData)
	{
		int[] array = new int[4];
		armySize--;
		array[1] = MathF.Round(compositionData.RangedPercentage * (float)armySize);
		array[2] = MathF.Round(compositionData.CavalryPercentage * (float)armySize);
		array[3] = MathF.Round(compositionData.RangedCavalryPercentage * (float)armySize);
		array[0] = armySize - array.Sum();
		return array;
	}

	public static float[] GetWallHitpointPercentages(int breachedWallCount)
	{
		float[] array = new float[2];
		switch (breachedWallCount)
		{
		case 1:
		{
			int num = MBRandom.RandomInt(2);
			array[num] = 0f;
			array[1 - num] = 1f;
			break;
		}
		case 0:
			array[0] = 1f;
			array[1] = 1f;
			break;
		default:
			array[0] = 0f;
			array[1] = 0f;
			break;
		}
		return array;
	}

	public static SiegeEngineType GetSiegeWeaponType(SiegeEngineType siegeWeaponType)
	{
		if (siegeWeaponType == DefaultSiegeEngineTypes.Ladder)
		{
			return DefaultSiegeEngineTypes.Ladder;
		}
		if (siegeWeaponType == DefaultSiegeEngineTypes.Ballista)
		{
			return DefaultSiegeEngineTypes.Ballista;
		}
		if (siegeWeaponType == DefaultSiegeEngineTypes.FireBallista)
		{
			return DefaultSiegeEngineTypes.FireBallista;
		}
		if (siegeWeaponType == DefaultSiegeEngineTypes.Ram || siegeWeaponType == DefaultSiegeEngineTypes.ImprovedRam)
		{
			return DefaultSiegeEngineTypes.Ram;
		}
		if (siegeWeaponType == DefaultSiegeEngineTypes.SiegeTower)
		{
			return DefaultSiegeEngineTypes.SiegeTower;
		}
		if (siegeWeaponType == DefaultSiegeEngineTypes.Onager || siegeWeaponType == DefaultSiegeEngineTypes.Catapult)
		{
			return DefaultSiegeEngineTypes.Onager;
		}
		if (siegeWeaponType == DefaultSiegeEngineTypes.FireOnager || siegeWeaponType == DefaultSiegeEngineTypes.FireCatapult)
		{
			return DefaultSiegeEngineTypes.FireOnager;
		}
		if (siegeWeaponType == DefaultSiegeEngineTypes.Trebuchet || siegeWeaponType == DefaultSiegeEngineTypes.Bricole)
		{
			return DefaultSiegeEngineTypes.Trebuchet;
		}
		return siegeWeaponType;
	}

	public static CustomBattleData PrepareBattleData(BasicCharacterObject playerCharacter, BasicCharacterObject playerSideGeneralCharacter, CustomBattleCombatant playerParty, CustomBattleCombatant enemyParty, CustomBattlePlayerSide playerSide, CustomBattlePlayerType battlePlayerType, string gameTypeStringId, string scene, string season, float timeOfDay, List<MissionSiegeWeapon> attackerMachines, List<MissionSiegeWeapon> defenderMachines, float[] wallHitPointsPercentages, int sceneLevel, bool isSallyOut)
	{
		bool isPlayerAttacker = playerSide == CustomBattlePlayerSide.Attacker;
		bool isPlayerGeneral = battlePlayerType == CustomBattlePlayerType.Commander;
		CustomBattleData result = new CustomBattleData
		{
			GameTypeStringId = gameTypeStringId,
			SceneId = scene,
			PlayerCharacter = playerCharacter,
			PlayerParty = playerParty,
			EnemyParty = enemyParty,
			IsPlayerGeneral = isPlayerGeneral,
			PlayerSideGeneralCharacter = playerSideGeneralCharacter,
			SeasonId = season,
			SceneLevel = "",
			TimeOfDay = timeOfDay
		};
		if (result.GameTypeStringId == "Siege")
		{
			result.AttackerMachines = attackerMachines;
			result.DefenderMachines = defenderMachines;
			result.WallHitpointPercentages = wallHitPointsPercentages;
			result.HasAnySiegeTower = attackerMachines.Exists((MissionSiegeWeapon mm) => mm.Type == DefaultSiegeEngineTypes.SiegeTower);
			result.IsPlayerAttacker = isPlayerAttacker;
			result.SceneUpgradeLevel = sceneLevel;
			result.IsSallyOut = isSallyOut;
			result.IsReliefAttack = false;
		}
		return result;
	}

	public static CustomBattleCombatant[] GetCustomBattleParties(BasicCharacterObject playerCharacter, BasicCharacterObject playerSideGeneralCharacter, BasicCharacterObject enemyCharacter, BasicCultureObject playerFaction, int[] playerNumbers, List<BasicCharacterObject>[] playerTroopSelections, BasicCultureObject enemyFaction, int[] enemyNumbers, List<BasicCharacterObject>[] enemyTroopSelections, bool isPlayerAttacker)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		Banner val = new Banner(playerFaction.Banner, playerFaction.Color, playerFaction.Color2);
		Banner val2 = new Banner(enemyFaction.Banner, enemyFaction.Color, enemyFaction.Color2);
		if (((MBObjectBase)playerFaction).StringId == ((MBObjectBase)enemyFaction).StringId)
		{
			uint primaryColor = val2.GetPrimaryColor();
			val2.ChangePrimaryColor(val2.GetFirstIconColor());
			val2.ChangeIconColors(primaryColor);
		}
		CustomBattleCombatant[] array = (CustomBattleCombatant[])(object)new CustomBattleCombatant[2]
		{
			new CustomBattleCombatant(new TextObject("{=sSJSTe5p}Player Party", (Dictionary<string, object>)null), playerFaction, val),
			new CustomBattleCombatant(new TextObject("{=0xC75dN6}Enemy Party", (Dictionary<string, object>)null), enemyFaction, val2)
		};
		array[0].Side = (BattleSideEnum)(isPlayerAttacker ? 1 : 0);
		array[0].AddCharacter(playerCharacter, 1);
		if (playerSideGeneralCharacter != null)
		{
			array[0].AddCharacter(playerSideGeneralCharacter, 1);
			array[0].SetGeneral(playerSideGeneralCharacter);
		}
		else
		{
			array[0].SetGeneral(playerCharacter);
		}
		array[1].Side = Extensions.GetOppositeSide(array[0].Side);
		array[1].AddCharacter(enemyCharacter, 1);
		for (int i = 0; i < array.Length; i++)
		{
			PopulateListsWithDefaults(ref array[i], (i == 0) ? playerNumbers : enemyNumbers, (i == 0) ? playerTroopSelections : enemyTroopSelections);
		}
		return array;
	}

	private static void PopulateListsWithDefaults(ref CustomBattleCombatant customBattleParties, int[] numbers, List<BasicCharacterObject>[] troopList)
	{
		BasicCultureObject basicCulture = customBattleParties.BasicCulture;
		if (troopList == null)
		{
			troopList = new List<BasicCharacterObject>[4]
			{
				new List<BasicCharacterObject>(),
				new List<BasicCharacterObject>(),
				new List<BasicCharacterObject>(),
				new List<BasicCharacterObject>()
			};
		}
		if (troopList[0].Count == 0)
		{
			troopList[0] = new List<BasicCharacterObject> { GetDefaultTroopOfFormationForFaction(basicCulture, (FormationClass)0) };
		}
		if (troopList[1].Count == 0)
		{
			troopList[1] = new List<BasicCharacterObject> { GetDefaultTroopOfFormationForFaction(basicCulture, (FormationClass)1) };
		}
		if (troopList[2].Count == 0)
		{
			troopList[2] = new List<BasicCharacterObject> { GetDefaultTroopOfFormationForFaction(basicCulture, (FormationClass)2) };
		}
		if (troopList[3].Count == 0)
		{
			troopList[3] = new List<BasicCharacterObject> { GetDefaultTroopOfFormationForFaction(basicCulture, (FormationClass)3) };
		}
		if (troopList[3].Count == 0 || troopList[3].All((BasicCharacterObject troop) => troop == null))
		{
			numbers[2] += numbers[3] / 3;
			numbers[1] += numbers[3] / 3;
			numbers[0] += numbers[3] / 3;
			numbers[0] += numbers[3] - numbers[3] / 3 * 3;
			numbers[3] = 0;
		}
		for (int num = 0; num < 4; num++)
		{
			int count = troopList[num].Count;
			int num2 = numbers[num];
			if (num2 <= 0)
			{
				continue;
			}
			float num3 = (float)num2 / (float)count;
			float num4 = 0f;
			for (int num5 = 0; num5 < count; num5++)
			{
				float num6 = num3 + num4;
				int num7 = MathF.Floor(num6);
				num4 = num6 - (float)num7;
				customBattleParties.AddCharacter(troopList[num][num5], num7);
				numbers[num] -= num7;
				if (num5 == count - 1 && numbers[num] > 0)
				{
					customBattleParties.AddCharacter(troopList[num][num5], numbers[num]);
					numbers[num] = 0;
				}
			}
		}
	}

	public static void AssertMissingTroopsForDebug()
	{
		foreach (BasicCultureObject faction in CustomBattleData.Factions)
		{
			for (int i = 0; i < 4; i++)
			{
				GetDefaultTroopOfFormationForFaction(faction, (FormationClass)i);
			}
		}
	}

	public static BasicCharacterObject GetDefaultTroopOfFormationForFaction(BasicCultureObject culture, FormationClass formation)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected I4, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected I4, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected I4, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected I4, but got Unknown
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected I4, but got Unknown
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Expected I4, but got Unknown
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Invalid comparison between Unknown and I4
		if (((MBObjectBase)culture).StringId.ToLower() == "empire")
		{
			switch ((int)formation)
			{
			case 0:
				return GetTroopFromId("imperial_veteran_infantryman");
			case 1:
				return GetTroopFromId("imperial_archer");
			case 2:
				return GetTroopFromId("imperial_heavy_horseman");
			case 3:
				return GetTroopFromId("bucellarii");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "sturgia")
		{
			switch ((int)formation)
			{
			case 0:
				return GetTroopFromId("sturgian_spearman");
			case 1:
				return GetTroopFromId("sturgian_archer");
			case 2:
				return GetTroopFromId("sturgian_hardened_brigand");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "aserai")
		{
			switch ((int)formation)
			{
			case 0:
				return GetTroopFromId("aserai_infantry");
			case 1:
				return GetTroopFromId("aserai_archer");
			case 2:
				return GetTroopFromId("aserai_mameluke_cavalry");
			case 3:
				return GetTroopFromId("aserai_faris");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "vlandia")
		{
			switch ((int)formation)
			{
			case 0:
				return GetTroopFromId("vlandian_swordsman");
			case 1:
				return GetTroopFromId("vlandian_hardened_crossbowman");
			case 2:
				return GetTroopFromId("vlandian_knight");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "battania")
		{
			switch ((int)formation)
			{
			case 0:
				return GetTroopFromId("battanian_picked_warrior");
			case 1:
				return GetTroopFromId("battanian_hero");
			case 2:
				return GetTroopFromId("battanian_scout");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "khuzait")
		{
			switch ((int)formation)
			{
			case 0:
				return GetTroopFromId("khuzait_spear_infantry");
			case 1:
				return GetTroopFromId("khuzait_archer");
			case 2:
				return GetTroopFromId("khuzait_lancer");
			case 3:
				return GetTroopFromId("khuzait_horse_archer");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "nord")
		{
			if ((int)formation == 0)
			{
				return GetTroopFromId("nord_spear_warrior");
			}
			if ((int)formation == 1)
			{
				return GetTroopFromId("nord_marksman");
			}
		}
		return null;
	}

	private static BasicCharacterObject GetTroopFromId(string troopId)
	{
		return MBObjectManager.Instance.GetObject<BasicCharacterObject>(troopId);
	}
}
