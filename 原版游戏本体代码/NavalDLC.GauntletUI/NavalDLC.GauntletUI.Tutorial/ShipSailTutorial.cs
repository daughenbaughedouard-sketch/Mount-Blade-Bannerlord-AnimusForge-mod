using NavalDLC.Missions.Objects;
using NavalDLC.Storyline;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipSailTutorial")]
public class ShipSailTutorial : TutorialItemBase
{
	public ShipSailTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "SailToggle";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		Mission current = Mission.Current;
		MissionShip missionShip = ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null)?.MissionShip;
		if (missionShip != null)
		{
			if (missionShip.IsPlayerControlled)
			{
				return missionShip.SailTargetSetting > 0.5f;
			}
			return false;
		}
		return false;
	}

	public override bool IsConditionsMetForActivation()
	{
		Mission current = Mission.Current;
		return ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null)?.IsFirstHighlightCleared() ?? false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}
}
