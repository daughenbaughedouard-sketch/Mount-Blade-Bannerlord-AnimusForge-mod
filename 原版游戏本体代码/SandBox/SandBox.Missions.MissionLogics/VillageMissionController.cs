using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class VillageMissionController : MissionLogic
{
	public override void OnCreated()
	{
		((MissionBehavior)this).OnCreated();
		((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment = false;
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		bool isNight = Campaign.Current.IsNight;
		((MissionBehavior)this).Mission.IsInventoryAccessible = true;
		((MissionBehavior)this).Mission.IsQuestScreenAccessible = true;
		MissionAgentHandler missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>();
		SandBoxHelpers.MissionHelper.SpawnPlayer(((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment);
		missionBehavior.SpawnLocationCharacters();
		SandBoxHelpers.MissionHelper.SpawnHorses();
		if (!isNight)
		{
			SandBoxHelpers.MissionHelper.SpawnSheeps();
			SandBoxHelpers.MissionHelper.SpawnCows();
			SandBoxHelpers.MissionHelper.SpawnHogs();
			SandBoxHelpers.MissionHelper.SpawnGeese();
			SandBoxHelpers.MissionHelper.SpawnChicken();
		}
	}
}
