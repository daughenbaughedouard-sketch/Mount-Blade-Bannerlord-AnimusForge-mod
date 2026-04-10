using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.PirateBattle;

public class PirateBattlePhase2Objective : MissionObjective
{
	private readonly PirateBattleMissionController _missionController;

	private readonly TextObject _name;

	private readonly TextObject _description;

	private MissionObjectiveProgressInfo _cachedProgress;

	public override string UniqueId => "PirateBattlePhase2Objective";

	public override TextObject Name => _name;

	public override TextObject Description => _description;

	public PirateBattlePhase2Objective(Mission mission, PirateBattleMissionController missionController)
		: base(mission)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		_name = new TextObject("{=0uxtZE36}Defeat the Reinforcements", (Dictionary<string, object>)null);
		_description = new TextObject("{=rqhEyQ5L}Attack the second Sea Hounds ship with your allies.", (Dictionary<string, object>)null);
		_missionController = missionController;
		_cachedProgress = default(MissionObjectiveProgressInfo);
		_cachedProgress.RequiredProgressAmount = 0;
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
