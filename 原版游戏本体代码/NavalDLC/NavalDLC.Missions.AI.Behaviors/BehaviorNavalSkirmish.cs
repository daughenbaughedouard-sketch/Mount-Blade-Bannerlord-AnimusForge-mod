using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.Behaviors;

public sealed class BehaviorNavalSkirmish : NavalBehaviorComponent
{
	private NavalShipsLogic _navalShipsLogic;

	private MissionShip _formationMainShip;

	public BehaviorNavalSkirmish(Formation formation)
		: base(formation)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).BehaviorCoherence = 0.8f;
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
	}

	private void CalculateAndSetShipOrders()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (((BehaviorComponent)this).Formation.CachedClosestEnemyFormation != null && _formationMainShip.IsFormationAndShipAIControlled)
		{
			MissionShip missionShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.Team.Team.TeamSide, ((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.Formation.FormationIndex).MissionShip;
			_formationMainShip.ShipOrder.SetShipSkirmishOrder(missionShip);
		}
	}

	public override void RefreshShipReferences()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
	}

	public override void OnDeploymentFinished()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).OnDeploymentFinished();
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
	}

	public override void ResetBehavior()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).ResetBehavior();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
	}

	protected override void OnBehaviorActivatedAux()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
		_formationMainShip.ShipOrder.SetBoardingTargetShip(null);
		_formationMainShip.ShipOrder.SetCutLoose(enable: false);
		_formationMainShip.ShipOrder.SetOrderOarsmenLevel(2);
		((BehaviorComponent)this).Formation.SetMovementOrder(((BehaviorComponent)this).CurrentOrder);
		((BehaviorComponent)this).Formation.SetFacingOrder(((BehaviorComponent)this).CurrentFacingOrder);
		((BehaviorComponent)this).Formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
		((BehaviorComponent)this).Formation.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
		((BehaviorComponent)this).Formation.SetFormOrder(FormOrder.FormOrderWide, true);
	}

	public override void TickOccasionally()
	{
		if (_navalShipsLogic == null)
		{
			_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
			if (_navalShipsLogic == null)
			{
				return;
			}
		}
		CalculateAndSetShipOrders();
	}

	protected override float GetAiWeight()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (_formationMainShip.Formation != ((BehaviorComponent)this).Formation)
		{
			_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		}
		float num = 0f;
		if (((BehaviorComponent)this).Formation.CachedClosestEnemyFormation != null)
		{
			num = ((!(((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.FormationMeleeFightingPower > 0f)) ? 5f : (((BehaviorComponent)this).Formation.QuerySystem.FormationMeleeFightingPower / ((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.FormationMeleeFightingPower));
		}
		return ((_formationMainShip == null || _formationMainShip.GetIsConnected()) ? 0f : 1.5f) * MathF.Clamp(num, 0f, 5f) * ((BehaviorComponent)this).Formation.QuerySystem.RangedUnitRatio;
	}
}
