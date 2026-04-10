using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public abstract class GauntletOrderUIHandler : MissionView
{
	protected MBReadOnlyList<Formation> _focusedFormationsCache;

	protected string _radialOrderMovieName = "OrderRadial";

	protected string _barOrderMovieName = "OrderBar";

	protected float _holdTime;

	protected bool _holdHandled;

	protected OrderTroopPlacer _orderTroopPlacer;

	protected GauntletLayer _gauntletLayer;

	protected GauntletMovieIdentifier _movie;

	protected SpriteCategory _spriteCategory;

	protected MissionOrderVM _dataSource;

	protected SiegeDeploymentHandler _siegeDeploymentHandler;

	protected MissionFormationTargetSelectionHandler _formationTargetHandler;

	protected bool _isOrderRadialEnabled;

	protected bool _isReceivingInput;

	protected bool _isInitialized;

	protected bool _slowedDownMission;

	protected float _latestDt;

	protected bool _targetFormationOrderGivenWithActionButton;

	protected bool _isTransferEnabled;

	public abstract bool IsDeployment { get; }

	public abstract bool IsSiegeDeployment { get; }

	public abstract bool IsValidForTick { get; }

	public CursorStates CursorState
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			MissionOrderVM dataSource = _dataSource;
			if (dataSource == null)
			{
				return (CursorStates)0;
			}
			return dataSource.CursorState;
		}
	}

	protected float _minHoldTimeForActivation => 0f;

	public bool IsOrderMenuActive
	{
		get
		{
			MissionOrderVM dataSource = _dataSource;
			if (dataSource == null)
			{
				return false;
			}
			return dataSource.IsToggleOrderShown;
		}
	}

	public bool IsAnyOrderSetActive
	{
		get
		{
			MissionOrderVM dataSource = _dataSource;
			if (dataSource == null)
			{
				return false;
			}
			return dataSource.IsAnyOrderSetActive;
		}
	}

	public bool IsViewCreated
	{
		get
		{
			if (_gauntletLayer != null)
			{
				return _dataSource != null;
			}
			return false;
		}
	}

	public GauntletOrderUIHandler()
	{
		ViewOrderPriority = 14;
	}

	protected abstract void OnTransferFinished();

	protected abstract void SetLayerEnabled(bool isEnabled);

	protected virtual void SetSuspendTroopPlacer(bool value)
	{
		_orderTroopPlacer.SuspendTroopPlacer = value;
		base.MissionScreen.SetOrderFlagVisibility(!value);
	}

	public virtual void SelectFormationAtIndex(int index)
	{
		MissionOrderVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnTroopFormationSelected(index);
		}
	}

	public virtual void DeselectFormationAtIndex(int index)
	{
		MissionOrderVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.TroopController.OnDeselectFormation(index);
		}
	}

	protected virtual IOrderable GetFocusedOrderableObject()
	{
		return base.MissionScreen.OrderFlag?.FocusedOrderableObject;
	}

	protected VisualOrderExecutionParameters GetVisualOrderExecutionParameters()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Formation val = null;
		MBReadOnlyList<Formation> focusedFormationsCache = _focusedFormationsCache;
		if (focusedFormationsCache != null && ((List<Formation>)(object)focusedFormationsCache).Count > 0)
		{
			val = ((List<Formation>)(object)_focusedFormationsCache)[0];
		}
		WorldPosition? val2 = null;
		if ((NativeObject)(object)base.MissionScreen.Mission.Scene != (NativeObject)null)
		{
			Vec3 orderFlagPosition = base.MissionScreen.GetOrderFlagPosition();
			val2 = new WorldPosition(base.MissionScreen.Mission.Scene, orderFlagPosition);
		}
		return new VisualOrderExecutionParameters(Agent.Main, val, val2);
	}

	public override void OnMissionScreenActivate()
	{
		base.OnMissionScreenActivate();
		if (_dataSource != null)
		{
			_dataSource.AfterInitialize();
			_isInitialized = true;
		}
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
	}

	public override void OnMissionScreenDeactivate()
	{
		base.OnMissionScreenDeactivate();
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Remove(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
	}

	private void OnGamepadActiveStateChanged()
	{
		if (_dataSource != null)
		{
			((List<OrderTroopItemVM>)(object)_dataSource.TroopController.TroopList).ForEach((Action<OrderTroopItemVM>)delegate(OrderTroopItemVM t)
			{
				t.UpdateSelectionKeyInfo();
			});
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Invalid comparison between Unknown and I4
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Invalid comparison between Unknown and I4
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		base.OnMissionScreenTick(dt);
		_latestDt = dt;
		_isReceivingInput = false;
		if (IsValidForTick && _dataSource != null && ((ScreenLayer)_gauntletLayer).IsActive)
		{
			TickInput(dt);
			_dataSource.Update();
			if (_dataSource.IsToggleOrderShown)
			{
				if (_targetFormationOrderGivenWithActionButton)
				{
					SetSuspendTroopPlacer(value: false);
					_targetFormationOrderGivenWithActionButton = false;
				}
				OrderTroopPlacer orderTroopPlacer = _orderTroopPlacer;
				OrderSetVM selectedOrderSet = _dataSource.SelectedOrderSet;
				orderTroopPlacer.IsDrawingForced = ((selectedOrderSet != null) ? selectedOrderSet.OrderSet.StringId : null) == "order_type_movement";
				OrderTroopPlacer orderTroopPlacer2 = _orderTroopPlacer;
				OrderSetVM selectedOrderSet2 = _dataSource.SelectedOrderSet;
				orderTroopPlacer2.IsDrawingFacing = ((selectedOrderSet2 != null) ? selectedOrderSet2.OrderSet.StringId : null) == "order_type_facing";
				_orderTroopPlacer.IsDrawingForming = false;
				if ((int)CursorState == 1)
				{
					MBReadOnlyList<Formation> selectedFormations = ((MissionBehavior)this).Mission.MainAgent.Team.PlayerOrderController.SelectedFormations;
					Vec3 position = base.MissionScreen.OrderFlag.Position;
					Vec2 orderLookAtDirection = OrderController.GetOrderLookAtDirection((IEnumerable<Formation>)selectedFormations, ((Vec3)(ref position)).AsVec2);
					base.MissionScreen.OrderFlag.SetArrowVisibility(isVisible: true, orderLookAtDirection);
				}
				else
				{
					base.MissionScreen.OrderFlag.SetArrowVisibility(isVisible: false, Vec2.Invalid);
				}
				if ((int)CursorState == 2)
				{
					float orderFormCustomWidth = OrderController.GetOrderFormCustomWidth((IEnumerable<Formation>)((MissionBehavior)this).Mission.MainAgent.Team.PlayerOrderController.SelectedFormations, base.MissionScreen.OrderFlag.Position);
					base.MissionScreen.OrderFlag.SetWidthVisibility(isVisible: true, orderFormCustomWidth);
				}
				else
				{
					base.MissionScreen.OrderFlag.SetWidthVisibility(isVisible: false, -1f);
				}
				if (Input.IsGamepadActive)
				{
					OrderSetVM selectedOrderSet3 = _dataSource.SelectedOrderSet;
					if (selectedOrderSet3 == null || selectedOrderSet3.HasSingleOrder)
					{
						if (_orderTroopPlacer.SuspendTroopPlacer && _dataSource.ActiveTargetState == 0)
						{
							_orderTroopPlacer.SuspendTroopPlacer = false;
						}
					}
					else if (!_orderTroopPlacer.SuspendTroopPlacer)
					{
						_orderTroopPlacer.SuspendTroopPlacer = true;
					}
				}
			}
			else if (_dataSource.TroopController.IsTransferActive || IsDeployment)
			{
				((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			}
			else
			{
				if (!_dataSource.TroopController.IsTransferActive && !_orderTroopPlacer.SuspendTroopPlacer)
				{
					_orderTroopPlacer.SuspendTroopPlacer = true;
				}
				((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			}
			if (IsDeployment)
			{
				if (!base.MissionScreen.IsRadialMenuActive && (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsKeyDown((InputKey)225) || ((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsKeyDown((InputKey)254)))
				{
					((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(false);
				}
				else
				{
					((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(true);
				}
			}
			base.MissionScreen.OrderFlag.IsTroop = _dataSource.ActiveTargetState == 0;
			TickOrderFlag(_latestDt, forceUpdate: false);
		}
		bool flag = IsOrderRadialActive();
		if (_isOrderRadialEnabled && !flag)
		{
			base.MissionScreen.UnregisterRadialMenuObject(this);
		}
		else if (!_isOrderRadialEnabled && flag)
		{
			base.MissionScreen.RegisterRadialMenuObject(this);
		}
		_isOrderRadialEnabled = flag;
		_targetFormationOrderGivenWithActionButton = false;
		MissionOrderVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.UpdateCanUseShortcuts(_isReceivingInput);
		}
	}

	protected virtual void TickInput(float dt)
	{
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Expected I4, but got Unknown
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Unknown result type (might be due to invalid IL or missing references)
		//IL_0717: Unknown result type (might be due to invalid IL or missing references)
		//IL_071b: Unknown result type (might be due to invalid IL or missing references)
		if (_dataSource == null)
		{
			return;
		}
		bool displayDialog = ((IMissionScreen)base.MissionScreen).GetDisplayDialog();
		bool flag = ((ScreenLayer)base.MissionScreen.SceneLayer).IsHitThisFrame || ((ScreenLayer)_gauntletLayer).IsHitThisFrame;
		if (displayDialog || (Input.IsGamepadActive && !flag))
		{
			_isReceivingInput = false;
			_dataSource.UpdateCanUseShortcuts(false);
			return;
		}
		if (Input.IsGamepadActive)
		{
			for (int i = 0; i < ((List<OrderTroopItemVM>)(object)_dataSource.TroopController.TroopList).Count; i++)
			{
				OrderTroopItemVM val = ((List<OrderTroopItemVM>)(object)_dataSource.TroopController.TroopList)[i];
				((OrderSubjectVM)val).ShowSelectionInputs = ((OrderSubjectVM)val).IsSelectionHighlightActive && ((OrderSubjectVM)val).IsSelectable;
			}
		}
		else
		{
			for (int j = 0; j < ((List<OrderTroopItemVM>)(object)_dataSource.TroopController.TroopList).Count; j++)
			{
				OrderTroopItemVM obj = ((List<OrderTroopItemVM>)(object)_dataSource.TroopController.TroopList)[j];
				((OrderSubjectVM)obj).IsSelectionHighlightActive = false;
				((OrderSubjectVM)obj).ShowSelectionInputs = ((OrderSubjectVM)obj).IsSelectable;
			}
		}
		_isReceivingInput = true;
		if (!IsDeployment)
		{
			if (!_holdHandled && base.Input.IsGameKeyDown(87) && !_dataSource.IsToggleOrderShown)
			{
				_holdTime += dt;
				if (_holdTime >= _minHoldTimeForActivation)
				{
					_dataSource.OpenToggleOrder(true, !_dataSource.IsHolding);
					_dataSource.IsHolding = true;
					_holdHandled = true;
				}
			}
			else if (_holdHandled && !base.Input.IsGameKeyDown(87))
			{
				if (_dataSource.IsHolding && _dataSource.IsToggleOrderShown)
				{
					_dataSource.TryCloseToggleOrder(true);
				}
				_dataSource.IsHolding = false;
				_holdTime = 0f;
				_holdHandled = false;
			}
		}
		if (_dataSource.IsToggleOrderShown)
		{
			if (_dataSource.ActiveTargetState == 0 && (base.Input.IsKeyReleased((InputKey)224) || base.Input.IsKeyReleased((InputKey)255)))
			{
				if (_dataSource.SelectedOrderSet != null && Input.IsGamepadActive)
				{
					VisualOrderExecutionParameters visualOrderExecutionParameters = GetVisualOrderExecutionParameters();
					OrderItemVM? obj2 = ((IEnumerable<OrderItemVM>)_dataSource.SelectedOrderSet.Orders).FirstOrDefault((Func<OrderItemVM, bool>)((OrderItemVM o) => ((OrderItemBaseVM)o).IsSelected));
					if (obj2 != null)
					{
						((OrderItemBaseVM)obj2).ExecuteAction(visualOrderExecutionParameters);
					}
				}
				else
				{
					CursorStates cursorState = CursorState;
					switch ((int)cursorState)
					{
					case 0:
					{
						MBReadOnlyList<Formation> focusedFormationsCache = _focusedFormationsCache;
						if (focusedFormationsCache != null && ((List<Formation>)(object)focusedFormationsCache).Count > 0)
						{
							OrderItemVM chargeOrder = GetChargeOrder();
							VisualOrderExecutionParameters visualOrderExecutionParameters2 = GetVisualOrderExecutionParameters();
							((OrderItemBaseVM)chargeOrder).ExecuteAction(visualOrderExecutionParameters2);
							SetSuspendTroopPlacer(value: true);
							_targetFormationOrderGivenWithActionButton = true;
							if (!_dataSource.IsHolding)
							{
								_dataSource.TryCloseToggleOrder(false);
							}
							break;
						}
						IOrderable focusedOrderableObject = GetFocusedOrderableObject();
						if (focusedOrderableObject != null)
						{
							if (((List<Formation>)(object)_dataSource.OrderController.SelectedFormations).Count > 0)
							{
								_dataSource.OrderController.SetOrderWithOrderableObject(focusedOrderableObject);
							}
							else
							{
								Debug.FailedAssert("No selected formations when issuing order", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletOrderUIBase.cs", "TickInput", 370);
							}
						}
						break;
					}
					case 1:
						_dataSource.OrderController.SetOrderWithPosition((OrderType)15, new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, base.MissionScreen.GetOrderFlagPosition(), false));
						break;
					case 2:
						_dataSource.OrderController.SetOrderWithPosition((OrderType)24, new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, base.MissionScreen.GetOrderFlagPosition(), false));
						break;
					default:
						Debug.FailedAssert("false", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletOrderUIBase.cs", "TickInput", 385);
						break;
					}
				}
			}
			if (base.Input.IsKeyReleased((InputKey)225) && !IsDeployment)
			{
				_dataSource.OnEscape();
			}
		}
		else if (_dataSource.TroopController.IsTransferActive != _isTransferEnabled)
		{
			_isTransferEnabled = _dataSource.TroopController.IsTransferActive;
			if (!_isTransferEnabled)
			{
				_gauntletLayer.UIContext.ContextAlpha = (BannerlordConfig.HideBattleUI ? 0f : 1f);
				((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
				ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
			}
			else
			{
				_gauntletLayer.UIContext.ContextAlpha = 1f;
				((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
				ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
			}
		}
		else if (_dataSource.TroopController.IsTransferActive)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.TroopController.ExecuteCancelTransfer();
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
			{
				if (_dataSource.TroopController.IsTransferValid)
				{
					UISoundsHelper.PlayUISound("event:/ui/default");
					_dataSource.TroopController.ExecuteConfirmTransfer();
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Reset"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.TroopController.ExecuteReset();
			}
		}
		int num = -1;
		if ((!Input.IsGamepadActive || _dataSource.IsToggleOrderShown) && !((MissionBehavior)this).DebugInput.IsControlDown())
		{
			if (base.Input.IsGameKeyPressed(69))
			{
				num = 0;
			}
			else if (base.Input.IsGameKeyPressed(70))
			{
				num = 1;
			}
			else if (base.Input.IsGameKeyPressed(71))
			{
				num = 2;
			}
			else if (base.Input.IsGameKeyPressed(72))
			{
				num = 3;
			}
			else if (base.Input.IsGameKeyPressed(73))
			{
				num = 4;
			}
			else if (base.Input.IsGameKeyPressed(74))
			{
				num = 5;
			}
			else if (base.Input.IsGameKeyPressed(75))
			{
				num = 6;
			}
			else if (base.Input.IsGameKeyPressed(76))
			{
				num = 7;
			}
			else if (base.Input.IsGameKeyPressed(77) && !Input.IsGamepadActive)
			{
				num = 8;
			}
		}
		if (num > -1)
		{
			if (_dataSource.SelectedOrderSet != null)
			{
				int count = ((Collection<OrderItemVM>)(object)_dataSource.SelectedOrderSet.Orders).Count;
				if (count > 0 && num >= 0)
				{
					if (num == 8 && ((IEnumerable<OrderItemVM>)_dataSource.SelectedOrderSet.Orders).Any((OrderItemVM x) => x.Order is ReturnVisualOrder))
					{
						_dataSource.SelectedOrderSet.ExecuteDeSelect();
					}
					else if (num < count)
					{
						OrderItemVM val2 = ((Collection<OrderItemVM>)(object)_dataSource.SelectedOrderSet.Orders)[num];
						if (!(val2.Order is ReturnVisualOrder))
						{
							VisualOrderExecutionParameters visualOrderExecutionParameters3 = GetVisualOrderExecutionParameters();
							((OrderItemBaseVM)val2).ExecuteAction(visualOrderExecutionParameters3);
							if (IsDeployment || _dataSource.IsHolding)
							{
								OrderSetVM selectedOrderSet = _dataSource.SelectedOrderSet;
								if (selectedOrderSet != null)
								{
									selectedOrderSet.ExecuteDeSelect();
								}
							}
							else
							{
								_dataSource.TryCloseToggleOrder(false);
							}
						}
					}
				}
			}
			else
			{
				_dataSource.OpenToggleOrder(false, true);
				if (_dataSource.IsToggleOrderShown)
				{
					if (num == 8 && ((IEnumerable<OrderSetVM>)_dataSource.OrderSets).Any((OrderSetVM x) => x.HasSingleOrder && ((Collection<OrderItemVM>)(object)x.Orders)[0].Order is ReturnVisualOrder))
					{
						_dataSource.TryCloseToggleOrder(false);
					}
					else
					{
						OrderSetVM orderSetAtIndex = _dataSource.GetOrderSetAtIndex(num);
						if (orderSetAtIndex != null && (!orderSetAtIndex.HasSingleOrder || !(((Collection<OrderItemVM>)(object)orderSetAtIndex.Orders)[0].Order is ReturnVisualOrder)))
						{
							_dataSource.TrySelectOrderSet(orderSetAtIndex);
						}
					}
				}
			}
		}
		int num2 = -1;
		if (base.Input.IsGameKeyPressed(78))
		{
			num2 = 100;
		}
		else if (base.Input.IsGameKeyPressed(79))
		{
			num2 = 0;
		}
		else if (base.Input.IsGameKeyPressed(80))
		{
			num2 = 1;
		}
		else if (base.Input.IsGameKeyPressed(81))
		{
			num2 = 2;
		}
		else if (base.Input.IsGameKeyPressed(82))
		{
			num2 = 3;
		}
		else if (base.Input.IsGameKeyPressed(83))
		{
			num2 = 4;
		}
		else if (base.Input.IsGameKeyPressed(84))
		{
			num2 = 5;
		}
		else if (base.Input.IsGameKeyPressed(85))
		{
			num2 = 6;
		}
		else if (base.Input.IsGameKeyPressed(86))
		{
			num2 = 7;
		}
		if (!IsDeployment && _dataSource.IsToggleOrderShown && Input.IsGamepadActive)
		{
			if (base.Input.IsGameKeyPressed(88))
			{
				_dataSource.OnTroopHighlightSelection(true);
			}
			else if (base.Input.IsGameKeyPressed(89))
			{
				_dataSource.OnTroopHighlightSelection(false);
			}
			else if (base.Input.IsGameKeyPressed(90))
			{
				_dataSource.ExecuteSelectHighlightedFormation();
			}
			else if (base.Input.IsGameKeyPressed(91))
			{
				_dataSource.ExecuteToggleHighlightedFormation();
			}
		}
		if (num2 != -1)
		{
			_dataSource.OnTroopFormationSelected(num2);
		}
		if (base.Input.IsGameKeyPressed(68))
		{
			_dataSource.ViewOrders();
		}
	}

	protected virtual OrderItemVM GetChargeOrder()
	{
		if (_dataSource == null)
		{
			return null;
		}
		for (int i = 0; i < ((Collection<OrderSetVM>)(object)_dataSource.OrderSets).Count; i++)
		{
			OrderSetVM val = ((Collection<OrderSetVM>)(object)_dataSource.OrderSets)[i];
			for (int j = 0; j < ((Collection<OrderItemVM>)(object)val.Orders).Count; j++)
			{
				OrderItemVM val2 = ((Collection<OrderItemVM>)(object)val.Orders)[j];
				if (val2.Order.StringId == "order_movement_charge")
				{
					return val2;
				}
			}
		}
		return null;
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if (_isInitialized && agent.IsHuman && _dataSource != null)
		{
			_dataSource.TroopController.AddTroops(agent);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		if (affectedAgent.IsHuman && _dataSource != null)
		{
			_dataSource.TroopController.RemoveTroops(affectedAgent);
		}
	}

	public override bool OnEscape()
	{
		if (_dataSource != null)
		{
			bool isToggleOrderShown = _dataSource.IsToggleOrderShown;
			_dataSource.OnEscape();
			return isToggleOrderShown;
		}
		return false;
	}

	public override bool IsReady()
	{
		return _spriteCategory.IsCategoryFullyLoaded();
	}

	private bool IsOrderRadialActive()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		if (_dataSource != null && _dataSource.IsToggleOrderShown && (Input.IsGamepadActive || (int)((MissionBehavior)this).Mission.Mode == 6))
		{
			return ((IEnumerable<OrderSetVM>)_dataSource.OrderSets).Any((OrderSetVM x) => ((OrderItemBaseVM)x).IsSelected);
		}
		return false;
	}

	public void OnActivateToggleOrder()
	{
		SetLayerEnabled(isEnabled: true);
	}

	public void OnDeactivateToggleOrder()
	{
		if (_dataSource != null && !_dataSource.TroopController.IsTransferActive)
		{
			SetLayerEnabled(isEnabled: false);
		}
	}

	protected void OnBeforeOrder()
	{
		TickOrderFlag(_latestDt, forceUpdate: true);
	}

	protected void TickOrderFlag(float dt, bool forceUpdate)
	{
		if ((base.MissionScreen.OrderFlag.IsVisible || forceUpdate) && Utilities.EngineFrameNo != base.MissionScreen.OrderFlag.LatestUpdateFrameNo)
		{
			base.MissionScreen.OrderFlag.Tick(_latestDt);
		}
	}

	protected void ToggleScreenRotation(bool isLocked)
	{
		MissionScreen.SetFixedMissionCameraActive(isLocked);
	}

	protected override void OnSuspendView()
	{
		base.OnSuspendView();
		_dataSource.TryCloseToggleOrder(false);
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
	}

	protected override void OnResumeView()
	{
		base.OnResumeView();
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
	}
}
