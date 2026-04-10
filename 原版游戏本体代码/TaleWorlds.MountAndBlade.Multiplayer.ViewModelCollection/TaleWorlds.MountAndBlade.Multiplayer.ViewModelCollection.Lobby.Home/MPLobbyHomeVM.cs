using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Library.NewsManager;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Home;

public class MPLobbyHomeVM : ViewModel
{
	private const float _announcementUpdateIntervalInSeconds = 30f;

	private readonly Action<MPLobbyVM.LobbyPage> _onChangePageRequest;

	private bool _isEnabled;

	private bool _isMatchFindPossible;

	private bool _hasUnofficialModulesLoaded;

	private bool _isNewsAvailable;

	private string _findGameText;

	private string _matchFindNotPossibleText;

	private string _selectionInfoText;

	private string _openProfileText;

	private MPLobbyPlayerBaseVM _player;

	private MPNewsVM _news;

	private MPAnnouncementsVM _announcements;

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMatchFindPossible
	{
		get
		{
			return _isMatchFindPossible;
		}
		set
		{
			if (value != _isMatchFindPossible)
			{
				_isMatchFindPossible = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMatchFindPossible");
			}
		}
	}

	[DataSourceProperty]
	public bool HasUnofficialModulesLoaded
	{
		get
		{
			return _hasUnofficialModulesLoaded;
		}
		set
		{
			if (value != _hasUnofficialModulesLoaded)
			{
				_hasUnofficialModulesLoaded = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasUnofficialModulesLoaded");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNewsAvailable
	{
		get
		{
			return _isNewsAvailable;
		}
		set
		{
			if (value != _isNewsAvailable)
			{
				_isNewsAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsNewsAvailable");
			}
		}
	}

	[DataSourceProperty]
	public string FindGameText
	{
		get
		{
			return _findGameText;
		}
		set
		{
			if (value != _findGameText)
			{
				_findGameText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FindGameText");
			}
		}
	}

	[DataSourceProperty]
	public string MatchFindNotPossibleText
	{
		get
		{
			return _matchFindNotPossibleText;
		}
		set
		{
			if (value != _matchFindNotPossibleText)
			{
				_matchFindNotPossibleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MatchFindNotPossibleText");
			}
		}
	}

	[DataSourceProperty]
	public string SelectionInfoText
	{
		get
		{
			return _selectionInfoText;
		}
		set
		{
			if (value != _selectionInfoText)
			{
				_selectionInfoText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SelectionInfoText");
			}
		}
	}

	[DataSourceProperty]
	public string OpenProfileText
	{
		get
		{
			return _openProfileText;
		}
		set
		{
			if (value != _openProfileText)
			{
				_openProfileText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OpenProfileText");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerBaseVM Player
	{
		get
		{
			return _player;
		}
		set
		{
			if (value != _player)
			{
				_player = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPlayerBaseVM>(value, "Player");
			}
		}
	}

	[DataSourceProperty]
	public MPNewsVM News
	{
		get
		{
			return _news;
		}
		set
		{
			if (value != _news)
			{
				_news = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPNewsVM>(value, "News");
			}
		}
	}

	[DataSourceProperty]
	public MPAnnouncementsVM Announcements
	{
		get
		{
			return _announcements;
		}
		set
		{
			if (value != _announcements)
			{
				_announcements = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPAnnouncementsVM>(value, "Announcements");
			}
		}
	}

	public event Action OnFindGameRequested;

	public MPLobbyHomeVM(NewsManager newsManager, Action<MPLobbyVM.LobbyPage> onChangePageRequest)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		_onChangePageRequest = onChangePageRequest;
		HasUnofficialModulesLoaded = NetworkMain.GameClient.HasUnofficialModulesLoaded;
		Player = new MPLobbyPlayerBaseVM(NetworkMain.GameClient.PlayerID);
		News = new MPNewsVM(newsManager);
		IsNewsAvailable = true;
		Announcements = new MPAnnouncementsVM(IsNewsAvailable ? new float?(30f) : ((float?)null));
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
		FindGameText = ((object)new TextObject("{=yA45PqFc}FIND GAME", (Dictionary<string, object>)null)).ToString();
		MatchFindNotPossibleText = ((object)new TextObject("{=BrYUHFsg}CHOOSE GAME", (Dictionary<string, object>)null)).ToString();
		OpenProfileText = ((object)new TextObject("{=aBCi76ig}Show More", (Dictionary<string, object>)null)).ToString();
		((ViewModel)Player).RefreshValues();
		((ViewModel)News).RefreshValues();
	}

	public void OnTick(float dt)
	{
		Announcements.OnTick(dt);
	}

	public void RefreshPlayerData(PlayerData playerData, bool updateRating = true)
	{
		Player.UpdateWith(playerData);
		if (updateRating)
		{
			Player.UpdateRating(OnRatingReceived);
		}
	}

	private void OnRatingReceived()
	{
		Player.RefreshSelectableGameTypes(isRankedOnly: true, Player.UpdateDisplayedRankInfo);
	}

	public void OnMatchSelectionChanged(string selectionInfo, bool isMatchFindPossible)
	{
		SelectionInfoText = selectionInfo;
		IsMatchFindPossible = isMatchFindPossible;
	}

	private void ExecuteFindGame()
	{
		if (IsMatchFindPossible)
		{
			this.OnFindGameRequested?.Invoke();
		}
		else
		{
			_onChangePageRequest?.Invoke(MPLobbyVM.LobbyPage.Matchmaking);
		}
	}

	private void ExecuteOpenMatchmaking()
	{
		_onChangePageRequest?.Invoke(MPLobbyVM.LobbyPage.Matchmaking);
	}

	private void ExecuteOpenProfile()
	{
		_onChangePageRequest?.Invoke(MPLobbyVM.LobbyPage.Profile);
	}

	public void OnClanInfoChanged()
	{
		Player.UpdateClanInfo();
	}

	public void OnPlayerNameUpdated(string playerName)
	{
		Player.UpdateNameAndAvatar(forceUpdate: true);
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		((ViewModel)News).OnFinalize();
		News = null;
	}
}
