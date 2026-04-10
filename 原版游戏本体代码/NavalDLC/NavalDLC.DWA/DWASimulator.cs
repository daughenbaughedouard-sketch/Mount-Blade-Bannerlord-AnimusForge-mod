using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;

namespace NavalDLC.DWA;

public class DWASimulator
{
	internal const int MaxObstacleVertexCount = 32;

	private MBList<DWAAgent> _agentsData;

	private MBList<DWAObstacleVertex> _obstaclesData;

	private DWAKdTree _kdTree;

	private MBList<int> _obstacleIndices;

	private MBList<int> _removedAgentIndices;

	private bool _isInitialized;

	private int _currentAgentIndexToProcess;

	private DWAAgent[] _agentsToProcess;

	private int _agentsToProcessCount;

	private DWAThread[] _processThreads;

	private DWASimulatorParameters _parameters;

	private ushort _parity;

	private readonly ParallelForAuxPredicate RunSampleThreadsAuxParallelPredicate;

	public bool IsInitialized => _isInitialized;

	internal ref readonly DWASimulatorParameters Parameters => ref _parameters;

	public int NumAgents => ((List<DWAAgent>)(object)_agentsData).Count - ((List<int>)(object)_removedAgentIndices).Count;

	public int NumObstacles => ((List<DWAObstacleVertex>)(object)_obstaclesData).Count;

	internal MBReadOnlyList<DWAAgent> AgentsIncludingRemoved => (MBReadOnlyList<DWAAgent>)(object)_agentsData;

	internal MBReadOnlyList<DWAObstacleVertex> Obstacles => (MBReadOnlyList<DWAObstacleVertex>)(object)_obstaclesData;

