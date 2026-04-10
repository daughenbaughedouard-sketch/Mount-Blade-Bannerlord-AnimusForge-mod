using System;
using System.Collections.Generic;
using SandBox.Objects.Cinematics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics.Hideout;

public class HideoutCinematicController : MissionLogic
{
	public delegate void OnInitialFadeOutFinished(ref Agent playerAgent, ref List<Agent> playerCompanions, ref Agent bossAgent, ref List<Agent> bossCompanions, ref float placementPerturbation, ref float placementAngle);

	public delegate void OnHideoutCinematicFinished();

	public readonly struct HideoutCinematicAgentInfo
	{
		public readonly Agent Agent;

		public readonly MatrixFrame InitialFrame;

		public readonly MatrixFrame TargetFrame;

		public readonly HideoutAgentType Type;

		public HideoutCinematicAgentInfo(Agent agent, HideoutAgentType type, in MatrixFrame initialFrame, in MatrixFrame targetFrame)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			Agent = agent;
			InitialFrame = initialFrame;
			TargetFrame = targetFrame;
			Type = type;
		}

		public bool HasReachedTarget(float proximityThreshold = 0.5f)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			Vec3 position = Agent.Position;
			return ((Vec3)(ref position)).Distance(TargetFrame.origin) <= proximityThreshold;
		}
	}

	public enum HideoutCinematicState
	{
		None,
		InitialFadeOut,
		PreCinematic,
		Cinematic,
		PostCinematic,
		Completed
	}

	public enum HideoutAgentType
	{
		Player,
		Boss,
		Ally,
		Bandit
	}

	public enum HideoutPreCinematicPhase
	{
		NotStarted,
		InitializeFormations,
		StopFormations,
		InitializeAgents,
		MoveAgents,
		Completed
	}

	public enum HideoutPostCinematicPhase
	{
		NotStarted,
		MoveAgents,
		FinalizeAgents,
		Completed
	}

	private const float AgentTargetProximityThreshold = 0.5f;

	private const float AgentMaxSpeedCinematicOverride = 0.65f;

	public const string HideoutSceneEntityTag = "hideout_boss_fight";

	public const float DefaultTransitionDuration = 0.4f;

	public const float DefaultStateDuration = 0.2f;

	public const float DefaultCinematicDuration = 8f;

	public const float DefaultPlacementPerturbation = 0.25f;

	public const float DefaultPlacementAngle = MathF.PI / 15f;

	private OnInitialFadeOutFinished _initialFadeOutFinished;

	private float _cinematicDuration = 8f;

	private float _stateDuration = 0.2f;

	private float _transitionDuration = 0.4f;

	private float _remainingCinematicDuration = 8f;

	private float _remainingStateDuration = 0.2f;

	private float _remainingTransitionDuration = 0.4f;

	private List<Formation> _cachedAgentFormations;

	private List<HideoutCinematicAgentInfo> _hideoutAgentsInfo;

	private HideoutCinematicAgentInfo _bossAgentInfo;

	private HideoutCinematicAgentInfo _playerAgentInfo;

	private bool _isBehaviorInit;

	private HideoutPreCinematicPhase _preCinematicPhase;

	private HideoutPostCinematicPhase _postCinematicPhase;

	private HideoutBossFightBehavior _hideoutBossFightBehavior;

	public HideoutCinematicState State { get; private set; }

	public bool InStateTransition { get; private set; }

	public bool IsCinematicActive => State != HideoutCinematicState.None;

	public float CinematicDuration => _cinematicDuration;

	public float TransitionDuration => _transitionDuration;

	public override MissionBehaviorType BehaviorType => (MissionBehaviorType)0;

	public event Action OnCinematicFinished;

	public event Action<HideoutCinematicState> OnCinematicStateChanged;

	public event Action<HideoutCinematicState, float> OnCinematicTransition;

	public HideoutCinematicController()
	{
		State = HideoutCinematicState.None;
		InStateTransition = false;
		_isBehaviorInit = false;
	}

	public void StartCinematic(OnInitialFadeOutFinished initialFadeOutFinished, Action cinematicFinishedCallback, float transitionDuration = 0.4f, float stateDuration = 0.2f, float cinematicDuration = 8f, bool forceDismountAgents = false)
	{
		if (_isBehaviorInit && State == HideoutCinematicState.None)
		{
			OnCinematicFinished += cinematicFinishedCallback;
			_initialFadeOutFinished = initialFadeOutFinished;
			_preCinematicPhase = HideoutPreCinematicPhase.InitializeFormations;
			_postCinematicPhase = HideoutPostCinematicPhase.MoveAgents;
			_transitionDuration = transitionDuration;
			_stateDuration = stateDuration;
			_cinematicDuration = cinematicDuration;
			_remainingCinematicDuration = _cinematicDuration;
			BeginStateTransition(HideoutCinematicState.InitialFadeOut);
		}
		else if (!_isBehaviorInit)
		{
			Debug.FailedAssert("Hideout cinematic controller is not initialized.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\Hideout\\HideoutCinematicController.cs", "StartCinematic", 178);
		}
		else if (State != HideoutCinematicState.None)
		{
			Debug.FailedAssert("There is already an ongoing cinematic.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\Hideout\\HideoutCinematicController.cs", "StartCinematic", 182);
		}
	}

	public void GetBossStandingEyePosition(out Vec3 eyePosition)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Agent agent = _bossAgentInfo.Agent;
		if (((agent != null) ? agent.Monster : null) != null)
		{
			eyePosition = _bossAgentInfo.InitialFrame.origin + Vec3.Up * (_bossAgentInfo.Agent.AgentScale * _bossAgentInfo.Agent.Monster.StandingEyeHeight);
			return;
		}
		eyePosition = Vec3.Zero;
		Debug.FailedAssert("false", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\Hideout\\HideoutCinematicController.cs", "GetBossStandingEyePosition", 195);
	}

	public void GetPlayerStandingEyePosition(out Vec3 eyePosition)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Agent agent = _playerAgentInfo.Agent;
		if (((agent != null) ? agent.Monster : null) != null)
		{
			eyePosition = _playerAgentInfo.InitialFrame.origin + Vec3.Up * (_playerAgentInfo.Agent.AgentScale * _playerAgentInfo.Agent.Monster.StandingEyeHeight);
			return;
		}
		eyePosition = Vec3.Zero;
		Debug.FailedAssert("false", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\Hideout\\HideoutCinematicController.cs", "GetPlayerStandingEyePosition", 208);
	}

	public MatrixFrame GetBanditsInitialFrame()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_hideoutBossFightBehavior.GetBanditsInitialFrame(out var frame);
		return frame;
	}

	public void GetScenePrefabParameters(out float innerRadius, out float outerRadius, out float walkDistance)
	{
		innerRadius = 0f;
		outerRadius = 0f;
		walkDistance = 0f;
		if (_hideoutBossFightBehavior != null)
		{
			innerRadius = _hideoutBossFightBehavior.InnerRadius;
			outerRadius = _hideoutBossFightBehavior.OuterRadius;
			walkDistance = _hideoutBossFightBehavior.WalkDistance;
		}
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("hideout_boss_fight");
		_hideoutBossFightBehavior = ((val != null) ? val.GetFirstScriptOfType<HideoutBossFightBehavior>() : null);
		_isBehaviorInit = val != (GameEntity)null && _hideoutBossFightBehavior != null;
	}

	public override void OnMissionTick(float dt)
	{
		if (!_isBehaviorInit || !IsCinematicActive)
		{
			return;
		}
		if (InStateTransition)
		{
			TickStateTransition(dt);
			return;
		}
		switch (State)
		{
		case HideoutCinematicState.InitialFadeOut:
			if (TickInitialFadeOut(dt))
			{
				BeginStateTransition(HideoutCinematicState.PreCinematic);
			}
			break;
		case HideoutCinematicState.PreCinematic:
			if (TickPreCinematic(dt))
			{
				BeginStateTransition(HideoutCinematicState.Cinematic);
			}
			break;
		case HideoutCinematicState.Cinematic:
			if (TickCinematic(dt))
			{
				BeginStateTransition(HideoutCinematicState.PostCinematic);
			}
			break;
		case HideoutCinematicState.PostCinematic:
			if (TickPostCinematic(dt))
			{
				BeginStateTransition(HideoutCinematicState.Completed);
			}
			break;
		case HideoutCinematicState.Completed:
			this.OnCinematicFinished?.Invoke();
			this.OnCinematicFinished = null;
			this.OnCinematicStateChanged = null;
			this.OnCinematicTransition = null;
			State = HideoutCinematicState.None;
			break;
		}
	}

	private void TickStateTransition(float dt)
	{
		_remainingTransitionDuration -= dt;
		if (_remainingTransitionDuration <= 0f)
		{
			InStateTransition = false;
			this.OnCinematicStateChanged?.Invoke(State);
			_remainingStateDuration = _stateDuration;
		}
	}

	private bool TickInitialFadeOut(float dt)
	{
		_remainingStateDuration -= dt;
		if (_remainingStateDuration <= 0f)
		{
			Agent playerAgent = null;
			Agent bossAgent = null;
			List<Agent> playerCompanions = null;
			List<Agent> bossCompanions = null;
			float placementPerturbation = 0.25f;
			float placementAngle = MathF.PI / 15f;
			_initialFadeOutFinished?.Invoke(ref playerAgent, ref playerCompanions, ref bossAgent, ref bossCompanions, ref placementPerturbation, ref placementAngle);
			ComputeAgentFrames(playerAgent, playerCompanions, bossAgent, bossCompanions, placementPerturbation, placementAngle);
		}
		return _remainingStateDuration <= 0f;
	}

	private bool TickPreCinematic(float dt)
	{
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = ((MissionBehavior)this).Mission.Scene;
		_remainingStateDuration -= dt;
		Vec2 val2;
		switch (_preCinematicPhase)
		{
		case HideoutPreCinematicPhase.InitializeFormations:
		{
			_playerAgentInfo.Agent.Controller = (AgentControllerType)1;
			bool isTeleportingAgents2 = ((MissionBehavior)this).Mission.IsTeleportingAgents;
			((MissionBehavior)this).Mission.IsTeleportingAgents = true;
			_hideoutBossFightBehavior.GetAlliesInitialFrame(out var frame);
			WorldPosition val4 = default(WorldPosition);
			foreach (Formation item in (List<Formation>)(object)((MissionBehavior)this).Mission.Teams.Attacker.FormationsIncludingEmpty)
			{
				if (item.CountOfUnits > 0)
				{
					((WorldPosition)(ref val4))._002Ector(scene, frame.origin);
					item.SetMovementOrder(MovementOrder.MovementOrderMove(val4));
				}
			}
			_hideoutBossFightBehavior.GetBanditsInitialFrame(out var frame2);
			WorldPosition val5 = default(WorldPosition);
			foreach (Formation item2 in (List<Formation>)(object)((MissionBehavior)this).Mission.Teams.Defender.FormationsIncludingEmpty)
			{
				if (item2.CountOfUnits > 0)
				{
					((WorldPosition)(ref val5))._002Ector(scene, frame2.origin);
					item2.SetMovementOrder(MovementOrder.MovementOrderMove(val5));
				}
			}
			foreach (HideoutCinematicAgentInfo item3 in _hideoutAgentsInfo)
			{
				Agent agent3 = item3.Agent;
				Vec3 val6 = (agent3.LookDirection = item3.InitialFrame.rotation.f);
				val2 = ((Vec3)(ref val6)).AsVec2;
				val2 = ((Vec2)(ref val2)).Normalized();
				agent3.SetMovementDirection(ref val2);
			}
			((MissionBehavior)this).Mission.IsTeleportingAgents = isTeleportingAgents2;
			_preCinematicPhase = HideoutPreCinematicPhase.StopFormations;
			break;
		}
		case HideoutPreCinematicPhase.StopFormations:
			foreach (Formation item4 in (List<Formation>)(object)((MissionBehavior)this).Mission.Teams.Attacker.FormationsIncludingEmpty)
			{
				if (item4.CountOfUnits > 0)
				{
					item4.SetMovementOrder(MovementOrder.MovementOrderStop);
				}
			}
			foreach (Formation item5 in (List<Formation>)(object)((MissionBehavior)this).Mission.Teams.Defender.FormationsIncludingEmpty)
			{
				if (item5.CountOfUnits > 0)
				{
					item5.SetMovementOrder(MovementOrder.MovementOrderStop);
				}
			}
			_preCinematicPhase = HideoutPreCinematicPhase.InitializeAgents;
			break;
		case HideoutPreCinematicPhase.InitializeAgents:
		{
			bool isTeleportingAgents = ((MissionBehavior)this).Mission.IsTeleportingAgents;
			((MissionBehavior)this).Mission.IsTeleportingAgents = true;
			_cachedAgentFormations = new List<Formation>();
			WorldPosition val3 = default(WorldPosition);
			foreach (HideoutCinematicAgentInfo item6 in _hideoutAgentsInfo)
			{
				Agent agent2 = item6.Agent;
				_cachedAgentFormations.Add(agent2.Formation);
				agent2.Formation = null;
				MatrixFrame initialFrame = item6.InitialFrame;
				((WorldPosition)(ref val3))._002Ector(scene, initialFrame.origin);
				Vec3 f = initialFrame.rotation.f;
				agent2.TeleportToPosition(((WorldPosition)(ref val3)).GetGroundVec3());
				agent2.LookDirection = f;
				val2 = ((Vec3)(ref f)).AsVec2;
				val2 = ((Vec2)(ref val2)).Normalized();
				agent2.SetMovementDirection(ref val2);
			}
			((MissionBehavior)this).Mission.IsTeleportingAgents = isTeleportingAgents;
			_preCinematicPhase = HideoutPreCinematicPhase.MoveAgents;
			break;
		}
		case HideoutPreCinematicPhase.MoveAgents:
		{
			WorldPosition val = default(WorldPosition);
			foreach (HideoutCinematicAgentInfo item7 in _hideoutAgentsInfo)
			{
				Agent agent = item7.Agent;
				MatrixFrame targetFrame = item7.TargetFrame;
				((WorldPosition)(ref val))._002Ector(scene, targetFrame.origin);
				agent.SetMaximumSpeedLimit(0.65f, false);
				val2 = ((Vec3)(ref targetFrame.rotation.f)).AsVec2;
				agent.SetScriptedPositionAndDirection(ref val, ((Vec2)(ref val2)).RotationInRadians, true, (AIScriptedFrameFlags)0);
			}
			_preCinematicPhase = HideoutPreCinematicPhase.Completed;
			break;
		}
		}
		if (_preCinematicPhase == HideoutPreCinematicPhase.Completed)
		{
			return _remainingStateDuration <= 0f;
		}
		return false;
	}

	private bool TickCinematic(float dt)
	{
		_remainingCinematicDuration -= dt;
		_remainingStateDuration -= dt;
		if (_remainingCinematicDuration <= 0f && _remainingStateDuration <= 0f)
		{
			return true;
		}
		return false;
	}

	private bool TickPostCinematic(float dt)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		_remainingStateDuration -= dt;
		switch (_postCinematicPhase)
		{
		case HideoutPostCinematicPhase.MoveAgents:
		{
			int num = 0;
			WorldPosition val = default(WorldPosition);
			foreach (HideoutCinematicAgentInfo item in _hideoutAgentsInfo)
			{
				Agent agent2 = item.Agent;
				if (!item.HasReachedTarget())
				{
					MatrixFrame targetFrame = item.TargetFrame;
					((WorldPosition)(ref val))._002Ector(((MissionBehavior)this).Mission.Scene, targetFrame.origin);
					agent2.TeleportToPosition(((WorldPosition)(ref val)).GetGroundVec3());
					Vec2 val2 = ((Vec3)(ref targetFrame.rotation.f)).AsVec2;
					val2 = ((Vec2)(ref val2)).Normalized();
					agent2.SetMovementDirection(ref val2);
				}
				agent2.Formation = _cachedAgentFormations[num];
				num++;
			}
			_postCinematicPhase = HideoutPostCinematicPhase.FinalizeAgents;
			break;
		}
		case HideoutPostCinematicPhase.FinalizeAgents:
			foreach (HideoutCinematicAgentInfo item2 in _hideoutAgentsInfo)
			{
				Agent agent = item2.Agent;
				agent.DisableScriptedMovement();
				agent.SetMaximumSpeedLimit(-1f, false);
			}
			_postCinematicPhase = HideoutPostCinematicPhase.Completed;
			break;
		}
		if (_postCinematicPhase == HideoutPostCinematicPhase.Completed)
		{
			return _remainingStateDuration <= 0f;
		}
		return false;
	}

	private void BeginStateTransition(HideoutCinematicState nextState)
	{
		State = nextState;
		_remainingTransitionDuration = _transitionDuration;
		InStateTransition = true;
		this.OnCinematicTransition?.Invoke(State, _remainingTransitionDuration);
	}

	private bool CheckNavMeshValidity(ref Vec3 initial, ref Vec3 target)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = ((MissionBehavior)this).Mission.Scene;
		bool result = false;
		bool num = scene.GetNavigationMeshForPosition(ref initial) != UIntPtr.Zero;
		bool flag = scene.GetNavigationMeshForPosition(ref target) != UIntPtr.Zero;
		if (num && flag)
		{
			WorldPosition val = default(WorldPosition);
			((WorldPosition)(ref val))._002Ector(scene, initial);
			WorldPosition val2 = default(WorldPosition);
			((WorldPosition)(ref val2))._002Ector(scene, target);
			result = scene.DoesPathExistBetweenPositions(val, val2);
		}
		return result;
	}

	private void ComputeAgentFrames(Agent playerAgent, List<Agent> playerCompanions, Agent bossAgent, List<Agent> bossCompanions, float placementPerturbation, float placementAngle)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		_hideoutAgentsInfo = new List<HideoutCinematicAgentInfo>();
		_hideoutBossFightBehavior.GetPlayerFrames(out var initialFrame, out var targetFrame, placementPerturbation);
		_playerAgentInfo = new HideoutCinematicAgentInfo(playerAgent, HideoutAgentType.Player, in initialFrame, in targetFrame);
		_hideoutAgentsInfo.Add(_playerAgentInfo);
		_hideoutBossFightBehavior.GetAllyFrames(out var initialFrames, out var targetFrames, playerCompanions.Count, placementAngle, placementPerturbation);
		for (int i = 0; i < playerCompanions.Count; i++)
		{
			initialFrame = initialFrames[i];
			targetFrame = targetFrames[i];
			_hideoutAgentsInfo.Add(new HideoutCinematicAgentInfo(playerCompanions[i], HideoutAgentType.Ally, in initialFrame, in targetFrame));
		}
		_hideoutBossFightBehavior.GetBossFrames(out initialFrame, out targetFrame, placementPerturbation);
		_bossAgentInfo = new HideoutCinematicAgentInfo(bossAgent, HideoutAgentType.Boss, in initialFrame, in targetFrame);
		_hideoutAgentsInfo.Add(_bossAgentInfo);
		_hideoutBossFightBehavior.GetBanditFrames(out initialFrames, out targetFrames, bossCompanions.Count, placementAngle, placementPerturbation);
		for (int j = 0; j < bossCompanions.Count; j++)
		{
			initialFrame = initialFrames[j];
			targetFrame = targetFrames[j];
			_hideoutAgentsInfo.Add(new HideoutCinematicAgentInfo(bossCompanions[j], HideoutAgentType.Bandit, in initialFrame, in targetFrame));
		}
	}
}
