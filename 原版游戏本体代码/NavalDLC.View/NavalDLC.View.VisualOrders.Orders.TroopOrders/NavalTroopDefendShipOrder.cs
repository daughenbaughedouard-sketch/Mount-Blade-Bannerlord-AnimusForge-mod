using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;

namespace NavalDLC.View.VisualOrders.Orders.TroopOrders;

public class NavalTroopDefendShipOrder : VisualOrder
{
	public NavalTroopDefendShipOrder(string iconId)
		: base(iconId)
	{
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		orderController.SetOrder((OrderType)34);
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=FUeeV5aO}Defend Ship", (Dictionary<string, object>)null);
	}

	public override bool IsTargeted()
	{
		return false;
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		return VisualOrderHelper.DoesFormationHaveOrderType(formation, (OrderType)34);
	}
}
