using TaleWorlds.Library;

namespace NavalDLC.ViewModelCollection.Missions.ShipControl;

public class MissionHitPointPropertiesVM : ViewModel
{
	private bool _isRelevant;

	private int _activeHitPoints;

	private int _maxHitPoints;

	[DataSourceProperty]
	public bool IsRelevant
	{
		get
		{
			return _isRelevant;
		}
		set
		{
			if (value != _isRelevant)
			{
				_isRelevant = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRelevant");
			}
		}
	}

	[DataSourceProperty]
	public int ActiveHitPoints
	{
		get
		{
			return _activeHitPoints;
		}
		set
		{
			if (value != _activeHitPoints)
			{
				_activeHitPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ActiveHitPoints");
			}
		}
	}

	[DataSourceProperty]
	public int MaxHitPoints
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
}
