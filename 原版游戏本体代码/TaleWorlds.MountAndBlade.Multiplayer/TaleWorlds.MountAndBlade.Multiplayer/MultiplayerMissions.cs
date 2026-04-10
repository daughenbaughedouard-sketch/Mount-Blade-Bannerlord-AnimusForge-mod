using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Multiplayer.Missions;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace TaleWorlds.MountAndBlade.Multiplayer;

[MissionManager]
public static class MultiplayerMissions
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static InitializeMissionBehaviorsDelegate _003C_003E9__0_0;

		public static InitializeMissionBehaviorsDelegate _003C_003E9__1_0;

		public static InitializeMissionBehaviorsDelegate _003C_003E9__2_0;

		public static InitializeMissionBehaviorsDelegate _003C_003E9__3_0;

		public static InitializeMissionBehaviorsDelegate _003C_003E9__4_0;

		public static InitializeMissionBehaviorsDelegate _003C_003E9__5_0;

		internal IEnumerable<MissionBehavior> _003COpenTeamDeathmatchMission_003Eb__0_0(Mission missionController)
		{
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Expected O, but got Unknown
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Expected O, but got Unknown
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Expected O, but got Unknown
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Expected O, but got Unknown
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Expected O, but got Unknown
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Expected O, but got Unknown
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Expected O, but got Unknown
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Expected O, but got Unknown
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Expected O, but got Unknown
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Expected O, but got Unknown
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Expected O, but got Unknown
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Expected O, but got Unknown
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Expected O, but got Unknown
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected O, but got Unknown
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Expected O, but got Unknown
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Expected O, but got Unknown
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Expected O, but got Unknown
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Expected O, but got Unknown
			if (!GameNetwork.IsServer)
			{
				return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[21]
				{
					(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
					(MissionBehavior)new MissionMultiplayerTeamDeathmatchClient(),
					(MissionBehavior)new MultiplayerAchievementComponent(),
					(MissionBehavior)new MultiplayerTimerComponent(),
					(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
					(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
					(MissionBehavior)new ConsoleMatchStartEndHandler(),
					(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
					(MissionBehavior)new MultiplayerTeamSelectComponent(),
					(MissionBehavior)new MissionHardBorderPlacer(),
					(MissionBehavior)new MissionBoundaryPlacer(),
					(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
					(MissionBehavior)new MultiplayerPollComponent(),
					(MissionBehavior)new MultiplayerAdminComponent(),
					(MissionBehavior)new MultiplayerGameNotificationsComponent(),
					(MissionBehavior)new MissionOptionsComponent(),
					(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new TDMScoreboardData()),
					(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
					(MissionBehavior)new EquipmentControllerLeaveLogic(),
					(MissionBehavior)new MissionRecentPlayersComponent(),
					(MissionBehavior)new MultiplayerPreloadHelper()
				};
			}
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[22]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerTeamDeathmatch(),
				(MissionBehavior)new MissionMultiplayerTeamDeathmatchClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)(object)new TeamDeathmatchSpawnFrameBehavior(), (SpawningBehaviorBase)(object)new TeamDeathmatchSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new TDMScoreboardData()),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			};
		}

		internal IEnumerable<MissionBehavior> _003COpenDuelMission_003Eb__1_0(Mission missionController)
		{
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Expected O, but got Unknown
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Expected O, but got Unknown
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Expected O, but got Unknown
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Expected O, but got Unknown
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Expected O, but got Unknown
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Expected O, but got Unknown
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Expected O, but got Unknown
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Expected O, but got Unknown
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Expected O, but got Unknown
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Expected O, but got Unknown
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Expected O, but got Unknown
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Expected O, but got Unknown
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Expected O, but got Unknown
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected O, but got Unknown
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Expected O, but got Unknown
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Expected O, but got Unknown
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Expected O, but got Unknown
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			if (!GameNetwork.IsServer)
			{
				return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[20]
				{
					(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
					(MissionBehavior)new MissionMultiplayerGameModeDuelClient(),
					(MissionBehavior)new MultiplayerAchievementComponent(),
					(MissionBehavior)new MultiplayerTimerComponent(),
					(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
					(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
					(MissionBehavior)new ConsoleMatchStartEndHandler(),
					(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
					(MissionBehavior)new MissionHardBorderPlacer(),
					(MissionBehavior)new MissionBoundaryPlacer(),
					(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
					(MissionBehavior)new MultiplayerPollComponent(),
					(MissionBehavior)new MultiplayerAdminComponent(),
					(MissionBehavior)new MultiplayerGameNotificationsComponent(),
					(MissionBehavior)new MissionOptionsComponent(),
					(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new DuelScoreboardData()),
					(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
					(MissionBehavior)new EquipmentControllerLeaveLogic(),
					(MissionBehavior)new MissionRecentPlayersComponent(),
					(MissionBehavior)new MultiplayerPreloadHelper()
				};
			}
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[21]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerDuel(),
				(MissionBehavior)new MissionMultiplayerGameModeDuelClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)(object)new DuelSpawnFrameBehavior(), (SpawningBehaviorBase)(object)new DuelSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new DuelScoreboardData()),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			};
		}

		internal IEnumerable<MissionBehavior> _003COpenSiegeMission_003Eb__2_0(Mission missionController)
		{
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Expected O, but got Unknown
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Expected O, but got Unknown
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Expected O, but got Unknown
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Expected O, but got Unknown
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Expected O, but got Unknown
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Expected O, but got Unknown
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Expected O, but got Unknown
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Expected O, but got Unknown
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Expected O, but got Unknown
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Expected O, but got Unknown
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Expected O, but got Unknown
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Expected O, but got Unknown
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Expected O, but got Unknown
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected O, but got Unknown
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected O, but got Unknown
			//IL_0062: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected O, but got Unknown
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Expected O, but got Unknown
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Expected O, but got Unknown
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Expected O, but got Unknown
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Expected O, but got Unknown
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Expected O, but got Unknown
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Expected O, but got Unknown
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Expected O, but got Unknown
			if (!GameNetwork.IsServer)
			{
				return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[22]
				{
					(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
					(MissionBehavior)new MultiplayerWarmupComponent(),
					(MissionBehavior)new MissionMultiplayerSiegeClient(),
					(MissionBehavior)new MultiplayerAchievementComponent(),
					(MissionBehavior)new MultiplayerTimerComponent(),
					(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
					(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
					(MissionBehavior)new ConsoleMatchStartEndHandler(),
					(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
					(MissionBehavior)new MultiplayerTeamSelectComponent(),
					(MissionBehavior)new MissionHardBorderPlacer(),
					(MissionBehavior)new MissionBoundaryPlacer(),
					(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
					(MissionBehavior)new MultiplayerPollComponent(),
					(MissionBehavior)new MultiplayerAdminComponent(),
					(MissionBehavior)new MultiplayerGameNotificationsComponent(),
					(MissionBehavior)new MissionOptionsComponent(),
					(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new SiegeScoreboardData()),
					(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
					(MissionBehavior)new EquipmentControllerLeaveLogic(),
					(MissionBehavior)new MissionRecentPlayersComponent(),
					(MissionBehavior)new MultiplayerPreloadHelper()
				};
			}
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[23]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerSiege(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerSiegeClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)new SiegeSpawnFrameBehavior(), (SpawningBehaviorBase)new SiegeSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new SiegeScoreboardData()),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			};
		}

		internal IEnumerable<MissionBehavior> _003COpenBattleMission_003Eb__3_0(Mission missionController)
		{
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Expected O, but got Unknown
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Expected O, but got Unknown
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Expected O, but got Unknown
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Expected O, but got Unknown
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Expected O, but got Unknown
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Expected O, but got Unknown
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Expected O, but got Unknown
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Expected O, but got Unknown
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Expected O, but got Unknown
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Expected O, but got Unknown
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Expected O, but got Unknown
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Expected O, but got Unknown
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Expected O, but got Unknown
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Expected O, but got Unknown
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_006c: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Expected O, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Expected O, but got Unknown
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Expected O, but got Unknown
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Expected O, but got Unknown
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Expected O, but got Unknown
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			if (!GameNetwork.IsServer)
			{
				return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[21]
				{
					(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
					(MissionBehavior)new MultiplayerRoundComponent(),
					(MissionBehavior)new MultiplayerWarmupComponent(),
					(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
					(MissionBehavior)new MultiplayerTimerComponent(),
					(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
					(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
					(MissionBehavior)new ConsoleMatchStartEndHandler(),
					(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
					(MissionBehavior)new MultiplayerTeamSelectComponent(),
					(MissionBehavior)new MissionHardBorderPlacer(),
					(MissionBehavior)new MissionBoundaryPlacer(),
					(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
					(MissionBehavior)new MultiplayerPollComponent(),
					(MissionBehavior)new MultiplayerAdminComponent(),
					(MissionBehavior)new MultiplayerGameNotificationsComponent(),
					(MissionBehavior)new MissionOptionsComponent(),
					(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new BattleScoreboardData()),
					(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
					(MissionBehavior)new EquipmentControllerLeaveLogic(),
					(MissionBehavior)new MultiplayerPreloadHelper()
				};
			}
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[24]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MultiplayerRoundController(),
				(MissionBehavior)new MissionMultiplayerFlagDomination((MultiplayerGameType)3),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)new FlagDominationSpawnFrameBehavior(), (SpawningBehaviorBase)new FlagDominationSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new BattleScoreboardData()),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			};
		}

		internal IEnumerable<MissionBehavior> _003COpenCaptainMission_003Eb__4_0(Mission missionController)
		{
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Expected O, but got Unknown
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Expected O, but got Unknown
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Expected O, but got Unknown
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Expected O, but got Unknown
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Expected O, but got Unknown
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Expected O, but got Unknown
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Expected O, but got Unknown
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Expected O, but got Unknown
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Expected O, but got Unknown
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Expected O, but got Unknown
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Expected O, but got Unknown
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Expected O, but got Unknown
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Expected O, but got Unknown
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Expected O, but got Unknown
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Expected O, but got Unknown
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_006c: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected O, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Expected O, but got Unknown
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Expected O, but got Unknown
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Expected O, but got Unknown
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Expected O, but got Unknown
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Expected O, but got Unknown
			if (!GameNetwork.IsServer)
			{
				return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[23]
				{
					(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
					(MissionBehavior)new MultiplayerAchievementComponent(),
					(MissionBehavior)new MultiplayerWarmupComponent(),
					(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
					(MissionBehavior)new MultiplayerRoundComponent(),
					(MissionBehavior)new MultiplayerTimerComponent(),
					(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
					(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
					(MissionBehavior)new ConsoleMatchStartEndHandler(),
					(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
					(MissionBehavior)new MultiplayerTeamSelectComponent(),
					(MissionBehavior)new MissionHardBorderPlacer(),
					(MissionBehavior)new MissionBoundaryPlacer(),
					(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
					(MissionBehavior)new MultiplayerPollComponent(),
					(MissionBehavior)new MultiplayerAdminComponent(),
					(MissionBehavior)new MultiplayerGameNotificationsComponent(),
					(MissionBehavior)new MissionOptionsComponent(),
					(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new CaptainScoreboardData()),
					(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
					(MissionBehavior)new EquipmentControllerLeaveLogic(),
					(MissionBehavior)new MissionRecentPlayersComponent(),
					(MissionBehavior)new MultiplayerPreloadHelper()
				};
			}
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[25]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerFlagDomination((MultiplayerGameType)4),
				(MissionBehavior)new MultiplayerRoundController(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)new FlagDominationSpawnFrameBehavior(), (SpawningBehaviorBase)new FlagDominationSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new CaptainScoreboardData()),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			};
		}

		internal IEnumerable<MissionBehavior> _003COpenSkirmishMission_003Eb__5_0(Mission missionController)
		{
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Expected O, but got Unknown
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Expected O, but got Unknown
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Expected O, but got Unknown
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Expected O, but got Unknown
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Expected O, but got Unknown
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Expected O, but got Unknown
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Expected O, but got Unknown
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Expected O, but got Unknown
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Expected O, but got Unknown
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Expected O, but got Unknown
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Expected O, but got Unknown
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Expected O, but got Unknown
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Expected O, but got Unknown
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Expected O, but got Unknown
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Expected O, but got Unknown
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Expected O, but got Unknown
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_006c: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected O, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Expected O, but got Unknown
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Expected O, but got Unknown
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Expected O, but got Unknown
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Expected O, but got Unknown
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Expected O, but got Unknown
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Expected O, but got Unknown
			if (!GameNetwork.IsServer)
			{
				return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[24]
				{
					(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
					(MissionBehavior)new MultiplayerAchievementComponent(),
					(MissionBehavior)new MultiplayerWarmupComponent(),
					(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
					(MissionBehavior)new MultiplayerRoundComponent(),
					(MissionBehavior)new MultiplayerTimerComponent(),
					(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
					(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
					(MissionBehavior)new ConsoleMatchStartEndHandler(),
					(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
					(MissionBehavior)new MultiplayerTeamSelectComponent(),
					(MissionBehavior)new MissionHardBorderPlacer(),
					(MissionBehavior)new MissionBoundaryPlacer(),
					(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
					(MissionBehavior)new MultiplayerPollComponent(),
					(MissionBehavior)new MultiplayerAdminComponent(),
					(MissionBehavior)new MultiplayerGameNotificationsComponent(),
					(MissionBehavior)new MissionOptionsComponent(),
					(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new SkirmishScoreboardData()),
					(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
					(MissionBehavior)new EquipmentControllerLeaveLogic(),
					(MissionBehavior)new MissionRecentPlayersComponent(),
					(MissionBehavior)new VoiceChatHandler(),
					(MissionBehavior)new MultiplayerPreloadHelper()
				};
			}
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[26]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerFlagDomination((MultiplayerGameType)5),
				(MissionBehavior)new MultiplayerRoundController(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)new FlagDominationSpawnFrameBehavior(), (SpawningBehaviorBase)new FlagDominationSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new SkirmishScoreboardData()),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new VoiceChatHandler(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			};
		}
	}

	[MissionMethod]
	public static void OpenTeamDeathmatchMission(string scene)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		MissionInitializerRecord val = new MissionInitializerRecord(scene);
		object obj = _003C_003Ec._003C_003E9__0_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val2 = (Mission missionController) => (IEnumerable<MissionBehavior>)(object)((!GameNetwork.IsServer) ? new MissionBehavior[21]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerTeamDeathmatchClient(),
				(MissionBehavior)new MultiplayerAchievementComponent(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new TDMScoreboardData()),
				(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MissionRecentPlayersComponent(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			} : new MissionBehavior[22]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerTeamDeathmatch(),
				(MissionBehavior)new MissionMultiplayerTeamDeathmatchClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)(object)new TeamDeathmatchSpawnFrameBehavior(), (SpawningBehaviorBase)(object)new TeamDeathmatchSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new TDMScoreboardData()),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			});
			_003C_003Ec._003C_003E9__0_0 = val2;
			obj = (object)val2;
		}
		MissionState.OpenNew("MultiplayerTeamDeathmatch", val, (InitializeMissionBehaviorsDelegate)obj, true, true);
	}

	[MissionMethod]
	public static void OpenDuelMission(string scene)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		MissionInitializerRecord val = new MissionInitializerRecord(scene);
		object obj = _003C_003Ec._003C_003E9__1_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val2 = (Mission missionController) => (IEnumerable<MissionBehavior>)(object)((!GameNetwork.IsServer) ? new MissionBehavior[20]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerGameModeDuelClient(),
				(MissionBehavior)new MultiplayerAchievementComponent(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new DuelScoreboardData()),
				(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MissionRecentPlayersComponent(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			} : new MissionBehavior[21]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerDuel(),
				(MissionBehavior)new MissionMultiplayerGameModeDuelClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)(object)new DuelSpawnFrameBehavior(), (SpawningBehaviorBase)(object)new DuelSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new DuelScoreboardData()),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			});
			_003C_003Ec._003C_003E9__1_0 = val2;
			obj = (object)val2;
		}
		MissionState.OpenNew("MultiplayerDuel", val, (InitializeMissionBehaviorsDelegate)obj, true, true);
	}

	[MissionMethod]
	public static void OpenSiegeMission(string scene)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		MissionInitializerRecord val = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref val))._002Ector(scene);
		val.SceneUpgradeLevel = 3;
		val.SceneLevels = "";
		MissionInitializerRecord val2 = val;
		object obj = _003C_003Ec._003C_003E9__2_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val3 = (Mission missionController) => (IEnumerable<MissionBehavior>)(object)((!GameNetwork.IsServer) ? new MissionBehavior[22]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerSiegeClient(),
				(MissionBehavior)new MultiplayerAchievementComponent(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new SiegeScoreboardData()),
				(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MissionRecentPlayersComponent(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			} : new MissionBehavior[23]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerSiege(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerSiegeClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)new SiegeSpawnFrameBehavior(), (SpawningBehaviorBase)new SiegeSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new SiegeScoreboardData()),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			});
			_003C_003Ec._003C_003E9__2_0 = val3;
			obj = (object)val3;
		}
		MissionState.OpenNew("MultiplayerSiege", val2, (InitializeMissionBehaviorsDelegate)obj, true, true);
	}

	[MissionMethod]
	public static void OpenBattleMission(string scene)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		MissionInitializerRecord val = new MissionInitializerRecord(scene);
		object obj = _003C_003Ec._003C_003E9__3_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val2 = (Mission missionController) => (IEnumerable<MissionBehavior>)(object)((!GameNetwork.IsServer) ? new MissionBehavior[21]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MultiplayerRoundComponent(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new BattleScoreboardData()),
				(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			} : new MissionBehavior[24]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MultiplayerRoundController(),
				(MissionBehavior)new MissionMultiplayerFlagDomination((MultiplayerGameType)3),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)new FlagDominationSpawnFrameBehavior(), (SpawningBehaviorBase)new FlagDominationSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new BattleScoreboardData()),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			});
			_003C_003Ec._003C_003E9__3_0 = val2;
			obj = (object)val2;
		}
		MissionState.OpenNew("MultiplayerBattle", val, (InitializeMissionBehaviorsDelegate)obj, true, true);
	}

	[MissionMethod]
	public static void OpenCaptainMission(string scene)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		MissionInitializerRecord val = new MissionInitializerRecord(scene);
		object obj = _003C_003Ec._003C_003E9__4_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val2 = (Mission missionController) => (IEnumerable<MissionBehavior>)(object)((!GameNetwork.IsServer) ? new MissionBehavior[23]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MultiplayerAchievementComponent(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
				(MissionBehavior)new MultiplayerRoundComponent(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new CaptainScoreboardData()),
				(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MissionRecentPlayersComponent(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			} : new MissionBehavior[25]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerFlagDomination((MultiplayerGameType)4),
				(MissionBehavior)new MultiplayerRoundController(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)new FlagDominationSpawnFrameBehavior(), (SpawningBehaviorBase)new FlagDominationSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new CaptainScoreboardData()),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			});
			_003C_003Ec._003C_003E9__4_0 = val2;
			obj = (object)val2;
		}
		MissionState.OpenNew("MultiplayerCaptain", val, (InitializeMissionBehaviorsDelegate)obj, true, true);
	}

	[MissionMethod]
	public static void OpenSkirmishMission(string scene)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		MissionInitializerRecord val = new MissionInitializerRecord(scene);
		object obj = _003C_003Ec._003C_003E9__5_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val2 = (Mission missionController) => (IEnumerable<MissionBehavior>)(object)((!GameNetwork.IsServer) ? new MissionBehavior[24]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MultiplayerAchievementComponent(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
				(MissionBehavior)new MultiplayerRoundComponent(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new SkirmishScoreboardData()),
				(MissionBehavior)MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new MissionRecentPlayersComponent(),
				(MissionBehavior)new VoiceChatHandler(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			} : new MissionBehavior[26]
			{
				(MissionBehavior)MissionLobbyComponent.CreateBehavior(),
				(MissionBehavior)new MissionMultiplayerFlagDomination((MultiplayerGameType)5),
				(MissionBehavior)new MultiplayerRoundController(),
				(MissionBehavior)new MultiplayerWarmupComponent(),
				(MissionBehavior)new MissionMultiplayerGameModeFlagDominationClient(),
				(MissionBehavior)new MultiplayerTimerComponent(),
				(MissionBehavior)new MultiplayerBattleMissionAgentInteractionLogic(),
				(MissionBehavior)new MultiplayerMissionAgentVisualSpawnComponent(),
				(MissionBehavior)new ConsoleMatchStartEndHandler(),
				(MissionBehavior)new SpawnComponent((SpawnFrameBehaviorBase)new FlagDominationSpawnFrameBehavior(), (SpawningBehaviorBase)new FlagDominationSpawningBehavior()),
				(MissionBehavior)new MissionLobbyEquipmentNetworkComponent(),
				(MissionBehavior)new MultiplayerTeamSelectComponent(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MultiplayerPollComponent(),
				(MissionBehavior)new MultiplayerAdminComponent(),
				(MissionBehavior)new MultiplayerGameNotificationsComponent(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new MissionScoreboardComponent((IScoreboardData)(object)new SkirmishScoreboardData()),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new VoiceChatHandler(),
				(MissionBehavior)new MultiplayerPreloadHelper()
			});
			_003C_003Ec._003C_003E9__5_0 = val2;
			obj = (object)val2;
		}
		MissionState.OpenNew("MultiplayerSkirmish", val, (InitializeMissionBehaviorsDelegate)obj, true, true);
	}
}
