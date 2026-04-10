using System;
using System.Collections.Generic;
using SandBox.CampaignBehaviors;
using SandBox.Missions.AgentBehaviors;
using SandBox.Objects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;

namespace SandBox.Missions.MissionLogics;

public class StealthPatrolPointMissionLogic : MissionLogic, IMissionAgentSpawnLogic, IMissionBehavior
{
	private const string CoverCowId = "cover_cow";

	private readonly Dictionary<Agent, GameEntity> _spawnedEnemyAgentsOnPatrolPoints;

	private readonly Dictionary<PatrolPoint, Agent> _coverAnimalPatrolPoints;

	private CheckpointMissionLogic _checkpointMissionLogic;

	public StealthPatrolPointMissionLogic()
	{
		_spawnedEnemyAgentsOnPatrolPoints = new Dictionary<Agent, GameEntity>();
		_coverAnimalPatrolPoints = new Dictionary<PatrolPoint, Agent>();
		Game.Current.EventManager.RegisterEvent<CheckpointLoadedMissionEvent>((Action<CheckpointLoadedMissionEvent>)OnCheckpointLoadedEvent);
		Game.Current.EventManager.RegisterEvent<LocationCharacterAgentSpawnedMissionEvent>((Action<LocationCharacterAgentSpawnedMissionEvent>)OnLocationCharacterAgentSpawned);
	}

	protected override void OnEndMission()
	{
		Game.Current.EventManager.UnregisterEvent<CheckpointLoadedMissionEvent>((Action<CheckpointLoadedMissionEvent>)OnCheckpointLoadedEvent);
		Game.Current.EventManager.UnregisterEvent<LocationCharacterAgentSpawnedMissionEvent>((Action<LocationCharacterAgentSpawnedMissionEvent>)OnLocationCharacterAgentSpawned);
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_checkpointMissionLogic = Mission.Current.GetMissionBehavior<CheckpointMissionLogic>();
		List<GameEntity> dynamicPatrolAreas = new List<GameEntity>();
		((MissionBehavior)this).Mission.Scene.GetAllEntitiesWithScriptComponent<DynamicPatrolAreaParent>(ref dynamicPatrolAreas);
		SpawnCoverAnimals(dynamicPatrolAreas);
	}

