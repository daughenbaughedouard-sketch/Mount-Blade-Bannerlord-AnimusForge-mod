using System.Collections.Generic;
using System.Linq;
using NavalDLC.DWA;
using NavalDLC.Missions.Objects;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

public class NavalTrajectoryPlanningLogic : MissionLogic
{
	public const string StaticObstacleTag = "naval_static_obstacle";

	private NavalShipsLogic _navalShipsLogic;

	private DWASimulator _simulator;

	private DWASimulatorParameters _simulatorParameters;

	public override void OnBehaviorInitialize()
	{
		_simulator = new DWASimulator();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.ShipSpawnedEvent += OnShipSpawned;
		_navalShipsLogic.ShipRemovedEvent += OnShipRemoved;
		_simulatorParameters = DWASimulatorParameters.Create();
	}

	public override void OnDeploymentFinished()
	{
		Initialize();
	}

	public override void OnMissionStateFinalized()
	{
		_navalShipsLogic.ShipSpawnedEvent -= OnShipSpawned;
		_navalShipsLogic.ShipRemovedEvent -= OnShipRemoved;
		if (_simulator.IsInitialized)
		{
			_simulator.Clear();
		}
		_simulator = null;
	}

	public override void OnMissionTick(float dt)
	{
		if (((MissionBehavior)this).Mission.IsDeploymentFinished)
		{
			_simulator.Tick(dt);
		}
	}

	public void ForceReinitialize()
	{
		Initialize();
	}

	public void OnShipSpawned(MissionShip ship)
	{
		if (((MissionBehavior)this).Mission.IsDeploymentFinished)
		{
			AddShipAux(ship);
		}
	}

	public void OnShipRemoved(MissionShip ship)
	{
		if (((MissionBehavior)this).Mission.IsDeploymentFinished)
		{
			RemoveShipAux(ship);
		}
	}

