using SandBox.Missions.MissionLogics;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class StandGuardBehavior : AgentBehavior
{
	private enum GuardState
	{
		StandIdle,
		StandAttention,
		StandCautious,
		GotToStandPoint
	}

	private UsableMachine _oldStandPoint;

	private UsableMachine _standPoint;

	private readonly MissionAgentHandler _missionAgentHandler;

	public StandGuardBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
		_missionAgentHandler = base.Mission.GetMissionBehavior<MissionAgentHandler>();
	}

	public override void Tick(float dt, bool isSimulation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)base.OwnerAgent.CurrentWatchState == 0)
		{
			if (_standPoint == null || isSimulation)
			{
				UsableMachine val = _oldStandPoint ?? _missionAgentHandler.FindUnusedPointWithTagForAgent(base.OwnerAgent, base.Navigator.SpecialTargetTag);
				if (val != null)
				{
					_oldStandPoint = null;
					_standPoint = val;
					base.Navigator.SetTarget(_standPoint, isInitialTarget: false, (AIScriptedFrameFlags)0);
				}
			}
		}
		else if (_standPoint != null)
		{
			_oldStandPoint = _standPoint;
			base.Navigator.SetTarget(null, isInitialTarget: false, (AIScriptedFrameFlags)0);
			_standPoint = null;
		}
	}

	protected override void OnDeactivate()
	{
		base.Navigator.ClearTarget();
		_standPoint = null;
	}

	public override float GetAvailability(bool isSimulation)
	{
		return 1f;
	}

	public override string GetDebugInfo()
	{
		return "Guard stand";
	}
}
