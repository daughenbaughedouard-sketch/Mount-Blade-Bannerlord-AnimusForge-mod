using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.View;

public static class NavalViewExtensions
{
	public static BoundingBox GetBoundingBoxIncludingChildren(this GameEntity entity)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		BoundingBox boundingBox = default(BoundingBox);
		GetBoundingBoxIncludingChildrenAux(entity, ref boundingBox);
		((BoundingBox)(ref boundingBox)).RecomputeRadius();
		return boundingBox;
	}

	private static void GetBoundingBoxIncludingChildrenAux(GameEntity entity, ref BoundingBox boundingBox)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		int componentCount = entity.GetComponentCount((ComponentType)0);
		for (int i = 0; i < componentCount; i++)
		{
			MetaMesh metaMesh = entity.GetMetaMesh(i);
			if ((NativeObject)(object)metaMesh != (NativeObject)null)
			{
				BoundingBox boundingBox2 = metaMesh.GetBoundingBox();
				((BoundingBox)(ref boundingBox)).RelaxMinMaxWithPoint(ref boundingBox2.min);
				((BoundingBox)(ref boundingBox)).RelaxMinMaxWithPoint(ref boundingBox2.max);
			}
		}
		Mesh firstMesh = entity.GetFirstMesh();
		if ((NativeObject)(object)firstMesh != (NativeObject)null)
		{
			Vec3 boundingBoxMin = firstMesh.GetBoundingBoxMin();
			((BoundingBox)(ref boundingBox)).RelaxMinMaxWithPoint(ref boundingBoxMin);
			boundingBoxMin = firstMesh.GetBoundingBoxMax();
			((BoundingBox)(ref boundingBox)).RelaxMinMaxWithPoint(ref boundingBoxMin);
		}
		for (int j = 0; j < entity.ChildCount; j++)
		{
			GetBoundingBoxIncludingChildrenAux(entity.GetChild(j), ref boundingBox);
		}
	}

	public static void FitEntityInsideView(this Camera camera, Vec3 normalizedCameraOffset, GameEntity entity)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		entity.RecomputeBoundingBox();
		float boundingBoxRadius = entity.GetBoundingBoxRadius();
		Vec3 val = entity.GetFrame().origin + (entity.GetBoundingBoxMin() + entity.GetBoundingBoxMax()) * 0.5f;
		float num = boundingBoxRadius / MathF.Abs(MathF.Sin(camera.HorizontalFov * 0.5f));
		Vec3 val2 = val + normalizedCameraOffset * num;
		camera.LookAt(val2, val, Vec3.Up);
	}
}
