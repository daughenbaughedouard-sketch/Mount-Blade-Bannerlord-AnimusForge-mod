using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.GameComponents;

public class NavalEncounterMenuModel : EncounterGameMenuModel
{
	public override string GetEncounterMenu(PartyBase attackerParty, PartyBase defenderParty, out bool startBattle, out bool joinBattle)
	{
		PartyBase encounteredPartyBase = MapEventHelper.GetEncounteredPartyBase(attackerParty, defenderParty);
		if (NavalStorylineData.IsNavalStoryLineActive() && encounteredPartyBase.IsMobile && ((MBObjectBase)encounteredPartyBase.MobileParty).StringId == "free_the_sea_hounds_captives_initial_quest_party")
		{
			startBattle = false;
			joinBattle = false;
			return "act_3_quest_5_encounter_menu";
		}
		if (NavalStorylineData.IsNavalStoryLineActive() && defenderParty.IsSettlement && defenderParty.Settlement.IsTown && defenderParty.Settlement.HasPort)
		{
			startBattle = false;
			joinBattle = false;
			return "naval_storyline_virtualport";
		}
		if (NavalStorylineData.IsNavalStoryLineActive() && Settlement.CurrentSettlement == null)
		{
			bool num = attackerParty.IsMobile && attackerParty.MobileParty.IsBandit;
			bool flag = defenderParty.IsMobile && defenderParty.MobileParty.IsBandit;
			if (!num && !flag && (!defenderParty.IsMobile || attackerParty != PartyBase.MainParty || !defenderParty.IsNavalStorylineQuestParty()) && (!attackerParty.IsMobile || defenderParty != PartyBase.MainParty || !attackerParty.IsNavalStorylineQuestParty()))
			{
				startBattle = false;
				joinBattle = false;
				return "naval_storyline_encounter_blocking";
			}
		}
		string encounterMenu = ((MBGameModel<EncounterGameMenuModel>)this).BaseModel.GetEncounterMenu(attackerParty, defenderParty, ref startBattle, ref joinBattle);
		PartyBase party = ((attackerParty == PartyBase.MainParty) ? defenderParty : attackerParty);
		if (NavalStorylineData.IsNavalStoryLineActive() && party.IsNavalStorylineQuestParty())
		{
			switch (encounterMenu)
			{
			case "encounter_meeting":
				return "naval_storyline_encounter_meeting";
			case "encounter":
				return "naval_storyline_encounter";
			case "join_encounter":
				return "naval_storyline_join_encounter";
			}
		}
		return encounterMenu;
	}

	public override string GetGenericStateMenu()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string genericStateMenu = ((MBGameModel<EncounterGameMenuModel>)this).BaseModel.GetGenericStateMenu();
		if (NavalStorylineData.IsNavalStoryLineActive() && genericStateMenu == "encounter")
		{
			MapEvent mapEvent = MobileParty.MainParty.MapEvent;
			if (((IEnumerable<MapEventParty>)mapEvent.PartiesOnSide(mapEvent.GetOtherSide(mapEvent.PlayerSide))).Any((MapEventParty x) => x.Party.IsNavalStorylineQuestParty()))
			{
				return "naval_storyline_encounter";
			}
		}
		return genericStateMenu;
	}

	public override string GetNewPartyJoinMenu(MobileParty newParty)
	{
		return ((MBGameModel<EncounterGameMenuModel>)this).BaseModel.GetNewPartyJoinMenu(newParty);
	}

	public override string GetRaidCompleteMenu()
	{
		return ((MBGameModel<EncounterGameMenuModel>)this).BaseModel.GetRaidCompleteMenu();
	}

	public override bool IsPlunderMenu(string menuId)
	{
		return ((MBGameModel<EncounterGameMenuModel>)this).BaseModel.IsPlunderMenu(menuId);
	}
}
