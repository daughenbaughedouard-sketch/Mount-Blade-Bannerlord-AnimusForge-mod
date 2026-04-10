using System;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Missions.ShipControl;
using NavalDLC.Missions.ShipInput;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.SiegeWeapon;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace NavalDLC.View.MissionViews;

public class MissionShipControlView : MissionBattleUIBaseView
{
	public enum CameraModes
	{
		Back,
		Shoulder,
		Front,
		NumPositions
	}

	protected SailInput SailControl;

	protected NavalShipsLogic NavalShipsLogic;

	private Vec3 _lastCameraOffset;

	private float _lastCameraFovMultiplier = 1f;

	private bool _wasOrderMenuOpenLastFrame;

	protected bool IsAimingWithRangedWeapon;

	private float _backCameraDistanceMultiplier = 1f;

	private float _lastForwardKeyPressTime;

	private float _lastBackwardKeyPressTime;

	private int _lastAccelerationAxisInput;

	public CameraModes ActiveCameraMode { get; protected set; }

	public ShipControllerMachine ControllerMachine { get; private set; }

	protected bool IsAimingWithRangedWeaponAndAllowed
	{
		get
		{
			if (IsAimingWithRangedWeapon)
			{
				return IsAimingWithRangedWeaponAllowed;
			}
			return false;
		}
	}

	protected bool IsAimingWithRangedWeaponAllowed
	{
		get
		{
			if (!((MissionBehavior)this).Mission.IsOrderMenuOpen && !_wasOrderMenuOpenLastFrame && RangedSiegeWeapon != null && !((MissionObject)RangedSiegeWeapon).IsDisabled)
			{
				return !((UsableMachine)RangedSiegeWeapon).IsDestroyed;
			}
			return false;
		}
	}

	protected bool IsDisplayingADialog
	{
		get
		{
			MissionScreen missionScreen = ((MissionView)this).MissionScreen;
			if (missionScreen == null || !((IMissionScreen)missionScreen).GetDisplayDialog())
			{
				MissionScreen missionScreen2 = ((MissionView)this).MissionScreen;
				if (missionScreen2 == null || !missionScreen2.IsRadialMenuActive)
				{
					return ((MissionBehavior)this).Mission?.IsOrderMenuOpen ?? false;
				}
			}
			return true;
		}
	}

	protected RangedSiegeWeapon RangedSiegeWeapon { get; private set; }

