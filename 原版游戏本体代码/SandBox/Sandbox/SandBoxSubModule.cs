using SandBox.AI;
using SandBox.CampaignBehaviors;
using SandBox.GameComponents;
using SandBox.Issues;
using SandBox.Objects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace SandBox;

public class SandBoxSubModule : MBSubModuleBase
{
	private bool _initialized;

	protected override void OnSubModuleLoad()
	{
		((MBSubModuleBase)this).OnSubModuleLoad();
		Module.CurrentModule.SetEditorMissionTester((IEditorMissionTester)(object)new SandBoxEditorMissionTester());
		TauntUsageManager.Initialize();
	}

	protected override void InitializeGameStarter(Game game, IGameStarter gameStarterObject)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		if (game.GameType is Campaign)
		{
			gameStarterObject.AddModel<AgentStatCalculateModel>((MBGameModel<AgentStatCalculateModel>)(object)new SandboxAgentStatCalculateModel());
			gameStarterObject.AddModel<StrikeMagnitudeCalculationModel>((MBGameModel<StrikeMagnitudeCalculationModel>)(object)new SandboxStrikeMagnitudeModel());
			gameStarterObject.AddModel<AgentApplyDamageModel>((MBGameModel<AgentApplyDamageModel>)(object)new SandboxAgentApplyDamageModel());
			gameStarterObject.AddModel<MissionDifficultyModel>((MBGameModel<MissionDifficultyModel>)(object)new SandboxMissionDifficultyModel());
			gameStarterObject.AddModel<ApplyWeatherEffectsModel>((MBGameModel<ApplyWeatherEffectsModel>)(object)new SandboxApplyWeatherEffectsModel());
			gameStarterObject.AddModel<AutoBlockModel>((MBGameModel<AutoBlockModel>)(object)new SandboxAutoBlockModel());
			gameStarterObject.AddModel<AgentDecideKilledOrUnconsciousModel>((MBGameModel<AgentDecideKilledOrUnconsciousModel>)(object)new SandboxAgentDecideKilledOrUnconsciousModel());
			gameStarterObject.AddModel<BattleBannerBearersModel>((MBGameModel<BattleBannerBearersModel>)(object)new SandboxBattleBannerBearersModel());
			gameStarterObject.AddModel<FormationArrangementModel>((MBGameModel<FormationArrangementModel>)new DefaultFormationArrangementModel());
			gameStarterObject.AddModel<BattleMoraleModel>((MBGameModel<BattleMoraleModel>)(object)new SandboxBattleMoraleModel());
			gameStarterObject.AddModel<BattleInitializationModel>((MBGameModel<BattleInitializationModel>)(object)new SandboxBattleInitializationModel());
			gameStarterObject.AddModel<BattleSpawnModel>((MBGameModel<BattleSpawnModel>)(object)new SandboxBattleSpawnModel());
			gameStarterObject.AddModel<DamageParticleModel>((MBGameModel<DamageParticleModel>)new DefaultDamageParticleModel());
			gameStarterObject.AddModel<ItemPickupModel>((MBGameModel<ItemPickupModel>)new DefaultItemPickupModel());
			gameStarterObject.AddModel<MissionSiegeEngineCalculationModel>((MBGameModel<MissionSiegeEngineCalculationModel>)new DefaultSiegeEngineCalculationModel());
			CampaignGameStarter val = (CampaignGameStarter)(object)((gameStarterObject is CampaignGameStarter) ? gameStarterObject : null);
			if (val != null)
			{
				val.AddBehavior((CampaignBehaviorBase)(object)new HideoutConversationsCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new AlleyCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new CommonTownsfolkCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new CompanionDismissCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new DefaultNotificationsCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new ClanMemberRolesCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new PrisonBreakCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new GuardsCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new SettlementMusiciansCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new BoardGameCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new TradersCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new ArenaMasterCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new CommonVillagersCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new HeirSelectionCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new DefaultCutscenesCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new RivalGangMovingInIssueBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new RuralNotableInnAndOutIssueBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new FamilyFeudIssueBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new NotableWantsDaughterFoundIssueBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new TheSpyPartyIssueQuestBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new ProdigalSonIssueBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new BarberCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new SnareTheWealthyIssueBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new RetirementCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new StatisticsCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new DumpIntegrityCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new CheckpointCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new StealthCharactersCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new TavernEmployeesCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new TownMerchantsCampaignBehavior());
				val.AddBehavior((CampaignBehaviorBase)(object)new RecruitmentAgentSpawnBehavior());
			}
		}
	}

	public override void OnCampaignStart(Game game, object starterObject)
	{
		GameType gameType = game.GameType;
		Campaign val = (Campaign)(object)((gameType is Campaign) ? gameType : null);
		if (val != null)
		{
			SandBoxManager sandBoxManager = val.SandBoxManager;
			sandBoxManager.SandBoxMissionManager = (ISandBoxMissionManager)(object)new SandBoxMissionManager();
			sandBoxManager.AgentBehaviorManager = (IAgentBehaviorManager)(object)new AgentBehaviorManager();
			sandBoxManager.SandBoxSaveManager = (ISaveManager)(object)new SandBoxSaveManager();
		}
	}

	private void OnRegisterTypes()
	{
		MBObjectManager.Instance.RegisterType<InstrumentData>("MusicInstrument", "MusicInstruments", 54u, true, false);
		MBObjectManager.Instance.RegisterType<SettlementMusicData>("MusicTrack", "MusicTracks", 55u, true, false);
		new DefaultMusicInstrumentData();
		MBObjectManagerExtensions.LoadXML(MBObjectManager.Instance, "MusicInstruments", false);
		MBObjectManagerExtensions.LoadXML(MBObjectManager.Instance, "MusicTracks", false);
	}

	public override void OnGameInitializationFinished(Game game)
	{
		GameType gameType = game.GameType;
		Campaign val = (Campaign)(object)((gameType is Campaign) ? gameType : null);
		if (val != null)
		{
			val.CampaignMissionManager = (ICampaignMissionManager)(object)new CampaignMissionManager();
			val.MapSceneCreator = (IMapSceneCreator)(object)new MapSceneCreator();
			val.EncyclopediaManager.CreateEncyclopediaPages();
			OnRegisterTypes();
		}
	}

	public override void RegisterSubModuleObjects(bool isSavedCampaign)
	{
		Campaign.Current.SandBoxManager.InitializeSandboxXMLs(isSavedCampaign);
	}

	public override void AfterRegisterSubModuleObjects(bool isSavedCampaign)
	{
		Campaign.Current.SandBoxManager.InitializeCharactersAfterLoad(isSavedCampaign);
	}

	private void StartGame(LoadResult loadResult)
	{
		MBGameManager.StartNewGame((MBGameManager)(object)new SandBoxGameManager(loadResult));
		MouseManager.ShowCursor(false);
	}

	public override void OnGameLoaded(Game game, object starterObject)
	{
		GameType gameType = game.GameType;
		Campaign val = (Campaign)(object)((gameType is Campaign) ? gameType : null);
		if (val != null)
		{
			SandBoxManager sandBoxManager = val.SandBoxManager;
			sandBoxManager.SandBoxMissionManager = (ISandBoxMissionManager)(object)new SandBoxMissionManager();
			sandBoxManager.AgentBehaviorManager = (IAgentBehaviorManager)(object)new AgentBehaviorManager();
			sandBoxManager.SandBoxSaveManager = (ISaveManager)(object)new SandBoxSaveManager();
		}
	}

	protected override void OnBeforeInitialModuleScreenSetAsRoot()
	{
		((MBSubModuleBase)this).OnBeforeInitialModuleScreenSetAsRoot();
		if (!_initialized)
		{
			MBSaveLoad.Initialize(Module.CurrentModule.GlobalTextManager);
			_initialized = true;
		}
	}

	public override void OnConfigChanged()
	{
		if (Campaign.Current != null)
		{
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnConfigChanged();
		}
	}

	protected override void OnNewModuleLoad()
	{
		SaveManager.InitializeGlobalDefinitionContext();
	}
}
