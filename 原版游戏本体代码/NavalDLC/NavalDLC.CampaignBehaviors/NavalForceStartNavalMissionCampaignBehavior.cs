using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.ComponentInterfaces;
using NavalDLC.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.CampaignBehaviors;

public class NavalForceStartNavalMissionCampaignBehavior : CampaignBehaviorBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConditionDelegate _003C_003E9__14_0;

		public static Func<MobileParty, bool> _003C_003E9__18_0;

		public static Func<Settlement, bool> _003C_003E9__18_1;

		public static Func<Ship, int> _003C_003E9__22_0;

		internal bool _003CAddGameMenus_003Eb__14_0(MenuCallbackArgs args)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			args.optionLeaveType = (LeaveType)1;
			return true;
		}

		internal bool _003CStartNavalMissionWithHandlingCheat_003Eb__18_0(MobileParty x)
		{
			return x.IsActive;
		}

		internal bool _003CStartNavalMissionWithHandlingCheat_003Eb__18_1(Settlement x)
		{
			return x.HasPort;
		}

		internal int _003CGetMaximumTroopCountForShipList_003Eb__22_0(Ship ship)
		{
			return ship.TotalCrewCapacity;
		}
	}

	private static bool _forceStartNavalMission = false;

	private const string DefaultTestSceneName = "battle_terrain_opensea_northern";

	private static string _sceneName = "battle_terrain_opensea_northern";

	private static int _enemyMeleeTroopCount = 30;

	private static int _enemyRangedTroopCount = 30;

	private static int _playerMeleeTroopCount = 30;

	private static int _playerRangedTroopCount = 30;

	private static bool _maximizeTroopCounts = true;

	private static MBList<string> _defaultShipHullIds;

	private static MBList<string>[] _shipHullIds;

	private PartyBase _enemyParty;

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)OnTick);
	}

	private void OnSessionLaunched(CampaignGameStarter starter)
	{
		AddGameMenus(starter);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void AddGameMenus(CampaignGameStarter starter)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		object obj = _003C_003Ec._003C_003E9__14_0;
		if (obj == null)
		{
			OnConditionDelegate val = delegate(MenuCallbackArgs args)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				args.optionLeaveType = (LeaveType)1;
				return true;
			};
			_003C_003Ec._003C_003E9__14_0 = val;
			obj = (object)val;
		}
		starter.AddGameMenuOption("encounter", "attack_naval", "{=!}Start Naval Mission (Cheat)", (OnConditionDelegate)obj, new OnConsequenceDelegate(StartNavalBattle), false, 2, false, (object)null);
	}

	private void OnTick(float dt)
	{
		if (_forceStartNavalMission && GameStateManager.Current.ActiveState is MapState)
		{
			StartNavalMissionFromCheats();
			_forceStartNavalMission = false;
		}
	}

	private void HealPartiesInPlayerEncounterCheat()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		foreach (MapEventParty item in (List<MapEventParty>)(object)MapEvent.PlayerMapEvent.PartiesOnSide(PlayerEncounter.Current.PlayerSide))
		{
			PartyBase party = item.Party;
			for (int i = 0; i < party.MemberRoster.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex = party.MemberRoster.GetElementCopyAtIndex(i);
				if (((BasicCharacterObject)elementCopyAtIndex.Character).IsHero)
				{
					elementCopyAtIndex.Character.HeroObject.Heal(elementCopyAtIndex.Character.HeroObject.MaxHitPoints, false);
				}
				else
				{
					party.AddToMemberRosterElementAtIndex(i, 0, -party.MemberRoster.GetElementWoundedNumber(i));
				}
			}
		}
		foreach (MapEventParty item2 in (List<MapEventParty>)(object)MapEvent.PlayerMapEvent.PartiesOnSide(PlayerEncounter.Current.OpponentSide))
		{
			PartyBase party2 = item2.Party;
			for (int j = 0; j < party2.MemberRoster.Count; j++)
			{
				TroopRosterElement elementCopyAtIndex2 = party2.MemberRoster.GetElementCopyAtIndex(j);
				if (((BasicCharacterObject)elementCopyAtIndex2.Character).IsHero)
				{
					elementCopyAtIndex2.Character.HeroObject.Heal(elementCopyAtIndex2.Character.HeroObject.MaxHitPoints, false);
				}
				else
				{
					party2.AddToMemberRosterElementAtIndex(j, 0, -party2.MemberRoster.GetElementWoundedNumber(j));
				}
			}
		}
	}

	private void StartNavalMissionFromCheats()
	{
		StartNavalMissionWithHandlingCheat();
	}

	private void StartNavalMissionWithHandlingCheat()
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Expected I4, but got Unknown
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		PartyBase mainParty = PartyBase.MainParty;
		if (PlayerEncounter.Current == null)
		{
			SetupTeamForEncounterCheat((TeamSideEnum)0, mainParty);
			if (_enemyParty == null)
			{
				IEnumerable<MobileParty> enumerable = ((IEnumerable<MobileParty>)MobileParty.AllLordParties).Where((MobileParty x) => x.IsActive);
				_enemyParty = Extensions.GetRandomElementInefficiently<MobileParty>(enumerable).Party;
				_enemyParty.MemberRoster.Clear();
			}
			SetupTeamForEncounterCheat((TeamSideEnum)2, _enemyParty);
			if (_enemyParty.Position.IsOnLand)
			{
				CampaignVec2 position = NavigationHelper.FindPointAroundPosition(Extensions.GetRandomElementInefficiently<Settlement>(((IEnumerable<Settlement>)Campaign.Current.Settlements).Where((Settlement x) => x.HasPort)).PortPosition, (NavigationType)2, 10f, 1f, true, false);
				_enemyParty.MobileParty.Position = position;
			}
			PlayerEncounter.RestartPlayerEncounter(_enemyParty, mainParty, true);
		}
		else if (_enemyParty == null)
		{
			_enemyParty = ((List<MapEventParty>)(object)MapEvent.PlayerMapEvent.PartiesOnSide(PlayerEncounter.Current.OpponentSide))[0].Party;
		}
		if (((List<Ship>)(object)mainParty.Ships).Count == 0)
		{
			AddShipsToTeamPartyForEncounterCheat((TeamSideEnum)0, mainParty);
		}
		if (((List<Ship>)(object)_enemyParty.Ships).Count == 0)
		{
			AddShipsToTeamPartyForEncounterCheat((TeamSideEnum)2, _enemyParty);
		}
		if (mainParty.MemberRoster.TotalManCount == 1)
		{
			AddTroopsToTeamPartyForEncounterCheat((TeamSideEnum)0, mainParty);
		}
		if (_enemyParty.MemberRoster.TotalManCount == 0)
		{
			AddTroopsToTeamPartyForEncounterCheat((TeamSideEnum)2, _enemyParty);
		}
		if (_enemyParty.Position.IsOnLand != mainParty.Position.IsOnLand)
		{
			if (_enemyParty.Position.IsOnLand)
			{
				_enemyParty.MobileParty.Position = mainParty.Position;
			}
			else
			{
				mainParty.MobileParty.Position = _enemyParty.Position;
			}
		}
		if (!_enemyParty.MapFaction.IsAtWarWith(Clan.PlayerClan.MapFaction))
		{
			DeclareWarAction.ApplyByDefault(_enemyParty.MapFaction, Clan.PlayerClan.MapFaction);
		}
		if (PlayerEncounter.Battle == null)
		{
			PlayerEncounter.StartBattle();
		}
		HealPartiesInPlayerEncounterCheat();
		string text = ((!string.IsNullOrEmpty(_sceneName)) ? _sceneName : "battle_terrain_opensea_northern");
		MissionInitializerRecord rec = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref rec))._002Ector(text);
		TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
		rec.TerrainType = (int)faceTerrainType;
		rec.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		rec.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		rec.NeedsRandomTerrain = false;
		rec.PlayingInCampaignMode = true;
		rec.RandomTerrainSeed = MBRandom.RandomInt(10000);
		rec.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position);
		rec.SceneHasMapPatch = false;
		rec.DecalAtlasGroup = 2;
		NavalMissions.OpenNavalBattleMission(rec);
	}

	private void SetupTeamForEncounterCheat(TeamSideEnum teamSide, PartyBase teamParty)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)teamParty.MemberRoster.GetTroopRoster())
		{
			TroopRosterElement current = item;
			if (current.Character != CharacterObject.PlayerCharacter)
			{
				teamParty.MemberRoster.RemoveTroop(current.Character, ((TroopRosterElement)(ref current)).Number, default(UniqueTroopDescriptor), 0);
			}
		}
		foreach (Ship item2 in ((IEnumerable<Ship>)teamParty.Ships).ToList())
		{
			DestroyShipAction.Apply(item2);
		}
		AddShipsToTeamPartyForEncounterCheat(teamSide, teamParty);
		AddTroopsToTeamPartyForEncounterCheat(teamSide, teamParty);
	}

	private static void AddTroopsToTeamPartyForEncounterCheat(TeamSideEnum teamSide, PartyBase teamParty)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		int maxMeleeTroopCount = 0;
		int maxRangedTroopCount = 0;
		if (_maximizeTroopCounts)
		{
			GetMaximumTroopCountForShipList(teamParty.Ships, out maxMeleeTroopCount, out maxRangedTroopCount);
		}
		else if ((int)teamSide == 0)
		{
			maxMeleeTroopCount = _playerMeleeTroopCount;
			maxRangedTroopCount = _playerRangedTroopCount;
		}
		else if ((int)teamSide == 2)
		{
			maxMeleeTroopCount = _enemyMeleeTroopCount;
			maxRangedTroopCount = _enemyRangedTroopCount;
		}
		else
		{
			Debug.FailedAssert("This team side is not currently supported", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\CampaignBehaviors\\NavalForceStartNavalMissionCampaignBehavior.cs", "AddTroopsToTeamPartyForEncounterCheat", 287);
		}
		teamParty.MemberRoster.AddToCounts(MBObjectManager.Instance.GetObject<CharacterObject>("imperial_recruit"), maxMeleeTroopCount, false, 0, 0, true, -1);
		teamParty.MemberRoster.AddToCounts(MBObjectManager.Instance.GetObject<CharacterObject>("imperial_archer"), maxRangedTroopCount, false, 0, 0, true, -1);
	}

	private static MBList<Ship> AddShipsToTeamPartyForEncounterCheat(TeamSideEnum teamSide, PartyBase teamParty)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		MBList<Ship> defaultShipSet = GetDefaultShipSet(teamSide);
		foreach (Ship item in (List<Ship>)(object)defaultShipSet)
		{
			ChangeShipOwnerAction.ApplyByLooting(teamParty, item);
		}
		return defaultShipSet;
	}

	private static void GetMaximumTroopCountForShipList(MBReadOnlyList<Ship> shipList, out int maxMeleeTroopCount, out int maxRangedTroopCount)
	{
		int num = ((IEnumerable<Ship>)shipList).Sum((Ship ship) => ship.TotalCrewCapacity);
		maxRangedTroopCount = num / 2;
		maxMeleeTroopCount = num - _playerRangedTroopCount;
	}

	private void StartNavalBattle(MenuCallbackArgs args)
	{
		StartNavalMissionWithHandlingCheat();
	}

	private static Ship CreateShip(string shipHullId)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		ShipHull val = MBObjectManager.Instance.GetObject<ShipHull>(shipHullId);
		if (val != null)
		{
			return new Ship(val);
		}
		return null;
	}

	private static MBList<Ship> GetDefaultShipSet(TeamSideEnum teamSide)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		MBList<Ship> val = new MBList<Ship>();
		foreach (string item2 in (List<string>)(object)_shipHullIds[teamSide])
		{
			Ship item = CreateShip(item2);
			((List<Ship>)(object)val).Add(item);
		}
		return val;
	}

	private static string GetMissionSettings()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		string text = "Scene Name: " + _sceneName + "\nTroop Counts Maximized: " + _maximizeTroopCounts;
		if (!_maximizeTroopCounts)
		{
			text = text + "\nPlayer Melee Troop Count: " + _playerMeleeTroopCount + "\nPlayer Ranged Troop Count: " + _playerRangedTroopCount + "\nEnemy Melee Troop Count: " + _enemyMeleeTroopCount + "\nEnemy Ranged Troop Count: " + _enemyRangedTroopCount;
		}
		for (int i = 0; i < _shipHullIds.Length; i++)
		{
			MBList<string> val = _shipHullIds[i];
			if (!Extensions.IsEmpty<string>((IEnumerable<string>)val))
			{
				text = text + "\n" + ((object)(TeamSideEnum)i/*cast due to .constrained prefix*/).ToString() + " Mission Ships:";
				int num = ((List<string>)(object)val).Count - 1;
				for (int j = 0; j < num; j++)
				{
					text = text + ((List<string>)(object)val)[j] + ", ";
				}
				text += ((List<string>)(object)val)[num];
			}
		}
		return text;
	}

	private static void ResetMissionSettings()
	{
		_sceneName = "battle_terrain_opensea_northern";
		_maximizeTroopCounts = false;
		_playerMeleeTroopCount = 30;
		_playerRangedTroopCount = 30;
		_enemyMeleeTroopCount = 30;
		_enemyRangedTroopCount = 30;
		ResetShipHullsToDefault();
	}

	private static void ResetShipHullsToDefault()
	{
		((List<string>)(object)_shipHullIds[0]).Clear();
		((List<string>)(object)_shipHullIds[0]).AddRange((IEnumerable<string>)_defaultShipHullIds);
		((List<string>)(object)_shipHullIds[1]).Clear();
		((List<string>)(object)_shipHullIds[2]).Clear();
		((List<string>)(object)_shipHullIds[2]).AddRange((IEnumerable<string>)_defaultShipHullIds);
	}

	[CommandLineArgumentFunction("get_mission_settings", "naval")]
	public static string GetMissionSettings(List<string> strings)
	{
		return GetMissionSettings();
	}

	[CommandLineArgumentFunction("reset_mission_settings", "naval")]
	public static string ResetMissionSettings(List<string> strings)
	{
		ResetMissionSettings();
		return "Mission settings reset successfully.\n" + GetMissionSettings();
	}

	[CommandLineArgumentFunction("set_mission_scene", "naval")]
	public static string SetMissionScene(List<string> strings)
	{
		if (strings.Count == 1)
		{
			_sceneName = strings[0];
			return "Mission scene is set to " + _sceneName;
		}
		return "usage: naval.set_mission_scene [SceneName]";
	}

	[CommandLineArgumentFunction("set_mission_ships", "naval")]
	public unsafe static string SetMissionShips(List<string> strings)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected I4, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Invalid comparison between Unknown and I4
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Expected I4, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Invalid comparison between Unknown and I4
		bool flag = false;
		string text = "";
		TeamSideEnum val = (TeamSideEnum)(-1);
		if (strings.Count == 0)
		{
			text += "Invalid number of arguments provided\n";
			flag = true;
		}
		if (strings.Count == 1)
		{
			string text2 = strings[0];
			if (text2.ToLower() == "help")
			{
				flag = true;
			}
			else if (text2.ToLower() == "default")
			{
				val = (TeamSideEnum)3;
				ResetShipHullsToDefault();
			}
			else
			{
				text += "Unable to parse single parameter argument.\nFor single parameter calls, the parameter must either be \"default\" or \"help\"\n";
				flag = true;
			}
		}
		else
		{
			switch (strings[0].ToLower())
			{
			case "player":
			case "playerTeam":
				val = (TeamSideEnum)0;
				break;
			case "playerAlly":
			case "playerAllyTeam":
				val = (TeamSideEnum)1;
				break;
			case "enemy":
			case "enemyTeam":
				val = (TeamSideEnum)2;
				break;
			}
			if (TeamSideEnumExtensions.IsValid(val))
			{
				int num = (int)val;
				MBList<string> val2 = _shipHullIds[num];
				if (strings.Count == 2 && strings[1].ToLower() == "default")
				{
					((List<string>)(object)val2).Clear();
					((List<string>)(object)val2).AddRange((IEnumerable<string>)_defaultShipHullIds);
				}
				else
				{
					((List<string>)(object)val2).Clear();
					int num2 = strings.Count - 1;
					if (num2 > 8)
					{
						text += "At most 8 ships hull ids can be passed as parameter\n";
						num2 = 8;
					}
					for (int i = 0; i < num2; i++)
					{
						string text3 = strings[i + 1];
						MBObjectManager instance = MBObjectManager.Instance;
						if (instance != null)
						{
							if (instance.GetObject<ShipHull>(text3) != null)
							{
								((List<string>)(object)val2).Add(text3);
							}
							else
							{
								text = text + "Passed ship hull id: " + text3 + " does not refer to a valid ship hull. Omitting this\n";
							}
						}
						else
						{
							((List<string>)(object)val2).Add(text3);
						}
					}
					if (Extensions.IsEmpty<string>((IEnumerable<string>)val2))
					{
						text += "None of the passed ship hull ids refer to a valid ship hull\n";
						text = text + "Reverting to default ship hulls for " + ((object)(*(TeamSideEnum*)(&val))/*cast due to .constrained prefix*/).ToString().ToLower() + "\n";
						if ((int)val != 1)
						{
							((List<string>)(object)val2).AddRange((IEnumerable<string>)_defaultShipHullIds);
						}
					}
				}
			}
			else
			{
				text += "Unable to parse team side argument\nIt must refer to a valid team side like \"player\",\"playerAlly\" or \"enemy\"\n";
				flag = true;
			}
		}
		if (flag)
		{
			text += "Mission will be loaded with the specified ship hulls for the given team\n\nUsage: naval.set_mission_ships [TeamSide] [ShipHullId0] [ShipHullId1] ...\n\n- TeamSide: is the side of the team for which starting ships will be changed.\n  Can be \"player\", \"playerAlly\" or \"enemy\"\n- ShipHullId(s): are the hull id(s) of the ships to be spawned for the given side.\n  These must exist in ShipHulls.xml file.\n\nRemarks: Passing \"default\" as the first parameter will reset ships to default for all teams\n          Passing \"default\" as the second parameter after the TeamSide parameter will set ships to default\n         for only the given team";
		}
		else if ((int)val == 3)
		{
			text += "Player and enemy teams will start with their default ships:\n";
			int num3 = ((List<string>)(object)_defaultShipHullIds).Count - 1;
			for (int j = 0; j < num3; j++)
			{
				text = text + ((List<string>)(object)_defaultShipHullIds)[j] + ", ";
			}
			text = text + ((List<string>)(object)_defaultShipHullIds)[num3] + "\n";
		}
		else if (TeamSideEnumExtensions.IsValid(val))
		{
			int num4 = (int)val;
			text = text + ((object)(*(TeamSideEnum*)(&val))/*cast due to .constrained prefix*/).ToString() + " will use the following ships:\n";
			MBList<string> val3 = _shipHullIds[num4];
			int num5 = ((List<string>)(object)val3).Count - 1;
			for (int k = 0; k < num5; k++)
			{
				text = text + ((List<string>)(object)val3)[k] + ", ";
			}
			text = text + ((List<string>)(object)val3)[num5] + "\n";
		}
		return text;
	}

	[CommandLineArgumentFunction("set_maximize_troop_counts", "naval")]
	public static string SetMaximizeTroopCounts(List<string> strings)
	{
		bool flag = false;
		string text = "";
		if (strings.Count == 1)
		{
			if (strings[0].ToLower() == "help")
			{
				flag = true;
			}
			else if (strings[0] == "1" || strings[0] == "0")
			{
				_maximizeTroopCounts = strings[0] == "1";
			}
			else
			{
				text = "Unable to parse parameter.\n";
				flag = true;
			}
		}
		else
		{
			_maximizeTroopCounts = !_maximizeTroopCounts;
		}
		if (flag)
		{
			return text + "\nIf set, mission will start with all ships having maximum number of troops\nusage: naval.set_maximize_troop_counts [value]\n- value: If passed 1 setting is enabled, if passed 0 it is disabled. Omitting the parameter toggles the setting";
		}
		if (_maximizeTroopCounts)
		{
			return text + "Troops counts will be maximized in next mission";
		}
		return text + "Troops counts will be specified manually in next mission\n- Player Melee Troop Count:" + _playerMeleeTroopCount + "\n- Player Ranged Troop Count:" + _playerRangedTroopCount + "\n- Enemy Melee Troop Count:" + _enemyMeleeTroopCount + "\n- Enemy Ranged Troop Count:" + _enemyRangedTroopCount;
	}

	[CommandLineArgumentFunction("set_mission_troop_counts", "naval")]
	public static string SetMissionTroopCounts(List<string> strings)
	{
		string text = "";
		bool flag = false;
		if (strings.Count == 1 && strings[0].ToLower() == "help")
		{
			flag = true;
		}
		else if (strings.Count == 4 && int.TryParse(strings[0] ?? "error", out _playerMeleeTroopCount) && int.TryParse(strings[1] ?? "error", out _playerRangedTroopCount) && int.TryParse(strings[2] ?? "error", out _enemyMeleeTroopCount) && int.TryParse(strings[3] ?? "error", out _enemyRangedTroopCount))
		{
			if (_maximizeTroopCounts)
			{
				_maximizeTroopCounts = false;
				text += "Troop count maximization disabled\n";
			}
			text = text + "Mission troop counts are successfully set.\n- Player Melee Troop Count:" + _playerMeleeTroopCount + "\n- Player Ranged Troop Count:" + _playerRangedTroopCount + "\n- Enemy Melee Troop Count:" + _enemyMeleeTroopCount + "\n- Enemy Ranged Troop Count:" + _enemyRangedTroopCount;
		}
		else
		{
			text += "Unable to parse one or more of the parameters.\n";
			flag = true;
		}
		if (flag)
		{
			text += "usage: naval.set_mission_troop_counts [PlayerMeleeTroopCount] [PlayerRangedTroopCount] [EnemyMeleeTroopCount] [EnemyRangedTroopCount]";
		}
		return text;
	}

	[CommandLineArgumentFunction("start_mission", "naval")]
	public static string StartMission(List<string> strings)
	{
		if (!_forceStartNavalMission)
		{
			_forceStartNavalMission = true;
			ShipDeploymentModel.IgnoreDeploymentLimits = true;
			if (GameStateManager.Current.ActiveState is InitialState)
			{
				Module.CurrentModule.ExecuteInitialStateOptionWithId("SandBoxNewGame");
			}
			else
			{
				ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo("NavalDLC");
				if (moduleInfo == null || !moduleInfo.IsActive)
				{
					_forceStartNavalMission = false;
					return "Naval DLC module isn't active!";
				}
				Campaign.Current.TimeControlMode = (CampaignTimeControlMode)2;
			}
		}
		return "Starting mission with current mission settings...\n" + GetMissionSettings();
	}

	static NavalForceStartNavalMissionCampaignBehavior()
	{
		MBList<string> obj = new MBList<string>();
		((List<string>)(object)obj).Add("northern_trade_ship");
		((List<string>)(object)obj).Add("nord_medium_ship");
		((List<string>)(object)obj).Add("vlandia_heavy_ship");
		_defaultShipHullIds = obj;
		_shipHullIds = new MBList<string>[3]
		{
			new MBList<string>((List<string>)(object)_defaultShipHullIds),
			new MBList<string>(),
			new MBList<string>((List<string>)(object)_defaultShipHullIds)
		};
	}
}
