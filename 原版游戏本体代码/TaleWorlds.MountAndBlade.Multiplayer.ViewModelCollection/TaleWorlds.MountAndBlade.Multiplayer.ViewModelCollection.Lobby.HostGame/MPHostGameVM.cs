using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame;

public class MPHostGameVM : ViewModel
{
	private LobbyState _lobbyState;

	private MPCustomGameVM.CustomGameMode _customGameMode;

	private MPHostGameOptionsVM _hostGameOptions;

	private string _createText;

	[DataSourceProperty]
	public MPHostGameOptionsVM HostGameOptions
	{
		get
		{
			return _hostGameOptions;
		}
		set
		{
			if (value != _hostGameOptions)
			{
				_hostGameOptions = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPHostGameOptionsVM>(value, "HostGameOptions");
			}
		}
	}

	[DataSourceProperty]
	public string CreateText
	{
		get
		{
			return _createText;
		}
		set
		{
			if (value != _createText)
			{
				_createText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CreateText");
			}
		}
	}

	public MPHostGameVM(LobbyState lobbyState, MPCustomGameVM.CustomGameMode customGameMode)
	{
		_lobbyState = lobbyState;
		_customGameMode = customGameMode;
		HostGameOptions = new MPHostGameOptionsVM(isInMission: false, _customGameMode);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		CreateText = ((object)new TextObject("{=aRzlp5XH}CREATE", (Dictionary<string, object>)null)).ToString();
		((ViewModel)HostGameOptions).RefreshValues();
	}

	public void ExecuteStart()
	{
		if (_customGameMode == MPCustomGameVM.CustomGameMode.CustomServer)
		{
			_lobbyState.HostGame();
		}
		else if (_customGameMode == MPCustomGameVM.CustomGameMode.PremadeGame)
		{
			_lobbyState.CreatePremadeGame();
		}
	}
}
