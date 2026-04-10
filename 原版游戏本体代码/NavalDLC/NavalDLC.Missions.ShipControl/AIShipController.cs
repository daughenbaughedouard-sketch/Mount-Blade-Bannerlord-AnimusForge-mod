using System;
using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.ShipInput;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.ShipControl;

public class AIShipController : ShipController
{
	public enum TargetMode
	{
		None,
		Position,
		State,
		Ship,
		ShipOffset
	}

	public const float ProportionalControllerSamplingPeriod = 1f / 30f;

	private const float LateralInputAccelerationThreshold = 0.01f;

	private const float LongitudinalInputAccelerationThreshold = 0.01f;

	private const float RaisedSailInputThresholdMultiplier = 0.2f;

	private const float FullSailInputThresholdMultiplier = 0.6f;

	private TargetMode _targetMode;

	private NavalState _targetState;

	private NavalVec _targetOffset;

	private bool _stopOnArrival;

	private bool _ignoreTargetShipCollision;

	private uint _rowerLateralDebounceCounter;

	private uint _rowerLongitudinalDebounceCounter;

	private uint _rudderLateralDebounceCounter;

	private uint _sailDebounceCounter;

	private ShipInputRecord _inputRecord;

	private NavalShipsLogic _navalShipsLogic;

	private NavigationPath _navigationPath;

	private int _lastNavPathPointIndex = -1;

	private UIntPtr _lastNavPathStartFace;

	private UIntPtr _lastNavPathTargetFace;

	private Vec2 _lastNavPathTargetPosition;

	private float _navPathTargetDriftAccumulator;

	private float _lastNavPathHardRecomputeTime;

	private bool _collisionChecksActive = true;

	private bool _avoidShipCollisions = true;

	private bool _avoidObstacleCollisions = true;

	private MBList<MissionShip> _shipCollisionIgnoreList = new MBList<MissionShip>();

	public MissionShip TargetShip { get; private set; }

	internal MBReadOnlyList<MissionShip> ShipCollisionIgnoreList => (MBReadOnlyList<MissionShip>)(object)_shipCollisionIgnoreList;

	public bool CanAvoidCollisions
	{
		get
		{
			if (_ownerShip.HasDWAAgent && CollisionChecksActive)
			{
				if (!AvoidShipCollisions)
				{
					return AvoidObstacleCollisions;
				}
				return true;
			}
			return false;
		}
	}

	internal bool CollisionChecksActive => _collisionChecksActive;

	internal bool AvoidShipCollisions => _avoidShipCollisions;

	internal bool AvoidObstacleCollisions => _avoidObstacleCollisions;

	internal float DesiredLinearAcceleration { get; private set; }

	internal float DesiredAngularAcceleration { get; private set; }

	public bool HasTarget => _targetMode != TargetMode.None;

