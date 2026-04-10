using System.Collections.Generic;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.Behaviors;

public sealed class BehaviorNavalRemoveConnection : NavalBehaviorComponent
{
	private NavalShipsLogic _navalShipsLogic;

	private MissionShip _formationMainShip;

	private bool _readyToSeparate;

	public BehaviorNavalRemoveConnection(Formation formation)
		: base(formation)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).BehaviorCoherence = 0.8f;
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		((BehaviorComponent)this).CalculateCurrentOrder();
	}

	public override void RefreshShipReferences()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
	}

	protected override void CalculateCurrentOrder()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).CurrentOrder = ((_formationMainShip != null) ? NavalOrderController.GetNavalDefensiveMovementOrder(_formationMainShip) : MovementOrder.MovementOrderStop);
	}

	public override void OnDeploymentFinished()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).OnDeploymentFinished();
		_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
	}

	public override void ResetBehavior()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).ResetBehavior();
		_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
	}

	protected override void OnBehaviorActivatedAux()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		_readyToSeparate = false;
		_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		if (_formationMainShip != null)
		{
			_formationMainShip.ShipOrder.SetBoardingTargetShip(null);
			_formationMainShip.ShipOrder.SetCutLoose(enable: false);
			_formationMainShip.ShipOrder.SetOrderOarsmenLevel(2);
			_formationMainShip.ShipOrder.SetShipStopOrder();
		}
		((BehaviorComponent)this).CalculateCurrentOrder();
		((BehaviorComponent)this).Formation.SetMovementOrder(((BehaviorComponent)this).CurrentOrder);
		((BehaviorComponent)this).Formation.SetFacingOrder(((BehaviorComponent)this).CurrentFacingOrder);
		((BehaviorComponent)this).Formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
		((BehaviorComponent)this).Formation.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
		((BehaviorComponent)this).Formation.SetFormOrder(FormOrder.FormOrderWide, true);
	}

	public override void TickOccasionally()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).CalculateCurrentOrder();
		((BehaviorComponent)this).Formation.SetMovementOrder(((BehaviorComponent)this).CurrentOrder);
		if (!_readyToSeparate && _formationMainShip != null)
		{
			int num = 0;
			foreach (IFormationUnit item in (List<IFormationUnit>)(object)((BehaviorComponent)this).Formation.UnitsWithoutLooseDetachedOnes)
			{
				Agent val;
				if ((val = (Agent)(object)((item is Agent) ? item : null)) != null)
				{
					int currentNavigationFaceId = val.GetCurrentNavigationFaceId();
					if (currentNavigationFaceId >= 0 && !_formationMainShip.IsAgentOnShipNavmesh(currentNavigationFaceId))
					{
						num++;
					}
				}
			}
			if ((float)num <= (float)((BehaviorComponent)this).Formation.CountOfUnitsWithoutLooseDetachedOnes * 0.2f)
			{
				_readyToSeparate = true;
			}
		}
		if (_readyToSeparate)
		{
			_formationMainShip.ShipOrder.SetCutLoose(enable: true);
		}
	}

	protected override float GetAiWeight()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (_formationMainShip.Formation != ((BehaviorComponent)this).Formation)
		{
			_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		}
		if (!_formationMainShip.GetIsConnected() || _formationMainShip.SearchShipConnection(null, isDirect: true, findEnemy: true, enforceActive: true, acceptNotBridgedConnections: true))
		{
			return 0f;
		}
		return 5000f;
	}
}
