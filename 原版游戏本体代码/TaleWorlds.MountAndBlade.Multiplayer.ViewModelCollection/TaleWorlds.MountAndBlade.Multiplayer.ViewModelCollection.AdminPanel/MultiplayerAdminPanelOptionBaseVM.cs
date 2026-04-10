using System;
using System.Collections.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.Admin;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.AdminPanel;

public abstract class MultiplayerAdminPanelOptionBaseVM : ViewModel
{
	protected readonly IAdminPanelOption _option;

	private bool _isRequired;

	private bool _isDisabled;

	private bool _isDirty;

	private bool _canResetToDefault;

	private bool _isFilteredOut;

	private bool _requiresRestart;

	private string _optionTitle;

	private string _optionDescription;

	private HintViewModel _disabledHint;

	private HintViewModel _descriptionHint;

	private HintViewModel _requiresRestartHint;

	private HintViewModel _isDirtyHint;

	private HintViewModel _restoreToDefaultsHint;

	[DataSourceProperty]
	public bool IsRequired
	{
		get
		{
			return _isRequired;
		}
		set
		{
			if (value != _isRequired)
			{
				_isRequired = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRequired");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDisabled
	{
		get
		{
			return _isDisabled;
		}
		set
		{
			if (value != _isDisabled)
			{
				_isDisabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDisabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDirty
	{
		get
		{
			return _isDirty;
		}
		set
		{
			if (value != _isDirty)
			{
				_isDirty = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDirty");
			}
		}
	}

	[DataSourceProperty]
	public bool CanResetToDefault
	{
		get
		{
			return _canResetToDefault;
		}
		set
		{
			if (value != _canResetToDefault)
			{
				_canResetToDefault = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanResetToDefault");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFilteredOut
	{
		get
		{
			return _isFilteredOut;
		}
		set
		{
			if (value != _isFilteredOut)
			{
				_isFilteredOut = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFilteredOut");
			}
		}
	}

	[DataSourceProperty]
	public bool RequiresRestart
	{
		get
		{
			return _requiresRestart;
		}
		set
		{
			if (value != _requiresRestart)
			{
				_requiresRestart = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "RequiresRestart");
			}
		}
	}

	[DataSourceProperty]
	public string OptionTitle
	{
		get
		{
			return _optionTitle;
		}
		set
		{
			if (value != _optionTitle)
			{
				_optionTitle = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OptionTitle");
			}
		}
	}

	[DataSourceProperty]
	public string OptionDescription
	{
		get
		{
			return _optionDescription;
		}
		set
		{
			if (value != _optionDescription)
			{
				_optionDescription = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OptionDescription");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel DisabledHint
	{
		get
		{
			return _disabledHint;
		}
		set
		{
			if (value != _disabledHint)
			{
				_disabledHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "DisabledHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel DescriptionHint
	{
		get
		{
			return _descriptionHint;
		}
		set
		{
			if (value != _descriptionHint)
			{
				_descriptionHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "DescriptionHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel RequiresRestartHint
	{
		get
		{
			return _requiresRestartHint;
		}
		set
		{
			if (value != _requiresRestartHint)
			{
				_requiresRestartHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "RequiresRestartHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel IsDirtyHint
	{
		get
		{
			return _isDirtyHint;
		}
		set
		{
			if (value != _isDirtyHint)
			{
				_isDirtyHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "IsDirtyHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel RestoreToDefaultsHint
	{
		get
		{
			return _restoreToDefaultsHint;
		}
		set
		{
			if (value != _restoreToDefaultsHint)
			{
				_restoreToDefaultsHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "RestoreToDefaultsHint");
			}
		}
	}

	public static event Action<MultiplayerAdminPanelOptionBaseVM> OnOptionRefreshed;

	protected MultiplayerAdminPanelOptionBaseVM(IAdminPanelOption option)
	{
		_option = option;
		_option?.SetOnRefreshCallback(OnOptionRefreshedAux);
		RequiresRestart = option?.RequiresMissionRestart ?? false;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		OptionTitle = _option?.Name?.ToString();
		OptionDescription = _option?.Description?.ToString();
		if (!string.IsNullOrEmpty(_option?.Description))
		{
			DescriptionHint = new HintViewModel(new TextObject("{=!}" + _option.Description, (Dictionary<string, object>)null), (string)null);
		}
		else
		{
			DescriptionHint = null;
		}
		RequiresRestartHint = new HintViewModel(new TextObject("{=MxRJ4CWL}This option won't take effect until next mission.", (Dictionary<string, object>)null), (string)null);
		IsDirtyHint = new HintViewModel(new TextObject("{=ftM2TjQ5}Revert changes", (Dictionary<string, object>)null), (string)null);
		RestoreToDefaultsHint = new HintViewModel(new TextObject("{=36ll5uSI}Restore to defaults", (Dictionary<string, object>)null), (string)null);
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		_option?.SetOnRefreshCallback(null);
	}

	private void OnOptionRefreshedAux()
	{
		MultiplayerAdminPanelOptionBaseVM.OnOptionRefreshed?.Invoke(this);
	}

	public virtual void UpdateValues()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		IAdminPanelOption option = _option;
		IsFilteredOut = option != null && !option.GetIsAvailable();
		IsDirty = _option?.IsDirty ?? false;
		CanResetToDefault = _option?.CanRevertToDefaultValue ?? false;
		string reason = string.Empty;
		IsDisabled = _option?.GetIsDisabled(out reason) ?? false;
		IsRequired = _option?.IsRequired ?? false;
		if (!string.IsNullOrEmpty(reason))
		{
			DisabledHint = new HintViewModel(new TextObject("{=!}" + reason, (Dictionary<string, object>)null), (string)null);
		}
		else
		{
			DisabledHint = null;
		}
	}

	public virtual void ExecuteRevertChanges()
	{
		_option?.RevertChanges();
	}

	public virtual void ExecuteRestoreDefaults()
	{
		_option?.RestoreDefaults();
	}
}
