using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class ShipPlacementDetachment : IDetachment
{
	private class ShipPlacementPosition
	{
		private bool _isHighPos;

		private Agent _extraAgent;

		public Agent AssignedAgent { get; private set; }

		public MatrixFrame LocalFrame { get; }

		public bool IsOuterPos { get; }

		public bool HasExtraAgent { get; private set; }

		public bool LentToOtherFrame => ExtraFrameIndex >= 0;

		public int ExtraFrameIndex { get; private set; } = -1;

		public ShipPlacementPosition(MatrixFrame frame, bool isOuterPos, bool isHighPos)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			LocalFrame = frame;
			IsOuterPos = isOuterPos;
			_isHighPos = isHighPos;
			HasExtraAgent = false;
			AssignedAgent = null;
			_extraAgent = null;
		}

		public void RemoveAgent()
		{
			AssignedAgent = null;
			_extraAgent = null;
		}

		public void LendToExtraPosition(int extraFrameIndex)
		{
			ExtraFrameIndex = extraFrameIndex;
		}

		public void ResetPlacementPosition()
		{
			AssignedAgent = null;
			ResetExtraPosition();
		}

		public void ResetExtraPosition()
		{
			ExtraFrameIndex = -1;
			HasExtraAgent = false;
			_extraAgent = null;
		}

		public void SetAgent(Agent agent)
		{
			AssignedAgent = agent;
		}

		public void SetExtraAgent(Agent agent)
		{
			HasExtraAgent = agent != null;
			_extraAgent = agent;
		}

		public void CalculateDefaultScore(out float resultScore, out float resultPossibleGain, out PositionCondition outGainCondition)
		{
			float num = 1f * (IsOuterPos ? 10f : 1f) * (_isHighPos ? 50f : 1f);
			resultScore = ((AssignedAgent == null) ? 0f : (AssignedAgent.HasAnyRangedWeaponCached ? num : 1f));
			resultPossibleGain = num - resultScore;
			outGainCondition = PositionCondition.Ranged;
		}

		public void CalculateUnderMissileFireScore(out float resultScore, out float resultPossibleGain, out PositionCondition outGainCondition)
		{
			float num = 1f * (IsOuterPos ? 50f : 1f) * (_isHighPos ? 50f : 1f);
			if (!IsOuterPos && !_isHighPos)
			{
				num = 1f;
				resultScore = ((AssignedAgent != null) ? num : 0f);
				resultPossibleGain = num - resultScore;
				outGainCondition = PositionCondition.Any;
			}
			else if (!_isHighPos)
			{
				num = 50f;
				outGainCondition = PositionCondition.RangedOrShield;
				resultScore = ((AssignedAgent == null) ? 0f : (CheckCondition(outGainCondition, AssignedAgent) ? 50f : 1f));
				resultPossibleGain = num - resultScore;
			}
			else if (!IsOuterPos)
			{
				num = 50f;
				outGainCondition = PositionCondition.Ranged;
				resultScore = ((AssignedAgent == null) ? 0f : (CheckCondition(outGainCondition, AssignedAgent) ? 50f : 1f));
				resultPossibleGain = num - resultScore;
			}
			else
			{
				num = 250f;
				outGainCondition = PositionCondition.Ranged;
				resultScore = ((AssignedAgent == null) ? 0f : (CheckCondition(outGainCondition, AssignedAgent) ? 250f : 1f));
				resultPossibleGain = num - resultScore;
			}
		}

		public void CalculateBoardingScore(Vec2 boardingLocalPosition, out float resultScore, out float resultPossibleGain, out PositionCondition outGainCondition, out bool requestExtraAgent)
		{
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			requestExtraAgent = false;
			Vec2 val;
			MatrixFrame localFrame;
			Vec2 asVec;
			if (_isHighPos)
			{
				float num = 1f;
				if (((Vec2)(ref boardingLocalPosition)).IsNonZero())
				{
					if (boardingLocalPosition.x * LocalFrame.origin.x >= 0f)
					{
						val = ((Vec2)(ref boardingLocalPosition)).Normalized();
						localFrame = LocalFrame;
						asVec = ((Vec3)(ref localFrame.origin)).AsVec2;
						float num2 = ((Vec2)(ref val)).DotProduct(((Vec2)(ref asVec)).Normalized());
						if (num2 >= 0f)
						{
							num = MathF.Clamp(num2 * 10f, 1f, 10f);
						}
					}
				}
				else
				{
					num = 10f;
				}
				float num3 = 50f * (IsOuterPos ? 10f : 1f) * num;
				outGainCondition = PositionCondition.Ranged;
				resultScore = ((AssignedAgent == null) ? 0f : (CheckCondition(outGainCondition, AssignedAgent) ? num3 : 1f));
				resultPossibleGain = num3 - resultScore;
				return;
			}
			float num4;
			if (((Vec2)(ref boardingLocalPosition)).IsNonZero())
			{
				num4 = 0.1f;
				if (boardingLocalPosition.x * LocalFrame.origin.x >= 0f)
				{
					val = ((Vec2)(ref boardingLocalPosition)).Normalized();
					localFrame = LocalFrame;
					asVec = ((Vec3)(ref localFrame.origin)).AsVec2;
					num4 = MathF.Clamp((((Vec2)(ref val)).DotProduct(((Vec2)(ref asVec)).Normalized()) + 1f) * 10f, 1f, 15f);
					requestExtraAgent = AssignedAgent != null && _extraAgent == null;
				}
			}
			else
			{
				num4 = 10f;
			}
			float num5 = 100f * (IsOuterPos ? 10f : 1f) * num4;
			outGainCondition = PositionCondition.Any;
			resultScore = ((AssignedAgent == null) ? 0f : ((CheckCondition(outGainCondition, AssignedAgent) ? num5 : (num5 * 0.1f)) + ((_extraAgent == null) ? 0f : (CheckCondition(outGainCondition, _extraAgent) ? num5 : (num5 * 0.1f)))));
			resultPossibleGain = num5 * (requestExtraAgent ? 2f : 1f) - resultScore;
		}
	}

	private enum PositionCondition
	{
		Any,
		RangedOrShield,
		Ranged
	}

	private readonly Agent[] _agents;

	private readonly MBList<Formation> _userFormations;

	private readonly MBList<ShipPlacementPosition> _shipPlacementPositions;

	private readonly MissionShip _ownerShip;

	private bool _isUnderMissileFire;

	private bool _isBoarding;

	private Vec2 _boardingDirection;

	private MissionTimer _placementDetachmentTimer;

	private bool _isTickRequired = true;

	public MBReadOnlyList<Formation> UserFormations => (MBReadOnlyList<Formation>)(object)_userFormations;

	public bool IsLoose => true;

	public bool IsActive => true;

	public bool HasAgent => CountOfAgents > 0;

	public int CountOfAgents { get; private set; }

	public bool HasAvailableSlots => ((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Count > CountOfAgents;

	public bool IsTickRequired
	{
		get
		{
			if (!_isTickRequired)
			{
				return _placementDetachmentTimer.Check(false);
			}
			return true;
		}
	}

	public ShipPlacementDetachment(in MissionShip ownerShip)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Expected O, but got Unknown
		_ownerShip = ownerShip;
		_userFormations = new MBList<Formation>();
		_shipPlacementPositions = new MBList<ShipPlacementPosition>();
		float num = 0f;
		foreach (MatrixFrame item in (List<MatrixFrame>)(object)ownerShip.OuterDeckLocalFrames)
		{
			num += item.origin.z;
		}
		foreach (MatrixFrame item2 in (List<MatrixFrame>)(object)ownerShip.InnerDeckLocalFrames)
		{
			num += item2.origin.z;
		}
		foreach (MatrixFrame item3 in (List<MatrixFrame>)(object)ownerShip.CrewSpawnLocalFrames)
		{
			num += item3.origin.z;
		}
		int num2 = ((List<MatrixFrame>)(object)ownerShip.OuterDeckLocalFrames).Count + ((List<MatrixFrame>)(object)ownerShip.InnerDeckLocalFrames).Count + ((List<MatrixFrame>)(object)ownerShip.CrewSpawnLocalFrames).Count;
		float num3 = num / (float)((num2 <= 0) ? 1 : num2);
		foreach (MatrixFrame item4 in (List<MatrixFrame>)(object)ownerShip.OuterDeckLocalFrames)
		{
			((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Add(new ShipPlacementPosition(item4, isOuterPos: true, item4.origin.z - num3 >= 1f));
		}
		foreach (MatrixFrame item5 in (List<MatrixFrame>)(object)ownerShip.InnerDeckLocalFrames)
		{
			((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Add(new ShipPlacementPosition(item5, isOuterPos: false, item5.origin.z - num3 >= 1f));
		}
		foreach (MatrixFrame item6 in (List<MatrixFrame>)(object)ownerShip.CrewSpawnLocalFrames)
		{
			((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Add(new ShipPlacementPosition(item6, isOuterPos: false, item6.origin.z - num3 >= 1f));
		}
		_agents = (Agent[])(object)new Agent[((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Count];
		_boardingDirection = Vec2.Zero;
		_placementDetachmentTimer = new MissionTimer(5f);
	}

	public void AddAgent(Agent agent, int slotIndex, AIScriptedFrameFlags customFlags = (AIScriptedFrameFlags)0)
	{
		_agents[slotIndex] = agent;
		CountOfAgents++;
	}

	public void AddAgentAtSlotIndex(Agent agent, int slotIndex)
	{
		_agents[slotIndex] = agent;
		CountOfAgents++;
		((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[slotIndex].SetAgent(agent);
		Formation formation = agent.Formation;
		if (formation != null)
		{
			formation.DetachUnit(agent, true);
		}
		agent.Detachment = (IDetachment)(object)this;
		agent.SetDetachmentWeight(1f);
		agent.SetDetachmentIndex(slotIndex);
	}

	public void AddAgent(Agent agent)
	{
		for (int i = 0; i < _agents.Length; i++)
		{
			if (_agents[i] == null)
			{
				AddAgentAtSlotIndex(agent, i);
				break;
			}
		}
	}

	void IDetachment.FormationStartUsing(Formation formation)
	{
		((List<Formation>)(object)_userFormations).Add(formation);
	}

	void IDetachment.FormationStopUsing(Formation formation)
	{
		((List<Formation>)(object)_userFormations).Remove(formation);
	}

	public bool IsUsedByFormation(Formation formation)
	{
		return ((List<Formation>)(object)_userFormations).Contains(formation);
	}

	Agent IDetachment.GetMovingAgentAtSlotIndex(int slotIndex)
	{
		if (slotIndex >= _agents.Length)
		{
			return null;
		}
		return _agents[slotIndex];
	}

	void IDetachment.GetSlotIndexWeightTuples(List<(int, float)> slotIndexWeightTuples)
	{
	}

	bool IDetachment.IsSlotAtIndexAvailableForAgent(int slotIndex, Agent agent)
	{
		return false;
	}

	bool IDetachment.IsAgentEligible(Agent agent)
	{
		return (object)agent.Detachment == this;
	}

	void IDetachment.UnmarkDetachment()
	{
	}

	bool IDetachment.IsDetachmentRecentlyEvaluated()
	{
		return true;
	}

	void IDetachment.MarkSlotAtIndex(int slotIndex)
	{
	}

	bool IDetachment.IsAgentUsingOrInterested(Agent agent)
	{
		if (agent.DetachmentIndex >= 0 && agent.DetachmentIndex < _agents.Length)
		{
			return _agents[agent.DetachmentIndex] == agent;
		}
		return false;
	}

	void IDetachment.OnFormationLeave(Formation formation)
	{
		for (int num = _agents.Length - 1; num >= 0; num--)
		{
			Agent val = _agents[num];
			if (val != null && val.Formation == formation && !val.IsPlayerControlled)
			{
				_agents[num] = null;
				CountOfAgents--;
				val.SetCrouchMode(false);
				val.EnforceShieldUsage((UsageDirection)(-1));
				val.DisableScriptedMovement();
				val.DisableScriptedCombatMovement();
				formation.AttachUnit(val);
			}
		}
		for (int i = 0; i < ((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Count; i++)
		{
			((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[i].ResetPlacementPosition();
		}
	}

	public bool IsStandingPointAvailableForAgent(Agent agent)
	{
		return false;
	}

	public List<float> GetTemplateCostsOfAgent(Agent candidate, List<float> oldValue)
	{
		return oldValue;
	}

	float IDetachment.GetExactCostOfAgentAtSlot(Agent candidate, int slotIndex)
	{
		return float.MaxValue;
	}

	public float GetTemplateWeightOfAgent(Agent candidate)
	{
		return float.MaxValue;
	}

	public float? GetWeightOfAgentAtNextSlot(List<Agent> newAgents, out Agent match)
	{
		match = null;
		return null;
	}

	public float? GetWeightOfAgentAtNextSlot(List<(Agent, float)> agentTemplateScores, out Agent match)
	{
		match = null;
		return null;
	}

	public float? GetWeightOfAgentAtOccupiedSlot(Agent detachedAgent, List<Agent> newAgents, out Agent match)
	{
		match = null;
		return float.MaxValue;
	}

	public void RemoveAgent(Agent agent)
	{
		_agents[agent.DetachmentIndex] = null;
		CountOfAgents--;
		((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[agent.DetachmentIndex].RemoveAgent();
		agent.SetCrouchMode(false);
		agent.EnforceShieldUsage((UsageDirection)(-1));
		agent.DisableScriptedMovement();
		agent.DisableScriptedCombatMovement();
	}

	public int GetNumberOfUsableSlots()
	{
		return ((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Count - CountOfAgents;
	}

	public void SetUnderMissileFire(bool isUnderMissileFire)
	{
		if (_isUnderMissileFire != isUnderMissileFire)
		{
			_isUnderMissileFire = isUnderMissileFire;
			_isTickRequired = true;
		}
	}

	public void SetBoarding(bool isBoarding, Vec2 localDir)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (_isBoarding == isBoarding && (!((Vec2)(ref _boardingDirection)).IsNonZero() || ((Vec2)(ref localDir)).IsNonZero()) && (((Vec2)(ref _boardingDirection)).IsNonZero() || !((Vec2)(ref localDir)).IsNonZero()))
		{
			return;
		}
		if (!isBoarding || !((Vec2)(ref localDir)).IsNonZero())
		{
			for (int i = 0; i < ((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Count; i++)
			{
				((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[i].ResetExtraPosition();
			}
		}
		_isBoarding = isBoarding;
		_boardingDirection = localDir;
		_isTickRequired = true;
	}

	public void Tick()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		int num2 = -1;
		float num3 = float.MaxValue;
		int num4 = -1;
		PositionCondition positionCondition = PositionCondition.Any;
		bool flag = false;
		float resultPossibleGain = 0f;
		float resultScore = 0f;
		PositionCondition outGainCondition = PositionCondition.Any;
		bool requestExtraAgent = false;
		for (int i = 0; i < ((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Count; i++)
		{
			if (_isBoarding)
			{
				((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[i].CalculateBoardingScore(_boardingDirection, out resultScore, out resultPossibleGain, out outGainCondition, out requestExtraAgent);
			}
			else if (_isUnderMissileFire)
			{
				((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[i].CalculateUnderMissileFireScore(out resultScore, out resultPossibleGain, out outGainCondition);
			}
			else
			{
				((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[i].CalculateDefaultScore(out resultScore, out resultPossibleGain, out outGainCondition);
			}
			if (resultPossibleGain > num)
			{
				num = resultPossibleGain;
				num2 = i;
				positionCondition = outGainCondition;
				flag = requestExtraAgent;
			}
		}
		for (int j = 0; j < ((List<ShipPlacementPosition>)(object)_shipPlacementPositions).Count; j++)
		{
			if (((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[j].AssignedAgent != null && !((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[j].LentToOtherFrame && CheckCondition(positionCondition, ((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[j].AssignedAgent))
			{
				if (_isBoarding)
				{
					((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[j].CalculateBoardingScore(_boardingDirection, out resultScore, out resultPossibleGain, out outGainCondition, out requestExtraAgent);
				}
				else if (_isUnderMissileFire)
				{
					((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[j].CalculateUnderMissileFireScore(out resultScore, out resultPossibleGain, out outGainCondition);
				}
				else
				{
					((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[j].CalculateDefaultScore(out resultScore, out resultPossibleGain, out outGainCondition);
				}
				if (resultScore < num3)
				{
					num3 = resultScore;
					num4 = j;
				}
			}
		}
		if (num2 != num4 && num2 > -1 && num4 > -1 && num > num3)
		{
			Agent assignedAgent = ((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[num2].AssignedAgent;
			Agent assignedAgent2 = ((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[num4].AssignedAgent;
			if (flag)
			{
				((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[num4].LendToExtraPosition(num2);
				((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[num2].SetExtraAgent(assignedAgent2);
			}
			else
			{
				((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[num2].SetAgent(assignedAgent2);
				_agents[num2] = assignedAgent2;
				assignedAgent2.SetDetachmentIndex(num2);
				if (assignedAgent != null)
				{
					((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[num4].SetAgent(assignedAgent);
					_agents[num4] = assignedAgent;
					assignedAgent.SetDetachmentIndex(num4);
				}
				else
				{
					((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[num4].RemoveAgent();
					_agents[num4] = null;
				}
			}
			_isTickRequired = true;
		}
		else
		{
			_isTickRequired = false;
			_placementDetachmentTimer.Reset();
		}
	}

	public WorldFrame? GetAgentFrame(Agent agent)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		ShipPlacementPosition shipPlacementPosition = ((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[agent.DetachmentIndex];
		if (shipPlacementPosition.LentToOtherFrame)
		{
			shipPlacementPosition = ((List<ShipPlacementPosition>)(object)_shipPlacementPositions)[shipPlacementPosition.ExtraFrameIndex];
		}
		agent.EnforceShieldUsage((UsageDirection)((_isUnderMissileFire && !agent.HasAnyRangedWeaponCached) ? ((shipPlacementPosition.IsOuterPos && agent.HasShieldCached) ? 5 : 4) : (-1)));
		MatrixFrame val = shipPlacementPosition.LocalFrame;
		Vec3 val2;
		if (_isBoarding && shipPlacementPosition.HasExtraAgent)
		{
			ref Mat3 rotation = ref val.rotation;
			val2 = new Vec3(val.origin.x, val.origin.y + ((agent == shipPlacementPosition.AssignedAgent) ? (-0.5f) : 0.5f), val.origin.z, -1f);
			val = new MatrixFrame(ref rotation, ref val2);
		}
		MatrixFrame globalFrame = _ownerShip.GlobalFrame;
		MatrixFrame val3 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		Mat3 val4;
		if ((shipPlacementPosition.IsOuterPos && (agent.HasAnyRangedWeaponCached || _isBoarding)) || (_isUnderMissileFire && agent.HasShieldCached))
		{
			if (val.origin.x > 0f)
			{
				val2 = -_ownerShip.GlobalFrame.rotation.f;
				globalFrame = _ownerShip.GlobalFrame;
				ref Vec3 s = ref globalFrame.rotation.s;
				MatrixFrame globalFrame2 = _ownerShip.GlobalFrame;
				val4 = new Mat3(ref val2, ref s, ref globalFrame2.rotation.u);
			}
			else
			{
				globalFrame = _ownerShip.GlobalFrame;
				ref Vec3 f = ref globalFrame.rotation.f;
				val2 = -_ownerShip.GlobalFrame.rotation.s;
				MatrixFrame globalFrame2 = _ownerShip.GlobalFrame;
				val4 = new Mat3(ref f, ref val2, ref globalFrame2.rotation.u);
			}
		}
		else
		{
			val4 = val3.rotation;
		}
		int crouchMode;
		if (_isUnderMissileFire && !agent.HasAnyRangedWeaponCached && !agent.HasShieldCached)
		{
			val2 = agent.Position;
			crouchMode = ((((Vec3)(ref val2)).DistanceSquared(val3.origin) <= 1f) ? 1 : 0);
		}
		else
		{
			crouchMode = 0;
		}
		agent.SetCrouchMode((byte)crouchMode != 0);
		return new WorldFrame(val4, ModuleExtensions.ToWorldPosition(val3.origin));
	}

	public float? GetWeightOfNextSlot(BattleSideEnum side)
	{
		return null;
	}

	public float GetWeightOfOccupiedSlot(Agent agent)
	{
		return float.MinValue;
	}

	float IDetachment.GetDetachmentWeight(BattleSideEnum side)
	{
		return float.MinValue;
	}

	void IDetachment.ResetEvaluation()
	{
	}

	bool IDetachment.IsEvaluated()
	{
		return true;
	}

	void IDetachment.SetAsEvaluated()
	{
	}

	float IDetachment.GetDetachmentWeightFromCache()
	{
		return float.MinValue;
	}

	float IDetachment.ComputeAndCacheDetachmentWeight(BattleSideEnum side)
	{
		return float.MinValue;
	}

	public Agent PickLastAgent()
	{
		Agent result = null;
		for (int num = _agents.Length - 1; num >= 0; num--)
		{
			if (_agents[num] != null)
			{
				result = _agents[num];
				RemoveAgent(result);
				result.Formation.AttachUnit(result);
				return result;
			}
		}
		return result;
	}

	private static bool CheckCondition(PositionCondition positionCondition, Agent checkedAgent)
	{
		switch (positionCondition)
		{
		case PositionCondition.Any:
			return true;
		case PositionCondition.RangedOrShield:
			if (!checkedAgent.HasShieldCached)
			{
				return checkedAgent.HasAnyRangedWeaponCached;
			}
			return true;
		case PositionCondition.Ranged:
			return checkedAgent.HasAnyRangedWeaponCached;
		default:
			return false;
		}
	}
}
