using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Storyline.MissionControllers;

internal class Quest5BattleObserverMissionLogic : BattleObserverMissionLogic
{
	private bool _isGunnarAddedBefore;

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((object)affectedAgent.Character != NavalStorylineData.Gunnar.CharacterObject)
		{
			((BattleObserverMissionLogic)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
		}
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if ((object)agent.Character == NavalStorylineData.Gunnar.CharacterObject)
		{
			if (!_isGunnarAddedBefore)
			{
				_isGunnarAddedBefore = true;
				((BattleObserverMissionLogic)this).OnAgentBuild(agent, banner);
			}
		}
		else
		{
			((BattleObserverMissionLogic)this).OnAgentBuild(agent, banner);
		}
	}
}
