using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Deployment;

public class NavalDeploymentPlan
{
	public const float HorizontalShipGap = 20f;

	public const float DeployZoneMinimumWidth = 400f;

	public const float RiverSceneDeployZoneFixedWidth = 200f;

	public const float DeployZoneForwardMargin = 50f;

	public const float DeployZoneBackwardMargin = 100f;

	public readonly Team Team;

	public readonly SpawnPathData SpawnPathData;

	private readonly Mission _mission;

	private int _planCount;

	private bool _isRiverScene;

	private readonly NavalFormationDeploymentPlan[] _formationPlans;

	private MatrixFrame _deploymentFrame;

	private float _deploymentWidth;

	private float _deploymentDepth;

	private MBList<Vec2> _meanBoundaryPositions;

	private readonly MBList<(string id, MBList<Vec2> points)> _deploymentBoundaries;

	public int PlanCount => _planCount;

	public bool IsPlanMade { get; private set; }

	public float SpawnPathOffset { get; private set; }

	public bool HasDeploymentBoundaries { get; private set; }

	public int TroopCount
	{
		get
		{
			int num = 0;
			NavalFormationDeploymentPlan[] formationPlans = _formationPlans;
			foreach (NavalFormationDeploymentPlan navalFormationDeploymentPlan in formationPlans)
			{
				num += navalFormationDeploymentPlan.PlannedTroopCount;
			}
			return num;
		}
	}

