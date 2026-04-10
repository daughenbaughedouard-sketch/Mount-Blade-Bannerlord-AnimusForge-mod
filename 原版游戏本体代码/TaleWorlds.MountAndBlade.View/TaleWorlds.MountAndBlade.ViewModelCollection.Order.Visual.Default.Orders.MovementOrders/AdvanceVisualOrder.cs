using System.Collections.Generic;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;

public class AdvanceVisualOrder : VisualOrder
{
	public AdvanceVisualOrder(string iconId)
		: base(iconId)
	{
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=A38xbjqm}Engage", (Dictionary<string, object>)null);
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (executionParameters.HasFormation)
		{
			orderController.SetOrderWithFormation((OrderType)12, executionParameters.Formation);
		}
		else
		{
			orderController.SetOrder((OrderType)12);
		}
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		return (int)OrderController.GetActiveMovementOrderOf(formation) == 12;
	}

	public override bool IsTargeted()
	{
		return true;
	}
}
