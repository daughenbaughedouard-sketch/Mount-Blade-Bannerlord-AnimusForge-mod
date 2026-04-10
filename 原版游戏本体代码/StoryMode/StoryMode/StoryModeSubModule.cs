using System;
using System.Runtime.CompilerServices;
using StoryMode.GameComponents;
using StoryMode.GameComponents.CampaignBehaviors;
using StoryMode.Quests.PlayerClanQuests;
using StoryMode.Quests.SecondPhase;
using StoryMode.Quests.ThirdPhase;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace StoryMode;

public class StoryModeSubModule : MBSubModuleBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConditionDelegate _003C_003E9__2_0;

		internal bool _003CAddGameMenus_003Eb__2_0(MenuCallbackArgs args)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			args.optionLeaveType = (LeaveType)17;
			return true;
		}
	}

	protected override void InitializeGameStarter(Game game, IGameStarter gameStarterObject)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (game.GameType is CampaignStoryMode campaignStoryMode)
		{
			CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarterObject;
			((Campaign)campaignStoryMode).AddCampaignEventReceiver((CampaignEventReceiver)(object)StoryModeEvents.Instance);
			AddGameMenus(campaignGameStarter);
			AddModels(campaignGameStarter);
			AddBehaviors(campaignGameStarter);
		}
	}

	public override void OnGameEnd(Game game)
	{
		((MBSubModuleBase)this).OnGameEnd(game);
		if (game.GameType is CampaignStoryMode && StoryModeManager.Current != null)
		{
			StoryModeManager.Current.Destroy();
		}
	}

	private void AddGameMenus(CampaignGameStarter campaignGameStarter)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		campaignGameStarter.AddGameMenu("menu_story_mode_welcome", "{=GGfM1HKn}Welcome to MBII Bannerlord", (OnInitDelegate)null, (MenuOverlayType)0, (MenuFlags)0, (object)null);
		object obj = _003C_003Ec._003C_003E9__2_0;
		if (obj == null)
		{
			OnConditionDelegate val = delegate(MenuCallbackArgs args)
			{
				//IL_0003: Unknown result type (might be due to invalid IL or missing references)
				args.optionLeaveType = (LeaveType)17;
				return true;
			};
			_003C_003Ec._003C_003E9__2_0 = val;
			obj = (object)val;
		}
		campaignGameStarter.AddGameMenuOption("menu_story_mode_welcome", "mno_continue", "{=str_continue}Continue...", (OnConditionDelegate)obj, (OnConsequenceDelegate)null, false, -1, false, (object)null);
	}

	private void AddBehaviors(CampaignGameStarter campaignGameStarter)
	{
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new LordConversationsStoryModeBehavior());
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new MainStorylineCampaignBehavior());
		if (!StoryModeManager.Current.MainStoryLine.IsCompleted)
		{
			if (!StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted)
			{
				campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new TutorialPhaseCampaignBehavior());
			}
			if (!StoryModeManager.Current.MainStoryLine.IsFirstPhaseCompleted)
			{
				campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new FirstPhaseCampaignBehavior());
			}
			if (!StoryModeManager.Current.MainStoryLine.IsSecondPhaseCompleted)
			{
				campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new SecondPhaseCampaignBehavior());
			}
			campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new ThirdPhaseCampaignBehavior());
		}
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new TrainingFieldCampaignBehavior());
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new StoryModeTutorialBoxCampaignBehavior());
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new StoryModeCharacterCreationCampaignBehavior());
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new StoryModeBanditSpawnCampaignBehavior());
		Debug.Print("campaignGameStarter.AddBehavior(AchievementsCampaignBehavior)", 0, (DebugColor)12, 17592186044416uL);
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new AchievementsCampaignBehavior());
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new WeakenEmpireQuestBehavior());
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new AssembleEmpireQuestBehavior());
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new DefeatTheConspiracyQuestBehavior());
		campaignGameStarter.AddBehavior((CampaignBehaviorBase)(object)new RescueFamilyQuestBehavior());
	}

	private void AddModels(CampaignGameStarter campaignGameStarter)
	{
		campaignGameStarter.AddModel<BanditDensityModel>((MBGameModel<BanditDensityModel>)(object)new StoryModeBanditDensityModel());
		campaignGameStarter.AddModel<EncounterGameMenuModel>((MBGameModel<EncounterGameMenuModel>)(object)new StoryModeEncounterGameMenuModel());
		campaignGameStarter.AddModel<BattleRewardModel>((MBGameModel<BattleRewardModel>)(object)new StoryModeBattleRewardModel());
		campaignGameStarter.AddModel<TargetScoreCalculatingModel>((MBGameModel<TargetScoreCalculatingModel>)(object)new StoryModeTargetScoreCalculatingModel());
		campaignGameStarter.AddModel<PartyWageModel>((MBGameModel<PartyWageModel>)(object)new StoryModePartyWageModel());
		campaignGameStarter.AddModel<KingdomDecisionPermissionModel>((MBGameModel<KingdomDecisionPermissionModel>)(object)new StoryModeKingdomDecisionPermissionModel());
		campaignGameStarter.AddModel<CombatXpModel>((MBGameModel<CombatXpModel>)(object)new StoryModeCombatXpModel());
		campaignGameStarter.AddModel<GenericXpModel>((MBGameModel<GenericXpModel>)(object)new StoryModeGenericXpModel());
		campaignGameStarter.AddModel<NotableSpawnModel>((MBGameModel<NotableSpawnModel>)(object)new StoryModeNotableSpawnModel());
		campaignGameStarter.AddModel<HeroDeathProbabilityCalculationModel>((MBGameModel<HeroDeathProbabilityCalculationModel>)(object)new StoryModeHeroDeathProbabilityCalculationModel());
		campaignGameStarter.AddModel<AgentDecideKilledOrUnconsciousModel>((MBGameModel<AgentDecideKilledOrUnconsciousModel>)(object)new StoryModeAgentDecideKilledOrUnconsciousModel());
		campaignGameStarter.AddModel<PartySizeLimitModel>((MBGameModel<PartySizeLimitModel>)(object)new StoryModePartySizeLimitModel());
		campaignGameStarter.AddModel<BannerItemModel>((MBGameModel<BannerItemModel>)(object)new StoryModeBannerItemModel());
		campaignGameStarter.AddModel<PrisonerRecruitmentCalculationModel>((MBGameModel<PrisonerRecruitmentCalculationModel>)(object)new StoryModePrisonerRecruitmentCalculationModel());
		campaignGameStarter.AddModel<TroopSupplierProbabilityModel>((MBGameModel<TroopSupplierProbabilityModel>)(object)new StoryModeTroopSupplierProbabilityModel());
		campaignGameStarter.AddModel<CutsceneSelectionModel>((MBGameModel<CutsceneSelectionModel>)(object)new StoryModeCutsceneSelectionModel());
		campaignGameStarter.AddModel<VoiceOverModel>((MBGameModel<VoiceOverModel>)(object)new StoryModeVoiceOverModel());
		campaignGameStarter.AddModel<IncidentModel>((MBGameModel<IncidentModel>)(object)new StoryModeIncidentModel());
	}
}
