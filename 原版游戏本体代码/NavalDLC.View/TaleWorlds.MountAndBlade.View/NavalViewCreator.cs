using System;
using NavalDLC.View.MissionViews;
using NavalDLC.View.MissionViews.Storyline;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace TaleWorlds.MountAndBlade.View;

public static class NavalViewCreator
{
	public static MissionView CreateNavalOrderUIHandler(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<NavalMissionOrderUIHandler>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateNavalOrderOfBattleView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<NavalOrderOfBattleView>(false, mission, new object[1] { mission });
	}

	public static MissionView CreateNavalShipMarkerUIHandler(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<NavalMissionShipMarkerUIHandler>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateNavalShipTargetSelectionHandler(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<NavalShipTargetSelectionHandler>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateMissionShipControlView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<MissionShipControlView>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateNavalMissionCaptureShipView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<NavalMissionCaptureShipView>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateQuest5SetPieceBattleMissionView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<Quest5SetPieceBattleMissionView>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateQuest5SetPieceBattleBossFightCameraView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<Quest5SetPieceBattleBossFightCameraView>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateQuest5SetPieceBattleInteriorConversationCameraView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<Quest5SetPieceBattleInteriorConversationCameraView>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateCaptivityMissionView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<NavalCaptivityBattleMissionView>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateFloatingFortressView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<FloatingFortressView>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreatePirateBattleMissionView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<NavalStorylinePirateBattleMissionView>(false, mission, Array.Empty<object>());
	}

	public static MissionView CreateHelpingAnAllyMissionView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<HelpingAnAllyMissionView>(false, mission, Array.Empty<object>());
	}
}