	public DWASimulator()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		_agentsData = new MBList<DWAAgent>();
		_obstaclesData = new MBList<DWAObstacleVertex>();
		_obstacleIndices = new MBList<int>();
		_removedAgentIndices = new MBList<int>();
		SetParameters(DWASimulatorParameters.Create());
		_kdTree = new DWAKdTree(this);
		RunSampleThreadsAuxParallelPredicate = new ParallelForAuxPredicate(RunSampleThreadsAuxParallel);
		_parity = 0;
	}

	public void SetParameters(in DWASimulatorParameters newParameters)
	{
		_parameters.CopyFrom(in newParameters);
		if (!_parameters.CheckRequiresUpdate(reset: true))
		{
			return;
		}
		_agentsToProcessCount = 0;
		_agentsToProcess = new DWAAgent[_parameters.AgentsToProcessPerTick];
		_currentAgentIndexToProcess = 0;
		_processThreads = new DWAThread[_parameters.TotalNumAccelerationSamples];
		for (int i = 0; i < _processThreads.Length; i++)
		{
			_processThreads[i] = new DWAThread(i);
		}
		foreach (DWAAgent item in (List<DWAAgent>)(object)_agentsData)
		{
			item?.SetForecastStates(_parameters.NumTimeSamples);
		}
	}

	public DWAAgentState GetAgentAgentNeighbor(int agentId, int neighborIndex)
	{
		return ((List<KeyValuePair<float, DWAAgent>>)(object)((List<DWAAgent>)(object)_agentsData)[agentId].AgentNeighbors)[neighborIndex].Value.State;
	}

	public IDWAObstacleVertex GetAgentObstacleNeighbor(int agentId, int neighborIndex)
	{
		return ((List<KeyValuePair<float, DWAObstacleVertex>>)(object)((List<DWAAgent>)(object)_agentsData)[agentId].ObstacleNeighbors)[neighborIndex].Value;
	}

	public DWAAgentState GetAgentState(int agentId)
	{
		return ((List<DWAAgent>)(object)_agentsData)[agentId].State;
	}

	public int GetAgentNumAgentNeighbors(int agentId)
	{
		return ((List<KeyValuePair<float, DWAAgent>>)(object)((List<DWAAgent>)(object)_agentsData)[agentId].AgentNeighbors).Count;
	}

	public int GetAgentNumObstacleNeighbors(int agentId)
	{
		return ((List<KeyValuePair<float, DWAObstacleVertex>>)(object)((List<DWAAgent>)(object)_agentsData)[agentId].ObstacleNeighbors).Count;
	}

	public IDWAObstacleVertex GetObstacle(int obstacleId)
	{
		return ((List<DWAObstacleVertex>)(object)_obstaclesData)[obstacleId];
	}

	public IDWAObstacleVertex GetNextObstacleOfObstacle(int obstacleId)
	{
		return ((List<DWAObstacleVertex>)(object)_obstaclesData)[obstacleId].Next;
	}

	public IDWAObstacleVertex GetPrevObstacleOfObstacle(int obstacleId)
	{
		return ((List<DWAObstacleVertex>)(object)_obstaclesData)[obstacleId].Previous;
	}

	public int AddAgent(IDWAAgentDelegate agentDelegate)
	{
		DWAAgent dWAAgent = null;
		int num;
		if (((List<int>)(object)_removedAgentIndices).Count > 0)
		{
			num = ((IEnumerable<int>)_removedAgentIndices).Last();
			((List<int>)(object)_removedAgentIndices).RemoveAt(((List<int>)(object)_removedAgentIndices).Count - 1);
			dWAAgent = new DWAAgent(this, num, agentDelegate);
			((List<DWAAgent>)(object)_agentsData)[num] = dWAAgent;
		}
		else
		{
			num = ((List<DWAAgent>)(object)_agentsData).Count;
			dWAAgent = new DWAAgent(this, num, agentDelegate);
			((List<DWAAgent>)(object)_agentsData).Add(dWAAgent);
		}
		dWAAgent.SetForecastStates(_parameters.NumTimeSamples);
		dWAAgent.Delegate.Initialize(num);
		return num;
	}

	public bool RemoveAgent(IDWAAgentDelegate agentDelegate)
	{
		for (int i = 0; i < ((List<DWAAgent>)(object)_agentsData).Count; i++)
		{
			if (((List<DWAAgent>)(object)_agentsData)[i] != null && ((List<DWAAgent>)(object)AgentsIncludingRemoved)[i].Delegate == agentDelegate)
			{
				RemoveAgent(i);
				return true;
			}
		}
		return false;
	}

	public void RemoveAgent(int agentIndex)
	{
		((List<DWAAgent>)(object)_agentsData)[agentIndex] = null;
		InsertRemovedIndex(agentIndex);
	}

	public int AddObstacle(MBList<Vec3> vertices)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		if (((List<Vec3>)(object)vertices).Count < 2)
		{
			Debug.FailedAssert("Obstacle vertex count must be greater than one", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\DWACollision\\DWASimulator.cs", "AddObstacle", 329);
			return -1;
		}
		int count = ((List<DWAObstacleVertex>)(object)_obstaclesData).Count;
		for (int i = 0; i < ((List<Vec3>)(object)vertices).Count; i++)
		{
			DWAObstacleVertex dWAObstacleVertex = new DWAObstacleVertex(((List<DWAObstacleVertex>)(object)_obstaclesData).Count);
			Vec3 val = ((List<Vec3>)(object)vertices)[i];
			dWAObstacleVertex.Point = ((Vec3)(ref val)).AsVec2;
			dWAObstacleVertex.PointZ = ((List<Vec3>)(object)vertices)[i].z;
			if (i != 0)
			{
				dWAObstacleVertex.Previous = ((List<DWAObstacleVertex>)(object)_obstaclesData)[((List<DWAObstacleVertex>)(object)_obstaclesData).Count - 1];
				dWAObstacleVertex.Previous.Next = dWAObstacleVertex;
			}
			if (i == ((List<Vec3>)(object)vertices).Count - 1)
			{
				dWAObstacleVertex.Next = ((List<DWAObstacleVertex>)(object)_obstaclesData)[count];
				dWAObstacleVertex.Next.Previous = dWAObstacleVertex;
			}
			val = ((List<Vec3>)(object)vertices)[(i != ((List<Vec3>)(object)vertices).Count - 1) ? (i + 1) : 0];
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			val = ((List<Vec3>)(object)vertices)[i];
			Vec2 val2 = asVec - ((Vec3)(ref val)).AsVec2;
			dWAObstacleVertex.Direction = ((Vec2)(ref val2)).Normalized();
			if (((List<Vec3>)(object)vertices).Count == 2)
			{
				dWAObstacleVertex.IsConvex = true;
			}
			else
			{
				val = ((List<Vec3>)(object)vertices)[(i == 0) ? (((List<Vec3>)(object)vertices).Count - 1) : (i - 1)];
				val2 = ((Vec3)(ref val)).AsVec2;
				val = ((List<Vec3>)(object)vertices)[i];
				Vec2 asVec2 = ((Vec3)(ref val)).AsVec2;
				val = ((List<Vec3>)(object)vertices)[(i != ((List<Vec3>)(object)vertices).Count - 1) ? (i + 1) : 0];
				Vec2 asVec3 = ((Vec3)(ref val)).AsVec2;
				dWAObstacleVertex.IsConvex = MBMath.GetSignedDistanceOfPointToLineSegment(ref val2, ref asVec2, ref asVec3) >= 0f;
			}
			((List<DWAObstacleVertex>)(object)_obstaclesData).Add(dWAObstacleVertex);
		}
		((List<int>)(object)_obstacleIndices).Add(count);
		return count;
	}

	public void Clear()
	{
		((List<DWAAgent>)(object)_agentsData).Clear();
		((List<DWAObstacleVertex>)(object)_obstaclesData).Clear();
		((List<int>)(object)_obstacleIndices).Clear();
		_kdTree = new DWAKdTree(this);
		((List<int>)(object)_removedAgentIndices).Clear();
		_currentAgentIndexToProcess = 0;
		_agentsToProcessCount = 0;
		for (int i = 0; i < _agentsToProcess.Length; i++)
		{
			_agentsToProcess[i] = null;
		}
		for (int j = 0; j < _processThreads.Length; j++)
		{
			_processThreads[j].Clear();
		}
		_isInitialized = false;
	}

	public void Tick(float dt)
	{
		if (!_isInitialized)
		{
			return;
		}
		_kdTree.BuildAgentTree();
		ComputeAndUpdateAgentsToProcess(_parity, ref _currentAgentIndexToProcess, out _agentsToProcessCount);
		if (_agentsToProcessCount > 0)
		{
			ComputeAndForecastNeighbors(_parity);
			DWAAgent[] agentsToProcess = _agentsToProcess;
			foreach (DWAAgent dWAAgent in agentsToProcess)
			{
				if (dWAAgent != null)
				{
					dWAAgent.InitializeThreads(in _parameters, _processThreads);
					dWAAgent.ComputeTargetOcclusion();
					TWParallel.For(0, _processThreads.Length, RunSampleThreadsAuxParallelPredicate, 16);
					var (dV, dOmega) = dWAAgent.SelectAction(_processThreads, out var _, out var _);
					dWAAgent.Delegate.UpdateSelectedAction(dV, dOmega);
				}
			}
		}
		ClearProcessThreads();
		_parity++;
	}

	public bool QueryVisibility(Vec2 point1, Vec2 point2, float radius)
	{
		return _kdTree.QueryVisibility(in point1, in point2, radius);
	}

	private void RunSampleThreadsAuxParallel(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			_processThreads[i].Run();
		}
	}

	internal void AddObstacleVertex(DWAObstacleVertex newObstacle)
	{
		((List<DWAObstacleVertex>)(object)_obstaclesData).Add(newObstacle);
	}

	internal void ComputeAgentNeighbors(DWAAgent agent, float rangeSq, ushort parity)
	{
		_kdTree.ComputeAgentNeighbors(agent, rangeSq, parity);
	}

	internal void ComputeObstacleNeighbors(DWAAgent agent, float rangeSq)
	{
		_kdTree.ComputeObstacleNeighbors(agent, rangeSq);
	}

	internal void Initialize()
	{
		_kdTree.BuildObstacleTree();
		_isInitialized = true;
	}

	private void ComputeAndUpdateAgentsToProcess(ushort parity, ref int currentAgentIndexToProcess, out int agentsToProcessCount)
	{
		agentsToProcessCount = 0;
		if (((List<DWAAgent>)(object)_agentsData).Count <= 0)
		{
			return;
		}
		int num = currentAgentIndexToProcess;
		do
		{
			DWAAgent dWAAgent = ((List<DWAAgent>)(object)_agentsData)[currentAgentIndexToProcess];
			if (dWAAgent != null && dWAAgent.Delegate.CanPlanTrajectory())
			{
				dWAAgent.TryUpdateState(parity);
				if (!dWAAgent.Delegate.HasArrivedAtTarget())
				{
					_agentsToProcess[agentsToProcessCount] = dWAAgent;
					agentsToProcessCount++;
				}
				else
				{
					dWAAgent.Delegate.UpdateSelectedAction(0f, 0f);
				}
			}
			currentAgentIndexToProcess = (currentAgentIndexToProcess + 1) % ((List<DWAAgent>)(object)_agentsData).Count;
		}
		while (agentsToProcessCount < _agentsToProcess.Length && currentAgentIndexToProcess != num);
	}

	private void ComputeAndForecastNeighbors(ushort parity)
	{
		for (int i = 0; i < _agentsToProcessCount; i++)
		{
			_agentsToProcess[i].ComputeNeighbors(parity);
			foreach (KeyValuePair<float, DWAAgent> item in (List<KeyValuePair<float, DWAAgent>>)(object)_agentsToProcess[i].AgentNeighbors)
			{
				DWAAgent value = item.Value;
				if (!value.IsForecast)
				{
					value.ForecastTrajectory(_parameters.DeltaTime, _parameters.NumTimeSamples);
				}
			}
		}
	}

	private void ClearProcessThreads()
	{
		for (int i = 0; i < _processThreads.Length; i++)
		{
			_processThreads[i].Clear();
		}
	}

	private void InsertRemovedIndex(int removedIndex)
	{
		int num = ((List<int>)(object)_removedAgentIndices).BinarySearch(removedIndex, (IComparer<int>?)Comparer<int>.Create((int a, int b) => b.CompareTo(a)));
		if (num < 0)
		{
			num = ~num;
		}
		((List<int>)(object)_removedAgentIndices).Insert(num, removedIndex);
	}
}
