using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace SandBox.GameComponents;

public class SandboxStrikeMagnitudeModel : StrikeMagnitudeCalculationModel
{
	public override float CalculateHorseArcheryFactor(BasicCharacterObject characterObject)
	{
		return 100f;
	}

	public override float CalculateStrikeMagnitudeForMissile(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float missileSpeed)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Invalid comparison between Unknown and I4
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Invalid comparison between Unknown and I4
		BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
		MissionWeapon val = weapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref val)).CurrentUsageItem;
		AttackCollisionData val2 = collisionData;
		float missileTotalDamage = ((AttackCollisionData)(ref val2)).MissileTotalDamage;
		val2 = collisionData;
		float missileStartingBaseSpeed = ((AttackCollisionData)(ref val2)).MissileStartingBaseSpeed;
		float num = missileSpeed;
		float num2 = missileSpeed - missileStartingBaseSpeed;
		if (num2 > 0f)
		{
			ExplainedNumber val3 = default(ExplainedNumber);
			((ExplainedNumber)(ref val3))._002Ector(0f, false, (TextObject)null);
			CharacterObject val4 = (CharacterObject)(object)((attackerAgentCharacter is CharacterObject) ? attackerAgentCharacter : null);
			if (val4 != null && ((BasicCharacterObject)val4).IsHero)
			{
				WeaponClass ammoClass = currentUsageItem.AmmoClass;
				if ((int)ammoClass == 18 || (int)ammoClass == 19 || (int)ammoClass == 21 || (int)ammoClass == 22 || (int)ammoClass == 23)
				{
					PerkHelper.AddPerkBonusForCharacter(Throwing.RunningThrow, val4, true, ref val3, false);
				}
			}
			num += num2 * ((ExplainedNumber)(ref val3)).ResultNumber;
		}
		num /= missileStartingBaseSpeed;
		return num * num * missileTotalDamage;
	}

	public override float CalculateBaseBlowMagnitudeForPassiveUsage(in AttackInformation attackInformation, in AttackCollisionData collisionData, float extraLinearSpeed)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(extraLinearSpeed, false, (TextObject)null);
		BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
		CharacterObject val2 = (CharacterObject)(object)((attackerAgentCharacter is CharacterObject) ? attackerAgentCharacter : null);
		BasicCharacterObject attackerCaptainCharacter = attackInformation.AttackerCaptainCharacter;
		CharacterObject val3 = (CharacterObject)(object)((attackerCaptainCharacter is CharacterObject) ? attackerCaptainCharacter : null);
		bool doesAttackerHaveMountAgent = attackInformation.DoesAttackerHaveMountAgent;
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		SkillObject relevantSkill = ((MissionWeapon)(ref attackerWeapon)).CurrentUsageItem.RelevantSkill;
		if (doesAttackerHaveMountAgent)
		{
			PerkHelper.AddPerkBonusFromCaptain(Riding.NomadicTraditions, val3, ref val);
		}
		else
		{
			PerkHelper.AddPerkBonusForCharacter(Athletics.SurgingBlow, val2, true, ref val, false);
			PerkHelper.AddPerkBonusFromCaptain(Athletics.SurgingBlow, val3, ref val);
		}
		if (relevantSkill == DefaultSkills.Polearm)
		{
			PerkHelper.AddPerkBonusFromCaptain(Polearm.Lancer, val3, ref val);
			if (doesAttackerHaveMountAgent)
			{
				PerkHelper.AddPerkBonusForCharacter(Polearm.Lancer, val2, true, ref val, false);
				PerkHelper.AddPerkBonusFromCaptain(Polearm.UnstoppableForce, val3, ref val);
			}
		}
		attackerWeapon = attackInformation.AttackerWeapon;
		return CombatStatCalculator.CalculateBaseBlowMagnitudeForPassiveUsage(((MissionWeapon)(ref attackerWeapon)).Item.Weight, ((ExplainedNumber)(ref val)).ResultNumber);
	}

	public override float CalculateStrikeMagnitudeForSwing(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float swingSpeed, float impactPointAsPercent, float extraLinearSpeed)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
		BasicCharacterObject attackerCaptainCharacter = attackInformation.AttackerCaptainCharacter;
		bool doesAttackerHaveMountAgent = attackInformation.DoesAttackerHaveMountAgent;
		MissionWeapon val = weapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref val)).CurrentUsageItem;
		CharacterObject val2 = (CharacterObject)(object)((attackerAgentCharacter is CharacterObject) ? attackerAgentCharacter : null);
		ExplainedNumber val3 = default(ExplainedNumber);
		((ExplainedNumber)(ref val3))._002Ector(extraLinearSpeed, false, (TextObject)null);
		if (val2 != null && extraLinearSpeed > 0f)
		{
			SkillObject relevantSkill = currentUsageItem.RelevantSkill;
			CharacterObject val4 = (CharacterObject)(object)((attackerCaptainCharacter is CharacterObject) ? attackerCaptainCharacter : null);
			if (doesAttackerHaveMountAgent)
			{
				PerkHelper.AddPerkBonusFromCaptain(Riding.NomadicTraditions, val4, ref val3);
			}
			else
			{
				if (relevantSkill == DefaultSkills.TwoHanded)
				{
					PerkHelper.AddPerkBonusForCharacter(TwoHanded.RecklessCharge, val2, true, ref val3, false);
				}
				PerkHelper.AddPerkBonusForCharacter(Roguery.DashAndSlash, val2, true, ref val3, false);
				PerkHelper.AddPerkBonusForCharacter(Athletics.SurgingBlow, val2, true, ref val3, false);
				PerkHelper.AddPerkBonusFromCaptain(Athletics.SurgingBlow, val4, ref val3);
			}
			if (relevantSkill == DefaultSkills.Polearm)
			{
				PerkHelper.AddPerkBonusFromCaptain(Polearm.Lancer, val4, ref val3);
				if (doesAttackerHaveMountAgent)
				{
					PerkHelper.AddPerkBonusForCharacter(Polearm.Lancer, val2, true, ref val3, false);
					PerkHelper.AddPerkBonusFromCaptain(Polearm.UnstoppableForce, val4, ref val3);
				}
			}
		}
		val = weapon;
		ItemObject item = ((MissionWeapon)(ref val)).Item;
		float num = CombatStatCalculator.CalculateStrikeMagnitudeForSwing(swingSpeed, impactPointAsPercent, item.Weight, currentUsageItem.GetRealWeaponLength(), currentUsageItem.TotalInertia, currentUsageItem.CenterOfMass, ((ExplainedNumber)(ref val3)).ResultNumber);
		if (item.IsCraftedByPlayer)
		{
			ExplainedNumber val5 = default(ExplainedNumber);
			((ExplainedNumber)(ref val5))._002Ector(num, false, (TextObject)null);
			PerkHelper.AddPerkBonusForCharacter(Crafting.SharpenedEdge, val2, true, ref val5, false);
			num = ((ExplainedNumber)(ref val5)).ResultNumber;
		}
		return num;
	}

	public override float CalculateStrikeMagnitudeForUnarmedAttack(in AttackInformation attackInformation, in AttackCollisionData collisionData, float progressEffect, float momentumRemaining)
	{
		return momentumRemaining * progressEffect * ManagedParameters.Instance.GetManagedParameter((ManagedParametersEnum)15) * 2f;
	}

	public override float CalculateStrikeMagnitudeForThrust(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float thrustWeaponSpeed, float extraLinearSpeed, bool isThrown = false)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
		BasicCharacterObject attackerCaptainCharacter = attackInformation.AttackerCaptainCharacter;
		bool doesAttackerHaveMountAgent = attackInformation.DoesAttackerHaveMountAgent;
		MissionWeapon val = weapon;
		ItemObject item = ((MissionWeapon)(ref val)).Item;
		float weight = item.Weight;
		val = weapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref val)).CurrentUsageItem;
		CharacterObject val2 = (CharacterObject)(object)((attackerAgentCharacter is CharacterObject) ? attackerAgentCharacter : null);
		ExplainedNumber val3 = default(ExplainedNumber);
		((ExplainedNumber)(ref val3))._002Ector(extraLinearSpeed, false, (TextObject)null);
		if (val2 != null && extraLinearSpeed > 0f)
		{
			SkillObject relevantSkill = currentUsageItem.RelevantSkill;
			CharacterObject val4 = (CharacterObject)(object)((attackerCaptainCharacter is CharacterObject) ? attackerCaptainCharacter : null);
			if (doesAttackerHaveMountAgent)
			{
				PerkHelper.AddPerkBonusFromCaptain(Riding.NomadicTraditions, val4, ref val3);
			}
			else
			{
				if (relevantSkill == DefaultSkills.TwoHanded)
				{
					PerkHelper.AddPerkBonusForCharacter(TwoHanded.RecklessCharge, val2, true, ref val3, false);
				}
				PerkHelper.AddPerkBonusForCharacter(Roguery.DashAndSlash, val2, true, ref val3, false);
				PerkHelper.AddPerkBonusForCharacter(Athletics.SurgingBlow, val2, true, ref val3, false);
				PerkHelper.AddPerkBonusFromCaptain(Athletics.SurgingBlow, val4, ref val3);
			}
			if (relevantSkill == DefaultSkills.Polearm)
			{
				PerkHelper.AddPerkBonusFromCaptain(Polearm.Lancer, val4, ref val3);
				if (doesAttackerHaveMountAgent)
				{
					PerkHelper.AddPerkBonusForCharacter(Polearm.Lancer, val2, true, ref val3, false);
					PerkHelper.AddPerkBonusFromCaptain(Polearm.UnstoppableForce, val4, ref val3);
				}
			}
		}
		float num = CombatStatCalculator.CalculateStrikeMagnitudeForThrust(thrustWeaponSpeed, weight, ((ExplainedNumber)(ref val3)).ResultNumber, isThrown);
		if (item.IsCraftedByPlayer)
		{
			ExplainedNumber val5 = default(ExplainedNumber);
			((ExplainedNumber)(ref val5))._002Ector(num, false, (TextObject)null);
			PerkHelper.AddPerkBonusForCharacter(Crafting.SharpenedTip, val2, true, ref val5, false);
			num = ((ExplainedNumber)(ref val5)).ResultNumber;
		}
		return num;
	}

	public override float ComputeRawDamage(DamageTypes damageType, float magnitude, float armorEffectiveness, float absorbedDamageRatio)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected I4, but got Unknown
		float bluntDamageFactorByDamageType = ((StrikeMagnitudeCalculationModel)this).GetBluntDamageFactorByDamageType(damageType);
		float num = 50f / (50f + armorEffectiveness);
		float num2 = magnitude * num;
		float num3 = bluntDamageFactorByDamageType * num2;
		float num4;
		switch ((int)damageType)
		{
		case 0:
			num4 = MathF.Max(0f, num2 - armorEffectiveness * 0.5f);
			break;
		case 1:
			num4 = MathF.Max(0f, num2 - armorEffectiveness * 0.33f);
			break;
		case 2:
			num4 = MathF.Max(0f, num2 - armorEffectiveness * 0.2f);
			break;
		default:
			Debug.FailedAssert("Given damage type is invalid.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\GameComponents\\SandboxStrikeMagnitudeModel.cs", "ComputeRawDamage", 259);
			return 0f;
		}
		num3 += (1f - bluntDamageFactorByDamageType) * num4;
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
			result = 0.6f;
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

	public override float CalculateAdjustedArmorForBlow(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseArmor, BasicCharacterObject attackerCharacter, BasicCharacterObject attackerCaptainCharacter, BasicCharacterObject victimCharacter, BasicCharacterObject victimCaptainCharacter, WeaponComponentData weaponComponent)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		float num = baseArmor;
		CharacterObject val = (CharacterObject)(object)((attackerCharacter is CharacterObject) ? attackerCharacter : null);
		CharacterObject val2 = (CharacterObject)(object)((attackerCaptainCharacter is CharacterObject) ? attackerCaptainCharacter : null);
		if ((object)attackerCharacter == val2)
		{
			val2 = null;
		}
		if (num > 0f && val != null)
		{
			if (weaponComponent != null)
			{
				if (weaponComponent.RelevantSkill == DefaultSkills.Crossbow && baseArmor < Crossbow.Piercer.PrimaryBonus && val.GetPerkValue(Crossbow.Piercer))
				{
					flag = true;
				}
				else if ((int)weaponComponent.WeaponClass == 14)
				{
					AttackCollisionData val3 = collisionData;
					if ((int)((AttackCollisionData)(ref val3)).VictimHitBodyPart == 0 && val.GetPerkValue(Throwing.SlingingCompetitions))
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				num = 0f;
			}
			else
			{
				ExplainedNumber val4 = default(ExplainedNumber);
				((ExplainedNumber)(ref val4))._002Ector(baseArmor, false, (TextObject)null);
				PerkHelper.AddPerkBonusForCharacter(TwoHanded.Vandal, val, true, ref val4, false);
				if (weaponComponent != null)
				{
					if (weaponComponent.RelevantSkill == DefaultSkills.OneHanded)
					{
						PerkHelper.AddPerkBonusForCharacter(OneHanded.ChinkInTheArmor, val, true, ref val4, false);
					}
					else if (weaponComponent.RelevantSkill == DefaultSkills.Bow)
					{
						PerkHelper.AddPerkBonusForCharacter(Bow.Bodkin, val, true, ref val4, false);
						if (val2 != null)
						{
							PerkHelper.AddPerkBonusFromCaptain(Bow.Bodkin, val2, ref val4);
						}
					}
					else if (weaponComponent.RelevantSkill == DefaultSkills.Crossbow)
					{
						PerkHelper.AddPerkBonusForCharacter(Crossbow.Puncture, val, true, ref val4, false);
						if (val2 != null)
						{
							PerkHelper.AddPerkBonusFromCaptain(Crossbow.Puncture, val2, ref val4);
						}
					}
					else if (weaponComponent.RelevantSkill == DefaultSkills.Throwing)
					{
						PerkHelper.AddPerkBonusForCharacter(Throwing.WeakSpot, val, true, ref val4, false);
						if (val2 != null)
						{
							PerkHelper.AddPerkBonusFromCaptain(Throwing.WeakSpot, val2, ref val4);
						}
					}
				}
				float num2 = ((ExplainedNumber)(ref val4)).ResultNumber - baseArmor;
				num = MathF.Max(0f, baseArmor - num2);
				if (weaponComponent != null)
				{
					if (weaponComponent.RelevantSkill == DefaultSkills.Bow)
					{
						num *= 1f - attackInformation.AttackerAgent.AgentDrivenProperties.ArmorPenetrationMultiplierBow;
					}
					else if (weaponComponent.RelevantSkill == DefaultSkills.Crossbow)
					{
						num *= 1f - attackInformation.AttackerAgent.AgentDrivenProperties.ArmorPenetrationMultiplierCrossbow;
					}
				}
			}
		}
		return num;
	}
}
