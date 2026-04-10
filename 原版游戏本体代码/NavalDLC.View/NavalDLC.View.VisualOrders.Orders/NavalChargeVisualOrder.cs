using NavalDLC.Missions.MissionLogics;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;

namespace NavalDLC.View.VisualOrders.Orders;

public class NavalChargeVisualOrder : ChargeVisualOrder
{
	public NavalChargeVisualOrder(string iconId)
		: base(iconId)
	{
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		if (((ChargeVisualOrder)this).OnGetFormationHasOrder(formation) == true)
		{
			return true;
		}
		if ((int)OrderController.GetActiveMovementOrderOf(formation) == 1)
		{
			Mission current = Mission.Current;
			NavalShipsLogic navalShipsLogic = ((current != null) ? current.GetMissionBehavior<NavalShipsLogic>() : null);
			if (navalShipsLogic != null && navalShipsLogic.GetShip(formation, out var ship))
			{
				return ship.ShipOrder?.GetIsChargeOrderOverridden() ?? false;
			}
		}
		return false;
	}
}
