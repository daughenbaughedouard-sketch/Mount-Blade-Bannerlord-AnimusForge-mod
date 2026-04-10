using System.Collections.Generic;
using System.Linq;
using SandBox.Conversation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class MissionAgentLookHandler : MissionLogic
{
	private class PointOfInterest
	{
		public const int MaxSelectDistanceForAgent = 5;

		public const int MaxSelectDistanceForFrame = 4;

		private readonly int _selectDistance;

		private readonly int _releaseDistanceSquare;

		private readonly Agent _agent;

		private readonly MatrixFrame _frame;

		private readonly bool _ignoreDirection;

		private readonly int _priority;

		public bool IsActive
		{
			get
			{
				if (_agent != null)
				{
					return _agent.IsActive();
				}
				return true;
			}
		}

		public PointOfInterest(Agent agent)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Invalid comparison between Unknown and I4
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Invalid comparison between Unknown and I4
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Invalid comparison between Unknown and I4
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Invalid comparison between Unknown and I4
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Invalid comparison between Unknown and I4
			_agent = agent;
			_selectDistance = 5;
			_releaseDistanceSquare = 36;
			_ignoreDirection = false;
			CharacterObject val = (CharacterObject)agent.Character;
			if (!agent.IsHuman)
			{
				_priority = 1;
			}
			else if (((BasicCharacterObject)val).IsHero)
			{
				_priority = 5;
			}
			else if ((int)val.Occupation == 12 || (int)val.Occupation == 10 || (int)val.Occupation == 4 || (int)val.Occupation == 11 || (int)val.Occupation == 28)
			{
				_priority = 3;
			}
			else
			{
				_priority = 1;
			}
		}

		public PointOfInterest(MatrixFrame frame)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			_frame = frame;
			_selectDistance = 4;
			_releaseDistanceSquare = 25;
			_ignoreDirection = true;
			_priority = 2;
		}

		public float GetScore(Agent agent)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			if (agent != _agent)
			{
				Vec3 basicPosition = GetBasicPosition();
				if (!(((Vec3)(ref basicPosition)).DistanceSquared(agent.Position) > (float)(_selectDistance * _selectDistance)))
				{
					Vec3 val = GetTargetPosition() - agent.GetEyeGlobalPosition();
					float num = ((Vec3)(ref val)).Normalize();
					if (Vec2.DotProduct(((Vec3)(ref val)).AsVec2, agent.GetMovementDirection()) < 0.7f)
					{
						return -1f;
					}
					float num2 = (float)(_priority * _selectDistance) / num;
					if (IsMoving())
					{
						num2 *= 5f;
					}
					if (!_ignoreDirection)
					{
						MatrixFrame val2 = GetTargetFrame();
						Vec2 asVec = ((Vec3)(ref val2.rotation.f)).AsVec2;
						val2 = agent.Frame;
						float num3 = Vec2.DotProduct(asVec, ((Vec3)(ref val2.rotation.f)).AsVec2);
						if (num3 < -0.7f)
						{
							num2 *= 2f;
						}
						else if (MathF.Abs(num3) < 0.1f)
						{
							num2 *= 2f;
						}
					}
					return num2;
				}
			}
			return -1f;
		}

		public Vec3 GetTargetPosition()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			Agent agent = _agent;
			if (agent == null)
			{
				return _frame.origin;
			}
			return agent.GetEyeGlobalPosition();
		}

		public Vec3 GetBasicPosition()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (_agent == null)
			{
				return _frame.origin;
			}
			return _agent.Position;
		}

		private bool IsMoving()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (_agent != null)
			{
				Vec2 currentVelocity = _agent.GetCurrentVelocity();
				return ((Vec2)(ref currentVelocity)).LengthSquared > 0.040000003f;
			}
			return true;
		}

		private MatrixFrame GetTargetFrame()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if (_agent == null)
			{
				return _frame;
			}
			return _agent.Frame;
		}

		public bool IsVisibleFor(Agent agent)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			Vec3 basicPosition = GetBasicPosition();
			Vec3 position = agent.Position;
			if (agent == _agent || ((Vec3)(ref position)).DistanceSquared(basicPosition) > (float)_releaseDistanceSquare)
			{
				return false;
			}
			Vec3 val = basicPosition - position;
			((Vec3)(ref val)).Normalize();
			return Vec2.DotProduct(((Vec3)(ref val)).AsVec2, agent.GetMovementDirection()) > 0.4f;
		}

		public bool IsRelevant(Agent agent)
		{
			return agent == _agent;
		}
	}

	private class LookInfo
	{
		public readonly Agent Agent;

		public PointOfInterest PointOfInterest;

		public readonly Timer CheckTimer;

		public LookInfo(Agent agent, float checkTime)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			Agent = agent;
			CheckTimer = new Timer(Mission.Current.CurrentTime, checkTime, true);
		}

		public void Reset(PointOfInterest pointOfInterest, float duration)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (PointOfInterest != pointOfInterest)
			{
				PointOfInterest = pointOfInterest;
				if (PointOfInterest != null)
				{
					Agent.SetLookToPointOfInterest(PointOfInterest.GetTargetPosition());
				}
				else if (Agent.IsActive())
				{
					Agent.DisableLookToPointOfInterest();
				}
			}
			CheckTimer.Reset(Mission.Current.CurrentTime, duration);
		}
	}

	private delegate PointOfInterest SelectionDelegate(Agent agent);

	private readonly List<PointOfInterest> _staticPointList;

	private readonly List<LookInfo> _checklist;

	private SelectionDelegate _selectionDelegate;

	public MissionAgentLookHandler()
	{
		_staticPointList = new List<PointOfInterest>();
		_checklist = new List<LookInfo>();
		_selectionDelegate = SelectRandomAccordingToScore;
	}

	public override void AfterStart()
	{
		AddStablePointsOfInterest();
	}

	private void AddStablePointsOfInterest()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		foreach (GameEntity item in ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("point_of_interest"))
		{
			_staticPointList.Add(new PointOfInterest(item.GetGlobalFrame()));
		}
	}

	private void DebugTick()
	{
	}

	public override void OnMissionTick(float dt)
	{
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		if (Game.Current.IsDevelopmentMode)
		{
			DebugTick();
		}
		float currentTime = ((MissionBehavior)this).Mission.CurrentTime;
		foreach (LookInfo item in _checklist)
		{
			if (!item.Agent.IsActive() || ConversationMission.ConversationAgents.Contains(item.Agent) || (ConversationMission.ConversationAgents.Any() && item.Agent.IsPlayerControlled))
			{
				continue;
			}
			if (item.CheckTimer.Check(currentTime))
			{
				PointOfInterest pointOfInterest = _selectionDelegate(item.Agent);
				if (pointOfInterest != null)
				{
					item.Reset(pointOfInterest, 5f);
				}
				else
				{
					item.Reset(null, 1f + MBRandom.RandomFloat);
				}
			}
			else if (item.PointOfInterest != null && (!item.PointOfInterest.IsActive || !item.PointOfInterest.IsVisibleFor(item.Agent)))
			{
				PointOfInterest pointOfInterest2 = _selectionDelegate(item.Agent);
				if (pointOfInterest2 != null)
				{
					item.Reset(pointOfInterest2, 5f + MBRandom.RandomFloat);
				}
				else
				{
					item.Reset(null, MBRandom.RandomFloat * 5f + 5f);
				}
			}
			else if (item.PointOfInterest != null)
			{
				Vec3 targetPosition = item.PointOfInterest.GetTargetPosition();
				item.Agent.SetLookToPointOfInterest(targetPosition);
			}
		}
	}

	private PointOfInterest SelectFirstNonAgent(Agent agent)
	{
		if (agent.IsAIControlled)
		{
			int num = MBRandom.RandomInt(_staticPointList.Count);
			int num2 = num;
			do
			{
				PointOfInterest pointOfInterest = _staticPointList[num2];
				if (pointOfInterest.GetScore(agent) > 0f)
				{
					return pointOfInterest;
				}
				num2 = ((num2 + 1 != _staticPointList.Count) ? (num2 + 1) : 0);
			}
			while (num2 != num);
		}
		return null;
	}

	private PointOfInterest SelectBestOfLimitedNonAgent(Agent agent)
	{
		int num = 3;
		PointOfInterest result = null;
		float num2 = -1f;
		if (agent.IsAIControlled)
		{
			int num3 = MBRandom.RandomInt(_staticPointList.Count);
			int num4 = num3;
			do
			{
				PointOfInterest pointOfInterest = _staticPointList[num4];
				float score = pointOfInterest.GetScore(agent);
				if (score > 0f)
				{
					if (score > num2)
					{
						num2 = score;
						result = pointOfInterest;
					}
					num--;
				}
				num4 = ((num4 + 1 != _staticPointList.Count) ? (num4 + 1) : 0);
			}
			while (num4 != num3 && num > 0);
		}
		return result;
	}

	private PointOfInterest SelectBest(Agent agent)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		PointOfInterest result = null;
		float num = -1f;
		if (agent.IsAIControlled)
		{
			foreach (PointOfInterest staticPoint in _staticPointList)
			{
				float score = staticPoint.GetScore(agent);
				if (score > 0f && score > num)
				{
					num = score;
					result = staticPoint;
				}
			}
			Mission mission = ((MissionBehavior)this).Mission;
			Vec3 position = agent.Position;
			ProximityMapSearchStruct val = AgentProximityMap.BeginSearch(mission, ((Vec3)(ref position)).AsVec2, 5f, false);
			while (((ProximityMapSearchStruct)(ref val)).LastFoundAgent != null)
			{
				PointOfInterest pointOfInterest = new PointOfInterest(((ProximityMapSearchStruct)(ref val)).LastFoundAgent);
				float score2 = pointOfInterest.GetScore(agent);
				if (score2 > 0f && score2 > num)
				{
					num = score2;
					result = pointOfInterest;
				}
				AgentProximityMap.FindNext(((MissionBehavior)this).Mission, ref val);
			}
		}
		return result;
	}

	private PointOfInterest SelectRandomAccordingToScore(Agent agent)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		List<KeyValuePair<float, PointOfInterest>> list = new List<KeyValuePair<float, PointOfInterest>>();
		if (agent.IsAIControlled)
		{
			foreach (PointOfInterest staticPoint in _staticPointList)
			{
				float score = staticPoint.GetScore(agent);
				if (score > 0f)
				{
					list.Add(new KeyValuePair<float, PointOfInterest>(score, staticPoint));
					num += score;
				}
			}
			Mission current2 = Mission.Current;
			Vec3 position = agent.Position;
			ProximityMapSearchStruct val = AgentProximityMap.BeginSearch(current2, ((Vec3)(ref position)).AsVec2, 5f, false);
			while (((ProximityMapSearchStruct)(ref val)).LastFoundAgent != null)
			{
				PointOfInterest pointOfInterest = new PointOfInterest(((ProximityMapSearchStruct)(ref val)).LastFoundAgent);
				float score2 = pointOfInterest.GetScore(agent);
				if (score2 > 0f)
				{
					list.Add(new KeyValuePair<float, PointOfInterest>(score2, pointOfInterest));
					num += score2;
				}
				AgentProximityMap.FindNext(Mission.Current, ref val);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		float num2 = MBRandom.RandomFloat * num;
		PointOfInterest value = list[list.Count - 1].Value;
		foreach (KeyValuePair<float, PointOfInterest> item in list)
		{
			num2 -= item.Key;
			if (num2 <= 0f)
			{
				value = item.Value;
				break;
			}
		}
		return value;
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if (agent.IsHuman)
		{
			_checklist.Add(new LookInfo(agent, MBRandom.RandomFloat));
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		for (int i = 0; i < _checklist.Count; i++)
		{
			LookInfo lookInfo = _checklist[i];
			if (lookInfo.Agent == affectedAgent)
			{
				_checklist.RemoveAt(i);
				i--;
			}
			else if (lookInfo.PointOfInterest != null && lookInfo.PointOfInterest.IsRelevant(affectedAgent))
			{
				lookInfo.Reset(null, MBRandom.RandomFloat * 2f + 2f);
			}
		}
	}
}
