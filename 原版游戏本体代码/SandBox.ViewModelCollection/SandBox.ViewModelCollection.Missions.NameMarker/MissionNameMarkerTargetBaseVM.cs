using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Missions.NameMarker;

public abstract class MissionNameMarkerTargetBaseVM : ViewModel
{
	private MBBindingList<QuestMarkerVM> _quests;

	private Vec2 _screenPosition;

	private int _distance;

	private string _name;

	private string _iconType = string.Empty;

	private string _nameType = string.Empty;

	private bool _isEnabled;

	private bool _isTracked;

	private bool _isQuestMainStory;

	private bool _isEnemy;

	private bool _isFriendly;

	private bool _isPersistent;

	[DataSourceProperty]
	public MBBindingList<QuestMarkerVM> Quests
	{
		get
		{
			return _quests;
		}
		set
		{
			if (value != _quests)
			{
				_quests = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<QuestMarkerVM>>(value, "Quests");
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
	public string IconType
	{
		get
		{
			return _iconType;
		}
		set
		{
			if (value != _iconType)
			{
				_iconType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "IconType");
			}
		}
	}

	[DataSourceProperty]
	public string NameType
	{
		get
		{
			return _nameType;
		}
		set
		{
			if (value != _nameType)
			{
				_nameType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NameType");
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
	public bool IsTracked
	{
		get
		{
			return _isTracked;
		}
		set
		{
			if (value != _isTracked)
			{
				_isTracked = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTracked");
			}
		}
	}

	[DataSourceProperty]
	public bool IsQuestMainStory
	{
		get
		{
			return _isQuestMainStory;
		}
		set
		{
			if (value != _isQuestMainStory)
			{
				_isQuestMainStory = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsQuestMainStory");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEnemy
	{
		get
		{
			return _isEnemy;
		}
		set
		{
			if (value != _isEnemy)
			{
				_isEnemy = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEnemy");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFriendly
	{
		get
		{
			return _isFriendly;
		}
		set
		{
			if (value != _isFriendly)
			{
				_isFriendly = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFriendly");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPersistent
	{
		get
		{
			return _isPersistent;
		}
		set
		{
			if (value != _isPersistent)
			{
				_isPersistent = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPersistent");
				if (IsPersistent)
				{
					SetEnabledState(enabled: true);
				}
				else if (!IsEnabled)
				{
					SetEnabledState(enabled: false);
				}
			}
		}
	}

	public MissionNameMarkerTargetBaseVM()
	{
		Quests = new MBBindingList<QuestMarkerVM>();
	}

	public abstract void UpdatePosition(Camera missionCamera);

	public abstract bool Equals(MissionNameMarkerTargetBaseVM other);

	protected abstract TextObject GetName();

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = ((object)GetName()).ToString();
	}

	protected void UpdatePositionWith(Camera missionCamera, Vec3 worldPosition)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		float num = -100f;
		float num2 = -100f;
		float num3 = 0f;
		MBWindowManager.WorldToScreenInsideUsableArea(missionCamera, worldPosition, ref num, ref num2, ref num3);
		if (num3 > 0f)
		{
			ScreenPosition = new Vec2(num, num2);
			Vec3 val = worldPosition - missionCamera.Position;
			Distance = (int)((Vec3)(ref val)).Length;
		}
		else
		{
			Distance = -1;
			ScreenPosition = new Vec2(-500f, -500f);
		}
	}

	public void SetEnabledState(bool enabled)
	{
		IsEnabled = IsPersistent || enabled;
	}
}
