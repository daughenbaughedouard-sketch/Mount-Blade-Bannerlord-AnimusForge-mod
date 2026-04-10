using System;
using System.Collections.ObjectModel;
using NavalDLC.CustomBattle.CustomBattle.SelectionItem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.CustomBattle.CustomBattle;

public class NavalCustomBattleFactionSelectionVM : ViewModel
{
	private Action<BasicCultureObject> _onSelectionChanged;

	private MBBindingList<NavalCustomBattleFactionItemVM> _factions;

	private string _selectedFactionName;

	private NavalCustomBattleFactionItemVM _selectedItem;

	[DataSourceProperty]
	public MBBindingList<NavalCustomBattleFactionItemVM> Factions
	{
		get
		{
			return _factions;
		}
		set
		{
			if (value != _factions)
			{
				_factions = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<NavalCustomBattleFactionItemVM>>(value, "Factions");
			}
		}
	}

	[DataSourceProperty]
	public string SelectedFactionName
	{
		get
		{
			return _selectedFactionName;
		}
		set
		{
			if (value != _selectedFactionName)
			{
				_selectedFactionName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SelectedFactionName");
			}
		}
	}

	[DataSourceProperty]
	public NavalCustomBattleFactionItemVM SelectedItem
	{
		get
		{
			return _selectedItem;
		}
		set
		{
			if (value != _selectedItem)
			{
				if (_selectedItem != null)
				{
					_selectedItem.IsSelected = false;
				}
				_selectedItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<NavalCustomBattleFactionItemVM>(value, "SelectedItem");
				if (_selectedItem != null)
				{
					_selectedItem.IsSelected = true;
				}
			}
		}
	}

	public NavalCustomBattleFactionSelectionVM(Action<BasicCultureObject> onSelectionChanged)
	{
		_onSelectionChanged = onSelectionChanged;
		Factions = new MBBindingList<NavalCustomBattleFactionItemVM>();
		foreach (BasicCultureObject faction in NavalCustomBattleData.Factions)
		{
			((Collection<NavalCustomBattleFactionItemVM>)(object)Factions).Add(new NavalCustomBattleFactionItemVM(faction, OnFactionSelected));
		}
		SelectFaction(0);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		NavalCustomBattleFactionItemVM selectedItem = SelectedItem;
		SelectedFactionName = ((selectedItem != null) ? ((object)selectedItem.Faction.Name).ToString() : null);
		Factions.ApplyActionOnAllItems((Action<NavalCustomBattleFactionItemVM>)delegate(NavalCustomBattleFactionItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public void SelectFaction(int index)
	{
		if (index >= 0 && index < ((Collection<NavalCustomBattleFactionItemVM>)(object)Factions).Count)
		{
			SelectedItem = ((Collection<NavalCustomBattleFactionItemVM>)(object)Factions)[index];
		}
	}

	public void ExecuteRandomize()
	{
		int index = MBRandom.RandomInt(((Collection<NavalCustomBattleFactionItemVM>)(object)Factions).Count);
		SelectFaction(index);
	}

	private void OnFactionSelected(NavalCustomBattleFactionItemVM faction)
	{
		SelectedItem = faction;
		_onSelectionChanged(faction.Faction);
		SelectedFactionName = ((object)SelectedItem.Faction.Name).ToString();
	}
}
