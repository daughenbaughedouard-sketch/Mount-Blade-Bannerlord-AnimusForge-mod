using Helpers;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace StoryMode.GameComponents;

public class StoryModeEncounterGameMenuModel : EncounterGameMenuModel
{
	public override string GetEncounterMenu(PartyBase attackerParty, PartyBase defenderParty, out bool startBattle, out bool joinBattle)
	{
		Settlement settlement = MapEventHelper.GetEncounteredPartyBase(attackerParty, defenderParty).Settlement;
		string result;
		if (settlement != null && settlement.SettlementComponent is TrainingField)
		{
			result = "training_field_menu";
			startBattle = false;
			joinBattle = false;
		}
		else if (StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted)
		{
			result = "storymode_game_menu_blocker";
			startBattle = false;
			joinBattle = false;
		}
		else
		{
			result = ((MBGameModel<EncounterGameMenuModel>)this).BaseModel.GetEncounterMenu(attackerParty, defenderParty, ref startBattle, ref joinBattle);
		}
		return result;
	}

	public override string GetGenericStateMenu()
	{
		return ((MBGameModel<EncounterGameMenuModel>)this).BaseModel.GetGenericStateMenu();
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
