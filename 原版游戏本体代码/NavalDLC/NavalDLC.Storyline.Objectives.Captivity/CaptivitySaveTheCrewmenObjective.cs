using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Captivity;

public class CaptivitySaveTheCrewmenObjective : MissionObjective
{
	private class CaptivityCrewmenTarget : MissionObjectiveTarget<Agent>
	{
		private readonly TextObject _name;

		public CaptivityCrewmenTarget(TextObject name, Agent agent)
			: base(agent)
		{
			_name = name;
		}

		public override Vec3 GetGlobalPosition()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			if (Agent.Main == null)
			{
				return Vec3.Invalid;
			}
			return base.Target.Position + base.Target.Frame.rotation.u * 1.5f;
		}

		public override TextObject GetName()
		{
			return _name;
		}

		public override bool IsActive()
		{
			return !base.Target.IsOnLand();
		}
	}

	private readonly NavalStorylineCaptivityMissionController _captivityMissionController;

	private readonly TextObject _name;

	private readonly TextObject _description;

	private readonly TextObject _targetName;

	private MissionObjectiveProgressInfo _cachedProgress;

	public override string UniqueId => "CaptivitySaveTheCrewmenObjective";

	public override TextObject Name => _name;

	public override TextObject Description => _description;

	public CaptivitySaveTheCrewmenObjective(Mission mission, NavalStorylineCaptivityMissionController captivityMissionController)
		: base(mission)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		_name = new TextObject("{=tvGCC1BF}Save the Crewmen", (Dictionary<string, object>)null);
		_description = new TextObject("{=Ed0TIDfv}Steer the ship to save the crewmen in the water.", (Dictionary<string, object>)null);
		_targetName = new TextObject("{=i0ELqRca}Rescue", (Dictionary<string, object>)null);
		_captivityMissionController = captivityMissionController;
		foreach (Agent scatteredCrewman in _captivityMissionController.GetScatteredCrewmen())
		{
			CaptivityCrewmenTarget captivityCrewmenTarget = new CaptivityCrewmenTarget(_targetName, scatteredCrewman);
			((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)captivityCrewmenTarget);
		}
	}

	protected override bool IsActivationRequirementsMet()
	{
		return true;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		return _cachedProgress.CurrentProgressAmount == _cachedProgress.RequiredProgressAmount;
	}

	public override MissionObjectiveProgressInfo GetCurrentProgress()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _cachedProgress;
	}

	protected override void OnTick(float dt)
	{
		((MissionObjective)this).OnTick(dt);
		MBReadOnlyList<CaptivityCrewmenTarget> targetsCopy = ((MissionObjective)this).GetTargetsCopy<CaptivityCrewmenTarget>();
		_cachedProgress.CurrentProgressAmount = 0;
		_cachedProgress.RequiredProgressAmount = ((List<CaptivityCrewmenTarget>)(object)targetsCopy).Count;
		for (int i = 0; i < ((List<CaptivityCrewmenTarget>)(object)targetsCopy).Count; i++)
		{
			if (((MissionObjectiveTarget<Agent>)((List<CaptivityCrewmenTarget>)(object)targetsCopy)[i]).Target.IsOnLand())
			{
				_cachedProgress.CurrentProgressAmount++;
			}
		}
	}
}
