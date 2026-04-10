using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Missions.MainAgentDetection;

public class MissionLosingTargetVM : ViewModel
{
	private bool _isLosingTarget;

	private float _losingTargetRatio;

	private string _losingTargetWarningText;

	[DataSourceProperty]
	public bool IsLosingTarget
	{
		get
		{
			return _isLosingTarget;
		}
		set
		{
			if (value != _isLosingTarget)
			{
				_isLosingTarget = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsLosingTarget");
			}
		}
	}

	[DataSourceProperty]
	public float LosingTargetRatio
	{
		get
		{
			return _losingTargetRatio;
		}
		set
		{
			if (value != _losingTargetRatio)
			{
				_losingTargetRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "LosingTargetRatio");
			}
		}
	}

	[DataSourceProperty]
	public string LosingTargetWarningText
	{
		get
		{
			return _losingTargetWarningText;
		}
		set
		{
			if (value != _losingTargetWarningText)
			{
				_losingTargetWarningText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LosingTargetWarningText");
			}
		}
	}

	public MissionLosingTargetVM()
	{
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		LosingTargetWarningText = ((object)new TextObject("{=kXy4R7ca}You are about to lose the target.", (Dictionary<string, object>)null)).ToString();
	}

	public void UpdateLosingTargetValues(bool isLosingTarget, float losingTargetTimer, float losingTargetTreshold)
	{
		IsLosingTarget = isLosingTarget;
		LosingTargetRatio = MathF.Clamp(losingTargetTimer / losingTargetTreshold * 100f, 0f, 100f);
	}
}
