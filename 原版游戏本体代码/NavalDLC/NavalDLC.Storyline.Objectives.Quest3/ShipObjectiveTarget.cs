using NavalDLC.Missions.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest3;

internal class ShipObjectiveTarget : MissionObjectiveTarget
{
	private readonly MissionShip _ship;

	private readonly TextObject _name;

	private readonly bool _showController;

	internal ShipObjectiveTarget(MissionShip ship, TextObject name, bool showController = false)
	{
		_ship = ship;
		_name = name;
		_showController = showController;
	}

	public override Vec3 GetGlobalPosition()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		if (_showController)
		{
			gameEntity = ((ScriptComponentBehavior)_ship.ShipControllerMachine).GameEntity;
			return ((WeakGameEntity)(ref gameEntity)).GlobalPosition + Vec3.Up;
		}
		gameEntity = ((ScriptComponentBehavior)_ship).GameEntity;
		return ((WeakGameEntity)(ref gameEntity)).GlobalPosition + Vec3.Up * 3f;
	}

	public override TextObject GetName()
	{
		return _name;
	}

	public override bool IsActive()
	{
		if (_ship != null && !((MissionObject)_ship).IsDisabled)
		{
			if (_showController)
			{
				return !_ship.IsPlayerControlled;
			}
			return true;
		}
		return false;
	}
}
