using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using NavalDLC.Storyline.MissionControllers;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest3;

internal class ReachEscapeZoneObjective : MissionObjective
{
	private BlockedEstuaryMissionController _controller;

	private List<CheckpointObjectiveTarget> _targets = new List<CheckpointObjectiveTarget>();

	public override string UniqueId => "naval_storyline_quest_3_reach_position_objective";

	public override TextObject Name => new TextObject("{=nGpnbplB}Escape Zone", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=4YtHaWFC}Reach the open seas by avoiding enemy ships.", (Dictionary<string, object>)null);

	internal ReachEscapeZoneObjective(Mission mission, MissionShip ship, Vec3 position)
		: base(mission)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		_controller = ((MissionObjective)this).Mission.GetMissionBehavior<BlockedEstuaryMissionController>();
		((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)new ShipObjectiveTarget(ship, ship.ShipOrigin.Name, showController: true));
		List<GameEntity> list = CollectCheckpoints();
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (GameEntity item in list)
		{
			CheckpointObjectiveTarget checkpointObjectiveTarget = new CheckpointObjectiveTarget(item);
			((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)checkpointObjectiveTarget);
			_targets.Add(checkpointObjectiveTarget);
		}
		_targets[0].SetActive(isActive: true);
		_targets[_targets.Count - 1].SetName(new TextObject("{=nGpnbplB}Escape Zone", (Dictionary<string, object>)null));
	}

	private List<GameEntity> CollectCheckpoints()
	{
		List<GameEntity> list = new List<GameEntity>();
		int num = 1;
		while (true)
		{
			GameEntity val = ((MissionObjective)this).Mission.Scene.FindEntityWithTag("sp_escape_objective_" + num);
			if (val == (GameEntity)null)
			{
				break;
			}
			list.Add(val);
			num++;
		}
		return list;
	}

	protected override void OnTick(float dt)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		((MissionObjective)this).OnTick(dt);
		if (Agent.Main == null || !Agent.Main.IsActive())
		{
			return;
		}
		for (int i = 0; i < _targets.Count; i++)
		{
			CheckpointObjectiveTarget checkpointObjectiveTarget = _targets[i];
			if (checkpointObjectiveTarget.IsInside(Agent.Main.Position))
			{
				checkpointObjectiveTarget.SetActive(isActive: false);
				for (int num = i - 1; num >= 0; num--)
				{
					_targets[num].SetActive(isActive: false);
				}
				if (i < _targets.Count - 1)
				{
					_targets[i + 1].SetActive(isActive: true);
				}
			}
		}
	}

	protected override bool IsActivationRequirementsMet()
	{
		if (_controller != null)
		{
			return _controller.CurrentPhase == BlockedEstuaryMissionController.BattlePhase.Phase3;
		}
		return false;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		return false;
	}
}
