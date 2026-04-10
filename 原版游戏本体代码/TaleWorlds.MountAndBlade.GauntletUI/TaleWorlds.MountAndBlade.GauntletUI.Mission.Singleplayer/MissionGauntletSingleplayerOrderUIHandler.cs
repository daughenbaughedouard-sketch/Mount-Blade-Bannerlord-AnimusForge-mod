using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionOrderUIHandler))]
public class MissionGauntletSingleplayerOrderUIHandler : GauntletOrderUIHandler, ISiegeDeploymentView
{
	private const float _slowDownAmountWhileOrderIsOpen = 0.25f;

	private const int _missionTimeSpeedRequestID = 864;

	private List<DeploymentSiegeMachineVM> _deploymentPointDataSources;

	public override bool IsValidForTick
	{
		get
		{
			if (!base.MissionScreen.IsPhotoModeEnabled && !GameStateManager.Current.ActiveStateDisabledByUser)
			{
				if (base.MissionScreen.IsRadialMenuActive)
				{
					return _dataSource.IsToggleOrderShown;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsDeployment
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			Mission mission = ((MissionBehavior)this).Mission;
			if (mission == null)
			{
				return false;
			}
			return (int)mission.Mode == 6;
		}
	}

	public override bool IsSiegeDeployment
	{
		get
		{
			if (IsDeployment)
			{
				return _siegeDeploymentHandler != null;
			}
			return false;
		}
	}

	protected virtual MissionOrderVM CreateDataSource(OrderController orderController)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		MissionOrderVM val = new MissionOrderVM(orderController, IsDeployment, false);
		val.SetDeploymentParemeters(base.MissionScreen.CombatCamera, IsSiegeDeployment ? _siegeDeploymentHandler.PlayerDeploymentPoints.ToList() : new List<DeploymentPoint>());
		MissionOrderCallbacks callbacks = new MissionOrderCallbacks
		{
			ToggleMissionInputs = base.ToggleScreenRotation,
			RefreshVisuals = new OnRefreshVisualsDelegate(RefreshVisuals),
			GetVisualOrderExecutionParameters = new GetOrderExecutionParametersDelegate(base.GetVisualOrderExecutionParameters)
		};
		callbacks.SetSuspendTroopPlacer = new ToggleOrderPositionVisibilityDelegate(SetSuspendTroopPlacer);
		callbacks.OnActivateToggleOrder = new OnToggleActivateOrderStateDelegate(base.OnActivateToggleOrder);
		callbacks.OnDeactivateToggleOrder = new OnToggleActivateOrderStateDelegate(base.OnDeactivateToggleOrder);
		callbacks.OnTransferTroopsFinished = new OnTransferTroopsFinishedDelegate(OnTransferFinished);
		callbacks.OnBeforeOrder = new OnBeforeOrderDelegate(base.OnBeforeOrder);
		val.SetCallbacks(callbacks);
		return val;
	}

	public override void OnConversationBegin()
	{
		base.OnConversationBegin();
		MissionOrderVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.TryCloseToggleOrder(false);
		}
	}

