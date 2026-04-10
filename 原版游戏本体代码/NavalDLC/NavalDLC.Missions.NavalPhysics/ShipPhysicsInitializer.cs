using TaleWorlds.Library;

namespace NavalDLC.Missions.NavalPhysics;

public static class ShipPhysicsInitializer
{
	public static Vec3 GetDefaultInertia(float mass, in Vec3 draftVolume)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.08333f * mass * (draftVolume.y * draftVolume.y + draftVolume.z * draftVolume.z);
		float num2 = 0.08333f * mass * (draftVolume.x * draftVolume.x + draftVolume.z * draftVolume.z);
		float num3 = 0.08333f * mass * (draftVolume.x * draftVolume.x + draftVolume.y * draftVolume.y);
		return new Vec3(num, num2, num3, -1f);
	}
}
