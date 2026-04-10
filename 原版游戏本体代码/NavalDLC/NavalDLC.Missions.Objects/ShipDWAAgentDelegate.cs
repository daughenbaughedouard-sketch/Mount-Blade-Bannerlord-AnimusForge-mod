using System.Collections.Generic;
using NavalDLC.DWA;
using NavalDLC.Missions.NavalPhysics;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects;

public class ShipDWAAgentDelegate : IDWAAgentDelegate
{
	private DWAAgentState _state;

	private float _detectionRadius;

	private bool _hasTarget;

	private Vec2 _targetPos;

	private Vec2 _targetHeadingDir;

	private Vec2 _shipToTargetDir;

	private Vec2 _shipToTargetNormalDir;

	private Vec2 _shipToTargetTangentDir;

	private float _dotShipFwdToTargetHeading;

	private float _targetSpeed;

	private float _shipToTargetDistance;

	private float _timeHorizon;

	private (float dV, float dOmega) _selectedAction;

	public int Id { get; private set; }

	public MissionShip OwnerShip { get; private set; }

	public float ShapeOffsetY { get; private set; }

	public float ShapeComOffsetY { get; private set; }

	public ref readonly DWAAgentState State => ref _state;

	public float NeighborDistance => _detectionRadius;

	public float MaxLinearSpeed => OwnerShip.MissionShipObject.MaxLinearSpeed;

	public float MaxLinearAcceleration => OwnerShip.MissionShipObject.MaxLinearAccel;

	public float MaxAngularSpeed => OwnerShip.MissionShipObject.MaxAngularSpeed;

	public float MaxAngularAcceleration => OwnerShip.MissionShipObject.MaxAngularAccel;

	bool IDWAAgentDelegate.AvoidAgentCollisions
	{
		get
		{
			if (OwnerShip.IsAIControlled)
			{
				return OwnerShip.AIController.AvoidShipCollisions;
			}
			return false;
		}
	}

	bool IDWAAgentDelegate.AvoidObstacleCollisions
	{
		get
		{
			if (OwnerShip.IsAIControlled)
			{
				return OwnerShip.AIController.AvoidObstacleCollisions;
			}
			return false;
		}
	}

