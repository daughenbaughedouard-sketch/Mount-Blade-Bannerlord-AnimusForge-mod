using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

public class MPLobbyPartyPlayerSuggestionPopupVM : ViewModel
{
	public class PlayerPartySuggestionData
	{
		public PlayerId PlayerId { get; private set; }

		public string PlayerName { get; private set; }

		public PlayerId SuggestingPlayerId { get; private set; }

		public string SuggestingPlayerName { get; private set; }

		public PlayerPartySuggestionData(PlayerId playerId, string playerName, PlayerId suggestingPlayerId, string suggestingPlayerName)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			PlayerId = playerId;
			PlayerName = playerName;
			SuggestingPlayerId = suggestingPlayerId;
			SuggestingPlayerName = suggestingPlayerName;
		}
	}

	private PlayerId _suggestedPlayerId;

	private bool _isEnabled;

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
	public string DoYouWantToInviteText
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
				((ViewModel)this).OnPropertyChanged("DoYouWantToInviteText");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerSuggestedText
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
				((ViewModel)this).OnPropertyChanged("PlayerSuggestedText");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerBaseVM SuggestedPlayer
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
				((ViewModel)this).OnPropertyChanged("SuggestedPlayer");
			}
		}
	}

	public MPLobbyPartyPlayerSuggestionPopupVM()
	{
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=q2Y7aHSF}Invite Suggestion", (Dictionary<string, object>)null)).ToString();
		DoYouWantToInviteText = ((object)new TextObject("{=VFqoa6vD}Do you want to invite this player to your party?", (Dictionary<string, object>)null)).ToString();
	}

	public void OpenWith(PlayerPartySuggestionData data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		_suggestedPlayerId = data.PlayerId;
		SuggestedPlayer = new MPLobbyPlayerBaseVM(data.PlayerId);
		TextObject val = new TextObject("{=C7OHivNl}Your friend <a style=\"Strong\"><b>{PLAYER_NAME}</b></a> wants you to invite the player below to your party.", (Dictionary<string, object>)null);
		GameTexts.SetVariable("PLAYER_NAME", data.SuggestingPlayerName);
		PlayerSuggestedText = ((object)val).ToString();
		IsEnabled = true;
	}

	public void Close()
	{
		IsEnabled = false;
	}

	private void ExecuteAcceptSuggestion()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		PlatformServices.Instance.CheckPrivilege((Privilege)3, true, (PrivilegeResult)delegate(bool result)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			if (result)
			{
				PlatformServices.Instance.CheckPermissionWithUser((Permission)0, _suggestedPlayerId, (PermissionResult)async delegate(bool permissionResult)
				{
					if (permissionResult)
					{
						if (PlatformServices.Instance.UsePlatformInvitationService(_suggestedPlayerId))
						{
							await NetworkMain.GameClient.InviteToPlatformSession(_suggestedPlayerId);
						}
						else
						{
							bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(_suggestedPlayerId);
							NetworkMain.GameClient.InviteToParty(_suggestedPlayerId, flag);
						}
					}
					Close();
				});
			}
			else
			{
				Close();
			}
		});
	}

	private void ExecuteDeclineSuggestion()
	{
		Close();
	}
}
