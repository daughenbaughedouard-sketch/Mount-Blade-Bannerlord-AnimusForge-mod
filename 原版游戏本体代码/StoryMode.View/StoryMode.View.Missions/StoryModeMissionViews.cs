using System.Collections.Generic;
using SandBox.View;
using SandBox.View.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound;

namespace StoryMode.View.Missions;

[ViewCreatorModule]
public class StoryModeMissionViews
{
	[ViewMethod("TrainingField")]
	public static MissionView[] OpenVillageMission(Mission mission)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)new MissionCampaignView(),
			(MissionView)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			SandBoxViewCreator.CreateMissionBarterView(),
			ViewCreator.CreateMissionLeaveView(),
			ViewCreator.CreatePhotoModeView(),
			(MissionView)new MissionBoundaryWallView(),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			StoryModeViewCreator.CreateTrainingFieldObjectiveView(mission),
			(MissionView)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
		}.ToArray();
	}

	[ViewMethod("SneakIntoTheVillaMission")]
	public static MissionView[] OpenSneakIntoTheVillaMission(Mission mission)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)new MissionCampaignView(),
			(MissionView)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			(MissionView)new MissionSingleplayerViewHandler(),
			(MissionView)new MusicStealthMissionView(),
			(MissionView)(object)new StealthTutorialView(),
			SandBoxViewCreator.CreateMissionStealthFailCounter((Mission)null),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			SandBoxViewCreator.CreateMissionAgentAlarmStateView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateMissionLeaveView()
		}.ToArray();
	}
}
