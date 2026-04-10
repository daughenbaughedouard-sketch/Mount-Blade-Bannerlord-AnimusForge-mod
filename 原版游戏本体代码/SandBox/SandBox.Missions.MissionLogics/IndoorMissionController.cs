using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class IndoorMissionController : MissionLogic
{
	private MissionAgentHandler _missionAgentHandler;

	public override void OnCreated()
	{
		((MissionBehavior)this).OnCreated();
		((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment = true;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_missionAgentHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>();
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)0, true);
		((MissionBehavior)this).Mission.IsInventoryAccessible = !Campaign.Current.IsMainHeroDisguised;
		((MissionBehavior)this).Mission.IsQuestScreenAccessible = true;
		SandBoxHelpers.MissionHelper.SpawnPlayer(((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment, noHorses: true);
		_missionAgentHandler.SpawnLocationCharacters();
	}
}
