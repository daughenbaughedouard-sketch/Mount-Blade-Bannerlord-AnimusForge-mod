using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SandBox.Missions.MissionLogics;
using TaleWorlds.MountAndBlade;

namespace AnimusForge;

internal static class MissionFightHandlerCompat
{
	internal static readonly FieldInfo PlayerSideOldTeamDataField = AccessTools.Field(typeof(MissionFightHandler), "_playerSideAgentsOldTeamData");

	internal static readonly FieldInfo OpponentSideOldTeamDataField = AccessTools.Field(typeof(MissionFightHandler), "_opponentSideAgentsOldTeamData");

	internal static readonly FieldInfo OpponentSideAgentsField = AccessTools.Field(typeof(MissionFightHandler), "_opponentSideAgents");

	internal static readonly FieldInfo FinishTimerField = AccessTools.Field(typeof(MissionFightHandler), "_finishTimer");

	internal static readonly MethodInfo OnMissionTickMethod = AccessTools.Method(typeof(MissionFightHandler), "OnMissionTick");

	internal static MissionFightHandler GetMissionBehaviorSafe(Mission mission)
	{
		try
		{
			return mission?.GetMissionBehavior<MissionFightHandler>();
		}
		catch
		{
			return null;
		}
	}

	internal static bool TryClearFinishTimer(MissionFightHandler fightHandler)
	{
		if (fightHandler == null || FinishTimerField == null)
		{
			return false;
		}
		try
		{
			FinishTimerField.SetValue(fightHandler, null);
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static BasicMissionTimer GetFinishTimer(MissionFightHandler fightHandler)
	{
		if (fightHandler == null || FinishTimerField == null)
		{
			return null;
		}
		try
		{
			return FinishTimerField.GetValue(fightHandler) as BasicMissionTimer;
		}
		catch
		{
			return null;
		}
	}

	internal static List<Agent> GetOpponentSideAgents(MissionFightHandler fightHandler)
	{
		if (fightHandler == null || OpponentSideAgentsField == null)
		{
			return null;
		}
		try
		{
			return OpponentSideAgentsField.GetValue(fightHandler) as List<Agent>;
		}
		catch
		{
			return null;
		}
	}

	internal static Dictionary<Agent, Team> GetSideOldTeamData(MissionFightHandler fightHandler, bool isPlayerSide)
	{
		if (fightHandler == null)
		{
			return null;
		}
		FieldInfo fieldInfo = isPlayerSide ? PlayerSideOldTeamDataField : OpponentSideOldTeamDataField;
		if (fieldInfo == null)
		{
			return null;
		}
		try
		{
			return fieldInfo.GetValue(fightHandler) as Dictionary<Agent, Team>;
		}
		catch
		{
			return null;
		}
	}

	internal static bool TryRememberOriginalTeam(MissionFightHandler fightHandler, Agent agent, Team originalTeam, bool isPlayerSide)
	{
		Dictionary<Agent, Team> sideOldTeamData = GetSideOldTeamData(fightHandler, isPlayerSide);
		if (sideOldTeamData == null || agent == null || originalTeam == null)
		{
			return false;
		}
		try
		{
			sideOldTeamData[agent] = originalTeam;
			return true;
		}
		catch
		{
			return false;
		}
	}
}
