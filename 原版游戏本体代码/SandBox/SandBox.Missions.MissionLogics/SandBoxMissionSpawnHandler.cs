using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class SandBoxMissionSpawnHandler : MissionLogic
{
	protected MissionAgentSpawnLogic _missionAgentSpawnLogic;

	protected MapEvent _mapEvent;

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_missionAgentSpawnLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentSpawnLogic>();
		_mapEvent = MapEvent.PlayerMapEvent;
	}

	protected static MissionSpawnSettings CreateSandBoxBattleWaveSpawnSettings()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int reinforcementWaveCount = BannerlordConfig.GetReinforcementWaveCount();
		return new MissionSpawnSettings((InitialSpawnMethod)0, (ReinforcementTimingMethod)0, (ReinforcementSpawnMethod)1, 3f, 0f, 0f, 0.5f, reinforcementWaveCount, 0f, 0f, 1f, 0.75f);
	}
}
