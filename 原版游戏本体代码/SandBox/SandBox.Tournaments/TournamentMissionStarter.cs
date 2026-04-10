using System.Collections.Generic;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.MissionLogics.Arena;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace SandBox.Tournaments;

[MissionManager]
public static class TournamentMissionStarter
{
	[MissionMethod]
	public static Mission OpenTournamentArcheryMission(string scene, TournamentGame tournamentGame, Settlement settlement, CultureObject culture, bool isPlayerParticipating)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		return MissionState.OpenNew("TournamentArchery", SandBoxMissions.CreateSandBoxMissionInitializerRecord(scene, "", doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Expected O, but got Unknown
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Expected O, but got Unknown
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected O, but got Unknown
			TournamentArcheryMissionController tournamentArcheryMissionController = new TournamentArcheryMissionController(culture);
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[12]
			{
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)tournamentArcheryMissionController,
				(MissionBehavior)new TournamentBehavior(tournamentGame, settlement, tournamentArcheryMissionController, isPlayerParticipating),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new ArenaAgentStateDeciderLogic(),
				(MissionBehavior)new BasicLeaveMissionLogic(true),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionOptionsComponent()
			};
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenTournamentFightMission(string scene, TournamentGame tournamentGame, Settlement settlement, CultureObject culture, bool isPlayerParticipating)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		return MissionState.OpenNew("TournamentFight", SandBoxMissions.CreateSandBoxMissionInitializerRecord(scene, "", doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Expected O, but got Unknown
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Expected O, but got Unknown
			TournamentFightMissionController tournamentFightMissionController = new TournamentFightMissionController(culture);
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[13]
			{
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)tournamentFightMissionController,
				(MissionBehavior)new TournamentBehavior(tournamentGame, settlement, tournamentFightMissionController, isPlayerParticipating),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new ArenaAgentStateDeciderLogic(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new SandboxHighlightsController()
			};
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenTournamentHorseRaceMission(string scene, TournamentGame tournamentGame, Settlement settlement, CultureObject culture, bool isPlayerParticipating)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		return MissionState.OpenNew("TournamentHorseRace", SandBoxMissions.CreateSandBoxMissionInitializerRecord(scene, "", doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Expected O, but got Unknown
			TownHorseRaceMissionController townHorseRaceMissionController = new TownHorseRaceMissionController(culture);
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[11]
			{
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)townHorseRaceMissionController,
				(MissionBehavior)new TournamentBehavior(tournamentGame, settlement, townHorseRaceMissionController, isPlayerParticipating),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new ArenaAgentStateDeciderLogic(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionOptionsComponent()
			};
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenTournamentJoustingMission(string scene, TournamentGame tournamentGame, Settlement settlement, CultureObject culture, bool isPlayerParticipating)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		return MissionState.OpenNew("TournamentJousting", SandBoxMissions.CreateSandBoxMissionInitializerRecord(scene, "", doNotUseLoadingScreen: false, (DecalAtlasGroup)3), (InitializeMissionBehaviorsDelegate)delegate
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Expected O, but got Unknown
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			TournamentJoustingMissionController tournamentJoustingMissionController = new TournamentJoustingMissionController(culture);
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[12]
			{
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)tournamentJoustingMissionController,
				(MissionBehavior)new TournamentBehavior(tournamentGame, settlement, tournamentJoustingMissionController, isPlayerParticipating),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new ArenaAgentStateDeciderLogic(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new MissionOptionsComponent()
			};
		}, true, true);
	}

	[MissionMethod]
	public static Mission OpenBattleChallengeMission(string scene, IList<Hero> priorityCharsAttacker, IList<Hero> priorityCharsDefender)
	{
		return null;
	}
}
