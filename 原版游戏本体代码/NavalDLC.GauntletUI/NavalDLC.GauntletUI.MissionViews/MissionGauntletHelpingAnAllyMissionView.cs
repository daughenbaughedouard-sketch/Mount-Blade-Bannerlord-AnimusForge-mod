using NavalDLC.View.MissionViews;
using NavalDLC.View.MissionViews.Storyline;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(HelpingAnAllyMissionView))]
public class MissionGauntletHelpingAnAllyMissionView : HelpingAnAllyMissionView
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
