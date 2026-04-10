using System.Collections.Generic;
using System.Diagnostics;
using SandBox.Objects;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.AI;

public class PassageAI : UsableMachineAIBase
{
	public PassageAI(UsableMachine usableMachine)
		: base(usableMachine)
	{
	}

	protected override AIScriptedFrameFlags GetScriptedFrameFlags(Agent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)agent.CurrentWatchState == 2)
		{
			return (AIScriptedFrameFlags)10;
		}
		return (AIScriptedFrameFlags)18;
	}

	protected override void OnTick(Agent agentToCompareTo, Formation formationToCompareTo, Team potentialUsersTeam, float dt)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		foreach (PassageUsePoint item in (List<StandingPoint>)(object)base.UsableMachine.StandingPoints)
		{
			if ((agentToCompareTo == null || ((UsableMissionObject)item).UserAgent == agentToCompareTo) && (formationToCompareTo == null || (((UsableMissionObject)item).UserAgent != null && ((UsableMissionObject)item).UserAgent.IsAIControlled && ((UsableMissionObject)item).UserAgent.Formation == formationToCompareTo)))
			{
				Debug.FailedAssert("isAgentManagedByThisMachineAI(standingPoint.UserAgent)", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\AI\\PassageAI.cs", "OnTick", 41);
				Agent userAgent = ((UsableMissionObject)item).UserAgent;
				if (((UsableMachineAIBase)this).HasActionCompleted || (potentialUsersTeam != null && base.UsableMachine.IsDisabledForBattleSideAI(potentialUsersTeam.Side)) || userAgent.IsRunningAway)
				{
					((UsableMachineAIBase)this).HandleAgentStopUsingStandingPoint(userAgent, (StandingPoint)(object)item);
				}
			}
			for (int num = ((List<Agent>)(object)item.MovingAgents).Count - 1; num >= 0; num--)
			{
				Agent val = ((List<Agent>)(object)item.MovingAgents)[num];
				if ((agentToCompareTo == null || val == agentToCompareTo) && (formationToCompareTo == null || (val != null && val.IsAIControlled && val.Formation == formationToCompareTo)))
				{
					if (((UsableMachineAIBase)this).HasActionCompleted || (potentialUsersTeam != null && base.UsableMachine.IsDisabledForBattleSideAI(potentialUsersTeam.Side)) || val.IsRunningAway)
					{
						Debug.FailedAssert("HasActionCompleted || (potentialUsersTeam != null && UsableMachine.IsDisabledForBattleSideAI(potentialUsersTeam.Side)) || agent.IsRunningAway", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\AI\\PassageAI.cs", "OnTick", 69);
						((UsableMachineAIBase)this).HandleAgentStopUsingStandingPoint(val, (StandingPoint)(object)item);
					}
					else if (!((MissionObject)item).IsDisabled && !val.IsPaused)
					{
						WorldFrame userFrameForAgent = ((UsableMissionObject)item).GetUserFrameForAgent(val);
						Vec3 groundVec = ((WorldPosition)(ref userFrameForAgent.Origin)).GetGroundVec3();
						if (val.CanReachAndUseObject((UsableMissionObject)(object)item, ((Vec3)(ref groundVec)).DistanceSquared(val.Position)))
						{
							val.UseGameObject((UsableMissionObject)(object)item, -1);
							val.SetScriptedFlags((AIScriptedFrameFlags)(val.GetScriptedFlags() & ~((StandingPoint)item).DisableScriptedFrameFlags));
						}
					}
				}
			}
		}
	}

	[Conditional("DEBUG")]
	private void TickForDebug()
	{
		if (!Input.DebugInput.IsHotKeyDown("UsableMachineAiBaseHotkeyShowMachineUsers"))
		{
			return;
		}
		foreach (PassageUsePoint item in (List<StandingPoint>)(object)base.UsableMachine.StandingPoints)
		{
			foreach (Agent item2 in (List<Agent>)(object)item.MovingAgents)
			{
				_ = item2;
			}
			_ = ((UsableMissionObject)item).UserAgent;
		}
	}
}