	public MatrixFrame DeploymentFrame
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			UpdateDeploymentFrameZ();
			return _deploymentFrame;
		}
	}

	public float DeploymentWidth => _deploymentWidth;

	public float DeploymentDepth => _deploymentDepth;

	public MBReadOnlyList<(string, MBList<Vec2>)> DeploymentBoundaries => (MBReadOnlyList<(string, MBList<Vec2>)>)(object)_deploymentBoundaries;

	public static NavalDeploymentPlan CreatePlan(Mission mission, Team team)
	{
		return new NavalDeploymentPlan(mission, team, null);
	}

	private NavalDeploymentPlan(Mission mission, Team team, SpawnPathData spawnPathData)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		_mission = mission;
		_planCount = 0;
		Team = team;
		SpawnPathData = spawnPathData;
		_formationPlans = new NavalFormationDeploymentPlan[11];
		_deploymentFrame = MatrixFrame.Identity;
		_deploymentWidth = 0f;
		_deploymentDepth = 0f;
		_deploymentBoundaries = new MBList<(string, MBList<Vec2>)>();
		_meanBoundaryPositions = new MBList<Vec2>();
		IsPlanMade = false;
		SpawnPathOffset = 0f;
		for (int i = 0; i < _formationPlans.Length; i++)
		{
			FormationClass fClass = (FormationClass)i;
			_formationPlans[i] = new NavalFormationDeploymentPlan(fClass, _mission);
		}
		ClearAddedShips();
		ClearPlan();
	}

	public void ClearAddedShips()
	{
		NavalFormationDeploymentPlan[] formationPlans = _formationPlans;
		for (int i = 0; i < formationPlans.Length; i++)
		{
			formationPlans[i].SetShipOrigin(null);
		}
	}

	public void ClearPlan()
	{
		NavalFormationDeploymentPlan[] formationPlans = _formationPlans;
		for (int i = 0; i < formationPlans.Length; i++)
		{
			formationPlans[i].Clear();
		}
		IsPlanMade = false;
		((List<(string, MBList<Vec2>)>)(object)_deploymentBoundaries).Clear();
		((List<Vec2>)(object)_meanBoundaryPositions).Clear();
		HasDeploymentBoundaries = false;
	}

	public void AddShip(FormationClass formationClass, IShipOrigin shipOrigin)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		int num = (int)formationClass;
		_formationPlans[num].SetShipOrigin(shipOrigin);
	}

	public bool RemoveShip(FormationClass formationIndex)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NavalFormationDeploymentPlan navalFormationDeploymentPlan = _formationPlans[formationIndex];
		if (navalFormationDeploymentPlan.ShipObject != null)
		{
			navalFormationDeploymentPlan.SetShipOrigin(null);
			return true;
		}
		return false;
	}

	public void MakeDeploymentPlan(float spawnPathOffset, float targetOffset)
	{
		SpawnPathOffset = spawnPathOffset;
		PlanNavalBattleDeploymentFromSpawnPath(spawnPathOffset, targetOffset);
		PlanDeploymentZone();
	}

	public NavalFormationDeploymentPlan GetFormationPlan(FormationClass fClass)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _formationPlans[fClass];
	}

	public bool GetFormationDeploymentFrame(FormationClass fClass, out MatrixFrame frame)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		NavalFormationDeploymentPlan formationPlan = GetFormationPlan(fClass);
		if (formationPlan.HasFrame())
		{
			frame = formationPlan.GetFrame();
			return true;
		}
		frame = MatrixFrame.Identity;
		return false;
	}

	public bool IsPositionInsideDeploymentBoundaries(in Vec2 position)
	{
		bool result = false;
		foreach (var item2 in (List<(string, MBList<Vec2>)>)(object)_deploymentBoundaries)
		{
			MBList<Vec2> item = item2.Item2;
			if (MBSceneUtilities.IsPointInsideBoundaries(ref position, item, 0.05f))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public Vec2 GetClosestBoundaryPosition(in Vec2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Vec2 result = position;
		float num = float.MaxValue;
		Vec2 val = default(Vec2);
		foreach (var item2 in (List<(string, MBList<Vec2>)>)(object)_deploymentBoundaries)
		{
			MBList<Vec2> item = item2.Item2;
			if (((List<Vec2>)(object)item).Count > 2)
			{
				float num2 = MBSceneUtilities.FindClosestPointToBoundaries(ref position, item, ref val);
				if (num2 < num)
				{
					num = num2;
					result = val;
				}
			}
		}
		return result;
	}

	private void PlanNavalBattleDeploymentFromSpawnPath(float pathOffset, float targetOffset)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		SpawnPathData initialSpawnPathData = _mission.GetInitialSpawnPathData(Team.Side);
		_isRiverScene = _mission.Scene.GetNavmeshFaceCountBetweenTwoIds(1, 1) > 0;
		Vec2 deployPosition = default(Vec2);
		Vec2 deployDirection = default(Vec2);
		initialSpawnPathData.GetSpawnPathFrameFacingTarget(pathOffset, targetOffset, _isRiverScene, ref deployPosition, ref deployDirection, false, 0.2f);
		DeployShips(deployPosition, deployDirection);
		IsPlanMade = true;
		_planCount++;
	}

	private void PlanDeploymentZone()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.Zero;
		Vec2 val2 = Vec2.Zero;
		int num = 0;
		NavalFormationDeploymentPlan[] formationPlans = _formationPlans;
		foreach (NavalFormationDeploymentPlan navalFormationDeploymentPlan in formationPlans)
		{
			if (navalFormationDeploymentPlan.HasFrame())
			{
				val += navalFormationDeploymentPlan.GetPosition();
				val2 += navalFormationDeploymentPlan.GetDirection();
				num++;
			}
		}
		val /= (float)num;
		Vec3 val3 = ((Vec2)(ref val2)).ToVec3(0f);
		val3 = ((Vec3)(ref val3)).NormalizedCopy();
		Mat3 val4 = Mat3.CreateMat3WithForward(ref val3);
		_deploymentFrame = new MatrixFrame(ref val4, ref val);
		float num2 = 0f;
		float num3 = 0f;
		for (int j = 0; j < 10; j++)
		{
			FormationClass fClass = (FormationClass)j;
			NavalFormationDeploymentPlan formationPlan = GetFormationPlan(fClass);
			if (formationPlan.HasFrame())
			{
				ref MatrixFrame deploymentFrame = ref _deploymentFrame;
				MatrixFrame frame = formationPlan.GetFrame();
				MatrixFrame val5 = ((MatrixFrame)(ref deploymentFrame)).TransformToLocal(ref frame);
				num2 = Math.Max(val5.origin.y, num2);
				num3 = Math.Max(Math.Abs(val5.origin.x), num3);
			}
		}
		num2 += 50f;
		((MatrixFrame)(ref _deploymentFrame)).Advance(num2);
		((List<(string, MBList<Vec2>)>)(object)_deploymentBoundaries).Clear();
		((List<Vec2>)(object)_meanBoundaryPositions).Clear();
		float val6 = 2f * num3;
		if (_isRiverScene)
		{
			_deploymentWidth = 200f;
		}
		else
		{
			_deploymentWidth = Math.Max(val6, 400f);
		}
		_deploymentDepth = 150f;
		Vec2 item = default(Vec2);
		foreach (KeyValuePair<string, ICollection<Vec2>> boundary in _mission.Boundaries)
		{
			string key = boundary.Key;
			ICollection<Vec2> value = boundary.Value;
			MBList<Vec2> val7 = ComputeDeploymentBoundariesFromMissionBoundaries(value);
			((List<(string, MBList<Vec2>)>)(object)_deploymentBoundaries).Add((key, val7));
			((Vec2)(ref item))._002Ector(((IEnumerable<Vec2>)val7).Average((Vec2 v) => v.x), ((IEnumerable<Vec2>)val7).Average((Vec2 v) => v.y));
			((List<Vec2>)(object)_meanBoundaryPositions).Add(item);
		}
		UpdateDeploymentFrameZ();
		HasDeploymentBoundaries = true;
	}

	private void DeployShips(Vec2 deployPosition, Vec2 deployDirection)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		List<(int, NavalFormationDeploymentPlan)> list = new List<(int, NavalFormationDeploymentPlan)>();
		for (int i = 0; i < _formationPlans.Count(); i++)
		{
			NavalFormationDeploymentPlan navalFormationDeploymentPlan = _formationPlans[i];
			if (navalFormationDeploymentPlan.HasShipObject)
			{
				int totalCrewCapacity = navalFormationDeploymentPlan.ShipOrigin.TotalCrewCapacity;
				list.Add((totalCrewCapacity, navalFormationDeploymentPlan));
			}
		}
		list.Sort(((int crewCapacity, NavalFormationDeploymentPlan plan) x, (int crewCapacity, NavalFormationDeploymentPlan plan) y) => y.crewCapacity.CompareTo(x.crewCapacity));
		float num = 0f;
		float num2 = 0f;
		Vec2 val = ((Vec2)(ref deployDirection)).LeftVec();
		Vec2 val2 = ((Vec2)(ref val)).Normalized();
		Vec2 val3 = -val2;
		int num3 = 0;
		if (list.Count % 2 != 0)
		{
			NavalFormationDeploymentPlan item = list[num3].Item2;
			item.SetFrame(in deployPosition, in deployDirection);
			float num4 = item.ShipObject.DeploymentArea.x / 2f;
			num += num4;
			num2 += num4;
			num3++;
		}
		for (; num3 < list.Count; num3++)
		{
			NavalFormationDeploymentPlan item2 = list[num3].Item2;
			float num5 = item2.ShipObject.DeploymentArea.x / 2f;
			if (num3 % 2 == 0)
			{
				num2 += 20f + num5;
				item2.SetFrame(deployPosition + val3 * num2, in deployDirection);
				num2 += num5;
			}
			else
			{
				num += 20f + num5;
				item2.SetFrame(deployPosition + val2 * num, in deployDirection);
				num += num5;
			}
		}
		list.Clear();
	}

	private MBList<Vec2> ComputeDeploymentBoundariesFromMissionBoundaries(ICollection<Vec2> missionBoundaries)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		MBList<Vec2> val = new MBList<Vec2>();
		if (missionBoundaries.Count > 2)
		{
			Vec2 asVec = ((Vec3)(ref _deploymentFrame.origin)).AsVec2;
			Vec2 val2 = ((Vec3)(ref _deploymentFrame.rotation.s)).AsVec2;
			Vec2 val3 = ((Vec2)(ref val2)).Normalized();
			val2 = ((Vec3)(ref _deploymentFrame.rotation.f)).AsVec2;
			Vec2 val4 = ((Vec2)(ref val2)).Normalized();
			Vec2 val5 = asVec - _deploymentDepth / 2f * val4;
			MBList<Vec2> val6 = new MBList<Vec2>();
			Vec2 val7 = asVec - _deploymentWidth / 2f * val3;
			((List<Vec2>)(object)val6).Add(val7);
			Vec2 val8 = val7 - val4 * _deploymentDepth;
			((List<Vec2>)(object)val6).Add(val8);
			Vec2 val9 = val8 + val3 * _deploymentWidth;
			((List<Vec2>)(object)val6).Add(val9);
			Vec2 item = val9 + val4 * _deploymentDepth;
			((List<Vec2>)(object)val6).Add(item);
			MBList<Vec2> val10 = Extensions.ToMBList<Vec2>((IEnumerable<Vec2>)missionBoundaries);
			Vec2 point = default(Vec2);
			Vec2 point2 = default(Vec2);
			foreach (Vec2 item2 in (List<Vec2>)(object)val6)
			{
				Vec2 current = item2;
				if (MBSceneUtilities.IsPointInsideBoundaries(ref current, val10, 0.05f))
				{
					AddDeploymentBoundaryPoint(val, current);
					continue;
				}
				val2 = val5 - current;
				Vec2 val11 = ((Vec2)(ref val2)).Normalized();
				Vec2 val12 = ((Vec2.DotProduct(val11, val3) >= 0f) ? val3 : (-val3));
				if (MBMath.IntersectRayWithPolygon(current, val12, val10, ref point))
				{
					AddDeploymentBoundaryPoint(val, point);
				}
				Vec2 val13 = ((Vec2.DotProduct(val11, val4) >= 0f) ? val4 : (-val4));
				if (MBMath.IntersectRayWithPolygon(current, val13, val10, ref point2))
				{
					AddDeploymentBoundaryPoint(val, point2);
				}
			}
			foreach (Vec2 item3 in (List<Vec2>)(object)val10)
			{
				Vec2 current2 = item3;
				if (MBSceneUtilities.IsPointInsideBoundaries(ref current2, val6, 0.05f))
				{
					AddDeploymentBoundaryPoint(val, current2);
				}
			}
			MBSceneUtilities.RadialSortBoundary(ref val);
			MBSceneUtilities.FindConvexHull(ref val);
		}
		return val;
	}

	private void AddDeploymentBoundaryPoint(MBList<Vec2> deploymentBoundaries, Vec2 point)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!((List<Vec2>)(object)deploymentBoundaries).Exists((Predicate<Vec2>)((Vec2 boundaryPoint) => ((Vec2)(ref boundaryPoint)).Distance(point) <= 0.1f)))
		{
			((List<Vec2>)(object)deploymentBoundaries).Add(point);
		}
	}

	private Vec2 ClampRayToMissionBoundaries(MBList<Vec2> boundaries, Vec2 origin, Vec2 direction, float maxLength)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (_mission.IsPositionInsideBoundaries(origin))
		{
			Vec2 val = origin + direction * maxLength;
			if (_mission.IsPositionInsideBoundaries(val))
			{
				return val;
			}
		}
		Vec2 result = default(Vec2);
		if (MBMath.IntersectRayWithPolygon(origin, direction, boundaries, ref result))
		{
			return result;
		}
		return origin;
	}

	private void UpdateDeploymentFrameZ()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		_deploymentFrame.origin.z = _mission.Scene.GetWaterLevelAtPosition(((Vec3)(ref _deploymentFrame.origin)).AsVec2, true, false);
	}

	internal Vec2 GetMeanBoundaryPosition(int boundaryIndex = 0)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((List<Vec2>)(object)_meanBoundaryPositions)[boundaryIndex];
	}
}
