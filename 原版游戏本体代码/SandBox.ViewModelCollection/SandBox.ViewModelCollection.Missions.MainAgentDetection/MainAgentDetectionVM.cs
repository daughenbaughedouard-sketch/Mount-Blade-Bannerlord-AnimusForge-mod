using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Missions.MainAgentDetection;

public class MainAgentDetectionVM : ViewModel
{
	private bool _hasDetection;

	private bool _hasReachedSuspicionTreshold;

	private float _minimumDetectionLevel;

	private float _maximumDetectionLevel;

	private float _currentDetectionLevel;

	private float _currentDetectionLevelRatio;

	private string _suspicionFullText;

	[DataSourceProperty]
	public bool HasDetection
	{
		get
		{
			return _hasDetection;
		}
		set
		{
			if (value != _hasDetection)
			{
				_hasDetection = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasDetection");
			}
		}
	}

	[DataSourceProperty]
	public bool HasReachedSuspicionTreshold
	{
		get
		{
			return _hasReachedSuspicionTreshold;
		}
		set
		{
			if (value != _hasReachedSuspicionTreshold)
			{
				_hasReachedSuspicionTreshold = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasReachedSuspicionTreshold");
			}
		}
	}

	[DataSourceProperty]
	public float MinimumDetectionLevel
	{
		get
		{
			return _minimumDetectionLevel;
		}
		set
		{
			if (value != _minimumDetectionLevel)
			{
				_minimumDetectionLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MinimumDetectionLevel");
			}
		}
	}

	[DataSourceProperty]
	public float MaximumDetectionLevel
	{
		get
		{
			return _maximumDetectionLevel;
		}
		set
		{
			if (value != _maximumDetectionLevel)
			{
				_maximumDetectionLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MaximumDetectionLevel");
			}
		}
	}

	[DataSourceProperty]
	public float CurrentDetectionLevel
	{
		get
		{
			return _currentDetectionLevel;
		}
		set
		{
			if (value != _currentDetectionLevel)
			{
				_currentDetectionLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CurrentDetectionLevel");
			}
		}
	}

	[DataSourceProperty]
	public float CurrentDetectionLevelRatio
	{
		get
		{
			return _currentDetectionLevelRatio;
		}
		set
		{
			if (value != _currentDetectionLevelRatio)
			{
				_currentDetectionLevelRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CurrentDetectionLevelRatio");
			}
		}
	}

	[DataSourceProperty]
	public string SuspicionFullText
	{
		get
		{
			return _suspicionFullText;
		}
		set
		{
			if (value != _suspicionFullText)
			{
				_suspicionFullText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SuspicionFullText");
			}
		}
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		SuspicionFullText = ((object)new TextObject("{=KgTFCWG8}You are suspicious", (Dictionary<string, object>)null)).ToString();
	}

	public void UpdateDetectionValues(float minDetectionLevel, float maxDetectionLevel, float currentDetectionLevel)
	{
		MinimumDetectionLevel = minDetectionLevel;
		MaximumDetectionLevel = maxDetectionLevel;
		CurrentDetectionLevel = currentDetectionLevel;
		CurrentDetectionLevelRatio = MBMath.InverseLerp(MinimumDetectionLevel, MaximumDetectionLevel, CurrentDetectionLevel);
		HasDetection = CurrentDetectionLevel > 0f;
		HasReachedSuspicionTreshold = CurrentDetectionLevel >= MaximumDetectionLevel;
	}
}
