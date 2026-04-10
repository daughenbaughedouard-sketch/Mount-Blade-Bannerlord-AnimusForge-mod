using NavalDLC.CampaignBehaviors;
using NavalDLC.ComponentInterfaces;
using NavalDLC.GameComponents;
using NavalDLC.Missions;
using NavalDLC.Storyline;
using NavalDLC.Storyline.CampaignBehaviors;
using SandBox.GameComponents;
using StoryMode;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.ObjectSystem;

namespace NavalDLC;

public class NavalDLCSubModule : MBSubModuleBase
{
	public const string ShipPhysicsReferencesXMLPath = "ShipPhysicsReferences";

	public const string MissionShipsXMLPath = "MissionShips";

	public const string ModuleName = "NavalDLC";

	public const string FigureheadSlotTag = "figurehead";

	protected override void OnSubModuleLoad()
	{
		TauntUsageManager.Initialize();
	}

	protected override void RegisterSubModuleTypes()
	{
	}

	protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
	{
		game.AddGameHandler<NavalDLCManager>();
		NavalDLCManager.Instance = Game.Current.GetGameHandler<NavalDLCManager>();
		NavalDLCManager.Instance.OnGameStart(game, gameStarterObject);
		string applicationVersionBuildNumber = NavalVersion.GetApplicationVersionBuildNumber();
		Utilities.SetWatchdogValue("crash_tags.txt", "ModuleVersion", "NavalDLC", applicationVersionBuildNumber);
	}

	public override void OnGameEnd(Game game)
	{
		NavalDLCManager.Instance.OnGameEnd(game);
	}

	public override void InitializeSubModuleGameObjects(Game game)
	{
		NavalDLCManager.Instance.InitializeNavalGameObjects(game);
	}

	public override void RegisterSubModuleObjects(bool isSavedCampaign)
	{
		MBObjectManagerExtensions.LoadXML(MBObjectManager.Instance, "ShipUpgradePieces", false);
		MBObjectManagerExtensions.LoadXML(MBObjectManager.Instance, "ShipSlots", false);
		MBObjectManagerExtensions.LoadXML(MBObjectManager.Instance, "ShipHulls", false);
		MBObjectManagerExtensions.LoadXML(MBObjectManager.Instance, "ShipPhysicsReferences", false);
		MBObjectManagerExtensions.LoadXML(MBObjectManager.Instance, "MissionShips", false);
	}

	protected override void InitializeGameStarter(Game game, IGameStarter gameStarterObject)
	{
		if (game.GameType is Campaign)
		{
			CampaignGameStarter val = (CampaignGameStarter)(object)((gameStarterObject is CampaignGameStarter) ? gameStarterObject : null);
			AddBehaviors(val, game);
			AddModels(val);
		}
		else if (game.GameType is EditorGame)
		{
			gameStarterObject.AddModel<ShipPhysicsParametersModel>((MBGameModel<ShipPhysicsParametersModel>)new NavalDLCShipPhysicsParametersModel());
		}
	}

	public override void OnAfterGameInitializationFinished(Game game, object starterObject)
	{
		GameType gameType = game.GameType;
		Campaign val = (Campaign)(object)((gameType is Campaign) ? gameType : null);
		if (val != null)
		{
			val.CampaignMissionManager = (ICampaignMissionManager)(object)new NavalMissionManager(val.CampaignMissionManager);
		}
	}

	public override void OnGameInitializationFinished(Game game)
	{
		if (game.GameType is Campaign && game.GameType is CampaignStoryMode && StoryModeManager.Current != null)
		{
			NavalDLCManager.Instance.NavalStorylineData.Initialize();
		}
	}

