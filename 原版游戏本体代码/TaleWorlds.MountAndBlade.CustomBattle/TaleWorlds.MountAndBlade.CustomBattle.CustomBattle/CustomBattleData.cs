using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;

namespace TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

public struct CustomBattleData
{
	public const int NumberOfAttackerMeleeMachines = 3;

	public const int NumberOfAttackerRangedMachines = 4;

	public const int NumberOfDefenderRangedMachines = 4;

	public const string CoreContentDefaultSceneName = "battle_terrain_029";

	public string GameTypeStringId;

	public string SceneId;

	public string SeasonId;

	public BasicCharacterObject PlayerCharacter;

	public BasicCharacterObject PlayerSideGeneralCharacter;

	public CustomBattleCombatant PlayerParty;

	public CustomBattleCombatant EnemyParty;

	public float TimeOfDay;

	public bool IsPlayerGeneral;

	public string SceneLevel;

	public List<MissionSiegeWeapon> AttackerMachines;

	public List<MissionSiegeWeapon> DefenderMachines;

	public float[] WallHitpointPercentages;

	public bool HasAnySiegeTower;

	public bool IsPlayerAttacker;

	public bool IsReliefAttack;

	public bool IsSallyOut;

	public int SceneUpgradeLevel;

	public static IEnumerable<Tuple<string, string>> GameTypes
	{
		get
		{
			yield return new Tuple<string, string>(((object)GameTexts.FindText("str_battle", (string)null)).ToString(), "Battle");
			if (!Module.CurrentModule.IsOnlyCoreContentEnabled)
			{
				yield return new Tuple<string, string>(((object)new TextObject("{=Ua6CNLBZ}Village", (Dictionary<string, object>)null)).ToString(), "Village");
				yield return new Tuple<string, string>(((object)GameTexts.FindText("str_siege", (string)null)).ToString(), "Siege");
			}
		}
	}

	public static IEnumerable<Tuple<string, CustomBattlePlayerType>> PlayerTypes
	{
		get
		{
			yield return new Tuple<string, CustomBattlePlayerType>(((object)GameTexts.FindText("str_team_commander", (string)null)).ToString(), CustomBattlePlayerType.Commander);
			if (!Module.CurrentModule.IsOnlyCoreContentEnabled)
			{
				yield return new Tuple<string, CustomBattlePlayerType>(((object)new TextObject("{=g9VIbA9s}Sergeant", (Dictionary<string, object>)null)).ToString(), CustomBattlePlayerType.Sergeant);
			}
		}
	}

	public static IEnumerable<Tuple<string, CustomBattlePlayerSide>> PlayerSides
	{
		get
		{
			yield return new Tuple<string, CustomBattlePlayerSide>(((object)new TextObject("{=XEVFUaFj}Defender", (Dictionary<string, object>)null)).ToString(), CustomBattlePlayerSide.Defender);
			yield return new Tuple<string, CustomBattlePlayerSide>(((object)new TextObject("{=KASD0tnO}Attacker", (Dictionary<string, object>)null)).ToString(), CustomBattlePlayerSide.Attacker);
		}
	}

