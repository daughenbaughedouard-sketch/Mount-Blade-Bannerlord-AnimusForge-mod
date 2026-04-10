using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandBox;

public class CampaignMissionManager : ICampaignMissionManager
{
	IMission ICampaignMissionManager.OpenSiegeMissionWithDeployment(string scene, float[] wallHitPointsPercentages, bool hasAnySiegeTower, List<MissionSiegeWeapon> siegeWeaponsOfAttackers, List<MissionSiegeWeapon> siegeWeaponsOfDefenders, bool isPlayerAttacker, int upgradeLevel, bool isSallyOut, bool isReliefForceAttack)
	{
		return (IMission)(object)SandBoxMissions.OpenSiegeMissionWithDeployment(scene, wallHitPointsPercentages, hasAnySiegeTower, siegeWeaponsOfAttackers, siegeWeaponsOfDefenders, isPlayerAttacker, upgradeLevel, isSallyOut, isReliefForceAttack);
	}

	IMission ICampaignMissionManager.OpenSiegeMissionNoDeployment(string scene, bool isSallyOut, bool isReliefForceAttack)
	{
		return (IMission)(object)SandBoxMissions.OpenSiegeMissionNoDeployment(scene, isSallyOut, isReliefForceAttack);
	}

	IMission ICampaignMissionManager.OpenSiegeLordsHallFightMission(string scene, FlattenedTroopRoster attackerPriorityList)
	{
		return (IMission)(object)SandBoxMissions.OpenSiegeLordsHallFightMission(scene, attackerPriorityList);
	}

	IMission ICampaignMissionManager.OpenBattleMission(MissionInitializerRecord rec)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return (IMission)(object)SandBoxMissions.OpenBattleMission(rec);
	}

	IMission ICampaignMissionManager.OpenCaravanBattleMission(MissionInitializerRecord rec, bool isCaravan)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return (IMission)(object)SandBoxMissions.OpenCaravanBattleMission(rec, isCaravan);
	}

	IMission ICampaignMissionManager.OpenBattleMission(string scene, bool usesTownDecalAtlas)
	{
		return (IMission)(object)SandBoxMissions.OpenBattleMission(scene, usesTownDecalAtlas);
	}

	IMission ICampaignMissionManager.OpenAlleyFightMission(string scene, int upgradeLevel, Location location, TroopRoster playerSideTroops, TroopRoster rivalSideTroops)
	{
		return (IMission)(object)SandBoxMissions.OpenAlleyFightMission(scene, upgradeLevel, location, playerSideTroops, rivalSideTroops);
	}

	IMission ICampaignMissionManager.OpenCombatMissionWithDialogue(string scene, CharacterObject characterToTalkTo, int upgradeLevel)
	{
		return (IMission)(object)SandBoxMissions.OpenCombatMissionWithDialogue(scene, characterToTalkTo, upgradeLevel);
	}

	IMission ICampaignMissionManager.OpenBattleMissionWhileEnteringSettlement(string scene, int upgradeLevel, int numberOfMaxTroopToBeSpawnedForPlayer, int numberOfMaxTroopToBeSpawnedForOpponent)
	{
		return (IMission)(object)SandBoxMissions.OpenBattleMissionWhileEnteringSettlement(scene, upgradeLevel, numberOfMaxTroopToBeSpawnedForPlayer, numberOfMaxTroopToBeSpawnedForOpponent);
	}

	IMission ICampaignMissionManager.OpenHideoutBattleMission(string scene, FlattenedTroopRoster playerTroops, bool isTutorial)
	{
		return (IMission)(object)SandBoxMissions.OpenHideoutBattleMission(scene, playerTroops, isTutorial);
	}

	IMission ICampaignMissionManager.OpenTownCenterMission(string scene, int townUpgradeLevel, Location location, CharacterObject talkToChar, string playerSpawnTag)
	{
		return (IMission)(object)SandBoxMissions.OpenTownCenterMission(scene, townUpgradeLevel, location, talkToChar, playerSpawnTag);
	}

	IMission ICampaignMissionManager.OpenCastleCourtyardMission(string scene, int castleUpgradeLevel, Location location, CharacterObject talkToChar)
	{
		return (IMission)(object)SandBoxMissions.OpenCastleCourtyardMission(scene, castleUpgradeLevel, location, talkToChar);
	}

	IMission ICampaignMissionManager.OpenVillageMission(string scene, Location location, CharacterObject talkToChar)
	{
		return (IMission)(object)SandBoxMissions.OpenVillageMission(scene, location, talkToChar);
	}

	IMission ICampaignMissionManager.OpenIndoorMission(string scene, int upgradeLevel, Location location, CharacterObject talkToChar)
	{
		return (IMission)(object)SandBoxMissions.OpenIndoorMission(scene, upgradeLevel, location, talkToChar);
	}

	IMission ICampaignMissionManager.OpenPrisonBreakMission(string scene, Location location, CharacterObject prisonerCharacter)
	{
		return (IMission)(object)SandBoxMissions.OpenPrisonBreakMission(scene, location, prisonerCharacter);
	}

	IMission ICampaignMissionManager.OpenArenaStartMission(string scene, Location location, CharacterObject talkToChar)
	{
		return (IMission)(object)SandBoxMissions.OpenArenaStartMission(scene, location, talkToChar);
	}

	public IMission OpenArenaDuelMission(string scene, Location location, CharacterObject duelCharacter, bool requireCivilianEquipment, bool spawnBOthSidesWithHorse, Action<CharacterObject> onDuelEndAction, float customAgentHealth)
	{
		return (IMission)(object)SandBoxMissions.OpenArenaDuelMission(scene, location, duelCharacter, requireCivilianEquipment, spawnBOthSidesWithHorse, onDuelEndAction, customAgentHealth);
	}

	IMission ICampaignMissionManager.OpenConversationMission(ConversationCharacterData playerCharacterData, ConversationCharacterData conversationPartnerData, string specialScene, string sceneLevels, bool isMultiAgentConversation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return (IMission)(object)SandBoxMissions.OpenConversationMission(playerCharacterData, conversationPartnerData, specialScene, sceneLevels, isMultiAgentConversation);
	}

	IMission ICampaignMissionManager.OpenMeetingMission(string scene, CharacterObject character)
	{
		return (IMission)(object)SandBoxMissions.OpenMeetingMission(scene, character);
	}

	IMission ICampaignMissionManager.OpenRetirementMission(string scene, Location location, CharacterObject talkToChar, string sceneLevels, string unconsciousMenuId)
	{
		return (IMission)(object)SandBoxMissions.OpenRetirementMission(scene, location, talkToChar, sceneLevels, unconsciousMenuId);
	}

	IMission ICampaignMissionManager.OpenHideoutAmbushMission(string sceneName, FlattenedTroopRoster playerTroops, Location location)
	{
		return (IMission)(object)SandBoxMissions.OpenHideoutAmbushMission(sceneName, playerTroops, location);
	}

	public IMission OpenDisguiseMission(string scene, bool willSetUpContact, string sceneLevels, Location fromLocation)
	{
		return (IMission)(object)SandBoxMissions.OpenDisguiseMission(scene, willSetUpContact, fromLocation, sceneLevels);
	}

	public IMission OpenNavalBattleMission(MissionInitializerRecord rec)
	{
		return null;
	}

	public IMission OpenNavalSetPieceBattleMission(MissionInitializerRecord rec, MBList<IShipOrigin> playerShips, MBList<IShipOrigin> playerAllyShips, MBList<IShipOrigin> enemyShips)
	{
		return null;
	}
}
