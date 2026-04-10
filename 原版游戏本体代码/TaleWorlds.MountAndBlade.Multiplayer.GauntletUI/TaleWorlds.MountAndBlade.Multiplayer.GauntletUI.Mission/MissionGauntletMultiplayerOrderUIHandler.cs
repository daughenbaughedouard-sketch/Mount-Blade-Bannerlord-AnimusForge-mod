using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MultiplayerMissionOrderUIHandler))]
public class MissionGauntletMultiplayerOrderUIHandler : GauntletOrderUIHandler
{
	private IRoundComponent _roundComponent;

	private bool _isValid;

	private bool _shouldTick;

	private bool _shouldInitializeFormationInfo;

	public override bool IsDeployment => false;

	public override bool IsSiegeDeployment => false;

	public override bool IsValidForTick
	{
		get
		{
			if (_shouldTick && (!((MissionView)this).MissionScreen.IsRadialMenuActive || base._dataSource.IsToggleOrderShown))
			{
				return !GameStateManager.Current.ActiveStateDisabledByUser;
			}
			return false;
		}
	}

	public MissionGauntletMultiplayerOrderUIHandler()
	{
		((MissionView)this).ViewOrderPriority = 19;
	}

	public override bool IsReady()
	{
		return true;
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		int num = default(int);
		MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)20, (MultiplayerOptionsAccessMode)1).GetValue(ref num);
		_shouldTick = num > 0;
	}

	public override void OnMissionScreenTick(float dt)
	{
		if (((GauntletOrderUIHandler)this).IsValidForTick)
		{
			if (!base._isInitialized)
			{
				Team val = (GameNetwork.IsMyPeerReady ? PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).Team : null);
				if (val != null && (val == ((MissionBehavior)this).Mission.AttackerTeam || val == ((MissionBehavior)this).Mission.DefenderTeam))
				{
					InitializeInADisgustingManner();
				}
			}
			if (!_isValid)
			{
				Team val2 = (GameNetwork.IsMyPeerReady ? PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).Team : null);
				if (val2 != null && (val2 == ((MissionBehavior)this).Mission.AttackerTeam || val2 == ((MissionBehavior)this).Mission.DefenderTeam))
				{
					ValidateInADisgustingManner();
				}
				return;
			}
			if (_shouldInitializeFormationInfo)
			{
				Team val3 = (GameNetwork.IsMyPeerReady ? PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).Team : null);
				if (base._dataSource != null && val3 != null)
				{
					base._dataSource.AfterInitialize();
					_shouldInitializeFormationInfo = false;
				}
			}
		}
		((GauntletOrderUIHandler)this).OnMissionScreenTick(dt);
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("MissionOrderHotkeyCategory"));
		base._siegeDeploymentHandler = null;
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		MissionMultiplayerGameModeBaseClient missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		_roundComponent = ((missionBehavior != null) ? missionBehavior.RoundComponent : null);
		if (_roundComponent != null)
		{
			_roundComponent.OnRoundStarted += OnRoundStarted;
			_roundComponent.OnPreparationEnded += OnPreparationEnded;
		}
	}

	private void OnRoundStarted()
	{
		MissionOrderVM dataSource = base._dataSource;
		if (dataSource != null)
		{
			dataSource.AfterInitialize();
		}
	}

	private void OnPreparationEnded()
	{
		_shouldInitializeFormationInfo = true;
	}

	private void OnManagedOptionChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 35)
		{
			if (base._gauntletLayer != null && base._movie != null)
			{
				base._gauntletLayer.ReleaseMovie(base._movie);
				string text = ((BannerlordConfig.OrderType == 0) ? base._barOrderMovieName : base._radialOrderMovieName);
				base._movie = base._gauntletLayer.LoadMovie(text, (ViewModel)(object)base._dataSource);
			}
		}
		else if ((int)changedManagedOptionsType == 36)
		{
			MissionOrderVM dataSource = base._dataSource;
			if (dataSource != null)
			{
				dataSource.OnOrderLayoutTypeChanged();
			}
		}
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		Clear();
		base._orderTroopPlacer = null;
		MissionPeer.OnTeamChanged -= new OnTeamChangedDelegate(TeamChange);
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		if (_roundComponent != null)
		{
			_roundComponent.OnRoundStarted -= OnRoundStarted;
			_roundComponent.OnPreparationEnded -= OnPreparationEnded;
		}
		((MissionView)this).OnMissionScreenFinalize();
	}

	protected override void OnTransferFinished()
	{
	}

	protected override void SetLayerEnabled(bool isEnabled)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (isEnabled)
		{
			if (base._dataSource == null || base._dataSource.ActiveTargetState == 0)
			{
				base._orderTroopPlacer.SuspendTroopPlacer = false;
			}
			((MissionView)this).MissionScreen.SetOrderFlagVisibility(true);
			Game.Current.EventManager.TriggerEvent<MissionPlayerToggledOrderViewEvent>(new MissionPlayerToggledOrderViewEvent(true));
		}
		else
		{
			base._orderTroopPlacer.SuspendTroopPlacer = true;
			((MissionView)this).MissionScreen.SetOrderFlagVisibility(false);
			((MissionView)this).MissionScreen.UnregisterRadialMenuObject((object)this);
			Game.Current.EventManager.TriggerEvent<MissionPlayerToggledOrderViewEvent>(new MissionPlayerToggledOrderViewEvent(false));
		}
	}

	public void InitializeInADisgustingManner()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		((MissionBehavior)this).AfterStart();
		base._orderTroopPlacer = ((MissionBehavior)this).Mission.GetMissionBehavior<OrderTroopPlacer>();
		((MissionView)this).MissionScreen.OrderFlag = base._orderTroopPlacer.OrderFlag;
		((MissionView)this).MissionScreen.SetOrderFlagVisibility(false);
		MissionPeer.OnTeamChanged += new OnTeamChangedDelegate(TeamChange);
		base._isInitialized = true;
	}

	public void ValidateInADisgustingManner()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Expected O, but got Unknown
		base._dataSource = new MissionOrderVM(((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController, false, true);
		base._dataSource.SetDeploymentParemeters(((MissionView)this).MissionScreen.CombatCamera, ((GauntletOrderUIHandler)this).IsSiegeDeployment ? base._siegeDeploymentHandler.PlayerDeploymentPoints.ToList() : new List<DeploymentPoint>());
		MissionOrderVM dataSource = base._dataSource;
		MissionOrderCallbacks callbacks = new MissionOrderCallbacks
		{
			ToggleMissionInputs = base.ToggleScreenRotation,
			RefreshVisuals = new OnRefreshVisualsDelegate(RefreshVisuals),
			GetVisualOrderExecutionParameters = new GetOrderExecutionParametersDelegate(base.GetVisualOrderExecutionParameters)
		};
		callbacks.SetSuspendTroopPlacer = new ToggleOrderPositionVisibilityDelegate(((GauntletOrderUIHandler)this).SetSuspendTroopPlacer);
		callbacks.OnActivateToggleOrder = new OnToggleActivateOrderStateDelegate(base.OnActivateToggleOrder);
		callbacks.OnDeactivateToggleOrder = new OnToggleActivateOrderStateDelegate(base.OnDeactivateToggleOrder);
		callbacks.OnTransferTroopsFinished = new OnTransferTroopsFinishedDelegate(((GauntletOrderUIHandler)this).OnTransferFinished);
		callbacks.OnBeforeOrder = new OnBeforeOrderDelegate(base.OnBeforeOrder);
		dataSource.SetCallbacks(callbacks);
		base._dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("ToggleEscapeMenu"));
		base._dataSource.TroopController.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		base._dataSource.TroopController.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		base._dataSource.TroopController.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		GameKeyContext category = HotKeyManager.GetCategory("MissionOrderHotkeyCategory");
		base._dataSource.SetOrderIndexKey(0, category.GetGameKey(69));
		base._dataSource.SetOrderIndexKey(1, category.GetGameKey(70));
		base._dataSource.SetOrderIndexKey(2, category.GetGameKey(71));
		base._dataSource.SetOrderIndexKey(3, category.GetGameKey(72));
		base._dataSource.SetOrderIndexKey(4, category.GetGameKey(73));
		base._dataSource.SetOrderIndexKey(5, category.GetGameKey(74));
		base._dataSource.SetOrderIndexKey(6, category.GetGameKey(75));
		base._dataSource.SetOrderIndexKey(7, category.GetGameKey(76));
		base._dataSource.SetOrderIndexKey(8, category.GetGameKey(77));
		base._dataSource.SetReturnKey(category.GetGameKey(77));
		base._gauntletLayer = new GauntletLayer("MultiplayerOrder", ((MissionView)this).ViewOrderPriority, false);
		base._spriteCategory = UIResourceManager.LoadSpriteCategory("ui_order");
		string text = ((BannerlordConfig.OrderType == 0) ? base._barOrderMovieName : base._radialOrderMovieName);
		base._movie = base._gauntletLayer.LoadMovie(text, (ViewModel)(object)base._dataSource);
		base._dataSource.InputRestrictions = ((ScreenLayer)base._gauntletLayer).InputRestrictions;
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)base._gauntletLayer);
		base._dataSource.AfterInitialize();
		_isValid = true;
	}

	private void RefreshVisuals()
	{
	}

	private void Clear()
	{
		if (base._gauntletLayer != null)
		{
			((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)base._gauntletLayer);
		}
		if (base._dataSource != null)
		{
			((ViewModel)base._dataSource).OnFinalize();
		}
		base._gauntletLayer = null;
		base._dataSource = null;
		base._movie = null;
		if (_isValid)
		{
			base._spriteCategory.Unload();
		}
	}

	private void TeamChange(NetworkCommunicator peer, Team previousTeam, Team newTeam)
	{
		if (peer.IsMine)
		{
			Clear();
			_isValid = false;
		}
	}
}
