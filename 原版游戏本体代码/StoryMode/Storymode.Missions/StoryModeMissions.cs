using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SandBox;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions;
using SandBox.Missions.MissionEvents;
using SandBox.Missions.MissionLogics;
using Storymode.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;

namespace StoryMode.Missions;

[MissionManager]
public static class StoryModeMissions
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static InitializeMissionBehaviorsDelegate _003C_003E9__1_0;

		internal IEnumerable<MissionBehavior> _003COpenSneakIntoTheVillaMission_003Eb__1_0(Mission mission)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Expected O, but got Unknown
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Expected O, but got Unknown
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Expected O, but got Unknown
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Expected O, but got Unknown
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Expected O, but got Unknown
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Expected O, but got Unknown
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Expected O, but got Unknown
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Expected O, but got Unknown
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Expected O, but got Unknown
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[23]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new MissionBasicTeamLogic(),
				(MissionBehavior)new StealthPatrolPointMissionLogic(),
				(MissionBehavior)new MissionAgentHandler(),
				(MissionBehavior)new SneakIntoTheVillaMissionController(),
				(MissionBehavior)new MissionConversationLogic(),
				(MissionBehavior)new BattleAgentLogic(),
				(MissionBehavior)new MountAgentLogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new BattleSurgeonLogic(),
				(MissionBehavior)new StealthFailCounterMissionLogic(),
				(MissionBehavior)new MissionAIActivationDeactivationEventListenerLogic(),
				(MissionBehavior)new CorpseDraggingMissionLogic(),
				(MissionBehavior)new ShowQuickInformationEventListenerLogic()
			};
		}
	}

	[MissionMethod]
	public static Mission OpenTrainingFieldMission(string scene, Location location, CharacterObject talkToChar = null, string sceneLevels = null)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		return MissionState.OpenNew("TrainingField", SandBoxMissions.CreateSandBoxTrainingMissionInitializerRecord(scene, sceneLevels, false), (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[23]
		{
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new CampaignMissionComponent(),
			(MissionBehavior)new MissionBasicTeamLogic(),
			(MissionBehavior)new TrainingFieldMissionController(),
			(MissionBehavior)new BasicLeaveMissionLogic(),
			(MissionBehavior)new LeaveMissionLogic("settlement_player_unconscious"),
			(MissionBehavior)new MissionAgentLookHandler(),
			(MissionBehavior)new SandBoxMissionHandler(),
			(MissionBehavior)new MissionConversationLogic(talkToChar),
			(MissionBehavior)new MissionFightHandler(),
			(MissionBehavior)new MissionAgentHandler(),
			(MissionBehavior)new MissionAlleyHandler(),
			(MissionBehavior)new HeroSkillHandler(),
			(MissionBehavior)new MissionFacialAnimationHandler(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new BattleAgentLogic(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new MissionCrimeHandler(),
			(MissionBehavior)new MissionHardBorderPlacer(),
			(MissionBehavior)new MissionBoundaryPlacer(),
			(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
			(MissionBehavior)new VisualTrackerMissionBehavior(),
			(MissionBehavior)new EquipmentControllerLeaveLogic()
		}), true, true);
	}

	[MissionMethod]
	public static Mission OpenSneakIntoTheVillaMission(string scene, CampaignTime overridenCt, string sceneLevels = null)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		MissionInitializerRecord val = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref val))._002Ector(scene);
		val.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		val.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		val.PlayingInCampaignMode = (int)Campaign.Current.GameMode == 1;
		val.AtmosphereOnCampaign = (((int)Campaign.Current.GameMode == 1) ? Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position) : AtmosphereInfo.GetInvalidAtmosphereInfo());
		val.TerrainType = ((Campaign.Current.MapSceneWrapper != null) ? ((int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace)) : 0);
		val.SceneLevels = sceneLevels;
		val.DoNotUseLoadingScreen = false;
		val.DisableCorpseFadeOut = true;
		val.DecalAtlasGroup = 3;
		MissionInitializerRecord val2 = val;
		object obj = _003C_003Ec._003C_003E9__1_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val3 = (Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[23]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new CampaignMissionComponent(),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new MissionBasicTeamLogic(),
				(MissionBehavior)new StealthPatrolPointMissionLogic(),
				(MissionBehavior)new MissionAgentHandler(),
				(MissionBehavior)new SneakIntoTheVillaMissionController(),
				(MissionBehavior)new MissionConversationLogic(),
				(MissionBehavior)new BattleAgentLogic(),
				(MissionBehavior)new MountAgentLogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionAgentPanicHandler(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new HighlightsController(),
				(MissionBehavior)new BattleHighlightsController(),
				(MissionBehavior)new EquipmentControllerLeaveLogic(),
				(MissionBehavior)new BattleSurgeonLogic(),
				(MissionBehavior)new StealthFailCounterMissionLogic(),
				(MissionBehavior)new MissionAIActivationDeactivationEventListenerLogic(),
				(MissionBehavior)new CorpseDraggingMissionLogic(),
				(MissionBehavior)new ShowQuickInformationEventListenerLogic()
			};
			_003C_003Ec._003C_003E9__1_0 = val3;
			obj = (object)val3;
		}
		return MissionState.OpenNew("SneakIntoTheVillaMission", val2, (InitializeMissionBehaviorsDelegate)obj, true, true);
	}
}
