using System;
using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipMangonel : Mangonel
{
	private MissionShip _ship;

	private NavalShipsLogic _navalShipsLogic;

	[EditableScriptComponentVariable(true, "")]
	private float _directionRestriction = MathF.PI * 2f / 3f;

	public override string MultipleProjectileId => "mangonel_c_grapeshot_stack";

	public override float DirectionRestriction => _directionRestriction;

	public override string MultipleProjectileFlyingId => "mangonel_c_grapeshot_projectile";

	public override string MultipleFireProjectileId => "mangonel_c_grapeshot_fire_stack";

	public override string MultipleFireProjectileFlyingId => "mangonel_c_grapeshot_fire_projectile";

	protected override float ReloadSpeedMultiplier => 6.2f;

	protected override float HorizontalAimSensitivity => 0.5f;

	protected override void OnInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		_ship = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<MissionShip>();
		((Mangonel)this).OnInit();
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.ShipSpawnedEvent += OnShipSpawned;
		foreach (StandingPoint item in (List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)
		{
			((UsableMissionObject)item).IsDisabledForPlayers = true;
		}
	}

	private void OnShipSpawned(MissionShip ship)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (ship == _ship)
		{
			((RangedSiegeWeapon)this).DefaultSide = ship.BattleSide;
		}
		_navalShipsLogic.ShipSpawnedEvent -= OnShipSpawned;
	}
}
