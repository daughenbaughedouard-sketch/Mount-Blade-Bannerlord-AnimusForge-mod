using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

internal class NavalDLCPartyTroopUpgradeModel : PartyTroopUpgradeModel
{
	public override bool CanPartyUpgradeTroopToTarget(PartyBase party, CharacterObject character, CharacterObject target)
	{
		return ((MBGameModel<PartyTroopUpgradeModel>)this).BaseModel.CanPartyUpgradeTroopToTarget(party, character, target);
	}

	public override bool IsTroopUpgradeable(PartyBase party, CharacterObject character)
	{
		return ((MBGameModel<PartyTroopUpgradeModel>)this).BaseModel.IsTroopUpgradeable(party, character);
	}

	public override bool DoesPartyHaveRequiredItemsForUpgrade(PartyBase party, CharacterObject upgradeTarget)
	{
		return ((MBGameModel<PartyTroopUpgradeModel>)this).BaseModel.DoesPartyHaveRequiredItemsForUpgrade(party, upgradeTarget);
	}

	public override bool DoesPartyHaveRequiredPerksForUpgrade(PartyBase party, CharacterObject character, CharacterObject upgradeTarget, out PerkObject requiredPerk)
	{
		return ((MBGameModel<PartyTroopUpgradeModel>)this).BaseModel.DoesPartyHaveRequiredPerksForUpgrade(party, character, upgradeTarget, ref requiredPerk);
	}

	public override ExplainedNumber GetGoldCostForUpgrade(PartyBase party, CharacterObject characterObject, CharacterObject upgradeTarget)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber goldCostForUpgrade = ((MBGameModel<PartyTroopUpgradeModel>)this).BaseModel.GetGoldCostForUpgrade(party, characterObject, upgradeTarget);
		if (party.IsMobile && characterObject.IsMariner && !((BasicCharacterObject)characterObject).IsHero)
		{
			PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.EfficientCaptain, party.MobileParty, true, ref goldCostForUpgrade, false);
		}
		return goldCostForUpgrade;
	}

	public override int GetXpCostForUpgrade(PartyBase party, CharacterObject characterObject, CharacterObject upgradeTarget)
	{
		return ((MBGameModel<PartyTroopUpgradeModel>)this).BaseModel.GetXpCostForUpgrade(party, characterObject, upgradeTarget);
	}

	public override int GetSkillXpFromUpgradingTroops(PartyBase party, CharacterObject troop, int numberOfTroops)
	{
		return ((MBGameModel<PartyTroopUpgradeModel>)this).BaseModel.GetSkillXpFromUpgradingTroops(party, troop, numberOfTroops);
	}

	public override float GetUpgradeChanceForTroopUpgrade(PartyBase party, CharacterObject troop, int upgradeTargetIndex)
	{
		return ((MBGameModel<PartyTroopUpgradeModel>)this).BaseModel.GetUpgradeChanceForTroopUpgrade(party, troop, upgradeTargetIndex);
	}
}
