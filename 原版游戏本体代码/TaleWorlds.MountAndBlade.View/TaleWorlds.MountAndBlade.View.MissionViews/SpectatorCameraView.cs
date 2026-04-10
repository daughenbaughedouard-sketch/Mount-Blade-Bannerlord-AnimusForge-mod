using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class SpectatorCameraView : MissionView
{
	private List<MatrixFrame> _spectateCamerFrames = new List<MatrixFrame>();

	public override void OnMissionScreenInitialize()
	{
		base.OnMissionScreenInitialize();
		((ScreenLayer)base.MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("MultiplayerHotkeyCategory"));
	}

	public override void AfterStart()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 10; i++)
		{
			_spectateCamerFrames.Add(MatrixFrame.Identity);
		}
		for (int j = 0; j < 10; j++)
		{
			string text = "spectate_cam_" + j;
			List<GameEntity> list = Mission.Current.Scene.FindEntitiesWithTag(text).ToList();
			if (list.Count > 0)
			{
				_spectateCamerFrames[j] = list[0].GetGlobalFrame();
			}
		}
	}
}
