using System.Collections.Generic;
using SandBox.Missions.MissionLogics;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class FightBehavior : AgentBehavior
{
	public FightBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if (base.OwnerAgent.HumanAIComponent == null)
		{
			base.OwnerAgent.AddComponent((AgentComponent)new HumanAIComponent(base.OwnerAgent));
		}
	}

	public override float GetAvailability(bool isSimulation)
	{
		if (!MissionFightHandler.IsAgentAggressive(base.OwnerAgent))
		{
			return 0.1f;
		}
		return 1f;
	}

	protected override void OnActivate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		TextObject val = new TextObject("{=!}{p0} {p1} activate alarmed behavior group.", (Dictionary<string, object>)null);
		val.SetTextVariable("p0", base.OwnerAgent.Name.ToString());
		val.SetTextVariable("p1", base.OwnerAgent.Index.ToString());
	}

	protected override void OnDeactivate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		TextObject val = new TextObject("{=!}{p0} {p1} deactivate fight behavior.", (Dictionary<string, object>)null);
		val.SetTextVariable("p0", base.OwnerAgent.Name.ToString());
		val.SetTextVariable("p1", base.OwnerAgent.Index.ToString());
	}

	public override string GetDebugInfo()
	{
		return "Fight";
	}
}
