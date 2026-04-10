using System;
using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;

namespace NavalDLC.CampaignBehaviors;

public class NavalNimbleSurgeCampaignBehaviour : CampaignBehaviorBase
{
	private Dictionary<MobileParty, Dictionary<Settlement, CampaignTime>> _lastTimeEntered = new Dictionary<MobileParty, Dictionary<Settlement, CampaignTime>>();

	public override void RegisterEvents()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
	}

	private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
	{
		_lastTimeEntered.Remove(mobileParty);
	}

	public override void SyncData(IDataStore dataStore)
	{
		DoCleanUp();
		dataStore.SyncData<Dictionary<MobileParty, Dictionary<Settlement, CampaignTime>>>("_lastTimeEntered", ref _lastTimeEntered);
	}

	private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (mobileParty == null || !mobileParty.IsCaravan || !mobileParty.HasNavalNavigationCapability || !settlement.IsFortification || settlement.Town.Governor == null || settlement.Town.BuildingsInProgress.Count <= 0)
		{
			return;
		}
		Town town = settlement.Town;
		if (!town.Governor.GetPerkValue(NavalPerks.Shipmaster.FavorableTide))
		{
			return;
		}
		if (_lastTimeEntered.ContainsKey(mobileParty) && _lastTimeEntered[mobileParty].ContainsKey(settlement))
		{
			CampaignTime val = _lastTimeEntered[mobileParty][settlement];
			if (!(((CampaignTime)(ref val)).ElapsedDaysUntilNow > 1f))
			{
				return;
			}
		}
		if (!_lastTimeEntered.ContainsKey(mobileParty))
		{
			_lastTimeEntered[mobileParty] = new Dictionary<Settlement, CampaignTime>();
		}
		_lastTimeEntered[mobileParty][settlement] = CampaignTime.Now;
		Building currentBuilding = town.CurrentBuilding;
		currentBuilding.BuildingProgress += 1f;
		BuildingHelper.CheckIfBuildingIsComplete(town.CurrentBuilding);
	}

	private void DoCleanUp()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<MobileParty, Dictionary<Settlement, CampaignTime>> item in _lastTimeEntered)
		{
			List<Settlement> list = new List<Settlement>();
			foreach (KeyValuePair<Settlement, CampaignTime> item2 in item.Value)
			{
				CampaignTime value = item2.Value;
				if (((CampaignTime)(ref value)).ElapsedDaysUntilNow > 1f)
				{
					list.Add(item2.Key);
				}
			}
			foreach (Settlement item3 in list)
			{
				item.Value.Remove(item3);
			}
		}
		List<MobileParty> list2 = new List<MobileParty>();
		foreach (KeyValuePair<MobileParty, Dictionary<Settlement, CampaignTime>> item4 in _lastTimeEntered)
		{
			if (_lastTimeEntered[item4.Key] == null || _lastTimeEntered[item4.Key].Count == 0)
			{
				list2.Add(item4.Key);
			}
		}
		foreach (MobileParty item5 in list2)
		{
			_lastTimeEntered.Remove(item5);
		}
	}
}
