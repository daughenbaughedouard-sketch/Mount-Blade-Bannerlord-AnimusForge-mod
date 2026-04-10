using System;
using System.Collections.Generic;
using SandBox.View.Missions.Sound.Components;
using SandBox.View.Missions.Tournaments;
using SandBox.ViewModelCollection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

namespace SandBox.View.Missions;

[ViewCreatorModule]
public class SandBoxMissionViews
{
	[ViewMethod("TownCenter")]
	public static MissionView[] OpenTownCenterMission(Mission mission)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			ViewCreator.CreateMissionLeaveView(),
			ViewCreator.CreatePhotoModeView(),
			SandBoxViewCreator.CreateMissionBarterView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView()
		}.ToArray();
	}

	[ViewMethod("FacialAnimationTest")]
	public static MissionView[] OpenFacialAnimationTest(Mission mission)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			SandBoxViewCreator.CreateMissionBarterView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("Indoor")]
	public static MissionView[] OpenTavernMission(Mission mission)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			(MissionView)new MusicSilencedMissionView(),
			SandBoxViewCreator.CreateMissionBarterView(),
			ViewCreator.CreateMissionLeaveView(),
			SandBoxViewCreator.CreateBoardGameView(),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
		}.ToArray();
	}

	[ViewMethod("PrisonBreak")]
	public static MissionView[] OpenPrisonBreakMission(Mission mission)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			(MissionView)new MusicSilencedMissionView(),
			ViewCreator.CreateMissionLeaveView(),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			SandBoxViewCreator.CreateMissionStealthFailCounter(),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			SandBoxViewCreator.CreateMissionAgentAlarmStateView(mission),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
		}.ToArray();
	}

	[ViewMethod("Village")]
	public static MissionView[] OpenVillageMission(Mission mission)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			SandBoxViewCreator.CreateMissionBarterView(),
			ViewCreator.CreateMissionLeaveView(),
			(MissionView)new MissionBoundaryWallView(),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
		}.ToArray();
	}

	[ViewMethod("Retirement")]
	public static MissionView[] OpenRetirementMission(Mission mission)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			SandBoxViewCreator.CreateMissionBarterView(),
			ViewCreator.CreateMissionLeaveView(),
			(MissionView)new MissionBoundaryWallView(),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("ArenaPracticeFight")]
	public static MissionView[] OpenArenaStartMission(Mission mission)
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			(MissionView)(object)new MissionAudienceHandler(0.4f + MBRandom.RandomFloat * 0.3f),
			SandBoxViewCreator.CreateMissionArenaPracticeFightView(),
			ViewCreator.CreateMissionLeaveView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			(MissionView)(object)new MusicArenaPracticeMissionView(),
			SandBoxViewCreator.CreateMissionBarterView(),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			(MissionView)(object)new ArenaPreloadView()
		}.ToArray();
	}

	[ViewMethod("ArenaDuelMission")]
	public static MissionView[] OpenArenaDuelMission(Mission mission)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionLeaveView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			(MissionView)new MusicSilencedMissionView(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			(MissionView)(object)new MissionAudienceHandler(0.4f + MBRandom.RandomFloat * 0.3f),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("TownMerchant")]
	public static MissionView[] OpenTownMerchantMission(Mission mission)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionLeaveView(),
			SandBoxViewCreator.CreateMissionBarterView(),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
		}.ToArray();
	}

	[ViewMethod("Alley")]
	public static MissionView[] OpenAlleyMission(Mission mission)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateMissionLeaveView(),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			SandBoxViewCreator.CreateMissionBarterView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
		}.ToArray();
	}

	[ViewMethod("SneakTeam3")]
	public static MissionView[] OpenSneakTeam3Mission(Mission mission)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
		}.ToArray();
	}

	[ViewMethod("SimpleMountedPlayer")]
	public static MissionView[] OpenSimpleMountedPlayerMission(Mission mission)
	{
		return new List<MissionView>().ToArray();
	}

	[ViewMethod("Battle")]
	public static MissionView[] OpenBattleMission(Mission mission)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Expected O, but got Unknown
		List<MissionView> obj = new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission)
		};
		MissionView val = ViewCreator.CreateMissionOrderUIHandler((Mission)null);
		obj.Add(val);
		obj.Add((MissionView)new OrderTroopPlacer((OrderController)null));
		obj.Add((MissionView)(object)new MissionSingleplayerViewHandler());
		obj.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
		obj.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
		obj.Add(ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission));
		obj.Add(ViewCreator.CreateMissionAgentLockVisualizerView(mission));
		obj.Add((MissionView)new MusicBattleMissionView(false));
		obj.Add((MissionView)new DeploymentMissionView());
		obj.Add((MissionView)new MissionDeploymentBoundaryMarker("swallowtail_banner", 2f));
		obj.Add(ViewCreator.CreateMissionBoundaryCrossingView());
		obj.Add((MissionView)new MissionBoundaryWallView());
		obj.Add(ViewCreator.CreateMissionFormationMarkerUIHandler(mission));
		obj.Add((MissionView)new MissionFormationTargetSelectionHandler());
		obj.Add(ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler());
		obj.Add(ViewCreator.CreateMissionSpectatorControlView(mission));
		obj.Add((MissionView)new MissionItemContourControllerView());
		obj.Add((MissionView)new MissionAgentContourControllerView());
		obj.Add((MissionView)(object)new MissionPreloadView());
		obj.Add((MissionView)(object)new MissionCampaignBattleSpectatorView());
		obj.Add(ViewCreator.CreatePhotoModeView());
		obj.Add((MissionView)new MissionFaceCacheView());
		ISiegeDeploymentView val2 = (ISiegeDeploymentView)(object)((val is ISiegeDeploymentView) ? val : null);
		obj.Add((MissionView)new MissionEntitySelectionUIHandler((Action<WeakGameEntity>)val2.OnEntitySelection, (Action<WeakGameEntity>)val2.OnEntityHover));
		obj.Add(ViewCreator.CreateMissionOrderOfBattleUIHandler(mission, (OrderOfBattleVM)(object)new SPOrderOfBattleVM()));
		return obj.ToArray();
	}

	[ViewMethod("AlleyFight")]
	public static MissionView[] OpenAlleyFightMission(Mission mission)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
			(MissionView)new MissionFormationTargetSelectionHandler(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("HideoutBattle")]
	public static MissionView[] OpenHideoutBattleMission(Mission mission)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			(MissionView)(object)new MissionHideoutCinematicView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			(MissionView)new MusicSilencedMissionView(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
			(MissionView)new MissionFormationTargetSelectionHandler(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			(MissionView)(object)new MissionPreloadView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateMissionObjectiveView((Mission)null)
		}.ToArray();
	}

	[ViewMethod("HideoutAmbushMission")]
	public static MissionView[] OpenHideoutAmbushMission(Mission mission)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			(MissionView)new MusicStealthMissionView(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			SandBoxViewCreator.CreateMissionAgentAlarmStateView(mission),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateMissionLeaveView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			SandBoxViewCreator.CreateMissionStealthFailCounter(),
			(MissionView)new OrderTroopPlacer((OrderController)null),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
			(MissionView)(object)new MissionPreloadView(),
			ViewCreatorManager.CreateMissionView<MissionHideoutAmbushCinematicView>(mission != null, mission, Array.Empty<object>()),
			ViewCreatorManager.CreateMissionView<MissionHideoutAmbushBossFightCinematicView>(mission != null, mission, Array.Empty<object>()),
			ViewCreator.CreateMissionObjectiveView((Mission)null)
		}.ToArray();
	}

	[ViewMethod("EnteringSettlementBattle")]
	public static MissionView[] OpenBattleMissionWhileEnteringSettlement(Mission mission)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
			(MissionView)new MissionFormationTargetSelectionHandler(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("CombatWithDialogue")]
	public static MissionView[] OpenCombatMissionWithDialogue(Mission mission)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
			(MissionView)new MissionFormationTargetSelectionHandler(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("SiegeEngine")]
	public static MissionView[] OpenTestSiegeEngineMission(Mission mission)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null)
		}.ToArray();
	}

	[ViewMethod("CustomCameraMission")]
	public static MissionView[] OpenCustomCameraMission(Mission mission)
	{
		return new List<MissionView>
		{
			(MissionView)(object)new MissionConversationCameraView(),
			(MissionView)(object)new MissionCustomCameraView()
		}.ToArray();
	}

	[ViewMethod("AmbushBattle")]
	public static MissionView[] OpenAmbushBattleMission(Mission mission)
	{
		throw new NotImplementedException("Ambush battle is not implemented.");
	}

	[ViewMethod("Camp")]
	public static MissionView[] OpenCampMission(Mission mission)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
		}.ToArray();
	}

	[ViewMethod("SiegeMissionWithDeployment")]
	public static MissionView[] OpenSiegeMissionWithDeployment(Mission mission)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Expected O, but got Unknown
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		List<MissionView> list = new List<MissionView>();
		mission.GetMissionBehavior<SiegeDeploymentHandler>();
		list.Add((MissionView)(object)new MissionCampaignView());
		list.Add((MissionView)(object)new MissionConversationCameraView());
		list.Add(ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode));
		list.Add(ViewCreator.CreateOptionsUIHandler());
		list.Add(ViewCreator.CreateMissionMainAgentEquipDropView(mission));
		list.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission));
		list.Add(ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)));
		MissionView val = ViewCreator.CreateMissionOrderUIHandler((Mission)null);
		list.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
		list.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
		list.Add(ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission));
		list.Add(ViewCreator.CreateMissionAgentLockVisualizerView(mission));
		list.Add(val);
		list.Add((MissionView)new OrderTroopPlacer((OrderController)null));
		list.Add((MissionView)(object)new MissionSingleplayerViewHandler());
		list.Add((MissionView)new MusicBattleMissionView(true));
		list.Add((MissionView)new DeploymentMissionView());
		list.Add((MissionView)new MissionDeploymentBoundaryMarker("swallowtail_banner", 2f));
		list.Add(ViewCreator.CreateMissionBoundaryCrossingView());
		list.Add(ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler());
		list.Add(ViewCreator.CreatePhotoModeView());
		list.Add(ViewCreator.CreateMissionFormationMarkerUIHandler(mission));
		list.Add((MissionView)new MissionFormationTargetSelectionHandler());
		ISiegeDeploymentView val2 = (ISiegeDeploymentView)(object)((val is ISiegeDeploymentView) ? val : null);
		list.Add((MissionView)new MissionEntitySelectionUIHandler((Action<WeakGameEntity>)val2.OnEntitySelection, (Action<WeakGameEntity>)val2.OnEntityHover));
		list.Add(ViewCreator.CreateMissionSpectatorControlView(mission));
		list.Add((MissionView)new MissionItemContourControllerView());
		list.Add((MissionView)new MissionAgentContourControllerView());
		list.Add((MissionView)(object)new MissionPreloadView());
		list.Add((MissionView)(object)new MissionCampaignBattleSpectatorView());
		list.Add(ViewCreator.CreateMissionOrderOfBattleUIHandler(mission, (OrderOfBattleVM)(object)new SPOrderOfBattleVM()));
		list.Add(ViewCreator.CreateMissionSiegeEngineMarkerView(mission));
		list.Add((MissionView)new MissionFaceCacheView());
		return list.ToArray();
	}

	[ViewMethod("SiegeMissionNoDeployment")]
	public static MissionView[] OpenSiegeMissionNoDeployment(Mission mission)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
			(MissionView)new MissionFormationTargetSelectionHandler(),
			(MissionView)new MusicBattleMissionView(true),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionPreloadView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreateMissionSiegeEngineMarkerView(mission)
		}.ToArray();
	}

	[ViewMethod("SiegeLordsHallFightMission")]
	public static MissionView[] OpenSiegeLordsHallFightMission(Mission mission)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
			(MissionView)new MissionFormationTargetSelectionHandler(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionPreloadView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("Siege")]
	public static MissionView[] OpenSiegeMission(Mission mission)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		List<MissionView> list = new List<MissionView>();
		mission.GetMissionBehavior<SiegeDeploymentHandler>();
		list.Add((MissionView)(object)new MissionCampaignView());
		list.Add((MissionView)(object)new MissionConversationCameraView());
		list.Add(ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode));
		list.Add(ViewCreator.CreateOptionsUIHandler());
		list.Add(ViewCreator.CreateMissionMainAgentEquipDropView(mission));
		MissionView val = ViewCreator.CreateMissionOrderUIHandler((Mission)null);
		list.Add(val);
		list.Add((MissionView)new OrderTroopPlacer((OrderController)null));
		list.Add((MissionView)(object)new MissionSingleplayerViewHandler());
		list.Add((MissionView)new DeploymentMissionView());
		list.Add((MissionView)new MissionDeploymentBoundaryMarker("swallowtail_banner", 2f));
		list.Add(ViewCreator.CreateMissionBoundaryCrossingView());
		list.Add(ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler());
		list.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
		list.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
		list.Add(ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission));
		list.Add(ViewCreator.CreateMissionAgentLockVisualizerView(mission));
		list.Add(ViewCreator.CreateMissionSpectatorControlView(mission));
		list.Add(ViewCreator.CreatePhotoModeView());
		ISiegeDeploymentView val2 = (ISiegeDeploymentView)(object)((val is ISiegeDeploymentView) ? val : null);
		list.Add((MissionView)new MissionEntitySelectionUIHandler((Action<WeakGameEntity>)val2.OnEntitySelection, (Action<WeakGameEntity>)val2.OnEntityHover));
		list.Add(ViewCreator.CreateMissionFormationMarkerUIHandler(mission));
		list.Add((MissionView)new MissionFormationTargetSelectionHandler());
		list.Add((MissionView)new MissionItemContourControllerView());
		list.Add((MissionView)new MissionAgentContourControllerView());
		list.Add((MissionView)(object)new MissionCampaignBattleSpectatorView());
		list.Add(ViewCreator.CreateMissionSiegeEngineMarkerView(mission));
		list.Add((MissionView)new MissionFaceCacheView());
		return list.ToArray();
	}

	[ViewMethod("SiegeMissionForTutorial")]
	public static MissionView[] OpenSiegeMissionForTutorial(Mission mission)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		Debug.FailedAssert("Do not use SiegeForTutorial! Use campaign!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Missions\\SandBoxMissionViews.cs", "OpenSiegeMissionForTutorial", 883);
		List<MissionView> obj = new List<MissionView>
		{
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission)
		};
		MissionView val = ViewCreator.CreateMissionOrderUIHandler((Mission)null);
		obj.Add(val);
		obj.Add((MissionView)new OrderTroopPlacer((OrderController)null));
		obj.Add((MissionView)(object)new MissionSingleplayerViewHandler());
		obj.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
		obj.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
		obj.Add(ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission));
		obj.Add(ViewCreator.CreateMissionAgentLockVisualizerView(mission));
		obj.Add(ViewCreator.CreateMissionSpectatorControlView(mission));
		obj.Add(ViewCreator.CreatePhotoModeView());
		obj.Add(ViewCreator.CreateMissionSiegeEngineMarkerView(mission));
		ISiegeDeploymentView val2 = (ISiegeDeploymentView)(object)((val is ISiegeDeploymentView) ? val : null);
		obj.Add((MissionView)new MissionEntitySelectionUIHandler((Action<WeakGameEntity>)val2.OnEntitySelection, (Action<WeakGameEntity>)val2.OnEntityHover));
		obj.Add((MissionView)new MissionDeploymentBoundaryMarker("swallowtail_banner", 2f));
		obj.Add((MissionView)(object)new MissionCampaignBattleSpectatorView());
		return obj.ToArray();
	}

	[ViewMethod("FormationTest")]
	public static MissionView[] OpenFormationTestMission(Mission mission)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null)
		}.ToArray();
	}

	[ViewMethod("VillageBattle")]
	public static MissionView[] OpenVillageBattleMission(Mission mission)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)(object)SPScoreboardVM.CreateMission(mission)),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
			(MissionView)new MissionFormationTargetSelectionHandler(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler()
		}.ToArray();
	}

	[ViewMethod("SettlementTest")]
	public static MissionView[] OpenSettlementTestMission(Mission mission)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("EquipmentTest")]
	public static MissionView[] OpenEquipmentTestMission(Mission mission)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("FacialAnimTest")]
	public static MissionView[] OpenFacialAnimTestMission(Mission mission)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			SandBoxViewCreator.CreateMissionBarterView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView()
		}.ToArray();
	}

	[ViewMethod("EquipItemTool")]
	public static MissionView[] OpenEquipItemToolMission(Mission mission)
	{
		return new List<MissionView>
		{
			(MissionView)(object)new MissionConversationCameraView(),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionEquipItemToolView(),
			ViewCreator.CreateMissionLeaveView()
		}.ToArray();
	}

	[ViewMethod("Conversation")]
	public static MissionView[] OpenConversationMission(Mission mission)
	{
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			SandBoxViewCreator.CreateMissionBarterView(),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			ViewCreator.CreatePhotoModeView(),
			(MissionView)(object)new MissionConversationPrepareView()
		}.ToArray();
	}

	[ViewMethod("ShadowingATargetMission")]
	public static MissionView[] OpenShadowingATargetMission(Mission mission)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			(MissionView)new MusicStealthMissionView(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			ViewCreator.CreateMissionLeaveView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			SandBoxViewCreator.CreateMissionAgentAlarmStateView(mission),
			SandBoxViewCreator.CreateMissionMainAgentDetectionView(),
			ViewCreatorManager.CreateMissionView<EavesdroppingMissionCameraView>(mission != null, mission, Array.Empty<object>())
		}.ToArray();
	}

	[ViewMethod("DisguiseMission")]
	public static MissionView[] OpenDisguiseMission(Mission mission)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		return new List<MissionView>
		{
			(MissionView)(object)new MissionCampaignView(),
			(MissionView)(object)new MissionConversationCameraView(),
			SandBoxViewCreator.CreateMissionConversationView(mission),
			ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			(MissionView)(object)new MissionSingleplayerViewHandler(),
			(MissionView)new MusicStealthMissionView(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionAgentLockVisualizerView(mission),
			SandBoxViewCreator.CreateMissionNameMarkerUIHandler(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			ViewCreator.CreateMissionLeaveView(),
			ViewCreator.CreatePhotoModeView(),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)(object)new MissionCampaignBattleSpectatorView(),
			SandBoxViewCreator.CreateMissionAgentAlarmStateView(mission),
			SandBoxViewCreator.CreateMissionMainAgentDetectionView(),
			(MissionView)(object)new StealthMissionUIHandler()
		}.ToArray();
	}
}
