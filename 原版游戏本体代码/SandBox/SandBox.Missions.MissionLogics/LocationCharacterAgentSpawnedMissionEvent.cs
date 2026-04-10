using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Engine;
using TaleWorlds.Library.EventSystem;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class LocationCharacterAgentSpawnedMissionEvent : EventBase
{
	public readonly LocationCharacter LocationCharacter;

	public readonly Agent Agent;

	public readonly WeakGameEntity SpawnedOnGameEntity;

	public LocationCharacterAgentSpawnedMissionEvent(LocationCharacter locationCharacter, Agent agent, WeakGameEntity spawnedOnGameEntity)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		LocationCharacter = locationCharacter;
		Agent = agent;
		SpawnedOnGameEntity = spawnedOnGameEntity;
	}
}
