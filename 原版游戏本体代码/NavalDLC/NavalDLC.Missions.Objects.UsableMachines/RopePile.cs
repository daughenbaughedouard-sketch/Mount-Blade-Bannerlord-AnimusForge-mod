using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class RopePile : ScriptComponentBehavior
{
	public Vec3 point0 = new Vec3(0f, 0f, 0f, 0f);

	public Vec3 point1 = new Vec3(0f, 0f, 0f, 0f);

	public Vec3 point2 = new Vec3(0f, 0f, 0f, 0f);

	public Vec3 point3 = new Vec3(0f, 0f, 0f, 0f);

	public float factor;

	public override TickRequirement GetTickRequirement()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(((ScriptComponentBehavior)this).GetTickRequirement() | 2);
	}

	protected override void OnInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetFirstMesh().SetupAdditionalBoneBuffer(1);
	}

	protected override void OnTick(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Mesh firstMesh = ((WeakGameEntity)(ref gameEntity)).GetFirstMesh();
		Mat3 val = new Mat3(ref point0, ref point1, ref point2);
		MatrixFrame val2 = new MatrixFrame(ref val, ref point3);
		firstMesh.SetAdditionalBoneFrame(0, ref val2);
		Vec3 vectorArgument = firstMesh.GetVectorArgument();
		vectorArgument.z = factor;
		firstMesh.SetVectorArgument(vectorArgument.x, vectorArgument.y, vectorArgument.z, vectorArgument.w);
	}
}
