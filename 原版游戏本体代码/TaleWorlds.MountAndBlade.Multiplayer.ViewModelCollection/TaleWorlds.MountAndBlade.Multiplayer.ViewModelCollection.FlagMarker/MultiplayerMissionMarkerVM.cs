using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker;

public class MultiplayerMissionMarkerVM : ViewModel
{
	public class MarkerDistanceComparer : IComparer<MissionMarkerTargetVM>
	{
		public int Compare(MissionMarkerTargetVM x, MissionMarkerTargetVM y)
		{
			return y.Distance.CompareTo(x.Distance);
		}
	}

	private readonly Camera _missionCamera;

	private bool _prevEnabledState;

	private bool _fadeOutTimerStarted;

	private float _fadeOutTimer;

	private MarkerDistanceComparer _distanceComparer;

	private readonly ICommanderInfo _commanderInfo;

	private readonly Dictionary<MissionPeer, MissionPeerMarkerTargetVM> _teammateDictionary;

	private readonly MissionMultiplayerSiegeClient _siegeClient;

	private readonly List<PlayerId> _friendIDs;

	private MBBindingList<MissionFlagMarkerTargetVM> _flagTargets;

	private MBBindingList<MissionPeerMarkerTargetVM> _peerTargets;

	private MBBindingList<MissionSiegeEngineMarkerTargetVM> _siegeEngineTargets;

	private MBBindingList<MissionAlwaysVisibleMarkerTargetVM> _alwaysVisibleTargets;

	private bool _isEnabled;

