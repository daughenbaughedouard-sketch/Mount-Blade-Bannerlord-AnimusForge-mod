using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

public class MPLobbyFriendsVM : ViewModel
{
	private const string _inviteFailedSoundEvent = "event:/ui/notification/quest_update";

	private List<LobbyNotification> _activeNotifications;

	private int _activeServiceIndex;

	private bool _isEnabled;

	private bool _isListEnabled = true;

	private bool _isPartyAvailable;

	private bool _isPartyFull;

	private bool _isPlayerActionsActive;

	private bool _isInParty;

	private MPLobbyPartyPlayerVM _player;

	private MBBindingList<MPLobbyPartyPlayerVM> _partyFriends;

	private MBBindingList<StringPairItemWithActionVM> _playerActions;

	private string _titleText;

	private string _inGameText;

	private string _onlineText;

	private string _offlineText;

	private int _totalOnlineFriendCount;

	private int _notificationCount;

	private bool _hasNotification;

	private HintViewModel _friendListHint;

	private MBBindingList<MPLobbyFriendServiceVM> _friendServices;

	private MPLobbyFriendServiceVM _activeService;

	private InputKeyItemVM _toggleInputKey;

	private PlayerId? _partyLeaderId
	{
		get
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			PartyPlayerInLobbyClient? obj = ((IEnumerable<PartyPlayerInLobbyClient>)NetworkMain.GameClient.PlayersInParty).SingleOrDefault((Func<PartyPlayerInLobbyClient, bool>)((PartyPlayerInLobbyClient p) => p.IsPartyLeader));
			if (obj == null)
			{
				return null;
			}
			return obj.PlayerId;
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
				IsEnabledUpdated();
			}
		}
	}

	[DataSourceProperty]
	public bool IsListEnabled
	{
		get
		{
			return _isListEnabled;
		}
		set
		{
			if (value != _isListEnabled)
			{
				_isListEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsListEnabled");
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
	public bool IsPartyAvailable
	{
		get
		{
			return _isPartyAvailable;
		}
		set
		{
			if (value != _isPartyAvailable)
			{
				_isPartyAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPartyAvailable");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPartyFull
	{
		get
		{
			return _isPartyFull;
		}
		set
		{
			if (value != _isPartyFull)
			{
				_isPartyFull = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPartyFull");
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
	public MPLobbyPartyPlayerVM Player
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
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPartyPlayerVM>(value, "Player");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyPartyPlayerVM> PartyFriends
	{
		get
		{
			return _partyFriends;
		}
		set
		{
			if (value != _partyFriends)
			{
				_partyFriends = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyPartyPlayerVM>>(value, "PartyFriends");
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
	public string InGameText
	{
		get
		{
			return _inGameText;
		}
		set
		{
			if (value != _inGameText)
			{
				_inGameText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "InGameText");
			}
		}
	}

	[DataSourceProperty]
	public string OnlineText
	{
		get
		{
			return _onlineText;
		}
		set
		{
			if (value != _onlineText)
			{
				_onlineText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OnlineText");
			}
		}
	}

	[DataSourceProperty]
	public string OfflineText
	{
		get
		{
			return _offlineText;
		}
		set
		{
			if (value != _offlineText)
			{
				_offlineText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OfflineText");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel FriendListHint
	{
		get
		{
			return _friendListHint;
		}
		set
		{
			if (value != _friendListHint)
			{
				_friendListHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "FriendListHint");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyFriendServiceVM> FriendServices
	{
		get
		{
			return _friendServices;
		}
		set
		{
			if (value != _friendServices)
			{
				_friendServices = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyFriendServiceVM>>(value, "FriendServices");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyFriendServiceVM ActiveService
	{
		get
		{
			return _activeService;
		}
		set
		{
			if (value != _activeService)
			{
				_activeService = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyFriendServiceVM>(value, "ActiveService");
			}
		}
	}

	[DataSourceProperty]
	public int TotalOnlineFriendCount
	{
		get
		{
			return _totalOnlineFriendCount;
		}
		set
		{
			if (value != _totalOnlineFriendCount)
			{
				_totalOnlineFriendCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TotalOnlineFriendCount");
			}
		}
	}

	[DataSourceProperty]
	public int NotificationCount
	{
		get
		{
			return _notificationCount;
		}
		set
		{
			if (value != _notificationCount)
			{
				_notificationCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NotificationCount");
				HasNotification = value > 0;
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
	public InputKeyItemVM ToggleInputKey
	{
		get
		{
			return _toggleInputKey;
		}
		set
		{
			if (value != _toggleInputKey)
			{
				_toggleInputKey = value;
				((ViewModel)this).OnPropertyChanged("ToggleInputKey");
			}
		}
	}

	public MPLobbyFriendsVM()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Player = new MPLobbyPartyPlayerVM(NetworkMain.GameClient.PlayerID, ActivatePlayerActions);
		PartyFriends = new MBBindingList<MPLobbyPartyPlayerVM>();
		PlayerActions = new MBBindingList<StringPairItemWithActionVM>();
		FriendServices = new MBBindingList<MPLobbyFriendServiceVM>();
		IFriendListService[] friendListServices = PlatformServices.Instance.GetFriendListServices();
		for (int i = 0; i < friendListServices.Length; i++)
		{
			MPLobbyFriendServiceVM item = new MPLobbyFriendServiceVM(friendListServices[i], OnFriendRequestAnswered, ActivatePlayerActions);
			((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Add(item);
		}
		_activeServiceIndex = 0;
		UpdateActiveService();
		_activeNotifications = new List<LobbyNotification>();
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
		//IL_0070: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=abxndmIh}Social", (Dictionary<string, object>)null)).ToString();
		InGameText = ((object)new TextObject("{=uUoSmCBS}In Bannerlord", (Dictionary<string, object>)null)).ToString();
		OnlineText = ((object)new TextObject("{=V305MaOP}Online", (Dictionary<string, object>)null)).ToString();
		OfflineText = ((object)new TextObject("{=Zv1lg272}Offline", (Dictionary<string, object>)null)).ToString();
		FriendListHint = new HintViewModel(new TextObject("{=tjioq56N}Friend List", (Dictionary<string, object>)null), (string)null);
		PartyFriends.ApplyActionOnAllItems((Action<MPLobbyPartyPlayerVM>)delegate(MPLobbyPartyPlayerVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		PlayerActions.ApplyActionOnAllItems((Action<StringPairItemWithActionVM>)delegate(StringPairItemWithActionVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		FriendServices.ApplyActionOnAllItems((Action<MPLobbyFriendServiceVM>)delegate(MPLobbyFriendServiceVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		foreach (MPLobbyFriendServiceVM item in (Collection<MPLobbyFriendServiceVM>)(object)FriendServices)
		{
			((ViewModel)item).OnFinalize();
		}
		((ViewModel)ToggleInputKey).OnFinalize();
	}

	public void OnStateActivate()
	{
		IsPartyAvailable = NetworkMain.GameClient.PartySystemAvailable;
		GetPartyData();
		foreach (MPLobbyFriendServiceVM item in (Collection<MPLobbyFriendServiceVM>)(object)FriendServices)
		{
			item.OnStateActivate();
		}
	}

	private void IsEnabledUpdated()
	{
		GetPartyData();
	}

	private void GetPartyData()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		((Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends).Clear();
		if (!NetworkMain.GameClient.IsInParty)
		{
			return;
		}
		foreach (PartyPlayerInLobbyClient item in NetworkMain.GameClient.PlayersInParty)
		{
			if (item.WaitingInvitation)
			{
				OnPlayerInvitedToParty(item.PlayerId);
			}
			else
			{
				OnPlayerAddedToParty(item.PlayerId);
			}
		}
	}

	public void OnTick(float dt)
	{
		int num = 0;
		foreach (MPLobbyFriendServiceVM item in (Collection<MPLobbyFriendServiceVM>)(object)FriendServices)
		{
			item.OnTick(dt);
			if (item.FriendListService.IncludeInAllFriends)
			{
				num += ((Collection<MPLobbyFriendItemVM>)(object)item.OnlineFriends.FriendList).Count;
				num += ((Collection<MPLobbyFriendItemVM>)(object)item.InGameFriends.FriendList).Count;
			}
		}
		TotalOnlineFriendCount = num;
		IsInParty = NetworkMain.GameClient.IsInParty;
	}

	public void OnPlayerInvitedToParty(PlayerId playerId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (playerId != NetworkMain.GameClient.PlayerData.PlayerId)
		{
			MPLobbyPartyPlayerVM mPLobbyPartyPlayerVM = new MPLobbyPartyPlayerVM(playerId, ActivatePlayerActions);
			mPLobbyPartyPlayerVM.IsWaitingConfirmation = true;
			((Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends).Add(mPLobbyPartyPlayerVM);
		}
	}

	public void OnPlayerAddedToParty(PlayerId playerId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (playerId != NetworkMain.GameClient.PlayerData.PlayerId)
		{
			MPLobbyPartyPlayerVM mPLobbyPartyPlayerVM = FindPartyFriend(playerId);
			if (mPLobbyPartyPlayerVM == null)
			{
				mPLobbyPartyPlayerVM = new MPLobbyPartyPlayerVM(playerId, ActivatePlayerActions);
				((Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends).Add(mPLobbyPartyPlayerVM);
			}
			else
			{
				mPLobbyPartyPlayerVM.IsWaitingConfirmation = false;
			}
		}
		UpdateCanInviteOtherPlayersToParty();
		UpdatePartyLeader();
	}

	public void OnPlayerRemovedFromParty(PlayerId playerId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (playerId == NetworkMain.GameClient.PlayerData.PlayerId)
		{
			((Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends).Clear();
		}
		else
		{
			int num = -1;
			for (int i = 0; i < ((Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends).Count; i++)
			{
				if (((Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends)[i].ProvidedID == playerId)
				{
					num = i;
					break;
				}
			}
			if (((Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends).Count > 0 && num > -1 && num < ((Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends).Count)
			{
				((Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends).RemoveAt(num);
			}
		}
		UpdateCanInviteOtherPlayersToParty();
		UpdatePartyLeader();
	}

	private MPLobbyPartyPlayerVM FindPartyFriend(PlayerId playerId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		foreach (MPLobbyPartyPlayerVM item in (Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends)
		{
			if (item.ProvidedID == playerId)
			{
				return item;
			}
		}
		return null;
	}

	internal void OnPlayerAssignedPartyLeader()
	{
		UpdatePartyLeader();
	}

	internal void OnClanInfoChanged()
	{
		MPLobbyFriendServiceVM mPLobbyFriendServiceVM = ((IEnumerable<MPLobbyFriendServiceVM>)FriendServices).FirstOrDefault((MPLobbyFriendServiceVM f) => f.FriendListService.GetServiceCodeName() == "ClanFriends");
		if (NetworkMain.GameClient.IsInClan && mPLobbyFriendServiceVM == null)
		{
			IFriendListService[] friendListServices = PlatformServices.Instance.GetFriendListServices();
			IFriendListService val = ((IEnumerable<IFriendListService>)friendListServices).FirstOrDefault((Func<IFriendListService, bool>)((IFriendListService f) => f.GetServiceCodeName() == "ClanFriends"));
			if (val != null)
			{
				MPLobbyFriendServiceVM mPLobbyFriendServiceVM2 = new MPLobbyFriendServiceVM(val, OnFriendRequestAnswered, ActivatePlayerActions);
				((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Insert(friendListServices.Length - 2, mPLobbyFriendServiceVM2);
				mPLobbyFriendServiceVM2.ForceRefresh();
			}
		}
		else if (NetworkMain.GameClient.IsInClan && mPLobbyFriendServiceVM != null)
		{
			mPLobbyFriendServiceVM.ForceRefresh();
		}
		else
		{
			if (NetworkMain.GameClient.IsInClan || mPLobbyFriendServiceVM == null)
			{
				return;
			}
			for (int num = ((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Count - 1; num >= 0; num--)
			{
				IFriendListService friendListService = ((Collection<MPLobbyFriendServiceVM>)(object)FriendServices)[num].FriendListService;
				if (!NetworkMain.GameClient.IsInClan && friendListService.GetServiceCodeName() == "ClanFriends")
				{
					((ViewModel)((Collection<MPLobbyFriendServiceVM>)(object)FriendServices)[num]).OnFinalize();
					((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).RemoveAt(num);
					break;
				}
			}
		}
	}

	private void ActivatePlayerActions(MPLobbyPlayerBaseVM player)
	{
		((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Clear();
		if (player is MPLobbyPartyPlayerVM player2)
		{
			ActivatePartyPlayerActions(player2);
		}
		else if (player is MPLobbyFriendItemVM player3)
		{
			ActivateFriendPlayerActions(player3);
		}
		IsPlayerActionsActive = false;
		IsPlayerActionsActive = ((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Count > 0;
	}

	private void ExecuteSetPlayerAsLeader(object playerObj)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyPlayerBaseVM mPLobbyPlayerBaseVM = playerObj as MPLobbyPlayerBaseVM;
		NetworkMain.GameClient.PromotePlayerToPartyLeader(mPLobbyPlayerBaseVM.ProvidedID);
		UpdatePartyLeader();
	}

	private void ExecuteKickPlayerFromParty(object playerObj)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyPlayerBaseVM mPLobbyPlayerBaseVM = playerObj as MPLobbyPlayerBaseVM;
		if (NetworkMain.GameClient.IsInParty && NetworkMain.GameClient.IsPartyLeader)
		{
			NetworkMain.GameClient.KickPlayerFromParty(mPLobbyPlayerBaseVM.ProvidedID);
		}
		UpdatePartyLeader();
	}

	private void ExecuteLeaveParty(object playerObj)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyPlayerBaseVM mPLobbyPlayerBaseVM = playerObj as MPLobbyPlayerBaseVM;
		if (NetworkMain.GameClient.IsInParty && mPLobbyPlayerBaseVM.ProvidedID == NetworkMain.GameClient.PlayerData.PlayerId)
		{
			NetworkMain.GameClient.KickPlayerFromParty(NetworkMain.GameClient.PlayerData.PlayerId);
		}
	}

	private void ExecuteInviteFriend(PlayerId providedId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		bool dontUseNameForUnknownPlayer = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(providedId);
		PermissionResult val2 = default(PermissionResult);
		PlatformServices.Instance.CheckPrivilege((Privilege)3, true, (PrivilegeResult)delegate(bool result)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Expected O, but got Unknown
			//IL_008c: Expected O, but got Unknown
			if (result)
			{
				PlayerIdProvidedTypes providedType = ((PlayerId)(ref providedId)).ProvidedType;
				LobbyClient gameClient = NetworkMain.GameClient;
				PlayerIdProvidedTypes? obj;
				if (gameClient == null)
				{
					obj = null;
				}
				else
				{
					PlayerId playerID = gameClient.PlayerID;
					obj = ((PlayerId)(ref playerID)).ProvidedType;
				}
				if ((PlayerIdProvidedTypes?)providedType != obj)
				{
					NetworkMain.GameClient.InviteToParty(providedId, dontUseNameForUnknownPlayer);
				}
				else
				{
					IPlatformServices instance = PlatformServices.Instance;
					PlayerId val = providedId;
					PermissionResult obj2 = val2;
					if (obj2 == null)
					{
						PermissionResult val3 = delegate(bool permissionResult)
						{
							//IL_0020: Unknown result type (might be due to invalid IL or missing references)
							//IL_002a: Expected O, but got Unknown
							//IL_0030: Unknown result type (might be due to invalid IL or missing references)
							//IL_003a: Expected O, but got Unknown
							//IL_0049: Unknown result type (might be due to invalid IL or missing references)
							//IL_0053: Expected O, but got Unknown
							//IL_0009: Unknown result type (might be due to invalid IL or missing references)
							//IL_0080: Unknown result type (might be due to invalid IL or missing references)
							//IL_008c: Expected O, but got Unknown
							if (permissionResult)
							{
								NetworkMain.GameClient.InviteToParty(providedId, dontUseNameForUnknownPlayer);
							}
							else
							{
								string? text = ((object)new TextObject("{=ZwN6rzTC}No permission", (Dictionary<string, object>)null)).ToString();
								string text2 = ((object)new TextObject("{=wlz3eQWp}No permission to invite player.", (Dictionary<string, object>)null)).ToString();
								InformationManager.ShowInquiry(new InquiryData(text, text2, false, true, "", ((object)new TextObject("{=dismissnotification}Dismiss", (Dictionary<string, object>)null)).ToString(), (Action)null, (Action)delegate
								{
									InformationManager.HideInquiry();
								}, "event:/ui/notification/quest_update", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
							}
						};
						PermissionResult val4 = val3;
						val2 = val3;
						obj2 = val4;
					}
					instance.CheckPermissionWithUser((Permission)0, val, obj2);
				}
			}
		});
	}

	private void ExecuteRequestFriendship(object playerObj)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyPlayerBaseVM mPLobbyPlayerBaseVM = playerObj as MPLobbyPlayerBaseVM;
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(mPLobbyPlayerBaseVM.ProvidedID);
		NetworkMain.GameClient.AddFriend(mPLobbyPlayerBaseVM.ProvidedID, flag);
	}

	private void ExecuteTerminateFriendship(object memberObj)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyPlayerBaseVM mPLobbyPlayerBaseVM = memberObj as MPLobbyPlayerBaseVM;
		NetworkMain.GameClient.RemoveFriend(mPLobbyPlayerBaseVM.ProvidedID);
	}

	public void UpdateCanInviteOtherPlayersToParty()
	{
		foreach (MPLobbyFriendServiceVM item in (Collection<MPLobbyFriendServiceVM>)(object)FriendServices)
		{
			item.UpdateCanInviteOtherPlayersToParty();
		}
	}

	public void UpdatePartyLeader()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyPartyPlayerVM player = Player;
		int isPartyLeader;
		if (NetworkMain.GameClient.IsInParty)
		{
			PlayerId providedID = Player.ProvidedID;
			PlayerId? partyLeaderId = _partyLeaderId;
			isPartyLeader = ((partyLeaderId.HasValue && providedID == partyLeaderId.GetValueOrDefault()) ? 1 : 0);
		}
		else
		{
			isPartyLeader = 0;
		}
		player.IsPartyLeader = (byte)isPartyLeader != 0;
		foreach (MPLobbyPartyPlayerVM item in (Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends)
		{
			PlayerId providedID = item.ProvidedID;
			PlayerId? partyLeaderId = _partyLeaderId;
			item.IsPartyLeader = partyLeaderId.HasValue && providedID == partyLeaderId.GetValueOrDefault();
		}
	}

	public void OnFriendRequestNotificationsReceived(List<LobbyNotification> notifications)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		foreach (LobbyNotification item in _activeNotifications.Except(notifications).ToList())
		{
			_activeNotifications.Remove(item);
			NotificationCount--;
		}
		foreach (LobbyNotification notification in notifications)
		{
			string notificationPlayerIDString = notification.Parameters["friend_requester"];
			if (!Extensions.IsEmpty<LobbyNotification>(_activeNotifications.Where((LobbyNotification n) => n.Parameters["friend_requester"].Equals(notificationPlayerIDString))))
			{
				continue;
			}
			PlayerId notificationPlayerID = PlayerId.FromString(notificationPlayerIDString);
			PermissionResult val2 = default(PermissionResult);
			PlatformServices.Instance.CheckPrivilege((Privilege)3, false, (PrivilegeResult)delegate(bool privilegeResult)
			{
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				//IL_004e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Unknown result type (might be due to invalid IL or missing references)
				//IL_0055: Expected O, but got Unknown
				//IL_005a: Expected O, but got Unknown
				if (!privilegeResult)
				{
					ProcessNotification(notification, notificationPlayerID, allowed: false);
				}
				else
				{
					IPlatformServices instance = PlatformServices.Instance;
					PlayerId val = notificationPlayerID;
					PermissionResult obj = val2;
					if (obj == null)
					{
						PermissionResult val3 = delegate(bool permissionResult)
						{
							//IL_0021: Unknown result type (might be due to invalid IL or missing references)
							ProcessNotification(notification, notificationPlayerID, permissionResult);
						};
						PermissionResult val4 = val3;
						val2 = val3;
						obj = val4;
					}
					instance.CheckPermissionWithUser((Permission)1, val, obj);
				}
			});
		}
	}

	private void ProcessNotification(LobbyNotification notification, PlayerId notificationPlayerID, bool allowed)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!allowed)
		{
			NetworkMain.GameClient.MarkNotificationAsRead(notification.Id);
		}
		else if (MultiplayerPlayerHelper.IsBlocked(notificationPlayerID))
		{
			bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(notificationPlayerID);
			NetworkMain.GameClient.RespondToFriendRequest(notificationPlayerID, flag, false, true);
			NetworkMain.GameClient.MarkNotificationAsRead(notification.Id);
		}
		else
		{
			_activeNotifications.Add(notification);
			NotificationCount++;
		}
	}

	private unsafe void OnFriendRequestAnswered(PlayerId playerID)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		IEnumerable<LobbyNotification> enumerable = _activeNotifications.Where((LobbyNotification n) => n.Parameters["friend_requester"].Equals(((object)(*(PlayerId*)(&playerID))/*cast due to .constrained prefix*/).ToString()));
		foreach (LobbyNotification item in enumerable)
		{
			NotificationCount--;
			NetworkMain.GameClient.MarkNotificationAsRead(item.Id);
		}
		_activeNotifications = _activeNotifications.Except(enumerable).ToList();
	}

	public MBBindingList<MPLobbyPlayerBaseVM> GetAllFriends()
	{
		MBBindingList<MPLobbyPlayerBaseVM> val = new MBBindingList<MPLobbyPlayerBaseVM>();
		foreach (MPLobbyFriendServiceVM item in (Collection<MPLobbyFriendServiceVM>)(object)FriendServices)
		{
			if (!item.FriendListService.IncludeInAllFriends)
			{
				continue;
			}
			foreach (MPLobbyFriendItemVM item2 in (Collection<MPLobbyFriendItemVM>)(object)item.InGameFriends.FriendList)
			{
				((Collection<MPLobbyPlayerBaseVM>)(object)val).Add((MPLobbyPlayerBaseVM)item2);
			}
			foreach (MPLobbyFriendItemVM item3 in (Collection<MPLobbyFriendItemVM>)(object)item.OnlineFriends.FriendList)
			{
				((Collection<MPLobbyPlayerBaseVM>)(object)val).Add((MPLobbyPlayerBaseVM)item3);
			}
		}
		return val;
	}

	public void OnSupportedFeaturesRefreshed(SupportedFeatures supportedFeatures)
	{
		if (!supportedFeatures.SupportsFeatures((Features)16))
		{
			MPLobbyFriendServiceVM mPLobbyFriendServiceVM = ((IEnumerable<MPLobbyFriendServiceVM>)FriendServices).FirstOrDefault((MPLobbyFriendServiceVM fs) => ((object)fs.FriendListService).GetType() == typeof(BannerlordFriendListService));
			if (mPLobbyFriendServiceVM != null)
			{
				((ViewModel)mPLobbyFriendServiceVM).OnFinalize();
			}
			((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Remove(mPLobbyFriendServiceVM);
		}
	}

	public void OnFriendListUpdated(bool forceUpdate = false)
	{
		foreach (MPLobbyFriendServiceVM item in (Collection<MPLobbyFriendServiceVM>)(object)FriendServices)
		{
			item.OnFriendListUpdated(forceUpdate);
		}
		Player.UpdateNameAndAvatar(forceUpdate);
		foreach (MPLobbyPartyPlayerVM item2 in (Collection<MPLobbyPartyPlayerVM>)(object)PartyFriends)
		{
			item2.UpdateNameAndAvatar(forceUpdate);
		}
	}

	public void SetToggleFriendListKey(HotKey hotkey)
	{
		ToggleInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}

	private void ActivatePartyPlayerActions(MPLobbyPartyPlayerVM player)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected O, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		if (NetworkMain.GameClient.IsPartyLeader && player.ProvidedID != NetworkMain.GameClient.PlayerData.PlayerId)
		{
			PartyPlayerInLobbyClient? obj = ((IEnumerable<PartyPlayerInLobbyClient>)NetworkMain.GameClient.PlayersInParty).SingleOrDefault((Func<PartyPlayerInLobbyClient, bool>)((PartyPlayerInLobbyClient p) => p.PlayerId == player.ProvidedID));
			if (obj != null && !obj.WaitingInvitation && PlatformServices.InvitationServices == null)
			{
				((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteSetPlayerAsLeader, ((object)new TextObject("{=P7moPm3F}Set as party leader", (Dictionary<string, object>)null)).ToString(), "PromoteToPartyLeader", (object)player));
			}
			((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteKickPlayerFromParty, ((object)new TextObject("{=partykick}Kick", (Dictionary<string, object>)null)).ToString(), "Kick", (object)player));
		}
		if (player.ProvidedID == NetworkMain.GameClient.PlayerData.PlayerId)
		{
			if (NetworkMain.GameClient.IsInParty)
			{
				((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteLeaveParty, ((object)new TextObject("{=9w9JsBYP}Leave party", (Dictionary<string, object>)null)).ToString(), "LeaveParty", (object)player));
			}
			return;
		}
		bool flag = false;
		FriendInfo[] friendInfos = NetworkMain.GameClient.FriendInfos;
		for (int num = 0; num < friendInfos.Length; num++)
		{
			if (friendInfos[num].Id == player.ProvidedID)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteRequestFriendship, ((object)new TextObject("{=UwkpJq9N}Add As Friend", (Dictionary<string, object>)null)).ToString(), "RequestFriendship", (object)player));
		}
		else
		{
			((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteTerminateFriendship, ((object)new TextObject("{=2YIVRuRa}Remove From Friends", (Dictionary<string, object>)null)).ToString(), "TerminateFriendship", (object)player));
		}
		MultiplayerPlayerContextMenuHelper.AddLobbyViewProfileOptions(player, PlayerActions);
	}

	private void ActivateFriendPlayerActions(MPLobbyFriendItemVM player)
	{
		MultiplayerPlayerContextMenuHelper.AddLobbyViewProfileOptions(player, PlayerActions);
	}

	private void ExecuteSwitchToNextService()
	{
		if (((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Count == 0)
		{
			Debug.FailedAssert("Friend service list is empty!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Friends\\MPLobbyFriendsVM.cs", "ExecuteSwitchToNextService", 557);
			return;
		}
		_activeServiceIndex++;
		if (_activeServiceIndex >= ((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Count)
		{
			_activeServiceIndex = 0;
		}
		UpdateActiveService();
	}

	private void ExecuteSwitchToPreviousService()
	{
		if (((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Count == 0)
		{
			Debug.FailedAssert("Friend service list is empty!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Friends\\MPLobbyFriendsVM.cs", "ExecuteSwitchToPreviousService", 574);
			return;
		}
		_activeServiceIndex--;
		if (_activeServiceIndex < 0)
		{
			_activeServiceIndex = ((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Count - 1;
		}
		UpdateActiveService();
	}

	private void UpdateActiveService()
	{
		if (_activeServiceIndex < 0 || _activeServiceIndex >= ((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Count)
		{
			Debug.FailedAssert($"Multiplayer service index is invalid: {_activeServiceIndex}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Friends\\MPLobbyFriendsVM.cs", "UpdateActiveService", 591);
			if (((Collection<MPLobbyFriendServiceVM>)(object)FriendServices).Count <= 0)
			{
				Debug.FailedAssert("Cancelling service update request.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Friends\\MPLobbyFriendsVM.cs", "UpdateActiveService", 599);
				return;
			}
			Debug.FailedAssert("Defaulting to first available service: " + ((Collection<MPLobbyFriendServiceVM>)(object)FriendServices)[0].ServiceName, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Friends\\MPLobbyFriendsVM.cs", "UpdateActiveService", 594);
			_activeServiceIndex = 0;
		}
		ActiveService = ((Collection<MPLobbyFriendServiceVM>)(object)FriendServices)[_activeServiceIndex];
	}
}
