using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCPartySizeLimitModel : PartySizeLimitModel
{
	public override int MinimumNumberOfVillagersAtVillagerParty => ((MBGameModel<PartySizeLimitModel>)this).BaseModel.MinimumNumberOfVillagersAtVillagerParty;

	public override ExplainedNumber CalculateGarrisonPartySizeLimit(Settlement settlement, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.CalculateGarrisonPartySizeLimit(settlement, includeDescriptions);
	}

	public override TroopRoster FindAppropriateInitialRosterForMobileParty(MobileParty party, PartyTemplateObject partyTemplate)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.FindAppropriateInitialRosterForMobileParty(party, partyTemplate);
	}

	public override List<Ship> FindAppropriateInitialShipsForMobileParty(MobileParty party, PartyTemplateObject partyTemplate)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.FindAppropriateInitialShipsForMobileParty(party, partyTemplate);
	}

	public override int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetAssumedPartySizeForLordParty(leaderHero, partyMapFaction, actualClan);
	}

	public override int GetClanTierPartySizeEffectForHero(Hero hero)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetClanTierPartySizeEffectForHero(hero);
	}

	public override int GetIdealVillagerPartySize(Village village)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetIdealVillagerPartySize(village);
	}

	public override int GetNextClanTierPartySizeEffectChangeForHero(Hero hero)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetNextClanTierPartySizeEffectChangeForHero(hero);
	}

	public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (party.IsNavalStorylineQuestParty(out var partyData) && partyData.IsQuestParty)
		{
			return new ExplainedNumber((float)partyData.PartySize, false, (TextObject)null);
		}
		if (party.IsMobile && party.MobileParty.ActualClan != null && party.MobileParty.ActualClan.IsBanditFaction && !party.MobileParty.IsCurrentlyUsedByAQuest && party.MobileParty.HasNavalNavigationCapability)
		{
			return new ExplainedNumber((float)party.MobileParty.ActualClan.DefaultPartyTemplate.GetUpperTroopLimit(), false, (TextObject)null);
		}
		if (party.IsMobile && party.MobileParty.IsPatrolParty && party.MobileParty.PatrolPartyComponent.IsNaval)
		{
			return CalculatePatrolPartySizeLimit(party.MobileParty, includeDescriptions);
		}
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetPartyMemberSizeLimit(party, includeDescriptions);
	}

	private ExplainedNumber CalculatePatrolPartySizeLimit(MobileParty mobileParty, bool includeDescriptions)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new ExplainedNumber((float)mobileParty.HomeSettlement.Culture.SettlementPatrolPartyTemplateNaval.GetUpperTroopLimit(), includeDescriptions, (TextObject)null);
	}

	public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetPartyPrisonerSizeLimit(party, includeDescriptions);
	}
}
