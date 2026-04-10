using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects;

public class ShipTargetMissionObject : MissionObject, ITargetable
{
	private readonly Vec3 BoundingBoxOffset = Vec3.One;

	private MissionShip _ship;

	private NavalAgentsLogic _navalAgentsLogic;

	protected override void OnInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		_ship = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<MissionShip>();
		_navalAgentsLogic = Mission.Current.GetMissionBehavior<NavalAgentsLogic>();
	}

	public TargetFlags GetTargetFlags()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		TargetFlags val = (TargetFlags)513;
		if (_ship.IsSinking)
		{
			val = (TargetFlags)(val | 0x40);
		}
		return val;
	}

	public float GetTargetValue(List<Vec3> weaponPositions)
	{
		return 500f * GetMultiplierOfShip();
	}

	public WeakGameEntity GetTargetEntity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ((ScriptComponentBehavior)this).GameEntity;
	}

	public Vec3 GetTargetingOffset()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Vec3.Zero;
	}

	public BattleSideEnum GetSide()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _ship.BattleSide;
	}

	public WeakGameEntity Entity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ((ScriptComponentBehavior)this).GameEntity;
	}

	public (Vec3, Vec3) ComputeGlobalPhysicsBoundingBoxMinMax()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		return (globalPosition - BoundingBoxOffset, globalPosition + BoundingBoxOffset);
	}

	public Vec3 GetTargetGlobalVelocity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ship).GameEntity;
		WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
		return GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(gameEntity, ((WeakGameEntity)(ref gameEntity2)).GlobalPosition);
	}

	public bool IsDestructable()
	{
		return true;
	}

	private float GetMultiplierOfShip()
	{
		float num = (float)_navalAgentsLogic.GetActiveAgentCountOfShip(_ship) / ((float)_ship.CrewSizeOnMainDeck * 1f);
		num *= num;
		if (num < 0.0025000002f)
		{
			num = 0f;
		}
		float num2 = MathF.Max(1f, 2f - MathF.Log10(_ship.HitPoints / _ship.MaxHealth * 10f + 1f));
		return num * num2;
	}
}
