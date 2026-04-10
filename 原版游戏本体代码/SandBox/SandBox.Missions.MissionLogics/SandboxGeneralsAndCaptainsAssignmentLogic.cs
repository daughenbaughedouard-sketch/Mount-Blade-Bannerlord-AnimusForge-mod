using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class SandboxGeneralsAndCaptainsAssignmentLogic : GeneralsAndCaptainsAssignmentLogic
{
	public SandboxGeneralsAndCaptainsAssignmentLogic(TextObject attackerGeneralName, TextObject defenderGeneralName, TextObject attackerAllyGeneralName = null, TextObject defenderAllyGeneralName = null, bool createBodyguard = true)
		: base(attackerGeneralName, defenderGeneralName, attackerAllyGeneralName, defenderAllyGeneralName, createBodyguard)
	{
	}

	protected override void SortCaptainsByPriority(Team team, ref List<Agent> captains)
	{
		EncounterModel encounterModel = Campaign.Current.Models.EncounterModel;
		if (encounterModel != null)
		{
			captains = captains.OrderByDescending(delegate(Agent captain)
			{
				if (captain != team.GeneralAgent)
				{
					BasicCharacterObject character = captain.Character;
					CharacterObject val;
					return ((val = (CharacterObject)(object)((character is CharacterObject) ? character : null)) != null && val.HeroObject != null) ? encounterModel.GetCharacterSergeantScore(val.HeroObject) : 0;
				}
				return float.MaxValue;
			}).ToList();
		}
		else
		{
			((GeneralsAndCaptainsAssignmentLogic)this).SortCaptainsByPriority(team, ref captains);
		}
	}
}
