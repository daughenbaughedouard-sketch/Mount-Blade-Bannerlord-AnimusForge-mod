using System.Collections.Generic;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest4;

public class DestroyMangonelsObjective : MissionObjective
{
	private readonly int _initialTargets;

	private int _remainingTargets;

	public override string UniqueId => "naval_storyline_quest_4_destroy_targets_objective";

	public override TextObject Name => new TextObject("{=ZpuppygP}Destroy the Mangonels", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=OrI07kdd}Steer the Wasp and destroy the mangonels with your ballista without getting hit yourself", (Dictionary<string, object>)null);

	public DestroyMangonelsObjective(Mission mission, MBList<ShipMangonel> targets)
		: base(mission)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		_initialTargets = ((List<ShipMangonel>)(object)targets).Count;
		_remainingTargets = ((List<ShipMangonel>)(object)targets).Count;
		foreach (ShipMangonel item in (List<ShipMangonel>)(object)targets)
		{
			((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)new MangonelObjectiveTarget(item));
			((UsableMachine)item).DestructionComponent.OnDestroyed += new OnHitTakenAndDestroyedDelegate(OnMangonelDestroyed);
		}
	}

	private void OnMangonelDestroyed(DestructableComponent target, Agent attackerAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
	{
		_remainingTargets--;
	}

	protected override bool IsActivationRequirementsMet()
	{
		return _remainingTargets > 0;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		return _remainingTargets == 0;
	}

	public override MissionObjectiveProgressInfo GetCurrentProgress()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		return new MissionObjectiveProgressInfo
		{
			CurrentProgressAmount = _initialTargets - _remainingTargets,
			RequiredProgressAmount = _initialTargets
		};
	}
}
