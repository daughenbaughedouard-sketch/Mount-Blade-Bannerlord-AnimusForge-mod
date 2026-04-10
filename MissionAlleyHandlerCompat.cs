using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace AnimusForge;

internal static class MissionAlleyHandlerCompat
{
	internal static readonly Type MissionAlleyHandlerType = AccessTools.TypeByName("SandBox.Missions.MissionLogics.MissionAlleyHandler");

	internal static readonly Type MissionConversationLogicType = AccessTools.TypeByName("SandBox.Conversation.MissionLogics.MissionConversationLogic");

	internal static readonly MethodInfo MissionConversationStartConversationMethod = AccessTools.Method(MissionConversationLogicType, "StartConversation", new Type[3]
	{
		typeof(Agent),
		typeof(bool),
		typeof(bool)
	});

	internal static readonly MethodInfo CheckAndTriggerConversationWithRivalThugMethod = AccessTools.Method(MissionAlleyHandlerType, "CheckAndTriggerConversationWithRivalThug");

	internal static readonly MethodInfo StartCommonAreaBattleMethod = AccessTools.Method(MissionAlleyHandlerType, "StartCommonAreaBattle");

	private static readonly MethodInfo MissionGetMissionBehaviorGenericMethod = typeof(Mission).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((MethodInfo m) => m.Name == "GetMissionBehavior" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);

	internal static object GetMissionAlleyHandler(Mission mission)
	{
		if (mission == null || MissionAlleyHandlerType == null || MissionGetMissionBehaviorGenericMethod == null)
		{
			return null;
		}
		try
		{
			return MissionGetMissionBehaviorGenericMethod.MakeGenericMethod(MissionAlleyHandlerType).Invoke(mission, null);
		}
		catch
		{
			return null;
		}
	}

	internal static bool TryStartCommonAreaBattle(Mission mission, object alley, out string failureReason)
	{
		failureReason = "";
		if (mission == null)
		{
			failureReason = "Mission is unavailable.";
			return false;
		}
		if (alley == null)
		{
			failureReason = "Alley context is unavailable.";
			return false;
		}
		if (StartCommonAreaBattleMethod == null)
		{
			failureReason = "StartCommonAreaBattle method is unavailable.";
			return false;
		}
		object missionAlleyHandler = GetMissionAlleyHandler(mission);
		if (missionAlleyHandler == null)
		{
			failureReason = "MissionAlleyHandler was not found.";
			return false;
		}
		try
		{
			StartCommonAreaBattleMethod.Invoke(missionAlleyHandler, new object[1] { alley });
			return true;
		}
		catch (Exception ex)
		{
			failureReason = ex.GetType().Name + ": " + ex.Message;
			return false;
		}
	}
}
