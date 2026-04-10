using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCPartyTrainingModel : PartyTrainingModel
{
	public override int GenerateSharedXp(CharacterObject troop, int xp, MobileParty mobileParty)
	{
		return ((MBGameModel<PartyTrainingModel>)this).BaseModel.GenerateSharedXp(troop, xp, mobileParty);
	}

	public override ExplainedNumber CalculateXpGainFromBattles(FlattenedTroopRosterElement troopRosterElement, PartyBase party)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<PartyTrainingModel>)this).BaseModel.CalculateXpGainFromBattles(troopRosterElement, party);
		CharacterObject troop = ((FlattenedTroopRosterElement)(ref troopRosterElement)).Troop;
		if (!((BasicCharacterObject)troop).IsHero)
		{
			if (troop.IsMariner)
			{
				PerkHelper.AddPerkBonusForParty(NavalPerks.Mariner.Arr, party.MobileParty, false, ref result, false);
			}
			if (troop.IsRegular)
			{
				PerkHelper.AddPerkBonusForParty(NavalPerks.Mariner.PirateHunter, party.MobileParty, false, ref result, false);
			}
		}
		return result;
	}

	public override int GetXpReward(CharacterObject character)
	{
		return ((MBGameModel<PartyTrainingModel>)this).BaseModel.GetXpReward(character);
	}

	public override ExplainedNumber GetEffectiveDailyExperience(MobileParty party, TroopRosterElement troop)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<PartyTrainingModel>)this).BaseModel.GetEffectiveDailyExperience(party, troop);
	}
}
