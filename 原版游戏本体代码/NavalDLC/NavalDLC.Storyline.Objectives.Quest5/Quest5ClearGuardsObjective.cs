using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest5;

public class Quest5ClearGuardsObjective : MissionObjective
{
	private class ClearGuardObjectiveTarget : MissionObjectiveTarget
	{
		private readonly Agent _target;

		public ClearGuardObjectiveTarget(Agent target)
		{
			_target = target;
		}

		public override TextObject GetName()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return new TextObject("{=1sJcKkVP}Guard", (Dictionary<string, object>)null);
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
			if (_target != null)
			{
				return _target.IsActive();
			}
			return false;
		}
	}

	private readonly List<Agent> _stealthAgents;

	private readonly int _requiredProgressAmount;

	public override string UniqueId => "quest_5_clear_guards_objective";

	public override TextObject Name => new TextObject("{=qc5Ymr0P}Take out the guards", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=12lWaxfF}Take out the guards as stealthily as possible.", (Dictionary<string, object>)null);

	public Quest5ClearGuardsObjective(Mission mission, List<Agent> stealthAgents)
		: base(mission)
	{
		_stealthAgents = stealthAgents;
		_requiredProgressAmount = _stealthAgents.Count;
	}

	public override MissionObjectiveProgressInfo GetCurrentProgress()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return new MissionObjectiveProgressInfo
		{
			CurrentProgressAmount = _requiredProgressAmount - _stealthAgents.Count,
			RequiredProgressAmount = _requiredProgressAmount
		};
	}

	protected override bool IsActivationRequirementsMet()
	{
		return _stealthAgents != null;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		if (_stealthAgents != null)
		{
			if (!Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_stealthAgents))
			{
				return !LinQuick.AnyQ<Agent>(_stealthAgents, (Func<Agent, bool>)((Agent a) => a.IsActive()));
			}
			return true;
		}
		return false;
	}
}
