using System;
using System.Collections.Generic;
using NavalDLC.Missions;
using NavalDLC.Missions.Deployment;
using NavalDLC.Missions.Handlers;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace NavalDLC.CustomBattle;

[MissionManager]
public static class CustomNavalMissions
{
	public static AtmosphereInfo CreateAtmosphereInfoForMission(string seasonId, int timeOfDay, float windStrength, Vec2 windDirection, TerrainType terrain)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Invalid comparison between Unknown and I4
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Invalid comparison between Unknown and I4
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Invalid comparison between Unknown and I4
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Invalid comparison between Unknown and I4
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		dictionary.Add("spring", 0);
		dictionary.Add("summer", 1);
		dictionary.Add("fall", 2);
		dictionary.Add("winter", 3);
		dictionary.TryGetValue(seasonId, out var value);
		if (!((Vec2)(ref windDirection)).IsNonZero())
		{
			windDirection = Vec2.Side;
		}
		return new AtmosphereInfo
		{
			TimeInfo = new TimeInformation
			{
				Season = value,
				TimeOfDay = timeOfDay
			},
			NauticalInfo = new NauticalInformation
			{
				WindVector = windStrength * ((Vec2)(ref windDirection)).Normalized(),
				CanUseLowAltitudeAtmosphere = 1,
				IsRiverBattle = (((int)terrain == 11) ? 1 : 0),
				UsesNavalSimulatedWater = (((int)terrain == 11 || (int)terrain == 10 || (int)terrain == 19 || (int)terrain == 18) ? 1 : 0)
			}
		};
	}

	[MissionMethod]
	public static Mission OpenNavalBattleForCustomMission(string scene, BasicCharacterObject playerCharacter, CustomBattleCombatant playerParty, MBList<IShipOrigin> playerTeamShips, CustomBattleCombatant enemyParty, MBList<IShipOrigin> enemyTeamShips, bool isPlayerGeneral, string seasonString, float timeOfDay, float windStrength, NavalCustomBattleWindConfig.Direction windDirection, TerrainType terrain)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected I4, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		BattleSideEnum playerSide = playerParty.Side;
		bool isPlayerAttacker = (int)playerSide == 1;
		IMissionTroopSupplier[] troopSuppliers = (IMissionTroopSupplier[])(object)new IMissionTroopSupplier[2];
		CustomBattleTroopSupplier val = new CustomBattleTroopSupplier(playerParty, true, isPlayerGeneral, false, (Func<BasicCharacterObject, bool>)null);
		troopSuppliers[playerParty.Side] = (IMissionTroopSupplier)(object)val;
		CustomBattleTroopSupplier val2 = new CustomBattleTroopSupplier(enemyParty, false, false, false, (Func<BasicCharacterObject, bool>)null);
		troopSuppliers[enemyParty.Side] = (IMissionTroopSupplier)(object)val2;
		bool isPlayerSergeant = !isPlayerGeneral;
		MissionInitializerRecord rec = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref rec))._002Ector(scene);
		TerrainType terrainType = terrain;
		rec.TerrainType = (int)terrainType;
		rec.NeedsRandomTerrain = false;
		rec.PlayingInCampaignMode = false;
		rec.AtmosphereOnCampaign = CreateAtmosphereInfoForMission(seasonString, (int)timeOfDay, windStrength, new Vec2(0f, 1f), terrain);
		rec.SceneHasMapPatch = false;
		rec.PlayingInCampaignMode = true;
		rec.DecalAtlasGroup = 2;
		rec.AtmosphereOnCampaign.NauticalInfo.UsesNavalSimulatedWater = 1;
		int[] maxDeployableTroopCountPerTeam = NavalDLCManager.Instance.GameModels.ShipDeploymentModel.GetMaximumDeployableTroopCountPerTeam(playerTeamShips, null, enemyTeamShips);
		int deployablePlayerShipCount = MathF.Min(((List<IShipOrigin>)(object)playerTeamShips).Count, NavalShipDeploymentLimit.Max().NetDeploymentLimit);
		Mission obj = NavalMissionState.OpenNew("NavalCustomBattle", rec, (InitializeMissionBehaviorsDelegate)((Mission mission) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[31]
		{
			(MissionBehavior)new NavalShipsLogic(),
			(MissionBehavior)new NavalFloatsamLogic(),
			(MissionBehavior)new NavalAgentsLogic(),
			(MissionBehavior)new DefaultNavalMissionLogic(playerTeamShips, null, enemyTeamShips, NavalShipDeploymentLimit.Max(), NavalShipDeploymentLimit.Invalid(), NavalShipDeploymentLimit.Max()),
			(MissionBehavior)new NavalTrajectoryPlanningLogic(),
			(MissionBehavior)new ShipAgentSpawnLogic(troopSuppliers, playerSide, deployablePlayerShipCount, maxDeployableTroopCountPerTeam),
			(MissionBehavior)new NavalMissionDeploymentPlanningLogic(mission),
			(MissionBehavior)new BattlePowerCalculationLogic(),
			(MissionBehavior)new CustomBattleAgentLogic(),
			(MissionBehavior)new WaveParametersComputerLogic(),
			(MissionBehavior)new MissionOptionsComponent(),
			(MissionBehavior)new NavalAgentMoraleInteractionLogic(),
			(MissionBehavior)new NavalBattleEndLogic(),
			(MissionBehavior)new NavalBoundaryForceFieldLogic(),
			(MissionBehavior)new NavalMissionCombatantsLogic((IEnumerable<IBattleCombatant>)new List<CustomBattleCombatant> { playerParty, enemyParty }, (IBattleCombatant)(object)playerParty, (IBattleCombatant)(object)((!isPlayerAttacker) ? playerParty : enemyParty), (IBattleCombatant)(object)(isPlayerAttacker ? playerParty : enemyParty), (MissionTeamAITypeEnum)4, isPlayerSergeant),
			(MissionBehavior)new BattleObserverMissionLogic(),
			(MissionBehavior)new AgentHumanAILogic(),
			(MissionBehavior)new AgentVictoryLogic(),
			(MissionBehavior)new ShipCollisionOutcomeLogic(mission),
			(MissionBehavior)new ShipRetreatLogic(),
			(MissionBehavior)new BattleMissionAgentInteractionLogic(),
			(MissionBehavior)new NavalAssignPlayerRoleInTeamMissionController(!isPlayerSergeant, isPlayerSergeant, isPlayerInArmy: false),
			(MissionBehavior)new EquipmentControllerLeaveLogic(),
			(MissionBehavior)new MissionHardBorderPlacer(),
			(MissionBehavior)new MissionBoundaryPlacer(),
			(MissionBehavior)new MissionBoundaryCrossingHandler(30f),
			(MissionBehavior)new HighlightsController(),
			(MissionBehavior)new BattleHighlightsController(),
			(MissionBehavior)new NavalDeploymentMissionController(isPlayerAttacker),
			(MissionBehavior)new NavalDeploymentHandler(isPlayerAttacker),
			(MissionBehavior)new NavalCustomBattleWindAndWaveLogic(windDirection, terrainType)
		}));
		obj.SetPlayerCanTakeControlOfAnotherAgentWhenDead();
		return obj;
	}
}
