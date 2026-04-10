using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MissionDuelMarkersVM : ViewModel
{
	private class PeerMarkerDistanceComparer : IComparer<MissionDuelPeerMarkerVM>
	{
		public int Compare(MissionDuelPeerMarkerVM x, MissionDuelPeerMarkerVM y)
		{
			return y.Distance.CompareTo(x.Distance);
		}
	}

	private const string ZoneLandmarkTag = "duel_zone_landmark";

	private const float FocusScreenDistanceThreshold = 350f;

	private const float LandmarkFocusDistanceThrehsold = 500f;

	private bool _hasEnteredLobby;

	private Camera _missionCamera;

	private MissionDuelPeerMarkerVM _previousFocusTarget;

	private MissionDuelPeerMarkerVM _currentFocusTarget;

	private MissionDuelLandmarkMarkerVM _previousLandmarkTarget;

	private MissionDuelLandmarkMarkerVM _currentLandmarkTarget;

	private PeerMarkerDistanceComparer _distanceComparer;

	private readonly Dictionary<MissionPeer, MissionDuelPeerMarkerVM> _targetPeersToMarkersDictionary;

	private readonly MissionMultiplayerGameModeDuelClient _client;

	private Vec2 _screenCenter;

	private Dictionary<MissionPeer, bool> _targetPeersInDuelDictionary;

	private int _playerPreferredArenaType;

	private bool _isPlayerFocused;

	private bool _isEnabled;

	private MBBindingList<MissionDuelPeerMarkerVM> _targets;

	private MBBindingList<MissionDuelLandmarkMarkerVM> _landmarks;

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
				UpdateTargetsEnabled(value);
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionDuelPeerMarkerVM> Targets
	{
		get
		{
			return _targets;
		}
		set
		{
			if (value != _targets)
			{
				_targets = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionDuelPeerMarkerVM>>(value, "Targets");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionDuelLandmarkMarkerVM> Landmarks
	{
		get
		{
			return _landmarks;
		}
		set
		{
			if (value != _landmarks)
			{
				_landmarks = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionDuelLandmarkMarkerVM>>(value, "Landmarks");
			}
		}
	}

	public MissionDuelMarkersVM(Camera missionCamera, MissionMultiplayerGameModeDuelClient client)
	{
		_missionCamera = missionCamera;
		_client = client;
		List<GameEntity> list = new List<GameEntity>();
		list.AddRange(Mission.Current.Scene.FindEntitiesWithTag("duel_zone_landmark"));
		Landmarks = new MBBindingList<MissionDuelLandmarkMarkerVM>();
		foreach (GameEntity item in list)
		{
			((Collection<MissionDuelLandmarkMarkerVM>)(object)Landmarks).Add(new MissionDuelLandmarkMarkerVM(item));
		}
		Targets = new MBBindingList<MissionDuelPeerMarkerVM>();
		_targetPeersToMarkersDictionary = new Dictionary<MissionPeer, MissionDuelPeerMarkerVM>();
		_targetPeersInDuelDictionary = new Dictionary<MissionPeer, bool>();
		_distanceComparer = new PeerMarkerDistanceComparer();
		UpdateScreenCenter();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Targets.ApplyActionOnAllItems((Action<MissionDuelPeerMarkerVM>)delegate(MissionDuelPeerMarkerVM t)
		{
			((ViewModel)t).RefreshValues();
		});
		Landmarks.ApplyActionOnAllItems((Action<MissionDuelLandmarkMarkerVM>)delegate(MissionDuelLandmarkMarkerVM l)
		{
			((ViewModel)l).RefreshValues();
		});
	}

	public void UpdateScreenCenter()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_screenCenter = new Vec2(Screen.RealScreenResolutionWidth / 2f, Screen.RealScreenResolutionHeight / 2f);
	}

	public void Tick(float dt)
	{
		if (_hasEnteredLobby && GameNetwork.MyPeer != null)
		{
			OnRefreshPeerMarkers();
			UpdateTargets(dt);
		}
	}

	public void RegisterEvents()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		DuelMissionRepresentative myRepresentative = _client.MyRepresentative;
		myRepresentative.OnDuelRequestSentEvent = (Action<MissionPeer>)Delegate.Combine(myRepresentative.OnDuelRequestSentEvent, new Action<MissionPeer>(OnDuelRequestSent));
		DuelMissionRepresentative myRepresentative2 = _client.MyRepresentative;
		myRepresentative2.OnDuelRequestedEvent = (Action<MissionPeer, TroopType>)Delegate.Combine(myRepresentative2.OnDuelRequestedEvent, new Action<MissionPeer, TroopType>(OnDuelRequested));
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionsChanged));
	}

	public void UnregisterEvents()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		DuelMissionRepresentative myRepresentative = _client.MyRepresentative;
		myRepresentative.OnDuelRequestSentEvent = (Action<MissionPeer>)Delegate.Remove(myRepresentative.OnDuelRequestSentEvent, new Action<MissionPeer>(OnDuelRequestSent));
		DuelMissionRepresentative myRepresentative2 = _client.MyRepresentative;
		myRepresentative2.OnDuelRequestedEvent = (Action<MissionPeer, TroopType>)Delegate.Remove(myRepresentative2.OnDuelRequestedEvent, new Action<MissionPeer, TroopType>(OnDuelRequested));
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionsChanged));
	}

	private void OnManagedOptionsChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 34)
		{
			Targets.ApplyActionOnAllItems((Action<MissionDuelPeerMarkerVM>)delegate(MissionDuelPeerMarkerVM t)
			{
				((ViewModel)t).RefreshValues();
			});
		}
	}

	private void UpdateTargets(float dt)
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		if (_currentFocusTarget != null)
		{
			_previousFocusTarget = _currentFocusTarget;
			_currentFocusTarget = null;
			if (_isPlayerFocused)
			{
				_previousFocusTarget.IsFocused = false;
			}
		}
		if (_currentLandmarkTarget != null)
		{
			_previousLandmarkTarget = _currentLandmarkTarget;
			_currentLandmarkTarget = null;
			if (_isPlayerFocused)
			{
				_previousLandmarkTarget.IsFocused = false;
			}
		}
		DuelMissionRepresentative myRepresentative = _client.MyRepresentative;
		if (((myRepresentative != null) ? ((MissionRepresentativeBase)myRepresentative).MissionPeer.ControlledAgent : null) == null)
		{
			return;
		}
		float num = float.MaxValue;
		foreach (MissionDuelPeerMarkerVM item in (Collection<MissionDuelPeerMarkerVM>)(object)Targets)
		{
			item.OnTick(dt);
			if (item.IsEnabled)
			{
				if (!item.HasSentDuelRequest && !item.HasDuelRequestForPlayer && item.TargetPeer.ControlledAgent != null)
				{
					item.PreferredArenaType = _playerPreferredArenaType;
				}
				item.UpdateScreenPosition(_missionCamera);
				item.HasDuelRequestForPlayer = _client.MyRepresentative.CheckHasRequestFromAndRemoveRequestIfNeeded(item.TargetPeer);
				Vec2 screenPosition = item.ScreenPosition;
				float num2 = ((Vec2)(ref screenPosition)).Distance(_screenCenter);
				if (!_isPlayerFocused && item.WSign >= 0 && num2 < 350f && num2 < num)
				{
					num = num2;
					_currentFocusTarget = item;
				}
			}
		}
		Targets.Sort((IComparer<MissionDuelPeerMarkerVM>)_distanceComparer);
		if (_client.MyRepresentative == null)
		{
			return;
		}
		if (_currentFocusTarget != null && _currentFocusTarget.TargetPeer.ControlledAgent != null)
		{
			_client.MyRepresentative.OnObjectFocused((IFocusable)(object)_currentFocusTarget.TargetPeer.ControlledAgent);
			if (_previousFocusTarget != null && _currentFocusTarget.TargetPeer != _previousFocusTarget.TargetPeer)
			{
				_previousFocusTarget.IsFocused = false;
			}
			_currentFocusTarget.IsFocused = true;
			if (_previousLandmarkTarget != null)
			{
				_previousLandmarkTarget.IsFocused = false;
			}
			return;
		}
		if (_previousFocusTarget != null)
		{
			_previousFocusTarget.IsFocused = false;
		}
		foreach (MissionDuelLandmarkMarkerVM item2 in (Collection<MissionDuelLandmarkMarkerVM>)(object)Landmarks)
		{
			if (Agent.Main == null)
			{
				continue;
			}
			item2.UpdateScreenPosition(_missionCamera);
			if (_isPlayerFocused || !item2.IsInScreenBoundaries)
			{
				continue;
			}
			WorldPosition worldPosition = Agent.Main.GetWorldPosition();
			Vec3 groundVec = ((WorldPosition)(ref worldPosition)).GetGroundVec3();
			if (!(((Vec3)(ref groundVec)).DistanceSquared(item2.Entity.GlobalPosition) < 500f))
			{
				continue;
			}
			item2.IsFocused = true;
			_currentLandmarkTarget = item2;
			if (_previousLandmarkTarget != item2)
			{
				if (_previousLandmarkTarget != null)
				{
					_previousLandmarkTarget.IsFocused = false;
				}
				_currentLandmarkTarget.IsFocused = true;
			}
			_client.MyRepresentative.OnObjectFocused(item2.FocusableComponent);
			break;
		}
		if (_currentLandmarkTarget == null && _previousLandmarkTarget != null)
		{
			_previousLandmarkTarget.IsFocused = false;
		}
		if (_currentFocusTarget == null && _currentLandmarkTarget == null)
		{
			_client.MyRepresentative.OnObjectFocusLost();
		}
	}

	public void RefreshPeerEquipments()
	{
		foreach (MissionPeer item in VirtualPlayer.Peers<MissionPeer>())
		{
			OnPeerEquipmentRefreshed(item);
		}
	}

	private void OnRefreshPeerMarkers()
	{
		List<MissionDuelPeerMarkerVM> list = ((IEnumerable<MissionDuelPeerMarkerVM>)Targets).ToList();
		foreach (MissionPeer item in VirtualPlayer.Peers<MissionPeer>())
		{
			if (((item != null) ? item.Team : null) == null || !item.IsControlledAgentActive || ((PeerComponent)item).IsMine)
			{
				continue;
			}
			if (!_targetPeersToMarkersDictionary.ContainsKey(item))
			{
				MissionDuelPeerMarkerVM missionDuelPeerMarkerVM = new MissionDuelPeerMarkerVM(item);
				((Collection<MissionDuelPeerMarkerVM>)(object)Targets).Add(missionDuelPeerMarkerVM);
				_targetPeersToMarkersDictionary.Add(item, missionDuelPeerMarkerVM);
				OnPeerEquipmentRefreshed(item);
				if (_targetPeersInDuelDictionary.ContainsKey(item))
				{
					missionDuelPeerMarkerVM.UpdateCurentDuelStatus(_targetPeersInDuelDictionary[item]);
				}
			}
			else
			{
				list.Remove(_targetPeersToMarkersDictionary[item]);
			}
			if (!_targetPeersInDuelDictionary.ContainsKey(item))
			{
				_targetPeersInDuelDictionary.Add(item, value: false);
			}
		}
		foreach (MissionDuelPeerMarkerVM item2 in list)
		{
			((Collection<MissionDuelPeerMarkerVM>)(object)Targets).Remove(item2);
			_targetPeersToMarkersDictionary.Remove(item2.TargetPeer);
		}
	}

	private void UpdateTargetsEnabled(bool isEnabled)
	{
		foreach (MissionDuelPeerMarkerVM item in (Collection<MissionDuelPeerMarkerVM>)(object)Targets)
		{
			item.IsEnabled = !item.IsInDuel && isEnabled;
		}
	}

	private void OnDuelRequestSent(MissionPeer targetPeer)
	{
		foreach (MissionDuelPeerMarkerVM item in (Collection<MissionDuelPeerMarkerVM>)(object)Targets)
		{
			if (item.TargetPeer == targetPeer)
			{
				item.HasSentDuelRequest = true;
			}
		}
	}

	private void OnDuelRequested(MissionPeer targetPeer, TroopType troopType)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected I4, but got Unknown
		MissionDuelPeerMarkerVM missionDuelPeerMarkerVM = ((IEnumerable<MissionDuelPeerMarkerVM>)Targets).FirstOrDefault((MissionDuelPeerMarkerVM t) => t.TargetPeer == targetPeer);
		if (missionDuelPeerMarkerVM != null)
		{
			missionDuelPeerMarkerVM.HasDuelRequestForPlayer = true;
			missionDuelPeerMarkerVM.PreferredArenaType = (int)troopType;
		}
	}

	public void OnAgentSpawnedWithoutDuel()
	{
		_hasEnteredLobby = true;
		IsEnabled = true;
	}

	public void OnAgentBuiltForTheFirstTime()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected I4, but got Unknown
		_playerPreferredArenaType = (int)MultiplayerDuelVM.GetAgentDefaultPreferredArenaType(Agent.Main);
	}

	public void OnDuelStarted(MissionPeer firstPeer, MissionPeer secondPeer)
	{
		if (((MissionRepresentativeBase)_client.MyRepresentative).MissionPeer == firstPeer || ((MissionRepresentativeBase)_client.MyRepresentative).MissionPeer == secondPeer)
		{
			IsEnabled = false;
		}
		foreach (MissionDuelPeerMarkerVM item in (Collection<MissionDuelPeerMarkerVM>)(object)Targets)
		{
			if (item.TargetPeer == firstPeer || item.TargetPeer == secondPeer)
			{
				item.OnDuelStarted();
			}
		}
		_targetPeersInDuelDictionary[firstPeer] = true;
		_targetPeersInDuelDictionary[secondPeer] = true;
	}

	public void SetMarkerOfPeerEnabled(MissionPeer peer, bool isEnabled)
	{
		if (peer != null)
		{
			if (_targetPeersToMarkersDictionary.ContainsKey(peer))
			{
				_targetPeersToMarkersDictionary[peer].UpdateCurentDuelStatus(!isEnabled);
				_targetPeersToMarkersDictionary[peer].UpdateBounty();
			}
			if (_targetPeersInDuelDictionary.ContainsKey(peer))
			{
				_targetPeersInDuelDictionary[peer] = !isEnabled;
			}
		}
	}

	public void OnPlayerPreferredZoneChanged(int playerPrefferedArenaType)
	{
		_playerPreferredArenaType = playerPrefferedArenaType;
	}

	public void OnFocusGained()
	{
		_isPlayerFocused = true;
	}

	public void OnFocusLost()
	{
		_isPlayerFocused = false;
	}

	public void OnPeerEquipmentRefreshed(MissionPeer peer)
	{
		if (_targetPeersToMarkersDictionary.TryGetValue(peer, out var value))
		{
			value.RefreshPerkSelection();
		}
	}
}
