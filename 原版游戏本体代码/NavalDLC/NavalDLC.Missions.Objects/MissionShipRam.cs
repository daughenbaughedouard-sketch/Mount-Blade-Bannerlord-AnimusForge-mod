using System;
using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects;

public class MissionShipRam : MissionObject
{
	private struct RamCollisionData
	{
		public MissionShip TargetShip;

		public CapsuleData CapsuleData;

		public bool RamWillBeHandled;

		public Vec3 SelectedIntersectionPoint;

		public Vec3 AverageIntersectionPoint;

		public Vec3 RamDirection;

		public float PenetrationLength;

		public bool HasPoint;

		public float CalculatedDamage;

		public Vec3 PointVelocityOnOwner;

		public Vec3 PointVelocityOnTarget;

		public bool IsValid => TargetShip != null;
	}

	private const float SpeedFactorOnMagnitude = 0.03f;

	private const string ShipDebrisAndParticlePrefabName = "decal_ship_damaged_b_heap";

	private const string ShipBodyPhysicsEntityTag = "body_mesh";

	private const float RamHitDirectionThresholdPercentage = 0.3f;

	private const float RamStickThresholdPercentage = 0.33f;

	private const string PhysicsMaterialName = "wood_ship";

	private static readonly int RamCollisionSoundEffectSoundId = SoundManager.GetEventGlobalIndex("event:/physics/vessel/ship_ramming");

	private const BodyFlags RamRaycastExcludeFlags = (BodyFlags)2147497729u;

	private static (float, float, float, float, bool)[] _ramQualityThresholds = new(float, float, float, float, bool)[5]
	{
		(10f, 70f, 0.2f, 5f, true),
		(8f, 60f, 0.3f, 4f, true),
		(6f, 45f, 0.45f, 2.5f, false),
		(5f, 30f, 0.65f, 1.5f, false),
		(3f, 0f, 0.9f, 0.5f, false)
	};

	private Intersection[] _intersectionsCache = (Intersection[])(object)new Intersection[128];

	private WeakGameEntity[] _entitiesCache = (WeakGameEntity[])(object)new WeakGameEntity[128];

	private UIntPtr[] _entityPointersCache = new UIntPtr[128];

	private Intersection[] _selectedIntersectionsCache = (Intersection[])(object)new Intersection[128];

	private MissionShip _ownerShip;

	private MissionShip _ramStuckTargetShip;

	private bool _ramCollisionBeingHandled;

	private RamCollisionData _ramDamageData;

	private RamCollisionData _ramCollisionData;

	private Scene _ownScene;

	private int _lastRamHitQuality;

	[EditableScriptComponentVariable(true, "")]
	private float _ramLength = 5f;

	[EditableScriptComponentVariable(true, "")]
	private float _ramRadius = 0.5f;

	[EditableScriptComponentVariable(true, "")]
	private Vec3 _ramAttachmentPointOffset = Vec3.Zero;

	[EditableScriptComponentVariable(true, "")]
	private float _ramTierDamageMultiplier = 1f;

	private float _scaledRamRadius = -1f;

	private float _scaledRamLength = -1f;

	private static float ForwardSpeedThresholdToDamage => _ramQualityThresholds[_ramQualityThresholds.Length - 1].Item1;

	private static float DistanceToShipCenterThresholdToDamage => _ramQualityThresholds[_ramQualityThresholds.Length - 1].Item3;

	public float RamLength => _ramLength;

