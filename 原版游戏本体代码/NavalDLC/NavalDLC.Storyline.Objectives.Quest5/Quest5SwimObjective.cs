using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;
using TaleWorlds.MountAndBlade.Objects.Usables;

namespace NavalDLC.Storyline.Objectives.Quest5;

public class Quest5SwimObjective : MissionObjective
{
	private class SwimObjectiveTarget : MissionObjectiveTarget
	{
		private readonly MissionShip _target;

		public SwimObjectiveTarget(MissionShip target)
		{
			_target = target;
		}

		public override TextObject GetName()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return new TextObject("{=4hW7wMrj}Prisoner ship", (Dictionary<string, object>)null);
		}

		public override Vec3 GetGlobalPosition()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)((IEnumerable<ClimbingMachine>)_target.ClimbingMachines).First()).GameEntity;
			return ((WeakGameEntity)(ref gameEntity)).GlobalPosition + Vec3.Up;
		}

		public override bool IsActive()
		{
			return true;
		}
	}

	private SwimObjectiveTarget _target;

	private MissionShip _targetShip;

	public override string UniqueId => "quest_5_swim_objective";

	public override TextObject Name => new TextObject("{=zcQhNQ7i}Reach the prisoner ship", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=lXv922C6}Swim with Gunnar to the ship where the captives are held.", (Dictionary<string, object>)null);

	public Quest5SwimObjective(Mission mission, Agent targetAgent, MissionShip targetShip)
		: base(mission)
	{
		_targetShip = targetShip;
		_target = new SwimObjectiveTarget(targetShip);
		((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)_target);
	}

	protected override bool IsActivationRequirementsMet()
	{
		return _target != null;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		if (_target != null)
		{
			return _targetShip.GetIsAgentOnShip(Agent.Main);
		}
		return false;
	}
}
