using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;

public class MissionMultiplayerKillNotificationUIHandler : MissionView
{
	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		if (!GameNetwork.IsDedicatedServer && affectedAgent.IsHuman)
		{
			string text = ((affectorAgent == null) ? string.Empty : ((affectorAgent.MissionPeer != null) ? affectorAgent.MissionPeer.DisplayedName : affectorAgent.Name));
			string text2 = ((affectedAgent.MissionPeer != null) ? affectedAgent.MissionPeer.DisplayedName : affectedAgent.Name);
			uint num = 4291306250u;
			MissionPeer val = null;
			if (GameNetwork.MyPeer != null)
			{
				val = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
			}
			if (val != null && ((val.Team != ((MissionBehavior)this).Mission.SpectatorTeam && val.Team != affectedAgent.Team) || (affectorAgent != null && affectorAgent.MissionPeer == val)))
			{
				num = 4281589009u;
			}
			TextObject val2;
			if (affectorAgent != null)
			{
				val2 = new TextObject("{=2ZarUUbw}{KILLERPLAYERNAME} has killed {KILLEDPLAYERNAME}!", (Dictionary<string, object>)null);
				val2.SetTextVariable("KILLERPLAYERNAME", text);
			}
			else
			{
				val2 = new TextObject("{=9CnRKZOb}{KILLEDPLAYERNAME} has died!", (Dictionary<string, object>)null);
			}
			val2.SetTextVariable("KILLEDPLAYERNAME", text2);
			MessageManager.DisplayMessage(((object)val2).ToString(), num);
		}
	}
}
