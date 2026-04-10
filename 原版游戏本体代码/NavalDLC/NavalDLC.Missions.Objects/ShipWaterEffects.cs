using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.NavalPhysics;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Missions.Objects;

[ScriptComponentParams("ship_visual_only", "ship_water_effects")]
public class ShipWaterEffects : ScriptComponentBehavior
{
	internal enum ParticleType
	{
		Movement,
		Splash
	}

	internal enum MovementParticleType
	{
		Small,
		Medium,
		Large
	}

	internal enum ShipHullHeightType
	{
		Small,
		Medium,
		Large
	}

	internal enum ResolutionScale
	{
		one,
		half,
		quarter,
		one_eight,
		one_sixteenth
	}

	private struct FloaterData
	{
		internal float HeightMin;

		internal float VerticalLength;

		internal float HorizontalArea;
	}

	private class WetnessDecalData
	{
		internal Decal Decal;

		internal Vec3 Normal;

		internal Vec3 LocalPosition;

		internal float CurrentAlpha;
	}

	private struct SliceSampleData
	{
		internal Vec3 localPosition;

		internal Vec3 limitingUpVector;
	}

	private class ParticleData
	{
		internal ParticleSystem MovementParticleSystem;

		internal MatrixFrame LocalFrame = MatrixFrame.Identity;

		internal Vec3 SurfaceNormal = Vec3.Zero;

		internal ParticleSystem CurrentSplashParticle;

		internal float SplashTimer;

		internal float LastSpawnTime;

		internal bool WasAboveWater = true;

		internal Vec3 SplashVelocity = Vec3.Zero;

		internal Vec3 SplashPosition = Vec3.Zero;

		internal float SplashWaterMultiplier;

		internal List<KeyValuePair<float, SliceSampleData>> PerSlicePositions;

		internal float Size;
	}

	private class SplashFoamDecal
	{
		internal Decal _splashFoamDecal;

		internal MatrixFrame _currentFrame;

		internal float _cumulativeDtTillStart;

		internal Vec3 _randomScale;

		internal Vec3 _currentSpeed;

		internal Vec3 _sideVectorStart;

		internal Vec3 _sideVectorEnd;

		internal bool _isLeft;

