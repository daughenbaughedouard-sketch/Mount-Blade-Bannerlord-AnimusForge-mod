using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerDuelUI))]
public class MissionGauntletDuelUI : MissionView
{
	private MultiplayerDuelVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private SpriteCategory _mpMissionCategory;

	private MissionMultiplayerGameModeDuelClient _client;

	private MissionLobbyEquipmentNetworkComponent _equipmentController;

	private MissionLobbyComponent _lobbyComponent;

	private bool _isPeerEquipmentsDirty;

	public override void OnMissionScreenInitialize()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		base.ViewOrderPriority = 15;
		_client = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeDuelClient>();
		_dataSource = new MultiplayerDuelVM(((MissionView)this).MissionScreen.CombatCamera, _client);
		_gauntletLayer = new GauntletLayer("MultiplayerDuel", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerDuel", (ViewModel)(object)_dataSource);
		_mpMissionCategory = UIResourceManager.LoadSpriteCategory("ui_mpmission");
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		_equipmentController = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyEquipmentNetworkComponent>();
		_equipmentController.OnEquipmentRefreshed += new OnRefreshEquipmentEventDelegate(OnEquipmentRefreshed);
		MissionPeer.OnEquipmentIndexRefreshed += new OnUpdateEquipmentSetIndexEventDelegate(OnPeerEquipmentIndexRefreshed);
		_lobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_lobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
		NativeOptions.OnNativeOptionChanged = (OnNativeOptionChangedDelegate)Delegate.Combine((Delegate?)(object)NativeOptions.OnNativeOptionChanged, (Delegate?)new OnNativeOptionChangedDelegate(OnNativeOptionChanged));
		_dataSource.IsEnabled = true;
		_isPeerEquipmentsDirty = true;
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		SpriteCategory mpMissionCategory = _mpMissionCategory;
		if (mpMissionCategory != null)
		{
			mpMissionCategory.Unload();
		}
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_gauntletLayer = null;
		_equipmentController.OnEquipmentRefreshed -= new OnRefreshEquipmentEventDelegate(OnEquipmentRefreshed);
		MissionPeer.OnEquipmentIndexRefreshed -= new OnUpdateEquipmentSetIndexEventDelegate(OnPeerEquipmentIndexRefreshed);
		_lobbyComponent.OnPostMatchEnded -= OnPostMatchEnded;
		NativeOptions.OnNativeOptionChanged = (OnNativeOptionChangedDelegate)Delegate.Remove((Delegate?)(object)NativeOptions.OnNativeOptionChanged, (Delegate?)new OnNativeOptionChangedDelegate(OnNativeOptionChanged));
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		_dataSource.Tick(dt);
		DuelMissionRepresentative myRepresentative = _client.MyRepresentative;
		if (((myRepresentative != null) ? ((MissionRepresentativeBase)myRepresentative).ControlledAgent : null) != null && ((MissionView)this).Input.IsGameKeyReleased(13))
		{
			_client.MyRepresentative.OnInteraction();
		}
		if (_isPeerEquipmentsDirty)
		{
			_dataSource.Markers.RefreshPeerEquipments();
			_isPeerEquipmentsDirty = false;
		}
	}

	private void OnNativeOptionChanged(NativeOptionsType optionType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)optionType == 22)
		{
			_dataSource.OnScreenResolutionChanged();
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
		if (affectedAgent == Agent.Main)
		{
			_dataSource.OnMainAgentRemoved();
		}
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		((MissionBehavior)this).OnAgentBuild(agent, banner);
		if (agent == Agent.Main)
		{
			_dataSource.OnMainAgentBuild();
		}
	}

	public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
	{
		((MissionBehavior)this).OnFocusGained(agent, focusableObject, isInteractable);
		if (!(focusableObject is DuelZoneLandmark) && !(focusableObject is Agent))
		{
			_dataSource.Markers.OnFocusGained();
		}
	}

	public override void OnFocusLost(Agent agent, IFocusable focusableObject)
	{
		((MissionBehavior)this).OnFocusLost(agent, focusableObject);
		if (!(focusableObject is DuelZoneLandmark) && !(focusableObject is Agent))
		{
			_dataSource.Markers.OnFocusLost();
		}
	}

	public void OnPeerEquipmentIndexRefreshed(MissionPeer peer, int equipmentSetIndex)
	{
		_dataSource.Markers.OnPeerEquipmentRefreshed(peer);
	}

	private void OnEquipmentRefreshed(MissionPeer peer)
	{
		_dataSource.Markers.OnPeerEquipmentRefreshed(peer);
	}

	private void OnPostMatchEnded()
	{
		_dataSource.IsEnabled = false;
	}
}
