using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalTradeAgreementModel : TradeAgreementModel
{
	public override bool CanMakeTradeAgreement(Kingdom kingdom, Kingdom other, bool checkOtherSideTradeSupport, out TextObject reason, bool includeReason = false)
	{
		return ((MBGameModel<TradeAgreementModel>)this).BaseModel.CanMakeTradeAgreement(kingdom, other, checkOtherSideTradeSupport, ref reason, includeReason);
	}

	public override int GetInfluenceCostOfProposingTradeAgreement(Clan clan)
	{
		return ((MBGameModel<TradeAgreementModel>)this).BaseModel.GetInfluenceCostOfProposingTradeAgreement(clan);
	}

	public override int GetMaximumTradeAgreementCount(Kingdom kingdom)
	{
		return ((MBGameModel<TradeAgreementModel>)this).BaseModel.GetMaximumTradeAgreementCount(kingdom);
	}

	public override int GetProfitPerCaravanVisit(MobileParty mobileParty)
	{
		if (mobileParty.HasNavalNavigationCapability)
		{
			return 1000;
		}
		return ((MBGameModel<TradeAgreementModel>)this).BaseModel.GetProfitPerCaravanVisit(mobileParty);
	}

	public override float GetScoreOfStartingTradeAgreement(Kingdom kingdom, Kingdom targetKingdom, Clan clan, out TextObject explanation, bool includeExplanation = false)
	{
		return ((MBGameModel<TradeAgreementModel>)this).BaseModel.GetScoreOfStartingTradeAgreement(kingdom, targetKingdom, clan, ref explanation, includeExplanation);
	}

	public override CampaignTime GetTradeAgreementDurationInYears(Kingdom iniatatingKingdom, Kingdom otherKingdom)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<TradeAgreementModel>)this).BaseModel.GetTradeAgreementDurationInYears(iniatatingKingdom, otherKingdom);
	}
}
