using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace NavalDLC.GameComponents;

public class NavalStrikeMagnitudeModel : StrikeMagnitudeCalculationModel
{
	public override float CalculateHorseArcheryFactor(BasicCharacterObject characterObject)
	{
		return ((MBGameModel<StrikeMagnitudeCalculationModel>)this).BaseModel.CalculateHorseArcheryFactor(characterObject);
	}

	public override float CalculateStrikeMagnitudeForMissile(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float missileSpeed)
	{
		return ((MBGameModel<StrikeMagnitudeCalculationModel>)this).BaseModel.CalculateStrikeMagnitudeForMissile(ref attackInformation, ref collisionData, ref weapon, missileSpeed);
	}

	public override float CalculateStrikeMagnitudeForSwing(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float swingSpeed, float impactPointAsPercent, float extraLinearSpeed)
	{
		return ((MBGameModel<StrikeMagnitudeCalculationModel>)this).BaseModel.CalculateStrikeMagnitudeForSwing(ref attackInformation, ref collisionData, ref weapon, swingSpeed, impactPointAsPercent, extraLinearSpeed);
	}

	public override float CalculateStrikeMagnitudeForUnarmedAttack(in AttackInformation attackInformation, in AttackCollisionData collisionData, float progressEffect, float momentumRemaining)
	{
		return ((MBGameModel<StrikeMagnitudeCalculationModel>)this).BaseModel.CalculateStrikeMagnitudeForUnarmedAttack(ref attackInformation, ref collisionData, progressEffect, momentumRemaining);
	}

	public override float CalculateStrikeMagnitudeForThrust(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float thrustWeaponSpeed, float extraLinearSpeed, bool isThrown = false)
	{
		return ((MBGameModel<StrikeMagnitudeCalculationModel>)this).BaseModel.CalculateStrikeMagnitudeForThrust(ref attackInformation, ref collisionData, ref weapon, thrustWeaponSpeed, extraLinearSpeed, isThrown);
	}

	public override float CalculateBaseBlowMagnitudeForPassiveUsage(in AttackInformation attackInformation, in AttackCollisionData collisionData, float extraLinearSpeed)
	{
		return ((MBGameModel<StrikeMagnitudeCalculationModel>)this).BaseModel.CalculateBaseBlowMagnitudeForPassiveUsage(ref attackInformation, ref collisionData, extraLinearSpeed);
	}

	public override float ComputeRawDamage(DamageTypes damageType, float magnitude, float armorEffectiveness, float absorbedDamageRatio)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<StrikeMagnitudeCalculationModel>)this).BaseModel.ComputeRawDamage(damageType, magnitude, armorEffectiveness, absorbedDamageRatio);
	}

	public override float GetBluntDamageFactorByDamageType(DamageTypes damageType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<StrikeMagnitudeCalculationModel>)this).BaseModel.GetBluntDamageFactorByDamageType(damageType);
	}

	public override float CalculateAdjustedArmorForBlow(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseArmor, BasicCharacterObject attackerCharacter, BasicCharacterObject attackerCaptainCharacter, BasicCharacterObject victimCharacter, BasicCharacterObject victimCaptainCharacter, WeaponComponentData weaponComponent)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Invalid comparison between Unknown and I4
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		float num = ((MBGameModel<StrikeMagnitudeCalculationModel>)this).BaseModel.CalculateAdjustedArmorForBlow(ref attackInformation, ref collisionData, baseArmor, attackerCharacter, attackerCaptainCharacter, victimCharacter, victimCaptainCharacter, weaponComponent);
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
					if (weaponComponent.IsMeleeWeapon)
					{
						PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.ShatteringBlow, val, true, ref val4, false);
						if (val2 != null)
						{
							PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.ShatteringBlow, val2, ref val4);
						}
					}
					else if (weaponComponent.IsConsumable && weaponComponent.RelevantSkill != null)
					{
						PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.ShatteringVolley, val, true, ref val4, false);
						if (val2 != null)
						{
							PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.ShatteringVolley, val2, ref val4);
						}
					}
				}
				float num2 = ((ExplainedNumber)(ref val4)).ResultNumber - baseArmor;
				num = MathF.Max(0f, baseArmor - num2);
			}
		}
		return num;
	}
}
