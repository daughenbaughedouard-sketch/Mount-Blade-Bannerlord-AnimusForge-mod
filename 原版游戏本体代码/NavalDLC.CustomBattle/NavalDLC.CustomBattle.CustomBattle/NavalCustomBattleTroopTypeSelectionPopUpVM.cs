using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace NavalDLC.CustomBattle.CustomBattle;

public class NavalCustomBattleTroopTypeSelectionPopUpVM : ViewModel
{
	public Action OnPopUpClosed;

	private List<bool> _itemSelectionsBackUp;

	private int _selectedItemCount;

	private InputKeyItemVM _doneInputKey;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _resetInputKey;

	private MBBindingList<NavalCustomBattleTroopTypeVM> _items;

	private string _title;

	private string _doneLbl;

	private string _cancelLbl;

	private string _selectAllLbl;

	private string _backToDefaultLbl;

	private bool _isOpen;

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

	[DataSourceProperty]
	public InputKeyItemVM CancelInputKey
	{
		get
		{
			return _cancelInputKey;
		}
		set
		{
			if (value != _cancelInputKey)
			{
				_cancelInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "CancelInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM ResetInputKey
	{
		get
		{
			return _resetInputKey;
		}
		set
		{
			if (value != _resetInputKey)
			{
				_resetInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "ResetInputKey");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<NavalCustomBattleTroopTypeVM> Items
	{
		get
		{
			return _items;
		}
		set
		{
			if (value != _items)
			{
				_items = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<NavalCustomBattleTroopTypeVM>>(value, "Items");
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
	public string DoneLbl
	{
		get
		{
			return _doneLbl;
		}
		set
		{
			if (value != _doneLbl)
			{
				_doneLbl = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DoneLbl");
			}
		}
	}

	[DataSourceProperty]
	public string CancelLbl
	{
		get
		{
			return _cancelLbl;
		}
		set
		{
			if (value != _cancelLbl)
			{
				_cancelLbl = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelLbl");
			}
		}
	}

	[DataSourceProperty]
	public string SelectAllLbl
	{
		get
		{
			return _selectAllLbl;
		}
		set
		{
			if (value != _selectAllLbl)
			{
				_selectAllLbl = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SelectAllLbl");
			}
		}
	}

	[DataSourceProperty]
	public string BackToDefaultLbl
	{
		get
		{
			return _backToDefaultLbl;
		}
		set
		{
			if (value != _backToDefaultLbl)
			{
				_backToDefaultLbl = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BackToDefaultLbl");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOpen
	{
		get
		{
			return _isOpen;
		}
		set
		{
			if (value != _isOpen)
			{
				_isOpen = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsOpen");
			}
		}
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		DoneLbl = ((object)GameTexts.FindText("str_done", (string)null)).ToString();
		CancelLbl = ((object)GameTexts.FindText("str_cancel", (string)null)).ToString();
		SelectAllLbl = ((object)GameTexts.FindText("str_custom_battle_select_all", (string)null)).ToString();
		BackToDefaultLbl = ((object)GameTexts.FindText("str_custom_battle_back_to_default", (string)null)).ToString();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		((ViewModel)DoneInputKey).OnFinalize();
		((ViewModel)CancelInputKey).OnFinalize();
		((ViewModel)ResetInputKey).OnFinalize();
	}

	public void OpenPopUp(string title, MBBindingList<NavalCustomBattleTroopTypeVM> troops)
	{
		_itemSelectionsBackUp = new List<bool>();
		foreach (NavalCustomBattleTroopTypeVM item in (Collection<NavalCustomBattleTroopTypeVM>)(object)troops)
		{
			_itemSelectionsBackUp.Add(item.IsSelected);
		}
		_selectedItemCount = ((IEnumerable<NavalCustomBattleTroopTypeVM>)troops).Count((NavalCustomBattleTroopTypeVM x) => x.IsSelected);
		Title = title;
		Items = troops;
		IsOpen = true;
	}

	public void OnItemSelectionToggled(NavalCustomBattleTroopTypeVM item)
	{
		if (_selectedItemCount > 1 || !item.IsSelected)
		{
			item.IsSelected = !item.IsSelected;
			_selectedItemCount += (item.IsSelected ? 1 : (-1));
		}
	}

	public void ExecuteSelectAll()
	{
		Items.ApplyActionOnAllItems((Action<NavalCustomBattleTroopTypeVM>)delegate(NavalCustomBattleTroopTypeVM x)
		{
			x.IsSelected = true;
		});
		_selectedItemCount = ((Collection<NavalCustomBattleTroopTypeVM>)(object)Items).Count;
	}

	public void ExecuteBackToDefault()
	{
		Items.ApplyActionOnAllItems((Action<NavalCustomBattleTroopTypeVM>)delegate(NavalCustomBattleTroopTypeVM x)
		{
			x.IsSelected = x.IsDefault;
		});
		_selectedItemCount = ((IEnumerable<NavalCustomBattleTroopTypeVM>)Items).Count((NavalCustomBattleTroopTypeVM x) => x.IsSelected);
	}

	public void ExecuteCancel()
	{
		ExecuteReset();
		OnPopUpClosed?.Invoke();
		IsOpen = false;
	}

	public void ExecuteDone()
	{
		IsOpen = false;
	}

	public void ExecuteReset()
	{
		int count = _itemSelectionsBackUp.Count;
		if (count != ((Collection<NavalCustomBattleTroopTypeVM>)(object)Items).Count)
		{
			Debug.FailedAssert("Backup troop count does not match with the actual troop count.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.CustomBattle\\CustomBattle\\NavalCustomBattleTroopTypeSelectionPopUpVM.cs", "ExecuteReset", 99);
			return;
		}
		for (int i = 0; i < count; i++)
		{
			((Collection<NavalCustomBattleTroopTypeVM>)(object)Items)[i].IsSelected = _itemSelectionsBackUp[i];
		}
		_selectedItemCount = ((IEnumerable<NavalCustomBattleTroopTypeVM>)Items).Count((NavalCustomBattleTroopTypeVM x) => x.IsSelected);
	}

	public void SetCancelInputKey(HotKey hotkey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}

	public void SetDoneInputKey(HotKey hotkey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}

	public void SetResetInputKey(HotKey hotkey)
	{
		ResetInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}
}
