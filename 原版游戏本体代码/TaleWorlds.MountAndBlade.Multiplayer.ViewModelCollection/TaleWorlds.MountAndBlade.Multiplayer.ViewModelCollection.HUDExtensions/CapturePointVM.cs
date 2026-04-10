using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.Compass;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;

public class CapturePointVM : CompassTargetVM
{
	public readonly FlagCapturePoint Target;

	private float _flagProgress;

	private int _remainingRemovalTime = -1;

	private bool _isKeepFlag;

	private bool _isSpawnAffectorFlag;

	[DataSourceProperty]
	public float FlagProgress
	{
		get
		{
			return _flagProgress;
		}
		set
		{
			if (value != _flagProgress)
			{
				_flagProgress = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "FlagProgress");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSpawnAffectorFlag
	{
		get
		{
			return _isSpawnAffectorFlag;
		}
		set
		{
			if (value != _isSpawnAffectorFlag)
			{
				_isSpawnAffectorFlag = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSpawnAffectorFlag");
			}
		}
	}

	[DataSourceProperty]
	public bool IsKeepFlag
	{
		get
		{
			return _isKeepFlag;
		}
		set
		{
			if (value != _isKeepFlag)
			{
				_isKeepFlag = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsKeepFlag");
			}
		}
	}

	[DataSourceProperty]
	public int RemainingRemovalTime
	{
		get
		{
			return _remainingRemovalTime;
		}
		set
		{
			if (value != _remainingRemovalTime)
			{
				_remainingRemovalTime = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "RemainingRemovalTime");
			}
		}
	}

	public CapturePointVM(FlagCapturePoint target, TargetIconType iconType)
		: base(iconType, 0u, 0u, (Banner)null, false, false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Target = target;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)Target).GameEntity;
		string[] tags = ((WeakGameEntity)(ref gameEntity)).Tags;
		foreach (string text in tags)
		{
			if (text.StartsWith("enable_") || text.StartsWith("disable_"))
			{
				IsSpawnAffectorFlag = true;
			}
		}
		gameEntity = ((ScriptComponentBehavior)Target).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).HasTag("keep_capture_point"))
		{
			IsKeepFlag = true;
		}
		ResetFlag();
	}

	public override void Refresh(float circleX, float x, float distance)
	{
		((CompassTargetVM)this).Refresh(circleX, x, distance);
		FlagProgress = Target.GetFlagProgress();
	}

	public void OnOwnerChanged(Team newTeam)
	{
		uint num = ((newTeam != null) ? newTeam.Color : 4284111450u);
		uint num2 = ((newTeam != null) ? newTeam.Color2 : uint.MaxValue);
		((CompassTargetVM)this).RefreshColor(num, num2);
	}

	public void ResetFlag()
	{
		OnOwnerChanged(null);
	}

	internal void OnRemainingMoraleChanged(int remainingMorale)
	{
		if (RemainingRemovalTime != remainingMorale && remainingMorale != 90)
		{
			RemainingRemovalTime = (int)((float)remainingMorale / 1f);
		}
	}
}
