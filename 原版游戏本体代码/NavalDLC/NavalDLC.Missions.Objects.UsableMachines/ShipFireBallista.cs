using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipFireBallista : ShipBallista
{
	public override SiegeEngineType GetSiegeEngineType()
	{
		return DefaultSiegeEngineTypes.FireBallista;
	}

	public override float ProcessTargetValue(float baseValue, TargetFlags flags)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (Extensions.HasAnyFlag<TargetFlags>(flags, (TargetFlags)64))
		{
			return -1000f;
		}
		if (Extensions.HasAnyFlag<TargetFlags>(flags, (TargetFlags)512))
		{
			baseValue *= 2f;
		}
		if (Extensions.HasAnyFlag<TargetFlags>(flags, (TargetFlags)2))
		{
			baseValue *= 2f;
		}
		if (Extensions.HasAnyFlag<TargetFlags>(flags, (TargetFlags)128))
		{
			baseValue *= 1000f;
		}
		return baseValue;
	}
}
