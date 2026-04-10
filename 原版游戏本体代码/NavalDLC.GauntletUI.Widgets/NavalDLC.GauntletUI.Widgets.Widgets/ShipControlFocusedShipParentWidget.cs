using System;
using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class ShipControlFocusedShipParentWidget : Widget
{
	private int _wSign;

	private Vec2 _position;

	[DataSourceProperty]
	public int WSign
	{
		get
		{
			return _wSign;
		}
		set
		{
			if (_wSign != value)
			{
				_wSign = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "WSign");
			}
		}
	}

	[DataSourceProperty]
	public Vec2 Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _position;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (_position != value)
			{
				_position = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "Position");
			}
		}
	}

	public ShipControlFocusedShipParentWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		((Widget)this).OnLateUpdate(dt);
		if (((Widget)this).IsVisible)
		{
			UpdateScreenPosition();
		}
	}

	private void UpdateScreenPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		Vec2 position = Position;
		float num = ((Vec2)(ref position)).X - ((Widget)this).Size.X / 2f;
		position = Position;
		float num2 = ((Vec2)(ref position)).X + ((Widget)this).Size.X / 2f;
		position = Position;
		float num3 = ((Vec2)(ref position)).Y - ((Widget)this).Size.Y;
		position = Position;
		float y = ((Vec2)(ref position)).Y;
		if (WSign <= 0 || !(num > 0f) || !(num2 < ((Widget)this).Context.EventManager.PageSize.X) || !(num3 > 0f) || !(y < ((Widget)this).Context.EventManager.PageSize.Y))
		{
			Vec2 val = default(Vec2);
			((Vec2)(ref val))._002Ector(num, num3);
			Vector2 vector = ((Widget)this).Context.EventManager.PageSize - ((Widget)this).Size;
			Vec2 val2 = Vec2.op_Implicit(vector / 2f);
			val -= val2;
			if (WSign < 0)
			{
				val *= -1f;
			}
			float num4 = Mathf.Atan2(val.y, val.x) - MathF.PI / 2f;
			float num5 = Mathf.Cos(num4);
			float num6 = Mathf.Sin(num4);
			float num7 = num5 / num6;
			Vec2 val3 = val2 * 1f;
			val = ((num5 > 0f) ? new Vec2((0f - val3.y) / num7, val2.y) : new Vec2(val3.y / num7, 0f - val2.y));
			if (val.x > val3.x)
			{
				((Vec2)(ref val))._002Ector(val3.x, (0f - val3.x) * num7);
			}
			else if (val.x < 0f - val3.x)
			{
				((Vec2)(ref val))._002Ector(0f - val3.x, val3.x * num7);
			}
			val += val2;
			((Widget)this).ScaledPositionXOffset = Mathf.Clamp(val.x, 0f, vector.X);
			((Widget)this).ScaledPositionYOffset = Mathf.Clamp(val.y, 0f, vector.Y);
		}
		else
		{
			((Widget)this).ScaledPositionXOffset = num;
			((Widget)this).ScaledPositionYOffset = num3;
		}
	}
}
