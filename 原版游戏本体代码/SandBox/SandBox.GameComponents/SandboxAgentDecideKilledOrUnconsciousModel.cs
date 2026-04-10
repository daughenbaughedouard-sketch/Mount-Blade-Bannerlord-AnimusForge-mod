using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace SandBox.GameComponents;

public class SandboxAgentDecideKilledOrUnconsciousModel : AgentDecideKilledOrUnconsciousModel
{
	public override float GetAgentStateProbability(Agent affectorAgent, Agent effectedAgent, DamageTypes damageType, WeaponFlags weaponFlags, out float useSurgeryProbability)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		useSurgeryProbability = 1f;
		if (effectedAgent.IsHuman)
		{
			CharacterObject val = (CharacterObject)effectedAgent.Character;
			if (Campaign.Current != null)
			{
				if (((BasicCharacterObject)val).IsHero && !val.HeroObject.CanDie((KillCharacterActionDetail)4))
				{
					return 0f;
				}
				PartyBase val2 = effectedAgent.GetComponent<CampaignAgentComponent>()?.OwnerParty;
				if (affectorAgent != null && affectorAgent.IsHuman)
				{
					PartyBase val3 = affectorAgent.GetComponent<CampaignAgentComponent>()?.OwnerParty;
					return 1f - Campaign.Current.Models.PartyHealingModel.GetSurvivalChance(val2, val, damageType, Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)17179869184L), val3);
				}
				return 1f - Campaign.Current.Models.PartyHealingModel.GetSurvivalChance(val2, val, damageType, Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)17179869184L), (PartyBase)null);
			}
		}
		return 1f;
	}
}
