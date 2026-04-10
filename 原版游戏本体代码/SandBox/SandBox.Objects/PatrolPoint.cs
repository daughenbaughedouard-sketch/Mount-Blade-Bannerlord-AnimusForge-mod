using System;
using System.Collections.Generic;
using SandBox.Objects.AnimationPoints;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects;

public class PatrolPoint : StandingPoint
{
	public readonly int WaitDuration;

	public readonly int WaitDeviation;

	public readonly int Index;

	public readonly string SpawnGroupTag;

	public readonly bool IsInfiniteWaitPoint;

	public readonly float PatrollingSpeed = -1f;

	public string LoopAction = "";

	private ActionIndexCache _loopAction;

	public string RightHandItem = "";

	public HumanBone RightHandItemBone = (HumanBone)27;

	public string LeftHandItem = "";

	public HumanBone LeftHandItemBone = (HumanBone)20;

	private List<AnimationPoint.ItemForBone> _itemsForBones;

	private string _selectedRightHandItem;

	private string _selectedLeftHandItem;

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

	protected void AssignItemToBone(AnimationPoint.ItemForBone newItem)
	{
		if (!string.IsNullOrEmpty(newItem.ItemPrefabName) && !_itemsForBones.Contains(newItem))
		{
			_itemsForBones.Add(newItem);
		}
	}

	public void SetAgentItemsVisibility(bool isVisible)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (((UsableMissionObject)this).UserAgent.IsMainAgent)
		{
			return;
		}
		foreach (AnimationPoint.ItemForBone itemsForBone in _itemsForBones)
		{
			sbyte realBoneIndex = ((UsableMissionObject)this).UserAgent.AgentVisuals.GetRealBoneIndex(itemsForBone.HumanBone);
			((UsableMissionObject)this).UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SetPrefabVisibility(realBoneIndex, itemsForBone.ItemPrefabName, isVisible);
			itemsForBone.IsVisible = isVisible;
		}
	}

	public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
	{
		((StandingPoint)this).OnUse(userAgent, agentBoneIndex);
		((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref _loopAction, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		SetAgentItemsVisibility(isVisible: true);
	}

	public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
		((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref ActionIndexCache.act_none, false, (AnimFlags)Math.Min(((UsableMissionObject)this).UserAgent.GetCurrentActionPriority(0), 73), 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		SetAgentItemsVisibility(isVisible: false);
		((StandingPoint)this).OnUseStopped(userAgent, isSuccessful, preferenceIndex);
	}

	protected override void OnInit()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((StandingPoint)this).OnInit();
		_itemsForBones = new List<AnimationPoint.ItemForBone>();
		_loopAction = ActionIndexCache.Create(LoopAction);
		SelectedRightHandItem = RightHandItem;
		SelectedLeftHandItem = LeftHandItem;
	}

	protected override void OnEditorTick(float dt)
	{
		((UsableMissionObject)this).OnEditorTick(dt);
		_itemsForBones = new List<AnimationPoint.ItemForBone>();
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		return null;
	}
}
