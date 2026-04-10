using System;
using NavalDLC.Missions.ShipInput;
using NavalDLC.Storyline.MissionControllers;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;

namespace NavalDLC.View.MissionViews;

public class BlockedEstuaryView : MissionView
{
	private const string CameraSpawnId = "sp_camera";

	private const string CameraShipSpawnId = "sp_camera_ship";

	private BlockedEstuaryMissionController _controller;

	private Camera _camera;

	private bool _isInitialized;

	private GameEntity _cameraFrame;

	private GameEntity _shipCameraFrame;

	private MissionMainAgentController _mainAgentController;

	private bool _checkPointReached;

	private MatrixFrame _cameraTargetFrame;

	private bool _useShipCamera;

	private float _switchTimer;

	private float _transitionSpeed = 2f;

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (!_isInitialized)
		{
			InitializeView();
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (_isInitialized && !Game.Current.GameStateManager.ActiveStateDisabledByUser && (NativeObject)(object)_camera != (NativeObject)null)
		{
			UpdateCamera(dt);
			if (!((MatrixFrame)(ref _cameraTargetFrame)).IsIdentity && !((MatrixFrame)(ref _cameraTargetFrame)).IsZero)
			{
				Camera camera = _camera;
				MatrixFrame frame = _camera.Frame;
				camera.Frame = MatrixFrame.Lerp(ref frame, ref _cameraTargetFrame, dt * _transitionSpeed);
			}
		}
	}

	public void FadeToBlack(float fadeOutTime, float blackTime, float fadeInTime)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		ScreenFadeController.BeginFadeOutAndIn(fadeOutTime, blackTime, fadeInTime);
		MissionScreen missionScreen = ((MissionView)this).MissionScreen;
		Vec3 lookDirection = Agent.Main.LookDirection;
		missionScreen.CameraBearing = ((Vec3)(ref lookDirection)).RotationZ;
	}

	private void UpdateCamera(float dt)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (_controller.CollisionImminent)
		{
			if (_switchTimer <= 2f)
			{
				_switchTimer += dt;
			}
			else
			{
				_useShipCamera = false;
			}
		}
		if (_useShipCamera)
		{
			_transitionSpeed = 4f;
			SetCameraFrame(_shipCameraFrame.GlobalPosition, -_shipCameraFrame.GetGlobalFrame().rotation.u * 2f);
		}
		else if (_controller.CollisionImminent)
		{
			_transitionSpeed = 0.3f;
			SetCameraFrame(_cameraFrame.GlobalPosition, -_cameraFrame.GetGlobalFrame().rotation.u);
		}
	}

	private void SetupCamera()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		_camera = Camera.CreateCamera();
		Vec3 zero = Vec3.Zero;
		_cameraFrame.GetCameraParamsFromCameraScript(_camera, ref zero);
		((MissionView)this).MissionScreen.CustomCamera = _camera;
		_camera.Frame = ((MissionView)this).MissionScreen.CombatCamera.Frame;
		_switchTimer = 0f;
		_useShipCamera = true;
	}

	private void SetCameraFrame(Vec3 position, Vec3 direction)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame frame = _camera.Frame;
		frame.origin = position;
		frame.rotation.s = Vec3.Side;
		frame.rotation.f = Vec3.Up;
		frame.rotation.u = -direction;
		((Mat3)(ref frame.rotation)).Orthonormalize();
		_cameraTargetFrame = frame;
	}

	private void InitializeView()
	{
		_controller = ((MissionBehavior)this).Mission.GetMissionBehavior<BlockedEstuaryMissionController>();
		_mainAgentController = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>();
		BlockedEstuaryMissionController controller = _controller;
		controller.OnCheckPointReachedEvent = (Action)Delegate.Combine(controller.OnCheckPointReachedEvent, new Action(OnCheckPointReached));
		BlockedEstuaryMissionController controller2 = _controller;
		controller2.OnLastExitZoneReachedEvent = (Action)Delegate.Combine(controller2.OnLastExitZoneReachedEvent, new Action(LastExitZoneReached));
		BlockedEstuaryMissionController controller3 = _controller;
		controller3.OnPhaseEnd = (Action)Delegate.Combine(controller3.OnPhaseEnd, new Action(OnPhaseEnd));
		_cameraFrame = GetCameraEntity();
		_shipCameraFrame = GetShipCameraEntity();
		_isInitialized = true;
		MissionShipControlView missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionShipControlView>();
		if (missionBehavior != null && ((MissionView)missionBehavior).IsReady())
		{
			if (_controller.CurrentPhase != BlockedEstuaryMissionController.BattlePhase.Phase3)
			{
				missionBehavior.SetSailInput(SailInput.Full);
			}
			missionBehavior.SetActiveCameraMode(MissionShipControlView.CameraModes.Back);
		}
	}

	private void OnPhaseEnd()
	{
		if ((NativeObject)(object)_camera != (NativeObject)null)
		{
			ReleaseCamera();
		}
		FadeToBlack(0.1f, 0.5f, 0.5f);
	}

	private void LastExitZoneReached()
	{
		_mainAgentController.Disable();
		SetupCamera();
	}

	private void OnPlayerDismounted()
	{
		FadeToBlack(0.1f, 0.5f, 0.5f);
	}

	private void OnCheckPointReached()
	{
		if (Agent.Main.HasMount)
		{
			_mainAgentController.Disable();
		}
		_checkPointReached = true;
	}

	public override void OnAgentDismount(Agent agent)
	{
		if (agent.IsMainAgent && _checkPointReached)
		{
			_mainAgentController.Enable();
			OnPlayerDismounted();
		}
	}

	public override void OnMissionScreenFinalize()
	{
		BlockedEstuaryMissionController controller = _controller;
		controller.OnCheckPointReachedEvent = (Action)Delegate.Remove(controller.OnCheckPointReachedEvent, new Action(OnCheckPointReached));
		BlockedEstuaryMissionController controller2 = _controller;
		controller2.OnLastExitZoneReachedEvent = (Action)Delegate.Remove(controller2.OnLastExitZoneReachedEvent, new Action(LastExitZoneReached));
		BlockedEstuaryMissionController controller3 = _controller;
		controller3.OnPhaseEnd = (Action)Delegate.Remove(controller3.OnPhaseEnd, new Action(OnPhaseEnd));
		((MissionView)this).OnMissionScreenFinalize();
	}

	private void ReleaseCamera()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_mainAgentController.Enable();
		((MissionView)this).MissionScreen.UpdateFreeCamera(((MissionView)this).MissionScreen.CustomCamera.Frame);
		((MissionView)this).MissionScreen.CustomCamera = null;
		_camera.ReleaseCamera();
		_camera = null;
	}

	private GameEntity GetCameraEntity()
	{
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera");
		if (val != (GameEntity)null)
		{
			return val;
		}
		Debug.FailedAssert("Cant find CameraEntity", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.View\\MissionViews\\BlockedEstuaryView.cs", "GetCameraEntity", 217);
		return null;
	}

	private GameEntity GetShipCameraEntity()
	{
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_ship");
		if (val != (GameEntity)null)
		{
			return val;
		}
		Debug.FailedAssert("Cant find ShipCameraEntity", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.View\\MissionViews\\BlockedEstuaryView.cs", "GetShipCameraEntity", 229);
		return null;
	}
}
