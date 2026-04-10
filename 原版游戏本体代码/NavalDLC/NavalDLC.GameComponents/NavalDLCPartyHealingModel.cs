using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCPartyHealingModel : PartyHealingModel
{
	public override float GetSurgeryChance(PartyBase party)
	{
		return ((MBGameModel<PartyHealingModel>)this).BaseModel.GetSurgeryChance(party);
	}

	public override float GetSurvivalChance(PartyBase party, CharacterObject agentCharacter, DamageTypes damageType, bool canDamageKillEvenIfBlunt, PartyBase enemyParty = null)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<PartyHealingModel>)this).BaseModel.GetSurvivalChance(party, agentCharacter, damageType, canDamageKillEvenIfBlunt, enemyParty);
	}

	public override int GetSkillXpFromHealingTroop(PartyBase party)
	{
		return ((MBGameModel<PartyHealingModel>)this).BaseModel.GetSkillXpFromHealingTroop(party);
	}

	public override ExplainedNumber GetDailyHealingForRegulars(PartyBase partyBase, bool isPrisoner, bool includeDescriptions = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber dailyHealingForRegulars = ((MBGameModel<PartyHealingModel>)this).BaseModel.GetDailyHealingForRegulars(partyBase, isPrisoner, includeDescriptions);
		if (partyBase.IsMobile && partyBase.MobileParty.IsCurrentlyAtSea)
		{
			PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.Resilience, partyBase.MobileParty, false, ref dailyHealingForRegulars, false);
		}
		return dailyHealingForRegulars;
	}

	public override ExplainedNumber GetDailyHealingHpForHeroes(PartyBase partyBase, bool isPrisoners, bool includeDescriptions = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<PartyHealingModel>)this).BaseModel.GetDailyHealingHpForHeroes(partyBase, isPrisoners, includeDescriptions);
	}

	public override int GetHeroesEffectedHealingAmount(Hero hero, float healingRate)
	{
		return ((MBGameModel<PartyHealingModel>)this).BaseModel.GetHeroesEffectedHealingAmount(hero, healingRate);
	}

	public override float GetSiegeBombardmentHitSurgeryChance(PartyBase party)
	{
		return ((MBGameModel<PartyHealingModel>)this).BaseModel.GetSiegeBombardmentHitSurgeryChance(party);
	}

	public override ExplainedNumber GetBattleEndHealingAmount(PartyBase partyBase, Hero hero)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber battleEndHealingAmount = ((MBGameModel<PartyHealingModel>)this).BaseModel.GetBattleEndHealingAmount(partyBase, hero);
		if (hero.GetPerkValue(NavalPerks.Boatswain.Resilience))
		{
			((ExplainedNumber)(ref battleEndHealingAmount)).Add(NavalPerks.Boatswain.Resilience.PrimaryBonus * (float)(hero.MaxHitPoints - hero.HitPoints), ((PropertyObject)NavalPerks.Boatswain.Resilience).Name, (TextObject)null);
		}
		return battleEndHealingAmount;
	}
}
