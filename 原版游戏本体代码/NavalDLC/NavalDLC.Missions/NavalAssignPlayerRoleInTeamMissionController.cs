using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Missions;

public class NavalAssignPlayerRoleInTeamMissionController : AssignPlayerRoleInTeamMissionController
{
	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	private ShipAgentSpawnLogic _shipAgentSpawnLogic;

	public NavalAssignPlayerRoleInTeamMissionController(bool isPlayerGeneral, bool isPlayerSergeant, bool isPlayerInArmy, List<string> charactersInPlayerSideByPriority = null)
		: base(isPlayerGeneral, isPlayerSergeant, isPlayerInArmy, charactersInPlayerSideByPriority)
	{
	}

	public override void OnPlayerChoiceMade(int chosenIndex)
	{
		Debug.FailedAssert("Player cannot make a choice in naval battles as its decision is fixed by design", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\NavalAssignPlayerRoleInTeamMissionController.cs", "OnPlayerChoiceMade", 25);
	}

	public override void OnPlayerTeamDeployed()
	{
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_shipAgentSpawnLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<ShipAgentSpawnLogic>();
		((AssignPlayerRoleInTeamMissionController)this).PlayerChosenIndex = 0;
		if (!MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle())
		{
			return;
		}
		Team playerTeam = Mission.Current.PlayerTeam;
		base.FormationsLockedWithSergeants = new Dictionary<int, Agent>();
		base.FormationsWithLooselyChosenSergeants = new Dictionary<int, Agent>();
		if (playerTeam.IsPlayerGeneral)
		{
			base.CharacterNamesInPlayerSideByPriorityQueue = new Queue<string>();
			base.RemainingFormationsToAssignSergeantsTo = new List<Formation>();
			return;
		}
		base.CharacterNamesInPlayerSideByPriorityQueue = ((base.CharactersInPlayerSideByPriority != null) ? new Queue<string>(base.CharactersInPlayerSideByPriority) : new Queue<string>());
		base.RemainingFormationsToAssignSergeantsTo = LinQuick.WhereQ<Formation>((List<Formation>)(object)playerTeam.FormationsIncludingSpecialAndEmpty, (Func<Formation, bool>)((Formation f) => f.CountOfUnits > 0)).ToList();
		while (base.CharacterNamesInPlayerSideByPriorityQueue.Count > 0 && base.RemainingFormationsToAssignSergeantsTo.Count > 0)
		{
			string nextAgentNameToProcess = base.CharacterNamesInPlayerSideByPriorityQueue.Dequeue();
			Agent val = ((IEnumerable<Agent>)playerTeam.ActiveAgents).FirstOrDefault((Func<Agent, bool>)((Agent aa) => ((MBObjectBase)aa.Character).StringId.Equals(nextAgentNameToProcess)));
			if (val != null)
			{
				Formation val2 = base.RemainingFormationsToAssignSergeantsTo[0];
				base.FormationsLockedWithSergeants.Add(val2.Index, val);
				base.RemainingFormationsToAssignSergeantsTo.Remove(val2);
			}
		}
	}

	protected override void AssignSergeant(Formation formationToLead, Agent sergeant)
	{
		_navalShipsLogic.GetShip(formationToLead, out var ship);
		if (formationToLead.Captain != sergeant)
		{
			_navalAgentsLogic.AssignCaptainToShipForDeploymentMode(sergeant, ship);
		}
		if (!sergeant.IsAIControlled || sergeant == Agent.Main)
		{
			formationToLead.PlayerOwner = sergeant;
		}
	}
}
