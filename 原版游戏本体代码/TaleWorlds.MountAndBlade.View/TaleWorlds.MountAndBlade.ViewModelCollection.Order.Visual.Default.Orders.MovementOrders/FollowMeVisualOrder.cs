using System.Collections.Generic;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;

public class FollowMeVisualOrder : VisualOrder
{
	public FollowMeVisualOrder(string iconId)
		: base(iconId)
	{
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=5LpufKs7}Follow Me", (Dictionary<string, object>)null);
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		orderController.SetOrderWithAgent((OrderType)7, executionParameters.Agent);
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		return (int)OrderController.GetActiveMovementOrderOf(formation) == 7;
	}

	public override bool IsTargeted()
	{
		return false;
	}
}
