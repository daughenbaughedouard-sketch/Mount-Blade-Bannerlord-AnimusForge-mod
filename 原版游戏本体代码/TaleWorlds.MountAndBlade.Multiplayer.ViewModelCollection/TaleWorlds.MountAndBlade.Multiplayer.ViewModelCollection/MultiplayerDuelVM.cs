using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.KillFeed;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MultiplayerDuelVM : ViewModel
{
	public struct DuelArenaProperties
	{
		public GameEntity FlagEntity;

		public int Index;

		public TroopType ArenaTroopType;

		public DuelArenaProperties(GameEntity flagEntity, int index, TroopType arenaTroopType)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			FlagEntity = flagEntity;
			Index = index;
			ArenaTroopType = arenaTroopType;
		}
	}

	private const string ArenaFlagTag = "area_flag";

	private const string AremaTypeFlagTagBase = "flag_";

	private readonly MissionMultiplayerGameModeDuelClient _client;

	private readonly MissionMultiplayerGameModeBaseClient _gameMode;

	private bool _isMyRepresentativeAssigned;

	private List<DuelArenaProperties> _duelArenaProperties;

	private TextObject _scoreWithSeparatorText;

	private bool _isAgentBuiltForTheFirstTime = true;

	private bool _hasPlayerChangedArenaPreferrence;

	private string _cachedPlayerClassID;

	private bool _showSpawnPoints;

	private Camera _missionCamera;

	private bool _isEnabled;

	private bool _areOngoingDuelsActive;

	private bool _isPlayerInDuel;

	private int _playerBounty;

	private int _playerPreferredArenaType;

	private string _playerScoreText;

	private string _remainingRoundTime;

	private MissionDuelMarkersVM _markers;

	private DuelMatchVM _playerDuelMatch;

	private MBBindingList<DuelMatchVM> _ongoingDuels;

	private MBBindingList<MPDuelKillNotificationItemVM> _killNotifications;

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
	public bool AreOngoingDuelsActive
	{
		get
		{
			return _areOngoingDuelsActive;
		}
		set
		{
			if (value != _areOngoingDuelsActive)
			{
				_areOngoingDuelsActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AreOngoingDuelsActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerInDuel
	{
		get
		{
			return _isPlayerInDuel;
		}
		set
		{
			if (value != _isPlayerInDuel)
			{
				_isPlayerInDuel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerInDuel");
			}
		}
	}

	[DataSourceProperty]
	public int PlayerBounty
	{
		get
		{
			return _playerBounty;
		}
		set
		{
			if (value != _playerBounty)
			{
				_playerBounty = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PlayerBounty");
			}
		}
	}

	[DataSourceProperty]
	public int PlayerPrefferedArenaType
	{
		get
		{
			return _playerPreferredArenaType;
		}
		set
		{
			if (value != _playerPreferredArenaType)
			{
				_playerPreferredArenaType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PlayerPrefferedArenaType");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerScoreText
	{
		get
		{
			return _playerScoreText;
		}
		set
		{
			if (value != _playerScoreText)
			{
				_playerScoreText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PlayerScoreText");
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
	public MissionDuelMarkersVM Markers
	{
		get
		{
			return _markers;
		}
		set
		{
			if (value != _markers)
			{
				_markers = value;
				((ViewModel)this).OnPropertyChangedWithValue<MissionDuelMarkersVM>(value, "Markers");
			}
		}
	}

	[DataSourceProperty]
	public DuelMatchVM PlayerDuelMatch
	{
		get
		{
			return _playerDuelMatch;
		}
		set
		{
			if (value != _playerDuelMatch)
			{
				_playerDuelMatch = value;
				((ViewModel)this).OnPropertyChangedWithValue<DuelMatchVM>(value, "PlayerDuelMatch");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<DuelMatchVM> OngoingDuels
	{
		get
		{
			return _ongoingDuels;
		}
		set
		{
			if (value != _ongoingDuels)
			{
				_ongoingDuels = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<DuelMatchVM>>(value, "OngoingDuels");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPDuelKillNotificationItemVM> KillNotifications
	{
		get
		{
			return _killNotifications;
		}
		set
		{
			if (value != _killNotifications)
			{
				_killNotifications = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPDuelKillNotificationItemVM>>(value, "KillNotifications");
			}
		}
	}

	public MultiplayerDuelVM(Camera missionCamera, MissionMultiplayerGameModeDuelClient client)
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		_missionCamera = missionCamera;
		_client = client;
		MissionMultiplayerGameModeDuelClient client2 = _client;
		client2.OnMyRepresentativeAssigned = (Action)Delegate.Combine(client2.OnMyRepresentativeAssigned, new Action(OnMyRepresentativeAssigned));
		_gameMode = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		PlayerDuelMatch = new DuelMatchVM();
		OngoingDuels = new MBBindingList<DuelMatchVM>();
		_duelArenaProperties = new List<DuelArenaProperties>();
		List<GameEntity> list = new List<GameEntity>();
		list.AddRange(Mission.Current.Scene.FindEntitiesWithTagExpression("area_flag(_\\d+)*"));
		foreach (GameEntity item in list)
		{
			DuelArenaProperties arenaPropertiesOfFlagEntity = GetArenaPropertiesOfFlagEntity(item);
			_duelArenaProperties.Add(arenaPropertiesOfFlagEntity);
		}
		Markers = new MissionDuelMarkersVM(missionCamera, _client);
		KillNotifications = new MBBindingList<MPDuelKillNotificationItemVM>();
		_scoreWithSeparatorText = new TextObject("{=J5rb5YVV}/ {SCORE}", (Dictionary<string, object>)null);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		((ViewModel)PlayerDuelMatch).RefreshValues();
		((ViewModel)Markers).RefreshValues();
	}

	private void OnMyRepresentativeAssigned()
	{
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		DuelMissionRepresentative myRepresentative = _client.MyRepresentative;
		myRepresentative.OnDuelPrepStartedEvent = (Action<MissionPeer, int>)Delegate.Combine(myRepresentative.OnDuelPrepStartedEvent, new Action<MissionPeer, int>(OnDuelPrepStarted));
		DuelMissionRepresentative myRepresentative2 = _client.MyRepresentative;
		myRepresentative2.OnAgentSpawnedWithoutDuelEvent = (Action)Delegate.Combine(myRepresentative2.OnAgentSpawnedWithoutDuelEvent, new Action(OnAgentSpawnedWithoutDuel));
		DuelMissionRepresentative myRepresentative3 = _client.MyRepresentative;
		myRepresentative3.OnDuelPreparationStartedForTheFirstTimeEvent = (Action<MissionPeer, MissionPeer, int>)Delegate.Combine(myRepresentative3.OnDuelPreparationStartedForTheFirstTimeEvent, new Action<MissionPeer, MissionPeer, int>(OnDuelStarted));
		DuelMissionRepresentative myRepresentative4 = _client.MyRepresentative;
		myRepresentative4.OnDuelEndedEvent = (Action<MissionPeer>)Delegate.Combine(myRepresentative4.OnDuelEndedEvent, new Action<MissionPeer>(OnDuelEnded));
		DuelMissionRepresentative myRepresentative5 = _client.MyRepresentative;
		myRepresentative5.OnDuelRoundEndedEvent = (Action<MissionPeer>)Delegate.Combine(myRepresentative5.OnDuelRoundEndedEvent, new Action<MissionPeer>(OnDuelRoundEnded));
		DuelMissionRepresentative myRepresentative6 = _client.MyRepresentative;
		myRepresentative6.OnMyPreferredZoneChanged = (Action<TroopType>)Delegate.Combine(myRepresentative6.OnMyPreferredZoneChanged, new Action<TroopType>(OnPlayerPreferredZoneChanged));
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		Markers.RegisterEvents();
		UpdatePlayerScore();
		_isMyRepresentativeAssigned = true;
	}

	public void Tick(float dt)
	{
		int num = default(int);
		int num2 = default(int);
		if (_gameMode.CheckTimer(ref num, ref num2, false))
		{
			RemainingRoundTime = TimeSpan.FromSeconds(num).ToString("mm':'ss");
		}
		Markers.Tick(dt);
		if (PlayerDuelMatch.IsEnabled)
		{
			PlayerDuelMatch.Tick(dt);
		}
	}

	[Conditional("DEBUG")]
	private void DebugTick()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		if (Input.IsKeyReleased((InputKey)81))
		{
			_showSpawnPoints = !_showSpawnPoints;
		}
		if (!_showSpawnPoints)
		{
			return;
		}
		string text = "spawnpoint_area(_\\d+)*";
		Vec3 val = default(Vec3);
		foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTagExpression(text))
		{
			((Vec3)(ref val))._002Ector(item.GlobalPosition.x, item.GlobalPosition.y, item.GlobalPosition.z, -1f);
			Vec3 val2 = _missionCamera.WorldPointToViewPortPoint(ref val);
			val2.y = 1f - val2.y;
			if (val2.z < 0f)
			{
				val2.x = 1f - val2.x;
				val2.y = 1f - val2.y;
				val2.z = 0f;
				float num = 0f;
				num = ((val2.x > num) ? val2.x : num);
				num = ((val2.y > num) ? val2.y : num);
				num = ((val2.z > num) ? val2.z : num);
				val2 /= num;
			}
			if (float.IsPositiveInfinity(val2.x))
			{
				val2.x = 1f;
			}
			else if (float.IsNegativeInfinity(val2.x))
			{
				val2.x = 0f;
			}
			if (float.IsPositiveInfinity(val2.y))
			{
				val2.y = 1f;
			}
			else if (float.IsNegativeInfinity(val2.y))
			{
				val2.y = 0f;
			}
			val2.x = MathF.Clamp(val2.x, 0f, 1f) * Screen.RealScreenResolutionWidth;
			val2.y = MathF.Clamp(val2.y, 0f, 1f) * Screen.RealScreenResolutionHeight;
		}
	}

	public override void OnFinalize()
	{
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		((ViewModel)this).OnFinalize();
		MissionMultiplayerGameModeDuelClient client = _client;
		client.OnMyRepresentativeAssigned = (Action)Delegate.Remove(client.OnMyRepresentativeAssigned, new Action(OnMyRepresentativeAssigned));
		if (_isMyRepresentativeAssigned)
		{
			DuelMissionRepresentative myRepresentative = _client.MyRepresentative;
			myRepresentative.OnDuelPrepStartedEvent = (Action<MissionPeer, int>)Delegate.Remove(myRepresentative.OnDuelPrepStartedEvent, new Action<MissionPeer, int>(OnDuelPrepStarted));
			DuelMissionRepresentative myRepresentative2 = _client.MyRepresentative;
			myRepresentative2.OnAgentSpawnedWithoutDuelEvent = (Action)Delegate.Remove(myRepresentative2.OnAgentSpawnedWithoutDuelEvent, new Action(OnAgentSpawnedWithoutDuel));
			DuelMissionRepresentative myRepresentative3 = _client.MyRepresentative;
			myRepresentative3.OnDuelPreparationStartedForTheFirstTimeEvent = (Action<MissionPeer, MissionPeer, int>)Delegate.Remove(myRepresentative3.OnDuelPreparationStartedForTheFirstTimeEvent, new Action<MissionPeer, MissionPeer, int>(OnDuelStarted));
			DuelMissionRepresentative myRepresentative4 = _client.MyRepresentative;
			myRepresentative4.OnDuelEndedEvent = (Action<MissionPeer>)Delegate.Remove(myRepresentative4.OnDuelEndedEvent, new Action<MissionPeer>(OnDuelEnded));
			DuelMissionRepresentative myRepresentative5 = _client.MyRepresentative;
			myRepresentative5.OnDuelRoundEndedEvent = (Action<MissionPeer>)Delegate.Remove(myRepresentative5.OnDuelRoundEndedEvent, new Action<MissionPeer>(OnDuelRoundEnded));
			DuelMissionRepresentative myRepresentative6 = _client.MyRepresentative;
			myRepresentative6.OnMyPreferredZoneChanged = (Action<TroopType>)Delegate.Remove(myRepresentative6.OnMyPreferredZoneChanged, new Action<TroopType>(OnPlayerPreferredZoneChanged));
			ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
			Markers.UnregisterEvents();
		}
	}

	private void OnManagedOptionChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 34)
		{
			_ongoingDuels.ApplyActionOnAllItems((Action<DuelMatchVM>)delegate(DuelMatchVM d)
			{
				d.RefreshNames(changeGenericNames: true);
			});
		}
	}

	private void OnDuelPrepStarted(MissionPeer opponentPeer, int duelStartTime)
	{
		PlayerDuelMatch.OnDuelPrepStarted(opponentPeer, duelStartTime);
		AreOngoingDuelsActive = false;
		Markers.IsEnabled = false;
	}

	private void OnAgentSpawnedWithoutDuel()
	{
		Markers.OnAgentSpawnedWithoutDuel();
		AreOngoingDuelsActive = true;
	}

	private void OnPlayerPreferredZoneChanged(TroopType zoneType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		int num = (int)zoneType;
		if (num != PlayerPrefferedArenaType)
		{
			PlayerPrefferedArenaType = num;
			Markers.OnPlayerPreferredZoneChanged(num);
			_hasPlayerChangedArenaPreferrence = true;
			GameTexts.SetVariable("ARENA_TYPE", GetArenaTypeLocalizedName(zoneType));
			InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=nLdQvaRK}Arena preference updated to {ARENA_TYPE}.", (Dictionary<string, object>)null)).ToString()));
		}
		else
		{
			InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=YLZV7dxI}This arena type is already the preferred one.", (Dictionary<string, object>)null)).ToString()));
		}
	}

	private void OnDuelStarted(MissionPeer firstPeer, MissionPeer secondPeer, int flagIndex)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected I4, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected I4, but got Unknown
		Markers.OnDuelStarted(firstPeer, secondPeer);
		DuelArenaProperties duelArenaProperties = _duelArenaProperties.First((DuelArenaProperties f) => f.Index == flagIndex);
		if (firstPeer == ((MissionRepresentativeBase)_client.MyRepresentative).MissionPeer || secondPeer == ((MissionRepresentativeBase)_client.MyRepresentative).MissionPeer)
		{
			AreOngoingDuelsActive = false;
			IsPlayerInDuel = true;
			PlayerDuelMatch.OnDuelStarted(firstPeer, secondPeer, (int)duelArenaProperties.ArenaTroopType);
		}
		else
		{
			DuelMatchVM duelMatchVM = new DuelMatchVM();
			duelMatchVM.OnDuelStarted(firstPeer, secondPeer, (int)duelArenaProperties.ArenaTroopType);
			((Collection<DuelMatchVM>)(object)OngoingDuels).Add(duelMatchVM);
		}
	}

	private void OnDuelEnded(MissionPeer winnerPeer)
	{
		if (PlayerDuelMatch.FirstPlayerPeer == winnerPeer || PlayerDuelMatch.SecondPlayerPeer == winnerPeer)
		{
			AreOngoingDuelsActive = true;
			IsPlayerInDuel = false;
			Markers.IsEnabled = true;
			Markers.SetMarkerOfPeerEnabled(PlayerDuelMatch.FirstPlayerPeer, isEnabled: true);
			Markers.SetMarkerOfPeerEnabled(PlayerDuelMatch.SecondPlayerPeer, isEnabled: true);
			PlayerDuelMatch.OnDuelEnded();
			PlayerBounty = _client.MyRepresentative.Bounty;
			UpdatePlayerScore();
		}
		DuelMatchVM duelMatchVM = ((IEnumerable<DuelMatchVM>)OngoingDuels).FirstOrDefault((DuelMatchVM d) => d.FirstPlayerPeer == winnerPeer || d.SecondPlayerPeer == winnerPeer);
		if (duelMatchVM != null)
		{
			Markers.SetMarkerOfPeerEnabled(duelMatchVM.FirstPlayerPeer, isEnabled: true);
			Markers.SetMarkerOfPeerEnabled(duelMatchVM.SecondPlayerPeer, isEnabled: true);
			((Collection<DuelMatchVM>)(object)OngoingDuels).Remove(duelMatchVM);
		}
	}

	private void OnDuelRoundEnded(MissionPeer winnerPeer)
	{
		if (PlayerDuelMatch.FirstPlayerPeer == winnerPeer || PlayerDuelMatch.SecondPlayerPeer == winnerPeer)
		{
			PlayerDuelMatch.OnPeerScored(winnerPeer);
			((Collection<MPDuelKillNotificationItemVM>)(object)KillNotifications).Add(new MPDuelKillNotificationItemVM(PlayerDuelMatch.FirstPlayerPeer, PlayerDuelMatch.SecondPlayerPeer, PlayerDuelMatch.FirstPlayerScore, PlayerDuelMatch.SecondPlayerScore, (TroopType)PlayerDuelMatch.ArenaType, RemoveKillNotification));
			return;
		}
		DuelMatchVM duelMatchVM = ((IEnumerable<DuelMatchVM>)OngoingDuels).FirstOrDefault((DuelMatchVM d) => d.FirstPlayerPeer == winnerPeer || d.SecondPlayerPeer == winnerPeer);
		if (duelMatchVM != null)
		{
			duelMatchVM.OnPeerScored(winnerPeer);
			((Collection<MPDuelKillNotificationItemVM>)(object)KillNotifications).Add(new MPDuelKillNotificationItemVM(duelMatchVM.FirstPlayerPeer, duelMatchVM.SecondPlayerPeer, duelMatchVM.FirstPlayerScore, duelMatchVM.SecondPlayerScore, (TroopType)duelMatchVM.ArenaType, RemoveKillNotification));
		}
	}

	private void UpdatePlayerScore()
	{
		GameTexts.SetVariable("SCORE", _client.MyRepresentative.Score);
		PlayerScoreText = ((object)_scoreWithSeparatorText).ToString();
	}

	private void RemoveKillNotification(MPDuelKillNotificationItemVM item)
	{
		((Collection<MPDuelKillNotificationItemVM>)(object)KillNotifications).Remove(item);
	}

	public void OnScreenResolutionChanged()
	{
		Markers.UpdateScreenCenter();
	}

	public void OnMainAgentRemoved()
	{
		if (!PlayerDuelMatch.IsEnabled)
		{
			Markers.IsEnabled = false;
			AreOngoingDuelsActive = false;
		}
	}

	public void OnMainAgentBuild()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected I4, but got Unknown
		if (!PlayerDuelMatch.IsEnabled)
		{
			Markers.IsEnabled = true;
			AreOngoingDuelsActive = true;
		}
		string stringId = ((MBObjectBase)MultiplayerClassDivisions.GetMPHeroClassForPeer(((MissionRepresentativeBase)_client.MyRepresentative).MissionPeer, false)).StringId;
		if (_isAgentBuiltForTheFirstTime || (stringId != _cachedPlayerClassID && !_hasPlayerChangedArenaPreferrence))
		{
			PlayerPrefferedArenaType = (int)GetAgentDefaultPreferredArenaType(Agent.Main);
			Markers.OnAgentBuiltForTheFirstTime();
			_isAgentBuiltForTheFirstTime = false;
			_cachedPlayerClassID = stringId;
		}
	}

	private string GetArenaTypeName(TroopType duelArenaType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		switch ((int)duelArenaType)
		{
		case 0:
			return "infantry";
		case 1:
			return "archery";
		case 2:
			return "cavalry";
		default:
			Debug.FailedAssert("Invalid duel arena type!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\MultiplayerDuelVM.cs", "GetArenaTypeName", 363);
			return "";
		}
	}

	private TextObject GetArenaTypeLocalizedName(TroopType duelArenaType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		switch ((int)duelArenaType)
		{
		case 0:
			return new TextObject("{=1Bm1Wk1v}Infantry", (Dictionary<string, object>)null);
		case 1:
			return new TextObject("{=OJbpmlXu}Ranged", (Dictionary<string, object>)null);
		case 2:
			return new TextObject("{=YVGtcLHF}Cavalry", (Dictionary<string, object>)null);
		default:
			Debug.FailedAssert("Invalid duel arena type!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\MultiplayerDuelVM.cs", "GetArenaTypeLocalizedName", 379);
			return TextObject.GetEmpty();
		}
	}

	private DuelArenaProperties GetArenaPropertiesOfFlagEntity(GameEntity flagEntity)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Invalid comparison between Unknown and I4
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		DuelArenaProperties result = default(DuelArenaProperties);
		result.FlagEntity = flagEntity;
		string text = flagEntity.Tags.FirstOrDefault((string t) => t.StartsWith("area_flag"));
		if (!Extensions.IsEmpty<char>((IEnumerable<char>)text))
		{
			result.Index = int.Parse(text.Substring(text.LastIndexOf('_') + 1)) - 1;
		}
		else
		{
			result.Index = 0;
			Debug.FailedAssert("Flag has duel_area Tag Missing!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\MultiplayerDuelVM.cs", "GetArenaPropertiesOfFlagEntity", 397);
		}
		result.ArenaTroopType = (TroopType)0;
		for (TroopType val = (TroopType)0; (int)val < 3; val = (TroopType)(val + 1))
		{
			if (flagEntity.HasTag("flag_" + GetArenaTypeName(val)))
			{
				result.ArenaTroopType = val;
			}
		}
		return result;
	}

	public static TroopType GetAgentDefaultPreferredArenaType(Agent agent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return FormationClassExtensions.GetTroopTypeForRegularFormation(agent.Character.DefaultFormationClass);
	}
}
