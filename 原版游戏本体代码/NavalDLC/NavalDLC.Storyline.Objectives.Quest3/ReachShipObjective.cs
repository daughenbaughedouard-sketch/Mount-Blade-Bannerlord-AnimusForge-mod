using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using NavalDLC.Storyline.MissionControllers;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest3;

internal class ReachShipObjective : MissionObjective
{
	private BlockedEstuaryMissionController _controller;

	private List<CheckpointObjectiveTarget> _targets = new List<CheckpointObjectiveTarget>();

	private MissionShip _playerShip;

	public override string UniqueId => "naval_storyline_quest_3_reach_ship_objective";

	public override TextObject Name => new TextObject("{=4mQj5K5L}Reach the ship", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=fE1Atxa5}Get to the Sturgian ship. There may be enemies nearby.", (Dictionary<string, object>)null);

	internal ReachShipObjective(Mission mission, Agent gunnarAgent, MissionShip ship)
		: base(mission)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		_controller = ((MissionObjective)this).Mission.GetMissionBehavior<BlockedEstuaryMissionController>();
		_playerShip = ship;
		if (gunnarAgent != null && gunnarAgent.IsActive())
		{
			((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)new AgentObjectiveTarget(gunnarAgent));
		}
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
		if (_playerShip != null)
		{
			CheckpointObjectiveTarget checkpointObjectiveTarget2 = new CheckpointObjectiveTarget(GameEntity.CreateFromWeakEntity(((ScriptComponentBehavior)_playerShip).GameEntity));
			checkpointObjectiveTarget2.SetName(_playerShip.ShipOrigin.Name);
			_targets.Add(checkpointObjectiveTarget2);
			((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)checkpointObjectiveTarget2);
		}
		_targets[0].SetActive(isActive: true);
	}

	private List<GameEntity> CollectCheckpoints()
	{
		List<GameEntity> list = new List<GameEntity>();
		int num = 1;
		while (true)
		{
			GameEntity val = ((MissionObjective)this).Mission.Scene.FindEntityWithTag("sp_horse_objective_" + num);
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
		return true;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		if (_controller != null)
		{
			return _controller.CurrentPhase == BlockedEstuaryMissionController.BattlePhase.Phase3;
		}
		return false;
	}
}
