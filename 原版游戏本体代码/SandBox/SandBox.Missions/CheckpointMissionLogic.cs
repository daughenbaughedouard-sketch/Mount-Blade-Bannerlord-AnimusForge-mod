using System.Collections.Generic;
using System.Linq;
using SandBox.CampaignBehaviors;
using SandBox.Objects;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.Missions;

public class CheckpointMissionLogic : MissionLogic
{
	private readonly Dictionary<Agent, AgentSaveData> _allSpawnedSaveableAgents;

	private readonly CheckpointCampaignBehavior _checkpointCampaignBehavior;

	private bool _isInitialized;

	private bool _isRenderingStarted;

	public CheckpointMissionLogic()
	{
		_allSpawnedSaveableAgents = new Dictionary<Agent, AgentSaveData>();
		_checkpointCampaignBehavior = Campaign.Current.GetCampaignBehavior<CheckpointCampaignBehavior>();
	}

	public override void EarlyStart()
	{
		DisablePatrolAreasAccordingToTheLastUsedCheckpoint();
	}

	public override void OnRenderingStarted()
	{
		_isRenderingStarted = true;
	}

	public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (affectedAgent.Team != Mission.Current.PlayerEnemyTeam || (int)agentState != 4)
		{
			return;
		}
		foreach (KeyValuePair<Agent, AgentSaveData> allSpawnedSaveableAgent in _allSpawnedSaveableAgents)
		{
			if (allSpawnedSaveableAgent.Key == affectedAgent)
			{
				AgentSaveData value = allSpawnedSaveableAgent.Value;
				Mat3 lookRotation = allSpawnedSaveableAgent.Key.LookRotation;
				Vec3 position = allSpawnedSaveableAgent.Key.Position;
				((AgentSaveData)(ref value)).UpdateSpawnFrame(new MatrixFrame(ref lookRotation, ref position));
				break;
			}
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnMissionTick(dt);
		if (_isInitialized || Agent.Main == null || !_isRenderingStarted)
		{
			return;
		}
		_isInitialized = true;
		if (_checkpointCampaignBehavior.LastUsedMissionCheckpointId >= 0)
		{
			List<GameEntity> list = new List<GameEntity>();
			Mission.Current.Scene.GetAllEntitiesWithScriptComponent<CheckpointArea>(ref list);
			CheckpointArea checkpointArea = null;
			foreach (GameEntity item in list)
			{
				CheckpointArea firstScriptOfType = item.GetFirstScriptOfType<CheckpointArea>();
				if (firstScriptOfType.UniqueId == _checkpointCampaignBehavior.LastUsedMissionCheckpointId)
				{
					checkpointArea = firstScriptOfType;
					Vec3 globalPosition = checkpointArea.SpawnPoint.GlobalPosition;
					Agent.Main.TeleportToPosition(globalPosition);
					break;
				}
			}
			if (checkpointArea == null)
			{
				List<GameEntity> list2 = new List<GameEntity>();
				Mission.Current.Scene.GetAllEntitiesWithScriptComponent<CheckpointUsePoint>(ref list2);
				foreach (GameEntity item2 in list2)
				{
					CheckpointUsePoint firstScriptOfType2 = item2.GetFirstScriptOfType<CheckpointUsePoint>();
					if (firstScriptOfType2.UniqueId == _checkpointCampaignBehavior.LastUsedMissionCheckpointId)
					{
						Vec3 globalPosition2 = firstScriptOfType2.SpawnPoint.GlobalPosition;
						Agent.Main.TeleportToPosition(globalPosition2);
						break;
					}
				}
			}
			Game.Current.EventManager.TriggerEvent<CheckpointLoadedMissionEvent>(new CheckpointLoadedMissionEvent(_checkpointCampaignBehavior.LastUsedMissionCheckpointId));
		}
		SpawnCorpses();
	}

	private bool CanUseCheckpoint()
	{
		bool result = true;
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (item.Team == Mission.Current.PlayerEnemyTeam && (item.IsCautious() || item.IsPatrollingCautious() || item.IsAlarmed()))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public void OnCheckpointUsed(int checkpointUniqueId)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		if (!CanUseCheckpoint())
		{
			return;
		}
		_checkpointCampaignBehavior.LastUsedMissionCheckpointId = checkpointUniqueId;
		_checkpointCampaignBehavior.CorpseList.Clear();
		foreach (KeyValuePair<Agent, AgentSaveData> allSpawnedSaveableAgent in _allSpawnedSaveableAgents)
		{
			if ((int)allSpawnedSaveableAgent.Key.State == 4 || (int)allSpawnedSaveableAgent.Key.State == 3)
			{
				_checkpointCampaignBehavior.CorpseList.Add(allSpawnedSaveableAgent.Value);
			}
		}
	}

	private void DisablePatrolAreasAccordingToTheLastUsedCheckpoint()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (Extensions.IsEmpty<AgentSaveData>((IEnumerable<AgentSaveData>)_checkpointCampaignBehavior.CorpseList))
		{
			return;
		}
		List<GameEntity> list = new List<GameEntity>();
		Mission.Current.Scene.GetAllEntitiesWithScriptComponent<DynamicPatrolAreaParent>(ref list);
		foreach (AgentSaveData corpse in _checkpointCampaignBehavior.CorpseList)
		{
			foreach (GameEntity item in list)
			{
				foreach (GameEntity child in item.GetChildren())
				{
					if (child.GetChild(0).Tags.SequenceEqual(corpse.AgentSpawnPointTags))
					{
						((MissionObject)child.GetChild(0).GetFirstScriptOfType<PatrolPoint>()).SetDisabled(true);
					}
				}
			}
		}
	}

	private void SpawnCorpses()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		foreach (AgentSaveData corpse in _checkpointCampaignBehavior.CorpseList)
		{
			AgentSaveData current = corpse;
			CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>(current.CharacterStringId);
			AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor))).EquipmentSeed(current.AgentSeed).InitialPosition(ref current.SpawnFrame.origin);
			Vec3 val2 = current.SpawnFrame.rotation.f;
			val2 = ((Vec3)(ref val2)).NormalizedCopy();
			Vec2 asVec = ((Vec3)(ref val2)).AsVec2;
			AgentBuildData val3 = obj.InitialDirection(ref asVec);
			Agent val4 = Mission.Current.SpawnAgent(val3, false);
			val4.MakeDead(true, ActionIndexCache.act_none, -1);
			GameEntity entity = val4.AgentVisuals.GetEntity();
			string[] agentSpawnPointTags = current.AgentSpawnPointTags;
			foreach (string text in agentSpawnPointTags)
			{
				entity.AddTag(text);
			}
			RegisterAgent(val4);
		}
	}

	public void RegisterAgent(Agent agent)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<Agent, AgentSaveData> allSpawnedSaveableAgents = _allSpawnedSaveableAgents;
		string stringId = ((MBObjectBase)agent.Character).StringId;
		Mat3 lookRotation = agent.LookRotation;
		Vec3 position = agent.Position;
		allSpawnedSaveableAgents.Add(agent, new AgentSaveData(stringId, new MatrixFrame(ref lookRotation, ref position), agent.AgentVisuals.GetEntity().Tags, agent.Origin.Seed));
	}
}