	public void OnLocationCharacterAgentSpawned(LocationCharacterAgentSpawnedMissionEvent locationCharacterAgentSpawnedEvent)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		if (Campaign.Current.GetCampaignBehavior<StealthCharactersCampaignBehavior>() == null)
		{
			return;
		}
		LocationCharacter locationCharacter = locationCharacterAgentSpawnedEvent.LocationCharacter;
		Agent agent = locationCharacterAgentSpawnedEvent.Agent;
		GameEntity val = GameEntity.CreateFromWeakEntity(locationCharacterAgentSpawnedEvent.SpawnedOnGameEntity);
		if (!(locationCharacter.SpecialTargetTag == "stealth_agent") && !(locationCharacter.SpecialTargetTag == "stealth_agent_forced") && !(locationCharacter.SpecialTargetTag == "disguise_default_agent") && !(locationCharacter.SpecialTargetTag == "disguise_officer_agent") && !(locationCharacter.SpecialTargetTag == "disguise_shadow_agent") && !(locationCharacter.SpecialTargetTag == "prison_break_reinforcement_point"))
		{
			return;
		}
		string[] tags = val.GetChild(0).Tags;
		foreach (string text in tags)
		{
			if (!string.IsNullOrEmpty(text))
			{
				agent.AgentVisuals.GetEntity().AddTag(text);
			}
		}
		agent.SetAgentFlags((AgentFlag)(agent.GetAgentFlags() | 0x10000));
		agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>().GetBehavior<PatrolAgentBehavior>().SetDynamicPatrolArea(val.Parent);
		_spawnedEnemyAgentsOnPatrolPoints.Add(agent, val);
		_checkpointMissionLogic?.RegisterAgent(agent);
	}

	public override void OnAgentInteraction(Agent userAgent, Agent agent, sbyte agentBoneIndex)
	{
		((MissionBehavior)this).OnAgentInteraction(userAgent, agent, agentBoneIndex);
		if (userAgent != Agent.Main)
		{
			return;
		}
		foreach (KeyValuePair<PatrolPoint, Agent> coverAnimalPatrolPoint in _coverAnimalPatrolPoints)
		{
			if (coverAnimalPatrolPoint.Value == agent)
			{
				agent.GetComponent<CoverAnimalAgentComponent>().StartMovement();
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (affectorAgent != null && affectorAgent.IsMainAgent)
		{
			_spawnedEnemyAgentsOnPatrolPoints.Remove(affectedAgent);
		}
	}

	public override bool IsThereAgentAction(Agent userAgent, Agent otherAgent)
	{
		if (userAgent == Agent.Main)
		{
			foreach (KeyValuePair<PatrolPoint, Agent> coverAnimalPatrolPoint in _coverAnimalPatrolPoints)
			{
				if (coverAnimalPatrolPoint.Value == otherAgent && !otherAgent.GetComponent<CoverAnimalAgentComponent>().IsMovementStarted)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SpawnCoverAnimals(IEnumerable<GameEntity> dynamicPatrolAreas)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		ItemRosterElement val2 = default(ItemRosterElement);
		foreach (GameEntity dynamicPatrolArea in dynamicPatrolAreas)
		{
			if (((MissionObject)dynamicPatrolArea.GetFirstScriptOfType<DynamicPatrolAreaParent>()).IsDisabled)
			{
				continue;
			}
			foreach (GameEntity child in dynamicPatrolArea.GetChildren())
			{
				PatrolPoint firstScriptOfType = child.GetChild(0).GetFirstScriptOfType<PatrolPoint>();
				if (firstScriptOfType != null && !((MissionObject)firstScriptOfType).IsDisabled && !string.IsNullOrEmpty(firstScriptOfType.SpawnGroupTag) && firstScriptOfType.SpawnGroupTag == "cover_cow")
				{
					ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>(firstScriptOfType.SpawnGroupTag);
					if (val == null)
					{
						break;
					}
					if (!_coverAnimalPatrolPoints.ContainsKey(firstScriptOfType))
					{
						_coverAnimalPatrolPoints.Add(firstScriptOfType, null);
					}
					MatrixFrame globalFrame = child.GetGlobalFrame();
					((ItemRosterElement)(ref val2))._002Ector(val, 0, (ItemModifier)null);
					((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
					Mission current3 = Mission.Current;
					ItemRosterElement val3 = val2;
					ref Vec3 origin = ref globalFrame.origin;
					Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
					Agent val4 = current3.SpawnMonster(val3, default(ItemRosterElement), ref origin, ref asVec, -1);
					val4.SetAgentExcludeStateForFaceGroupId(1, true);
					AnimalSpawnSettings.CheckAndSetAnimalAgentFlags(child, val4);
					SimulateAnimalAnimations(val4);
					val4.AddComponent((AgentComponent)(object)new CoverAnimalAgentComponent(val4));
					val4.GetComponent<CoverAnimalAgentComponent>().SetDynamicPatrolArea(dynamicPatrolArea);
					_coverAnimalPatrolPoints[firstScriptOfType] = val4;
					if ((int)val4.CurrentMortalityState == 0)
					{
						val4.ToggleInvulnerable();
					}
				}
			}
		}
	}

	private void SimulateAnimalAnimations(Agent agent)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		int num = 10 + MBRandom.RandomInt(90);
		for (int i = 0; i < num; i++)
		{
			agent.TickActionChannels(0.1f);
			agent.AgentVisuals.GetSkeleton().TickAnimations(0.1f, agent.AgentVisuals.GetGlobalFrame(), true);
		}
		Vec3 val = agent.ComputeAnimationDisplacement(0.1f * (float)num);
		if (((Vec3)(ref val)).LengthSquared > 0f)
		{
			agent.TeleportToPosition(agent.Position + val);
		}
	}

	public void OnCheckpointLoadedEvent(CheckpointLoadedMissionEvent checkpointLoadedMissionEvent)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (checkpointLoadedMissionEvent.LoadedCheckpointUniqueId < 0)
		{
			return;
		}
		string text = "sp_checkpoint_" + checkpointLoadedMissionEvent.LoadedCheckpointUniqueId;
		foreach (KeyValuePair<Agent, GameEntity> spawnedEnemyAgentsOnPatrolPoint in _spawnedEnemyAgentsOnPatrolPoints)
		{
			foreach (GameEntity child in spawnedEnemyAgentsOnPatrolPoint.Value.GetChildren())
			{
				GameEntity firstChildEntityWithTag = child.GetFirstChildEntityWithTag(text);
				if (firstChildEntityWithTag != (GameEntity)null)
				{
					spawnedEnemyAgentsOnPatrolPoint.Key.TeleportToPosition(firstChildEntityWithTag.GlobalPosition);
					break;
				}
			}
		}
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

	public bool IsSideDepleted(BattleSideEnum side)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		if ((int)side == 0)
		{
			return _spawnedEnemyAgentsOnPatrolPoints.Count <= 0;
		}
		if ((int)side == 1)
		{
			Agent main = Agent.Main;
			if (main == null)
			{
				return false;
			}
			return !main.IsActive();
		}
		return false;
	}

	public float GetReinforcementInterval()
	{
		return 0f;
	}

	public IEnumerable<IAgentOriginBase> GetAllTroopsForSide(BattleSideEnum side)
	{
		throw new NotImplementedException();
	}

	public int GetNumberOfPlayerControllableTroops()
	{
		throw new NotImplementedException();
	}

	public bool GetSpawnHorses(BattleSideEnum side)
	{
		throw new NotImplementedException();
	}
}
