using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Objects.AreaMarkers;

public class StealthAreaMarker : AreaMarker
{
	private const string ReinforcementAllyGroupSpawnPointTag = "reinforcement_ally_group_spawn_point_tag";

	private const string WaitPointTag = "wait_point_tag";

	public string ReinforcementAllyGroupId;

	public GameEntity ReinforcementAllyGroupSpawnPoint { get; private set; }

	public GameEntity WaitPoint { get; private set; }

	public override void AfterMissionStart()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child;
			if (((WeakGameEntity)(ref current)).HasTag("reinforcement_ally_group_spawn_point_tag"))
			{
				ReinforcementAllyGroupSpawnPoint = GameEntity.CreateFromWeakEntity(current);
			}
			if (((WeakGameEntity)(ref current)).HasTag("wait_point_tag"))
			{
				WaitPoint = GameEntity.CreateFromWeakEntity(current);
			}
		}
	}
}
