using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace NavalDLC.ComponentInterfaces;

public class NavalCustomBattleInitializationModel : BattleInitializationModel
{
	public override List<FormationClass> GetAllAvailableTroopTypes()
	{
		return ((MBGameModel<BattleInitializationModel>)this).BaseModel.GetAllAvailableTroopTypes();
	}

	protected override bool CanPlayerSideDeployWithOrderOfBattleAux()
	{
		return Mission.Current.GetMissionBehavior<ShipAgentSpawnLogic>().DeployablePlayerShipCount > 1;
	}
}
