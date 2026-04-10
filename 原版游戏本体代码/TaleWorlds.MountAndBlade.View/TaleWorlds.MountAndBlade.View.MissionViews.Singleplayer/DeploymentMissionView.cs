using TaleWorlds.MountAndBlade.View.MissionViews.Order;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

public class DeploymentMissionView : MissionView
{
	protected OrderTroopPlacer _orderTroopPlacer;

	protected MissionDeploymentBoundaryMarker _deploymentBoundaryMarkerHandler;

	protected MissionEntitySelectionUIHandler _entitySelectionHandler;

	public override void AfterStart()
	{
		_orderTroopPlacer = ((MissionBehavior)this).Mission.GetMissionBehavior<OrderTroopPlacer>();
		_entitySelectionHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionEntitySelectionUIHandler>();
		_deploymentBoundaryMarkerHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionDeploymentBoundaryMarker>();
	}

	public override void OnDeploymentPlanMade(Team team, bool isFirstPlan)
	{
		if (team == ((MissionBehavior)this).Mission.PlayerTeam && ((MissionBehavior)this).Mission.DeploymentPlan.HasDeploymentBoundaries(((MissionBehavior)this).Mission.PlayerTeam))
		{
			_orderTroopPlacer?.RestrictOrdersToDeploymentBoundaries(enabled: true);
		}
	}

	public override void OnDeploymentFinished()
	{
		if (_entitySelectionHandler != null)
		{
			((MissionBehavior)this).Mission.RemoveMissionBehavior((MissionBehavior)(object)_entitySelectionHandler);
		}
		if (_deploymentBoundaryMarkerHandler != null)
		{
			if (((MissionBehavior)this).Mission.DeploymentPlan.HasDeploymentBoundaries(((MissionBehavior)this).Mission.PlayerTeam))
			{
				_orderTroopPlacer?.RestrictOrdersToDeploymentBoundaries(enabled: false);
			}
			((MissionBehavior)this).Mission.RemoveMissionBehavior((MissionBehavior)(object)_deploymentBoundaryMarkerHandler);
		}
		if (!((MissionBehavior)this).Mission.HasMissionBehavior<MissionBoundaryWallView>())
		{
			MissionBoundaryWallView missionView = new MissionBoundaryWallView();
			base.MissionScreen.AddMissionView(missionView);
		}
	}
}
