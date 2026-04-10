using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.NavalPhysics;

public class NavalPhysicsAgentCollisionSolver : ScriptComponentBehavior
{
	private const float CutoffDistance = 0.6f;

	private const float CollisionAcceleration = 2f;

	private NavalPhysics _floatableEntityNavalPhysicsScript;

	private MBList<Agent> _nearbyAgentsCache;

	private Vec3[] _floatableMeshBoundingBoxGlobalVertices;

	private Vec3 _forceToBeAppliedOnFixedTick;

	private Vec3 _torqueToBeAppliedOnFixedTick;

	protected override void OnInit()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		_nearbyAgentsCache = new MBList<Agent>(5);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_floatableEntityNavalPhysicsScript = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<NavalPhysics>();
		_floatableMeshBoundingBoxGlobalVertices = (Vec3[])(object)new Vec3[8];
		_forceToBeAppliedOnFixedTick = Vec3.Zero;
		_torqueToBeAppliedOnFixedTick = Vec3.Zero;
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(0x30 | ((ScriptComponentBehavior)this).GetTickRequirement());
	}

	private bool IsPointInsideLocalBoundingBox(MatrixFrame globalFrame, Vec3 point, float margin)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref point);
		BoundingBox physicsBoundingBoxWithChildren = _floatableEntityNavalPhysicsScript.PhysicsBoundingBoxWithChildren;
		if (val.x > physicsBoundingBoxWithChildren.min.x - margin && val.y > physicsBoundingBoxWithChildren.min.y - margin && val.z > physicsBoundingBoxWithChildren.min.z - margin && val.x - margin < physicsBoundingBoxWithChildren.max.x && val.y - margin < physicsBoundingBoxWithChildren.max.y && val.z - margin < physicsBoundingBoxWithChildren.max.z)
		{
			return true;
		}
		return false;
	}

	private void UpdateFloatableMeshBoundingBoxGlobalVertices(MatrixFrame globalFrame)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		BoundingBox physicsBoundingBoxWithChildren = _floatableEntityNavalPhysicsScript.PhysicsBoundingBoxWithChildren;
		Vec3[] floatableMeshBoundingBoxGlobalVertices = _floatableMeshBoundingBoxGlobalVertices;
		Vec3 val = new Vec3(physicsBoundingBoxWithChildren.min.x, physicsBoundingBoxWithChildren.min.y, physicsBoundingBoxWithChildren.min.z, -1f);
		floatableMeshBoundingBoxGlobalVertices[0] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		Vec3[] floatableMeshBoundingBoxGlobalVertices2 = _floatableMeshBoundingBoxGlobalVertices;
		val = new Vec3(physicsBoundingBoxWithChildren.min.x, physicsBoundingBoxWithChildren.max.y, physicsBoundingBoxWithChildren.min.z, -1f);
		floatableMeshBoundingBoxGlobalVertices2[1] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		Vec3[] floatableMeshBoundingBoxGlobalVertices3 = _floatableMeshBoundingBoxGlobalVertices;
		val = new Vec3(physicsBoundingBoxWithChildren.max.x, physicsBoundingBoxWithChildren.max.y, physicsBoundingBoxWithChildren.min.z, -1f);
		floatableMeshBoundingBoxGlobalVertices3[2] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		Vec3[] floatableMeshBoundingBoxGlobalVertices4 = _floatableMeshBoundingBoxGlobalVertices;
		val = new Vec3(physicsBoundingBoxWithChildren.max.x, physicsBoundingBoxWithChildren.min.y, physicsBoundingBoxWithChildren.min.z, -1f);
		floatableMeshBoundingBoxGlobalVertices4[3] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		Vec3[] floatableMeshBoundingBoxGlobalVertices5 = _floatableMeshBoundingBoxGlobalVertices;
		val = new Vec3(physicsBoundingBoxWithChildren.min.x, physicsBoundingBoxWithChildren.min.y, physicsBoundingBoxWithChildren.max.z, -1f);
		floatableMeshBoundingBoxGlobalVertices5[4] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		Vec3[] floatableMeshBoundingBoxGlobalVertices6 = _floatableMeshBoundingBoxGlobalVertices;
		val = new Vec3(physicsBoundingBoxWithChildren.min.x, physicsBoundingBoxWithChildren.max.y, physicsBoundingBoxWithChildren.max.z, -1f);
		floatableMeshBoundingBoxGlobalVertices6[5] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		Vec3[] floatableMeshBoundingBoxGlobalVertices7 = _floatableMeshBoundingBoxGlobalVertices;
		val = new Vec3(physicsBoundingBoxWithChildren.max.x, physicsBoundingBoxWithChildren.max.y, physicsBoundingBoxWithChildren.max.z, -1f);
		floatableMeshBoundingBoxGlobalVertices7[6] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
		Vec3[] floatableMeshBoundingBoxGlobalVertices8 = _floatableMeshBoundingBoxGlobalVertices;
		val = new Vec3(physicsBoundingBoxWithChildren.max.x, physicsBoundingBoxWithChildren.min.y, physicsBoundingBoxWithChildren.max.z, -1f);
		floatableMeshBoundingBoxGlobalVertices8[7] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
	}

	protected override void OnFixedTick(float fixedDt)
	{
		if (((Vec3)(ref _forceToBeAppliedOnFixedTick)).LengthSquared > 0f)
		{
			_floatableEntityNavalPhysicsScript.ApplyForceToDynamicBody(in _forceToBeAppliedOnFixedTick, (ForceMode)0);
			_floatableEntityNavalPhysicsScript.ApplyTorque(in _torqueToBeAppliedOnFixedTick, (ForceMode)0);
		}
	}

	protected override void OnParallelFixedTick(float fixedDt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		_forceToBeAppliedOnFixedTick = Vec3.Zero;
		_torqueToBeAppliedOnFixedTick = Vec3.Zero;
		BoundingBox physicsBoundingBoxWithChildren = _floatableEntityNavalPhysicsScript.PhysicsBoundingBoxWithChildren;
		MatrixFrame globalMassFrame = _floatableEntityNavalPhysicsScript.GetGlobalMassFrame();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		UpdateFloatableMeshBoundingBoxGlobalVertices(bodyWorldTransform);
		Vec3 val = Vec3.Vec3Max(physicsBoundingBoxWithChildren.min, physicsBoundingBoxWithChildren.max);
		Mission.Current.GetNearbyAgents(((Vec3)(ref bodyWorldTransform.origin)).AsVec2, ((Vec3)(ref val)).Length + 0.6f, _nearbyAgentsCache);
		foreach (Agent item in (List<Agent>)(object)_nearbyAgentsCache)
		{
			if (!item.IsInWater())
			{
				continue;
			}
			Vec3 eyeGlobalPosition = item.GetEyeGlobalPosition();
			Vec3 val2 = Vec3.Invalid;
			float num = float.MaxValue;
			if (IsPointInsideLocalBoundingBox(bodyWorldTransform, eyeGlobalPosition, -0.05f))
			{
				Vec3 val3 = -item.Frame.rotation.f;
				float num2 = MathF.Min(_floatableEntityNavalPhysicsScript.Mass, item.GetTotalMass());
				Vec3 val4 = val3 * num2 * 2f * 5f;
				_forceToBeAppliedOnFixedTick += val4;
				continue;
			}
			for (int i = 0; i < 4; i++)
			{
				Vec3 val5 = _floatableMeshBoundingBoxGlobalVertices[i];
				Vec3 val6 = _floatableMeshBoundingBoxGlobalVertices[(i + 1) % 4];
				Vec3 closestPointOnLineSegmentToPoint = MBMath.GetClosestPointOnLineSegmentToPoint(ref val5, ref val6, ref eyeGlobalPosition);
				float num3 = ((Vec3)(ref closestPointOnLineSegmentToPoint)).DistanceSquared(eyeGlobalPosition);
				if (num3 < num)
				{
					num = num3;
					val2 = closestPointOnLineSegmentToPoint;
				}
				Vec3 val7 = _floatableMeshBoundingBoxGlobalVertices[i + 4];
				Vec3 val8 = _floatableMeshBoundingBoxGlobalVertices[(i + 1) % 4 + 4];
				Vec3 closestPointOnLineSegmentToPoint2 = MBMath.GetClosestPointOnLineSegmentToPoint(ref val7, ref val8, ref eyeGlobalPosition);
				float num4 = ((Vec3)(ref closestPointOnLineSegmentToPoint2)).DistanceSquared(eyeGlobalPosition);
				if (num4 < num)
				{
					num = num4;
					val2 = closestPointOnLineSegmentToPoint2;
				}
				Vec3 closestPointOnLineSegmentToPoint3 = MBMath.GetClosestPointOnLineSegmentToPoint(ref val5, ref val7, ref eyeGlobalPosition);
				float num5 = ((Vec3)(ref closestPointOnLineSegmentToPoint3)).DistanceSquared(eyeGlobalPosition);
				if (num5 < num)
				{
					num = num5;
					val2 = closestPointOnLineSegmentToPoint3;
				}
			}
			if (num < 0.36f)
			{
				Vec3 val9 = val2 - eyeGlobalPosition;
				float num6 = ((Vec3)(ref val9)).Normalize();
				float num7 = 0.6f - num6;
				float num8 = MathF.Min(_floatableEntityNavalPhysicsScript.Mass, item.GetTotalMass());
				Vec3 val10 = val9 * num8 * 2f / MathF.Max(0.25f, num7);
				Vec3 val11 = Vec3.CrossProduct(val2 - globalMassFrame.origin, val10);
				_forceToBeAppliedOnFixedTick += val10;
				_torqueToBeAppliedOnFixedTick += val11;
			}
		}
	}
}
