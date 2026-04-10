using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade.Missions.BattleScore;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

namespace TaleWorlds.MountAndBlade.CustomBattle;

[ViewCreatorModule]
public class CustomBattleViews
{
	[ViewMethod("CustomBattle")]
	public static MissionView[] OpenCustomBattleMission(Mission mission)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		List<MissionView> obj = new List<MissionView>
		{
			ViewCreator.CreateMissionSingleplayerEscapeMenu(false),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)new CustomBattleScoreboardVM((BattleScoreContext)new CustomBattleScoreContext(mission))),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission)
		};
		MissionView val = ViewCreator.CreateMissionOrderUIHandler((Mission)null);
		obj.Add(val);
		obj.Add((MissionView)new OrderTroopPlacer((OrderController)null));
		obj.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
		obj.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
		obj.Add(ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission));
		obj.Add(ViewCreator.CreateMissionAgentLockVisualizerView(mission));
		obj.Add((MissionView)new MusicBattleMissionView(false));
		obj.Add((MissionView)new DeploymentMissionView());
		ISiegeDeploymentView val2 = (ISiegeDeploymentView)(object)((val is ISiegeDeploymentView) ? val : null);
		obj.Add((MissionView)new MissionEntitySelectionUIHandler((Action<WeakGameEntity>)val2.OnEntitySelection, (Action<WeakGameEntity>)val2.OnEntityHover));
		obj.Add((MissionView)new MissionFormationTargetSelectionHandler());
		obj.Add(ViewCreator.CreateMissionBoundaryCrossingView());
		obj.Add((MissionView)new MissionBoundaryWallView());
		obj.Add((MissionView)new MissionDeploymentBoundaryMarker("swallowtail_banner", 2f));
		obj.Add(ViewCreator.CreateMissionFormationMarkerUIHandler(mission));
		obj.Add(ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler());
		obj.Add(ViewCreator.CreateMissionSpectatorControlView(mission));
		obj.Add(ViewCreator.CreatePhotoModeView());
		obj.Add((MissionView)new MissionItemContourControllerView());
		obj.Add((MissionView)new MissionAgentContourControllerView());
		obj.Add((MissionView)new MissionCustomBattlePreloadView());
		obj.Add(ViewCreator.CreateMissionOrderOfBattleUIHandler(mission, new OrderOfBattleVM()));
		obj.Add(ViewCreator.CreateMissionObjectiveView(mission));
		obj.Add((MissionView)new MissionFaceCacheView());
		return obj.ToArray();
	}

	[ViewMethod("CustomSiegeBattle")]
	public static MissionView[] OpenCustomSiegeBattleMission(Mission mission)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		List<MissionView> list = new List<MissionView>();
		mission.GetMissionBehavior<SiegeDeploymentHandler>();
		list.Add(ViewCreator.CreateMissionSingleplayerEscapeMenu(false));
		list.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission));
		list.Add(ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)new CustomBattleScoreboardVM((BattleScoreContext)new CustomBattleScoreContext(mission))));
		list.Add(ViewCreator.CreateOptionsUIHandler());
		list.Add(ViewCreator.CreateMissionMainAgentEquipDropView(mission));
		MissionView val = ViewCreator.CreateMissionOrderUIHandler((Mission)null);
		list.Add(val);
		list.Add((MissionView)new OrderTroopPlacer((OrderController)null));
		list.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
		list.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
		list.Add(ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission));
		list.Add(ViewCreator.CreateMissionAgentLockVisualizerView(mission));
		list.Add((MissionView)new MusicBattleMissionView(true));
		list.Add((MissionView)new DeploymentMissionView());
		ISiegeDeploymentView val2 = (ISiegeDeploymentView)(object)((val is ISiegeDeploymentView) ? val : null);
		list.Add((MissionView)new MissionEntitySelectionUIHandler((Action<WeakGameEntity>)val2.OnEntitySelection, (Action<WeakGameEntity>)val2.OnEntityHover));
		list.Add((MissionView)new MissionFormationTargetSelectionHandler());
		list.Add(ViewCreator.CreateMissionBoundaryCrossingView());
		list.Add((MissionView)new MissionDeploymentBoundaryMarker("swallowtail_banner", 2f));
		list.Add(ViewCreator.CreateMissionFormationMarkerUIHandler(mission));
		list.Add(ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler());
		list.Add(ViewCreator.CreateMissionSpectatorControlView(mission));
		list.Add(ViewCreator.CreatePhotoModeView());
		list.Add((MissionView)new SiegeDeploymentVisualizationMissionView());
		list.Add((MissionView)new MissionItemContourControllerView());
		list.Add((MissionView)new MissionAgentContourControllerView());
		list.Add((MissionView)new MissionCustomBattlePreloadView());
		list.Add(ViewCreator.CreateMissionSiegeEngineMarkerView(mission));
		list.Add(ViewCreator.CreateMissionOrderOfBattleUIHandler(mission, new OrderOfBattleVM()));
		return list.ToArray();
	}

	[ViewMethod("CustomBattleLordsHall")]
	public static MissionView[] OpenCustomBattleLordsHallMission(Mission mission)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		return new List<MissionView>
		{
			ViewCreator.CreateMissionSingleplayerEscapeMenu(false),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionBattleScoreUIHandler(mission, (ScoreboardBaseVM)new CustomBattleScoreboardVM((BattleScoreContext)new CustomBattleScoreContext(mission))),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateMissionOrderUIHandler((Mission)null),
			(MissionView)new OrderTroopPlacer((OrderController)null),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
			ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionSpectatorControlView(mission),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			ViewCreator.CreatePhotoModeView(),
			(MissionView)new MissionCustomBattlePreloadView()
		}.ToArray();
	}
}
