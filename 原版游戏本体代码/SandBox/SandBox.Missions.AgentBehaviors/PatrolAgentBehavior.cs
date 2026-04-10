using System.Linq;
using SandBox.Objects;
using SandBox.Objects.Usables;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class PatrolAgentBehavior : AgentBehavior
{
	private const float DefaultPatrollingSpeed = 1.05f;

	private PatrolPoint[] _patrolPoints;

	private int _currentPatrolIndex;

	private Timer _waitTimer;

	private bool _infiniteWaitPointReached;

	private int NextPatrolIndex
	{
		get
		{
			int num = _currentPatrolIndex + 1;
			if (num >= _patrolPoints.Length)
			{
				num = 0;
			}
			return num;
		}
	}

	public PatrolAgentBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
	}

	public void SetDynamicPatrolArea(GameEntity parentPatrolPoint)
	{
		_patrolPoints = new PatrolPoint[parentPatrolPoint.ChildCount];
		PatrolPoint[] array = new PatrolPoint[parentPatrolPoint.ChildCount];
		for (int i = 0; i < parentPatrolPoint.ChildCount; i++)
		{
			array[i] = parentPatrolPoint.GetChild(i).GetChild(0).GetFirstScriptOfType<PatrolPoint>();
		}
		_patrolPoints = array.OrderBy((PatrolPoint x) => x.Index).ToArray();
	}

	protected override void OnActivate()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		base.OwnerAgent.SetMaximumSpeedLimit(1.05f, false);
		_infiniteWaitPointReached = false;
		PatrolPoint patrolPoint = null;
		float num = float.MaxValue;
		PatrolPoint[] patrolPoints = _patrolPoints;
		foreach (PatrolPoint patrolPoint2 in patrolPoints)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)patrolPoint2).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			float num2 = ((Vec3)(ref globalPosition)).DistanceSquared(base.OwnerAgent.Position);
			if (num2 < num)
			{
				num = num2;
				patrolPoint = patrolPoint2;
			}
		}
		_currentPatrolIndex = Extensions.IndexOf<PatrolPoint>(_patrolPoints, patrolPoint);
		MoveAgentToThePoint(_currentPatrolIndex, correctRotation: true, isSimulation: false);
	}

	protected override void OnDeactivate()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_waitTimer = null;
		if (base.OwnerAgent.CurrentlyUsedGameObject != null)
		{
			base.OwnerAgent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
		}
		base.Navigator.SetTarget(null, isInitialTarget: false, (AIScriptedFrameFlags)0);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_patrolPoints[_currentPatrolIndex]).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<PatrolPoint>().PatrollingSpeed != -1f || base.OwnerAgent.GetMaximumSpeedLimit().Equals(1.05f))
		{
			base.OwnerAgent.SetMaximumSpeedLimit(-1f, false);
		}
	}

	public override void Tick(float dt, bool isSimulation)
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		if (!_infiniteWaitPointReached && base.OwnerAgent.CurrentlyUsedGameObject != null)
		{
			if (_waitTimer == null)
			{
				if (!(base.OwnerAgent.CurrentlyUsedGameObject is PatrolPoint patrolPoint))
				{
					return;
				}
				if (patrolPoint.IsInfiniteWaitPoint)
				{
					_infiniteWaitPointReached = true;
					return;
				}
				float num = (float)patrolPoint.WaitDuration + MBRandom.RandomFloatRanged((float)(-patrolPoint.WaitDeviation), (float)patrolPoint.WaitDeviation);
				if (num == 0f)
				{
					MoveAgentToNextPatrolPoint(isSimulation);
				}
				else
				{
					_waitTimer = new Timer(base.Mission.CurrentTime, num, true);
				}
			}
			else if (_waitTimer.Check(base.Mission.CurrentTime))
			{
				MoveAgentToNextPatrolPoint(isSimulation);
			}
			return;
		}
		if (base.Navigator.IsTargetReached())
		{
			base.Navigator.ClearTarget();
		}
		if (base.Navigator.TargetUsableMachine == null)
		{
			WorldPosition targetPosition = base.Navigator.TargetPosition;
			if (!((WorldPosition)(ref targetPosition)).IsValid)
			{
				MoveAgentToNextPatrolPoint(isSimulation);
			}
		}
	}

	public override float GetAvailability(bool isSimulation)
	{
		if (!base.OwnerAgent.IsAlarmed() && !base.OwnerAgent.IsPatrollingCautious())
		{
			return 0.5f;
		}
		return 0f;
	}

	private void MoveAgentToNextPatrolPoint(bool isSimulation)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		_waitTimer = null;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_patrolPoints[_currentPatrolIndex]).GameEntity;
		PatrolPoint firstScriptOfType = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<PatrolPoint>();
		base.OwnerAgent.SetMaximumSpeedLimit((firstScriptOfType.PatrollingSpeed == -1f) ? 1.05f : firstScriptOfType.PatrollingSpeed, false);
		MoveAgentToThePoint(NextPatrolIndex, correctRotation: false, isSimulation);
		_currentPatrolIndex = NextPatrolIndex;
	}

	private void MoveAgentToThePoint(int pointIndex, bool correctRotation, bool isSimulation)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_patrolPoints[pointIndex]).GameEntity;
		PatrolPoint firstScriptOfType = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<PatrolPoint>();
		if (firstScriptOfType.WaitDuration == 0 && firstScriptOfType.WaitDeviation == 0)
		{
			WorldPosition val = default(WorldPosition);
			((WorldPosition)(ref val))._002Ector(((WeakGameEntity)(ref gameEntity)).Scene, ((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			AgentNavigator navigator = base.Navigator;
			WorldPosition position = val;
			MatrixFrame frame = ((WeakGameEntity)(ref gameEntity)).GetFrame();
			navigator.SetTargetFrame(position, ((Vec3)(ref frame.rotation.f)).RotationX, correctRotation ? 1f : (-1f), correctRotation ? 0.8f : (-10f), (AIScriptedFrameFlags)0);
		}
		else
		{
			AgentNavigator navigator2 = base.Navigator;
			WeakGameEntity parent = ((WeakGameEntity)(ref gameEntity)).Parent;
			navigator2.SetTarget((UsableMachine)(object)((WeakGameEntity)(ref parent)).GetFirstScriptOfType<UsablePlace>(), isSimulation, (AIScriptedFrameFlags)24);
		}
	}

	public override string GetDebugInfo()
	{
		return "Patrol Agent Behavior";
	}
}
