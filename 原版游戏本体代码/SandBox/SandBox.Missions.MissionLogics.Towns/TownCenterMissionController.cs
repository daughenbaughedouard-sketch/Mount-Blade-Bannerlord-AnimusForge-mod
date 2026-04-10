using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics.Towns;

public class TownCenterMissionController : MissionLogic
{
	public override void OnCreated()
	{
		((MissionBehavior)this).OnCreated();
		((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment = true;
	}

	public override void AfterStart()
	{
		bool isNight = Campaign.Current.IsNight;
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)0, true);
		((MissionBehavior)this).Mission.IsInventoryAccessible = !Campaign.Current.IsMainHeroDisguised;
		((MissionBehavior)this).Mission.IsQuestScreenAccessible = true;
		MissionAgentHandler missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>();
		SandBoxHelpers.MissionHelper.SpawnPlayer(((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment, noHorses: true);
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