	protected RangedSiegeWeapon DirectlyControlledRangedSiegeWeapon { get; private set; }

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		NavalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
	}

	public override void OnPreMissionTick(float dt)
	{
		((MissionBehavior)this).OnPreMissionTick(dt);
		HandleShipControls(dt);
		HandleShipCamera(dt);
	}

	public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		StandingPoint standingPoint;
		if (!userAgent.IsMainAgent || (standingPoint = (StandingPoint)(object)((usedObject is StandingPoint) ? usedObject : null)) == null)
		{
			return;
		}
		UsableMachine usableMachineFromPoint = GetUsableMachineFromPoint(standingPoint);
		WeakGameEntity val;
		RangedSiegeWeapon val2;
		if (usableMachineFromPoint is ShipControllerMachine shipControllerMachine)
		{
			ControllerMachine = shipControllerMachine;
			val = ((ScriptComponentBehavior)shipControllerMachine).GameEntity;
			RangedSiegeWeapon firstScriptInFamilyDescending = MBExtensions.GetFirstScriptInFamilyDescending<RangedSiegeWeapon>(((WeakGameEntity)(ref val)).Root);
			if (firstScriptInFamilyDescending != null)
			{
				RangedSiegeWeapon = firstScriptInFamilyDescending;
			}
		}
		else if ((val2 = (RangedSiegeWeapon)(object)((usableMachineFromPoint is RangedSiegeWeapon) ? usableMachineFromPoint : null)) != null)
		{
			val = ((ScriptComponentBehavior)val2).GameEntity;
			val = ((WeakGameEntity)(ref val)).Root;
			MissionShip firstScriptOfType;
			if ((firstScriptOfType = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<MissionShip>()) != null)
			{
				DirectlyControlledRangedSiegeWeapon = val2;
				firstScriptOfType.OnSetRangedWeaponControlMode(value: true);
			}
		}
	}

	public override void OnObjectStoppedBeingUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		StandingPoint standingPoint;
		if (!userAgent.IsMainAgent || (standingPoint = (StandingPoint)(object)((usedObject is StandingPoint) ? usedObject : null)) == null)
		{
			return;
		}
		UsableMachine usableMachineFromPoint = GetUsableMachineFromPoint(standingPoint);
		RangedSiegeWeapon val;
		if (usableMachineFromPoint is ShipControllerMachine)
		{
			RangedSiegeWeapon rangedSiegeWeapon = RangedSiegeWeapon;
			if (rangedSiegeWeapon != null)
			{
				rangedSiegeWeapon.SetPlayerForceUse(false);
			}
			ControllerMachine = null;
			RangedSiegeWeapon = null;
			((MissionBehavior)this).Mission.SetListenerAndAttenuationPosBlendFactor(0f);
		}
		else if ((val = (RangedSiegeWeapon)(object)((usableMachineFromPoint is RangedSiegeWeapon) ? usableMachineFromPoint : null)) != null)
		{
			WeakGameEntity val2 = ((ScriptComponentBehavior)val).GameEntity;
			val2 = ((WeakGameEntity)(ref val2)).Root;
			MissionShip firstScriptOfType;
			if ((firstScriptOfType = ((WeakGameEntity)(ref val2)).GetFirstScriptOfType<MissionShip>()) != null)
			{
				DirectlyControlledRangedSiegeWeapon = null;
				firstScriptOfType.OnSetRangedWeaponControlMode(value: false);
			}
		}
	}

	private static UsableMachine GetUsableMachineFromPoint(StandingPoint standingPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)standingPoint).GameEntity;
		while (((WeakGameEntity)(ref val)).IsValid && !((WeakGameEntity)(ref val)).HasScriptOfType<UsableMachine>())
		{
			val = ((WeakGameEntity)(ref val)).Parent;
		}
		if (((WeakGameEntity)(ref val)).IsValid)
		{
			UsableMachine firstScriptOfType = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<UsableMachine>();
			if (firstScriptOfType != null)
			{
				return firstScriptOfType;
			}
		}
		return null;
	}

	private void TickRowerInput(Vec2 inputVec, out RowerLongitudinalInput longitudinalRowerControl, out RowerLongitudinalInput longitudinalControlDoubleTap, out RowerLateralInput lateralRowerControl)
	{
		int num = 0;
		int num2 = 0;
		if (((Vec2)(ref inputVec)).LengthSquared > 0f)
		{
			((Vec2)(ref inputVec)).Normalize();
			float num3 = MBMath.ToDegrees(((Vec2)(ref inputVec)).RotationInRadians);
			bool flag = false;
			if (num3 < 0f)
			{
				flag = true;
				num3 = 0f - num3;
			}
			if (num3 <= 22.5f)
			{
				num = 1;
			}
			else if (num3 <= 67.5f)
			{
				num = 1;
				num2 = 1;
			}
			else if (num3 <= 112.5f)
			{
				num2 = 1;
			}
			else if (num3 < 157.5f)
			{
				num = -1;
				num2 = 1;
			}
			else
			{
				num = -1;
			}
			if (flag)
			{
				num2 = -num2;
			}
		}
		bool flag2 = num == 1 && _lastAccelerationAxisInput == 1;
		bool flag3 = num == -1 && _lastAccelerationAxisInput == -1;
		_lastAccelerationAxisInput = num;
		bool flag4 = false;
		bool flag5 = false;
		longitudinalRowerControl = (RowerLongitudinalInput)num;
		longitudinalControlDoubleTap = RowerLongitudinalInput.None;
		if (num == 1)
		{
			if (flag2 && _lastForwardKeyPressTime + 0.3f > Time.ApplicationTime)
			{
				longitudinalControlDoubleTap = RowerLongitudinalInput.Forward;
				flag4 = true;
			}
		}
		else if (num == -1 && flag3 && _lastBackwardKeyPressTime + 0.3f > Time.ApplicationTime)
		{
			longitudinalControlDoubleTap = RowerLongitudinalInput.Backward;
			flag5 = true;
		}
		lateralRowerControl = (RowerLateralInput)num2;
		if (!flag4 && flag2)
		{
			_lastForwardKeyPressTime = Time.ApplicationTime;
		}
		if (!flag5 && flag3)
		{
			_lastBackwardKeyPressTime = Time.ApplicationTime;
		}
	}

	private float TickRudderInput(Vec2 inputVec)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return MathF.Min(MathF.Abs(inputVec.x) * 1.4f, 1f) * (float)MathF.Sign(inputVec.x);
	}

	private void HandleShipControls(float dt)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		_wasOrderMenuOpenLastFrame = ((MissionBehavior)this).Mission.IsOrderMenuOpen;
		MissionShip missionShip = NavalShipsLogic?.PlayerControlledShip;
		if (missionShip == null || !missionShip.IsPlayerControlled)
		{
			return;
		}
		PlayerShipController playerController = missionShip.PlayerController;
		RowerLongitudinalInput longitudinalRowerControl = RowerLongitudinalInput.None;
		RowerLongitudinalInput longitudinalControlDoubleTap = RowerLongitudinalInput.None;
		RowerLateralInput lateralRowerControl = RowerLateralInput.None;
		float rudderLateral = 0f;
		if (!((MissionView)this).MissionScreen.IsCheatGhostMode)
		{
			float gameKeyAxis = ((MissionView)this).Input.GetGameKeyAxis("MovementAxisY");
			float gameKeyAxis2 = ((MissionView)this).Input.GetGameKeyAxis("MovementAxisX");
			Vec2 val = default(Vec2);
			((Vec2)(ref val))._002Ector(gameKeyAxis2, gameKeyAxis);
			if (MathF.Abs(val.x) <= 0.2f)
			{
				val.x = 0f;
			}
			if (MathF.Abs(val.y) <= 0.2f)
			{
				val.y = 0f;
			}
			TickRowerInput(val, out longitudinalRowerControl, out longitudinalControlDoubleTap, out lateralRowerControl);
			rudderLateral = TickRudderInput(val);
		}
		playerController.SetInput(new ShipInputRecord(lateralRowerControl, longitudinalRowerControl, longitudinalControlDoubleTap, rudderLateral, SailControl));
	}

	public void SetSailInput(SailInput sailInput)
	{
		SailControl = sailInput;
	}

	public void SetActiveCameraMode(CameraModes mode)
	{
		ActiveCameraMode = mode;
	}

	private void HandleShipCamera(float dt)
	{
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		if (ControllerMachine != null)
		{
			if (RangedSiegeWeapon != null)
			{
				RangedSiegeWeaponView component = ((UsableMachine)RangedSiegeWeapon).GetComponent<RangedSiegeWeaponView>();
				if (component == null)
				{
					component = (RangedSiegeWeaponView)new BallistaView();
					component.Initialize(RangedSiegeWeapon, ((MissionView)this).MissionScreen);
					((UsableMachine)RangedSiegeWeapon).AddComponent((UsableMissionObjectComponent)(object)component);
				}
				RangedSiegeWeapon.SetPlayerForceUse(IsAimingWithRangedWeaponAndAllowed);
			}
			Agent pilotAgent = ((UsableMachine)ControllerMachine).PilotAgent;
			Vec3 val;
			float num;
			Vec3 val2 = default(Vec3);
			switch (ActiveCameraMode)
			{
			case CameraModes.Back:
			{
				val = ControllerMachine.BackCameraOffset * 0.5f;
				num = ControllerMachine.BackCameraFovMultiplier;
				if (((MissionBehavior)this).Mission.InputManager.IsGameKeyDown(28))
				{
					_backCameraDistanceMultiplier -= 0.5f * dt;
				}
				if (((MissionBehavior)this).Mission.InputManager.IsGameKeyDown(29))
				{
					_backCameraDistanceMultiplier += 0.5f * dt;
				}
				_backCameraDistanceMultiplier = MBMath.ClampFloat(_backCameraDistanceMultiplier, 0.2f, 3f);
				Vec3 backCameraTargetLocalPosition = ControllerMachine.BackCameraTargetLocalPosition;
				((Vec3)(ref val2))._002Ector(((Vec3)(ref backCameraTargetLocalPosition)).AsVec2, ControllerMachine.BackCameraTargetLocalPosition.z * _backCameraDistanceMultiplier, -1f);
				((MissionBehavior)this).Mission.SetListenerAndAttenuationPosBlendFactor(0.33f);
				break;
			}
			case CameraModes.Front:
				val = ControllerMachine.FrontCameraOffset;
				val2 = ControllerMachine.FrontCameraTargetLocalPosition;
				num = ControllerMachine.FrontCameraFovMultiplier;
				((MissionBehavior)this).Mission.SetListenerAndAttenuationPosBlendFactor(1f);
				break;
			default:
				val = ControllerMachine.ShoulderCameraOffset;
				val2 = ControllerMachine.ShoulderCameraTargetLocalPosition;
				num = ControllerMachine.ShoulderCameraFovMultiplier;
				((MissionBehavior)this).Mission.SetListenerAndAttenuationPosBlendFactor(0f);
				break;
			}
			bool flag = (!((Vec3)(ref _lastCameraOffset)).NearlyEquals(ref val, 0.001f) || MathF.Abs(_lastCameraFovMultiplier - num) > 0.001f) && !IsAimingWithRangedWeaponAndAllowed;
			_lastCameraOffset = (flag ? MBMath.Lerp(_lastCameraOffset, val, dt * 5f, 0.001f) : val);
			_lastCameraFovMultiplier = (flag ? MBMath.Lerp(_lastCameraFovMultiplier, num, dt * 5f, 0.001f) : num);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)ControllerMachine).GameEntity;
			WeakGameEntity root = ((WeakGameEntity)(ref gameEntity)).Root;
			float num2;
			Vec3 val3;
			if (pilotAgent != null)
			{
				num2 = MBMath.WrapAngle(((MissionView)this).MissionScreen.CameraBearing - pilotAgent.MovementDirectionAsAngle);
				val3 = pilotAgent.Position;
			}
			else
			{
				num2 = MBMath.WrapAngle(((MissionView)this).MissionScreen.CameraBearing);
				val3 = ((WeakGameEntity)(ref root)).GlobalPosition;
			}
			Vec3 val4;
			if (!((Vec3)(ref val2)).IsNonZero)
			{
				val4 = Vec3.Zero;
			}
			else
			{
				Vec3 val5 = val3;
				gameEntity = ((ScriptComponentBehavior)ControllerMachine).GameEntity;
				MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				Vec3 val6 = val5 - ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val2);
				Vec3 val7;
				if (ActiveCameraMode != CameraModes.Shoulder)
				{
					if (ActiveCameraMode != CameraModes.Front)
					{
						val7 = Vec3.Zero;
					}
					else
					{
						gameEntity = ((ScriptComponentBehavior)ControllerMachine.AttachedShip).GameEntity;
						globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
						val7 = ((Vec3)(ref globalFrame.rotation.f)).NormalizedCopy() * MathF.Cos(Math.Min(MathF.Abs(num2) * 2.5f, MathF.PI / 2f)) * 8f;
					}
				}
				else
				{
					gameEntity = ((ScriptComponentBehavior)ControllerMachine.AttachedShip).GameEntity;
					globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
					val7 = ((Vec3)(ref globalFrame.rotation.s)).NormalizedCopy() * MathF.Sin(num2) * ControllerMachine.ShoulderCameraDistance;
				}
				val4 = val6 - val7;
			}
			Vec3 val8 = val4;
			Mission.Current.SetCustomCameraFixedDistance((ActiveCameraMode == CameraModes.Front) ? ControllerMachine.FrontCameraDistance : ((ActiveCameraMode == CameraModes.Back) ? (((Vec3)(ref val)).Length * _backCameraDistanceMultiplier) : float.MinValue));
			Mission.Current.SetCustomCameraTargetLocalOffset(MBMath.Lerp(Mission.Current.CustomCameraTargetLocalOffset, -val8, dt * 10f, 0.001f));
			if (ActiveCameraMode == CameraModes.Shoulder)
			{
				if (!flag)
				{
					Mission.Current.SetIgnoredEntityForCamera((GameEntity)null);
				}
			}
			else if (Mission.Current.IgnoredEntityForCamera != root)
			{
				Mission.Current.SetIgnoredEntityForCamera(GameEntity.CreateFromWeakEntity(root));
			}
			Mission.Current.SetCustomCameraIgnoreCollision(ActiveCameraMode == CameraModes.Front);
		}
		else
		{
			_lastCameraOffset = MBMath.Lerp(_lastCameraOffset, Vec3.Zero, dt * 5f, 0.001f);
			_lastCameraFovMultiplier = MBMath.Lerp(_lastCameraFovMultiplier, 1f, dt * 5f, 0.001f);
			Mission.Current.SetCustomCameraFixedDistance(float.MinValue);
			Mission.Current.SetCustomCameraTargetLocalOffset(MBMath.Lerp(Mission.Current.CustomCameraTargetLocalOffset, Vec3.Zero, dt * 5f, 0.001f));
			if (!((Vec3)(ref _lastCameraOffset)).IsNonZero)
			{
				Mission.Current.SetIgnoredEntityForCamera((GameEntity)null);
			}
			Mission.Current.SetCustomCameraIgnoreCollision(false);
		}
		Mission.Current.SetCustomCameraLocalOffset(_lastCameraOffset);
		Mission.Current.SetCustomCameraFovMultiplier(_lastCameraFovMultiplier);
	}

	protected override void OnCreateView()
	{
	}

	protected override void OnDestroyView()
	{
	}

	protected override void OnSuspendView()
	{
	}

	protected override void OnResumeView()
	{
	}
}
