using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.Screens;

namespace NavalDLC.View.MissionViews.Order;

public class NavalOrderFlag : OrderFlag
{
	public NavalOrderFlag(Mission mission, MissionScreen missionScreen, float flagScale = 20f)
		: base(mission, missionScreen, flagScale)
	{
	}

	protected override Vec3 GetFlagPosition(out bool isOnValidGround, bool checkForTargetEntity, Vec3 targetCollisionPoint)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!base._mission.IsNavalBattle)
		{
			return ((OrderFlag)this).GetFlagPosition(ref isOnValidGround, checkForTargetEntity, targetCollisionPoint);
		}
		Vec3 val = default(Vec3);
		if (base._missionScreen.GetProjectedMousePositionOnWater(ref val))
		{
			((Vec3)(ref val))._002Ector(val.x, val.y, base._mission.Scene.GetWaterLevelAtPosition(((Vec3)(ref val)).AsVec2, true, true), -1f);
			WorldPosition val2 = default(WorldPosition);
			((WorldPosition)(ref val2))._002Ector(Mission.Current.Scene, UIntPtr.Zero, val, false);
			isOnValidGround = ((OrderFlag)this).IsPositionOnValidGround(val2);
			return val;
		}
		isOnValidGround = false;
		return new Vec3(0f, 0f, -10000f, -1f);
	}

	public override bool IsPositionOnValidGround(WorldPosition worldPosition)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!base._mission.IsNavalBattle)
		{
			return ((OrderFlag)this).IsPositionOnValidGround(worldPosition);
		}
		if ((int)Mission.Current.Mode == 6 && Mission.Current.DeploymentPlan.HasDeploymentBoundaries(Mission.Current.PlayerTeam))
		{
			IMissionDeploymentPlan deploymentPlan = Mission.Current.DeploymentPlan;
			Team playerTeam = Mission.Current.PlayerTeam;
			Vec2 asVec = ((WorldPosition)(ref worldPosition)).AsVec2;
			if (!deploymentPlan.IsPositionInsideDeploymentBoundaries(playerTeam, ref asVec))
			{
				return false;
			}
		}
		return true;
	}
}
