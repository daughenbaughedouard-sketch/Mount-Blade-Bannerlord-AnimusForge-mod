using NavalDLC.Missions;
using NavalDLC.Storyline;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipBoardingAttemptBoardingTutorial")]
public class ShipBoardingAttemptBoardingTutorial : TutorialItemBase
{
	public ShipBoardingAttemptBoardingTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		Mission current = Mission.Current;
		if (((current != null) ? current.GetMissionBehavior<PirateBattleMissionController>() : null) != null)
		{
			Agent main = Agent.Main;
			return ((main != null) ? main.GetComponent<AgentNavalComponent>().FormationShip : null)?.GetIsAnyBridgeActive() ?? false;
		}
		return false;
	}

	public override bool IsConditionsMetForActivation()
	{
		if (Mission.Current == null || !Mission.Current.IsNavalBattle)
		{
			return false;
		}
		PirateBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<PirateBattleMissionController>();
		if (missionBehavior != null)
		{
			return !missionBehavior.IsFirstShipCleared;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}
}
