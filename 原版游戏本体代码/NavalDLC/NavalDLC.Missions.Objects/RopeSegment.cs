using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.Missions.Objects;

[ScriptComponentParams("ship_visual_only", "rope_segment")]
internal class RopeSegment : ScriptComponentBehavior
{
	private const int BridgeCurveLinearSampleCount = 8;

	private const string PhysicsEntityTag = "rope_physics_body";

	private static readonly Comparer<KeyValuePair<float, Vec3>> _cacheCompareDelegate = Comparer<KeyValuePair<float, Vec3>>.Create((KeyValuePair<float, Vec3> x, KeyValuePair<float, Vec3> y) => x.Key.CompareTo(y.Key));

	private static float[] _physicsCheckPoints = new float[3] { 0.05f, 0.5f, 0.93f };

	[EditableScriptComponentVariable(true, "Segment Index")]
	private int _segmentIndex;

	[EditableScriptComponentVariable(true, "Is Fixed")]
	private bool _isFixed;

	[EditableScriptComponentVariable(true, "Loose Amount")]
	private float _looseAmount = 0.1f;

	[EditableScriptComponentVariable(true, "Default Rope Length")]
	private float _defaultRopeLength = 25.9f;

	[EditableScriptComponentVariable(true, "Uses Physics Body")]
	private bool _usesPhysicsBody;

	[EditableScriptComponentVariable(true, "Swing Multiplier")]
	private float _swingMultiplier = 1f;

	private KeyValuePair<float, Vec3>[] _bridgeCurveLinearAccessCache = new KeyValuePair<float, Vec3>[8];

	private bool _firstTick = true;

	private Vec3 _previousPosition = Vec3.Zero;

	private Vec3 _previousVelocity = Vec3.Zero;

	private MatrixFrame _prevParentFrame = MatrixFrame.Identity;

	private float _pendulumVelocity;

	private float _pendulumCurrentRotation;

	private int _tickRemainingForPhysics = 30;

	private GameEntity _endEntity;

	private GameEntity _physicsEntity;

	private Mesh _ropeMesh;

	private bool _externalEndEntitySet;

	private float _cumulativeTime;

	private MatrixFrame _currentFrameSwingFrame = MatrixFrame.Identity;

	private Vec3 _previousChangeDueToShip = Vec3.Zero;

	private List<RopeSegmentCosmetics> _ropeSegmentCosmetics = new List<RopeSegmentCosmetics>();

	private bool _dynamicMode;

	private List<float> _ropeSegmentCosmeticsDxCached = new List<float>();

	public float RuntimeLooseMultiplier { get; private set; }

	public bool UseDistanceAsRopeLength { get; private set; }

	public float BurnedClipFactor { get; set; }

	public bool BurnedClipReverseMode { get; set; }

	public Mesh RopeMesh
	{
		get
		{
			return _ropeMesh;
		}
		private set
		{
			_ropeMesh = value;
		}
	}

	public float CurrentRopeLength { get; private set; }

	public bool LinearMode { get; private set; }

	public float LooseAmount
	{
		get
		{
			return _looseAmount;
		}
		private set
		{
			_looseAmount = value;
		}
	}

	public bool IsFixed
	{
		get
		{
			return _isFixed;
		}
		private set
		{
			_isFixed = value;
		}
	}

	public int SegmentIndex
	{
		get
		{
			return _segmentIndex;
		}
		private set
		{
			_segmentIndex = value;
		}
	}

	public float DefaultRopeLength
	{
		get
		{
			return _defaultRopeLength;
		}
		private set
		{
			_defaultRopeLength = value;
		}
	}

