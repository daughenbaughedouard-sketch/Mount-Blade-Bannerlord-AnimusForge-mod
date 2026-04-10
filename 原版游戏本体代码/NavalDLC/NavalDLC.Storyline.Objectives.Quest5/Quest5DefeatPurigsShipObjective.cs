using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest5;

public class Quest5DefeatPurigsShipObjective : MissionObjective
{
	private class DefeatPurigsShipTarget : MissionObjectiveTarget
	{
		private readonly MissionShip _target;

		public DefeatPurigsShipTarget(MissionShip target)
		{
			_target = target;
		}

		public override TextObject GetName()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return new TextObject("{=ny9Rllh3}Purig's Ship", (Dictionary<string, object>)null);
		}

		public override Vec3 GetGlobalPosition()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			return _target.GlobalFrame.origin + Vec3.Up;
		}

		public override bool IsActive()
		{
			Agent main = Agent.Main;
			if (main != null && main.IsActive())
			{
				return !_target.GetIsAgentOnShip(Agent.Main);
			}
			return false;
		}
	}

	private readonly List<Agent> _purigShipAgents;

	private MissionObjectiveProgressInfo _cachedProgress;

	public override string UniqueId => "quest_5_defeat_purigs_ship_objective";

	public override TextObject Name => new TextObject("{=CedcuMUS}Defeat Purig's crew", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=YDPv1Nsm}Board Purig's ship and defeat his crew.", (Dictionary<string, object>)null);

	public Quest5DefeatPurigsShipObjective(Mission mission, List<Agent> purigShipAgents, MissionShip purigsShip)
		: base(mission)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)new DefeatPurigsShipTarget(purigsShip));
		_purigShipAgents = purigShipAgents;
		_cachedProgress = default(MissionObjectiveProgressInfo);
		_cachedProgress.RequiredProgressAmount = _purigShipAgents.Count;
	}

	public override MissionObjectiveProgressInfo GetCurrentProgress()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		_cachedProgress.CurrentProgressAmount = _cachedProgress.RequiredProgressAmount - _purigShipAgents.Count();
		return _cachedProgress;
	}

	protected override bool IsActivationRequirementsMet()
	{
		return true;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		if (!Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_purigShipAgents))
		{
			return !_purigShipAgents.Any((Agent a) => a.IsActive());
		}
		return true;
	}
}
