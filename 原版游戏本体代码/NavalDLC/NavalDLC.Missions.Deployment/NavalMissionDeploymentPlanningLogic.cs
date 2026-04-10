using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Deployment;

public class NavalMissionDeploymentPlanningLogic : MissionDeploymentPlanningLogic
{
	private Mission _mission;

	private MBList<(Team team, NavalTeamDeploymentPlan plan)> _teamDeploymentPlans = new MBList<(Team, NavalTeamDeploymentPlan)>();

	public NavalMissionDeploymentPlanningLogic(Mission mission)
	{
		_mission = mission;
	}

	public override void Initialize()
	{
		((List<(Team, NavalTeamDeploymentPlan)>)(object)_teamDeploymentPlans).Clear();
		foreach (Team item2 in (List<Team>)(object)_mission.Teams)
		{
			NavalTeamDeploymentPlan item = new NavalTeamDeploymentPlan(_mission, item2);
			((List<(Team, NavalTeamDeploymentPlan)>)(object)_teamDeploymentPlans).Add((item2, item));
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

	public void ClearAddedShips(Team team)
	{
		GetTeamPlan(team).ClearAddedShips();
	}

	public override void ClearAll()
	{
		foreach (var item in (List<(Team, NavalTeamDeploymentPlan)>)(object)_teamDeploymentPlans)
		{
			item.Item2.ClearAddedShips();
			item.Item2.ClearPlan();
		}
	}

	public void AddShip(Team team, FormationClass formationIndex, IShipOrigin shipOrigin)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GetTeamPlan(team).AddShip(formationIndex, shipOrigin);
	}

	public bool RemoveShip(Team team, FormationClass formationIndex)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return GetTeamPlan(team).RemoveShip(formationIndex);
	}

	public override void MakeDeploymentPlan(Team team, float spawnPathOffset = 0f, float targetOffset = 0f)
	{
		NavalTeamDeploymentPlan teamPlan = GetTeamPlan(team);
		if (!((MissionDeploymentPlanningLogic)this).IsPlanMade(team))
		{
			teamPlan.MakeDeploymentPlan(spawnPathOffset, targetOffset);
			bool flag = default(bool);
			if (((MissionDeploymentPlanningLogic)this).IsPlanMade(team, ref flag))
			{
				_mission.OnDeploymentPlanMade(team, flag);
			}
		}
	}

