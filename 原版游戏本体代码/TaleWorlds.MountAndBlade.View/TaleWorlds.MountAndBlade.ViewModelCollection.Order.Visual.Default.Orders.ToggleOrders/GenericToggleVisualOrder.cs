using System.Collections.Generic;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.ToggleOrders;

public class GenericToggleVisualOrder : VisualOrder
{
	private readonly TextObject _positiveOrderName;

	private readonly TextObject _negativeOrderName;

	public OrderType PositiveOrder { get; }

	public OrderType NegativeOrder { get; }

	public GenericToggleVisualOrder(string stringId, OrderType positiveOrder, OrderType negativeOrder)
		: base(stringId)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		PositiveOrder = positiveOrder;
		NegativeOrder = negativeOrder;
		_positiveOrderName = GetOrderName(positiveOrder);
		_negativeOrderName = GetOrderName(negativeOrder);
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		OrderState activeState = ((VisualOrder)this).GetActiveState(orderController);
		if ((int)activeState == 3 || (int)activeState == 2)
		{
			return _positiveOrderName;
		}
		return _negativeOrderName;
	}

	public override bool IsTargeted()
	{
		return false;
	}

	private static TextObject GetOrderName(OrderType orderType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected I4, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		if ((int)orderType != 14)
		{
			if ((int)orderType != 15)
			{
				return (TextObject)((orderType - 31) switch
				{
					1 => (object)new TextObject("{=itoYrj8d}Firing at will", (Dictionary<string, object>)null), 
					0 => (object)new TextObject("{=VyI0rimN}Holding Fire", (Dictionary<string, object>)null), 
					3 => (object)new TextObject("{=ubTGIdcv}Mounted", (Dictionary<string, object>)null), 
					4 => (object)new TextObject("{=Ema5Vd6o}Dismounted", (Dictionary<string, object>)null), 
					5 => (object)new TextObject("{=zatDiaEI}Delegate Command On", (Dictionary<string, object>)null), 
					6 => (object)new TextObject("{=JceqNdWx}Delegate Command Off", (Dictionary<string, object>)null), 
					_ => TextObject.GetEmpty(), 
				});
			}
			return new TextObject("{=1gC25EMb}Face this Direction", (Dictionary<string, object>)null);
		}
		return new TextObject("{=u8j8nN5U}Face Enemy", (Dictionary<string, object>)null);
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((VisualOrder)this).GetActiveState(orderController) == 3)
		{
			orderController.SetOrder(NegativeOrder);
		}
		else
		{
			orderController.SetOrder(PositiveOrder);
		}
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		if (VisualOrderHelper.DoesFormationHaveOrderType(formation, PositiveOrder))
		{
			return true;
		}
		return false;
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
}
