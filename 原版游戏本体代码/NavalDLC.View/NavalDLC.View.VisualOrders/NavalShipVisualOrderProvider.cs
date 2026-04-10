using System.Collections.Generic;
using NavalDLC.View.VisualOrders.Orders;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.VisualOrders.OrderSets;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.ToggleOrders;

namespace NavalDLC.View.VisualOrders;

public class NavalShipVisualOrderProvider : VisualOrderProvider
{
	public override MBReadOnlyList<VisualOrderSet> GetOrders()
	{
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Expected O, but got Unknown
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Expected O, but got Unknown
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Expected O, but got Unknown
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected O, but got Unknown
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		MBList<VisualOrderSet> val = new MBList<VisualOrderSet>();
		if (Input.IsGamepadActive)
		{
			GenericVisualOrderSet val2 = new GenericVisualOrderSet("order_type_movement", new TextObject("{=KiJd6Xik}Movement", (Dictionary<string, object>)null), true, true);
			((VisualOrderSet)val2).AddOrder((VisualOrder)(object)new NavalMovementOrder("order_movement_move", (OrderType)1, new TextObject("{=F7JGCr9s}Move", (Dictionary<string, object>)null), useWorldPosition: true));
			((VisualOrderSet)val2).AddOrder((VisualOrder)(object)new NavalMovementOrder("order_movement_follow", (OrderType)7, new TextObject("{=5LpufKs7}Follow Me", (Dictionary<string, object>)null)));
			((VisualOrderSet)val2).AddOrder((VisualOrder)(object)new NavalSkirmishOrder("order_movement_skirmish"));
			((VisualOrderSet)val2).AddOrder((VisualOrder)(object)new NavalMovementOrder("order_movement_advance", (OrderType)12, new TextObject("{=A38xbjqm}Engage", (Dictionary<string, object>)null), useWorldPosition: false, isTargeted: true));
			((VisualOrderSet)val2).AddOrder((VisualOrder)(object)new NavalMovementOrder("order_movement_stop", (OrderType)6, new TextObject("{=QTr6UDAa}Stop", (Dictionary<string, object>)null)));
			((VisualOrderSet)val2).AddOrder((VisualOrder)(object)new NavalMovementOrder("order_movement_retreat", (OrderType)9, new TextObject("{=VbeHEAsa}Retreat", (Dictionary<string, object>)null)));
			((VisualOrderSet)val2).AddOrder((VisualOrder)new ReturnVisualOrder());
			GenericVisualOrderSet val3 = new GenericVisualOrderSet("order_type_toggle", new TextObject("{=0HTNYQz2}Toggle", (Dictionary<string, object>)null), false, false);
			GenericToggleVisualOrder val4 = new GenericToggleVisualOrder("order_toggle_fire", (OrderType)32, (OrderType)31);
			GenericToggleVisualOrder val5 = (GameNetwork.IsMultiplayer ? ((GenericToggleVisualOrder)null) : new GenericToggleVisualOrder("order_toggle_ai", (OrderType)36, (OrderType)37));
			((VisualOrderSet)val3).AddOrder((VisualOrder)(object)val4);
			if (val5 != null)
			{
				((VisualOrderSet)val3).AddOrder((VisualOrder)(object)val5);
			}
			((VisualOrderSet)val3).AddOrder((VisualOrder)new ReturnVisualOrder());
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)val2);
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)val3);
			if (val5 != null)
			{
				((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val5));
			}
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)new ReturnVisualOrder()));
		}
		else
		{
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)CreateSingleOrderSetFor((VisualOrder)(object)new NavalMovementOrder("order_movement_move", (OrderType)1, new TextObject("{=F7JGCr9s}Move", (Dictionary<string, object>)null), useWorldPosition: true)));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)CreateSingleOrderSetFor((VisualOrder)(object)new NavalMovementOrder("order_movement_follow", (OrderType)7, new TextObject("{=5LpufKs7}Follow Me", (Dictionary<string, object>)null))));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)CreateSingleOrderSetFor((VisualOrder)(object)new NavalSkirmishOrder("order_movement_skirmish")));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)CreateSingleOrderSetFor((VisualOrder)(object)new NavalMovementOrder("order_movement_advance", (OrderType)12, new TextObject("{=A38xbjqm}Engage", (Dictionary<string, object>)null), useWorldPosition: false, isTargeted: true)));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)CreateSingleOrderSetFor((VisualOrder)(object)new NavalMovementOrder("order_movement_stop", (OrderType)6, new TextObject("{=QTr6UDAa}Stop", (Dictionary<string, object>)null))));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)CreateSingleOrderSetFor((VisualOrder)(object)new NavalMovementOrder("order_movement_retreat", (OrderType)9, new TextObject("{=VbeHEAsa}Retreat", (Dictionary<string, object>)null))));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)CreateSingleOrderSetFor((VisualOrder)new GenericToggleVisualOrder("order_toggle_fire", (OrderType)32, (OrderType)31)));
			GenericToggleVisualOrder val6 = (GameNetwork.IsMultiplayer ? ((GenericToggleVisualOrder)null) : new GenericToggleVisualOrder("order_toggle_ai", (OrderType)36, (OrderType)37));
			if (val6 != null)
			{
				((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)CreateSingleOrderSetFor((VisualOrder)(object)val6));
			}
		}
		return (MBReadOnlyList<VisualOrderSet>)(object)val;
	}

	private SingleVisualOrderSet CreateSingleOrderSetFor(VisualOrder order)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new SingleVisualOrderSet(order);
	}

	public override bool IsAvailable()
	{
		Mission current = Mission.Current;
		if (current != null && current.IsNavalBattle)
		{
			return NavalDLCHelpers.IsShipOrdersAvailable();
		}
		return false;
	}
}
