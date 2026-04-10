using System;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Missions;

public class MissionAgentAlarmTargetVM : ViewModel
{
	private enum AlarmStateEnum
	{
		Invalid = -1,
		None,
		Default,
		Cautious,
		PatrollingCautious,
		Alarmed
	}

	public readonly Agent TargetAgent;

	private readonly Action<MissionAgentAlarmTargetVM> _onRemove;

	private float _latestX;

	private float _latestY;

	private float _latestW;

	private float _wPosAfterPositionCalculation;

	private AlarmedBehaviorGroup _alarmedBehaviorGroupCache;

	private bool _isStealthModeEnabled;

	private bool _isMainAgentInVisibilityRange;

	private bool _isInVision;

	private bool _isSuspected;

	private string _alarmState;

	private int _wSign;

	private int _alarmProgress;

	private Vec2 _screenPosition;

	public bool HasCautiousness
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			if (!Extensions.HasAnyFlag<AIStateFlag>(TargetAgent.AIStateFlags, (AIStateFlag)3))
			{
				return AlarmedBehaviorGroup.AlarmFactor > 0f;
			}
			return true;
		}
	}

	public AlarmedBehaviorGroup AlarmedBehaviorGroup
	{
		get
		{
			if (_alarmedBehaviorGroupCache == null)
			{
				_alarmedBehaviorGroupCache = TargetAgent.GetComponent<CampaignAgentComponent>().AgentNavigator?.GetBehaviorGroup<AlarmedBehaviorGroup>();
			}
			return _alarmedBehaviorGroupCache;
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
	public bool IsMainAgentInVisibilityRange
	{
		get
		{
			return _isMainAgentInVisibilityRange;
		}
		set
		{
			if (value != _isMainAgentInVisibilityRange)
			{
				_isMainAgentInVisibilityRange = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMainAgentInVisibilityRange");
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
	public bool IsSuspected
	{
		get
		{
			return _isSuspected;
		}
		set
		{
			if (value != _isSuspected)
			{
				_isSuspected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSuspected");
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

	public MissionAgentAlarmTargetVM(Agent agent, Action<MissionAgentAlarmTargetVM> onRemove)
	{
		TargetAgent = agent;
		_onRemove = onRemove;
	}

	public void UpdateValues()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		string agentAlarmState = GetAgentAlarmState(TargetAgent.AIStateFlags);
		float num = AlarmedBehaviorGroup?.AlarmFactor ?? 0f;
		if (num > 1f)
		{
			num = MathF.Min(num, 2f);
			num -= 1f;
			num = MathF.Lerp(0.3f, 1f, num, 1E-05f);
		}
		if (!IsInVision || !IsStealthModeEnabled || (!((float)AlarmProgress > 0f) && !IsMainAgentInVisibilityRange))
		{
			AlarmProgress = 0;
			AlarmState = AlarmStateEnum.Invalid.ToString();
		}
		else
		{
			AlarmState = agentAlarmState;
			AlarmProgress = (int)(num * 100f);
		}
	}

	private static string GetAgentAlarmState(AIStateFlag stateFlag)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		if ((stateFlag & 3) == 3)
		{
			return AlarmStateEnum.Alarmed.ToString();
		}
		if ((stateFlag & 3) == 1)
		{
			return AlarmStateEnum.Cautious.ToString();
		}
		if ((stateFlag & 3) == 2)
		{
			return AlarmStateEnum.PatrollingCautious.ToString();
		}
		return AlarmStateEnum.None.ToString();
	}

	public void UpdateScreenPosition(Camera missionCamera)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = TargetAgent.Position;
		position.z += TargetAgent.GetEyeGlobalHeight() + 0.35f;
		_latestX = 0f;
		_latestY = 0f;
		_latestW = 0f;
		MBWindowManager.WorldToScreenInsideUsableArea(missionCamera, position, ref _latestX, ref _latestY, ref _latestW);
		_wPosAfterPositionCalculation = ((_latestW < 0f) ? (-1f) : 1.1f);
		WSign = (int)_wPosAfterPositionCalculation;
		ScreenPosition = new Vec2(_latestX, _latestY);
		_ = WSign;
		_ = 0;
	}

	public void ExecuteRemove()
	{
		_onRemove?.Invoke(this);
	}
}
