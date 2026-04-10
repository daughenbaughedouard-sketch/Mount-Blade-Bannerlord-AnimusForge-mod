using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace NavalDLC.GameComponents;

public class NavalAgentApplyDamageModel : AgentApplyDamageModel
{
	private const float SallyOutSiegeEngineDamageMultiplier = 4.5f;

	private NavalShipsLogic GetNavalShipsLogic()
	{
		return Mission.Current.GetMissionBehavior<NavalShipsLogic>();
	}

	public override bool IsDamageIgnored(in AttackInformation attackInformation, in AttackCollisionData collisionData)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.IsDamageIgnored(ref attackInformation, ref collisionData);
	}

	public override float ApplyDamageAmplifications(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Invalid comparison between Unknown and I4
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Invalid comparison between Unknown and I4
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Invalid comparison between Unknown and I4
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Invalid comparison between Unknown and I4
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Invalid comparison between Unknown and I4
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Invalid comparison between Unknown and I4
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Invalid comparison between Unknown and I4
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Invalid comparison between Unknown and I4
		float num = ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.ApplyDamageAmplifications(ref attackInformation, ref collisionData, baseDamage);
		bool isNavalBattle = Mission.Current.IsNavalBattle;
		Agent val = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerAgent.RiderAgent : attackInformation.AttackerAgent);
		BasicCharacterObject obj = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerRiderAgentCharacter : attackInformation.AttackerAgentCharacter);
		CharacterObject val2 = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		BasicCharacterObject attackerCaptainCharacter = attackInformation.AttackerCaptainCharacter;
		CharacterObject val3 = (CharacterObject)(object)((attackerCaptainCharacter is CharacterObject) ? attackerCaptainCharacter : null);
		Agent agent = (attackInformation.IsVictimAgentMount ? attackInformation.AttackerAgent.RiderAgent : attackInformation.VictimAgent);
		_ = attackInformation.IsVictimAgentMount;
		BasicCharacterObject victimCaptainCharacter = attackInformation.VictimCaptainCharacter;
		CharacterObject val4 = (CharacterObject)(object)((victimCaptainCharacter is CharacterObject) ? victimCaptainCharacter : null);
		AttackCollisionData val5 = collisionData;
		int num2;
		if (!((AttackCollisionData)(ref val5)).AttackBlockedWithShield)
		{
			val5 = collisionData;
			num2 = (((AttackCollisionData)(ref val5)).CollidedWithShieldOnBack ? 1 : 0);
		}
		else
		{
			num2 = 1;
		}
		bool flag = (byte)num2 != 0;
		ExplainedNumber val6 = default(ExplainedNumber);
		((ExplainedNumber)(ref val6))._002Ector(num, false, (TextObject)null);
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref attackerWeapon)).CurrentUsageItem;
		if (val2 != null)
		{
			if (currentUsageItem != null)
			{
				if (currentUsageItem.IsMeleeWeapon)
				{
					if (Mission.Current.IsNavalBattle)
					{
						if (currentUsageItem.RelevantSkill == DefaultSkills.OneHanded)
						{
							PerkHelper.AddPerkBonusForCharacter(NavalPerks.Shipmaster.TheCorsairsEdge, val2, true, ref val6, false);
						}
						if ((int)currentUsageItem.WeaponClass == 4 || (int)currentUsageItem.WeaponClass == 5)
						{
							PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.AxeOfTheNorthwind, val2, true, ref val6, false);
						}
						if ((int)currentUsageItem.WeaponClass == 2 || (int)currentUsageItem.WeaponClass == 3)
						{
							PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.SunnyDisposition, val2, true, ref val6, false);
						}
						if ((int)currentUsageItem.WeaponClass == 5 || (int)currentUsageItem.WeaponClass == 8 || (int)currentUsageItem.WeaponClass == 10 || (int)currentUsageItem.WeaponClass == 3)
						{
							PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.MightyBlows, val3, ref val6);
						}
						if (currentUsageItem.IsMeleeWeapon)
						{
							PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.WarriorsMight, val2, true, ref val6, false);
						}
					}
				}
				else if (currentUsageItem.IsConsumable)
				{
					if (currentUsageItem.RelevantSkill == DefaultSkills.Bow)
					{
						val5 = collisionData;
						if (((AttackCollisionData)(ref val5)).CollisionBoneIndex != -1)
						{
							if (isNavalBattle)
							{
								PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.TheSkysFury, val3, ref val6);
							}
							goto IL_0251;
						}
					}
					if (currentUsageItem.RelevantSkill == DefaultSkills.Crossbow)
					{
						val5 = collisionData;
						if (((AttackCollisionData)(ref val5)).CollisionBoneIndex != -1)
						{
							if (isNavalBattle)
							{
								PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.TheSkysFury, val3, ref val6);
							}
							goto IL_0251;
						}
					}
					if (currentUsageItem.RelevantSkill == DefaultSkills.Throwing && isNavalBattle)
					{
						PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.CrewOfSpears, val3, ref val6);
						PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.WarriorsMight, val3, ref val6);
					}
					goto IL_0251;
				}
			}
			goto IL_02b7;
		}
		goto IL_0364;
		IL_02b7:
		if ((currentUsageItem == null || currentUsageItem.IsMeleeWeapon) && Mission.Current.IsNavalBattle)
		{
			if (IsAgentOnEnemyShip(val))
			{
				_ = val.Name == "Itsul Ironeye";
				PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.BoardingMaster, val2, true, ref val6, false);
				PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.BoardingMaster, val3, ref val6);
			}
			else if (IsAgentOnOwnShip(val))
			{
				PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.HomeTurfAdvantage, val2, true, ref val6, false);
				PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.HomeTurfAdvantage, val3, ref val6);
			}
		}
		val5 = collisionData;
		if (((AttackCollisionData)(ref val5)).IsAlternativeAttack)
		{
			PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.BruteForce, val2, true, ref val6, false);
		}
		if (flag && isNavalBattle)
		{
			PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.Forceful, val3, ref val6);
		}
		goto IL_0364;
		IL_0251:
		if (isNavalBattle && (currentUsageItem.RelevantSkill == DefaultSkills.Bow || currentUsageItem.RelevantSkill == DefaultSkills.Crossbow || currentUsageItem.RelevantSkill == DefaultSkills.Throwing))
		{
			if (flag)
			{
				PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Boatswain.AccuracyTraining, val3, ref val6);
			}
			if (!IsAgentCrewBoarded(agent))
			{
				PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Shipmaster.SeaborneFortress, val4, ref val6);
			}
			PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.TheSkysFury, val2, true, ref val6, false);
		}
		goto IL_02b7;
		IL_0364:
		return ((ExplainedNumber)(ref val6)).ResultNumber;
	}

	public override float ApplyDamageScaling(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.ApplyDamageScaling(ref attackInformation, ref collisionData, baseDamage);
	}

	public override float ApplyDamageReductions(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Invalid comparison between Unknown and I4
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Invalid comparison between Unknown and I4
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Invalid comparison between Unknown and I4
		float num = ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.ApplyDamageReductions(ref attackInformation, ref collisionData, baseDamage);
		bool isNavalBattle = Mission.Current.IsNavalBattle;
		_ = attackInformation.IsAttackerAgentMount;
		Agent val = (attackInformation.IsVictimAgentMount ? attackInformation.VictimAgent.RiderAgent : attackInformation.VictimAgent);
		Agent val2 = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerAgent.RiderAgent : attackInformation.AttackerAgent);
		BasicCharacterObject obj = (attackInformation.IsVictimAgentMount ? attackInformation.VictimRiderAgentCharacter : attackInformation.VictimAgentCharacter);
		CharacterObject val3 = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		BasicCharacterObject victimCaptainCharacter = attackInformation.VictimCaptainCharacter;
		CharacterObject val4 = (CharacterObject)(object)((victimCaptainCharacter is CharacterObject) ? victimCaptainCharacter : null);
		ExplainedNumber damageResult = default(ExplainedNumber);
		((ExplainedNumber)(ref damageResult))._002Ector(num, false, (TextObject)null);
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref attackerWeapon)).CurrentUsageItem;
		if (val3 != null && currentUsageItem != null)
		{
			if (currentUsageItem.IsConsumable)
			{
				if (isNavalBattle)
				{
					if (val.CurrentlyUsedGameObject != null && val.CurrentlyUsedGameObject.GetComponent<UserDamageCalculateComponent>() != null)
					{
						UserDamageCalculateComponent component = val.CurrentlyUsedGameObject.GetComponent<UserDamageCalculateComponent>();
						component.ApplyPerkBonusForCharacter(NavalPerks.Shipmaster.TheHelmsmansShield, isPrimaryBonus: true, val3, ref damageResult);
						if (val == Agent.Main)
						{
							((ExplainedNumber)(ref damageResult)).AddFactor(component.DamageReductionFactor, (TextObject)null);
							if ((int)currentUsageItem.WeaponClass == 26 && NavalStorylineData.GetNavalStorylineSetPieceBattleMissionType() == NavalStorylineData.NavalStorylineSetPieceBattleMissionTypes.Act3Quest4)
							{
								((ExplainedNumber)(ref damageResult)).AddFactor(-0.9f, (TextObject)null);
							}
						}
					}
					if (val2 != null && val2.IsAIControlled && ((int)currentUsageItem.WeaponClass == 13 || (int)currentUsageItem.WeaponClass == 12))
					{
						((ExplainedNumber)(ref damageResult)).AddFactor(-0.15f, (TextObject)null);
					}
				}
			}
			else if (currentUsageItem.IsMeleeWeapon)
			{
				if (Mission.Current.IsNavalBattle && IsAgentOnEnemyShip(val))
				{
					PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.TerrorOfTheSeas, val4, ref damageResult);
				}
				else if (Mission.Current.IsNavalBattle && IsAgentOnOwnShip(val) && val4 != null && val4.GetPerkValue(NavalPerks.Mariner.RallyingCry))
				{
					((ExplainedNumber)(ref damageResult)).AddFactor(NavalPerks.Mariner.RallyingCry.SecondaryBonus, (TextObject)null);
				}
			}
		}
		return ((ExplainedNumber)(ref damageResult)).ResultNumber;
	}

	public override float ApplyGeneralDamageModifiers(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.ApplyGeneralDamageModifiers(ref attackInformation, ref collisionData, baseDamage);
	}

	public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy, UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsage)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.DecideCrushedThrough(attackerAgent, defenderAgent, totalAttackEnergy, attackDirection, strikeType, defendItem, isPassiveUsage);
	}

	public override void DecideMissileWeaponFlags(Agent attackerAgent, in MissionWeapon missileWeapon, ref WeaponFlags missileWeaponFlags)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		((MBGameModel<AgentApplyDamageModel>)this).BaseModel.DecideMissileWeaponFlags(attackerAgent, ref missileWeapon, ref missileWeaponFlags);
		BasicCharacterObject obj = ((attackerAgent != null) ? attackerAgent.Character : null);
		CharacterObject val = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		if (val != null)
		{
			MissionWeapon val2 = missileWeapon;
			if ((int)((MissionWeapon)(ref val2)).CurrentUsageItem.WeaponClass == 23 && Mission.Current.IsNavalBattle && val.GetPerkValue(NavalPerks.Mariner.CrewOfSpears))
			{
				missileWeaponFlags = (WeaponFlags)((ulong)missileWeaponFlags | 0x20000uL);
			}
		}
	}

	public override bool CanWeaponIgnoreFriendlyFireChecks(WeaponComponentData weapon)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CanWeaponIgnoreFriendlyFireChecks(weapon);
	}

	public override bool CanWeaponDealSneakAttack(in AttackInformation attackInformation, WeaponComponentData weapon)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CanWeaponDealSneakAttack(ref attackInformation, weapon);
	}

	public override bool CanWeaponDismount(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CanWeaponDismount(attackerAgent, attackerWeapon, ref blow, ref collisionData);
	}

	public override void CalculateDefendedBlowStunMultipliers(Agent attackerAgent, Agent defenderAgent, CombatCollisionResult collisionResult, WeaponComponentData attackerWeapon, WeaponComponentData defenderWeapon, ref float attackerStunPeriod, ref float defenderStunPeriod)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CalculateDefendedBlowStunMultipliers(attackerAgent, defenderAgent, collisionResult, attackerWeapon, defenderWeapon, ref attackerStunPeriod, ref defenderStunPeriod);
	}

	public override bool CanWeaponKnockback(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CanWeaponKnockback(attackerAgent, attackerWeapon, ref blow, ref collisionData);
	}

	public override bool CanWeaponKnockDown(Agent attackerAgent, Agent victimAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CanWeaponKnockDown(attackerAgent, victimAgent, attackerWeapon, ref blow, ref collisionData);
	}

	public override float GetDismountPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.GetDismountPenetration(attackerAgent, attackerWeapon, ref blow, ref collisionData);
	}

	public override float GetKnockBackPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		float num = ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.GetKnockBackPenetration(attackerAgent, attackerWeapon, ref blow, ref collisionData);
		if (Mission.Current.IsNavalBattle && attackerWeapon != null && attackerAgent.Formation != null)
		{
			Agent captain = attackerAgent.Formation.Captain;
			CharacterObject val;
			if (captain != null && attackerAgent != captain && (val = (CharacterObject)/*isinst with value type is only supported in some contexts*/) != null && val.GetPerkValue(NavalPerks.Mariner.BruteForce))
			{
				num += NavalPerks.Mariner.BruteForce.SecondaryBonus;
			}
		}
		return num;
	}

	public override float GetKnockDownPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.GetKnockDownPenetration(attackerAgent, attackerWeapon, ref blow, ref collisionData);
	}

	public override float GetHorseChargePenetration()
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.GetHorseChargePenetration();
	}

	public override float CalculateStaggerThresholdDamage(Agent defenderAgent, in Blow blow)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CalculateStaggerThresholdDamage(defenderAgent, ref blow);
	}

	public override float CalculateAlternativeAttackDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData, WeaponComponentData weapon)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CalculateAlternativeAttackDamage(ref attackInformation, ref collisionData, weapon);
	}

	public override float CalculatePassiveAttackDamage(BasicCharacterObject attackerCharacter, in AttackCollisionData collisionData, float baseDamage)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CalculatePassiveAttackDamage(attackerCharacter, ref collisionData, baseDamage);
	}

	public override MeleeCollisionReaction DecidePassiveAttackCollisionReaction(Agent attacker, Agent defender, bool isFatalHit)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.DecidePassiveAttackCollisionReaction(attacker, defender, isFatalHit);
	}

	public override float CalculateShieldDamage(in AttackInformation attackInformation, float baseDamage)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CalculateShieldDamage(ref attackInformation, baseDamage);
	}

	public override float CalculateSailFireDamage(Agent agent, IShipOrigin shipOrigin, float baseDamage, bool damageFromShipMachine)
	{
		float num = ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CalculateSailFireDamage(agent, shipOrigin, baseDamage, damageFromShipMachine);
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(num, false, (TextObject)null);
		Formation formation = agent.Formation;
		object obj;
		if (formation == null)
		{
			obj = null;
		}
		else
		{
			Agent captain = formation.Captain;
			obj = ((captain != null) ? captain.Character : null);
		}
		CharacterObject val2 = (CharacterObject)((obj is CharacterObject) ? obj : null);
		if (val2 != null)
		{
			PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Mariner.EnemyOfTheWood, val2, ref val);
			if (!damageFromShipMachine)
			{
				PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Boatswain.SpecialArrows, val2, ref val);
			}
		}
		Figurehead figurehead = ((Ship)((shipOrigin is Ship) ? shipOrigin : null)).Figurehead;
		if (figurehead != null && figurehead == DefaultFigureheads.SeaSerpent)
		{
			((ExplainedNumber)(ref val)).AddFactor(0f - figurehead.EffectAmount, (TextObject)null);
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override float CalculateHullFireDamage(float baseFireDamage, IShipOrigin shipOrigin)
	{
		((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CalculateHullFireDamage(baseFireDamage, shipOrigin);
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(baseFireDamage, false, (TextObject)null);
		Figurehead figurehead = ((Ship)((shipOrigin is Ship) ? shipOrigin : null)).Figurehead;
		if (figurehead != null && figurehead == DefaultFigureheads.SeaSerpent)
		{
			((ExplainedNumber)(ref val)).AddFactor(0f - figurehead.EffectAmount, (TextObject)null);
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override float GetDamageMultiplierForBodyPart(BoneBodyPartType bodyPart, DamageTypes type, bool isHuman, bool isMissile)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.GetDamageMultiplierForBodyPart(bodyPart, type, isHuman, isMissile);
	}

	public override bool DecideAgentShrugOffBlow(Agent victimAgent, in AttackCollisionData collisionData, in Blow blow)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.DecideAgentShrugOffBlow(victimAgent, ref collisionData, ref blow);
	}

	public override bool DecideAgentDismountedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.DecideAgentDismountedByBlow(attackerAgent, victimAgent, ref collisionData, attackerWeapon, ref blow);
	}

	public override bool DecideAgentKnockedBackByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.DecideAgentKnockedBackByBlow(attackerAgent, victimAgent, ref collisionData, attackerWeapon, ref blow);
	}

	public override bool DecideAgentKnockedDownByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.DecideAgentKnockedDownByBlow(attackerAgent, victimAgent, ref collisionData, attackerWeapon, ref blow);
	}

	public override bool DecideMountRearedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
	{
		return ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.DecideMountRearedByBlow(attackerAgent, victimAgent, ref collisionData, attackerWeapon, ref blow);
	}

	public override void DecideWeaponCollisionReaction(in Blow registeredBlow, in AttackCollisionData collisionData, Agent attacker, Agent defender, in MissionWeapon attackerWeapon, bool isFatalHit, bool isShruggedOff, float momentumRemaining, out MeleeCollisionReaction colReaction)
	{
		((MBGameModel<AgentApplyDamageModel>)this).BaseModel.DecideWeaponCollisionReaction(ref registeredBlow, ref collisionData, attacker, defender, ref attackerWeapon, isFatalHit, isShruggedOff, momentumRemaining, ref colReaction);
	}

	public override bool ShouldMissilePassThroughAfterShieldBreak(Agent attackerAgent, WeaponComponentData attackerWeapon)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		bool result = ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.ShouldMissilePassThroughAfterShieldBreak(attackerAgent, attackerWeapon);
		CharacterObject val = (CharacterObject)attackerAgent.Character;
		if (val != null && Mission.Current.IsNavalBattle && attackerWeapon != null && (int)attackerWeapon.WeaponClass == 21 && val.GetPerkValue(NavalPerks.Mariner.CrewOfSpears))
		{
			return true;
		}
		return result;
	}

	public override float CalculateRemainingMomentum(float originalMomentum, in Blow b, in AttackCollisionData collisionData, Agent attacker, Agent victim, in MissionWeapon attackerWeapon, bool isCrushThrough)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		float num = ((MBGameModel<AgentApplyDamageModel>)this).BaseModel.CalculateRemainingMomentum(originalMomentum, ref b, ref collisionData, attacker, victim, ref attackerWeapon, isCrushThrough);
		CharacterObject val = (CharacterObject)attacker.Character;
		AttackCollisionData val2 = collisionData;
		if (((AttackCollisionData)(ref val2)).IsColliderAgent)
		{
			val2 = collisionData;
			if (!((AttackCollisionData)(ref val2)).IsHorseCharge && (attacker == null || !attacker.IsDoingPassiveAttack) && !MissionCombatMechanicsHelper.HitWithAnotherBone(ref collisionData, attacker, ref attackerWeapon))
			{
				MissionWeapon val3 = attackerWeapon;
				if (!((MissionWeapon)(ref val3)).IsEmpty && (int)b.StrikeType != 1)
				{
					val3 = attackerWeapon;
					if (!((MissionWeapon)(ref val3)).IsEmpty)
					{
						val3 = attackerWeapon;
						if (((MissionWeapon)(ref val3)).CurrentUsageItem.RelevantSkill == DefaultSkills.TwoHanded)
						{
							ExplainedNumber val4 = default(ExplainedNumber);
							((ExplainedNumber)(ref val4))._002Ector(0f, false, (TextObject)null);
							((ExplainedNumber)(ref val4)).LimitMin(0f);
							if ((float)b.InflictedDamage > 0f)
							{
								((ExplainedNumber)(ref val4)).Add(b.AbsorbedByArmor / (float)b.InflictedDamage, (TextObject)null, (TextObject)null);
								if (val != null)
								{
									PerkHelper.AddPerkBonusForCharacter(NavalPerks.Mariner.MightyBlows, val, true, ref val4, false);
								}
							}
							num = originalMomentum - ((ExplainedNumber)(ref val4)).ResultNumber;
							num *= 0.5f;
							if (num < 0.25f)
							{
								num = 0f;
							}
						}
					}
				}
			}
		}
		return num;
	}

	private bool IsAgentOnEnemyShip(Agent agent)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		foreach (MissionShip item in (List<MissionShip>)(object)GetNavalShipsLogic().AllShips)
		{
			if (((ScriptComponentBehavior)item).GameEntity != (GameEntity)null && item.Team != null && item.GetIsAgentOnShip(agent) && agent.Team.IsEnemyOf(item.Team))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsAgentOnOwnShip(Agent agent)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		foreach (MissionShip item in (List<MissionShip>)(object)GetNavalShipsLogic().AllShips)
		{
			if (((ScriptComponentBehavior)item).GameEntity != (GameEntity)null && item.Team != null && item.GetIsAgentOnShip(agent) && agent.Team.IsFriendOf(item.Team))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsAgentCrewBoarded(Agent agent)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		NavalShipsLogic navalShipsLogic = GetNavalShipsLogic();
		bool result = false;
		foreach (MissionShip item in (List<MissionShip>)(object)navalShipsLogic.AllShips)
		{
			if (((ScriptComponentBehavior)item).GameEntity != (GameEntity)null && item.GetIsConnectedToEnemy())
			{
				result = true;
			}
		}
		return result;
	}
}
