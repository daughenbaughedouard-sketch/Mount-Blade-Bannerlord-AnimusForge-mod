using System;
using TaleWorlds.Library;

namespace NavalDLC.Missions.ShipControl;

public struct NavalVec
{
	private Vec2 _deltaPosition;

	private float _deltaOrientation;

	private float _deltaSpeed;

	public Vec2 DeltaPosition => _deltaPosition;

	public float DeltaOrientation => _deltaOrientation;

	public float DeltaSpeed => _deltaSpeed;

	public static NavalVec Zero => new NavalVec(in Vec2.Zero, 0f);

	public NavalVec(in Vec2 deltaPosition, float deltaRotation, float deltaSpeed = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_deltaPosition = deltaPosition;
		_deltaOrientation = deltaRotation;
		_deltaSpeed = deltaSpeed;
	}

	public NavalVec(in Vec2 deltaPosition)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_deltaPosition = deltaPosition;
		_deltaOrientation = 0f;
		_deltaSpeed = 0f;
	}

	public void ClampAngle()
	{
		_deltaOrientation = MathF.Clamp(_deltaOrientation, -MathF.PI, MathF.PI);
	}

	public static NavalVec operator +(in NavalVec vec1, in NavalVec vec2)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return new NavalVec(vec1.DeltaPosition + vec2.DeltaPosition, vec1.DeltaOrientation + vec2.DeltaOrientation, vec1.DeltaSpeed + vec2.DeltaSpeed);
	}

	public static NavalVec operator -(in NavalVec vec1, in NavalVec vec2)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return new NavalVec(vec1.DeltaPosition - vec2.DeltaPosition, vec1.DeltaOrientation - vec2.DeltaOrientation, vec1.DeltaSpeed - vec2.DeltaSpeed);
	}

	public static NavalVec operator *(in NavalVec vector, float scalar)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new NavalVec(vector.DeltaPosition * scalar, vector.DeltaOrientation * scalar, vector.DeltaSpeed * scalar);
	}

	public static NavalVec operator *(float scalar, in NavalVec vector)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new NavalVec(scalar * vector.DeltaPosition, scalar * vector.DeltaOrientation, scalar * vector.DeltaSpeed);
	}

	public static NavalVec operator *(in Vec3 vector, in NavalVec nVector)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new NavalVec(vector.x * nVector.DeltaPosition, vector.y * nVector.DeltaOrientation, vector.z * nVector.DeltaSpeed);
	}

	public static NavalVec operator *(in NavalVec nVector, in Vec3 vector)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new NavalVec(nVector.DeltaPosition * vector.x, nVector.DeltaOrientation * vector.y, nVector.DeltaSpeed * vector.z);
	}

	public static NavalVec operator /(in NavalVec vector, float scalar)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new NavalVec(vector.DeltaPosition / scalar, vector.DeltaOrientation / scalar, vector.DeltaSpeed / scalar);
	}
}
