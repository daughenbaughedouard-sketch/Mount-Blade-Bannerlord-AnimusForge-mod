using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace StoryMode.GameComponents;

public class StoryModePrisonerRecruitmentCalculationModel : PrisonerRecruitmentCalculationModel
{
	public override int CalculateRecruitableNumber(PartyBase party, CharacterObject character)
	{
		return ((MBGameModel<PrisonerRecruitmentCalculationModel>)this).BaseModel.CalculateRecruitableNumber(party, character);
	}

	public override ExplainedNumber GetConformityChangePerHour(PartyBase party, CharacterObject character)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (party == PartyBase.MainParty && !StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted)
		{
			return new ExplainedNumber(0f, false, (TextObject)null);
		}
		return ((MBGameModel<PrisonerRecruitmentCalculationModel>)this).BaseModel.GetConformityChangePerHour(party, character);
	}

	public override int GetConformityNeededToRecruitPrisoner(CharacterObject character)
	{
		return ((MBGameModel<PrisonerRecruitmentCalculationModel>)this).BaseModel.GetConformityNeededToRecruitPrisoner(character);
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
}
