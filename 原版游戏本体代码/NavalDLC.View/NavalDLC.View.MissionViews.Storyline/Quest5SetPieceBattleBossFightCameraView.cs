using NavalDLC.Storyline.MissionControllers;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.View.MissionViews.Storyline;

public class Quest5SetPieceBattleBossFightCameraView : MissionView
{
	private Quest5SetPieceBattleMissionController _quest5SetPieceBattleMissionController;

	private Camera _bossFightCamera;

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_quest5SetPieceBattleMissionController = Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>();
	}

	public override void OnMissionTick(float dt)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnMissionTick(dt);
		if (_quest5SetPieceBattleMissionController.State < Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.BossFightConversationInProgress)
		{
			return;
		}
		if (_quest5SetPieceBattleMissionController.BossFightConversationCameraGameEntity != (GameEntity)null)
		{
			if ((NativeObject)(object)_bossFightCamera == (NativeObject)null)
			{
				Vec3 invalid = Vec3.Invalid;
				_bossFightCamera = Camera.CreateCamera();
				_quest5SetPieceBattleMissionController.BossFightConversationCameraGameEntity.GetCameraParamsFromCameraScript(_bossFightCamera, ref invalid);
				_bossFightCamera.SetFovVertical(_bossFightCamera.GetFovVertical(), Screen.AspectRatio, _bossFightCamera.Near, _bossFightCamera.Far);
			}
			else
			{
				_bossFightCamera.Frame = _quest5SetPieceBattleMissionController.BossFightConversationCameraGameEntity.GetGlobalFrame();
			}
			((MissionView)this).MissionScreen.CustomCamera = _bossFightCamera;
		}
		else if ((NativeObject)(object)((MissionView)this).MissionScreen.CustomCamera != (NativeObject)null)
		{
			((MissionView)this).MissionScreen.CustomCamera = null;
		}
	}
}
