using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPLobbyPartyInvitationPopupVM : ViewModel
{
	private bool _isEnabled;

	private float _remainingAnswerDuration;

	private float _maxAnswerDuration;

	private string _title;

	private string _message;

	private MPLobbyPlayerBaseVM _invitingPlayer;

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
	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			if (value != _message)
			{
				_message = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Message");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerBaseVM InvitingPlayer
	{
		get
		{
			return _invitingPlayer;
		}
		set
		{
			if (value != _invitingPlayer)
			{
				_invitingPlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPlayerBaseVM>(value, "InvitingPlayer");
			}
		}
	}

	[DataSourceProperty]
	public float RemainingAnswerDuration
	{
		get
		{
			return _remainingAnswerDuration;
		}
		set
		{
			if (value != _remainingAnswerDuration)
			{
				_remainingAnswerDuration = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "RemainingAnswerDuration");
			}
		}
	}

	[DataSourceProperty]
	public float MaxAnswerDuration
	{
		get
		{
			return _maxAnswerDuration;
		}
		set
		{
			if (value != _maxAnswerDuration)
			{
				_maxAnswerDuration = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MaxAnswerDuration");
			}
		}
	}

	public MPLobbyPartyInvitationPopupVM()
	{
		((ViewModel)this).RefreshValues();
		MaxAnswerDuration = 60f;
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		Title = ((object)new TextObject("{=QDNcl3DH}Party Invitation", (Dictionary<string, object>)null)).ToString();
		Message = ((object)new TextObject("{=AaAcmalE}You've been invited to join a party by", (Dictionary<string, object>)null)).ToString();
	}

	public void OpenWith(PlayerId invitingPlayerID)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		RemainingAnswerDuration = MaxAnswerDuration;
		InvitingPlayer = new MPLobbyPlayerBaseVM(invitingPlayerID);
		IsEnabled = true;
	}

	public void Close()
	{
		if (IsEnabled)
		{
			ExecuteDecline();
		}
	}

	public void OnTick(float dt)
	{
		if (IsEnabled)
		{
			RemainingAnswerDuration -= dt;
			if (RemainingAnswerDuration <= 0f)
			{
				ExecuteDecline();
			}
		}
	}

	private void ExecuteAccept()
	{
		IsEnabled = false;
		NetworkMain.GameClient.AcceptPartyInvitation();
	}

	private void ExecuteDecline()
	{
		IsEnabled = false;
		NetworkMain.GameClient.DeclinePartyInvitation();
	}
}
