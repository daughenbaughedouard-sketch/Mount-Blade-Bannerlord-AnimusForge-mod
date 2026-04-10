using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;

namespace TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

public class GameTypeSelectionGroupVM : ViewModel
{
	private readonly Action<CustomBattlePlayerType> _onPlayerTypeChange;

	private readonly Action<string> _onGameTypeChange;

	private SelectorVM<GameTypeItemVM> _gameTypeSelection;

	private SelectorVM<PlayerTypeItemVM> _playerTypeSelection;

	private SelectorVM<PlayerSideItemVM> _playerSideSelection;

	private string _gameTypeText;

	private string _playerTypeText;

	private string _playerSideText;

	public string SelectedGameTypeString { get; private set; }

	public CustomBattlePlayerType SelectedPlayerType { get; private set; }

	public CustomBattlePlayerSide SelectedPlayerSide { get; private set; }

	[DataSourceProperty]
	public SelectorVM<GameTypeItemVM> GameTypeSelection
	{
		get
		{
			return _gameTypeSelection;
		}
		set
		{
			if (value != _gameTypeSelection)
			{
				_gameTypeSelection = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<GameTypeItemVM>>(value, "GameTypeSelection");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<PlayerTypeItemVM> PlayerTypeSelection
	{
		get
		{
			return _playerTypeSelection;
		}
		set
		{
			if (value != _playerTypeSelection)
			{
				_playerTypeSelection = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<PlayerTypeItemVM>>(value, "PlayerTypeSelection");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<PlayerSideItemVM> PlayerSideSelection
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
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<PlayerSideItemVM>>(value, "PlayerSideSelection");
			}
		}
	}

	[DataSourceProperty]
	public string GameTypeText
	{
		get
		{
			return _gameTypeText;
		}
		set
		{
			if (value != _gameTypeText)
			{
				_gameTypeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameTypeText");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerTypeText
	{
		get
		{
			return _playerTypeText;
		}
		set
		{
			if (value != _playerTypeText)
			{
				_playerTypeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PlayerTypeText");
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

	public GameTypeSelectionGroupVM(Action<CustomBattlePlayerType> onPlayerTypeChange, Action<string> onGameTypeChange)
	{
		_onPlayerTypeChange = onPlayerTypeChange;
		_onGameTypeChange = onGameTypeChange;
		GameTypeSelection = new SelectorVM<GameTypeItemVM>(0, (Action<SelectorVM<GameTypeItemVM>>)OnGameTypeSelection);
		PlayerTypeSelection = new SelectorVM<PlayerTypeItemVM>(0, (Action<SelectorVM<PlayerTypeItemVM>>)OnPlayerTypeSelection);
		PlayerSideSelection = new SelectorVM<PlayerSideItemVM>(0, (Action<SelectorVM<PlayerSideItemVM>>)OnPlayerSideSelection);
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
		((ViewModel)this).RefreshValues();
		GameTypeText = ((object)new TextObject("{=JPimShCw}Game Type", (Dictionary<string, object>)null)).ToString();
		PlayerTypeText = ((object)new TextObject("{=bKg8Mmwb}Player Type", (Dictionary<string, object>)null)).ToString();
		PlayerSideText = ((object)new TextObject("{=P3rMg4uZ}Player Side", (Dictionary<string, object>)null)).ToString();
		((Collection<GameTypeItemVM>)(object)GameTypeSelection.ItemList).Clear();
		((Collection<PlayerTypeItemVM>)(object)PlayerTypeSelection.ItemList).Clear();
		((Collection<PlayerSideItemVM>)(object)PlayerSideSelection.ItemList).Clear();
		foreach (Tuple<string, string> gameType in CustomBattleData.GameTypes)
		{
			GameTypeSelection.AddItem(new GameTypeItemVM(gameType.Item1, gameType.Item2));
		}
		foreach (Tuple<string, CustomBattlePlayerType> playerType in CustomBattleData.PlayerTypes)
		{
			PlayerTypeSelection.AddItem(new PlayerTypeItemVM(playerType.Item1, playerType.Item2));
		}
		foreach (Tuple<string, CustomBattlePlayerSide> playerSide in CustomBattleData.PlayerSides)
		{
			PlayerSideSelection.AddItem(new PlayerSideItemVM(playerSide.Item1, playerSide.Item2));
		}
		GameTypeSelection.SelectedIndex = 0;
		PlayerTypeSelection.SelectedIndex = 0;
		PlayerSideSelection.SelectedIndex = 0;
	}

	public void RandomizeAll()
	{
		GameTypeSelection.ExecuteRandomize();
		PlayerTypeSelection.ExecuteRandomize();
		PlayerSideSelection.ExecuteRandomize();
	}

	private void OnGameTypeSelection(SelectorVM<GameTypeItemVM> selector)
	{
		SelectedGameTypeString = selector.SelectedItem.GameTypeStringId;
		_onGameTypeChange(SelectedGameTypeString);
	}

	private void OnPlayerTypeSelection(SelectorVM<PlayerTypeItemVM> selector)
	{
		SelectedPlayerType = selector.SelectedItem.PlayerType;
		_onPlayerTypeChange(SelectedPlayerType);
	}

	private void OnPlayerSideSelection(SelectorVM<PlayerSideItemVM> selector)
	{
		SelectedPlayerSide = selector.SelectedItem.PlayerSide;
	}
}
