using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace SandBox.Missions.MissionLogics.Hideout.Objectives;

internal class DefeatHideoutBossObjective : MissionObjective
{
	private readonly TextObject _name;

	private readonly TextObject _description;

	public override string UniqueId => "hideout_mission_defeat_hideout_boss_objective";

	public override TextObject Name => _name;

	public override TextObject Description => _description;

	public DefeatHideoutBossObjective(Mission mission, bool isDuel)
		: base(mission)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		_name = (isDuel ? new TextObject("{=QEynMlwL}Win the Duel", (Dictionary<string, object>)null) : new TextObject("{=0sPTRh6L}Win the Fight", (Dictionary<string, object>)null));
		_description = (isDuel ? new TextObject("{=t13oVKkw}Win the duel against the bandit boss.", (Dictionary<string, object>)null) : new TextObject("{=7vqW1CsE}Eliminate the bandit boss and his troops.", (Dictionary<string, object>)null));
	}
}
