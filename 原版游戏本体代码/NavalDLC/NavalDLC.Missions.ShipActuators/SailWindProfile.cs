using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.Missions.ShipActuators;

public class SailWindProfile
{
	private const int BinCount = 36;

	private const float BinAngleInDegrees = 10f;

	private static SailWindProfile _instance;

	private (float dragCoef, float liftCoef)[][] _sailWindProfiles;

	public static SailWindProfile Instance => _instance;

	public static bool IsSailWindProfileInitialized => _instance != null;

	public static void InitializeProfile()
	{
		_instance = new SailWindProfile();
	}

	public static void InitializeProfileForEditor()
	{
		if (_instance == null)
		{
			_instance = new SailWindProfile();
		}
	}

	public static void FinalizeProfile()
	{
		_instance.Destroy();
		_instance = null;
	}

	private void FillSailProfiles()
	{
		(float, float)[] array = GenerateSquareSailWindProfile();
		_sailWindProfiles[0] = array;
		(float, float)[] array2 = GenerateLateenSailWindProfile();
		_sailWindProfiles[1] = array2;
	}

	private SailWindProfile()
	{
		_sailWindProfiles = new(float, float)[2][];
		FillSailProfiles();
	}

	private void Destroy()
	{
		for (int i = 0; i < 2; i++)
		{
			_sailWindProfiles[i] = null;
		}
		_sailWindProfiles = null;
	}

	public float ComputeSailThrustValue(SailType sailType, Vec2 sailDir, Vec2 desiredThrustDir, Vec2 windDir)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return Vec2.DotProduct(GetSailForceCoefficients(sailType, sailDir, windDir), desiredThrustDir);
	}

	public Vec2 GetMaximumSailForceCoefficients(SailType sailType)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		float num = -MathF.PI;
		float num2 = -MathF.PI;
		Vec2 result = default(Vec2);
		((Vec2)(ref result))._002Ector(0f, 0f);
		float num3 = 0.17453292f;
		Vec2 sailDir = default(Vec2);
		Vec2 windDir = default(Vec2);
		for (int i = 0; i < 36; i++)
		{
			((Vec2)(ref sailDir))._002Ector(MathF.Cos(num), MathF.Sin(num));
			for (int j = 0; j < 36; j++)
			{
				((Vec2)(ref windDir))._002Ector(MathF.Cos(num2), MathF.Sin(num2));
				float angleOfAttack = GetAngleOfAttack(in sailDir, in windDir);
				(float, float) sailCoefs = GetSailCoefs(angleOfAttack, sailType);
				Vec2 val = ((Vec2)(ref windDir)).LeftVec();
				Vec2 val2 = windDir * sailCoefs.Item1 + val * sailCoefs.Item2;
				if (((Vec2)(ref val2)).LengthSquared >= ((Vec2)(ref result)).LengthSquared)
				{
					result = val2;
				}
				num2 += num3;
			}
			num += num3;
		}
		return result;
	}

	public Vec2 GetSailForceCoefficients(SailType sailType, Vec2 sailDir, Vec2 windDir)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float angleOfAttack = GetAngleOfAttack(in sailDir, in windDir);
		(float, float) sailCoefs = GetSailCoefs(angleOfAttack, sailType);
		Vec2 val = ((Vec2)(ref windDir)).LeftVec();
		return windDir * sailCoefs.Item1 + val * sailCoefs.Item2;
	}

	public (float dragCoef, float liftCoef) GetSailCoefs(float angleOfAttackInRadians, SailType sailType)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		float num = ((angleOfAttackInRadians < 0f) ? (angleOfAttackInRadians + MathF.PI * 2f) : angleOfAttackInRadians) * 57.29578f;
		int num2 = (int)(num / 10f) % 36;
		int num3 = (num2 + 1) % 36;
		(float, float)[] array = _sailWindProfiles[sailType];
		float num4 = num % 10f / 10f;
		float item = (1f - num4) * array[num2].Item1 + num4 * array[num3].Item1;
		float item2 = (1f - num4) * array[num2].Item2 + num4 * array[num3].Item2;
		return (dragCoef: item, liftCoef: item2);
	}

	private (float dragCoef, float liftCoef)[] GenerateLateenSailWindProfile()
	{
		return new(float, float)[36]
		{
			(0.02f, 0f),
			(0.06f, 0.08f),
			(0.08f, 0.12f),
			(0.12f, 0.1f),
			(0.13f, 0.08f),
			(0.17f, 0.06f),
			(0.28f, 0.04f),
			(0.41f, 0.03f),
			(0.46f, 0.02f),
			(0.6f, 0f),
			(0.46f, -0.02f),
			(0.41f, -0.03f),
			(0.28f, -0.04f),
			(0.17f, -0.06f),
			(0.13f, -0.08f),
			(0.12f, -0.1f),
			(0.08f, -0.12f),
			(0.06f, -0.08f),
			(0.02f, 0f),
			(0.06f, 0.12f),
			(0.08f, 0.38f),
			(0.14f, 0.36f),
			(0.26f, 0.24f),
			(0.34f, 0.16f),
			(0.56f, 0.12f),
			(0.82f, 0.09f),
			(0.92f, 0.03f),
			(1f, 0f),
			(0.92f, -0.03f),
			(0.82f, -0.09f),
			(0.56f, -0.12f),
			(0.34f, -0.16f),
			(0.26f, -0.24f),
			(0.14f, -0.36f),
			(0.08f, -0.38f),
			(0.06f, -0.12f)
		};
	}

	private (float dragCoef, float liftCoef)[] GenerateSquareSailWindProfile()
	{
		return new(float, float)[36]
		{
			(1f, 0f),
			(0.94f, -0.03f),
			(0.86f, -0.09f),
			(0.72f, -0.12f),
			(0.52f, -0.16f),
			(0.36f, -0.24f),
			(0.32f, -0.36f),
			(0.18f, -0.38f),
			(0.06f, -0.12f),
			(0.04f, -0f),
			(0.06f, 0.03f),
			(0.18f, 0.07f),
			(0.32f, 0.1f),
			(0.36f, 0.13f),
			(0.52f, 0.13f),
			(0.72f, 0.1f),
			(0.86f, 0.07f),
			(0.94f, 0.03f),
			(1f, 0f),
			(0.94f, -0.03f),
			(0.86f, -0.07f),
			(0.72f, -0.1f),
			(0.52f, -0.13f),
			(0.36f, -0.13f),
			(0.32f, -0.1f),
			(0.18f, -0.07f),
			(0.06f, -0.03f),
			(0.04f, 0f),
			(0.06f, 0.12f),
			(0.18f, 0.38f),
			(0.32f, 0.36f),
			(0.36f, 0.24f),
			(0.52f, 0.16f),
			(0.72f, 0.12f),
			(0.86f, 0.09f),
			(0.94f, 0.03f)
		};
	}

	public static float GetAngleOfAttack(in Vec2 sailDir, in Vec2 windDir)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = sailDir;
		Vec3 val2 = ((Vec2)(ref val)).ToVec3(0f);
		val = windDir;
		Vec3 val3 = Vec3.CrossProduct(val2, ((Vec2)(ref val)).ToVec3(0f));
		float num = Vec2.DotProduct(sailDir, windDir);
		return MathF.Atan2(val3.z, num);
	}

	public static float NormalizeThrustValue(float thrustValue, float minThrustValue, float maxThrustValue)
	{
		if (maxThrustValue == minThrustValue)
		{
			return 0f;
		}
		return (thrustValue - minThrustValue) / (maxThrustValue - minThrustValue);
	}
}
