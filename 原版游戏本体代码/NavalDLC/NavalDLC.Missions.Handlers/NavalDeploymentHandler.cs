using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.Deployment;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Handlers;

public class NavalDeploymentHandler : DeploymentHandler
{
	private NavalMissionDeploymentPlanningLogic _navalDeploymentPlan;

	private NavalShipsLogic _navalShipsLogic;

	public NavalDeploymentHandler(bool isPlayerAttacker)
		: base(isPlayerAttacker)
	{
	}

	public override void OnBehaviorInitialize()
	{
		((DeploymentHandler)this).OnBehaviorInitialize();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		((MissionBehavior)this).Mission.GetDeploymentPlan<NavalMissionDeploymentPlanningLogic>(ref _navalDeploymentPlan);
	}

	public override void AfterStart()
	{
		((DeploymentHandler)this).AfterStart();
	}

	public override void AutoDeployTeamUsingDeploymentPlan(Team team)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		((MissionDeploymentPlanningLogic)_navalDeploymentPlan).RemakeDeploymentPlan(((MissionBehavior)this).Mission.PlayerTeam);
		List<Formation> list = ((IEnumerable<Formation>)team.FormationsIncludingEmpty).ToList();
		if (list.Count > 0)
		{
			bool isTeleportingShips = _navalShipsLogic.IsTeleportingShips;
			_navalShipsLogic.SetTeleportShips(value: true);
			MBQueue<(MissionShip, Oriented2DArea)> val = new MBQueue<(MissionShip, Oriented2DArea)>();
			Vec2 targetDirection;
			foreach (Formation item4 in list)
			{
				FormationClass formationIndex = item4.FormationIndex;
				ShipAssignment shipAssignment = _navalShipsLogic.GetShipAssignment(team.TeamSide, formationIndex);
				IFormationDeploymentPlan formationPlan = ((MissionDeploymentPlanningLogic)_navalDeploymentPlan).GetFormationPlan(team, formationIndex, false);
				MissionShip missionShip = shipAssignment.MissionShip;
				if (missionShip != null && formationPlan != null && formationPlan.HasFrame())
				{
					MatrixFrame frame = formationPlan.GetFrame();
					Vec2 asVec = ((Vec3)(ref frame.origin)).AsVec2;
					targetDirection = ((Vec3)(ref frame.rotation.f)).AsVec2;
					Vec2 val2 = ((Vec2)(ref targetDirection)).Normalized();
					targetDirection = missionShip.MissionShipObject.DeploymentArea;
					Oriented2DArea item = new Oriented2DArea(ref asVec, ref val2, ref targetDirection);
					((Queue<(MissionShip, Oriented2DArea)>)(object)val).Enqueue((missionShip, item));
				}
			}
			int num = 0;
			int num2 = ((Queue<(MissionShip, Oriented2DArea)>)(object)val).Count * 5;
			while (!Extensions.IsEmpty<(MissionShip, Oriented2DArea)>((IEnumerable<(MissionShip, Oriented2DArea)>)val) && num < num2)
			{
				var (missionShip2, area) = ((Queue<(MissionShip, Oriented2DArea)>)(object)val).Dequeue();
				if (_navalShipsLogic.IsAreaFreeOfShipCollision(in area, 1f, missionShip2.Index))
				{
					ShipOrder shipOrder = missionShip2.ShipOrder;
					Vec2 globalCenter = ((Oriented2DArea)(ref area)).GlobalCenter;
					targetDirection = ((Oriented2DArea)(ref area)).GlobalForward;
					shipOrder.SetShipMovementOrder(globalCenter, in targetDirection);
				}
				else
				{
					((Queue<(MissionShip, Oriented2DArea)>)(object)val).Enqueue((missionShip2, area));
				}
				num++;
			}
			while (!Extensions.IsEmpty<(MissionShip, Oriented2DArea)>((IEnumerable<(MissionShip, Oriented2DArea)>)val))
			{
				(MissionShip, Oriented2DArea) tuple2 = ((Queue<(MissionShip, Oriented2DArea)>)(object)val).Dequeue();
				MissionShip item2 = tuple2.Item1;
				Oriented2DArea item3 = tuple2.Item2;
				ShipOrder shipOrder2 = item2.ShipOrder;
				Vec2 globalCenter2 = ((Oriented2DArea)(ref item3)).GlobalCenter;
				targetDirection = ((Oriented2DArea)(ref item3)).GlobalForward;
				shipOrder2.SetShipMovementOrder(globalCenter2, in targetDirection);
			}
			if ((team.IsPlayerTeam ? team.PlayerOrderController : team.MasterOrderController) is NavalOrderController navalOrderController)
			{
				((OrderController)navalOrderController).SelectAllFormations(false);
				((OrderController)navalOrderController).SetOrder((OrderType)37);
				((OrderController)navalOrderController).SetFormationUpdateEnabledAfterSetOrder(false);
				((OrderController)navalOrderController).SetOrder((OrderType)34);
				((OrderController)navalOrderController).SetOrder((OrderType)32);
				((OrderController)navalOrderController).SetOrder((OrderType)6);
				((OrderController)navalOrderController).SetFormationUpdateEnabledAfterSetOrder(true);
				((OrderController)navalOrderController).ClearSelectedFormations();
				Formation val3 = ((IEnumerable<Formation>)team.FormationsIncludingEmpty).FirstOrDefault((Func<Formation, bool>)((Formation x) => NavalDLCHelpers.IsPlayerCaptainOfFormationShip(x)));
				if (val3 != null)
				{
					((OrderController)navalOrderController).SelectFormation(val3);
					((OrderController)navalOrderController).SetOrder((OrderType)34);
					((OrderController)navalOrderController).SetFormationUpdateEnabledAfterSetOrder(true);
					((OrderController)navalOrderController).ClearSelectedFormations();
				}
			}
			else
			{
				Debug.FailedAssert("Team order controller is not of type naval order controller", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\MissionLogics\\NavalDeploymentHandler.cs", "AutoDeployTeamUsingDeploymentPlan", 148);
			}
			_navalShipsLogic.SetTeleportShips(isTeleportingShips);
		}
		if (team.IsPlayerTeam && base._deploymentMissionController is NavalDeploymentMissionController navalDeploymentMissionController)
		{
			navalDeploymentMissionController.OnPlayerShipsUpdated();
		}
	}

	public override void ForceUpdateAllUnits()
	{
	}
}
