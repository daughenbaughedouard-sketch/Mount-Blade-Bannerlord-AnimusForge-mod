using MBHelpers;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace NavalDLC.GameComponents;

public class NavalDLCCustomAgentApplyDamageModel : AgentApplyDamageModel
{
	private const float SallyOutSiegeEngineDamageMultiplier = 4.5f;

	public override bool IsDamageIgnored(in AttackInformation attackInformation, in AttackCollisionData collisionData)
	{
		return false;
	}

	public override float ApplyDamageAmplifications(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		BasicCharacterObject obj = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerRiderAgentCharacter : attackInformation.AttackerAgentCharacter);
		Formation attackerFormation = attackInformation.AttackerFormation;
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(attackerFormation);
		_ = attackInformation.IsVictimAgentMount;
		Formation victimFormation = attackInformation.VictimFormation;
		BannerComponent activeBanner2 = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
		FactoredNumber val = default(FactoredNumber);
		((FactoredNumber)(ref val))._002Ector(baseDamage);
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref attackerWeapon)).CurrentUsageItem;
		if (obj != null)
		{
			if (currentUsageItem != null)
			{
				if (currentUsageItem.IsMeleeWeapon)
				{
					if (activeBanner != null)
					{
						BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMeleeDamage, activeBanner, ref val);
						if (attackInformation.DoesVictimHaveMountAgent)
						{
							BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMeleeDamageAgainstMountedTroops, activeBanner, ref val);
						}
					}
				}
				else if (currentUsageItem.IsConsumable && activeBanner != null)
				{
					BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedRangedDamage, activeBanner, ref val);
				}
			}
			AttackCollisionData val2 = collisionData;
			if (((AttackCollisionData)(ref val2)).IsHorseCharge)
			{
				if (activeBanner != null)
				{
					BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedChargeDamage, activeBanner, ref val);
				}
				if (activeBanner2 != null)
				{
					BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedChargeDamage, activeBanner2, ref val);
				}
			}
		}
		return ((FactoredNumber)(ref val)).ResultNumber;
	}

	public override float ApplyDamageScaling(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		if (Mission.Current.IsSallyOutBattle)
		{
			DestructableComponent hitObjectDestructibleComponent = attackInformation.HitObjectDestructibleComponent;
			if (hitObjectDestructibleComponent != null)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)hitObjectDestructibleComponent).GameEntity;
				if (((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<SiegeWeapon>() != null)
				{
					num *= 4.5f;
				}
			}
		}
		return baseDamage * num;
	}

	public override float ApplyDamageReductions(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Invalid comparison between Unknown and I4
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Invalid comparison between Unknown and I4
		Agent val = (attackInformation.IsVictimAgentMount ? attackInformation.VictimAgent.RiderAgent : attackInformation.VictimAgent);
		BasicCharacterObject obj = (attackInformation.IsVictimAgentMount ? attackInformation.VictimRiderAgentCharacter : attackInformation.VictimAgentCharacter);
		Formation victimFormation = attackInformation.VictimFormation;
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
		Agent val2 = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerAgent.RiderAgent : attackInformation.AttackerAgent);
		FactoredNumber val3 = default(FactoredNumber);
		((FactoredNumber)(ref val3))._002Ector(baseDamage);
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref attackerWeapon)).CurrentUsageItem;
		if (obj != null && currentUsageItem != null)
		{
			if (currentUsageItem.IsConsumable)
			{
				if (activeBanner != null)
				{
					BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedRangedAttackDamage, activeBanner, ref val3);
				}
				if (Mission.Current.IsNavalBattle)
				{
					if (val == Agent.Main && val.CurrentlyUsedGameObject != null && val.CurrentlyUsedGameObject.GetComponent<UserDamageCalculateComponent>() != null)
					{
						UserDamageCalculateComponent component = val.CurrentlyUsedGameObject.GetComponent<UserDamageCalculateComponent>();
						((FactoredNumber)(ref val3)).AddFactor(component.DamageReductionFactor);
					}
					if (val2 != null && val2.IsAIControlled && ((int)currentUsageItem.WeaponClass == 13 || (int)currentUsageItem.WeaponClass == 12))
					{
						((FactoredNumber)(ref val3)).AddFactor(-0.2f);
					}
				}
			}
			else if (currentUsageItem.IsMeleeWeapon && activeBanner != null)
			{
				BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedMeleeAttackDamage, activeBanner, ref val3);
			}
		}
		return ((FactoredNumber)(ref val3)).ResultNumber;
	}

	public override float ApplyGeneralDamageModifiers(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		return baseDamage;
	}

	public override void DecideMissileWeaponFlags(Agent attackerAgent, in MissionWeapon missileWeapon, ref WeaponFlags missileWeaponFlags)
	{
	}

	public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy, UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		EquipmentIndex val = attackerAgent.GetOffhandWieldedItemIndex();
		if ((int)val == -1)
		{
			val = attackerAgent.GetPrimaryWieldedItemIndex();
		}
		object obj;
		if ((int)val == -1)
		{
			obj = null;
		}
		else
		{
			MissionWeapon val2 = attackerAgent.Equipment[val];
			obj = ((MissionWeapon)(ref val2)).CurrentUsageItem;
		}
		WeaponComponentData val3 = (WeaponComponentData)obj;
		if (val3 == null || isPassiveUsage || !Extensions.HasAnyFlag<WeaponFlags>(val3.WeaponFlags, (WeaponFlags)134217728) || (int)strikeType != 0 || (int)attackDirection != 0)
		{
			return false;
		}
		float num = 58f;
		if (defendItem != null && defendItem.IsShield)
		{
			num *= 1.2f;
		}
		return totalAttackEnergy > num;
	}

	public override bool CanWeaponDealSneakAttack(in AttackInformation attackInformation, WeaponComponentData weapon)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (weapon != null && (weapon.IsMeleeWeapon || (int)weapon.WeaponClass == 22) && attackInformation.IsVictimAgentHuman && !attackInformation.IsVictimPlayer)
		{
			if ((attackInformation.VictimAgentAIStateFlags & 3) == 0)
			{
				return true;
			}
			if (!Extensions.HasAllFlags<AIStateFlag>(attackInformation.VictimAgentAIStateFlags, (AIStateFlag)3) && !attackInformation.IsAttackerAgentNull)
			{
				Vec3 val = attackInformation.AttackerAgentPosition - attackInformation.VictimAgentPosition;
				Vec2 asVec = ((Vec3)(ref val)).AsVec2;
				if (Vec2.DotProduct(((Vec2)(ref asVec)).Normalized(), attackInformation.VictimAgentMovementDirection) < 0.174f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool CanWeaponDismount(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected I4, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!MBMath.IsBetween((int)blow.VictimBodyPart, 0, 6))
		{
			return false;
		}
		if (!attackerAgent.HasMount && (int)blow.StrikeType == 0 && Extensions.HasAnyFlag<WeaponFlags>(blow.WeaponRecord.WeaponFlags, (WeaponFlags)33554432))
		{
			return true;
		}
		if ((int)blow.StrikeType == 1)
		{
			return Extensions.HasAnyFlag<WeaponFlags>(blow.WeaponRecord.WeaponFlags, (WeaponFlags)16777216);
		}
		return false;
	}

	public override void CalculateDefendedBlowStunMultipliers(Agent attackerAgent, Agent defenderAgent, CombatCollisionResult collisionResult, WeaponComponentData attackerWeapon, WeaponComponentData defenderWeapon, ref float attackerStunPeriod, ref float defenderStunPeriod)
	{
	}

	public override bool CanWeaponKnockback(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		AttackCollisionData val = collisionData;
		if (MBMath.IsBetween((int)((AttackCollisionData)(ref val)).VictimHitBodyPart, 0, 6) && !Extensions.HasAnyFlag<WeaponFlags>(attackerWeapon.WeaponFlags, (WeaponFlags)67108864))
		{
			if (!attackerWeapon.IsConsumable && (blow.BlowFlag & 0x80) == 0)
			{
				if ((int)blow.StrikeType == 1)
				{
					return Extensions.HasAnyFlag<WeaponFlags>(blow.WeaponRecord.WeaponFlags, (WeaponFlags)64);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool CanWeaponKnockDown(Agent attackerAgent, Agent victimAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected I4, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)attackerWeapon.WeaponClass == 20)
		{
			return true;
		}
		AttackCollisionData val = collisionData;
		BoneBodyPartType victimHitBodyPart = ((AttackCollisionData)(ref val)).VictimHitBodyPart;
		bool flag = MBMath.IsBetween((int)victimHitBodyPart, 0, 6);
		if (!victimAgent.HasMount && (int)victimHitBodyPart == 8)
		{
			flag = true;
		}
		if (flag && Extensions.HasAnyFlag<WeaponFlags>(blow.WeaponRecord.WeaponFlags, (WeaponFlags)67108864))
		{
			if (!attackerWeapon.IsPolearm || (int)blow.StrikeType != 1)
			{
				if (attackerWeapon.IsMeleeWeapon && (int)blow.StrikeType == 0)
				{
					return MissionCombatMechanicsHelper.DecideSweetSpotCollision(ref collisionData);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override float GetDismountPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if ((int)blow.StrikeType == 0 && Extensions.HasAnyFlag<WeaponFlags>(blow.WeaponRecord.WeaponFlags, (WeaponFlags)33554432))
		{
			num += 0.25f;
		}
		return num;
	}

	public override float GetKnockBackPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		return 0f;
	}

	public override float GetKnockDownPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if ((int)attackerWeapon.WeaponClass == 20)
		{
			num += 0.25f;
		}
		else if (attackerWeapon.IsMeleeWeapon)
		{
			AttackCollisionData val = attackCollisionData;
			if ((int)((AttackCollisionData)(ref val)).VictimHitBodyPart == 8 && (int)blow.StrikeType == 0)
			{
				num += 0.1f;
			}
			else
			{
				val = attackCollisionData;
				if ((int)((AttackCollisionData)(ref val)).VictimHitBodyPart == 0)
				{
					num += 0.15f;
				}
			}
		}
		return num;
	}

	public override float GetHorseChargePenetration()
	{
		return 0.4f;
	}

	public override float CalculateStaggerThresholdDamage(Agent defenderAgent, in Blow blow)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ManagedParametersEnum val = (((int)blow.DamageType == 0) ? ((ManagedParametersEnum)10) : (((int)blow.DamageType != 1) ? ((ManagedParametersEnum)11) : ((ManagedParametersEnum)9)));
		return ManagedParameters.Instance.GetManagedParameter(val);
	}

	public override float CalculateAlternativeAttackDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData, WeaponComponentData weapon)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if (weapon == null)
		{
			return 2f;
		}
		if ((int)weapon.WeaponClass == 29)
		{
			return 2f;
		}
		if ((int)weapon.WeaponClass == 28)
		{
			return 1f;
		}
		if (weapon.IsTwoHanded)
		{
			return 2f;
		}
		return 1f;
	}

	public override float CalculatePassiveAttackDamage(BasicCharacterObject attackerCharacter, in AttackCollisionData collisionData, float baseDamage)
	{
		return baseDamage;
	}

	public override MeleeCollisionReaction DecidePassiveAttackCollisionReaction(Agent attacker, Agent defender, bool isFatalHit)
	{
		return (MeleeCollisionReaction)3;
	}

	public override float CalculateShieldDamage(in AttackInformation attackInformation, float baseDamage)
	{
		baseDamage *= 1.25f;
		FactoredNumber val = default(FactoredNumber);
		((FactoredNumber)(ref val))._002Ector(baseDamage);
		Formation victimFormation = attackInformation.VictimFormation;
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
		if (activeBanner != null)
		{
			BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedShieldDamage, activeBanner, ref val);
		}
		return MathF.Max(0f, ((FactoredNumber)(ref val)).ResultNumber);
	}

	public override float CalculateSailFireDamage(Agent attackerAgent, IShipOrigin shipOrigin, float baseDamage, bool damageFromShipMachine)
	{
		return baseDamage;
	}

	public override float CalculateHullFireDamage(float baseFireDamage, IShipOrigin shipOrigin)
	{
		return baseFireDamage;
	}

	public override float GetDamageMultiplierForBodyPart(BoneBodyPartType bodyPart, DamageTypes type, bool isHuman, bool isMissile)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected I4, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected I4, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected I4, but got Unknown
		float result = 1f;
		switch (bodyPart - -1)
		{
		case 0:
			result = 1f;
			break;
		case 1:
			switch (type - -1)
			{
			case 0:
				result = 1.5f;
				break;
			case 1:
				result = 1.2f;
				break;
			case 2:
				result = ((!isHuman) ? 1.2f : (isMissile ? 2f : 1.25f));
				break;
			case 3:
				result = 1.2f;
				break;
			}
			break;
		case 2:
			switch (type - -1)
			{
			case 0:
				result = 1.5f;
				break;
			case 1:
				result = 1.2f;
				break;
			case 2:
				result = ((!isHuman) ? 1.2f : (isMissile ? 2f : 1.25f));
				break;
			case 3:
				result = 1.2f;
				break;
			}
			break;
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
			result = ((!isHuman) ? 0.8f : 1f);
			break;
		case 9:
			result = 0.8f;
			break;
		}
		return result;
	}

	public override bool CanWeaponIgnoreFriendlyFireChecks(WeaponComponentData weapon)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (weapon != null && weapon.IsConsumable && Extensions.HasAnyFlag<WeaponFlags>(weapon.WeaponFlags, (WeaponFlags)131072) && Extensions.HasAnyFlag<WeaponFlags>(weapon.WeaponFlags, (WeaponFlags)1073741824))
		{
			return true;
		}
		return false;
	}

	public override bool DecideAgentShrugOffBlow(Agent victimAgent, in AttackCollisionData collisionData, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideAgentShrugOffBlow(victimAgent, ref collisionData, ref blow);
	}

	public override bool DecideAgentDismountedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideAgentDismountedByBlow(attackerAgent, victimAgent, ref collisionData, attackerWeapon, ref blow);
	}

	public override bool DecideAgentKnockedBackByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideAgentKnockedBackByBlow(attackerAgent, victimAgent, ref collisionData, attackerWeapon, ref blow);
	}

	public override bool DecideAgentKnockedDownByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideAgentKnockedDownByBlow(attackerAgent, victimAgent, ref collisionData, attackerWeapon, ref blow);
	}

	public override bool DecideMountRearedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return MissionCombatMechanicsHelper.DecideMountRearedByBlow(attackerAgent, victimAgent, ref collisionData, attackerWeapon, ref blow);
	}

	public override void DecideWeaponCollisionReaction(in Blow registeredBlow, in AttackCollisionData collisionData, Agent attacker, Agent defender, in MissionWeapon attackerWeapon, bool isFatalHit, bool isShruggedOff, float momentumRemaining, out MeleeCollisionReaction colReaction)
	{
		MissionCombatMechanicsHelper.DecideWeaponCollisionReaction(ref registeredBlow, ref collisionData, attacker, defender, ref attackerWeapon, isFatalHit, isShruggedOff, momentumRemaining, ref colReaction);
	}

	public override bool ShouldMissilePassThroughAfterShieldBreak(Agent attackerAgent, WeaponComponentData attackerWeapon)
	{
		return false;
	}

	public override float CalculateRemainingMomentum(float originalMomentum, in Blow b, in AttackCollisionData collisionData, Agent attacker, Agent victim, in MissionWeapon attackerWeapon, bool isCrushThrough)
	{
		return ((AgentApplyDamageModel)this).CalculateDefaultRemainingMomentum(originalMomentum, ref b, ref collisionData, attacker, victim, ref attackerWeapon, isCrushThrough);
	}

	private UsableMachine GetUsableMachineFromUsableMissionObject(UsableMissionObject usableMissionObject)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		StandingPoint val;
		if ((val = (StandingPoint)(object)((usableMissionObject is StandingPoint) ? usableMissionObject : null)) != null)
		{
			WeakGameEntity val2 = ((ScriptComponentBehavior)val).GameEntity;
			while (val2 != (GameEntity)null && !((WeakGameEntity)(ref val2)).HasScriptOfType<UsableMachine>())
			{
				val2 = ((WeakGameEntity)(ref val2)).Parent;
			}
			if (val2 != (GameEntity)null)
			{
				UsableMachine firstScriptOfType = ((WeakGameEntity)(ref val2)).GetFirstScriptOfType<UsableMachine>();
				if (firstScriptOfType != null)
				{
					return firstScriptOfType;
				}
			}
		}
		return null;
	}
}
