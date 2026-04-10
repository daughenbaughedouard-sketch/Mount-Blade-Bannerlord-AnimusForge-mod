using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Avatar.PlayerServices;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;
using TaleWorlds.MountAndBlade.Diamond.Ranked;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

public class MPLobbyPlayerBaseVM : ViewModel
{
	public enum OnlineStatus
	{
		None,
		InGame,
		Online,
		Offline
	}

	public static Action<PlayerId> OnPlayerProfileRequested;

	public static Action<PlayerId> OnBannerlordIDChangeRequested;

	public static Action<PlayerId> OnAddFriendWithBannerlordIDRequested;

	public static Action<PlayerId> OnSigilChangeRequested;

	public static Action<PlayerId> OnBadgeChangeRequested;

	public static Action<MPLobbyPlayerBaseVM> OnRankProgressionRequested;

	public static Action<string> OnRankLeaderboardRequested;

	public static Action OnClanPageRequested;

	public static Action OnClanLeaderboardRequested;

	private const int DefaultBannerBackgroundColorId = 99;

	private PlayerId _providedID;

	private readonly string _forcedName = string.Empty;

	private int _forcedAvatarIndex = -1;

	private Action<PlayerId> _onInviteToParty;

	private readonly Action<PlayerId> _onInviteToClan;

	private readonly Action<PlayerId> _onFriendRequestAnswered;

	private bool _isKnownPlayer;

	private readonly TextObject _genericPlayerName = new TextObject("{=RN6zHak0}Player", (Dictionary<string, object>)null);

	public Action OnPlayerStatsReceived;

	protected bool _hasReceivedPlayerStats;

	protected bool _isReceivingPlayerStats;

	private const string _skirmishGameTypeID = "Skirmish";

	private const string _captainGameTypeID = "Captain";

	private const string _duelGameTypeID = "Duel";

	private const string _teamDeathmatchGameTypeID = "TeamDeathmatch";

	private const string _siegeGameTypeID = "Siege";

	public Action<string> OnRankInfoChanged;

	private bool _canCopyID;

	private bool _showLevel;

	private bool _isSelected;

	private bool _hasNotification;

	private bool _isFriendRequest;

	private bool _isPendingRequest;

	private bool _canRemove;

	private bool _canBeInvited;

	private bool _canInviteToParty;

	private bool _canInviteToClan;

	private bool _isSigilChangeInformationEnabled;

	private bool _isRankInfoLoading;

	private bool _isRankInfoCasual;

	private bool _isClanInfoSupported;

	private bool _isBannerlordIDSupported;

	private int _level;

	private int _rating;

	private int _loot;

	private int _experienceRatio;

	private int _ratingRatio;

	private string _name = "";

	private string _stateText;

	private string _levelText;

	private string _levelTitleText;

	private string _ratingText;

	private string _gameTypeText;

	private string _ratingID;

	private string _clanName;

	private string _clanTag;

	private string _changeText;

	private string _clanInfoTitleText;

	private string _badgeInfoTitleText;

	private string _avatarInfoTitleText;

	private string _experienceText;

	private string _rankText;

	private string _bannerlordID;

	private string _selectedBadgeID;

	private HintViewModel _nameHint;

	private HintViewModel _inviteToPartyHint;

	private HintViewModel _removeFriendHint;

	private HintViewModel _acceptFriendRequestHint;

	private HintViewModel _declineFriendRequestHint;

	private HintViewModel _cancelFriendRequestHint;

	private HintViewModel _inviteToClanHint;

	private HintViewModel _changeBannerlordIDHint;

	private HintViewModel _copyBannerlordIDHint;

	private HintViewModel _addFriendWithBannerlordIDHint;

	private HintViewModel _experienceHint;

	private HintViewModel _ratingHint;

	private HintViewModel _lootHint;

	private HintViewModel _skirmishRatingHint;

	private HintViewModel _captainRatingHint;

	private HintViewModel _clanLeaderboardHint;

	private PlayerAvatarImageIdentifierVM _avatar;

	private BannerImageIdentifierVM _clanBanner;

	private MPLobbySigilItemVM _sigil;

	private MPLobbyBadgeItemVM _shownBadge;

	private CharacterViewModel _characterVisual;

	private MBBindingList<MPLobbyPlayerStatItemVM> _displayedStats;

	private MBBindingList<MPLobbyGameTypeVM> _gameTypes;

	public OnlineStatus CurrentOnlineStatus { get; private set; }

