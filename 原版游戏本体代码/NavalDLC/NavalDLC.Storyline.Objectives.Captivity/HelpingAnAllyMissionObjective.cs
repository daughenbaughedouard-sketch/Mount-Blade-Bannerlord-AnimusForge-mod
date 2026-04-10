using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Captivity;

public class HelpingAnAllyMissionObjective : MissionObjective
{
	private readonly TextObject _name;

	private readonly TextObject _description;

	private MissionObjectiveProgressInfo _cachedProgress;

	public override string UniqueId => "HelpingAnAllyMissionObjective";

	public override TextObject Name => _name;

	public override TextObject Description => _description;

	public HelpingAnAllyMissionObjective(Mission mission)
		: base(mission)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_name = new TextObject("{=J9ruJTIQ}Protect the Merchants", (Dictionary<string, object>)null);
		_description = new TextObject("{=u2q4PdaI}Defeat all Sea Hounds before they capture the Vlandian merchantman", (Dictionary<string, object>)null);
		_cachedProgress = default(MissionObjectiveProgressInfo);
	}

	protected override bool IsActivationRequirementsMet()
	{
		return true;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		return false;
	}

	public override MissionObjectiveProgressInfo GetCurrentProgress()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _cachedProgress;
	}
}
