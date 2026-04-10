using System.Collections.Generic;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest4;

public class MangonelObjectiveTarget : MissionObjectiveTarget
{
	private readonly ShipMangonel _shipMangonel;

	public MangonelObjectiveTarget(ShipMangonel shipMangonel)
	{
		_shipMangonel = shipMangonel;
	}

	public override bool IsActive()
	{
		ShipMangonel shipMangonel = _shipMangonel;
		if (shipMangonel == null)
		{
			return false;
		}
		DestructableComponent destructionComponent = ((UsableMachine)shipMangonel).DestructionComponent;
		return ((destructionComponent != null) ? new bool?(destructionComponent.IsDestroyed) : ((bool?)null)) == false;
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=NbpcDXtJ}Mangonel", (Dictionary<string, object>)null);
	}

	public override Vec3 GetGlobalPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_shipMangonel).GameEntity;
		return ((WeakGameEntity)(ref gameEntity)).GlobalPosition + Vec3.Up * 7f;
	}
}
