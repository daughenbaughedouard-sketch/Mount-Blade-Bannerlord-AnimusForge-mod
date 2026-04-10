using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;

namespace SandBox.CampaignBehaviors;

public class StatisticsCampaignBehavior : CampaignBehaviorBase, IStatisticsCampaignBehavior, ICampaignBehavior
{
	private class StatisticsMissionLogic : MissionLogic
	{
		private readonly StatisticsCampaignBehavior behavior = Campaign.Current.CampaignBehaviorManager.GetBehavior<StatisticsCampaignBehavior>();

		public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
		{
			if (behavior != null)
			{
				behavior.OnAgentRemoved(affectedAgent, affectorAgent);
			}
		}
	}

	private int _highestTournamentRank;

	private int _numberOfTournamentWins;

	private int _numberOfChildrenBorn;

	private int _numberOfPrisonersRecruited;

	private int _numberOfTroopsRecruited;

	private int _numberOfClansDefected;

	private int _numberOfIssuesSolved;

	private int _totalInfluenceEarned;

	private int _totalCrimeRatingGained;

	private ulong _totalTimePlayedInSeconds;

	private int _numberOfbattlesWon;

	private int _numberOfbattlesLost;

	private int _largestBattleWonAsLeader;

	private int _largestArmyFormedByPlayer;

	private int _numberOfEnemyClansDestroyed;

	private int _numberOfHeroesKilledInBattle;

	private int _numberOfTroopsKnockedOrKilledAsParty;

	private int _numberOfTroopsKnockedOrKilledByPlayer;

	private int _numberOfHeroPrisonersTaken;

	private int _numberOfTroopPrisonersTaken;

	private int _numberOfTownsCaptured;

	private int _numberOfHideoutsCleared;

	private int _numberOfCastlesCaptured;

	private int _numberOfVillagesRaided;

	private CampaignTime _timeSpentAsPrisoner;

	private ulong _totalDenarsEarned;

	private ulong _denarsEarnedFromCaravans;

	private ulong _denarsEarnedFromWorkshops;

	private ulong _denarsEarnedFromRansoms;

	private ulong _denarsEarnedFromTaxes;

	private ulong _denarsEarnedFromTributes;

	private ulong _denarsPaidAsTributes;

	private int _numberOfCraftingPartsUnlocked;

	private int _numberOfWeaponsCrafted;

	private int _numberOfCraftingOrdersCompleted;

	private (string, int) _mostExpensiveItemCrafted = (null, 0);

	private int _numberOfCompanionsHired;

	private Dictionary<Hero, (int, int)> _companionData = new Dictionary<Hero, (int, int)>();

	private int _lastPlayerBattleSize;

