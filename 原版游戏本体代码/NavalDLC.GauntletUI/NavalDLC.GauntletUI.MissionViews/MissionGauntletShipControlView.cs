using System;
using System.Collections.Generic;
using System.Numerics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipInput;
using NavalDLC.View.MissionViews;
using NavalDLC.ViewModelCollection.Missions.ShipControl;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(MissionShipControlView))]
public class MissionGauntletShipControlView : MissionShipControlView
{
	[Flags]
	public enum ShipControlFeatureFlags
	{
		ShipFocus = 1,
		ShipSelection = 2,
		AttemptBoarding = 4,
		ToggleOarsmen = 8,
		ToggleSails = 0x10,
		CutLoose = 0x20,
		BallistaOrder = 0x40,
		ShootBallista = 0x80,
		ChangeCamera = 0x100
	}

	private GauntletLayer _gauntletLayer;

	private MissionShipControlVM _dataSource;

	private MissionGauntletSingleplayerOrderUIHandler _orderUIHandler;

	private MissionGauntletCrosshair _crosshairView;

	private NavalMissionShipHighlightView _shipHighlightView;

	private MissionGauntletNavalAgentStatus _agentStatusView;

	private MissionShip _playerControlledShip;

	private MissionShip _focusedShip;

	private bool _playerControlledShipHasHybridSails;

	private bool _isAnyBridgeActive;

	private bool _isBattleUIVisible;

	private bool _isPhotoModeActive;

	private bool _lastFirstPersonModeSelection;

	private const float AttemptBoardingDistance = 50f;

	private const float SelectShipDistance = 300f;

	public ShipControlFeatureFlags SuspendedFeatures { get; private set; }

