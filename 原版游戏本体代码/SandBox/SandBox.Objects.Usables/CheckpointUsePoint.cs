using System.Collections.Generic;
using SandBox.Missions;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.Usables;

public class CheckpointUsePoint : UsableMachine
{
	public const string CheckpointSpawnPointTag = "sp_checkpoint";

	public int UniqueId;

	[EditorVisibleScriptComponentVariable(false)]
	private CheckpointMissionLogic _checkpointMissionLogic;

	[EditorVisibleScriptComponentVariable(false)]
	public GameEntity SpawnPoint { get; private set; }

	protected override void OnInit()
	{
		((UsableMachine)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick((TickRequirement)2);
	}

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
		((UsableMachine)this).OnTick(dt);
		if (_checkpointMissionLogic == null)
		{
			return;
		}
		Agent main = Agent.Main;
		if (main == null || !main.IsActive())
		{
			return;
		}
		for (int i = 0; i < ((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints).Count; i++)
		{
			if (((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)[i]).HasUser)
			{
				_checkpointMissionLogic.OnCheckpointUsed(UniqueId);
			}
		}
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=G2IaEr2Z}Use", (Dictionary<string, object>)null);
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=eO7p1Q3C}Checkpoint", (Dictionary<string, object>)null);
	}
}
