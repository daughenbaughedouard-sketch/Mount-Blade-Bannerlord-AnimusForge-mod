using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest5;

public class Quest5ApproachObjective : MissionObjective
{
	private class ApproachObjectiveTarget : MissionObjectiveTarget
	{
		public readonly MatrixFrame ApproachTargetFrame;

		public ApproachObjectiveTarget(MatrixFrame approachTargetFrame)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			ApproachTargetFrame = approachTargetFrame;
		}

		public override TextObject GetName()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return new TextObject("{=9pyEoT2i}Hailing point", (Dictionary<string, object>)null);
		}

		public override Vec3 GetGlobalPosition()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ApproachTargetFrame.origin;
		}

		public override bool IsActive()
		{
			return true;
		}
	}

	private readonly MissionShip _playerShip;

	private readonly float _completionDistance;

	private ApproachObjectiveTarget _target;

	public override string UniqueId => "quest_5_approach_objective";

	public override TextObject Name => new TextObject("{=s8t5kclT}Approach the meeting zone", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=EmIS3tfC}Sail to within hailing distance of the Sea Hound ship.", (Dictionary<string, object>)null);

	public Quest5ApproachObjective(Mission mission, MissionShip playerShip, MatrixFrame approachTargetFrame, float completionDistance)
		: base(mission)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		_playerShip = playerShip;
		_completionDistance = completionDistance;
		_target = new ApproachObjectiveTarget(approachTargetFrame);
		((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)_target);
	}

	protected override bool IsActivationRequirementsMet()
	{
		return _target != null;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (_target != null)
		{
			Vec3 origin = _target.ApproachTargetFrame.origin;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_playerShip).GameEntity;
			return ((Vec3)(ref origin)).Distance(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin) <= _completionDistance;
		}
		return false;
	}
}
