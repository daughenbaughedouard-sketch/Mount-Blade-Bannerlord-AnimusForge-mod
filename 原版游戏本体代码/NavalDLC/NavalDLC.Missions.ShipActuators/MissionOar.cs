using System;
using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.ShipActuators;

public class MissionOar
{
	private class OarFoamDecal
	{
		private Decal _splashFoamDecal;

		private MatrixFrame _currentFrame;

		private Vec3 _sideVectorStart;

		private Vec3 _sideVectorEnd;

		private float _cumulativeDtTillStart;

		private float _randomScale;

		private Vec3 _currentSpeed;

		private float _lifeTimeRandomness;

		internal OarFoamDecal()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			_splashFoamDecal = null;
			_currentFrame = MatrixFrame.Identity;
			_cumulativeDtTillStart = 0f;
			_randomScale = 1f;
			_currentSpeed = Vec3.Zero;
			_lifeTimeRandomness = 0f;
			_sideVectorStart = Vec3.Zero;
			_sideVectorEnd = Vec3.Zero;
		}

		internal void Tick(float dt, MissionShip ownerShip)
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			float num = 5.8f + _lifeTimeRandomness;
			if ((NativeObject)(object)_splashFoamDecal != (NativeObject)null && _cumulativeDtTillStart < num)
			{
				Vec3 val = new Vec3(0.65f, 1f, 1f, -1f);
				Vec3 val2 = val * 4.5f;
				_cumulativeDtTillStart += dt;
				float num3;
				if (_cumulativeDtTillStart > 1.05f)
				{
					float num2 = _cumulativeDtTillStart - 1.05f;
					num3 = MathF.Clamp(1f - num2 / (num - 1.05f), 0f, 1f);
				}
				else
				{
					num3 = MathF.Clamp(_cumulativeDtTillStart / 1.05f, 0f, 1f);
				}
				float num4 = MathF.Pow(num3, 4f);
				_splashFoamDecal.SetAlpha(num4 * 0.17499998f + 0.475f);
				_currentFrame.origin.z = ((ScriptComponentBehavior)ownerShip).Scene.GetWaterLevelAtPosition(((Vec3)(ref _currentFrame.origin)).AsVec2, true, false) - 0.15f;
				ref Vec3 origin = ref _currentFrame.origin;
				origin += _currentSpeed * dt;
				Vec3 currentSpeed = _currentSpeed;
				float num5 = ((Vec3)(ref currentSpeed)).Normalize();
				num5 = MathF.Max(num5 - dt * 0.5f, 0f);
				_currentSpeed = num5 * currentSpeed;
				float num6 = MathF.Clamp(_cumulativeDtTillStart / num, 0f, 1f);
				Vec3 val3 = Vec3.Lerp(val, val2, num6) * _randomScale;
				float num7 = MathF.Clamp(_cumulativeDtTillStart / num, 0f, 1f);
				Vec3 s = Vec3.Slerp(_sideVectorStart, _sideVectorEnd, num7);
				((Vec3)(ref s)).Normalize();
				_currentFrame.rotation.s = s;
				_currentFrame.rotation.u = Vec3.Up;
				_currentFrame.rotation.f = -((Vec3)(ref _currentFrame.rotation.s)).CrossProductWithUp();
				((Mat3)(ref _currentFrame.rotation)).ApplyScaleLocal(ref val3);
				_splashFoamDecal.Frame = _currentFrame;
			}
		}

		internal void Fill(in Vec3 spawnPosition, MissionShip ownerShip)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_0239: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_031b: Unknown result type (might be due to invalid IL or missing references)
			if ((NativeObject)(object)_splashFoamDecal == (NativeObject)null)
			{
				Decal val = Decal.CreateDecal((string)null);
				val.SetMaterial(Material.GetFromResource("decal_water_foam"));
				((ScriptComponentBehavior)ownerShip).Scene.AddDecalInstance(val, "editor_set", true);
				_splashFoamDecal = val;
			}
			MatrixFrame identity = MatrixFrame.Identity;
			identity.origin = spawnPosition;
			identity.rotation.u = Vec3.Up;
			Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)ownerShip).GameEntity, spawnPosition);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)ownerShip).GameEntity;
			Vec3 s = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation.s;
			Vec2 asVec = ((Vec3)(ref s)).AsVec2;
			((Vec2)(ref asVec)).Normalize();
			identity.rotation.s = new Vec3(asVec, 0f, -1f);
			identity.rotation.f = -((Vec3)(ref identity.rotation.s)).CrossProductWithUp();
			((Vec3)(ref identity.rotation.f)).Normalize();
			ref Vec3 origin = ref identity.origin;
			origin += (-0.5f + MBRandom.RandomFloat) * identity.rotation.f;
			ref Vec3 origin2 = ref identity.origin;
			origin2 += (-0.5f + MBRandom.RandomFloat) * identity.rotation.s;
			_cumulativeDtTillStart = 0f;
			MathF.Clamp((((Vec3)(ref linearVelocityAtGlobalPointForEntityWithDynamicBody)).Length - 4f) / 8f, 0f, 1f);
			float num = 1f;
			_randomScale = (0.7f + MBRandom.RandomFloat * 0.6f) * num;
			_splashFoamDecal.Frame = identity;
			_splashFoamDecal.SetAlpha(0f);
			_currentFrame = identity;
			int num2 = MBRandom.RandomInt(3);
			float num3 = (float)(num2 % 2) * 0.5f;
			float num4 = (float)(num2 / 2) * 0.5f;
			_splashFoamDecal.SetVectorArgument(num3, num4, -0.5f, -0.5f);
			float num5 = 0.1f * (-0.5f + MBRandom.RandomFloat) * 0.25f;
			float num6 = 0.2f * (0.9f + MBRandom.RandomFloat * 0.2f);
			_currentSpeed = linearVelocityAtGlobalPointForEntityWithDynamicBody * num6 + identity.rotation.s * ((Vec3)(ref linearVelocityAtGlobalPointForEntityWithDynamicBody)).Length * num5;
			_lifeTimeRandomness = (-0.5f + MBRandom.RandomFloat) * 2f;
			float num7 = MathF.PI * (2f * MBRandom.RandomFloat - 1f);
			float num8 = -0.34906584f * (0.8f + MBRandom.RandomFloat * 0.4f);
			_sideVectorStart = new Vec3(asVec, 0f, -1f);
			((Vec3)(ref _sideVectorStart)).RotateAboutZ(num7);
			_sideVectorEnd = _sideVectorStart;
			((Vec3)(ref _sideVectorEnd)).RotateAboutZ(num8);
			Vec2 val2 = default(Vec2);
			((Vec2)(ref val2))._002Ector(2.5f, 2.5f);
			_splashFoamDecal.OverrideRoadBoundaryP0(val2);
			Vec2 val3 = default(Vec2);
			((Vec2)(ref val3))._002Ector(MBRandom.RandomFloat, MBRandom.RandomFloat);
			_splashFoamDecal.OverrideRoadBoundaryP1(val3);
		}
	}

	private struct OarRollAnimKeyFrame
	{
		public float KeyProgress;

		public float RollAngleInRad;

		public OarRollAnimKeyFrame(float keyProgress, float rollAngleInRad)
		{
			KeyProgress = keyProgress;
			RollAngleInRad = rollAngleInRad;
		}
	}

	private static class OarRollAnimManager
	{
		private static readonly OarRollAnimKeyFrame[] rollAnim = new OarRollAnimKeyFrame[7]
		{
			new OarRollAnimKeyFrame(0f, -1.2217305f),
			new OarRollAnimKeyFrame(0.15f, MathF.PI / 10f),
			new OarRollAnimKeyFrame(0.25f, 0.34906584f),
			new OarRollAnimKeyFrame(0.5f, 0.34906584f),
			new OarRollAnimKeyFrame(0.7f, -0.6981317f),
			new OarRollAnimKeyFrame(0.73f, -1.2217305f),
			new OarRollAnimKeyFrame(1f, -1.2217305f)
		};

		private static readonly OarRollAnimKeyFrame[] rollAnim2 = new OarRollAnimKeyFrame[5]
		{
			new OarRollAnimKeyFrame(0f, MathF.PI * -13f / 36f),
			new OarRollAnimKeyFrame(0.25f, 0.34906584f),
			new OarRollAnimKeyFrame(0.5f, 0.34906584f),
			new OarRollAnimKeyFrame(0.7f, 0.34906584f),
			new OarRollAnimKeyFrame(1f, MathF.PI * -13f / 36f)
		};

		private static readonly OarRollAnimKeyFrame[] rollAnim3 = new OarRollAnimKeyFrame[6]
		{
			new OarRollAnimKeyFrame(0f, -1.2217305f),
			new OarRollAnimKeyFrame(0.25f, 0f),
			new OarRollAnimKeyFrame(0.5f, 0f),
			new OarRollAnimKeyFrame(0.75f, -0.6981317f),
			new OarRollAnimKeyFrame(0.88f, -MathF.PI / 3f),
			new OarRollAnimKeyFrame(1f, -1.2217305f)
		};

		private static readonly OarRollAnimKeyFrame[] rollAnim4 = new OarRollAnimKeyFrame[4]
		{
			new OarRollAnimKeyFrame(0f, MathF.PI * -13f / 36f),
			new OarRollAnimKeyFrame(0.27f, MathF.PI / 6f),
			new OarRollAnimKeyFrame(0.7f, MathF.PI / 6f),
			new OarRollAnimKeyFrame(1f, MathF.PI * -13f / 36f)
		};

		private static readonly OarRollAnimKeyFrame[] rollAnim5 = new OarRollAnimKeyFrame[6]
		{
			new OarRollAnimKeyFrame(0f, -0.34906584f),
			new OarRollAnimKeyFrame(0.25f, -0.6981317f),
			new OarRollAnimKeyFrame(0.27f, 0.17453292f),
			new OarRollAnimKeyFrame(0.7f, 0.34906584f),
			new OarRollAnimKeyFrame(0.77f, -0.43633232f),
			new OarRollAnimKeyFrame(1f, -0.34906584f)
		};

		private static readonly OarRollAnimKeyFrame[] rollAnim6 = new OarRollAnimKeyFrame[5]
		{
			new OarRollAnimKeyFrame(0f, MathF.PI * -13f / 36f),
			new OarRollAnimKeyFrame(0.15f, 0.34906584f),
			new OarRollAnimKeyFrame(0.5f, 0.34906584f),
			new OarRollAnimKeyFrame(0.55f, 0.34906584f),
			new OarRollAnimKeyFrame(1f, MathF.PI * -13f / 36f)
		};

		private static readonly OarRollAnimKeyFrame[] rollAnim7 = new OarRollAnimKeyFrame[3]
		{
			new OarRollAnimKeyFrame(0f, -1.4835298f),
			new OarRollAnimKeyFrame(0.5f, 0.61086524f),
			new OarRollAnimKeyFrame(1f, -1.4835298f)
		};

		public static readonly OarRollAnimKeyFrame[][] RollAnimations = new OarRollAnimKeyFrame[7][] { rollAnim, rollAnim2, rollAnim3, rollAnim4, rollAnim5, rollAnim6, rollAnim7 };
	}

	private const int NumberOfFoamDecals = 4;

	private float _phaseDelayForSlowDown;

	private float _phaseDelayOffset;

	private float _phaseDelayOffsetTimeScale;

	private float _visualVerticalBaseAngleOffset;

	private float _visualVerticalAngleMultiplier;

	private float _visualLateralAngleMultiplier;

	private float _visualOarConstantRollAngle;

	private float _visualOarRollAnimationAngleFactor;

	private int _visualOarRollAnimationIndex;

	private float _slowDownPhaseMultiplier;

	private float _slowDownPhaseDuration;

	private readonly OarFoamDecal[] _splashFoamDecals = new OarFoamDecal[4];

	private int _nextDecalIndexToUse;

	private Vec3 _bladeContact = Vec3.Invalid;

	private readonly Vec3 _oarGateOffset;

	private OarSidePhaseController _sidePhaseData;

	private float _timeLeftToCheckForCloseShipsForRetraction;

	private Vec3 _lastGlobalBladeContact;

	private ParticleSystem _oarWaterParticleSmall;

	private bool _wakeActive;

	private bool _decalSpawned;

	private MBFastRandom _oarEffectsRandom;

	private Scene _ownerSceneCached;

	public MissionShip OwnerShip { get; }

	public float VisualPhase { get; private set; }

	public Vec3 GateOffset => _oarGateOffset;

	public float Extraction { get; private set; }

	public bool IsRetracted => Extraction <= 0f;

	public bool IsExtracted => Extraction >= 1f;

	public bool IsUsed { get; private set; }

	public bool IsRetracting { get; private set; }

	public Vec3 BladeContact
	{
		get
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if (!((Vec3)(ref _bladeContact)).IsValid)
			{
				_bladeContact = ComputeBladeContactPosition();
			}
			return _bladeContact;
		}
	}

	public OarDeckParameters DeckParameters { get; private set; }

	public float ForceMultiplierFromUserAgent { get; private set; }

	public float NeededRevolutionRate => _sidePhaseData.NeededRevolutionRate;

	private MissionOar(MissionShip ownerShip, GameEntity entity, OarDeckParameters deckParameters, OarSidePhaseController phaseData)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		OwnerShip = ownerShip;
		DeckParameters = deckParameters;
		_ownerSceneCached = ((ScriptComponentBehavior)OwnerShip).Scene;
		MatrixFrame globalFrame = entity.GetGlobalFrame();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)OwnerShip).GameEntity;
		MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		MatrixFrame val = ((MatrixFrame)(ref globalFrame2)).TransformToLocal(ref globalFrame);
		_oarGateOffset = val.origin;
		_sidePhaseData = phaseData;
		VisualPhase = _sidePhaseData.VisualPhase;
		ReRandomizeVisualParameters(-1);
		_phaseDelayForSlowDown = 0f;
		Extraction = 1f;
		IsRetracting = false;
		IsUsed = true;
		_slowDownPhaseMultiplier = 1f;
		_slowDownPhaseDuration = 0f;
		_timeLeftToCheckForCloseShipsForRetraction = 0f;
		ForceMultiplierFromUserAgent = 1f;
		if (!_ownerSceneCached.IsEditorScene())
		{
			MatrixFrame identity = MatrixFrame.Identity;
			_oarWaterParticleSmall = ParticleSystem.CreateParticleSystemAttachedToEntity("psys_naval_oar_on_move_small", ((ScriptComponentBehavior)ownerShip).GameEntity, ref identity);
			_oarWaterParticleSmall.SetEnable(false);
		}
		for (int i = 0; i < 4; i++)
		{
			_splashFoamDecals[i] = new OarFoamDecal();
		}
	}

	private void ReRandomizeVisualParameters(int userAgentIndex)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		uint num = ((userAgentIndex < 0) ? ((uint)((_oarGateOffset.x + _oarGateOffset.y + _oarGateOffset.z) * 1000f)) : ((uint)userAgentIndex));
		_oarEffectsRandom = new MBFastRandom(num);
		_phaseDelayOffset = _oarEffectsRandom.NextFloatRanged(-10f, 10f) * (MathF.PI / 180f);
		_phaseDelayOffsetTimeScale = _oarEffectsRandom.NextFloatRanged(0.5f, 1.2f);
		_visualVerticalBaseAngleOffset = _oarEffectsRandom.NextFloatRanged(-MathF.PI / 120f, MathF.PI / 120f);
		_visualVerticalAngleMultiplier = _oarEffectsRandom.NextFloatRanged(1f, 1.1f);
		_visualLateralAngleMultiplier = _oarEffectsRandom.NextFloatRanged(0.95f, 1.01f);
		_visualOarConstantRollAngle = _oarEffectsRandom.NextFloatRanged(-MathF.PI / 60f, MathF.PI / 60f);
		_visualOarRollAnimationAngleFactor = _oarEffectsRandom.NextFloatRanged(0.7f, 1f);
		_visualOarRollAnimationIndex = _oarEffectsRandom.Next(OarRollAnimManager.RollAnimations.Length);
	}

	public void SetUsed(bool newIsUsed, int userAgentIndex)
	{
		if (IsUsed != newIsUsed)
		{
			SetRetractOars(IsUsed);
			IsUsed = newIsUsed;
			if (IsUsed)
			{
				ReRandomizeVisualParameters(userAgentIndex);
			}
		}
	}

	public void SetRetractOars(bool value)
	{
		IsRetracting = value;
	}

	public void SetSlowDownPhaseForDuration(float slowDownMultiplier, float slowDownDuration)
	{
		_slowDownPhaseMultiplier = slowDownMultiplier;
		_slowDownPhaseDuration = slowDownDuration;
	}

	public void OnParallelTick(float dt)
	{
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		float extraction = Extraction;
		if (IsRetracting)
		{
			extraction -= dt * DeckParameters.RetractionRate;
			extraction = MathF.Max(0f, extraction);
		}
		else
		{
			extraction += dt * DeckParameters.RetractionRate;
			extraction = MathF.Min(extraction, 1f);
		}
		Extraction = extraction;
		float num = 0f;
		if (!IsRetracted)
		{
			float currentTime = Mission.Current.CurrentTime;
			num = _phaseDelayOffset * MathF.Sin(currentTime * _phaseDelayOffsetTimeScale) * extraction;
		}
		float num2 = MBMath.WrapAngleSafe(_sidePhaseData.VisualPhase + num);
		if (_slowDownPhaseDuration > 0f || _phaseDelayForSlowDown != 0f)
		{
			_slowDownPhaseDuration -= dt;
			if (_slowDownPhaseDuration > 0f)
			{
				_phaseDelayForSlowDown -= _sidePhaseData.PhaseRate * dt * (1f - _slowDownPhaseMultiplier);
				_phaseDelayForSlowDown = MBMath.WrapAngleSafe(_phaseDelayForSlowDown);
			}
			else
			{
				_slowDownPhaseDuration = 0f;
				float phaseDelayForSlowDown = _phaseDelayForSlowDown;
				_phaseDelayForSlowDown += _sidePhaseData.PhaseRate * dt * (1f - _slowDownPhaseMultiplier);
				_phaseDelayForSlowDown = MBMath.WrapAngleSafe(_phaseDelayForSlowDown);
				if (phaseDelayForSlowDown * _phaseDelayForSlowDown <= 0f && MathF.Abs(phaseDelayForSlowDown) < MathF.PI / 2f && MathF.Abs(_phaseDelayForSlowDown) < MathF.PI / 2f)
				{
					_phaseDelayForSlowDown = 0f;
				}
			}
		}
		num2 += _phaseDelayForSlowDown;
		VisualPhase = MBMath.WrapAngleSafe(num2);
		TickFoamDecals(dt);
		MatrixFrame val = OwnerShip.GlobalFrame;
		Vec3 spawnPosition = ((MatrixFrame)(ref val)).TransformToParent(ref _bladeContact);
		if (IsExtracted && MBMath.IsBetweenInclusive(VisualPhase, -0.87266463f, 0.87266463f))
		{
			if (_wakeActive)
			{
				float lastSubmergedHeightFactorForActuators = _sidePhaseData.GetLastSubmergedHeightFactorForActuators();
				if (_sidePhaseData.CycleArcSizeMult > 0.5f && lastSubmergedHeightFactorForActuators > 0.01f)
				{
					float num3 = (1f - _sidePhaseData.LastSlowDownFactor * _sidePhaseData.LastSlowDownFactor * _sidePhaseData.LastSlowDownFactor + 0.4f) * 0.25f * dt * lastSubmergedHeightFactorForActuators;
					_ownerSceneCached.AddWaterWakeWithCapsule(_lastGlobalBladeContact, 0.90000004f, spawnPosition, num3, num3, 0f);
				}
				if (lastSubmergedHeightFactorForActuators > 0.01f && _sidePhaseData.CycleArcSizeMult > 0.5f && (MBMath.IsBetweenInclusive(VisualPhase, -0.87266463f, -MathF.PI / 6f) || MBMath.IsBetweenInclusive(VisualPhase, 0.17453295f, 0.87266463f)))
				{
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)OwnerShip).GameEntity;
					MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
					MatrixFrame identity = MatrixFrame.Identity;
					identity.rotation.s = globalFrame.rotation.s;
					if (GateOffset.x < 0f)
					{
						ref Vec3 s = ref identity.rotation.s;
						s *= -1f;
					}
					identity.rotation.s.z = 0f;
					((Vec3)(ref identity.rotation.s)).Normalize();
					identity.rotation.f = -((Vec3)(ref identity.rotation.s)).CrossProductWithUp();
					identity.origin = spawnPosition;
					identity.origin.z = _ownerSceneCached.GetWaterLevelAtPosition(((Vec3)(ref identity.origin)).AsVec2, true, false);
					ParticleSystem oarWaterParticleSmall = _oarWaterParticleSmall;
					val = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref identity);
					oarWaterParticleSmall.SetLocalFrame(ref val);
					_oarWaterParticleSmall.SetEnable(true);
					if (!_decalSpawned)
					{
						if (_oarEffectsRandom.NextFloat() > 0.4f)
						{
							SpawnNewDecal(in spawnPosition);
						}
						_decalSpawned = true;
					}
				}
				else
				{
					_oarWaterParticleSmall.SetEnable(false);
				}
			}
			_wakeActive = true;
		}
		else
		{
			_oarWaterParticleSmall.SetEnable(false);
			_wakeActive = false;
			_decalSpawned = false;
		}
		_lastGlobalBladeContact = spawnPosition;
	}

	private void SpawnNewDecal(in Vec3 spawnPosition)
	{
		_splashFoamDecals[_nextDecalIndexToUse].Fill(in spawnPosition, OwnerShip);
		_nextDecalIndexToUse = (_nextDecalIndexToUse + 1) % 4;
	}

	private void TickFoamDecals(float dt)
	{
		OarFoamDecal[] splashFoamDecals = _splashFoamDecals;
		for (int i = 0; i < splashFoamDecals.Length; i++)
		{
			splashFoamDecals[i].Tick(dt, OwnerShip);
		}
	}

	public void FixedUpdate(float fixedDt, in MatrixFrame shipGlobalFrame, MBList<(MissionShip ship, OarSidePhaseController.OarSide shipSide)> nearbyShips)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (!IsUsed)
		{
			IsRetracting = true;
			_timeLeftToCheckForCloseShipsForRetraction = 0f;
		}
		else
		{
			_timeLeftToCheckForCloseShipsForRetraction -= fixedDt;
			if (_timeLeftToCheckForCloseShipsForRetraction < 0f)
			{
				_timeLeftToCheckForCloseShipsForRetraction = _oarEffectsRandom.NextFloatRanged(0.15f, 0.2f);
				IsRetracting = false;
				MatrixFrame val = shipGlobalFrame;
				Vec3 val2 = ((MatrixFrame)(ref val)).TransformToParent(ref _oarGateOffset);
				foreach (var item2 in (List<(MissionShip, OarSidePhaseController.OarSide)>)(object)nearbyShips)
				{
					if (_sidePhaseData.Side == item2.Item2)
					{
						MissionShip item = item2.Item1;
						WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
						MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
						Vec3 localPoint = ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref val2);
						Vec3 closestPointToBoundingBox = item.Physics.GetClosestPointToBoundingBox(in localPoint);
						float num = DeckParameters.OarLength + DeckParameters.RetractionOffset;
						if (((Vec3)(ref closestPointToBoundingBox)).DistanceSquared(localPoint) < num * num)
						{
							IsRetracting = true;
							break;
						}
					}
				}
			}
		}
		_bladeContact = ComputeBladeContactPosition();
	}

	public Vec3 ComputeBladeContactPosition(bool ignoreRetraction = true)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		float retraction = (ignoreRetraction ? 1f : Extraction);
		return ComputeBladeContactPositionAux(in _oarGateOffset, DeckParameters, _sidePhaseData.Phase, retraction);
	}

	public Vec3 ComputeBladeVisualContactPosition(bool ignoreRetraction = true)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		float retraction = (ignoreRetraction ? 1f : Extraction);
		float verticalBaseAngleOffset = _sidePhaseData.VisualVerticalBaseAngleOffsetFromShipRoll + _visualVerticalBaseAngleOffset;
		float verticalAngleMultiplier = _sidePhaseData.CycleArcSizeMult * _visualVerticalAngleMultiplier;
		float visualLateralAngleMultiplier = _visualLateralAngleMultiplier;
		return ComputeBladeContactPositionAux(in _oarGateOffset, DeckParameters, VisualPhase, retraction, verticalBaseAngleOffset, verticalAngleMultiplier, visualLateralAngleMultiplier);
	}

	public static Vec3 ComputeBladeContactPositionAux(in Vec3 gateOffset, OarDeckParameters deckParameters, float phase = 0f, float retraction = 1f, float verticalBaseAngleOffset = 0f, float verticalAngleMultiplier = 1f, float lateralAngleMultiplier = 1f)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		int num = MathF.Sign(gateOffset.x);
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector((float)num * deckParameters.OarLength * retraction, 0f, 0f, -1f);
		float verticalRotationAngle = deckParameters.VerticalRotationAngle * verticalAngleMultiplier;
		float lateralRotationAngle = deckParameters.LateralRotationAngle * lateralAngleMultiplier;
		float num2 = (float)num * GetVerticalAngle(phase, deckParameters.VerticalBaseAngle + verticalBaseAngleOffset, verticalRotationAngle);
		float num3 = (float)num * GetLateralAngle(phase, deckParameters.LateralBaseAngle, lateralRotationAngle);
		float num4 = default(float);
		float num5 = default(float);
		MathF.SinCos(num2, ref num4, ref num5);
		val.z = (0f - val.x) * num4;
		val.x *= num5;
		MathF.SinCos(num3, ref num4, ref num5);
		val.y = val.x * num4;
		val.x *= num5;
		return gateOffset + val;
	}

	public Vec3 ComputeBladeContactVelocity(bool ignoreRetraction = false)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		float retraction = (ignoreRetraction ? 1f : Extraction);
		return ComputeBladeContactVelocityAux(DeckParameters, _sidePhaseData.Phase, _sidePhaseData.PhaseRate, retraction);
	}

	public static Vec3 ComputeBladeContactVelocityAux(OarDeckParameters deckParameters, float phase, float phaseRate, float retraction = 1f)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		float verticalAngle = GetVerticalAngle(phase, deckParameters.VerticalBaseAngle, deckParameters.VerticalRotationAngle);
		float lateralAngle = GetLateralAngle(phase, deckParameters.LateralBaseAngle, deckParameters.LateralRotationAngle);
		float num = MathF.Sin(0f - phase) * deckParameters.VerticalRotationAngle * phaseRate;
		float num2 = (0f - MathF.Cos(0f - phase)) * deckParameters.LateralRotationAngle * phaseRate;
		float num3 = MathF.Sin(verticalAngle);
		float num4 = MathF.Cos(verticalAngle);
		float num5 = MathF.Sin(lateralAngle);
		float num6 = MathF.Cos(lateralAngle);
		float num7 = retraction * deckParameters.OarLength;
		float num8 = (0f - num7) * num3 * num * num6 - num7 * num4 * num5 * num2;
		float num9 = (0f - num7) * num3 * num * num5 + num7 * num4 * num6 * num2;
		float num10 = (0f - num7) * num4 * num;
		return new Vec3(num8, num9, num10, -1f);
	}

	public static float GetVerticalAngle(float phase, float verticalBaseAngle, float verticalRotationAngle)
	{
		return verticalBaseAngle + MathF.Cos(0f - phase) * verticalRotationAngle;
	}

	public static float GetLateralAngle(float phase, float lateralBaseAngle, float lateralRotationAngle)
	{
		return lateralBaseAngle + MathF.Sin(0f - phase) * lateralRotationAngle;
	}

	public static MissionOar CreateShipOar(MissionShip ownerShip, GameEntity entity, OarDeckParameters deckParameters, OarSidePhaseController sidePhase)
	{
		return new MissionOar(ownerShip, entity, deckParameters, sidePhase);
	}

	public MatrixFrame ComputeOarEntityFrame(float dt, in MatrixFrame oarMachineLocalFrame, in MatrixFrame oarEntityLocalFrame, in MatrixFrame _oarExtractedEntitialFrame, in MatrixFrame _oarRetractedEntitialFrame, float _lastIdleTime, bool forUnmanned)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = ComputeBladeVisualContactPosition();
		MatrixFrame val2 = oarMachineLocalFrame;
		Vec3 val3 = ((MatrixFrame)(ref val2)).TransformToLocal(ref val);
		Mat3 val6;
		Quaternion val8;
		if (IsExtracted)
		{
			float currentTime = Mission.Current.CurrentTime;
			MatrixFrame val4 = _oarExtractedEntitialFrame;
			val4.rotation.f = val3 - val4.origin;
			((Mat3)(ref val4.rotation)).Orthonormalize();
			float num = _phaseDelayOffset * MathF.Sin(currentTime * _phaseDelayOffsetTimeScale);
			float num2 = ComputeOarRollAccordingToPhase(MBMath.WrapAngleSafe(num + VisualPhase)) * _visualOarRollAnimationAngleFactor + _visualOarConstantRollAngle;
			if (_sidePhaseData.Side == OarSidePhaseController.OarSide.Left)
			{
				num2 *= -1f;
			}
			((Mat3)(ref val4.rotation)).RotateAboutForward(num2);
			float num3 = currentTime - _lastIdleTime;
			if (num3 < 1.5f)
			{
				Quaternion val5 = ((Mat3)(ref val4.rotation)).ToQuaternion();
				val6 = _oarExtractedEntitialFrame.rotation;
				Quaternion val7 = ((Mat3)(ref val6)).ToQuaternion();
				val8 = Quaternion.Slerp(val7, val5, num3 / 1.5f);
				val4.rotation = ((Quaternion)(ref val8)).ToMat3();
				((Mat3)(ref val4.rotation)).Orthonormalize();
			}
			return val4;
		}
		Vec3 val10;
		Vec2 asVec2;
		if (IsRetracted)
		{
			MatrixFrame val9 = _oarRetractedEntitialFrame;
			if (forUnmanned)
			{
				Vec2 asVec = ((Vec3)(ref val9.origin)).AsVec2;
				val10 = _oarExtractedEntitialFrame.origin;
				Vec2 val11 = asVec - ((Vec3)(ref val10)).AsVec2;
				ref Vec3 origin = ref val9.origin;
				float z = _oarExtractedEntitialFrame.origin.z;
				float num4 = MathF.Sign(Vec2.DotProduct(((Vec3)(ref val9.rotation.f)).AsVec2, val11));
				float length = ((Vec2)(ref val11)).Length;
				asVec2 = ((Vec3)(ref val9.rotation.f)).AsVec2;
				origin.z = z + num4 * (length / ((Vec2)(ref asVec2)).Length) * val9.rotation.f.z;
			}
			else
			{
				ref Vec3 origin2 = ref val9.origin;
				float z2 = _oarExtractedEntitialFrame.origin.z;
				asVec2 = ((Vec3)(ref val9.origin)).AsVec2;
				val10 = _oarExtractedEntitialFrame.origin;
				float num5 = ((Vec2)(ref asVec2)).Distance(((Vec3)(ref val10)).AsVec2);
				asVec2 = ((Vec3)(ref val9.rotation.f)).AsVec2;
				origin2.z = z2 + num5 / ((Vec2)(ref asVec2)).Length * val9.rotation.f.z;
			}
			return val9;
		}
		val6 = oarEntityLocalFrame.rotation;
		Quaternion val12 = ((Mat3)(ref val6)).ToQuaternion();
		Quaternion val13;
		if (!IsRetracting)
		{
			val6 = _oarExtractedEntitialFrame.rotation;
			val13 = ((Mat3)(ref val6)).ToQuaternion();
		}
		else
		{
			val6 = _oarRetractedEntitialFrame.rotation;
			val13 = ((Mat3)(ref val6)).ToQuaternion();
		}
		Quaternion val14 = val13;
		val8 = Quaternion.Slerp(val14, val12, MathF.Pow(2f, dt * -3f));
		val6 = ((Quaternion)(ref val8)).ToMat3();
		val10 = Vec3.Lerp(_oarRetractedEntitialFrame.origin, _oarExtractedEntitialFrame.origin, Extraction);
		MatrixFrame val15 = new MatrixFrame(ref val6, ref val10);
		((Mat3)(ref val15.rotation)).Orthonormalize();
		if (forUnmanned)
		{
			Vec2 asVec3 = ((Vec3)(ref val15.origin)).AsVec2;
			val10 = _oarExtractedEntitialFrame.origin;
			Vec2 val16 = asVec3 - ((Vec3)(ref val10)).AsVec2;
			ref Vec3 origin3 = ref val15.origin;
			float z3 = _oarExtractedEntitialFrame.origin.z;
			float num6 = MathF.Sign(Vec2.DotProduct(((Vec3)(ref val15.rotation.f)).AsVec2, val16));
			float length2 = ((Vec2)(ref val16)).Length;
			asVec2 = ((Vec3)(ref val15.rotation.f)).AsVec2;
			origin3.z = z3 + num6 * (length2 / ((Vec2)(ref asVec2)).Length) * val15.rotation.f.z;
		}
		else
		{
			ref Vec3 origin4 = ref val15.origin;
			float z4 = _oarExtractedEntitialFrame.origin.z;
			asVec2 = ((Vec3)(ref val15.origin)).AsVec2;
			val10 = _oarExtractedEntitialFrame.origin;
			float num7 = ((Vec2)(ref asVec2)).Distance(((Vec3)(ref val10)).AsVec2);
			asVec2 = ((Vec3)(ref val15.rotation.f)).AsVec2;
			origin4.z = z4 + num7 / ((Vec2)(ref asVec2)).Length * val15.rotation.f.z;
		}
		return val15;
	}

	private float ComputeOarRollAccordingToPhase(float phase)
	{
		OarRollAnimKeyFrame[] array = OarRollAnimManager.RollAnimations[_visualOarRollAnimationIndex];
		float num = (phase + MathF.PI) / (MathF.PI * 2f);
		if (num >= 1f)
		{
			num -= 1f;
		}
		float result = 0f;
		for (int i = 0; i < array.Length - 1; i++)
		{
			OarRollAnimKeyFrame oarRollAnimKeyFrame = array[i];
			OarRollAnimKeyFrame oarRollAnimKeyFrame2 = array[i + 1];
			if (oarRollAnimKeyFrame.KeyProgress <= num && num < oarRollAnimKeyFrame2.KeyProgress)
			{
				float num2 = oarRollAnimKeyFrame2.KeyProgress - oarRollAnimKeyFrame.KeyProgress;
				float num3 = (num - oarRollAnimKeyFrame.KeyProgress) / num2;
				result = MathF.Lerp(oarRollAnimKeyFrame.RollAngleInRad, oarRollAnimKeyFrame2.RollAngleInRad, num3, 1E-05f);
				break;
			}
		}
		return result;
	}

	public void SetOarForceMultiplierFromUserAgent(float forceMultiplierFromUserAgent)
	{
		ForceMultiplierFromUserAgent = forceMultiplierFromUserAgent;
	}

	public void OnPilotAssignedDuringSpawn()
	{
		IsRetracting = false;
		Extraction = 1f;
	}

	public bool IsInRowingMotion()
	{
		return _sidePhaseData.IsInRowingMotion();
	}
}
