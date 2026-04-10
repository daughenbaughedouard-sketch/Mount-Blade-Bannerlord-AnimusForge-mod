using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCVillageProductionCalculatorModel : VillageProductionCalculatorModel
{
	public override float CalculateProductionSpeedOfItemCategory(ItemCategory item)
	{
		return ((MBGameModel<VillageProductionCalculatorModel>)this).BaseModel.CalculateProductionSpeedOfItemCategory(item);
	}

	public override ExplainedNumber CalculateDailyProductionAmount(Village village, ItemObject item)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<VillageProductionCalculatorModel>)this).BaseModel.CalculateDailyProductionAmount(village, item);
		if (village.TradeBound != null)
		{
			if (item.ItemCategory == NavalItemCategories.WalrusTusk || item.ItemCategory == NavalItemCategories.WhaleOil)
			{
				PerkHelper.AddPerkBonusForTown(NavalPerks.Boatswain.PortAuthority, village.TradeBound.Town, ref result);
			}
			if (item.ItemCategory == DefaultItemCategories.Fish)
			{
				PerkHelper.AddPerkBonusForTown(NavalPerks.Boatswain.BlessingsOfTheSea, village.TradeBound.Town, ref result);
			}
		}
		Clan ownerClan = village.Bound.OwnerClan;
		Kingdom val = ((ownerClan != null) ? ownerClan.Kingdom : null);
		if (val != null)
		{
			if (val.HasPolicy(NavalPolicies.MaritimeWealEdict))
			{
				((ExplainedNumber)(ref result)).AddFactor(0.25f, ((PropertyObject)NavalPolicies.MaritimeWealEdict).Name);
			}
			if (val.HasPolicy(NavalPolicies.BolsterTheFyrd))
			{
				((ExplainedNumber)(ref result)).AddFactor(-0.05f, ((PropertyObject)NavalPolicies.BolsterTheFyrd).Name);
			}
		}
		return result;
	}

	public override float CalculateDailyFoodProductionAmount(Village village)
	{
		return ((MBGameModel<VillageProductionCalculatorModel>)this).BaseModel.CalculateDailyFoodProductionAmount(village);
	}
}
