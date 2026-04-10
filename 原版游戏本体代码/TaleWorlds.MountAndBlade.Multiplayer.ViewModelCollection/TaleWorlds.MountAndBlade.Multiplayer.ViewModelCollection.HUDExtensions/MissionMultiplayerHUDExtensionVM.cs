using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;

public class MissionMultiplayerHUDExtensionVM : ViewModel
{
	private const float RemainingTimeWarningThreshold = 5f;

	private readonly Mission _mission;

	private readonly Dictionary<MissionPeer, MPPlayerVM> _teammateDictionary;

	private readonly Dictionary<MissionPeer, MPPlayerVM> _enemyDictionary;

	private readonly MissionScoreboardComponent _missionScoreboardComponent;

	private readonly MissionLobbyEquipmentNetworkComponent _missionLobbyEquipmentNetworkComponent;

	private readonly MissionMultiplayerGameModeBaseClient _gameMode;

	private readonly bool _isTeamsEnabled;

	private bool _isAttackerTeamAlly;

	private bool _isTeammateAndEnemiesRelevant;

	private bool _isTeamScoresEnabled;

	private bool _isTeamScoresDirty;

	private bool _isOrderActive;

	private CommanderInfoVM _commanderInfo;

	private MissionMultiplayerSpectatorHUDVM _spectatorControls;

	private bool _warnRemainingTime;

	private bool _isRoundCountdownAvailable;

	private bool _isRoundCountdownSuspended;

	private bool _showTeamScores;

	private string _remainingRoundTime;

	private string _allyTeamColor;

	private string _allyTeamColor2;

	private string _enemyTeamColor;

	private string _enemyTeamColor2;

	private string _warmupInfoText;

	private int _allyTeamScore = -1;

	private int _enemyTeamScore = -1;

	private MBBindingList<MPPlayerVM> _teammatesList;

	private MBBindingList<MPPlayerVM> _enemiesList;

	private bool _showHUD;

	private bool _showCommanderInfo;

	private bool _showPowerLevels;

	private bool _isInWarmup;

	private int _generalWarningCountdown;

	private bool _isGeneralWarningCountdownActive;

	private BannerImageIdentifierVM _defenderBanner;

	private BannerImageIdentifierVM _attackerBanner;

