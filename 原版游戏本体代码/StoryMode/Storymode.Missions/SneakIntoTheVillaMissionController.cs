using System;
using System.Collections.Generic;
using SandBox;
using SandBox.Missions;
using SandBox.Missions.AgentBehaviors;
using SandBox.Objects;
using SandBox.Objects.Usables;
using StoryMode.Quests.TutorialPhase;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Storymode.Missions;

public class SneakIntoTheVillaMissionController : MissionLogic
{
	public enum MissionState
	{
		Start,
		Crouch,
		WalkSlow,
		HideInBushes,
		HideInBushesEnd,
		Distraction,
		DarkZone,
		DarkZoneEnd,
		StealthKill,
		HideCorpse,
		End
	}

	private Dictionary<MissionState, VolumeBox> _volumeBoxes = new Dictionary<MissionState, VolumeBox>();

	private const string FirstDoorId = "doors_before_convo";

	private const string SecondDoorId = "doors_after_convo";

	private const string DistractionAgentSpawnPointId = "sp_agent_distraction";

	private const string StealthKillAgentSpawnPointId = "sp_agent_stealth_kill";

	private const string HeadmanSpawnPoint = "sp_captive";

	private VillagersInNeed _talkToVillagersQuest;

	private MissionTimer _missionEndTimer;

	private Agent _distractionTargetAgent;

	private Agent _stealthKillTargetAgent;

	private bool _isStealthAttackComplete;

	public bool AreVisualsDirty;

	public static SneakIntoTheVillaMissionController Instance { get; private set; }

	public MissionState State { get; private set; }

	public Agent HeadmanAgent { get; private set; }

	public override void OnMissionTick(float dt)
	{
		if (_missionEndTimer != null && _missionEndTimer.Check(false))
		{
			((MissionBehavior)this).Mission.EndMission();
			_missionEndTimer = null;
		}
		CheckTriggers();
	}

