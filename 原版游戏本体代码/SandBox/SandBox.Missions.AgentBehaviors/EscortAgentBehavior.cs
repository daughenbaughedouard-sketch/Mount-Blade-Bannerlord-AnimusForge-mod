using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class EscortAgentBehavior : AgentBehavior
{
	public delegate bool OnTargetReachedDelegate(Agent agent, ref Agent escortedAgent, ref Agent targetAgent, ref UsableMachine targetMachine, ref Vec3? targetPosition);

	private enum State
	{
		NotEscorting,
		ReturnToEscortedAgent,
		Wait,
		Escorting
	}

	private const float StartWaitingDistanceSquared = 25f;

	private const float ReturnToEscortedAgentDistanceSquared = 100f;

	private const float EscortFinishedDistanceSquared = 16f;

	private const float TargetProximityThreshold = 3f;

	private const float MountedMoveProximityThreshold = 2.2f;

	private const float OnFootMoveProximityThreshold = 1.2f;

	private State _state;

	private Agent _escortedAgent;

	private Agent _targetAgent;

	private UsableMachine _targetMachine;

	private Vec3? _targetPosition;

	private bool _myLastStateWasRunning;

	private float _initialMaxSpeedLimit;

	private OnTargetReachedDelegate _onTargetReached;

	private bool _escortFinished;

	public Agent EscortedAgent => _escortedAgent;

	public Agent TargetAgent => _targetAgent;

	public EscortAgentBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
		_targetAgent = null;
		_escortedAgent = null;
		_myLastStateWasRunning = false;
		_initialMaxSpeedLimit = 1f;
	}

	public void Initialize(Agent escortedAgent, Agent targetAgent, OnTargetReachedDelegate onTargetReached = null)
	{
		_escortedAgent = escortedAgent;
		_targetAgent = targetAgent;
		_targetMachine = null;
		_targetPosition = null;
		_onTargetReached = onTargetReached;
		_escortFinished = false;
		_initialMaxSpeedLimit = base.OwnerAgent.GetMaximumSpeedLimit();
		_state = State.Escorting;
	}

	public void Initialize(Agent escortedAgent, UsableMachine targetMachine, OnTargetReachedDelegate onTargetReached = null)
	{
		_escortedAgent = escortedAgent;
		_targetAgent = null;
		_targetMachine = targetMachine;
		_targetPosition = null;
		_onTargetReached = onTargetReached;
		_escortFinished = false;
		_initialMaxSpeedLimit = base.OwnerAgent.GetMaximumSpeedLimit();
		_state = State.Escorting;
	}

	public void Initialize(Agent escortedAgent, Vec3? targetPosition, OnTargetReachedDelegate onTargetReached = null)
	{
		_escortedAgent = escortedAgent;
		_targetAgent = null;
		_targetMachine = null;
		_targetPosition = targetPosition;
		_onTargetReached = onTargetReached;
		_escortFinished = false;
		_initialMaxSpeedLimit = base.OwnerAgent.GetMaximumSpeedLimit();
		_state = State.Escorting;
	}

	public override void Tick(float dt, bool isSimulation)
	{
		if (_escortedAgent == null || !_escortedAgent.IsActive() || _targetAgent == null || !_targetAgent.IsActive())
		{
			_state = State.NotEscorting;
		}
		if (_escortedAgent != null && _state != State.NotEscorting)
		{
			ControlMovement();
		}
	}

	public bool IsEscortFinished()
	{
		return _escortFinished;
	}

	private void ControlMovement()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		Mission mission = base.Mission;
		Team team = _escortedAgent.Team;
		Vec3 val = _escortedAgent.Position;
		int nearbyEnemyAgentCount = mission.GetNearbyEnemyAgentCount(team, ((Vec3)(ref val)).AsVec2, 5f);
		if (_state != State.NotEscorting && nearbyEnemyAgentCount > 0)
		{
			_state = State.NotEscorting;
			base.OwnerAgent.ResetLookAgent();
			base.Navigator.ClearTarget();
			base.OwnerAgent.DisableScriptedMovement();
			base.OwnerAgent.SetMaximumSpeedLimit(_initialMaxSpeedLimit, false);
			Debug.Print("[Escort agent behavior] Escorted agent got into a fight... Disable!", 0, (DebugColor)12, 17592186044416uL);
			return;
		}
		float rangeThreshold = (base.OwnerAgent.HasMount ? 2.2f : 1.2f);
		val = base.OwnerAgent.Position;
		float num = ((Vec3)(ref val)).DistanceSquared(_escortedAgent.Position);
		float num2;
		WorldPosition targetPosition;
		MatrixFrame frame;
		Vec2 asVec;
		float targetRotation;
		if (_targetAgent != null)
		{
			val = base.OwnerAgent.Position;
			num2 = ((Vec3)(ref val)).DistanceSquared(_targetAgent.Position);
			targetPosition = _targetAgent.GetWorldPosition();
			frame = _targetAgent.Frame;
			asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
			targetRotation = ((Vec2)(ref asVec)).RotationInRadians;
		}
		else if (_targetMachine != null)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_targetMachine).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			val = base.OwnerAgent.Position;
			num2 = ((Vec3)(ref val)).DistanceSquared(globalFrame.origin);
			targetPosition = ModuleExtensions.ToWorldPosition(globalFrame.origin);
			asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			targetRotation = ((Vec2)(ref asVec)).RotationInRadians;
		}
		else if (_targetPosition.HasValue)
		{
			val = base.OwnerAgent.Position;
			num2 = ((Vec3)(ref val)).DistanceSquared(_targetPosition.Value);
			targetPosition = ModuleExtensions.ToWorldPosition(_targetPosition.Value);
			val = _targetPosition.Value - base.OwnerAgent.Position;
			asVec = ((Vec3)(ref val)).AsVec2;
			targetRotation = ((Vec2)(ref asVec)).RotationInRadians;
		}
		else
		{
			Debug.FailedAssert("At least one target must be specified for the escort behavior.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\AgentBehaviors\\EscortAgentBehavior.cs", "ControlMovement", 160);
			num2 = 0f;
			targetPosition = base.OwnerAgent.GetWorldPosition();
			targetRotation = 0f;
		}
		if (_escortFinished)
		{
			bool flag = false;
			base.OwnerAgent.SetMaximumSpeedLimit(_initialMaxSpeedLimit, false);
			if (_onTargetReached != null)
			{
				flag = _onTargetReached(base.OwnerAgent, ref _escortedAgent, ref _targetAgent, ref _targetMachine, ref _targetPosition);
			}
			if (flag && _escortedAgent != null && (_targetAgent != null || _targetMachine != null || _targetPosition.HasValue))
			{
				_state = State.Escorting;
			}
			else
			{
				_state = State.NotEscorting;
			}
		}
		switch (_state)
		{
		case State.Wait:
		{
			if (num < 25f)
			{
				_state = State.Escorting;
				Debug.Print("[Escort agent behavior] Escorting!", 0, (DebugColor)12, 17592186044416uL);
				break;
			}
			if (num > 100f)
			{
				_state = State.ReturnToEscortedAgent;
				Debug.Print("[Escort agent behavior] Escorted agent is too far away! Return to escorted agent!", 0, (DebugColor)12, 17592186044416uL);
				break;
			}
			WorldPosition worldPosition2 = base.OwnerAgent.GetWorldPosition();
			frame = base.OwnerAgent.Frame;
			asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
			SetMovePos(worldPosition2, ((Vec2)(ref asVec)).RotationInRadians, 0f);
			break;
		}
		case State.Escorting:
			if (num >= 25f)
			{
				_state = State.Wait;
				Debug.Print("[Escort agent behavior] Stop walking! Wait", 0, (DebugColor)12, 17592186044416uL);
			}
			else
			{
				SetMovePos(targetPosition, targetRotation, 3f);
			}
			break;
		case State.ReturnToEscortedAgent:
		{
			if (num < 25f)
			{
				_state = State.Wait;
				break;
			}
			WorldPosition worldPosition = _escortedAgent.GetWorldPosition();
			frame = _escortedAgent.Frame;
			asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
			SetMovePos(worldPosition, ((Vec2)(ref asVec)).RotationInRadians, rangeThreshold);
			break;
		}
		}
		if (_state == State.Escorting && num2 < 16f && num < 16f && !Campaign.Current.ConversationManager.IsConversationInProgress)
		{
			_escortFinished = true;
		}
	}

	private void SetMovePos(WorldPosition targetPosition, float targetRotation, float rangeThreshold)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		AIScriptedFrameFlags val = (AIScriptedFrameFlags)2;
		if (base.Navigator.CharacterHasVisiblePrefabs)
		{
			_myLastStateWasRunning = false;
		}
		else
		{
			Vec3 val2 = base.OwnerAgent.Position;
			Vec2 asVec = ((Vec3)(ref val2)).AsVec2;
			float num = ((Vec2)(ref asVec)).Distance(((WorldPosition)(ref targetPosition)).AsVec2);
			val2 = _escortedAgent.Velocity;
			asVec = ((Vec3)(ref val2)).AsVec2;
			float length = ((Vec2)(ref asVec)).Length;
			if (num - rangeThreshold <= 0.5f * (_myLastStateWasRunning ? 1f : 1.2f) && length <= base.OwnerAgent.Monster.WalkingSpeedLimit * (_myLastStateWasRunning ? 1f : 1.2f))
			{
				_myLastStateWasRunning = false;
			}
			else
			{
				base.OwnerAgent.SetMaximumSpeedLimit(num - rangeThreshold + length, false);
				_myLastStateWasRunning = true;
			}
		}
		if (!_myLastStateWasRunning)
		{
			val = (AIScriptedFrameFlags)(val | 0x10);
		}
		base.Navigator.SetTargetFrame(targetPosition, targetRotation, rangeThreshold, -10f, val);
	}

	public override float GetAvailability(bool isSimulation)
	{
		return (_state != State.NotEscorting) ? 1 : 0;
	}

	protected override void OnDeactivate()
	{
		_escortedAgent = null;
		_targetAgent = null;
		_targetMachine = null;
		_targetPosition = null;
		_onTargetReached = null;
		_state = State.NotEscorting;
		base.OwnerAgent.DisableScriptedMovement();
		base.OwnerAgent.ResetLookAgent();
		base.Navigator.ClearTarget();
	}

	public override string GetDebugInfo()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		return "Escort " + _escortedAgent.Name + " (id:" + _escortedAgent.Index + ")" + ((_targetAgent != null) ? (" to " + _targetAgent.Name + " (id:" + _targetAgent.Index + ")") : ((_targetMachine != null) ? string.Concat(" to ", _targetMachine, "(id:", ((MissionObject)_targetMachine).Id, ")") : (_targetPosition.HasValue ? (" to position: " + _targetPosition.Value) : " to NO TARGET")));
	}

	public static void AddEscortAgentBehavior(Agent ownerAgent, Agent targetAgent, OnTargetReachedDelegate onTargetReached)
	{
		InterruptingBehaviorGroup interruptingBehaviorGroup = ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator?.GetBehaviorGroup<InterruptingBehaviorGroup>();
		if (interruptingBehaviorGroup != null)
		{
			bool num = interruptingBehaviorGroup.GetBehavior<EscortAgentBehavior>() == null;
			EscortAgentBehavior escortAgentBehavior = interruptingBehaviorGroup.GetBehavior<EscortAgentBehavior>() ?? interruptingBehaviorGroup.AddBehavior<EscortAgentBehavior>();
			if (num)
			{
				interruptingBehaviorGroup.SetScriptedBehavior<EscortAgentBehavior>();
			}
			escortAgentBehavior.Initialize(Agent.Main, targetAgent, onTargetReached);
		}
	}

	public static void RemoveEscortBehaviorOfAgent(Agent ownerAgent)
	{
		InterruptingBehaviorGroup interruptingBehaviorGroup = ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator?.GetBehaviorGroup<InterruptingBehaviorGroup>();
		if (interruptingBehaviorGroup != null && interruptingBehaviorGroup.GetBehavior<EscortAgentBehavior>() != null)
		{
			interruptingBehaviorGroup.RemoveBehavior<EscortAgentBehavior>();
		}
	}

	public static bool CheckIfAgentIsEscortedBy(Agent ownerAgent, Agent escortedAgent)
	{
		EscortAgentBehavior escortAgentBehavior = (ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator?.GetBehaviorGroup<InterruptingBehaviorGroup>())?.GetBehavior<EscortAgentBehavior>();
		if (escortAgentBehavior != null)
		{
			return escortAgentBehavior.EscortedAgent == escortedAgent;
		}
		return false;
	}
}