		internal SplashFoamDecal()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			_splashFoamDecal = null;
			_currentFrame = MatrixFrame.Identity;
			_sideVectorStart = Vec3.Zero;
			_sideVectorEnd = Vec3.Zero;
			_cumulativeDtTillStart = 0f;
			_randomScale = new Vec3(1f, 1f, 1f, -1f);
			_currentSpeed = Vec3.Zero;
			_isLeft = false;
		}
	}

	private const string FloaterHolderTag = "floater_volume_holder";

	private const string FloaterTag = "floater_volume";

	private const string BodyMeshTag = "body_mesh";

	private const string SplashEntityTag = "splash_particles";

	private const string MovementEntityTag = "movement_particles";

	private const string WaterDepthRenderMeshTag = "render_to_depth";

	private const float ParticleSliceHeightDx = 0.5f;

	private const int NumberOfSplashDecal = 50;

	private const float SmallSplashSoundEventMaxDistanceSquared = 400f;

	private static readonly Comparer<KeyValuePair<float, SliceSampleData>> _cacheCompareDelegate = Comparer<KeyValuePair<float, SliceSampleData>>.Create((KeyValuePair<float, SliceSampleData> x, KeyValuePair<float, SliceSampleData> y) => x.Key.CompareTo(y.Key));

	[EditableScriptComponentVariable(true, "Water Simulation Bounding Box")]
	private Vec3 _waterSimulationBoundingBox = Vec3.One;

	[EditableScriptComponentVariable(true, "Show Water Simulation Bounding Box")]
	private bool _showWaterSimulationBoundingBox;

	[EditableScriptComponentVariable(true, "Reset Water Simulation Bounding Box")]
	private SimpleButton _resetWaterSimulationBoundingBox = new SimpleButton();

	[EditableScriptComponentVariable(true, "Re-render Depth Texture")]
	private SimpleButton _reRenderDepthTexture = new SimpleButton();

	[EditableScriptComponentVariable(true, "Reset In-Hull Water")]
	private SimpleButton _resetInHullWater = new SimpleButton();

	[EditableScriptComponentVariable(true, "Show Hull Water Debug Panel")]
	private bool _showHullWaterDebugPanel;

	[EditableScriptComponentVariable(true, "Hull Water Simulation Resolution Scale")]
	private ResolutionScale _hullWaterResScale = ResolutionScale.half;

	[EditableScriptComponentVariable(true, "Hull Water Splash Water Multiplier")]
	private float _hullWaterSplashWaterMultiplier = 1.75f;

	[EditableScriptComponentVariable(true, "Hull Water Splash Point Initial Offset")]
	private float _hullWaterSplashPointInitialOffset = 0.5f;

	[EditableScriptComponentVariable(true, "Hull Water Splash Point Speed Multiplier")]
	private float _hullWaterSplashPointSpeedMultiplier = 1f;

	[EditableScriptComponentVariable(true, "Ship Hull Height Type")]
	private ShipHullHeightType _shipHullHeightType;

	[EditableScriptComponentVariable(true, "Movement Particle Height Offset")]
	private float _movementParticleHeightOffset = 0.34f;

	[EditableScriptComponentVariable(true, "Splash Particle Height Offset")]
	private float _splashParticleHeightOffset = 0.4f;

	[EditableScriptComponentVariable(true, "Movement Particle Surface Distance Offset")]
	private float _movementParticleSurfaceDistanceOffset = 0.7f;

	[EditableScriptComponentVariable(true, "Splash Particle Surface Distance Offset")]
	private float _splashParticleSurfaceDistanceOffset = 0.7f;

	[EditableScriptComponentVariable(true, "Movement Particle Type")]
	private MovementParticleType _movementParticleType;

	[EditableScriptComponentVariable(true, "Movement Particle Side Speed Vector")]
	private float _movementParticleSideSpeedVector = 0.5f;

	[EditableScriptComponentVariable(true, "Show Movement Particles")]
	private bool _showMovementParticles;

	[EditableScriptComponentVariable(true, "Show Splash Particles")]
	private bool _showSplashParticles;

	[EditableScriptComponentVariable(true, "Show Water Balance Plane")]
	private bool _showWaterBalancePlane;

	[EditableScriptComponentVariable(true, "Show Wetness Decal Values")]
	private bool _showWetnessDecalValues;

	[EditableScriptComponentVariable(true, "Force Wetness Decal To Full")]
	private bool _forceWetnessDecalsToFull;

	private UIntPtr _waterVisualRecord = UIntPtr.Zero;

	private GameEntity _movementParticleEntity;

	private GameEntity _splashParticleEntity;

	private readonly List<ParticleData> _movementParticles = new List<ParticleData>();

	private readonly List<ParticleData> _splashParticles = new List<ParticleData>();

	private readonly MBFastRandom _splashRandom = new MBFastRandom();

	private readonly List<WetnessDecalData> _wetnessDecals = new List<WetnessDecalData>();

	private MatrixFrame _previousShipFrame = MatrixFrame.Identity;

	private float _cumulativeDt;

	private bool _inCampaignMode;

	private Scene _ownerSceneCached;

	private int _smallSplashParticleIndex = -1;

	private int _mediumSplashParticleIndex = -1;

	private int _largeSplashParticleIndex = -1;

	private bool _hullLocalFramesSetForMission;

	private bool _wakeAndParticlesEnabled;

	private BoundingBox _bodyBB;

	private readonly SplashFoamDecal[] _splashFoamDecals = new SplashFoamDecal[50];

	private int _nextDecalToUse;

	private Vec3 _lastDecalLeftSpawnPosition = Vec3.Zero;

	private Vec3 _lastDecalRightSpawnPosition = Vec3.Zero;

	private float _nextDecalLeftSpawnMetersSq = 49f;

	private float _nextDecalRightSpawnMetersSq = 49f;

	private Vec3 _previousShipFrameForDecalSpawn = Vec3.Zero;

	private int _leftDecalParticleIndex = -1;

	private int _rightDecalParticleIndex = -1;

	public void DummyFunc()
	{
		Debug.Print(_showWaterSimulationBoundingBox.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_movementParticleHeightOffset.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_splashParticleHeightOffset.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_showMovementParticles.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_showSplashParticles.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_showHullWaterDebugPanel.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_hullWaterResScale.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_showWaterBalancePlane.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_movementParticleSideSpeedVector.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_showWetnessDecalValues.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_forceWetnessDecalsToFull.ToString(), 0, (DebugColor)12, 17592186044416uL);
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)6;
	}

	protected override void OnInit()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		_showMovementParticles = false;
		_showSplashParticles = false;
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		_movementParticleEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTag("movement_particles"));
		val = ((ScriptComponentBehavior)this).GameEntity;
		_splashParticleEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTag("splash_particles"));
		if (_splashParticleEntity != (GameEntity)null)
		{
			foreach (GameEntity item in _splashParticleEntity.GetChildren().ToList())
			{
				item.Remove(23);
			}
		}
		if (_movementParticleEntity != (GameEntity)null)
		{
			foreach (GameEntity item2 in _movementParticleEntity.GetChildren().ToList())
			{
				item2.Remove(23);
			}
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		_inCampaignMode = ((WeakGameEntity)(ref val)).Scene.GetName() == "Main_map";
		val = ((ScriptComponentBehavior)this).GameEntity;
		_ownerSceneCached = ((WeakGameEntity)(ref val)).Scene;
		FetchEntities();
		if (!_inCampaignMode)
		{
			if (_wakeAndParticlesEnabled)
			{
				float num = 0f;
				val = ((ScriptComponentBehavior)this).GameEntity;
				val = ((WeakGameEntity)(ref val)).Root;
				NavalDLC.Missions.NavalPhysics.NavalPhysics firstScriptOfType = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<NavalDLC.Missions.NavalPhysics.NavalPhysics>();
				if (firstScriptOfType != null)
				{
					num = firstScriptOfType.StabilitySubmergedHeightOfShip;
				}
				PlaceParticles(ParticleType.Splash, num + _splashParticleHeightOffset);
				PlaceParticles(ParticleType.Movement, num + _movementParticleHeightOffset);
				if (_waterVisualRecord == UIntPtr.Zero)
				{
					CheckWaterVisualRegistry();
				}
			}
			_largeSplashParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_naval_ship_water_splash_large");
			_mediumSplashParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_naval_ship_water_splash_mid");
			_smallSplashParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_naval_ship_water_splash_small");
			if (_ownerSceneCached.HasDecalRenderer())
			{
				for (int i = 0; i < 50; i++)
				{
					_splashFoamDecals[i] = new SplashFoamDecal();
				}
			}
			val = ((ScriptComponentBehavior)this).GameEntity;
			WeakGameEntity parent = ((WeakGameEntity)(ref val)).Parent;
			_wetnessDecals.Clear();
			val = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
			if (parent != (GameEntity)null && (NativeObject)(object)((WeakGameEntity)(ref parent)).Scene != (NativeObject)null)
			{
				WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref parent)).GetFirstChildEntityWithTag("wetness_decals");
				foreach (WeakGameEntity child in ((WeakGameEntity)(ref firstChildEntityWithTag)).GetChildren())
				{
					WeakGameEntity current = child;
					GameEntityComponent componentAtIndex = ((WeakGameEntity)(ref current)).GetComponentAtIndex(0, (ComponentType)7);
					Decal val2 = (Decal)(object)((componentAtIndex is Decal) ? componentAtIndex : null);
					if ((NativeObject)(object)val2 != (NativeObject)null)
					{
						WetnessDecalData wetnessDecalData = new WetnessDecalData();
						wetnessDecalData.Decal = val2;
						val2.CheckAndRegisterToDecalSet();
						wetnessDecalData.CurrentAlpha = 0f;
						ref Mat3 rotation = ref globalFrame.rotation;
						MatrixFrame val3 = ((WeakGameEntity)(ref current)).GetLocalFrame();
						Vec3 val4 = ((Vec3)(ref val3.rotation.u)).NormalizedCopy();
						wetnessDecalData.Normal = ((Mat3)(ref rotation)).TransformToLocal(ref val4);
						val3 = ((WeakGameEntity)(ref current)).GetGlobalFrame();
						wetnessDecalData.LocalPosition = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref val3.origin);
						_wetnessDecals.Add(wetnessDecalData);
					}
				}
			}
		}
		ComputeWakeCapsuleParameters();
		val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		_previousShipFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
	}

	protected override void OnTick(float dt)
	{
		if (!_inCampaignMode && _waterVisualRecord == UIntPtr.Zero && _wakeAndParticlesEnabled)
		{
			CheckWaterVisualRegistry();
			ComputeWakeCapsuleParameters();
		}
	}

	protected override void OnTickParallel(float dt)
	{
		OnMissionTick(dt);
	}

	protected override void OnRemoved(int removeReason)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (_waterVisualRecord != UIntPtr.Zero)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.DeRegisterShipVisual(_waterVisualRecord);
		}
		if (!((NativeObject)(object)_ownerSceneCached != (NativeObject)null))
		{
			return;
		}
		if (_ownerSceneCached.HasDecalRenderer())
		{
			SplashFoamDecal[] splashFoamDecals = _splashFoamDecals;
			foreach (SplashFoamDecal splashFoamDecal in splashFoamDecals)
			{
				if (splashFoamDecal != null && (NativeObject)(object)splashFoamDecal._splashFoamDecal != (NativeObject)null)
				{
					_ownerSceneCached.RemoveDecalInstance(splashFoamDecal._splashFoamDecal, "editor_set");
				}
			}
		}
		if ((NativeObject)(object)_ownerSceneCached != (NativeObject)null)
		{
			((NativeObject)_ownerSceneCached).ManualInvalidate();
			_ownerSceneCached = null;
		}
	}

	private void OnMissionTick(float dt)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (_waterVisualRecord == UIntPtr.Zero)
		{
			return;
		}
		_cumulativeDt += dt;
		if (!_inCampaignMode)
		{
			if (_wakeAndParticlesEnabled)
			{
				SnapMovementParticlePositionsToWater(dt);
				if (dt > 1E-06f)
				{
					CheckAndSpawnSplashes(dt);
				}
			}
			TickHullWater(dt, fromEditor: false);
			HandleWetnessDecals(dt);
			if (_ownerSceneCached.HasDecalRenderer())
			{
				HandleSplashFoamDecals(dt);
			}
		}
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		_previousShipFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
	}

	private GameEntity GetParticleParentEntity(ParticleType particleType)
	{
		return (GameEntity)(particleType switch
		{
			ParticleType.Splash => _splashParticleEntity, 
			ParticleType.Movement => _movementParticleEntity, 
			_ => null, 
		});
	}

	private List<ParticleData> GetParticleDataList(ParticleType particleType)
	{
		return particleType switch
		{
			ParticleType.Splash => _splashParticles, 
			ParticleType.Movement => _movementParticles, 
			_ => null, 
		};
	}

	private ParticleSystem CreateMovementParticle(GameEntity parentEntity, MatrixFrame localFrame)
	{
		return (ParticleSystem)(_movementParticleType switch
		{
			MovementParticleType.Small => ParticleSystem.CreateParticleSystemAttachedToEntity("psys_naval_ship_emit_on_move_small", parentEntity, ref localFrame), 
			MovementParticleType.Medium => ParticleSystem.CreateParticleSystemAttachedToEntity("psys_naval_ship_emit_on_move_mid", parentEntity, ref localFrame), 
			MovementParticleType.Large => ParticleSystem.CreateParticleSystemAttachedToEntity("psys_naval_ship_emit_on_move_large", parentEntity, ref localFrame), 
			_ => null, 
		});
	}

	private void RecomputeWaterSimulationBoundingBox()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		((WeakGameEntity)(ref val)).GetChildrenWithTagRecursive(list, "render_to_depth");
		BoundingBox val2 = default(BoundingBox);
		((BoundingBox)(ref val2)).RecomputeRadius();
		val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current = item;
			BoundingBox localBoundingBox = ((WeakGameEntity)(ref current)).GetLocalBoundingBox();
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref current)).GetGlobalFrame();
			((BoundingBox)(ref val2)).RelaxWithChildBoundingBox(localBoundingBox, ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref globalFrame2));
		}
		float num = MathF.Max(val2.max.x, val2.min.x);
		float num2 = MathF.Max(val2.max.y, val2.min.y);
		float num3 = MathF.Max(val2.max.z, val2.min.z);
		float num4 = 1f;
		switch (_hullWaterResScale)
		{
		case ResolutionScale.half:
			num4 = 0.5f;
			break;
		case ResolutionScale.quarter:
			num4 = 0.25f;
			break;
		case ResolutionScale.one_eight:
			num4 = 0.125f;
			break;
		case ResolutionScale.one_sixteenth:
			num4 = 0.0625f;
			break;
		}
		_waterSimulationBoundingBox = new Vec3(num, num2, num3, -1f) * 2f;
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).ChangeResolutionMultiplierOfWaterVisual(_waterVisualRecord, num4, ref _waterSimulationBoundingBox);
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).RefreshMeshesToRenderToHullWater(_waterVisualRecord, "render_to_depth");
	}

	private void FetchEntities()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_movementParticleEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("movement_particles"));
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_splashParticleEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("splash_particles"));
		if (_movementParticleEntity != (GameEntity)null)
		{
			GameEntity movementParticleEntity = _movementParticleEntity;
			movementParticleEntity.EntityFlags = (EntityFlags)(movementParticleEntity.EntityFlags | 0x20000);
		}
		else
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			_movementParticleEntity = GameEntity.CreateEmpty(((WeakGameEntity)(ref gameEntity)).Scene, true, true, true);
			_movementParticleEntity.Name = "movement_parent";
			_movementParticleEntity.AddTag("movement_particles");
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).AddChild(_movementParticleEntity.WeakEntity, false);
			MatrixFrame identity = MatrixFrame.Identity;
			_movementParticleEntity.SetFrame(ref identity, true);
		}
		if (_splashParticleEntity != (GameEntity)null)
		{
			GameEntity splashParticleEntity = _splashParticleEntity;
			splashParticleEntity.EntityFlags = (EntityFlags)(splashParticleEntity.EntityFlags | 0x20000);
		}
		else
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			_splashParticleEntity = GameEntity.CreateEmpty(((WeakGameEntity)(ref gameEntity)).Scene, true, true, true);
			_splashParticleEntity.Name = "movement_parent";
			_splashParticleEntity.AddTag("splash_particles");
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).AddChild(_splashParticleEntity.WeakEntity, false);
			MatrixFrame identity2 = MatrixFrame.Identity;
			_splashParticleEntity.SetFrame(ref identity2, true);
		}
		MatrixFrame identity3 = MatrixFrame.Identity;
		_movementParticleEntity.SetLocalFrame(ref identity3, true);
		_splashParticleEntity.SetLocalFrame(ref identity3, true);
	}

	private void ComputeWakeCapsuleParameters()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		if (_waterVisualRecord == UIntPtr.Zero)
		{
			return;
		}
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		WeakGameEntity firstChildEntityWithTagRecursive = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("body_mesh");
		if (((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).IsValid)
		{
			val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Root;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
			((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).ValidateBoundingBox();
			BoundingBox globalBoundingBox = ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).GetGlobalBoundingBox();
			_bodyBB = ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).GetLocalBoundingBox();
			float num = globalBoundingBox.radius + 1f;
			Vec3 center = globalBoundingBox.center;
			center.z = MBMath.Lerp(center.z, globalBoundingBox.min.z, 0.5f, 1E-05f);
			Vec3 val2 = -globalFrame.rotation.f;
			Vec3 f = globalFrame.rotation.f;
			Vec3 s = globalFrame.rotation.s;
			Vec3 val3 = -globalFrame.rotation.s;
			Vec3 val4 = center - val2 * num;
			Vec3 val5 = center - f * num;
			Vec3 val6 = center - s * num;
			Vec3 val7 = center - val3 * num;
			float num2 = 0f;
			float num3 = 0f;
			bool num4 = ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).RayHitEntity(val4, val2, num * 2f, ref num2);
			bool flag = ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).RayHitEntity(val5, f, num * 2f, ref num3);
			float num5 = 0f;
			float num6 = 0f;
			bool flag2 = ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).RayHitEntity(val6, s, num * 2f, ref num5);
			bool flag3 = ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).RayHitEntity(val7, val3, num * 2f, ref num6);
			if (num4 && flag && flag2 && flag3)
			{
				float num7 = ((Vec3)(ref center)).Distance(val4 + val2 * (num2 + 4.5f));
				float num8 = ((Vec3)(ref center)).Distance(val5 + f * num3);
				float num9 = ((Vec3)(ref center)).Distance(val6 + s * num5);
				float num10 = ((Vec3)(ref center)).Distance(val7 + val3 * num6);
				val = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref val)).SetVisualRecordWakeParams(_waterVisualRecord, new Vec3(num7, num8, num9, num10));
			}
		}
	}

	private bool RayCastToEntities(List<WeakGameEntity> rayCastEntities, Vec3 rayStart, Vec3 rayDirection, float maxLength, ref float resultLength, ref Vec3 surfaceNormal)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		resultLength = maxLength;
		foreach (WeakGameEntity rayCastEntity in rayCastEntities)
		{
			WeakGameEntity current = rayCastEntity;
			float num = maxLength;
			if (((WeakGameEntity)(ref current)).RayHitEntityWithNormal(rayStart, rayDirection, maxLength, ref surfaceNormal, ref num) && num < resultLength)
			{
				result = true;
				resultLength = num;
			}
		}
		return result;
	}

	private void PlaceParticles(ParticleType particleType, float waterLineHeight)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0982: Unknown result type (might be due to invalid IL or missing references)
		//IL_0987: Unknown result type (might be due to invalid IL or missing references)
		//IL_098e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0993: Unknown result type (might be due to invalid IL or missing references)
		//IL_099a: Unknown result type (might be due to invalid IL or missing references)
		//IL_099f: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_064b: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0652: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_067f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0683: Unknown result type (might be due to invalid IL or missing references)
		//IL_0685: Unknown result type (might be due to invalid IL or missing references)
		//IL_068a: Unknown result type (might be due to invalid IL or missing references)
		//IL_068f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06de: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0705: Unknown result type (might be due to invalid IL or missing references)
		//IL_070a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0718: Unknown result type (might be due to invalid IL or missing references)
		//IL_075f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0761: Unknown result type (might be due to invalid IL or missing references)
		//IL_091a: Unknown result type (might be due to invalid IL or missing references)
		//IL_091c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0772: Unknown result type (might be due to invalid IL or missing references)
		//IL_0774: Unknown result type (might be due to invalid IL or missing references)
		//IL_0783: Unknown result type (might be due to invalid IL or missing references)
		//IL_0788: Unknown result type (might be due to invalid IL or missing references)
		//IL_078a: Unknown result type (might be due to invalid IL or missing references)
		//IL_078f: Unknown result type (might be due to invalid IL or missing references)
		//IL_079b: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07de: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0810: Unknown result type (might be due to invalid IL or missing references)
		//IL_0815: Unknown result type (might be due to invalid IL or missing references)
		//IL_0817: Unknown result type (might be due to invalid IL or missing references)
		//IL_0821: Unknown result type (might be due to invalid IL or missing references)
		//IL_0826: Unknown result type (might be due to invalid IL or missing references)
		//IL_0828: Unknown result type (might be due to invalid IL or missing references)
		//IL_082a: Unknown result type (might be due to invalid IL or missing references)
		//IL_082c: Unknown result type (might be due to invalid IL or missing references)
		//IL_082e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0837: Unknown result type (might be due to invalid IL or missing references)
		//IL_083c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0841: Unknown result type (might be due to invalid IL or missing references)
		//IL_0843: Unknown result type (might be due to invalid IL or missing references)
		//IL_0870: Unknown result type (might be due to invalid IL or missing references)
		//IL_0872: Unknown result type (might be due to invalid IL or missing references)
		//IL_0874: Unknown result type (might be due to invalid IL or missing references)
		//IL_087e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0883: Unknown result type (might be due to invalid IL or missing references)
		//IL_0885: Unknown result type (might be due to invalid IL or missing references)
		//IL_0887: Unknown result type (might be due to invalid IL or missing references)
		//IL_0855: Unknown result type (might be due to invalid IL or missing references)
		//IL_0857: Unknown result type (might be due to invalid IL or missing references)
		//IL_0859: Unknown result type (might be due to invalid IL or missing references)
		//IL_0863: Unknown result type (might be due to invalid IL or missing references)
		//IL_0868: Unknown result type (might be due to invalid IL or missing references)
		//IL_086a: Unknown result type (might be due to invalid IL or missing references)
		//IL_086c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0889: Unknown result type (might be due to invalid IL or missing references)
		//IL_088b: Unknown result type (might be due to invalid IL or missing references)
		//IL_088d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0892: Unknown result type (might be due to invalid IL or missing references)
		GameEntity particleParentEntity = GetParticleParentEntity(particleType);
		if (particleParentEntity == (GameEntity)null)
		{
			return;
		}
		MatrixFrame globalFrame = particleParentEntity.GetGlobalFrame();
		List<ParticleData> particleDataList = GetParticleDataList(particleType);
		foreach (ParticleData item in particleDataList)
		{
			if ((NativeObject)(object)item.MovementParticleSystem != (NativeObject)null)
			{
				particleParentEntity.RemoveComponent((GameEntityComponent)(object)item.MovementParticleSystem);
			}
		}
		particleDataList.Clear();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity root = ((WeakGameEntity)(ref gameEntity)).Root;
		WeakGameEntity firstChildEntityWithTagRecursive = ((WeakGameEntity)(ref root)).GetFirstChildEntityWithTagRecursive("body_mesh");
		if (!((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).IsValid)
		{
			return;
		}
		MatrixFrame globalFrame2 = ((WeakGameEntity)(ref root)).GetGlobalFrame();
		GameEntityComponent componentAtIndex = ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).GetComponentAtIndex(0, (ComponentType)0);
		BoundingBox boundingBox = ((MetaMesh)((componentAtIndex is MetaMesh) ? componentAtIndex : null)).GetBoundingBox();
		float radius = boundingBox.radius;
		Vec3 center = boundingBox.center;
		center.z = waterLineHeight;
		Vec3 val = boundingBox.max - boundingBox.min;
		Vec3 val2 = val;
		float num = MathF.Min(MathF.Min(val2.x, val2.y), val2.z);
		if (num > 0f)
		{
			val2 /= num;
		}
		val2 = Vec3.Lerp(val2, Vec3.One, 0.5f);
		float num2 = ((particleType == ParticleType.Splash) ? _splashParticleSurfaceDistanceOffset : _movementParticleSurfaceDistanceOffset);
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		list.Add(firstChildEntityWithTagRecursive);
		float num3 = 0f;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity parent = ((WeakGameEntity)(ref gameEntity)).Parent;
		if (parent != (GameEntity)null)
		{
			foreach (WeakGameEntity child2 in ((WeakGameEntity)(ref parent)).GetChildren())
			{
				WeakGameEntity current2 = child2;
				if (((WeakGameEntity)(ref current2)).ChildCount <= 0)
				{
					continue;
				}
				WeakGameEntity child = ((WeakGameEntity)(ref current2)).GetChild(0);
				if (!((WeakGameEntity)(ref child)).HasTag("bow"))
				{
					continue;
				}
				foreach (WeakGameEntity child3 in ((WeakGameEntity)(ref child)).GetChildren())
				{
					WeakGameEntity current3 = child3;
					if (((WeakGameEntity)(ref current3)).IsVisibleIncludeParents())
					{
						MissionShipRam firstScriptOfType = ((WeakGameEntity)(ref current3)).GetFirstScriptOfType<MissionShipRam>();
						if (firstScriptOfType != null)
						{
							num3 = MathF.Max(firstScriptOfType.RamLength, num3);
						}
					}
				}
				break;
			}
		}
		float num4 = 0f;
		int num5 = 5;
		Vec3 rayDirection = default(Vec3);
		for (int i = 0; i < num5; i++)
		{
			float resultLength = 0f;
			Vec3 surfaceNormal = Vec3.Zero;
			Vec3 val3 = new Vec3(0f, 1f, 0f, -1f) * val.y;
			val3.z = waterLineHeight - 0.5f + (float)i * 0.2f;
			((Vec3)(ref rayDirection))._002Ector(0f, -1f, 0f, -1f);
			val3 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref val3);
			rayDirection = ((Mat3)(ref globalFrame2.rotation)).TransformToParent(ref rayDirection);
			((Vec3)(ref rayDirection)).Normalize();
			if (RayCastToEntities(list, val3, rayDirection, radius * 8f, ref resultLength, ref surfaceNormal))
			{
				num4 = MathF.Max(num4, val.y - resultLength);
			}
		}
		num4 += num3;
		float num6 = 0f;
		int num7 = 5;
		Vec3 rayDirection2 = default(Vec3);
		for (int j = 0; j < num7; j++)
		{
			float resultLength2 = 0f;
			Vec3 surfaceNormal2 = Vec3.Zero;
			Vec3 val4 = new Vec3(0f, -1f, 0f, -1f) * val.y;
			val4.z = waterLineHeight - 0.5f + (float)j * 0.2f;
			((Vec3)(ref rayDirection2))._002Ector(0f, 1f, 0f, -1f);
			val4 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref val4);
			rayDirection2 = ((Mat3)(ref globalFrame2.rotation)).TransformToParent(ref rayDirection2);
			((Vec3)(ref rayDirection2)).Normalize();
			if (RayCastToEntities(list, val4, rayDirection2, radius * 8f, ref resultLength2, ref surfaceNormal2))
			{
				num6 = MathF.Max(num6, val.y - resultLength2);
			}
		}
		int num8 = 0;
		float num9 = num4 + num6;
		float num10 = 1f;
		int num11 = (int)(val.y / 5.5f);
		if (particleType == ParticleType.Movement)
		{
			num8 = num11 * 2 + 1;
		}
		else
		{
			float num12 = num9 - 3f;
			num8 = (int)(num12 / num10);
			num10 = num12 / (float)num8;
			num8 *= 2;
		}
		int num13 = num8 / 2;
		int num14 = 0;
		int num15 = 0;
		Vec3 val5 = default(Vec3);
		Vec3 val6 = default(Vec3);
		for (int k = 0; k < num8; k++)
		{
			bool flag = false;
			bool flag2 = false;
			((Vec3)(ref val5))._002Ector(0f, 0f, 0f, -1f);
			if (particleType == ParticleType.Splash)
			{
				float num16 = ((k >= num13) ? (-1f) : 1f);
				int num17 = k % num13;
				float y = num4 - 1.5f - (float)num17 * num10;
				val5.x = val.x * 2f * num16;
				val5.y = y;
				val5.z = center.z;
				((Vec3)(ref val6))._002Ector(0f - num16, 0f, 0f, -1f);
			}
			else if (k == 0)
			{
				val5.x = 0f;
				val5.y = num4 + 4f;
				val5.z = center.z;
				((Vec3)(ref val6))._002Ector(0f, -1f, 0f, -1f);
			}
			else
			{
				float num18 = ((k - 1 >= num11) ? (-1f) : 1f);
				int num19 = (k - 1) % num11;
				float y2 = num4 - (0.7f + (float)num19) * 2.05f;
				val5.x = val.x * 2f * num18;
				val5.y = y2;
				val5.z = center.z;
				((Vec3)(ref val6))._002Ector(0f - num18, 0f, 0f, -1f);
				flag = num19 == num14 && num18 == -1f;
				flag2 = num19 == num15 && num18 == 1f;
			}
			Vec3 val7 = val5;
			val5 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref val5);
			val6 = ((Mat3)(ref globalFrame2.rotation)).TransformToParent(ref val6);
			((Vec3)(ref val6)).Normalize();
			float resultLength3 = 0f;
			Vec3 surfaceNormal3 = Vec3.Zero;
			int num20 = 5;
			bool flag3 = false;
			while (!flag3 && num20 > 0)
			{
				flag3 = RayCastToEntities(list, val5, val6, radius * 8f, ref resultLength3, ref surfaceNormal3);
				if (!flag3)
				{
					val5.z += 0.05f;
				}
				num20--;
			}
			if (flag3)
			{
				Vec3 val8 = -val6;
				if (particleType == ParticleType.Movement && k == 0)
				{
					resultLength3 -= num3;
				}
				val8.z = 0f;
				((Vec3)(ref val8)).Normalize();
				MatrixFrame identity = MatrixFrame.Identity;
				identity.origin = val5 + resultLength3 * val6 + val8 * num2;
				identity.rotation.s = val8;
				identity.rotation.u = Vec3.Up;
				identity.rotation.f = -((Vec3)(ref identity.rotation.s)).CrossProductWithUp();
				ParticleData particleData = new ParticleData();
				particleData.LocalFrame = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref identity);
				particleData.SurfaceNormal = ((Mat3)(ref globalFrame.rotation)).TransformToLocal(ref surfaceNormal3);
				if (particleType == ParticleType.Movement)
				{
					particleData.MovementParticleSystem = CreateMovementParticle(particleParentEntity, particleData.LocalFrame);
				}
				particleData.LastSpawnTime = 0f;
				if (flag)
				{
					_leftDecalParticleIndex = particleDataList.Count;
				}
				if (flag2)
				{
					_rightDecalParticleIndex = particleDataList.Count;
				}
				particleData.PerSlicePositions = new List<KeyValuePair<float, SliceSampleData>>();
				for (float num21 = boundingBox.min.z; num21 < boundingBox.max.z; num21 += 0.25f)
				{
					Vec3 val9 = val7;
					val9.z = num21;
					val9 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref val9);
					Vec3 surfaceNormal4 = Vec3.Zero;
					float resultLength4 = 0f;
					if (!RayCastToEntities(list, val9, val6, radius * 8f, ref resultLength4, ref surfaceNormal4))
					{
						continue;
					}
					Vec3 val10 = val9 + resultLength4 * val6 + surfaceNormal4 * num2;
					Vec3 localPosition = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref val10);
					Vec3 val11 = Vec3.Up;
					Vec3 zero = Vec3.Zero;
					Vec3 val12 = val8;
					float num22 = 0f;
					if (((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).RayHitEntity(val10, val11, 8f, ref num22))
					{
						Vec3 val13 = (Vec3.Up + val8) * 0.5f;
						zero = val8;
						val12 = val11;
						val11 = val13;
						Vec3 val16;
						do
						{
							float resultLength5 = 0f;
							Vec3 surfaceNormal5 = Vec3.Zero;
							if (!RayCastToEntities(list, val10, val11, 8f, ref resultLength5, ref surfaceNormal5))
							{
								Vec3 val14 = (val11 + val12) * 0.5f;
								zero = val11;
								val11 = val14;
							}
							else
							{
								Vec3 val15 = (val11 + zero) * 0.5f;
								val12 = val11;
								val11 = val15;
							}
							val16 = Vec3.CrossProduct(val12, val11);
						}
						while (!(MathF.Abs(MathF.Asin(((Vec3)(ref val16)).Length)) < MathF.PI / 60f));
					}
					Vec3 val17 = Vec3.CrossProduct(val8, val11);
					((Vec3)(ref val17)).Normalize();
					val11 = ((Vec3)(ref val11)).RotateAboutAnArbitraryVector(val17, -0.34906584f);
					Vec3 limitingUpVector = ((Mat3)(ref globalFrame.rotation)).TransformToLocal(ref val11);
					SliceSampleData value = new SliceSampleData
					{
						localPosition = localPosition,
						limitingUpVector = limitingUpVector
					};
					particleData.PerSlicePositions.Add(new KeyValuePair<float, SliceSampleData>(num21, value));
				}
				particleDataList.Add(particleData);
			}
			else
			{
				if (flag)
				{
					num14++;
				}
				if (flag2)
				{
					num15++;
				}
			}
		}
		if (particleType == ParticleType.Movement && _movementParticles.Count > 0)
		{
			_lastDecalLeftSpawnPosition = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref _movementParticles[0].LocalFrame.origin);
			_lastDecalRightSpawnPosition = _lastDecalLeftSpawnPosition;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			_previousShipFrameForDecalSpawn = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
		}
	}

	private float GetFloaterForceMultiplier()
	{
		if (MBObjectManager.Instance != null)
		{
			MBReadOnlyList<MissionShipObject> objects = MBObjectManager.Instance.GetObjects<MissionShipObject>((Func<MissionShipObject, bool>)delegate(MissionShipObject x)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				string prefab = x.Prefab;
				WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
				val = ((WeakGameEntity)(ref val)).Root;
				return prefab == ((WeakGameEntity)(ref val)).Name;
			});
			if (((List<MissionShipObject>)(object)objects).Count > 0)
			{
				return ((List<MissionShipObject>)(object)objects)[0].FloatingForceMultiplier;
			}
		}
		return 1f;
	}

	private float CalculateWaterBalancePoint()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity root = ((WeakGameEntity)(ref gameEntity)).Root;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref root)).GetGlobalFrame();
		WeakGameEntity firstChildEntityWithName = MBExtensions.GetFirstChildEntityWithName(root, "floater_volume_holder");
		if (!((WeakGameEntity)(ref firstChildEntityWithName)).IsValid)
		{
			return 0f;
		}
		float floaterForceMultiplier = GetFloaterForceMultiplier();
		List<FloaterData> list = new List<FloaterData>();
		float num = 1000f;
		float num2 = -1000f;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref firstChildEntityWithName)).GetChildren())
		{
			WeakGameEntity current = child;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref current)).GetGlobalFrame();
			MatrixFrame frame = ((WeakGameEntity)(ref current)).GetFrame();
			Vec3 val = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonUnit(ref globalFrame2.origin);
			Vec3 scaleVector = ((Mat3)(ref frame.rotation)).GetScaleVector();
			FloaterData item = new FloaterData
			{
				HeightMin = val.z,
				VerticalLength = scaleVector.z,
				HorizontalArea = scaleVector.x * scaleVector.y
			};
			list.Add(item);
			num = MathF.Min(num, item.HeightMin);
			num2 = MathF.Max(num2, item.HeightMin + item.VerticalLength);
		}
		float num3 = ((WeakGameEntity)(ref root)).Mass * 9.806f;
		float num4 = 0.01f;
		float num5;
		for (num5 = num; num2 > num5; num5 += num4)
		{
			float num6 = 0f;
			foreach (FloaterData item2 in list)
			{
				if (num5 > item2.HeightMin)
				{
					float num7 = MathF.Min(num5 - item2.HeightMin, item2.VerticalLength) * item2.HorizontalArea * 1020f * 9.806f * floaterForceMultiplier;
					num6 += num7;
				}
			}
			if (num6 > num3)
			{
				break;
			}
		}
		return num5;
	}

	private void CheckAndSpawnSplashes(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_052a: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_055c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec2 globalWindVelocityOfScene = ((WeakGameEntity)(ref gameEntity)).GetGlobalWindVelocityOfScene();
		((Vec2)(ref globalWindVelocityOfScene)).Normalize();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity root = ((WeakGameEntity)(ref gameEntity)).Root;
		((WeakGameEntity)(ref root)).GetGlobalFrame();
		GameEntity particleParentEntity = GetParticleParentEntity(ParticleType.Splash);
		MatrixFrame globalFrame = particleParentEntity.GetGlobalFrame();
		Vec3 origin = SoundManager.GetListenerFrame().origin;
		foreach (ParticleData splashParticle in _splashParticles)
		{
			if (splashParticle.SplashTimer > 0.001f)
			{
				splashParticle.SplashTimer -= dt;
				continue;
			}
			splashParticle.SplashTimer -= dt;
			if ((NativeObject)(object)splashParticle.CurrentSplashParticle != (NativeObject)null)
			{
				if (!splashParticle.CurrentSplashParticle.HasAliveParticles())
				{
					if (((GameEntityComponent)splashParticle.CurrentSplashParticle).GetEntity() == particleParentEntity)
					{
						particleParentEntity.RemoveComponent((GameEntityComponent)(object)splashParticle.CurrentSplashParticle);
					}
					splashParticle.CurrentSplashParticle = null;
				}
				continue;
			}
			_ = splashParticle.LocalFrame;
			Vec3 origin2 = Vec3.Zero;
			Vec3 zero = Vec3.Zero;
			Vec3 limitingVector = Vec3.Zero;
			Vec3 val = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref splashParticle.LocalFrame.origin);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			float waterLevelAtPosition = ((WeakGameEntity)(ref gameEntity)).GetWaterLevelAtPosition(((Vec3)(ref val)).AsVec2, true, false);
			val.z = waterLevelAtPosition;
			bool pointIsValid = false;
			origin2 = GetHeightCorrectedPosForSlice(splashParticle, ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref val).z, ref pointIsValid, ref limitingVector);
			if (!pointIsValid)
			{
				continue;
			}
			zero = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref origin2);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((WeakGameEntity)(ref gameEntity)).Root, zero);
			Vec3 waterSpeedAtPosition = _ownerSceneCached.GetWaterSpeedAtPosition(((Vec3)(ref origin2)).AsVec2, true);
			Vec3 val2 = (splashParticle.SurfaceNormal + splashParticle.LocalFrame.rotation.s) * 0.5f;
			Vec3 val3 = ((Mat3)(ref globalFrame.rotation)).TransformToParent(ref val2);
			Vec3 val4 = linearVelocityAtGlobalPointForEntityWithDynamicBody - waterSpeedAtPosition;
			float num = MathF.Max(0f - val4.z, 0f);
			float num2 = MathF.Max(Vec3.DotProduct(val3, val4), 0f);
			float num3 = num + num2;
			int num4 = -1;
			float num5 = 0f;
			splashParticle.WasAboveWater = false;
			bool flag = false;
			if (num3 > 8f)
			{
				num4 = _largeSplashParticleIndex;
				num5 = 3f;
			}
			else if (num3 > 5f)
			{
				num4 = _mediumSplashParticleIndex;
				num5 = 2f;
			}
			else
			{
				if (!(num3 > 2f))
				{
					continue;
				}
				num4 = _smallSplashParticleIndex;
				num5 = 1f;
				flag = num3 > 4f;
			}
			MatrixFrame localFrame = splashParticle.LocalFrame;
			localFrame.origin = origin2;
			ParticleSystem val5 = ParticleSystem.CreateParticleSystemAttachedToEntity(num4, particleParentEntity, ref localFrame);
			val5.SetDontRemoveFromEntity(true);
			splashParticle.CurrentSplashParticle = val5;
			splashParticle.LastSpawnTime = _cumulativeDt;
			splashParticle.SplashPosition = splashParticle.PerSlicePositions[splashParticle.PerSlicePositions.Count - 1].Value.localPosition;
			splashParticle.SplashVelocity = -splashParticle.LocalFrame.rotation.s;
			((Vec3)(ref splashParticle.SplashVelocity)).Normalize();
			splashParticle.SplashVelocity *= (0.75f + _splashRandom.NextFloat() * 0.5f) * 0.6f;
			splashParticle.SplashPosition -= splashParticle.LocalFrame.rotation.s * _hullWaterSplashPointInitialOffset;
			MatrixFrame val6 = ((MatrixFrame)(ref _previousShipFrame)).TransformToParent(ref localFrame);
			Vec3 val7 = linearVelocityAtGlobalPointForEntityWithDynamicBody;
			val7.z = MathF.Abs(val7.z);
			Vec3 val8 = ((Mat3)(ref globalFrame.rotation)).TransformToParent(ref limitingVector);
			val8.z = 0f;
			((Vec3)(ref val8)).Normalize();
			float num6 = MathF.Clamp(num3, 3f, 20f);
			if (num4 == _smallSplashParticleIndex)
			{
				num3 *= 1.35f;
			}
			float num7 = num / num3;
			float num8 = num2 / num3;
			Vec3 val9 = (num7 * 0.75f + 0.25f) * Vec3.Up + val8 * (num8 * 0.75f + 0.25f);
			((Vec3)(ref val9)).Normalize();
			float num9 = MathF.Clamp((num6 - 2f) / 8f, 0.01f, 1f);
			float num10 = MathF.Lerp(3.5f, 4.5f, num9, 1E-05f);
			Vec3 val10 = val9 * num6 * num10;
			Vec3 val11 = Vec3.CrossProduct(splashParticle.LocalFrame.rotation.s, limitingVector);
			if (((Vec3)(ref val11)).LengthSquared > 0f)
			{
				((Vec3)(ref val11)).Normalize();
				Vec3 val12 = ((Mat3)(ref globalFrame.rotation)).TransformToLocal(ref val10);
				Vec3 val13 = Vec3.DotProduct(val12, val11) * val11;
				Vec3 val14 = val12 - val13;
				Vec3 val15 = Vec3.CrossProduct(val14, limitingVector);
				if (((Vec3)(ref val15)).LengthSquared > 0f && Vec3.DotProduct(val15, val11) < 0f)
				{
					val14 = limitingVector * ((Vec3)(ref val14)).Length;
					val12 = val14 + val13;
					val10 = ((Mat3)(ref globalFrame.rotation)).TransformToParent(ref val12);
				}
			}
			val6.origin = zero - val10 * dt;
			val5.SetPreviousGlobalFrame(ref val6);
			splashParticle.SplashTimer = num5 * 0.5f;
			if (flag && ((Vec3)(ref origin)).DistanceSquared(val6.origin) < 400f)
			{
				SoundManager.StartOneShotEvent("event:/mission/ambient/special/wash_splash_small", ref val6.origin);
			}
			splashParticle.Size = num5;
			if (_splashRandom.NextFloat() < 0.5f * num5)
			{
				splashParticle.SplashWaterMultiplier = (0.5f + 0.5f * _splashRandom.NextFloat()) * 0.53f * num5;
			}
			else
			{
				splashParticle.SplashWaterMultiplier = 0f;
			}
		}
	}

	private void SnapMovementParticlePositionsToWater(float dt)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		float num = 1.5f;
		if (_movementParticleType == MovementParticleType.Small)
		{
			num = 1f;
		}
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		bool flag = true;
		foreach (ParticleData movementParticle in _movementParticles)
		{
			if ((NativeObject)(object)movementParticle.MovementParticleSystem == (NativeObject)null)
			{
				continue;
			}
			Vec3 val2 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref movementParticle.LocalFrame.origin);
			val = ((ScriptComponentBehavior)this).GameEntity;
			float z = ((WeakGameEntity)(ref val)).GetWaterLevelAtPosition(((Vec3)(ref val2)).AsVec2, true, false) + _movementParticleHeightOffset;
			Vec3 val3 = val2;
			val3.z = z;
			float z2 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref val3).z;
			bool pointIsValid = false;
			Vec3 limitingVector = Vec3.Zero;
			Vec3 heightCorrectedPosForSlice = GetHeightCorrectedPosForSlice(movementParticle, z2, ref pointIsValid, ref limitingVector);
			if (pointIsValid)
			{
				movementParticle.MovementParticleSystem.SetEnable(true);
				MatrixFrame localFrame = movementParticle.LocalFrame;
				if (!flag)
				{
					localFrame.origin = heightCorrectedPosForSlice;
				}
				movementParticle.MovementParticleSystem.SetLocalFrame(ref localFrame);
				float runtimeEmissionRateMultiplier = 1f;
				MatrixFrame val4 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref localFrame);
				val = ((ScriptComponentBehavior)this).GameEntity;
				Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((WeakGameEntity)(ref val)).Root, val4.origin);
				MatrixFrame identity = MatrixFrame.Identity;
				float length = ((Vec3)(ref linearVelocityAtGlobalPointForEntityWithDynamicBody)).Length;
				Vec3 val5 = linearVelocityAtGlobalPointForEntityWithDynamicBody;
				val5.z = 0f;
				identity.origin = val4.origin - val5 * dt * 0.35f * num;
				Vec3 val6 = ((Mat3)(ref globalFrame.rotation)).TransformToParent(ref limitingVector);
				((Vec3)(ref val6)).Normalize();
				ref Vec3 origin = ref identity.origin;
				origin -= length * val6 * 0.06f * dt;
				if (!flag)
				{
					ref Vec3 origin2 = ref identity.origin;
					origin2 -= length * val4.rotation.s * 0.25f * dt * num;
				}
				movementParticle.MovementParticleSystem.SetPreviousGlobalFrame(ref identity);
				flag = false;
				movementParticle.MovementParticleSystem.SetRuntimeEmissionRateMultiplier(runtimeEmissionRateMultiplier);
			}
			else
			{
				movementParticle.MovementParticleSystem.SetEnable(false);
			}
		}
	}

	private void TickHullWater(float dt, bool fromEditor)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetWaterVisualRecordFrameAndDt(_waterVisualRecord, globalFrame, dt);
		if (fromEditor)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).UpdateHullWaterEffectFrames(_waterVisualRecord);
		}
		else if (!_hullLocalFramesSetForMission)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).UpdateHullWaterEffectFrames(_waterVisualRecord);
			_hullLocalFramesSetForMission = true;
		}
		MatrixFrame globalFrame2 = GetParticleParentEntity(ParticleType.Splash).GetGlobalFrame();
		float num = 0.1f;
		foreach (ParticleData splashParticle in _splashParticles)
		{
			float num2 = 0.4f * splashParticle.Size;
			float num3 = 1f;
			if (_shipHullHeightType == ShipHullHeightType.Large)
			{
				if (splashParticle.Size == 1f)
				{
					num3 = 0.5f;
				}
				else if (splashParticle.Size == 0f)
				{
					num3 = 0f;
				}
			}
			if (num3 > 0f)
			{
				if (_cumulativeDt - splashParticle.LastSpawnTime > num && _cumulativeDt - splashParticle.LastSpawnTime < num2)
				{
					splashParticle.SplashPosition += splashParticle.SplashVelocity * dt * _hullWaterSplashPointSpeedMultiplier;
					Vec3 val = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref splashParticle.SplashPosition);
					val.z = 1f;
					val.w = splashParticle.SplashWaterMultiplier * _hullWaterSplashWaterMultiplier * 2.75f * num3;
					gameEntity = ((ScriptComponentBehavior)this).GameEntity;
					((WeakGameEntity)(ref gameEntity)).AddSplashPositionToWaterVisualRecord(_waterVisualRecord, val);
				}
				else
				{
					splashParticle.SplashPosition += splashParticle.SplashVelocity * dt;
				}
			}
		}
		if (_ownerSceneCached.GetFallDensity() > 0.5f)
		{
			float num4 = MathF.Clamp(0.016f / dt, 0f, 1f) * 0.9f;
			int num5 = 13;
			for (int i = 0; i < num5; i++)
			{
				Vec3 val2 = _bodyBB.max - _bodyBB.min;
				Vec3 min = _bodyBB.min;
				min.x += val2.x * _splashRandom.NextFloatRanged(0.1f, 0.9f);
				min.y += val2.y * _splashRandom.NextFloatRanged(0.1f, 0.9f);
				Vec3 val3 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref min);
				val3.w = _splashRandom.NextFloatRanged(3.25f, 10.65f) * num4;
				val3.z = _splashRandom.NextFloatRanged(0.05f, 0.07f);
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).AddSplashPositionToWaterVisualRecord(_waterVisualRecord, val3);
			}
			if ((float)_splashRandom.Next() > 0.2f)
			{
				Vec3 val4 = _bodyBB.max - _bodyBB.min;
				Vec3 min2 = _bodyBB.min;
				min2.x += val4.x * _splashRandom.NextFloatRanged(0.1f, 0.9f);
				min2.y += val4.y * _splashRandom.NextFloatRanged(0.1f, 0.9f);
				Vec3 val5 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref min2);
				val5.w = _splashRandom.NextFloatRanged(1.05f, 2.05f) * num4;
				val5.z = _splashRandom.NextFloatRanged(0.45f, 0.85f);
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).AddSplashPositionToWaterVisualRecord(_waterVisualRecord, val5);
			}
		}
	}

	private Vec3 GetHeightCorrectedPosForSlice(ParticleData particleData, float height, ref bool pointIsValid, ref Vec3 limitingVector)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		int num = particleData.PerSlicePositions.BinarySearch(new KeyValuePair<float, SliceSampleData>(height, default(SliceSampleData)), _cacheCompareDelegate);
		if (num >= 0)
		{
			pointIsValid = true;
			limitingVector = particleData.PerSlicePositions[num].Value.limitingUpVector;
			return particleData.PerSlicePositions[num].Value.localPosition;
		}
		int num2 = ~num;
		if (num2 > 0 && num2 < particleData.PerSlicePositions.Count)
		{
			int index = num2 - 1;
			KeyValuePair<float, SliceSampleData> keyValuePair = particleData.PerSlicePositions[index];
			KeyValuePair<float, SliceSampleData> keyValuePair2 = particleData.PerSlicePositions[num2];
			float num3 = (height - keyValuePair.Key) / (keyValuePair2.Key - keyValuePair.Key);
			pointIsValid = true;
			limitingVector = Vec3.Lerp(keyValuePair.Value.limitingUpVector, keyValuePair2.Value.limitingUpVector, num3);
			return Vec3.Lerp(keyValuePair.Value.localPosition, keyValuePair2.Value.localPosition, num3);
		}
		pointIsValid = false;
		return Vec3.Zero;
	}

	private void CheckWaterVisualRegistry()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_waterVisualRecord = ((WeakGameEntity)(ref gameEntity)).Scene.RegisterShipVisualToWaterRenderer(((ScriptComponentBehavior)this).GameEntity, ref _waterSimulationBoundingBox);
		if (_waterVisualRecord != UIntPtr.Zero)
		{
			float num = 1f;
			switch (_hullWaterResScale)
			{
			case ResolutionScale.half:
				num = 0.5f;
				break;
			case ResolutionScale.quarter:
				num = 0.25f;
				break;
			case ResolutionScale.one_eight:
				num = 0.125f;
				break;
			case ResolutionScale.one_sixteenth:
				num = 0.0625f;
				break;
			}
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).ChangeResolutionMultiplierOfWaterVisual(_waterVisualRecord, num, ref _waterSimulationBoundingBox);
			SetMeshesToRenderForInHullWater();
		}
	}

	private void SetMeshesToRenderForInHullWater()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).RefreshMeshesToRenderToHullWater(_waterVisualRecord, "render_to_depth");
	}

	public void EnableWakeAndParticles()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!_wakeAndParticlesEnabled)
		{
			float num = 0f;
			WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Root;
			NavalDLC.Missions.NavalPhysics.NavalPhysics firstScriptOfType = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<NavalDLC.Missions.NavalPhysics.NavalPhysics>();
			if (firstScriptOfType != null)
			{
				num = firstScriptOfType.StabilitySubmergedHeightOfShip;
			}
			PlaceParticles(ParticleType.Splash, num + _splashParticleHeightOffset);
			PlaceParticles(ParticleType.Movement, num + _movementParticleHeightOffset);
		}
		_wakeAndParticlesEnabled = true;
	}

	public void DeregisterWaterMeshMaterials()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (_waterVisualRecord != UIntPtr.Zero)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).DeRegisterWaterMeshMaterials(_waterVisualRecord);
		}
	}

	private void HandleSplashFoamDecals(float dt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c23: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c24: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0804: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Unknown result type (might be due to invalid IL or missing references)
		//IL_080b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0810: Unknown result type (might be due to invalid IL or missing references)
		//IL_0816: Unknown result type (might be due to invalid IL or missing references)
		//IL_081b: Unknown result type (might be due to invalid IL or missing references)
		//IL_081d: Unknown result type (might be due to invalid IL or missing references)
		//IL_081e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0823: Unknown result type (might be due to invalid IL or missing references)
		//IL_0828: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0902: Unknown result type (might be due to invalid IL or missing references)
		//IL_0910: Unknown result type (might be due to invalid IL or missing references)
		//IL_0915: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0544: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0621: Unknown result type (might be due to invalid IL or missing references)
		//IL_0637: Unknown result type (might be due to invalid IL or missing references)
		//IL_063e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0643: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_069a: Unknown result type (might be due to invalid IL or missing references)
		//IL_070a: Unknown result type (might be due to invalid IL or missing references)
		//IL_070e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0713: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_072d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0732: Unknown result type (might be due to invalid IL or missing references)
		//IL_0737: Unknown result type (might be due to invalid IL or missing references)
		//IL_0757: Unknown result type (might be due to invalid IL or missing references)
		//IL_0759: Unknown result type (might be due to invalid IL or missing references)
		//IL_0773: Unknown result type (might be due to invalid IL or missing references)
		//IL_0778: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0953: Unknown result type (might be due to invalid IL or missing references)
		//IL_0958: Unknown result type (might be due to invalid IL or missing references)
		//IL_095a: Unknown result type (might be due to invalid IL or missing references)
		//IL_095f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0967: Unknown result type (might be due to invalid IL or missing references)
		//IL_096c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0978: Unknown result type (might be due to invalid IL or missing references)
		//IL_097d: Unknown result type (might be due to invalid IL or missing references)
		//IL_098b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0990: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_09af: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a01: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a06: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a70: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a77: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b10: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b15: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b17: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b28: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b92: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c03: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		if (_movementParticles.Count == 0)
		{
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(1.564f, 1.428f, 2f, -1f);
		Vec3 val2 = default(Vec3);
		((Vec3)(ref val2))._002Ector(val.x * 17.5f, val.y * 17.5f, val.z, -1f);
		SplashFoamDecal[] splashFoamDecals = _splashFoamDecals;
		foreach (SplashFoamDecal splashFoamDecal in splashFoamDecals)
		{
			float num = 11.5f;
			if (_movementParticleType == MovementParticleType.Large)
			{
				num += 3f;
			}
			else if (_movementParticleType == MovementParticleType.Medium)
			{
				num += 1.5f;
			}
			float num2 = num - 0.75f;
			if ((NativeObject)(object)splashFoamDecal._splashFoamDecal != (NativeObject)null && splashFoamDecal._cumulativeDtTillStart < num)
			{
				splashFoamDecal._cumulativeDtTillStart += dt;
				if (splashFoamDecal._cumulativeDtTillStart > 0.75f)
				{
					float num3 = splashFoamDecal._cumulativeDtTillStart - 0.75f;
					float num4 = MathF.Clamp(1f - num3 / num2, 0f, 1f);
					float num5 = 4f;
					float num6 = 0.475f;
					float alpha = MathF.Pow(num4, num5) * (0.95f - num6) + num6;
					splashFoamDecal._splashFoamDecal.SetAlpha(alpha);
				}
				else
				{
					float num7 = MathF.Clamp(splashFoamDecal._cumulativeDtTillStart / 0.75f, 0f, 1f);
					float num8 = 4f;
					float num9 = 0.475f;
					float alpha2 = (1f - MathF.Pow(1f - num7, num8)) * (0.95f - num9) + num9;
					splashFoamDecal._splashFoamDecal.SetAlpha(alpha2);
				}
				ref Vec3 origin = ref splashFoamDecal._currentFrame.origin;
				origin += splashFoamDecal._currentSpeed * dt;
				splashFoamDecal._currentFrame.origin.z = _ownerSceneCached.GetWaterLevelAtPosition(((Vec3)(ref splashFoamDecal._currentFrame.origin)).AsVec2, true, false) - 0.15f;
				Vec3 currentSpeed = splashFoamDecal._currentSpeed;
				float num10 = ((Vec3)(ref currentSpeed)).Normalize();
				num10 = MathF.Max(num10 - dt * 2.5f, 0f);
				splashFoamDecal._currentSpeed = num10 * currentSpeed;
				float num11 = MathF.Clamp(splashFoamDecal._cumulativeDtTillStart / num, 0f, 1f);
				Vec3 val3 = Vec3.Lerp(val, val2, num11);
				val3.x *= splashFoamDecal._randomScale.x;
				val3.y *= splashFoamDecal._randomScale.y;
				val3.z *= splashFoamDecal._randomScale.z;
				float num12 = num;
				float num13 = MathF.Clamp(splashFoamDecal._cumulativeDtTillStart / num12, 0f, 1f);
				Vec3 s = Vec3.Slerp(splashFoamDecal._sideVectorStart, splashFoamDecal._sideVectorEnd, num13);
				((Vec3)(ref s)).Normalize();
				splashFoamDecal._currentFrame.rotation.s = s;
				splashFoamDecal._currentFrame.rotation.u = Vec3.Up;
				splashFoamDecal._currentFrame.rotation.f = -((Vec3)(ref splashFoamDecal._currentFrame.rotation.s)).CrossProductWithUp();
				((Mat3)(ref splashFoamDecal._currentFrame.rotation)).ApplyScaleLocal(ref val3);
				splashFoamDecal._splashFoamDecal.Frame = splashFoamDecal._currentFrame;
			}
			else if ((NativeObject)(object)splashFoamDecal._splashFoamDecal != (NativeObject)null)
			{
				splashFoamDecal._splashFoamDecal.SetIsVisible(false);
			}
		}
		Vec3 val4 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref _movementParticles[0].LocalFrame.origin);
		float num14 = ((Vec3)(ref _lastDecalLeftSpawnPosition)).DistanceSquared(val4);
		if (_nextDecalLeftSpawnMetersSq < num14)
		{
			Vec3 val5 = (globalFrame.origin - _previousShipFrameForDecalSpawn) / dt;
			Vec3 s2 = globalFrame.rotation.s;
			s2.z = 0f;
			((Vec3)(ref s2)).Normalize();
			SplashFoamDecal splashFoamDecal2 = _splashFoamDecals[_nextDecalToUse];
			if ((NativeObject)(object)splashFoamDecal2._splashFoamDecal == (NativeObject)null)
			{
				Decal val6 = Decal.CreateDecal((string)null);
				val6.SetMaterial(Material.GetFromResource("decal_water_foam"));
				_ownerSceneCached.AddDecalInstance(val6, "editor_set", true);
				splashFoamDecal2._splashFoamDecal = val6;
			}
			Vec3 origin2 = _movementParticles[_leftDecalParticleIndex].LocalFrame.origin;
			bool pointIsValid = true;
			Vec3 val7 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref origin2);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			float waterLevelAtPosition = ((WeakGameEntity)(ref gameEntity)).GetWaterLevelAtPosition(((Vec3)(ref val7)).AsVec2, true, false);
			val7.z = waterLevelAtPosition + 2.5f;
			Vec3 limitingVector = Vec3.Zero;
			origin2 = GetHeightCorrectedPosForSlice(_movementParticles[_leftDecalParticleIndex], ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref val7).z, ref pointIsValid, ref limitingVector);
			if (pointIsValid)
			{
				float num15 = 4f + (MBRandom.RandomFloat - 0.5f) * 1.5f;
				_nextDecalLeftSpawnMetersSq = num15 * num15;
				Vec3 surfaceNormal = _movementParticles[_leftDecalParticleIndex].SurfaceNormal;
				MatrixFrame identity = MatrixFrame.Identity;
				identity.origin = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref origin2);
				identity.rotation.u = Vec3.Up;
				Vec3 val8 = ((Mat3)(ref globalFrame.rotation)).TransformToParent(ref surfaceNormal);
				val8.z = 0f;
				((Vec3)(ref val8)).Normalize();
				identity.rotation.s = val8;
				identity.rotation.f = -((Vec3)(ref identity.rotation.s)).CrossProductWithUp();
				((Vec3)(ref identity.rotation.f)).Normalize();
				ref Vec3 origin3 = ref identity.origin;
				origin3 -= 0.35f * val8;
				splashFoamDecal2._cumulativeDtTillStart = 0f;
				splashFoamDecal2._splashFoamDecal.SetIsVisible(true);
				float num16 = MathF.Clamp((((Vec3)(ref val5)).Length - 4f) / 8f, 0f, 1f);
				float num17 = 0.6f + num16 * 0.2f;
				splashFoamDecal2._randomScale = Vec3.One * (0.9f + MBRandom.RandomFloat * 0.2f) * num17;
				splashFoamDecal2._randomScale.x *= 1f * MBRandom.RandomFloat + 0.4f;
				((Mat3)(ref identity.rotation)).ApplyScaleLocal(ref val);
				splashFoamDecal2._splashFoamDecal.Frame = identity;
				splashFoamDecal2._splashFoamDecal.SetAlpha(0f);
				splashFoamDecal2._currentFrame = identity;
				int num18 = MBRandom.RandomInt(3);
				float num19 = (float)(num18 % 2) * 0.5f;
				float num20 = (float)(num18 / 2) * 0.5f;
				splashFoamDecal2._splashFoamDecal.SetVectorArgument(num19, num20, -0.5f, -0.5f);
				float num21 = 0.16f * (0.8f + MBRandom.RandomFloat * 0.4f);
				float num22 = 0.45f * (0.8f + MBRandom.RandomFloat * 0.4f);
				splashFoamDecal2._currentSpeed = val5 * num22 + identity.rotation.s * ((Vec3)(ref val5)).Length * num21;
				float num23 = -0.34906584f * (0.8f + MBRandom.RandomFloat * 0.4f);
				splashFoamDecal2._sideVectorStart = val8;
				((Vec3)(ref splashFoamDecal2._sideVectorStart)).RotateAboutZ(MathF.PI / 2f);
				splashFoamDecal2._sideVectorEnd = splashFoamDecal2._sideVectorStart;
				((Vec3)(ref splashFoamDecal2._sideVectorEnd)).RotateAboutZ(num23);
				splashFoamDecal2._isLeft = true;
				Vec2 val9 = default(Vec2);
				((Vec2)(ref val9))._002Ector(2.5f, 2.5f);
				splashFoamDecal2._splashFoamDecal.OverrideRoadBoundaryP0(val9);
				Vec2 val10 = default(Vec2);
				((Vec2)(ref val10))._002Ector(MBRandom.RandomFloat, MBRandom.RandomFloat);
				splashFoamDecal2._splashFoamDecal.OverrideRoadBoundaryP1(val10);
				_nextDecalToUse = (_nextDecalToUse + 1) % 50;
				_lastDecalLeftSpawnPosition = val4;
			}
		}
		num14 = ((Vec3)(ref _lastDecalRightSpawnPosition)).DistanceSquared(val4);
		if (_nextDecalRightSpawnMetersSq < num14)
		{
			Vec3 val11 = (globalFrame.origin - _previousShipFrameForDecalSpawn) / dt;
			Vec3 s3 = globalFrame.rotation.s;
			s3.z = 0f;
			((Vec3)(ref s3)).Normalize();
			SplashFoamDecal splashFoamDecal3 = _splashFoamDecals[_nextDecalToUse];
			if ((NativeObject)(object)splashFoamDecal3._splashFoamDecal == (NativeObject)null)
			{
				Decal val12 = Decal.CreateDecal((string)null);
				val12.SetMaterial(Material.GetFromResource("decal_water_foam"));
				_ownerSceneCached.AddDecalInstance(val12, "editor_set", true);
				splashFoamDecal3._splashFoamDecal = val12;
			}
			Vec3 origin4 = _movementParticles[_rightDecalParticleIndex].LocalFrame.origin;
			bool pointIsValid2 = true;
			Vec3 val13 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref origin4);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			float waterLevelAtPosition2 = ((WeakGameEntity)(ref gameEntity)).GetWaterLevelAtPosition(((Vec3)(ref val13)).AsVec2, true, false);
			val13.z = waterLevelAtPosition2 + 2.5f;
			Vec3 limitingVector2 = Vec3.Zero;
			origin4 = GetHeightCorrectedPosForSlice(_movementParticles[_rightDecalParticleIndex], ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref val13).z, ref pointIsValid2, ref limitingVector2);
			if (pointIsValid2)
			{
				float num24 = 4f + (MBRandom.RandomFloat - 0.5f) * 1.5f;
				_nextDecalRightSpawnMetersSq = num24 * num24;
				Vec3 surfaceNormal2 = _movementParticles[_rightDecalParticleIndex].SurfaceNormal;
				MatrixFrame identity2 = MatrixFrame.Identity;
				identity2.origin = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref origin4);
				identity2.rotation.u = Vec3.Up;
				Vec3 val14 = ((Mat3)(ref globalFrame.rotation)).TransformToParent(ref surfaceNormal2);
				val14.z = 0f;
				((Vec3)(ref val14)).Normalize();
				identity2.rotation.s = val14;
				identity2.rotation.f = -((Vec3)(ref identity2.rotation.s)).CrossProductWithUp();
				((Vec3)(ref identity2.rotation.f)).Normalize();
				ref Vec3 origin5 = ref identity2.origin;
				origin5 -= 0.35f * val14;
				splashFoamDecal3._cumulativeDtTillStart = 0f;
				splashFoamDecal3._splashFoamDecal.SetIsVisible(true);
				float num25 = MathF.Clamp((((Vec3)(ref val11)).Length - 4f) / 8f, 0f, 1f);
				float num26 = 0.6f + num25 * 0.2f;
				splashFoamDecal3._randomScale = Vec3.One * (0.9f + MBRandom.RandomFloat * 0.2f) * num26;
				splashFoamDecal3._randomScale.x *= 1f * MBRandom.RandomFloat + 0.4f;
				((Mat3)(ref identity2.rotation)).ApplyScaleLocal(ref val);
				splashFoamDecal3._splashFoamDecal.Frame = identity2;
				splashFoamDecal3._splashFoamDecal.SetAlpha(0f);
				splashFoamDecal3._currentFrame = identity2;
				float num27 = 0.16f * (0.8f + MBRandom.RandomFloat * 0.4f);
				float num28 = 0.45f * (0.8f + MBRandom.RandomFloat * 0.4f);
				splashFoamDecal3._currentSpeed = val11 * num28 + identity2.rotation.s * ((Vec3)(ref val11)).Length * num27;
				int num29 = MBRandom.RandomInt(3);
				float num30 = (float)(num29 % 2) * 0.5f;
				float num31 = (float)(num29 / 2) * 0.5f;
				splashFoamDecal3._splashFoamDecal.SetVectorArgument(num30, num31, -0.5f, 0.5f);
				float num32 = 0.34906584f * (0.8f + MBRandom.RandomFloat * 0.4f);
				splashFoamDecal3._sideVectorStart = val14;
				((Vec3)(ref splashFoamDecal3._sideVectorStart)).RotateAboutZ(-MathF.PI / 2f);
				splashFoamDecal3._sideVectorEnd = splashFoamDecal3._sideVectorStart;
				((Vec3)(ref splashFoamDecal3._sideVectorEnd)).RotateAboutZ(num32);
				splashFoamDecal3._isLeft = false;
				Vec2 val15 = default(Vec2);
				((Vec2)(ref val15))._002Ector(2.5f, 2.5f);
				splashFoamDecal3._splashFoamDecal.OverrideRoadBoundaryP0(val15);
				Vec2 val16 = default(Vec2);
				((Vec2)(ref val16))._002Ector(MBRandom.RandomFloat, MBRandom.RandomFloat);
				splashFoamDecal3._splashFoamDecal.OverrideRoadBoundaryP1(val16);
				_nextDecalToUse = (_nextDecalToUse + 1) % 50;
				_lastDecalRightSpawnPosition = val4;
			}
		}
		_previousShipFrameForDecalSpawn = globalFrame.origin;
	}

	private void HandleWetnessDecals(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).IsInEditorScene();
		float num = dt / 6f;
		foreach (WetnessDecalData wetnessDecal in _wetnessDecals)
		{
			foreach (ParticleData splashParticle in _splashParticles)
			{
				if (!((NativeObject)(object)splashParticle.CurrentSplashParticle != (NativeObject)null) || !splashParticle.CurrentSplashParticle.HasAliveParticles())
				{
					continue;
				}
				float num2 = 0.13f * splashParticle.Size * dt;
				float num3 = splashParticle.Size * 2.1f;
				if (Vec3.DotProduct(wetnessDecal.Normal, splashParticle.LocalFrame.rotation.s) > 0f)
				{
					Vec2 asVec = ((Vec3)(ref wetnessDecal.LocalPosition)).AsVec2;
					if (((Vec2)(ref asVec)).Distance(((Vec3)(ref splashParticle.LocalFrame.origin)).AsVec2) < num3)
					{
						float num4 = 1f;
						wetnessDecal.CurrentAlpha = Math.Min(wetnessDecal.CurrentAlpha + num2 * num4, 1f);
					}
				}
			}
			wetnessDecal.CurrentAlpha = MathF.Max(wetnessDecal.CurrentAlpha - num, 0f);
			float num5 = MathF.Pow(wetnessDecal.CurrentAlpha, 0.5f);
			float num6 = 0.2f + num5 * 0.8f;
			wetnessDecal.Decal.SetAlpha(MathF.Min(num6, 1f));
		}
	}
}
