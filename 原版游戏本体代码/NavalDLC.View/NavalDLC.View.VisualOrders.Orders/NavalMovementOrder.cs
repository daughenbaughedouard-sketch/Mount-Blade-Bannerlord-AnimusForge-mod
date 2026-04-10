using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;

namespace NavalDLC.View.VisualOrders.Orders;

public class NavalMovementOrder : VisualOrder
{
	private OrderType _orderType;

	private bool _useWorldPosition;

	private bool _isTargeted;

	private TextObject _name;

	public NavalMovementOrder(string stringId, OrderType order, TextObject name, bool useWorldPosition = false, bool isTargeted = false)
		: base(stringId)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_orderType = order;
		_useWorldPosition = useWorldPosition;
		_isTargeted = isTargeted;
		_name = name;
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (_useWorldPosition && executionParameters.HasWorldPosition)
		{
			orderController.SetOrderWithPosition(_orderType, executionParameters.WorldPosition);
		}
		else if (_isTargeted && executionParameters.HasFormation)
		{
			orderController.SetOrderWithFormation(_orderType, executionParameters.Formation);
		}
		else
		{
			orderController.SetOrder(_orderType);
		}
	}

	public override TextObject GetName(OrderController orderController)
	{
		return _name;
	}

	public override bool IsTargeted()
	{
		return _isTargeted;
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		if (missionBehavior == null)
		{
			return false;
		}
		ShipOrder.ShipMovementOrderEnum movementOrderEnum = GetMovementOrderEnum();
		missionBehavior.GetShip(formation.Team.TeamSide, formation.FormationIndex, out var ship);
		if (ship == null)
		{
			return false;
		}
		if (ship.IsPlayerShip || ship.IsPlayerControlled)
		{
			return null;
		}
		return ship.ShipOrder.MovementOrderEnum == movementOrderEnum;
	}

	private ShipOrder.ShipMovementOrderEnum GetMovementOrderEnum()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected I4, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		OrderType orderType = _orderType;
		if ((int)orderType != 1)
		{
			switch (orderType - 6)
			{
			case 1:
				return ShipOrder.ShipMovementOrderEnum.StaticOrderCount;
			case 6:
				return ShipOrder.ShipMovementOrderEnum.Engage;
			case 0:
				return ShipOrder.ShipMovementOrderEnum.Stop;
			case 3:
				return ShipOrder.ShipMovementOrderEnum.Retreat;
			default:
				Debug.FailedAssert("Failed to find corresponding ship order of: " + _orderType, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.View\\VisualOrders\\Orders\\NavalMovementOrder.cs", "GetMovementOrderEnum", 96);
				return ShipOrder.ShipMovementOrderEnum.Move;
			}
		}
		return ShipOrder.ShipMovementOrderEnum.Move;
	}
}
