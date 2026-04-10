using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace SandBox.AI;

public class UsablePlaceAI : UsableMachineAIBase
{
	public UsablePlaceAI(UsableMachine usableMachine)
		: base(usableMachine)
	{
	}

	protected override AIScriptedFrameFlags GetScriptedFrameFlags(Agent agent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)base.UsableMachine).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).HasTag("quest_wanderer_target"))
		{
			return (AIScriptedFrameFlags)0;
		}
		return (AIScriptedFrameFlags)16;
	}
}
