using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.Missions.Objects;

[ScriptComponentParams("ship_visual_only", "pulley_system")]
internal class PulleySystem : ScriptComponentBehavior
{
	private struct SegmentData
	{
		internal RopeSegment RopeSegment;

		internal GameEntity RopeEntity;
	}

	private const string PulleyTag = "pulley";

	private const string PulleyWheelTag = "pulley_wheel";

	private const string PulleyLeftPointTag = "pulley_left_point";

	private const string PulleyRightPointTag = "pulley_right_point";

	private const string EndPointRopeTag = "end_point_rope";

	private const string EndPointTargetTag = "end_point_target";

	private const string AttachedToYardTag = "attached_to_yard";

	private const string FreePileTag = "free_pile";

	[EditableScriptComponentVariable(true, "End Rope Length")]
	private float _endRopeLength = 2f;

	private GameEntity _pulleyEntity;

	private GameEntity _pulleyWheelEntity;

	private GameEntity _pulleyLeftRopeConnectionEntity;

	private GameEntity _pulleyRightRopeConnectionEntity;

	private List<RopeSegment> _tiedToYardSegments = new List<RopeSegment>();

	private List<SegmentData> _fixedSegments = new List<SegmentData>();

	private List<SegmentData> _freeSegments = new List<SegmentData>();

	private SegmentData _endPointRope;

	private GameEntity _endTargetEntity;

	private Vec3 _targetPositionLocalPrevFrame = Vec3.Zero;

	private float _endRopeConnectionOffset;

	private float _looseAmountMultiplier;

	private bool _firstTick = true;

