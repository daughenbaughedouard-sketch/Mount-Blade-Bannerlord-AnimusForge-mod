using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace NavalDLC.Missions.Objects;

[ScriptComponentParams("ship_visual_only", "sail_visual")]
public class SailVisual : ScriptComponentBehavior
{
	internal struct BurningRecord
	{
		internal List<BurningSystem> SailFires;

		internal BurningSystem MastFire;

		internal float SailLengthZ;

		internal BurningSystem YardLeftFire;

		internal float FireDt;

		internal BurningSystem YardRightFire;

		internal float YardFireStartDt;

		internal List<BurningSystem> RotatorFires;

		internal float RotatorFireStartDt;

		internal Color InitialYardMastColor;

		internal List<BurningSystem> StabilizerFires;

		internal bool BurningFinished;

		internal List<BurningSystem> FoldFires;

		internal List<BurningSystem> StaticRopeFires;

		internal float SailLengthX;

		internal BurningRecord(bool _ = false)
		{
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			SailFires = new List<BurningSystem>();
			MastFire = null;
			YardLeftFire = null;
			YardRightFire = null;
			RotatorFires = new List<BurningSystem>();
			StabilizerFires = new List<BurningSystem>();
			FoldFires = new List<BurningSystem>();
			StaticRopeFires = new List<BurningSystem>();
			SailLengthX = 0f;
			SailLengthZ = 0f;
			FireDt = 0f;
			YardFireStartDt = 0f;
			RotatorFireStartDt = 0f;
			InitialYardMastColor = Color.White;
			BurningFinished = false;
		}
	}

	internal struct SailFoldProgress
	{
		internal const float FoldUnfoldSoundEventAnimationDxStopThreshold = 0.875f;

		internal float CurrentProgress;

		internal float RealProgress;

		internal bool FoldIsOngoing;

		internal bool UnfoldIsOngoing;

		internal int NumberOfMorphKeys;

		internal Vec3[] LeftVertexPositions;

		internal Vec3[] RightVertexPositions;

		internal Vec3[] CenterVertexPositions;

		internal Vec3 CurrentLeftFreeBonePosition;

		internal Vec3 CurrentRightFreeBonePosition;

		internal Vec3 CurrentCenterFreeBonePosition;

		internal SoundEvent FoldUnfoldSoundEvent;

		internal bool ShouldMakeFoldUnfoldSound;

		internal bool ShouldStopFoldUnfoldSound;
	}

	internal struct LateenSailData
	{
		internal GameEntity RollRotationEntity;

		internal GameEntity YardShiftEntity;

		internal float LastYawSection;

		internal float RollRotationAnimProgress;

		internal float RollRotationRealDt;

		internal bool RollRotationInProgress;

		internal float RollRotationInitial;

		internal float RollRotationTarget;

		internal float YardShiftInitial;

		internal float YardShiftTarget;

		internal SoundEvent RollAnimationSoundEvent;
	}

	internal struct PulleyDataCache
	{
		internal GameEntity Entity;

		internal PulleySystem PulleySystem;
	}

	internal struct SimpleRopeRecord
	{
		internal GameEntity ParentEntity;

		internal GameEntity RopeEntity;

		internal GameEntity TargetEntity;

		internal RopeSegment RopeSegment;

		internal bool StartPointAttachedToYard;

		internal bool EndPointAttachedToYard;

		internal bool IsBigRope;
	}

	internal struct KnobConnectionPoint
	{
		internal Vec3 ShipLocalPosition;

		internal Vec3 GlobalPosition;

		internal bool IsFixed;

		internal bool RightOfYard;

		internal void UpdateGlobalPosition(Vec3 pos)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			GlobalPosition = pos;
		}

