using System;
using System.Collections.Generic;
using NavalDLC.Missions.Handlers;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.ShipControl;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Deployment;

public class NavalDeploymentMissionController : DeploymentMissionController
{
	private ShipAgentSpawnLogic _shipAgentSpawnLogic;

	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	private DefaultNavalMissionLogic _navalMissionLogic;

	private NavalDeploymentHandler _navalDeploymentHandler;

	public event Action PlayerShipsUpdated;

	public NavalDeploymentMissionController(bool isPlayerAttacker)
		: base(isPlayerAttacker)
	{
	}

	public override void OnBehaviorInitialize()
	{
		((DeploymentMissionController)this).OnBehaviorInitialize();
		_navalMissionLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<DefaultNavalMissionLogic>();
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_shipAgentSpawnLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<ShipAgentSpawnLogic>();
		_shipAgentSpawnLogic.PlayerShipsUpdated += OnPlayerShipsUpdated;
		_navalDeploymentHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalDeploymentHandler>();
	}

	protected override void OnAfterStart()
	{
		for (int i = 0; i < 2; i++)
		{
			_shipAgentSpawnLogic.SetSpawnTroops((BattleSideEnum)i, spawnTroops: false);
		}
	}

	public override void OnMissionStateFinalized()
	{
		_shipAgentSpawnLogic.PlayerShipsUpdated -= OnPlayerShipsUpdated;
	}

	public bool TryAssignShipToFormation(IShipOrigin shipOrigin, Formation formation, bool updateShips = true)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		ShipAssignment shipAssignment = null;
		bool flag = shipOrigin != null && _navalShipsLogic.FindAssignmentOfShipOrigin(shipOrigin, out shipAssignment);
		if (flag && shipAssignment.Formation == formation)
		{
			return false;
		}
		bool flag2 = _navalShipsLogic.IsAShipAssignedToFormation(formation);
		if (shipOrigin == null && !flag2)
		{
			return false;
		}
		if (flag2)
		{
			_navalShipsLogic.RemoveShip(formation);
		}
		if (shipOrigin != null)
		{
			if (flag)
			{
				_navalShipsLogic.TransferShipToFormation(shipOrigin, shipAssignment.Formation, formation);
			}
			else
			{
				_navalShipsLogic.SpawnShip(shipOrigin, MatrixFrame.Zero, formation.Team, formation, spawnAnchored: true, (FormationClass)8).SetController(ShipControllerType.None);
			}
		}
		if (updateShips)
		{
			UpdateShips(formation.Team.TeamSide);
		}
		return true;
	}

	public void UpdateShips(TeamSideEnum teamSide)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_shipAgentSpawnLogic.UpdateShips(teamSide);
	}

	public bool IsShipAssignedToFormation(Formation formation)
	{
		return _navalShipsLogic.IsAShipAssignedToFormation(formation);
	}

	public bool TryAssignCaptainToFormation(IAgentOriginBase captainOrigin, Formation formation)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		_navalShipsLogic.GetShip(formation, out var ship);
		if (captainOrigin != null)
		{
			Agent foundAgent;
			MissionShip onShip;
			bool flag = _navalAgentsLogic.IsAgentOnAnyShip(captainOrigin, out foundAgent, out onShip, formation.Team.TeamSide);
			if (flag && formation.Captain == foundAgent)
			{
				return false;
			}
			if (!flag)
			{
				_navalAgentsLogic.SpawnExistingHero(captainOrigin, ship, out foundAgent);
			}
			_navalAgentsLogic.AssignCaptainToShipForDeploymentMode(foundAgent, ship, onShip);
			return true;
		}
		if (formation.Captain == null)
		{
			return false;
		}
		_navalAgentsLogic.UnassignCaptainOfShipForDeploymentMode(ship);
		return true;
	}

	public bool SetTroopClassFilter(TroopTraitsMask troopClassFilter, Formation targetFormation, bool updateShips)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		_navalShipsLogic.GetShip(targetFormation, out var ship);
		_navalAgentsLogic.SetTroopClassFilter(ship, troopClassFilter);
		if (updateShips)
		{
			UpdateShips(targetFormation.Team.TeamSide);
		}
		return updateShips;
	}

	public bool SetTroopTraitsFilter(TroopTraitsMask troopTraitsFilter, Formation targetFormation, bool updateShips)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		_navalShipsLogic.GetShip(targetFormation, out var ship);
		_navalAgentsLogic.SetTroopTraitsFilter(ship, troopTraitsFilter);
		if (updateShips)
		{
			UpdateShips(targetFormation.Team.TeamSide);
		}
		return updateShips;
	}

	public IReadOnlyCollection<IAgentOriginBase> GetAllPlayerTeamHeroes()
	{
		return _navalAgentsLogic.GetTeamHeroOrigins((TeamSideEnum)0);
	}

	public MBReadOnlyList<IShipOrigin> GetAllPlayerShips()
	{
		return _navalMissionLogic.PlayerShips;
	}

	public MBReadOnlyList<Formation> GetUsableFormations()
	{
		return (MBReadOnlyList<Formation>)(object)((MissionBehavior)this).Mission.PlayerTeam.FormationsIncludingEmpty;
	}

	protected override void OnSetupTeamsOfSide(BattleSideEnum battleSide)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		_navalMissionLogic.DeployBattleSide(battleSide);
		_shipAgentSpawnLogic.AllocateAndDeployInitialTroops(battleSide);
		((DeploymentMissionController)this).SetupAgentAIStatesForSide(battleSide);
		_shipAgentSpawnLogic.OnSideDeploymentOver(battleSide);
	}

	protected override void OnSetupTeamsFinished()
	{
		_navalShipsLogic.SetTeleportShips(value: true);
	}

	protected override void SetupAIOfEnemySide(BattleSideEnum enemySide)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		Team val = (((int)enemySide == 1) ? ((MissionBehavior)this).Mission.AttackerTeam : ((MissionBehavior)this).Mission.DefenderTeam);
		((DeploymentMissionController)this).SetupAIOfEnemyTeam(val);
		Team val2 = (((int)enemySide == 1) ? ((MissionBehavior)this).Mission.AttackerAllyTeam : ((MissionBehavior)this).Mission.DefenderAllyTeam);
		if (val2 != null)
		{
			((DeploymentMissionController)this).SetupAIOfEnemyTeam(val2);
		}
	}

	protected override void SetupAIOfEnemyTeam(Team team)
	{
		foreach (Formation item in (List<Formation>)(object)team.FormationsIncludingEmpty)
		{
			if (item.CountOfUnits > 0)
			{
				item.SetControlledByAI(true, false);
			}
		}
		team.QuerySystem.Expire();
		((MissionBehavior)this).Mission.AllowAiTicking = true;
		((MissionBehavior)this).Mission.ForceTickOccasionally = true;
		team.ResetTactic();
		((MissionBehavior)this).Mission.AllowAiTicking = false;
		((MissionBehavior)this).Mission.ForceTickOccasionally = false;
	}

	protected override void BeforeDeploymentFinished()
	{
		_navalShipsLogic.SetTeleportShips(value: false);
	}

	protected override void AfterDeploymentFinished()
	{
		((MissionBehavior)this).Mission.RemoveMissionBehavior((MissionBehavior)(object)_navalDeploymentHandler);
	}

	internal void OnPlayerShipsUpdated()
	{
		this.PlayerShipsUpdated?.Invoke();
	}
}
