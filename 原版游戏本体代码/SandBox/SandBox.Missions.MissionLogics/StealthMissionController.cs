using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class StealthMissionController : MissionLogic
{
	public override void AfterStart()
	{
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)4, true);
		((MissionBehavior)this).Mission.IsInventoryAccessible = !Campaign.Current.IsMainHeroDisguised;
		((MissionBehavior)this).Mission.IsQuestScreenAccessible = true;
		SandBoxHelpers.MissionHelper.SpawnPlayer(civilianEquipment: false, noHorses: true);
		Mission.Current.GetMissionBehavior<MissionAgentHandler>().SpawnLocationCharacters();
	}
}
