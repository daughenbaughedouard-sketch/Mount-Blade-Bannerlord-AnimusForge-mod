using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.AnimationPoints;

public class ChairUsePoint : AnimationPoint
{
	private enum ChairAction
	{
		None,
		LeanOnTable,
		Drink,
		Eat
	}

	public bool NearTable;

	public string NearTableLoopAction = "";

	public string NearTablePairLoopAction = "";

	public bool Drink;

	public string DrinkLoopAction = "";

	public string DrinkPairLoopAction = "";

	public string DrinkRightHandItem = "";

	public string DrinkLeftHandItem = "";

	public bool Eat;

	public string EatLoopAction = "";

	public string EatPairLoopAction = "";

	public string EatRightHandItem = "";

	public string EatLeftHandItem = "";

	private ActionIndexCache _loopAction;

	private ActionIndexCache _pairLoopAction;

	private ActionIndexCache _nearTableLoopAction;

	private ActionIndexCache _nearTablePairLoopAction;

	private ActionIndexCache _drinkLoopAction;

	private ActionIndexCache _drinkPairLoopAction;

	private ActionIndexCache _eatLoopAction;

	private ActionIndexCache _eatPairLoopAction;

	protected override void SetActionCodes()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		base.SetActionCodes();
		_loopAction = ActionIndexCache.Create(LoopStartAction);
		_pairLoopAction = ActionIndexCache.Create(PairLoopStartAction);
		_nearTableLoopAction = ActionIndexCache.Create(NearTableLoopAction);
		_nearTablePairLoopAction = ActionIndexCache.Create(NearTablePairLoopAction);
		_drinkLoopAction = ActionIndexCache.Create(DrinkLoopAction);
		_drinkPairLoopAction = ActionIndexCache.Create(DrinkPairLoopAction);
		_eatLoopAction = ActionIndexCache.Create(EatLoopAction);
		_eatPairLoopAction = ActionIndexCache.Create(EatPairLoopAction);
		SetChairAction(GetRandomChairAction());
	}

	protected override bool ShouldUpdateOnEditorVariableChanged(string variableName)
	{
		if (!base.ShouldUpdateOnEditorVariableChanged(variableName))
		{
			switch (variableName)
			{
			default:
				return variableName == "EatLoopAction";
			case "NearTable":
			case "Drink":
			case "Eat":
			case "NearTableLoopAction":
			case "DrinkLoopAction":
				break;
			}
		}
		return true;
	}

	public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
	{
		ChairAction chairAction = (CanAgentUseItem(userAgent) ? GetRandomChairAction() : ChairAction.None);
		SetChairAction(chairAction);
		base.OnUse(userAgent, agentBoneIndex);
	}

	private ChairAction GetRandomChairAction()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		List<ChairAction> list = new List<ChairAction> { ChairAction.None };
		if (NearTable && _nearTableLoopAction != ActionIndexCache.act_none)
		{
			list.Add(ChairAction.LeanOnTable);
		}
		if (Drink && _drinkLoopAction != ActionIndexCache.act_none)
		{
			list.Add(ChairAction.Drink);
		}
		if (Eat && _eatLoopAction != ActionIndexCache.act_none)
		{
			list.Add(ChairAction.Eat);
		}
		return list[new Random().Next(list.Count)];
	}

	private void SetChairAction(ChairAction chairAction)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		switch (chairAction)
		{
		case ChairAction.None:
			LoopStartActionCode = _loopAction;
			PairLoopStartActionCode = _pairLoopAction;
			base.SelectedRightHandItem = RightHandItem;
			base.SelectedLeftHandItem = LeftHandItem;
			break;
		case ChairAction.LeanOnTable:
			LoopStartActionCode = _nearTableLoopAction;
			PairLoopStartActionCode = _nearTablePairLoopAction;
			base.SelectedRightHandItem = string.Empty;
			base.SelectedLeftHandItem = string.Empty;
			break;
		case ChairAction.Drink:
			LoopStartActionCode = _drinkLoopAction;
			PairLoopStartActionCode = _drinkPairLoopAction;
			base.SelectedRightHandItem = DrinkRightHandItem;
			base.SelectedLeftHandItem = DrinkLeftHandItem;
			break;
		case ChairAction.Eat:
			LoopStartActionCode = _eatLoopAction;
			PairLoopStartActionCode = _eatPairLoopAction;
			base.SelectedRightHandItem = EatRightHandItem;
			base.SelectedLeftHandItem = EatLeftHandItem;
			break;
		}
	}

	protected override void OnTick(float dt)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.OnTick(dt);
		if (((UsableMissionObject)this).UserAgent != null && !((UsableMissionObject)this).UserAgent.IsAIControlled && Extensions.HasAnyFlag<EventControlFlag>(((UsableMissionObject)this).UserAgent.EventControlFlags, (EventControlFlag)24576))
		{
			((UsableMissionObject)this).UserAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
	}
}
