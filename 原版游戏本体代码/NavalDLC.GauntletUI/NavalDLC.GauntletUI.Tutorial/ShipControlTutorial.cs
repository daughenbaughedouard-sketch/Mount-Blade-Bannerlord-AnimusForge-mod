using NavalDLC.Missions.Objects;
using NavalDLC.Storyline;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipControlTutorial")]
public class ShipControlTutorial : TutorialItemBase
{
	public ShipControlTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)0;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		Mission current = Mission.Current;
		NavalStorylineCaptivityMissionController navalStorylineCaptivityMissionController = ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null);
		if (navalStorylineCaptivityMissionController != null)
		{
			MissionShip missionShip = navalStorylineCaptivityMissionController.MissionShip;
			if (missionShip != null)
			{
				return missionShip.IsPlayerControlled;
			}
		}
		return false;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		if (Mission.Current == null || !Mission.Current.IsNavalBattle)
		{
			return false;
		}
		NavalStorylineCaptivityMissionController missionBehavior = Mission.Current.GetMissionBehavior<NavalStorylineCaptivityMissionController>();
		if (missionBehavior != null && missionBehavior.HasTalkedToGunnar)
		{
			return (int)Mission.Current.Mode != 1;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}
}
