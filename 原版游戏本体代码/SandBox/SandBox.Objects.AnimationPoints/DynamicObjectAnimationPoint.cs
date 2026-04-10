using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.AnimationPoints;

public class DynamicObjectAnimationPoint : StandingPoint
{
	private enum State
	{
		NotUsing,
		StartToUse,
		Using
	}

	private const float RangeThreshold = 0.2f;

	private const float RotationScoreThreshold = 0.99f;

	private const float ActionSpeedRandomMinValue = 0.8f;

	private const float AnimationRandomProgressMaxValue = 0.5f;

	private const string AlternativeTag = "alternative";

	private ActionIndexCache _lastAction;

	public string ArriveAction = "";

	public string LoopStartAction = "";

	public string LeaveAction = "";

	public float ActionSpeed = 1f;

	public bool KeepOldVisibility;

	private Vec3 _pointRotation;

	private ActionIndexCache ArriveActionCode;

	protected ActionIndexCache LoopStartActionCode;

	private ActionIndexCache LeaveActionCode;

	protected ActionIndexCache DefaultActionCode;

	private State _state;

	public float ForwardDistanceToPivotPoint;

	public float SideDistanceToPivotPoint;

	private List<AnimationPoint.ItemForBone> _itemsForBones;

	public string RightHandItem = "";

	public HumanBone RightHandItemBone = (HumanBone)27;

	public string LeftHandItem = "";

	public HumanBone LeftHandItemBone = (HumanBone)20;

	private EquipmentIndex _equipmentIndexMainHand;

	private EquipmentIndex _equipmentIndexOffHand;

	public int GroupId = -1;

	private string _selectedRightHandItem;

	private string _selectedLeftHandItem;

	public bool IsArriveActionFinished { get; private set; }

