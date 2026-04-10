using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Conversation.MissionLogics;

public class MissionConversationLogic : MissionLogic
{
	private const string CenterConversationPointMappingTag = "CenterConversationPoint";

	private const int StartingConversationFromBehindAngleThresholdInDegrees = 45;

	private const float MinDistanceThresholdToOpenConversation = 0.2f;

	private const float MaxDistanceThresholdToOpenConversation = 2f;

	private MissionMode _oldMissionMode;

	private readonly CharacterObject _teleportNearCharacter;

	private GameEntity _selectedConversationPoint;

	private bool _conversationStarted;

	private bool _teleported;

	private bool _conversationAgentFound;

	private bool _disableStartConversation;

	private readonly Dictionary<string, MBList<GameEntity>> _conversationPoints;

	private string _customSpawnTag;

	private HashSet<Agent> _uninteractableAgents = new HashSet<Agent>();

	public static MissionConversationLogic Current => Mission.Current.GetMissionBehavior<MissionConversationLogic>();

	public MissionState State { get; private set; }

	public ConversationManager ConversationManager { get; private set; }

	public bool IsReadyForConversation
	{
		get
		{
			if (ConversationAgent != null && ConversationManager != null)
			{
				if (Agent.Main != null)
				{
					return Agent.Main.IsActive();
				}
				return false;
			}
			return false;
		}
	}

	public Agent ConversationAgent { get; private set; }

	public MissionConversationLogic(CharacterObject teleportNearChar)
	{
		_teleportNearCharacter = teleportNearChar;
		_conversationPoints = new Dictionary<string, MBList<GameEntity>>();
	}

	public MissionConversationLogic()
	{
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		CampaignEvents.LocationCharactersSimulatedEvent.AddNonSerializedListener((object)this, (Action)OnLocationCharactersSimulated);
	}

	public override void OnRemoveBehavior()
	{
		((MissionBehavior)this).OnRemoveBehavior();
		((IMbEventBase)CampaignEvents.LocationCharactersAreReadyToSpawnEvent).ClearListeners((object)this);
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if (_teleportNearCharacter != null && (object)agent.Character == _teleportNearCharacter)
		{
			ConversationAgent = agent;
			_conversationAgentFound = true;
		}
		if (agent.IsHuman)
		{
			BasicCharacterObject character = agent.Character;
			CharacterObject val = (CharacterObject)(object)((character is CharacterObject) ? character : null);
			if (val != null && val.Culture.FemaleDancer == val)
			{
				_uninteractableAgents.Add(agent);
			}
		}
	}

	public void SetSpawnArea(Alley alley)
	{
		_customSpawnTag = ((SettlementArea)alley).Tag;
	}

	public void SetSpawnArea(Workshop workshop)
	{
		_customSpawnTag = ((SettlementArea)workshop).Tag;
	}

	public void SetSpawnArea(string customTag)
	{
		_customSpawnTag = customTag;
	}