		internal void UpdateRightOfYard(bool value)
		{
			RightOfYard = value;
		}
	}

	internal class FreeBoneRecord
	{
		internal MatrixFrame InitialLocalFrame;

		internal MatrixFrame CurrentLocalFrame;

		internal Vec3 CurrentFrameWithoutRandomWind;

		internal GameEntity Entity;

		internal PulleyDataCache FoldSailPulley;

		internal List<PulleyDataCache> RotatorPulleys;

		internal List<PulleyDataCache> StabilityPulleys;

		internal List<SimpleRopeRecord> StabilityRopes;

		internal sbyte BoneIndex;

		internal FreeBoneConnectionType ConnectionType;

		internal FreeBoneType BoneType;
	}

	internal class FlagCaptureAnimation
	{
		internal bool AnimationInProgress;

		internal Texture NewBannerTexture;

		internal float DtTillStart;

		internal bool MaterialSet;

		internal float BannerWindFactor;
	}

	internal enum FreeBoneConnectionType
	{
		All,
		Closest,
		ClosestTwo
	}

	public enum SailType
	{
		SquareSail,
		LateenSail
	}

	internal enum KnobTypeEnum
	{
		Bollard,
		Cleat,
		Belaying
	}

	internal enum FreeBoneType
	{
		Left,
		Right,
		Center
	}

	internal enum LevelForEditor
	{
		None,
		Lvl1,
		Lvl2,
		Lvl3
	}

	private const string SailMeshEntityTag = "sail_mesh_entity";

	private const string StaticFoldedSailMeshEntityTag = "folded_static_entity";

	private const string SailTopBannerTag = "bd_banner_b";

	private const string FreeBoneTag = "free_bone";

	private const string RollRotationEntityTag = "roll_rotation_entity";

	private const string YawRotationEntityTag = "yaw_rotation_entity";

	private const string YardShiftEntityTag = "yard_shift";

	private const string SailYardEntityTag = "sail_yard";

	private const string PulleySystemsParentTag = "pulley_systems_parent";

	private const string FoldPulleysParentTag = "sail_fold_pulleys";

	private const string RotatePulleysParentTag = "sail_rotate_pulleys";

	private const string StabilityRopesParentTag = "stability_ropes_parent";

	private const string StaticRopesParentTag = "static_ropes_parent";

	private const string MastEntityTag = "mast_entity";

	private const string SimpleRopeTag = "simple_rope";

	private const string SimpleRopeStartTag = "simple_rope_start";

	private const string SimpleRopeEndTag = "simple_rope_end";

	private const string AttachedToYardTag = "attached_to_yard";

	private const string KnobPointsParentTag = "knob_points_parent";

	private const string KnobPointTag = "knot_point";

	private const string KnobPointDynamicTag = "dynamic_knob";

	private const string YardMeshEntity = "yard_mesh";

	private const string SailMeshBurningEntity = "sail_mesh_free_entity";

	private const string SquareSailLvl3ShiftEntityTag = "lvl3_shift_entity";

	private const string SquareSailLvl3Visibilitytag = "lvl3_lateens";

	private const string SquareSailLvl3MeshHoldertag = "lvl3_lateens_entity";

	private const string SquareSailLvl3FoldedParentTag = "lvl3_lateens_folded";

	private const string BallistaVisibilityRopeTag = "ballista_visibility";

	private const string TopFlagRopeTag = "flag_capture_rope";

	private static readonly string[] ClothFragmentPrefabs = new string[8] { "cloth_fragment_a", "cloth_fragment_b", "cloth_fragment_c", "cloth_fragment_e", "cloth_fragment_g", "cloth_fragment_i", "cloth_fragment_d", "cloth_fragment_h" };

	private const float InvisibleDistanceSquared = 22500f;

	private const float LinearDistanceSquared = 2025f;

	private static readonly int SailUnfoldSoundEventId = SoundManager.GetEventGlobalIndex("event:/mission/movement/vessel/sail/sail_open");

	private static readonly int SailFoldSoundEventId = SoundManager.GetEventGlobalIndex("event:/mission/movement/vessel/sail/sail_close");

	private static readonly int LateenSailRollSoundEventId = SoundManager.GetEventGlobalIndex("event:/mission/movement/vessel/sail/lateen_rotation");

	private List<KnobConnectionPoint> _knobConnectionPoints = new List<KnobConnectionPoint>();

	[EditableScriptComponentVariable(true, "Fold Sail Duration")]
	private float _foldSailDuration = 3f;

	[EditableScriptComponentVariable(true, "Folded Sail Transition Duration")]
	private float _foldedSailTransitionDuration = 0.5f;

	[EditableScriptComponentVariable(true, "Fold Free Bone Reset Duration")]
	private float _foldFreeBoneResetDuration = 1.2f;

	[EditableScriptComponentVariable(true, "Unfold Sail Duration")]
	private float _unfoldSailDuration = 4f;

	[EditableScriptComponentVariable(true, "Fold Sail Step Multiplier")]
	private float _foldSailStepMultiplier = 2f;

	[EditableScriptComponentVariable(true, "Lateen Yard Shift")]
	private float _lateenYardShift;

	[EditableScriptComponentVariable(true, "Lateen Roll Change Degree Limit")]
	private float _lateenRollChangeDegreeLimit = 20f;

	[EditableScriptComponentVariable(true, "Lateen Roll Change Animation Duration")]
	private float _lateenRollChangeAnimationDuration = 3f;

	[EditableScriptComponentVariable(true, "Lateen Roll Change Animation Step Multiplier")]
	private float _lateenRollChangeAnimationStepMultiplier = 1f;

	[EditableScriptComponentVariable(true, "Lateen Roll Change Yard Shift Start")]
	private float _lateenRollChangeYardShiftStart = 3f;

	[EditableScriptComponentVariable(true, "Lateen Roll Change Yard Shift Duration")]
	private float _lateenRollChangeYardShiftDuration = 4f;

	[EditableScriptComponentVariable(true, "Lateen Roll Change Yard Shift Acceleration")]
	private float _lateenRollChangeYardShiftAcceleration = 8f;

	[EditableScriptComponentVariable(true, "Lateen Roll Degrees")]
	private float _lateenRollDegrees = 45f;

	[EditableScriptComponentVariable(true, "Rope Connection Max Distance")]
	private float _ropeConnectionMaxDistance = 7f;

	[EditableScriptComponentVariable(true, "Knob Type")]
	private KnobTypeEnum _knobType;

	[EditableScriptComponentVariable(true, "Place Knobs")]
	private SimpleButton _placeKnobButton = new SimpleButton();

	[EditableScriptComponentVariable(true, "Knob Color")]
	private Color _placeKnobColor = Color.White;

	[EditableScriptComponentVariable(true, "Start Fire")]
	private SimpleButton _startFireButton = new SimpleButton();

	[EditableScriptComponentVariable(true, "Place Cloth Fragments")]
	private SimpleButton _placeClothFragments = new SimpleButton();

	[EditableScriptComponentVariable(true, "Sail Type")]
	private SailType _sailType;

	[EditableScriptComponentVariable(true, "Burning Animation Duration")]
	private float _burningAnimationDuration = 20f;

	private LateenSailData _lateenSailData;

	[EditableScriptComponentVariable(true, "Square Lvl3 Mast Shift")]
	private float _squareLvl3MastShift;

	[EditableScriptComponentVariable(true, "Editor Only Level Selection")]
	private LevelForEditor _editorOnlyLevelSelection;

	[EditableScriptComponentVariable(true, "Top Lateen Fire Material")]
	private Material _topLateenFireMaterial;

	[EditableScriptComponentVariable(true, "Editor Only Ship Health")]
	private float _editorOnlyShipHealth = 1f;

	[EditableScriptComponentVariable(true, "Top Flag Rope Position")]
	private float _topFlagRopePosition = 0.8f;

	[EditableScriptComponentVariable(true, "Capture Flag Bottom Rope Position")]
	private float _captureTheFlagBottomPosition = 0.25f;

	[EditableScriptComponentVariable(true, "Start Capture The Flag Animation")]
	private SimpleButton _startCaptureTheFlagAnimation = new SimpleButton();

	private SailFoldProgress _ongoingAnimationData;

	private readonly List<FreeBoneRecord> _freeBones = new List<FreeBoneRecord>();

	private readonly List<SimpleRopeRecord> _simpleRopes = new List<SimpleRopeRecord>();

	private readonly List<SimpleRopeRecord> _mastRopes = new List<SimpleRopeRecord>();

	private Skeleton _sailSkeleton;

	private float _totalFoldDuration;

	private float _totalUnfoldDuration;

	private float _mastClipDistanceFromOrigin = 100f;

	private GameEntity _mastEntity;

	private GameEntity _yardEntity;

	private Mesh _foldedStaticSailMesh;

	private GameEntity _foldedStaticSailEntity;

	private GameEntity _knobParent;

	private SimpleRopeRecord _topFlagRope;

	private GameEntity _burningSailEntity;

	private Mesh _burningSailMesh;

	private Vec3 _currentFrameGlobalWind = Vec3.Zero;

	private Mesh _yardMesh;

	private MatrixFrame _previousYawEntityFrame = MatrixFrame.Identity;

	private MatrixFrame _previousSailYardFrame = MatrixFrame.Identity;

	private float _cumulativeDt;

	private int _resetClothMeshFrameCounter;

	private bool _ropesAreInvisibleThisFrame;

	private bool _ropesWereInvisibleLastFrame;

	private bool _ropesWereLinearLastFrame;

	private bool _lodCheckFirstFrame = true;

	private List<WeakGameEntity> _topLateenSails = new List<WeakGameEntity>();

	private List<WeakGameEntity> _topLateenFoldedSails = new List<WeakGameEntity>();

	private List<WeakGameEntity> _ballistaVisibilityRopes = new List<WeakGameEntity>();

	private int _ballistaRopeEnableFrameCounter;

	private int _currentSailLevelUsed = -1;

	private BurningRecord _burningRecord;

	private bool _isBurning;

	private float _sailEntityAlpha = 1f;

	private float _lastMorphAnimKeySet = -1f;

	private int _remainingFramesForAnimation = 3;

	private float _foldAnimWindReductionFactor = 1f;

	private FlagCaptureAnimation _captureTheFlagAnimation;

	public float TotalFoldDuration => _totalFoldDuration;

	public float TotalUnfoldDuration => _totalUnfoldDuration;

	public ClothSimulatorComponent SailClothComponent { get; private set; }

	public GameEntity SailSkeletonEntity { get; private set; }

	public GameEntity SailYawRotationEntity { get; private set; }

	public ClothSimulatorComponent SailTopBannerClothComponent { get; private set; }

	public GameEntity SailTopBannerEntity { get; private set; }

	public SailType Type => _sailType;

	public ShipVisual ShipVisual { get; private set; }

	public bool SailEnabled { get; set; } = true;

	public bool SoundsEnabled { get; set; }

	public bool FoldAnimationEnabled { get; set; } = true;

	internal SailVisual()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		_ongoingAnimationData.CurrentProgress = 0f;
		_ongoingAnimationData.RealProgress = 0f;
		_ongoingAnimationData.FoldIsOngoing = false;
		_ongoingAnimationData.UnfoldIsOngoing = false;
		_ongoingAnimationData.LeftVertexPositions = null;
		_ongoingAnimationData.RightVertexPositions = null;
		_ongoingAnimationData.CenterVertexPositions = null;
		_ongoingAnimationData.NumberOfMorphKeys = -1;
		_ongoingAnimationData.CurrentLeftFreeBonePosition = Vec3.Zero;
		_ongoingAnimationData.CurrentRightFreeBonePosition = Vec3.Zero;
		_ongoingAnimationData.CurrentCenterFreeBonePosition = Vec3.Zero;
		_ongoingAnimationData.FoldUnfoldSoundEvent = null;
		_ongoingAnimationData.ShouldMakeFoldUnfoldSound = false;
		_ongoingAnimationData.ShouldStopFoldUnfoldSound = false;
		_lateenSailData = default(LateenSailData);
		_lateenSailData.RollRotationEntity = null;
		_lateenSailData.YardShiftEntity = null;
		_lateenSailData.LastYawSection = 0f;
		_lateenSailData.RollRotationAnimProgress = 0f;
		_lateenSailData.RollRotationRealDt = 0f;
		_lateenSailData.RollRotationInProgress = false;
		_lateenSailData.RollRotationInitial = 0f;
		_lateenSailData.RollRotationTarget = 0f;
		_lateenSailData.YardShiftInitial = 0f;
		_lateenSailData.YardShiftTarget = 0f;
		_lateenSailData.RollAnimationSoundEvent = null;
		_captureTheFlagAnimation = new FlagCaptureAnimation();
		_captureTheFlagAnimation.AnimationInProgress = false;
		_captureTheFlagAnimation.NewBannerTexture = null;
		_captureTheFlagAnimation.DtTillStart = 0f;
		_captureTheFlagAnimation.MaterialSet = false;
		_captureTheFlagAnimation.BannerWindFactor = 1f;
		_topFlagRope = default(SimpleRopeRecord);
	}

	protected override void OnEditorInit()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		SoundsEnabled = true;
		_editorOnlyLevelSelection = LevelForEditor.None;
		_editorOnlyShipHealth = 1f;
		FetchEntities();
		UpdatePreviousYardFrame();
		if (_yardEntity != (GameEntity)null)
		{
			MatrixFrame globalFrame = _yardEntity.GetGlobalFrame();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			_previousSailYardFrame = ((MatrixFrame)(ref globalFrame2)).TransformToLocalNonOrthogonal(ref globalFrame);
		}
		if (_sailType == SailType.LateenSail)
		{
			InitLateenSailData();
		}
		InitSailFoldAnimationResources();
		if ((NativeObject)(object)_sailSkeleton != (NativeObject)null)
		{
			_sailSkeleton.EnableScriptDrivenPostIntegrateCallback();
		}
		ClothSimulatorComponent sailTopBannerClothComponent = SailTopBannerClothComponent;
		if (sailTopBannerClothComponent != null)
		{
			sailTopBannerClothComponent.SetForcedGustStrength(0f);
		}
		ClothSimulatorComponent sailClothComponent = SailClothComponent;
		if (sailClothComponent != null)
		{
			sailClothComponent.SetForcedGustStrength(0f);
		}
		UpdateTotalFoldDuration();
		UpdateTotalUnfoldDuration();
		ComputeMastClipPlane();
		PlaceClothFragmentsRandomly((int)(Time.ApplicationTime * 100f));
		PlaceTopFlag(_topFlagRopePosition);
	}

	protected override void OnEditorTick(float dt)
	{
		_cumulativeDt += dt;
		FetchEntities();
		if (_ongoingAnimationData.NumberOfMorphKeys == -1)
		{
			InitSailFoldAnimationResources();
		}
		ComputeMastClipPlane();
		if (!_isBurning)
		{
			HandleLOD();
		}
		CheckFoldAnimationState(dt);
		if (_sailType == SailType.LateenSail)
		{
			TickLateenSail(dt);
		}
		if (SailSkeletonEntity != (GameEntity)null)
		{
			SetButtomRopePositions(dt, disableWind: false);
		}
		if (!_ropesAreInvisibleThisFrame)
		{
			TickRopesAndPulleys();
		}
		if (Input.IsKeyReleased((InputKey)61))
		{
			SailEnabled = false;
		}
		else if (Input.IsKeyReleased((InputKey)62))
		{
			SailEnabled = true;
		}
		FoldUnfoldSoundEventTick();
		if (_editorOnlyLevelSelection == LevelForEditor.None)
		{
			int num = FetchSailLevel();
			if (num != _currentSailLevelUsed)
			{
				AdjustLevelOfSail(num);
				_currentSailLevelUsed = num;
			}
		}
		CheckClothResetTimer();
		if ((NativeObject)(object)_sailSkeleton != (NativeObject)null)
		{
			_sailSkeleton.EnableScriptDrivenPostIntegrateCallback();
		}
		if (_isBurning && !_burningRecord.BurningFinished)
		{
			TickFire(dt);
		}
		UpdateMastClipPlane();
		if (_captureTheFlagAnimation.AnimationInProgress)
		{
			TickFlagCaptureAnimation(dt);
		}
	}

	protected override void OnInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		ShipVisual = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<ShipVisual>();
		ShipVisual shipVisual = ShipVisual;
		if (shipVisual != null)
		{
			shipVisual.SailVisuals?.Add((ScriptComponentBehavior)(object)this);
		}
		_editorOnlyLevelSelection = LevelForEditor.None;
		_editorOnlyShipHealth = 1f;
		FetchEntities();
		UpdatePreviousYardFrame();
		if (_yardEntity != (GameEntity)null)
		{
			MatrixFrame globalFrame = _yardEntity.GetGlobalFrame();
			val = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref val)).GetGlobalFrame();
			_previousSailYardFrame = ((MatrixFrame)(ref globalFrame2)).TransformToLocalNonOrthogonal(ref globalFrame);
		}
		if (_sailType == SailType.LateenSail)
		{
			InitLateenSailData();
		}
		InitSailFoldAnimationResources();
		int num = FetchSailLevel();
		AdjustLevelOfSail(num);
		_currentSailLevelUsed = num;
		if ((NativeObject)(object)_sailSkeleton != (NativeObject)null)
		{
			_sailSkeleton.EnableScriptDrivenPostIntegrateCallback();
		}
		ClothSimulatorComponent sailTopBannerClothComponent = SailTopBannerClothComponent;
		if (sailTopBannerClothComponent != null)
		{
			sailTopBannerClothComponent.SetForcedGustStrength(0f);
		}
		ClothSimulatorComponent sailClothComponent = SailClothComponent;
		if (sailClothComponent != null)
		{
			sailClothComponent.SetForcedGustStrength(0f);
		}
		UpdateTotalFoldDuration();
		UpdateTotalUnfoldDuration();
		ComputeMastClipPlane();
		int seed = (int)(Time.ApplicationTime * 100f);
		if (ShipVisual != null)
		{
			seed = ShipVisual.Seed;
		}
		PlaceClothFragmentsRandomly(seed);
	}

	protected override void OnTickParallel(float dt)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		_cumulativeDt += dt;
		if (_ongoingAnimationData.NumberOfMorphKeys == -1)
		{
			InitSailFoldAnimationResources();
		}
		HandleLOD();
		if (_remainingFramesForAnimation == 0)
		{
			CheckFoldAnimationState(dt);
		}
		else
		{
			_remainingFramesForAnimation--;
		}
		if (_sailType == SailType.LateenSail)
		{
			TickLateenSail(dt);
		}
		if (SailSkeletonEntity != (GameEntity)null)
		{
			SetButtomRopePositions(dt, disableWind: false);
		}
		if (!_ropesAreInvisibleThisFrame)
		{
			TickRopesAndPulleys();
		}
		CheckClothResetTimer();
		if (_isBurning && !_burningRecord.BurningFinished)
		{
			TickFire(dt);
		}
		UpdateMastClipPlane();
		if (_ballistaRopeEnableFrameCounter > 0)
		{
			_ballistaRopeEnableFrameCounter--;
			if (_ballistaRopeEnableFrameCounter == 0)
			{
				foreach (WeakGameEntity ballistaVisibilityRope in _ballistaVisibilityRopes)
				{
					WeakGameEntity current = ballistaVisibilityRope;
					((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(true);
				}
			}
		}
		if (_captureTheFlagAnimation.AnimationInProgress)
		{
			TickFlagCaptureAnimation(dt);
		}
	}

	protected override void OnTick(float dt)
	{
		FoldUnfoldSoundEventTick();
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
	}

	protected override bool SkeletonPostIntegrateCallback(AnimResult result)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		foreach (FreeBoneRecord freeBone in _freeBones)
		{
			if (freeBone.BoneIndex != -1)
			{
				Vec3 origin = freeBone.CurrentLocalFrame.origin;
				sbyte parentBoneIndex = _sailSkeleton.GetParentBoneIndex(freeBone.BoneIndex);
				Transformation entitialOutTransform = ((AnimResult)(ref result)).GetEntitialOutTransform(parentBoneIndex, _sailSkeleton);
				((AnimResult)(ref result)).SetOutBoneDisplacement(freeBone.BoneIndex, ((Transformation)(ref entitialOutTransform)).TransformToLocal(origin), _sailSkeleton);
			}
		}
		if (_sailType == SailType.LateenSail && _mastEntity != (GameEntity)null)
		{
			sbyte b = 2;
			MatrixFrame globalFrame = SailSkeletonEntity.GetGlobalFrame();
			MatrixFrame globalFrame2 = _mastEntity.GetGlobalFrame();
			Vec3 u = globalFrame2.rotation.u;
			bool flag = default(bool);
			MBMath.FindPlaneLineIntersectionPointWithNormal(globalFrame2.origin, u, globalFrame.origin, globalFrame.origin - u * 100f, ref flag);
			MatrixFrame globalFrame3 = _yardEntity.GetGlobalFrame();
			ref Vec3 origin2 = ref globalFrame2.origin;
			origin2 += globalFrame3.rotation.f * 0.25f * ((Vec3)(ref globalFrame.rotation.f)).Length;
			MatrixFrame val = globalFrame;
			((Mat3)(ref val.rotation)).MakeUnit();
			MatrixFrame identity = MatrixFrame.Identity;
			identity.origin = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref globalFrame2.origin);
			identity.rotation = ((MatrixFrame)(ref val)).TransformToLocal(ref globalFrame2).rotation;
			sbyte parentBoneIndex2 = _sailSkeleton.GetParentBoneIndex(b);
			Transformation entitialOutTransform2 = ((AnimResult)(ref result)).GetEntitialOutTransform(parentBoneIndex2, _sailSkeleton);
			Transformation val2 = ((Transformation)(ref entitialOutTransform2)).TransformToLocal(Transformation.CreateFromMatrixFrame(identity));
			((Transformation)(ref val2)).Rotate(-MathF.PI / 2f, Vec3.Forward);
			((AnimResult)(ref result)).SetOutBoneDisplacement(b, val2.Origin, _sailSkeleton);
			((AnimResult)(ref result)).SetOutQuat(b, val2.Rotation, _sailSkeleton);
		}
		return true;
	}

	protected override void OnBoundingBoxValidate()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		if (_yardEntity == (GameEntity)null || (NativeObject)(object)_sailSkeleton == (NativeObject)null)
		{
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		BoundingBox bb = default(BoundingBox);
		((BoundingBox)(ref bb)).BeginRelaxation();
		BoundingBox localBoundingBox = _yardEntity.GetLocalBoundingBox();
		Vec3 origin = _yardEntity.GetGlobalFrame().origin;
		origin = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref origin);
		float num = localBoundingBox.radius * 1.1f;
		Vec3 val = origin + Vec3.One * num;
		((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val);
		val = origin - Vec3.One * num;
		((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val);
		foreach (FreeBoneRecord freeBone in _freeBones)
		{
			if (freeBone.FoldSailPulley.PulleySystem != null)
			{
				freeBone.FoldSailPulley.PulleySystem.ApplyBoundingBox(globalFrame, ref bb);
			}
			if (freeBone.RotatorPulleys != null)
			{
				foreach (PulleyDataCache rotatorPulley in freeBone.RotatorPulleys)
				{
					if (rotatorPulley.PulleySystem != null)
					{
						rotatorPulley.PulleySystem.ApplyBoundingBox(globalFrame, ref bb);
					}
				}
			}
			if (freeBone.StabilityPulleys != null)
			{
				foreach (PulleyDataCache stabilityPulley in freeBone.StabilityPulleys)
				{
					if (stabilityPulley.PulleySystem != null)
					{
						stabilityPulley.PulleySystem.ApplyBoundingBox(globalFrame, ref bb);
					}
				}
			}
			if (freeBone.StabilityRopes == null)
			{
				continue;
			}
			foreach (SimpleRopeRecord stabilityRope in freeBone.StabilityRopes)
			{
				if (stabilityRope.RopeSegment != null)
				{
					stabilityRope.RopeSegment.ApplyBoundingBox(globalFrame, ref bb);
				}
			}
		}
		if (_simpleRopes != null)
		{
			foreach (SimpleRopeRecord simpleRope in _simpleRopes)
			{
				if (simpleRope.RopeSegment != null)
				{
					simpleRope.RopeSegment.ApplyBoundingBox(globalFrame, ref bb);
				}
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).RelaxLocalBoundingBox(ref bb);
	}

	protected override void OnRemoved(int removeReason)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnRemoved(removeReason);
		if (_lateenSailData.RollAnimationSoundEvent != null)
		{
			_lateenSailData.RollAnimationSoundEvent.Stop();
			_lateenSailData.RollAnimationSoundEvent = null;
		}
		_ongoingAnimationData.LeftVertexPositions = null;
		_ongoingAnimationData.RightVertexPositions = null;
		_ongoingAnimationData.CenterVertexPositions = null;
		_ongoingAnimationData.ShouldMakeFoldUnfoldSound = false;
		_ongoingAnimationData.ShouldStopFoldUnfoldSound = true;
		if (_ongoingAnimationData.FoldUnfoldSoundEvent != null)
		{
			_ongoingAnimationData.FoldUnfoldSoundEvent.Stop();
			_ongoingAnimationData.FoldUnfoldSoundEvent = null;
		}
		_freeBones.Clear();
		_simpleRopes.Clear();
		_mastRopes.Clear();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		bool flag = ((WeakGameEntity)(ref gameEntity)).IsGhostObject();
		if ((NativeObject)(object)_sailSkeleton != (NativeObject)null && !flag)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.RemoveAlwaysRenderedSkeleton(_sailSkeleton);
		}
		_sailSkeleton = null;
		_yardEntity = null;
		_foldedStaticSailEntity = null;
		SailClothComponent = null;
		SailTopBannerClothComponent = null;
		SailTopBannerEntity = null;
		SailSkeletonEntity = null;
		SailYawRotationEntity = null;
		_mastEntity = null;
		_isBurning = false;
	}

	protected override bool OnCheckForProblems()
	{
		return CheckForProblemsInternal();
	}

	protected override void OnSaveAsPrefab()
	{
		CheckForProblemsInternal();
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)6;
	}

	public void RefreshSailVisual()
	{
		int num = FetchSailLevel();
		AdjustLevelOfSail(num);
		_currentSailLevelUsed = num;
	}

	public void UpdateForcedWindOfSailsAndTopBanner(float dt, Vec3 globalBannerRelativeWindVelocity, in Vec3 sailRelativeGlobalWindVelocity, in Vec3 globalSailForce)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (((Vec3)(ref globalBannerRelativeWindVelocity)).LengthSquared >= 100f)
		{
			globalBannerRelativeWindVelocity = ((Vec3)(ref globalBannerRelativeWindVelocity)).NormalizedCopy() * 10f;
		}
		globalBannerRelativeWindVelocity /= Scene.MaximumWindSpeed;
		globalBannerRelativeWindVelocity *= _captureTheFlagAnimation.BannerWindFactor;
		ClothSimulatorComponent sailTopBannerClothComponent = SailTopBannerClothComponent;
		if (sailTopBannerClothComponent != null)
		{
			sailTopBannerClothComponent.SetForcedWind(globalBannerRelativeWindVelocity, false);
		}
		Vec3 val = globalSailForce;
		val = ((Vec3)(ref val)).RotateVectorToXYPlane();
		Vec3 val2 = ((Vec3)(ref val)).NormalizedCopy();
		val = sailRelativeGlobalWindVelocity;
		Vec2 asVec = ((Vec3)(ref val)).AsVec2;
		Vec3 val3 = ((Vec2)(ref asVec)).Length * val2 * _foldAnimWindReductionFactor;
		val3 *= 2f;
		_currentFrameGlobalWind = Vec3.Lerp(_currentFrameGlobalWind, val3, dt);
		if (((Vec3)(ref _currentFrameGlobalWind)).LengthSquared >= 100f)
		{
			_currentFrameGlobalWind = ((Vec3)(ref _currentFrameGlobalWind)).NormalizedCopy() * 10f;
		}
		SailClothComponent.SetForcedWind(_currentFrameGlobalWind / Scene.MaximumWindSpeed, false);
	}

	public void SetFoldSailDuration(float foldSailDuration)
	{
		_foldSailDuration = foldSailDuration;
		UpdateTotalFoldDuration();
	}

	public void SetFoldSailStepMultiplier(float foldSailStepMultiplier)
	{
		_foldSailStepMultiplier = foldSailStepMultiplier;
		UpdateTotalFoldDuration();
	}

	public void SetUnfoldSailDuration(float unfoldSailDuration)
	{
		_unfoldSailDuration = unfoldSailDuration;
		UpdateTotalUnfoldDuration();
	}

	public void SetSailEntityAlpha(float alpha)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_sailEntityAlpha = alpha;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetAlpha(alpha);
	}

	public void InstantCloseSails()
	{
		SailEnabled = false;
		_ongoingAnimationData.FoldIsOngoing = true;
		_ongoingAnimationData.CurrentProgress = _foldSailDuration + _foldFreeBoneResetDuration + _foldedSailTransitionDuration;
		_ongoingAnimationData.UnfoldIsOngoing = false;
	}

	private bool CheckForProblemsInternal()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		if ((NativeObject)(object)SailTopBannerClothComponent != (NativeObject)null)
		{
			MetaMesh firstMetaMesh = ((GameEntityComponent)SailTopBannerClothComponent).GetFirstMetaMesh();
			for (int i = 0; i < firstMetaMesh.MeshCount; i++)
			{
				Mesh meshAtIndex = firstMetaMesh.GetMeshAtIndex(i);
				if (meshAtIndex.HasCloth() && meshAtIndex.GetClothLinearVelocityMultiplier() != 0f)
				{
					WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
					string text;
					if (!(((WeakGameEntity)(ref val)).Root != ((ScriptComponentBehavior)this).GameEntity))
					{
						val = ((ScriptComponentBehavior)this).GameEntity;
						text = ((WeakGameEntity)(ref val)).Name;
					}
					else
					{
						val = ((ScriptComponentBehavior)this).GameEntity;
						val = ((WeakGameEntity)(ref val)).Root;
						string name = ((WeakGameEntity)(ref val)).Name;
						val = ((ScriptComponentBehavior)this).GameEntity;
						text = name + "|" + ((WeakGameEntity)(ref val)).Name;
					}
					string text2 = text;
					string text3 = "Top banner (" + meshAtIndex.Name + ") of Sail Entity (" + text2 + ") has non-zero linear velocity cloth parameter.";
					MBEditor.AddEntityWarning(((GameEntityComponent)SailTopBannerClothComponent).GetEntity(), text3);
					result = false;
				}
			}
		}
		return result;
	}

	private void PlaceKnobs()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		if (_knobType == KnobTypeEnum.Bollard)
		{
			text = "bollard_a";
		}
		else if (_knobType == KnobTypeEnum.Cleat)
		{
			text = "cleat_a";
		}
		else if (_knobType == KnobTypeEnum.Belaying)
		{
			text = "belaying_pins_a";
		}
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		List<WeakGameEntity> list2 = new List<WeakGameEntity>();
		foreach (FreeBoneRecord freeBone in _freeBones)
		{
			if (freeBone.RotatorPulleys != null)
			{
				foreach (PulleyDataCache rotatorPulley in freeBone.RotatorPulleys)
				{
					WeakGameEntity firstFixedEntity = rotatorPulley.PulleySystem.FirstFixedEntity;
					if (((WeakGameEntity)(ref firstFixedEntity)).IsValid)
					{
						list2.Add(firstFixedEntity);
					}
				}
			}
			if (freeBone.StabilityPulleys != null)
			{
				foreach (PulleyDataCache stabilityPulley in freeBone.StabilityPulleys)
				{
					WeakGameEntity firstFixedEntity2 = stabilityPulley.PulleySystem.FirstFixedEntity;
					if (((WeakGameEntity)(ref firstFixedEntity2)).IsValid)
					{
						list2.Add(firstFixedEntity2);
					}
				}
			}
			if (freeBone.StabilityRopes == null)
			{
				continue;
			}
			foreach (SimpleRopeRecord stabilityRope in freeBone.StabilityRopes)
			{
				if (stabilityRope.RopeEntity != (GameEntity)null)
				{
					list2.Add(stabilityRope.RopeEntity.WeakEntity);
				}
			}
		}
		foreach (WeakGameEntity item in list2)
		{
			WeakGameEntity current3 = item;
			int num = ((WeakGameEntity)(ref current3)).ChildCount - 1;
			while (num >= 0 && num < ((WeakGameEntity)(ref current3)).ChildCount)
			{
				WeakGameEntity child = ((WeakGameEntity)(ref current3)).GetChild(num);
				if (!((WeakGameEntity)(ref child)).HasScriptComponent("rope_segment_cosmetics"))
				{
					((WeakGameEntity)(ref current3)).RemoveChild(child, false, false, true, 37);
				}
				num--;
			}
			WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
			GameEntity val2 = GameEntity.Instantiate(((WeakGameEntity)(ref val)).Scene, text, true, true, "");
			if (!(val2 != (GameEntity)null))
			{
				continue;
			}
			((WeakGameEntity)(ref current3)).AddChild(val2.WeakEntity, false);
			list.Clear();
			foreach (GameEntity child2 in val2.GetChildren())
			{
				if (child2.HasTag("knot_point"))
				{
					list.Add(child2.WeakEntity);
				}
			}
			if (list.Count > 0)
			{
				val = list[MBRandom.RandomInt(list.Count)];
				MatrixFrame frame = ((WeakGameEntity)(ref val)).GetFrame();
				((MatrixFrame)(ref frame)).Fill();
				MatrixFrame val3 = ((MatrixFrame)(ref frame)).Inverse();
				((WeakGameEntity)(ref current3)).SetFrame(ref val3, true);
			}
			foreach (Mesh item2 in ((WeakGameEntity)(ref current3)).GetAllMeshesWithTag("auto_factor_color"))
			{
				item2.Color = ((Color)(ref _placeKnobColor)).ToUnsignedInteger();
			}
		}
	}

	private void SetKnobColors()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		foreach (FreeBoneRecord freeBone in _freeBones)
		{
			if (freeBone.RotatorPulleys != null)
			{
				foreach (PulleyDataCache rotatorPulley in freeBone.RotatorPulleys)
				{
					WeakGameEntity firstFixedEntity = rotatorPulley.PulleySystem.FirstFixedEntity;
					if (firstFixedEntity != (GameEntity)null)
					{
						list.Add(firstFixedEntity);
					}
				}
			}
			if (freeBone.StabilityPulleys != null)
			{
				foreach (PulleyDataCache stabilityPulley in freeBone.StabilityPulleys)
				{
					WeakGameEntity firstFixedEntity2 = stabilityPulley.PulleySystem.FirstFixedEntity;
					if (firstFixedEntity2 != (GameEntity)null)
					{
						list.Add(firstFixedEntity2);
					}
				}
			}
			if (freeBone.StabilityRopes == null)
			{
				continue;
			}
			foreach (SimpleRopeRecord stabilityRope in freeBone.StabilityRopes)
			{
				if (stabilityRope.RopeEntity != (GameEntity)null)
				{
					list.Add(stabilityRope.RopeEntity.WeakEntity);
				}
			}
		}
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current3 = item;
			foreach (Mesh item2 in ((WeakGameEntity)(ref current3)).GetAllMeshesWithTag("auto_factor_color"))
			{
				item2.Color = ((Color)(ref _placeKnobColor)).ToUnsignedInteger();
			}
		}
	}

	private int FetchSailLevel()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTagRecursive = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTagRecursive("upgrade_slot");
		if (firstChildEntityWithTagRecursive != (GameEntity)null)
		{
			foreach (WeakGameEntity child in ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).GetChildren())
			{
				WeakGameEntity current = child;
				if (((WeakGameEntity)(ref current)).GetVisibilityExcludeParents())
				{
					if (((WeakGameEntity)(ref current)).HasTag("base"))
					{
						if (num != -1)
						{
							return -1;
						}
						num = 1;
					}
					else if (((WeakGameEntity)(ref current)).HasTag("lvl2"))
					{
						if (num != -1)
						{
							return -1;
						}
						num = 2;
					}
					else if (((WeakGameEntity)(ref current)).HasTag("lvl3"))
					{
						if (num != -1)
						{
							return -1;
						}
						num = 3;
					}
				}
			}
			return num;
		}
		return 1;
	}

	private void CheckClothResetTimer()
	{
		if (_resetClothMeshFrameCounter > 0)
		{
			_resetClothMeshFrameCounter--;
			if (_resetClothMeshFrameCounter == 0 && (NativeObject)(object)SailClothComponent != (NativeObject)null)
			{
				SailClothComponent.SetResetRequired();
			}
		}
	}

	private void SetSailMaterialWrtLevel(Mesh mesh, int sailLevel, bool isEditorScene)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (sailLevel == -1 && !isEditorScene)
		{
			return;
		}
		List<string> list = new List<string>();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity root = ((WeakGameEntity)(ref gameEntity)).Root;
		int num = 0;
		if (isEditorScene)
		{
			num += (int)(_cumulativeDt * 5f);
		}
		else
		{
			ShipVisual shipVisual = ShipVisual;
			num = ((shipVisual != null) ? shipVisual.Seed : ((int)((ulong)((WeakGameEntity)(ref root)).Pointer & 0xFFFFFFFFu)));
		}
		float num2 = 1f;
		if (ShipVisual != null)
		{
			num2 = ShipVisual.Health;
		}
		else if (isEditorScene)
		{
			num2 = _editorOnlyShipHealth;
		}
		Material val = null;
		if (ShipVisual != null && !string.IsNullOrEmpty(ShipVisual.CustomSailPatternId))
		{
			val = Material.GetFromResource(ShipVisual.CustomSailPatternId);
		}
		if ((NativeObject)(object)val == (NativeObject)null)
		{
			Random random = new Random(num);
			if (_sailType == SailType.SquareSail)
			{
				switch (sailLevel)
				{
				case 1:
					list.Add("00");
					break;
				case 2:
					list.Add("04");
					list.Add("05");
					list.Add("06");
					list.Add("10");
					break;
				case 3:
					list.Add("01");
					list.Add("02");
					list.Add("03");
					list.Add("07");
					list.Add("08");
					list.Add("09");
					list.Add("11");
					break;
				}
			}
			else
			{
				switch (sailLevel)
				{
				case 1:
					list.Add("00");
					break;
				case 2:
					list.Add("04");
					list.Add("06");
					break;
				case 3:
					list.Add("01");
					list.Add("02");
					list.Add("03");
					list.Add("05");
					list.Add("07");
					list.Add("08");
					list.Add("09");
					break;
				}
			}
			string text = "generated_";
			text += ((_sailType == SailType.SquareSail) ? "square_" : "lateen_");
			text = ((sailLevel == 1) ? ((num2 > 0.75f) ? (text + "l1_h4_") : ((num2 > 0.5f) ? (text + "l1_h3_") : ((!(num2 > 0.25f)) ? (text + "l1_h1_") : (text + "l1_h2_")))) : ((num2 > 0.75f) ? (text + "_h4_") : ((num2 > 0.5f) ? (text + "_h3_") : ((!(num2 > 0.25f)) ? (text + "_h1_") : (text + "_h2_")))));
			if (list.Count > 0)
			{
				text += list[random.Next(list.Count)];
			}
			val = Material.GetFromResource(text);
		}
		if (mesh.HasTag("faction_color"))
		{
			if ((NativeObject)(object)val != (NativeObject)null)
			{
				mesh.SetMaterial(val);
			}
			if (ShipVisual != null)
			{
				mesh.Color = ShipVisual.SailColors.Item1;
				mesh.Color2 = ShipVisual.SailColors.Item2;
			}
		}
	}

	private void AdjustSquareSailSpecificLevelData(int sailLevel, bool isEditorScene)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = sailLevel == 3;
		WeakGameEntity gameEntity;
		if ((flag && _currentSailLevelUsed != 3) || (!flag && _currentSailLevelUsed == 3))
		{
			float num = _squareLvl3MastShift;
			if (_currentSailLevelUsed == 3 && !flag)
			{
				num *= -1f;
			}
			List<WeakGameEntity> list = new List<WeakGameEntity>();
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).GetChildrenWithTagRecursive(list, "lvl3_shift_entity");
			foreach (WeakGameEntity item in list)
			{
				WeakGameEntity current = item;
				MatrixFrame frame = ((WeakGameEntity)(ref current)).GetFrame();
				frame.origin.z += num;
				((WeakGameEntity)(ref current)).SetLocalFrame(ref frame, true);
			}
		}
		List<WeakGameEntity> list2 = new List<WeakGameEntity>();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetChildrenWithTagRecursive(list2, "lvl3_lateens");
		foreach (WeakGameEntity item2 in list2)
		{
			WeakGameEntity current2 = item2;
			((WeakGameEntity)(ref current2)).SetDoNotCheckVisibility(true);
			((WeakGameEntity)(ref current2)).SetVisibilityExcludeParents(flag);
		}
		foreach (WeakGameEntity topLateenSail in _topLateenSails)
		{
			WeakGameEntity current3 = topLateenSail;
			((WeakGameEntity)(ref current3)).SetDoNotCheckVisibility(true);
			foreach (Mesh item3 in ((WeakGameEntity)(ref current3)).GetAllMeshesWithTag("faction_color"))
			{
				SetSailMaterialWrtLevel(item3, sailLevel, isEditorScene);
			}
		}
		foreach (WeakGameEntity topLateenFoldedSail in _topLateenFoldedSails)
		{
			WeakGameEntity current5 = topLateenFoldedSail;
			((WeakGameEntity)(ref current5)).SetDoNotCheckVisibility(true);
			foreach (Mesh item4 in ((WeakGameEntity)(ref current5)).GetAllMeshesWithTag("faction_color"))
			{
				SetSailMaterialWrtLevel(item4, sailLevel, isEditorScene);
			}
		}
	}

	private void AdjustLevelOfSail(int sailLevel)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)_sailSkeleton == (NativeObject)null)
		{
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		bool isEditorScene = ((WeakGameEntity)(ref gameEntity)).Scene.IsEditorScene();
		foreach (Mesh allMesh in _sailSkeleton.GetAllMeshes())
		{
			if (allMesh.HasTag("faction_color"))
			{
				SetSailMaterialWrtLevel(allMesh, sailLevel, isEditorScene);
			}
		}
		if (_sailType == SailType.SquareSail)
		{
			AdjustSquareSailSpecificLevelData(sailLevel, isEditorScene);
		}
		if (!(_foldedStaticSailEntity != (GameEntity)null))
		{
			return;
		}
		foreach (Mesh item in _foldedStaticSailEntity.GetAllMeshesWithTag("faction_color"))
		{
			SetSailMaterialWrtLevel(item, sailLevel, isEditorScene);
		}
	}

	private void ApplyRandomWindToRope(ref Vec3 position, float factor)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector((float)Math.Cos(position.x * 2.5f + _cumulativeDt * 4.5f), (float)Math.Cos(position.y * 1.2f + _cumulativeDt * 6.5f), (float)Math.Cos(position.z * 3.5f + _cumulativeDt * 3.5f), -1f);
		position += val * 0.1f * factor;
	}

	private void SetButtomRopePositions(float dt, bool disableWind)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec2 globalWindVelocityOfScene = ((WeakGameEntity)(ref gameEntity)).GetGlobalWindVelocityOfScene();
		float num = MathF.Min(((Vec2)(ref globalWindVelocityOfScene)).Normalize(), 8f);
		float num2 = MathF.Min(num, 4f);
		float num3 = (float)Math.Pow(num / 8f, 0.44999998807907104) * 8f;
		num2 = (float)Math.Pow(num / 4f, 0.44999998807907104) * 4f;
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(globalWindVelocityOfScene, 0f, -1f);
		MatrixFrame globalFrame = SailSkeletonEntity.GetGlobalFrame();
		((Mat3)(ref globalFrame.rotation)).Orthonormalize();
		Vec3 val2 = ((Mat3)(ref globalFrame.rotation)).TransformToLocal(ref val);
		if (((Vec3)(ref val2)).Length > 0f)
		{
			((Vec3)(ref val2)).Normalize();
		}
		if (_yardEntity != (GameEntity)null)
		{
			Vec3 f = _yardEntity.GetGlobalFrame().rotation.f;
			((Vec3)(ref f)).Normalize();
			float num4 = MathF.Clamp(Vec3.DotProduct(val, f), 0f, 1f);
			num3 *= 0.5f + 0.5f * num4;
			num2 *= 0.5f + 0.5f * num4;
		}
		float num5 = 0f;
		if (_ongoingAnimationData.FoldIsOngoing)
		{
			num5 = _ongoingAnimationData.CurrentProgress / _foldFreeBoneResetDuration;
			num5 = MathF.Clamp(num5, 0f, 1f);
			num3 = MathF.Lerp(num3, 0f, num5, 1E-05f);
		}
		if (_ongoingAnimationData.UnfoldIsOngoing)
		{
			num5 = MathF.Clamp((_ongoingAnimationData.CurrentProgress - (_unfoldSailDuration + _foldedSailTransitionDuration)) / _foldFreeBoneResetDuration, 0f, 1f);
			num3 = MathF.Lerp(0f, num3, num5, 1E-05f);
		}
		ref Mat3 rotation = ref globalFrame.rotation;
		Vec3 val3 = -Vec3.Up;
		Vec3 val4 = ((Mat3)(ref rotation)).TransformToLocal(ref val3);
		((Vec3)(ref val4)).Normalize();
		foreach (FreeBoneRecord freeBone in _freeBones)
		{
			MatrixFrame initialLocalFrame = freeBone.InitialLocalFrame;
			Vec3 origin = freeBone.InitialLocalFrame.origin;
			if (!disableWind && freeBone.BoneIndex != -1)
			{
				ref Vec3 origin2 = ref initialLocalFrame.origin;
				origin2 += val2 * num3 * 0.07f;
				if (_sailType == SailType.SquareSail)
				{
					num2 = MathF.Lerp(num2, 0f, num5, 1E-05f);
					ref Vec3 origin3 = ref initialLocalFrame.origin;
					origin3 += val4 * num2 * 0.08f;
				}
				origin = initialLocalFrame.origin;
				ApplyRandomWindToRope(ref initialLocalFrame.origin, 0.1f);
			}
			if (freeBone.BoneIndex != -1)
			{
				bool flag = false;
				if (_ongoingAnimationData.FoldIsOngoing && _ongoingAnimationData.CurrentProgress > _foldFreeBoneResetDuration)
				{
					flag = true;
				}
				else if (_ongoingAnimationData.UnfoldIsOngoing && _ongoingAnimationData.CurrentProgress < _unfoldSailDuration)
				{
					flag = true;
				}
				else if (_sailType == SailType.LateenSail && _lateenSailData.RollRotationInProgress)
				{
					flag = true;
				}
				if (flag)
				{
					if (_sailType == SailType.SquareSail)
					{
						if (freeBone.BoneType == FreeBoneType.Left)
						{
							initialLocalFrame.origin = _ongoingAnimationData.CurrentLeftFreeBonePosition;
						}
						else
						{
							initialLocalFrame.origin = _ongoingAnimationData.CurrentRightFreeBonePosition;
						}
					}
					else if (freeBone.BoneType == FreeBoneType.Left)
					{
						initialLocalFrame.origin = _ongoingAnimationData.CurrentLeftFreeBonePosition;
					}
					else if (freeBone.BoneType == FreeBoneType.Right)
					{
						initialLocalFrame.origin = _ongoingAnimationData.CurrentRightFreeBonePosition;
					}
					else
					{
						initialLocalFrame.origin = _ongoingAnimationData.CurrentCenterFreeBonePosition;
					}
					origin = initialLocalFrame.origin;
				}
			}
			freeBone.CurrentLocalFrame = initialLocalFrame;
			freeBone.CurrentFrameWithoutRandomWind = origin;
		}
	}

	private void FoldUnfoldSoundEventTick()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		if (_ongoingAnimationData.FoldUnfoldSoundEvent != null && _ongoingAnimationData.FoldUnfoldSoundEvent.IsPlaying())
		{
			SoundEvent foldUnfoldSoundEvent = _ongoingAnimationData.FoldUnfoldSoundEvent;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			foldUnfoldSoundEvent.SetPosition(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin);
		}
		if (_ongoingAnimationData.ShouldMakeFoldUnfoldSound)
		{
			int num = (_ongoingAnimationData.UnfoldIsOngoing ? SailUnfoldSoundEventId : SailFoldSoundEventId);
			if (_ongoingAnimationData.FoldUnfoldSoundEvent != null)
			{
				_ongoingAnimationData.FoldUnfoldSoundEvent.Stop();
				_ongoingAnimationData.FoldUnfoldSoundEvent = null;
			}
			_ongoingAnimationData.ShouldMakeFoldUnfoldSound = false;
			ref SailFoldProgress ongoingAnimationData = ref _ongoingAnimationData;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			ongoingAnimationData.FoldUnfoldSoundEvent = SoundEvent.CreateEvent(num, ((WeakGameEntity)(ref gameEntity)).Scene);
			SoundEvent foldUnfoldSoundEvent2 = _ongoingAnimationData.FoldUnfoldSoundEvent;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			foldUnfoldSoundEvent2.SetPosition(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin);
			_ongoingAnimationData.FoldUnfoldSoundEvent.Play();
		}
		if (_ongoingAnimationData.ShouldStopFoldUnfoldSound)
		{
			_ongoingAnimationData.FoldUnfoldSoundEvent.Stop();
			_ongoingAnimationData.FoldUnfoldSoundEvent = null;
			_ongoingAnimationData.ShouldStopFoldUnfoldSound = false;
		}
	}

	private void TickRopesAndPulleys()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b69: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_08dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08df: Unknown result type (might be due to invalid IL or missing references)
		//IL_065c: Unknown result type (might be due to invalid IL or missing references)
		//IL_065e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_092a: Unknown result type (might be due to invalid IL or missing references)
		//IL_091d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c02: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a32: Unknown result type (might be due to invalid IL or missing references)
		//IL_095d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d02: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cdb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c54: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c65: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a72: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0995: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_0714: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d28: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d43: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aaa: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0754: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aea: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = ((_yardEntity != (GameEntity)null) ? _yardEntity.GetGlobalFrame() : MatrixFrame.Identity);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec2 globalWindVelocityOfScene = ((WeakGameEntity)(ref gameEntity)).GetGlobalWindVelocityOfScene();
		((Vec2)(ref globalWindVelocityOfScene)).Normalize();
		bool flag = false;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity root = ((WeakGameEntity)(ref gameEntity)).Root;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref root)).GetGlobalFrame();
		flag = Vec2.DotProduct(globalWindVelocityOfScene, ((Vec3)(ref val.rotation.f)).AsVec2) < 0f;
		if (_knobParent != (GameEntity)null)
		{
			MatrixFrame globalFrame2 = _knobParent.GetGlobalFrame();
			for (int i = 0; i < _knobConnectionPoints.Count; i++)
			{
				KnobConnectionPoint value = _knobConnectionPoints[i];
				Vec3 val2 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref value.ShipLocalPosition);
				value.UpdateGlobalPosition(val2);
				bool value2 = Vec3.DotProduct(val2 - val.origin, val.rotation.f) > 0f;
				value.UpdateRightOfYard(value2);
				_knobConnectionPoints[i] = value;
			}
		}
		if (SailSkeletonEntity != (GameEntity)null && _yardEntity != (GameEntity)null)
		{
			MatrixFrame localFrame = SailYawRotationEntity.GetLocalFrame();
			if (!((MatrixFrame)(ref localFrame)).NearlyEquals(_previousYawEntityFrame, 0.0001f))
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MatrixFrame globalFrame3 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				MatrixFrame previousSailYardFrame = ((MatrixFrame)(ref globalFrame3)).TransformToLocalNonOrthogonal(ref val);
				foreach (SimpleRopeRecord simpleRope in _simpleRopes)
				{
					if (simpleRope.StartPointAttachedToYard)
					{
						MatrixFrame globalFrame4 = simpleRope.RopeEntity.GetGlobalFrame();
						Vec3 val3 = ((MatrixFrame)(ref globalFrame3)).TransformToLocalNonOrthogonal(ref globalFrame4.origin);
						Vec3 val4 = ((MatrixFrame)(ref _previousSailYardFrame)).TransformToLocalNonOrthogonal(ref val3);
						globalFrame4.origin = ((MatrixFrame)(ref previousSailYardFrame)).TransformToParent(ref val4);
						globalFrame4.origin = ((MatrixFrame)(ref globalFrame3)).TransformToParent(ref globalFrame4.origin);
						simpleRope.RopeEntity.SetGlobalFrame(ref globalFrame4, false);
					}
					if (simpleRope.EndPointAttachedToYard)
					{
						MatrixFrame globalFrame5 = simpleRope.TargetEntity.GetGlobalFrame();
						Vec3 val5 = ((MatrixFrame)(ref globalFrame3)).TransformToLocalNonOrthogonal(ref globalFrame5.origin);
						Vec3 val6 = ((MatrixFrame)(ref _previousSailYardFrame)).TransformToLocalNonOrthogonal(ref val5);
						globalFrame5.origin = ((MatrixFrame)(ref previousSailYardFrame)).TransformToParent(ref val6);
						globalFrame5.origin = ((MatrixFrame)(ref globalFrame3)).TransformToParent(ref globalFrame5.origin);
						simpleRope.TargetEntity.SetGlobalFrame(ref globalFrame5, false);
					}
				}
				foreach (FreeBoneRecord freeBone in _freeBones)
				{
					if (freeBone.FoldSailPulley.PulleySystem == null)
					{
						continue;
					}
					foreach (RopeSegment tiedToYardSegment in freeBone.FoldSailPulley.PulleySystem.TiedToYardSegments)
					{
						WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)tiedToYardSegment).GameEntity;
						MatrixFrame globalFrame6 = ((WeakGameEntity)(ref gameEntity2)).GetGlobalFrame();
						Vec3 val7 = ((MatrixFrame)(ref globalFrame3)).TransformToLocalNonOrthogonal(ref globalFrame6.origin);
						Vec3 val8 = ((MatrixFrame)(ref _previousSailYardFrame)).TransformToLocalNonOrthogonal(ref val7);
						Vec3 val9 = ((MatrixFrame)(ref previousSailYardFrame)).TransformToParent(ref val8);
						globalFrame6.origin = ((MatrixFrame)(ref globalFrame3)).TransformToParent(ref val9);
						gameEntity = ((ScriptComponentBehavior)tiedToYardSegment).GameEntity;
						((WeakGameEntity)(ref gameEntity)).SetGlobalFrame(ref globalFrame6, false);
					}
				}
				_previousSailYardFrame = previousSailYardFrame;
			}
			bool flag2 = !_ongoingAnimationData.FoldIsOngoing && !_ongoingAnimationData.UnfoldIsOngoing;
			MatrixFrame globalFrame7 = SailSkeletonEntity.GetGlobalFrame();
			foreach (FreeBoneRecord freeBone2 in _freeBones)
			{
				Vec3 origin = ((MatrixFrame)(ref globalFrame7)).TransformToParent(ref freeBone2.CurrentLocalFrame).origin;
				Vec3 val10 = ((MatrixFrame)(ref globalFrame7)).TransformToParent(ref freeBone2.CurrentFrameWithoutRandomWind);
				Vec3 shipLocalPosition = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref origin);
				if (freeBone2.ConnectionType == FreeBoneConnectionType.Closest)
				{
					_ = 1;
				}
				else
					_ = freeBone2.ConnectionType == FreeBoneConnectionType.ClosestTwo;
				if (freeBone2.FoldSailPulley.PulleySystem != null)
				{
					freeBone2.FoldSailPulley.PulleySystem.SetEndTargetPosition(origin);
					if (_ongoingAnimationData.FoldIsOngoing)
					{
						float num = MathF.Min(_ongoingAnimationData.CurrentProgress / _foldFreeBoneResetDuration, 1f);
						freeBone2.FoldSailPulley.PulleySystem.SetRuntimeLooseMultiplier(1f - num);
					}
					else if (_ongoingAnimationData.UnfoldIsOngoing)
					{
						float num2 = MathF.Clamp((_ongoingAnimationData.CurrentProgress - _unfoldSailDuration) / _foldFreeBoneResetDuration, 0f, 1f);
						freeBone2.FoldSailPulley.PulleySystem.SetRuntimeLooseMultiplier(1f - num2);
					}
					else
					{
						freeBone2.FoldSailPulley.PulleySystem.SetRuntimeLooseMultiplier(1f);
					}
				}
				if (freeBone2.RotatorPulleys != null)
				{
					foreach (PulleyDataCache rotatorPulley in freeBone2.RotatorPulleys)
					{
						if (rotatorPulley.PulleySystem != null)
						{
							rotatorPulley.PulleySystem.SetEndTargetPosition(origin);
						}
					}
					if (_knobConnectionPoints.Count > 1 && flag2)
					{
						if (freeBone2.RotatorPulleys.Count > 0)
						{
							(int, int) tuple = FindClosestTwoKnobPoint(val10, shipLocalPosition, _knobConnectionPoints, sideOfYard: true);
							if (tuple.Item1 != -1)
							{
								freeBone2.RotatorPulleys[0].PulleySystem.SetFirstFixedGlobalPosition(_knobConnectionPoints[tuple.Item1].GlobalPosition);
							}
							else
							{
								int num3 = FindClosestPointFallback(val10, _knobConnectionPoints);
								if (num3 != -1)
								{
									freeBone2.RotatorPulleys[0].PulleySystem.SetFirstFixedGlobalPosition(_knobConnectionPoints[num3].GlobalPosition);
								}
							}
							if (tuple.Item2 != -1)
							{
								freeBone2.RotatorPulleys[0].PulleySystem.SetFirstFreeGlobalPosition(_knobConnectionPoints[tuple.Item2].GlobalPosition);
							}
							else
							{
								int num4 = FindClosestPointFallback(val10, _knobConnectionPoints);
								if (num4 != -1)
								{
									freeBone2.RotatorPulleys[0].PulleySystem.SetFirstFreeGlobalPosition(_knobConnectionPoints[num4].GlobalPosition);
								}
							}
						}
						if (freeBone2.RotatorPulleys.Count > 1)
						{
							(int, int) tuple2 = FindClosestTwoKnobPoint(val10, shipLocalPosition, _knobConnectionPoints, sideOfYard: false);
							if (tuple2.Item1 != -1)
							{
								freeBone2.RotatorPulleys[1].PulleySystem.SetFirstFixedGlobalPosition(_knobConnectionPoints[tuple2.Item1].GlobalPosition);
							}
							else
							{
								int num5 = FindClosestPointFallback(val10, _knobConnectionPoints);
								if (num5 != -1)
								{
									freeBone2.RotatorPulleys[1].PulleySystem.SetFirstFixedGlobalPosition(_knobConnectionPoints[num5].GlobalPosition);
								}
							}
							if (tuple2.Item2 != -1)
							{
								freeBone2.RotatorPulleys[1].PulleySystem.SetFirstFreeGlobalPosition(_knobConnectionPoints[tuple2.Item2].GlobalPosition);
							}
							else
							{
								int num6 = FindClosestPointFallback(val10, _knobConnectionPoints);
								if (num6 != -1)
								{
									freeBone2.RotatorPulleys[1].PulleySystem.SetFirstFreeGlobalPosition(_knobConnectionPoints[num6].GlobalPosition);
								}
							}
							int num7 = ((!flag) ? 1 : 0);
							freeBone2.RotatorPulleys[num7].PulleySystem.SetRuntimeLooseMultiplier(0.0023f);
							freeBone2.RotatorPulleys[(num7 + 1) % 2].PulleySystem.SetRuntimeLooseMultiplier(0.1f);
						}
					}
				}
				if (freeBone2.StabilityPulleys != null)
				{
					foreach (PulleyDataCache stabilityPulley in freeBone2.StabilityPulleys)
					{
						if (stabilityPulley.PulleySystem != null)
						{
							stabilityPulley.PulleySystem.SetEndTargetPosition(val10);
							if (_ongoingAnimationData.FoldIsOngoing)
							{
								float num8 = MathF.Clamp(_ongoingAnimationData.CurrentProgress / (_foldFreeBoneResetDuration + _foldSailDuration), 0f, 1f);
								stabilityPulley.PulleySystem.SetRuntimeLooseMultiplier(0.5f * num8);
							}
							else if (_ongoingAnimationData.UnfoldIsOngoing)
							{
								float num9 = MathF.Clamp(_ongoingAnimationData.CurrentProgress / (_foldFreeBoneResetDuration + _unfoldSailDuration), 0f, 1f);
								stabilityPulley.PulleySystem.SetRuntimeLooseMultiplier(0.5f * (1f - num9));
							}
							else
							{
								stabilityPulley.PulleySystem.SetRuntimeLooseMultiplier(0.05f);
							}
						}
					}
					if (_knobConnectionPoints.Count > 1 && flag2)
					{
						if (freeBone2.StabilityPulleys.Count > 0)
						{
							(int, int) tuple3 = FindClosestTwoKnobPoint(val10, shipLocalPosition, _knobConnectionPoints, sideOfYard: true);
							if (tuple3.Item1 != -1)
							{
								freeBone2.StabilityPulleys[0].PulleySystem.SetFirstFixedGlobalPosition(_knobConnectionPoints[tuple3.Item1].GlobalPosition);
							}
							else
							{
								int num10 = FindClosestPointFallback(val10, _knobConnectionPoints);
								if (num10 != -1)
								{
									freeBone2.StabilityPulleys[0].PulleySystem.SetFirstFixedGlobalPosition(_knobConnectionPoints[num10].GlobalPosition);
								}
							}
							if (tuple3.Item2 != -1)
							{
								freeBone2.StabilityPulleys[0].PulleySystem.SetFirstFreeGlobalPosition(_knobConnectionPoints[tuple3.Item2].GlobalPosition);
							}
							else
							{
								int num11 = FindClosestPointFallback(val10, _knobConnectionPoints);
								if (num11 != -1)
								{
									freeBone2.StabilityPulleys[0].PulleySystem.SetFirstFreeGlobalPosition(_knobConnectionPoints[num11].GlobalPosition);
								}
							}
						}
						if (freeBone2.StabilityPulleys.Count > 1)
						{
							(int, int) tuple4 = FindClosestTwoKnobPoint(val10, shipLocalPosition, _knobConnectionPoints, sideOfYard: false);
							if (tuple4.Item1 != -1)
							{
								freeBone2.StabilityPulleys[1].PulleySystem.SetFirstFixedGlobalPosition(_knobConnectionPoints[tuple4.Item1].GlobalPosition);
							}
							else
							{
								int num12 = FindClosestPointFallback(val10, _knobConnectionPoints);
								if (num12 != -1)
								{
									freeBone2.StabilityPulleys[1].PulleySystem.SetFirstFixedGlobalPosition(_knobConnectionPoints[num12].GlobalPosition);
								}
							}
							if (tuple4.Item2 != -1)
							{
								freeBone2.StabilityPulleys[1].PulleySystem.SetFirstFreeGlobalPosition(_knobConnectionPoints[tuple4.Item2].GlobalPosition);
							}
							else
							{
								int num13 = FindClosestPointFallback(val10, _knobConnectionPoints);
								if (num13 != -1)
								{
									freeBone2.StabilityPulleys[1].PulleySystem.SetFirstFreeGlobalPosition(_knobConnectionPoints[num13].GlobalPosition);
								}
							}
							int num14 = ((!flag) ? 1 : 0);
							freeBone2.StabilityPulleys[num14].PulleySystem.SetRuntimeLooseMultiplier(0.0023f);
							freeBone2.StabilityPulleys[(num14 + 1) % 2].PulleySystem.SetRuntimeLooseMultiplier(0.1f);
						}
					}
				}
				if (freeBone2.StabilityRopes == null)
				{
					continue;
				}
				foreach (SimpleRopeRecord stabilityRope in freeBone2.StabilityRopes)
				{
					MatrixFrame globalFrame8 = stabilityRope.TargetEntity.GetGlobalFrame();
					globalFrame8.origin = origin;
					stabilityRope.TargetEntity.SetGlobalFrame(ref globalFrame8, false);
				}
				if (!(_knobConnectionPoints.Count > 0 && flag2))
				{
					continue;
				}
				if (freeBone2.StabilityRopes.Count > 0)
				{
					int num15 = FindClosestKnobPoint(val10, shipLocalPosition, _knobConnectionPoints, sideOfYard: true);
					if (num15 != -1)
					{
						MatrixFrame globalFrame9 = freeBone2.StabilityRopes[0].RopeEntity.GetGlobalFrame();
						globalFrame9.origin = _knobConnectionPoints[num15].GlobalPosition;
						freeBone2.StabilityRopes[0].RopeEntity.SetGlobalFrame(ref globalFrame9, true);
					}
					else
					{
						int num16 = FindClosestPointFallback(val10, _knobConnectionPoints);
						if (num16 != -1)
						{
							MatrixFrame globalFrame10 = freeBone2.StabilityRopes[0].RopeEntity.GetGlobalFrame();
							globalFrame10.origin = _knobConnectionPoints[num16].GlobalPosition;
							freeBone2.StabilityRopes[0].RopeEntity.SetGlobalFrame(ref globalFrame10, true);
						}
					}
				}
				if (freeBone2.StabilityRopes.Count <= 1)
				{
					continue;
				}
				int num17 = FindClosestKnobPoint(val10, shipLocalPosition, _knobConnectionPoints, sideOfYard: false);
				if (num17 != -1)
				{
					MatrixFrame globalFrame11 = freeBone2.StabilityRopes[1].RopeEntity.GetGlobalFrame();
					globalFrame11.origin = _knobConnectionPoints[num17].GlobalPosition;
					freeBone2.StabilityRopes[1].RopeEntity.SetGlobalFrame(ref globalFrame11, true);
				}
				else
				{
					int num18 = FindClosestPointFallback(val10, _knobConnectionPoints);
					if (num18 != -1)
					{
						MatrixFrame globalFrame12 = freeBone2.StabilityRopes[1].RopeEntity.GetGlobalFrame();
						globalFrame12.origin = _knobConnectionPoints[num18].GlobalPosition;
						freeBone2.StabilityRopes[1].RopeEntity.SetGlobalFrame(ref globalFrame12, true);
					}
				}
				int num19 = ((!flag) ? 1 : 0);
				freeBone2.StabilityRopes[num19].RopeSegment.SetRuntimeLooseMultiplier(0.005f);
				freeBone2.StabilityRopes[(num19 + 1) % 2].RopeSegment.SetRuntimeLooseMultiplier(0.2f);
			}
		}
		UpdatePreviousYardFrame();
	}

	private int FindClosestPointFallback(Vec3 position, List<KnobConnectionPoint> records)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		int result = -1;
		float num = 1E+12f;
		for (int i = 0; i < records.Count; i++)
		{
			Vec3 val = position - records[i].GlobalPosition;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			float lengthSquared = ((Vec2)(ref asVec)).LengthSquared;
			if (lengthSquared < num)
			{
				result = i;
				num = lengthSquared;
			}
		}
		return result;
	}

	private int FindClosestKnobPointWind(Vec3 position, Vec3 shipLocalPosition, List<KnobConnectionPoint> records, bool sideOfYard, Vec2 windDirection)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		int result = -1;
		float num = 0f;
		for (int i = 0; i < records.Count; i++)
		{
			if (records[i].RightOfYard == sideOfYard && (MathF.Sign(records[i].ShipLocalPosition.x) == MathF.Sign(shipLocalPosition.x) || _sailType == SailType.LateenSail))
			{
				Vec3 val = position - records[i].GlobalPosition;
				Vec2 asVec = ((Vec3)(ref val)).AsVec2;
				float length = ((Vec2)(ref asVec)).Length;
				((Vec3)(ref val)).Normalize();
				float num2 = MathF.Abs(Vec2.DotProduct(((Vec3)(ref val)).AsVec2, windDirection));
				if (num2 > num && length < _ropeConnectionMaxDistance)
				{
					result = i;
					num = num2;
				}
			}
		}
		return result;
	}

	private (int, int) FindClosestTwoKnobPointWind(Vec3 position, Vec3 shipLocalPosition, List<KnobConnectionPoint> records, bool sideOfYard, Vec2 windDirection)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		(int, int) result = (-1, -1);
		(float, float) tuple = (0f, 0f);
		for (int i = 0; i < records.Count; i++)
		{
			if (records[i].RightOfYard != sideOfYard || (MathF.Sign(records[i].ShipLocalPosition.x) != MathF.Sign(shipLocalPosition.x) && _sailType != SailType.LateenSail))
			{
				continue;
			}
			Vec3 val = position - records[i].GlobalPosition;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			float length = ((Vec2)(ref asVec)).Length;
			((Vec3)(ref val)).Normalize();
			float num = MathF.Abs(Vec2.DotProduct(((Vec3)(ref val)).AsVec2, windDirection));
			if (length < _ropeConnectionMaxDistance)
			{
				if (num > tuple.Item1)
				{
					tuple.Item2 = tuple.Item1;
					result.Item2 = result.Item1;
					tuple.Item1 = num;
					result.Item1 = i;
				}
				else if (num > tuple.Item2)
				{
					tuple.Item2 = num;
					result.Item2 = i;
				}
			}
		}
		return result;
	}

	private int FindClosestKnobPoint(Vec3 position, Vec3 shipLocalPosition, List<KnobConnectionPoint> records, bool sideOfYard)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		float num = _ropeConnectionMaxDistance * _ropeConnectionMaxDistance;
		int result = -1;
		float num2 = 1E+12f;
		for (int i = 0; i < records.Count; i++)
		{
			if (records[i].RightOfYard == sideOfYard && (MathF.Sign(records[i].ShipLocalPosition.x) == MathF.Sign(shipLocalPosition.x) || _sailType == SailType.LateenSail))
			{
				Vec3 val = position - records[i].GlobalPosition;
				float lengthSquared = ((Vec3)(ref val)).LengthSquared;
				Vec2 asVec = ((Vec3)(ref val)).AsVec2;
				float lengthSquared2 = ((Vec2)(ref asVec)).LengthSquared;
				if (lengthSquared < num2 && lengthSquared < num)
				{
					result = i;
					num2 = lengthSquared2;
				}
			}
		}
		return result;
	}

	private (int, int) FindClosestTwoKnobPoint(Vec3 position, Vec3 shipLocalPosition, List<KnobConnectionPoint> records, bool sideOfYard)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		float num = _ropeConnectionMaxDistance * _ropeConnectionMaxDistance;
		(int, int) result = (-1, -1);
		(float, float) tuple = (1E+12f, 1E+12f);
		for (int i = 0; i < records.Count; i++)
		{
			if (records[i].RightOfYard != sideOfYard || (MathF.Sign(records[i].ShipLocalPosition.x) != MathF.Sign(shipLocalPosition.x) && _sailType != SailType.LateenSail))
			{
				continue;
			}
			Vec3 val = position - records[i].GlobalPosition;
			float lengthSquared = ((Vec3)(ref val)).LengthSquared;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			if (((Vec2)(ref asVec)).LengthSquared < num)
			{
				if (lengthSquared < tuple.Item1)
				{
					tuple.Item2 = tuple.Item1;
					result.Item2 = result.Item1;
					tuple.Item1 = lengthSquared;
					result.Item1 = i;
				}
				else if (lengthSquared < tuple.Item2)
				{
					tuple.Item2 = lengthSquared;
					result.Item2 = i;
				}
			}
		}
		return result;
	}

	private void CheckFoldAnimationState(float dt)
	{
		if (!_ongoingAnimationData.FoldIsOngoing && !_ongoingAnimationData.UnfoldIsOngoing && !SailEnabled)
		{
			StartFoldAnimation();
		}
		if (HasFoldFinished() && !_ongoingAnimationData.UnfoldIsOngoing && SailEnabled)
		{
			StartUnfoldAnimation();
		}
		if (_ongoingAnimationData.FoldIsOngoing)
		{
			if (!HasFoldFinished() && SailEnabled)
			{
				CancelAnimation();
				TickUnfoldAnimation(dt);
			}
			else
			{
				TickFoldAnimation(dt);
			}
		}
		else if (_ongoingAnimationData.UnfoldIsOngoing)
		{
			if (!SailEnabled)
			{
				CancelAnimation();
				TickFoldAnimation(dt);
			}
			else
			{
				TickUnfoldAnimation(dt);
			}
		}
	}

	private void DisableMorphAnimation()
	{
		if ((NativeObject)(object)SailClothComponent != (NativeObject)null)
		{
			SailClothComponent.DisableMorphAnimation();
		}
		_lastMorphAnimKeySet = -1f;
	}

	private void SetMorphAnimToCloth(float currentMorphKey)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		if (_lastMorphAnimKeySet == currentMorphKey)
		{
			return;
		}
		if ((NativeObject)(object)SailClothComponent != (NativeObject)null)
		{
			SailClothComponent.SetMorphBuffer(currentMorphKey);
			int num = (int)currentMorphKey;
			int num2 = Math.Min(num + 1, _ongoingAnimationData.NumberOfMorphKeys - 1);
			float num3 = currentMorphKey - (float)num;
			if (_sailType == SailType.LateenSail)
			{
				if (_ongoingAnimationData.CenterVertexPositions != null)
				{
					Vec3 val = _ongoingAnimationData.CenterVertexPositions[num];
					Vec3 val2 = _ongoingAnimationData.CenterVertexPositions[num2];
					_ongoingAnimationData.CurrentCenterFreeBonePosition = Vec3.Lerp(val, val2, num3);
				}
			}
			else if (_sailType == SailType.SquareSail)
			{
				if (_ongoingAnimationData.LeftVertexPositions != null)
				{
					Vec3 val3 = _ongoingAnimationData.LeftVertexPositions[num];
					Vec3 val4 = _ongoingAnimationData.LeftVertexPositions[num2];
					_ongoingAnimationData.CurrentLeftFreeBonePosition = Vec3.Lerp(val3, val4, num3);
				}
				if (_ongoingAnimationData.RightVertexPositions != null)
				{
					Vec3 val5 = _ongoingAnimationData.RightVertexPositions[num];
					Vec3 val6 = _ongoingAnimationData.RightVertexPositions[num2];
					_ongoingAnimationData.CurrentRightFreeBonePosition = Vec3.Lerp(val5, val6, num3);
				}
			}
		}
		_lastMorphAnimKeySet = currentMorphKey;
	}

	private void TickLateenSail(float dt)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		if (_lateenRollDegrees < 1E-06f && _lateenYardShift < 1E-06f)
		{
			if (_lateenSailData.RollRotationEntity != (GameEntity)null)
			{
				MatrixFrame frame = _lateenSailData.RollRotationEntity.GetFrame();
				if (Math.Abs(((Mat3)(ref frame.rotation)).GetEulerAngles().y * 57.29578f - _lateenRollDegrees) > 0.001f)
				{
					float num = _lateenRollDegrees * (MathF.PI / 180f);
					frame.rotation = Mat3.Identity;
					ref Mat3 rotation = ref frame.rotation;
					Vec3 val = new Vec3(0f, num, 0f, -1f);
					((Mat3)(ref rotation)).ApplyEulerAngles(ref val);
					_lateenSailData.RollRotationEntity.SetFrame(ref frame, true);
				}
			}
			if (_lateenSailData.YardShiftEntity != (GameEntity)null)
			{
				MatrixFrame frame2 = _lateenSailData.YardShiftEntity.GetFrame();
				if (Math.Abs(frame2.origin.x - _lateenYardShift) > 0.001f)
				{
					frame2.origin.x = _lateenYardShift;
					_lateenSailData.YardShiftEntity.SetFrame(ref frame2, true);
				}
			}
		}
		else
		{
			if (!(_lateenSailData.RollRotationEntity != (GameEntity)null) || !(SailYawRotationEntity != (GameEntity)null) || !(_lateenSailData.YardShiftEntity != (GameEntity)null))
			{
				return;
			}
			WeakGameEntity gameEntity;
			if (_lateenSailData.RollRotationInProgress)
			{
				_lateenSailData.RollRotationRealDt += dt;
				float num2 = _lateenSailData.RollRotationRealDt * _lateenRollChangeAnimationStepMultiplier;
				num2 -= (float)(int)num2;
				float val2 = MathF.Lerp(0.35f, 2f, (float)Math.Pow(num2, 1.5), 1E-05f);
				val2 = Math.Min(val2, 1f);
				val2 = MathF.Clamp(val2 - 0.2f, 0f, 1f) * 1.6f;
				_lateenSailData.RollRotationAnimProgress += dt * val2 / _lateenRollChangeAnimationStepMultiplier;
				float num3 = MathF.Clamp(_lateenSailData.RollRotationAnimProgress / _lateenRollChangeAnimationDuration, 0f, 1f);
				float num4 = MathF.Lerp(_lateenSailData.RollRotationInitial, _lateenSailData.RollRotationTarget, num3, 1E-05f);
				MatrixFrame frame3 = _lateenSailData.RollRotationEntity.GetFrame();
				frame3.rotation = Mat3.Identity;
				ref Mat3 rotation2 = ref frame3.rotation;
				Vec3 val = new Vec3(0f, num4, 0f, -1f);
				((Mat3)(ref rotation2)).ApplyEulerAngles(ref val);
				_lateenSailData.RollRotationEntity.SetFrame(ref frame3, true);
				float num5 = _lateenRollChangeAnimationDuration - _lateenRollChangeYardShiftStart;
				float num6 = (float)Math.Pow(MathF.Clamp((_lateenSailData.RollRotationRealDt - num5) / _lateenRollChangeYardShiftDuration, 0f, 1f), _lateenRollChangeYardShiftAcceleration);
				float x = MathF.Lerp(_lateenSailData.YardShiftInitial, _lateenSailData.YardShiftTarget, num6, 1E-05f);
				MatrixFrame frame4 = _lateenSailData.YardShiftEntity.GetFrame();
				frame4.origin.x = x;
				_lateenSailData.YardShiftEntity.SetFrame(ref frame4, true);
				if (_lateenSailData.RollRotationAnimProgress >= _lateenRollChangeAnimationDuration && num6 >= 1f)
				{
					_lateenSailData.RollRotationInProgress = false;
				}
				if (_lateenSailData.RollAnimationSoundEvent != null)
				{
					if (_lateenSailData.RollRotationAnimProgress >= _lateenRollChangeAnimationDuration * 0.9f && num6 >= 0.1f)
					{
						_lateenSailData.RollAnimationSoundEvent.Stop();
						_lateenSailData.RollAnimationSoundEvent = null;
					}
					else
					{
						SoundEvent rollAnimationSoundEvent = _lateenSailData.RollAnimationSoundEvent;
						gameEntity = ((ScriptComponentBehavior)this).GameEntity;
						rollAnimationSoundEvent.SetPosition(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin);
					}
				}
				_ = _ongoingAnimationData;
				if (!_lateenSailData.RollRotationInProgress)
				{
					DisableMorphAnimation();
				}
				return;
			}
			float num7 = _lateenRollDegrees * (MathF.PI / 180f);
			float num8 = 0f;
			MatrixFrame frame5 = SailYawRotationEntity.GetFrame();
			float num9;
			for (num9 = ((Mat3)(ref frame5.rotation)).GetEulerAngles().z * 57.29578f; num9 > 180f; num9 -= 180f)
			{
			}
			for (; num9 < -180f; num9 += 180f)
			{
			}
			float num10 = _lateenRollChangeDegreeLimit - 90f;
			float num11 = 0f - _lateenRollChangeDegreeLimit - 90f;
			float num12 = 0f - _lateenRollChangeDegreeLimit + 90f;
			float num13 = _lateenRollChangeDegreeLimit + 90f;
			if (num9 < num11 || num9 > num13)
			{
				num8 = -1f;
			}
			else if (num9 > num10 && num9 < num12)
			{
				num8 = 1f;
			}
			frame5 = _lateenSailData.RollRotationEntity.GetFrame();
			float num14 = ((Mat3)(ref frame5.rotation)).GetEulerAngles().y * 57.29578f;
			float num15 = ((num14 > 0f) ? 1f : (-1f));
			if (num8 != 0f && num15 != num8)
			{
				_lateenSailData.RollRotationInProgress = true;
				_lateenSailData.RollRotationInitial = num14 * (MathF.PI / 180f);
				_lateenSailData.RollRotationTarget = num8 * num7;
				_lateenSailData.YardShiftInitial = _lateenSailData.YardShiftEntity.GetFrame().origin.x;
				_lateenSailData.YardShiftTarget = _lateenYardShift * num8;
				_lateenSailData.RollRotationAnimProgress = 0f;
				_lateenSailData.RollRotationRealDt = 0f;
				if (SoundsEnabled)
				{
					ref LateenSailData lateenSailData = ref _lateenSailData;
					int lateenSailRollSoundEventId = LateenSailRollSoundEventId;
					gameEntity = ((ScriptComponentBehavior)this).GameEntity;
					lateenSailData.RollAnimationSoundEvent = SoundEvent.CreateEvent(lateenSailRollSoundEventId, ((WeakGameEntity)(ref gameEntity)).Scene);
					_lateenSailData.RollAnimationSoundEvent.Play();
				}
			}
		}
	}

	private void SetClothMeshMaxDistance(float value)
	{
		if ((NativeObject)(object)SailClothComponent != (NativeObject)null)
		{
			SailClothComponent.SetMaxDistanceMultiplier(value);
		}
	}

	private void TickFoldAnimation(float dt)
	{
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		if (_ongoingAnimationData.CurrentProgress < _foldFreeBoneResetDuration || _ongoingAnimationData.CurrentProgress > _foldSailDuration + _foldFreeBoneResetDuration)
		{
			_ongoingAnimationData.CurrentProgress += dt;
		}
		else
		{
			_ongoingAnimationData.RealProgress += dt;
			if (_sailType == SailType.LateenSail || !FoldAnimationEnabled)
			{
				_ongoingAnimationData.CurrentProgress += dt;
			}
			else
			{
				_ongoingAnimationData.CurrentProgress += dt * ComputeSquareSailProgressMultiplier(_ongoingAnimationData.RealProgress);
			}
		}
		_ongoingAnimationData.CurrentProgress = Math.Min(_ongoingAnimationData.CurrentProgress, _foldSailDuration + _foldFreeBoneResetDuration + _foldedSailTransitionDuration);
		if (_ongoingAnimationData.CurrentProgress < _foldFreeBoneResetDuration)
		{
			return;
		}
		float num = (_ongoingAnimationData.CurrentProgress - _foldFreeBoneResetDuration) / _foldSailDuration;
		num = MathF.Clamp(num, 0f, 1f);
		float morphAnimToCloth = num * (float)(_ongoingAnimationData.NumberOfMorphKeys - 1);
		SetMorphAnimToCloth(morphAnimToCloth);
		float num2 = 0f;
		float num3 = 1f;
		float num4 = 1f - (num - num2) / MathF.Max(num3 - num2, 0.01f);
		num4 = MathF.Clamp(num4, 0f, 1f);
		SetClothMeshMaxDistance(num4);
		float num5 = 0f;
		float num6 = 0.75f;
		float num7 = 1f - (num - num5) / MathF.Max(num6 - num5, 0.01f);
		num7 = MathF.Clamp(num7, 0f, 1f);
		_foldAnimWindReductionFactor = num7;
		if (_ongoingAnimationData.FoldUnfoldSoundEvent != null && num > 0.875f)
		{
			_ongoingAnimationData.ShouldStopFoldUnfoldSound = true;
		}
		if (_ongoingAnimationData.CurrentProgress > _foldSailDuration + _foldFreeBoneResetDuration)
		{
			float num8 = (_ongoingAnimationData.CurrentProgress - (_foldSailDuration + _foldFreeBoneResetDuration)) / _foldedSailTransitionDuration;
			num8 = MathF.Clamp(num8, 0f, 1f);
			if (_foldedStaticSailEntity != (GameEntity)null)
			{
				if (!_isBurning)
				{
					_foldedStaticSailEntity.SetVisibilityExcludeParents(true);
				}
				_foldedStaticSailEntity.SetAlpha(num8 * _sailEntityAlpha);
				SailSkeletonEntity.SetAlpha(num8);
				if ((NativeObject)(object)_foldedStaticSailMesh != (NativeObject)null && !_isBurning)
				{
					_foldedStaticSailMesh.SetVectorArgument(1f, 0f, 0f, 0f);
				}
				SailClothComponent.SetVectorArgument(-1f, 0f, 0f, 0f);
				if (num8 >= 0.99999f)
				{
					SailSkeletonEntity.SetVisibilityExcludeParents(false);
				}
			}
			if (_currentSailLevelUsed != 3 || _sailType != SailType.SquareSail)
			{
				return;
			}
			if (num8 < 0.99999f)
			{
				foreach (WeakGameEntity topLateenSail in _topLateenSails)
				{
					WeakGameEntity current = topLateenSail;
					((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(true);
					((WeakGameEntity)(ref current)).SetAlpha(1f - num8);
				}
			}
			else
			{
				foreach (WeakGameEntity topLateenSail2 in _topLateenSails)
				{
					WeakGameEntity current2 = topLateenSail2;
					((WeakGameEntity)(ref current2)).SetVisibilityExcludeParents(false);
				}
			}
			{
				foreach (WeakGameEntity topLateenFoldedSail in _topLateenFoldedSails)
				{
					WeakGameEntity current3 = topLateenFoldedSail;
					((WeakGameEntity)(ref current3)).SetVisibilityExcludeParents(true);
					((WeakGameEntity)(ref current3)).SetAlpha(num8);
				}
				return;
			}
		}
		foreach (WeakGameEntity topLateenSail3 in _topLateenSails)
		{
			WeakGameEntity current4 = topLateenSail3;
			((WeakGameEntity)(ref current4)).SetVisibilityExcludeParents(true);
			((WeakGameEntity)(ref current4)).SetAlpha(1f);
		}
		foreach (WeakGameEntity topLateenFoldedSail2 in _topLateenFoldedSails)
		{
			WeakGameEntity current5 = topLateenFoldedSail2;
			((WeakGameEntity)(ref current5)).SetVisibilityExcludeParents(false);
			((WeakGameEntity)(ref current5)).SetAlpha(0f);
		}
	}

	private void TickUnfoldAnimation(float dt)
	{
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		_ongoingAnimationData.CurrentProgress += dt;
		_ongoingAnimationData.CurrentProgress = MathF.Min(_ongoingAnimationData.CurrentProgress, _unfoldSailDuration + _foldFreeBoneResetDuration + _foldedSailTransitionDuration);
		if (HasUnfoldFinished())
		{
			_ongoingAnimationData.CurrentProgress = 0f;
			_ongoingAnimationData.RealProgress = 0f;
			_ongoingAnimationData.UnfoldIsOngoing = false;
			_foldAnimWindReductionFactor = 1f;
			DisableMorphAnimation();
			SetClothMeshMaxDistance(1f);
			return;
		}
		if (_ongoingAnimationData.CurrentProgress < _foldedSailTransitionDuration)
		{
			float num = _ongoingAnimationData.CurrentProgress / _foldedSailTransitionDuration;
			num = MathF.Clamp(num, 0f, 1f) * _sailEntityAlpha;
			SailSkeletonEntity.SetVisibilityExcludeParents(true);
			SailSkeletonEntity.SetAlpha(num);
			SailClothComponent.SetVectorArgument(1f, 0f, 0f, 0f);
			if (_foldedStaticSailEntity != (GameEntity)null)
			{
				_foldedStaticSailEntity.SetVisibilityExcludeParents(true);
				_foldedStaticSailEntity.SetAlpha(num);
				if ((NativeObject)(object)_foldedStaticSailMesh != (NativeObject)null)
				{
					_foldedStaticSailMesh.SetVectorArgument(-1f, 0f, 0f, 0f);
				}
			}
			if (_currentSailLevelUsed != 3 || _sailType != SailType.SquareSail)
			{
				return;
			}
			if (num < 0.99999f)
			{
				foreach (WeakGameEntity topLateenSail in _topLateenSails)
				{
					WeakGameEntity current = topLateenSail;
					((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(true);
					((WeakGameEntity)(ref current)).SetAlpha(num);
				}
				{
					foreach (WeakGameEntity topLateenFoldedSail in _topLateenFoldedSails)
					{
						WeakGameEntity current2 = topLateenFoldedSail;
						((WeakGameEntity)(ref current2)).SetVisibilityExcludeParents(true);
						((WeakGameEntity)(ref current2)).SetAlpha(1f - num);
					}
					return;
				}
			}
			foreach (WeakGameEntity topLateenSail2 in _topLateenSails)
			{
				WeakGameEntity current3 = topLateenSail2;
				((WeakGameEntity)(ref current3)).SetVisibilityExcludeParents(false);
			}
			{
				foreach (WeakGameEntity topLateenFoldedSail2 in _topLateenFoldedSails)
				{
					WeakGameEntity current4 = topLateenFoldedSail2;
					((WeakGameEntity)(ref current4)).SetVisibilityExcludeParents(true);
					((WeakGameEntity)(ref current4)).SetAlpha(1f);
				}
				return;
			}
		}
		if (_foldedStaticSailEntity != (GameEntity)null)
		{
			_foldedStaticSailEntity.SetVisibilityExcludeParents(false);
		}
		foreach (WeakGameEntity topLateenFoldedSail3 in _topLateenFoldedSails)
		{
			WeakGameEntity current5 = topLateenFoldedSail3;
			((WeakGameEntity)(ref current5)).SetVisibilityExcludeParents(false);
			((WeakGameEntity)(ref current5)).SetAlpha(0f);
		}
		foreach (WeakGameEntity topLateenSail3 in _topLateenSails)
		{
			WeakGameEntity current6 = topLateenSail3;
			((WeakGameEntity)(ref current6)).SetVisibilityExcludeParents(true);
			((WeakGameEntity)(ref current6)).SetAlpha(1f);
		}
		SailSkeletonEntity.SetAlpha(_sailEntityAlpha);
		float num2 = MathF.Clamp((_ongoingAnimationData.CurrentProgress - _foldedSailTransitionDuration) / _unfoldSailDuration, 0f, 1f);
		if (num2 >= 1f)
		{
			if (_ongoingAnimationData.FoldUnfoldSoundEvent != null)
			{
				_ongoingAnimationData.ShouldStopFoldUnfoldSound = true;
			}
			DisableMorphAnimation();
		}
		else
		{
			float morphAnimToCloth = (1f - num2) * (float)(_ongoingAnimationData.NumberOfMorphKeys - 1);
			SetMorphAnimToCloth(morphAnimToCloth);
		}
		float num3 = 0f;
		float num4 = 1f;
		float num5 = 1f - (1f - num2 - num3) / MathF.Max(num4 - num3, 0.01f);
		num5 = MathF.Clamp(num5, 0f, 1f);
		SetClothMeshMaxDistance(num5);
		float num6 = 0.25f;
		float num7 = 1f;
		float num8 = 1f - (1f - num2 - num6) / MathF.Max(num7 - num6, 0.01f);
		num8 = MathF.Clamp(num8, 0f, 1f);
		_foldAnimWindReductionFactor = num8;
	}

	private void InitSailFoldAnimationResources()
	{
		if (!((NativeObject)(object)SailClothComponent != (NativeObject)null))
		{
			return;
		}
		_ongoingAnimationData.NumberOfMorphKeys = SailClothComponent.GetNumberOfMorphKeys();
		if (_ongoingAnimationData.NumberOfMorphKeys > 0)
		{
			if (_sailType == SailType.SquareSail)
			{
				_ongoingAnimationData.LeftVertexPositions = (Vec3[])(object)new Vec3[_ongoingAnimationData.NumberOfMorphKeys];
				SailClothComponent.GetMorphAnimLeftPoints(_ongoingAnimationData.LeftVertexPositions);
				_ongoingAnimationData.RightVertexPositions = (Vec3[])(object)new Vec3[_ongoingAnimationData.NumberOfMorphKeys];
				SailClothComponent.GetMorphAnimRightPoints(_ongoingAnimationData.RightVertexPositions);
			}
			else
			{
				_ongoingAnimationData.CenterVertexPositions = (Vec3[])(object)new Vec3[_ongoingAnimationData.NumberOfMorphKeys];
				SailClothComponent.GetMorphAnimCenterPoints(_ongoingAnimationData.CenterVertexPositions);
			}
		}
	}

	private void StartFoldAnimation()
	{
		_ongoingAnimationData.CurrentProgress = 0f;
		_ongoingAnimationData.RealProgress = 0f;
		_ongoingAnimationData.FoldIsOngoing = true;
		_ongoingAnimationData.UnfoldIsOngoing = false;
		if (SoundsEnabled)
		{
			_ongoingAnimationData.ShouldMakeFoldUnfoldSound = true;
		}
	}

	private void StartUnfoldAnimation()
	{
		_ongoingAnimationData.CurrentProgress = 0f;
		_ongoingAnimationData.RealProgress = 0f;
		_ongoingAnimationData.FoldIsOngoing = false;
		_ongoingAnimationData.UnfoldIsOngoing = true;
		if (SoundsEnabled)
		{
			_ongoingAnimationData.ShouldMakeFoldUnfoldSound = true;
		}
	}

	private void CancelAnimation()
	{
		if (_ongoingAnimationData.UnfoldIsOngoing)
		{
			float num = 0f;
			if (_ongoingAnimationData.CurrentProgress < _foldedSailTransitionDuration)
			{
				num = _foldSailDuration + _foldFreeBoneResetDuration + (_foldedSailTransitionDuration - _ongoingAnimationData.CurrentProgress);
			}
			else if (_ongoingAnimationData.CurrentProgress < _foldedSailTransitionDuration + _unfoldSailDuration)
			{
				float num2 = (_ongoingAnimationData.CurrentProgress - _foldedSailTransitionDuration) / _unfoldSailDuration;
				num = _foldFreeBoneResetDuration + _foldSailDuration * (1f - num2);
			}
			else
			{
				num = (_unfoldSailDuration + _foldFreeBoneResetDuration + _foldedSailTransitionDuration - _ongoingAnimationData.CurrentProgress) * _foldFreeBoneResetDuration / _foldedSailTransitionDuration;
			}
			StartFoldAnimation();
			_ongoingAnimationData.CurrentProgress = num;
		}
		else if (_ongoingAnimationData.FoldIsOngoing)
		{
			float num3 = 0f;
			if (_ongoingAnimationData.CurrentProgress < _foldFreeBoneResetDuration)
			{
				num3 = _unfoldSailDuration + _foldFreeBoneResetDuration + (_foldedSailTransitionDuration - _ongoingAnimationData.CurrentProgress);
			}
			else if (_ongoingAnimationData.CurrentProgress < _foldSailDuration + _foldFreeBoneResetDuration)
			{
				float num4 = (_ongoingAnimationData.CurrentProgress - _foldFreeBoneResetDuration) / _foldSailDuration;
				num3 = _foldedSailTransitionDuration + _unfoldSailDuration * (1f - num4);
			}
			else
			{
				num3 = (_foldSailDuration + _foldFreeBoneResetDuration + _foldedSailTransitionDuration - _ongoingAnimationData.CurrentProgress) * _foldedSailTransitionDuration / _foldFreeBoneResetDuration;
			}
			StartUnfoldAnimation();
			_ongoingAnimationData.CurrentProgress = num3;
		}
	}

	private bool HasFoldFinished()
	{
		return _ongoingAnimationData.CurrentProgress >= _foldSailDuration + _foldFreeBoneResetDuration + _foldedSailTransitionDuration;
	}

	private bool HasUnfoldFinished()
	{
		return _ongoingAnimationData.CurrentProgress >= _unfoldSailDuration + _foldFreeBoneResetDuration + _foldedSailTransitionDuration;
	}

	private void UpdateTotalFoldDuration()
	{
		_totalFoldDuration = _foldFreeBoneResetDuration + _foldedSailTransitionDuration;
		if (_sailType == SailType.LateenSail)
		{
			_totalFoldDuration += _foldSailDuration;
			return;
		}
		float num = EstimateSquareSailFoldAnimationDuration();
		_totalFoldDuration += num;
	}

	private void UpdateTotalUnfoldDuration()
	{
		_totalUnfoldDuration = _unfoldSailDuration + _foldFreeBoneResetDuration + _foldedSailTransitionDuration;
	}

	private void HandleLOD()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec3 lastFinalRenderCameraPositionOfScene = ((WeakGameEntity)(ref gameEntity)).GetLastFinalRenderCameraPositionOfScene();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
		float num = ((Vec3)(ref lastFinalRenderCameraPositionOfScene)).DistanceSquared(origin);
		_ropesAreInvisibleThisFrame = num > 22500f;
		bool flag = num > 2025f;
		if (_ropesWereInvisibleLastFrame != _ropesAreInvisibleThisFrame || _lodCheckFirstFrame)
		{
			foreach (FreeBoneRecord freeBone in _freeBones)
			{
				if (freeBone.FoldSailPulley.Entity != (GameEntity)null)
				{
					freeBone.FoldSailPulley.Entity.SetVisibilityExcludeParents(!_ropesAreInvisibleThisFrame);
				}
				if (freeBone.RotatorPulleys != null)
				{
					foreach (PulleyDataCache rotatorPulley in freeBone.RotatorPulleys)
					{
						rotatorPulley.Entity.SetVisibilityExcludeParents(!_ropesAreInvisibleThisFrame);
					}
				}
				if (freeBone.StabilityPulleys != null)
				{
					foreach (PulleyDataCache stabilityPulley in freeBone.StabilityPulleys)
					{
						stabilityPulley.Entity.SetVisibilityExcludeParents(!_ropesAreInvisibleThisFrame);
					}
				}
				if (freeBone.StabilityRopes == null)
				{
					continue;
				}
				foreach (SimpleRopeRecord stabilityRope in freeBone.StabilityRopes)
				{
					stabilityRope.ParentEntity.SetVisibilityExcludeParents(!_ropesAreInvisibleThisFrame);
				}
			}
			foreach (SimpleRopeRecord simpleRope in _simpleRopes)
			{
				if (!simpleRope.IsBigRope)
				{
					simpleRope.RopeEntity.SetVisibilityExcludeParents(!_ropesAreInvisibleThisFrame);
				}
			}
			foreach (SimpleRopeRecord mastRope in _mastRopes)
			{
				if (!mastRope.IsBigRope)
				{
					mastRope.RopeEntity.SetVisibilityExcludeParents(!_ropesAreInvisibleThisFrame);
				}
			}
		}
		if (_ropesWereLinearLastFrame != flag || _lodCheckFirstFrame)
		{
			foreach (FreeBoneRecord freeBone2 in _freeBones)
			{
				if (freeBone2.FoldSailPulley.Entity != (GameEntity)null)
				{
					freeBone2.FoldSailPulley.PulleySystem.SetLinearMode(flag);
				}
				if (freeBone2.RotatorPulleys != null)
				{
					foreach (PulleyDataCache rotatorPulley2 in freeBone2.RotatorPulleys)
					{
						rotatorPulley2.PulleySystem.SetLinearMode(flag);
					}
				}
				if (freeBone2.StabilityPulleys != null)
				{
					foreach (PulleyDataCache stabilityPulley2 in freeBone2.StabilityPulleys)
					{
						stabilityPulley2.PulleySystem.SetLinearMode(flag);
					}
				}
				if (freeBone2.StabilityRopes == null)
				{
					continue;
				}
				foreach (SimpleRopeRecord stabilityRope2 in freeBone2.StabilityRopes)
				{
					stabilityRope2.RopeSegment.SetLinearMode(flag);
				}
			}
			foreach (SimpleRopeRecord simpleRope2 in _simpleRopes)
			{
				simpleRope2.RopeSegment.SetLinearMode(flag);
			}
			foreach (SimpleRopeRecord mastRope2 in _mastRopes)
			{
				mastRope2.RopeSegment.SetLinearMode(flag);
			}
		}
		_ropesWereInvisibleLastFrame = _ropesAreInvisibleThisFrame;
		_ropesWereLinearLastFrame = flag;
		_lodCheckFirstFrame = false;
	}

	private float ComputeSquareSailProgressMultiplier(float progress)
	{
		float num = progress * _foldSailStepMultiplier;
		num -= (float)(int)num;
		return MathF.Clamp(Math.Min(MathF.Lerp(0f, 1f, num, 1E-05f), 1f) - 0.2f, 0f, 1f) * 1.6f / _foldSailStepMultiplier;
	}

	public float EstimateSquareSailFoldAnimationDuration()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0.01f;
		while (num < _foldSailDuration)
		{
			num += num3 * ComputeSquareSailProgressMultiplier(num2);
			num2 += num3;
			if (num2 > _foldSailDuration * 10f)
			{
				break;
			}
		}
		return num2;
	}

	private SimpleRopeRecord FillSimpleRopeRecord(WeakGameEntity parentEntity)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		SimpleRopeRecord result = new SimpleRopeRecord
		{
			StartPointAttachedToYard = false,
			EndPointAttachedToYard = false,
			ParentEntity = GameEntity.CreateFromWeakEntity(parentEntity),
			RopeSegment = null,
			IsBigRope = ((WeakGameEntity)(ref parentEntity)).HasTag("big_rope"),
			RopeEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref parentEntity)).GetFirstChildEntityWithTagRecursive("simple_rope_start"))
		};
		if (result.RopeEntity != (GameEntity)null)
		{
			result.StartPointAttachedToYard = result.RopeEntity.HasTag("attached_to_yard");
			result.RopeSegment = result.RopeEntity.GetFirstScriptOfType<RopeSegment>();
		}
		result.TargetEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref parentEntity)).GetFirstChildEntityWithTagRecursive("simple_rope_end"));
		if (result.TargetEntity != (GameEntity)null)
		{
			result.EndPointAttachedToYard = result.TargetEntity.HasTag("attached_to_yard");
		}
		if (result.RopeSegment != null)
		{
			result.RopeSegment.SetUseDistanceAsRopeLength();
		}
		return result;
	}

	private void PlaceClothFragmentsRandomly(int seed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		MBFastRandom val = new MBFastRandom();
		val.SetSeed((uint)seed, 0u);
		List<RopeSegment> segments = new List<RopeSegment>();
		float num = 0.04f;
		foreach (FreeBoneRecord freeBone in _freeBones)
		{
			if (freeBone.FoldSailPulley.PulleySystem != null)
			{
				freeBone.FoldSailPulley.PulleySystem.GetAllRopeSegments(ref segments, num);
			}
			if (freeBone.RotatorPulleys != null)
			{
				foreach (PulleyDataCache rotatorPulley in freeBone.RotatorPulleys)
				{
					rotatorPulley.PulleySystem.GetAllRopeSegments(ref segments, num);
				}
			}
			if (freeBone.StabilityPulleys != null)
			{
				foreach (PulleyDataCache stabilityPulley in freeBone.StabilityPulleys)
				{
					stabilityPulley.PulleySystem.GetAllRopeSegments(ref segments, num);
				}
			}
			if (freeBone.StabilityRopes == null)
			{
				continue;
			}
			foreach (SimpleRopeRecord stabilityRope in freeBone.StabilityRopes)
			{
				if ((NativeObject)(object)stabilityRope.RopeSegment.RopeMesh != (NativeObject)null && stabilityRope.RopeSegment.RopeMesh.GetVectorArgument().w < num)
				{
					segments.Add(stabilityRope.RopeSegment);
				}
			}
		}
		foreach (SimpleRopeRecord simpleRope in _simpleRopes)
		{
			if ((NativeObject)(object)simpleRope.RopeSegment.RopeMesh != (NativeObject)null && simpleRope.RopeSegment.RopeMesh.GetVectorArgument().w < num)
			{
				segments.Add(simpleRope.RopeSegment);
			}
		}
		for (int num2 = MathF.Min(6, segments.Count); num2 > 0; num2--)
		{
			int index = val.Next(0, segments.Count);
			RopeSegment ropeSegment = segments[index];
			int num3 = 2 + (int)(val.NextFloat() * 1.5f);
			for (int i = 0; i < num3; i++)
			{
				string text = ClothFragmentPrefabs[val.Next(0, ClothFragmentPrefabs.Count() - 1)];
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				GameEntity val2 = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, text, true, true, "");
				gameEntity = ((ScriptComponentBehavior)ropeSegment).GameEntity;
				((WeakGameEntity)(ref gameEntity)).AddChild(val2.WeakEntity, false);
				val2.EntityFlags = (EntityFlags)(val2.EntityFlags | 0x20000);
				float num4 = 1f + val.NextFloat() * 1f;
				MatrixFrame identity = MatrixFrame.Identity;
				((Mat3)(ref identity.rotation)).ApplyScaleLocal(num4);
				val2.SetLocalFrame(ref identity, false);
				RopeSegmentCosmetics firstScriptOfType = val2.GetFirstScriptOfType<RopeSegmentCosmetics>();
				if (firstScriptOfType != null)
				{
					firstScriptOfType.RopeLocalPosition = val.NextFloat();
				}
			}
			segments[index] = segments[segments.Count - 1];
			segments.RemoveAt(segments.Count - 1);
		}
	}

	private void FetchEntities()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Unknown result type (might be due to invalid IL or missing references)
		//IL_0666: Unknown result type (might be due to invalid IL or missing references)
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0690: Unknown result type (might be due to invalid IL or missing references)
		//IL_0695: Unknown result type (might be due to invalid IL or missing references)
		//IL_0697: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b94: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b99: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0767: Unknown result type (might be due to invalid IL or missing references)
		//IL_076c: Unknown result type (might be due to invalid IL or missing references)
		//IL_076e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0870: Unknown result type (might be due to invalid IL or missing references)
		//IL_0875: Unknown result type (might be due to invalid IL or missing references)
		//IL_0877: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b17: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b49: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0798: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a33: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a38: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a49: Unknown result type (might be due to invalid IL or missing references)
		//IL_070c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a88: Unknown result type (might be due to invalid IL or missing references)
		//IL_07de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0973: Unknown result type (might be due to invalid IL or missing references)
		//IL_0905: Unknown result type (might be due to invalid IL or missing references)
		bool flag = _yardEntity == (GameEntity)null;
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		bool flag2 = ((WeakGameEntity)(ref val)).Scene.IsEditorScene();
		_freeBones.Clear();
		_simpleRopes.Clear();
		_mastRopes.Clear();
		_knobConnectionPoints.Clear();
		SailSkeletonEntity = null;
		SailClothComponent = null;
		_sailSkeleton = null;
		_yardEntity = null;
		_foldedStaticSailEntity = null;
		_foldedStaticSailMesh = null;
		_burningSailEntity = null;
		_burningSailMesh = null;
		_topLateenSails.Clear();
		_topLateenFoldedSails.Clear();
		_ballistaVisibilityRopes.Clear();
		val = ((ScriptComponentBehavior)this).GameEntity;
		SailYawRotationEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTag("yaw_rotation_entity"));
		if (SailYawRotationEntity != (GameEntity)null)
		{
			WeakGameEntity weakEntity = SailYawRotationEntity.WeakEntity;
			if (_sailType == SailType.LateenSail)
			{
				_lateenSailData.RollRotationEntity = SailYawRotationEntity.GetFirstChildEntityWithTagRecursive("roll_rotation_entity");
				if (_lateenSailData.RollRotationEntity != (GameEntity)null)
				{
					_lateenSailData.YardShiftEntity = _lateenSailData.RollRotationEntity.GetFirstChildEntityWithTag("yard_shift");
					if (_lateenSailData.YardShiftEntity != (GameEntity)null)
					{
						weakEntity = _lateenSailData.YardShiftEntity.WeakEntity;
					}
				}
			}
			val = ((ScriptComponentBehavior)this).GameEntity;
			WeakGameEntity firstChildEntityWithTagRecursive = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("mast_entity");
			_mastEntity = GameEntity.CreateFromWeakEntity(firstChildEntityWithTagRecursive);
			if (((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).IsValid)
			{
				val = ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).Parent;
				if (((WeakGameEntity)(ref val)).IsValid)
				{
					WeakGameEntity parent = ((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).Parent;
					foreach (WeakGameEntity child in ((WeakGameEntity)(ref parent)).GetChildren())
					{
						WeakGameEntity current = child;
						if (((WeakGameEntity)(ref current)).HasTag("simple_rope"))
						{
							SimpleRopeRecord item = FillSimpleRopeRecord(current);
							_mastRopes.Add(item);
						}
					}
				}
			}
			SailSkeletonEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref weakEntity)).GetFirstChildEntityWithTag("sail_mesh_entity"));
			if (SailSkeletonEntity != (GameEntity)null)
			{
				_sailSkeleton = SailSkeletonEntity.Skeleton;
				if ((NativeObject)(object)_sailSkeleton != (NativeObject)null)
				{
					GameEntityComponent componentAtIndex = _sailSkeleton.GetComponentAtIndex((ComponentType)3, 0);
					SailClothComponent = (ClothSimulatorComponent)(object)((componentAtIndex is ClothSimulatorComponent) ? componentAtIndex : null);
					_sailSkeleton.EnableScriptDrivenPostIntegrateCallback();
					if ((NativeObject)(object)_sailSkeleton != (NativeObject)null)
					{
						val = ((ScriptComponentBehavior)this).GameEntity;
						((WeakGameEntity)(ref val)).Scene.AddAlwaysRenderedSkeleton(_sailSkeleton);
					}
				}
			}
			_burningSailEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref weakEntity)).GetFirstChildEntityWithTag("sail_mesh_free_entity"));
			if (_burningSailEntity != (GameEntity)null && (NativeObject)(object)_burningSailEntity.Skeleton != (NativeObject)null)
			{
				GameEntityComponent componentAtIndex2 = _burningSailEntity.Skeleton.GetComponentAtIndex((ComponentType)3, 0);
				ClothSimulatorComponent val2 = (ClothSimulatorComponent)(object)((componentAtIndex2 is ClothSimulatorComponent) ? componentAtIndex2 : null);
				if ((NativeObject)(object)val2 != (NativeObject)null)
				{
					_burningSailMesh = ((GameEntityComponent)val2).GetFirstMetaMesh().GetMeshAtIndex(0);
				}
			}
			val = ((ScriptComponentBehavior)this).GameEntity;
			SailTopBannerEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("bd_banner_b"));
			if (SailTopBannerEntity != (GameEntity)null)
			{
				SailTopBannerClothComponent = SailTopBannerEntity.GetClothSimulator(0);
				SailTopBannerEntity.SetDoNotCheckVisibility(true);
			}
			_yardEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref weakEntity)).GetFirstChildEntityWithTag("sail_yard"));
			_foldedStaticSailEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref weakEntity)).GetFirstChildEntityWithTag("folded_static_entity"));
			((WeakGameEntity)(ref weakEntity)).GetChildrenWithTagRecursive(_topLateenSails, "lvl3_lateens_entity");
			((WeakGameEntity)(ref weakEntity)).GetChildrenWithTagRecursive(_topLateenFoldedSails, "lvl3_lateens_folded");
			if (_foldedStaticSailEntity != (GameEntity)null)
			{
				_foldedStaticSailMesh = _foldedStaticSailEntity.GetFirstMesh();
			}
			if (_yardEntity != (GameEntity)null)
			{
				val = _yardEntity.WeakEntity;
				WeakGameEntity firstChildEntityWithTagRecursive2 = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("yard_mesh");
				_yardMesh = ((firstChildEntityWithTagRecursive2 != (GameEntity)null) ? ((WeakGameEntity)(ref firstChildEntityWithTagRecursive2)).GetFirstMesh() : null);
			}
			if (flag && _yardEntity != (GameEntity)null)
			{
				UpdatePreviousYardFrame();
				MatrixFrame globalFrame = _yardEntity.GetGlobalFrame();
				val = ((ScriptComponentBehavior)this).GameEntity;
				MatrixFrame globalFrame2 = ((WeakGameEntity)(ref val)).GetGlobalFrame();
				_previousSailYardFrame = ((MatrixFrame)(ref globalFrame2)).TransformToLocalNonOrthogonal(ref globalFrame);
			}
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		if (SailSkeletonEntity != (GameEntity)null)
		{
			Skeleton skeleton = SailSkeletonEntity.Skeleton;
			foreach (GameEntity child2 in SailSkeletonEntity.GetChildren())
			{
				if (!child2.HasTag("free_bone") || dictionary.ContainsKey(child2.Name))
				{
					continue;
				}
				FreeBoneRecord freeBoneRecord = new FreeBoneRecord();
				freeBoneRecord.InitialLocalFrame = child2.GetFrame();
				freeBoneRecord.CurrentLocalFrame = freeBoneRecord.InitialLocalFrame;
				freeBoneRecord.BoneIndex = -1;
				freeBoneRecord.ConnectionType = FreeBoneConnectionType.All;
				freeBoneRecord.Entity = child2;
				freeBoneRecord.FoldSailPulley.Entity = null;
				freeBoneRecord.FoldSailPulley.PulleySystem = null;
				if (child2.HasTag("closest_pulley"))
				{
					freeBoneRecord.ConnectionType = FreeBoneConnectionType.Closest;
				}
				else if (child2.HasTag("closest_two_pulleys"))
				{
					freeBoneRecord.ConnectionType = FreeBoneConnectionType.ClosestTwo;
				}
				if (_sailType == SailType.SquareSail)
				{
					if (child2.Name.Contains("_l"))
					{
						freeBoneRecord.BoneType = FreeBoneType.Left;
					}
					else if (child2.Name.Contains("_r"))
					{
						freeBoneRecord.BoneType = FreeBoneType.Right;
					}
				}
				else if (_sailType == SailType.LateenSail)
				{
					if (child2.Name.Contains("_l"))
					{
						freeBoneRecord.BoneType = FreeBoneType.Left;
					}
					else if (child2.Name.Contains("_r"))
					{
						freeBoneRecord.BoneType = FreeBoneType.Right;
					}
					else if (child2.Name.Contains("_c"))
					{
						freeBoneRecord.BoneType = FreeBoneType.Center;
					}
				}
				if ((NativeObject)(object)skeleton != (NativeObject)null)
				{
					string name = child2.Name;
					for (int i = 0; i < skeleton.GetBoneCount(); i++)
					{
						if (skeleton.GetBoneName((sbyte)i) == name)
						{
							freeBoneRecord.InitialLocalFrame = skeleton.GetBoneEntitialRestFrame((sbyte)i, false);
							freeBoneRecord.BoneIndex = (sbyte)i;
							break;
						}
					}
				}
				dictionary.Add(child2.Name, _freeBones.Count);
				_freeBones.Add(freeBoneRecord);
			}
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTag("pulley_systems_parent");
		if (firstChildEntityWithTag != (GameEntity)null)
		{
			((WeakGameEntity)(ref firstChildEntityWithTag)).SetDoNotCheckVisibility(true);
			WeakGameEntity firstChildEntityWithTag2 = ((WeakGameEntity)(ref firstChildEntityWithTag)).GetFirstChildEntityWithTag("sail_fold_pulleys");
			if (firstChildEntityWithTag2 != (GameEntity)null)
			{
				((WeakGameEntity)(ref firstChildEntityWithTag2)).SetDoNotCheckVisibility(true);
				foreach (WeakGameEntity child3 in ((WeakGameEntity)(ref firstChildEntityWithTag2)).GetChildren())
				{
					WeakGameEntity current3 = child3;
					string[] tags = ((WeakGameEntity)(ref current3)).Tags;
					foreach (string text in tags)
					{
						if (text != "fold_pulley_system")
						{
							int value = -1;
							if (dictionary.TryGetValue(text, out value))
							{
								_freeBones[value].FoldSailPulley.Entity = GameEntity.CreateFromWeakEntity(current3);
								_freeBones[value].FoldSailPulley.PulleySystem = ((WeakGameEntity)(ref current3)).GetFirstScriptOfType<PulleySystem>();
								break;
							}
						}
					}
				}
			}
			WeakGameEntity firstChildEntityWithTag3 = ((WeakGameEntity)(ref firstChildEntityWithTag)).GetFirstChildEntityWithTag("sail_rotate_pulleys");
			if (firstChildEntityWithTag3 != (GameEntity)null)
			{
				((WeakGameEntity)(ref firstChildEntityWithTag3)).SetDoNotCheckVisibility(true);
				foreach (WeakGameEntity child4 in ((WeakGameEntity)(ref firstChildEntityWithTag3)).GetChildren())
				{
					WeakGameEntity current4 = child4;
					string[] tags = ((WeakGameEntity)(ref current4)).Tags;
					foreach (string text2 in tags)
					{
						if (!(text2 != "pulley_system"))
						{
							continue;
						}
						int value2 = -1;
						if (dictionary.TryGetValue(text2, out value2))
						{
							PulleyDataCache item2 = new PulleyDataCache
							{
								Entity = GameEntity.CreateFromWeakEntity(current4),
								PulleySystem = ((WeakGameEntity)(ref current4)).GetFirstScriptOfType<PulleySystem>()
							};
							if (_freeBones[value2].RotatorPulleys == null)
							{
								_freeBones[value2].RotatorPulleys = new List<PulleyDataCache>();
							}
							_freeBones[value2].RotatorPulleys.Add(item2);
							break;
						}
					}
				}
			}
			WeakGameEntity firstChildEntityWithTag4 = ((WeakGameEntity)(ref firstChildEntityWithTag)).GetFirstChildEntityWithTag("stability_ropes_parent");
			if (firstChildEntityWithTag4 != (GameEntity)null)
			{
				((WeakGameEntity)(ref firstChildEntityWithTag4)).SetDoNotCheckVisibility(true);
				foreach (WeakGameEntity child5 in ((WeakGameEntity)(ref firstChildEntityWithTag4)).GetChildren())
				{
					WeakGameEntity current5 = child5;
					bool flag3 = ((WeakGameEntity)(ref current5)).HasTag("simple_rope");
					string text3 = (flag3 ? "simple_rope" : "pulley_system");
					string[] tags = ((WeakGameEntity)(ref current5)).Tags;
					foreach (string text4 in tags)
					{
						if (!(text4 != text3))
						{
							continue;
						}
						int value3 = -1;
						if (!dictionary.TryGetValue(text4, out value3))
						{
							continue;
						}
						if (flag3)
						{
							SimpleRopeRecord item3 = FillSimpleRopeRecord(current5);
							if (item3.RopeSegment != null)
							{
								item3.RopeSegment.SetAsDynamic();
							}
							if (_freeBones[value3].StabilityRopes == null)
							{
								_freeBones[value3].StabilityRopes = new List<SimpleRopeRecord>();
							}
							_freeBones[value3].StabilityRopes.Add(item3);
						}
						else
						{
							PulleyDataCache item4 = new PulleyDataCache
							{
								Entity = GameEntity.CreateFromWeakEntity(current5),
								PulleySystem = ((WeakGameEntity)(ref current5)).GetFirstScriptOfType<PulleySystem>()
							};
							if (_freeBones[value3].StabilityPulleys == null)
							{
								_freeBones[value3].StabilityPulleys = new List<PulleyDataCache>();
							}
							_freeBones[value3].StabilityPulleys.Add(item4);
						}
						break;
					}
				}
			}
			WeakGameEntity firstChildEntityWithTag5 = ((WeakGameEntity)(ref firstChildEntityWithTag)).GetFirstChildEntityWithTag("static_ropes_parent");
			if (firstChildEntityWithTag5 != (GameEntity)null)
			{
				((WeakGameEntity)(ref firstChildEntityWithTag5)).SetDoNotCheckVisibility(true);
				foreach (WeakGameEntity child6 in ((WeakGameEntity)(ref firstChildEntityWithTag5)).GetChildren())
				{
					WeakGameEntity current6 = child6;
					if (((WeakGameEntity)(ref current6)).HasTag("simple_rope"))
					{
						SimpleRopeRecord item5 = FillSimpleRopeRecord(current6);
						if (item5.RopeSegment != null)
						{
							item5.RopeSegment.SetUseDistanceAsRopeLength();
						}
						_simpleRopes.Add(item5);
					}
					if (((WeakGameEntity)(ref current6)).HasTag("ballista_visibility"))
					{
						_ballistaVisibilityRopes.Add(current6);
					}
				}
			}
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity root = ((WeakGameEntity)(ref val)).Root;
		_knobParent = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref root)).GetFirstChildEntityWithTagRecursive("knob_points_parent"));
		if (_knobParent != (GameEntity)null)
		{
			MatrixFrame globalFrame3 = ((WeakGameEntity)(ref root)).GetGlobalFrame();
			List<WeakGameEntity> list = new List<WeakGameEntity>();
			val = _knobParent.WeakEntity;
			((WeakGameEntity)(ref val)).GetChildrenWithTagRecursive(list, "knot_point");
			foreach (WeakGameEntity item7 in list)
			{
				WeakGameEntity current7 = item7;
				KnobConnectionPoint item6 = default(KnobConnectionPoint);
				item6.GlobalPosition = ((WeakGameEntity)(ref current7)).GetGlobalFrame().origin;
				item6.ShipLocalPosition = ((MatrixFrame)(ref globalFrame3)).TransformToLocalNonOrthogonal(ref item6.GlobalPosition);
				item6.IsFixed = ((WeakGameEntity)(ref current7)).HasTag("dynamic_knob");
				_knobConnectionPoints.Add(item6);
				if (!flag2)
				{
					((WeakGameEntity)(ref current7)).Remove(79);
				}
			}
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTagRecursive3 = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("flag_capture_rope");
		if (firstChildEntityWithTagRecursive3 != (GameEntity)null)
		{
			_topFlagRope = FillSimpleRopeRecord(firstChildEntityWithTagRecursive3);
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).SetHasCustomBoundingBoxValidationSystem(true);
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).SetBoundingboxDirty();
	}

	private void InitLateenSailData()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		if (!(SailYawRotationEntity == (GameEntity)null) && !(_lateenSailData.RollRotationEntity == (GameEntity)null) && !(_lateenSailData.YardShiftEntity == (GameEntity)null))
		{
			MatrixFrame frame = SailYawRotationEntity.GetFrame();
			float z = ((Mat3)(ref frame.rotation)).GetEulerAngles().z;
			if (z > _lateenRollChangeDegreeLimit)
			{
				float num = ((z > 0.01f) ? 1f : (-1f));
				float num2 = _lateenRollDegrees * (MathF.PI / 180f);
				float num3 = num * num2;
				MatrixFrame frame2 = _lateenSailData.RollRotationEntity.GetFrame();
				frame2.rotation = Mat3.Identity;
				ref Mat3 rotation = ref frame2.rotation;
				Vec3 val = new Vec3(0f, num3, 0f, -1f);
				((Mat3)(ref rotation)).ApplyEulerAngles(ref val);
				_lateenSailData.RollRotationEntity.SetFrame(ref frame2, true);
				MatrixFrame frame3 = _lateenSailData.YardShiftEntity.GetFrame();
				frame3.origin.x = num * _lateenYardShift;
				_lateenSailData.YardShiftEntity.SetFrame(ref frame3, true);
			}
		}
	}

	private void UpdatePreviousYardFrame()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_previousYawEntityFrame = SailYawRotationEntity.GetLocalFrame();
	}

	private void TickFire(float dt)
	{
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_060b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		_burningRecord.FireDt += dt;
		float num = _burningRecord.FireDt / _burningAnimationDuration;
		bool burningFinished = true;
		foreach (BurningSystem sailFire in _burningRecord.SailFires)
		{
			sailFire.Tick(dt);
		}
		if ((NativeObject)(object)SailClothComponent != (NativeObject)null)
		{
			float num2 = 0.99f + MathF.Min(num, 1f) * 0.55f;
			if (SailEnabled)
			{
				if ((NativeObject)(object)_burningSailMesh != (NativeObject)null)
				{
					_burningSailMesh.SetVectorArgument(num2, 0f, 0f, 0f);
				}
			}
			else
			{
				_foldedStaticSailMesh.SetVectorArgument(num2, 0f, 0f, 0f);
			}
			if (num2 < 1.52f)
			{
				burningFinished = false;
			}
		}
		if ((NativeObject)(object)_topLateenFireMaterial != (NativeObject)null && _currentSailLevelUsed == 3 && _sailType == SailType.SquareSail)
		{
			float num3 = 0.99f + num * 1.01f;
			foreach (WeakGameEntity topLateenSail in _topLateenSails)
			{
				WeakGameEntity current = topLateenSail;
				foreach (Mesh item in ((WeakGameEntity)(ref current)).GetAllMeshesWithTag("faction_color"))
				{
					item.SetVectorArgument(num3, 0f, 0f, 0f);
				}
			}
			foreach (WeakGameEntity topLateenFoldedSail in _topLateenFoldedSails)
			{
				WeakGameEntity current2 = topLateenFoldedSail;
				((WeakGameEntity)(ref current2)).SetDoNotCheckVisibility(true);
				foreach (Mesh item2 in ((WeakGameEntity)(ref current2)).GetAllMeshesWithTag("faction_color"))
				{
					item2.SetVectorArgument(num3, 0f, 0f, 0f);
				}
			}
		}
		foreach (BurningSystem sailFire2 in _burningRecord.SailFires)
		{
			MBReadOnlyList<BurningNode> burningNodes = sailFire2.BurningNodes;
			int num4 = (int)((num - 0.2f) * (float)((List<BurningNode>)(object)burningNodes).Count);
			for (int i = 0; i < num4 && i < ((List<BurningNode>)(object)burningNodes).Count; i++)
			{
				((List<BurningNode>)(object)burningNodes)[i].CurrentFireProgress = 0f;
			}
		}
		if (_burningRecord.MastFire != null)
		{
			_burningRecord.MastFire.Tick(dt);
			float flameProgress = _burningRecord.MastFire.GetFlameProgress();
			Color val = Color.Lerp(_burningRecord.InitialYardMastColor, _burningRecord.InitialYardMastColor * 0.75f, flameProgress);
			val.Alpha = 1f;
			_mastEntity.SetFactorColor(((Color)(ref val)).ToUnsignedInteger());
			float num5 = _burningAnimationDuration * 9f;
			float num6 = _burningRecord.FireDt / num5;
			float num7 = MathF.Clamp(1f - (num6 - 0.75f) * 4f, 0f, 1f);
			_burningRecord.MastFire.SetExternalFlameMultiplier(num7);
			if (num7 > 0f)
			{
				_burningRecord.MastFire.CheckWater();
				burningFinished = false;
			}
		}
		if (_burningRecord.FireDt > _burningAnimationDuration * 0.5f)
		{
			if (_burningRecord.YardFireStartDt == 0f)
			{
				_burningRecord.YardFireStartDt = _burningRecord.FireDt;
			}
			float num8 = 0f;
			if (_burningRecord.YardRightFire != null)
			{
				_burningRecord.YardRightFire.Tick(dt);
				num8 = Math.Max(num8, _burningRecord.YardRightFire.GetFlameProgress());
				float num9 = _burningAnimationDuration * 9f;
				float num10 = (_burningRecord.FireDt - _burningRecord.YardFireStartDt) / num9;
				float num11 = MathF.Clamp(1f - (num10 - 0.75f) * 4f, 0f, 1f);
				_burningRecord.YardRightFire.SetExternalFlameMultiplier(num11);
				if (num11 > 0f)
				{
					_burningRecord.YardRightFire.CheckWater();
					burningFinished = false;
				}
			}
			if (_burningRecord.YardLeftFire != null)
			{
				_burningRecord.YardLeftFire.Tick(dt);
				num8 = Math.Max(num8, _burningRecord.YardLeftFire.GetFlameProgress());
				float num12 = _burningAnimationDuration * 9f;
				float num13 = (_burningRecord.FireDt - _burningRecord.YardFireStartDt) / num12;
				float num14 = MathF.Clamp(1f - (num13 - 0.75f) * 4f, 0f, 1f);
				_burningRecord.YardLeftFire.SetExternalFlameMultiplier(num14);
				if (num14 > 0f)
				{
					_burningRecord.YardLeftFire.CheckWater();
					burningFinished = false;
				}
			}
			foreach (BurningSystem staticRopeFire in _burningRecord.StaticRopeFires)
			{
				staticRopeFire.Tick(dt);
				if (staticRopeFire.BurnedRope != null)
				{
					float num15 = (_burningRecord.FireDt - _burningRecord.YardFireStartDt) / staticRopeFire.GetBurningAnimationDuration();
					float num16 = 1f - (num15 - 0.4f);
					staticRopeFire.BurnedRope.SetAlpha(MathF.Max(num16, 0.01f));
					staticRopeFire.SetExternalFlameMultiplier(num16);
					if (num16 > 0f)
					{
						burningFinished = false;
						staticRopeFire.CheckWater();
					}
				}
			}
			if ((NativeObject)(object)_yardMesh != (NativeObject)null)
			{
				Color val2 = Color.Lerp(_burningRecord.InitialYardMastColor, _burningRecord.InitialYardMastColor * 0.75f, num8);
				val2.Alpha = 1f;
				_yardMesh.Color = ((Color)(ref val2)).ToUnsignedInteger();
			}
		}
		if (_burningRecord.FireDt > _burningAnimationDuration * 0.25f)
		{
			foreach (BurningSystem rotatorFire in _burningRecord.RotatorFires)
			{
				rotatorFire.Tick(dt);
				if (rotatorFire.BurnedPulley != null)
				{
					float num17 = _burningRecord.FireDt / rotatorFire.GetBurningAnimationDuration();
					float num18 = 1f - (num17 - 0.4f);
					rotatorFire.BurnedPulley.SetAlpha(MathF.Max(num18, 0.01f));
					rotatorFire.SetExternalFlameMultiplier(num18);
					if (num18 > 0f)
					{
						rotatorFire.CheckWater();
						burningFinished = false;
					}
				}
			}
		}
		foreach (BurningSystem foldFire in _burningRecord.FoldFires)
		{
			foldFire.Tick(dt);
			if (foldFire.BurnedPulley != null)
			{
				float num19 = _burningRecord.FireDt / foldFire.GetBurningAnimationDuration();
				float num20 = 1f - (num19 - 0.4f);
				foldFire.BurnedPulley.SetAlpha(MathF.Max(num20, 0.01f));
				foldFire.SetExternalFlameMultiplier(num20);
				if (num20 > 0f)
				{
					foldFire.CheckWater();
					burningFinished = false;
				}
			}
		}
		foreach (BurningSystem stabilizerFire in _burningRecord.StabilizerFires)
		{
			stabilizerFire.Tick(dt);
			if (stabilizerFire.BurnedRope != null)
			{
				float num21 = _burningRecord.FireDt / stabilizerFire.GetBurningAnimationDuration();
				float num22 = 1f - (num21 - 0.4f);
				stabilizerFire.BurnedRope.SetAlpha(MathF.Max(num22, 0.01f));
				stabilizerFire.SetExternalFlameMultiplier(num22);
				if (num22 > 0f)
				{
					stabilizerFire.CheckWater();
					burningFinished = false;
				}
			}
		}
		_burningRecord.BurningFinished = burningFinished;
	}

	private void PositionSailFireParticles()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.Zero;
		Vec3 val2 = Vec3.Zero;
		Vec3 val3 = Vec3.Zero;
		Vec3 val4 = Vec3.Zero;
		foreach (FreeBoneRecord freeBone in _freeBones)
		{
			if (freeBone.BoneIndex != -1)
			{
				if (freeBone.BoneType == FreeBoneType.Left)
				{
					val = freeBone.CurrentLocalFrame.origin;
					val3 = freeBone.InitialLocalFrame.origin;
				}
				else if (freeBone.BoneType == FreeBoneType.Right)
				{
					val2 = freeBone.CurrentLocalFrame.origin;
					val4 = freeBone.InitialLocalFrame.origin;
				}
			}
		}
		MatrixFrame globalFrame = SailSkeletonEntity.GetGlobalFrame();
		Vec3 val5 = default(Vec3);
		((Vec3)(ref val5))._002Ector((0f - _burningRecord.SailLengthX) * 0.5f, 0f, _burningRecord.SailLengthZ * 0.5f, -1f);
		Vec3 val6 = new Vec3(_burningRecord.SailLengthX * 0.5f, 0f, _burningRecord.SailLengthZ * 0.5f, -1f);
		Vec3 val7 = val5 - val3;
		Vec3 val8 = val6 - val5;
		Vec3 val9 = val - val3;
		Vec3 val10 = val2 - val4;
		_ = 1f / (float)_burningRecord.SailFires.Count;
		float num = 3.45f;
		float num2 = 0.62f;
		foreach (BurningSystem sailFire in _burningRecord.SailFires)
		{
			foreach (BurningNode item in (List<BurningNode>)(object)sailFire.BurningNodes)
			{
				Vec3 val11 = val3;
				Vec2 sailStripLocation = item.SailStripLocation;
				val11 += val8 * sailStripLocation.x;
				val11 += val7 * sailStripLocation.y;
				sailStripLocation.y = 1f - sailStripLocation.y;
				Vec3 val12 = Vec3.Lerp(val9, val10, sailStripLocation.x);
				Vec3 zero = Vec3.Zero;
				if (sailStripLocation.y > num2)
				{
					float num3 = 1f - (sailStripLocation.y - num2) / MathF.Max(1f - num2, 0.01f);
					zero += val12 * (1f + num * num3);
				}
				else
				{
					float num4 = sailStripLocation.y / MathF.Max(num2, 0.01f);
					zero += val12 * num * num4;
				}
				if (sailStripLocation.x > 0.5f)
				{
					float num5 = 1f - (sailStripLocation.x - 0.5f) / 0.5f;
					zero += val12 * 1.83f * num5;
				}
				else
				{
					float num6 = sailStripLocation.x / 0.5f;
					zero += val12 * 1.83f * num6;
				}
				val11 += zero;
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				globalFrame2.origin = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val11);
				gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				((WeakGameEntity)(ref gameEntity)).SetGlobalFrame(ref globalFrame2, true);
			}
		}
	}

	private void PlaceTopFlag(float dx)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (_topFlagRope.RopeSegment != null && SailTopBannerEntity != (GameEntity)null)
		{
			Vec3 origin = _topFlagRope.RopeEntity.GetGlobalFrame().origin;
			Vec3 origin2 = _topFlagRope.TargetEntity.GetGlobalFrame().origin;
			Vec3 val = RopeSegment.CalculateAutoCurvePosition(origin, origin2, _topFlagRope.RopeSegment.CurrentRopeLength, dx);
			MatrixFrame localFrame = SailTopBannerEntity.GetLocalFrame();
			MatrixFrame globalFrame = SailTopBannerEntity.Parent.GetGlobalFrame();
			Vec3 val2 = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref val);
			localFrame.origin.z = val2.z;
			SailTopBannerEntity.SetLocalFrame(ref localFrame, false);
		}
	}

	private void TickFlagCaptureAnimation(float dt)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		_captureTheFlagAnimation.DtTillStart += dt;
		WeakGameEntity gameEntity;
		if (_captureTheFlagAnimation.DtTillStart < 4f)
		{
			float num = MathF.Clamp(_captureTheFlagAnimation.DtTillStart / 4f, 0f, 1f);
			float dx = MathF.Lerp(_topFlagRopePosition, _captureTheFlagBottomPosition, num, 1E-05f);
			PlaceTopFlag(dx);
			_captureTheFlagAnimation.BannerWindFactor = 0.15f;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).IsInEditorScene())
			{
				Vec3 val = new Vec3(((ScriptComponentBehavior)this).Scene.GetGlobalWindVelocity() * 0.15f, 0f, -1f) / Scene.MaximumWindSpeed;
				SailTopBannerClothComponent.SetForcedWind(val, false);
			}
		}
		else if (_captureTheFlagAnimation.DtTillStart < 5f)
		{
			if ((NativeObject)(object)SailTopBannerClothComponent != (NativeObject)null && !_captureTheFlagAnimation.MaterialSet)
			{
				Mesh meshAtIndex = ((GameEntityComponent)SailTopBannerClothComponent).GetFirstMetaMesh().GetMeshAtIndex(0);
				Material material = meshAtIndex.GetMaterial();
				material = material.CreateCopy();
				material.SetTexture((MBTextureType)1, _captureTheFlagAnimation.NewBannerTexture);
				meshAtIndex.SetMaterial(material);
				_captureTheFlagAnimation.MaterialSet = true;
			}
			_captureTheFlagAnimation.BannerWindFactor = 0.15f;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).IsInEditorScene())
			{
				Vec3 val2 = new Vec3(((ScriptComponentBehavior)this).Scene.GetGlobalWindVelocity() * 0.15f, 0f, -1f) / Scene.MaximumWindSpeed;
				SailTopBannerClothComponent.SetForcedWind(val2, false);
			}
		}
		else if (_captureTheFlagAnimation.DtTillStart < 9f)
		{
			float num2 = MathF.Clamp((_captureTheFlagAnimation.DtTillStart - 5f) / 4f, 0f, 1f);
			float dx2 = MathF.Lerp(_captureTheFlagBottomPosition, _topFlagRopePosition, num2, 1E-05f);
			PlaceTopFlag(dx2);
			float num3 = MathF.Clamp((num2 - 0.8f) / 0.2f, 0.15f, 1f);
			_captureTheFlagAnimation.BannerWindFactor = num3;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).IsInEditorScene())
			{
				Vec3 val3 = new Vec3(((ScriptComponentBehavior)this).Scene.GetGlobalWindVelocity() * num3, 0f, -1f) / Scene.MaximumWindSpeed;
				SailTopBannerClothComponent.SetForcedWind(val3, false);
			}
		}
		else
		{
			_captureTheFlagAnimation.AnimationInProgress = false;
			_captureTheFlagAnimation.BannerWindFactor = 1f;
			SailTopBannerClothComponent.DisableForcedWind();
		}
	}

	public bool IsBurningFinished()
	{
		return _burningRecord.BurningFinished;
	}

	public bool IsBurning()
	{
		return _isBurning;
	}

	public void StartFire()
	{
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_074a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0751: Unknown result type (might be due to invalid IL or missing references)
		//IL_078b: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0581: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_087f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0886: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0800: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0935: Unknown result type (might be due to invalid IL or missing references)
		//IL_093a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e3e: Unknown result type (might be due to invalid IL or missing references)
		if (_isBurning)
		{
			foreach (BurningSystem sailFire in _burningRecord.SailFires)
			{
				sailFire.Remove();
			}
			if (_burningRecord.YardLeftFire != null)
			{
				_burningRecord.YardLeftFire.Remove();
			}
			if (_burningRecord.YardRightFire != null)
			{
				_burningRecord.YardRightFire.Remove();
			}
			if (_burningRecord.MastFire != null)
			{
				_burningRecord.MastFire.Remove();
			}
			foreach (BurningSystem rotatorFire in _burningRecord.RotatorFires)
			{
				rotatorFire.Remove();
			}
			foreach (BurningSystem stabilizerFire in _burningRecord.StabilizerFires)
			{
				stabilizerFire.Remove();
			}
			foreach (BurningSystem foldFire in _burningRecord.FoldFires)
			{
				foldFire.Remove();
			}
			foreach (BurningSystem staticRopeFire in _burningRecord.StaticRopeFires)
			{
				staticRopeFire.Remove();
			}
		}
		_isBurning = true;
		_burningRecord = new BurningRecord(_: true);
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		Scene scene = ((WeakGameEntity)(ref val)).Scene;
		bool flag = false;
		if (SailSkeletonEntity != (GameEntity)null && (NativeObject)(object)_sailSkeleton != (NativeObject)null && flag)
		{
			MatrixFrame globalFrame = SailSkeletonEntity.GetGlobalFrame();
			Mesh val2 = null;
			using (IEnumerator<Mesh> enumerator2 = _sailSkeleton.GetAllMeshes().GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					val2 = enumerator2.Current;
				}
			}
			if ((NativeObject)(object)val2 != (NativeObject)null)
			{
				int num = 6;
				Vec3 boundingBoxMax = val2.GetBoundingBoxMax();
				Vec3 boundingBoxMin = val2.GetBoundingBoxMin();
				Vec3 val3 = boundingBoxMax - boundingBoxMin;
				float num2 = 1f / (float)num;
				float num3 = 1f / (float)num;
				_burningRecord.SailLengthX = val3.x;
				_burningRecord.SailLengthZ = val3.z;
				string text = "burning_node";
				float num4 = _burningAnimationDuration / (float)num;
				for (int i = 0; i < num; i++)
				{
					GameEntity val4 = GameEntity.CreateEmpty(scene, true, true, true);
					val4.Name = $"sail_strip_root_{i}";
					SailSkeletonEntity.AddChild(val4, false);
					BurningSystem burningSystem = new BurningSystem(val4, 1f / num4);
					for (int j = 0; j < num; j++)
					{
						Vec2 zero = Vec2.Zero;
						zero.x = ((float)i + 0.1f + MBRandom.RandomFloat * 0.8f) * num2;
						zero.y = ((float)j + 0.1f + MBRandom.RandomFloat * 0.8f) * num3;
						float x = zero.x * val3.x + boundingBoxMin.x;
						float z = zero.y * (0f - val3.z) + boundingBoxMax.z;
						val = ((ScriptComponentBehavior)this).GameEntity;
						GameEntity val5 = GameEntity.Instantiate(((WeakGameEntity)(ref val)).Scene, text, true, true, "");
						val5.EntityFlags = (EntityFlags)(val5.EntityFlags | 0x20000);
						val4.AddChild(val5, false);
						MatrixFrame identity = MatrixFrame.Identity;
						identity.origin.x = x;
						identity.origin.z = z;
						identity = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref identity);
						val5.SetGlobalFrame(ref identity, true);
						val5.UpdateTriadFrameForEditor();
						BurningNode firstScriptOfType = val5.GetFirstScriptOfType<BurningNode>();
						if (firstScriptOfType != null)
						{
							firstScriptOfType.SetSailStripLocation(zero);
							burningSystem.AddNewNode(firstScriptOfType);
							if (MBRandom.RandomFloat > 0.82f)
							{
								firstScriptOfType.EnableSparks();
							}
						}
					}
					_burningRecord.SailFires.Add(burningSystem);
				}
			}
		}
		if (_mastEntity != (GameEntity)null)
		{
			MatrixFrame globalFrame2 = _mastEntity.GetGlobalFrame();
			Mesh firstMesh = _mastEntity.GetFirstMesh();
			if ((NativeObject)(object)firstMesh != (NativeObject)null)
			{
				GameEntity val6 = GameEntity.CreateEmpty(scene, true, true, true);
				val6.Name = "mastFireRoot";
				_mastEntity.AddChild(val6, false);
				float num5 = _burningAnimationDuration * 0.25f;
				_burningRecord.InitialYardMastColor = Color.FromUint(firstMesh.Color);
				_burningRecord.InitialYardMastColor.Alpha = 1f;
				Vec3 boundingBoxMin2 = firstMesh.GetBoundingBoxMin();
				Vec3 boundingBoxMax2 = firstMesh.GetBoundingBoxMax();
				Vec3 val7 = default(Vec3);
				((Vec3)(ref val7))._002Ector(0f, 0f, boundingBoxMax2.z, -1f);
				Vec3 val8 = default(Vec3);
				((Vec3)(ref val8))._002Ector(0f, 0f, boundingBoxMin2.z, -1f);
				float num6 = val8.z + 4.35f;
				val = ((ScriptComponentBehavior)this).GameEntity;
				val = ((WeakGameEntity)(ref val)).Root;
				WeakGameEntity firstChildEntityWithTagRecursive = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("body_mesh");
				if (firstChildEntityWithTagRecursive != (GameEntity)null)
				{
					Vec3 val9 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref val7);
					Vec3 val10 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref val8);
					Vec3 val11 = val9 - val10;
					float num7 = ((Vec3)(ref val11)).Normalize();
					float num8 = -1f;
					if (((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).RayHitEntity(val7, val11, num7, ref num8))
					{
						Vec3 val12 = val7 + val11 * num8;
						val8 = ((MatrixFrame)(ref globalFrame2)).TransformToLocalNonOrthogonal(ref val12);
						num6 = val8.z + 3f;
					}
				}
				Vec3 val13 = val8 - val7;
				float num9 = ((Vec3)(ref val13)).Normalize();
				float num10 = 2f;
				int num11 = (int)(num9 / num10);
				num11 = MathF.Max(0, num11 - 2);
				float num12 = num5 / (float)num11;
				_burningRecord.MastFire = new BurningSystem(val6, 1f / num12);
				string text2 = "burning_node_yard";
				for (int k = 0; k < num11; k++)
				{
					val = ((ScriptComponentBehavior)this).GameEntity;
					GameEntity val14 = GameEntity.Instantiate(((WeakGameEntity)(ref val)).Scene, text2, true, true, "");
					if (!(val14 == (GameEntity)null))
					{
						val6.AddChild(val14, false);
						BurningNode firstScriptOfType2 = val14.GetFirstScriptOfType<BurningNode>();
						if (firstScriptOfType2 != null)
						{
							_burningRecord.MastFire.AddNewNode(firstScriptOfType2);
						}
						if (MBRandom.RandomFloat > 0.82f)
						{
							firstScriptOfType2.EnableSparks();
						}
						MatrixFrame identity2 = MatrixFrame.Identity;
						identity2.origin.z = num6 + (float)k * num10;
						((Mat3)(ref identity2.rotation)).RotateAboutForward(MathF.PI / 2f);
						val14.SetFrame(ref identity2, true);
					}
				}
			}
		}
		if ((NativeObject)(object)_yardMesh != (NativeObject)null)
		{
			_mastEntity.GetGlobalFrame();
			Vec3 boundingBoxMin3 = _yardMesh.GetBoundingBoxMin();
			Vec3 boundingBoxMax3 = _yardMesh.GetBoundingBoxMax();
			Vec3 val15 = (boundingBoxMin3 + boundingBoxMax3) * 0.5f;
			string text3 = "burning_node_yard";
			GameEntity val16 = GameEntity.CreateEmpty(scene, true, true, true);
			val16.Name = "mastFireRootLeft";
			if (_sailType == SailType.LateenSail)
			{
				_lateenSailData.RollRotationEntity.AddChild(val16, false);
			}
			else
			{
				_yardEntity.AddChild(val16, false);
			}
			float num13 = 2f;
			int num14 = (int)((val15.x - boundingBoxMin3.x) / num13);
			float num15 = _burningAnimationDuration * 0.25f / (float)num14;
			_burningRecord.YardLeftFire = new BurningSystem(val16, 1f / num15);
			float y = val15.y;
			for (int l = 0; l < num14; l++)
			{
				val = ((ScriptComponentBehavior)this).GameEntity;
				GameEntity val17 = GameEntity.Instantiate(((WeakGameEntity)(ref val)).Scene, text3, true, true, "");
				if (!(val17 == (GameEntity)null))
				{
					val16.AddChild(val17, false);
					BurningNode firstScriptOfType3 = val17.GetFirstScriptOfType<BurningNode>();
					if (firstScriptOfType3 != null)
					{
						_burningRecord.YardLeftFire.AddNewNode(firstScriptOfType3);
					}
					if (MBRandom.RandomFloat > 0.62f)
					{
						firstScriptOfType3.EnableSparks();
					}
					MatrixFrame identity3 = MatrixFrame.Identity;
					identity3.origin.x = y - (float)l * num13;
					val17.SetFrame(ref identity3, true);
				}
			}
			GameEntity val18 = GameEntity.CreateEmpty(scene, true, true, true);
			val18.Name = "mastFireRootRight";
			if (_sailType == SailType.LateenSail)
			{
				_lateenSailData.RollRotationEntity.AddChild(val18, false);
			}
			else
			{
				_yardEntity.AddChild(val18, false);
			}
			float num16 = 2f;
			int num17 = (int)((boundingBoxMax3.x - val15.x) / num16);
			float num18 = _burningAnimationDuration * 0.25f / (float)num17;
			_burningRecord.YardRightFire = new BurningSystem(val18, 1f / num18);
			float y2 = val15.y;
			for (int m = 0; m < num17; m++)
			{
				val = ((ScriptComponentBehavior)this).GameEntity;
				GameEntity val19 = GameEntity.Instantiate(((WeakGameEntity)(ref val)).Scene, text3, true, true, "");
				if (!(val19 == (GameEntity)null))
				{
					val18.AddChild(val19, false);
					BurningNode firstScriptOfType4 = val19.GetFirstScriptOfType<BurningNode>();
					if (firstScriptOfType4 != null)
					{
						_burningRecord.YardRightFire.AddNewNode(firstScriptOfType4);
					}
					if (MBRandom.RandomFloat > 0.62f)
					{
						firstScriptOfType4.EnableSparks();
					}
					MatrixFrame identity4 = MatrixFrame.Identity;
					identity4.origin.x = y2 + (float)m * num16;
					val19.SetFrame(ref identity4, true);
				}
			}
		}
		_burningRecord.RotatorFires = new List<BurningSystem>();
		foreach (FreeBoneRecord freeBone in _freeBones)
		{
			if (freeBone.RotatorPulleys != null)
			{
				foreach (PulleyDataCache rotatorPulley in freeBone.RotatorPulleys)
				{
					BurningSystem burningSystem2 = new BurningSystem(null, 2.7f, rotatorPulley.PulleySystem);
					_burningRecord.RotatorFires.Add(burningSystem2);
					rotatorPulley.PulleySystem.FillBurningRecord(burningSystem2);
					float num19 = _burningAnimationDuration * 0.5f / (float)((List<BurningNode>)(object)burningSystem2.BurningNodes).Count;
					burningSystem2.SpreadRate = 1f / num19;
				}
			}
			if (freeBone.StabilityPulleys != null)
			{
				foreach (PulleyDataCache stabilityPulley in freeBone.StabilityPulleys)
				{
					BurningSystem burningSystem3 = new BurningSystem(null, 2.7f, stabilityPulley.PulleySystem);
					_burningRecord.StabilizerFires.Add(burningSystem3);
					stabilityPulley.PulleySystem.FillBurningRecord(burningSystem3);
					float num20 = _burningAnimationDuration * 0.5f / (float)((List<BurningNode>)(object)burningSystem3.BurningNodes).Count;
					burningSystem3.SpreadRate = 1f / num20;
				}
			}
			if (freeBone.FoldSailPulley.PulleySystem != null)
			{
				BurningSystem burningSystem4 = new BurningSystem(null, 4.7f, freeBone.FoldSailPulley.PulleySystem);
				_burningRecord.FoldFires.Add(burningSystem4);
				freeBone.FoldSailPulley.PulleySystem.FillBurningRecord(burningSystem4);
				float num21 = _burningAnimationDuration * 0.5f / (float)((List<BurningNode>)(object)burningSystem4.BurningNodes).Count;
				burningSystem4.SpreadRate = 1f / num21;
			}
			if (freeBone.StabilityRopes == null)
			{
				continue;
			}
			float nodeLength = 2f;
			string prefabName = "burning_node_rope";
			foreach (SimpleRopeRecord stabilityRope in freeBone.StabilityRopes)
			{
				BurningSystem burningSystem5 = new BurningSystem(null, 1.2f, stabilityRope.RopeSegment);
				stabilityRope.RopeSegment.FillBurningRecordForSegment(burningSystem5, prefabName, nodeLength, reversePlacement: true);
				stabilityRope.RopeSegment.BurnedClipReverseMode = true;
				if (((List<BurningNode>)(object)burningSystem5.BurningNodes).Count > 0)
				{
					_burningRecord.StabilizerFires.Add(burningSystem5);
					float num22 = _burningAnimationDuration * 0.5f / (float)((List<BurningNode>)(object)burningSystem5.BurningNodes).Count;
					burningSystem5.SpreadRate = 1f / num22;
				}
			}
		}
		float nodeLength2 = 2f;
		string prefabName2 = "burning_node_rope";
		foreach (SimpleRopeRecord simpleRope in _simpleRopes)
		{
			if (!(MBRandom.RandomFloat < 0.3f))
			{
				BurningSystem burningSystem6 = new BurningSystem(null, 1.4f, simpleRope.RopeSegment);
				simpleRope.RopeSegment.FillBurningRecordForSegment(burningSystem6, prefabName2, nodeLength2, reversePlacement: false);
				if (((List<BurningNode>)(object)burningSystem6.BurningNodes).Count > 0)
				{
					_burningRecord.StaticRopeFires.Add(burningSystem6);
					float num23 = _burningAnimationDuration * 0.5f / (float)((List<BurningNode>)(object)burningSystem6.BurningNodes).Count;
					burningSystem6.SpreadRate = 1f / num23;
				}
			}
		}
		if (SailEnabled)
		{
			if (_burningSailEntity != (GameEntity)null)
			{
				_burningSailEntity.SetVisibilityExcludeParents(true);
				SailSkeletonEntity.SetVisibilityExcludeParents(false);
			}
		}
		else if ((NativeObject)(object)_burningSailMesh != (NativeObject)null)
		{
			_foldedStaticSailMesh.SetMaterial(_burningSailMesh.GetMaterial());
			foreach (Mesh item in _foldedStaticSailEntity.GetAllMeshesWithTag("static_ropes"))
			{
				item.SetVisibilityMask((VisibilityMaskFlags)0);
			}
		}
		if (!((NativeObject)(object)_topLateenFireMaterial != (NativeObject)null) || _currentSailLevelUsed != 3 || _sailType != SailType.SquareSail)
		{
			return;
		}
		foreach (WeakGameEntity topLateenSail in _topLateenSails)
		{
			WeakGameEntity current6 = topLateenSail;
			((WeakGameEntity)(ref current6)).SetDoNotCheckVisibility(true);
			foreach (Mesh item2 in ((WeakGameEntity)(ref current6)).GetAllMeshesWithTag("faction_color"))
			{
				item2.SetMaterial(_topLateenFireMaterial);
			}
		}
		foreach (WeakGameEntity topLateenFoldedSail in _topLateenFoldedSails)
		{
			WeakGameEntity current7 = topLateenFoldedSail;
			((WeakGameEntity)(ref current7)).SetDoNotCheckVisibility(true);
			foreach (Mesh item3 in ((WeakGameEntity)(ref current7)).GetAllMeshesWithTag("faction_color"))
			{
				item3.SetMaterial(_topLateenFireMaterial);
			}
		}
	}

	private void ComputeMastClipPlane()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		WeakGameEntity firstChildEntityWithTagRecursive = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("body_mesh");
		if (firstChildEntityWithTagRecursive != (GameEntity)null && _mastEntity != (GameEntity)null)
		{
			float num = 30f;
			MatrixFrame globalFrame = _mastEntity.GetGlobalFrame();
			Vec3 u = globalFrame.rotation.u;
			Vec3 val2 = globalFrame.origin - num * u;
			float num2 = -1f;
			if (((WeakGameEntity)(ref firstChildEntityWithTagRecursive)).RayHitEntity(val2, u, num * 2f, ref num2))
			{
				_mastClipDistanceFromOrigin = num - num2;
			}
		}
	}

	private void UpdateMastClipPlane()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (_mastEntity != (GameEntity)null)
		{
			MatrixFrame globalFrame = _mastEntity.GetGlobalFrame();
			Vec3 val = globalFrame.origin - globalFrame.rotation.u * _mastClipDistanceFromOrigin;
			_mastEntity.SetCustomClipPlane(val, globalFrame.rotation.u, false);
		}
	}

	public void GetDimensions(in MatrixFrame shipFrame, bool isLateen, out float width, out float height, out Vec3 center)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = SailSkeletonEntity.GetGlobalFrame();
		Vec3 scaleVector = ((Mat3)(ref globalFrame.rotation)).GetScaleVector();
		BoundingBox boundingBox = ((GameEntityComponent)SailClothComponent).GetFirstMetaMesh().GetBoundingBox();
		Vec3 val = boundingBox.max - boundingBox.min;
		width = val.x * scaleVector.x;
		height = val.z * scaleVector.z;
		if (isLateen)
		{
			height = MathF.Sqrt(width * width + height * height) * 0.88f;
		}
		MatrixFrame val2 = shipFrame;
		MatrixFrame val3 = ((MatrixFrame)(ref val2)).TransformToLocalNonOrthogonal(ref globalFrame);
		center = ((MatrixFrame)(ref val3)).TransformToParent(ref boundingBox.center);
	}

	public void SetBallistaRopeVisibility(bool value)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (value)
		{
			_ballistaRopeEnableFrameCounter = 2;
			return;
		}
		foreach (WeakGameEntity ballistaVisibilityRope in _ballistaVisibilityRopes)
		{
			WeakGameEntity current = ballistaVisibilityRope;
			((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(value);
		}
		_ballistaRopeEnableFrameCounter = 0;
	}

	public void StartFlagCaptureAnimation(Texture newTexture)
	{
		if ((NativeObject)(object)SailTopBannerClothComponent != (NativeObject)null && (NativeObject)(object)((GameEntityComponent)SailTopBannerClothComponent).GetFirstMetaMesh().GetMeshAtIndex(0).GetMaterial()
			.GetTexture((MBTextureType)1) != (NativeObject)(object)newTexture)
		{
			_captureTheFlagAnimation.AnimationInProgress = true;
			_captureTheFlagAnimation.NewBannerTexture = newTexture;
			_captureTheFlagAnimation.DtTillStart = 0f;
			_captureTheFlagAnimation.MaterialSet = false;
		}
	}
}
