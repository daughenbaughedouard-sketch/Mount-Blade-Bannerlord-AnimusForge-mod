using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.Admin;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.AdminPanel;

public class MultiplayerAdminPanelVM : ViewModel
{
	private readonly Action<bool> _onEscapeMenuToggled;

	private readonly MBReadOnlyList<IAdminPanelOptionProvider> _optionProviders;

	private readonly Func<IAdminPanelOption, MultiplayerAdminPanelOptionBaseVM> _onCreateOptionViewModel;

	private readonly Func<IAdminPanelAction, MultiplayerAdminPanelOptionBaseVM> _onCreateActionViewModel;

	private bool _areOptionValuesDirty;

	private bool _isApplyDisabled;

	private string _titleText;

	private string _cancelText;

	private string _applyText;

	private string _startMissionText;

	private HintViewModel _applyDisabledHint;

	private MBBindingList<MultiplayerAdminPanelOptionGroupVM> _optionGroups;

	[DataSourceProperty]
	public bool IsApplyDisabled
	{
		get
		{
			return _isApplyDisabled;
		}
		set
		{
			if (value != _isApplyDisabled)
			{
				_isApplyDisabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsApplyDisabled");
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
	public string CancelText
	{
		get
		{
			return _cancelText;
		}
		set
		{
			if (value != _cancelText)
			{
				_cancelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelText");
			}
		}
	}

	[DataSourceProperty]
	public string ApplyText
	{
		get
		{
			return _applyText;
		}
		set
		{
			if (value != _applyText)
			{
				_applyText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ApplyText");
			}
		}
	}

	[DataSourceProperty]
	public string StartMissionText
	{
		get
		{
			return _startMissionText;
		}
		set
		{
			if (value != _startMissionText)
			{
				_startMissionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "StartMissionText");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ApplyDisabledHint
	{
		get
		{
			return _applyDisabledHint;
		}
		set
		{
			if (value != _applyDisabledHint)
			{
				_applyDisabledHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ApplyDisabledHint");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MultiplayerAdminPanelOptionGroupVM> OptionGroups
	{
		get
		{
			return _optionGroups;
		}
		set
		{
			if (value != _optionGroups)
			{
				_optionGroups = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MultiplayerAdminPanelOptionGroupVM>>(value, "OptionGroups");
			}
		}
	}

	public MultiplayerAdminPanelVM(Action<bool> onEscapeMenuToggled, MBReadOnlyList<IAdminPanelOptionProvider> optionProviders, Func<IAdminPanelOption, MultiplayerAdminPanelOptionBaseVM> onGetOptionViewModel, Func<IAdminPanelAction, MultiplayerAdminPanelOptionBaseVM> onGetActionViewModel)
	{
		_onEscapeMenuToggled = onEscapeMenuToggled;
		_optionProviders = optionProviders;
		_onCreateOptionViewModel = onGetOptionViewModel;
		_onCreateActionViewModel = onGetActionViewModel;
		OptionGroups = new MBBindingList<MultiplayerAdminPanelOptionGroupVM>();
		InitializeOptions();
		InitializeCallbacks();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=xILeUbY3}Admin Panel", (Dictionary<string, object>)null)).ToString();
		CancelText = ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString();
		ApplyText = ((object)new TextObject("{=WZQnNSwV}Apply Changes", (Dictionary<string, object>)null)).ToString();
		StartMissionText = ((object)new TextObject("{=kwo09aDm}Apply and Start Mission", (Dictionary<string, object>)null)).ToString();
		ApplyDisabledHint = new HintViewModel(new TextObject("{=TrY4VS1R}Please select valid values for options.", (Dictionary<string, object>)null), (string)null);
		OptionGroups.ApplyActionOnAllItems((Action<MultiplayerAdminPanelOptionGroupVM>)delegate(MultiplayerAdminPanelOptionGroupVM o)
		{
			((ViewModel)o).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		if (_optionProviders != null)
		{
			for (int i = 0; i < ((List<IAdminPanelOptionProvider>)(object)_optionProviders).Count; i++)
			{
				((List<IAdminPanelOptionProvider>)(object)_optionProviders)[i].OnFinalize();
			}
		}
		FinalizeCallbacks();
		OptionGroups.ApplyActionOnAllItems((Action<MultiplayerAdminPanelOptionGroupVM>)delegate(MultiplayerAdminPanelOptionGroupVM o)
		{
			((ViewModel)o).OnFinalize();
		});
	}

	public void OnTick(float dt)
	{
		if (_optionProviders != null)
		{
			for (int i = 0; i < ((List<IAdminPanelOptionProvider>)(object)_optionProviders).Count; i++)
			{
				((List<IAdminPanelOptionProvider>)(object)_optionProviders)[i].OnTick(dt);
			}
		}
		if (_areOptionValuesDirty)
		{
			UpdateOptionValues();
			_areOptionValuesDirty = false;
		}
	}

	private void InitializeCallbacks()
	{
		MultiplayerAdminPanelOptionBaseVM.OnOptionRefreshed += OnOptionChanged;
	}

	private void FinalizeCallbacks()
	{
		MultiplayerAdminPanelOptionBaseVM.OnOptionRefreshed -= OnOptionChanged;
	}

	private void InitializeOptions()
	{
		((Collection<MultiplayerAdminPanelOptionGroupVM>)(object)OptionGroups).Clear();
		if (_optionProviders != null)
		{
			foreach (IAdminPanelOptionProvider item2 in (List<IAdminPanelOptionProvider>)(object)_optionProviders)
			{
				foreach (IAdminPanelOptionGroup item3 in (List<IAdminPanelOptionGroup>)(object)item2.GetOptionGroups())
				{
					MultiplayerAdminPanelOptionGroupVM item = new MultiplayerAdminPanelOptionGroupVM(item3, _onCreateOptionViewModel, _onCreateActionViewModel);
					((Collection<MultiplayerAdminPanelOptionGroupVM>)(object)OptionGroups).Add(item);
				}
			}
		}
		UpdateOptionValues();
	}

	private void OnOptionChanged(MultiplayerAdminPanelOptionBaseVM option)
	{
		_areOptionValuesDirty = true;
	}

	private void UpdateOptionValues()
	{
		bool isApplyDisabled = false;
		foreach (MultiplayerAdminPanelOptionGroupVM item in (Collection<MultiplayerAdminPanelOptionGroupVM>)(object)OptionGroups)
		{
			foreach (MultiplayerAdminPanelOptionBaseVM item2 in (Collection<MultiplayerAdminPanelOptionBaseVM>)(object)item.Options)
			{
				item2.UpdateValues();
				if (item2.IsRequired && item2.IsDisabled)
				{
					isApplyDisabled = true;
				}
			}
		}
		IsApplyDisabled = isApplyDisabled;
	}

	public void ExecuteApplyChanges()
	{
		if (_optionProviders == null)
		{
			return;
		}
		foreach (IAdminPanelOptionProvider item in (List<IAdminPanelOptionProvider>)(object)_optionProviders)
		{
			item.ApplyOptions();
		}
		_areOptionValuesDirty = true;
	}

	public void ExecuteCancel()
	{
		_onEscapeMenuToggled(obj: false);
	}
}
