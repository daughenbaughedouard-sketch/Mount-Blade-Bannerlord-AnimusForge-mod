using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Deployment;

public class NavalTeamDeploymentPlan
{
	public readonly Team Team;

	private readonly Mission _mission;

	private readonly NavalDeploymentPlan _plan;

	public float SpawnPathOffset { get; private set; }

	public float TargetOffset { get; private set; }

	internal NavalTeamDeploymentPlan(Mission mission, Team team)
	{
		_mission = mission;
		Team = team;
		_plan = NavalDeploymentPlan.CreatePlan(_mission, team);
		SpawnPathOffset = 0f;
		TargetOffset = 0f;
	}

	public void MakeDeploymentPlan(float spawnPathOffset, float targetOffset = 0f)
	{
		SpawnPathOffset = spawnPathOffset;
		TargetOffset = targetOffset;
		if (!_plan.IsPlanMade)
		{
			_plan.MakeDeploymentPlan(spawnPathOffset, targetOffset);
		}
	}

	public void ClearPlan()
	{
		_plan.ClearPlan();
	}

	public void ClearAddedShips()
	{
		_plan.ClearAddedShips();
	}

	internal void AddShip(FormationClass formationClass, IShipOrigin shipOrigin)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_plan.AddShip(formationClass, shipOrigin);
	}

	internal bool RemoveShip(FormationClass formationIndex)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _plan.RemoveShip(formationIndex);
	}

	public bool IsFirstPlan()
	{
		return _plan.PlanCount == 1;
	}

	public bool IsPlanMade()
	{
		return _plan.IsPlanMade;
	}

	public float GetSpawnPathOffset()
	{
		return _plan.SpawnPathOffset;
	}

	public MatrixFrame GetDeploymentFrame()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _plan.DeploymentFrame;
	}

	public MBReadOnlyList<(string, MBList<Vec2>)> GetDeploymentBoundaries()
	{
		return _plan.DeploymentBoundaries;
	}

	public bool HasDeploymentBoundaries()
	{
		return _plan.HasDeploymentBoundaries;
	}

	public IFormationDeploymentPlan GetFormationPlan(FormationClass fClass)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return (IFormationDeploymentPlan)(object)_plan.GetFormationPlan(fClass);
	}

	public bool IsPositionInsideDeploymentBoundaries(in Vec2 position)
	{
		return _plan.IsPositionInsideDeploymentBoundaries(in position);
	}

	public Vec2 GetClosestDeploymentBoundaryPosition(in Vec2 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return _plan.GetClosestBoundaryPosition(in position);
	}

	public Vec2 GetMeanBoundaryPosition(int boundaryIndex = 0)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return _plan.GetMeanBoundaryPosition(boundaryIndex);
	}
}
