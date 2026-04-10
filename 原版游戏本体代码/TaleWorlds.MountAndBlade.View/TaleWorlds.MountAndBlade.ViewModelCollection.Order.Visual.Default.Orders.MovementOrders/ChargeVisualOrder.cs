using System.Collections.Generic;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;

public class ChargeVisualOrder : VisualOrder
{
	public ChargeVisualOrder(string iconId)
		: base(iconId)
	{
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=Dxmq32qW}Charge", (Dictionary<string, object>)null);
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (executionParameters.HasFormation)
		{
			orderController.SetOrderWithFormation((OrderType)4, executionParameters.Formation);
		}
		else
		{
			orderController.SetOrder((OrderType)4);
		}
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		OrderType activeMovementOrderOf = OrderController.GetActiveMovementOrderOf(formation);
		return (int)activeMovementOrderOf == 4 || (int)activeMovementOrderOf == 5;
	}

	public override bool IsTargeted()
	{
		return true;
	}
}
