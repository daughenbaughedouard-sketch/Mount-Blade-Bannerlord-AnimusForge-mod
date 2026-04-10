using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.GameComponents;

public class NavalDLCWorkshopModel : WorkshopModel
{
	public override int DaysForPlayerSaveWorkshopFromBankruptcy => ((MBGameModel<WorkshopModel>)this).BaseModel.DaysForPlayerSaveWorkshopFromBankruptcy;

	public override int CapitalLowLimit => ((MBGameModel<WorkshopModel>)this).BaseModel.CapitalLowLimit;

	public override int InitialCapital => ((MBGameModel<WorkshopModel>)this).BaseModel.InitialCapital;

	public override int DailyExpense => ((MBGameModel<WorkshopModel>)this).BaseModel.DailyExpense;

	public override int WarehouseCapacity => ((MBGameModel<WorkshopModel>)this).BaseModel.WarehouseCapacity;

	public override int DefaultWorkshopCountInSettlement => ((MBGameModel<WorkshopModel>)this).BaseModel.DefaultWorkshopCountInSettlement;

	public override int MaximumWorkshopsPlayerCanHave => ((MBGameModel<WorkshopModel>)this).BaseModel.MaximumWorkshopsPlayerCanHave;

	public override int GetMaxWorkshopCountForClanTier(int tier)
	{
		return ((MBGameModel<WorkshopModel>)this).BaseModel.GetMaxWorkshopCountForClanTier(tier);
	}

	public override int GetCostForPlayer(Workshop workshop)
	{
		return ((MBGameModel<WorkshopModel>)this).BaseModel.GetCostForPlayer(workshop);
	}

	public override int GetCostForNotable(Workshop workshop)
	{
		return ((MBGameModel<WorkshopModel>)this).BaseModel.GetCostForNotable(workshop);
	}

	public override Hero GetNotableOwnerForWorkshop(Workshop workshop)
	{
		return ((MBGameModel<WorkshopModel>)this).BaseModel.GetNotableOwnerForWorkshop(workshop);
	}

	public override ExplainedNumber GetEffectiveConversionSpeedOfProduction(Workshop workshop, float speed, bool includeDescriptions)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber effectiveConversionSpeedOfProduction = ((MBGameModel<WorkshopModel>)this).BaseModel.GetEffectiveConversionSpeedOfProduction(workshop, speed, includeDescriptions);
		Clan clan = ((SettlementArea)workshop).Owner.Clan;
		Kingdom val = ((clan != null) ? clan.Kingdom : null);
		if (val != null)
		{
			if (val.HasPolicy(NavalPolicies.RoyalNavyPrerogative) && (((MBObjectBase)workshop.WorkshopType).StringId == "smithy" || ((MBObjectBase)workshop.WorkshopType).StringId == "wood_WorkshopType"))
			{
				((ExplainedNumber)(ref effectiveConversionSpeedOfProduction)).AddFactor(-0.05f, ((PropertyObject)NavalPolicies.RoyalNavyPrerogative).Name);
			}
			if (val.HasPolicy(NavalPolicies.MaritimeWealEdict) && ((SettlementArea)workshop).Settlement.HasPort)
			{
				((ExplainedNumber)(ref effectiveConversionSpeedOfProduction)).AddFactor(0.25f, ((PropertyObject)NavalPolicies.MaritimeWealEdict).Name);
			}
		}
		return effectiveConversionSpeedOfProduction;
	}

	public override int GetConvertProductionCost(WorkshopType workshopType)
	{
		return ((MBGameModel<WorkshopModel>)this).BaseModel.GetConvertProductionCost(workshopType);
	}

	public override bool CanPlayerSellWorkshop(Workshop workshop, out TextObject explanation)
	{
		return ((MBGameModel<WorkshopModel>)this).BaseModel.CanPlayerSellWorkshop(workshop, ref explanation);
	}

	public override float GetTradeXpPerWarehouseProduction(EquipmentElement production)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<WorkshopModel>)this).BaseModel.GetTradeXpPerWarehouseProduction(production);
	}
}