	public override void OnMissionScreenInitialize()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		((MissionBattleUIBaseView)this).OnMissionScreenInitialize();
		_dataSource = new MissionShipControlVM();
		_gauntletLayer = new GauntletLayer("MissionShipControl", ((MissionView)this).ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MissionShipControl", (ViewModel)(object)_dataSource);
		_orderUIHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletSingleplayerOrderUIHandler>();
		_crosshairView = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletCrosshair>();
		_shipHighlightView = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalMissionShipHighlightView>();
		_agentStatusView = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletNavalAgentStatus>();
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)0);
		if (!((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsCategoryRegistered(HotKeyManager.GetCategory("NavalShipControlsHotKeyCategory")))
		{
			((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("NavalShipControlsHotKeyCategory"));
		}
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		SetControlKeys();
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionBattleUIBaseView)this).OnMissionScreenFinalize();
		((ViewModel)_dataSource).OnFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_dataSource = null;
		_gauntletLayer = null;
	}

	protected override void OnCreateView()
	{
		base.OnCreateView();
		_isBattleUIVisible = true;
	}

	protected override void OnDestroyView()
	{
		base.OnDestroyView();
		_isBattleUIVisible = false;
	}

	public void SuspendFeature(ShipControlFeatureFlags feature)
	{
		SuspendedFeatures |= feature;
	}

	public bool IsFeatureSuspended(ShipControlFeatureFlags feature)
	{
		return (SuspendedFeatures & feature) != 0;
	}

	public void ResumeFeature(ShipControlFeatureFlags feature)
	{
		SuspendedFeatures &= ~feature;
	}

	public override void OnPhotoModeActivated()
	{
		((MissionView)this).OnPhotoModeActivated();
		_isPhotoModeActive = true;
	}

	public override void OnPhotoModeDeactivated()
	{
		((MissionView)this).OnPhotoModeDeactivated();
		_isPhotoModeActive = false;
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionBattleUIBaseView)this).OnMissionScreenTick(dt);
		UpdateVisibility();
		MissionShip playerControlledShip = _playerControlledShip;
		_playerControlledShip = NavalShipsLogic?.PlayerControlledShip;
		_isAnyBridgeActive = _playerControlledShip?.GetIsAnyBridgeActive() ?? false;
		if (playerControlledShip != _playerControlledShip)
		{
			if (_playerControlledShip != null)
			{
				MissionGauntletCrosshair crosshairView = _crosshairView;
				if (crosshairView != null)
				{
					((MissionView)crosshairView).SuspendView();
				}
				_lastFirstPersonModeSelection = ((MissionBehavior)this).Mission.CameraIsFirstPerson;
				((MissionBehavior)this).Mission.CameraIsFirstPerson = false;
			}
			else
			{
				MissionGauntletCrosshair crosshairView2 = _crosshairView;
				if (crosshairView2 != null)
				{
					((MissionView)crosshairView2).ResumeView();
				}
				((MissionBehavior)this).Mission.CameraIsFirstPerson = _lastFirstPersonModeSelection;
				if (IsAimingWithRangedWeapon)
				{
					IsAimingWithRangedWeapon = false;
					playerControlledShip?.OnSetRangedWeaponControlMode(value: false);
				}
			}
		}
		if (_playerControlledShip != null && IsAimingWithRangedWeapon && !GetIsRangedWeaponAvailable())
		{
			IsAimingWithRangedWeapon = false;
			_playerControlledShip.OnSetRangedWeaponControlMode(value: false);
		}
		UpdateShipValues();
		RefreshControlKeys();
		UpdateFocusedShip();
		TickInput();
	}

	private void UpdateHitPoints()
	{
		if (_dataSource != null)
		{
			if (_playerControlledShip == null)
			{
				_dataSource.ShipHitPoints.IsRelevant = false;
				_dataSource.SailHitPoints.IsRelevant = false;
				_dataSource.FireHitPoints.IsRelevant = false;
				return;
			}
			_dataSource.ShipHitPoints.IsRelevant = true;
			_dataSource.SailHitPoints.IsRelevant = true;
			_dataSource.FireHitPoints.IsRelevant = true;
			_dataSource.ShipHitPoints.ActiveHitPoints = MathF.Round(_playerControlledShip.HitPoints);
			_dataSource.ShipHitPoints.MaxHitPoints = MathF.Round(_playerControlledShip.MaxHealth);
			_dataSource.SailHitPoints.ActiveHitPoints = MathF.Round(_playerControlledShip.SailHitPoints);
			_dataSource.SailHitPoints.MaxHitPoints = MathF.Round(_playerControlledShip.MaxSailHitPoints);
			_dataSource.FireHitPoints.ActiveHitPoints = MathF.Round(_playerControlledShip.FireHitPoints);
			_dataSource.FireHitPoints.MaxHitPoints = MathF.Round(_playerControlledShip.MaxFireHealth);
		}
	}

	private void TickInput()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Expected O, but got Unknown
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Invalid comparison between Unknown and I4
		MissionScreen missionScreen = ((MissionView)this).MissionScreen;
		object obj;
		if (missionScreen == null)
		{
			obj = null;
		}
		else
		{
			SceneLayer sceneLayer = missionScreen.SceneLayer;
			obj = ((sceneLayer != null) ? ((ScreenLayer)sceneLayer).Input : null);
		}
		InputContext val = (InputContext)obj;
		if (val == null || _playerControlledShip == null || ((MissionView)this).MissionScreen.IsPhotoModeEnabled || base.IsDisplayingADialog || ((MissionView)this).MissionScreen.IsCheatGhostMode)
		{
			return;
		}
		if (val.IsGameKeyReleased(111))
		{
			if (GetCanToggleOarsmen())
			{
				int num = (_playerControlledShip.ShipOrder.OarsmenLevel + 2) % 3;
				_playerControlledShip.ShipOrder.SetOrderOarsmenLevel(num);
				TextObject val2 = null;
				switch (num)
				{
				case 0:
					val2 = new TextObject("{=RtRNkfMA}Stop using the oars!", (Dictionary<string, object>)null);
					break;
				case 1:
					val2 = new TextObject("{=a7CzRLXb}Use oars in half power!", (Dictionary<string, object>)null);
					break;
				case 2:
					val2 = new TextObject("{=RKthVuaC}Use oars in full power!", (Dictionary<string, object>)null);
					break;
				}
				if (val2 != (TextObject)null)
				{
					DisplayCommandForSelectedFormations(val2);
				}
			}
			else if (GetCanCutLoose() && !GetIsCutLooseTemporarilyBlocked())
			{
				_playerControlledShip.ShipOrder.SetCutLoose(enable: true);
				DisplayCommandForSelectedFormations(new TextObject("{=siE18G0C}Cut loose!", (Dictionary<string, object>)null));
			}
		}
		if (val.IsGameKeyReleased(110) && GetCanToggleSail())
		{
			SailControl = (SailControl.IsMax() ? SailControl.Min(_playerControlledShipHasHybridSails) : SailControl.Raise(_playerControlledShipHasHybridSails));
			switch (SailControl)
			{
			case SailInput.Raised:
				DisplayCommandForSelectedFormations(new TextObject("{=kWfyfiVA}Furl sails!", (Dictionary<string, object>)null));
				break;
			case SailInput.SquareSailsRaised:
				DisplayCommandForSelectedFormations(new TextObject("{=kGtL9Kea}Furl square sails!", (Dictionary<string, object>)null));
				break;
			case SailInput.Full:
				DisplayCommandForSelectedFormations(new TextObject("{=75VaP7bL}Open sails!", (Dictionary<string, object>)null));
				break;
			}
		}
		if (val.IsGameKeyReleased(112) && GetCanChangeCamera())
		{
			base.ActiveCameraMode = (CameraModes)((int)(base.ActiveCameraMode + 1) % 3);
		}
		if (val.IsGameKeyReleased(113) && GetCanSelectShip())
		{
			int num2 = _focusedShip.Formation?.Index ?? (-1);
			if (num2 >= 0)
			{
				((GauntletOrderUIHandler)_orderUIHandler).SelectFormationAtIndex(num2);
			}
		}
		if (val.IsGameKeyReleased(114) && GetCanAttemptBoarding())
		{
			if (GetIsCancelBoardingAvailable())
			{
				_playerControlledShip.ShipOrder.SetBoardingTargetShip(null);
				DisplayCommandForSelectedFormations(new TextObject("{=U6Z4GFPW}Stop boarding!", (Dictionary<string, object>)null));
			}
			else if (!GetIsAttemptBoardingTemporarilyBlocked())
			{
				_playerControlledShip.ShipOrder.SetBoardingTargetShip(_focusedShip);
				DisplayCommandForSelectedFormations(new TextObject("{=HSALr4nl}Board {SHIP_NAME}!", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((int)_focusedShip.Team.TeamSide == 2) ? _focusedShip.ShipOrigin.Hull.Name : _focusedShip.ShipOrigin.Name));
			}
		}
		if (val.IsGameKeyReleased(115) && GetCanToggleRangedWeaponOrderMode())
		{
			IsAimingWithRangedWeapon = !IsAimingWithRangedWeapon;
			_playerControlledShip.OnSetRangedWeaponControlMode(IsAimingWithRangedWeapon);
		}
		if (val.IsGameKeyReleased(9) && GetCanShootBallista())
		{
			_playerControlledShip.ShootBallista();
		}
	}

	private void DisplayCommandForSelectedFormations(TextObject command)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		TextObject val = new TextObject("{=ApD0xQXT}{STR1}: {STR2}", (Dictionary<string, object>)null);
		MissionShip playerControlledShip = _playerControlledShip;
		object obj;
		if (playerControlledShip == null)
		{
			obj = null;
		}
		else
		{
			IShipOrigin shipOrigin = playerControlledShip.ShipOrigin;
			obj = ((shipOrigin != null) ? shipOrigin.Name : null);
		}
		if (obj == null)
		{
			obj = (object)new TextObject("{=wXCM8BnW}Crew", (Dictionary<string, object>)null);
		}
		val.SetTextVariable("STR1", (TextObject)obj);
		val.SetTextVariable("STR2", command);
		InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
	}

	private void UpdateFocusedShip()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)((MissionBehavior)this).Mission.Scene == (NativeObject)null || _playerControlledShip == null || ((MissionView)this).MissionScreen.IsPhotoModeEnabled || base.IsDisplayingADialog || IsFeatureSuspended(ShipControlFeatureFlags.ShipFocus))
		{
			_dataSource?.SetTargetedShip(null);
			SetFocusedShip(null);
			_dataSource.SetBoardingTargetShip(null);
			return;
		}
		MatrixFrame lastFinalRenderCameraFrame = ((MissionBehavior)this).Mission.Scene.LastFinalRenderCameraFrame;
		Vec2 screenCenter = Screen.RealScreenResolution * 0.5f;
		float closestDistance = float.MaxValue;
		float focusRadius = Screen.RealScreenResolutionHeight / 4f;
		MissionShip closestShip = null;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_playerControlledShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec3 hitScreenPosition = Vec3.Zero;
		for (int i = 0; i < ((List<MissionShip>)(object)NavalShipsLogic.AllShips).Count; i++)
		{
			CheckFocusableShip(((List<MissionShip>)(object)NavalShipsLogic.AllShips)[i], globalPosition, 100f, 350f, lastFinalRenderCameraFrame, screenCenter, ref hitScreenPosition, ref closestDistance, focusRadius, ref closestShip, out var directHitFound);
			if (directHitFound)
			{
				break;
			}
		}
		SetFocusedShip(closestShip);
		if (_dataSource != null)
		{
			_dataSource.SetTargetedShip(closestShip, hitScreenPosition.x, hitScreenPosition.y - 70f, hitScreenPosition.z);
			_dataSource.TargetedShipHasAction = !MBCommon.IsPaused && (GetCanAttemptBoarding() || GetCanSelectShip());
			_dataSource.IsCancelBoardingOrderAvailable = GetIsCancelBoardingAvailable();
		}
	}

	private void CheckFocusableShip(MissionShip focusableShip, Vec3 playerShipPosition, float enemyFocusDistance, float friendlyFocusDistance, MatrixFrame cameraFrame, Vec2 screenCenter, ref Vec3 hitScreenPosition, ref float closestDistance, float focusRadius, ref MissionShip closestShip, out bool directHitFound)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		directHitFound = false;
		if (((MissionObject)focusableShip).IsDisabled || focusableShip.IsSinking || focusableShip == _playerControlledShip)
		{
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)focusableShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		if ((focusableShip.BattleSide == ((MissionBehavior)this).Mission.PlayerEnemyTeam.Side && ((Vec3)(ref globalPosition)).DistanceSquared(playerShipPosition) > enemyFocusDistance * enemyFocusDistance) || (focusableShip.BattleSide == ((MissionBehavior)this).Mission.PlayerTeam.Side && ((Vec3)(ref globalPosition)).DistanceSquared(playerShipPosition) > friendlyFocusDistance * friendlyFocusDistance))
		{
			return;
		}
		Vec3 shipFocusPosition = GetShipFocusPosition(focusableShip);
		float num = -5000f;
		float num2 = -5000f;
		float num3 = -5000f;
		MBWindowManager.WorldToScreenInsideUsableArea(((MissionView)this).MissionScreen.CombatCamera, shipFocusPosition, ref num, ref num2, ref num3);
		float num4 = 0f;
		gameEntity = ((ScriptComponentBehavior)focusableShip).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).RayHitEntity(cameraFrame.origin, -cameraFrame.rotation.u, friendlyFocusDistance, ref num4))
		{
			hitScreenPosition = new Vec3(num, num2, num3, -1f);
			closestShip = focusableShip;
			directHitFound = true;
			return;
		}
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(num, num2);
		float num5 = ((Vec2)(ref val)).Distance(screenCenter);
		if (num3 > 0f && num5 < closestDistance && ((Vec2)(ref screenCenter)).DistanceSquared(val) < focusRadius * focusRadius)
		{
			closestShip = focusableShip;
			closestDistance = num5;
			hitScreenPosition = new Vec3(num, num2, num3, -1f);
		}
	}

	private void SetFocusedShip(MissionShip ship)
	{
		_focusedShip = ship;
		_shipHighlightView?.OnShipFocused(ship);
	}

	private Vec3 GetShipFocusPosition(MissionShip ship)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
		return ((WeakGameEntity)(ref gameEntity)).GlobalPosition + Vec3.Up * 3f;
	}

	private void UpdateShipValues()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Invalid comparison between Unknown and I4
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		if (_dataSource != null)
		{
			_dataSource.IsControllingShip = _playerControlledShip != null;
			_dataSource.IsUsingBallistaRemotely = base.IsAimingWithRangedWeaponAndAllowed;
			_dataSource.IsUsingBallistaDirectly = base.DirectlyControlledRangedSiegeWeapon != null;
			if (base.RangedSiegeWeapon != null || base.DirectlyControlledRangedSiegeWeapon != null)
			{
				MissionShipControlVM dataSource = _dataSource;
				RangedSiegeWeapon rangedSiegeWeapon = base.RangedSiegeWeapon;
				dataSource.BallistaAmmoCount = ((rangedSiegeWeapon != null) ? rangedSiegeWeapon.AmmoCount : base.DirectlyControlledRangedSiegeWeapon.AmmoCount);
				_dataSource.IsAmmoCountWarned = _dataSource.BallistaAmmoCount <= 3;
			}
		}
		if (_playerControlledShip == null || (NativeObject)(object)((MissionBehavior)this).Mission.Scene == (NativeObject)null || _dataSource == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		bool flag4 = true;
		foreach (MissionSail item in (List<MissionSail>)(object)_playerControlledShip.Sails)
		{
			if ((int)item.SailObject.Type == 1)
			{
				flag = true;
				if (item.TargetSailSetting <= 0f)
				{
					flag4 = false;
				}
			}
			else if ((int)item.SailObject.Type == 0)
			{
				flag2 = true;
				if (item.TargetSailSetting <= 0f)
				{
					flag3 = false;
				}
			}
		}
		_playerControlledShipHasHybridSails = flag && flag2;
		if (_playerControlledShipHasHybridSails)
		{
			if (flag4 && flag3)
			{
				_dataSource.SetSailState(SailInput.Full);
			}
			else if (!flag4 && !flag3)
			{
				_dataSource.SetSailState(SailInput.Raised);
			}
			else
			{
				_dataSource.SetSailState(SailInput.SquareSailsRaised);
			}
		}
		else if (flag)
		{
			_dataSource.SetSailState(flag4 ? SailInput.Full : SailInput.Raised);
		}
		else
		{
			_dataSource.SetSailState(flag3 ? SailInput.Full : SailInput.Raised);
		}
		_dataSource.SetOarsmanLevel(_playerControlledShip.ShipOrder.OarsmenLevel);
		_dataSource.SetSailType(flag, flag2);
		Vec2 val = ((MissionBehavior)this).Mission.Scene.GetGlobalWindStrengthVector();
		Vec2 to = ((Vec2)(ref val)).Normalized();
		MatrixFrame globalFrame = _playerControlledShip.GlobalFrame;
		val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Vec2 val2 = ((Vec2)(ref val)).Normalized();
		MissionShipControlVM dataSource2 = _dataSource;
		val = GetProjection(val2, to);
		dataSource2.ProjectedWindDirection = ((Vec2)(ref val)).Normalized();
		UpdateHitPoints();
		MissionShipControlVM dataSource3 = _dataSource;
		MissionShip playerControlledShip = _playerControlledShip;
		dataSource3.IsCutLooseOrderActive = playerControlledShip != null && playerControlledShip.ShipOrder.GetIsCuttingLoose() && _isAnyBridgeActive;
		_dataSource.IsAttemptBoardingOrderActive = _playerControlledShip?.ShipOrder.GetIsAttemptingBoarding() ?? false;
		if (_dataSource.IsAttemptBoardingOrderActive)
		{
			MissionShip missionShip = _playerControlledShip?.ShipOrder.GetBoardingTargetShip();
			if (missionShip != null)
			{
				Vec3 shipFocusPosition = GetShipFocusPosition(missionShip);
				float screenX = -5000f;
				float num = -5000f;
				float screenW = -5000f;
				MBWindowManager.WorldToScreenInsideUsableArea(((MissionView)this).MissionScreen.CombatCamera, shipFocusPosition, ref screenX, ref num, ref screenW);
				_dataSource.SetBoardingTargetShip(missionShip, screenX, num - 70f, screenW);
			}
			else
			{
				_dataSource.SetBoardingTargetShip(null);
			}
		}
		else
		{
			_dataSource.SetBoardingTargetShip(null);
		}
	}

	private static Vec2 GetProjection(Vec2 from, Vec2 to)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = ((Vec2)(ref from)).Normalized();
		Vec2 val2 = default(Vec2);
		((Vec2)(ref val2))._002Ector(0f - val.y, val.x);
		return Vec2.op_Implicit(new Vector2(Vec2.DotProduct(to, val), Vec2.DotProduct(to, val2)));
	}

	private void SetControlKeys()
	{
		GameKeyContext category = HotKeyManager.GetCategory("NavalShipControlsHotKeyCategory");
		GameKeyContext category2 = HotKeyManager.GetCategory("CombatHotKeyCategory");
		_dataSource.SetChangeCameraKey(category.GetGameKey(112));
		_dataSource.SetCutLooseKey(category.GetGameKey(111));
		_dataSource.SetToggleOarsmenKey(category.GetGameKey(111));
		_dataSource.SetToggleSailKey(category.GetGameKey(110));
		_dataSource.SetToggleBallistaKey(category.GetGameKey(115));
		_dataSource.SetAttemptBoardingKey(category.GetGameKey(114));
		_dataSource.SetStopUsingShipKey(category2.GetGameKey(13));
	}

	private void RefreshControlKeys()
	{
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		if (_playerControlledShip == null || ((MissionView)this).MissionScreen.IsPhotoModeEnabled || base.IsDisplayingADialog)
		{
			if (_dataSource != null)
			{
				_dataSource.ChangeCameraKey.IsVisible = false;
				_dataSource.CutLooseKey.IsVisible = false;
				_dataSource.ToggleOarsmenKey.IsVisible = false;
				_dataSource.ToggleSailKey.IsVisible = false;
				_dataSource.ToggleBallistaKey.IsVisible = false;
				_dataSource.AttemptBoardingKey.IsVisible = false;
				_dataSource.StopUsingShipKey.IsVisible = false;
			}
			_agentStatusView?.UpdateShipInteractionTexts(null);
			return;
		}
		if (_dataSource != null)
		{
			_dataSource.ChangeCameraKey.IsVisible = GetCanChangeCamera();
			_dataSource.CutLooseKey.IsVisible = GetCanCutLoose();
			_dataSource.CutLooseKey.IsDisabled = GetIsCutLooseTemporarilyBlocked();
			_dataSource.ToggleOarsmenKey.IsVisible = GetCanToggleOarsmen();
			_dataSource.ToggleSailKey.IsVisible = GetCanToggleSail();
			_dataSource.ToggleBallistaKey.IsVisible = GetCanToggleRangedWeaponOrderMode();
			_dataSource.AttemptBoardingKey.IsVisible = GetCanAttemptBoarding();
			_dataSource.AttemptBoardingKey.IsDisabled = !GetIsCancelBoardingAvailable() && GetIsAttemptBoardingTemporarilyBlocked();
			_dataSource.StopUsingShipKey.IsVisible = true;
		}
		MissionGauntletNavalAgentStatus agentStatusView = _agentStatusView;
		if (agentStatusView != null)
		{
			IShipOrigin origin = _focusedShip?.ShipOrigin;
			MissionShip focusedShip = _focusedShip;
			int isEnemy;
			if (focusedShip == null)
			{
				isEnemy = 0;
			}
			else
			{
				Team team = focusedShip.Team;
				isEnemy = ((((team != null) ? new TeamSideEnum?(team.TeamSide) : ((TeamSideEnum?)null)) == (TeamSideEnum?)2) ? 1 : 0);
			}
			agentStatusView.UpdateShipInteractionTexts(origin, (byte)isEnemy != 0, GetCanSelectShip(), GetCanAttemptBoarding(), GetIsAttemptBoardingTemporarilyBlocked(), GetIsCancelBoardingAvailable());
		}
	}

	private bool GetCanAttemptBoarding()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (IsFeatureSuspended(ShipControlFeatureFlags.AttemptBoarding))
		{
			return false;
		}
		if (_focusedShip != null && !_focusedShip.IsConnectionPermanentlyBlocked() && _focusedShip.ShipOrder.IsBoardingAvailable && !_playerControlledShip.GetIsThereActiveBridgeTo(_focusedShip))
		{
			WeakGameEntity gameEntity;
			Vec3 globalPosition;
			bool num;
			if (!GetIsCancelBoardingAvailable())
			{
				gameEntity = ((ScriptComponentBehavior)_focusedShip).GameEntity;
				globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				gameEntity = ((ScriptComponentBehavior)_playerControlledShip).GameEntity;
				num = ((Vec3)(ref globalPosition)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) <= 50f;
			}
			else
			{
				gameEntity = ((ScriptComponentBehavior)_focusedShip).GameEntity;
				globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				gameEntity = ((ScriptComponentBehavior)_playerControlledShip).GameEntity;
				num = ((Vec3)(ref globalPosition)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) <= 300f;
			}
			if (num)
			{
				return !base.IsAimingWithRangedWeaponAndAllowed;
			}
		}
		return false;
	}

	private bool GetIsAttemptBoardingTemporarilyBlocked()
	{
		MissionShip focusedShip = _focusedShip;
		if (focusedShip == null || !focusedShip.IsConnectionBlocked())
		{
			return _playerControlledShip.ShipOrder.GetBoardingTargetShip() == _focusedShip;
		}
		return true;
	}

	private bool GetIsCancelBoardingAvailable()
	{
		MissionShip playerControlledShip = _playerControlledShip;
		if (playerControlledShip != null && playerControlledShip.ShipOrder.GetIsAttemptingBoarding())
		{
			return _playerControlledShip.ShipOrder.GetBoardingTargetShip() == _focusedShip;
		}
		return false;
	}

	private bool GetCanChangeCamera()
	{
		if (IsFeatureSuspended(ShipControlFeatureFlags.ChangeCamera))
		{
			return false;
		}
		return !base.IsAimingWithRangedWeaponAndAllowed;
	}

	private bool GetCanCutLoose()
	{
		if (IsFeatureSuspended(ShipControlFeatureFlags.CutLoose))
		{
			return false;
		}
		return _isAnyBridgeActive;
	}

	private bool GetIsCutLooseTemporarilyBlocked()
	{
		if (!_playerControlledShip.ShipOrder.GetIsCuttingLoose())
		{
			return _playerControlledShip.IsDisconnectionBlocked();
		}
		return true;
	}

	private bool GetCanSelectShip()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (IsFeatureSuspended(ShipControlFeatureFlags.ShipSelection))
		{
			return false;
		}
		if (_orderUIHandler != null && _focusedShip?.Formation != null && _focusedShip.Formation.CountOfUnits > 0 && _focusedShip.Team.IsPlayerTeam && _focusedShip.Formation.PlayerOwner == Agent.Main)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_focusedShip).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			gameEntity = ((ScriptComponentBehavior)_playerControlledShip).GameEntity;
			if (((Vec3)(ref globalPosition)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) <= 300f)
			{
				return !base.IsAimingWithRangedWeaponAndAllowed;
			}
		}
		return false;
	}

	private bool GetCanToggleOarsmen()
	{
		if (IsFeatureSuspended(ShipControlFeatureFlags.ToggleOarsmen))
		{
			return false;
		}
		if (!_isAnyBridgeActive)
		{
			return !_playerControlledShip.ShipOrder.IsOarsmenLevelLocked();
		}
		return false;
	}

	private bool GetCanToggleSail()
	{
		if (IsFeatureSuspended(ShipControlFeatureFlags.ToggleSails))
		{
			return false;
		}
		if (!_isAnyBridgeActive)
		{
			return _playerControlledShip.ShipSailState == MissionShip.SailState.Intact;
		}
		return false;
	}

	private bool GetCanToggleRangedWeaponOrderMode()
	{
		if (GetIsRangedWeaponAvailable())
		{
			return base.IsAimingWithRangedWeaponAllowed;
		}
		return false;
	}

	private bool GetIsRangedWeaponAvailable()
	{
		if (IsFeatureSuspended(ShipControlFeatureFlags.BallistaOrder))
		{
			return false;
		}
		if (_playerControlledShip.ShipSiegeWeapon != null && !((MissionObject)_playerControlledShip.ShipSiegeWeapon).IsDisabled && !((UsableMachine)_playerControlledShip.ShipSiegeWeapon).IsDeactivated)
		{
			return !((UsableMachine)_playerControlledShip.ShipSiegeWeapon).IsDestroyed;
		}
		return false;
	}

	private bool GetCanShootBallista()
	{
		if (IsFeatureSuspended(ShipControlFeatureFlags.ShootBallista))
		{
			return false;
		}
		if (base.IsAimingWithRangedWeaponAndAllowed && _playerControlledShip.ShipSiegeWeapon != null)
		{
			return ((UsableMachine)_playerControlledShip.ShipSiegeWeapon).UserCountNotInStruckAction > 0;
		}
		return false;
	}

	private void UpdateVisibility()
	{
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = ((_isBattleUIVisible && !_isPhotoModeActive && !((MissionView)this).IsViewSuspended) ? 1 : 0);
		}
	}
}
