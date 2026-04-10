using System.Collections.Generic;
using NavalDLC.Missions.Deployment;
using NavalDLC.Missions.Objects;
using NavalDLC.View.MissionViews;
using NavalDLC.ViewModelCollection.OrderOfBattle;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(NavalOrderOfBattleView))]
public class MissionGauntletNavalOrderOfBattleView : MissionView
{
	private NavalOrderOfBattleVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private SpriteCategory _orderOfBattleSpriteCategory;

	private MissionGauntletNavalOrderUIHandler _orderUIHandler;

	private NavalDeploymentMissionController _navalDeploymentController;

	private bool _isActive;

	private bool _wereHotkeysEnabledLastFrame;

	private bool _isResetPressed;

	private bool _isReadyPressed;

	private float _cachedOrderTypeSetting;

	public MissionGauntletNavalOrderOfBattleView(Mission mission)
	{
		_dataSource = new NavalOrderOfBattleVM(mission, OnFormationSelected, ClearFormationSelection, OnAutoDeploy, OnBeginMission);
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("OrderOfBattleHotKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("OrderOfBattleHotKeyCategory").GetHotKey("AutoDeploy"));
		base.ViewOrderPriority = 13;
	}

	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		InitializeView();
		_orderUIHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletNavalOrderUIHandler>();
		_navalDeploymentController = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalDeploymentMissionController>();
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (!_isActive && ((DeploymentMissionController)_navalDeploymentController).TeamSetupOver && !((MissionBehavior)this).Mission.IsDeploymentFinished)
		{
			_cachedOrderTypeSetting = ManagedOptions.GetConfig((ManagedOptionsType)35);
			ManagedOptions.SetConfig((ManagedOptionsType)35, 1f);
			_dataSource.Initialize();
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			_isActive = true;
		}
		if (_isActive)
		{
			UpdateFormationPositions();
			_wereHotkeysEnabledLastFrame = _dataSource.AreHotkeysEnabled;
			HandleLayerFocus();
			_dataSource.AreHotkeysEnabled = !((MissionView)this).MissionScreen.IsRadialMenuActive && !((MissionBehavior)this).Mission.IsOrderMenuOpen && Input.IsGamepadActive && !((ScreenLayer)_gauntletLayer).IsFocusLayer;
			TickInput();
		}
	}

	public override void OnDeploymentFinished()
	{
		((MissionBehavior)this).OnDeploymentFinished();
		DestroyView();
	}

