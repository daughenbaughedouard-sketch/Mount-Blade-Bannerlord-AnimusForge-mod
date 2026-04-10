using System;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.MissionViews.SiegeWeapon;

public class RangedSiegeWeaponView : UsableMissionObjectComponent
{
	private float _cameraYaw;

	private float _cameraPitch;

	private float _cameraRoll;

	private float _cameraInitialYaw;

	private float _cameraInitialPitch;

	private Vec3 _cameraPositionOffset;

	private bool _isInWeaponCameraMode;

	protected bool UsesMouseForAiming;

	public RangedSiegeWeapon RangedSiegeWeapon { get; private set; }

	public MissionScreen MissionScreen { get; private set; }

	public Camera Camera { get; private set; }

	public GameEntity CameraHolder => RangedSiegeWeapon.CameraHolder;

	public Agent PilotAgent => ((UsableMachine)RangedSiegeWeapon).PilotAgent;

	public void Initialize(RangedSiegeWeapon rangedSiegeWeapon, MissionScreen missionScreen)
	{
		RangedSiegeWeapon = rangedSiegeWeapon;
		MissionScreen = missionScreen;
	}

	protected override void OnAdded(Scene scene)
	{
		((UsableMissionObjectComponent)this).OnAdded(scene);
		if (CameraHolder != (GameEntity)null)
		{
			CreateCamera();
		}
	}

	protected override void OnMissionReset()
	{
		((UsableMissionObjectComponent)this).OnMissionReset();
		if (CameraHolder != (GameEntity)null)
		{
			_cameraYaw = _cameraInitialYaw;
			_cameraPitch = _cameraInitialPitch;
			ApplyCameraRotation();
			_isInWeaponCameraMode = false;
			ResetCamera();
		}
	}

	public override bool IsOnTickRequired()
	{
		return true;
	}

	protected override void OnTick(float dt)
	{
		((UsableMissionObjectComponent)this).OnTick(dt);
		if (!GameNetwork.IsReplay)
		{
			HandleUserInput(dt);
		}
	}

	protected virtual void HandleUserInput(float dt)
	{
		if (CameraHolder != (GameEntity)null && ((PilotAgent != null && PilotAgent.IsMainAgent) || RangedSiegeWeapon.PlayerForceUse))
		{
			if (!_isInWeaponCameraMode)
			{
				_isInWeaponCameraMode = true;
				StartUsingWeaponCamera();
			}
			if (RangedSiegeWeapon.PlayerForceUse)
			{
				HandleUserCameraRotation(dt);
			}
		}
		if (_isInWeaponCameraMode && (PilotAgent == null || !PilotAgent.IsMainAgent) && !RangedSiegeWeapon.PlayerForceUse)
		{
			_isInWeaponCameraMode = false;
			ResetCamera();
		}
		HandleUserAiming(dt);
	}

