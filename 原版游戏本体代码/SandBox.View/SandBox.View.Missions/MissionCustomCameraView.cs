using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions;

public class MissionCustomCameraView : MissionView
{
	public string tag = "customcamera";

	private readonly List<Camera> _cameras = new List<Camera>();

	public Vec3 _dofParams;

	private int _currentCameraIndex;

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		foreach (GameEntity item in ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag(tag))
		{
			Camera val = Camera.CreateCamera();
			item.GetCameraParamsFromCameraScript(val, ref _dofParams);
			_cameras.Add(val);
		}
		((MissionView)this).MissionScreen.CustomCamera = _cameras[0];
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (((MissionBehavior)this).DebugInput.IsHotKeyReleased("CustomCameraMissionViewHotkeyIncreaseCustomCameraIndex"))
		{
			_currentCameraIndex++;
			if (_currentCameraIndex >= _cameras.Count)
			{
				_currentCameraIndex = 0;
			}
			((MissionView)this).MissionScreen.CustomCamera = _cameras[_currentCameraIndex];
		}
	}
}
