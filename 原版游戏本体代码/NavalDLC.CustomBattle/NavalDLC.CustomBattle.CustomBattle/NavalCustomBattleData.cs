using System;
using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.CustomBattle.CustomBattle;

public struct NavalCustomBattleData
{
	public string SceneId;

	public string SeasonId;

	public BasicCharacterObject PlayerCharacter;

	public CustomBattleCombatant PlayerParty;

	public CustomBattleCombatant EnemyParty;

	public List<IShipOrigin> PlayerShips;

	public List<IShipOrigin> EnemyShips;

	public float TimeOfDay;

	public float WindStrength;

	public NavalCustomBattleWindConfig.Direction WindDirection;

	public TerrainType Terrain;

	public static IEnumerable<Tuple<string, NavalCustomBattlePlayerSide>> PlayerSides
	{
		get
		{
			yield return new Tuple<string, NavalCustomBattlePlayerSide>(((object)new TextObject("{=KASD0tnO}Attacker", (Dictionary<string, object>)null)).ToString(), NavalCustomBattlePlayerSide.Attacker);
			yield return new Tuple<string, NavalCustomBattlePlayerSide>(((object)new TextObject("{=XEVFUaFj}Defender", (Dictionary<string, object>)null)).ToString(), NavalCustomBattlePlayerSide.Defender);
		}
	}

	public static IEnumerable<BasicCharacterObject> Characters
	{
		get
		{
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_1");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_2");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_3");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_4");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_5");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_6");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_7");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_8");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_9");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_10");
			yield return Game.Current.ObjectManager.GetObject<BasicCharacterObject>("commander_11");
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

	public static IEnumerable<BasicCultureObject> Factions
	{
		get
		{
			yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("empire");
			yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("sturgia");
			yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("aserai");
			yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("vlandia");
			yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("battania");
			yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("khuzait");
			yield return Game.Current.ObjectManager.GetObject<BasicCultureObject>("nord");
		}
	}

	public static IEnumerable<ShipHull> ShipHulls
	{
		get
		{
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("empire_medium_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("empire_heavy_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("aserai_heavy_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("nord_medium_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("northern_light_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("northern_trade_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("eastern_heavy_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("western_light_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("khuzait_heavy_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("eastern_medium_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("western_medium_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("vlandia_heavy_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("sturgia_heavy_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("northern_medium_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("battanian_light_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("western_trade_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("central_light_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("eastern_trade_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("empire_trade_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("nord_mediumballista_ship");
			yield return Game.Current.ObjectManager.GetObject<ShipHull>("battanian_medium_ship");
		}
	}

	public static IEnumerable<Tuple<string, NavalCustomBattleTimeOfDay>> TimesOfDay
	{
		get
		{
			yield return new Tuple<string, NavalCustomBattleTimeOfDay>(((object)new TextObject("{=X3gcUz7C}Morning", (Dictionary<string, object>)null)).ToString(), NavalCustomBattleTimeOfDay.Morning);
			yield return new Tuple<string, NavalCustomBattleTimeOfDay>(((object)new TextObject("{=CTtjSwRb}Noon", (Dictionary<string, object>)null)).ToString(), NavalCustomBattleTimeOfDay.Noon);
			yield return new Tuple<string, NavalCustomBattleTimeOfDay>(((object)new TextObject("{=J2gvnexb}Afternoon", (Dictionary<string, object>)null)).ToString(), NavalCustomBattleTimeOfDay.Afternoon);
			yield return new Tuple<string, NavalCustomBattleTimeOfDay>(((object)new TextObject("{=gENb9SSW}Evening", (Dictionary<string, object>)null)).ToString(), NavalCustomBattleTimeOfDay.Evening);
			yield return new Tuple<string, NavalCustomBattleTimeOfDay>(((object)new TextObject("{=fAxjyMt5}Night", (Dictionary<string, object>)null)).ToString(), NavalCustomBattleTimeOfDay.Night);
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

	public static IEnumerable<Tuple<string, float>> WindStrengths
	{
		get
		{
			yield return new Tuple<string, float>(((object)new TextObject("{=windstrengthweak}Weak", (Dictionary<string, object>)null)).ToString(), 0.4f);
			yield return new Tuple<string, float>(((object)new TextObject("{=windstrengthmild}Mild", (Dictionary<string, object>)null)).ToString(), 0.5f);
			yield return new Tuple<string, float>(((object)new TextObject("{=windstrengthstrong}Strong", (Dictionary<string, object>)null)).ToString(), 0.7f);
			yield return new Tuple<string, float>(((object)new TextObject("{=windstrengthstormy}Stormy", (Dictionary<string, object>)null)).ToString(), 1f);
		}
	}

	public static IEnumerable<Tuple<string, NavalCustomBattleWindConfig.Direction>> WindDirections
	{
		get
		{
			yield return new Tuple<string, NavalCustomBattleWindConfig.Direction>(((object)new TextObject("{=vz4kmcdI}Towards the Defender", (Dictionary<string, object>)null)).ToString(), NavalCustomBattleWindConfig.Direction.TowardsDefender);
			yield return new Tuple<string, NavalCustomBattleWindConfig.Direction>(((object)new TextObject("{=OjOsvTkT}Towards the Side", (Dictionary<string, object>)null)).ToString(), NavalCustomBattleWindConfig.Direction.Side);
			yield return new Tuple<string, NavalCustomBattleWindConfig.Direction>(((object)new TextObject("{=M0Fiya6u}Towards the Attacker", (Dictionary<string, object>)null)).ToString(), NavalCustomBattleWindConfig.Direction.TowardsAttacker);
			yield return new Tuple<string, NavalCustomBattleWindConfig.Direction>(((object)new TextObject("{=vBkrw5VV}Random", (Dictionary<string, object>)null)).ToString(), NavalCustomBattleWindConfig.Direction.Random);
		}
	}
}
