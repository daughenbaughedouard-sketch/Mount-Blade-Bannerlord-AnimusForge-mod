using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.Compass;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;

public class CommanderInfoVM : ViewModel
{
	private readonly MissionRepresentativeBase _missionRepresentative;

	private readonly MissionMultiplayerGameModeBaseClient _gameMode;

	private readonly MissionMultiplayerSiegeClient _siegeClient;

	private readonly MissionScoreboardComponent _missionScoreboardComponent;

	private const float InitialArmyStrength = 1f;

	private int _attackerTeamInitialMemberCount;

	private int _defenderTeamInitialMemberCount;

	private Team _allyTeam;

	private Team _enemyTeam;

	private ICommanderInfo _commanderInfo;

	private bool _areMoraleEventsRegistered;

	private MBBindingList<CapturePointVM> _allyControlPoints;

	private MBBindingList<CapturePointVM> _neutralControlPoints;

	private MBBindingList<CapturePointVM> _enemyControlPoints;

	private int _allyMoraleIncreaseLevel;

	private int _enemyMoraleIncreaseLevel;

	private int _allyMoralePercentage;

	private int _enemyMoralePercentage;

	private int _allyMemberCount;

	private int _enemyMemberCount;

	private PowerLevelComparer _powerLevelComparer;

	private bool _showTacticalInfo;

	private bool _usePowerComparer;

	private bool _useMoraleComparer;

	private bool _areMoralesIndependent;

	private bool _showControlPointStatus;

	private string _allyTeamColor;

	private string _allyTeamColorSecondary;

	private string _enemyTeamColor;

	private string _enemyTeamColorSecondary;

