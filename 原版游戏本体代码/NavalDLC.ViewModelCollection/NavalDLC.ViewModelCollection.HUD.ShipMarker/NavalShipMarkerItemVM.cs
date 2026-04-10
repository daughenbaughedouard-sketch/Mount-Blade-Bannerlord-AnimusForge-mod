using NavalDLC.Missions.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.FormationMarker;

namespace NavalDLC.ViewModelCollection.HUD.ShipMarker;

public class NavalShipMarkerItemVM : ViewModel
{
	public enum TeamTypes
	{
		PlayerTeam,
		PlayerAllyTeam,
		EnemyTeam
	}

	public readonly Formation Formation;

	public readonly MissionShip Ship;

	private readonly string _formationType;

	private readonly string _shipType;

	private int _teamType;

	private bool _isEnabled;

	private bool _isCenterOfFocus;

	private bool _isShipTargetRelevant;

	private bool _isTargetingAShip;

	private bool _showDistanceTexts;

	private int _size;

	private int _wSign;

	private float _distance;

	private string _distanceText;

	private string _markerType;

	private Vec2 _screenPosition;

	private int _crewCount;

	private float _hitPoints;

	private float _maxHitPoints;

	private bool _hasAnyTroops;

	[DataSourceProperty]
	public int TeamType
	{
		get
		{
			return _teamType;
		}
		set
		{
			if (value != _teamType)
			{
				_teamType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TeamType");
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
	public bool IsCenterOfFocus
	{
		get
		{
			return _isCenterOfFocus;
		}
		set
		{
			if (_isCenterOfFocus != value)
			{
				_isCenterOfFocus = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCenterOfFocus");
			}
		}
	}

	[DataSourceProperty]
	public bool IsShipTargetRelevant
	{
		get
		{
			return _isShipTargetRelevant;
		}
		set
		{
			if (_isShipTargetRelevant != value)
			{
				_isShipTargetRelevant = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsShipTargetRelevant");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTargetingAShip
	{
		get
		{
			return _isTargetingAShip;
		}
		set
		{
			if (_isTargetingAShip != value)
			{
				_isTargetingAShip = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTargetingAShip");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowDistanceTexts
	{
		get
		{
			return _showDistanceTexts;
		}
		set
		{
			if (_showDistanceTexts != value)
			{
				_showDistanceTexts = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowDistanceTexts");
			}
		}
	}

	[DataSourceProperty]
	public int Size
	{
		get
		{
			return _size;
		}
		set
		{
			if (value != _size)
			{
				_size = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Size");
			}
		}
	}

	[DataSourceProperty]
	public int WSign
	{
		get
		{
			return _wSign;
		}
		set
		{
			if (value != _wSign)
			{
				_wSign = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "WSign");
			}
		}
	}

	[DataSourceProperty]
	public float Distance
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
	public string DistanceText
	{
		get
		{
			return _distanceText;
		}
		set
		{
			if (value != _distanceText)
			{
				_distanceText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DistanceText");
			}
		}
	}

	[DataSourceProperty]
	public int CrewCount
	{
		get
		{
			return _crewCount;
		}
		set
		{
			if (value != _crewCount)
			{
				_crewCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CrewCount");
			}
		}
	}

	[DataSourceProperty]
	public string MarkerType
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
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MarkerType");
			}
		}
	}

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
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _screenPosition)
			{
				_screenPosition = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ScreenPosition");
			}
		}
	}

	[DataSourceProperty]
	public float HitPoints
	{
		get
		{
			return _hitPoints;
		}
		set
		{
			if (value != _hitPoints)
			{
				_hitPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HitPoints");
			}
		}
	}

	[DataSourceProperty]
	public float MaxHitPoints
	{
		get
		{
			return _maxHitPoints;
		}
		set
		{
			if (value != _maxHitPoints)
			{
				_maxHitPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MaxHitPoints");
			}
		}
	}

	[DataSourceProperty]
	public bool HasAnyTroops
	{
		get
		{
			return _hasAnyTroops;
		}
		set
		{
			if (value != _hasAnyTroops)
			{
				_hasAnyTroops = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasAnyTroops");
			}
		}
	}

	public NavalShipMarkerItemVM(Formation formation, MissionShip ship)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Formation = formation;
		Ship = ship;
		_formationType = MissionFormationMarkerTargetVM.GetFormationType(Formation.RepresentativeClass);
		MissionShip ship2 = Ship;
		_shipType = "Ship_" + ((ship2 != null) ? ((object)ship2.ShipOrigin.Hull.Type/*cast due to .constrained prefix*/).ToString() : null);
		if (Formation.Team.IsPlayerTeam)
		{
			TeamType = 0;
		}
		else if (Formation.Team.IsPlayerAlly)
		{
			TeamType = 1;
		}
		else
		{
			TeamType = 2;
		}
		Refresh();
	}

	public void Refresh()
	{
		Size = Formation.CountOfUnits;
		HasAnyTroops = Size > 0;
		MarkerType = (IsShipActive() ? _shipType : _formationType);
		HitPoints = (IsShipActive() ? Ship.HitPoints : 0f);
		MaxHitPoints = Ship?.MaxHealth ?? 1f;
	}

	public void SetTargetedState(bool isFocused, bool isTargetingAShip)
	{
		IsCenterOfFocus = isFocused;
		IsTargetingAShip = isTargetingAShip;
	}

	public bool IsShipActive()
	{
		if (Ship != null && !((MissionObject)Ship).IsDisabled && !Ship.IsSinking && !Ship.IsRemoved)
		{
			return Ship.HitPoints > 0f;
		}
		return false;
	}
}
