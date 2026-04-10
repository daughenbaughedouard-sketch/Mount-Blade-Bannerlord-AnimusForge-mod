using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Objects;

namespace SandBox.Missions.AgentBehaviors;

public class FollowAgentBehavior : AgentBehavior
{
	private enum State
	{
		Idle,
		OnMove,
		Fight
	}

	private const float _moveReactionProximityThreshold = 4f;

	private const float _longitudinalClearanceOffset = 2f;

	private const float _onFootMoveProximityThreshold = 1.2f;

	private const float _mountedMoveProximityThreshold = 2.2f;

	private const float _onFootAgentLongitudinalOffset = 0.6f;

	private const float _onFootAgentLateralOffset = 1f;

	private const float _mountedAgentLongitudinalOffset = 1.25f;

	private const float _mountedAgentLateralOffset = 1.5f;

	private float _idleDistance;

	private Agent _selectedAgent;

	private State _state;

	private Agent _deactivatedAgent;

	private bool _myLastStateWasRunning;

	private bool _updatePositionThisFrame;

	public FollowAgentBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
		_selectedAgent = null;
		_deactivatedAgent = null;
		_myLastStateWasRunning = false;
	}

	public void SetTargetAgent(Agent agent)
	{
		_selectedAgent = agent;
		_state = State.Idle;
		GameEntity val = base.Mission.Scene.FindEntityWithTag("navigation_mesh_deactivator");
		if (val != (GameEntity)null)
		{
			int disableFaceWithId = val.GetFirstScriptOfType<NavigationMeshDeactivator>().DisableFaceWithId;
			if (disableFaceWithId != -1)
			{
				base.OwnerAgent.SetAgentExcludeStateForFaceGroupId(disableFaceWithId, false);
			}
		}
		TryMoveStateTransition(forceMove: true);
	}

	public override void Tick(float dt, bool isSimulation)
	{
		if (_selectedAgent != null)
		{
			ControlMovement();
		}
	}

	private void ControlMovement()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		WorldPosition targetPosition = base.Navigator.TargetPosition;
		Vec3 position;
		if (((WorldPosition)(ref targetPosition)).IsValid && base.Navigator.IsTargetReached())
		{
			base.OwnerAgent.DisableScriptedMovement();
			base.OwnerAgent.SetMaximumSpeedLimit(-1f, false);
			if (_state == State.OnMove)
			{
				position = base.OwnerAgent.Position;
				Vec2 asVec = ((Vec3)(ref position)).AsVec2;
				position = _selectedAgent.Position;
				_idleDistance = ((Vec2)(ref asVec)).Distance(((Vec3)(ref position)).AsVec2);
			}
			_state = State.Idle;
		}
		Mission mission = base.Mission;
		Team team = base.OwnerAgent.Team;
		position = base.OwnerAgent.Position;
		int nearbyEnemyAgentCount = mission.GetNearbyEnemyAgentCount(team, ((Vec3)(ref position)).AsVec2, 5f);
		if (_state != State.Fight && nearbyEnemyAgentCount > 0)
		{
			base.OwnerAgent.SetWatchState((WatchState)2);
			base.OwnerAgent.ResetLookAgent();
			base.Navigator.ClearTarget();
			base.OwnerAgent.DisableScriptedMovement();
			_state = State.Fight;
			Debug.Print("[Follow agent behavior] Fight!", 0, (DebugColor)12, 17592186044416uL);
		}
		switch (_state)
		{
		case State.Fight:
			if (nearbyEnemyAgentCount == 0)
			{
				base.OwnerAgent.SetWatchState((WatchState)0);
				base.OwnerAgent.SetLookAgent(_selectedAgent);
				_state = State.Idle;
				Debug.Print("[Follow agent behavior] Stop fighting!", 0, (DebugColor)12, 17592186044416uL);
			}
			break;
		case State.Idle:
			TryMoveStateTransition(forceMove: false);
			break;
		case State.OnMove:
			MoveToFollowingAgent(forcedMove: false);
			break;
		}
	}

	private void TryMoveStateTransition(bool forceMove)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (_selectedAgent != null)
		{
			if ((base.OwnerAgent.GetScriptedFlags() & 0x200) != (_selectedAgent.GetScriptedFlags() & 0x200))
			{
				base.OwnerAgent.SetCrouchMode(_selectedAgent.CrouchMode);
			}
			Vec3 position = base.OwnerAgent.Position;
			Vec2 asVec = ((Vec3)(ref position)).AsVec2;
			position = _selectedAgent.Position;
			if (((Vec2)(ref asVec)).Distance(((Vec3)(ref position)).AsVec2) > 4f + _idleDistance)
			{
				_state = State.OnMove;
				MoveToFollowingAgent(forceMove);
			}
		}
	}

	private void MoveToFollowingAgent(bool forcedMove)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = _selectedAgent.Velocity;
		Vec2 asVec = ((Vec3)(ref val)).AsVec2;
		if (!(_updatePositionThisFrame || forcedMove) && !((Vec2)(ref asVec)).IsNonZero())
		{
			return;
		}
		_updatePositionThisFrame = false;
		WorldPosition worldPosition = _selectedAgent.GetWorldPosition();
		Vec2 val2 = (((Vec2)(ref asVec)).IsNonZero() ? ((Vec2)(ref asVec)).Normalized() : _selectedAgent.GetMovementDirection());
		Vec2 val3 = ((Vec2)(ref val2)).LeftVec();
		val = _selectedAgent.Position;
		Vec2 asVec2 = ((Vec3)(ref val)).AsVec2;
		val = base.OwnerAgent.Position;
		Vec2 val4 = asVec2 - ((Vec3)(ref val)).AsVec2;
		float lengthSquared = ((Vec2)(ref val4)).LengthSquared;
		int num = ((Vec2.DotProduct(val4, val3) > 0f) ? 1 : (-1));
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		foreach (Agent item in (List<Agent>)(object)base.Mission.Agents)
		{
			CampaignAgentComponent component = item.GetComponent<CampaignAgentComponent>();
			if (component?.AgentNavigator == null)
			{
				continue;
			}
			FollowAgentBehavior followAgentBehavior = component.AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>()?.GetBehavior<FollowAgentBehavior>();
			if (followAgentBehavior == null || followAgentBehavior._selectedAgent == null || followAgentBehavior._selectedAgent != _selectedAgent)
			{
				continue;
			}
			val = _selectedAgent.Position;
			Vec2 asVec3 = ((Vec3)(ref val)).AsVec2;
			val = item.Position;
			Vec2 val5 = asVec3 - ((Vec3)(ref val)).AsVec2;
			int num6 = ((Vec2.DotProduct(val5, val3) > 0f) ? 1 : (-1));
			if (!(((Vec2)(ref val5)).LengthSquared < lengthSquared))
			{
				continue;
			}
			if (num6 == num)
			{
				if (item.HasMount)
				{
					num3++;
				}
				else
				{
					num2++;
				}
			}
			if (Vec2.DotProduct(val5, val2) > 0.3f)
			{
				if (item.HasMount)
				{
					num5++;
				}
				else
				{
					num4++;
				}
			}
		}
		float num7 = (_selectedAgent.HasMount ? 1.25f : 0.6f);
		float num8 = (base.OwnerAgent.HasMount ? 1.25f : 0.6f);
		float num9 = (_selectedAgent.HasMount ? 1.5f : 1f);
		float num10 = (base.OwnerAgent.HasMount ? 1.5f : 1f);
		Vec2 val6 = val2 * (2f + 0.5f * (num8 + num7) + (float)num2 * 0.6f + (float)num3 * 1.25f);
		Vec2 val7 = (float)num * val3 * (0.5f * (num10 + num9) + (float)num2 * 1f + (float)num3 * 1.5f);
		val = _selectedAgent.Position;
		Vec2 val8 = ((Vec3)(ref val)).AsVec2 - val6 - val7;
		bool flag = false;
		ProximityMapSearchStruct val9 = AgentProximityMap.BeginSearch(Mission.Current, val8, 0.5f, false);
		while (((ProximityMapSearchStruct)(ref val9)).LastFoundAgent != null)
		{
			Agent lastFoundAgent = ((ProximityMapSearchStruct)(ref val9)).LastFoundAgent;
			if (lastFoundAgent.Index != base.OwnerAgent.Index && lastFoundAgent.Index != _selectedAgent.Index)
			{
				flag = true;
				break;
			}
			AgentProximityMap.FindNext(Mission.Current, ref val9);
		}
		float num11 = (base.OwnerAgent.HasMount ? 2.2f : 1.2f);
		if (!flag)
		{
			WorldPosition val10 = worldPosition;
			((WorldPosition)(ref val10))._002Ector(base.Mission.Scene, UIntPtr.Zero, ((WorldPosition)(ref val10)).GetGroundVec3(), false);
			((WorldPosition)(ref val10)).SetVec2(val8);
			if (((WorldPosition)(ref val10)).GetNavMesh() != UIntPtr.Zero && base.Mission.Scene.IsLineToPointClear(ref val10, ref worldPosition, base.OwnerAgent.Monster.BodyCapsuleRadius))
			{
				WorldPosition pos = val10;
				((WorldPosition)(ref pos)).SetVec2(((WorldPosition)(ref pos)).AsVec2 + val2 * 1.5f);
				if (((WorldPosition)(ref pos)).GetNavMesh() != UIntPtr.Zero && base.Mission.Scene.IsLineToPointClear(ref pos, ref val10, base.OwnerAgent.Monster.BodyCapsuleRadius))
				{
					SetMovePos(pos, _selectedAgent.MovementDirectionAsAngle, num11, (AIScriptedFrameFlags)2);
				}
				else
				{
					SetMovePos(val10, _selectedAgent.MovementDirectionAsAngle, num11, (AIScriptedFrameFlags)2);
				}
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			float rangeThreshold = num11 + (float)num4 * 0.6f + (float)num5 * 1.25f;
			SetMovePos(worldPosition, _selectedAgent.MovementDirectionAsAngle, rangeThreshold, (AIScriptedFrameFlags)2);
		}
	}

	private void SetMovePos(WorldPosition pos, float rotationInRadians, float rangeThreshold, AIScriptedFrameFlags flags)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (int)base.Mission.Mode == 4;
		if (base.Navigator.CharacterHasVisiblePrefabs)
		{
			_myLastStateWasRunning = false;
		}
		else
		{
			if (flag && _selectedAgent.CrouchMode)
			{
				flags = (AIScriptedFrameFlags)(flags | 0x200);
			}
			if (flag && _selectedAgent.WalkMode)
			{
				base.OwnerAgent.SetMaximumSpeedLimit(_selectedAgent.CrouchMode ? _selectedAgent.Monster.CrouchWalkingSpeedLimit : _selectedAgent.Monster.WalkingSpeedLimit, false);
				_myLastStateWasRunning = false;
			}
			else
			{
				Vec3 val = base.OwnerAgent.Position;
				Vec2 asVec = ((Vec3)(ref val)).AsVec2;
				float num = ((Vec2)(ref asVec)).Distance(((WorldPosition)(ref pos)).AsVec2);
				if (num - rangeThreshold <= 0.5f * (_myLastStateWasRunning ? 1f : 1.2f))
				{
					val = _selectedAgent.Velocity;
					asVec = ((Vec3)(ref val)).AsVec2;
					if (((Vec2)(ref asVec)).Length <= base.OwnerAgent.Monster.WalkingSpeedLimit * (_myLastStateWasRunning ? 1f : 1.2f))
					{
						_myLastStateWasRunning = false;
						goto IL_0158;
					}
				}
				Agent ownerAgent = base.OwnerAgent;
				float num2 = num - rangeThreshold;
				val = _selectedAgent.Velocity;
				asVec = ((Vec3)(ref val)).AsVec2;
				ownerAgent.SetMaximumSpeedLimit(num2 + ((Vec2)(ref asVec)).Length, false);
				_myLastStateWasRunning = true;
			}
		}
		goto IL_0158;
		IL_0158:
		if (!_myLastStateWasRunning)
		{
			flags = (AIScriptedFrameFlags)(flags | 0x10);
		}
		base.Navigator.SetTargetFrame(pos, rotationInRadians, rangeThreshold, -10f, flags, flag);
	}

	public override void OnAgentRemoved(Agent agent)
	{
		if (agent == _selectedAgent)
		{
			base.OwnerAgent.ResetLookAgent();
			_selectedAgent = null;
		}
	}

	protected override void OnActivate()
	{
		if (_deactivatedAgent != null)
		{
			SetTargetAgent(_deactivatedAgent);
			_deactivatedAgent = null;
		}
	}

	protected override void OnDeactivate()
	{
		_state = State.Idle;
		_deactivatedAgent = _selectedAgent;
		_selectedAgent = null;
		base.OwnerAgent.DisableScriptedMovement();
		base.OwnerAgent.ResetLookAgent();
		base.Navigator.ClearTarget();
	}

	public override string GetDebugInfo()
	{
		return "Follow " + _selectedAgent.Name + " (id:" + _selectedAgent.Index + ")";
	}

	public override float GetAvailability(bool isSimulation)
	{
		return (_selectedAgent != null) ? 100 : 0;
	}
}
