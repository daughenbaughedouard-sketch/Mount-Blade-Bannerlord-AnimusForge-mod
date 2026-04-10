using NavalDLC.View.MissionViews;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(NavalStorylinePirateBattleMissionView))]
internal class MissionGauntletPirateBattleMissionView : NavalStorylinePirateBattleMissionView
{
	protected override void OnShipsInitializedInternal()
	{
		MissionGauntletShipControlView missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletShipControlView>();
		if (missionBehavior != null && ((MissionView)missionBehavior).IsReady())
		{
			missionBehavior.SetActiveCameraMode(MissionShipControlView.CameraModes.Back);
		}
	}
}
