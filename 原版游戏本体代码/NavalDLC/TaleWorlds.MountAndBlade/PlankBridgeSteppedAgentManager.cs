using System.Collections.Generic;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class PlankBridgeSteppedAgentManager : ScriptComponentBehavior
{
	private Dictionary<int, float> _accumulatedCostDict;

	public Vec3 WeightedPosition { get; private set; }

	public float TotalMass { get; private set; }

	public int AgentCount { get; private set; }

	public ShipAttachmentMachine.ShipBridgeNavmeshHolder NavmeshHolder { get; private set; }

	public void SetNavmeshHolder(ShipAttachmentMachine.ShipBridgeNavmeshHolder navmeshHolder)
	{
		NavmeshHolder = navmeshHolder;
		_accumulatedCostDict = new Dictionary<int, float>();
		_accumulatedCostDict.Add(NavmeshHolder.GetFace1GroupIndex(), 0f);
	}

	protected override void OnInit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		WeightedPosition = Vec3.Zero;
		TotalMass = 0f;
		AgentCount = 0;
	}

	public void ClearAgentWeightAndPositionInformation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		WeightedPosition = Vec3.Zero;
		TotalMass = 0f;
		AgentCount = 0;
		ShipAttachmentMachine.ShipBridgeNavmeshHolder navmeshHolder = NavmeshHolder;
		if (navmeshHolder != null)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)navmeshHolder).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetCostAdderForAttachedFaces(0f);
		}
	}

	public void AddAgentWeightAndPositionInformation(Agent agent)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		float totalMass = agent.GetTotalMass();
		int currentNavigationFaceId = agent.GetCurrentNavigationFaceId();
		if (NavmeshHolder != null && _accumulatedCostDict.ContainsKey(currentNavigationFaceId))
		{
			_accumulatedCostDict[currentNavigationFaceId] += 7.5f;
			Mission.Current.SetNavigationFaceCostWithIdAroundPosition(currentNavigationFaceId, agent.Position, _accumulatedCostDict[currentNavigationFaceId]);
		}
		Vec3 position = agent.Position;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		if (((Vec3)(ref globalFrame.origin)).DistanceSquared(position) < 25f)
		{
			WeightedPosition += totalMass * agent.Position;
			TotalMass += totalMass;
			AgentCount++;
		}
	}
}
