using NavalDLC.View.MissionViews;
using NavalDLC.View.MissionViews.Storyline;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(NavalCaptivityBattleMissionView))]
public class MissionGauntletCaptivityMissionView : NavalCaptivityBattleMissionView
{
	private bool _hasHandledOarsmenLevel;

	protected override void OnFirstHighlightClearedInternal()
	{
		MissionGauntletShipControlView missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletShipControlView>();
		if (missionBehavior != null && ((MissionView)missionBehavior).IsReady())
		{
			missionBehavior.ResumeFeature(MissionGauntletShipControlView.ShipControlFeatureFlags.ToggleSails);
		}
	}

	protected override void OnPlayerStartedEscapeInternal()
	{
		MissionGauntletShipControlView missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletShipControlView>();
		if (missionBehavior != null && ((MissionView)missionBehavior).IsReady())
		{
			missionBehavior.SuspendFeature(MissionGauntletShipControlView.ShipControlFeatureFlags.ToggleSails);
			missionBehavior.SuspendFeature(MissionGauntletShipControlView.ShipControlFeatureFlags.ChangeCamera);
			missionBehavior.SetActiveCameraMode(MissionShipControlView.CameraModes.Shoulder);
		}
	}

	protected override void OnOarsmenLevelChangedInternal(int level)
	{
		if (!_hasHandledOarsmenLevel && level == 2)
		{
			_hasHandledOarsmenLevel = true;
			((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletShipControlView>().ResumeFeature(MissionGauntletShipControlView.ShipControlFeatureFlags.ChangeCamera);
		}
	}
}
