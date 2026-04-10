using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.Deployment;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Storyline.MissionControllers;

public class Quest5NavalMissionDeploymentPlanningLogic : NavalMissionDeploymentPlanningLogic
{
	private Mission _mission;

	private List<(Team team, DefaultDeploymentPlan plan)> _teamDeploymentPlans = new List<(Team, DefaultDeploymentPlan)>();

	public Quest5NavalMissionDeploymentPlanningLogic(Mission mission)
		: base(mission)
	{
		_mission = mission;
	}

	public override void Initialize()
	{
		_teamDeploymentPlans.Clear();
		foreach (Team item2 in (List<Team>)(object)_mission.Teams)
		{
			DefaultDeploymentPlan item = DefaultDeploymentPlan.CreateInitialPlan(_mission, item2);
			_teamDeploymentPlans.Add((item2, item));
		}
	}

	public override void ClearDeploymentPlan(Team team)
	{
		GetTeamPlan(team).ClearPlan();
	}

	public override bool SupportsReinforcements()
	{
		return false;
	}

	public override bool SupportsNavmesh()
	{
		return false;
	}

	public override bool HasPlayerSpawnFrame(BattleSideEnum battleSide)
	{
		return false;
	}

	public override bool GetPlayerSpawnFrame(BattleSideEnum battleSide, out WorldPosition position, out Vec2 direction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		position = WorldPosition.Invalid;
		direction = Vec2.Invalid;
		return false;
	}

	public new void ClearAddedShips(Team team)
	{
	}

	public override void ClearAll()
	{
		foreach (var teamDeploymentPlan in _teamDeploymentPlans)
		{
			teamDeploymentPlan.plan.ClearPlan();
		}
	}

	public new void AddShip(Team team, FormationClass formationIndex, IShipOrigin shipOrigin)
	{
	}

	public new bool RemoveShip(Team team, FormationClass formationIndex)
	{
		return true;
	}

	public override void MakeDeploymentPlan(Team team, float spawnPathOffset = 0f, float targetOffset = 0f)
	{
	}

	public override bool RemakeDeploymentPlan(Team team)
	{
		return true;
	}

	public override bool IsPositionInsideDeploymentBoundaries(Team team, in Vec2 position)
	{
		return true;
	}

	public override Vec2 GetClosestDeploymentBoundaryPosition(Team team, in Vec2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return position;
	}

	public override void ProjectPositionToDeploymentBoundaries(Team team, ref WorldPosition position)
	{
	}

	public override bool GetPathDeploymentBoundaryIntersection(Team team, in WorldPosition startPosition, in WorldPosition endPosition, out WorldPosition intersection)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		intersection = WorldPosition.Invalid;
		return true;
	}

	public override float GetSpawnPathOffset(Team team)
	{
		return 1f;
	}

	public override MatrixFrame GetZoomFocusFrame(Team team)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return MatrixFrame.Identity;
	}

	public override float GetZoomOffset(Team team, float fovAngle)
	{
		return 1f;
	}

	public override IFormationDeploymentPlan GetFormationPlan(Team team, FormationClass fClass, bool isReinforcement = false)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!isReinforcement)
		{
			return (IFormationDeploymentPlan)(object)GetTeamPlan(team).GetFormationPlan(fClass);
		}
		Debug.FailedAssert("Reinforcement plans are not supported by naval deployment plans", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\MissionControllers\\Quest5NavalMissionDeploymentPlanningLogic.cs", "GetFormationPlan", 149);
		return null;
	}

	public override bool IsPlanMade(Team team)
	{
		return GetTeamPlan(team) != null;
	}

	public override bool IsPlanMade(Team team, out bool isFirstPlan)
	{
		isFirstPlan = false;
		if (GetTeamPlan(team) != null)
		{
			isFirstPlan = true;
			return true;
		}
		return false;
	}

	public override bool HasDeploymentBoundaries(Team team)
	{
		return false;
	}

	public override MatrixFrame GetDeploymentFrame(Team team)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return MatrixFrame.Identity;
	}

	public new float GetTargetOffset(Team team)
	{
		return GetTeamPlan(team).TargetOffset;
	}

	public override MBReadOnlyList<(string, MBList<Vec2>)> GetDeploymentBoundaries(Team team)
	{
		return null;
	}

	public override bool GetMeanBoundaryPosition(Team team, out Vec2 meanPosition, int boundaryIndex = 0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		meanPosition = Vec2.Invalid;
		return true;
	}

	private DefaultDeploymentPlan GetTeamPlan(Team team)
	{
		DefaultDeploymentPlan val = _teamDeploymentPlans.FirstOrDefault(((Team team, DefaultDeploymentPlan plan) t) => t.team == team).plan;
		if (val == null)
		{
			val = DefaultDeploymentPlan.CreateInitialPlan(_mission, team);
			_teamDeploymentPlans.Add((team, val));
		}
		return val;
	}
}
