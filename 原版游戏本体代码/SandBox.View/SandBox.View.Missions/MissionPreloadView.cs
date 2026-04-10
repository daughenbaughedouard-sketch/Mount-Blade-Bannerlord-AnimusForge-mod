using System.Collections.Generic;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions;

public class MissionPreloadView : MissionView
{
	private readonly PreloadHelper _helperInstance = new PreloadHelper();

	private bool _preloadDone;

	public override void OnPreMissionTick(float dt)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (_preloadDone)
		{
			return;
		}
		List<BasicCharacterObject> list = new List<BasicCharacterObject>();
		foreach (PartyBase involvedParty in MapEvent.PlayerMapEvent.InvolvedParties)
		{
			foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)involvedParty.MemberRoster.GetTroopRoster())
			{
				TroopRosterElement current = item;
				for (int i = 0; i < ((TroopRosterElement)(ref current)).Number; i++)
				{
					list.Add((BasicCharacterObject)(object)current.Character);
				}
			}
		}
		_helperInstance.PreloadCharacters(list);
		SiegeDeploymentMissionController missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<SiegeDeploymentMissionController>();
		if (missionBehavior != null)
		{
			_helperInstance.PreloadItems(missionBehavior.GetSiegeMissiles());
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

	public override void OnRemoveBehavior()
	{
		((MissionView)this).OnRemoveBehavior();
		_helperInstance.Clear();
	}
}
