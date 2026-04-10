using System;
using System.Collections.Generic;
using SandBox.Conversation;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.Objects.AnimationPoints;

public class AnimationPoint : StandingPoint
{
	public class ItemForBone
	{
		public HumanBone HumanBone;

		public string ItemPrefabName;

		public bool IsVisible;

		public bool OldVisibility;

		public ItemForBone(HumanBone bone, string name, bool isVisible)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			HumanBone = bone;
			ItemPrefabName = name;
			IsVisible = isVisible;
			OldVisibility = isVisible;
		}
	}

	private enum State
	{
		NotUsing,
		StartToUse,
		Using
	}

	private enum PairState
	{
		NoPair,
		BecomePair,
		Greeting,
		StartPairAnimation,
		Pair
	}

	private const string AlternativeTag = "alternative";

	private const float RangeThreshold = 0.2f;

	private const float RotationScoreThreshold = 0.99f;

	private const float ActionSpeedRandomMinValue = 0.8f;

	private const float AnimationRandomProgressMaxValue = 0.5f;

	private static readonly ActionIndexCache[] _greetingFrontActions = (ActionIndexCache[])(object)new ActionIndexCache[4]
	{
		ActionIndexCache.act_greeting_front_1,
		ActionIndexCache.act_greeting_front_2,
		ActionIndexCache.act_greeting_front_3,
		ActionIndexCache.act_greeting_front_4
	};

	private static readonly ActionIndexCache[] _greetingRightActions = (ActionIndexCache[])(object)new ActionIndexCache[4]
	{
		ActionIndexCache.act_greeting_right_1,
		ActionIndexCache.act_greeting_right_2,
		ActionIndexCache.act_greeting_right_3,
		ActionIndexCache.act_greeting_right_4
	};

	private static readonly ActionIndexCache[] _greetingLeftActions = (ActionIndexCache[])(object)new ActionIndexCache[4]
	{
		ActionIndexCache.act_greeting_left_1,
		ActionIndexCache.act_greeting_left_2,
		ActionIndexCache.act_greeting_left_3,
		ActionIndexCache.act_greeting_left_4
	};

	public string ArriveAction = "";

	public string LoopStartAction = "";

	public string PairLoopStartAction = "";

	public string LeaveAction = "";

	public int GroupId = -1;

	public string RightHandItem = "";

	public HumanBone RightHandItemBone = (HumanBone)27;

	public string LeftHandItem = "";

	public HumanBone LeftHandItemBone = (HumanBone)20;

	public GameEntity PairEntity;

	public int MinUserToStartInteraction = 1;

	public bool ActivatePairs;

	public float MinWaitinSeconds = 30f;

	public float MaxWaitInSeconds = 120f;

	public float ForwardDistanceToPivotPoint;

	public float SideDistanceToPivotPoint;

	private bool _startPairAnimationWithGreeting;

	protected float ActionSpeed = 1f;

	public bool KeepOldVisibility;

	private ActionIndexCache ArriveActionCode;

	protected ActionIndexCache LoopStartActionCode;

	protected ActionIndexCache PairLoopStartActionCode;

	private ActionIndexCache LeaveActionCode;

	protected ActionIndexCache DefaultActionCode;

	private bool _resyncAnimations;

	private string _selectedRightHandItem;

	private string _selectedLeftHandItem;

	private State _state;

	private PairState _pairState;

	private Vec3 _pointRotation;

	private List<AnimationPoint> _pairPoints;

	private List<ItemForBone> _itemsForBones;

	private ActionIndexCache _lastAction;

	private Timer _greetingTimer;

	private GameEntity _animatedEntity;

	private Vec3 _animatedEntityDisplacement = Vec3.Zero;

	private EquipmentIndex _equipmentIndexMainHand;

	private EquipmentIndex _equipmentIndexOffHand;

	public override bool PlayerStopsUsingWhenInteractsWithOther => false;

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
				ItemForBone newItem = new ItemForBone(RightHandItemBone, value, isVisible: false);
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
				ItemForBone newItem = new ItemForBone(LeftHandItemBone, value, isVisible: false);
				AssignItemToBone(newItem);
				_selectedLeftHandItem = value;
			}
		}
	}

	public bool IsActive { get; private set; } = true;

	public override bool DisableCombatActionsOnUse => !((UsableMissionObject)this).IsInstantUse;

	public AnimationPoint()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		_greetingTimer = null;
	}

	private void CreateVisualizer()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (!(PairLoopStartActionCode != ActionIndexCache.act_none) && !(LoopStartActionCode != ActionIndexCache.act_none))
		{
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_animatedEntity = GameEntity.CreateEmpty(((WeakGameEntity)(ref gameEntity)).Scene, false, true, true);
		GameEntity animatedEntity = _animatedEntity;
		animatedEntity.EntityFlags = (EntityFlags)(animatedEntity.EntityFlags | 0x20000);
		_animatedEntity.Name = "ap_visual_entity";
		MBActionSet val = MBActionSet.GetActionSetWithIndex(0);
		ActionIndexCache val2 = ActionIndexCache.act_none;
		int numberOfActionSets = MBActionSet.GetNumberOfActionSets();
		for (int i = 0; i < numberOfActionSets; i++)
		{
			MBActionSet actionSetWithIndex = MBActionSet.GetActionSetWithIndex(i);
			if (ArriveActionCode == ActionIndexCache.act_none || MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, ref ArriveActionCode))
			{
				if (PairLoopStartActionCode != ActionIndexCache.act_none && MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, ref PairLoopStartActionCode))
				{
					val = actionSetWithIndex;
					val2 = PairLoopStartActionCode;
					break;
				}
				if (LoopStartActionCode != ActionIndexCache.act_none && MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, ref LoopStartActionCode))
				{
					val = actionSetWithIndex;
					val2 = LoopStartActionCode;
					break;
				}
			}
		}
		if (val2 == ActionIndexCache.act_none)
		{
			val2 = ActionIndexCache.act_jump_loop;
		}
		GameEntityExtensions.CreateAgentSkeleton(_animatedEntity, "human_skeleton", true, val, "human", MBObjectManager.Instance.GetObject<Monster>("human"));
		MBSkeletonExtensions.SetAgentActionChannel(_animatedEntity.Skeleton, 0, ref val2, 0f, -0.2f, true, 0f);
		_animatedEntity.AddMultiMeshToSkeleton(MetaMesh.GetCopy("roman_cloth_tunic_a", true, false));
		_animatedEntity.AddMultiMeshToSkeleton(MetaMesh.GetCopy("casual_02_boots", true, false));
		_animatedEntity.AddMultiMeshToSkeleton(MetaMesh.GetCopy("hands_male_a", true, false));
		_animatedEntity.AddMultiMeshToSkeleton(MetaMesh.GetCopy("head_male_a", true, false));
		_animatedEntityDisplacement = Vec3.Zero;
		if (ArriveActionCode != ActionIndexCache.act_none && (int)(MBActionSet.GetActionAnimationFlags(val, ref ArriveActionCode) & 0x400000000000L) != 0)
		{
			_animatedEntityDisplacement = MBActionSet.GetActionDisplacementVector(val, ref ArriveActionCode);
		}
		UpdateAnimatedEntityFrame();
	}

	private void UpdateAnimatedEntityFrame()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Mat3 identity = Mat3.Identity;
		MatrixFrame val = new MatrixFrame(ref identity, ref _animatedEntityDisplacement);
		val = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		globalFrame.origin = val.origin;
		_animatedEntity.SetFrame(ref globalFrame, true);
	}

	protected override void OnEditModeVisibilityChanged(bool currentVisibility)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (_animatedEntity != (GameEntity)null)
		{
			_animatedEntity.SetVisibilityExcludeParents(currentVisibility);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (!((WeakGameEntity)(ref gameEntity)).IsGhostObject())
			{
				_resyncAnimations = true;
			}
		}
	}

	protected override void OnEditorTick(float dt)
	{
		if (_animatedEntity != (GameEntity)null)
		{
			if (_resyncAnimations)
			{
				ResetAnimations();
				_resyncAnimations = false;
			}
			bool flag = _animatedEntity.IsVisibleIncludeParents();
			if (flag && !MBEditor.HelpersEnabled())
			{
				_animatedEntity.SetVisibilityExcludeParents(false);
				flag = false;
			}
			if (flag)
			{
				UpdateAnimatedEntityFrame();
			}
		}
	}

	protected override void OnEditorInit()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_itemsForBones = new List<ItemForBone>();
		SetActionCodes();
		InitParameters();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (!((WeakGameEntity)(ref gameEntity)).IsGhostObject())
		{
			CreateVisualizer();
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		((UsableMissionObject)this).OnRemoved(removeReason);
		if (_animatedEntity != (GameEntity)null)
		{
			Scene scene = _animatedEntity.Scene;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if ((NativeObject)(object)scene == (NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Scene)
			{
				_animatedEntity.Remove(removeReason);
				_animatedEntity = null;
			}
		}
		PairEntity = null;
	}

	protected void ResetAnimations()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		ActionIndexCache val = ActionIndexCache.act_none;
		int numberOfActionSets = MBActionSet.GetNumberOfActionSets();
		for (int i = 0; i < numberOfActionSets; i++)
		{
			MBActionSet actionSetWithIndex = MBActionSet.GetActionSetWithIndex(i);
			if (ArriveActionCode == ActionIndexCache.act_none || MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, ref ArriveActionCode))
			{
				if (PairLoopStartActionCode != ActionIndexCache.act_none && MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, ref PairLoopStartActionCode))
				{
					val = PairLoopStartActionCode;
					break;
				}
				if (LoopStartActionCode != ActionIndexCache.act_none && MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, ref LoopStartActionCode))
				{
					val = LoopStartActionCode;
					break;
				}
			}
		}
		if (val != ActionIndexCache.act_none)
		{
			MBSkeletonExtensions.SetAgentActionChannel(_animatedEntity.Skeleton, 0, ref ActionIndexCache.act_jump_loop, 0f, -0.2f, true, 0f);
			MBSkeletonExtensions.SetAgentActionChannel(_animatedEntity.Skeleton, 0, ref val, 0f, -0.2f, true, 0f);
		}
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		if (ShouldUpdateOnEditorVariableChanged(variableName))
		{
			if (_animatedEntity != (GameEntity)null)
			{
				_animatedEntity.Remove(91);
			}
			SetActionCodes();
			CreateVisualizer();
		}
	}

	public void RequestResync()
	{
		_resyncAnimations = true;
	}

	public override void AfterMissionStart()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (Agent.Main != null && LoopStartActionCode != ActionIndexCache.act_none && !MBActionSet.CheckActionAnimationClipExists(Agent.Main.ActionSet, ref LoopStartActionCode))
		{
			((UsableMissionObject)this).IsDisabledForPlayers = true;
		}
	}

	protected virtual bool ShouldUpdateOnEditorVariableChanged(string variableName)
	{
		if (!(variableName == "ArriveAction") && !(variableName == "LoopStartAction"))
		{
			return variableName == "PairLoopStartAction";
		}
		return true;
	}

	protected void ClearAssignedItems()
	{
		SetAgentItemsVisibility(isVisible: false);
		_itemsForBones.Clear();
	}

	protected void AssignItemToBone(ItemForBone newItem)
	{
		if (!string.IsNullOrEmpty(newItem.ItemPrefabName) && !_itemsForBones.Contains(newItem))
		{
			_itemsForBones.Add(newItem);
		}
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

	protected override void OnInit()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		((StandingPoint)this).OnInit();
		_itemsForBones = new List<ItemForBone>();
		SetActionCodes();
		InitParameters();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	private void InitParameters()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_greetingTimer = null;
		_pointRotation = Vec3.Zero;
		_state = State.NotUsing;
		_pairPoints = GetPairs(PairEntity);
		if (ActivatePairs)
		{
			SetPairsActivity(isActive: false);
		}
		((UsableMissionObject)this).LockUserPositions = true;
	}

	protected virtual void SetActionCodes()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		ArriveActionCode = ActionIndexCache.Create(ArriveAction);
		LoopStartActionCode = ActionIndexCache.Create(LoopStartAction);
		PairLoopStartActionCode = ActionIndexCache.Create(PairLoopStartAction);
		LeaveActionCode = ActionIndexCache.Create(LeaveAction);
		SelectedRightHandItem = RightHandItem;
		SelectedLeftHandItem = LeftHandItem;
	}

	protected override bool DoesActionTypeStopUsingGameObject(ActionCodeType actionType)
	{
		return false;
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

	protected override void OnTick(float dt)
	{
		((StandingPoint)this).OnTick(dt);
		Tick(dt);
	}

	private List<AnimationPoint> GetPairs(GameEntity entity)
	{
		List<AnimationPoint> list = new List<AnimationPoint>();
		if (entity != (GameEntity)null)
		{
			if (entity.HasScriptOfType<AnimationPoint>())
			{
				AnimationPoint firstScriptOfType = entity.GetFirstScriptOfType<AnimationPoint>();
				list.Add(firstScriptOfType);
			}
			else
			{
				foreach (GameEntity child in entity.GetChildren())
				{
					list.AddRange(GetPairs(child));
				}
			}
		}
		if (list.Contains(this))
		{
			list.Remove(this);
		}
		return list;
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

	private void Tick(float dt, bool isSimulation = false)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
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
			if (!(((Vec2)(ref val)).LengthSquared < 0.1f) || !((UsableMissionObject)this).UserAgent.IsAbleToUseMachine())
			{
				break;
			}
			if (ArriveActionCode != ActionIndexCache.act_none)
			{
				Agent userAgent = ((UsableMissionObject)this).UserAgent;
				ref ActionIndexCache arriveActionCode = ref ArriveActionCode;
				long num = 0L;
				float num2 = (isSimulation ? 0f : (-0.2f));
				userAgent.SetActionChannel(0, ref arriveActionCode, false, (AnimFlags)num, 0f, MBRandom.RandomFloatRanged(0.8f, 1f), num2, 0.4f, 0f, false, -0.2f, 0, true);
				if (Extensions.HasAnyFlag<AnimFlags>(((UsableMissionObject)this).UserAgent.GetCurrentAnimationFlag(0), (AnimFlags)70368744177664L))
				{
					((UsableMissionObject)this).UserAgent.ClearTargetFrame();
					((UsableMissionObject)this).LockUserFrames = false;
					((UsableMissionObject)this).LockUserPositions = false;
				}
			}
			_state = State.StartToUse;
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
			if (IsArriveActionFinished && ((UsableMissionObject)this).UserAgent != Agent.Main)
			{
				PairTick(isSimulation);
			}
			break;
		}
	}

	private void PairTick(bool isSimulation)
	{
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		MBList<Agent> pairEntityUsers = GetPairEntityUsers();
		if (PairEntity != (GameEntity)null)
		{
			bool agentItemsVisibility = ((UsableMissionObject)this).UserAgent != ConversationMission.OneToOneConversationAgent && ((List<Agent>)(object)pairEntityUsers).Count + 1 >= MinUserToStartInteraction;
			SetAgentItemsVisibility(agentItemsVisibility);
		}
		if (_pairState != PairState.NoPair && ((List<Agent>)(object)pairEntityUsers).Count < MinUserToStartInteraction)
		{
			_pairState = PairState.NoPair;
			if (((UsableMissionObject)this).UserAgent != ConversationMission.OneToOneConversationAgent)
			{
				((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref _lastAction, false, (AnimFlags)Math.Min(((UsableMissionObject)this).UserAgent.GetCurrentActionPriority(0), 73), 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				((UsableMissionObject)this).UserAgent.ResetLookAgent();
			}
			_greetingTimer = null;
		}
		else if (_pairState == PairState.NoPair && ((List<Agent>)(object)pairEntityUsers).Count >= MinUserToStartInteraction && IsRotationCorrectDuringUsage())
		{
			_lastAction = ((UsableMissionObject)this).UserAgent.GetCurrentAction(0);
			if (_startPairAnimationWithGreeting)
			{
				_pairState = PairState.BecomePair;
				_greetingTimer = new Timer(Mission.Current.CurrentTime, (float)MBRandom.RandomInt(5) * 0.3f, true);
			}
			else
			{
				_pairState = PairState.StartPairAnimation;
			}
		}
		else if (_pairState == PairState.BecomePair && _greetingTimer.Check(Mission.Current.CurrentTime))
		{
			_greetingTimer = null;
			_pairState = PairState.Greeting;
			Vec3 eyeGlobalPosition = Extensions.GetRandomElement<Agent>(pairEntityUsers).GetEyeGlobalPosition();
			Vec3 eyeGlobalPosition2 = ((UsableMissionObject)this).UserAgent.GetEyeGlobalPosition();
			Vec3 val = eyeGlobalPosition - eyeGlobalPosition2;
			((Vec3)(ref val)).Normalize();
			Mat3 rotation = ((UsableMissionObject)this).UserAgent.Frame.rotation;
			if (Vec3.DotProduct(rotation.f, val) > 0f)
			{
				ActionIndexCache greetingActionId = GetGreetingActionId(eyeGlobalPosition2, eyeGlobalPosition, rotation);
				((UsableMissionObject)this).UserAgent.SetActionChannel(1, ref greetingActionId, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			}
		}
		else if (_pairState == PairState.Greeting && ((UsableMissionObject)this).UserAgent.GetCurrentAction(1) == ActionIndexCache.act_none)
		{
			_pairState = PairState.StartPairAnimation;
		}
		if (_pairState == PairState.StartPairAnimation)
		{
			_pairState = PairState.Pair;
			((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref PairLoopStartActionCode, false, (AnimFlags)0, 0f, MBRandom.RandomFloatRanged(0.8f, ActionSpeed), isSimulation ? 0f : (-0.2f), 0.4f, isSimulation ? MBRandom.RandomFloatRanged(0f, 0.5f) : 0f, false, -0.2f, 0, true);
		}
		if (_pairState == PairState.Pair && IsRotationCorrectDuringUsage())
		{
			((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref PairLoopStartActionCode, false, (AnimFlags)0, 0f, MBRandom.RandomFloatRanged(0.8f, ActionSpeed), isSimulation ? 0f : (-0.2f), 0.4f, isSimulation ? MBRandom.RandomFloatRanged(0f, 0.5f) : 0f, false, -0.2f, 0, true);
		}
	}

	private ActionIndexCache GetGreetingActionId(Vec3 userAgentGlobalEyePoint, Vec3 lookTarget, Mat3 userAgentRot)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = lookTarget - userAgentGlobalEyePoint;
		((Vec3)(ref val)).Normalize();
		float num = Vec3.DotProduct(userAgentRot.f, val);
		if (num > 0.8f)
		{
			return _greetingFrontActions[MBRandom.RandomInt(_greetingFrontActions.Length)];
		}
		if (num > 0f)
		{
			if (!(Vec3.DotProduct(Vec3.CrossProduct(val, userAgentRot.f), userAgentRot.u) > 0f))
			{
				return _greetingLeftActions[MBRandom.RandomInt(_greetingLeftActions.Length)];
			}
			return _greetingRightActions[MBRandom.RandomInt(_greetingRightActions.Length)];
		}
		return ActionIndexCache.act_none;
	}

	private MBList<Agent> GetPairEntityUsers()
	{
		MBList<Agent> val = new MBList<Agent>();
		if (((UsableMissionObject)this).UserAgent != ConversationMission.OneToOneConversationAgent)
		{
			foreach (AnimationPoint pairPoint in _pairPoints)
			{
				if (((UsableMissionObject)pairPoint).HasUser && pairPoint._state == State.Using && ((UsableMissionObject)pairPoint).UserAgent != ConversationMission.OneToOneConversationAgent)
				{
					((List<Agent>)(object)val).Add(((UsableMissionObject)pairPoint).UserAgent);
				}
			}
		}
		return val;
	}

	private void SetPairsActivity(bool isActive)
	{
		foreach (AnimationPoint pairPoint in _pairPoints)
		{
			pairPoint.IsActive = isActive;
			if (!isActive)
			{
				if (((UsableMissionObject)pairPoint).HasAIUser)
				{
					((UsableMissionObject)pairPoint).UserAgent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
				}
				Agent movingAgent = ((UsableMissionObject)pairPoint).MovingAgent;
				if (movingAgent != null)
				{
					movingAgent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
				}
			}
		}
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
		if (ActivatePairs)
		{
			SetPairsActivity(isActive: true);
		}
	}

	private void RevertWeaponWieldSheathState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected I4, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Invalid comparison between Unknown and I4
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected I4, but got Unknown
		if ((int)_equipmentIndexMainHand != -1 && base.AutoSheathWeapons)
		{
			((UsableMissionObject)this).UserAgent.Mission.AddTickActionMT((MissionTickAction)2, ((UsableMissionObject)this).UserAgent, (int)_equipmentIndexMainHand, 0);
		}
		else if ((int)_equipmentIndexMainHand == -1 && base.AutoWieldWeapons)
		{
			((UsableMissionObject)this).UserAgent.Mission.AddTickActionMT((MissionTickAction)0, ((UsableMissionObject)this).UserAgent, 0, 0);
		}
		if ((int)_equipmentIndexOffHand != -1 && base.AutoSheathWeapons)
		{
			((UsableMissionObject)this).UserAgent.Mission.AddTickActionMT((MissionTickAction)2, ((UsableMissionObject)this).UserAgent, (int)_equipmentIndexOffHand, 0);
		}
		else if ((int)_equipmentIndexOffHand == -1 && base.AutoWieldWeapons)
		{
			((UsableMissionObject)this).UserAgent.Mission.AddTickActionMT((MissionTickAction)0, ((UsableMissionObject)this).UserAgent, 1, 0);
		}
	}

	public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
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
					float num = 0f;
					MBActionSet actionSet2 = userAgent.ActionSet;
					float currentActionProgress = userAgent.GetCurrentActionProgress(0);
					float actionBlendOutStartProgress = MBActionSet.GetActionBlendOutStartProgress(actionSet2, ref currentAction2);
					if (currentActionProgress < actionBlendOutStartProgress)
					{
						float num2 = (actionBlendOutStartProgress - currentActionProgress) / actionBlendOutStartProgress;
						num = MBActionSet.GetActionBlendOutStartProgress(actionSet2, ref LeaveActionCode) * num2;
					}
					((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref LeaveActionCode, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, num, false, -0.2f, 0, true);
				}
			}
		}
		_pairState = PairState.NoPair;
		_lastAction = ActionIndexCache.act_none;
		if (((UsableMissionObject)this).UserAgent.GetLookAgent() != null)
		{
			((UsableMissionObject)this).UserAgent.ResetLookAgent();
		}
		IsArriveActionFinished = false;
		((UsableMissionObject)this).LockUserFrames = true;
		((UsableMissionObject)this).LockUserPositions = true;
		((StandingPoint)this).OnUseStopped(userAgent, isSuccessful, preferenceIndex);
		if (ActivatePairs)
		{
			SetPairsActivity(isActive: false);
		}
	}

	public override void SimulateTick(float dt)
	{
		Tick(dt, isSimulation: true);
	}

	public override bool HasAlternative()
	{
		return GroupId >= 0;
	}

	public float GetRandomWaitInSeconds()
	{
		if (MinWaitinSeconds < 0f || MaxWaitInSeconds < 0f)
		{
			return -1f;
		}
		if (!(MathF.Abs(MinWaitinSeconds - MaxWaitInSeconds) < float.Epsilon))
		{
			return MinWaitinSeconds + MBRandom.RandomFloat * (MaxWaitInSeconds - MinWaitinSeconds);
		}
		return MinWaitinSeconds;
	}

	public List<AnimationPoint> GetAlternatives()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		List<AnimationPoint> list = new List<AnimationPoint>();
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Parent;
		IEnumerable<WeakGameEntity> children = ((WeakGameEntity)(ref val)).GetChildren();
		if (children != null)
		{
			foreach (WeakGameEntity item in children)
			{
				WeakGameEntity current = item;
				AnimationPoint firstScriptOfType = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<AnimationPoint>();
				if (firstScriptOfType != null && ((StandingPoint)firstScriptOfType).HasAlternative() && GroupId == firstScriptOfType.GroupId)
				{
					list.Add(firstScriptOfType);
				}
			}
		}
		return list;
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
		foreach (ItemForBone itemsForBone in _itemsForBones)
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
				SetAgentItemVisibility(itemsForBone, isVisible: true);
			}
		}
	}

	public override void OnUserConversationStart()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_pointRotation = ((UsableMissionObject)this).UserAgent.Frame.rotation.f;
		((Vec3)(ref _pointRotation)).Normalize();
		if (!KeepOldVisibility)
		{
			return;
		}
		foreach (ItemForBone itemsForBone in _itemsForBones)
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
		if (!KeepOldVisibility)
		{
			return;
		}
		foreach (ItemForBone itemsForBone in _itemsForBones)
		{
			if (itemsForBone.OldVisibility)
			{
				SetAgentItemVisibility(itemsForBone, isVisible: true);
			}
		}
	}

	public void SetAgentItemsVisibility(bool isVisible)
	{
		if (((UsableMissionObject)this).UserAgent.IsMainAgent)
		{
			return;
		}
		foreach (ItemForBone itemsForBone in _itemsForBones)
		{
			SetAgentItemVisibility(itemsForBone, isVisible);
		}
	}

	private void SetAgentItemVisibility(ItemForBone item, bool isVisible)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		sbyte realBoneIndex = ((UsableMissionObject)this).UserAgent.AgentVisuals.GetRealBoneIndex(item.HumanBone);
		((UsableMissionObject)this).UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SetPrefabVisibility(realBoneIndex, item.ItemPrefabName, isVisible);
		item.IsVisible = isVisible;
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
}
