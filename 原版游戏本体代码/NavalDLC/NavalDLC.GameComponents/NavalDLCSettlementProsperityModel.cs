using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCSettlementProsperityModel : SettlementProsperityModel
{
	public override ExplainedNumber CalculateProsperityChange(Town fortification, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<SettlementProsperityModel>)this).BaseModel.CalculateProsperityChange(fortification, includeDescriptions);
	}

	public override ExplainedNumber CalculateHearthChange(Village village, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<SettlementProsperityModel>)this).BaseModel.CalculateHearthChange(village, includeDescriptions);
		if (village.Bound.HasPort && village.Bound.IsFortification)
		{
			PerkHelper.AddPerkBonusForTown(NavalPerks.Shipmaster.FairWinds, village.Bound.Town, ref result);
		}
		return result;
	}
}
