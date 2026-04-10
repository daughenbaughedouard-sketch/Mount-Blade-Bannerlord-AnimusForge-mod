using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class TeamDeathmatchSpawnFrameBehavior : SpawnFrameBehaviorBase
{
	public override MatrixFrame GetSpawnFrame(Team team, bool hasMount, bool isInitialSpawn)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return ((SpawnFrameBehaviorBase)this).GetSpawnFrameFromSpawnPoints((IList<GameEntity>)base.SpawnPoints.ToList(), team, hasMount);
	}
}
