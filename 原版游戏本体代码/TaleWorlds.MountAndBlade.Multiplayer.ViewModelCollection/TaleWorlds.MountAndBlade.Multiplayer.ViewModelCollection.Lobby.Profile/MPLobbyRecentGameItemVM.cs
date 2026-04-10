using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyRecentGameItemVM : ViewModel
{
	private readonly Action<MPLobbyRecentGamePlayerItemVM> _onActivatePlayerActions;

	public MBBindingList<MPLobbyRecentGamePlayerItemVM> _playersA;

	public MBBindingList<MPLobbyRecentGamePlayerItemVM> _playersB;

	private string _lastSeenPlayersText;

	private string _factionNameA;

	private string _factionNameB;

	private string _cultureA;

	private string _cultureB;

	private string _scoreA;

	private string _scoreB;

	private string _gameMode;

	private string _date;

	private string _seperator;

	private int _playerResultIndex;

	private int _matchResultIndex;

	private HintViewModel _abandonedHint;

	private HintViewModel _wonHint;

	private HintViewModel _lostHint;

	public MatchHistoryData MatchInfo { get; private set; }

	[DataSourceProperty]
	public string LastSeenPlayersText
	{
		get
		{
			return _lastSeenPlayersText;
		}
		set
		{
			if (value != _lastSeenPlayersText)
			{
				_lastSeenPlayersText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LastSeenPlayersText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyRecentGamePlayerItemVM> PlayersA
	{
		get
		{
			return _playersA;
		}
		set
		{
			if (value != _playersA)
			{
				_playersA = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyRecentGamePlayerItemVM>>(value, "PlayersA");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyRecentGamePlayerItemVM> PlayersB
	{
		get
		{
			return _playersB;
		}
		set
		{
			if (value != _playersB)
			{
				_playersB = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyRecentGamePlayerItemVM>>(value, "PlayersB");
			}
		}
	}

	[DataSourceProperty]
	public string CultureA
	{
		get
		{
			return _cultureA;
		}
		set
		{
			if (value != _cultureA)
			{
				_cultureA = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureA");
			}
		}
	}

	[DataSourceProperty]
	public string CultureB
	{
		get
		{
			return _cultureB;
		}
		set
		{
			if (value != _cultureB)
			{
				_cultureB = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureB");
			}
		}
	}

	[DataSourceProperty]
	public string FactionNameA
	{
		get
		{
			return _factionNameA;
		}
		set
		{
			if (value != _factionNameA)
			{
				_factionNameA = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FactionNameA");
			}
		}
	}

	[DataSourceProperty]
	public string FactionNameB
	{
		get
		{
			return _factionNameB;
		}
		set
		{
			if (value != _factionNameB)
			{
				_factionNameB = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FactionNameB");
			}
		}
	}

	[DataSourceProperty]
	public string ScoreA
	{
		get
		{
			return _scoreA;
		}
		set
		{
			if (value != _scoreA)
			{
				_scoreA = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ScoreA");
			}
		}
	}

	[DataSourceProperty]
	public string ScoreB
	{
		get
		{
			return _scoreB;
		}
		set
		{
			if (value != _scoreB)
			{
				_scoreB = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ScoreB");
			}
		}
	}

	[DataSourceProperty]
	public string GameMode
	{
		get
		{
			return _gameMode;
		}
		set
		{
			if (value != _gameMode)
			{
				_gameMode = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameMode");
			}
		}
	}

	[DataSourceProperty]
	public string Date
	{
		get
		{
			return _date;
		}
		set
		{
			if (value != _date)
			{
				_date = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Date");
			}
		}
	}

	[DataSourceProperty]
	public string Seperator
	{
		get
		{
			return _seperator;
		}
		set
		{
			if (value != _seperator)
			{
				_seperator = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Seperator");
			}
		}
	}

	[DataSourceProperty]
	public int MatchResultIndex
	{
		get
		{
			return _matchResultIndex;
		}
		set
		{
			if (value != _matchResultIndex)
			{
				_matchResultIndex = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MatchResultIndex");
			}
		}
	}

	[DataSourceProperty]
	public int PlayerResultIndex
	{
		get
		{
			return _playerResultIndex;
		}
		set
		{
			if (value != _playerResultIndex)
			{
				_playerResultIndex = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PlayerResultIndex");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel AbandonedHint
	{
		get
		{
			return _abandonedHint;
		}
		set
		{
			if (value != _abandonedHint)
			{
				_abandonedHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "AbandonedHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel WonHint
	{
		get
		{
			return _wonHint;
		}
		set
		{
			if (value != _wonHint)
			{
				_wonHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "WonHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel LostHint
	{
		get
		{
			return _lostHint;
		}
		set
		{
			if (value != _lostHint)
			{
				_lostHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "LostHint");
			}
		}
	}

	public MPLobbyRecentGameItemVM(Action<MPLobbyRecentGamePlayerItemVM> onActivatePlayerActions)
	{
		_onActivatePlayerActions = onActivatePlayerActions;
		PlayersA = new MBBindingList<MPLobbyRecentGamePlayerItemVM>();
		PlayersB = new MBBindingList<MPLobbyRecentGamePlayerItemVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		LastSeenPlayersText = ((object)new TextObject("{=NJolh9ye}Recent Games", (Dictionary<string, object>)null)).ToString();
		Seperator = ((object)new TextObject("{=4NaOKslb}-", (Dictionary<string, object>)null)).ToString();
		PlayersA.ApplyActionOnAllItems((Action<MPLobbyRecentGamePlayerItemVM>)delegate(MPLobbyRecentGamePlayerItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		PlayersB.ApplyActionOnAllItems((Action<MPLobbyRecentGamePlayerItemVM>)delegate(MPLobbyRecentGamePlayerItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		AbandonedHint = new HintViewModel(new TextObject("{=eQPSEUml}Abandoned", (Dictionary<string, object>)null), (string)null);
		WonHint = new HintViewModel(new TextObject("{=IS4SifJG}Won", (Dictionary<string, object>)null), (string)null);
		LostHint = new HintViewModel(new TextObject("{=b2aqL7T2}Lost", (Dictionary<string, object>)null), (string)null);
		if (MatchInfo != null)
		{
			FillFrom(MatchInfo);
		}
	}

	public void FillFrom(MatchHistoryData match)
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		MatchInfo = match;
		((Collection<MPLobbyRecentGamePlayerItemVM>)(object)PlayersA).Clear();
		((Collection<MPLobbyRecentGamePlayerItemVM>)(object)PlayersB).Clear();
		GameMode = ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", match.GameType)).ToString();
		PlayerResultIndex = ((match.WinnerTeam != -1) ? 1 : 0);
		CultureA = match.Faction1;
		FactionNameA = GetLocalizedCultureNameFromStringID(match.Faction1);
		ScoreA = match.AttackerScore.ToString();
		CultureB = match.Faction2;
		FactionNameB = GetLocalizedCultureNameFromStringID(match.Faction2);
		ScoreB = match.DefenderScore.ToString();
		MatchResultIndex = ((match.DefenderScore != match.AttackerScore) ? ((match.DefenderScore <= match.AttackerScore) ? 1 : 2) : 0);
		Date = LocalizedTextManager.GetDateFormattedByLanguage(BannerlordConfig.Language, match.MatchDate);
		foreach (PlayerInfo player in match.Players)
		{
			PlayerId val = PlayerId.FromString(player.PlayerId);
			if (!MultiplayerPlayerHelper.IsBlocked(val))
			{
				MPLobbyRecentGamePlayerItemVM item = new MPLobbyRecentGamePlayerItemVM(val, match, _onActivatePlayerActions);
				if (match.WinnerTeam != -1 && val == NetworkMain.GameClient.PlayerID)
				{
					PlayerResultIndex = ((player.TeamNo == match.WinnerTeam) ? 1 : 2);
				}
				if (player.TeamNo == 1)
				{
					((Collection<MPLobbyRecentGamePlayerItemVM>)(object)PlayersA).Add(item);
				}
				else
				{
					((Collection<MPLobbyRecentGamePlayerItemVM>)(object)PlayersB).Add(item);
				}
			}
		}
	}

	public void OnFriendListUpdated(bool forceUpdate = false)
	{
		foreach (MPLobbyRecentGamePlayerItemVM item in (Collection<MPLobbyRecentGamePlayerItemVM>)(object)PlayersA)
		{
			item.UpdateNameAndAvatar(forceUpdate);
		}
		foreach (MPLobbyRecentGamePlayerItemVM item2 in (Collection<MPLobbyRecentGamePlayerItemVM>)(object)PlayersB)
		{
			item2.UpdateNameAndAvatar(forceUpdate);
		}
	}

	private static string GetLocalizedCultureNameFromStringID(string cultureID)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		switch (cultureID)
		{
		case "sturgia":
			return ((object)new TextObject("{=PjO7oY16}Sturgia", (Dictionary<string, object>)null)).ToString();
		case "vlandia":
			return ((object)new TextObject("{=FjwRsf1C}Vlandia", (Dictionary<string, object>)null)).ToString();
		case "battania":
			return ((object)new TextObject("{=0B27RrYJ}Battania", (Dictionary<string, object>)null)).ToString();
		case "empire":
			return ((object)new TextObject("{=empirefaction}Empire", (Dictionary<string, object>)null)).ToString();
		case "khuzait":
			return ((object)new TextObject("{=sZLd6VHi}Khuzait", (Dictionary<string, object>)null)).ToString();
		case "aserai":
			return ((object)new TextObject("{=aseraifaction}Aserai", (Dictionary<string, object>)null)).ToString();
		default:
			Debug.FailedAssert("Unidentified culture id: " + cultureID, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Profile\\MPLobbyRecentGameItemVM.cs", "GetLocalizedCultureNameFromStringID", 384);
			return "";
		}
	}
}
