using System;
using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionLobbyEquipmentUIHandler))]
public class MissionGauntletClassLoadout : MissionView
{
	private MultiplayerClassLoadoutVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private MissionRepresentativeBase _myRepresentative;

	private MissionNetworkComponent _missionNetworkComponent;

	private MissionLobbyComponent _missionLobbyComponent;

	private MissionLobbyEquipmentNetworkComponent _missionLobbyEquipmentNetworkComponent;

	private MissionMultiplayerGameModeBaseClient _gameModeClient;

	private MultiplayerTeamSelectComponent _teamSelectComponent;

	private MissionGauntletMultiplayerScoreboard _scoreboardGauntletComponent;

	private MPHeroClass _lastSelectedHeroClass;

	private bool _tryToInitialize;

	public bool IsActive { get; private set; }

	public bool IsForceClosed { get; private set; }

	public override void OnMissionScreenInitialize()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		base.ViewOrderPriority = 20;
		_missionLobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_missionLobbyEquipmentNetworkComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyEquipmentNetworkComponent>();
		_gameModeClient = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		_teamSelectComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerTeamSelectComponent>();
		if (_teamSelectComponent != null)
		{
			_teamSelectComponent.OnSelectingTeam += new OnSelectingTeamDelegate(OnSelectingTeam);
		}
		_scoreboardGauntletComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletMultiplayerScoreboard>();
		if (_scoreboardGauntletComponent != null)
		{
			MissionGauntletMultiplayerScoreboard scoreboardGauntletComponent = _scoreboardGauntletComponent;
			scoreboardGauntletComponent.OnScoreboardToggled = (Action<bool>)Delegate.Combine(scoreboardGauntletComponent.OnScoreboardToggled, new Action<bool>(OnScoreboardToggled));
		}
		_missionNetworkComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionNetworkComponent>();
		if (_missionNetworkComponent != null)
		{
			_missionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
		}
		MissionPeer.OnTeamChanged += new OnTeamChangedDelegate(OnTeamChanged);
		_missionLobbyEquipmentNetworkComponent.OnToggleLoadout += new OnToggleLoadoutDelegate(OnTryToggle);
		_missionLobbyEquipmentNetworkComponent.OnEquipmentRefreshed += new OnRefreshEquipmentEventDelegate(OnPeerEquipmentRefreshed);
	}

	private void OnMyClientSynchronized()
	{
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		_myRepresentative = ((myPeer != null) ? myPeer.VirtualPlayer.GetComponent<MissionRepresentativeBase>() : null);
		_myRepresentative.OnGoldUpdated += OnGoldUpdated;
		_missionLobbyComponent.OnClassRestrictionChanged += OnGoldUpdated;
	}

	private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
	{
		if (peer.IsMine && newTeam != null && (newTeam.IsAttacker || newTeam.IsDefender))
		{
			if (IsActive)
			{
				OnTryToggle(isActive: false);
			}
			OnTryToggle(isActive: true);
		}
	}

	private void OnRefreshSelection(MPHeroClass heroClass)
	{
		_lastSelectedHeroClass = heroClass;
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		if (_gauntletLayer != null)
		{
			((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			_gauntletLayer = null;
		}
		if (_dataSource != null)
		{
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
		}
		if (_teamSelectComponent != null)
		{
			_teamSelectComponent.OnSelectingTeam -= new OnSelectingTeamDelegate(OnSelectingTeam);
		}
		if (_scoreboardGauntletComponent != null)
		{
			MissionGauntletMultiplayerScoreboard scoreboardGauntletComponent = _scoreboardGauntletComponent;
			scoreboardGauntletComponent.OnScoreboardToggled = (Action<bool>)Delegate.Remove(scoreboardGauntletComponent.OnScoreboardToggled, new Action<bool>(OnScoreboardToggled));
		}
		if (_missionNetworkComponent != null)
		{
			_missionNetworkComponent.OnMyClientSynchronized -= OnMyClientSynchronized;
			if (_myRepresentative != null)
			{
				_myRepresentative.OnGoldUpdated -= OnGoldUpdated;
				_missionLobbyComponent.OnClassRestrictionChanged -= OnGoldUpdated;
			}
		}
		_missionLobbyEquipmentNetworkComponent.OnToggleLoadout -= new OnToggleLoadoutDelegate(OnTryToggle);
		_missionLobbyEquipmentNetworkComponent.OnEquipmentRefreshed -= new OnRefreshEquipmentEventDelegate(OnPeerEquipmentRefreshed);
		MissionPeer.OnTeamChanged -= new OnTeamChangedDelegate(OnTeamChanged);
		((MissionView)this).OnMissionScreenFinalize();
	}

	private void CreateView()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		MissionMultiplayerGameModeBaseClient missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		_dataSource = new MultiplayerClassLoadoutVM(missionBehavior, OnRefreshSelection, _lastSelectedHeroClass);
		_gauntletLayer = new GauntletLayer("MultiplayerClassLoadout", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerClassLoadout", (ViewModel)(object)_dataSource);
	}

	public void OnTryToggle(bool isActive)
	{
		if (isActive)
		{
			_tryToInitialize = true;
			return;
		}
		IsForceClosed = false;
		OnToggled(isActive: false);
	}

	private bool OnToggled(bool isActive)
	{
		if (IsActive == isActive)
		{
			return true;
		}
		if (!((MissionView)this).MissionScreen.SetDisplayDialog(isActive))
		{
			return false;
		}
		if (isActive)
		{
			CreateView();
			_dataSource.Tick(1f);
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		}
		else
		{
			((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			_gauntletLayer = null;
		}
		IsActive = isActive;
		return true;
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (_tryToInitialize && GameNetwork.IsMyPeerReady && PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).HasSpawnedAgentVisuals && OnToggled(isActive: true))
		{
			_tryToInitialize = false;
		}
		if (IsActive)
		{
			_dataSource.Tick(dt);
			MissionMultiplayerGameModeFlagDominationClient val;
			if (((MissionView)this).Input.IsHotKeyReleased("ForfeitSpawn") && (val = (MissionMultiplayerGameModeFlagDominationClient)/*isinst with value type is only supported in some contexts*/) != null)
			{
				val.OnRequestForfeitSpawn();
			}
		}
	}

	private void OnSelectingTeam(List<Team> disableTeams)
	{
		IsForceClosed = true;
		OnToggled(isActive: false);
	}

	private void OnScoreboardToggled(bool isEnabled)
	{
		if (isEnabled)
		{
			GauntletLayer gauntletLayer = _gauntletLayer;
			if (gauntletLayer != null)
			{
				((ScreenLayer)gauntletLayer).InputRestrictions.ResetInputRestrictions();
			}
		}
		else
		{
			GauntletLayer gauntletLayer2 = _gauntletLayer;
			if (gauntletLayer2 != null)
			{
				((ScreenLayer)gauntletLayer2).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			}
		}
	}

	private void OnPeerEquipmentRefreshed(MissionPeer peer)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if ((int)_gameModeClient.GameType == 5 || (int)_gameModeClient.GameType == 4)
		{
			_dataSource?.OnPeerEquipmentRefreshed(peer);
		}
	}

	private void OnGoldUpdated()
	{
		_dataSource?.OnGoldUpdated();
	}
}
