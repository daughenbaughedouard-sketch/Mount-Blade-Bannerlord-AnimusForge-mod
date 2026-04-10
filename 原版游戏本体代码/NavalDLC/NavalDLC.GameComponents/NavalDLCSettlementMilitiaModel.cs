using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCSettlementMilitiaModel : SettlementMilitiaModel
{
	public override int MilitiaToSpawnAfterSiege(Town town)
	{
		return ((MBGameModel<SettlementMilitiaModel>)this).BaseModel.MilitiaToSpawnAfterSiege(town);
	}

	public override ExplainedNumber CalculateMilitiaChange(Settlement settlement, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<SettlementMilitiaModel>)this).BaseModel.CalculateMilitiaChange(settlement, includeDescriptions);
		if (settlement.IsTown && settlement.HasPort)
		{
			PerkHelper.AddPerkBonusForTown(NavalPerks.Boatswain.AccuracyTraining, settlement.Town, ref result);
		}
		else if (settlement.IsVillage)
		{
			Town town = settlement.Village.Bound.Town;
			if (town != null && ((SettlementComponent)town).Settlement.HasPort && town.Governor != null && town.Governor.GetPerkValue(NavalPerks.Boatswain.AccuracyTraining))
			{
				((ExplainedNumber)(ref result)).Add(NavalPerks.Boatswain.AccuracyTraining.SecondaryBonus, NavalPerks.Boatswain.AccuracyTraining.SecondaryDescription, (TextObject)null);
			}
		}
		Clan ownerClan = settlement.OwnerClan;
		Kingdom val = ((ownerClan != null) ? ownerClan.Kingdom : null);
		if (val != null && val.HasPolicy(NavalPolicies.BolsterTheFyrd))
		{
			((ExplainedNumber)(ref result)).AddFactor(0.25f, ((PropertyObject)NavalPolicies.BolsterTheFyrd).Name);
		}
		return result;
	}

	public override ExplainedNumber CalculateVeteranMilitiaSpawnChance(Settlement settlement)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<SettlementMilitiaModel>)this).BaseModel.CalculateVeteranMilitiaSpawnChance(settlement);
		if (settlement.IsTown && settlement.HasPort)
		{
			PerkHelper.AddPerkBonusForTown(NavalPerks.Mariner.NavalFightingTraining, settlement.Town, ref result);
		}
		if (settlement.IsVillage && settlement.Village.Bound.HasPort)
		{
			PerkHelper.AddPerkBonusForTown(NavalPerks.Mariner.NavalFightingTraining, settlement.Village.Bound.Town, ref result);
		}
		return result;
	}

	public override void CalculateMilitiaSpawnRate(Settlement settlement, out float meleeTroopRate, out float rangedTroopRate)
	{
		((MBGameModel<SettlementMilitiaModel>)this).BaseModel.CalculateMilitiaSpawnRate(settlement, ref meleeTroopRate, ref rangedTroopRate);
	}
}
