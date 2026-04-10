using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.Deployment;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipControl;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

public class DefaultNavalMissionLogic : MissionLogic, IAgentStateDecider, IMissionBehavior
{
	private const float InterTeamDeploymentGap = 32f;

	private NavalShipsLogic _shipsLogic;

	private NavalMissionDeploymentPlanningLogic _deploymentPlan;

	private readonly MBList<IShipOrigin> _playerTeamShips;

	private readonly MBList<IShipOrigin> _playerAllyTeamShips;

	private readonly MBList<IShipOrigin> _enemyTeamShips;

	private readonly NavalShipDeploymentLimit _playerTeamShipDeploymentLimit;

	private readonly NavalShipDeploymentLimit _playerAllyTeamShipDeploymentLimit;

	private readonly NavalShipDeploymentLimit _enemyTeamShipDeploymentLimit;

	public MBReadOnlyList<IShipOrigin> PlayerShips => (MBReadOnlyList<IShipOrigin>)(object)_playerTeamShips;

	public MBReadOnlyList<IShipOrigin> PlayerAllyShips => (MBReadOnlyList<IShipOrigin>)(object)_playerAllyTeamShips;

	public MBReadOnlyList<IShipOrigin> PlayerEnemyShips => (MBReadOnlyList<IShipOrigin>)(object)_enemyTeamShips;

	protected override void OnEndMission()
	{
		((MissionBehavior)this).OnEndMission();
		Mission.Current.OnMissileRemovedEvent -= ((MissionBehavior)this).OnMissileRemoved;
	}

	public override void OnMissionStateFinalized()
	{
		SailWindProfile.FinalizeProfile();
	}

	public override void OnDeploymentFinished()
	{
		foreach (MissionShip item in (List<MissionShip>)(object)_shipsLogic.AllShips)
		{
			item.SetAnchor(isAnchored: false);
			if (!item.IsPlayerShip)
			{
				item.SetController(ShipControllerType.AI);
			}
		}
		_shipsLogic.SetDeploymentMode(value: false);
	}

	internal void DeployBattleSide(BattleSideEnum battleSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		MakeDeploymentPlansForSide(battleSide);
		foreach (Team item in Mission.GetTeamsOfSide(battleSide))
		{
			foreach (Formation item2 in (List<Formation>)(object)item.FormationsIncludingEmpty)
			{
				FormationClass formationIndex = item2.FormationIndex;
				IFormationDeploymentPlan formationPlan = ((MissionDeploymentPlanningLogic)_deploymentPlan).GetFormationPlan(item, formationIndex, false);
				if (formationPlan.HasFrame())
				{
					MatrixFrame spawnFrame = formationPlan.GetFrame();
					_shipsLogic.SpawnShip(item2, in spawnFrame).SetController(ShipControllerType.None);
				}
			}
		}
	}

	public DefaultNavalMissionLogic(MBList<IShipOrigin> playerShips, MBList<IShipOrigin> playerAllyShips, MBList<IShipOrigin> enemyShips, NavalShipDeploymentLimit playerTeamShipDeploymentLimit, NavalShipDeploymentLimit playerAllyTeamShipDeploymentLimit, NavalShipDeploymentLimit enemyTeamShipDeploymentLimit)
	{
		_playerTeamShips = playerShips;
		_playerAllyTeamShips = playerAllyShips;
		_enemyTeamShips = enemyShips;
		_playerTeamShipDeploymentLimit = playerTeamShipDeploymentLimit;
		_playerAllyTeamShipDeploymentLimit = playerAllyTeamShipDeploymentLimit;
		_enemyTeamShipDeploymentLimit = enemyTeamShipDeploymentLimit;
	}

