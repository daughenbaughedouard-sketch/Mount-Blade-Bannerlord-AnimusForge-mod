using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Lobby;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.AfterBattle;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Authentication;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Home;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.OfficialGame;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Popup;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPLobbyVM : ViewModel
{
	public enum LobbyPage
	{
		NotAssigned = 0,
		Authentication = 1,
		Rejoin = 2,
		Options = 3,
		Home = 4,
		Armory = 5,
		Matchmaking = 6,
		Profile = 7,
		HotkeySelectablePageBegin = 3,
		HotkeySelectablePageEnd = 7
	}

	private enum PartyActionType
	{
		Add,
		Remove,
		AssignLeader
	}

	private LobbyClient _lobbyClient;

	private LobbyState _lobbyState;

	private const float PlayerCountInQueueTimerInterval = 5f;

	private float _playerCountInQueueTimer;

	private PlayerData _playerDataToRefreshWith;

	private MBQueue<MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData> _partySuggestionQueue;

	private ConcurrentQueue<(PartyActionType, PlayerId, PartyRemoveReason)> _partyActionQueue;

	private bool _waitingForEscapeResult;

	private bool _isCustomGameCheckingForMaps;

	private bool _isDisconnecting;

	private bool _isRejoinRequested;

	private bool _isRejoining;

	private bool _isStartingGameFind;

	private bool? _cachedHasUserGeneratedContentPrivilege;

	private readonly Action _onForceCloseFacegen;

	private readonly Action _onLogout;

	private readonly Action<KeyOptionVM> _onKeybindRequest;

	private readonly Action<bool> _setNavigationRestriction;

	private const string _defaultSound = "event:/ui/default";

	private const string _tabSound = "event:/ui/tab";

	private const string _sortSound = "event:/ui/sort";

	private const string _purchaseSound = "event:/ui/multiplayer/shop_purchase_proceed";

	private bool _isLoggedIn;

	private bool _isArmoryActive;

	private bool _isSearchGameRequested;

	private bool _isSearchingGame;

	private bool _isMatchmakingEnabled;

	private bool _isCustomGameFindEnabled;

	private bool _isPartyLeader;

	private bool _isInParty;

	private MPLobbyBlockerStateVM _blockerState;

	private MPLobbyMenuVM _menu;

	private MPAuthenticationVM _login;

	private MPLobbyRejoinVM _rejoin;

	private MPLobbyFriendsVM _friends;

	private MPLobbyHomeVM _home;

	private MPMatchmakingVM _matchmaking;

	private MPArmoryVM _armory;

	private MPLobbyGameSearchVM _gameSearch;

	private MPLobbyPlayerProfileVM _playerProfile;

	private MPAfterBattlePopupVM _afterBattlePopup;

	private MPLobbyPartyInvitationPopupVM _partyInvitationPopup;

	private MPLobbyPartyJoinRequestPopupVM _partyJoinRequestPopup;

	private MPLobbyInformationPopup _informationPopup;

	private MPLobbyQueryPopupVM _queryPopup;

	private MPLobbyPartyPlayerSuggestionPopupVM _partyPlayerSuggestionPopup;

	private MPOptionsVM _options;

	private MPLobbyProfileVM _profile;

	private BrightnessOptionVM _brightnessPopup;

	private ExposureOptionVM _exposurePopup;

	private MPLobbyClanVM _clan;

	private MPLobbyClanCreationPopupVM _clanCreationPopup;

	private MPLobbyClanCreationInformationVM _clanCreationInformationPopup;

	private MPLobbyClanInvitationPopupVM _clanInvitationPopup;

	private MPLobbyClanMatchmakingRequestPopupVM _clanMatchmakingRequestPopup;

	private MPLobbyClanInviteFriendsPopupVM _clanInviteFriendsPopup;

	private MPLobbyClanLeaderboardVM _clanLeaderboardPopup;

	private MPCosmeticObtainPopupVM _cosmeticObtainPopup;

	private MPLobbyBannerlordIDChangePopup _bannerlordIDChangePopup;

	private MPLobbyBannerlordIDAddFriendPopupVM _bannerlordIDAddFriendPopup;

	private MPLobbyBadgeProgressInformationVM _badgeProgressionInformation;

	private MPLobbyBadgeSelectionPopupVM _badgeSelectionPopup;

	private MPLobbyHomeChangeSigilPopupVM _changeSigilPopup;

	private MPLobbyRecentGamesVM _recentGames;

	private MPLobbyRankProgressInformationVM _rankProgressInformation;

	private MPLobbyRankLeaderboardVM _rankLeaderboard;

	private PlayerId _partyLeaderId => NetworkMain.GameClient.PlayersInParty.Find((PartyPlayerInLobbyClient p) => p.IsPartyLeader).PlayerId;

	public LobbyPage CurrentPage { get; private set; }

	public List<LobbyPage> DisallowedPages { get; private set; }

	[DataSourceProperty]
	public bool IsLoggedIn
	{
		get
		{
			return _isLoggedIn;
		}
		set
		{
			if (value != _isLoggedIn)
			{
				_isLoggedIn = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsLoggedIn");
			}
		}
	}

	[DataSourceProperty]
	public BrightnessOptionVM BrightnessPopup
	{
		get
		{
			return _brightnessPopup;
		}
		set
		{
			if (value != _brightnessPopup)
			{
				_brightnessPopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<BrightnessOptionVM>(value, "BrightnessPopup");
			}
		}
	}

	[DataSourceProperty]
	public ExposureOptionVM ExposurePopup
	{
		get
		{
			return _exposurePopup;
		}
		set
		{
			if (value != _exposurePopup)
			{
				_exposurePopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<ExposureOptionVM>(value, "ExposurePopup");
			}
		}
	}

	[DataSourceProperty]
	public bool IsArmoryActive
	{
		get
		{
			return _isArmoryActive;
		}
		set
		{
			if (value != _isArmoryActive)
			{
				_isArmoryActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsArmoryActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSearchGameRequested
	{
		get
		{
			return _isSearchGameRequested;
		}
		set
		{
			if (value != _isSearchGameRequested)
			{
				_isSearchGameRequested = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSearchGameRequested");
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
			}
		}
	}

	[DataSourceProperty]
	public bool IsSearchingGame
	{
		get
		{
			return _isSearchingGame;
		}
		set
		{
			if (value != _isSearchingGame)
			{
				_isSearchingGame = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSearchingGame");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMatchmakingEnabled
	{
		get
		{
			return _isMatchmakingEnabled;
		}
		set
		{
			if (value != _isMatchmakingEnabled)
			{
				_isMatchmakingEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMatchmakingEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCustomGameFindEnabled
	{
		get
		{
			return _isCustomGameFindEnabled;
		}
		set
		{
			if (value != _isCustomGameFindEnabled)
			{
				_isCustomGameFindEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCustomGameFindEnabled");
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
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyBlockerStateVM BlockerState
	{
		get
		{
			return _blockerState;
		}
		set
		{
			if (value != _blockerState)
			{
				_blockerState = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyBlockerStateVM>(value, "BlockerState");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyMenuVM Menu
	{
		get
		{
			return _menu;
		}
		set
		{
			if (value != _menu)
			{
				_menu = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyMenuVM>(value, "Menu");
			}
		}
	}

	[DataSourceProperty]
	public MPAuthenticationVM Login
	{
		get
		{
			return _login;
		}
		set
		{
			if (value != _login)
			{
				_login = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPAuthenticationVM>(value, "Login");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyRejoinVM Rejoin
	{
		get
		{
			return _rejoin;
		}
		set
		{
			if (value != _rejoin)
			{
				_rejoin = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyRejoinVM>(value, "Rejoin");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyFriendsVM Friends
	{
		get
		{
			return _friends;
		}
		set
		{
			if (value != _friends)
			{
				_friends = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyFriendsVM>(value, "Friends");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyHomeVM Home
	{
		get
		{
			return _home;
		}
		set
		{
			if (value != _home)
			{
				_home = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyHomeVM>(value, "Home");
			}
		}
	}

	[DataSourceProperty]
	public MPMatchmakingVM Matchmaking
	{
		get
		{
			return _matchmaking;
		}
		set
		{
			if (value != _matchmaking)
			{
				_matchmaking = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPMatchmakingVM>(value, "Matchmaking");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryVM Armory
	{
		get
		{
			return _armory;
		}
		set
		{
			if (value != _armory)
			{
				_armory = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryVM>(value, "Armory");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyGameSearchVM GameSearch
	{
		get
		{
			return _gameSearch;
		}
		set
		{
			if (value != _gameSearch)
			{
				_gameSearch = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyGameSearchVM>(value, "GameSearch");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerProfileVM PlayerProfile
	{
		get
		{
			return _playerProfile;
		}
		set
		{
			if (value != _playerProfile)
			{
				_playerProfile = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPlayerProfileVM>(value, "PlayerProfile");
			}
		}
	}

	[DataSourceProperty]
	public MPAfterBattlePopupVM AfterBattlePopup
	{
		get
		{
			return _afterBattlePopup;
		}
		set
		{
			if (value != _afterBattlePopup)
			{
				_afterBattlePopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPAfterBattlePopupVM>(value, "AfterBattlePopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPartyInvitationPopupVM PartyInvitationPopup
	{
		get
		{
			return _partyInvitationPopup;
		}
		set
		{
			if (value != _partyInvitationPopup)
			{
				_partyInvitationPopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPartyInvitationPopupVM>(value, "PartyInvitationPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPartyJoinRequestPopupVM PartyJoinRequestPopup
	{
		get
		{
			return _partyJoinRequestPopup;
		}
		set
		{
			if (value != _partyJoinRequestPopup)
			{
				_partyJoinRequestPopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPartyJoinRequestPopupVM>(value, "PartyJoinRequestPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyInformationPopup InformationPopup
	{
		get
		{
			return _informationPopup;
		}
		set
		{
			if (value != _informationPopup)
			{
				_informationPopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyInformationPopup>(value, "InformationPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyQueryPopupVM QueryPopup
	{
		get
		{
			return _queryPopup;
		}
		set
		{
			if (value != _queryPopup)
			{
				_queryPopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyQueryPopupVM>(value, "QueryPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPartyPlayerSuggestionPopupVM PartyPlayerSuggestionPopup
	{
		get
		{
			return _partyPlayerSuggestionPopup;
		}
		set
		{
			if (value != _partyPlayerSuggestionPopup)
			{
				_partyPlayerSuggestionPopup = value;
				((ViewModel)this).OnPropertyChanged("PartyPlayerSuggestionPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPOptionsVM Options
	{
		get
		{
			return _options;
		}
		set
		{
			if (value != _options)
			{
				_options = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPOptionsVM>(value, "Options");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyProfileVM Profile
	{
		get
		{
			return _profile;
		}
		set
		{
			if (value != _profile)
			{
				_profile = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyProfileVM>(value, "Profile");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanVM Clan
	{
		get
		{
			return _clan;
		}
		set
		{
			if (value != _clan)
			{
				_clan = value;
				((ViewModel)this).OnPropertyChanged("Clan");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanCreationPopupVM ClanCreationPopup
	{
		get
		{
			return _clanCreationPopup;
		}
		set
		{
			if (value != _clanCreationPopup)
			{
				_clanCreationPopup = value;
				((ViewModel)this).OnPropertyChanged("ClanCreationPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanCreationInformationVM ClanCreationInformationPopup
	{
		get
		{
			return _clanCreationInformationPopup;
		}
		set
		{
			if (value != _clanCreationInformationPopup)
			{
				_clanCreationInformationPopup = value;
				((ViewModel)this).OnPropertyChanged("ClanCreationInformationPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanInvitationPopupVM ClanInvitationPopup
	{
		get
		{
			return _clanInvitationPopup;
		}
		set
		{
			if (value != _clanInvitationPopup)
			{
				_clanInvitationPopup = value;
				((ViewModel)this).OnPropertyChanged("ClanInvitationPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanMatchmakingRequestPopupVM ClanMatchmakingRequestPopup
	{
		get
		{
			return _clanMatchmakingRequestPopup;
		}
		set
		{
			if (value != _clanMatchmakingRequestPopup)
			{
				_clanMatchmakingRequestPopup = value;
				((ViewModel)this).OnPropertyChanged("ClanMatchmakingRequestPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanInviteFriendsPopupVM ClanInviteFriendsPopup
	{
		get
		{
			return _clanInviteFriendsPopup;
		}
		set
		{
			if (value != _clanInviteFriendsPopup)
			{
				_clanInviteFriendsPopup = value;
				((ViewModel)this).OnPropertyChanged("ClanInviteFriendsPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanLeaderboardVM ClanLeaderboardPopup
	{
		get
		{
			return _clanLeaderboardPopup;
		}
		set
		{
			if (value != _clanLeaderboardPopup)
			{
				_clanLeaderboardPopup = value;
				((ViewModel)this).OnPropertyChanged("ClanLeaderboardPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPCosmeticObtainPopupVM CosmeticObtainPopup
	{
		get
		{
			return _cosmeticObtainPopup;
		}
		set
		{
			if (value != _cosmeticObtainPopup)
			{
				_cosmeticObtainPopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPCosmeticObtainPopupVM>(value, "CosmeticObtainPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyBannerlordIDAddFriendPopupVM BannerlordIDAddFriendPopup
	{
		get
		{
			return _bannerlordIDAddFriendPopup;
		}
		set
		{
			if (value != _bannerlordIDAddFriendPopup)
			{
				_bannerlordIDAddFriendPopup = value;
				((ViewModel)this).OnPropertyChanged("BannerlordIDAddFriendPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyBannerlordIDChangePopup BannerlordIDChangePopup
	{
		get
		{
			return _bannerlordIDChangePopup;
		}
		set
		{
			if (value != _bannerlordIDChangePopup)
			{
				_bannerlordIDChangePopup = value;
				((ViewModel)this).OnPropertyChanged("BannerlordIDChangePopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyBadgeProgressInformationVM BadgeProgressionInformation
	{
		get
		{
			return _badgeProgressionInformation;
		}
		set
		{
			if (value != _badgeProgressionInformation)
			{
				_badgeProgressionInformation = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyBadgeProgressInformationVM>(value, "BadgeProgressionInformation");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyBadgeSelectionPopupVM BadgeSelectionPopup
	{
		get
		{
			return _badgeSelectionPopup;
		}
		set
		{
			if (value != _badgeSelectionPopup)
			{
				_badgeSelectionPopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyBadgeSelectionPopupVM>(value, "BadgeSelectionPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyHomeChangeSigilPopupVM ChangeSigilPopup
	{
		get
		{
			return _changeSigilPopup;
		}
		set
		{
			if (value != _changeSigilPopup)
			{
				_changeSigilPopup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyHomeChangeSigilPopupVM>(value, "ChangeSigilPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyRecentGamesVM RecentGames
	{
		get
		{
			return _recentGames;
		}
		set
		{
			if (value != _recentGames)
			{
				_recentGames = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyRecentGamesVM>(value, "RecentGames");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyRankProgressInformationVM RankProgressInformation
	{
		get
		{
			return _rankProgressInformation;
		}
		set
		{
			if (value != _rankProgressInformation)
			{
				_rankProgressInformation = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyRankProgressInformationVM>(value, "RankProgressInformation");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyRankLeaderboardVM RankLeaderboard
	{
		get
		{
			return _rankLeaderboard;
		}
		set
		{
			if (value != _rankLeaderboard)
			{
				_rankLeaderboard = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyRankLeaderboardVM>(value, "RankLeaderboard");
			}
		}
	}

	public MPLobbyVM(LobbyState lobbyState, Action<BasicCharacterObject> onOpenFacegen, Action onForceCloseFacegen, Action onLogout, Action<KeyOptionVM> onKeybindRequest, Func<string> getContinueKeyText, Action<bool> setNavigationRestriction)
	{
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Expected O, but got Unknown
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Expected O, but got Unknown
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Expected O, but got Unknown
		CurrentPage = LobbyPage.NotAssigned;
		_onForceCloseFacegen = onForceCloseFacegen;
		_onLogout = onLogout;
		_onKeybindRequest = onKeybindRequest;
		_setNavigationRestriction = setNavigationRestriction;
		_lobbyState = lobbyState;
		_lobbyClient = _lobbyState.LobbyClient;
		_partySuggestionQueue = new MBQueue<MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData>();
		_partyActionQueue = new ConcurrentQueue<(PartyActionType, PlayerId, PartyRemoveReason)>();
		BlockerState = new MPLobbyBlockerStateVM(_setNavigationRestriction);
		Menu = new MPLobbyMenuVM(lobbyState, _setNavigationRestriction, RequestExit);
		bool isAbleToSearchForGame = NetworkMain.GameClient.IsAbleToSearchForGame;
		IsMatchmakingEnabled = !isAbleToSearchForGame;
		IsMatchmakingEnabled = isAbleToSearchForGame;
		IsCustomGameFindEnabled = NetworkMain.GameClient.IsCustomBattleAvailable;
		IsPartyLeader = NetworkMain.GameClient.IsPartyLeader;
		IsInParty = NetworkMain.GameClient.IsInParty;
		Login = new MPAuthenticationVM(lobbyState);
		Rejoin = new MPLobbyRejoinVM(OnChangePageRequest);
		Home = new MPLobbyHomeVM(lobbyState.NewsManager, OnChangePageRequest);
		Profile = new MPLobbyProfileVM(lobbyState, OnChangePageRequest, ExecuteOpenRecentGames);
		Matchmaking = new MPMatchmakingVM(lobbyState, OnChangePageRequest, OnMatchSelectionChanged, OnGameFindStateChanged);
		Armory = new MPArmoryVM(onOpenFacegen, OnItemObtainRequested, getContinueKeyText);
		Friends = new MPLobbyFriendsVM();
		GameSearch = new MPLobbyGameSearchVM();
		PlayerProfile = new MPLobbyPlayerProfileVM(lobbyState);
		AfterBattlePopup = new MPAfterBattlePopupVM(getContinueKeyText);
		PartyInvitationPopup = new MPLobbyPartyInvitationPopupVM();
		PartyJoinRequestPopup = new MPLobbyPartyJoinRequestPopupVM();
		PartyPlayerSuggestionPopup = new MPLobbyPartyPlayerSuggestionPopupVM();
		QueryPopup = new MPLobbyQueryPopupVM();
		InformationPopup = new MPLobbyInformationPopup();
		Options = new MPOptionsVM(false, (Action)ExecuteShowBrightness, (Action)ExecuteShowExposure, _onKeybindRequest);
		BrightnessPopup = new BrightnessOptionVM((Action<bool>)null);
		ExposurePopup = new ExposureOptionVM((Action<bool>)null);
		BannerlordIDChangePopup = new MPLobbyBannerlordIDChangePopup();
		BannerlordIDAddFriendPopup = new MPLobbyBannerlordIDAddFriendPopupVM();
		Clan = new MPLobbyClanVM(OpenInviteClanMemberPopup);
		ClanCreationPopup = new MPLobbyClanCreationPopupVM();
		ClanCreationInformationPopup = new MPLobbyClanCreationInformationVM(OpenClanCreationPopup);
		ClanInvitationPopup = new MPLobbyClanInvitationPopupVM();
		ClanMatchmakingRequestPopup = new MPLobbyClanMatchmakingRequestPopupVM();
		ClanInviteFriendsPopup = new MPLobbyClanInviteFriendsPopupVM(Friends.GetAllFriends);
		ClanLeaderboardPopup = new MPLobbyClanLeaderboardVM();
		CosmeticObtainPopup = new MPCosmeticObtainPopupVM(OnItemObtained, getContinueKeyText);
		ChangeSigilPopup = new MPLobbyHomeChangeSigilPopupVM(OnItemObtainRequested);
		BadgeProgressionInformation = new MPLobbyBadgeProgressInformationVM(getContinueKeyText);
		BadgeSelectionPopup = new MPLobbyBadgeSelectionPopupVM(OnBadgeNotificationRead, OnBadgeSelectionUpdated, OnBadgeProgressInfoRequested);
		RecentGames = new MPLobbyRecentGamesVM();
		RankProgressInformation = new MPLobbyRankProgressInformationVM(getContinueKeyText);
		RankLeaderboard = new MPLobbyRankLeaderboardVM(lobbyState);
		Home.OnFindGameRequested += AutoFindGameRequested;
		Profile.OnFindGameRequested += AutoFindGameRequested;
		MPLobbyRejoinVM rejoin = Rejoin;
		rejoin.OnRejoinRequested = (Action)Delegate.Combine(rejoin.OnRejoinRequested, new Action(OnRejoinRequested));
		if (_lobbyClient.LoggedIn)
		{
			SetPage(LobbyPage.Home);
		}
		DisallowedPages = new List<LobbyPage>();
		InformationManager.ClearAllMessages();
		_lobbyState.RegisterForCustomServerAction(OnServerActionRequested);
		LobbyState lobbyState2 = _lobbyState;
		lobbyState2.OnUserGeneratedContentPrivilegeUpdated = (Action<bool>)Delegate.Combine(lobbyState2.OnUserGeneratedContentPrivilegeUpdated, new Action<bool>(OnUserGeneratedContentPrivilegeUpdated));
		InitializeCallbacks();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		((ViewModel)BlockerState).RefreshValues();
		((ViewModel)Login).RefreshValues();
		((ViewModel)Rejoin).RefreshValues();
		((ViewModel)Menu).RefreshValues();
		((ViewModel)Home).RefreshValues();
		((ViewModel)Matchmaking).RefreshValues();
		((ViewModel)Armory).RefreshValues();
		((ViewModel)Profile).RefreshValues();
		((ViewModel)Friends).RefreshValues();
		((ViewModel)GameSearch).RefreshValues();
		((ViewModel)PlayerProfile).RefreshValues();
		((ViewModel)Options).RefreshValues();
		((ViewModel)QueryPopup).RefreshValues();
		((ViewModel)AfterBattlePopup).RefreshValues();
		((ViewModel)PartyInvitationPopup).RefreshValues();
		((ViewModel)PartyJoinRequestPopup).RefreshValues();
		((ViewModel)PartyPlayerSuggestionPopup).RefreshValues();
		((ViewModel)Clan).RefreshValues();
		((ViewModel)ClanCreationPopup).RefreshValues();
		((ViewModel)ClanCreationInformationPopup).RefreshValues();
		((ViewModel)ClanInvitationPopup).RefreshValues();
		((ViewModel)ClanMatchmakingRequestPopup).RefreshValues();
		((ViewModel)ClanInviteFriendsPopup).RefreshValues();
		((ViewModel)ClanLeaderboardPopup).RefreshValues();
		((ViewModel)BannerlordIDChangePopup).RefreshValues();
		((ViewModel)BannerlordIDAddFriendPopup).RefreshValues();
		((ViewModel)CosmeticObtainPopup).RefreshValues();
		((ViewModel)ChangeSigilPopup).RefreshValues();
		((ViewModel)BadgeProgressionInformation).RefreshValues();
		((ViewModel)BadgeSelectionPopup).RefreshValues();
		((ViewModel)RecentGames).RefreshValues();
		((ViewModel)RankProgressInformation).RefreshValues();
		((ViewModel)RankLeaderboard).RefreshValues();
		((ViewModel)BrightnessPopup).RefreshValues();
		((ViewModel)ExposurePopup).RefreshValues();
		RefreshPlayerData(NetworkMain.GameClient.PlayerData);
	}

	private void OnUserGeneratedContentPrivilegeUpdated(bool hasPrivilege)
	{
		if (_cachedHasUserGeneratedContentPrivilege != hasPrivilege)
		{
			OnFriendListUpdated(forceUpdate: true);
		}
	}

	private List<CustomServerAction> OnServerActionRequested(GameServerEntry arg)
	{
		GameServerEntry obj = arg;
		if (obj != null && !obj.IsOfficial)
		{
			return new List<CustomServerAction>
			{
				new CustomServerAction(delegate
				{
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					OnShowPlayerProfile(arg.HostId);
				}, arg, ((object)GameTexts.FindText("str_mp_scoreboard_context_viewprofile", (string)null)).ToString())
			};
		}
		return null;
	}

	public void CreateInputKeyVisuals(HotKey cancelInputKey, HotKey doneInputKey, HotKey previousInputKey, HotKey nextInputKey, HotKey firstInputKey, HotKey lastInputKey)
	{
		BannerlordIDAddFriendPopup.SetCancelInputKey(cancelInputKey);
		BannerlordIDAddFriendPopup.SetDoneInputKey(doneInputKey);
		BannerlordIDChangePopup.SetCancelInputKey(cancelInputKey);
		BannerlordIDChangePopup.SetDoneInputKey(doneInputKey);
		RankLeaderboard.SetCancelInputKey(cancelInputKey);
		RankLeaderboard.SetPreviousInputKey(previousInputKey);
		RankLeaderboard.SetNextInputKey(nextInputKey);
		RankLeaderboard.SetFirstInputKey(firstInputKey);
		RankLeaderboard.SetLastInputKey(lastInputKey);
		ChangeSigilPopup.SetCancelInputKey(cancelInputKey);
		ChangeSigilPopup.SetDoneInputKey(doneInputKey);
		ClanCreationPopup.SetCancelInputKey(cancelInputKey);
		Clan.ClanOverview.ChangeSigilPopup.SetCancelInputKey(cancelInputKey);
		Clan.ClanOverview.ChangeSigilPopup.SetDoneInputKey(doneInputKey);
		Clan.ClanOverview.ChangeFactionPopup.SetCancelInputKey(cancelInputKey);
		Clan.ClanOverview.ChangeFactionPopup.SetDoneInputKey(doneInputKey);
		Clan.ClanOverview.SendAnnouncementPopup.SetCancelInputKey(cancelInputKey);
		Clan.ClanOverview.SendAnnouncementPopup.SetDoneInputKey(doneInputKey);
		Clan.ClanOverview.SetClanInformationPopup.SetCancelInputKey(doneInputKey);
		Clan.ClanOverview.SetClanInformationPopup.SetDoneInputKey(doneInputKey);
		BadgeSelectionPopup.SetCancelInputKey(cancelInputKey);
		Login.SetDoneInputKey(doneInputKey);
		Login.SetCancelInputKey(cancelInputKey);
		CosmeticObtainPopup.SetDoneInputKey(doneInputKey);
		QueryPopup.SetDoneInputKey(doneInputKey);
		QueryPopup.SetCancelInputKey(cancelInputKey);
	}

	public override void OnFinalize()
	{
		FinalizeCallbacks();
		((ViewModel)Clan.ClanOverview.ChangeSigilPopup).OnFinalize();
		((ViewModel)Clan.ClanOverview.ChangeFactionPopup).OnFinalize();
		((ViewModel)Clan.ClanOverview.SendAnnouncementPopup).OnFinalize();
		((ViewModel)BannerlordIDAddFriendPopup).OnFinalize();
		((ViewModel)BannerlordIDChangePopup).OnFinalize();
		((ViewModel)RankLeaderboard).OnFinalize();
		((ViewModel)ChangeSigilPopup).OnFinalize();
		((ViewModel)ClanCreationPopup).OnFinalize();
		((ViewModel)BadgeSelectionPopup).OnFinalize();
		((ViewModel)BadgeProgressionInformation).OnFinalize();
		((ViewModel)CosmeticObtainPopup).OnFinalize();
		InformationManager.ClearAllMessages();
		((ViewModel)Login).OnFinalize();
		((ViewModel)Armory).OnFinalize();
		((ViewModel)Matchmaking).OnFinalize();
		((ViewModel)Friends).OnFinalize();
		Home.OnFindGameRequested -= AutoFindGameRequested;
		Profile.OnFindGameRequested -= AutoFindGameRequested;
		if (_lobbyState != null)
		{
			_lobbyState.UnregisterForCustomServerAction(OnServerActionRequested);
			LobbyState lobbyState = _lobbyState;
			lobbyState.OnUserGeneratedContentPrivilegeUpdated = (Action<bool>)Delegate.Remove(lobbyState.OnUserGeneratedContentPrivilegeUpdated, new Action<bool>(OnUserGeneratedContentPrivilegeUpdated));
		}
		MPLobbyRejoinVM rejoin = Rejoin;
		rejoin.OnRejoinRequested = (Action)Delegate.Remove(rejoin.OnRejoinRequested, new Action(OnRejoinRequested));
		((ViewModel)Menu).OnFinalize();
		((ViewModel)Home).OnFinalize();
		_lobbyState = null;
		((ViewModel)this).OnFinalize();
	}

	private void InitializeCallbacks()
	{
		MPLobbyPlayerBaseVM.OnPlayerProfileRequested = OnShowPlayerProfile;
		MPLobbyPlayerBaseVM.OnBannerlordIDChangeRequested = OnBannerlordIDChangeRequested;
		MPLobbyPlayerBaseVM.OnAddFriendWithBannerlordIDRequested = OnAddFriendWithBannerlordIDRequested;
		MPLobbyPlayerBaseVM.OnSigilChangeRequested = OnSigilChangeRequested;
		MPLobbyPlayerBaseVM.OnBadgeChangeRequested = OnBadgeChangeRequested;
		MPLobbyPlayerBaseVM.OnRankProgressionRequested = OnRankProgressionRequested;
		MPLobbyPlayerBaseVM.OnRankLeaderboardRequested = OnRankLeaderboardRequested;
		MPLobbyPlayerBaseVM.OnClanPageRequested = OnClanPageRequested;
		MPLobbyPlayerBaseVM.OnClanLeaderboardRequested = OnClanLeaderboardRequested;
		MPArmoryCosmeticItemBaseVM.OnPurchaseRequested += OnItemObtainRequested;
		MPAnnouncementItemVM.OnInspect += OnAnnouncementInspected;
		MPCustomGameVM.OnMapCheckingStateChanged += OnCustomGameMapCheckingStateChanged;
	}

	private void FinalizeCallbacks()
	{
		MPLobbyPlayerBaseVM.OnPlayerProfileRequested = null;
		MPLobbyPlayerBaseVM.OnBannerlordIDChangeRequested = null;
		MPLobbyPlayerBaseVM.OnAddFriendWithBannerlordIDRequested = null;
		MPLobbyPlayerBaseVM.OnSigilChangeRequested = null;
		MPLobbyPlayerBaseVM.OnBadgeChangeRequested = null;
		MPLobbyPlayerBaseVM.OnRankProgressionRequested = null;
		MPLobbyPlayerBaseVM.OnRankLeaderboardRequested = null;
		MPLobbyPlayerBaseVM.OnClanPageRequested = null;
		MPLobbyPlayerBaseVM.OnClanLeaderboardRequested = null;
		MPArmoryCosmeticItemBaseVM.OnPurchaseRequested -= OnItemObtainRequested;
		MPAnnouncementItemVM.OnInspect -= OnAnnouncementInspected;
		MPCustomGameVM.OnMapCheckingStateChanged -= OnCustomGameMapCheckingStateChanged;
	}

	public void OnActivate()
	{
		_isRejoining = false;
		_isDisconnecting = false;
	}

	public void OnDeactivate()
	{
		_isRejoining = false;
	}

	public void OnTick(float dt)
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		IsLoggedIn = NetworkMain.GameClient.LoggedIn;
		Login.OnTick(dt);
		Friends.OnTick(dt);
		Armory.OnTick(dt);
		Home.OnTick(dt);
		PartyInvitationPopup.OnTick(dt);
		PartyJoinRequestPopup.OnTick(dt);
		UpdateBlockerState();
		if (IsSearchingGame)
		{
			_playerCountInQueueTimer += dt;
			if (_playerCountInQueueTimer >= 5f)
			{
				UpdatePlayerCountInQueue();
				_playerCountInQueueTimer = 0f;
			}
		}
		else
		{
			_playerCountInQueueTimer = 5f;
		}
		if (!PartyPlayerSuggestionPopup.IsEnabled && !Extensions.IsEmpty<MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData>((IEnumerable<MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData>)_partySuggestionQueue) && !_lobbyClient.IsPartyFull)
		{
			MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData data = ((Queue<MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData>)(object)_partySuggestionQueue).Dequeue();
			PartyPlayerSuggestionPopup.OpenWith(data);
		}
		else if (!Extensions.IsEmpty<MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData>((IEnumerable<MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData>)_partySuggestionQueue) && _lobbyClient.IsPartyFull)
		{
			((Queue<MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData>)(object)_partySuggestionQueue).Clear();
		}
		if (_partyActionQueue.TryDequeue(out var result))
		{
			var (partyActionType, playerID, reason) = result;
			switch (partyActionType)
			{
			case PartyActionType.Add:
				HandlePlayerAddedToParty(playerID);
				break;
			case PartyActionType.Remove:
				HandlePlayerRemovedFromParty(playerID, reason);
				break;
			case PartyActionType.AssignLeader:
				HandlePlayerAssignedPartyLeader(playerID);
				break;
			}
		}
		if (_playerDataToRefreshWith != null && !NetworkMain.GameClient.IsRefreshingPlayerData)
		{
			RefreshPlayerDataInternal();
		}
		else if (_playerDataToRefreshWith == null && NetworkMain.GameClient.IsRefreshingPlayerData)
		{
			_playerDataToRefreshWith = null;
			NetworkMain.GameClient.IsRefreshingPlayerData = false;
		}
		Matchmaking.OnTick(dt);
		GameSearch.OnTick(dt);
		Armory.Cosmetics.OnTick(dt);
	}

	public void OnConfirm()
	{
		if (Login.IsEnabled)
		{
			if (Login.CanTryLogin)
			{
				SoundEvent.PlaySound2D("event:/ui/default");
				Login.ExecuteLogin();
			}
		}
		else if (Options.IsEnabled)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			Options.ExecuteApply();
		}
		else if (QueryPopup.IsEnabled)
		{
			if (QueryPopup.IsInquiry)
			{
				SoundEvent.PlaySound2D("event:/ui/default");
				QueryPopup.ExecuteAccept();
			}
		}
		else if (ExposurePopup.Visible)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			ExposurePopup.ExecuteConfirm();
		}
		else if (BrightnessPopup.Visible)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			BrightnessPopup.ExecuteConfirm();
		}
		else if (ChangeSigilPopup.IsEnabled && !CosmeticObtainPopup.IsEnabled)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			ChangeSigilPopup.ExecuteChangeSigil();
		}
		else if (CosmeticObtainPopup.IsEnabled)
		{
			if (CosmeticObtainPopup.ObtainState == 0 && CosmeticObtainPopup.CanObtain)
			{
				SoundEvent.PlaySound2D("event:/ui/multiplayer/shop_purchase_proceed");
				CosmeticObtainPopup.ExecuteAction();
			}
		}
		else if (BannerlordIDAddFriendPopup.IsSelected)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			BannerlordIDAddFriendPopup.ExecuteTryAddFriend();
		}
		else if (BannerlordIDChangePopup.IsSelected)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			BannerlordIDChangePopup.ExecuteApply();
		}
		else if (Clan.ClanOverview.ChangeSigilPopup.IsSelected)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			Clan.ClanOverview.ChangeSigilPopup.ExecuteChangeSigil();
		}
		else if (Clan.ClanOverview.ChangeFactionPopup.IsSelected)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			Clan.ClanOverview.ChangeFactionPopup.ExecuteChangeFaction();
		}
		else if (Clan.ClanOverview.SetClanInformationPopup.IsSelected)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			Clan.ClanOverview.SetClanInformationPopup.ExecuteSend();
		}
		else if (Clan.ClanOverview.SendAnnouncementPopup.IsSelected)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			Clan.ClanOverview.SendAnnouncementPopup.ExecuteSend();
		}
	}

	public async void OnEscape()
	{
		if (_waitingForEscapeResult)
		{
			return;
		}
		_waitingForEscapeResult = true;
		if (CurrentPage == LobbyPage.Authentication)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			if (HasNoPopupOpen())
			{
				Login.ExecuteExit();
			}
			else
			{
				ForceClosePopups();
			}
		}
		else if (QueryPopup.IsEnabled)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			QueryPopup.ExecuteDecline();
		}
		else if (ExposurePopup.Visible)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			ExposurePopup.ExecuteCancel();
		}
		else if (BrightnessPopup.Visible)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			BrightnessPopup.ExecuteCancel();
		}
		else if (ClanCreationPopup.IsEnabled)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			ClanCreationPopup.ExecuteClosePopup();
		}
		else if (CosmeticObtainPopup.IsEnabled)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			CosmeticObtainPopup.ExecuteClosePopup();
		}
		else if (Friends.IsPlayerActionsActive)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			Friends.IsPlayerActionsActive = false;
		}
		else if (RankLeaderboard.IsPlayerActionsActive)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			RankLeaderboard.IsPlayerActionsActive = false;
		}
		else if (RecentGames.IsPlayerActionsActive)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			RecentGames.IsPlayerActionsActive = false;
		}
		else if (Armory.IsManagingTaunts)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			if (Armory.Cosmetics.SelectedTauntItem != null || Armory.Cosmetics.SelectedTauntSlot != null)
			{
				Armory.ExecuteClearTauntSelection();
			}
			else
			{
				Armory.ExecuteToggleManageTauntsState();
			}
		}
		else if (NetworkMain.GameClient.IsRefreshingPlayerData)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_exit", (string)null)).ToString(), ((object)new TextObject("{=usLhlY2j}Please wait until player data is downloaded.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
		else if (HasAnyContextMenuOpen())
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			ForceCloseContextMenus();
		}
		else if (HasNoPopupOpen())
		{
			SoundEvent.PlaySound2D("event:/ui/sort");
			await RequestExit();
		}
		else
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			ForceClosePopups();
		}
		_waitingForEscapeResult = false;
	}

	public bool HasAnyContextMenuOpen()
	{
		if (!Clan.ClanRoster.IsMemberActionsActive && !Friends.IsPlayerActionsActive && !RankLeaderboard.IsPlayerActionsActive)
		{
			return RecentGames.IsPlayerActionsActive;
		}
		return true;
	}

	public void ForceCloseContextMenus()
	{
		Clan.ClanRoster.IsMemberActionsActive = false;
		Friends.IsPlayerActionsActive = false;
		RankLeaderboard.IsPlayerActionsActive = false;
		RecentGames.IsPlayerActionsActive = false;
	}

	public bool HasNoPopupOpen()
	{
		if (!Clan.IsEnabled && !Clan.ClanOverview.ChangeFactionPopup.IsSelected && !Clan.ClanOverview.ChangeSigilPopup.IsSelected && !Clan.ClanOverview.SendAnnouncementPopup.IsSelected && !Clan.ClanOverview.SetClanInformationPopup.IsSelected && !PartyInvitationPopup.IsEnabled && !PartyJoinRequestPopup.IsEnabled && !PartyPlayerSuggestionPopup.IsEnabled && !ClanInvitationPopup.IsEnabled && !ClanMatchmakingRequestPopup.IsEnabled && !BannerlordIDChangePopup.IsSelected && !BannerlordIDAddFriendPopup.IsSelected && !CosmeticObtainPopup.IsEnabled && !AfterBattlePopup.IsEnabled && !ClanCreationPopup.IsEnabled && !ClanCreationInformationPopup.IsEnabled && !ClanInviteFriendsPopup.IsEnabled && !ClanLeaderboardPopup.IsEnabled && !BadgeProgressionInformation.IsEnabled && !BadgeSelectionPopup.IsEnabled && !ChangeSigilPopup.IsEnabled && !RecentGames.IsEnabled && !PlayerProfile.IsEnabled && !RankProgressInformation.IsEnabled && !RankLeaderboard.IsEnabled && !ExposurePopup.Visible && !BrightnessPopup.Visible)
		{
			return !QueryPopup.IsEnabled;
		}
		return false;
	}

	private void ForceClosePopups()
	{
		Clan.ExecuteClosePopup();
		Clan.ClanOverview.ChangeFactionPopup.ExecuteClosePopup();
		Clan.ClanOverview.ChangeSigilPopup.ExecuteClosePopup();
		Clan.ClanOverview.SendAnnouncementPopup.ExecuteClosePopup();
		Clan.ClanOverview.SetClanInformationPopup.ExecuteClosePopup();
		PartyInvitationPopup.Close();
		PartyJoinRequestPopup.Close();
		PartyPlayerSuggestionPopup.Close();
		ClanInvitationPopup.Close();
		ClanMatchmakingRequestPopup.Close();
		BannerlordIDChangePopup.ExecuteClosePopup();
		BannerlordIDAddFriendPopup.ExecuteClosePopup();
		CosmeticObtainPopup.ExecuteClosePopup();
		AfterBattlePopup.ExecuteClose();
		ClanCreationPopup.ExecuteClosePopup();
		ClanCreationInformationPopup.ExecuteClosePopup();
		ClanInviteFriendsPopup.ExecuteClosePopup();
		ClanLeaderboardPopup.ExecuteClosePopup();
		BadgeProgressionInformation.ExecuteClosePopup();
		BadgeSelectionPopup.ExecuteClosePopup();
		ChangeSigilPopup.ExecuteClosePopup();
		RecentGames.ExecuteClosePopup();
		PlayerProfile.ExecuteClosePopup();
		RankProgressInformation.ExecuteClosePopup();
		RankLeaderboard.ExecuteClosePopup();
		ExposurePopup.ExecuteCancel();
		BrightnessPopup.ExecuteCancel();
	}

	public async Task RequestExit()
	{
		if ((int)NetworkMain.GameClient.CurrentState == 15 || (int)NetworkMain.GameClient.CurrentState == 12 || (int)NetworkMain.GameClient.CurrentState == 2 || (int)NetworkMain.GameClient.CurrentState == 3 || (int)NetworkMain.GameClient.CurrentState == 1)
		{
			return;
		}
		while ((int)NetworkMain.GameClient.CurrentState == 6 || (int)NetworkMain.GameClient.CurrentState == 7)
		{
			await Task.Yield();
		}
		if ((int)NetworkMain.GameClient.CurrentState == 8)
		{
			if (!NetworkMain.GameClient.IsInParty || NetworkMain.GameClient.IsPartyLeader)
			{
				NetworkMain.GameClient.CancelFindGame();
			}
		}
		else if (!_isDisconnecting)
		{
			if (Input.IsGamepadActive)
			{
				InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=exitMenuOption}Exit", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=NMh61YLB}Are you sure you want to exit?", (Dictionary<string, object>)null)).ToString(), true, true, ((object)new TextObject("{=aeouhelq}Yes", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=8OkPHu4f}No", (Dictionary<string, object>)null)).ToString(), (Action)OnExit, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
			else
			{
				OnExit();
			}
		}
	}

	private void OnExit()
	{
		_isDisconnecting = true;
		_setNavigationRestriction?.Invoke(obj: true);
		_onLogout?.Invoke();
		NetworkMain.GameClient.Logout((TextObject)null);
	}

	private async void UpdatePlayerCountInQueue()
	{
		LobbyClient gameClient = NetworkMain.GameClient;
		if (gameClient.Connected && GameSearch.CustomGameMode != MPCustomGameVM.CustomGameMode.PremadeGame)
		{
			MatchmakingWaitTimeStats matchmakingWaitTimeStats = await gameClient.GetMatchmakingWaitTimes();
			(from t in (IEnumerable<MPMatchmakingItemVM>)Matchmaking.QuickplayGameTypes
				where t.IsSelected
				select t.Type).ToArray();
			GameSearch.UpdateData(matchmakingWaitTimeStats, null);
		}
	}

	public void ConnectionStateUpdated(bool isAuthenticated)
	{
		if (isAuthenticated)
		{
			RefreshRecentGames();
			Friends.OnStateActivate();
		}
		else
		{
			PartyInvitationPopup.Close();
			PartyJoinRequestPopup.Close();
			ClanInvitationPopup.Close();
		}
		OnSearchBattleCanceled();
	}

	public void ShowOptionsChangedInquiry(Action onAccept = null, Action onDecline = null)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0054: Expected O, but got Unknown
		QueryPopup.ShowInquiry(new TextObject("{=Rov73lC3}Unsaved Changes", (Dictionary<string, object>)null), new TextObject("{=u0HNU5pA}You have unsaved changes. Do you want to apply them before you continue?", (Dictionary<string, object>)null), delegate
		{
			Options.ExecuteApply();
			onAccept?.Invoke();
		}, delegate
		{
			Options.ForceCancel();
			onDecline?.Invoke();
		});
	}

	public void SetPage(LobbyPage lobbyPage, MPMatchmakingVM.MatchmakingSubPages matchmakingSubPage = MPMatchmakingVM.MatchmakingSubPages.Default)
	{
		if (CurrentPage != lobbyPage)
		{
			if (lobbyPage != LobbyPage.Authentication && CurrentPage == LobbyPage.Options && ((OptionsVM)Options).IsOptionsChanged())
			{
				ShowOptionsChangedInquiry();
			}
			bool isEnabled = false;
			bool isEnabled2 = false;
			bool isEnabled3 = false;
			bool isEnabled4 = false;
			bool isEnabled5 = false;
			bool isEnabled6 = false;
			bool isEnabled7 = false;
			bool isEnabled8 = false;
			bool isEnabled9 = false;
			switch (lobbyPage)
			{
			case LobbyPage.Matchmaking:
				isEnabled = true;
				isEnabled8 = true;
				isEnabled5 = true;
				Matchmaking.TrySetMatchmakingSubPage(matchmakingSubPage);
				break;
			case LobbyPage.Authentication:
				isEnabled2 = true;
				break;
			case LobbyPage.Rejoin:
				isEnabled3 = true;
				break;
			case LobbyPage.Armory:
				isEnabled = true;
				isEnabled8 = true;
				isEnabled6 = true;
				break;
			case LobbyPage.Home:
				isEnabled = true;
				isEnabled8 = true;
				isEnabled4 = true;
				break;
			case LobbyPage.Options:
				isEnabled = true;
				isEnabled7 = true;
				break;
			case LobbyPage.Profile:
				isEnabled = true;
				isEnabled9 = true;
				isEnabled8 = true;
				break;
			}
			_isDisconnecting = false;
			Menu.IsEnabled = isEnabled;
			Login.IsEnabled = isEnabled2;
			Rejoin.IsEnabled = isEnabled3;
			Home.IsEnabled = isEnabled4;
			Matchmaking.IsEnabled = isEnabled5;
			Armory.IsEnabled = isEnabled6;
			Options.IsEnabled = isEnabled7;
			Friends.IsEnabled = isEnabled8;
			Profile.IsEnabled = isEnabled9;
			IsArmoryActive = Armory.IsEnabled;
			if (CurrentPage == LobbyPage.Matchmaking)
			{
				Matchmaking.TrySetMatchmakingSubPage(MPMatchmakingVM.MatchmakingSubPages.QuickPlay);
			}
			else if (CurrentPage == LobbyPage.Profile)
			{
				RefreshRecentGames();
			}
			CurrentPage = lobbyPage;
			_setNavigationRestriction(CurrentPage == LobbyPage.Authentication);
			Menu.SetPage(lobbyPage);
			if (lobbyPage != LobbyPage.Profile)
			{
				Menu.HasProfileNotification = BadgeSelectionPopup.HasNotifications;
			}
			else
			{
				Menu.HasProfileNotification = false;
			}
		}
	}

	public void RefreshRecentGames()
	{
		MBReadOnlyList<MatchHistoryData> entries = ((MultiplayerLocalDataContainer<MatchHistoryData>)(object)MultiplayerLocalDataManager.Instance.MatchHistory).GetEntries();
		if (entries != null)
		{
			Profile.RefreshRecentGames(entries);
			RecentGames.RefreshData(entries);
		}
	}

	public void OnDisconnected()
	{
		if (CurrentPage != LobbyPage.Authentication)
		{
			SetPage(LobbyPage.Authentication);
		}
		Friends.UpdateCanInviteOtherPlayersToParty();
		IsPartyLeader = false;
		ForceClosePopups();
	}

	private void AutoFindGameRequested()
	{
		Matchmaking.ExecuteAutoFindGame();
	}

	public void OnServerStatusReceived(ServerStatus serverStatus)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		Matchmaking.OnServerStatusReceived(serverStatus);
		IsMatchmakingEnabled = NetworkMain.GameClient.IsAbleToSearchForGame;
		IsCustomGameFindEnabled = NetworkMain.GameClient.IsCustomBattleAvailable;
		if (!NetworkMain.GameClient.IsAbleToSearchForGame && CurrentPage == LobbyPage.Matchmaking && Matchmaking.CurrentSubPage == MPMatchmakingVM.MatchmakingSubPages.QuickPlay)
		{
			SetPage(LobbyPage.Home);
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=aO3avkK9}Matchmaking Disabled", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=5baU17n0}Matchmaking feature is currently disabled", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
		if (!serverStatus.IsCustomBattleEnabled && CurrentPage == LobbyPage.Matchmaking && (Matchmaking.CurrentSubPage == MPMatchmakingVM.MatchmakingSubPages.CustomGameList || Matchmaking.CurrentSubPage == MPMatchmakingVM.MatchmakingSubPages.CustomGame))
		{
			SetPage(LobbyPage.Home);
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=i0OEsfLt}Custom Battle Disabled", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=F7dBjl83}Custom Battle feature is currently disabled", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
	}

	public void OnRejoinBattleRequestAnswered(bool isSuccessful)
	{
		if (isSuccessful && _isRejoinRequested)
		{
			_isRejoining = true;
		}
		else
		{
			SetPage(LobbyPage.Home);
		}
	}

	public void OnRequestedToSearchBattle()
	{
		IsSearchGameRequested = true;
	}

	public void OnUpdateFindingGame(MatchmakingWaitTimeStats matchmakingWaitTimeStats, string[] gameTypeInfo)
	{
		if (!IsSearchingGame)
		{
			IsSearchGameRequested = false;
			IsSearchingGame = true;
			GameSearch.SetEnabled(enabled: true);
			Armory.SetCanOpenFacegen(enabled: false);
			Matchmaking.OnFindingGame();
		}
		if (IsSearchingGame)
		{
			_onForceCloseFacegen?.Invoke();
		}
		if (gameTypeInfo == null)
		{
			Matchmaking.GetSelectedGameTypesInfo(out gameTypeInfo);
		}
		GameSearch.UpdateData(matchmakingWaitTimeStats, gameTypeInfo);
	}

	public void OnPremadeGameCreated()
	{
		IsSearchingGame = true;
		Matchmaking.OnFindingGame();
		GameSearch.UpdatePremadeGameData();
		GameSearch.SetEnabled(enabled: true);
		Clan.ClanOverview.AreActionButtonsEnabled = false;
		Armory.SetCanOpenFacegen(enabled: false);
		_onForceCloseFacegen?.Invoke();
		SetPage(LobbyPage.Profile);
	}

	public void OnRequestedToCancelSearchBattle()
	{
		GameSearch.OnRequestedToCancelSearchBattle();
	}

	private void HandlePlayerRemovedFromParty(PlayerId playerID, PartyRemoveReason reason)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Invalid comparison between Unknown and I4
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Invalid comparison between Unknown and I4
		Friends.OnPlayerRemovedFromParty(playerID);
		if (NetworkMain.GameClient.ClanHomeInfo == null || !NetworkMain.GameClient.ClanHomeInfo.IsInClan)
		{
			RefreshClanInfo();
		}
		if (ClanCreationPopup.IsEnabled)
		{
			ClanCreationPopup.ExecuteClosePopup();
		}
		if (playerID == NetworkMain.GameClient.PlayerData.PlayerId && (int)reason != 3)
		{
			IsPartyLeader = false;
			IsInParty = false;
			TextObject val = GameTexts.FindText("str_youve_been_removed", (string)null);
			if ((int)reason == 1)
			{
				val = GameTexts.FindText("str_left_party", (string)null);
			}
			else if ((int)reason == 2)
			{
				val = GameTexts.FindText("str_party_disbanded", (string)null);
			}
			else if ((int)reason == 4)
			{
				val = GameTexts.FindText("str_party_join_declined", (string)null);
			}
			else if ((int)reason == 5)
			{
				val = GameTexts.FindText("str_party_join_permission_failed", (string)null);
			}
			InformationManager.ShowInquiry(new InquiryData(string.Empty, ((object)val).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), string.Empty, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
		GameSearch.UpdateCanCancel();
	}

	private void HandlePlayerAddedToParty(PlayerId playerID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Friends.OnPlayerAddedToParty(playerID);
		if (NetworkMain.GameClient.ClanHomeInfo == null || !NetworkMain.GameClient.ClanHomeInfo.IsInClan)
		{
			RefreshClanInfo();
		}
		IsInParty = NetworkMain.GameClient.IsInParty;
		IsPartyLeader = NetworkMain.GameClient.IsPartyLeader;
		GameSearch.UpdateCanCancel();
	}

	public void OnPlayerRemovedFromParty(PlayerId playerId, PartyRemoveReason reason)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_partyActionQueue.Enqueue((PartyActionType.Remove, playerId, reason));
	}

	public void OnPlayerAddedToParty(PlayerId playerId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_partyActionQueue.Enqueue((PartyActionType.Add, playerId, (PartyRemoveReason)0));
	}

	public void OnPlayerAssignedPartyLeader(PlayerId newPartyLeaderId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_partyActionQueue.Enqueue((PartyActionType.AssignLeader, newPartyLeaderId, (PartyRemoveReason)0));
	}

	private void HandlePlayerAssignedPartyLeader(PlayerId playerID)
	{
		Friends.OnPlayerAssignedPartyLeader();
		IsInParty = NetworkMain.GameClient.IsInParty;
		IsPartyLeader = NetworkMain.GameClient.IsPartyLeader;
		GameSearch.UpdateCanCancel();
		Friends.UpdateCanInviteOtherPlayersToParty();
	}

	public void OnPlayerSuggestedToParty(PlayerId playerId, string playerName, PlayerId suggestingPlayerId, string suggestingPlayerName)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((Queue<MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData>)(object)_partySuggestionQueue).Enqueue(new MPLobbyPartyPlayerSuggestionPopupVM.PlayerPartySuggestionData(playerId, playerName, suggestingPlayerId, suggestingPlayerName));
	}

	public void OnPlayerNameUpdated(string playerName)
	{
		Home?.OnPlayerNameUpdated(playerName);
		Profile?.OnPlayerNameUpdated(playerName);
		Clan?.OnPlayerNameUpdated(playerName);
		ClanCreationInformationPopup?.OnPlayerNameUpdated();
	}

	public void OnSearchBattleCanceled()
	{
		IsSearchGameRequested = false;
		IsSearchingGame = false;
		Clan.ClanOverview.AreActionButtonsEnabled = true;
		GameSearch.SetEnabled(enabled: false);
		Armory.SetCanOpenFacegen(enabled: true);
		Matchmaking.OnCancelFindingGame();
	}

	private async void RefreshPlayerDataInternal()
	{
		NetworkMain.GameClient.IsRefreshingPlayerData = true;
		if (NetworkMain.GameClient.Connected)
		{
			await NetworkMain.GameClient.GetCosmeticsInfo();
			Armory.RefreshPlayerData(_playerDataToRefreshWith);
			Friends.Player.UpdateWith(_playerDataToRefreshWith);
			BadgeSelectionPopup.RefreshPlayerData(_playerDataToRefreshWith);
			Matchmaking.RefreshPlayerData(_playerDataToRefreshWith);
		}
		if (NetworkMain.GameClient.Connected)
		{
			await NetworkMain.GameClient.GetClanHomeInfo();
			Home.RefreshPlayerData(_playerDataToRefreshWith);
			Profile.UpdatePlayerData(_playerDataToRefreshWith);
		}
		_playerDataToRefreshWith = null;
		NetworkMain.GameClient.IsRefreshingPlayerData = false;
		RefreshSupportedFeatures();
	}

	public void RefreshPlayerData(PlayerData playerData)
	{
		if (playerData != null)
		{
			if (_playerDataToRefreshWith != playerData)
			{
				_playerDataToRefreshWith = playerData;
			}
		}
		else
		{
			SetPage(LobbyPage.Authentication);
		}
	}

	public void RefreshSupportedFeatures()
	{
		SupportedFeatures supportedFeatures = _lobbyClient.SupportedFeatures;
		Matchmaking.OnSupportedFeaturesRefreshed(supportedFeatures);
		Menu.OnSupportedFeaturesRefreshed(supportedFeatures);
		Friends.OnSupportedFeaturesRefreshed(supportedFeatures);
		if (!Menu.IsMatchmakingSupported)
		{
			DisallowedPages.Add(LobbyPage.Matchmaking);
		}
	}

	private void UpdateBlockerState()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Invalid comparison between Unknown and I4
		State currentState = _lobbyClient.CurrentState;
		bool flag = NetworkMain.GameClient.IsRefreshingPlayerData || _isDisconnecting || _isRejoining || _isStartingGameFind || _isCustomGameCheckingForMaps || (int)currentState == 9 || (int)currentState == 10 || (int)currentState == 13 || (int)currentState == 14 || (int)currentState == 15 || (int)currentState == 16;
		if (flag && !BlockerState.IsEnabled)
		{
			TextObject description = new TextObject("{=Rc95Kq8r}Please wait...", (Dictionary<string, object>)null);
			BlockerState.OnLobbyStateIsBlocker(description);
		}
		else if (!flag && BlockerState.IsEnabled)
		{
			BlockerState.OnLobbyStateNotBlocker();
		}
	}

	private void OnChangePageRequest(LobbyPage page)
	{
		SetPage(page);
	}

	private void OnMatchSelectionChanged(string selectionInfo, bool isMatchFindPossible)
	{
		Home.OnMatchSelectionChanged(selectionInfo, isMatchFindPossible);
		Profile.OnMatchSelectionChanged(selectionInfo, isMatchFindPossible);
	}

	private void OnGameFindStateChanged(bool isStartingGameFind)
	{
		_isStartingGameFind = isStartingGameFind;
	}

	private void OnShowPlayerProfile(PlayerId playerID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		PlayerProfile.OpenWith(playerID);
	}

	private void ExecuteShowBrightness()
	{
		BrightnessPopup.Visible = true;
	}

	private void ExecuteShowExposure()
	{
		ExposurePopup.Visible = true;
	}

	private void OpenClanCreationPopup()
	{
		ClanCreationPopup.ExecuteOpenPopup();
	}

	private void CloseClanCreationPopup()
	{
		ClanCreationPopup.ExecuteClosePopup();
	}

	private void ExecuteOpenRecentGames()
	{
		RefreshRecentGames();
		RecentGames.ExecuteOpenPopup();
	}

	private void OpenInviteClanMemberPopup()
	{
		ClanInviteFriendsPopup.Open();
	}

	private void OnBadgeProgressInfoRequested(MPLobbyAchievementBadgeGroupVM achivementGroup)
	{
		BadgeProgressionInformation.OpenWith(achivementGroup);
	}

	private void OnBadgeNotificationRead()
	{
		Profile.HasBadgeNotification = BadgeSelectionPopup.HasNotifications;
		Menu.HasProfileNotification = BadgeSelectionPopup.HasNotifications;
	}

	private void OnBadgeSelectionUpdated()
	{
		PlayerData playerData = NetworkMain.GameClient.PlayerData;
		if (playerData != null)
		{
			Home.RefreshPlayerData(playerData, updateRating: false);
			Profile.UpdatePlayerData(playerData, updateStatistics: false, updateRating: false);
		}
	}

	private void OnBadgeChangeRequested(PlayerId playerID)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (playerID == NetworkMain.GameClient.PlayerID)
		{
			BadgeSelectionPopup.Open();
		}
	}

	private void OnRankProgressionRequested(MPLobbyPlayerBaseVM player)
	{
		RankProgressInformation.OpenWith(player);
	}

	private void OnRankLeaderboardRequested(string gameMode)
	{
		RankLeaderboard.OpenWith(gameMode);
	}

	private void OnClanPageRequested()
	{
		if (NetworkMain.GameClient.IsInClan)
		{
			Clan.ExecuteOpenPopup();
			return;
		}
		ClanCreationInformationPopup.RefreshWith(NetworkMain.GameClient.ClanHomeInfo);
		ClanCreationInformationPopup.ExecuteOpenPopup();
	}

	private void OnClanLeaderboardRequested()
	{
		ClanLeaderboardPopup.ExecuteOpenPopup();
	}

	public void OnNotificationsReceived(LobbyNotification[] notifications)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		List<LobbyNotification> notifications2 = notifications.Where((LobbyNotification n) => (int)n.Type == 1).ToList();
		Friends.OnFriendRequestNotificationsReceived(notifications2);
		foreach (LobbyNotification val in notifications)
		{
			if ((int)val.Type == 2)
			{
				Clan.OnNotificationReceived(val);
			}
			else if ((int)val.Type == 0)
			{
				Profile.OnNotificationReceived(val);
				BadgeSelectionPopup.OnNotificationReceived(val);
				if (!Profile.IsEnabled)
				{
					Menu.HasProfileNotification = BadgeSelectionPopup.HasNotifications;
				}
			}
		}
	}

	private void OnAddFriendWithBannerlordIDRequested(PlayerId playerID)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (playerID == NetworkMain.GameClient.PlayerID)
		{
			BannerlordIDAddFriendPopup.ExecuteOpenPopup();
		}
	}

	private void OnBannerlordIDChangeRequested(PlayerId playerID)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (playerID == NetworkMain.GameClient.PlayerID)
		{
			BannerlordIDChangePopup.ExecuteOpenPopup();
		}
	}

	private void OnItemObtainRequested(MPLobbyCosmeticSigilItemVM sigilItem)
	{
		CosmeticObtainPopup.OpenWith(sigilItem);
	}

	private void OnItemObtainRequested(MPArmoryCosmeticItemBaseVM cosmeticItem)
	{
		if (cosmeticItem is MPArmoryCosmeticClothingItemVM item)
		{
			CosmeticObtainPopup.OpenWith(item);
		}
		else if (cosmeticItem is MPArmoryCosmeticTauntItemVM item2)
		{
			CosmeticObtainPopup.OpenWith(item2, Armory.HeroPreview.HeroVisual);
		}
	}

	private void OnAnnouncementInspected(MPAnnouncementItemVM announcement)
	{
		InformationPopup.ShowInformation(announcement.Title, announcement.Description);
	}

	private void OnCustomGameMapCheckingStateChanged(bool isCheckingForMaps)
	{
		_isCustomGameCheckingForMaps = isCheckingForMaps;
	}

	private void OnItemObtained(string cosmeticID, int finalLoot)
	{
		Armory.Cosmetics.OnItemObtained(cosmeticID, finalLoot);
		ChangeSigilPopup.OnLootUpdated(finalLoot);
		Home.Player.Loot = finalLoot;
	}

	private void OnSigilChangeRequested(PlayerId playerID)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (playerID == NetworkMain.GameClient.PlayerID)
		{
			ChangeSigilPopup.Open();
		}
	}

	public void OnSigilChanged(int iconID)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		Home.Player.Sigil.RefreshWith(iconID);
		Profile.PlayerInfo.Player?.Sigil.RefreshWith(iconID);
		Banner val = Banner.CreateOneColoredEmptyBanner(0);
		BannerData val2 = new BannerData(iconID, 0, 0, new Vec2(512f, 512f), new Vec2(764f, 764f), false, false, 0f);
		val.AddIconData(val2);
		NetworkMain.GameClient.PlayerData.Sigil = val.Serialize();
	}

	public void OnClanCreationFinished()
	{
		ClanCreationPopup.IsWaiting = false;
		ClanCreationPopup.HasCreationStarted = false;
		ClanInvitationPopup.Close();
		ClanCreationPopup.ExecuteClosePopup();
		RefreshClanInfo();
	}

	public void OnEnableGenericAvatarsChanged()
	{
		OnFriendListUpdated(forceUpdate: true);
	}

	public void OnEnableGenericNamesChanged()
	{
		OnFriendListUpdated(forceUpdate: true);
	}

	public void OnFriendListUpdated(bool forceUpdate = false)
	{
		_lobbyClient.FriendIDs.Clear();
		IFriendListService[] friendListServices = PlatformServices.Instance.GetFriendListServices();
		foreach (IFriendListService val in friendListServices)
		{
			if (val.IncludeInAllFriends)
			{
				_lobbyClient.FriendIDs.AddRange(val.GetAllFriends());
			}
		}
		Friends.OnFriendListUpdated(forceUpdate);
		RecentGames.OnFriendListUpdated(forceUpdate);
		ClanCreationInformationPopup?.OnFriendListUpdated(forceUpdate);
		Home.Player.UpdateNameAndAvatar(forceUpdate);
	}

	public void OnClanInfoChanged()
	{
		Clan.OnClanInfoChanged();
		Friends.OnClanInfoChanged();
		Home.OnClanInfoChanged();
		Profile.OnClanInfoChanged();
		PlayerProfile.OnClanInfoChanged();
		if (Clan.IsEnabled && !NetworkMain.GameClient.ClanHomeInfo.IsInClan)
		{
			SetPage(LobbyPage.Home);
		}
	}

	private async void RefreshClanInfo()
	{
		await NetworkMain.GameClient.GetClanHomeInfo();
	}

	private void OnRejoinRequested()
	{
		_isRejoinRequested = true;
	}

	public static string GetLocalizedGameTypesString(string[] gameTypes)
	{
		if (gameTypes.Length == 0)
		{
			return ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", "None")).ToString();
		}
		string text = "";
		for (int i = 0; i < gameTypes.Length; i++)
		{
			text += ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", gameTypes[i])).ToString();
			if (i != gameTypes.Length - 1)
			{
				text += ", ";
			}
		}
		return text;
	}

	public static string GetLocalizedRankName(string rankID)
	{
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Expected O, but got Unknown
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Expected O, but got Unknown
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Expected O, but got Unknown
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Expected O, but got Unknown
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Expected O, but got Unknown
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Expected O, but got Unknown
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Expected O, but got Unknown
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Expected O, but got Unknown
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Expected O, but got Unknown
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Expected O, but got Unknown
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Expected O, but got Unknown
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Expected O, but got Unknown
		return rankID switch
		{
			"" => ((object)new TextObject("{=E3Bqugs0}Unranked", (Dictionary<string, object>)null)).ToString(), 
			"bronze1" => ((object)new TextObject("{=CacPs8hA}Bronze I", (Dictionary<string, object>)null)).ToString(), 
			"bronze2" => ((object)new TextObject("{=e3IeNR9W}Bronze II", (Dictionary<string, object>)null)).ToString(), 
			"bronze3" => ((object)new TextObject("{=Xsq7z0PG}Bronze III", (Dictionary<string, object>)null)).ToString(), 
			"silver1" => ((object)new TextObject("{=DUrxKsJj}Silver I", (Dictionary<string, object>)null)).ToString(), 
			"silver2" => ((object)new TextObject("{=zpAamvDv}Silver II", (Dictionary<string, object>)null)).ToString(), 
			"silver3" => ((object)new TextObject("{=HGqvwRJt}Silver III", (Dictionary<string, object>)null)).ToString(), 
			"gold1" => ((object)new TextObject("{=2faDtaGz}Gold I", (Dictionary<string, object>)null)).ToString(), 
			"gold2" => ((object)new TextObject("{=9hJtoWot}Gold II", (Dictionary<string, object>)null)).ToString(), 
			"gold3" => ((object)new TextObject("{=0FVlAbbJ}Gold III", (Dictionary<string, object>)null)).ToString(), 
			"sergeant" => ((object)new TextObject("{=g9VIbA9s}Sergeant", (Dictionary<string, object>)null)).ToString(), 
			"captain" => ((object)new TextObject("{=F70rOpkK}Captain", (Dictionary<string, object>)null)).ToString(), 
			"general" => ((object)new TextObject("{=mprankgeneral}General", (Dictionary<string, object>)null)).ToString(), 
			"conqueror" => ((object)new TextObject("{=wwbIcqsq}Conqueror", (Dictionary<string, object>)null)).ToString(), 
			_ => string.Empty, 
		};
	}
}
