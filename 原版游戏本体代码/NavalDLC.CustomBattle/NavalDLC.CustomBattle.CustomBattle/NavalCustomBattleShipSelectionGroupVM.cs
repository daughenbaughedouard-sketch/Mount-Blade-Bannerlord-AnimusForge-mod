using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NavalDLC.CustomBattle.CustomBattle.SelectionItem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace NavalDLC.CustomBattle.CustomBattle;

public class NavalCustomBattleShipSelectionGroupVM : ViewModel
{
	private readonly Action _onShipSelectedOrUpgraded;

	private MBBindingList<NavalCustomBattleShipSelectionItemVM> _shipSelectionItems;

	[DataSourceProperty]
	public MBBindingList<NavalCustomBattleShipSelectionItemVM> ShipSelectionItems
	{
		get
		{
			return _shipSelectionItems;
		}
		set
		{
			if (value != _shipSelectionItems)
			{
				_shipSelectionItems = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<NavalCustomBattleShipSelectionItemVM>>(value, "ShipSelectionItems");
			}
		}
	}

	public NavalCustomBattleShipSelectionGroupVM(bool isPlayerSide, NavalCustomBattleShipSelectionPopUpVM shipSelectionPopUp, Action onShipSelectedOrUpgraded, Action<NavalCustomBattleShipItemVM> onShipFocused)
	{
		_onShipSelectedOrUpgraded = onShipSelectedOrUpgraded;
		ShipSelectionItems = new MBBindingList<NavalCustomBattleShipSelectionItemVM>();
		for (int i = 0; i < 8; i++)
		{
			((Collection<NavalCustomBattleShipSelectionItemVM>)(object)ShipSelectionItems).Add(new NavalCustomBattleShipSelectionItemVM(isPlayerSide, shipSelectionPopUp, OnShipSelectedOrUpgraded, onShipFocused));
		}
		((Collection<NavalCustomBattleShipSelectionItemVM>)(object)ShipSelectionItems)[0].SelectedItem = new NavalCustomBattleShipItemVM(NavalCustomBattleData.ShipHulls.ElementAt(0), isPlayerSide, OnShipSelectedOrUpgraded);
		UpdateCanShipsBecomeEmpty();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		ShipSelectionItems.ApplyActionOnAllItems((Action<NavalCustomBattleShipSelectionItemVM>)delegate(NavalCustomBattleShipSelectionItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public void ExecuteRandomize(int targetDeckSize)
	{
		List<ShipHull> source = new List<ShipHull>();
		int num = int.MaxValue;
		for (int i = 0; i < 20; i++)
		{
			int deckSize;
			List<ShipHull> list = CreateRandomFleet(targetDeckSize, out deckSize);
			int num2 = Math.Abs(targetDeckSize - deckSize);
			if (num2 < num)
			{
				num = num2;
				source = list;
				if (num2 == 0)
				{
					break;
				}
			}
		}
		for (int j = 0; j < ((Collection<NavalCustomBattleShipSelectionItemVM>)(object)ShipSelectionItems).Count; j++)
		{
			((Collection<NavalCustomBattleShipSelectionItemVM>)(object)ShipSelectionItems)[j].SetHull(source.ElementAtOrDefault(j));
			((Collection<NavalCustomBattleShipSelectionItemVM>)(object)ShipSelectionItems)[j].SelectedItem?.RandomizeUpgrades();
		}
	}

	private List<ShipHull> CreateRandomFleet(int targetDeckSize, out int deckSize)
	{
		List<ShipHull> list = new List<ShipHull>();
		deckSize = 0;
		for (int i = 0; i < ((Collection<NavalCustomBattleShipSelectionItemVM>)(object)ShipSelectionItems).Count; i++)
		{
			ShipHull randomElementInefficiently = Extensions.GetRandomElementInefficiently<ShipHull>(NavalCustomBattleData.ShipHulls);
			list.Add(randomElementInefficiently);
			deckSize += randomElementInefficiently.MainDeckCrewCapacity;
			if (deckSize >= targetDeckSize)
			{
				break;
			}
		}
		return list;
	}

	public List<IShipOrigin> GetSelectedShips()
	{
		List<IShipOrigin> list = new List<IShipOrigin>();
		foreach (NavalCustomBattleShipSelectionItemVM item in (Collection<NavalCustomBattleShipSelectionItemVM>)(object)ShipSelectionItems)
		{
			if (item.HasSelectedItem)
			{
				list.Add((IShipOrigin)(object)item.SelectedItem.Ship);
			}
		}
		return list;
	}

	private void OnShipSelectedOrUpgraded()
	{
		_onShipSelectedOrUpgraded?.Invoke();
		UpdateCanShipsBecomeEmpty();
	}

	private void UpdateCanShipsBecomeEmpty()
	{
		int totalSelectedItemCount = ((IEnumerable<NavalCustomBattleShipSelectionItemVM>)ShipSelectionItems).Count((NavalCustomBattleShipSelectionItemVM x) => x.HasSelectedItem);
		ShipSelectionItems.ApplyActionOnAllItems((Action<NavalCustomBattleShipSelectionItemVM>)delegate(NavalCustomBattleShipSelectionItemVM x)
		{
			x.CanBecomeEmpty = x.HasSelectedItem && totalSelectedItemCount > 1;
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		ShipSelectionItems.ApplyActionOnAllItems((Action<NavalCustomBattleShipSelectionItemVM>)delegate(NavalCustomBattleShipSelectionItemVM x)
		{
			((ViewModel)x).OnFinalize();
		});
	}

	public void SetCycleTierInputKey(HotKey hotkey)
	{
		foreach (NavalCustomBattleShipSelectionItemVM item in (Collection<NavalCustomBattleShipSelectionItemVM>)(object)ShipSelectionItems)
		{
			item.SetCycleTierInputKey(hotkey);
		}
	}
}
