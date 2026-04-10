using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace SandBox;

public class SandboxBattleBannerBearersModel : BattleBannerBearersModel
{
	private static readonly int[] BannerBearerPriorityPerTier = new int[7] { 0, 1, 3, 5, 6, 4, 2 };

	public override int GetMinimumFormationTroopCountToBearBanners()
	{
		return 2;
	}

	public override float GetBannerInteractionDistance(Agent interactingAgent)
	{
		if (!interactingAgent.HasMount)
		{
			return 1.5f;
		}
		return 3f;
	}

	public override bool CanBannerBearerProvideEffectToFormation(Agent agent, Formation formation)
	{
		if (agent.Formation != formation)
		{
			if (agent.IsPlayerControlled)
			{
				return agent.Team == formation.Team;
			}
			return false;
		}
		return true;
	}

	public override bool CanAgentPickUpAnyBanner(Agent agent)
	{
		if (agent.IsHuman && agent.Banner == null && agent.CanBeAssignedForScriptedMovement() && (agent.CommonAIComponent == null || !agent.CommonAIComponent.IsPanicked))
		{
			if (agent.HumanAIComponent != null)
			{
				return !agent.HumanAIComponent.IsInImportantCombatAction();
			}
			return true;
		}
		return false;
	}

	public override bool CanAgentBecomeBannerBearer(Agent agent)
	{
		if (agent.IsHuman && !agent.IsMainAgent && !agent.IsHero && agent.IsAIControlled)
		{
			return agent.Character is CharacterObject;
		}
		return false;
	}

	public override int GetAgentBannerBearingPriority(Agent agent)
	{
		if (!((BattleBannerBearersModel)this).CanAgentBecomeBannerBearer(agent))
		{
			return 0;
		}
		if (agent.Formation != null)
		{
			bool calculateHasSignificantNumberOfMounted = agent.Formation.CalculateHasSignificantNumberOfMounted;
			if ((calculateHasSignificantNumberOfMounted && !agent.HasMount) || (!calculateHasSignificantNumberOfMounted && agent.HasMount))
			{
				return 0;
			}
		}
		if (agent.Banner != null)
		{
			return int.MaxValue;
		}
		int num = 0;
		BasicCharacterObject character = agent.Character;
		CharacterObject val;
		if ((val = (CharacterObject)(object)((character is CharacterObject) ? character : null)) != null)
		{
			int num2 = Math.Min(val.Tier, BannerBearerPriorityPerTier.Length - 1);
			num += BannerBearerPriorityPerTier[num2];
		}
		return num;
	}

	public override bool CanFormationDeployBannerBearers(Formation formation)
	{
		BannerBearerLogic bannerBearerLogic = ((BattleBannerBearersModel)this).BannerBearerLogic;
		if (bannerBearerLogic == null || formation.CountOfUnits < ((BattleBannerBearersModel)this).GetMinimumFormationTroopCountToBearBanners() || bannerBearerLogic.GetFormationBanner(formation) == null)
		{
			return false;
		}
		Agent val;
		return ((IEnumerable<IFormationUnit>)formation.UnitsWithoutLooseDetachedOnes).Count((IFormationUnit unit) => (val = (Agent)(object)((unit is Agent) ? unit : null)) != null && ((BattleBannerBearersModel)this).CanAgentBecomeBannerBearer(val)) > 0;
	}

	public override int GetDesiredNumberOfBannerBearersForFormation(Formation formation)
	{
		if (!((BattleBannerBearersModel)this).CanFormationDeployBannerBearers(formation))
		{
			return 0;
		}
		return 1;
	}

	public override ItemObject GetBannerBearerReplacementWeapon(BasicCharacterObject agentCharacter)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected I4, but got Unknown
		CharacterObject val;
		CultureObject val2;
		if ((val = (CharacterObject)(object)((agentCharacter is CharacterObject) ? agentCharacter : null)) != null && (val2 = (CultureObject)/*isinst with value type is only supported in some contexts*/) != null && !Extensions.IsEmpty<ItemObject>((IEnumerable<ItemObject>)val2.BannerBearerReplacementWeapons))
		{
			List<(int, ItemObject)> list = new List<(int, ItemObject)>();
			int minTierDifference = int.MaxValue;
			foreach (ItemObject item in (List<ItemObject>)(object)val2.BannerBearerReplacementWeapons)
			{
				int num = MathF.Abs(item.Tier + 1 - val.Tier);
				if (num < minTierDifference)
				{
					minTierDifference = num;
				}
				list.Add((num, item));
			}
			return Extensions.GetRandomElementInefficiently<(int, ItemObject)>(list.Where<(int, ItemObject)>(((int TierDifference, ItemObject Weapon) tuple) => tuple.TierDifference == minTierDifference)).Item2;
		}
		return null;
	}
}