	[DataSourceProperty]
	public MBBindingList<CapturePointVM> AllyControlPoints
	{
		get
		{
			return _allyControlPoints;
		}
		set
		{
			if (value != _allyControlPoints)
			{
				_allyControlPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CapturePointVM>>(value, "AllyControlPoints");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CapturePointVM> NeutralControlPoints
	{
		get
		{
			return _neutralControlPoints;
		}
		set
		{
			if (value != _neutralControlPoints)
			{
				_neutralControlPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CapturePointVM>>(value, "NeutralControlPoints");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CapturePointVM> EnemyControlPoints
	{
		get
		{
			return _enemyControlPoints;
		}
		set
		{
			if (value != _enemyControlPoints)
			{
				_enemyControlPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CapturePointVM>>(value, "EnemyControlPoints");
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
	public string AllyTeamColorSecondary
	{
		get
		{
			return _allyTeamColorSecondary;
		}
		set
		{
			if (value != _allyTeamColorSecondary)
			{
				_allyTeamColorSecondary = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AllyTeamColorSecondary");
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
	public string EnemyTeamColorSecondary
	{
		get
		{
			return _enemyTeamColorSecondary;
		}
		set
		{
			if (value != _enemyTeamColorSecondary)
			{
				_enemyTeamColorSecondary = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "EnemyTeamColorSecondary");
			}
		}
	}

	[DataSourceProperty]
	public int AllyMoraleIncreaseLevel
	{
		get
		{
			return _allyMoraleIncreaseLevel;
		}
		set
		{
			if (value != _allyMoraleIncreaseLevel)
			{
				_allyMoraleIncreaseLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AllyMoraleIncreaseLevel");
			}
		}
	}

	[DataSourceProperty]
	public int EnemyMoraleIncreaseLevel
	{
		get
		{
			return _enemyMoraleIncreaseLevel;
		}
		set
		{
			if (value != _enemyMoraleIncreaseLevel)
			{
				_enemyMoraleIncreaseLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "EnemyMoraleIncreaseLevel");
			}
		}
	}

	[DataSourceProperty]
	public int AllyMoralePercentage
	{
		get
		{
			return _allyMoralePercentage;
		}
		set
		{
			if (value != _allyMoralePercentage)
			{
				_allyMoralePercentage = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AllyMoralePercentage");
			}
		}
	}

	[DataSourceProperty]
	public int EnemyMoralePercentage
	{
		get
		{
			return _enemyMoralePercentage;
		}
		set
		{
			if (value != _enemyMoralePercentage)
			{
				_enemyMoralePercentage = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "EnemyMoralePercentage");
			}
		}
	}

	[DataSourceProperty]
	public int AllyMemberCount
	{
		get
		{
			return _allyMemberCount;
		}
		set
		{
			if (value != _allyMemberCount)
			{
				_allyMemberCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AllyMemberCount");
			}
		}
	}

	[DataSourceProperty]
	public int EnemyMemberCount
	{
		get
		{
			return _enemyMemberCount;
		}
		set
		{
			if (value != _enemyMemberCount)
			{
				_enemyMemberCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "EnemyMemberCount");
			}
		}
	}

	[DataSourceProperty]
	public PowerLevelComparer PowerLevelComparer
	{
		get
		{
			return _powerLevelComparer;
		}
		set
		{
			if (value != _powerLevelComparer)
			{
				_powerLevelComparer = value;
				((ViewModel)this).OnPropertyChangedWithValue<PowerLevelComparer>(value, "PowerLevelComparer");
			}
		}
	}

	[DataSourceProperty]
	public bool UsePowerComparer
	{
		get
		{
			return _usePowerComparer;
		}
		set
		{
			if (value != _usePowerComparer)
			{
				_usePowerComparer = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "UsePowerComparer");
			}
		}
	}

	[DataSourceProperty]
	public bool UseMoraleComparer
	{
		get
		{
			return _useMoraleComparer;
		}
		set
		{
			if (value != _useMoraleComparer)
			{
				_useMoraleComparer = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "UseMoraleComparer");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowTacticalInfo
	{
		get
		{
			return _showTacticalInfo;
		}
		set
		{
			if (value != _showTacticalInfo)
			{
				_showTacticalInfo = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowTacticalInfo");
			}
		}
	}

	[DataSourceProperty]
	public bool AreMoralesIndependent
	{
		get
		{
			return _areMoralesIndependent;
		}
		set
		{
			if (value != _areMoralesIndependent)
			{
				_areMoralesIndependent = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AreMoralesIndependent");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowControlPointStatus
	{
		get
		{
			return _showControlPointStatus;
		}
		set
		{
			if (value != _showControlPointStatus)
			{
				_showControlPointStatus = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowControlPointStatus");
			}
		}
	}

	public CommanderInfoVM(MissionRepresentativeBase missionRepresentative)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		_missionRepresentative = missionRepresentative;
		AllyControlPoints = new MBBindingList<CapturePointVM>();
		NeutralControlPoints = new MBBindingList<CapturePointVM>();
		EnemyControlPoints = new MBBindingList<CapturePointVM>();
		_gameMode = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		_missionScoreboardComponent = Mission.Current.GetMissionBehavior<MissionScoreboardComponent>();
		_commanderInfo = Mission.Current.GetMissionBehavior<ICommanderInfo>();
		ShowTacticalInfo = true;
		if (_gameMode != null)
		{
			UpdateWarmupDependentFlags(_gameMode.IsInWarmup);
			UsePowerComparer = (int)_gameMode.GameType == 3 && _gameMode.ScoreboardComponent != null;
			if (UsePowerComparer)
			{
				PowerLevelComparer = new PowerLevelComparer(1.0, 1.0);
			}
			if (UseMoraleComparer)
			{
				RegisterMoraleEvents();
			}
		}
		_siegeClient = Mission.Current.GetMissionBehavior<MissionMultiplayerSiegeClient>();
		if (_siegeClient != null)
		{
			_siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent += OnCapturePointRemainingMoraleGainsChanged;
		}
		Mission.Current.OnMissionReset += OnMissionReset;
		MultiplayerMissionAgentVisualSpawnComponent missionBehavior = Mission.Current.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>();
		missionBehavior.OnMyAgentSpawnedFromVisual += OnPreparationEnded;
		missionBehavior.OnMyAgentVisualSpawned += OnRoundStarted;
		OnTeamChanged();
	}

	private void OnRoundStarted()
	{
		OnTeamChanged();
		if (UsePowerComparer)
		{
			_attackerTeamInitialMemberCount = _missionScoreboardComponent.Sides[1].Players.Count();
			_defenderTeamInitialMemberCount = _missionScoreboardComponent.Sides[0].Players.Count();
		}
	}

	private void RegisterMoraleEvents()
	{
		if (!_areMoraleEventsRegistered)
		{
			_commanderInfo.OnMoraleChangedEvent += OnUpdateMorale;
			_commanderInfo.OnFlagNumberChangedEvent += OnNumberOfCapturePointsChanged;
			_commanderInfo.OnCapturePointOwnerChangedEvent += OnCapturePointOwnerChanged;
			AreMoralesIndependent = _commanderInfo.AreMoralesIndependent;
			ResetCapturePointLists();
			InitCapturePoints();
			_areMoraleEventsRegistered = true;
		}
	}

	private void OnPreparationEnded()
	{
		ShowTacticalInfo = true;
		OnTeamChanged();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		if (_commanderInfo != null)
		{
			_commanderInfo.OnMoraleChangedEvent -= OnUpdateMorale;
			_commanderInfo.OnFlagNumberChangedEvent -= OnNumberOfCapturePointsChanged;
			_commanderInfo.OnCapturePointOwnerChangedEvent -= OnCapturePointOwnerChanged;
		}
		Mission.Current.OnMissionReset -= OnMissionReset;
		MultiplayerMissionAgentVisualSpawnComponent missionBehavior = Mission.Current.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>();
		missionBehavior.OnMyAgentSpawnedFromVisual -= OnPreparationEnded;
		missionBehavior.OnMyAgentVisualSpawned -= OnRoundStarted;
		if (_siegeClient != null)
		{
			_siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent -= OnCapturePointRemainingMoraleGainsChanged;
		}
	}

	public void UpdateWarmupDependentFlags(bool isInWarmup)
	{
		UseMoraleComparer = !isInWarmup && _gameMode.IsGameModeTactical && _commanderInfo != null;
		ShowControlPointStatus = !isInWarmup;
		if (!isInWarmup && UseMoraleComparer)
		{
			RegisterMoraleEvents();
		}
	}

	public void OnUpdateMorale(BattleSideEnum side, float morale)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (_allyTeam != null && _allyTeam.Side == side)
		{
			AllyMoralePercentage = MathF.Round(MathF.Abs(morale * 100f));
		}
		else if (_enemyTeam != null && _enemyTeam.Side == side)
		{
			EnemyMoralePercentage = MathF.Round(MathF.Abs(morale * 100f));
		}
	}

	private void OnMissionReset(object sender, PropertyChangedEventArgs e)
	{
		if (UseMoraleComparer)
		{
			AllyMoralePercentage = 50;
			EnemyMoralePercentage = 50;
		}
		if (UsePowerComparer)
		{
			PowerLevelComparer.Update(1.0, 1.0, 1.0, 1.0);
		}
	}

	internal void Tick(float dt)
	{
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Invalid comparison between Unknown and I4
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Invalid comparison between Unknown and I4
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Invalid comparison between Unknown and I4
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Invalid comparison between Unknown and I4
		foreach (CapturePointVM item in (Collection<CapturePointVM>)(object)AllyControlPoints)
		{
			((CompassTargetVM)item).Refresh(0f, 0f, 0f);
		}
		foreach (CapturePointVM item2 in (Collection<CapturePointVM>)(object)EnemyControlPoints)
		{
			((CompassTargetVM)item2).Refresh(0f, 0f, 0f);
		}
		foreach (CapturePointVM item3 in (Collection<CapturePointVM>)(object)NeutralControlPoints)
		{
			((CompassTargetVM)item3).Refresh(0f, 0f, 0f);
		}
		if (_allyTeam != null && UsePowerComparer)
		{
			int count = ((List<Agent>)(object)Mission.Current.AttackerTeam.ActiveAgents).Count;
			int count2 = ((List<Agent>)(object)Mission.Current.DefenderTeam.ActiveAgents).Count;
			AllyMemberCount = (((int)_allyTeam.Side == 1) ? count : count2);
			EnemyMemberCount = (((int)_allyTeam.Side == 1) ? count2 : count);
			int num = (((int)_allyTeam.Side == 1) ? _attackerTeamInitialMemberCount : _defenderTeamInitialMemberCount);
			Team allyTeam = _allyTeam;
			int num2 = ((allyTeam != null && (int)allyTeam.Side == 1) ? _defenderTeamInitialMemberCount : _attackerTeamInitialMemberCount);
			if (num2 == 0 && num == 0)
			{
				PowerLevelComparer.Update(1.0, 1.0, 1.0, 1.0);
			}
			else
			{
				PowerLevelComparer.Update((double)EnemyMemberCount, (double)AllyMemberCount, (double)num2, (double)num);
			}
		}
	}

	private void OnCapturePointOwnerChanged(FlagCapturePoint target, Team newOwnerTeam)
	{
		CapturePointVM capturePointVM = FindCapturePointInLists(target);
		if (capturePointVM != null)
		{
			RemoveFlagFromLists(capturePointVM);
			HandleAddNewCapturePoint(capturePointVM);
			capturePointVM.OnOwnerChanged(newOwnerTeam);
		}
	}

	private void OnCapturePointRemainingMoraleGainsChanged(int[] remainingMoraleArr)
	{
		foreach (CapturePointVM item in (Collection<CapturePointVM>)(object)AllyControlPoints)
		{
			int flagIndex = item.Target.FlagIndex;
			if (flagIndex >= 0 && remainingMoraleArr.Length > flagIndex)
			{
				item.OnRemainingMoraleChanged(remainingMoraleArr[flagIndex]);
			}
		}
		foreach (CapturePointVM item2 in (Collection<CapturePointVM>)(object)EnemyControlPoints)
		{
			int flagIndex2 = item2.Target.FlagIndex;
			if (flagIndex2 >= 0 && remainingMoraleArr.Length > flagIndex2)
			{
				item2.OnRemainingMoraleChanged(remainingMoraleArr[flagIndex2]);
			}
		}
		foreach (CapturePointVM item3 in (Collection<CapturePointVM>)(object)NeutralControlPoints)
		{
			int flagIndex3 = item3.Target.FlagIndex;
			if (flagIndex3 >= 0 && remainingMoraleArr.Length > flagIndex3)
			{
				item3.OnRemainingMoraleChanged(remainingMoraleArr[flagIndex3]);
			}
		}
	}

	private void OnNumberOfCapturePointsChanged()
	{
		ResetCapturePointLists();
		InitCapturePoints();
	}

	private void InitCapturePoints()
	{
		if (_commanderInfo == null)
		{
			return;
		}
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		object obj;
		if (myPeer == null)
		{
			obj = null;
		}
		else
		{
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(myPeer);
			obj = ((component != null) ? component.Team : null);
		}
		if (obj != null)
		{
			FlagCapturePoint[] array = _commanderInfo.AllCapturePoints.Where((FlagCapturePoint c) => !c.IsDeactivated).ToArray();
			foreach (FlagCapturePoint val in array)
			{
				CapturePointVM capturePointVM = new CapturePointVM(val, (TargetIconType)(17 + val.FlagIndex));
				HandleAddNewCapturePoint(capturePointVM);
			}
			RefreshMoraleIncreaseLevels();
		}
	}

	private void HandleAddNewCapturePoint(CapturePointVM capturePointVM)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Invalid comparison between Unknown and I4
		RemoveFlagFromLists(capturePointVM);
		if (_allyTeam != null)
		{
			Team val = _commanderInfo.GetFlagOwner(capturePointVM.Target);
			if (val != null && ((int)val.Side == -1 || (int)val.Side == 2))
			{
				val = null;
			}
			capturePointVM.OnOwnerChanged(val);
			bool isDeactivated = capturePointVM.Target.IsDeactivated;
			if ((val == null || val.TeamIndex == -1) && !isDeactivated)
			{
				int index = MathF.Min(((Collection<CapturePointVM>)(object)NeutralControlPoints).Count, capturePointVM.Target.FlagIndex);
				((Collection<CapturePointVM>)(object)NeutralControlPoints).Insert(index, capturePointVM);
			}
			else if (_allyTeam == val)
			{
				int index2 = MathF.Min(((Collection<CapturePointVM>)(object)AllyControlPoints).Count, capturePointVM.Target.FlagIndex);
				((Collection<CapturePointVM>)(object)AllyControlPoints).Insert(index2, capturePointVM);
			}
			else if (_allyTeam != val)
			{
				int index3 = MathF.Min(((Collection<CapturePointVM>)(object)EnemyControlPoints).Count, capturePointVM.Target.FlagIndex);
				((Collection<CapturePointVM>)(object)EnemyControlPoints).Insert(index3, capturePointVM);
			}
			else if ((int)val.Side != -1)
			{
				Debug.FailedAssert("Incorrect flag team state", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\HUDExtensions\\CommanderInfoVM.cs", "HandleAddNewCapturePoint", 321);
			}
			RefreshMoraleIncreaseLevels();
		}
	}

	private void RefreshMoraleIncreaseLevels()
	{
		AllyMoraleIncreaseLevel = MathF.Max(0, ((Collection<CapturePointVM>)(object)AllyControlPoints).Count - ((Collection<CapturePointVM>)(object)EnemyControlPoints).Count);
		EnemyMoraleIncreaseLevel = MathF.Max(0, ((Collection<CapturePointVM>)(object)EnemyControlPoints).Count - ((Collection<CapturePointVM>)(object)AllyControlPoints).Count);
	}

	private void RemoveFlagFromLists(CapturePointVM capturePoint)
	{
		if (((Collection<CapturePointVM>)(object)AllyControlPoints).Contains(capturePoint))
		{
			((Collection<CapturePointVM>)(object)AllyControlPoints).Remove(capturePoint);
		}
		else if (((Collection<CapturePointVM>)(object)NeutralControlPoints).Contains(capturePoint))
		{
			((Collection<CapturePointVM>)(object)NeutralControlPoints).Remove(capturePoint);
		}
		else if (((Collection<CapturePointVM>)(object)EnemyControlPoints).Contains(capturePoint))
		{
			((Collection<CapturePointVM>)(object)EnemyControlPoints).Remove(capturePoint);
		}
	}

	public void OnTeamChanged()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		if (!GameNetwork.IsMyPeerReady || !ShowTacticalInfo)
		{
			return;
		}
		MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
		_allyTeam = component.Team;
		if (_allyTeam != null)
		{
			IEnumerable<Team> source = ((IEnumerable<Team>)Mission.Current.Teams).Where((Team t) => t.IsEnemyOf(_allyTeam));
			_enemyTeam = source.FirstOrDefault();
			if ((int)_allyTeam.Side == -1)
			{
				_allyTeam = Mission.Current.AttackerTeam;
				return;
			}
			ResetCapturePointLists();
			InitCapturePoints();
		}
	}

	private void ResetCapturePointLists()
	{
		((Collection<CapturePointVM>)(object)AllyControlPoints).Clear();
		((Collection<CapturePointVM>)(object)NeutralControlPoints).Clear();
		((Collection<CapturePointVM>)(object)EnemyControlPoints).Clear();
	}

	private CapturePointVM FindCapturePointInLists(FlagCapturePoint target)
	{
		CapturePointVM capturePointVM = ((IEnumerable<CapturePointVM>)AllyControlPoints).SingleOrDefault((CapturePointVM c) => c.Target == target);
		if (capturePointVM != null)
		{
			return capturePointVM;
		}
		CapturePointVM capturePointVM2 = ((IEnumerable<CapturePointVM>)EnemyControlPoints).SingleOrDefault((CapturePointVM c) => c.Target == target);
		if (capturePointVM2 != null)
		{
			return capturePointVM2;
		}
		CapturePointVM capturePointVM3 = ((IEnumerable<CapturePointVM>)NeutralControlPoints).SingleOrDefault((CapturePointVM c) => c.Target == target);
		if (capturePointVM3 != null)
		{
			return capturePointVM3;
		}
		return null;
	}

	public void RefreshColors(string allyTeamColor, string allyTeamColorSecondary, string enemyTeamColor, string enemyTeamColorSecondary)
	{
		AllyTeamColor = allyTeamColor;
		AllyTeamColorSecondary = allyTeamColorSecondary;
		EnemyTeamColor = enemyTeamColor;
		EnemyTeamColorSecondary = enemyTeamColorSecondary;
		if (UsePowerComparer)
		{
			PowerLevelComparer.SetColors(EnemyTeamColor, AllyTeamColor);
		}
	}
}
