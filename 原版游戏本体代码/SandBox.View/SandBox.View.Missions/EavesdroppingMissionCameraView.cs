using SandBox.Missions;
using TaleWorlds.DotNet;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions;

public class EavesdroppingMissionCameraView : MissionView
{
	private enum CameraSwitchState
	{
		None,
		ReadyForFadeOut,
		FadeOutAndInStarted,
		WaitingForFadeInToEnd
	}

	private CameraSwitchState _cameraSwitchState;

	private EavesdroppingMissionLogic _eavesdroppingMissionLogic;

	protected virtual void SetPlayerMovementEnabled(bool isPlayerMovementEnabled)
	{
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_cameraSwitchState = CameraSwitchState.None;
		_eavesdroppingMissionLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<EavesdroppingMissionLogic>();
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (_eavesdroppingMissionLogic == null)
		{
			return;
		}
		switch (_cameraSwitchState)
		{
		case CameraSwitchState.None:
			if ((_eavesdroppingMissionLogic.EavesdropStarted && (NativeObject)(object)((MissionView)this).MissionScreen.CustomCamera == (NativeObject)null) || (!_eavesdroppingMissionLogic.EavesdropStarted && (NativeObject)(object)((MissionView)this).MissionScreen.CustomCamera != (NativeObject)null))
			{
				if (_eavesdroppingMissionLogic.EavesdropStarted && (NativeObject)(object)((MissionView)this).MissionScreen.CustomCamera == (NativeObject)null)
				{
					SetPlayerMovementEnabled(isPlayerMovementEnabled: false);
				}
				_cameraSwitchState = CameraSwitchState.ReadyForFadeOut;
			}
			break;
		case CameraSwitchState.ReadyForFadeOut:
			ScreenFadeController.BeginFadeOutAndIn(0.5f, 0.5f, 0.5f);
			_cameraSwitchState = CameraSwitchState.FadeOutAndInStarted;
			break;
		case CameraSwitchState.FadeOutAndInStarted:
			if (ScreenFadeController.IsFadedOut)
			{
				((MissionView)this).MissionScreen.CustomCamera = (((NativeObject)(object)((MissionView)this).MissionScreen.CustomCamera == (NativeObject)null) ? _eavesdroppingMissionLogic.CurrentEavesdroppingCamera : null);
				if ((NativeObject)(object)((MissionView)this).MissionScreen.CustomCamera == (NativeObject)null)
				{
					SetPlayerMovementEnabled(isPlayerMovementEnabled: true);
				}
				_cameraSwitchState = CameraSwitchState.WaitingForFadeInToEnd;
			}
			break;
		case CameraSwitchState.WaitingForFadeInToEnd:
			if (!ScreenFadeController.IsFadeActive)
			{
				_cameraSwitchState = CameraSwitchState.None;
			}
			break;
		}
	}
}
