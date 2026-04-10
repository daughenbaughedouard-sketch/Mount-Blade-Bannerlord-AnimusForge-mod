using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace TaleWorlds.MountAndBlade;

public class MultiplayerStrikeMagnitudeModel : StrikeMagnitudeCalculationModel
{
	public override float CalculateStrikeMagnitudeForMissile(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float missileSpeed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		AttackCollisionData val = collisionData;
		float missileTotalDamage = ((AttackCollisionData)(ref val)).MissileTotalDamage;
		val = collisionData;
		float missileStartingBaseSpeed = ((AttackCollisionData)(ref val)).MissileStartingBaseSpeed;
		float num = missileSpeed;
		float num2 = missileSpeed - missileStartingBaseSpeed;
		if (num2 > 0f)
		{
			MPCombatPerkHandler combatPerkHandler = MPPerkObject.GetCombatPerkHandler(attackInformation.AttackerAgent, attackInformation.VictimAgent);
			if (combatPerkHandler != null)
			{
				MissionWeapon val2 = weapon;
				WeaponComponentData currentUsageItem = ((MissionWeapon)(ref val2)).CurrentUsageItem;
				val = collisionData;
				float num3 = MathF.Max(MathF.Sqrt(1f + combatPerkHandler.GetSpeedBonusEffectiveness(currentUsageItem, (DamageTypes)((AttackCollisionData)(ref val)).DamageType)) - 1f, 0f);
				num += num2 * num3;
			}
		}
		num /= missileStartingBaseSpeed;
		return num * num * missileTotalDamage;
	}

	public override float CalculateStrikeMagnitudeForSwing(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float swingSpeed, float impactPoint, float extraLinearSpeed)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float num = extraLinearSpeed;
		MissionWeapon val;
		if (extraLinearSpeed > 0f)
		{
			MPCombatPerkHandler combatPerkHandler = MPPerkObject.GetCombatPerkHandler(attackInformation.AttackerAgent, attackInformation.VictimAgent);
			if (combatPerkHandler != null)
			{
				val = weapon;
				WeaponComponentData currentUsageItem = ((MissionWeapon)(ref val)).CurrentUsageItem;
				AttackCollisionData val2 = collisionData;
				float num2 = MathF.Max(MathF.Sqrt(1f + combatPerkHandler.GetSpeedBonusEffectiveness(currentUsageItem, (DamageTypes)((AttackCollisionData)(ref val2)).DamageType)) - 1f, 0f);
				num += num * num2;
			}
		}
		val = weapon;
		WeaponComponentData currentUsageItem2 = ((MissionWeapon)(ref val)).CurrentUsageItem;
		val = weapon;
		return CombatStatCalculator.CalculateStrikeMagnitudeForSwing(swingSpeed, impactPoint, ((MissionWeapon)(ref val)).Item.Weight, currentUsageItem2.GetRealWeaponLength(), currentUsageItem2.TotalInertia, currentUsageItem2.CenterOfMass, num);
	}

	public override float CalculateStrikeMagnitudeForUnarmedAttack(in AttackInformation attackInformation, in AttackCollisionData collisionData, float progressEffect, float momentumRemaining)
	{
		return momentumRemaining * progressEffect * ManagedParameters.Instance.GetManagedParameter((ManagedParametersEnum)15);
	}

	public override float CalculateStrikeMagnitudeForThrust(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float thrustWeaponSpeed, float extraLinearSpeed, bool isThrown = false)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		float num = extraLinearSpeed;
		MissionWeapon val;
		if (extraLinearSpeed > 0f)
		{
			MPCombatPerkHandler combatPerkHandler = MPPerkObject.GetCombatPerkHandler(attackInformation.AttackerAgent, attackInformation.VictimAgent);
			if (combatPerkHandler != null)
			{
				val = weapon;
				WeaponComponentData currentUsageItem = ((MissionWeapon)(ref val)).CurrentUsageItem;
				AttackCollisionData val2 = collisionData;
				float num2 = MathF.Max(MathF.Sqrt(1f + combatPerkHandler.GetSpeedBonusEffectiveness(currentUsageItem, (DamageTypes)((AttackCollisionData)(ref val2)).DamageType)) - 1f, 0f);
				num += num * num2;
			}
		}
		val = weapon;
		return CombatStatCalculator.CalculateStrikeMagnitudeForThrust(thrustWeaponSpeed, ((MissionWeapon)(ref val)).Item.Weight, num, isThrown);
	}

	public override float ComputeRawDamage(DamageTypes damageType, float magnitude, float armorEffectiveness, float absorbedDamageRatio)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		float bluntDamageFactorByDamageType = ((StrikeMagnitudeCalculationModel)this).GetBluntDamageFactorByDamageType(damageType);
		float num = 100f / (100f + armorEffectiveness);
		float num2 = magnitude * num;
		float num3 = bluntDamageFactorByDamageType * num2;
		if ((int)damageType != 2)
		{
			float num4;
			if ((int)damageType != 0)
			{
				if ((int)damageType != 1)
				{
					Debug.FailedAssert("Given damage type is invalid.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\ComponentInterfaces\\MultiplayerStrikeMagnitudeModel.cs", "ComputeRawDamage", 107);
					return 0f;
				}
				num4 = MathF.Max(0f, magnitude * (45f / (45f + armorEffectiveness)));
			}
			else
			{
				num4 = MathF.Max(0f, magnitude * (1f - 0.6f * armorEffectiveness / (20f + 0.4f * armorEffectiveness)));
			}
			num3 += (1f - bluntDamageFactorByDamageType) * num4;
		}
		return num3 * absorbedDamageRatio;
	}

	public override float GetBluntDamageFactorByDamageType(DamageTypes damageType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected I4, but got Unknown
		float result = 0f;
		switch ((int)damageType)
		{
		case 2:
			result = 1f;
			break;
		case 0:
			result = 0.1f;
			break;
		case 1:
			result = 0.25f;
			break;
		}
		return result;
	}

	public override float CalculateHorseArcheryFactor(BasicCharacterObject characterObject)
	{
		return 100f;
	}

	public override float CalculateBaseBlowMagnitudeForPassiveUsage(in AttackInformation attackInformation, in AttackCollisionData collisionData, float extraLinearSpeed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		return CombatStatCalculator.CalculateBaseBlowMagnitudeForPassiveUsage(((MissionWeapon)(ref attackerWeapon)).Item.Weight, extraLinearSpeed);
	}
}
