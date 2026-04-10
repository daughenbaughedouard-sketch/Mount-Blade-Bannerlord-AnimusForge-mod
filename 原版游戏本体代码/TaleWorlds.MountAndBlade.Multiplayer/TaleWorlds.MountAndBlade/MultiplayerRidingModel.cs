using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade;

public class MultiplayerRidingModel : RidingModel
{
	public override float CalculateAcceleration(in EquipmentElement mountElement, in EquipmentElement harnessElement, int ridingSkill)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		EquipmentElement val = mountElement;
		float num = (float)((EquipmentElement)(ref val)).GetModifiedMountManeuver(ref harnessElement) * 0.008f;
		if (ridingSkill >= 0)
		{
			float num2 = num;
			float num3 = ridingSkill;
			val = mountElement;
			num = num2 * (0.7f + 0.003f * (num3 - 1.5f * (float)((EquipmentElement)(ref val)).Item.Difficulty));
		}
		return MathF.Clamp(num, 0.15f, 0.7f);
	}
}
