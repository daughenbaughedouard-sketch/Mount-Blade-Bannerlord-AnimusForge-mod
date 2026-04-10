using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.CampaignBehaviors;

public class ShipTradeCampaignBehavior : CampaignBehaviorBase
{
	private const float ShipSellingChance = 0.1f;

	private const float ShipTransferringChance = 0.75f;

	private const float ClanGoldRatioToBuyShip = 0.2f;

	public static bool DebugNavalLordParties;

	public static bool DebugLordParties;

	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter, int>)OnNewGameCreatedPartialFollowUp);
		CampaignEvents.DailyTickClanEvent.AddNonSerializedListener((object)this, (Action<Clan>)DailyTickClan);
		CampaignEvents.OnShipOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Ship, PartyBase, ShipOwnerChangeDetail>)OnShipOwnerChanged);
		CampaignEvents.OnShipRepairedEvent.AddNonSerializedListener((object)this, (Action<Ship, Settlement>)OnShipRepaired);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)Tick);
	}

	private void OnGameLoadFinished()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!MBSaveLoad.IsUpdatingGameVersion)
		{
			return;
		}
		ApplicationVersion lastLoadedGameVersion = MBSaveLoad.LastLoadedGameVersion;
		if (!((ApplicationVersion)(ref lastLoadedGameVersion)).IsOlderThan(ApplicationVersion.FromString("v1.3.9.103828", 0)))
		{
			return;
		}
		foreach (MobileParty item in (List<MobileParty>)(object)MobileParty.AllLordParties)
		{
			if (item != MobileParty.MainParty && item.MapEvent == null && item.SiegeEvent == null && item.ActualClan != null && item.LeaderHero != null && ((List<Ship>)(object)item.Ships).Count > 0 && item.IsActive && !item.IsCurrentlyUsedByAQuest && item.LeaderHero.IsActive)
			{
				int num = 0;
				Ship shipToSell;
				while (TryGetShipToSell(item, out shipToSell))
				{
					num += (int)Campaign.Current.Models.ShipCostModel.GetShipTradeValue(shipToSell, item.Party, (PartyBase)null);
					DestroyShipAction.Apply(shipToSell);
				}
				if (num > 0)
				{
					GiveGoldAction.ApplyBetweenCharacters((Hero)null, item.ActualClan.Leader, num, false);
				}
			}
		}
		foreach (MobileParty item2 in (List<MobileParty>)(object)MobileParty.AllBanditParties)
		{
			if (item2.MapEvent == null && item2.ActualClan != null && ((List<Ship>)(object)item2.Ships).Count > 0 && item2.IsActive && !item2.IsCurrentlyUsedByAQuest && ((List<Ship>)(object)item2.Ships).Count > Campaign.Current.Models.PartyShipLimitModel.GetIdealShipNumber(item2))
			{
				for (int num2 = ((List<Ship>)(object)item2.Ships).Count - 1; num2 > Campaign.Current.Models.PartyShipLimitModel.GetIdealShipNumber(item2) - 1; num2--)
				{
					DestroyShipAction.Apply(((List<Ship>)(object)item2.Ships)[num2]);
				}
			}
		}
	}

	private void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int index)
	{
		foreach (Clan item in (List<Clan>)(object)Clan.All)
		{
			DailyTickClan(item);
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void DailyTickClan(Clan clan)
	{
		if (!clan.IsBanditFaction && !clan.IsEliminated && clan != Clan.PlayerClan)
		{
			ConsiderPurchasingShip(clan);
			ConsiderSwappingClanLeaderShips(clan);
			ConsiderSwappingShipsBetweenClanParties(clan);
			if (GetTotalNumberOfWarShipsInClan(clan) > NavalDLCManager.Instance.GameModels.ClanShipOwnershipModel.GetIdealShipNumberForClan(clan))
			{
				ConsiderSellingShips(clan);
			}
		}
	}

	private void ConsiderPurchasingShip(Clan clan)
	{
		if (!(MBRandom.RandomFloat < GetClanShipPurchaseChance(clan)))
		{
			return;
		}
		MobileParty partyToGiveShipTo = GetPartyToGiveShipTo(clan);
		if (partyToGiveShipTo != null)
		{
			Town townToBuyShipFrom = GetTownToBuyShipFrom(clan);
			if (townToBuyShipFrom != null)
			{
				TryPurchasingShipFromTown(partyToGiveShipTo, townToBuyShipFrom);
			}
		}
	}

	private float GetClanShipPurchaseChance(Clan clan)
	{
		return 0.5f;
	}

	private void TryPurchasingShipFromTown(MobileParty mobileParty, Town town)
	{
		Ship val = null;
		MBList<Ship> val2 = Extensions.ToMBList<Ship>((List<Ship>)(object)mobileParty.Ships);
		float num = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(mobileParty, (MBReadOnlyList<Ship>)(object)val2);
		foreach (Ship item in (List<Ship>)(object)town.AvailableShips)
		{
			if (NavalDLCManager.Instance.GameModels.ShipDistributionModel.CanPartyTakeShip(mobileParty.Party, item) && Campaign.Current.Models.ShipCostModel.GetShipTradeValue(item, ((SettlementComponent)town).Settlement.Party, mobileParty.Party) < (float)mobileParty.ActualClan.Gold * 0.2f)
			{
				((List<Ship>)(object)val2).Add(item);
				float scoreForPartyShipComposition = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(mobileParty, (MBReadOnlyList<Ship>)(object)val2);
				((List<Ship>)(object)val2).Remove(item);
				if (scoreForPartyShipComposition > num)
				{
					num = scoreForPartyShipComposition;
					val = item;
				}
			}
		}
		Ship val3 = null;
		foreach (Ship item2 in (List<Ship>)(object)town.AvailableShips)
		{
			if (!NavalDLCManager.Instance.GameModels.ShipDistributionModel.CanPartyTakeShip(mobileParty.Party, item2))
			{
				continue;
			}
			for (int i = 0; i < ((List<Ship>)(object)val2).Count; i++)
			{
				if (Campaign.Current.Models.ShipCostModel.GetShipTradeValue(item2, ((SettlementComponent)town).Settlement.Party, mobileParty.Party) < (float)mobileParty.ActualClan.Gold * 0.2f)
				{
					Ship val4 = ((List<Ship>)(object)val2)[i];
					((List<Ship>)(object)val2)[i] = item2;
					float scoreForPartyShipComposition2 = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(mobileParty, (MBReadOnlyList<Ship>)(object)val2);
					if (scoreForPartyShipComposition2 > num)
					{
						num = scoreForPartyShipComposition2;
						val = item2;
						val3 = val4;
					}
					((List<Ship>)(object)val2)[i] = val4;
				}
			}
		}
		if (val != null)
		{
			if (val3 != null)
			{
				ChangeShipOwnerAction.ApplyByTrade(((SettlementComponent)town).Settlement.Party, val3);
			}
			ChangeShipOwnerAction.ApplyByTrade(mobileParty.Party, val);
		}
	}

	private MobileParty GetPartyToGiveShipTo(Clan clan)
	{
		MobileParty result = null;
		float num = float.MaxValue;
		foreach (WarPartyComponent item in (List<WarPartyComponent>)(object)clan.WarPartyComponents)
		{
			if (CanPartyTradeShip(((PartyComponent)item).MobileParty))
			{
				float scoreForPartyShipComposition = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(((PartyComponent)item).MobileParty, ((PartyComponent)item).MobileParty.Ships);
				if (scoreForPartyShipComposition < num)
				{
					num = scoreForPartyShipComposition;
					result = ((PartyComponent)item).MobileParty;
				}
			}
		}
		return result;
	}

	private Town GetTownToBuyShipFrom(Clan clan)
	{
		Town val = null;
		if (((List<Town>)(object)clan.MapFaction.Fiefs).Count > 0)
		{
			val = Extensions.GetRandomElementWithPredicate<Town>(clan.MapFaction.Fiefs, (Func<Town, bool>)((Town x) => CanClanBuyShipFromTown(clan, x)));
		}
		if (val == null && MBRandom.RandomFloat < 0.2f)
		{
			val = Extensions.GetRandomElementWithPredicate<Town>(Town.AllTowns, (Func<Town, bool>)((Town x) => CanClanBuyShipFromTown(clan, x) && !((SettlementComponent)x).MapFaction.IsAtWarWith((IFaction)(object)clan)));
		}
		return val;
	}

	private bool CanClanBuyShipFromTown(Clan clan, Town town)
	{
		if (!town.IsUnderSiege)
		{
			return ((List<Ship>)(object)town.AvailableShips).Count > 0;
		}
		return false;
	}

	private void ConsiderSwappingClanLeaderShips(Clan clan)
	{
		if (!(MBRandom.RandomFloat < 0.75f) || ((List<WarPartyComponent>)(object)clan.WarPartyComponents).Count <= 2 || !CanPartyTradeShip(clan.Leader.PartyBelongedTo))
		{
			return;
		}
		MobileParty mobileParty = ((PartyComponent)Extensions.GetRandomElementWithPredicate<WarPartyComponent>(clan.WarPartyComponents, (Func<WarPartyComponent, bool>)((WarPartyComponent x) => ((PartyComponent)x).MobileParty != clan.Leader.PartyBelongedTo))).MobileParty;
		if (mobileParty == null || !CanPartyTradeShip(mobileParty))
		{
			return;
		}
		MBList<Ship> val = Extensions.ToMBList<Ship>((List<Ship>)(object)clan.Leader.PartyBelongedTo.Ships);
		float num = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(clan.Leader.PartyBelongedTo, (MBReadOnlyList<Ship>)(object)val);
		Tuple<Ship, Ship> tuple = new Tuple<Ship, Ship>(null, null);
		for (int num2 = ((List<Ship>)(object)val).Count - 1; num2 >= 0; num2--)
		{
			Ship val2 = ((List<Ship>)(object)val)[num2];
			if (val2.IsTradeable && NavalDLCManager.Instance.GameModels.ShipDistributionModel.CanPartyTakeShip(mobileParty.Party, val2))
			{
				MBList<Ship> val3 = Extensions.ToMBList<Ship>((List<Ship>)(object)mobileParty.Ships);
				if (((IEnumerable<Ship>)val3).Any())
				{
					((List<Ship>)(object)val).RemoveAt(num2);
					for (int num3 = 0; num3 < ((List<Ship>)(object)val3).Count; num3++)
					{
						Ship val4 = ((List<Ship>)(object)val3)[num3];
						if (val4.IsTradeable && NavalDLCManager.Instance.GameModels.ShipDistributionModel.CanPartyTakeShip(clan.Leader.PartyBelongedTo.Party, val4))
						{
							((List<Ship>)(object)val).Add(val4);
							float scoreForPartyShipComposition = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(clan.Leader.PartyBelongedTo, (MBReadOnlyList<Ship>)(object)val);
							if (scoreForPartyShipComposition > num)
							{
								num = scoreForPartyShipComposition;
								tuple = new Tuple<Ship, Ship>(val2, val4);
							}
							((List<Ship>)(object)val).Remove(val4);
						}
					}
					((List<Ship>)(object)val).Add(val2);
				}
			}
		}
		if (tuple.Item1 != null)
		{
			ChangeShipOwnerAction.ApplyByTransferring(tuple.Item2.Owner, tuple.Item1);
			ChangeShipOwnerAction.ApplyByTransferring(clan.Leader.PartyBelongedTo.Party, tuple.Item2);
		}
	}

	private void ConsiderSwappingShipsBetweenClanParties(Clan clan)
	{
		if (!(MBRandom.RandomFloat < 0.75f) || ((List<WarPartyComponent>)(object)clan.WarPartyComponents).Count <= 2)
		{
			return;
		}
		WarPartyComponent randomElementWithPredicate = Extensions.GetRandomElementWithPredicate<WarPartyComponent>(clan.WarPartyComponents, (Func<WarPartyComponent, bool>)((WarPartyComponent x) => CanPartyTradeShip(((PartyComponent)x).MobileParty)));
		MobileParty party1 = ((randomElementWithPredicate != null) ? ((PartyComponent)randomElementWithPredicate).MobileParty : null);
		WarPartyComponent randomElementWithPredicate2 = Extensions.GetRandomElementWithPredicate<WarPartyComponent>(clan.WarPartyComponents, (Func<WarPartyComponent, bool>)((WarPartyComponent x) => ((PartyComponent)x).MobileParty != party1 && CanPartyTradeShip(((PartyComponent)x).MobileParty)));
		MobileParty val = ((randomElementWithPredicate2 != null) ? ((PartyComponent)randomElementWithPredicate2).MobileParty : null);
		if (party1 == null || val == null || party1.IsDisbanding || val.IsDisbanding)
		{
			return;
		}
		MBList<Ship> val2 = Extensions.ToMBList<Ship>((List<Ship>)(object)party1.Ships);
		MBList<Ship> val3 = Extensions.ToMBList<Ship>((List<Ship>)(object)val.Ships);
		float scoreForPartyShipComposition = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(party1, (MBReadOnlyList<Ship>)(object)val2);
		float scoreForPartyShipComposition2 = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(val, (MBReadOnlyList<Ship>)(object)val3);
		float num = scoreForPartyShipComposition + scoreForPartyShipComposition2;
		Tuple<Ship, Ship> tuple = new Tuple<Ship, Ship>(null, null);
		for (int num2 = ((List<Ship>)(object)val2).Count - 1; num2 >= 0; num2--)
		{
			Ship val4 = ((List<Ship>)(object)val2)[num2];
			if (val4.IsTradeable && NavalDLCManager.Instance.GameModels.ShipDistributionModel.CanPartyTakeShip(val.Party, val4))
			{
				((List<Ship>)(object)val2).RemoveAt(num2);
				float scoreForPartyShipComposition3 = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(party1, (MBReadOnlyList<Ship>)(object)val2);
				((List<Ship>)(object)val3).Add(val4);
				float scoreForPartyShipComposition4 = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(val, (MBReadOnlyList<Ship>)(object)val3);
				((List<Ship>)(object)val3).Remove(val4);
				if (scoreForPartyShipComposition3 + scoreForPartyShipComposition4 > num && ((List<Ship>)(object)party1.Ships).Count > 1 && (clan.Leader.PartyBelongedTo != party1 || scoreForPartyShipComposition3 > scoreForPartyShipComposition) && (clan.Leader.PartyBelongedTo != val || scoreForPartyShipComposition4 > scoreForPartyShipComposition2))
				{
					num = scoreForPartyShipComposition3 + scoreForPartyShipComposition4;
					tuple = new Tuple<Ship, Ship>(val4, null);
				}
				for (int num3 = ((List<Ship>)(object)val3).Count - 1; num3 >= 0; num3--)
				{
					Ship val5 = ((List<Ship>)(object)val3)[num3];
					if (val5.IsTradeable && NavalDLCManager.Instance.GameModels.ShipDistributionModel.CanPartyTakeShip(party1.Party, val5))
					{
						((List<Ship>)(object)val2).Add(val5);
						((List<Ship>)(object)val3).Add(val4);
						((List<Ship>)(object)val3).RemoveAt(num3);
						scoreForPartyShipComposition3 = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(party1, (MBReadOnlyList<Ship>)(object)val2);
						scoreForPartyShipComposition4 = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(val, (MBReadOnlyList<Ship>)(object)val3);
						if (scoreForPartyShipComposition3 + scoreForPartyShipComposition4 > num && (clan.Leader.PartyBelongedTo != party1 || scoreForPartyShipComposition3 > scoreForPartyShipComposition) && (clan.Leader.PartyBelongedTo != val || scoreForPartyShipComposition4 > scoreForPartyShipComposition2))
						{
							num = scoreForPartyShipComposition3 + scoreForPartyShipComposition4;
							tuple = new Tuple<Ship, Ship>(val4, val5);
						}
						((List<Ship>)(object)val3).Remove(val4);
						((List<Ship>)(object)val3).Add(val5);
					}
				}
				((List<Ship>)(object)val2).Add(val4);
			}
		}
		if (tuple.Item1 != null)
		{
			if (tuple.Item2 != null)
			{
				ChangeShipOwnerAction.ApplyByTransferring(party1.Party, tuple.Item2);
			}
			ChangeShipOwnerAction.ApplyByTransferring(val.Party, tuple.Item1);
		}
	}

	private void ConsiderSellingShips(Clan clan)
	{
		if (!(MBRandom.RandomFloat < 0.1f) || !((IEnumerable<WarPartyComponent>)clan.WarPartyComponents).Any())
		{
			return;
		}
		MobileParty mobileParty = ((PartyComponent)Extensions.GetRandomElement<WarPartyComponent>(clan.WarPartyComponents)).MobileParty;
		if (!mobileParty.IsDisbanding && CanPartyTradeShip(mobileParty) && TryGetShipToSell(mobileParty, out var shipToSell))
		{
			Town townToSellShip = GetTownToSellShip(clan);
			if (townToSellShip != null)
			{
				ChangeShipOwnerAction.ApplyByTrade(((SettlementComponent)townToSellShip).Settlement.Party, shipToSell);
			}
		}
	}

	private bool TryGetShipToSell(MobileParty mobileParty, out Ship shipToSell)
	{
		shipToSell = null;
		MBList<Ship> val = Extensions.ToMBList<Ship>((List<Ship>)(object)mobileParty.Ships);
		float num = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(mobileParty, (MBReadOnlyList<Ship>)(object)val);
		for (int num2 = ((List<Ship>)(object)val).Count - 1; num2 >= 0; num2--)
		{
			Ship val2 = ((List<Ship>)(object)val)[num2];
			if (val2.IsTradeable)
			{
				((List<Ship>)(object)val).RemoveAt(num2);
				float scoreForPartyShipComposition = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(mobileParty, (MBReadOnlyList<Ship>)(object)val);
				if (scoreForPartyShipComposition > num)
				{
					num = scoreForPartyShipComposition;
					shipToSell = val2;
				}
				((List<Ship>)(object)val).Add(val2);
			}
		}
		return shipToSell != null;
	}

	private Town GetTownToSellShip(Clan clan)
	{
		return Extensions.GetRandomElementWithPredicate<Town>(clan.MapFaction.Fiefs, (Func<Town, bool>)((Town x) => ((SettlementComponent)x).IsTown && x.GetShipyard() != null && x.GetShipyard().CurrentLevel > 0));
	}

	private int GetTotalNumberOfWarShipsInClan(Clan clan)
	{
		int num = 0;
		for (int i = 0; i < ((List<WarPartyComponent>)(object)clan.WarPartyComponents).Count; i++)
		{
			num += ((List<Ship>)(object)((PartyComponent)((List<WarPartyComponent>)(object)clan.WarPartyComponents)[i]).MobileParty.Ships).Count;
		}
		return num;
	}

	private bool CanPartyTradeShip(MobileParty party)
	{
		if (party != null && party.MapEvent == null && party.SiegeEvent == null && !party.IsCurrentlyAtSea && party.LeaderHero != null)
		{
			return party.IsActive;
		}
		return false;
	}

	private void OnShipOwnerChanged(Ship ship, PartyBase oldOwner, ShipOwnerChangeDetail details)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)details == 0)
		{
			Hero val = null;
			if (oldOwner.IsSettlement)
			{
				val = oldOwner.Settlement.Town.Governor;
			}
			else if (ship.Owner.IsSettlement)
			{
				val = ship.Owner.Settlement.Town.Governor;
			}
			if (val != null && (val != Hero.MainHero || ship.Owner.LeaderHero != Hero.MainHero))
			{
				ExplainedNumber val2 = default(ExplainedNumber);
				((ExplainedNumber)(ref val2))._002Ector(0f, false, (TextObject)null);
				PerkHelper.AddPerkBonusForTown(NavalPerks.Boatswain.MerchantPrince, val.CurrentSettlement.Town, ref val2);
				GiveGoldAction.ApplyBetweenCharacters((Hero)null, val, ((ExplainedNumber)(ref val2)).RoundedResultNumber, false);
			}
		}
	}

	private void OnShipRepaired(Ship ship, Settlement repairPort)
	{
		if (repairPort != null && repairPort.IsTown)
		{
			Hero governor = repairPort.Town.Governor;
			if (governor != null && (governor != Hero.MainHero || ship.Owner.LeaderHero != Hero.MainHero))
			{
				ExplainedNumber val = default(ExplainedNumber);
				((ExplainedNumber)(ref val))._002Ector(0f, false, (TextObject)null);
				PerkHelper.AddPerkBonusForTown(NavalPerks.Boatswain.MasterShipwright, repairPort.Town, ref val);
				GiveGoldAction.ApplyBetweenCharacters((Hero)null, governor, ((ExplainedNumber)(ref val)).RoundedResultNumber, false);
			}
		}
	}

	public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		if (mobileParty == null || !mobileParty.IsCaravan || !mobileParty.HasNavalNavigationCapability || !settlement.IsTown || settlement.Town.Governor == null)
		{
			return;
		}
		if (settlement.Town.Governor.GetPerkValue(NavalPerks.Boatswain.Salvage))
		{
			Town town = settlement.Town;
			town.TradeTaxAccumulated += MathF.Round(NavalPerks.Boatswain.Salvage.SecondaryBonus);
		}
		if (!settlement.Town.Governor.GetPerkValue(NavalPerks.Boatswain.ShipwrightsHand))
		{
			return;
		}
		CharacterObject basicTroop = settlement.MapFaction.BasicTroop;
		int characterWage = Campaign.Current.Models.PartyWageModel.GetCharacterWage(basicTroop);
		if (settlement.GarrisonWagePaymentLimit > characterWage + 5)
		{
			MobileParty garrisonParty = ((Fief)settlement.Town).GarrisonParty;
			if (garrisonParty == null)
			{
				settlement.AddGarrisonParty();
				garrisonParty = ((Fief)settlement.Town).GarrisonParty;
			}
			int num = Math.Min(garrisonParty.GetAvailableWageBudget() / characterWage, MathF.Round(NavalPerks.Boatswain.ShipwrightsHand.SecondaryBonus));
			if (num > 0)
			{
				garrisonParty.MemberRoster.AddToCounts(basicTroop, num, false, 0, 0, true, -1);
			}
		}
	}

	private void Tick(float dt)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugNavalLordParties && !DebugLordParties)
		{
			return;
		}
		foreach (MobileParty item in (List<MobileParty>)(object)MobileParty.AllLordParties)
		{
			if ((!DebugLordParties && (!item.IsCurrentlyAtSea || !DebugNavalLordParties)) || (item.Army != null && item.Army.LeaderParty != item) || item.CurrentSettlement != null || item.IsInRaftState || item == MobileParty.MainParty)
			{
				continue;
			}
			CampaignVec2 position = item.Position;
			Vec3 val = ((CampaignVec2)(ref position)).AsVec3() + Vec3.Up * 3.75f;
			val.x -= 1f;
			if (item.Army != null)
			{
				_ = $"Army Ship Count: {((List<Ship>)(object)item.Ships).Count + ((IEnumerable<MobileParty>)item.AttachedParties).Sum((MobileParty x) => ((List<Ship>)(object)x.Ships).Count)}";
			}
			else
			{
				_ = $"Ship Count: {((List<Ship>)(object)item.Ships).Count}";
			}
		}
		int num = 0;
		foreach (Kingdom item2 in (List<Kingdom>)(object)Kingdom.All)
		{
			((IEnumerable<WarPartyComponent>)item2.WarPartyComponents).Count((WarPartyComponent x) => ((PartyComponent)x).MobileParty.IsCurrentlyAtSea && !((PartyComponent)x).MobileParty.IsInRaftState);
			((IEnumerable<WarPartyComponent>)item2.WarPartyComponents).Count((WarPartyComponent x) => !((PartyComponent)x).MobileParty.IsCurrentlyAtSea);
			num++;
		}
	}
}
