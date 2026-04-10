using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Core;

namespace SandBox.ViewModelCollection.GameOver;

public class GameOverStatsProvider
{
	private readonly IStatisticsCampaignBehavior _statSource;

	public GameOverStatsProvider()
	{
		_statSource = Campaign.Current.GetCampaignBehavior<IStatisticsCampaignBehavior>();
	}

	public IEnumerable<StatCategory> GetGameOverStats()
	{
		yield return new StatCategory("General", GetGeneralStats(_statSource));
		yield return new StatCategory("Battle", GetBattleStats(_statSource));
		yield return new StatCategory("Finance", GetFinanceStats(_statSource));
		yield return new StatCategory("Crafting", GetCraftingStats(_statSource));
		yield return new StatCategory("Companion", GetCompanionStats(_statSource));
	}

	private IEnumerable<StatItem> GetGeneralStats(IStatisticsCampaignBehavior source)
	{
		CampaignTime totalTimePlayed = source.GetTotalTimePlayed();
		int num = (int)((CampaignTime)(ref totalTimePlayed)).ToYears;
		totalTimePlayed = source.GetTotalTimePlayed();
		int num2 = (int)((CampaignTime)(ref totalTimePlayed)).ToSeasons % CampaignTime.SeasonsInYear;
		totalTimePlayed = source.GetTotalTimePlayed();
		int num3 = (int)((CampaignTime)(ref totalTimePlayed)).ToDays % CampaignTime.DaysInSeason;
		GameTexts.SetVariable("YEAR_IS_PLURAL", (num != 1) ? 1 : 0);
		GameTexts.SetVariable("YEAR", num);
		GameTexts.SetVariable("SEASON_IS_PLURAL", (num2 != 1) ? 1 : 0);
		GameTexts.SetVariable("SEASON", num2);
		GameTexts.SetVariable("DAY_IS_PLURAL", (num3 != 1) ? 1 : 0);
		GameTexts.SetVariable("DAY", num3);
		GameTexts.SetVariable("STR1", GameTexts.FindText("str_YEAR_years", (string)null));
		GameTexts.SetVariable("STR2", GameTexts.FindText("str_SEASON_seasons", (string)null));
		string text = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		GameTexts.SetVariable("STR1", text);
		GameTexts.SetVariable("STR2", GameTexts.FindText("str_DAY_days", (string)null));
		text = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		yield return new StatItem("CampaignPlayTime", text);
		string text2 = $"{TimeSpan.FromSeconds(source.GetTotalTimePlayedInSeconds()).TotalHours:0.##}";
		GameTexts.SetVariable("PLURAL_HOURS", 1);
		GameTexts.SetVariable("HOUR", text2);
		yield return new StatItem("CampaignRealPlayTime", ((object)GameTexts.FindText("str_hours", (string)null)).ToString());
		yield return new StatItem("ChildrenBorn", source.GetNumberOfChildrenBorn().ToString());
		yield return new StatItem("InfluenceEarned", source.GetTotalInfluenceEarned().ToString(), StatItem.StatType.Influence);
		yield return new StatItem("IssuesSolved", source.GetNumberOfIssuesSolved().ToString(), StatItem.StatType.Issue);
		yield return new StatItem("TournamentsWon", source.GetNumberOfTournamentWins().ToString(), StatItem.StatType.Tournament);
		yield return new StatItem("HighestLeaderboardRank", source.GetHighestTournamentRank().ToString());
		yield return new StatItem("PrisonersRecruited", source.GetNumberOfPrisonersRecruited().ToString());
		yield return new StatItem("TroopsRecruited", source.GetNumberOfTroopsRecruited().ToString());
		yield return new StatItem("ClansDefected", source.GetNumberOfClansDefected().ToString());
		yield return new StatItem("TotalCrimeGained", source.GetTotalCrimeRatingGained().ToString(), StatItem.StatType.Crime);
	}

