using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;

namespace SandBox.View.Missions;

public class MissionCampaignBattleSpectatorView : MissionView
{
	public override void AfterStart()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((MissionView)this).MissionScreen.SetCustomAgentListToSpectateGatherer(new GatherCustomAgentListToSpectateDelegate(SpectateListGatherer));
	}

	private int CalculateAgentScore(Agent agent)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		Mission mission = agent.Mission;
		CharacterObject val = (CharacterObject)agent.Character;
		int num = (agent.IsPlayerControlled ? 2000000 : 0);
		if (agent.Team != null && agent.Team.IsValid)
		{
			num += ((mission.PlayerTeam == null || !mission.PlayerTeam.IsValid || !agent.Team.IsEnemyOf(mission.PlayerTeam)) ? 1000000 : 0);
			if (agent.Team.GeneralAgent == agent)
			{
				num += 500000;
			}
			else if (((BasicCharacterObject)val).IsHero)
			{
				num = ((!val.HeroObject.IsLord) ? (num + 250000) : (num + 125000));
				foreach (Formation item in (List<Formation>)(object)agent.Team.FormationsIncludingEmpty)
				{
					if (item.Captain == agent)
					{
						num += 100000;
					}
				}
			}
			if (((BasicCharacterObject)val).IsMounted)
			{
				num += 50000;
			}
			if (!((BasicCharacterObject)val).IsRanged)
			{
				num += 25000;
			}
			num += (int)agent.Health;
		}
		return num;
	}

	private List<Agent> SpectateListGatherer(Agent forcedAgentToInclude)
	{
		return LinQuick.WhereQ<Agent>((List<Agent>)(object)((MissionBehavior)this).Mission.AllAgents, (Func<Agent, bool>)((Agent x) => x.IsCameraAttachable() || x == forcedAgentToInclude)).OrderByDescending(CalculateAgentScore).ToList();
	}
}
