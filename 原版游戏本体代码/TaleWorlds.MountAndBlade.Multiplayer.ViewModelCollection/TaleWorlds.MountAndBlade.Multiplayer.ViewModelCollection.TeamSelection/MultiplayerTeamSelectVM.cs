using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.TeamSelection;

public class MultiplayerTeamSelectVM : ViewModel
{
	private readonly Action _onClose;

	private readonly Action _onAutoAssign;

	private readonly MissionMultiplayerGameModeBaseClient _gameMode;

	private readonly MissionPeer _missionPeer;

	private readonly string _gamemodeStr;

	private string _teamSelectTitle;

	private bool _isRoundCountdownAvailable;

	private string _remainingRoundTime;

	private string _gamemodeLbl;

	private string _autoassignLbl;

	private bool _isCancelDisabled;

	private TeamSelectTeamInstanceVM _team1;

	private TeamSelectTeamInstanceVM _team2;

	private TeamSelectTeamInstanceVM _teamSpectators;

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
	public TeamSelectTeamInstanceVM Team1
	{
		get
		{
			return _team1;
		}
		set
		{
			if (value != _team1)
			{
				_team1 = value;
				((ViewModel)this).OnPropertyChangedWithValue<TeamSelectTeamInstanceVM>(value, "Team1");
			}
		}
	}

	[DataSourceProperty]
	public TeamSelectTeamInstanceVM Team2
	{
		get
		{
			return _team2;
		}
		set
		{
			if (value != _team2)
			{
				_team2 = value;
				((ViewModel)this).OnPropertyChangedWithValue<TeamSelectTeamInstanceVM>(value, "Team2");
			}
		}
	}

	[DataSourceProperty]
	public TeamSelectTeamInstanceVM TeamSpectators
	{
		get
		{
			return _teamSpectators;
		}
		set
		{
			if (value != _teamSpectators)
			{
				_teamSpectators = value;
				((ViewModel)this).OnPropertyChangedWithValue<TeamSelectTeamInstanceVM>(value, "TeamSpectators");
			}
		}
	}

