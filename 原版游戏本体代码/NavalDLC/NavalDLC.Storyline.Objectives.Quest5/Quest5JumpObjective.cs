using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest5;

public class Quest5JumpObjective : MissionObjective
{
	private class JumpObjectiveTarget : MissionObjectiveTarget
	{
		private readonly Agent _target;

		public JumpObjectiveTarget(Agent target)
		{
			_target = target;
		}

		public override TextObject GetName()
		{
			return _target.Character.Name;
		}

		public override Vec3 GetGlobalPosition()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			return _target.Position + Vec3.Up * 2f;
		}

		public override bool IsActive()
		{
			return true;
		}
	}

	private JumpObjectiveTarget _target;

	public override string UniqueId => "quest_5_jump_objective";

	public override TextObject Name => new TextObject("{=tbHD7j4G}Follow Gunnar into the water", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=bNX3b3Ry}Jump off the ship, following Gunnar into the water.", (Dictionary<string, object>)null);

	public Quest5JumpObjective(Mission mission, Agent targetAgent)
		: base(mission)
	{
		_target = new JumpObjectiveTarget(targetAgent);
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
			return Agent.Main.IsInWater();
		}
		return false;
	}
}
