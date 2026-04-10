using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Helpers;
using SandBox.Conversation;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.AgentBehaviors;
using SandBox.Objects;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.Source.Objects;

namespace SandBox.Missions.MissionLogics;

public class DisguiseMissionLogic : MissionLogic, IPlayerInputEffector, IMissionBehavior
{
	public class ShadowingAgentOffenseInfo
	{
		public Agent Agent { get; }

		public bool CanPlayerCameraSeeTheAgent { get; private set; }

		public StealthOffenseTypes OffenseType { get; private set; }

		public ShadowingAgentOffenseInfo(Agent agent, StealthOffenseTypes offenseType)
		{
			Agent = agent;
			OffenseType = offenseType;
		}

		public void SetCanPlayerCameraSeeTheAgent(bool value)
		{
			CanPlayerCameraSeeTheAgent = value;
		}

		internal void SetOffenseType(StealthOffenseTypes offenseType)
		{
			OffenseType = offenseType;
		}
	}

	public const float PlayerSuspiciousLevelMin = 0f;

	public const float PlayerSuspiciousLevelMax = 1f;

	public const float ToggleStealthModeSuspiciousThreshold = 0.95f;

	public const float MissionFailDistanceToTargetAgent = 5000f;

	private const float StartSuspiciousDecayAfterSeconds = 2f;

	private const float OfficerAgentPersonalZoneRadius = 3.5f;

	private const float DefaultAgentPersonalZoneRadius = 0f;

	private const float InConsistentMovementToleranceFactor = 0.2f;

	private const float MaximumWorstMovementRotationFactor = 1f;

	private const float InconsistentMovementDecayFactor = 2f;

	private const float CircularMovementDetectRadiusSquared = 4f;

	private const float DefaultDecayFactor = -0.01f;

	private const float DefaultSuspiciousFactor = 0.1f;

	private const float GuardSpawnDistanceThreshold = 20f;

	private const float MaximumContactAgentDistance = 250f;

	private const float StaticGuardSpawnPercentage = 0.3f;

	private readonly List<CharacterObject> _troopPool;

	private Dictionary<Agent, ShadowingAgentOffenseInfo> _disguiseAgentOffenseInfos;

	private Agent _contactAgent;

	private Timer _isAgentDeadTimer;

	private readonly List<GameEntity> _customPoints = new List<GameEntity>();

	private readonly List<GameEntity> _dynamicPoints = new List<GameEntity>();

	public float PlayerSuspiciousLevel;

	private Vec2 _lastFramePlayerPosition;

	private int _disabledFaceId;

	private readonly CharacterObject _defaultContractorCharacter;

	private readonly List<Agent> _officerAgents;

	private readonly List<Agent> _defaultDisguiseAgents;

	private readonly List<Agent> _agentsToBeRemoved;

	private readonly bool _willSetUpContact;

	private readonly Location _fromLocation;

	private Dictionary<Agent, AlarmedBehaviorGroup> _agentAlarmedBehaviorCache;

	private List<Agent> _suspiciousAgentsThisFrame;

	private MBList<GameEntity> _stealthIndoorLightingAreas;

	private bool _isBehaviorInitialized;

	private bool _firstTickPassed;

	private bool _firstEventControlTickPassed;

	private bool _disguiseAgentsStealthModeIsOn;

	private float _angleDifferenceBetweenCurrentAndLastPositionOfPlayer;

	private float _cumulativePositionAndRotationDifference;

	private Vec3 _averagePlayerPosition;

	private MissionTimer _lastSuspiciousTimer;

	private bool _contactSet;

	private int _staticGuardsCount;

	private bool _playerWillBeTakenPrisoner;

	public bool IsInStealthMode => PlayerSuspiciousLevel >= 0.95f;

	public ReadOnlyDictionary<Agent, ShadowingAgentOffenseInfo> ThreatAgentInfos { get; }

