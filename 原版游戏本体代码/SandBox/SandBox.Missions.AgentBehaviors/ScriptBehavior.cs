using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class ScriptBehavior : AgentBehavior
{
	public delegate bool SelectTargetDelegate(Agent agent, ref Agent targetAgent, ref UsableMachine targetUsableMachine, ref WorldFrame targetFrame, ref float customTargetReachedRangeThreshold, ref float customTargetReachedRotationThreshold);

	public delegate bool OnTargetReachedDelegate(Agent agent, ref Agent targetAgent, ref UsableMachine targetUsableMachine, ref WorldFrame targetFrame);

	public delegate void OnTargetReachedWaitDelegate(Agent agent, ref float waitTimeInSeconds);

	private enum State
	{
		NoTarget,
		GoToUsableMachine,
		GoToAgent,
		GoToTargetFrame,
		NearAgent,
		NearStationaryTarget
	}

	private UsableMachine _targetUsableMachine;

	private Agent _targetAgent;

	private WorldFrame _targetFrame;

	private State _state;

	private bool _sentToTarget;

	private float _waitTimeInSeconds;

	private bool _isWaiting;

	private MissionTimer _waitTimer;

	private float _customTargetReachedRangeThreshold = 1f;

	private float _customTargetReachedRotationThreshold = 1f;

	private float _initialWaitInSeconds;

	private bool _isInitiallyWaiting;

	private SelectTargetDelegate _selectTargetDelegate;

	private OnTargetReachedDelegate _onTargetReachedDelegate;

	private OnTargetReachedWaitDelegate _onTargetReachWaitDelegate;

	public ScriptBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
	}

	public static void AddUsableMachineTarget(Agent ownerAgent, UsableMachine targetUsableMachine)
	{
		DailyBehaviorGroup behaviorGroup = ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		ScriptBehavior scriptBehavior = behaviorGroup.GetBehavior<ScriptBehavior>() ?? behaviorGroup.AddBehavior<ScriptBehavior>();
		bool num = behaviorGroup.ScriptedBehavior != scriptBehavior;
		scriptBehavior._targetUsableMachine = targetUsableMachine;
		scriptBehavior._state = State.GoToUsableMachine;
		scriptBehavior._sentToTarget = false;
		if (num)
		{
			behaviorGroup.SetScriptedBehavior<ScriptBehavior>();
		}
	}

	public static void AddAgentTarget(Agent ownerAgent, Agent targetAgent)
	{
		DailyBehaviorGroup behaviorGroup = ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		ScriptBehavior scriptBehavior = behaviorGroup.GetBehavior<ScriptBehavior>() ?? behaviorGroup.AddBehavior<ScriptBehavior>();
		bool num = behaviorGroup.ScriptedBehavior != scriptBehavior;
		scriptBehavior._targetAgent = targetAgent;
		scriptBehavior._state = State.GoToAgent;
		scriptBehavior._sentToTarget = false;
		if (num)
		{
			behaviorGroup.SetScriptedBehavior<ScriptBehavior>();
		}
	}

	public static void AddWorldFrameTarget(Agent ownerAgent, WorldFrame targetWorldFrame)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		DailyBehaviorGroup behaviorGroup = ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		ScriptBehavior scriptBehavior = behaviorGroup.GetBehavior<ScriptBehavior>() ?? behaviorGroup.AddBehavior<ScriptBehavior>();
		bool num = behaviorGroup.ScriptedBehavior != scriptBehavior;
		scriptBehavior._targetFrame = targetWorldFrame;
		scriptBehavior._state = State.GoToTargetFrame;
		scriptBehavior._sentToTarget = false;
		if (num)
		{
			behaviorGroup.SetScriptedBehavior<ScriptBehavior>();
		}
	}

	public static void AddTargetWithDelegate(Agent ownerAgent, SelectTargetDelegate selectTargetDelegate, OnTargetReachedWaitDelegate onTargetReachWaitDelegate, OnTargetReachedDelegate onTargetReachedDelegate, float initialWaitInSeconds = 0f)
	{
		DailyBehaviorGroup behaviorGroup = ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		ScriptBehavior scriptBehavior = behaviorGroup.GetBehavior<ScriptBehavior>() ?? behaviorGroup.AddBehavior<ScriptBehavior>();
		bool num = behaviorGroup.ScriptedBehavior != scriptBehavior;
		scriptBehavior._selectTargetDelegate = selectTargetDelegate;
		scriptBehavior._onTargetReachedDelegate = onTargetReachedDelegate;
		scriptBehavior._onTargetReachWaitDelegate = onTargetReachWaitDelegate;
		scriptBehavior._initialWaitInSeconds = initialWaitInSeconds;
		scriptBehavior._isInitiallyWaiting = initialWaitInSeconds > 0f;
		scriptBehavior._state = State.NoTarget;
		scriptBehavior._sentToTarget = false;
		if (num)
		{
			behaviorGroup.SetScriptedBehavior<ScriptBehavior>();
		}
	}

	public bool IsNearTarget(Agent targetAgent)
	{
		if (_targetAgent == targetAgent)
		{
			if (_state != State.NearAgent)
			{
				return _state == State.NearStationaryTarget;
			}
			return true;
		}
		return false;
	}

	public override void Tick(float dt, bool isSimulation)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		if (_isInitiallyWaiting)
		{
			if (_waitTimer == null)
			{
				_waitTimer = new MissionTimer(_initialWaitInSeconds);
			}
			else if (_waitTimer.Check(false))
			{
				_isInitiallyWaiting = false;
				_waitTimer = null;
			}
			return;
		}
		if (_state == State.NoTarget)
		{
			if (_selectTargetDelegate == null)
			{
				if (BehaviorGroup.ScriptedBehavior == this)
				{
					BehaviorGroup.DisableScriptedBehavior();
				}
				return;
			}
			SearchForNewTarget();
		}
		Vec3 position;
		MatrixFrame frame;
		Vec2 asVec;
		switch (_state)
		{
		case State.GoToUsableMachine:
			if (!_sentToTarget)
			{
				base.Navigator.SetTarget(_targetUsableMachine, isInitialTarget: false, (AIScriptedFrameFlags)0);
				_sentToTarget = true;
			}
			else
			{
				if (!base.OwnerAgent.IsUsingGameObject)
				{
					break;
				}
				position = base.OwnerAgent.Position;
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)_targetUsableMachine).GameEntity;
				if (((Vec3)(ref position)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin) < 1f)
				{
					if (CheckForSearchNewTarget(State.NearStationaryTarget))
					{
						base.OwnerAgent.StopUsingGameObject(false, (StopUsingGameObjectFlags)1);
					}
					else
					{
						RemoveTargets();
					}
				}
			}
			break;
		case State.GoToAgent:
			if (_targetAgent.IsActive())
			{
				float interactionDistanceToUsable = base.OwnerAgent.GetInteractionDistanceToUsable((IUsable)(object)_targetAgent);
				position = base.OwnerAgent.Position;
				if (((Vec3)(ref position)).DistanceSquared(_targetAgent.Position) < interactionDistanceToUsable * interactionDistanceToUsable)
				{
					if (!CheckForSearchNewTarget(State.NearAgent))
					{
						AgentNavigator navigator2 = base.Navigator;
						WorldPosition worldPosition2 = base.OwnerAgent.GetWorldPosition();
						frame = base.OwnerAgent.Frame;
						asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
						navigator2.SetTargetFrame(worldPosition2, ((Vec2)(ref asVec)).RotationInRadians, _customTargetReachedRangeThreshold, _customTargetReachedRotationThreshold, (AIScriptedFrameFlags)0);
						RemoveTargets();
					}
				}
				else
				{
					AgentNavigator navigator3 = base.Navigator;
					WorldPosition worldPosition3 = _targetAgent.GetWorldPosition();
					frame = _targetAgent.Frame;
					asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
					navigator3.SetTargetFrame(worldPosition3, ((Vec2)(ref asVec)).RotationInRadians, _customTargetReachedRangeThreshold, _customTargetReachedRotationThreshold, (AIScriptedFrameFlags)0);
				}
			}
			else if (!CheckForSearchNewTarget(State.NearAgent))
			{
				AgentNavigator navigator4 = base.Navigator;
				WorldPosition worldPosition4 = base.OwnerAgent.GetWorldPosition();
				frame = base.OwnerAgent.Frame;
				asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
				navigator4.SetTargetFrame(worldPosition4, ((Vec2)(ref asVec)).RotationInRadians, _customTargetReachedRangeThreshold, _customTargetReachedRotationThreshold, (AIScriptedFrameFlags)0);
				RemoveTargets();
			}
			break;
		case State.GoToTargetFrame:
			if (!_sentToTarget)
			{
				AgentNavigator navigator5 = base.Navigator;
				WorldPosition origin = _targetFrame.Origin;
				asVec = ((Vec3)(ref _targetFrame.Rotation.f)).AsVec2;
				navigator5.SetTargetFrame(origin, ((Vec2)(ref asVec)).RotationInRadians, _customTargetReachedRangeThreshold, _customTargetReachedRotationThreshold, (AIScriptedFrameFlags)16);
				_sentToTarget = true;
			}
			else if (base.Navigator.IsTargetReached() && !CheckForSearchNewTarget(State.NearStationaryTarget) && _waitTimer == null)
			{
				RemoveTargets();
			}
			break;
		case State.NearAgent:
		{
			position = base.OwnerAgent.Position;
			if (((Vec3)(ref position)).DistanceSquared(_targetAgent.Position) >= 1f)
			{
				_state = State.GoToAgent;
				break;
			}
			AgentNavigator navigator = base.Navigator;
			WorldPosition worldPosition = base.OwnerAgent.GetWorldPosition();
			frame = base.OwnerAgent.Frame;
			asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
			navigator.SetTargetFrame(worldPosition, ((Vec2)(ref asVec)).RotationInRadians, _customTargetReachedRangeThreshold, _customTargetReachedRotationThreshold, (AIScriptedFrameFlags)0);
			RemoveTargets();
			break;
		}
		}
	}

	private bool CheckForSearchNewTarget(State endState)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		bool flag = false;
		bool flag2 = false;
		if (_onTargetReachWaitDelegate != null && !_isWaiting)
		{
			_onTargetReachWaitDelegate(base.OwnerAgent, ref _waitTimeInSeconds);
			_isWaiting = _waitTimeInSeconds > 0f;
		}
		if (_isWaiting)
		{
			if (_waitTimer == null)
			{
				_waitTimer = new MissionTimer(_waitTimeInSeconds);
			}
			else if (_waitTimer.Check(false))
			{
				_isWaiting = false;
				_waitTimer = null;
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			if (_onTargetReachedDelegate != null)
			{
				flag2 = _onTargetReachedDelegate(base.OwnerAgent, ref _targetAgent, ref _targetUsableMachine, ref _targetFrame);
			}
			if (flag2)
			{
				SearchForNewTarget();
			}
			else
			{
				_state = endState;
			}
			return flag2;
		}
		return false;
	}

	private void SearchForNewTarget()
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Agent targetAgent = null;
		UsableMachine targetUsableMachine = null;
		WorldFrame targetFrame = WorldFrame.Invalid;
		float customTargetReachedRangeThreshold = _customTargetReachedRangeThreshold;
		float customTargetReachedRotationThreshold = _customTargetReachedRotationThreshold;
		if (_selectTargetDelegate(base.OwnerAgent, ref targetAgent, ref targetUsableMachine, ref targetFrame, ref customTargetReachedRangeThreshold, ref customTargetReachedRotationThreshold))
		{
			if (targetAgent != null)
			{
				_targetAgent = targetAgent;
				_state = State.GoToAgent;
				_sentToTarget = false;
			}
			else if (targetUsableMachine != null)
			{
				_targetUsableMachine = targetUsableMachine;
				_state = State.GoToUsableMachine;
				_sentToTarget = false;
			}
			else
			{
				_targetFrame = targetFrame;
				_state = State.GoToTargetFrame;
				_sentToTarget = false;
			}
			_customTargetReachedRangeThreshold = customTargetReachedRangeThreshold;
			_customTargetReachedRotationThreshold = customTargetReachedRotationThreshold;
		}
	}

	public override float GetAvailability(bool isSimulation)
	{
		return (_state != State.NoTarget) ? 1 : 0;
	}

	protected override void OnDeactivate()
	{
		base.Navigator.ClearTarget();
		RemoveTargets();
	}

	private void RemoveTargets()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_targetUsableMachine = null;
		_targetAgent = null;
		_targetFrame = WorldFrame.Invalid;
		_state = State.NoTarget;
		_selectTargetDelegate = null;
		_onTargetReachedDelegate = null;
		_sentToTarget = false;
	}

	public override string GetDebugInfo()
	{
		return "Scripted";
	}
}