	private CapsuleData GetRamCapsuleData(float fixedDt, bool getDataForNextFrame)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame val = ((WeakGameEntity)(ref gameEntity)).ComputePreciseGlobalFrameForFixedTickSlow();
		Vec3 f = val.rotation.f;
		Vec3 val2 = ((MatrixFrame)(ref val)).TransformToParent(ref _ramAttachmentPointOffset);
		Vec3 val3 = val2 + f * _ramLength;
		float num = _ramRadius * Math.Max(((Vec3)(ref val.rotation.u)).Length, ((Vec3)(ref val.rotation.s)).Length);
		if (getDataForNextFrame)
		{
			val2 += GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)_ownerShip).GameEntity, val2) * fixedDt;
			val3 += GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)_ownerShip).GameEntity, val3) * fixedDt;
		}
		return new CapsuleData(num, val2, val3);
	}

	protected override void OnTick(float dt)
	{
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		if (!_ramDamageData.IsValid)
		{
			return;
		}
		float calculatedDamage = _ramDamageData.CalculatedDamage;
		if (calculatedDamage > 0f)
		{
			_ramDamageData.TargetShip.DealCollisionDamage(_ownerShip, isRamDamage: true, _ramDamageData.SelectedIntersectionPoint, calculatedDamage);
			_ownerShip.UpdateDamageCooldown(_ramDamageData.TargetShip);
			Vec3 averageIntersectionPoint = _ramDamageData.AverageIntersectionPoint;
			foreach (DestructableComponent item in (List<DestructableComponent>)(object)_ramDamageData.TargetShip.AllDestructableComponents)
			{
				if (!item.IsDestroyed)
				{
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
					Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
					if (((Vec3)(ref globalPosition)).DistanceSquared(averageIntersectionPoint) < 25f)
					{
						item.DestroyOnAnyHit = true;
						item.TriggerOnHit((Agent)null, 1, averageIntersectionPoint, _ramDamageData.RamDirection, ref MissionWeapon.Invalid, -1, (ScriptComponentBehavior)(object)this);
					}
				}
			}
			Agent val = _ownerShip.Captain;
			if (val == null || !val.IsMainAgent)
			{
				Agent val2 = null;
				if (((UsableMachine)_ownerShip.ShipControllerMachine).PilotAgent != null && (val2 == null || ((UsableMachine)_ownerShip.ShipControllerMachine).PilotAgent.IsMainAgent))
				{
					val2 = ((UsableMachine)_ownerShip.ShipControllerMachine).PilotAgent;
				}
				if (val == null || (val2 != null && val2.IsMainAgent))
				{
					val = val2;
				}
			}
			Blow val4 = default(Blow);
			for (int num = ((List<Agent>)(object)Mission.Current.Agents).Count - 1; num >= 0; num--)
			{
				Agent val3 = ((List<Agent>)(object)Mission.Current.Agents)[num];
				Vec3 position = val3.Position;
				if (val3.IsActive())
				{
					Vec2 asVec = ((Vec3)(ref position)).AsVec2;
					if (((Vec2)(ref asVec)).DistanceSquared(((Vec3)(ref averageIntersectionPoint)).AsVec2) < 4f)
					{
						((Blow)(ref val4))._002Ector((val != null) ? val.Index : val3.Index);
						val4.DamageType = (DamageTypes)2;
						val4.BaseMagnitude = 200f;
						val4.InflictedDamage = 200;
						val4.GlobalPosition = position;
						val4.DamagedPercentage = 1f;
						val3.Die(val4, (KillInfo)(-1));
					}
				}
			}
		}
		TriggerRamCollisionParticleAndSoundEffect(_ramDamageData.TargetShip.Index, ((ScriptComponentBehavior)_ramDamageData.TargetShip).GameEntity, _ramDamageData.CapsuleData, calculatedDamage);
		_ramDamageData = default(RamCollisionData);
	}

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		((MissionObject)this).OnInit();
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		_ownerShip = ((WeakGameEntity)(ref val)).GetFirstScriptOfTypeInFamily<MissionShip>();
		CapsuleData ramCapsuleData = GetRamCapsuleData(0f, getDataForNextFrame: false);
		_scaledRamRadius = ((CapsuleData)(ref ramCapsuleData)).Radius;
		Vec3 val2 = ((CapsuleData)(ref ramCapsuleData)).P2 - ((CapsuleData)(ref ramCapsuleData)).P1;
		_scaledRamLength = ((Vec3)(ref val2)).Length + _scaledRamRadius;
		val = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		val2 = ((CapsuleData)(ref ramCapsuleData)).P1;
		Vec3 val3 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref val2);
		Vec3 p = ((CapsuleData)(ref ramCapsuleData)).P2;
		GameEntityPhysicsExtensions.PushCapsuleShapeToEntityBody(gameEntity, val3, ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref p), ((CapsuleData)(ref ramCapsuleData)).Radius, "wood_ship");
		val = ((ScriptComponentBehavior)this).GameEntity;
		_ownScene = ((WeakGameEntity)(ref val)).Scene;
	}

	protected override void OnFixedTick(float fixedDt)
	{
		RamCollisionHandleFixedTick(fixedDt);
	}

	protected override void OnParallelFixedTick(float fixedDt)
	{
		RamCollisionCheckTick(fixedDt);
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)50;
	}

	private void TriggerRamCollisionParticleAndSoundEffect(int targetShipIndex, WeakGameEntity targetEntity, CapsuleData shipRamCapsule, float damage)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		List<WeakGameEntity> list = ((WeakGameEntity)(ref targetEntity)).CollectChildrenEntitiesWithTag("body_mesh");
		if (list.Count == 0)
		{
			return;
		}
		WeakGameEntity val = list[0];
		Vec3 val2 = ((CapsuleData)(ref shipRamCapsule)).P2 - ((CapsuleData)(ref shipRamCapsule)).P1;
		((Vec3)(ref val2)).Normalize();
		Vec3 p = ((CapsuleData)(ref shipRamCapsule)).P1;
		float num = _scaledRamLength * 3f;
		Vec3 val3 = p;
		float num2 = num;
		Vec3 zero = Vec3.Zero;
		if (((WeakGameEntity)(ref val)).RayHitEntityWithNormal(p, val2, num, ref zero, ref num2))
		{
			MatrixFrame identity = MatrixFrame.Identity;
			identity.origin = p + val2 * num2;
			identity.rotation.u = zero;
			identity.rotation.f = Vec3.Up;
			identity.rotation.s = Vec3.CrossProduct(identity.rotation.f, identity.rotation.u);
			((Vec3)(ref identity.rotation.s)).Normalize();
			identity.rotation.f = Vec3.CrossProduct(identity.rotation.u, identity.rotation.s);
			GameEntity val4 = GameEntity.Instantiate(Mission.Current.Scene, "decal_ship_damaged_b_heap", identity, true, "");
			((WeakGameEntity)(ref targetEntity)).AddChild(val4.WeakEntity, true);
			val3 = identity.origin;
			Color val5 = Colors.White;
			WeakGameEntity val6 = ((ScriptComponentBehavior)this).GameEntity;
			val6 = ((WeakGameEntity)(ref val6)).Root;
			ColorAssigner firstScriptOfType = ((WeakGameEntity)(ref val6)).GetFirstScriptOfType<ColorAssigner>();
			if (firstScriptOfType != null)
			{
				val5 = firstScriptOfType.RamDebrisColor;
			}
			foreach (GameEntity child in val4.GetChildren())
			{
				if (child.HasTag("plank"))
				{
					MatrixFrame globalFrame = child.GetGlobalFrame();
					Vec3 val7 = globalFrame.origin + zero * 2f;
					float num3 = 0f;
					Vec3 val8 = globalFrame.origin;
					bool flag = ((WeakGameEntity)(ref val)).RayHitEntity(val7, -zero, 2.5f, ref num3);
					if (flag)
					{
						val8 = val7 - zero * num3;
						Vec3 boundingBoxMax = child.GetBoundingBoxMax();
						Vec3 boundingBoxMin = child.GetBoundingBoxMin();
						_ = val8 + boundingBoxMax.z * globalFrame.rotation.u + boundingBoxMax.x * globalFrame.rotation.s;
						Vec3 val9 = val8 + zero;
						flag = ((WeakGameEntity)(ref val)).RayHitEntity(val9, -zero, 1.5f, ref num3);
						if (flag)
						{
							_ = val8 + boundingBoxMin.z * globalFrame.rotation.u + boundingBoxMax.x * globalFrame.rotation.s;
							Vec3 val10 = val8 + zero;
							flag = ((WeakGameEntity)(ref val)).RayHitEntity(val10, -zero, 1.5f, ref num3);
						}
						if (flag)
						{
							_ = val8 + boundingBoxMin.z * globalFrame.rotation.u + boundingBoxMin.x * globalFrame.rotation.s;
							Vec3 val11 = val8 + zero;
							flag = ((WeakGameEntity)(ref val)).RayHitEntity(val11, -zero, 1.5f, ref num3);
						}
						if (flag)
						{
							_ = val8 + boundingBoxMax.z * globalFrame.rotation.u + boundingBoxMin.x * globalFrame.rotation.s;
							Vec3 val12 = val8 + zero;
							flag = ((WeakGameEntity)(ref val)).RayHitEntity(val12, -zero, 1.5f, ref num3);
						}
					}
					if (flag)
					{
						globalFrame.origin = val8;
						child.SetGlobalFrame(ref globalFrame, true);
						child.SetFactorColor(((Color)(ref val5)).ToUnsignedInteger());
					}
					else
					{
						child.SetVisibilityExcludeParents(false);
					}
				}
				else if (child.HasTag("decal"))
				{
					child.SetFactorColor(((Color)(ref val5)).ToUnsignedInteger());
				}
			}
		}
		else
		{
			MBDebug.Print("Could not hit body\n", 0, (DebugColor)12, 17592186044416uL);
		}
		SoundEventParameter val13 = default(SoundEventParameter);
		((SoundEventParameter)(ref val13))._002Ector("Force", MathF.Min(damage * 0.01f, 1f));
		MBSoundEvent.PlaySound(RamCollisionSoundEffectSoundId, ref val13, val3);
	}

	private void RamCollisionHandleFixedTick(float fixedDt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05db: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_0513: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_056d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0550: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec3 val = ((Vec3)(ref bodyWorldTransform.rotation.f)).NormalizedCopy();
		MissionShip targetShip = _ramCollisionData.TargetShip;
		CapsuleData capsuleData = _ramCollisionData.CapsuleData;
		bool flag = _ramCollisionData.RamWillBeHandled;
		if (_ramCollisionData.HasPoint)
		{
			Vec3 averageIntersectionPoint = _ramCollisionData.AverageIntersectionPoint;
			WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)targetShip).GameEntity;
			MatrixFrame bodyWorldTransform2 = ((WeakGameEntity)(ref gameEntity2)).GetBodyWorldTransform();
			Vec3 val2 = ((Vec3)(ref bodyWorldTransform2.rotation.f)).NormalizedCopy();
			if (_ramStuckTargetShip == null && _lastRamHitQuality > 0 && _ramQualityThresholds[_ramQualityThresholds.Length - _lastRamHitQuality].Item5 && _ramCollisionData.PenetrationLength >= _scaledRamLength * 0.33f && targetShip.HitPoints > 0f)
			{
				_ramStuckTargetShip = targetShip;
			}
			if (_ramStuckTargetShip != null)
			{
				if (_ramStuckTargetShip.HitPoints <= 0f)
				{
					_ramStuckTargetShip = null;
				}
				flag = true;
			}
			Vec3 pointVelocityOnOwner = _ramCollisionData.PointVelocityOnOwner;
			Vec3 pointVelocityOnTarget = _ramCollisionData.PointVelocityOnTarget;
			Vec3 val3 = pointVelocityOnOwner - pointVelocityOnTarget;
			float num = ((Vec3)(ref val3)).Normalize();
			bool flag2 = true;
			float num2 = num * 0.03f;
			if (_ramStuckTargetShip == null)
			{
				flag = true;
				gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
				float num3 = 1f / ((WeakGameEntity)(ref gameEntity)).Mass;
				gameEntity = ((ScriptComponentBehavior)targetShip).GameEntity;
				float num4 = num3 + 1f / ((WeakGameEntity)(ref gameEntity)).Mass;
				Vec3 val4 = -val3 * num2 / num4;
				Vec3 val5 = val3 * num2 / num4;
				float num5 = Vec3.DotProduct(val5, -val);
				if (num5 > 0f)
				{
					Vec3 val6 = -val * num5;
					val5 -= val6;
				}
				GameEntityPhysicsExtensions.ApplyGlobalForceAtLocalPosToDynamicBody(gameEntity2, ((MatrixFrame)(ref bodyWorldTransform2)).TransformToLocal(ref averageIntersectionPoint), val5, (ForceMode)1);
				GameEntityPhysicsExtensions.ApplyGlobalForceAtLocalPosToDynamicBody(((ScriptComponentBehavior)_ownerShip).GameEntity, ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref averageIntersectionPoint), val4, (ForceMode)1);
				float num6 = MathF.Abs(Vec3.DotProduct(pointVelocityOnOwner - pointVelocityOnTarget, val));
				BoundingBox localPhysicsBoundingBox = GameEntityPhysicsExtensions.GetLocalPhysicsBoundingBox(gameEntity2, false);
				Vec3 center = localPhysicsBoundingBox.center;
				center = ((MatrixFrame)(ref bodyWorldTransform2)).TransformToParent(ref center);
				center.z = _ramCollisionData.SelectedIntersectionPoint.z;
				float num7 = MathF.Abs(Vec3.DotProduct(_ramCollisionData.SelectedIntersectionPoint - center, val2));
				float num8 = localPhysicsBoundingBox.max.y - localPhysicsBoundingBox.min.y;
				int num9 = 1;
				float num10 = MathF.Acos(MathF.Abs(Vec3.DotProduct(val, val2))) * (180f / MathF.PI);
				float item = _ramQualityThresholds[_ramQualityThresholds.Length - 1].Item4;
				for (int i = 0; i < _ramQualityThresholds.Length; i++)
				{
					(float, float, float, float, bool) tuple = _ramQualityThresholds[i];
					if (num6 >= tuple.Item1 && num10 >= tuple.Item2 && num7 * 2f <= num8 * tuple.Item3)
					{
						if (tuple.Item5)
						{
							flag2 = true;
						}
						item = tuple.Item4;
						num9 = _ramQualityThresholds.Length - i;
						break;
					}
				}
				float num11 = 12f * (float)Math.Sqrt(_ownerShip.Physics.Mass / 500f) * _ramTierDamageMultiplier * item * num6;
				bool flag3 = !_ramCollisionBeingHandled && flag;
				if (flag3 && _ownerShip.CanDealDamage(targetShip))
				{
					_lastRamHitQuality = num9;
					if (!_ramDamageData.IsValid)
					{
						_ramDamageData = new RamCollisionData
						{
							TargetShip = _ramCollisionData.TargetShip,
							CapsuleData = _ramCollisionData.CapsuleData,
							RamWillBeHandled = _ramCollisionData.RamWillBeHandled,
							SelectedIntersectionPoint = _ramCollisionData.SelectedIntersectionPoint,
							AverageIntersectionPoint = _ramCollisionData.AverageIntersectionPoint,
							RamDirection = _ramCollisionData.RamDirection,
							PenetrationLength = _ramCollisionData.PenetrationLength,
							HasPoint = _ramCollisionData.HasPoint,
							CalculatedDamage = num11
						};
					}
				}
				_ownerShip.ShipsLogic.OnShipRamming(_ownerShip, targetShip, num11 / targetShip.HitPoints, flag3, capsuleData, num9);
			}
			if (flag && _ramStuckTargetShip != null)
			{
				if (1f - Math.Abs(Vec3.DotProduct(val, val2)) < 0.3f)
				{
					_ramStuckTargetShip = null;
				}
				else if (flag2)
				{
					Vec3 val7 = pointVelocityOnOwner - pointVelocityOnTarget;
					gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
					float num12 = 1f / ((WeakGameEntity)(ref gameEntity)).Mass;
					gameEntity = ((ScriptComponentBehavior)targetShip).GameEntity;
					float num13 = num12 + 1f / ((WeakGameEntity)(ref gameEntity)).Mass;
					Vec3 val8 = -0.1f * val7 / num13;
					Vec3 val9 = 0.1f * val7 / num13;
					float num14 = Vec3.DotProduct(val9, -val);
					if (num14 > 0f)
					{
						Vec3 val10 = -((Vec3)(ref val)).NormalizedCopy() * num14;
						val9 -= val10;
					}
					GameEntityPhysicsExtensions.ApplyGlobalForceAtLocalPosToDynamicBody(((ScriptComponentBehavior)_ownerShip).GameEntity, ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref averageIntersectionPoint), val8, (ForceMode)1);
					GameEntityPhysicsExtensions.ApplyGlobalForceAtLocalPosToDynamicBody(((ScriptComponentBehavior)targetShip).GameEntity, ((MatrixFrame)(ref bodyWorldTransform2)).TransformToLocal(ref averageIntersectionPoint), val9, (ForceMode)1);
				}
			}
		}
		else if (_ramStuckTargetShip != null)
		{
			_ramStuckTargetShip = null;
		}
		if (_ramCollisionBeingHandled != flag)
		{
			if (flag)
			{
				GameEntityPhysicsExtensions.PopCapsuleShapeFromEntityBody(((ScriptComponentBehavior)_ownerShip).GameEntity);
			}
			else
			{
				_lastRamHitQuality = 0;
				WeakGameEntity gameEntity3 = ((ScriptComponentBehavior)_ownerShip).GameEntity;
				Vec3 p = ((CapsuleData)(ref capsuleData)).P1;
				Vec3 val11 = ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref p);
				Vec3 p2 = ((CapsuleData)(ref capsuleData)).P2;
				GameEntityPhysicsExtensions.PushCapsuleShapeToEntityBody(gameEntity3, val11, ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref p2), ((CapsuleData)(ref capsuleData)).Radius, "wood_ship");
				Mission.Current.GetMissionBehavior<ShipCollisionOutcomeLogic>()?.ActivateCooldownForShip(_ownerShip, 0.2f);
			}
			_ramCollisionBeingHandled = flag;
		}
	}

	private void RamCollisionCheckTick(float fixedDt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int num = -1;
		MissionShip missionShip = null;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity root = ((WeakGameEntity)(ref gameEntity)).Root;
		gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec3 val = ((Vec3)(ref bodyWorldTransform.rotation.f)).NormalizedCopy();
		CapsuleData ramCapsuleData = GetRamCapsuleData(fixedDt, !_ramCollisionBeingHandled);
		Vec3 val2 = ((CapsuleData)(ref ramCapsuleData)).P2 - ((CapsuleData)(ref ramCapsuleData)).P1;
		Vec3 invalid = Vec3.Invalid;
		WeakGameEntity invalid2 = WeakGameEntity.Invalid;
		float num2 = -1f;
		gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		BodyFlags bodyFlag = ((WeakGameEntity)(ref gameEntity)).BodyFlag;
		Scene ownScene = _ownScene;
		Vec3 val3 = ((CapsuleData)(ref ramCapsuleData)).P1;
		Vec3 val4 = ((CapsuleData)(ref ramCapsuleData)).P1 + val2 * 2f;
		if (ownScene.RayCastForRamming(ref val3, ref val4, ((ScriptComponentBehavior)_ownerShip).GameEntity, _scaledRamRadius, ref num2, ref invalid, ref invalid2, (BodyFlags)(-2147469567), bodyFlag))
		{
			float num3 = -1f;
			missionShip = ((WeakGameEntity)(ref invalid2)).GetFirstScriptWithNameHash(MissionShip.MissionShipScriptNameHash) as MissionShip;
			if (missionShip != null)
			{
				float num4 = 0f;
				int num5 = 0;
				int num6 = 0;
				num6 = _ownScene.GenerateContactsWithCapsule(ref ramCapsuleData, (BodyFlags)896, true, _intersectionsCache, _entitiesCache, _entityPointersCache);
				for (int i = 0; i < num6; i++)
				{
					WeakGameEntity val5 = _entitiesCache[i];
					if (val5 == (GameEntity)null)
					{
						continue;
					}
					WeakGameEntity root2 = ((WeakGameEntity)(ref val5)).Root;
					if (root == root2)
					{
						continue;
					}
					MissionShip firstScriptOfType = ((WeakGameEntity)(ref root2)).GetFirstScriptOfType<MissionShip>();
					if (firstScriptOfType != null && firstScriptOfType != _ownerShip && firstScriptOfType == missionShip)
					{
						if (_ramCollisionBeingHandled && Extensions.HasAnyFlag<BodyFlags>(((WeakGameEntity)(ref val5)).BodyFlag, (BodyFlags)32))
						{
							flag = true;
							continue;
						}
						_selectedIntersectionsCache[num5] = _intersectionsCache[i];
						num5++;
						num4 += Vec3.DotProduct(_intersectionsCache[i].IntersectionPoint - ((CapsuleData)(ref ramCapsuleData)).P1, val);
					}
				}
				if (num5 > 0)
				{
					num3 = num4 / (float)num5;
				}
				float num7 = float.MaxValue;
				for (int j = 0; j < num5; j++)
				{
					if (missionShip != null && missionShip != _ownerShip)
					{
						float num8 = Math.Abs(((Vec3)(ref _selectedIntersectionsCache[j].IntersectionPoint)).DistanceSquared(((CapsuleData)(ref ramCapsuleData)).P1) - num3 * num3);
						if (num8 < num7)
						{
							num7 = num8;
							num = j;
						}
					}
				}
			}
			int num9 = -1;
			Vec3 val6 = Vec3.Invalid;
			Vec3 selectedIntersectionPoint = Vec3.Invalid;
			Vec3 val7 = Vec3.Invalid;
			Vec3 val8 = Vec3.Invalid;
			if (num >= 0)
			{
				val6 = ((CapsuleData)(ref ramCapsuleData)).P1 + val * num3;
				val7 = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)_ownerShip).GameEntity, val6);
				val8 = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)missionShip).GameEntity, val6);
				val3 = Vec3.DotProduct(val7 - val8, val) * val;
				if (((Vec3)(ref val3)).Length > ForwardSpeedThresholdToDamage)
				{
					Vec3 intersectionPoint = _selectedIntersectionsCache[num].IntersectionPoint;
					WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)missionShip).GameEntity;
					MatrixFrame bodyWorldTransform2 = ((WeakGameEntity)(ref gameEntity2)).GetBodyWorldTransform();
					Vec3 val9 = ((Vec3)(ref bodyWorldTransform2.rotation.f)).NormalizedCopy();
					BoundingBox localPhysicsBoundingBox = GameEntityPhysicsExtensions.GetLocalPhysicsBoundingBox(gameEntity2, false);
					Vec3 center = localPhysicsBoundingBox.center;
					center = ((MatrixFrame)(ref bodyWorldTransform2)).TransformToParent(ref center);
					center.z = intersectionPoint.z;
					float num10 = localPhysicsBoundingBox.max.y - localPhysicsBoundingBox.min.y;
					if (MathF.Abs(Vec3.DotProduct(intersectionPoint - center, val9)) * 2f <= num10 * DistanceToShipCenterThresholdToDamage)
					{
						selectedIntersectionPoint = intersectionPoint;
						num9 = num;
					}
				}
			}
			if (missionShip != null && !flag && !_ramCollisionBeingHandled)
			{
				Vec2 asVec = ((Vec3)(ref val)).AsVec2;
				val3 = _ownerShip.Physics.LinearVelocity;
				float speedInRamDirection = ((Vec2)(ref asVec)).DotProduct(((Vec3)(ref val3)).AsVec2);
				_ownerShip.ShipsLogic.OnShipAboutToBeRammed(_ownerShip, missionShip, num2, speedInRamDirection);
			}
			_ramCollisionData = new RamCollisionData
			{
				TargetShip = missionShip,
				CapsuleData = ramCapsuleData,
				RamWillBeHandled = flag,
				SelectedIntersectionPoint = selectedIntersectionPoint,
				AverageIntersectionPoint = val6,
				RamDirection = val,
				PenetrationLength = MathF.Max(0f, _scaledRamLength - num2),
				HasPoint = (num9 >= 0),
				PointVelocityOnOwner = val7,
				PointVelocityOnTarget = val8
			};
		}
		else
		{
			_ramCollisionData = new RamCollisionData
			{
				CapsuleData = ramCapsuleData
			};
		}
	}

	protected override bool CanPhysicsCollideBetweenTwoEntities(WeakGameEntity myEntity, BodyFlags myEntityBodyFlags, WeakGameEntity otherEntity, BodyFlags otherEntityBodyFlags)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (myEntity != ((ScriptComponentBehavior)this).GameEntity)
		{
			return true;
		}
		if (!((WeakGameEntity)(ref otherEntity)).IsValid)
		{
			return true;
		}
		WeakGameEntity root = ((WeakGameEntity)(ref otherEntity)).Root;
		if (((WeakGameEntity)(ref root)).HasScriptOfType<MissionShip>())
		{
			return false;
		}
		return true;
	}
}
