using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NetworkMessages.FromClient;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace NavalDLC.Missions.AI.TeamAI;

internal class NavalOrderController : OrderController
{
	private readonly NavalShipsLogic _navalShipsLogic;

	public NavalOrderController(Mission mission, Team team, Agent owner)
		: base(mission, team, owner)
	{
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
	}

	protected override void SelectAllFormations(Agent selectorAgent, bool uiFeedback)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new SelectAllFormations());
			GameNetwork.EndModuleEventAsClient();
		}
		if (uiFeedback && selectorAgent != null && ((OrderController)this).AreGesturesEnabled())
		{
			selectorAgent.MakeVoice(VoiceType.Everyone, (CombatVoiceNetworkPredictionType)2);
		}
		((List<Formation>)(object)base._selectedFormations).Clear();
		IEnumerable<Formation> enumerable = ((IEnumerable<Formation>)base.Team.FormationsIncludingEmpty).Where((Formation f) => ((OrderController)this).IsFormationSelectable(f, selectorAgent));
		if (enumerable.Count() == 1)
		{
			((List<Formation>)(object)base._selectedFormations).Add(enumerable.First());
		}
		else
		{
			foreach (Formation item in enumerable)
			{
				if (!NavalDLCHelpers.IsAgentCaptainOfFormationShip(selectorAgent, item))
				{
					((List<Formation>)(object)base._selectedFormations).Add(item);
				}
			}
		}
		((OrderController)this).OnSelectedFormationsCollectionChanged();
	}

	protected override void SelectFormation(Formation formation, Agent selectorAgent)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		if (((List<Formation>)(object)base._selectedFormations).Contains(formation) || !((OrderController)this).IsFormationSelectable(formation, selectorAgent))
		{
			return;
		}
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new SelectFormation(formation.Index));
			GameNetwork.EndModuleEventAsClient();
		}
		if (selectorAgent != null && ((OrderController)this).AreGesturesEnabled())
		{
			OrderController.PlayFormationSelectedGesture(formation, selectorAgent);
		}
		if (NavalDLCHelpers.IsAgentCaptainOfFormationShip(selectorAgent, formation))
		{
			((List<Formation>)(object)base._selectedFormations).Clear();
		}
		else
		{
			((List<Formation>)(object)base._selectedFormations).RemoveAll((Predicate<Formation>)((Formation x) => NavalDLCHelpers.IsAgentCaptainOfFormationShip(selectorAgent, x)));
		}
		((List<Formation>)(object)base._selectedFormations).Add(formation);
		((OrderController)this).OnSelectedFormationsCollectionChanged();
	}

	public override void SetOrderWithTwoPositions(OrderType orderType, WorldPosition position1, WorldPosition position2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((OrderController)this).SetOrderWithPosition(orderType, position1);
	}

	public override void SetOrderWithPosition(OrderType orderType, WorldPosition position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		((OrderController)this).BeforeSetOrder(orderType);
		SetSkirmishState(isSkirmishing: false);
		SetDefensiveState(isDefensive: false);
		MBList<Formation> val = Extensions.ToMBList<Formation>(((IEnumerable<Formation>)(((List<Formation>)(object)((OrderController)this).SelectedFormations)[0].Team.TeamAI as TeamAINavalComponent).TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Where((Formation sf) => ((List<Formation>)(object)((OrderController)this).SelectedFormations).Contains(sf)));
		for (int num = 0; num < ((List<Formation>)(object)val).Count; num++)
		{
			Formation val2 = ((List<Formation>)(object)val)[num];
			float num2 = (0f - ((float)((List<Formation>)(object)val).Count - 1f) * 0.5f + (float)num) * 20f;
			Vec2 asVec = ((WorldPosition)(ref position)).AsVec2;
			Vec2 val3 = (((List<Formation>)(object)((OrderController)this).SelectedFormations)[0].Team.TeamAI as TeamAINavalComponent).TeamNavalQuerySystem.AverageEnemyShipPosition - (((List<Formation>)(object)((OrderController)this).SelectedFormations)[0].Team.TeamAI as TeamAINavalComponent).TeamNavalQuerySystem.AverageShipPosition;
			val3 = ((Vec2)(ref val3)).RightVec();
			Vec2 targetPosition = asVec + num2 * ((Vec2)(ref val3)).Normalized();
			_navalShipsLogic.GetShip(val2.Team.TeamSide, val2.FormationIndex, out var ship);
			if (!ship.IsPlayerControlled)
			{
				ship.ShipOrder.SetShipMovementOrder(in targetPosition);
			}
		}
		((OrderController)this).FireOnOrderIssued(orderType, (MBReadOnlyList<Formation>)(object)val, (OrderController)(object)this, Array.Empty<object>());
	}

	public override void SetOrder(OrderType orderType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected I4, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		switch (orderType - 4)
		{
		default:
			if ((int)orderType != 34)
			{
				if ((int)orderType != 35)
				{
					goto case 4;
				}
				((OrderController)this).BeforeSetOrder(orderType);
				SetNavalSkirmishWithTargetFormation(null);
				SetSkirmishState(isSkirmishing: true);
				SetDefensiveState(isDefensive: false);
				break;
			}
			((OrderController)this).BeforeSetOrder(orderType);
			SetNavalTroopsDefensive();
			SetSkirmishState(isSkirmishing: false);
			SetDefensiveState(isDefensive: true);
			break;
		case 3:
			((OrderController)this).BeforeSetOrder(orderType);
			SetNavalFollowMeOrder();
			SetSkirmishState(isSkirmishing: false);
			SetDefensiveState(isDefensive: false);
			break;
		case 0:
		case 1:
			((OrderController)this).SetOrder(orderType);
			SetDefensiveState(isDefensive: false);
			break;
		case 8:
			((OrderController)this).BeforeSetOrder(orderType);
			SetNavalEngageWithTargetFormation(null);
			break;
		case 2:
			((OrderController)this).BeforeSetOrder(orderType);
			SetNavalStop();
			break;
		case 5:
			((OrderController)this).BeforeSetOrder(orderType);
			SetNavalRetreat();
			break;
		case 4:
		case 6:
		case 7:
		case 9:
			((OrderController)this).SetOrder(orderType);
			break;
		case 10:
		case 11:
			break;
		}
		((OrderController)this).FireOnOrderIssued(orderType, ((OrderController)this).SelectedFormations, (OrderController)(object)this, Array.Empty<object>());
	}

	public override void SetOrderWithAgent(OrderType orderType, Agent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		((OrderController)this).SetOrderWithAgent(orderType, agent);
		if (!NavalDLCHelpers.IsShipOrdersAvailable())
		{
			SetSkirmishState(isSkirmishing: false);
			SetDefensiveState(isDefensive: false);
		}
	}

	private void SetSkirmishState(bool isSkirmishing)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((List<Formation>)(object)((OrderController)this).SelectedFormations).Count; i++)
		{
			((List<Formation>)(object)((OrderController)this).SelectedFormations)[i].SetRidingOrder(isSkirmishing ? RidingOrder.RidingOrderDismount : RidingOrder.RidingOrderFree);
		}
	}

	private void SetDefensiveState(bool isDefensive)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((List<Formation>)(object)((OrderController)this).SelectedFormations).Count; i++)
		{
			((List<Formation>)(object)((OrderController)this).SelectedFormations)[i].SetRidingOrder(isDefensive ? RidingOrder.RidingOrderMount : RidingOrder.RidingOrderFree);
		}
	}

	public override void SetOrderWithFormation(OrderType orderType, Formation orderFormation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)orderType == 12)
		{
			((OrderController)this).BeforeSetOrder(orderType);
			SetNavalEngageWithTargetFormation(orderFormation);
			((OrderController)this).FireOnOrderIssued(orderType, ((OrderController)this).SelectedFormations, (OrderController)(object)this, Array.Empty<object>());
		}
		else if ((int)orderType == 35)
		{
			((OrderController)this).BeforeSetOrder(orderType);
			SetNavalSkirmishWithTargetFormation(orderFormation);
			((OrderController)this).FireOnOrderIssued(orderType, ((OrderController)this).SelectedFormations, (OrderController)(object)this, Array.Empty<object>());
		}
		else
		{
			((OrderController)this).SetOrderWithFormation(orderType, orderFormation);
		}
		SetSkirmishState(isSkirmishing: false);
		SetDefensiveState(isDefensive: false);
	}

	public override void SetOrderWithOrderableObject(IOrderable target)
	{
		((OrderController)this).BeforeSetOrder((OrderType)7);
		SetNavalFollowOrder(target as MissionShip);
		((OrderController)this).FireOnOrderIssued((OrderType)7, ((OrderController)this).SelectedFormations, (OrderController)(object)this, Array.Empty<object>());
		SetSkirmishState(isSkirmishing: false);
		SetDefensiveState(isDefensive: false);
	}

	private void SetNavalFollowOrder(MissionShip targetShip)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		MBList<Formation> val = Extensions.ToMBList<Formation>(((IEnumerable<Formation>)(((List<Formation>)(object)((OrderController)this).SelectedFormations)[0].Team.TeamAI as TeamAINavalComponent).TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Where((Formation sf) => ((List<Formation>)(object)((OrderController)this).SelectedFormations).Contains(sf)));
		for (int num = 0; num < ((List<Formation>)(object)val).Count; num++)
		{
			Formation val2 = ((List<Formation>)(object)((OrderController)this).SelectedFormations)[num];
			float offsetDistance = (0f - ((float)((List<Formation>)(object)val).Count - 1f) * 0.5f + (float)num) * 20f;
			_navalShipsLogic.GetShip(val2.Team.TeamSide, val2.FormationIndex, out var ship);
			if (ship != targetShip)
			{
				ship.ShipOrder.SetShipFollowOrder(targetShip, offsetDistance);
				ship.ShipOrder.SetCutLoose(enable: true);
			}
		}
	}

	private void SetNavalFollowMeOrder()
	{
		MissionShip formationShip = Agent.Main.GetComponent<AgentNavalComponent>().FormationShip;
		SetNavalFollowOrder(formationShip);
	}

	private void SetNavalEngageWithTargetFormation(Formation targetFormation)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Formation item in (List<Formation>)(object)((OrderController)this).SelectedFormations)
		{
			if (targetFormation == null && item.CachedClosestEnemyFormation == null)
			{
				continue;
			}
			bool num = targetFormation != null;
			_navalShipsLogic.GetShip(item.Team.TeamSide, item.FormationIndex, out var ship);
			if (num)
			{
				_navalShipsLogic.GetShip(targetFormation.Team.TeamSide, targetFormation.FormationIndex, out var ship2);
				ship.ShipOrder.SetShipEngageOrder(ship2);
				ship.ShipOrder.SetBoardingTargetShip(ship2);
				continue;
			}
			ship.ShipOrder.SetShipEngageOrder();
			if (ship.ShipOrder.TargetShip != null)
			{
				ship.ShipOrder.SetBoardingTargetShip(ship.ShipOrder.TargetShip);
			}
		}
	}

	private void SetNavalSkirmishWithTargetFormation(Formation targetFormation)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		foreach (Formation item in (List<Formation>)(object)((OrderController)this).SelectedFormations)
		{
			if (targetFormation != null || item.CachedClosestEnemyFormation != null)
			{
				_navalShipsLogic.GetShip(item.Team.TeamSide, item.FormationIndex, out var ship);
				if (targetFormation != null)
				{
					_navalShipsLogic.GetShip(targetFormation.Team.TeamSide, targetFormation.FormationIndex, out var ship2);
					ship.ShipOrder.SetShipSkirmishOrder(ship2);
				}
				else
				{
					ship.ShipOrder.SetShipSkirmishOrder();
				}
				ship.ShipOrder.SetCutLoose(enable: true);
			}
		}
	}

	private void SetNavalStop()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		foreach (Formation item in (List<Formation>)(object)((OrderController)this).SelectedFormations)
		{
			_navalShipsLogic.GetShip(item.Team.TeamSide, item.FormationIndex, out var ship);
			ship.ShipOrder.SetShipStopOrder();
			ship.ShipOrder.SetBoardingTargetShip(null);
			ship.ShipOrder.SetCutLoose(enable: false);
		}
	}

	private void SetNavalRetreat()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		foreach (Formation item in (List<Formation>)(object)((OrderController)this).SelectedFormations)
		{
			_navalShipsLogic.GetShip(item.Team.TeamSide, item.FormationIndex, out var ship);
			ship.ShipOrder.SetShipRetreatOrder();
			ship.ShipOrder.SetCutLoose(enable: true);
		}
	}

	private void SetNavalTroopsAggressive()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (Formation item in (List<Formation>)(object)((OrderController)this).SelectedFormations)
		{
			item.SetMovementOrder(MovementOrder.MovementOrderCharge);
			item.SetRidingOrder(RidingOrder.RidingOrderDismount);
		}
	}

	public static MovementOrder GetNavalDefensiveMovementOrder(MissionShip missionShip)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		missionShip.GetWorldPositionOnDeck(out var worldPosition);
		return MovementOrder.MovementOrderMove(worldPosition);
	}

	private void SetNavalTroopsDefensive()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		foreach (Formation item in (List<Formation>)(object)((OrderController)this).SelectedFormations)
		{
			_navalShipsLogic.GetShip(item.Team.TeamSide, item.FormationIndex, out var ship);
			ship?.SetPositioningOrdersToRallyPoint(applyToPlayerFormation: true, playersOrder: true);
			item.SetRidingOrder(RidingOrder.RidingOrderMount);
		}
	}
}
