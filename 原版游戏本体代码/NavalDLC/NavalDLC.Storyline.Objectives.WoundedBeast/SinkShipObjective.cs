using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.WoundedBeast;

internal class SinkShipObjective : MissionObjective
{
	private class SinkShipObjectiveTarget : MissionObjectiveTarget
	{
		public readonly MissionShip TargetShip;

		private readonly TextObject _name;

		public SinkShipObjectiveTarget(MissionShip targetShip, TextObject name)
		{
			TargetShip = targetShip;
			_name = name;
		}

		public override Vec3 GetGlobalPosition()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)TargetShip).GameEntity;
			return ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		}

		public override TextObject GetName()
		{
			return _name;
		}

		public override bool IsActive()
		{
			if (TargetShip != null)
			{
				return !TargetShip.IsSinking;
			}
			return false;
		}
	}

	private readonly MissionShip _targetShip;

	private SinkShipObjectiveTarget _sinkShipObjectiveTarget;

	public override string UniqueId => "naval_storyline_quest_2_sink_ship_objective";

	public override TextObject Name => new TextObject("{=VMVbnNau}Sink Fahda's Flagship", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=rlEJ3pC8}Fahda's flagship was crippled by the storm. Ram it until it sinks!", (Dictionary<string, object>)null);

	public SinkShipObjective(Mission mission, MissionShip targetShip)
		: base(mission)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		_targetShip = targetShip;
		_sinkShipObjectiveTarget = new SinkShipObjectiveTarget(_targetShip, new TextObject("{=gCWSOyLJ}Fahda's Ship", (Dictionary<string, object>)null));
		((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)_sinkShipObjectiveTarget);
	}

	protected override bool IsActivationRequirementsMet()
	{
		return _targetShip != null;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		if (_targetShip != null)
		{
			if (!(_targetShip.HitPoints <= 0f))
			{
				return _targetShip.IsSinking;
			}
			return true;
		}
		return false;
	}
}
