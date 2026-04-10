using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class BattleSurgeonLogic : MissionLogic
{
	private Dictionary<string, Agent> _surgeonAgents = new Dictionary<string, Agent>();

	protected override void OnGetAgentState(Agent agent, bool usedSurgery)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (usedSurgery)
		{
			PartyBase ownerParty = agent.GetComponent<CampaignAgentComponent>().OwnerParty;
			if (ownerParty != null && _surgeonAgents.TryGetValue(ownerParty.Id, out var value) && (int)value.State == 1)
			{
				SkillLevelingManager.OnSurgeryApplied(ownerParty.MobileParty, true, ((CharacterObject)agent.Character).Tier);
			}
		}
	}

	public override void OnAgentCreated(Agent agent)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		((MissionBehavior)this).OnAgentCreated(agent);
		CharacterObject val = (CharacterObject)agent.Character;
		object obj;
		if (val == null)
		{
			obj = null;
		}
		else
		{
			Hero heroObject = val.HeroObject;
			obj = ((heroObject != null) ? heroObject.PartyBelongedTo : null);
		}
		if (obj != null && val.HeroObject == val.HeroObject.PartyBelongedTo.EffectiveSurgeon)
		{
			string id = val.HeroObject.PartyBelongedTo.Party.Id;
			if (_surgeonAgents.ContainsKey(id))
			{
				_surgeonAgents.Remove(id);
			}
			_surgeonAgents.Add(id, agent);
		}
	}
}
