using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;

namespace TaleWorlds.MountAndBlade.View.VisualOrders.Orders.ToggleOrders;

public class ToggleFacingVisualOrder : VisualOrder
{
	public ToggleFacingVisualOrder(string iconId)
		: base(iconId)
	{
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		OrderState activeState = ((VisualOrder)this).GetActiveState(orderController);
		if ((int)activeState != 3 && (int)activeState != 2)
		{
			return new TextObject("{=LWVwNcRA}Facing Direction", (Dictionary<string, object>)null);
		}
		return new TextObject("{=qWzBa3KT}Facing Enemy", (Dictionary<string, object>)null);
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (IsFacingEnemy(((VisualOrder)this).GetActiveState(orderController)))
		{
			orderController.SetOrderWithPosition((OrderType)15, executionParameters.WorldPosition);
		}
		else
		{
			orderController.SetOrder((OrderType)14);
		}
	}

	public override bool IsTargeted()
	{
		return false;
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		return (int)OrderController.GetActiveFacingOrderOf(formation) == 14;
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

	private static bool IsFacingEnemy(OrderState activeState)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)activeState == 3;
	}
}
