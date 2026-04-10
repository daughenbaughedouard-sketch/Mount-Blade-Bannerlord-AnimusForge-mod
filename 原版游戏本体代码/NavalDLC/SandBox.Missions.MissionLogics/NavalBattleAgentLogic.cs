using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class NavalBattleAgentLogic : BattleAgentLogic
{
	private NavalAgentsLogic _agentsLogic;

	public override void OnBehaviorInitialize()
	{
		_agentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		((MissionBehavior)this).OnBehaviorInitialize();
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!_agentsLogic.IsDeploymentMode && !_agentsLogic.IsMissionEnding)
		{
			((BattleAgentLogic)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		}
	}
}
