using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

public class MissionSingleplayerKillNotificationUIHandler : MissionView
{
	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		if (affectedAgent.IsHuman)
		{
			string text = ((affectorAgent == null) ? string.Empty : affectorAgent.Name);
			string text2 = ((affectedAgent == null) ? string.Empty : affectedAgent.Name);
			uint num = 4291306250u;
			Agent main = Agent.Main;
			if (main != null && ((main.Team != ((MissionBehavior)this).Mission.SpectatorTeam && main.Team != affectedAgent.Team) || (affectorAgent != null && affectorAgent == main)))
			{
				num = 4281589009u;
			}
			TextObject val;
			if (affectorAgent != null)
			{
				val = new TextObject("{=2ZarUUbw}{KILLERPLAYERNAME} has killed {KILLEDPLAYERNAME}!", (Dictionary<string, object>)null);
				val.SetTextVariable("KILLERPLAYERNAME", text);
			}
			else
			{
				val = new TextObject("{=9CnRKZOb}{KILLEDPLAYERNAME} has died!", (Dictionary<string, object>)null);
			}
			val.SetTextVariable("KILLEDPLAYERNAME", text2);
			MessageManager.DisplayMessage(((object)val).ToString(), num);
		}
	}
}
