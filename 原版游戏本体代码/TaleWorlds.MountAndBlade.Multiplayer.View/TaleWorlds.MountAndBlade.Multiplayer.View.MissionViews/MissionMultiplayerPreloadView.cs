using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;

public class MissionMultiplayerPreloadView : MissionView
{
	private PreloadHelper _helperInstance = new PreloadHelper();

	private bool _preloadDone;

	public override void OnPreMissionTick(float dt)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Invalid comparison between Unknown and I4
		if (_preloadDone)
		{
			return;
		}
		MissionMultiplayerGameModeBaseClient missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		IEnumerable<MPHeroClass> mPHeroClasses = MultiplayerClassDivisions.GetMPHeroClasses(MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1)));
		IEnumerable<MPHeroClass> mPHeroClasses2 = MultiplayerClassDivisions.GetMPHeroClasses(MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1)));
		List<BasicCharacterObject> list = new List<BasicCharacterObject>();
		foreach (MPHeroClass item in mPHeroClasses)
		{
			list.Add(item.HeroCharacter);
			if ((int)missionBehavior.GameType == 4)
			{
				list.Add(item.TroopCharacter);
			}
		}
		foreach (MPHeroClass item2 in mPHeroClasses2)
		{
			list.Add(item2.HeroCharacter);
			if ((int)missionBehavior.GameType == 4)
			{
				list.Add(item2.TroopCharacter);
			}
		}
		_helperInstance.PreloadCharacters(list);
		MissionMultiplayerSiegeClient missionBehavior2 = Mission.Current.GetMissionBehavior<MissionMultiplayerSiegeClient>();
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
