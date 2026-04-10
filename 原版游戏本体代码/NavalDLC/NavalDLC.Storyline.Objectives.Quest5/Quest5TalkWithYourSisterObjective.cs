using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest5;

public class Quest5TalkWithYourSisterObjective : MissionObjective
{
	private class TalkWithYourSisterObjectiveTarget : MissionObjectiveTarget
	{
		public readonly Agent TargetAgent;

		public TalkWithYourSisterObjectiveTarget(Agent sister)
		{
			TargetAgent = sister;
		}

		public override TextObject GetName()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return new TextObject("{=pY5bft0t}Cage for prisoners", (Dictionary<string, object>)null);
		}

		public override Vec3 GetGlobalPosition()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return TargetAgent.GetEyeGlobalPosition();
		}

		public override bool IsActive()
		{
			if (TargetAgent != null)
			{
				return TargetAgent.IsActive();
			}
			return false;
		}
	}

	private TalkWithYourSisterObjectiveTarget _target;

	public override string UniqueId => "quest_5_talk_with_your_sister_objective";

	public override TextObject Name => new TextObject("{=btfAQ47G}Find your sister", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=VTjKuGYw}Find your sister in the hold of the prisoner ship.", (Dictionary<string, object>)null);

	public Quest5TalkWithYourSisterObjective(Mission mission, Agent sister)
		: base(mission)
	{
		_target = new TalkWithYourSisterObjectiveTarget(sister);
		((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)_target);
	}

	protected override bool IsActivationRequirementsMet()
	{
		return _target != null;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		return false;
	}
}
