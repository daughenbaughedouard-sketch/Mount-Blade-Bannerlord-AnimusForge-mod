using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

internal class ShipAgentSpawnLogicTeamSide
{
	private readonly ShipAgentSpawnLogic _agentSpawnLogic;

	private readonly NavalShipsLogic _shipsLogic;

	private readonly NavalAgentsLogic _agentsLogic;

	private readonly MBQueue<(Formation formation, IAgentOriginBase captainOrigin)> _pendingCaptainAssignments;

	private bool _updateShipsOnNextTick;

	private bool _troopSpawningActive;

	public BattleSideEnum BattleSide { get; private set; }

	public TeamSideEnum TeamSide { get; }

	public bool TroopSpawningActive
	{
		get
		{
			return _troopSpawningActive;
		}
		private set
		{
			_troopSpawningActive = value;
			if (_agentsLogic.IsDeploymentFinished)
			{
				_agentsLogic.SetSpawnReinforcementsOnTick(_troopSpawningActive);
			}
		}
	}

	public ShipAgentSpawnLogicTeamSide(ShipAgentSpawnLogic spawnLogic, BattleSideEnum battleSide, TeamSideEnum teamSide, MBList<IAgentOriginBase> troopOrigins)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		_agentSpawnLogic = spawnLogic;
		BattleSide = battleSide;
		TeamSide = teamSide;
		_agentsLogic = ((MissionBehavior)_agentSpawnLogic).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_shipsLogic = ((MissionBehavior)_agentSpawnLogic).Mission.GetMissionBehavior<NavalShipsLogic>();
		_agentsLogic.SetSpawnReinforcementsOnTick(teamSide, TroopSpawningActive);
		_agentsLogic.AddTroopOrigins(teamSide, troopOrigins);
		_agentsLogic.SetRestrictRecentlySwappedAgentTransfers(teamSide, value: true);
		_pendingCaptainAssignments = new MBQueue<(Formation, IAgentOriginBase)>();
	}

	public void OnDeploymentFinished()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_agentsLogic.SetRestrictRecentlySwappedAgentTransfers(TeamSide, value: false);
		_agentsLogic.SetSpawnReinforcementsOnTick(TroopSpawningActive);
	}

	public void OnDeploymentTick(float dt, Mission mission)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		_agentsLogic.ClearRecentlySwappedAgentsData(TeamSide);
		if (_updateShipsOnNextTick)
		{
			_updateShipsOnNextTick = false;
			_agentsLogic.AssignTroops(TeamSide);
			_agentsLogic.InitializeReinforcementTimers(TeamSide);
			ReassignPendingCaptains();
			CheckSpawnNextBatch();
			_agentsLogic.AssignAndTeleportCrewToShipMachines(TeamSide);
			if ((int)TeamSide == 0)
			{
				_agentSpawnLogic.OnPlayerShipsUpdated();
			}
		}
	}

	public void AllocateAndDeployInitialTroops(Mission mission)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_agentsLogic.AutoComputeDesiredTroopCountsPerShip(TeamSide);
		if ((int)TeamSide == 0)
		{
			AllocateAndDeployInitialTroopsOfPlayerTeam();
		}
		else
		{
			AllocateAndDeployInitialTroopsOfTeam();
		}
		_agentsLogic.AssignAndTeleportCrewToShipMachines(TeamSide);
	}

	public void UpdateShips()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		_agentsLogic.AutoComputeDesiredTroopCountsPerShip(TeamSide);
		_agentsLogic.UnassignTroops(TeamSide);
		_updateShipsOnNextTick = true;
	}

	public void SetSpawnTroops(bool spawnTroops, bool enforceSpawn = false)
	{
		TroopSpawningActive = spawnTroops;
		if (enforceSpawn)
		{
			CheckSpawnNextBatch();
		}
	}

	public bool HasPendingCaptainAssignment(Formation formation)
	{
		return ((IEnumerable<(Formation, IAgentOriginBase)>)_pendingCaptainAssignments).Any<(Formation, IAgentOriginBase)>(((Formation formation, IAgentOriginBase captainOrigin) pca) => pca.formation == formation);
	}

	private void ReassignPendingCaptains()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		while (!Extensions.IsEmpty<(Formation, IAgentOriginBase)>((IEnumerable<(Formation, IAgentOriginBase)>)_pendingCaptainAssignments))
		{
			(Formation, IAgentOriginBase) tuple = ((Queue<(Formation, IAgentOriginBase)>)(object)_pendingCaptainAssignments).Dequeue();
			IAgentOriginBase item = tuple.Item2;
			var (formation, _) = tuple;
			if (_shipsLogic.GetShip(formation, out var ship))
			{
				if (!_agentsLogic.IsAgentOnAnyShip(item, out var foundAgent, out var onShip, TeamSide))
				{
					_agentsLogic.SpawnExistingHero(item, ship, out foundAgent);
					onShip = ship;
				}
				_agentsLogic.AssignCaptainToShipForDeploymentMode(foundAgent, ship, onShip);
			}
		}
	}

	public void OnBeforeShipRemoved(MissionShip ship)
	{
		if (!_shipsLogic.IsMissionEnding && _agentSpawnLogic.ReassignCaptainsOfRemovedShips && ship.Captain != null)
		{
			((Queue<(Formation, IAgentOriginBase)>)(object)_pendingCaptainAssignments).Enqueue((ship.Formation, ship.Captain.Origin));
		}
	}

	private void AllocateAndDeployInitialTroopsOfPlayerTeam()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		IAgentOriginBase troopOrigin = _agentsLogic.FindTroopOrigin(TeamSide, (IAgentOriginBase origin) => origin.Troop.IsPlayerCharacter);
		MissionShip missionShip = _shipsLogic.GetShipAssignment((TeamSideEnum)0, (FormationClass)0).MissionShip;
		_agentsLogic.AddReservedTroopToShip(troopOrigin, missionShip);
		_agentsLogic.AssignTroops(TeamSide);
		_agentsLogic.InitializeReinforcementTimers(TeamSide);
		CheckSpawnNextBatch();
		Agent val = ((IEnumerable<Agent>)_agentsLogic.GetActiveHeroesOfShip(missionShip)).FirstOrDefault((Func<Agent, bool>)((Agent agent) => agent.IsPlayerTroop));
		if (missionShip.Captain != val)
		{
			_agentsLogic.AssignCaptainToShipForDeploymentMode(val, missionShip, missionShip);
		}
	}

	private void AllocateAndDeployInitialTroopsOfTeam()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		_agentsLogic.AssignTroops(TeamSide);
		_agentsLogic.InitializeReinforcementTimers(TeamSide);
		CheckSpawnNextBatch();
	}

	private int CheckSpawnNextBatch()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if (TroopSpawningActive)
		{
			num += _agentsLogic.SpawnNextBatch(TeamSide);
		}
		return num;
	}
}
