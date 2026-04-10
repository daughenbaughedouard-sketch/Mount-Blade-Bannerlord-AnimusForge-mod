using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.TeamSelection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MultiplayerTeamSelectUIHandler))]
public class MissionGauntletTeamSelection : MissionView
{
	private GauntletLayer _gauntletLayer;

	private MultiplayerTeamSelectVM _dataSource;

	private MissionNetworkComponent _missionNetworkComponent;

	private MultiplayerTeamSelectComponent _multiplayerTeamSelectComponent;

	private MissionGauntletMultiplayerScoreboard _scoreboardGauntletComponent;

	private MissionGauntletClassLoadout _classLoadoutGauntletComponent;

	private MissionLobbyComponent _lobbyComponent;

	private List<Team> _disabledTeams;

	private bool _toOpen;

	private bool _isSynchronized;

	private bool _isActive;

	public MissionGauntletTeamSelection()
	{
		base.ViewOrderPriority = 22;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_missionNetworkComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionNetworkComponent>();
		_multiplayerTeamSelectComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerTeamSelectComponent>();
		_classLoadoutGauntletComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletClassLoadout>();
		_lobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_missionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
		_lobbyComponent.OnPostMatchEnded += OnClose;
		_multiplayerTeamSelectComponent.OnSelectingTeam += new OnSelectingTeamDelegate(MissionLobbyComponentOnSelectingTeam);
		_multiplayerTeamSelectComponent.OnUpdateTeams += MissionLobbyComponentOnUpdateTeams;
		_multiplayerTeamSelectComponent.OnUpdateFriendsPerTeam += MissionLobbyComponentOnFriendsUpdated;
		_scoreboardGauntletComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletMultiplayerScoreboard>();
		if (_scoreboardGauntletComponent != null)
		{
			MissionGauntletMultiplayerScoreboard scoreboardGauntletComponent = _scoreboardGauntletComponent;
			scoreboardGauntletComponent.OnScoreboardToggled = (Action<bool>)Delegate.Combine(scoreboardGauntletComponent.OnScoreboardToggled, new Action<bool>(OnScoreboardToggled));
		}
		_multiplayerTeamSelectComponent.OnMyTeamChange += OnMyTeamChanged;
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		_missionNetworkComponent.OnMyClientSynchronized -= OnMyClientSynchronized;
		_lobbyComponent.OnPostMatchEnded -= OnClose;
		_multiplayerTeamSelectComponent.OnSelectingTeam -= new OnSelectingTeamDelegate(MissionLobbyComponentOnSelectingTeam);
		_multiplayerTeamSelectComponent.OnUpdateTeams -= MissionLobbyComponentOnUpdateTeams;
		_multiplayerTeamSelectComponent.OnUpdateFriendsPerTeam -= MissionLobbyComponentOnFriendsUpdated;
		_multiplayerTeamSelectComponent.OnMyTeamChange -= OnMyTeamChanged;
		if (_gauntletLayer != null)
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			_gauntletLayer = null;
		}
		if (_dataSource != null)
		{
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
		}
		if (_scoreboardGauntletComponent != null)
		{
			MissionGauntletMultiplayerScoreboard scoreboardGauntletComponent = _scoreboardGauntletComponent;
			scoreboardGauntletComponent.OnScoreboardToggled = (Action<bool>)Delegate.Remove(scoreboardGauntletComponent.OnScoreboardToggled, new Action<bool>(OnScoreboardToggled));
		}
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override bool OnEscape()
	{
		if (_isActive && !_dataSource.IsCancelDisabled)
		{
			OnClose();
			return true;
		}
		return ((MissionView)this).OnEscape();
	}

	private void OnClose()
	{
		if (_isActive)
		{
			_isActive = false;
			_disabledTeams = null;
			((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			((MissionView)this).MissionScreen.SetCameraLockState(false);
			((MissionView)this).MissionScreen.SetDisplayDialog(false);
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			_gauntletLayer = null;
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
			if (_classLoadoutGauntletComponent != null && _classLoadoutGauntletComponent.IsForceClosed)
			{
				_classLoadoutGauntletComponent.OnTryToggle(isActive: true);
			}
		}
	}

	private void OnOpen()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		if (!_isActive)
		{
			_isActive = true;
			string strValue = MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1);
			_dataSource = new MultiplayerTeamSelectVM(((MissionBehavior)this).Mission, OnChangeTeamTo, OnAutoassign, OnClose, (IEnumerable<Team>)((MissionBehavior)this).Mission.Teams, strValue);
			_dataSource.RefreshDisabledTeams(_disabledTeams);
			_gauntletLayer = new GauntletLayer("MultiplayerTeamSelection", base.ViewOrderPriority, false);
			_gauntletLayer.LoadMovie("MultiplayerTeamSelection", (ViewModel)(object)_dataSource);
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)3);
			((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
			((MissionView)this).MissionScreen.SetCameraLockState(true);
			MissionLobbyComponentOnUpdateTeams();
			MissionLobbyComponentOnFriendsUpdated();
		}
	}

	private void OnChangeTeamTo(Team targetTeam)
	{
		_multiplayerTeamSelectComponent.ChangeTeam(targetTeam);
	}

	private void OnMyTeamChanged()
	{
		OnClose();
	}

	private void OnAutoassign()
	{
		_multiplayerTeamSelectComponent.AutoAssignTeam(GameNetwork.MyPeer);
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (_isSynchronized && _toOpen && ((MissionView)this).MissionScreen.SetDisplayDialog(true))
		{
			_toOpen = false;
			OnOpen();
		}
		_dataSource?.Tick(dt);
	}

	private void MissionLobbyComponentOnSelectingTeam(List<Team> disabledTeams)
	{
		_disabledTeams = disabledTeams;
		_toOpen = true;
	}

	private void MissionLobbyComponentOnFriendsUpdated()
	{
		if (_isActive)
		{
			IEnumerable<MissionPeer> friendsTeamOne = from x in _multiplayerTeamSelectComponent.GetFriendsForTeam(((MissionBehavior)this).Mission.AttackerTeam)
				select x.GetComponent<MissionPeer>();
			IEnumerable<MissionPeer> friendsTeamTwo = from x in _multiplayerTeamSelectComponent.GetFriendsForTeam(((MissionBehavior)this).Mission.DefenderTeam)
				select x.GetComponent<MissionPeer>();
			_dataSource.RefreshFriendsPerTeam(friendsTeamOne, friendsTeamTwo);
		}
	}

	private void MissionLobbyComponentOnUpdateTeams()
	{
		if (_isActive)
		{
			List<Team> disabledTeams = _multiplayerTeamSelectComponent.GetDisabledTeams();
			_dataSource.RefreshDisabledTeams(disabledTeams);
			int playerCountForTeam = _multiplayerTeamSelectComponent.GetPlayerCountForTeam(((MissionBehavior)this).Mission.AttackerTeam);
			int playerCountForTeam2 = _multiplayerTeamSelectComponent.GetPlayerCountForTeam(((MissionBehavior)this).Mission.DefenderTeam);
			int intValue = MultiplayerOptionsExtensions.GetIntValue((OptionType)18, (MultiplayerOptionsAccessMode)1);
			int intValue2 = MultiplayerOptionsExtensions.GetIntValue((OptionType)19, (MultiplayerOptionsAccessMode)1);
			_dataSource.RefreshPlayerAndBotCount(playerCountForTeam, playerCountForTeam2, intValue, intValue2);
		}
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

	private void OnMyClientSynchronized()
	{
		_isSynchronized = true;
	}
}
