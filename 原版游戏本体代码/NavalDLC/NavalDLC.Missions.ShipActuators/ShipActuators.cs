using System;
using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.ShipActuators;

public class ShipActuators
{
	private struct RowingSoundEventData
	{
		internal float SoundEventRowingPowerParam;

		internal int NumberOfActiveOars;

		internal bool ShouldTriggerOarSound;

		internal bool IsOarsInWater;

		internal Vec3 RowingSoundEventPositions;

		internal int FurthestOarIndex;

		internal int ClosestOarIndex;

		internal SoundEvent OarsSoundEvents;
	}

	public struct OarPhaseData
	{
		public float CurPhase;

		public float LastNonZeroRevolutionRate;

		public bool LockedToTargetPhase;

		public float CycleArcSizeMult;
	}

	public struct OarAnimKeyFrame
	{
		public float KeyProgress;

		public float Speed;

		public OarAnimKeyFrame(float keyProgress, float speed)
		{
			KeyProgress = keyProgress;
			Speed = speed;
		}
	}

	private static class OarRowSpeedAnimationManager
	{
		public static OarAnimKeyFrame[] ForwardPhaseSpeedAnim = new OarAnimKeyFrame[9]
		{
			new OarAnimKeyFrame(0f, 1.5f),
			new OarAnimKeyFrame(0.15f, 1.6f),
			new OarAnimKeyFrame(0.25f, 1.2f),
			new OarAnimKeyFrame(0.3f, 1f),
			new OarAnimKeyFrame(0.65f, 1f),
			new OarAnimKeyFrame(0.7f, 1.4f),
			new OarAnimKeyFrame(0.75f, 1.5f),
			new OarAnimKeyFrame(0.9f, 1.5f),
			new OarAnimKeyFrame(1f, 1.5f)
		};

		public static OarAnimKeyFrame[] PartialPhaseSpeedAnim = new OarAnimKeyFrame[9]
		{
			new OarAnimKeyFrame(0f, 1.5f),
			new OarAnimKeyFrame(0.15f, 1.6f),
			new OarAnimKeyFrame(0.25f, 1.2f),
			new OarAnimKeyFrame(0.3f, 1f),
			new OarAnimKeyFrame(0.65f, 1f),
			new OarAnimKeyFrame(0.7f, 1.4f),
			new OarAnimKeyFrame(0.75f, 1.5f),
			new OarAnimKeyFrame(0.9f, 1.5f),
			new OarAnimKeyFrame(1f, 1.5f)
		};

		public static OarAnimKeyFrame[] OnPointTurnPhaseSpeedAnim = new OarAnimKeyFrame[9]
		{
			new OarAnimKeyFrame(0f, 1.5f),
			new OarAnimKeyFrame(0.15f, 1.6f),
			new OarAnimKeyFrame(0.25f, 1.2f),
			new OarAnimKeyFrame(0.3f, 1f),
			new OarAnimKeyFrame(0.65f, 1f),
			new OarAnimKeyFrame(0.7f, 1.4f),
			new OarAnimKeyFrame(0.75f, 1.5f),
			new OarAnimKeyFrame(0.9f, 1.5f),
			new OarAnimKeyFrame(1f, 1.5f)
		};
	}

	private static readonly int[] _rowingSoundEventIds = new int[2]
	{
		SoundManager.GetEventGlobalIndex("event:/mission/movement/vessel/rowing/rowing_left_side"),
		SoundManager.GetEventGlobalIndex("event:/mission/movement/vessel/rowing/rowing_right_side")
	};

	public const string SailTagPrefix = "sail_center_";

	public const string RudderStockPositionTag = "rudder_stock";

	private const float MinSpeedToUseBothOarsToTurn = 6f;

	private static readonly int _rudderSoundEventId = SoundEvent.GetEventIdFromString("event:/mission/movement/vessel/ship_steering");

	private static readonly int _shipPresenceSoundEventId = SoundEvent.GetEventIdFromString("event:/mission/movement/vessel/basic_ship_presence");

	private float _rudderLocalRotation;

	private float _lastRudderLocalRotation;

	private float _lastAddedFromInputRudderLocalRotation;

	private float _lastTargetRudderStabilityLocalRotation;

	private Vec3 _rudderStockLocalPosition;

	private readonly MissionShip _ownerMissionShip;

	private readonly Scene _cachedOwnerScene;

	private float _rowersPhase;

	private float _lastFramePhaseRate;

	private bool _evenCycle;

	private OarPhaseData _leftPhaseData;

	private OarPhaseData _rightPhaseData;

	private readonly MBList<MissionSail> _sails = new MBList<MissionSail>();

	private readonly MBList<(GameEntity entity, MissionOar oar)> _leftSideOars = new MBList<(GameEntity, MissionOar)>();

	private readonly MBList<(GameEntity entity, MissionOar oar)> _rightSideOars = new MBList<(GameEntity, MissionOar)>();

	private MBList<ShipForce> _leftOarForces = new MBList<ShipForce>();

	private MBList<ShipForce> _rightOarForces = new MBList<ShipForce>();

	private MBList<ShipForce> _sailForces = new MBList<ShipForce>();

	private ShipForce _rudderShipForce;

	private OarSidePhaseController _leftOarsPhaseController;

	private OarSidePhaseController _rightOarsPhaseController;

	private float _oarsmenForceMultiplier;

	private float _oarsmenSpeedMultiplier;

	private float _oarsTipSpeedReferenceMultiplier;

	private float _oarFrictionMultiplier;

	private float _oarAppliedForceMultiplierForStoryMission;

	private float _maxOarLength;

	private readonly MBList<(MissionShip ship, OarSidePhaseController.OarSide shipSide)> _nearbyShips;

	private float _timeLeftToUpdateNearbyShips;

	private readonly NavalShipsLogic _navalShipsLogic;

	private Vec3 _leftSideAverageOarLocalPos;

	private Vec3 _rightSideAverageOarLocalPos;

	private SoundEvent _rudderSoundEvent;

	private SoundEvent _shipPresenceSoundEvent;

	private RowingSoundEventData[] _rowingSoundEventData = new RowingSoundEventData[2];

	private float _rudderStressSoundParam;

	private float _shipPresenceSoundParam;

	public int VisualRudderPullDirection { get; private set; }

	public float VisualRudderLocalRotation { get; private set; }

	public MBReadOnlyList<MissionSail> Sails => (MBReadOnlyList<MissionSail>)(object)_sails;

