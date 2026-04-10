using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Nameplate;

public class NameplateVM : ViewModel
{
	protected enum NameplateSize
	{
		Small,
		Normal,
		Big
	}

	protected bool _bindIsTargetedByTutorial;

	private Vec2 _position;

	private bool _isVisibleOnMap;

	private string _factionColor;

	private bool _isTargetedByTutorial;

	private float _distanceToCamera;

	private bool _canParley;

	public double Scale { get; set; }

	public int NameplateOrder { get; set; }

	public string FactionColor
	{
		get
		{
			return _factionColor;
		}
		set
		{
			if (value != _factionColor)
			{
				_factionColor = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FactionColor");
			}
		}
	}

	public float DistanceToCamera
	{
		get
		{
			return _distanceToCamera;
		}
		set
		{
			if (value != _distanceToCamera)
			{
				_distanceToCamera = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "DistanceToCamera");
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
			if (value != _isVisibleOnMap)
			{
				_isVisibleOnMap = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsVisibleOnMap");
			}
		}
	}

	public bool IsTargetedByTutorial
	{
		get
		{
			return _isTargetedByTutorial;
		}
		set
		{
			if (value != _isTargetedByTutorial)
			{
				_isTargetedByTutorial = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTargetedByTutorial");
				((ViewModel)this).OnPropertyChanged("ShouldShowFullName");
				((ViewModel)this).OnPropertyChanged("IsTracked");
			}
		}
	}

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

	public bool CanParley
	{
		get
		{
			return _canParley;
		}
		set
		{
			if (value != _canParley)
			{
				_canParley = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanParley");
			}
		}
	}

	protected void OnTutorialNotificationElementChanged(TutorialNotificationElementChangeEvent obj)
	{
		RefreshTutorialStatus(((obj != null) ? obj.NewNotificationElementID : null) ?? string.Empty);
	}

	public virtual void RefreshDynamicProperties(bool forceUpdate)
	{
	}

	public virtual void RefreshPosition()
	{
	}

	public virtual void RefreshRelationStatus()
	{
	}

	public virtual void RefreshTutorialStatus(string newTutorialHighlightElementID)
	{
	}
}
