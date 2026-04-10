using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

public class MPLobbyFriendServiceVM : ViewModel
{
	private class PlayerStateComparer : IComparer<MPLobbyPlayerBaseVM>
	{
		public int Compare(MPLobbyPlayerBaseVM x, MPLobbyPlayerBaseVM y)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			int stateImportanceOrder = GetStateImportanceOrder(x.State);
			int stateImportanceOrder2 = GetStateImportanceOrder(y.State);
			if (stateImportanceOrder != stateImportanceOrder2)
			{
				return stateImportanceOrder.CompareTo(stateImportanceOrder2);
			}
			return x.Name.CompareTo(y.Name);
		}

		private int GetStateImportanceOrder(AnotherPlayerState state)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected I4, but got Unknown
			return (state - 2) switch
			{
				0 => 0, 
				1 => 1, 
				2 => 2, 
				_ => int.MaxValue, 
			};
		}
	}

	private const string _inviteFailedSoundEvent = "event:/ui/notification/quest_update";

	public readonly IFriendListService FriendListService;

	private readonly Action<MPLobbyPlayerBaseVM> _activatePlayerActions;

	private bool _populatingFriends;

	private bool _isInGameFriendsRelevant;

	private const float UpdateInterval = 2f;

	private float _lastUpdateTimePassed;

	private const float StateRequestInterval = 10f;

	private float _lastStateRequestTimePassed;

	private bool _isStateRequestActive;

	private readonly PlayerStateComparer _playerStateComparer;

	private Action<PlayerId> _onFriendRequestAnswered;

	private bool _isPartyAvailable;

	private static Dictionary<PlayerId, long> _friendRequestsInProcess = new Dictionary<PlayerId, long>();

	private const int BlockedFriendRequestTimeout = 10000;

	private bool _isInGameStatusActive;

	private MPLobbyFriendGroupVM _inGameFriends;

	private MPLobbyFriendGroupVM _onlineFriends;

	private MPLobbyFriendGroupVM _offlineFriends;

	private string _inGameText;

	private string _onlineText;

	private string _offlineText;

	private string _serviceName;

	private HintViewModel _serviceNameHint;

	private MPLobbyFriendGroupVM _friendRequests;

	private bool _gotAnyFriendRequests;

	private MPLobbyFriendGroupVM _pendingRequests;

	private bool _gotAnyPendingRequests;

	public IEnumerable<MPLobbyPlayerBaseVM> AllFriends => ((IEnumerable<MPLobbyFriendItemVM>)InGameFriends.FriendList).Union(((IEnumerable<MPLobbyFriendItemVM>)OnlineFriends.FriendList).Union((IEnumerable<MPLobbyFriendItemVM>)OfflineFriends.FriendList));

	[DataSourceProperty]
	public bool IsInGameStatusActive
	{
		get
		{
			return _isInGameStatusActive;
		}
		set
		{
			if (value != _isInGameStatusActive)
			{
				_isInGameStatusActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInGameStatusActive");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyFriendGroupVM InGameFriends
	{
		get
		{
			return _inGameFriends;
		}
		set
		{
			if (value != _inGameFriends)
			{
				_inGameFriends = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyFriendGroupVM>(value, "InGameFriends");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyFriendGroupVM OnlineFriends
	{
		get
		{
			return _onlineFriends;
		}
		set
		{
			if (value != _onlineFriends)
			{
				_onlineFriends = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyFriendGroupVM>(value, "OnlineFriends");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyFriendGroupVM OfflineFriends
	{
		get
		{
			return _offlineFriends;
		}
		set
		{
			if (value != _offlineFriends)
			{
				_offlineFriends = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyFriendGroupVM>(value, "OfflineFriends");
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
	public string ServiceName
	{
		get
		{
			return _serviceName;
		}
		set
		{
			if (value != _serviceName)
			{
				_serviceName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ServiceName");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyFriendGroupVM FriendRequests
	{
		get
		{
			return _friendRequests;
		}
		set
		{
			if (value != _friendRequests)
			{
				_friendRequests = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyFriendGroupVM>(value, "FriendRequests");
			}
		}
	}

	[DataSourceProperty]
	public bool GotAnyFriendRequests
	{
		get
		{
			return _gotAnyFriendRequests;
		}
		set
		{
			if (value != _gotAnyFriendRequests)
			{
				_gotAnyFriendRequests = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "GotAnyFriendRequests");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyFriendGroupVM PendingRequests
	{
		get
		{
			return _pendingRequests;
		}
		set
		{
			if (value != _pendingRequests)
			{
				_pendingRequests = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyFriendGroupVM>(value, "PendingRequests");
			}
		}
	}

	[DataSourceProperty]
	public bool GotAnyPendingRequests
	{
		get
		{
			return _gotAnyPendingRequests;
		}
		set
		{
			if (value != _gotAnyPendingRequests)
			{
				_gotAnyPendingRequests = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "GotAnyPendingRequests");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ServiceNameHint
	{
		get
		{
			return _serviceNameHint;
		}
		set
		{
			if (value != _serviceNameHint)
			{
				_serviceNameHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ServiceNameHint");
			}
		}
	}

	public MPLobbyFriendServiceVM(IFriendListService friendListService, Action<PlayerId> onFriendRequestAnswered, Action<MPLobbyPlayerBaseVM> activatePlayerActions)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		FriendListService = friendListService;
		_onFriendRequestAnswered = onFriendRequestAnswered;
		_activatePlayerActions = activatePlayerActions;
		_playerStateComparer = new PlayerStateComparer();
		InGameFriends = new MPLobbyFriendGroupVM(MPLobbyFriendGroupVM.FriendGroupType.InGame);
		OnlineFriends = new MPLobbyFriendGroupVM(MPLobbyFriendGroupVM.FriendGroupType.Online);
		OfflineFriends = new MPLobbyFriendGroupVM(MPLobbyFriendGroupVM.FriendGroupType.Offline);
		FriendRequests = new MPLobbyFriendGroupVM(MPLobbyFriendGroupVM.FriendGroupType.FriendRequests);
		PendingRequests = new MPLobbyFriendGroupVM(MPLobbyFriendGroupVM.FriendGroupType.PendingRequests);
		FriendListServiceType friendListServiceType = friendListService.GetFriendListServiceType();
		_isInGameFriendsRelevant = (int)friendListServiceType != 0 && (int)friendListServiceType != 1 && (int)friendListServiceType != 2 && (int)friendListServiceType != 9;
		PlatformServices.Instance.OnBlockedUserListUpdated += BlockedUserListChanged;
		FriendListService.OnUserStatusChanged += UserOnlineStatusChanged;
		FriendListService.OnFriendRemoved += FriendRemoved;
		FriendListService.OnFriendListChanged += FriendListChanged;
		ServiceName = friendListService.GetServiceCodeName();
		((ViewModel)this).RefreshValues();
		UpdateCanInviteOtherPlayersToParty();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		InGameText = ((object)new TextObject("{=uUoSmCBS}In Bannerlord", (Dictionary<string, object>)null)).ToString();
		OnlineText = ((object)new TextObject("{=V305MaOP}Online", (Dictionary<string, object>)null)).ToString();
		OfflineText = ((object)new TextObject("{=Zv1lg272}Offline", (Dictionary<string, object>)null)).ToString();
		ServiceNameHint = new HintViewModel(FriendListService.GetServiceLocalizedName(), (string)null);
		((ViewModel)InGameFriends).RefreshValues();
		((ViewModel)OnlineFriends).RefreshValues();
		((ViewModel)OfflineFriends).RefreshValues();
		((ViewModel)FriendRequests).RefreshValues();
		((ViewModel)PendingRequests).RefreshValues();
	}

	public override void OnFinalize()
	{
		PlatformServices.Instance.OnBlockedUserListUpdated -= BlockedUserListChanged;
		FriendListService.OnUserStatusChanged -= UserOnlineStatusChanged;
		FriendListService.OnFriendRemoved -= FriendRemoved;
		FriendListService.OnFriendListChanged -= FriendListChanged;
		((ViewModel)this).OnFinalize();
	}

	public void OnStateActivate()
	{
		_isPartyAvailable = NetworkMain.GameClient.PartySystemAvailable;
		IsInGameStatusActive = FriendListService.InGameStatusFetchable && _isInGameFriendsRelevant;
		GetFriends();
	}

	private async void GetFriends()
	{
		IEnumerable<PlayerId> allFriends = FriendListService.GetAllFriends();
		if (allFriends == null || _populatingFriends)
		{
			return;
		}
		_populatingFriends = true;
		InGameFriends.ClearFriends();
		OnlineFriends.ClearFriends();
		OfflineFriends.ClearFriends();
		foreach (PlayerId item in allFriends)
		{
			await CreateAndAddFriendToList(item);
		}
		_lastStateRequestTimePassed = 11f;
		_populatingFriends = false;
	}

	public void OnTick(float dt)
	{
		UpdateFriendStates(dt);
		_lastUpdateTimePassed += dt;
		if (_lastUpdateTimePassed >= 2f)
		{
			_lastUpdateTimePassed = 0f;
			if (FriendListService.AllowsFriendOperations)
			{
				TickFriendOperations(dt);
			}
		}
		InGameFriends?.Tick();
		OnlineFriends?.Tick();
		OfflineFriends?.Tick();
		FriendRequests?.Tick();
		PendingRequests?.Tick();
	}

	private void TimeoutProcessedFriendRequests()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		PlayerId[] array = _friendRequestsInProcess.Keys.ToArray();
		foreach (PlayerId key in array)
		{
			if (Environment.TickCount - _friendRequestsInProcess[key] > 10000)
			{
				_friendRequestsInProcess.Remove(key);
			}
		}
	}

	private void BlockFriendRequest(PlayerId friendId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(friendId);
		NetworkMain.GameClient.RespondToFriendRequest(friendId, flag, false, true);
	}

	private void ProcessFriendRequest(PlayerId friendId)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		if (_friendRequestsInProcess.ContainsKey(friendId))
		{
			return;
		}
		_friendRequestsInProcess[friendId] = Environment.TickCount;
		PermissionResult val2 = default(PermissionResult);
		PlatformServices.Instance.CheckPrivilege((Privilege)3, false, (PrivilegeResult)delegate(bool privilegeResult)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			//IL_0070: Expected O, but got Unknown
			if (!privilegeResult)
			{
				BlockFriendRequest(friendId);
			}
			else
			{
				PlayerIdProvidedTypes providedType = ((PlayerId)(ref friendId)).ProvidedType;
				PlayerId playerID = NetworkMain.GameClient.PlayerID;
				if (providedType != ((PlayerId)(ref playerID)).ProvidedType)
				{
					AddFriendRequestItem(friendId);
				}
				else
				{
					IPlatformServices instance = PlatformServices.Instance;
					PlayerId val = friendId;
					PermissionResult obj = val2;
					if (obj == null)
					{
						PermissionResult val3 = delegate(bool permissionResult)
						{
							//IL_001c: Unknown result type (might be due to invalid IL or missing references)
							//IL_000a: Unknown result type (might be due to invalid IL or missing references)
							if (!permissionResult)
							{
								BlockFriendRequest(friendId);
							}
							else
							{
								AddFriendRequestItem(friendId);
							}
						};
						PermissionResult val4 = val3;
						val2 = val3;
						obj = val4;
					}
					instance.CheckPermissionWithUser((Permission)1, val, obj);
				}
			}
		});
	}

	private void AddFriendRequestItem(PlayerId playerID)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyFriendItemVM mPLobbyFriendItemVM = new MPLobbyFriendItemVM(playerID, _activatePlayerActions, null, _onFriendRequestAnswered);
		mPLobbyFriendItemVM.IsFriendRequest = true;
		mPLobbyFriendItemVM.CanRemove = false;
		FriendRequests.AddFriend(mPLobbyFriendItemVM);
	}

	private void TickFriendOperations(float dt)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		IEnumerable<PlayerId> receivedRequests = FriendListService.GetReceivedRequests();
		if (receivedRequests != null)
		{
			GotAnyFriendRequests = receivedRequests.Any();
			TimeoutProcessedFriendRequests();
			foreach (PlayerId friendId in receivedRequests)
			{
				if (((IEnumerable<MPLobbyFriendItemVM>)FriendRequests.FriendList).FirstOrDefault((MPLobbyFriendItemVM p) => p.ProvidedID == friendId) == null)
				{
					ProcessFriendRequest(friendId);
				}
			}
			int i;
			for (i = ((Collection<MPLobbyFriendItemVM>)(object)FriendRequests.FriendList).Count - 1; i >= 0; i--)
			{
				PlayerId val = receivedRequests.FirstOrDefault((Func<PlayerId, bool>)((PlayerId p) => p == ((Collection<MPLobbyFriendItemVM>)(object)FriendRequests.FriendList)[i].ProvidedID));
				if (!((PlayerId)(ref val)).IsValid)
				{
					FriendRequests.RemoveFriend(((Collection<MPLobbyFriendItemVM>)(object)FriendRequests.FriendList)[i]);
				}
			}
		}
		else
		{
			GotAnyFriendRequests = false;
		}
		IEnumerable<PlayerId> pendingRequests = FriendListService.GetPendingRequests();
		if (pendingRequests != null)
		{
			GotAnyPendingRequests = pendingRequests.Any();
			foreach (PlayerId friendId2 in pendingRequests)
			{
				if (((IEnumerable<MPLobbyFriendItemVM>)PendingRequests.FriendList).FirstOrDefault((MPLobbyFriendItemVM p) => p.ProvidedID == friendId2) == null)
				{
					MPLobbyFriendItemVM mPLobbyFriendItemVM = new MPLobbyFriendItemVM(friendId2, _activatePlayerActions);
					mPLobbyFriendItemVM.IsPendingRequest = true;
					mPLobbyFriendItemVM.CanRemove = false;
					PendingRequests.AddFriend(mPLobbyFriendItemVM);
				}
			}
			int i2;
			for (i2 = ((Collection<MPLobbyFriendItemVM>)(object)PendingRequests.FriendList).Count - 1; i2 >= 0; i2--)
			{
				PlayerId val2 = pendingRequests.FirstOrDefault((Func<PlayerId, bool>)((PlayerId p) => p == ((Collection<MPLobbyFriendItemVM>)(object)PendingRequests.FriendList)[i2].ProvidedID));
				if (!((PlayerId)(ref val2)).IsValid)
				{
					PendingRequests.RemoveFriend(((Collection<MPLobbyFriendItemVM>)(object)PendingRequests.FriendList)[i2]);
				}
			}
		}
		else
		{
			GotAnyPendingRequests = false;
		}
	}

	private void UpdateFriendStates(float dt)
	{
		if (!NetworkMain.GameClient.AtLobby || _isStateRequestActive)
		{
			return;
		}
		_lastStateRequestTimePassed += dt;
		if (_lastStateRequestTimePassed >= 10f)
		{
			List<PlayerId> list = new List<PlayerId>();
			list.AddRange(((IEnumerable<MPLobbyFriendItemVM>)InGameFriends.FriendList).Select((MPLobbyFriendItemVM p) => p.ProvidedID));
			list.AddRange(((IEnumerable<MPLobbyFriendItemVM>)OnlineFriends.FriendList).Select((MPLobbyFriendItemVM p) => p.ProvidedID));
			_lastStateRequestTimePassed = 0f;
			UpdatePlayerStates(list);
		}
	}

	private async void UpdatePlayerStates(List<PlayerId> players)
	{
		if (players == null || players.Count <= 0)
		{
			return;
		}
		_isStateRequestActive = true;
		List<(PlayerId, AnotherPlayerData)> list = await NetworkMain.GameClient.GetOtherPlayersState(players);
		if (list != null)
		{
			foreach (var (playerId, playerData) in list)
			{
				GetFriendWithID(playerId)?.UpdatePlayerState(playerData);
			}
			InGameFriends.FriendList.Sort((IComparer<MPLobbyFriendItemVM>)_playerStateComparer);
			OnlineFriends.FriendList.Sort((IComparer<MPLobbyFriendItemVM>)_playerStateComparer);
			OfflineFriends.FriendList.Sort((IComparer<MPLobbyFriendItemVM>)_playerStateComparer);
		}
		_isStateRequestActive = false;
		UpdateCanInviteOtherPlayersToParty();
	}

	private void BlockedUserListChanged()
	{
		GetFriends();
	}

	private void FriendListChanged()
	{
		GetFriends();
	}

	public void ForceRefresh()
	{
		GetFriends();
	}

	private async void UserOnlineStatusChanged(PlayerId providedId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		await CreateAndAddFriendToList(providedId);
	}

	private void FriendRemoved(PlayerId providedId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		RemoveFriend(providedId);
	}

	private void RemoveFriend(PlayerId providedId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyFriendItemVM mPLobbyFriendItemVM = ((IEnumerable<MPLobbyFriendItemVM>)InGameFriends.FriendList).ToList().FirstOrDefault((MPLobbyFriendItemVM p) => p.ProvidedID == providedId);
		if (mPLobbyFriendItemVM != null)
		{
			InGameFriends.RemoveFriend(mPLobbyFriendItemVM);
			return;
		}
		mPLobbyFriendItemVM = ((IEnumerable<MPLobbyFriendItemVM>)OnlineFriends.FriendList).ToList().FirstOrDefault((MPLobbyFriendItemVM p) => p.ProvidedID == providedId);
		if (mPLobbyFriendItemVM != null)
		{
			OnlineFriends.RemoveFriend(mPLobbyFriendItemVM);
			return;
		}
		mPLobbyFriendItemVM = ((IEnumerable<MPLobbyFriendItemVM>)OfflineFriends.FriendList).ToList().FirstOrDefault((MPLobbyFriendItemVM p) => p.ProvidedID == providedId);
		if (mPLobbyFriendItemVM != null)
		{
			OfflineFriends.RemoveFriend(mPLobbyFriendItemVM);
		}
	}

	private async Task CreateAndAddFriendToList(PlayerId playerId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (MultiplayerPlayerHelper.IsBlocked(playerId))
		{
			return;
		}
		RemoveFriend(playerId);
		MPLobbyPlayerBaseVM.OnlineStatus onlineStatus = await GetOnlineStatus(playerId);
		MPLobbyFriendItemVM mPLobbyFriendItemVM = new MPLobbyFriendItemVM(playerId, _activatePlayerActions, ExecuteInviteToClan)
		{
			CanRemove = (FriendListService.AllowsFriendOperations && FriendListService.IncludeInAllFriends)
		};
		if (_isInGameFriendsRelevant)
		{
			switch (onlineStatus)
			{
			case MPLobbyPlayerBaseVM.OnlineStatus.InGame:
				InGameFriends.AddFriend(mPLobbyFriendItemVM);
				break;
			case MPLobbyPlayerBaseVM.OnlineStatus.Online:
				OnlineFriends.AddFriend(mPLobbyFriendItemVM);
				break;
			case MPLobbyPlayerBaseVM.OnlineStatus.Offline:
				OfflineFriends.AddFriend(mPLobbyFriendItemVM);
				break;
			}
		}
		else if (onlineStatus == MPLobbyPlayerBaseVM.OnlineStatus.InGame || onlineStatus == MPLobbyPlayerBaseVM.OnlineStatus.Online)
		{
			OnlineFriends.AddFriend(mPLobbyFriendItemVM);
		}
		else
		{
			OfflineFriends.AddFriend(mPLobbyFriendItemVM);
		}
		mPLobbyFriendItemVM.OnStatusChanged(onlineStatus, IsInGameStatusActive);
	}

	private MPLobbyPlayerBaseVM GetFriendWithID(PlayerId playerId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyFriendItemVM mPLobbyFriendItemVM = ((IEnumerable<MPLobbyFriendItemVM>)_onlineFriends.FriendList).ToList().FirstOrDefault((MPLobbyFriendItemVM p) => p.ProvidedID == playerId);
		if (mPLobbyFriendItemVM != null)
		{
			return mPLobbyFriendItemVM;
		}
		MPLobbyFriendItemVM mPLobbyFriendItemVM2 = ((IEnumerable<MPLobbyFriendItemVM>)_inGameFriends.FriendList).ToList().FirstOrDefault((MPLobbyFriendItemVM p) => p.ProvidedID == playerId);
		if (mPLobbyFriendItemVM2 != null)
		{
			return mPLobbyFriendItemVM2;
		}
		MPLobbyFriendItemVM mPLobbyFriendItemVM3 = ((IEnumerable<MPLobbyFriendItemVM>)_offlineFriends.FriendList).ToList().FirstOrDefault((MPLobbyFriendItemVM p) => p.ProvidedID == playerId);
		if (mPLobbyFriendItemVM3 != null)
		{
			return mPLobbyFriendItemVM3;
		}
		return null;
	}

	public void UpdateCanInviteOtherPlayersToParty()
	{
		OfflineFriends.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
		{
			f.SetOnInvite(null);
		});
		PendingRequests.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
		{
			f.SetOnInvite(null);
		});
		FriendRequests.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
		{
			f.SetOnInvite(null);
		});
		OnlineFriends.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			f.SetOnInvite(GetOnInvite(f.ProvidedID, f.CurrentOnlineStatus, f.State));
		});
		InGameFriends.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			f.SetOnInvite(GetOnInvite(f.ProvidedID, f.CurrentOnlineStatus, f.State));
		});
	}

	public void OnFriendListUpdated(bool updateForced = false)
	{
		if (!_populatingFriends)
		{
			InGameFriends.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
			{
				f.UpdateNameAndAvatar(updateForced);
			});
			OnlineFriends.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
			{
				f.UpdateNameAndAvatar(updateForced);
			});
			OfflineFriends.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
			{
				f.UpdateNameAndAvatar(updateForced);
			});
			FriendRequests.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
			{
				f.UpdateNameAndAvatar(updateForced);
			});
			PendingRequests.FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM f)
			{
				f.UpdateNameAndAvatar(updateForced);
			});
		}
	}

	private async Task<MPLobbyPlayerBaseVM.OnlineStatus> GetOnlineStatus(PlayerId playerId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		bool isOnline = await FriendListService.GetUserOnlineStatus(playerId);
		bool flag = false;
		if (IsInGameStatusActive)
		{
			flag = await FriendListService.IsPlayingThisGame(playerId);
		}
		return (!isOnline) ? MPLobbyPlayerBaseVM.OnlineStatus.Offline : (flag ? MPLobbyPlayerBaseVM.OnlineStatus.InGame : MPLobbyPlayerBaseVM.OnlineStatus.Online);
	}

	private Action<PlayerId> GetOnInvite(PlayerId playerId, MPLobbyPlayerBaseVM.OnlineStatus onlineStatus, AnotherPlayerState state)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		Action<PlayerId> result = null;
		if (PlatformServices.Instance.UsePlatformInvitationService(playerId))
		{
			PlayerIdProvidedTypes providedType = ((PlayerId)(ref playerId)).ProvidedType;
			PlayerId playerID = NetworkMain.GameClient.PlayerID;
			if (providedType == ((PlayerId)(ref playerID)).ProvidedType)
			{
				result = (((int)ApplicationPlatform.CurrentPlatform != 8 || (int)state == 2) ? new Action<PlayerId>(ExecuteInviteToPlatformSession) : null);
				goto IL_0065;
			}
		}
		if (onlineStatus == MPLobbyPlayerBaseVM.OnlineStatus.Offline || onlineStatus == MPLobbyPlayerBaseVM.OnlineStatus.None)
		{
			result = null;
		}
		else if ((int)state == 2)
		{
			result = ExecuteInviteToParty;
		}
		goto IL_0065;
		IL_0065:
		return result;
	}

	private void ExecuteInviteToParty(PlayerId providedId)
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

	private void ExecuteInviteToPlatformSession(PlayerId providedId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		MPLobbyPlayerBaseVM friend = GetFriendWithID(providedId);
		friend.CanBeInvited = false;
		PermissionResult val2 = default(PermissionResult);
		PlatformServices.Instance.CheckPrivilege((Privilege)3, true, (PrivilegeResult)delegate(bool result)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			//IL_002d: Expected O, but got Unknown
			if (result)
			{
				IPlatformServices instance = PlatformServices.Instance;
				PlayerId val = providedId;
				PermissionResult obj = val2;
				if (obj == null)
				{
					PermissionResult val3 = async delegate(bool permissionResult)
					{
						if (permissionResult)
						{
							if (PlatformServices.Instance.UsePlatformInvitationService(providedId) && (!NetworkMain.GameClient.IsInParty || NetworkMain.GameClient.IsPartyLeader))
							{
								Debug.Print("UsePlatformInvitationService InviteToPlatformSession", 0, (DebugColor)12, 17592186044416uL);
								await NetworkMain.GameClient.InviteToPlatformSession(providedId);
							}
							else
							{
								bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(providedId);
								NetworkMain.GameClient.InviteToParty(providedId, flag);
							}
						}
						else
						{
							string? text = ((object)new TextObject("{=ZwN6rzTC}No permission", (Dictionary<string, object>)null)).ToString();
							string text2 = ((object)new TextObject("{=wlz3eQWp}No permission to invite player.", (Dictionary<string, object>)null)).ToString();
							InformationManager.ShowInquiry(new InquiryData(text, text2, false, true, "", ((object)new TextObject("{=dismissnotification}Dismiss", (Dictionary<string, object>)null)).ToString(), (Action)null, (Action)delegate
							{
								InformationManager.HideInquiry();
							}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
						}
						friend.CanBeInvited = true;
					};
					PermissionResult val4 = val3;
					val2 = val3;
					obj = val4;
				}
				instance.CheckPermissionWithUser((Permission)0, val, obj);
			}
			else
			{
				friend.CanBeInvited = true;
			}
		});
	}

	private void ExecuteInviteToClan(PlayerId providedId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		PlatformServices.Instance.CheckPrivilege((Privilege)4, true, (PrivilegeResult)delegate(bool result)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if (result)
			{
				bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(providedId);
				NetworkMain.GameClient.InviteToClan(providedId, flag);
			}
		});
	}
}
