using System.Collections.Generic;
using System.Linq;
using SandBox.Objects;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions;

public class CoverAnimalAgentComponent : AgentComponent, IFocusable
{
	private enum NavigationState
	{
		WaitingToStart,
		NoTarget,
		GoToTarget,
		AtFinalPosition
	}

	private PatrolPoint[] _patrolPoints;

	private int _currentPatrolAreaIndex;

	private Timer _waitTimer;

	private NavigationState _agentState;

	private WorldPosition _targetPosition;

	private Vec2 _targetDirection;

	private bool _targetReached;

	private float _rangeThreshold;

	public bool IsMovementStarted => _agentState != NavigationState.WaitingToStart;

	public bool IsAtFinalPoint => _agentState == NavigationState.AtFinalPosition;

	public FocusableObjectType FocusableObjectType => (FocusableObjectType)0;

	public virtual bool IsFocusable => true;

	public CoverAnimalAgentComponent(Agent agent)
		: base(agent)
	{
		_agentState = NavigationState.WaitingToStart;
		base.Agent.SetMaximumSpeedLimit(1f, false);
	}

	public void SetDynamicPatrolArea(GameEntity parentPatrolPoint)
	{
		_patrolPoints = new PatrolPoint[parentPatrolPoint.ChildCount];
		bool flag = false;
		PatrolPoint[] array = new PatrolPoint[parentPatrolPoint.ChildCount];
		for (int i = 0; i < parentPatrolPoint.ChildCount; i++)
		{
			array[i] = parentPatrolPoint.GetChild(i).GetChild(0).GetFirstScriptOfType<PatrolPoint>();
			if (!flag)
			{
				flag = array[i].IsInfiniteWaitPoint;
			}
		}
		_patrolPoints = array.OrderBy((PatrolPoint x) => x.Index).ToArray();
	}

	public void StartMovement()
	{
		if (!IsMovementStarted)
		{
			_agentState = NavigationState.NoTarget;
			base.Agent.SetMaximumSpeedLimit(1f, false);
		}
	}

	public override void OnTick(float dt)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Agent.Mission.AllowAiTicking || !base.Agent.IsAIControlled || _agentState == NavigationState.WaitingToStart)
		{
			return;
		}
		if (_waitTimer == null && IsTargetReached() && _agentState != NavigationState.NoTarget)
		{
			PatrolPoint patrolPoint = _patrolPoints[_currentPatrolAreaIndex];
			float num = (float)patrolPoint.WaitDuration + MBRandom.RandomFloatRanged((float)(-patrolPoint.WaitDeviation), (float)patrolPoint.WaitDeviation);
			_waitTimer = new Timer(Mission.Current.CurrentTime, num, true);
		}
		if (_agentState != NavigationState.AtFinalPosition)
		{
			if (!((WorldPosition)(ref _targetPosition)).IsValid)
			{
				MoveAnimalToNextPatrolPoint();
			}
			Timer waitTimer = _waitTimer;
			if (waitTimer != null && waitTimer.Check(Mission.Current.CurrentTime))
			{
				_waitTimer = null;
				base.Agent.ClearTargetFrame();
				_targetPosition = WorldPosition.Invalid;
				_agentState = NavigationState.NoTarget;
			}
		}
	}

	private void DebugTick()
	{
		int num = _currentPatrolAreaIndex;
		if (num == -1)
		{
			num = 0;
		}
		if (num + 1 >= _patrolPoints.Length)
		{
			num = -1;
		}
		for (int i = 0; i < _patrolPoints.Length; i++)
		{
		}
		_ = _waitTimer;
	}

	public bool IsTargetReached()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (((Vec2)(ref _targetDirection)).IsValid && ((WorldPosition)(ref _targetPosition)).IsValid)
		{
			Vec3 val = base.Agent.Position - ((WorldPosition)(ref _targetPosition)).GetGroundVec3();
			_targetReached = ((Vec3)(ref val)).LengthSquared < _rangeThreshold * _rangeThreshold;
		}
		return _targetReached;
	}

	public void SetTargetFrame(WorldPosition position, float rotation, float rangeThreshold = 1f, AIScriptedFrameFlags flags = (AIScriptedFrameFlags)0)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (_agentState != NavigationState.NoTarget)
		{
			base.Agent.ClearTargetFrame();
			_targetPosition = WorldPosition.Invalid;
			_agentState = NavigationState.NoTarget;
		}
		_targetPosition = position;
		_targetDirection = Vec2.FromRotation(rotation);
		_rangeThreshold = rangeThreshold;
		if (IsTargetReached())
		{
			_targetPosition = WorldPosition.Invalid;
			_agentState = NavigationState.NoTarget;
		}
		else
		{
			base.Agent.SetScriptedPosition(ref position, false, flags);
			_agentState = NavigationState.GoToTarget;
		}
	}

	private void MoveAnimalToNextPatrolPoint()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		_waitTimer = null;
		if (_patrolPoints[_currentPatrolAreaIndex].IsInfiniteWaitPoint)
		{
			_agentState = NavigationState.AtFinalPosition;
			return;
		}
		_currentPatrolAreaIndex++;
		if (_currentPatrolAreaIndex >= _patrolPoints.Length)
		{
			_currentPatrolAreaIndex = 0;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_patrolPoints[_currentPatrolAreaIndex]).GameEntity;
		WorldPosition val = default(WorldPosition);
		((WorldPosition)(ref val))._002Ector(((WeakGameEntity)(ref gameEntity)).Scene, ((WeakGameEntity)(ref gameEntity)).GlobalPosition);
		WorldPosition position = val;
		MatrixFrame frame = ((WeakGameEntity)(ref gameEntity)).GetFrame();
		SetTargetFrame(position, ((Vec3)(ref frame.rotation.f)).RotationX, 1f, (AIScriptedFrameFlags)16);
		_agentState = NavigationState.GoToTarget;
	}

	public void OnFocusGain(Agent userAgent)
	{
	}

	public void OnFocusLose(Agent userAgent)
	{
	}

	public TextObject GetInfoTextForBeingNotInteractable(Agent userAgent)
	{
		return null;
	}

	public TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		TextObject obj = GameTexts.FindText("str_key_action", (string)null);
		obj.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		obj.SetTextVariable("ACTION", new TextObject("{=F7JGCr9s}Move", (Dictionary<string, object>)null));
		return obj;
	}
}
