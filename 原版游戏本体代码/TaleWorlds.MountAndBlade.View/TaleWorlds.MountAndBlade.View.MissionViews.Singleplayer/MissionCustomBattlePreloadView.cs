using System.Collections.Generic;
using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

public class MissionCustomBattlePreloadView : MissionView
{
	private PreloadHelper _helperInstance = new PreloadHelper();

	private bool _preloadDone;

	public override void OnPreMissionTick(float dt)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (_preloadDone)
		{
			return;
		}
		MissionCombatantsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionCombatantsLogic>();
		List<BasicCharacterObject> list = new List<BasicCharacterObject>();
		foreach (IBattleCombatant allCombatant in missionBehavior.GetAllCombatants())
		{
			list.AddRange(((CustomBattleCombatant)allCombatant).Characters);
		}
		_helperInstance.PreloadCharacters(list);
		SiegeDeploymentMissionController missionBehavior2 = Mission.Current.GetMissionBehavior<SiegeDeploymentMissionController>();
		if (missionBehavior2 != null)
		{
			_helperInstance.PreloadItems(missionBehavior2.GetSiegeMissiles());
		}
		_preloadDone = true;
	}

	public override void OnSceneRenderingStarted()
	{
		_helperInstance.WaitForMeshesToBeLoaded();
	}

	public override void OnMissionStateDeactivated()
	{
		((MissionBehavior)this).OnMissionStateDeactivated();
		_helperInstance.Clear();
	}
}
