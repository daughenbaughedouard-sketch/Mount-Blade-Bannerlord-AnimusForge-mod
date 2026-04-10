using System;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;

public class CustomBattleFactionSelectionVM : ViewModel
{
	private Action<BasicCultureObject> _onSelectionChanged;

	private MBBindingList<FactionItemVM> _factions;

	private string _selectedFactionName;

	private FactionItemVM _selectedItem;

	[DataSourceProperty]
	public MBBindingList<FactionItemVM> Factions
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
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<FactionItemVM>>(value, "Factions");
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
	public FactionItemVM SelectedItem
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
				((ViewModel)this).OnPropertyChangedWithValue<FactionItemVM>(value, "SelectedItem");
				if (_selectedItem != null)
				{
					_selectedItem.IsSelected = true;
				}
			}
		}
	}

	public CustomBattleFactionSelectionVM(Action<BasicCultureObject> onSelectionChanged)
	{
		_onSelectionChanged = onSelectionChanged;
		Factions = new MBBindingList<FactionItemVM>();
		foreach (BasicCultureObject faction in CustomBattleData.Factions)
		{
			((Collection<FactionItemVM>)(object)Factions).Add(new FactionItemVM(faction, OnFactionSelected));
		}
		SelectFaction(0);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		FactionItemVM selectedItem = SelectedItem;
		SelectedFactionName = ((selectedItem != null) ? ((object)selectedItem.Faction.Name).ToString() : null);
		Factions.ApplyActionOnAllItems((Action<FactionItemVM>)delegate(FactionItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public void SelectFaction(int index)
	{
		if (index >= 0 && index < ((Collection<FactionItemVM>)(object)Factions).Count)
		{
			SelectedItem = ((Collection<FactionItemVM>)(object)Factions)[index];
		}
	}

	public void ExecuteRandomize()
	{
		int index = MBRandom.RandomInt(((Collection<FactionItemVM>)(object)Factions).Count);
		SelectFaction(index);
	}

	private void OnFactionSelected(FactionItemVM faction)
	{
		SelectedItem = faction;
		_onSelectionChanged(faction.Faction);
		SelectedFactionName = ((object)SelectedItem.Faction.Name).ToString();
	}
}