	public ShipDWAAgentDelegate(MissionShip ownerShip, in DWASimulatorParameters parameters)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Id = -1;
		OwnerShip = ownerShip;
		BoundingBox physicsBoundingBoxWithoutChildren = ownerShip.Physics.PhysicsBoundingBoxWithoutChildren;
		Vec3 physicsBoundingBoxSizeWithoutChildren = ownerShip.Physics.PhysicsBoundingBoxSizeWithoutChildren;
		Vec3 val = (physicsBoundingBoxWithoutChildren.min + physicsBoundingBoxWithoutChildren.max) * 0.5f;
		ShapeOffsetY = val.y;
		Vec3 localCenterOfMass = ownerShip.Physics.LocalCenterOfMass;
		ShapeComOffsetY = ShapeOffsetY - localCenterOfMass.y;
		_state.ShapeHalfSize = new Vec2(physicsBoundingBoxSizeWithoutChildren.x / 2f, physicsBoundingBoxSizeWithoutChildren.y / 2f);
		_state.ShapeOffset = new Vec2(0f, ShapeComOffsetY);
		SetTimeHorizon(parameters.TimeHorizon);
	}

	void IDWAAgentDelegate.Initialize(int id)
	{
		Id = id;
		CacheDynamicParameters();
	}

	void IDWAAgentDelegate.SetParameters(in DWASimulatorParameters parameters)
	{
		SetTimeHorizon(parameters.TimeHorizon);
	}

	float IDWAAgentDelegate.GetSafetyFactor()
	{
		return 1f;
	}

	bool IDWAAgentDelegate.CanPlanTrajectory()
	{
		if (!OwnerShip.IsAIControlled)
		{
			return false;
		}
		return true;
	}

	bool IDWAAgentDelegate.HasArrivedAtTarget()
	{
		float postionErrorSquared;
		float rotationError;
		if (_hasTarget)
		{
			return OwnerShip.AIController.HasArrivedAtTarget(out postionErrorSquared, out rotationError);
		}
		return false;
	}

	bool IDWAAgentDelegate.IsAgentEligibleNeighbor(int targetAgentId, IDWAAgentDelegate targetAgentDelegate)
	{
		foreach (MissionShip item in (List<MissionShip>)(object)OwnerShip.AIController.ShipCollisionIgnoreList)
		{
			if (item.DWAAgentId == targetAgentId)
			{
				return false;
			}
		}
		return true;
	}

	bool IDWAAgentDelegate.IsObstacleSegmentEligibleNeighbor(IDWAObstacleVertex obstacle1, IDWAObstacleVertex obstacle2)
	{
		return true;
	}

	void IDWAAgentDelegate.OnStateUpdate()
	{
		CacheDynamicParameters();
		_hasTarget = false;
		if (OwnerShip.IsAIControlled)
		{
			_hasTarget = OwnerShip.AIController.GetNextTarget(out var targetPosition, out var targetDirection, out var targetSpeed);
			if (_hasTarget)
			{
				CacheShipTrajectoryData(in targetPosition, in targetDirection, targetSpeed);
			}
		}
	}

	void IDWAAgentDelegate.UpdateSelectedAction(float dV, float dOmega)
	{
		if (OwnerShip.IsAIControlled)
		{
			OwnerShip.AIController.UpdateTrajectory(dV, dOmega);
		}
		_selectedAction = (dV: dV, dOmega: dOmega);
	}

	float IDWAAgentDelegate.GetGoalDirection(out Vec2 goalDir)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		goalDir = _shipToTargetDir;
		return _shipToTargetDistance;
	}

	(float dV, float dOmega) IDWAAgentDelegate.GetSelectedAction()
	{
		return _selectedAction;
	}

	float IDWAAgentDelegate.ComputeGoalCost(int sampleIndex, in DWAAgentState sampleState, (float distance, float amount) targetOcclusion)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasTarget)
		{
			return 0f;
		}
		float num = 16f;
		float num2 = 16f;
		float num3 = 1f;
		float num4 = 0.5f;
		float num5 = 0.1f;
		Vec2 shapeCenter = sampleState.ShapeCenter;
		Vec2 direction = sampleState.Direction;
		Vec2 linearVelocity = sampleState.LinearVelocity;
		Vec2 val2;
		Vec2 val = (val2 = _targetPos - shapeCenter);
		float num6 = ((Vec2)(ref val2)).Normalize();
		if (num6 <= 1E-06f)
		{
			val2 = _targetHeadingDir;
		}
		float distance = MathF.Abs(Vec2.DotProduct(val, _shipToTargetNormalDir));
		float x = sampleState.ShapeHalfSize.x;
		float y = sampleState.ShapeHalfSize.y;
		float num7 = 2f * y;
		float num8 = MBMath.SmoothStep(0.15f, 0.85f, targetOcclusion.amount);
		float num9 = DWAHelpers.GateNear(targetOcclusion.distance, num7, num7);
		float num10 = MathF.Clamp(num8 * num9, 0f, 1f);
		float num11 = 1f - num10;
		float num12 = _timeHorizon * MaxLinearSpeed;
		float num13 = _shipToTargetDistance - num6;
		float num14 = 0f;
		if (num13 >= 0f)
		{
			float num15 = MathF.Min(_shipToTargetDistance, num12);
			num14 = MathF.Clamp(num13 / MathF.Max(num15, 0.001f), 0f, 1f);
		}
		float num16 = 8f * (1f - num14);
		float num17 = 0f;
		if (num13 < 0f)
		{
			num17 = (0f - num13) / num12;
			num17 = MathF.Min(num17, 1f);
		}
		float num18 = num * num17;
		float num19 = MathF.Clamp(Vec2.DotProduct(direction, _targetHeadingDir), -1f, 1f);
		float num20 = MathF.Clamp(Vec2.DotProduct(direction, val2), -1f, 1f);
		float num21 = 0.5f * (1f - num19);
		float num22 = 0.5f * (1f - num20);
		float num23 = DWAHelpers.GateNear(distance, 0.5f * x, 0.5f * x);
		float num24 = num23 * num21 + (1f - num23) * num22;
		float num25 = 0.2f + 0.8f * DWAHelpers.GateNear(num6, 2.5f * num7, x);
		float num26 = num2 * (num11 * num11) * num25 * num24;
		float num27 = MathF.Clamp(Vec2.DotProduct(linearVelocity, direction) / MaxLinearSpeed, -1f, 1f);
		float num28 = DWAHelpers.GateFar(num6, 2f * num7, num7);
		float num29 = num3 * num11 * num28 * MathF.Max(0f, 0f - num27);
		float num30 = Vec2.DotProduct(linearVelocity, _targetHeadingDir);
		float num31 = MathF.Abs(_targetSpeed - num30) / MaxLinearSpeed;
		float num32 = DWAHelpers.GateNear(num6, 3f * num7, num7);
		float num33 = num4 * num11 * num32 * MathF.Clamp(num31, 0f, 1f);
		Vec2 val3 = ((ScriptComponentBehavior)OwnerShip).Scene.GetGlobalWindVelocity();
		if (((Vec2)(ref val3)).Normalize() <= 1E-06f)
		{
			val3 = _targetHeadingDir;
		}
		float num34 = MathF.Clamp(Vec2.DotProduct(direction, val3), -1f, 1f);
		float num35 = 0.5f * (1f - num34);
		float num36 = DWAHelpers.GateFar(num6, 2f * num7, num7);
		float num37 = num5 * num11 * num36 * num35;
		return num16 + num18 + num26 + num29 + num33 + num37;
	}

	void IDWAAgentDelegate.ComputeExternalAccelerationsOnState(float dt, in DWAAgentState state, out Vec2 extLinearAcc, out float extAngularAcc)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		extLinearAcc = Vec2.Zero;
		extAngularAcc = 0f;
		MatrixFrame globalFrame = new MatrixFrame
		{
			origin = state.Position3D
		};
		ref Mat3 rotation = ref globalFrame.rotation;
		Vec2 val = state.Direction;
		rotation.f = ((Vec2)(ref val)).ToVec3(0f);
		((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		NavalDLC.Missions.NavalPhysics.NavalPhysics physics = OwnerShip.Physics;
		NavalDLC.Missions.NavalPhysics.NavalPhysics.BuoyancyComputationResult buoyancyComputationResult = new NavalDLC.Missions.NavalPhysics.NavalPhysics.BuoyancyComputationResult
		{
			NetGlobalBuoyancyForce = physics.Mass * -MBGlobals.GravitationalAcceleration,
			SimulatingAirFriction = true,
			SubmergedHeightFactor = 1f,
			SubmergedFloaterCountFactor = 1f,
			PitchSubmergedAreaFactor = 1f,
			RollSubmergedAreaFactor = 1f
		};
		NavalDLC.Missions.NavalPhysics.NavalPhysics.DragForceComputationResult dragComputationResult = default(NavalDLC.Missions.NavalPhysics.NavalPhysics.DragForceComputationResult);
		MatrixFrame centerOfMassGlobalFrame = default(MatrixFrame);
		centerOfMassGlobalFrame.rotation = globalFrame.rotation;
		Vec3 localCenterOfMass = physics.LocalCenterOfMass;
		centerOfMassGlobalFrame.origin = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref localCenterOfMass);
		NavalDLC.Missions.NavalPhysics.NavalPhysics.ComputeAngularDrag(dt, 1, state.AngularVelocity * Vec3.Up, in centerOfMassGlobalFrame, physics.MassSpaceInertia, in physics.PhysicsParameters, in buoyancyComputationResult, in physics.AngularDragTerm, in physics.AngularDampingTerm, physics.AngularDragYSideComponentTerm, physics.AngularDampingYSideComponentTerm, ref dragComputationResult);
		val = state.LinearVelocity;
		NavalDLC.Missions.NavalPhysics.NavalPhysics.ComputeLinearDrag(dt, 1, ((Vec2)(ref val)).ToVec3(0f), in globalFrame, physics.Mass, physics.LocalCenterOfMass, in physics.PhysicsParameters, in buoyancyComputationResult, physics.LinearDragTerm, physics.LinearDampingTerm, physics.ConstantLinearDampingTerm, physics.MinFloaterEntitialBottomPos, physics.MaxFloaterEntitialTopPos, ref dragComputationResult, out var _);
		extLinearAcc += (((Vec3)(ref dragComputationResult.LateralDragForceGlobal)).AsVec2 + ((Vec3)(ref dragComputationResult.LongitudinalDragForceGlobal)).AsVec2) / physics.Mass;
		extAngularAcc += dragComputationResult.AngularDragTorqueGlobal.z / physics.MassSpaceInertia.z;
	}

	private void CacheDynamicParameters()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = OwnerShip.GlobalFrame;
		_state.Position = ((Vec3)(ref globalFrame.origin)).AsVec2;
		_state.PositionZ = globalFrame.origin.z;
		ref DWAAgentState state = ref _state;
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		state.Direction = ((Vec2)(ref asVec)).Normalized();
		ref DWAAgentState state2 = ref _state;
		Vec3 linearVelocity = OwnerShip.Physics.LinearVelocity;
		state2.LinearVelocity = ((Vec3)(ref linearVelocity)).AsVec2;
		_state.AngularVelocity = OwnerShip.Physics.AngularVelocity.z;
	}

	private static float ComputeDetectionRadius(float halfLength, float timeHorizon, float maxLinearSpeed)
	{
		return 4f * halfLength + timeHorizon * maxLinearSpeed;
	}

	private void CacheShipTrajectoryData(in Vec2 targetPos, in Vec2 targetDir, float targetSpeed)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		_targetPos = targetPos;
		_targetSpeed = targetSpeed;
		MatrixFrame globalFrame = OwnerShip.GlobalFrame;
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Vec2 val2 = ((Vec2)(ref val)).Normalized();
		_shipToTargetDir = _targetPos - State.Position;
		_shipToTargetDistance = ((Vec2)(ref _shipToTargetDir)).Normalize();
		if (_shipToTargetDistance <= 1E-06f)
		{
			_shipToTargetDir = val2;
			_shipToTargetDistance = 0f;
		}
		_targetHeadingDir = targetDir;
		if (((Vec2)(ref _targetHeadingDir)).Normalize() <= 1E-06f)
		{
			_targetHeadingDir = val2;
		}
		_dotShipFwdToTargetHeading = Vec2.DotProduct(val2, _targetHeadingDir);
		Vec2 val3 = State.Position - _targetPos;
		Vec2 val4 = Vec2.DotProduct(val3, _targetHeadingDir) * _targetHeadingDir;
		Vec2 val5 = val3 - val4;
		if (((Vec2)(ref val4)).LengthSquared >= 1E-05f)
		{
			_shipToTargetTangentDir = -val4;
			((Vec2)(ref _shipToTargetTangentDir)).Normalize();
		}
		else
		{
			_shipToTargetTangentDir = -_targetHeadingDir;
		}
		if (((Vec2)(ref val5)).LengthSquared >= 1E-05f)
		{
			_shipToTargetNormalDir = -val5;
			return;
		}
		val = -_targetHeadingDir;
		_shipToTargetNormalDir = ((Vec2)(ref val)).LeftVec();
	}

	private void SetTimeHorizon(float timeHorizon)
	{
		_timeHorizon = timeHorizon;
		_detectionRadius = ComputeDetectionRadius(_state.ShapeHalfSize.y, timeHorizon, OwnerShip.MissionShipObject.MaxLinearSpeed);
	}
}
