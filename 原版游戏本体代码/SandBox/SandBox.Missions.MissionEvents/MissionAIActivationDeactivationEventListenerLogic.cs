using System;
using System.Collections.Generic;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Missions.MissionEvents;

public class MissionAIActivationDeactivationEventListenerLogic : MissionLogic
{
	public const string ActivationEventId = "activate_agent_ai";

	public const string DeactivationEventId = "deactivate_agent_ai";

	public MissionAIActivationDeactivationEventListenerLogic()
	{
		Game.Current.EventManager.RegisterEvent<GenericMissionEvent>((Action<GenericMissionEvent>)OnGenericMissionEventTriggered);
	}

	protected override void OnEndMission()
	{
		Game.Current.EventManager.UnregisterEvent<GenericMissionEvent>((Action<GenericMissionEvent>)OnGenericMissionEventTriggered);
	}

	private void OnGenericMissionEventTriggered(GenericMissionEvent missionEvent)
	{
		if (missionEvent.EventId == "activate_agent_ai")
		{
			string[] array = missionEvent.Parameter.Split(new char[1] { ' ' });
			SandBoxHelpers.MissionHelper.DisableGenericMissionEventScript(array[0], missionEvent);
			string[] activationTags = new string[array.Length - 1];
			Array.Copy(array, 1, activationTags, 0, activationTags.Length);
			{
				foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
				{
					if (item.AgentVisuals.IsValid() && LinQuick.AnyQ<string>(item.AgentVisuals.GetEntity().Tags, (Func<string, bool>)((string x) => LinQuick.ContainsQ<string>(activationTags, x))))
					{
						CheckRemoveScriptedBehaviorFromAgent(item);
					}
				}
				return;
			}
		}
		if (!(missionEvent.EventId == "deactivate_agent_ai"))
		{
			return;
		}
		string[] array2 = missionEvent.Parameter.Split(new char[1] { ' ' });
		SandBoxHelpers.MissionHelper.DisableGenericMissionEventScript(array2[0], missionEvent);
		string[] deactivationTags = new string[array2.Length - 1];
		Array.Copy(array2, 1, deactivationTags, 0, deactivationTags.Length);
		foreach (Agent item2 in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (item2.AgentVisuals.IsValid() && LinQuick.AnyQ<string>(item2.AgentVisuals.GetEntity().Tags, (Func<string, bool>)((string x) => LinQuick.ContainsQ<string>(deactivationTags, x))))
			{
				CheckAddScriptedBehaviorToAgent(item2);
			}
		}
	}

	private void CheckRemoveScriptedBehaviorFromAgent(Agent agent)
	{
		DailyBehaviorGroup behaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		if (behaviorGroup.HasBehavior<IdleAgentBehavior>())
		{
			behaviorGroup.RemoveBehavior<IdleAgentBehavior>();
		}
	}

	private void CheckAddScriptedBehaviorToAgent(Agent agent)
	{
		DailyBehaviorGroup behaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		if (!behaviorGroup.HasBehavior<IdleAgentBehavior>())
		{
			behaviorGroup.AddBehavior<IdleAgentBehavior>();
		}
		behaviorGroup.SetScriptedBehavior<IdleAgentBehavior>();
	}
}