	public override void AfterStart()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		((MissionBehavior)this).AfterStart();
		_deploymentPlan = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalMissionDeploymentPlanningLogic>();
		UpdateSceneWindDirection();
		if ((int)((MissionBehavior)this).Mission.TerrainType != 11)
		{
			UpdateSceneWaterStrength();
		}
		InitializeShipAssignments();
	}

	public override void OnBehaviorInitialize()
	{
		if (!SailWindProfile.IsSailWindProfileInitialized)
		{
			SailWindProfile.InitializeProfile();
		}
		_shipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_shipsLogic.SetDeploymentMode(value: true);
		_shipsLogic.SetTeamShipDeploymentLimit((TeamSideEnum)0, _playerTeamShipDeploymentLimit);
		_shipsLogic.SetTeamShipDeploymentLimit((TeamSideEnum)1, _playerAllyTeamShipDeploymentLimit);
		_shipsLogic.SetTeamShipDeploymentLimit((TeamSideEnum)2, _enemyTeamShipDeploymentLimit);
		MissionGameModels.Current.BattleInitializationModel.InitializeModel();
	}

	private void InitializeShipAssignments()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		NavalAgentsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_shipsLogic.ClearShipAssignments();
		if (((List<IShipOrigin>)(object)_playerTeamShips).Count > 0)
		{
			int num = MathF.Min(_playerTeamShipDeploymentLimit.NetDeploymentLimit, ((List<IShipOrigin>)(object)_playerTeamShips).Count);
			num = MathF.Min(missionBehavior.GetTeamTroopOrigins((TeamSideEnum)0).Count(), num);
			foreach (var item in AssignShipsToFormations((MBReadOnlyList<IShipOrigin>)(object)_playerTeamShips, num))
			{
				_shipsLogic.SetShipAssignment((TeamSideEnum)0, item.formationIndex, item.ship);
			}
		}
		if (_playerAllyTeamShips != null && ((List<IShipOrigin>)(object)_playerAllyTeamShips).Count > 0)
		{
			int num2 = MathF.Min(_playerAllyTeamShipDeploymentLimit.NetDeploymentLimit, ((List<IShipOrigin>)(object)_playerAllyTeamShips).Count);
			num2 = MathF.Min(missionBehavior.GetTeamTroopOrigins((TeamSideEnum)1).Count(), num2);
			foreach (var item2 in AssignShipsToFormations((MBReadOnlyList<IShipOrigin>)(object)_playerAllyTeamShips, num2))
			{
				_shipsLogic.SetShipAssignment((TeamSideEnum)1, item2.formationIndex, item2.ship);
			}
		}
		if (((List<IShipOrigin>)(object)_enemyTeamShips).Count <= 0)
		{
			return;
		}
		int num3 = MathF.Min(_enemyTeamShipDeploymentLimit.NetDeploymentLimit, ((List<IShipOrigin>)(object)_enemyTeamShips).Count);
		num3 = MathF.Min(missionBehavior.GetTeamTroopOrigins((TeamSideEnum)2).Count(), num3);
		foreach (var item3 in AssignShipsToFormations((MBReadOnlyList<IShipOrigin>)(object)_enemyTeamShips, num3))
		{
			_shipsLogic.SetShipAssignment((TeamSideEnum)2, item3.formationIndex, item3.ship);
		}
	}

	private float GetTeamSpawnPathOffsetRange(Path initialSpawnPath, Team team)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		_ = team.TeamSide;
		for (int i = 0; i < 11; i++)
		{
			ShipAssignment shipAssignment = _shipsLogic.GetShipAssignment(team.TeamSide, (FormationClass)i);
			if (shipAssignment.IsSet)
			{
				num = Math.Max(shipAssignment.MissionShipObject.DeploymentArea.y, num);
			}
		}
		return 1.1f * num;
	}

	private List<(FormationClass formationIndex, IShipOrigin ship)> AssignShipsToFormations(MBReadOnlyList<IShipOrigin> ships, int shipCount)
	{
		List<(FormationClass, IShipOrigin)> list = new List<(FormationClass, IShipOrigin)>();
		int num = 8;
		int num2 = 0;
		foreach (IShipOrigin item in (List<IShipOrigin>)(object)ships)
		{
			if (num2 < num && num2 < shipCount)
			{
				list.Add(((FormationClass)num2, item));
				num2++;
				continue;
			}
			break;
		}
		return list;
	}

	private void MakeDeploymentPlansForSide(BattleSideEnum battleSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		MBReadOnlyList<(Team, int)> val = CollectSortedBattleSideTeamsData(battleSide);
		SpawnPathData initialSpawnPathData = Mission.Current.GetInitialSpawnPathData(battleSide);
		Path path = initialSpawnPathData.Path;
		float[] array = new float[((List<(Team, int)>)(object)val).Count];
		for (int i = 0; i < ((List<(Team, int)>)(object)val).Count; i++)
		{
			Team item = ((List<(Team, int)>)(object)val)[i].Item1;
			AddTeamShipsToDeploymentPlan(item);
			array[i] = GetTeamSpawnPathOffsetRange(path, item);
		}
		float num = _shipsLogic.ComputeSpawnPathDeploymentOffset(path);
		float num2 = default(float);
		float num3 = default(float);
		MissionAgentSpawnLogic.ComputeDeploymentBaseOffsets(initialSpawnPathData, num, ref num2, ref num3);
		float[] array2 = default(float[]);
		MissionAgentSpawnLogic.ComputeTeamDeploymentOffsets(initialSpawnPathData, num2, 32f, array, ref array2);
		for (int j = 0; j < ((List<(Team, int)>)(object)val).Count; j++)
		{
			((MissionDeploymentPlanningLogic)_deploymentPlan).MakeDeploymentPlan(((List<(Team, int)>)(object)val)[j].Item1, array2[j], num3);
		}
	}

	private void UpdateSceneWindDirection()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vec2 globalWindVelocity = Mission.Current.Scene.GetGlobalWindVelocity();
		if (((Vec2)(ref globalWindVelocity)).IsNonZero())
		{
			float northRotation = Mission.Current.Scene.GetNorthRotation();
			((Vec2)(ref globalWindVelocity)).RotateCCW(northRotation);
			Mission.Current.Scene.SetGlobalWindVelocity(ref globalWindVelocity);
		}
	}

	private void UpdateSceneWaterStrength()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vec2 globalWindVelocity = Mission.Current.Scene.GetGlobalWindVelocity();
		float length = ((Vec2)(ref globalWindVelocity)).Length;
		float num = 30f;
		float num2 = 10f;
		Mission.Current.Scene.SetWaterStrength(length * num2 / num);
	}

	private void AddTeamShipsToDeploymentPlan(Team team)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 11; i++)
		{
			ShipAssignment shipAssignment = _shipsLogic.GetShipAssignment(team.TeamSide, (FormationClass)i);
			if (shipAssignment.IsSet)
			{
				_deploymentPlan.AddShip(team, shipAssignment.FormationIndex, shipAssignment.ShipOrigin);
			}
		}
	}

	public AgentState GetAgentState(Agent affectedAgent, float deathProbability, out bool usedSurgery)
	{
		if (affectedAgent.IsInWater())
		{
			usedSurgery = true;
			if (affectedAgent.Character != null && affectedAgent.Character.IsHero)
			{
				return (AgentState)3;
			}
			return (AgentState)4;
		}
		usedSurgery = false;
		return (AgentState)0;
	}

	private MBReadOnlyList<(Team team, int shipCount)> CollectSortedBattleSideTeamsData(BattleSideEnum battleSide)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		MBList<(Team, int)> val = new MBList<(Team, int)>();
		foreach (Team item in (List<Team>)(object)((MissionBehavior)this).Mission.Teams)
		{
			if (item.Side == battleSide)
			{
				int countOfSetShipAssignments = _shipsLogic.GetCountOfSetShipAssignments(item.TeamSide);
				if (countOfSetShipAssignments > 0)
				{
					((List<(Team, int)>)(object)val).Add((item, countOfSetShipAssignments));
				}
			}
		}
		((List<(Team, int)>)(object)val).Sort((Comparison<(Team, int)>)delegate((Team team, int shipCount) t1, (Team team, int shipCount) t2)
		{
			bool flag = t1.team == ((MissionBehavior)this).Mission.PlayerTeam || t1.team == ((MissionBehavior)this).Mission.PlayerEnemyTeam;
			bool flag2 = t2.team == ((MissionBehavior)this).Mission.PlayerTeam || t2.team == ((MissionBehavior)this).Mission.PlayerEnemyTeam;
			if (!flag && !flag2)
			{
				if (t1.shipCount > t2.shipCount)
				{
					return -1;
				}
				if (t1.shipCount < t2.shipCount)
				{
					return 1;
				}
				return 0;
			}
			return flag ? 1 : (-1);
		});
		return (MBReadOnlyList<(Team team, int shipCount)>)(object)val;
	}
}
