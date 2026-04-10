using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.CampaignBehaviors;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCBattleRewardModel : BattleRewardModel
{
	public override int CalculateGoldLossAfterDefeat(Hero partyLeaderHero)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.CalculateGoldLossAfterDefeat(partyLeaderHero);
	}

	public override ExplainedNumber CalculateInfluenceGain(PartyBase winnerParty, float influenceValueOfBattleForWinnerSide, float contributionShareOfWinnerParty, float influenceMultiplierForWinnerSide, bool includeDescriptions)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.CalculateInfluenceGain(winnerParty, influenceValueOfBattleForWinnerSide, contributionShareOfWinnerParty, influenceMultiplierForWinnerSide, includeDescriptions);
	}

	public override float CalculateMoraleChangeOnRoundVictory(PartyBase party, MapEventSide partySide, BattleSideEnum roundWinner)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.CalculateMoraleChangeOnRoundVictory(party, partySide, roundWinner);
	}

	public override ExplainedNumber CalculateMoraleGainVictory(PartyBase winnerParty, float renownValueOfBattleForWinnerSide, float contributionShareOfWinnerParty, bool includeDescriptions)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.CalculateMoraleGainVictory(winnerParty, renownValueOfBattleForWinnerSide, contributionShareOfWinnerParty, includeDescriptions);
	}

	public override int CalculatePlunderedGoldAmountFromDefeatedParty(PartyBase defeatedParty)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.CalculatePlunderedGoldAmountFromDefeatedParty(defeatedParty);
	}

	public override ExplainedNumber CalculateRenownGain(PartyBase winnerParty, float renownValueOfBattleForWinnerSide, float contributionShareOfWinnerParty, float renownMultiplierForWinnerSide, bool includeDescriptions)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.CalculateRenownGain(winnerParty, renownValueOfBattleForWinnerSide, contributionShareOfWinnerParty, renownMultiplierForWinnerSide, includeDescriptions);
	}

	public override float GetAITradePenalty()
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetAITradePenalty();
	}

	public override float GetBannerLootChanceFromDefeatedHero(Hero defeatedHero)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetBannerLootChanceFromDefeatedHero(defeatedHero);
	}

	public override ItemObject GetBannerRewardForWinningMapEvent(MapEvent mapEvent)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetBannerRewardForWinningMapEvent(mapEvent);
	}

	public override float GetExpectedLootedItemValueFromCasualty(Hero winnerPartyLeaderHero, CharacterObject casualtyCharacter)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetExpectedLootedItemValueFromCasualty(winnerPartyLeaderHero, casualtyCharacter);
	}

	public override MBReadOnlyList<KeyValuePair<MapEventParty, float>> GetLootCasualtyChances(MBReadOnlyList<MapEventParty> winnerParties, PartyBase defeatedParty)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetLootCasualtyChances(winnerParties, defeatedParty);
	}

	public override EquipmentElement GetLootedItemFromTroop(CharacterObject character, float targetValue)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetLootedItemFromTroop(character, targetValue);
	}

	public override MBReadOnlyList<KeyValuePair<MapEventParty, float>> GetLootGoldChances(MBReadOnlyList<MapEventParty> winnerParties)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetLootGoldChances(winnerParties);
	}

	public override MBList<KeyValuePair<MapEventParty, float>> GetLootItemChancesForWinnerParties(MBReadOnlyList<MapEventParty> winnerParties, PartyBase defeatedParty)
	{
		MBList<KeyValuePair<MapEventParty, float>> lootItemChancesForWinnerParties = ((MBGameModel<BattleRewardModel>)this).BaseModel.GetLootItemChancesForWinnerParties(winnerParties, defeatedParty);
		if (defeatedParty.IsMobile && (defeatedParty.MobileParty.IsCaravan || defeatedParty.MobileParty.IsVillager))
		{
			ExplainedNumber val = default(ExplainedNumber);
			for (int i = 0; i < ((List<KeyValuePair<MapEventParty, float>>)(object)lootItemChancesForWinnerParties).Count; i++)
			{
				PartyBase party = ((List<KeyValuePair<MapEventParty, float>>)(object)lootItemChancesForWinnerParties)[i].Key.Party;
				((ExplainedNumber)(ref val))._002Ector(((List<KeyValuePair<MapEventParty, float>>)(object)lootItemChancesForWinnerParties)[i].Value, false, (TextObject)null);
				if (PartyBaseHelper.HasFeat(party, NavalCulturalFeats.NordHostileActionBonusFeat))
				{
					((ExplainedNumber)(ref val)).AddFactor(NavalCulturalFeats.NordHostileActionBonusFeat.EffectBonus, (TextObject)null);
				}
				if (defeatedParty.MobileParty.IsCaravan && ((PartyComponent)defeatedParty.MobileParty.CaravanPartyComponent).CanHaveNavalNavigationCapability)
				{
					PerkHelper.AddPerkBonusForParty(NavalPerks.Mariner.PiratesProwess, party.MobileParty, false, ref val, false);
				}
				((List<KeyValuePair<MapEventParty, float>>)(object)lootItemChancesForWinnerParties)[i] = new KeyValuePair<MapEventParty, float>(((List<KeyValuePair<MapEventParty, float>>)(object)lootItemChancesForWinnerParties)[i].Key, ((ExplainedNumber)(ref val)).ResultNumber);
			}
		}
		return lootItemChancesForWinnerParties;
	}

	public override void GetCaptureMemberChancesForWinnerParties(MapEvent endedMapEvent, MBReadOnlyList<MapEventParty> winnerParties, out MBList<KeyValuePair<MapEventParty, float>> woundedMemberChances, out MBList<KeyValuePair<MapEventParty, float>> healthyMemberChances)
	{
		woundedMemberChances = new MBList<KeyValuePair<MapEventParty, float>>();
		healthyMemberChances = new MBList<KeyValuePair<MapEventParty, float>>();
		((MBGameModel<BattleRewardModel>)this).BaseModel.GetCaptureMemberChancesForWinnerParties(endedMapEvent, winnerParties, ref woundedMemberChances, ref healthyMemberChances);
		float num = 0f;
		ExplainedNumber val = default(ExplainedNumber);
		for (int i = 0; i < ((List<KeyValuePair<MapEventParty, float>>)(object)woundedMemberChances).Count; i++)
		{
			KeyValuePair<MapEventParty, float> keyValuePair = ((List<KeyValuePair<MapEventParty, float>>)(object)woundedMemberChances)[i];
			MapEventParty key = keyValuePair.Key;
			((ExplainedNumber)(ref val))._002Ector(keyValuePair.Value, false, (TextObject)null);
			if (key.Party.IsMobile)
			{
				PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.RiverRaider, key.Party.MobileParty, false, ref val, false);
			}
			((List<KeyValuePair<MapEventParty, float>>)(object)woundedMemberChances)[i] = new KeyValuePair<MapEventParty, float>(key, ((ExplainedNumber)(ref val)).ResultNumber);
			num += ((List<KeyValuePair<MapEventParty, float>>)(object)woundedMemberChances)[i].Value;
		}
		if (num > 0f)
		{
			for (int j = 0; j < ((List<KeyValuePair<MapEventParty, float>>)(object)woundedMemberChances).Count; j++)
			{
				((List<KeyValuePair<MapEventParty, float>>)(object)woundedMemberChances)[j] = new KeyValuePair<MapEventParty, float>(((List<KeyValuePair<MapEventParty, float>>)(object)woundedMemberChances)[j].Key, ((List<KeyValuePair<MapEventParty, float>>)(object)woundedMemberChances)[j].Value / num);
			}
		}
		num = 0f;
		ExplainedNumber val2 = default(ExplainedNumber);
		for (int k = 0; k < ((List<KeyValuePair<MapEventParty, float>>)(object)healthyMemberChances).Count; k++)
		{
			KeyValuePair<MapEventParty, float> keyValuePair2 = ((List<KeyValuePair<MapEventParty, float>>)(object)healthyMemberChances)[k];
			MapEventParty key2 = keyValuePair2.Key;
			((ExplainedNumber)(ref val2))._002Ector(keyValuePair2.Value, false, (TextObject)null);
			if (key2.Party.IsMobile)
			{
				PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.RiverRaider, key2.Party.MobileParty, false, ref val2, false);
			}
			((List<KeyValuePair<MapEventParty, float>>)(object)healthyMemberChances)[k] = new KeyValuePair<MapEventParty, float>(key2, ((ExplainedNumber)(ref val2)).ResultNumber);
			num += ((List<KeyValuePair<MapEventParty, float>>)(object)woundedMemberChances)[k].Value;
		}
		if (num > 0f)
		{
			for (int l = 0; l < ((List<KeyValuePair<MapEventParty, float>>)(object)healthyMemberChances).Count; l++)
			{
				((List<KeyValuePair<MapEventParty, float>>)(object)healthyMemberChances)[l] = new KeyValuePair<MapEventParty, float>(((List<KeyValuePair<MapEventParty, float>>)(object)healthyMemberChances)[l].Key, ((List<KeyValuePair<MapEventParty, float>>)(object)healthyMemberChances)[l].Value / num);
			}
		}
	}

	public override MBReadOnlyList<KeyValuePair<MapEventParty, float>> GetLootPrisonerChances(MBReadOnlyList<MapEventParty> winnerParties, TroopRosterElement prisonerElement)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetLootPrisonerChances(winnerParties, prisonerElement);
	}

	public override float CalculateShipDamageAfterDefeat(Ship ship)
	{
		return ship.MaxHitPoints * MBRandom.RandomFloatRanged(0.2f, 0.5f);
	}

	public override MBReadOnlyList<KeyValuePair<Ship, MapEventParty>> DistributeDefeatedPartyShipsAmongWinners(MapEvent mapEvent, MBReadOnlyList<Ship> shipsToLoot, MBReadOnlyList<MapEventParty> winnerParties)
	{
		if (mapEvent.IsPlayerMapEvent && NavalStorylineData.IsNavalStoryLineActive())
		{
			return new MBReadOnlyList<KeyValuePair<Ship, MapEventParty>>();
		}
		Dictionary<Ship, MapEventParty> dictionary = new Dictionary<Ship, MapEventParty>();
		MBList<Ship> val = new MBList<Ship>();
		foreach (Ship item in (List<Ship>)(object)shipsToLoot)
		{
			dictionary.Add(item, null);
			if (MBRandom.RandomFloat < 0.5f)
			{
				if (item.CanEquipFigurehead)
				{
					item.ChangeFigurehead((Figurehead)null);
				}
				((List<Ship>)(object)val).Add(item);
			}
		}
		IEnumerable<MapEventParty> enumerable = LinQuick.WhereQ<MapEventParty>((List<MapEventParty>)(object)winnerParties, (Func<MapEventParty, bool>)((MapEventParty x) => x.Party.IsMobile && x.Party.MobileParty.PartyComponent.CanHaveNavalNavigationCapability && !x.Party.MobileParty.IsPatrolParty));
		if (LinQuick.AnyQ<MapEventParty>(enumerable))
		{
			float winnerPartiesTotalScoreForLootingShips = LinQuick.SumQ<MapEventParty>(enumerable, (Func<MapEventParty, float>)((MapEventParty x) => PartyLootShipScore(x)));
			List<MapEventParty> list = LinQuick.OrderByQ<MapEventParty, float>(enumerable, (Func<MapEventParty, float>)((MapEventParty x) => (float)((List<Ship>)(object)x.Party.Ships).Count + (1f - PartyLootShipScore(x) / winnerPartiesTotalScoreForLootingShips))).ToList();
			List<MapEventParty> list2 = new List<MapEventParty>();
			if (((List<Ship>)(object)val).Count < list.Count)
			{
				list2 = list.GetRange(((List<Ship>)(object)val).Count, list.Count - ((List<Ship>)(object)val).Count).ToList();
				list.RemoveRange(((List<Ship>)(object)val).Count, list.Count - ((List<Ship>)(object)val).Count);
			}
			list = list.OrderByDescending((MapEventParty x) => PartyLootShipScore(x)).ToList();
			if (LinQuick.AnyQ<MapEventParty>(list2))
			{
				list.AddRange(list2.OrderByDescending((MapEventParty x) => PartyLootShipScore(x)).ToList());
			}
			bool flag = true;
			while (flag && ((List<Ship>)(object)val).Count > 0)
			{
				flag = false;
				foreach (MapEventParty item2 in list)
				{
					MBList<Ship> val2 = Extensions.ToMBList<Ship>((List<Ship>)(object)item2.Ships);
					foreach (KeyValuePair<Ship, MapEventParty> item3 in dictionary)
					{
						if (item3.Value == item2)
						{
							((List<Ship>)(object)val2).Add(item3.Key);
						}
					}
					Ship shipToLootForWinnerParty = GetShipToLootForWinnerParty(item2, val2, val);
					if (shipToLootForWinnerParty != null)
					{
						flag = true;
						dictionary[shipToLootForWinnerParty] = item2;
						((List<Ship>)(object)val).Remove(shipToLootForWinnerParty);
					}
					if (((List<Ship>)(object)val).Count == 0)
					{
						break;
					}
				}
				if (((List<Ship>)(object)val).Count > 0)
				{
					list = list.OrderByDescending((MapEventParty x) => PartyLootShipScore(x)).ToList();
				}
			}
		}
		if (((List<Ship>)(object)val).Count > 0 && LinQuick.AnyQ<MapEventParty>((List<MapEventParty>)(object)winnerParties, (Func<MapEventParty, bool>)((MapEventParty x) => x.Party == PartyBase.MainParty)))
		{
			Extensions.Shuffle<Ship>((IList<Ship>)val);
			int num = LinQuick.CountQ<KeyValuePair<Ship, MapEventParty>>((IEnumerable<KeyValuePair<Ship, MapEventParty>>)dictionary, (Func<KeyValuePair<Ship, MapEventParty>, bool>)delegate(KeyValuePair<Ship, MapEventParty> x)
			{
				MapEventParty value = x.Value;
				return ((value != null) ? value.Party : null) == PartyBase.MainParty;
			});
			if (((List<Ship>)(object)val).Count + num > 25)
			{
				val = Extensions.ToMBList<Ship>(((IEnumerable<Ship>)val).Take(25 - num));
			}
			MapEventParty val3 = ((List<MapEventParty>)(object)winnerParties).Find((Predicate<MapEventParty>)((MapEventParty x) => x.Party == PartyBase.MainParty));
			int num2 = 0;
			foreach (MapEventParty item4 in (List<MapEventParty>)(object)winnerParties)
			{
				int contributionToBattle = item4.ContributionToBattle;
				num2 += contributionToBattle;
			}
			foreach (Ship item5 in (List<Ship>)(object)val)
			{
				if (MBRandom.RandomInt(num2) < val3.ContributionToBattle)
				{
					dictionary[item5] = val3;
					continue;
				}
				break;
			}
		}
		return (MBReadOnlyList<KeyValuePair<Ship, MapEventParty>>)(object)Extensions.ToMBList<KeyValuePair<Ship, MapEventParty>>((IEnumerable<KeyValuePair<Ship, MapEventParty>>)dictionary);
	}

	private float PartyLootShipScore(MapEventParty party)
	{
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector((float)party.ContributionToBattle, false, (TextObject)null);
		((ExplainedNumber)(ref val)).Add((float)party.Party.MemberRoster.TotalManCount, (TextObject)null, (TextObject)null);
		if (party.Party.LeaderHero != null)
		{
			Hero leaderHero = party.Party.LeaderHero;
			if (leaderHero.IsKingdomLeader)
			{
				((ExplainedNumber)(ref val)).Add(50000f, (TextObject)null, (TextObject)null);
			}
			else if (leaderHero.IsClanLeader)
			{
				((ExplainedNumber)(ref val)).Add(20000f, (TextObject)null, (TextObject)null);
			}
			if (leaderHero.Clan != null)
			{
				float num = MBMath.Map((float)leaderHero.Clan.Tier, (float)Campaign.Current.Models.ClanTierModel.MinClanTier, (float)Campaign.Current.Models.ClanTierModel.MaxClanTier, 5000f, 10000f);
				((ExplainedNumber)(ref val)).Add(num, (TextObject)null, (TextObject)null);
			}
		}
		MobileParty mobileParty = party.Party.MobileParty;
		if (((mobileParty != null) ? mobileParty.ActualClan : null) != null)
		{
			float num2 = MBMath.Map((float)party.Party.MobileParty.ActualClan.Tier, (float)Campaign.Current.Models.ClanTierModel.MinClanTier, (float)Campaign.Current.Models.ClanTierModel.MaxClanTier, 5000f, 10000f);
			((ExplainedNumber)(ref val)).Add(num2, (TextObject)null, (TextObject)null);
		}
		if (party.Party.IsMobile)
		{
			PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.GildedPurse, party.Party.MobileParty, true, ref val, false);
		}
		return ((ExplainedNumber)(ref val)).RoundedResultNumber;
	}

	private Ship GetShipToLootForWinnerParty(MapEventParty winnerParty, MBList<Ship> partyShipsToConsider, MBList<Ship> lootableShips)
	{
		float num = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(winnerParty.Party.MobileParty, (MBReadOnlyList<Ship>)(object)partyShipsToConsider);
		Ship result = null;
		foreach (Ship item in (List<Ship>)(object)lootableShips)
		{
			if (NavalDLCManager.Instance.GameModels.ShipDistributionModel.CanPartyTakeShip(winnerParty.Party, item))
			{
				((List<Ship>)(object)partyShipsToConsider).Add(item);
				float scoreForPartyShipComposition = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(winnerParty.Party.MobileParty, (MBReadOnlyList<Ship>)(object)partyShipsToConsider);
				((List<Ship>)(object)partyShipsToConsider).Remove(item);
				if (scoreForPartyShipComposition > num)
				{
					num = scoreForPartyShipComposition;
					result = item;
				}
			}
		}
		return result;
	}

	public override float GetMainPartyMemberScatterChance()
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetMainPartyMemberScatterChance();
	}

	public override int GetPlayerGainedRelationAmount(MapEvent mapEvent, Hero hero)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetPlayerGainedRelationAmount(mapEvent, hero);
	}

	public override float GetShipSiegeEngineHitMoraleEffect(Ship ship, SiegeEngineType siegeEngineType)
	{
		return 0f;
	}

	public override float GetSunkenShipMoraleEffect(PartyBase shipOwner, Ship ship)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected I4, but got Unknown
		float result = -2f;
		ShipType type = ship.ShipHull.Type;
		switch ((int)type)
		{
		case 0:
			result = -1f;
			break;
		case 1:
			result = -2f;
			break;
		case 2:
			result = -3f;
			break;
		default:
			Debug.FailedAssert("Ship type not handled", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\GameComponents\\NavalDLCBattleRewardModel.cs", "GetSunkenShipMoraleEffect", 437);
			break;
		}
		return result;
	}

	public override MBReadOnlyList<MapEventParty> GetWinnerPartiesThatCanPlunderGoldFromShips(MBReadOnlyList<MapEventParty> winnerParties)
	{
		MBList<MapEventParty> val = new MBList<MapEventParty>();
		foreach (MapEventParty item in (List<MapEventParty>)(object)winnerParties)
		{
			if (item.Party != PartyBase.MainParty && item.ContributionToBattle > 0 && item.Party.IsMobile && !item.Party.MobileParty.IsBandit && !item.Party.MobileParty.IsCaravan)
			{
				((List<MapEventParty>)(object)val).Add(item);
			}
		}
		return (MBReadOnlyList<MapEventParty>)(object)val;
	}

	public override Figurehead GetFigureheadLoot(MBReadOnlyList<MapEventParty> defeatedParties, PartyBase defeatedSideLeaderParty)
	{
		Figurehead result = null;
		if (CanUnlockFigurehead())
		{
			IEnumerable<Hero> heroes = LinQuick.SelectQ<MapEventParty, Hero>(LinQuick.WhereQ<MapEventParty>((List<MapEventParty>)(object)defeatedParties, (Func<MapEventParty, bool>)((MapEventParty x) => x.Party.LeaderHero != null)), (Func<MapEventParty, Hero>)((MapEventParty x) => x.Party.LeaderHero));
			float figureheadDropChanceForHeroes = GetFigureheadDropChanceForHeroes(heroes);
			if (MBRandom.RandomFloat <= figureheadDropChanceForHeroes)
			{
				List<Figurehead> unlockedFigureheadsByMainHero = Campaign.Current.UnlockedFigureheadsByMainHero;
				List<(Figurehead, float)> list = new List<(Figurehead, float)>();
				foreach (MapEventParty item in (List<MapEventParty>)(object)defeatedParties)
				{
					foreach (Ship item2 in (List<Ship>)(object)item.Ships)
					{
						if (item2.Figurehead == null || unlockedFigureheadsByMainHero.Contains(item2.Figurehead))
						{
							continue;
						}
						if (item.Party == defeatedSideLeaderParty)
						{
							MobileParty mobileParty = defeatedSideLeaderParty.MobileParty;
							object obj;
							if (mobileParty == null)
							{
								obj = null;
							}
							else
							{
								Army army = mobileParty.Army;
								obj = ((army != null) ? army.LeaderParty : null);
							}
							if (obj == defeatedSideLeaderParty.MobileParty)
							{
								list.Add((item2.Figurehead, 0.2f));
								continue;
							}
						}
						list.Add((item2.Figurehead, 0.1f));
					}
				}
				return MBRandom.ChooseWeighted<Figurehead>((IReadOnlyList<ValueTuple<Figurehead, float>>)list);
			}
		}
		return result;
	}

	private bool CanUnlockFigurehead()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		CampaignTime lastFigureheadLootTime = Campaign.Current.GetCampaignBehavior<NavalDLCFigureheadCampaignBehavior>().LastFigureheadLootTime;
		return ((CampaignTime)(ref lastFigureheadLootTime)).ElapsedDaysUntilNow >= 8f;
	}

	private float GetFigureheadDropChanceForHeroes(IEnumerable<Hero> heroes)
	{
		float num = 0f;
		foreach (Hero hero in heroes)
		{
			IFaction mapFaction = hero.MapFaction;
			if (mapFaction != null && mapFaction.IsKingdomFaction && hero.MapFaction.Leader == hero)
			{
				num = 0.6f;
				break;
			}
			Clan clan = hero.Clan;
			if (((clan != null) ? clan.Leader : null) == hero && num < 0.5f)
			{
				num = 0.5f;
			}
			else if (hero.Clan != null && num < 0.4f)
			{
				num = 0.4f;
			}
		}
		return num;
	}

	public override bool CanTroopBeTakenPrisoner(CharacterObject troop)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.CanTroopBeTakenPrisoner(troop);
	}
}