	private void Initialize()
	{
		_simulator.SetParameters(in _simulatorParameters);
		if (_simulator.IsInitialized)
		{
			_simulator.Clear();
		}
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			AddShipAux(item);
		}
		List<GameEntity> staticObstacles = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("naval_static_obstacle").ToList();
		AddStaticObstacles(staticObstacles);
		_simulator.Initialize();
	}

	private void AddStaticObstacles(IReadOnlyList<GameEntity> staticObstacles)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		if (staticObstacles.Count == 0)
		{
			return;
		}
		MBList<Vec3> obj = new MBList<Vec3>();
		((List<Vec3>)(object)obj).Add(Vec3.Zero);
		((List<Vec3>)(object)obj).Add(Vec3.Zero);
		((List<Vec3>)(object)obj).Add(Vec3.Zero);
		((List<Vec3>)(object)obj).Add(Vec3.Zero);
		MBList<Vec3> val = obj;
		MatrixFrame[] array = null;
		foreach (GameEntity staticObstacle in staticObstacles)
		{
			Path pathWithName = Mission.Current.Scene.GetPathWithName(staticObstacle.Name);
			if ((NativeObject)(object)pathWithName != (NativeObject)null)
			{
				int numberOfPoints = pathWithName.NumberOfPoints;
				if (array == null || array.Length < numberOfPoints)
				{
					array = (MatrixFrame[])(object)new MatrixFrame[numberOfPoints];
				}
				pathWithName.GetPoints(array);
				Vec3 val2 = array[1].origin - array[0].origin;
				Vec2 nextDir = ((Vec3)(ref val2)).AsVec2;
				if (((Vec2)(ref nextDir)).Normalize() < 1E-05f)
				{
					nextDir = Vec2.Zero;
				}
				Vec2 val3 = ComputeOffset(in Vec2.Zero, hasPrev: false, in nextDir, hasNext: true, 1);
				Vec2 val4 = ComputeOffset(in Vec2.Zero, hasPrev: false, in nextDir, hasNext: true, -1);
				for (int i = 0; i < numberOfPoints - 1; i++)
				{
					Vec3 origin = array[i].origin;
					Vec3 origin2 = array[i + 1].origin;
					Vec2 nextDir2 = Vec2.Zero;
					if (i + 2 < numberOfPoints)
					{
						val2 = array[i + 2].origin - array[i + 1].origin;
						nextDir2 = ((Vec3)(ref val2)).AsVec2;
						if (((Vec2)(ref nextDir2)).Normalize() < 1E-05f)
						{
							nextDir2 = Vec2.Zero;
						}
					}
					bool hasPrev = ((Vec2)(ref nextDir)).LengthSquared > 1E-05f;
					bool hasNext = ((Vec2)(ref nextDir2)).LengthSquared > 1E-05f;
					Vec2 val5 = ComputeOffset(in nextDir, hasPrev, in nextDir2, hasNext, 1);
					Vec2 val6 = ComputeOffset(in nextDir, hasPrev, in nextDir2, hasNext, -1);
					Vec3 value = origin + ((Vec2)(ref val3)).ToVec3(0f);
					Vec3 value2 = origin + ((Vec2)(ref val4)).ToVec3(0f);
					Vec3 value3 = origin2 + ((Vec2)(ref val5)).ToVec3(0f);
					Vec3 value4 = origin2 + ((Vec2)(ref val6)).ToVec3(0f);
					((List<Vec3>)(object)val)[0] = value2;
					((List<Vec3>)(object)val)[1] = value;
					((List<Vec3>)(object)val)[2] = value3;
					((List<Vec3>)(object)val)[3] = value4;
					_simulator.AddObstacle(val);
					val3 = val5;
					val4 = val6;
					nextDir = nextDir2;
				}
				continue;
			}
			IOrderedEnumerable<GameEntity> orderedEnumerable = from entity in staticObstacle.GetChildren()
				orderby entity.Name
				select entity;
			MBList<Vec3> val7 = new MBList<Vec3>();
			foreach (GameEntity item in orderedEnumerable)
			{
				Vec3 origin3 = item.GetGlobalFrame().origin;
				((List<Vec3>)(object)val7).Add(origin3);
			}
			MBSceneUtilities.RadialSortBoundary(ref val7);
			_simulator.AddObstacle(val7);
		}
		static Vec2 ComputeOffset(in Vec2 prevDir, bool flag, in Vec2 reference, bool flag2, int sideSign)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			if (!flag && !flag2)
			{
				return Vec2.Zero;
			}
			Vec2 val8;
			if (flag && !flag2)
			{
				val8 = prevDir;
				Vec2 val9 = ((Vec2)(ref val8)).RightVec() * (float)sideSign;
				if (((Vec2)(ref val9)).LengthSquared > 1E-05f)
				{
					val9 = ((Vec2)(ref val9)).Normalized();
				}
				return val9 * 8f;
			}
			if (!flag && flag2)
			{
				val8 = reference;
				Vec2 val10 = ((Vec2)(ref val8)).RightVec() * (float)sideSign;
				if (((Vec2)(ref val10)).LengthSquared > 1E-05f)
				{
					val10 = ((Vec2)(ref val10)).Normalized();
				}
				return val10 * 8f;
			}
			val8 = prevDir;
			Vec2 val11 = ((Vec2)(ref val8)).RightVec() * (float)sideSign;
			val8 = reference;
			Vec2 val12 = ((Vec2)(ref val8)).RightVec() * (float)sideSign;
			bool flag3 = ((Vec2)(ref val11)).LengthSquared > 1E-05f;
			bool flag4 = ((Vec2)(ref val12)).LengthSquared > 1E-05f;
			if (!flag3 && !flag4)
			{
				return Vec2.Zero;
			}
			if (!flag3)
			{
				val12 = ((Vec2)(ref val12)).Normalized();
				return val12 * 8f;
			}
			if (!flag4)
			{
				val11 = ((Vec2)(ref val11)).Normalized();
				return val11 * 8f;
			}
			Vec2 val13 = val11 + val12;
			float lengthSquared = ((Vec2)(ref val13)).LengthSquared;
			if (lengthSquared <= 1E-05f)
			{
				return ((Vec2)(ref val12)).Normalized() * 8f;
			}
			val13 /= MathF.Sqrt(lengthSquared);
			Vec2 val14 = ((Vec2)(ref val12)).Normalized();
			float num = MathF.Abs(Vec2.DotProduct(val13, val14));
			if (num <= 1E-05f)
			{
				return val14 * 8f;
			}
			float num2 = 8f / num;
			float num3 = 32f;
			if (num2 > num3)
			{
				num2 = num3;
			}
			else if (num2 < 0f - num3)
			{
				num2 = 0f - num3;
			}
			return val13 * num2;
		}
	}

	private void AddShipAux(MissionShip ship)
	{
		IDWAAgentDelegate agentDelegate = ship.CreateDWAAgent(in _simulator.Parameters);
		_simulator.AddAgent(agentDelegate);
	}

	private void RemoveShipAux(MissionShip ship)
	{
		_simulator.RemoveAgent(ship.DWAAgentId);
	}
}
