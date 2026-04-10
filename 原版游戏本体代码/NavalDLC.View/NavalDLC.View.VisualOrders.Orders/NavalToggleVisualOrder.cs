using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;

namespace NavalDLC.View.VisualOrders.Orders;

public class NavalToggleVisualOrder : VisualOrder
{
	private OrderType _positiveOrder;

	private OrderType _negativeOrder;

	private TextObject _positiveOrderName;

	private TextObject _negativeOrderName;

	public NavalToggleVisualOrder(string stringId, OrderType positiveOrder, OrderType negativeOrder, TextObject positiveOrderName, TextObject negativeOrderName)
		: base(stringId)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_positiveOrder = positiveOrder;
		_negativeOrder = negativeOrder;
		_positiveOrderName = positiveOrderName;
		_negativeOrderName = negativeOrderName;
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((VisualOrder)this).GetActiveState(orderController) == 3)
		{
			orderController.SetOrder(_negativeOrder);
		}
		else
		{
			orderController.SetOrder(_positiveOrder);
		}
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		OrderState activeState = ((VisualOrder)this).GetActiveState(orderController);
		if ((int)activeState == 3 || (int)activeState == 2)
		{
			return _positiveOrderName;
		}
		return _negativeOrderName;
	}

	public override bool IsTargeted()
	{
		return false;
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected I4, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		if (missionBehavior == null)
		{
			return false;
		}
		missionBehavior.GetShip(formation.Team.TeamSide, formation.FormationIndex, out var ship);
		OrderType positiveOrder = _positiveOrder;
		switch ((int)positiveOrder)
		{
		case 14:
			return ship.ShipOrder.BoardAtWill;
		case 35:
			if ((int)((MovementOrder)formation.GetReadonlyMovementOrderReference()).OrderEnum != 2)
			{
				return false;
			}
			return true;
		default:
			return VisualOrderHelper.DoesFormationHaveOrderType(formation, _positiveOrder);
		}
	}

	protected override string GetIconId()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		string iconId = ((VisualOrder)this).GetIconId();
		if ((int)base._lastActiveState == 3)
		{
			return iconId + "_active";
		}
		return iconId;
	}
}
