using System.Collections.Generic;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class MissionBoundaryWallView : MissionView
{
	public override void OnMissionScreenInitialize()
	{
		base.OnMissionScreenInitialize();
		foreach (ICollection<Vec2> value in ((MissionBehavior)this).Mission.Boundaries.Values)
		{
			CreateBoundaryEntity(value);
		}
	}

	private void CreateBoundaryEntity(ICollection<Vec2> boundaryPoints)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Mesh val = BoundaryWallView.CreateBoundaryMesh(((MissionBehavior)this).Mission.Scene, boundaryPoints, 536918784u);
		if ((NativeObject)(object)val != (NativeObject)null)
		{
			GameEntity obj = GameEntity.CreateEmpty(((MissionBehavior)this).Mission.Scene, true, true, true);
			obj.AddMesh(val, true);
			MatrixFrame identity = MatrixFrame.Identity;
			obj.SetGlobalFrame(ref identity, true);
			obj.Name = "boundary_wall";
			obj.SetMobility((Mobility)0);
			obj.EntityFlags = (EntityFlags)(obj.EntityFlags | 0x40000000);
		}
	}
}
