using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

public class MPLobbyPartyJoinRequestPopupVM : ViewModel
{
	private PlayerId _viaPlayerId;

	private bool _isEnabled;

	private float _remainingAnswerDuration;

	private float _maxAnswerDuration;

	private string _titleText;

	private string _doYouWantToInviteText;

	private string _playerSuggestedText;

	private MPLobbyPlayerBaseVM _suggestedPlayer;

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
	public string AcceptJoinRequestText
	{
		get
		{
			return _doYouWantToInviteText;
		}
		set
		{
			if (value != _doYouWantToInviteText)
			{
				_doYouWantToInviteText = value;
				((ViewModel)this).OnPropertyChanged("AcceptJoinRequestText");
			}
		}
	}

	[DataSourceProperty]
	public string JoiningPlayerText
	{
		get
		{
			return _playerSuggestedText;
		}
		set
		{
			if (value != _playerSuggestedText)
			{
				_playerSuggestedText = value;
				((ViewModel)this).OnPropertyChanged("JoiningPlayerText");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerBaseVM JoiningPlayer
	{
		get
		{
			return _suggestedPlayer;
		}
		set
		{
			if (value != _suggestedPlayer)
			{
				_suggestedPlayer = value;
				((ViewModel)this).OnPropertyChanged("JoiningPlayer");
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

	public MPLobbyPartyJoinRequestPopupVM()
	{
		((ViewModel)this).RefreshValues();
		MaxAnswerDuration = 30f;
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=re37GzKI}Party Join Request", (Dictionary<string, object>)null)).ToString();
		AcceptJoinRequestText = ((object)new TextObject("{=Ogr2N5bx}Accept request to join to your party?", (Dictionary<string, object>)null)).ToString();
	}

	public void OpenWith(PlayerId joiningPlayer, PlayerId viaPlayerId, string viaPlayerName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		_viaPlayerId = viaPlayerId;
		JoiningPlayer = new MPLobbyPlayerBaseVM(joiningPlayer);
		RemainingAnswerDuration = MaxAnswerDuration;
		if (viaPlayerId == NetworkMain.GameClient.PlayerID)
		{
			TextObject val = new TextObject("{=BcEN71ts}Player wants to join your party.", (Dictionary<string, object>)null);
			JoiningPlayerText = ((object)val).ToString();
		}
		else
		{
			TextObject val = new TextObject("{=q3uBjUyB}Player wants to join your party through your party member <a style=\"Strong\"><b>{PLAYER_NAME}</b></a>.", (Dictionary<string, object>)null);
			GameTexts.SetVariable("PLAYER_NAME", viaPlayerName);
			JoiningPlayerText = ((object)val).ToString();
		}
		IsEnabled = true;
	}

	public void OpenWithNewParty(PlayerId joiningPlayer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		JoiningPlayer = new MPLobbyPlayerBaseVM(joiningPlayer);
		JoiningPlayerText = "";
		RemainingAnswerDuration = MaxAnswerDuration;
		IsEnabled = true;
	}

	public void Close()
	{
		if (IsEnabled)
		{
			ExecuteDeclineJoinRequest();
		}
	}

	public void OnTick(float dt)
	{
		if (IsEnabled)
		{
			RemainingAnswerDuration -= dt;
			if (RemainingAnswerDuration <= 0f)
			{
				ExecuteDeclineJoinRequest();
			}
		}
	}

	private void ExecuteAcceptJoinRequest()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		PlatformServices.Instance.CheckPrivilege((Privilege)3, true, (PrivilegeResult)delegate(bool result)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			if (result)
			{
				PlatformServices.Instance.CheckPermissionWithUser((Permission)0, JoiningPlayer.ProvidedID, (PermissionResult)delegate(bool permissionResult)
				{
					//IL_0025: Unknown result type (might be due to invalid IL or missing references)
					//IL_000e: Unknown result type (might be due to invalid IL or missing references)
					if (permissionResult)
					{
						NetworkMain.GameClient.AcceptPartyJoinRequest(JoiningPlayer.ProvidedID);
					}
					else
					{
						NetworkMain.GameClient.DeclinePartyJoinRequest(JoiningPlayer.ProvidedID, (PartyJoinDeclineReason)1);
					}
					IsEnabled = false;
				});
			}
			else
			{
				NetworkMain.GameClient.DeclinePartyJoinRequest(JoiningPlayer.ProvidedID, (PartyJoinDeclineReason)1);
				IsEnabled = false;
			}
		});
	}

	private void ExecuteDeclineJoinRequest()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		NetworkMain.GameClient.DeclinePartyJoinRequest(JoiningPlayer.ProvidedID, (PartyJoinDeclineReason)2);
		IsEnabled = false;
	}
}
