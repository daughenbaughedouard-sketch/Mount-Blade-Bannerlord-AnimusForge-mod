using System.Collections.Generic;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;

[ViewCreatorModule]
public class MultiplayerMissionViews
{
	[ViewMethod("MultiplayerTeamDeathmatch")]
	public static MissionView[] OpenTeamDeathmatchMission(Mission mission)
	{
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		return new List<MissionView>
		{
			MultiplayerViewCreator.CreateMissionServerStatusUIHandler(),
			MultiplayerViewCreator.CreateMissionMultiplayerPreloadView(mission),
			MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler(),
			MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			MultiplayerViewCreator.CreateMissionMultiplayerEscapeMenu("TeamDeathmatch"),
			MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, isSingleTeam: false),
			MultiplayerViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
			MultiplayerViewCreator.CreateLobbyEquipmentUIHandler(),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			MultiplayerViewCreator.CreatePollProgressUIHandler(),
			MultiplayerViewCreator.CreateMissionFlagMarkerUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			MultiplayerViewCreator.CreateMultiplayerAdminPanelUIHandler(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView()
		}.ToArray();
	}

	[ViewMethod("MultiplayerDuel")]
	public static MissionView[] OpenDuelMission(Mission mission)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		return new List<MissionView>
		{
			MultiplayerViewCreator.CreateMissionServerStatusUIHandler(),
			MultiplayerViewCreator.CreateMissionMultiplayerPreloadView(mission),
			MultiplayerViewCreator.CreateMultiplayerCultureSelectUIHandler(),
			MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			MultiplayerViewCreator.CreateMissionMultiplayerEscapeMenu("Duel"),
			MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
			MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, isSingleTeam: true),
			MultiplayerViewCreator.CreateLobbyEquipmentUIHandler(),
			MultiplayerViewCreator.CreateMissionMultiplayerDuelUI(),
			MultiplayerViewCreator.CreatePollProgressUIHandler(),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			MultiplayerViewCreator.CreateMultiplayerAdminPanelUIHandler(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView()
		}.ToArray();
	}

	[ViewMethod("MultiplayerSiege")]
	public static MissionView[] OpenSiegeMission(Mission mission)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		return new List<MissionView>
		{
			MultiplayerViewCreator.CreateMissionServerStatusUIHandler(),
			MultiplayerViewCreator.CreateMissionMultiplayerPreloadView(mission),
			MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			MultiplayerViewCreator.CreateMissionMultiplayerEscapeMenu("Siege"),
			MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler(),
			MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, isSingleTeam: false),
			MultiplayerViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
			MultiplayerViewCreator.CreateLobbyEquipmentUIHandler(),
			MultiplayerViewCreator.CreatePollProgressUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			MultiplayerViewCreator.CreateMissionFlagMarkerUIHandler(),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			MultiplayerViewCreator.CreateMultiplayerAdminPanelUIHandler(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView()
		}.ToArray();
	}

	[ViewMethod("MultiplayerBattle")]
	public static MissionView[] OpenBattle(Mission mission)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		return new List<MissionView>
		{
			MultiplayerViewCreator.CreateLobbyEquipmentUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			MultiplayerViewCreator.CreateMissionMultiplayerPreloadView(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			MultiplayerViewCreator.CreateMissionMultiplayerEscapeMenu("Battle"),
			MultiplayerViewCreator.CreateMultiplayerMissionOrderUIHandler(mission),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateOrderTroopPlacerView((OrderController)null),
			MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler(),
			MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, isSingleTeam: false),
			MultiplayerViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
			MultiplayerViewCreator.CreatePollProgressUIHandler(),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
			MultiplayerViewCreator.CreateMissionFlagMarkerUIHandler(),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			MultiplayerViewCreator.CreateMultiplayerAdminPanelUIHandler(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)new SpectatorCameraView()
		}.ToArray();
	}

	[ViewMethod("MultiplayerCaptain")]
	public static MissionView[] OpenCaptain(Mission mission)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		return new List<MissionView>
		{
			MultiplayerViewCreator.CreateLobbyEquipmentUIHandler(),
			MultiplayerViewCreator.CreateMissionServerStatusUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
			MultiplayerViewCreator.CreateMissionMultiplayerPreloadView(mission),
			MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			MultiplayerViewCreator.CreateMissionMultiplayerEscapeMenu("Captain"),
			MultiplayerViewCreator.CreateMultiplayerMissionOrderUIHandler(mission),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateOrderTroopPlacerView((OrderController)null),
			MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler(),
			MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, isSingleTeam: false),
			MultiplayerViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
			MultiplayerViewCreator.CreatePollProgressUIHandler(),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			MultiplayerViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
			MultiplayerViewCreator.CreateMissionFlagMarkerUIHandler(),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			MultiplayerViewCreator.CreateMultiplayerAdminPanelUIHandler(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)new SpectatorCameraView()
		}.ToArray();
	}

	[ViewMethod("MultiplayerSkirmish")]
	public static MissionView[] OpenSkirmish(Mission mission)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		return new List<MissionView>
		{
			MultiplayerViewCreator.CreateLobbyEquipmentUIHandler(),
			MultiplayerViewCreator.CreateMissionServerStatusUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
			MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
			ViewCreator.CreateMissionAgentStatusUIHandler(mission),
			MultiplayerViewCreator.CreateMissionMultiplayerPreloadView(mission),
			ViewCreator.CreateMissionMainAgentEquipmentController(mission),
			ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
			MultiplayerViewCreator.CreateMissionMultiplayerEscapeMenu("Skirmish"),
			MultiplayerViewCreator.CreateMultiplayerMissionOrderUIHandler(mission),
			ViewCreator.CreateMissionAgentLabelUIHandler(mission),
			ViewCreator.CreateOrderTroopPlacerView((OrderController)null),
			MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler(),
			MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, isSingleTeam: false),
			MultiplayerViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
			MultiplayerViewCreator.CreatePollProgressUIHandler(),
			(MissionView)new MissionItemContourControllerView(),
			(MissionView)new MissionAgentContourControllerView(),
			MultiplayerViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
			MultiplayerViewCreator.CreateMultiplayerMissionVoiceChatUIHandler(),
			MultiplayerViewCreator.CreateMissionFlagMarkerUIHandler(),
			ViewCreator.CreateOptionsUIHandler(),
			ViewCreator.CreateMissionMainAgentEquipDropView(mission),
			MultiplayerViewCreator.CreateMultiplayerAdminPanelUIHandler(),
			ViewCreator.CreateMissionBoundaryCrossingView(),
			(MissionView)new MissionBoundaryWallView(),
			(MissionView)new SpectatorCameraView()
		}.ToArray();
	}
}
