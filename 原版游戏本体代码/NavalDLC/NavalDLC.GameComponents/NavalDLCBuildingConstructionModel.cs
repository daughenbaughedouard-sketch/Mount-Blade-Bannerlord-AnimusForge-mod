using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCBuildingConstructionModel : BuildingConstructionModel
{
	public override int TownBoostCost => ((MBGameModel<BuildingConstructionModel>)this).BaseModel.TownBoostCost;

	public override int TownBoostBonus => ((MBGameModel<BuildingConstructionModel>)this).BaseModel.TownBoostBonus;

	public override int CastleBoostCost => ((MBGameModel<BuildingConstructionModel>)this).BaseModel.CastleBoostCost;

	public override int CastleBoostBonus => ((MBGameModel<BuildingConstructionModel>)this).BaseModel.CastleBoostBonus;

	public override ExplainedNumber CalculateDailyConstructionPower(Town town, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<BuildingConstructionModel>)this).BaseModel.CalculateDailyConstructionPower(town, includeDescriptions);
		Clan ownerClan = town.OwnerClan;
		Kingdom val = ((ownerClan != null) ? ownerClan.Kingdom : null);
		if (val != null && val.HasPolicy(NavalPolicies.MaritimeWealEdict) && !((SettlementComponent)town).Settlement.HasPort)
		{
			((ExplainedNumber)(ref result)).AddFactor(0.2f, ((PropertyObject)NavalPolicies.MaritimeWealEdict).Name);
		}
		return result;
	}

	public override int CalculateDailyConstructionPowerWithoutBoost(Town town)
	{
		return ((MBGameModel<BuildingConstructionModel>)this).BaseModel.CalculateDailyConstructionPowerWithoutBoost(town);
	}

	public override int GetBoostCost(Town town)
	{
		return ((MBGameModel<BuildingConstructionModel>)this).BaseModel.GetBoostCost(town);
	}

	public override int GetBoostAmount(Town town)
	{
		return ((MBGameModel<BuildingConstructionModel>)this).BaseModel.GetBoostAmount(town);
	}
}
