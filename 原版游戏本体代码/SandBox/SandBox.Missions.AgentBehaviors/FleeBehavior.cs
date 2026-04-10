using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.MissionLogics;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class FleeBehavior : AgentBehavior
{
	private abstract class FleeGoalBase
	{
		protected readonly AgentNavigator _navigator;

		protected readonly Agent _ownerAgent;

		protected FleeGoalBase(AgentNavigator navigator, Agent ownerAgent)
		{
			_navigator = navigator;
			_ownerAgent = ownerAgent;
		}

		public abstract void TargetReached();

		public abstract void GoToTarget();

		public abstract bool IsGoalAchievable();

		public abstract bool IsGoalAchieved();
	}

	private class FleeAgentTarget : FleeGoalBase
	{
		public Agent Savior { get; private set; }

		public FleeAgentTarget(AgentNavigator navigator, Agent ownerAgent, Agent savior)
			: base(navigator, ownerAgent)
		{
			Savior = savior;
		}

		public override void GoToTarget()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			AgentNavigator navigator = _navigator;
			WorldPosition worldPosition = Savior.GetWorldPosition();
			MatrixFrame frame = Savior.Frame;
			Vec2 asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
			navigator.SetTargetFrame(worldPosition, ((Vec2)(ref asVec)).RotationInRadians, 0.2f, 0.02f, (AIScriptedFrameFlags)10);
		}

		public override bool IsGoalAchievable()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Invalid comparison between Unknown and I4
			WorldPosition val = Savior.GetWorldPosition();
			if (((WorldPosition)(ref val)).GetNearestNavMesh() != UIntPtr.Zero)
			{
				val = _navigator.TargetPosition;
				if (((WorldPosition)(ref val)).IsValid && Savior.IsActive())
				{
					return (int)Savior.CurrentWatchState != 2;
				}
			}
			return false;
		}

		public override bool IsGoalAchieved()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			WorldPosition targetPosition = _navigator.TargetPosition;
			if (((WorldPosition)(ref targetPosition)).IsValid)
			{
				targetPosition = _navigator.TargetPosition;
				Vec3 groundVec = ((WorldPosition)(ref targetPosition)).GetGroundVec3();
				return ((Vec3)(ref groundVec)).Distance(_ownerAgent.Position) <= _ownerAgent.GetInteractionDistanceToUsable((IUsable)(object)Savior);
			}
			return false;
		}

		public override void TargetReached()
		{
			_ownerAgent.SetActionChannel(0, ref ActionIndexCache.act_cheer_1, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			_ownerAgent.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			_ownerAgent.DisableScriptedMovement();
			Savior.DisableScriptedMovement();
			Savior.SetLookAgent(_ownerAgent);
			_ownerAgent.SetLookAgent(Savior);
		}
	}

	private class FleePassageTarget : FleeGoalBase
	{
		public Passage EscapePortal { get; private set; }

		public FleePassageTarget(AgentNavigator navigator, Agent ownerAgent, Passage escapePortal)
			: base(navigator, ownerAgent)
		{
			EscapePortal = escapePortal;
		}

		public override void GoToTarget()
		{
			_navigator.SetTarget((UsableMachine)(object)EscapePortal, isInitialTarget: false, (AIScriptedFrameFlags)0);
		}

		public override bool IsGoalAchievable()
		{
			if (((UsableMachine)EscapePortal).GetVacantStandingPointForAI(_ownerAgent) != null)
			{
				return !((UsableMachine)EscapePortal).IsDestroyed;
			}
			return false;
		}

		public override bool IsGoalAchieved()
		{
			StandingPoint vacantStandingPointForAI = ((UsableMachine)EscapePortal).GetVacantStandingPointForAI(_ownerAgent);
			if (vacantStandingPointForAI != null)
			{
				return ((UsableMissionObject)vacantStandingPointForAI).IsUsableByAgent(_ownerAgent);
			}
			return false;
		}

		public override void TargetReached()
		{
		}
	}

	private class FleePositionTarget : FleeGoalBase
	{
		public Vec3 Position { get; private set; }

		public FleePositionTarget(AgentNavigator navigator, Agent ownerAgent, Vec3 position)
			: base(navigator, ownerAgent)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			Position = position;
		}

		public override void GoToTarget()
		{
		}

		public override bool IsGoalAchievable()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			WorldPosition targetPosition = _navigator.TargetPosition;
			return ((WorldPosition)(ref targetPosition)).IsValid;
		}

		public override bool IsGoalAchieved()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			WorldPosition targetPosition = _navigator.TargetPosition;
			if (((WorldPosition)(ref targetPosition)).IsValid)
			{
				return _navigator.IsTargetReached();
			}
			return false;
		}

		public override void TargetReached()
		{
		}
	}

	private class FleeCoverTarget : FleeGoalBase
	{
		public FleeCoverTarget(AgentNavigator navigator, Agent ownerAgent)
			: base(navigator, ownerAgent)
		{
		}

		public override void GoToTarget()
		{
			_ownerAgent.DisableScriptedMovement();
		}

		public override bool IsGoalAchievable()
		{
			return true;
		}

		public override bool IsGoalAchieved()
		{
			return true;
		}

		public override void TargetReached()
		{
		}
	}

	private enum State
	{
		None,
		Afraid,
		LookForPlace,
		Flee,
		Complain
	}

	private enum FleeTargetType
	{
		Indoor,
		Guard,
		Cover
	}

	public const float ScoreThreshold = 1f;

	public const float DangerDistance = 5f;

	public const float ImmediateDangerDistance = 2f;

	public const float DangerDistanceSquared = 25f;

	public const float ImmediateDangerDistanceSquared = 4f;

	private readonly MissionAgentHandler _missionAgentHandler;

	private readonly MissionFightHandler _missionFightHandler;

	private State _state;

	private readonly BasicMissionTimer _reconsiderFleeTargetTimer;

	private const float ReconsiderImmobilizedFleeTargetTime = 0.5f;

	private const float ReconsiderDefaultFleeTargetTime = 1f;

	private FleeGoalBase _selectedGoal;

	private BasicMissionTimer _scareTimer;

	private float _scareTime;

	private BasicMissionTimer _complainToGuardTimer;

	private const float ComplainToGuardTime = 2f;

	private FleeTargetType _selectedFleeTargetType;

	private FleeTargetType SelectedFleeTargetType
	{
		get
		{
			return _selectedFleeTargetType;
		}
		set
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (value != _selectedFleeTargetType)
			{
				_selectedFleeTargetType = value;
				MBActionSet actionSet = base.OwnerAgent.ActionSet;
				ActionIndexCache currentAction = base.OwnerAgent.GetCurrentAction(1);
				if (_selectedFleeTargetType != FleeTargetType.Cover && !((MBActionSet)(ref actionSet)).AreActionsAlternatives(ref currentAction, ref ActionIndexCache.act_scared_idle_1) && !((MBActionSet)(ref actionSet)).AreActionsAlternatives(ref currentAction, ref ActionIndexCache.act_scared_reaction_1))
				{
					base.OwnerAgent.SetActionChannel(1, ref ActionIndexCache.act_scared_reaction_1, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				}
				if (_selectedFleeTargetType == FleeTargetType.Cover)
				{
					BeAfraid();
				}
				_selectedGoal.GoToTarget();
			}
		}
	}

	public FleeBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		_missionAgentHandler = base.Mission.GetMissionBehavior<MissionAgentHandler>();
		_missionFightHandler = base.Mission.GetMissionBehavior<MissionFightHandler>();
		_reconsiderFleeTargetTimer = new BasicMissionTimer();
		_state = State.None;
	}

	public override void Tick(float dt, bool isSimulation)
	{
		switch (_state)
		{
		case State.None:
			base.OwnerAgent.DisableScriptedMovement();
			base.OwnerAgent.SetActionChannel(1, ref ActionIndexCache.act_scared_reaction_1, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloat, false, -0.2f, 0, true);
			_selectedGoal = new FleeCoverTarget(base.Navigator, base.OwnerAgent);
			SelectedFleeTargetType = FleeTargetType.Cover;
			break;
		case State.Afraid:
			if (_scareTimer.ElapsedTime > _scareTime)
			{
				_state = State.LookForPlace;
				_scareTimer = null;
			}
			break;
		case State.LookForPlace:
			LookForPlace();
			break;
		case State.Flee:
			Flee();
			break;
		case State.Complain:
			if (_complainToGuardTimer != null && _complainToGuardTimer.ElapsedTime > 2f)
			{
				_complainToGuardTimer = null;
				base.OwnerAgent.SetActionChannel(0, ref ActionIndexCache.act_none, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				base.OwnerAgent.SetLookAgent((Agent)null);
				(_selectedGoal as FleeAgentTarget).Savior.SetLookAgent((Agent)null);
				AlarmedBehaviorGroup.AlarmAgent((_selectedGoal as FleeAgentTarget).Savior);
				_state = State.LookForPlace;
			}
			break;
		}
	}

	private Vec3 GetDangerPosition()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.Zero;
		if (_missionFightHandler != null)
		{
			IEnumerable<Agent> dangerSources = _missionFightHandler.GetDangerSources(base.OwnerAgent);
			if (dangerSources.Any())
			{
				foreach (Agent item in dangerSources)
				{
					val += item.Position;
				}
				val /= (float)dangerSources.Count();
			}
		}
		return val;
	}

	private bool IsThereDanger()
	{
		if (_missionFightHandler == null)
		{
			return false;
		}
		return _missionFightHandler.GetDangerSources(base.OwnerAgent).Any();
	}

	private float GetPathScore(WorldPosition startWorldPos, WorldPosition targetWorldPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		NavigationPath val = new NavigationPath();
		base.Mission.Scene.GetPathBetweenAIFaces(((WorldPosition)(ref startWorldPos)).GetNearestNavMesh(), ((WorldPosition)(ref targetWorldPos)).GetNearestNavMesh(), ((WorldPosition)(ref startWorldPos)).AsVec2, ((WorldPosition)(ref targetWorldPos)).AsVec2, 0f, val, (int[])null);
		Vec3 dangerPosition = GetDangerPosition();
		Vec2 asVec = ((Vec3)(ref dangerPosition)).AsVec2;
		Vec2 val2 = asVec - ((WorldPosition)(ref startWorldPos)).AsVec2;
		float num2 = MBMath.WrapAngle(((Vec2)(ref val2)).RotationInRadians);
		float rotationInRadians;
		if (val.Size <= 0)
		{
			val2 = ((WorldPosition)(ref targetWorldPos)).AsVec2 - ((WorldPosition)(ref startWorldPos)).AsVec2;
			rotationInRadians = ((Vec2)(ref val2)).RotationInRadians;
		}
		else
		{
			val2 = val.PathPoints[0] - ((WorldPosition)(ref startWorldPos)).AsVec2;
			rotationInRadians = ((Vec2)(ref val2)).RotationInRadians;
		}
		float num3 = MathF.Abs(MBMath.GetSmallestDifferenceBetweenTwoAngles(MBMath.WrapAngle(rotationInRadians), num2)) / MathF.PI * 1f;
		val2 = ((WorldPosition)(ref startWorldPos)).AsVec2;
		float num4 = ((Vec2)(ref val2)).DistanceSquared(asVec);
		if (val.Size > 0)
		{
			float num5 = float.MaxValue;
			Vec2 val3 = ((WorldPosition)(ref startWorldPos)).AsVec2;
			for (int i = 0; i < val.Size; i++)
			{
				float num6 = Vec2.DistanceToLineSegmentSquared(val.PathPoints[i], val3, asVec);
				val3 = val.PathPoints[i];
				if (num6 < num5)
				{
					num5 = num6;
				}
			}
			num = ((num4 > num5 && num5 < 25f) ? (1f * (num5 - num4) / 225f) : ((!(num4 > 4f)) ? 1f : (1f * num5 / 225f)));
		}
		val2 = ((WorldPosition)(ref startWorldPos)).AsVec2;
		float num7 = 1f * (225f / ((Vec2)(ref val2)).DistanceSquared(((WorldPosition)(ref targetWorldPos)).AsVec2));
		return (1f + num3) * (1f + num3) - 2f + num + num7;
	}

	private void LookForPlace()
	{
		FleeGoalBase selectedGoal = new FleeCoverTarget(base.Navigator, base.OwnerAgent);
		FleeTargetType selectedFleeTargetType = FleeTargetType.Cover;
		if (IsThereDanger())
		{
			List<(float, Agent)> availableGuardScores = GetAvailableGuardScores();
			List<(float, Passage)> availablePassageScores = GetAvailablePassageScores();
			float num = float.MinValue;
			foreach (var item in availablePassageScores)
			{
				var (num2, _) = item;
				if (num2 > num)
				{
					num = num2;
					selectedFleeTargetType = FleeTargetType.Indoor;
					selectedGoal = new FleePassageTarget(base.Navigator, base.OwnerAgent, item.Item2);
				}
			}
			foreach (var item2 in availableGuardScores)
			{
				var (num3, _) = item2;
				if (num3 > num)
				{
					num = num3;
					selectedFleeTargetType = FleeTargetType.Guard;
					selectedGoal = new FleeAgentTarget(base.Navigator, base.OwnerAgent, item2.Item2);
				}
			}
		}
		_selectedGoal = selectedGoal;
		SelectedFleeTargetType = selectedFleeTargetType;
		_state = State.Flee;
	}

	private bool ShouldChangeTarget()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (_selectedFleeTargetType == FleeTargetType.Guard)
		{
			WorldPosition worldPosition = (_selectedGoal as FleeAgentTarget).Savior.GetWorldPosition();
			WorldPosition worldPosition2 = base.OwnerAgent.GetWorldPosition();
			if (GetPathScore(worldPosition2, worldPosition) <= 1f)
			{
				return IsThereASafePlaceToEscape();
			}
			return false;
		}
		if (_selectedFleeTargetType == FleeTargetType.Indoor)
		{
			StandingPoint vacantStandingPointForAI = ((UsableMachine)(_selectedGoal as FleePassageTarget).EscapePortal).GetVacantStandingPointForAI(base.OwnerAgent);
			if (vacantStandingPointForAI == null)
			{
				return true;
			}
			WorldPosition worldPosition3 = base.OwnerAgent.GetWorldPosition();
			WorldPosition origin = ((UsableMissionObject)vacantStandingPointForAI).GetUserFrameForAgent(base.OwnerAgent).Origin;
			if (GetPathScore(worldPosition3, origin) <= 1f)
			{
				return IsThereASafePlaceToEscape();
			}
			return false;
		}
		return true;
	}

	private bool IsThereASafePlaceToEscape()
	{
		if (!GetAvailablePassageScores(1).Any(((float, Passage) d) => d.Item1 > 1f))
		{
			return GetAvailableGuardScores(1).Any(((float, Agent) d) => d.Item1 > 1f);
		}
		return true;
	}

	private List<(float, Passage)> GetAvailablePassageScores(int maxPaths = 10)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		WorldPosition worldPosition = base.OwnerAgent.GetWorldPosition();
		List<(float, Passage)> list = new List<(float, Passage)>();
		List<(float, Passage)> list2 = new List<(float, Passage)>();
		List<(WorldPosition, Passage)> list3 = new List<(WorldPosition, Passage)>();
		if (_missionAgentHandler.TownPassageProps != null)
		{
			foreach (UsableMachine townPassageProp in _missionAgentHandler.TownPassageProps)
			{
				StandingPoint vacantStandingPointForAI = townPassageProp.GetVacantStandingPointForAI(base.OwnerAgent);
				Passage passage = townPassageProp as Passage;
				if (vacantStandingPointForAI != null && passage != null)
				{
					WorldPosition origin = ((UsableMissionObject)vacantStandingPointForAI).GetUserFrameForAgent(base.OwnerAgent).Origin;
					list3.Add((origin, passage));
				}
			}
		}
		list3 = list3.OrderBy(delegate((WorldPosition, Passage) a)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			Vec3 position = base.OwnerAgent.Position;
			Vec2 asVec = ((Vec3)(ref position)).AsVec2;
			return ((Vec2)(ref asVec)).DistanceSquared(((WorldPosition)(ref a.Item1)).AsVec2);
		}).ToList();
		foreach (var item2 in list3)
		{
			var (targetWorldPos, _) = item2;
			if (((WorldPosition)(ref targetWorldPos)).IsValid && !(((WorldPosition)(ref targetWorldPos)).GetNearestNavMesh() == UIntPtr.Zero))
			{
				float pathScore = GetPathScore(worldPosition, targetWorldPos);
				(float, Passage) item = (pathScore, item2.Item2);
				list.Add(item);
				if (pathScore > 1f)
				{
					list2.Add(item);
				}
				if (list2.Count >= maxPaths)
				{
					break;
				}
			}
		}
		if (list2.Count > 0)
		{
			return list2;
		}
		return list;
	}

	private List<(float, Agent)> GetAvailableGuardScores(int maxGuards = 5)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Invalid comparison between Unknown and I4
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Invalid comparison between Unknown and I4
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		WorldPosition worldPosition = base.OwnerAgent.GetWorldPosition();
		List<(float, Agent)> list = new List<(float, Agent)>();
		List<(float, Agent)> list2 = new List<(float, Agent)>();
		List<Agent> list3 = new List<Agent>();
		foreach (Agent item2 in (List<Agent>)(object)base.OwnerAgent.Team.ActiveAgents)
		{
			BasicCharacterObject character = item2.Character;
			CharacterObject val;
			if ((val = (CharacterObject)(object)((character is CharacterObject) ? character : null)) != null && item2.IsAIControlled && (int)item2.CurrentWatchState != 2 && ((int)val.Occupation == 7 || (int)val.Occupation == 24 || (int)val.Occupation == 23))
			{
				list3.Add(item2);
			}
		}
		list3 = list3.OrderBy(delegate(Agent a)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Vec3 position = base.OwnerAgent.Position;
			return ((Vec3)(ref position)).DistanceSquared(a.Position);
		}).ToList();
		foreach (Agent item3 in list3)
		{
			WorldPosition worldPosition2 = item3.GetWorldPosition();
			if (((WorldPosition)(ref worldPosition2)).IsValid)
			{
				float pathScore = GetPathScore(worldPosition, worldPosition2);
				(float, Agent) item = (pathScore, item3);
				list.Add(item);
				if (pathScore > 1f)
				{
					list2.Add(item);
				}
				if (list2.Count >= maxGuards)
				{
					break;
				}
			}
		}
		if (list2.Count > 0)
		{
			return list2;
		}
		return list;
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		_state = State.None;
	}

	private void Flee()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		if (_selectedGoal.IsGoalAchievable())
		{
			if (_selectedGoal.IsGoalAchieved())
			{
				_selectedGoal.TargetReached();
				switch (SelectedFleeTargetType)
				{
				case FleeTargetType.Cover:
					if (_reconsiderFleeTargetTimer.ElapsedTime > 0.5f)
					{
						_state = State.LookForPlace;
						_reconsiderFleeTargetTimer.Reset();
					}
					break;
				case FleeTargetType.Guard:
					_complainToGuardTimer = new BasicMissionTimer();
					_state = State.Complain;
					break;
				}
				return;
			}
			if (SelectedFleeTargetType == FleeTargetType.Guard)
			{
				_selectedGoal.GoToTarget();
			}
			if (_reconsiderFleeTargetTimer.ElapsedTime > 1f)
			{
				_reconsiderFleeTargetTimer.Reset();
				if (ShouldChangeTarget())
				{
					_state = State.LookForPlace;
				}
			}
		}
		else
		{
			_state = State.LookForPlace;
		}
	}

	private void BeAfraid()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		_scareTimer = new BasicMissionTimer();
		_scareTime = 0.5f + MBRandom.RandomFloat * 0.5f;
		_state = State.Afraid;
	}

	public override string GetDebugInfo()
	{
		return "Flee " + _state;
	}

	public override float GetAvailability(bool isSimulation)
	{
		if (base.Mission.CurrentTime < 3f)
		{
			return 0f;
		}
		if (!MissionFightHandler.IsAgentAggressive(base.OwnerAgent))
		{
			return 0.9f;
		}
		return 0.1f;
	}
}
