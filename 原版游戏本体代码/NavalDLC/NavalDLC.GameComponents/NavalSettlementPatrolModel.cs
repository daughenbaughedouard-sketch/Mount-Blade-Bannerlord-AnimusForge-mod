using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalSettlementPatrolModel : SettlementPatrolModel
{
	public override bool CanSettlementHavePatrolParties(Settlement settlement, bool naval)
	{
		if (naval)
		{
			if (settlement.OwnerClan != null && !settlement.OwnerClan.IsRebelClan && settlement.IsTown && settlement.HasPort && settlement.OwnerClan.Kingdom != null)
			{
				return HasCoastalEdict(settlement.OwnerClan.Kingdom);
			}
			return false;
		}
		return ((MBGameModel<SettlementPatrolModel>)this).BaseModel.CanSettlementHavePatrolParties(settlement, naval);
	}

	public override PartyTemplateObject GetPartyTemplateForPatrolParty(Settlement settlement, bool naval)
	{
		if (naval)
		{
			return settlement.OwnerClan.Culture.SettlementPatrolPartyTemplateNaval;
		}
		return ((MBGameModel<SettlementPatrolModel>)this).BaseModel.GetPartyTemplateForPatrolParty(settlement, naval);
	}

	public override CampaignTime GetPatrolPartySpawnDuration(Settlement settlement, bool naval)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (naval)
		{
			CampaignTime now = CampaignTime.Now;
			return CampaignTime.Days(RandomOwnerExtensions.RandomFloatWithSeed((IRandomOwner)(object)settlement, (uint)((CampaignTime)(ref now)).ElapsedMillisecondsUntilNow, 5f, 7f));
		}
		return ((MBGameModel<SettlementPatrolModel>)this).BaseModel.GetPatrolPartySpawnDuration(settlement, naval);
	}

	private bool HasCoastalEdict(Kingdom kingdom)
	{
		return kingdom.HasPolicy(NavalPolicies.CoastalGuardEdict);
	}
}
