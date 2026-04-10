using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCPartyMoraleModel : PartyMoraleModel
{
	public override float HighMoraleValue => ((MBGameModel<PartyMoraleModel>)this).BaseModel.HighMoraleValue;

	public override int GetDailyStarvationMoralePenalty(PartyBase party)
	{
		return ((MBGameModel<PartyMoraleModel>)this).BaseModel.GetDailyStarvationMoralePenalty(party);
	}

	public override int GetDailyNoWageMoralePenalty(MobileParty party)
	{
		return ((MBGameModel<PartyMoraleModel>)this).BaseModel.GetDailyNoWageMoralePenalty(party);
	}

	public override float GetStandardBaseMorale(PartyBase party)
	{
		return ((MBGameModel<PartyMoraleModel>)this).BaseModel.GetStandardBaseMorale(party);
	}

	public override float GetVictoryMoraleChange(PartyBase party)
	{
		return ((MBGameModel<PartyMoraleModel>)this).BaseModel.GetVictoryMoraleChange(party);
	}

	public override float GetDefeatMoraleChange(PartyBase party)
	{
		return ((MBGameModel<PartyMoraleModel>)this).BaseModel.GetDefeatMoraleChange(party);
	}

	public override ExplainedNumber GetEffectivePartyMorale(MobileParty party, bool includeDescription = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber effectivePartyMorale = ((MBGameModel<PartyMoraleModel>)this).BaseModel.GetEffectivePartyMorale(party, includeDescription);
		if (party.Anchor != null && party.CurrentSettlement != null && party.Anchor.IsAtSettlement(party.CurrentSettlement))
		{
			PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.EfficientCaptain, party, false, ref effectivePartyMorale, false);
		}
		return effectivePartyMorale;
	}
}
