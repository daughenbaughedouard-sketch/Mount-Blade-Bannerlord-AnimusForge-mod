using System;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.CampaignBehaviors;

public class NavalFishingCampaignBehaviour : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnHourlyTickParty);
		CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener((object)this, (Action<Settlement>)OnDailyTickSettlement);
	}

	private void OnDailyTickSettlement(Settlement settlement)
	{
		if (settlement.IsVillage && settlement.Village.TradeBound != null)
		{
			ExplainedNumber val = default(ExplainedNumber);
			((ExplainedNumber)(ref val))._002Ector(0f, false, (TextObject)null);
			PerkHelper.AddPerkBonusForTown(NavalPerks.Shipmaster.NightRaider, settlement.Village.TradeBound.Town, ref val);
			if (((ExplainedNumber)(ref val)).RoundedResultNumber > 0)
			{
				ItemObject val2 = MBObjectManager.Instance.GetObject<ItemObject>("fish");
				int roundedResultNumber = ((ExplainedNumber)(ref val)).RoundedResultNumber;
				((SettlementComponent)settlement.Village).Owner.ItemRoster.AddToCounts(val2, roundedResultNumber);
				((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnItemProduced(val2, ((SettlementComponent)settlement.Village).Owner.Settlement, roundedResultNumber);
			}
		}
	}

	private void OnHourlyTickParty(MobileParty party)
	{
		if (party.IsCurrentlyAtSea)
		{
			float num = 0f;
			if (party.HasPerk(NavalPerks.Shipmaster.MasterAngler, false))
			{
				num += NavalPerks.Shipmaster.MasterAngler.PrimaryBonus;
			}
			if (MBRandom.RandomFloat < num)
			{
				ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>("fish");
				party.ItemRoster.AddToCounts(val, 1);
			}
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
