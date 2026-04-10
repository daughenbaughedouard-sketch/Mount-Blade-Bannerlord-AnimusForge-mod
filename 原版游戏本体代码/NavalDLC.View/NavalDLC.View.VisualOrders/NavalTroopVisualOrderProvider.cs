using System.Collections.Generic;
using NavalDLC.View.VisualOrders.Orders;
using NavalDLC.View.VisualOrders.Orders.TroopOrders;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.VisualOrders.OrderSets;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.ToggleOrders;

namespace NavalDLC.View.VisualOrders;

public class NavalTroopVisualOrderProvider : VisualOrderProvider
{
	public override MBReadOnlyList<VisualOrderSet> GetOrders()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		MBList<VisualOrderSet> val = new MBList<VisualOrderSet>();
		if (Input.IsGamepadActive)
		{
			GenericVisualOrderSet val2 = new GenericVisualOrderSet("troop_visual_orders", new TextObject("{=bEmrKaHS}Orders", (Dictionary<string, object>)null), true, false);
			((VisualOrderSet)val2).AddOrder((VisualOrder)(object)new NavalTroopDefendShipOrder("naval_troop_defend_ship_order"));
			((VisualOrderSet)val2).AddOrder((VisualOrder)new FollowMeVisualOrder("order_movement_follow"));
			((VisualOrderSet)val2).AddOrder((VisualOrder)(object)new NavalChargeVisualOrder("order_movement_charge"));
			((VisualOrderSet)val2).AddOrder((VisualOrder)new GenericToggleVisualOrder("order_toggle_fire", (OrderType)32, (OrderType)31));
			if (!GameNetwork.IsMultiplayer)
			{
				((VisualOrderSet)val2).AddOrder((VisualOrder)new GenericToggleVisualOrder("order_toggle_ai", (OrderType)36, (OrderType)37));
			}
			((VisualOrderSet)val2).AddOrder((VisualOrder)new ReturnVisualOrder());
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)val2);
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)new ReturnVisualOrder()));
		}
		else
		{
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)new NavalTroopDefendShipOrder("naval_troop_defend_ship_order")));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)new FollowMeVisualOrder("order_movement_follow")));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)new NavalChargeVisualOrder("order_movement_charge")));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)new GenericToggleVisualOrder("order_toggle_fire", (OrderType)32, (OrderType)31)));
			if (!GameNetwork.IsMultiplayer)
			{
				((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)new GenericToggleVisualOrder("order_toggle_ai", (OrderType)36, (OrderType)37)));
			}
		}
		return (MBReadOnlyList<VisualOrderSet>)(object)val;
	}

	public override bool IsAvailable()
	{
		Mission current = Mission.Current;
		if (current != null && current.IsNavalBattle)
		{
			return !NavalDLCHelpers.IsShipOrdersAvailable();
		}
		return false;
	}
}
