using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPLobbyGameSearchVM : ViewModel
{
	private float _waitingTimeElapsed;

	private string _shortTimeTextFormat = "mm\\:ss";

	private string _longTimeTextFormat = "hh\\:mm\\:ss";

	private bool _isEnabled;

	private bool _canCancelSearch;

	private bool _canEnterPracticeBattle;

	private bool _showStats;

	private string _titleText;

	private string _gameTypesText;

	private string _cancelText;

	private string _practiceText;

	private string _averageWaitingTime;

	private string _averageWaitingTimeDescription;

	private string _currentWaitingTime;

	private string _currentWaitingTimeDescription;

	public MPCustomGameVM.CustomGameMode CustomGameMode { get; private set; }

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
	public bool CanEnterPracticeBattle
	{
		get
		{
			return _canEnterPracticeBattle;
		}
		set
		{
			if (value != _canEnterPracticeBattle)
			{
				_canEnterPracticeBattle = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanEnterPracticeBattle");
			}
		}
	}

	[DataSourceProperty]
	public bool CanCancelSearch
	{
		get
		{
			return _canCancelSearch;
		}
		set
		{
			if (value != _canCancelSearch)
			{
				_canCancelSearch = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanCancelSearch");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowStats
	{
		get
		{
			return _showStats;
		}
		set
		{
			if (value != _showStats)
			{
				_showStats = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowStats");
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
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string GameTypesText
	{
		get
		{
			return _gameTypesText;
		}
		set
		{
			if (value != _gameTypesText)
			{
				_gameTypesText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameTypesText");
			}
		}
	}

	[DataSourceProperty]
	public string CancelText
	{
		get
		{
			return _cancelText;
		}
		set
		{
			if (value != _cancelText)
			{
				_cancelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelText");
			}
		}
	}

	[DataSourceProperty]
	public string PracticeText
	{
		get
		{
			return _practiceText;
		}
		set
		{
			if (value != _practiceText)
			{
				_practiceText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PracticeText");
			}
		}
	}

	[DataSourceProperty]
	public string AverageWaitingTime
	{
		get
		{
			return _averageWaitingTime;
		}
		set
		{
			if (value != _averageWaitingTime)
			{
				_averageWaitingTime = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AverageWaitingTime");
			}
		}
	}

	[DataSourceProperty]
	public string AverageWaitingTimeDescription
	{
		get
		{
			return _averageWaitingTimeDescription;
		}
		set
		{
			if (value != _averageWaitingTimeDescription)
			{
				_averageWaitingTimeDescription = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AverageWaitingTimeDescription");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentWaitingTime
	{
		get
		{
			return _currentWaitingTime;
		}
		set
		{
			if (value != _currentWaitingTime)
			{
				_currentWaitingTime = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentWaitingTime");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentWaitingTimeDescription
	{
		get
		{
			return _currentWaitingTimeDescription;
		}
		set
		{
			if (value != _currentWaitingTimeDescription)
			{
				_currentWaitingTimeDescription = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentWaitingTimeDescription");
			}
		}
	}

	public MPLobbyGameSearchVM()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		GameTypesText = ((object)new TextObject("{=cK5DE88I}N/A", (Dictionary<string, object>)null)).ToString();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		if (CustomGameMode == MPCustomGameVM.CustomGameMode.PremadeGame)
		{
			TitleText = ((object)new TextObject("{=dkPL25g9}Waiting for an opponent team", (Dictionary<string, object>)null)).ToString();
			GameTypesText = "";
			ShowStats = false;
		}
		else
		{
			TitleText = ((object)new TextObject("{=FD7EQDmW}Looking for game", (Dictionary<string, object>)null)).ToString();
			ShowStats = true;
		}
		GameTexts.SetVariable("STR1", "");
		GameTexts.SetVariable("STR2", new TextObject("{=mFMPj9zg}Searching for matches", (Dictionary<string, object>)null));
		CurrentWaitingTimeDescription = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		GameTexts.SetVariable("STR2", new TextObject("{=18yFEEIL}Estimated wait time", (Dictionary<string, object>)null));
		AverageWaitingTimeDescription = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		CancelText = ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString();
		PracticeText = ((object)new TextObject("{=cjBboOaH}Practice while waiting", (Dictionary<string, object>)null)).ToString();
	}

	public void OnTick(float dt)
	{
		if (IsEnabled)
		{
			_waitingTimeElapsed += dt;
			CurrentWaitingTime = SecondsToString(_waitingTimeElapsed);
		}
	}

	public void SetEnabled(bool enabled)
	{
		IsEnabled = enabled;
		if (enabled)
		{
			CanCancelSearch = true;
			CanEnterPracticeBattle = false;
			_waitingTimeElapsed = 0f;
		}
		((ViewModel)this).RefreshValues();
		if (CustomGameMode != MPCustomGameVM.CustomGameMode.PremadeGame)
		{
			UpdateCanCancel();
		}
	}

	public void UpdateData(MatchmakingWaitTimeStats matchmakingWaitTimeStats, string[] gameTypeInfo)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		ShowStats = true;
		CustomGameMode = MPCustomGameVM.CustomGameMode.CustomServer;
		TitleText = ((object)new TextObject("{=FD7EQDmW}Looking for game", (Dictionary<string, object>)null)).ToString();
		int num = 0;
		string[] array = GameTypesText.Replace(" ", "").Split(new char[1] { ',' });
		string[] array2 = array;
		foreach (string text in array2)
		{
			WaitTimeStatType val = (WaitTimeStatType)0;
			if (NetworkMain.GameClient.PlayersInParty.Count >= 3 && NetworkMain.GameClient.PlayersInParty.Count <= 5)
			{
				val = (WaitTimeStatType)1;
			}
			else if (NetworkMain.GameClient.IsPartyFull)
			{
				val = (WaitTimeStatType)2;
			}
			num += matchmakingWaitTimeStats.GetWaitTime(MultiplayerMain.GetUserCurrentRegion(), text, val);
		}
		AverageWaitingTime = SecondsToString(num / array.Length);
		if (gameTypeInfo != null)
		{
			GameTypesText = MPLobbyVM.GetLocalizedGameTypesString(gameTypeInfo);
		}
	}

	public void UpdatePremadeGameData()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		ShowStats = false;
		CustomGameMode = MPCustomGameVM.CustomGameMode.PremadeGame;
		TitleText = ((object)new TextObject("{=dkPL25g9}Waiting for an opponent team", (Dictionary<string, object>)null)).ToString();
		GameTypesText = "";
	}

	public void OnJoinPremadeGameRequestSuccessful()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		TitleText = ((object)new TextObject("{=5coyTZOI}Game is starting!", (Dictionary<string, object>)null)).ToString();
	}

	public void OnRequestedToCancelSearchBattle()
	{
		CanCancelSearch = false;
		CanEnterPracticeBattle = false;
	}

	public void UpdateCanCancel()
	{
		CanCancelSearch = !NetworkMain.GameClient.IsInParty || NetworkMain.GameClient.IsPartyLeader;
		CanEnterPracticeBattle = false;
	}

	private void ExecuteCancel()
	{
		if (CustomGameMode != MPCustomGameVM.CustomGameMode.PremadeGame)
		{
			NetworkMain.GameClient.CancelFindGame();
		}
		else
		{
			NetworkMain.GameClient.CancelCreatingPremadeGame();
		}
	}

	private string SecondsToString(float seconds)
	{
		return TimeSpan.FromSeconds(seconds).ToString((seconds >= 3600f) ? _longTimeTextFormat : _shortTimeTextFormat);
	}
}
