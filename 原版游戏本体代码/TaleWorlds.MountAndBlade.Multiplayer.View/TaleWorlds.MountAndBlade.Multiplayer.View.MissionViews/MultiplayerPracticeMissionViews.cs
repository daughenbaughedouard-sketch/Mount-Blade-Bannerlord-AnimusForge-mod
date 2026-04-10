using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade.Missions.BattleScore;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

namespace TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;

[ViewCreatorModule]
public static class MultiplayerPracticeMissionViews
{
	[ViewMethod("MultiplayerPractice")]
	public static MissionView[] OpenMultiplayerPracticeMission(Mission mission)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected O, but got Unknown
		List<MissionView> obj = new List<MissionView>
		{
			MultiplayerViewCreator.CreateMissionMultiplayerPracticeEscapeMenu(),
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
		obj.Add((MissionView)new DeploymentMissionView());
		ISiegeDeploymentView val2 = (ISiegeDeploymentView)(object)((val is ISiegeDeploymentView) ? val : null);
		obj.Add((MissionView)new MissionEntitySelectionUIHandler((Action<WeakGameEntity>)val2.OnEntitySelection, (Action<WeakGameEntity>)val2.OnEntityHover));
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
		return obj.ToArray();
	}
}
