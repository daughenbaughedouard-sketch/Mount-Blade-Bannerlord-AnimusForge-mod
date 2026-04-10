using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

public class MPLobbyFriendGroupVM : ViewModel
{
	private readonly struct FriendOperation
	{
		public enum OperationTypes
		{
			Add,
			Remove,
			Clear
		}

		public readonly MPLobbyFriendItemVM Friend;

		public readonly OperationTypes Type;

		public FriendOperation(OperationTypes type, MPLobbyFriendItemVM friend)
		{
			Type = type;
			Friend = friend;
		}
	}

	public enum FriendGroupType
	{
		InGame,
		Online,
		Offline,
		FriendRequests,
		PendingRequests
	}

	private List<FriendOperation> _friendOperationQueue;

	private string _title;

	private FriendGroupType _groupType;

	private MBBindingList<MPLobbyFriendItemVM> _friendList;

	[DataSourceProperty]
	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (value != _title)
			{
				_title = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Title");
			}
		}
	}

	[DataSourceProperty]
	public FriendGroupType GroupType
	{
		get
		{
			return _groupType;
		}
		set
		{
			if (value != _groupType)
			{
				_groupType = value;
				((ViewModel)this).OnPropertyChanged("GroupType");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyFriendItemVM> FriendList
	{
		get
		{
			return _friendList;
		}
		set
		{
			if (value != _friendList)
			{
				_friendList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyFriendItemVM>>(value, "FriendList");
			}
		}
	}

	public MPLobbyFriendGroupVM(FriendGroupType groupType)
	{
		GroupType = groupType;
		_friendOperationQueue = new List<FriendOperation>();
		FriendList = new MBBindingList<MPLobbyFriendItemVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		switch (GroupType)
		{
		case FriendGroupType.InGame:
			Title = ((object)new TextObject("{=uUoSmCBS}In Bannerlord", (Dictionary<string, object>)null)).ToString();
			break;
		case FriendGroupType.Online:
			Title = ((object)new TextObject("{=V305MaOP}Online", (Dictionary<string, object>)null)).ToString();
			break;
		case FriendGroupType.Offline:
			Title = ((object)new TextObject("{=Zv1lg272}Offline", (Dictionary<string, object>)null)).ToString();
			break;
		case FriendGroupType.FriendRequests:
			Title = ((object)new TextObject("{=K8CGzQYL}Received Requests", (Dictionary<string, object>)null)).ToString();
			break;
		case FriendGroupType.PendingRequests:
			Title = ((object)new TextObject("{=QwbVdMLi}Sent Requests", (Dictionary<string, object>)null)).ToString();
			break;
		}
		FriendList.ApplyActionOnAllItems((Action<MPLobbyFriendItemVM>)delegate(MPLobbyFriendItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public void Tick()
	{
		for (int i = 0; i < _friendOperationQueue.Count; i++)
		{
			HandleFriendOperationAux(_friendOperationQueue[i]);
		}
		_friendOperationQueue.Clear();
	}

	private void HandleFriendOperationAux(FriendOperation operation)
	{
		lock (_friendOperationQueue)
		{
			switch (operation.Type)
			{
			case FriendOperation.OperationTypes.Add:
				((Collection<MPLobbyFriendItemVM>)(object)FriendList).Add(operation.Friend);
				operation.Friend.UpdateNameAndAvatar();
				break;
			case FriendOperation.OperationTypes.Remove:
				((Collection<MPLobbyFriendItemVM>)(object)FriendList).Remove(operation.Friend);
				break;
			case FriendOperation.OperationTypes.Clear:
				((Collection<MPLobbyFriendItemVM>)(object)FriendList).Clear();
				break;
			}
		}
	}

	public void ClearFriends()
	{
		lock (_friendOperationQueue)
		{
			_friendOperationQueue.Add(new FriendOperation(FriendOperation.OperationTypes.Clear, null));
		}
	}

	public void AddFriend(MPLobbyFriendItemVM player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (player.ProvidedID != NetworkMain.GameClient.PlayerID)
		{
			lock (_friendOperationQueue)
			{
				_friendOperationQueue.Add(new FriendOperation(FriendOperation.OperationTypes.Add, player));
			}
		}
	}

	public void RemoveFriend(MPLobbyFriendItemVM player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (player.ProvidedID != NetworkMain.GameClient.PlayerID)
		{
			lock (_friendOperationQueue)
			{
				_friendOperationQueue.Add(new FriendOperation(FriendOperation.OperationTypes.Remove, player));
			}
		}
	}
}
