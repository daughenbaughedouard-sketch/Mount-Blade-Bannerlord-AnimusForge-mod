using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionLogics;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Missions.MainAgentDetection;

public class MissionDisguiseMarkerItemVM : ViewModel
{
	public enum AgentAlarmStateEnum
	{
		None = -1,
		Alarmed,
		Cautious,
		PatrollingCautious,
		Suspicious,
		Visible
	}

	public enum AgentStealthOffenseType
	{
		None = -1,
		Default,
		Visible,
		Suspicious
	}

	private Camera _missionCamera;

	private AgentAlarmStateEnum _activeAlarmState;

	private AgentStealthOffenseType _offenseType;

	private Vec2 _screenPosition;

	private int _alarmProgress;

	private string _alarmState;

	private string _offenseTypeIdentifier;

	private bool _isStealthModeEnabled;

	private bool _isSuspicious;

	private bool _isTarget;

	private bool _isInVision;

	private bool _isInVisibilityRange;

	public DisguiseMissionLogic.ShadowingAgentOffenseInfo OffenseInfo { get; }

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
	public int AlarmProgress
	{
		get
		{
			return _alarmProgress;
		}
		set
		{
			if (value != _alarmProgress)
			{
				_alarmProgress = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AlarmProgress");
			}
		}
	}

	[DataSourceProperty]
	public string AlarmState
	{
		get
		{
			return _alarmState;
		}
		set
		{
			if (value != _alarmState)
			{
				_alarmState = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AlarmState");
			}
		}
	}

	[DataSourceProperty]
	public string OffenseTypeIdentifier
	{
		get
		{
			return _offenseTypeIdentifier;
		}
		set
		{
			if (value != _offenseTypeIdentifier)
			{
				_offenseTypeIdentifier = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OffenseTypeIdentifier");
			}
		}
	}

	[DataSourceProperty]
	public bool IsStealthModeEnabled
	{
		get
		{
			return _isStealthModeEnabled;
		}
		set
		{
			if (value != _isStealthModeEnabled)
			{
				_isStealthModeEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsStealthModeEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSuspicious
	{
		get
		{
			return _isSuspicious;
		}
		set
		{
			if (value != _isSuspicious)
			{
				_isSuspicious = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSuspicious");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTarget
	{
		get
		{
			return _isTarget;
		}
		set
		{
			if (value != _isTarget)
			{
				_isTarget = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTarget");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInVision
	{
		get
		{
			return _isInVision;
		}
		set
		{
			if (value != _isInVision)
			{
				_isInVision = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInVision");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInVisibilityRange
	{
		get
		{
			return _isInVisibilityRange;
		}
		set
		{
			if (value != _isInVisibilityRange)
			{
				_isInVisibilityRange = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInVisibilityRange");
			}
		}
	}

	public MissionDisguiseMarkerItemVM(Camera missionCamera, DisguiseMissionLogic.ShadowingAgentOffenseInfo offenseInfo)
	{
		_missionCamera = missionCamera;
		OffenseInfo = offenseInfo;
	}

	public void RefreshVisuals()
	{
		OffenseTypeIdentifier = GetOffenseTypeIdentifier(OffenseInfo?.OffenseType ?? StealthOffenseTypes.None);
		UpdateAlarmState();
	}

	public void UpdatePosition()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		Vec3 position = OffenseInfo.Agent.Position;
		position.z += OffenseInfo.Agent.GetEyeGlobalHeight() + 0.35f;
		if (((Vec3)(ref position)).IsValid)
		{
			MBWindowManager.WorldToScreenInsideUsableArea(_missionCamera, position, ref num, ref num2, ref num3);
		}
		if (!((Vec3)(ref position)).IsValid || num3 < 0f || !MathF.IsValidValue(num) || !MathF.IsValidValue(num2))
		{
			num = -10000f;
			num2 = -10000f;
			num3 = 0f;
		}
		ScreenPosition = new Vec2(num, num2);
	}

	private void UpdateAlarmState()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Agent agent = OffenseInfo.Agent;
		AlarmedBehaviorGroup alarmedBehaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator?.GetBehaviorGroup<AlarmedBehaviorGroup>();
		AIStateFlag aIStateFlags = agent.AIStateFlags;
		if (Extensions.HasAnyFlag<AIStateFlag>(aIStateFlags, (AIStateFlag)3))
		{
			_activeAlarmState = AgentAlarmStateEnum.Alarmed;
		}
		else if (Extensions.HasAnyFlag<AIStateFlag>(aIStateFlags, (AIStateFlag)1))
		{
			_activeAlarmState = AgentAlarmStateEnum.Cautious;
		}
		else if (Extensions.HasAnyFlag<AIStateFlag>(aIStateFlags, (AIStateFlag)2))
		{
			_activeAlarmState = AgentAlarmStateEnum.PatrollingCautious;
		}
		else
		{
			_activeAlarmState = AgentAlarmStateEnum.None;
		}
		float num = ((!Extensions.HasAnyFlag<AIStateFlag>(aIStateFlags, (AIStateFlag)3)) ? MathF.Clamp(alarmedBehaviorGroup.AlarmFactor / 2f, 0f, 1f) : 1f);
		AlarmState = _activeAlarmState.ToString();
		AlarmProgress = (int)(num * 100f);
	}

	private string GetOffenseTypeIdentifier(StealthOffenseTypes offenseType)
	{
		if (IsStealthModeEnabled || !IsInVision || !IsInVisibilityRange)
		{
			_offenseType = AgentStealthOffenseType.None;
			return _offenseType.ToString();
		}
		switch (offenseType)
		{
		case StealthOffenseTypes.None:
			_offenseType = AgentStealthOffenseType.Default;
			break;
		case StealthOffenseTypes.IsVisible:
			_offenseType = ((!IsSuspicious) ? AgentStealthOffenseType.Visible : AgentStealthOffenseType.Suspicious);
			break;
		case StealthOffenseTypes.IsInPersonalZone:
			_offenseType = AgentStealthOffenseType.Suspicious;
			break;
		}
		return _offenseType.ToString();
	}
}
