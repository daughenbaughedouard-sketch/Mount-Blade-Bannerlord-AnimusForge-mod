using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanInviteFriendsPopupVM : ViewModel
{
	private Func<MBBindingList<MPLobbyPlayerBaseVM>> _getAllFriends;

	private bool _isEnabled;

	private string _titleText;

	private string _inviteText;

	private string _closeText;

	private string _selectPlayersText;

	private MBBindingList<MPLobbyPlayerBaseVM> _onlineFriends;

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
				((ViewModel)this).OnPropertyChanged("IsEnabled");
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
				((ViewModel)this).OnPropertyChanged("TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string InviteText
	{
		get
		{
			return _inviteText;
		}
		set
		{
			if (value != _inviteText)
			{
				_inviteText = value;
				((ViewModel)this).OnPropertyChanged("InviteText");
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
				((ViewModel)this).OnPropertyChanged("CloseText");
			}
		}
	}

	[DataSourceProperty]
	public string SelectPlayersText
	{
		get
		{
			return _selectPlayersText;
		}
		set
		{
			if (value != _selectPlayersText)
			{
				_selectPlayersText = value;
				((ViewModel)this).OnPropertyChanged("SelectPlayersText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyPlayerBaseVM> OnlineFriends
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
				((ViewModel)this).OnPropertyChanged("OnlineFriends");
			}
		}
	}

	public MPLobbyClanInviteFriendsPopupVM(Func<MBBindingList<MPLobbyPlayerBaseVM>> getAllFriends)
	{
		_getAllFriends = getAllFriends;
		OnlineFriends = new MBBindingList<MPLobbyPlayerBaseVM>();
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
		TitleText = ((object)new TextObject("{=v4hVLpap}Invite Players to Clan", (Dictionary<string, object>)null)).ToString();
		InviteText = ((object)new TextObject("{=aZnS9ECC}Invite", (Dictionary<string, object>)null)).ToString();
		CloseText = ((object)new TextObject("{=yQtzabbe}Close", (Dictionary<string, object>)null)).ToString();
		SelectPlayersText = ((object)new TextObject("{=ZAejS7WF}Select players to invite to your clan", (Dictionary<string, object>)null)).ToString();
	}

	public void Open()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkMain.GameClient.ClanID == Guid.Empty || NetworkMain.GameClient.ClanInfo == null)
		{
			return;
		}
		IEnumerable<PlayerId> source = NetworkMain.GameClient.ClanInfo.Players.Select((ClanPlayer c) => c.PlayerId);
		((Collection<MPLobbyPlayerBaseVM>)(object)OnlineFriends).Clear();
		foreach (MPLobbyPlayerBaseVM onlineFriend in (Collection<MPLobbyPlayerBaseVM>)(object)_getAllFriends())
		{
			if (!source.Contains(onlineFriend.ProvidedID) && !((IEnumerable<MPLobbyPlayerBaseVM>)OnlineFriends).Any((MPLobbyPlayerBaseVM f) => f.ProvidedID == onlineFriend.ProvidedID))
			{
				((Collection<MPLobbyPlayerBaseVM>)(object)OnlineFriends).Add(onlineFriend);
			}
		}
		IsEnabled = true;
	}

	private void ExecuteSendInvitation()
	{
		foreach (MPLobbyPlayerBaseVM item in (Collection<MPLobbyPlayerBaseVM>)(object)OnlineFriends)
		{
			if (item.IsSelected)
			{
				item.ExecuteInviteToClan();
			}
		}
		ExecuteClosePopup();
	}

	private void ResetSelection()
	{
		foreach (MPLobbyPlayerBaseVM item in (Collection<MPLobbyPlayerBaseVM>)(object)OnlineFriends)
		{
			item.IsSelected = false;
		}
	}

	public void ExecuteClosePopup()
	{
		if (IsEnabled)
		{
			ResetSelection();
			IsEnabled = false;
		}
	}
}
