using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCSettlementGarrisonModel : SettlementGarrisonModel
{
	public override int GetMaximumDailyAutoRecruitmentCount(Town town)
	{
		return ((MBGameModel<SettlementGarrisonModel>)this).BaseModel.GetMaximumDailyAutoRecruitmentCount(town);
	}

	public override ExplainedNumber CalculateBaseGarrisonChange(Settlement settlement, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<SettlementGarrisonModel>)this).BaseModel.CalculateBaseGarrisonChange(settlement, includeDescriptions);
	}

	public override int FindNumberOfTroopsToTakeFromGarrison(MobileParty mobileParty, Settlement settlement, float idealGarrisonStrengthPerWalledCenter = 0f)
	{
		return ((MBGameModel<SettlementGarrisonModel>)this).BaseModel.FindNumberOfTroopsToTakeFromGarrison(mobileParty, settlement, idealGarrisonStrengthPerWalledCenter);
	}

	public override int FindNumberOfTroopsToLeaveToGarrison(MobileParty mobileParty, Settlement settlement)
	{
		return ((MBGameModel<SettlementGarrisonModel>)this).BaseModel.FindNumberOfTroopsToLeaveToGarrison(mobileParty, settlement);
	}

	public override float GetMaximumDailyRepairAmount(Settlement settlement)
	{
		return ((MBGameModel<SettlementGarrisonModel>)this).BaseModel.GetMaximumDailyRepairAmount(settlement);
	}
}
