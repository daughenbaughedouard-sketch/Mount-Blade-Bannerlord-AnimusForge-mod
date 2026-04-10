using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Tutorial;

public class TutorialItemVM : ViewModel
{
	public enum ItemPlacements
	{
		Left,
		Right,
		Top,
		Bottom,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		Center
	}

	private const string ControllerIdentificationModifier = "_controller";

	private string _tutorialTypeId;

	private Action _onFinishTutorial;

	private string _titleText;

	private string _descriptionText;

	private ImageIdentifierVM _centerImage;

	private string _soundId;

	private string _stepCountText;

	private string _tutorialsEnabledText;

	private string _tutorialTitleText;

	private bool _isEnabled;

	private bool _requiresMouse;

	private HintViewModel _disableCurrentTutorialHint;

	private HintViewModel _disableAllTutorialsHint;

	private bool _areTutorialsEnabled;

	public Action<bool> SetIsActive { get; private set; }

	[DataSourceProperty]
	public HintViewModel DisableCurrentTutorialHint
	{
		get
		{
			return _disableCurrentTutorialHint;
		}
		set
		{
			if (value != _disableCurrentTutorialHint)
			{
				_disableCurrentTutorialHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "DisableCurrentTutorialHint");
			}
		}
	}

	[DataSourceProperty]
	public bool AreTutorialsEnabled
	{
		get
		{
			return _areTutorialsEnabled;
		}
		set
		{
			if (value != _areTutorialsEnabled)
			{
				_areTutorialsEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AreTutorialsEnabled");
				BannerlordConfig.EnableTutorialHints = value;
			}
		}
	}

	[DataSourceProperty]
	public string TutorialsEnabledText
	{
		get
		{
			return _tutorialsEnabledText;
		}
		set
		{
			if (value != _tutorialsEnabledText)
			{
				_tutorialsEnabledText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TutorialsEnabledText");
			}
		}
	}

	[DataSourceProperty]
	public string TutorialTitleText
	{
		get
		{
			return _tutorialTitleText;
		}
		set
		{
			if (value != _tutorialTitleText)
			{
				_tutorialTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TutorialTitleText");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel DisableAllTutorialsHint
	{
		get
		{
			return _disableAllTutorialsHint;
		}
		set
		{
			if (value != _disableAllTutorialsHint)
			{
				_disableAllTutorialsHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "DisableAllTutorialsHint");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string StepCountText
	{
		get
		{
			return _stepCountText;
		}
		set
		{
			if (value != _stepCountText)
			{
				_stepCountText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "StepCountText");
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
	public string DescriptionText
	{
		get
		{
			return _descriptionText;
		}
		set
		{
			if (value != _descriptionText)
			{
				_descriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public string SoundId
	{
		get
		{
			return _soundId;
		}
		set
		{
			if (value != _soundId)
			{
				_soundId = value;
				((ViewModel)this).OnPropertyChanged("SoundId");
			}
		}
	}

	[DataSourceProperty]
	public ImageIdentifierVM CenterImage
	{
		get
		{
			return _centerImage;
		}
		set
		{
			if (value != _centerImage)
			{
				_centerImage = value;
				((ViewModel)this).OnPropertyChanged("CenterImage");
			}
		}
	}

	[DataSourceProperty]
	public bool RequiresMouse
	{
		get
		{
			return _requiresMouse;
		}
		set
		{
			if (value != _requiresMouse)
			{
				_requiresMouse = value;
				((ViewModel)this).OnPropertyChanged("RequiresMouse");
			}
		}
	}

	public TutorialItemVM()
	{
		IsEnabled = false;
	}

	public void Init(string tutorialTypeId, bool requiresMouse, Action onFinishTutorial)
	{
		IsEnabled = false;
		StepCountText = "DISABLED";
		RequiresMouse = requiresMouse;
		IsEnabled = true;
		_onFinishTutorial = onFinishTutorial;
		_tutorialTypeId = tutorialTypeId;
		AreTutorialsEnabled = BannerlordConfig.EnableTutorialHints;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		DisableCurrentTutorialHint = new HintViewModel(GameTexts.FindText("str_disable_current_tutorial_step", (string)null), (string)null);
		DisableAllTutorialsHint = new HintViewModel(GameTexts.FindText("str_disable_all_tutorials", (string)null), (string)null);
		TutorialsEnabledText = ((object)GameTexts.FindText("str_tutorials_enabled", (string)null)).ToString();
		TutorialTitleText = ((object)GameTexts.FindText("str_initial_menu_option", "Tutorial")).ToString();
		TitleText = ((object)GameTexts.FindText("str_campaign_tutorial_title", _tutorialTypeId)).ToString();
		TextObject val = default(TextObject);
		if (Input.IsGamepadActive && GameTexts.TryGetText("str_campaign_tutorial_description", ref val, _tutorialTypeId + "_controller"))
		{
			DescriptionText = ((object)val).ToString();
		}
		else
		{
			DescriptionText = ((object)GameTexts.FindText("str_campaign_tutorial_description", _tutorialTypeId)).ToString();
		}
	}

	public void CloseTutorialPanel()
	{
		IsEnabled = false;
	}

	private void ExecuteFinishTutorial()
	{
		_onFinishTutorial();
	}

	private void ExecuteToggleDisableAllTutorials()
	{
		AreTutorialsEnabled = !AreTutorialsEnabled;
	}
}
