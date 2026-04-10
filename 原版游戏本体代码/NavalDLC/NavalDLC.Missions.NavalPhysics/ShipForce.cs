using TaleWorlds.Library;

namespace NavalDLC.Missions.NavalPhysics;

public struct ShipForce
{
	public enum SourceType
	{
		None,
		Sail,
		Oar,
		Rudder
	}

	public readonly Vec3 LocalPosition;

	public Vec3 Force;

	public readonly SourceType Source;

	public readonly float GamifiedForceMultiplier;

	public bool IsApplicable
	{
		get
		{
			if (((Vec3)(ref Force)).IsValid)
			{
				return ((Vec3)(ref Force)).IsNonZero;
			}
			return false;
		}
	}

	public ShipForce(in Vec3 localPosition, in Vec3 force, SourceType source, float gamifiedForceMultiplier)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		LocalPosition = localPosition;
		Force = new Vec3(force, 0f);
		Source = source;
		GamifiedForceMultiplier = gamifiedForceMultiplier;
	}

	public ShipForce(SourceType source)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		LocalPosition = Vec3.Zero;
		Force = Vec3.Zero;
		Source = source;
		GamifiedForceMultiplier = 1f;
	}

	public void ComputeRealisticAndGamifiedForceComponents(out Vec3 realisticForce, out Vec3 gamifiedForce)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		realisticForce = Force / GamifiedForceMultiplier;
		gamifiedForce = realisticForce * (GamifiedForceMultiplier - 1f);
	}

	public static ShipForce None()
	{
		return new ShipForce(SourceType.None);
	}

	public static ShipForce None(SourceType source)
	{
		return new ShipForce(source);
	}
}
