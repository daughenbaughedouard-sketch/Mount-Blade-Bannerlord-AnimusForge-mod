using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class ShipControlWindIndicatorParent : Widget
{
	private Widget _windHandle;

	private string _sailState;

	private Vec2 _projectedWindDirection;

	[Editor(false)]
	public Widget WindHandle
	{
		get
		{
			return _windHandle;
		}
		set
		{
			if (value != _windHandle)
			{
				_windHandle = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "WindHandle");
			}
		}
	}

	[Editor(false)]
	public string SailState
	{
		get
		{
			return _sailState;
		}
		set
		{
			if (value != _sailState)
			{
				_sailState = value;
				((PropertyOwnerObject)this).OnPropertyChanged<string>(value, "SailState");
				((Widget)this).SetState(value);
			}
		}
	}

	[Editor(false)]
	public Vec2 ProjectedWindDirection
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _projectedWindDirection;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _projectedWindDirection)
			{
				_projectedWindDirection = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "ProjectedWindDirection");
			}
		}
	}

	public ShipControlWindIndicatorParent(UIContext context)
		: base(context)
	{
	}

	protected override void OnUpdate(float dt)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((Widget)this).OnUpdate(dt);
		if (WindHandle != null)
		{
			Vec2 projectedWindDirection = ProjectedWindDirection;
			Vec2 val = ((Vec2)(ref projectedWindDirection)).Normalized();
			WindHandle.PivotX = 0.5f;
			WindHandle.PivotY = 0.5f;
			WindHandle.Rotation = Mathf.Atan2(val.x, val.y) * (180f / MathF.PI) - 90f;
		}
	}
}
