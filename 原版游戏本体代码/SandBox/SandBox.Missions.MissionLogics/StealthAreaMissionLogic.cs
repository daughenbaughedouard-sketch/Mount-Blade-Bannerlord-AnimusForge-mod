using System.Collections.Generic;
using System.Linq;
using SandBox.Objects.AreaMarkers;
using SandBox.Objects.Usables;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Missions.MissionLogics;

public class StealthAreaMissionLogic : MissionLogic
{
	public delegate MBList<Agent> SpawnReinforcementAllyTroopsDelegate(StealthAreaData triggeredStealthAreaData, StealthAreaMarker stealthAreaMarker);

	public class StealthAreaData
	{
		internal bool IsStealthAreaTriggered;

		internal bool IsReinforcementCalled;

		internal readonly StealthAreaUsePoint StealthAreaUsePoint;

		internal readonly Dictionary<StealthAreaMarker, List<Agent>> StealthAreaMarkers;

		internal StealthAreaData(StealthAreaUsePoint stealthAreaUsePoint)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			StealthAreaUsePoint = stealthAreaUsePoint;
			StealthAreaMarkers = new Dictionary<StealthAreaMarker, List<Agent>>();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)stealthAreaUsePoint).GameEntity;
			foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
			{
				WeakGameEntity current = child;
				if (((WeakGameEntity)(ref current)).HasScriptOfType<StealthAreaMarker>())
				{
					StealthAreaMarkers.Add(((WeakGameEntity)(ref current)).GetFirstScriptOfType<StealthAreaMarker>(), new List<Agent>());
				}
			}
		}

		internal void AddAgentToStealthAreaMarker(StealthAreaMarker stealthAreaMarker, Agent agent)
		{
			StealthAreaMarkers[stealthAreaMarker].Add(agent);
		}

		internal void RemoveAgentFromStealthAreaMarker(StealthAreaMarker stealthAreaMarker, Agent agent)
		{
			StealthAreaMarkers[stealthAreaMarker].Remove(agent);
			if (StealthAreaMarkers.All((KeyValuePair<StealthAreaMarker, List<Agent>> x) => Extensions.IsEmpty<Agent>((IEnumerable<Agent>)x.Value)))
			{
				StealthAreaUsePoint.EnableStealthAreaUsePoint();
				IsStealthAreaTriggered = true;
			}
		}
	}

	private readonly MBList<StealthAreaData> _stealthAreaData = new MBList<StealthAreaData>();

	private readonly Dictionary<string, Dictionary<string, int>> _agentSpawnTypes = new Dictionary<string, Dictionary<string, int>>();

	private readonly MBList<Agent> _allyTroops = new MBList<Agent>();

	public SpawnReinforcementAllyTroopsDelegate SpawnReinforcementAllyTroopsEvent;

	public MBReadOnlyList<Agent> AllyTroops => (MBReadOnlyList<Agent>)(object)_allyTroops;

	public bool AllReinforcementsCalled { get; private set; }

	public bool IsSentry(Agent agent)
	{
		foreach (StealthAreaData item in (List<StealthAreaData>)(object)_stealthAreaData)
		{
			foreach (KeyValuePair<StealthAreaMarker, List<Agent>> stealthAreaMarker in item.StealthAreaMarkers)
			{
				if (stealthAreaMarker.Value.Contains(agent))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		foreach (StealthAreaUsePoint item in MBExtensions.FindAllWithType<StealthAreaUsePoint>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.MissionObjects))
		{
			((List<StealthAreaData>)(object)_stealthAreaData).Add(new StealthAreaData(item));
		}
	}

	private MBList<Agent> SpawnReinforcementAllyGroupTroops(StealthAreaData triggeredStealthAreaData, StealthAreaMarker stealthAreaMarker)
	{
		return SpawnReinforcementAllyTroopsEvent?.Invoke(triggeredStealthAreaData, stealthAreaMarker) ?? new MBList<Agent>();
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		((MissionBehavior)this).OnAgentBuild(agent, banner);
		CheckStealthAreaMarkerForAgent(agent);
	}

	public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
	{
		((MissionBehavior)this).OnAgentTeamChanged(prevTeam, newTeam, agent);
		CheckStealthAreaMarkerForAgent(agent);
	}

	private void CheckStealthAreaMarkerForAgent(Agent agent)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!agent.IsHuman || agent.Team != Mission.Current.PlayerEnemyTeam)
		{
			return;
		}
		foreach (StealthAreaData item in (List<StealthAreaData>)(object)_stealthAreaData)
		{
			foreach (KeyValuePair<StealthAreaMarker, List<Agent>> stealthAreaMarker in item.StealthAreaMarkers)
			{
				if (((AreaMarker)stealthAreaMarker.Key).IsPositionInRange(agent.Position))
				{
					item.AddAgentToStealthAreaMarker(stealthAreaMarker.Key, agent);
					break;
				}
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (affectorAgent == null || !affectorAgent.IsMainAgent)
		{
			return;
		}
		foreach (StealthAreaData item in (List<StealthAreaData>)(object)_stealthAreaData)
		{
			foreach (KeyValuePair<StealthAreaMarker, List<Agent>> stealthAreaMarker in item.StealthAreaMarkers)
			{
				if (stealthAreaMarker.Value.Contains(affectedAgent))
				{
					item.RemoveAgentFromStealthAreaMarker(stealthAreaMarker.Key, affectedAgent);
				}
			}
		}
	}

	public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		if (usedObject is StealthAreaUsePoint)
		{
			if (IsInCombat())
			{
				return;
			}
			StealthAreaData stealthAreaData = null;
			foreach (StealthAreaData item in (List<StealthAreaData>)(object)_stealthAreaData)
			{
				if ((object)item.StealthAreaUsePoint == usedObject)
				{
					stealthAreaData = item;
					break;
				}
			}
			if (stealthAreaData != null)
			{
				stealthAreaData.IsReinforcementCalled = true;
				foreach (KeyValuePair<StealthAreaMarker, List<Agent>> stealthAreaMarker in stealthAreaData.StealthAreaMarkers)
				{
					MBList<Agent> collection = SpawnReinforcementAllyGroupTroops(stealthAreaData, stealthAreaMarker.Key);
					((List<Agent>)(object)_allyTroops).AddRange((IEnumerable<Agent>)collection);
				}
			}
		}
		AllReinforcementsCalled = ((IEnumerable<StealthAreaData>)_stealthAreaData).All((StealthAreaData x) => x.IsReinforcementCalled);
	}

	private bool IsInCombat()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		foreach (Agent item in (List<Agent>)(object)Mission.Current.AllAgents)
		{
			if (item.IsActive())
			{
				AIStateFlag val = (AIStateFlag)3;
				if ((AIStateFlag)(item.AIStateFlags & val) == val)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public bool CheckIfAllStealthAreasAreTriggered()
	{
		return ((IEnumerable<StealthAreaData>)_stealthAreaData).All((StealthAreaData x) => x.IsStealthAreaTriggered);
	}

	public bool CheckIfAllStealthAreasReinforcementsAreCalled()
	{
		return AllReinforcementsCalled;
	}
}
