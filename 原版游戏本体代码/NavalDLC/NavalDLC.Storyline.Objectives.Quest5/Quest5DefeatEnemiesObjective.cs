using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest5;

public class Quest5DefeatEnemiesObjective : MissionObjective
{
	private readonly int _phase3TotalEnemyCount;

	public override string UniqueId => "quest_5_defeat_enemies_objective";

	public override TextObject Name => new TextObject("{=camyYPvf}Defeat the Sea Hound fleet", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=fgshYPOw}Lead your fleet into battle and defeat your foes.", (Dictionary<string, object>)null);

	public Quest5DefeatEnemiesObjective(Mission mission, int phase3TotalEnemyCount)
		: base(mission)
	{
		_phase3TotalEnemyCount = phase3TotalEnemyCount;
	}

	protected override bool IsActivationRequirementsMet()
	{
		return true;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		return (float)((List<Agent>)(object)Mission.Current.PlayerEnemyTeam.ActiveAgents).Count <= (float)_phase3TotalEnemyCount * 0.01f;
	}
}