	public static IEnumerable<BasicCharacterObject> Characters
	{
		get
		{
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_1");
			if (!Module.CurrentModule.IsOnlyCoreContentEnabled)
			{
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_2");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_3");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_4");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_5");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_6");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_7");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_8");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_9");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_10");
			}
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_11");
			if (!Module.CurrentModule.IsOnlyCoreContentEnabled)
			{
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_12");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_13");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_14");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_15");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_16");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_17");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_18");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_19");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_20");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_21");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_22");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_23");
				yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_24");
			}
		}
	}

	public static IEnumerable<BasicCultureObject> Factions
	{
		get
		{
			yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("empire");
			if (!Module.CurrentModule.IsOnlyCoreContentEnabled)
			{
				yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("sturgia");
				yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("aserai");
				yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("vlandia");
				yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("battania");
				yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("khuzait");
				if (ModuleHelper.IsModuleActive("NavalDLC"))
				{
					yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("nord");
				}
			}
		}
	}

	public static IEnumerable<Tuple<string, CustomBattleTimeOfDay>> TimesOfDay
	{
		get
		{
			yield return new Tuple<string, CustomBattleTimeOfDay>(((object)new TextObject("{=X3gcUz7C}Morning", (Dictionary<string, object>)null)).ToString(), CustomBattleTimeOfDay.Morning);
			yield return new Tuple<string, CustomBattleTimeOfDay>(((object)new TextObject("{=CTtjSwRb}Noon", (Dictionary<string, object>)null)).ToString(), CustomBattleTimeOfDay.Noon);
			yield return new Tuple<string, CustomBattleTimeOfDay>(((object)new TextObject("{=J2gvnexb}Afternoon", (Dictionary<string, object>)null)).ToString(), CustomBattleTimeOfDay.Afternoon);
			yield return new Tuple<string, CustomBattleTimeOfDay>(((object)new TextObject("{=gENb9SSW}Evening", (Dictionary<string, object>)null)).ToString(), CustomBattleTimeOfDay.Evening);
			yield return new Tuple<string, CustomBattleTimeOfDay>(((object)new TextObject("{=fAxjyMt5}Night", (Dictionary<string, object>)null)).ToString(), CustomBattleTimeOfDay.Night);
		}
	}

	public static IEnumerable<Tuple<string, string>> Seasons
	{
		get
		{
			yield return new Tuple<string, string>(((object)new TextObject("{=f7vOVQb7}Summer", (Dictionary<string, object>)null)).ToString(), "summer");
			yield return new Tuple<string, string>(((object)new TextObject("{=cZzfNlxd}Fall", (Dictionary<string, object>)null)).ToString(), "fall");
			yield return new Tuple<string, string>(((object)new TextObject("{=nwqUFaU8}Winter", (Dictionary<string, object>)null)).ToString(), "winter");
			yield return new Tuple<string, string>(((object)new TextObject("{=nWbp3o3H}Spring", (Dictionary<string, object>)null)).ToString(), "spring");
		}
	}

	public static IEnumerable<Tuple<string, int>> WallHitpoints
	{
		get
		{
			yield return new Tuple<string, int>(((object)new TextObject("{=dsMeB3vi}Solid", (Dictionary<string, object>)null)).ToString(), 0);
			yield return new Tuple<string, int>(((object)new TextObject("{=Kvxo2jzJ}Single Breached", (Dictionary<string, object>)null)).ToString(), 1);
			yield return new Tuple<string, int>(((object)new TextObject("{=AiNXIt5N}Dual Breached", (Dictionary<string, object>)null)).ToString(), 2);
		}
	}

	public static IEnumerable<int> SceneLevels
	{
		get
		{
			yield return 1;
			yield return 2;
			yield return 3;
		}
	}

	public static IEnumerable<SiegeEngineType> GetAllAttackerMeleeMachines()
	{
		yield return DefaultSiegeEngineTypes.Ram;
		yield return DefaultSiegeEngineTypes.SiegeTower;
	}

	public static IEnumerable<SiegeEngineType> GetAllDefenderRangedMachines()
	{
		yield return DefaultSiegeEngineTypes.Ballista;
		yield return DefaultSiegeEngineTypes.FireBallista;
		yield return DefaultSiegeEngineTypes.Catapult;
		yield return DefaultSiegeEngineTypes.FireCatapult;
	}

	public static IEnumerable<SiegeEngineType> GetAllAttackerRangedMachines()
	{
		yield return DefaultSiegeEngineTypes.Ballista;
		yield return DefaultSiegeEngineTypes.FireBallista;
		yield return DefaultSiegeEngineTypes.Onager;
		yield return DefaultSiegeEngineTypes.FireOnager;
		yield return DefaultSiegeEngineTypes.Trebuchet;
	}
}
