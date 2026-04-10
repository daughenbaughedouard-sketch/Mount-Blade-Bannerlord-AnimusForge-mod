using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace NavalDLC.CampaignBehaviors;

public class ShipUpgradeCampaignBehavior : CampaignBehaviorBase
{
	private const float CaravanShipUpgradeChance = 0.4f;

	public override void RegisterEvents()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)DailyTickPartyEvent);
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter, int>)OnNewGameCreatedPartialFollowUp);
	}

	private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		if (mobileParty == null || !mobileParty.IsCaravan || !settlement.HasPort || !settlement.IsTown || !CanPartyUpgradeShips(mobileParty) || settlement.Town.GetShipyard().CurrentLevel <= 0)
		{
			return;
		}
		List<ShipUpgradePiece> availableShipUpgradePieces = settlement.Town.GetAvailableShipUpgradePieces();
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			if (!(MBRandom.RandomFloat < 0.4f))
			{
				continue;
			}
			KeyValuePair<string, ShipSlot> randomSlot = Extensions.GetRandomElementInefficiently<KeyValuePair<string, ShipSlot>>((IEnumerable<KeyValuePair<string, ShipSlot>>)item.ShipHull.AvailableSlots);
			ShipUpgradePiece randomElementWithPredicate = Extensions.GetRandomElementWithPredicate<ShipUpgradePiece>((IReadOnlyList<ShipUpgradePiece>)availableShipUpgradePieces, (Func<ShipUpgradePiece, bool>)((ShipUpgradePiece x) => x.DoesPieceMatchSlot(randomSlot.Value)));
			if (randomElementWithPredicate != null)
			{
				int shipUpgradePieceCost = Campaign.Current.Models.ShipCostModel.GetShipUpgradePieceCost(item, randomElementWithPredicate, item.Owner);
				if ((float)mobileParty.PartyTradeGold * 0.2f > (float)shipUpgradePieceCost)
				{
					UpgradeShip(item, randomSlot.Key, randomElementWithPredicate);
					GiveGoldAction.ApplyForPartyToSettlement(mobileParty.Party, settlement, shipUpgradePieceCost, false);
				}
			}
		}
	}

	private float GetChanceToUpgradeShipForLord(Hero hero)
	{
		float num = (float)(hero.Clan.Tier + 1 - Campaign.Current.Models.ClanTierModel.MinClanTier) / (float)(1 + Campaign.Current.Models.ClanTierModel.MaxClanTier - Campaign.Current.Models.ClanTierModel.MinClanTier);
		float num2 = (hero.IsKingdomLeader ? 0.6f : (hero.IsClanLeader ? 0.4f : 0.2f));
		return num * num2;
	}

	private void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int index)
	{
		if (index % 2 != 0)
		{
			return;
		}
		foreach (MobileParty item in (List<MobileParty>)(object)MobileParty.All)
		{
			DailyTickPartyEvent(item);
		}
	}

	private void DailyTickPartyEvent(MobileParty party)
	{
		if (party.LeaderHero == null || party.IsCurrentlyAtSea || !CanPartyUpgradeShips(party))
		{
			return;
		}
		float chanceToUpgradeShipForLord = GetChanceToUpgradeShipForLord(party.LeaderHero);
		foreach (Ship item in (List<Ship>)(object)party.Ships)
		{
			if (MBRandom.RandomFloat < chanceToUpgradeShipForLord)
			{
				KeyValuePair<string, ShipSlot> randomElementInefficiently = Extensions.GetRandomElementInefficiently<KeyValuePair<string, ShipSlot>>((IEnumerable<KeyValuePair<string, ShipSlot>>)item.ShipHull.AvailableSlots);
				ShipUpgradePiece pieceAtSlot = item.GetPieceAtSlot(randomElementInefficiently.Key);
				int upgradePieceLevelToLook = ((pieceAtSlot == null) ? 1 : (pieceAtSlot.RequiredPortLevel + 1));
				ShipUpgradePiece randomElementWithPredicate = Extensions.GetRandomElementWithPredicate<ShipUpgradePiece>(randomElementInefficiently.Value.MatchingPieces, (Func<ShipUpgradePiece, bool>)((ShipUpgradePiece x) => !x.NotMerchandise && x.RequiredPortLevel == upgradePieceLevelToLook));
				if (randomElementWithPredicate != null)
				{
					UpgradeShip(item, randomElementInefficiently.Key, randomElementWithPredicate);
				}
			}
		}
	}

	private void UpgradeShip(Ship ship, string slotId, ShipUpgradePiece upgradePiece)
	{
		ShipUpgradePiece pieceAtSlot = ship.GetPieceAtSlot(slotId);
		_ = ship.ShipHull.AvailableSlots[slotId];
		if (pieceAtSlot == null || pieceAtSlot.RequiredPortLevel != 3)
		{
			ship.EquipUpgradePiece(slotId, upgradePiece);
		}
		PartyBase owner = ship.Owner;
		if (owner != null)
		{
			MobileParty mobileParty = owner.MobileParty;
			if (mobileParty != null)
			{
				mobileParty.SetNavalVisualAsDirty();
			}
		}
	}

	private bool CanPartyUpgradeShips(MobileParty party)
	{
		if (party.ActualClan != Clan.PlayerClan && ((List<Ship>)(object)party.Ships).Count > 0 && !party.IsCurrentlyUsedByAQuest && party.IsActive && party.MapEvent == null && party.SiegeEvent == null && !party.IsInRaftState)
		{
			return !party.IsDisbanding;
		}
		return false;
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