	private void TickInput()
	{
		if (!_dataSource.IsAssignmentDirty)
		{
			if (((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsKeyDown((InputKey)225) || ((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsKeyDown((InputKey)254))
			{
				((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(false);
				_dataSource.AreCameraControlsEnabled = true;
			}
			else
			{
				((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(true);
				_dataSource.AreCameraControlsEnabled = false;
			}
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit") && (_dataSource.HasSelectedHero || _dataSource.HasSelectedShip) && _dataSource.CanToggleHeroOrShipSelection)
			{
				UISoundsHelper.PlayUISound("event:/ui/oob/officer_pick");
				_dataSource.ExecuteClearHeroAndShipSelection();
			}
			if (((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsHotKeyPressed("AutoDeploy"))
			{
				_isResetPressed = _dataSource.AreHotkeysEnabled && _wereHotkeysEnabledLastFrame;
			}
			if (((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsHotKeyPressed("Confirm"))
			{
				_isReadyPressed = _dataSource.AreHotkeysEnabled && _wereHotkeysEnabledLastFrame;
			}
			if (!_dataSource.AreHotkeysEnabled)
			{
				_isResetPressed = false;
				_isReadyPressed = false;
			}
			if (((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsHotKeyReleased("AutoDeploy") && _dataSource.AreHotkeysEnabled && _isResetPressed)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ExecuteAutoDeploy();
			}
			if (((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsHotKeyReleased("Confirm") && _dataSource.AreHotkeysEnabled && _dataSource.CanStartMission && _isReadyPressed)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ExecuteBeginMission();
			}
		}
	}

	private void HandleLayerFocus()
	{
		bool flag = _dataSource.HasSelectedHero || _dataSource.HasSelectedShip;
		if (((ScreenLayer)_gauntletLayer).IsFocusLayer && !flag)
		{
			((MissionView)this).MissionScreen.SetDisplayDialog(false);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		}
		else if (!((ScreenLayer)_gauntletLayer).IsFocusLayer && flag)
		{
			((MissionView)this).MissionScreen.SetDisplayDialog(true);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
	}

	private void UpdateFormationPositions()
	{
		if (!_dataSource.IsAssignmentDirty)
		{
			for (int i = 0; i < ((List<NavalOrderOfBattleFormationItemVM>)(object)_dataSource.AllFormations).Count; i++)
			{
				UpdateFormationPosition(((List<NavalOrderOfBattleFormationItemVM>)(object)_dataSource.AllFormations)[i]);
			}
		}
	}

	private void UpdateFormationPosition(NavalOrderOfBattleFormationItemVM formation)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (formation.HasShip)
		{
			MissionShip missionShip = formation.Ship.MissionShip;
			if (missionShip != null)
			{
				Vec3 val = missionShip.GlobalFrame.origin + Vec3.Up * 3f;
				float num = 0f;
				float num2 = 0f;
				float num3 = 0f;
				MBWindowManager.WorldToScreenInsideUsableArea(((MissionView)this).MissionScreen.CombatCamera, val, ref num, ref num2, ref num3);
				formation.ScreenPosition = new Vec2(num, num2 - 50f);
				formation.WSign = MathF.Sign(num3);
			}
		}
	}

	public override bool OnEscape()
	{
		bool flag = false;
		if (_isActive)
		{
			bool flag2 = false;
			if (_orderUIHandler != null && ((GauntletOrderUIHandler)_orderUIHandler).IsOrderMenuActive)
			{
				flag2 = ((GauntletOrderUIHandler)_orderUIHandler).IsAnyOrderSetActive;
				flag = ((MissionView)_orderUIHandler).OnEscape();
			}
			if (!flag2)
			{
				flag = _dataSource.OnEscape() || flag;
			}
		}
		return flag;
	}

	public override void OnMissionScreenFinalize()
	{
		DestroyView();
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return !_isActive;
	}

	public override void OnPhotoModeActivated()
	{
		((MissionView)this).OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		((MissionView)this).OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}

	private void InitializeView()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		_gauntletLayer = new GauntletLayer("NavalOrderOfBattle", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("NavalOrderOfBattle", (ViewModel)(object)_dataSource);
		_orderOfBattleSpriteCategory = UIResourceManager.LoadSpriteCategory("ui_order_of_battle");
		((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("OrderOfBattleHotKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("OrderOfBattleHotKeyCategory"));
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	private void DestroyView()
	{
		if (_gauntletLayer != null || _dataSource != null)
		{
			if (_isActive)
			{
				ManagedOptions.SetConfig((ManagedOptionsType)35, _cachedOrderTypeSetting);
			}
			_isActive = false;
			((MissionView)this).MissionScreen.SetDisplayDialog(false);
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
			((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			_gauntletLayer = null;
			_orderOfBattleSpriteCategory.Unload();
		}
	}

	private void OnFormationSelected(NavalOrderOfBattleFormationItemVM selectedFormation)
	{
		SelectFormationAtIndex(selectedFormation.Formation.Index);
	}

	private void SelectFormationAtIndex(int index)
	{
		MissionGauntletNavalOrderUIHandler orderUIHandler = _orderUIHandler;
		if (orderUIHandler != null)
		{
			((GauntletOrderUIHandler)orderUIHandler).SelectFormationAtIndex(index);
		}
	}

	private void DeselectFormationAtIndex(int index)
	{
		MissionGauntletNavalOrderUIHandler orderUIHandler = _orderUIHandler;
		if (orderUIHandler != null)
		{
			((GauntletOrderUIHandler)orderUIHandler).DeselectFormationAtIndex(index);
		}
	}

	private void ClearFormationSelection()
	{
		MissionGauntletNavalOrderUIHandler orderUIHandler = _orderUIHandler;
		if (orderUIHandler != null)
		{
			((MissionGauntletSingleplayerOrderUIHandler)orderUIHandler).ClearFormationSelection();
		}
	}

	private void OnAutoDeploy()
	{
		((MissionGauntletSingleplayerOrderUIHandler)_orderUIHandler).OnAutoDeploy();
	}

	private void OnBeginMission()
	{
		((MissionGauntletSingleplayerOrderUIHandler)_orderUIHandler).OnFiltersSet(_dataSource.CurrentFilterConfiguration);
		_orderUIHandler.OnClassesSet(_dataSource.CurrentClassConfiguration);
		((MissionGauntletSingleplayerOrderUIHandler)_orderUIHandler).OnBeginMission();
	}
}
