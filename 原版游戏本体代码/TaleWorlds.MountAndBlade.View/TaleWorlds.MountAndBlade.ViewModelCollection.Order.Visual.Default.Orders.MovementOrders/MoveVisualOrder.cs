using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;

public class MoveVisualOrder : VisualOrder
{
	public MoveVisualOrder(string iconId)
		: base(iconId)
	{
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=vbAZwibd}Move to Position", (Dictionary<string, object>)null);
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (executionParameters.HasWorldPosition)
		{
			WorldPosition worldPosition = executionParameters.WorldPosition;
			orderController.SetOrderWithTwoPositions((OrderType)2, worldPosition, worldPosition);
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
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		OrderType activeMovementOrderOf = OrderController.GetActiveMovementOrderOf(formation);
		return (int)activeMovementOrderOf == 1 || (int)activeMovementOrderOf == 2 || (int)activeMovementOrderOf == 3;
	}

	public override bool IsTargeted()
	{
		return false;
	}
}
