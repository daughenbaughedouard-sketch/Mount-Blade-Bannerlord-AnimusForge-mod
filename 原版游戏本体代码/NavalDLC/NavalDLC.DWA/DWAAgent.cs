using System.Collections.Generic;
using TaleWorlds.Library;

namespace NavalDLC.DWA;

public class DWAAgent
{
	private ushort _lastStateUpdateParity;

	private MBList<KeyValuePair<float, DWAAgent>> _agentNeighbors = new MBList<KeyValuePair<float, DWAAgent>>();

	private MBList<KeyValuePair<float, DWAObstacleVertex>> _obstacleNeighbors = new MBList<KeyValuePair<float, DWAObstacleVertex>>();

	private readonly DWASimulator _simulator;

	private DWAAgentState[] _forecastStates;

	private (float distance, float amount) _targetOcclusion;

	public int Id { get; private set; }

	public ref readonly DWAAgentState State => ref Delegate.State;

	public MBReadOnlyList<KeyValuePair<float, DWAAgent>> AgentNeighbors => (MBReadOnlyList<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors;

	public MBReadOnlyList<KeyValuePair<float, DWAObstacleVertex>> ObstacleNeighbors => (MBReadOnlyList<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors;

	public IDWAAgentDelegate Delegate { get; private set; }

	public bool IsForecast { get; private set; }

	public int LastForecastNumTimeSamples { get; private set; }

	public (float distance, float amount) TargetOcclusion => _targetOcclusion;

	public DWAAgent(DWASimulator simulator, int id, IDWAAgentDelegate agentDelegate)
	{
		Id = id;
		_simulator = simulator;
		Delegate = agentDelegate;
		_lastStateUpdateParity = ushort.MaxValue;
	}

	public bool TryUpdateState(ushort parity)
	{
		if (parity != _lastStateUpdateParity)
		{
			IsForecast = false;
			Delegate.OnStateUpdate();
			_lastStateUpdateParity = parity;
			return true;
		}
		return false;
	}

	public bool IsStateUpToDate(ushort parity)
	{
		return _lastStateUpdateParity == parity;
	}

	public void ComputeNeighbors(ushort parity)
	{
		((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors).Clear();
		float neighborDistance = Delegate.NeighborDistance;
		float rangeSq = neighborDistance * neighborDistance;
		if (Delegate.AvoidObstacleCollisions && _simulator.Parameters.MaxObstacleNeighbors > 0)
		{
			_simulator.ComputeObstacleNeighbors(this, rangeSq);
		}
		((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors).Clear();
		if (Delegate.AvoidAgentCollisions && _simulator.Parameters.MaxAgentNeighbors > 0)
		{
			_simulator.ComputeAgentNeighbors(this, rangeSq, parity);
		}
	}

	public void SetForecastStates(int maxTimeSamples)
	{
		if (_forecastStates == null || _forecastStates.Length != maxTimeSamples)
		{
			_forecastStates = new DWAAgentState[maxTimeSamples];
		}
	}

	public void ForecastTrajectory(float dt, int numTimeSamples)
	{
		LastForecastNumTimeSamples = numTimeSamples;
		DWAAgentState curState = State;
		DWAAgentState newState = default(DWAAgentState);
		for (int i = 0; i < numTimeSamples; i++)
		{
			IntegrateState(in curState, dt, ref newState);
			_forecastStates[i] = newState;
			curState = newState;
		}
		IsForecast = true;
	}

	public void InsertAgentNeighbor(DWAAgent agent, ref float rangeSq, ushort parity)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (this == agent)
		{
			return;
		}
		agent.TryUpdateState(parity);
		Vec2 val = State.Position - agent.State.Position;
		float lengthSquared = ((Vec2)(ref val)).LengthSquared;
		int maxAgentNeighbors = _simulator.Parameters.MaxAgentNeighbors;
		int num = ((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors).Count;
		if (num != maxAgentNeighbors || !(lengthSquared >= rangeSq))
		{
			if (num < maxAgentNeighbors)
			{
				((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors).Add(new KeyValuePair<float, DWAAgent>(lengthSquared, agent));
				num++;
			}
			int num2 = num - 1;
			while (num2 != 0 && lengthSquared < ((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors)[num2 - 1].Key)
			{
				((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors)[num2] = ((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors)[num2 - 1];
				num2--;
			}
			((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors)[num2] = new KeyValuePair<float, DWAAgent>(lengthSquared, agent);
			if (((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors).Count == maxAgentNeighbors)
			{
				rangeSq = ((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors)[((List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors).Count - 1].Key;
			}
		}
	}

	public void InsertObstacleNeighbor(DWAObstacleVertex obstacle, ref float rangeSq)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		DWAObstacleVertex next = obstacle.Next;
		int maxObstacleNeighbors = _simulator.Parameters.MaxObstacleNeighbors;
		Vec2 point = obstacle.Point;
		Vec2 point2 = next.Point;
		float distanceSquareOfPointToLineSegment = MBMath.GetDistanceSquareOfPointToLineSegment(ref point, ref point2, State.Position);
		int num = ((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors).Count;
		if (num != maxObstacleNeighbors || !(distanceSquareOfPointToLineSegment >= rangeSq))
		{
			if (num < maxObstacleNeighbors)
			{
				((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors).Add(default(KeyValuePair<float, DWAObstacleVertex>));
				num++;
			}
			int num2 = num - 1;
			while (num2 != 0 && distanceSquareOfPointToLineSegment < ((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors)[num2 - 1].Key)
			{
				((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors)[num2] = ((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors)[num2 - 1];
				num2--;
			}
			((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors)[num2] = new KeyValuePair<float, DWAObstacleVertex>(distanceSquareOfPointToLineSegment, obstacle);
			if (((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors).Count == maxObstacleNeighbors)
			{
				rangeSq = ((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors)[((List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors).Count - 1].Key;
			}
		}
	}

	public void InitializeThreads(in DWASimulatorParameters parameters, DWAThread[] processThreads)
	{
		int numLinearAccelerationSamples = parameters.NumLinearAccelerationSamples;
		int numAngularAccelerationSamples = parameters.NumAngularAccelerationSamples;
		bool ignoreZeroAction = parameters.IgnoreZeroAction;
		float maxLinearAcceleration = Delegate.MaxLinearAcceleration;
		float maxAngularAcceleration = Delegate.MaxAngularAcceleration;
		int num = numLinearAccelerationSamples / 2;
		int num2 = numAngularAccelerationSamples / 2;
		float num3 = ((numLinearAccelerationSamples > 1) ? (2f * maxLinearAcceleration / (float)(numLinearAccelerationSamples - 1)) : 0f);
		float num4 = ((numAngularAccelerationSamples > 1) ? (2f * maxAngularAcceleration / (float)(numAngularAccelerationSamples - 1)) : 0f);
		int num5 = 0;
		for (int i = 0; i < numLinearAccelerationSamples; i++)
		{
			float dV = 0f - maxLinearAcceleration + (float)i * num3;
			if (i == num)
			{
				dV = 0f;
			}
			for (int j = 0; j < numAngularAccelerationSamples; j++)
			{
				float dOmega = 0f - maxAngularAcceleration + (float)j * num4;
				if (j == num2)
				{
					dOmega = 0f;
				}
				if (!ignoreZeroAction || j != num2 || i != num)
				{
					processThreads[num5++].Initialize(this, dV, dOmega, parameters.DeltaTime, parameters.NumTimeSamples);
				}
			}
		}
	}

	public void ComputeTargetOcclusion()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		Vec2 goalDir;
		float goalDirection = Delegate.GetGoalDirection(out goalDir);
		float minExtent = Delegate.State.MinExtent;
		float maxExtent = Delegate.State.MaxExtent;
		float num = 2.5f * minExtent;
		float num2 = MathF.Min(goalDirection, 8f * maxExtent);
		float num3 = 0f;
		float num4 = float.PositiveInfinity;
		foreach (KeyValuePair<float, DWAAgent> item in (List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors)
		{
			Vec2 val = item.Value.State.ShapeCenter - State.Position;
			float num5 = Vec2.DotProduct(val, goalDir);
			if (!(num5 <= 0f) && !(num5 >= num2))
			{
				float num6 = MathF.Abs(Vec2.DotProduct(val, ((Vec2)(ref goalDir)).LeftVec()));
				float num7 = 2f * maxExtent;
				float num8 = DWAHelpers.GateNear(num6, num) * DWAHelpers.GateNear(num5, MathF.Max(num2 - num7, 1E-05f), num7);
				if (num8 > num3)
				{
					num3 = num8;
				}
				if (num6 < num && num5 < num4)
				{
					num4 = num5;
				}
			}
		}
		int num9 = 100;
		foreach (KeyValuePair<float, DWAObstacleVertex> item2 in (List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors)
		{
			DWAObstacleVertex value = item2.Value;
			int num10 = 0;
			DWAObstacleVertex dWAObstacleVertex = value;
			do
			{
				Vec2 val2 = dWAObstacleVertex.Point - State.Position;
				float num11 = Vec2.DotProduct(val2, goalDir);
				if (num11 > 0f && num11 < num2)
				{
					float num12 = MathF.Abs(Vec2.DotProduct(val2, ((Vec2)(ref goalDir)).LeftVec()));
					float num13 = DWAHelpers.GateNear(num12, num) * DWAHelpers.GateNear(num11, num2);
					if (num13 > num3)
					{
						num3 = num13;
					}
					if (num12 < num && num11 < num4)
					{
						num4 = num11;
					}
				}
				dWAObstacleVertex = dWAObstacleVertex.Next;
			}
			while (dWAObstacleVertex != value && num10 < num9);
		}
		if (float.IsPositiveInfinity(num4))
		{
			num4 = num2;
		}
		_targetOcclusion = (distance: num4, amount: num3);
	}

	public void EvaluateState(in DWAAgentState state, int sampleIndex, out bool hasCollision, out DWAAgent collidedAgent, out DWAObstacleVertex collidedObstacle, out float goalCost, out float proxCost, out float maxPenetration, Vec2[] obstaclePolyBuffer)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		goalCost = Delegate.ComputeGoalCost(sampleIndex, in state, _targetOcclusion);
		hasCollision = false;
		collidedAgent = null;
		collidedObstacle = null;
		Vec2 shapeHalfSize = state.ShapeHalfSize;
		MathF.Max(shapeHalfSize.x, shapeHalfSize.y);
		float safetyFactor = Delegate.GetSafetyFactor();
		float num = 0f;
		float num2 = 0f;
		maxPenetration = 0f;
		foreach (KeyValuePair<float, DWAAgent> item in (List<KeyValuePair<float, DWAAgent>>)(object)_agentNeighbors)
		{
			DWAAgent value = item.Value;
			ref DWAAgentState reference = ref value._forecastStates[sampleIndex];
			Vec2 center = state.ShapeCenter;
			ref readonly Vec2 direction = ref state.Direction;
			ref readonly Vec2 shapeHalfSize2 = ref state.ShapeHalfSize;
			DWAAgentState dWAAgentState = reference;
			float num3 = DWAHelpers.AgentToAgentSignedClearance(in center, in direction, in shapeHalfSize2, dWAAgentState.ShapeCenter, in reference.Direction, in reference.ShapeHalfSize);
			bool num4 = num3 < 0f;
			float num5 = 0f - MathF.Min(0f, num3);
			maxPenetration = MathF.Max(maxPenetration, num5);
			float num6 = ProximityCost(num3, safetyFactor);
			num += num6;
			if (num4 && collidedAgent == null)
			{
				hasCollision = true;
				collidedAgent = value;
			}
		}
		foreach (KeyValuePair<float, DWAObstacleVertex> item2 in (List<KeyValuePair<float, DWAObstacleVertex>>)(object)_obstacleNeighbors)
		{
			DWAObstacleVertex value2 = item2.Value;
			DWAHelpers.ReadStaticObstacle(value2, obstaclePolyBuffer, out var obsVertexCount);
			bool overlap;
			float num7 = DWAHelpers.AgentToConvexPolySignedClearance(state.ShapeCenter, in state.Direction, in state.ShapeHalfSize, obstaclePolyBuffer, obsVertexCount, out overlap);
			float num8 = 0f - MathF.Min(0f, num7);
			maxPenetration = MathF.Max(maxPenetration, num8);
			float signedClearDist = MathF.Max(0f, num7);
			float num9;
			if (overlap)
			{
				hasCollision = true;
				if (collidedObstacle == null)
				{
					collidedObstacle = value2;
				}
				num9 = ProximityCost(0f, safetyFactor);
			}
			else
			{
				num9 = ProximityCost(signedClearDist, safetyFactor);
			}
			num2 += num9;
		}
		proxCost = num + num2;
	}

	public (float dV, float dOmega) SelectAction(DWAThread[] threads, out int selectedActionThreadIndex, out DWAThread selectedActionThread)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.02f;
		float num2 = 1f;
		Vec2 shapeHalfSize = State.ShapeHalfSize;
		float y = ((Vec2)(ref shapeHalfSize)).Y;
		selectedActionThread = null;
		selectedActionThreadIndex = -1;
		int num3 = 0;
		float num4 = float.PositiveInfinity;
		for (int i = 0; i < threads.Length; i++)
		{
			float cost = threads[i].Cost;
			if (cost < num4)
			{
				num4 = cost;
				num3 = i;
			}
		}
		DWAThread dWAThread = threads[num3];
		(float dV, float dOmega) selectedAction = Delegate.GetSelectedAction();
		float item = selectedAction.dV;
		float item2 = selectedAction.dOmega;
		int num5 = 0;
		float num6 = float.PositiveInfinity;
		for (int j = 0; j < threads.Length; j++)
		{
			DWAThread dWAThread2 = threads[j];
			float num7 = num2 * MathF.Abs(dWAThread2.DV - item) + y * MathF.Abs(dWAThread2.DOmega - item2);
			if (num7 < num6)
			{
				num6 = num7;
				num5 = j;
			}
		}
		DWAThread dWAThread3 = threads[num5];
		if (num5 == num3)
		{
			selectedActionThreadIndex = num3;
			selectedActionThread = dWAThread;
			return (dV: dWAThread.DV, dOmega: dWAThread.DOmega);
		}
		float cost2 = dWAThread3.Cost;
		float num8 = cost2 - num4;
		float num9 = MathF.Max(1f, cost2);
		if (num8 / num9 >= num)
		{
			selectedActionThreadIndex = num3;
			selectedActionThread = dWAThread;
			return (dV: dWAThread.DV, dOmega: dWAThread.DOmega);
		}
		selectedActionThreadIndex = num5;
		selectedActionThread = dWAThread3;
		return (dV: dWAThread3.DV, dOmega: dWAThread3.DOmega);
	}

	internal void IntegrateState(in DWAAgentState curState, float dt, ref DWAAgentState newState)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		float num = dt * dt;
		Vec2 position = curState.Position;
		Vec2 direction = curState.Direction;
		Vec2 linearVelocity = curState.LinearVelocity;
		float angularVelocity = curState.AngularVelocity;
		float linearAcceleration = curState.LinearAcceleration;
		float angularAcceleration = curState.AngularAcceleration;
		Delegate.ComputeExternalAccelerationsOnState(dt, in curState, out var extLinearAcc, out var extAngularAcc);
		float num2 = angularVelocity * dt + 0.5f * angularAcceleration * num;
		Vec2 val = direction;
		((Vec2)(ref val)).RotateCCW(num2 * 0.5f);
		Vec2 val2 = linearVelocity + (linearAcceleration * val + extLinearAcc) * dt;
		float angularVelocity2 = angularVelocity + (angularAcceleration + extAngularAcc) * dt;
		Vec2 position2 = position + 0.5f * (linearVelocity + val2) * dt;
		Vec2 direction2 = direction;
		((Vec2)(ref direction2)).RotateCCW(num2);
		newState.Position = position2;
		newState.Direction = direction2;
		newState.LinearVelocity = val2;
		newState.AngularVelocity = angularVelocity2;
		newState.LinearAcceleration = curState.LinearAcceleration;
		newState.AngularAcceleration = curState.AngularAcceleration;
		newState.PositionZ = curState.PositionZ;
		newState.ShapeHalfSize = curState.ShapeHalfSize;
		newState.ShapeOffset = curState.ShapeOffset;
	}

	public static float ProximityCost(float signedClearDist, float safetyFactor = 1f)
	{
		float num = 1f;
		if (signedClearDist <= 0f)
		{
			return 1f;
		}
		float num2 = 1f / (1f + signedClearDist / safetyFactor);
		return num * num2;
	}
}
