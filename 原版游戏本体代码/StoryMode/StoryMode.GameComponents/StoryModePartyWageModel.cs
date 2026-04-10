using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents;

public class StoryModePartyWageModel : PartyWageModel
{
	private const int StoryModeTutorialTroopCost = 50;

	public override int MaxWagePaymentLimit => ((MBGameModel<PartyWageModel>)this).BaseModel.MaxWagePaymentLimit;

	public override int GetCharacterWage(CharacterObject character)
	{
		return ((MBGameModel<PartyWageModel>)this).BaseModel.GetCharacterWage(character);
	}

	public override ExplainedNumber GetTotalWage(MobileParty mobileParty, TroopRoster troopRoster, bool includeDescriptions = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<PartyWageModel>)this).BaseModel.GetTotalWage(mobileParty, troopRoster, includeDescriptions);
	}

	public override ExplainedNumber GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted)
		{
			return ((MBGameModel<PartyWageModel>)this).BaseModel.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost);
		}
		if (!(((MBObjectBase)troop).StringId == "tutorial_placeholder_volunteer"))
		{
			return ((MBGameModel<PartyWageModel>)this).BaseModel.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost);
		}
		return new ExplainedNumber(50f, false, (TextObject)null);
	}
}
