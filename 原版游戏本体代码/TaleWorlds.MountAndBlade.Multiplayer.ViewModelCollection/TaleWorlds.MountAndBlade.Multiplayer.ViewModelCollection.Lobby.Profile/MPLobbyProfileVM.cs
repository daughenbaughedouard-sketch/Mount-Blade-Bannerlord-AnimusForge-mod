using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyProfileVM : ViewModel
{
	private readonly Action<MPLobbyVM.LobbyPage> _onChangePageRequest;

	private readonly Action _onOpenRecentGames;

	private bool _isEnabled;

	private bool _isMatchFindPossible;

	private bool _hasUnofficialModulesLoaded;

	private bool _hasBadgeNotification;

	private string _showMoreText;

	private string _findGameText;

	private string _matchFindNotPossibleText;

	private string _selectionInfoText;

	private string _recentGamesTitleText;

	private MBBindingList<MPLobbyRecentGameItemVM> _recentGamesSummary;

	private MPLobbyPlayerProfileVM _playerInfo;

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
	public string ShowMoreText
	{
		get
		{
			return _showMoreText;
		}
		set
		{
			if (value != _showMoreText)
			{
				_showMoreText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ShowMoreText");
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
				OnEnabledChanged();
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
	public string RecentGamesTitleText
	{
		get
		{
			return _recentGamesTitleText;
		}
		set
		{
			if (value != _recentGamesTitleText)
			{
				_recentGamesTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RecentGamesTitleText");
			}
		}
	}

	[DataSourceProperty]
	public bool HasBadgeNotification
	{
		get
		{
			return _hasBadgeNotification;
		}
		set
		{
			if (value != _hasBadgeNotification)
			{
				_hasBadgeNotification = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasBadgeNotification");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyRecentGameItemVM> RecentGamesSummary
	{
		get
		{
			return _recentGamesSummary;
		}
		set
		{
			if (value != _recentGamesSummary)
			{
				_recentGamesSummary = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyRecentGameItemVM>>(value, "RecentGamesSummary");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerProfileVM PlayerInfo
	{
		get
		{
			return _playerInfo;
		}
		set
		{
			if (value != _playerInfo)
			{
				_playerInfo = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPlayerProfileVM>(value, "PlayerInfo");
			}
		}
	}

	public event Action OnFindGameRequested;

	public MPLobbyProfileVM(LobbyState lobbyState, Action<MPLobbyVM.LobbyPage> onChangePageRequest, Action onOpenRecentGames)
	{
		_onChangePageRequest = onChangePageRequest;
		_onOpenRecentGames = onOpenRecentGames;
		HasUnofficialModulesLoaded = NetworkMain.GameClient.HasUnofficialModulesLoaded;
		PlayerInfo = new MPLobbyPlayerProfileVM(lobbyState);
		RecentGamesSummary = new MBBindingList<MPLobbyRecentGameItemVM>();
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
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		FindGameText = ((object)new TextObject("{=yA45PqFc}FIND GAME", (Dictionary<string, object>)null)).ToString();
		MatchFindNotPossibleText = ((object)new TextObject("{=BrYUHFsg}CHOOSE GAME", (Dictionary<string, object>)null)).ToString();
		ShowMoreText = ((object)new TextObject("{=aBCi76ig}Show More", (Dictionary<string, object>)null)).ToString();
		RecentGamesTitleText = ((object)new TextObject("{=NJolh9ye}Recent Games", (Dictionary<string, object>)null)).ToString();
		RecentGamesSummary.ApplyActionOnAllItems((Action<MPLobbyRecentGameItemVM>)delegate(MPLobbyRecentGameItemVM r)
		{
			((ViewModel)r).RefreshValues();
		});
		((ViewModel)PlayerInfo).RefreshValues();
	}

	public void RefreshRecentGames(MBReadOnlyList<MatchHistoryData> recentGames)
	{
		((Collection<MPLobbyRecentGameItemVM>)(object)RecentGamesSummary).Clear();
		IOrderedEnumerable<MatchHistoryData> source = ((IEnumerable<MatchHistoryData>)recentGames).OrderByDescending((MatchHistoryData m) => m.MatchDate);
		int num = Math.Min(3, source.Count());
		for (int num2 = 0; num2 < num; num2++)
		{
			MPLobbyRecentGameItemVM mPLobbyRecentGameItemVM = new MPLobbyRecentGameItemVM(null);
			mPLobbyRecentGameItemVM.FillFrom(source.ElementAt(num2));
			((Collection<MPLobbyRecentGameItemVM>)(object)RecentGamesSummary).Add(mPLobbyRecentGameItemVM);
		}
	}

	public void OnMatchSelectionChanged(string selectionInfo, bool isMatchFindPossible)
	{
		SelectionInfoText = selectionInfo;
		IsMatchFindPossible = isMatchFindPossible;
	}

	public void UpdatePlayerData(PlayerData playerData, bool updateStatistics = true, bool updateRating = true)
	{
		PlayerInfo.UpdatePlayerData(playerData, updateStatistics, updateRating);
	}

	public void OnPlayerNameUpdated(string playerName)
	{
		PlayerInfo?.OnPlayerNameUpdated(playerName);
	}

	public void OnNotificationReceived(LobbyNotification notification)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)notification.Type == 0)
		{
			HasBadgeNotification = true;
		}
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

	private void ExecuteOpenRecentGames()
	{
		_onOpenRecentGames?.Invoke();
	}

	public void OnClanInfoChanged()
	{
		PlayerInfo.OnClanInfoChanged();
	}

	private void OnEnabledChanged()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (PlayerInfo?.Player != null)
		{
			PlatformServices.Instance.CheckPermissionWithUser((Permission)4, PlayerInfo.Player.ProvidedID, (PermissionResult)delegate(bool hasBannerlordIDPrivilege)
			{
				PlayerInfo.Player.IsBannerlordIDSupported = hasBannerlordIDPrivilege;
			});
		}
	}
}
