using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NavalDLC.CustomBattle.CustomBattle.SelectionItem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace NavalDLC.CustomBattle.CustomBattle;

public class NavalCustomBattleShipSelectionPopUpVM : ViewModel
{
	private Action<ShipHull> _onConfirm;

	private InputKeyItemVM _closeInputKey;

	private MBBindingList<NavalCustomBattleShipHullItemVM> _items;

	private string _title;

	private string _doneLbl;

	private string _cancelLbl;

	private bool _isOpen;

	[DataSourceProperty]
	public InputKeyItemVM CloseInputKey
	{
		get
		{
			return _closeInputKey;
		}
		set
		{
			if (value != _closeInputKey)
			{
				_closeInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "CloseInputKey");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<NavalCustomBattleShipHullItemVM> Items
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
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<NavalCustomBattleShipHullItemVM>>(value, "Items");
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

	public NavalCustomBattleShipSelectionPopUpVM()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0034: Expected O, but got Unknown
		MBBindingList<NavalCustomBattleShipHullItemVM> obj = new MBBindingList<NavalCustomBattleShipHullItemVM>();
		((Collection<NavalCustomBattleShipHullItemVM>)(object)obj).Add(new NavalCustomBattleShipHullItemVM(new TextObject("{=koX9okuG}None", (Dictionary<string, object>)null), new TextObject("{=fNyb979i}Must have at least one ship", (Dictionary<string, object>)null), OnShipHullSelected));
		Items = obj;
		foreach (ShipHull shipHull in NavalCustomBattleData.ShipHulls)
		{
			((Collection<NavalCustomBattleShipHullItemVM>)(object)Items).Add(new NavalCustomBattleShipHullItemVM(shipHull, OnShipHullSelected));
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		((ViewModel)CloseInputKey).OnFinalize();
	}

	public void OpenPopUp(string title, ShipHull selectedItem, bool canSelectEmpty, Action<ShipHull> onConfirm)
	{
		Title = title;
		IsOpen = true;
		_onConfirm = onConfirm;
		Items.ApplyActionOnAllItems((Action<NavalCustomBattleShipHullItemVM>)delegate(NavalCustomBattleShipHullItemVM item)
		{
			item.IsSelected = item.ShipHull == selectedItem;
			item.IsDisabled = item.ShipHull == null && !canSelectEmpty;
		});
	}

	public void ExecuteClose()
	{
		IsOpen = false;
		_onConfirm = null;
	}

	private void OnShipHullSelected(NavalCustomBattleShipHullItemVM item)
	{
		_onConfirm?.Invoke(item?.ShipHull);
		IsOpen = false;
		_onConfirm = null;
	}

	public void SetCloseInputKey(HotKey hotkey)
	{
		CloseInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}
}
