using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.NavalPhysics;

[ScriptComponentParams("ship_visual_only", "")]
public class NavalPhysics : ScriptComponentBehavior
{
	public struct NavalPhysicsParameters
	{
		public float OverrideMass;

		public float MassMultiplier;

		public Vec3 MomentOfInertiaMultiplier;

		public float FloatingForceMultiplier;

		public float MaximumSubmergedVolumeRatio;

		public float ForwardDragMultiplier;

		public LinearFrictionTerm LinearFrictionMultiplier;

		public Vec3 AngularFrictionMultiplier;

		public float TorqueMultiplierOfLateralBuoyantForces;

		public Vec3 TorqueMultiplierOfVerticalBuoyantForces;

		public float UpSideDownFrictionMultiplier;

		public float MaxLinearSpeedForLateralDragCenterShift;

		public float MaxLateralDragShift;

		public float LateralDragShiftCriticalAngle;

		public float StepAgentWeightMultiplier;

		public bool MakeAgentsStepToEntityEvenUnderWater;
	}

	public struct BuoyancyComputationResult
	{
		public float PitchSubmergedAreaFactor;

		public float RollSubmergedAreaFactor;

		public float SubmergedHeightFactor;

		public float SubmergedFloaterCountFactor;

		public Vec3 AvgLocalBuoyancyApplyPosition;

		public Vec3 NetGlobalBuoyancyForce;

		public Vec3 NetBuoyancyTorque;

		public bool SimulatingAirFriction;

