using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.Library;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class ShipFireContainerWidget : Widget
{
	private int _fireHitPoints;

	private int _maxFireHitPoints;

	private Widget _compassCenterWidget;

	[Editor(false)]
	public int FireHitPoints
	{
		get
		{
			return _fireHitPoints;
		}
		set
		{
			if (_fireHitPoints != value)
			{
				_fireHitPoints = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "FireHitPoints");
				OnFireDamageUpdated();
			}
		}
	}

	[Editor(false)]
	public int MaxFireHitPoints
	{
		get
		{
			return _maxFireHitPoints;
		}
		set
		{
			if (_maxFireHitPoints != value)
			{
				_maxFireHitPoints = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "MaxFireHitPoints");
				OnFireDamageUpdated();
			}
		}
	}

	[Editor(false)]
	public Widget CompassCenterWidget
	{
		get
		{
			return _compassCenterWidget;
		}
		set
		{
			if (_compassCenterWidget != value)
			{
				_compassCenterWidget = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "CompassCenterWidget");
				OnFireDamageUpdated();
			}
		}
	}

	public ShipFireContainerWidget(UIContext context)
		: base(context)
	{
	}

	private void OnFireDamageUpdated()
	{
		if (((Widget)this).ChildCount <= 0)
		{
			return;
		}
		float num = ((MaxFireHitPoints != 0) ? ((float)(MaxFireHitPoints - FireHitPoints) / (float)MaxFireHitPoints * 100f) : 100f);
		num = MathF.Clamp(num, 0f, 100f);
		num = MathF.Floor(num);
		float num2 = 100 / ((Widget)this).ChildCount;
		for (int i = 0; i < ((Widget)this).ChildCount; i++)
		{
			float num3 = (num - (float)i * num2) / num2;
			num3 = MathF.Clamp(num3, 0f, 1f);
			Widget child = ((Widget)this).GetChild(i);
			if (num3 == 0f)
			{
				if (num == 0f)
				{
					child.SetState("Disabled");
				}
				else
				{
					child.SetState("Inactive");
				}
			}
			else if (num3 < 1f)
			{
				child.SetState("Default");
			}
			else if (num == 100f)
			{
				child.SetState("FastBurning");
			}
			else
			{
				child.SetState("SlowBurning");
			}
			FillBarVerticalWidget val;
			if ((val = (FillBarVerticalWidget)(object)((child is FillBarVerticalWidget) ? child : null)) != null)
			{
				val.InitialAmountAsFloat = num3;
				val.MaxAmountAsFloat = 1f;
			}
		}
		if (num == 100f)
		{
			Widget compassCenterWidget = CompassCenterWidget;
			if (compassCenterWidget != null)
			{
				compassCenterWidget.SetState("Burning");
			}
		}
		else
		{
			Widget compassCenterWidget2 = CompassCenterWidget;
			if (compassCenterWidget2 != null)
			{
				compassCenterWidget2.SetState("Default");
			}
		}
	}
}
