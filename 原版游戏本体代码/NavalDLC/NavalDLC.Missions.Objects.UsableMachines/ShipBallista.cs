using System.Collections.Generic;
using NavalDLC.Missions.AI.UsableMachineAIs;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipBallista : Ballista
{
	private MissionShip _ship;

	[EditableScriptComponentVariable(true, "")]
	private float _horizontalAimSensitivity = 0.5f;

	[EditableScriptComponentVariable(true, "")]
	private float _verticalAimSensitivity = 0.5f;

	private NavalShipsLogic _navalShipsLogic;

	protected override float HorizontalAimSensitivity => _horizontalAimSensitivity;

	protected override float VerticalAimSensitivity => _verticalAimSensitivity;

	protected override bool WeaponMovesDownToReload
	{
		get
		{
			if (!(((UsableMachine)this).Ai as ShipBallistaAI).IsUnderDirectControl)
			{
				return ((UsableMachine)this).PilotAgent.IsAIControlled;
			}
			return false;
		}
	}

	public override string MultipleProjectileId => "ballista_c_projectile_grape";

	public override string MultipleProjectileFlyingId => "ballista_c_projectile_grape_projectile";

	public override string MultipleFireProjectileId => "ballista_c_projectile_grape_fire";

	public override string MultipleFireProjectileFlyingId => "ballista_c_projectile_grape_fire_projectile";

	protected override void OnInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		_ship = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<MissionShip>();
		((Ballista)this).OnInit();
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.ShipSpawnedEvent += OnShipSpawned;
	}

	private void OnShipSpawned(MissionShip ship)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (ship == _ship)
		{
			((RangedSiegeWeapon)this).DefaultSide = ship.BattleSide;
		}
		_navalShipsLogic.ShipSpawnedEvent -= OnShipSpawned;
	}

	public override float GetTargetReleaseAngle(Vec3 target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		Vec3 globalVelocity = ((RangedSiegeWeapon)this).GetGlobalVelocity();
		Vec3 val = ((RangedSiegeWeapon)this).ShootingSpeed * ((RangedSiegeWeapon)this).ShootingDirection + globalVelocity;
		float num = ((Vec3)(ref val)).Normalize();
		float missileVerticalAimCorrection = Mission.GetMissileVerticalAimCorrection(target - ((RangedSiegeWeapon)this).MissileStartingGlobalPositionForSimulation, num, ref ((RangedSiegeWeapon)this).OriginalMissileWeaponStatsDataForTargeting, ItemObject.GetAirFrictionConstant(((RangedSiegeWeapon)this).OriginalMissileItem.PrimaryWeapon.WeaponClass, ((RangedSiegeWeapon)this).OriginalMissileItem.PrimaryWeapon.WeaponFlags));
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		return missileVerticalAimCorrection + MBMath.ToRadians(((Mat3)(ref globalFrame.rotation)).GetEulerAngles().x);
	}

	public override Vec3 GetEstimatedTargetMovementVector(Vec3 targetPosition, Vec3 targetVelocity)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = ((RangedSiegeWeapon)this).ShootingSpeed * ((RangedSiegeWeapon)this).ShootingDirection + ((RangedSiegeWeapon)this).GetGlobalVelocity();
		float num = ((Vec3)(ref val)).Normalize();
		float num2 = 0f;
		float missileTravelTimeApproximation = GetMissileTravelTimeApproximation(((RangedSiegeWeapon)this).MissileStartingGlobalPositionForSimulation, targetPosition, val * num, ItemObject.GetAirFrictionConstant(((RangedSiegeWeapon)this).OriginalMissileItem.PrimaryWeapon.WeaponClass, ((RangedSiegeWeapon)this).OriginalMissileItem.PrimaryWeapon.WeaponFlags));
		Vec3 val2 = targetPosition + targetVelocity * missileTravelTimeApproximation;
		int num3 = 0;
		while (MathF.Abs(missileTravelTimeApproximation - num2) > 1E-05f && num3++ < 10)
		{
			num2 = missileTravelTimeApproximation;
			missileTravelTimeApproximation = GetMissileTravelTimeApproximation(((RangedSiegeWeapon)this).MissileStartingGlobalPositionForSimulation, val2, val * num, ItemObject.GetAirFrictionConstant(((RangedSiegeWeapon)this).OriginalMissileItem.PrimaryWeapon.WeaponClass, ((RangedSiegeWeapon)this).OriginalMissileItem.PrimaryWeapon.WeaponFlags));
			val2 = targetPosition + targetVelocity * missileTravelTimeApproximation;
		}
		return val2 - targetPosition;
	}

	private float GetMissileTravelTimeApproximation(Vec3 startingPos, Vec3 targetPos, Vec3 velocity, float airFriction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = startingPos;
		float num = 0f;
		do
		{
			val += velocity * 0.02f;
			velocity += MBGlobals.GravitationalAcceleration * 0.02f;
			float num2 = ((Vec3)(ref velocity)).Normalize();
			num2 -= airFriction * num2 * num2 * 0.02f;
			velocity *= num2;
			num += 0.02f;
		}
		while (!(((Vec3)(ref val)).DistanceSquared(targetPos) < 0.1f) && (!(((Vec3)(ref val)).DistanceSquared(startingPos) > 100f) || !(val.z < targetPos.z)));
		return num;
	}

	protected override Missile ShootProjectileAux(ItemObject missileItem, bool randomizeMissileSpeed)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = default(Vec3);
		Mat3 val2 = default(Mat3);
		float num = default(float);
		float num2 = default(float);
		((RangedSiegeWeapon)this).SetupProjectileToShoot(randomizeMissileSpeed, ref val, ref val2, ref num, ref num2);
		if (((RangedSiegeWeapon)this).PlayerForceUse)
		{
			((RangedSiegeWeapon)this).LastShooterAgent = Agent.Main;
		}
		WeakGameEntity val3 = ((ScriptComponentBehavior)this).GameEntity;
		val3 = ((WeakGameEntity)(ref val3)).Root;
		MissionObject val4 = (MissionObject)(((object)((WeakGameEntity)(ref val3)).GetFirstScriptOfType<MissionObject>()) ?? ((object)this));
		Mission current = Mission.Current;
		Agent lastShooterAgent = ((RangedSiegeWeapon)this).LastShooterAgent;
		IAgentOriginBase origin = ((RangedSiegeWeapon)this).LastShooterAgent.Origin;
		Missile val5 = current.AddCustomMissile(lastShooterAgent, new MissionWeapon(missileItem, (ItemModifier)null, (origin != null) ? origin.Banner : null, (short)1), ((RangedSiegeWeapon)this).ProjectileEntityCurrentGlobalPosition, val, val2, num2, num, false, val4, -1);
		_navalShipsLogic.AddShipSiegeEngineMissile(val5);
		return val5;
	}

	public override Vec3 GetGlobalVelocity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)_ship).GameEntity, ((RangedSiegeWeapon)this).MissileStartingGlobalPositionForSimulation);
	}

	protected override bool CheckFriendlyFireForObjects(Vec3 globalTargetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		if (((RangedSiegeWeapon)this).CheckFriendlyFireForObjects(globalTargetPosition))
		{
			return true;
		}
		LineSegment2D val4 = default(LineSegment2D);
		foreach (MissionShip item in (List<MissionShip>)(object)_ship.ShipsLogic.AllShips)
		{
			if (item != _ship && item.Team != null && _ship.Team != null && item.Team.TeamSide == _ship.Team.TeamSide)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				Vec3 max = item.Physics.PhysicsBoundingBoxWithChildren.max;
				Vec3 min = item.Physics.PhysicsBoundingBoxWithChildren.min;
				Vec3 center = item.Physics.PhysicsBoundingBoxWithChildren.center;
				Vec3 val = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref center);
				Vec2 asVec = ((Vec3)(ref val)).AsVec2;
				Vec2 asVec2 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				Vec2 val2 = ((Vec2)(ref asVec2)).Normalized();
				val = max - min;
				Vec2 asVec3 = ((Vec3)(ref val)).AsVec2;
				Oriented2DArea val3 = new Oriented2DArea(ref asVec, ref val2, ref asVec3);
				Vec2 asVec4 = ((Vec3)(ref globalTargetPosition)).AsVec2;
				val = ((RangedSiegeWeapon)this).MissileStartingGlobalPositionForSimulation;
				((LineSegment2D)(ref val4))._002Ector(asVec4, ((Vec3)(ref val)).AsVec2);
				if (((Oriented2DArea)(ref val3)).Intersects(ref val4, 1f))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override float ProcessTargetValue(float baseValue, TargetFlags flags)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (Extensions.HasAnyFlag<TargetFlags>(flags, (TargetFlags)64))
		{
			return -1000f;
		}
		if (Extensions.HasAnyFlag<TargetFlags>(flags, (TargetFlags)512))
		{
			baseValue *= 2f;
		}
		if (Extensions.HasAnyFlag<TargetFlags>(flags, (TargetFlags)128))
		{
			baseValue *= 10000f;
		}
		return baseValue;
	}

	protected override void DetermineDefaultBattleSide()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((RangedSiegeWeapon)this).DefaultSide = _ship.BattleSide;
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new ShipBallistaAI((Ballista)(object)this);
	}

	protected override void GetSoundEventIndices()
	{
		((RangedSiegeWeapon)this).MoveSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/ballista_naval/move");
		((RangedSiegeWeapon)this).ReloadSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/ballista_naval/reload");
		((RangedSiegeWeapon)this).FireSoundIndex = SoundEvent.GetEventIdFromString("event:/mission/ballista_naval/fire");
	}
}
