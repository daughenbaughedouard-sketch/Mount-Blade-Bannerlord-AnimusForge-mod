using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;

namespace TaleWorlds.MountAndBlade.View.VisualOrders.OrderSets;

public class GenericVisualOrderSet : VisualOrderSet
{
	private readonly TextObject _name;

	private readonly string _stringId;

	private readonly bool _useActiveOrderForIconId;

	private readonly bool _useActiveOrderForName;

	public override bool IsSoloOrder => false;

	public override string StringId => _stringId;

	public override string IconId
	{
		get
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Invalid comparison between Unknown and I4
			if (_useActiveOrderForIconId)
			{
				for (int i = 0; i < ((List<VisualOrder>)(object)((VisualOrderSet)this).Orders).Count; i++)
				{
					if ((int)((List<VisualOrder>)(object)((VisualOrderSet)this).Orders)[i].GetActiveState(Mission.Current.PlayerTeam.PlayerOrderController) == 3)
					{
						return ((List<VisualOrder>)(object)((VisualOrderSet)this).Orders)[i].IconId;
					}
				}
			}
			return _stringId;
		}
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		if (_useActiveOrderForName)
		{
			for (int i = 0; i < ((List<VisualOrder>)(object)((VisualOrderSet)this).Orders).Count; i++)
			{
				if ((int)((List<VisualOrder>)(object)((VisualOrderSet)this).Orders)[i].GetActiveState(orderController) == 3)
				{
					return ((List<VisualOrder>)(object)((VisualOrderSet)this).Orders)[i].GetName(orderController);
				}
			}
		}
		return _name;
	}

	public GenericVisualOrderSet(string stringId, TextObject name, bool useActiveOrderForIconId, bool useActiveOrderForName)
	{
		_stringId = stringId;
		_name = name;
		_useActiveOrderForIconId = useActiveOrderForIconId;
		_useActiveOrderForName = useActiveOrderForName;
	}
}
