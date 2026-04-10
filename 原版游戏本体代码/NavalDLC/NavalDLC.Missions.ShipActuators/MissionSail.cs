using System;
using System.Collections.Generic;
using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.ShipActuators;

public class MissionSail : MissionObject
{
	public enum SailTurningState : sbyte
	{
		Stationary,
		TurningLeft,
		TurningRight
	}

	public const float OptimalDirectionSearchInterval = 1f;

	private const int PhysicsPointCountPerAxis = 9;

	private const float BlowSoundEventCooldown = 10f;

	private static readonly int _sailContinuousSoundEventId = SoundEvent.GetEventIdFromString("event:/mission/movement/vessel/sail/sail_movement");

	private static readonly int _sailRotationSoundEventId = SoundEvent.GetEventIdFromString("event:/mission/movement/vessel/sail/sail_rotation");

	private const float MinSearchSpaceForTargetSailRotationInRadians = MathF.PI / 30f;

	private ShipSail _sailObject;

	private MissionShip _ownerShip;

	private SailVisual _sailVisual;

	private float _localSailRotation;

	private float _currentSailRotationSpeed;

	private Vec3 _centerOfSailForceShipLocal;

	private float _width;

	private float _height;

	private float _sailRotationStateTimer;

	private float _fullSailWeight;

	private bool _fullSailMode;

	private ShipForce _force;

	private bool _gustMode;

	private SailTurningState _currentSailTurningState;

	private float _targetSailRotation;

	private SoundEvent _sailContinuousSoundEvent;

	private SoundEvent _sailRotationSoundEvent;

	private float _blowSoundEventCooldown;

	private float _sailSoundEventRotationParam;

	private bool _shouldMakeBlowingSound;

	public override TextObject HitObjectName => new TextObject("{=92jVTPDA}Ship Sails", (Dictionary<string, object>)null);

	public ShipSail SailObject => _sailObject;

	public ShipForce Force => _force;

	public float LocalSailRotation => _localSailRotation;

	public float Setting { get; private set; }

	public float TargetSailSetting { get; private set; }

	public Vec3 CenterOfSailForceShipLocal => _centerOfSailForceShipLocal;

	public float FoldDuration => _sailVisual.TotalFoldDuration;

	public float UnfoldDuration => _sailVisual.TotalUnfoldDuration;

	public GameEntity SailEntity { get; private set; }

	public float Area => _width * (((int)_sailObject.Type == 1) ? (_height * 0.5f) : _height);

