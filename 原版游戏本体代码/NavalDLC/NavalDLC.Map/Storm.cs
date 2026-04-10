using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Map;

public class Storm
{
	public enum StormTypes
	{
		Storm,
		ThunderStorm,
		Hurricane
	}

	public struct PreviousData : ISavedStruct
	{
		[SaveableField(10)]
		public Vec2 Position;

		[SaveableField(20)]
		public float EffectRadius;

		public static PreviousData Invalid => new PreviousData(Vec2.Invalid, -1f);

		public PreviousData(Vec2 position, float effectRadius)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			Position = position;
			EffectRadius = effectRadius;
		}

		public bool IsDefault()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			if (Position == Vec2.Zero)
			{
				return EffectRadius == 0f;
			}
			return false;
		}

		public override string ToString()
		{
			return ((object)Unsafe.As<Vec2, Vec2>(ref Position)/*cast due to .constrained prefix*/).ToString() + ": " + EffectRadius;
		}
	}

	private const int PreviousPositionsCount = 6;

	private const int LastPositionUpdatePeriodInHours = 4;

	[SaveableField(10)]
	private Vec2 _currentPosition;

	[SaveableField(100)]
	private PreviousData[] _previousPositionsAndRadius = new PreviousData[6];

	[SaveableField(120)]
	private int _nextUpdatePreviousDataArrayIndex;

	[SaveableField(130)]
	private CampaignTime _nextUpdateTime;

	[SaveableField(20)]
	public readonly StormTypes StormType;

	[SaveableField(30)]
	private float _intensity;

	private float _speed;

	[SaveableField(50)]
	private CampaignTime _developingStateFinishCampaignTime;

	[SaveableField(60)]
	private CampaignTime _finalizingStateStartCampaignTime;

	[SaveableField(80)]
	private Vec2 _desiredMoveDirection;

	[SaveableField(90)]
	private Vec2 _currentMoveDirection;

	public bool IsActive
	{
		get
		{
			if (!IsInDevelopingState)
			{
				return !IsInFinalizingState;
			}
			return false;
		}
	}

	public Vec2 CurrentPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _currentPosition;
		}
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			_currentPosition = value;
			if (IsPositionOutOfMapBoundary(_currentPosition))
			{
				ForceDeactivate();
			}
			SetVisualDirty();
		}
	}

	public float Intensity
	{
		get
		{
			return _intensity;
		}
		set
		{
			_intensity = MBMath.ClampFloat(value, 0f, 1f);
			if (_intensity <= 0f)
			{
				ForceDeactivate();
			}
		}
	}

	public bool IsInDevelopingState => ((CampaignTime)(ref _developingStateFinishCampaignTime)).IsFuture;

	public bool IsInFinalizingState => ((CampaignTime)(ref _finalizingStateStartCampaignTime)).IsPast;

	public bool IsReadyToBeFinalized
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			CampaignTime val = _finalizingStateStartCampaignTime + NavalDLCManager.Instance.GameModels.MapStormModel.GetFinalizingStateDurationOfStorm(this);
			return ((CampaignTime)(ref val)).IsPast;
		}
	}

	public bool IsVisuallyDirty { get; private set; }

	public float EffectRadius => NavalDLCManager.Instance.GameModels.MapStormModel.GetEffectRadiusOfStorm(this);

	public float EyeRadius => NavalDLCManager.Instance.GameModels.MapStormModel.GetEyeRadiusOfStorm(this);

	public Storm(Vec2 initialPosition, StormTypes stormType)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		StormType = stormType;
		CurrentPosition = initialPosition;
		Intensity = 0.5f;
		_speed = NavalDLCManager.Instance.GameModels.MapStormModel.GetSpeedOfStorm(this);
		_developingStateFinishCampaignTime = CampaignTime.Now + NavalDLCManager.Instance.GameModels.MapStormModel.GetDevelopingStateDurationOfStorm(this);
		NavalDLCManager.Instance.GameModels.MapStormModel.GetStormLifeSpan(out var minimumDuration, out var maximumDuration);
		CampaignTime val = minimumDuration + CampaignTime.Days((float)MBRandom.RandomInt(0, (int)((CampaignTime)(ref maximumDuration)).ToDays));
		_finalizingStateStartCampaignTime = _developingStateFinishCampaignTime + val;
		ChangeMoveDirection();
		SetVisualDirty();
	}

	public void ForceDeactivate()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_finalizingStateStartCampaignTime = CampaignTime.Now;
		SetVisualDirty();
	}

	public void SetVisualDirty()
	{
		IsVisuallyDirty = true;
	}

	public void OnVisualUpdated()
	{
		IsVisuallyDirty = false;
	}

	public bool HasWetWeatherEffectAtPosition(Vec2 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Vec2 currentPosition = CurrentPosition;
		if (((Vec2)(ref currentPosition)).DistanceSquared(pos) < EffectRadius * EffectRadius * 1.2f)
		{
			return true;
		}
		int num = Math.Min(_nextUpdatePreviousDataArrayIndex, 6);
		for (int i = 0; i < num; i++)
		{
			PreviousData previousData = _previousPositionsAndRadius[i];
			if (((Vec2)(ref previousData.Position)).DistanceSquared(pos) < previousData.EffectRadius * previousData.EffectRadius * 1.2f)
			{
				return true;
			}
		}
		return false;
	}

	public void HourlyTick()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (IsActive && ((CampaignTime)(ref _nextUpdateTime)).IsPast)
		{
			_previousPositionsAndRadius[_nextUpdatePreviousDataArrayIndex] = new PreviousData(CurrentPosition, EffectRadius);
			_nextUpdatePreviousDataArrayIndex = (_nextUpdatePreviousDataArrayIndex + 1) % _previousPositionsAndRadius.Length;
			_nextUpdateTime = CampaignTime.HoursFromNow(4f);
		}
	}

	public void Tick(float dt)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (IsActive && !NavalDLCManager.Instance.StormManager.DebugVisualsStopped)
		{
			_currentMoveDirection = Vec2.Lerp(_currentMoveDirection, _desiredMoveDirection, dt);
			CurrentPosition += _currentMoveDirection * dt * _speed;
		}
	}

	public void OnAfterLoad()
	{
		_speed = NavalDLCManager.Instance.GameModels.MapStormModel.GetSpeedOfStorm(this);
		SetVisualDirty();
	}

	public void ChangeMoveDirection()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		_desiredMoveDirection = Campaign.Current.Models.MapWeatherModel.GetWindForPosition(new CampaignVec2(CurrentPosition, false));
		float num = MBRandom.RandomFloatNormal * 30f;
		((Vec2)(ref _desiredMoveDirection)).RotateCCW(num * (MathF.PI / 180f));
		((Vec2)(ref _desiredMoveDirection)).Normalize();
	}

	private bool IsPositionOutOfMapBoundary(Vec2 position)
	{
		Vec2 val = default(Vec2);
		Vec2 val2 = default(Vec2);
		float num = default(float);
		Campaign.Current.MapSceneWrapper.GetMapBorders(ref val, ref val2, ref num);
		if (((Vec2)(ref position)).X < ((Vec2)(ref val)).X || ((Vec2)(ref position)).X > ((Vec2)(ref val2)).X || ((Vec2)(ref position)).Y < ((Vec2)(ref val)).Y || ((Vec2)(ref position)).Y > ((Vec2)(ref val2)).Y)
		{
			return true;
		}
		return false;
	}

	[Conditional("DEBUG")]
	private void DebugVisualTick()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (NavalDLCManager.Instance.StormManager.DebugVisualsEnabled)
		{
			_ = IsActive;
			new Vec3(CurrentPosition, 5f, -1f);
			PreviousData[] previousPositionsAndRadius = _previousPositionsAndRadius;
			for (int i = 0; i < previousPositionsAndRadius.Length; i++)
			{
				_ = ref previousPositionsAndRadius[i];
			}
		}
	}
}
