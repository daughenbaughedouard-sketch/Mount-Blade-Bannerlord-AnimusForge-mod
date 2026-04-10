using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace SandBox.Missions.MissionLogics.Hideout.Objectives;

public class LocateTheMainCampObjective : MissionObjective
{
	public override string UniqueId => "hideout_mission_locate_the_main_camp_objective";

	public override TextObject Name => new TextObject("{=2g03vuC7}Locate the Main Camp", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=wmvJ0bcH}Sneak your way through the sentries.", (Dictionary<string, object>)null);

	public LocateTheMainCampObjective(Mission mission)
		: base(mission)
	{
	}
}
