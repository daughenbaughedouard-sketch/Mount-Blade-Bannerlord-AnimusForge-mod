using NavalDLC.HotKeyCategories;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace NavalDLC.View;

[GameStateScreen(typeof(NavalMissionState))]
internal class NavalMissionScreen : MissionScreen
{
	private NavalShipsLogic _navalShipsLogic;

	public NavalMissionScreen(MissionState missionState)
		: base(missionState)
	{
	}

	protected override void InitializeMissionView()
	{
		((MissionScreen)this).InitializeMissionView();
		SceneLayer obj = ((ScreenBase)this).FindLayer<SceneLayer>();
		if (obj != null)
		{
			((ScreenLayer)obj).Input.RegisterHotKeyCategory((GameKeyContext)(object)new NavalCheatsHotKeyCategory());
		}
		_navalShipsLogic = ((MissionScreen)this).Mission.GetMissionBehavior<NavalShipsLogic>();
	}

	protected override bool CanViewCharacter()
	{
		if (_navalShipsLogic == null)
		{
			return true;
		}
		return _navalShipsLogic.PlayerControlledShip == null;
	}

	protected override bool CanToggleCamera()
	{
		if (_navalShipsLogic?.PlayerControlledShip == null)
		{
			return ((MissionScreen)this).CanToggleCamera();
		}
		return false;
	}

	public override void TeleportMainAgentToCameraFocusForCheat()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		MissionShip missionShip = missionBehavior?.PlayerControlledShip;
		if (missionShip != null)
		{
			MatrixFrame globalFrame = missionShip.GlobalFrame;
			MatrixFrame lastFinalRenderCameraFrame = ((MissionScreen)this).Mission.Scene.LastFinalRenderCameraFrame;
			float num = ((Vec3)(ref globalFrame.origin)).Z - ((Vec3)(ref lastFinalRenderCameraFrame.origin)).Z;
			Vec3 val = -lastFinalRenderCameraFrame.rotation.u;
			float num2 = num / ((Vec3)(ref val)).Z;
			Vec3 f = lastFinalRenderCameraFrame.rotation.f;
			f.z = 0f;
			((Vec3)(ref f)).Normalize();
			if (num2 <= 400f)
			{
				val *= num2;
				globalFrame.origin = lastFinalRenderCameraFrame.origin + val;
				globalFrame.origin = new Vec3(((Vec3)(ref globalFrame.origin)).AsVec2, Mission.Current.Scene.GetWaterLevelAtPosition(((Vec3)(ref globalFrame.origin)).AsVec2, true, false), -1f);
				globalFrame.rotation = Mat3.CreateMat3WithForward(ref f);
				missionBehavior.TeleportShip(missionShip, globalFrame, checkFreeArea: false);
			}
		}
		else
		{
			((MissionScreen)this).TeleportMainAgentToCameraFocusForCheat();
		}
	}
}
