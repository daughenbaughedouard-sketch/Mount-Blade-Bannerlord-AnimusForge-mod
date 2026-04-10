using System.Collections.Generic;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Captivity;

public class CaptivityFreePrisonersObjective : MissionObjective
{
	private class CaptivityPrisonerTarget : MissionObjectiveTarget<AgentBindsMachine>
	{
		private readonly TextObject _name;

		public CaptivityPrisonerTarget(TextObject name, AgentBindsMachine agentBindMachine)
			: base(agentBindMachine)
		{
			_name = name;
		}

		public override Vec3 GetGlobalPosition()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			if (Agent.Main == null)
			{
				return Vec3.Invalid;
			}
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)base.Target).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			gameEntity = ((ScriptComponentBehavior)base.Target).GameEntity;
			return globalPosition + ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation.u * 1.5f;
		}

		public override TextObject GetName()
		{
			return _name;
		}

		public override bool IsActive()
		{
			return base.Target.HasCaptive;
		}
	}

	private readonly NavalStorylineCaptivityMissionController _captivityMissionController;

	private readonly TextObject _name;

	private readonly TextObject _description;

	private readonly TextObject _targetName;

	private MissionObjectiveProgressInfo _cachedProgress;

	public override string UniqueId => "CaptivityFreePrisonersObjective";

	public override TextObject Name => _name;

	public override TextObject Description => _description;

	public CaptivityFreePrisonersObjective(Mission mission, NavalStorylineCaptivityMissionController captivityMissionController)
		: base(mission)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		_name = new TextObject("{=Kl4fHd5i}Escape Captivity", (Dictionary<string, object>)null);
		_description = new TextObject("{=57iHCBz9}Set all prisoners on the ship free.", (Dictionary<string, object>)null);
		_targetName = new TextObject("{=mx9zqEzQ}Unchain", (Dictionary<string, object>)null);
		_captivityMissionController = captivityMissionController;
		foreach (AgentBindsMachine markedAgentBind in _captivityMissionController.GetMarkedAgentBinds())
		{
			CaptivityPrisonerTarget captivityPrisonerTarget = new CaptivityPrisonerTarget(_targetName, markedAgentBind);
			((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)captivityPrisonerTarget);
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
		MBReadOnlyList<CaptivityPrisonerTarget> targetsCopy = ((MissionObjective)this).GetTargetsCopy<CaptivityPrisonerTarget>();
		_cachedProgress.CurrentProgressAmount = 0;
		_cachedProgress.RequiredProgressAmount = ((List<CaptivityPrisonerTarget>)(object)targetsCopy).Count;
		for (int i = 0; i < ((List<CaptivityPrisonerTarget>)(object)targetsCopy).Count; i++)
		{
			if (!((MissionObjectiveTarget<AgentBindsMachine>)((List<CaptivityPrisonerTarget>)(object)targetsCopy)[i]).Target.HasCaptive)
			{
				_cachedProgress.CurrentProgressAmount++;
			}
		}
	}
}