	public ShipActuators(MissionShip ownerShip)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		_ownerMissionShip = ownerShip;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)ownerShip).GameEntity;
		_cachedOwnerScene = ((WeakGameEntity)(ref gameEntity)).Scene;
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		OnShipObjectUpdated();
		_rowersPhase = MathF.PI;
		_evenCycle = true;
		_nearbyShips = new MBList<(MissionShip, OarSidePhaseController.OarSide)>();
		_timeLeftToUpdateNearbyShips = 0f;
	}

	public void OnShipObjectUpdated()
	{
		LoadRudder();
		LoadOars();
		LoadSails();
	}

	public unsafe ShipForceRecord OnParallelFixedTick(float fixedDt, in ShipActuatorRecord actuatorInput)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerMissionShip).GameEntity;
		MatrixFrame shipEntityGlobalFrame = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		TWSharedMutexReadLock val = default(TWSharedMutexReadLock);
		((TWSharedMutexReadLock)(ref val))._002Ector(Scene.PhysicsAndRayCastLock);
		Vec3 shipLinearVelocityGlobal;
		Vec3 shipAngularVelocityGlobal;
		try
		{
			shipLinearVelocityGlobal = GameEntityPhysicsExtensions.GetLinearVelocityMT(((ScriptComponentBehavior)_ownerMissionShip).GameEntity);
			shipAngularVelocityGlobal = GameEntityPhysicsExtensions.GetAngularVelocityMT(((ScriptComponentBehavior)_ownerMissionShip).GameEntity);
		}
		finally
		{
			((IDisposable)(*(TWSharedMutexReadLock*)(&val))/*cast due to .constrained prefix*/).Dispose();
		}
		float shipForwardSpeed = Vec3.DotProduct(shipLinearVelocityGlobal, shipEntityGlobalFrame.rotation.f);
		FixedUpdateRowers(fixedDt, in actuatorInput, in shipEntityGlobalFrame, shipForwardSpeed);
		if (((List<MissionSail>)(object)_sails).Count > 0)
		{
			FixedUpdateSails(fixedDt, in actuatorInput, in shipLinearVelocityGlobal, in shipAngularVelocityGlobal);
		}
		FixedUpdateRudder(fixedDt, in actuatorInput, in shipEntityGlobalFrame, shipForwardSpeed);
		MBList<ShipForce> leftOarForces = _leftOarForces;
		MBList<ShipForce> rightOarForces = _rightOarForces;
		MBReadOnlyList<ShipForce> sailForces = (MBReadOnlyList<ShipForce>)(object)_sailForces;
		return new ShipForceRecord((MBReadOnlyList<ShipForce>)(object)leftOarForces, (MBReadOnlyList<ShipForce>)(object)rightOarForces, in sailForces, in _rudderShipForce);
	}

	public void OnTickParallel(float dt)
	{
		OnParallelTickRowers(dt);
		OnParallelTickRudder(dt);
	}

	private void CalculateOarSoundPositionsAndParams()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if (!_ownerMissionShip.ShouldUpdateSoundPos)
		{
			return;
		}
		if (_ownerMissionShip.Physics.LastSubmergedHeightFactorForActuators > 0.01f)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerMissionShip).GameEntity;
			MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			for (int i = 0; i < 2; i++)
			{
				if (_rowingSoundEventData[i].NumberOfActiveOars <= 0)
				{
					continue;
				}
				MBList<ShipForce> val = ((i == 0) ? _leftOarForces : _rightOarForces);
				float num = ((i == 0) ? _leftOarsPhaseController.VisualPhase : _rightOarsPhaseController.VisualPhase);
				ShipForce shipForce = ((List<ShipForce>)(object)val)[_rowingSoundEventData[i].ClosestOarIndex];
				Vec3 closestOarGlobalPos = ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref shipForce.LocalPosition);
				shipForce = ((List<ShipForce>)(object)val)[_rowingSoundEventData[i].FurthestOarIndex];
				Vec3 furthestOarGlobalPos = ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref shipForce.LocalPosition);
				_rowingSoundEventData[i].RowingSoundEventPositions = CalculateRowingSoundPosition(in closestOarGlobalPos, in furthestOarGlobalPos);
				if (MBMath.IsBetweenInclusive(num, -1.3962634f, 1.3962634f))
				{
					if (!_rowingSoundEventData[i].IsOarsInWater)
					{
						_rowingSoundEventData[i].SoundEventRowingPowerParam = CalculateOarRowingPowerSoundParameter((OarSidePhaseController.OarSide)i, in _rowingSoundEventData[i].RowingSoundEventPositions);
						if (_rowingSoundEventData[i].SoundEventRowingPowerParam > 0f)
						{
							_rowingSoundEventData[i].ShouldTriggerOarSound = true;
							_rowingSoundEventData[i].IsOarsInWater = true;
						}
					}
				}
				else
				{
					_rowingSoundEventData[i].IsOarsInWater = false;
				}
			}
		}
		else
		{
			_rowingSoundEventData[0].IsOarsInWater = false;
			_rowingSoundEventData[0].ShouldTriggerOarSound = false;
			_rowingSoundEventData[1].IsOarsInWater = false;
			_rowingSoundEventData[1].ShouldTriggerOarSound = false;
		}
	}

	internal void Update(float dt)
	{
		for (int i = 0; i < ((List<MissionSail>)(object)_sails).Count; i++)
		{
			((List<MissionSail>)(object)_sails)[i].Update(dt);
		}
		UpdateSoundEventPositions();
	}

	private void FixedUpdateSails(float fixedDt, in ShipActuatorRecord actuatorInput, in Vec3 shipLinearVelocityGlobal, in Vec3 shipAngularVelocityGlobal)
	{
		for (int i = 0; i < ((List<MissionSail>)(object)_sails).Count; i++)
		{
			MissionSail missionSail = ((List<MissionSail>)(object)_sails)[i];
			missionSail.FixedUpdate(fixedDt, in actuatorInput, in shipLinearVelocityGlobal, in shipAngularVelocityGlobal);
			((List<ShipForce>)(object)_sailForces)[i] = missionSail.Force;
		}
	}

	private void UpdateSoundEventPositions()
	{
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		if (_ownerMissionShip.ShouldUpdateSoundPos)
		{
			if (_rudderSoundEvent == null)
			{
				_rudderSoundEvent = SoundEvent.CreateEvent(_rudderSoundEventId, _cachedOwnerScene);
				_shipPresenceSoundEvent = SoundEvent.CreateEvent(_shipPresenceSoundEventId, _cachedOwnerScene);
				_rudderSoundEvent.Play();
				_shipPresenceSoundEvent.Play();
			}
			for (int i = 0; i < 2; i++)
			{
				if (_rowingSoundEventData[i].ShouldTriggerOarSound)
				{
					SoundEvent oarsSoundEvents = _rowingSoundEventData[i].OarsSoundEvents;
					if (oarsSoundEvents != null)
					{
						oarsSoundEvents.Stop();
					}
					_rowingSoundEventData[i].OarsSoundEvents = SoundEvent.CreateEvent(_rowingSoundEventIds[i], _cachedOwnerScene);
					_rowingSoundEventData[i].OarsSoundEvents.SetParameter("RowingPower", _rowingSoundEventData[i].SoundEventRowingPowerParam);
					_rowingSoundEventData[i].OarsSoundEvents.SetParameter("OarsmanLevel", (float)_rowingSoundEventData[i].NumberOfActiveOars);
					_rowingSoundEventData[i].OarsSoundEvents.SetPosition(_rowingSoundEventData[i].RowingSoundEventPositions);
					_rowingSoundEventData[i].OarsSoundEvents.Play();
					_rowingSoundEventData[i].ShouldTriggerOarSound = false;
				}
				else
				{
					SoundEvent oarsSoundEvents2 = _rowingSoundEventData[i].OarsSoundEvents;
					if (oarsSoundEvents2 != null)
					{
						oarsSoundEvents2.SetPosition(_rowingSoundEventData[i].RowingSoundEventPositions);
					}
				}
			}
			MatrixFrame globalFrame = _ownerMissionShip.GlobalFrame;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerMissionShip).GameEntity;
			Vec3 centerOfMass = ((WeakGameEntity)(ref gameEntity)).CenterOfMass;
			Vec3 position = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref centerOfMass);
			position.z += 3f;
			_shipPresenceSoundEvent.SetPosition(position);
			_shipPresenceSoundEvent.SetParameter("ForceContinuous", _shipPresenceSoundParam);
			_rudderSoundEvent.SetPosition(((MatrixFrame)(ref globalFrame)).TransformToParent(ref _rudderShipForce.LocalPosition));
			_rudderSoundEvent.SetParameter("RudderStress", _rudderStressSoundParam);
		}
		else
		{
			SoundEvent oarsSoundEvents3 = _rowingSoundEventData[0].OarsSoundEvents;
			if (oarsSoundEvents3 != null)
			{
				oarsSoundEvents3.Stop();
			}
			SoundEvent oarsSoundEvents4 = _rowingSoundEventData[1].OarsSoundEvents;
			if (oarsSoundEvents4 != null)
			{
				oarsSoundEvents4.Stop();
			}
			SoundEvent rudderSoundEvent = _rudderSoundEvent;
			if (rudderSoundEvent != null)
			{
				rudderSoundEvent.Stop();
			}
			SoundEvent shipPresenceSoundEvent = _shipPresenceSoundEvent;
			if (shipPresenceSoundEvent != null)
			{
				shipPresenceSoundEvent.Stop();
			}
			_rowingSoundEventData[0].OarsSoundEvents = null;
			_rowingSoundEventData[1].OarsSoundEvents = null;
			_rudderSoundEvent = null;
			_shipPresenceSoundEvent = null;
		}
	}

	private Vec3 CalculateRowingSoundPosition(in Vec3 closestOarGlobalPos, in Vec3 furthestOarGlobalPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Vec3 origin = SoundManager.GetListenerFrame().origin;
		Vec3 val = furthestOarGlobalPos - closestOarGlobalPos;
		float num = Vec3.DotProduct(origin - closestOarGlobalPos, val) / ((Vec3)(ref val)).LengthSquared;
		return closestOarGlobalPos + MathF.Clamp(num, 0f, 1f) * val;
	}

	private float CalculateOarRowingPowerSoundParameter(OarSidePhaseController.OarSide oarSide, in Vec3 soundPos)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		MBList<ShipForce> val = null;
		MBList<(GameEntity, MissionOar)> val2 = null;
		switch (oarSide)
		{
		case OarSidePhaseController.OarSide.Left:
			val = _leftOarForces;
			val2 = _leftSideOars;
			break;
		case OarSidePhaseController.OarSide.Right:
			val = _rightOarForces;
			val2 = _rightSideOars;
			break;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerMissionShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		float num = 0f;
		float num2 = 0f;
		float num3 = -1f;
		for (int i = 0; i < ((List<ShipForce>)(object)val).Count; i++)
		{
			float num4 = (((List<(GameEntity, MissionOar)>)(object)val2)[i].Item2.IsExtracted ? 5000f : 0f);
			Vec3 val3 = soundPos;
			ShipForce shipForce = ((List<ShipForce>)(object)val)[i];
			float num5 = ((Vec3)(ref val3)).Distance(((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref shipForce.LocalPosition));
			if (num5 < 0.010000001f && num4 > 0f)
			{
				num3 = num4;
				break;
			}
			if (num5 > 0.010000001f)
			{
				float num6 = 1f / num5;
				num += num6 * num4;
				num2 += num6;
			}
		}
		if (num3 == -1f && num2 != 0f)
		{
			num3 = num / num2;
		}
		return MathF.Min(num3 * 0.1f, 500f);
	}

	private void LoadOars()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0513: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05db: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		MBList<ShipOarDeck> val = MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<ShipOarDeck>(((ScriptComponentBehavior)_ownerMissionShip).GameEntity);
		_leftOarsPhaseController = new OarSidePhaseController(_ownerMissionShip, OarSidePhaseController.OarSide.Left);
		((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Clear();
		((List<ShipForce>)(object)_leftOarForces).Clear();
		_rightOarsPhaseController = new OarSidePhaseController(_ownerMissionShip, OarSidePhaseController.OarSide.Right);
		((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Clear();
		((List<ShipForce>)(object)_rightOarForces).Clear();
		_maxOarLength = 0f;
		for (int i = 0; i < ((List<ShipOarDeck>)(object)val).Count; i++)
		{
			ShipOarDeck shipOarDeck = ((List<ShipOarDeck>)(object)val)[i];
			OarDeckParameters parameters = shipOarDeck.GetParameters();
			_maxOarLength = MathF.Max(_maxOarLength, parameters.OarLength);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)shipOarDeck).GameEntity;
			List<WeakGameEntity> list = ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("oar_gate_left");
			gameEntity = ((ScriptComponentBehavior)shipOarDeck).GameEntity;
			List<WeakGameEntity> list2 = ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("oar_gate_right");
			foreach (WeakGameEntity item in list)
			{
				GameEntity val2 = GameEntity.CreateFromWeakEntity(item);
				MissionOar missionOar = MissionOar.CreateShipOar(_ownerMissionShip, val2, parameters, _leftOarsPhaseController);
				GetOarScriptFromEntity(item).InitializeOar(missionOar);
				((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Add((val2, missionOar));
				((List<ShipForce>)(object)_leftOarForces).Add(ShipForce.None(ShipForce.SourceType.Oar));
			}
			foreach (WeakGameEntity item2 in list2)
			{
				GameEntity val3 = GameEntity.CreateFromWeakEntity(item2);
				MissionOar missionOar2 = MissionOar.CreateShipOar(_ownerMissionShip, val3, parameters, _rightOarsPhaseController);
				GetOarScriptFromEntity(item2).InitializeOar(missionOar2);
				((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Add((val3, missionOar2));
				((List<ShipForce>)(object)_rightOarForces).Add(ShipForce.None(ShipForce.SourceType.Oar));
			}
		}
		GenerateAverageSideDeckParameters(out var leftSideAverageDeckParameters, out var rightSideAverageDeckParameters, _leftSideOars, _rightSideOars);
		_leftOarsPhaseController.SetAverageOarDeckParameters(leftSideAverageDeckParameters);
		_rightOarsPhaseController.SetAverageOarDeckParameters(rightSideAverageDeckParameters);
		_rowingSoundEventData[0].ClosestOarIndex = 0;
		_rowingSoundEventData[1].ClosestOarIndex = 0;
		_rowingSoundEventData[0].FurthestOarIndex = 0;
		_rowingSoundEventData[1].FurthestOarIndex = 0;
		_leftSideAverageOarLocalPos = Vec3.Zero;
		_rightSideAverageOarLocalPos = Vec3.Zero;
		Vec3 bladeContact2;
		for (int j = 0; j < ((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Count; j++)
		{
			Vec3 bladeContact = ((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[j].Item2.BladeContact;
			float num = ((Vec3)(ref bladeContact)).DistanceSquared(_rudderStockLocalPosition);
			bladeContact2 = ((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[_rowingSoundEventData[0].FurthestOarIndex].Item2.BladeContact;
			if (num > ((Vec3)(ref bladeContact2)).DistanceSquared(_rudderStockLocalPosition))
			{
				_rowingSoundEventData[0].FurthestOarIndex = j;
			}
			_leftSideAverageOarLocalPos += bladeContact;
		}
		_leftSideAverageOarLocalPos /= (float)((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Count;
		for (int k = 0; k < ((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Count; k++)
		{
			Vec3 bladeContact3 = ((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[k].Item2.BladeContact;
			Vec3 bladeContact4 = ((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[_rowingSoundEventData[0].FurthestOarIndex].Item2.BladeContact;
			float num2 = ((Vec3)(ref bladeContact3)).DistanceSquared(bladeContact4);
			bladeContact2 = ((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[_rowingSoundEventData[0].ClosestOarIndex].Item2.BladeContact;
			if (num2 > ((Vec3)(ref bladeContact2)).DistanceSquared(bladeContact4))
			{
				_rowingSoundEventData[0].ClosestOarIndex = k;
			}
		}
		for (int l = 0; l < ((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Count; l++)
		{
			Vec3 bladeContact5 = ((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[l].Item2.BladeContact;
			float num3 = ((Vec3)(ref bladeContact5)).DistanceSquared(_rudderStockLocalPosition);
			bladeContact2 = ((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[_rowingSoundEventData[1].FurthestOarIndex].Item2.BladeContact;
			if (num3 > ((Vec3)(ref bladeContact2)).DistanceSquared(_rudderStockLocalPosition))
			{
				_rowingSoundEventData[1].FurthestOarIndex = l;
			}
			_rightSideAverageOarLocalPos += bladeContact5;
		}
		_rightSideAverageOarLocalPos /= (float)((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Count;
		for (int m = 0; m < ((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Count; m++)
		{
			Vec3 bladeContact6 = ((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[m].Item2.BladeContact;
			Vec3 bladeContact7 = ((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[_rowingSoundEventData[1].FurthestOarIndex].Item2.BladeContact;
			float num4 = ((Vec3)(ref bladeContact6)).DistanceSquared(bladeContact7);
			bladeContact2 = ((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[_rowingSoundEventData[1].ClosestOarIndex].Item2.BladeContact;
			if (num4 > ((Vec3)(ref bladeContact2)).DistanceSquared(bladeContact7))
			{
				_rowingSoundEventData[1].ClosestOarIndex = m;
			}
		}
		float num5 = 1f;
		float oarsmenSpeedMultiplier = 1f;
		if (_ownerMissionShip.ShipOrigin != null)
		{
			num5 = 1f + _ownerMissionShip.ShipOrigin.MaxOarForceFactor;
			oarsmenSpeedMultiplier = 1f + _ownerMissionShip.ShipOrigin.MaxOarPowerFactor;
		}
		_oarsmenForceMultiplier = _ownerMissionShip.MissionShipObject.OarsmenForceMultiplier * num5;
		_oarsmenSpeedMultiplier = oarsmenSpeedMultiplier;
		_oarFrictionMultiplier = _ownerMissionShip.MissionShipObject.OarFrictionMultiplier;
		Vec3 val4 = MissionOar.ComputeBladeContactVelocityAux(leftSideAverageDeckParameters, 0f, MathF.PI * 2f);
		_oarsTipSpeedReferenceMultiplier = MathF.Abs(_ownerMissionShip.MissionShipObject.OarsTipSpeed / val4.y);
		_oarAppliedForceMultiplierForStoryMission = 1f;
	}

	public void OnShipRemoved(MissionShip ship)
	{
		((List<(MissionShip, OarSidePhaseController.OarSide)>)(object)_nearbyShips).Clear();
		_timeLeftToUpdateNearbyShips = 0f;
	}

	private static OarDeckParameters GenerateAverageSideDeckParametersAux(MBList<(GameEntity entity, MissionOar oar)> sideOars)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		foreach (var item in (List<(GameEntity, MissionOar)>)(object)sideOars)
		{
			OarDeckParameters deckParameters = item.Item2.DeckParameters;
			num += deckParameters.VerticalBaseAngle;
			num2 += deckParameters.LateralBaseAngle;
			num3 += deckParameters.VerticalRotationAngle;
			num4 += deckParameters.LateralRotationAngle;
			num5 += deckParameters.OarLength;
			num6 += deckParameters.RetractionRate;
			num7 += deckParameters.RetractionOffset;
		}
		float num8 = 1f / (float)((List<(GameEntity, MissionOar)>)(object)sideOars).Count;
		num *= num8;
		num2 *= num8;
		num3 *= num8;
		num4 *= num8;
		num5 *= num8;
		num6 *= num8;
		num7 *= num8;
		return new OarDeckParameters(num, num2, num3, num4, num5, num6, num7);
	}

	private static void GenerateAverageSideDeckParameters(out OarDeckParameters leftSideAverageDeckParameters, out OarDeckParameters rightSideAverageDeckParameters, MBList<(GameEntity entity, MissionOar oar)> leftSideOars, MBList<(GameEntity entity, MissionOar oar)> rightSideOars)
	{
		leftSideAverageDeckParameters = GenerateAverageSideDeckParametersAux(leftSideOars);
		rightSideAverageDeckParameters = GenerateAverageSideDeckParametersAux(rightSideOars);
	}

	private void LoadSails()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		((List<MissionSail>)(object)_sails).Clear();
		((List<ShipForce>)(object)_sailForces).Clear();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerMissionShip).GameEntity;
		for (int i = 0; i < ((List<ShipSail>)(object)_ownerMissionShip.MissionShipObject.Sails).Count; i++)
		{
			ShipSail sailObject = ((List<ShipSail>)(object)_ownerMissionShip.MissionShipObject.Sails)[i];
			string text = "sail_center_" + i;
			List<WeakGameEntity> list = ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag(text);
			if (list.Count > 0)
			{
				WeakGameEntity val = list[0];
				SailVisual firstScriptOfType = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<SailVisual>();
				firstScriptOfType.SoundsEnabled = true;
				val = list[0];
				((WeakGameEntity)(ref val)).CreateAndAddScriptComponent("MissionSail", true);
				val = list[0];
				MissionSail firstScriptOfType2 = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<MissionSail>();
				firstScriptOfType2.InitWithVariables(sailObject, _ownerMissionShip, firstScriptOfType);
				((List<MissionSail>)(object)_sails).Add(firstScriptOfType2);
				((List<ShipForce>)(object)_sailForces).Add(ShipForce.None());
			}
			else
			{
				Debug.FailedAssert("Unable to find a sail entity on ship prefab (" + ((WeakGameEntity)(ref gameEntity)).GetPrefabName() + ") with tag: " + text, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\ShipActuators\\ShipActuators.cs", "LoadSails", 646);
			}
		}
	}

	private void LoadRudder()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerMissionShip).GameEntity;
		if (_ownerMissionShip.MissionShipObject.HasValidRudderStockPosition)
		{
			_rudderStockLocalPosition = _ownerMissionShip.MissionShipObject.RudderStockPosition;
			return;
		}
		List<WeakGameEntity> list = ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("rudder_stock");
		if (list.Count > 0)
		{
			WeakGameEntity val = list[0];
			_rudderStockLocalPosition = ((WeakGameEntity)(ref val)).GetFrame().origin;
		}
		else
		{
			Debug.FailedAssert("Stock position is not defined for ship: " + ((WeakGameEntity)(ref gameEntity)).Name, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\ShipActuators\\ShipActuators.cs", "LoadRudder", 668);
			_rudderStockLocalPosition = Vec3.Zero;
		}
	}

	private void OnParallelTickRowers(float dt)
	{
		_leftOarsPhaseController.OnParallelTick(dt);
		_rightOarsPhaseController.OnParallelTick(dt);
		for (int i = 0; i < ((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Count; i++)
		{
			((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[i].Item2.OnParallelTick(dt);
		}
		for (int j = 0; j < ((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Count; j++)
		{
			((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[j].Item2.OnParallelTick(dt);
		}
	}

	public static void BlendPhaseTo(ref OarPhaseData phaseData, float targetPhase, float alphaInRadOverSeconds, float maxAlphaInRadOverSeconds, float fixedDt, bool toFullStop, bool isPartialStop)
	{
		targetPhase = MBMath.WrapAngleSafe(targetPhase);
		float num = MathF.Abs(MBMath.GetSmallestDifferenceBetweenTwoAngles(phaseData.CurPhase, targetPhase));
		if (phaseData.LockedToTargetPhase && num > alphaInRadOverSeconds * fixedDt * 2f)
		{
			phaseData.LockedToTargetPhase = false;
		}
		bool flag = false;
		if (!phaseData.LockedToTargetPhase)
		{
			if (toFullStop)
			{
				alphaInRadOverSeconds = maxAlphaInRadOverSeconds * 1.4f;
			}
			else if (isPartialStop)
			{
				alphaInRadOverSeconds = maxAlphaInRadOverSeconds * 1.4f;
			}
			else
			{
				alphaInRadOverSeconds = maxAlphaInRadOverSeconds * 1.3f;
				flag = true;
			}
		}
		if (!phaseData.LockedToTargetPhase)
		{
			float smallestDifferenceBetweenTwoAngles = MBMath.GetSmallestDifferenceBetweenTwoAngles(MBMath.WrapAngleSafe(phaseData.CurPhase + alphaInRadOverSeconds * fixedDt), targetPhase);
			float smallestDifferenceBetweenTwoAngles2 = MBMath.GetSmallestDifferenceBetweenTwoAngles(MBMath.WrapAngleSafe(phaseData.CurPhase - alphaInRadOverSeconds * fixedDt), targetPhase);
			float smallestDifferenceBetweenTwoAngles3 = MBMath.GetSmallestDifferenceBetweenTwoAngles(phaseData.CurPhase, targetPhase);
			float num2 = (flag ? 0.005f : 0.3f);
			float num3 = MathF.Abs(smallestDifferenceBetweenTwoAngles3) / alphaInRadOverSeconds;
			float num4 = ((toFullStop || isPartialStop) ? 0.03f : 0.1f);
			float num5 = (((MathF.Abs(smallestDifferenceBetweenTwoAngles3) > MathF.PI / 2f) ? (MathF.Sign(phaseData.LastNonZeroRevolutionRate) >= 0) : (MathF.Abs(smallestDifferenceBetweenTwoAngles) < MathF.Abs(smallestDifferenceBetweenTwoAngles2))) ? ((!(num3 > num2)) ? (alphaInRadOverSeconds * MathF.Max(num3 / num2, num4)) : alphaInRadOverSeconds) : ((!(num3 > num2)) ? ((0f - alphaInRadOverSeconds) * MathF.Max(num3 / num2, num4)) : (0f - alphaInRadOverSeconds)));
			if (MathF.Abs(smallestDifferenceBetweenTwoAngles3 / num5) <= 0f)
			{
				phaseData.LockedToTargetPhase = true;
			}
			float smallestDifferenceBetweenTwoAngles4 = MBMath.GetSmallestDifferenceBetweenTwoAngles(MBMath.WrapAngleSafe(phaseData.CurPhase + num5 * fixedDt), targetPhase);
			float smallestDifferenceBetweenTwoAngles5 = MBMath.GetSmallestDifferenceBetweenTwoAngles(MBMath.WrapAngleSafe(phaseData.CurPhase - num5 * fixedDt), targetPhase);
			if (smallestDifferenceBetweenTwoAngles4 * smallestDifferenceBetweenTwoAngles5 <= 0f && MathF.Abs(smallestDifferenceBetweenTwoAngles3) <= MathF.Abs(num5 * fixedDt))
			{
				phaseData.LockedToTargetPhase = true;
			}
			phaseData.CurPhase += num5 * fixedDt;
			phaseData.CurPhase = MBMath.WrapAngleSafe(phaseData.CurPhase);
		}
		if (phaseData.LockedToTargetPhase)
		{
			phaseData.CurPhase = targetPhase;
		}
		phaseData.CurPhase = MBMath.WrapAngleSafe(phaseData.CurPhase);
		float num6 = 1f;
		float num7 = 0.9599311f;
		if (!phaseData.LockedToTargetPhase && toFullStop && phaseData.CurPhase < num7 && phaseData.CurPhase > 0f - num7)
		{
			num6 = 0f;
		}
		phaseData.CycleArcSizeMult = MathF.Lerp(phaseData.CycleArcSizeMult, num6, fixedDt * 1.2f, 1E-05f);
	}

	private static float GetRowSpeedAccordingToPhase(float phase, bool forwards, bool partialTurn, bool onPointTurn)
	{
		OarAnimKeyFrame[] array;
		if (onPointTurn)
		{
			array = OarRowSpeedAnimationManager.OnPointTurnPhaseSpeedAnim;
			forwards = true;
		}
		else
		{
			array = (partialTurn ? OarRowSpeedAnimationManager.PartialPhaseSpeedAnim : OarRowSpeedAnimationManager.ForwardPhaseSpeedAnim);
		}
		float num = ((forwards ? phase : MBMath.WrapAngleSafe(MathF.PI * 2f - phase)) + MathF.PI) / (MathF.PI * 2f);
		if (num >= 1f)
		{
			num -= 1f;
		}
		float result = 1f;
		if (forwards)
		{
			for (int i = 0; i < array.Length - 1; i++)
			{
				OarAnimKeyFrame oarAnimKeyFrame = array[i];
				OarAnimKeyFrame oarAnimKeyFrame2 = array[i + 1];
				if (oarAnimKeyFrame.KeyProgress <= num && num < oarAnimKeyFrame2.KeyProgress)
				{
					float num2 = oarAnimKeyFrame2.KeyProgress - oarAnimKeyFrame.KeyProgress;
					float num3 = (num - oarAnimKeyFrame.KeyProgress) / num2;
					result = MathF.Lerp(oarAnimKeyFrame.Speed, oarAnimKeyFrame2.Speed, num3, 1E-05f);
					break;
				}
			}
		}
		else
		{
			for (int num4 = array.Length - 1; num4 >= 1; num4--)
			{
				OarAnimKeyFrame oarAnimKeyFrame3 = array[num4];
				OarAnimKeyFrame oarAnimKeyFrame4 = array[num4 - 1];
				if (oarAnimKeyFrame4.KeyProgress <= num && num < oarAnimKeyFrame3.KeyProgress)
				{
					float num5 = oarAnimKeyFrame3.KeyProgress - oarAnimKeyFrame4.KeyProgress;
					float num6 = (num - oarAnimKeyFrame4.KeyProgress) / num5;
					result = MathF.Lerp(oarAnimKeyFrame4.Speed, oarAnimKeyFrame3.Speed, num6, 1E-05f);
					break;
				}
			}
		}
		return result;
	}

	private void FixedUpdateRowers(float fixedDt, in ShipActuatorRecord actuatorInput, in MatrixFrame shipEntityGlobalFrame, float shipForwardSpeed)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_065e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0755: Unknown result type (might be due to invalid IL or missing references)
		//IL_075a: Unknown result type (might be due to invalid IL or missing references)
		//IL_076a: Unknown result type (might be due to invalid IL or missing references)
		//IL_076c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0771: Unknown result type (might be due to invalid IL or missing references)
		if (_ownerMissionShip.Physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Floating && _ownerMissionShip.FireHitPoints > 0f)
		{
			_timeLeftToUpdateNearbyShips -= fixedDt;
			if (_timeLeftToUpdateNearbyShips < 0f)
			{
				_timeLeftToUpdateNearbyShips = MBRandom.RandomFloatRanged(0.15f, 0.2f);
				BoundingBox physicsBoundingBoxWithChildren = _ownerMissionShip.Physics.PhysicsBoundingBoxWithChildren;
				Vec2 val = Vec2.Abs(((Vec3)(ref physicsBoundingBoxWithChildren.max)).AsVec2);
				physicsBoundingBoxWithChildren = _ownerMissionShip.Physics.PhysicsBoundingBoxWithChildren;
				Vec2 val2 = Vec2.Max(val, Vec2.Abs(((Vec3)(ref physicsBoundingBoxWithChildren.min)).AsVec2));
				float distanceLimit = ((Vec2)(ref val2)).Length + _maxOarLength;
				((List<(MissionShip, OarSidePhaseController.OarSide)>)(object)_nearbyShips).Clear();
				_navalShipsLogic?.FillClosestShips(in shipEntityGlobalFrame, distanceLimit, _nearbyShips, _ownerMissionShip);
			}
			int num = ((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Count + ((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Count;
			float num2 = (float)ComputeUsedOarCount() / (float)num;
			num2 = num2 * 0.9f + 0.1f;
			float maxForceMultiplierFromUser = 1f;
			FixedUpdateSideOars(fixedDt, in shipEntityGlobalFrame, _nearbyShips, _leftSideOars, ref maxForceMultiplierFromUser);
			FixedUpdateSideOars(fixedDt, in shipEntityGlobalFrame, _nearbyShips, _rightSideOars, ref maxForceMultiplierFromUser);
			UpdateRowerParameters(actuatorInput.RowerThrust, actuatorInput.RowerRotation, shipForwardSpeed, out var leftRowersNeededRevolutionRate, out var rightRowersNeededRevolutionRate);
			float num3 = ((leftRowersNeededRevolutionRate >= 0f) ? _rowersPhase : MBMath.WrapAngleSafe(MathF.PI * 2f - _rowersPhase));
			float num4 = ((rightRowersNeededRevolutionRate >= 0f) ? _rowersPhase : MBMath.WrapAngleSafe(MathF.PI * 2f - _rowersPhase));
			if (leftRowersNeededRevolutionRate == 0f && rightRowersNeededRevolutionRate == 0f)
			{
				num3 = MathF.PI;
				num4 = MathF.PI;
			}
			else if (leftRowersNeededRevolutionRate == 0f)
			{
				num3 = MathF.PI;
			}
			else if (rightRowersNeededRevolutionRate == 0f)
			{
				num4 = MathF.PI;
			}
			if (leftRowersNeededRevolutionRate != 0f)
			{
				_leftPhaseData.LastNonZeroRevolutionRate = leftRowersNeededRevolutionRate;
			}
			if (rightRowersNeededRevolutionRate != 0f)
			{
				_rightPhaseData.LastNonZeroRevolutionRate = rightRowersNeededRevolutionRate;
			}
			float num5 = MathF.Abs(rightRowersNeededRevolutionRate);
			float num6 = MathF.Abs(leftRowersNeededRevolutionRate);
			if (num5 == 1f && num6 == 1f)
			{
				_evenCycle = true;
			}
			bool partialTurn = false;
			if (!_evenCycle)
			{
				if (num5 < 1f && num5 > 0f)
				{
					num4 = MathF.PI;
					partialTurn = true;
				}
				else if (num6 < 1f && num6 > 0f)
				{
					num3 = MathF.PI;
					partialTurn = true;
				}
			}
			else if (num5 < 1f && num5 > 0f)
			{
				partialTurn = true;
			}
			else if (num6 < 1f && num6 > 0f)
			{
				partialTurn = true;
			}
			float num7 = MathF.Clamp(_ownerMissionShip.Physics.LastSubmergedHeightFactorForActuators, 0f, 1.2f);
			bool onPointTurn = leftRowersNeededRevolutionRate * rightRowersNeededRevolutionRate < 0f;
			float num8 = GetRowSpeedAccordingToPhase(num3, leftRowersNeededRevolutionRate >= 0f, partialTurn, onPointTurn);
			float num9 = GetRowSpeedAccordingToPhase(num4, rightRowersNeededRevolutionRate >= 0f, partialTurn, onPointTurn);
			if (num5 < 1f && num5 > 0f)
			{
				num9 = float.MaxValue;
			}
			else if (num6 < 1f && num6 > 0f)
			{
				num8 = float.MaxValue;
			}
			float num10 = MathF.Min(num8, num9);
			float num11 = MathF.PI * 2f * _oarsTipSpeedReferenceMultiplier * _oarsmenSpeedMultiplier;
			num11 *= num10;
			float num12 = ((leftRowersNeededRevolutionRate != 0f || rightRowersNeededRevolutionRate != 0f) ? MathF.Lerp(_lastFramePhaseRate, num11, 5f * fixedDt, 1E-05f) : 0f);
			(float, float) tuple = ComputeAverageOarTipPointForwardVelocities();
			float item = tuple.Item1;
			float item2 = tuple.Item2;
			float maxTipSpeed = _ownerMissionShip.MissionShipObject.OarsTipSpeed / MathF.Max(num7, 0.5f);
			(float, float) tuple2 = _leftOarsPhaseController.ComputeForceAndSlowDownFactor(leftRowersNeededRevolutionRate, item, num3, num12, _oarsmenForceMultiplier * maxForceMultiplierFromUser, _oarFrictionMultiplier * num7, maxTipSpeed);
			float item3 = tuple2.Item1;
			float item4 = tuple2.Item2;
			(float, float) tuple3 = _rightOarsPhaseController.ComputeForceAndSlowDownFactor(rightRowersNeededRevolutionRate, item2, num4, num12, _oarsmenForceMultiplier * maxForceMultiplierFromUser, _oarFrictionMultiplier * num7, maxTipSpeed);
			float item5 = tuple3.Item1;
			float item6 = tuple3.Item2;
			float num13 = MathF.Min(item4, item6);
			num12 = (_lastFramePhaseRate = num12 * num13);
			_rowersPhase += num12 * fixedDt;
			if (_rowersPhase >= MathF.PI)
			{
				_evenCycle = !_evenCycle;
			}
			_rowersPhase = MBMath.WrapAngleSafe(_rowersPhase);
			float num14 = num12;
			float num15 = num12;
			if (leftRowersNeededRevolutionRate == 0f)
			{
				num14 = 0f;
			}
			else if (rightRowersNeededRevolutionRate == 0f)
			{
				num15 = 0f;
			}
			bool isPartialStop = false;
			bool isPartialStop2 = false;
			if (!_evenCycle)
			{
				if (num5 < 1f && num5 > 0f)
				{
					num15 = 0f;
					num4 = MathF.PI;
					isPartialStop2 = true;
				}
				else if (num6 < 1f && num6 > 0f)
				{
					num14 = 0f;
					num3 = MathF.PI;
					isPartialStop = true;
				}
			}
			else
			{
				if (rightRowersNeededRevolutionRate < 1f && rightRowersNeededRevolutionRate > 0f && _rowersPhase > MathF.PI / 2f)
				{
					num4 = MathF.PI;
					isPartialStop2 = true;
				}
				else if (rightRowersNeededRevolutionRate > -1f && rightRowersNeededRevolutionRate < 0f && _rowersPhase > MathF.PI / 2f)
				{
					num4 = MathF.PI;
					isPartialStop2 = true;
				}
				if (leftRowersNeededRevolutionRate < 1f && leftRowersNeededRevolutionRate > 0f && _rowersPhase > MathF.PI / 2f)
				{
					num3 = MathF.PI;
					isPartialStop = true;
				}
				else if (leftRowersNeededRevolutionRate > -1f && leftRowersNeededRevolutionRate < 0f && _rowersPhase > MathF.PI / 2f)
				{
					num3 = MathF.PI;
					isPartialStop = true;
				}
			}
			bool toFullStop = false;
			if (leftRowersNeededRevolutionRate == 0f && rightRowersNeededRevolutionRate == 0f)
			{
				toFullStop = true;
				_rowersPhase = MathF.PI;
			}
			BlendPhaseTo(ref _leftPhaseData, num3, num14, num11, fixedDt, toFullStop, isPartialStop);
			BlendPhaseTo(ref _rightPhaseData, num4, num15, num11, fixedDt, toFullStop, isPartialStop2);
			_leftOarsPhaseController.SetPhaseData(_leftPhaseData.CurPhase, num14, _leftPhaseData.CycleArcSizeMult, leftRowersNeededRevolutionRate);
			_rightOarsPhaseController.SetPhaseData(_rightPhaseData.CurPhase, num15, _rightPhaseData.CycleArcSizeMult, rightRowersNeededRevolutionRate);
			Vec3 f = shipEntityGlobalFrame.rotation.f;
			f.z = 0f;
			((Vec3)(ref f)).Normalize();
			_rowingSoundEventData[0].NumberOfActiveOars = 0;
			_rowingSoundEventData[1].NumberOfActiveOars = 0;
			for (int i = 0; i < ((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Count; i++)
			{
				Vec3 localPosition = ((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[i].Item2.BladeContact;
				Vec3 force = num2 * item3 * _oarAppliedForceMultiplierForStoryMission * num7 * f;
				((List<ShipForce>)(object)_leftOarForces)[i] = new ShipForce(in localPosition, in force, ShipForce.SourceType.Oar, 1f);
				_rowingSoundEventData[0].NumberOfActiveOars += (((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[i].Item2.IsExtracted ? 1 : 0);
			}
			for (int j = 0; j < ((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Count; j++)
			{
				Vec3 localPosition2 = ((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[j].Item2.BladeContact;
				Vec3 force2 = num2 * item5 * _oarAppliedForceMultiplierForStoryMission * num7 * f;
				((List<ShipForce>)(object)_rightOarForces)[j] = new ShipForce(in localPosition2, in force2, ShipForce.SourceType.Oar, 1f);
				_rowingSoundEventData[1].NumberOfActiveOars += (((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[j].Item2.IsExtracted ? 1 : 0);
			}
			CalculateOarSoundPositionsAndParams();
		}
		else
		{
			StopRovers();
		}
	}

	private void StopRovers()
	{
		_leftOarsPhaseController.Stop();
		for (int i = 0; i < ((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Count; i++)
		{
			((List<ShipForce>)(object)_leftOarForces)[i] = ShipForce.None();
		}
		_rightOarsPhaseController.Stop();
		for (int j = 0; j < ((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Count; j++)
		{
			((List<ShipForce>)(object)_rightOarForces)[j] = ShipForce.None();
		}
		for (int k = 0; k < 2; k++)
		{
			SoundEvent oarsSoundEvents = _rowingSoundEventData[k].OarsSoundEvents;
			if (oarsSoundEvents != null)
			{
				oarsSoundEvents.Stop();
			}
			_rowingSoundEventData[k].OarsSoundEvents = null;
		}
	}

	private void FixedUpdateRudder(float fixedDt, in ShipActuatorRecord actuatorInput, in MatrixFrame shipEntityGlobalFrame, float shipForwardSpeed)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_0583: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05aa: Unknown result type (might be due to invalid IL or missing references)
		Vec3 u = shipEntityGlobalFrame.rotation.u;
		MatrixFrame val = shipEntityGlobalFrame;
		Vec3 val2 = ((MatrixFrame)(ref val)).TransformToParent(ref _rudderStockLocalPosition);
		Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)_ownerMissionShip).GameEntity, val2);
		Mat3 rotation = shipEntityGlobalFrame.rotation;
		Vec3 rudderStockLocalVelocity = ((Mat3)(ref rotation)).TransformToLocal(ref linearVelocityAtGlobalPointForEntityWithDynamicBody);
		float lengthSquared = ((Vec3)(ref rudderStockLocalVelocity)).LengthSquared;
		if (lengthSquared < 16f)
		{
			if (lengthSquared <= 1f)
			{
				rudderStockLocalVelocity = Vec3.Zero;
			}
			else
			{
				float length = ((Vec3)(ref rudderStockLocalVelocity)).Length;
				float num = 1f - (length - 1f) / 3f;
				rudderStockLocalVelocity = Vec3.Lerp(rudderStockLocalVelocity, new Vec3(0f, (float)MathF.Sign(rudderStockLocalVelocity.y) * length, 0f, -1f), num);
			}
		}
		Vec3 rudderStockLocalVelocityDirection = rudderStockLocalVelocity;
		rudderStockLocalVelocityDirection.z = 0f;
		rudderStockLocalVelocityDirection = (Vec3)((((Vec3)(ref rudderStockLocalVelocityDirection)).LengthSquared > 0.0001f) ? rudderStockLocalVelocityDirection : new Vec3(0f, -1f, 0f, -1f));
		((Vec3)(ref rudderStockLocalVelocityDirection)).Normalize();
		Vec3 unClampedRudderStabilityDirectionLocal = rudderStockLocalVelocityDirection;
		if (unClampedRudderStabilityDirectionLocal.y >= 0f)
		{
			unClampedRudderStabilityDirectionLocal = -unClampedRudderStabilityDirectionLocal;
		}
		float rudderRotationMax = _ownerMissionShip.MissionShipObject.RudderRotationMax;
		Vec2 asVec = ((Vec3)(ref unClampedRudderStabilityDirectionLocal)).AsVec2;
		float num2 = 0f - ((Vec2)(ref asVec)).AngleBetween(new Vec2(0f, -1f));
		float num3 = 0.8f;
		num2 = MathF.Clamp(num2, (0f - rudderRotationMax) * num3, rudderRotationMax * num3);
		float num4 = fixedDt * _ownerMissionShip.MissionShipObject.RudderRotationRate * 2f;
		_lastTargetRudderStabilityLocalRotation = num2;
		if (_lastTargetRudderStabilityLocalRotation > num2)
		{
			_lastTargetRudderStabilityLocalRotation -= num4;
			if (_lastTargetRudderStabilityLocalRotation < num2)
			{
				_lastTargetRudderStabilityLocalRotation = num2;
			}
		}
		else if (_lastTargetRudderStabilityLocalRotation < num2)
		{
			_lastTargetRudderStabilityLocalRotation += num4;
			if (_lastTargetRudderStabilityLocalRotation > num2)
			{
				_lastTargetRudderStabilityLocalRotation = num2;
			}
		}
		num2 = _lastTargetRudderStabilityLocalRotation;
		float rudderRotation = actuatorInput.RudderRotation;
		rudderRotation = (float)MathF.Sign(rudderRotation) * MathF.Pow(rudderRotation, 2f);
		int num5 = -MathF.Sign(rudderRotation);
		float num6 = rudderRotation * (float)MathF.Sign((shipForwardSpeed > -1f) ? 1f : shipForwardSpeed) * _ownerMissionShip.MissionShipObject.RudderRotationMax;
		VisualRudderPullDirection = MathF.Sign(num6);
		float num7 = fixedDt * _ownerMissionShip.MissionShipObject.RudderRotationRate * ((_lastAddedFromInputRudderLocalRotation * num6 <= 0f) ? 1f : 1f);
		if (_lastAddedFromInputRudderLocalRotation > num6)
		{
			_lastAddedFromInputRudderLocalRotation -= num7;
			if (_lastAddedFromInputRudderLocalRotation < num6)
			{
				_lastAddedFromInputRudderLocalRotation = num6;
			}
		}
		else if (_lastAddedFromInputRudderLocalRotation < num6)
		{
			_lastAddedFromInputRudderLocalRotation += num7;
			if (_lastAddedFromInputRudderLocalRotation > num6)
			{
				_lastAddedFromInputRudderLocalRotation = num6;
			}
		}
		_lastAddedFromInputRudderLocalRotation = MathF.Clamp(num2 + _lastAddedFromInputRudderLocalRotation, 0f - rudderRotationMax, rudderRotationMax) - num2;
		float num8 = MathF.Clamp(_ownerMissionShip.Physics.LastSubmergedHeightFactorForActuators, 0f, 1f);
		float rudderSurfaceArea = _ownerMissionShip.MissionShipObject.RudderBladeLength * _ownerMissionShip.MissionShipObject.RudderBladeHeight;
		float rudderDeflectionCoef = _ownerMissionShip.MissionShipObject.RudderDeflectionCoef;
		float rudderForceMax = _ownerMissionShip.MissionShipObject.RudderForceMax;
		Vec3 val3 = Vec3.Zero;
		float num9 = _lastAddedFromInputRudderLocalRotation;
		int num10 = ((_lastAddedFromInputRudderLocalRotation == 0f) ? 1 : (MathF.Ceiling(MathF.Abs(_lastAddedFromInputRudderLocalRotation) / 0.0017453292f) + 1));
		num10 = MBMath.ClampInt(num10, 1, 250);
		for (int i = 0; i <= num10; i++)
		{
			float num11 = (float)i / (float)num10 * _lastAddedFromInputRudderLocalRotation;
			float num12 = num2 + num11;
			num12 = MathF.Clamp(num12, 0f - rudderRotationMax, rudderRotationMax);
			var (val4, val5) = ComputeRudderDeflectionForce(num12, in unClampedRudderStabilityDirectionLocal, in rudderStockLocalVelocity, in rudderStockLocalVelocityDirection, rudderSurfaceArea);
			if (MathF.Sign(val5.x) == num5)
			{
				Vec3 val6 = val4 + val5;
				val6 *= MathF.Abs(u.z);
				val6 *= rudderDeflectionCoef;
				val6 *= num8;
				rotation = shipEntityGlobalFrame.rotation;
				val3 = ((Mat3)(ref rotation)).TransformToParent(ref val6);
				num9 = num12 - num2;
				if (MathF.Abs(val6.x) >= rudderForceMax)
				{
					val3 *= rudderForceMax / MathF.Abs(val6.x);
					break;
				}
			}
		}
		_lastAddedFromInputRudderLocalRotation = num9;
		float num13 = fixedDt * _ownerMissionShip.MissionShipObject.RudderRotationRate * 0.5f;
		if (_lastAddedFromInputRudderLocalRotation > num9)
		{
			_lastAddedFromInputRudderLocalRotation -= num13;
			if (_lastAddedFromInputRudderLocalRotation < num9)
			{
				_lastAddedFromInputRudderLocalRotation = num9;
			}
		}
		else if (_lastAddedFromInputRudderLocalRotation < num9)
		{
			_lastAddedFromInputRudderLocalRotation += num13;
			if (_lastAddedFromInputRudderLocalRotation > num9)
			{
				_lastAddedFromInputRudderLocalRotation = num9;
			}
		}
		num9 = _lastAddedFromInputRudderLocalRotation;
		_lastRudderLocalRotation = _rudderLocalRotation;
		float num14 = MathF.Clamp(num2 + num9, 0f - rudderRotationMax, rudderRotationMax);
		_rudderLocalRotation = MathF.Lerp(_rudderLocalRotation, num14, fixedDt * 5f, 1E-05f);
		val3 *= 1f + _ownerMissionShip.ShipOrigin.RudderSurfaceAreaFactor;
		Vec3 val7 = default(Vec3);
		((Vec3)(ref val7))._002Ector(0f, -1f, 0f, -1f);
		((Vec3)(ref val7)).RotateAboutZ(_rudderLocalRotation);
		_rudderShipForce = new ShipForce(_rudderStockLocalPosition + val7 * (_ownerMissionShip.MissionShipObject.RudderBladeLength * 0.5f), in val3, ShipForce.SourceType.Rudder, rudderDeflectionCoef);
		_shipPresenceSoundParam = MathF.Min(MathF.Abs(((Vec3)(ref _rudderShipForce.Force)).Length / 10000f), 1f);
		_rudderStressSoundParam = ((Vec3)(ref _rudderShipForce.Force)).LengthSquared / (rudderForceMax * rudderForceMax);
	}

	private void OnParallelTickRudder(float dt)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		float num2 = default(float);
		_cachedOwnerScene.GetInterpolationFactorForBodyWorldTransformSmoothing(ref num, ref num2);
		VisualRudderLocalRotation = MathF.Lerp(_lastRudderLocalRotation, _rudderLocalRotation, num, 1E-05f);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerMissionShip).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 val = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref _rudderStockLocalPosition);
		float num3 = MathF.Clamp(_ownerMissionShip.Physics.LastSubmergedHeightFactorForActuators, 0f, 1f);
		float num4 = 0.15f * dt * num3;
		float num5 = _shipPresenceSoundParam * 0.25f + 0.1f;
		_cachedOwnerScene.AddWaterWakeWithCapsule(val, num5 * 1.5f, val, num5, num4, 0f);
	}

	private int ComputeExtractedOarCount()
	{
		int num = 0;
		for (int i = 0; i < ((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Count; i++)
		{
			if (((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[i].Item2.IsExtracted)
			{
				num++;
			}
		}
		for (int j = 0; j < ((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Count; j++)
		{
			if (((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[j].Item2.IsExtracted)
			{
				num++;
			}
		}
		return num;
	}

	private int ComputeUsedOarCount()
	{
		int num = 0;
		for (int i = 0; i < ((List<(GameEntity, MissionOar)>)(object)_leftSideOars).Count; i++)
		{
			if (((List<(GameEntity, MissionOar)>)(object)_leftSideOars)[i].Item2.IsUsed)
			{
				num++;
			}
		}
		for (int j = 0; j < ((List<(GameEntity, MissionOar)>)(object)_rightSideOars).Count; j++)
		{
			if (((List<(GameEntity, MissionOar)>)(object)_rightSideOars)[j].Item2.IsUsed)
			{
				num++;
			}
		}
		return num;
	}

	private (float, float) ComputeAverageOarTipPointForwardVelocities()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerMissionShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		gameEntity = ((ScriptComponentBehavior)_ownerMissionShip).GameEntity;
		Vec3 centerOfMass = ((WeakGameEntity)(ref gameEntity)).CenterOfMass;
		Vec3 val = ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref centerOfMass);
		Vec3 linearVelocity = _ownerMissionShip.Physics.LinearVelocity;
		Vec3 angularVelocity = _ownerMissionShip.Physics.AngularVelocity;
		Vec3 val2 = ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref _leftSideAverageOarLocalPos) - val;
		Vec3 val3 = Vec3.CrossProduct(angularVelocity, val2);
		float item = Vec3.DotProduct(linearVelocity + val3, bodyWorldTransform.rotation.f);
		Vec3 val4 = ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref _rightSideAverageOarLocalPos) - val;
		Vec3 val5 = Vec3.CrossProduct(angularVelocity, val4);
		float item2 = Vec3.DotProduct(linearVelocity + val5, bodyWorldTransform.rotation.f);
		return (item, item2);
	}

	private void FixedUpdateSideOars(float fixedDt, in MatrixFrame shipGlobalFrame, MBList<(MissionShip ship, OarSidePhaseController.OarSide shipSide)> nearbyShips, MBList<(GameEntity entity, MissionOar oar)> shipOars, ref float maxForceMultiplierFromUser)
	{
		for (int i = 0; i < ((List<(GameEntity, MissionOar)>)(object)shipOars).Count; i++)
		{
			MissionOar item = ((List<(GameEntity, MissionOar)>)(object)shipOars)[i].Item2;
			item.FixedUpdate(fixedDt, in shipGlobalFrame, nearbyShips);
			maxForceMultiplierFromUser = MathF.Max(maxForceMultiplierFromUser, item.ForceMultiplierFromUserAgent);
		}
	}

	private void UpdateRowerParameters(float rowersThrustRate, float rowersRotationRate, float shipForwardSpeed, out float leftRowersNeededRevolutionRate, out float rightRowersNeededRevolutionRate)
	{
		if (rowersThrustRate == 0f && rowersRotationRate != 0f)
		{
			if (MathF.Abs(shipForwardSpeed) <= 6f)
			{
				leftRowersNeededRevolutionRate = 0f - rowersRotationRate;
				rightRowersNeededRevolutionRate = rowersRotationRate;
			}
			else if (rowersRotationRate > 0f)
			{
				rightRowersNeededRevolutionRate = rowersRotationRate;
				leftRowersNeededRevolutionRate = 0f;
			}
			else
			{
				leftRowersNeededRevolutionRate = 0f - rowersRotationRate;
				rightRowersNeededRevolutionRate = 0f;
			}
			return;
		}
		leftRowersNeededRevolutionRate = rowersThrustRate;
		rightRowersNeededRevolutionRate = rowersThrustRate;
		if (rowersRotationRate != 0f)
		{
			float num = 0.5f;
			if (shipForwardSpeed * rowersThrustRate < 0f && MathF.Abs(shipForwardSpeed) > 6f)
			{
				num = 0f;
			}
			if (rowersThrustRate * rowersRotationRate > 0f)
			{
				leftRowersNeededRevolutionRate = num;
			}
			else
			{
				rightRowersNeededRevolutionRate = num;
			}
		}
	}

	private IShipOarScriptComponent GetOarScriptFromEntity(WeakGameEntity oarEntity)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		IShipOarScriptComponent shipOarScriptComponent = null;
		WeakGameEntity val = oarEntity;
		while (((WeakGameEntity)(ref val)).IsValid && shipOarScriptComponent == null)
		{
			shipOarScriptComponent = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<ShipOarMachine>();
			if (shipOarScriptComponent == null)
			{
				shipOarScriptComponent = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<ShipUnmannedOar>();
			}
			val = ((WeakGameEntity)(ref val)).Parent;
		}
		return shipOarScriptComponent;
	}

	internal static float ComputeActuatorParameter(float value, float target, float dt, float incrementRate)
	{
		float num = target - value;
		float num2 = Math.Min(Math.Abs(num), dt * incrementRate);
		return value + (float)MathF.Sign(num) * num2;
	}

	private static (Vec3, Vec3) ComputeRudderDeflectionForce(float totalTargetRot, in Vec3 unClampedRudderStabilityDirectionLocal, in Vec3 rudderStockLocalVelocity, in Vec3 rudderStockLocalVelocityDirection, float rudderSurfaceArea)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(0f, -1f, 0f, -1f);
		((Vec3)(ref val)).RotateAboutZ(totalTargetRot);
		Vec2 asVec = ((Vec3)(ref val)).AsVec2;
		Vec3 val2 = unClampedRudderStabilityDirectionLocal;
		float num = ((Vec2)(ref asVec)).AngleBetween(((Vec3)(ref val2)).AsVec2);
		if (num < -MathF.PI / 2f)
		{
			num += MathF.PI;
		}
		else if (num > MathF.PI / 2f)
		{
			num -= MathF.PI;
		}
		float num2 = 0.5f * NavalDLC.Missions.NavalPhysics.NavalPhysics.GetWaterDensity();
		val2 = rudderStockLocalVelocity;
		float num3 = num2 * ((Vec3)(ref val2)).LengthSquared;
		float num4 = MathF.Abs(num);
		float num5 = MathF.Sign((num == 0f) ? 1f : num);
		float num6 = 0.72f * (MathF.PI * 2f * num);
		float num7 = 1.1f * MathF.Sin(2f * num4) * num5;
		float num8 = MathF.Sin(num4);
		float num9 = MBMath.SmoothStep(MathF.PI / 15f, 0.61086524f, num4);
		float num10 = MBMath.Lerp(num6, num7, num9, 1E-05f);
		float num11 = (0.06f + 0.1f * num10 * num10 + 1.1f * num8) * num8;
		float num12 = num10 * num3 * rudderSurfaceArea;
		float num13 = num11 * num3 * rudderSurfaceArea;
		Vec3 val3 = -rudderStockLocalVelocityDirection;
		Vec3 val4 = val3;
		((Vec3)(ref val4)).RotateAboutZ(MathF.PI / 2f);
		Vec3 item = num13 * val3;
		Vec3 item2 = num12 * val4;
		return (item, item2);
	}

	public void SetOarAppliedForceMultiplierForStoryMission(float newOarAppliedForceMultiplierForStoryMission)
	{
		_oarAppliedForceMultiplierForStoryMission = newOarAppliedForceMultiplierForStoryMission;
	}
}
