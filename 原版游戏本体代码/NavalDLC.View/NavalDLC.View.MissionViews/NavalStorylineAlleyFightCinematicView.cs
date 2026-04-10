using NavalDLC.Storyline;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.View.MissionViews;

public class NavalStorylineAlleyFightCinematicView : MissionView
{
	private bool _isInitialized;

	private bool _isCinematicPartActive;

	private NavalStorylineAlleyFightCinematicController _cinematicLogicController;

	private Camera _camera;

	private MatrixFrame _cameraFrame = MatrixFrame.Identity;

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (!_isInitialized)
		{
			InitializeView();
		}
		else if (!Game.Current.GameStateManager.ActiveStateDisabledByUser)
		{
			UpdateCamera(dt);
		}
	}

	public override bool IsPhotoModeAllowed()
	{
		return !_isCinematicPartActive;
	}

	private void GetCameraFrame(Vec3 position, Vec3 direction, out MatrixFrame cameraFrame)
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
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		_camera = Camera.CreateCamera();
		Camera combatCamera = ((MissionView)this).MissionScreen.CombatCamera;
		if ((NativeObject)(object)combatCamera != (NativeObject)null)
		{
			_camera.FillParametersFrom(combatCamera);
		}
		else
		{
			Debug.FailedAssert("Combat camera is null.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.View\\MissionViews\\NavalStorylineAlleyFightCinematicView.cs", "SetupCamera", 62);
		}
		_cinematicLogicController.GetCameraFrame(out var position, out var forward);
		GetCameraFrame(position, forward, out _cameraFrame);
		_camera.Frame = _cameraFrame;
		((MissionView)this).MissionScreen.CustomCamera = _camera;
	}

	private void UpdateCamera(float dt)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)_camera != (NativeObject)null)
		{
			_cinematicLogicController.GetCameraFrame(out var position, out var forward);
			GetCameraFrame(position, forward, out _cameraFrame);
			_camera.Frame = _cameraFrame;
		}
	}

	private void ReleaseCamera()
	{
		((MissionView)this).MissionScreen.CustomCamera = null;
		_camera.ReleaseCamera();
	}

	private void OnCinematicStateChanged(NavalStorylineAlleyFightCinematicController.NavalAlleyFightCinematicState state)
	{
		if (_isInitialized)
		{
			float fadeDuration = _cinematicLogicController.GetFadeDuration();
			float fadeDuration2 = _cinematicLogicController.GetFadeDuration();
			switch (state)
			{
			case NavalStorylineAlleyFightCinematicController.NavalAlleyFightCinematicState.InitialFadeOut:
				((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>().Disable();
				_isCinematicPartActive = true;
				ScreenFadeController.BeginFadeOutAndIn(fadeDuration, fadeDuration2, fadeDuration);
				break;
			case NavalStorylineAlleyFightCinematicController.NavalAlleyFightCinematicState.InitialFadeIn:
				SetupCamera();
				break;
			case NavalStorylineAlleyFightCinematicController.NavalAlleyFightCinematicState.Completed:
				((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>().Enable();
				_isCinematicPartActive = false;
				ReleaseCamera();
				break;
			}
		}
	}

	private void OnFightEnded(float fadeInDuration, float blackDuration, float fadeOutDuration)
	{
		ScreenFadeController.BeginFadeOutAndIn(fadeInDuration, blackDuration, fadeOutDuration);
	}

	private void OnConversationSetup(Vec3 direction)
	{
		((MissionView)this).MissionScreen.CameraBearing = ((Vec3)(ref direction)).RotationZ;
	}

	private void InitializeView()
	{
		_cinematicLogicController = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalStorylineAlleyFightCinematicController>();
		_isInitialized = _cinematicLogicController != null;
		if (_cinematicLogicController != null)
		{
			_cinematicLogicController.OnCinematicStateChanged += OnCinematicStateChanged;
			_cinematicLogicController.OnFightEndedEvent += OnFightEnded;
			_cinematicLogicController.OnConversationSetupEvent += OnConversationSetup;
		}
		MissionAgentLabelView missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentLabelView>();
		if (missionBehavior != null && ((MissionView)missionBehavior).IsReady())
		{
			((MissionView)missionBehavior).SuspendView();
		}
	}
}
