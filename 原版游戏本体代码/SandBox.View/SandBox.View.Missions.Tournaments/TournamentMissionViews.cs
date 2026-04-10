using System.Collections.Generic;
using SandBox.View.Missions.Sound.Components;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

namespace SandBox.View.Missions.Tournaments;

[ViewCreatorModule]
public class TournamentMissionViews
{
	[ViewMethod("TournamentArchery")]
	public static MissionView[] OpenTournamentArcheryMission(Mission mission)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			SandBoxViewCreator.CreateMissionTournamentView(),
			(MissionView)(object)new MissionAudienceHandler(0.4f + MBRandom.RandomFloat * 0.6f),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			(MissionView)new MissionMessageUIHandler(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			ViewCreator.CreatePhotoModeView(),
			(MissionView)(object)new ArenaPreloadView()
		}.ToArray();
	}

	[ViewMethod("TournamentFight")]
	public static MissionView[] OpenTournamentFightMission(Mission mission)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			SandBoxViewCreator.CreateMissionTournamentView(),
			(MissionView)(object)new MissionAudienceHandler(0.4f + MBRandom.RandomFloat * 0.6f),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)(object)new MusicTournamentMissionView(),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			(MissionView)(object)new ArenaPreloadView()
		}.ToArray();
	}

	[ViewMethod("TournamentHorseRace")]
	public static MissionView[] OpenTournamentHorseRaceMission(Mission mission)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)(object)new MissionTournamentView(),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			(MissionView)(object)new MissionAudienceHandler(0.4f + MBRandom.RandomFloat * 0.6f),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			(MissionView)(object)new ArenaPreloadView()
		}.ToArray();
	}

	[ViewMethod("TournamentJousting")]
	public static MissionView[] OpenTournamentJoustingMission(Mission mission)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			SandBoxViewCreator.CreateMissionTournamentView(),
			(MissionView)(object)new MissionAudienceHandler(0.4f + MBRandom.RandomFloat * 0.6f),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			(MissionView)new MissionMessageUIHandler(),
			(MissionView)new MissionScoreUIHandler(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			(MissionView)(object)new MissionTournamentJoustingView(),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			(MissionView)(object)new ArenaPreloadView()
		}.ToArray();
	}
}
