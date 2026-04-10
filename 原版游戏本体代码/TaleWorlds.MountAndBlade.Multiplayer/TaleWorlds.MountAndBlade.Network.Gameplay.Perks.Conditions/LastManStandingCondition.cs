using System.Xml;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Conditions;

public class LastManStandingCondition : MPPerkCondition
{
	protected static string StringType = "LastManStanding";

	public override PerkEventFlags EventFlags => (PerkEventFlags)272;

	protected LastManStandingCondition()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
	}

	public override bool Check(MissionPeer peer)
	{
		return ((MPPerkCondition)this).Check((peer != null) ? peer.ControlledAgent : null);
	}

	public override bool Check(Agent agent)
	{
		agent = ((agent != null && agent.IsMount) ? agent.RiderAgent : agent);
		MissionPeer val = ((agent != null) ? agent.MissionPeer : null) ?? ((agent != null) ? agent.OwningAgentMissionPeer : null);
		if (MultiplayerOptionsExtensions.GetIntValue((OptionType)20, (MultiplayerOptionsAccessMode)1) > 0 && ((val != null) ? val.ControlledFormation : null) != null && agent.IsActive())
		{
			if (!agent.IsPlayerControlled)
			{
				return val.BotsUnderControlAlive == 1;
			}
			return val.BotsUnderControlAlive == 0;
		}
		return false;
	}
}
