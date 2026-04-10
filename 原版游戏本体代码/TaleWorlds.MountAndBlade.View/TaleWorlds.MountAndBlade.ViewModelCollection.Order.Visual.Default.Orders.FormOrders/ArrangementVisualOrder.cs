using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.FormOrders;

public class ArrangementVisualOrder : VisualOrder
{
	public ArrangementOrderEnum ArrangementOrder { get; }

	public ArrangementVisualOrder(ArrangementOrderEnum arrangementOrder, string iconId)
		: base(iconId)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		ArrangementOrder = arrangementOrder;
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected I4, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		ArrangementOrderEnum arrangementOrder = ArrangementOrder;
		return (TextObject)((int)arrangementOrder switch
		{
			0 => (object)new TextObject("{=9TGLirQf}Circle", (Dictionary<string, object>)null), 
			1 => (object)new TextObject("{=WsmZzaOq}Column", (Dictionary<string, object>)null), 
			2 => (object)new TextObject("{=9aboazgu}Line", (Dictionary<string, object>)null), 
			3 => (object)new TextObject("{=iJXH3841}Loose", (Dictionary<string, object>)null), 
			4 => (object)new TextObject("{=eEf7hE4r}Scatter", (Dictionary<string, object>)null), 
			5 => (object)new TextObject("{=rTPnyeJ3}Shield Wall", (Dictionary<string, object>)null), 
			6 => (object)new TextObject("{=uCyQNvq1}Skein", (Dictionary<string, object>)null), 
			7 => (object)new TextObject("{=squareOrder}Square", (Dictionary<string, object>)null), 
			_ => TextObject.GetEmpty(), 
		});
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ArrangementOrder val = default(ArrangementOrder);
		((ArrangementOrder)(ref val))._002Ector(ArrangementOrder);
		orderController.SetOrder(((ArrangementOrder)(ref val)).OrderType);
	}

	public override bool IsTargeted()
	{
		return false;
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return formation.ArrangementOrder.OrderEnum == ArrangementOrder;
	}

	private static OrderType GetArrangementOrderType(ArrangementOrderEnum arrangementOrderEnum)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected I4, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		switch ((int)arrangementOrderEnum)
		{
		case 0:
			return (OrderType)19;
		case 1:
			return (OrderType)22;
		case 2:
			return (OrderType)16;
		case 3:
			return (OrderType)16;
		default:
			Debug.FailedAssert("Failed to find arrangement order type: " + arrangementOrderEnum, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\VisualOrders\\Orders\\FormOrders\\ArrangementVisualOrder.cs", "GetArrangementOrderType", 78);
			return (OrderType)0;
		}
	}
}
