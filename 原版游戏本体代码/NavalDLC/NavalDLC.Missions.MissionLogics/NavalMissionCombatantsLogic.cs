using System.Collections.Generic;
using NavalDLC.Missions.AI.Tactics;
using NavalDLC.Missions.AI.TeamAI;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

public class NavalMissionCombatantsLogic : MissionCombatantsLogic
{
	public NavalMissionCombatantsLogic(IEnumerable<IBattleCombatant> battleCombatants, IBattleCombatant playerBattleCombatant, IBattleCombatant defenderLeaderBattleCombatant, IBattleCombatant attackerLeaderBattleCombatant, MissionTeamAITypeEnum teamAIType, bool isPlayerSergeant)
		: base(battleCombatants, playerBattleCombatant, defenderLeaderBattleCombatant, attackerLeaderBattleCombatant, teamAIType, isPlayerSergeant)
	{
	}//IL_0006: Unknown result type (might be due to invalid IL or missing references)


	public override void EarlyStart()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		Mission.Current.MissionTeamAIType = base.TeamAIType;
		foreach (Team item in (List<Team>)(object)Mission.Current.Teams)
		{
			item.AddTeamAI((TeamAIComponent)(object)new TeamAINavalComponent(((MissionBehavior)this).Mission, item, 5f, 1f), false);
		}
		if (((List<Team>)(object)Mission.Current.Teams).Count <= 0)
		{
			return;
		}
		foreach (Team item2 in (List<Team>)(object)Mission.Current.Teams)
		{
			if (item2.HasTeamAi)
			{
				item2.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(item2));
				if ((int)item2.Side == 0)
				{
					item2.AddTacticOption((TacticComponent)(object)new TacticNavalLineDefense(item2));
				}
			}
		}
		foreach (Team item3 in (List<Team>)(object)((MissionBehavior)this).Mission.Teams)
		{
			item3.QuerySystem.Expire();
			item3.ResetTactic();
		}
	}
}
