using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NavalDLC.ViewModelCollection.Port.PortScreenHandlers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port;

public class ShipUpgradeContainerVM : ViewModel
{
	public delegate void ShipSlotSelectedDelegate(ShipUpgradeSlotBaseVM slot);

	public static ShipSlotSelectedDelegate OnSlotSelected;

	private bool _canTradeUpgrades;

	private bool _hasSelectedSlot;

	private ShipItemVM _ship;

	private ShipUpgradeSlotBaseVM _selectedSlot;

	private MBBindingList<ShipUpgradeSlotBaseVM> _upgradeSlots;

	[DataSourceProperty]
	public bool CanTradeUpgrades
	{
		get
		{
			return _canTradeUpgrades;
		}
		set
		{
			if (value != _canTradeUpgrades)
			{
				_canTradeUpgrades = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanTradeUpgrades");
			}
		}
	}

	[DataSourceProperty]
	public bool HasSelectedSlot
	{
		get
		{
			return _hasSelectedSlot;
		}
		set
		{
			if (value != _hasSelectedSlot)
			{
				_hasSelectedSlot = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasSelectedSlot");
			}
		}
	}

	[DataSourceProperty]
	public ShipItemVM Ship
	{
		get
		{
			return _ship;
		}
		set
		{
			if (value != _ship)
			{
				_ship = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipItemVM>(value, "Ship");
			}
		}
	}

	[DataSourceProperty]
	public ShipUpgradeSlotBaseVM SelectedSlot
	{
		get
		{
			return _selectedSlot;
		}
		set
		{
			if (value != _selectedSlot)
			{
				if (_selectedSlot != null)
				{
					_selectedSlot.IsSelected = false;
				}
				_selectedSlot = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipUpgradeSlotBaseVM>(value, "SelectedSlot");
				if (_selectedSlot != null)
				{
					_selectedSlot.IsSelected = true;
				}
				HasSelectedSlot = _selectedSlot != null;
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ShipUpgradeSlotBaseVM> UpgradeSlots
	{
		get
		{
			return _upgradeSlots;
		}
		set
		{
			if (value != _upgradeSlots)
			{
				_upgradeSlots = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<ShipUpgradeSlotBaseVM>>(value, "UpgradeSlots");
			}
		}
	}

	public ShipUpgradeContainerVM(ShipItemVM ship)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		Ship = ship;
		UpgradeSlots = new MBBindingList<ShipUpgradeSlotBaseVM>();
		foreach (KeyValuePair<string, ShipSlot> availableSlot in Ship.Ship.ShipHull.AvailableSlots)
		{
			((Collection<ShipUpgradeSlotBaseVM>)(object)UpgradeSlots).Add((ShipUpgradeSlotBaseVM)new ShipUpgradeSlotVM(Ship.Ship, availableSlot.Value.GetSlotTypeName(), availableSlot.Key, availableSlot.Value.TypeId, OnSlotSelectedAux));
		}
		if (Ship.Ship.CanEquipFigurehead)
		{
			((Collection<ShipUpgradeSlotBaseVM>)(object)UpgradeSlots).Add((ShipUpgradeSlotBaseVM)new ShipFigureheadSlotVM(Ship.Ship, new TextObject("{=YLbBHN0Z}Figurehead", (Dictionary<string, object>)null), "figurehead", "figurehead", OnSlotSelectedAux));
		}
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		UpgradeSlots.ApplyActionOnAllItems((Action<ShipUpgradeSlotBaseVM>)delegate(ShipUpgradeSlotBaseVM us)
		{
			((ViewModel)us).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		UpgradeSlots.ApplyActionOnAllItems((Action<ShipUpgradeSlotBaseVM>)delegate(ShipUpgradeSlotBaseVM us)
		{
			((ViewModel)us).OnFinalize();
		});
	}

	public void ResetUpgradePieces()
	{
		UpgradeSlots.ApplyActionOnAllItems((Action<ShipUpgradeSlotBaseVM>)delegate(ShipUpgradeSlotBaseVM s)
		{
			s.ResetPieces();
		});
	}

	public void UpdateEnabledStatus(in PortActionInfo actionInfo)
	{
		CanTradeUpgrades = actionInfo.IsEnabled;
		for (int i = 0; i < ((Collection<ShipUpgradeSlotBaseVM>)(object)UpgradeSlots).Count; i++)
		{
			((Collection<ShipUpgradeSlotBaseVM>)(object)UpgradeSlots)[i].UpdateEnabledStatus(in actionInfo);
		}
	}

	private void OnSlotSelectedAux(ShipUpgradeSlotBaseVM slot)
	{
		if (SelectedSlot != null && SelectedSlot == slot)
		{
			SelectedSlot = null;
			OnSlotSelected?.Invoke(SelectedSlot);
		}
		else if (slot == null || ((Collection<ShipUpgradePieceBaseVM>)(object)slot.AvailablePieces).Count != 0 || slot.HasSelectedPiece)
		{
			SelectedSlot = slot;
			OnSlotSelected?.Invoke(SelectedSlot);
		}
	}

	public void ExecuteClearSelection()
	{
		SelectedSlot?.ExecuteDeselect();
	}

	public void Update()
	{
		for (int i = 0; i < ((Collection<ShipUpgradeSlotBaseVM>)(object)UpgradeSlots).Count; i++)
		{
			((Collection<ShipUpgradeSlotBaseVM>)(object)UpgradeSlots)[i].Update();
		}
	}
}
