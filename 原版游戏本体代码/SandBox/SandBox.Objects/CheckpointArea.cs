using SandBox.Missions;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects;

public class CheckpointArea : VolumeBox
{
	public const string CheckpointSpawnPointTag = "sp_checkpoint";

	public int UniqueId;

	[EditorVisibleScriptComponentVariable(false)]
	private CheckpointMissionLogic _checkpointMissionLogic;

	[EditorVisibleScriptComponentVariable(false)]
	public GameEntity SpawnPoint { get; private set; }

	public override void AfterMissionStart()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		_checkpointMissionLogic = Mission.Current.GetMissionBehavior<CheckpointMissionLogic>();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child;
			if (((WeakGameEntity)(ref current)).HasTag("sp_checkpoint"))
			{
				SpawnPoint = GameEntity.CreateFromWeakEntity(current);
				break;
			}
		}
	}

	protected override void OnTick(float dt)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (_checkpointMissionLogic != null)
		{
			Agent main = Agent.Main;
			if (main != null && main.IsActive() && ((VolumeBox)this).IsPointIn(Agent.Main.Position))
			{
				_checkpointMissionLogic.OnCheckpointUsed(UniqueId);
			}
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}
}
