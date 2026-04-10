using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyRecentGamesVM : ViewModel
{
	private MatchHistoryData _currentMatchOfTheActivePlayer;

	private bool _isEnabled;

	private bool _gotItems;

	private bool _isPlayerActionsActive;

	private string _recentGamesText;

	private string _noRecentGamesFoundText;

	private string _closeText;

	private MBBindingList<StringPairItemWithActionVM> _playerActions;

	private MBBindingList<MPLobbyRecentGameItemVM> _games;

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
	public bool GotItems
	{
		get
		{
			return _gotItems;
		}
		set
		{
			if (value != _gotItems)
			{
				_gotItems = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "GotItems");
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
	public string RecentGamesText
	{
		get
		{
			return _recentGamesText;
		}
		set
		{
			if (value != _recentGamesText)
			{
				_recentGamesText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RecentGamesText");
			}
		}
	}

	[DataSourceProperty]
	public string NoRecentGamesFoundText
	{
		get
		{
			return _noRecentGamesFoundText;
		}
		set
		{
			if (value != _noRecentGamesFoundText)
			{
				_noRecentGamesFoundText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NoRecentGamesFoundText");
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
	public MBBindingList<MPLobbyRecentGameItemVM> Games
	{
		get
		{
			return _games;
		}
		set
		{
			if (value != _games)
			{
				_games = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyRecentGameItemVM>>(value, "Games");
			}
		}
	}

	public MPLobbyRecentGamesVM()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		_games = new MBBindingList<MPLobbyRecentGameItemVM>();
		PlayerActions = new MBBindingList<StringPairItemWithActionVM>();
		NoRecentGamesFoundText = ((object)new TextObject("{=TzYWE9tA}No Recent Games Found", (Dictionary<string, object>)null)).ToString();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		RecentGamesText = ((object)new TextObject("{=NJolh9ye}Recent Games", (Dictionary<string, object>)null)).ToString();
		CloseText = ((object)GameTexts.FindText("str_close", (string)null)).ToString();
		Games.ApplyActionOnAllItems((Action<MPLobbyRecentGameItemVM>)delegate(MPLobbyRecentGameItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public void RefreshData(MBReadOnlyList<MatchHistoryData> matches)
	{
		((Collection<MPLobbyRecentGameItemVM>)(object)Games).Clear();
		if (matches != null)
		{
			foreach (MatchHistoryData item in ((IEnumerable<MatchHistoryData>)matches).OrderByDescending((MatchHistoryData m) => m.MatchDate))
			{
				if (item != null)
				{
					MPLobbyRecentGameItemVM mPLobbyRecentGameItemVM = new MPLobbyRecentGameItemVM(ActivatePlayerActions);
					mPLobbyRecentGameItemVM.FillFrom(item);
					((Collection<MPLobbyRecentGameItemVM>)(object)Games).Add(mPLobbyRecentGameItemVM);
				}
			}
		}
		GotItems = ((List<MatchHistoryData>)(object)matches).Count > 0;
	}

	public void ActivatePlayerActions(MPLobbyRecentGamePlayerItemVM playerVM)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Clear();
		_currentMatchOfTheActivePlayer = playerVM.MatchOfThePlayer;
		if (playerVM.ProvidedID != NetworkMain.GameClient.PlayerID)
		{
			StringPairItemWithActionVM val = new StringPairItemWithActionVM((Action<object>)ExecuteReport, ((object)GameTexts.FindText("str_mp_scoreboard_context_report", (string)null)).ToString(), "Report", (object)playerVM);
			if (MultiplayerReportPlayerManager.IsPlayerReportedOverLimit(playerVM.ProvidedID))
			{
				val.IsEnabled = false;
				val.Hint.HintText = new TextObject("{=klkYFik9}You've already reported this player.", (Dictionary<string, object>)null);
			}
			((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(val);
			bool flag = false;
			FriendInfo[] friendInfos = NetworkMain.GameClient.FriendInfos;
			for (int i = 0; i < friendInfos.Length; i++)
			{
				if (friendInfos[i].Id == playerVM.ProvidedID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteRequestFriendship, ((object)new TextObject("{=UwkpJq9N}Add As Friend", (Dictionary<string, object>)null)).ToString(), "RequestFriendship", (object)playerVM));
			}
			else
			{
				((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteTerminateFriendship, ((object)new TextObject("{=2YIVRuRa}Remove From Friends", (Dictionary<string, object>)null)).ToString(), "TerminateFriendship", (object)playerVM));
			}
			MultiplayerPlayerContextMenuHelper.AddLobbyViewProfileOptions(playerVM, PlayerActions);
		}
		IsPlayerActionsActive = false;
		IsPlayerActionsActive = ((Collection<StringPairItemWithActionVM>)(object)PlayerActions).Count > 0;
	}

	private void ExecuteRequestFriendship(object playerObj)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyRecentGamePlayerItemVM mPLobbyRecentGamePlayerItemVM = playerObj as MPLobbyRecentGamePlayerItemVM;
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(mPLobbyRecentGamePlayerItemVM.ProvidedID);
		NetworkMain.GameClient.AddFriend(mPLobbyRecentGamePlayerItemVM.ProvidedID, flag);
	}

	private void ExecuteTerminateFriendship(object memberObj)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyRecentGamePlayerItemVM mPLobbyRecentGamePlayerItemVM = memberObj as MPLobbyRecentGamePlayerItemVM;
		NetworkMain.GameClient.RemoveFriend(mPLobbyRecentGamePlayerItemVM.ProvidedID);
	}

	private void ExecuteReport(object playerObj)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyRecentGamePlayerItemVM mPLobbyRecentGamePlayerItemVM = playerObj as MPLobbyRecentGamePlayerItemVM;
		MultiplayerReportPlayerManager.RequestReportPlayer(_currentMatchOfTheActivePlayer.MatchId, mPLobbyRecentGamePlayerItemVM.ProvidedID, mPLobbyRecentGamePlayerItemVM.Name, isRequestedFromMission: false);
	}

	public void ExecuteOpenPopup()
	{
		IsEnabled = true;
	}

	public void ExecuteClosePopup()
	{
		IsEnabled = false;
	}

	public void OnFriendListUpdated(bool forceUpdate = false)
	{
		foreach (MPLobbyRecentGameItemVM item in (Collection<MPLobbyRecentGameItemVM>)(object)Games)
		{
			item.OnFriendListUpdated(forceUpdate);
		}
	}
}
