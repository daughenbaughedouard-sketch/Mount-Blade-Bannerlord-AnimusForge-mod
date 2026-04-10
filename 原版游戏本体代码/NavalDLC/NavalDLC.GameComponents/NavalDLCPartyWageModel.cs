using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCPartyWageModel : PartyWageModel
{
	private const float PartyWageReductionAtSea = 0.2f;

	private const float ConvoyPartyWageCut = -0.8f;

	private readonly TextObject _convoyPartyWageCutText = new TextObject("{=lDxu6pez}Convoy Wage Multiplier", (Dictionary<string, object>)null);

	private readonly TextObject _partyWageReductionAtSea = new TextObject("{=sWhNhHkV}Wage Reduction At Sea", (Dictionary<string, object>)null);

	public override int MaxWagePaymentLimit => ((MBGameModel<PartyWageModel>)this).BaseModel.MaxWagePaymentLimit;

	public override int GetCharacterWage(CharacterObject character)
	{
		return ((MBGameModel<PartyWageModel>)this).BaseModel.GetCharacterWage(character);
	}

	public override ExplainedNumber GetTotalWage(MobileParty mobileParty, TroopRoster troopRoster, bool includeDescriptions = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber totalWage = ((MBGameModel<PartyWageModel>)this).BaseModel.GetTotalWage(mobileParty, troopRoster, includeDescriptions);
		bool flag = !mobileParty.HasPerk(Steward.AidCorps, false);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < troopRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = troopRoster.GetElementCopyAtIndex(i);
			CharacterObject character = elementCopyAtIndex.Character;
			int num3 = (flag ? ((TroopRosterElement)(ref elementCopyAtIndex)).Number : (((TroopRosterElement)(ref elementCopyAtIndex)).Number - ((TroopRosterElement)(ref elementCopyAtIndex)).WoundedNumber));
			if (!((BasicCharacterObject)character).IsHero)
			{
				int num4 = character.TroopWage * num3;
				if (!character.IsMariner)
				{
					num += num4;
				}
				if (((BasicCharacterObject)character).IsMounted)
				{
					num2 += num4;
				}
			}
		}
		if (mobileParty.IsCurrentlyAtSea)
		{
			((ExplainedNumber)(ref totalWage)).AddFactor(-0.2f, _partyWageReductionAtSea);
			if (mobileParty.IsCaravan)
			{
				((ExplainedNumber)(ref totalWage)).AddFactor(-0.8f, _convoyPartyWageCutText);
			}
			if (mobileParty.HasPerk(NavalPerks.Boatswain.Optimization, false))
			{
				float num5 = (float)num / ((ExplainedNumber)(ref totalWage)).BaseNumber;
				if (num5 > 0f)
				{
					float num6 = NavalPerks.Boatswain.Optimization.PrimaryBonus * num5;
					((ExplainedNumber)(ref totalWage)).AddFactor(num6, ((PropertyObject)NavalPerks.Boatswain.Optimization).Name);
				}
			}
			if (mobileParty.HasPerk(NavalPerks.Boatswain.NavalHorde, false))
			{
				float num7 = (float)num2 / ((ExplainedNumber)(ref totalWage)).BaseNumber;
				if (num7 > 0f)
				{
					float num8 = NavalPerks.Boatswain.NavalHorde.PrimaryBonus * num7;
					((ExplainedNumber)(ref totalWage)).AddFactor(num8, ((PropertyObject)NavalPerks.Boatswain.NavalHorde).Name);
				}
			}
		}
		return totalWage;
	}

	public override ExplainedNumber GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber troopRecruitmentCost = ((MBGameModel<PartyWageModel>)this).BaseModel.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost);
		if (buyerHero != null)
		{
			PerkHelper.AddPerkBonusForCharacter(NavalPerks.Boatswain.PopularCaptain, buyerHero.CharacterObject, true, ref troopRecruitmentCost, false);
		}
		return troopRecruitmentCost;
	}
}
