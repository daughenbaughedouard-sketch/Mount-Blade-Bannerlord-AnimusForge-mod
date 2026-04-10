using System.Collections.Generic;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace StoryMode.GameComponents;

public class StoryModeBattleRewardModel : BattleRewardModel
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
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (TutorialPhase.Instance != null && !TutorialPhase.Instance.IsCompleted && winnerParty == PartyBase.MainParty)
		{
			return default(ExplainedNumber);
		}
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.CalculateRenownGain(winnerParty, renownValueOfBattleForWinnerSide, contributionShareOfWinnerParty, renownMultiplierForWinnerSide, includeDescriptions);
	}

	public override float CalculateShipDamageAfterDefeat(Ship ship)
	{
		return 0f;
	}

	public override MBReadOnlyList<KeyValuePair<Ship, MapEventParty>> DistributeDefeatedPartyShipsAmongWinners(MapEvent mapEvent, MBReadOnlyList<Ship> shipsToLoot, MBReadOnlyList<MapEventParty> winnerParties)
	{
		return new MBReadOnlyList<KeyValuePair<Ship, MapEventParty>>();
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

	public override Figurehead GetFigureheadLoot(MBReadOnlyList<MapEventParty> defeatedParties, PartyBase defeatedSideLeaderParty)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetFigureheadLoot(defeatedParties, defeatedSideLeaderParty);
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
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetLootItemChancesForWinnerParties(winnerParties, defeatedParty);
	}

	public override void GetCaptureMemberChancesForWinnerParties(MapEvent endedMapEvent, MBReadOnlyList<MapEventParty> winnerParties, out MBList<KeyValuePair<MapEventParty, float>> woundedMemberChances, out MBList<KeyValuePair<MapEventParty, float>> healthyMemberChances)
	{
		((MBGameModel<BattleRewardModel>)this).BaseModel.GetCaptureMemberChancesForWinnerParties(endedMapEvent, winnerParties, ref woundedMemberChances, ref healthyMemberChances);
	}

	public override MBReadOnlyList<KeyValuePair<MapEventParty, float>> GetLootPrisonerChances(MBReadOnlyList<MapEventParty> winnerParties, TroopRosterElement prisonerElement)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (StoryModeData.IsConspiracyTroop(prisonerElement.Character))
		{
			MBList<KeyValuePair<MapEventParty, float>> val = new MBList<KeyValuePair<MapEventParty, float>>();
			{
				foreach (MapEventParty item in (List<MapEventParty>)(object)winnerParties)
				{
					((List<KeyValuePair<MapEventParty, float>>)(object)val).Add(new KeyValuePair<MapEventParty, float>(item, 0f));
				}
				return (MBReadOnlyList<KeyValuePair<MapEventParty, float>>)(object)val;
			}
		}
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetLootPrisonerChances(winnerParties, prisonerElement);
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
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetShipSiegeEngineHitMoraleEffect(ship, siegeEngineType);
	}

	public override float GetSunkenShipMoraleEffect(PartyBase shipOwner, Ship ship)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetSunkenShipMoraleEffect(shipOwner, ship);
	}

	public override MBReadOnlyList<MapEventParty> GetWinnerPartiesThatCanPlunderGoldFromShips(MBReadOnlyList<MapEventParty> winnerParties)
	{
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.GetWinnerPartiesThatCanPlunderGoldFromShips(winnerParties);
	}

	public override bool CanTroopBeTakenPrisoner(CharacterObject troop)
	{
		if (StoryModeData.IsConspiracyTroop(troop))
		{
			return false;
		}
		return ((MBGameModel<BattleRewardModel>)this).BaseModel.CanTroopBeTakenPrisoner(troop);
	}
}
