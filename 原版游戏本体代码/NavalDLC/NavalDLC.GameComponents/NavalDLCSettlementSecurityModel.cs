using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCSettlementSecurityModel : SettlementSecurityModel
{
	public override int MaximumSecurityInSettlement => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.MaximumSecurityInSettlement;

	public override int SecurityDriftMedium => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.SecurityDriftMedium;

	public override float MapEventSecurityEffectRadius => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.MapEventSecurityEffectRadius;

	public override float HideoutClearedSecurityEffectRadius => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.HideoutClearedSecurityEffectRadius;

	public override int HideoutClearedSecurityGain => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.HideoutClearedSecurityGain;

	public override int ThresholdForTaxCorruption => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.ThresholdForTaxCorruption;

	public override int ThresholdForHigherTaxCorruption => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.ThresholdForHigherTaxCorruption;

	public override int ThresholdForTaxBoost => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.ThresholdForTaxBoost;

	public override int SettlementTaxBoostPercentage => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.SettlementTaxBoostPercentage;

	public override int SettlementTaxPenaltyPercentage => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.SettlementTaxPenaltyPercentage;

	public override int ThresholdForNotableRelationBonus => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.ThresholdForNotableRelationBonus;

	public override int ThresholdForNotableRelationPenalty => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.ThresholdForNotableRelationPenalty;

	public override int DailyNotableRelationBonus => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.DailyNotableRelationBonus;

	public override int DailyNotableRelationPenalty => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.DailyNotableRelationPenalty;

	public override int DailyNotablePowerBonus => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.DailyNotablePowerBonus;

	public override int DailyNotablePowerPenalty => ((MBGameModel<SettlementSecurityModel>)this).BaseModel.DailyNotablePowerPenalty;

	public override ExplainedNumber CalculateSecurityChange(Town town, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<SettlementSecurityModel>)this).BaseModel.CalculateSecurityChange(town, includeDescriptions);
		Clan ownerClan = town.OwnerClan;
		Kingdom val = ((ownerClan != null) ? ownerClan.Kingdom : null);
		if (val != null && val.HasPolicy(NavalPolicies.RaidersSpoils))
		{
			((ExplainedNumber)(ref result)).Add((float)(-LinQuick.CountQ<MobileParty>((List<MobileParty>)(object)((SettlementComponent)town).Settlement.Parties, (Func<MobileParty, bool>)((MobileParty x) => x.IsLordParty))), ((PropertyObject)NavalPolicies.RaidersSpoils).Name, (TextObject)null);
		}
		return result;
	}

	public override float GetNearbyBanditPartyDefeatedSecurityEffect(Town town, float sumOfAttackedPartyStrengths)
	{
		return ((MBGameModel<SettlementSecurityModel>)this).BaseModel.GetNearbyBanditPartyDefeatedSecurityEffect(town, sumOfAttackedPartyStrengths);
	}

	public override float GetLootedNearbyPartySecurityEffect(Town town, float sumOfAttackedPartyStrengths)
	{
		return ((MBGameModel<SettlementSecurityModel>)this).BaseModel.GetLootedNearbyPartySecurityEffect(town, sumOfAttackedPartyStrengths);
	}

	public override void CalculateGoldGainDueToHighSecurity(Town town, ref ExplainedNumber explainedNumber)
	{
		((MBGameModel<SettlementSecurityModel>)this).BaseModel.CalculateGoldGainDueToHighSecurity(town, ref explainedNumber);
	}

	public override void CalculateGoldCutDueToLowSecurity(Town town, ref ExplainedNumber explainedNumber)
	{
		((MBGameModel<SettlementSecurityModel>)this).BaseModel.CalculateGoldCutDueToLowSecurity(town, ref explainedNumber);
	}
}
