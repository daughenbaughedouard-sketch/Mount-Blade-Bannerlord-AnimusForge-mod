using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class BattleSpawnFrameBehavior : SpawnFrameBehaviorBase
{
	private List<GameEntity> _spawnPointsOfAttackers;

	private List<GameEntity> _spawnPointsOfDefenders;

	public override void Initialize()
	{
		((SpawnFrameBehaviorBase)this).Initialize();
		_spawnPointsOfAttackers = base.SpawnPoints.Where((GameEntity x) => x.HasTag("attacker")).ToList();
		_spawnPointsOfDefenders = base.SpawnPoints.Where((GameEntity x) => x.HasTag("defender")).ToList();
	}

	public override MatrixFrame GetSpawnFrame(Team team, bool hasMount, bool isInitialSpawn)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		List<GameEntity> list = ((team == Mission.Current.AttackerTeam) ? _spawnPointsOfAttackers : _spawnPointsOfDefenders).ToList();
		float num = float.MinValue;
		int index = -1;
		for (int i = 0; i < list.Count; i++)
		{
			float num2 = MBRandom.RandomFloat * 0.2f;
			Mission current = Mission.Current;
			Vec3 val = list[i].GlobalPosition;
			ProximityMapSearchStruct val2 = AgentProximityMap.BeginSearch(current, ((Vec3)(ref val)).AsVec2, 2f, false);
			while (((ProximityMapSearchStruct)(ref val2)).LastFoundAgent != null)
			{
				val = ((ProximityMapSearchStruct)(ref val2)).LastFoundAgent.Position;
				float num3 = ((Vec3)(ref val)).DistanceSquared(list[i].GlobalPosition);
				if (num3 < 4f)
				{
					float num4 = MathF.Sqrt(num3);
					num2 -= (2f - num4) * 5f;
				}
				AgentProximityMap.FindNext(Mission.Current, ref val2);
			}
			if (hasMount && list[i].HasTag("exclude_mounted"))
			{
				num2 -= 100f;
			}
			if (num2 > num)
			{
				num = num2;
				index = i;
			}
		}
		return list[index].GetGlobalFrame();
	}
}
