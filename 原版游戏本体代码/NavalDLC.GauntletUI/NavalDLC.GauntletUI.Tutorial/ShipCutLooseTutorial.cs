using System.Collections.Generic;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Storyline;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipCutLooseTutorial")]
public class ShipCutLooseTutorial : TutorialItemBase
{
	private int _lastControllerHashCode;

	private bool _hasCutLoose;

	public ShipCutLooseTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		Mission current = Mission.Current;
		PirateBattleMissionController pirateBattleMissionController = ((current != null) ? current.GetMissionBehavior<PirateBattleMissionController>() : null);
		Mission current2 = Mission.Current;
		NavalShipsLogic navalShipsLogic = ((current2 != null) ? current2.GetMissionBehavior<NavalShipsLogic>() : null);
		if (pirateBattleMissionController != null)
		{
			if (_lastControllerHashCode != ((object)pirateBattleMissionController).GetHashCode())
			{
				_hasCutLoose = false;
				_lastControllerHashCode = ((object)pirateBattleMissionController).GetHashCode();
			}
			if (navalShipsLogic != null)
			{
				MBList<MissionShip> val = new MBList<MissionShip>();
				navalShipsLogic.FillTeamShips((TeamSideEnum)0, val);
				if (pirateBattleMissionController.HasSelectedShip && ((List<MissionShip>)(object)val).Count == 2)
				{
					MissionShip missionShip = ((List<MissionShip>)(object)val)[0];
					MissionShip missionShip2 = ((List<MissionShip>)(object)val)[1];
					if (missionShip.IsDisconnectionBlocked())
					{
						missionShip.ResetDisconnectionBlock();
					}
					if (missionShip2.IsDisconnectionBlocked())
					{
						missionShip2.ResetDisconnectionBlock();
					}
					Agent main = Agent.Main;
					if (((main != null) ? main.GetComponent<AgentNavalComponent>().FormationShip : null) != null && !missionShip.GetIsThereActiveBridgeTo(missionShip2) && pirateBattleMissionController.HasSelectedShip)
					{
						_hasCutLoose = true;
					}
				}
			}
		}
		return _hasCutLoose;
	}

	public override bool IsConditionsMetForActivation()
	{
		if (Mission.Current == null || !Mission.Current.IsNavalBattle)
		{
			return false;
		}
		PirateBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<PirateBattleMissionController>();
		if (missionBehavior != null && missionBehavior.IsFirstShipCleared && missionBehavior.HasSelectedShip)
		{
			return !_hasCutLoose;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}
}
