using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class IdleAgentBehavior : AgentBehavior
{
	public IdleAgentBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
	}

	public override float GetAvailability(bool isSimulation)
	{
		return 1f;
	}

	protected override void OnActivate()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		base.OwnerAgent.SetIsAIPaused(true);
		Agent ownerAgent = base.OwnerAgent;
		WorldPosition worldPosition = base.OwnerAgent.GetWorldPosition();
		ownerAgent.SetTargetPosition(((WorldPosition)(ref worldPosition)).AsVec2);
	}

	protected override void OnDeactivate()
	{
		base.OwnerAgent.SetIsAIPaused(false);
		base.OwnerAgent.ClearTargetFrame();
	}

	public override string GetDebugInfo()
	{
		return "Idle Behavior";
	}
}