	private void AddBehaviors(CampaignGameStarter gameStarter, Game game)
	{
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalTransitionCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalCharacterCreationCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new SeaDamageCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new ShipProductionCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new ShipTradeCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new ShipRepairCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new RaftStateCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new ShipUpgradeCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new PortCharactersCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new ClanFleetManagementCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalPatrolPartiesCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalVeteransWisdomCampaignBehaviour());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalFishingCampaignBehaviour());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalNimbleSurgeCampaignBehaviour());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStormriderCampaignBehaviour());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalOrderOfBattleCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalDLCTutorialBoxCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new PiratesCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalKingdomPolicyCampaignBehaviour());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new FishingPartyCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new StormCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalDLCFigureheadCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalShipDistributionCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new ShipNameCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalInitializationCampaignBehavior());
		gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalCompanionRolesCampaignBehavior());
		if (game.GameType is CampaignStoryMode && StoryModeManager.Current != null)
		{
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineCampaignBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineFirstActCampaignBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineSecondActCampaignBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineThirdActSecondQuestBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineThirdActThirdQuestBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineTravelCommentaryCampaignBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylinePlayerTownVisitCampaignBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineHeroAgentSpawnBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineThirdActFirstQuestBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineThirdActFourthQuestBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new DefeatTheCaptorsQuestBehavior());
			gameStarter.AddBehavior((CampaignBehaviorBase)(object)new NavalStorylineThirdActFifthQuestBehaviour());
		}
	}

	private void AddModels(CampaignGameStarter campaignGameStarter)
	{
		campaignGameStarter.AddModel<PartyNavigationModel>((MBGameModel<PartyNavigationModel>)(object)new NavalPartyNavigationModel(campaignGameStarter.GetModel<PartyNavigationModel>()));
		campaignGameStarter.AddModel<BanditDensityModel>((MBGameModel<BanditDensityModel>)(object)new NavalDLCBanditDensityModel());
		campaignGameStarter.AddModel<CampaignShipDamageModel>((MBGameModel<CampaignShipDamageModel>)(object)new NavalDLCCampaignShipDamageModel());
		campaignGameStarter.AddModel<CampaignShipParametersModel>((MBGameModel<CampaignShipParametersModel>)(object)new NavalDLCCampaignShipParametersModel());
		campaignGameStarter.AddModel<ShipDeploymentModel>((MBGameModel<ShipDeploymentModel>)new NavalDLCShipDeploymentModel());
		campaignGameStarter.AddModel<ArmyManagementCalculationModel>((MBGameModel<ArmyManagementCalculationModel>)(object)new NavalDLCArmyManagementCalculationModel());
		campaignGameStarter.AddModel<PartySpeedModel>((MBGameModel<PartySpeedModel>)(object)new NavalDLCPartySpeedCalculationModel());
		campaignGameStarter.AddModel<RaidModel>((MBGameModel<RaidModel>)(object)new NavalDLCRaidModel());
		campaignGameStarter.AddModel<BuildingModel>((MBGameModel<BuildingModel>)(object)new NavalDLCBuildingModel());
		campaignGameStarter.AddModel<BattleRewardModel>((MBGameModel<BattleRewardModel>)(object)new NavalDLCBattleRewardModel());
		campaignGameStarter.AddModel<MilitaryPowerModel>((MBGameModel<MilitaryPowerModel>)(object)new NavalDLCMilitaryPowerModel());
		campaignGameStarter.AddModel<ShipCostModel>((MBGameModel<ShipCostModel>)(object)new NavalDLCShipCostModel());
		campaignGameStarter.AddModel<CombatSimulationModel>((MBGameModel<CombatSimulationModel>)(object)new NavalDLCCombatSimulationModel());
		campaignGameStarter.AddModel<IncidentModel>((MBGameModel<IncidentModel>)(object)new NavalDLCIncidentModel());
		campaignGameStarter.AddModel<EncounterGameMenuModel>((MBGameModel<EncounterGameMenuModel>)(object)new NavalEncounterMenuModel());
		campaignGameStarter.AddModel<CaravanModel>((MBGameModel<CaravanModel>)(object)new NavalDLCCaravanModel());
		campaignGameStarter.AddModel<PartyShipLimitModel>((MBGameModel<PartyShipLimitModel>)(object)new NavalDLCShipLimitModel());
		campaignGameStarter.AddModel<PartySizeLimitModel>((MBGameModel<PartySizeLimitModel>)(object)new NavalDLCPartySizeLimitModel());
		campaignGameStarter.AddModel<MobilePartyAIModel>((MBGameModel<MobilePartyAIModel>)(object)new NavalDLCMobilePartyAIModel());
		campaignGameStarter.AddModel<EncounterModel>((MBGameModel<EncounterModel>)(object)new NavalDLCEncounterModel());
		campaignGameStarter.AddModel<VoiceOverModel>((MBGameModel<VoiceOverModel>)(object)new NavalDLCVoiceOverModel());
		campaignGameStarter.AddModel<HeroAgentLocationModel>((MBGameModel<HeroAgentLocationModel>)(object)new NavalDLCHeroAgentLocationModel());
		campaignGameStarter.AddModel<TournamentModel>((MBGameModel<TournamentModel>)(object)new NavalDLCTournamentModel());
		campaignGameStarter.AddModel<SettlementAccessModel>((MBGameModel<SettlementAccessModel>)(object)new NavalDLCSettlementAccessModel());
		campaignGameStarter.AddModel<FleetManagementModel>((MBGameModel<FleetManagementModel>)(object)new NavalDLCFleetManagementModel());
		campaignGameStarter.AddModel<TroopSacrificeModel>((MBGameModel<TroopSacrificeModel>)(object)new NavalDLCTroopSacrificeModel());
		campaignGameStarter.AddModel<CombatXpModel>((MBGameModel<CombatXpModel>)(object)new NavalDLCCombatXpModel());
		campaignGameStarter.AddModel<InventoryCapacityModel>((MBGameModel<InventoryCapacityModel>)(object)new NavalDLCInventoryCapacityModel());
		campaignGameStarter.AddModel<MobilePartyFoodConsumptionModel>((MBGameModel<MobilePartyFoodConsumptionModel>)(object)new NavalDLCMobilePartyFoodConsumptionModel());
		campaignGameStarter.AddModel<PartyHealingModel>((MBGameModel<PartyHealingModel>)(object)new NavalDLCPartyHealingModel());
		campaignGameStarter.AddModel<PartyMoraleModel>((MBGameModel<PartyMoraleModel>)(object)new NavalDLCPartyMoraleModel());
		campaignGameStarter.AddModel<PartyTrainingModel>((MBGameModel<PartyTrainingModel>)(object)new NavalDLCPartyTrainingModel());
		campaignGameStarter.AddModel<PartyTroopUpgradeModel>((MBGameModel<PartyTroopUpgradeModel>)(object)new NavalDLCPartyTroopUpgradeModel());
		campaignGameStarter.AddModel<PartyWageModel>((MBGameModel<PartyWageModel>)(object)new NavalDLCPartyWageModel());
		campaignGameStarter.AddModel<PrisonerRecruitmentCalculationModel>((MBGameModel<PrisonerRecruitmentCalculationModel>)(object)new NavalDLCPrisonerRecruitmentCalculationModel());
		campaignGameStarter.AddModel<SettlementGarrisonModel>((MBGameModel<SettlementGarrisonModel>)(object)new NavalDLCSettlementGarrisonModel());
		campaignGameStarter.AddModel<SettlementMilitiaModel>((MBGameModel<SettlementMilitiaModel>)(object)new NavalDLCSettlementMilitiaModel());
		campaignGameStarter.AddModel<VillageProductionCalculatorModel>((MBGameModel<VillageProductionCalculatorModel>)(object)new NavalDLCVillageProductionCalculatorModel());
		campaignGameStarter.AddModel<TroopSacrificeModel>((MBGameModel<TroopSacrificeModel>)(object)new NavalDLCTroopSacrificeModel());
		campaignGameStarter.AddModel<MapDistanceModel>((MBGameModel<MapDistanceModel>)(object)new NavalDLCMapDistanceModel());
		campaignGameStarter.AddModel<MapVisibilityModel>((MBGameModel<MapVisibilityModel>)(object)new NavalDLCMapVisibilityModel());
		campaignGameStarter.AddModel<PartyImpairmentModel>((MBGameModel<PartyImpairmentModel>)(object)new NavalDLCPartyImpairmentModel());
		campaignGameStarter.AddModel<PartyTransitionModel>((MBGameModel<PartyTransitionModel>)(object)new NavalDLCPartyTransitionModel());
		campaignGameStarter.AddModel<SettlementProsperityModel>((MBGameModel<SettlementProsperityModel>)(object)new NavalDLCSettlementProsperityModel());
		campaignGameStarter.AddModel<WorkshopModel>((MBGameModel<WorkshopModel>)(object)new NavalDLCWorkshopModel());
		campaignGameStarter.AddModel<BuildingConstructionModel>((MBGameModel<BuildingConstructionModel>)(object)new NavalDLCBuildingConstructionModel());
		campaignGameStarter.AddModel<SettlementSecurityModel>((MBGameModel<SettlementSecurityModel>)(object)new NavalDLCSettlementSecurityModel());
		campaignGameStarter.AddModel<ClanFinanceModel>((MBGameModel<ClanFinanceModel>)(object)new NavalDLCClanFinanceModel());
		campaignGameStarter.AddModel<ClanPoliticsModel>((MBGameModel<ClanPoliticsModel>)(object)new NavalDLCClanPoliticsModel());
		campaignGameStarter.AddModel<ShipStatModel>((MBGameModel<ShipStatModel>)(object)new NavalDLCShipStatModel());
		campaignGameStarter.AddModel<MapStormModel>((MBGameModel<MapStormModel>)new NavalDLCStormModel());
		campaignGameStarter.AddModel<ShipPhysicsParametersModel>((MBGameModel<ShipPhysicsParametersModel>)new NavalDLCShipPhysicsParametersModel());
		campaignGameStarter.AddModel<ClanShipOwnershipModel>((MBGameModel<ClanShipOwnershipModel>)new NavalDLCClanShipOwnershipModel());
		campaignGameStarter.AddModel<SettlementPatrolModel>((MBGameModel<SettlementPatrolModel>)(object)new NavalSettlementPatrolModel());
		campaignGameStarter.AddModel<CharacterDevelopmentModel>((MBGameModel<CharacterDevelopmentModel>)(object)new NavalCharacterDevelopmentModel());
		campaignGameStarter.AddModel<TradeAgreementModel>((MBGameModel<TradeAgreementModel>)(object)new NavalTradeAgreementModel());
		campaignGameStarter.AddModel<AgentStatCalculateModel>((MBGameModel<AgentStatCalculateModel>)(object)new NavalAgentStatCalculateModel());
		campaignGameStarter.AddModel<AgentApplyDamageModel>((MBGameModel<AgentApplyDamageModel>)(object)new NavalAgentApplyDamageModel());
		campaignGameStarter.AddModel<StrikeMagnitudeCalculationModel>((MBGameModel<StrikeMagnitudeCalculationModel>)(object)new NavalStrikeMagnitudeModel());
		campaignGameStarter.AddModel<BattleMoraleModel>((MBGameModel<BattleMoraleModel>)(object)new NavalBattleMoraleModel());
		campaignGameStarter.AddModel<MissionShipParametersModel>((MBGameModel<MissionShipParametersModel>)(object)new NavalMissionShipParametersModel());
		campaignGameStarter.AddModel<MissionSiegeEngineCalculationModel>((MBGameModel<MissionSiegeEngineCalculationModel>)(object)new NavalMissionSiegeEngineCalculationModel());
		campaignGameStarter.AddModel<BattleInitializationModel>((MBGameModel<BattleInitializationModel>)(object)new NavalBattleInitializationModel());
		campaignGameStarter.AddModel<ShipDistributionModel>((MBGameModel<ShipDistributionModel>)new NavalDLCShipDistributionModel());
		campaignGameStarter.AddModel<ClanMemberPartyRoleModel>((MBGameModel<ClanMemberPartyRoleModel>)(object)new NavalDLCClanMemberPartyRoleModel());
		campaignGameStarter.AddModel<TargetScoreCalculatingModel>((MBGameModel<TargetScoreCalculatingModel>)(object)new NavalTargetScoreCalculatingModel());
		if (Game.Current.GameType is Campaign)
		{
			campaignGameStarter.AddModel<MapWeatherModel>((MBGameModel<MapWeatherModel>)(object)new NavalDLCMapWeatherModel());
		}
	}
}