	private void OnLocationCharactersSimulated()
	{
		if (_conversationAgentFound)
		{
			if (FillConversationPointList())
			{
				DetermineSpawnPoint();
				_teleported = TryToTeleportBothToCertainPoints();
			}
			else
			{
				((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>()?.TeleportTargetAgentNearReferenceAgent(ConversationAgent, Agent.Main, teleportFollowers: true, teleportOpposite: false);
			}
		}
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (!IsReadyForConversation)
		{
			return;
		}
		if (!_teleported)
		{
			((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>().TeleportTargetAgentNearReferenceAgent(ConversationAgent, Agent.Main, teleportFollowers: true, teleportOpposite: false);
			_teleported = true;
		}
		if (_teleportNearCharacter != null && !_conversationStarted)
		{
			StartConversation(ConversationAgent, setActionsInstantly: true, isInitialization: true);
			if (ConversationManager.NeedsToActivateForMapConversation && !GameNetwork.IsReplay)
			{
				ConversationManager.BeginConversation();
			}
		}
	}

	private bool TryToTeleportBothToCertainPoints()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		MissionAgentHandler missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>();
		bool flag = Agent.Main.MountAgent != null;
		WorldFrame val = default(WorldFrame);
		((WorldFrame)(ref val))._002Ector(_selectedConversationPoint.GetGlobalFrame().rotation, new WorldPosition(Agent.Main.Mission.Scene, _selectedConversationPoint.GetGlobalFrame().origin));
		((WorldPosition)(ref val.Origin)).SetVec2(((WorldPosition)(ref val.Origin)).AsVec2 + ((Vec3)(ref val.Rotation.f)).AsVec2 * (flag ? 1f : 0.5f));
		WorldFrame val2 = default(WorldFrame);
		((WorldFrame)(ref val2))._002Ector(_selectedConversationPoint.GetGlobalFrame().rotation, new WorldPosition(Agent.Main.Mission.Scene, _selectedConversationPoint.GetGlobalFrame().origin));
		((WorldPosition)(ref val2.Origin)).SetVec2(((WorldPosition)(ref val2.Origin)).AsVec2 - ((Vec3)(ref val2.Rotation.f)).AsVec2 * (flag ? 1f : 0.5f));
		Vec3 val3 = default(Vec3);
		((Vec3)(ref val3))._002Ector(((WorldPosition)(ref val.Origin)).AsVec2 - ((WorldPosition)(ref val2.Origin)).AsVec2, 0f, -1f);
		Vec3 val4 = default(Vec3);
		((Vec3)(ref val4))._002Ector(((WorldPosition)(ref val2.Origin)).AsVec2 - ((WorldPosition)(ref val.Origin)).AsVec2, 0f, -1f);
		((Mat3)(ref val.Rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		ConversationAgent.LookDirection = ((Vec3)(ref val4)).NormalizedCopy();
		ConversationAgent.TeleportToPosition(((WorldPosition)(ref val.Origin)).GetGroundVec3());
		((Mat3)(ref val2.Rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		Vec2 val5;
		if (Agent.Main.MountAgent != null)
		{
			val5 = ((Vec3)(ref val4)).AsVec2;
			val5 = ((Vec2)(ref val5)).RightVec();
			Vec3 val6 = ((Vec2)(ref val5)).ToVec3(0f);
			Agent.Main.MountAgent.LookDirection = ((Vec3)(ref val6)).NormalizedCopy();
		}
		((MissionBehavior)this).Mission.MainAgent.LookDirection = ((Vec3)(ref val3)).NormalizedCopy();
		((MissionBehavior)this).Mission.MainAgent.TeleportToPosition(((WorldPosition)(ref val2.Origin)).GetGroundVec3());
		SetConversationAgentAnimations(ConversationAgent);
		WorldPosition origin = val2.Origin;
		((WorldPosition)(ref origin)).SetVec2(((WorldPosition)(ref origin)).AsVec2 - ((Vec3)(ref val2.Rotation.s)).AsVec2 * 2f);
		if (missionBehavior != null)
		{
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				LocationCharacter val7 = LocationComplex.Current.FindCharacter((IAgent)(object)item);
				AccompanyingCharacter accompanyingCharacter = PlayerEncounter.LocationEncounter.GetAccompanyingCharacter(val7);
				if (accompanyingCharacter != null && accompanyingCharacter.IsFollowingPlayerAtMissionStart)
				{
					if (item.MountAgent != null && Agent.Main.MountAgent != null)
					{
						item.MountAgent.LookDirection = Agent.Main.MountAgent.LookDirection;
					}
					if (accompanyingCharacter.LocationCharacter.Character == _teleportNearCharacter)
					{
						item.LookDirection = ((Vec3)(ref val4)).NormalizedCopy();
						val5 = ((Vec3)(ref val2.Rotation.f)).AsVec2;
						item.SetMovementDirection(ref val5);
						item.TeleportToPosition(((WorldPosition)(ref val.Origin)).GetGroundVec3());
					}
					else
					{
						item.LookDirection = ((Vec3)(ref val3)).NormalizedCopy();
						val5 = ((Vec3)(ref val.Rotation.f)).AsVec2;
						item.SetMovementDirection(ref val5);
						item.TeleportToPosition(((WorldPosition)(ref origin)).GetGroundVec3());
					}
				}
			}
		}
		_teleported = true;
		return true;
	}

	private void SetConversationAgentAnimations(Agent conversationAgent)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		CampaignAgentComponent component = conversationAgent.GetComponent<CampaignAgentComponent>();
		AgentBehavior agentBehavior = component.AgentNavigator?.GetActiveBehavior();
		if (agentBehavior != null)
		{
			agentBehavior.IsActive = false;
			component.AgentNavigator.ForceThink(0f);
			conversationAgent.SetActionChannel(0, ref ActionIndexCache.act_none, false, (AnimFlags)conversationAgent.GetCurrentActionPriority(0), 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			conversationAgent.SetActionChannel(0, ref ActionIndexCache.act_none, false, (AnimFlags)Math.Min(conversationAgent.GetCurrentActionPriority(0), 73), 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			conversationAgent.TickActionChannels(0.1f);
			conversationAgent.AgentVisuals.GetSkeleton().TickAnimationsAndForceUpdate(0.1f, conversationAgent.AgentVisuals.GetGlobalFrame(), true);
		}
	}

	private void OnConversationEnd()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent conversationAgent in ConversationManager.ConversationAgents)
		{
			conversationAgent.AgentVisuals.SetVisible(true);
			conversationAgent.AgentVisuals.SetClothComponentKeepStateOfAllMeshes(false);
			Agent mountAgent = conversationAgent.MountAgent;
			if (mountAgent != null)
			{
				mountAgent.AgentVisuals.SetVisible(true);
			}
		}
		if ((int)((MissionBehavior)this).Mission.Mode == 1 && !((MissionBehavior)this).Mission.IsMissionEnding)
		{
			((MissionBehavior)this).Mission.SetMissionMode(_oldMissionMode, false);
		}
		if (Agent.Main != null)
		{
			Agent.Main.AgentVisuals.SetVisible(true);
			Agent.Main.AgentVisuals.SetClothComponentKeepStateOfAllMeshes(false);
			if (Agent.Main.MountAgent != null)
			{
				Agent.Main.MountAgent.AgentVisuals.SetVisible(true);
			}
		}
		((MissionBehavior)this).Mission.MainAgentServer.Controller = (AgentControllerType)2;
		ConversationManager.ConversationEnd -= OnConversationEnd;
	}

	public override void EarlyStart()
	{
		GameState activeState = Game.Current.GameStateManager.ActiveState;
		State = (MissionState)(object)((activeState is MissionState) ? activeState : null);
	}

	protected override void OnEndMission()
	{
		if (ConversationManager != null && ConversationManager.IsConversationInProgress)
		{
			ConversationManager.EndConversation();
		}
		State = null;
		CampaignEvents.LocationCharactersSimulatedEvent.ClearListeners((object)this);
	}

	public override void OnAgentInteraction(Agent userAgent, Agent agent, sbyte agentBoneIndex)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Campaign.Current.GameMode == 1 && ((MissionBehavior)this).IsThereAgentAction(userAgent, agent) && !ConversationManager.IsConversationInProgress)
		{
			StartConversation(agent, setActionsInstantly: false);
		}
	}

	public void StartConversation(Agent agent, bool setActionsInstantly, bool isInitialization = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		_oldMissionMode = ((MissionBehavior)this).Mission.Mode;
		ConversationManager = Campaign.Current.ConversationManager;
		ConversationManager.SetupAndStartMissionConversation((IAgent)(object)agent, (IAgent)(object)((MissionBehavior)this).Mission.MainAgent, setActionsInstantly);
		ConversationManager.ConversationEnd += OnConversationEnd;
		_conversationStarted = true;
		foreach (Agent conversationAgent in ConversationManager.ConversationAgents)
		{
			conversationAgent.ForceAiBehaviorSelection();
			conversationAgent.AgentVisuals.SetClothComponentKeepStateOfAllMeshes(true);
		}
		((MissionBehavior)this).Mission.MainAgentServer.AgentVisuals.SetClothComponentKeepStateOfAllMeshes(true);
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)1, setActionsInstantly);
	}

	public override bool IsThereAgentAction(Agent userAgent, Agent otherAgent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((MissionBehavior)this).Mission.Mode != 2 && (int)((MissionBehavior)this).Mission.Mode != 3 && (int)((MissionBehavior)this).Mission.Mode != 1 && otherAgent.IsHuman && !_disableStartConversation && otherAgent.IsActive() && !otherAgent.IsEnemyOf(userAgent) && otherAgent.GetDistanceTo(Agent.Main) > 0.2f && otherAgent.GetDistanceTo(Agent.Main) < 2f && !_uninteractableAgents.Contains(otherAgent))
		{
			if (Extensions.HasAnyFlag<AnimFlags>(otherAgent.GetCurrentAnimationFlag(0), (AnimFlags)755914244096L))
			{
				Vec3 lookDirection = userAgent.LookDirection;
				Vec2 asVec = ((Vec3)(ref lookDirection)).AsVec2;
				lookDirection = otherAgent.LookDirection;
				return Math.Abs(((Vec2)(ref asVec)).AngleBetween(((Vec3)(ref lookDirection)).AsVec2) * (180f / MathF.PI)) > 45f;
			}
			return true;
		}
		return false;
	}