		public void Reset()
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			PitchSubmergedAreaFactor = 0f;
			RollSubmergedAreaFactor = 0f;
			SubmergedHeightFactor = 0f;
			SubmergedFloaterCountFactor = 1f;
			AvgLocalBuoyancyApplyPosition = Vec3.Zero;
			NetGlobalBuoyancyForce = Vec3.Zero;
			NetBuoyancyTorque = Vec3.Zero;
			SimulatingAirFriction = false;
		}
	}

	public struct DragForceComputationResult
	{
		public Vec3 CenterOfLateralDragLocal;

		public Vec3 LateralDragForceGlobal;

		public Vec3 CenterOfVerticalDragLocal;

		public Vec3 VerticalDragForceGlobal;

		public Vec3 CenterOfLongitudinalDragLocal;

		public Vec3 LongitudinalDragForceGlobal;

		public Vec3 AngularDragTorqueGlobal;

		public Vec3 DriftForceFromAngularDragGlobal;

		public void Reset()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			CenterOfLateralDragLocal = Vec3.Zero;
			LateralDragForceGlobal = Vec3.Zero;
			CenterOfVerticalDragLocal = Vec3.Zero;
			VerticalDragForceGlobal = Vec3.Zero;
			CenterOfLongitudinalDragLocal = Vec3.Zero;
			LongitudinalDragForceGlobal = Vec3.Zero;
			AngularDragTorqueGlobal = Vec3.Zero;
			DriftForceFromAngularDragGlobal = Vec3.Zero;
		}
	}

	public struct WaterDriftForceData
	{
		public float DriftSpeed;

		public float DriftForceTimer;

		public MBFastRandom DriftRandom;

		public Vec3 ResultForce;

		public void Initialize()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			DriftSpeed = 0f;
			DriftForceTimer = 0f;
			DriftRandom = new MBFastRandom();
			DriftForceTimer = DriftRandom.NextFloatRanged(0f, MathF.PI * 10f);
			ResultForce = Vec3.Zero;
		}
	}

	public enum ShipPart : byte
	{
		LeftBack,
		RightBack,
		LeftMid,
		RightMid,
		LeftFront,
		RightFront,
		Count
	}

	public enum SinkingState : byte
	{
		Floating,
		Sinking,
		Sunk
	}

	public const byte VerticalPartitionCount = 3;

	public const byte HorizontalPartitionCount = 2;

	private NavalPhysicsParameters _physicsParameters;

	private float _stabilityAvgSubmergedHeight;

	private int _stabilitySubmergedFloaterCount;

	private float _minFloaterEntitialBottomPos;

	private Scene _ownScene;

	private float _maxFloaterEntitialTopPos;

	private float _minimumFloaterDurabilityToFloatWhileNotSinking;

	[EditableScriptComponentVariable(false, "")]
	public Vec3 AngularDragTerm;

	[EditableScriptComponentVariable(true, "Sink")]
	private SimpleButton _sinkButton = new SimpleButton();

	private float _angularDragYSideComponentTerm;

	[EditableScriptComponentVariable(false, "")]
	public Vec3 AngularDampingTerm;

	private float _angularDampingYSideComponentTerm;

	private float _cachedMass;

	private float[] _shipPartsDurabilities;

	private ShipPart[] _floaterVolumesShipPartMap;

	private float[] _shipPartsTargetDurabilities;

	private VolumeDataForSubmergeComputation[] _floaterVolumeData;

	private UIntPtr _floaterVolumeDataPinnedPointer = UIntPtr.Zero;

	private GCHandle _floaterVolumeDataPinnedGCHandler;

	private float _totalFloaterVolumeCached;

	private ShipForceRecord _shipForceRecord;

	private BuoyancyComputationResult _buoyancyComputationResult;

	private DragForceComputationResult _dragComputationResult;

	private MatrixFrame _anchorGlobalFrame;

	private float _anchorForceMultiplier = 1f;

	private Vec3 _weightedAgentsPosition;

	private float _totalMass;

	private Vec3 _committedWeightedAgentsPosition;

	private float _committedTotalMass;

	private WaterDriftForceData _continuousDriftForceData;

	public bool IsInitialized { get; private set; }

	public Vec3 PhysicsBoundingBoxWithChildrenSize { get; private set; }

	public Vec3 PhysicsBoundingBoxSizeWithoutChildren { get; private set; }

	public BoundingBox PhysicsBoundingBoxWithChildren { get; private set; }

	public BoundingBox PhysicsBoundingBoxWithoutChildren { get; private set; }

	public float Mass => _cachedMass;

	public Vec3 LocalCenterOfMass
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			return ((WeakGameEntity)(ref gameEntity)).CenterOfMass;
		}
	}

	public Vec3 MassSpaceInertia => GameEntityPhysicsExtensions.GetMassSpaceInertia(((ScriptComponentBehavior)this).GameEntity);

	public ref readonly NavalPhysicsParameters PhysicsParameters => ref _physicsParameters;

	public SinkingState NavalSinkingState { get; private set; }

	private float StabilitySubmergedVolume => Mass / (GetWaterDensity() * _physicsParameters.FloatingForceMultiplier);

	public float FloatingForceMultiplierWhenDamaged => StabilitySubmergedVolume / (_totalFloaterVolumeCached * _physicsParameters.MaximumSubmergedVolumeRatio);

	public float StabilitySubmergedHeightOfShip { get; private set; }

	public float LastSubmergedHeightFactorForActuators { get; private set; }

	public LinearFrictionTerm LinearDragTerm { get; private set; }

	public LinearFrictionTerm LinearDampingTerm { get; private set; }

	public float MinFloaterEntitialBottomPos => _minFloaterEntitialBottomPos;

	public float MaxFloaterEntitialTopPos => _maxFloaterEntitialTopPos;

	public float AngularDragYSideComponentTerm => _angularDragYSideComponentTerm;

	public LinearFrictionTerm ConstantLinearDampingTerm { get; private set; }

	public float AngularDampingYSideComponentTerm => _angularDampingYSideComponentTerm;

	public Vec3 LinearVelocity => GameEntityPhysicsExtensions.GetLinearVelocity(((ScriptComponentBehavior)this).GameEntity);

	public Vec3 AngularVelocity => GameEntityPhysicsExtensions.GetAngularVelocity(((ScriptComponentBehavior)this).GameEntity);

	public bool IsAnchored { get; private set; }

	public MatrixFrame AnchorGlobalFrame => _anchorGlobalFrame;

	protected override void OnEditorInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_ownScene = ((WeakGameEntity)(ref gameEntity)).Scene;
		if (_ownScene.GetEnginePhysicsEnabled())
		{
			if (!IsInitialized)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				if (!((WeakGameEntity)(ref gameEntity)).HasScriptOfType<MissionShip>())
				{
					((ScriptComponentBehavior)this).OnInit();
				}
			}
		}
		else
		{
			IsInitialized = false;
		}
	}

	protected override void OnInit()
	{
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		if (!IsInitialized)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<MissionShip>() == null)
			{
				StabilitySubmergedHeightOfShip = 0f;
				_weightedAgentsPosition = Vec3.Zero;
				_totalMass = 0f;
				_committedWeightedAgentsPosition = Vec3.Zero;
				_committedTotalMass = 0f;
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				CustomNavalPhysicsParameters customNavalPhysicsParameters = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<CustomNavalPhysicsParameters>() ?? new CustomNavalPhysicsParameters();
				ShipPhysicsReference basePhysicsRef = (customNavalPhysicsParameters.BehaveLikeShip ? ShipPhysicsReference.Default : ShipPhysicsReference.DefaultDebris);
				NavalPhysicsParameters physicsParameters = new NavalPhysicsParameters
				{
					OverrideMass = 0f,
					MassMultiplier = 1f,
					MomentOfInertiaMultiplier = Vec3.One,
					FloatingForceMultiplier = customNavalPhysicsParameters.FloatingForceMultiplier,
					LinearFrictionMultiplier = new LinearFrictionTerm(customNavalPhysicsParameters.LinearFrictionMultiplierRight, customNavalPhysicsParameters.LinearFrictionMultiplierLeft, customNavalPhysicsParameters.LinearFrictionMultiplierForward, customNavalPhysicsParameters.LinearFrictionMultiplierBackward, customNavalPhysicsParameters.LinearFrictionMultiplierUp, customNavalPhysicsParameters.LinearFrictionMultiplierDown),
					AngularFrictionMultiplier = customNavalPhysicsParameters.AngularFrictionMultiplier,
					MaximumSubmergedVolumeRatio = 0.7f,
					ForwardDragMultiplier = 1f,
					TorqueMultiplierOfLateralBuoyantForces = 1f,
					TorqueMultiplierOfVerticalBuoyantForces = Vec3.One,
					UpSideDownFrictionMultiplier = 1f,
					MaxLinearSpeedForLateralDragCenterShift = 1E+09f,
					MaxLateralDragShift = 0f,
					LateralDragShiftCriticalAngle = 1f,
					StepAgentWeightMultiplier = 1f,
					MakeAgentsStepToEntityEvenUnderWater = false
				};
				Initialize(physicsParameters, basePhysicsRef);
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_ownScene = ((WeakGameEntity)(ref gameEntity)).Scene;
	}

	protected override void OnPreInit()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("batched_physics_entity");
		if (firstChildEntityWithTag != WeakGameEntity.Invalid)
		{
			GameEntityPhysicsExtensions.CreateVariableRatePhysics(firstChildEntityWithTag, true);
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			if (child != firstChildEntityWithTag)
			{
				GameEntityPhysicsExtensions.CreateVariableRatePhysics(child, true);
			}
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		if (_floaterVolumeDataPinnedPointer != UIntPtr.Zero)
		{
			_floaterVolumeDataPinnedGCHandler.Free();
			_floaterVolumeDataPinnedPointer = UIntPtr.Zero;
		}
	}

	public void Initialize(NavalPhysicsParameters physicsParameters, ShipPhysicsReference basePhysicsRef)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		_shipForceRecord = ShipForceRecord.None();
		_continuousDriftForceData.Initialize();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).Scene.SetOnCollisionFilterCallbackActive(true);
		UpdateShipPhysics(physicsParameters, basePhysicsRef);
		LoadFloaterVolumes();
		PreComputeAngularDragTerms(out AngularDampingTerm, out AngularDragTerm, out _angularDampingYSideComponentTerm, out _angularDragYSideComponentTerm);
		if (!physicsParameters.MakeAgentsStepToEntityEvenUnderWater)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).AddBodyFlags((BodyFlags)(-1073741824), true);
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).Scene.SetFixedTickCallbackActive(true);
		IsInitialized = true;
		IsAnchored = false;
		_anchorGlobalFrame = MatrixFrame.Zero;
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)52;
	}

	protected override void OnSaveAsPrefab()
	{
	}

	public static float GetAirDensity()
	{
		return GameModels.Instance.ShipPhysicsParametersModel.GetAirDensity();
	}

	public static float GetWaterDensity()
	{
		return GameModels.Instance.ShipPhysicsParametersModel.GetWaterDensity();
	}

	public void CheckPrefab()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		float waterDensity = GetWaterDensity();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		float num = ((WeakGameEntity)(ref gameEntity)).Mass * 9.806f * 1.01f;
		float num2 = _totalFloaterVolumeCached * waterDensity * 9.806f * _physicsParameters.FloatingForceMultiplier;
		if (num2 <= num)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<MissionShip>();
		}
		_ = num2 * FloatingForceMultiplierWhenDamaged;
	}

	public void OnShipObjectUpdated(NavalPhysicsParameters physicsParameters, ShipPhysicsReference basePhysicsRef)
	{
		UpdateShipPhysics(physicsParameters, basePhysicsRef);
	}

	public void SetShipForceRecord(in ShipForceRecord record)
	{
		_shipForceRecord = record;
	}

	public void SetContinuousDriftSpeed(float driftSpeed)
	{
		_continuousDriftForceData.DriftSpeed = driftSpeed;
	}

	public void SetAnchor(bool isAnchored, bool anchorInPlace = false, float forceMultiplier = 1f)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		IsAnchored = isAnchored;
		if (IsAnchored)
		{
			if (((MatrixFrame)(ref _anchorGlobalFrame)).IsZero || anchorInPlace)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				Vec2 position = ((Vec3)(ref globalFrame.origin)).AsVec2;
				Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				SetAnchorFrame(in position, ((Vec2)(ref asVec)).Normalized(), forceMultiplier);
			}
		}
		else
		{
			_anchorGlobalFrame = MatrixFrame.Zero;
			_anchorForceMultiplier = 1f;
		}
	}

	public void SetAnchorFrame(in Vec2 position, in Vec2 direction, float forceMultiplier = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		float waterLevelAtPosition = ((WeakGameEntity)(ref gameEntity)).Scene.GetWaterLevelAtPosition(position, true, false);
		ref MatrixFrame anchorGlobalFrame = ref _anchorGlobalFrame;
		Vec2 val = position;
		anchorGlobalFrame.origin = ((Vec2)(ref val)).ToVec3(waterLevelAtPosition);
		ref Mat3 rotation = ref _anchorGlobalFrame.rotation;
		val = direction;
		rotation.f = ((Vec2)(ref val)).ToVec3(0f);
		((Mat3)(ref _anchorGlobalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		_anchorForceMultiplier = forceMultiplier;
	}

	protected unsafe override void OnParallelFixedTick(float fixedDt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (GameEntityPhysicsExtensions.HasDynamicRigidBodyAndActiveSimulation(((ScriptComponentBehavior)this).GameEntity))
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			TWSharedMutexReadLock val = default(TWSharedMutexReadLock);
			((TWSharedMutexReadLock)(ref val))._002Ector(Scene.PhysicsAndRayCastLock);
			Vec3 globalLinearVelocity;
			Vec3 globalAngularVelocity;
			Vec3 massSpaceLocalInertia;
			try
			{
				globalLinearVelocity = LinearVelocity;
				globalAngularVelocity = AngularVelocity;
				massSpaceLocalInertia = MassSpaceInertia;
			}
			finally
			{
				((IDisposable)(*(TWSharedMutexReadLock*)(&val))/*cast due to .constrained prefix*/).Dispose();
			}
			if (IsAnchored)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				float waterLevelAtPosition = ((WeakGameEntity)(ref gameEntity)).Scene.GetWaterLevelAtPosition(((Vec3)(ref _anchorGlobalFrame.origin)).AsVec2, true, false);
				_anchorGlobalFrame.origin.z = waterLevelAtPosition;
			}
			UpdateFloaterVolumeData();
			TickFloaterDurabilities(fixedDt);
			FillWaterHeightQueryResultsIterative();
			ComputeBuoyancyForces(fixedDt, in globalLinearVelocity, in globalAngularVelocity);
			ComputeDragForces(fixedDt, in globalLinearVelocity, in globalAngularVelocity, in massSpaceLocalInertia);
			ComputeContinuousDriftForce(fixedDt);
		}
	}

	protected override void OnFixedTick(float fixedDt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (GameEntityPhysicsExtensions.HasDynamicRigidBodyAndActiveSimulation(((ScriptComponentBehavior)this).GameEntity))
		{
			LastSubmergedHeightFactorForActuators = _buoyancyComputationResult.SubmergedHeightFactor;
			ApplyForceToDynamicBody(MBGlobals.GravitationalAcceleration * Mass, (ForceMode)0);
			ApplyAgentForces();
			ApplyBuoyancyForces();
			ApplyDragForces();
			ApplyActuatorForces();
			ApplyAnchorForces();
			ApplyContinuousDriftForce();
		}
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).Scene.GetEnginePhysicsEnabled())
		{
			if (!IsInitialized)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				if (!((WeakGameEntity)(ref gameEntity)).HasScriptOfType<MissionShip>())
				{
					((ScriptComponentBehavior)this).OnInit();
				}
			}
		}
		else
		{
			IsInitialized = false;
		}
	}

	public void ApplyGlobalForceAtLocalPos(in Vec3 localPos, in Vec3 globalForceVec, ForceMode forceMode = (ForceMode)0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		GameEntityPhysicsExtensions.ApplyGlobalForceAtLocalPosToDynamicBody(((ScriptComponentBehavior)this).GameEntity, localPos, globalForceVec, forceMode);
	}

	public void ApplyLocalForceAtLocalPos(in Vec3 localPos, in Vec3 localForceVec, ForceMode forceMode = (ForceMode)0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		GameEntityPhysicsExtensions.ApplyLocalForceAtLocalPosToDynamicBody(((ScriptComponentBehavior)this).GameEntity, localPos, localForceVec, forceMode);
	}

	public void ApplyForceToDynamicBody(in Vec3 forceVec, ForceMode forceMode = (ForceMode)0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		GameEntityPhysicsExtensions.ApplyForceToDynamicBody(((ScriptComponentBehavior)this).GameEntity, forceVec, forceMode);
	}

	public void ApplyTorque(in Vec3 torqueVec, ForceMode forceMode = (ForceMode)0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		GameEntityPhysicsExtensions.ApplyTorqueToDynamicBody(((ScriptComponentBehavior)this).GameEntity, torqueVec, forceMode);
	}

	public MatrixFrame GetGlobalMassFrame()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		MatrixFrame result = default(MatrixFrame);
		result.rotation = bodyWorldTransform.rotation;
		Vec3 localCenterOfMass = LocalCenterOfMass;
		result.origin = ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref localCenterOfMass);
		return result;
	}

	public Vec3 GetClosestPointToBoundingBox(in Vec3 localPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vec3 min = PhysicsBoundingBoxWithoutChildren.min;
		Vec3 max = PhysicsBoundingBoxWithoutChildren.max;
		float num = Math.Max(min.x, Math.Min(max.x, localPoint.x));
		float num2 = Math.Max(min.y, Math.Min(max.y, localPoint.y));
		float num3 = Math.Max(min.z, Math.Min(max.z, localPoint.z));
		return new Vec3(num, num2, num3, -1f);
	}

	public void SetTargetDurabilityOfPart(int part, float targetDurability)
	{
		_shipPartsTargetDurabilities[part] = MathF.Max(0.01f, MathF.Min(_shipPartsTargetDurabilities[part], targetDurability));
	}

	private void SetTargetDurabilityToAdjacentParts(int part, float targetDurability)
	{
		if (part - 1 >= 0 && part % 2 - (part - 1) % 2 == 1)
		{
			SetTargetDurabilityOfPart(part - 1, targetDurability);
		}
		if (part + 1 < 6 && (part + 1) % 2 - part % 2 == 1)
		{
			SetTargetDurabilityOfPart(part + 1, targetDurability);
		}
		if (part - 2 >= 0)
		{
			SetTargetDurabilityOfPart(part - 2, targetDurability);
		}
		if (part + 2 < 6)
		{
			SetTargetDurabilityOfPart(part + 2, targetDurability);
		}
	}

	private void TickFloaterDurabilities(float fixedDt)
	{
		float floatingForceMultiplierWhenDamaged = FloatingForceMultiplierWhenDamaged;
		for (int i = 0; i < 6; i++)
		{
			float num = ((NavalSinkingState != SinkingState.Floating) ? _shipPartsTargetDurabilities[i] : MathF.Max(_shipPartsTargetDurabilities[i], floatingForceMultiplierWhenDamaged));
			if (num < _shipPartsDurabilities[i])
			{
				_shipPartsDurabilities[i] = MathF.Max(num, _shipPartsDurabilities[i] - 0.2f * fixedDt * MathF.Max(0.5f, _shipPartsDurabilities[i]));
				float targetDurability = ((_shipPartsDurabilities[i] <= 0.01f) ? 0.01f : MathF.Min(1f, 1f - (1f - _shipPartsDurabilities[i]) / 2f));
				SetTargetDurabilityToAdjacentParts(i, targetDurability);
			}
		}
	}

	protected override bool CanPhysicsCollideBetweenTwoEntities(WeakGameEntity myEntity, BodyFlags myEntityBodyFlags, WeakGameEntity otherEntity, BodyFlags otherEntityBodyFlags)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!Extensions.HasAnyFlag<BodyFlags>(myEntityBodyFlags, (BodyFlags)int.MinValue))
		{
			return true;
		}
		if (!Extensions.HasAnyFlag<BodyFlags>(otherEntityBodyFlags, (BodyFlags)16))
		{
			return true;
		}
		if (Extensions.HasAnyFlag<BodyFlags>(otherEntityBodyFlags, (BodyFlags)int.MinValue))
		{
			return Extensions.HasAnyFlag<BodyFlags>(otherEntityBodyFlags, (BodyFlags)8);
		}
		return !Extensions.HasAnyFlag<BodyFlags>(otherEntityBodyFlags, (BodyFlags)8);
	}

	private void FillWaterHeightQueryResultsIterative()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		_ownScene.GetBulkWaterLevelAtVolumes(_floaterVolumeDataPinnedPointer, _floaterVolumeData.Length, ref bodyWorldTransform);
	}

	private static (float, float) RungeKuttaIntegrationStepForBuoyancyAndGravity(float prevIterationUpSpeed, float prevIterationUpAcceleration, float baseShipUpSpeed, float fixedDt, float baseSubmergedHeight, float volumeHeight, float volumeWidthMultDepth, float waterDensity, float durabilityMultiplier, float curInvVolumeMass)
	{
		float num = prevIterationUpSpeed * fixedDt;
		float item = MathF.Clamp(baseSubmergedHeight - num, 0f, volumeHeight) * volumeWidthMultDepth * waterDensity * 9.806f * durabilityMultiplier * curInvVolumeMass + -9.806f;
		float item2 = baseShipUpSpeed + fixedDt * prevIterationUpAcceleration;
		return (item, item2);
	}

	private void ComputeBuoyancyForces(float fixedDt, in Vec3 globalLinearVelocity, in Vec3 globalAngularVelocity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Expected I4, but got Unknown
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Expected I4, but got Unknown
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected I4, but got Unknown
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_06aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		MatrixFrame globalMassFrame = GetGlobalMassFrame();
		float waterDensity = GetWaterDensity();
		float z = globalLinearVelocity.z;
		float num = Mass / _totalFloaterVolumeCached;
		float num2 = ((NavalSinkingState != SinkingState.Floating) ? 0f : _minimumFloaterDurabilityToFloatWhileNotSinking);
		_buoyancyComputationResult.Reset();
		int num3 = 0;
		Vec3 localCenterOfMass = LocalCenterOfMass;
		float floatingForceMultiplier = _physicsParameters.FloatingForceMultiplier;
		float num4 = 0f;
		Vec3 val = Vec3.Zero;
		float num5 = 0f;
		Vec3 val2 = ((Mat3)(ref globalMassFrame.rotation)).TransformToLocal(ref globalAngularVelocity);
		Mat3 identity = Mat3.Identity;
		Vec3 val3 = ((Mat3)(ref bodyWorldTransform.rotation)).TransformToLocal(ref Vec3.Up);
		float num6 = 0f;
		float num7 = 0f;
		float num8 = (LinearDampingTerm.Up / LinearDampingTerm.Down + LinearDragTerm.Up / LinearDragTerm.Down) * 0.5f;
		Vec2 val8 = default(Vec2);
		for (int i = 0; i < _floaterVolumeData.Length; i++)
		{
			VolumeDataForSubmergeComputation val4 = _floaterVolumeData[i];
			float inOutWaterHeightWrtVolume = val4.InOutWaterHeightWrtVolume;
			if (inOutWaterHeightWrtVolume > 0f)
			{
				float height = ((VolumeDataForSubmergeComputation)(ref val4)).Height;
				float width = ((VolumeDataForSubmergeComputation)(ref val4)).Width;
				float depth = ((VolumeDataForSubmergeComputation)(ref val4)).Depth;
				float num9 = MathF.Clamp(inOutWaterHeightWrtVolume, 0f, height);
				num3++;
				num5 += num9;
				Vec3 val5 = Vec3.CrossProduct(val2, val4.DynamicLocalBottomPos - localCenterOfMass);
				Vec3 val6 = ((Mat3)(ref globalMassFrame.rotation)).TransformToParent(ref val5);
				float baseShipUpSpeed = val6.z + z;
				if (inOutWaterHeightWrtVolume >= ((VolumeDataForSubmergeComputation)(ref val4)).Height || val6.z <= 0f)
				{
					num6 += 1f;
					num7 += 1f;
				}
				else
				{
					num6 += num8;
					num7 += num8;
				}
				float num10 = width * depth;
				float num11 = height * num10 * num;
				float curInvVolumeMass = 1f / num11;
				float num12 = _shipPartsDurabilities[(uint)_floaterVolumesShipPartMap[i]] * floatingForceMultiplier;
				if (num12 < num2)
				{
					num12 = num2;
				}
				Vec3 val7 = ((!(((Mat3)(ref bodyWorldTransform.rotation))[(int)val4.DynamicUpAxis].z < 0f)) ? (val4.DynamicLocalBottomPos + ((Mat3)(ref identity))[(int)val4.DynamicUpAxis] * (num9 * 0.5f)) : (val4.DynamicLocalBottomPos + ((Mat3)(ref identity))[(int)val4.DynamicUpAxis] * (height - num9 * 0.5f)));
				(float, float) tuple = RungeKuttaIntegrationStepForBuoyancyAndGravity(0f, 0f, baseShipUpSpeed, fixedDt, inOutWaterHeightWrtVolume, height, num10, waterDensity, num12, curInvVolumeMass);
				float item = tuple.Item1;
				(float, float) tuple2 = RungeKuttaIntegrationStepForBuoyancyAndGravity(tuple.Item2, item, baseShipUpSpeed, fixedDt * 0.5f, inOutWaterHeightWrtVolume, height, num10, waterDensity, num12, curInvVolumeMass);
				float item2 = tuple2.Item1;
				(float, float) tuple3 = RungeKuttaIntegrationStepForBuoyancyAndGravity(tuple2.Item2, item2, baseShipUpSpeed, fixedDt * 0.5f, inOutWaterHeightWrtVolume, height, num10, waterDensity, num12, curInvVolumeMass);
				float item3 = tuple3.Item1;
				float item4 = RungeKuttaIntegrationStepForBuoyancyAndGravity(tuple3.Item2, item3, baseShipUpSpeed, fixedDt, inOutWaterHeightWrtVolume, height, num10, waterDensity, num12, curInvVolumeMass).Item1;
				float num13 = 1f / 6f * (item + 2f * item2 + 2f * item3 + item4);
				float num14 = num9 * num10;
				float num15 = (num13 + 9.806f) * num11;
				num4 += num14;
				val += val7 * num14;
				Vec3 val13;
				if (inOutWaterHeightWrtVolume < height)
				{
					Vec3 outGlobalWaterSurfaceNormal = _floaterVolumeData[i].OutGlobalWaterSurfaceNormal;
					float num16 = MathF.Clamp(outGlobalWaterSurfaceNormal.x / outGlobalWaterSurfaceNormal.z * width, 0f - height, height);
					float num17 = MathF.Clamp(outGlobalWaterSurfaceNormal.y / outGlobalWaterSurfaceNormal.z * depth, 0f - height, height);
					((Vec2)(ref val8))._002Ector(num16, num17);
					Vec2 val9 = Vec2.Abs(new Vec2(val8.x * width, val8.y * depth));
					Vec2 val10 = val8 * 0.5f;
					Vec2 val11 = Vec2.ElementWiseProduct(waterDensity * 9.806f * val10, val9) * num12;
					ref Mat3 rotation = ref bodyWorldTransform.rotation;
					Vec3 val12 = new Vec3(val11, 0f, -1f);
					val13 = ((Mat3)(ref rotation)).TransformToLocal(ref val12);
				}
				else
				{
					val13 = Vec3.Zero;
				}
				Vec3 val14 = ((Mat3)(ref bodyWorldTransform.rotation)).TransformToLocal(ref _floaterVolumeData[i].OutGlobalWaterSurfaceNormal) * (num15 * 0.1f) + val3 * num15;
				Vec3 val15 = val14 + val13;
				ref Vec3 netGlobalBuoyancyForce = ref _buoyancyComputationResult.NetGlobalBuoyancyForce;
				netGlobalBuoyancyForce += ((Mat3)(ref bodyWorldTransform.rotation)).TransformToParent(ref val15);
				Vec3 torqueMultiplierOfVerticalBuoyantForces = _physicsParameters.TorqueMultiplierOfVerticalBuoyantForces;
				Vec3 val16 = Vec3.CrossProduct(Vec3.ElementWiseProduct(val14, torqueMultiplierOfVerticalBuoyantForces) + val13 * _physicsParameters.TorqueMultiplierOfLateralBuoyantForces, localCenterOfMass - val7);
				Vec3 val17 = ((Mat3)(ref bodyWorldTransform.rotation)).TransformToParent(ref val16);
				ref Vec3 netBuoyancyTorque = ref _buoyancyComputationResult.NetBuoyancyTorque;
				netBuoyancyTorque += val17;
			}
		}
		_buoyancyComputationResult.SubmergedFloaterCountFactor = (float)num3 / (float)_stabilitySubmergedFloaterCount;
		_buoyancyComputationResult.PitchSubmergedAreaFactor = num6 / ((float)_stabilitySubmergedFloaterCount * 0.5f) / (1f + num8);
		_buoyancyComputationResult.RollSubmergedAreaFactor = num7 / ((float)_stabilitySubmergedFloaterCount * 0.5f) / (1f + num8);
		if (num3 > 0)
		{
			float num18 = num5 / (float)_stabilitySubmergedFloaterCount;
			_buoyancyComputationResult.SubmergedHeightFactor = num18 / _stabilityAvgSubmergedHeight;
			if (_buoyancyComputationResult.SubmergedHeightFactor > 2f)
			{
				_buoyancyComputationResult.SubmergedHeightFactor = 2f;
			}
		}
		else
		{
			_buoyancyComputationResult.SubmergedHeightFactor = 0f;
		}
		float num19 = GetAirDensity() / GetWaterDensity();
		if (_buoyancyComputationResult.SubmergedHeightFactor < num19)
		{
			_buoyancyComputationResult.SubmergedHeightFactor = num19;
			_buoyancyComputationResult.SimulatingAirFriction = true;
		}
		if (_buoyancyComputationResult.SubmergedFloaterCountFactor < num19)
		{
			_buoyancyComputationResult.SubmergedFloaterCountFactor = num19;
			_buoyancyComputationResult.SimulatingAirFriction = true;
		}
		if (_buoyancyComputationResult.PitchSubmergedAreaFactor < num19)
		{
			_buoyancyComputationResult.PitchSubmergedAreaFactor = num19;
			_buoyancyComputationResult.SimulatingAirFriction = true;
		}
		if (_buoyancyComputationResult.RollSubmergedAreaFactor < num19)
		{
			_buoyancyComputationResult.RollSubmergedAreaFactor = num19;
			_buoyancyComputationResult.SimulatingAirFriction = true;
		}
		if (_buoyancyComputationResult.RollSubmergedAreaFactor < 0.25f)
		{
			_buoyancyComputationResult.RollSubmergedAreaFactor = 0.25f;
		}
		if (num4 > 0f)
		{
			val /= num4;
			_buoyancyComputationResult.AvgLocalBuoyancyApplyPosition = val;
		}
		else
		{
			_buoyancyComputationResult.AvgLocalBuoyancyApplyPosition = Vec3.Zero;
		}
	}

	private void PreComputeAngularDragTerms(out Vec3 angularDampingTerm, out Vec3 angularDragTerm, out float angularDampingYSideComponentTerm, out float angularDragYSideComponentTerm)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		angularDampingTerm = Vec3.One;
		angularDragTerm = Vec3.One;
		Vec3 localCenterOfMass = LocalCenterOfMass;
		double num = PhysicsBoundingBoxWithoutChildren.max.y - PhysicsBoundingBoxWithoutChildren.min.y;
		double num2 = 0.001;
		double num3 = 0.001;
		double num4 = localCenterOfMass.y;
		double num5 = (double)LinearDragTerm.Up / num;
		double num6 = (double)LinearDampingTerm.Up / num;
		double num7 = (double)LinearDragTerm.Down / num;
		double num8 = (double)LinearDampingTerm.Down / num;
		double num9 = num3 * num5 + num3 * num7;
		double num10 = num3 * num6 + num3 * num8;
		double num11 = 0.0;
		double num12 = 0.0;
		for (double num13 = PhysicsBoundingBoxWithoutChildren.min.y; num13 <= (double)PhysicsBoundingBoxWithoutChildren.max.y; num13 += num3)
		{
			double num14 = Math.Abs(num13 - num4);
			num11 += num14 * num14 * num14;
			num12 += num14 * num14;
		}
		num11 *= num9;
		num12 *= num10;
		angularDampingTerm.x = (float)num12;
		angularDragTerm.x = (float)num11;
		double num15 = localCenterOfMass.x;
		double num16 = (double)(MathF.Abs(PhysicsBoundingBoxWithoutChildren.min.x) + MathF.Abs(PhysicsBoundingBoxWithoutChildren.max.x) + MathF.Abs(PhysicsBoundingBoxWithoutChildren.min.x) + MathF.Abs(PhysicsBoundingBoxWithoutChildren.max.x)) * 0.25;
		double num17 = (double)LinearDragTerm.Up / num16;
		double num18 = (double)LinearDampingTerm.Up / num16;
		double num19 = (double)LinearDragTerm.Down / num16;
		double num20 = (double)LinearDampingTerm.Down / num16;
		double num21 = num2 * num17 + num2 * num19;
		double num22 = num2 * num18 + num2 * num20;
		double num23 = 0.0;
		double num24 = 0.0;
		for (double num25 = 0.0 - num16; num25 <= num16; num25 += num2)
		{
			double num26 = Math.Abs(num25 - num15);
			num23 += num26 * num26 * num26;
			num24 += num26 * num26;
		}
		num23 *= num21;
		num24 *= num22;
		angularDampingTerm.y = (float)num24;
		angularDragTerm.y = (float)num23;
		double num27 = localCenterOfMass.z;
		double num28 = StabilitySubmergedHeightOfShip;
		double num29 = num28 - (double)_minFloaterEntitialBottomPos;
		double num30 = 0.001;
		double num31 = (double)(LinearDragTerm.Left + LinearDragTerm.Right) * 1.0 / num29;
		double num32 = (double)(LinearDampingTerm.Left + LinearDampingTerm.Right) * 1.0 / num29;
		double num33 = num30 * num31;
		double num34 = num30 * num32;
		double num35 = 0.0;
		double num36 = 0.0;
		for (double num37 = _minFloaterEntitialBottomPos; num37 <= num28; num37 += num30)
		{
			double num38 = Math.Abs(num37 - num27);
			num35 += num38 * num38 * num38;
			num36 += num38 * num38;
		}
		num35 *= num33;
		num36 *= num34;
		angularDampingYSideComponentTerm = (float)num36;
		angularDragYSideComponentTerm = (float)num35;
		double num39 = localCenterOfMass.y;
		double num40 = num;
		double num41 = (double)(LinearDragTerm.Left + LinearDragTerm.Right) * 0.5 / num40;
		double num42 = (double)(LinearDampingTerm.Left + LinearDampingTerm.Right) * 0.5 / num40;
		double num43 = num3 * num41;
		double num44 = num3 * num42;
		double num45 = 0.0;
		double num46 = 0.0;
		for (double num47 = PhysicsBoundingBoxWithoutChildren.min.y; num47 <= (double)PhysicsBoundingBoxWithoutChildren.max.y; num47 += num3)
		{
			double num48 = Math.Abs(num47 - num39);
			num45 += num48 * num48 * num48;
			num46 += num48 * num48;
		}
		num45 *= num43;
		num46 *= num44;
		angularDampingTerm.z = (float)num46;
		angularDragTerm.z = (float)num45;
	}

	private void ComputeDragForces(float fixedDt, in Vec3 globalLinearVelocity, in Vec3 globalAngularVelocity, in Vec3 massSpaceLocalInertia)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		_dragComputationResult.Reset();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame entityGlobalFrame = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		MatrixFrame centerOfMassGlobalFrame = GetGlobalMassFrame();
		Vec3 localCenterOfMass = LocalCenterOfMass;
		int substepCount = MathF.Ceiling(fixedDt / (1f / 60f));
		ComputeAngularDrag(fixedDt, substepCount, in globalAngularVelocity, in centerOfMassGlobalFrame, in massSpaceLocalInertia, in _physicsParameters, in _buoyancyComputationResult, in AngularDragTerm, in AngularDampingTerm, _angularDragYSideComponentTerm, _angularDampingYSideComponentTerm, ref _dragComputationResult);
		ComputeDriftFromAngularFriction(fixedDt, in entityGlobalFrame, in centerOfMassGlobalFrame);
		ComputeLinearDrag(fixedDt, substepCount, in globalLinearVelocity, in entityGlobalFrame, Mass, in localCenterOfMass, in _physicsParameters, in _buoyancyComputationResult, LinearDragTerm, LinearDampingTerm, ConstantLinearDampingTerm, _minFloaterEntitialBottomPos, _maxFloaterEntitialTopPos, ref _dragComputationResult, out var _);
	}

	private void ComputeDriftFromAngularFriction(float fixedDt, in MatrixFrame entityGlobalFrame, in MatrixFrame centerOfMassGlobalFrame)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if (!_buoyancyComputationResult.SimulatingAirFriction && _buoyancyComputationResult.SubmergedHeightFactor < 2f && NavalSinkingState == SinkingState.Floating)
		{
			MatrixFrame val = entityGlobalFrame;
			Vec3 val2 = ((MatrixFrame)(ref val)).TransformToParent(ref _buoyancyComputationResult.AvgLocalBuoyancyApplyPosition);
			Vec3 val3 = _dragComputationResult.AngularDragTorqueGlobal * fixedDt;
			val3.z = 0f;
			Vec3 val4 = default(Vec3);
			Vec3 val5 = default(Vec3);
			GameEntityPhysicsExtensions.ComputeVelocityDeltaFromImpulse(((ScriptComponentBehavior)this).GameEntity, ref Vec3.Zero, ref val3, ref val4, ref val5);
			Vec3 val6 = -Vec3.CrossProduct(centerOfMassGlobalFrame.origin - val2, val5);
			float num = 1f;
			if (_buoyancyComputationResult.SubmergedHeightFactor > 1f)
			{
				num = 2f / _buoyancyComputationResult.SubmergedHeightFactor - 1f;
			}
			val6 *= num;
			if (((Vec3)(ref val6)).LengthSquared > 0.010000001f)
			{
				val6 = ((Vec3)(ref val6)).NormalizedCopy() * 0.1f;
			}
			_dragComputationResult.DriftForceFromAngularDragGlobal = Mass * (-val6 / fixedDt);
		}
	}

	protected override void OnTickParallel(float dt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_committedWeightedAgentsPosition = _weightedAgentsPosition;
		_committedTotalMass = _totalMass;
		ClearAgentWeightAndPositionInformation();
	}

	private void ApplyDragForces()
	{
		ApplyGlobalForceAtLocalPos(in _dragComputationResult.CenterOfLateralDragLocal, in _dragComputationResult.LateralDragForceGlobal, (ForceMode)0);
		ApplyGlobalForceAtLocalPos(in _dragComputationResult.CenterOfLongitudinalDragLocal, in _dragComputationResult.LongitudinalDragForceGlobal, (ForceMode)0);
		ApplyGlobalForceAtLocalPos(in _dragComputationResult.CenterOfVerticalDragLocal, in _dragComputationResult.VerticalDragForceGlobal, (ForceMode)0);
		ApplyTorque(in _dragComputationResult.AngularDragTorqueGlobal, (ForceMode)0);
		ApplyForceToDynamicBody(in _dragComputationResult.DriftForceFromAngularDragGlobal, (ForceMode)0);
	}

	private void ApplyAgentForces()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (_committedTotalMass > 0f)
		{
			Vec3 val = _committedWeightedAgentsPosition / _committedTotalMass;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			Vec3 localPos = ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref val);
			BoundingBox physicsBoundingBoxWithoutChildren = PhysicsBoundingBoxWithoutChildren;
			if (((BoundingBox)(ref physicsBoundingBoxWithoutChildren)).PointInsideBox(localPos, 0.1f))
			{
				float stepAgentWeightMultiplier = _physicsParameters.StepAgentWeightMultiplier;
				ApplyGlobalForceAtLocalPos(in localPos, _committedTotalMass * stepAgentWeightMultiplier * MBGlobals.GravitationalAcceleration, (ForceMode)0);
			}
		}
	}

	private void ClearAgentWeightAndPositionInformation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_weightedAgentsPosition = Vec3.Zero;
		_totalMass = 0f;
	}

	private void ApplyBuoyancyForces()
	{
		ApplyForceToDynamicBody(in _buoyancyComputationResult.NetGlobalBuoyancyForce, (ForceMode)0);
		ApplyTorque(in _buoyancyComputationResult.NetBuoyancyTorque, (ForceMode)0);
	}

	public void AddAgentWeightAndPositionInformation(Agent agent)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		float totalMass = agent.GetTotalMass();
		Vec3 position = agent.Position;
		BoundingBox physicsBoundingBoxWithoutChildren = PhysicsBoundingBoxWithoutChildren;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		if (((BoundingBox)(ref physicsBoundingBoxWithoutChildren)).PointInsideBox(((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref position), 0.1f))
		{
			_weightedAgentsPosition += totalMass * position;
			_totalMass += totalMass;
		}
	}

	private void ApplyActuatorForces()
	{
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		if (_shipForceRecord.HasLeftOarForces)
		{
			foreach (ShipForce item in (List<ShipForce>)(object)_shipForceRecord.LeftOarForces)
			{
				ShipForce current = item;
				if (current.IsApplicable)
				{
					ApplyGlobalForceAtLocalPos(in current.LocalPosition, in current.Force, (ForceMode)0);
				}
			}
		}
		if (_shipForceRecord.HasRightOarForces)
		{
			foreach (ShipForce item2 in (List<ShipForce>)(object)_shipForceRecord.RightOarForces)
			{
				ShipForce current2 = item2;
				if (current2.IsApplicable)
				{
					ApplyGlobalForceAtLocalPos(in current2.LocalPosition, in current2.Force, (ForceMode)0);
				}
			}
		}
		if (_shipForceRecord.HasSailForces)
		{
			foreach (ShipForce item3 in (List<ShipForce>)(object)_shipForceRecord.SailForces)
			{
				ShipForce current3 = item3;
				if (current3.IsApplicable)
				{
					current3.ComputeRealisticAndGamifiedForceComponents(out var realisticForce, out var gamifiedForce);
					ApplyGlobalForceAtLocalPos(in current3.LocalPosition, in realisticForce, (ForceMode)0);
					ApplyForceToDynamicBody(in gamifiedForce, (ForceMode)0);
				}
			}
		}
		if (_shipForceRecord.RudderForce.IsApplicable)
		{
			_shipForceRecord.RudderForce.ComputeRealisticAndGamifiedForceComponents(out var realisticForce2, out var gamifiedForce2);
			ApplyGlobalForceAtLocalPos(in _shipForceRecord.RudderForce.LocalPosition, in realisticForce2, (ForceMode)0);
			Vec3 localPos = _shipForceRecord.RudderForce.LocalPosition;
			localPos.z = LocalCenterOfMass.z;
			ApplyGlobalForceAtLocalPos(in localPos, in gamifiedForce2, (ForceMode)0);
		}
	}

	private void ApplyAnchorForces()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		Vec3 globalForceVec = Vec3.Zero;
		Vec3 zero = Vec3.Zero;
		_ = Vec3.Zero;
		if (IsAnchored)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			((Mat3)(ref bodyWorldTransform.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			Vec3 val = _anchorGlobalFrame.origin - bodyWorldTransform.origin;
			globalForceVec = Mass * _anchorForceMultiplier * (1.2f * val - 3.6f * LinearVelocity);
			globalForceVec.z = 0f;
			float num = ((Vec3)(ref globalForceVec)).Normalize();
			float num2 = 2f * Mass * 9.806f;
			globalForceVec = MathF.Min(num, num2) * globalForceVec;
			float y = PhysicsBoundingBoxWithChildrenSize.y;
			float num3 = 0.6f * y;
			if (((Vec3)(ref val)).LengthSquared <= num3 * num3)
			{
				Vec2 asVec = ((Vec3)(ref bodyWorldTransform.rotation.f)).AsVec2;
				Vec2 val2 = ((Vec2)(ref asVec)).Normalized();
				MatrixFrame anchorGlobalFrame = AnchorGlobalFrame;
				asVec = ((Vec3)(ref anchorGlobalFrame.rotation.f)).AsVec2;
				Vec2 val3 = ((Vec2)(ref asVec)).Normalized();
				float num4 = MathF.Atan2(Vec2.Determinant(ref val2, ref val3), Vec2.DotProduct(val2, val3));
				float num5 = (1.4f * num4 - 4.2f * AngularVelocity.z) * _anchorForceMultiplier;
				num5 = (float)MathF.Sign(num5) * MathF.Min(MathF.PI / 9f, MathF.Abs(num5));
				Vec3 val4 = num5 * Vec3.Up;
				Vec3 val5 = ((Mat3)(ref bodyWorldTransform.rotation)).TransformToLocal(ref val4);
				Vec3 val6 = Vec3.ElementWiseProduct(MassSpaceInertia, val5);
				Vec3 torqueVec = ((Mat3)(ref bodyWorldTransform.rotation)).TransformToParent(ref val6);
				ApplyGlobalForceAtLocalPos(LocalCenterOfMass, in globalForceVec, (ForceMode)0);
				ApplyTorque(in torqueVec, (ForceMode)0);
			}
			else
			{
				Vec3 val7 = ((Vec3.DotProduct(((Vec3)(ref val)).NormalizedCopy(), bodyWorldTransform.rotation.f) >= 0f) ? 1f : (-1f)) * (0.1f * y * Vec3.Forward);
				ApplyGlobalForceAtLocalPos(LocalCenterOfMass + val7, in globalForceVec, (ForceMode)0);
			}
		}
	}

	public Oriented2DArea GetGlobalMaximal2DArea()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		BoundingBox physicsBoundingBoxWithChildren = PhysicsBoundingBoxWithChildren;
		Vec2 asVec = ((Vec3)(ref physicsBoundingBoxWithChildren.min)).AsVec2;
		physicsBoundingBoxWithChildren = PhysicsBoundingBoxWithChildren;
		Vec2 asVec2 = ((Vec3)(ref physicsBoundingBoxWithChildren.max)).AsVec2;
		Vec2 val = (asVec2 + asVec) / 2f;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec2 asVec3 = ((Vec3)(ref bodyWorldTransform.rotation.f)).AsVec2;
		Vec2 val2 = ((Vec2)(ref asVec3)).Normalized();
		Vec2 val3 = -((Vec2)(ref val2)).LeftVec();
		Vec2 val4 = ((Vec3)(ref bodyWorldTransform.origin)).AsVec2 + ((Vec2)(ref val)).X * val3 + ((Vec2)(ref val)).Y * val2;
		Vec2 val5 = asVec2 - asVec;
		return new Oriented2DArea(ref val4, ref val2, ref val5);
	}

	public int GetPartIndexAtPosition(Vec3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		BoundingBox physicsBoundingBoxWithoutChildren = PhysicsBoundingBoxWithoutChildren;
		Vec2 asVec = ((Vec3)(ref physicsBoundingBoxWithoutChildren.min)).AsVec2;
		physicsBoundingBoxWithoutChildren = PhysicsBoundingBoxWithoutChildren;
		Vec2 asVec2 = ((Vec3)(ref physicsBoundingBoxWithoutChildren.max)).AsVec2;
		float num = ((Vec2)(ref asVec2)).Y - ((Vec2)(ref asVec)).Y;
		float num2 = ((Vec2)(ref asVec2)).X - ((Vec2)(ref asVec)).X;
		float num3 = num / 3f;
		float num4 = num2 / 2f;
		float num5 = position.y + num * 0.5f - (asVec2.y + asVec.y) * 0.5f;
		float num6 = position.x + num2 * 0.5f - (asVec2.x + asVec.x) * 0.5f;
		int num7 = MathF.Floor(num5 / num3);
		int num8 = MathF.Floor(num6 / num4);
		num7 = MBMath.ClampIndex(num7, 0, 3);
		num8 = MBMath.ClampIndex(num8, 0, 2);
		return num7 * 2 + num8;
	}

	private void LoadFloaterVolumes()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		PhysicsBoundingBoxWithChildren = GameEntityPhysicsExtensions.GetLocalPhysicsBoundingBox(((ScriptComponentBehavior)this).GameEntity, true);
		PhysicsBoundingBoxWithChildrenSize = PhysicsBoundingBoxWithChildren.max - PhysicsBoundingBoxWithChildren.min;
		PhysicsBoundingBoxWithoutChildren = GameEntityPhysicsExtensions.GetLocalPhysicsBoundingBox(((ScriptComponentBehavior)this).GameEntity, false);
		PhysicsBoundingBoxSizeWithoutChildren = PhysicsBoundingBoxWithoutChildren.max - PhysicsBoundingBoxWithoutChildren.min;
		_totalFloaterVolumeCached = 0f;
		WeakGameEntity val = WeakGameEntity.Invalid;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child2 in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child2;
			if (((WeakGameEntity)(ref current)).Name == "floater_volume_holder")
			{
				val = current;
				break;
			}
		}
		if (!(val == WeakGameEntity.Invalid))
		{
			int num = ((WeakGameEntity)(ref val)).GetChildren().Count();
			_floaterVolumesShipPartMap = new ShipPart[num];
			_floaterVolumeData = (VolumeDataForSubmergeComputation[])(object)new VolumeDataForSubmergeComputation[num];
			_floaterVolumeDataPinnedGCHandler = GCHandle.Alloc(_floaterVolumeData, GCHandleType.Pinned);
			_floaterVolumeDataPinnedPointer = (UIntPtr)(ulong)(long)_floaterVolumeDataPinnedGCHandler.AddrOfPinnedObject();
			float num2 = float.MaxValue;
			float num3 = float.MinValue;
			for (int i = 0; i < num; i++)
			{
				WeakGameEntity child = ((WeakGameEntity)(ref val)).GetChild(i);
				MatrixFrame localFrame = ((WeakGameEntity)(ref child)).GetLocalFrame();
				_floaterVolumeData[i].DynamicUpAxis = (FloaterVolumeDynamicUpAxis)2;
				_floaterVolumeData[i].DynamicLocalBottomPos = localFrame.origin;
				_floaterVolumeData[i].LocalFrame = localFrame;
				_floaterVolumeData[i].LocalScale = ((MatrixFrame)(ref localFrame)).GetScale();
				_floaterVolumeData[i].OutGlobalWaterSurfaceNormal = Vec3.Up;
				_floaterVolumeData[i].InOutWaterHeightWrtVolume = ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Height * 0.5f;
				_floaterVolumesShipPartMap[i] = (ShipPart)GetPartIndexAtPosition(_floaterVolumeData[i].DynamicLocalBottomPos);
				_totalFloaterVolumeCached += ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Width * ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Depth * ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Height;
				num2 = Math.Min(num2, _floaterVolumeData[i].DynamicLocalBottomPos.z);
				num3 = Math.Max(num3, _floaterVolumeData[i].DynamicLocalBottomPos.z + ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Height);
			}
			float waterDensity = GetWaterDensity();
			float num4 = Mass * 9.806f;
			float num5 = _totalFloaterVolumeCached * waterDensity * 9.806f;
			_minimumFloaterDurabilityToFloatWhileNotSinking = num4 * 1.1f / num5;
			_shipPartsDurabilities = Enumerable.Repeat(1f, 6).ToArray();
			_shipPartsTargetDurabilities = Enumerable.Repeat(1f, 6).ToArray();
			ComputeAndCacheStabilityAvgSubmergedHeight(num2, num3);
		}
	}

	private void UpdateFloaterVolumeData()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected I4, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected I4, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Invalid comparison between Unknown and I4
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected I4, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Mat3 rotation = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().rotation;
		for (int i = 0; i < _floaterVolumeData.Length; i++)
		{
			Vec3 localScale = _floaterVolumeData[i].LocalScale;
			int num = (int)_floaterVolumeData[i].DynamicUpAxis;
			float num2 = ((Vec3)(ref localScale))[num] * MathF.Abs(((Mat3)(ref rotation))[num].z);
			for (int j = 1; j < 3; j++)
			{
				int num3 = (_floaterVolumeData[i].DynamicUpAxis + j) % 3;
				float num4 = ((Vec3)(ref localScale))[num3] * MathF.Abs(((Mat3)(ref rotation))[num3].z);
				if (num4 > num2 * 1.1f)
				{
					num2 = num4;
					num = num3;
				}
			}
			if ((int)_floaterVolumeData[i].DynamicUpAxis != (byte)num)
			{
				float num5 = _floaterVolumeData[i].InOutWaterHeightWrtVolume / ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Height;
				_floaterVolumeData[i].DynamicUpAxis = (FloaterVolumeDynamicUpAxis)(byte)num;
				FloaterVolumeDynamicUpAxis dynamicUpAxis = _floaterVolumeData[i].DynamicUpAxis;
				switch ((int)dynamicUpAxis)
				{
				case 0:
					_floaterVolumeData[i].DynamicLocalBottomPos = _floaterVolumeData[i].LocalFrame.origin + new Vec3((0f - localScale.x) * 0.5f, 0f, localScale.z * 0.5f, -1f);
					break;
				case 1:
					_floaterVolumeData[i].DynamicLocalBottomPos = _floaterVolumeData[i].LocalFrame.origin + new Vec3(0f, (0f - localScale.y) * 0.5f, localScale.z * 0.5f, -1f);
					break;
				case 2:
					_floaterVolumeData[i].DynamicLocalBottomPos = _floaterVolumeData[i].LocalFrame.origin;
					break;
				}
				_floaterVolumeData[i].InOutWaterHeightWrtVolume = ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Height * num5;
			}
		}
	}

	private void ComputeAndCacheStabilityAvgSubmergedHeight(float minimumEntitialFloaterZ, float maximumEntitialFloaterZ)
	{
		float waterDensity = GetWaterDensity();
		float num = Mass * 9.806f;
		float num2 = minimumEntitialFloaterZ + 0.01f;
		float floatingForceMultiplier = _physicsParameters.FloatingForceMultiplier;
		_stabilityAvgSubmergedHeight = maximumEntitialFloaterZ - minimumEntitialFloaterZ;
		_stabilitySubmergedFloaterCount = _floaterVolumeData.Length;
		_minFloaterEntitialBottomPos = minimumEntitialFloaterZ;
		_maxFloaterEntitialTopPos = maximumEntitialFloaterZ;
		for (; maximumEntitialFloaterZ > num2; num2 += 0.01f)
		{
			float num3 = 0f;
			int num4 = 0;
			float num5 = 0f;
			for (int i = 0; i < _floaterVolumeData.Length; i++)
			{
				float num6 = num2 - _floaterVolumeData[i].DynamicLocalBottomPos.z;
				if (num6 > 0f)
				{
					float num7 = Math.Min(num6, ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Height);
					float num8 = num7 * ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Width * ((VolumeDataForSubmergeComputation)(ref _floaterVolumeData[i])).Depth * waterDensity * 9.806f * floatingForceMultiplier;
					num3 += num7;
					num4++;
					num5 += num8;
				}
			}
			if (num5 >= num)
			{
				StabilitySubmergedHeightOfShip = num2;
				_stabilityAvgSubmergedHeight = num3 / (float)num4;
				_stabilitySubmergedFloaterCount = num4;
				break;
			}
		}
	}

	private void UpdateShipPhysics(NavalPhysicsParameters physicsParameters, ShipPhysicsReference basePhysicsRef)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		_physicsParameters = physicsParameters;
		float overrideMass = _physicsParameters.OverrideMass;
		WeakGameEntity gameEntity;
		float num;
		if (overrideMass > 0f)
		{
			num = overrideMass;
		}
		else
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			num = ((WeakGameEntity)(ref gameEntity)).Mass;
		}
		num *= _physicsParameters.MassMultiplier;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec3 centerOfMass = ((WeakGameEntity)(ref gameEntity)).CenterOfMass;
		GameEntityPhysicsExtensions.SetMassAndUpdateInertiaAndCenterOfMass(((ScriptComponentBehavior)this).GameEntity, num);
		GameEntityPhysicsExtensions.SetCenterOfMass(((ScriptComponentBehavior)this).GameEntity, centerOfMass);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_cachedMass = ((WeakGameEntity)(ref gameEntity)).Mass;
		Vec3 val = Vec3.ElementWiseProduct(GameEntityPhysicsExtensions.GetMassSpaceInertia(((ScriptComponentBehavior)this).GameEntity), _physicsParameters.MomentOfInertiaMultiplier);
		GameEntityPhysicsExtensions.SetMassSpaceInertia(((ScriptComponentBehavior)this).GameEntity, val);
		LinearDragTerm = basePhysicsRef.LinearDragTerm * _cachedMass;
		LinearDampingTerm = basePhysicsRef.LinearDampingTerm * _cachedMass;
		ConstantLinearDampingTerm = basePhysicsRef.ConstantLinearDampingTerm * _cachedMass;
		GameEntityPhysicsExtensions.SetLinearVelocity(((ScriptComponentBehavior)this).GameEntity, Vec3.Zero);
		GameEntityPhysicsExtensions.SetAngularVelocity(((ScriptComponentBehavior)this).GameEntity, Vec3.Zero);
		GameEntityPhysicsExtensions.DisableGravity(((ScriptComponentBehavior)this).GameEntity);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		PhysicsMaterial physicsMaterial = ((WeakGameEntity)(ref gameEntity)).GetPhysicsMaterial();
		GameEntityPhysicsExtensions.SetDamping(((ScriptComponentBehavior)this).GameEntity, ((PhysicsMaterial)(ref physicsMaterial)).GetLinearDamping(), ((PhysicsMaterial)(ref physicsMaterial)).GetAngularDamping());
	}

	private void ComputeContinuousDriftForce(float fixedDt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		_continuousDriftForceData.ResultForce = Vec3.Zero;
		if (_continuousDriftForceData.DriftSpeed > 0f && IsInitialized && !_buoyancyComputationResult.SimulatingAirFriction && NavalSinkingState == SinkingState.Floating)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Vec2 val = ((WeakGameEntity)(ref gameEntity)).GetGlobalWindVelocityOfScene();
			Vec2 val2 = ((Vec2)(ref val)).Normalized();
			val = val2 * _continuousDriftForceData.DriftSpeed;
			Vec2 val3 = val;
			Vec3 linearVelocity = LinearVelocity;
			Vec2 val4 = val3 - ((Vec3)(ref linearVelocity)).AsVec2;
			float num = ((Vec2)(ref val2)).DotProduct(val4);
			if (num > 0f)
			{
				Vec2 val5 = val2 * num;
				float num2 = MathF.Clamp(LastSubmergedHeightFactorForActuators, 0f, 1f);
				float num3 = MathF.Sin(_continuousDriftForceData.DriftForceTimer * MathF.PI * 0.1f);
				_continuousDriftForceData.DriftForceTimer += fixedDt * num2 * _continuousDriftForceData.DriftRandom.NextFloat();
				num2 *= num3 * 0.4f + 0.8f;
				float num4 = num3;
				Vec2 val6 = val5;
				((Vec2)(ref val6)).RotateCCW(num4 * 0.08726646f);
				_continuousDriftForceData.ResultForce = ((Vec2)(ref val6)).ToVec3(0f) * num2 * Mass;
			}
		}
	}

	private void ApplyContinuousDriftForce()
	{
		if (((Vec3)(ref _continuousDriftForceData.ResultForce)).LengthSquared > 0f)
		{
			ApplyForceToDynamicBody(in _continuousDriftForceData.ResultForce, (ForceMode)0);
		}
	}

	private static float ComputeLateralDragShift(in Vec3 localVelocity, float maxLateralDragShift, float lateralDragShiftCriticalAngle, float maxLateralShiftSpeed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = localVelocity;
		float num = MathF.Acos(MathF.Max(((Vec3)(ref val)).NormalizedCopy().y, 0f));
		float num2 = 2.5f * num / lateralDragShiftCriticalAngle;
		float num3 = 1f - (float)Math.Exp(0f - num2 * num2);
		return MathF.Clamp(localVelocity.y / maxLateralShiftSpeed, 0f, 1f) * num3 * maxLateralDragShift;
	}

	public void SetSinkingState(SinkingState state)
	{
		NavalSinkingState = state;
	}

	public void ForceSink()
	{
		if (_shipPartsTargetDurabilities != null)
		{
			for (int i = 0; i < _shipPartsTargetDurabilities.Length; i++)
			{
				_shipPartsTargetDurabilities[i] = 0f;
			}
		}
		NavalSinkingState = SinkingState.Sinking;
	}

	private static Vec3 SubStepIntegrationStepForLinearFriction(Vec3 absLinearVelocityLocal, float subStepFixedDt, float mass, Vec3 submergedLinearDragTerm, Vec3 submergedLinearDampingTerm, Vec3 submergedConstantLinearDampingTerm, Vec3 submergedFactorLinear)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.ElementWiseProduct(ComputeVelocityFactorForClampingDrag(absLinearVelocityLocal), submergedFactorLinear);
		Vec3 val2 = Vec3.ElementWiseProduct(absLinearVelocityLocal, absLinearVelocityLocal);
		return (Vec3.ElementWiseProduct(submergedLinearDragTerm, val2) + Vec3.ElementWiseProduct(submergedLinearDampingTerm, absLinearVelocityLocal) + submergedConstantLinearDampingTerm + Vec3.ElementWiseProduct(submergedLinearDragTerm, val)) / mass * subStepFixedDt;
	}

	private static Vec3 SubStepIntegrationStepForAngularFriction(Vec3 absMassLocalAngularVelocity, float subStepFixedDt, Vec3 massLocalInertia, Vec3 angularDragTerm, Vec3 angularDampingTerm, float angularDragYSideComponentTerm, float angularDampingYSideComponentTerm, in BuoyancyComputationResult buoyancyComputationResult)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		Vec3 zero = Vec3.Zero;
		zero.x = angularDragTerm.x * absMassLocalAngularVelocity.x * absMassLocalAngularVelocity.x;
		zero.x += angularDampingTerm.x * absMassLocalAngularVelocity.x;
		zero.x *= buoyancyComputationResult.PitchSubmergedAreaFactor;
		float num = angularDragTerm.y * absMassLocalAngularVelocity.y * absMassLocalAngularVelocity.y;
		num += angularDampingTerm.y * absMassLocalAngularVelocity.y;
		num *= buoyancyComputationResult.RollSubmergedAreaFactor;
		zero.y = num;
		float num2 = angularDragYSideComponentTerm * absMassLocalAngularVelocity.y * absMassLocalAngularVelocity.y;
		num2 += angularDampingYSideComponentTerm * absMassLocalAngularVelocity.y;
		num2 *= buoyancyComputationResult.SubmergedHeightFactor;
		zero.y += num2;
		zero.z = angularDragTerm.z * absMassLocalAngularVelocity.z * absMassLocalAngularVelocity.z;
		zero.z += angularDampingTerm.z * absMassLocalAngularVelocity.z;
		zero.z *= buoyancyComputationResult.SubmergedHeightFactor;
		return Vec3.ElementWiseDivision(zero, massLocalInertia) * subStepFixedDt;
	}

	public static void ComputeLinearDrag(float fixedDt, int substepCount, in Vec3 globalLinearVelocity, in MatrixFrame globalFrame, in float mass, in Vec3 localCenterOfMass, in NavalPhysicsParameters physicsParameters, in BuoyancyComputationResult buoyancyComputationResult, in LinearFrictionTerm linearDragTerm, in LinearFrictionTerm linearDampingTerm, in LinearFrictionTerm constantLinearDampingTerm, float minFloaterEntitialBottomPos, float maxFloaterEntitialTopPos, ref DragForceComputationResult dragComputationResult, out float lateralDragForwardShift)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = globalLinearVelocity;
		Mat3 rotation = globalFrame.rotation;
		Vec3 localVelocity = ((Mat3)(ref rotation)).TransformToLocal(ref val);
		Vec3 val2 = default(Vec3);
		((Vec3)(ref val2))._002Ector(buoyancyComputationResult.SubmergedHeightFactor, buoyancyComputationResult.SubmergedHeightFactor, buoyancyComputationResult.SubmergedFloaterCountFactor, -1f);
		LinearFrictionTerm val3 = linearDragTerm;
		LinearFrictionTerm val4 = ((LinearFrictionTerm)(ref val3)).ElementWiseProduct(physicsParameters.LinearFrictionMultiplier);
		val3 = linearDampingTerm;
		LinearFrictionTerm val5 = ((LinearFrictionTerm)(ref val3)).ElementWiseProduct(physicsParameters.LinearFrictionMultiplier);
		val3 = constantLinearDampingTerm;
		LinearFrictionTerm val6 = ((LinearFrictionTerm)(ref val3)).ElementWiseProduct(physicsParameters.LinearFrictionMultiplier);
		Vec3 val7 = default(Vec3);
		((Vec3)(ref val7))._002Ector((localVelocity.x >= 0f) ? val4.Right : val4.Left, (localVelocity.y >= 0f) ? val4.Forward : val4.Backward, (localVelocity.z >= 0f) ? val4.Up : val4.Down, -1f);
		Vec3 val8 = default(Vec3);
		((Vec3)(ref val8))._002Ector((localVelocity.x >= 0f) ? val5.Right : val5.Left, (localVelocity.y >= 0f) ? val5.Forward : val5.Backward, (localVelocity.z >= 0f) ? val5.Up : val5.Down, -1f);
		Vec3 val9 = new Vec3((localVelocity.x >= 0f) ? val6.Right : val6.Left, (localVelocity.y >= 0f) ? val6.Forward : val6.Backward, (localVelocity.z >= 0f) ? val6.Up : val6.Down, -1f);
		Vec3 submergedLinearDragTerm = Vec3.ElementWiseProduct(val7, val2);
		Vec3 submergedLinearDampingTerm = Vec3.ElementWiseProduct(val8, val2);
		Vec3 submergedConstantLinearDampingTerm = Vec3.ElementWiseProduct(val9, val2);
		Vec3 val10 = Vec3.Abs(localVelocity);
		Vec3 one = Vec3.One;
		one.y *= physicsParameters.ForwardDragMultiplier;
		one *= GetWaterDensity();
		if (globalFrame.rotation.u.z < -0.4f)
		{
			one *= physicsParameters.UpSideDownFrictionMultiplier;
		}
		float subStepFixedDt = fixedDt / (float)substepCount;
		Vec3 val11 = val10;
		for (int i = 0; i < substepCount; i++)
		{
			Vec3 val12 = SubStepIntegrationStepForLinearFriction(val11, subStepFixedDt, mass, submergedLinearDragTerm, submergedLinearDampingTerm, submergedConstantLinearDampingTerm, val2);
			val12 = Vec3.ElementWiseProduct(val12, one);
			val11 -= val12;
			if (val11.x < 0f)
			{
				val11.x = 0f;
			}
			if (val11.y < 0f)
			{
				val11.y = 0f;
			}
			if (val11.z < 0f)
			{
				val11.z = 0f;
			}
		}
		Vec3 val13 = (val10 - val11) * (mass / fixedDt);
		Vec3 val14 = mass * val10;
		Vec3 val15 = 1f / fixedDt * val14;
		Vec3 val16 = new Vec3((float)(-MathF.Sign(localVelocity.x)) * MathF.Min(val15.x, val13.x), (float)(-MathF.Sign(localVelocity.y)) * MathF.Min(val15.y, val13.y), (float)(-MathF.Sign(localVelocity.z)) * MathF.Min(val15.z, val13.z), -1f);
		Vec3 lateralDragForceGlobal = val16.x * globalFrame.rotation.s;
		Vec3 longitudinalDragForceGlobal = val16.y * globalFrame.rotation.f;
		Vec3 verticalDragForceGlobal = val16.z * globalFrame.rotation.u;
		float maxLateralShiftSpeed = physicsParameters.MaxLinearSpeedForLateralDragCenterShift * 0.2f;
		lateralDragForwardShift = ComputeLateralDragShift(in localVelocity, physicsParameters.MaxLateralDragShift, physicsParameters.LateralDragShiftCriticalAngle, maxLateralShiftSpeed);
		dragComputationResult.LateralDragForceGlobal = lateralDragForceGlobal;
		dragComputationResult.LongitudinalDragForceGlobal = longitudinalDragForceGlobal;
		dragComputationResult.VerticalDragForceGlobal = verticalDragForceGlobal;
		if (buoyancyComputationResult.SimulatingAirFriction)
		{
			dragComputationResult.CenterOfLateralDragLocal = localCenterOfMass;
			dragComputationResult.CenterOfLongitudinalDragLocal = localCenterOfMass;
			dragComputationResult.CenterOfVerticalDragLocal = localCenterOfMass;
			return;
		}
		dragComputationResult.CenterOfLateralDragLocal.x = buoyancyComputationResult.AvgLocalBuoyancyApplyPosition.x;
		dragComputationResult.CenterOfLateralDragLocal.y = localCenterOfMass.y - Vec3.Forward.y * lateralDragForwardShift;
		dragComputationResult.CenterOfLateralDragLocal.z = buoyancyComputationResult.AvgLocalBuoyancyApplyPosition.z;
		dragComputationResult.CenterOfLongitudinalDragLocal = localCenterOfMass;
		dragComputationResult.CenterOfVerticalDragLocal.x = buoyancyComputationResult.AvgLocalBuoyancyApplyPosition.x;
		dragComputationResult.CenterOfVerticalDragLocal.y = buoyancyComputationResult.AvgLocalBuoyancyApplyPosition.y;
		dragComputationResult.CenterOfVerticalDragLocal.z = ((globalFrame.rotation.u.z >= 0f) ? minFloaterEntitialBottomPos : maxFloaterEntitialTopPos);
	}

	public static void ComputeAngularDrag(float fixedDt, int substepCount, in Vec3 globalAngularVelocity, in MatrixFrame centerOfMassGlobalFrame, in Vec3 massSpaceLocalInertia, in NavalPhysicsParameters physicsParameters, in BuoyancyComputationResult buoyancyComputationResult, in Vec3 angularDragTerm, in Vec3 angularDampingTerm, float angularDragYSideComponentTerm, float angularDampingYSideComponentTerm, ref DragForceComputationResult dragComputationResult)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = globalAngularVelocity;
		Mat3 rotation = centerOfMassGlobalFrame.rotation;
		Vec3 val2 = ((Mat3)(ref rotation)).TransformToLocal(ref val);
		Vec3 val3 = Vec3.Abs(val2);
		Vec3 val4 = Vec3.ElementWiseProduct(massSpaceLocalInertia, val3);
		Vec3 val5 = 1f / fixedDt * val4;
		Vec3 val6 = physicsParameters.AngularFrictionMultiplier * GetWaterDensity();
		if (centerOfMassGlobalFrame.rotation.u.z < -0.4f)
		{
			val6 *= physicsParameters.UpSideDownFrictionMultiplier;
		}
		float subStepFixedDt = fixedDt / (float)substepCount;
		Vec3 val7 = val3;
		for (int i = 0; i < substepCount; i++)
		{
			Vec3 val8 = SubStepIntegrationStepForAngularFriction(val7, subStepFixedDt, massSpaceLocalInertia, angularDragTerm, angularDampingTerm, angularDragYSideComponentTerm, angularDampingYSideComponentTerm, in buoyancyComputationResult);
			val8 = Vec3.ElementWiseProduct(val8, val6);
			val7 -= val8;
			if (val7.x < 0f)
			{
				val7.x = 0f;
			}
			if (val7.y < 0f)
			{
				val7.y = 0f;
			}
			if (val7.z < 0f)
			{
				val7.z = 0f;
			}
		}
		Vec3 val9 = Vec3.ElementWiseProduct(val3 - val7, massSpaceLocalInertia) / fixedDt;
		Vec3 val10 = default(Vec3);
		((Vec3)(ref val10))._002Ector((float)(-MathF.Sign(val2.x)) * MathF.Min(val5.x, val9.x), (float)(-MathF.Sign(val2.y)) * MathF.Min(val5.y, val9.y), (float)(-MathF.Sign(val2.z)) * MathF.Min(val5.z, val9.z), -1f);
		rotation = centerOfMassGlobalFrame.rotation;
		Vec3 angularDragTorqueGlobal = ((Mat3)(ref rotation)).TransformToParent(ref val10);
		dragComputationResult.AngularDragTorqueGlobal = angularDragTorqueGlobal;
	}

	private static Vec3 ComputeVelocityFactorForClampingDrag(Vec3 absLinearVelocityLocal)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(7f, 20f, 20f, -1f);
		Vec3 zero = Vec3.Zero;
		for (int i = 0; i < 3; i++)
		{
			float num = ((Vec3)(ref absLinearVelocityLocal))[i] - ((Vec3)(ref val))[i];
			if (num > 0f)
			{
				((Vec3)(ref zero))[i] = MathF.Pow(num, 4f);
			}
		}
		return zero;
	}
}
