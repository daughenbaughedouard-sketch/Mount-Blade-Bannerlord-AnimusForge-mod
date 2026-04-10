using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.MissionSpawnHandlers;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Multiplayer.Missions;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer;

[MissionManager]
public static class MultiplayerPracticeMissions
{
	[MissionMethod]
	public static Mission OpenMultiplayerPracticeMission(string scene, BasicCharacterObject playerCharacter, CustomBattleCombatant playerParty, CustomBattleCombatant enemyParty, bool isPlayerGeneral, BasicCharacterObject playerSideGeneralCharacter, string sceneLevels = "", string seasonString = "", float timeOfDay = 6f)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		BattleSideEnum playerSide = playerParty.Side;
		bool isPlayerAttacker = (int)playerSide == 1;
		IMissionTroopSupplier[] troopSuppliers = (IMissionTroopSupplier[])(object)new IMissionTroopSupplier[2];
		CustomBattleTroopSupplier val = new CustomBattleTroopSupplier(playerParty, true, isPlayerGeneral, false, (Func<BasicCharacterObject, bool>)null);
		troopSuppliers[playerParty.Side] = (IMissionTroopSupplier)(object)val;
		CustomBattleTroopSupplier val2 = new CustomBattleTroopSupplier(enemyParty, false, false, false, (Func<BasicCharacterObject, bool>)null);
		troopSuppliers[enemyParty.Side] = (IMissionTroopSupplier)(object)val2;
		bool isPlayerSergeant = !isPlayerGeneral;
		MissionInitializerRecord val3 = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref val3))._002Ector(scene);
		val3.DoNotUseLoadingScreen = false;
		val3.PlayingInCampaignMode = false;
		val3.AtmosphereOnCampaign = BannerlordMissions.CreateAtmosphereInfoForMission(seasonString, (int)timeOfDay);
		val3.SceneLevels = sceneLevels;
		val3.DecalAtlasGroup = 2;
		return MissionState.OpenNew("MultiplayerPractice", val3, (InitializeMissionBehaviorsDelegate)((Mission missionController) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[26]
		{
			(MissionBehavior)new MissionAgentSpawnLogic(troopSuppliers, playerSide, (BattleSizeType)0),
			(MissionBehavior)new BattlePowerCalculationLogic(),
			(MissionBehavior)new CustomBattleAgentLogic(),
			(MissionBehavior)new BannerBearerLogic(),
			(MissionBehavior)new CustomBattleMissionSpawnHandler((!isPlayerAttacker) ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty),
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new BattleEndLogic(),
			(MissionBehavior)new BattleReinforcementsSpawnController(),
			(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)null, (IBattleCombatant)(object)playerParty, (IBattleCombatant)(object)((!isPlayerAttacker) ? playerParty : enemyParty), (IBattleCombatant)(object)(isPlayerAttacker ? playerParty : enemyParty), (MissionTeamAITypeEnum)1, isPlayerSergeant),
			(MissionBehavior)new BattleObserverMissionLogic(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new AgentVictoryLogic(),
			(MissionBehavior)new MissionAgentPanicHandler(),
			(MissionBehavior)new BattleMissionAgentInteractionLogic(),
			(MissionBehavior)new AgentMoraleInteractionLogic(),
			(MissionBehavior)new AssignPlayerRoleInTeamMissionController(isPlayerGeneral, isPlayerSergeant, false, isPlayerSergeant ? Enumerable.Repeat(((MBObjectBase)playerCharacter).StringId, 1).ToList() : new List<string>()),
			(MissionBehavior)new GeneralsAndCaptainsAssignmentLogic((isPlayerAttacker && isPlayerGeneral) ? ((MBObjectBase)playerCharacter).GetName() : ((isPlayerAttacker && isPlayerSergeant) ? ((MBObjectBase)playerSideGeneralCharacter).GetName() : null), (!isPlayerAttacker && isPlayerGeneral) ? ((MBObjectBase)playerCharacter).GetName() : ((!isPlayerAttacker && isPlayerSergeant) ? ((MBObjectBase)playerSideGeneralCharacter).GetName() : null), (TextObject)null, (TextObject)null, true),
			(MissionBehavior)new EquipmentControllerLeaveLogic(),
			(MissionBehavior)new MissionHardBorderPlacer(),
			(MissionBehavior)new MissionBoundaryPlacer(),
			(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
			(MissionBehavior)new HighlightsController(),
			(MissionBehavior)new BattleHighlightsController(),
			(MissionBehavior)new BattleDeploymentMissionController(isPlayerAttacker),
			(MissionBehavior)new BattleDeploymentHandler(isPlayerAttacker),
			(MissionBehavior)new MultiplayerPracticeMissionComponent()
		}), true, true);
	}
}