	public bool HasNavigationPath
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			if (_navigationPath == null && _navalShipsLogic.SceneHasNavMeshForPathFinding)
			{
				_navigationPath = new NavigationPath();
				_lastNavPathPointIndex = -1;
			}
			return _navigationPath != null;
		}
	}

	public AIShipController(MissionShip ownerShip)
		: base(ownerShip)
	{
		_controllerType = ShipControllerType.AI;
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		ClearTarget();
	}

	public override ShipInputRecord Update(float dt)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		ShipInputRecord inputRecord = ShipInputRecord.None();
		if (UpdateTargetState())
		{
			float postionErrorSquared;
			float rotationError;
			bool flag = HasArrivedAtTarget(out postionErrorSquared, out rotationError);
			if (_stopOnArrival && flag)
			{
				ClearTarget();
				inputRecord = ShipInputRecord.Stop();
			}
			else if (!flag)
			{
				ShipInputRecord oldInputRecord = inputRecord;
				MatrixFrame globalFrame = _ownerShip.GlobalFrame;
				Vec2 globalWindVelocity = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				Vec2 shipForward2D = ((Vec2)(ref globalWindVelocity)).Normalized();
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
				globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				ref Mat3 rotation = ref globalFrame.rotation;
				Vec3 linearVelocity = _ownerShip.Physics.LinearVelocity;
				Vec3 shipLocalVelocity = ((Mat3)(ref rotation)).TransformToLocal(ref linearVelocity);
				globalWindVelocity = ((ScriptComponentBehavior)_ownerShip).Scene.GetGlobalWindVelocity();
				DecideControl(in oldInputRecord, in shipForward2D, in globalWindVelocity, DesiredAngularAcceleration, DesiredLinearAcceleration, _ownerShip.MissionShipObject.MaxLinearAccel, _ownerShip.MissionShipObject.MaxAngularAccel, out inputRecord, shipLocalVelocity, _ownerShip.ShipOrder.EnforceSailUsage);
			}
		}
		_inputRecord = inputRecord;
		return _inputRecord;
	}

	public void SetTargetPosition(in Vec2 targetPosition, bool stopOnArrival = false)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		_targetMode = TargetMode.Position;
		SetTargetShipAux(null);
		_targetOffset = NavalVec.Zero;
		_stopOnArrival = stopOnArrival;
		MatrixFrame globalFrame = _ownerShip.GlobalFrame;
		Vec2 direction = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		direction = ((Vec2)(ref direction)).Normalized();
		NavalState newTargetState = new NavalState(in targetPosition, in direction);
		if (HasNavigationPath)
		{
			ReComputeNavigationPath(_ownerShip.GetNavalState(), in newTargetState);
		}
		_targetState = newTargetState;
	}

	public void SetTargetState(in Vec2 targetPosition, in Vec2 targetDirection, bool stopOnArrival = false)
	{
		_targetMode = TargetMode.State;
		SetTargetShipAux(null);
		_targetOffset = NavalVec.Zero;
		_stopOnArrival = stopOnArrival;
		NavalState newTargetState = new NavalState(in targetPosition, in targetDirection);
		if (HasNavigationPath)
		{
			ReComputeNavigationPath(_ownerShip.GetNavalState(), in newTargetState);
		}
		_targetState = newTargetState;
	}

	public void SetTargetState(in NavalState targetState, bool stopOnArrival = false)
	{
		_targetMode = TargetMode.State;
		SetTargetShipAux(null);
		_targetOffset = NavalVec.Zero;
		_stopOnArrival = stopOnArrival;
		if (HasNavigationPath)
		{
			ReComputeNavigationPath(_ownerShip.GetNavalState(), in targetState);
		}
		_targetState = targetState;
	}

	public void SetTargetShip(in MissionShip targetShip, bool stopOnArrival = false, bool ignoreTargetShipCollision = false)
	{
		_targetMode = TargetMode.Ship;
		SetTargetShipAux(targetShip, ignoreTargetShipCollision);
		_targetOffset = NavalVec.Zero;
		_stopOnArrival = stopOnArrival;
		NavalState newTargetState = TargetShip.GetNavalState();
		if (HasNavigationPath)
		{
			ReComputeNavigationPath(_ownerShip.GetNavalState(), in newTargetState);
		}
		_targetState = newTargetState;
	}

	public void SetTargetShipWithOffset(in MissionShip targetShip, in NavalVec localOffset, bool stopOnArrival = false, bool ignoreTargetShipCollision = false)
	{
		_targetMode = TargetMode.ShipOffset;
		SetTargetShipAux(targetShip, ignoreTargetShipCollision);
		_targetOffset = localOffset;
		_stopOnArrival = stopOnArrival;
		NavalState newTargetState = TargetShip.GetNavalState(in localOffset);
		if (HasNavigationPath)
		{
			ReComputeNavigationPath(_ownerShip.GetNavalState(), in newTargetState);
		}
		_targetState = newTargetState;
	}

	internal void AddShipToCollisionIgnoreListOnAccountOfRamming(MissionShip ship)
	{
		AddShipToCollisionIgnoreList(ship);
	}

	internal void AddShipToCollisionIgnoreList(MissionShip ship)
	{
		if (!((List<MissionShip>)(object)_shipCollisionIgnoreList).Contains(ship))
		{
			((List<MissionShip>)(object)_shipCollisionIgnoreList).Add(ship);
		}
	}

	internal void SetAvoidShipCollisions(bool value = true)
	{
		_avoidShipCollisions = value;
	}

	internal void RemoveShipFromCollisionIgnoreListOnAccountOfRamming(MissionShip ship)
	{
		RemoveShipFromCollisionIgnoreList(ship);
	}

	internal void RemoveShipFromCollisionIgnoreList(MissionShip ship)
	{
		((List<MissionShip>)(object)_shipCollisionIgnoreList).Remove(ship);
	}

	internal void SetAvoidObstacleCollisions(bool value = true)
	{
		_avoidObstacleCollisions = value;
	}

	internal void SetCollisionChecksActive(bool value = true)
	{
		_collisionChecksActive = value;
	}

	internal void ClearShipCollisionIgnoreList()
	{
		((List<MissionShip>)(object)_shipCollisionIgnoreList).Clear();
	}

	internal bool CheckShipInCollisionIgnoreList(MissionShip ship)
	{
		return ((List<MissionShip>)(object)_shipCollisionIgnoreList).Contains(ship);
	}

	public bool GetRawTargetState(out Vec2 targetPosition, out Vec2 targetDirection, out float targetSpeed)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (_targetMode != TargetMode.None)
		{
			targetPosition = _targetState.Position;
			targetDirection = _targetState.Direction;
			targetSpeed = _targetState.Speed;
			return true;
		}
		targetPosition = Vec2.Invalid;
		targetDirection = Vec2.Invalid;
		targetSpeed = 0f;
		return false;
	}

	public bool GetNextTarget(out Vec2 targetPosition, out Vec2 targetDirection, out float targetSpeed)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (_targetMode != TargetMode.None)
		{
			if (HasNavigationPath && _navigationPath.Size > 0)
			{
				NavalState nextTargetStateOverPath = GetNextTargetStateOverPath();
				targetPosition = nextTargetStateOverPath.Position;
				targetDirection = nextTargetStateOverPath.Direction;
				targetSpeed = _targetState.Speed;
				return true;
			}
			return GetRawTargetState(out targetPosition, out targetDirection, out targetSpeed);
		}
		targetPosition = Vec2.Invalid;
		targetDirection = Vec2.Invalid;
		targetSpeed = 0f;
		return false;
	}

	public bool HasArrivedAtTarget(out float postionErrorSquared, out float rotationError)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		float num = _ownerShip.Physics.PhysicsBoundingBoxSizeWithoutChildren.y / 20f;
		float num2 = MathF.PI / 12f;
		NavalState fromState = _ownerShip.GetNavalState();
		NavalVec navalVec = _targetState - fromState;
		Vec2 deltaPosition = navalVec.DeltaPosition;
		postionErrorSquared = ((Vec2)(ref deltaPosition)).LengthSquared;
		rotationError = MathF.Abs(navalVec.DeltaOrientation);
		if (postionErrorSquared < num * num && rotationError < num2)
		{
			return true;
		}
		return false;
	}

	internal void UpdateTrajectory(float desiredLinearAcceleration, float desiredAngularAcceleration)
	{
		DesiredLinearAcceleration = desiredLinearAcceleration;
		DesiredAngularAcceleration = desiredAngularAcceleration;
	}

	public void ClearTarget()
	{
		if (_ignoreTargetShipCollision)
		{
			RemoveShipFromCollisionIgnoreList(TargetShip);
			_ignoreTargetShipCollision = false;
		}
		TargetShip = null;
		_targetMode = TargetMode.None;
		_targetState = NavalState.Zero;
		if (HasNavigationPath)
		{
			ClearNavigationPathAux();
		}
		_targetOffset = NavalVec.Zero;
		_stopOnArrival = false;
	}

	public bool UpdateTargetState()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (_targetMode != TargetMode.None)
		{
			NavalState currentState = _ownerShip.GetNavalState();
			if (_targetMode == TargetMode.Position)
			{
				ref NavalState targetState = ref _targetState;
				MatrixFrame globalFrame = _ownerShip.GlobalFrame;
				Vec2 targetDirection = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				targetDirection = ((Vec2)(ref targetDirection)).Normalized();
				targetState.SetTargetDirection(in targetDirection);
				if (HasNavigationPath)
				{
					if (_navigationPath.Size > 0)
					{
						UpdateNavigationPath(in currentState);
					}
					else
					{
						ReComputeNavigationPath(in currentState, in _targetState);
					}
				}
				return true;
			}
			if (_targetMode == TargetMode.State)
			{
				if (HasNavigationPath)
				{
					if (_navigationPath.Size > 0)
					{
						UpdateNavigationPath(in currentState);
					}
					else
					{
						ReComputeNavigationPath(in currentState, in _targetState);
					}
				}
				return true;
			}
			if (_targetMode == TargetMode.Ship || _targetMode == TargetMode.ShipOffset)
			{
				NavalState newTargetState = ((_targetMode != TargetMode.Ship) ? TargetShip.GetNavalState(in _targetOffset) : TargetShip.GetNavalState());
				if (HasNavigationPath)
				{
					ReComputeNavigationPath(in currentState, in newTargetState);
				}
				_targetState = newTargetState;
				return true;
			}
		}
		return false;
	}

	public float GetTargetStateZ()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		float result = 0f;
		if (_targetMode != TargetMode.None)
		{
			if (_targetMode == TargetMode.Ship)
			{
				result = TargetShip.GlobalFrame.origin.z;
			}
			else if (_targetMode == TargetMode.ShipOffset)
			{
				Vec3 origin = TargetShip.GlobalFrame.origin;
				float waterLevelAtPosition = ((ScriptComponentBehavior)_ownerShip).Scene.GetWaterLevelAtPosition(((Vec3)(ref origin)).AsVec2, true, false);
				float num = MathF.Max(0f, origin.z - waterLevelAtPosition);
				result = ((ScriptComponentBehavior)_ownerShip).Scene.GetWaterLevelAtPosition(_targetState.Position, true, false) + num;
			}
			else
			{
				result = ((ScriptComponentBehavior)_ownerShip).Scene.GetWaterLevelAtPosition(_targetState.Position, true, false);
			}
		}
		return result;
	}

	private ShipInputRecord StabilizeInput(ShipInputRecord inputRecord)
	{
		int num = 5;
		RowerLateralInput rowerLateral = _inputRecord.RowerLateral;
		RowerLongitudinalInput rowerLongitudinal = _inputRecord.RowerLongitudinal;
		RowerLongitudinalInput rowerLongitudinalDoubleTap = _inputRecord.RowerLongitudinalDoubleTap;
		float rudderLateral = _inputRecord.RudderLateral;
		SailInput sail = _inputRecord.Sail;
		if (inputRecord.RowerLateral != rowerLateral)
		{
			_rowerLateralDebounceCounter++;
			if (_rowerLateralDebounceCounter >= num)
			{
				rowerLateral = inputRecord.RowerLateral;
				_rowerLateralDebounceCounter = 0u;
			}
		}
		else
		{
			_rowerLateralDebounceCounter = 0u;
		}
		if (inputRecord.RowerLongitudinal != rowerLongitudinal)
		{
			_rowerLongitudinalDebounceCounter++;
			if (_rowerLongitudinalDebounceCounter >= num)
			{
				rowerLongitudinal = inputRecord.RowerLongitudinal;
				_rowerLongitudinalDebounceCounter = 0u;
			}
		}
		else
		{
			_rowerLongitudinalDebounceCounter = 0u;
		}
		if (inputRecord.RudderLateral != rudderLateral)
		{
			_rudderLateralDebounceCounter++;
			if (_rudderLateralDebounceCounter >= num)
			{
				rudderLateral = inputRecord.RudderLateral;
				_rudderLateralDebounceCounter = 0u;
			}
		}
		else
		{
			_rudderLateralDebounceCounter = 0u;
		}
		if (inputRecord.Sail != sail)
		{
			_sailDebounceCounter++;
			if (_sailDebounceCounter >= num)
			{
				sail = inputRecord.Sail;
				_sailDebounceCounter = 0u;
			}
		}
		else
		{
			_sailDebounceCounter = 0u;
		}
		return new ShipInputRecord(rowerLateral, rowerLongitudinal, rowerLongitudinalDoubleTap, rudderLateral, sail);
	}

	private void SetTargetShipAux(MissionShip targetShip, bool ignoreCollision = false)
	{
		if (_ignoreTargetShipCollision != ignoreCollision || TargetShip != targetShip)
		{
			if (_ignoreTargetShipCollision)
			{
				RemoveShipFromCollisionIgnoreList(TargetShip);
			}
			if (ignoreCollision)
			{
				AddShipToCollisionIgnoreList(targetShip);
			}
			_ignoreTargetShipCollision = ignoreCollision;
			TargetShip = targetShip;
		}
	}

	private static void DecideControl(in ShipInputRecord oldInputRecord, in Vec2 shipForward2D, in Vec2 globalWindVelocity, float desiredAngularAcceleration, in float desiredLinearAcceleration, float maxLinearAcceleration, float maxAngularAcceleration, out ShipInputRecord inputRecord, Vec3 shipLocalVelocity, bool enforceSailUsage)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		inputRecord = ShipInputRecord.None();
		float num = MathF.Abs(desiredAngularAcceleration);
		float num2 = MathF.Abs(desiredLinearAcceleration);
		bool flag = num > 0.3f || (num > 0f && num2 <= 0.001f);
		if (num > 0.01f && flag)
		{
			if (desiredAngularAcceleration > 0f)
			{
				inputRecord.SetRowerLateral(RowerLateralInput.Left);
			}
			else if (desiredAngularAcceleration < 0f)
			{
				inputRecord.SetRowerLateral(RowerLateralInput.Right);
			}
		}
		if (flag)
		{
			if (shipLocalVelocity.y > 1f)
			{
				inputRecord.SetRowerLongitudinal(RowerLongitudinalInput.Backward);
			}
			else if (shipLocalVelocity.y < -1f)
			{
				inputRecord.SetRowerLongitudinal(RowerLongitudinalInput.Forward);
			}
		}
		else if (num2 >= 0.01f)
		{
			if (desiredLinearAcceleration >= 0f)
			{
				inputRecord.SetRowerLongitudinal(RowerLongitudinalInput.Forward);
			}
			else
			{
				inputRecord.SetRowerLongitudinal(RowerLongitudinalInput.Backward);
			}
		}
		float rudderLateral = 0f;
		if (flag)
		{
			rudderLateral = inputRecord.RowerLateral.ToRudderInput();
		}
		else if (desiredAngularAcceleration > 0f)
		{
			rudderLateral = -1f;
		}
		else if (desiredAngularAcceleration < 0f)
		{
			rudderLateral = 1f;
		}
		inputRecord.SetRudderLateral(rudderLateral);
		Vec2 val = globalWindVelocity;
		float num3 = Vec2.DotProduct(((Vec2)(ref val)).Normalized(), shipForward2D) * desiredLinearAcceleration;
		float num4 = 0.2f * maxLinearAcceleration;
		float num5 = 0.6f * maxLinearAcceleration;
		if (enforceSailUsage)
		{
			inputRecord.SetSail(SailInput.Full);
		}
		else if (flag)
		{
			inputRecord.SetSail(SailInput.Raised);
		}
		else if (num3 > num5)
		{
			inputRecord.SetSail(SailInput.Full);
		}
		else if (num3 < num4)
		{
			inputRecord.SetSail(SailInput.Raised);
		}
		else
		{
			inputRecord.SetSail(oldInputRecord.Sail);
		}
	}

	private void ReComputeNavigationPath(in NavalState currentState, in NavalState newTargetState, bool forceRecompute = false)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val3;
		if (forceRecompute || ShouldRecomputePath(in currentState, in newTargetState))
		{
			Vec3 val = default(Vec3);
			((Vec3)(ref val))._002Ector(currentState.Position, 0f, -1f);
			Vec3 val2 = default(Vec3);
			((Vec3)(ref val2))._002Ector(newTargetState.Position, 0f, -1f);
			Mission.Current.Scene.SetAbilityOfFacesWithId(1, true);
			UIntPtr nearestNavigationMeshForPosition = Mission.Current.Scene.GetNearestNavigationMeshForPosition(ref val, 1000000f, true);
			UIntPtr nearestNavigationMeshForPosition2 = Mission.Current.Scene.GetNearestNavigationMeshForPosition(ref val2, 1000000f, true);
			if (nearestNavigationMeshForPosition != UIntPtr.Zero && nearestNavigationMeshForPosition2 != UIntPtr.Zero)
			{
				float num = MathF.Lerp(_ownerShip.Physics.PhysicsBoundingBoxSizeWithoutChildren.x, _ownerShip.Physics.PhysicsBoundingBoxSizeWithoutChildren.y, 0.75f, 1E-05f);
				bool num2 = nearestNavigationMeshForPosition == _lastNavPathStartFace && nearestNavigationMeshForPosition2 == _lastNavPathTargetFace;
				bool flag = false;
				bool flag2 = _navigationPath.Size > 0;
				if (!num2 || !flag2)
				{
					_navigationPath.Size = 0;
					flag2 = Mission.Current.Scene.GetPathBetweenAIFaces(nearestNavigationMeshForPosition, nearestNavigationMeshForPosition2, ((Vec3)(ref val)).AsVec2, ((Vec3)(ref val2)).AsVec2, num, _navigationPath, (int[])null);
					flag = true;
				}
				else if (flag2)
				{
					NavigationPath navigationPath = _navigationPath;
					int num3 = _navigationPath.Size - 1;
					val3 = newTargetState.Position;
					navigationPath.OverridePathPointAtIndex(num3, ref val3);
				}
				Mission.Current.Scene.SetAbilityOfFacesWithId(1, false);
				if (flag2)
				{
					if (flag)
					{
						_lastNavPathPointIndex = 0;
						_lastNavPathHardRecomputeTime = Mission.Current.CurrentTime;
					}
					_lastNavPathStartFace = nearestNavigationMeshForPosition;
					_lastNavPathTargetFace = nearestNavigationMeshForPosition2;
					_lastNavPathTargetPosition = newTargetState.Position;
					_navPathTargetDriftAccumulator = 0f;
					UpdateNavigationPath(in currentState);
				}
				else
				{
					ClearNavigationPathAux();
				}
			}
			else
			{
				ClearNavigationPathAux();
			}
		}
		else
		{
			if (_navigationPath.Size > 0)
			{
				NavigationPath navigationPath2 = _navigationPath;
				int num4 = _navigationPath.Size - 1;
				val3 = newTargetState.Position;
				navigationPath2.OverridePathPointAtIndex(num4, ref val3);
			}
			val3 = newTargetState.Position - _lastNavPathTargetPosition;
			float length = ((Vec2)(ref val3)).Length;
			if (length >= 0.0001f)
			{
				_navPathTargetDriftAccumulator += length;
			}
			_lastNavPathTargetPosition = newTargetState.Position;
			UpdateNavigationPath(in currentState);
		}
	}

	private bool ShouldRecomputePath(in NavalState currentState, in NavalState newTargetState)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (_navigationPath.Size == 0)
		{
			return true;
		}
		float num = Mission.Current.CurrentTime - _lastNavPathHardRecomputeTime;
		Vec2 currentPos = _lastNavPathTargetPosition - newTargetState.Position;
		if (((Vec2)(ref currentPos)).LengthSquared >= 16f)
		{
			return true;
		}
		if (num >= 0.5f)
		{
			if (_navPathTargetDriftAccumulator >= 4f)
			{
				return true;
			}
			currentPos = currentState.Position;
			if (NavPathStartOrGoalFaceChanged(in currentPos, newTargetState.Position))
			{
				return true;
			}
		}
		return false;
	}

	private bool NavPathStartOrGoalFaceChanged(in Vec2 currentPos, in Vec2 newTargetPos)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		Mission.Current.Scene.SetAbilityOfFacesWithId(1, true);
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(currentPos, 0f, -1f);
		Vec3 val2 = default(Vec3);
		((Vec3)(ref val2))._002Ector(newTargetPos, 0f, -1f);
		UIntPtr nearestNavigationMeshForPosition = Mission.Current.Scene.GetNearestNavigationMeshForPosition(ref val, 1000000f, true);
		UIntPtr nearestNavigationMeshForPosition2 = Mission.Current.Scene.GetNearestNavigationMeshForPosition(ref val2, 1000000f, true);
		if (nearestNavigationMeshForPosition == UIntPtr.Zero || nearestNavigationMeshForPosition2 == UIntPtr.Zero)
		{
			result = true;
		}
		else if (nearestNavigationMeshForPosition != _lastNavPathStartFace || nearestNavigationMeshForPosition2 != _lastNavPathTargetFace)
		{
			result = true;
		}
		Mission.Current.Scene.SetAbilityOfFacesWithId(1, false);
		return result;
	}

	private void UpdateNavigationPath(in NavalState currentState)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Vec2[] pathPoints = _navigationPath.PathPoints;
		int num = _navigationPath.Size - 1;
		Vec2 position = currentState.Position;
		while (_lastNavPathPointIndex < num)
		{
			int lastNavPathPointIndex = _lastNavPathPointIndex;
			int num2 = lastNavPathPointIndex + 1;
			Vec2 val = pathPoints[lastNavPathPointIndex];
			Vec2 val2 = pathPoints[num2];
			Vec2 val3 = position - val;
			if (((Vec2)(ref val3)).LengthSquared <= 900f)
			{
				_lastNavPathPointIndex++;
				continue;
			}
			Vec2 val4 = val2 - val;
			if (((Vec2)(ref val3)).DotProduct(val4) > 0f)
			{
				_lastNavPathPointIndex++;
				continue;
			}
			break;
		}
	}

	private NavalState GetNextTargetStateOverPath()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (_lastNavPathPointIndex < _navigationPath.Size - 1)
		{
			Vec2 position = _navigationPath[_lastNavPathPointIndex];
			Vec2 val = _navigationPath[_lastNavPathPointIndex + 1] - _navigationPath[_lastNavPathPointIndex];
			return new NavalState(in position, ((Vec2)(ref val)).RotationInRadians);
		}
		return _targetState;
	}

	private void ClearNavigationPathAux()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_navigationPath.Size = 0;
		_lastNavPathPointIndex = -1;
		_lastNavPathStartFace = UIntPtr.Zero;
		_lastNavPathTargetFace = UIntPtr.Zero;
		_lastNavPathTargetPosition = Vec2.Zero;
		_navPathTargetDriftAccumulator = 0f;
	}
}