	protected string SelectedRightHandItem
	{
		get
		{
			return _selectedRightHandItem;
		}
		set
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (value != _selectedRightHandItem)
			{
				AnimationPoint.ItemForBone newItem = new AnimationPoint.ItemForBone(RightHandItemBone, value, isVisible: false);
				AssignItemToBone(newItem);
				_selectedRightHandItem = value;
			}
		}
	}

	protected string SelectedLeftHandItem
	{
		get
		{
			return _selectedLeftHandItem;
		}
		set
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (value != _selectedLeftHandItem)
			{
				AnimationPoint.ItemForBone newItem = new AnimationPoint.ItemForBone(LeftHandItemBone, value, isVisible: false);
				AssignItemToBone(newItem);
				_selectedLeftHandItem = value;
			}
		}
	}

	public override bool PlayerStopsUsingWhenInteractsWithOther => false;

	public override bool DisableCombatActionsOnUse => !((UsableMissionObject)this).IsInstantUse;

	public bool IsActive { get; private set; } = true;

	protected override void OnTick(float dt)
	{
		((StandingPoint)this).OnTick(dt);
		Tick(dt);
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (((UsableMissionObject)this).HasUser)
		{
			return (TickRequirement)(((StandingPoint)this).GetTickRequirement() | 2);
		}
		return ((StandingPoint)this).GetTickRequirement();
	}

	protected override bool DoesActionTypeStopUsingGameObject(ActionCodeType actionType)
	{
		return false;
	}

	public override bool IsUsableByAgent(Agent userAgent)
	{
		if (IsActive)
		{
			return ((StandingPoint)this).IsUsableByAgent(userAgent);
		}
		return false;
	}

	public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		((StandingPoint)this).OnUse(userAgent, agentBoneIndex);
		_equipmentIndexMainHand = ((UsableMissionObject)this).UserAgent.GetPrimaryWieldedItemIndex();
		_equipmentIndexOffHand = ((UsableMissionObject)this).UserAgent.GetOffhandWieldedItemIndex();
		_state = State.NotUsing;
	}

	public override WorldFrame GetUserFrameForAgent(Agent agent)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		WorldFrame userFrameForAgent = ((StandingPoint)this).GetUserFrameForAgent(agent);
		float agentScale = agent.AgentScale;
		((WorldPosition)(ref userFrameForAgent.Origin)).SetVec2(((WorldPosition)(ref userFrameForAgent.Origin)).AsVec2 + (((Vec3)(ref userFrameForAgent.Rotation.f)).AsVec2 * (0f - ForwardDistanceToPivotPoint) + ((Vec3)(ref userFrameForAgent.Rotation.s)).AsVec2 * SideDistanceToPivotPoint) * (1f - agentScale));
		return userFrameForAgent;
	}

	public override bool IsDisabledForAgent(Agent agent)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		if (((UsableMissionObject)this).HasUser && ((UsableMissionObject)this).UserAgent == agent)
		{
			if (IsActive)
			{
				return ((UsableMissionObject)this).IsDeactivated;
			}
			return true;
		}
		if (!IsActive || agent.MountAgent != null || ((UsableMissionObject)this).IsDeactivated || !agent.IsAbleToUseMachine() || (!agent.IsAIControlled && (((UsableMissionObject)this).IsDisabledForPlayers || ((UsableMissionObject)this).HasUser)))
		{
			return true;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity parent = ((WeakGameEntity)(ref gameEntity)).Parent;
		if (((WeakGameEntity)(ref parent)).IsValid && ((WeakGameEntity)(ref parent)).HasScriptOfType<UsableMachine>())
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).HasTag("alternative"))
			{
				if (agent.IsAIControlled && ((WeakGameEntity)(ref parent)).HasTag("reserved"))
				{
					return true;
				}
				string text = ((agent.GetComponent<CampaignAgentComponent>()?.AgentNavigator != null) ? agent.GetComponent<CampaignAgentComponent>().AgentNavigator.SpecialTargetTag : string.Empty);
				if (!string.IsNullOrEmpty(text) && !((WeakGameEntity)(ref parent)).HasTag(text))
				{
					return true;
				}
				foreach (StandingPoint item in (List<StandingPoint>)(object)((WeakGameEntity)(ref parent)).GetFirstScriptOfType<UsableMachine>().StandingPoints)
				{
					if (item is AnimationPoint animationPoint && GroupId == animationPoint.GroupId && !((UsableMissionObject)animationPoint).IsDeactivated && (((UsableMissionObject)animationPoint).HasUser || (((UsableMissionObject)animationPoint).HasAIMovingTo && !((UsableMissionObject)animationPoint).IsAIMovingTo(agent))))
					{
						gameEntity = ((ScriptComponentBehavior)animationPoint).GameEntity;
						if (((WeakGameEntity)(ref gameEntity)).HasTag("alternative"))
						{
							return true;
						}
					}
				}
				return false;
			}
		}
		return ((StandingPoint)this).IsDisabledForAgent(agent);
	}

	public override void SimulateTick(float dt)
	{
		Tick(dt, isSimulation: true);
	}

	public override bool HasAlternative()
	{
		return GroupId >= 0;
	}

	protected override void OnInit()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		((StandingPoint)this).OnInit();
		_itemsForBones = new List<AnimationPoint.ItemForBone>();
		SetActionCodes();
		InitParameters();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	protected override void OnEditorInit()
	{
		_itemsForBones = new List<AnimationPoint.ItemForBone>();
		SetActionCodes();
		InitParameters();
	}

	public override void OnUserConversationStart()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_pointRotation = ((UsableMissionObject)this).UserAgent.Frame.rotation.f;
		((Vec3)(ref _pointRotation)).Normalize();
		if (KeepOldVisibility)
		{
			return;
		}
		foreach (AnimationPoint.ItemForBone itemsForBone in _itemsForBones)
		{
			itemsForBone.OldVisibility = itemsForBone.IsVisible;
		}
		SetAgentItemsVisibility(isVisible: false);
	}

	public override void OnUserConversationEnd()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((UsableMissionObject)this).UserAgent.ResetLookAgent();
		((UsableMissionObject)this).UserAgent.LookDirection = _pointRotation;
		((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref LoopStartActionCode, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		foreach (AnimationPoint.ItemForBone itemsForBone in _itemsForBones)
		{
			if (itemsForBone.OldVisibility)
			{
				SetAgentItemVisibility(itemsForBone, isVisible: true);
			}
		}
	}

	public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		SetAgentItemsVisibility(isVisible: false);
		RevertWeaponWieldSheathState();
		if (((UsableMissionObject)this).UserAgent.IsActive())
		{
			if (LeaveActionCode == ActionIndexCache.act_none)
			{
				((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref LeaveActionCode, false, (AnimFlags)Math.Min(((UsableMissionObject)this).UserAgent.GetCurrentActionPriority(0), 73), 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			}
			else if (IsArriveActionFinished)
			{
				ActionIndexCache currentAction = ((UsableMissionObject)this).UserAgent.GetCurrentAction(0);
				if (currentAction != LeaveActionCode)
				{
					MBActionSet actionSet = ((UsableMissionObject)this).UserAgent.ActionSet;
					if (!((MBActionSet)(ref actionSet)).AreActionsAlternatives(ref currentAction, ref LeaveActionCode))
					{
						AnimFlags val = (AnimFlags)Math.Min(((UsableMissionObject)this).UserAgent.GetCurrentActionPriority(0), ((UsableMissionObject)this).UserAgent.IsSitting() ? 94 : 73);
						((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref LeaveActionCode, false, val, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
					}
				}
			}
			else
			{
				ActionIndexCache currentAction2 = userAgent.GetCurrentAction(0);
				if (currentAction2 == ArriveActionCode && ArriveActionCode != ActionIndexCache.act_none)
				{
					MBActionSet actionSet2 = userAgent.ActionSet;
					float currentActionProgress = userAgent.GetCurrentActionProgress(0);
					float actionBlendOutStartProgress = MBActionSet.GetActionBlendOutStartProgress(actionSet2, ref currentAction2);
					if (currentActionProgress < actionBlendOutStartProgress)
					{
						float num = (actionBlendOutStartProgress - currentActionProgress) / actionBlendOutStartProgress;
						MBActionSet.GetActionBlendOutStartProgress(actionSet2, ref LeaveActionCode);
					}
				}
			}
		}
		_lastAction = ActionIndexCache.act_none;
		if (((UsableMissionObject)this).UserAgent.GetLookAgent() != null)
		{
			((UsableMissionObject)this).UserAgent.ResetLookAgent();
		}
		IsArriveActionFinished = false;
		((StandingPoint)this).OnUseStopped(userAgent, isSuccessful, preferenceIndex);
	}

	private void RevertWeaponWieldSheathState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)_equipmentIndexMainHand != -1 && base.AutoSheathWeapons)
		{
			((UsableMissionObject)this).UserAgent.TryToWieldWeaponInSlot(_equipmentIndexMainHand, (WeaponWieldActionType)0, false);
		}
		else if ((int)_equipmentIndexMainHand == -1 && base.AutoWieldWeapons)
		{
			((UsableMissionObject)this).UserAgent.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)0);
		}
		if ((int)_equipmentIndexOffHand != -1 && base.AutoSheathWeapons)
		{
			((UsableMissionObject)this).UserAgent.TryToWieldWeaponInSlot(_equipmentIndexOffHand, (WeaponWieldActionType)0, false);
		}
		else if ((int)_equipmentIndexOffHand == -1 && base.AutoWieldWeapons)
		{
			((UsableMissionObject)this).UserAgent.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)0);
		}
	}

	public void SetAgentItemsVisibility(bool isVisible)
	{
		if (((UsableMissionObject)this).UserAgent.IsMainAgent)
		{
			return;
		}
		foreach (AnimationPoint.ItemForBone itemsForBone in _itemsForBones)
		{
			SetAgentItemVisibility(itemsForBone, isVisible);
		}
	}

	private void SetAgentItemVisibility(AnimationPoint.ItemForBone item, bool isVisible)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		sbyte realBoneIndex = ((UsableMissionObject)this).UserAgent.AgentVisuals.GetRealBoneIndex(item.HumanBone);
		((UsableMissionObject)this).UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SetPrefabVisibility(realBoneIndex, item.ItemPrefabName, isVisible);
		item.IsVisible = isVisible;
	}

	private void Tick(float dt, bool isSimulation = false)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		if (!((UsableMissionObject)this).HasUser)
		{
			return;
		}
		Vec2 val;
		if (Game.Current != null && Game.Current.IsDevelopmentMode)
		{
			val = ((UsableMissionObject)this).UserAgent.GetTargetPosition();
			((Vec2)(ref val)).IsNonZero();
		}
		ActionIndexCache currentAction = ((UsableMissionObject)this).UserAgent.GetCurrentAction(0);
		switch (_state)
		{
		case State.NotUsing:
			if (!IsTargetReached())
			{
				break;
			}
			val = ((UsableMissionObject)this).UserAgent.MovementVelocity;
			if (((Vec2)(ref val)).LengthSquared < 0.1f && ((UsableMissionObject)this).UserAgent.IsAbleToUseMachine())
			{
				if (ArriveActionCode != ActionIndexCache.act_none)
				{
					Agent userAgent = ((UsableMissionObject)this).UserAgent;
					ref ActionIndexCache arriveActionCode = ref ArriveActionCode;
					long num = 0L;
					float num2 = (isSimulation ? 0f : (-0.2f));
					userAgent.SetActionChannel(0, ref arriveActionCode, false, (AnimFlags)num, 0f, MBRandom.RandomFloatRanged(0.8f, 1f), num2, 0.4f, 0f, false, -0.2f, 0, true);
				}
				_state = State.StartToUse;
			}
			break;
		case State.StartToUse:
		{
			if (ArriveActionCode != ActionIndexCache.act_none && isSimulation)
			{
				SimulateAnimations(0.1f);
			}
			if (!(ArriveActionCode == ActionIndexCache.act_none) && !(currentAction == ArriveActionCode))
			{
				MBActionSet actionSet = ((UsableMissionObject)this).UserAgent.ActionSet;
				if (!((MBActionSet)(ref actionSet)).AreActionsAlternatives(ref currentAction, ref ArriveActionCode))
				{
					break;
				}
			}
			((UsableMissionObject)this).UserAgent.ClearTargetFrame();
			WorldFrame userFrameForAgent = ((UsableMissionObject)this).GetUserFrameForAgent(((UsableMissionObject)this).UserAgent);
			_pointRotation = userFrameForAgent.Rotation.f;
			((Vec3)(ref _pointRotation)).Normalize();
			if (((UsableMissionObject)this).UserAgent != Agent.Main)
			{
				Agent userAgent2 = ((UsableMissionObject)this).UserAgent;
				ref WorldPosition origin = ref userFrameForAgent.Origin;
				val = ((Vec3)(ref userFrameForAgent.Rotation.f)).AsVec2;
				userAgent2.SetScriptedPositionAndDirection(ref origin, ((Vec2)(ref val)).RotationInRadians, false, (AIScriptedFrameFlags)16);
			}
			_state = State.Using;
			break;
		}
		case State.Using:
			if (isSimulation)
			{
				float dt2 = 0.1f;
				if (currentAction != ArriveActionCode)
				{
					dt2 = 0.01f + MBRandom.RandomFloat * 0.09f;
				}
				SimulateAnimations(dt2);
			}
			if (!IsArriveActionFinished && (ArriveActionCode == ActionIndexCache.act_none || ((UsableMissionObject)this).UserAgent.GetCurrentAction(0) != ArriveActionCode))
			{
				IsArriveActionFinished = true;
				AddItemsToAgent();
			}
			if (IsRotationCorrectDuringUsage())
			{
				((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref LoopStartActionCode, false, (AnimFlags)0, 0f, (ActionSpeed < 0.8f) ? ActionSpeed : MBRandom.RandomFloatRanged(0.8f, ActionSpeed), isSimulation ? 0f : (-0.2f), 0.4f, isSimulation ? MBRandom.RandomFloatRanged(0f, 0.5f) : 0f, false, -0.2f, 0, true);
			}
			break;
		}
	}

	private void SetActionCodes()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ArriveActionCode = ActionIndexCache.Create(ArriveAction);
		LoopStartActionCode = ActionIndexCache.Create(LoopStartAction);
		LeaveActionCode = ActionIndexCache.Create(LeaveAction);
		SelectedRightHandItem = RightHandItem;
		SelectedLeftHandItem = LeftHandItem;
	}

	private void InitParameters()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_pointRotation = Vec3.Zero;
		_state = State.NotUsing;
		((UsableMissionObject)this).LockUserPositions = true;
	}

	protected void AssignItemToBone(AnimationPoint.ItemForBone newItem)
	{
		if (!string.IsNullOrEmpty(newItem.ItemPrefabName) && !_itemsForBones.Contains(newItem))
		{
			_itemsForBones.Add(newItem);
		}
	}

	public bool IsRotationCorrectDuringUsage()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!((Vec3)(ref _pointRotation)).IsNonZero)
		{
			return false;
		}
		return Vec2.DotProduct(((Vec3)(ref _pointRotation)).AsVec2, ((UsableMissionObject)this).UserAgent.GetMovementDirection()) > 0.99f;
	}

	protected bool CanAgentUseItem(Agent agent)
	{
		if (agent.GetComponent<CampaignAgentComponent>() != null)
		{
			return agent.GetComponent<CampaignAgentComponent>().AgentNavigator != null;
		}
		return false;
	}

	protected void AddItemsToAgent()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (!CanAgentUseItem(((UsableMissionObject)this).UserAgent) || !IsArriveActionFinished)
		{
			return;
		}
		if (_itemsForBones.Count != 0)
		{
			((UsableMissionObject)this).UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.HoldAndHideRecentlyUsedMeshes();
		}
		MissionWeapon val4 = default(MissionWeapon);
		foreach (AnimationPoint.ItemForBone itemsForBone in _itemsForBones)
		{
			ItemObject val = Game.Current.ObjectManager.GetObject<ItemObject>(itemsForBone.ItemPrefabName);
			if (val != null)
			{
				EquipmentIndex val2 = FindProperSlot(val);
				MissionWeapon val3 = ((UsableMissionObject)this).UserAgent.Equipment[val2];
				if (!((MissionWeapon)(ref val3)).IsEmpty)
				{
					((UsableMissionObject)this).UserAgent.DropItem(val2, (WeaponClass)0);
				}
				IAgentOriginBase origin = ((UsableMissionObject)this).UserAgent.Origin;
				((MissionWeapon)(ref val4))._002Ector(val, (ItemModifier)null, (origin != null) ? origin.Banner : null);
				((UsableMissionObject)this).UserAgent.EquipWeaponWithNewEntity(val2, ref val4);
				((UsableMissionObject)this).UserAgent.TryToWieldWeaponInSlot(val2, (WeaponWieldActionType)1, false);
			}
			else
			{
				sbyte realBoneIndex = ((UsableMissionObject)this).UserAgent.AgentVisuals.GetRealBoneIndex(itemsForBone.HumanBone);
				((UsableMissionObject)this).UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SetPrefabVisibility(realBoneIndex, itemsForBone.ItemPrefabName, isVisible: true);
			}
		}
	}

	private EquipmentIndex FindProperSlot(ItemObject item)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		EquipmentIndex result = (EquipmentIndex)3;
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val <= 3; val = (EquipmentIndex)(val + 1))
		{
			MissionWeapon val2 = ((UsableMissionObject)this).UserAgent.Equipment[val];
			if (((MissionWeapon)(ref val2)).IsEmpty)
			{
				result = val;
				continue;
			}
			val2 = ((UsableMissionObject)this).UserAgent.Equipment[val];
			if (((MissionWeapon)(ref val2)).Item == item)
			{
				return val;
			}
		}
		return result;
	}

	private void SimulateAnimations(float dt)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		((UsableMissionObject)this).UserAgent.TickActionChannels(dt);
		Vec3 val = ((UsableMissionObject)this).UserAgent.ComputeAnimationDisplacement(dt);
		if (((Vec3)(ref val)).LengthSquared > 0f)
		{
			((UsableMissionObject)this).UserAgent.TeleportToPosition(((UsableMissionObject)this).UserAgent.Position + val);
		}
		((UsableMissionObject)this).UserAgent.AgentVisuals.GetSkeleton().TickAnimations(dt, ((UsableMissionObject)this).UserAgent.AgentVisuals.GetGlobalFrame(), true);
	}

	private bool IsTargetReached()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vec3 targetDirection = ((UsableMissionObject)this).UserAgent.GetTargetDirection();
		float num = Vec2.DotProduct(((Vec3)(ref targetDirection)).AsVec2, ((UsableMissionObject)this).UserAgent.GetMovementDirection());
		Vec3 position = ((UsableMissionObject)this).UserAgent.Position;
		Vec2 val = ((Vec3)(ref position)).AsVec2 - ((UsableMissionObject)this).UserAgent.GetTargetPosition();
		if (((Vec2)(ref val)).LengthSquared < 0.040000003f)
		{
			return num > 0.99f;
		}
		return false;
	}
}
