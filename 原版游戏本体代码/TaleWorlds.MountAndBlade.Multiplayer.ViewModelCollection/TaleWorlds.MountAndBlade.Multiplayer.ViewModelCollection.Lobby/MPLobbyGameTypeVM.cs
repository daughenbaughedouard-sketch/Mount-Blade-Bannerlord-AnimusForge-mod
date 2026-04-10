using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPLobbyGameTypeVM : ViewModel
{
	private readonly Action<string> _onSelection;

	public readonly bool IsCasual;

	private bool _isSelected;

	private string _gameTypeID;

	private HintViewModel _hint;

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
				if (value)
				{
					OnSelected();
				}
			}
		}
	}

	[DataSourceProperty]
	public string GameTypeID
	{
		get
		{
			return _gameTypeID;
		}
		set
		{
			if (value != _gameTypeID)
			{
				_gameTypeID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameTypeID");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel Hint
	{
		get
		{
			return _hint;
		}
		set
		{
			if (value != _hint)
			{
				_hint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "Hint");
			}
		}
	}

	public MPLobbyGameTypeVM(string gameType, bool isCasual, Action<string> onSelection)
	{
		GameTypeID = gameType;
		IsCasual = isCasual;
		_onSelection = onSelection;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		Hint = new HintViewModel(GameTexts.FindText("str_multiplayer_game_stats_description", GameTypeID), (string)null);
	}

	private void OnSelected()
	{
		_onSelection?.Invoke(GameTypeID);
	}
}
