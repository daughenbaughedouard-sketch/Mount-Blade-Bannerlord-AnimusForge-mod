using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class RopePileBaked : ScriptComponentBehavior
{
	public const float HookLength = 0.5f;

	private const int NumberOfPoints = 64;

	private const int PaddedNumberOfPoints = 72;

	private const int NumberOfDataPerFrame = 12;

	private Mesh _ropeMesh;

	private BoundingBox _localUpdatedBoundingBox;

	private BoundingBox _ropePileBaseBoundingBox;

	protected override void OnEditorInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorInit();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_ropeMesh = ((WeakGameEntity)(ref gameEntity)).GetFirstMesh();
	}

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_ropeMesh = ((WeakGameEntity)(ref gameEntity)).GetFirstMesh();
		_ropeMesh.SetupAdditionalBoneBuffer(7);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_ropePileBaseBoundingBox = ((WeakGameEntity)(ref gameEntity)).GetLocalBoundingBox();
		_localUpdatedBoundingBox = _ropePileBaseBoundingBox;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetHasCustomBoundingBoxValidationSystem(true);
	}

	protected override void OnBoundingBoxValidate()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		BoundingBox val = default(BoundingBox);
		((BoundingBox)(ref val)).BeginRelaxation();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).ChildCount > 0)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref gameEntity)).ComputeBoundingBoxIncludeChildren();
		}
		((BoundingBox)(ref val)).RelaxWithBoundingBox(_localUpdatedBoundingBox);
		((BoundingBox)(ref val)).RecomputeRadius();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).RelaxLocalBoundingBox(ref val);
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)0;
	}

	public MatrixFrame UpdateRopeMeshVisualAccordingToTargetPoint(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalPosition, in Vec3 globalVelocity, float time)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ComputeFreeFallPoints(in sourceGlobalPosition, in targetGlobalPosition, in globalVelocity, time);
	}

	public Vec3 UpdateRopeMeshVisualAccordingToTargetPointLinear(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalPosition)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return ComputeFreeFallPointsLinear(in sourceGlobalPosition, in targetGlobalPosition);
	}

	public Vec3 UpdateRopeMeshVisualAccordingToTargetPointLinearWithoutBoundingBoxUpdate(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalPosition)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return ComputeFreeFallPointsLinearWithoutBoundingBoxUpdate(in sourceGlobalPosition, in targetGlobalPosition);
	}

	private Vec3 GetPositionAtProjectileCurveProgress(in Vec3 globalVelocity, in Vec3 sourceGlobalPosition, float time, int progressInterval)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (progressInterval < 64)
		{
			time *= (float)progressInterval / 63f;
			return sourceGlobalPosition + globalVelocity * time + 0.5f * MBGlobals.GravitationalAcceleration * time * time;
		}
		return Vec3.Zero;
	}

	private Vec3 ComputeFreeFallPointsLinearWithoutBoundingBoxUpdate(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = targetGlobalPosition;
		Vec3 val2 = targetGlobalPosition - sourceGlobalPosition;
		Vec3 val3 = val - ((Vec3)(ref val2)).NormalizedCopy() * 0.5f;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 val4 = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref sourceGlobalPosition);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 val5 = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref val3);
		val2 = new Vec3(2f, 0f, 0f, -1f);
		Mat3 val6 = new Mat3(ref val4, ref val2, ref val5);
		MatrixFrame val7 = new MatrixFrame(ref val6, ref val5);
		_ropeMesh.SetAdditionalBoneFrame(0, ref val7);
		val2 = new Vec3(val4.z, val5.z, 0f, 1f);
		Vec3 val8 = new Vec3(0f, 0f, 0f, 1f);
		Vec3 val9 = new Vec3(0f, 0f, 0f, 1f);
		val6 = new Mat3(ref val2, ref val8, ref val9);
		Vec3 val10 = new Vec3(0f, 0f, 0f, 1f);
		val7 = new MatrixFrame(ref val6, ref val10);
		_ropeMesh.SetAdditionalBoneFrame(1, ref val7);
		Vec3 vectorArgument = _ropeMesh.GetVectorArgument();
		float x = vectorArgument.x;
		val2 = sourceGlobalPosition;
		vectorArgument.z = 1f - MathF.Max((x - ((Vec3)(ref val2)).Distance(val3)) / vectorArgument.x, 0f);
		_ropeMesh.SetVectorArgument(vectorArgument.x, vectorArgument.y, vectorArgument.z, vectorArgument.w);
		return val3;
	}

	private Vec3 ComputeFreeFallPointsLinear(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 val = targetGlobalPosition;
		Vec3 val2 = targetGlobalPosition - sourceGlobalPosition;
		Vec3 val3 = val - ((Vec3)(ref val2)).NormalizedCopy() * 0.5f;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 val4 = ((MatrixFrame)(ref globalFrame2)).TransformToLocalNonOrthogonal(ref sourceGlobalPosition);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 val5 = ((MatrixFrame)(ref globalFrame2)).TransformToLocalNonOrthogonal(ref val3);
		val2 = new Vec3(2f, 0f, 0f, -1f);
		Mat3 val6 = new Mat3(ref val4, ref val2, ref val5);
		MatrixFrame val7 = new MatrixFrame(ref val6, ref val5);
		_ropeMesh.SetAdditionalBoneFrame(0, ref val7);
		val2 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref val3);
		BoundingBox candidateLocalBoundingBox = new BoundingBox(ref val2);
		val2 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref sourceGlobalPosition);
		((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPointAndRadius(ref val2, 1f);
		val2 = new Vec3(val4.z, val5.z, 0f, 1f);
		Vec3 val8 = new Vec3(0f, 0f, 0f, 1f);
		Vec3 val9 = new Vec3(0f, 0f, 0f, 1f);
		val6 = new Mat3(ref val2, ref val8, ref val9);
		Vec3 val10 = new Vec3(0f, 0f, 0f, 1f);
		val7 = new MatrixFrame(ref val6, ref val10);
		_ropeMesh.SetAdditionalBoneFrame(1, ref val7);
		Vec3 vectorArgument = _ropeMesh.GetVectorArgument();
		float x = vectorArgument.x;
		val2 = sourceGlobalPosition;
		vectorArgument.z = 1f - MathF.Max((x - ((Vec3)(ref val2)).Distance(val3)) / vectorArgument.x, 0f);
		_ropeMesh.SetVectorArgument(vectorArgument.x, vectorArgument.y, vectorArgument.z, vectorArgument.w);
		UpdateRopeLocalBoundingBox(in candidateLocalBoundingBox);
		return val3;
	}

	private void UpdateRopeLocalBoundingBox(in BoundingBox candidateLocalBoundingBox)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		BoundingBox localBoundingBox = ((WeakGameEntity)(ref val)).GetLocalBoundingBox();
		if (BoundingBox.ArrangeWithAnotherBoundingBox(ref localBoundingBox, candidateLocalBoundingBox, 10f))
		{
			_localUpdatedBoundingBox = localBoundingBox;
			val = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref val)).SetBoundingboxDirty();
			val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Root;
			((WeakGameEntity)(ref val)).GetFirstScriptOfType<MissionShip>()?.InvalidateLocalBoundingBoxCache();
		}
	}

	public void SetRopeBoundingBoxToInitialState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetManualLocalBoundingBox(ref _ropePileBaseBoundingBox);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity parent = ((WeakGameEntity)(ref gameEntity)).Parent;
		if (((WeakGameEntity)(ref parent)).IsValid)
		{
			((WeakGameEntity)(ref parent)).SetBoundingboxDirty();
		}
	}

	private MatrixFrame ComputeFreeFallPoints(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalPosition, in Vec3 globalVelocity, float time)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		MatrixFrame identity = MatrixFrame.Identity;
		Vec3 val = globalVelocity + MBGlobals.GravitationalAcceleration * time;
		time -= 0.5f / ((Vec3)(ref val)).Length;
		identity.origin = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, 63);
		identity.rotation.f = ((Vec3)(ref val)).NormalizedCopy();
		ref Mat3 rotation = ref identity.rotation;
		Vec3 val2 = Vec3.CrossProduct(identity.rotation.f, identity.rotation.u);
		rotation.s = ((Vec3)(ref val2)).NormalizedCopy();
		identity.rotation.u = Vec3.CrossProduct(identity.rotation.s, identity.rotation.f);
		((Mat3)(ref identity.rotation)).RotateAboutSide(-MathF.PI / 2f);
		Vec3 val3 = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref sourceGlobalPosition);
		Vec3 val4 = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref identity.origin);
		val2 = new Vec3(64f, 0f, 0f, -1f);
		Mat3 val5 = new Mat3(ref val3, ref val2, ref val4);
		MatrixFrame val6 = new MatrixFrame(ref val5, ref val4);
		_ropeMesh.SetAdditionalBoneFrame(0, ref val6);
		val2 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref identity.origin);
		BoundingBox candidateLocalBoundingBox = new BoundingBox(ref val2);
		val2 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref sourceGlobalPosition);
		((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPointAndRadius(ref val2, 1f);
		for (int i = 0; i < 72; i += 12)
		{
			Vec3 positionAtProjectileCurveProgress = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i);
			Vec3 val7 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress);
			if (i < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val7);
			}
			Vec3 positionAtProjectileCurveProgress2 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 1);
			Vec3 val8 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress2);
			if (i + 1 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val8);
			}
			Vec3 positionAtProjectileCurveProgress3 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 2);
			Vec3 val9 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress3);
			if (i + 2 < 64)
			{
				val2 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref val9);
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val2);
			}
			Vec3 positionAtProjectileCurveProgress4 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 3);
			Vec3 val10 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress4);
			if (i + 3 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val10);
			}
			Vec3 positionAtProjectileCurveProgress5 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 4);
			Vec3 val11 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress5);
			if (i + 4 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val11);
			}
			Vec3 positionAtProjectileCurveProgress6 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 5);
			Vec3 val12 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress6);
			if (i + 5 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val12);
			}
			Vec3 positionAtProjectileCurveProgress7 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 6);
			Vec3 val13 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress7);
			if (i + 6 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val13);
			}
			Vec3 positionAtProjectileCurveProgress8 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 7);
			Vec3 val14 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress8);
			if (i + 7 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val14);
			}
			Vec3 positionAtProjectileCurveProgress9 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 8);
			Vec3 val15 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress9);
			if (i + 8 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val15);
			}
			Vec3 positionAtProjectileCurveProgress10 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 9);
			Vec3 val16 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress10);
			if (i + 9 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val16);
			}
			Vec3 positionAtProjectileCurveProgress11 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 10);
			Vec3 val17 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress11);
			if (i + 10 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val17);
			}
			Vec3 positionAtProjectileCurveProgress12 = GetPositionAtProjectileCurveProgress(in globalVelocity, in sourceGlobalPosition, time, i + 11);
			Vec3 val18 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref positionAtProjectileCurveProgress12);
			if (i + 11 < 64)
			{
				((BoundingBox)(ref candidateLocalBoundingBox)).RelaxMinMaxWithPoint(ref val18);
			}
			val2 = new Vec3(val7.z, val8.z, val9.z, 1f);
			Vec3 val19 = new Vec3(val10.z, val11.z, val12.z, 1f);
			Vec3 val20 = new Vec3(val13.z, val14.z, val15.z, 1f);
			val5 = new Mat3(ref val2, ref val19, ref val20);
			Vec3 val21 = new Vec3(val16.z, val17.z, val18.z, 1f);
			MatrixFrame val22 = new MatrixFrame(ref val5, ref val21);
			_ropeMesh.SetAdditionalBoneFrame(i / 12 + 1, ref val22);
		}
		Vec3 vectorArgument = _ropeMesh.GetVectorArgument();
		float x = vectorArgument.x;
		val2 = sourceGlobalPosition;
		vectorArgument.z = 1f - MathF.Max((x - ((Vec3)(ref val2)).Distance(identity.origin)) / vectorArgument.x, 0f);
		_ropeMesh.SetVectorArgument(vectorArgument.x, vectorArgument.y, vectorArgument.z, vectorArgument.w);
		UpdateRopeLocalBoundingBox(in candidateLocalBoundingBox);
		return identity;
	}
}
