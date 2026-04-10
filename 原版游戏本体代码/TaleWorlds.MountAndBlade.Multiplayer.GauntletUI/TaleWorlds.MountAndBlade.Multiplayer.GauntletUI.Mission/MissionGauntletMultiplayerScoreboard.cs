using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionScoreboardUIHandler))]
public class MissionGauntletMultiplayerScoreboard : MissionView
{
	private GauntletLayer _gauntletLayer;

	private MissionScoreboardVM _dataSource;

	private bool _isSingleTeam;

	private bool _isActive;

	private bool _isMissionEnding;

	private bool _mouseRequstedWhileScoreboardActive;

	private bool _isMouseVisible;

	private MissionLobbyComponent _missionLobbyComponent;

	private MultiplayerTeamSelectComponent _teamSelectComponent;

	public Action<bool> OnScoreboardToggled;

	private float _scoreboardStayDuration;

	private float _scoreboardStayTimeElapsed;

	[UsedImplicitly]
	public MissionGauntletMultiplayerScoreboard(bool isSingleTeam)
	{
		_isSingleTeam = isSingleTeam;
		base.ViewOrderPriority = 25;
	}

	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		InitializeLayer();
		((MissionBehavior)this).Mission.IsFriendlyMission = false;
		GameKeyContext category = HotKeyManager.GetCategory("ScoreboardHotKeyCategory");
		if (!((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsCategoryRegistered(category))
		{
			((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(category);
		}
		_missionLobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_scoreboardStayDuration = MissionLobbyComponent.PostMatchWaitDuration / 2f;
		_teamSelectComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerTeamSelectComponent>();
		RegisterEvents();
		if (_dataSource != null)
		{
			_dataSource.IsActive = false;
		}
	}

	public override void OnRemoveBehavior()
	{
		UnregisterEvents();
		FinalizeLayer();
		((MissionView)this).OnRemoveBehavior();
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		UnregisterEvents();
		FinalizeLayer();
		((MissionView)this).OnMissionScreenFinalize();
	}

	private void RegisterEvents()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		if (((MissionView)this).MissionScreen != null)
		{
			((MissionView)this).MissionScreen.OnSpectateAgentFocusIn += new OnSpectateAgentDelegate(HandleSpectateAgentFocusIn);
			((MissionView)this).MissionScreen.OnSpectateAgentFocusOut += new OnSpectateAgentDelegate(HandleSpectateAgentFocusOut);
		}
		_missionLobbyComponent.CurrentMultiplayerStateChanged += MissionLobbyComponentOnCurrentMultiplayerStateChanged;
		_missionLobbyComponent.OnCultureSelectionRequested += OnCultureSelectionRequested;
		if (_teamSelectComponent != null)
		{
			_teamSelectComponent.OnSelectingTeam += new OnSelectingTeamDelegate(OnSelectingTeam);
		}
		MissionPeer.OnTeamChanged += new OnTeamChangedDelegate(OnTeamChanged);
	}

	private void UnregisterEvents()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		if (((MissionView)this).MissionScreen != null)
		{
			((MissionView)this).MissionScreen.OnSpectateAgentFocusIn -= new OnSpectateAgentDelegate(HandleSpectateAgentFocusIn);
			((MissionView)this).MissionScreen.OnSpectateAgentFocusOut -= new OnSpectateAgentDelegate(HandleSpectateAgentFocusOut);
		}
		_missionLobbyComponent.CurrentMultiplayerStateChanged -= MissionLobbyComponentOnCurrentMultiplayerStateChanged;
		_missionLobbyComponent.OnCultureSelectionRequested -= OnCultureSelectionRequested;
		if (_teamSelectComponent != null)
		{
			_teamSelectComponent.OnSelectingTeam -= new OnSelectingTeamDelegate(OnSelectingTeam);
		}
		MissionPeer.OnTeamChanged -= new OnTeamChangedDelegate(OnTeamChanged);
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (_isMissionEnding)
		{
			if (_scoreboardStayTimeElapsed >= _scoreboardStayDuration)
			{
				ToggleScoreboard(isActive: false);
				return;
			}
			_scoreboardStayTimeElapsed += dt;
		}
		_dataSource.Tick(dt);
		if (Input.IsGamepadActive)
		{
			bool flag = ((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsGameKeyPressed(4) || ((ScreenLayer)_gauntletLayer).Input.IsGameKeyPressed(4);
			if (_isMissionEnding)
			{
				ToggleScoreboard(isActive: true);
			}
			else if (flag && !((MissionView)this).MissionScreen.IsRadialMenuActive && !((MissionBehavior)this).Mission.IsOrderMenuOpen)
			{
				ToggleScoreboard(!_dataSource.IsActive);
			}
		}
		else
		{
			bool flag2 = ((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsHotKeyDown("HoldShow") || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyDown("HoldShow");
			bool isActive = _isMissionEnding || (flag2 && !((MissionView)this).MissionScreen.IsRadialMenuActive && !((MissionBehavior)this).Mission.IsOrderMenuOpen);
			ToggleScoreboard(isActive);
		}
		if (_isActive && (((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsGameKeyPressed(35) || ((ScreenLayer)_gauntletLayer).Input.IsGameKeyPressed(35)))
		{
			_mouseRequstedWhileScoreboardActive = true;
		}
		bool mouseState = _isMissionEnding || (_isActive && _mouseRequstedWhileScoreboardActive);
		SetMouseState(mouseState);
	}

	private void ToggleScoreboard(bool isActive)
	{
		if (_isActive != isActive)
		{
			_isActive = isActive;
			_dataSource.IsActive = _isActive;
			((MissionView)this).MissionScreen.SetCameraLockState(_isActive);
			if (!_isActive)
			{
				_mouseRequstedWhileScoreboardActive = false;
			}
			OnScoreboardToggled?.Invoke(_isActive);
		}
	}

	private void SetMouseState(bool isMouseVisible)
	{
		if (_isMouseVisible != isMouseVisible)
		{
			_isMouseVisible = isMouseVisible;
			if (!_isMouseVisible)
			{
				((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			}
			else
			{
				((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(_isMouseVisible, (InputUsageMask)3);
			}
			_dataSource?.SetMouseState(isMouseVisible);
		}
	}

	private void HandleSpectateAgentFocusOut(Agent followedAgent)
	{
		if (followedAgent.MissionPeer != null)
		{
			MissionPeer component = ((PeerComponent)followedAgent.MissionPeer).GetComponent<MissionPeer>();
			_dataSource.DecreaseSpectatorCount(component);
		}
	}

	private void HandleSpectateAgentFocusIn(Agent followedAgent)
	{
		if (followedAgent.MissionPeer != null)
		{
			MissionPeer component = ((PeerComponent)followedAgent.MissionPeer).GetComponent<MissionPeer>();
			_dataSource.IncreaseSpectatorCount(component);
		}
	}

	private void MissionLobbyComponentOnCurrentMultiplayerStateChanged(MultiplayerGameState newState)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		_isMissionEnding = (int)newState == 2;
	}

	private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
	{
		if (peer.IsMine)
		{
			FinalizeLayer();
			InitializeLayer();
		}
	}

	private void FinalizeLayer()
	{
		if (_dataSource != null)
		{
			((ViewModel)_dataSource).OnFinalize();
		}
		if (_gauntletLayer != null)
		{
			((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		}
		_gauntletLayer = null;
		_dataSource = null;
		_isActive = false;
	}

	private void InitializeLayer()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		_dataSource = new MissionScoreboardVM(_isSingleTeam, ((MissionBehavior)this).Mission);
		_gauntletLayer = new GauntletLayer("MultiplayerScoreboard", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerScoreboard", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ScoreboardHotKeyCategory"));
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		_dataSource.IsActive = _isActive;
	}

	private void OnSelectingTeam(List<Team> disableTeams)
	{
		ToggleScoreboard(isActive: false);
	}

	private void OnCultureSelectionRequested()
	{
		ToggleScoreboard(isActive: false);
	}
}