	public PlayerId ProvidedID
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _providedID;
		}
		protected set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (_providedID != value)
			{
				_providedID = value;
				LobbyClient gameClient = NetworkMain.GameClient;
				UpdateAvatar(gameClient != null && gameClient.IsKnownPlayer(ProvidedID));
			}
		}
	}

	public PlayerData PlayerData { get; private set; }

	public AnotherPlayerState State { get; protected set; }

	public float TimeSinceLastStateUpdate { get; protected set; }

	public PlayerStatsBase[] PlayerStats { get; private set; }

	public GameTypeRankInfo[] RankInfo { get; private set; }

	public string RankInfoGameTypeID { get; private set; }

	[DataSourceProperty]
	public bool CanCopyID
	{
		get
		{
			return _canCopyID;
		}
		set
		{
			if (value != _canCopyID)
			{
				_canCopyID = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanCopyID");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowLevel
	{
		get
		{
			return _showLevel;
		}
		set
		{
			if (value != _showLevel)
			{
				_showLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowLevel");
			}
		}
	}

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
			}
		}
	}

	[DataSourceProperty]
	public bool HasNotification
	{
		get
		{
			return _hasNotification;
		}
		set
		{
			if (value != _hasNotification)
			{
				_hasNotification = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasNotification");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFriendRequest
	{
		get
		{
			return _isFriendRequest;
		}
		set
		{
			if (value != _isFriendRequest)
			{
				_isFriendRequest = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFriendRequest");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPendingRequest
	{
		get
		{
			return _isPendingRequest;
		}
		set
		{
			if (value != _isPendingRequest)
			{
				_isPendingRequest = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPendingRequest");
			}
		}
	}

	[DataSourceProperty]
	public bool CanRemove
	{
		get
		{
			return _canRemove;
		}
		set
		{
			if (value != _canRemove)
			{
				_canRemove = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanRemove");
			}
		}
	}

	[DataSourceProperty]
	public bool CanBeInvited
	{
		get
		{
			return _canBeInvited;
		}
		set
		{
			if (value != _canBeInvited)
			{
				_canBeInvited = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanBeInvited");
			}
		}
	}

	[DataSourceProperty]
	public bool CanInviteToParty
	{
		get
		{
			return _canInviteToParty;
		}
		set
		{
			if (value != _canInviteToParty)
			{
				_canInviteToParty = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanInviteToParty");
			}
		}
	}

	[DataSourceProperty]
	public bool CanInviteToClan
	{
		get
		{
			return _canInviteToClan;
		}
		set
		{
			if (value != _canInviteToClan)
			{
				_canInviteToClan = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanInviteToClan");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSigilChangeInformationEnabled
	{
		get
		{
			return _isSigilChangeInformationEnabled;
		}
		set
		{
			if (value != _isSigilChangeInformationEnabled)
			{
				_isSigilChangeInformationEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSigilChangeInformationEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRankInfoLoading
	{
		get
		{
			return _isRankInfoLoading;
		}
		set
		{
			if (value != _isRankInfoLoading)
			{
				_isRankInfoLoading = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRankInfoLoading");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRankInfoCasual
	{
		get
		{
			return _isRankInfoCasual;
		}
		set
		{
			if (value != _isRankInfoCasual)
			{
				_isRankInfoCasual = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRankInfoCasual");
			}
		}
	}

	[DataSourceProperty]
	public bool IsClanInfoSupported
	{
		get
		{
			return _isClanInfoSupported;
		}
		set
		{
			if (value != _isClanInfoSupported)
			{
				_isClanInfoSupported = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsClanInfoSupported");
			}
		}
	}

	[DataSourceProperty]
	public bool IsBannerlordIDSupported
	{
		get
		{
			return _isBannerlordIDSupported;
		}
		set
		{
			if (value != _isBannerlordIDSupported)
			{
				_isBannerlordIDSupported = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBannerlordIDSupported");
			}
		}
	}

	[DataSourceProperty]
	public int Level
	{
		get
		{
			return _level;
		}
		set
		{
			if (value != _level)
			{
				_level = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Level");
			}
		}
	}

	[DataSourceProperty]
	public int Rating
	{
		get
		{
			return _rating;
		}
		set
		{
			if (value != _rating)
			{
				_rating = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Rating");
			}
		}
	}

	[DataSourceProperty]
	public int Loot
	{
		get
		{
			return _loot;
		}
		set
		{
			if (value != _loot)
			{
				_loot = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Loot");
			}
		}
	}

	[DataSourceProperty]
	public int ExperienceRatio
	{
		get
		{
			return _experienceRatio;
		}
		set
		{
			if (value != _experienceRatio)
			{
				_experienceRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ExperienceRatio");
			}
		}
	}

	[DataSourceProperty]
	public int RatingRatio
	{
		get
		{
			return _ratingRatio;
		}
		set
		{
			if (value != _ratingRatio)
			{
				_ratingRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "RatingRatio");
			}
		}
	}

	[DataSourceProperty]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public string StateText
	{
		get
		{
			return _stateText;
		}
		set
		{
			if (value != _stateText)
			{
				_stateText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "StateText");
			}
		}
	}

	[DataSourceProperty]
	public string LevelText
	{
		get
		{
			return _levelText;
		}
		set
		{
			if (value != _levelText)
			{
				_levelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LevelText");
			}
		}
	}

	[DataSourceProperty]
	public string LevelTitleText
	{
		get
		{
			return _levelTitleText;
		}
		set
		{
			if (value != _levelTitleText)
			{
				_levelTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LevelTitleText");
			}
		}
	}

	[DataSourceProperty]
	public string RatingText
	{
		get
		{
			return _ratingText;
		}
		set
		{
			if (value != _ratingText)
			{
				_ratingText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RatingText");
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
	public string RatingID
	{
		get
		{
			return _ratingID;
		}
		set
		{
			if (value != _ratingID)
			{
				_ratingID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RatingID");
			}
		}
	}

	[DataSourceProperty]
	public string ClanName
	{
		get
		{
			return _clanName;
		}
		set
		{
			if (value != _clanName)
			{
				_clanName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ClanName");
			}
		}
	}

	[DataSourceProperty]
	public string ClanTag
	{
		get
		{
			return _clanTag;
		}
		set
		{
			if (value != _clanTag)
			{
				_clanTag = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ClanTag");
			}
		}
	}

	[DataSourceProperty]
	public string ChangeText
	{
		get
		{
			return _changeText;
		}
		set
		{
			if (value != _changeText)
			{
				_changeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ChangeText");
			}
		}
	}

	[DataSourceProperty]
	public string ClanInfoTitleText
	{
		get
		{
			return _clanInfoTitleText;
		}
		set
		{
			if (value != _clanInfoTitleText)
			{
				_clanInfoTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ClanInfoTitleText");
			}
		}
	}

	[DataSourceProperty]
	public string BadgeInfoTitleText
	{
		get
		{
			return _badgeInfoTitleText;
		}
		set
		{
			if (value != _badgeInfoTitleText)
			{
				_badgeInfoTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BadgeInfoTitleText");
			}
		}
	}

	[DataSourceProperty]
	public string AvatarInfoTitleText
	{
		get
		{
			return _avatarInfoTitleText;
		}
		set
		{
			if (value != _avatarInfoTitleText)
			{
				_avatarInfoTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AvatarInfoTitleText");
			}
		}
	}

	[DataSourceProperty]
	public string ExperienceText
	{
		get
		{
			return _experienceText;
		}
		set
		{
			if (value != _experienceText)
			{
				_experienceText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ExperienceText");
			}
		}
	}

	[DataSourceProperty]
	public string RankText
	{
		get
		{
			return _rankText;
		}
		set
		{
			if (value != _rankText)
			{
				_rankText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RankText");
			}
		}
	}

	[DataSourceProperty]
	public string BannerlordID
	{
		get
		{
			return _bannerlordID;
		}
		set
		{
			if (value != _bannerlordID)
			{
				_bannerlordID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BannerlordID");
			}
		}
	}

	[DataSourceProperty]
	public string SelectedBadgeID
	{
		get
		{
			return _selectedBadgeID;
		}
		set
		{
			if (value != _selectedBadgeID)
			{
				_selectedBadgeID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SelectedBadgeID");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel NameHint
	{
		get
		{
			return _nameHint;
		}
		set
		{
			if (value != _nameHint)
			{
				_nameHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "NameHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel InviteToPartyHint
	{
		get
		{
			return _inviteToPartyHint;
		}
		set
		{
			if (value != _inviteToPartyHint)
			{
				_inviteToPartyHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "InviteToPartyHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel RemoveFriendHint
	{
		get
		{
			return _removeFriendHint;
		}
		set
		{
			if (value != _removeFriendHint)
			{
				_removeFriendHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "RemoveFriendHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel AcceptFriendRequestHint
	{
		get
		{
			return _acceptFriendRequestHint;
		}
		set
		{
			if (value != _acceptFriendRequestHint)
			{
				_acceptFriendRequestHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "AcceptFriendRequestHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel DeclineFriendRequestHint
	{
		get
		{
			return _declineFriendRequestHint;
		}
		set
		{
			if (value != _declineFriendRequestHint)
			{
				_declineFriendRequestHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "DeclineFriendRequestHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CancelFriendRequestHint
	{
		get
		{
			return _cancelFriendRequestHint;
		}
		set
		{
			if (value != _cancelFriendRequestHint)
			{
				_cancelFriendRequestHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "CancelFriendRequestHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel InviteToClanHint
	{
		get
		{
			return _inviteToClanHint;
		}
		set
		{
			if (value != _inviteToClanHint)
			{
				_inviteToClanHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "InviteToClanHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ChangeBannerlordIDHint
	{
		get
		{
			return _changeBannerlordIDHint;
		}
		set
		{
			if (value != _changeBannerlordIDHint)
			{
				_changeBannerlordIDHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ChangeBannerlordIDHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CopyBannerlordIDHint
	{
		get
		{
			return _copyBannerlordIDHint;
		}
		set
		{
			if (value != _copyBannerlordIDHint)
			{
				_copyBannerlordIDHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "CopyBannerlordIDHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel AddFriendWithBannerlordIDHint
	{
		get
		{
			return _addFriendWithBannerlordIDHint;
		}
		set
		{
			if (value != _addFriendWithBannerlordIDHint)
			{
				_addFriendWithBannerlordIDHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "AddFriendWithBannerlordIDHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ExperienceHint
	{
		get
		{
			return _experienceHint;
		}
		set
		{
			if (value != _experienceHint)
			{
				_experienceHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ExperienceHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel RatingHint
	{
		get
		{
			return _ratingHint;
		}
		set
		{
			if (value != _ratingHint)
			{
				_ratingHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "RatingHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel LootHint
	{
		get
		{
			return _lootHint;
		}
		set
		{
			if (value != _lootHint)
			{
				_lootHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "LootHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel SkirmishRatingHint
	{
		get
		{
			return _skirmishRatingHint;
		}
		set
		{
			if (value != _skirmishRatingHint)
			{
				_skirmishRatingHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "SkirmishRatingHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CaptainRatingHint
	{
		get
		{
			return _captainRatingHint;
		}
		set
		{
			if (value != _captainRatingHint)
			{
				_captainRatingHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "CaptainRatingHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ClanLeaderboardHint
	{
		get
		{
			return _clanLeaderboardHint;
		}
		set
		{
			if (value != _clanLeaderboardHint)
			{
				_clanLeaderboardHint = value;
				((ViewModel)this).OnPropertyChanged("ClanLeaderboardHint");
			}
		}
	}

	[DataSourceProperty]
	public PlayerAvatarImageIdentifierVM Avatar
	{
		get
		{
			return _avatar;
		}
		set
		{
			if (value != _avatar)
			{
				_avatar = value;
				((ViewModel)this).OnPropertyChangedWithValue<PlayerAvatarImageIdentifierVM>(value, "Avatar");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM ClanBanner
	{
		get
		{
			return _clanBanner;
		}
		set
		{
			if (value != _clanBanner)
			{
				_clanBanner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "ClanBanner");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbySigilItemVM Sigil
	{
		get
		{
			return _sigil;
		}
		set
		{
			if (value != _sigil)
			{
				_sigil = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbySigilItemVM>(value, "Sigil");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyBadgeItemVM ShownBadge
	{
		get
		{
			return _shownBadge;
		}
		set
		{
			if (value != _shownBadge)
			{
				_shownBadge = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyBadgeItemVM>(value, "ShownBadge");
			}
		}
	}

	[DataSourceProperty]
	public CharacterViewModel CharacterVisual
	{
		get
		{
			return _characterVisual;
		}
		set
		{
			if (value != _characterVisual)
			{
				_characterVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterViewModel>(value, "CharacterVisual");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyPlayerStatItemVM> DisplayedStats
	{
		get
		{
			return _displayedStats;
		}
		set
		{
			if (value != _displayedStats)
			{
				_displayedStats = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyPlayerStatItemVM>>(value, "DisplayedStats");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyGameTypeVM> GameTypes
	{
		get
		{
			return _gameTypes;
		}
		set
		{
			if (value != _gameTypes)
			{
				_gameTypes = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyGameTypeVM>>(value, "GameTypes");
			}
		}
	}

	public MPLobbyPlayerBaseVM(PlayerId id, string forcedName = "", Action<PlayerId> onInviteToClan = null, Action<PlayerId> onFriendRequestAnswered = null)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		ProvidedID = id;
		_forcedName = forcedName;
		SetOnInvite(null);
		_onInviteToClan = onInviteToClan;
		_onFriendRequestAnswered = onFriendRequestAnswered;
		NameHint = new HintViewModel();
		ExperienceHint = new HintViewModel();
		RatingHint = new HintViewModel();
		LobbyClient gameClient = NetworkMain.GameClient;
		UpdateName(gameClient != null && gameClient.IsKnownPlayer(ProvidedID));
		CanBeInvited = true;
		CanInviteToParty = _onInviteToParty != null;
		CanInviteToClan = _onInviteToClan != null;
		PlatformServices.Instance.CheckPermissionWithUser((Permission)4, id, (PermissionResult)delegate(bool hasBannerlordIDPrivilege)
		{
			CanCopyID = hasBannerlordIDPrivilege;
		});
		IsRankInfoLoading = true;
		GameTypes = new MBBindingList<MPLobbyGameTypeVM>();
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
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		ClanInfoTitleText = ((object)new TextObject("{=j4F7tTzy}Clan", (Dictionary<string, object>)null)).ToString();
		BadgeInfoTitleText = ((object)new TextObject("{=4PrfimcK}Badge", (Dictionary<string, object>)null)).ToString();
		AvatarInfoTitleText = ((object)new TextObject("{=5tbWdY1j}Avatar", (Dictionary<string, object>)null)).ToString();
		ChangeText = ((object)new TextObject("{=Ba50zU7Z}Change", (Dictionary<string, object>)null)).ToString();
		LevelTitleText = ((object)new TextObject("{=OKUTPdaa}Level", (Dictionary<string, object>)null)).ToString();
		GameTypeText = ((object)new TextObject("{=JPimShCw}Game Type", (Dictionary<string, object>)null)).ToString();
		InviteToPartyHint = new HintViewModel(new TextObject("{=aZnS9ECC}Invite", (Dictionary<string, object>)null), (string)null);
		InviteToClanHint = new HintViewModel(new TextObject("{=fLddxLjh}Invite to Clan", (Dictionary<string, object>)null), (string)null);
		RemoveFriendHint = new HintViewModel(new TextObject("{=d7ysGcsN}Remove Friend", (Dictionary<string, object>)null), (string)null);
		AcceptFriendRequestHint = new HintViewModel(new TextObject("{=BSUteZmt}Accept Friend Request", (Dictionary<string, object>)null), (string)null);
		DeclineFriendRequestHint = new HintViewModel(new TextObject("{=942B3LfA}Decline Friend Request", (Dictionary<string, object>)null), (string)null);
		CancelFriendRequestHint = new HintViewModel(new TextObject("{=lGbrWyEe}Cancel Friend Request", (Dictionary<string, object>)null), (string)null);
		LootHint = new HintViewModel(new TextObject("{=Th8q8wC2}Loot", (Dictionary<string, object>)null), (string)null);
		ClanLeaderboardHint = new HintViewModel(new TextObject("{=JdEiK70R}Clan Leaderboard", (Dictionary<string, object>)null), (string)null);
		ChangeBannerlordIDHint = new HintViewModel(new TextObject("{=ozREO8ev}Change Bannerlord ID", (Dictionary<string, object>)null), (string)null);
		AddFriendWithBannerlordIDHint = new HintViewModel(new TextObject("{=tC9C8TLi}Add Friend", (Dictionary<string, object>)null), (string)null);
		CopyBannerlordIDHint = new HintViewModel(new TextObject("{=Pwi1YCjH}Copy Bannerlord ID", (Dictionary<string, object>)null), (string)null);
		DisplayedStats?.ApplyActionOnAllItems((Action<MPLobbyPlayerStatItemVM>)delegate(MPLobbyPlayerStatItemVM s)
		{
			((ViewModel)s).RefreshValues();
		});
		MPLobbyBadgeItemVM shownBadge = ShownBadge;
		if (shownBadge != null)
		{
			((ViewModel)shownBadge).RefreshValues();
		}
	}

	public void RefreshSelectableGameTypes(bool isRankedOnly, Action<string> onRefreshed, string initialGameTypeID = "")
	{
		((Collection<MPLobbyGameTypeVM>)(object)GameTypes).Clear();
		((Collection<MPLobbyGameTypeVM>)(object)GameTypes).Add(new MPLobbyGameTypeVM("Skirmish", isCasual: false, onRefreshed));
		((Collection<MPLobbyGameTypeVM>)(object)GameTypes).Add(new MPLobbyGameTypeVM("Captain", isCasual: false, onRefreshed));
		if (!isRankedOnly)
		{
			((Collection<MPLobbyGameTypeVM>)(object)GameTypes).Add(new MPLobbyGameTypeVM("Duel", isCasual: true, onRefreshed));
			((Collection<MPLobbyGameTypeVM>)(object)GameTypes).Add(new MPLobbyGameTypeVM("TeamDeathmatch", isCasual: true, onRefreshed));
			((Collection<MPLobbyGameTypeVM>)(object)GameTypes).Add(new MPLobbyGameTypeVM("Siege", isCasual: true, UpdateDisplayedRankInfo));
		}
		MPLobbyGameTypeVM mPLobbyGameTypeVM = ((IEnumerable<MPLobbyGameTypeVM>)GameTypes).FirstOrDefault((MPLobbyGameTypeVM gt) => gt.GameTypeID == initialGameTypeID);
		if (mPLobbyGameTypeVM != null)
		{
			mPLobbyGameTypeVM.IsSelected = true;
		}
		else
		{
			((Collection<MPLobbyGameTypeVM>)(object)GameTypes)[0].IsSelected = true;
		}
	}

	private void UpdateForcedAvatarIndex(bool isKnownPlayer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (ProvidedID != NetworkMain.GameClient.PlayerID)
		{
			Game current = Game.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				GameStateManager gameStateManager = current.GameStateManager;
				obj = ((gameStateManager != null) ? gameStateManager.ActiveState : null);
			}
			LobbyState obj2 = obj as LobbyState;
			if (obj2 != null && obj2.HasUserGeneratedContentPrivilege == false)
			{
				_forcedAvatarIndex = AvatarServices.GetForcedAvatarIndexOfPlayer(ProvidedID);
				return;
			}
		}
		if (!BannerlordConfig.EnableGenericAvatars || ProvidedID == NetworkMain.GameClient.PlayerID || isKnownPlayer)
		{
			_forcedAvatarIndex = -1;
		}
		else
		{
			_forcedAvatarIndex = AvatarServices.GetForcedAvatarIndexOfPlayer(ProvidedID);
		}
	}

	protected async void UpdateName(bool isKnownPlayer)
	{
		string genericName = (Name = ((object)_genericPlayerName).ToString());
		PlayerId providedID;
		if (ProvidedID != NetworkMain.GameClient.PlayerID)
		{
			Game current = Game.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				GameStateManager gameStateManager = current.GameStateManager;
				obj = ((gameStateManager != null) ? gameStateManager.ActiveState : null);
			}
			LobbyState obj2 = obj as LobbyState;
			if (obj2 != null && obj2.HasUserGeneratedContentPrivilege == false)
			{
				providedID = ProvidedID;
				if ((int)((PlayerId)(ref providedID)).ProvidedType != 7)
				{
					providedID = ProvidedID;
					if ((int)((PlayerId)(ref providedID)).ProvidedType != 6)
					{
						Name = genericName;
						goto IL_0279;
					}
				}
			}
		}
		if (_forcedName != string.Empty && !BannerlordConfig.EnableGenericNames)
		{
			Name = _forcedName;
		}
		else if (ProvidedID == NetworkMain.GameClient.PlayerID)
		{
			Name = NetworkMain.GameClient.Name;
		}
		else if (!isKnownPlayer && BannerlordConfig.EnableGenericNames)
		{
			Name = genericName;
		}
		else if (PlayerData != null)
		{
			string lastPlayerName = PlayerData.LastPlayerName;
			Name = lastPlayerName;
		}
		else
		{
			providedID = ProvidedID;
			if (((PlayerId)(ref providedID)).IsValid)
			{
				IFriendListService[] friendListServices = PlatformServices.Instance.GetFriendListServices();
				string foundName = genericName;
				for (int i = friendListServices.Length - 1; i >= 0; i--)
				{
					string text2 = await friendListServices[i].GetUserName(ProvidedID);
					if (!string.IsNullOrEmpty(text2) && text2 != "-" && text2 != genericName)
					{
						foundName = text2;
						break;
					}
				}
				Name = foundName;
			}
		}
		goto IL_0279;
		IL_0279:
		NameHint.HintText = new TextObject("{=!}" + Name, (Dictionary<string, object>)null);
	}

	protected void UpdateAvatar(bool isKnownPlayer)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		UpdateForcedAvatarIndex(isKnownPlayer);
		Avatar = new PlayerAvatarImageIdentifierVM(ProvidedID, _forcedAvatarIndex);
	}

	public void UpdatePlayerState(AnotherPlayerData playerData)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (playerData != null)
		{
			if ((int)playerData.PlayerState != 0)
			{
				State = playerData.PlayerState;
				StateText = ((object)GameTexts.FindText("str_multiplayer_lobby_state", ((object)State/*cast due to .constrained prefix*/).ToString())).ToString();
			}
			TimeSinceLastStateUpdate = Game.Current.ApplicationTime;
		}
	}

	public virtual void UpdateWith(PlayerData playerData)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (playerData == null)
		{
			Debug.FailedAssert("PlayerData shouldn't be null at this stage!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Friends\\MPLobbyPlayerBaseVM.cs", "UpdateWith", 280);
			return;
		}
		PlayerData = playerData;
		ProvidedID = PlayerData.PlayerId;
		UpdateNameAndAvatar(forceUpdate: true);
		UpdateExperienceData();
		if (NetworkMain.GameClient != null && NetworkMain.GameClient.SupportedFeatures.SupportsFeatures((Features)8))
		{
			IsClanInfoSupported = true;
		}
		else
		{
			IsClanInfoSupported = false;
		}
		Loot = playerData.Gold;
		Sigil = new MPLobbySigilItemVM();
		Sigil.RefreshWith(playerData.Sigil);
		ShownBadge = new MPLobbyBadgeItemVM(BadgeManager.GetById(playerData.ShownBadgeId), null, (Badge badge) => true, null);
		BannerlordID = $"{playerData.Username}#{playerData.UserId}";
		SelectedBadgeID = playerData.ShownBadgeId;
		StateText = "";
		_hasReceivedPlayerStats = false;
		_isReceivingPlayerStats = false;
	}

	public void UpdateNameAndAvatar(bool forceUpdate = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		bool flag = NetworkMain.GameClient.IsKnownPlayer(ProvidedID);
		if (_isKnownPlayer != flag || forceUpdate)
		{
			_isKnownPlayer = flag;
			UpdateAvatar(flag);
			UpdateName(flag);
		}
	}

	public void OnStatusChanged(OnlineStatus status, bool isInGameStatusActive)
	{
		CurrentOnlineStatus = status;
		StateText = "";
		TimeSinceLastStateUpdate = 0f;
		CanInviteToParty = _onInviteToParty != null && status switch
		{
			OnlineStatus.Online => !isInGameStatusActive, 
			OnlineStatus.InGame => true, 
			_ => false, 
		};
		ShowLevel = status switch
		{
			OnlineStatus.Online => !isInGameStatusActive, 
			OnlineStatus.InGame => true, 
			_ => false, 
		};
		CanInviteToClan = _onInviteToClan != null && status == OnlineStatus.Online;
	}

	public void SetOnInvite(Action<PlayerId> onInvite)
	{
		_onInviteToParty = onInvite;
		CanInviteToParty = onInvite != null;
		((ViewModel)this).RefreshValues();
	}

	public async void UpdateStats(Action onDone)
	{
		if (!_hasReceivedPlayerStats && !_isReceivingPlayerStats)
		{
			_isReceivingPlayerStats = true;
			PlayerStats = await NetworkMain.GameClient.GetPlayerStats(ProvidedID);
			_isReceivingPlayerStats = false;
			_hasReceivedPlayerStats = PlayerStats != null;
			if (_hasReceivedPlayerStats)
			{
				OnPlayerStatsReceived?.Invoke();
				onDone?.Invoke();
			}
		}
	}

	public void UpdateExperienceData()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		Level = PlayerData.Level;
		int num = PlayerDataExperience.ExperienceRequiredForLevel(PlayerData.Level + 1);
		float num2 = (float)PlayerData.ExperienceInCurrentLevel / (float)num;
		ExperienceRatio = (int)(num2 * 100f);
		string text = PlayerData.ExperienceInCurrentLevel + " / " + num;
		ExperienceHint.HintText = new TextObject("{=!}" + text, (Dictionary<string, object>)null);
		TextObject val = new TextObject("{=5Z0pvuNL}Level {LEVEL}", (Dictionary<string, object>)null);
		val.SetTextVariable("LEVEL", Level);
		LevelText = ((object)val).ToString();
		int experienceToNextLevel = PlayerData.ExperienceToNextLevel;
		TextObject val2 = new TextObject("{=NUSH5bJu}{EXPERIENCE} exp to next level", (Dictionary<string, object>)null);
		val2.SetTextVariable("EXPERIENCE", experienceToNextLevel);
		ExperienceText = ((object)val2).ToString();
	}

	public async void UpdateRating(Action onDone)
	{
		IsRankInfoLoading = true;
		RankInfo = await NetworkMain.GameClient.GetGameTypeRankInfo(ProvidedID);
		IsRankInfoLoading = false;
		onDone?.Invoke();
	}

	public void UpdateDisplayedRankInfo(string gameType)
	{
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected O, but got Unknown
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected O, but got Unknown
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		GameTypeRankInfo val = null;
		if (gameType == "Skirmish")
		{
			val = RankInfo?.FirstOrDefault((Func<GameTypeRankInfo, bool>)((GameTypeRankInfo r) => r.GameType == "Skirmish"));
			RankInfoGameTypeID = "Skirmish";
		}
		else if (gameType == "Captain")
		{
			val = RankInfo?.FirstOrDefault((Func<GameTypeRankInfo, bool>)((GameTypeRankInfo r) => r.GameType == "Captain"));
			RankInfoGameTypeID = "Captain";
		}
		if (val != null)
		{
			RankBarInfo rankBarInfo = val.RankBarInfo;
			Rating = rankBarInfo.Rating;
			RatingID = rankBarInfo.RankId;
			RatingText = MPLobbyVM.GetLocalizedRankName(RatingID);
			if (rankBarInfo.IsEvaluating)
			{
				TextObject val2 = new TextObject("{=Ise5gWw3}{PLAYED_GAMES} / {TOTAL_GAMES} Evaluation matches played", (Dictionary<string, object>)null);
				val2.SetTextVariable("PLAYED_GAMES", rankBarInfo.EvaluationMatchesPlayed);
				val2.SetTextVariable("TOTAL_GAMES", rankBarInfo.TotalEvaluationMatchesRequired);
				RankText = ((object)val2).ToString();
				RatingRatio = MathF.Floor((float)rankBarInfo.EvaluationMatchesPlayed / (float)rankBarInfo.TotalEvaluationMatchesRequired * 100f);
			}
			else
			{
				TextObject val3 = new TextObject("{=BUOtUW1u}{RATING} Points", (Dictionary<string, object>)null);
				val3.SetTextVariable("RATING", rankBarInfo.Rating);
				RankText = ((object)val3).ToString();
				RatingRatio = (string.IsNullOrEmpty(rankBarInfo.NextRankId) ? 100 : MathF.Floor(rankBarInfo.ProgressPercentage));
			}
			GameTexts.SetVariable("NUMBER", RatingRatio.ToString("0.00"));
			RatingHint.HintText = GameTexts.FindText("str_NUMBER_percent", (string)null);
		}
		else
		{
			Rating = 0;
			RatingRatio = 0;
			RatingID = "norank";
			RatingText = ((object)new TextObject("{=GXosklej}Casual", (Dictionary<string, object>)null)).ToString();
			RankText = ((object)new TextObject("{=56FyokuX}Game mode is casual", (Dictionary<string, object>)null)).ToString();
			RatingHint.HintText = TextObject.GetEmpty();
		}
		OnRankInfoChanged?.Invoke(gameType);
		IsRankInfoCasual = gameType != "Skirmish" && gameType != "Captain";
	}

	public async void UpdateClanInfo()
	{
		if (!(ProvidedID == PlayerId.Empty))
		{
			bool isSelfPlayer = ProvidedID == NetworkMain.GameClient.PlayerID;
			ClanInfo val = ((!isSelfPlayer) ? (await NetworkMain.GameClient.GetPlayerClanInfo(ProvidedID)) : NetworkMain.GameClient.ClanInfo);
			if (val != null && (isSelfPlayer || (!isSelfPlayer && val.Players.Length != 0)))
			{
				ClanBanner = new BannerImageIdentifierVM(new Banner(val.Sigil), true);
				ClanName = val.Name;
				GameTexts.SetVariable("STR", val.Tag);
				ClanTag = ((object)new TextObject("{=uTXYEAOg}[{STR}]", (Dictionary<string, object>)null)).ToString();
			}
			else
			{
				ClanBanner = new BannerImageIdentifierVM(Banner.CreateOneColoredEmptyBanner(99), false);
				ClanName = ((object)new TextObject("{=0DnHFlia}Not In a Clan", (Dictionary<string, object>)null)).ToString();
				ClanTag = string.Empty;
			}
		}
	}

	public void FilterStatsForGameMode(string gameModeCode)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Expected O, but got Unknown
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Expected O, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Expected O, but got Unknown
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Expected O, but got Unknown
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Expected O, but got Unknown
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Expected O, but got Unknown
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Expected O, but got Unknown
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Expected O, but got Unknown
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Expected O, but got Unknown
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Expected O, but got Unknown
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Expected O, but got Unknown
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Expected O, but got Unknown
		//IL_0591: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Expected O, but got Unknown
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c6: Expected O, but got Unknown
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Expected O, but got Unknown
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Expected O, but got Unknown
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Expected O, but got Unknown
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Expected O, but got Unknown
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Expected O, but got Unknown
		//IL_061a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0629: Expected O, but got Unknown
		//IL_0640: Unknown result type (might be due to invalid IL or missing references)
		//IL_064f: Expected O, but got Unknown
		//IL_0666: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Expected O, but got Unknown
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Expected O, but got Unknown
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Expected O, but got Unknown
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Expected O, but got Unknown
		//IL_06a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b5: Expected O, but got Unknown
		//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06db: Expected O, but got Unknown
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0701: Expected O, but got Unknown
		//IL_0718: Unknown result type (might be due to invalid IL or missing references)
		//IL_0727: Expected O, but got Unknown
		//IL_073e: Unknown result type (might be due to invalid IL or missing references)
		//IL_074d: Expected O, but got Unknown
		//IL_08b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Expected O, but got Unknown
		//IL_08d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e6: Expected O, but got Unknown
		//IL_08fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_090c: Expected O, but got Unknown
		//IL_077e: Unknown result type (might be due to invalid IL or missing references)
		//IL_078d: Expected O, but got Unknown
		//IL_07a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b3: Expected O, but got Unknown
		//IL_07ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Expected O, but got Unknown
		//IL_07f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ff: Expected O, but got Unknown
		//IL_082d: Unknown result type (might be due to invalid IL or missing references)
		//IL_083c: Expected O, but got Unknown
		//IL_0853: Unknown result type (might be due to invalid IL or missing references)
		//IL_0862: Expected O, but got Unknown
		//IL_0879: Unknown result type (might be due to invalid IL or missing references)
		//IL_0888: Expected O, but got Unknown
		if (PlayerStats == null)
		{
			return;
		}
		if (DisplayedStats == null)
		{
			DisplayedStats = new MBBindingList<MPLobbyPlayerStatItemVM>();
		}
		((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Clear();
		IEnumerable<PlayerStatsBase> enumerable = PlayerStats.Where((PlayerStatsBase s) => s.GameType == gameModeCode);
		foreach (PlayerStatsBase item in enumerable)
		{
			if (gameModeCode == "Skirmish" || gameModeCode == "Captain")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=WW2N3zJf}Wins", (Dictionary<string, object>)null), item.WinCount));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=4nr9Km6t}Losses", (Dictionary<string, object>)null), item.LoseCount));
			}
			if (gameModeCode == "Skirmish")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=ab2cbidI}Total Score", (Dictionary<string, object>)null), ((PlayerStatsSkirmish)((item is PlayerStatsSkirmish) ? item : null)).Score));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=fdR3xpBS}MVP Badges", (Dictionary<string, object>)null), ((PlayerStatsSkirmish)((item is PlayerStatsSkirmish) ? item : null)).MVPs));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=2FaZ6E1k}Kill Death Ratio", (Dictionary<string, object>)null), item.AverageKillPerDeath));
			}
			else if (gameModeCode == "Captain")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=ab2cbidI}Total Score", (Dictionary<string, object>)null), ((PlayerStatsCaptain)((item is PlayerStatsCaptain) ? item : null)).Score));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=fdR3xpBS}MVP Badges", (Dictionary<string, object>)null), ((PlayerStatsCaptain)((item is PlayerStatsCaptain) ? item : null)).MVPs));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=9FSk2daF}Captains Killed", (Dictionary<string, object>)null), ((PlayerStatsCaptain)((item is PlayerStatsCaptain) ? item : null)).CaptainsKilled));
			}
			else if (gameModeCode == "Siege")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=ab2cbidI}Total Score", (Dictionary<string, object>)null), ((PlayerStatsSiege)((item is PlayerStatsSiege) ? item : null)).Score));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=XKWGPrYt}Siege Engines Destroyed", (Dictionary<string, object>)null), ((PlayerStatsSiege)((item is PlayerStatsSiege) ? item : null)).SiegeEnginesDestroyed));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=7APa598U}Kills With a Siege Engine", (Dictionary<string, object>)null), ((PlayerStatsSiege)((item is PlayerStatsSiege) ? item : null)).SiegeEngineKills));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=FaKWQccs}Gold Gained From Objectives", (Dictionary<string, object>)null), ((PlayerStatsSiege)((item is PlayerStatsSiege) ? item : null)).ObjectiveGoldGained));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=2FaZ6E1k}Kill Death Ratio", (Dictionary<string, object>)null), item.AverageKillPerDeath));
			}
			else if (gameModeCode == "Duel")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=SS5WyUWR}Duels Won", (Dictionary<string, object>)null), ((PlayerStatsDuel)((item is PlayerStatsDuel) ? item : null)).DuelsWon));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=Iu2eFSsh}Infantry Wins", (Dictionary<string, object>)null), ((PlayerStatsDuel)((item is PlayerStatsDuel) ? item : null)).InfantryWins));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=wyKhcvbd}Ranged Wins", (Dictionary<string, object>)null), ((PlayerStatsDuel)((item is PlayerStatsDuel) ? item : null)).ArcherWins));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=qipBkhys}Cavalry Wins", (Dictionary<string, object>)null), ((PlayerStatsDuel)((item is PlayerStatsDuel) ? item : null)).CavalryWins));
			}
			else if (gameModeCode == "TeamDeathmatch")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=ab2cbidI}Total Score", (Dictionary<string, object>)null), ((PlayerStatsTeamDeathmatch)((item is PlayerStatsTeamDeathmatch) ? item : null)).Score));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=9ET13VOe}Average Score", (Dictionary<string, object>)null), ((PlayerStatsTeamDeathmatch)((item is PlayerStatsTeamDeathmatch) ? item : null)).AverageScore));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=2FaZ6E1k}Kill Death Ratio", (Dictionary<string, object>)null), item.AverageKillPerDeath));
			}
			if (gameModeCode != "Duel")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=FKe05WtJ}Kills", (Dictionary<string, object>)null), item.KillCount));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=8eZFlPVu}Deaths", (Dictionary<string, object>)null), item.DeathCount));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(item.GameType, new TextObject("{=1imGhhZl}Assists", (Dictionary<string, object>)null), item.AssistCount));
			}
		}
		if (Extensions.IsEmpty<PlayerStatsBase>(enumerable))
		{
			if (gameModeCode == "Skirmish" || gameModeCode == "Captain")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=WW2N3zJf}Wins", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=4nr9Km6t}Losses", (Dictionary<string, object>)null), "-"));
			}
			if (gameModeCode == "Skirmish")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=ab2cbidI}Total Score", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=fdR3xpBS}MVP Badges", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=2FaZ6E1k}Kill Death Ratio", (Dictionary<string, object>)null), "-"));
			}
			else if (gameModeCode == "Captain")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=ab2cbidI}Total Score", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=fdR3xpBS}MVP Badges", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=9FSk2daF}Captains Killed", (Dictionary<string, object>)null), "-"));
			}
			else if (gameModeCode == "Siege")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=ab2cbidI}Total Score", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=XKWGPrYt}Siege Engines Destroyed", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=7APa598U}Kills With a Siege Engine", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=FaKWQccs}Gold Gained From Objectives", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=2FaZ6E1k}Kill Death Ratio", (Dictionary<string, object>)null), "-"));
			}
			else if (gameModeCode == "Duel")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=SS5WyUWR}Duels Won", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=Iu2eFSsh}Infantry Wins", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=wyKhcvbd}Ranged Wins", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=qipBkhys}Cavalry Wins", (Dictionary<string, object>)null), "-"));
			}
			else if (gameModeCode == "TeamDeathmatch")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=ab2cbidI}Total Score", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=9ET13VOe}Average Score", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=2FaZ6E1k}Kill Death Ratio", (Dictionary<string, object>)null), "-"));
			}
			if (gameModeCode != "Duel")
			{
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=FKe05WtJ}Kills", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=8eZFlPVu}Deaths", (Dictionary<string, object>)null), "-"));
				((Collection<MPLobbyPlayerStatItemVM>)(object)DisplayedStats).Add(new MPLobbyPlayerStatItemVM(gameModeCode, new TextObject("{=1imGhhZl}Assists", (Dictionary<string, object>)null), "-"));
			}
		}
	}

	public unsafe void RefreshCharacterVisual()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		CharacterVisual = new CharacterViewModel();
		BasicCharacterObject val = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
		val.UpdatePlayerCharacterBodyProperties(PlayerData.BodyProperties, PlayerData.Race, PlayerData.IsFemale);
		CharacterVisual.FillFrom(val, -1, (string)null);
		CharacterViewModel characterVisual = CharacterVisual;
		BodyProperties val2 = PlayerData.BodyProperties;
		DynamicBodyProperties dynamicProperties = ((BodyProperties)(ref val2)).DynamicProperties;
		val2 = val.BodyPropertyRange.BodyPropertyMin;
		val2 = new BodyProperties(dynamicProperties, ((BodyProperties)(ref val2)).StaticProperties);
		characterVisual.BodyProperties = ((object)(*(BodyProperties*)(&val2))/*cast due to .constrained prefix*/).ToString();
		CharacterVisual.IsFemale = PlayerData.IsFemale;
		CharacterVisual.Race = PlayerData.Race;
	}

	public void ExecuteSelectPlayer()
	{
		IsSelected = !IsSelected;
	}

	public void ExecuteInviteToParty()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_onInviteToParty?.Invoke(ProvidedID);
	}

	public void ExecuteInviteToClan()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_onInviteToClan?.Invoke(ProvidedID);
	}

	public void ExecuteKickFromParty()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkMain.GameClient.IsInParty && NetworkMain.GameClient.IsPartyLeader)
		{
			NetworkMain.GameClient.KickPlayerFromParty(ProvidedID);
		}
	}

	public void ExecuteAcceptFriendRequest()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(ProvidedID);
		NetworkMain.GameClient.RespondToFriendRequest(ProvidedID, flag, true, false);
		_onFriendRequestAnswered?.Invoke(ProvidedID);
		if (HasNotification)
		{
			HasNotification = false;
		}
	}

	public void ExecuteDeclineFriendRequest()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(ProvidedID);
		NetworkMain.GameClient.RespondToFriendRequest(ProvidedID, flag, false, false);
		_onFriendRequestAnswered?.Invoke(ProvidedID);
		if (HasNotification)
		{
			HasNotification = false;
		}
	}

	public void ExecuteCancelPendingFriendRequest()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NetworkMain.GameClient.RemoveFriend(ProvidedID);
	}

	public void ExecuteRemoveFriend()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NetworkMain.GameClient.RemoveFriend(ProvidedID);
	}

	public void ExecuteCopyBannerlordID()
	{
		Input.SetClipboardText(BannerlordID);
	}

	private void ExecuteAddFriend()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		string[] array = BannerlordID.Split(new char[1] { '#' });
		string text = array[0];
		if (int.TryParse(array[1], out var result))
		{
			bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(ProvidedID);
			NetworkMain.GameClient.AddFriendByUsernameAndId(text, result, flag);
		}
	}

	public void ExecuteShowProfile()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		OnPlayerProfileRequested?.Invoke(ProvidedID);
	}

	private void ExecuteActivateSigilChangeInformation()
	{
		IsSigilChangeInformationEnabled = true;
	}

	private void ExecuteDeactivateSigilChangeInformation()
	{
		IsSigilChangeInformationEnabled = false;
	}

	private void ExecuteChangeSigil()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		OnSigilChangeRequested?.Invoke(ProvidedID);
	}

	private void ExecuteChangeBannerlordID()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		OnBannerlordIDChangeRequested?.Invoke(ProvidedID);
	}

	private void ExecuteAddFriendWithBannerlordID()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		OnAddFriendWithBannerlordIDRequested?.Invoke(ProvidedID);
	}

	private void ExecuteChangeBadge()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		OnBadgeChangeRequested?.Invoke(ProvidedID);
	}

	private void ExecuteShowRankProgression()
	{
		MPLobbyGameTypeVM? mPLobbyGameTypeVM = ((IEnumerable<MPLobbyGameTypeVM>)GameTypes).FirstOrDefault((MPLobbyGameTypeVM gt) => gt.IsSelected);
		if (mPLobbyGameTypeVM != null && !mPLobbyGameTypeVM.IsCasual)
		{
			OnRankProgressionRequested?.Invoke(this);
		}
	}

	private void ExecuteShowRankLeaderboard()
	{
		MPLobbyGameTypeVM mPLobbyGameTypeVM = ((IEnumerable<MPLobbyGameTypeVM>)GameTypes).FirstOrDefault((MPLobbyGameTypeVM gt) => gt.IsSelected);
		if (mPLobbyGameTypeVM != null && !mPLobbyGameTypeVM.IsCasual)
		{
			OnRankLeaderboardRequested?.Invoke(mPLobbyGameTypeVM.GameTypeID);
		}
	}

	private void ExecuteShowClanPage()
	{
		OnClanPageRequested?.Invoke();
	}

	private void ExecuteShowClanLeaderboard()
	{
		OnClanLeaderboardRequested?.Invoke();
	}
}
