using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.MissionObjects;
using NavalDLC.Missions;
using NavalDLC.Missions.AI.Tactics;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipControl;
using NavalDLC.Missions.ShipInput;
using NavalDLC.Storyline.Objectives.Quest5;
using SandBox;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.AgentBehaviors;
using SandBox.Objects;
using SandBox.Objects.Usables;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.Missions.Objectives;
using TaleWorlds.MountAndBlade.Objects.Usables;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.MissionControllers;

public class Quest5SetPieceBattleMissionController : MissionLogic, IMissionAgentSpawnLogic, IMissionBehavior
{
	public class ConversationSound
	{
		public TextObject Line;

		public NotificationPriority Priority;

		public CharacterObject Character;

		public ConversationSound(TextObject line, NotificationPriority priority, CharacterObject character)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Line = line;
			Priority = priority;
			Character = character;
		}
	}

	public enum Quest5SetPieceBattleMissionState
	{
		None,
		InitializePhase1Part1,
		InitializePhase1Part2,
		Phase1GoToEnemyShip,
		Phase1SwimmingPhase,
		InitializeStealthPhasePart1,
		InitializeStealthPhasePart2,
		Phase1StealthPhase,
		Phase1GoToShipInteriorFadeOut,
		Phase1InitializeShipInteriorPhase,
		Phase1GoToShipInteriorFadeIn,
		Phase1ShipInteriorPhase,
		Phase1GoBackToShipFadeOut,
		Phase1InitializeGoBackToShip,
		Phase1GoBackToShipFadeIn,
		Phase1EscapePhase,
		Phase1ToPhase2FadeOut,
		InitializePhase2Part1,
		InitializePhase2Part2,
		InitializePhase2Part3,
		InitializePhase2Part4,
		Phase1ToPhase2FadeIn,
		Phase2InProgress,
		Phase2ToPhase3FadeOut,
		InitializePhase3Part1,
		InitializePhase3Part2,
		InitializePhase3Part3,
		Phase2ToPhase3FadeIn,
		Phase3InProgress,
		Phase3ToPhase4FadeOut,
		InitializePhase4Part1,
		InitializePhase4Part2,
		Phase3ToPhase4FadeIn,
		Phase4InProgress,
		Phase4ToBossFightFadeOut,
		InitializeBossFightPart1,
		InitializeBossFightPart2,
		Phase4ToBossFightFadeIn,
		StartBossFightConversation,
		BossFightConversationInProgress,
		BossFightInProgressAsDuel,
		BossFightInProgressAsAll,
		End,
		Exit
	}

	private enum Quest5InstructionState
	{
		None,
		Approach,
		WaitForJump,
		Jump,
		WaitForSwim,
		Swim,
		WaitForClearGuards,
		ClearGuards,
		WaitForCheckInterior,
		CheckInterior,
		WaitForTalkSister,
		TalkSister,
		WaitForReturnToDeck,
		ReturnToDeck,
		WaitForCutLoose,
		CutLoose,
		WaitForGunnarUsesShip,
		GunnarUsesShip,
		WaitForEscapeQuietly,
		EscapeQuietly,
		WaitForReachAllies,
		ReachAllies,
		WaitForDefeatEnemies,
		DefeatEnemies,
		WaitForDefeatPurigsShip,
		DefeatPurigsShip,
		WaitForDefeatPurig,
		DefeatPurig,
		WaitForEnd,
		End
	}

	private enum GunnarMovementState
	{
		None,
		GoToInitialJumpingPosition,
		WaitForReachingInitialJumpingPosition,
		GoToJumpingTargetPosition,
		WaitForReachingJumpingTargetPosition,
		SwimToTheHidingSpot,
		WaitForTeleportingToTheHidingSpot,
		TeleportToTargetPosition,
		WaitAtTheHidingSpot,
		GoToTheEscapeShip,
		WaitForReachingToTheEscapeShip,
		UseTheEscapeShip,
		End
	}

	private enum GunnarMovementStateForClimbingShip
	{
		None,
		Start,
		GoingToTheTargetClimbingMachine,
		TargetReached,
		UsingClimbingMachine,
		OnDeck,
		GoToFinalTargetPoint,
		End
	}

	public enum BossFightOutComeEnum
	{
		None,
		PlayerRefusedTheDuel,
		PlayerAcceptedAndWonTheDuel,
		PlayerDefeatedWaitingForConversation,
		PlayerAcceptedTheDuelLostItAndLetPurigGo,
		PlayerAcceptedTheDuelLostItAndHadPurigKilledAnyway
	}

	private enum BossFightStateEnum
	{
		None,
		Duel,
		All
	}

	private const string SceneStealthPhaseAtmosphereName = "TOD_02_00_SemiCloudy";

	private const string SceneInteriorAtmosphereName = "TOD_01_00_SemiCloudy";

	private const string ScenePhase2AtmosphereName = "TOD_naval_03_00_sunset";

	private const string ScenePhase3AtmosphereName = "TOD_naval_05_30_sunset";

	private const string MainOarPrefabName = "oars_holder";

	private const float GunnarFellIntoTheWaterTimer = 10f;

	private const string RampHolderId = "ramp_holder";

	private const string GunnarInitialJumpOffPositionTag = "gangradir_jump_off_initial";

	private const string GunnarJumpOffTargetPositionTag = "gangradir_jump_off_target";

	private const string Phase1EnemyShip4GunnarHidingSpotStringId = "sp_gangradir_hiding_spot";

	private const float MaximumAllowedReachDistanceToPhase1EnemyShip1 = 25f;

	private const float AllowedSwimRadius = 200f;

	private const float AllowedSwimRadiusCheckFrequencyAsSeconds = 5f;

	private const string Phase1CustomStealthEquipmentId = "naval_storyline_quest5_stealth_set";

	private const string Phase1ApproachPointTag = "phase_1_approach_point";

	private const float Phase1ApproachDistance = 30f;

	private const float Phase1EscapePhaseAutoCutLooseTimer = 300f;

	private const string Phase1SlaveTraderAgentCharacterStringId = "sea_hounds";

	private const string Phase1StealthAgentCharacterStringId = "sea_hound_captivity";

	private const string Phase1PlayerShipStringId = "crusas_roundship_nested_q5";

	private const string Phase1PlayerShipSpawnPointTag = "phase_1_player_ship_sp";

	private const string Phase1EnemyShip1StringId = "sturgia_heavy_ship";

	private const string Phase1EnemyShip1SpawnPointTag = "phase_1_enemy_ship_1_sp_initial";

	private const string Phase1EnemyShip1TargetPointTag = "phase_1_enemy_ship_1_sp";

	private const int Phase1EnemyShip1TroopCount = 7;

	private const string Phase1EnemyShip2StringId = "ship_lodya_storyline";

	private const string Phase1EnemyShip2SpawnPointTag = "phase_1_enemy_ship_2_sp";

	private const int Phase1EnemyShip2TroopCount = 6;

	private const string Phase1EnemyShip2AttachmentPoint1Tag = "bridge_a";

	private const string Phase1EnemyShip2AttachmentPoint2Tag = "bridge_b";

	private const string Phase1EnemyShip2AttachmentPoint3Tag = "bridge_c";

	private const string Phase1EnemyShip3StringId = "ship_dromon_storyline";

	private const string Phase1EnemyShip3SpawnPointTag = "phase_1_enemy_ship_3_sp";

	private const int Phase1EnemyShip3TroopCount = 100;

	private const string Phase1EnemyShip3AttachmentPoint1Tag = "bridge_a";

	private const string Phase1EnemyShip3AttachmentPoint2Tag = "bridge_b";

	private const string Phase1EnemyShip3ToInteriorDoorTag = "phase_1_enemy_ship_3_to_interior_door_tag";

	private const string Phase1EnemyShip4StringId = "ship_birlinn_storyline";

	private const string Phase1EnemyShip4AttachmentPoint1Tag = "bridge_d";

	private const string Phase1EnemyShip4SpawnPointTag = "phase_1_enemy_ship_4_sp";

	private const int Phase1EnemyShip4TroopCount = 6;

	private const string Phase1EnemyShip4StealthCheckpointSpawnPointStringId = "sp_player_stealth_checkpoint";

	private const string Phase1InteriorMissionPlayerSpawnPointTag = "phase_1_interior_player_sp";

	private const string Phase1InteriorMissionSisterSpawnPointTag = "phase_1_interior_sister_sp";

	private const string Phase1InteriorToEnemyShip3DoorTag = "phase_1_interior_to_enemy_ship_3_door_tag";

	private const string CrusasPhase1EquipmentStringId = "npc_merchant_equipment_empire";

	private const string EscapeShipRoofUpgradeId = "roof_5";

	private const string EscapeShipDeckUpgradeId = "deck_large_arrow_and_javelin_crates_lvl3";

	private const string SlaveTraderShipOarsmanActionId = "act_sit_2";

	private const string SisterWoundedActionId = "act_conversation_weary2_loop";

	private const string Phase1InteriorCameraSisterTag = "phase_1_interior_camera_sister";

	private const string Phase2EscapeShipPirateTargetFrame1Tag = "phase_2_anchor_1";

	private const string Phase2EscapeShipPirateTargetFrame2Tag = "phase_2_anchor_2";

	private const string Phase2EscapeShipPirateTargetFrame3Tag = "phase_2_anchor_3";

	private const string Phase2EscapeShipPirateTargetFrame4Tag = "phase_2_anchor_4";

	private const string Phase2EscapeShipPirateTargetFrame5Tag = "phase_2_anchor_5";

	private const string Phase2EnemyShip1SpawnPointTag = "phase_2_enemy_ship_1_sp";

	private const string Phase2EnemyShip2SpawnPointTag = "phase_2_enemy_ship_2_sp";

	private const string Phase2EnemyShip3SpawnPointTag = "phase_2_enemy_ship_3_sp";

	private const string Phase2EnemyShip4SpawnPointTag = "phase_2_enemy_ship_4_sp";

	private const string Phase2EnemyShip5SpawnPointTag = "phase_2_enemy_ship_5_sp";

	private const string Phase2EnemyShipStationary1SpawnPointTag = "phase_2_enemy_ship_stationary_1";

	private const string Phase2EnemyShip1TargetPointTag = "phase_2_enemy_ship_1_target";

	private const string Phase2EnemyShip2TargetPointTag = "phase_2_enemy_ship_2_target";

	private const string Phase2EnemyShip3TargetPointTag = "phase_2_enemy_ship_3_target";

	private const string Phase2EnemyShip4TargetPointTag = "phase_2_enemy_ship_4_target";

	private const string Phase2EnemyShip5TargetPointTag = "phase_2_enemy_ship_5_target";

	private const string Phase2EnemyShip1StringId = "ship_meditlight_storyline_q5";

	private const string Phase2EnemyShip2StringId = "ship_meditlight_storyline_q5";

	private const string Phase2EnemyShip3StringId = "ship_meditlight_storyline_q5";

	private const string Phase2EnemyShip4StringId = "ship_meditlight_storyline_q5";

	private const string Phase2EnemyShip5StringId = "ship_meditlight_storyline_q5";

	private const string Phase2EnemyShipStationary1StringId = "western_medium_ship";

	private const string Phase2AllyShip1SpawnPointTag = "phase_2_ally_ship_1_sp";

	private const string Phase2AllyShip2SpawnPointTag = "phase_2_ally_ship_2_sp";

	private const string Phase2AllyShip3SpawnPointTag = "phase_2_ally_ship_3_sp";

	private const string Phase2AllyShip4SpawnPointTag = "phase_2_ally_ship_4_sp";

	private const string Phase2AllyShip5SpawnPointTag = "phase_2_ally_ship_5_sp";

	private const string Phase2AllyShip1StringId = "aserai_heavy_ship";

	private const string Phase2AllyShip2StringId = "nord_medium_ship";

	private const string Phase2AllyShip3StringId = "northern_medium_ship";

	private const string Phase2AllyShip4StringId = "sturgia_heavy_ship";

	private const string Phase2AllyShip5StringId = "northern_medium_ship";

	private const float AutoCutLoosePirateShipTimer = 25f;

	private const float AutoEstablishConnectionsForPirateShipsTimer = 7f;

	private const string Phase2EscapeShipTargetPointPrefix = "phase_2_escape_ship_target";

	private const string Phase2EscapeShipTargetPointExpression = "phase_2_escape_ship_target(_\\d+)*";

	private const string Phase2EscapeShipBarrierTag = "phase_2_barricade";

	private const string Phase3TriggerVolumeBoxTag = "phase_3_trigger_volume_box_tag";

	private const string Phase3EnemyShip1StringId = "eastern_heavy_ship";

	private const string Phase3EnemyShip2StringId = "aserai_heavy_ship";

	private const string Phase3EnemyShip3StringId = "nord_medium_ship";

	private const string Phase3EnemyShip4StringId = "nord_medium_ship";

	private const string Phase3EnemyShip5StringId = "khuzait_heavy_ship";

	private const string Phase3EnemyShip1SpawnPointTag = "phase_3_enemy_ship_1_sp";

	private const string Phase3EnemyShip2SpawnPointTag = "phase_3_enemy_ship_2_sp";

	private const string Phase3EnemyShip3SpawnPointTag = "phase_3_enemy_ship_3_sp";

	private const string Phase3EnemyShip4SpawnPointTag = "phase_3_enemy_ship_4_sp";

	private const string Phase3EnemyShip5SpawnPointTag = "phase_3_enemy_ship_5_sp";

	private const string Phase3EnemyShipReinforcementSpawnPoint1Tag = "phase_3_enemy_reinforcement_1_sp";

	private const string Phase3EnemyShipReinforcementSpawnPoint2Tag = "phase_3_enemy_reinforcement_2_sp";

	private const string Phase3EnemyReinforcementShip1StringId = "empire_medium_ship";

	private const string Phase3EnemyReinforcementShip2StringId = "nord_medium_ship";

	private const string Phase3EnemyReinforcementShip3StringId = "sturgia_heavy_ship";

	private const string Phase3AllyShip1SpawnPointTag = "phase_3_ally_ship_1_sp";

	private const string Phase3AllyShip2SpawnPointTag = "phase_3_ally_ship_2_sp";

	private const string Phase3AllyShip3SpawnPointTag = "phase_3_ally_ship_3_sp";

	private const string Phase3AllyShip4SpawnPointTag = "phase_3_ally_ship_4_sp";

	private const string Phase3AllyShip5SpawnPointTag = "phase_3_ally_ship_5_sp";

	private const string Phase3PlayerShipSpawnPointTag = "phase_3_player_ship_sp";

	private const string Phase3PlayerShipStringId = "empire_heavy_ship";

	private const string Phase3PlayerShipUsePointStringId = "sp_troop_captain";

	private const string PurigsEnterenceTriggerBoxTag = "phase_4_purigs_entrance_trigger_box";

	private const string PurigImmortalShipSpawnPointTag = "sp_immortal_purig";

	private const string PurigBodyguard1ImmortalShipSpawnPointTag = "sp_immortal_bodyguard_1";

	private const string PurigBodyguard2ImmortalShipSpawnPointTag = "sp_immortal_bodyguard_2";

	private const string PurigShipSpawnPointTag = "phase_4_purig_ship_sp";

	private const string PurigShipStringId = "purigs_roundship_storyline";

	private const string PurigShipTroopStringId = "sea_hounds";

	private const int PurigShipTroopCount = 40;

	private const string NavalBossFightPlayerSpawnPointTag = "naval_boss_fight_player_sp";

	private const string NavalBossFightPlayerAllySpawnPointTagPrefix = "naval_boss_fight_player_ally_sp_";

	private const string NavalBossFightEnemyBossSpawnPointTag = "naval_boss_fight_enemy_boss_sp";

	private const string NavalBossFightEnemyTroopSpawnPointTagPrefix = "naval_boss_fight_player_enemy_sp_";

	private const int NavalBossFightAllyTroopCount = 2;

	private const int NavalBossFightEnemyTroopCount = 2;

	private const string NavalBossFightPlayerBodyguardTroopStringId = "gangradirs_kin_melee";

	private const string NavalBossFightEnemyBodyguardTroopStringId = "sea_hounds";

	private const string BossFightConversationCameraTag = "sp_boss_fight_camera";

	private readonly List<KeyValuePair<string, string>> _phase1EnemyShip2UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", ""),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", ""),
		new KeyValuePair<string, string>("roof", "roof_7"),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "")
	};

	private readonly List<KeyValuePair<string, string>> _escapeShipUpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_large_arrow_and_javelin_crates_lvl3"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", ""),
		new KeyValuePair<string, string>("roof", "roof_5"),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "")
	};

	private readonly List<KeyValuePair<string, string>> _phase2AllyShip1UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("aft", "aft_battlement_lvl3_wbarracks"),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_arrow_and_javelin_crates_lvl2"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_southern_shields_lvl1")
	};

	private readonly List<KeyValuePair<string, string>> _phase2AllyShip2UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_arrow_and_javelin_crates_lvl2"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl3"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_northern_shields_lvl1")
	};

	private readonly List<KeyValuePair<string, string>> _phase2AllyShip3UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_large_arrow_and_javelin_crates_lvl3"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl3"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_northern_shields_lvl2")
	};

	private readonly List<KeyValuePair<string, string>> _phase2AllyShip4UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_large_arrow_and_javelin_crates_lvl3"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_northern_shields_lvl2")
	};

	private readonly List<KeyValuePair<string, string>> _phase2AllyShip5UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_arrow_and_javelin_crates_lvl2"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_northern_shields_lvl1")
	};

	private readonly List<KeyValuePair<string, string>> _phase3EnemyShip1UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_arrow_and_javelin_crates_lvl2"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl3"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_southern_shields_lvl2")
	};

	private readonly List<KeyValuePair<string, string>> _phase3EnemyShip2UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_ammo_crates_lvl2"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_southern_shields_lvl2")
	};

	private readonly List<KeyValuePair<string, string>> _phase3EnemyShip3UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_ammo_crates_lvl2"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "")
	};

	private readonly List<KeyValuePair<string, string>> _phase3EnemyShip4UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_ammo_bins_lvl1"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl3"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_northern_shields_lvl1")
	};

	private readonly List<KeyValuePair<string, string>> _phase3EnemyShip5UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_ammo_bins_lvl1"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_southern_shields_lvl1")
	};

	private readonly List<KeyValuePair<string, string>> _phase3EnemyReinforcementShip1UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_boarding_weapons_lvl3"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_southern_shields_lvl2")
	};

	private readonly List<KeyValuePair<string, string>> _phase3EnemyReinforcementShip2UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_boarding_weapons_lvl3"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl3"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_northern_shields_lvl1")
	};

	private readonly List<KeyValuePair<string, string>> _phase3EnemyReinforcementShip3UpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", ""),
		new KeyValuePair<string, string>("aft", ""),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", "deck_boarding_weapons_lvl3"),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "side_northern_shields_lvl2")
	};

	private readonly List<KeyValuePair<string, string>> _phase4PurigsShipUpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", "fore_battlement_lvl3_wbarracks"),
		new KeyValuePair<string, string>("aft", "aft_battlement_lvl3_wbarracks"),
		new KeyValuePair<string, string>("hull", ""),
		new KeyValuePair<string, string>("deck", ""),
		new KeyValuePair<string, string>("oars", ""),
		new KeyValuePair<string, string>("sail", "sails_lvl3"),
		new KeyValuePair<string, string>("roof", ""),
		new KeyValuePair<string, string>("bow", ""),
		new KeyValuePair<string, string>("side", "")
	};

	private Quest5InstructionState _instructionState;

	private Quest5ApproachObjective _approachObjective;

	private Quest5JumpObjective _jumpObjective;

	private Quest5SwimObjective _swimObjective;

	private Quest5ClearGuardsObjective _clearGuardsObjective;

	private Quest5CheckInteriorObjective _checkInteriorObjective;

	private Quest5TalkWithYourSisterObjective _talkWithYourSisterObjective;

	private Quest5ReturnToDeckObjective _returnToDeckObjective;

	private Quest5CutLooseObjective _cutLooseObjective;

	private Quest5GunnarUsesShipObjective _gunnarUsesShipObjective;

	private Quest5EscapeObjective _escapeObjective;

	private Quest5ReachAlliesObjective _reachAlliesObjective;

	private Quest5DefeatEnemiesObjective _defeatEnemiesObjective;

	private Quest5DefeatPurigsShipObjective _defeatPurigsShipObjective;

	private Quest5DefeatPurigObjective _defeatPurigObjective;

	private GunnarMovementState _gunnarMovementState;

	private GunnarMovementStateForClimbingShip _gunnarMovementStateForClimbingShip;

	private ClimbingMachine _targetClimbingMachine;

	private MissionTimer _gunnarFellIntoTheWaterTimer;

	private GameEntity _jumpOffInitialPositionGameEntity;

	private GameEntity _jumpOffTargetPositionGameEntity;

	private GameEntity _hidingSpot1PositionGameEntity;

	private MissionShip _phase1EnemyShip1;

	private MissionShip _phase1EnemyShip2;

	private MissionShip _phase1EnemyShip3;

	private MissionShip _phase1EnemyShip4;

	private Figurehead EscapeShipFigurehead = DefaultFigureheads.Lion;

	private bool _talkedWithSister;

	private bool _crusasAndSeaHoundMovedToTheConversationPoints;

	private List<GameEntity> _dynamicPatrolAreas = new List<GameEntity>();

	private List<Agent> _stealthAgents = new List<Agent>();

	private WeakGameEntity _crusasConversationPointFrame;

	private WeakGameEntity _slaveTraderConversationPointFrame;

	private GameEntity _approachPointEntity;

	private GameEntity _phase1EnemyShipToInteriorShipDoorEntity;

	private GameEntity _phase1InteriorToEnemyShip3ShipDoorEntity;

	private GameEntity _phase1EnemyShip1InitialSpawnEntity;

	private GameEntity _phase1EnemyShip1TargetEntity;

	private Queue<ConversationSound> _conversationSounds = new Queue<ConversationSound>();

	private List<DialogNotificationHandle> _dialogNotificationHandleCache = new List<DialogNotificationHandle>();

	private float _lastCachedPlayerShipDistanceToTargetApproachPoint;

	private MissionTimer _playerShipsTargetApproachPointDistanceCheckTimer;

	private MissionTimer _escapeShipCutLooseTimer;

	private MissionTimer _allowedSwimRadiusCheckTimer;

	private ActionIndexCache _sisterWoundedAnimationActionIndexCache;

	private ActionIndexCache _slaveTraderShipOarsmanActionIndexCache;

	private Vec3 _phase1PlayerShipSpawnPosition = Vec3.Invalid;

	private Equipment _mainAgentEquipmentCopyForInteriorMission;

	private MissionShip _phase2EnemyShip1;

	private MissionShip _phase2EnemyShip2;

	private MissionShip _phase2EnemyShip3;

	private MissionShip _phase2EnemyShip4;

	private MissionShip _phase2EnemyShip5;

	private MissionShip _phase2EnemyShipStationary1;

	private GameEntity _phase2EscapeShipPirateTargetFrame1;

	private GameEntity _phase2EscapeShipPirateTargetFrame2;

	private GameEntity _phase2EscapeShipPirateTargetFrame3;

	private GameEntity _phase2EscapeShipPirateTargetFrame4;

	private GameEntity _phase2EscapeShipPirateTargetFrame5;

	private GameEntity _currentPhase2EscapeShipTargetPoint;

	private MissionShip _phase2AllyShip1;

	private MissionShip _phase2AllyShip2;

	private MissionShip _phase2AllyShip3;

	private MissionShip _phase2AllyShip4;

	private MissionShip _phase2AllyShip5;

	private Dictionary<MissionShip, GameEntity> _pirateShipTriggerPoints = new Dictionary<MissionShip, GameEntity>();

	private Dictionary<MissionShip, bool> _isPirateShipMovementDisabled = new Dictionary<MissionShip, bool>();

	private Dictionary<MissionShip, ShipAttachmentMachine> _pirateShipEnabledAttachmentMachine = new Dictionary<MissionShip, ShipAttachmentMachine>();

	private Dictionary<MissionShip, bool> _isPirateShipTriggered = new Dictionary<MissionShip, bool>();

	private Dictionary<MissionShip, bool> _isPirateShipMovingToTheEscapeShip = new Dictionary<MissionShip, bool>();

	private Dictionary<MissionShip, bool> _isPirateShipLostItsCrew = new Dictionary<MissionShip, bool>();

	private Dictionary<MissionShip, bool> _limitPirateShipChasingSpeed = new Dictionary<MissionShip, bool>();

	private Dictionary<MissionShip, MissionTimer> _autoCutLooseTimersForPirateShips = new Dictionary<MissionShip, MissionTimer>();

	private Dictionary<MissionShip, MissionTimer> _autoEstablishConnectionsForPirateShips = new Dictionary<MissionShip, MissionTimer>();

	private Dictionary<MissionShip, bool> _isMissionShipBoardedToTheEscapeShip = new Dictionary<MissionShip, bool>();

	private List<GameEntity> _phase2EscapeShipTargetPointEntities = new List<GameEntity>(32);

	private Queue<GameEntity> _phase2EscapeShipTargetPoints = new Queue<GameEntity>();

	private MissionTimer _playerLeftTheEscapeShipTimer;

	private MissionTimer _phase2EscapeShipStuckTimer;

	private Vec3 _phase2EscapeShipStuckCheckPosition = Vec3.Invalid;

	private float _escapeShipTargetSpeed;

	private float _escapeShipSpeed;

	private Vec2 _escapeShipTargetDirection;

	private Vec2 _escapeShipDirection;

	private readonly List<KeyValuePair<string, int>> _phase2AllyShip1Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("aserai_marine_t5", 54),
		new KeyValuePair<string, int>("southern_pirates_chief", 18)
	};

	private readonly List<KeyValuePair<string, int>> _phase2AllyShip2Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("skolderbrotva_tier_2", 5),
		new KeyValuePair<string, int>("skolderbrotva_tier_3", 34)
	};

	private readonly List<KeyValuePair<string, int>> _phase2AllyShip3Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("gangradirs_kin_ranged", 18),
		new KeyValuePair<string, int>("gangradirs_kin_melee", 19)
	};

	private readonly List<KeyValuePair<string, int>> _phase2AllyShip4Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("skolderbrotva_tier_2", 32),
		new KeyValuePair<string, int>("skolderbrotva_tier_3", 34)
	};

	private readonly List<KeyValuePair<string, int>> _phase2AllyShip5Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("skolderbrotva_tier_3", 18),
		new KeyValuePair<string, int>("skolderbrotva_tier_2", 17)
	};

	private readonly List<KeyValuePair<string, int>> _phase2EnemyShip1Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hound_captivity", 4),
		new KeyValuePair<string, int>("sea_hound_captivity", 1)
	};

	private readonly List<KeyValuePair<string, int>> _phase2EnemyShip2Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hound_captivity", 3),
		new KeyValuePair<string, int>("sea_hound_captivity", 2)
	};

	private readonly List<KeyValuePair<string, int>> _phase2EnemyShip3Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hound_captivity", 3),
		new KeyValuePair<string, int>("sea_hound_captivity", 2)
	};

	private readonly List<KeyValuePair<string, int>> _phase2EnemyShip4Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hound_captivity", 3),
		new KeyValuePair<string, int>("sea_hound_captivity", 2)
	};

	private readonly List<KeyValuePair<string, int>> _phase2EnemyShip5Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hound_captivity", 3),
		new KeyValuePair<string, int>("sea_hound_captivity", 2)
	};

	private readonly List<KeyValuePair<string, int>> _phase2EnemyShipStationary1Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hounds_marksman", 8)
	};

	private MissionShip _phase3EnemyShip1;

	private MissionShip _phase3EnemyShip2;

	private MissionShip _phase3EnemyShip3;

	private MissionShip _phase3EnemyShip4;

	private MissionShip _phase3EnemyShip5;

	private MissionShip _phase3EnemyReinforcementShip1;

	private MissionShip _phase3EnemyReinforcementShip2;

	private VolumeBox _phase3TriggerVolumeBox;

	private readonly List<MissionShip> _allyShipTargetKeysBuffer = new List<MissionShip>(16);

	private readonly HashSet<MissionShip> _assignedEnemyShips = new HashSet<MissionShip>();

	private bool _isReinforcementCalled;

	private bool _isReinforcementInitialized;

	private readonly List<KeyValuePair<string, int>> _phase3PlayerShipTroops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("gangradirs_kin_melee", 40),
		new KeyValuePair<string, int>("gangradirs_kin_melee", 40)
	};

	private readonly List<KeyValuePair<string, int>> _phase3EnemyShip1Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hounds", 52),
		new KeyValuePair<string, int>("sea_hounds_marksman", 10)
	};

	private readonly List<KeyValuePair<string, int>> _phase3EnemyShip2Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hounds_pups", 64),
		new KeyValuePair<string, int>("sea_hounds_marksman", 10)
	};

	private readonly List<KeyValuePair<string, int>> _phase3EnemyShip3Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hounds_pups", 25),
		new KeyValuePair<string, int>("sea_hounds", 44)
	};

	private readonly List<KeyValuePair<string, int>> _phase3EnemyShip4Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hounds_pups", 15),
		new KeyValuePair<string, int>("sea_hounds", 50)
	};

	private readonly List<KeyValuePair<string, int>> _phase3EnemyShip5Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hounds_marksman", 16),
		new KeyValuePair<string, int>("sea_hounds", 50)
	};

	private readonly List<KeyValuePair<string, int>> _phase3EnemyReinforcementShip1Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hounds_marksman", 15),
		new KeyValuePair<string, int>("sea_hound_captivity", 30)
	};

	private readonly List<KeyValuePair<string, int>> _phase3EnemyReinforcementShip2Troops = new List<KeyValuePair<string, int>>
	{
		new KeyValuePair<string, int>("sea_hounds_marksman", 15),
		new KeyValuePair<string, int>("sea_hounds", 30)
	};

	private int _phase3TotalEnemyCount;

	private BossFightStateEnum BossFightState;

	private List<Agent> _purigShipAgents = new List<Agent>();

	private List<Agent> _duelPhaseAllyAgents;

	private List<Agent> _duelPhaseEnemyAgents;

	private Queue<ConversationSound> _purigNotifications = new Queue<ConversationSound>();

	private Agent _purigBodyguard1;

	private Agent _purigBodyguard2;

	private bool _isPurigCutsceneStarted;

	private bool _isPlayerUsingShipAtTheStartOfThePurigCutscene;

	private StandingPoint _playerStandingPointAtTheStartOfThePurigCutscene;

	private VolumeBox _phase4TriggerVolumeBox;

	private GameEntity _playerSpawnPointEntity;

	private GameEntity _enemyBossSpawnPointEntity;

	private BattleSideEnum _winnerSide = (BattleSideEnum)(-1);

	private NavalAgentsLogic _navalAgentsLogic;

	private NavalShipsLogic _navalShipsLogic;

	private NavalTrajectoryPlanningLogic _navalTrajectoryPlanningLogic;

	private MissionObjectiveLogic _missionObjectiveLogic;

	private LightScriptedFiresMissionController _lightScriptedFiresMissionController;

	private List<Formation> _availableAllyFormations = new List<Formation>();

	private List<Formation> _availableEnemyFormations = new List<Formation>();

	private MissionTimer _endMissionTimer;

	private Formation _playerFormation;

	private MissionShip _playerShip;

	private readonly MobileParty _enemyParty;

	private Agent _laharAgent;

	private Agent _bjolgurAgent;

	private Agent _crusasAgent;

	private Agent _gunnarAgent;

	private Agent _purigAgent;

	private Agent _slaveTraderAgent;

	private CharacterObject _slaveTraderCharacter;

	private Agent[] _slaveTraderShipOarsmen = (Agent[])(object)new Agent[6];

	private AgentNavalComponent _gunnarAgentNavalComponent;

	private bool _isCheckpointInitialize;

	private bool _isMissionFailPopUpTriggered;

	private GameEntity JumpOffInitialPosition
	{
		get
		{
			if (_jumpOffInitialPositionGameEntity == (GameEntity)null)
			{
				_jumpOffInitialPositionGameEntity = Mission.Current.Scene.FindEntityWithTag("gangradir_jump_off_initial");
			}
			return _jumpOffInitialPositionGameEntity;
		}
	}

	private GameEntity JumpOffTargetPosition
	{
		get
		{
			if (_jumpOffTargetPositionGameEntity == (GameEntity)null)
			{
				_jumpOffTargetPositionGameEntity = Mission.Current.Scene.FindEntityWithTag("gangradir_jump_off_target");
			}
			return _jumpOffTargetPositionGameEntity;
		}
	}

	private GameEntity HidingSpot1Position
	{
		get
		{
			if (_hidingSpot1PositionGameEntity == (GameEntity)null)
			{
				_hidingSpot1PositionGameEntity = Mission.Current.Scene.FindEntityWithTag("sp_gangradir_hiding_spot");
			}
			return _hidingSpot1PositionGameEntity;
		}
	}

	private MatrixFrame GunnarShipUsePosition => EscapeShip.GetCaptainSpawnGlobalFrame();

	public GameEntity Phase1InteriorCameraSisterEntity { get; private set; }

	private MissionShip EscapeShip => _phase1EnemyShip3 ?? _playerShip;

	public bool IsEscapeShipStuck { get; private set; }

	private int Phase2AllyShip1TroopCount => _phase2AllyShip1Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2AllyShip2TroopCount => _phase2AllyShip2Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2AllyShip3TroopCount => _phase2AllyShip3Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2AllyShip4TroopCount => _phase2AllyShip4Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2AllyShip5TroopCount => _phase2AllyShip5Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2EnemyShip1TroopCount => _phase2EnemyShip1Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2EnemyShip2TroopCount => _phase2EnemyShip2Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2EnemyShip3TroopCount => _phase2EnemyShip3Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2EnemyShip4TroopCount => _phase2EnemyShip4Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2EnemyShip5TroopCount => _phase2EnemyShip5Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase2EnemyShipStationary1TroopCount => _phase2EnemyShipStationary1Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase3PlayerShipTroopCount => _phase3PlayerShipTroops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase3EnemyShip1TroopCount => _phase3EnemyShip1Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase3EnemyShip2TroopCount => _phase3EnemyShip2Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase3EnemyShip3TroopCount => _phase3EnemyShip3Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase3EnemyShip4TroopCount => _phase3EnemyShip4Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase3EnemyShip5TroopCount => _phase3EnemyShip5Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase3EnemyReinforcementShip1TroopCount => _phase3EnemyReinforcementShip1Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	private int Phase3EnemyReinforcementShip2TroopCount => _phase3EnemyReinforcementShip2Troops.Sum((KeyValuePair<string, int> kvp) => kvp.Value);

	public BossFightOutComeEnum BossFightOutCome { get; private set; }

	public GameEntity BossFightConversationCameraGameEntity { get; private set; }

	public MissionShip Phase4PurigShip { get; private set; }

	public Agent SisterAgent { get; private set; }

	public Quest5SetPieceBattleMissionState LastHitCheckpoint { get; private set; }

	public Quest5SetPieceBattleMissionState State { get; private set; }

	public bool ShouldMissionContinueFromCheckpoint { get; private set; }

	public Quest5SetPieceBattleMissionController(Quest5SetPieceBattleMissionState lastHitCheckpoint, MobileParty enemyParty)
	{
		//IL_0c8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c93: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d1f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d24: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b8: Unknown result type (might be due to invalid IL or missing references)
		BossFightOutCome = BossFightOutComeEnum.None;
		State = Quest5SetPieceBattleMissionState.None;
		LastHitCheckpoint = lastHitCheckpoint;
		ShouldMissionContinueFromCheckpoint = false;
		_enemyParty = enemyParty;
		Hero.MainHero.HitPoints = Hero.MainHero.MaxHitPoints;
		NavalStorylineData.Gunnar.HitPoints = NavalStorylineData.Gunnar.MaxHitPoints;
		NavalStorylineData.Prusas.HitPoints = NavalStorylineData.Prusas.MaxHitPoints;
		NavalStorylineData.Purig.HitPoints = NavalStorylineData.Purig.MaxHitPoints;
		NavalStorylineData.Bjolgur.HitPoints = NavalStorylineData.Bjolgur.MaxHitPoints;
		NavalStorylineData.Lahar.HitPoints = NavalStorylineData.Lahar.MaxHitPoints;
	}

	public override void AfterStart()
	{
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).AfterStart();
		Mission.Current.Scene.SetAtmosphereWithName("TOD_02_00_SemiCloudy");
		_slaveTraderCharacter = MBObjectManager.Instance.GetObject<CharacterObject>("sea_hounds");
		AddConversationSounds();
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalTrajectoryPlanningLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalTrajectoryPlanningLogic>();
		_missionObjectiveLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionObjectiveLogic>();
		_lightScriptedFiresMissionController = ((MissionBehavior)this).Mission.GetMissionBehavior<LightScriptedFiresMissionController>();
		Team team = Mission.GetTeam((TeamSideEnum)0);
		AddAvailableAllyFormation(team.GetFormation((FormationClass)0));
		AddAvailableAllyFormation(team.GetFormation((FormationClass)1));
		AddAvailableAllyFormation(team.GetFormation((FormationClass)2));
		AddAvailableAllyFormation(team.GetFormation((FormationClass)3));
		AddAvailableAllyFormation(team.GetFormation((FormationClass)4));
		AddAvailableAllyFormation(team.GetFormation((FormationClass)5));
		AddAvailableAllyFormation(team.GetFormation((FormationClass)6));
		AddAvailableAllyFormation(team.GetFormation((FormationClass)7));
		Team team2 = Mission.GetTeam((TeamSideEnum)2);
		AddAvailableEnemyFormation(team2.GetFormation((FormationClass)0));
		AddAvailableEnemyFormation(team2.GetFormation((FormationClass)1));
		AddAvailableEnemyFormation(team2.GetFormation((FormationClass)2));
		AddAvailableEnemyFormation(team2.GetFormation((FormationClass)3));
		AddAvailableEnemyFormation(team2.GetFormation((FormationClass)4));
		AddAvailableEnemyFormation(team2.GetFormation((FormationClass)5));
		AddAvailableEnemyFormation(team2.GetFormation((FormationClass)6));
		AddAvailableEnemyFormation(team2.GetFormation((FormationClass)7));
		_phase1InteriorToEnemyShip3ShipDoorEntity = Mission.Current.Scene.FindEntityWithTag("phase_1_interior_to_enemy_ship_3_door_tag");
		_phase1InteriorToEnemyShip3ShipDoorEntity.GetFirstScriptOfType<ShipDoorUsePoint>().SetShipDoorUsePointEnabled(isEnabled: false);
		foreach (GameEntity item2 in Mission.Current.Scene.FindEntitiesWithTagExpression("phase_2_escape_ship_target(_\\d+)*"))
		{
			_phase2EscapeShipTargetPointEntities.Add(item2);
		}
		GameEntity[] array = (GameEntity[])(object)new GameEntity[_phase2EscapeShipTargetPointEntities.Count];
		foreach (GameEntity phase2EscapeShipTargetPointEntity in _phase2EscapeShipTargetPointEntities)
		{
			int num = int.Parse(phase2EscapeShipTargetPointEntity.Tags.FirstOrDefault().Split(new char[1] { '_' })[^1]);
			array[num - 1] = phase2EscapeShipTargetPointEntity;
		}
		GameEntity[] array2 = array;
		foreach (GameEntity item in array2)
		{
			_phase2EscapeShipTargetPoints.Enqueue(item);
		}
		_phase1EnemyShip1InitialSpawnEntity = Mission.Current.Scene.FindEntityWithTag("phase_1_enemy_ship_1_sp_initial");
		_phase1EnemyShip1TargetEntity = Mission.Current.Scene.FindEntityWithTag("phase_1_enemy_ship_1_sp");
		_phase3TriggerVolumeBox = Mission.Current.Scene.FindEntityWithTag("phase_3_trigger_volume_box_tag").GetFirstScriptOfType<VolumeBox>();
		_phase4TriggerVolumeBox = Mission.Current.Scene.FindEntityWithTag("phase_4_purigs_entrance_trigger_box").GetFirstScriptOfType<VolumeBox>();
		_approachPointEntity = Mission.Current.Scene.FindEntityWithTag("phase_1_approach_point");
		_navalShipsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetTeamShipDeploymentLimit((TeamSideEnum)0, NavalShipDeploymentLimit.Max());
		_navalShipsLogic.SetTeamShipDeploymentLimit((TeamSideEnum)1, NavalShipDeploymentLimit.Max());
		_navalShipsLogic.SetTeamShipDeploymentLimit((TeamSideEnum)2, NavalShipDeploymentLimit.Max());
		_navalShipsLogic.SetDeploymentMode(value: false);
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().DisableStartConversation(true);
		_playerFormation = GetAvailableAllyFormation();
		Hero.MainHero.Heal(Hero.MainHero.MaxHitPoints, false);
		NavalStorylineData.Gunnar.Heal(NavalStorylineData.Gunnar.MaxHitPoints, false);
		NavalStorylineData.Prusas.Heal(NavalStorylineData.Prusas.MaxHitPoints, false);
		StoryModeHeroes.LittleSister.Heal(StoryModeHeroes.LittleSister.MaxHitPoints, false);
		_sisterWoundedAnimationActionIndexCache = ActionIndexCache.Create("act_conversation_weary2_loop");
		_slaveTraderShipOarsmanActionIndexCache = ActionIndexCache.Create("act_sit_2");
		_navalAgentsLogic.SetSpawnReinforcementsOnTick(value: false);
		State = LastHitCheckpoint;
	}

	public override void OnBehaviorInitialize()
	{
		if (!SailWindProfile.IsSailWindProfileInitialized)
		{
			SailWindProfile.InitializeProfile();
		}
		if (_navalShipsLogic == null)
		{
			_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		}
		_navalShipsLogic.ShipAttachmentBrokenEvent += OnAttachmentBroken;
	}

	public override void OnFixedMissionTick(float fixedDt)
	{
		((MissionBehavior)this).OnFixedMissionTick(fixedDt);
		Quest5SetPieceBattleMissionState state = State;
		if (state == Quest5SetPieceBattleMissionState.Phase2InProgress)
		{
			HandlePirateShipGettingCloseToEscapeShip(_phase2EnemyShip1, _phase2EscapeShipPirateTargetFrame1, 5f, fixedDt);
			HandlePirateShipGettingCloseToEscapeShip(_phase2EnemyShip2, _phase2EscapeShipPirateTargetFrame2, 5f, fixedDt);
			HandlePirateShipGettingCloseToEscapeShip(_phase2EnemyShip3, _phase2EscapeShipPirateTargetFrame3, 5f, fixedDt);
			HandlePirateShipGettingCloseToEscapeShip(_phase2EnemyShip4, _phase2EscapeShipPirateTargetFrame4, 5f, fixedDt);
			HandlePirateShipGettingCloseToEscapeShip(_phase2EnemyShip5, _phase2EscapeShipPirateTargetFrame5, 5f, fixedDt);
			MoveEscapeShipAlongTheTrack(fixedDt);
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_098f: Unknown result type (might be due to invalid IL or missing references)
		//IL_099b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b52: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b5c: Expected O, but got Unknown
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a60: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac7: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Expected O, but got Unknown
		//IL_0bb8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b35: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnMissionTick(dt);
		HandleStealthShipsBridgeConnections();
		Vec3 position;
		switch (State)
		{
		case Quest5SetPieceBattleMissionState.InitializePhase1Part1:
			InitializePhase1Part1();
			State = Quest5SetPieceBattleMissionState.InitializePhase1Part2;
			break;
		case Quest5SetPieceBattleMissionState.InitializePhase1Part2:
			InitializePhase1Part2();
			HandlePlayersBridgeAndControlPointUsagesForPhase1GoToEnemyShip();
			State = Quest5SetPieceBattleMissionState.Phase1GoToEnemyShip;
			break;
		case Quest5SetPieceBattleMissionState.Phase1GoToEnemyShip:
		{
			if (_instructionState == Quest5InstructionState.None)
			{
				_instructionState = Quest5InstructionState.Approach;
			}
			AdjustWindDirectionAccordingToTargetFrame(_approachPointEntity.GetGlobalFrame(), 2f, addRandomRotation: true);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_playerShip).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			if (((Vec3)(ref globalFrame.origin)).Distance(_approachPointEntity.GetGlobalFrame().origin) <= 30f)
			{
				DisableSlaveTraderShipAgents();
				OnPlayerShipReachedApproachDistance();
				HandlePlayersBridgeAndControlPointUsagesForPhase1SwimmingAndStealthPhase();
			}
			_phase1EnemyShip3.SetAnchor(isAnchored: true);
			_phase1EnemyShip3.ShipOrder.SetShipStopOrder();
			HandleStealthShipsBridgeConnections();
			MovePhase1EnemyShip1ToItsTargetPoint();
			break;
		}
		case Quest5SetPieceBattleMissionState.Phase1SwimmingPhase:
			if (_instructionState == Quest5InstructionState.WaitForJump)
			{
				_instructionState = Quest5InstructionState.Jump;
			}
			else if (_instructionState == Quest5InstructionState.WaitForSwim && Agent.Main.IsInWater())
			{
				_instructionState = Quest5InstructionState.Swim;
			}
			_playerShip.ShipOrder.SetShipStopOrder();
			_playerShip.ShipOrder.SetOrderOarsmenLevel(0);
			CheckAndPlayCrusasAndSlaveTraderConversationSound();
			if (_phase1EnemyShip4.GetIsAgentOnShip(Agent.Main))
			{
				State = Quest5SetPieceBattleMissionState.InitializeStealthPhasePart1;
				SetLastCheckpoint(Quest5SetPieceBattleMissionState.InitializeStealthPhasePart1);
			}
			break;
		case Quest5SetPieceBattleMissionState.InitializeStealthPhasePart1:
			InitializeStealthPhasePart1();
			State = Quest5SetPieceBattleMissionState.InitializeStealthPhasePart2;
			break;
		case Quest5SetPieceBattleMissionState.InitializeStealthPhasePart2:
			InitializeStealthPhasePart2();
			HealMainHero();
			State = Quest5SetPieceBattleMissionState.Phase1StealthPhase;
			HandlePlayersBridgeAndControlPointUsagesForPhase1SwimmingAndStealthPhase();
			break;
		case Quest5SetPieceBattleMissionState.Phase1StealthPhase:
			HandleStealthShipsBridgeConnections();
			HandleEscapeShipInteriorDoorUsage();
			if (Agent.Main == null || !Agent.Main.IsActive())
			{
				EndMissionWithAutoContinueFromCheckpoint();
			}
			else
			{
				_phase1EnemyShip2.GetWorldPositionOnDeck(out var worldPosition);
				Vec2 asVec = ((WorldPosition)(ref worldPosition)).AsVec2;
				position = Agent.Main.Position;
				if (((Vec2)(ref asVec)).Distance(((Vec3)(ref position)).AsVec2) < 20f && _instructionState == Quest5InstructionState.WaitForClearGuards)
				{
					_instructionState = Quest5InstructionState.ClearGuards;
				}
				if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_stealthAgents) && _instructionState == Quest5InstructionState.WaitForCheckInterior)
				{
					_instructionState = Quest5InstructionState.CheckInterior;
				}
			}
			_phase1EnemyShip3.SetAnchor(isAnchored: true);
			_phase1EnemyShip3.ShipOrder.SetShipStopOrder();
			break;
		case Quest5SetPieceBattleMissionState.Phase1InitializeShipInteriorPhase:
			InitializeShipInteriorPhase();
			break;
		case Quest5SetPieceBattleMissionState.Phase1ShipInteriorPhase:
			if (_talkedWithSister)
			{
				_phase1InteriorToEnemyShip3ShipDoorEntity.GetFirstScriptOfType<ShipDoorUsePoint>().SetShipDoorUsePointEnabled(isEnabled: true);
			}
			else
			{
				position = SisterAgent.Position;
				if (((Vec3)(ref position)).Distance(Agent.Main.Position) < 3f)
				{
					Phase1InteriorCameraSisterEntity = Mission.Current.Scene.FindEntityWithTag("phase_1_interior_camera_sister");
				}
			}
			SisterAgent.SetActionChannel(0, ref _sisterWoundedAnimationActionIndexCache, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			break;
		case Quest5SetPieceBattleMissionState.Phase1InitializeGoBackToShip:
			InitializeGoBackToShip();
			if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_stealthAgents))
			{
				_instructionState = Quest5InstructionState.WaitForCutLoose;
			}
			break;
		case Quest5SetPieceBattleMissionState.Phase1EscapePhase:
			if (_talkedWithSister)
			{
				if (_instructionState < Quest5InstructionState.WaitForCutLoose)
				{
					_instructionState = Quest5InstructionState.WaitForCutLoose;
				}
				bool isThereActiveBridgeTo = _phase1EnemyShip3.GetIsThereActiveBridgeTo(_phase1EnemyShip2);
				if (isThereActiveBridgeTo && _instructionState == Quest5InstructionState.WaitForCutLoose && Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_stealthAgents))
				{
					_instructionState = Quest5InstructionState.CutLoose;
					_escapeShipCutLooseTimer = new MissionTimer(300f);
				}
				else if (!isThereActiveBridgeTo && _instructionState == Quest5InstructionState.WaitForGunnarUsesShip && Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_stealthAgents))
				{
					_instructionState = Quest5InstructionState.GunnarUsesShip;
				}
				else if (!isThereActiveBridgeTo)
				{
					State = Quest5SetPieceBattleMissionState.Phase1ToPhase2FadeOut;
				}
				HandleEscapeShipCutLoose();
			}
			break;
		case Quest5SetPieceBattleMissionState.InitializePhase2Part1:
			InitializePhase2Part1();
			State = Quest5SetPieceBattleMissionState.InitializePhase2Part2;
			break;
		case Quest5SetPieceBattleMissionState.InitializePhase2Part2:
			State = Quest5SetPieceBattleMissionState.InitializePhase2Part3;
			InitializePhase2Part2();
			break;
		case Quest5SetPieceBattleMissionState.InitializePhase2Part3:
			State = Quest5SetPieceBattleMissionState.InitializePhase2Part4;
			InitializePhase2Part3();
			break;
		case Quest5SetPieceBattleMissionState.InitializePhase2Part4:
			InitializePhase2Part4();
			HealMainHero();
			SetLastCheckpoint(Quest5SetPieceBattleMissionState.InitializePhase2Part1);
			break;
		case Quest5SetPieceBattleMissionState.Phase2InProgress:
			UpdatePhase2MovingShipParameters(dt);
			if (_isCheckpointInitialize)
			{
				_isCheckpointInitialize = false;
			}
			CheckForEscapeShipStuck();
			HandleEscapeShipSpeed();
			HandleEscapeShipMovement();
			HandlePirateShipMovement(_phase2EnemyShip1, _phase2EscapeShipPirateTargetFrame1);
			HandlePirateShipMovement(_phase2EnemyShip2, _phase2EscapeShipPirateTargetFrame2);
			HandlePirateShipMovement(_phase2EnemyShip3, _phase2EscapeShipPirateTargetFrame3);
			HandlePirateShipMovement(_phase2EnemyShip4, _phase2EscapeShipPirateTargetFrame4);
			HandlePirateShipMovement(_phase2EnemyShip5, _phase2EscapeShipPirateTargetFrame5);
			HandlePirateShipSailModeAccordingToTheGlobalWindVelocity(_phase2EnemyShip1);
			HandlePirateShipSailModeAccordingToTheGlobalWindVelocity(_phase2EnemyShip2);
			HandlePirateShipSailModeAccordingToTheGlobalWindVelocity(_phase2EnemyShip3);
			HandlePirateShipSailModeAccordingToTheGlobalWindVelocity(_phase2EnemyShip4);
			HandlePirateShipSailModeAccordingToTheGlobalWindVelocity(_phase2EnemyShip5);
			HandleStationaryShipMovement(_phase2EnemyShipStationary1);
			CheckIfMainAgentLeftTheEscapeShip();
			AutoCutLooseEmptyPirateShipIfPlayerDoesNotForALongTime(_phase2EnemyShip1);
			AutoCutLooseEmptyPirateShipIfPlayerDoesNotForALongTime(_phase2EnemyShip2);
			AutoCutLooseEmptyPirateShipIfPlayerDoesNotForALongTime(_phase2EnemyShip3);
			AutoCutLooseEmptyPirateShipIfPlayerDoesNotForALongTime(_phase2EnemyShip4);
			AutoCutLooseEmptyPirateShipIfPlayerDoesNotForALongTime(_phase2EnemyShip5);
			AutoEstablishConnectionsForPirateShips(_phase2EnemyShip1, _phase2EscapeShipPirateTargetFrame1);
			AutoEstablishConnectionsForPirateShips(_phase2EnemyShip2, _phase2EscapeShipPirateTargetFrame2);
			AutoEstablishConnectionsForPirateShips(_phase2EnemyShip3, _phase2EscapeShipPirateTargetFrame3);
			AutoEstablishConnectionsForPirateShips(_phase2EnemyShip4, _phase2EscapeShipPirateTargetFrame4);
			AutoEstablishConnectionsForPirateShips(_phase2EnemyShip5, _phase2EscapeShipPirateTargetFrame5);
			HandleAllyShipMovementDuringPhase2(_phase2AllyShip1);
			HandleAllyShipMovementDuringPhase2(_phase2AllyShip2);
			HandleAllyShipMovementDuringPhase2(_phase2AllyShip3);
			HandleAllyShipMovementDuringPhase2(_phase2AllyShip4);
			HandleAllyShipMovementDuringPhase2(_phase2AllyShip5);
			HandlePirateShipBridgeConnectionCount(_phase2EnemyShip1);
			HandlePirateShipBridgeConnectionCount(_phase2EnemyShip2);
			HandlePirateShipBridgeConnectionCount(_phase2EnemyShip3);
			HandlePirateShipBridgeConnectionCount(_phase2EnemyShip4);
			HandlePirateShipBridgeConnectionCount(_phase2EnemyShip5);
			if (_instructionState == Quest5InstructionState.WaitForReachAllies && AreAllPhase2PirateShipsEliminated())
			{
				_instructionState = Quest5InstructionState.ReachAllies;
			}
			if (Agent.Main != null && _phase3TriggerVolumeBox.IsPointIn(Agent.Main.Position))
			{
				State = Quest5SetPieceBattleMissionState.Phase2ToPhase3FadeOut;
			}
			break;
		case Quest5SetPieceBattleMissionState.InitializePhase3Part1:
			InitializePhase3Part1();
			State = Quest5SetPieceBattleMissionState.InitializePhase3Part2;
			SetLastCheckpoint(Quest5SetPieceBattleMissionState.InitializePhase3Part1);
			break;
		case Quest5SetPieceBattleMissionState.InitializePhase3Part2:
			InitializePhase3Part2();
			State = Quest5SetPieceBattleMissionState.InitializePhase3Part3;
			break;
		case Quest5SetPieceBattleMissionState.InitializePhase3Part3:
			InitializePhase3Part3();
			HealMainHero();
			foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
			{
				if (item != _playerShip)
				{
					item.ShipOrder.SetShipEngageOrder();
				}
			}
			break;
		case Quest5SetPieceBattleMissionState.Phase3InProgress:
		{
			if (_isCheckpointInitialize)
			{
				_isCheckpointInitialize = false;
			}
			if (_instructionState == Quest5InstructionState.WaitForDefeatEnemies)
			{
				_instructionState = Quest5InstructionState.DefeatEnemies;
			}
			int count = ((List<Agent>)(object)Mission.Current.PlayerEnemyTeam.ActiveAgents).Count;
			if (_isReinforcementCalled && _isReinforcementInitialized && CanProceedToPhase4())
			{
				if (Agent.Main.IsUsingGameObject && Agent.Main.CurrentlyUsedGameObject is StandingPoint && (object)((UsableMachine)_playerShip.ShipControllerMachine).PilotStandingPoint == Agent.Main.CurrentlyUsedGameObject)
				{
					_isPlayerUsingShipAtTheStartOfThePurigCutscene = true;
					ref StandingPoint playerStandingPointAtTheStartOfThePurigCutscene = ref _playerStandingPointAtTheStartOfThePurigCutscene;
					UsableMissionObject currentlyUsedGameObject = Agent.Main.CurrentlyUsedGameObject;
					playerStandingPointAtTheStartOfThePurigCutscene = (StandingPoint)(object)((currentlyUsedGameObject is StandingPoint) ? currentlyUsedGameObject : null);
				}
				_playerShip.ShipOrder.SetShipStopOrder();
				State = Quest5SetPieceBattleMissionState.Phase3ToPhase4FadeOut;
			}
			else if ((float)count <= (float)_phase3TotalEnemyCount * 0.5f)
			{
				if (!_isReinforcementCalled && !_isReinforcementInitialized)
				{
					CallReinforcement();
				}
				else if (_isReinforcementCalled && !_isReinforcementInitialized)
				{
					InitializeReinforcement();
				}
			}
			if (_isReinforcementCalled && _isReinforcementInitialized)
			{
				if (_phase3EnemyReinforcementShip1.ShipOrder.MovementOrderEnum != ShipOrder.ShipMovementOrderEnum.Engage)
				{
					_phase3EnemyReinforcementShip1.ShipOrder.SetShipEngageOrder();
				}
				if (_phase3EnemyReinforcementShip2.ShipOrder.MovementOrderEnum != ShipOrder.ShipMovementOrderEnum.Engage)
				{
					_phase3EnemyReinforcementShip2.ShipOrder.SetShipEngageOrder();
				}
			}
			CheckIfEnemyAgentFallIntoTheWater();
			break;
		}
		case Quest5SetPieceBattleMissionState.InitializePhase4Part1:
			InitializePhase4Part1();
			break;
		case Quest5SetPieceBattleMissionState.InitializePhase4Part2:
			InitializePhase4Part2();
			HealMainHero();
			break;
		case Quest5SetPieceBattleMissionState.Phase4InProgress:
			if (_isCheckpointInitialize)
			{
				_isCheckpointInitialize = false;
			}
			if (_isPurigCutsceneStarted)
			{
				CheckAndPlayPurigCutsceneNotifications();
			}
			if (_purigShipAgents.Count == 0)
			{
				State = Quest5SetPieceBattleMissionState.Phase4ToBossFightFadeOut;
				_playerShip.SetAnchor(isAnchored: true);
				_playerShip.ShipOrder.SetShipStopOrder();
				Phase4PurigShip.SetAnchor(isAnchored: true);
				DisableAllShipOrderControllers();
			}
			break;
		case Quest5SetPieceBattleMissionState.InitializeBossFightPart1:
			InitializeNavalBossFightPart1();
			State = Quest5SetPieceBattleMissionState.InitializeBossFightPart2;
			break;
		case Quest5SetPieceBattleMissionState.InitializeBossFightPart2:
			InitializeNavalBossFightPart2();
			State = Quest5SetPieceBattleMissionState.Phase4ToBossFightFadeIn;
			break;
		case Quest5SetPieceBattleMissionState.StartBossFightConversation:
			State = Quest5SetPieceBattleMissionState.BossFightConversationInProgress;
			StartBossFightConversation();
			break;
		case Quest5SetPieceBattleMissionState.BossFightConversationInProgress:
			if (ActionIndexCache.act_conversation_naval_start == _purigAgent.GetCurrentAction(0) || ActionIndexCache.act_conversation_naval_idle_loop == _purigAgent.GetCurrentAction(0))
			{
				_purigAgent.SetCurrentActionProgress(0, 1f);
				_purigAgent.SetActionChannel(0, ref ActionIndexCache.act_conversation_normal_loop, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			}
			break;
		case Quest5SetPieceBattleMissionState.BossFightInProgressAsDuel:
			if (_purigAgent == null || !_purigAgent.IsActive())
			{
				OnDuelOver(((MissionBehavior)this).Mission.PlayerTeam.Side);
			}
			else if (Agent.Main == null || !Agent.Main.IsActive())
			{
				OnDuelOver(((MissionBehavior)this).Mission.PlayerEnemyTeam.Side);
			}
			break;
		case Quest5SetPieceBattleMissionState.BossFightInProgressAsAll:
		{
			bool flag = false;
			for (int i = 0; i < _duelPhaseEnemyAgents.Count; i++)
			{
				if (_duelPhaseEnemyAgents[i].IsActive())
				{
					flag = true;
					break;
				}
			}
			if (!flag && (_purigAgent == null || !_purigAgent.IsActive()))
			{
				OnDuelOver(((MissionBehavior)this).Mission.PlayerTeam.Side);
				break;
			}
			bool flag2 = false;
			for (int j = 0; j < _duelPhaseAllyAgents.Count; j++)
			{
				if (_duelPhaseAllyAgents[j].IsActive())
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2 && (Agent.Main == null || !Agent.Main.IsActive()))
			{
				OnDuelOver(((MissionBehavior)this).Mission.PlayerEnemyTeam.Side);
			}
			break;
		}
		case Quest5SetPieceBattleMissionState.End:
			if (_endMissionTimer == null)
			{
				_endMissionTimer = new MissionTimer(2f);
			}
			else
			{
				if (!_endMissionTimer.Check(false) && !_isMissionFailPopUpTriggered)
				{
					break;
				}
				foreach (DialogNotificationHandle item2 in _dialogNotificationHandleCache)
				{
					CampaignInformationManager.ClearDialogNotification(item2, true);
				}
				_dialogNotificationHandleCache.Clear();
				if (_winnerSide == ((MissionBehavior)this).Mission.PlayerTeam.Side && !ShouldMissionContinueFromCheckpoint)
				{
					TriggerPurigsDeadPopUp();
				}
				else
				{
					((MissionBehavior)this).Mission.EndMission();
				}
				State = Quest5SetPieceBattleMissionState.Exit;
			}
			break;
		}
		CheckAndPrintInstructionNotification();
		HandleGunnarMovement();
		HandleIfGunnarFallsIntoTheWater();
	}

	public override void OnRemoveBehavior()
	{
		((MissionBehavior)this).OnRemoveBehavior();
		_navalShipsLogic.ShipAttachmentBrokenEvent -= OnAttachmentBroken;
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
		if ((int)((MissionBehavior)this).Mission.Mode == 4 && _stealthAgents.Contains(affectedAgent))
		{
			_stealthAgents.Remove(affectedAgent);
		}
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (((item != _phase3EnemyReinforcementShip1 && item != _phase3EnemyReinforcementShip2) || _isReinforcementInitialized) && item != _playerShip && _navalAgentsLogic.GetActiveAgentCountOfShip(item) <= 0 && item.HasController)
			{
				DisableShipOrderController(item);
			}
		}
	}

	public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		((MissionBehavior)this).OnObjectUsed(userAgent, usedObject);
		if (userAgent.IsMainAgent && usedObject is ShipDoorUsePoint)
		{
			if (State == Quest5SetPieceBattleMissionState.Phase1StealthPhase)
			{
				State = Quest5SetPieceBattleMissionState.Phase1GoToShipInteriorFadeOut;
				State = Quest5SetPieceBattleMissionState.Phase1GoToShipInteriorFadeOut;
				_phase1EnemyShipToInteriorShipDoorEntity.GetFirstScriptOfType<ShipDoorUsePoint>().SetShipDoorUsePointEnabled(isEnabled: false);
			}
			else if (State == Quest5SetPieceBattleMissionState.Phase1ShipInteriorPhase)
			{
				State = Quest5SetPieceBattleMissionState.Phase1GoBackToShipFadeOut;
				_phase1InteriorToEnemyShip3ShipDoorEntity.GetFirstScriptOfType<ShipDoorUsePoint>().SetShipDoorUsePointEnabled(isEnabled: false);
			}
		}
	}

	public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentTeamChanged(prevTeam, newTeam, agent);
		if (newTeam == ((MissionBehavior)this).Mission.PlayerEnemyTeam && State < Quest5SetPieceBattleMissionState.Phase1ToPhase2FadeOut)
		{
			AgentFlag agentFlags = agent.GetAgentFlags();
			agent.SetAgentFlags((AgentFlag)(agentFlags | 0x10000));
			((AgentBehaviorGroup)agent.GetComponent<CampaignAgentComponent>().CreateAgentNavigator().AddBehaviorGroup<AlarmedBehaviorGroup>()).AddBehavior<CautiousBehavior>();
		}
	}

	public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (affectedAgent.IsMainAgent)
		{
			if (State <= Quest5SetPieceBattleMissionState.BossFightConversationInProgress)
			{
				Agent.Main.Health = Agent.Main.HealthLimit;
				EndMissionWithAutoContinueFromCheckpoint();
			}
			MakeGunnarStopUsingGameObjectBeforeMissionEnd();
		}
		if (_purigShipAgents.Contains(affectedAgent))
		{
			_purigShipAgents.Remove(affectedAgent);
		}
	}

	public override InquiryData OnEndMissionRequest(out bool canLeave)
	{
		MakeGunnarStopUsingGameObjectBeforeMissionEnd();
		return ((MissionLogic)this).OnEndMissionRequest(ref canLeave);
	}

	protected override void OnEndMission()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		foreach (DialogNotificationHandle item in _dialogNotificationHandleCache)
		{
			CampaignInformationManager.ClearDialogNotification(item, true);
		}
		_dialogNotificationHandleCache.Clear();
		MakeGunnarStopUsingGameObjectBeforeMissionEnd();
		((MissionBehavior)this).OnEndMission();
		((Ship)(_playerShip?.ShipOrigin)).Owner = null;
		if (_phase2AllyShip1 != null)
		{
			((Ship)_phase2AllyShip1.ShipOrigin).Owner = null;
		}
		if (_phase2AllyShip2 != null)
		{
			((Ship)_phase2AllyShip2.ShipOrigin).Owner = null;
		}
		if (_phase2AllyShip3 != null)
		{
			((Ship)_phase2AllyShip3.ShipOrigin).Owner = null;
		}
		if (_phase2AllyShip4 != null)
		{
			((Ship)_phase2AllyShip4.ShipOrigin).Owner = null;
		}
		if (_phase2AllyShip5 != null)
		{
			((Ship)_phase2AllyShip5.ShipOrigin).Owner = null;
		}
	}

	public override void OnRetreatMission()
	{
		MakeGunnarStopUsingGameObjectBeforeMissionEnd();
		((MissionLogic)this).OnRetreatMission();
	}

	public override void OnSurrenderMission()
	{
		MakeGunnarStopUsingGameObjectBeforeMissionEnd();
		((MissionLogic)this).OnSurrenderMission();
	}

	private void DeactivateObjectiveIfItIsActive(MissionObjective objective)
	{
		if (objective != null && objective.IsActive)
		{
			_missionObjectiveLogic.CompleteCurrentObjective();
		}
	}

	private void CheckAndPrintInstructionNotification()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		switch (_instructionState)
		{
		case Quest5InstructionState.Approach:
			DisplayCurrentInstructionNotification();
			if (_missionObjectiveLogic != null)
			{
				_approachObjective = new Quest5ApproachObjective(Mission.Current, _playerShip, _approachPointEntity.GetGlobalFrame(), 30f);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_approachObjective);
			}
			_instructionState = Quest5InstructionState.WaitForJump;
			break;
		case Quest5InstructionState.Jump:
			DisplayCurrentInstructionNotification();
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_approachObjective);
				_jumpObjective = new Quest5JumpObjective(Mission.Current, _gunnarAgent);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_jumpObjective);
			}
			_instructionState = Quest5InstructionState.WaitForSwim;
			break;
		case Quest5InstructionState.Swim:
			DisplayCurrentInstructionNotification();
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_jumpObjective);
				_swimObjective = new Quest5SwimObjective(Mission.Current, _gunnarAgent, _phase1EnemyShip4);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_swimObjective);
			}
			_instructionState = Quest5InstructionState.WaitForClearGuards;
			break;
		case Quest5InstructionState.ClearGuards:
			_instructionState = Quest5InstructionState.WaitForCheckInterior;
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_talkWithYourSisterObjective);
				_clearGuardsObjective = new Quest5ClearGuardsObjective(Mission.Current, _stealthAgents);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_clearGuardsObjective);
			}
			break;
		case Quest5InstructionState.CheckInterior:
			DisplayCurrentInstructionNotification();
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_swimObjective);
				GameEntity interiorSpawnPointEntity = Mission.Current.Scene.FindEntityWithTag("phase_1_interior_player_sp");
				_checkInteriorObjective = new Quest5CheckInteriorObjective(Mission.Current, _phase1EnemyShipToInteriorShipDoorEntity, interiorSpawnPointEntity);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_checkInteriorObjective);
			}
			_instructionState = Quest5InstructionState.WaitForTalkSister;
			break;
		case Quest5InstructionState.TalkSister:
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_checkInteriorObjective);
				_talkWithYourSisterObjective = new Quest5TalkWithYourSisterObjective(Mission.Current, SisterAgent);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_talkWithYourSisterObjective);
			}
			_instructionState = Quest5InstructionState.WaitForReturnToDeck;
			break;
		case Quest5InstructionState.ReturnToDeck:
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_talkWithYourSisterObjective);
				_returnToDeckObjective = new Quest5ReturnToDeckObjective(Mission.Current, _phase1InteriorToEnemyShip3ShipDoorEntity, _phase1EnemyShipToInteriorShipDoorEntity);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_returnToDeckObjective);
			}
			_instructionState = Quest5InstructionState.WaitForCutLoose;
			break;
		case Quest5InstructionState.CutLoose:
			DisplayCurrentInstructionNotification();
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_returnToDeckObjective);
				_cutLooseObjective = new Quest5CutLooseObjective(((MissionBehavior)this).Mission, _phase1EnemyShip3.AttachmentMachines, _phase1EnemyShip3.AttachmentPointMachines);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_cutLooseObjective);
			}
			_instructionState = Quest5InstructionState.WaitForGunnarUsesShip;
			break;
		case Quest5InstructionState.GunnarUsesShip:
			if (State == Quest5SetPieceBattleMissionState.Phase2InProgress)
			{
				DisplayCurrentInstructionNotification();
				if (_missionObjectiveLogic != null)
				{
					DeactivateObjectiveIfItIsActive((MissionObjective)(object)_cutLooseObjective);
					_gunnarUsesShipObjective = new Quest5GunnarUsesShipObjective(Mission.Current);
					_missionObjectiveLogic.StartObjective((MissionObjective)(object)_gunnarUsesShipObjective);
				}
				_instructionState = Quest5InstructionState.WaitForEscapeQuietly;
			}
			break;
		case Quest5InstructionState.EscapeQuietly:
			DisplayCurrentInstructionNotification();
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_gunnarUsesShipObjective);
				_escapeObjective = new Quest5EscapeObjective(Mission.Current, GetCurrentGunnarInstructionText(Quest5InstructionState.EscapeQuietly));
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_escapeObjective);
			}
			_instructionState = Quest5InstructionState.WaitForReachAllies;
			break;
		case Quest5InstructionState.ReachAllies:
			DisplayCurrentInstructionNotification();
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_escapeObjective);
				_reachAlliesObjective = new Quest5ReachAlliesObjective(Mission.Current, _phase3TriggerVolumeBox);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_reachAlliesObjective);
			}
			_instructionState = Quest5InstructionState.WaitForDefeatEnemies;
			break;
		case Quest5InstructionState.DefeatEnemies:
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_reachAlliesObjective);
				_defeatEnemiesObjective = new Quest5DefeatEnemiesObjective(Mission.Current, _phase3TotalEnemyCount);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_defeatEnemiesObjective);
			}
			_instructionState = Quest5InstructionState.WaitForDefeatPurigsShip;
			break;
		case Quest5InstructionState.DefeatPurigsShip:
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_defeatEnemiesObjective);
				_defeatPurigsShipObjective = new Quest5DefeatPurigsShipObjective(Mission.Current, _purigShipAgents, Phase4PurigShip);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_defeatPurigsShipObjective);
			}
			_instructionState = Quest5InstructionState.WaitForDefeatPurig;
			break;
		case Quest5InstructionState.DefeatPurig:
			if (_missionObjectiveLogic != null)
			{
				DeactivateObjectiveIfItIsActive((MissionObjective)(object)_defeatPurigsShipObjective);
				_defeatPurigObjective = new Quest5DefeatPurigObjective(Mission.Current, _purigAgent);
				_missionObjectiveLogic.StartObjective((MissionObjective)(object)_defeatPurigObjective);
			}
			_instructionState = Quest5InstructionState.WaitForEnd;
			break;
		case Quest5InstructionState.End:
			DeactivateObjectiveIfItIsActive((MissionObjective)(object)_defeatPurigObjective);
			break;
		case Quest5InstructionState.WaitForJump:
		case Quest5InstructionState.WaitForSwim:
		case Quest5InstructionState.WaitForClearGuards:
		case Quest5InstructionState.WaitForCheckInterior:
		case Quest5InstructionState.WaitForTalkSister:
		case Quest5InstructionState.WaitForReturnToDeck:
		case Quest5InstructionState.WaitForCutLoose:
		case Quest5InstructionState.WaitForGunnarUsesShip:
		case Quest5InstructionState.WaitForEscapeQuietly:
		case Quest5InstructionState.WaitForReachAllies:
		case Quest5InstructionState.WaitForDefeatEnemies:
		case Quest5InstructionState.WaitForDefeatPurigsShip:
		case Quest5InstructionState.WaitForDefeatPurig:
		case Quest5InstructionState.WaitForEnd:
			break;
		}
	}

	private TextObject GetCurrentGunnarInstructionText(Quest5InstructionState instructionState)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		switch (instructionState)
		{
		case Quest5InstructionState.Approach:
		case Quest5InstructionState.WaitForJump:
			return new TextObject("{=Gap3mlD3}Do you see that big cluster of ships back there? That's got to be where they're holding the prisoners.", (Dictionary<string, object>)null);
		case Quest5InstructionState.Jump:
		case Quest5InstructionState.WaitForSwim:
			return new TextObject("{=DQNbUvkL}Into the water! Let's go, while Purig's men are distracted. Swim fast, but keep your distance from any lookouts.", (Dictionary<string, object>)null);
		case Quest5InstructionState.ClearGuards:
		case Quest5InstructionState.WaitForCheckInterior:
			return new TextObject("{=uQjanqh7}Be careful of the guards! Try to take them out without raising an alarm.", (Dictionary<string, object>)null);
		case Quest5InstructionState.CheckInterior:
		case Quest5InstructionState.WaitForCutLoose:
			return new TextObject("{=vOXiHDxu}Very good! Now, get to the hold.", (Dictionary<string, object>)null);
		case Quest5InstructionState.CutLoose:
		case Quest5InstructionState.WaitForGunnarUsesShip:
			return new TextObject("{=Ju7ku4LZ}Well done! But your sister is still within, and we need to get her to safety. Cut the lines tying us to the other ship, and let's be away.", (Dictionary<string, object>)null);
		case Quest5InstructionState.GunnarUsesShip:
		case Quest5InstructionState.WaitForEscapeQuietly:
			return new TextObject("{=P1nDlx4L}Good work! Now, let's get back to our people. The wind and current are in our favor. Even though it's just the two of us, I think we can rejoin Bjolgur and Lahar before they catch us. I'll look to the sails [and take the helm], and you can cut us loose.", (Dictionary<string, object>)null);
		case Quest5InstructionState.EscapeQuietly:
		case Quest5InstructionState.WaitForReachAllies:
			return new TextObject("{=wnhaoGoW}Gods' blood! We can't get past them! They're going to board. Shoot those bastards, cut them down as they come over the side, whatever it takes!", (Dictionary<string, object>)null);
		case Quest5InstructionState.ReachAllies:
		case Quest5InstructionState.WaitForDefeatEnemies:
			return new TextObject("{=igHojAHJ}Hah! We went through their net like a slippery old eel. Bjolgur and Lahar are right over there. Let's turn the tables on those bastards!", (Dictionary<string, object>)null);
		default:
			return TextObject.GetEmpty();
		}
	}

	private void DisplayCurrentInstructionNotification()
	{
		TextObject currentGunnarInstructionText = GetCurrentGunnarInstructionText(_instructionState);
		if (!currentGunnarInstructionText.IsEmpty())
		{
			DialogNotificationHandle item = CampaignInformationManager.AddDialogLine(currentGunnarInstructionText, NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 1000, (NotificationPriority)3);
			_dialogNotificationHandleCache.Add(item);
		}
	}

	private void HandleGunnarMovement()
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_050e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val;
		Vec3 val2;
		switch (_gunnarMovementState)
		{
		case GunnarMovementState.GoToInitialJumpingPosition:
		{
			Agent gunnarAgent8 = _gunnarAgent;
			if (gunnarAgent8 != null && gunnarAgent8.IsUsingGameObject)
			{
				_gunnarAgent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
			}
			EnableRamp();
			_gunnarAgent.ClearTargetFrame();
			new WorldPosition(((MissionBehavior)this).Mission.Scene, JumpOffInitialPosition.GlobalPosition);
			Vec3 val8 = JumpOffInitialPosition.GlobalPosition - _gunnarAgent.Position;
			Agent gunnarAgent9 = _gunnarAgent;
			val = JumpOffInitialPosition.GlobalPosition;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			gunnarAgent9.SetTargetPositionAndDirection(ref asVec, ref val8);
			_gunnarAgent.LookDirection = ((Vec3)(ref val8)).NormalizedCopy();
			_gunnarMovementState = GunnarMovementState.WaitForReachingInitialJumpingPosition;
			break;
		}
		case GunnarMovementState.WaitForReachingInitialJumpingPosition:
		{
			val = _gunnarAgent.Position;
			val2 = JumpOffInitialPosition.GlobalPosition;
			if (((Vec3)(ref val)).NearlyEquals(ref val2, 1f))
			{
				_gunnarMovementState = GunnarMovementState.GoToJumpingTargetPosition;
				break;
			}
			Vec3 val4 = JumpOffInitialPosition.GlobalPosition - _gunnarAgent.Position;
			Agent gunnarAgent5 = _gunnarAgent;
			val2 = JumpOffInitialPosition.GlobalPosition;
			Vec2 asVec = ((Vec3)(ref val2)).AsVec2;
			gunnarAgent5.SetTargetPositionAndDirection(ref asVec, ref val4);
			break;
		}
		case GunnarMovementState.GoToJumpingTargetPosition:
		{
			_gunnarAgent.ClearTargetFrame();
			new WorldPosition(((MissionBehavior)this).Mission.Scene, JumpOffTargetPosition.GlobalPosition);
			Vec3 val7 = JumpOffTargetPosition.GlobalPosition - _gunnarAgent.Position;
			Agent gunnarAgent7 = _gunnarAgent;
			val2 = JumpOffTargetPosition.GlobalPosition;
			Vec2 asVec = ((Vec3)(ref val2)).AsVec2;
			gunnarAgent7.SetTargetPositionAndDirection(ref asVec, ref val7);
			_gunnarAgent.LookDirection = ((Vec3)(ref val7)).NormalizedCopy();
			_gunnarMovementState = GunnarMovementState.WaitForReachingJumpingTargetPosition;
			break;
		}
		case GunnarMovementState.WaitForReachingJumpingTargetPosition:
			val2 = _gunnarAgent.Position;
			val = JumpOffTargetPosition.GlobalPosition;
			if (((Vec3)(ref val2)).NearlyEquals(ref val, 3f))
			{
				if (Agent.Main.IsInWater())
				{
					_gunnarMovementState = GunnarMovementState.SwimToTheHidingSpot;
				}
				else
				{
					Agent gunnarAgent2 = _gunnarAgent;
					val = _gunnarAgent.Position;
					gunnarAgent2.SetTargetPosition(((Vec3)(ref val)).AsVec2);
				}
			}
			_gunnarAgentNavalComponent.SetCanDrown(canDrown: false);
			break;
		case GunnarMovementState.SwimToTheHidingSpot:
		{
			Agent gunnarAgent3 = _gunnarAgent;
			if (gunnarAgent3 != null && gunnarAgent3.IsUsingGameObject)
			{
				_gunnarAgent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
			}
			_gunnarAgent.ClearTargetFrame();
			Vec3 val3 = HidingSpot1Position.GlobalPosition - _gunnarAgent.Position;
			Agent gunnarAgent4 = _gunnarAgent;
			val = HidingSpot1Position.GlobalPosition;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			gunnarAgent4.SetTargetPositionAndDirection(ref asVec, ref val3);
			_gunnarAgent.LookDirection = ((Vec3)(ref val3)).NormalizedCopy();
			_targetClimbingMachine = ((IEnumerable<ClimbingMachine>)_phase1EnemyShip4.ClimbingMachines).First();
			_gunnarMovementStateForClimbingShip = GunnarMovementStateForClimbingShip.Start;
			_gunnarMovementState = GunnarMovementState.WaitForTeleportingToTheHidingSpot;
			break;
		}
		case GunnarMovementState.WaitForTeleportingToTheHidingSpot:
			MakeGunnarClimbToDeck();
			if (_gunnarMovementStateForClimbingShip == GunnarMovementStateForClimbingShip.End)
			{
				_gunnarAgent.SetCrouchMode(true);
				_gunnarAgent.Controller = (AgentControllerType)0;
				_gunnarAgent.SetActionChannel(0, ref ActionIndexCache.act_crouch_walk_idle_unarmed, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				_gunnarMovementState = GunnarMovementState.WaitAtTheHidingSpot;
			}
			break;
		case GunnarMovementState.TeleportToTargetPosition:
		{
			Vec3 globalPosition = HidingSpot1Position.GlobalPosition;
			_gunnarAgent.TeleportToPosition(globalPosition);
			Agent gunnarAgent = _gunnarAgent;
			Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
			val = globalPosition - _gunnarAgent.Position;
			gunnarAgent.SetTargetPositionAndDirection(ref asVec, ref val);
			_gunnarAgent.SetCrouchMode(true);
			_gunnarAgent.Controller = (AgentControllerType)0;
			_gunnarAgent.SetActionChannel(0, ref ActionIndexCache.act_crouch_walk_idle_unarmed, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			_gunnarMovementState = GunnarMovementState.WaitAtTheHidingSpot;
			break;
		}
		case GunnarMovementState.WaitAtTheHidingSpot:
		{
			_gunnarAgent.SetCrouchMode(true);
			Agent gunnarAgent6 = _gunnarAgent;
			val = HidingSpot1Position.GlobalPosition;
			gunnarAgent6.SetTargetPosition(((Vec3)(ref val)).AsVec2);
			_gunnarAgent.SetActionChannel(0, ref ActionIndexCache.act_crouch_walk_idle_unarmed, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_stealthAgents))
			{
				_gunnarAgent.ClearTargetFrame();
				_gunnarAgent.SetCrouchMode(false);
				_gunnarAgent.Controller = (AgentControllerType)1;
				_gunnarAgent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				_gunnarMovementState = GunnarMovementState.GoToTheEscapeShip;
			}
			CheckIfAnEnemyIsAttackingGunnar();
			break;
		}
		case GunnarMovementState.GoToTheEscapeShip:
		{
			_gunnarAgent.ClearTargetFrame();
			WorldPosition val5 = default(WorldPosition);
			((WorldPosition)(ref val5))._002Ector(((MissionBehavior)this).Mission.Scene, GunnarShipUsePosition.origin);
			Vec3 val6 = GunnarShipUsePosition.origin - _gunnarAgent.Position;
			_gunnarAgent.SetScriptedPositionAndDirection(ref val5, MBMath.ToRadians(((Vec3)(ref val6)).RotationX), false, (AIScriptedFrameFlags)0);
			_gunnarAgent.LookDirection = ((Vec3)(ref val6)).NormalizedCopy();
			_gunnarMovementState = GunnarMovementState.WaitForReachingToTheEscapeShip;
			break;
		}
		case GunnarMovementState.WaitForReachingToTheEscapeShip:
			if ((_phase1EnemyShip2 == null || !EscapeShip.GetIsThereActiveBridgeTo(_phase1EnemyShip2)) && EscapeShip.Captain == _gunnarAgent && State == Quest5SetPieceBattleMissionState.Phase2InProgress && (object)_gunnarAgent.CurrentlyUsedGameObject == ((UsableMachine)EscapeShip.ShipControllerMachine).PilotStandingPoint)
			{
				_gunnarMovementState = GunnarMovementState.UseTheEscapeShip;
			}
			break;
		case GunnarMovementState.UseTheEscapeShip:
			HandleEscapeShipMovement();
			EscapeShip.Formation.SetControlledByAI(false, false);
			break;
		case GunnarMovementState.None:
		case GunnarMovementState.End:
			break;
		}
	}

	private void EnableRamp()
	{
		Mission.Current.Scene.FindEntityWithTag("ramp_holder").SetVisibilityExcludeParents(true);
	}

	private void HandleIfGunnarFallsIntoTheWater()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		if (_gunnarAgent == null || !_gunnarAgent.IsActive())
		{
			return;
		}
		Vec3 val;
		switch (State)
		{
		case Quest5SetPieceBattleMissionState.Phase1StealthPhase:
			if (_gunnarFellIntoTheWaterTimer == null)
			{
				if (_gunnarAgent.IsInWater())
				{
					_gunnarFellIntoTheWaterTimer = new MissionTimer(10f);
				}
			}
			else if (_gunnarFellIntoTheWaterTimer.Check(false) && !Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_stealthAgents))
			{
				Vec3 globalPosition = HidingSpot1Position.GlobalPosition;
				val = _gunnarAgent.Position - globalPosition;
				if (((Vec3)(ref val)).LengthSquared > 1f)
				{
					_gunnarAgent.TeleportToPosition(globalPosition);
				}
				Agent gunnarAgent = _gunnarAgent;
				Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
				val = GunnarShipUsePosition.origin - _gunnarAgent.Position;
				val = ((Vec3)(ref val)).NormalizedCopy();
				gunnarAgent.SetTargetPositionAndDirection(ref asVec, ref val);
				_gunnarAgent.SetCrouchMode(true);
			}
			break;
		case Quest5SetPieceBattleMissionState.Phase1EscapePhase:
		case Quest5SetPieceBattleMissionState.Phase2InProgress:
			if (_gunnarFellIntoTheWaterTimer == null)
			{
				if (_gunnarAgent.IsInWater())
				{
					_gunnarFellIntoTheWaterTimer = new MissionTimer(10f);
				}
			}
			else if (_gunnarFellIntoTheWaterTimer.Check(false))
			{
				val = _gunnarAgent.Position - GunnarShipUsePosition.origin;
				if (((Vec3)(ref val)).LengthSquared > 1f)
				{
					_gunnarAgent.TeleportToPosition(GunnarShipUsePosition.origin);
				}
			}
			break;
		}
	}

	private void MakeGunnarClimbToDeck()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		Vec3 val;
		switch (_gunnarMovementStateForClimbingShip)
		{
		case GunnarMovementStateForClimbingShip.Start:
		{
			Scene scene = ((MissionBehavior)this).Mission.Scene;
			gameEntity = ((ScriptComponentBehavior)((UsableMachine)_targetClimbingMachine).PilotStandingPoint).GameEntity;
			WorldPosition val3 = default(WorldPosition);
			((WorldPosition)(ref val3))._002Ector(scene, ((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			_gunnarAgent.SetScriptedPosition(ref val3, true, (AIScriptedFrameFlags)0);
			_gunnarMovementStateForClimbingShip = GunnarMovementStateForClimbingShip.GoingToTheTargetClimbingMachine;
			break;
		}
		case GunnarMovementStateForClimbingShip.GoingToTheTargetClimbingMachine:
		{
			val = _gunnarAgent.Position;
			gameEntity = ((ScriptComponentBehavior)_targetClimbingMachine).GameEntity;
			if (((Vec3)(ref val)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) < 2.5f)
			{
				_gunnarMovementStateForClimbingShip = GunnarMovementStateForClimbingShip.TargetReached;
				break;
			}
			if (_phase1EnemyShip4.GetIsAgentOnShip(_gunnarAgent))
			{
				_gunnarAgent.SetCrouchMode(true);
				_gunnarMovementStateForClimbingShip = GunnarMovementStateForClimbingShip.OnDeck;
				break;
			}
			Agent gunnarAgent2 = _gunnarAgent;
			gameEntity = ((ScriptComponentBehavior)((UsableMachine)_targetClimbingMachine).PilotStandingPoint).GameEntity;
			val = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			gunnarAgent2.SetTargetPosition(((Vec3)(ref val)).AsVec2);
			break;
		}
		case GunnarMovementStateForClimbingShip.TargetReached:
			if (!((UsableMissionObject)((UsableMachine)_targetClimbingMachine).PilotStandingPoint).HasUser)
			{
				_gunnarAgent.UseGameObject((UsableMissionObject)(object)((UsableMachine)_targetClimbingMachine).PilotStandingPoint, -1);
				_gunnarMovementStateForClimbingShip = GunnarMovementStateForClimbingShip.UsingClimbingMachine;
			}
			break;
		case GunnarMovementStateForClimbingShip.UsingClimbingMachine:
			val = _gunnarAgent.Position;
			gameEntity = ((ScriptComponentBehavior)_targetClimbingMachine).GameEntity;
			if (((Vec3)(ref val)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) > 2.5f)
			{
				if (!_gunnarAgent.IsUsingGameObject)
				{
					_gunnarMovementStateForClimbingShip = GunnarMovementStateForClimbingShip.GoingToTheTargetClimbingMachine;
				}
			}
			else if (_phase1EnemyShip4.GetIsAgentOnShip(_gunnarAgent))
			{
				_gunnarAgent.SetCrouchMode(true);
				_gunnarMovementStateForClimbingShip = GunnarMovementStateForClimbingShip.OnDeck;
			}
			break;
		case GunnarMovementStateForClimbingShip.OnDeck:
		{
			_gunnarAgent.ClearTargetFrame();
			Vec3 val2 = HidingSpot1Position.GlobalPosition - _gunnarAgent.Position;
			Agent gunnarAgent = _gunnarAgent;
			val = HidingSpot1Position.GlobalPosition;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			gunnarAgent.SetTargetPositionAndDirection(ref asVec, ref val2);
			_gunnarMovementStateForClimbingShip = GunnarMovementStateForClimbingShip.GoToFinalTargetPoint;
			break;
		}
		case GunnarMovementStateForClimbingShip.GoToFinalTargetPoint:
		{
			val = _gunnarAgent.Position;
			Vec3 globalPosition = HidingSpot1Position.GlobalPosition;
			if (((Vec3)(ref val)).NearlyEquals(ref globalPosition, 1f))
			{
				_gunnarMovementStateForClimbingShip = GunnarMovementStateForClimbingShip.End;
			}
			break;
		}
		case GunnarMovementStateForClimbingShip.None:
		case GunnarMovementStateForClimbingShip.End:
			break;
		}
	}

	private void InitializePhase1Part1()
	{
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		TeamAINavalComponent teamAINavalComponent = new TeamAINavalComponent(((MissionBehavior)this).Mission, ((MissionBehavior)this).Mission.AttackerTeam, 5f, 1f);
		((MissionBehavior)this).Mission.AttackerTeam.AddTeamAI((TeamAIComponent)(object)teamAINavalComponent, false);
		((MissionBehavior)this).Mission.AttackerTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(((MissionBehavior)this).Mission.AttackerTeam));
		TeamAINavalComponent teamAINavalComponent2 = new TeamAINavalComponent(((MissionBehavior)this).Mission, ((MissionBehavior)this).Mission.DefenderTeam, 5f, 1f);
		((MissionBehavior)this).Mission.DefenderTeam.AddTeamAI((TeamAIComponent)(object)teamAINavalComponent2, false);
		((MissionBehavior)this).Mission.DefenderTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(((MissionBehavior)this).Mission.DefenderTeam));
		_playerShip = CreateShip("crusas_roundship_nested_q5", "phase_1_player_ship_sp", _playerFormation);
		_phase1EnemyShip1 = CreateShip("sturgia_heavy_ship", "phase_1_enemy_ship_1_sp_initial", GetAvailableEnemyFormation(), spawnAnchored: false, null, null, checkForFreeArea: false);
		_phase1EnemyShip2 = CreateShip("ship_lodya_storyline", "phase_1_enemy_ship_2_sp", GetAvailableEnemyFormation(), spawnAnchored: true, _phase1EnemyShip2UpgradePieceList, null, checkForFreeArea: false);
		_phase1EnemyShip3 = CreateShip("ship_dromon_storyline", "phase_1_enemy_ship_3_sp", GetAvailableEnemyFormation(), spawnAnchored: true, _escapeShipUpgradePieceList, null, checkForFreeArea: false);
		_phase1EnemyShip4 = CreateShip("ship_birlinn_storyline", "phase_1_enemy_ship_4_sp", GetAvailableEnemyFormation(), spawnAnchored: true, null, null, checkForFreeArea: false);
		_phase1EnemyShip1.SetCanBeTakenOver(value: false);
		_phase1EnemyShip2.SetCanBeTakenOver(value: false);
		_phase1EnemyShip3.SetCanBeTakenOver(value: false);
		_phase1EnemyShip4.SetCanBeTakenOver(value: false);
		_phase1EnemyShipToInteriorShipDoorEntity = Mission.Current.Scene.FindEntityWithTag("phase_1_enemy_ship_3_to_interior_door_tag");
		_phase1EnemyShipToInteriorShipDoorEntity.GetFirstScriptOfType<ShipDoorUsePoint>().SetShipDoorUsePointEnabled(isEnabled: false);
		HandleStealthShipsBridgeConnections();
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)4, true);
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_phase1EnemyShip3.AttachmentMachines)
		{
			WeakGameEntity val = ((ScriptComponentBehavior)item).GameEntity;
			val = ((WeakGameEntity)(ref val)).Parent;
			if (((WeakGameEntity)(ref val)).HasTag("bridge_a"))
			{
				continue;
			}
			val = ((ScriptComponentBehavior)item).GameEntity;
			val = ((WeakGameEntity)(ref val)).Parent;
			if (((WeakGameEntity)(ref val)).HasTag("bridge_b"))
			{
				continue;
			}
			foreach (StandingPoint item2 in (List<StandingPoint>)(object)((UsableMachine)item).StandingPoints)
			{
				((UsableMissionObject)item2).IsDisabledForPlayers = true;
			}
		}
	}

	private void CheckIfAnEnemyIsAttackingGunnar()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (_isMissionFailPopUpTriggered)
		{
			return;
		}
		bool flag = false;
		foreach (Agent stealthAgent in _stealthAgents)
		{
			if (stealthAgent.IsAlarmed())
			{
				Vec3 position = stealthAgent.Position;
				if (((Vec3)(ref position)).Distance(_gunnarAgent.Position) < 2f)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			TriggerMissionFailPopup();
		}
	}

	private void InitializePhase1Part2()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		_phase1PlayerShipSpawnPosition = _playerShip.GlobalFrame.origin;
		_phase1EnemyShip1.SetAnchor(isAnchored: true);
		_phase1EnemyShip1.ShipOrder.SetShipStopOrder();
		_phase1EnemyShip1.SetController(ShipControllerType.AI);
		_phase1EnemyShip1.SetShipOrderActive(isOrderActive: false);
		_phase1EnemyShip2.SetAnchor(isAnchored: true);
		_phase1EnemyShip2.ShipOrder.SetShipStopOrder();
		_phase1EnemyShip2.SetController(ShipControllerType.AI);
		_phase1EnemyShip2.SetShipOrderActive(isOrderActive: false);
		_phase1EnemyShip3.SetAnchor(isAnchored: true);
		_phase1EnemyShip3.ShipOrder.SetShipStopOrder();
		_phase1EnemyShip3.SetController(ShipControllerType.AI);
		_phase1EnemyShip3.SetShipOrderActive(isOrderActive: false);
		_phase1EnemyShip4.SetAnchor(isAnchored: true);
		_phase1EnemyShip4.ShipOrder.SetShipStopOrder();
		_phase1EnemyShip4.SetController(ShipControllerType.AI);
		_phase1EnemyShip4.SetShipOrderActive(isOrderActive: false);
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_playerShip.AttachmentMachines)
		{
			foreach (StandingPoint item2 in (List<StandingPoint>)(object)((UsableMachine)item).StandingPoints)
			{
				((UsableMissionObject)item2).IsDisabledForPlayers = true;
			}
		}
		foreach (ShipAttachmentMachine item3 in (List<ShipAttachmentMachine>)(object)_phase1EnemyShip1.AttachmentMachines)
		{
			foreach (StandingPoint item4 in (List<StandingPoint>)(object)((UsableMachine)item3).StandingPoints)
			{
				((UsableMissionObject)item4).IsDisabledForPlayers = true;
			}
		}
		foreach (ShipAttachmentMachine item5 in (List<ShipAttachmentMachine>)(object)_phase1EnemyShip2.AttachmentMachines)
		{
			WeakGameEntity val = ((ScriptComponentBehavior)item5).GameEntity;
			val = ((WeakGameEntity)(ref val)).Parent;
			if (((WeakGameEntity)(ref val)).HasTag("bridge_a"))
			{
				continue;
			}
			val = ((ScriptComponentBehavior)item5).GameEntity;
			val = ((WeakGameEntity)(ref val)).Parent;
			if (((WeakGameEntity)(ref val)).HasTag("bridge_b"))
			{
				continue;
			}
			val = ((ScriptComponentBehavior)item5).GameEntity;
			val = ((WeakGameEntity)(ref val)).Parent;
			if (((WeakGameEntity)(ref val)).HasTag("bridge_c"))
			{
				continue;
			}
			foreach (StandingPoint item6 in (List<StandingPoint>)(object)((UsableMachine)item5).StandingPoints)
			{
				((UsableMissionObject)item6).IsDisabledForPlayers = true;
			}
		}
		foreach (ClimbingMachine item7 in (List<ClimbingMachine>)(object)_phase1EnemyShip1.ClimbingMachines)
		{
			foreach (StandingPoint item8 in (List<StandingPoint>)(object)((UsableMachine)item7).StandingPoints)
			{
				((UsableMissionObject)item8).IsDisabledForPlayers = true;
			}
		}
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		SpawnPhase1AllyTroops();
		SpawnPhase1EnemyTroops();
		((MissionBehavior)this).Mission.PlayerTeam.SetPlayerRole(true, true);
		Agent.Main.SetClothingColor1(4279111698u);
		Agent.Main.SetClothingColor2(4279111698u);
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(GetScriptedStealthEquipment());
		_gunnarAgent.UpdateSpawnEquipmentAndRefreshVisuals(GetScriptedStealthEquipment());
		_navalAgentsLogic.AssignCaptainToShipForDeploymentMode(Agent.Main, _playerShip, _playerShip);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase1EnemyShip1);
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
		Mission.Current.OnDeploymentFinished();
		Mission.Current.Scene.FindEntityWithTag("phase_2_barricade").SetVisibilityExcludeParents(false);
		RemoveShipControlPointDescriptionOfAllEnemyShips();
	}

	private void SpawnPhase1AllyTroops()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, CharacterObject.PlayerCharacter, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
		_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, Hero.MainHero.Culture.BasicTroop, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
		_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, Hero.MainHero.Culture.BasicTroop, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
		_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, Hero.MainHero.Culture.BasicTroop, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
		SpawnGunnarOnShip(_playerShip);
		SpawnCrusasOnShip(_playerShip);
		_crusasAgent.UpdateSpawnEquipmentAndRefreshVisuals(MBObjectManager.Instance.GetObject<MBEquipmentRoster>("npc_merchant_equipment_empire").DefaultEquipment);
		_gunnarAgent.SetMortalityState((MortalityState)2);
		_playerShip.Formation.PlayerOwner = Agent.Main;
	}

	private void SpawnPhase1EnemyTroops()
	{
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Expected O, but got Unknown
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Expected O, but got Unknown
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Expected O, but got Unknown
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).Mission.Scene.GetAllEntitiesWithScriptComponent<DynamicPatrolAreaParent>(ref _dynamicPatrolAreas);
		CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>("sea_hound_captivity");
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase1EnemyShip1, 7);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase1EnemyShip2, 6);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase1EnemyShip3, 100);
		Vec2 asVec;
		foreach (GameEntity dynamicPatrolArea in _dynamicPatrolAreas)
		{
			if (((MissionObject)dynamicPatrolArea.GetFirstScriptOfType<DynamicPatrolAreaParent>()).IsDisabled)
			{
				continue;
			}
			IEnumerable<GameEntity> children = dynamicPatrolArea.GetChildren();
			bool flag = false;
			MissionShip shipOfDynamicPartolArea = GetShipOfDynamicPartolArea(dynamicPatrolArea);
			foreach (GameEntity item in children)
			{
				PatrolPoint firstScriptOfType = item.GetChild(0).GetFirstScriptOfType<PatrolPoint>();
				shipOfDynamicPartolArea.Formation.JoinDetachment((IDetachment)(object)item.GetFirstScriptOfType<UsablePlace>());
				if (firstScriptOfType == null || flag || ((MissionObject)firstScriptOfType).IsDisabled || string.IsNullOrEmpty(firstScriptOfType.SpawnGroupTag))
				{
					continue;
				}
				Equipment val2 = Extensions.GetRandomElementInefficiently<Equipment>(((BasicCharacterObject)val).BattleEquipments).Clone(false);
				for (int i = 0; i < 12; i++)
				{
					if (i != 0 && i != 1 && i != 2 && i != 3 && i != 4)
					{
						continue;
					}
					EquipmentElement val3 = val2[i];
					if (((EquipmentElement)(ref val3)).IsEmpty)
					{
						continue;
					}
					val3 = val2[i];
					if (((EquipmentElement)(ref val3)).Item.WeaponComponent != null)
					{
						val3 = val2[i];
						if (((EquipmentElement)(ref val3)).Item.WeaponComponent.PrimaryWeapon.IsShield)
						{
							val2[i] = EquipmentElement.Invalid;
						}
					}
				}
				AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam);
				MatrixFrame globalFrame = item.GetGlobalFrame();
				AgentBuildData obj2 = obj.InitialPosition(ref globalFrame.origin);
				MatrixFrame globalFrame2 = item.GetGlobalFrame();
				asVec = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
				AgentBuildData val4 = obj2.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false)
					.Equipment(val2);
				Agent val5 = ((MissionBehavior)this).Mission.SpawnAgent(val4, false);
				MBActionSet actionSet = MBGlobals.GetActionSet("as_human_hideout_bandit");
				AnimationSystemData val6 = MonsterExtensions.FillAnimationSystemData(val4.AgentMonster, actionSet, ((BasicCharacterObject)val).GetStepSize(), false);
				val5.SetActionSet(ref val6);
				AgentFlag agentFlags = val5.GetAgentFlags();
				val5.SetAgentFlags((AgentFlag)(agentFlags | 0x10000));
				AgentNavigator obj3 = val5.GetComponent<CampaignAgentComponent>().CreateAgentNavigator();
				((AgentBehaviorGroup)obj3.AddBehaviorGroup<AlarmedBehaviorGroup>()).AddBehavior<CautiousBehavior>();
				((AgentBehaviorGroup)obj3.AddBehaviorGroup<DailyBehaviorGroup>()).AddBehavior<PatrolAgentBehavior>().SetDynamicPatrolArea(dynamicPatrolArea);
				_stealthAgents.Add(val5);
				flag = true;
			}
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)((UsableMachine)_phase1EnemyShip1.ShipControllerMachine).PilotStandingPoint).GameEntity;
		MatrixFrame globalFrame3 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		AgentBuildData obj4 = new AgentBuildData((BasicCharacterObject)(object)_slaveTraderCharacter).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)_slaveTraderCharacter, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam).InitialPosition(ref globalFrame3.origin);
		asVec = ((Vec3)(ref globalFrame3.rotation.f)).AsVec2;
		AgentBuildData val7 = obj4.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false);
		_slaveTraderAgent = ((MissionBehavior)this).Mission.SpawnAgent(val7, false);
		_navalAgentsLogic.AddAgentToShip(_slaveTraderAgent, _phase1EnemyShip1);
		MBActionSet actionSet2 = MBGlobals.GetActionSet("as_human_hideout_bandit");
		AnimationSystemData val8 = MonsterExtensions.FillAnimationSystemData(val7.AgentMonster, actionSet2, ((BasicCharacterObject)val).GetStepSize(), false);
		_slaveTraderAgent.SetActionSet(ref val8);
		_slaveTraderAgent.SetAgentFlags((AgentFlag)(_slaveTraderAgent.GetAgentFlags() & -65561));
		Queue<MatrixFrame> queue = new Queue<MatrixFrame>();
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_phase1EnemyShip1.AttachmentPointMachines)
		{
			gameEntity = ((ScriptComponentBehavior)((IEnumerable<StandingPoint>)((UsableMachine)item2).StandingPoints).First()).GameEntity;
			queue.Enqueue(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame());
		}
		for (int j = 0; j < _slaveTraderShipOarsmen.Length; j++)
		{
			MatrixFrame val9 = queue.Dequeue();
			AgentBuildData obj5 = new AgentBuildData((BasicCharacterObject)(object)_slaveTraderCharacter).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)_slaveTraderCharacter, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam).InitialPosition(ref val9.origin);
			asVec = ((Vec3)(ref val9.rotation.f)).AsVec2;
			AgentBuildData val10 = obj5.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false);
			Agent val11 = ((MissionBehavior)this).Mission.SpawnAgent(val10, false);
			_slaveTraderShipOarsmen[j] = val11;
			_navalAgentsLogic.AddAgentToShip(val11, _phase1EnemyShip1);
			val11.SetActionSet(ref val8);
			val11.SetAgentFlags((AgentFlag)(val11.GetAgentFlags() & -65561));
		}
	}

	private void DisableSlaveTraderShipAgents()
	{
		_slaveTraderAgent.SetTeam(Team.Invalid, true);
		for (int i = 0; i < _slaveTraderShipOarsmen.Length; i++)
		{
			_slaveTraderShipOarsmen[i].SetTeam(Team.Invalid, true);
		}
	}

	private MissionShip GetShipOfDynamicPartolArea(GameEntity dynamicPatrolArea)
	{
		if (dynamicPatrolArea.Parent.Parent.Name.Equals(_phase1EnemyShip2.MissionShipObject.Prefab))
		{
			return _phase1EnemyShip2;
		}
		if (dynamicPatrolArea.Parent.Parent.Name.Equals(_phase1EnemyShip3.MissionShipObject.Prefab))
		{
			return _phase1EnemyShip3;
		}
		if (dynamicPatrolArea.Parent.Parent.Name.Equals(_phase1EnemyShip4.MissionShipObject.Prefab))
		{
			return _phase1EnemyShip4;
		}
		return null;
	}

	private void HandleStealthShipsBridgeConnections()
	{
		if (_phase1EnemyShip2 != null && _phase1EnemyShip3 != null && _phase1EnemyShip4 != null && !_talkedWithSister)
		{
			_phase1EnemyShip3.TryToMaintainConnectionToAnotherShip(_phase1EnemyShip2, forceBridge: true, unbreakableBridge: true);
			_phase1EnemyShip4.TryToMaintainConnectionToAnotherShip(_phase1EnemyShip2, forceBridge: true, unbreakableBridge: true);
		}
	}

	private void HandleEscapeShipInteriorDoorUsage()
	{
		_phase1EnemyShipToInteriorShipDoorEntity.GetFirstScriptOfType<ShipDoorUsePoint>().SetShipDoorUsePointEnabled(Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_stealthAgents));
	}

	private void OnPlayerShipReachedApproachDistance()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		State = Quest5SetPieceBattleMissionState.Phase1SwimmingPhase;
		_gunnarMovementState = GunnarMovementState.GoToInitialJumpingPosition;
		if (Agent.Main.IsUsingGameObject)
		{
			Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		Agent crusasAgent = _crusasAgent;
		if (crusasAgent != null && crusasAgent.IsUsingGameObject)
		{
			_crusasAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		Agent slaveTraderAgent = _slaveTraderAgent;
		if (slaveTraderAgent != null && slaveTraderAgent.IsUsingGameObject)
		{
			_slaveTraderAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		_playerShip.SetCustomSailSetting(enableCustomSailSetting: true, SailInput.Raised);
		_playerShip.ShipOrder.SetShipStopOrder();
		_playerShip.SetAnchor(isAnchored: true);
		CalculateBuySlaveConversationPoint();
		_crusasAgent.ClearTargetFrame();
		_slaveTraderAgent.ClearTargetFrame();
		WorldPosition val = default(WorldPosition);
		((WorldPosition)(ref val))._002Ector(((MissionBehavior)this).Mission.Scene, ((WeakGameEntity)(ref _crusasConversationPointFrame)).GetGlobalFrame().origin);
		Vec3 val2 = ((WeakGameEntity)(ref _crusasConversationPointFrame)).GetGlobalFrame().origin - _crusasAgent.Position;
		float num = MBMath.ToRadians(((Vec3)(ref val2)).RotationX);
		_crusasAgent.SetScriptedPositionAndDirection(ref val, num, true, (AIScriptedFrameFlags)0);
		WorldPosition val3 = default(WorldPosition);
		((WorldPosition)(ref val3))._002Ector(((MissionBehavior)this).Mission.Scene, ((WeakGameEntity)(ref _slaveTraderConversationPointFrame)).GetGlobalFrame().origin);
		val2 = ((WeakGameEntity)(ref _slaveTraderConversationPointFrame)).GetGlobalFrame().origin - _slaveTraderAgent.Position;
		float num2 = MBMath.ToRadians(((Vec3)(ref val2)).RotationX);
		_slaveTraderAgent.SetScriptedPositionAndDirection(ref val3, num2, false, (AIScriptedFrameFlags)0);
		_crusasAgent.SetLookAgent(_slaveTraderAgent);
		_slaveTraderAgent.SetLookAgent(_crusasAgent);
		MakeShipOarsInvisible(_playerShip);
	}

	private void InitializeStealthPhasePart1()
	{
		if (_playerShip == null)
		{
			_isCheckpointInitialize = true;
			TeamAINavalComponent teamAINavalComponent = new TeamAINavalComponent(((MissionBehavior)this).Mission, ((MissionBehavior)this).Mission.AttackerTeam, 5f, 1f);
			((MissionBehavior)this).Mission.AttackerTeam.AddTeamAI((TeamAIComponent)(object)teamAINavalComponent, false);
			((MissionBehavior)this).Mission.AttackerTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(((MissionBehavior)this).Mission.AttackerTeam));
			TeamAINavalComponent teamAINavalComponent2 = new TeamAINavalComponent(((MissionBehavior)this).Mission, ((MissionBehavior)this).Mission.DefenderTeam, 5f, 1f);
			((MissionBehavior)this).Mission.DefenderTeam.AddTeamAI((TeamAIComponent)(object)teamAINavalComponent2, false);
			((MissionBehavior)this).Mission.DefenderTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(((MissionBehavior)this).Mission.DefenderTeam));
			_playerShip = CreateShip("crusas_roundship_nested_q5", "phase_1_player_ship_sp", _playerFormation);
			_phase1EnemyShip1 = CreateShip("sturgia_heavy_ship", "phase_1_enemy_ship_1_sp", GetAvailableEnemyFormation(), spawnAnchored: true, null, null, checkForFreeArea: false);
			_phase1EnemyShip2 = CreateShip("ship_lodya_storyline", "phase_1_enemy_ship_2_sp", GetAvailableEnemyFormation(), spawnAnchored: true, _phase1EnemyShip2UpgradePieceList, null, checkForFreeArea: false);
			_phase1EnemyShip3 = CreateShip("ship_dromon_storyline", "phase_1_enemy_ship_3_sp", GetAvailableEnemyFormation(), spawnAnchored: true, _escapeShipUpgradePieceList, null, checkForFreeArea: false);
			_phase1EnemyShip4 = CreateShip("ship_birlinn_storyline", "phase_1_enemy_ship_4_sp", GetAvailableEnemyFormation(), spawnAnchored: true, null, null, checkForFreeArea: false);
			_phase1EnemyShip1.SetCanBeTakenOver(value: false);
			_phase1EnemyShip2.SetCanBeTakenOver(value: false);
			_phase1EnemyShip3.SetCanBeTakenOver(value: false);
			_phase1EnemyShip4.SetCanBeTakenOver(value: false);
			_phase1EnemyShipToInteriorShipDoorEntity = Mission.Current.Scene.FindEntityWithTag("phase_1_enemy_ship_3_to_interior_door_tag");
			_phase1EnemyShipToInteriorShipDoorEntity.GetFirstScriptOfType<ShipDoorUsePoint>().SetShipDoorUsePointEnabled(isEnabled: false);
			HandleStealthShipsBridgeConnections();
			((MissionBehavior)this).Mission.SetMissionMode((MissionMode)4, true);
		}
	}

	private void InitializeStealthPhasePart2()
	{
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		if (_isCheckpointInitialize)
		{
			_phase1EnemyShip1.SetAnchor(isAnchored: true);
			_phase1EnemyShip1.ShipOrder.SetShipStopOrder();
			_phase1EnemyShip1.SetController(ShipControllerType.AI);
			_phase1EnemyShip1.SetShipOrderActive(isOrderActive: false);
			_phase1EnemyShip2.SetAnchor(isAnchored: true);
			_phase1EnemyShip2.ShipOrder.SetShipStopOrder();
			_phase1EnemyShip2.SetController(ShipControllerType.AI);
			_phase1EnemyShip2.SetShipOrderActive(isOrderActive: false);
			_phase1EnemyShip3.SetAnchor(isAnchored: true);
			_phase1EnemyShip3.ShipOrder.SetShipStopOrder();
			_phase1EnemyShip3.SetController(ShipControllerType.AI);
			_phase1EnemyShip3.SetShipOrderActive(isOrderActive: false);
			_phase1EnemyShip4.SetAnchor(isAnchored: true);
			_phase1EnemyShip4.ShipOrder.SetShipStopOrder();
			_phase1EnemyShip4.SetController(ShipControllerType.AI);
			_phase1EnemyShip4.SetShipOrderActive(isOrderActive: false);
			foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_phase1EnemyShip1.AttachmentMachines)
			{
				foreach (StandingPoint item2 in (List<StandingPoint>)(object)((UsableMachine)item).StandingPoints)
				{
					((UsableMissionObject)item2).IsDisabledForPlayers = true;
				}
			}
			foreach (ShipAttachmentMachine item3 in (List<ShipAttachmentMachine>)(object)_phase1EnemyShip2.AttachmentMachines)
			{
				WeakGameEntity val = ((ScriptComponentBehavior)item3).GameEntity;
				val = ((WeakGameEntity)(ref val)).Parent;
				if (((WeakGameEntity)(ref val)).HasTag("bridge_a"))
				{
					continue;
				}
				val = ((ScriptComponentBehavior)item3).GameEntity;
				val = ((WeakGameEntity)(ref val)).Parent;
				if (((WeakGameEntity)(ref val)).HasTag("bridge_b"))
				{
					continue;
				}
				val = ((ScriptComponentBehavior)item3).GameEntity;
				val = ((WeakGameEntity)(ref val)).Parent;
				if (((WeakGameEntity)(ref val)).HasTag("bridge_c"))
				{
					continue;
				}
				foreach (StandingPoint item4 in (List<StandingPoint>)(object)((UsableMachine)item3).StandingPoints)
				{
					((UsableMissionObject)item4).IsDisabledForPlayers = true;
				}
			}
			foreach (ClimbingMachine item5 in (List<ClimbingMachine>)(object)_phase1EnemyShip1.ClimbingMachines)
			{
				foreach (StandingPoint item6 in (List<StandingPoint>)(object)((UsableMachine)item5).StandingPoints)
				{
					((UsableMissionObject)item6).IsDisabledForPlayers = true;
				}
			}
			Mission.Current.OnDeploymentFinished();
			SpawnPhase1AllyTroops();
			SpawnPhase1EnemyTroops();
			((MissionBehavior)this).Mission.PlayerTeam.SetPlayerRole(true, true);
			_playerShip.Formation.PlayerOwner = Agent.Main;
			Agent.Main.SetClothingColor1(4279111698u);
			Agent.Main.SetClothingColor2(4279111698u);
			Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(GetScriptedStealthEquipment());
			_gunnarAgent.UpdateSpawnEquipmentAndRefreshVisuals(GetScriptedStealthEquipment());
			_gunnarMovementState = GunnarMovementState.TeleportToTargetPosition;
			HandleGunnarMovement();
			GameEntity val2 = Mission.Current.Scene.FindEntityWithTag("sp_player_stealth_checkpoint");
			Agent.Main.TeleportToPosition(val2.GlobalPosition);
			Mission.Current.Scene.FindEntityWithTag("phase_2_barricade").SetVisibilityExcludeParents(false);
			_isCheckpointInitialize = false;
			_instructionState = Quest5InstructionState.ClearGuards;
			Agent.Main.SetCrouchMode(true);
			RemoveShipControlPointDescriptionOfAllEnemyShips();
		}
		foreach (DialogNotificationHandle item7 in _dialogNotificationHandleCache)
		{
			CampaignInformationManager.ClearDialogNotification(item7, true);
		}
		_dialogNotificationHandleCache.Clear();
	}

	private void MovePhase1EnemyShip1ToItsTargetPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = _phase1EnemyShip1TargetEntity.GetGlobalFrame();
		Vec3 val;
		Vec2 asVec;
		if (((Vec3)(ref globalFrame.origin)).Distance(_phase1EnemyShip1.GlobalFrame.origin) <= 2f)
		{
			_phase1EnemyShip1.ShipOrder.SetShipStopOrder();
			_phase1EnemyShip1.SetAnchor(isAnchored: true);
			globalFrame = _phase1EnemyShip1TargetEntity.GetGlobalFrame();
			Vec2 position = ((Vec3)(ref globalFrame.origin)).AsVec2;
			val = _phase1EnemyShip1TargetEntity.GetGlobalFrame().origin - _phase1EnemyShip1InitialSpawnEntity.GetGlobalFrame().origin;
			asVec = ((Vec3)(ref val)).AsVec2;
			Vec2 direction = ((Vec2)(ref asVec)).Normalized();
			_phase1EnemyShip1.SetAnchorFrame(in position, in direction);
			_phase1EnemyShip1.ShipOrder.SetOrderOarsmenLevel(0);
		}
		else
		{
			globalFrame = _phase1EnemyShip1TargetEntity.GetGlobalFrame();
			Vec2 asVec2 = ((Vec3)(ref globalFrame.origin)).AsVec2;
			val = _phase1EnemyShip1TargetEntity.GetGlobalFrame().origin - _phase1EnemyShip1InitialSpawnEntity.GetGlobalFrame().origin;
			asVec = ((Vec3)(ref val)).AsVec2;
			Vec2 targetDirection = ((Vec2)(ref asVec)).Normalized();
			_phase1EnemyShip1.ShipOrder.SetShipMovementOrder(asVec2, in targetDirection);
		}
	}

	private void InitializeShipInteriorPhase()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		Mission.Current.Scene.SetAtmosphereWithName("TOD_01_00_SemiCloudy");
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().DisableStartConversation(true);
		GameEntity val = Mission.Current.Scene.FindEntityWithTag("phase_1_interior_player_sp");
		GameEntity obj = Mission.Current.Scene.FindEntityWithTag("phase_1_interior_sister_sp");
		Agent.Main.TeleportToPosition(val.GlobalPosition);
		Vec3 globalPosition = obj.GlobalPosition;
		MatrixFrame globalFrame = obj.GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Equipment val2 = StoryModeHeroes.LittleSister.CivilianEquipment.Clone(false);
		for (int i = 0; i < 5; i++)
		{
			val2[i] = EquipmentElement.Invalid;
		}
		val2[5] = EquipmentElement.Invalid;
		val2[9] = EquipmentElement.Invalid;
		StoryModeHeroes.LittleSister.HitPoints = StoryModeHeroes.LittleSister.WoundedHealthLimit - 1;
		AgentBuildData val3 = new AgentBuildData((BasicCharacterObject)(object)StoryModeHeroes.LittleSister.CharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, StoryModeHeroes.LittleSister.CharacterObject, -1, default(UniqueTroopDescriptor), false, true)).Team(((MissionBehavior)this).Mission.PlayerTeam).InitialPosition(ref globalPosition)
			.InitialDirection(ref asVec)
			.Equipment(val2)
			.NoHorses(true)
			.NoWeapons(false);
		SisterAgent = Mission.Current.SpawnAgent(val3, false);
		SisterAgent.SetMortalityState((MortalityState)2);
		_mainAgentEquipmentCopyForInteriorMission = Agent.Main.SpawnEquipment.Clone(false);
		Equipment val4 = Agent.Main.SpawnEquipment.Clone(false);
		for (int j = 0; j < 12; j++)
		{
			if (j == 0 || j == 1 || j == 2 || j == 3 || j == 4)
			{
				val4[j] = EquipmentElement.Invalid;
			}
		}
		Agent.Main.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)1);
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(val4);
		_instructionState = Quest5InstructionState.TalkSister;
		Mission.Current.SetMissionMode((MissionMode)0, false);
		State = Quest5SetPieceBattleMissionState.Phase1GoToShipInteriorFadeIn;
	}

	private void InitializeGoBackToShip()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_stealthAgents))
		{
			_gunnarAgent.TeleportToPosition(GunnarShipUsePosition.origin);
			_gunnarMovementState = GunnarMovementState.WaitForReachingToTheEscapeShip;
		}
		Mission.Current.Scene.SetAtmosphereWithName("TOD_naval_03_00_sunset");
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().DisableStartConversation(true);
		SisterAgent.SetMortalityState((MortalityState)0);
		SisterAgent.FadeOut(true, false);
		Agent.Main.TeleportToPosition(_phase1EnemyShipToInteriorShipDoorEntity.GlobalPosition);
		Mission.Current.Scene.FindEntityWithTag("phase_2_barricade").SetVisibilityExcludeParents(true);
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)4, false);
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(_mainAgentEquipmentCopyForInteriorMission);
		State = Quest5SetPieceBattleMissionState.Phase1GoBackToShipFadeIn;
	}

	public void GetIntendedMainAgentDirectionForPhase1InteriorTeleport(out Vec3 mainAgentDirection)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		mainAgentDirection = SisterAgent.Position - Agent.Main.Position;
	}

	public void GetIntendedMainAgentDirectionForPhase1EscapeShipTeleport(out Vec3 mainAgentDirection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		mainAgentDirection = Agent.Main.Position - _gunnarAgent.Position;
	}

	public void TriggerPhase1InitializeShipInteriorPhase()
	{
		State = Quest5SetPieceBattleMissionState.Phase1InitializeShipInteriorPhase;
	}

	public void CompletePhase1GoToShipInteriorTransition()
	{
		State = Quest5SetPieceBattleMissionState.Phase1ShipInteriorPhase;
	}

	public void TriggerPhase1InitializeGoBackToShipPhase()
	{
		State = Quest5SetPieceBattleMissionState.Phase1InitializeGoBackToShip;
	}

	public void CompletePhase1InitializeGoBackToShipTransition()
	{
		State = Quest5SetPieceBattleMissionState.Phase1EscapePhase;
		HandlePlayersBridgeAndControlPointUsagesForPhase1EscapePhase();
	}

	public void SetTalkedWithSister()
	{
		_talkedWithSister = true;
		DeactivateObjectiveIfItIsActive((MissionObjective)(object)_talkWithYourSisterObjective);
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().DisableStartConversation(true);
		Phase1InteriorCameraSisterEntity = null;
		_instructionState = Quest5InstructionState.ReturnToDeck;
	}

	private void CalculateBuySlaveConversationPoint()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		foreach (ShipAttachmentPointMachine item in (List<ShipAttachmentPointMachine>)(object)_playerShip.AttachmentPointMachines)
		{
			foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_phase1EnemyShip1.AttachmentPointMachines)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				ref Vec3 origin = ref globalFrame.origin;
				gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
				float num2 = ((Vec3)(ref origin)).Distance(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin);
				if (num > num2)
				{
					_crusasConversationPointFrame = ((ScriptComponentBehavior)item).GameEntity;
					_slaveTraderConversationPointFrame = ((ScriptComponentBehavior)item2).GameEntity;
					num = num2;
				}
			}
		}
	}

	private void AddConversationSounds()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected O, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Expected O, but got Unknown
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Expected O, but got Unknown
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=kAAkgKFB}Ahoy! Who approaches?", (Dictionary<string, object>)null), (NotificationPriority)2, _slaveTraderCharacter));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=61hcBa4X}I am Crusas Salautas. I seek Purig of Agilting.", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Prusas.CharacterObject));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=JAtDE00L}This is his ship, but he's away. Should be back shortly, though - we signalled him. Keep your distance for now.", (Dictionary<string, object>)null), (NotificationPriority)2, _slaveTraderCharacter));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=JPVD5sfc}I am one of Purig's longtime customers, and I am in a bit of a hurry. I made arrangements weeks ago to buy his merchandise. How long is Purig going to be? Can I come aboard?", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Prusas.CharacterObject));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=MNnk6LAa}You'll need to be patient, friend. Purig's instructions were to let no one aboard. But he won't be long. He's just offshore, out looking for prey.", (Dictionary<string, object>)null), (NotificationPriority)2, _slaveTraderCharacter));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=wJZTakoT}How many do you have to sell?", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Prusas.CharacterObject));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=4Z7a0Kre}Several score, all in good health. We've been feeding them well, sparing no expense. We take pride in our work.", (Dictionary<string, object>)null), (NotificationPriority)2, _slaveTraderCharacter));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=XEbbugis}That's fine, but I was expecting more.", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Prusas.CharacterObject));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=AXz58qHq}You're not the only buyer, my friend! Mines, buildings, repairs... Even on the mainland, mix a handful of our fellows in with some convicts or war captives, and who's to notice?", (Dictionary<string, object>)null), (NotificationPriority)2, _slaveTraderCharacter));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=Zu3lj2s1}So... Can we talk price?", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Prusas.CharacterObject));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=aYmx5ODE}You'll need to wait for our master to return before you start bargaining. Don't push your friendship with Purig too much, though - he's got expensive tastes. He likes to see the envy in other men's eyes when the sun sparkles off his fine golden helm.", (Dictionary<string, object>)null), (NotificationPriority)2, _slaveTraderCharacter));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=dndcy626}I don't like just to sit here idly. Maybe I can come aboard and inspect some of the captives? I can conclude the deal more quickly when your master arrives, and let him get back to his hunting.", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Prusas.CharacterObject));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=ediTKoqo}My instructions were clear. No one aboard the ship.", (Dictionary<string, object>)null), (NotificationPriority)2, _slaveTraderCharacter));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=csvVz5f2}The air is stifling. I hope you've been letting the captives up on deck? No signs of disease?", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Prusas.CharacterObject));
		_conversationSounds.Enqueue(new ConversationSound(new TextObject("{=aKq3AMpG}If you think they're sick you're welcome not to buy any.", (Dictionary<string, object>)null), (NotificationPriority)2, _slaveTraderCharacter));
	}

	private void CheckAndPlayCrusasAndSlaveTraderConversationSound()
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val;
		if (_crusasAndSeaHoundMovedToTheConversationPoints)
		{
			val = Agent.Main.Position;
			if (((Vec3)(ref val)).Distance(_playerShip.GetCaptainSpawnGlobalFrame().origin) < 30f)
			{
				if (!Extensions.IsEmpty<ConversationSound>((IEnumerable<ConversationSound>)_conversationSounds))
				{
					ConversationSound conversationSound = _conversationSounds.Dequeue();
					DialogNotificationHandle item = CampaignInformationManager.AddDialogLine(conversationSound.Line, conversationSound.Character, (Equipment)null, 0, conversationSound.Priority);
					_dialogNotificationHandleCache.Add(item);
				}
				return;
			}
			foreach (DialogNotificationHandle item2 in _dialogNotificationHandleCache)
			{
				CampaignInformationManager.ClearDialogNotification(item2, true);
			}
			_dialogNotificationHandleCache.Clear();
			return;
		}
		val = _crusasAgent.Position;
		Vec2 asVec = ((Vec3)(ref val)).AsVec2;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref _crusasConversationPointFrame)).GetGlobalFrame();
		if (((Vec2)(ref asVec)).NearlyEquals(((Vec3)(ref globalFrame.origin)).AsVec2, 3f))
		{
			val = _slaveTraderAgent.Position;
			asVec = ((Vec3)(ref val)).AsVec2;
			globalFrame = ((WeakGameEntity)(ref _slaveTraderConversationPointFrame)).GetGlobalFrame();
			if (((Vec2)(ref asVec)).NearlyEquals(((Vec3)(ref globalFrame.origin)).AsVec2, 3f))
			{
				_crusasAndSeaHoundMovedToTheConversationPoints = true;
				return;
			}
		}
		WorldPosition val2 = default(WorldPosition);
		((WorldPosition)(ref val2))._002Ector(((MissionBehavior)this).Mission.Scene, ((WeakGameEntity)(ref _crusasConversationPointFrame)).GetGlobalFrame().origin);
		Vec3 val3 = ((WeakGameEntity)(ref _crusasConversationPointFrame)).GetGlobalFrame().origin - _crusasAgent.Position;
		_crusasAgent.SetScriptedPositionAndDirection(ref val2, MBMath.ToRadians(((Vec3)(ref val3)).RotationX), true, (AIScriptedFrameFlags)0);
		WorldPosition val4 = default(WorldPosition);
		((WorldPosition)(ref val4))._002Ector(((MissionBehavior)this).Mission.Scene, ((WeakGameEntity)(ref _slaveTraderConversationPointFrame)).GetGlobalFrame().origin);
		val = ((WeakGameEntity)(ref _slaveTraderConversationPointFrame)).GetGlobalFrame().origin - _slaveTraderAgent.Position;
		float num = MBMath.ToRadians(((Vec3)(ref val)).RotationX);
		_slaveTraderAgent.SetScriptedPositionAndDirection(ref val4, num, true, (AIScriptedFrameFlags)0);
	}

	private Equipment GetScriptedStealthEquipment()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Invalid comparison between Unknown and I4
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		Equipment val = MBObjectManager.Instance.GetObject<MBEquipmentRoster>("naval_storyline_quest5_stealth_set").DefaultEquipment.Clone(false);
		if (val == null)
		{
			val = Campaign.Current.DefaultStealthEquipment.Clone(false);
			for (int i = 0; i < 12; i++)
			{
				switch (i)
				{
				case 5:
				{
					ItemObject val5 = MBObjectManager.Instance.GetObject<ItemObject>("assassin_hood");
					if (val5 != null)
					{
						val[i] = new EquipmentElement(val5, (ItemModifier)null, (ItemObject)null, false);
					}
					break;
				}
				case 9:
				{
					ItemObject val3 = MBObjectManager.Instance.GetObject<ItemObject>("assassin_shoulder");
					if (val3 != null)
					{
						val[i] = new EquipmentElement(val3, (ItemModifier)null, (ItemObject)null, false);
					}
					break;
				}
				case 6:
				{
					ItemObject val4 = MBObjectManager.Instance.GetObject<ItemObject>("assassin_armor");
					if (val4 != null)
					{
						val[i] = new EquipmentElement(val4, (ItemModifier)null, (ItemObject)null, false);
					}
					break;
				}
				case 7:
				{
					ItemObject val2 = MBObjectManager.Instance.GetObject<ItemObject>("assassin_boot");
					if (val2 != null)
					{
						val[i] = new EquipmentElement(val2, (ItemModifier)null, (ItemObject)null, false);
					}
					break;
				}
				}
				if (i != 0 && i != 1 && i != 2 && i != 3 && i != 4)
				{
					continue;
				}
				EquipmentElement val6 = val[i];
				if (((EquipmentElement)(ref val6)).IsEmpty)
				{
					continue;
				}
				val6 = val[i];
				if (((EquipmentElement)(ref val6)).Item.WeaponComponent != null)
				{
					val6 = val[i];
					if ((int)((EquipmentElement)(ref val6)).Item.WeaponComponent.PrimaryWeapon.WeaponClass == 19)
					{
						val[i] = EquipmentElement.Invalid;
					}
				}
			}
		}
		return val;
	}

	private void HandleEscapeShipCutLoose()
	{
		if (_escapeShipCutLooseTimer == null || !_escapeShipCutLooseTimer.Check(false))
		{
			return;
		}
		_escapeShipCutLooseTimer = null;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_phase1EnemyShip3.AttachmentMachines)
		{
			if (item.IsShipAttachmentMachineBridged())
			{
				item.DisconnectAttachment();
			}
		}
		foreach (ShipAttachmentMachine item2 in (List<ShipAttachmentMachine>)(object)_phase1EnemyShip2.AttachmentMachines)
		{
			if (item2.IsShipAttachmentMachineBridged() && item2.CurrentAttachment.AttachmentTarget.OwnerShip == _phase1EnemyShip3)
			{
				item2.DisconnectAttachment();
			}
		}
	}

	public bool ShouldTeleportPlayerBetweenTargetPositionAndHidingSpot()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		if (Agent.Main != null && Agent.Main.IsActive() && !Agent.Main.IsInWater())
		{
			return false;
		}
		if (_allowedSwimRadiusCheckTimer == null)
		{
			_allowedSwimRadiusCheckTimer = new MissionTimer(5f);
		}
		else if (Agent.Main != null && Agent.Main.IsActive() && _allowedSwimRadiusCheckTimer.Check(false))
		{
			_allowedSwimRadiusCheckTimer.Reset();
			Vec3 position = Agent.Main.Position;
			if (((Vec3)(ref position)).Distance(HidingSpot1Position.GlobalPosition) > 200f)
			{
				DialogNotificationHandle item = CampaignInformationManager.AddDialogLine(new TextObject("{=4O6feRM9}Hey! Over here! Let's not get separated.", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)3);
				_dialogNotificationHandleCache.Add(item);
				return true;
			}
			position = Agent.Main.Position;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_phase1EnemyShip1).GameEntity;
			if (((Vec3)(ref position)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) < 25f)
			{
				DialogNotificationHandle item2 = CampaignInformationManager.AddDialogLine(new TextObject("{=y0EgxaLN}Keep away from those lookouts!", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)3);
				_dialogNotificationHandleCache.Add(item2);
				return true;
			}
		}
		return false;
	}

	public void TeleportPlayerBetweenTargetPositionAndHidingSpot(out Vec3 mainAgentDirection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		mainAgentDirection = Agent.Main.LookDirection;
		if (State == Quest5SetPieceBattleMissionState.Phase1GoToEnemyShip)
		{
			StandingPoint pilotStandingPoint = ((UsableMachine)_playerShip.ShipControllerMachine).PilotStandingPoint;
			Agent main = Agent.Main;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)pilotStandingPoint).GameEntity;
			main.TeleportToPosition(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			Agent.Main.HandleStartUsingAction((UsableMissionObject)(object)pilotStandingPoint, -1);
		}
		else
		{
			Vec3 val = (_approachPointEntity.GlobalPosition + HidingSpot1Position.GlobalPosition) * 0.5f;
			Vec3 val2 = HidingSpot1Position.GlobalPosition - val;
			mainAgentDirection = ((Vec3)(ref val2)).NormalizedCopy();
			Agent.Main.TeleportToPosition(val);
		}
	}

	public bool ShouldTeleportPlayerShipToStartingPosition()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if (_playerShip != null)
		{
			MatrixFrame globalFrame = _playerShip.GlobalFrame;
			if (((Vec3)(ref globalFrame.origin)).NearlyEquals(ref _phase1PlayerShipSpawnPosition, 2f))
			{
				return false;
			}
			if (MBMath.ApproximatelyEqualsTo(_lastCachedPlayerShipDistanceToTargetApproachPoint, 0f, 1E-05f))
			{
				globalFrame = _playerShip.GlobalFrame;
				_lastCachedPlayerShipDistanceToTargetApproachPoint = ((Vec3)(ref globalFrame.origin)).Distance(_approachPointEntity.GlobalPosition);
				_playerShipsTargetApproachPointDistanceCheckTimer = new MissionTimer(6f);
			}
			else
			{
				MissionTimer playerShipsTargetApproachPointDistanceCheckTimer = _playerShipsTargetApproachPointDistanceCheckTimer;
				if (playerShipsTargetApproachPointDistanceCheckTimer != null && playerShipsTargetApproachPointDistanceCheckTimer.Check(false))
				{
					globalFrame = _playerShip.GlobalFrame;
					float num = ((Vec3)(ref globalFrame.origin)).Distance(_approachPointEntity.GlobalPosition);
					if (num > _lastCachedPlayerShipDistanceToTargetApproachPoint)
					{
						_lastCachedPlayerShipDistanceToTargetApproachPoint = 0f;
						_playerShipsTargetApproachPointDistanceCheckTimer = null;
						return true;
					}
					_lastCachedPlayerShipDistanceToTargetApproachPoint = num;
				}
			}
		}
		return false;
	}

	public void TeleportPlayerShipToStartingPosition(out Vec3 mainAgentDirection)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("phase_1_player_ship_sp");
		_navalShipsLogic.TeleportShip(_playerShip, val.GetGlobalFrame(), checkFreeArea: true);
		mainAgentDirection = Agent.Main.LookDirection;
	}

	public Vec3 CalculateMissionStartDirection()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = _approachPointEntity.GetGlobalFrame().origin - Agent.Main.Frame.origin;
		return ((Vec3)(ref val)).NormalizedCopy();
	}

	private void HandlePlayersBridgeAndControlPointUsagesForPhase1GoToEnemyShip()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (item != _playerShip)
			{
				foreach (ClimbingMachine item2 in (List<ClimbingMachine>)(object)item.ClimbingMachines)
				{
					foreach (StandingPoint item3 in (List<StandingPoint>)(object)((UsableMachine)item2).StandingPoints)
					{
						((UsableMissionObject)item3).IsDisabledForPlayers = true;
					}
				}
			}
			foreach (ShipAttachmentMachine item4 in (List<ShipAttachmentMachine>)(object)item.AttachmentMachines)
			{
				foreach (StandingPoint item5 in (List<StandingPoint>)(object)((UsableMachine)item4).StandingPoints)
				{
					((UsableMissionObject)item5).IsDisabledForPlayers = true;
				}
			}
			foreach (ShipAttachmentPointMachine item6 in (List<ShipAttachmentPointMachine>)(object)item.AttachmentPointMachines)
			{
				foreach (StandingPoint item7 in (List<StandingPoint>)(object)((UsableMachine)item6).StandingPoints)
				{
					((UsableMissionObject)item7).IsDisabledForPlayers = true;
				}
			}
		}
	}

	private void HandlePlayersBridgeAndControlPointUsagesForPhase1SwimmingAndStealthPhase()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (item != _phase1EnemyShip4)
			{
				foreach (ClimbingMachine item2 in (List<ClimbingMachine>)(object)item.ClimbingMachines)
				{
					foreach (StandingPoint item3 in (List<StandingPoint>)(object)((UsableMachine)item2).StandingPoints)
					{
						((UsableMissionObject)item3).IsDisabledForPlayers = true;
					}
				}
			}
			else
			{
				foreach (ClimbingMachine item4 in (List<ClimbingMachine>)(object)item.ClimbingMachines)
				{
					foreach (StandingPoint item5 in (List<StandingPoint>)(object)((UsableMachine)item4).StandingPoints)
					{
						((UsableMissionObject)item5).IsDisabledForPlayers = false;
					}
				}
			}
			foreach (ShipAttachmentMachine item6 in (List<ShipAttachmentMachine>)(object)item.AttachmentMachines)
			{
				foreach (StandingPoint item7 in (List<StandingPoint>)(object)((UsableMachine)item6).StandingPoints)
				{
					((UsableMissionObject)item7).IsDisabledForPlayers = true;
				}
			}
			foreach (ShipAttachmentPointMachine item8 in (List<ShipAttachmentPointMachine>)(object)item.AttachmentPointMachines)
			{
				foreach (StandingPoint item9 in (List<StandingPoint>)(object)((UsableMachine)item8).StandingPoints)
				{
					((UsableMissionObject)item9).IsDisabledForPlayers = true;
				}
			}
		}
	}

	private void HandlePlayersBridgeAndControlPointUsagesForPhase1EscapePhase()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			foreach (ClimbingMachine item2 in (List<ClimbingMachine>)(object)item.ClimbingMachines)
			{
				foreach (StandingPoint item3 in (List<StandingPoint>)(object)((UsableMachine)item2).StandingPoints)
				{
					((UsableMissionObject)item3).IsDisabledForPlayers = false;
				}
			}
			if (item != _phase1EnemyShip3)
			{
				foreach (ShipAttachmentMachine item4 in (List<ShipAttachmentMachine>)(object)item.AttachmentMachines)
				{
					foreach (StandingPoint item5 in (List<StandingPoint>)(object)((UsableMachine)item4).StandingPoints)
					{
						((UsableMissionObject)item5).IsDisabledForPlayers = true;
					}
				}
				foreach (ShipAttachmentPointMachine item6 in (List<ShipAttachmentPointMachine>)(object)item.AttachmentPointMachines)
				{
					foreach (StandingPoint item7 in (List<StandingPoint>)(object)((UsableMachine)item6).StandingPoints)
					{
						((UsableMissionObject)item7).IsDisabledForPlayers = true;
					}
				}
				continue;
			}
			foreach (ShipAttachmentMachine item8 in (List<ShipAttachmentMachine>)(object)item.AttachmentMachines)
			{
				if (item8.CurrentAttachment == null)
				{
					foreach (StandingPoint item9 in (List<StandingPoint>)(object)((UsableMachine)item8).StandingPoints)
					{
						((UsableMissionObject)item9).IsDisabledForPlayers = true;
					}
					continue;
				}
				foreach (StandingPoint item10 in (List<StandingPoint>)(object)((UsableMachine)item8).StandingPoints)
				{
					((UsableMissionObject)item10).IsDisabledForPlayers = false;
				}
			}
			foreach (ShipAttachmentPointMachine item11 in (List<ShipAttachmentPointMachine>)(object)item.AttachmentPointMachines)
			{
				if (item11.CurrentAttachment == null)
				{
					foreach (StandingPoint item12 in (List<StandingPoint>)(object)((UsableMachine)item11).StandingPoints)
					{
						((UsableMissionObject)item12).IsDisabledForPlayers = true;
					}
					continue;
				}
				foreach (StandingPoint item13 in (List<StandingPoint>)(object)((UsableMachine)item11).StandingPoints)
				{
					((UsableMissionObject)item13).IsDisabledForPlayers = false;
				}
			}
		}
	}

	private void ClearPhase1OnPhaseTransition()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		_phase1EnemyShip1 = null;
		_phase1EnemyShip2 = null;
		_phase1EnemyShip4 = null;
		_dynamicPatrolAreas = null;
		_stealthAgents = null;
		_crusasConversationPointFrame = WeakGameEntity.Invalid;
		_slaveTraderConversationPointFrame = WeakGameEntity.Invalid;
		_approachPointEntity = null;
		_phase1EnemyShipToInteriorShipDoorEntity = null;
		_phase1InteriorToEnemyShip3ShipDoorEntity = null;
		_phase1EnemyShip1InitialSpawnEntity = null;
		_phase1EnemyShip1TargetEntity = null;
		_conversationSounds = null;
		_dialogNotificationHandleCache.Clear();
		_sisterWoundedAnimationActionIndexCache = ActionIndexCache.act_none;
		_slaveTraderShipOarsmanActionIndexCache = ActionIndexCache.act_none;
		Phase1InteriorCameraSisterEntity = null;
		GC.Collect();
	}

	public void TriggerInitializePhase2()
	{
		State = Quest5SetPieceBattleMissionState.InitializePhase2Part1;
		MBMusicManager.Current.StartTheme((MusicTheme)10246, 0.3f, false);
	}

	public void CompletePhase1ToPhase2Transition()
	{
		State = Quest5SetPieceBattleMissionState.Phase2InProgress;
	}

	private void InitializePhase2Part1()
	{
		Mission.Current.Scene.SetAtmosphereWithName("TOD_naval_03_00_sunset");
		if (_gunnarAgent != null && _gunnarAgent.IsUsingGameObject)
		{
			_gunnarAgent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
		}
		if (Agent.Main != null && Agent.Main.IsActive() && Agent.Main.IsUsingGameObject)
		{
			Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		if (_slaveTraderAgent != null && _slaveTraderAgent.IsActive())
		{
			_slaveTraderAgent.FadeOut(true, false);
			for (int i = 0; i < _slaveTraderShipOarsmen.Length; i++)
			{
				Agent val = _slaveTraderShipOarsmen[i];
				if (val != null)
				{
					val.FadeOut(true, false);
				}
			}
			_navalTrajectoryPlanningLogic.ForceReinitialize();
		}
		((MissionBehavior)this).Mission.GetMissionBehavior<Quest5WanderingShipsMissionLogic>()?.OnPhase2Started();
		foreach (DialogNotificationHandle item in _dialogNotificationHandleCache)
		{
			CampaignInformationManager.ClearDialogNotification(item, true);
		}
		_dialogNotificationHandleCache.Clear();
	}

	private void InitializePhase2Part2()
	{
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Expected O, but got Unknown
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0800: Unknown result type (might be due to invalid IL or missing references)
		//IL_080e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0813: Unknown result type (might be due to invalid IL or missing references)
		//IL_0817: Unknown result type (might be due to invalid IL or missing references)
		//IL_081c: Unknown result type (might be due to invalid IL or missing references)
		_phase2AllyShip1 = CreateShip("aserai_heavy_ship", "phase_2_ally_ship_1_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip1UpgradePieceList);
		_phase2AllyShip2 = CreateShip("nord_medium_ship", "phase_2_ally_ship_2_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip2UpgradePieceList);
		_phase2AllyShip3 = CreateShip("northern_medium_ship", "phase_2_ally_ship_3_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip3UpgradePieceList);
		_phase2AllyShip4 = CreateShip("sturgia_heavy_ship", "phase_2_ally_ship_4_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip4UpgradePieceList);
		_phase2AllyShip5 = CreateShip("northern_medium_ship", "phase_2_ally_ship_5_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip5UpgradePieceList);
		if (_phase1EnemyShip3 == null)
		{
			_isCheckpointInitialize = true;
			TeamAINavalComponent teamAINavalComponent = new TeamAINavalComponent(((MissionBehavior)this).Mission, ((MissionBehavior)this).Mission.AttackerTeam, 5f, 1f);
			((MissionBehavior)this).Mission.AttackerTeam.AddTeamAI((TeamAIComponent)(object)teamAINavalComponent, false);
			((MissionBehavior)this).Mission.AttackerTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(((MissionBehavior)this).Mission.AttackerTeam));
			TeamAINavalComponent teamAINavalComponent2 = new TeamAINavalComponent(((MissionBehavior)this).Mission, ((MissionBehavior)this).Mission.DefenderTeam, 5f, 1f);
			((MissionBehavior)this).Mission.DefenderTeam.AddTeamAI((TeamAIComponent)(object)teamAINavalComponent2, false);
			((MissionBehavior)this).Mission.DefenderTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(((MissionBehavior)this).Mission.DefenderTeam));
			_navalAgentsLogic.SetDeploymentMode(value: true);
			_navalShipsLogic.SetDeploymentMode(value: true);
			_playerShip = CreateShip("ship_dromon_storyline", "phase_1_enemy_ship_3_sp", _playerFormation, spawnAnchored: false, _escapeShipUpgradePieceList);
			_navalAgentsLogic.SetDesiredTroopCountOfShip(_playerShip, 2);
			Hero.MainHero.Heal(Hero.MainHero.MaxHitPoints, false);
			_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, CharacterObject.PlayerCharacter, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
			_navalAgentsLogic.SetDeploymentMode(value: false);
			_navalShipsLogic.SetDeploymentMode(value: false);
			_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
			SpawnGunnarOnShip(_playerShip);
			_gunnarAgent.Controller = (AgentControllerType)0;
			_gunnarAgent.SetMortalityState((MortalityState)2);
			_playerShip.SetController(ShipControllerType.AI);
			_phase1EnemyShipToInteriorShipDoorEntity = Mission.Current.Scene.FindEntityWithTag("phase_1_enemy_ship_3_to_interior_door_tag");
			_phase1EnemyShipToInteriorShipDoorEntity.GetFirstScriptOfType<ShipDoorUsePoint>().SetShipDoorUsePointEnabled(isEnabled: false);
			Agent.Main.TeleportToPosition(_playerShip.GetMiddleInnerSpawnGlobalFrame().origin);
			_playerShip.SetAnchor(isAnchored: false);
			_playerShip.SetShipOrderActive(isOrderActive: true);
			((UsableMissionObject)((UsableMachine)_playerShip.ShipControllerMachine).PilotStandingPoint).IsDisabledForPlayers = true;
		}
		else
		{
			Formation availableAllyFormation = GetAvailableAllyFormation();
			_navalShipsLogic.TransferShipToTeam(EscapeShip, ((MissionBehavior)this).Mission.PlayerTeam, availableAllyFormation, (FormationClass)8);
			_navalAgentsLogic.AddAgentToShip(_gunnarAgent, EscapeShip);
			_navalAgentsLogic.TransferAgentToShip(Agent.Main, EscapeShip);
			RemoveShipInternal(_playerShip);
			AddAvailableAllyFormation(availableAllyFormation);
			_navalShipsLogic.TransferShipToFormation(EscapeShip, _playerFormation);
			_playerShip = EscapeShip;
			_navalAgentsLogic.AssignCaptainToShip(_gunnarAgent, EscapeShip);
			EscapeShip.ShipOrder.ManageShipDetachments();
			_gunnarAgent.TeleportToPosition(GunnarShipUsePosition.origin);
			((UsableMissionObject)((UsableMachine)EscapeShip.ShipControllerMachine).PilotStandingPoint).IsDisabledForPlayers = true;
			_navalAgentsLogic.SetDeploymentMode(value: true);
			_navalShipsLogic.SetDeploymentMode(value: true);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(EscapeShip);
			_navalAgentsLogic.SetDeploymentMode(value: false);
			_navalShipsLogic.SetDeploymentMode(value: false);
			Vec3 position = Agent.Main.Position;
			Agent.Main.TeleportToPosition(position);
			AddAvailableEnemyFormation(_phase1EnemyShip1.Formation);
			RemoveShipInternal(_phase1EnemyShip1);
			AddAvailableEnemyFormation(_phase1EnemyShip4.Formation);
			RemoveShipInternal(_phase1EnemyShip4);
			_navalTrajectoryPlanningLogic.ForceReinitialize();
		}
		_phase2EnemyShip1 = CreateShip("ship_meditlight_storyline_q5", "phase_2_enemy_ship_1_sp", GetAvailableEnemyFormation());
		_phase2EnemyShip2 = CreateShip("ship_meditlight_storyline_q5", "phase_2_enemy_ship_2_sp", GetAvailableEnemyFormation());
		_phase2EnemyShip3 = CreateShip("ship_meditlight_storyline_q5", "phase_2_enemy_ship_3_sp", GetAvailableEnemyFormation());
		_phase2EnemyShip4 = CreateShip("ship_meditlight_storyline_q5", "phase_2_enemy_ship_4_sp", GetAvailableEnemyFormation());
		_phase2EnemyShip5 = CreateShip("ship_meditlight_storyline_q5", "phase_2_enemy_ship_5_sp", GetAvailableEnemyFormation());
		_phase2EnemyShip1.SetCanBeTakenOver(value: false);
		_phase2EnemyShip2.SetCanBeTakenOver(value: false);
		_phase2EnemyShip3.SetCanBeTakenOver(value: false);
		_phase2EnemyShip4.SetCanBeTakenOver(value: false);
		_phase2EnemyShip5.SetCanBeTakenOver(value: false);
		_phase2EnemyShipStationary1 = CreateShip("western_medium_ship", "phase_2_enemy_ship_stationary_1", GetAvailableEnemyFormation());
		_phase2EnemyShipStationary1.SetCanBeTakenOver(value: false);
		AddTriggerPointForPirateShip(_phase2EnemyShip1, ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("phase_2_enemy_ship_1_target"));
		AddTriggerPointForPirateShip(_phase2EnemyShip2, ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("phase_2_enemy_ship_2_target"));
		AddTriggerPointForPirateShip(_phase2EnemyShip3, ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("phase_2_enemy_ship_3_target"));
		AddTriggerPointForPirateShip(_phase2EnemyShip4, ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("phase_2_enemy_ship_4_target"));
		AddTriggerPointForPirateShip(_phase2EnemyShip5, ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("phase_2_enemy_ship_5_target"));
		_phase2EnemyShip1.SetFoldSailsOnBridgeConnection(value: false);
		_phase2EnemyShip2.SetFoldSailsOnBridgeConnection(value: false);
		_phase2EnemyShip3.SetFoldSailsOnBridgeConnection(value: false);
		_phase2EnemyShip4.SetFoldSailsOnBridgeConnection(value: false);
		_phase2EnemyShip5.SetFoldSailsOnBridgeConnection(value: false);
		_autoCutLooseTimersForPirateShips.Add(_phase2EnemyShip1, null);
		_autoCutLooseTimersForPirateShips.Add(_phase2EnemyShip2, null);
		_autoCutLooseTimersForPirateShips.Add(_phase2EnemyShip3, null);
		_autoCutLooseTimersForPirateShips.Add(_phase2EnemyShip4, null);
		_autoCutLooseTimersForPirateShips.Add(_phase2EnemyShip5, null);
		_autoEstablishConnectionsForPirateShips.Add(_phase2EnemyShip1, null);
		_autoEstablishConnectionsForPirateShips.Add(_phase2EnemyShip2, null);
		_autoEstablishConnectionsForPirateShips.Add(_phase2EnemyShip3, null);
		_autoEstablishConnectionsForPirateShips.Add(_phase2EnemyShip4, null);
		_autoEstablishConnectionsForPirateShips.Add(_phase2EnemyShip5, null);
		EscapeShip.SetFoldSailsOnBridgeConnection(value: false);
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)EscapeShip.AttachmentMachines)
		{
			if (((MissionObject)item).IsDisabled)
			{
				((MissionObject)item).SetEnabledAndMakeVisible(false, false);
			}
		}
		SetShipAttachmentJointPhysicsEnabledForShip(_phase2EnemyShip1, enabled: false);
		SetShipAttachmentJointPhysicsEnabledForShip(_phase2EnemyShip2, enabled: false);
		SetShipAttachmentJointPhysicsEnabledForShip(_phase2EnemyShip3, enabled: false);
		SetShipAttachmentJointPhysicsEnabledForShip(_phase2EnemyShip4, enabled: false);
		SetShipAttachmentJointPhysicsEnabledForShip(_phase2EnemyShip5, enabled: false);
		EscapeShip.SetController(ShipControllerType.AI);
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)2, true);
		_escapeShipTargetSpeed = 0f;
		_escapeShipSpeed = 0f;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)EscapeShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec2 asVec = ((Vec3)(ref bodyWorldTransform.rotation.f)).AsVec2;
		_escapeShipTargetDirection = ((Vec2)(ref asVec)).Normalized();
		gameEntity = ((ScriptComponentBehavior)EscapeShip).GameEntity;
		bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		asVec = ((Vec3)(ref bodyWorldTransform.rotation.f)).AsVec2;
		_escapeShipDirection = ((Vec2)(ref asVec)).Normalized();
	}

	private void InitializePhase2Part3()
	{
		SetDisableShipAttachmentMachinesForPlayer(EscapeShip, isDisabled: true);
		SpawnPhase2AllyTroops();
		SpawnPhase2EnemyTroops();
		if (_isCheckpointInitialize)
		{
			Mission.Current.OnDeploymentFinished();
		}
		else
		{
			((MissionObject)_phase2EnemyShip1).OnDeploymentFinished();
			((MissionObject)_phase2EnemyShip2).OnDeploymentFinished();
			((MissionObject)_phase2EnemyShip3).OnDeploymentFinished();
			((MissionObject)_phase2EnemyShip4).OnDeploymentFinished();
			((MissionObject)_phase2EnemyShip5).OnDeploymentFinished();
			((MissionObject)_phase2EnemyShipStationary1).OnDeploymentFinished();
			((MissionObject)_phase2AllyShip1).OnDeploymentFinished();
			((MissionObject)_phase2AllyShip2).OnDeploymentFinished();
			((MissionObject)_phase2AllyShip3).OnDeploymentFinished();
			((MissionObject)_phase2AllyShip4).OnDeploymentFinished();
			((MissionObject)_phase2AllyShip5).OnDeploymentFinished();
			_navalTrajectoryPlanningLogic.ForceReinitialize();
		}
		_lightScriptedFiresMissionController.TriggerFiring();
		_gunnarAgent.Controller = (AgentControllerType)0;
		HandlePlayersBridgeAndControlPointUsagesForPhase2InProgress();
		RemoveShipControlPointDescriptionOfAllEnemyShips();
		_isMissionShipBoardedToTheEscapeShip.Add(_phase2EnemyShip1, value: false);
		_isMissionShipBoardedToTheEscapeShip.Add(_phase2EnemyShip2, value: false);
		_isMissionShipBoardedToTheEscapeShip.Add(_phase2EnemyShip3, value: false);
		_isMissionShipBoardedToTheEscapeShip.Add(_phase2EnemyShip4, value: false);
		_isMissionShipBoardedToTheEscapeShip.Add(_phase2EnemyShip5, value: false);
		_phase2EscapeShipPirateTargetFrame1 = Mission.Current.Scene.FindEntityWithTag("phase_2_anchor_1");
		_phase2EscapeShipPirateTargetFrame2 = Mission.Current.Scene.FindEntityWithTag("phase_2_anchor_2");
		_phase2EscapeShipPirateTargetFrame3 = Mission.Current.Scene.FindEntityWithTag("phase_2_anchor_3");
		_phase2EscapeShipPirateTargetFrame4 = Mission.Current.Scene.FindEntityWithTag("phase_2_anchor_4");
		_phase2EscapeShipPirateTargetFrame5 = Mission.Current.Scene.FindEntityWithTag("phase_2_anchor_5");
		EscapeShip.SetCustomSailSetting(enableCustomSailSetting: true, SailInput.Full);
		if (!_isCheckpointInitialize)
		{
			ClearPhase1OnPhaseTransition();
		}
	}

	private void InitializePhase2Part4()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (_isCheckpointInitialize)
		{
			_gunnarAgent.Controller = (AgentControllerType)1;
			_navalAgentsLogic.AssignCaptainToShip(_gunnarAgent, EscapeShip);
			_navalAgentsLogic.TransferAgentToShip(Agent.Main, EscapeShip);
			EscapeShip.ShipOrder.ManageShipDetachments();
			_navalAgentsLogic.SetDeploymentMode(value: true);
			_navalShipsLogic.SetDeploymentMode(value: true);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(EscapeShip);
			_navalAgentsLogic.SetDeploymentMode(value: false);
			_navalShipsLogic.SetDeploymentMode(value: false);
			_gunnarAgent.Controller = (AgentControllerType)0;
			_gunnarMovementState = GunnarMovementState.UseTheEscapeShip;
			_playerShip.GetNextCrewSpawnGlobalFrame(out var crewSpawnGlobalFrame);
			Agent.Main.TeleportToPosition(crewSpawnGlobalFrame.origin);
			Agent.Main.SetClothingColor1(4279111698u);
			Agent.Main.SetClothingColor2(4279111698u);
			Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(GetScriptedStealthEquipment());
			_gunnarAgent.UpdateSpawnEquipmentAndRefreshVisuals(GetScriptedStealthEquipment());
			_instructionState = Quest5InstructionState.GunnarUsesShip;
			State = Quest5SetPieceBattleMissionState.Phase2InProgress;
		}
		else
		{
			State = Quest5SetPieceBattleMissionState.Phase1ToPhase2FadeIn;
		}
		RemoveGunnarsHelmet();
		ModifyMainAgentEquipmentForPhase2();
	}

	private void AddTriggerPointForPirateShip(MissionShip ship, GameEntity triggerPoint)
	{
		_pirateShipTriggerPoints[ship] = triggerPoint;
		_isPirateShipTriggered[ship] = false;
		_isPirateShipMovementDisabled[ship] = false;
		_pirateShipEnabledAttachmentMachine[ship] = null;
		_isPirateShipMovingToTheEscapeShip[ship] = false;
		_isPirateShipLostItsCrew[ship] = false;
		_limitPirateShipChasingSpeed[ship] = false;
	}

	private void SpawnPhase2AllyTroops()
	{
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip1, Phase2AllyShip1TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip2, Phase2AllyShip2TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip3, Phase2AllyShip3TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip4, Phase2AllyShip4TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip5, Phase2AllyShip5TroopCount);
		AddMissionShipTroops(_phase2AllyShip1Troops, _phase2AllyShip1, PartyBase.MainParty);
		AddMissionShipTroops(_phase2AllyShip2Troops, _phase2AllyShip2, PartyBase.MainParty);
		AddMissionShipTroops(_phase2AllyShip3Troops, _phase2AllyShip3, PartyBase.MainParty);
		AddMissionShipTroops(_phase2AllyShip4Troops, _phase2AllyShip4, PartyBase.MainParty);
		AddMissionShipTroops(_phase2AllyShip5Troops, _phase2AllyShip5, PartyBase.MainParty);
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip1);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip2);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip3);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip4);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip5);
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
	}

	private void SpawnPhase2EnemyTroops()
	{
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2EnemyShip1, Phase2EnemyShip1TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2EnemyShip2, Phase2EnemyShip2TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2EnemyShip3, Phase2EnemyShip3TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2EnemyShip4, Phase2EnemyShip4TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2EnemyShip5, Phase2EnemyShip5TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2EnemyShipStationary1, Phase2EnemyShipStationary1TroopCount);
		AddMissionShipTroops(_phase2EnemyShip1Troops, _phase2EnemyShip1);
		AddMissionShipTroops(_phase2EnemyShip2Troops, _phase2EnemyShip2);
		AddMissionShipTroops(_phase2EnemyShip3Troops, _phase2EnemyShip3);
		AddMissionShipTroops(_phase2EnemyShip4Troops, _phase2EnemyShip4);
		AddMissionShipTroops(_phase2EnemyShip5Troops, _phase2EnemyShip5);
		AddMissionShipTroops(_phase2EnemyShipStationary1Troops, _phase2EnemyShipStationary1);
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)2);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2EnemyShip1);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2EnemyShip2);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2EnemyShip3);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2EnemyShip4);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2EnemyShip5);
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
	}

	private void HandleEscapeShipMovement()
	{
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		if (!EscapeShip.IsAIControlled)
		{
			EscapeShip.SetController(ShipControllerType.AI);
		}
		WeakGameEntity gameEntity;
		Vec3 val;
		MatrixFrame bodyWorldTransform;
		if (_currentPhase2EscapeShipTargetPoint == (GameEntity)null)
		{
			if (!Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_phase2EscapeShipTargetPoints))
			{
				_currentPhase2EscapeShipTargetPoint = _phase2EscapeShipTargetPoints.Dequeue();
			}
			else
			{
				_currentPhase2EscapeShipTargetPoint = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("phase_3_enemy_ship_2_sp");
			}
			Vec2 targetPosition;
			if (!_isPirateShipMovementDisabled[_phase2EnemyShip5])
			{
				Vec3 origin = _currentPhase2EscapeShipTargetPoint.GetGlobalFrame().origin;
				gameEntity = ((ScriptComponentBehavior)EscapeShip).GameEntity;
				val = origin - ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().origin;
				targetPosition = ((Vec3)(ref val)).AsVec2;
				_escapeShipTargetDirection = ((Vec2)(ref targetPosition)).Normalized();
			}
			else
			{
				gameEntity = ((ScriptComponentBehavior)EscapeShip).GameEntity;
				bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
				targetPosition = ((Vec3)(ref bodyWorldTransform.rotation.f)).AsVec2;
				_escapeShipTargetDirection = ((Vec2)(ref targetPosition)).Normalized();
			}
			ShipOrder shipOrder = EscapeShip.ShipOrder;
			val = _currentPhase2EscapeShipTargetPoint.GlobalPosition;
			targetPosition = ((Vec3)(ref val)).AsVec2;
			shipOrder.SetShipMovementOrder(in targetPosition);
			EscapeShip.ShipOrder.SetOrderOarsmenLevel(2);
		}
		else
		{
			val = _currentPhase2EscapeShipTargetPoint.GlobalPosition;
			gameEntity = ((ScriptComponentBehavior)EscapeShip).GameEntity;
			bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			if (((Vec3)(ref val)).NearlyEquals(ref bodyWorldTransform.origin, 35f))
			{
				_currentPhase2EscapeShipTargetPoint = null;
			}
		}
		if (_currentPhase2EscapeShipTargetPoint != (GameEntity)null)
		{
			EscapeShip.ShipOrder.SetOrderOarsmenLevel(2);
		}
	}

	private void HandleEscapeShipSpeed()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (State == Quest5SetPieceBattleMissionState.Phase2InProgress)
		{
			AdjustWindDirectionAccordingToTargetFrame(EscapeShip.GlobalFrame, 1f);
			_escapeShipTargetSpeed = (GetIsThereActiveBridgeToBetweenEscapeShipAndAnyPirateShips() ? 2.7f : 5f);
		}
	}

	private void HandlePirateShipGettingCloseToEscapeShip(MissionShip pirateShip, GameEntity finalTargetFrameEntity, float gettingCloseSpeed, float fixedDt)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		if (_navalAgentsLogic.GetActiveAgentCountOfShip(pirateShip) > 0 && _isPirateShipMovingToTheEscapeShip[pirateShip])
		{
			MatrixFrame globalFrameImpreciseForFixedTick = finalTargetFrameEntity.GetGlobalFrameImpreciseForFixedTick();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)pirateShip).GameEntity;
			MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			Vec2 asVec = ((Vec3)(ref bodyWorldTransform.origin)).AsVec2;
			Vec2 val = ((Vec3)(ref globalFrameImpreciseForFixedTick.origin)).AsVec2 - asVec;
			float length = ((Vec2)(ref val)).Length;
			Vec3 linearVelocity = EscapeShip.Physics.LinearVelocity;
			Vec2 asVec2 = ((Vec3)(ref linearVelocity)).AsVec2;
			float num = ((length > 1E-06f) ? MathF.Min(gettingCloseSpeed, length / fixedDt) : 0f);
			Vec2 val2 = (Vec2)((length > 1E-06f) ? (val / length) : new Vec2(1f, 0f));
			Vec2 val3 = asVec2 + val2 * num;
			if (_limitPirateShipChasingSpeed[pirateShip])
			{
				((Vec2)(ref val3)).ClampMagnitude(0f, ((Vec2)(ref val3)).Length * 0.5f);
			}
			Vec2 targetPosition = asVec + val3 * fixedDt;
			Vec2 asVec3;
			Vec2 val4;
			if (!(((Vec2)(ref val3)).Length > 1E-06f))
			{
				asVec3 = ((Vec3)(ref bodyWorldTransform.rotation.f)).AsVec2;
				val4 = ((Vec2)(ref asVec3)).Normalized();
			}
			else
			{
				val4 = ((Vec2)(ref val3)).Normalized();
			}
			Vec2 val5 = val4;
			float num2 = 1f - MathF.Min(length, 200f) / 200f;
			Vec2 zero = Vec2.Zero;
			if (length <= 4f)
			{
				asVec3 = ((Vec3)(ref globalFrameImpreciseForFixedTick.rotation.f)).AsVec2;
				zero = ((Vec2)(ref asVec3)).Normalized();
			}
			else
			{
				asVec3 = ((Vec3)(ref globalFrameImpreciseForFixedTick.rotation.f)).AsVec2;
				zero = Vec2.Lerp(val5, ((Vec2)(ref asVec3)).Normalized(), num2);
			}
			pirateShip.MoveShipToTheTargetWithDirection(bodyWorldTransform, targetPosition, zero, 5f, 2.5f, fixedDt);
		}
	}

	private void HandlePirateShipMovement(MissionShip pirateShip, GameEntity finalTargetFrameEntity)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0691: Unknown result type (might be due to invalid IL or missing references)
		//IL_0696: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		if (_navalAgentsLogic.GetActiveAgentCountOfShip(pirateShip) <= 0)
		{
			return;
		}
		pirateShip.ShipOrder.SetCutLoose(enable: false);
		MatrixFrame val;
		WeakGameEntity gameEntity;
		Vec2 targetPosition;
		if (_isPirateShipMovingToTheEscapeShip[pirateShip])
		{
			val = pirateShip.GlobalFrame;
			if (((Vec3)(ref val.origin)).Distance(finalTargetFrameEntity.GetGlobalFrame().origin) <= 60f)
			{
				pirateShip.Formation.SetMovementOrder(MovementOrder.MovementOrderCharge);
				pirateShip.Formation.SetTargetFormation(Agent.Main.Formation);
				pirateShip.ShipOrder.SetShipEngageOrder(EscapeShip);
				pirateShip.ShipOrder.SetBoardingTargetShip(EscapeShip);
			}
			gameEntity = ((ScriptComponentBehavior)pirateShip).GameEntity;
			val = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			targetPosition = ((Vec3)(ref val.origin)).AsVec2;
			val = finalTargetFrameEntity.GetGlobalFrame();
			if (((Vec2)(ref targetPosition)).DistanceSquared(((Vec3)(ref val.origin)).AsVec2) <= 2f)
			{
				if (_pirateShipEnabledAttachmentMachine[pirateShip] == null)
				{
					ShipAttachmentMachine shipAttachmentMachine = null;
					float num = -1f;
					foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)pirateShip.AttachmentMachines)
					{
						gameEntity = ((ScriptComponentBehavior)item).GameEntity;
						Vec3 f = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation.f;
						gameEntity = ((ScriptComponentBehavior)EscapeShip).GameEntity;
						Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().origin;
						gameEntity = ((ScriptComponentBehavior)item).GameEntity;
						if (!(Vec3.DotProduct(f, origin - ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin) > 0f))
						{
							continue;
						}
						foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)EscapeShip.AttachmentPointMachines)
						{
							float num2 = ShipAttachmentMachine.ComputePotentialAttachmentValue(item, item2, checkInteractionDistance: true, checkConnectionBlock: true, allowWiderAngleBetweenConnections: false);
							if (num2 > num)
							{
								num = num2;
								shipAttachmentMachine = item;
							}
						}
					}
					if (shipAttachmentMachine != null)
					{
						_pirateShipEnabledAttachmentMachine[pirateShip] = shipAttachmentMachine;
					}
				}
				else
				{
					((MissionObject)_pirateShipEnabledAttachmentMachine[pirateShip]).SetEnabled(true);
					((UsableMachine)_pirateShipEnabledAttachmentMachine[pirateShip]).SetIsDisabledForAI(false);
				}
				return;
			}
			pirateShip.Formation.SetMovementOrder(MovementOrder.MovementOrderCharge);
			pirateShip.Formation.SetTargetFormation(Agent.Main.Formation);
			pirateShip.ShipOrder.SetShipEngageOrder(EscapeShip);
			{
				foreach (ShipAttachmentMachine item3 in (List<ShipAttachmentMachine>)(object)pirateShip.AttachmentMachines)
				{
					if (item3.CurrentAttachment == null)
					{
						if (((UsableMachine)item3).PilotAgent != null)
						{
							((UsableMachine)item3).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
						}
						((MissionObject)item3).SetDisabled(true);
					}
					else if (item3.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.RopesPulling || item3.CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.RopeThrown)
					{
						item3.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
						if (((UsableMachine)item3).PilotAgent != null)
						{
							((UsableMachine)item3).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
						}
						((MissionObject)item3).SetDisabled(true);
					}
				}
				return;
			}
		}
		Vec3 globalPosition;
		if (_isPirateShipTriggered[pirateShip])
		{
			gameEntity = ((ScriptComponentBehavior)pirateShip).GameEntity;
			val = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			float num3 = ((Vec3)(ref val.origin)).Distance(_pirateShipTriggerPoints[pirateShip].GlobalPosition);
			val = pirateShip.GlobalFrame;
			float num4 = ((Vec3)(ref val.origin)).Distance(EscapeShip.GlobalFrame.origin);
			if (num3 <= 40f || num4 < 40f)
			{
				pirateShip.ShipOrder.SetShipEngageOrder(EscapeShip);
				pirateShip.Formation.SetTargetFormation(EscapeShip.Formation);
				foreach (ShipAttachmentMachine item4 in (List<ShipAttachmentMachine>)(object)pirateShip.AttachmentMachines)
				{
					if (((UsableMachine)item4).PilotAgent != null)
					{
						((UsableMachine)item4).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
					}
					((MissionObject)item4).SetDisabled(true);
				}
				foreach (ShipAttachmentPointMachine item5 in (List<ShipAttachmentPointMachine>)(object)pirateShip.AttachmentPointMachines)
				{
					if (((UsableMachine)item5).PilotAgent != null)
					{
						((UsableMachine)item5).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
					}
					((MissionObject)item5).SetDisabled(false);
					foreach (StandingPoint item6 in (List<StandingPoint>)(object)((UsableMachine)item5).StandingPoints)
					{
						((MissionObject)item6).SetDisabled(false);
					}
				}
				_isPirateShipMovingToTheEscapeShip[pirateShip] = true;
			}
			else
			{
				pirateShip.SetShipOrderActive(isOrderActive: true);
				ShipOrder shipOrder = pirateShip.ShipOrder;
				globalPosition = _pirateShipTriggerPoints[pirateShip].GlobalPosition;
				targetPosition = ((Vec3)(ref globalPosition)).AsVec2;
				shipOrder.SetShipMovementOrder(in targetPosition);
			}
			return;
		}
		if (_isPirateShipLostItsCrew[pirateShip])
		{
			_isPirateShipMovementDisabled[pirateShip] = true;
			_isPirateShipTriggered[pirateShip] = false;
			_isPirateShipMovingToTheEscapeShip[pirateShip] = false;
			pirateShip.SetAnchor(isAnchored: true);
			pirateShip.ShipOrder.SetShipStopOrder();
			pirateShip.Formation.SetMovementOrder(MovementOrder.MovementOrderStop);
			pirateShip.Formation.SetTargetFormation((Formation)null);
			foreach (ShipAttachmentMachine item7 in (List<ShipAttachmentMachine>)(object)pirateShip.AttachmentMachines)
			{
				((MissionObject)item7).SetDisabled(true);
			}
			{
				foreach (ShipAttachmentPointMachine item8 in (List<ShipAttachmentPointMachine>)(object)pirateShip.AttachmentPointMachines)
				{
					((MissionObject)item8).SetDisabled(false);
					foreach (StandingPoint item9 in (List<StandingPoint>)(object)((UsableMachine)item8).StandingPoints)
					{
						((MissionObject)item9).SetDisabled(false);
					}
				}
				return;
			}
		}
		if (_isPirateShipMovementDisabled[pirateShip])
		{
			pirateShip.SetAnchor(isAnchored: true);
			pirateShip.ShipOrder.SetShipStopOrder();
			pirateShip.Formation.SetMovementOrder(MovementOrder.MovementOrderStop);
			foreach (ShipAttachmentMachine item10 in (List<ShipAttachmentMachine>)(object)pirateShip.AttachmentMachines)
			{
				((MissionObject)item10).SetDisabled(true);
			}
			{
				foreach (ShipAttachmentPointMachine item11 in (List<ShipAttachmentPointMachine>)(object)pirateShip.AttachmentPointMachines)
				{
					((MissionObject)item11).SetDisabled(false);
					foreach (StandingPoint item12 in (List<StandingPoint>)(object)((UsableMachine)item11).StandingPoints)
					{
						((MissionObject)item12).SetDisabled(false);
					}
				}
				return;
			}
		}
		globalPosition = _pirateShipTriggerPoints[pirateShip].GlobalPosition;
		if (((Vec3)(ref globalPosition)).Distance(EscapeShip.GlobalFrame.origin) < 170f)
		{
			_isPirateShipTriggered[pirateShip] = true;
			pirateShip.SetController(ShipControllerType.None);
			pirateShip.SetAnchor(isAnchored: false);
			pirateShip.SetShipOrderActive(isOrderActive: true);
			ShipOrder shipOrder2 = pirateShip.ShipOrder;
			globalPosition = _pirateShipTriggerPoints[pirateShip].GlobalPosition;
			targetPosition = ((Vec3)(ref globalPosition)).AsVec2;
			shipOrder2.SetShipMovementOrder(in targetPosition);
			if (_instructionState == Quest5InstructionState.WaitForEscapeQuietly)
			{
				_instructionState = Quest5InstructionState.EscapeQuietly;
			}
		}
		else
		{
			pirateShip.SetAnchor(isAnchored: true);
			pirateShip.ShipOrder.SetShipStopOrder();
			pirateShip.SetShipOrderActive(isOrderActive: false);
		}
	}

	private void HandlePirateShipSailModeAccordingToTheGlobalWindVelocity(MissionShip ship)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (_navalAgentsLogic.GetActiveAgentCountOfShip(ship) <= 0)
		{
			ship.SetAnchor(isAnchored: true);
			return;
		}
		MatrixFrame globalFrame = EscapeShip.GlobalFrame;
		if (((Vec3)(ref globalFrame.origin)).Distance(ship.GlobalFrame.origin) < 40f)
		{
			ship.SetCustomSailSetting(enableCustomSailSetting: true, SailInput.Raised);
			return;
		}
		globalFrame = ship.GlobalFrame;
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Vec2 val2 = ((Vec2)(ref val)).Normalized();
		val = ((MissionBehavior)this).Mission.Scene.GetGlobalWindVelocity();
		Vec2 val3 = ((Vec2)(ref val)).Normalized();
		float num = MathF.Abs(Vec2.DotProduct(val2, val3));
		ship.SetCustomSailSetting(enableCustomSailSetting: true, (num > 0.75f) ? SailInput.Full : SailInput.Raised);
	}

	private bool GetIsThereActiveBridgeToBetweenEscapeShipAndAnyPirateShips()
	{
		if (!EscapeShip.GetIsThereActiveBridgeTo(_phase2EnemyShip1) && !EscapeShip.GetIsThereActiveBridgeTo(_phase2EnemyShip2) && !EscapeShip.GetIsThereActiveBridgeTo(_phase2EnemyShip3) && !EscapeShip.GetIsThereActiveBridgeTo(_phase2EnemyShip4))
		{
			return EscapeShip.GetIsThereActiveBridgeTo(_phase2EnemyShip5);
		}
		return true;
	}

	private void HandleStationaryShipMovement(MissionShip stationaryShip)
	{
		stationaryShip.SetAnchor(isAnchored: true);
		stationaryShip.ShipOrder.SetShipStopOrder();
		stationaryShip.SetShipOrderActive(isOrderActive: false);
		stationaryShip.SetCustomSailSetting(enableCustomSailSetting: true, SailInput.Raised);
		foreach (Agent item in (List<Agent>)(object)_navalAgentsLogic.GetActiveAgentsOfShip(stationaryShip))
		{
			if (item.IsUsingGameObject)
			{
				item.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			}
		}
		stationaryShip.Formation.SetTargetFormation(_playerFormation);
	}

	private void AutoEstablishConnectionsForPirateShips(MissionShip ship, GameEntity finalTargetFrameEntity)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		if (_isPirateShipMovementDisabled[ship] || !_isPirateShipMovingToTheEscapeShip[ship])
		{
			return;
		}
		if (_autoEstablishConnectionsForPirateShips[ship] == null)
		{
			if (!EscapeShip.GetIsThereActiveBridgeTo(ship))
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
				MatrixFrame val = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
				Vec2 asVec = ((Vec3)(ref val.origin)).AsVec2;
				val = finalTargetFrameEntity.GetGlobalFrame();
				if (((Vec2)(ref asVec)).DistanceSquared(((Vec3)(ref val.origin)).AsVec2) <= 2f)
				{
					_autoEstablishConnectionsForPirateShips[ship] = new MissionTimer(7f);
				}
			}
		}
		else if (_autoEstablishConnectionsForPirateShips[ship].Check(false) && !EscapeShip.GetIsThereActiveBridgeTo(ship))
		{
			EscapeShip.TryToConnectionToAttachmentMachine(_pirateShipEnabledAttachmentMachine[ship]);
			_autoEstablishConnectionsForPirateShips[ship] = null;
		}
	}

	private void AutoCutLooseEmptyPirateShipIfPlayerDoesNotForALongTime(MissionShip ship)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		if (_autoCutLooseTimersForPirateShips[ship] == null)
		{
			if (EscapeShip.GetIsThereActiveBridgeTo(ship))
			{
				_autoCutLooseTimersForPirateShips[ship] = new MissionTimer(25f);
			}
		}
		else
		{
			if (!_autoCutLooseTimersForPirateShips[ship].Check(false))
			{
				return;
			}
			_isPirateShipLostItsCrew[ship] = true;
			_isPirateShipMovingToTheEscapeShip[ship] = false;
			_isPirateShipTriggered[ship] = false;
			_isPirateShipMovementDisabled[ship] = true;
			foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ship.AttachmentMachines)
			{
				if (item.CurrentAttachment != null)
				{
					item.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
				}
			}
			foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)ship.AttachmentPointMachines)
			{
				if (item2.CurrentAttachment != null)
				{
					item2.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
				}
			}
			_autoCutLooseTimersForPirateShips[ship] = null;
		}
	}

	private void SetShipAttachmentJointPhysicsEnabledForShip(MissionShip ship, bool enabled)
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ship.ShipAttachmentMachines)
		{
			item.SetShipAttachmentJointPhysicsEnabled(enabled);
		}
	}

	private void SetDisableShipAttachmentMachinesForPlayer(MissionShip ship, bool isDisabled)
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ship.ShipAttachmentMachines)
		{
			if (isDisabled)
			{
				((MissionObject)item).SetDisabled(false);
			}
			else
			{
				((MissionObject)item).SetEnabled(false);
			}
		}
	}

	private void OnAttachmentBroken(ShipAttachmentMachine attachmentMachine, ShipAttachmentPointMachine attachmentPointMachine)
	{
		MissionShip ownerShip = attachmentMachine.OwnerShip;
		if (ownerShip == EscapeShip || attachmentPointMachine == null || ((UsableMachine)attachmentPointMachine).PilotAgent == null || ((UsableMachine)attachmentPointMachine).PilotAgent != Agent.Main || !_isPirateShipMovingToTheEscapeShip.TryGetValue(ownerShip, out var _))
		{
			return;
		}
		_isPirateShipMovingToTheEscapeShip[ownerShip] = false;
		_isPirateShipLostItsCrew[ownerShip] = true;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ownerShip.ShipAttachmentMachines)
		{
			_ = item;
			if (attachmentMachine.CurrentAttachment != null)
			{
				attachmentMachine.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
			}
			((MissionObject)attachmentMachine).SetDisabled(true);
		}
		((MissionObject)ownerShip.ShipControllerMachine).SetDisabled(true);
		foreach (ShipOarMachine item2 in (List<ShipOarMachine>)(object)ownerShip.LeftSideShipOarMachines)
		{
			((MissionObject)item2).SetDisabled(true);
		}
		foreach (ShipOarMachine item3 in (List<ShipOarMachine>)(object)ownerShip.RightSideShipOarMachines)
		{
			((MissionObject)item3).SetDisabled(true);
		}
	}

	private void HandleAllyShipMovementDuringPhase2(MissionShip ship)
	{
		ship.SetAnchor(isAnchored: true);
		ship.ShipOrder.SetShipStopOrder();
		ship.SetController(ShipControllerType.None);
	}

	private void HandlePirateShipBridgeConnectionCount(MissionShip pirateShip)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		if (EscapeShip.GetIsThereActiveBridgeTo(pirateShip))
		{
			if (!_isMissionShipBoardedToTheEscapeShip[pirateShip])
			{
				_isMissionShipBoardedToTheEscapeShip[pirateShip] = true;
				DialogNotificationHandle item = CampaignInformationManager.AddDialogLine(new TextObject("{=s3PsXlsG}They've grappled us!", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)3);
				_dialogNotificationHandleCache.Add(item);
			}
			bool flag = true;
			pirateShip.Formation.SetMovementOrder(MovementOrder.MovementOrderCharge);
			foreach (Agent item3 in (List<Agent>)(object)_navalAgentsLogic.GetActiveAgentsOfShip(pirateShip))
			{
				item3.SetAutomaticTargetSelection(false);
				item3.SetTargetAgent(Agent.Main);
				flag = flag && EscapeShip.GetIsAgentOnShip(item3);
			}
			if (!flag || !Agent.Main.IsActive() || !EscapeShip.GetIsAgentOnShip(Agent.Main))
			{
				return;
			}
			DialogNotificationHandle item2 = CampaignInformationManager.AddDialogLine(new TextObject("{=RUavLWSF}They're on deck!", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)3);
			_dialogNotificationHandleCache.Add(item2);
			_isPirateShipMovingToTheEscapeShip[pirateShip] = false;
			_isPirateShipLostItsCrew[pirateShip] = true;
			_isPirateShipTriggered[pirateShip] = false;
			pirateShip.SetAnchor(isAnchored: true);
			pirateShip.Formation.SetMovementOrder(MovementOrder.MovementOrderCharge);
			foreach (ShipAttachmentMachine item4 in (List<ShipAttachmentMachine>)(object)pirateShip.ShipAttachmentMachines)
			{
				if (item4.CurrentAttachment != null)
				{
					item4.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
				}
				((MissionObject)item4).SetDisabled(true);
			}
			((MissionObject)pirateShip.ShipControllerMachine).SetDisabled(true);
			foreach (ShipOarMachine item5 in (List<ShipOarMachine>)(object)pirateShip.LeftSideShipOarMachines)
			{
				((MissionObject)item5).SetDisabled(true);
			}
			{
				foreach (ShipOarMachine item6 in (List<ShipOarMachine>)(object)pirateShip.RightSideShipOarMachines)
				{
					((MissionObject)item6).SetDisabled(true);
				}
				return;
			}
		}
		if (_isMissionShipBoardedToTheEscapeShip[pirateShip])
		{
			_isMissionShipBoardedToTheEscapeShip[pirateShip] = false;
		}
	}

	private bool AreAllPhase2PirateShipsEliminated()
	{
		if (_phase2EnemyShip1.Formation.CountOfUnits <= 0 && _phase2EnemyShip2.Formation.CountOfUnits <= 0 && _phase2EnemyShip3.Formation.CountOfUnits <= 0 && _phase2EnemyShip4.Formation.CountOfUnits <= 0)
		{
			return _phase2EnemyShip5.Formation.CountOfUnits <= 0;
		}
		return false;
	}

	private void HandlePlayersBridgeAndControlPointUsagesForPhase2InProgress()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			foreach (ShipAttachmentMachine item2 in (List<ShipAttachmentMachine>)(object)item.AttachmentMachines)
			{
				foreach (StandingPoint item3 in (List<StandingPoint>)(object)((UsableMachine)item2).StandingPoints)
				{
					((UsableMissionObject)item3).IsDisabledForPlayers = false;
				}
			}
			foreach (ShipAttachmentPointMachine item4 in (List<ShipAttachmentPointMachine>)(object)item.AttachmentPointMachines)
			{
				foreach (StandingPoint item5 in (List<StandingPoint>)(object)((UsableMachine)item4).StandingPoints)
				{
					((UsableMissionObject)item5).IsDisabledForPlayers = false;
				}
			}
		}
	}

	private void CheckForEscapeShipStuck()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckIfThereIsAnActiveAgentOfShip(_phase2EnemyShip1) || !CheckIfThereIsAnActiveAgentOfShip(_phase2EnemyShip2) || !CheckIfThereIsAnActiveAgentOfShip(_phase2EnemyShip3) || !CheckIfThereIsAnActiveAgentOfShip(_phase2EnemyShip4) || !CheckIfThereIsAnActiveAgentOfShip(_phase2EnemyShip5))
		{
			return;
		}
		if (_phase2EscapeShipStuckTimer == null)
		{
			_phase2EscapeShipStuckTimer = new MissionTimer(10f);
			_phase2EscapeShipStuckCheckPosition = EscapeShip.GlobalFrame.origin;
		}
		else if (_phase2EscapeShipStuckTimer.Check(false))
		{
			MatrixFrame globalFrame = EscapeShip.GlobalFrame;
			if (((Vec3)(ref globalFrame.origin)).NearlyEquals(ref _phase2EscapeShipStuckCheckPosition, 3f))
			{
				IsEscapeShipStuck = true;
				return;
			}
			_phase2EscapeShipStuckTimer = null;
			_phase2EscapeShipStuckCheckPosition = Vec3.Invalid;
		}
	}

	private bool CheckIfThereIsAnActiveAgentOfShip(MissionShip ship)
	{
		if (ship != null && _isPirateShipTriggered.ContainsKey(ship) && _isPirateShipTriggered[ship] && _navalAgentsLogic.GetActiveAgentCountOfShip(ship) > 0)
		{
			return false;
		}
		return true;
	}

	public void HandleEscapeShipStuck()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		IsEscapeShipStuck = false;
		_phase2EscapeShipStuckTimer = null;
		_phase2EscapeShipStuckCheckPosition = Vec3.Invalid;
		_navalShipsLogic.TeleportShip(EscapeShip, _currentPhase2EscapeShipTargetPoint.GetGlobalFrame(), checkFreeArea: true);
	}

	private void MoveEscapeShipAlongTheTrack(float fixedDt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (_escapeShipSpeed != 0f)
		{
			Vec2 val = _escapeShipDirection * _escapeShipSpeed * fixedDt;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)EscapeShip).GameEntity;
			MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			Vec2 targetPosition = ((Vec3)(ref bodyWorldTransform.origin)).AsVec2 + val;
			EscapeShip.MoveShipToTheTargetWithDirection(bodyWorldTransform, targetPosition, _escapeShipDirection, 100f, 2.5f, fixedDt);
		}
	}

	private void UpdatePhase2MovingShipParameters(float dt)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		_escapeShipSpeed = MathF.Lerp(_escapeShipSpeed, _escapeShipTargetSpeed, dt * 0.25f, 1E-05f);
		_escapeShipDirection = Vec2.Slerp(_escapeShipDirection, _escapeShipTargetDirection, dt * 0.15f);
	}

	private void ModifyMainAgentEquipmentForPhase2()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>("nord_shield_tier_2_d");
		Equipment val2 = Agent.Main.SpawnEquipment.Clone(false);
		for (int i = 0; i < 12; i++)
		{
			EquipmentElement val3 = val2[i];
			ItemObject item = ((EquipmentElement)(ref val3)).Item;
			if (item != null && ((MBObjectBase)item).StringId.Equals("Broad_Skaen"))
			{
				val2[i] = new EquipmentElement(val, (ItemModifier)null, (ItemObject)null, false);
				break;
			}
		}
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(val2);
	}

	public void TriggerInitializePhase3()
	{
		State = Quest5SetPieceBattleMissionState.InitializePhase3Part1;
	}

	public void CompletePhase2ToPhase3Transition()
	{
		State = Quest5SetPieceBattleMissionState.Phase3InProgress;
	}

	private void InitializePhase3Part1()
	{
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		_gunnarMovementState = GunnarMovementState.End;
		if (_phase2EnemyShip1 != null)
		{
			_phase2EnemyShip1.BreakAllExistingConnections();
			AddAvailableEnemyFormation(_phase2EnemyShip1.Formation);
			RemoveShipInternal(_phase2EnemyShip1);
			_phase2EnemyShip1 = null;
		}
		else
		{
			_isCheckpointInitialize = true;
		}
		if (_isCheckpointInitialize)
		{
			TeamAINavalComponent teamAINavalComponent = new TeamAINavalComponent(((MissionBehavior)this).Mission, ((MissionBehavior)this).Mission.AttackerTeam, 5f, 1f);
			((MissionBehavior)this).Mission.AttackerTeam.AddTeamAI((TeamAIComponent)(object)teamAINavalComponent, false);
			((MissionBehavior)this).Mission.AttackerTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(((MissionBehavior)this).Mission.AttackerTeam));
			TeamAINavalComponent teamAINavalComponent2 = new TeamAINavalComponent(((MissionBehavior)this).Mission, ((MissionBehavior)this).Mission.DefenderTeam, 5f, 1f);
			((MissionBehavior)this).Mission.DefenderTeam.AddTeamAI((TeamAIComponent)(object)teamAINavalComponent2, false);
			((MissionBehavior)this).Mission.DefenderTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(((MissionBehavior)this).Mission.DefenderTeam));
		}
		if (_phase2EnemyShip2 != null)
		{
			_phase2EnemyShip2.BreakAllExistingConnections();
			AddAvailableEnemyFormation(_phase2EnemyShip2.Formation);
			RemoveShipInternal(_phase2EnemyShip2);
			_phase2EnemyShip2 = null;
		}
		else
		{
			_isCheckpointInitialize = true;
		}
		if (_phase2EnemyShip3 != null)
		{
			_phase2EnemyShip3.BreakAllExistingConnections();
			AddAvailableEnemyFormation(_phase2EnemyShip3.Formation);
			RemoveShipInternal(_phase2EnemyShip3);
			_phase2EnemyShip3 = null;
		}
		else
		{
			_isCheckpointInitialize = true;
		}
		if (_phase2EnemyShip4 != null)
		{
			_phase2EnemyShip4.BreakAllExistingConnections();
			AddAvailableEnemyFormation(_phase2EnemyShip4.Formation);
			RemoveShipInternal(_phase2EnemyShip4);
			_phase2EnemyShip4 = null;
		}
		else
		{
			_isCheckpointInitialize = true;
		}
		if (_phase2EnemyShip5 != null)
		{
			_phase2EnemyShip5.BreakAllExistingConnections();
			AddAvailableEnemyFormation(_phase2EnemyShip5.Formation);
			RemoveShipInternal(_phase2EnemyShip5);
			_phase2EnemyShip5 = null;
		}
		else
		{
			_isCheckpointInitialize = true;
		}
		if (_phase1EnemyShip2 != null)
		{
			_phase1EnemyShip2.BreakAllExistingConnections();
			AddAvailableEnemyFormation(_phase1EnemyShip2.Formation);
			RemoveShipInternal(_phase1EnemyShip2);
		}
		if (_phase1EnemyShip4 != null)
		{
			_phase1EnemyShip4.BreakAllExistingConnections();
			AddAvailableEnemyFormation(_phase1EnemyShip4.Formation);
			RemoveShipInternal(_phase1EnemyShip4);
		}
		if (_phase2EnemyShipStationary1 != null)
		{
			_phase2EnemyShipStationary1.BreakAllExistingConnections();
			AddAvailableEnemyFormation(_phase2EnemyShipStationary1.Formation);
			RemoveShipInternal(_phase2EnemyShipStationary1);
		}
		_phase3EnemyShip1 = CreateShip("eastern_heavy_ship", "phase_3_enemy_ship_1_sp", GetAvailableEnemyFormation(), spawnAnchored: false, _phase3EnemyShip1UpgradePieceList);
		_phase3EnemyShip2 = CreateShip("aserai_heavy_ship", "phase_3_enemy_ship_2_sp", GetAvailableEnemyFormation(), spawnAnchored: false, _phase3EnemyShip2UpgradePieceList);
		_phase3EnemyShip3 = CreateShip("nord_medium_ship", "phase_3_enemy_ship_3_sp", GetAvailableEnemyFormation(), spawnAnchored: false, _phase3EnemyShip3UpgradePieceList);
		_phase3EnemyShip4 = CreateShip("nord_medium_ship", "phase_3_enemy_ship_4_sp", GetAvailableEnemyFormation(), spawnAnchored: false, _phase3EnemyShip4UpgradePieceList);
		_phase3EnemyShip5 = CreateShip("khuzait_heavy_ship", "phase_3_enemy_ship_5_sp", GetAvailableEnemyFormation(), spawnAnchored: false, _phase3EnemyShip5UpgradePieceList);
		_phase3EnemyShip1.SetCanBeTakenOver(value: false);
		_phase3EnemyShip2.SetCanBeTakenOver(value: false);
		_phase3EnemyShip3.SetCanBeTakenOver(value: false);
		_phase3EnemyShip4.SetCanBeTakenOver(value: false);
		_phase3EnemyShip5.SetCanBeTakenOver(value: false);
		if (_phase2AllyShip1 != null)
		{
			GameEntity val = Mission.Current.Scene.FindEntityWithTag("phase_3_ally_ship_1_sp");
			_navalShipsLogic.TeleportShip(_phase2AllyShip1, val.GetGlobalFrame(), checkFreeArea: true);
		}
		else
		{
			_phase2AllyShip1 = CreateShip("aserai_heavy_ship", "phase_3_ally_ship_1_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip1UpgradePieceList);
		}
		if (_phase2AllyShip2 != null)
		{
			GameEntity val2 = Mission.Current.Scene.FindEntityWithTag("phase_3_ally_ship_2_sp");
			_navalShipsLogic.TeleportShip(_phase2AllyShip2, val2.GetGlobalFrame(), checkFreeArea: true);
		}
		else
		{
			_phase2AllyShip2 = CreateShip("nord_medium_ship", "phase_3_ally_ship_2_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip2UpgradePieceList);
		}
		if (_phase2AllyShip3 != null)
		{
			GameEntity val3 = Mission.Current.Scene.FindEntityWithTag("phase_3_ally_ship_3_sp");
			_navalShipsLogic.TeleportShip(_phase2AllyShip3, val3.GetGlobalFrame(), checkFreeArea: true);
		}
		else
		{
			_phase2AllyShip3 = CreateShip("northern_medium_ship", "phase_3_ally_ship_3_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip3UpgradePieceList);
		}
		if (_phase2AllyShip4 != null)
		{
			GameEntity val4 = Mission.Current.Scene.FindEntityWithTag("phase_3_ally_ship_4_sp");
			_navalShipsLogic.TeleportShip(_phase2AllyShip4, val4.GetGlobalFrame(), checkFreeArea: true);
		}
		else
		{
			_phase2AllyShip4 = CreateShip("sturgia_heavy_ship", "phase_3_ally_ship_4_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip4UpgradePieceList);
		}
		if (_phase2AllyShip5 != null)
		{
			GameEntity val5 = Mission.Current.Scene.FindEntityWithTag("phase_3_ally_ship_5_sp");
			_navalShipsLogic.TeleportShip(_phase2AllyShip5, val5.GetGlobalFrame(), checkFreeArea: true);
		}
		else
		{
			_phase2AllyShip5 = CreateShip("northern_medium_ship", "phase_3_ally_ship_5_sp", GetAvailableAllyFormation(), spawnAnchored: false, _phase2AllyShip5UpgradePieceList);
		}
		_navalTrajectoryPlanningLogic.ForceReinitialize();
		foreach (DialogNotificationHandle item in _dialogNotificationHandleCache)
		{
			CampaignInformationManager.ClearDialogNotification(item, true);
		}
		_dialogNotificationHandleCache.Clear();
		if (!_isCheckpointInitialize)
		{
			_lightScriptedFiresMissionController.PutOutFires();
		}
	}

	private void InitializePhase3Part2()
	{
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Expected O, but got Unknown
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		Mission.Current.Scene.SetAtmosphereWithName("TOD_naval_05_30_sunset");
		if (_playerShip != null)
		{
			foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_playerShip.AttachmentMachines)
			{
				if (item.CurrentAttachment != null)
				{
					item.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
				}
			}
			if (Agent.Main.IsUsingGameObject)
			{
				Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			}
			_navalAgentsLogic.TransferAgentToShip(Agent.Main, _phase2AllyShip1);
			Agent gunnarAgent = _gunnarAgent;
			if (gunnarAgent != null && gunnarAgent.IsActive())
			{
				_gunnarAgent.Controller = (AgentControllerType)1;
				_navalAgentsLogic.TransferAgentToShip(_gunnarAgent, _phase2AllyShip1);
			}
			if (_playerShip != null)
			{
				if (_gunnarAgent.IsUsingGameObject)
				{
					_gunnarAgent.Controller = (AgentControllerType)1;
					_gunnarAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				}
				RemoveShipInternal(_playerShip);
			}
			if (_phase1EnemyShip3 != null && _phase1EnemyShip3.Team != null)
			{
				Agent gunnarAgent2 = _gunnarAgent;
				if (gunnarAgent2 != null && gunnarAgent2.IsUsingGameObject)
				{
					_gunnarAgent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
				}
				_navalAgentsLogic.UnassignCaptainOfShip(_phase1EnemyShip3);
				RemoveShipInternal(_phase1EnemyShip3);
			}
			_navalShipsLogic.SetDeploymentMode(value: true);
			_navalAgentsLogic.SetDeploymentMode(value: true);
			_playerShip = CreateShip("empire_heavy_ship", "phase_3_player_ship_sp", _playerFormation, spawnAnchored: false, _escapeShipUpgradePieceList, EscapeShipFigurehead);
			_navalShipsLogic.SetDeploymentMode(value: false);
			_navalAgentsLogic.SetDeploymentMode(value: false);
			_navalAgentsLogic.TransferAgentToShip(Agent.Main, _playerShip);
			Agent gunnarAgent3 = _gunnarAgent;
			if (gunnarAgent3 != null && gunnarAgent3.IsActive())
			{
				_navalAgentsLogic.TransferAgentToShip(_gunnarAgent, _playerShip);
			}
			_playerShip.ShipOrder.SetShipStopOrder();
			_playerShip.Formation.PlayerOwner = Agent.Main;
		}
		else
		{
			_playerShip = CreateShip("empire_heavy_ship", "phase_3_player_ship_sp", _playerFormation, spawnAnchored: false, _escapeShipUpgradePieceList, EscapeShipFigurehead);
			_navalAgentsLogic.SetDeploymentMode(value: true);
			_navalShipsLogic.SetDeploymentMode(value: true);
			_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, CharacterObject.PlayerCharacter, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
			_navalAgentsLogic.SetDeploymentMode(value: false);
			_navalShipsLogic.SetDeploymentMode(value: false);
			_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
			_playerShip.Formation.PlayerOwner = Agent.Main;
		}
		_navalTrajectoryPlanningLogic.ForceReinitialize();
		Agent.Main.TeleportToPosition(_playerShip.GetCaptainSpawnGlobalFrame().origin);
	}

	private void ClearPhase2OnPhaseTransition()
	{
		_phase2EnemyShip1 = null;
		_phase2EnemyShip2 = null;
		_phase2EnemyShip3 = null;
		_phase2EnemyShip4 = null;
		_phase2EnemyShip5 = null;
		_phase2EnemyShipStationary1 = null;
		_phase2EscapeShipPirateTargetFrame1 = null;
		_phase2EscapeShipPirateTargetFrame2 = null;
		_phase2EscapeShipPirateTargetFrame3 = null;
		_phase2EscapeShipPirateTargetFrame4 = null;
		_phase2EscapeShipPirateTargetFrame5 = null;
		_currentPhase2EscapeShipTargetPoint = null;
		_pirateShipTriggerPoints.Clear();
		_isPirateShipTriggered.Clear();
		_isPirateShipMovingToTheEscapeShip.Clear();
		_isPirateShipLostItsCrew.Clear();
		_limitPirateShipChasingSpeed.Clear();
		_autoCutLooseTimersForPirateShips.Clear();
		_isMissionShipBoardedToTheEscapeShip.Clear();
		_phase2EscapeShipTargetPointEntities.Clear();
		_phase2EscapeShipTargetPoints.Clear();
		_playerLeftTheEscapeShipTimer = null;
		_phase2EscapeShipStuckTimer = null;
		GC.Collect();
	}

	private void InitializePhase3Part3()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		_navalAgentsLogic.AssignCaptainToShip(Agent.Main, _playerShip);
		_playerShip.Formation.PlayerOwner = Agent.Main;
		SpawnPhase3EnemyTroops();
		SpawnPhase3AllyTroops();
		Agent.Main.SetClothingColor1(4279111698u);
		Agent.Main.SetClothingColor2(4279111698u);
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(Hero.MainHero.BattleEquipment);
		_gunnarAgent.UpdateSpawnEquipmentAndRefreshVisuals(((BasicCharacterObject)NavalStorylineData.Gunnar.CharacterObject).Equipment);
		_gunnarAgent.TeleportToPosition(_playerShip.GetCaptainSpawnGlobalFrame().origin);
		if (_isCheckpointInitialize)
		{
			Mission.Current.OnDeploymentFinished();
		}
		else
		{
			((MissionObject)_phase3EnemyShip1).OnDeploymentFinished();
			((MissionObject)_phase3EnemyShip2).OnDeploymentFinished();
			((MissionObject)_phase3EnemyShip3).OnDeploymentFinished();
			((MissionObject)_phase3EnemyShip4).OnDeploymentFinished();
			((MissionObject)_phase3EnemyShip5).OnDeploymentFinished();
			((MissionObject)_playerShip).OnDeploymentFinished();
			_navalTrajectoryPlanningLogic.ForceReinitialize();
		}
		TriggerShip(_phase3EnemyShip1);
		TriggerShip(_phase3EnemyShip2);
		TriggerShip(_phase3EnemyShip3);
		TriggerShip(_phase3EnemyShip4);
		TriggerShip(_phase3EnemyShip5);
		TriggerShip(_phase2AllyShip1);
		TriggerShip(_phase2AllyShip2);
		TriggerShip(_phase2AllyShip3);
		TriggerShip(_phase2AllyShip4);
		TriggerShip(_phase2AllyShip5);
		_gunnarAgent.Controller = (AgentControllerType)1;
		_instructionState = Quest5InstructionState.DefeatEnemies;
		State = (_isCheckpointInitialize ? Quest5SetPieceBattleMissionState.Phase3InProgress : Quest5SetPieceBattleMissionState.Phase2ToPhase3FadeIn);
		_playerShip.SetController(ShipControllerType.Player);
		HandlePlayersBridgeAndControlPointUsagesForPhase3InProgress();
		AdjustWindDirectionAccordingToTargetFrame(_playerShip.GlobalFrame, 3f);
		ShowStartNotifications();
		RemoveShipControlPointDescriptionOfAllEnemyShips();
		_phase3TotalEnemyCount = Phase3EnemyShip1TroopCount + Phase3EnemyShip2TroopCount + Phase3EnemyShip3TroopCount + Phase3EnemyShip4TroopCount + Phase3EnemyShip5TroopCount;
		foreach (Formation item in (List<Formation>)(object)((MissionBehavior)this).Mission.PlayerTeam.FormationsIncludingEmpty)
		{
			item.PlayerOwner = Agent.Main;
		}
		if (!_gunnarAgent.IsAlarmed())
		{
			_gunnarAgent.SetAlarmState((AIStateFlag)3);
		}
		if (!_isCheckpointInitialize)
		{
			ClearPhase2OnPhaseTransition();
		}
	}

	private void SpawnPhase3EnemyTroops()
	{
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase3EnemyShip1, Phase3EnemyShip1TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase3EnemyShip2, Phase3EnemyShip2TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase3EnemyShip3, Phase3EnemyShip3TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase3EnemyShip4, Phase3EnemyShip4TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase3EnemyShip5, Phase3EnemyShip5TroopCount);
		AddMissionShipTroops(_phase3EnemyShip1Troops, _phase3EnemyShip1);
		AddMissionShipTroops(_phase3EnemyShip2Troops, _phase3EnemyShip2);
		AddMissionShipTroops(_phase3EnemyShip3Troops, _phase3EnemyShip3);
		AddMissionShipTroops(_phase3EnemyShip4Troops, _phase3EnemyShip4);
		AddMissionShipTroops(_phase3EnemyShip5Troops, _phase3EnemyShip5);
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)2);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase3EnemyShip1);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase3EnemyShip2);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase3EnemyShip3);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase3EnemyShip4);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase3EnemyShip5);
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
	}

	private void SpawnPhase3AllyTroops()
	{
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_playerShip, Phase3PlayerShipTroopCount + 2);
		AddMissionShipTroops(_phase3PlayerShipTroops, _playerShip, PartyBase.MainParty);
		if (_isCheckpointInitialize)
		{
			_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip1, Phase2AllyShip1TroopCount + 2);
			_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip2, Phase2AllyShip2TroopCount + 2);
			_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip3, Phase2AllyShip3TroopCount + 2);
			_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip4, Phase2AllyShip4TroopCount + 2);
			_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase2AllyShip5, Phase2AllyShip5TroopCount + 2);
			AddMissionShipTroops(_phase2AllyShip1Troops, _phase2AllyShip1, PartyBase.MainParty);
			AddMissionShipTroops(_phase2AllyShip2Troops, _phase2AllyShip2, PartyBase.MainParty);
			AddMissionShipTroops(_phase2AllyShip3Troops, _phase2AllyShip3, PartyBase.MainParty);
			AddMissionShipTroops(_phase2AllyShip4Troops, _phase2AllyShip4, PartyBase.MainParty);
			AddMissionShipTroops(_phase2AllyShip5Troops, _phase2AllyShip5, PartyBase.MainParty);
		}
		SpawnBjolgurOnShip(_phase2AllyShip2);
		SpawnLaharOnShip(_phase2AllyShip3);
		if (_gunnarAgent == null || !_gunnarAgent.IsActive())
		{
			SpawnGunnarOnShip(_playerShip);
		}
		_gunnarAgent.SetMortalityState((MortalityState)2);
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_playerShip);
		if (_isCheckpointInitialize)
		{
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip1);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip2);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip3);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip4);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase2AllyShip5);
		}
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
	}

	private void CallReinforcement()
	{
		_isReinforcementCalled = true;
		_phase3EnemyReinforcementShip1 = CreateShip("empire_medium_ship", "phase_3_enemy_reinforcement_1_sp", GetAvailableEnemyFormation());
		_phase3EnemyReinforcementShip2 = CreateShip("nord_medium_ship", "phase_3_enemy_reinforcement_2_sp", GetAvailableEnemyFormation());
		_phase3EnemyReinforcementShip1.SetCanBeTakenOver(value: false);
		_phase3EnemyReinforcementShip2.SetCanBeTakenOver(value: false);
	}

	private void InitializeReinforcement()
	{
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		_isReinforcementInitialized = true;
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase3EnemyReinforcementShip1, Phase3EnemyReinforcementShip1TroopCount);
		_navalAgentsLogic.SetDesiredTroopCountOfShip(_phase3EnemyReinforcementShip2, Phase3EnemyReinforcementShip2TroopCount);
		_phase3TotalEnemyCount += Phase3EnemyReinforcementShip1TroopCount + Phase3EnemyReinforcementShip2TroopCount;
		AddMissionShipTroops(_phase3EnemyReinforcementShip1Troops, _phase3EnemyReinforcementShip1);
		AddMissionShipTroops(_phase3EnemyReinforcementShip2Troops, _phase3EnemyReinforcementShip2);
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)2);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase3EnemyReinforcementShip1);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_phase3EnemyReinforcementShip2);
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
		((MissionObject)_phase3EnemyReinforcementShip1).OnDeploymentFinished();
		((MissionObject)_phase3EnemyReinforcementShip2).OnDeploymentFinished();
		_navalTrajectoryPlanningLogic.ForceReinitialize();
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SetOrder((OrderType)4);
		DialogNotificationHandle item = CampaignInformationManager.AddDialogLine(new TextObject("{=jxQc5JVQ}Ah, gods - I see more of them coming up... No rest for my sword-arm today!", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)3);
		_dialogNotificationHandleCache.Add(item);
		_phase3EnemyReinforcementShip1.ShipOrder.SetShipEngageOrder();
		_phase3EnemyReinforcementShip2.ShipOrder.SetShipEngageOrder();
	}

	private bool CanProceedToPhase4()
	{
		MBReadOnlyList<Agent> activeAgents = ((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents;
		bool flag = ((List<Agent>)(object)activeAgents).Count <= 0;
		if (!flag)
		{
			bool flag2 = true;
			foreach (Agent item in (List<Agent>)(object)activeAgents)
			{
				if (item.Formation != null)
				{
					flag2 = false;
					break;
				}
			}
			flag = flag2;
		}
		return flag;
	}

	public void TriggerInitializePhase4()
	{
		if (Agent.Main.IsUsingGameObject)
		{
			Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		State = Quest5SetPieceBattleMissionState.InitializePhase4Part1;
	}

	public void CompletePhase3ToPhase4Transition()
	{
		State = Quest5SetPieceBattleMissionState.Phase4InProgress;
	}

	private void ShowStartNotifications()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		DialogNotificationHandle item = CampaignInformationManager.AddDialogLine(new TextObject("{=a1IqRXcx}Ahoy to you, Gunnar! An exemplary escape! Is the captive safe?", (Dictionary<string, object>)null), NavalStorylineData.Lahar.CharacterObject, (Equipment)null, 0, (NotificationPriority)3);
		_dialogNotificationHandleCache.Add(item);
		DialogNotificationHandle item2 = CampaignInformationManager.AddDialogLine(new TextObject("{=EdYmUbcM}You two snatched their ship right out from under their noses! A fine story to tell my brothers, if we survive this.", (Dictionary<string, object>)null), NavalStorylineData.Bjolgur.CharacterObject, (Equipment)null, 0, (NotificationPriority)3);
		_dialogNotificationHandleCache.Add(item2);
		DialogNotificationHandle item3 = CampaignInformationManager.AddDialogLine(new TextObject("{=HgdLgYtA}Ahoy to you, Bjolgur! And ahoy to you, Lahar! She is indeed safe, with us. But now it looks like the whole pack of Hounds are coming baying out to meet us. You two brave fellows get on our flanks, and we'll meet them prow to prow", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)3);
		_dialogNotificationHandleCache.Add(item3);
	}

	private void ClearPhase4OnPhaseTransition()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (_phase2AllyShip1 != null)
		{
			((Ship)_phase2AllyShip1.ShipOrigin).Owner = null;
		}
		if (_phase2AllyShip2 != null)
		{
			((Ship)_phase2AllyShip2.ShipOrigin).Owner = null;
		}
		if (_phase2AllyShip3 != null)
		{
			((Ship)_phase2AllyShip3.ShipOrigin).Owner = null;
		}
		if (_phase2AllyShip4 != null)
		{
			((Ship)_phase2AllyShip4.ShipOrigin).Owner = null;
		}
		if (_phase2AllyShip5 != null)
		{
			((Ship)_phase2AllyShip5.ShipOrigin).Owner = null;
		}
		_phase2AllyShip1 = null;
		_phase2AllyShip2 = null;
		_phase2AllyShip3 = null;
		_phase2AllyShip4 = null;
		_phase2AllyShip5 = null;
		_phase3EnemyShip1 = null;
		_phase3EnemyShip2 = null;
		_phase3EnemyShip3 = null;
		_phase3EnemyShip4 = null;
		_phase3EnemyShip5 = null;
		_phase3EnemyReinforcementShip1 = null;
		_phase3EnemyReinforcementShip2 = null;
		_phase3TriggerVolumeBox = null;
		_allyShipTargetKeysBuffer.Clear();
		_assignedEnemyShips.Clear();
		GC.Collect();
	}

	public void TriggerInitializeBossFight()
	{
		State = Quest5SetPieceBattleMissionState.InitializeBossFightPart1;
	}

	public void CompletePhase4ToBossFightTransition()
	{
		State = Quest5SetPieceBattleMissionState.StartBossFightConversation;
	}

	private void HandlePlayersBridgeAndControlPointUsagesForPhase3InProgress()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			foreach (ShipAttachmentMachine item2 in (List<ShipAttachmentMachine>)(object)item.AttachmentMachines)
			{
				foreach (StandingPoint item3 in (List<StandingPoint>)(object)((UsableMachine)item2).StandingPoints)
				{
					((UsableMissionObject)item3).IsDisabledForPlayers = false;
				}
			}
			foreach (ShipAttachmentPointMachine item4 in (List<ShipAttachmentPointMachine>)(object)item.AttachmentPointMachines)
			{
				foreach (StandingPoint item5 in (List<StandingPoint>)(object)((UsableMachine)item4).StandingPoints)
				{
					((UsableMissionObject)item5).IsDisabledForPlayers = false;
				}
			}
		}
	}

	public void OnPurigCutsceneStarted()
	{
		_isPurigCutsceneStarted = true;
		_playerShip.ShipOrder.SetShipStopOrder();
		_playerShip.SetAnchor(isAnchored: true);
	}

	public void OnPurigShipCutsceneEnded()
	{
		_playerShip.SetAnchor(isAnchored: false);
		if (_isPlayerUsingShipAtTheStartOfThePurigCutscene)
		{
			Agent.Main.HandleStartUsingAction((UsableMissionObject)(object)_playerStandingPointAtTheStartOfThePurigCutscene, -1);
			_isPlayerUsingShipAtTheStartOfThePurigCutscene = false;
			_playerStandingPointAtTheStartOfThePurigCutscene = null;
		}
		_playerShip.ShipOrder.SetShipEngageOrder(Phase4PurigShip);
		Phase4PurigShip.ShipOrder.SetShipEngageOrder(_playerShip);
		_instructionState = Quest5InstructionState.DefeatPurigsShip;
	}

	private void CheckIfEnemyAgentFallIntoTheWater()
	{
		MBReadOnlyList<Agent> activeAgents = ((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents;
		if (((List<Agent>)(object)activeAgents).Count >= 10)
		{
			return;
		}
		for (int num = ((List<Agent>)(object)activeAgents).Count - 1; num >= 0; num--)
		{
			Agent val = ((List<Agent>)(object)activeAgents)[num];
			if (val.IsInWater())
			{
				val.FadeOut(true, false);
			}
		}
	}

	public void GetIntendedMainAgentDirectionForBossFight(out Vec3 direction)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = Mission.Current.Scene.FindEntityWithTag("naval_boss_fight_player_sp");
		GameEntity val2 = Mission.Current.Scene.FindEntityWithTag("naval_boss_fight_enemy_boss_sp");
		Vec3 val3 = val2.GlobalPosition - val.GlobalPosition;
		direction = ((Vec3)(ref val3)).NormalizedCopy();
	}

	private void CollectPurigCutsceneNotifications()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		StringHelpers.SetCharacterProperties("QUEST_5_COMPANION", NavalStorylineData.Gunnar.CharacterObject, (TextObject)null, false);
		_purigNotifications.Enqueue(new ConversationSound(new TextObject("{=jm8pWVv6}Who dares provoke the Hounds in their lair? Is that you, {QUEST_5_COMPANION.NAME}? You and your companion? I will fall upon you like an eagle and tear out your livers, I will shatter your ships to splinters!", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Purig.CharacterObject));
		_purigNotifications.Enqueue(new ConversationSound(new TextObject("{=qPaqVlQX}I will spill your blood upon the waters, I will send your corpses to the slimy depths!", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Purig.CharacterObject));
		_purigNotifications.Enqueue(new ConversationSound(new TextObject("{=SdqOuRuL}Your skull will be a home for scuttling things and Ran shall make a toothpick of your shin-bone! Do you hear me!", (Dictionary<string, object>)null), (NotificationPriority)2, NavalStorylineData.Purig.CharacterObject));
	}

	private void CheckAndPlayPurigCutsceneNotifications()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (_isPurigCutsceneStarted && !Extensions.IsEmpty<ConversationSound>((IEnumerable<ConversationSound>)_purigNotifications))
		{
			ConversationSound conversationSound = _purigNotifications.Dequeue();
			DialogNotificationHandle item = CampaignInformationManager.AddDialogLine(conversationSound.Line, conversationSound.Character, (Equipment)null, 0, conversationSound.Priority);
			_dialogNotificationHandleCache.Add(item);
		}
	}

	private void InitializePhase4Part1()
	{
		_playerShip.ShipOrder.SetShipStopOrder();
		Phase4PurigShip = CreateShip("purigs_roundship_storyline", "phase_4_purig_ship_sp", GetAvailableEnemyFormation(), spawnAnchored: false, _phase4PurigsShipUpgradePieceList);
		Phase4PurigShip.SetCanBeTakenOver(value: false);
		if (_playerShip == null)
		{
			_isCheckpointInitialize = true;
			_playerShip = CreateShip("ship_dromon_storyline", "phase_3_player_ship_sp", _playerFormation, spawnAnchored: false, _escapeShipUpgradePieceList);
		}
		CollectPurigCutsceneNotifications();
		State = Quest5SetPieceBattleMissionState.InitializePhase4Part2;
	}

	private void InitializePhase4Part2()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		Phase4PurigShip.SetController(ShipControllerType.AI);
		ShipOrder shipOrder = Phase4PurigShip.ShipOrder;
		Vec3 globalPosition = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("phase_3_enemy_ship_5_sp").GlobalPosition;
		shipOrder.SetShipMovementOrder(((Vec3)(ref globalPosition)).AsVec2);
		SpawnPhase4EnemyTroops();
		((MissionObject)Phase4PurigShip).OnDeploymentFinished();
		_navalTrajectoryPlanningLogic.ForceReinitialize();
		if (_isCheckpointInitialize)
		{
			_navalAgentsLogic.SetDeploymentMode(value: true);
			_navalShipsLogic.SetDeploymentMode(value: true);
			_playerShip.Formation.PlayerOwner = Agent.Main;
			_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, CharacterObject.PlayerCharacter, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
			SpawnGunnarOnShip(_playerShip);
			_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(_playerShip);
			_navalAgentsLogic.SetDeploymentMode(value: false);
			_navalShipsLogic.SetDeploymentMode(value: false);
			State = Quest5SetPieceBattleMissionState.Phase4InProgress;
		}
		else
		{
			State = Quest5SetPieceBattleMissionState.Phase3ToPhase4FadeIn;
		}
		RemoveShipControlPointDescriptionOfAllEnemyShips();
		_purigShipAgents = new List<Agent>((IEnumerable<Agent>)_navalAgentsLogic.GetActiveAgentsOfShip(Phase4PurigShip));
	}

	private void SpawnPhase4EnemyTroops()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>("sea_hounds");
		_navalAgentsLogic.SetDesiredTroopCountOfShip(Phase4PurigShip, 40);
		for (int i = 0; i < 40; i++)
		{
			_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor)), Phase4PurigShip);
		}
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)2);
		SpawnImmortalAgents();
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines(Phase4PurigShip);
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
	}

	private void SpawnImmortalAgents()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Expected O, but got Unknown
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = Mission.Current.Scene.FindEntityWithTag("sp_immortal_purig");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Purig.CharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(_enemyParty.Party, NavalStorylineData.Purig.CharacterObject, -1, default(UniqueTroopDescriptor), false, false)).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam);
		Vec3 globalPosition = val.GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = val.GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val2 = obj2.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false);
		_purigAgent = Mission.Current.SpawnAgent(val2, false);
		_purigAgent.SetTeam(Team.Invalid, true);
		_purigAgent.SetAlarmState((AIStateFlag)0);
		_purigAgent.SetIsAIPaused(true);
		_purigAgent.SetMortalityState((MortalityState)2);
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (item.Team == ((MissionBehavior)this).Mission.PlayerEnemyTeam && Phase4PurigShip.GetIsAgentOnShip(item))
			{
				_purigShipAgents.Add(item);
			}
		}
		CharacterObject val3 = MBObjectManager.Instance.GetObject<CharacterObject>("sea_hounds");
		GameEntity val4 = Mission.Current.Scene.FindEntityWithTag("sp_immortal_bodyguard_1");
		AgentBuildData obj3 = new AgentBuildData((BasicCharacterObject)(object)val3).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val3, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam);
		globalPosition = val4.GlobalPosition;
		AgentBuildData obj4 = obj3.InitialPosition(ref globalPosition);
		globalFrame = val4.GetGlobalFrame();
		asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val5 = obj4.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false);
		_purigBodyguard1 = Mission.Current.SpawnAgent(val5, false);
		_purigBodyguard1.SetTeam(Team.Invalid, true);
		_purigBodyguard1.SetAlarmState((AIStateFlag)0);
		_purigBodyguard1.SetIsAIPaused(true);
		_purigBodyguard1.SetMortalityState((MortalityState)2);
		GameEntity val6 = Mission.Current.Scene.FindEntityWithTag("sp_immortal_bodyguard_2");
		AgentBuildData obj5 = new AgentBuildData((BasicCharacterObject)(object)val3).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val3, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam);
		globalPosition = val6.GlobalPosition;
		AgentBuildData obj6 = obj5.InitialPosition(ref globalPosition);
		globalFrame = val6.GetGlobalFrame();
		asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val7 = obj6.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false);
		_purigBodyguard2 = Mission.Current.SpawnAgent(val7, false);
		_purigBodyguard2.SetTeam(Team.Invalid, true);
		_purigBodyguard2.SetAlarmState((AIStateFlag)0);
		_purigBodyguard2.SetIsAIPaused(true);
		_purigBodyguard2.SetMortalityState((MortalityState)2);
	}

	private void InitializeNavalBossFightPart1()
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		Phase4PurigShip.ShipOrder.SetShipStopOrder();
		Phase4PurigShip.SetShipOrderActive(isOrderActive: false);
		Phase4PurigShip.SetAnchor(isAnchored: true);
		BossFightConversationCameraGameEntity = Mission.Current.Scene.FindEntityWithTag("sp_boss_fight_camera");
		MBObjectManager.Instance.GetObject<CharacterObject>("gangradirs_kin_melee");
		MBObjectManager.Instance.GetObject<CharacterObject>("sea_hounds");
		_duelPhaseAllyAgents = new List<Agent>();
		_duelPhaseEnemyAgents = new List<Agent>();
		_playerSpawnPointEntity = Mission.Current.Scene.FindEntityWithTag("naval_boss_fight_player_sp");
		if (Agent.Main.IsUsingGameObject)
		{
			Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			item.SetController(ShipControllerType.None, autoUpdateController: false);
		}
		Agent.Main.TeleportToPosition(_playerSpawnPointEntity.GlobalPosition);
		List<GameEntity> allyFrames = new List<GameEntity>();
		GetAllyFrames(out allyFrames);
		if (_gunnarAgent != null && _gunnarAgent.IsActive())
		{
			_gunnarAgent.ClearTargetFrame();
			GameEntity val = allyFrames.First();
			_gunnarAgent.TeleportToPosition(val.GlobalPosition);
			allyFrames.Remove(val);
			_duelPhaseAllyAgents.Add(_gunnarAgent);
		}
		if (_bjolgurAgent == null || !_bjolgurAgent.IsActive())
		{
			SpawnBjolgurOnShip(_playerShip);
		}
		if (_bjolgurAgent != null && _bjolgurAgent.IsActive())
		{
			_bjolgurAgent.ClearTargetFrame();
			GameEntity val2 = allyFrames.First();
			_bjolgurAgent.TeleportToPosition(val2.GlobalPosition);
			allyFrames.Remove(val2);
			_duelPhaseAllyAgents.Add(_bjolgurAgent);
		}
		_enemyBossSpawnPointEntity = Mission.Current.Scene.FindEntityWithTag("naval_boss_fight_enemy_boss_sp");
		_purigAgent.SetTeam(((MissionBehavior)this).Mission.PlayerEnemyTeam, true);
		_purigAgent.TeleportToPosition(_enemyBossSpawnPointEntity.GlobalPosition);
		_purigAgent.SetIsAIPaused(false);
		_purigAgent.SetMortalityState((MortalityState)0);
		_duelPhaseEnemyAgents.Add(_purigAgent);
		_navalAgentsLogic.AddAgentToShip(_purigAgent, Phase4PurigShip);
		List<GameEntity> enemyFrames = new List<GameEntity>();
		GetEnemyFrames(out enemyFrames);
		_purigBodyguard1.SetTeam(((MissionBehavior)this).Mission.PlayerEnemyTeam, true);
		_purigBodyguard1.TeleportToPosition(enemyFrames[0].GlobalPosition);
		_purigBodyguard1.SetIsAIPaused(false);
		_purigBodyguard1.SetMortalityState((MortalityState)0);
		_duelPhaseEnemyAgents.Add(_purigBodyguard1);
		_navalAgentsLogic.AddAgentToShip(_purigBodyguard1, Phase4PurigShip);
		_purigBodyguard2.SetTeam(((MissionBehavior)this).Mission.PlayerEnemyTeam, true);
		_purigBodyguard2.TeleportToPosition(enemyFrames[1].GlobalPosition);
		_purigBodyguard2.SetIsAIPaused(false);
		_purigBodyguard2.SetMortalityState((MortalityState)0);
		_duelPhaseEnemyAgents.Add(_purigBodyguard2);
		_navalAgentsLogic.AddAgentToShip(_purigBodyguard2, Phase4PurigShip);
		RemoveAllAgentsExcept(new List<Agent>
		{
			Agent.Main,
			_gunnarAgent,
			_bjolgurAgent,
			_purigAgent,
			_purigBodyguard1,
			_purigBodyguard2
		});
		foreach (ShipAttachmentMachine item2 in (List<ShipAttachmentMachine>)(object)Phase4PurigShip.AttachmentMachines)
		{
			if (item2.IsShipAttachmentMachineBridged())
			{
				item2.DisconnectAttachment();
			}
			foreach (StandingPoint item3 in (List<StandingPoint>)(object)((UsableMachine)item2).StandingPoints)
			{
				((UsableMissionObject)item3).IsDisabledForPlayers = true;
			}
		}
		foreach (ShipAttachmentPointMachine item4 in (List<ShipAttachmentPointMachine>)(object)Phase4PurigShip.AttachmentPointMachines)
		{
			if (item4.CurrentAttachment != null)
			{
				item4.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
			}
			foreach (StandingPoint item5 in (List<StandingPoint>)(object)((UsableMachine)item4).StandingPoints)
			{
				((UsableMissionObject)item5).IsDisabledForPlayers = true;
			}
		}
		Phase4PurigShip.SetCustomSailSetting(enableCustomSailSetting: true, SailInput.Raised);
		Phase4PurigShip.ShipOrder.SetShipStopOrder();
		Phase4PurigShip.SetAnchor(isAnchored: true);
		_playerShip.ShipOrder.SetShipStopOrder();
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
		ClearPhase4OnPhaseTransition();
		_navalTrajectoryPlanningLogic.ForceReinitialize();
	}

	private void InitializeNavalBossFightPart2()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent item in _duelPhaseAllyAgents.Concat(_duelPhaseEnemyAgents))
		{
			if (item != Agent.Main)
			{
				ResetAgentForBossFight(item);
			}
		}
		Vec3 position;
		if (_gunnarAgent != null && _gunnarAgent.IsActive())
		{
			Agent gunnarAgent = _gunnarAgent;
			position = _gunnarAgent.Position;
			gunnarAgent.SetTargetPosition(((Vec3)(ref position)).AsVec2);
			_gunnarAgent.SetAlarmState((AIStateFlag)0);
		}
		if (_bjolgurAgent != null && _bjolgurAgent.IsActive())
		{
			Agent bjolgurAgent = _bjolgurAgent;
			position = _bjolgurAgent.Position;
			bjolgurAgent.SetTargetPosition(((Vec3)(ref position)).AsVec2);
			_bjolgurAgent.SetAlarmState((AIStateFlag)0);
		}
		Agent.Main.SetLookAgent(_purigAgent);
		_purigAgent.SetLookAgent(Agent.Main);
		foreach (Formation item2 in (List<Formation>)(object)((MissionBehavior)this).Mission.Teams.Attacker.FormationsIncludingEmpty)
		{
			if (item2.CountOfUnits > 0)
			{
				item2.SetMovementOrder(MovementOrder.MovementOrderStop);
			}
		}
		foreach (Formation item3 in (List<Formation>)(object)((MissionBehavior)this).Mission.Teams.Defender.FormationsIncludingEmpty)
		{
			if (item3.CountOfUnits > 0)
			{
				item3.SetMovementOrder(MovementOrder.MovementOrderStop);
			}
		}
	}

	private void RemoveAllAgentsExcept(List<Agent> exceptionAgents)
	{
		for (int num = ((List<Agent>)(object)((MissionBehavior)this).Mission.Agents).Count - 1; num >= 0; num--)
		{
			Agent val = ((List<Agent>)(object)((MissionBehavior)this).Mission.Agents)[num];
			if (val.IsActive() && !exceptionAgents.Contains(val))
			{
				val.FadeOut(true, false);
			}
		}
	}

	public void StartBossFight(bool isDuel)
	{
		_instructionState = Quest5InstructionState.DefeatPurig;
		BossFightConversationCameraGameEntity = null;
		if (isDuel)
		{
			BossFightState = BossFightStateEnum.Duel;
			StartBossFightDuelModeInternal();
		}
		else
		{
			BossFightState = BossFightStateEnum.All;
			BossFightOutCome = BossFightOutComeEnum.PlayerRefusedTheDuel;
			StartBossFightBattleModeInternal();
		}
	}

	private void StartBossFightDuelModeInternal()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		ResetAgentForBossFight(_purigAgent);
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().DisableStartConversation(true);
		foreach (Agent duelPhaseAllyAgent in _duelPhaseAllyAgents)
		{
			if (!duelPhaseAllyAgent.IsMainAgent)
			{
				duelPhaseAllyAgent.SetTeam(Team.Invalid, true);
				WorldPosition worldPosition = duelPhaseAllyAgent.GetWorldPosition();
				duelPhaseAllyAgent.SetScriptedPosition(ref worldPosition, false, (AIScriptedFrameFlags)0);
				duelPhaseAllyAgent.SetLookAgent(Agent.Main);
			}
		}
		foreach (Agent duelPhaseEnemyAgent in _duelPhaseEnemyAgents)
		{
			if (duelPhaseEnemyAgent != _purigAgent)
			{
				duelPhaseEnemyAgent.SetTeam(Team.Invalid, true);
				WorldPosition worldPosition2 = duelPhaseEnemyAgent.GetWorldPosition();
				duelPhaseEnemyAgent.SetScriptedPosition(ref worldPosition2, false, (AIScriptedFrameFlags)0);
				duelPhaseEnemyAgent.SetLookAgent(_purigAgent);
				Vec3 position = duelPhaseEnemyAgent.Position;
				duelPhaseEnemyAgent.SetTargetPosition(((Vec3)(ref position)).AsVec2);
			}
		}
		_purigAgent.SetTargetAgent(Agent.Main);
		_purigAgent.Formation.AI.ResetBehaviorWeights();
		_purigAgent.HumanAIComponent.RefreshBehaviorValues((MovementOrderEnum)2, (ArrangementOrderEnum)2);
		_purigAgent.SetAlarmState((AIStateFlag)3);
		State = Quest5SetPieceBattleMissionState.BossFightInProgressAsDuel;
	}

	private void StartBossFightBattleModeInternal()
	{
		foreach (Agent item in _duelPhaseAllyAgents.Concat(_duelPhaseEnemyAgents))
		{
			if (item != Agent.Main)
			{
				ResetAgentForBossFight(item);
			}
		}
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().DisableStartConversation(true);
		_purigAgent.Formation.AI.ResetBehaviorWeights();
		_purigAgent.HumanAIComponent.RefreshBehaviorValues((MovementOrderEnum)2, (ArrangementOrderEnum)2);
		_purigAgent.SetAlarmState((AIStateFlag)3);
		((MissionBehavior)this).Mission.PlayerTeam.SetIsEnemyOf(((MissionBehavior)this).Mission.PlayerEnemyTeam, true);
		State = Quest5SetPieceBattleMissionState.BossFightInProgressAsAll;
		foreach (Agent duelPhaseEnemyAgent in _duelPhaseEnemyAgents)
		{
			duelPhaseEnemyAgent.Formation.AI.ResetBehaviorWeights();
			duelPhaseEnemyAgent.HumanAIComponent.RefreshBehaviorValues((MovementOrderEnum)2, (ArrangementOrderEnum)2);
			duelPhaseEnemyAgent.SetAlarmState((AIStateFlag)3);
		}
		foreach (Agent duelPhaseAllyAgent in _duelPhaseAllyAgents)
		{
			if (!duelPhaseAllyAgent.IsMainAgent)
			{
				duelPhaseAllyAgent.SetAlarmState((AIStateFlag)3);
			}
		}
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SetOrder((OrderType)4);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SetOrder((OrderType)4);
	}

	private void ResetAgentForBossFight(Agent agent)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (agent.IsUsingGameObject)
		{
			agent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		agent.ClearTargetFrame();
		ActionIndexCache act_none = ActionIndexCache.act_none;
		float num = -0.2f;
		agent.SetActionChannel(1, ref act_none, false, (AnimFlags)72, 0f, 1f, num, 0.4f, 0f, false, -0.2f, 0, true);
		agent.SetActionChannel(0, ref act_none, false, (AnimFlags)72, 0f, 1f, num, 0.4f, 0f, false, -0.2f, 0, true);
	}

	private void StartBossFightConversation()
	{
		_gunnarAgent.SetMortalityState((MortalityState)0);
		MissionConversationLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>();
		missionBehavior.DisableStartConversation(false);
		missionBehavior.StartConversation(_purigAgent, false, false);
	}

	private void GetAllyFrames(out List<GameEntity> allyFrames)
	{
		allyFrames = new List<GameEntity>();
		for (int i = 0; i < 2; i++)
		{
			GameEntity item = Mission.Current.Scene.FindEntityWithTag("naval_boss_fight_player_ally_sp_" + (i + 1));
			allyFrames.Add(item);
		}
	}

	private void GetEnemyFrames(out List<GameEntity> enemyFrames)
	{
		enemyFrames = new List<GameEntity>();
		for (int i = 0; i < 2; i++)
		{
			GameEntity item = Mission.Current.Scene.FindEntityWithTag("naval_boss_fight_player_enemy_sp_" + (i + 1));
			enemyFrames.Add(item);
		}
	}

	private void OnDuelOver(BattleSideEnum winnerSide)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		AgentVictoryLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<AgentVictoryLogic>();
		if (missionBehavior != null)
		{
			missionBehavior.SetCheerActionGroup((CheerActionGroupEnum)3);
		}
		if (missionBehavior != null)
		{
			missionBehavior.SetCheerReactionTimerSettings(0.25f, 3f);
		}
		_winnerSide = winnerSide;
		if (winnerSide == ((MissionBehavior)this).Mission.PlayerTeam.Side)
		{
			if (BossFightState == BossFightStateEnum.Duel)
			{
				BossFightOutCome = BossFightOutComeEnum.PlayerAcceptedAndWonTheDuel;
			}
			MapEvent.PlayerMapEvent.SetOverrideWinner(((MissionBehavior)this).Mission.PlayerTeam.Side);
		}
		else
		{
			BossFightOutCome = BossFightOutComeEnum.PlayerDefeatedWaitingForConversation;
			MapEvent.PlayerMapEvent.SetOverrideWinner(((MissionBehavior)this).Mission.PlayerEnemyTeam.Side);
		}
		LastHitCheckpoint = Quest5SetPieceBattleMissionState.End;
		State = Quest5SetPieceBattleMissionState.End;
	}

	private MissionShip CreateShip(string shipHullId, string spawnPointId, Formation formation, bool spawnAnchored = false, List<KeyValuePair<string, string>> additionalUpgradePieces = null, Figurehead figurehead = null, bool checkForFreeArea = true)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = Mission.Current.Scene.FindEntityWithTag(spawnPointId);
		MatrixFrame globalFrame = val.GetGlobalFrame();
		Scene scene = Mission.Current.Scene;
		Vec3 globalPosition = val.GlobalPosition;
		float waterLevelAtPosition = scene.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, false, false);
		globalFrame.origin = new Vec3(val.GlobalPosition.x, val.GlobalPosition.y, waterLevelAtPosition, -1f);
		Ship val2 = new Ship(((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>(shipHullId));
		if (formation.Team == ((MissionBehavior)this).Mission.PlayerEnemyTeam)
		{
			val2.Owner = _enemyParty.Party;
		}
		else if (formation.Team == ((MissionBehavior)this).Mission.PlayerTeam)
		{
			val2.Owner = PartyBase.MainParty;
		}
		if (additionalUpgradePieces != null)
		{
			foreach (KeyValuePair<string, string> additionalUpgradePiece in additionalUpgradePieces)
			{
				if (!string.IsNullOrEmpty(additionalUpgradePiece.Value))
				{
					ShipUpgradePiece val3 = MBObjectManager.Instance.GetObject<ShipUpgradePiece>(additionalUpgradePiece.Value);
					val2.EquipUpgradePiece(additionalUpgradePiece.Key, val3);
				}
			}
		}
		if (figurehead != null)
		{
			val2.ChangeFigurehead(figurehead);
		}
		MatrixFrame shipFrame = MatrixFrame.Identity;
		Vec3 globalPosition2 = val.GlobalPosition;
		Scene scene2 = ((MissionBehavior)this).Mission.Scene;
		globalPosition = val.GlobalPosition;
		globalPosition2.z = scene2.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, true, false);
		shipFrame.origin = globalPosition2;
		ref Mat3 rotation = ref shipFrame.rotation;
		Vec2 val4 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		val4 = ((Vec2)(ref val4)).Normalized();
		rotation.f = ((Vec2)(ref val4)).ToVec3(0f);
		((Mat3)(ref shipFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		MissionShip missionShip = _navalShipsLogic.SpawnShip((IShipOrigin)(object)val2, in shipFrame, formation.Team, formation, spawnAnchored, (FormationClass)8, checkForFreeArea);
		missionShip.ShipOrder.FormationJoinShip(formation);
		return missionShip;
	}

	private Formation GetAvailableAllyFormation()
	{
		Formation val = _availableAllyFormations.FirstOrDefault();
		if (val != null)
		{
			_availableAllyFormations.Remove(val);
		}
		else
		{
			MBReadOnlyList<MissionShip> allShips = _navalShipsLogic.AllShips;
			for (int num = ((List<MissionShip>)(object)allShips).Count - 1; num >= 0; num--)
			{
				MissionShip missionShip = ((List<MissionShip>)(object)allShips)[num];
				if (missionShip.Formation.Team == ((MissionBehavior)this).Mission.PlayerTeam)
				{
					MBReadOnlyList<Agent> activeAgentsOfShip = _navalAgentsLogic.GetActiveAgentsOfShip(missionShip);
					if (activeAgentsOfShip == null || Extensions.IsEmpty<Agent>((IEnumerable<Agent>)activeAgentsOfShip))
					{
						val = missionShip.Formation;
						RemoveShipInternal(missionShip);
						_navalTrajectoryPlanningLogic.ForceReinitialize();
						break;
					}
				}
			}
		}
		return val;
	}

	private void SpawnGunnarOnShip(MissionShip ship)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)Extensions.GetRandomElement<ShipOarMachine>(ship.LeftSideShipOarMachines)).GameEntity;
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, NavalStorylineData.Gunnar.CharacterObject, -1, default(UniqueTroopDescriptor), false, true)).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		AgentBuildData val2 = obj2.InitialDirection(ref val).NoHorses(true).NoWeapons(false)
			.Banner(((MissionBehavior)this).Mission.PlayerTeam.Banner);
		_gunnarAgent = Mission.Current.SpawnAgent(val2, false);
		_navalAgentsLogic.SetIgnoreTroopCapacities(value: true);
		_navalAgentsLogic.AddAgentToShip(_gunnarAgent, ship);
		_gunnarAgentNavalComponent = _gunnarAgent.GetComponent<AgentNavalComponent>();
	}

	private void TriggerShip(MissionShip ship)
	{
		ship.SetAnchor(isAnchored: false);
		ship.Formation.SetControlledByAI(true, false);
		ship.SetShipOrderActive(isOrderActive: true);
		ship.ShipOrder.SetShipEngageOrder();
	}

	private void SpawnCrusasOnShip(MissionShip ship)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)Extensions.GetRandomElement<ShipOarMachine>(ship.LeftSideShipOarMachines)).GameEntity;
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Prusas.CharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, NavalStorylineData.Prusas.CharacterObject, -1, default(UniqueTroopDescriptor), false, true)).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		AgentBuildData val2 = obj2.InitialDirection(ref val).NoHorses(true).NoWeapons(false);
		_crusasAgent = Mission.Current.SpawnAgent(val2, false);
		_navalAgentsLogic.AddAgentToShip(_crusasAgent, ship);
	}

	private void SpawnLaharOnShip(MissionShip ship)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)Extensions.GetRandomElement<ShipOarMachine>(ship.LeftSideShipOarMachines)).GameEntity;
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Lahar.CharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, NavalStorylineData.Lahar.CharacterObject, -1, default(UniqueTroopDescriptor), false, true)).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		AgentBuildData val2 = obj2.InitialDirection(ref val).NoHorses(true).NoWeapons(false);
		_laharAgent = Mission.Current.SpawnAgent(val2, false);
		_navalAgentsLogic.AddAgentToShip(_laharAgent, ship);
	}

	private void SpawnBjolgurOnShip(MissionShip ship)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)Extensions.GetRandomElement<ShipOarMachine>(ship.LeftSideShipOarMachines)).GameEntity;
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Bjolgur.CharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, NavalStorylineData.Bjolgur.CharacterObject, -1, default(UniqueTroopDescriptor), false, true)).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		AgentBuildData val2 = obj2.InitialDirection(ref val).NoHorses(true).NoWeapons(false);
		_bjolgurAgent = Mission.Current.SpawnAgent(val2, false);
		_navalAgentsLogic.AddAgentToShip(_bjolgurAgent, ship);
	}

	private void AddAvailableAllyFormation(Formation formation)
	{
		if (!_availableAllyFormations.Contains(formation))
		{
			_availableAllyFormations.Add(formation);
		}
		else
		{
			Debug.FailedAssert("Formation has been already added.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\MissionControllers\\Quest5SetPieceBattleMissionController.cs", "AddAvailableAllyFormation", 6261);
		}
	}

	private Formation GetAvailableEnemyFormation()
	{
		Formation val = _availableEnemyFormations.FirstOrDefault();
		if (val != null)
		{
			_availableEnemyFormations.Remove(val);
		}
		else
		{
			foreach (Formation item in (List<Formation>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.FormationsIncludingEmpty)
			{
				if (!_navalShipsLogic.IsAShipAssignedToFormation(item))
				{
					val = item;
					break;
				}
			}
			if (val == null)
			{
				MissionShip missionShip = null;
				int num = 0;
				MBReadOnlyList<MissionShip> allShips = _navalShipsLogic.AllShips;
				for (int num2 = ((List<MissionShip>)(object)allShips).Count - 1; num2 >= 0; num2--)
				{
					MissionShip missionShip2 = ((List<MissionShip>)(object)allShips)[num2];
					if (missionShip2.Formation.Team == ((MissionBehavior)this).Mission.PlayerEnemyTeam)
					{
						MBReadOnlyList<Agent> activeAgentsOfShip = _navalAgentsLogic.GetActiveAgentsOfShip(missionShip2);
						if (missionShip2 != _phase3EnemyReinforcementShip1 && missionShip2 != _phase3EnemyReinforcementShip2)
						{
							if (activeAgentsOfShip == null || Extensions.IsEmpty<Agent>((IEnumerable<Agent>)activeAgentsOfShip))
							{
								val = missionShip2.Formation;
								RemoveShipInternal(missionShip2);
								_navalTrajectoryPlanningLogic.ForceReinitialize();
								break;
							}
							if (missionShip == null || ((List<Agent>)(object)activeAgentsOfShip).Count < num)
							{
								missionShip = missionShip2;
								num = ((List<Agent>)(object)activeAgentsOfShip).Count;
							}
						}
					}
				}
				if (val == null && missionShip != null)
				{
					val = missionShip.Formation;
					RemoveShipInternal(missionShip);
					_navalTrajectoryPlanningLogic.ForceReinitialize();
				}
			}
		}
		return val;
	}

	private void AddAvailableEnemyFormation(Formation formation)
	{
		if (!_availableEnemyFormations.Contains(formation))
		{
			_availableEnemyFormations.Add(formation);
		}
		else
		{
			Debug.FailedAssert("Formation has been already added.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\MissionControllers\\Quest5SetPieceBattleMissionController.cs", "AddAvailableEnemyFormation", 6347);
		}
	}

	private void AdjustWindDirectionAccordingToTargetFrame(MatrixFrame frame, float windPowerMultiplier, bool addRandomRotation = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = ((Vec3)(ref frame.rotation.f)).AsVec2;
		Vec2 val2 = ((Vec2)(ref val)).Normalized();
		Scene scene = Mission.Current.Scene;
		val = val2 * windPowerMultiplier;
		scene.SetGlobalWindVelocity(ref val);
		Scene scene2 = Mission.Current.Scene;
		val = val2 * windPowerMultiplier;
		scene2.SetGlobalWindStrengthVector(ref val);
	}

	private void TriggerMissionFailPopup()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		_isMissionFailPopUpTriggered = true;
		InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=wQbfWNZO}Mission Failed!", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=xOhvBfoE}You have been caught.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), string.Empty, (Action)EndMissionWithAutoContinueFromCheckpoint, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void CheckIfMainAgentLeftTheEscapeShip()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		if (Agent.Main.IsActive())
		{
			if (EscapeShip.GetIsAgentOnShip(Agent.Main))
			{
				_playerLeftTheEscapeShipTimer = null;
			}
			else if (_playerLeftTheEscapeShipTimer == null)
			{
				DialogNotificationHandle item = CampaignInformationManager.AddDialogLine(new TextObject("{=n17xuLkd*}Get back on our ship! Don't risk getting left behind!", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject, (Equipment)null, 0, (NotificationPriority)3);
				_dialogNotificationHandleCache.Add(item);
				_playerLeftTheEscapeShipTimer = new MissionTimer(10f);
			}
			else if (!_isMissionFailPopUpTriggered && _playerLeftTheEscapeShipTimer.Check(false))
			{
				TriggerMissionFailPopup();
				_playerLeftTheEscapeShipTimer = null;
			}
		}
	}

	private void EndMissionWithAutoContinueFromCheckpoint()
	{
		ShouldMissionContinueFromCheckpoint = true;
		MakeGunnarStopUsingGameObjectBeforeMissionEnd();
		foreach (DialogNotificationHandle item in _dialogNotificationHandleCache)
		{
			CampaignInformationManager.ClearDialogNotification(item, true);
		}
		_dialogNotificationHandleCache.Clear();
		State = Quest5SetPieceBattleMissionState.End;
	}

	private void RemoveGunnarsHelmet()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (_gunnarAgent == null || !_gunnarAgent.IsActive())
		{
			return;
		}
		Equipment val = GetScriptedStealthEquipment().Clone(false);
		for (int i = 0; i < 12; i++)
		{
			if (i == 5)
			{
				val[i] = EquipmentElement.Invalid;
				break;
			}
		}
		_gunnarAgent.UpdateSpawnEquipmentAndRefreshVisuals(val);
	}

	private void AddMissionShipTroops(List<KeyValuePair<string, int>> troops, MissionShip ship, PartyBase party = null)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		_navalAgentsLogic.SetIgnoreTroopCapacities(ship, value: true);
		foreach (KeyValuePair<string, int> troop in troops)
		{
			CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>(troop.Key);
			int value = troop.Value;
			for (int i = 0; i < value; i++)
			{
				if (party != null)
				{
					_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(party, val, -1, default(UniqueTroopDescriptor), false, true), ship);
				}
				else
				{
					_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor)), ship);
				}
			}
		}
	}

	private void HealMainHero()
	{
		Hero.MainHero.Heal(Hero.MainHero.MaxHitPoints, false);
		if (Agent.Main != null && Agent.Main.IsActive())
		{
			Agent.Main.Health = Agent.Main.HealthLimit;
		}
	}

	private void RemoveShipInternal(MissionShip ship)
	{
		ship.BreakAllExistingConnections();
		Formation formation = ship.Formation;
		_navalShipsLogic.RemoveShip(ship.Formation);
		formation.AI.ResetBehaviorWeights();
	}

	private void CutLooseAllBridgesOfTheShip(MissionShip ship)
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ship.ShipAttachmentMachines)
		{
			if (item.CurrentAttachment != null)
			{
				item.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)ship.AttachmentPointMachines)
		{
			if (item2.CurrentAttachment != null)
			{
				item2.CurrentAttachment.SetAttachmentState(ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
			}
		}
	}

	private void MakeGunnarStopUsingGameObjectBeforeMissionEnd()
	{
		if (_gunnarAgent == null || !_gunnarAgent.IsActive())
		{
			return;
		}
		_gunnarAgent.Controller = (AgentControllerType)1;
		if (_gunnarAgent.IsUsingGameObject)
		{
			_gunnarAgent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
			return;
		}
		_gunnarAgent.DisableScriptedMovement();
		if (_gunnarAgent.IsAIControlled && AgentComponentExtensions.AIMoveToGameObjectIsEnabled(_gunnarAgent))
		{
			AgentComponentExtensions.AIMoveToGameObjectDisable(_gunnarAgent);
			Formation formation = _gunnarAgent.Formation;
			if (formation != null)
			{
				formation.Team.DetachmentManager.RemoveScoresOfAgentFromDetachments(_gunnarAgent);
			}
		}
	}

	private void SetLastCheckpoint(Quest5SetPieceBattleMissionState state)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		if (state == Quest5SetPieceBattleMissionState.InitializeStealthPhasePart1 || state == Quest5SetPieceBattleMissionState.InitializePhase2Part1 || state == Quest5SetPieceBattleMissionState.InitializePhase3Part1)
		{
			LastHitCheckpoint = state;
			InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=BWSp3Uyj}Checkpoint reached.", (Dictionary<string, object>)null)).ToString(), new Color(0f, 1f, 0f, 1f)));
		}
		else
		{
			Debug.FailedAssert("Unexpected checkpoint set!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\MissionControllers\\Quest5SetPieceBattleMissionController.cs", "SetLastCheckpoint", 6535);
		}
	}

	private void TriggerPurigsDeadPopUp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=dS3R9lW7}Success", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=suHWcRSn}As you cut Purig down, there is a moment of silence. Then a great cheer wells up from your men. Gunnar closes his eyes and offers a muttered prayer to his gods. Meanwhile, with your sister foremost in your mind, you hurry back to the roundship.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), string.Empty, (Action)delegate
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			LastHitCheckpoint = Quest5SetPieceBattleMissionState.End;
			MapEvent.PlayerMapEvent.SetOverrideWinner(((MissionBehavior)this).Mission.PlayerTeam.Side);
			foreach (DialogNotificationHandle item in _dialogNotificationHandleCache)
			{
				CampaignInformationManager.ClearDialogNotification(item, true);
			}
			_dialogNotificationHandleCache.Clear();
			((MissionBehavior)this).Mission.EndMission();
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void MakeShipOarsInvisible(MissionShip ship)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child;
			if (((WeakGameEntity)(ref current)).Name.Equals("oars_holder"))
			{
				((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(false);
				break;
			}
		}
	}

	private void DisableAllShipOrderControllers()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (item != _playerShip)
			{
				DisableShipOrderController(item);
			}
		}
	}

	private void DisableShipOrderController(MissionShip ship)
	{
		ship.ShipOrder.SetShipStopOrder();
		ship.SetController(ShipControllerType.None);
		ship.SetShipOrderActive(isOrderActive: false);
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (item != ship && item.ShipOrder.TargetShip == ship)
			{
				item.ShipOrder.SetShipStopOrder();
				item.ShipOrder.SetShipEngageOrder();
			}
		}
	}

	private void RemoveShipControlPointDescriptionOfAllEnemyShips()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (item.Team == ((MissionBehavior)this).Mission.PlayerEnemyTeam)
			{
				RemoveShipControlPointDescriptionOfShip(item);
			}
		}
	}

	private void RemoveShipControlPointDescriptionOfShip(MissionShip ship)
	{
		ship.ShipControllerMachine.SetOverridenDescriptionForActiveEnemyShipControllerMachine(TextObject.GetEmpty());
	}

	private bool IsThereAnyShipBoardedToThePlayerShip()
	{
		bool flag = false;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_playerShip.AttachmentMachines)
		{
			if (item.IsShipAttachmentMachineBridged())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_playerShip.AttachmentPointMachines)
			{
				if (item2.CurrentAttachment != null)
				{
					flag = true;
					break;
				}
			}
		}
		return flag;
	}

	private bool IsThereAnyEnemyShipsWithinRange(MissionShip missionShip, float range)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (item.Team != missionShip.Team && item.Team != Team.Invalid && _navalAgentsLogic.GetActiveAgentCountOfShip(item) > 0)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				gameEntity = ((ScriptComponentBehavior)missionShip).GameEntity;
				if (((Vec3)(ref globalPosition)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) <= range)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public void StartSpawner(BattleSideEnum side)
	{
	}

	public void StopSpawner(BattleSideEnum side)
	{
	}

	public bool IsSideSpawnEnabled(BattleSideEnum side)
	{
		return true;
	}

	public float GetReinforcementInterval()
	{
		return 0f;
	}

	public bool IsSideDepleted(BattleSideEnum side)
	{
		return false;
	}

	public IEnumerable<IAgentOriginBase> GetAllTroopsForSide(BattleSideEnum side)
	{
		return new List<IAgentOriginBase>();
	}

	public bool GetSpawnHorses(BattleSideEnum side)
	{
		return true;
	}

	public int GetNumberOfPlayerControllableTroops()
	{
		return ((List<Agent>)(object)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents).Count - 1;
	}
}
