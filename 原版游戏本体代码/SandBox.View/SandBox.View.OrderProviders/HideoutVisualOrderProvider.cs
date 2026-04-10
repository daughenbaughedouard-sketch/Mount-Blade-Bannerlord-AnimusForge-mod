using System.Collections.Generic;
using SandBox.Missions.MissionLogics.Hideout;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.VisualOrders.OrderSets;
using TaleWorlds.MountAndBlade.View.VisualOrders.Orders;
using TaleWorlds.MountAndBlade.View.VisualOrders.Orders.ToggleOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.FormOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.ToggleOrders;

namespace SandBox.View.OrderProviders;

internal class HideoutVisualOrderProvider : VisualOrderProvider
{
	public override bool IsAvailable()
	{
		Mission current = Mission.Current;
		if (current == null)
		{
			return false;
		}
		return current.HasMissionBehavior<HideoutMissionController>();
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
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected O, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected O, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected O, but got Unknown
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Expected O, but got Unknown
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Expected O, but got Unknown
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Expected O, but got Unknown
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Expected O, but got Unknown
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Expected O, but got Unknown
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Expected O, but got Unknown
		MBList<VisualOrderSet> val = new MBList<VisualOrderSet>();
		GenericVisualOrderSet val2 = new GenericVisualOrderSet("order_type_movement", new TextObject("{=KiJd6Xik}Movement", (Dictionary<string, object>)null), true, true);
		((VisualOrderSet)val2).AddOrder((VisualOrder)new MoveVisualOrder("order_movement_move"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new FollowMeVisualOrder("order_movement_follow"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new ChargeVisualOrder("order_movement_charge"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new FallbackVisualOrder("order_movement_fallback"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new StopVisualOrder("order_movement_stop"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new RetreatVisualOrder("order_movement_retreat"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new ReturnVisualOrder());
		GenericVisualOrderSet val3 = new GenericVisualOrderSet("order_type_form", new TextObject("{=iBk2wbn3}Form", (Dictionary<string, object>)null), true, true);
		ArrangementVisualOrder val4 = new ArrangementVisualOrder((ArrangementOrderEnum)2, "order_form_line");
		ArrangementVisualOrder val5 = new ArrangementVisualOrder((ArrangementOrderEnum)5, "order_form_close");
		((VisualOrderSet)val3).AddOrder((VisualOrder)(object)val4);
		((VisualOrderSet)val3).AddOrder((VisualOrder)(object)val5);
		((VisualOrderSet)val3).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)3, "order_form_loose"));
		((VisualOrderSet)val3).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)0, "order_form_circular"));
		((VisualOrderSet)val3).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)7, "order_form_schiltron"));
		((VisualOrderSet)val3).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)6, "order_form_v"));
		((VisualOrderSet)val3).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)1, "order_form_column"));
		((VisualOrderSet)val3).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)4, "order_form_scatter"));
		((VisualOrderSet)val3).AddOrder((VisualOrder)new ReturnVisualOrder());
		GenericVisualOrderSet val6 = new GenericVisualOrderSet("order_type_toggle", new TextObject("{=0HTNYQz2}Toggle", (Dictionary<string, object>)null), false, false);
		ToggleFacingVisualOrder val7 = new ToggleFacingVisualOrder("order_toggle_facing");
		GenericToggleVisualOrder val8 = new GenericToggleVisualOrder("order_toggle_fire", (OrderType)32, (OrderType)31);
		GenericToggleVisualOrder val9 = new GenericToggleVisualOrder("order_toggle_mount", (OrderType)34, (OrderType)35);
		GenericToggleVisualOrder val10 = (GameNetwork.IsMultiplayer ? ((GenericToggleVisualOrder)null) : new GenericToggleVisualOrder("order_toggle_ai", (OrderType)36, (OrderType)37));
		TransferTroopsVisualOrder val11 = (GameNetwork.IsMultiplayer ? ((TransferTroopsVisualOrder)null) : new TransferTroopsVisualOrder());
		((VisualOrderSet)val6).AddOrder((VisualOrder)(object)val7);
		((VisualOrderSet)val6).AddOrder((VisualOrder)(object)val8);
		((VisualOrderSet)val6).AddOrder((VisualOrder)(object)val9);
		if (val10 != null)
		{
			((VisualOrderSet)val6).AddOrder((VisualOrder)(object)val10);
		}
		if (val11 != null)
		{
			((VisualOrderSet)val6).AddOrder((VisualOrder)(object)val11);
		}
		((VisualOrderSet)val6).AddOrder((VisualOrder)new ReturnVisualOrder());
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)val2);
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)val3);
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)val6);
		if (!Input.IsGamepadActive)
		{
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val8));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val9));
			if (val10 != null)
			{
				((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val10));
			}
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val7));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val5));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val4));
		}
		return (MBReadOnlyList<VisualOrderSet>)(object)val;
	}

	private MBList<VisualOrderSet> GetLegacyOrders()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Expected O, but got Unknown
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Expected O, but got Unknown
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Expected O, but got Unknown
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Expected O, but got Unknown
		MBList<VisualOrderSet> val = new MBList<VisualOrderSet>();
		GenericVisualOrderSet val2 = new GenericVisualOrderSet("order_type_movement", new TextObject("{=KiJd6Xik}Movement", (Dictionary<string, object>)null), true, false);
		((VisualOrderSet)val2).AddOrder((VisualOrder)new MoveVisualOrder("order_movement_move"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new FollowMeVisualOrder("order_movement_follow"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new ChargeVisualOrder("order_movement_charge"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new FallbackVisualOrder("order_movement_fallback"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new StopVisualOrder("order_movement_stop"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new RetreatVisualOrder("order_movement_retreat"));
		((VisualOrderSet)val2).AddOrder((VisualOrder)new ReturnVisualOrder());
		GenericVisualOrderSet val3 = new GenericVisualOrderSet("order_type_facing", new TextObject("{=psynaDsM}Facing", (Dictionary<string, object>)null), true, false);
		SingleVisualOrder val4 = new SingleVisualOrder("order_toggle_facing", new TextObject("{=MH9Pi3ao}Face Direction", (Dictionary<string, object>)null), (OrderType)15, false, true);
		SingleVisualOrder val5 = new SingleVisualOrder("order_toggle_facing_active", new TextObject("{=u8j8nN5U}Face Enemy", (Dictionary<string, object>)null), (OrderType)14, false, false);
		((VisualOrderSet)val3).AddOrder((VisualOrder)(object)val4);
		((VisualOrderSet)val3).AddOrder((VisualOrder)(object)val5);
		GenericVisualOrderSet val6 = new GenericVisualOrderSet("order_type_form", new TextObject("{=iBk2wbn3}Form", (Dictionary<string, object>)null), true, true);
		ArrangementVisualOrder val7 = new ArrangementVisualOrder((ArrangementOrderEnum)2, "order_form_line");
		ArrangementVisualOrder val8 = new ArrangementVisualOrder((ArrangementOrderEnum)5, "order_form_close");
		((VisualOrderSet)val6).AddOrder((VisualOrder)(object)val7);
		((VisualOrderSet)val6).AddOrder((VisualOrder)(object)val8);
		((VisualOrderSet)val6).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)3, "order_form_loose"));
		((VisualOrderSet)val6).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)0, "order_form_circular"));
		((VisualOrderSet)val6).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)7, "order_form_schiltron"));
		((VisualOrderSet)val6).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)6, "order_form_v"));
		((VisualOrderSet)val6).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)1, "order_form_column"));
		((VisualOrderSet)val6).AddOrder((VisualOrder)new ArrangementVisualOrder((ArrangementOrderEnum)4, "order_form_scatter"));
		((VisualOrderSet)val6).AddOrder((VisualOrder)new ReturnVisualOrder());
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)val2);
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)val3);
		((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)(object)val6);
		GenericToggleVisualOrder val9 = new GenericToggleVisualOrder("order_toggle_fire", (OrderType)32, (OrderType)31);
		GenericToggleVisualOrder val10 = new GenericToggleVisualOrder("order_toggle_mount", (OrderType)34, (OrderType)35);
		GenericToggleVisualOrder val11 = (GameNetwork.IsMultiplayer ? ((GenericToggleVisualOrder)null) : new GenericToggleVisualOrder("order_toggle_ai", (OrderType)36, (OrderType)37));
		TransferTroopsVisualOrder val12 = (GameNetwork.IsMultiplayer ? ((TransferTroopsVisualOrder)null) : new TransferTroopsVisualOrder());
		if (!Input.IsGamepadActive)
		{
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val9));
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val10));
			if (val11 != null)
			{
				((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val11));
			}
			if (val12 != null)
			{
				((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)(object)val12));
			}
			((List<VisualOrderSet>)(object)val).Add((VisualOrderSet)new SingleVisualOrderSet((VisualOrder)new ReturnVisualOrder()));
		}
		return val;
	}
}