	private DateTime _lastGameplayTimeCheck;

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<int>("_highestTournamentRank", ref _highestTournamentRank);
		dataStore.SyncData<int>("_numberOfTournamentWins", ref _numberOfTournamentWins);
		dataStore.SyncData<int>("_numberOfChildrenBorn", ref _numberOfChildrenBorn);
		dataStore.SyncData<int>("_numberOfPrisonersRecruited", ref _numberOfPrisonersRecruited);
		dataStore.SyncData<int>("_numberOfTroopsRecruited", ref _numberOfTroopsRecruited);
		dataStore.SyncData<int>("_numberOfClansDefected", ref _numberOfClansDefected);
		dataStore.SyncData<int>("_numberOfIssuesSolved", ref _numberOfIssuesSolved);
		dataStore.SyncData<int>("_totalInfluenceEarned", ref _totalInfluenceEarned);
		dataStore.SyncData<int>("_totalCrimeRatingGained", ref _totalCrimeRatingGained);
		dataStore.SyncData<ulong>("_totalTimePlayedInSeconds", ref _totalTimePlayedInSeconds);
		dataStore.SyncData<int>("_numberOfbattlesWon", ref _numberOfbattlesWon);
		dataStore.SyncData<int>("_numberOfbattlesLost", ref _numberOfbattlesLost);
		dataStore.SyncData<int>("_largestBattleWonAsLeader", ref _largestBattleWonAsLeader);
		dataStore.SyncData<int>("_largestArmyFormedByPlayer", ref _largestArmyFormedByPlayer);
		dataStore.SyncData<int>("_numberOfEnemyClansDestroyed", ref _numberOfEnemyClansDestroyed);
		dataStore.SyncData<int>("_numberOfHeroesKilledInBattle", ref _numberOfHeroesKilledInBattle);
		dataStore.SyncData<int>("_numberOfTroopsKnockedOrKilledAsParty", ref _numberOfTroopsKnockedOrKilledAsParty);
		dataStore.SyncData<int>("_numberOfTroopsKnockedOrKilledByPlayer", ref _numberOfTroopsKnockedOrKilledByPlayer);
		dataStore.SyncData<int>("_numberOfHeroPrisonersTaken", ref _numberOfHeroPrisonersTaken);
		dataStore.SyncData<int>("_numberOfTroopPrisonersTaken", ref _numberOfTroopPrisonersTaken);
		dataStore.SyncData<int>("_numberOfTownsCaptured", ref _numberOfTownsCaptured);
		dataStore.SyncData<int>("_numberOfHideoutsCleared", ref _numberOfHideoutsCleared);
		dataStore.SyncData<int>("_numberOfCastlesCaptured", ref _numberOfCastlesCaptured);
		dataStore.SyncData<int>("_numberOfVillagesRaided", ref _numberOfVillagesRaided);
		dataStore.SyncData<CampaignTime>("_timeSpentAsPrisoner", ref _timeSpentAsPrisoner);
		dataStore.SyncData<ulong>("_totalDenarsEarned", ref _totalDenarsEarned);
		dataStore.SyncData<ulong>("_denarsEarnedFromCaravans", ref _denarsEarnedFromCaravans);
		dataStore.SyncData<ulong>("_denarsEarnedFromWorkshops", ref _denarsEarnedFromWorkshops);
		dataStore.SyncData<ulong>("_denarsEarnedFromRansoms", ref _denarsEarnedFromRansoms);
		dataStore.SyncData<ulong>("_denarsEarnedFromTaxes", ref _denarsEarnedFromTaxes);
		dataStore.SyncData<ulong>("_denarsEarnedFromTributes", ref _denarsEarnedFromTributes);
		dataStore.SyncData<ulong>("_denarsPaidAsTributes", ref _denarsPaidAsTributes);
		dataStore.SyncData<int>("_numberOfCraftingPartsUnlocked", ref _numberOfCraftingPartsUnlocked);
		dataStore.SyncData<int>("_numberOfWeaponsCrafted", ref _numberOfWeaponsCrafted);
		dataStore.SyncData<int>("_numberOfCraftingOrdersCompleted", ref _numberOfCraftingOrdersCompleted);
		dataStore.SyncData<(string, int)>("_mostExpensiveItemCrafted", ref _mostExpensiveItemCrafted);
		dataStore.SyncData<int>("_numberOfCompanionsHired", ref _numberOfCompanionsHired);
		dataStore.SyncData<Dictionary<Hero, (int, int)>>("_companionData", ref _companionData);
		dataStore.SyncData<int>("_lastPlayerBattleSize", ref _lastPlayerBattleSize);
	}

	public override void RegisterEvents()
	{
		CampaignEvents.HeroCreated.AddNonSerializedListener((object)this, (Action<Hero, bool>)OnHeroCreated);
		CampaignEvents.OnIssueUpdatedEvent.AddNonSerializedListener((object)this, (Action<IssueBase, IssueUpdateDetails, Hero>)OnIssueUpdated);
		CampaignEvents.TournamentFinished.AddNonSerializedListener((object)this, (Action<CharacterObject, MBReadOnlyList<CharacterObject>, Town, ItemObject>)OnTournamentFinished);
		CampaignEvents.OnClanInfluenceChangedEvent.AddNonSerializedListener((object)this, (Action<Clan, float>)OnClanInfluenceChanged);
		CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnAfterSessionLaunched);
		CampaignEvents.CrimeRatingChanged.AddNonSerializedListener((object)this, (Action<IFaction, float>)OnCrimeRatingChanged);
		CampaignEvents.OnMainPartyPrisonerRecruitedEvent.AddNonSerializedListener((object)this, (Action<FlattenedTroopRoster>)OnMainPartyPrisonerRecruited);
		CampaignEvents.OnUnitRecruitedEvent.AddNonSerializedListener((object)this, (Action<CharacterObject, int>)OnUnitRecruited);
		CampaignEvents.OnBeforeSaveEvent.AddNonSerializedListener((object)this, (Action)OnBeforeSave);
		CampaignEvents.CraftingPartUnlockedEvent.AddNonSerializedListener((object)this, (Action<CraftingPiece>)OnCraftingPartUnlocked);
		CampaignEvents.OnNewItemCraftedEvent.AddNonSerializedListener((object)this, (Action<ItemObject, ItemModifier, bool>)OnNewItemCrafted);
		CampaignEvents.NewCompanionAdded.AddNonSerializedListener((object)this, (Action<Hero>)OnNewCompanionAdded);
		CampaignEvents.HeroOrPartyTradedGold.AddNonSerializedListener((object)this, (Action<(Hero, PartyBase), (Hero, PartyBase), (int, string), bool>)OnHeroOrPartyTradedGold);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnd);
		CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener((object)this, (Action<Clan>)OnClanDestroyed);
		CampaignEvents.PartyAttachedAnotherParty.AddNonSerializedListener((object)this, (Action<MobileParty>)OnPartyAttachedAnotherParty);
		CampaignEvents.ArmyCreated.AddNonSerializedListener((object)this, (Action<Army>)OnArmyCreated);
		CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener((object)this, (Action<PartyBase, Hero>)OnHeroPrisonerTaken);
		CampaignEvents.OnPrisonerTakenEvent.AddNonSerializedListener((object)this, (Action<FlattenedTroopRoster>)OnPrisonersTaken);
		CampaignEvents.RaidCompletedEvent.AddNonSerializedListener((object)this, (Action<BattleSideEnum, RaidEventComponent>)OnRaidCompleted);
		CampaignEvents.OnHideoutBattleCompletedEvent.AddNonSerializedListener((object)this, (Action<BattleSideEnum, HideoutEventComponent, HideoutBattleEndState>)OnHideoutBattleCompleted);
		CampaignEvents.MapEventStarted.AddNonSerializedListener((object)this, (Action<MapEvent, PartyBase, PartyBase>)OnMapEventStarted);
		CampaignEvents.HeroPrisonerReleased.AddNonSerializedListener((object)this, (Action<Hero, PartyBase, IFaction, EndCaptivityDetail, bool>)OnHeroPrisonerReleased);
		CampaignEvents.OnPlayerPartyKnockedOrKilledTroopEvent.AddNonSerializedListener((object)this, (Action<CharacterObject>)OnPlayerPartyKnockedOrKilledTroop);
		CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionStarted);
		CampaignEvents.OnPlayerEarnedGoldFromAssetEvent.AddNonSerializedListener((object)this, (Action<AssetIncomeType, int>)OnPlayerEarnedGoldFromAsset);
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
	}

	private void OnBeforeSave()
	{
		UpdateTotalTimePlayedInSeconds();
	}

	private void OnAfterSessionLaunched(CampaignGameStarter starter)
	{
		_lastGameplayTimeCheck = DateTime.Now;
		if (_highestTournamentRank == 0)
		{
			_highestTournamentRank = Campaign.Current.TournamentManager.GetLeaderBoardRank(Hero.MainHero);
		}
	}

	public void OnDefectionPersuasionSucess()
	{
		_numberOfClansDefected++;
	}

	private void OnUnitRecruited(CharacterObject character, int amount)
	{
		_numberOfTroopsRecruited += amount;
	}

	private void OnMainPartyPrisonerRecruited(FlattenedTroopRoster flattenedTroopRoster)
	{
		_numberOfPrisonersRecruited += LinQuick.CountQ<FlattenedTroopRosterElement>((IEnumerable<FlattenedTroopRosterElement>)flattenedTroopRoster);
	}

	private void OnCrimeRatingChanged(IFaction kingdom, float deltaCrimeAmount)
	{
		if (deltaCrimeAmount > 0f)
		{
			_totalCrimeRatingGained += (int)deltaCrimeAmount;
		}
	}

	private void OnClanInfluenceChanged(Clan clan, float change)
	{
		if (change > 0f && clan == Clan.PlayerClan)
		{
			_totalInfluenceEarned += (int)change;
		}
	}

	private void OnTournamentFinished(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town, ItemObject prize)
	{
		if (winner.HeroObject == Hero.MainHero)
		{
			_numberOfTournamentWins++;
			int leaderBoardRank = Campaign.Current.TournamentManager.GetLeaderBoardRank(Hero.MainHero);
			if (leaderBoardRank < _highestTournamentRank)
			{
				_highestTournamentRank = leaderBoardRank;
			}
		}
	}

	private void OnIssueUpdated(IssueBase issue, IssueUpdateDetails details, Hero issueSolver = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if ((int)details != 5 && (int)details != 3 && (int)details != 6)
		{
			return;
		}
		_numberOfIssuesSolved++;
		if (issueSolver != null && issueSolver.IsPlayerCompanion)
		{
			if (_companionData.ContainsKey(issueSolver))
			{
				_companionData[issueSolver] = (_companionData[issueSolver].Item1 + 1, _companionData[issueSolver].Item2);
			}
			else
			{
				_companionData.Add(issueSolver, (1, 0));
			}
		}
	}

	private void OnHeroCreated(Hero hero, bool isBornNaturally = false)
	{
		if (hero.Mother == Hero.MainHero || hero.Father == Hero.MainHero)
		{
			_numberOfChildrenBorn++;
		}
	}

	private void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (killer != null && killer.PartyBelongedTo == MobileParty.MainParty && (int)detail == 4)
		{
			_numberOfHeroesKilledInBattle++;
		}
	}

	private void OnMissionStarted(IMission mission)
	{
		StatisticsMissionLogic statisticsMissionLogic = new StatisticsMissionLogic();
		Mission.Current.AddMissionBehavior((MissionBehavior)(object)statisticsMissionLogic);
	}

	private void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Invalid comparison between Unknown and I4
		if (affectorAgent == null)
		{
			return;
		}
		if (affectorAgent == Agent.Main)
		{
			_numberOfTroopsKnockedOrKilledByPlayer++;
		}
		else if (affectorAgent.IsPlayerTroop)
		{
			_numberOfTroopsKnockedOrKilledAsParty++;
		}
		else if (affectorAgent.IsHero)
		{
			BasicCharacterObject character = affectorAgent.Character;
			Hero heroObject = ((CharacterObject)((character is CharacterObject) ? character : null)).HeroObject;
			if (heroObject.IsPlayerCompanion)
			{
				if (_companionData.ContainsKey(heroObject))
				{
					_companionData[heroObject] = (_companionData[heroObject].Item1, _companionData[heroObject].Item2 + 1);
				}
				else
				{
					_companionData.Add(heroObject, (0, 1));
				}
			}
		}
		if (affectedAgent.IsHero && (int)affectedAgent.State == 4)
		{
			_numberOfHeroesKilledInBattle++;
		}
	}

	private void OnPlayerPartyKnockedOrKilledTroop(CharacterObject troop)
	{
		_numberOfTroopsKnockedOrKilledAsParty++;
	}

	private void OnHeroPrisonerReleased(Hero prisoner, PartyBase party, IFaction capturerFaction, EndCaptivityDetail detail, bool showNotification)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (prisoner == Hero.MainHero)
		{
			_timeSpentAsPrisoner += CampaignTime.Now - PlayerCaptivity.CaptivityStartTime;
		}
	}

	private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
	{
		if (mapEvent.IsPlayerMapEvent)
		{
			_lastPlayerBattleSize = mapEvent.AttackerSide.TroopCount + mapEvent.DefenderSide.TroopCount;
		}
	}

	private void OnHideoutBattleCompleted(BattleSideEnum winnerSide, HideoutEventComponent hideoutEventComponent, HideoutBattleEndState battleEndState)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (((MapEventComponent)hideoutEventComponent).MapEvent.PlayerSide == winnerSide)
		{
			_numberOfHideoutsCleared++;
		}
	}

	private void OnRaidCompleted(BattleSideEnum winnerSide, RaidEventComponent raidEventComponent)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (((MapEventComponent)raidEventComponent).MapEvent.HasWinner && ((MapEventComponent)raidEventComponent).MapEvent.PlayerSide == winnerSide)
		{
			_numberOfVillagesRaided++;
		}
	}

	private void OnPrisonersTaken(FlattenedTroopRoster troopRoster)
	{
		_numberOfTroopPrisonersTaken += LinQuick.CountQ<FlattenedTroopRosterElement>((IEnumerable<FlattenedTroopRosterElement>)troopRoster);
	}

	private void OnHeroPrisonerTaken(PartyBase capturer, Hero prisoner)
	{
		if (capturer == PartyBase.MainParty)
		{
			_numberOfHeroPrisonersTaken++;
		}
	}

	private void OnArmyCreated(Army army)
	{
		if (army.LeaderParty == MobileParty.MainParty && _largestArmyFormedByPlayer < army.TotalManCount)
		{
			_largestArmyFormedByPlayer = army.TotalManCount;
		}
	}

	private void OnPartyAttachedAnotherParty(MobileParty mobileParty)
	{
		if (mobileParty.Army == MobileParty.MainParty.Army && MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty && _largestArmyFormedByPlayer < MobileParty.MainParty.Army.TotalManCount)
		{
			_largestArmyFormedByPlayer = MobileParty.MainParty.Army.TotalManCount;
		}
	}

	private void OnClanDestroyed(Clan clan)
	{
		if (clan.MapFaction.IsAtWarWith(Clan.PlayerClan.MapFaction))
		{
			_numberOfEnemyClansDestroyed++;
		}
	}

	private void OnMapEventEnd(MapEvent mapEvent)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!mapEvent.IsPlayerMapEvent || !mapEvent.HasWinner)
		{
			return;
		}
		if (mapEvent.WinningSide == mapEvent.PlayerSide)
		{
			_numberOfbattlesWon++;
			if (mapEvent.IsSiegeAssault && !mapEvent.IsPlayerSergeant() && mapEvent.MapEventSettlement != null)
			{
				if (mapEvent.MapEventSettlement.IsTown)
				{
					_numberOfTownsCaptured++;
				}
				else if (mapEvent.MapEventSettlement.IsCastle)
				{
					_numberOfCastlesCaptured++;
				}
			}
			if (_largestBattleWonAsLeader < _lastPlayerBattleSize && !mapEvent.IsPlayerSergeant())
			{
				_largestBattleWonAsLeader = _lastPlayerBattleSize;
			}
		}
		else
		{
			_numberOfbattlesLost++;
		}
	}

	private void OnHeroOrPartyTradedGold((Hero, PartyBase) giver, (Hero, PartyBase) recipient, (int, string) goldAmount, bool showNotification)
	{
		if (recipient.Item1 == Hero.MainHero || recipient.Item2 == PartyBase.MainParty)
		{
			_totalDenarsEarned += (ulong)goldAmount.Item1;
		}
	}

	public void OnPlayerAcceptedRansomOffer(int ransomPrice)
	{
		_denarsEarnedFromRansoms += (ulong)ransomPrice;
	}

	private void OnPlayerEarnedGoldFromAsset(AssetIncomeType assetType, int amount)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		switch ((int)assetType)
		{
		case 0:
			_denarsEarnedFromWorkshops += (ulong)amount;
			break;
		case 1:
			_denarsEarnedFromCaravans += (ulong)amount;
			break;
		case 2:
			_denarsEarnedFromTaxes += (ulong)amount;
			break;
		case 3:
			_denarsEarnedFromTributes += (ulong)amount;
			break;
		}
	}

	private void OnNewCompanionAdded(Hero hero)
	{
		_numberOfCompanionsHired++;
	}

	private void OnNewItemCrafted(ItemObject itemObject, ItemModifier overriddenItemModifier, bool isCraftingOrderItem)
	{
		_numberOfWeaponsCrafted++;
		if (isCraftingOrderItem)
		{
			_numberOfCraftingOrdersCompleted++;
		}
		if (_mostExpensiveItemCrafted.Item2 == 0 || _mostExpensiveItemCrafted.Item2 < itemObject.Value)
		{
			_mostExpensiveItemCrafted.Item1 = ((object)itemObject.Name).ToString();
			_mostExpensiveItemCrafted.Item2 = itemObject.Value;
		}
	}

	private void OnCraftingPartUnlocked(CraftingPiece craftingPiece)
	{
		_numberOfCraftingPartsUnlocked++;
	}

	public (string name, int value) GetCompanionWithMostKills()
	{
		if (Extensions.IsEmpty<KeyValuePair<Hero, (int, int)>>((IEnumerable<KeyValuePair<Hero, (int, int)>>)_companionData))
		{
			return (name: null, value: 0);
		}
		KeyValuePair<Hero, (int, int)> keyValuePair = Extensions.MaxBy<KeyValuePair<Hero, (int, int)>, int>((IEnumerable<KeyValuePair<Hero, (int, int)>>)_companionData, (Func<KeyValuePair<Hero, (int, int)>, int>)((KeyValuePair<Hero, (int, int)> kvp) => kvp.Value.Item2));
		return (name: ((object)keyValuePair.Key.Name).ToString(), value: keyValuePair.Value.Item2);
	}

	public (string name, int value) GetCompanionWithMostIssuesSolved()
	{
		if (Extensions.IsEmpty<KeyValuePair<Hero, (int, int)>>((IEnumerable<KeyValuePair<Hero, (int, int)>>)_companionData))
		{
			return (name: null, value: 0);
		}
		KeyValuePair<Hero, (int, int)> keyValuePair = Extensions.MaxBy<KeyValuePair<Hero, (int, int)>, int>((IEnumerable<KeyValuePair<Hero, (int, int)>>)_companionData, (Func<KeyValuePair<Hero, (int, int)>, int>)((KeyValuePair<Hero, (int, int)> kvp) => kvp.Value.Item1));
		return (name: ((object)keyValuePair.Key.Name).ToString(), value: keyValuePair.Value.Item1);
	}

	public int GetHighestTournamentRank()
	{
		return _highestTournamentRank;
	}

	public int GetNumberOfTournamentWins()
	{
		return _numberOfTournamentWins;
	}

	public int GetNumberOfChildrenBorn()
	{
		return _numberOfChildrenBorn;
	}

	public int GetNumberOfPrisonersRecruited()
	{
		return _numberOfPrisonersRecruited;
	}

	public int GetNumberOfTroopsRecruited()
	{
		return _numberOfTroopsRecruited;
	}

	public int GetNumberOfClansDefected()
	{
		return _numberOfClansDefected;
	}

	public int GetNumberOfIssuesSolved()
	{
		return _numberOfIssuesSolved;
	}

	public int GetTotalInfluenceEarned()
	{
		return _totalInfluenceEarned;
	}

	public int GetTotalCrimeRatingGained()
	{
		return _totalCrimeRatingGained;
	}

	public int GetNumberOfBattlesWon()
	{
		return _numberOfbattlesWon;
	}

	public int GetNumberOfBattlesLost()
	{
		return _numberOfbattlesLost;
	}

	public int GetLargestBattleWonAsLeader()
	{
		return _largestBattleWonAsLeader;
	}

	public int GetLargestArmyFormedByPlayer()
	{
		return _largestArmyFormedByPlayer;
	}

	public int GetNumberOfEnemyClansDestroyed()
	{
		return _numberOfEnemyClansDestroyed;
	}

	public int GetNumberOfHeroesKilledInBattle()
	{
		return _numberOfHeroesKilledInBattle;
	}

	public int GetNumberOfTroopsKnockedOrKilledAsParty()
	{
		return _numberOfTroopsKnockedOrKilledAsParty;
	}

	public int GetNumberOfTroopsKnockedOrKilledByPlayer()
	{
		return _numberOfTroopsKnockedOrKilledByPlayer;
	}

	public int GetNumberOfHeroPrisonersTaken()
	{
		return _numberOfHeroPrisonersTaken;
	}

	public int GetNumberOfTroopPrisonersTaken()
	{
		return _numberOfTroopPrisonersTaken;
	}

	public int GetNumberOfTownsCaptured()
	{
		return _numberOfTownsCaptured;
	}

	public int GetNumberOfHideoutsCleared()
	{
		return _numberOfHideoutsCleared;
	}

	public int GetNumberOfCastlesCaptured()
	{
		return _numberOfCastlesCaptured;
	}

	public int GetNumberOfVillagesRaided()
	{
		return _numberOfVillagesRaided;
	}

	public int GetNumberOfCraftingPartsUnlocked()
	{
		return _numberOfCraftingPartsUnlocked;
	}

	public int GetNumberOfWeaponsCrafted()
	{
		return _numberOfWeaponsCrafted;
	}

	public int GetNumberOfCraftingOrdersCompleted()
	{
		return _numberOfCraftingOrdersCompleted;
	}

	public int GetNumberOfCompanionsHired()
	{
		return _numberOfCompanionsHired;
	}

	public CampaignTime GetTimeSpentAsPrisoner()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _timeSpentAsPrisoner;
	}

	public ulong GetTotalTimePlayedInSeconds()
	{
		UpdateTotalTimePlayedInSeconds();
		return _totalTimePlayedInSeconds;
	}

	public ulong GetTotalDenarsEarned()
	{
		return _totalDenarsEarned;
	}

	public ulong GetDenarsEarnedFromCaravans()
	{
		return _denarsEarnedFromCaravans;
	}

	public ulong GetDenarsEarnedFromWorkshops()
	{
		return _denarsEarnedFromWorkshops;
	}

	public ulong GetDenarsEarnedFromRansoms()
	{
		return _denarsEarnedFromRansoms;
	}

	public ulong GetDenarsEarnedFromTaxes()
	{
		return _denarsEarnedFromTaxes;
	}

	public ulong GetDenarsEarnedFromTributes()
	{
		return _denarsEarnedFromTributes;
	}

	public ulong GetDenarsPaidAsTributes()
	{
		return _denarsPaidAsTributes;
	}

	public CampaignTime GetTotalTimePlayed()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return CampaignTime.Now - Campaign.Current.Models.CampaignTimeModel.CampaignStartTime;
	}

	public (string, int) GetMostExpensiveItemCrafted()
	{
		return _mostExpensiveItemCrafted;
	}

	private void UpdateTotalTimePlayedInSeconds()
	{
		double totalSeconds = (DateTime.Now - _lastGameplayTimeCheck).TotalSeconds;
		if (totalSeconds > 9.999999747378752E-06)
		{
			_totalTimePlayedInSeconds += (ulong)totalSeconds;
			_lastGameplayTimeCheck = DateTime.Now;
		}
	}
}
