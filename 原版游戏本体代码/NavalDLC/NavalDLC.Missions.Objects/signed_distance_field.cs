using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.Missions.Objects;

[ScriptComponentParams("ship_visual_only", "")]
public class signed_distance_field : ScriptComponentBehavior
{
	[EditableScriptComponentVariable(true, "SDF Texture")]
	private Texture _sdfTexture;

	[EditableScriptComponentVariable(true, "Visualize SDF")]
	private bool _visualizeSDF;

	private int _sdfIndex = -1;

	public void DummyFunc()
	{
		Debug.Print(_visualizeSDF.ToString(), 0, (DebugColor)12, 17592186044416uL);
	}

	private signed_distance_field()
	{
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)4;
	}

	protected override void OnInit()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (!((WeakGameEntity)(ref gameEntity)).IsGhostObject())
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			_sdfIndex = ((WeakGameEntity)(ref gameEntity)).RegisterWaterSDFClip(_sdfTexture);
			SetSDFParams();
		}
	}

	protected override void OnEditorInit()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (!((WeakGameEntity)(ref gameEntity)).IsGhostObject())
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			_sdfIndex = ((WeakGameEntity)(ref gameEntity)).RegisterWaterSDFClip(_sdfTexture);
			SetSDFParams();
		}
	}

	protected override void OnTickParallel(float dt)
	{
		SetSDFParams();
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
	}

	protected override void OnRemoved(int removeReason)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (_sdfIndex != -1)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).DeRegisterWaterSDFClip(_sdfIndex);
		}
	}

	private MatrixFrame ComputeBBOXFrame(ref Vec3 sdfBBExtend)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = default(Vec3);
		Vec3 val2 = default(Vec3);
		_sdfTexture.GetSDFBoundingBoxData(ref val, ref val2);
		BoundingBox val3 = default(BoundingBox);
		((BoundingBox)(ref val3)).BeginRelaxation();
		((BoundingBox)(ref val3)).RelaxMinMaxWithPoint(ref val);
		((BoundingBox)(ref val3)).RelaxMinMaxWithPoint(ref val2);
		((BoundingBox)(ref val3)).RecomputeRadius();
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = val3.center;
		sdfBBExtend = val3.max - val3.min;
		ref Vec3 s = ref identity.rotation.s;
		s *= sdfBBExtend.x * 0.5f;
		ref Vec3 f = ref identity.rotation.f;
		f *= sdfBBExtend.y * 0.5f;
		ref Vec3 u = ref identity.rotation.u;
		u *= sdfBBExtend.z * 0.5f;
		return identity;
	}

	private void SetSDFParams()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)_sdfTexture != (NativeObject)null && _sdfIndex != -1)
		{
			Vec3 sdfBBExtend = default(Vec3);
			MatrixFrame val = ComputeBBOXFrame(ref sdfBBExtend);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			val = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
			((MatrixFrame)(ref val)).Fill();
			MatrixFrame val2 = ((MatrixFrame)(ref val)).Inverse();
			((MatrixFrame)(ref val2)).Fill();
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			int sdfIndex = _sdfIndex;
			WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetWaterSDFClipData(sdfIndex, ref val2, ((WeakGameEntity)(ref gameEntity2)).IsVisibleIncludeParents());
		}
	}
}
