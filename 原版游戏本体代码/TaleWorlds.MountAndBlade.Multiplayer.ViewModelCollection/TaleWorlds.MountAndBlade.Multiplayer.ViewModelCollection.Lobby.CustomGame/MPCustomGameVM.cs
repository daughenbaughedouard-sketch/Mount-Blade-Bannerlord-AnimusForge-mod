using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Lobby;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame.HostGameOptions;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;

public class MPCustomGameVM : ViewModel
{
	public enum CustomGameMode
	{
		CustomServer,
		PremadeGame
	}

	private readonly LobbyState _lobbyState;

	private List<GameServerEntry> _currentCustomGameList;

	private CustomGameMode _customGameMode;

	private bool _canJoinOfficialServersAsAdmin;

	private const string _officialServerAdminBadgeName = "badge_official_server_admin";

	private InputKeyItemVM _refreshInputKey;

	private bool _isEnabled;

	private bool _isRefreshing;

	private bool _isPlayerBasedCustomBattleEnabled;

	private bool _isPremadeGameEnabled;

	private bool _isInParty;

	private bool _isPartyLeader;

	private bool _isCustomServerActionsActive;

	private bool _isAnyGameSelected;

	private bool _isCreateGamePanelActive;

	private bool _canPlayerCreateGame;

	private bool _isJoinEnabled;

	private MPCustomGameItemVM _selectedGame;

	private MPCustomGameFiltersVM _filtersData;

	private MPHostGameVM _hostGame;

	private MPCustomGameSortControllerVM _sortController;

	private MBBindingList<MPCustomGameItemVM> _gameList;

	private MBBindingList<StringPairItemWithActionVM> _customServerActionsList;

	private string _createServerText;

	private string _closeText;

	private string _refreshText;

	private string _joinText;

	private string _serverNameText;

	private string _gameTypeText;

	private string _mapText;

	private string _playerCountText;

	private string _pingText;

	private string _passwordText;

	private string _firstFactionText;

	private string _secondFactionText;

	private string _regionText;

	private string _premadeMatchTypeText;

	private string _hostText;

	private HintViewModel _isPasswordProtectedHint;

	public static bool IsPingInfoAvailable => true;

