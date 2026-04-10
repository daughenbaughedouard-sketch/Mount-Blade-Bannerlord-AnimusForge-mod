using System.Collections.Generic;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class NavalBoundaryForceFieldLogic : MissionLogic
{
	private const float SoftStart = 20f;

	private const float HardStop = 0.25f;

	private const float MaxAcceleleration = 6f;

	private const float VRef = 3f;

	private const float SeparationVelocityGain = 4f;

	private const float Damping = 2f;

	private MBList<Vec2> _hardBoundaryPoints;

	private NavalShipsLogic _navalShipsLogic;

	public MBReadOnlyList<Vec2> HardBoundaryPoints => (MBReadOnlyList<Vec2>)(object)_hardBoundaryPoints;

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_hardBoundaryPoints = new MBList<Vec2>();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
	}

	public override void OnAfterDeploymentFinished()
	{
		_hardBoundaryPoints = MBSceneUtilities.GetHardBoundaryPoints(Mission.Current.Scene);
	}

	public override void OnFixedMissionTick(float fixedDt)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		if (!((MissionBehavior)this).Mission.IsDeploymentFinished)
		{
			return;
		}
		float num = 0f;
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			num = MathF.Max(num, item.Physics.PhysicsBoundingBoxWithChildren.radius);
		}
		float num2 = 20f + num;
		float num3 = num2 * num2;
		Vec2 val = default(Vec2);
		bool flag = default(bool);
		foreach (MissionShip item2 in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (!item2.IsShipOrderActive || item2.ShipOrder.MovementOrderEnum == ShipOrder.ShipMovementOrderEnum.Retreat)
			{
				continue;
			}
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
			Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().origin;
			Vec2 asVec = ((Vec3)(ref origin)).AsVec2;
			float num4 = MBSceneUtilities.FindClosestPointToBoundariesReturnDistanceSquared(ref asVec, _hardBoundaryPoints, ref val, ref flag);
			Vec2 val2 = asVec - val;
			Vec3 val3 = ((Vec2)(ref val2)).ToVec3(0f);
			if (!(num4 >= 1E-05f) || !(num4 <= num3))
			{
				continue;
			}
			float num5 = ((Vec3)(ref val3)).Normalize();
			float radius = item2.Physics.PhysicsBoundingBoxWithoutChildren.radius;
			Vec3 val4 = origin - val3 * radius;
			val2 = ((Vec3)(ref val4)).AsVec2 - val;
			float length = ((Vec2)(ref val2)).Length;
			float num6 = MathF.Max(19.75f, 0.001f);
			if (!(length <= 20f))
			{
				continue;
			}
			float mass = item2.Physics.Mass;
			float num7 = Vec3.DotProduct(item2.Physics.LinearVelocity, -val3);
			float num8 = 20f - (length - 0.25f);
			float num9 = MathF.Clamp(num8 / num6, 0f, 1f);
			float num10 = MathF.Clamp(num7 / 3f, 0f, 1f);
			float num11 = num9 * (0.5f + 0.5f * num10);
			if (num8 >= num6)
			{
				if (num7 > 0f)
				{
					Vec3 forceVec = val3 * (num7 * mass);
					item2.Physics.ApplyForceToDynamicBody(in forceVec, (ForceMode)1);
					num7 = 0f;
				}
				float num12 = 4f * (num8 - num6);
				if (num12 > 0f)
				{
					float num13 = num12 - num7;
					if (num13 > 0f)
					{
						Vec3 forceVec2 = val3 * (mass * num13);
						item2.Physics.ApplyForceToDynamicBody(in forceVec2, (ForceMode)1);
					}
				}
			}
			if (num8 > 0f || num5 <= radius + 20f)
			{
				float num14 = 6f * (0.25f + 0.75f * num11);
				Vec3 forceVec3 = val3 * (num14 * mass);
				item2.Physics.ApplyForceToDynamicBody(in forceVec3, (ForceMode)0);
			}
			if (num7 > 0f)
			{
				item2.Physics.ApplyForceToDynamicBody(val3 * (2f * num7 * mass), (ForceMode)0);
			}
		}
	}
}
