using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public static class LocationCharacterMissionExtensions
{
	public static AgentBuildData GetAgentBuildData(this LocationCharacter locationCharacter)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new AgentBuildData(locationCharacter.AgentData);
	}
}
