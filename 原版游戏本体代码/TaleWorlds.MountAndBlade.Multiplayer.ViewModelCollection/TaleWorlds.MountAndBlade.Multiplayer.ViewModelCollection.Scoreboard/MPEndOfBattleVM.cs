using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

public class MPEndOfBattleVM : ViewModel
{
	private MissionScoreboardComponent _missionScoreboardComponent;

	private MissionMultiplayerGameModeBaseClient _gameMode;

	private MissionLobbyComponent _lobbyComponent;

	private bool _isSingleTeam;

	private BattleSideEnum _allyBattleSide;

	private BattleSideEnum _enemyBattleSide;

	private bool _isAvailable;

	private string _countdownTitle;

	private int _countdown;

	private string _header;

	private int _battleResult;

	private string _resultText;

	private MPEndOfBattleSideVM _allySide;

	private MPEndOfBattleSideVM _enemySide;

	private MissionRepresentativeBase missionRep
	{
		get
		{
			NetworkCommunicator myPeer = GameNetwork.MyPeer;
			if (myPeer == null)
			{
				return null;
			}
			VirtualPlayer virtualPlayer = myPeer.VirtualPlayer;
			if (virtualPlayer == null)
			{
				return null;
			}
			return virtualPlayer.GetComponent<MissionRepresentativeBase>();
		}
	}

	[DataSourceProperty]
	public bool IsAvailable
	{
		get
		{
			return _isAvailable;
		}
		set
		{
			if (value != _isAvailable)
			{
				_isAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAvailable");
			}
		}
	}

	[DataSourceProperty]
	public string CountdownTitle
	{
		get
		{
			return _countdownTitle;
		}
		set
		{
			if (value != _countdownTitle)
			{
				_countdownTitle = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CountdownTitle");
			}
		}
	}

	[DataSourceProperty]
	public int Countdown
	{
		get
		{
			return _countdown;
		}
		set
		{
			if (value != _countdown)
			{
				_countdown = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Countdown");
			}
		}
	}

	[DataSourceProperty]
	public string Header
	{
		get
		{
			return _header;
		}
		set
		{
			if (value != _header)
			{
				_header = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Header");
			}
		}
	}

	[DataSourceProperty]
	public int BattleResult
	{
		get
		{
			return _battleResult;
		}
		set
		{
			if (value != _battleResult)
			{
				_battleResult = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "BattleResult");
			}
		}
	}

	[DataSourceProperty]
	public string ResultText
	{
		get
		{
			return _resultText;
		}
		set
		{
			if (value != _resultText)
			{
				_resultText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ResultText");
			}
		}
	}

	[DataSourceProperty]
	public MPEndOfBattleSideVM AllySide
	{
		get
		{
			return _allySide;
		}
		set
		{
			if (value != _allySide)
			{
				_allySide = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPEndOfBattleSideVM>(value, "AllySide");
			}
		}
	}

	[DataSourceProperty]
	public MPEndOfBattleSideVM EnemySide
	{
		get
		{
			return _enemySide;
		}
		set
		{
			if (value != _enemySide)
			{
				_enemySide = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPEndOfBattleSideVM>(value, "EnemySide");
			}
		}
	}

	public MPEndOfBattleVM(Mission mission, MissionScoreboardComponent missionScoreboardComponent, bool isSingleTeam)
	{
		_missionScoreboardComponent = missionScoreboardComponent;
		_gameMode = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		_lobbyComponent = mission.GetMissionBehavior<MissionLobbyComponent>();
		_lobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
		_isSingleTeam = isSingleTeam;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		CountdownTitle = ((object)new TextObject("{=wGjQgQlY}Next Game begins in:", (Dictionary<string, object>)null)).ToString();
		Header = ((object)new TextObject("{=HXxNfncd}End of Battle", (Dictionary<string, object>)null)).ToString();
		MPEndOfBattleSideVM allySide = AllySide;
		if (allySide != null)
		{
			((ViewModel)allySide).RefreshValues();
		}
		MPEndOfBattleSideVM enemySide = EnemySide;
		if (enemySide != null)
		{
			((ViewModel)enemySide).RefreshValues();
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		_lobbyComponent.OnPostMatchEnded -= OnPostMatchEnded;
	}

	public void Tick(float dt)
	{
		Countdown = MathF.Ceiling(_gameMode.RemainingTime);
	}

	private void OnPostMatchEnded()
	{
		OnFinalRoundEnded();
	}

	private void OnFinalRoundEnded()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!_isSingleTeam)
		{
			IsAvailable = true;
			InitSides();
			MissionScoreboardComponent missionScoreboardComponent = _missionScoreboardComponent;
			BattleSideEnum val = (BattleSideEnum)((missionScoreboardComponent == null) ? (-1) : ((int)missionScoreboardComponent.GetMatchWinnerSide()));
			if (val == _enemyBattleSide)
			{
				BattleResult = 0;
				ResultText = ((object)GameTexts.FindText("str_defeat", (string)null)).ToString();
			}
			else if (val == _allyBattleSide)
			{
				BattleResult = 1;
				ResultText = ((object)GameTexts.FindText("str_victory", (string)null)).ToString();
			}
			else
			{
				BattleResult = 2;
				ResultText = ((object)GameTexts.FindText("str_draw", (string)null)).ToString();
			}
		}
	}

	private void InitSides()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Invalid comparison between Unknown and I4
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		_allyBattleSide = (BattleSideEnum)1;
		_enemyBattleSide = (BattleSideEnum)0;
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		MissionPeer val = ((myPeer != null) ? PeerExtensions.GetComponent<MissionPeer>(myPeer) : null);
		if (val != null)
		{
			Team team = val.Team;
			if (team != null && (int)team.Side == 0)
			{
				_allyBattleSide = (BattleSideEnum)0;
				_enemyBattleSide = (BattleSideEnum)1;
			}
		}
		MissionScoreboardSide val2 = ((IEnumerable<MissionScoreboardSide>)_missionScoreboardComponent.Sides).FirstOrDefault((Func<MissionScoreboardSide, bool>)((MissionScoreboardSide s) => s != null && s.Side == _allyBattleSide));
		MissionScoreboardSide val3 = ((IEnumerable<MissionScoreboardSide>)_missionScoreboardComponent.Sides).FirstOrDefault((Func<MissionScoreboardSide, bool>)((MissionScoreboardSide s) => s != null && s.Side == _enemyBattleSide));
		string text = ((val2 != null && (int)val2.Side == 1) ? MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1) : MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
		string text2 = ((val3 != null && (int)val3.Side == 1) ? MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1) : MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
		BasicCultureObject obj = (string.IsNullOrEmpty(text) ? null : MBObjectManager.Instance.GetObject<BasicCultureObject>(text));
		BasicCultureObject val4 = (string.IsNullOrEmpty(text2) ? null : MBObjectManager.Instance.GetObject<BasicCultureObject>(text2));
		MultiplayerBattleColors val5 = MultiplayerBattleColors.CreateWith(obj, val4);
		if (val2 != null)
		{
			AllySide = new MPEndOfBattleSideVM(_missionScoreboardComponent, val2, val5.AttackerColors);
		}
		if (val3 != null)
		{
			EnemySide = new MPEndOfBattleSideVM(_missionScoreboardComponent, val3, val5.DefenderColors);
		}
	}
}
