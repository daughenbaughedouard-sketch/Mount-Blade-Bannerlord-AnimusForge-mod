using System.Collections.Generic;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;

namespace NavalDLC.View.VisualOrders.Orders;

public class NavalSkirmishOrder : VisualOrder
{
	private NavalShipsLogic _shipsLogic;

	public NavalSkirmishOrder(string stringId)
		: base(stringId)
	{
	}

	public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!executionParameters.HasFormation)
		{
			orderController.SetOrder((OrderType)35);
		}
		else
		{
			orderController.SetOrderWithFormation((OrderType)35, executionParameters.Formation);
		}
	}

	public override TextObject GetName(OrderController orderController)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=skirmishOrder}Skirmish", (Dictionary<string, object>)null);
	}

	public override bool IsTargeted()
	{
		return true;
	}

	protected override bool? OnGetFormationHasOrder(Formation formation)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (_shipsLogic == null)
		{
			_shipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		}
		if (_shipsLogic != null)
		{
			_shipsLogic.GetShip(formation.Team.TeamSide, formation.FormationIndex, out var ship);
			if (ship != null)
			{
				return ship.ShipOrder.MovementOrderEnum == ShipOrder.ShipMovementOrderEnum.Skirmish;
			}
		}
		return VisualOrderHelper.DoesFormationHaveOrderType(formation, (OrderType)35);
	}
}
