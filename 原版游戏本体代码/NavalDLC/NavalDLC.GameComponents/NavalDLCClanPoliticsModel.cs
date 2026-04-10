using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCClanPoliticsModel : ClanPoliticsModel
{
	public override ExplainedNumber CalculateInfluenceChange(Clan clan, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<ClanPoliticsModel>)this).BaseModel.CalculateInfluenceChange(clan, includeDescriptions);
		if (clan.Kingdom != null && !clan.IsUnderMercenaryService && clan.Kingdom.HasPolicy(NavalPolicies.NavalConjoiningStatute))
		{
			List<Ship> source = ((IEnumerable<Hero>)clan.AliveLords).Where((Hero x) => x.PartyBelongedTo != null).SelectMany((Hero x) => (IEnumerable<Ship>)x.PartyBelongedTo.Ships).ToList();
			if (source.Any((Ship x) => (int)x.ShipHull.Type == 2))
			{
				((ExplainedNumber)(ref result)).Add(1f, ((PropertyObject)NavalPolicies.NavalConjoiningStatute).Name, (TextObject)null);
			}
			else if (source.All((Ship x) => (int)x.ShipHull.Type == 0))
			{
				((ExplainedNumber)(ref result)).Add(-1f, ((PropertyObject)NavalPolicies.NavalConjoiningStatute).Name, (TextObject)null);
			}
		}
		return result;
	}

	public override float CalculateSupportForPolicyInClan(Clan clan, PolicyObject policy)
	{
		return ((MBGameModel<ClanPoliticsModel>)this).BaseModel.CalculateSupportForPolicyInClan(clan, policy);
	}

	public override float CalculateRelationshipChangeWithSponsor(Clan clan, Clan sponsorClan)
	{
		return ((MBGameModel<ClanPoliticsModel>)this).BaseModel.CalculateRelationshipChangeWithSponsor(clan, sponsorClan);
	}

	public override int GetInfluenceRequiredToOverrideKingdomDecision(DecisionOutcome popularOption, DecisionOutcome overridingOption, KingdomDecision decision)
	{
		return ((MBGameModel<ClanPoliticsModel>)this).BaseModel.GetInfluenceRequiredToOverrideKingdomDecision(popularOption, overridingOption, decision);
	}

	public override bool CanHeroBeGovernor(Hero hero)
	{
		return ((MBGameModel<ClanPoliticsModel>)this).BaseModel.CanHeroBeGovernor(hero);
	}
}
