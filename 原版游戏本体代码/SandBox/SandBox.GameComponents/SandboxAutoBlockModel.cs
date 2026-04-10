using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace SandBox.GameComponents;

public class SandboxAutoBlockModel : AutoBlockModel
{
	public override UsageDirection GetBlockDirection(Mission mission)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Invalid comparison between Unknown and I4
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		Agent mainAgent = mission.MainAgent;
		float num = float.MinValue;
		UsageDirection val = (UsageDirection)1;
		foreach (Agent item in (List<Agent>)(object)mission.Agents)
		{
			if (!item.IsHuman)
			{
				continue;
			}
			ActionStage currentActionStage = item.GetCurrentActionStage(1);
			if (((int)currentActionStage != 0 && (int)currentActionStage != 1 && (int)currentActionStage != 2) || !item.IsEnemyOf(mainAgent))
			{
				continue;
			}
			Vec3 val2 = item.Position - mainAgent.Position;
			float num2 = ((Vec3)(ref val2)).Normalize();
			float num3 = MBMath.ClampFloat(Vec3.DotProduct(val2, mainAgent.LookDirection) + 0.8f, 0f, 1f);
			float num4 = MBMath.ClampFloat(1f / (num2 + 0.5f), 0f, 1f);
			float num5 = MBMath.ClampFloat(0f - Vec3.DotProduct(val2, item.LookDirection) + 0.5f, 0f, 1f);
			float num6 = num3 * num4 * num5;
			if (num6 > num)
			{
				num = num6;
				val = item.GetCurrentActionDirection(1);
				if ((int)val == -1)
				{
					val = (UsageDirection)1;
				}
			}
		}
		return val;
	}
}
