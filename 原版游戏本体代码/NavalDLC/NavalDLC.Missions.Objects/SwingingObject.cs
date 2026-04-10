using System;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects;

public class SwingingObject : MissionObject
{
	[EditableScriptComponentVariable(true, "Damping")]
	public float _damping = 5f;

	[EditableScriptComponentVariable(true, "Center of Mass Height")]
	public float _centerOfMassHeight = -0.8f;

	[EditableScriptComponentVariable(true, "Mass")]
	public float _mass = 1f;

	[EditableScriptComponentVariable(true, "Moment Of Inertia")]
	public float _momentOfInertia = 0.5f;

	[EditableScriptComponentVariable(true, "Reset Simulation")]
	public SimpleButton _resetSimulation = new SimpleButton();

	[EditableScriptComponentVariable(true, "Test Collision")]
	public SimpleButton _testCollision = new SimpleButton();

	private Vec2 _currSwing;

	private Vec2 _prevSwing;

	private Vec2 _swingVelocity;

	private float _minLimitXRotation;

	private Vec3 _accumulatedAcceleration = Vec3.Zero;

	private WeakGameEntity _swingingEntity = WeakGameEntity.Invalid;

	private Vec3 _parentPrevVelocity = Vec3.Zero;

	private MatrixFrame _frameWrtDynamicRoot = MatrixFrame.Identity;

	private Scene _ownerSceneCached;

	internal SwingingObject()
	{
	}//IL_002d: Unknown result type (might be due to invalid IL or missing references)
	//IL_0037: Expected O, but got Unknown
	//IL_0038: Unknown result type (might be due to invalid IL or missing references)
	//IL_0042: Expected O, but got Unknown
	//IL_0043: Unknown result type (might be due to invalid IL or missing references)
	//IL_0048: Unknown result type (might be due to invalid IL or missing references)
	//IL_004e: Unknown result type (might be due to invalid IL or missing references)
	//IL_0053: Unknown result type (might be due to invalid IL or missing references)
	//IL_0059: Unknown result type (might be due to invalid IL or missing references)
	//IL_005e: Unknown result type (might be due to invalid IL or missing references)
	//IL_0064: Unknown result type (might be due to invalid IL or missing references)
	//IL_0069: Unknown result type (might be due to invalid IL or missing references)


	public void DummyFunc()
	{
		Debug.Print(((object)_resetSimulation).ToString(), 0, (DebugColor)12, 17592186044416uL);
	}