	public WeakGameEntity FirstFixedEntity
	{
		get
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (_fixedSegments.Count > 0)
			{
				return _fixedSegments[0].RopeEntity.WeakEntity;
			}
			return WeakGameEntity.Invalid;
		}
	}

	public List<RopeSegment> TiedToYardSegments => _tiedToYardSegments;

	private PulleySystem()
	{
	}//IL_002d: Unknown result type (might be due to invalid IL or missing references)
	//IL_0032: Unknown result type (might be due to invalid IL or missing references)


	protected override void OnEditorInit()
	{
		FetchEntities();
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).IsVisibleIncludeParents())
		{
			FetchEntities();
			TickAux();
		}
	}

	protected override void OnInit()
	{
		FetchEntities();
	}

	protected override void OnTickParallel2(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).IsVisibleIncludeParents())
		{
			TickAux();
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		((ScriptComponentBehavior)this).OnRemoved(removeReason);
		_pulleyEntity = null;
		_pulleyWheelEntity = null;
		_pulleyLeftRopeConnectionEntity = null;
		_pulleyRightRopeConnectionEntity = null;
		_tiedToYardSegments.Clear();
		_freeSegments.Clear();
		_fixedSegments.Clear();
		_endPointRope.RopeEntity = null;
		_endPointRope.RopeSegment = null;
		_endTargetEntity = null;
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)8;
	}

	private void FetchEntities()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		_tiedToYardSegments.Clear();
		FetchRopeSegmentsForSide(((ScriptComponentBehavior)this).GameEntity, isFixed: true, ref _fixedSegments);
		FetchRopeSegmentsForSide(((ScriptComponentBehavior)this).GameEntity, isFixed: false, ref _freeSegments);
		foreach (SegmentData freeSegment in _freeSegments)
		{
			freeSegment.RopeSegment.SetUseDistanceAsRopeLength();
			freeSegment.RopeSegment.SetAsDynamic();
		}
		foreach (SegmentData fixedSegment in _fixedSegments)
		{
			fixedSegment.RopeSegment.SetAsDynamic();
		}
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetChildrenRecursive(ref list);
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current2 = item;
			if (((WeakGameEntity)(ref current2)).HasTag("pulley"))
			{
				_pulleyEntity = GameEntity.CreateFromWeakEntity(current2);
			}
			else if (((WeakGameEntity)(ref current2)).HasTag("end_point_rope"))
			{
				_endPointRope.RopeEntity = GameEntity.CreateFromWeakEntity(current2);
				_endPointRope.RopeSegment = _endPointRope.RopeEntity.GetFirstScriptOfType<RopeSegment>();
			}
			else if (((WeakGameEntity)(ref current2)).HasTag("end_point_target"))
			{
				_endTargetEntity = GameEntity.CreateFromWeakEntity(current2);
			}
		}
		if (_pulleyEntity != (GameEntity)null)
		{
			_pulleyRightRopeConnectionEntity = _pulleyEntity.GetFirstChildEntityWithTag("pulley_right_point");
			_pulleyLeftRopeConnectionEntity = _pulleyEntity.GetFirstChildEntityWithTag("pulley_left_point");
			_pulleyWheelEntity = _pulleyEntity.GetFirstChildEntityWithTag("pulley_wheel");
			Mesh firstMesh = _pulleyEntity.GetFirstMesh();
			if ((NativeObject)(object)firstMesh != (NativeObject)null)
			{
				Vec3 val = firstMesh.GetBoundingBoxMax() - firstMesh.GetBoundingBoxMin();
				_endRopeConnectionOffset = val.z;
			}
		}
		if (_freeSegments.Count > 0)
		{
			int count = _freeSegments.Count;
			for (int i = 0; i < count - 1; i++)
			{
				_freeSegments[i].RopeSegment.SetEndEntity(_freeSegments[i + 1].RopeEntity.WeakEntity);
			}
			_freeSegments[count - 1].RopeSegment.SetEndEntity(_pulleyLeftRopeConnectionEntity.WeakEntity);
		}
		if (_fixedSegments.Count > 0)
		{
			int count2 = _fixedSegments.Count;
			for (int j = 0; j < count2 - 1; j++)
			{
				_fixedSegments[j].RopeSegment.SetEndEntity(_fixedSegments[j + 1].RopeEntity.WeakEntity);
			}
			_fixedSegments[count2 - 1].RopeSegment.SetEndEntity(_pulleyRightRopeConnectionEntity.WeakEntity);
		}
		if (_endPointRope.RopeSegment != null)
		{
			_endPointRope.RopeSegment.SetEndEntity(_endTargetEntity.WeakEntity);
			_endPointRope.RopeSegment.SetAsFixedEntity();
			_endPointRope.RopeSegment.SetAsDynamic();
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetDoNotCheckVisibility(true);
	}

	private void TickAux()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		if (!(_pulleyEntity == (GameEntity)null) && _freeSegments.Count != 0 && _fixedSegments.Count != 0 && !(_pulleyLeftRopeConnectionEntity == (GameEntity)null) && !(_pulleyRightRopeConnectionEntity == (GameEntity)null) && !(_endTargetEntity == (GameEntity)null))
		{
			Vec3 origin = _endTargetEntity.GetGlobalFrame().origin;
			WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Root;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
			_ = Vec3.Zero;
			Vec3 val2 = (_freeSegments[_freeSegments.Count - 1].RopeEntity.GetGlobalFrame().origin + _fixedSegments[_fixedSegments.Count - 1].RopeEntity.GetGlobalFrame().origin) * 0.5f - origin;
			((Vec3)(ref val2)).Normalize();
			MatrixFrame globalFrame2 = _pulleyEntity.GetGlobalFrame();
			float x = ((Mat3)(ref globalFrame2.rotation)).GetScaleVector().x;
			float num = _endRopeLength * x;
			globalFrame2.origin = origin + val2 * num;
			_pulleyEntity.SetGlobalFrame(ref globalFrame2, true);
			MatrixFrame identity = MatrixFrame.Identity;
			identity.rotation = globalFrame.rotation;
			Vec3 val3 = ((MatrixFrame)(ref identity)).TransformToLocalNonOrthogonal(ref val2);
			Vec3 val4 = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref origin);
			if (_firstTick)
			{
				_targetPositionLocalPrevFrame = val4;
				_firstTick = false;
			}
			Vec3 val5 = val4 - _targetPositionLocalPrevFrame;
			float num2 = ((Vec3)(ref val5)).Length;
			if (Vec3.DotProduct(val3, val5) < 0f)
			{
				num2 *= -1f;
			}
			float num3 = 0f;
			float num4 = 0f;
			for (int i = 0; i < _freeSegments.Count - 1; i++)
			{
				WeakGameEntity weakEntity = _freeSegments[i + 1].RopeEntity.WeakEntity;
				SetRopeParamsForSegment(weakEntity, _freeSegments[i], isFixed: true, num2 * 2f, moveUV: true, is_end_rope: false);
			}
			num3 += SetRopeParamsForSegment(_pulleyLeftRopeConnectionEntity.WeakEntity, _freeSegments[_freeSegments.Count - 1], isFixed: true, num2 * 2f, moveUV: true, is_end_rope: false);
			for (int j = 0; j < _fixedSegments.Count - 1; j++)
			{
				WeakGameEntity weakEntity2 = _fixedSegments[j + 1].RopeEntity.WeakEntity;
				SetRopeParamsForSegment(weakEntity2, _fixedSegments[j], isFixed: true, num2 * 2f, moveUV: false, is_end_rope: false);
			}
			num4 += SetRopeParamsForSegment(_pulleyLeftRopeConnectionEntity.WeakEntity, _fixedSegments[_fixedSegments.Count - 1], isFixed: true, num2 * 2f, moveUV: false, is_end_rope: false);
			ComputePulleyFrame(0f, (num3 + num4) * 0.5f);
			int num5 = 5;
			for (int k = 0; k < num5; k++)
			{
				SetRopeParamsForSegment(_pulleyLeftRopeConnectionEntity.WeakEntity, _freeSegments[_freeSegments.Count - 1], isFixed: true, 0f, moveUV: false, is_end_rope: false);
				SetRopeParamsForSegment(_pulleyRightRopeConnectionEntity.WeakEntity, _fixedSegments[_fixedSegments.Count - 1], isFixed: true, 0f, moveUV: false, is_end_rope: false);
			}
			if (_endPointRope.RopeEntity != (GameEntity)null)
			{
				MatrixFrame globalFrame3 = _pulleyEntity.GetGlobalFrame();
				Vec3 origin2 = globalFrame3.origin;
				origin2 += (_endRopeConnectionOffset - 0.165f) * globalFrame3.rotation.u;
				MatrixFrame globalFrame4 = _endPointRope.RopeEntity.GetGlobalFrame();
				globalFrame4.origin = origin2;
				_endPointRope.RopeEntity.SetGlobalFrame(ref globalFrame4, true);
				SetRopeParamsForSegment(_endTargetEntity.WeakEntity, _endPointRope, isFixed: true, 0f, moveUV: false, is_end_rope: true);
			}
			_targetPositionLocalPrevFrame = val4;
		}
	}

	private void ComputePulleyFrame(float move_amount, float total_rope_length)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.Zero;
		float num = 0f;
		int num2 = 0;
		float num3 = 0f;
		RopeSegment ropeSegment = _endPointRope.RopeSegment;
		WeakGameEntity weakEntity = _freeSegments[_freeSegments.Count - 1].RopeEntity.WeakEntity;
		Vec3 origin = ((WeakGameEntity)(ref weakEntity)).GetGlobalFrame().origin;
		WeakGameEntity weakEntity2 = _freeSegments[_freeSegments.Count - 1].RopeEntity.WeakEntity;
		Vec3 origin2 = ((WeakGameEntity)(ref weakEntity2)).GetGlobalFrame().origin;
		val += origin;
		RopeSegment ropeSegment2 = _freeSegments[_freeSegments.Count - 1].RopeSegment;
		if (ropeSegment2 != null)
		{
			num += MathF.Max(0.0005f, ropeSegment2.LooseAmount * _looseAmountMultiplier);
			num2++;
		}
		val += origin2;
		RopeSegment ropeSegment3 = _fixedSegments[_fixedSegments.Count - 1].RopeSegment;
		if (ropeSegment3 != null)
		{
			num += MathF.Max(0.0005f, ropeSegment3.LooseAmount * _looseAmountMultiplier);
			num2++;
		}
		if (ropeSegment != null)
		{
			num3 = MathF.Max(0.0005f, ropeSegment.LooseAmount * _looseAmountMultiplier);
		}
		val *= 0.5f;
		if (num2 > 0)
		{
			num /= (float)num2;
		}
		Vec3 origin3 = _endTargetEntity.GetGlobalFrame().origin;
		float num4 = ((Vec3)(ref val)).Distance(origin3);
		num4 += num + num3;
		MatrixFrame globalFrame = _pulleyEntity.GetGlobalFrame();
		float x = ((Mat3)(ref globalFrame.rotation)).GetScaleVector().x;
		float num5 = _endRopeLength * x;
		float num6 = 1f - num5 / num4;
		num6 = MathF.Clamp(num6, 0f, 1f);
		Vec3 val2 = RopeSegment.CalculateAutoCurvePosition(val, origin3, num4, num6);
		float dx = MathF.Min(num6 + 0.01f, 1f);
		Vec3 val3 = RopeSegment.CalculateAutoCurvePosition(val, origin3, num4, dx) - val2;
		if (((Vec3)(ref val3)).LengthSquared > 0f)
		{
			((Vec3)(ref val3)).Normalize();
		}
		val3 = val3 * 0.5f + (origin3 - val2) * 0.5f;
		((Vec3)(ref val3)).Normalize();
		WeakGameEntity weakEntity3 = _fixedSegments[_fixedSegments.Count - 1].RopeEntity.WeakEntity;
		WeakGameEntity weakEntity4 = _freeSegments[_freeSegments.Count - 1].RopeEntity.WeakEntity;
		Vec3 s = ((WeakGameEntity)(ref weakEntity3)).GetGlobalFrame().origin - ((WeakGameEntity)(ref weakEntity4)).GetGlobalFrame().origin;
		if (!(((Vec3)(ref s)).Length < 1E-06f))
		{
			((Vec3)(ref s)).Normalize();
			MatrixFrame frame = _pulleyEntity.GetFrame();
			frame.rotation.u = val3;
			frame.rotation.s = s;
			frame.rotation.f = Vec3.CrossProduct(frame.rotation.s, frame.rotation.u);
			((Vec3)(ref frame.rotation.f)).Normalize();
			frame.rotation.s = Vec3.CrossProduct(frame.rotation.f, frame.rotation.u);
			((Vec3)(ref frame.rotation.s)).Normalize();
			WeakGameEntity weakEntity5 = _pulleyEntity.WeakEntity;
			WeakGameEntity parent = ((WeakGameEntity)(ref weakEntity5)).Parent;
			if (parent != (GameEntity)null)
			{
				MatrixFrame globalFrame2 = ((WeakGameEntity)(ref parent)).GetGlobalFrame();
				frame.rotation = ((MatrixFrame)(ref globalFrame2)).TransformToLocalNonOrthogonal(ref frame).rotation;
			}
			((Mat3)(ref frame.rotation)).Orthonormalize();
			_pulleyEntity.SetFrame(ref frame, true);
			MatrixFrame globalFrame3 = _pulleyEntity.GetGlobalFrame();
			globalFrame3.origin = val2;
			_pulleyEntity.SetGlobalFrame(ref globalFrame3, true);
		}
	}

	private float SetRopeParamsForSegment(WeakGameEntity pulleyRopeConnectPoint, SegmentData segmentData, bool isFixed, float pull_amount, bool moveUV, bool is_end_rope)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		((WeakGameEntity)(ref pulleyRopeConnectPoint)).GetGlobalFrame();
		segmentData.RopeEntity.GetGlobalFrame();
		if (moveUV)
		{
			Vec3 vectorArgument = segmentData.RopeSegment.RopeMesh.GetVectorArgument2();
			vectorArgument.w += pull_amount * 25.9f;
			segmentData.RopeSegment.RopeMesh.SetVectorArgument2(vectorArgument.x, vectorArgument.y, vectorArgument.z, vectorArgument.w);
		}
		if (!isFixed || moveUV)
		{
			segmentData.RopeSegment.ShiftRope(0f - pull_amount);
		}
		return 25.9f;
	}

	public void SetEndTargetPosition(Vec3 position)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (_endTargetEntity != (GameEntity)null)
		{
			MatrixFrame globalFrame = _endTargetEntity.GetGlobalFrame();
			globalFrame.origin = position;
			_endTargetEntity.SetGlobalFrame(ref globalFrame, true);
		}
	}

	public void SetLinearMode(bool value)
	{
		foreach (SegmentData freeSegment in _freeSegments)
		{
			freeSegment.RopeSegment.SetLinearMode(value);
		}
		foreach (SegmentData fixedSegment in _fixedSegments)
		{
			fixedSegment.RopeSegment.SetLinearMode(value);
		}
		if (_endPointRope.RopeSegment != null)
		{
			_endPointRope.RopeSegment.SetLinearMode(value);
		}
	}

	public bool DeregisterRopeSegmentCosmetics(RopeSegmentCosmetics cosmetics)
	{
		bool result = false;
		foreach (SegmentData fixedSegment in _fixedSegments)
		{
			if (fixedSegment.RopeSegment.DeregisterRopeSegmentCosmetics(cosmetics))
			{
				result = true;
			}
		}
		foreach (SegmentData freeSegment in _freeSegments)
		{
			if (freeSegment.RopeSegment.DeregisterRopeSegmentCosmetics(cosmetics))
			{
				result = true;
			}
		}
		if (_endPointRope.RopeSegment != null && _endPointRope.RopeSegment.DeregisterRopeSegmentCosmetics(cosmetics))
		{
			result = true;
		}
		return result;
	}

	private void FetchRopeSegmentsForSide(WeakGameEntity parentEntity, bool isFixed, ref List<SegmentData> output)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		output.Clear();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child;
			RopeSegment firstScriptOfType = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<RopeSegment>();
			if (firstScriptOfType != null && firstScriptOfType.IsFixed == isFixed && !((WeakGameEntity)(ref current)).HasTag("end_point_rope"))
			{
				SegmentData item = new SegmentData
				{
					RopeSegment = firstScriptOfType,
					RopeEntity = GameEntity.CreateFromWeakEntity(current)
				};
				output.Add(item);
				if (((WeakGameEntity)(ref current)).HasTag("attached_to_yard"))
				{
					_tiedToYardSegments.Add(firstScriptOfType);
				}
			}
		}
		output.Sort((SegmentData a, SegmentData b) => a.RopeSegment.SegmentIndex.CompareTo(b.RopeSegment.SegmentIndex));
	}

	public void SetRuntimeLooseMultiplier(float value)
	{
		_looseAmountMultiplier = value;
		foreach (SegmentData freeSegment in _freeSegments)
		{
			freeSegment.RopeSegment.SetRuntimeLooseMultiplier(value);
		}
		foreach (SegmentData fixedSegment in _fixedSegments)
		{
			fixedSegment.RopeSegment.SetRuntimeLooseMultiplier(value);
		}
		_endPointRope.RopeSegment.SetRuntimeLooseMultiplier(value * 0.25f);
	}

	public void ApplyBoundingBox(MatrixFrame parentFrame, ref BoundingBox bb)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		foreach (SegmentData freeSegment in _freeSegments)
		{
			MatrixFrame globalFrame = freeSegment.RopeEntity.GetGlobalFrame();
			Vec3 val = ((MatrixFrame)(ref parentFrame)).TransformToLocalNonOrthogonal(ref globalFrame.origin);
			Vec3 val2 = val + Vec3.One * 0.25f;
			((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
			val2 = val - Vec3.One * 0.25f;
			((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
		}
		foreach (SegmentData fixedSegment in _fixedSegments)
		{
			MatrixFrame globalFrame = fixedSegment.RopeEntity.GetGlobalFrame();
			Vec3 val3 = ((MatrixFrame)(ref parentFrame)).TransformToLocalNonOrthogonal(ref globalFrame.origin);
			Vec3 val2 = val3 + Vec3.One * 0.25f;
			((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
			val2 = val3 - Vec3.One * 0.25f;
			((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
		}
		if (_endTargetEntity != (GameEntity)null)
		{
			MatrixFrame globalFrame = _endTargetEntity.GetGlobalFrame();
			Vec3 val4 = ((MatrixFrame)(ref parentFrame)).TransformToLocalNonOrthogonal(ref globalFrame.origin);
			Vec3 val2 = val4 + Vec3.One * 0.25f;
			((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
			val2 = val4 - Vec3.One * 0.25f;
			((BoundingBox)(ref bb)).RelaxMinMaxWithPoint(ref val2);
		}
	}

	public Vec3 GetTiePointCenter()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (_freeSegments.Count == 0 || _fixedSegments.Count == 0)
		{
			return Vec3.Zero;
		}
		return (_freeSegments[_freeSegments.Count - 1].RopeEntity.GetGlobalFrame().origin + _fixedSegments[_fixedSegments.Count - 1].RopeEntity.GetGlobalFrame().origin) * 0.5f;
	}

	public void SetFirstFreeGlobalPosition(Vec3 position)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (_freeSegments.Count > 0)
		{
			MatrixFrame globalFrame = _freeSegments[0].RopeEntity.GetGlobalFrame();
			globalFrame.origin = position;
			_freeSegments[0].RopeEntity.SetGlobalFrame(ref globalFrame, true);
		}
	}

	public void SetFirstFixedGlobalPosition(Vec3 position)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (_fixedSegments.Count > 0)
		{
			MatrixFrame globalFrame = _fixedSegments[0].RopeEntity.GetGlobalFrame();
			globalFrame.origin = position;
			_fixedSegments[0].RopeEntity.SetGlobalFrame(ref globalFrame, true);
		}
	}

	public void FillBurningRecord(BurningSystem system)
	{
		float nodeLength = 2f;
		string prefabName = "burning_node_rope";
		if (_endPointRope.RopeSegment == null)
		{
			return;
		}
		_endPointRope.RopeSegment.FillBurningRecordForSegment(system, prefabName, nodeLength, reversePlacement: true);
		foreach (SegmentData item in (MBRandom.RandomFloat > 0.5f) ? _freeSegments : _fixedSegments)
		{
			item.RopeSegment.FillBurningRecordForSegment(system, prefabName, nodeLength, reversePlacement: true);
		}
	}

	public void SetAlpha(float value)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		if (value <= 0f)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
			return;
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(true);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetAlpha(MathF.Clamp(value, 0f, 1f));
	}

	public void GetAllRopeSegments(ref List<RopeSegment> segments, float maximumRopeThickness)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		foreach (SegmentData freeSegment in _freeSegments)
		{
			if ((NativeObject)(object)freeSegment.RopeSegment.RopeMesh != (NativeObject)null && freeSegment.RopeSegment.RopeMesh.GetVectorArgument().w < maximumRopeThickness)
			{
				segments.Add(freeSegment.RopeSegment);
			}
		}
		foreach (SegmentData fixedSegment in _fixedSegments)
		{
			if ((NativeObject)(object)fixedSegment.RopeSegment.RopeMesh != (NativeObject)null && fixedSegment.RopeSegment.RopeMesh.GetVectorArgument().w < maximumRopeThickness)
			{
				segments.Add(fixedSegment.RopeSegment);
			}
		}
		if (_endPointRope.RopeSegment != null && (NativeObject)(object)_endPointRope.RopeSegment.RopeMesh != (NativeObject)null && _endPointRope.RopeSegment.RopeMesh.GetVectorArgument().w < maximumRopeThickness)
		{
			segments.Add(_endPointRope.RopeSegment);
		}
	}
}
