using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest5;

public class Quest5ReturnToDeckObjective : MissionObjective
{
	private class ReturnToDeckObjectiveTarget : MissionObjectiveTarget<GameEntity>
	{
		public ReturnToDeckObjectiveTarget(GameEntity target)
			: base(target)
		{
		}

		public override TextObject GetName()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return new TextObject("{=5MH4xtlD}Gunnar", (Dictionary<string, object>)null);
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

	private GameEntity _deckSpawnPointEntity;

	private ReturnToDeckObjectiveTarget _targetDoor;

	public override string UniqueId => "quest_5_return_to_deck_objective";

	public override TextObject Name => new TextObject("{=Cvwf3F6h}Return to Gunnar", (Dictionary<string, object>)null);

	public override TextObject Description => new TextObject("{=ZRLg1dYM}Leave the hold to talk to Gunnar.", (Dictionary<string, object>)null);

	public Quest5ReturnToDeckObjective(Mission mission, GameEntity targetDoorEntity, GameEntity deckSpawnPointEntity)
		: base(mission)
	{
		_deckSpawnPointEntity = deckSpawnPointEntity;
		_targetDoor = new ReturnToDeckObjectiveTarget(targetDoorEntity);
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
			return ((Vec3)(ref position)).Distance(_deckSpawnPointEntity.GlobalPosition) <= 3f;
		}
		return false;
	}
}