	public override void OnRenderingStarted()
	{
		ConversationManager = Campaign.Current.ConversationManager;
		if (ConversationManager == null)
		{
			throw new ArgumentNullException("conversationManager");
		}
	}

	public void DisableStartConversation(bool isDisabled)
	{
		_disableStartConversation = isDisabled;
	}

	private bool FillConversationPointList()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		List<GameEntity> list = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_player_conversation").ToList();
		bool result = false;
		if (!Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)list))
		{
			List<AreaMarker> list2 = MBExtensions.FindAllWithType<AreaMarker>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.ActiveMissionObjects).ToList();
			foreach (GameEntity item in list)
			{
				bool flag = false;
				foreach (AreaMarker item2 in list2)
				{
					if (item2.IsPositionInRange(item.GlobalPosition))
					{
						if (_conversationPoints.ContainsKey(item2.Tag))
						{
							((List<GameEntity>)(object)_conversationPoints[item2.Tag]).Add(item);
						}
						else
						{
							Dictionary<string, MBList<GameEntity>> conversationPoints = _conversationPoints;
							string tag = item2.Tag;
							MBList<GameEntity> obj = new MBList<GameEntity>();
							((List<GameEntity>)(object)obj).Add(item);
							conversationPoints.Add(tag, obj);
						}
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					if (_conversationPoints.ContainsKey("CenterConversationPoint"))
					{
						((List<GameEntity>)(object)_conversationPoints["CenterConversationPoint"]).Add(item);
						continue;
					}
					Dictionary<string, MBList<GameEntity>> conversationPoints2 = _conversationPoints;
					MBList<GameEntity> obj2 = new MBList<GameEntity>();
					((List<GameEntity>)(object)obj2).Add(item);
					conversationPoints2.Add("CenterConversationPoint", obj2);
				}
			}
			result = true;
		}
		else
		{
			Debug.FailedAssert("Scene must have at least one 'sp_player_conversation' game entity. Scene Name: " + Mission.Current.Scene.GetName(), "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Conversation\\Logics\\MissionConversationLogic.cs", "FillConversationPointList", 404);
		}
		return result;
	}

	private void DetermineSpawnPoint()
	{
		if (_customSpawnTag != null && _conversationPoints.TryGetValue(_customSpawnTag, out var value))
		{
			_selectedConversationPoint = Extensions.GetRandomElement<GameEntity>(value);
			return;
		}
		string agentsTag = ConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SpecialTargetTag;
		if (agentsTag != null)
		{
			KeyValuePair<string, MBList<GameEntity>> keyValuePair = _conversationPoints.FirstOrDefault((KeyValuePair<string, MBList<GameEntity>> x) => agentsTag.Contains(x.Key));
			MBList<GameEntity> value2 = keyValuePair.Value;
			_selectedConversationPoint = ((value2 != null) ? Extensions.GetRandomElement<GameEntity>(value2) : null);
		}
		if (_selectedConversationPoint == (GameEntity)null)
		{
			if (_conversationPoints.ContainsKey("CenterConversationPoint"))
			{
				_selectedConversationPoint = Extensions.GetRandomElement<GameEntity>(_conversationPoints["CenterConversationPoint"]);
			}
			else
			{
				_selectedConversationPoint = Extensions.GetRandomElement<GameEntity>(Extensions.GetRandomElementInefficiently<KeyValuePair<string, MBList<GameEntity>>>((IEnumerable<KeyValuePair<string, MBList<GameEntity>>>)_conversationPoints).Value);
			}
		}
	}
}