	private Team _playerTeam
	{
		get
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			if (!GameNetwork.IsMyPeerReady)
			{
				return null;
			}
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
			if (component == null)
			{
				return null;
			}
			if (component == null)
			{
				return null;
			}
			if (component.Team == null || (int)component.Team.Side == -1)
			{
				return null;
			}
			return component.Team;
		}
	}

	[DataSourceProperty]
	public bool IsOrderActive
	{
		get
		{
			return _isOrderActive;
		}
		set
		{
			if (value != _isOrderActive)
			{
				_isOrderActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsOrderActive");
			}
		}
	}

	[DataSourceProperty]
	public CommanderInfoVM CommanderInfo
	{
		get
		{
			return _commanderInfo;
		}
		set
		{
			if (value != _commanderInfo)
			{
				_commanderInfo = value;
				((ViewModel)this).OnPropertyChangedWithValue<CommanderInfoVM>(value, "CommanderInfo");
			}
		}
	}

	[DataSourceProperty]
	public MissionMultiplayerSpectatorHUDVM SpectatorControls
	{
		get
		{
			return _spectatorControls;
		}
		set
		{
			if (value != _spectatorControls)
			{
				_spectatorControls = value;
				((ViewModel)this).OnPropertyChangedWithValue<MissionMultiplayerSpectatorHUDVM>(value, "SpectatorControls");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPPlayerVM> Teammates
	{
		get
		{
			return _teammatesList;
		}
		set
		{
			if (value != _teammatesList)
			{
				_teammatesList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPPlayerVM>>(value, "Teammates");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPPlayerVM> Enemies
	{
		get
		{
			return _enemiesList;
		}
		set
		{
			if (value != _enemiesList)
			{
				_enemiesList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPPlayerVM>>(value, "Enemies");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM AllyBanner
	{
		get
		{
			return _defenderBanner;
		}
		set
		{
			if (value != _defenderBanner)
			{
				_defenderBanner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "AllyBanner");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM EnemyBanner
	{
		get
		{
			return _attackerBanner;
		}
		set
		{
			if (value != _attackerBanner)
			{
				_attackerBanner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "EnemyBanner");
			}
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
	public bool IsRoundCountdownSuspended
	{
		get
		{
			return _isRoundCountdownSuspended;
		}
		set
		{
			if (value != _isRoundCountdownSuspended)
			{
				_isRoundCountdownSuspended = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRoundCountdownSuspended");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowTeamScores
	{
		get
		{
			return _showTeamScores;
		}
		set
		{
			if (value != _showTeamScores)
			{
				_showTeamScores = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowTeamScores");
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
	public bool WarnRemainingTime
	{
		get
		{
			return _warnRemainingTime;
		}
		set
		{
			if (value != _warnRemainingTime)
			{
				_warnRemainingTime = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "WarnRemainingTime");
			}
		}
	}

	[DataSourceProperty]
	public int AllyTeamScore
	{
		get
		{
			return _allyTeamScore;
		}
		set
		{
			if (value != _allyTeamScore)
			{
				_allyTeamScore = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AllyTeamScore");
			}
		}
	}

	[DataSourceProperty]
	public int EnemyTeamScore
	{
		get
		{
			return _enemyTeamScore;
		}
		set
		{
			if (value != _enemyTeamScore)
			{
				_enemyTeamScore = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "EnemyTeamScore");
			}
		}
	}

	[DataSourceProperty]
	public string AllyTeamColor
	{
		get
		{
			return _allyTeamColor;
		}
		set
		{
			if (value != _allyTeamColor)
			{
				_allyTeamColor = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AllyTeamColor");
			}
		}
	}

	[DataSourceProperty]
	public string AllyTeamColor2
	{
		get
		{
			return _allyTeamColor2;
		}
		set
		{
			if (value != _allyTeamColor2)
			{
				_allyTeamColor2 = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AllyTeamColor2");
			}
		}
	}

	[DataSourceProperty]
	public string EnemyTeamColor
	{
		get
		{
			return _enemyTeamColor;
		}
		set
		{
			if (value != _enemyTeamColor)
			{
				_enemyTeamColor = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "EnemyTeamColor");
			}
		}
	}

	[DataSourceProperty]
	public string EnemyTeamColor2
	{
		get
		{
			return _enemyTeamColor2;
		}
		set
		{
			if (value != _enemyTeamColor2)
			{
				_enemyTeamColor2 = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "EnemyTeamColor2");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowHud
	{
		get
		{
			return _showHUD;
		}
		set
		{
			if (value != _showHUD)
			{
				_showHUD = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowHud");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowCommanderInfo
	{
		get
		{
			return _showCommanderInfo;
		}
		set
		{
			if (value != _showCommanderInfo)
			{
				_showCommanderInfo = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowCommanderInfo");
				UpdateShowTeamScores();
			}
		}
	}

	[DataSourceProperty]
	public bool ShowPowerLevels
	{
		get
		{
			return _showPowerLevels;
		}
		set
		{
			if (value != _showPowerLevels)
			{
				_showPowerLevels = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowPowerLevels");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInWarmup
	{
		get
		{
			return _isInWarmup;
		}
		set
		{
			if (value != _isInWarmup)
			{
				_isInWarmup = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInWarmup");
				UpdateShowTeamScores();
				CommanderInfo?.UpdateWarmupDependentFlags(_isInWarmup);
			}
		}
	}

	[DataSourceProperty]
	public string WarmupInfoText
	{
		get
		{
			return _warmupInfoText;
		}
		set
		{
			if (value != _warmupInfoText)
			{
				_warmupInfoText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "WarmupInfoText");
			}
		}
	}

	[DataSourceProperty]
	public int GeneralWarningCountdown
	{
		get
		{
			return _generalWarningCountdown;
		}
		set
		{
			if (value != _generalWarningCountdown)
			{
				_generalWarningCountdown = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "GeneralWarningCountdown");
			}
		}
	}

	[DataSourceProperty]
	public bool IsGeneralWarningCountdownActive
	{
		get
		{
			return _isGeneralWarningCountdownActive;
		}
		set
		{
			if (value != _isGeneralWarningCountdownActive)
			{
				_isGeneralWarningCountdownActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsGeneralWarningCountdownActive");
			}
		}
	}

	public MissionMultiplayerHUDExtensionVM(Mission mission)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Invalid comparison between Unknown and I4
		_mission = mission;
		_missionScoreboardComponent = mission.GetMissionBehavior<MissionScoreboardComponent>();
		_gameMode = _mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		SpectatorControls = new MissionMultiplayerSpectatorHUDVM(_mission);
		if (_gameMode.RoundComponent != null)
		{
			_gameMode.RoundComponent.OnCurrentRoundStateChanged += OnCurrentGameModeStateChanged;
		}
		if (_gameMode.WarmupComponent != null)
		{
			_gameMode.WarmupComponent.OnWarmupEnded += OnCurrentGameModeStateChanged;
		}
		_missionScoreboardComponent.OnRoundPropertiesChanged += SetTeamScoresDirty;
		MissionPeer.OnTeamChanged += new OnTeamChangedDelegate(OnTeamChanged);
		NetworkCommunicator.OnPeerComponentAdded += OnPeerComponentAdded;
		Mission.Current.OnMissionReset += OnMissionReset;
		MissionLobbyComponent missionBehavior = mission.GetMissionBehavior<MissionLobbyComponent>();
		_isTeamsEnabled = (int)missionBehavior.MissionType != 1;
		_missionLobbyEquipmentNetworkComponent = mission.GetMissionBehavior<MissionLobbyEquipmentNetworkComponent>();
		IsRoundCountdownAvailable = _gameMode.IsGameModeUsingRoundCountdown;
		IsRoundCountdownSuspended = false;
		_isTeamScoresEnabled = _isTeamsEnabled;
		UpdateShowTeamScores();
		Teammates = new MBBindingList<MPPlayerVM>();
		Enemies = new MBBindingList<MPPlayerVM>();
		_teammateDictionary = new Dictionary<MissionPeer, MPPlayerVM>();
		_enemyDictionary = new Dictionary<MissionPeer, MPPlayerVM>();
		ShowHud = true;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		string strValue = MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1);
		TextObject val = new TextObject("{=XJTX8w8M}Warmup Phase - {GAME_MODE}{newline}Waiting for players to join", (Dictionary<string, object>)null);
		val.SetTextVariable("GAME_MODE", GameTexts.FindText("str_multiplayer_official_game_type_name", strValue));
		WarmupInfoText = ((object)val).ToString();
		((ViewModel)SpectatorControls).RefreshValues();
	}

	private void OnMissionReset(object sender, PropertyChangedEventArgs e)
	{
		IsGeneralWarningCountdownActive = false;
	}

	private void OnPeerComponentAdded(PeerComponent component)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Invalid comparison between Unknown and I4
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Invalid comparison between Unknown and I4
		if (component.IsMine && component is MissionRepresentativeBase)
		{
			NetworkCommunicator myPeer = GameNetwork.MyPeer;
			MissionRepresentativeBase missionRepresentative = ((myPeer != null) ? myPeer.VirtualPlayer.GetComponent<MissionRepresentativeBase>() : null);
			AllyTeamScore = _missionScoreboardComponent.GetRoundScore((BattleSideEnum)1);
			EnemyTeamScore = _missionScoreboardComponent.GetRoundScore((BattleSideEnum)0);
			_isTeammateAndEnemiesRelevant = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>().IsGameModeTactical && !Mission.Current.HasMissionBehavior<MissionMultiplayerSiegeClient>() && (int)_gameMode.GameType != 3;
			CommanderInfo = new CommanderInfoVM(missionRepresentative);
			ShowCommanderInfo = true;
			if (_isTeammateAndEnemiesRelevant)
			{
				OnRefreshTeamMembers();
				OnRefreshEnemyMembers();
			}
			ShowPowerLevels = (int)_gameMode.GameType == 3;
		}
	}

	public override void OnFinalize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		MissionPeer.OnTeamChanged -= new OnTeamChangedDelegate(OnTeamChanged);
		if (_gameMode.RoundComponent != null)
		{
			_gameMode.RoundComponent.OnCurrentRoundStateChanged -= OnCurrentGameModeStateChanged;
		}
		if (_gameMode.WarmupComponent != null)
		{
			_gameMode.WarmupComponent.OnWarmupEnded -= OnCurrentGameModeStateChanged;
		}
		_missionScoreboardComponent.OnRoundPropertiesChanged -= SetTeamScoresDirty;
		NetworkCommunicator.OnPeerComponentAdded -= OnPeerComponentAdded;
		CommanderInfoVM commanderInfo = CommanderInfo;
		if (commanderInfo != null)
		{
			((ViewModel)commanderInfo).OnFinalize();
		}
		CommanderInfo = null;
		MissionMultiplayerSpectatorHUDVM spectatorControls = SpectatorControls;
		if (spectatorControls != null)
		{
			((ViewModel)spectatorControls).OnFinalize();
		}
		SpectatorControls = null;
		((ViewModel)this).OnFinalize();
	}

	public void Tick(float dt)
	{
		IsInWarmup = _gameMode.IsInWarmup;
		CheckTimers();
		if (_isTeammateAndEnemiesRelevant)
		{
			OnRefreshTeamMembers();
			OnRefreshEnemyMembers();
		}
		if (_isTeamScoresDirty)
		{
			UpdateTeamScores();
			_isTeamScoresDirty = false;
		}
		_commanderInfo?.Tick(dt);
		_spectatorControls?.Tick(dt);
	}

	private void CheckTimers(bool forceUpdate = false)
	{
		int num = default(int);
		int num2 = default(int);
		if (_gameMode.CheckTimer(ref num, ref num2, forceUpdate))
		{
			RemainingRoundTime = TimeSpan.FromSeconds(num).ToString("mm':'ss");
			WarnRemainingTime = (float)num <= 5f;
			if (GeneralWarningCountdown != num2)
			{
				IsGeneralWarningCountdownActive = num2 > 0;
				GeneralWarningCountdown = num2;
			}
		}
	}

	private void OnToggleLoadout(bool isActive)
	{
		ShowHud = !isActive;
	}

	public void OnSpectatedAgentFocusIn(Agent followedAgent)
	{
		_spectatorControls?.OnSpectatedAgentFocusIn(followedAgent);
	}

	public void OnSpectatedAgentFocusOut(Agent followedPeer)
	{
		_spectatorControls?.OnSpectatedAgentFocusOut(followedPeer);
	}

	private void OnCurrentGameModeStateChanged()
	{
		CheckTimers(forceUpdate: true);
	}

	private void SetTeamScoresDirty()
	{
		_isTeamScoresDirty = true;
	}

	private void UpdateTeamScores()
	{
		if (_isTeamScoresEnabled)
		{
			int roundScore = _missionScoreboardComponent.GetRoundScore((BattleSideEnum)1);
			int roundScore2 = _missionScoreboardComponent.GetRoundScore((BattleSideEnum)0);
			AllyTeamScore = (_isAttackerTeamAlly ? roundScore : roundScore2);
			EnemyTeamScore = (_isAttackerTeamAlly ? roundScore2 : roundScore);
		}
	}

	private void UpdateTeamBanners()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		Team attackerTeam = Mission.Current.AttackerTeam;
		BannerImageIdentifierVM val = new BannerImageIdentifierVM((attackerTeam != null) ? attackerTeam.Banner : null, true);
		Team defenderTeam = Mission.Current.DefenderTeam;
		BannerImageIdentifierVM val2 = new BannerImageIdentifierVM((defenderTeam != null) ? defenderTeam.Banner : null, true);
		AllyBanner = (_isAttackerTeamAlly ? val : val2);
		EnemyBanner = (_isAttackerTeamAlly ? val2 : val);
	}

	private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Invalid comparison between Unknown and I4
		if (peer.IsMine)
		{
			if (_isTeamScoresEnabled || (int)_gameMode.GameType == 3)
			{
				_isAttackerTeamAlly = (int)newTeam.Side == 1;
				SetTeamScoresDirty();
			}
			CommanderInfo?.OnTeamChanged();
		}
		if (CommanderInfo == null)
		{
			return;
		}
		((IEnumerable<MPPlayerVM>)Teammates).SingleOrDefault((MPPlayerVM x) => PeerExtensions.GetNetworkPeer((PeerComponent)(object)x.Peer) == peer)?.RefreshTeam();
		GetTeamColors(Mission.Current.AttackerTeam, out var color, out var color2);
		if (_isTeamScoresEnabled || (int)_gameMode.GameType == 3)
		{
			GetTeamColors(Mission.Current.DefenderTeam, out var color3, out var color4);
			if (_isAttackerTeamAlly)
			{
				AllyTeamColor = color;
				AllyTeamColor2 = color2;
				EnemyTeamColor = color3;
				EnemyTeamColor2 = color4;
			}
			else
			{
				AllyTeamColor = color3;
				AllyTeamColor2 = color4;
				EnemyTeamColor = color;
				EnemyTeamColor2 = color2;
			}
			CommanderInfo.RefreshColors(AllyTeamColor, AllyTeamColor2, EnemyTeamColor, EnemyTeamColor2);
		}
		else
		{
			AllyTeamColor = color;
			AllyTeamColor2 = color2;
			CommanderInfo.RefreshColors(AllyTeamColor, AllyTeamColor2, EnemyTeamColor, EnemyTeamColor2);
		}
		UpdateTeamBanners();
	}

	private void GetTeamColors(Team team, out string color, out string color2)
	{
		color = team.Color.ToString("X");
		color = color.Remove(0, 2);
		color = "#" + color + "FF";
		color2 = team.Color2.ToString("X");
		color2 = color2.Remove(0, 2);
		color2 = "#" + color2 + "FF";
	}

	private void OnRefreshTeamMembers()
	{
		List<MPPlayerVM> list = ((IEnumerable<MPPlayerVM>)Teammates).ToList();
		foreach (MissionPeer item in VirtualPlayer.Peers<MissionPeer>())
		{
			if (PeerExtensions.GetComponent<MissionPeer>(PeerExtensions.GetNetworkPeer((PeerComponent)(object)item)) != null && _playerTeam != null && item.Team != null && item.Team == _playerTeam && item.Team != Mission.Current.SpectatorTeam)
			{
				if (_teammateDictionary.TryGetValue(item, out var value))
				{
					list.Remove(value);
					continue;
				}
				MPPlayerVM mPPlayerVM = new MPPlayerVM(item);
				((Collection<MPPlayerVM>)(object)Teammates).Add(mPPlayerVM);
				_teammateDictionary.Add(item, mPPlayerVM);
			}
		}
		foreach (MPPlayerVM item2 in list)
		{
			((Collection<MPPlayerVM>)(object)Teammates).Remove(item2);
			_teammateDictionary.Remove(item2.Peer);
		}
		foreach (MPPlayerVM item3 in (Collection<MPPlayerVM>)(object)Teammates)
		{
			item3.RefreshDivision();
			item3.RefreshGold();
			item3.RefreshProperties();
			item3.UpdateDisabled();
		}
	}

	private void OnRefreshEnemyMembers()
	{
		List<MPPlayerVM> list = ((IEnumerable<MPPlayerVM>)Enemies).ToList();
		foreach (MissionPeer item in VirtualPlayer.Peers<MissionPeer>())
		{
			if (PeerExtensions.GetComponent<MissionPeer>(PeerExtensions.GetNetworkPeer((PeerComponent)(object)item)) != null && _playerTeam != null && item.Team != null && item.Team != _playerTeam && item.Team != Mission.Current.SpectatorTeam)
			{
				if (_enemyDictionary.TryGetValue(item, out var value))
				{
					list.Remove(value);
					continue;
				}
				MPPlayerVM mPPlayerVM = new MPPlayerVM(item);
				((Collection<MPPlayerVM>)(object)Enemies).Add(mPPlayerVM);
				_enemyDictionary.Add(item, mPPlayerVM);
			}
		}
		foreach (MPPlayerVM item2 in list)
		{
			((Collection<MPPlayerVM>)(object)Enemies).Remove(item2);
			_enemyDictionary.Remove(item2.Peer);
		}
		foreach (MPPlayerVM item3 in (Collection<MPPlayerVM>)(object)Enemies)
		{
			item3.RefreshDivision();
			item3.UpdateDisabled();
		}
	}

	private void UpdateShowTeamScores()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		ShowTeamScores = !_gameMode.IsInWarmup && ShowCommanderInfo && (int)_gameMode.GameType != 2;
	}
}
