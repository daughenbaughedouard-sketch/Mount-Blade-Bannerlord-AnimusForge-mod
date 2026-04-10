using NavalDLC.View.MissionViews.Order;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

namespace NavalDLC.View.MissionViews;

public class NavalDeploymentMissionView : DeploymentMissionView
{
	public override void AfterStart()
	{
		base._orderTroopPlacer = (OrderTroopPlacer)(object)((MissionBehavior)this).Mission.GetMissionBehavior<NavalOrderTroopPlacer>();
		base._deploymentBoundaryMarkerHandler = (MissionDeploymentBoundaryMarker)(object)((MissionBehavior)this).Mission.GetMissionBehavior<NavalMissionDeploymentBoundaryMarker>();
	}
}
