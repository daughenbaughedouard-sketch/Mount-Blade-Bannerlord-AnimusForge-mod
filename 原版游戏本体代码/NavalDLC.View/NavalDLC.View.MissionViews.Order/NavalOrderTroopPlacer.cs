using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;

namespace NavalDLC.View.MissionViews.Order;

public class NavalOrderTroopPlacer : OrderTroopPlacer
{
	private NavalShipsLogic _navalShipsLogic;

	public NavalOrderTroopPlacer(OrderController orderController)
		: base(orderController)
	{
	}

	public override void AfterStart()
	{
		((OrderTroopPlacer)this).AfterStart();
		((OrderTroopPlacer)this).OrderFlag.IsVisible = false;
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
	}

	protected override bool CanUpdate()
	{
		if (((OrderTroopPlacer)this).OrderController == Mission.Current.PlayerEnemyTeam.MasterOrderController)
		{
			return ((OrderTroopPlacer)this).CanUpdate();
		}
		if (((OrderTroopPlacer)this).CanUpdate())
		{
			NavalShipsLogic navalShipsLogic = _navalShipsLogic;
			if (navalShipsLogic == null)
			{
				return false;
			}
			return navalShipsLogic.GetNumTeamShips((TeamSideEnum)0) > 0;
		}
		return false;
	}

	protected override OrderFlag CreateOrderFlag()
	{
		return (OrderFlag)(object)new NavalOrderFlag(((MissionBehavior)this).Mission, ((MissionView)this).MissionScreen);
	}

	protected override CursorState GetCursorState()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionBehavior)this).Mission.IsNavalBattle)
		{
			return ((OrderTroopPlacer)this).GetGroundOrNormalCursor();
		}
		return ((OrderTroopPlacer)this).GetCursorState();
	}

	protected override bool TryGetScreenMiddleToWorldPosition(out WorldPosition worldPosition, out float collisionDistance, out WeakGameEntity collidedEntity)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionBehavior)this).Mission.IsNavalBattle)
		{
			Vec3 val = default(Vec3);
			if (((MissionView)this).MissionScreen.GetProjectedMousePositionOnWater(ref val))
			{
				worldPosition = new WorldPosition(((MissionBehavior)this).Mission.Scene, val);
				Vec3 val2 = val - ((MissionBehavior)this).Mission.GetCameraFrame().origin;
				collisionDistance = ((Vec3)(ref val2)).Length;
				collidedEntity = WeakGameEntity.Invalid;
				return true;
			}
			worldPosition = WorldPosition.Invalid;
			collisionDistance = 0f;
			collidedEntity = WeakGameEntity.Invalid;
			return false;
		}
		return ((OrderTroopPlacer)this).TryGetScreenMiddleToWorldPosition(ref worldPosition, ref collisionDistance, ref collidedEntity);
	}

	protected override Vec3 GetGroundedVec3(WorldPosition worldPosition)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionBehavior)this).Mission.IsNavalBattle)
		{
			Vec2 asVec = ((WorldPosition)(ref worldPosition)).AsVec2;
			return new Vec3(((Vec2)(ref asVec)).X, ((Vec2)(ref asVec)).Y, ((MissionBehavior)this).Mission.Scene.GetWaterLevelAtPosition(asVec, true, true), -1f);
		}
		return ((OrderTroopPlacer)this).GetGroundedVec3(worldPosition);
	}
}