	public DisguiseMissionLogic(CharacterObject contractorCharacter, Location fromLocation, bool willSetUpContact)
	{
		_troopPool = CharacterHelper.GetTroopTree(Settlement.CurrentSettlement.Culture.BasicTroop, 2f, 3f).ToList();
		_defaultContractorCharacter = contractorCharacter;
		_fromLocation = fromLocation;
		_defaultDisguiseAgents = new List<Agent>();
		_officerAgents = new List<Agent>();
		_suspiciousAgentsThisFrame = new List<Agent>();
		_agentsToBeRemoved = new List<Agent>();
		_agentAlarmedBehaviorCache = new Dictionary<Agent, AlarmedBehaviorGroup>();
		_disguiseAgentOffenseInfos = new Dictionary<Agent, ShadowingAgentOffenseInfo>();
		ThreatAgentInfos = new ReadOnlyDictionary<Agent, ShadowingAgentOffenseInfo>(_disguiseAgentOffenseInfos);
		Game.Current.EventManager.RegisterEvent<LocationCharacterAgentSpawnedMissionEvent>((Action<LocationCharacterAgentSpawnedMissionEvent>)OnLocationCharacterAgentSpawned);
		CampaignEvents.BeforePlayerAgentSpawnEvent.AddNonSerializedListener((object)this, (ReferenceAction<MatrixFrame>)OnBeforePlayerAgentSpawn);
		CampaignEvents.CanPlayerMeetWithHeroAfterConversationEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanPlayerMeetWithHeroAfterConversation);
		_willSetUpContact = willSetUpContact;
		PlayerEncounter.LocationEncounter.RemoveAllAccompanyingCharacters();
	}

	private void OnBeforePlayerAgentSpawn(ref MatrixFrame matrixFrame)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (_fromLocation != null)
		{
			matrixFrame = GetSpawnFrameOfPassage(_fromLocation);
			((Mat3)(ref matrixFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		}
	}

	public override void OnCreated()
	{
		CampaignMission.Current.Location = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
	}

	public MatrixFrame GetSpawnFrameOfPassage(Location location)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame result = MatrixFrame.Identity;
		UsableMachine val = ((IEnumerable<UsableMachine>)Mission.Current.GetMissionBehavior<MissionAgentHandler>().TownPassageProps).FirstOrDefault((Func<UsableMachine, bool>)((UsableMachine x) => ((Passage)(object)x).ToLocation == location)) ?? ((IEnumerable<UsableMachine>)Mission.Current.GetMissionBehavior<MissionAgentHandler>().DisabledPassages).FirstOrDefault((Func<UsableMachine, bool>)((UsableMachine x) => ((Passage)(object)x).ToLocation == location));
		if (val != null)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)val.PilotStandingPoint).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			globalFrame.origin.z = ((MissionBehavior)this).Mission.Scene.GetGroundHeightAtPosition(globalFrame.origin, (BodyFlags)544321929);
			((Mat3)(ref globalFrame.rotation)).RotateAboutUp(MathF.PI);
			result = globalFrame;
		}
		return result;
	}

	public bool IsContactAgentTracked(Agent agent)
	{
		if (agent == _contactAgent)
		{
			return !_contactSet;
		}
		return false;
	}

	public bool CanCommonAreaFightBeTriggered()
	{
		return ContactAlreadySetCommonCondition();
	}

	private void CanPlayerMeetWithHeroAfterConversation(Hero hero, ref bool result)
	{
		result = ContactAlreadySetCommonCondition();
	}

	public bool ContactAlreadySetCommonCondition()
	{
		if (!_contactSet)
		{
			return !_willSetUpContact;
		}
		return true;
	}

	public bool IsOnLeftSide(Vec2 lineA, Vec2 lineB, Vec2 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (lineB.x - lineA.x) * (point.y - lineA.y) - (lineB.y - lineA.y) * (point.x - lineA.x) > 0f;
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		if (!agent.IsHuman)
		{
			return;
		}
		if (((IEnumerable<BasicCharacterObject>)_troopPool).Contains(agent.Character))
		{
			_defaultDisguiseAgents.Add(agent);
			_disguiseAgentOffenseInfos[agent] = new ShadowingAgentOffenseInfo(agent, StealthOffenseTypes.None);
			return;
		}
		BasicCharacterObject character = agent.Character;
		CharacterObject val;
		if ((val = (CharacterObject)(object)((character is CharacterObject) ? character : null)) != null && ((int)val.Occupation == 24 || (int)val.Occupation == 7))
		{
			_defaultDisguiseAgents.Add(agent);
			_disguiseAgentOffenseInfos[agent] = new ShadowingAgentOffenseInfo(agent, StealthOffenseTypes.None);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (affectedAgent.IsHuman)
		{
			if (_defaultDisguiseAgents.Contains(affectedAgent))
			{
				_defaultDisguiseAgents.Remove(affectedAgent);
			}
			if (_officerAgents.Contains(affectedAgent))
			{
				_officerAgents.Remove(affectedAgent);
			}
			if (affectedAgent.IsMainAgent)
			{
				Campaign.Current.GameMenuManager.SetNextMenu(_contactSet ? "settlement_player_unconscious_when_disguise_contact_set" : "settlement_player_unconscious_when_disguise_contact_not_set");
			}
		}
	}

	private void SetStealthModeToDisguiseAgents(bool isActive)
	{
		foreach (Agent defaultDisguiseAgent in _defaultDisguiseAgents)
		{
			SetStealthModeInternal(defaultDisguiseAgent, isActive);
		}
		foreach (Agent officerAgent in _officerAgents)
		{
			SetStealthModeInternal(officerAgent, isActive);
		}
		_disguiseAgentsStealthModeIsOn = isActive;
	}

	private void SetStealthModeInternal(Agent agent, bool isActive)
	{
		if (!_agentAlarmedBehaviorCache.TryGetValue(agent, out var value))
		{
			return;
		}
		value.DoNotCheckForAlarmFactorIncrease = !isActive;
		if (isActive)
		{
			value.DoNotIncreaseAlarmFactorDueToSeeingOrHearingTheEnemy = false;
			if (agent.InteractingWithAnyGameObject())
			{
				agent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			}
		}
	}

	protected override void OnEndMission()
	{
		_officerAgents.Clear();
		_defaultDisguiseAgents.Clear();
		_agentsToBeRemoved.Clear();
		_agentAlarmedBehaviorCache = null;
		_suspiciousAgentsThisFrame = null;
		if (!_playerWillBeTakenPrisoner && Agent.Main != null && Agent.Main.IsActive())
		{
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				if (!item.IsMainAgent && item.IsAlarmed())
				{
					Campaign.Current.GameMenuManager.SetNextMenu("settlement_player_run_away_when_disguise");
				}
			}
		}
		Game.Current.EventManager.UnregisterEvent<LocationCharacterAgentSpawnedMissionEvent>((Action<LocationCharacterAgentSpawnedMissionEvent>)OnLocationCharacterAgentSpawned);
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
		Campaign.Current.ConversationManager.RemoveRelatedLines((object)this);
	}

	private void InitializeMissionBehavior()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		Mission.Current.IsKingdomWindowAccessible = false;
		Mission.Current.IsBannerWindowAccessible = false;
		Mission.Current.IsClanWindowAccessible = false;
		Mission.Current.IsCharacterWindowAccessible = false;
		Mission.Current.IsEncyclopediaWindowAccessible = false;
		Mission.Current.IsInventoryAccessible = false;
		Mission.Current.IsPartyWindowAccessible = false;
		SandBoxHelpers.MissionHelper.SpawnPlayer(((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawnpoint_player"), civilianEquipment: false, noHorses: true);
		List<GameEntity> list = new List<GameEntity>();
		((MissionBehavior)this).Mission.Scene.GetAllEntitiesWithScriptComponent<StealthIndoorLightingArea>(ref list);
		_stealthIndoorLightingAreas = new MBList<GameEntity>(list);
		Mission.Current.GetMissionBehavior<MissionAgentHandler>().SpawnLocationCharacters();
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("navigation_mesh_deactivator");
		if (val != (GameEntity)null)
		{
			NavigationMeshDeactivator firstScriptOfType = val.GetFirstScriptOfType<NavigationMeshDeactivator>();
			_disabledFaceId = firstScriptOfType.DisableFaceWithId;
		}
		SetStealthModeToDisguiseAgents(isActive: false);
		Vec3 position = Agent.Main.Position;
		_lastFramePlayerPosition = ((Vec3)(ref position)).AsVec2;
		_averagePlayerPosition = Agent.Main.Position - Agent.Main.Frame.rotation.f * 2f;
		_lastSuspiciousTimer = new MissionTimer(2f);
		foreach (Agent item in _agentsToBeRemoved)
		{
			item.FadeOut(true, true);
		}
		_agentsToBeRemoved.Clear();
		Campaign.Current.ConversationManager.AddDialogFlow(GetContactDialogFlow(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlow1(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlow2(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlow3(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlow4(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(GetThugDialogFlow(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(FailedDialogFlow(), (object)this);
		if (_willSetUpContact)
		{
			SpawnContactAgent();
			TogglePassages(isActive: false);
			_contactSet = false;
		}
		else
		{
			_contactSet = true;
		}
		TurnGuardsToDisguiseAgents();
		SpawnCustomGuards();
	}

	private void TogglePassages(bool isActive)
	{
		foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("npc_passage"))
		{
			PassageUsePoint firstScriptOfTypeRecursive = item.GetFirstScriptOfTypeRecursive<PassageUsePoint>();
			if (firstScriptOfTypeRecursive != null)
			{
				if (isActive)
				{
					((MissionObject)firstScriptOfTypeRecursive).SetEnabledAndMakeVisible(false, false);
				}
				else
				{
					((MissionObject)firstScriptOfTypeRecursive).SetDisabledAndMakeInvisible(false, false);
				}
			}
		}
	}

	private void SpawnCustomGuards()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		List<GameEntity> list = Mission.Current.Scene.FindEntitiesWithTag("npc_common").ToList();
		list.AddRange(Mission.Current.Scene.FindEntitiesWithTag("npc_wait").ToList());
		List<AreaMarker> list2 = (from x in Mission.Current.Scene.FindEntitiesWithTag("alley_marker")
			select x.GetFirstScriptOfType<AreaMarker>()).ToList();
		list2.AddRange(from x in Mission.Current.Scene.FindEntitiesWithTag("workshop_area_marker")
			select x.GetFirstScriptOfType<AreaMarker>());
		int num = default(int);
		Vec3 val;
		foreach (GameEntity item in list)
		{
			Vec3 globalPosition = item.GlobalPosition;
			if (!(Mission.Current.Scene.GetNavigationMeshForPosition(ref globalPosition, ref num, 1.5f, false) != UIntPtr.Zero) || item.GetFirstScriptOfTypeRecursive<StandingPoint>() == null || ((UsableMissionObject)item.GetFirstScriptOfTypeRecursive<StandingPoint>()).IsDeactivated || num == _disabledFaceId)
			{
				continue;
			}
			bool flag = false;
			foreach (AreaMarker item2 in list2)
			{
				if (item2.IsPositionInRange(globalPosition))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			bool flag2 = false;
			foreach (Agent item3 in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				val = item3.Position;
				if (((Vec3)(ref val)).Distance(globalPosition) < 2f)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				_customPoints.Add(item);
			}
		}
		for (int num2 = _customPoints.Count - 1; num2 >= 0; num2--)
		{
			for (int num3 = 0; num3 < num2; num3++)
			{
				GameEntity obj = _customPoints[num2];
				GameEntity val2 = _customPoints[num3];
				val = obj.GlobalPosition;
				if (((Vec3)(ref val)).Distance(val2.GlobalPosition) < 20f)
				{
					_customPoints.RemoveAt(num2);
					break;
				}
			}
		}
		_staticGuardsCount = (int)((float)_customPoints.Count * 0.3f);
		for (int num4 = 0; num4 < _customPoints.Count; num4++)
		{
			GameEntity val3 = _customPoints[num4];
			CharacterObject randomElementInefficiently = Extensions.GetRandomElementInefficiently<CharacterObject>((IEnumerable<CharacterObject>)_troopPool);
			Vec3 globalPosition2 = val3.GlobalPosition;
			MatrixFrame frame = val3.GetFrame();
			Vec2 asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
			Agent ownerAgent = SpawnDisguiseMissionAgentInternal(randomElementInefficiently, globalPosition2, ((Vec2)(ref asVec)).Normalized(), "_guard");
			if (num4 > _staticGuardsCount)
			{
				ScriptBehavior.AddTargetWithDelegate(ownerAgent, GuardAgentSelectTargetDelegate(), GuardAgentWaitDelegate, GuardAgentOnTargetReachDelegate);
				_dynamicPoints.Add(val3);
			}
		}
	}

	private bool GuardAgentOnTargetReachDelegate(Agent agent, ref Agent targetAgent, ref UsableMachine targetUsableMachine, ref WorldFrame targetFrame)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		GameEntity randomElement = Extensions.GetRandomElement<GameEntity>((IReadOnlyList<GameEntity>)_dynamicPoints);
		WorldFrame val = default(WorldFrame);
		((WorldFrame)(ref val))._002Ector(randomElement.GetGlobalFrame().rotation, new WorldPosition(Mission.Current.Scene, randomElement.GetGlobalFrame().origin));
		targetFrame = val;
		return true;
	}

	private void GuardAgentWaitDelegate(Agent agent, ref float waitTimeInSeconds)
	{
		waitTimeInSeconds = MBRandom.RandomInt(10, 80);
	}

	private ScriptBehavior.SelectTargetDelegate GuardAgentSelectTargetDelegate()
	{
		return delegate(Agent agent1, ref Agent targetAgent, ref UsableMachine machine, ref WorldFrame frame, ref float customTargetReachedRangeThreshold, ref float customTargetReachedRotationThreshold)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			customTargetReachedRangeThreshold = 2.5f;
			customTargetReachedRotationThreshold = 0.8f;
			GameEntity randomElement = Extensions.GetRandomElement<GameEntity>((IReadOnlyList<GameEntity>)_dynamicPoints);
			frame = new WorldFrame(randomElement.GetGlobalFrame().rotation, new WorldPosition(Mission.Current.Scene, randomElement.GetGlobalFrame().origin));
			return true;
		};
	}

	private void TurnGuardsToDisguiseAgents()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		for (int num = ((List<Agent>)(object)((MissionBehavior)this).Mission.Agents).Count - 1; num >= 0; num--)
		{
			Agent val = ((List<Agent>)(object)((MissionBehavior)this).Mission.Agents)[num];
			CharacterObject val2;
			if (val.IsHuman && (val2 = (CharacterObject)/*isinst with value type is only supported in some contexts*/) != null && !((BasicCharacterObject)val2).IsFemale && ((int)val2.Occupation == 7 || (int)val2.Occupation == 24))
			{
				AddBehaviorGroups(val);
				val.SetTeam(((MissionBehavior)this).Mission.PlayerEnemyTeam, true);
				val.SetAgentFlags((AgentFlag)(val.GetAgentFlags() | 0x4000 | 0x10000));
				string text = ActionSetCode.GenerateActionSetNameWithSuffix(val.Monster, false, "_guard");
				AnimationSystemData val3 = MonsterExtensions.FillAnimationSystemData(val.Monster, MBGlobals.GetActionSet(text), val.Character.GetStepSize(), false);
				val.SetActionSet(ref val3);
				SetStealthModeInternal(val, _disguiseAgentsStealthModeIsOn);
				val.SetMortalityState((MortalityState)2);
				if (val.Character.IsRanged)
				{
					val.InitializeSpawnEquipment(val.Character.FirstBattleEquipment.Clone(true));
				}
			}
		}
	}

	public Agent SpawnDisguiseMissionAgentInternal(CharacterObject agentCharacter, Vec3 initialPosition, Vec2 initialDirection, string actionSetId, bool isEnemy = true)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		Equipment val = ((BasicCharacterObject)agentCharacter).FirstBattleEquipment.Clone(true);
		AgentBuildData val2 = new AgentBuildData((BasicCharacterObject)(object)agentCharacter).InitialPosition(ref initialPosition).InitialDirection(ref initialDirection).CivilianEquipment(false)
			.Equipment(val)
			.NoHorses(true)
			.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)agentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
		if (isEnemy)
		{
			val2.Team(((MissionBehavior)this).Mission.PlayerEnemyTeam);
		}
		Agent val3 = Mission.Current.SpawnAgent(val2, false);
		AddBehaviorGroups(val3);
		if (isEnemy)
		{
			val3.SetAgentFlags((AgentFlag)(val3.GetAgentFlags() | 0x4000 | 0x10000));
		}
		string text = ActionSetCode.GenerateActionSetNameWithSuffix(val3.Monster, false, actionSetId);
		AnimationSystemData val4 = MonsterExtensions.FillAnimationSystemData(val2.AgentMonster, MBGlobals.GetActionSet(text), ((BasicCharacterObject)agentCharacter).GetStepSize(), false);
		val3.SetActionSet(ref val4);
		SetStealthModeInternal(val3, _disguiseAgentsStealthModeIsOn);
		val3.SetMortalityState((MortalityState)2);
		return val3;
	}

	private void AddBehaviorGroups(Agent agent)
	{
		AgentNavigator agentNavigator = agent.GetComponent<CampaignAgentComponent>().CreateAgentNavigator();
		agentNavigator.AddBehaviorGroup<DailyBehaviorGroup>();
		agentNavigator.AddBehaviorGroup<AlarmedBehaviorGroup>();
		AlarmedBehaviorGroup behaviorGroup = agentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>();
		behaviorGroup.AddBehavior<CautiousBehavior>();
		behaviorGroup.AddBehavior<FightBehavior>();
		agent.SetAgentExcludeStateForFaceGroupId(_disabledFaceId, true);
		_agentAlarmedBehaviorCache.Add(agent, behaviorGroup);
	}

	private void SpawnContactAgent()
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Expected O, but got Unknown
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		float num = 2.5f;
		float num2 = 10f;
		IEnumerable<GameEntity> enumerable = Mission.Current.Scene.FindEntitiesWithTag("npc_passage");
		List<GameEntity> list = new List<GameEntity>();
		foreach (GameEntity item in enumerable)
		{
			Passage firstScriptOfType = item.GetFirstScriptOfType<Passage>();
			if (firstScriptOfType != null)
			{
				if (firstScriptOfType.ToLocation == Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern"))
				{
					list.Add(item);
				}
				else if (firstScriptOfType.ToLocation == Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("arena"))
				{
					list.Add(item);
				}
			}
		}
		IEnumerable<GameEntity> source = Mission.Current.Scene.FindEntitiesWithTag("workshop_area_marker");
		list.AddRange(source.ToList());
		float num3 = float.MinValue;
		float num4 = 250f;
		GameEntity val = null;
		WorldPosition val2 = default(WorldPosition);
		WorldPosition val3 = default(WorldPosition);
		float num5 = default(float);
		PathFaceRecord val4 = default(PathFaceRecord);
		foreach (GameEntity item2 in list)
		{
			((WorldPosition)(ref val2))._002Ector(Mission.Current.Scene, item2.GlobalPosition);
			((WorldPosition)(ref val3))._002Ector(Mission.Current.Scene, Agent.Main.Position);
			bool pathDistanceBetweenPositions = Mission.Current.Scene.GetPathDistanceBetweenPositions(ref val2, ref val3, 0.1f, ref num5);
			((PathFaceRecord)(ref val4))._002Ector(-1, -1, -1);
			Mission.Current.Scene.GetNavMeshFaceIndex(ref val4, item2.GlobalPosition, false);
			if (val == (GameEntity)null && ((PathFaceRecord)(ref val4)).IsValid())
			{
				val = item2;
			}
			if (((PathFaceRecord)(ref val4)).IsValid() && pathDistanceBetweenPositions && num5 < num4 && num5 > num3)
			{
				num3 = num5;
				val = item2;
			}
		}
		if (val == (GameEntity)null)
		{
			val = list.First();
		}
		WorldPosition val5 = default(WorldPosition);
		((WorldPosition)(ref val5))._002Ector(Mission.Current.Scene, Agent.Main.Position);
		Vec3 val6 = val.GlobalPosition;
		PathFaceRecord val7 = default(PathFaceRecord);
		((PathFaceRecord)(ref val7))._002Ector(-1, -1, -1);
		Mission.Current.Scene.GetNavMeshFaceIndex(ref val7, val6, false);
		WorldPosition val8 = default(WorldPosition);
		((WorldPosition)(ref val8))._002Ector(Mission.Current.Scene, val6);
		int num6 = 0;
		float num7 = default(float);
		while ((val7.FaceGroupIndex != _disabledFaceId || !Mission.Current.Scene.GetPathDistanceBetweenPositions(ref val8, ref val5, 0.3f, ref num7) || num7 < 5f || num7 > 40f) && num6 <= 150)
		{
			val6 = Mission.Current.GetRandomPositionAroundPoint(val.GetFrame().origin, num, num2, MBRandom.RandomFloat < 0.5f);
			((WorldPosition)(ref val8))._002Ector(Mission.Current.Scene, val6);
			Mission.Current.Scene.GetNavMeshFaceIndex(ref val7, val6, true);
			num6++;
		}
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)_defaultContractorCharacter).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)_defaultContractorCharacter, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.SpectatorTeam).InitialPosition(ref val6);
		Vec2 val9 = Vec2.One;
		val9 = ((Vec2)(ref val9)).Normalized();
		AgentBuildData val10 = obj.InitialDirection(ref val9).CivilianEquipment(true).NoHorses(true)
			.NoWeapons(true)
			.ClothingColor1(((MissionBehavior)this).Mission.PlayerTeam.Color)
			.ClothingColor2(((MissionBehavior)this).Mission.PlayerTeam.Color2);
		_contactAgent = ((MissionBehavior)this).Mission.SpawnAgent(val10, false);
		_contactAgent.GetComponent<CampaignAgentComponent>().CreateAgentNavigator();
		Campaign.Current.VisualTrackerManager.SetDirty();
	}

	private DialogFlow GetNotableDialogFlow1()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		TextObject val = new TextObject("{=7hlGVkbq}{PLAYER.NAME}... I don't know why you're dressed like that, and I don't think I want to know. If you look around, though, I think you'll find someone who can help you out.", (Dictionary<string, object>)null);
		return DialogFlow.CreateDialogFlow("start", 1000).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => GeneralNotableDialogCondition() && ConversationMission.OneToOneConversationCharacter.HeroObject.HasMet))
			.CloseDialog();
	}

	private DialogFlow GetNotableDialogFlow2()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		return DialogFlow.CreateDialogFlow("start", 1000).NpcLine(new TextObject("{=RAA6bEw8}If you're a stranger in this town, I'm sure you can find someone who'll let you stay on a pile of straw or under a bridge for a few coppers.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => DialogCondition2() || BlackSmithCondition()))
			.CloseDialog();
	}

	private DialogFlow GetNotableDialogFlow3()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		return DialogFlow.CreateDialogFlow("start", 1000).NpcLine(new TextObject("{=tgUUxK7Z}Look, mate - I can't really help you right now, but I'm sure if you look around you can find someone who'll give you whatever you need.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(DialogCondition3))
			.CloseDialog();
	}

	private DialogFlow GetNotableDialogFlow4()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		return DialogFlow.CreateDialogFlow("start", 1000).NpcLine(new TextObject("{=qdDRe8QC}Clear off, you beggar. Find someone who caters to the likes of you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(DialogCondition4))
			.CloseDialog();
	}

	private bool DialogCondition2()
	{
		if (GeneralNotableDialogCondition())
		{
			int traitLevel = ConversationMission.OneToOneConversationCharacter.HeroObject.GetTraitLevel(DefaultTraits.Generosity);
			int traitLevel2 = ConversationMission.OneToOneConversationCharacter.HeroObject.GetTraitLevel(DefaultTraits.Mercy);
			return traitLevel + traitLevel2 > 0;
		}
		return false;
	}

	private bool DialogCondition3()
	{
		if (GeneralNotableDialogCondition())
		{
			int traitLevel = ConversationMission.OneToOneConversationCharacter.HeroObject.GetTraitLevel(DefaultTraits.Generosity);
			int traitLevel2 = ConversationMission.OneToOneConversationCharacter.HeroObject.GetTraitLevel(DefaultTraits.Mercy);
			return traitLevel + traitLevel2 == 0;
		}
		return false;
	}

	private bool DialogCondition4()
	{
		if (GeneralNotableDialogCondition())
		{
			int traitLevel = ConversationMission.OneToOneConversationCharacter.HeroObject.GetTraitLevel(DefaultTraits.Generosity);
			int traitLevel2 = ConversationMission.OneToOneConversationCharacter.HeroObject.GetTraitLevel(DefaultTraits.Mercy);
			return traitLevel + traitLevel2 < 0;
		}
		return false;
	}

	private bool GeneralNotableDialogCondition()
	{
		if (!_contactSet)
		{
			if (((BasicCharacterObject)ConversationMission.OneToOneConversationCharacter).IsHero)
			{
				return ConversationMission.OneToOneConversationCharacter.HeroObject.IsNotable;
			}
			return false;
		}
		return false;
	}

	private bool BlackSmithCondition()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (!_contactSet)
		{
			return (int)ConversationMission.OneToOneConversationCharacter.Occupation == 28;
		}
		return false;
	}

	private DialogFlow GetThugDialogFlow()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		return DialogFlow.CreateDialogFlow("start", 101).NpcLine(new TextObject("{=3buSOoHl}Get lost!", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(ThugConversationCondition))
			.CloseDialog();
	}

	private bool ThugConversationCondition()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		Agent oneToOneConversationAgent = ConversationMission.OneToOneConversationAgent;
		AgentNavigator agentNavigator = ((oneToOneConversationAgent != null) ? oneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator : null);
		if (_willSetUpContact && !_contactSet && agentNavigator?.MemberOfAlley != null && (int)agentNavigator.MemberOfAlley.State == 1)
		{
			return ((SettlementArea)agentNavigator.MemberOfAlley).Owner != Hero.MainHero;
		}
		return false;
	}

	private DialogFlow FailedDialogFlow()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		return DialogFlow.CreateDialogFlow("start", 101).NpcLine(new TextObject("{=91x5mjXa}Hey! You thought you could fool us, wearing that nonsense? To the dungeons you go, until we decide what to do with you!", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _defaultDisguiseAgents.Contains(ConversationMission.OneToOneConversationAgent)))
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += mission_failed_through_dialog_consequence;
			})
			.CloseDialog();
	}

	private void mission_failed_through_dialog_consequence()
	{
		_playerWillBeTakenPrisoner = true;
		Campaign.Current.GameMenuManager.SetNextMenu("menu_captivity_castle_taken_prisoner");
		Mission.Current.EndMission();
	}

	private DialogFlow GetContactDialogFlow()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0037: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_005c: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		return DialogFlow.CreateDialogFlow("start", 101).BeginNpcOptions("start", false).NpcOption(new TextObject("{=fT57TeqJ}You can go about your business now and we don't need to see each other ever again.", (Dictionary<string, object>)null), (OnConditionDelegate)(() => _contactSet && ConversationMission.OneToOneConversationAgent == _contactAgent), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.NpcOption(new TextObject("{=mdJapWRd}Right… Something tells me that you're not just an ordinary beggar. Look, I can help you lie low and stay out of sight for a bit, if that's what you need.", (Dictionary<string, object>)null), (OnConditionDelegate)(() => !_contactSet && ConversationMission.OneToOneConversationAgent == _contactAgent), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=toHJ01dX}What do you want in exchange?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=G3sImCKI}Nothing... I suspect your good favor is worth more than the few coppers I normally charge for my services.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=rshplAOt}Hmm.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("start")
			.PlayerOption(new TextObject("{=QuNcB0dA}Very well. I accept.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=bNRfxIy7}Very good. I think it will be safe for you to go about your business in a short time. The less time we spend talking together the better, so you might not see me again.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Expected O, but got Unknown
				_contactSet = true;
				((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>()?.DisableStartConversation(isDisabled: false);
				Campaign.Current.GetCampaignBehavior<EncounterGameMenuBehavior>().AddCurrentSettlementAsAlreadySneakedIn();
				MBInformationManager.AddQuickInformation(new TextObject("{=MZJhzaUJ}You now have a contact in this town.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_update");
				TogglePassages(isActive: true);
			})
			.CloseDialog()
			.EndNpcOptions();
	}

	private void OnLocationCharacterAgentSpawned(LocationCharacterAgentSpawnedMissionEvent eventData)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		if (((BasicCharacterObject)eventData.LocationCharacter.Character).IsHero && eventData.LocationCharacter.Character.HeroObject.IsPlayerCompanion)
		{
			_agentsToBeRemoved.Add(eventData.Agent);
		}
		else if ((int)eventData.LocationCharacter.Character.Occupation == 26 || eventData.LocationCharacter.Character.Culture.FemaleDancer == eventData.LocationCharacter.Character)
		{
			_agentsToBeRemoved.Add(eventData.Agent);
		}
		else if (eventData.LocationCharacter.MemberOfAlley != null && ((SettlementArea)eventData.LocationCharacter.MemberOfAlley).Owner == Hero.MainHero)
		{
			_agentsToBeRemoved.Add(eventData.Agent);
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (!_firstTickPassed)
		{
			_firstTickPassed = true;
			return;
		}
		if (!_isBehaviorInitialized)
		{
			InitializeMissionBehavior();
			_isBehaviorInitialized = true;
			return;
		}
		_suspiciousAgentsThisFrame.Clear();
		if (Agent.Main != null)
		{
			PlayerSuspiciousLevel += GetPlayerSuspiciousFactor(dt) * dt * Campaign.Current.Models.DifficultyModel.GetDisguiseDifficultyMultiplier();
			PlayerSuspiciousLevel = MathF.Clamp(PlayerSuspiciousLevel, 0f, 1f);
			if (PlayerSuspiciousLevel >= 0.95f)
			{
				if (!_disguiseAgentsStealthModeIsOn)
				{
					SetStealthModeToDisguiseAgents(isActive: true);
				}
				foreach (Agent item in _suspiciousAgentsThisFrame)
				{
					if (_agentAlarmedBehaviorCache.TryGetValue(item, out var value) && value.AlarmFactor < 0.25f)
					{
						value.AddAlarmFactor(0.25f - value.AlarmFactor, Agent.Main.GetWorldPosition());
					}
				}
			}
			else if (_disguiseAgentsStealthModeIsOn)
			{
				SetStealthModeToDisguiseAgents(isActive: false);
			}
			CheckCaughtConversationActivation();
		}
		else if (Agent.Main == null || !Agent.Main.IsActive())
		{
			if (_isAgentDeadTimer == null)
			{
				_isAgentDeadTimer = new Timer(Mission.Current.CurrentTime, 5f, true);
			}
			if (_isAgentDeadTimer.Check(Mission.Current.CurrentTime))
			{
				Mission.Current.NextCheckTimeEndMission = 0f;
				Mission.Current.EndMission();
			}
		}
		else if (_isAgentDeadTimer != null)
		{
			_isAgentDeadTimer = null;
		}
	}

	private void CheckCaughtConversationActivation()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (Campaign.Current.ConversationManager.IsConversationFlowActive)
		{
			return;
		}
		Vec3 position;
		foreach (Agent officerAgent in _officerAgents)
		{
			if (officerAgent.IsAlarmed())
			{
				position = officerAgent.Position;
				if (((Vec3)(ref position)).DistanceSquared(Agent.Main.Position) < 9f)
				{
					ConversationMission.StartConversationWithAgent(officerAgent);
					break;
				}
			}
		}
		if (!Campaign.Current.ConversationManager.IsConversationFlowActive)
		{
			foreach (Agent defaultDisguiseAgent in _defaultDisguiseAgents)
			{
				if (defaultDisguiseAgent.IsAlarmed())
				{
					position = defaultDisguiseAgent.Position;
					if (((Vec3)(ref position)).DistanceSquared(Agent.Main.Position) < 9f)
					{
						ConversationMission.StartConversationWithAgent(defaultDisguiseAgent);
						break;
					}
				}
			}
		}
		if (!Campaign.Current.ConversationManager.IsConversationFlowActive)
		{
			return;
		}
		SetStealthModeToDisguiseAgents(isActive: false);
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			item.SetAlarmState((AIStateFlag)0);
			item.SetAgentFlags((AgentFlag)(item.GetAgentFlags() & -65537));
		}
	}

	public ShadowingAgentOffenseInfo GetAgentOffenseInfo(Agent agent)
	{
		if (agent == null)
		{
			return null;
		}
		if (!_disguiseAgentOffenseInfos.TryGetValue(agent, out var value))
		{
			return null;
		}
		return value;
	}

	private float GetPlayerSuspiciousFactor(float dt)
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Invalid comparison between Unknown and I4
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Invalid comparison between Unknown and I4
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected I4, but got Unknown
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool hasVisualOnCorpse;
		foreach (Agent officerAgent in _officerAgents)
		{
			StealthOffenseTypes offenseType = StealthOffenseTypes.None;
			if (CanAgentSeeAgent(officerAgent, Agent.Main, (MBReadOnlyList<GameEntity>)(object)_stealthIndoorLightingAreas, out hasVisualOnCorpse))
			{
				num++;
				num2++;
				offenseType = StealthOffenseTypes.IsVisible;
				_suspiciousAgentsThisFrame.Add(officerAgent);
			}
			if (IsInOfficerPersonalZone(officerAgent))
			{
				num3++;
				offenseType = StealthOffenseTypes.IsInPersonalZone;
			}
			if (_disguiseAgentOffenseInfos.TryGetValue(officerAgent, out var value))
			{
				value.SetOffenseType(offenseType);
			}
		}
		foreach (Agent defaultDisguiseAgent in _defaultDisguiseAgents)
		{
			StealthOffenseTypes offenseType2 = StealthOffenseTypes.None;
			if (CanAgentSeeAgent(defaultDisguiseAgent, Agent.Main, (MBReadOnlyList<GameEntity>)(object)_stealthIndoorLightingAreas, out hasVisualOnCorpse))
			{
				num++;
				offenseType2 = StealthOffenseTypes.IsVisible;
				_suspiciousAgentsThisFrame.Add(defaultDisguiseAgent);
			}
			if (IsInDefaultAgentPersonalZone(defaultDisguiseAgent))
			{
				num3 += 15;
				offenseType2 = StealthOffenseTypes.IsInPersonalZone;
			}
			if (_disguiseAgentOffenseInfos.TryGetValue(defaultDisguiseAgent, out var value2))
			{
				value2.SetOffenseType(offenseType2);
			}
		}
		float num4 = MathF.Sqrt((float)(num3 * 2 + num + num2));
		bool flag = num4 <= 0f;
		Vec2 movementVelocity = Agent.Main.MovementVelocity;
		bool flag2 = ((Vec2)(ref movementVelocity)).Length > 1E-05f;
		bool flag3 = Agent.Main.IsUsingGameObject || ConversationMission.OneToOneConversationAgent != null;
		bool crouchMode = Agent.Main.CrouchMode;
		bool walkMode = Agent.Main.WalkMode;
		bool flag4 = Agent.Main.IsAbleToUseMachine();
		bool flag5 = (int)Agent.Main.GetPrimaryWieldedItemIndex() != -1 || (int)Agent.Main.GetOffhandWieldedItemIndex() != -1;
		bool flag6 = MBMath.IsBetween((int)Agent.Main.GetCurrentActionType(0), 19, 23);
		float num5 = 0f;
		bool flag7 = false;
		if (!flag3)
		{
			flag7 = CalculateErraticMovementSuspiciousValue(dt);
			if (!flag)
			{
				num5 = CalculateCircularMovementSuspiciousValue(dt);
			}
		}
		float num6 = ((!(num4 > 0f)) ? (-0.07f) : ((num3 > 0 && !flag3) ? 0.13f : (flag6 ? 0.75f : (flag5 ? 0.55f : ((!flag4) ? 0.2f : (crouchMode ? 0.15f : ((num2 > 0 && !flag3) ? 0.040000003f : ((!walkMode && flag2 && !flag3) ? 0.3f : ((flag7 && _cumulativePositionAndRotationDifference > 0.2f) ? (0.1f * _cumulativePositionAndRotationDifference) : ((num5 > 0f) ? (0.1f * num5) : ((!flag2 && !flag && !flag3) ? 0.1f : (flag3 ? (-0.07f) : ((!flag || flag2) ? (-0.049999997f) : (-0.07f))))))))))))));
		if (num4 > 0f)
		{
			num6 *= num4;
		}
		if (num6 > 0.05f)
		{
			_lastSuspiciousTimer.Reset();
		}
		else if (!_lastSuspiciousTimer.Check(false))
		{
			num6 = 0f;
		}
		if (num6 < 0f && (_defaultDisguiseAgents.Any((Agent x) => !x.IsAlarmStateNormal()) || _officerAgents.Any((Agent x) => !x.IsAlarmStateNormal())))
		{
			num6 = 0f;
		}
		return num6;
	}

	private float CalculateCircularMovementSuspiciousValue(float dt)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = Agent.Main.Position;
		_averagePlayerPosition = Vec3.Lerp(_averagePlayerPosition, position, dt * 0.6f);
		return Math.Max(0f, (4f - ((Vec3)(ref _averagePlayerPosition)).DistanceSquared(position)) / 4f);
	}

	public bool IsAgentInDetectionRadius(Agent offenderAgent, Agent detectorAgent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = offenderAgent.Position;
		return ((Vec3)(ref position)).DistanceSquared(detectorAgent.Position) < 4f;
	}

	private bool CalculateErraticMovementSuspiciousValue(float dt)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = Agent.Main.Position;
		Vec2 asVec = ((Vec3)(ref position)).AsVec2;
		bool result = false;
		float num = MathF.Atan2(((Vec2)(ref asVec)).Y - ((Vec2)(ref _lastFramePlayerPosition)).Y, ((Vec2)(ref asVec)).X - ((Vec2)(ref _lastFramePlayerPosition)).X);
		if (num > MathF.PI)
		{
			num = MathF.PI * 2f - num;
		}
		num /= MathF.PI;
		float num2 = MathF.Sqrt(MathF.Abs(_angleDifferenceBetweenCurrentAndLastPositionOfPlayer - num) * 0.5f);
		if (num2 > 0.02f)
		{
			_cumulativePositionAndRotationDifference += _cumulativePositionAndRotationDifference / 1f * num2;
			result = true;
		}
		_angleDifferenceBetweenCurrentAndLastPositionOfPlayer = num;
		_lastFramePlayerPosition = asVec;
		_cumulativePositionAndRotationDifference = MathF.Clamp(_cumulativePositionAndRotationDifference - 2f * dt, 0.2f, 0.6f);
		return result;
	}

	public override InquiryData OnEndMissionRequest(out bool canPlayerLeave)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		canPlayerLeave = PlayerSuspiciousLevel < 0.25f;
		if (!canPlayerLeave)
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=9w6zmKQ1}You can't sneak out while people are suspicious!", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		return null;
	}

	private bool IsInOfficerPersonalZone(Agent agent)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = Agent.Main.Position;
		return ((Vec3)(ref position)).DistanceSquared(agent.Position) < 12.25f;
	}

	private bool IsInDefaultAgentPersonalZone(Agent agent)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = Agent.Main.Position;
		return ((Vec3)(ref position)).DistanceSquared(agent.Position) < 0f;
	}

	private bool CanAgentSeeAgent(Agent agent1, Agent agent2, MBReadOnlyList<GameEntity> stealthIndoorLightingAreas, out bool hasVisualOnCorpse)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val;
		Vec3 val2;
		if (!agent1.IsHuman || !agent1.AgentVisuals.IsValid())
		{
			val = agent1.LookDirection;
		}
		else
		{
			MatrixFrame frame = agent1.Frame;
			ref Mat3 rotation = ref frame.rotation;
			val2 = agent1.AgentVisuals.GetCurrentHeadLookDirection();
			val = ((Mat3)(ref rotation)).TransformToParent(ref val2);
		}
		Vec3 val3 = val;
		val2 = Vec3.CrossProduct(Vec3.Up, val3);
		val3 = ((Vec3)(ref val3)).RotateAboutAnArbitraryVector(((Vec3)(ref val2)).NormalizedCopy(), 0.2f);
		bool hasVisualOnEnemy = false;
		hasVisualOnCorpse = false;
		_agentAlarmedBehaviorCache[agent1].GetVisualFactor(val3, agent2, stealthIndoorLightingAreas, ref hasVisualOnCorpse, ref hasVisualOnEnemy);
		return hasVisualOnEnemy;
	}

	public EventControlFlag OnCollectPlayerEventControlFlags()
	{
		if (!_firstEventControlTickPassed)
		{
			_firstEventControlTickPassed = true;
			return (EventControlFlag)2048;
		}
		return (EventControlFlag)0;
	}
}