	internal void InitWithVariables(ShipSail sailObject, MissionShip ownerShip, SailVisual sailVisual)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		_sailObject = sailObject;
		_ownerShip = ownerShip;
		_sailVisual = sailVisual;
		SailEntity = GameEntity.CreateFromWeakEntity(((ScriptComponentBehavior)sailVisual).GameEntity);
		InitializeCenterOfSailForceLocal();
		Setting = 0f;
		_sailRotationStateTimer = 7f;
		_fullSailWeight = 0f;
		_localSailRotation = (0f - _sailObject.RightRotationLimit + _sailObject.LeftRotationLimit) * 0.5f;
		_localSailRotation = MathF.Clamp(_localSailRotation, 0f - _sailObject.RightRotationLimit, _sailObject.LeftRotationLimit);
		_targetSailRotation = _localSailRotation;
		_currentSailRotationSpeed = 0f;
		TargetSailSetting = 1f;
		_currentSailTurningState = SailTurningState.Stationary;
		_gustMode = false;
		SetVisualSailEnabled(visualSailEnabled: false);
		InitializeSailSounds();
		InitSailRotationAccordingToWindDirection();
	}

	private void InitSailRotationAccordingToWindDirection()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Vec2 globalWindVelocity = ((ScriptComponentBehavior)_ownerShip).Scene.GetGlobalWindVelocity();
		if (((Vec2)(ref globalWindVelocity)).LengthSquared > 1f)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			ref Mat3 rotation = ref globalFrame.rotation;
			Vec3 val = ((Vec2)(ref globalWindVelocity)).ToVec3(0f);
			Vec3 val2 = ((Mat3)(ref rotation)).TransformToLocal(ref val);
			Vec2 asVec = ((Vec3)(ref val2)).AsVec2;
			Vec2 relWindDirection2DShip = ((Vec2)(ref asVec)).Normalized();
			FixedTickTargetSailRotation(relWindDirection2DShip, forceFindTheBestAngle: true);
			_localSailRotation = _targetSailRotation;
		}
	}

	public bool CheckSailFlags(bool editMode)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		List<GameEntity> list = new List<GameEntity>();
		SailEntity.GetChildrenRecursive(ref list);
		bool flag = false;
		list.Add(SailEntity);
		foreach (GameEntity item in list)
		{
			if (!Extensions.HasAnyFlag<EntityFlags>(item.EntityFlags, (EntityFlags)131072) && !Extensions.HasAnyFlag<EntityFlags>(item.EntityFlags, (EntityFlags)4096))
			{
				flag = true;
			}
		}
		if (flag)
		{
			string text = "In Root Entity " + SailEntity.Root.Name + ", " + SailEntity.Name + "'s every descendant including itself must have Does not Affect Parent's Local Bounding Box flag.";
			if (editMode)
			{
				MBEditor.AddEntityWarning(SailEntity.WeakEntity, text);
			}
		}
		return flag;
	}

	public void UpdateForcedWindOfSailsAndTopBanner(float dt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		Vec3 linearVelocity = _ownerShip.Physics.LinearVelocity;
		Vec3 angularVelocity = _ownerShip.Physics.AngularVelocity;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec3 localCenterOfMass = _ownerShip.Physics.LocalCenterOfMass;
		Vec3 val = ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref localCenterOfMass);
		Vec2 globalWindVelocity = ((ScriptComponentBehavior)_ownerShip).Scene.GetGlobalWindVelocity();
		Vec3 val2 = ((Vec2)(ref globalWindVelocity)).ToVec3(0f);
		Vec3 val3 = ComputeCenterOfSailForceGlobal() - val;
		Vec3 val4 = Vec3.CrossProduct(angularVelocity, val3) + linearVelocity;
		Vec3 sailRelativeGlobalWindVelocity = val2 - val4;
		Vec3 val5 = _sailVisual.SailTopBannerEntity.GetGlobalFrame().origin - val;
		Vec3 val6 = Vec3.CrossProduct(angularVelocity, val5) + linearVelocity;
		Vec3 globalBannerRelativeWindVelocity = val2 - val6;
		Vec3 globalSailForce = ((!_force.IsApplicable) ? Vec3.Zero : (_force.Force / _force.GamifiedForceMultiplier));
		_sailVisual.UpdateForcedWindOfSailsAndTopBanner(dt, globalBannerRelativeWindVelocity, in sailRelativeGlobalWindVelocity, in globalSailForce);
	}

	private void SetTargetSailSetting(in ShipActuatorRecord actuatorInput)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		if ((int)_sailObject.Type == 0)
		{
			TargetSailSetting = actuatorInput.SquareSailSetting;
		}
		else if ((int)_sailObject.Type == 1)
		{
			TargetSailSetting = actuatorInput.LateenSailSetting;
		}
	}

	private void FixedUpdateSailForce(Vec3 windVelocityGlobal, Vec3 sailLinearVelocityGlobal, Vec3 sailLinearVelocityFromAngularGlobal)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		MatrixFrame shipFrame = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec3 val = Compute3DSailDirection();
		Vec3 force = Vec3.Zero;
		Vec2 asVec;
		if (shipFrame.rotation.u.z > 0f)
		{
			Vec3 val2 = windVelocityGlobal * shipFrame.rotation.u.z * shipFrame.rotation.u.z - sailLinearVelocityGlobal;
			float num = 16f;
			if (((Vec3)(ref val2)).LengthSquared > num * num)
			{
				val2 = ((Vec3)(ref val2)).NormalizedCopy() * num;
			}
			Vec3 val3 = ((Mat3)(ref shipFrame.rotation)).TransformToLocal(ref val2);
			Vec2 relWindDirection2DShip = ((Vec3)(ref val3)).AsVec2;
			float num2 = ((Vec2)(ref relWindDirection2DShip)).Normalize();
			Vec3 val4 = ((Mat3)(ref shipFrame.rotation)).TransformToLocal(ref val);
			asVec = ((Vec3)(ref val4)).AsVec2;
			Vec2 sailDirection2DShip = ((Vec2)(ref asVec)).Normalized();
			float num3 = MathF.Abs(shipFrame.rotation.u.z);
			float effectiveSailArea = Setting * Area * num3;
			float relWindSpeed2DShip = num2 * num3;
			Vec3 val5 = ComputeSailForce(in sailDirection2DShip, in relWindDirection2DShip, relWindSpeed2DShip, in shipFrame, effectiveSailArea, _sailObject.Type);
			if (_gustMode)
			{
				val5 *= 0.5f;
			}
			force += val5;
		}
		Vec3 val6 = -sailLinearVelocityFromAngularGlobal;
		Vec3 val7 = ((Mat3)(ref shipFrame.rotation)).TransformToLocal(ref val6);
		Vec2 relWindDirection2DShip2 = ((Vec3)(ref val7)).AsVec2;
		float num4 = ((Vec2)(ref relWindDirection2DShip2)).Normalize();
		Vec3 val8 = ((Mat3)(ref shipFrame.rotation)).TransformToLocal(ref val);
		asVec = ((Vec3)(ref val8)).AsVec2;
		Vec2 sailDirection2DShip2 = ((Vec2)(ref asVec)).Normalized();
		float effectiveSailArea2 = Setting * Area;
		float relWindSpeed2DShip2 = num4;
		Vec3 val9 = ComputeSailForce(in sailDirection2DShip2, in relWindDirection2DShip2, relWindSpeed2DShip2, in shipFrame, effectiveSailArea2, _sailObject.Type);
		if (shipFrame.rotation.u.z <= 0f)
		{
			val9 *= 2f;
		}
		force += val9;
		float num5 = (1f + _ownerShip.ShipOrigin.SailForceFactor) * _sailObject.ForceMultiplier;
		force *= num5;
		float num6 = ((Vec3)(ref force)).Normalize();
		float num7 = MissionGameModels.Current.MissionShipParametersModel.CalculateWindBonus(_ownerShip.ShipOrigin, _ownerShip.Captain, num6);
		float num8 = ((num6 > 0f) ? (num7 / num6) : 1f);
		num5 *= num8;
		force *= num7;
		_force = new ShipForce(in _centerOfSailForceShipLocal, in force, ShipForce.SourceType.Sail, num5);
	}

	public void FixedUpdate(float fixedDt, in ShipActuatorRecord actuatorInput, in Vec3 shipLinearVelocityGlobal, in Vec3 shipAngularVelocityGlobal)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (_ownerShip.ShipSailState == MissionShip.SailState.Intact)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
			MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			Vec3 localCenterOfMass = _ownerShip.Physics.LocalCenterOfMass;
			Vec3 val = ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref localCenterOfMass);
			Vec3 val2 = ComputeCenterOfSailForceGlobal() - val;
			Vec3 sailLinearVelocityFromAngularGlobal = Vec3.CrossProduct(shipAngularVelocityGlobal, val2);
			Vec3 val3 = shipLinearVelocityGlobal;
			gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
			Vec2 globalWindVelocityOfScene = ((WeakGameEntity)(ref gameEntity)).GetGlobalWindVelocityOfScene();
			Vec3 val4 = ((Vec2)(ref globalWindVelocityOfScene)).ToVec3(0f);
			Vec3 relWindVelocityGlobal = val4 - val3;
			SetTargetSailSetting(in actuatorInput);
			float localSailRotation = _localSailRotation;
			FixedUpdateSailRotation(fixedDt, in actuatorInput, in relWindVelocityGlobal);
			if (TargetSailSetting == 1f)
			{
				Vec3 force = _force.Force;
				FixedUpdateSailForce(val4, val3, sailLinearVelocityFromAngularGlobal);
				if (_ownerShip.ShouldUpdateSoundPos && _blowSoundEventCooldown <= 0.01f && ((Vec3)(ref _force.Force)).LengthSquared / ((Vec3)(ref force)).LengthSquared > 1.21f)
				{
					_shouldMakeBlowingSound = true;
					_blowSoundEventCooldown += 10f;
				}
				CalculateSailSoundEventRotationParamAndShouldUpdateSoundPos(fixedDt, MathF.Abs(_localSailRotation - localSailRotation));
				_blowSoundEventCooldown -= fixedDt;
				_blowSoundEventCooldown = ((_blowSoundEventCooldown < 0f) ? 0f : _blowSoundEventCooldown);
			}
			else
			{
				_force = new ShipForce(in _centerOfSailForceShipLocal, in Vec3.Zero, ShipForce.SourceType.Sail, 1f);
			}
		}
		else
		{
			_force = new ShipForce(in _centerOfSailForceShipLocal, in Vec3.Zero, ShipForce.SourceType.Sail, 1f);
		}
	}

	private void UpdateSailRotationVisual(float dt)
	{
		float value = _targetSailRotation - _localSailRotation;
		float val = Math.Abs(value);
		float val2 = dt * _currentSailRotationSpeed;
		val2 = Math.Min(val, val2);
		float num = _localSailRotation + (float)Math.Sign(value) * val2;
		_localSailRotation = MathF.Clamp(num, 0f - _sailObject.RightRotationLimit, _sailObject.LeftRotationLimit);
	}

	private void UpdateSailSetting(float dt)
	{
		float targetSailSetting = TargetSailSetting;
		float num = ((targetSailSetting - Setting >= 0f) ? UnfoldDuration : FoldDuration);
		float num2 = 1f / num;
		Setting = ShipActuators.ComputeActuatorParameter(Setting, targetSailSetting, dt, num2 * (1f + _ownerShip.ShipOrigin.FurlUnfurlSpeedFactor));
		Setting = MathF.Clamp(Setting, 0f, 1f);
	}

	private void UpdateSailVisuals(float dt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame localFrame = _sailVisual.SailYawRotationEntity.GetLocalFrame();
		localFrame.rotation = Mat3.Identity;
		((Mat3)(ref localFrame.rotation)).RotateAboutUp(_localSailRotation);
		_sailVisual.SailYawRotationEntity.SetLocalFrame(ref localFrame, false);
		SetVisualSailEnabled(TargetSailSetting > 0.5f);
		UpdateForcedWindOfSailsAndTopBanner(dt);
	}

	private void UpdateSoundPos()
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		if (_ownerShip.ShouldUpdateSoundPos && _sailContinuousSoundEvent == null)
		{
			int sailContinuousSoundEventId = _sailContinuousSoundEventId;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
			_sailContinuousSoundEvent = SoundEvent.CreateEvent(sailContinuousSoundEventId, ((WeakGameEntity)(ref gameEntity)).Scene);
			int sailRotationSoundEventId = _sailRotationSoundEventId;
			gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
			_sailRotationSoundEvent = SoundEvent.CreateEvent(sailRotationSoundEventId, ((WeakGameEntity)(ref gameEntity)).Scene);
			Vec3 position = ComputeCenterOfSailForceGlobal();
			_sailContinuousSoundEvent.SetPosition(position);
			_sailRotationSoundEvent.SetPosition(position);
			_sailRotationSoundEvent.SetParameter("SailRotation", _sailSoundEventRotationParam);
			_sailRotationSoundEvent.Play();
			_sailContinuousSoundEvent.Play();
		}
		else if (_ownerShip.ShouldUpdateSoundPos)
		{
			Vec3 position2 = ComputeCenterOfSailForceGlobal();
			_sailContinuousSoundEvent.SetPosition(position2);
			_sailRotationSoundEvent.SetPosition(position2);
			_sailRotationSoundEvent.SetParameter("SailRotation", _sailSoundEventRotationParam);
			if (_shouldMakeBlowingSound)
			{
				MatrixFrame globalFrame = SailEntity.GetGlobalFrame();
				SoundManager.StartOneShotEvent("event:/mission/movement/vessel/sail/sail_blow", ref globalFrame.origin);
				_shouldMakeBlowingSound = false;
			}
		}
		else if (_sailContinuousSoundEvent != null)
		{
			_sailRotationSoundEvent.Stop();
			_sailContinuousSoundEvent.Stop();
			_sailRotationSoundEvent = null;
			_sailContinuousSoundEvent = null;
		}
	}

	public void Update(float dt)
	{
		UpdateSailRotationVisual(dt);
		UpdateSailSetting(dt);
		UpdateSailVisuals(dt);
		UpdateSoundPos();
	}

	public static Vec3 ComputeSailForce(in Vec2 sailDirection2DShip, in Vec2 relWindDirection2DShip, float relWindSpeed2DShip, in MatrixFrame shipFrame, float effectiveSailArea, SailType sailType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Vec2 sailForceCoefficients = SailWindProfile.Instance.GetSailForceCoefficients(sailType, sailDirection2DShip, relWindDirection2DShip);
		float num = relWindSpeed2DShip * relWindSpeed2DShip;
		float airDensity = GameModels.Instance.ShipPhysicsParametersModel.GetAirDensity();
		Vec2 val = 0.5f * airDensity * num * sailForceCoefficients * effectiveSailArea;
		Mat3 rotation = shipFrame.rotation;
		Vec3 val2 = ((Vec2)(ref val)).ToVec3(0f);
		return ((Mat3)(ref rotation)).TransformToParent(ref val2);
	}

	public float ComputeMaximumForceMagnitudeSailCanApply()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vec2 maximumSailForceCoefficients = SailWindProfile.Instance.GetMaximumSailForceCoefficients(_sailObject.Type);
		float maximumWindSpeed = Scene.MaximumWindSpeed;
		float area = Area;
		return 0.5f * GameModels.Instance.ShipPhysicsParametersModel.GetAirDensity() * maximumWindSpeed * maximumWindSpeed * (_sailObject.ForceMultiplier * ((Vec2)(ref maximumSailForceCoefficients)).Length) * area * (1f + _ownerShip.ShipOrigin.SailForceFactor);
	}

	private Vec3 ComputeWindVectorForSailVisuals(in Vec3 sailForceGlobal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = sailForceGlobal;
		Vec3 val2 = ((Vec3)(ref val)).NormalizedCopy();
		val = sailForceGlobal;
		float num = MathF.Sqrt(((Vec3)(ref val)).Length * 2f / (GameModels.Instance.ShipPhysicsParametersModel.GetAirDensity() * _sailObject.ForceMultiplier * Area));
		return val2 * num;
	}

	private void SetVisualSailEnabled(bool visualSailEnabled)
	{
		_sailVisual.SailEnabled = visualSailEnabled;
	}

	private void FixedTickFullSailInputWeight(float fixedDt, in ShipActuatorRecord actuatorInput)
	{
		float num = actuatorInput.RowerThrust;
		if (TargetSailSetting <= 0f || (!_gustMode && TargetSailSetting < 1f))
		{
			num = 0f;
		}
		if (num > 0f)
		{
			float rowerThrustDoubleTap = actuatorInput.RowerThrustDoubleTap;
			if (_fullSailWeight >= 0f)
			{
				_fullSailWeight += fixedDt * 0.4f;
				if (rowerThrustDoubleTap > 0f && _fullSailWeight < 0.5f)
				{
					_fullSailWeight = 0.5f;
				}
			}
			else
			{
				_fullSailWeight += fixedDt * 2f;
				_fullSailMode = false;
			}
			if (_fullSailWeight >= 1f)
			{
				_fullSailMode = true;
				_fullSailWeight = 1f;
			}
		}
		else if (num < 0f)
		{
			if (_fullSailWeight <= 0f)
			{
				_fullSailWeight -= fixedDt * 0.4f;
			}
			else
			{
				_fullSailWeight -= fixedDt * 2f;
				_fullSailMode = false;
			}
			if (_fullSailWeight <= -1f)
			{
				_fullSailMode = true;
				_fullSailWeight = -1f;
			}
		}
		else
		{
			float num2 = fixedDt * 2f;
			if (MathF.Abs(_fullSailWeight) <= num2)
			{
				_fullSailMode = false;
				_fullSailWeight = 0f;
			}
			else
			{
				_fullSailWeight -= (float)MathF.Sign(_fullSailWeight) * num2;
			}
		}
	}

	public bool GetVisualSailEnabled()
	{
		return _sailVisual.SailEnabled;
	}

	private void FixedTickTargetSailRotation(Vec2 relWindDirection2DShip, bool forceFindTheBestAngle)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		float num = ((_currentSailTurningState != SailTurningState.Stationary) ? _targetSailRotation : _localSailRotation);
		Vec2 forward = Vec2.Forward;
		((Vec2)(ref forward)).RotateCCW(num);
		float num2 = SailWindProfile.Instance.ComputeSailThrustValue(_sailObject.Type, forward, Vec2.Forward, relWindDirection2DShip);
		float targetSailRotation = num;
		float num3 = 1f;
		if (!forceFindTheBestAngle && !_gustMode && _currentSailTurningState == SailTurningState.Stationary)
		{
			num3 = ((!_fullSailMode || !(_fullSailWeight > 0f)) ? 1.3f : 1.1f);
		}
		float num4 = num2 * num3;
		float num5 = 0f - _sailObject.RightRotationLimit;
		float num6 = _sailObject.LeftRotationLimit;
		if (_currentSailTurningState == SailTurningState.TurningLeft)
		{
			num5 = _localSailRotation;
		}
		else if (_currentSailTurningState == SailTurningState.TurningRight)
		{
			num6 = _localSailRotation;
		}
		float num7 = (num6 - num5) * 0.01f;
		if (num6 - num5 > MathF.PI / 30f)
		{
			for (float num8 = num5; num8 <= num6; num8 += num7)
			{
				Vec2 forward2 = Vec2.Forward;
				((Vec2)(ref forward2)).RotateCCW(num8);
				float num9 = SailWindProfile.Instance.ComputeSailThrustValue(_sailObject.Type, forward2, Vec2.Forward, relWindDirection2DShip);
				float num10 = num9;
				if (num10 > num4)
				{
					num4 = num10;
					num2 = num9;
					targetSailRotation = num8;
				}
			}
			if (forceFindTheBestAngle)
			{
				if (num2 > 0f)
				{
					_targetSailRotation = targetSailRotation;
				}
			}
			else if (!_gustMode || num2 > 0f)
			{
				_targetSailRotation = targetSailRotation;
			}
		}
		_targetSailRotation = MathF.Clamp(_targetSailRotation, 0f - _sailObject.RightRotationLimit, _sailObject.LeftRotationLimit);
	}

	private void FixedTickSailGustMode(float thrustDirection, float curSailThrustValue, float maxThrustValue)
	{
		if (thrustDirection >= 0f)
		{
			if (_fullSailMode && _fullSailWeight > 0f)
			{
				_gustMode = curSailThrustValue < 0f;
			}
			else if (_gustMode && (curSailThrustValue > 0f || maxThrustValue > 0f || !(curSailThrustValue * _fullSailWeight <= 0f)))
			{
				_gustMode = false;
			}
		}
		else
		{
			_gustMode = true;
		}
	}

	private Vec3 Compute3DSailDirection()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec3 f = bodyWorldTransform.rotation.f;
		Vec3 u = bodyWorldTransform.rotation.u;
		Vec3 result = ((Vec3)(ref f)).RotateAboutAnArbitraryVector(u, _localSailRotation);
		((Vec3)(ref result)).Normalize();
		return result;
	}

	private void InitializeCenterOfSailForceLocal()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		MatrixFrame shipFrame = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		_sailVisual.GetDimensions(in shipFrame, (int)_sailObject.Type == 1, out _width, out _height, out _centerOfSailForceShipLocal);
	}

	private void FixedUpdateSailRotation(float fixedDt, in ShipActuatorRecord actuatorInput, in Vec3 relWindVelocityGlobal)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		Vec3 val = ((Mat3)(ref bodyWorldTransform.rotation)).TransformToLocal(ref relWindVelocityGlobal);
		Vec2 asVec = ((Vec3)(ref val)).AsVec2;
		Vec2 val2 = ((Vec2)(ref asVec)).Normalized();
		float rowerThrust = actuatorInput.RowerThrust;
		if (TargetSailSetting <= 0f)
		{
			_sailRotationStateTimer = float.MaxValue;
		}
		FixedTickFullSailInputWeight(fixedDt, in actuatorInput);
		bool flag = TargetSailSetting == 1f && Setting == 0f;
		if (flag)
		{
			_sailRotationStateTimer = 0f;
		}
		if (_fullSailMode && _fullSailWeight > 0f && _sailRotationStateTimer > 2f && _currentSailTurningState == SailTurningState.Stationary)
		{
			_sailRotationStateTimer = 2f;
		}
		_sailRotationStateTimer -= fixedDt;
		Vec2 forward = Vec2.Forward;
		((Vec2)(ref forward)).RotateCCW(_localSailRotation);
		float num = SailWindProfile.Instance.ComputeSailThrustValue(_sailObject.Type, forward, Vec2.Forward, val2);
		bool num2 = (_currentSailTurningState != SailTurningState.Stationary || _sailRotationStateTimer <= 0f) && TargetSailSetting >= 1f;
		float num3 = num;
		if (num2)
		{
			FixedTickTargetSailRotation(val2, flag);
			Vec2 forward2 = Vec2.Forward;
			((Vec2)(ref forward2)).RotateCCW(_targetSailRotation);
			float num4 = SailWindProfile.Instance.ComputeSailThrustValue(_sailObject.Type, forward2, Vec2.Forward, val2);
			num3 = MathF.Max(num3, num4);
			if (_currentSailTurningState == SailTurningState.Stationary && !MBMath.ApproximatelyEqualsTo(_targetSailRotation, _localSailRotation, MathF.PI / 30f))
			{
				_sailRotationStateTimer = 30f;
				_currentSailTurningState = ((!(_targetSailRotation < _localSailRotation)) ? SailTurningState.TurningLeft : SailTurningState.TurningRight);
			}
		}
		FixedTickSailGustMode(rowerThrust, num, num3);
		if (_currentSailTurningState != SailTurningState.Stationary)
		{
			float num5 = _sailObject.RotationRate * (1f + _ownerShip.ShipOrigin.SailRotationSpeedFactor);
			float num6 = _targetSailRotation - _localSailRotation;
			float num7 = Math.Abs(num6);
			float num8 = num7 / num5;
			if (TargetSailSetting < 1f && num8 > 1f)
			{
				num7 = num5;
				num6 = (float)MathF.Sign(num6) * num7;
				_targetSailRotation = _localSailRotation + num6;
				num8 = 1f;
			}
			float num9 = ((!(num8 > 1f)) ? (num7 / 1f) : num5);
			_currentSailRotationSpeed = MathF.Lerp(_currentSailRotationSpeed, num9, fixedDt * 2f, 1E-05f);
			if (MBMath.ApproximatelyEqualsTo(_currentSailRotationSpeed, 0f, 0.005f) && MBMath.ApproximatelyEqualsTo(num7, 0f, 0.005f))
			{
				_sailRotationStateTimer = ((_fullSailMode && _fullSailWeight > 0f) ? 2f : 2f);
				_currentSailTurningState = SailTurningState.Stationary;
				_currentSailRotationSpeed = 0f;
			}
		}
	}

	private Vec3 ComputeCenterOfSailForceGlobal()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
		return ((MatrixFrame)(ref bodyWorldTransform)).TransformToParent(ref _centerOfSailForceShipLocal);
	}

	public void ForceFold()
	{
		_sailVisual.InstantCloseSails();
	}

	private void CalculateSailSoundEventRotationParamAndShouldUpdateSoundPos(float dt, float rotationDiff)
	{
		if (_ownerShip.ShouldUpdateSoundPos)
		{
			float num = dt * _sailObject.RotationRate;
			_sailSoundEventRotationParam = ((num > 0f) ? MathF.Clamp(rotationDiff / num, 0f, 1f) : 0f);
		}
	}

	private void InitializeSailSounds()
	{
		CalculateSailSoundEventRotationParamAndShouldUpdateSoundPos(0f, 0f);
		UpdateSoundPos();
		_blowSoundEventCooldown = 0f;
	}

	private BoundingBox GetPhysicsBoundingBox()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		BoundingBox result = default(BoundingBox);
		((BoundingBox)(ref result)).BeginRelaxation();
		MatrixFrame sailGlobalFrame = _sailVisual.SailSkeletonEntity.GetGlobalFrame();
		if ((int)_sailObject.Type == 0)
		{
			Vec3 val = default(Vec3);
			((Vec3)(ref val))._002Ector(-0.5f, 0f, -0.5f, -1f);
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					Vec3 globalSailPoint = GetGlobalSailPoint(val + 0.125f * new Vec3((float)j, 0f, (float)i, -1f), in sailGlobalFrame);
					((BoundingBox)(ref result)).RelaxMinMaxWithPoint(ref globalSailPoint);
				}
			}
		}
		else
		{
			Vec3 val2 = default(Vec3);
			((Vec3)(ref val2))._002Ector(-0.5f, 0f, 0f, -1f);
			for (int k = 0; k < 5; k++)
			{
				int num = 9 - k * 2;
				for (int l = 0; l < num; l++)
				{
					Vec3 globalSailPoint2 = GetGlobalSailPoint(val2 + 0.125f * new Vec3((float)(l + k), 0f, (float)(-k), -1f), in sailGlobalFrame);
					((BoundingBox)(ref result)).RelaxMinMaxWithPoint(ref globalSailPoint2);
				}
			}
		}
		return result;
	}

	public bool IsBurningFinished()
	{
		return _sailVisual.IsBurningFinished();
	}

	public bool IsBurning()
	{
		return _sailVisual.IsBurning();
	}

	public void StartFire()
	{
		_sailVisual.StartFire();
	}

	public bool IntersectLineSegmentWithSail(in Vec3 lineSegmentStart, in Vec3 lineSegmentEnd)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		BoundingBox physicsBoundingBox = GetPhysicsBoundingBox();
		if (MBMath.IntersectLineSegmentWithBoundingBox(ref lineSegmentStart, ref lineSegmentEnd, ref physicsBoundingBox.min, ref physicsBoundingBox.max))
		{
			MatrixFrame sailGlobalFrame = _sailVisual.SailSkeletonEntity.GetGlobalFrame();
			if ((int)_sailObject.Type == 0)
			{
				Vec3 val = default(Vec3);
				((Vec3)(ref val))._002Ector(-0.5f, 0f, -0.5f, -1f);
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						Vec3 globalSailPoint = GetGlobalSailPoint(val + 0.125f * new Vec3((float)j, 0f, (float)i, -1f), in sailGlobalFrame);
						Vec3 globalSailPoint2 = GetGlobalSailPoint(val + 0.125f * new Vec3((float)(j + 1), 0f, (float)i, -1f), in sailGlobalFrame);
						Vec3 globalSailPoint3 = GetGlobalSailPoint(val + 0.125f * new Vec3((float)(j + 1), 0f, (float)(i + 1), -1f), in sailGlobalFrame);
						if (MBMath.IntersectLineSegmentWithTriangle(ref lineSegmentStart, ref lineSegmentEnd, ref globalSailPoint, ref globalSailPoint3, ref globalSailPoint2))
						{
							return true;
						}
						Vec3 globalSailPoint4 = GetGlobalSailPoint(val + 0.125f * new Vec3((float)j, 0f, (float)(i + 1), -1f), in sailGlobalFrame);
						if (MBMath.IntersectLineSegmentWithTriangle(ref lineSegmentStart, ref lineSegmentEnd, ref globalSailPoint, ref globalSailPoint4, ref globalSailPoint3))
						{
							return true;
						}
					}
				}
			}
			else
			{
				Vec3 val2 = default(Vec3);
				((Vec3)(ref val2))._002Ector(-0.5f, 0f, 0f, -1f);
				for (int k = 0; k < 4; k++)
				{
					int num = 9 - k * 2 - 1;
					for (int l = 0; l < num; l++)
					{
						if (l == num - 1)
						{
							Vec3 globalSailPoint5 = GetGlobalSailPoint(val2 + 0.125f * new Vec3((float)(l + k), 0f, (float)(-k), -1f), in sailGlobalFrame);
							Vec3 globalSailPoint6 = GetGlobalSailPoint(val2 + 0.125f * new Vec3((float)(l + k + 1), 0f, (float)(-k), -1f), in sailGlobalFrame);
							Vec3 globalSailPoint7 = GetGlobalSailPoint(val2 + 0.125f * new Vec3((float)(l + k), 0f, (float)(-k - 1), -1f), in sailGlobalFrame);
							if (MBMath.IntersectLineSegmentWithTriangle(ref lineSegmentStart, ref lineSegmentEnd, ref globalSailPoint5, ref globalSailPoint7, ref globalSailPoint6))
							{
								return true;
							}
							continue;
						}
						Vec3 globalSailPoint8 = GetGlobalSailPoint(val2 + 0.125f * new Vec3((float)(l + k), 0f, (float)(-k), -1f), in sailGlobalFrame);
						Vec3 globalSailPoint9 = GetGlobalSailPoint(val2 + 0.125f * new Vec3((float)(l + k + 1), 0f, (float)(-k), -1f), in sailGlobalFrame);
						Vec3 globalSailPoint10 = GetGlobalSailPoint(val2 + 0.125f * new Vec3((float)(l + k + 1), 0f, (float)(-k - 1), -1f), in sailGlobalFrame);
						if (MBMath.IntersectLineSegmentWithTriangle(ref lineSegmentStart, ref lineSegmentEnd, ref globalSailPoint8, ref globalSailPoint10, ref globalSailPoint9))
						{
							return true;
						}
						if (l > 0)
						{
							Vec3 globalSailPoint11 = GetGlobalSailPoint(val2 + 0.125f * new Vec3((float)(l + k), 0f, (float)(-k - 1), -1f), in sailGlobalFrame);
							if (MBMath.IntersectLineSegmentWithTriangle(ref lineSegmentStart, ref lineSegmentEnd, ref globalSailPoint8, ref globalSailPoint11, ref globalSailPoint10))
							{
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	private Vec3 GetGlobalSailPoint(Vec3 point, in MatrixFrame sailGlobalFrame)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = sailGlobalFrame;
		Vec3 scale = ((MatrixFrame)(ref val)).GetScale();
		float num = _width / scale.x;
		float num2 = _height / scale.z;
		point.x *= num;
		point.z *= num2;
		float num3 = MathF.Min(((int)_sailObject.Type == 0) ? (0.5f * num2 - point.z) : (0f - point.z), ((int)_sailObject.Type == 0) ? ((Vec3)(ref point)).Distance(new Vec3(((point.x > 0f) ? 0.5f : (-0.5f)) * num, 0f, -0.5f * num2, -1f)) : ((Vec3)(ref point)).Distance(new Vec3(0f, 0f, -0.5f * num2, -1f)));
		float val2 = (((int)_sailObject.Type == 0) ? (0.5f * num2 + ((point.z > 0f) ? (0f - point.z) : point.z)) : (0f - point.z));
		val2 = Math.Min(val2, ((int)_sailObject.Type == 0) ? (0.5f * num + ((point.x > 0f) ? (0f - point.x) : point.x)) : (0f - point.z));
		float num4 = MathF.Sqrt(num3 * (val2 + 0.4f) / (Math.Min(num2, num) * 0.5f + 0.4f));
		point.z += (1f - Setting) * (((int)_sailObject.Type == 0) ? (0.25f * num2 - point.z) : (0f - point.z));
		Vec2 asVec = ((Vec3)(ref _force.Force)).AsVec2;
		float num5 = ((Vec2)(ref asVec)).Normalize();
		Vec2 val3 = asVec * (MathF.Sqrt(num5) / 100f);
		val = sailGlobalFrame;
		Vec3 val4 = ((MatrixFrame)(ref val)).TransformToParent(ref point);
		Vec3 val5;
		Vec2 asVec2;
		if ((int)_sailObject.Type == 0)
		{
			val5 = sailGlobalFrame.rotation.s;
			asVec2 = ((Vec3)(ref val5)).AsVec2;
			Vec2 val6 = ((Vec2)(ref asVec2)).Normalized();
			val5 = sailGlobalFrame.rotation.f;
			asVec2 = ((Vec3)(ref val5)).AsVec2;
			Vec2 val7 = ((Vec2)(ref asVec2)).Normalized();
			float num6 = Math.Max(0f, Vec2.DotProduct(val7, val3));
			val4 += new Vec3(val6 * (Vec2.DotProduct(val6, val3) * 0.65f * num4), 0f, -1f);
			val4 += new Vec3(val7 * (num6 * 0.9f * num4), 0f, -1f);
			val4 += new Vec3(0f, 0f, (0.5f - point.z / num2) * 0.35f * num6 * 0.9f * num4, -1f);
			return val4 + (0.5f - point.z / num2) * sailGlobalFrame.rotation.f * 0.6f;
		}
		val5 = sailGlobalFrame.rotation.s;
		asVec2 = ((Vec3)(ref val5)).AsVec2;
		Vec2 val8 = -((Vec2)(ref asVec2)).Normalized();
		val5 = sailGlobalFrame.rotation.f;
		asVec2 = ((Vec3)(ref val5)).AsVec2;
		Vec2 val9 = -((Vec2)(ref asVec2)).Normalized();
		float num7 = Math.Max(0f, Vec2.DotProduct(val9, val3));
		val4 += new Vec3(val8 * (Vec2.DotProduct(val8, val3) * 0.1f * num4), 0f, -1f);
		val4 += new Vec3(val9 * (num7 * 0.7f * num4), 0f, -1f);
		val4 += new Vec3(0f, 0f, (0.5f - point.z / num2) * 0.1f * num7 * 0.7f * num4, -1f);
		return val4 + sailGlobalFrame.rotation.f * 0.25f;
	}

	public void OnSailHit(Agent attackerAgent, float rawDamage, float inflictedDamage)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		bool isHuman = attackerAgent.IsHuman;
		bool isMine = attackerAgent.IsMine;
		bool num = attackerAgent.RiderAgent != null;
		Agent riderAgent = attackerAgent.RiderAgent;
		CombatLogData val = default(CombatLogData);
		((CombatLogData)(ref val))._002Ector(false, isHuman, isMine, num, riderAgent != null && riderAgent.IsMine, attackerAgent.IsMount, false, false, false, false, false, false, (MissionObject)(object)this, false, false, false, 0f);
		val.InflictedFireDamage = (int)rawDamage;
		val.ModifiedFireDamage = MathF.Round(inflictedDamage - rawDamage);
		Mission.Current.AddCombatLogSafe(attackerAgent, (Agent)null, val);
	}

	public void StartShipCaptureAnimation(Texture newTexture)
	{
		_sailVisual.StartFlagCaptureAnimation(newTexture);
	}
}
