using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest5;

public class Quest5CheckInteriorObjective : MissionObjective
{
	private class CheckInteriorObjectiveTarget : MissionObjectiveTarget<GameEntity>
	{
		public CheckInteriorObjectiveTarget(GameEntity target)
			: base(target)
		{
		}

		public override TextObject GetName()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return new TextObject("{=shipHold}Hold", (Dictionary<string, object>)null);
		}

		public override Vec3 GetGlobalPosition()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			return base.Target.GetGlobalFrame().origin + Vec3.Up;
		}

		public override bool IsActive()
		{
			return true;
		}
	}

	private readonly GameEntity _interiorSpawnPointEntity;

	private CheckInteriorObjectiveTarget _targetDoor;

	public override string UniqueId => "quest_5_check_interior_objective";

	public override TextObject Name => new TextObject("{=eVJ4HNv1}Enter the hold", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=aKzRozvo}Enter the hold of the ship.", (Dictionary<string, object>)null);

	public Quest5CheckInteriorObjective(Mission mission, GameEntity targetDoor, GameEntity interiorSpawnPointEntity)
		: base(mission)
	{
		_interiorSpawnPointEntity = interiorSpawnPointEntity;
		_targetDoor = new CheckInteriorObjectiveTarget(targetDoor);
		((MissionObjective)this).AddTarget((MissionObjectiveTarget)(object)_targetDoor);
	}

	protected override bool IsActivationRequirementsMet()
	{
		return _targetDoor != null;
	}

	protected override bool IsCompletionRequirementsMet()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (_targetDoor != null)
		{
			Vec3 position = Agent.Main.Position;
			return ((Vec3)(ref position)).Distance(_interiorSpawnPointEntity.GlobalPosition) <= 3f;
		}
		return false;
	}
}
