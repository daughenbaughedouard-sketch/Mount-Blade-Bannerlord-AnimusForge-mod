using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCPartyImpairmentModel : PartyImpairmentModel
{
	public override ExplainedNumber GetDisorganizedStateDuration(MobileParty party)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber disorganizedStateDuration = ((MBGameModel<PartyImpairmentModel>)this).BaseModel.GetDisorganizedStateDuration(party);
		if (party.IsCurrentlyAtSea)
		{
			PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.Windborne, party, false, ref disorganizedStateDuration, false);
		}
		return disorganizedStateDuration;
	}

	public override float GetVulnerabilityStateDuration(PartyBase party)
	{
		return ((MBGameModel<PartyImpairmentModel>)this).BaseModel.GetVulnerabilityStateDuration(party);
	}

	public override float GetSiegeExpectedVulnerabilityTime()
	{
		return ((MBGameModel<PartyImpairmentModel>)this).BaseModel.GetSiegeExpectedVulnerabilityTime();
	}

	public override bool CanGetDisorganized(PartyBase partyBase)
	{
		return ((MBGameModel<PartyImpairmentModel>)this).BaseModel.CanGetDisorganized(partyBase);
	}
}
