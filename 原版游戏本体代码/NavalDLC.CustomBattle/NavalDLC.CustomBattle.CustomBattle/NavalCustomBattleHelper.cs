using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.CustomBattle.CustomBattle;

public static class NavalCustomBattleHelper
{
	private const string EmpireInfantryTroop = "imperial_veteran_infantryman";

	private const string EmpireRangedTroop = "imperial_archer";

	private const string SturgiaInfantryTroop = "sturgian_spearman";

	private const string SturgiaRangedTroop = "sturgian_archer";

	private const string AseraiInfantryTroop = "aserai_infantry";

	private const string AseraiRangedTroop = "aserai_archer";

	private const string VlandiaInfantryTroop = "vlandian_swordsman";

	private const string VlandiaRangedTroop = "vlandian_hardened_crossbowman";

	private const string BattaniaInfantryTroop = "battanian_picked_warrior";

	private const string BattaniaRangedTroop = "battanian_hero";

	private const string KhuzaitInfantryTroop = "khuzait_spear_infantry";

	private const string KhuzaitRangedTroop = "khuzait_archer";

	private const string NordInfantryTroop = "nord_spear_warrior";

	private const string NordRangedTroop = "nord_marksman";

	public static void StartGame(NavalCustomBattleData data)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Game.Current.PlayerTroop = data.PlayerCharacter;
		CustomNavalMissions.OpenNavalBattleForCustomMission(data.SceneId, data.PlayerCharacter, data.PlayerParty, Extensions.ToMBList<IShipOrigin>(data.PlayerShips), data.EnemyParty, Extensions.ToMBList<IShipOrigin>(data.EnemyShips), isPlayerGeneral: true, data.SeasonId, data.TimeOfDay, data.WindStrength, data.WindDirection, data.Terrain);
	}

	public static NavalCustomBattleData PrepareBattleData(BasicCharacterObject playerCharacter, CustomBattleCombatant playerParty, List<IShipOrigin> playerShips, CustomBattleCombatant enemyParty, List<IShipOrigin> enemyShips, string scene, string season, float timeOfDay, float windStrength, NavalCustomBattleWindConfig.Direction windDirection, TerrainType terrain)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		return new NavalCustomBattleData
		{
			SceneId = scene,
			PlayerCharacter = playerCharacter,
			PlayerParty = playerParty,
			PlayerShips = playerShips,
			EnemyParty = enemyParty,
			EnemyShips = enemyShips,
			SeasonId = season,
			TimeOfDay = timeOfDay,
			WindStrength = windStrength,
			WindDirection = windDirection,
			Terrain = terrain
		};
	}

	public static CustomBattleCombatant[] GetCustomBattleParties(BasicCharacterObject playerCharacter, BasicCharacterObject enemyCharacter, List<BasicCharacterObject> remainingHeroes, BasicCultureObject playerFaction, int[] playerNumbers, List<BasicCharacterObject>[] playerTroopSelections, int playerShipCount, BasicCultureObject enemyFaction, int[] enemyNumbers, List<BasicCharacterObject>[] enemyTroopSelections, int enemyShipCount, bool isPlayerAttacker)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		object obj;
		if (playerFaction == null)
		{
			obj = null;
		}
		else
		{
			Banner banner = playerFaction.Banner;
			obj = ((banner != null) ? banner.BannerCode : null);
		}
		if (obj == null)
		{
			obj = string.Empty;
		}
		Banner val;
		if (Banner.IsValidBannerCode((string)obj))
		{
			val = new Banner(playerFaction.Banner, playerFaction.Color, playerFaction.Color2);
		}
		else
		{
			object obj2;
			if (playerFaction == null)
			{
				obj2 = null;
			}
			else
			{
				Banner banner2 = playerFaction.Banner;
				obj2 = ((banner2 != null) ? banner2.BannerCode : null);
			}
			Debug.FailedAssert("Banner code for player faction is not valid: " + (string?)obj2, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.CustomBattle\\CustomBattle\\NavalCustomBattleHelper.cs", "GetCustomBattleParties", 91);
			val = Banner.CreateOneColoredEmptyBanner(92);
		}
		object obj3;
		if (enemyFaction == null)
		{
			obj3 = null;
		}
		else
		{
			Banner banner3 = enemyFaction.Banner;
			obj3 = ((banner3 != null) ? banner3.BannerCode : null);
		}
		if (obj3 == null)
		{
			obj3 = string.Empty;
		}
		Banner val2;
		if (Banner.IsValidBannerCode((string)obj3))
		{
			val2 = new Banner(enemyFaction.Banner, enemyFaction.Color, enemyFaction.Color2);
		}
		else
		{
			object obj4;
			if (playerFaction == null)
			{
				obj4 = null;
			}
			else
			{
				Banner banner4 = playerFaction.Banner;
				obj4 = ((banner4 != null) ? banner4.BannerCode : null);
			}
			Debug.FailedAssert("Banner code for enemy faction is not valid: " + (string?)obj4, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.CustomBattle\\CustomBattle\\NavalCustomBattleHelper.cs", "GetCustomBattleParties", 101);
			val2 = Banner.CreateOneColoredEmptyBanner(92);
		}
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
		int num = playerShipCount - 1;
		int num2 = enemyShipCount - 1;
		array[0].Side = (BattleSideEnum)(isPlayerAttacker ? 1 : 0);
		array[0].AddCharacter(playerCharacter, 1);
		array[0].SetGeneral(playerCharacter);
		for (int i = 0; i < num; i++)
		{
			int index = MBRandom.RandomInt(0, remainingHeroes.Count);
			array[0].AddCharacter(remainingHeroes[index], 1);
			remainingHeroes.RemoveAt(index);
		}
		array[1].Side = Extensions.GetOppositeSide(array[0].Side);
		array[1].AddCharacter(enemyCharacter, 1);
		for (int j = 0; j < num2; j++)
		{
			int index2 = MBRandom.RandomInt(0, remainingHeroes.Count);
			array[1].AddCharacter(remainingHeroes[index2], 1);
			remainingHeroes.RemoveAt(index2);
		}
		for (int k = 0; k < array.Length; k++)
		{
			PopulateListsWithDefaults(ref array[k], (k == 0) ? playerNumbers : enemyNumbers, (k == 0) ? playerTroopSelections : enemyTroopSelections);
		}
		return array;
	}

	public static List<IShipOrigin>[] GetCustomBattleShipLists(List<IShipOrigin> playerShips, List<IShipOrigin> enemyShips)
	{
		List<IShipOrigin>[] array = new List<IShipOrigin>[2]
		{
			new List<IShipOrigin>(),
			new List<IShipOrigin>()
		};
		foreach (IShipOrigin playerShip in playerShips)
		{
			if (playerShip is CustomBattleShip customBattleShip)
			{
				array[0].Add((IShipOrigin)(object)customBattleShip.GetCopy());
			}
		}
		foreach (IShipOrigin enemyShip in enemyShips)
		{
			if (enemyShip is CustomBattleShip customBattleShip2)
			{
				array[1].Add((IShipOrigin)(object)customBattleShip2.GetCopy());
			}
		}
		return array;
	}

	private static void PopulateListsWithDefaults(ref CustomBattleCombatant customBattleParties, int[] numbers, List<BasicCharacterObject>[] troopList)
	{
		BasicCultureObject basicCulture = customBattleParties.BasicCulture;
		if (troopList == null)
		{
			troopList = new List<BasicCharacterObject>[2]
			{
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
		for (int i = 0; i < 2; i++)
		{
			int count = troopList[i].Count;
			int num = numbers[i];
			if (num <= 0)
			{
				continue;
			}
			float num2 = (float)num / (float)count;
			float num3 = 0f;
			for (int j = 0; j < count; j++)
			{
				float num4 = num2 + num3;
				int num5 = MathF.Floor(num4);
				num3 = num4 - (float)num5;
				customBattleParties.AddCharacter(troopList[i][j], num5);
				numbers[i] -= num5;
				if (j == count - 1 && numbers[i] > 0)
				{
					customBattleParties.AddCharacter(troopList[i][j], numbers[i]);
					numbers[i] = 0;
				}
			}
		}
	}

	public static int[] GetTroopCounts(int armySize, int shipCount, NavalCustomBattleCompositionData compositionData)
	{
		int[] array = new int[2];
		armySize -= shipCount;
		array[1] = MathF.Round(compositionData.RangedPercentage * (float)armySize);
		array[0] = armySize - array.Sum();
		return array;
	}

	private static BasicCharacterObject GetTroopFromId(string troopId)
	{
		return MBObjectManager.Instance.GetObject<BasicCharacterObject>(troopId);
	}

	public static BasicCharacterObject GetDefaultTroopOfFormationForFaction(BasicCultureObject culture, FormationClass formation)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I4
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Invalid comparison between Unknown and I4
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Invalid comparison between Unknown and I4
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Invalid comparison between Unknown and I4
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Invalid comparison between Unknown and I4
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Invalid comparison between Unknown and I4
		if (((MBObjectBase)culture).StringId.ToLower() == "empire")
		{
			if ((int)formation == 0)
			{
				return GetTroopFromId("imperial_veteran_infantryman");
			}
			if ((int)formation == 1)
			{
				return GetTroopFromId("imperial_archer");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "sturgia")
		{
			if ((int)formation == 0)
			{
				return GetTroopFromId("sturgian_spearman");
			}
			if ((int)formation == 1)
			{
				return GetTroopFromId("sturgian_archer");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "aserai")
		{
			if ((int)formation == 0)
			{
				return GetTroopFromId("aserai_infantry");
			}
			if ((int)formation == 1)
			{
				return GetTroopFromId("aserai_archer");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "vlandia")
		{
			if ((int)formation == 0)
			{
				return GetTroopFromId("vlandian_swordsman");
			}
			if ((int)formation == 1)
			{
				return GetTroopFromId("vlandian_hardened_crossbowman");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "battania")
		{
			if ((int)formation == 0)
			{
				return GetTroopFromId("battanian_picked_warrior");
			}
			if ((int)formation == 1)
			{
				return GetTroopFromId("battanian_hero");
			}
		}
		else if (((MBObjectBase)culture).StringId.ToLower() == "khuzait")
		{
			if ((int)formation == 0)
			{
				return GetTroopFromId("khuzait_spear_infantry");
			}
			if ((int)formation == 1)
			{
				return GetTroopFromId("khuzait_archer");
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
}
