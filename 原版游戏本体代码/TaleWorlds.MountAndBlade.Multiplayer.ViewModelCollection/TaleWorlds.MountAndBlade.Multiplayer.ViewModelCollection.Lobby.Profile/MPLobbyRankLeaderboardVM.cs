using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyRankLeaderboardVM : ViewModel
{
	private const int PlayerItemsPerPage = 100;

	private string _currentGameType;

	private readonly LobbyState _lobbyState;

	private readonly TextObject _noDataAvailableTextObject = new TextObject("{=vw6Va7ho}There are currently no players in the leaderboard.", (Dictionary<string, object>)null);

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _previousInputKey;

	private InputKeyItemVM _nextInputKey;

	private InputKeyItemVM _firstInputKey;

	private InputKeyItemVM _lastInputKey;

	private int _currentPageIndex;

	private int _totalPageCount;

	private bool _isEnabled;

	private bool _isDataLoading;

	private bool _hasData;

	private bool _isPlayerActionsActive;

	private bool _isPreviousPageAvailable;

	private bool _isNextPageAvailable;

	private string _titleText;

	private string _closeText;

	private string _noDataAvailableText;

	private string _currentPageText;

	private MBBindingList<MPLobbyLeaderboardPlayerItemVM> _leaderboardPlayers;

	private MBBindingList<StringPairItemWithActionVM> _playerActions;

	private HintViewModel _firstPageHint;

	private HintViewModel _lastPageHint;

	private HintViewModel _previousPageHint;

	private HintViewModel _nextPageHint;

	[DataSourceProperty]
	public InputKeyItemVM CancelInputKey
	{
		get
		{
			return _cancelInputKey;
		}
		set
		{
			if (value != _cancelInputKey)
			{
				_cancelInputKey = value;
				((ViewModel)this).OnPropertyChanged("CancelInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM PreviousInputKey
	{
		get
		{
			return _previousInputKey;
		}
		set
		{
			if (value != _previousInputKey)
			{
				_previousInputKey = value;
				((ViewModel)this).OnPropertyChanged("PreviousInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM NextInputKey
	{
		get
		{
			return _nextInputKey;
		}
		set
		{
			if (value != _nextInputKey)
			{
				_nextInputKey = value;
				((ViewModel)this).OnPropertyChanged("NextInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM FirstInputKey
	{
		get
		{
			return _firstInputKey;
		}
		set
		{
			if (value != _firstInputKey)
			{
				_firstInputKey = value;
				((ViewModel)this).OnPropertyChanged("FirstInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM LastInputKey
	{
		get
		{
			return _lastInputKey;
		}
		set
		{
			if (value != _lastInputKey)
			{
				_lastInputKey = value;
				((ViewModel)this).OnPropertyChanged("LastInputKey");
			}
		}
	}

	[DataSourceProperty]
	public int CurrentPageIndex
	{
		get
		{
			return _currentPageIndex;
		}
		set
		{
			if (value != _currentPageIndex)
			{
				_currentPageIndex = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CurrentPageIndex");
				RefreshCurrentPageText();
				RefreshButtonsDisabled();
			}
		}
	}

	[DataSourceProperty]
	public int TotalPageCount
	{
		get
		{
			return _totalPageCount;
		}
		set
		{
			if (value != _totalPageCount)
			{
				_totalPageCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TotalPageCount");
				RefreshCurrentPageText();
				RefreshButtonsDisabled();
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
			}
		}
	}

	[DataSourceProperty]
	public bool IsDataLoading
	{
		get
		{
			return _isDataLoading;
		}
		set
		{
			if (value != _isDataLoading)
			{
				_isDataLoading = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDataLoading");
				RefreshButtonsDisabled();
			}
		}
	}

	[DataSourceProperty]
	public bool HasData
	{
		get
		{
			return _hasData;
		}
		set
		{
			if (value != _hasData)
			{
				_hasData = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasData");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerActionsActive
	{
		get
		{
			return _isPlayerActionsActive;
		}
		set
		{
			if (value != _isPlayerActionsActive)
			{
				_isPlayerActionsActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerActionsActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPreviousPageAvailable
	{
		get
		{
			return _isPreviousPageAvailable;
		}
		set
		{
			if (value != _isPreviousPageAvailable)
			{
				_isPreviousPageAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPreviousPageAvailable");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNextPageAvailable
	{
		get
		{
			return _isNextPageAvailable;
		}
		set
		{
			if (value != _isNextPageAvailable)
			{
				_isNextPageAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsNextPageAvailable");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string CloseText
	{
		get
		{
			return _closeText;
		}
		set
		{
			if (value != _closeText)
			{
				_closeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CloseText");
			}
		}
	}

	[DataSourceProperty]
	public string NoDataAvailableText
	{
		get
		{
			return _noDataAvailableText;
		}
		set
		{
			if (value != _noDataAvailableText)
			{
				_noDataAvailableText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NoDataAvailableText");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentPageText
	{
		get
		{
			return _currentPageText;
		}
		set
		{
			if (value != _currentPageText)
			{
				_currentPageText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentPageText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyLeaderboardPlayerItemVM> LeaderboardPlayers
	{
		get
		{
			return _leaderboardPlayers;
		}
		set
		{
			if (value != _leaderboardPlayers)
			{
				_leaderboardPlayers = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyLeaderboardPlayerItemVM>>(value, "LeaderboardPlayers");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemWithActionVM> PlayerActions
	{
		get
		{
			return _playerActions;
		}
		set
		{
			if (value != _playerActions)
			{
				_playerActions = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemWithActionVM>>(value, "PlayerActions");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel PreviousPageHint
	{
		get
		{
			return _previousPageHint;
		}
		set
		{
			if (value != _previousPageHint)
			{
				_previousPageHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "PreviousPageHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel NextPageHint
	{
		get
		{
			return _nextPageHint;
		}
		set
		{
			if (value != _nextPageHint)
			{
				_nextPageHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "NextPageHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel FirstPageHint
	{
		get
		{
			return _firstPageHint;
		}
		set
		{
			if (value != _firstPageHint)
			{
				_firstPageHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "FirstPageHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel LastPageHint
	{
		get
		{
			return _lastPageHint;
		}
		set
		{
			if (value != _lastPageHint)
			{
				_lastPageHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "LastPageHint");
			}
		}
	}

	public MPLobbyRankLeaderboardVM(LobbyState lobbyState)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		_lobbyState = lobbyState;
		LeaderboardPlayers = new MBBindingList<MPLobbyLeaderboardPlayerItemVM>();
		PlayerActions = new MBBindingList<StringPairItemWithActionVM>();
		FirstPageHint = new HintViewModel(GameTexts.FindText("str_first_page", (string)null), (string)null);
		LastPageHint = new HintViewModel(GameTexts.FindText("str_last_page", (string)null), (string)null);
		PreviousPageHint = new HintViewModel(GameTexts.FindText("str_previous", (string)null), (string)null);
		NextPageHint = new HintViewModel(GameTexts.FindText("str_next", (string)null), (string)null);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		CloseText = ((object)new TextObject("{=yQstzabbe}Close", (Dictionary<string, object>)null)).ToString();
		NoDataAvailableText = ((object)_noDataAvailableTextObject).ToString();
		RefreshTitleText();
		RefreshCurrentPageText();
	}

	private void RefreshCurrentPageText()
	{
		CurrentPageText = ((object)GameTexts.FindText("str_LEFT_over_RIGHT", (string)null).SetTextVariable("LEFT", CurrentPageIndex + 1).SetTextVariable("RIGHT", TotalPageCount)).ToString();
	}

	private void RefreshButtonsDisabled()
	{
		IsPreviousPageAvailable = !IsDataLoading && CurrentPageIndex > 0;
		IsNextPageAvailable = !IsDataLoading && CurrentPageIndex < TotalPageCount - 1;
	}

	private async void LoadDataForPage(int pageIndex)
	{
		IsDataLoading = true;
		((Collection<MPLobbyLeaderboardPlayerItemVM>)(object)LeaderboardPlayers).Clear();
		int startIndex = 100 * pageIndex;
		PlayerLeaderboardData[] leaderboardPlayerInfos = await NetworkMain.GameClient.GetRankedLeaderboard(_currentGameType, startIndex, 100);
		await _lobbyState.UpdateHasUserGeneratedContentPrivilege(showResolveUI: true);
		if (leaderboardPlayerInfos != null && leaderboardPlayerInfos.Length != 0)
		{
			for (int i = 0; i < leaderboardPlayerInfos.Length; i++)
			{
				MPLobbyLeaderboardPlayerItemVM item = new MPLobbyLeaderboardPlayerItemVM(i + 1 + startIndex, leaderboardPlayerInfos[i], ActivatePlayerActions);
				((Collection<MPLobbyLeaderboardPlayerItemVM>)(object)LeaderboardPlayers).Add(item);
			}
		}
		IsDataLoading = false;
	}

	public async void OpenWith(string gameType)
	{
		_currentGameType = gameType;
		RefreshTitleText();
		CurrentPageIndex = 0;
		HasData = false;
		IsEnabled = true;
		IsDataLoading = true;
		((Collection<MPLobbyLeaderboardPlayerItemVM>)(object)LeaderboardPlayers).Clear();
		int num = await NetworkMain.GameClient.GetRankedLeaderboardCount(gameType);
		TotalPageCount = (num + 100 - 1) / 100;
		HasData = num > 0;
		if (HasData)
		{
			LoadDataForPage(0);
		}
		else
		{
			IsDataLoading = false;
		}
	}

	public void ExecuteLoadFirstPage()
	{
		if (IsPreviousPageAvailable)
		{
			CurrentPageIndex = 0;
			LoadDataForPage(CurrentPageIndex);
		}
	}

	public void ExecuteLoadPreviousPage()
	{
		if (IsPreviousPageAvailable)
		{
			CurrentPageIndex--;
			LoadDataForPage(CurrentPageIndex);
		}
	}

	public void ExecuteLoadNextPage()
	{
		if (IsNextPageAvailable)
		{
			CurrentPageIndex++;
			LoadDataForPage(CurrentPageIndex);
		}
	}

	public void ExecuteLoadLastPage()
	{
		if (IsNextPageAvailable)
		{
			CurrentPageIndex = TotalPageCount - 1;
			LoadDataForPage(CurrentPageIndex);
		}
	}

	public void ExecuteClosePopup()
	{
		IsEnabled = false;
	}

	private void RefreshTitleText()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (string.IsNullOrEmpty(_currentGameType))
		{
			TitleText = ((object)new TextObject("{=vGF5S2hE}Leaderboard", (Dictionary<string, object>)null)).ToString();
		}
		else
		{
			TitleText = ((object)GameTexts.FindText("str_LEFT_colon_RIGHT_wSpaceAfterColon", (string)null).SetTextVariable("LEFT", ((object)new TextObject("{=vGF5S2hE}Leaderboard", (Dictionary<string, object>)null)).ToString()).SetTextVariable("RIGHT", ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", _currentGameType)).ToString())).ToString();
		}
	}

	public void ActivatePlayerActions(MPLobbyLeaderboardPlayerItemVM playerVM)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Clear();
		if (playerVM.ProvidedID != NetworkMain.GameClient.PlayerID)
		{
			bool flag = false;
			FriendInfo[] friendInfos = NetworkMain.GameClient.FriendInfos;
			for (int i = 0; i < friendInfos.Length; i++)
			{
				if (friendInfos[i].Id == playerVM.ProvidedID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteRequestFriendship, ((object)new TextObject("{=UwkpJq9N}Add As Friend", (Dictionary<string, object>)null)).ToString(), "RequestFriendship", (object)playerVM));
			}
			else
			{
				((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteTerminateFriendship, ((object)new TextObject("{=2YIVRuRa}Remove From Friends", (Dictionary<string, object>)null)).ToString(), "TerminateFriendship", (object)playerVM));
			}
			MultiplayerPlayerContextMenuHelper.AddLobbyViewProfileOptions(playerVM, PlayerActions);
			StringPairItemWithActionVM val = new StringPairItemWithActionVM((Action<object>)ExecuteReport, ((object)GameTexts.FindText("str_mp_scoreboard_context_report", (string)null)).ToString(), "Report", (object)playerVM);
			if (MultiplayerReportPlayerManager.IsPlayerReportedOverLimit(playerVM.ProvidedID))
			{
				val.IsEnabled = false;
				val.Hint.HintText = new TextObject("{=klkYFik9}You've already reported this player.", (Dictionary<string, object>)null);
			}
			((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(val);
		}
		IsPlayerActionsActive = false;
		IsPlayerActionsActive = ((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Count > 0;
	}

	private void ExecuteRequestFriendship(object playerObj)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		PlayerId providedID = (playerObj as MPLobbyLeaderboardPlayerItemVM).ProvidedID;
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(providedID);
		NetworkMain.GameClient.AddFriend(providedID, flag);
	}

	private void ExecuteTerminateFriendship(object playerObj)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		NetworkMain.GameClient.RemoveFriend((playerObj as MPLobbyLeaderboardPlayerItemVM).ProvidedID);
	}

	private void ExecuteReport(object playerObj)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerReportPlayerManager.RequestReportPlayer(Guid.Empty.ToString(), (playerObj as MPLobbyLeaderboardPlayerItemVM).ProvidedID, (playerObj as MPLobbyLeaderboardPlayerItemVM).Name, isRequestedFromMission: false);
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM cancelInputKey = CancelInputKey;
		if (cancelInputKey != null)
		{
			((ViewModel)cancelInputKey).OnFinalize();
		}
		InputKeyItemVM previousInputKey = PreviousInputKey;
		if (previousInputKey != null)
		{
			((ViewModel)previousInputKey).OnFinalize();
		}
		InputKeyItemVM nextInputKey = NextInputKey;
		if (nextInputKey != null)
		{
			((ViewModel)nextInputKey).OnFinalize();
		}
		InputKeyItemVM firstInputKey = FirstInputKey;
		if (firstInputKey != null)
		{
			((ViewModel)firstInputKey).OnFinalize();
		}
		InputKeyItemVM lastInputKey = LastInputKey;
		if (lastInputKey != null)
		{
			((ViewModel)lastInputKey).OnFinalize();
		}
	}

	public void SetCancelInputKey(HotKey hotKey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetPreviousInputKey(HotKey hotKey)
	{
		PreviousInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetNextInputKey(HotKey hotKey)
	{
		NextInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetFirstInputKey(HotKey hotKey)
	{
		FirstInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetLastInputKey(HotKey hotKey)
	{
		LastInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}
}
