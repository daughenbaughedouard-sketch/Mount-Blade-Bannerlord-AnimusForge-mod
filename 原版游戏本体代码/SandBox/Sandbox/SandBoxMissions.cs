using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions;
using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionEvents;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.MissionLogics.Arena;
using SandBox.Missions.MissionLogics.Hideout;
using SandBox.Missions.MissionLogics.Towns;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.TroopSuppliers;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;
using TaleWorlds.ObjectSystem;

namespace SandBox;

[MissionManager]
public static class SandBoxMissions
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<MapEventParty, bool> _003C_003E9__15_0;

		public static Func<MapEventParty, bool> _003C_003E9__16_0;

		public static Func<PartyBase, bool> _003C_003E9__16_2;

		public static Func<PartyBase, int> _003C_003E9__16_3;

		public static Func<PartyBase, bool> _003C_003E9__16_4;

		public static Func<PartyBase, int> _003C_003E9__16_5;

		public static Func<PartyBase, bool> _003C_003E9__16_6;

		public static Predicate<FlattenedTroopRosterElement> _003C_003E9__24_0;

		public static InitializeMissionBehaviorsDelegate _003C_003E9__25_0;

		public static InitializeMissionBehaviorsDelegate _003C_003E9__31_0;

		internal bool _003COpenBattleMission_003Eb__15_0(MapEventParty p)
		{
			return p.Party == MobileParty.MainParty.Party;
		}

		internal bool _003COpenCaravanBattleMission_003Eb__16_0(MapEventParty p)
		{
			return p.Party == MobileParty.MainParty.Party;
		}

		internal bool _003COpenCaravanBattleMission_003Eb__16_2(PartyBase ip)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)ip.Side == 1;
		}

		internal int _003COpenCaravanBattleMission_003Eb__16_3(PartyBase ip)
		{
			return ip.MobileParty.Party.MemberRoster.TotalManCount - ip.MobileParty.Party.MemberRoster.TotalWounded;
		}

		internal bool _003COpenCaravanBattleMission_003Eb__16_4(PartyBase ip)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)ip.Side == 0;
		}

		internal int _003COpenCaravanBattleMission_003Eb__16_5(PartyBase ip)
		{
			return ip.MobileParty.Party.MemberRoster.TotalManCount - ip.MobileParty.Party.MemberRoster.TotalWounded;
		}

		internal bool _003COpenCaravanBattleMission_003Eb__16_6(PartyBase ip)
		{
			if (ip.MobileParty.IsCaravan || ip.MobileParty.IsVillager)
			{
				if (!(((MBObjectBase)ip.Culture).StringId == "aserai"))
				{
					return ((MBObjectBase)ip.Culture).StringId == "khuzait";
				}
				return true;
			}
			return false;
		}

		internal bool _003COpenHideoutAmbushMission_003Eb__24_0(FlattenedTroopRosterElement x)
		{
			return ((BasicCharacterObject)((FlattenedTroopRosterElement)(ref x)).Troop).IsPlayerCharacter;
		}

		internal IEnumerable<MissionBehavior> _003COpenCampMission_003Eb__25_0(Mission mission)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Expected O, but got Unknown
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[9]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)0, false),
				(MissionBehavior)new BasicLeaveMissionLogic(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new EquipmentControllerLeaveLogic()
			};
		}

		internal IEnumerable<MissionBehavior> _003COpenMeetingMission_003Eb__31_0(Mission mission)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[5]
			{
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new MissionSettlementPrepareLogic(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionConversationLogic(),
				(MissionBehavior)new EquipmentControllerLeaveLogic()
			};
		}
	}

	public static MissionInitializerRecord CreateSandBoxMissionInitializerRecord(string sceneName, string sceneLevels, bool doNotUseLoadingScreen, DecalAtlasGroup decalAtlasGroup)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected I4, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		MissionInitializerRecord result = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref result))._002Ector(sceneName);
		result.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		result.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		result.PlayingInCampaignMode = (int)Campaign.Current.GameMode == 1;
		result.AtmosphereOnCampaign = (((int)Campaign.Current.GameMode == 1) ? Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position) : AtmosphereInfo.GetInvalidAtmosphereInfo());
		result.TerrainType = ((Campaign.Current.MapSceneWrapper != null) ? ((int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace)) : 0);
		result.SceneLevels = sceneLevels;
		result.DoNotUseLoadingScreen = doNotUseLoadingScreen;
		result.DecalAtlasGroup = (int)decalAtlasGroup;
		return result;
	}

	public static MissionInitializerRecord CreateSandBoxTrainingMissionInitializerRecord(string sceneName, string sceneLevels = "", bool doNotUseLoadingScreen = false)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected I4, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		MissionInitializerRecord result = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref result))._002Ector(sceneName);
		result.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		result.DamageFromPlayerToFriendsMultiplier = 1f;
		result.PlayingInCampaignMode = (int)Campaign.Current.GameMode == 1;
		result.AtmosphereOnCampaign = (((int)Campaign.Current.GameMode == 1) ? Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position) : AtmosphereInfo.GetInvalidAtmosphereInfo());
		result.TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
		result.SceneLevels = sceneLevels;
		result.DoNotUseLoadingScreen = doNotUseLoadingScreen;
		result.DecalAtlasGroup = 3;
		return result;
	}

	[MissionMethod]
	public static Mission OpenTownCenterMission(string scene, int townUpgradeLevel, Location location, CharacterObject talkToChar, string playerSpawnTag)
	{
		string civilianUpgradeLevelTag = Campaign.Current.Models.LocationModel.GetCivilianUpgradeLevelTag(townUpgradeLevel);
		return OpenTownCenterMission(scene, civilianUpgradeLevelTag, location, talkToChar, playerSpawnTag);
	}

	[MissionMethod]
	public static Mission OpenTownCenterMission(string scene, string sceneLevels, Location location, CharacterObject talkToChar, string playerSpawnTag)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		return MissionState.OpenNew("TownCenter", CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[28]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new MissionBasicTeamLogic(),
			(MissionBehavior)new MissionSettlementPrepareLogic(),
			(MissionBehavior)new TownCenterMissionController(),
			(MissionBehavior)new MissionAgentLookHandler(),
			(MissionBehavior)new SandBoxMissionHandler(),
			(MissionBehavior)new WorkshopMissionHandler(GetCurrentTown()),
			(MissionBehavior)new BasicLeaveMissionLogic(),
			(MissionBehavior)new LeaveMissionLogic(),
			(MissionBehavior)new BattleAgentLogic(),
			(MissionBehavior)new MountAgentLogic(),
			(MissionBehavior)new NotableSpawnPointHandler(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new MissionAlleyHandler(),
			(MissionBehavior)new MissionCrimeHandler(),
			(MissionBehavior)new MissionConversationLogic(talkToChar),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new MissionLocationLogic(location, playerSpawnTag),
			(MissionBehavior)new HeroSkillHandler(),
			(MissionBehavior)new MissionFightHandler(),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new MissionHardBorderPlacer(),
			(MissionBehavior)new MissionBoundaryPlacer(),
			(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
			(MissionBehavior)new VisualTrackerMissionBehavior(),
			(MissionBehavior)new EquipmentControllerLeaveLogic()
		}), true, true);
	}

	[MissionMethod]
	public static Mission OpenTownCenterShadowATargetMission(string scene, string sceneLevels, Location location, CharacterObject talkToChar, string playerSpawnTag)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		return MissionState.OpenNew("TownCenter", CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[27]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new MissionBasicTeamLogic(),
			(MissionBehavior)new MissionSettlementPrepareLogic(),
			(MissionBehavior)new TownCenterMissionController(),
			(MissionBehavior)new MissionAgentLookHandler(),
			(MissionBehavior)new SandBoxMissionHandler(),
			(MissionBehavior)new WorkshopMissionHandler(GetCurrentTown()),
			(MissionBehavior)new BasicLeaveMissionLogic(),
			(MissionBehavior)new LeaveMissionLogic(),
			(MissionBehavior)new BattleAgentLogic(),
			(MissionBehavior)new MountAgentLogic(),
			(MissionBehavior)new NotableSpawnPointHandler(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new MissionAlleyHandler(),
			(MissionBehavior)new MissionCrimeHandler(),
			(MissionBehavior)new MissionConversationLogic(talkToChar),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new HeroSkillHandler(),
			(MissionBehavior)new MissionFightHandler(),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new MissionHardBorderPlacer(),
			(MissionBehavior)new MissionBoundaryPlacer(),
			(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
			(MissionBehavior)new VisualTrackerMissionBehavior(),
			(MissionBehavior)new EquipmentControllerLeaveLogic()
		}), true, true);
	}

	[MissionMethod]
	public static Mission OpenCastleCourtyardMission(string scene, int castleUpgradeLevel, Location location, CharacterObject talkToChar)
	{
		string civilianUpgradeLevelTag = Campaign.Current.Models.LocationModel.GetCivilianUpgradeLevelTag(castleUpgradeLevel);
		return OpenCastleCourtyardMission(scene, civilianUpgradeLevelTag, location, talkToChar);
	}

	[MissionMethod]
	public static Mission OpenCastleCourtyardMission(string scene, string sceneLevels, Location location, CharacterObject talkToChar)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		return MissionState.OpenNew("TownCenter", CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Expected O, but got Unknown
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Expected O, but got Unknown
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Expected O, but got Unknown
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Expected O, but got Unknown
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Expected O, but got Unknown
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Expected O, but got Unknown
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Expected O, but got Unknown
			List<MissionBehavior> list = new List<MissionBehavior>
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)(object)new CampaignMissionComponent(),
				(MissionBehavior)(object)new MissionBasicTeamLogic(),
				(MissionBehavior)(object)new MissionSettlementPrepareLogic(),
				(MissionBehavior)(object)new TownCenterMissionController(),
				(MissionBehavior)(object)new MissionAgentLookHandler(),
				(MissionBehavior)(object)new SandBoxMissionHandler(),
				(MissionBehavior)new BasicLeaveMissionLogic(),
				(MissionBehavior)(object)new LeaveMissionLogic(),
				(MissionBehavior)(object)new BattleAgentLogic(),
				(MissionBehavior)(object)new MountAgentLogic()
			};
			Settlement currentTown = GetCurrentTown();
			if (currentTown != null)
			{
				list.Add((MissionBehavior)(object)new WorkshopMissionHandler(currentTown));
			}
			list.Add((MissionBehavior)new MissionAgentPanicHandler());
			list.Add((MissionBehavior)new AgentHumanAILogic());
			list.Add((MissionBehavior)(object)new MissionConversationLogic(talkToChar));
			list.Add((MissionBehavior)(object)new MissionAgentHandler());
			list.Add((MissionBehavior)(object)new MissionLocationLogic(location));
			list.Add((MissionBehavior)(object)new HeroSkillHandler());
			list.Add((MissionBehavior)(object)new MissionFightHandler());
			list.Add((MissionBehavior)new MissionFacialAnimationHandler());
			list.Add((MissionBehavior)new MissionHardBorderPlacer());
			list.Add((MissionBehavior)new MissionBoundaryPlacer());
			list.Add((MissionBehavior)new EquipmentControllerLeaveLogic());
			list.Add((MissionBehavior)new MissionBoundaryCrossingHandler(10f));
			list.Add((MissionBehavior)(object)new VisualTrackerMissionBehavior());
			return list.ToArray();
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenIndoorMission(string scene, int townUpgradeLevel, Location location, CharacterObject talkToChar)
	{
		string civilianUpgradeLevelTag = Campaign.Current.Models.LocationModel.GetCivilianUpgradeLevelTag(townUpgradeLevel);
		return OpenIndoorMission(scene, location, talkToChar, civilianUpgradeLevelTag);
	}

	[MissionMethod]
	public static Mission OpenIndoorMission(string scene, Location location, CharacterObject talkToChar = null, string sceneLevels = "")
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		List<Ship> mainPartyShips = new List<Ship>();
		List<Ship> townLordShips = new List<Ship>();
		if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsTown && location.StringId == "port")
		{
			mainPartyShips = ((IEnumerable<Ship>)MobileParty.MainParty.Ships).ToList();
			foreach (MobileParty item in (List<MobileParty>)(object)Settlement.CurrentSettlement.Parties)
			{
				townLordShips.AddRange((IEnumerable<Ship>)item.Ships);
			}
		}
		return MissionState.OpenNew("Indoor", CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: true, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[23]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new MissionBasicTeamLogic(),
			(MissionBehavior)new BasicLeaveMissionLogic(),
			(MissionBehavior)new LeaveMissionLogic(),
			(MissionBehavior)new SandBoxMissionHandler(),
			(MissionBehavior)new MissionAgentLookHandler(),
			(MissionBehavior)new MissionConversationLogic(talkToChar),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new MissionLocationLogic(location),
			(MissionBehavior)new HeroSkillHandler(),
			(MissionBehavior)new MissionFightHandler(),
			(MissionBehavior)new BattleAgentLogic(),
			(MissionBehavior)new MountAgentLogic(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new MissionCrimeHandler(),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new LocationItemSpawnHandler(),
			(MissionBehavior)new IndoorMissionController(),
			(MissionBehavior)new VisualTrackerMissionBehavior(),
			(MissionBehavior)new EquipmentControllerLeaveLogic(),
			(MissionBehavior)new BattleSurgeonLogic(),
			(MissionBehavior)new CivilianPortShipSpawnMissionLogic(mainPartyShips, townLordShips)
		}), true, true);
	}

	[MissionMethod]
	public static Mission OpenPrisonBreakMission(string scene, Location location, CharacterObject prisonerCharacter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		MissionInitializerRecord val = CreateSandBoxMissionInitializerRecord(scene, "prison_break", doNotUseLoadingScreen: true, (DecalAtlasGroup)3);
		val.DisableCorpseFadeOut = true;
		Mission obj = MissionState.OpenNew("PrisonBreak", val, (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Expected O, but got Unknown
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Expected O, but got Unknown
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Expected O, but got Unknown
			List<MissionBehavior> obj2 = new List<MissionBehavior>
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)(object)new CampaignMissionComponent(),
				(MissionBehavior)(object)new MissionBasicTeamLogic(),
				(MissionBehavior)new BasicLeaveMissionLogic(),
				(MissionBehavior)(object)new LeaveMissionLogic(),
				(MissionBehavior)(object)new SandBoxMissionHandler(),
				(MissionBehavior)(object)new BattleAgentLogic(),
				(MissionBehavior)(object)new MissionAgentLookHandler(),
				(MissionBehavior)(object)new MissionAgentHandler(),
				(MissionBehavior)(object)new StealthAreaMissionLogic(),
				(MissionBehavior)(object)new MissionLocationLogic(location, "sp_prison_break"),
				(MissionBehavior)(object)new HeroSkillHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)(object)new MissionCrimeHandler(),
				(MissionBehavior)new MissionFacialAnimationHandler(),
				(MissionBehavior)(object)new LocationItemSpawnHandler(),
				(MissionBehavior)(object)new PrisonBreakMissionController(prisonerCharacter),
				(MissionBehavior)(object)new CorpseDraggingMissionLogic(),
				(MissionBehavior)(object)new StealthFailCounterMissionLogic(),
				(MissionBehavior)(object)new VisualTrackerMissionBehavior(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)(object)new BattleSurgeonLogic(),
				(MissionBehavior)(object)new StealthPatrolPointMissionLogic()
			};
			_ = Game.Current.IsDevelopmentMode;
			return obj2.ToArray();
		}, true, true);
		obj.ForceNoFriendlyFire = true;
		return obj;
	}

	[MissionMethod]
	public static Mission OpenVillageMission(string scene, Location location, CharacterObject talkToChar = null, string sceneLevels = null)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		return MissionState.OpenNew("Village", CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[27]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new MissionBasicTeamLogic(),
			(MissionBehavior)new VillageMissionController(),
			(MissionBehavior)new NotableSpawnPointHandler(),
			(MissionBehavior)new BasicLeaveMissionLogic(),
			(MissionBehavior)new LeaveMissionLogic(),
			(MissionBehavior)new MissionAgentLookHandler(),
			(MissionBehavior)new SandBoxMissionHandler(),
			(MissionBehavior)new MissionConversationLogic(talkToChar),
			(MissionBehavior)new MissionFightHandler(),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new MissionLocationLogic(location),
			(MissionBehavior)new MissionAlleyHandler(),
			(MissionBehavior)new HeroSkillHandler(),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new BattleAgentLogic(),
			(MissionBehavior)new MountAgentLogic(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new MissionCrimeHandler(),
			(MissionBehavior)new MissionHardBorderPlacer(),
			(MissionBehavior)new MissionBoundaryPlacer(),
			(MissionBehavior)new EquipmentControllerLeaveLogic(),
			(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
			(MissionBehavior)new VisualTrackerMissionBehavior(),
			(MissionBehavior)new BattleSurgeonLogic()
		}), true, true);
	}

	[MissionMethod]
	public static Mission OpenArenaStartMission(string scene, Location location, CharacterObject talkToChar = null, string sceneLevels = "")
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		return MissionState.OpenNew("ArenaPracticeFight", CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[14]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new EquipmentControllerLeaveLogic(),
			(MissionBehavior)new ArenaPracticeFightMissionController(),
			(MissionBehavior)new BasicLeaveMissionLogic(),
			(MissionBehavior)new MissionConversationLogic(talkToChar),
			(MissionBehavior)new HeroSkillHandler(),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new ArenaAgentStateDeciderLogic(),
			(MissionBehavior)new VisualTrackerMissionBehavior(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new MissionLocationLogic(location)
		}), true, true);
	}

	[MissionMethod]
	public static Mission OpenRetirementMission(string scene, Location location, CharacterObject talkToChar = null, string sceneLevels = null, string unconsciousMenuId = "")
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		return MissionState.OpenNew("Retirement", CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[26]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new MissionBasicTeamLogic(),
			(MissionBehavior)new VillageMissionController(),
			(MissionBehavior)new NotableSpawnPointHandler(),
			(MissionBehavior)new BasicLeaveMissionLogic(),
			(MissionBehavior)new MissionAgentLookHandler(),
			(MissionBehavior)new MissionConversationLogic(talkToChar),
			(MissionBehavior)new MissionFightHandler(),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new MissionLocationLogic(location),
			(MissionBehavior)new MissionAlleyHandler(),
			(MissionBehavior)new HeroSkillHandler(),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new MountAgentLogic(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new MissionCrimeHandler(),
			(MissionBehavior)new MissionHardBorderPlacer(),
			(MissionBehavior)new MissionBoundaryPlacer(),
			(MissionBehavior)new EquipmentControllerLeaveLogic(),
			(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
			(MissionBehavior)new VisualTrackerMissionBehavior(),
			(MissionBehavior)new BattleSurgeonLogic(),
			(MissionBehavior)new RetirementMissionLogic(),
			(MissionBehavior)new LeaveMissionLogic(unconsciousMenuId)
		}), true, true);
	}

	[MissionMethod]
	public static Mission OpenArenaDuelMission(string scene, Location location, CharacterObject duelCharacter, bool requireCivilianEquipment, bool spawnBOthSidesWithHorse, Action<CharacterObject> onDuelEnd, float customAgentHealth, string sceneLevels = "")
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		return MissionState.OpenNew("ArenaDuelMission", CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[11]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new ArenaDuelMissionController(duelCharacter, requireCivilianEquipment, spawnBOthSidesWithHorse, onDuelEnd, customAgentHealth),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new ArenaAgentStateDeciderLogic(),
			(MissionBehavior)new VisualTrackerMissionBehavior(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new EquipmentControllerLeaveLogic(),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new MissionLocationLogic(location)
		}), true, true);
	}

	[MissionMethod]
	public static Mission OpenArenaDuelMission(string scene, Location location)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		return MissionState.OpenNew("ArenaDuel", CreateSandBoxMissionInitializerRecord(scene, "", doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[12]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new ArenaDuelMissionBehavior(),
			(MissionBehavior)new BasicLeaveMissionLogic(),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new MissionLocationLogic(location),
			(MissionBehavior)new HeroSkillHandler(),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new EquipmentControllerLeaveLogic(),
			(MissionBehavior)new ArenaAgentStateDeciderLogic()
		}), true, true);
	}

	[MissionMethod]
	public static Mission OpenBattleMission(MissionInitializerRecord rec)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		bool isPlayerSergeant = MobileParty.MainParty.MapEvent.IsPlayerSergeant();
		bool isPlayerInArmy = MobileParty.MainParty.Army != null;
		List<string> heroesOnPlayerSideByPriority = HeroHelper.OrderHeroesOnPlayerSideByPriority(false, false);
		bool isPlayerAttacker = !Extensions.IsEmpty<MapEventParty>(((IEnumerable<MapEventParty>)MobileParty.MainParty.MapEvent.AttackerSide.Parties).Where((MapEventParty p) => p.Party == MobileParty.MainParty.Party));
		Mission obj = MissionState.OpenNew("Battle", rec, (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected O, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected O, but got Unknown
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Expected O, but got Unknown
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Expected O, but got Unknown
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Expected O, but got Unknown
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Expected O, but got Unknown
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Expected O, but got Unknown
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Expected O, but got Unknown
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Expected O, but got Unknown
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Expected O, but got Unknown
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Expected O, but got Unknown
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Expected O, but got Unknown
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Expected O, but got Unknown
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Expected O, but got Unknown
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Expected O, but got Unknown
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Expected O, but got Unknown
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Expected O, but got Unknown
			MissionBehavior[] obj2 = new MissionBehavior[29]
			{
				(MissionBehavior)CreateCampaignMissionAgentSpawnLogic((BattleSizeType)0),
				(MissionBehavior)new BattlePowerCalculationLogic(),
				(MissionBehavior)new BattleSpawnLogic("battle_set"),
				(MissionBehavior)new SandBoxBattleMissionSpawnHandler(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new BattleAgentLogic(),
				(MissionBehavior)new MountAgentLogic(),
				(MissionBehavior)new BannerBearerLogic(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new BattleReinforcementsSpawnController(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)1, isPlayerSergeant),
				(MissionBehavior)new BattleObserverMissionLogic(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new BattleSurgeonLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new BattleMissionAgentInteractionLogic(),
				(MissionBehavior)new AgentMoraleInteractionLogic(),
				(MissionBehavior)new AssignPlayerRoleInTeamMissionController(!isPlayerSergeant, isPlayerSergeant, isPlayerInArmy, heroesOnPlayerSideByPriority),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior)
			};
			Hero leaderHero = MapEvent.PlayerMapEvent.AttackerSide.LeaderParty.LeaderHero;
			TextObject attackerGeneralName = ((leaderHero != null) ? leaderHero.Name : null);
			Hero leaderHero2 = MapEvent.PlayerMapEvent.DefenderSide.LeaderParty.LeaderHero;
			obj2[20] = (MissionBehavior)new SandboxGeneralsAndCaptainsAssignmentLogic(attackerGeneralName, (leaderHero2 != null) ? leaderHero2.Name : null);
			obj2[21] = (MissionBehavior)new EquipmentControllerLeaveLogic();
			obj2[22] = (MissionBehavior)new MissionHardBorderPlacer();
			obj2[23] = (MissionBehavior)new MissionBoundaryPlacer();
			obj2[24] = (MissionBehavior)new MissionBoundaryCrossingHandler(10f);
			obj2[25] = (MissionBehavior)new HighlightsController();
			obj2[26] = (MissionBehavior)new BattleHighlightsController();
			obj2[27] = (MissionBehavior)new BattleDeploymentMissionController(isPlayerAttacker);
			obj2[28] = (MissionBehavior)new BattleDeploymentHandler(isPlayerAttacker);
			return (IEnumerable<MissionBehavior>)(object)obj2;
		}, true, true);
		obj.SetPlayerCanTakeControlOfAnotherAgentWhenDead();
		return obj;
	}

	[MissionMethod]
	public static Mission OpenCaravanBattleMission(MissionInitializerRecord rec, bool isCaravan)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		bool isPlayerAttacker = !Extensions.IsEmpty<MapEventParty>(((IEnumerable<MapEventParty>)MobileParty.MainParty.MapEvent.AttackerSide.Parties).Where((MapEventParty p) => p.Party == MobileParty.MainParty.Party));
		bool isPlayerSergeant = MobileParty.MainParty.MapEvent.IsPlayerSergeant();
		bool isPlayerInArmy = MobileParty.MainParty.Army != null;
		return MissionState.OpenNew("Battle", rec, (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Expected O, but got Unknown
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Expected O, but got Unknown
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Expected O, but got Unknown
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Expected O, but got Unknown
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Expected O, but got Unknown
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Expected O, but got Unknown
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Expected O, but got Unknown
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Expected O, but got Unknown
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Expected O, but got Unknown
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Expected O, but got Unknown
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Expected O, but got Unknown
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Expected O, but got Unknown
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Expected O, but got Unknown
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Expected O, but got Unknown
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Expected O, but got Unknown
			MissionBehavior[] obj = new MissionBehavior[31]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new BattleReinforcementsSpawnController(),
				(MissionBehavior)new BannerBearerLogic(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)1, isPlayerSergeant),
				(MissionBehavior)new BattleSpawnLogic("battle_set"),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)CreateCampaignMissionAgentSpawnLogic((BattleSizeType)0),
				(MissionBehavior)new BattlePowerCalculationLogic(),
				(MissionBehavior)new SandBoxBattleMissionSpawnHandler(),
				(MissionBehavior)new BattleObserverMissionLogic(),
				(MissionBehavior)new BattleAgentLogic(),
				(MissionBehavior)new MountAgentLogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new BattleMissionAgentInteractionLogic(),
				(MissionBehavior)new AgentMoraleInteractionLogic(),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new AssignPlayerRoleInTeamMissionController(!isPlayerSergeant, isPlayerSergeant, isPlayerInArmy, (List<string>)null),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior),
				default(MissionBehavior)
			};
			Hero leaderHero = MapEvent.PlayerMapEvent.AttackerSide.LeaderParty.LeaderHero;
			TextObject attackerGeneralName = ((leaderHero != null) ? leaderHero.Name : null);
			Hero leaderHero2 = MapEvent.PlayerMapEvent.DefenderSide.LeaderParty.LeaderHero;
			obj[24] = (MissionBehavior)new SandboxGeneralsAndCaptainsAssignmentLogic(attackerGeneralName, (leaderHero2 != null) ? leaderHero2.Name : null);
			obj[25] = (MissionBehavior)new EquipmentControllerLeaveLogic();
			obj[26] = (MissionBehavior)new MissionCaravanOrVillagerTacticsHandler();
			obj[27] = (MissionBehavior)new CaravanBattleMissionHandler(MathF.Min(MapEvent.PlayerMapEvent.InvolvedParties.Where((PartyBase ip) => (int)ip.Side == 1).Sum((PartyBase ip) => ip.MobileParty.Party.MemberRoster.TotalManCount - ip.MobileParty.Party.MemberRoster.TotalWounded), MapEvent.PlayerMapEvent.InvolvedParties.Where((PartyBase ip) => (int)ip.Side == 0).Sum((PartyBase ip) => ip.MobileParty.Party.MemberRoster.TotalManCount - ip.MobileParty.Party.MemberRoster.TotalWounded)), MapEvent.PlayerMapEvent.InvolvedParties.Any((PartyBase ip) => (ip.MobileParty.IsCaravan || ip.MobileParty.IsVillager) && (((MBObjectBase)ip.Culture).StringId == "aserai" || ((MBObjectBase)ip.Culture).StringId == "khuzait")), isCaravan);
			obj[28] = (MissionBehavior)new BattleDeploymentHandler(isPlayerAttacker);
			obj[29] = (MissionBehavior)new BattleDeploymentMissionController(isPlayerAttacker);
			obj[30] = (MissionBehavior)new BattleSurgeonLogic();
			return (IEnumerable<MissionBehavior>)(object)obj;
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenAlleyFightMission(MissionInitializerRecord rec, Location location, TroopRoster playerSideTroops, TroopRoster rivalSideTroops)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		return MissionState.OpenNew("AlleyFight", rec, (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Expected O, but got Unknown
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Expected O, but got Unknown
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Expected O, but got Unknown
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Expected O, but got Unknown
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Expected O, but got Unknown
			List<MissionBehavior> list = new List<MissionBehavior>
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new BattlePowerCalculationLogic(),
				(MissionBehavior)(object)new CampaignMissionComponent(),
				(MissionBehavior)(object)new AlleyFightMissionHandler(playerSideTroops, rivalSideTroops),
				(MissionBehavior)new BattleObserverMissionLogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)(object)new MissionAgentHandler(),
				(MissionBehavior)(object)new MissionLocationLogic(location),
				(MissionBehavior)(object)new MissionFightHandler(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new BattleMissionAgentInteractionLogic(),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new EquipmentControllerLeaveLogic()
			};
			Settlement currentTown = GetCurrentTown();
			if (currentTown != null)
			{
				list.Add((MissionBehavior)(object)new WorkshopMissionHandler(currentTown));
			}
			return list.ToArray();
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenCombatMissionWithDialogue(MissionInitializerRecord rec, CharacterObject characterToTalkTo)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		return MissionState.OpenNew("CombatWithDialogue", rec, (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Expected O, but got Unknown
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Expected O, but got Unknown
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Expected O, but got Unknown
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Expected O, but got Unknown
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Expected O, but got Unknown
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Expected O, but got Unknown
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Expected O, but got Unknown
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Expected O, but got Unknown
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Expected O, but got Unknown
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Expected O, but got Unknown
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Expected O, but got Unknown
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Expected O, but got Unknown
			IMissionTroopSupplier[] suppliers = (IMissionTroopSupplier[])(object)new IMissionTroopSupplier[2]
			{
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(PlayerEncounter.Battle, (BattleSideEnum)0, (FlattenedTroopRoster)null, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null),
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(PlayerEncounter.Battle, (BattleSideEnum)1, (FlattenedTroopRoster)null, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null)
			};
			List<MissionBehavior> list = new List<MissionBehavior>
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)(object)new CampaignMissionComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)0, false),
				(MissionBehavior)new BattleSpawnLogic("battle_set"),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)(object)new CombatMissionWithDialogueController(suppliers, (BasicCharacterObject)(object)characterToTalkTo),
				(MissionBehavior)(object)new MissionConversationLogic(null),
				(MissionBehavior)new BattleObserverMissionLogic(),
				(MissionBehavior)(object)new BattleAgentLogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new BattleMissionAgentInteractionLogic(),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)(object)new BattleSurgeonLogic()
			};
			Settlement currentTown = GetCurrentTown();
			if (currentTown != null)
			{
				list.Add((MissionBehavior)(object)new WorkshopMissionHandler(currentTown));
			}
			return list.ToArray();
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenBattleMissionWhileEnteringSettlement(string scene, int upgradeLevel, int numberOfMaxTroopToBeSpawnedForPlayer, int numberOfMaxTroopToBeSpawnedForOpponent)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		MissionInitializerRecord val = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref val))._002Ector(scene);
		val.PlayingInCampaignMode = (int)Campaign.Current.GameMode == 1;
		val.AtmosphereOnCampaign = (((int)Campaign.Current.GameMode == 1) ? Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position) : AtmosphereInfo.GetInvalidAtmosphereInfo());
		val.DecalAtlasGroup = 3;
		val.SceneLevels = Campaign.Current.Models.LocationModel.GetCivilianUpgradeLevelTag(upgradeLevel);
		return MissionState.OpenNew("EnteringSettlementBattle", val, (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Expected O, but got Unknown
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Expected O, but got Unknown
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Expected O, but got Unknown
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Expected O, but got Unknown
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Expected O, but got Unknown
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Expected O, but got Unknown
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Expected O, but got Unknown
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Expected O, but got Unknown
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Expected O, but got Unknown
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Expected O, but got Unknown
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Expected O, but got Unknown
			IMissionTroopSupplier[] suppliers = (IMissionTroopSupplier[])(object)new IMissionTroopSupplier[2]
			{
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(PlayerEncounter.Battle, (BattleSideEnum)0, (FlattenedTroopRoster)null, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null),
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(PlayerEncounter.Battle, (BattleSideEnum)1, (FlattenedTroopRoster)null, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null)
			};
			List<MissionBehavior> list = new List<MissionBehavior>
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)(object)new CampaignMissionComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)0, false),
				(MissionBehavior)new BattleSpawnLogic("battle_set"),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new BattleObserverMissionLogic(),
				(MissionBehavior)(object)new WhileEnteringSettlementBattleMissionController(suppliers, numberOfMaxTroopToBeSpawnedForPlayer, numberOfMaxTroopToBeSpawnedForOpponent),
				(MissionBehavior)(object)new MissionFightHandler(),
				(MissionBehavior)(object)new BattleAgentLogic(),
				(MissionBehavior)(object)new MountAgentLogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new BattleMissionAgentInteractionLogic(),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)(object)new BattleSurgeonLogic()
			};
			Settlement currentTown = GetCurrentTown();
			if (currentTown != null)
			{
				list.Add((MissionBehavior)(object)new WorkshopMissionHandler(currentTown));
			}
			return list.ToArray();
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenBattleMission(string scene, bool usesTownDecalAtlas)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return OpenBattleMission(CreateSandBoxMissionInitializerRecord(scene, "", doNotUseLoadingScreen: false, (DecalAtlasGroup)(usesTownDecalAtlas ? 3 : 2)));
	}

	[MissionMethod]
	public static Mission OpenAlleyFightMission(string scene, int upgradeLevel, Location location, TroopRoster playerSideTroops, TroopRoster rivalSideTroops)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return OpenAlleyFightMission(CreateSandBoxMissionInitializerRecord(scene, Campaign.Current.Models.LocationModel.GetCivilianUpgradeLevelTag(upgradeLevel), doNotUseLoadingScreen: false, (DecalAtlasGroup)3), location, playerSideTroops, rivalSideTroops);
	}

	[MissionMethod]
	public static Mission OpenCombatMissionWithDialogue(string scene, CharacterObject characterToTalkTo, int upgradeLevel)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return OpenCombatMissionWithDialogue(CreateSandBoxMissionInitializerRecord(scene, Campaign.Current.Models.LocationModel.GetCivilianUpgradeLevelTag(upgradeLevel), doNotUseLoadingScreen: false, (DecalAtlasGroup)3), characterToTalkTo);
	}

	[MissionMethod]
	public static Mission OpenHideoutBattleMission(string scene, FlattenedTroopRoster playerTroops, bool isTutorial)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		List<MobileParty> list = new List<MobileParty>();
		foreach (MapEventParty item in (List<MapEventParty>)(object)MapEvent.PlayerMapEvent.PartiesOnSide((BattleSideEnum)0))
		{
			if (item.Party.IsMobile)
			{
				list.Add(item.Party.MobileParty);
			}
		}
		string sceneLevels = (isTutorial ? "level_1" : "level_2");
		int firstPhaseEnemySideTroopCount = default(int);
		FlattenedTroopRoster banditPriorityList = MapEventHelper.GetPriorityListForHideoutMission(list, ref firstPhaseEnemySideTroopCount);
		FlattenedTroopRoster playerPriorityList = playerTroops ?? MobilePartyHelper.GetStrongestAndPriorTroops(MobileParty.MainParty, Campaign.Current.Models.BanditDensityModel.GetMaximumTroopCountForHideoutMission(MobileParty.MainParty, true), true).ToFlattenedRoster();
		int firstPhasePlayerSideTroopCount = ((IEnumerable<FlattenedTroopRosterElement>)playerPriorityList).Count();
		MissionInitializerRecord val = CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: false, (DecalAtlasGroup)3);
		return MissionState.OpenNew("HideoutBattle", val, (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Expected O, but got Unknown
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Expected O, but got Unknown
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Expected O, but got Unknown
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Expected O, but got Unknown
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Expected O, but got Unknown
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Expected O, but got Unknown
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Expected O, but got Unknown
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Expected O, but got Unknown
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Expected O, but got Unknown
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Expected O, but got Unknown
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Expected O, but got Unknown
			IMissionTroopSupplier[] suppliers = (IMissionTroopSupplier[])(object)new IMissionTroopSupplier[2]
			{
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, (BattleSideEnum)0, banditPriorityList, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null),
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, (BattleSideEnum)1, playerPriorityList, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null)
			};
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[22]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)0, false),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new HideoutCinematicController(),
				(MissionBehavior)new MissionConversationLogic(),
				(MissionBehavior)new HideoutMissionController(suppliers, PartyBase.MainParty.Side, firstPhaseEnemySideTroopCount, firstPhasePlayerSideTroopCount),
				(MissionBehavior)new BattleObserverMissionLogic(),
				(MissionBehavior)new BattleAgentLogic(),
				(MissionBehavior)new MountAgentLogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new AgentMoraleInteractionLogic(),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new BattleSurgeonLogic(),
				(MissionBehavior)new MissionObjectiveLogic()
			};
		}, true, true);
	}

	[MissionMethod(UsableByEditor = true)]
	public static Mission OpenHideoutAmbushMission(string sceneName, FlattenedTroopRoster playerTroops, Location location)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Invalid comparison between Unknown and I4
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Invalid comparison between Unknown and I4
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Expected O, but got Unknown
		FlattenedTroopRoster priorAllyTroops = playerTroops ?? MobilePartyHelper.GetStrongestAndPriorTroops(MobileParty.MainParty, Campaign.Current.Models.BanditDensityModel.GetMaximumTroopCountForHideoutMission(MobileParty.MainParty, false), false).ToFlattenedRoster();
		priorAllyTroops.RemoveIf((Predicate<FlattenedTroopRosterElement>)((FlattenedTroopRosterElement x) => ((BasicCharacterObject)((FlattenedTroopRosterElement)(ref x)).Troop).IsPlayerCharacter));
		MissionInitializerRecord val = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref val))._002Ector(sceneName);
		val.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		val.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		val.PlayingInCampaignMode = (int)Campaign.Current.GameMode == 1;
		val.AtmosphereOnCampaign = (((int)Campaign.Current.GameMode == 1) ? Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position) : AtmosphereInfo.GetInvalidAtmosphereInfo());
		val.TerrainType = ((Campaign.Current.MapSceneWrapper != null) ? ((int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace)) : 0);
		val.SceneLevels = "level_1";
		val.DoNotUseLoadingScreen = false;
		val.DisableCorpseFadeOut = true;
		val.DecalAtlasGroup = 3;
		MissionInitializerRecord val2 = val;
		FlattenedTroopRoster banditPriorityList = new FlattenedTroopRoster(4);
		foreach (MapEventParty item in (List<MapEventParty>)(object)MapEvent.PlayerMapEvent.PartiesOnSide((BattleSideEnum)0))
		{
			if (item.Party.IsMobile)
			{
				banditPriorityList.Add(item.Party.MemberRoster.GetTroopRoster());
			}
		}
		int playerTroopCount = ((IEnumerable<FlattenedTroopRosterElement>)priorAllyTroops).Count();
		return MissionState.OpenNew("HideoutAmbushMission", val2, (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected O, but got Unknown
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Expected O, but got Unknown
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Expected O, but got Unknown
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Expected O, but got Unknown
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Expected O, but got Unknown
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Expected O, but got Unknown
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Expected O, but got Unknown
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Expected O, but got Unknown
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Expected O, but got Unknown
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Expected O, but got Unknown
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Expected O, but got Unknown
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Expected O, but got Unknown
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Expected O, but got Unknown
			IMissionTroopSupplier[] suppliers = (IMissionTroopSupplier[])(object)new IMissionTroopSupplier[2]
			{
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, (BattleSideEnum)0, banditPriorityList, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null),
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, (BattleSideEnum)1, priorAllyTroops, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null)
			};
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[31]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)0, false),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new StealthPatrolPointMissionLogic(),
				(MissionBehavior)new HideoutAmbushMissionController(suppliers, (BattleSideEnum)1, playerTroopCount),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new MountAgentLogic(),
				(MissionBehavior)new HideoutAmbushBossFightCinematicController(),
				(MissionBehavior)new MissionConversationLogic(),
				(MissionBehavior)new BattleObserverMissionLogic(),
				(MissionBehavior)new MissionAgentHandler(),
				(MissionBehavior)new BattleAgentLogic(),
				(MissionBehavior)new MissionLocationLogic(location),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new StealthFailCounterMissionLogic(),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new AgentMoraleInteractionLogic(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new BattleSurgeonLogic(),
				(MissionBehavior)new MissionAIActivationDeactivationEventListenerLogic(),
				(MissionBehavior)new CorpseDraggingMissionLogic(),
				(MissionBehavior)new ShowQuickInformationEventListenerLogic(),
				(MissionBehavior)new VisualTrackerMissionBehavior(),
				(MissionBehavior)new StealthAreaMissionLogic(),
				(MissionBehavior)new MissionObjectiveLogic()
			};
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenCampMission(string scene)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		MissionInitializerRecord val = CreateSandBoxMissionInitializerRecord(scene, "", doNotUseLoadingScreen: false, (DecalAtlasGroup)3);
		object obj = _003C_003Ec._003C_003E9__25_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val2 = (Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[9]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)0, false),
				(MissionBehavior)new BasicLeaveMissionLogic(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new EquipmentControllerLeaveLogic()
			};
			_003C_003Ec._003C_003E9__25_0 = val2;
			obj = (object)val2;
		}
		return MissionState.OpenNew("Camp", val, (InitializeMissionBehaviorsDelegate)obj, true, true);
	}

	[MissionMethod]
	public static Mission OpenSiegeMissionWithDeployment(string scene, float[] wallHitPointPercentages, bool hasAnySiegeTower, List<MissionSiegeWeapon> siegeWeaponsOfAttackers, List<MissionSiegeWeapon> siegeWeaponsOfDefenders, bool isPlayerAttacker, int sceneUpgradeLevel = 0, bool isSallyOut = false, bool isReliefForceAttack = false)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		string upgradeLevelTag = Campaign.Current.Models.LocationModel.GetUpgradeLevelTag(sceneUpgradeLevel);
		upgradeLevelTag += " siege";
		bool isPlayerSergeant = MobileParty.MainParty.MapEvent.IsPlayerSergeant();
		bool isPlayerInArmy = MobileParty.MainParty.Army != null;
		List<string> heroesOnPlayerSideByPriority = HeroHelper.OrderHeroesOnPlayerSideByPriority(false, false);
		Mission obj = MissionState.OpenNew("SiegeMissionWithDeployment", CreateSandBoxMissionInitializerRecord(scene, upgradeLevelTag, doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Invalid comparison between Unknown and I4
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Expected O, but got Unknown
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Expected O, but got Unknown
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Expected O, but got Unknown
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Expected O, but got Unknown
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Expected O, but got Unknown
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Expected O, but got Unknown
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Expected O, but got Unknown
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Expected O, but got Unknown
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Expected O, but got Unknown
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Expected O, but got Unknown
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Expected O, but got Unknown
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_0291: Expected O, but got Unknown
			//IL_0297: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Expected O, but got Unknown
			//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ac: Expected O, but got Unknown
			//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Expected O, but got Unknown
			//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Expected O, but got Unknown
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cd: Expected O, but got Unknown
			//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0304: Expected O, but got Unknown
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02eb: Expected O, but got Unknown
			//IL_030b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0315: Expected O, but got Unknown
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Expected O, but got Unknown
			List<MissionBehavior> list = new List<MissionBehavior>
			{
				(MissionBehavior)new BattleSpawnLogic(isSallyOut ? "sally_out_set" : (isReliefForceAttack ? "relief_force_attack_set" : "battle_set")),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)(object)new CampaignMissionComponent()
			};
			BattleEndLogic val = new BattleEndLogic();
			if ((int)MobileParty.MainParty.MapEvent.PlayerSide == 1)
			{
				val.EnableEnemyDefenderPullBack(Campaign.Current.Models.SiegeLordsHallFightModel.DefenderTroopNumberForSuccessfulPullBack);
			}
			list.Add((MissionBehavior)(object)val);
			list.Add((MissionBehavior)new BattleReinforcementsSpawnController());
			list.Add((MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)(isSallyOut ? 3 : 2), isPlayerSergeant));
			list.Add((MissionBehavior)new SiegeMissionPreparationHandler(isSallyOut, isReliefForceAttack, wallHitPointPercentages, hasAnySiegeTower));
			list.Add((MissionBehavior)(object)new CampaignSiegeStateHandler());
			Settlement currentTown = GetCurrentTown();
			if (currentTown != null)
			{
				list.Add((MissionBehavior)(object)new WorkshopMissionHandler(currentTown));
			}
			BattleSizeType battleSizeType = (BattleSizeType)1;
			if (isSallyOut)
			{
				battleSizeType = (BattleSizeType)2;
				FlattenedTroopRoster priorityTroopsForSallyOutAmbush = Campaign.Current.Models.SiegeEventModel.GetPriorityTroopsForSallyOutAmbush();
				list.Add((MissionBehavior)(object)new SandBoxSallyOutMissionController(isSallyOutAmbush: true));
				list.Add((MissionBehavior)(object)CreateCampaignMissionAgentSpawnLogic(battleSizeType, priorityTroopsForSallyOutAmbush));
			}
			else
			{
				if (isReliefForceAttack)
				{
					list.Add((MissionBehavior)(object)new SandBoxSallyOutMissionController(isSallyOutAmbush: false));
				}
				else
				{
					list.Add((MissionBehavior)(object)new SandBoxSiegeMissionSpawnHandler());
				}
				list.Add((MissionBehavior)(object)CreateCampaignMissionAgentSpawnLogic(battleSizeType));
			}
			list.Add((MissionBehavior)new BattlePowerCalculationLogic());
			list.Add((MissionBehavior)new BattleObserverMissionLogic());
			list.Add((MissionBehavior)(object)new BattleAgentLogic());
			list.Add((MissionBehavior)(object)new BattleSurgeonLogic());
			list.Add((MissionBehavior)(object)new MountAgentLogic());
			list.Add((MissionBehavior)new BannerBearerLogic());
			list.Add((MissionBehavior)new AgentHumanAILogic());
			if (!isSallyOut)
			{
				list.Add((MissionBehavior)new AmmoSupplyLogic(new List<BattleSideEnum> { (BattleSideEnum)0 }));
			}
			list.Add((MissionBehavior)new AgentVictoryLogic());
			list.Add((MissionBehavior)new AssignPlayerRoleInTeamMissionController(!isPlayerSergeant, isPlayerSergeant, isPlayerInArmy, heroesOnPlayerSideByPriority));
			Hero leaderHero = MapEvent.PlayerMapEvent.AttackerSide.LeaderParty.LeaderHero;
			TextObject attackerGeneralName = ((leaderHero != null) ? leaderHero.Name : null);
			Hero leaderHero2 = MapEvent.PlayerMapEvent.DefenderSide.LeaderParty.LeaderHero;
			list.Add((MissionBehavior)(object)new SandboxGeneralsAndCaptainsAssignmentLogic(attackerGeneralName, (leaderHero2 != null) ? leaderHero2.Name : null, null, null, createBodyguard: false));
			list.Add((MissionBehavior)new MissionAgentPanicHandler());
			list.Add((MissionBehavior)new MissionBoundaryPlacer());
			list.Add((MissionBehavior)new MissionBoundaryCrossingHandler(10f));
			list.Add((MissionBehavior)new AgentMoraleInteractionLogic());
			list.Add((MissionBehavior)new HighlightsController());
			list.Add((MissionBehavior)new BattleHighlightsController());
			list.Add((MissionBehavior)new EquipmentControllerLeaveLogic());
			if (isSallyOut)
			{
				list.Add((MissionBehavior)new MissionSiegeEnginesLogic(new List<MissionSiegeWeapon>(), siegeWeaponsOfAttackers));
			}
			else
			{
				list.Add((MissionBehavior)new MissionSiegeEnginesLogic(siegeWeaponsOfDefenders, siegeWeaponsOfAttackers));
			}
			list.Add((MissionBehavior)new SiegeDeploymentHandler(isPlayerAttacker));
			list.Add((MissionBehavior)new SiegeDeploymentMissionController(isPlayerAttacker));
			return list.ToArray();
		}, true, true);
		obj.SetPlayerCanTakeControlOfAnotherAgentWhenDead();
		return obj;
	}

	[MissionMethod]
	public static Mission OpenSiegeMissionNoDeployment(string scene, bool isSallyOut = false, bool isReliefForceAttack = false)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		string upgradeLevelTag = Campaign.Current.Models.LocationModel.GetUpgradeLevelTag(3);
		upgradeLevelTag += " siege";
		bool isPlayerSergeant = MobileParty.MainParty.MapEvent.IsPlayerSergeant();
		bool isPlayerInArmy = MobileParty.MainParty.Army != null;
		List<string> heroesOnPlayerSideByPriority = HeroHelper.OrderHeroesOnPlayerSideByPriority(false, false);
		return MissionState.OpenNew("SiegeMissionNoDeployment", CreateSandBoxMissionInitializerRecord(scene, upgradeLevelTag, doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Expected O, but got Unknown
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Expected O, but got Unknown
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Invalid comparison between Unknown and I4
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Expected O, but got Unknown
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Expected O, but got Unknown
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Expected O, but got Unknown
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Expected O, but got Unknown
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Expected O, but got Unknown
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Expected O, but got Unknown
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Expected O, but got Unknown
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Expected O, but got Unknown
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Expected O, but got Unknown
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Expected O, but got Unknown
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Expected O, but got Unknown
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Expected O, but got Unknown
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Expected O, but got Unknown
			List<MissionBehavior> list = new List<MissionBehavior>
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new BattleSpawnLogic(isSallyOut ? "sally_out_set" : (isReliefForceAttack ? "relief_force_attack_set" : "battle_set")),
				(MissionBehavior)(object)new CampaignMissionComponent()
			};
			BattleEndLogic val = new BattleEndLogic();
			if (!isSallyOut && !isReliefForceAttack && (int)MobileParty.MainParty.MapEvent.PlayerSide == 1)
			{
				val.EnableEnemyDefenderPullBack(Campaign.Current.Models.SiegeLordsHallFightModel.DefenderTroopNumberForSuccessfulPullBack);
			}
			list.Add((MissionBehavior)(object)val);
			list.Add((MissionBehavior)new BattleReinforcementsSpawnController());
			list.Add((MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)1, isPlayerSergeant));
			list.Add((MissionBehavior)(object)new CampaignSiegeStateHandler());
			BattleSizeType battleSizeType = (BattleSizeType)((!isSallyOut) ? 1 : 2);
			list.Add((MissionBehavior)(object)CreateCampaignMissionAgentSpawnLogic(battleSizeType));
			list.Add((MissionBehavior)new BattlePowerCalculationLogic());
			list.Add((MissionBehavior)(object)new SandBoxBattleMissionSpawnHandler());
			Settlement currentTown = GetCurrentTown();
			if (currentTown != null)
			{
				list.Add((MissionBehavior)(object)new WorkshopMissionHandler(currentTown));
			}
			list.Add((MissionBehavior)new BattleObserverMissionLogic());
			list.Add((MissionBehavior)(object)new BattleAgentLogic());
			list.Add((MissionBehavior)(object)new BattleSurgeonLogic());
			list.Add((MissionBehavior)(object)new MountAgentLogic());
			list.Add((MissionBehavior)new AgentVictoryLogic());
			list.Add((MissionBehavior)new AmmoSupplyLogic(new List<BattleSideEnum> { (BattleSideEnum)0 }));
			list.Add((MissionBehavior)new MissionAgentPanicHandler());
			list.Add((MissionBehavior)new MissionHardBorderPlacer());
			list.Add((MissionBehavior)new MissionBoundaryPlacer());
			list.Add((MissionBehavior)new EquipmentControllerLeaveLogic());
			list.Add((MissionBehavior)new MissionBoundaryCrossingHandler(10f));
			list.Add((MissionBehavior)new AgentHumanAILogic());
			list.Add((MissionBehavior)new AgentMoraleInteractionLogic());
			list.Add((MissionBehavior)new HighlightsController());
			list.Add((MissionBehavior)new BattleHighlightsController());
			list.Add((MissionBehavior)new AssignPlayerRoleInTeamMissionController(!isPlayerSergeant, isPlayerSergeant, isPlayerInArmy, heroesOnPlayerSideByPriority));
			Hero leaderHero = MapEvent.PlayerMapEvent.AttackerSide.LeaderParty.LeaderHero;
			TextObject attackerGeneralName = ((leaderHero != null) ? leaderHero.Name : null);
			Hero leaderHero2 = MapEvent.PlayerMapEvent.DefenderSide.LeaderParty.LeaderHero;
			list.Add((MissionBehavior)(object)new SandboxGeneralsAndCaptainsAssignmentLogic(attackerGeneralName, (leaderHero2 != null) ? leaderHero2.Name : null, null, null, createBodyguard: false));
			return list.ToArray();
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenSiegeLordsHallFightMission(string scene, FlattenedTroopRoster attackerPriorityList)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		int remainingDefenderArcherCount = Campaign.Current.Models.SiegeLordsHallFightModel.MaxDefenderArcherCount;
		FlattenedTroopRoster defenderPriorityList = Campaign.Current.Models.SiegeLordsHallFightModel.GetPriorityListForLordsHallFightMission(MapEvent.PlayerMapEvent, (BattleSideEnum)0, Campaign.Current.Models.SiegeLordsHallFightModel.MaxDefenderSideTroopCount);
		int attackerSideTroopCountMax = MathF.Min(Campaign.Current.Models.SiegeLordsHallFightModel.MaxAttackerSideTroopCount, attackerPriorityList.Troops.Count());
		int defenderSideTroopCountMax = MathF.Min(Campaign.Current.Models.SiegeLordsHallFightModel.MaxDefenderSideTroopCount, defenderPriorityList.Troops.Count());
		MissionInitializerRecord val = CreateSandBoxMissionInitializerRecord(scene, "siege", doNotUseLoadingScreen: false, (DecalAtlasGroup)3);
		return MissionState.OpenNew("SiegeLordsHallFightMission", val, (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Expected O, but got Unknown
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected O, but got Unknown
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Expected O, but got Unknown
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Expected O, but got Unknown
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Expected O, but got Unknown
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Expected O, but got Unknown
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Expected O, but got Unknown
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Expected O, but got Unknown
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Expected O, but got Unknown
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Expected O, but got Unknown
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Expected O, but got Unknown
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Expected O, but got Unknown
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Expected O, but got Unknown
			IMissionTroopSupplier[] array = (IMissionTroopSupplier[])(object)new IMissionTroopSupplier[2]
			{
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, (BattleSideEnum)0, defenderPriorityList, (Func<UniqueTroopDescriptor, MapEventParty, bool>)delegate(UniqueTroopDescriptor uniqueTroopDescriptor, MapEventParty mapEventParty)
				{
					//IL_0003: Unknown result type (might be due to invalid IL or missing references)
					bool result = true;
					if (((BasicCharacterObject)mapEventParty.GetTroop(uniqueTroopDescriptor)).IsRanged)
					{
						if (remainingDefenderArcherCount > 0)
						{
							remainingDefenderArcherCount--;
						}
						else
						{
							result = false;
						}
					}
					return result;
				}),
				(IMissionTroopSupplier)new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, (BattleSideEnum)1, attackerPriorityList, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null)
			};
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[19]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)0, false),
				(MissionBehavior)new CampaignSiegeStateHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new LordsHallFightMissionController(array, Campaign.Current.Models.SiegeLordsHallFightModel.AreaLostRatio, Campaign.Current.Models.SiegeLordsHallFightModel.AttackerDefenderTroopCountRatio, attackerSideTroopCountMax, defenderSideTroopCountMax, PartyBase.MainParty.Side),
				(MissionBehavior)new BattleObserverMissionLogic(),
				(MissionBehavior)new BattleAgentLogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new AmmoSupplyLogic(new List<BattleSideEnum> { (BattleSideEnum)0 }),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new BattleMissionAgentInteractionLogic(),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new BattleSurgeonLogic()
			};
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenVillageBattleMission(string scene)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		bool isPlayerSergeant = MobileParty.MainParty.MapEvent.IsPlayerSergeant();
		bool isPlayerInArmy = MobileParty.MainParty.Army != null;
		return MissionState.OpenNew("VillageBattle", CreateSandBoxMissionInitializerRecord(scene, "", doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Expected O, but got Unknown
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Expected O, but got Unknown
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Expected O, but got Unknown
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Expected O, but got Unknown
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected O, but got Unknown
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Expected O, but got Unknown
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Expected O, but got Unknown
			MissionBehavior[] obj = new MissionBehavior[17]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new BattleEndLogic(),
				(MissionBehavior)new BattleReinforcementsSpawnController(),
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)MobileParty.MainParty.MapEvent.InvolvedParties, (IBattleCombatant)(object)PartyBase.MainParty, (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)0), (IBattleCombatant)(object)MobileParty.MainParty.MapEvent.GetLeaderParty((BattleSideEnum)1), (MissionTeamAITypeEnum)1, isPlayerSergeant),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new AgentMoraleInteractionLogic(),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new AssignPlayerRoleInTeamMissionController(!isPlayerSergeant, isPlayerSergeant, isPlayerInArmy, (List<string>)null),
				default(MissionBehavior),
				default(MissionBehavior)
			};
			Hero leaderHero = MapEvent.PlayerMapEvent.AttackerSide.LeaderParty.LeaderHero;
			TextObject attackerGeneralName = ((leaderHero != null) ? leaderHero.Name : null);
			Hero leaderHero2 = MapEvent.PlayerMapEvent.DefenderSide.LeaderParty.LeaderHero;
			obj[15] = (MissionBehavior)new SandboxGeneralsAndCaptainsAssignmentLogic(attackerGeneralName, (leaderHero2 != null) ? leaderHero2.Name : null);
			obj[16] = (MissionBehavior)new BattleSurgeonLogic();
			return (IEnumerable<MissionBehavior>)(object)obj;
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenConversationMission(ConversationCharacterData playerCharacterData, ConversationCharacterData conversationPartnerData, string specialScene = "", string sceneLevels = "", bool isMultiAgentConversation = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		string sceneName = (Extensions.IsEmpty<char>((IEnumerable<char>)specialScene) ? Campaign.Current.Models.SceneModel.GetConversationSceneForMapPosition(PartyBase.MainParty.Position) : specialScene);
		return MissionState.OpenNew("Conversation", CreateSandBoxMissionInitializerRecord(sceneName, sceneLevels, doNotUseLoadingScreen: true, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[5]
		{
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new MissionConversationLogic(),
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new ConversationMissionLogic(playerCharacterData, conversationPartnerData, isMultiAgentConversation),
			(MissionBehavior)new EquipmentControllerLeaveLogic()
		}), true, false);
	}

	[MissionMethod]
	public static Mission OpenMeetingMission(string scene, CharacterObject character)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		Debug.FailedAssert("This mission was broken", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\SandBoxMissions.cs", "OpenMeetingMission", 1359);
		MissionInitializerRecord val = CreateSandBoxMissionInitializerRecord(scene, "", doNotUseLoadingScreen: false, (DecalAtlasGroup)3);
		object obj = _003C_003Ec._003C_003E9__31_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val2 = (Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[5]
			{
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new MissionSettlementPrepareLogic(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionConversationLogic(),
				(MissionBehavior)new EquipmentControllerLeaveLogic()
			};
			_003C_003Ec._003C_003E9__31_0 = val2;
			obj = (object)val2;
		}
		return MissionState.OpenNew("Conversation", val, (InitializeMissionBehaviorsDelegate)obj, true, false);
	}

	private static Settlement GetCurrentTown()
	{
		if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsTown)
		{
			return Settlement.CurrentSettlement;
		}
		if (MapEvent.PlayerMapEvent != null && MapEvent.PlayerMapEvent.MapEventSettlement != null && MapEvent.PlayerMapEvent.MapEventSettlement.IsTown)
		{
			return MapEvent.PlayerMapEvent.MapEventSettlement;
		}
		return null;
	}

	private static MissionAgentSpawnLogic CreateCampaignMissionAgentSpawnLogic(BattleSizeType battleSizeType, FlattenedTroopRoster priorTroopsForDefenders = null, FlattenedTroopRoster priorTroopsForAttackers = null)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		return new MissionAgentSpawnLogic((IMissionTroopSupplier[])(object)new IMissionTroopSupplier[2]
		{
			(IMissionTroopSupplier)new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, (BattleSideEnum)0, priorTroopsForDefenders, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null),
			(IMissionTroopSupplier)new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, (BattleSideEnum)1, priorTroopsForAttackers, (Func<UniqueTroopDescriptor, MapEventParty, bool>)null)
		}, PartyBase.MainParty.Side, battleSizeType);
	}

	[MissionMethod]
	public static Mission OpenDisguiseMission(string scene, bool willSetUpContact, Location fromLocation, string sceneLevels = null)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		CharacterObject defaultContractorCharacter = MBObjectManager.Instance.GetObject<CharacterObject>("disguise_contractor_character");
		MissionInitializerRecord val = CreateSandBoxMissionInitializerRecord(scene, sceneLevels, doNotUseLoadingScreen: false, (DecalAtlasGroup)3);
		return MissionState.OpenNew("DisguiseMission", val, (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[27]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new MissionBasicTeamLogic(),
			(MissionBehavior)new MissionSettlementPrepareLogic(),
			(MissionBehavior)new BasicLeaveMissionLogic(),
			(MissionBehavior)new BattleAgentLogic(),
			(MissionBehavior)new MountAgentLogic(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new MissionCrimeHandler(),
			(MissionBehavior)new MissionConversationLogic(),
			(MissionBehavior)new HeroSkillHandler(),
			(MissionBehavior)new MissionFightHandler(),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new MissionHardBorderPlacer(),
			(MissionBehavior)new CheckpointMissionLogic(),
			(MissionBehavior)new NotableSpawnPointHandler(),
			(MissionBehavior)new MissionBoundaryPlacer(),
			(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
			(MissionBehavior)new VisualTrackerMissionBehavior(),
			(MissionBehavior)new EquipmentControllerLeaveLogic(),
			(MissionBehavior)new MissionAIActivationDeactivationEventListenerLogic(),
			(MissionBehavior)new StealthPatrolPointMissionLogic(),
			(MissionBehavior)new MissionAlleyHandler(),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new DisguiseMissionLogic(defaultContractorCharacter, fromLocation, willSetUpContact),
			(MissionBehavior)new ShowQuickInformationEventListenerLogic()
		}), true, true);
	}
}
