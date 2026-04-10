using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.Admin;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.AdminPanel;

public class MultiplayerAdminPanelMultiSelectionOptionVM : MultiplayerAdminPanelOptionBaseVM
{
	public class AdminPanelOptionSelectorVM : SelectorVM<AdminPanelOptionSelectorItemVM>
	{
		private bool _isEnabled;

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

		public AdminPanelOptionSelectorVM(int selectedIndex, Action<SelectorVM<AdminPanelOptionSelectorItemVM>> onChange)
			: base(selectedIndex, onChange)
		{
		}
	}

	public class AdminPanelOptionSelectorItemVM : SelectorItemVM
	{
		public readonly IAdminPanelMultiSelectionItem SelectionItem;

		public AdminPanelOptionSelectorItemVM(IAdminPanelMultiSelectionItem selectionItem)
			: base(selectionItem?.DisplayName ?? selectionItem?.Value)
		{
			SelectionItem = selectionItem;
		}
	}

	private new readonly IAdminPanelMultiSelectionOption _option;

	private readonly SelectorItemVM _initialValue;

	private bool _isMultiSelectionOption;

	private AdminPanelOptionSelectorVM _multiSelectionOptions;

	[DataSourceProperty]
	public bool IsMultiSelectionOption
	{
		get
		{
			return _isMultiSelectionOption;
		}
		set
		{
			if (value != _isMultiSelectionOption)
			{
				_isMultiSelectionOption = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMultiSelectionOption");
			}
		}
	}

	[DataSourceProperty]
	public AdminPanelOptionSelectorVM MultiSelectionOptions
	{
		get
		{
			return _multiSelectionOptions;
		}
		set
		{
			if (value != _multiSelectionOptions)
			{
				_multiSelectionOptions = value;
				((ViewModel)this).OnPropertyChangedWithValue<AdminPanelOptionSelectorVM>(value, "MultiSelectionOptions");
			}
		}
	}

	public MultiplayerAdminPanelMultiSelectionOptionVM(IAdminPanelMultiSelectionOption option)
		: base(option)
	{
		_option = option;
		MultiSelectionOptions = new AdminPanelOptionSelectorVM(-1, null);
		IsMultiSelectionOption = true;
		((ViewModel)this).RefreshValues();
		_initialValue = (SelectorItemVM)(object)((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).SelectedItem;
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		RefreshOptions();
	}

	private void OnSelectorChange(SelectorVM<AdminPanelOptionSelectorItemVM> selector)
	{
		_option.SetValue(selector.SelectedItem?.SelectionItem);
	}

	public override void UpdateValues()
	{
		base.UpdateValues();
		if (!((IEnumerable<IAdminPanelMultiSelectionItem>)_option.GetAvailableOptions()).SequenceEqual(((IEnumerable<AdminPanelOptionSelectorItemVM>)((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).ItemList).Select((AdminPanelOptionSelectorItemVM i) => i.SelectionItem)))
		{
			RefreshOptions();
		}
		for (int num = 0; num < ((Collection<AdminPanelOptionSelectorItemVM>)(object)((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).ItemList).Count; num++)
		{
			if (((Collection<AdminPanelOptionSelectorItemVM>)(object)((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).ItemList)[num].SelectionItem == _option.GetValue())
			{
				((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).SelectedIndex = num;
				break;
			}
		}
	}

	public override void ExecuteRestoreDefaults()
	{
		base.ExecuteRestoreDefaults();
	}

	public override void ExecuteRevertChanges()
	{
		base.ExecuteRevertChanges();
	}

	private void RefreshOptions()
	{
		if (MultiSelectionOptions == null)
		{
			return;
		}
		List<AdminPanelOptionSelectorItemVM> list = new List<AdminPanelOptionSelectorItemVM>();
		if (_option == null)
		{
			((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).Refresh((IEnumerable<AdminPanelOptionSelectorItemVM>)list, 0, (Action<SelectorVM<AdminPanelOptionSelectorItemVM>>)OnSelectorChange);
			return;
		}
		IAdminPanelMultiSelectionItem value = _option.GetValue();
		MBReadOnlyList<IAdminPanelMultiSelectionItem> val = _option.GetAvailableOptions() ?? new MBReadOnlyList<IAdminPanelMultiSelectionItem>();
		if (val != null)
		{
			for (int i = 0; i < ((List<IAdminPanelMultiSelectionItem>)(object)val).Count; i++)
			{
				list.Add(new AdminPanelOptionSelectorItemVM(((List<IAdminPanelMultiSelectionItem>)(object)val)[i]));
			}
		}
		MultiSelectionOptions.IsEnabled = !base.IsDisabled;
		((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).Refresh((IEnumerable<AdminPanelOptionSelectorItemVM>)list, 0, (Action<SelectorVM<AdminPanelOptionSelectorItemVM>>)OnSelectorChange);
		if (MultiSelectionOptions == null)
		{
			return;
		}
		for (int j = 0; j < ((Collection<AdminPanelOptionSelectorItemVM>)(object)((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).ItemList).Count; j++)
		{
			if (((Collection<AdminPanelOptionSelectorItemVM>)(object)((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).ItemList)[j].SelectionItem == value)
			{
				((SelectorVM<AdminPanelOptionSelectorItemVM>)MultiSelectionOptions).SelectedIndex = j;
				break;
			}
		}
	}
}
