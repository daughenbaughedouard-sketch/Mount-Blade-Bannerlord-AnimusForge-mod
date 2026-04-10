using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Conversation;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public sealed class AgentNavigator
{
	public enum NavigationState
	{
		NoTarget,
		GoToTarget,
		AtTargetPosition,
		UseMachine
	}

	private const float SeeingDistance = 30f;

	public readonly Agent OwnerAgent;

	private readonly Mission _mission;

	private readonly List<AgentBehaviorGroup> _behaviorGroups;

	private readonly ItemObject _specialItem;

	private UsableMachineAIBase _targetBehavior;

	private bool _targetReached;

	private float _rangeThreshold;

	private float _rotationScoreThreshold;

	private string _specialTargetTag;

	private bool _disableClearTargetWhenTargetIsReached;

	private readonly Dictionary<sbyte, string> _prefabNamesForBones;

	private readonly List<int> _prevPrefabs;

	private readonly MissionConversationLogic _conversationHandler;

	private readonly BasicMissionTimer _checkBehaviorGroupsTimer;

	public UsableMachine TargetUsableMachine { get; private set; }

	public WorldPosition TargetPosition { get; private set; }

	public Vec2 TargetDirection { get; private set; }

	public GameEntity TargetEntity { get; private set; }

	public Alley MemberOfAlley { get; private set; }

	public string SpecialTargetTag
	{
		get
		{
			return _specialTargetTag;
		}
		set
		{
			if (value != _specialTargetTag)
			{
				_specialTargetTag = value;
				GetActiveBehavior()?.OnSpecialTargetChanged();
			}
		}
	}

	private Dictionary<KeyValuePair<sbyte, string>, int> _bodyComponents { get; set; }

	public NavigationState _agentState { get; private set; }

	public bool CharacterHasVisiblePrefabs { get; private set; }

	public AgentNavigator(Agent agent, LocationCharacter locationCharacter)
		: this(agent)
	{
		SpecialTargetTag = locationCharacter.SpecialTargetTag;
		_prefabNamesForBones = locationCharacter.PrefabNamesForBones;
		_specialItem = locationCharacter.SpecialItem;
		MemberOfAlley = locationCharacter.MemberOfAlley;
		SetItemsVisibility(isVisible: true);
		SetSpecialItem();
	}

	public AgentNavigator(Agent agent)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		_mission = agent.Mission;
		_conversationHandler = _mission.GetMissionBehavior<MissionConversationLogic>();
		OwnerAgent = agent;
		_prefabNamesForBones = new Dictionary<sbyte, string>();
		_behaviorGroups = new List<AgentBehaviorGroup>();
		_bodyComponents = new Dictionary<KeyValuePair<sbyte, string>, int>();
		SpecialTargetTag = string.Empty;
		MemberOfAlley = null;
		TargetUsableMachine = null;
		_checkBehaviorGroupsTimer = new BasicMissionTimer();
		_prevPrefabs = new List<int>();
		CharacterHasVisiblePrefabs = false;
	}

	public void OnStopUsingGameObject()
	{
		_targetBehavior = null;
		TargetUsableMachine = null;
		_agentState = NavigationState.NoTarget;
	}

	public void OnAgentRemoved(Agent agent)
	{
		foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
		{
			behaviorGroup.OnAgentRemoved(agent);
		}
	}

	public void SetTarget(UsableMachine usableMachine, bool isInitialTarget = false, AIScriptedFrameFlags customFlags = (AIScriptedFrameFlags)0)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (usableMachine == null)
		{
			UsableMachine targetUsableMachine = TargetUsableMachine;
			if (targetUsableMachine != null)
			{
				((IDetachment)targetUsableMachine).RemoveAgent(OwnerAgent);
			}
			TargetUsableMachine = null;
			OwnerAgent.DisableScriptedMovement();
			OwnerAgent.ClearTargetFrame();
			TargetPosition = WorldPosition.Invalid;
			TargetEntity = null;
			_agentState = NavigationState.NoTarget;
		}
		else if (TargetUsableMachine != usableMachine || isInitialTarget)
		{
			TargetPosition = WorldPosition.Invalid;
			_agentState = NavigationState.NoTarget;
			UsableMachine targetUsableMachine2 = TargetUsableMachine;
			if (targetUsableMachine2 != null)
			{
				((IDetachment)targetUsableMachine2).RemoveAgent(OwnerAgent);
			}
			if (usableMachine.IsStandingPointAvailableForAgent(OwnerAgent))
			{
				TargetUsableMachine = usableMachine;
				TargetPosition = WorldPosition.Invalid;
				_agentState = NavigationState.UseMachine;
				_targetBehavior = TargetUsableMachine.CreateAIBehaviorObject();
				((IDetachment)TargetUsableMachine).AddAgent(OwnerAgent, -1, customFlags);
				_targetReached = false;
			}
		}
	}

	public void SetTargetFrame(WorldPosition position, float rotation, float rangeThreshold = 1f, float rotationThreshold = -10f, AIScriptedFrameFlags flags = (AIScriptedFrameFlags)0, bool disableClearTargetWhenTargetIsReached = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (_agentState != NavigationState.NoTarget)
		{
			ClearTarget();
		}
		TargetPosition = position;
		TargetDirection = Vec2.FromRotation(rotation);
		_rangeThreshold = rangeThreshold;
		_rotationScoreThreshold = rotationThreshold;
		_disableClearTargetWhenTargetIsReached = disableClearTargetWhenTargetIsReached;
		if (IsTargetReached())
		{
			TargetPosition = WorldPosition.Invalid;
			_agentState = NavigationState.NoTarget;
		}
		else
		{
			OwnerAgent.SetScriptedPositionAndDirection(ref position, rotation, false, flags);
			_agentState = NavigationState.GoToTarget;
		}
	}

	public void ClearTarget()
	{
		SetTarget(null, isInitialTarget: false, (AIScriptedFrameFlags)0);
	}

	public void Tick(float dt, bool isSimulation = false)
	{
		HandleBehaviorGroups(isSimulation);
		if (ConversationMission.ConversationAgents.Contains(OwnerAgent))
		{
			foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
			{
				if (behaviorGroup.IsActive)
				{
					behaviorGroup.ConversationTick();
				}
			}
		}
		else
		{
			TickBehaviorGroups(dt, isSimulation);
		}
		if (TargetUsableMachine != null)
		{
			_targetBehavior.Tick(OwnerAgent, (Formation)null, (Team)null, dt);
		}
		else
		{
			HandleMovement();
		}
		if (TargetUsableMachine != null && isSimulation)
		{
			_targetBehavior.TeleportUserAgentsToMachine(new List<Agent> { OwnerAgent });
		}
	}

	public float GetDistanceToTarget(UsableMachine target)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		float result = 100000f;
		if (target != null && OwnerAgent.CurrentlyUsedGameObject != null)
		{
			WorldFrame userFrameForAgent = OwnerAgent.CurrentlyUsedGameObject.GetUserFrameForAgent(OwnerAgent);
			Vec3 groundVec = ((WorldPosition)(ref userFrameForAgent.Origin)).GetGroundVec3();
			result = ((Vec3)(ref groundVec)).Distance(OwnerAgent.Position);
		}
		return result;
	}

	public bool IsTargetReached()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Vec2 targetDirection = TargetDirection;
		if (((Vec2)(ref targetDirection)).IsValid)
		{
			WorldPosition targetPosition = TargetPosition;
			if (((WorldPosition)(ref targetPosition)).IsValid)
			{
				float num = Vec2.DotProduct(TargetDirection, OwnerAgent.GetMovementDirection());
				Vec3 position = OwnerAgent.Position;
				targetPosition = TargetPosition;
				Vec3 val = position - ((WorldPosition)(ref targetPosition)).GetGroundVec3();
				_targetReached = ((Vec3)(ref val)).LengthSquared < _rangeThreshold * _rangeThreshold && num > _rotationScoreThreshold;
			}
		}
		return _targetReached;
	}

	private void HandleMovement()
	{
		if (_agentState == NavigationState.GoToTarget && IsTargetReached())
		{
			_agentState = NavigationState.AtTargetPosition;
			if (!_disableClearTargetWhenTargetIsReached)
			{
				OwnerAgent.ClearTargetFrame();
			}
		}
	}

	public void HoldAndHideRecentlyUsedMeshes()
	{
		foreach (KeyValuePair<KeyValuePair<sbyte, string>, int> bodyComponent in _bodyComponents)
		{
			if (OwnerAgent.IsSynchedPrefabComponentVisible(bodyComponent.Value))
			{
				OwnerAgent.SetSynchedPrefabComponentVisibility(bodyComponent.Value, false);
				_prevPrefabs.Add(bodyComponent.Value);
			}
		}
	}

	public void RecoverRecentlyUsedMeshes()
	{
		foreach (int prevPrefab in _prevPrefabs)
		{
			OwnerAgent.SetSynchedPrefabComponentVisibility(prevPrefab, true);
		}
		_prevPrefabs.Clear();
	}

	public bool CanSeeAgent(Agent otherAgent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = OwnerAgent.Position - otherAgent.Position;
		if (((Vec3)(ref val)).Length < 30f)
		{
			Vec3 eyeGlobalPosition = otherAgent.GetEyeGlobalPosition();
			Vec3 eyeGlobalPosition2 = OwnerAgent.GetEyeGlobalPosition();
			float num = default(float);
			if (MathF.Abs(Vec3.AngleBetweenTwoVectors(otherAgent.Position - OwnerAgent.Position, OwnerAgent.LookDirection)) < 1.5f)
			{
				return !Mission.Current.Scene.RayCastForClosestEntityOrTerrain(eyeGlobalPosition2, eyeGlobalPosition, ref num, 0.01f, (BodyFlags)79617);
			}
		}
		return false;
	}

	public bool IsCarryingSomething()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if ((int)OwnerAgent.GetPrimaryWieldedItemIndex() >= 0 || (int)OwnerAgent.GetOffhandWieldedItemIndex() >= 0)
		{
			return true;
		}
		return _bodyComponents.Any((KeyValuePair<KeyValuePair<sbyte, string>, int> component) => OwnerAgent.IsSynchedPrefabComponentVisible(component.Value));
	}

	public void SetPrefabVisibility(sbyte realBoneIndex, string prefabName, bool isVisible)
	{
		KeyValuePair<sbyte, string> key = new KeyValuePair<sbyte, string>(realBoneIndex, prefabName);
		int value2;
		if (isVisible)
		{
			if (!_bodyComponents.TryGetValue(key, out var value))
			{
				_bodyComponents.Add(key, OwnerAgent.AddSynchedPrefabComponentToBone(prefabName, realBoneIndex));
			}
			else if (!OwnerAgent.IsSynchedPrefabComponentVisible(value))
			{
				OwnerAgent.SetSynchedPrefabComponentVisibility(value, true);
			}
		}
		else if (_bodyComponents.TryGetValue(key, out value2) && OwnerAgent.IsSynchedPrefabComponentVisible(value2))
		{
			OwnerAgent.SetSynchedPrefabComponentVisibility(value2, false);
		}
	}

	public bool GetPrefabVisibility(sbyte realBoneIndex, string prefabName)
	{
		KeyValuePair<sbyte, string> key = new KeyValuePair<sbyte, string>(realBoneIndex, prefabName);
		if (_bodyComponents.TryGetValue(key, out var value) && OwnerAgent.IsSynchedPrefabComponentVisible(value))
		{
			return true;
		}
		return false;
	}

	public void SetSpecialItem()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (_specialItem == null)
		{
			return;
		}
		bool flag = false;
		EquipmentIndex val = (EquipmentIndex)(-1);
		for (EquipmentIndex val2 = (EquipmentIndex)0; (int)val2 <= 3; val2 = (EquipmentIndex)(val2 + 1))
		{
			MissionWeapon val3 = OwnerAgent.Equipment[val2];
			if (((MissionWeapon)(ref val3)).IsEmpty)
			{
				val = val2;
				continue;
			}
			val3 = OwnerAgent.Equipment[val2];
			if (((MissionWeapon)(ref val3)).Item == _specialItem)
			{
				val = val2;
				flag = true;
				break;
			}
		}
		if ((int)val == -1)
		{
			OwnerAgent.DropItem((EquipmentIndex)3, (WeaponClass)0);
			val = (EquipmentIndex)3;
		}
		if (!flag)
		{
			ItemObject specialItem = _specialItem;
			IAgentOriginBase origin = OwnerAgent.Origin;
			MissionWeapon val4 = default(MissionWeapon);
			((MissionWeapon)(ref val4))._002Ector(specialItem, (ItemModifier)null, (origin != null) ? origin.Banner : null);
			OwnerAgent.EquipWeaponWithNewEntity(val, ref val4);
		}
		OwnerAgent.TryToWieldWeaponInSlot(val, (WeaponWieldActionType)1, false);
	}

	public void SetItemsVisibility(bool isVisible)
	{
		foreach (KeyValuePair<sbyte, string> prefabNamesForBone in _prefabNamesForBones)
		{
			SetPrefabVisibility(prefabNamesForBone.Key, prefabNamesForBone.Value, isVisible);
		}
		CharacterHasVisiblePrefabs = _prefabNamesForBones.Count > 0 && isVisible;
	}

	public void SetCommonArea(Alley alley)
	{
		if (alley != MemberOfAlley)
		{
			MemberOfAlley = alley;
			SpecialTargetTag = ((alley == null) ? "" : ((SettlementArea)alley).Tag);
		}
	}

	public void ForceThink(float inSeconds)
	{
		foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
		{
			behaviorGroup.ForceThink(inSeconds);
		}
	}

	public T AddBehaviorGroup<T>() where T : AgentBehaviorGroup
	{
		T val = GetBehaviorGroup<T>();
		if (val == null)
		{
			val = Activator.CreateInstance(typeof(T), this, _mission) as T;
			if (val != null)
			{
				_behaviorGroups.Add(val);
			}
		}
		return val;
	}

	public T GetBehaviorGroup<T>() where T : AgentBehaviorGroup
	{
		foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
		{
			if (behaviorGroup is T)
			{
				return (T)behaviorGroup;
			}
		}
		return null;
	}

	public AgentBehavior GetBehavior<T>() where T : AgentBehavior
	{
		foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
		{
			foreach (AgentBehavior behavior in behaviorGroup.Behaviors)
			{
				if (behavior.GetType() == typeof(T))
				{
					return behavior;
				}
			}
		}
		return null;
	}

	public bool HasBehaviorGroup<T>()
	{
		foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
		{
			if (behaviorGroup.GetType() is T)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveBehaviorGroup<T>() where T : AgentBehaviorGroup
	{
		for (int i = 0; i < _behaviorGroups.Count; i++)
		{
			if (_behaviorGroups[i] is T)
			{
				_behaviorGroups.RemoveAt(i);
			}
		}
	}

	public void RefreshBehaviorGroups(bool isSimulation)
	{
		_checkBehaviorGroupsTimer.Reset();
		float num = 0f;
		AgentBehaviorGroup agentBehaviorGroup = null;
		foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
		{
			float score = behaviorGroup.GetScore(isSimulation);
			if (score > num)
			{
				num = score;
				agentBehaviorGroup = behaviorGroup;
			}
		}
		if (num > 0f && agentBehaviorGroup != null && !agentBehaviorGroup.IsActive)
		{
			ActivateGroup(agentBehaviorGroup);
		}
	}

	private void ActivateGroup(AgentBehaviorGroup behaviorGroup)
	{
		foreach (AgentBehaviorGroup behaviorGroup2 in _behaviorGroups)
		{
			behaviorGroup2.IsActive = false;
		}
		behaviorGroup.IsActive = true;
	}

	private void HandleBehaviorGroups(bool isSimulation)
	{
		if (isSimulation || _checkBehaviorGroupsTimer.ElapsedTime > 1f)
		{
			RefreshBehaviorGroups(isSimulation);
		}
	}

	private void TickBehaviorGroups(float dt, bool isSimulation)
	{
		if (!OwnerAgent.IsActive())
		{
			return;
		}
		foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
		{
			behaviorGroup.Tick(dt, isSimulation);
		}
	}

	public AgentBehavior GetActiveBehavior()
	{
		foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
		{
			if (behaviorGroup.IsActive)
			{
				return behaviorGroup.GetActiveBehavior();
			}
		}
		return null;
	}

	public AgentBehaviorGroup GetActiveBehaviorGroup()
	{
		foreach (AgentBehaviorGroup behaviorGroup in _behaviorGroups)
		{
			if (behaviorGroup.IsActive)
			{
				return behaviorGroup;
			}
		}
		return null;
	}
}