	public override void OnCreated()
	{
		Instance = this;
		((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment = true;
		_talkToVillagersQuest = (VillagersInNeed)(object)LinQuick.FirstOrDefaultQ<QuestBase>((List<QuestBase>)(object)Campaign.Current.QuestManager.Quests, (Func<QuestBase, bool>)((QuestBase x) => x is VillagersInNeed));
	}

	public override void AfterStart()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)4, true);
		((MissionBehavior)this).Mission.IsInventoryAccessible = false;
		((MissionBehavior)this).Mission.IsQuestScreenAccessible = true;
		MissionHelper.SpawnPlayer(false, true, false, false, "");
		MBEquipmentRoster val = MBObjectManager.Instance.GetObject<MBEquipmentRoster>("stealth_tutorial_set_player");
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(val.DefaultEquipment);
		SpawnStealthAgents();
		SpawnHeadman();
		InitializeVolumeBoxes();
		((MissionBehavior)this).Mission.GetMissionBehavior<StealthFailCounterMissionLogic>().SetFailTexts((TextObject)null, new TextObject("{=eJ3iAJ8U}You alerted the bandits. The camp erupts in confusion, but in the darkness you are able to slip away. You watch from a distance as the chaos and noise die down, and you sense that it won’t be long before this ill-disciplined gang relaxes their guard, giving you another chance. When you are ready, you can return to Tevea and try again.", (Dictionary<string, object>)null));
		Game.Current.EventManager.RegisterEvent<OnStealthMissionCounterFailedEvent>((Action<OnStealthMissionCounterFailedEvent>)OnCaughtInStealthZone);
	}

	public static bool IsStealthTutorialReadyForActivation(MissionState missionState)
	{
		if (Mission.Current != null && Instance != null)
		{
			if (missionState != MissionState.Start)
			{
				return missionState <= Instance.State;
			}
			return true;
		}
		return false;
	}

	public static bool IsStealthTutorialReadyForCompletion(MissionState missionState)
	{
		if (Instance != null && missionState < MissionState.End)
		{
			return Instance.State > missionState;
		}
		return false;
	}

	public override void OnRemoveBehavior()
	{
		((MissionBehavior)this).OnRemoveBehavior();
		Instance = null;
	}

	protected override void OnEndMission()
	{
		Game.Current.EventManager.UnregisterEvent<OnStealthMissionCounterFailedEvent>((Action<OnStealthMissionCounterFailedEvent>)OnCaughtInStealthZone);
	}

	private void OnCaughtInStealthZone(OnStealthMissionCounterFailedEvent stealthFailedEvent)
	{
		_talkToVillagersQuest.OnRescueMissionFailed();
	}

	private void OnMainAgentIsWounded()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		_talkToVillagersQuest.OnRescueMissionFailed();
		_missionEndTimer = new MissionTimer(2f);
	}

	private void ShowMissionFailedPopup()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		TextObject val = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=wQbfWNZO}Mission Failed!", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=45IBacqS}You are knocked to the ground, but in the confusion and darkness you are able to crawl away. You watch from a distance as the chaos and noise in the hideout die down, and you sense that it won’t be long before this ill-disciplined gang relaxes their guard, giving you another chance. When you are ready, you can return to Tevea and try again.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)val).ToString(), (string)null, (Action)delegate
		{
			OnMainAgentIsWounded();
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (affectedAgent.IsMainAgent)
		{
			ShowMissionFailedPopup();
		}
	}

	public void OnAfterTalkingToPrisoner()
	{
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("doors_before_convo");
		GameEntity obj = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("doors_after_convo");
		val.SetVisibilityExcludeParents(false);
		obj.SetVisibilityExcludeParents(true);
		List<GameEntity> list = new List<GameEntity>();
		((MissionBehavior)this).Mission.Scene.GetAllEntitiesWithScriptComponent<Passage>(ref list);
		foreach (GameEntity item in list)
		{
			Passage firstScriptOfType = item.GetFirstScriptOfType<Passage>();
			((MissionObject)firstScriptOfType).SetEnabled(false);
			((UsableMissionObject)((UsableMachine)firstScriptOfType).PilotStandingPoint).IsDeactivated = false;
		}
		AreVisualsDirty = true;
	}

	public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		PassageUsePoint val;
		if (userAgent.IsMainAgent && (val = (PassageUsePoint)(object)((usedObject is PassageUsePoint) ? usedObject : null)) != null && val.IsMissionExit)
		{
			_talkToVillagersQuest.OnHeadmanRescued();
		}
	}

	private void SpawnHeadman()
	{
		GameEntity spawnPoint = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_captive");
		HeadmanAgent = SpawnAgent(_talkToVillagersQuest.Headman, spawnPoint, Team.Invalid);
	}

	private void SpawnStealthAgents()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>("mountain_bandits_raider");
		List<GameEntity> list = new List<GameEntity>();
		((MissionBehavior)this).Mission.Scene.GetAllEntitiesWithScriptComponent<DynamicPatrolAreaParent>(ref list);
		MBActionSet actionSet = MBGlobals.GetActionSet("as_human_hideout_bandit");
		foreach (GameEntity item in list)
		{
			foreach (GameEntity child in item.GetChildren())
			{
				PatrolPoint firstScriptOfType = child.GetChild(0).GetFirstScriptOfType<PatrolPoint>();
				if (firstScriptOfType.SpawnGroupTag == "stealth_agent")
				{
					Agent val2 = SpawnAgent(val, child, ((MissionBehavior)this).Mission.PlayerEnemyTeam);
					AgentNavigator obj = val2.GetComponent<CampaignAgentComponent>().CreateAgentNavigator();
					SandBoxManager.Instance.AgentBehaviorManager.AddStealthAgentBehaviors((IAgent)(object)val2);
					AnimationSystemData val3 = MonsterExtensions.FillAnimationSystemData(val2.Monster, actionSet, ((BasicCharacterObject)val).GetStepSize(), false);
					val2.SetActionSet(ref val3);
					AgentFlag agentFlags = val2.GetAgentFlags();
					val2.SetAgentFlags((AgentFlag)(agentFlags | 0x10000));
					((AgentBehaviorGroup)obj.GetBehaviorGroup<DailyBehaviorGroup>()).GetBehavior<PatrolAgentBehavior>().SetDynamicPatrolArea(item);
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)firstScriptOfType).GameEntity;
					if (((WeakGameEntity)(ref gameEntity)).HasTag("sp_agent_distraction"))
					{
						_distractionTargetAgent = val2;
					}
					gameEntity = ((ScriptComponentBehavior)firstScriptOfType).GameEntity;
					if (((WeakGameEntity)(ref gameEntity)).HasTag("sp_agent_stealth_kill"))
					{
						_stealthKillTargetAgent = val2;
					}
				}
			}
		}
	}

	private void CheckTriggers()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (State >= MissionState.End || Agent.Main == null)
		{
			return;
		}
		for (MissionState missionState = State + 1; missionState < MissionState.End; missionState++)
		{
			if (_volumeBoxes[missionState].IsPointIn(Agent.Main.Position))
			{
				State = missionState;
				break;
			}
		}
	}

	public bool IsTargetAgentDistracted()
	{
		if (State == MissionState.Distraction)
		{
			if (_distractionTargetAgent != null && !_distractionTargetAgent.IsCautious())
			{
				return _distractionTargetAgent.IsAlarmed();
			}
			return true;
		}
		return false;
	}

	public bool IsTargetAgentKilled()
	{
		if (State == MissionState.StealthKill)
		{
			if (!_isStealthAttackComplete)
			{
				return _stealthKillTargetAgent == null;
			}
			return true;
		}
		return false;
	}

	public bool IsMainAgentDraggingTargetBody()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		if (_stealthKillTargetAgent != null && _stealthKillTargetAgent.IsAddedAsCorpse() && Agent.Main != null && Agent.Main.IsActive())
		{
			return (Agent.Main.GetScriptedFlags() & 0x400) == 1024;
		}
		return false;
	}

	public override void OnMeleeHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!_isStealthAttackComplete && _stealthKillTargetAgent != null && collisionData.IsSneakAttack && victim == _stealthKillTargetAgent)
		{
			_isStealthAttackComplete = true;
		}
	}

	private void InitializeVolumeBoxes()
	{
		_volumeBoxes = new Dictionary<MissionState, VolumeBox>();
		_volumeBoxes[MissionState.Crouch] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("trigger_volume_crouch").GetFirstScriptOfType<VolumeBox>();
		_volumeBoxes[MissionState.WalkSlow] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("trigger_volume_walk_slowly").GetFirstScriptOfType<VolumeBox>();
		_volumeBoxes[MissionState.HideInBushes] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("trigger_volume_stealthbox").GetFirstScriptOfType<VolumeBox>();
		_volumeBoxes[MissionState.HideInBushesEnd] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("end_trigger_stealthbox").GetFirstScriptOfType<VolumeBox>();
		_volumeBoxes[MissionState.Distraction] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("trigger_volume_distraction").GetFirstScriptOfType<VolumeBox>();
		_volumeBoxes[MissionState.DarkZone] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("trigger_volume_dark_zone").GetFirstScriptOfType<VolumeBox>();
		_volumeBoxes[MissionState.DarkZoneEnd] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("end_trigger_darkness").GetFirstScriptOfType<VolumeBox>();
		_volumeBoxes[MissionState.StealthKill] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("trigger_volume_stealth_kill").GetFirstScriptOfType<VolumeBox>();
		_volumeBoxes[MissionState.HideCorpse] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("trigger_volume_hide_corpse").GetFirstScriptOfType<VolumeBox>();
		_volumeBoxes[MissionState.End] = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("trigger_volume_passage").GetFirstScriptOfType<VolumeBox>();
	}

	private Agent SpawnAgent(CharacterObject character, GameEntity spawnPoint, Team team)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		MatrixFrame globalFrame = spawnPoint.GetGlobalFrame();
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)character).NoHorses(true).InitialPosition(ref globalFrame.origin);
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		AgentBuildData val2 = obj.InitialDirection(ref val).CivilianEquipment(true).Team(team)
			.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)character, -1, (Banner)null, default(UniqueTroopDescriptor)));
		return Mission.Current.SpawnAgent(val2, false);
	}
}