	[DataSourceProperty]
	public MBBindingList<MissionFlagMarkerTargetVM> FlagTargets
	{
		get
		{
			return _flagTargets;
		}
		set
		{
			if (value != _flagTargets)
			{
				_flagTargets = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionFlagMarkerTargetVM>>(value, "FlagTargets");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionPeerMarkerTargetVM> PeerTargets
	{
		get
		{
			return _peerTargets;
		}
		set
		{
			if (value != _peerTargets)
			{
				_peerTargets = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionPeerMarkerTargetVM>>(value, "PeerTargets");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionSiegeEngineMarkerTargetVM> SiegeEngineTargets
	{
		get
		{
			return _siegeEngineTargets;
		}
		set
		{
			if (value != _siegeEngineTargets)
			{
				_siegeEngineTargets = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionSiegeEngineMarkerTargetVM>>(value, "SiegeEngineTargets");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionAlwaysVisibleMarkerTargetVM> AlwaysVisibleTargets
	{
		get
		{
			return _alwaysVisibleTargets;
		}
		set
		{
			if (value != _alwaysVisibleTargets)
			{
				_alwaysVisibleTargets = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionAlwaysVisibleMarkerTargetVM>>(value, "AlwaysVisibleTargets");
			}
		}
	}

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
				UpdateTargetStates(value);
			}
		}
	}

	public MultiplayerMissionMarkerVM(Camera missionCamera)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		_missionCamera = missionCamera;
		FlagTargets = new MBBindingList<MissionFlagMarkerTargetVM>();
		PeerTargets = new MBBindingList<MissionPeerMarkerTargetVM>();
		SiegeEngineTargets = new MBBindingList<MissionSiegeEngineMarkerTargetVM>();
		AlwaysVisibleTargets = new MBBindingList<MissionAlwaysVisibleMarkerTargetVM>();
		_teammateDictionary = new Dictionary<MissionPeer, MissionPeerMarkerTargetVM>();
		_distanceComparer = new MarkerDistanceComparer();
		_commanderInfo = Mission.Current.GetMissionBehavior<ICommanderInfo>();
		if (_commanderInfo != null)
		{
			_commanderInfo.OnFlagNumberChangedEvent += OnFlagNumberChangedEvent;
			_commanderInfo.OnCapturePointOwnerChangedEvent += OnCapturePointOwnerChangedEvent;
			OnFlagNumberChangedEvent();
			_siegeClient = Mission.Current.GetMissionBehavior<MissionMultiplayerSiegeClient>();
			if (_siegeClient != null)
			{
				_siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent += OnCapturePointRemainingMoraleGainsChanged;
			}
		}
		MissionPeer.OnTeamChanged += new OnTeamChangedDelegate(OnTeamChanged);
		_friendIDs = new List<PlayerId>();
		IFriendListService[] friendListServices = PlatformServices.Instance.GetFriendListServices();
		foreach (IFriendListService val in friendListServices)
		{
			_friendIDs.AddRange(val.GetAllFriends());
		}
	}

	public override void OnFinalize()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		((ViewModel)this).OnFinalize();
		if (_commanderInfo != null)
		{
			_commanderInfo.OnFlagNumberChangedEvent -= OnFlagNumberChangedEvent;
			_commanderInfo.OnCapturePointOwnerChangedEvent -= OnCapturePointOwnerChangedEvent;
			if (_siegeClient != null)
			{
				_siegeClient.OnCapturePointRemainingMoraleGainsChangedEvent -= OnCapturePointRemainingMoraleGainsChanged;
			}
		}
		MissionPeer.OnTeamChanged -= new OnTeamChangedDelegate(OnTeamChanged);
	}

	public void Tick(float dt)
	{
		OnRefreshPeerMarkers();
		UpdateAlwaysVisibleTargetScreenPosition();
		if (IsEnabled)
		{
			UpdateTargetScreenPositions();
			_fadeOutTimerStarted = false;
			_fadeOutTimer = 0f;
			_prevEnabledState = IsEnabled;
		}
		else
		{
			if (_prevEnabledState)
			{
				_fadeOutTimerStarted = true;
			}
			if (_fadeOutTimerStarted)
			{
				_fadeOutTimer += dt;
			}
			if (_fadeOutTimer < 2f)
			{
				UpdateTargetScreenPositions();
			}
			else
			{
				_fadeOutTimerStarted = false;
			}
		}
		_prevEnabledState = IsEnabled;
	}

	private void OnCapturePointRemainingMoraleGainsChanged(int[] remainingMoraleGainsArr)
	{
		foreach (MissionFlagMarkerTargetVM item in (Collection<MissionFlagMarkerTargetVM>)(object)FlagTargets)
		{
			int flagIndex = item.TargetFlag.FlagIndex;
			if (flagIndex >= 0 && flagIndex < remainingMoraleGainsArr.Length)
			{
				item.OnRemainingMoraleChanged(remainingMoraleGainsArr[flagIndex]);
			}
		}
		Debug.Print("OnCapturePointRemainingMoraleGainsChanged: " + remainingMoraleGainsArr.Length, 0, (DebugColor)12, 17592186044416uL);
	}

	private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (_commanderInfo != null)
		{
			OnFlagNumberChangedEvent();
		}
		if (!peer.IsMine)
		{
			return;
		}
		((Collection<MissionSiegeEngineMarkerTargetVM>)(object)SiegeEngineTargets).Clear();
		foreach (WeakGameEntity item in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<SiegeWeapon>())
		{
			WeakGameEntity current = item;
			SiegeWeapon firstScriptOfType = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<SiegeWeapon>();
			if (newTeam.Side == firstScriptOfType.Side)
			{
				((Collection<MissionSiegeEngineMarkerTargetVM>)(object)SiegeEngineTargets).Add(new MissionSiegeEngineMarkerTargetVM(firstScriptOfType));
			}
		}
	}

	private void UpdateTargetScreenPositions()
	{
		PeerTargets.ApplyActionOnAllItems((Action<MissionPeerMarkerTargetVM>)delegate(MissionPeerMarkerTargetVM pt)
		{
			pt.UpdateScreenPosition(_missionCamera);
		});
		FlagTargets.ApplyActionOnAllItems((Action<MissionFlagMarkerTargetVM>)delegate(MissionFlagMarkerTargetVM ft)
		{
			ft.UpdateScreenPosition(_missionCamera);
		});
		SiegeEngineTargets.ApplyActionOnAllItems((Action<MissionSiegeEngineMarkerTargetVM>)delegate(MissionSiegeEngineMarkerTargetVM st)
		{
			st.UpdateScreenPosition(_missionCamera);
		});
		PeerTargets.Sort((IComparer<MissionPeerMarkerTargetVM>)_distanceComparer);
		FlagTargets.Sort((IComparer<MissionFlagMarkerTargetVM>)_distanceComparer);
		SiegeEngineTargets.Sort((IComparer<MissionSiegeEngineMarkerTargetVM>)_distanceComparer);
	}

	private void UpdateAlwaysVisibleTargetScreenPosition()
	{
		foreach (MissionAlwaysVisibleMarkerTargetVM item in (Collection<MissionAlwaysVisibleMarkerTargetVM>)(object)AlwaysVisibleTargets)
		{
			item.UpdateScreenPosition(_missionCamera);
		}
	}

	private void OnFlagNumberChangedEvent()
	{
		ResetCapturePointLists();
		InitCapturePoints();
	}

	private void InitCapturePoints()
	{
		if (_commanderInfo != null)
		{
			FlagCapturePoint[] array = _commanderInfo.AllCapturePoints.Where((FlagCapturePoint c) => !c.IsDeactivated).ToArray();
			foreach (FlagCapturePoint val in array)
			{
				MissionFlagMarkerTargetVM missionFlagMarkerTargetVM = new MissionFlagMarkerTargetVM(val);
				((Collection<MissionFlagMarkerTargetVM>)(object)FlagTargets).Add(missionFlagMarkerTargetVM);
				missionFlagMarkerTargetVM.OnOwnerChanged(_commanderInfo.GetFlagOwner(val));
				missionFlagMarkerTargetVM.IsEnabled = IsEnabled;
			}
		}
	}

	private void ResetCapturePointLists()
	{
		((Collection<MissionFlagMarkerTargetVM>)(object)FlagTargets).Clear();
	}

	private void OnCapturePointOwnerChangedEvent(FlagCapturePoint flag, Team team)
	{
		foreach (MissionFlagMarkerTargetVM item in (Collection<MissionFlagMarkerTargetVM>)(object)FlagTargets)
		{
			if (item.TargetFlag == flag)
			{
				item.OnOwnerChanged(team);
			}
		}
	}

	private void OnRefreshPeerMarkers()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		if (GameNetwork.MyPeer == null)
		{
			return;
		}
		Agent controlledAgent = GameNetwork.MyPeer.ControlledAgent;
		BattleSideEnum val = (BattleSideEnum)((controlledAgent == null) ? (-1) : ((int)controlledAgent.Team.Side));
		List<MissionPeerMarkerTargetVM> list = ((IEnumerable<MissionPeerMarkerTargetVM>)PeerTargets).ToList();
		foreach (MissionPeer missionPeer in VirtualPlayer.Peers<MissionPeer>())
		{
			MissionPeer obj = missionPeer;
			if (((obj != null) ? obj.Team : null) == null || ((PeerComponent)missionPeer).IsMine || missionPeer.Team.Side != val)
			{
				continue;
			}
			IEnumerable<MissionPeerMarkerTargetVM> source = ((IEnumerable<MissionPeerMarkerTargetVM>)PeerTargets).Where(delegate(MissionPeerMarkerTargetVM t)
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				MissionPeer targetPeer2 = t.TargetPeer;
				if (targetPeer2 == null)
				{
					return false;
				}
				PlayerId id = ((PeerComponent)targetPeer2).Peer.Id;
				return ((PlayerId)(ref id)).Equals(((PeerComponent)missionPeer).Peer.Id);
			});
			if (source.Any())
			{
				MissionPeerMarkerTargetVM currentMarker = source.First();
				IEnumerable<MissionAlwaysVisibleMarkerTargetVM> source2 = ((IEnumerable<MissionAlwaysVisibleMarkerTargetVM>)AlwaysVisibleTargets).Where(delegate(MissionAlwaysVisibleMarkerTargetVM t)
				{
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					//IL_0023: Unknown result type (might be due to invalid IL or missing references)
					PlayerId id = ((PeerComponent)t.TargetPeer).Peer.Id;
					return ((PlayerId)(ref id)).Equals(((PeerComponent)currentMarker.TargetPeer).Peer.Id);
				});
				if (BannerlordConfig.EnableDeathIcon && !missionPeer.IsControlledAgentActive)
				{
					if (!source2.Any())
					{
						MissionPeer targetPeer = source.First().TargetPeer;
						if (((targetPeer != null) ? targetPeer.ControlledAgent : null) != null)
						{
							MissionAlwaysVisibleMarkerTargetVM missionAlwaysVisibleMarkerTargetVM = new MissionAlwaysVisibleMarkerTargetVM(currentMarker.TargetPeer, source.First().WorldPosition, OnRemoveAlwaysVisibleMarker);
							missionAlwaysVisibleMarkerTargetVM.UpdateScreenPosition(_missionCamera);
							((Collection<MissionAlwaysVisibleMarkerTargetVM>)(object)AlwaysVisibleTargets).Add(missionAlwaysVisibleMarkerTargetVM);
						}
					}
					continue;
				}
			}
			if (!_teammateDictionary.ContainsKey(missionPeer))
			{
				MissionPeerMarkerTargetVM missionPeerMarkerTargetVM = new MissionPeerMarkerTargetVM(missionPeer, _friendIDs.Contains(((PeerComponent)missionPeer).Peer.Id));
				((Collection<MissionPeerMarkerTargetVM>)(object)PeerTargets).Add(missionPeerMarkerTargetVM);
				_teammateDictionary.Add(missionPeer, missionPeerMarkerTargetVM);
			}
			else
			{
				list.Remove(_teammateDictionary[missionPeer]);
			}
		}
		foreach (MissionPeerMarkerTargetVM item in list)
		{
			MissionPeerMarkerTargetVM current;
			if ((current = item) != null)
			{
				((Collection<MissionPeerMarkerTargetVM>)(object)PeerTargets).Remove(current);
				_teammateDictionary.Remove(current.TargetPeer);
			}
		}
	}

	public void OnRemoveAlwaysVisibleMarker(MissionAlwaysVisibleMarkerTargetVM marker)
	{
		((Collection<MissionAlwaysVisibleMarkerTargetVM>)(object)AlwaysVisibleTargets).Remove(marker);
	}

	private void UpdateTargetStates(bool state)
	{
		PeerTargets.ApplyActionOnAllItems((Action<MissionPeerMarkerTargetVM>)delegate(MissionPeerMarkerTargetVM pt)
		{
			pt.IsEnabled = state;
		});
		FlagTargets.ApplyActionOnAllItems((Action<MissionFlagMarkerTargetVM>)delegate(MissionFlagMarkerTargetVM ft)
		{
			ft.IsEnabled = state;
		});
		SiegeEngineTargets.ApplyActionOnAllItems((Action<MissionSiegeEngineMarkerTargetVM>)delegate(MissionSiegeEngineMarkerTargetVM st)
		{
			st.IsEnabled = state;
		});
	}
}
