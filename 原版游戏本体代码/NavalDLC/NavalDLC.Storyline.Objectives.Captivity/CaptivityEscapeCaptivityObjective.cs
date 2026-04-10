using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Captivity;

public class CaptivityEscapeCaptivityObjective : MissionObjective
{
	private readonly NavalStorylineCaptivityMissionController _captivityMissionController;

	private readonly TextObject _name;

	private readonly TextObject _description;

	private MissionObjectiveProgressInfo _cachedProgress;

	public override string UniqueId => "CaptivityEscapeCaptivityObjective";

	public override TextObject Name => _name;

	public override TextObject Description => _description;

	public CaptivityEscapeCaptivityObjective(Mission mission, NavalStorylineCaptivityMissionController captivityMissionController)
		: base(mission)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		_name = new TextObject("{=Kl4fHd5i}Escape Captivity", (Dictionary<string, object>)null);
		_description = new TextObject("{=3Tvyyz7p}Unchain yourself from the oar bench.", (Dictionary<string, object>)null);
		_captivityMissionController = captivityMissionController;
		_cachedProgress.RequiredProgressAmount = 0;
		_cachedProgress.CurrentProgressAmount = 0;
	}

	protected override bool IsActivationRequirementsMet()
	{
		return true;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		return _captivityMissionController.IsPlayerFree;
	}

	public override MissionObjectiveProgressInfo GetCurrentProgress()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _cachedProgress;
	}
}
