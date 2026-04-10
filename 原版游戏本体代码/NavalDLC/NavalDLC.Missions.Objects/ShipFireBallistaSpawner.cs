using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects.Siege;

namespace NavalDLC.Missions.Objects;

public class ShipFireBallistaSpawner : ShipBallistaSpawner
{
	protected override void OnPreInit()
	{
		((SpawnerBase)this)._spawnerMissionHelper = (SpawnerEntityMissionHelper)(object)new ShipSpawnerEntityMissionHelper((SpawnerBase)(object)this, fireVersion: true);
	}
}
