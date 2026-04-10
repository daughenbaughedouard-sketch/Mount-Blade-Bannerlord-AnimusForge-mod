using NavalDLC.Settlements.Building;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCBuildingModel : BuildingModel
{
	public override bool CanAddBuildingTypeToTown(BuildingType buildingType, Town town)
	{
		if (buildingType == NavalBuildingTypes.SettlementShipyard)
		{
			if (((SettlementComponent)town).IsTown)
			{
				return ((SettlementComponent)town).Settlement.HasPort;
			}
			return false;
		}
		return ((MBGameModel<BuildingModel>)this).BaseModel.CanAddBuildingTypeToTown(buildingType, town);
	}
}
