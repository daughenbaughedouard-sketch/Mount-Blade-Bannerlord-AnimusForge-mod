using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC;

public class NavalMissionState : MissionState
{
	public static Mission OpenNew(string missionName, MissionInitializerRecord rec, InitializeMissionBehaviorsDelegate handler, bool addDefaultMissionBehaviors = true, bool needsMemoryCleanup = true)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Debug.Print("Opening new mission " + missionName + " " + rec.SceneLevels + ".\n", 0, (DebugColor)12, 17592186044416uL);
		if (!GameNetwork.IsClientOrReplay && !GameNetwork.IsServer)
		{
			MBCommon.CurrentGameType = (GameType)(MissionState.IsRecordingActive() ? 5 : 0);
		}
		Game.Current.OnMissionIsStarting(missionName, rec);
		NavalMissionState navalMissionState = Game.Current.GameStateManager.CreateState<NavalMissionState>();
		Mission obj = ((MissionState)navalMissionState).HandleOpenNew(missionName, rec, handler, addDefaultMissionBehaviors, needsMemoryCleanup);
		obj.SetCloseProximityWaveSoundsEnabled(true);
		obj.ForceDisableOcclusion(true);
		Game.Current.GameStateManager.PushState((GameState)(object)navalMissionState, 0);
		return obj;
	}
}
