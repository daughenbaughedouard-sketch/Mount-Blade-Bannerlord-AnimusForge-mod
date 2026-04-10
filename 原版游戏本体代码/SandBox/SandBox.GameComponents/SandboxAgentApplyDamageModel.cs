using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace SandBox.GameComponents;

public class SandboxAgentApplyDamageModel : AgentApplyDamageModel
{
	private const float SallyOutSiegeEngineDamageMultiplier = 4.5f;

	public override bool IsDamageIgnored(in AttackInformation attackInformation, in AttackCollisionData collisionData)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		BasicCharacterObject obj = (attackInformation.IsVictimAgentMount ? attackInformation.VictimRiderAgentCharacter : attackInformation.VictimAgentCharacter);
		CharacterObject val = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref attackerWeapon)).CurrentUsageItem;
		bool result = false;
		if (currentUsageItem != null && currentUsageItem.IsConsumable)
		{
			AttackCollisionData val2 = collisionData;
			if (((AttackCollisionData)(ref val2)).CollidedWithShieldOnBack && val != null && val.GetPerkValue(Crossbow.Pavise))
			{
				float num = MBMath.ClampFloat(Crossbow.Pavise.PrimaryBonus, 0f, 1f);
				result = MBRandom.RandomFloat <= num;
			}
		}
		return result;
	}

	public override float ApplyDamageAmplifications(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0802: Unknown result type (might be due to invalid IL or missing references)
		//IL_0807: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0901: Unknown result type (might be due to invalid IL or missing references)
		//IL_0906: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Invalid comparison between Unknown and I4
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Invalid comparison between Unknown and I4
		//IL_093e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0943: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Invalid comparison between Unknown and I4
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Invalid comparison between Unknown and I4
		//IL_06cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d2: Invalid comparison between Unknown and I4
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Invalid comparison between Unknown and I4
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		Formation attackerFormation = attackInformation.AttackerFormation;
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(attackerFormation);
		Agent val = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerAgent.RiderAgent : attackInformation.AttackerAgent);
		BasicCharacterObject obj = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerRiderAgentCharacter : attackInformation.AttackerAgentCharacter);
		CharacterObject val2 = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		BasicCharacterObject attackerCaptainCharacter = attackInformation.AttackerCaptainCharacter;
		CharacterObject val3 = (CharacterObject)(object)((attackerCaptainCharacter is CharacterObject) ? attackerCaptainCharacter : null);
		bool flag = attackInformation.IsAttackerAgentHuman && !attackInformation.DoesAttackerHaveMountAgent;
		bool flag2 = attackInformation.DoesAttackerHaveMountAgent || attackInformation.DoesAttackerHaveRiderAgent;
		BasicCharacterObject obj2 = (attackInformation.IsVictimAgentMount ? attackInformation.VictimRiderAgentCharacter : attackInformation.VictimAgentCharacter);
		CharacterObject val4 = (CharacterObject)(object)((obj2 is CharacterObject) ? obj2 : null);
		bool flag3 = attackInformation.IsVictimAgentHuman && !attackInformation.DoesVictimHaveMountAgent;
		bool flag4 = attackInformation.DoesVictimHaveMountAgent || attackInformation.DoesVictimHaveRiderAgent;
		Formation victimFormation = attackInformation.VictimFormation;
		BannerComponent activeBanner2 = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
		AttackCollisionData val5 = collisionData;
		int num;
		if (!((AttackCollisionData)(ref val5)).AttackBlockedWithShield)
		{
			val5 = collisionData;
			num = (((AttackCollisionData)(ref val5)).CollidedWithShieldOnBack ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		bool flag5 = (byte)num != 0;
		ExplainedNumber val6 = default(ExplainedNumber);
		((ExplainedNumber)(ref val6))._002Ector(baseDamage, false, (TextObject)null);
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref attackerWeapon)).CurrentUsageItem;
		if (val2 != null)
		{
			if (currentUsageItem != null)
			{
				if (currentUsageItem.IsMeleeWeapon)
				{
					if (currentUsageItem.RelevantSkill == DefaultSkills.OneHanded)
					{
						PerkHelper.AddPerkBonusForCharacter(OneHanded.DeadlyPurpose, val2, true, ref val6, false);
						if (flag2)
						{
							PerkHelper.AddPerkBonusForCharacter(OneHanded.Cavalry, val2, true, ref val6, false);
						}
						MissionWeapon offHandItem = attackInformation.OffHandItem;
						if (((MissionWeapon)(ref offHandItem)).IsEmpty)
						{
							PerkHelper.AddPerkBonusForCharacter(OneHanded.Duelist, val2, true, ref val6, false);
						}
						if ((int)currentUsageItem.WeaponClass == 6 || (int)currentUsageItem.WeaponClass == 4)
						{
							PerkHelper.AddPerkBonusForCharacter(OneHanded.ToBeBlunt, val2, true, ref val6, false);
						}
						if (flag5)
						{
							PerkHelper.AddPerkBonusForCharacter(OneHanded.Prestige, val2, true, ref val6, false);
						}
						PerkHelper.AddPerkBonusFromCaptain(Roguery.Carver, val3, ref val6);
						PerkHelper.AddEpicPerkBonusForCharacter(OneHanded.WayOfTheSword, val2, DefaultSkills.OneHanded, false, ref val6, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
					}
					else if (currentUsageItem.RelevantSkill == DefaultSkills.TwoHanded)
					{
						if (flag5)
						{
							PerkHelper.AddPerkBonusForCharacter(TwoHanded.WoodChopper, val2, true, ref val6, false);
							PerkHelper.AddPerkBonusFromCaptain(TwoHanded.WoodChopper, val3, ref val6);
							PerkHelper.AddPerkBonusForCharacter(TwoHanded.ShieldBreaker, val2, true, ref val6, false);
							PerkHelper.AddPerkBonusFromCaptain(TwoHanded.ShieldBreaker, val3, ref val6);
						}
						if ((int)currentUsageItem.WeaponClass == 5 || (int)currentUsageItem.WeaponClass == 8)
						{
							PerkHelper.AddPerkBonusForCharacter(TwoHanded.HeadBasher, val2, true, ref val6, false);
						}
						if (attackInformation.IsVictimAgentMount)
						{
							PerkHelper.AddPerkBonusForCharacter(TwoHanded.BeastSlayer, val2, true, ref val6, false);
							PerkHelper.AddPerkBonusFromCaptain(TwoHanded.BeastSlayer, val3, ref val6);
						}
						if (attackInformation.AttackerHitPointRate < 0.5f)
						{
							PerkHelper.AddPerkBonusForCharacter(TwoHanded.Berserker, val2, true, ref val6, false);
						}
						else if (attackInformation.AttackerHitPointRate > 0.9f)
						{
							PerkHelper.AddPerkBonusForCharacter(TwoHanded.Confidence, val2, true, ref val6, false);
						}
						PerkHelper.AddPerkBonusForCharacter(TwoHanded.BladeMaster, val2, true, ref val6, false);
						PerkHelper.AddPerkBonusFromCaptain(Roguery.DashAndSlash, val3, ref val6);
						PerkHelper.AddEpicPerkBonusForCharacter(TwoHanded.WayOfTheGreatAxe, val2, DefaultSkills.TwoHanded, false, ref val6, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
					}
					else if (currentUsageItem.RelevantSkill == DefaultSkills.Polearm)
					{
						if (flag2)
						{
							PerkHelper.AddPerkBonusForCharacter(Polearm.Cavalry, val2, true, ref val6, false);
						}
						else
						{
							PerkHelper.AddPerkBonusForCharacter(Polearm.Pikeman, val2, true, ref val6, false);
						}
						val5 = collisionData;
						if (((AttackCollisionData)(ref val5)).StrikeType == 1)
						{
							PerkHelper.AddPerkBonusForCharacter(Polearm.CleanThrust, val2, true, ref val6, false);
							PerkHelper.AddPerkBonusForCharacter(Polearm.SharpenTheTip, val2, true, ref val6, false);
						}
						if (attackInformation.IsVictimAgentMount)
						{
							PerkHelper.AddPerkBonusForCharacter(Polearm.SteedKiller, val2, true, ref val6, false);
							if (flag)
							{
								PerkHelper.AddPerkBonusFromCaptain(Polearm.SteedKiller, val3, ref val6);
							}
						}
						if (attackInformation.IsHeadShot)
						{
							PerkHelper.AddPerkBonusForCharacter(Polearm.Guards, val2, true, ref val6, false);
						}
						PerkHelper.AddPerkBonusFromCaptain(Polearm.Phalanx, val3, ref val6);
						PerkHelper.AddEpicPerkBonusForCharacter(Polearm.WayOfTheSpear, val2, DefaultSkills.Polearm, false, ref val6, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
					}
					else if (currentUsageItem.IsShield)
					{
						PerkHelper.AddPerkBonusForCharacter(OneHanded.Basher, val2, true, ref val6, false);
					}
					PerkHelper.AddPerkBonusForCharacter(Athletics.Powerful, val2, true, ref val6, false);
					PerkHelper.AddPerkBonusFromCaptain(Athletics.Powerful, val3, ref val6);
					PerkHelper.AddPerkBonusFromCaptain(Engineering.ImprovedTools, val3, ref val6);
					if (((MissionWeapon)(ref attackerWeapon)).Item != null && (int)((MissionWeapon)(ref attackerWeapon)).Item.ItemType == 12)
					{
						PerkHelper.AddPerkBonusForCharacter(Throwing.FlexibleFighter, val2, true, ref val6, false);
					}
					if (flag2)
					{
						PerkHelper.AddPerkBonusForCharacter(Riding.MountedWarrior, val2, true, ref val6, false);
						PerkHelper.AddPerkBonusFromCaptain(Riding.MountedWarrior, val3, ref val6);
						PerkHelper.AddPerkBonusFromCaptain(OneHanded.Cavalry, val3, ref val6);
					}
					else
					{
						PerkHelper.AddPerkBonusFromCaptain(OneHanded.DeadlyPurpose, val3, ref val6);
						val5 = collisionData;
						if (((AttackCollisionData)(ref val5)).StrikeType == 1)
						{
							PerkHelper.AddPerkBonusFromCaptain(Polearm.SharpenTheTip, val3, ref val6);
						}
					}
					if (activeBanner != null)
					{
						BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMeleeDamage, activeBanner, ref val6);
						if (attackInformation.DoesVictimHaveMountAgent)
						{
							BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMeleeDamageAgainstMountedTroops, activeBanner, ref val6);
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
							PerkHelper.AddPerkBonusFromCaptain(Bow.BowControl, val3, ref val6);
							if (attackInformation.IsHeadShot)
							{
								PerkHelper.AddPerkBonusForCharacter(Bow.DeadAim, val2, true, ref val6, false);
							}
							PerkHelper.AddPerkBonusForCharacter(Bow.StrongBows, val2, true, ref val6, false);
							if (val2.Tier >= 3)
							{
								PerkHelper.AddPerkBonusFromCaptain(Bow.StrongBows, val3, ref val6);
							}
							if (attackInformation.IsVictimAgentMount)
							{
								PerkHelper.AddPerkBonusForCharacter(Bow.HunterClan, val2, true, ref val6, false);
							}
							PerkHelper.AddEpicPerkBonusForCharacter(Bow.Deadshot, val2, DefaultSkills.Bow, false, ref val6, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus, false);
							goto IL_07aa;
						}
					}
					if (currentUsageItem.RelevantSkill == DefaultSkills.Crossbow)
					{
						val5 = collisionData;
						if (((AttackCollisionData)(ref val5)).CollisionBoneIndex != -1)
						{
							PerkHelper.AddPerkBonusForCharacter(Engineering.TorsionEngines, val2, false, ref val6, false);
							if (attackInformation.IsVictimAgentMount)
							{
								PerkHelper.AddPerkBonusForCharacter(Crossbow.Unhorser, val2, true, ref val6, false);
								PerkHelper.AddPerkBonusFromCaptain(Crossbow.Unhorser, val3, ref val6);
							}
							if (attackInformation.IsHeadShot)
							{
								PerkHelper.AddPerkBonusForCharacter(Crossbow.Sheriff, val2, true, ref val6, false);
							}
							if (flag3)
							{
								PerkHelper.AddPerkBonusFromCaptain(Crossbow.Sheriff, val3, ref val6);
							}
							PerkHelper.AddPerkBonusFromCaptain(Crossbow.HammerBolts, val3, ref val6);
							PerkHelper.AddPerkBonusFromCaptain(Engineering.DreadfulSieger, val3, ref val6);
							PerkHelper.AddEpicPerkBonusForCharacter(Crossbow.MightyPull, val2, DefaultSkills.Crossbow, false, ref val6, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus, false);
							goto IL_07aa;
						}
					}
					if (currentUsageItem.RelevantSkill == DefaultSkills.Throwing)
					{
						PerkHelper.AddPerkBonusForCharacter(Athletics.StrongArms, val2, true, ref val6, false);
						if (flag5)
						{
							PerkHelper.AddPerkBonusForCharacter(Throwing.ShieldBreaker, val2, true, ref val6, false);
							PerkHelper.AddPerkBonusFromCaptain(Throwing.ShieldBreaker, val3, ref val6);
							if ((int)currentUsageItem.WeaponClass == 21)
							{
								PerkHelper.AddPerkBonusForCharacter(Throwing.Splinters, val2, true, ref val6, false);
							}
							PerkHelper.AddPerkBonusFromCaptain(Throwing.Splinters, val3, ref val6);
						}
						if (attackInformation.IsVictimAgentMount)
						{
							PerkHelper.AddPerkBonusForCharacter(Throwing.Hunter, val2, true, ref val6, false);
							PerkHelper.AddPerkBonusFromCaptain(Throwing.Hunter, val3, ref val6);
						}
						if (flag2)
						{
							PerkHelper.AddPerkBonusFromCaptain(Throwing.MountedSkirmisher, val3, ref val6);
						}
						PerkHelper.AddPerkBonusFromCaptain(Throwing.Impale, val3, ref val6);
						if (flag4)
						{
							PerkHelper.AddPerkBonusFromCaptain(Throwing.KnockOff, val3, ref val6);
						}
						if (attackInformation.VictimAgentHealth <= attackInformation.VictimAgentMaxHealth * 0.5f)
						{
							PerkHelper.AddPerkBonusForCharacter(Throwing.LastHit, val2, true, ref val6, false);
						}
						if (attackInformation.IsHeadShot)
						{
							PerkHelper.AddPerkBonusForCharacter(Throwing.HeadHunter, val2, true, ref val6, false);
						}
						PerkHelper.AddEpicPerkBonusForCharacter(Throwing.UnstoppableForce, val2, DefaultSkills.Throwing, false, ref val6, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus, false);
					}
					goto IL_07aa;
				}
				goto IL_07db;
			}
			goto IL_0801;
		}
		goto IL_09ba;
		IL_07db:
		if (((MissionWeapon)(ref attackerWeapon)).Item != null && ((MissionWeapon)(ref attackerWeapon)).Item.IsCivilian)
		{
			PerkHelper.AddPerkBonusForCharacter(Roguery.Carver, val2, true, ref val6, false);
		}
		goto IL_0801;
		IL_09ba:
		return ((ExplainedNumber)(ref val6)).ResultNumber;
		IL_07aa:
		if (flag2)
		{
			PerkHelper.AddPerkBonusForCharacter(Riding.HorseArcher, val2, true, ref val6, false);
			PerkHelper.AddPerkBonusFromCaptain(Riding.HorseArcher, val3, ref val6);
		}
		if (activeBanner != null)
		{
			BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedRangedDamage, activeBanner, ref val6);
		}
		goto IL_07db;
		IL_0801:
		val5 = collisionData;
		if (((AttackCollisionData)(ref val5)).IsHorseCharge)
		{
			PerkHelper.AddPerkBonusForCharacter(Riding.FullSpeed, val2, true, ref val6, false);
			PerkHelper.AddPerkBonusFromCaptain(Riding.FullSpeed, val3, ref val6);
			if (val2.GetPerkValue(Riding.TheWayOfTheSaddle))
			{
				float num2 = (float)MathF.Max(MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(val, DefaultSkills.Riding) - Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, 0) * Riding.TheWayOfTheSaddle.PrimaryBonus;
				((ExplainedNumber)(ref val6)).Add(num2, (TextObject)null, (TextObject)null);
			}
			if (activeBanner != null)
			{
				BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedChargeDamage, activeBanner, ref val6);
			}
			if (activeBanner2 != null)
			{
				BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedChargeDamage, activeBanner2, ref val6);
			}
		}
		if (flag)
		{
			PerkHelper.AddPerkBonusFromCaptain(TwoHanded.HeadBasher, val3, ref val6);
			PerkHelper.AddPerkBonusFromCaptain(TwoHanded.RecklessCharge, val3, ref val6);
			PerkHelper.AddPerkBonusFromCaptain(Polearm.Pikeman, val3, ref val6);
			if (flag4)
			{
				PerkHelper.AddPerkBonusFromCaptain(Polearm.Braced, val3, ref val6);
			}
		}
		if (flag2)
		{
			PerkHelper.AddPerkBonusFromCaptain(Polearm.Cavalry, val3, ref val6);
		}
		if (currentUsageItem == null)
		{
			val5 = collisionData;
			if (((AttackCollisionData)(ref val5)).IsAlternativeAttack && val2.GetPerkValue(Athletics.StrongLegs))
			{
				((ExplainedNumber)(ref val6)).AddFactor(1f, (TextObject)null);
			}
		}
		if (flag5)
		{
			PerkHelper.AddPerkBonusFromCaptain(Engineering.WallBreaker, val3, ref val6);
		}
		val5 = collisionData;
		if (((AttackCollisionData)(ref val5)).EntityExists)
		{
			PerkHelper.AddPerkBonusFromCaptain(TwoHanded.Vandal, val3, ref val6);
		}
		if (val4 != null)
		{
			PerkHelper.AddPerkBonusFromCaptain(Tactics.Coaching, val3, ref val6);
			if (((BasicCultureObject)val4.Culture).IsBandit)
			{
				PerkHelper.AddPerkBonusFromCaptain(Tactics.LawKeeper, val3, ref val6);
			}
			if (flag2 && flag3)
			{
				PerkHelper.AddPerkBonusFromCaptain(Tactics.Gensdarmes, val3, ref val6);
			}
		}
		if (((BasicCultureObject)val2.Culture).IsBandit)
		{
			PerkHelper.AddPerkBonusFromCaptain(Roguery.PartnersInCrime, val3, ref val6);
		}
		goto IL_09ba;
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
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Invalid comparison between Unknown and I4
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Invalid comparison between Unknown and I4
		Formation attackerFormation = attackInformation.AttackerFormation;
		MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(attackerFormation);
		Agent val = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerAgent.RiderAgent : attackInformation.AttackerAgent);
		_ = attackInformation.IsAttackerAgentMount;
		if (attackInformation.IsAttackerAgentHuman)
		{
			_ = !attackInformation.DoesAttackerHaveMountAgent;
		}
		else
			_ = 0;
		if (attackInformation.DoesAttackerHaveMountAgent)
		{
			_ = 1;
		}
		else
			_ = attackInformation.DoesAttackerHaveRiderAgent;
		BasicCharacterObject obj = (attackInformation.IsVictimAgentMount ? attackInformation.VictimRiderAgentCharacter : attackInformation.VictimAgentCharacter);
		CharacterObject val2 = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		BasicCharacterObject victimCaptainCharacter = attackInformation.VictimCaptainCharacter;
		CharacterObject val3 = (CharacterObject)(object)((victimCaptainCharacter is CharacterObject) ? victimCaptainCharacter : null);
		bool flag = attackInformation.IsVictimAgentHuman && !attackInformation.DoesVictimHaveMountAgent;
		if (attackInformation.DoesVictimHaveMountAgent)
		{
			_ = 1;
		}
		else
			_ = attackInformation.DoesVictimHaveRiderAgent;
		Formation victimFormation = attackInformation.VictimFormation;
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
		MissionWeapon victimMainHandWeapon = attackInformation.VictimMainHandWeapon;
		_ = ((MissionWeapon)(ref victimMainHandWeapon)).CurrentUsageItem;
		victimMainHandWeapon = attackInformation.VictimMainHandWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref victimMainHandWeapon)).CurrentUsageItem;
		AttackCollisionData val4 = collisionData;
		int num;
		if (!((AttackCollisionData)(ref val4)).AttackBlockedWithShield)
		{
			val4 = collisionData;
			num = (((AttackCollisionData)(ref val4)).CollidedWithShieldOnBack ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		bool flag2 = (byte)num != 0;
		ExplainedNumber val5 = default(ExplainedNumber);
		((ExplainedNumber)(ref val5))._002Ector(baseDamage, false, (TextObject)null);
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		WeaponComponentData currentUsageItem2 = ((MissionWeapon)(ref attackerWeapon)).CurrentUsageItem;
		if (attackInformation.DoesAttackerHaveMountAgent && (currentUsageItem2 == null || currentUsageItem2.RelevantSkill != DefaultSkills.Crossbow))
		{
			int effectiveSkill = MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(val, DefaultSkills.Riding);
			SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.MountedWeaponDamagePenalty, ref val5, effectiveSkill);
		}
		if (val2 != null)
		{
			if (currentUsageItem2 != null)
			{
				if (currentUsageItem2.IsConsumable)
				{
					PerkHelper.AddPerkBonusForCharacter(Bow.SkirmishPhaseMaster, val2, true, ref val5, false);
					PerkHelper.AddPerkBonusFromCaptain(Throwing.Skirmisher, val3, ref val5);
					if (((BasicCharacterObject)val2).IsRanged)
					{
						PerkHelper.AddPerkBonusFromCaptain(Bow.SkirmishPhaseMaster, val3, ref val5);
					}
					if (currentUsageItem != null)
					{
						if ((int)currentUsageItem.WeaponClass == 17)
						{
							PerkHelper.AddPerkBonusForCharacter(Crossbow.CounterFire, val2, true, ref val5, false);
							PerkHelper.AddPerkBonusFromCaptain(Crossbow.CounterFire, val3, ref val5);
						}
						else if (currentUsageItem.RelevantSkill == DefaultSkills.Throwing)
						{
							PerkHelper.AddPerkBonusForCharacter(Throwing.Skirmisher, val2, true, ref val5, false);
						}
					}
					if (activeBanner != null)
					{
						BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedRangedAttackDamage, activeBanner, ref val5);
					}
				}
				else if (currentUsageItem2.IsMeleeWeapon)
				{
					if (val3 != null)
					{
						Formation victimFormation2 = attackInformation.VictimFormation;
						if (victimFormation2 != null && (int)victimFormation2.ArrangementOrder.OrderEnum == 5)
						{
							PerkHelper.AddPerkBonusFromCaptain(OneHanded.Basher, val3, ref val5);
						}
					}
					if (activeBanner != null)
					{
						BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedMeleeAttackDamage, activeBanner, ref val5);
					}
				}
			}
			if (flag2)
			{
				PerkHelper.AddPerkBonusForCharacter(OneHanded.SteelCoreShields, val2, true, ref val5, false);
				if (flag)
				{
					PerkHelper.AddPerkBonusFromCaptain(OneHanded.SteelCoreShields, val3, ref val5);
				}
				val4 = collisionData;
				if (((AttackCollisionData)(ref val4)).AttackBlockedWithShield)
				{
					val4 = collisionData;
					if (!((AttackCollisionData)(ref val4)).CorrectSideShieldBlock)
					{
						PerkHelper.AddPerkBonusForCharacter(OneHanded.ShieldWall, val2, true, ref val5, false);
					}
				}
			}
			val4 = collisionData;
			if (((AttackCollisionData)(ref val4)).IsHorseCharge)
			{
				PerkHelper.AddPerkBonusForCharacter(Polearm.SureFooted, val2, true, ref val5, false);
				PerkHelper.AddPerkBonusForCharacter(Athletics.Braced, val2, true, ref val5, false);
				if (val3 != null)
				{
					PerkHelper.AddPerkBonusFromCaptain(Polearm.SureFooted, val3, ref val5);
					PerkHelper.AddPerkBonusFromCaptain(Athletics.Braced, val3, ref val5);
				}
			}
			val4 = collisionData;
			if (((AttackCollisionData)(ref val4)).IsFallDamage)
			{
				PerkHelper.AddPerkBonusForCharacter(Athletics.StrongLegs, val2, true, ref val5, false);
			}
			PerkHelper.AddPerkBonusFromCaptain(Tactics.EliteReserves, val3, ref val5);
		}
		return ((ExplainedNumber)(ref val5)).ResultNumber;
	}

	public override float ApplyGeneralDamageModifiers(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_ = attackInformation.IsAttackerAgentMount;
		_ = attackInformation.IsVictimAgentMount;
		MissionWeapon attackerWeapon = attackInformation.AttackerWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref attackerWeapon)).CurrentUsageItem;
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(baseDamage, false, (TextObject)null);
		if (currentUsageItem != null)
		{
			if (currentUsageItem.RelevantSkill == DefaultSkills.Throwing)
			{
				((ExplainedNumber)(ref val))._002Ector(((ExplainedNumber)(ref val)).ResultNumber * (1f + attackInformation.AttackerAgent.AgentDrivenProperties.ThrowingWeaponDamageMultiplierBonus), false, (TextObject)null);
			}
			else if (currentUsageItem.IsMeleeWeapon)
			{
				((ExplainedNumber)(ref val))._002Ector(((ExplainedNumber)(ref val)).ResultNumber * (1f + attackInformation.AttackerAgent.AgentDrivenProperties.MeleeWeaponDamageMultiplierBonus), false, (TextObject)null);
			}
		}
		Agent attackerAgent = attackInformation.AttackerAgent;
		if (attackerAgent != null)
		{
			((ExplainedNumber)(ref val))._002Ector(((ExplainedNumber)(ref val)).ResultNumber * (1f + attackerAgent.AgentDrivenProperties.DamageMultiplierBonus), false, (TextObject)null);
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
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
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
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
		if (obj == null || isPassiveUsage || (int)strikeType != 0 || (int)attackDirection != 0)
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

	public override void DecideMissileWeaponFlags(Agent attackerAgent, in MissionWeapon missileWeapon, ref WeaponFlags missileWeaponFlags)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		BasicCharacterObject obj = ((attackerAgent != null) ? attackerAgent.Character : null);
		CharacterObject val = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		if (val != null)
		{
			MissionWeapon val2 = missileWeapon;
			if ((int)((MissionWeapon)(ref val2)).CurrentUsageItem.WeaponClass == 23 && val.GetPerkValue(Throwing.Impale))
			{
				missileWeaponFlags = (WeaponFlags)((ulong)missileWeaponFlags | 0x20000uL);
			}
		}
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

	public override bool CanWeaponDealSneakAttack(in AttackInformation attackInformation, WeaponComponentData weapon)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (weapon != null && (weapon.IsMeleeWeapon || (int)weapon.WeaponClass == 22) && attackInformation.IsVictimAgentHuman && !attackInformation.IsVictimPlayer)
		{
			if ((attackInformation.VictimAgentAIStateFlags & 3) == 0 && Extensions.HasAnyFlag<AgentFlag>(attackInformation.VictimAgentFlags, (AgentFlag)65536))
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
		if ((int)blow.StrikeType == 1 && Extensions.HasAnyFlag<WeaponFlags>(blow.WeaponRecord.WeaponFlags, (WeaponFlags)16777216))
		{
			return true;
		}
		BasicCharacterObject character = attackerAgent.Character;
		CharacterObject val;
		if ((val = (CharacterObject)(object)((character is CharacterObject) ? character : null)) != null)
		{
			if (attackerWeapon.RelevantSkill != DefaultSkills.Crossbow || !attackerWeapon.IsConsumable || !val.GetPerkValue(Crossbow.HammerBolts))
			{
				if (attackerWeapon.RelevantSkill == DefaultSkills.Throwing && attackerWeapon.IsConsumable)
				{
					return val.GetPerkValue(Throwing.KnockOff);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override void CalculateDefendedBlowStunMultipliers(Agent attackerAgent, Agent defenderAgent, CombatCollisionResult collisionResult, WeaponComponentData attackerWeapon, WeaponComponentData defenderWeapon, ref float attackerStunPeriod, ref float defenderStunPeriod)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		float num = 1f;
		float num2 = 1f;
		BasicCharacterObject character = attackerAgent.Character;
		CharacterObject val;
		if ((val = (CharacterObject)(object)((character is CharacterObject) ? character : null)) != null && ((int)collisionResult == 3 || (int)collisionResult == 4) && val.GetPerkValue(Athletics.MightyBlow))
		{
			num += num * Athletics.MightyBlow.PrimaryBonus;
		}
		num = MathF.Max(0f, num);
		num2 = MathF.Max(0f, num2);
		attackerStunPeriod *= num;
		defenderStunPeriod *= num2;
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
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected I4, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Invalid comparison between Unknown and I4
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if ((int)attackerWeapon.WeaponClass == 20 || (int)attackerWeapon.WeaponClass == 26)
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

	public override float GetDismountPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if ((int)blow.StrikeType == 0 && Extensions.HasAnyFlag<WeaponFlags>(blow.WeaponRecord.WeaponFlags, (WeaponFlags)33554432))
		{
			num += 0.25f;
		}
		CharacterObject val;
		if (attackerWeapon != null && (val = (CharacterObject)/*isinst with value type is only supported in some contexts*/) != null)
		{
			if (attackerWeapon.RelevantSkill == DefaultSkills.Polearm && val.GetPerkValue(Polearm.Braced))
			{
				num += Polearm.Braced.PrimaryBonus;
			}
			else if (attackerWeapon.RelevantSkill == DefaultSkills.Crossbow && attackerWeapon.IsConsumable && val.GetPerkValue(Crossbow.HammerBolts))
			{
				num += Crossbow.HammerBolts.PrimaryBonus;
			}
			else if (attackerWeapon.RelevantSkill == DefaultSkills.Throwing && attackerWeapon.IsConsumable && val.GetPerkValue(Throwing.KnockOff))
			{
				num += Throwing.KnockOff.PrimaryBonus;
			}
		}
		return MathF.Max(0f, num);
	}

	public override float GetKnockBackPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		float num = 0f;
		CharacterObject val;
		if (attackerWeapon != null && attackerWeapon.RelevantSkill == DefaultSkills.Polearm && (val = (CharacterObject)/*isinst with value type is only supported in some contexts*/) != null && (int)blow.StrikeType == 1 && val.GetPerkValue(Polearm.KeepAtBay))
		{
			num += Polearm.KeepAtBay.PrimaryBonus;
		}
		return num;
	}

	public override float GetKnockDownPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		float num = 0f;
		if ((int)attackerWeapon.WeaponClass == 20 || (int)attackerWeapon.WeaponClass == 26)
		{
			num += 0.25f;
		}
		else if (attackerWeapon.IsMeleeWeapon)
		{
			BasicCharacterObject obj = ((attackerAgent != null) ? attackerAgent.Character : null);
			CharacterObject val = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
			AttackCollisionData val2;
			if ((int)blow.StrikeType == 0)
			{
				val2 = collisionData;
				if ((int)((AttackCollisionData)(ref val2)).VictimHitBodyPart == 8)
				{
					num += 0.1f;
				}
				if (val != null && attackerWeapon.RelevantSkill == DefaultSkills.TwoHanded && val.GetPerkValue(TwoHanded.ShowOfStrength))
				{
					num += TwoHanded.ShowOfStrength.PrimaryBonus;
				}
			}
			val2 = collisionData;
			if ((int)((AttackCollisionData)(ref val2)).VictimHitBodyPart == 0)
			{
				num += 0.15f;
			}
			if (val != null && attackerWeapon.RelevantSkill == DefaultSkills.Polearm && val.GetPerkValue(Polearm.HardKnock))
			{
				num += Polearm.HardKnock.PrimaryBonus;
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
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Invalid comparison between Unknown and I4
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Invalid comparison between Unknown and I4
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		BasicCharacterObject character = defenderAgent.Character;
		CharacterObject val = (CharacterObject)(object)((character is CharacterObject) ? character : null);
		Formation formation = defenderAgent.Formation;
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
		if (val != null)
		{
			if (val2 == val)
			{
				val2 = null;
			}
			ExplainedNumber val3 = default(ExplainedNumber);
			((ExplainedNumber)(ref val3))._002Ector(1f, false, (TextObject)null);
			if (defenderAgent.HasMount)
			{
				PerkHelper.AddPerkBonusForCharacter(Riding.DauntlessSteed, val, true, ref val3, false);
			}
			else
			{
				PerkHelper.AddPerkBonusForCharacter(Athletics.Spartan, val, true, ref val3, false);
			}
			MissionWeapon wieldedWeapon = defenderAgent.WieldedWeapon;
			WeaponComponentData currentUsageItem = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
			if (currentUsageItem != null && (int)currentUsageItem.WeaponClass == 17)
			{
				wieldedWeapon = defenderAgent.WieldedWeapon;
				if (((MissionWeapon)(ref wieldedWeapon)).IsReloading)
				{
					PerkHelper.AddPerkBonusForCharacter(Crossbow.DeftHands, val, true, ref val3, false);
					if (val2 != null)
					{
						PerkHelper.AddPerkBonusFromCaptain(Crossbow.DeftHands, val2, ref val3);
					}
				}
			}
			num = ((ExplainedNumber)(ref val3)).ResultNumber;
		}
		ManagedParametersEnum val4 = (((int)blow.DamageType == 0) ? ((ManagedParametersEnum)10) : (((int)blow.DamageType != 1) ? ((ManagedParametersEnum)11) : ((ManagedParametersEnum)9)));
		return ManagedParameters.Instance.GetManagedParameter(val4) * num;
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
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		CharacterObject val = (CharacterObject)(object)((attackerCharacter is CharacterObject) ? attackerCharacter : null);
		if (val != null)
		{
			AttackCollisionData val2 = collisionData;
			if (((AttackCollisionData)(ref val2)).AttackBlockedWithShield && val.GetPerkValue(Polearm.UnstoppableForce))
			{
				baseDamage *= Polearm.UnstoppableForce.PrimaryBonus;
			}
		}
		return baseDamage;
	}

	public override MeleeCollisionReaction DecidePassiveAttackCollisionReaction(Agent attacker, Agent defender, bool isFatalHit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		MeleeCollisionReaction result = (MeleeCollisionReaction)3;
		if (isFatalHit && attacker.HasMount)
		{
			float num = 0.05f;
			BasicCharacterObject character = attacker.Character;
			CharacterObject val;
			if ((val = (CharacterObject)(object)((character is CharacterObject) ? character : null)) != null && val.GetPerkValue(Polearm.Skewer))
			{
				num += Polearm.Skewer.PrimaryBonus;
			}
			if (MBRandom.RandomFloat < num)
			{
				result = (MeleeCollisionReaction)0;
			}
		}
		return result;
	}

	public override float CalculateShieldDamage(in AttackInformation attackInformation, float baseDamage)
	{
		Formation victimFormation = attackInformation.VictimFormation;
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(baseDamage, false, (TextObject)null);
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
		if (activeBanner != null)
		{
			BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedShieldDamage, activeBanner, ref val);
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override float CalculateSailFireDamage(Agent attackerAgent, IShipOrigin shipOrigin, float baseDamage, bool damageFromShipMachine)
	{
		return baseDamage;
	}

	public override float CalculateHullFireDamage(float baseFireDamage, IShipOrigin shipOrigin)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber val = new ExplainedNumber(baseFireDamage, false, (TextObject)null);
		return ((ExplainedNumber)(ref val)).ResultNumber;
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
			result = (isHuman ? 1f : 0.8f);
			break;
		case 9:
			result = 0.8f;
			break;
		}
		return result;
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
}
