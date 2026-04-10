using NavalDLC.Missions.AI.UsableMachineAIs;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects.Siege;

namespace NavalDLC.Missions.Objects;

public class ShipBallistaSpawner : BallistaSpawner
{
	protected override void OnPreInit()
	{
		((SpawnerBase)this)._spawnerMissionHelper = (SpawnerEntityMissionHelper)(object)new ShipSpawnerEntityMissionHelper((SpawnerBase)(object)this);
	}

	public override void AssignParameters(SpawnerEntityMissionHelper _spawnerMissionHelper)
	{
		((BallistaSpawner)this).AssignParameters(_spawnerMissionHelper);
		if (Mission.Current != null)
		{
			Ballista firstScriptOfType = _spawnerMissionHelper.SpawnedEntity.GetFirstScriptOfType<Ballista>();
			((UsableMachine)firstScriptOfType).SetAI((UsableMachineAIBase)(object)new ShipBallistaAI(firstScriptOfType));
		}
	}
}
