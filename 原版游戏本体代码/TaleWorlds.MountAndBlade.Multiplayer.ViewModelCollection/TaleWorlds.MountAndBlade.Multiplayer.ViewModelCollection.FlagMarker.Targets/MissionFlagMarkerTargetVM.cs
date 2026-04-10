using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Objects;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;

public class MissionFlagMarkerTargetVM : MissionMarkerTargetVM
{
	private bool _isKeepFlag;

	private bool _isSpawnAffectorFlag;

	private float _flagProgress;

	private int _remainingRemovalTime = -1;

	public FlagCapturePoint TargetFlag { get; private set; }

	public override Vec3 WorldPosition
	{
		get
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (TargetFlag != null)
			{
				return TargetFlag.Position;
			}
			Debug.FailedAssert("No target found!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\FlagMarker\\Targets\\MissionFlagMarkerTargetVM.cs", "WorldPosition", 24);
			return Vec3.One;
		}
	}

	protected override float HeightOffset => 2f;

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

	public MissionFlagMarkerTargetVM(FlagCapturePoint flag)
		: base(MissionMarkerType.Flag)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		TargetFlag = flag;
		base.Name = Convert.ToChar(flag.FlagChar).ToString();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)TargetFlag).GameEntity;
		string[] tags = ((WeakGameEntity)(ref gameEntity)).Tags;
		foreach (string text in tags)
		{
			if (text.StartsWith("enable_") || text.StartsWith("disable_"))
			{
				IsSpawnAffectorFlag = true;
			}
		}
		gameEntity = ((ScriptComponentBehavior)TargetFlag).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).HasTag("keep_capture_point"))
		{
			IsKeepFlag = true;
		}
		OnOwnerChanged(null);
	}

	private Vec3 Vector3Maxamize(Vec3 vector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		num = ((vector.x > num) ? vector.x : num);
		num = ((vector.y > num) ? vector.y : num);
		num = ((vector.z > num) ? vector.z : num);
		return vector / num;
	}

	public override void UpdateScreenPosition(Camera missionCamera)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		Vec3 worldPosition = WorldPosition;
		worldPosition.z += HeightOffset;
		Vec3 val = missionCamera.WorldPointToViewPortPoint(ref worldPosition);
		val.y = 1f - val.y;
		if (val.z < 0f)
		{
			val.x = 1f - val.x;
			val.y = 1f - val.y;
			val.z = 0f;
			val = Vector3Maxamize(val);
		}
		if (float.IsPositiveInfinity(val.x))
		{
			val.x = 1f;
		}
		else if (float.IsNegativeInfinity(val.x))
		{
			val.x = 0f;
		}
		if (float.IsPositiveInfinity(val.y))
		{
			val.y = 1f;
		}
		else if (float.IsNegativeInfinity(val.y))
		{
			val.y = 0f;
		}
		val.x = MathF.Clamp(val.x, 0f, 1f) * Screen.RealScreenResolutionWidth;
		val.y = MathF.Clamp(val.y, 0f, 1f) * Screen.RealScreenResolutionHeight;
		base.ScreenPosition = new Vec2(val.x, val.y);
		FlagProgress = TargetFlag.GetFlagProgress();
	}

	public void OnOwnerChanged(Team team)
	{
		int num;
		int num2;
		if (team != null)
		{
			num = ((team.TeamIndex == -1) ? 1 : 0);
			if (num == 0)
			{
				num2 = (int)team.Color;
				goto IL_001f;
			}
		}
		else
		{
			num = 1;
		}
		num2 = -10855846;
		goto IL_001f;
		IL_001f:
		uint color = (uint)num2;
		uint color2 = ((num != 0) ? uint.MaxValue : team.Color2);
		RefreshColor(color, color2);
	}

	public void OnRemainingMoraleChanged(int remainingMorale)
	{
		if (RemainingRemovalTime != remainingMorale && remainingMorale != 90)
		{
			RemainingRemovalTime = (int)((float)remainingMorale / 1f);
		}
	}
}
