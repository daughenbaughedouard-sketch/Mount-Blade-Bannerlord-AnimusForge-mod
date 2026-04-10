using System.Collections.Generic;
using NavalDLC.Missions.AI.Behaviors;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.TeamAI;

public class TeamAINavalComponent : TeamAIComponent
{
	private readonly bool _isRiverBattle;

	private NavalShipsLogic _navalShipsLogic;

	private SpawnPathData _spawnPathData;

	public NavalQuerySystem TeamNavalQuerySystem { get; protected set; }

	public bool UseSpawnPathApproachPosition
	{
		get
		{
			if (_isRiverBattle && _spawnPathData != null)
			{
				return _spawnPathData.IsValid;
			}
			return false;
		}
	}

	public TeamAINavalComponent(Mission currentMission, Team currentTeam, float thinkTimerTime, float applyTimerTime)
		: base(currentMission, currentTeam, thinkTimerTime, applyTimerTime)
	{
		TeamNavalQuerySystem = new NavalQuerySystem(currentTeam);
		NavalOrderController navalOrderController = new NavalOrderController(base.Mission, base.Team, null);
		NavalOrderController navalOrderController2 = new NavalOrderController(base.Mission, base.Team, (base.Team.IsPlayerTeam && base.Team.IsPlayerGeneral) ? Mission.Current.MainAgent : null);
		base.Team.SetCustomOrderController((OrderController)(object)navalOrderController, (OrderController)(object)navalOrderController2);
		base.Team.DisableDetachmentTicking();
		_isRiverBattle = Mission.Current.Scene.GetNavmeshFaceCountBetweenTwoIds(1, 1) > 0;
	}

	public override void OnUnitAddedToFormationForTheFirstTime(Formation formation)
	{
		if (formation.AI.GetBehavior<BehaviorNavalRemoveConnection>() == null)
		{
			formation.ForceCalculateCaches();
			formation.AI.AddAiBehavior((BehaviorComponent)(object)new BehaviorNavalRemoveConnection(formation));
			formation.AI.AddAiBehavior((BehaviorComponent)(object)new BehaviorNavalEngageCorrespondingEnemy(formation));
			formation.AI.AddAiBehavior((BehaviorComponent)(object)new BehaviorNavalDefendInLine(formation));
			formation.AI.AddAiBehavior((BehaviorComponent)(object)new BehaviorNavalSkirmish(formation));
			formation.AI.AddAiBehavior((BehaviorComponent)(object)new BehaviorNavalRamming(formation));
			formation.AI.AddAiBehavior((BehaviorComponent)(object)new BehaviorNavalApproachInLine(formation));
		}
	}

	public override void OnDeploymentFinished()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		foreach (Formation item in (List<Formation>)(object)base.Team.FormationsIncludingEmpty)
		{
			item.OnDeploymentFinished();
		}
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		if (Mission.Current.IsBattleSpawnPathSelectorInitialized)
		{
			_spawnPathData = Mission.Current.GetInitialSpawnPathData(base.Team.Side);
		}
	}

	public Formation GetConnectedAllyFormation(ulong shipUniqueBitwiseID)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return _navalShipsLogic.GetConnectedTeamShip(base.Team.TeamSide, shipUniqueBitwiseID)?.Formation;
	}

	public Formation GetNearestAllyShipFormation(Agent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = agent.Frame.origin;
		return _navalShipsLogic.GetNearestTeamShip(base.Team.TeamSide, in position, float.MaxValue, (MissionShip ship) => ship.Physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Floating && ship.FireHitPoints > 0f)?.Formation;
	}

	public void GetRiverApproachPosition(out Vec2 position, out Vec2 direction)
	{
		_spawnPathData.GetSpawnPathFrameFacingTarget(0f, 1f, false, ref position, ref direction, false, 0.2f);
	}
}