	[DataSourceProperty]
	public string TeamSelectTitle
	{
		get
		{
			return _teamSelectTitle;
		}
		set
		{
			_teamSelectTitle = value;
			((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TeamSelectTitle");
		}
	}

	[DataSourceProperty]
	public bool IsRoundCountdownAvailable
	{
		get
		{
			return _isRoundCountdownAvailable;
		}
		set
		{
			if (value != _isRoundCountdownAvailable)
			{
				_isRoundCountdownAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRoundCountdownAvailable");
			}
		}
	}

	[DataSourceProperty]
	public string RemainingRoundTime
	{
		get
		{
			return _remainingRoundTime;
		}
		set
		{
			if (value != _remainingRoundTime)
			{
				_remainingRoundTime = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RemainingRoundTime");
			}
		}
	}

	[DataSourceProperty]
	public string GamemodeLbl
	{
		get
		{
			return _gamemodeLbl;
		}
		set
		{
			_gamemodeLbl = value;
			((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GamemodeLbl");
		}
	}

	[DataSourceProperty]
	public string AutoassignLbl
	{
		get
		{
			return _autoassignLbl;
		}
		set
		{
			_autoassignLbl = value;
			((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AutoassignLbl");
		}
	}

	[DataSourceProperty]
	public bool IsCancelDisabled
	{
		get
		{
			return _isCancelDisabled;
		}
		set
		{
			_isCancelDisabled = value;
			((ViewModel)this).OnPropertyChangedWithValue(value, "IsCancelDisabled");
		}
	}

	public MultiplayerTeamSelectVM(Mission mission, Action<Team> onChangeTeamTo, Action onAutoAssign, Action onClose, IEnumerable<Team> teams, string gamemode)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		_onClose = onClose;
		_onAutoAssign = onAutoAssign;
		_gamemodeStr = gamemode;
		Debug.Print("MultiplayerTeamSelectVM 1", 0, (DebugColor)12, 17179869184uL);
		_gameMode = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		MissionScoreboardComponent missionBehavior = mission.GetMissionBehavior<MissionScoreboardComponent>();
		Debug.Print("MultiplayerTeamSelectVM 2", 0, (DebugColor)12, 17179869184uL);
		IsRoundCountdownAvailable = _gameMode.IsGameModeUsingRoundCountdown;
		Debug.Print("MultiplayerTeamSelectVM 3", 0, (DebugColor)12, 17179869184uL);
		Team team = teams.FirstOrDefault((Func<Team, bool>)((Team t) => (int)t.Side == -1));
		TeamSpectators = new TeamSelectTeamInstanceVM(missionBehavior, team, null, null, onChangeTeamTo, new MultiplayerCultureColorInfo((BasicCultureObject)null, false));
		Debug.Print("MultiplayerTeamSelectVM 4", 0, (DebugColor)12, 17179869184uL);
		Team val = teams.FirstOrDefault((Func<Team, bool>)((Team t) => (int)t.Side == 1));
		BasicCultureObject val2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1));
		Debug.Print("MultiplayerTeamSelectVM 5", 0, (DebugColor)12, 17179869184uL);
		Team val3 = teams.FirstOrDefault((Func<Team, bool>)((Team t) => (int)t.Side == 0));
		BasicCultureObject val4 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
		MultiplayerBattleColors val5 = MultiplayerBattleColors.CreateWith(val2, val4);
		Team1 = new TeamSelectTeamInstanceVM(missionBehavior, val, val2, val.Banner, onChangeTeamTo, val5.AttackerColors);
		Team2 = new TeamSelectTeamInstanceVM(missionBehavior, val3, val4, val3.Banner, onChangeTeamTo, val5.DefenderColors);
		Debug.Print("MultiplayerTeamSelectVM 6", 0, (DebugColor)12, 17179869184uL);
		if (GameNetwork.IsMyPeerReady)
		{
			_missionPeer = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
			IsCancelDisabled = _missionPeer.Team == null;
		}
		Debug.Print("MultiplayerTeamSelectVM 7", 0, (DebugColor)12, 17179869184uL);
		((ViewModel)this).RefreshValues();
		Debug.Print("MultiplayerTeamSelectVM 8", 0, (DebugColor)12, 17179869184uL);
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		AutoassignLbl = ((object)new TextObject("{=bON4Kn6B}Auto Assign", (Dictionary<string, object>)null)).ToString();
		TeamSelectTitle = ((object)new TextObject("{=aVixswW5}Team Selection", (Dictionary<string, object>)null)).ToString();
		GamemodeLbl = ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", _gamemodeStr)).ToString();
		((ViewModel)Team1).RefreshValues();
		((ViewModel)Team2).RefreshValues();
		((ViewModel)TeamSpectators).RefreshValues();
	}

	public void Tick(float dt)
	{
		RemainingRoundTime = TimeSpan.FromSeconds(MathF.Ceiling(_gameMode.RemainingTime)).ToString("mm':'ss");
	}

	public void RefreshDisabledTeams(List<Team> disabledTeams)
	{
		if (disabledTeams == null)
		{
			TeamSpectators?.SetIsDisabled(isCurrentTeam: false, disabledForBalance: false);
			Team1?.SetIsDisabled(isCurrentTeam: false, disabledForBalance: false);
			Team2?.SetIsDisabled(isCurrentTeam: false, disabledForBalance: false);
			return;
		}
		TeamSpectators?.SetIsDisabled(isCurrentTeam: false, disabledTeams?.Contains(TeamSpectators?.Team) ?? false);
		TeamSelectTeamInstanceVM team = Team1;
		if (team != null)
		{
			Team obj = Team1?.Team;
			MissionPeer missionPeer = _missionPeer;
			team.SetIsDisabled(obj == ((missionPeer != null) ? missionPeer.Team : null), disabledTeams?.Contains(Team1?.Team) ?? false);
		}
		TeamSelectTeamInstanceVM team2 = Team2;
		if (team2 != null)
		{
			Team obj2 = Team2?.Team;
			MissionPeer missionPeer2 = _missionPeer;
			team2.SetIsDisabled(obj2 == ((missionPeer2 != null) ? missionPeer2.Team : null), disabledTeams?.Contains(Team2?.Team) ?? false);
		}
	}

	public void RefreshPlayerAndBotCount(int playersCountOne, int playersCountTwo, int botsCountOne, int botsCountTwo)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		MBTextManager.SetTextVariable("PLAYER_COUNT", playersCountOne.ToString(), false);
		Team1.DisplayedSecondary = ((object)new TextObject("{=Etjqamlh}{PLAYER_COUNT} Players", (Dictionary<string, object>)null)).ToString();
		MBTextManager.SetTextVariable("BOT_COUNT", botsCountOne.ToString(), false);
		Team1.DisplayedSecondarySub = ((object)new TextObject("{=eCOJSSUH}({BOT_COUNT} Bots)", (Dictionary<string, object>)null)).ToString();
		MBTextManager.SetTextVariable("PLAYER_COUNT", playersCountTwo.ToString(), false);
		Team2.DisplayedSecondary = ((object)new TextObject("{=Etjqamlh}{PLAYER_COUNT} Players", (Dictionary<string, object>)null)).ToString();
		MBTextManager.SetTextVariable("BOT_COUNT", botsCountTwo.ToString(), false);
		Team2.DisplayedSecondarySub = ((object)new TextObject("{=eCOJSSUH}({BOT_COUNT} Bots)", (Dictionary<string, object>)null)).ToString();
	}

	public void RefreshFriendsPerTeam(IEnumerable<MissionPeer> friendsTeamOne, IEnumerable<MissionPeer> friendsTeamTwo)
	{
		Team1.RefreshFriends(friendsTeamOne);
		Team2.RefreshFriends(friendsTeamTwo);
	}

	[UsedImplicitly]
	public void ExecuteCancel()
	{
		_onClose();
	}

	[UsedImplicitly]
	public void ExecuteAutoAssign()
	{
		_onAutoAssign();
	}
}
