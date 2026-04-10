using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Storyline;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipBoardingApproachTutorial")]
public class ShipBoardingApproachTutorial : TutorialItemBase
{
	public ShipBoardingApproachTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		Mission current = Mission.Current;
		if (((current != null) ? current.GetMissionBehavior<PirateBattleMissionController>() : null) != null)
		{
			Mission current2 = Mission.Current;
			NavalShipsLogic obj = ((current2 != null) ? current2.GetMissionBehavior<NavalShipsLogic>() : null);
			Agent main = Agent.Main;
			MissionShip missionShip = ((main != null) ? main.GetComponent<AgentNavalComponent>().FormationShip : null);
			MissionShip missionShip2 = ((IEnumerable<MissionShip>)obj?.AllShips).FirstOrDefault((MissionShip x) => !x.IsPlayerShip);
			if (missionShip2 != null && missionShip != null)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)missionShip).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				gameEntity = ((ScriptComponentBehavior)missionShip2).GameEntity;
				if (((Vec3)(ref globalPosition)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GlobalPosition) <= 2500f)
				{
					return true;
				}
			}
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
