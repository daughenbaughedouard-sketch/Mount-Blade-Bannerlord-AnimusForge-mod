using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Storyline;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipOarsmanTutorial")]
public class ShipOarsmanTutorial : TutorialItemBase
{
	public ShipOarsmanTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "OarsmenToggle";
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
				return missionShip.ShipOrder.OarsmenLevel == 2;
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
		Mission current = Mission.Current;
		NavalStorylineCaptivityMissionController obj = ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null);
		Mission current2 = Mission.Current;
		MissionShip missionShip = ((IEnumerable<MissionShip>)((current2 != null) ? current2.GetMissionBehavior<NavalShipsLogic>() : null)?.AllShips).FirstOrDefault();
		if (obj != null && missionShip != null)
		{
			return missionShip.IsPlayerControlled;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}
}
