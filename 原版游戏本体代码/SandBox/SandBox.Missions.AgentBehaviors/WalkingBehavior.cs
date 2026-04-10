using System.Collections.Generic;
using SandBox.Missions.MissionLogics;
using SandBox.Objects.AnimationPoints;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class WalkingBehavior : AgentBehavior
{
	private readonly MissionAgentHandler _missionAgentHandler;

	private readonly bool _isIndoor;

	private UsableMachine _wanderTarget;

	private UsableMachine _lastTarget;

	private Timer _waitTimer;

	private bool _indoorWanderingIsActive;

	private bool _outdoorWanderingIsActive;

	private bool _wasSimulation;

	private bool CanWander
	{
		get
		{
			if (!_isIndoor || !_indoorWanderingIsActive)
			{
				if (!_isIndoor)
				{
					return _outdoorWanderingIsActive;
				}
				return false;
			}
			return true;
		}
	}

	public WalkingBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
		_missionAgentHandler = base.Mission.GetMissionBehavior<MissionAgentHandler>();
		_wanderTarget = null;
		_isIndoor = CampaignMission.Current.Location.IsIndoor;
		_indoorWanderingIsActive = true;
		_outdoorWanderingIsActive = true;
		_wasSimulation = false;
	}

	public void SetIndoorWandering(bool isActive)
	{
		_indoorWanderingIsActive = isActive;
	}

	public void SetOutdoorWandering(bool isActive)
	{
		_outdoorWanderingIsActive = isActive;
	}

	public override void Tick(float dt, bool isSimulation)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		if (_wanderTarget == null || base.Navigator.TargetUsableMachine == null || ((MissionObject)_wanderTarget).IsDisabled || !_wanderTarget.IsStandingPointAvailableForAgent(base.OwnerAgent))
		{
			_wanderTarget = FindTarget();
			_lastTarget = _wanderTarget;
		}
		else if (base.Navigator.GetDistanceToTarget(_wanderTarget) < 5f)
		{
			bool flag = _wasSimulation && !isSimulation && _wanderTarget != null && _waitTimer != null && MBRandom.RandomFloat < (_isIndoor ? 0f : (Settlement.CurrentSettlement.IsVillage ? 0.6f : 0.1f));
			if (_waitTimer == null)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)_wanderTarget).GameEntity;
				if (!((WeakGameEntity)(ref gameEntity)).HasTag("npc_idle"))
				{
					SetTimerForTheAgent(isSimulation);
				}
			}
			else if (_waitTimer.Check(base.Mission.CurrentTime) || flag)
			{
				if (CanWander)
				{
					_waitTimer = null;
					UsableMachine val = FindTarget();
					if (val == null || IsChildrenOfSameParent(val, _wanderTarget))
					{
						SetTimerForTheAgent(isSimulation);
					}
					else
					{
						_lastTarget = _wanderTarget;
						_wanderTarget = val;
					}
				}
				else
				{
					_waitTimer.Reset(100f);
				}
			}
		}
		if (base.OwnerAgent.CurrentlyUsedGameObject != null && base.Navigator.GetDistanceToTarget(_lastTarget) > 1f)
		{
			base.Navigator.SetTarget(_lastTarget, _lastTarget == _wanderTarget, (AIScriptedFrameFlags)0);
		}
		base.Navigator.SetTarget(_wanderTarget, isInitialTarget: false, (AIScriptedFrameFlags)0);
		_wasSimulation = isSimulation;
	}

	private void SetTimerForTheAgent(bool isSimulation)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		float num = ((base.OwnerAgent.CurrentlyUsedGameObject is AnimationPoint animationPoint) ? animationPoint.GetRandomWaitInSeconds() : 10f);
		if (isSimulation && MBRandom.RandomFloat < 0.33f)
		{
			num /= 10f + MBRandom.RandomFloat * 10f;
		}
		_waitTimer = new Timer(base.Mission.CurrentTime, (num < 0f) ? 2.1474836E+09f : num, true);
	}

	private bool IsChildrenOfSameParent(UsableMachine machine, UsableMachine otherMachine)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)machine).GameEntity;
		WeakGameEntity parent;
		while (true)
		{
			parent = ((WeakGameEntity)(ref val)).Parent;
			if (!((WeakGameEntity)(ref parent)).IsValid)
			{
				break;
			}
			val = ((WeakGameEntity)(ref val)).Parent;
		}
		WeakGameEntity val2 = ((ScriptComponentBehavior)otherMachine).GameEntity;
		while (true)
		{
			parent = ((WeakGameEntity)(ref val2)).Parent;
			if (!((WeakGameEntity)(ref parent)).IsValid)
			{
				break;
			}
			val2 = ((WeakGameEntity)(ref val2)).Parent;
		}
		return val == val2;
	}

	public override void ConversationTick()
	{
		if (_waitTimer != null)
		{
			_waitTimer.Reset(base.Mission.CurrentTime);
		}
	}

	public override float GetAvailability(bool isSimulation)
	{
		if (FindTarget() == null)
		{
			return 0f;
		}
		return 1f;
	}

	public override void SetCustomWanderTarget(UsableMachine customUsableMachine)
	{
		_wanderTarget = customUsableMachine;
		if (_waitTimer != null)
		{
			_waitTimer = null;
		}
	}

	private UsableMachine FindRandomWalkingTarget(bool forWaiting)
	{
		if (forWaiting && (_wanderTarget ?? base.Navigator.TargetUsableMachine) != null)
		{
			return null;
		}
		string text = base.OwnerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SpecialTargetTag;
		if (text == null)
		{
			text = "npc_common";
		}
		else if (!_missionAgentHandler.HasUsablePointWithTag(text))
		{
			text = "npc_common_limited";
		}
		return _missionAgentHandler.FindUnusedPointWithTagForAgent(base.OwnerAgent, text);
	}

	private UsableMachine FindTarget()
	{
		return FindRandomWalkingTarget(_isIndoor && !_indoorWanderingIsActive);
	}

	private float GetTargetScore(UsableMachine usableMachine)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (base.OwnerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SpecialTargetTag != null)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)usableMachine).GameEntity;
			if (!((WeakGameEntity)(ref gameEntity)).HasTag(base.OwnerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SpecialTargetTag))
			{
				return 0f;
			}
		}
		StandingPoint vacantStandingPointForAI = usableMachine.GetVacantStandingPointForAI(base.OwnerAgent);
		if (vacantStandingPointForAI == null || ((UsableMissionObject)vacantStandingPointForAI).IsDisabledForAgent(base.OwnerAgent))
		{
			return 0f;
		}
		float num = 1f;
		WorldFrame userFrameForAgent = ((UsableMissionObject)vacantStandingPointForAI).GetUserFrameForAgent(base.OwnerAgent);
		Vec3 val = ((WorldPosition)(ref userFrameForAgent.Origin)).GetGroundVec3() - base.OwnerAgent.Position;
		if (((Vec3)(ref val)).Length < 2f)
		{
			num *= ((Vec3)(ref val)).Length / 2f;
		}
		return num * (0.8f + MBRandom.RandomFloat * 0.2f);
	}

	public override void OnSpecialTargetChanged()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (_wanderTarget == null)
		{
			return;
		}
		WeakGameEntity gameEntity;
		if (!Extensions.IsEmpty<char>((IEnumerable<char>)base.Navigator.SpecialTargetTag))
		{
			gameEntity = ((ScriptComponentBehavior)_wanderTarget).GameEntity;
			if (!((WeakGameEntity)(ref gameEntity)).HasTag(base.Navigator.SpecialTargetTag))
			{
				_wanderTarget = null;
				base.Navigator.SetTarget(_wanderTarget, isInitialTarget: false, (AIScriptedFrameFlags)0);
				return;
			}
		}
		if (Extensions.IsEmpty<char>((IEnumerable<char>)base.Navigator.SpecialTargetTag))
		{
			gameEntity = ((ScriptComponentBehavior)_wanderTarget).GameEntity;
			if (!((WeakGameEntity)(ref gameEntity)).HasTag("npc_common"))
			{
				_wanderTarget = null;
				base.Navigator.SetTarget(_wanderTarget, isInitialTarget: false, (AIScriptedFrameFlags)0);
			}
		}
	}

	public override string GetDebugInfo()
	{
		string text = "Walk ";
		if (_waitTimer != null)
		{
			text = text + "(Wait " + (int)_waitTimer.ElapsedTime() + "/" + _waitTimer.Duration + ")";
		}
		else if (_wanderTarget == null)
		{
			text += "(search for target!)";
		}
		return text;
	}

	protected override void OnDeactivate()
	{
		base.Navigator.ClearTarget();
		_wanderTarget = null;
		_waitTimer = null;
	}
}