	private IEnumerable<StatItem> GetBattleStats(IStatisticsCampaignBehavior source)
	{
		int numberOfBattlesWon = source.GetNumberOfBattlesWon();
		int numberOfBattlesLost = source.GetNumberOfBattlesLost();
		int num = numberOfBattlesWon + numberOfBattlesLost;
		GameTexts.SetVariable("BATTLES_WON", numberOfBattlesWon);
		GameTexts.SetVariable("BATTLES_LOST", numberOfBattlesLost);
		GameTexts.SetVariable("ALL_BATTLES", num);
		yield return new StatItem("BattlesWonLostAll", ((object)GameTexts.FindText("str_battles_won_lost", (string)null)).ToString());
		yield return new StatItem("BiggestBattleWonAsLeader", source.GetLargestBattleWonAsLeader().ToString());
		yield return new StatItem("BiggestArmyByPlayer", source.GetLargestArmyFormedByPlayer().ToString());
		yield return new StatItem("EnemyClansDestroyed", source.GetNumberOfEnemyClansDestroyed().ToString());
		yield return new StatItem("HeroesKilledInBattle", source.GetNumberOfHeroesKilledInBattle().ToString(), StatItem.StatType.Kill);
		yield return new StatItem("TroopsEliminatedByPlayer", source.GetNumberOfTroopsKnockedOrKilledByPlayer().ToString(), StatItem.StatType.Kill);
		yield return new StatItem("TroopsEliminatedByParty", source.GetNumberOfTroopsKnockedOrKilledAsParty().ToString(), StatItem.StatType.Kill);
		yield return new StatItem("HeroPrisonersTaken", source.GetNumberOfHeroPrisonersTaken().ToString());
		yield return new StatItem("TroopPrisonersTaken", source.GetNumberOfTroopPrisonersTaken().ToString());
		yield return new StatItem("CapturedTowns", source.GetNumberOfTownsCaptured().ToString());
		yield return new StatItem("CapturedCastles", source.GetNumberOfCastlesCaptured().ToString());
		yield return new StatItem("ClearedHideouts", source.GetNumberOfHideoutsCleared().ToString());
		yield return new StatItem("RaidedVillages", source.GetNumberOfVillagesRaided().ToString());
		CampaignTime timeSpentAsPrisoner = source.GetTimeSpentAsPrisoner();
		double toDays = ((CampaignTime)(ref timeSpentAsPrisoner)).ToDays;
		string text = $"{toDays:0.##}";
		GameTexts.SetVariable("DAY_IS_PLURAL", 1);
		GameTexts.SetVariable("DAY", text);
		yield return new StatItem("DaysSpentAsPrisoner", ((object)GameTexts.FindText("str_DAY_days", (string)null)).ToString());
	}

	private IEnumerable<StatItem> GetFinanceStats(IStatisticsCampaignBehavior source)
	{
		yield return new StatItem("TotalDenarsEarned", source.GetTotalDenarsEarned().ToString("0.##"), StatItem.StatType.Gold);
		yield return new StatItem("DenarsFromCaravans", source.GetDenarsEarnedFromCaravans().ToString("0.##"), StatItem.StatType.Gold);
		yield return new StatItem("DenarsFromWorkshops", source.GetDenarsEarnedFromWorkshops().ToString("0.##"), StatItem.StatType.Gold);
		yield return new StatItem("DenarsFromRansoms", source.GetDenarsEarnedFromRansoms().ToString("0.##"), StatItem.StatType.Gold);
		yield return new StatItem("DenarsFromTaxes", source.GetDenarsEarnedFromTaxes().ToString("0.##"), StatItem.StatType.Gold);
		yield return new StatItem("TributeCollected", source.GetDenarsEarnedFromTributes().ToString("0.##"), StatItem.StatType.Gold);
	}

	private IEnumerable<StatItem> GetCraftingStats(IStatisticsCampaignBehavior source)
	{
		yield return new StatItem("WeaponsCrafted", source.GetNumberOfWeaponsCrafted().ToString());
		string text = source.GetMostExpensiveItemCrafted().Item1 ?? ((object)GameTexts.FindText("str_no_items_crafted", (string)null)).ToString();
		GameTexts.SetVariable("LEFT", text);
		GameTexts.SetVariable("RIGHT", source.GetMostExpensiveItemCrafted().Item2.ToString());
		yield return new StatItem("MostExpensiveCraft", ((object)GameTexts.FindText("str_LEFT_over_RIGHT", (string)null)).ToString(), StatItem.StatType.Gold);
		yield return new StatItem("NumberOfPiecesUnlocked", source.GetNumberOfCraftingPartsUnlocked().ToString());
		yield return new StatItem("CraftingOrdersCompleted", source.GetNumberOfCraftingOrdersCompleted().ToString());
	}

	private IEnumerable<StatItem> GetCompanionStats(IStatisticsCampaignBehavior source)
	{
		yield return new StatItem("NumberOfHiredCompanions", source.GetNumberOfCompanionsHired().ToString());
		string text = source.GetCompanionWithMostIssuesSolved().Item1 ?? ((object)GameTexts.FindText("str_no_companions_with_issues_solved", (string)null)).ToString();
		GameTexts.SetVariable("LEFT", text);
		GameTexts.SetVariable("RIGHT", source.GetCompanionWithMostIssuesSolved().Item2);
		yield return new StatItem("MostIssueCompanion", ((object)GameTexts.FindText("str_LEFT_over_RIGHT", (string)null)).ToString(), StatItem.StatType.Issue);
		string text2 = source.GetCompanionWithMostKills().Item1 ?? ((object)GameTexts.FindText("str_no_companions_with_kills", (string)null)).ToString();
		GameTexts.SetVariable("LEFT", text2);
		GameTexts.SetVariable("RIGHT", source.GetCompanionWithMostKills().Item2);
		yield return new StatItem("MostKillCompanion", ((object)GameTexts.FindText("str_LEFT_over_RIGHT", (string)null)).ToString(), StatItem.StatType.Kill);
	}
}