	public override bool RemakeDeploymentPlan(Team team)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionDeploymentPlanningLogic)this).IsPlanMade(team))
		{
			float spawnPathOffset = ((MissionDeploymentPlanningLogic)this).GetSpawnPathOffset(team);
			float targetOffset = GetTargetOffset(team);
			ClearAddedShips(team);
			((MissionDeploymentPlanningLogic)this).ClearDeploymentPlan(team);
			NavalShipsLogic missionBehavior = _mission.GetMissionBehavior<NavalShipsLogic>();
			for (int i = 0; i < 11; i++)
			{
				FormationClass formationIndex = (FormationClass)i;
				ShipAssignment shipAssignment = missionBehavior.GetShipAssignment(team.TeamSide, formationIndex);
				if (shipAssignment.IsSet)
				{
					AddShip(team, formationIndex, shipAssignment.ShipOrigin);
				}
			}
			((MissionDeploymentPlanningLogic)this).MakeDeploymentPlan(team, spawnPathOffset, targetOffset);
			return ((MissionDeploymentPlanningLogic)this).IsPlanMade(team);
		}
		return false;
	}

	public override bool IsPositionInsideDeploymentBoundaries(Team team, in Vec2 position)
	{
		NavalTeamDeploymentPlan teamPlan = GetTeamPlan(team);
		if (teamPlan.HasDeploymentBoundaries())
		{
			return teamPlan.IsPositionInsideDeploymentBoundaries(in position);
		}
		Debug.FailedAssert("Cannot check if position is within deployment boundaries as requested team (index: " + team.TeamIndex + ") does not have deployment boundaries.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\Deployment\\NavalMissionDeploymentPlanningLogic.cs", "IsPositionInsideDeploymentBoundaries", 157);
		return false;
	}

	public override Vec2 GetClosestDeploymentBoundaryPosition(Team team, in Vec2 position)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		NavalTeamDeploymentPlan teamPlan = GetTeamPlan(team);
		if (teamPlan.HasDeploymentBoundaries())
		{
			return teamPlan.GetClosestDeploymentBoundaryPosition(in position);
		}
		Debug.FailedAssert("Cannot retrieve closest deployment boundary position as requested team (index: " + team.TeamIndex + ") does not have deployment boundaries.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\Deployment\\NavalMissionDeploymentPlanningLogic.cs", "GetClosestDeploymentBoundaryPosition", 170);
		return position;
	}

	public override void ProjectPositionToDeploymentBoundaries(Team team, ref WorldPosition position)
	{
		Debug.FailedAssert("Naval deployment plan does not support projection of position to deployment boundaries as it does not support a navmesh", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\Deployment\\NavalMissionDeploymentPlanningLogic.cs", "ProjectPositionToDeploymentBoundaries", 177);
	}

	public override bool GetPathDeploymentBoundaryIntersection(Team team, in WorldPosition startPosition, in WorldPosition endPosition, out WorldPosition intersection)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Debug.FailedAssert("Naval deployment plan does not support finding boundary intersection between positions as it does not support a navmesh", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\Deployment\\NavalMissionDeploymentPlanningLogic.cs", "GetPathDeploymentBoundaryIntersection", 183);
		intersection = WorldPosition.Invalid;
		return false;
	}

	public override float GetSpawnPathOffset(Team team)
	{
		return GetTeamPlan(team).GetSpawnPathOffset();
	}

	public override MatrixFrame GetZoomFocusFrame(Team team)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		NavalTeamDeploymentPlan teamPlan = GetTeamPlan(team);
		MatrixFrame deploymentFrame = teamPlan.GetDeploymentFrame();
		Vec3 val = Vec3.Zero;
		int num = 0;
		for (int i = 0; i < 11; i++)
		{
			IFormationDeploymentPlan formationPlan = teamPlan.GetFormationPlan((FormationClass)i);
			if (formationPlan.HasFrame())
			{
				MatrixFrame frame = formationPlan.GetFrame();
				val += frame.origin;
				num++;
			}
		}
		val /= (float)num;
		deploymentFrame.origin = val;
		return deploymentFrame;
	}

	public override float GetZoomOffset(Team team, float fovAngle)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		NavalTeamDeploymentPlan teamPlan = GetTeamPlan(team);
		MatrixFrame deploymentFrame = teamPlan.GetDeploymentFrame();
		float num = float.MinValue;
		for (int i = 0; i < 11; i++)
		{
			IFormationDeploymentPlan formationPlan = teamPlan.GetFormationPlan((FormationClass)i);
			if (formationPlan.HasFrame())
			{
				MatrixFrame frame = formationPlan.GetFrame();
				Vec2 asVec = ((Vec3)(ref frame.origin)).AsVec2;
				float num2 = ((Vec2)(ref asVec)).DistanceSquared(((Vec3)(ref deploymentFrame.origin)).AsVec2);
				num = MathF.Max(num, num2);
			}
		}
		return (MathF.Sqrt(num) + 20f) / MathF.Max(MathF.Tan(fovAngle / 2f), 0.01f);
	}

	public override IFormationDeploymentPlan GetFormationPlan(Team team, FormationClass fClass, bool isReinforcement = false)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!isReinforcement)
		{
			return GetTeamPlan(team).GetFormationPlan(fClass);
		}
		Debug.FailedAssert("Reinforcement plans are not supported by naval deployment plans", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\Deployment\\NavalMissionDeploymentPlanningLogic.cs", "GetFormationPlan", 265);
		return null;
	}

	public override bool IsPlanMade(Team team)
	{
		return GetTeamPlan(team)?.IsPlanMade() ?? false;
	}

	public override bool IsPlanMade(Team team, out bool isFirstPlan)
	{
		isFirstPlan = false;
		NavalTeamDeploymentPlan teamPlan = GetTeamPlan(team);
		if (teamPlan != null && teamPlan.IsPlanMade())
		{
			isFirstPlan = teamPlan.IsFirstPlan();
			return true;
		}
		return false;
	}

	public override bool HasDeploymentBoundaries(Team team)
	{
		return GetTeamPlan(team)?.HasDeploymentBoundaries() ?? false;
	}

	public override MatrixFrame GetDeploymentFrame(Team team)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionDeploymentPlanningLogic)this).IsPlanMade(team))
		{
			return GetTeamPlan(team).GetDeploymentFrame();
		}
		Debug.FailedAssert("Cannot retrieve formation deployment frame as deployment plan is not made for team " + team.TeamIndex, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\Deployment\\NavalMissionDeploymentPlanningLogic.cs", "GetDeploymentFrame", 313);
		return MatrixFrame.Identity;
	}

	public float GetTargetOffset(Team team)
	{
		return GetTeamPlan(team).TargetOffset;
	}

	public override MBReadOnlyList<(string, MBList<Vec2>)> GetDeploymentBoundaries(Team team)
	{
		if (((MissionDeploymentPlanningLogic)this).HasDeploymentBoundaries(team))
		{
			return GetTeamPlan(team).GetDeploymentBoundaries();
		}
		Debug.FailedAssert("Cannot retrieve deployment boundaries as they are not available for team " + team.TeamIndex, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Missions\\Deployment\\NavalMissionDeploymentPlanningLogic.cs", "GetDeploymentBoundaries", 333);
		return null;
	}

	public virtual bool GetMeanBoundaryPosition(Team team, out Vec2 meanPosition, int boundaryIndex = 0)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		NavalTeamDeploymentPlan teamPlan = GetTeamPlan(team);
		if (teamPlan != null && teamPlan.HasDeploymentBoundaries())
		{
			meanPosition = teamPlan.GetMeanBoundaryPosition(boundaryIndex);
			return true;
		}
		meanPosition = Vec2.Invalid;
		return false;
	}

	private NavalTeamDeploymentPlan GetTeamPlan(Team team)
	{
		return ((IEnumerable<(Team, NavalTeamDeploymentPlan)>)_teamDeploymentPlans).FirstOrDefault<(Team, NavalTeamDeploymentPlan)>(((Team team, NavalTeamDeploymentPlan plan) t) => t.team == team).Item2;
	}
}
