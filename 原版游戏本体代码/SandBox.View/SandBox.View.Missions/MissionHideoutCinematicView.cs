using SandBox.Missions.MissionLogics.Hideout;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions;

public class MissionHideoutCinematicView : MissionView
{
	private bool _isInitialized;

	private HideoutCinematicController _cinematicLogicController;

	private HideoutCinematicController.HideoutCinematicState _currentState;

	private HideoutCinematicController.HideoutCinematicState _nextState;

	private Camera _camera;

	private MatrixFrame _cameraFrame = MatrixFrame.Identity;

	private readonly Vec3 _cameraOffset = new Vec3(0.3f, 0.3f, 1.2f, -1f);

	private Vec3 _cameraMoveDir = Vec3.Forward;

	private float _cameraSpeed;

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (!_isInitialized)
		{
			InitializeView();
		}
		else if (!Game.Current.GameStateManager.ActiveStateDisabledByUser && (_currentState == HideoutCinematicController.HideoutCinematicState.Cinematic || _nextState == HideoutCinematicController.HideoutCinematicState.Cinematic))
		{
			UpdateCamera(dt);
		}
	}

	private void SetCameraFrame(Vec3 position, Vec3 direction, out MatrixFrame cameraFrame)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		cameraFrame.origin = position;
		cameraFrame.rotation.s = Vec3.Side;
		cameraFrame.rotation.f = Vec3.Up;
		cameraFrame.rotation.u = -direction;
		((Mat3)(ref cameraFrame.rotation)).Orthonormalize();
	}

	private void SetupCamera()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		_camera = Camera.CreateCamera();
		Camera combatCamera = ((MissionView)this).MissionScreen.CombatCamera;
		if ((NativeObject)(object)combatCamera != (NativeObject)null)
		{
			_camera.FillParametersFrom(combatCamera);
		}
		else
		{
			Debug.FailedAssert("Combat camera is null.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Missions\\MissionHideoutCinematicView.cs", "SetupCamera", 66);
		}
		_cinematicLogicController.GetBossStandingEyePosition(out var eyePosition);
		_cinematicLogicController.GetPlayerStandingEyePosition(out var eyePosition2);
		Vec3 val = eyePosition - eyePosition2;
		Vec3 val2 = ((Vec3)(ref val)).NormalizedCopy();
		_cinematicLogicController.GetScenePrefabParameters(out var innerRadius, out var outerRadius, out var walkDistance);
		float num = innerRadius + outerRadius + 1.5f * walkDistance;
		_cameraSpeed = num / MathF.Max(_cinematicLogicController.CinematicDuration, 0.1f);
		_cameraMoveDir = -val2;
		SetCameraFrame(eyePosition, val2, out _cameraFrame);
		Vec3 val3 = _cameraFrame.origin + _cameraOffset.x * _cameraFrame.rotation.s + _cameraOffset.y * _cameraFrame.rotation.f + _cameraOffset.z * _cameraFrame.rotation.u;
		val = eyePosition - val3;
		Vec3 direction = ((Vec3)(ref val)).NormalizedCopy();
		SetCameraFrame(val3, direction, out _cameraFrame);
		_camera.Frame = _cameraFrame;
		((MissionView)this).MissionScreen.CustomCamera = _camera;
	}

	private void UpdateCamera(float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = _cameraFrame.origin + _cameraMoveDir * _cameraSpeed * dt;
		_cinematicLogicController.GetBossStandingEyePosition(out var eyePosition);
		Vec3 val2 = eyePosition - val;
		Vec3 direction = ((Vec3)(ref val2)).NormalizedCopy();
		SetCameraFrame(val, direction, out _cameraFrame);
		_camera.Frame = _cameraFrame;
	}

	private void ReleaseCamera()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((MissionView)this).MissionScreen.UpdateFreeCamera(((MissionView)this).MissionScreen.CustomCamera.Frame);
		((MissionView)this).MissionScreen.CustomCamera = null;
		_camera.ReleaseCamera();
	}

	private void OnCinematicStateChanged(HideoutCinematicController.HideoutCinematicState state)
	{
		if (_isInitialized)
		{
			_currentState = state;
			if (_currentState == HideoutCinematicController.HideoutCinematicState.PreCinematic)
			{
				SetupCamera();
			}
			else if (_currentState == HideoutCinematicController.HideoutCinematicState.PostCinematic)
			{
				ReleaseCamera();
			}
		}
	}

	private void OnCinematicTransition(HideoutCinematicController.HideoutCinematicState nextState, float duration)
	{
		if (_isInitialized)
		{
			switch (nextState)
			{
			case HideoutCinematicController.HideoutCinematicState.InitialFadeOut:
			case HideoutCinematicController.HideoutCinematicState.PostCinematic:
				ScreenFadeController.BeginFadeOut(duration);
				break;
			case HideoutCinematicController.HideoutCinematicState.Cinematic:
			case HideoutCinematicController.HideoutCinematicState.Completed:
				ScreenFadeController.BeginFadeIn(duration);
				break;
			}
			_nextState = nextState;
		}
	}

	private void InitializeView()
	{
		_cinematicLogicController = ((MissionBehavior)this).Mission.GetMissionBehavior<HideoutCinematicController>();
		_isInitialized = _cinematicLogicController != null;
		if (_cinematicLogicController != null)
		{
			_cinematicLogicController.OnCinematicStateChanged += OnCinematicStateChanged;
			_cinematicLogicController.OnCinematicTransition += OnCinematicTransition;
		}
	}
}
