using System;
using System.Collections.Generic;
using Helpers;
using SandBox.Conversation;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class CampaignMissionComponent : MissionLogic, ICampaignMission
{
	private class AgentConversationState
	{
		private StackArray2Bool _actionAtChannelModified;

		public Agent Agent { get; private set; }

		public AgentConversationState(Agent agent)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Agent = agent;
			_actionAtChannelModified = default(StackArray2Bool);
			((StackArray2Bool)(ref _actionAtChannelModified))[0] = false;
			((StackArray2Bool)(ref _actionAtChannelModified))[1] = false;
		}

		public bool IsChannelModified(int channelNo)
		{
			return ((StackArray2Bool)(ref _actionAtChannelModified))[channelNo];
		}

		public void SetChannelModified(int channelNo)
		{
			((StackArray2Bool)(ref _actionAtChannelModified))[channelNo] = true;
		}
	}

	private MissionState _state;

	private SoundEvent _soundEvent;

	private Agent _currentAgent;

	private bool _isMainAgentAnimationSet;

	private readonly Dictionary<Agent, int> _agentSoundEvents = new Dictionary<Agent, int>();

	private readonly List<AgentConversationState> _conversationAgents = new List<AgentConversationState>();

	public GameState State => (GameState)(object)_state;

	public IMissionTroopSupplier AgentSupplier { get; set; }

	public Location Location { get; set; }

	public Alley LastVisitedAlley { get; set; }

	MissionMode ICampaignMission.Mode => ((MissionBehavior)this).Mission.Mode;

	void ICampaignMission.SetMissionMode(MissionMode newMode, bool atStart)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).Mission.SetMissionMode(newMode, atStart);
	}

	public override void OnAgentCreated(Agent agent)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		((MissionBehavior)this).OnAgentCreated(agent);
		agent.AddComponent((AgentComponent)(object)new CampaignAgentComponent(agent));
		CharacterObject val = (CharacterObject)agent.Character;
		if (((val != null) ? val.HeroObject : null) != null && val.HeroObject.IsPlayerCompanion)
		{
			agent.AgentRole = new TextObject("{=kPTp6TPT}({AGENT_ROLE})", (Dictionary<string, object>)null);
			agent.AgentRole.SetTextVariable("AGENT_ROLE", GameTexts.FindText("str_companion", (string)null));
		}
	}

	public override void OnPreDisplayMissionTick(float dt)
	{
		((MissionBehavior)this).OnPreDisplayMissionTick(dt);
		if (_soundEvent != null && !_soundEvent.IsPlaying())
		{
			RemovePreviousAgentsSoundEvent();
			_soundEvent.Stop();
			_soundEvent = null;
		}
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (Campaign.Current != null)
		{
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).MissionTick(dt);
		}
	}

	protected override void OnObjectDisabled(DestructableComponent missionObject)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)missionObject).GameEntity;
		SiegeWeapon firstScriptOfType = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<SiegeWeapon>();
		if (firstScriptOfType != null && Campaign.Current != null && (int)Campaign.Current.GameMode == 1)
		{
			CampaignSiegeStateHandler missionBehavior = Mission.Current.GetMissionBehavior<CampaignSiegeStateHandler>();
			if (missionBehavior != null && missionBehavior.IsSallyOut)
			{
				ISiegeEventSide siegeEventSide = missionBehavior.Settlement.SiegeEvent.GetSiegeEventSide(firstScriptOfType.Side);
				siegeEventSide.SiegeEvent.BreakSiegeEngine(siegeEventSide, firstScriptOfType.GetSiegeEngineType());
			}
		}
		((MissionBehavior)this).OnObjectDisabled(missionObject);
	}

	public override void EarlyStart()
	{
		ref MissionState state = ref _state;
		GameState activeState = Game.Current.GameStateManager.ActiveState;
		state = (MissionState)(object)((activeState is MissionState) ? activeState : null);
	}

	public override void OnCreated()
	{
		CampaignMission.Current = (ICampaignMission)(object)this;
		_isMainAgentAnimationSet = false;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnMissionStarted((IMission)(object)((MissionBehavior)this).Mission);
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnAfterMissionStarted((IMission)(object)((MissionBehavior)this).Mission);
	}

	private static void SimulateRunningAwayAgents()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			PartyBase ownerParty = item.GetComponent<CampaignAgentComponent>().OwnerParty;
			if (ownerParty != null && !item.IsHero && item.IsRunningAway && MBRandom.RandomFloat < 0.5f)
			{
				CharacterObject val = (CharacterObject)item.Character;
				ownerParty.MemberRoster.AddToCounts(val, -1, false, 0, 0, true, -1);
			}
		}
	}

	public override void OnMissionResultReady(MissionResult missionResult)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Campaign.Current.GameMode == 1 && PlayerEncounter.IsActive && PlayerEncounter.Battle != null)
		{
			if (missionResult.PlayerVictory)
			{
				PlayerEncounter.SetPlayerVictorious();
			}
			else if ((int)missionResult.BattleState == 3)
			{
				PlayerEncounter.SetPlayerSiegeContinueWithDefenderPullBack();
			}
			MissionResult missionResult2 = ((MissionBehavior)this).Mission.MissionResult;
			PlayerEncounter.CampaignBattleResult = CampaignBattleResult.GetResult((BattleState)((missionResult2 != null) ? ((int)missionResult2.BattleState) : 0), missionResult.EnemyRetreated);
		}
	}

	protected override void OnEndMission()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		if ((int)Campaign.Current.GameMode == 1)
		{
			if (PlayerEncounter.Battle != null && (PlayerEncounter.Battle.IsSiegeAssault || PlayerEncounter.Battle.IsSiegeAmbush) && ((int)Mission.Current.MissionTeamAIType == 2 || (int)Mission.Current.MissionTeamAIType == 3))
			{
				IEnumerable<IMissionSiegeWeapon> enumerable = default(IEnumerable<IMissionSiegeWeapon>);
				IEnumerable<IMissionSiegeWeapon> enumerable2 = default(IEnumerable<IMissionSiegeWeapon>);
				Mission.Current.GetMissionBehavior<MissionSiegeEnginesLogic>().GetMissionSiegeWeapons(ref enumerable, ref enumerable2);
				PlayerEncounter.Battle.GetLeaderParty((BattleSideEnum)1).SiegeEvent.SetSiegeEngineStatesAfterSiegeMission(enumerable2, enumerable);
			}
			if (_soundEvent != null)
			{
				RemovePreviousAgentsSoundEvent();
				_soundEvent.Stop();
				_soundEvent = null;
			}
		}
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnMissionEnded((IMission)(object)((MissionBehavior)this).Mission);
		CampaignMission.Current = null;
	}

	void ICampaignMission.OnCloseEncounterMenu()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)((MissionBehavior)this).Mission.Mode == 1)
		{
			Campaign.Current.ConversationManager.EndConversation();
			if (Game.Current.GameStateManager.ActiveState is MissionState)
			{
				Game.Current.GameStateManager.PopState(0);
			}
		}
	}

	bool ICampaignMission.AgentLookingAtAgent(IAgent agent1, IAgent agent2)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0017: Expected O, but got Unknown
		return ((MissionBehavior)this).Mission.AgentLookingAtAgent((Agent)agent1, (Agent)agent2);
	}

	void ICampaignMission.OnCharacterLocationChanged(LocationCharacter locationCharacter, Location fromLocation, Location toLocation)
	{
		MissionAgentHandler missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>();
		if (toLocation == null)
		{
			missionBehavior.FadeoutExitingLocationCharacter(locationCharacter);
		}
		else
		{
			missionBehavior.SpawnEnteringLocationCharacter(locationCharacter, fromLocation);
		}
	}

	void ICampaignMission.OnProcessSentence()
	{
	}

	void ICampaignMission.OnConversationContinue()
	{
	}

	bool ICampaignMission.CheckIfAgentCanFollow(IAgent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		AgentNavigator agentNavigator = ((Agent)agent).GetComponent<CampaignAgentComponent>().AgentNavigator;
		if (agentNavigator != null)
		{
			DailyBehaviorGroup behaviorGroup = agentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
			if (behaviorGroup != null)
			{
				return behaviorGroup.GetBehavior<FollowAgentBehavior>() == null;
			}
			return false;
		}
		return false;
	}

	void ICampaignMission.AddAgentFollowing(IAgent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		Agent val = (Agent)agent;
		if (val.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
		{
			DailyBehaviorGroup behaviorGroup = val.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
			behaviorGroup.AddBehavior<FollowAgentBehavior>();
			behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
		}
	}

	bool ICampaignMission.CheckIfAgentCanUnFollow(IAgent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		Agent val = (Agent)agent;
		if (val.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
		{
			DailyBehaviorGroup behaviorGroup = val.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
			if (behaviorGroup != null)
			{
				return behaviorGroup.GetBehavior<FollowAgentBehavior>() != null;
			}
			return false;
		}
		return false;
	}

	void ICampaignMission.RemoveAgentFollowing(IAgent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		Agent val = (Agent)agent;
		if (val.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
		{
			val.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>().RemoveBehavior<FollowAgentBehavior>();
		}
	}

	void ICampaignMission.EndMission()
	{
		((MissionBehavior)this).Mission.EndMission();
	}

	private string GetIdleAnimationId(Agent agent, string selectedId, bool startingConversation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		ActionCodeType currentActionType = agent.GetCurrentActionType(0);
		if ((int)currentActionType == 42)
		{
			return "sit";
		}
		if ((int)currentActionType == 43)
		{
			return "sit_floor";
		}
		if ((int)currentActionType == 44)
		{
			return "sit_throne";
		}
		if (agent.MountAgent != null)
		{
			(string, ConversationAnimData) animDataForRiderAndMountAgents = GetAnimDataForRiderAndMountAgents(agent);
			SetMountAgentAnimation((IAgent)(object)agent.MountAgent, animDataForRiderAndMountAgents.Item2, startingConversation);
			return animDataForRiderAndMountAgents.Item1;
		}
		if (agent == Agent.Main)
		{
			return "normal";
		}
		if (startingConversation)
		{
			CharacterObject val = (CharacterObject)agent.Character;
			PartyBase ownerParty = agent.GetComponent<CampaignAgentComponent>().OwnerParty;
			return CharacterHelper.GetStandingBodyIdle(val, ownerParty);
		}
		return selectedId;
	}

	private (string, ConversationAnimData) GetAnimDataForRiderAndMountAgents(Agent riderAgent)
	{
		bool flag = false;
		string item = "";
		bool flag2 = false;
		ConversationAnimData item2 = null;
		foreach (KeyValuePair<string, ConversationAnimData> conversationAnim in Campaign.Current.ConversationManager.ConversationAnimationManager.ConversationAnims)
		{
			if (conversationAnim.Value != null)
			{
				if (conversationAnim.Value.FamilyType == riderAgent.MountAgent.Monster.FamilyType)
				{
					item2 = conversationAnim.Value;
					flag2 = true;
				}
				else if (conversationAnim.Value.FamilyType == riderAgent.Monster.FamilyType && conversationAnim.Value.MountFamilyType == riderAgent.MountAgent.Monster.FamilyType)
				{
					item = conversationAnim.Key;
					flag = true;
				}
				if (flag2 && flag)
				{
					break;
				}
			}
		}
		return (item, item2);
	}

	private int GetActionChannelNoForConversation(Agent agent)
	{
		if (agent.IsSitting())
		{
			return 0;
		}
		if (agent.MountAgent != null)
		{
			return 1;
		}
		return 0;
	}

	private void SetMountAgentAnimation(IAgent agent, ConversationAnimData mountAnimData, bool startingConversation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Agent agent2 = (Agent)agent;
		if (mountAnimData != null)
		{
			if (startingConversation)
			{
				_conversationAgents.Add(new AgentConversationState(agent2));
			}
			SetConversationAgentActionAtChannel(agent2, string.IsNullOrEmpty(mountAnimData.IdleAnimStart) ? ActionIndexCache.Create(mountAnimData.IdleAnimLoop) : ActionIndexCache.Create(mountAnimData.IdleAnimStart), GetActionChannelNoForConversation(agent2), setInstantly: false, forceFaceMorphRestart: false);
		}
	}

	void ICampaignMission.OnConversationStart(IAgent iAgent, bool setActionsInstantly)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		((Agent)iAgent).AgentVisuals.SetAgentLodZeroOrMax(true);
		Agent.Main.AgentVisuals.SetAgentLodZeroOrMax(true);
		if (!_isMainAgentAnimationSet)
		{
			_isMainAgentAnimationSet = true;
			StartConversationAnimations((IAgent)(object)Agent.Main, setActionsInstantly);
		}
		StartConversationAnimations(iAgent, setActionsInstantly);
	}

	private void StartConversationAnimations(IAgent iAgent, bool setActionsInstantly)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Agent val = (Agent)iAgent;
		_conversationAgents.Add(new AgentConversationState(val));
		string idleAnimationId = GetIdleAnimationId(val, "", startingConversation: true);
		string defaultFaceIdle = CharacterHelper.GetDefaultFaceIdle((CharacterObject)val.Character);
		int actionChannelNoForConversation = GetActionChannelNoForConversation(val);
		if (Campaign.Current.ConversationManager.ConversationAnimationManager.ConversationAnims.TryGetValue(idleAnimationId, out var value))
		{
			SetConversationAgentActionAtChannel(val, string.IsNullOrEmpty(value.IdleAnimStart) ? ActionIndexCache.Create(value.IdleAnimLoop) : ActionIndexCache.Create(value.IdleAnimStart), actionChannelNoForConversation, setActionsInstantly, forceFaceMorphRestart: false);
			SetFaceIdle(val, defaultFaceIdle);
		}
		if (val.IsUsingGameObject)
		{
			val.CurrentlyUsedGameObject.OnUserConversationStart();
		}
	}

	private void EndConversationAnimations(IAgent iAgent, bool ignorePriority = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		Agent val = (Agent)iAgent;
		if (val.IsHuman)
		{
			val.SetAgentFacialAnimation((FacialAnimChannel)0, "", false);
			val.SetAgentFacialAnimation((FacialAnimChannel)1, "", false);
			if (val.HasMount)
			{
				EndConversationAnimations((IAgent)(object)val.MountAgent, ignorePriority: true);
			}
		}
		int num = -1;
		int count = _conversationAgents.Count;
		for (int i = 0; i < count; i++)
		{
			AgentConversationState agentConversationState = _conversationAgents[i];
			if (agentConversationState.Agent != val)
			{
				continue;
			}
			for (int j = 0; j < 2; j++)
			{
				if (agentConversationState.IsChannelModified(j))
				{
					val.SetActionChannel(j, ref ActionIndexCache.act_none, ignorePriority, (AnimFlags)Math.Min(val.GetCurrentActionPriority(j), 73), 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				}
			}
			if (val.IsUsingGameObject)
			{
				val.CurrentlyUsedGameObject.OnUserConversationEnd();
			}
			num = i;
			break;
		}
		if (num != -1)
		{
			_conversationAgents.RemoveAt(num);
		}
	}

	void ICampaignMission.OnConversationPlay(string idleActionId, string idleFaceAnimId, string reactionId, string reactionFaceAnimId, string soundPath)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		_currentAgent = (Agent)Campaign.Current.ConversationManager.SpeakerAgent;
		RemovePreviousAgentsSoundEvent();
		StopPreviousSound();
		string idleAnimationId = GetIdleAnimationId(_currentAgent, idleActionId, startingConversation: false);
		if (!string.IsNullOrEmpty(idleAnimationId) && Campaign.Current.ConversationManager.ConversationAnimationManager.ConversationAnims.TryGetValue(idleAnimationId, out var value))
		{
			if (!string.IsNullOrEmpty(reactionId))
			{
				SetConversationAgentActionAtChannel(_currentAgent, ActionIndexCache.Create(value.Reactions[reactionId]), 0, setInstantly: false, forceFaceMorphRestart: true);
			}
			else
			{
				ActionIndexCache action = (string.IsNullOrEmpty(value.IdleAnimStart) ? ActionIndexCache.Create(value.IdleAnimLoop) : ActionIndexCache.Create(value.IdleAnimStart));
				SetConversationAgentActionAtChannel(_currentAgent, in action, GetActionChannelNoForConversation(_currentAgent), setInstantly: false, forceFaceMorphRestart: false);
			}
		}
		if (!string.IsNullOrEmpty(reactionFaceAnimId))
		{
			_currentAgent.SetAgentFacialAnimation((FacialAnimChannel)1, reactionFaceAnimId, false);
		}
		else if (!string.IsNullOrEmpty(idleFaceAnimId))
		{
			SetFaceIdle(_currentAgent, idleFaceAnimId);
		}
		else
		{
			_currentAgent.SetAgentFacialAnimation((FacialAnimChannel)0, "", false);
		}
		if (!string.IsNullOrEmpty(soundPath))
		{
			PlayConversationSoundEvent(soundPath);
		}
	}

	private string GetRhubarbXmlPathFromSoundPath(string soundPath)
	{
		return soundPath[..soundPath.LastIndexOf('.')] + ".xml";
	}

	public void PlayConversationSoundEvent(string soundPath)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = ConversationMission.CurrentSpeakerAgent.Position;
		Debug.Print("Conversation sound playing: " + soundPath + ", position: " + position, 5, (DebugColor)12, 17592186044416uL);
		_soundEvent = SoundEvent.CreateEventFromExternalFile("event:/Extra/voiceover", soundPath, Mission.Current.Scene, false, false);
		_soundEvent.SetPosition(position);
		_soundEvent.Play();
		int soundId = _soundEvent.GetSoundId();
		_agentSoundEvents.Add(_currentAgent, soundId);
		string rhubarbXmlPathFromSoundPath = GetRhubarbXmlPathFromSoundPath(soundPath);
		_currentAgent.AgentVisuals.StartRhubarbRecord(rhubarbXmlPathFromSoundPath, soundId);
	}

	private void StopPreviousSound()
	{
		if (_soundEvent != null)
		{
			_soundEvent.Stop();
			_soundEvent = null;
		}
	}

	private void RemovePreviousAgentsSoundEvent()
	{
		if (_soundEvent == null || !_agentSoundEvents.ContainsValue(_soundEvent.GetSoundId()))
		{
			return;
		}
		Agent val = null;
		foreach (KeyValuePair<Agent, int> agentSoundEvent in _agentSoundEvents)
		{
			if (agentSoundEvent.Value == _soundEvent.GetSoundId())
			{
				val = agentSoundEvent.Key;
			}
		}
		_agentSoundEvents.Remove(val);
		val.AgentVisuals.StartRhubarbRecord("", -1);
	}

	void ICampaignMission.OnConversationEnd(IAgent iAgent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		Agent val = (Agent)iAgent;
		val.ResetLookAgent();
		val.DisableLookToPointOfInterest();
		Agent.Main.ResetLookAgent();
		Agent.Main.DisableLookToPointOfInterest();
		if (Settlement.CurrentSettlement != null && !((MissionBehavior)this).Mission.HasMissionBehavior<ConversationMissionLogic>())
		{
			val.AgentVisuals.SetAgentLodZeroOrMax(true);
			Agent.Main.AgentVisuals.SetAgentLodZeroOrMax(true);
		}
		if (_soundEvent != null)
		{
			RemovePreviousAgentsSoundEvent();
			_soundEvent.Stop();
		}
		if (_isMainAgentAnimationSet)
		{
			_isMainAgentAnimationSet = false;
			EndConversationAnimations((IAgent)(object)Agent.Main);
		}
		EndConversationAnimations(iAgent);
		_soundEvent = null;
	}

	private void SetFaceIdle(Agent agent, string idleFaceAnimId)
	{
		agent.SetAgentFacialAnimation((FacialAnimChannel)1, idleFaceAnimId, true);
	}

	private void SetConversationAgentActionAtChannel(Agent agent, in ActionIndexCache action, int channelNo, bool setInstantly, bool forceFaceMorphRestart)
	{
		agent.SetActionChannel(channelNo, ref action, false, (AnimFlags)0, 0f, 1f, setInstantly ? 0f : (-0.2f), 0.4f, 0f, false, -0.2f, 0, forceFaceMorphRestart);
		int count = _conversationAgents.Count;
		for (int i = 0; i < count; i++)
		{
			if (_conversationAgents[i].Agent == agent)
			{
				_conversationAgents[i].SetChannelModified(channelNo);
				break;
			}
		}
	}

	public void FadeOutCharacter(CharacterObject characterObject)
	{
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (item.Character != null && (object)item.Character == characterObject)
			{
				item.FadeOut(true, true);
				break;
			}
		}
	}

	public void OnGameStateChanged()
	{
		RemovePreviousAgentsSoundEvent();
		StopPreviousSound();
	}
}