	private void InitAux()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		_swingingEntity = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTag("swinging_entity");
		val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		val = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame2 = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		_frameWrtDynamicRoot = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref globalFrame2);
		val = ((ScriptComponentBehavior)this).GameEntity;
		_ownerSceneCached = ((WeakGameEntity)(ref val)).Scene;
		WeakGameEntity firstChildEntityWithName = MBExtensions.GetFirstChildEntityWithName(((ScriptComponentBehavior)this).GameEntity, "collision_sphere");
		Vec3 origin = ((WeakGameEntity)(ref firstChildEntityWithName)).GetLocalFrame().origin;
		origin.x = 0f;
		origin.y = 0f - origin.y;
		((Vec3)(ref origin)).Normalize();
		_minLimitXRotation = 0f - Vec3.DotProduct(origin, Vec3.Forward);
		if (_minLimitXRotation < -MathF.PI / 3f)
		{
			_minLimitXRotation = -MathF.PI / 3f;
		}
	}

	protected override void OnInit()
	{
		InitAux();
	}

	protected override void OnParallelFixedTick(float fixedDt)
	{
		if (fixedDt > 0f)
		{
			HandleSwingMotion(fixedDt);
		}
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (variableName == "Reset Simulation")
		{
			_prevSwing = Vec2.Zero;
			_currSwing = Vec2.Zero;
			_swingVelocity = Vec2.Zero;
		}
		else if (variableName == "Test Collision")
		{
			InitAux();
			_prevSwing.x = _minLimitXRotation;
			_currSwing.x = _minLimitXRotation;
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)36;
	}

	protected override void OnTickParallel(float dt)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (_ownerSceneCached.GetEnginePhysicsEnabled())
		{
			float num = default(float);
			float num2 = default(float);
			_ownerSceneCached.GetInterpolationFactorForBodyWorldTransformSmoothing(ref num, ref num2);
			MatrixFrame identity = MatrixFrame.Identity;
			((Mat3)(ref identity.rotation)).RotateAboutForward(MathF.Lerp(_prevSwing.y, _currSwing.y, num, 1E-05f));
			((Mat3)(ref identity.rotation)).RotateAboutSide(MathF.Lerp(_prevSwing.x, _currSwing.x, num, 1E-05f));
			((WeakGameEntity)(ref _swingingEntity)).SetFrame(ref identity, false);
		}
	}

	private void HandleSwingMotion(float fixedDt)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.Zero;
		WeakGameEntity val2 = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame val3;
		MatrixFrame val4;
		if (GameEntityPhysicsExtensions.HasPhysicsBody(((WeakGameEntity)(ref val2)).Root))
		{
			val2 = ((ScriptComponentBehavior)this).GameEntity;
			val2 = ((WeakGameEntity)(ref val2)).Root;
			val3 = ((WeakGameEntity)(ref val2)).GetBodyWorldTransform();
			val4 = ((MatrixFrame)(ref val3)).TransformToParent(ref _frameWrtDynamicRoot);
			val2 = ((ScriptComponentBehavior)this).GameEntity;
			val = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((WeakGameEntity)(ref val2)).Root, val4.origin);
		}
		else
		{
			val2 = ((ScriptComponentBehavior)this).GameEntity;
			val2 = ((WeakGameEntity)(ref val2)).Root;
			val3 = ((WeakGameEntity)(ref val2)).GetFrame();
			val4 = ((MatrixFrame)(ref val3)).TransformToParent(ref _frameWrtDynamicRoot);
		}
		Vec3 val5 = (val - _parentPrevVelocity) / fixedDt;
		_parentPrevVelocity = val;
		Vec3 val6 = MBGlobals.GravitationalAcceleration - val5 + _accumulatedAcceleration;
		_accumulatedAcceleration = Vec3.Zero;
		val3 = ((WeakGameEntity)(ref _swingingEntity)).GetFrame();
		MatrixFrame val7 = ((MatrixFrame)(ref val4)).TransformToParent(ref val3);
		Vec3 origin = val7.origin;
		Vec3 val8 = origin + val7.rotation.u * _centerOfMassHeight;
		Vec3 val9 = ((MatrixFrame)(ref val7)).TransformToLocalNonOrthogonal(ref origin);
		Vec3 val10 = ((MatrixFrame)(ref val7)).TransformToLocalNonOrthogonal(ref val8);
		Vec3 val11 = ((Mat3)(ref val7.rotation)).TransformToLocal(ref val6);
		Vec3 val12 = val10 - val9;
		Vec3 val13 = val11 * _mass;
		Vec3 val14 = Vec3.CrossProduct(val12, val13);
		float num = MathF.Max(_momentOfInertia * _mass, 0.001f);
		Vec3 side = Vec3.Side;
		float num2 = Vec3.DotProduct(val14, side) / num;
		_swingVelocity.x += num2 * fixedDt;
		Vec3 forward = Vec3.Forward;
		float num3 = Vec3.DotProduct(val14, forward) / num;
		_swingVelocity.y += num3 * fixedDt;
		if (MathF.Abs(_swingVelocity.x) > 5f)
		{
			_swingVelocity.x = 5f * (float)MathF.Sign(_swingVelocity.x);
		}
		if (MathF.Abs(_swingVelocity.y) > 5f)
		{
			_swingVelocity.y = 5f * (float)MathF.Sign(_swingVelocity.y);
		}
		_prevSwing = _currSwing;
		_currSwing += _swingVelocity * fixedDt;
		if (_currSwing.x > MathF.PI / 3f && _swingVelocity.x > 0f)
		{
			_swingVelocity.x *= -0.1f;
		}
		if (_currSwing.x < _minLimitXRotation)
		{
			_currSwing.x = _minLimitXRotation;
			if (_swingVelocity.x < 0f)
			{
				_swingVelocity.x *= -0.1f;
			}
		}
		if (_currSwing.y > MathF.PI / 3f)
		{
			_currSwing.y = MathF.PI / 3f;
			if (_swingVelocity.y > 0f)
			{
				_swingVelocity.y *= -0.1f;
			}
		}
		if (_currSwing.y < -MathF.PI / 3f)
		{
			_currSwing.y = -MathF.PI / 3f;
			if (_swingVelocity.y < 0f)
			{
				_swingVelocity.y *= -0.1f;
			}
		}
		Vec2 swingVelocity = _swingVelocity;
		float num4 = ((Vec2)(ref swingVelocity)).Normalize();
		float num5 = (_damping * 0.2f * num4 + _damping * 0.03f) / _mass;
		if (num5 > num4)
		{
			_swingVelocity = Vec2.Zero;
		}
		else
		{
			_swingVelocity -= swingVelocity * num5;
		}
	}

	protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage, out float finalDamage, out float fireDamage, out float modifiedFireDamage)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		MissionWeapon val = weapon;
		float num;
		if (((MissionWeapon)(ref val)).Item == null)
		{
			num = 1f;
		}
		else
		{
			val = weapon;
			num = ((MissionWeapon)(ref val)).GetWeight();
		}
		float num2 = num;
		num2 = MathF.Clamp(num2, 0.5f, 2f);
		Vec3 val2 = impactDirection * num2 * 300f;
		_accumulatedAcceleration += val2 / _mass;
		reportDamage = false;
		finalDamage = 0f;
		fireDamage = -1f;
		modifiedFireDamage = -1f;
		return true;
	}
}
