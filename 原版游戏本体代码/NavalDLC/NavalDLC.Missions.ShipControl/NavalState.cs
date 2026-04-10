using TaleWorlds.Library;

namespace NavalDLC.Missions.ShipControl;

public struct NavalState
{
	private Vec2 _position;

	private float _orientation;

	private float _speed;

	public Vec2 Position => _position;

	public float Orientation => _orientation;

	public Vec2 Direction => Vec2.FromRotation(_orientation);

	public float Speed => _speed;

	public static NavalState Zero => new NavalState(in Vec2.Zero, 0f);

	public NavalState(in Vec2 position, float orientation, float speed = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
		_orientation = MBMath.WrapAngle(orientation);
		_speed = speed;
	}

	public NavalState(in Vec2 position, in Vec2 direction, float speed = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
		Vec2 val = direction;
		_orientation = ((Vec2)(ref val)).RotationInRadians;
		_speed = speed;
	}

	public NavalState(in Vec2 position)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
		_orientation = 0f;
		_speed = 0f;
	}

	public static NavalState operator +(in NavalState state, in NavalVec vector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vec2 position = state._position + vector.DeltaPosition;
		float orientation = state._orientation + vector.DeltaOrientation;
		float speed = state.Speed + vector.DeltaSpeed;
		return new NavalState(in position, orientation, speed);
	}

	public static NavalState operator -(in NavalState state, in NavalVec vector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vec2 position = state._position - vector.DeltaPosition;
		float orientation = state._orientation - vector.DeltaOrientation;
		float speed = state.Speed - vector.DeltaSpeed;
		return new NavalState(in position, orientation, speed);
	}

	public static NavalVec operator -(in NavalState toState, in NavalState fromState)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vec2 deltaPosition = toState._position - fromState._position;
		float smallestDifferenceBetweenTwoAngles = MBMath.GetSmallestDifferenceBetweenTwoAngles(MBMath.WrapAngle(fromState._orientation), MBMath.WrapAngle(toState._orientation));
		float deltaSpeed = toState.Speed - fromState.Speed;
		return new NavalVec(in deltaPosition, smallestDifferenceBetweenTwoAngles, deltaSpeed);
	}

	public void SetTargetDirection(in Vec2 targetDirection)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = targetDirection;
		_orientation = ((Vec2)(ref val)).RotationInRadians;
	}
}
