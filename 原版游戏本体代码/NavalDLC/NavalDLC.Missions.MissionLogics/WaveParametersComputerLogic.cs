using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

public class WaveParametersComputerLogic : MissionLogic
{
	public struct WaterParameters
	{
		public float Amplitude;

		public float Wavelength;

		public float WaveNumber;

		public float Omega;

		public float WaveMax;

		public float WaveMin;
	}

	public static WaterParameters AnalyzeHeightMap(Vec2 waveDirection, Scene scene)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		waveDirection = ((Vec2)(ref waveDirection)).Normalized();
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = 0f;
		float num4 = 0f;
		List<float> list = new List<float>();
		float num5 = 0f;
		bool flag = false;
		float num6 = 0.15f;
		float num7 = 0f;
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(num3, num4);
		float num8 = scene.GetWaterLevelAtPosition(val, true, false);
		float num9 = num8;
		for (int i = 0; i < 1000; i++)
		{
			val += waveDirection * num6;
			Vec2 val2 = val + waveDirection * num6;
			num9 = scene.GetWaterLevelAtPosition(val, true, false);
			float waterLevelAtPosition = scene.GetWaterLevelAtPosition(val2, true, false);
			if (num9 > num8 && num9 > waterLevelAtPosition)
			{
				if (flag)
				{
					float item = num7 - num5;
					list.Add(item);
					num5 = num7;
				}
				else
				{
					flag = true;
					num5 = num7;
				}
			}
			num8 = num9;
			num7 += num6;
			if (num9 < num)
			{
				num = num9;
			}
			if (num9 > num2)
			{
				num2 = num9;
			}
		}
		float num10 = 0f;
		if (list.Count >= 1)
		{
			float num11 = 0f;
			foreach (float item2 in list)
			{
				num11 += item2;
			}
			num10 = num11 / (float)list.Count;
		}
		else
		{
			num10 = 80f;
		}
		float amplitude = (num2 - num) * 0.5f;
		float num12 = MathF.PI * 2f / num10;
		float omega = MathF.Sqrt(9.806f * num12);
		return new WaterParameters
		{
			Amplitude = amplitude,
			Wavelength = num10,
			WaveNumber = num12,
			Omega = omega,
			WaveMax = num2,
			WaveMin = num
		};
	}
}
