using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Map;

public class MapEventVisualItemVM : ViewModel
{
	private Camera _mapCamera;

	private bool _isAVisibleEvent;

	private CampaignVec2 _mapEventPositionCache;

	private const float CameraDistanceCutoff = 200f;

	private Vec2 _bindPosition;

	private bool _bindIsVisibleOnMap;

	private float _latestX;

	private float _latestY;

	private float _latestW;

	private Vec2 _position;

	private int _eventType;

	private bool _isVisibleOnMap;

	public MapEvent MapEvent { get; private set; }

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
				((ViewModel)this).OnPropertyChangedWithValue(value, "Position");
			}
		}
	}

	public int EventType
	{
		get
		{
			return _eventType;
		}
		set
		{
			if (_eventType != value)
			{
				_eventType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "EventType");
			}
		}
	}

	public bool IsVisibleOnMap
	{
		get
		{
			return _isVisibleOnMap;
		}
		set
		{
			if (_isVisibleOnMap != value)
			{
				_isVisibleOnMap = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsVisibleOnMap");
			}
		}
	}

	public MapEventVisualItemVM(Camera mapCamera, MapEvent mapEvent)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		_mapCamera = mapCamera;
		MapEvent = mapEvent;
		_mapEventPositionCache = mapEvent.Position;
	}

	public void UpdateProperties()
	{
		EventType = (int)SandBoxUIHelper.GetMapEventVisualTypeFromMapEvent(MapEvent);
		_isAVisibleEvent = MapEvent.IsVisible;
	}

	public void ParallelUpdatePosition()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		_latestX = 0f;
		_latestY = 0f;
		_latestW = 0f;
		if (_mapEventPositionCache != MapEvent.Position)
		{
			_mapEventPositionCache = MapEvent.Position;
		}
		MBWindowManager.WorldToScreenInsideUsableArea(_mapCamera, ((CampaignVec2)(ref _mapEventPositionCache)).AsVec3() + new Vec3(0f, 0f, 1.5f, -1f), ref _latestX, ref _latestY, ref _latestW);
		_bindPosition = new Vec2(_latestX, _latestY);
	}

	public void DetermineIsVisibleOnMap()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_bindIsVisibleOnMap = _latestW > 0f && _mapCamera.Position.z < 200f && _isAVisibleEvent;
	}

	public void UpdateBindingProperties()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Position = _bindPosition;
		IsVisibleOnMap = _bindIsVisibleOnMap;
	}
}
