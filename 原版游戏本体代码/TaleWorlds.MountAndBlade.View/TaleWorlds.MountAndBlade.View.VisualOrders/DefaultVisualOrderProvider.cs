using System.Collections.Generic;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.VisualOrders.OrderSets;
using TaleWorlds.MountAndBlade.View.VisualOrders.Orders;
using TaleWorlds.MountAndBlade.View.VisualOrders.Orders.ToggleOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.FormOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.ToggleOrders;

namespace TaleWorlds.MountAndBlade.View.VisualOrders;

public class DefaultVisualOrderProvider : VisualOrderProvider
{
	public override bool IsAvailable()
	{
		if (Mission.Current != null)
		{
			return !Mission.Current.IsFriendlyMission;
		}
		return false;
	}

	public override MBReadOnlyList<VisualOrderSet> GetOrders()
	{
		if (BannerlordConfig.OrderLayoutType == 1)
		{
			return (MBReadOnlyList<VisualOrderSet>)(object)GetLegacyOrders();
		}
		return GetDefaultOrders();
	}

	private MBReadOnlyList<VisualOrderSet> GetDefaultOrders()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected O, but got Unknown
		MBList<VisualOrderSet> val = new MBList<VisualOrderSet>();
		GenericVisualOrderSet genericVisualOrderSet = new GenericVisualOrderSet("order_type_movement", new TextObject("{=KiJd6Xik}Movement", (Dictionary<string, object>)null), useActiveOrderForIconId: true, useActiveOrderForName: true);
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new MoveVisualOrder("order_movement_move"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new FollowMeVisualOrder("order_movement_follow"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new ChargeVisualOrder("order_movement_charge"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new AdvanceVisualOrder("order_movement_advance"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new FallbackVisualOrder("order_movement_fallback"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new StopVisualOrder("order_movement_stop"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new RetreatVisualOrder("order_movement_retreat"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)new ReturnVisualOrder());
		GenericVisualOrderSet genericVisualOrderSet2 = new GenericVisualOrderSet("order_type_form", new TextObject("{=iBk2wbn3}Form", (Dictionary<string, object>)null), useActiveOrderForIconId: true, useActiveOrderForName: true);
		ArrangementVisualOrder arrangementVisualOrder = new ArrangementVisualOrder((ArrangementOrderEnum)2, "order_form_line");
		ArrangementVisualOrder arrangementVisualOrder2 = new ArrangementVisualOrder((ArrangementOrderEnum)5, "order_form_close");
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)arrangementVisualOrder);
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)arrangementVisualOrder2);
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)3, "order_form_loose"));
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)0, "order_form_circular"));
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)7, "order_form_schiltron"));
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)6, "order_form_v"));
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)1, "order_form_column"));
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)4, "order_form_scatter"));
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)new ReturnVisualOrder());
		GenericVisualOrderSet genericVisualOrderSet3 = new GenericVisualOrderSet("order_type_toggle", new TextObject("{=0HTNYQz2}Toggle", (Dictionary<string, object>)null), useActiveOrderForIconId: false, useActiveOrderForName: false);
		ToggleFacingVisualOrder toggleFacingVisualOrder = new ToggleFacingVisualOrder("order_toggle_facing");
		GenericToggleVisualOrder genericToggleVisualOrder = new GenericToggleVisualOrder("order_toggle_fire", (OrderType)32, (OrderType)31);
		GenericToggleVisualOrder genericToggleVisualOrder2 = new GenericToggleVisualOrder("order_toggle_mount", (OrderType)34, (OrderType)35);
		GenericToggleVisualOrder genericToggleVisualOrder3 = (GameNetwork.IsMultiplayer ? null : new GenericToggleVisualOrder("order_toggle_ai", (OrderType)36, (OrderType)37));
		TransferTroopsVisualOrder val2 = (GameNetwork.IsMultiplayer ? ((TransferTroopsVisualOrder)null) : new TransferTroopsVisualOrder());
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)toggleFacingVisualOrder);
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)genericToggleVisualOrder);
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)genericToggleVisualOrder2);
		if (genericToggleVisualOrder3 != null)
		{
			((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)genericToggleVisualOrder3);
		}
		if (val2 != null)
		{
			((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)val2);
		}
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)new ReturnVisualOrder());
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)genericVisualOrderSet);
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)genericVisualOrderSet2);
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)genericVisualOrderSet3);
		if (!Input.IsGamepadActive)
		{
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)genericToggleVisualOrder));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)genericToggleVisualOrder2));
			if (genericToggleVisualOrder3 != null)
			{
				((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)genericToggleVisualOrder3));
			}
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)toggleFacingVisualOrder));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)arrangementVisualOrder2));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)arrangementVisualOrder));
		}
		return (MBReadOnlyList<VisualOrderSet>)(object)val;
	}

	private MBList<VisualOrderSet> GetLegacyOrders()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		MBList<VisualOrderSet> val = new MBList<VisualOrderSet>();
		GenericVisualOrderSet genericVisualOrderSet = new GenericVisualOrderSet("order_type_movement", new TextObject("{=KiJd6Xik}Movement", (Dictionary<string, object>)null), useActiveOrderForIconId: true, useActiveOrderForName: false);
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new MoveVisualOrder("order_movement_move"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new FollowMeVisualOrder("order_movement_follow"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new ChargeVisualOrder("order_movement_charge"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new AdvanceVisualOrder("order_movement_advance"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new FallbackVisualOrder("order_movement_fallback"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new StopVisualOrder("order_movement_stop"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)(object)new RetreatVisualOrder("order_movement_retreat"));
		((VisualOrderSet)genericVisualOrderSet).AddOrder((VisualOrder)new ReturnVisualOrder());
		GenericVisualOrderSet genericVisualOrderSet2 = new GenericVisualOrderSet("order_type_facing", new TextObject("{=psynaDsM}Facing", (Dictionary<string, object>)null), useActiveOrderForIconId: true, useActiveOrderForName: false);
		SingleVisualOrder singleVisualOrder = new SingleVisualOrder("order_toggle_facing", new TextObject("{=MH9Pi3ao}Face Direction", (Dictionary<string, object>)null), (OrderType)15, useFormationTarget: false, useWorldPositionTarget: true);
		SingleVisualOrder singleVisualOrder2 = new SingleVisualOrder("order_toggle_facing_active", new TextObject("{=u8j8nN5U}Face Enemy", (Dictionary<string, object>)null), (OrderType)14, useFormationTarget: false, useWorldPositionTarget: false);
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)singleVisualOrder);
		((VisualOrderSet)genericVisualOrderSet2).AddOrder((VisualOrder)(object)singleVisualOrder2);
		GenericVisualOrderSet genericVisualOrderSet3 = new GenericVisualOrderSet("order_type_form", new TextObject("{=iBk2wbn3}Form", (Dictionary<string, object>)null), useActiveOrderForIconId: true, useActiveOrderForName: true);
		ArrangementVisualOrder arrangementVisualOrder = new ArrangementVisualOrder((ArrangementOrderEnum)2, "order_form_line");
		ArrangementVisualOrder arrangementVisualOrder2 = new ArrangementVisualOrder((ArrangementOrderEnum)5, "order_form_close");
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)arrangementVisualOrder);
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)arrangementVisualOrder2);
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)3, "order_form_loose"));
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)0, "order_form_circular"));
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)7, "order_form_schiltron"));
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)6, "order_form_v"));
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)1, "order_form_column"));
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)(object)new ArrangementVisualOrder((ArrangementOrderEnum)4, "order_form_scatter"));
		((VisualOrderSet)genericVisualOrderSet3).AddOrder((VisualOrder)new ReturnVisualOrder());
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)genericVisualOrderSet);
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)genericVisualOrderSet2);
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)genericVisualOrderSet3);
		GenericToggleVisualOrder order = new GenericToggleVisualOrder("order_toggle_fire", (OrderType)32, (OrderType)31);
		GenericToggleVisualOrder order2 = new GenericToggleVisualOrder("order_toggle_mount", (OrderType)34, (OrderType)35);
		GenericToggleVisualOrder genericToggleVisualOrder = (GameNetwork.IsMultiplayer ? null : new GenericToggleVisualOrder("order_toggle_ai", (OrderType)36, (OrderType)37));
		TransferTroopsVisualOrder val2 = (GameNetwork.IsMultiplayer ? ((TransferTroopsVisualOrder)null) : new TransferTroopsVisualOrder());
		if (!Input.IsGamepadActive)
		{
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)order));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)order2));
			if (genericToggleVisualOrder != null)
			{
				((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)genericToggleVisualOrder));
			}
			if (val2 != null)
			{
				((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)(object)val2));
			}
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)new SingleVisualOrderSet((VisualOrder)new ReturnVisualOrder()));
		}
		return val;
	}
}