	private void CreateCamera()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Camera = Camera.CreateCamera();
		float aspectRatio = Screen.AspectRatio;
		Camera.SetFovVertical(MathF.PI / 3f, aspectRatio, 0.1f, 12500f);
		Camera.Entity = CameraHolder;
		MatrixFrame frame = CameraHolder.GetFrame();
		Vec3 eulerAngles = ((Mat3)(ref frame.rotation)).GetEulerAngles();
		_cameraYaw = eulerAngles.z;
		_cameraPitch = eulerAngles.x;
		_cameraRoll = eulerAngles.y;
		_cameraPositionOffset = frame.origin;
		((Vec3)(ref _cameraPositionOffset)).RotateAboutZ(0f - _cameraYaw);
		((Vec3)(ref _cameraPositionOffset)).RotateAboutX(0f - _cameraPitch);
		((Vec3)(ref _cameraPositionOffset)).RotateAboutY(0f - _cameraRoll);
		_cameraInitialYaw = _cameraYaw;
		_cameraInitialPitch = _cameraPitch;
	}

	protected virtual void StartUsingWeaponCamera()
	{
		MissionScreen.CustomCamera = Camera;
		Agent.Main.IsLookDirectionLocked = true;
	}

	private void ResetCamera()
	{
		if ((NativeObject)(object)MissionScreen.CustomCamera == (NativeObject)(object)Camera)
		{
			MissionScreen.CustomCamera = null;
			if (Agent.Main != null)
			{
				Agent.Main.IsLookDirectionLocked = false;
				MissionScreen.SetExtraCameraParameters(newForceCanZoom: false, 0f);
			}
		}
	}

	protected virtual void HandleUserCameraRotation(float dt)
	{
		float cameraYaw = _cameraYaw;
		float cameraPitch = _cameraPitch;
		if (((ScreenLayer)MissionScreen.SceneLayer).Input.IsGameKeyDown(10))
		{
			_cameraYaw = _cameraInitialYaw;
			_cameraPitch = _cameraInitialPitch;
		}
		_cameraYaw += ((ScreenLayer)MissionScreen.SceneLayer).Input.GetMouseMoveX() * dt * 0.2f;
		_cameraPitch += ((ScreenLayer)MissionScreen.SceneLayer).Input.GetMouseMoveY() * dt * 0.2f;
		_cameraYaw = MBMath.ClampFloat(_cameraYaw, MathF.PI / 2f, 4.712389f);
		_cameraPitch = MBMath.ClampFloat(_cameraPitch, MathF.PI / 3f, MathF.PI * 5f / 9f);
		if (cameraPitch != _cameraPitch || cameraYaw != _cameraYaw)
		{
			ApplyCameraRotation();
		}
	}

	private void ApplyCameraRotation()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		((Mat3)(ref identity.rotation)).RotateAboutUp(_cameraYaw);
		((Mat3)(ref identity.rotation)).RotateAboutSide(_cameraPitch);
		((Mat3)(ref identity.rotation)).RotateAboutForward(_cameraRoll);
		((MatrixFrame)(ref identity)).Strafe(_cameraPositionOffset.x);
		((MatrixFrame)(ref identity)).Advance(_cameraPositionOffset.y);
		((MatrixFrame)(ref identity)).Elevate(_cameraPositionOffset.z);
		CameraHolder.SetFrame(ref identity, true);
	}

	private void HandleUserAiming(float dt)
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		float num = 0f;
		float num2 = 0f;
		if (PilotAgent != null && (PilotAgent.IsMainAgent || RangedSiegeWeapon.PlayerForceUse))
		{
			if (UsesMouseForAiming)
			{
				InputContext input = ((ScreenLayer)MissionScreen.SceneLayer).Input;
				float num3 = dt * 1666.6666f;
				float num4 = input.GetMouseMoveX() + num3 * input.GetGameKeyAxis("CameraAxisX");
				float num5 = input.GetMouseMoveY() + (0f - num3) * input.GetGameKeyAxis("CameraAxisY");
				if (NativeConfig.InvertMouse)
				{
					num5 *= -1f;
				}
				Vec2 val = default(Vec2);
				((Vec2)(ref val))._002Ector(0f - num4, 0f - num5);
				if (((Vec2)(ref val)).IsNonZero())
				{
					float num6 = ((Vec2)(ref val)).Normalize();
					num6 = MathF.Min(5f, MathF.Pow(num6, 1.5f) * 0.025f);
					val *= num6;
					num = val.x;
					num2 = val.y;
				}
			}
			else
			{
				if (((ScreenLayer)MissionScreen.SceneLayer).Input.IsGameKeyDown(2))
				{
					num = 1f;
				}
				else if (((ScreenLayer)MissionScreen.SceneLayer).Input.IsGameKeyDown(3))
				{
					num = -1f;
				}
				if (((ScreenLayer)MissionScreen.SceneLayer).Input.IsGameKeyDown(0))
				{
					num2 = 1f;
				}
				else if (((ScreenLayer)MissionScreen.SceneLayer).Input.IsGameKeyDown(1))
				{
					num2 = -1f;
				}
			}
			if (num != 0f)
			{
				flag = true;
			}
			if (num2 != 0f)
			{
				flag = true;
			}
		}
		if (flag)
		{
			RangedSiegeWeapon.GiveInput(num, num2);
		}
	}

	protected override void OnMissionObjectDisabled()
	{
		ResetCamera();
	}
}
