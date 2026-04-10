using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCMobilePartyFoodConsumptionModel : MobilePartyFoodConsumptionModel
{
	private const float PartyFoodConsumptionReductionAtSea = 0.2f;

	private readonly TextObject _partyFoodConsumptionReductionAtSea = new TextObject("{=Z1af4yEX}Food Consumption Reduction At Sea", (Dictionary<string, object>)null);

	public override int NumberOfMenOnMapToEatOneFood => ((MBGameModel<MobilePartyFoodConsumptionModel>)this).BaseModel.NumberOfMenOnMapToEatOneFood;

	public override ExplainedNumber CalculateDailyBaseFoodConsumptionf(MobileParty party, bool includeDescription = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<MobilePartyFoodConsumptionModel>)this).BaseModel.CalculateDailyBaseFoodConsumptionf(party, includeDescription);
	}

	public override ExplainedNumber CalculateDailyFoodConsumptionf(MobileParty party, ExplainedNumber baseConsumption)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<MobilePartyFoodConsumptionModel>)this).BaseModel.CalculateDailyFoodConsumptionf(party, baseConsumption);
		if (party.IsCurrentlyAtSea)
		{
			((ExplainedNumber)(ref result)).AddFactor(-0.2f, _partyFoodConsumptionReductionAtSea);
			PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.SmoothOperator, party, false, ref result, false);
		}
		return result;
	}

	public override bool DoesPartyConsumeFood(MobileParty mobileParty)
	{
		return ((MBGameModel<MobilePartyFoodConsumptionModel>)this).BaseModel.DoesPartyConsumeFood(mobileParty);
	}
}
