using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SandBox.ViewModelCollection.Input;
using TaleWorlds.CampaignSystem.Incidents;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Map.Incidents;

public class MapIncidentVM : ViewModel
{
	private readonly Incident _incident;

	private readonly Action _onClose;

	private bool _canConfirm;

	private bool _hasFocusedOption;

	private bool _hasSelectedOption;

	private string _title;

	private string _description;

	private string _confirmText;

	private string _incidentType;

	private string _activeHint;

	private HintViewModel _confirmHint;

	private MapIncidentOptionVM _focusedOption;

	private MapIncidentOptionVM _selectedOption;

	private MBBindingList<MapIncidentOptionVM> _options;

	private InputKeyItemVM _doneInputKey;

	[DataSourceProperty]
	public bool CanConfirm
	{
		get
		{
			return _canConfirm;
		}
		set
		{
			if (value != _canConfirm)
			{
				_canConfirm = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanConfirm");
			}
		}
	}

	[DataSourceProperty]
	public bool HasFocusedOption
	{
		get
		{
			return _hasFocusedOption;
		}
		set
		{
			if (value != _hasFocusedOption)
			{
				_hasFocusedOption = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasFocusedOption");
			}
		}
	}

	[DataSourceProperty]
	public bool HasSelectedOption
	{
		get
		{
			return _hasSelectedOption;
		}
		set
		{
			if (value != _hasSelectedOption)
			{
				_hasSelectedOption = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasSelectedOption");
			}
		}
	}

	[DataSourceProperty]
	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (value != _title)
			{
				_title = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Title");
			}
		}
	}

	[DataSourceProperty]
	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != _description)
			{
				_description = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Description");
			}
		}
	}

	[DataSourceProperty]
	public string ConfirmText
	{
		get
		{
			return _confirmText;
		}
		set
		{
			if (value != _confirmText)
			{
				_confirmText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ConfirmText");
			}
		}
	}

	[DataSourceProperty]
	public string IncidentType
	{
		get
		{
			return _incidentType;
		}
		set
		{
			if (value != _incidentType)
			{
				_incidentType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "IncidentType");
			}
		}
	}

	[DataSourceProperty]
	public string ActiveHint
	{
		get
		{
			return _activeHint;
		}
		set
		{
			if (value != _activeHint)
			{
				_activeHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ActiveHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ConfirmHint
	{
		get
		{
			return _confirmHint;
		}
		set
		{
			if (value != _confirmHint)
			{
				_confirmHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ConfirmHint");
			}
		}
	}

	[DataSourceProperty]
	public MapIncidentOptionVM FocusedOption
	{
		get
		{
			return _focusedOption;
		}
		set
		{
			if (value != _focusedOption)
			{
				if (_focusedOption != null)
				{
					_focusedOption.IsFocused = false;
				}
				_focusedOption = value;
				((ViewModel)this).OnPropertyChangedWithValue<MapIncidentOptionVM>(value, "FocusedOption");
				if (_focusedOption != null)
				{
					_focusedOption.IsFocused = true;
				}
				HasFocusedOption = value != null;
			}
		}
	}

	[DataSourceProperty]
	public MapIncidentOptionVM SelectedOption
	{
		get
		{
			return _selectedOption;
		}
		set
		{
			if (value != _selectedOption)
			{
				if (_selectedOption != null)
				{
					_selectedOption.IsSelected = false;
				}
				_selectedOption = value;
				((ViewModel)this).OnPropertyChangedWithValue<MapIncidentOptionVM>(value, "SelectedOption");
				if (_selectedOption != null)
				{
					_selectedOption.IsSelected = true;
				}
				HasSelectedOption = value != null;
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MapIncidentOptionVM> Options
	{
		get
		{
			return _options;
		}
		set
		{
			if (value != _options)
			{
				_options = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MapIncidentOptionVM>>(value, "Options");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM DoneInputKey
	{
		get
		{
			return _doneInputKey;
		}
		set
		{
			if (value != _doneInputKey)
			{
				_doneInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "DoneInputKey");
			}
		}
	}

	public MapIncidentVM(Incident incident, Action onClose)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		_incident = incident;
		_onClose = onClose;
		IncidentType = ((object)incident.Type/*cast due to .constrained prefix*/).ToString();
		ConfirmHint = new HintViewModel();
		Options = new MBBindingList<MapIncidentOptionVM>();
		PopulateOptions();
		((ViewModel)this).RefreshValues();
		UpdateCanConfirm();
	}

	public override void RefreshValues()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		Title = ((object)_incident.Title).ToString();
		Description = ((object)_incident.Description).ToString();
		ConfirmText = ((object)new TextObject("{=WiNRdfsm}Done", (Dictionary<string, object>)null)).ToString();
		Options.ApplyActionOnAllItems((Action<MapIncidentOptionVM>)delegate(MapIncidentOptionVM o)
		{
			((ViewModel)o).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		Options.ApplyActionOnAllItems((Action<MapIncidentOptionVM>)delegate(MapIncidentOptionVM o)
		{
			((ViewModel)o).OnFinalize();
		});
	}

	public void ExecuteConfirm()
	{
		if (SelectedOption != null)
		{
			int index = SelectedOption.Index;
			if (index >= 0 && index < _incident.NumOfOptions)
			{
				foreach (TextObject item in _incident.InvokeOption(index))
				{
					MBInformationManager.AddQuickInformation(item, 0, (BasicCharacterObject)null, (Equipment)null, "");
				}
			}
			else
			{
				Debug.FailedAssert("Selected incident option is out of bounds. Action won't be invoked", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Map\\Incidents\\MapIncidentVM.cs", "ExecuteConfirm", 69);
			}
		}
		else
		{
			Debug.FailedAssert("An incident option must be selected before confirm", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Map\\Incidents\\MapIncidentVM.cs", "ExecuteConfirm", 74);
		}
		_onClose();
	}

	private void PopulateOptions()
	{
		((Collection<MapIncidentOptionVM>)(object)Options).Clear();
		for (int i = 0; i < _incident.NumOfOptions; i++)
		{
			TextObject optionText = _incident.GetOptionText(i);
			List<TextObject> optionHint = _incident.GetOptionHint(i);
			MapIncidentOptionVM item = new MapIncidentOptionVM(optionText, optionHint, i, OnOptionSelected, OnOptionFocused);
			((Collection<MapIncidentOptionVM>)(object)Options).Add(item);
		}
	}

	private void OnOptionSelected(MapIncidentOptionVM option)
	{
		SelectedOption = option;
		UpdateActiveHint();
		UpdateCanConfirm();
	}

	private void OnOptionFocused(MapIncidentOptionVM option)
	{
		FocusedOption = option;
		UpdateActiveHint();
	}

	private void UpdateActiveHint()
	{
		if (FocusedOption != null)
		{
			ActiveHint = FocusedOption.Hint;
		}
		else if (SelectedOption != null)
		{
			ActiveHint = SelectedOption.Hint;
		}
		else
		{
			ActiveHint = string.Empty;
		}
	}

	private void UpdateCanConfirm()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (SelectedOption == null)
		{
			CanConfirm = false;
			ConfirmHint.HintText = new TextObject("{=R3Zn7x07}You must select an option", (Dictionary<string, object>)null);
		}
		else
		{
			CanConfirm = true;
			ConfirmHint.HintText = TextObject.GetEmpty();
		}
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}
}
