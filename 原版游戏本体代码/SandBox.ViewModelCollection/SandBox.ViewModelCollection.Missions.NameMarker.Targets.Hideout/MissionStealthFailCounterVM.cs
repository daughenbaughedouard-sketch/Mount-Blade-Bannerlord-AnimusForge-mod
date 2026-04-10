using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets.Hideout;

public class MissionStealthFailCounterVM : ViewModel
{
	private TextObject _countDownTextObject;

	private float _failCounterElapsedTime;

	private string _countDownText;

	private float _failCounterMaxTime;

	private bool _isCounterActive;

	[DataSourceProperty]
	public string CountDownText
	{
		get
		{
			return _countDownText;
		}
		set
		{
			if (value != _countDownText)
			{
				_countDownText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CountDownText");
			}
		}
	}

	[DataSourceProperty]
	public float FailCounterElapsedTime
	{
		get
		{
			return _failCounterElapsedTime;
		}
		set
		{
			if (value != _failCounterElapsedTime)
			{
				_failCounterElapsedTime = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "FailCounterElapsedTime");
			}
		}
	}

	[DataSourceProperty]
	public float FailCounterMaxTime
	{
		get
		{
			return _failCounterMaxTime;
		}
		set
		{
			if (value != _failCounterMaxTime)
			{
				_failCounterMaxTime = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "FailCounterMaxTime");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCounterActive
	{
		get
		{
			return _isCounterActive;
		}
		set
		{
			if (value != _isCounterActive)
			{
				_isCounterActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCounterActive");
			}
		}
	}

	public MissionStealthFailCounterVM()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		_countDownTextObject = new TextObject("{=pY8lnL11}Mission will fail in: {SEC}", (Dictionary<string, object>)null);
	}

	public void UpdateFailCounter(float failCounterElapsedTime, float failCounterMaxTime, bool isStealthFailCounterMissionLogicActive)
	{
		IsCounterActive = !BannerlordConfig.HideBattleUI && !MBCommon.IsPaused && isStealthFailCounterMissionLogicActive && failCounterElapsedTime > 0f;
		FailCounterMaxTime = failCounterMaxTime;
		if (IsCounterActive)
		{
			FailCounterElapsedTime = FailCounterMaxTime - failCounterElapsedTime;
			_countDownTextObject.SetTextVariable("SEC", MathF.Ceiling(FailCounterElapsedTime));
			CountDownText = ((object)_countDownTextObject).ToString();
		}
	}
}
