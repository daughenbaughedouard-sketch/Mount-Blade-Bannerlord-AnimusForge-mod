using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NavalDLC.CustomBattle.CustomBattle.SelectionItem;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.CustomBattle.CustomBattle;

public class NavalCustomBattleGameTypeSelectionGroupVM : ViewModel
{
	private SelectorVM<NavalCustomBattlePlayerSideItemVM> _playerSideSelection;

	private string _playerSideText;

	public NavalCustomBattlePlayerSide SelectedPlayerSide { get; private set; }

	[DataSourceProperty]
	public SelectorVM<NavalCustomBattlePlayerSideItemVM> PlayerSideSelection
	{
		get
		{
			return _playerSideSelection;
		}
		set
		{
			if (value != _playerSideSelection)
			{
				_playerSideSelection = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<NavalCustomBattlePlayerSideItemVM>>(value, "PlayerSideSelection");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerSideText
	{
		get
		{
			return _playerSideText;
		}
		set
		{
			if (value != _playerSideText)
			{
				_playerSideText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PlayerSideText");
			}
		}
	}

	public NavalCustomBattleGameTypeSelectionGroupVM()
	{
		PlayerSideSelection = new SelectorVM<NavalCustomBattlePlayerSideItemVM>(0, (Action<SelectorVM<NavalCustomBattlePlayerSideItemVM>>)OnPlayerSideSelection);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		PlayerSideText = ((object)new TextObject("{=P3rMg4uZ}Player Side", (Dictionary<string, object>)null)).ToString();
		((Collection<NavalCustomBattlePlayerSideItemVM>)(object)PlayerSideSelection.ItemList).Clear();
		foreach (Tuple<string, NavalCustomBattlePlayerSide> playerSide in NavalCustomBattleData.PlayerSides)
		{
			PlayerSideSelection.AddItem(new NavalCustomBattlePlayerSideItemVM(playerSide.Item1, playerSide.Item2));
		}
		PlayerSideSelection.SelectedIndex = 0;
	}

	public void RandomizeAll()
	{
		PlayerSideSelection.ExecuteRandomize();
	}

	private void OnPlayerSideSelection(SelectorVM<NavalCustomBattlePlayerSideItemVM> selector)
	{
		SelectedPlayerSide = selector.SelectedItem.PlayerSide;
	}
}
