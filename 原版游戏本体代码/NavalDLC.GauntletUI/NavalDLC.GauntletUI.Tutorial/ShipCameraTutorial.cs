using NavalDLC.Missions.Objects;
using NavalDLC.Storyline;
using NavalDLC.View.MissionViews;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipCameraTutorial")]
public class ShipCameraTutorial : TutorialItemBase
{
	public ShipCameraTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "CameraToggle";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		Mission current = Mission.Current;
		MissionShipControlView missionShipControlView = ((current != null) ? current.GetMissionBehavior<MissionShipControlView>() : null);
		Mission current2 = Mission.Current;
		NavalStorylineCaptivityMissionController navalStorylineCaptivityMissionController = ((current2 != null) ? current2.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null);
		if (navalStorylineCaptivityMissionController != null && navalStorylineCaptivityMissionController.IsPlayerInShipControls())
		{
			if (missionShipControlView == null)
			{
				return false;
			}
			return missionShipControlView.ActiveCameraMode == MissionShipControlView.CameraModes.Back;
		}
		return false;
	}

	public override bool IsConditionsMetForActivation()
	{
		if (Mission.Current == null || !Mission.Current.IsNavalBattle)
		{
			return false;
		}
		Mission current = Mission.Current;
		NavalStorylineCaptivityMissionController navalStorylineCaptivityMissionController = ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null);
		MissionShip missionShip = navalStorylineCaptivityMissionController?.MissionShip;
		if (missionShip != null)
		{
			if (navalStorylineCaptivityMissionController.HasTalkedToGunnar)
			{
				return missionShip.ShipOrder.OarsmenLevel == 2;
			}
			return false;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}
}
