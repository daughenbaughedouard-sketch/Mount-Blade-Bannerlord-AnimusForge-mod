using System.Collections.Generic;
using SandBox.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class PatrollingGuardBehavior : AgentBehavior
{
	private readonly MissionAgentHandler _missionAgentHandler;

	private UsableMachine _target;

	public PatrollingGuardBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
		_missionAgentHandler = base.Mission.GetMissionBehavior<MissionAgentHandler>();
	}

	public override void Tick(float dt, bool isSimulation)
	{
		if (_target == null)
		{
			UsableMachine val = ((base.Navigator.SpecialTargetTag == null || Extensions.IsEmpty<char>((IEnumerable<char>)base.Navigator.SpecialTargetTag)) ? _missionAgentHandler.FindUnusedPointWithTagForAgent(base.OwnerAgent, "npc_common") : _missionAgentHandler.FindUnusedPointWithTagForAgent(base.OwnerAgent, base.Navigator.SpecialTargetTag));
			if (val != null)
			{
				_target = val;
				base.Navigator.SetTarget(_target, isInitialTarget: false, (AIScriptedFrameFlags)0);
			}
		}
		else if (base.Navigator.TargetUsableMachine == null)
		{
			base.Navigator.SetTarget(_target, isInitialTarget: false, (AIScriptedFrameFlags)0);
		}
	}

	public override float GetAvailability(bool isSimulation)
	{
		if (_missionAgentHandler.GetAllUsablePointsWithTag(base.Navigator.SpecialTargetTag).Count <= 0)
		{
			return 0f;
		}
		return 1f;
	}

	protected override void OnDeactivate()
	{
		_target = null;
		base.Navigator.ClearTarget();
	}

	public override string GetDebugInfo()
	{
		return "Guard patrol";
	}
}