	public InputKeyItemVM RefreshInputKey
	{
		get
		{
			return _refreshInputKey;
		}
		set
		{
			if (value != _refreshInputKey)
			{
				_refreshInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "RefreshInputKey");
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
				if (IsEnabled && !IsRefreshing)
				{
					ExecuteRefresh();
				}
			}
		}
	}

	[DataSourceProperty]
	public bool IsAnyGameSelected
	{
		get
		{
			return _isAnyGameSelected;
		}
		set
		{
			if (value != _isAnyGameSelected)
			{
				_isAnyGameSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAnyGameSelected");
				UpdateIsJoinEnabled();
			}
		}
	}

	[DataSourceProperty]
	public bool IsCreateGamePanelActive
	{
		get
		{
			return _isCreateGamePanelActive;
		}
		set
		{
			if (value != _isCreateGamePanelActive)
			{
				_isCreateGamePanelActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCreateGamePanelActive");
			}
		}
	}

	[DataSourceProperty]
	public bool CanPlayerCreateGame
	{
		get
		{
			return _canPlayerCreateGame;
		}
		set
		{
			if (value != _canPlayerCreateGame)
			{
				_canPlayerCreateGame = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanPlayerCreateGame");
			}
		}
	}

	[DataSourceProperty]
	public bool IsJoinEnabled
	{
		get
		{
			return _isJoinEnabled;
		}
		set
		{
			if (value != _isJoinEnabled)
			{
				_isJoinEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsJoinEnabled");
			}
		}
	}

	[DataSourceProperty]
	public MPCustomGameItemVM SelectedGame
	{
		get
		{
			return _selectedGame;
		}
		set
		{
			if (value != _selectedGame)
			{
				_selectedGame = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPCustomGameItemVM>(value, "SelectedGame");
				IsAnyGameSelected = _selectedGame != null;
			}
		}
	}

	[DataSourceProperty]
	public MPCustomGameFiltersVM FiltersData
	{
		get
		{
			return _filtersData;
		}
		set
		{
			if (value != _filtersData)
			{
				_filtersData = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPCustomGameFiltersVM>(value, "FiltersData");
			}
		}
	}

	[DataSourceProperty]
	public MPHostGameVM HostGame
	{
		get
		{
			return _hostGame;
		}
		set
		{
			if (value != _hostGame)
			{
				_hostGame = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPHostGameVM>(value, "HostGame");
			}
		}
	}

	[DataSourceProperty]
	public MPCustomGameSortControllerVM SortController
	{
		get
		{
			return _sortController;
		}
		set
		{
			if (value != _sortController)
			{
				_sortController = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPCustomGameSortControllerVM>(value, "SortController");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPCustomGameItemVM> GameList
	{
		get
		{
			return _gameList;
		}
		set
		{
			if (value != _gameList)
			{
				_gameList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPCustomGameItemVM>>(value, "GameList");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel IsPasswordProtectedHint
	{
		get
		{
			return _isPasswordProtectedHint;
		}
		set
		{
			if (value != _isPasswordProtectedHint)
			{
				_isPasswordProtectedHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "IsPasswordProtectedHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRefreshing
	{
		get
		{
			return _isRefreshing;
		}
		set
		{
			if (value != _isRefreshing)
			{
				_isRefreshing = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRefreshing");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPartyLeader
	{
		get
		{
			return _isPartyLeader;
		}
		set
		{
			if (value != _isPartyLeader)
			{
				_isPartyLeader = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPartyLeader");
				UpdateCanPlayerCreateGame();
				UpdateIsJoinEnabled();
			}
		}
	}

	[DataSourceProperty]
	public bool IsInParty
	{
		get
		{
			return _isInParty;
		}
		set
		{
			if (value != _isInParty)
			{
				_isInParty = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInParty");
				UpdateCanPlayerCreateGame();
				UpdateIsJoinEnabled();
			}
		}
	}

	[DataSourceProperty]
	public string CreateServerText
	{
		get
		{
			return _createServerText;
		}
		set
		{
			if (value != _createServerText)
			{
				_createServerText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CreateServerText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCustomServerActionsActive
	{
		get
		{
			return _isCustomServerActionsActive;
		}
		set
		{
			if (value != _isCustomServerActionsActive)
			{
				_isCustomServerActionsActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCustomServerActionsActive");
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
	public string RefreshText
	{
		get
		{
			return _refreshText;
		}
		set
		{
			if (value != _refreshText)
			{
				_refreshText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RefreshText");
			}
		}
	}

	[DataSourceProperty]
	public string JoinText
	{
		get
		{
			return _joinText;
		}
		set
		{
			if (value != _joinText)
			{
				_joinText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "JoinText");
			}
		}
	}

	[DataSourceProperty]
	public string ServerNameText
	{
		get
		{
			return _serverNameText;
		}
		set
		{
			if (value != _serverNameText)
			{
				_serverNameText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ServerNameText");
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
	public string MapText
	{
		get
		{
			return _mapText;
		}
		set
		{
			if (value != _mapText)
			{
				_mapText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MapText");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerCountText
	{
		get
		{
			return _playerCountText;
		}
		set
		{
			if (value != _playerCountText)
			{
				_playerCountText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PlayerCountText");
			}
		}
	}

	[DataSourceProperty]
	public string PingText
	{
		get
		{
			return _pingText;
		}
		set
		{
			if (value != _pingText)
			{
				_pingText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PingText");
			}
		}
	}

	[DataSourceProperty]
	public string PasswordText
	{
		get
		{
			return _passwordText;
		}
		set
		{
			if (value != _passwordText)
			{
				_passwordText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PasswordText");
			}
		}
	}

	[DataSourceProperty]
	public string FirstFactionText
	{
		get
		{
			return _firstFactionText;
		}
		set
		{
			if (value != _firstFactionText)
			{
				_firstFactionText = value;
				((ViewModel)this).OnPropertyChanged("FirstFactionText");
			}
		}
	}

	[DataSourceProperty]
	public string SecondFactionText
	{
		get
		{
			return _secondFactionText;
		}
		set
		{
			if (value != _secondFactionText)
			{
				_secondFactionText = value;
				((ViewModel)this).OnPropertyChanged("SecondFactionText");
			}
		}
	}

	[DataSourceProperty]
	public string RegionText
	{
		get
		{
			return _regionText;
		}
		set
		{
			if (value != _regionText)
			{
				_regionText = value;
				((ViewModel)this).OnPropertyChanged("RegionText");
			}
		}
	}

	[DataSourceProperty]
	public string PremadeMatchTypeText
	{
		get
		{
			return _premadeMatchTypeText;
		}
		set
		{
			if (value != _premadeMatchTypeText)
			{
				_premadeMatchTypeText = value;
				((ViewModel)this).OnPropertyChanged("PremadeMatchTypeText");
			}
		}
	}

	[DataSourceProperty]
	public string HostText
	{
		get
		{
			return _hostText;
		}
		set
		{
			if (value != _hostText)
			{
				_hostText = value;
				((ViewModel)this).OnPropertyChanged("HostText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerBasedCustomBattleEnabled
	{
		get
		{
			return _isPlayerBasedCustomBattleEnabled;
		}
		set
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			if (_customGameMode == CustomGameMode.CustomServer)
			{
				CreateServerText = (value ? ((object)new TextObject("{=gzdNEM76}Create a Game", (Dictionary<string, object>)null)).ToString() : ((object)new TextObject("{=LrE2cUnG}Currently Disabled", (Dictionary<string, object>)null)).ToString());
				if (value != _isPlayerBasedCustomBattleEnabled)
				{
					_isPlayerBasedCustomBattleEnabled = value;
					((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerBasedCustomBattleEnabled");
					UpdateCanPlayerCreateGame();
				}
			}
		}
	}

	public bool IsPremadeGameEnabled
	{
		get
		{
			return _isPremadeGameEnabled;
		}
		set
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			if (_customGameMode == CustomGameMode.PremadeGame)
			{
				CreateServerText = (value ? ((object)new TextObject("{=gzdNEM76}Create a Game", (Dictionary<string, object>)null)).ToString() : ((object)new TextObject("{=LrE2cUnG}Currently Disabled", (Dictionary<string, object>)null)).ToString());
				if (value != _isPremadeGameEnabled)
				{
					_isPremadeGameEnabled = value;
					((ViewModel)this).OnPropertyChangedWithValue(value, "IsPremadeGameEnabled");
					UpdateCanPlayerCreateGame();
				}
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemWithActionVM> CustomServerActionsList
	{
		get
		{
			return _customServerActionsList;
		}
		set
		{
			if (value != _customServerActionsList)
			{
				_customServerActionsList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemWithActionVM>>(value, "CustomServerActionsList");
			}
		}
	}

	public static event Action<bool> OnMapCheckingStateChanged;

	public MPCustomGameVM(LobbyState lobbyState, CustomGameMode customGameMode)
	{
		_lobbyState = lobbyState;
		_currentCustomGameList = new List<GameServerEntry>();
		_customGameMode = customGameMode;
		HostGame = new MPHostGameVM(_lobbyState, _customGameMode);
		FiltersData = new MPCustomGameFiltersVM();
		GameList = new MBBindingList<MPCustomGameItemVM>();
		SortController = new MPCustomGameSortControllerVM(ref _gameList, _customGameMode);
		CustomServerActionsList = new MBBindingList<StringPairItemWithActionVM>();
		_currentCustomGameList = new List<GameServerEntry>();
		if (customGameMode == CustomGameMode.CustomServer)
		{
			_lobbyState.RegisterForCustomServerAction(OnServerActionRequested);
		}
		UpdateCanJoinOfficialServersAsAdmin();
		InitializeCallbacks();
		((ViewModel)this).RefreshValues();
	}

	private async void UpdateCanJoinOfficialServersAsAdmin()
	{
		_canJoinOfficialServersAsAdmin = (await NetworkMain.GameClient.GetPlayerBadges()).Any((Badge b) => b.StringId == "badge_official_server_admin");
	}

	private void InitializeCallbacks()
	{
		MPCustomGameFiltersVM filtersData = FiltersData;
		filtersData.OnFiltersApplied = (Action)Delegate.Combine(filtersData.OnFiltersApplied, new Action(RefreshFiltersAndSort));
	}

	private void FinalizeCallbacks()
	{
		MPCustomGameFiltersVM filtersData = FiltersData;
		filtersData.OnFiltersApplied = (Action)Delegate.Remove(filtersData.OnFiltersApplied, new Action(RefreshFiltersAndSort));
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Expected O, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		IsPasswordProtectedHint = new HintViewModel(new TextObject("{=dMdmyb3Y}Password Protected", (Dictionary<string, object>)null), (string)null);
		CreateServerText = ((object)new TextObject("{=gzdNEM76}Create a Game", (Dictionary<string, object>)null)).ToString();
		CloseText = ((object)new TextObject("{=6MQaCah5}Join a Game", (Dictionary<string, object>)null)).ToString();
		RefreshText = ((object)new TextObject("{=qFPBhVh4}Refresh", (Dictionary<string, object>)null)).ToString();
		JoinText = ((object)new TextObject("{=lWDq0Uss}JOIN", (Dictionary<string, object>)null)).ToString();
		PasswordText = ((object)new TextObject("{=8nJFaJio}Password", (Dictionary<string, object>)null)).ToString();
		ServerNameText = ((object)new TextObject("{=OVcoYxj1}Server Name", (Dictionary<string, object>)null)).ToString();
		GameTypeText = ((object)new TextObject("{=JPimShCw}Game Type", (Dictionary<string, object>)null)).ToString();
		MapText = ((object)new TextObject("{=w9m11T1y}Map", (Dictionary<string, object>)null)).ToString();
		PlayerCountText = ((object)new TextObject("{=RfXJdNye}Players", (Dictionary<string, object>)null)).ToString();
		PingText = ((object)new TextObject("{=7qySRF2T}Ping", (Dictionary<string, object>)null)).ToString();
		FirstFactionText = ((object)new TextObject("{=FhnKJODX}Faction A", (Dictionary<string, object>)null)).ToString();
		SecondFactionText = ((object)new TextObject("{=a9TcHtVw}Faction B", (Dictionary<string, object>)null)).ToString();
		RegionText = ((object)new TextObject("{=uoVKchoC}Region", (Dictionary<string, object>)null)).ToString();
		PremadeMatchTypeText = ((object)new TextObject("{=OzifZbSB}Match Type", (Dictionary<string, object>)null)).ToString();
		HostText = ((object)new TextObject("{=2baWg4Gq}Host", (Dictionary<string, object>)null)).ToString();
		GameList.ApplyActionOnAllItems((Action<MPCustomGameItemVM>)delegate(MPCustomGameItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		((ViewModel)SortController).RefreshValues();
		((ViewModel)FiltersData).RefreshValues();
		MPHostGameVM hostGame = HostGame;
		if (hostGame != null)
		{
			((ViewModel)hostGame).RefreshValues();
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		if (_lobbyState != null)
		{
			_lobbyState.UnregisterForCustomServerAction(OnServerActionRequested);
		}
		InputKeyItemVM refreshInputKey = RefreshInputKey;
		if (refreshInputKey != null)
		{
			((ViewModel)refreshInputKey).OnFinalize();
		}
		FinalizeCallbacks();
	}

	public void OnTick(float dt)
	{
		for (int i = 0; i < ((Collection<MPCustomGameItemVM>)(object)GameList).Count; i++)
		{
			((Collection<MPCustomGameItemVM>)(object)GameList)[i].UpdateIsFavorite();
		}
	}

	public void SetPremadeGameList(PremadeGameEntry[] entries)
	{
		OnGameSelected(null);
		((Collection<MPCustomGameItemVM>)(object)GameList).Clear();
		if (entries != null)
		{
			foreach (PremadeGameEntry premadeGameInfo in entries)
			{
				((Collection<MPCustomGameItemVM>)(object)GameList).Add(new MPCustomGameItemVM(premadeGameInfo, OnJoinGame));
			}
		}
	}

	public void SetCustomGameServerList(AvailableCustomGames availableCustomGames)
	{
		OnGameSelected(null);
		_currentCustomGameList = availableCustomGames.CustomGameServerInfos;
		RefreshFiltersAndSort();
	}

	private void RefreshFiltersAndSort()
	{
		OnGameSelected(null);
		((Collection<MPCustomGameItemVM>)(object)GameList).Clear();
		List<GameServerEntry> filteredServerList = FiltersData.GetFilteredServerList(_currentCustomGameList);
		GameServerEntry.FilterGameServerEntriesBasedOnCrossplay(ref filteredServerList, _lobbyState.HasCrossplayPrivilege == true);
		foreach (GameServerEntry item in filteredServerList)
		{
			((Collection<MPCustomGameItemVM>)(object)GameList).Add(new MPCustomGameItemVM(item, OnGameSelected, OnJoinGame, OnShowActionsForEntry, OnToggleFavoriteServer));
		}
		SortController.SortByCurrentState();
	}

	public async void ExecuteRefresh()
	{
		if (!IsEnabled)
		{
			return;
		}
		if (IsRefreshing)
		{
			Debug.FailedAssert("Trying to refresh game list but list is already being refreshed", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\CustomGame\\MPCustomGameVM.cs", "ExecuteRefresh", 198);
			return;
		}
		IsRefreshing = true;
		OnGameSelected(null);
		((Collection<MPCustomGameItemVM>)(object)GameList).Clear();
		Task task = null;
		if (_customGameMode == CustomGameMode.CustomServer)
		{
			task = NetworkMain.GameClient.GetCustomGameServerList();
			MultiplayerOptions.Instance.CurrentOptionsCategory = (OptionsCategory)0;
		}
		else if (_customGameMode == CustomGameMode.PremadeGame)
		{
			task = NetworkMain.GameClient.GetPremadeGameList();
			MultiplayerOptions.Instance.CurrentOptionsCategory = (OptionsCategory)1;
		}
		if (task != null)
		{
			DateTime refreshBeginTime = DateTime.Now;
			await Task.WhenAny(new Task[2]
			{
				task,
				Task.Delay(10000)
			});
			TimeSpan timeSpan = DateTime.Now - refreshBeginTime;
			if (timeSpan.TotalSeconds < 3.0)
			{
				await Task.Delay((int)((3.0 - timeSpan.TotalSeconds) * 1000.0));
			}
		}
		MultiplayerOptions.Instance.OnGameTypeChanged((MultiplayerOptionsAccessMode)1);
		foreach (GenericHostGameOptionDataVM item in (Collection<GenericHostGameOptionDataVM>)(object)HostGame.HostGameOptions.GeneralOptions)
		{
			if (item is MultipleSelectionHostGameOptionDataVM multipleSelectionHostGameOptionDataVM)
			{
				multipleSelectionHostGameOptionDataVM.RefreshList();
			}
		}
		IsRefreshing = false;
	}

	private void OnShowActionsForEntry(MPCustomGameItemVM serverVM)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		if (serverVM?.GameServerInfo == null)
		{
			return;
		}
		((Collection<StringPairItemWithActionVM>)(object)CustomServerActionsList).Clear();
		List<CustomServerAction> customActionsForServer = _lobbyState.GetCustomActionsForServer(serverVM.GameServerInfo);
		if (customActionsForServer.Count > 0)
		{
			for (int i = 0; i < customActionsForServer.Count; i++)
			{
				CustomServerAction customServerAction = customActionsForServer[i];
				((Collection<StringPairItemWithActionVM>)(object)CustomServerActionsList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteSelectCustomServerAction, customServerAction.Name, customServerAction.Name, (object)customServerAction));
			}
		}
		if (((Collection<StringPairItemWithActionVM>)(object)CustomServerActionsList).Count > 0)
		{
			IsCustomServerActionsActive = false;
			IsCustomServerActionsActive = true;
		}
	}

	private void OnGameSelected(MPCustomGameItemVM gameItem)
	{
		if (SelectedGame != null)
		{
			SelectedGame.IsSelected = false;
		}
		SelectedGame = gameItem;
		if (SelectedGame != null)
		{
			SelectedGame.IsSelected = true;
		}
	}

	public void ExecuteJoinSelectedGame()
	{
		if (IsJoinEnabled)
		{
			OnJoinGame(SelectedGame);
		}
	}

	public void OnJoinGame(MPCustomGameItemVM gameItem)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		if (gameItem == null)
		{
			Debug.FailedAssert("Server to join is null.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\CustomGame\\MPCustomGameVM.cs", "OnJoinGame", 299);
		}
		else if (gameItem.IsPasswordProtected)
		{
			string? text = ((object)GameTexts.FindText("str_password_required", (string)null)).ToString();
			string text2 = ((object)GameTexts.FindText("str_enter_password", (string)null)).ToString();
			string text3 = ((object)GameTexts.FindText("str_ok", (string)null)).ToString();
			string text4 = ((object)GameTexts.FindText("str_cancel", (string)null)).ToString();
			InformationManager.ShowTextInquiry(new TextInquiryData(text, text2, true, true, text3, text4, GetOnTryPasswordForServerAction(gameItem), (Action)null, true, (Func<string, Tuple<bool, string>>)null, "", ""), false, false);
		}
		else if (_customGameMode == CustomGameMode.CustomServer)
		{
			JoinCustomGame(gameItem.GameServerInfo);
		}
		else if (_customGameMode == CustomGameMode.PremadeGame)
		{
			JoinPremadeGame(gameItem.PremadeGameInfo);
		}
	}

	private void OnToggleFavoriteServer(MPCustomGameItemVM gameItem)
	{
		GameServerEntry gameServerInfo = gameItem.GameServerInfo;
		FavoriteServerData val = default(FavoriteServerData);
		if (MultiplayerLocalDataManager.Instance.FavoriteServers.TryGetServerData(gameServerInfo, ref val))
		{
			((MultiplayerLocalDataContainer<FavoriteServerData>)(object)MultiplayerLocalDataManager.Instance.FavoriteServers).RemoveEntry(val);
			return;
		}
		FavoriteServerData val2 = FavoriteServerData.CreateFrom(gameServerInfo);
		((MultiplayerLocalDataContainer<FavoriteServerData>)(object)MultiplayerLocalDataManager.Instance.FavoriteServers).AddEntry(val2);
	}

	private Action<string> GetOnTryPasswordForServerAction(MPCustomGameItemVM serverItem)
	{
		if (_customGameMode == CustomGameMode.CustomServer)
		{
			GameServerEntry serverInfo = serverItem.GameServerInfo;
			return delegate(string passwordInput)
			{
				JoinCustomGame(serverInfo, passwordInput);
			};
		}
		if (_customGameMode == CustomGameMode.PremadeGame)
		{
			PremadeGameEntry serverInfo2 = serverItem.PremadeGameInfo;
			return delegate(string passwordInput)
			{
				JoinPremadeGame(serverInfo2, passwordInput);
			};
		}
		return delegate
		{
			Debug.FailedAssert("Fell through game modes, should never happen", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\CustomGame\\MPCustomGameVM.cs", "GetOnTryPasswordForServerAction", 353);
		};
	}

	private List<CustomServerAction> OnServerActionRequested(GameServerEntry serverEntry)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		List<CustomServerAction> list = new List<CustomServerAction>();
		if (_canJoinOfficialServersAsAdmin || !serverEntry.IsOfficial)
		{
			CustomServerAction item = new CustomServerAction(delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Expected O, but got Unknown
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Expected O, but got Unknown
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Expected O, but got Unknown
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Expected O, but got Unknown
				//IL_006e: Unknown result type (might be due to invalid IL or missing references)
				//IL_007a: Expected O, but got Unknown
				InformationManager.ShowTextInquiry(new TextInquiryData(((object)new TextObject("{=FzG3CmEe}Join as Admin", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=MNXyaVCT}Enter Admin Password", (Dictionary<string, object>)null)).ToString(), true, true, ((object)new TextObject("{=es0Y3Bxc}Join", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString(), (Action<string>)delegate(string passwordInput)
				{
					JoinCustomGame(serverEntry, passwordInput, isJoinAsAdmin: true);
				}, (Action)null, true, (Func<string, Tuple<bool, string>>)null, "", ""), false, false);
			}, serverEntry, ((object)new TextObject("{=FzG3CmEe}Join as Admin", (Dictionary<string, object>)null)).ToString());
			list.Add(item);
		}
		return list;
	}

	private async void JoinCustomGame(GameServerEntry selectedServer, string passwordInput = "", bool isJoinAsAdmin = false)
	{
		MPCustomGameVM.OnMapCheckingStateChanged?.Invoke(obj: true);
		(bool, string) tuple = await MapCheckHelpers.CheckMaps(selectedServer);
		MPCustomGameVM.OnMapCheckingStateChanged?.Invoke(obj: false);
		if (tuple.Item1)
		{
			_lobbyState.OnClientRefusedToJoinCustomServer(selectedServer);
			string text = ((object)new TextObject("{=sVVaMyvb}You don't have at least one map ({MAP_NAME}) being played on the server or the local map is not identical. Download all missing maps from the server if you would like to join it.", (Dictionary<string, object>)null).SetTextVariable("MAP_NAME", tuple.Item2)).ToString();
			InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_couldnt_join_server", (string)null)).ToString(), text, false, true, "", ((object)GameTexts.FindText("str_dismiss", (string)null)).ToString(), (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
		else if (!(await NetworkMain.GameClient.RequestJoinCustomGame(selectedServer.Id, passwordInput, isJoinAsAdmin)))
		{
			InformationManager.ShowInquiry(new InquiryData("", ((object)GameTexts.FindText("str_couldnt_join_server", (string)null)).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), "", (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
	}

	private void JoinPremadeGame(PremadeGameEntry selectedGame, string passwordInput = "")
	{
		NetworkMain.GameClient.RequestToJoinPremadeGame(selectedGame.Id, passwordInput);
	}

	private void ExecuteSelectCustomServerAction(object actionParam)
	{
		(actionParam as CustomServerAction).Execute();
	}

	public void ExecuteOpenCreateGamePanel()
	{
		if (CanPlayerCreateGame)
		{
			IsCreateGamePanelActive = true;
		}
	}

	public void ExecuteCloseCreateGamePanel()
	{
		IsCreateGamePanelActive = false;
	}

	private void UpdateCanPlayerCreateGame()
	{
		CanPlayerCreateGame = (IsPlayerBasedCustomBattleEnabled || IsPremadeGameEnabled) && (IsPartyLeader || !IsInParty);
		if (!CanPlayerCreateGame)
		{
			IsCreateGamePanelActive = false;
		}
	}

	private void UpdateIsJoinEnabled()
	{
		IsJoinEnabled = IsAnyGameSelected && (IsPartyLeader || !IsInParty);
	}

	public void SetRefreshInputKey(HotKey hotKey)
	{
		RefreshInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}
}
