using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;

public abstract class MissionMarkerTargetVM : ViewModel
{
	public readonly MissionMarkerType MissionMarkerType;

	private Vec2 _screenPosition;

	private int _distance;

	private string _name;

	private bool _isEnabled;

	private string _color;

	private string _color2;

	private int _markerType;

	private string _visualState;

	public abstract Vec3 WorldPosition { get; }

	protected abstract float HeightOffset { get; }

	[DataSourceProperty]
	public Vec2 ScreenPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _screenPosition;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (value.x != _screenPosition.x || value.y != _screenPosition.y)
			{
				_screenPosition = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ScreenPosition");
			}
		}
	}

	[DataSourceProperty]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public int Distance
	{
		get
		{
			return _distance;
		}
		set
		{
			if (value != _distance)
			{
				_distance = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Distance");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public string Color
	{
		get
		{
			return _color;
		}
		set
		{
			if (value != _color)
			{
				_color = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Color");
			}
		}
	}

	[DataSourceProperty]
	public string Color2
	{
		get
		{
			return _color2;
		}
		set
		{
			if (value != _color2)
			{
				_color2 = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Color2");
			}
		}
	}

	[DataSourceProperty]
	public int MarkerType
	{
		get
		{
			return _markerType;
		}
		set
		{
			if (value != _markerType)
			{
				_markerType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MarkerType");
			}
		}
	}

	[DataSourceProperty]
	public string VisualState
	{
		get
		{
			return _visualState;
		}
		set
		{
			if (value != _visualState)
			{
				_visualState = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "VisualState");
			}
		}
	}

	public MissionMarkerTargetVM(MissionMarkerType markerType)
	{
		MissionMarkerType = markerType;
		MarkerType = (int)markerType;
	}

	public virtual void UpdateScreenPosition(Camera missionCamera)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		float num = -100f;
		float num2 = -100f;
		float num3 = 0f;
		Vec3 worldPosition = WorldPosition;
		worldPosition.z += HeightOffset;
		MBWindowManager.WorldToScreenInsideUsableArea(missionCamera, worldPosition, ref num, ref num2, ref num3);
		if (num3 > 0f)
		{
			ScreenPosition = new Vec2(num, num2);
			Vec3 val = WorldPosition - missionCamera.Position;
			Distance = (int)((Vec3)(ref val)).Length;
		}
		else
		{
			Distance = -1;
			ScreenPosition = new Vec2(-100f, -100f);
		}
	}

	protected void RefreshColor(uint color, uint color2)
	{
		if (color != 0)
		{
			string text = color.ToString("X");
			char c = text[0];
			char c2 = text[1];
			text = text.Remove(0, 2);
			text = Extensions.Add(text, c.ToString() + c2, false);
			Color = "#" + text;
		}
		else
		{
			Color = "#FFFFFFFF";
		}
		if (color2 != 0)
		{
			string text2 = color2.ToString("X");
			char c3 = text2[0];
			char c4 = text2[1];
			text2 = text2.Remove(0, 2);
			text2 = Extensions.Add(text2, c3.ToString() + c4, false);
			Color2 = "#" + text2;
		}
		else
		{
			Color2 = "#FFFFFFFF";
		}
	}
}
