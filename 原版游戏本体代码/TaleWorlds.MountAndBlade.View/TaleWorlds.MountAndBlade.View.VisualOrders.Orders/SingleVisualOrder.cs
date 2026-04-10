using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;

namespace TaleWorlds.MountAndBlade.View.VisualOrders.Orders;

public class SingleVisualOrder : VisualOrder
{
	private TextObject _name;

	private OrderType _orderType;

	private bool _useFormationTarget;

	private bool _useWorldPositionTarget;

	public SingleVisualOrder(string stringId, TextObject name, OrderType orderType, bool useFormationTarget, bool useWorldPositionTarget)
		: base(stringId)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_name = name;
		_orderType = orderType;
		_useFormationTarget = useFormationTarget;
		_useWorldPositionTarget = useWorldPositionTarget;
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (executionParameters.HasFormation && _useFormationTarget)
		{
			orderController.SetOrderWithFormation(_orderType, executionParameters.Formation);
		}
		else if (executionParameters.HasWorldPosition && _useWorldPositionTarget)
		{
			orderController.SetOrderWithPosition(_orderType, executionParameters.WorldPosition);
		}
		else
		{
			orderController.SetOrder(_orderType);
		}
	}

	public override TextObject GetName(OrderController orderController)
	{
		return _name;
	}

	public override bool IsTargeted()
	{
		if (!_useFormationTarget)
		{
			return _useWorldPositionTarget;
		}
		return true;
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return VisualOrderHelper.DoesFormationHaveOrderType(formation, _orderType);
	}
}
