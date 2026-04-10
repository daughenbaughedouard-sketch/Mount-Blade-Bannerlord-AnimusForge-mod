using System;
using TaleWorlds.Library;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

public class MPLobbyPartyPlayerVM : MPLobbyPlayerBaseVM
{
	private Action<MPLobbyPartyPlayerVM> _onActivatePlayerActions;

	private bool _isWaitingConfirmation;

	private bool _isPartyLeader;

	[DataSourceProperty]
	public bool IsWaitingConfirmation
	{
		get
		{
			return _isWaitingConfirmation;
		}
		set
		{
			if (value != _isWaitingConfirmation)
			{
				_isWaitingConfirmation = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsWaitingConfirmation");
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

	public MPLobbyPartyPlayerVM(PlayerId id, Action<MPLobbyPartyPlayerVM> onActivatePlayerActions)
		: base(id)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		_onActivatePlayerActions = onActivatePlayerActions;
	}

	private void ExecuteActivatePlayerActions()
	{
		_onActivatePlayerActions(this);
	}
}
