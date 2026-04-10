using SandBox.Missions.MissionLogics.Hideout;
using SandBox.Objects.Cinematics;
using SandBox.Objects.Usables;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions;

public class MissionHideoutAmbushCinematicView : MissionView
{
	private enum HideoutAmbushCinematicState
	{
		None,
		FirstFadeOut,
		ChangeToCustomCamera,
		FirstFadeIn,
		SendArrow,
		Wait,
		SecondFadeOut,
		ChangeBackToDefaultCamera,
		SecondFadeIn,
		Ending,
		Ended
	}

	private const string CameraTag = "hideout_ambush_cutscene_camera";

	private const string ArrowBarrelTag = "hideout_ambush_cutscene_arrow_barrel";

	private const string ArrowPathTag = "hideout_ambush_cutscene_arrow_path";

	private Camera _camera;

	private GameEntity _cameraEntity;

	private GameEntity _arrowPath;

	private HideoutAmbushMissionController _hideoutAmbushMissionController;

	private HideoutAmbushCinematicState _currentHideoutAmbushCinematicState;

	private Timer _timer;

	protected virtual void SetPlayerMovementEnabled(bool isPlayerMovementEnabled)
	{
	}

	public override void AfterStart()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).AfterStart();
		_cameraEntity = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("hideout_ambush_cutscene_camera");
		_arrowPath = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("hideout_ambush_cutscene_arrow_path");
		_hideoutAmbushMissionController = ((MissionBehavior)this).Mission.GetMissionBehavior<HideoutAmbushMissionController>();
		Vec3 invalid = Vec3.Invalid;
		_camera = Camera.CreateCamera();
		_cameraEntity.GetCameraParamsFromCameraScript(_camera, ref invalid);
		_camera.SetFovVertical(_camera.GetFovVertical(), Screen.AspectRatio, _camera.Near, _camera.Far);
		_arrowPath.SetVisibilityExcludeParents(false);
		_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.None;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		((MissionBehavior)this).OnMissionTick(dt);
		switch (_currentHideoutAmbushCinematicState)
		{
		case HideoutAmbushCinematicState.None:
		{
			HideoutAmbushMissionController hideoutAmbushMissionController = _hideoutAmbushMissionController;
			if (hideoutAmbushMissionController != null && hideoutAmbushMissionController.IsReadyForCallTroopsCinematic)
			{
				_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.FirstFadeOut;
				SetPlayerMovementEnabled(isPlayerMovementEnabled: false);
			}
			break;
		}
		case HideoutAmbushCinematicState.FirstFadeOut:
			ScreenFadeController.BeginFadeOutAndIn(0.5f, 0.5f, 0.5f);
			_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.ChangeToCustomCamera;
			break;
		case HideoutAmbushCinematicState.ChangeToCustomCamera:
			if (ScreenFadeController.IsFadedOut)
			{
				((MissionView)this).MissionScreen.CustomCamera = _camera;
				Agent.Main.AgentVisuals.SetVisible(false);
				_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.FirstFadeIn;
			}
			break;
		case HideoutAmbushCinematicState.FirstFadeIn:
			if (!ScreenFadeController.IsFadeActive)
			{
				_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.SendArrow;
			}
			break;
		case HideoutAmbushCinematicState.SendArrow:
			_arrowPath.SetVisibilityExcludeParents(true);
			_timer = new Timer(((MissionBehavior)this).Mission.CurrentTime, 5f, true);
			_arrowPath.GetFirstScriptOfType<CinematicBurningArrow>().StartMovement();
			_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.Wait;
			break;
		case HideoutAmbushCinematicState.Wait:
			if (_timer.Check(((MissionBehavior)this).Mission.CurrentTime))
			{
				_timer = null;
				_arrowPath.SetVisibilityExcludeParents(false);
				_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.SecondFadeOut;
			}
			break;
		case HideoutAmbushCinematicState.SecondFadeOut:
			ScreenFadeController.BeginFadeOutAndIn(0.5f, 0.5f, 0.5f);
			_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.ChangeBackToDefaultCamera;
			break;
		case HideoutAmbushCinematicState.ChangeBackToDefaultCamera:
			if (ScreenFadeController.IsFadedOut)
			{
				((MissionView)this).MissionScreen.CustomCamera = null;
				Agent.Main.AgentVisuals.SetVisible(true);
				_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.SecondFadeIn;
			}
			break;
		case HideoutAmbushCinematicState.SecondFadeIn:
			if (!ScreenFadeController.IsFadeActive)
			{
				_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.Ending;
			}
			break;
		case HideoutAmbushCinematicState.Ending:
			SetPlayerMovementEnabled(isPlayerMovementEnabled: true);
			_hideoutAmbushMissionController.OnAgentsShouldBeEnabled();
			_currentHideoutAmbushCinematicState = HideoutAmbushCinematicState.Ended;
			break;
		}
	}

	public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		((MissionBehavior)this).OnObjectUsed(userAgent, usedObject);
		if (userAgent == Agent.Main && usedObject is StealthAreaUsePoint)
		{
			MissionAgentAlarmStateView missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentAlarmStateView>();
			if (missionBehavior != null && ((MissionView)missionBehavior).IsReady())
			{
				((MissionView)missionBehavior).SuspendView();
			}
		}
	}
}
