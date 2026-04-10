using System.Collections.Generic;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class ShipRetreatLogic : MissionLogic
{
	private const float RetreatCheckInterval = 5f;

	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	private NavalBattleEndLogic _navalBattleEndLogic;

	private BasicMissionTimer _checkRetreatingTimer;

	private MBList<MissionShip> _tempRetreatedShips = new MBList<MissionShip>();

	private MBList<Agent> _tempOffShipAgents = new MBList<Agent>();

	private MBList<IAgentOriginBase> _tempRoutedReservedTroops = new MBList<IAgentOriginBase>();

	public override void OnBehaviorInitialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		((MissionBehavior)this).OnBehaviorInitialize();
		_checkRetreatingTimer = new BasicMissionTimer();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_navalBattleEndLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalBattleEndLogic>();
	}

	public override void OnDeploymentFinished()
	{
		_checkRetreatingTimer.Reset();
	}

	public override void OnMissionTick(float dt)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (!((MissionBehavior)this).Mission.IsDeploymentFinished || !(_checkRetreatingTimer.ElapsedTime > 5f))
		{
			return;
		}
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (item.IsShipOrderActive && item.ShipOrder.MovementOrderEnum == ShipOrder.ShipMovementOrderEnum.Retreat)
			{
				MatrixFrame globalFrame = item.GlobalFrame;
				Vec2 asVec = ((Vec3)(ref globalFrame.origin)).AsVec2;
				float num = item.Physics.PhysicsBoundingBoxWithChildrenSize.y / 2f + 0.5f;
				if (((Vec2)(ref asVec)).DistanceSquared(((MissionBehavior)this).Mission.GetClosestBoundaryPosition(asVec)) < num * num || !((MissionBehavior)this).Mission.IsPositionInsideBoundaries(asVec))
				{
					((List<MissionShip>)(object)_tempRetreatedShips).Add(item);
				}
			}
		}
		while (((List<MissionShip>)(object)_tempRetreatedShips).Count > 0)
		{
			MissionShip missionShip = ((List<MissionShip>)(object)_tempRetreatedShips)[((List<MissionShip>)(object)_tempRetreatedShips).Count - 1];
			((List<MissionShip>)(object)_tempRetreatedShips).RemoveAt(((List<MissionShip>)(object)_tempRetreatedShips).Count - 1);
			foreach (Agent item2 in (List<Agent>)(object)_navalAgentsLogic.GetActiveAgentsOfShip(missionShip))
			{
				if (item2.GetComponent<AgentNavalComponent>().SteppedShip != missionShip)
				{
					((List<Agent>)(object)_tempOffShipAgents).Add(item2);
				}
			}
			while (((List<Agent>)(object)_tempOffShipAgents).Count > 0)
			{
				Agent agent = ((List<Agent>)(object)_tempOffShipAgents)[((List<Agent>)(object)_tempOffShipAgents).Count - 1];
				((List<Agent>)(object)_tempOffShipAgents).RemoveAt(((List<Agent>)(object)_tempOffShipAgents).Count - 1);
				_navalAgentsLogic.RemoveAgentFromShip(agent, missionShip);
			}
			_navalAgentsLogic.FillReservedTroopsOfShip(missionShip, _tempRoutedReservedTroops);
			while (((List<IAgentOriginBase>)(object)_tempRoutedReservedTroops).Count > 0)
			{
				IAgentOriginBase obj = ((List<IAgentOriginBase>)(object)_tempRoutedReservedTroops)[((List<IAgentOriginBase>)(object)_tempRoutedReservedTroops).Count - 1];
				((List<IAgentOriginBase>)(object)_tempRoutedReservedTroops).RemoveAt(((List<IAgentOriginBase>)(object)_tempRoutedReservedTroops).Count - 1);
				obj.SetRouted(true);
			}
			_navalShipsLogic.RemoveShip(missionShip);
		}
		_checkRetreatingTimer.Reset();
	}
}
