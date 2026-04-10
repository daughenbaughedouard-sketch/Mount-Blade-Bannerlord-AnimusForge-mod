using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

internal class NavalDLCPrisonerRecruitmentCalculationModel : PrisonerRecruitmentCalculationModel
{
	public override int GetConformityNeededToRecruitPrisoner(CharacterObject character)
	{
		return ((MBGameModel<PrisonerRecruitmentCalculationModel>)this).BaseModel.GetConformityNeededToRecruitPrisoner(character);
	}

	public override ExplainedNumber GetConformityChangePerHour(PartyBase party, CharacterObject troopToBoost)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber conformityChangePerHour = ((MBGameModel<PrisonerRecruitmentCalculationModel>)this).BaseModel.GetConformityChangePerHour(party, troopToBoost);
		if (party.IsMobile && party.MobileParty.IsCurrentlyAtSea && troopToBoost.IsPirate())
		{
			PerkHelper.AddPerkBonusForParty(NavalPerks.Mariner.RollingThunder, party.MobileParty, false, ref conformityChangePerHour, false);
		}
		return conformityChangePerHour;
	}

	public override int GetPrisonerRecruitmentMoraleEffect(PartyBase party, CharacterObject character, int num)
	{
		return ((MBGameModel<PrisonerRecruitmentCalculationModel>)this).BaseModel.GetPrisonerRecruitmentMoraleEffect(party, character, num);
	}

	public override bool IsPrisonerRecruitable(PartyBase party, CharacterObject character, out int conformityNeeded)
	{
		return ((MBGameModel<PrisonerRecruitmentCalculationModel>)this).BaseModel.IsPrisonerRecruitable(party, character, ref conformityNeeded);
	}

	public override bool ShouldPartyRecruitPrisoners(PartyBase party)
	{
		return ((MBGameModel<PrisonerRecruitmentCalculationModel>)this).BaseModel.ShouldPartyRecruitPrisoners(party);
	}

	public override int CalculateRecruitableNumber(PartyBase party, CharacterObject character)
	{
		return ((MBGameModel<PrisonerRecruitmentCalculationModel>)this).BaseModel.CalculateRecruitableNumber(party, character);
	}
}
