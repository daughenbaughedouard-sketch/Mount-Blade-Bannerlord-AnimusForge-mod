using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPLobbyPlayerProfileVM : ViewModel
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static PrivilegeResult _003C_003E9__8_0;

		internal void _003CSetPlayerID_003Eb__8_0(bool result)
		{
			if (!result)
			{
				PlatformServices.Instance.ShowRestrictedInformation();
			}
		}
	}

	private readonly LobbyState _lobbyState;

	private PlayerId _activePlayerID;

	private PlayerData _activePlayerData;

	private bool _isStatsReceived;

	private bool _isRatingReceived;

	private bool _isEnabled;

	private bool _isDataLoading;

	private string _statsTitleText;

	private string _closeText;

	private MPLobbyPlayerBaseVM _player;

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
	public bool IsDataLoading
	{
		get
		{
			return _isDataLoading;
		}
		set
		{
			if (value != _isDataLoading)
			{
				_isDataLoading = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDataLoading");
			}
		}
	}

	[DataSourceProperty]
	public string StatsTitleText
	{
		get
		{
			return _statsTitleText;
		}
		set
		{
			if (value != _statsTitleText)
			{
				_statsTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "StatsTitleText");
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
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CloseText");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerBaseVM Player
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
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPlayerBaseVM>(value, "Player");
			}
		}
	}

	public MPLobbyPlayerProfileVM(LobbyState lobbyState)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		_lobbyState = lobbyState;
		Player = new MPLobbyPlayerBaseVM(PlayerId.Empty);
		MPLobbyPlayerBaseVM player = Player;
		player.OnRankInfoChanged = (Action<string>)Delegate.Combine(player.OnRankInfoChanged, new Action<string>(OnPlayerRankInfoChanged));
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		CloseText = ((object)GameTexts.FindText("str_close", (string)null)).ToString();
		StatsTitleText = ((object)new TextObject("{=GmU1to3Y}Statistics", (Dictionary<string, object>)null)).ToString();
		MPLobbyPlayerBaseVM player = Player;
		if (player != null)
		{
			((ViewModel)player).RefreshValues();
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		MPLobbyPlayerBaseVM player = Player;
		player.OnRankInfoChanged = (Action<string>)Delegate.Remove(player.OnRankInfoChanged, new Action<string>(OnPlayerRankInfoChanged));
	}

	public async void SetPlayerID(PlayerId playerID)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		IsEnabled = true;
		IsDataLoading = true;
		_activePlayerID = playerID;
		_activePlayerData = await NetworkMain.GameClient.GetAnotherPlayerData(playerID);
		if (_activePlayerData != null)
		{
			IPlatformServices instance = PlatformServices.Instance;
			if (instance != null)
			{
				object obj = _003C_003Ec._003C_003E9__8_0;
				if (obj == null)
				{
					PrivilegeResult val = delegate(bool result)
					{
						if (!result)
						{
							PlatformServices.Instance.ShowRestrictedInformation();
						}
					};
					_003C_003Ec._003C_003E9__8_0 = val;
					obj = (object)val;
				}
				instance.CheckPrivilege((Privilege)1, true, (PrivilegeResult)obj);
			}
			PlatformServices.Instance.CheckPermissionWithUser((Permission)4, _activePlayerID, (PermissionResult)delegate(bool hasPermission)
			{
				Player.IsBannerlordIDSupported = hasPermission;
			});
			await _lobbyState.UpdateHasUserGeneratedContentPrivilege(showResolveUI: true);
			Player.UpdateWith(_activePlayerData);
			if (NetworkMain.GameClient.SupportedFeatures.SupportsFeatures((Features)8))
			{
				Player.UpdateClanInfo();
			}
			Player.RefreshCharacterVisual();
			Player.UpdateStats(OnStatsReceived);
			Player.UpdateRating(OnRatingReceived);
		}
		else
		{
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=bhQiSzOU}Profile is not available", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=goQ0MZhr}This player does not have an active Bannerlord player profile.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)ExecuteClosePopup, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
	}

	public void OpenWith(PlayerId playerID)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		PlatformServices.Instance.CheckPermissionWithUser((Permission)4, playerID, (PermissionResult)delegate(bool hasBannerlordIDPrivilege)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			Player.IsBannerlordIDSupported = hasBannerlordIDPrivilege;
			SetPlayerID(playerID);
		});
	}

	public void UpdatePlayerData(PlayerData playerData, bool updateStatistics = false, bool updateRating = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		IsDataLoading = true;
		_activePlayerID = playerData.PlayerId;
		Player?.UpdateWith(playerData);
		if (updateStatistics)
		{
			Player.UpdateStats(OnStatsReceived);
		}
		if (updateRating)
		{
			Player.UpdateRating(OnRatingReceived);
		}
		Player.UpdateExperienceData();
		Player.RefreshSelectableGameTypes(isRankedOnly: false, Player.UpdateDisplayedRankInfo);
		IsDataLoading = false;
	}

	private void OnPlayerRankInfoChanged(string gameType)
	{
		Player.FilterStatsForGameMode(gameType);
	}

	public void ExecuteClosePopup()
	{
		IsEnabled = false;
		IsDataLoading = false;
	}

	public void OnClanInfoChanged()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (Player.ProvidedID == NetworkMain.GameClient.PlayerID)
		{
			Player.UpdateClanInfo();
		}
	}

	private void OnStatsReceived()
	{
		_isStatsReceived = true;
		CheckAndUpdateStatsAndRatingData();
	}

	private void OnRatingReceived()
	{
		_isRatingReceived = true;
		CheckAndUpdateStatsAndRatingData();
	}

	public void OnPlayerNameUpdated(string playerName)
	{
		Player?.UpdateNameAndAvatar(forceUpdate: true);
	}

	private void CheckAndUpdateStatsAndRatingData()
	{
		if (_isRatingReceived && _isStatsReceived)
		{
			Player.UpdateExperienceData();
			Player.RefreshSelectableGameTypes(isRankedOnly: false, Player.UpdateDisplayedRankInfo);
			IsDataLoading = false;
		}
	}
}