	public MissionGauntletSingleplayerOrderUIHandler()
	{
		ViewOrderPriority = 14;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Expected O, but got Unknown
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Expected O, but got Unknown
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Expected O, but got Unknown
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Expected O, but got Unknown
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		base.OnMissionScreenInitialize();
		GameKeyContext category = HotKeyManager.GetCategory("MissionOrderHotkeyCategory");
		GameKeyContext category2 = HotKeyManager.GetCategory("GenericPanelGameKeyCategory");
		((ScreenLayer)base.MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(category);
		_orderTroopPlacer = ((MissionBehavior)this).Mission.GetMissionBehavior<OrderTroopPlacer>();
		base.MissionScreen.OrderFlag = _orderTroopPlacer.OrderFlag;
		base.MissionScreen.SetOrderFlagVisibility(value: false);
		_siegeDeploymentHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<SiegeDeploymentHandler>();
		_formationTargetHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionFormationTargetSelectionHandler>();
		if (_formationTargetHandler != null)
		{
			_formationTargetHandler.OnFormationFocused += OnFormationFocused;
		}
		_deploymentPointDataSources = new List<DeploymentSiegeMachineVM>();
		_dataSource = CreateDataSource(((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController);
		_dataSource.SetCancelInputKey(category2.GetHotKey("ToggleEscapeMenu"));
		_dataSource.TroopController.SetDoneInputKey(category2.GetHotKey("Confirm"));
		_dataSource.TroopController.SetCancelInputKey(category2.GetHotKey("Exit"));
		_dataSource.TroopController.SetResetInputKey(category2.GetHotKey("Reset"));
		_dataSource.SetOrderIndexKey(0, category.GetGameKey(69));
		_dataSource.SetOrderIndexKey(1, category.GetGameKey(70));
		_dataSource.SetOrderIndexKey(2, category.GetGameKey(71));
		_dataSource.SetOrderIndexKey(3, category.GetGameKey(72));
		_dataSource.SetOrderIndexKey(4, category.GetGameKey(73));
		_dataSource.SetOrderIndexKey(5, category.GetGameKey(74));
		_dataSource.SetOrderIndexKey(6, category.GetGameKey(75));
		_dataSource.SetOrderIndexKey(7, category.GetGameKey(76));
		_dataSource.SetOrderIndexKey(8, category.GetGameKey(77));
		_dataSource.SetReturnKey(category.GetGameKey(77));
		if (IsSiegeDeployment)
		{
			foreach (DeploymentPoint playerDeploymentPoint in _siegeDeploymentHandler.PlayerDeploymentPoints)
			{
				DeploymentSiegeMachineVM val = new DeploymentSiegeMachineVM(playerDeploymentPoint, (SiegeWeapon)null, base.MissionScreen.CombatCamera, (Action<DeploymentSiegeMachineVM>)_dataSource.DeploymentController.OnRefreshSelectedDeploymentPoint, (Action<DeploymentPoint>)_dataSource.DeploymentController.OnEntityHover, false);
				WeakGameEntity val2 = ((ScriptComponentBehavior)playerDeploymentPoint).GameEntity;
				Vec3 origin = ((WeakGameEntity)(ref val2)).GetFrame().origin;
				int num = 0;
				while (true)
				{
					int num2 = num;
					val2 = ((ScriptComponentBehavior)playerDeploymentPoint).GameEntity;
					if (num2 >= ((WeakGameEntity)(ref val2)).ChildCount)
					{
						break;
					}
					val2 = ((ScriptComponentBehavior)playerDeploymentPoint).GameEntity;
					val2 = ((WeakGameEntity)(ref val2)).GetChild(num);
					if (((WeakGameEntity)(ref val2)).HasTag("deployment_point_icon_target"))
					{
						Vec3 val3 = origin;
						val2 = ((ScriptComponentBehavior)playerDeploymentPoint).GameEntity;
						val2 = ((WeakGameEntity)(ref val2)).GetChild(num);
						origin = val3 + ((WeakGameEntity)(ref val2)).GetFrame().origin;
						break;
					}
					num++;
				}
				_deploymentPointDataSources.Add(val);
				val.RemainingCount = 0;
			}
		}
		_gauntletLayer = new GauntletLayer("MissionOrder", ViewOrderPriority, false);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(category2);
		string text = ((!IsDeployment) ? ((BannerlordConfig.OrderType == 0) ? _barOrderMovieName : _radialOrderMovieName) : _radialOrderMovieName);
		_spriteCategory = UIResourceManager.LoadSpriteCategory("ui_order");
		_movie = _gauntletLayer.LoadMovie(text, (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		if (!IsDeployment && BannerlordConfig.HideBattleUI)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
		_dataSource.InputRestrictions = ((ScreenLayer)_gauntletLayer).InputRestrictions;
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	private void OnManagedOptionChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Invalid comparison between Unknown and I4
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 35)
		{
			if (!IsDeployment)
			{
				_gauntletLayer.ReleaseMovie(_movie);
				string text = ((BannerlordConfig.OrderType == 0) ? _barOrderMovieName : _radialOrderMovieName);
				_movie = _gauntletLayer.LoadMovie(text, (ViewModel)(object)_dataSource);
			}
		}
		else if ((int)changedManagedOptionsType == 36)
		{
			MissionOrderVM dataSource = _dataSource;
			if (dataSource != null)
			{
				dataSource.OnOrderLayoutTypeChanged();
			}
		}
		else if ((int)changedManagedOptionsType == 45)
		{
			if (!IsDeployment)
			{
				_gauntletLayer.UIContext.ContextAlpha = (BannerlordConfig.HideBattleUI ? 0f : 1f);
			}
		}
		else if ((int)changedManagedOptionsType == 39 && !BannerlordConfig.SlowDownOnOrder && _slowedDownMission)
		{
			((MissionBehavior)this).Mission.RemoveTimeSpeedRequest(864);
		}
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.OnMissionScreenFinalize();
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		if (_formationTargetHandler != null)
		{
			_formationTargetHandler.OnFormationFocused -= OnFormationFocused;
		}
		_deploymentPointDataSources = null;
		_orderTroopPlacer = null;
		_movie = null;
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_siegeDeploymentHandler = null;
		_spriteCategory.Unload();
		_formationTargetHandler = null;
	}

	protected override void OnTransferFinished()
	{
		if (!IsDeployment)
		{
			SetLayerEnabled(isEnabled: false);
		}
	}

	public void OnAutoDeploy()
	{
		_dataSource.DeploymentController.ExecuteAutoDeploy();
		ClearFormationSelection();
	}

	public void OnBeginMission()
	{
		_dataSource.DeploymentController.ExecuteBeginMission();
	}

	protected override void SetLayerEnabled(bool isEnabled)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (isEnabled)
		{
			if (!base.MissionScreen.IsRadialMenuActive)
			{
				if (_dataSource == null || _dataSource.ActiveTargetState == 0)
				{
					_orderTroopPlacer.SuspendTroopPlacer = false;
				}
				if (!_slowedDownMission && BannerlordConfig.SlowDownOnOrder)
				{
					((MissionBehavior)this).Mission.AddTimeSpeedRequest(new TimeSpeedRequest(0.25f, 864));
					_slowedDownMission = true;
				}
				base.MissionScreen.SetOrderFlagVisibility(value: true);
				Game.Current.EventManager.TriggerEvent<MissionPlayerToggledOrderViewEvent>(new MissionPlayerToggledOrderViewEvent(newIsEnabledState: true));
			}
		}
		else
		{
			SetSuspendTroopPlacer(value: true);
			if (_slowedDownMission)
			{
				((MissionBehavior)this).Mission.RemoveTimeSpeedRequest(864);
				_slowedDownMission = false;
			}
			Game.Current.EventManager.TriggerEvent<MissionPlayerToggledOrderViewEvent>(new MissionPlayerToggledOrderViewEvent(newIsEnabledState: false));
		}
	}

	public override void OnDeploymentFinished()
	{
		((MissionBehavior)this).OnDeploymentFinished();
		_dataSource.OnDeploymentFinished();
		_dataSource.TryCloseToggleOrder(false);
		_deploymentPointDataSources.Clear();
		SetSuspendTroopPlacer(value: true);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer.UIContext.ContextAlpha = (BannerlordConfig.HideBattleUI ? 0f : 1f);
		string text = ((BannerlordConfig.OrderType == 0) ? _barOrderMovieName : _radialOrderMovieName);
		if (text != _radialOrderMovieName)
		{
			_gauntletLayer.ReleaseMovie(_movie);
			_movie = _gauntletLayer.LoadMovie(text, (ViewModel)(object)_dataSource);
		}
	}

	public override void OnAfterDeploymentFinished()
	{
		((MissionBehavior)this).OnAfterDeploymentFinished();
		_dataSource.OnAfterDeploymentFinished();
	}

	protected void RefreshVisuals()
	{
		if (!IsSiegeDeployment)
		{
			return;
		}
		foreach (DeploymentSiegeMachineVM deploymentPointDataSource in _deploymentPointDataSources)
		{
			deploymentPointDataSource.RefreshWithDeployedWeapon();
		}
	}

	public void ClearFormationSelection()
	{
		MissionOrderVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.DeploymentController.ExecuteCancelSelectedDeploymentPoint();
		}
		MissionOrderVM dataSource2 = _dataSource;
		if (dataSource2 != null)
		{
			dataSource2.OrderController.ClearSelectedFormations();
		}
		MissionOrderVM dataSource3 = _dataSource;
		if (dataSource3 != null)
		{
			dataSource3.TryCloseToggleOrder(false);
		}
	}

	public void OnFiltersSet(List<FormationConfiguration> filterData)
	{
		_dataSource.OnFiltersSet(filterData);
	}

	private void OnFormationFocused(MBReadOnlyList<Formation> focusedFormations)
	{
		_focusedFormationsCache = focusedFormations;
		_dataSource.SetFocusedFormations(_focusedFormationsCache);
	}

	void ISiegeDeploymentView.OnEntityHover(WeakGameEntity hoveredEntity)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!((ScreenLayer)_gauntletLayer).IsHitThisFrame)
		{
			_dataSource.DeploymentController.OnEntityHover(hoveredEntity);
		}
	}

	void ISiegeDeploymentView.OnEntitySelection(WeakGameEntity selectedEntity)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_dataSource.DeploymentController.OnEntitySelect(selectedEntity);
	}
}
