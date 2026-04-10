using System;
using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionOrderOfBattleUIHandler))]
public class MissionGauntletOrderOfBattleUIHandler : MissionView
{
	private OrderOfBattleVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private GauntletMovieIdentifier _movie;

	private SpriteCategory _orderOfBattleCategory;

	private MissionGauntletSingleplayerOrderUIHandler _orderUIHandler;

	private AssignPlayerRoleInTeamMissionController _playerRoleMissionController;

	private OrderTroopPlacer _orderTroopPlacer;

	private bool _isActive;

	private bool _wereHotkeysEnabledLastFrame;

	private bool _isResetPressed;

	private bool _isReadyPressed;

	private bool _isAnyHeroSelected;

	private bool _isClassSelectionEnabled;

	private float _cachedOrderTypeSetting;

	public MissionGauntletOrderOfBattleUIHandler(OrderOfBattleVM dataSource)
	{
		_dataSource = dataSource;
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("OrderOfBattleHotKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("OrderOfBattleHotKeyCategory").GetHotKey("AutoDeploy"));
		ViewOrderPriority = 13;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		base.OnMissionScreenInitialize();
		_playerRoleMissionController = ((MissionBehavior)this).Mission.GetMissionBehavior<AssignPlayerRoleInTeamMissionController>();
		_playerRoleMissionController.OnPlayerTurnToChooseFormationToLead += new PlayerTurnToChooseFormationToLeadEvent(OnPlayerTurnToChooseFormationToLead);
		_playerRoleMissionController.OnAllFormationsAssignedSergeants += new AllFormationsAssignedSergeantsEvent(OnAllFormationsAssignedSergeants);
		_orderUIHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletSingleplayerOrderUIHandler>();
		_orderTroopPlacer = ((MissionBehavior)this).Mission.GetMissionBehavior<OrderTroopPlacer>();
		OrderTroopPlacer orderTroopPlacer = _orderTroopPlacer;
		orderTroopPlacer.OnUnitDeployed = (Action)Delegate.Combine(orderTroopPlacer.OnUnitDeployed, new Action(OnUnitDeployed));
		_gauntletLayer = new GauntletLayer("MissionOrderOfBattle", ViewOrderPriority, false);
		_movie = _gauntletLayer.LoadMovie("OrderOfBattle", (ViewModel)(object)_dataSource);
		_orderOfBattleCategory = UIResourceManager.LoadSpriteCategory("ui_order_of_battle");
		((ScreenLayer)base.MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("OrderOfBattleHotKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("OrderOfBattleHotKeyCategory"));
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
	}

	public override bool IsReady()
	{
		if (!((MissionBehavior)this).Mission.IsDeploymentFinished)
		{
			return _orderOfBattleCategory.IsLoaded;
		}
		return true;
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (_isActive)
		{
			_dataSource.Tick();
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (_isActive)
		{
			_wereHotkeysEnabledLastFrame = _dataSource.AreHotkeysEnabled;
			HandleLayerFocus(out _isAnyHeroSelected, out _isClassSelectionEnabled);
			_dataSource.AreHotkeysEnabled = !base.MissionScreen.IsRadialMenuActive && !((MissionBehavior)this).Mission.IsOrderMenuOpen && Input.IsGamepadActive && !((ScreenLayer)_gauntletLayer).IsFocusLayer;
			TickInput();
		}
	}

	private void DestroyView()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		if (_gauntletLayer != null || _dataSource != null)
		{
			if (_isActive)
			{
				ManagedOptions.SetConfig((ManagedOptionsType)35, _cachedOrderTypeSetting);
			}
			_isActive = false;
			base.MissionScreen.SetDisplayDialog(value: false);
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
			((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			_gauntletLayer = null;
			_orderOfBattleCategory.Unload();
			_playerRoleMissionController.OnPlayerTurnToChooseFormationToLead -= new PlayerTurnToChooseFormationToLeadEvent(OnPlayerTurnToChooseFormationToLead);
			_playerRoleMissionController.OnAllFormationsAssignedSergeants -= new AllFormationsAssignedSergeantsEvent(OnAllFormationsAssignedSergeants);
			OrderTroopPlacer orderTroopPlacer = _orderTroopPlacer;
			orderTroopPlacer.OnUnitDeployed = (Action)Delegate.Remove(orderTroopPlacer.OnUnitDeployed, new Action(OnUnitDeployed));
		}
	}

	private void TickInput()
	{
		if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsKeyDown((InputKey)225) || ((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsKeyDown((InputKey)254))
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(false);
			_dataSource.AreCameraControlsEnabled = true;
		}
		else
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(true);
			_dataSource.AreCameraControlsEnabled = false;
		}
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			if (_isClassSelectionEnabled)
			{
				UISoundsHelper.PlayUISound("event:/ui/oob/dropdown");
				_dataSource.ExecuteDisableAllClassSelections();
			}
			else if (_isAnyHeroSelected && _dataSource.CanToggleHeroSelection)
			{
				UISoundsHelper.PlayUISound("event:/ui/oob/officer_pick");
				_dataSource.ExecuteClearHeroSelection();
			}
		}
		if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsHotKeyPressed("AutoDeploy"))
		{
			_isResetPressed = _dataSource.AreHotkeysEnabled && _wereHotkeysEnabledLastFrame;
		}
		if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsHotKeyPressed("Confirm"))
		{
			_isReadyPressed = _dataSource.AreHotkeysEnabled && _wereHotkeysEnabledLastFrame;
		}
		if (!_dataSource.AreHotkeysEnabled)
		{
			_isResetPressed = false;
			_isReadyPressed = false;
		}
		if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsHotKeyReleased("AutoDeploy") && _dataSource.AreHotkeysEnabled && _isResetPressed)
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteAutoDeploy();
		}
		if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsHotKeyReleased("Confirm") && _dataSource.AreHotkeysEnabled && _dataSource.CanStartMission && _isReadyPressed)
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteBeginMission();
		}
	}

	private void HandleLayerFocus(out bool isAnyHeroSelected, out bool isClassSelectionEnabled)
	{
		isAnyHeroSelected = _dataSource.HasSelectedHeroes;
		isClassSelectionEnabled = _dataSource.IsAnyClassSelectionEnabled();
		bool flag = isAnyHeroSelected | isClassSelectionEnabled;
		if (((ScreenLayer)_gauntletLayer).IsFocusLayer && !flag)
		{
			base.MissionScreen.SetDisplayDialog(value: false);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		}
		else if (!((ScreenLayer)_gauntletLayer).IsFocusLayer && flag)
		{
			base.MissionScreen.SetDisplayDialog(value: true);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
	}

	public override void OnMissionScreenFinalize()
	{
		DestroyView();
		base.OnMissionScreenFinalize();
	}

	public override bool OnEscape()
	{
		bool flag = false;
		if (_isActive)
		{
			bool flag2 = false;
			if (_orderUIHandler != null && _orderUIHandler.IsOrderMenuActive)
			{
				flag2 = _orderUIHandler.IsAnyOrderSetActive;
				flag = _orderUIHandler.OnEscape();
			}
			if (!flag2)
			{
				flag = _dataSource.OnEscape() || flag;
			}
		}
		return flag;
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}

	public override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return !_isActive;
	}

	private void OnPlayerTurnToChooseFormationToLead(Dictionary<int, Agent> lockedFormationIndicesAndSergeants, List<int> remainingFormationIndices)
	{
		if (((MissionBehavior)this).Mission.PlayerTeam == null)
		{
			Debug.FailedAssert("Player team must be initialized before OOB", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\Mission\\Singleplayer\\MissionGauntletOrderOfBattleUIHandler.cs", "OnPlayerTurnToChooseFormationToLead", 285);
		}
		_cachedOrderTypeSetting = ManagedOptions.GetConfig((ManagedOptionsType)35);
		ManagedOptions.SetConfig((ManagedOptionsType)35, 1f);
		_dataSource.Initialize(((MissionBehavior)this).Mission, base.MissionScreen.CombatCamera, (Action<int>)SelectFormationAtIndex, (Action<int>)DeselectFormationAtIndex, (Action)ClearFormationSelection, (Action)OnAutoDeploy, (Action)OnBeginMission, lockedFormationIndicesAndSergeants);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		_isActive = true;
	}

	private void OnAllFormationsAssignedSergeants(Dictionary<int, Agent> formationsWithLooselyAssignedSergeants)
	{
		_dataSource.OnAllFormationsAssignedSergeants(formationsWithLooselyAssignedSergeants);
	}

	public override void OnDeploymentFinished()
	{
		((MissionBehavior)this).OnDeploymentFinished();
		bool flag = MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle();
		_dataSource.OnDeploymentFinalized(flag);
		DestroyView();
	}

	private void SelectFormationAtIndex(int index)
	{
		_orderUIHandler?.SelectFormationAtIndex(index);
	}

	private void DeselectFormationAtIndex(int index)
	{
		_orderUIHandler?.DeselectFormationAtIndex(index);
	}

	private void ClearFormationSelection()
	{
		_orderUIHandler?.ClearFormationSelection();
	}

	private void OnAutoDeploy()
	{
		_orderUIHandler.OnAutoDeploy();
	}

	private void OnBeginMission()
	{
		_orderUIHandler.OnFiltersSet(_dataSource.CurrentConfiguration);
		_orderUIHandler.OnBeginMission();
	}

	private void OnUnitDeployed()
	{
		_dataSource.OnUnitDeployed();
	}
}
