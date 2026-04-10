using NavalDLC.Missions.Objects;
using NavalDLC.Storyline;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipCloseSailTutorial")]
public class ShipCloseSailTutorial : TutorialItemBase
{
	public ShipCloseSailTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "SailToggle";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Mission current = Mission.Current;
		NavalStorylineCaptivityMissionController navalStorylineCaptivityMissionController = ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null);
		MissionShip missionShip = navalStorylineCaptivityMissionController?.MissionShip;
		if (missionShip != null)
		{
			if (navalStorylineCaptivityMissionController.IsReadyToCloseSails() && missionShip.IsPlayerControlled && missionShip.SailTargetSetting < 0.5f)
			{
				Vec3 linearVelocity = missionShip.Physics.LinearVelocity;
				return ((Vec3)(ref linearVelocity)).Length <= navalStorylineCaptivityMissionController.GetStoppedShipSpeedThreshold();
			}
			return false;
		}
		return false;
	}

	public override bool IsConditionsMetForActivation()
	{
		if (Mission.Current == null || !Mission.Current.IsNavalBattle)
		{
			return false;
		}
		return Mission.Current.GetMissionBehavior<NavalStorylineCaptivityMissionController>()?.IsReadyToCloseSails() ?? false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}
}
