using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.GameComponents;

public class SandboxAgentStatCalculateModel : AgentStatCalculateModel
{
	public override float GetDifficultyModifier()
	{
		Campaign current = Campaign.Current;
		float? obj;
		if (current == null)
		{
			obj = null;
		}
		else
		{
			GameModels models = current.Models;
			if (models == null)
			{
				obj = null;
			}
			else
			{
				DifficultyModel difficultyModel = models.DifficultyModel;
				obj = ((difficultyModel != null) ? new float?(difficultyModel.GetCombatAIDifficultyMultiplier()) : ((float?)null));
			}
		}
		return obj ?? 1f;
	}

	public override bool CanAgentRideMount(Agent agent, Agent targetMount)
	{
		return agent.CheckSkillForMounting(targetMount);
	}

	public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		agentDrivenProperties.ArmorEncumbrance = ((AgentStatCalculateModel)this).GetEffectiveArmorEncumbrance(agent, spawnEquipment);
		if (agent.IsHero)
		{
			BasicCharacterObject character = agent.Character;
			BasicCharacterObject obj = ((character is CharacterObject) ? character : null);
			AgentFlag val = agent.GetAgentFlags();
			if (((CharacterObject)obj).GetPerkValue(Bow.HorseMaster))
			{
				val = (AgentFlag)(val | 0x1000000);
			}
			if (((CharacterObject)obj).GetPerkValue(Crossbow.MountedCrossbowman))
			{
				val = (AgentFlag)(val | 0x2000000);
			}
			if (((CharacterObject)obj).GetPerkValue(TwoHanded.ProjectileDeflection))
			{
				val = (AgentFlag)(val | 0x4000000);
			}
			agent.SetAgentFlags(val);
		}
		else
		{
			agent.HealthLimit = ((AgentStatCalculateModel)this).GetEffectiveMaxHealth(agent);
			agent.Health = agent.HealthLimit;
		}
		agentDrivenProperties.OffhandWeaponDefendSpeedMultiplier = 1f;
		MissionGameModels.Current.AgentStatCalculateModel.UpdateAgentStats(agent, agentDrivenProperties);
	}

	public override void InitializeMissionEquipment(Agent agent)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Invalid comparison between Unknown and I4
		if (!agent.IsHuman)
		{
			return;
		}
		BasicCharacterObject character = agent.Character;
		CharacterObject val = (CharacterObject)(object)((character is CharacterObject) ? character : null);
		if (val == null)
		{
			return;
		}
		object obj;
		if (agent == null)
		{
			obj = null;
		}
		else
		{
			IAgentOriginBase origin = agent.Origin;
			obj = ((origin != null) ? origin.BattleCombatant : null);
		}
		PartyBase val2 = (PartyBase)obj;
		MapEvent val3 = ((val2 != null) ? val2.MapEvent : null);
		MobileParty val4 = ((val2 != null && val2.IsMobile) ? val2.MobileParty : null);
		CharacterObject val5 = PartyBaseHelper.GetVisualPartyLeader(val2);
		if (val5 == val)
		{
			val5 = null;
		}
		MissionEquipment equipment = agent.Equipment;
		ExplainedNumber val8 = default(ExplainedNumber);
		ExplainedNumber val9 = default(ExplainedNumber);
		ExplainedNumber val10 = default(ExplainedNumber);
		for (int i = 0; i < 5; i++)
		{
			EquipmentIndex val6 = (EquipmentIndex)i;
			MissionWeapon val7 = equipment[val6];
			if (((MissionWeapon)(ref val7)).IsEmpty)
			{
				continue;
			}
			WeaponComponentData currentUsageItem = ((MissionWeapon)(ref val7)).CurrentUsageItem;
			if (currentUsageItem == null)
			{
				continue;
			}
			if (currentUsageItem.IsConsumable && currentUsageItem.RelevantSkill != null)
			{
				((ExplainedNumber)(ref val8))._002Ector(0f, false, (TextObject)null);
				if (currentUsageItem.RelevantSkill == DefaultSkills.Bow)
				{
					PerkHelper.AddPerkBonusForCharacter(Bow.DeepQuivers, val, true, ref val8, false);
					if (val5 != null && val5.GetPerkValue(Bow.DeepQuivers))
					{
						((ExplainedNumber)(ref val8)).Add(Bow.DeepQuivers.SecondaryBonus, (TextObject)null, (TextObject)null);
					}
				}
				else if (currentUsageItem.RelevantSkill == DefaultSkills.Crossbow)
				{
					PerkHelper.AddPerkBonusForCharacter(Crossbow.Fletcher, val, true, ref val8, false);
					if (val5 != null && val5.GetPerkValue(Crossbow.Fletcher))
					{
						((ExplainedNumber)(ref val8)).Add(Crossbow.Fletcher.SecondaryBonus, (TextObject)null, (TextObject)null);
					}
				}
				else if (currentUsageItem.RelevantSkill == DefaultSkills.Throwing)
				{
					PerkHelper.AddPerkBonusForCharacter(Throwing.WellPrepared, val, true, ref val8, false);
					PerkHelper.AddPerkBonusForCharacter(Throwing.Resourceful, val, true, ref val8, false);
					if (agent.HasMount)
					{
						PerkHelper.AddPerkBonusForCharacter(Throwing.Saddlebags, val, true, ref val8, false);
					}
					PerkHelper.AddPerkBonusForParty(Throwing.WellPrepared, val4, false, ref val8, false);
				}
				int num = MathF.Round(((ExplainedNumber)(ref val8)).ResultNumber);
				((ExplainedNumber)(ref val9))._002Ector((float)(((MissionWeapon)(ref val7)).Amount + num), false, (TextObject)null);
				if (val4 != null && val3 != null && val3.AttackerSide == val2.MapEventSide && (int)val3.EventType == 5)
				{
					PerkHelper.AddPerkBonusForParty(Engineering.MilitaryPlanner, val4, true, ref val9, false);
				}
				int num2 = MathF.Round(((ExplainedNumber)(ref val9)).ResultNumber);
				if (num2 != ((MissionWeapon)(ref val7)).Amount)
				{
					equipment.SetAmountOfSlot(val6, (short)num2, true);
				}
			}
			else if (currentUsageItem.IsShield)
			{
				((ExplainedNumber)(ref val10))._002Ector((float)((MissionWeapon)(ref val7)).HitPoints, false, (TextObject)null);
				PerkHelper.AddPerkBonusForCharacter(Engineering.Scaffolds, val, false, ref val10, false);
				int num3 = MathF.Round(((ExplainedNumber)(ref val10)).ResultNumber);
				if (num3 != ((MissionWeapon)(ref val7)).HitPoints)
				{
					equipment.SetHitPointsOfSlot(val6, (short)num3, true);
				}
			}
		}
	}

	public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
	{
		if (agent.IsHuman)
		{
			UpdateHumanStats(agent, agentDrivenProperties);
		}
		else
		{
			UpdateHorseStats(agent, agentDrivenProperties);
		}
	}

	public override int GetEffectiveSkill(Agent agent, SkillObject skill)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Invalid comparison between Unknown and I4
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Invalid comparison between Unknown and I4
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector((float)((AgentStatCalculateModel)this).GetEffectiveSkill(agent, skill), false, (TextObject)null);
		BasicCharacterObject character = agent.Character;
		CharacterObject val2 = (CharacterObject)(object)((character is CharacterObject) ? character : null);
		Formation formation = agent.Formation;
		IAgentOriginBase origin = agent.Origin;
		PartyBase val3 = (PartyBase)((origin != null) ? origin.BattleCombatant : null);
		MobileParty val4 = ((val3 != null && val3.IsMobile) ? val3.MobileParty : null);
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
		CharacterObject val5 = (CharacterObject)((obj is CharacterObject) ? obj : null);
		if (val5 == val2)
		{
			val5 = null;
		}
		if (val5 != null)
		{
			bool flag = skill == DefaultSkills.Bow || skill == DefaultSkills.Crossbow || skill == DefaultSkills.Throwing;
			bool flag2 = skill == DefaultSkills.OneHanded || skill == DefaultSkills.TwoHanded || skill == DefaultSkills.Polearm;
			if ((((BasicCharacterObject)val2).IsInfantry && flag) || (((BasicCharacterObject)val2).IsRanged && flag2))
			{
				PerkHelper.AddPerkBonusFromCaptain(Throwing.FlexibleFighter, val5, ref val);
			}
		}
		if (skill == DefaultSkills.Bow)
		{
			if (val5 != null)
			{
				PerkHelper.AddPerkBonusFromCaptain(Bow.DeadAim, val5, ref val);
				if (((BasicCharacterObject)val2).HasMount())
				{
					PerkHelper.AddPerkBonusFromCaptain(Bow.HorseMaster, val5, ref val);
				}
			}
		}
		else if (skill == DefaultSkills.Throwing)
		{
			if (val5 != null)
			{
				PerkHelper.AddPerkBonusFromCaptain(Athletics.StrongArms, val5, ref val);
				PerkHelper.AddPerkBonusFromCaptain(Throwing.RunningThrow, val5, ref val);
			}
		}
		else if (skill == DefaultSkills.Crossbow && val5 != null)
		{
			PerkHelper.AddPerkBonusFromCaptain(Crossbow.DonkeysSwiftness, val5, ref val);
		}
		if (val4 != null && !val4.IsCurrentlyAtSea && val4.HasPerk(Roguery.OneOfTheFamily, false) && (int)val2.Occupation == 15 && skill.Attributes.Any((CharacterAttribute attribute) => attribute == DefaultCharacterAttributes.Vigor || attribute == DefaultCharacterAttributes.Control))
		{
			((ExplainedNumber)(ref val)).Add(Roguery.OneOfTheFamily.PrimaryBonus, ((PropertyObject)Roguery.OneOfTheFamily).Name, (TextObject)null);
		}
		if (((BasicCharacterObject)val2).HasMount())
		{
			if (skill == DefaultSkills.Riding && val5 != null)
			{
				PerkHelper.AddPerkBonusFromCaptain(Riding.NimbleSteed, val5, ref val);
			}
		}
		else
		{
			if (val4 != null && formation != null)
			{
				bool num = skill == DefaultSkills.OneHanded || skill == DefaultSkills.TwoHanded || skill == DefaultSkills.Polearm;
				bool flag3 = (int)formation.ArrangementOrder.OrderEnum == 5;
				if (num && flag3)
				{
					PerkHelper.AddPerkBonusForParty(Polearm.Phalanx, val4, true, ref val, false);
				}
			}
			if (val5 != null)
			{
				if (skill == DefaultSkills.OneHanded)
				{
					PerkHelper.AddPerkBonusFromCaptain(OneHanded.WrappedHandles, val5, ref val);
				}
				else if (skill == DefaultSkills.TwoHanded)
				{
					PerkHelper.AddPerkBonusFromCaptain(TwoHanded.StrongGrip, val5, ref val);
				}
				else if (skill == DefaultSkills.Polearm)
				{
					PerkHelper.AddPerkBonusFromCaptain(Polearm.CleanThrust, val5, ref val);
					PerkHelper.AddPerkBonusFromCaptain(Polearm.CounterWeight, val5, ref val);
				}
			}
		}
		return (int)((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override float GetWeaponDamageMultiplier(Agent agent, WeaponComponentData weapon)
	{
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(1f, false, (TextObject)null);
		SkillObject val2 = ((weapon != null) ? weapon.RelevantSkill : null);
		if (agent.Character is CharacterObject && val2 != null)
		{
			if (val2 == DefaultSkills.OneHanded)
			{
				int effectiveSkill = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, val2);
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.OneHandedDamage, ref val, effectiveSkill);
			}
			else if (val2 == DefaultSkills.TwoHanded)
			{
				int effectiveSkill2 = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, val2);
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.TwoHandedDamage, ref val, effectiveSkill2);
			}
			else if (val2 == DefaultSkills.Polearm)
			{
				int effectiveSkill3 = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, val2);
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.PolearmDamage, ref val, effectiveSkill3);
			}
			else if (val2 == DefaultSkills.Bow)
			{
				int effectiveSkill4 = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, val2);
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.BowDamage, ref val, effectiveSkill4);
			}
			else if (val2 == DefaultSkills.Throwing)
			{
				int effectiveSkill5 = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, val2);
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.ThrowingDamage, ref val, effectiveSkill5);
			}
		}
		return Math.Max(0f, ((ExplainedNumber)(ref val)).ResultNumber);
	}

	private float GetArmorStealthBonus(EquipmentElement armorElement, int maxBodyPartBonus)
	{
		if (!((EquipmentElement)(ref armorElement)).IsEmpty && ((EquipmentElement)(ref armorElement)).Item != null && ((EquipmentElement)(ref armorElement)).Item.HasArmorComponent)
		{
			return ((EquipmentElement)(ref armorElement)).GetModifiedStealthFactor();
		}
		return 0f;
	}

	public override float GetEquipmentStealthBonus(Agent agent)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Equipment spawnEquipment = agent.SpawnEquipment;
		return 0f + GetArmorStealthBonus(spawnEquipment[(EquipmentIndex)5], 25) + GetArmorStealthBonus(spawnEquipment[(EquipmentIndex)9], 15) + GetArmorStealthBonus(spawnEquipment[(EquipmentIndex)6], 30) + GetArmorStealthBonus(spawnEquipment[(EquipmentIndex)8], 10) + GetArmorStealthBonus(spawnEquipment[(EquipmentIndex)7], 20);
	}

	public override float GetSneakAttackMultiplier(Agent agent, WeaponComponentData weapon)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(1f, false, (TextObject)null);
		if (weapon != null && agent.Character is CharacterObject)
		{
			int effectiveSkill = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, DefaultSkills.Roguery);
			SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.SneakDamage, ref val, effectiveSkill);
			if ((weapon != null && (int)weapon.WeaponClass == 1) || (weapon != null && (int)weapon.WeaponClass == 22))
			{
				((ExplainedNumber)(ref val)).AddFactor(2f, (TextObject)null);
			}
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override float GetKnockBackResistance(Agent agent)
	{
		if (agent.IsHuman)
		{
			int effectiveSkill = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, DefaultSkills.Athletics);
			return DefaultSkillEffects.KnockBackResistance.GetSkillEffectValue(effectiveSkill);
		}
		return float.MaxValue;
	}

	public override float GetKnockDownResistance(Agent agent, StrikeType strikeType = (StrikeType)(-1))
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		if (agent.IsHuman)
		{
			int effectiveSkill = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, DefaultSkills.Athletics);
			float num = DefaultSkillEffects.KnockDownResistance.GetSkillEffectValue(effectiveSkill);
			if (agent.HasMount)
			{
				num += 0.1f;
			}
			else if ((int)strikeType == 1)
			{
				num += 0.15f;
			}
			return num;
		}
		return float.MaxValue;
	}

	public override float GetDismountResistance(Agent agent)
	{
		if (agent.IsHuman)
		{
			int effectiveSkill = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, DefaultSkills.Riding);
			return DefaultSkillEffects.DismountResistance.GetSkillEffectValue(effectiveSkill);
		}
		return float.MaxValue;
	}

	public override float GetBreatheHoldMaxDuration(Agent agent, float baseBreatheHoldMaxDuration)
	{
		return baseBreatheHoldMaxDuration;
	}

	public override float GetWeaponInaccuracy(Agent agent, WeaponComponentData weapon, int weaponSkill)
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Invalid comparison between Unknown and I4
		BasicCharacterObject character = agent.Character;
		CharacterObject val = (CharacterObject)(object)((character is CharacterObject) ? character : null);
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
		if (val == val2)
		{
			val2 = null;
		}
		float num = 0f;
		if (weapon.IsRangedWeapon)
		{
			ExplainedNumber val3 = default(ExplainedNumber);
			((ExplainedNumber)(ref val3))._002Ector(1f, false, (TextObject)null);
			if (val != null)
			{
				if (weapon.RelevantSkill == DefaultSkills.Bow)
				{
					SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.BowAccuracy, ref val3, weaponSkill);
					PerkHelper.AddPerkBonusFromCaptain(Bow.QuickAdjustments, val2, ref val3);
				}
				else if (weapon.RelevantSkill == DefaultSkills.Crossbow)
				{
					SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.CrossbowAccuracy, ref val3, weaponSkill);
				}
				else if (weapon.RelevantSkill == DefaultSkills.Throwing)
				{
					if ((int)weapon.WeaponClass == 18)
					{
						((ExplainedNumber)(ref val3))._002Ector(MathF.Max(1f - 0.007f * (float)weaponSkill, 0.2f), false, (TextObject)null);
					}
					else
					{
						SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.ThrowingAccuracy, ref val3, weaponSkill);
					}
				}
			}
			num = (100f - (float)weapon.Accuracy) * ((ExplainedNumber)(ref val3)).ResultNumber * 0.001f;
		}
		else if (Extensions.HasAllFlags<WeaponFlags>(weapon.WeaponFlags, (WeaponFlags)64))
		{
			num = 1f - (float)weaponSkill * 0.01f;
		}
		return MathF.Max(num, 0f);
	}

	public override float GetInteractionDistance(Agent agent)
	{
		CharacterObject val;
		if (agent.HasMount && (val = (CharacterObject)/*isinst with value type is only supported in some contexts*/) != null && val.GetPerkValue(Throwing.LongReach))
		{
			return 3f;
		}
		return ((AgentStatCalculateModel)this).GetInteractionDistance(agent);
	}

	public override float GetMaxCameraZoom(Agent agent)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		BasicCharacterObject character = agent.Character;
		CharacterObject val = (CharacterObject)(object)((character is CharacterObject) ? character : null);
		ExplainedNumber val2 = default(ExplainedNumber);
		((ExplainedNumber)(ref val2))._002Ector(1f, false, (TextObject)null);
		if (val != null)
		{
			MissionEquipment equipment = agent.Equipment;
			EquipmentIndex primaryWieldedItemIndex = agent.GetPrimaryWieldedItemIndex();
			object obj;
			if ((int)primaryWieldedItemIndex == -1)
			{
				obj = null;
			}
			else
			{
				MissionWeapon val3 = equipment[primaryWieldedItemIndex];
				obj = ((MissionWeapon)(ref val3)).CurrentUsageItem;
			}
			WeaponComponentData val4 = (WeaponComponentData)obj;
			if (val4 != null)
			{
				if (val4.RelevantSkill == DefaultSkills.Bow)
				{
					PerkHelper.AddPerkBonusForCharacter(Bow.EagleEye, val, true, ref val2, false);
				}
				else if (val4.RelevantSkill == DefaultSkills.Crossbow)
				{
					PerkHelper.AddPerkBonusForCharacter(Crossbow.LongShots, val, true, ref val2, false);
				}
				else if (val4.RelevantSkill == DefaultSkills.Throwing)
				{
					PerkHelper.AddPerkBonusForCharacter(Throwing.Focus, val, true, ref val2, false);
				}
			}
		}
		return ((ExplainedNumber)(ref val2)).ResultNumber;
	}

	public List<PerkObject> GetPerksOfAgent(CharacterObject agentCharacter, SkillObject skill = null, bool filterPartyRole = false, PartyRole partyRole = (PartyRole)12)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		List<PerkObject> list = new List<PerkObject>();
		if (agentCharacter != null)
		{
			foreach (PerkObject item in (List<PerkObject>)(object)PerkObject.All)
			{
				if (!agentCharacter.GetPerkValue(item) || (skill != null && skill != item.Skill))
				{
					continue;
				}
				if (filterPartyRole)
				{
					if (item.PrimaryRole == partyRole || item.SecondaryRole == partyRole)
					{
						list.Add(item);
					}
				}
				else
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public override string GetMissionDebugInfoForAgent(Agent agent)
	{
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		text += "Base: Initial stats modified only by skills\n";
		text += "Effective (Eff): Stats that are modified by perks & mission effects\n\n";
		string text2 = "{0,-20}";
		text = text + string.Format(text2, "Name") + ": " + agent.Name + "\n";
		text = text + string.Format(text2, "Age") + ": " + (int)agent.Age + "\n";
		text = text + string.Format(text2, "Health") + ": " + agent.Health + "\n";
		int num = (agent.IsHuman ? agent.Character.MaxHitPoints() : agent.Monster.HitPoints);
		text = text + string.Format(text2, "Max.Health") + ": " + num + "(Base)\n";
		text = text + string.Format(text2, "") + "  " + MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveMaxHealth(agent) + "(Eff)\n";
		text = text + string.Format(text2, "Team") + ": " + ((agent.Team == null) ? "N/A" : (agent.Team.IsAttacker ? "Attacker" : "Defender")) + "\n";
		if (agent.IsHuman)
		{
			string format = text2 + ": {1,4:G}, {2,4:G}";
			text += "-------------------------------------\n";
			text = text + string.Format(text2 + ": {1,4}, {2,4}", "Skills", "Base", "Eff") + "\n";
			text += "-------------------------------------\n";
			foreach (SkillObject item in (List<SkillObject>)(object)Skills.All)
			{
				int skillValue = agent.Character.GetSkillValue(item);
				int effectiveSkill = MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(agent, item);
				string text3 = string.Format(format, ((PropertyObject)item).Name, skillValue, effectiveSkill);
				text = text + text3 + "\n";
			}
			text += "-------------------------------------\n";
			BasicCharacterObject character = agent.Character;
			CharacterObject agentCharacter = (CharacterObject)(object)((character is CharacterObject) ? character : null);
			string debugPerkInfoForAgent = GetDebugPerkInfoForAgent(agentCharacter, filterPartyRole: false, (PartyRole)12);
			if (debugPerkInfoForAgent.Length > 0)
			{
				text = text + string.Format(text2 + ": ", "Perks") + "\n";
				text += "-------------------------------------\n";
				text += debugPerkInfoForAgent;
				text += "-------------------------------------\n";
			}
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
			CharacterObject val = (CharacterObject)((obj is CharacterObject) ? obj : null);
			string debugPerkInfoForAgent2 = GetDebugPerkInfoForAgent(val, filterPartyRole: true, (PartyRole)13);
			if (debugPerkInfoForAgent2.Length > 0)
			{
				text = string.Concat(text, string.Format(text2 + ": ", "Captain Perks"), ((BasicCharacterObject)val).Name, "\n");
				text += "-------------------------------------\n";
				text += debugPerkInfoForAgent2;
				text += "-------------------------------------\n";
			}
			IAgentOriginBase origin = agent.Origin;
			PartyBase val2 = (PartyBase)((origin != null) ? origin.BattleCombatant : null);
			object obj2;
			if ((int)val2 == 0)
			{
				obj2 = null;
			}
			else
			{
				MobileParty mobileParty = val2.MobileParty;
				obj2 = ((mobileParty != null) ? mobileParty.Party : null);
			}
			CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader((PartyBase)obj2);
			string debugPerkInfoForAgent3 = GetDebugPerkInfoForAgent(visualPartyLeader, filterPartyRole: true, (PartyRole)5);
			if (debugPerkInfoForAgent3.Length > 0)
			{
				text = string.Concat(text, string.Format(text2 + ": ", "Party Leader Perks"), ((BasicCharacterObject)visualPartyLeader).Name, "\n");
				text += "-------------------------------------\n";
				text += debugPerkInfoForAgent3;
				text += "-------------------------------------\n";
			}
		}
		return text;
	}

	public override float GetEffectiveArmorEncumbrance(Agent agent, Equipment equipment)
	{
		float totalWeightOfArmor = equipment.GetTotalWeightOfArmor(agent.IsHuman);
		float num = 1f;
		BasicCharacterObject character = agent.Character;
		CharacterObject val;
		if ((val = (CharacterObject)(object)((character is CharacterObject) ? character : null)) != null && val.GetPerkValue(Athletics.FormFittingArmor))
		{
			num += Athletics.FormFittingArmor.PrimaryBonus;
		}
		return MathF.Max(0f, totalWeightOfArmor * num);
	}

	public override float GetEffectiveMaxHealth(Agent agent)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		if (agent.IsHero)
		{
			return agent.Character.MaxHitPoints();
		}
		float baseHealthLimit = agent.BaseHealthLimit;
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(baseHealthLimit, false, (TextObject)null);
		if (agent.IsHuman)
		{
			BasicCharacterObject character = agent.Character;
			CharacterObject val2 = (CharacterObject)(object)((character is CharacterObject) ? character : null);
			IAgentOriginBase obj = ((agent != null) ? agent.Origin : null);
			PartyBase val3 = (PartyBase)((obj != null) ? obj.BattleCombatant : null);
			MobileParty val4 = (((int)val3 != 0) ? val3.MobileParty : null);
			object obj2;
			if (val4 == null)
			{
				obj2 = null;
			}
			else
			{
				Hero leaderHero = val4.LeaderHero;
				obj2 = ((leaderHero != null) ? leaderHero.CharacterObject : null);
			}
			CharacterObject val5 = (CharacterObject)obj2;
			if (val2 != null && val5 != null)
			{
				if (!val4.IsCurrentlyAtSea)
				{
					PerkHelper.AddPerkBonusForParty(TwoHanded.ThickHides, val4, false, ref val, false);
					PerkHelper.AddPerkBonusForParty(Polearm.HardyFrontline, val4, true, ref val, false);
				}
				if (((BasicCharacterObject)val2).IsRanged)
				{
					PerkHelper.AddPerkBonusForParty(Crossbow.PickedShots, val4, false, ref val, false);
				}
				if (!agent.HasMount)
				{
					if (!val4.IsCurrentlyAtSea)
					{
						PerkHelper.AddPerkBonusForParty(Athletics.WellBuilt, val4, false, ref val, false);
					}
					PerkHelper.AddPerkBonusForParty(Polearm.HardKnock, val4, false, ref val, false);
					if (!val4.IsCurrentlyAtSea && ((BasicCharacterObject)val2).IsInfantry)
					{
						PerkHelper.AddPerkBonusForParty(OneHanded.UnwaveringDefense, val4, false, ref val, false);
					}
				}
				if (val5.GetPerkValue(Medicine.MinisterOfHealth))
				{
					int num = (int)((float)MathF.Max(((BasicCharacterObject)val5).GetSkillValue(DefaultSkills.Medicine) - Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, 0) * Medicine.MinisterOfHealth.PrimaryBonus);
					if (num > 0)
					{
						((ExplainedNumber)(ref val)).Add((float)num, (TextObject)null, (TextObject)null);
					}
				}
			}
		}
		else
		{
			Agent riderAgent = agent.RiderAgent;
			if (riderAgent != null)
			{
				BasicCharacterObject obj3 = ((riderAgent != null) ? riderAgent.Character : null);
				CharacterObject val6 = (CharacterObject)(object)((obj3 is CharacterObject) ? obj3 : null);
				object obj4;
				if (riderAgent == null)
				{
					obj4 = null;
				}
				else
				{
					IAgentOriginBase origin = riderAgent.Origin;
					obj4 = ((origin != null) ? origin.BattleCombatant : null);
				}
				PartyBase val7 = (PartyBase)obj4;
				MobileParty val8 = (((int)val7 != 0) ? val7.MobileParty : null);
				PerkHelper.AddPerkBonusForParty(Medicine.Sledges, val8, false, ref val, false);
				PerkHelper.AddPerkBonusForCharacter(Riding.Veterinary, val6, true, ref val, false);
				PerkHelper.AddPerkBonusForParty(Riding.Veterinary, val8, false, ref val, false);
			}
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override float GetEnvironmentSpeedFactor(Agent agent)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		_ = agent.Mission.Scene;
		float num = 1f;
		if (!agent.Mission.Scene.IsAtmosphereIndoor)
		{
			if (agent.Mission.Scene.GetRainDensity() > 0f)
			{
				num *= 0.9f;
			}
			if (!agent.IsHuman)
			{
				CampaignTime now = CampaignTime.Now;
				if (((CampaignTime)(ref now)).IsNightTime)
				{
					num *= 0.9f;
				}
			}
		}
		return num;
	}

	private string GetDebugPerkInfoForAgent(CharacterObject agentCharacter, bool filterPartyRole = false, PartyRole partyRole = (PartyRole)12)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		string format = "{0,-18}";
		if (GetPerksOfAgent(agentCharacter, null, filterPartyRole, partyRole).Count > 0)
		{
			foreach (SkillObject item in (List<SkillObject>)(object)Skills.All)
			{
				List<PerkObject> perksOfAgent = GetPerksOfAgent(agentCharacter, item, filterPartyRole, partyRole);
				if (perksOfAgent == null || perksOfAgent.Count <= 0)
				{
					continue;
				}
				string text2 = string.Format(format, ((PropertyObject)item).Name) + ": ";
				int num = 5;
				int num2 = 0;
				foreach (PerkObject item2 in perksOfAgent)
				{
					string text3 = ((object)((PropertyObject)item2).Name).ToString();
					if (num2 == num)
					{
						text2 = text2 + "\n" + string.Format(format, "") + "  ";
						num2 = 0;
					}
					text2 = text2 + text3 + ", ";
					num2++;
				}
				text2 = text2.Remove(text2.LastIndexOf(","));
				text = text + text2 + "\n";
			}
		}
		return text;
	}

	private void UpdateHumanStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Invalid comparison between Unknown and I4
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Invalid comparison between Unknown and I4
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Invalid comparison between Unknown and I4
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Invalid comparison between Unknown and I4
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Invalid comparison between Unknown and I4
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Invalid comparison between Unknown and I4
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Expected I4, but got Unknown
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Invalid comparison between Unknown and I4
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Invalid comparison between Unknown and I4
		//IL_08f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Invalid comparison between Unknown and I4
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_058b: Invalid comparison between Unknown and I4
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Invalid comparison between Unknown and I4
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Invalid comparison between Unknown and I4
		Equipment spawnEquipment = agent.SpawnEquipment;
		agentDrivenProperties.ArmorHead = spawnEquipment.GetHeadArmorSum();
		agentDrivenProperties.ArmorTorso = spawnEquipment.GetHumanBodyArmorSum();
		agentDrivenProperties.ArmorLegs = spawnEquipment.GetLegArmorSum();
		agentDrivenProperties.ArmorArms = spawnEquipment.GetArmArmorSum();
		BasicCharacterObject character = agent.Character;
		CharacterObject val = (CharacterObject)(object)((character is CharacterObject) ? character : null);
		MissionEquipment equipment = agent.Equipment;
		float num = equipment.GetTotalWeightOfWeapons();
		float effectiveArmorEncumbrance = ((AgentStatCalculateModel)this).GetEffectiveArmorEncumbrance(agent, spawnEquipment);
		int weight = agent.Monster.Weight;
		EquipmentIndex primaryWieldedItemIndex = agent.GetPrimaryWieldedItemIndex();
		EquipmentIndex offhandWieldedItemIndex = agent.GetOffhandWieldedItemIndex();
		MissionWeapon val2;
		if ((int)primaryWieldedItemIndex != -1)
		{
			val2 = equipment[primaryWieldedItemIndex];
			ItemObject item = ((MissionWeapon)(ref val2)).Item;
			WeaponComponent weaponComponent = item.WeaponComponent;
			if (weaponComponent != null)
			{
				ItemTypeEnum itemType = weaponComponent.GetItemType();
				bool flag = false;
				if (val != null)
				{
					bool flag2 = (int)itemType == 9 && val.GetPerkValue(Bow.RangersSwiftness);
					bool flag3 = (int)itemType == 10 && val.GetPerkValue(Crossbow.LooseAndMove);
					flag = flag2 || flag3;
				}
				if (!flag)
				{
					float realWeaponLength = weaponComponent.PrimaryWeapon.GetRealWeaponLength();
					num += 4f * MathF.Sqrt(realWeaponLength) * item.Weight;
				}
			}
		}
		if ((int)offhandWieldedItemIndex != -1)
		{
			val2 = equipment[offhandWieldedItemIndex];
			ItemObject item2 = ((MissionWeapon)(ref val2)).Item;
			WeaponComponentData primaryWeapon = item2.PrimaryWeapon;
			if (primaryWeapon != null && Extensions.HasAnyFlag<WeaponFlags>(primaryWeapon.WeaponFlags, (WeaponFlags)268435456) && (val == null || !val.GetPerkValue(OneHanded.ShieldBearer)))
			{
				num += 1.5f * item2.Weight;
			}
		}
		agentDrivenProperties.WeaponExternalAccelerationAccuracyPenalty = 0f;
		agentDrivenProperties.WeaponsEncumbrance = num;
		agentDrivenProperties.ArmorEncumbrance = effectiveArmorEncumbrance;
		float num2 = effectiveArmorEncumbrance + num;
		EquipmentIndex primaryWieldedItemIndex2 = agent.GetPrimaryWieldedItemIndex();
		object obj;
		if ((int)primaryWieldedItemIndex2 == -1)
		{
			obj = null;
		}
		else
		{
			val2 = equipment[primaryWieldedItemIndex2];
			obj = ((MissionWeapon)(ref val2)).CurrentUsageItem;
		}
		WeaponComponentData val3 = (WeaponComponentData)obj;
		EquipmentIndex offhandWieldedItemIndex2 = agent.GetOffhandWieldedItemIndex();
		object obj2;
		if ((int)offhandWieldedItemIndex2 == -1)
		{
			obj2 = null;
		}
		else
		{
			val2 = equipment[offhandWieldedItemIndex2];
			obj2 = ((MissionWeapon)(ref val2)).CurrentUsageItem;
		}
		WeaponComponentData val4 = (WeaponComponentData)obj2;
		agentDrivenProperties.SwingSpeedMultiplier = 0.93f;
		agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = 0.93f;
		agentDrivenProperties.HandlingMultiplier = 1f;
		agentDrivenProperties.ShieldBashStunDurationMultiplier = 1f;
		agentDrivenProperties.KickStunDurationMultiplier = 1f;
		agentDrivenProperties.ReloadSpeed = 0.93f;
		agentDrivenProperties.MissileSpeedMultiplier = 1f;
		agentDrivenProperties.ReloadMovementPenaltyFactor = 1f;
		agentDrivenProperties.DamageMultiplierBonus = 0f;
		((AgentStatCalculateModel)this).SetAllWeaponInaccuracy(agent, agentDrivenProperties, (int)primaryWieldedItemIndex2, val3);
		int effectiveSkill = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, DefaultSkills.Athletics);
		int effectiveSkill2 = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, DefaultSkills.Riding);
		if (val3 != null)
		{
			WeaponComponentData val5 = val3;
			int effectiveSkillForWeapon = ((AgentStatCalculateModel)this).GetEffectiveSkillForWeapon(agent, val5);
			if (val5.IsRangedWeapon)
			{
				int thrustSpeed = val5.ThrustSpeed;
				if (!agent.HasMount)
				{
					float num3 = MathF.Max(0f, 1f - (float)effectiveSkillForWeapon / 500f);
					agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = MathF.Max(0f, 0.125f * num3);
					agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = MathF.Max(0f, 0.1f * num3);
				}
				else
				{
					float num4 = MathF.Max(0f, (1f - (float)effectiveSkillForWeapon / 500f) * (1f - (float)effectiveSkill2 / 1800f));
					agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = MathF.Max(0f, 0.025f * num4);
					agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = MathF.Max(0f, 0.12f * num4);
				}
				if (val5.RelevantSkill == DefaultSkills.Bow)
				{
					float num5 = ((float)thrustSpeed - 45f) / 90f;
					num5 = MBMath.ClampFloat(num5, 0f, 1f);
					agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 6f;
					agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 4.5f / MBMath.Lerp(0.75f, 2f, num5, 1E-05f);
				}
				else if (val5.RelevantSkill == DefaultSkills.Throwing)
				{
					if ((int)val5.WeaponClass == 18)
					{
						float num6 = ((float)thrustSpeed - 30f) / 90f;
						num6 = MBMath.ClampFloat(num6, 0f, 1f);
						agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 5f;
						agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 2.4f * MBMath.Lerp(2.4f, 1.2f, num6, 1E-05f);
					}
					else
					{
						float num7 = ((float)thrustSpeed - 91f) / 14f;
						num7 = MBMath.ClampFloat(num7, 0f, 1f);
						agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 0.5f;
						agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 1.5f * MBMath.Lerp(1.5f, 0.8f, num7, 1E-05f);
					}
				}
				else if (val5.RelevantSkill == DefaultSkills.Crossbow)
				{
					agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 2.5f;
					agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 1.2f;
				}
				if ((int)val5.WeaponClass == 16)
				{
					agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.3f + (95.75f - (float)thrustSpeed) * 0.005f;
					float num8 = ((float)thrustSpeed - 45f) / 90f;
					num8 = MBMath.ClampFloat(num8, 0f, 1f);
					agentDrivenProperties.WeaponUnsteadyBeginTime = 0.6f + (float)effectiveSkillForWeapon * 0.01f * MBMath.Lerp(2f, 4f, num8, 1E-05f);
					if (agent.IsAIControlled)
					{
						agentDrivenProperties.WeaponUnsteadyBeginTime *= 4f;
					}
					agentDrivenProperties.WeaponUnsteadyEndTime = 2f + agentDrivenProperties.WeaponUnsteadyBeginTime;
					agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
				}
				else if ((int)val5.WeaponClass == 23 || (int)val5.WeaponClass == 21 || (int)val5.WeaponClass == 22)
				{
					agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.2f + (89f - (float)thrustSpeed) * 0.009f;
					agentDrivenProperties.WeaponUnsteadyBeginTime = 2.5f + (float)effectiveSkillForWeapon * 0.01f;
					agentDrivenProperties.WeaponUnsteadyEndTime = 10f + agentDrivenProperties.WeaponUnsteadyBeginTime;
					agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.025f;
				}
				else if ((int)val5.WeaponClass == 18)
				{
					agentDrivenProperties.WeaponBestAccuracyWaitTime = 2.6f + (89f - (float)thrustSpeed) * 0.12f;
					agentDrivenProperties.WeaponUnsteadyBeginTime = 3f + (float)effectiveSkillForWeapon * 0.064f;
					agentDrivenProperties.WeaponUnsteadyEndTime = 22f + agentDrivenProperties.WeaponUnsteadyBeginTime;
					agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.2f;
				}
				else
				{
					agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.1f;
					agentDrivenProperties.WeaponUnsteadyBeginTime = 0f;
					agentDrivenProperties.WeaponUnsteadyEndTime = 0f;
					agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
				}
			}
			else if (Extensions.HasAllFlags<WeaponFlags>(val5.WeaponFlags, (WeaponFlags)64))
			{
				agentDrivenProperties.WeaponUnsteadyBeginTime = 1f + (float)effectiveSkillForWeapon * 0.005f;
				agentDrivenProperties.WeaponUnsteadyEndTime = 3f + (float)effectiveSkillForWeapon * 0.01f;
			}
		}
		agentDrivenProperties.TopSpeedReachDuration = 2.5f + MathF.Max(5f - (1f + (float)effectiveSkill * 0.01f), 1f) / 3.5f - MathF.Min((float)weight / ((float)weight + num2), 0.8f);
		ExplainedNumber val6 = default(ExplainedNumber);
		((ExplainedNumber)(ref val6))._002Ector(0.7f, false, (TextObject)null);
		ExplainedNumber val7 = default(ExplainedNumber);
		((ExplainedNumber)(ref val7))._002Ector(0.7f, false, (TextObject)null);
		SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.AthleticsSpeedFactor, ref val6, effectiveSkill);
		SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.AthleticsSpeedFactor, ref val7, 300);
		ExplainedNumber val8 = default(ExplainedNumber);
		((ExplainedNumber)(ref val8))._002Ector(0.2f, false, (TextObject)null);
		((ExplainedNumber)(ref val8)).LimitMin(0f);
		SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.AthleticsWeightFactor, ref val8, effectiveSkill);
		float num9 = ((ExplainedNumber)(ref val8)).ResultNumber * num2 / (float)weight;
		float num10 = MBMath.ClampFloat(((ExplainedNumber)(ref val6)).ResultNumber - num9, 0f, ((ExplainedNumber)(ref val7)).ResultNumber);
		agentDrivenProperties.MaxSpeedMultiplier = ((AgentStatCalculateModel)this).GetEnvironmentSpeedFactor(agent) * num10;
		float managedParameter = ManagedParameters.Instance.GetManagedParameter((ManagedParametersEnum)5);
		float managedParameter2 = ManagedParameters.Instance.GetManagedParameter((ManagedParametersEnum)6);
		float num11 = MathF.Min(num2 / (float)weight, 1f);
		agentDrivenProperties.CombatMaxSpeedMultiplier = MathF.Min(MBMath.Lerp(managedParameter2, managedParameter, num11, 1E-05f), 1f);
		agentDrivenProperties.CrouchedSpeedMultiplier = 1f;
		agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = 0.3f;
		Agent mountAgent = agent.MountAgent;
		float num12 = ((mountAgent != null) ? mountAgent.GetAgentDrivenPropertyValue((DrivenProperty)80) : 1f);
		agentDrivenProperties.AttributeRiding = (float)effectiveSkill2 * num12;
		agentDrivenProperties.AttributeHorseArchery = MissionGameModels.Current.StrikeMagnitudeModel.CalculateHorseArcheryFactor(character);
		agentDrivenProperties.BipedalRangedReadySpeedMultiplier = ManagedParameters.Instance.GetManagedParameter((ManagedParametersEnum)7);
		agentDrivenProperties.BipedalRangedReloadSpeedMultiplier = ManagedParameters.Instance.GetManagedParameter((ManagedParametersEnum)8);
		if (val != null)
		{
			if (val3 != null)
			{
				SetWeaponSkillEffectsOnAgent(agent, val, agentDrivenProperties, val3);
				if (agent.HasMount)
				{
					SetMountedPenaltiesOnAgent(agent, agentDrivenProperties, val3);
				}
			}
			SetPerkAndBannerEffectsOnAgent(agent, val, agentDrivenProperties, val3);
		}
		((AgentStatCalculateModel)this).SetAiRelatedProperties(agent, agentDrivenProperties, val3, val4);
		float num13 = 1f;
		if (!agent.Mission.Scene.IsAtmosphereIndoor)
		{
			float rainDensity = agent.Mission.Scene.GetRainDensity();
			float fog = agent.Mission.Scene.GetFog();
			if (rainDensity > 0f || fog > 0f)
			{
				num13 += MathF.Min(0.3f, rainDensity + fog);
			}
			CampaignTime now = CampaignTime.Now;
			if (((CampaignTime)(ref now)).IsNightTime)
			{
				num13 += 0.1f;
			}
		}
		agentDrivenProperties.AiShooterError *= num13;
	}

	private void UpdateHorseStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Equipment spawnEquipment = agent.SpawnEquipment;
		EquipmentElement val = spawnEquipment[(EquipmentIndex)10];
		ItemObject item = ((EquipmentElement)(ref val)).Item;
		EquipmentElement val2 = spawnEquipment[(EquipmentIndex)11];
		MBGUID id = ((MBObjectBase)item).Id;
		agentDrivenProperties.AiSpeciesIndex = (int)((MBGUID)(ref id)).InternalValue;
		agentDrivenProperties.AttributeRiding = 0.8f + ((((EquipmentElement)(ref val2)).Item != null) ? 0.2f : 0f);
		float num = 0f;
		for (int i = 1; i < 12; i++)
		{
			EquipmentElement val3 = spawnEquipment[i];
			if (((EquipmentElement)(ref val3)).Item != null)
			{
				float num2 = num;
				val3 = spawnEquipment[i];
				num = num2 + (float)((EquipmentElement)(ref val3)).GetModifiedMountBodyArmor();
			}
		}
		agentDrivenProperties.ArmorTorso = num;
		int modifiedMountManeuver = ((EquipmentElement)(ref val)).GetModifiedMountManeuver(ref val2);
		int num3 = ((EquipmentElement)(ref val)).GetModifiedMountSpeed(ref val2) + 1;
		int num4 = 0;
		float environmentSpeedFactor = ((AgentStatCalculateModel)this).GetEnvironmentSpeedFactor(agent);
		MapWeatherModel mapWeatherModel = Campaign.Current.Models.MapWeatherModel;
		CampaignVec2 position = MobileParty.MainParty.Position;
		bool flag = (int)mapWeatherModel.GetWeatherEffectOnTerrainForPosition(((CampaignVec2)(ref position)).ToVec2()) == 1;
		Agent riderAgent = agent.RiderAgent;
		if (riderAgent != null)
		{
			BasicCharacterObject character = riderAgent.Character;
			CharacterObject val4 = (CharacterObject)(object)((character is CharacterObject) ? character : null);
			Formation formation = riderAgent.Formation;
			Agent val5 = ((formation != null) ? formation.Captain : null);
			if (val5 == riderAgent)
			{
				val5 = null;
			}
			BasicCharacterObject obj = ((val5 != null) ? val5.Character : null);
			CharacterObject val6 = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
			BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(formation);
			ExplainedNumber val7 = default(ExplainedNumber);
			((ExplainedNumber)(ref val7))._002Ector((float)modifiedMountManeuver, false, (TextObject)null);
			ExplainedNumber val8 = default(ExplainedNumber);
			((ExplainedNumber)(ref val8))._002Ector((float)num3, false, (TextObject)null);
			num4 = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent.RiderAgent, DefaultSkills.Riding);
			SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.HorseManeuver, ref val7, num4);
			SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.HorseSpeed, ref val8, num4);
			if (activeBanner != null)
			{
				BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMountMovementSpeed, activeBanner, ref val8);
			}
			PerkHelper.AddPerkBonusForCharacter(Riding.NimbleSteed, val4, true, ref val7, false);
			PerkHelper.AddPerkBonusForCharacter(Riding.SweepingWind, val4, true, ref val8, false);
			ExplainedNumber val9 = default(ExplainedNumber);
			((ExplainedNumber)(ref val9))._002Ector(agentDrivenProperties.ArmorTorso, false, (TextObject)null);
			PerkHelper.AddPerkBonusFromCaptain(Riding.ToughSteed, val6, ref val9);
			PerkHelper.AddPerkBonusForCharacter(Riding.ToughSteed, val4, true, ref val9, false);
			if (val4.GetPerkValue(Riding.TheWayOfTheSaddle))
			{
				float num5 = (float)MathF.Max(num4 - Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, 0) * Riding.TheWayOfTheSaddle.PrimaryBonus;
				((ExplainedNumber)(ref val7)).Add(num5, (TextObject)null, (TextObject)null);
			}
			if (((EquipmentElement)(ref val2)).Item == null)
			{
				((ExplainedNumber)(ref val7)).AddFactor(-0.1f, (TextObject)null);
				((ExplainedNumber)(ref val8)).AddFactor(-0.1f, (TextObject)null);
			}
			if (flag)
			{
				((ExplainedNumber)(ref val8)).AddFactor(-0.25f, (TextObject)null);
			}
			agentDrivenProperties.ArmorTorso = ((ExplainedNumber)(ref val9)).ResultNumber;
			agentDrivenProperties.MountManeuver = ((ExplainedNumber)(ref val7)).ResultNumber;
			agentDrivenProperties.MountSpeed = environmentSpeedFactor * 0.22f * (1f + ((ExplainedNumber)(ref val8)).ResultNumber);
		}
		else
		{
			agentDrivenProperties.MountManeuver = modifiedMountManeuver;
			agentDrivenProperties.MountSpeed = environmentSpeedFactor * 0.22f * (float)(1 + num3);
		}
		float num6 = ((EquipmentElement)(ref val)).Weight / 2f + (((EquipmentElement)(ref val2)).IsEmpty ? 0f : ((EquipmentElement)(ref val2)).Weight);
		agentDrivenProperties.MountDashAccelerationMultiplier = ((!(num6 > 200f)) ? 1f : ((num6 < 300f) ? (1f - (num6 - 200f) / 111f) : 0.1f));
		if (flag)
		{
			agentDrivenProperties.MountDashAccelerationMultiplier *= 0.75f;
		}
		agentDrivenProperties.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(ref val, ref val2, num4);
		agentDrivenProperties.MountChargeDamage = (float)((EquipmentElement)(ref val)).GetModifiedMountCharge(ref val2) * 0.004f;
		agentDrivenProperties.MountDifficulty = ((EquipmentElement)(ref val)).Item.Difficulty;
	}

	private void SetPerkAndBannerEffectsOnAgent(Agent agent, CharacterObject agentCharacter, AgentDrivenProperties agentDrivenProperties, WeaponComponentData equippedWeaponComponent)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0792: Unknown result type (might be due to invalid IL or missing references)
		//IL_0797: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Invalid comparison between Unknown and I4
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Invalid comparison between Unknown and I4
		//IL_0a00: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0a: Invalid comparison between Unknown and I4
		//IL_0a0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0f: Invalid comparison between Unknown and I4
		//IL_06c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d9: Invalid comparison between Unknown and I4
		//IL_0a20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a23: Invalid comparison between Unknown and I4
		//IL_0a25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a28: Invalid comparison between Unknown and I4
		//IL_0a2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2d: Invalid comparison between Unknown and I4
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
		CharacterObject val = (CharacterObject)((obj is CharacterObject) ? obj : null);
		Formation formation2 = agent.Formation;
		if (((formation2 != null) ? formation2.Captain : null) == agent)
		{
			val = null;
		}
		ItemObject val2 = null;
		EquipmentIndex offhandWieldedItemIndex = agent.GetOffhandWieldedItemIndex();
		if ((int)offhandWieldedItemIndex != -1)
		{
			MissionWeapon val3 = agent.Equipment[offhandWieldedItemIndex];
			val2 = ((MissionWeapon)(ref val3)).Item;
		}
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(agent.Formation);
		bool flag = equippedWeaponComponent != null && equippedWeaponComponent.IsRangedWeapon;
		bool flag2 = equippedWeaponComponent != null && equippedWeaponComponent.IsMeleeWeapon;
		bool flag3 = val2 != null && val2.PrimaryWeapon.IsShield;
		ExplainedNumber val4 = default(ExplainedNumber);
		((ExplainedNumber)(ref val4))._002Ector(agentDrivenProperties.CombatMaxSpeedMultiplier, false, (TextObject)null);
		ExplainedNumber val5 = default(ExplainedNumber);
		((ExplainedNumber)(ref val5))._002Ector(agentDrivenProperties.MaxSpeedMultiplier, false, (TextObject)null);
		PerkHelper.AddPerkBonusForCharacter(OneHanded.FleetOfFoot, agentCharacter, true, ref val4, false);
		ExplainedNumber val6 = default(ExplainedNumber);
		((ExplainedNumber)(ref val6))._002Ector(agentDrivenProperties.KickStunDurationMultiplier, false, (TextObject)null);
		PerkHelper.AddPerkBonusForCharacter(Roguery.DirtyFighting, agentCharacter, true, ref val6, false);
		agentDrivenProperties.KickStunDurationMultiplier = ((ExplainedNumber)(ref val6)).ResultNumber;
		if (equippedWeaponComponent != null)
		{
			ExplainedNumber val7 = default(ExplainedNumber);
			((ExplainedNumber)(ref val7))._002Ector(agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier, false, (TextObject)null);
			if (flag2)
			{
				ExplainedNumber val8 = default(ExplainedNumber);
				((ExplainedNumber)(ref val8))._002Ector(agentDrivenProperties.SwingSpeedMultiplier, false, (TextObject)null);
				ExplainedNumber val9 = default(ExplainedNumber);
				((ExplainedNumber)(ref val9))._002Ector(agentDrivenProperties.HandlingMultiplier, false, (TextObject)null);
				if (!agent.HasMount)
				{
					PerkHelper.AddPerkBonusForCharacter(Athletics.Fury, agentCharacter, true, ref val9, false);
					if (val != null)
					{
						PerkHelper.AddPerkBonusFromCaptain(Athletics.Fury, val, ref val9);
						PerkHelper.AddPerkBonusFromCaptain(TwoHanded.OnTheEdge, val, ref val8);
						PerkHelper.AddPerkBonusFromCaptain(TwoHanded.BladeMaster, val, ref val8);
						PerkHelper.AddPerkBonusFromCaptain(Polearm.SwiftSwing, val, ref val8);
						PerkHelper.AddPerkBonusFromCaptain(TwoHanded.BladeMaster, val, ref val7);
					}
				}
				if (equippedWeaponComponent.RelevantSkill == DefaultSkills.OneHanded)
				{
					PerkHelper.AddPerkBonusForCharacter(OneHanded.SwiftStrike, agentCharacter, true, ref val8, false);
					PerkHelper.AddEpicPerkBonusForCharacter(OneHanded.WayOfTheSword, agentCharacter, DefaultSkills.OneHanded, true, ref val8, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
					PerkHelper.AddEpicPerkBonusForCharacter(OneHanded.WayOfTheSword, agentCharacter, DefaultSkills.OneHanded, true, ref val7, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
					PerkHelper.AddPerkBonusForCharacter(OneHanded.WrappedHandles, agentCharacter, true, ref val9, false);
				}
				else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.TwoHanded)
				{
					PerkHelper.AddPerkBonusForCharacter(TwoHanded.OnTheEdge, agentCharacter, true, ref val8, false);
					PerkHelper.AddEpicPerkBonusForCharacter(TwoHanded.WayOfTheGreatAxe, agentCharacter, DefaultSkills.TwoHanded, true, ref val8, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
					PerkHelper.AddEpicPerkBonusForCharacter(TwoHanded.WayOfTheGreatAxe, agentCharacter, DefaultSkills.TwoHanded, true, ref val7, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
					PerkHelper.AddPerkBonusForCharacter(TwoHanded.StrongGrip, agentCharacter, true, ref val9, false);
				}
				else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Polearm)
				{
					PerkHelper.AddPerkBonusForCharacter(Polearm.Footwork, agentCharacter, true, ref val4, false);
					PerkHelper.AddPerkBonusForCharacter(Polearm.SwiftSwing, agentCharacter, true, ref val8, false);
					PerkHelper.AddEpicPerkBonusForCharacter(Polearm.WayOfTheSpear, agentCharacter, DefaultSkills.Polearm, true, ref val8, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
					PerkHelper.AddEpicPerkBonusForCharacter(Polearm.WayOfTheSpear, agentCharacter, DefaultSkills.Polearm, true, ref val7, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
					if ((int)equippedWeaponComponent.SwingDamageType != -1)
					{
						PerkHelper.AddPerkBonusForCharacter(Polearm.CounterWeight, agentCharacter, true, ref val9, false);
					}
				}
				agentDrivenProperties.SwingSpeedMultiplier = ((ExplainedNumber)(ref val8)).ResultNumber;
				agentDrivenProperties.HandlingMultiplier = ((ExplainedNumber)(ref val9)).ResultNumber;
			}
			if (flag)
			{
				ExplainedNumber val10 = default(ExplainedNumber);
				((ExplainedNumber)(ref val10))._002Ector(agentDrivenProperties.WeaponInaccuracy, false, (TextObject)null);
				ExplainedNumber val11 = default(ExplainedNumber);
				((ExplainedNumber)(ref val11))._002Ector(agentDrivenProperties.WeaponMaxMovementAccuracyPenalty, false, (TextObject)null);
				ExplainedNumber val12 = default(ExplainedNumber);
				((ExplainedNumber)(ref val12))._002Ector(agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty, false, (TextObject)null);
				ExplainedNumber val13 = default(ExplainedNumber);
				((ExplainedNumber)(ref val13))._002Ector(agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians, false, (TextObject)null);
				ExplainedNumber val14 = default(ExplainedNumber);
				((ExplainedNumber)(ref val14))._002Ector(agentDrivenProperties.WeaponUnsteadyBeginTime, false, (TextObject)null);
				ExplainedNumber val15 = default(ExplainedNumber);
				((ExplainedNumber)(ref val15))._002Ector(agentDrivenProperties.WeaponUnsteadyEndTime, false, (TextObject)null);
				ExplainedNumber val16 = default(ExplainedNumber);
				((ExplainedNumber)(ref val16))._002Ector(agentDrivenProperties.ReloadMovementPenaltyFactor, false, (TextObject)null);
				ExplainedNumber val17 = default(ExplainedNumber);
				((ExplainedNumber)(ref val17))._002Ector(agentDrivenProperties.ReloadSpeed, false, (TextObject)null);
				ExplainedNumber val18 = default(ExplainedNumber);
				((ExplainedNumber)(ref val18))._002Ector(agentDrivenProperties.MissileSpeedMultiplier, false, (TextObject)null);
				PerkHelper.AddPerkBonusForCharacter(Bow.NockingPoint, agentCharacter, true, ref val16, false);
				if (val != null)
				{
					PerkHelper.AddPerkBonusFromCaptain(Crossbow.LooseAndMove, val, ref val5);
				}
				if (activeBanner != null)
				{
					BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedRangedAccuracyPenalty, activeBanner, ref val10);
				}
				if (agent.HasMount)
				{
					if (agentCharacter.GetPerkValue(Riding.Sagittarius))
					{
						PerkHelper.AddPerkBonusForCharacter(Riding.Sagittarius, agentCharacter, true, ref val11, false);
						PerkHelper.AddPerkBonusForCharacter(Riding.Sagittarius, agentCharacter, true, ref val12, false);
					}
					if (val != null && val.GetPerkValue(Riding.Sagittarius))
					{
						PerkHelper.AddPerkBonusFromCaptain(Riding.Sagittarius, val, ref val11);
						PerkHelper.AddPerkBonusFromCaptain(Riding.Sagittarius, val, ref val12);
					}
					if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Bow && agentCharacter.GetPerkValue(Bow.MountedArchery))
					{
						PerkHelper.AddPerkBonusForCharacter(Bow.MountedArchery, agentCharacter, true, ref val11, false);
						PerkHelper.AddPerkBonusForCharacter(Bow.MountedArchery, agentCharacter, true, ref val12, false);
					}
					if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Throwing && agentCharacter.GetPerkValue(Throwing.MountedSkirmisher))
					{
						PerkHelper.AddPerkBonusForCharacter(Throwing.MountedSkirmisher, agentCharacter, true, ref val11, false);
						PerkHelper.AddPerkBonusForCharacter(Throwing.MountedSkirmisher, agentCharacter, true, ref val12, false);
					}
				}
				bool flag4 = false;
				if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Bow)
				{
					flag4 = true;
					PerkHelper.AddPerkBonusForCharacter(Bow.BowControl, agentCharacter, true, ref val11, false);
					PerkHelper.AddPerkBonusForCharacter(Bow.RapidFire, agentCharacter, true, ref val17, false);
					PerkHelper.AddPerkBonusForCharacter(Bow.QuickAdjustments, agentCharacter, true, ref val13, false);
					PerkHelper.AddPerkBonusForCharacter(Bow.Discipline, agentCharacter, true, ref val14, false);
					PerkHelper.AddPerkBonusForCharacter(Bow.Discipline, agentCharacter, true, ref val15, false);
					PerkHelper.AddPerkBonusForCharacter(Bow.QuickDraw, agentCharacter, true, ref val7, false);
					if (val != null)
					{
						PerkHelper.AddPerkBonusFromCaptain(Bow.RapidFire, val, ref val17);
						if (!agent.HasMount)
						{
							PerkHelper.AddPerkBonusFromCaptain(Bow.NockingPoint, val, ref val5);
						}
					}
					PerkHelper.AddEpicPerkBonusForCharacter(Bow.Deadshot, agentCharacter, DefaultSkills.Bow, true, ref val17, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus, false);
				}
				else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Crossbow)
				{
					flag4 = true;
					if (agent.HasMount)
					{
						PerkHelper.AddPerkBonusForCharacter(Crossbow.Steady, agentCharacter, true, ref val11, false);
						PerkHelper.AddPerkBonusForCharacter(Crossbow.Steady, agentCharacter, true, ref val13, false);
					}
					PerkHelper.AddPerkBonusForCharacter(Crossbow.WindWinder, agentCharacter, true, ref val17, false);
					if (val != null)
					{
						PerkHelper.AddPerkBonusFromCaptain(Crossbow.WindWinder, val, ref val17);
					}
					PerkHelper.AddPerkBonusForCharacter(Crossbow.DonkeysSwiftness, agentCharacter, true, ref val11, false);
					PerkHelper.AddPerkBonusForCharacter(Crossbow.Marksmen, agentCharacter, true, ref val7, false);
					PerkHelper.AddEpicPerkBonusForCharacter(Crossbow.MightyPull, agentCharacter, DefaultSkills.Crossbow, true, ref val17, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus, false);
				}
				else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Throwing)
				{
					PerkHelper.AddPerkBonusForCharacter(Throwing.QuickDraw, agentCharacter, true, ref val17, false);
					PerkHelper.AddPerkBonusForCharacter(Throwing.PerfectTechnique, agentCharacter, true, ref val18, false);
					if (val != null)
					{
						PerkHelper.AddPerkBonusFromCaptain(Throwing.QuickDraw, val, ref val17);
						PerkHelper.AddPerkBonusFromCaptain(Throwing.PerfectTechnique, val, ref val18);
					}
					PerkHelper.AddEpicPerkBonusForCharacter(Throwing.UnstoppableForce, agentCharacter, DefaultSkills.Throwing, true, ref val18, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus, false);
				}
				if (flag4)
				{
					MapWeatherModel mapWeatherModel = Campaign.Current.Models.MapWeatherModel;
					CampaignVec2 position = MobileParty.MainParty.Position;
					if ((int)mapWeatherModel.GetWeatherEffectOnTerrainForPosition(((CampaignVec2)(ref position)).ToVec2()) == 1)
					{
						((ExplainedNumber)(ref val18)).AddFactor(-0.2f, (TextObject)null);
					}
				}
				agentDrivenProperties.ReloadMovementPenaltyFactor = ((ExplainedNumber)(ref val16)).ResultNumber;
				agentDrivenProperties.ReloadSpeed = ((ExplainedNumber)(ref val17)).ResultNumber;
				agentDrivenProperties.MissileSpeedMultiplier = ((ExplainedNumber)(ref val18)).ResultNumber;
				agentDrivenProperties.WeaponInaccuracy = ((ExplainedNumber)(ref val10)).ResultNumber;
				agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = ((ExplainedNumber)(ref val11)).ResultNumber;
				agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = ((ExplainedNumber)(ref val12)).ResultNumber;
				agentDrivenProperties.WeaponUnsteadyBeginTime = ((ExplainedNumber)(ref val14)).ResultNumber;
				agentDrivenProperties.WeaponUnsteadyEndTime = ((ExplainedNumber)(ref val15)).ResultNumber;
				agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = ((ExplainedNumber)(ref val13)).ResultNumber;
			}
			agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = ((ExplainedNumber)(ref val7)).ResultNumber;
		}
		if (flag3)
		{
			ExplainedNumber val19 = default(ExplainedNumber);
			((ExplainedNumber)(ref val19))._002Ector(agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder, false, (TextObject)null);
			if (val != null)
			{
				Formation formation3 = agent.Formation;
				if (formation3 != null && (int)formation3.ArrangementOrder.OrderEnum == 5)
				{
					PerkHelper.AddPerkBonusFromCaptain(OneHanded.ShieldWall, val, ref val19);
				}
				PerkHelper.AddPerkBonusFromCaptain(OneHanded.ArrowCatcher, val, ref val19);
			}
			PerkHelper.AddPerkBonusForCharacter(OneHanded.ArrowCatcher, agentCharacter, true, ref val19, false);
			agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = ((ExplainedNumber)(ref val19)).ResultNumber;
			ExplainedNumber val20 = default(ExplainedNumber);
			((ExplainedNumber)(ref val20))._002Ector(agentDrivenProperties.ShieldBashStunDurationMultiplier, false, (TextObject)null);
			PerkHelper.AddPerkBonusForCharacter(OneHanded.Basher, agentCharacter, true, ref val20, false);
			agentDrivenProperties.ShieldBashStunDurationMultiplier = ((ExplainedNumber)(ref val20)).ResultNumber;
		}
		else
		{
			PerkHelper.AddPerkBonusForCharacter(Athletics.MorningExercise, agentCharacter, true, ref val5, false);
			PerkHelper.AddPerkBonusForCharacter(Medicine.SelfMedication, agentCharacter, false, ref val5, false);
			if (!(flag3 || flag))
			{
				PerkHelper.AddPerkBonusForCharacter(Athletics.Sprint, agentCharacter, true, ref val5, false);
			}
			if (equippedWeaponComponent == null && val2 == null)
			{
				PerkHelper.AddPerkBonusForCharacter(Roguery.FleetFooted, agentCharacter, true, ref val5, false);
			}
			if (val != null)
			{
				PerkHelper.AddPerkBonusFromCaptain(Athletics.MorningExercise, val, ref val5);
				PerkHelper.AddPerkBonusFromCaptain(OneHanded.ShieldBearer, val, ref val5);
				PerkHelper.AddPerkBonusFromCaptain(OneHanded.FleetOfFoot, val, ref val5);
				PerkHelper.AddPerkBonusFromCaptain(TwoHanded.RecklessCharge, val, ref val5);
				PerkHelper.AddPerkBonusFromCaptain(Polearm.Footwork, val, ref val5);
				if (agentCharacter.Tier >= 3)
				{
					PerkHelper.AddPerkBonusFromCaptain(Athletics.FormFittingArmor, val, ref val5);
				}
				if (((BasicCharacterObject)agentCharacter).IsInfantry)
				{
					PerkHelper.AddPerkBonusFromCaptain(Athletics.Sprint, val, ref val5);
				}
			}
		}
		float num = 0f;
		float num2 = 0f;
		bool flag5 = false;
		if (val != null)
		{
			if (agent.HasMount && val.GetPerkValue(Riding.DauntlessSteed))
			{
				num += Riding.DauntlessSteed.SecondaryBonus;
				flag5 = true;
			}
			else if (!agent.HasMount && val.GetPerkValue(Athletics.IgnorePain))
			{
				num += Athletics.IgnorePain.SecondaryBonus;
				flag5 = true;
			}
			if (val.GetPerkValue(Engineering.Metallurgy))
			{
				num += Engineering.Metallurgy.SecondaryBonus;
				flag5 = true;
			}
		}
		if (!agent.HasMount && agentCharacter.GetPerkValue(Athletics.IgnorePain))
		{
			num2 += Athletics.IgnorePain.PrimaryBonus;
			flag5 = true;
		}
		if (flag5)
		{
			float num3 = 1f + num2;
			agentDrivenProperties.ArmorHead = MathF.Max(0f, (agentDrivenProperties.ArmorHead + num) * num3);
			agentDrivenProperties.ArmorTorso = MathF.Max(0f, (agentDrivenProperties.ArmorTorso + num) * num3);
			agentDrivenProperties.ArmorArms = MathF.Max(0f, (agentDrivenProperties.ArmorArms + num) * num3);
			agentDrivenProperties.ArmorLegs = MathF.Max(0f, (agentDrivenProperties.ArmorLegs + num) * num3);
		}
		if (Mission.Current != null && Mission.Current.HasValidTerrainType)
		{
			TerrainType terrainType = Mission.Current.TerrainType;
			if ((int)terrainType == 3 || (int)terrainType == 4)
			{
				PerkHelper.AddPerkBonusFromCaptain(Tactics.ExtendedSkirmish, val, ref val5);
			}
			else if ((int)terrainType == 1 || (int)terrainType == 5 || (int)terrainType == 2)
			{
				PerkHelper.AddPerkBonusFromCaptain(Tactics.DecisiveBattle, val, ref val5);
			}
		}
		if (agentCharacter.Tier >= 3 && ((BasicCharacterObject)agentCharacter).IsInfantry)
		{
			PerkHelper.AddPerkBonusFromCaptain(Athletics.FormFittingArmor, val, ref val5);
		}
		if (agent.Formation != null && agent.Formation.CountOfUnits <= 15)
		{
			PerkHelper.AddPerkBonusFromCaptain(Tactics.SmallUnitTactics, val, ref val5);
		}
		if (activeBanner != null)
		{
			BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedTroopMovementSpeed, activeBanner, ref val5);
		}
		agentDrivenProperties.MaxSpeedMultiplier = ((ExplainedNumber)(ref val5)).ResultNumber;
		agentDrivenProperties.CombatMaxSpeedMultiplier = ((ExplainedNumber)(ref val4)).ResultNumber;
	}

	private void SetWeaponSkillEffectsOnAgent(Agent agent, CharacterObject agentCharacter, AgentDrivenProperties agentDrivenProperties, WeaponComponentData equippedWeaponComponent)
	{
		if (equippedWeaponComponent != null)
		{
			int effectiveSkill = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, equippedWeaponComponent.RelevantSkill);
			ExplainedNumber val = default(ExplainedNumber);
			((ExplainedNumber)(ref val))._002Ector(agentDrivenProperties.SwingSpeedMultiplier, false, (TextObject)null);
			ExplainedNumber val2 = default(ExplainedNumber);
			((ExplainedNumber)(ref val2))._002Ector(agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier, false, (TextObject)null);
			ExplainedNumber val3 = default(ExplainedNumber);
			((ExplainedNumber)(ref val3))._002Ector(agentDrivenProperties.ReloadSpeed, false, (TextObject)null);
			if (equippedWeaponComponent.RelevantSkill == DefaultSkills.OneHanded)
			{
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.OneHandedSpeed, ref val, effectiveSkill);
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.OneHandedSpeed, ref val2, effectiveSkill);
			}
			else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.TwoHanded)
			{
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.TwoHandedSpeed, ref val, effectiveSkill);
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.TwoHandedSpeed, ref val2, effectiveSkill);
			}
			else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Polearm)
			{
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.PolearmSpeed, ref val, effectiveSkill);
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.PolearmSpeed, ref val2, effectiveSkill);
			}
			else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Crossbow)
			{
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.CrossbowReloadSpeed, ref val3, effectiveSkill);
			}
			else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Throwing)
			{
				SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.ThrowingSpeed, ref val2, effectiveSkill);
			}
			agentDrivenProperties.SwingSpeedMultiplier = ((ExplainedNumber)(ref val)).ResultNumber;
			agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = ((ExplainedNumber)(ref val2)).ResultNumber;
			agentDrivenProperties.ReloadSpeed = ((ExplainedNumber)(ref val3)).ResultNumber;
		}
		int effectiveSkill2 = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, DefaultSkills.Roguery);
		ExplainedNumber val4 = default(ExplainedNumber);
		((ExplainedNumber)(ref val4))._002Ector(1f, false, (TextObject)null);
		SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.CrouchedSpeed, ref val4, effectiveSkill2);
		agentDrivenProperties.CrouchedSpeedMultiplier = ((ExplainedNumber)(ref val4)).ResultNumber;
	}

	private void SetMountedPenaltiesOnAgent(Agent agent, AgentDrivenProperties agentDrivenProperties, WeaponComponentData equippedWeaponComponent)
	{
		int effectiveSkill = ((AgentStatCalculateModel)this).GetEffectiveSkill(agent, DefaultSkills.Riding);
		float skillEffectValue = DefaultSkillEffects.MountedWeaponSpeedPenalty.GetSkillEffectValue(effectiveSkill);
		if (skillEffectValue < 0f)
		{
			ExplainedNumber val = default(ExplainedNumber);
			((ExplainedNumber)(ref val))._002Ector(agentDrivenProperties.WeaponBestAccuracyWaitTime, false, (TextObject)null);
			ExplainedNumber val2 = default(ExplainedNumber);
			((ExplainedNumber)(ref val2))._002Ector(agentDrivenProperties.SwingSpeedMultiplier, false, (TextObject)null);
			ExplainedNumber val3 = default(ExplainedNumber);
			((ExplainedNumber)(ref val3))._002Ector(agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier, false, (TextObject)null);
			ExplainedNumber val4 = default(ExplainedNumber);
			((ExplainedNumber)(ref val4))._002Ector(agentDrivenProperties.ReloadSpeed, false, (TextObject)null);
			SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.MountedWeaponSpeedPenalty, ref val2, effectiveSkill);
			SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.MountedWeaponSpeedPenalty, ref val3, effectiveSkill);
			SkillHelper.AddSkillBonusForSkillLevel(DefaultSkillEffects.MountedWeaponSpeedPenalty, ref val4, effectiveSkill);
			((ExplainedNumber)(ref val)).AddFactor(-1f * skillEffectValue, (TextObject)null);
			agentDrivenProperties.SwingSpeedMultiplier = Math.Max(0f, ((ExplainedNumber)(ref val2)).ResultNumber);
			agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = Math.Max(0f, ((ExplainedNumber)(ref val3)).ResultNumber);
			agentDrivenProperties.ReloadSpeed = Math.Max(0f, ((ExplainedNumber)(ref val4)).ResultNumber);
			agentDrivenProperties.WeaponBestAccuracyWaitTime = Math.Max(0f, ((ExplainedNumber)(ref val)).ResultNumber);
		}
		float num = 5f - (float)effectiveSkill * 0.05f;
		if (num > 0f)
		{
			ExplainedNumber val5 = default(ExplainedNumber);
			((ExplainedNumber)(ref val5))._002Ector(agentDrivenProperties.WeaponInaccuracy, false, (TextObject)null);
			((ExplainedNumber)(ref val5)).AddFactor(num, (TextObject)null);
			agentDrivenProperties.WeaponInaccuracy = Math.Max(0f, ((ExplainedNumber)(ref val5)).ResultNumber);
		}
	}

	public static float CalculateMaximumSpeedMultiplier(int athletics, float baseWeight, float totalEncumbrance)
	{
		return MathF.Min((200f + (float)athletics) / 300f * (baseWeight * 2f / (baseWeight * 2f + totalEncumbrance)), 1f);
	}
}