	public WeakGameEntity EndEntity
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _endEntity.WeakEntity;
		}
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			_endEntity = GameEntity.CreateFromWeakEntity(value);
			_externalEndEntitySet = value != (GameEntity)null;
		}
	}

	private RopeSegment()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		RuntimeLooseMultiplier = 1f;
		CurrentRopeLength = 12.95f;
		UseDistanceAsRopeLength = false;
		LinearMode = false;
		BurnedClipFactor = 0f;
		BurnedClipReverseMode = false;
	}

	protected override void OnEditorInit()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		FetchEntities();
		if (_usesPhysicsBody)
		{
			WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Root;
			_physicsEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("rope_physics_body"));
		}
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		_physicsCheckPoints[0] = 0.15f;
		_physicsCheckPoints[1] = 0.5f;
		_physicsCheckPoints[2] = 0.85f;
		FetchEntities();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).IsVisibleIncludeParents())
		{
			TickAux(dt);
		}
		else
		{
			_firstTick = true;
		}
	}

	protected override void OnInit()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		FetchEntities();
		if (_usesPhysicsBody)
		{
			WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Root;
			_physicsEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("rope_physics_body"));
		}
	}

	protected override void OnTickParallel3(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).IsVisibleIncludeParents())
		{
			TickAux(dt);
		}
		else
		{
			_firstTick = true;
		}
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		if (variableName == "Default Rope Length")
		{
			CurrentRopeLength = _defaultRopeLength * 0.5f;
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		((ScriptComponentBehavior)this).OnRemoved(removeReason);
		_endEntity = null;
		_physicsEntity = null;
		_ropeMesh = null;
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)64;
	}

	private void FetchEntities()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		_ropeSegmentCosmetics.Clear();
		_physicsEntity = null;
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref val)).GetChildren())
		{
			WeakGameEntity current = child;
			RopeSegmentCosmetics firstScriptOfType = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<RopeSegmentCosmetics>();
			if (firstScriptOfType != null)
			{
				_ropeSegmentCosmetics.Add(firstScriptOfType);
				val = ((ScriptComponentBehavior)firstScriptOfType).GameEntity;
				((WeakGameEntity)(ref val)).SetDoNotCheckVisibility(true);
			}
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref val)).Parent != (GameEntity)null && !_externalEndEntitySet)
		{
			val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Parent;
			_endEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTag("simple_rope_end"));
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		_ropeMesh = ((WeakGameEntity)(ref val)).GetFirstMesh();
		if ((NativeObject)(object)_ropeMesh != (NativeObject)null)
		{
			_ropeMesh.SetupAdditionalBoneBuffer(2);
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).SetBoundingboxDirty();
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).SetDoNotCheckVisibility(true);
		MatrixFrame val2;
		if ((NativeObject)(object)_ropeMesh != (NativeObject)null)
		{
			Mesh ropeMesh = _ropeMesh;
			val2 = MatrixFrame.Identity;
			ropeMesh.SetAdditionalBoneFrame(1, ref val2);
		}
		if (!((NativeObject)(object)_ropeMesh != (NativeObject)null) || !(_endEntity != (GameEntity)null) || _ropeSegmentCosmetics.Count <= 0)
		{
			return;
		}
		_ropeSegmentCosmeticsDxCached.Clear();
		Vec3 plankTargetOrigin = default(Vec3);
		((Vec3)(ref plankTargetOrigin))._002Ector(0f, 0f, 0f, -1f);
		val = ((ScriptComponentBehavior)this).GameEntity;
		val2 = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		MatrixFrame globalFrame = _endEntity.GetGlobalFrame();
		Vec3 plankSourceOrigin = ((MatrixFrame)(ref val2)).TransformToLocalNonOrthogonal(ref globalFrame.origin);
		Vec3 vectorArgument = _ropeMesh.GetVectorArgument();
		float curvedLength = vectorArgument.x * vectorArgument.z;
		FillBridgeCurveAccessData(in plankTargetOrigin, in plankSourceOrigin, in curvedLength);
		foreach (RopeSegmentCosmetics ropeSegmentCosmetic in _ropeSegmentCosmetics)
		{
			float currentLength = MathF.Clamp(ropeSegmentCosmetic.RopeLocalPosition, 0f, 1f) * curvedLength;
			_ropeSegmentCosmeticsDxCached.Add(GetCurveDxFromDt(plankTargetOrigin, currentLength));
		}
	}

	private void TickAux(float dt)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (!(_endEntity == (GameEntity)null) && !((NativeObject)(object)_ropeMesh == (NativeObject)null))
		{
			_cumulativeTime += dt;
			Vec3 val = default(Vec3);
			((Vec3)(ref val))._002Ector(0f, 0f, 0f, -1f);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			MatrixFrame globalFrame2 = _endEntity.GetGlobalFrame();
			Vec3 val2 = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref globalFrame2.origin);
			SetRopeShaderParams(val, val2);
			TickSwingPhysics(dt, val, val2);
			TickCosmetics(val, val2);
		}
	}

	private void SetRopeShaderParams(Vec3 startPosition, Vec3 endPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		identity.rotation.s = startPosition;
		identity.origin = endPosition;
		Vec3 val = endPosition - startPosition;
		float num = ((Vec3)(ref val)).Normalize();
		_ropeMesh.SetAdditionalBoneFrame(0, ref identity);
		float num2 = 0f;
		if (!LinearMode)
		{
			num2 = _looseAmount;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		float x = ((Mat3)(ref globalFrame.rotation)).GetScaleVector().x;
		num2 = num2 * RuntimeLooseMultiplier * x;
		num2 = MathF.Max(0.005f, num2);
		float w = _ropeMesh.GetVectorArgument().w;
		if (_isFixed || UseDistanceAsRopeLength)
		{
			_ropeMesh.SetVectorArgument(num + num2, 25.9f, 1f, w);
			return;
		}
		float num3 = num + num2;
		float num4 = _defaultRopeLength - num3;
		float num5 = 1f - num4 / _defaultRopeLength;
		_ropeMesh.SetVectorArgument(num3, 25.9f, num5, w);
	}

	private float GetCurveDxFromDt(Vec3 startPosition, float currentLength)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		int num = Array.BinarySearch(_bridgeCurveLinearAccessCache, new KeyValuePair<float, Vec3>(currentLength, Vec3.Zero), _cacheCompareDelegate);
		float num2 = 1f / 7f;
		if (num >= 0)
		{
			return (float)num * num2;
		}
		int num3 = ~num;
		int num4 = num3 - 1;
		KeyValuePair<float, Vec3> keyValuePair = _bridgeCurveLinearAccessCache[num4];
		KeyValuePair<float, Vec3> keyValuePair2 = _bridgeCurveLinearAccessCache[num3];
		return ((currentLength - keyValuePair.Key) / (keyValuePair2.Key - keyValuePair.Key) + (float)num4) * num2;
	}

	private Vec3 GetCurvePositionFromLength(Vec3 startPosition, float currentLength)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		int num = Array.BinarySearch(_bridgeCurveLinearAccessCache, new KeyValuePair<float, Vec3>(currentLength, Vec3.Zero), _cacheCompareDelegate);
		if (num >= 0)
		{
			return _bridgeCurveLinearAccessCache[num].Value;
		}
		int num2 = ~num;
		int num3 = num2 - 1;
		KeyValuePair<float, Vec3> keyValuePair = _bridgeCurveLinearAccessCache[num3];
		KeyValuePair<float, Vec3> keyValuePair2 = _bridgeCurveLinearAccessCache[num2];
		float num4 = (currentLength - keyValuePair.Key) / (keyValuePair2.Key - keyValuePair.Key);
		Vec3 val = Vec3.Lerp(keyValuePair.Value, keyValuePair2.Value, num4);
		if (!LinearMode)
		{
			ref MatrixFrame currentFrameSwingFrame = ref _currentFrameSwingFrame;
			Vec3 val2 = val - startPosition;
			val = ((MatrixFrame)(ref currentFrameSwingFrame)).TransformToLocal(ref val2) + startPosition;
		}
		return val;
	}

	private void FillBridgeCurveAccessData(in Vec3 plankTargetOrigin, in Vec3 plankSourceOrigin, in float curvedLength)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		_bridgeCurveLinearAccessCache[0] = new KeyValuePair<float, Vec3>(0f, plankTargetOrigin);
		Vec3 val = plankTargetOrigin;
		float num = 1f / 7f;
		float num2 = 0f;
		for (int i = 1; i < 7; i++)
		{
			Vec3 val2 = CalculateAutoCurvePosition(plankTargetOrigin, plankSourceOrigin, curvedLength, (float)i * num);
			float num3 = ((Vec3)(ref val2)).Distance(val);
			num2 += num3;
			_bridgeCurveLinearAccessCache[i] = new KeyValuePair<float, Vec3>(num2, val2);
			val = val2;
		}
		_bridgeCurveLinearAccessCache[7] = new KeyValuePair<float, Vec3>(curvedLength, plankSourceOrigin);
	}

	private void TickCosmetics(Vec3 startPoint, Vec3 endPoint)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		Vec3 vectorArgument = _ropeMesh.GetVectorArgument();
		float curvedLength = vectorArgument.x * vectorArgument.z;
		if (_ropeSegmentCosmetics.Count > 0 && !LinearMode && _dynamicMode)
		{
			FillBridgeCurveAccessData(in startPoint, in endPoint, in curvedLength);
		}
		for (int i = 0; i < _ropeSegmentCosmetics.Count; i++)
		{
			RopeSegmentCosmetics ropeSegmentCosmetics = _ropeSegmentCosmetics[i];
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)ropeSegmentCosmetics).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			Vec3 zero = Vec3.Zero;
			WeakGameEntity gameEntity2;
			MatrixFrame globalFrame2;
			if (LinearMode)
			{
				zero = Vec3.Lerp(startPoint, endPoint, MathF.Clamp(ropeSegmentCosmetics.RopeLocalPosition, 0f, 1f));
				if (ropeSegmentCosmetics.IsBurningNode)
				{
					Vec3 s = endPoint - startPoint;
					gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
					globalFrame2 = ((WeakGameEntity)(ref gameEntity2)).GetGlobalFrame();
					s = ((Mat3)(ref globalFrame2.rotation)).TransformToParent(ref s);
					if ((double)((Vec3)(ref s)).LengthSquared > 0.0001)
					{
						((Vec3)(ref s)).Normalize();
						globalFrame.rotation.s = s;
						globalFrame.rotation.f = -((Vec3)(ref globalFrame.rotation.s)).CrossProductWithUp();
						((Vec3)(ref globalFrame.rotation.f)).Normalize();
						globalFrame.rotation.u = Vec3.CrossProduct(globalFrame.rotation.s, globalFrame.rotation.f);
					}
				}
			}
			else
			{
				float num = MathF.Clamp(ropeSegmentCosmetics.RopeLocalPosition, 0f, 1f) * curvedLength;
				if (_dynamicMode)
				{
					zero = GetCurvePositionFromLength(startPoint, num);
				}
				else
				{
					float dx = _ropeSegmentCosmeticsDxCached[i];
					zero = CalculateAutoCurvePosition(startPoint, endPoint, curvedLength, dx);
					ref MatrixFrame currentFrameSwingFrame = ref _currentFrameSwingFrame;
					Vec3 val = zero - startPoint;
					zero = ((MatrixFrame)(ref currentFrameSwingFrame)).TransformToLocal(ref val) + startPoint;
				}
				if (ropeSegmentCosmetics.IsBurningNode)
				{
					Vec3 s2 = GetCurvePositionFromLength(startPoint, MathF.Min(num + 0.1f, curvedLength)) - zero;
					gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
					globalFrame2 = ((WeakGameEntity)(ref gameEntity2)).GetGlobalFrame();
					s2 = ((Mat3)(ref globalFrame2.rotation)).TransformToParent(ref s2);
					if ((double)((Vec3)(ref s2)).LengthSquared > 1E-06)
					{
						((Vec3)(ref s2)).Normalize();
						globalFrame.rotation.s = s2;
						globalFrame.rotation.f = -((Vec3)(ref globalFrame.rotation.s)).CrossProductWithUp();
						((Vec3)(ref globalFrame.rotation.f)).Normalize();
						globalFrame.rotation.u = Vec3.CrossProduct(globalFrame.rotation.s, globalFrame.rotation.f);
					}
				}
			}
			gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
			globalFrame2 = ((WeakGameEntity)(ref gameEntity2)).GetGlobalFrame();
			globalFrame.origin = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref zero);
			((WeakGameEntity)(ref gameEntity)).SetGlobalFrame(ref globalFrame, true);
		}
	}

	private bool CheckPhysicsEntity(in Vec3 startPosition, in Vec3 endPosition, float currentRotation, float nextRotation, float ropeLength)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = endPosition - startPosition;
		((Vec3)(ref val)).Normalize();
		MatrixFrame identity = MatrixFrame.Identity;
		((Mat3)(ref identity.rotation)).RotateAboutAnArbitraryVector(ref val, currentRotation);
		MatrixFrame identity2 = MatrixFrame.Identity;
		((Mat3)(ref identity2.rotation)).RotateAboutAnArbitraryVector(ref val, nextRotation);
		float[] physicsCheckPoints = _physicsCheckPoints;
		foreach (float dx in physicsCheckPoints)
		{
			Vec3 val2 = CalculateAutoCurvePosition(startPosition, endPosition, ropeLength, dx);
			Vec3 val3 = val2;
			Vec3 val4 = val2 - startPosition;
			val2 = ((MatrixFrame)(ref identity)).TransformToParent(ref val4) + startPosition;
			val4 = val3 - startPosition;
			val3 = ((MatrixFrame)(ref identity2)).TransformToParent(ref val4) + startPosition;
			Vec3 val5 = val3 - val2;
			float num = ((Vec3)(ref val5)).Normalize();
			if (!(num < 0.0001f))
			{
				num += 0.02f;
				float num2 = 0f;
				GameEntity physicsEntity = _physicsEntity;
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				if (physicsEntity.RayHitEntity(((MatrixFrame)(ref globalFrame)).TransformToParent(ref val2), val5, num, ref num2) && num > num2)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void TickSwingPhysics(float dt, Vec3 startPoint, Vec3 endPoint)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0583: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0595: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)_ropeMesh == (NativeObject)null || _endEntity == (GameEntity)null || (double)_looseAmount < 1E-07 || dt == 0f)
		{
			_currentFrameSwingFrame = MatrixFrame.Identity;
			return;
		}
		if (_tickRemainingForPhysics > 0)
		{
			_tickRemainingForPhysics--;
			return;
		}
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity parent = ((WeakGameEntity)(ref val)).Parent;
		if (parent != (GameEntity)null && dt > 0f && !LinearMode)
		{
			val = ((WeakGameEntity)(ref parent)).Root;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
			Vec3 val2 = endPoint - startPoint;
			val = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame localFrame = ((WeakGameEntity)(ref val)).GetLocalFrame();
			Vec3 val3 = ((MatrixFrame)(ref localFrame)).TransformToParent(ref startPoint);
			((Vec3)(ref val2)).Normalize();
			if ((double)((Vec3)(ref val2)).Length < 1E-09)
			{
				return;
			}
			Vec3 vectorArgument = _ropeMesh.GetVectorArgument();
			float num = vectorArgument.x * vectorArgument.z;
			bool firstTick = _firstTick;
			if (_firstTick)
			{
				Vec3 previousPosition = val3;
				_previousPosition = previousPosition;
				_prevParentFrame = globalFrame;
				_firstTick = false;
			}
			Vec3 val4 = ((Vec3)(ref val2)).CrossProductWithUp();
			((Vec3)(ref val4)).Normalize();
			if (false)
			{
				MatrixFrame val5 = ((MatrixFrame)(ref _prevParentFrame)).TransformToLocalNonOrthogonal(ref globalFrame);
				Vec3 val6 = ((MatrixFrame)(ref val5)).TransformToParent(ref val3) - val3;
				if (firstTick)
				{
					_previousChangeDueToShip = val6;
				}
				Vec3 val7 = (val6 - _previousChangeDueToShip) * 0.0003f / dt;
				_previousChangeDueToShip = val6;
				float num2 = ((Vec3)(ref val7)).Normalize();
				num2 = MathF.Clamp(num2, 0f, 1f);
				float num3 = Vec3.DotProduct(-val7, val4);
				if (MathF.IsValidValue(num3))
				{
					if (MathF.Abs(num3) > 0f)
					{
						float num4 = MathF.Sign(num3);
						num3 = MathF.Max((MathF.Abs(num3) - 0.6f) * 2.5f, 0f) * num4 * num2;
					}
					_pendulumVelocity -= num3 * _swingMultiplier * 0.25f;
				}
			}
			if (_pendulumCurrentRotation > 0f)
			{
				float num5 = MBMath.SmoothStep(0f, 0.1f, _pendulumCurrentRotation);
				_pendulumVelocity -= dt * 2f * num5 * 1.027f * 0.3f;
			}
			else
			{
				float num6 = MBMath.SmoothStep(0f, -0.1f, _pendulumCurrentRotation);
				_pendulumVelocity += dt * 2f * num6 * 1.027f * 0.3f;
			}
			float num7 = MathF.Lerp(1f, 0.5f, dt * 4f, 1E-05f);
			_pendulumVelocity *= num7;
			Vec3 val8 = default(Vec3);
			((Vec3)(ref val8))._002Ector(MathF.Pow(MathF.Cos(startPoint.x * 0.5f + _cumulativeTime * 0.45f), 10f), MathF.Pow(MathF.Cos(startPoint.y * 1.2f + _cumulativeTime * 0.65f), 10f), MathF.Pow(MathF.Cos(startPoint.z * 3.5f + _cumulativeTime * 0.35f), 10f), -1f);
			((Vec3)(ref val8)).Normalize();
			float num8 = MathF.Clamp(MathF.Cos(startPoint.x * 0.5f + _cumulativeTime * 2.5f) - 0.95f, 0f, 1f) * 4.5f;
			num8 = MathF.Max(num8, 0f);
			float num9 = MathF.Clamp(MathF.Cos(startPoint.y * 0.9f + _cumulativeTime * 2.5f) - 0.95f, 0f, 1f) * 4.9f;
			num9 = MathF.Max(num9, 0f);
			float num10 = 1f + MathF.Cos(startPoint.z * 0.3f + _cumulativeTime * 0.345f);
			val = ((ScriptComponentBehavior)this).GameEntity;
			Vec2 globalWindStrengthVectorOfScene = ((WeakGameEntity)(ref val)).GetGlobalWindStrengthVectorOfScene();
			float num11 = MathF.Min(((Vec2)(ref globalWindStrengthVectorOfScene)).Length, 5f) * MathF.Max(num8, num9) * num10 * dt / MathF.Max(1f, num);
			_pendulumVelocity += num11 * 6.8f * _swingMultiplier;
			float num12 = _pendulumVelocity * dt * 50f;
			if (_physicsEntity != (GameEntity)null && !CheckPhysicsEntity(in startPoint, in endPoint, _pendulumCurrentRotation, _pendulumCurrentRotation + num12, num))
			{
				_pendulumVelocity *= -0.95f;
				num12 *= -1.25f;
			}
			float num13 = MathF.Sign(_pendulumVelocity);
			float num14 = MathF.Abs(_pendulumVelocity);
			num14 = MathF.Min(num14, 0.06f);
			_pendulumVelocity = num14 * num13;
			_pendulumCurrentRotation += num12;
			if (!MathF.IsValidValue(_pendulumCurrentRotation))
			{
				_pendulumCurrentRotation = 0f;
			}
			if (!MathF.IsValidValue(_pendulumVelocity))
			{
				_pendulumVelocity = 0f;
			}
			while (true)
			{
				if (_pendulumCurrentRotation > MathF.PI)
				{
					_pendulumCurrentRotation -= MathF.PI * 2f;
					continue;
				}
				if (!(_pendulumCurrentRotation < -MathF.PI))
				{
					break;
				}
				_pendulumCurrentRotation += MathF.PI * 2f;
			}
			_previousVelocity = (startPoint - _previousPosition) / dt;
			_previousPosition = startPoint;
			_prevParentFrame = globalFrame;
			Vec3 val9 = startPoint - endPoint;
			((Vec3)(ref val9)).Normalize();
			_currentFrameSwingFrame = MatrixFrame.Identity;
			((Mat3)(ref _currentFrameSwingFrame.rotation)).RotateAboutAnArbitraryVector(ref val9, _pendulumCurrentRotation);
		}
		else
		{
			_currentFrameSwingFrame = MatrixFrame.Identity;
		}
		_ropeMesh.SetAdditionalBoneFrame(1, ref _currentFrameSwingFrame);
	}

	public void ShiftRope(float meters)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Vec3 vectorArgument = _ropeMesh.GetVectorArgument();
		float num = vectorArgument.z * vectorArgument.x;
		if (!(num > 0f))
		{
			return;
		}
		float num2 = meters / num;
		foreach (RopeSegmentCosmetics ropeSegmentCosmetic in _ropeSegmentCosmetics)
		{
			ropeSegmentCosmetic.RopeLocalPosition += num2;
		}
	}

	public void ApplyBoundingBox(MatrixFrame parentFrame, ref BoundingBox bb)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 val = ((MatrixFrame)(ref parentFrame)).TransformToLocalNonOrthogonal(ref globalFrame.origin);
		Vec3 val2 = val + Vec3.One * 0.25f;
		((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
		val2 = val - Vec3.One * 0.25f;
		((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
		if (_endEntity != (GameEntity)null)
		{
			globalFrame = _endEntity.GetGlobalFrame();
			Vec3 val3 = ((MatrixFrame)(ref parentFrame)).TransformToLocalNonOrthogonal(ref globalFrame.origin);
			val2 = val3 + Vec3.One * 0.25f;
			((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
			val2 = val3 - Vec3.One * 0.25f;
			((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
		}
	}

	public void SetUseDistanceAsRopeLength()
	{
		UseDistanceAsRopeLength = true;
	}

	public void SetEndEntity(WeakGameEntity entity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_endEntity = GameEntity.CreateFromWeakEntity(entity);
		_externalEndEntitySet = entity != (GameEntity)null;
	}

	public void SetAsFixedEntity()
	{
		_isFixed = true;
	}

	public void AddRope(float value)
	{
		CurrentRopeLength += value;
	}

	public void SetLinearMode(bool value)
	{
		LinearMode = value;
	}

	public void SetRuntimeLooseMultiplier(float value)
	{
		RuntimeLooseMultiplier = value;
	}

	public void FillBurningRecordForSegment(BurningSystem system, string prefabName, float nodeLength, bool reversePlacement)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		float num = ((Vec3)(ref globalFrame.origin)).Distance(_endEntity.GetGlobalFrame().origin);
		int num2 = (int)(num / nodeLength);
		float num3 = nodeLength / (num * 2f);
		for (int i = 0; i < num2; i++)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			GameEntity val = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, prefabName, true, true, "");
			if (!(val == (GameEntity)null))
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).AddChild(val.WeakEntity, false);
				BurningNode firstScriptOfType = val.GetFirstScriptOfType<BurningNode>();
				if (firstScriptOfType != null)
				{
					system.AddNewNode(firstScriptOfType);
				}
				if (MBRandom.RandomFloat > 0.82f)
				{
					firstScriptOfType.EnableSparks();
				}
				val.CreateAndAddScriptComponent("rope_segment_cosmetics", true);
				RopeSegmentCosmetics firstScriptOfType2 = val.GetFirstScriptOfType<RopeSegmentCosmetics>();
				firstScriptOfType2.RopeLocalPosition = num3 + (float)i * nodeLength / num;
				_ropeSegmentCosmetics.Add(firstScriptOfType2);
				if (reversePlacement)
				{
					firstScriptOfType2.RopeLocalPosition = 1f - firstScriptOfType2.RopeLocalPosition;
				}
			}
		}
		_dynamicMode = true;
	}

	public bool DeregisterRopeSegmentCosmetics(RopeSegmentCosmetics cosmetics)
	{
		if (_ropeSegmentCosmetics.IndexOf(cosmetics) != -1)
		{
			_ropeSegmentCosmetics.Remove(cosmetics);
			return true;
		}
		return false;
	}

	public void SetAsDynamic()
	{
		_dynamicMode = true;
	}

	public void SetAlpha(float value)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)_ropeMesh != (NativeObject)null)
		{
			WeakGameEntity gameEntity;
			if (value <= 0f)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
			}
			else
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(true);
				_ropeMesh.SetColorAlpha((uint)(MathF.Clamp(value, 0f, 1f) * 255f));
			}
		}
	}

	public static Vec3 CalculateAutoCurvePosition(Vec3 startPos, Vec3 endPos, float ropeLength, float dx)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = ((Vec3)(ref startPos)).AsVec2 - ((Vec3)(ref endPos)).AsVec2;
		float num = MathF.Clamp((((Vec2)(ref val)).Length - 0.4f) / 0.2f, 0f, 1f);
		Vec3 val2 = Vec3.Lerp(startPos, endPos, dx);
		if (num < 1E-06f)
		{
			return val2;
		}
		if (startPos.z > endPos.z)
		{
			Vec3 val3 = startPos;
			startPos = endPos;
			endPos = val3;
			dx = 1f - dx;
			val *= -1f;
		}
		ropeLength = MathF.Max(ropeLength, ((Vec2)(ref val)).Length);
		float length = ((Vec2)(ref val)).Length;
		float num2 = (startPos.z - endPos.z) / length;
		ropeLength /= length;
		float num3 = MathF.Sqrt(ropeLength * ropeLength - num2 * num2);
		float num4 = 1f;
		for (int i = 0; i < 10; i++)
		{
			float num5 = num4;
			float num6 = (float)Math.Sinh(num5);
			float num7 = (float)Math.Cosh(num5);
			float num8 = num5 - (num3 - num6 / num5) / (num6 / (num5 * num5) - num7 / num5);
			if (!MathF.IsValidValue(num8))
			{
				break;
			}
			num4 = num8;
		}
		float num9 = 1f / (2f * num4);
		float num10 = (1f - MathF.Log((ropeLength - num2) / (ropeLength + num2)) * num9) * 0.5f;
		float num11 = (0f - Math.Abs(num2)) * 0.5f - ropeLength * 0.5f * (1f / (float)Math.Tanh(num4));
		float num12 = num9 * (float)Math.Cosh((dx - num10) / num9) + num11;
		Vec3 val4 = Vec3.Lerp(startPos, endPos, dx);
		val4.z = endPos.z + num12 * length;
		if (!((Vec3)(ref val4)).IsValid)
		{
			return val2;
		}
		return Vec3.Lerp(val2, val4, num);
	}
}
