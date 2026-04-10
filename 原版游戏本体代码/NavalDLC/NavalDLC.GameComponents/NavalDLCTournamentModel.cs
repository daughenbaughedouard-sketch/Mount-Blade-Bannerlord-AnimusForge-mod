using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.GameComponents;

public class NavalDLCTournamentModel : TournamentModel
{
	public override MBList<ItemObject> GetEliteRewardItems(Town town, int regularRewardMinValue, int regularRewardMaxValue)
	{
		MBList<ItemObject> eliteRewardItems = ((MBGameModel<TournamentModel>)this).BaseModel.GetEliteRewardItems(town, regularRewardMinValue, regularRewardMaxValue);
		string[] array = new string[2] { "head_breaker_2haxe", "world_chopper__1haxe" };
		foreach (string text in array)
		{
			ItemObject val = Game.Current.ObjectManager.GetObject<ItemObject>(text);
			if (val != null)
			{
				((List<ItemObject>)(object)eliteRewardItems).Add(val);
			}
		}
		return eliteRewardItems;
	}

	public override MBList<ItemObject> GetRegularRewardItems(Town town, int regularRewardMinValue, int regularRewardMaxValue)
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.GetRegularRewardItems(town, regularRewardMinValue, regularRewardMaxValue);
	}

	public override TournamentGame CreateTournament(Town town)
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.CreateTournament(town);
	}

	public override int GetInfluenceReward(Hero winner, Town town)
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.GetInfluenceReward(winner, town);
	}

	public override int GetNumLeaderboardVictoriesAtGameStart()
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.GetNumLeaderboardVictoriesAtGameStart();
	}

	public override Equipment GetParticipantArmor(CharacterObject participant)
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.GetParticipantArmor(participant);
	}

	public override int GetRenownReward(Hero winner, Town town)
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.GetRenownReward(winner, town);
	}

	public override (SkillObject skill, int xp) GetSkillXpGainFromTournament(Town town)
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.GetSkillXpGainFromTournament(town);
	}

	public override float GetTournamentEndChance(TournamentGame tournament)
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.GetTournamentEndChance(tournament);
	}

	public override float GetTournamentSimulationScore(CharacterObject character)
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.GetTournamentSimulationScore(character);
	}

	public override float GetTournamentStartChance(Town town)
	{
		return ((MBGameModel<TournamentModel>)this).BaseModel.GetTournamentStartChance(town);
	}
}
