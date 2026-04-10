using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.CampaignBehaviors;

public class ShipRepairCampaignBehavior : CampaignBehaviorBase
{
	private List<IFaction> _factionsThatDoNotHavePort;

	public override void RegisterEvents()
	{
		CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)AfterSessionLaunched);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)SettlementOwnerChanged);
		CampaignEvents.AfterSettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnAfterSettlementEnter);
		CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)DailyTickParty);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
		CampaignEvents.OnShipDestroyedEvent.AddNonSerializedListener((object)this, (Action<PartyBase, Ship, ShipDestroyDetail>)OnShipDestroyed);
	}

	private void OnShipDestroyed(PartyBase party, Ship ship, ShipDestroyDetail detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)detail != 1 || !party.IsMobile || !party.MobileParty.HasPerk(NavalPerks.Boatswain.ShipwrightsHand, false))
		{
			return;
		}
		float num = ship.HitPoints * 0.5f;
		foreach (Ship item in (List<Ship>)(object)party.Ships)
		{
			if (num <= 0f)
			{
				break;
			}
			float num2 = item.MaxHitPoints - item.HitPoints;
			float num3 = MathF.Min(num, num2);
			item.HitPoints += num3;
			num -= num3;
		}
	}

	private void AfterSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		_factionsThatDoNotHavePort = new List<IFaction>();
		foreach (Clan item in (List<Clan>)(object)Clan.All)
		{
			if (item.IsBanditFaction || _factionsThatDoNotHavePort.Contains(item.MapFaction))
			{
				continue;
			}
			bool flag = false;
			foreach (Settlement item2 in (List<Settlement>)(object)item.MapFaction.Settlements)
			{
				if (item2.HasPort)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_factionsThatDoNotHavePort.Add(item.MapFaction);
			}
		}
	}

	private void SettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
	{
		if (!settlement.HasPort)
		{
			return;
		}
		if (_factionsThatDoNotHavePort.Contains(newOwner.MapFaction))
		{
			_factionsThatDoNotHavePort.Remove(newOwner.MapFaction);
		}
		bool flag = false;
		foreach (Settlement item in (List<Settlement>)(object)oldOwner.MapFaction.Settlements)
		{
			if (item.HasPort)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			_factionsThatDoNotHavePort.Add(oldOwner.MapFaction);
		}
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
	{
		if (oldKingdom != null)
		{
			bool flag = false;
			foreach (Settlement item in (List<Settlement>)(object)oldKingdom.Settlements)
			{
				if (item.HasPort)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				_factionsThatDoNotHavePort.Remove((IFaction)(object)oldKingdom);
			}
		}
		else if (newKingdom != null)
		{
			_factionsThatDoNotHavePort.Remove((IFaction)(object)clan);
		}
		if (newKingdom != null)
		{
			bool flag2 = false;
			foreach (Settlement item2 in (List<Settlement>)(object)newKingdom.Settlements)
			{
				if (item2.HasPort)
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				_factionsThatDoNotHavePort.Remove((IFaction)(object)newKingdom);
			}
			return;
		}
		bool flag3 = false;
		foreach (Settlement item3 in (List<Settlement>)(object)clan.Settlements)
		{
			if (item3.HasPort)
			{
				flag3 = true;
				break;
			}
		}
		if (!flag3)
		{
			_factionsThatDoNotHavePort.Add((IFaction)(object)clan);
		}
	}

	private void DailyTickParty(MobileParty mobileParty)
	{
		if ((mobileParty.IsBandit || _factionsThatDoNotHavePort.Contains(mobileParty.MapFaction)) && !mobileParty.IsMainParty && ((IEnumerable<Ship>)mobileParty.Ships).Any() && mobileParty.IsCurrentlyAtSea && !mobileParty.IsInRaftState && mobileParty.MapEvent == null && MBRandom.RandomFloat < 0.1f)
		{
			if (mobileParty.IsBandit)
			{
				RepairBanditPartyShips(mobileParty);
			}
			else
			{
				RepairPortlessFactionPartyShips(mobileParty);
			}
		}
	}

	private void OnAfterSettlementEnter(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		if (mobileParty == null || mobileParty.IsMainParty || !settlement.HasPort || !settlement.IsFortification)
		{
			return;
		}
		if (mobileParty.IsCaravan)
		{
			RepairCaravanPartyShips(mobileParty);
			return;
		}
		Hero leaderHero = mobileParty.LeaderHero;
		if (leaderHero != null && leaderHero.IsMinorFactionHero)
		{
			RepairMinorFactionLordPartyShips(mobileParty);
		}
		else if (mobileParty.IsLordParty)
		{
			RepairLordPartyShips(mobileParty, settlement);
		}
	}

	private void RepairPortlessFactionPartyShips(MobileParty mobileParty)
	{
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			if (item.HitPoints < item.MaxHitPoints)
			{
				RepairShipAction.ApplyForFree(item);
			}
		}
	}

	private void RepairCaravanPartyShips(MobileParty mobileParty)
	{
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			if (item.HitPoints < item.MaxHitPoints && (float)mobileParty.PartyTradeGold > Campaign.Current.Models.ShipCostModel.GetShipRepairCost(item, (PartyBase)null))
			{
				RepairShipAction.ApplyForFree(item);
			}
		}
	}

	private void RepairBanditPartyShips(MobileParty mobileParty)
	{
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			if (item.HitPoints < item.MaxHitPoints)
			{
				RepairShipAction.ApplyForBanditShip(item);
			}
		}
	}

	private void RepairMinorFactionLordPartyShips(MobileParty mobileParty)
	{
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			if (item.HitPoints < item.MaxHitPoints)
			{
				RepairShipAction.ApplyForFree(item);
			}
		}
	}

	private void RepairLordPartyShips(MobileParty mobileParty, Settlement settlement)
	{
		if (mobileParty.LeaderHero == null)
		{
			return;
		}
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			if (item.HitPoints < item.MaxHitPoints && (float)mobileParty.PartyTradeGold > Campaign.Current.Models.ShipCostModel.GetShipRepairCost(item, mobileParty.Party))
			{
				RepairShipAction.Apply(item, settlement);
			}
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
