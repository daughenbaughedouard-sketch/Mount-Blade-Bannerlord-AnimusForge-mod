using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NavalDLC.Missions;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.View.MissionViews;
using NavalDLC.ViewModelCollection.Order;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(NavalMissionOrderUIHandler))]
public class MissionGauntletNavalOrderUIHandler : MissionGauntletSingleplayerOrderUIHandler
{
	protected NavalShipTargetSelectionHandler _shipTargetHandler;

	private OrderController _orderController;

	public MissionGauntletNavalOrderUIHandler()
	{
		((GauntletOrderUIHandler)this)._radialOrderMovieName = "NavalOrderRadial";
		((GauntletOrderUIHandler)this)._barOrderMovieName = "NavalOrderBar";
	}

	public override void OnMissionScreenInitialize()
	{
		((MissionGauntletSingleplayerOrderUIHandler)this).OnMissionScreenInitialize();
		_shipTargetHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipTargetSelectionHandler>();
		Mission mission = ((MissionBehavior)this).Mission;
		object orderController;
		if (mission == null)
		{
			orderController = null;
		}
		else
		{
			Team playerTeam = mission.PlayerTeam;
			orderController = ((playerTeam != null) ? playerTeam.PlayerOrderController : null);
		}
		_orderController = (OrderController)orderController;
		if (_orderController != null)
		{
			_orderController.OnSelectedFormationsChanged += OnSelectedFormationsChanged;
		}
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionGauntletSingleplayerOrderUIHandler)this).OnMissionScreenFinalize();
		if (_orderController != null)
		{
			_orderController.OnSelectedFormationsChanged -= OnSelectedFormationsChanged;
		}
	}

	protected override MissionOrderVM CreateDataSource(OrderController orderController)
	{
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
		NavalMissionOrderVM navalMissionOrderVM = new NavalMissionOrderVM(orderController, ((GauntletOrderUIHandler)this).IsDeployment, isMultiplayer: false);
		((MissionOrderVM)navalMissionOrderVM).SetDeploymentParemeters(((MissionView)this).MissionScreen.CombatCamera, ((GauntletOrderUIHandler)this).IsSiegeDeployment ? ((GauntletOrderUIHandler)this)._siegeDeploymentHandler.PlayerDeploymentPoints.ToList() : new List<DeploymentPoint>());
		MissionOrderCallbacks callbacks = new MissionOrderCallbacks
		{
			ToggleMissionInputs = ((GauntletOrderUIHandler)this).ToggleScreenRotation,
			RefreshVisuals = new OnRefreshVisualsDelegate(base.RefreshVisuals),
			GetVisualOrderExecutionParameters = new GetOrderExecutionParametersDelegate(((GauntletOrderUIHandler)this).GetVisualOrderExecutionParameters)
		};
		callbacks.SetSuspendTroopPlacer = new ToggleOrderPositionVisibilityDelegate(((GauntletOrderUIHandler)this).SetSuspendTroopPlacer);
		callbacks.OnActivateToggleOrder = new OnToggleActivateOrderStateDelegate(((GauntletOrderUIHandler)this).OnActivateToggleOrder);
		callbacks.OnDeactivateToggleOrder = new OnToggleActivateOrderStateDelegate(((GauntletOrderUIHandler)this).OnDeactivateToggleOrder);
		callbacks.OnTransferTroopsFinished = new OnTransferTroopsFinishedDelegate(((GauntletOrderUIHandler)this).OnTransferFinished);
		callbacks.OnBeforeOrder = new OnBeforeOrderDelegate(((GauntletOrderUIHandler)this).OnBeforeOrder);
		((MissionOrderVM)navalMissionOrderVM).SetCallbacks(callbacks);
		return (MissionOrderVM)(object)navalMissionOrderVM;
	}

	protected override OrderItemVM GetChargeOrder()
	{
		string text = (NavalDLCHelpers.IsShipOrdersAvailable() ? "order_movement_advance" : "order_movement_charge");
		for (int i = 0; i < ((Collection<OrderSetVM>)(object)((GauntletOrderUIHandler)this)._dataSource.OrderSets).Count; i++)
		{
			OrderSetVM val = ((Collection<OrderSetVM>)(object)((GauntletOrderUIHandler)this)._dataSource.OrderSets)[i];
			for (int j = 0; j < ((Collection<OrderItemVM>)(object)val.Orders).Count; j++)
			{
				OrderItemVM val2 = ((Collection<OrderItemVM>)(object)val.Orders)[j];
				if (val2.Order.StringId == text)
				{
					return val2;
				}
			}
		}
		return null;
	}

	public void OnClassesSet(List<ClassConfiguration> classData)
	{
		(((GauntletOrderUIHandler)this)._dataSource as NavalMissionOrderVM).OnClassesSet(classData);
	}

	protected override void TickInput(float dt)
	{
		bool flag = true;
		if (Agent.Main != null)
		{
			ShipControllerMachine shipControllerMachine = Agent.Main.GetComponent<AgentNavalComponent>()?.SteppedShip?.ShipControllerMachine;
			if (shipControllerMachine != null && ((UsableMachine)shipControllerMachine).PilotAgent == Agent.Main && shipControllerMachine.CaptureTimer > 0f)
			{
				flag = false;
			}
		}
		if (flag)
		{
			((GauntletOrderUIHandler)this).TickInput(dt);
			return;
		}
		((GauntletOrderUIHandler)this)._isReceivingInput = false;
		MissionOrderVM dataSource = ((GauntletOrderUIHandler)this)._dataSource;
		if (dataSource != null)
		{
			dataSource.UpdateCanUseShortcuts(false);
		}
		MissionOrderVM dataSource2 = ((GauntletOrderUIHandler)this)._dataSource;
		if (dataSource2 != null)
		{
			dataSource2.TryCloseToggleOrder(false);
		}
	}

	private void OnSelectedFormationsChanged()
	{
		OrderController orderController = _orderController;
		MBReadOnlyList<Formation> val = ((orderController != null) ? orderController.SelectedFormations : null);
		if (val != null)
		{
			bool isFormationTargetingDisabled = ((List<Formation>)(object)val).Count == 1 && NavalDLCHelpers.IsPlayerCaptainOfFormationShip(((List<Formation>)(object)val)[0]);
			MissionFormationTargetSelectionHandler formationTargetHandler = ((GauntletOrderUIHandler)this)._formationTargetHandler;
			if (formationTargetHandler != null)
			{
				formationTargetHandler.SetIsFormationTargetingDisabled(isFormationTargetingDisabled);
			}
			_shipTargetHandler?.SetIsFormationTargetingDisabled(isFormationTargetingDisabled);
		}
	}
}
