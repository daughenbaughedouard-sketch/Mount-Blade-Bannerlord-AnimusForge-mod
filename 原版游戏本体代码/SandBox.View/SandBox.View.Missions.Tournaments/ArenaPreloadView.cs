using System.Collections.Generic;
using SandBox.Missions.MissionLogics.Arena;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions.Tournaments;

internal class ArenaPreloadView : MissionView
{
	private readonly PreloadHelper _helperInstance = new PreloadHelper();

	private bool _preloadDone;

	public override void OnPreMissionTick(float dt)
	{
		if (_preloadDone)
		{
			return;
		}
		List<BasicCharacterObject> list = new List<BasicCharacterObject>();
		if (Mission.Current.GetMissionBehavior<ArenaPracticeFightMissionController>() != null)
		{
			foreach (CharacterObject participantCharacter in ArenaPracticeFightMissionController.GetParticipantCharacters(Settlement.CurrentSettlement))
			{
				list.Add((BasicCharacterObject)(object)participantCharacter);
			}
			list.Add((BasicCharacterObject)(object)CharacterObject.PlayerCharacter);
		}
		TournamentBehavior missionBehavior = Mission.Current.GetMissionBehavior<TournamentBehavior>();
		if (missionBehavior != null)
		{
			foreach (CharacterObject item in (List<CharacterObject>)(object)missionBehavior.GetAllPossibleParticipants())
			{
				list.Add((BasicCharacterObject)(object)item);
			}
		}
		_helperInstance.PreloadCharacters(list);
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
