using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCEncounterModel : EncounterModel
{
	public override float NeededMaximumLandDistanceForEncounteringMobileParty => ((MBGameModel<EncounterModel>)this).BaseModel.NeededMaximumLandDistanceForEncounteringMobileParty;

	public override float NeededMaximumNavalDistanceForEncounteringMobileParty => 1.5f;

	public override float MaximumAllowedLandDistanceForEncounteringMobilePartyInArmy => ((MBGameModel<EncounterModel>)this).BaseModel.MaximumAllowedLandDistanceForEncounteringMobilePartyInArmy;

	public override float MaximumAllowedNavalDistanceForEncounteringMobilePartyInArmy => 2.5f;

	public override float NeededMaximumDistanceForEncounteringTown => ((MBGameModel<EncounterModel>)this).BaseModel.NeededMaximumDistanceForEncounteringTown;

	public override float NeededMaximumDistanceForEncounteringBlockade => ((MBGameModel<EncounterModel>)this).BaseModel.NeededMaximumDistanceForEncounteringBlockade;

	public override float NeededMaximumDistanceForEncounteringVillage => ((MBGameModel<EncounterModel>)this).BaseModel.NeededMaximumDistanceForEncounteringVillage;

	public override float GetEncounterJoiningRadius => ((MBGameModel<EncounterModel>)this).BaseModel.GetEncounterJoiningRadius;

	public override float GetSettlementBeingNearFieldBattleRadius => ((MBGameModel<EncounterModel>)this).BaseModel.GetSettlementBeingNearFieldBattleRadius;

	public override float PlayerParleyDistance => ((MBGameModel<EncounterModel>)this).BaseModel.PlayerParleyDistance;

	public override bool CanMainHeroDoParleyWithParty(PartyBase partyBase, out TextObject explanation)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		bool flag = ((MBGameModel<EncounterModel>)this).BaseModel.CanMainHeroDoParleyWithParty(partyBase, ref explanation);
		if (flag)
		{
			if (MobileParty.MainParty.IsCurrentlyAtSea)
			{
				explanation = new TextObject("{=eWxpOYAe}You can't start parley while at sea.", (Dictionary<string, object>)null);
				flag = false;
			}
			else if (MobileParty.MainParty.IsTransitionInProgress)
			{
				explanation = new TextObject("{=boWTBYUF}You can't start parley while embarking.", (Dictionary<string, object>)null);
				flag = false;
			}
		}
		return flag;
	}

	public override MapEventComponent CreateMapEventComponentForEncounter(PartyBase attackerParty, PartyBase defenderParty, BattleTypes battleType)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<EncounterModel>)this).BaseModel.CreateMapEventComponentForEncounter(attackerParty, defenderParty, battleType);
	}

	public override void FindNonAttachedNpcPartiesWhoWillJoinPlayerEncounter(List<MobileParty> partiesToJoinPlayerSide, List<MobileParty> partiesToJoinEnemySide)
	{
		((MBGameModel<EncounterModel>)this).BaseModel.FindNonAttachedNpcPartiesWhoWillJoinPlayerEncounter(partiesToJoinPlayerSide, partiesToJoinEnemySide);
		if (!NavalStorylineData.IsNavalStoryLineActive())
		{
			return;
		}
		for (int num = partiesToJoinPlayerSide.Count - 1; num >= 0; num--)
		{
			if (!partiesToJoinPlayerSide[num].IsNavalStorylineQuestParty())
			{
				partiesToJoinPlayerSide.RemoveAt(num);
			}
		}
		for (int num2 = partiesToJoinEnemySide.Count - 1; num2 >= 0; num2--)
		{
			if (!partiesToJoinEnemySide[num2].IsNavalStorylineQuestParty())
			{
				partiesToJoinEnemySide.RemoveAt(num2);
			}
		}
	}

	public override bool CanPlayerForceBanditsToJoin(out TextObject explanation)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (MobileParty.MainParty.IsCurrentlyAtSea)
		{
			bool perkValue = Hero.MainHero.GetPerkValue(NavalPerks.Mariner.Arr);
			explanation = (perkValue ? null : new TextObject("{=MaetSSa1}You need '{PERK}' perk to make this party join you.", (Dictionary<string, object>)null).SetTextVariable("PERK", ((PropertyObject)NavalPerks.Mariner.Arr).Name));
			return perkValue;
		}
		return ((MBGameModel<EncounterModel>)this).BaseModel.CanPlayerForceBanditsToJoin(ref explanation);
	}

	public override float GetMapEventSideRunAwayChance(MapEventSide mapEventSide)
	{
		return ((MBGameModel<EncounterModel>)this).BaseModel.GetMapEventSideRunAwayChance(mapEventSide);
	}

	public override ExplainedNumber GetBribeChance(MobileParty defenderParty, MobileParty attackerParty)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber bribeChance = ((MBGameModel<EncounterModel>)this).BaseModel.GetBribeChance(defenderParty, attackerParty);
		if (defenderParty.IsBandit && defenderParty.HasNavalNavigationCapability)
		{
			PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.Arr, attackerParty.LeaderHero.CharacterObject, true, ref bribeChance, false);
		}
		return bribeChance;
	}

	public override int GetCharacterSergeantScore(Hero hero)
	{
		return ((MBGameModel<EncounterModel>)this).BaseModel.GetCharacterSergeantScore(hero);
	}

	public override IEnumerable<PartyBase> GetDefenderPartiesOfSettlement(Settlement settlement, BattleTypes mapEventType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<EncounterModel>)this).BaseModel.GetDefenderPartiesOfSettlement(settlement, mapEventType);
	}

	public override Hero GetLeaderOfMapEvent(MapEvent mapEvent, BattleSideEnum side)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<EncounterModel>)this).BaseModel.GetLeaderOfMapEvent(mapEvent, side);
	}

	public override Hero GetLeaderOfSiegeEvent(SiegeEvent siegeEvent, BattleSideEnum side)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<EncounterModel>)this).BaseModel.GetLeaderOfSiegeEvent(siegeEvent, side);
	}

	public override PartyBase GetNextDefenderPartyOfSettlement(Settlement settlement, ref int partyIndex, BattleTypes mapEventType)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<EncounterModel>)this).BaseModel.GetNextDefenderPartyOfSettlement(settlement, ref partyIndex, mapEventType);
	}

	public override float GetSurrenderChance(MobileParty defenderParty, MobileParty attackerParty)
	{
		return ((MBGameModel<EncounterModel>)this).BaseModel.GetSurrenderChance(defenderParty, attackerParty);
	}

	public override bool IsEncounterExemptFromHostileActions(PartyBase side1, PartyBase side2)
	{
		return ((MBGameModel<EncounterModel>)this).BaseModel.IsEncounterExemptFromHostileActions(side1, side2);
	}

	public override bool IsPartyUnderPlayerCommand(PartyBase party)
	{
		if (party.IsMobile && !party.MobileParty.IsMainParty && party.MobileParty.IsCurrentlyUsedByAQuest && NavalStorylineData.IsNavalStoryLineActive() && NavalStorylineData.GetStorylineStage() == NavalStorylineData.NavalStorylineStage.Act2)
		{
			return false;
		}
		return ((MBGameModel<EncounterModel>)this).BaseModel.IsPartyUnderPlayerCommand(party);
	}

	public override MBReadOnlyList<MobileParty> GetPartiesToTeleportOnMapEventFinalize(MapEvent mapEvent)
	{
		MBReadOnlyList<MobileParty> partiesToTeleportOnMapEventFinalize = ((MBGameModel<EncounterModel>)this).BaseModel.GetPartiesToTeleportOnMapEventFinalize(mapEvent);
		MBList<MobileParty> val = new MBList<MobileParty>();
		foreach (MobileParty item in (List<MobileParty>)(object)partiesToTeleportOnMapEventFinalize)
		{
			if (!item.IsCurrentlyAtSea || item.HasNavalNavigationCapability)
			{
				((List<MobileParty>)(object)val).Add(item);
			}
		}
		return (MBReadOnlyList<MobileParty>)(object)val;
	}
}
