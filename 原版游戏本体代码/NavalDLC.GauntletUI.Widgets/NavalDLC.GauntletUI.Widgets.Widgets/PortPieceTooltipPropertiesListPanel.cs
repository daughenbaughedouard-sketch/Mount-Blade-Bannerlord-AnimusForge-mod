using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class PortPieceTooltipPropertiesListPanel : ListPanel
{
	private bool _isDirty = true;

	public PortPieceTooltipPropertiesListPanel(UIContext context)
		: base(context)
	{
	}

	protected override void OnUpdate(float dt)
	{
		((ListPanel)this).OnUpdate(dt);
		if (((Widget)this).ChildCount == 0 || !_isDirty)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < ((Widget)this).ChildCount; i++)
		{
			Widget val = ((Widget)this).Children[i].Children[0];
			Widget val2 = ((Widget)this).Children[i].Children[1];
			float num3 = val.Size.X + val.ScaledMarginLeft + val.ScaledMarginRight;
			float num4 = val2.Size.X + val2.ScaledMarginLeft + val2.ScaledMarginRight;
			if (num < num3)
			{
				num = num3;
			}
			if (num2 < num4)
			{
				num2 = num4;
			}
		}
		float num5 = 0.5f;
		if (num2 > 0f || num > 0f)
		{
			num5 = num2 / (num2 + num);
		}
		for (int j = 0; j < ((Widget)this).ChildCount; j++)
		{
			Widget val3 = ((Widget)this).Children[j].Children[0];
			Widget obj = ((Widget)this).Children[j].Children[1];
			val3.WidthSizePolicy = (SizePolicy)1;
			obj.WidthSizePolicy = (SizePolicy)0;
			obj.ScaledSuggestedWidth = ((Widget)this).Size.X * num5;
			obj.MinWidth = ((Widget)this).Size.X * 1f / 6f * ((Widget)this)._inverseScaleToUse;
			obj.MaxWidth = ((Widget)this).Size.X * 2f / 3f * ((Widget)this)._inverseScaleToUse;
			if (obj.IsHidden)
			{
				((BrushWidget)((val3 is TextWidget) ? val3 : null)).Brush.TextHorizontalAlignment = (TextHorizontalAlignment)2;
				((BrushWidget)((val3 is TextWidget) ? val3 : null)).Brush.TextColorFactor = 0.9f;
			}
		}
		_isDirty = false;
	}

	protected override void OnChildAdded(Widget child)
	{
		((Container)this).OnChildAdded(child);
		_isDirty = true;
	}

	protected override void OnAfterChildRemoved(Widget child, int previousIndexOfChild)
	{
		((Container)this).OnAfterChildRemoved(child, previousIndexOfChild);
		_isDirty = true;
	}
}
