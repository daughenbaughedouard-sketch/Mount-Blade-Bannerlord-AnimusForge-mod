using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace SandBox.GameComponents;

public class SandboxBattleMoraleModel : BattleMoraleModel
{
	public override (float affectedSideMaxMoraleLoss, float affectorSideMaxMoraleGain) CalculateMaxMoraleChangeDueToAgentIncapacitated(Agent affectedAgent, AgentState affectedAgentState, Agent affectorAgent, in KillingBlow killingBlow)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Invalid comparison between Unknown and I4
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Invalid comparison between Unknown and I4
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		float battleImportance = affectedAgent.GetBattleImportance();
		Team team = affectedAgent.Team;
		BattleSideEnum val = (BattleSideEnum)((team == null) ? (-1) : ((int)team.Side));
		float num = ((BattleMoraleModel)this).CalculateCasualtiesFactor(val);
		BasicCharacterObject obj = ((affectorAgent != null) ? affectorAgent.Character : null);
		CharacterObject val2 = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		BasicCharacterObject obj2 = ((affectedAgent != null) ? affectedAgent.Character : null);
		BasicCharacterObject obj3 = ((obj2 is CharacterObject) ? obj2 : null);
		SkillObject relevantSkillFromWeaponClass = WeaponComponentData.GetRelevantSkillFromWeaponClass((WeaponClass)killingBlow.WeaponClass);
		bool flag = relevantSkillFromWeaponClass == DefaultSkills.OneHanded || relevantSkillFromWeaponClass == DefaultSkills.TwoHanded || relevantSkillFromWeaponClass == DefaultSkills.Polearm;
		bool flag2 = relevantSkillFromWeaponClass == DefaultSkills.Bow || relevantSkillFromWeaponClass == DefaultSkills.Crossbow || relevantSkillFromWeaponClass == DefaultSkills.Throwing;
		bool num2 = Extensions.HasAnyFlag<WeaponFlags>(killingBlow.WeaponRecordWeaponFlags, (WeaponFlags)1073766400);
		float num3 = 0.75f;
		if (num2)
		{
			num3 = 0.25f;
			if (Extensions.HasAllFlags<WeaponFlags>(killingBlow.WeaponRecordWeaponFlags, (WeaponFlags)1073774592))
			{
				num3 += num3 * 0.25f;
			}
		}
		else if (flag2)
		{
			num3 = 0.5f;
		}
		num3 = Math.Max(0f, num3);
		ExplainedNumber val3 = default(ExplainedNumber);
		((ExplainedNumber)(ref val3))._002Ector(battleImportance * 3f * num3, false, (TextObject)null);
		ExplainedNumber val4 = default(ExplainedNumber);
		((ExplainedNumber)(ref val4))._002Ector(battleImportance * 4f * num3 * num, false, (TextObject)null);
		if (val2 != null)
		{
			object obj4;
			if (affectorAgent == null)
			{
				obj4 = null;
			}
			else
			{
				Formation formation = affectorAgent.Formation;
				if (formation == null)
				{
					obj4 = null;
				}
				else
				{
					Agent captain = formation.Captain;
					obj4 = ((captain != null) ? captain.Character : null);
				}
			}
			CharacterObject val5 = (CharacterObject)((obj4 is CharacterObject) ? obj4 : null);
			PerkHelper.AddPerkBonusForCharacter(Leadership.MakeADifference, val2, true, ref val3, false);
			if (flag)
			{
				if (relevantSkillFromWeaponClass == DefaultSkills.TwoHanded)
				{
					PerkHelper.AddPerkBonusForCharacter(TwoHanded.Hope, val2, true, ref val3, false);
					PerkHelper.AddPerkBonusForCharacter(TwoHanded.Terror, val2, true, ref val4, false);
				}
				if (affectorAgent != null && affectorAgent.HasMount)
				{
					PerkHelper.AddPerkBonusForCharacter(Riding.ThunderousCharge, val2, true, ref val4, false);
					PerkHelper.AddPerkBonusFromCaptain(Riding.ThunderousCharge, val5, ref val4);
				}
			}
			else if (flag2)
			{
				if (relevantSkillFromWeaponClass == DefaultSkills.Crossbow)
				{
					PerkHelper.AddPerkBonusFromCaptain(Crossbow.Terror, val5, ref val4);
				}
				if (affectorAgent != null && affectorAgent.HasMount)
				{
					PerkHelper.AddPerkBonusForCharacter(Riding.AnnoyingBuzz, val2, true, ref val4, false);
					PerkHelper.AddPerkBonusFromCaptain(Riding.AnnoyingBuzz, val5, ref val4);
				}
			}
			PerkHelper.AddPerkBonusFromCaptain(Leadership.HeroicLeader, val5, ref val4);
		}
		if (obj3 != null)
		{
			object obj5;
			if (affectedAgent == null)
			{
				obj5 = null;
			}
			else
			{
				IAgentOriginBase origin = affectedAgent.Origin;
				obj5 = ((origin != null) ? origin.BattleCombatant : null);
			}
			object obj6 = ((obj5 is PartyBase) ? obj5 : null);
			MobileParty val6 = ((obj6 != null) ? ((PartyBase)obj6).MobileParty : null);
			if ((int)affectedAgentState == 3 && val6 != null && val6.HasPerk(Medicine.HealthAdvise, true))
			{
				val4 = default(ExplainedNumber);
			}
			else
			{
				Formation formation2 = affectedAgent.Formation;
				object obj7;
				if (formation2 == null)
				{
					obj7 = null;
				}
				else
				{
					Agent captain2 = formation2.Captain;
					obj7 = ((captain2 != null) ? captain2.Character : null);
				}
				CharacterObject val7;
				if ((val7 = (CharacterObject)((obj7 is CharacterObject) ? obj7 : null)) != null)
				{
					ArrangementOrder arrangementOrder = affectedAgent.Formation.ArrangementOrder;
					if (arrangementOrder == ArrangementOrder.ArrangementOrderShieldWall || arrangementOrder == ArrangementOrder.ArrangementOrderSquare || arrangementOrder == ArrangementOrder.ArrangementOrderSkein || arrangementOrder == ArrangementOrder.ArrangementOrderColumn)
					{
						PerkHelper.AddPerkBonusFromCaptain(Tactics.TightFormations, val7, ref val4);
					}
					if (arrangementOrder == ArrangementOrder.ArrangementOrderLine || arrangementOrder == ArrangementOrder.ArrangementOrderLoose || arrangementOrder == ArrangementOrder.ArrangementOrderCircle || arrangementOrder == ArrangementOrder.ArrangementOrderScatter)
					{
						PerkHelper.AddPerkBonusFromCaptain(Tactics.LooseFormations, val7, ref val4);
					}
					PerkHelper.AddPerkBonusFromCaptain(Polearm.StandardBearer, val7, ref val4);
				}
				Hero val8 = ((val6 != null) ? val6.EffectiveQuartermaster : null);
				if (val8 != null)
				{
					PerkHelper.AddEpicPerkBonusForCharacter(Steward.PriceOfLoyalty, val8.CharacterObject, DefaultSkills.Steward, true, ref val4, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
				}
			}
		}
		Formation formation3 = affectedAgent.Formation;
		BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(formation3);
		if (activeBanner != null)
		{
			BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedMoraleShock, activeBanner, ref val4);
		}
		Formation formation4 = affectorAgent.Formation;
		BannerComponent activeBanner2 = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(formation4);
		if (activeBanner2 != null && (int)affectorAgent.Character.DefaultFormationClass == 0 && flag)
		{
			BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMoraleShockByMeleeTroops, activeBanner2, ref val3);
		}
		return (affectedSideMaxMoraleLoss: MathF.Max(((ExplainedNumber)(ref val4)).ResultNumber, 0f), affectorSideMaxMoraleGain: MathF.Max(((ExplainedNumber)(ref val3)).ResultNumber, 0f));
	}

	public override (float affectedSideMaxMoraleLoss, float affectorSideMaxMoraleGain) CalculateMaxMoraleChangeDueToAgentPanicked(Agent agent)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float battleImportance = agent.GetBattleImportance();
		Team team = agent.Team;
		BattleSideEnum val = (BattleSideEnum)((team == null) ? (-1) : ((int)team.Side));
		float num = ((BattleMoraleModel)this).CalculateCasualtiesFactor(val);
		float num2 = battleImportance * 2f;
		float num3 = battleImportance * num * 1.1f;
		if (((agent != null) ? agent.Character : null) is CharacterObject)
		{
			ExplainedNumber val2 = default(ExplainedNumber);
			((ExplainedNumber)(ref val2))._002Ector(num3, false, (TextObject)null);
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
			CharacterObject val3 = (CharacterObject)((obj is CharacterObject) ? obj : null);
			BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(formation);
			if (val3 != null)
			{
				PerkHelper.AddPerkBonusFromCaptain(Polearm.StandardBearer, val3, ref val2);
			}
			object obj2;
			if (agent == null)
			{
				obj2 = null;
			}
			else
			{
				IAgentOriginBase origin = agent.Origin;
				obj2 = ((origin != null) ? origin.BattleCombatant : null);
			}
			object obj3 = ((obj2 is PartyBase) ? obj2 : null);
			MobileParty obj4 = ((obj3 != null) ? ((PartyBase)obj3).MobileParty : null);
			Hero val4 = ((obj4 != null) ? obj4.EffectiveQuartermaster : null);
			if (val4 != null)
			{
				PerkHelper.AddEpicPerkBonusForCharacter(Steward.PriceOfLoyalty, val4.CharacterObject, DefaultSkills.Steward, true, ref val2, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, false);
			}
			if (activeBanner != null)
			{
				BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedMoraleShock, activeBanner, ref val2);
			}
			num3 = ((ExplainedNumber)(ref val2)).ResultNumber;
		}
		return (affectedSideMaxMoraleLoss: MathF.Max(num3, 0f), affectorSideMaxMoraleGain: MathF.Max(num2, 0f));
	}

	public override float CalculateMoraleChangeToCharacter(Agent agent, float maxMoraleChange)
	{
		return maxMoraleChange / MathF.Max(1f, agent.Character.GetMoraleResistance());
	}

	public override float GetEffectiveInitialMorale(Agent agent, float baseMorale)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Invalid comparison between Unknown and I4
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Invalid comparison between Unknown and I4
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(baseMorale, false, (TextObject)null);
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
		MobileParty val3 = ((val2 != null && val2.IsMobile) ? val2.MobileParty : null);
		BasicCharacterObject obj2 = ((agent != null) ? agent.Character : null);
		CharacterObject val4 = (CharacterObject)(object)((obj2 is CharacterObject) ? obj2 : null);
		if (val3 != null && val4 != null)
		{
			Army army = val3.Army;
			object obj3;
			if (army == null)
			{
				obj3 = null;
			}
			else
			{
				MobileParty leaderParty = army.LeaderParty;
				if (leaderParty == null)
				{
					obj3 = null;
				}
				else
				{
					Hero leaderHero = leaderParty.LeaderHero;
					obj3 = ((leaderHero != null) ? leaderHero.CharacterObject : null);
				}
			}
			CharacterObject val5 = (CharacterObject)obj3;
			Hero leaderHero2 = val3.LeaderHero;
			CharacterObject val6 = ((leaderHero2 != null) ? leaderHero2.CharacterObject : null);
			val5 = ((val5 != val4) ? val5 : null);
			val6 = ((val6 != val4) ? val6 : null);
			if (val6 != null)
			{
				if ((int)val2.Side == 1)
				{
					PerkHelper.AddPerkBonusForParty(Leadership.FerventAttacker, val3, true, ref val, false);
				}
				else if ((int)val2.Side == 0)
				{
					PerkHelper.AddPerkBonusForParty(Leadership.StoutDefender, val3, true, ref val, false);
				}
				if (val6.Culture == val4.Culture)
				{
					PerkHelper.AddPerkBonusForParty(Leadership.GreatLeader, val3, false, ref val, false);
				}
				if (val6.GetPerkValue(Leadership.WePledgeOurSwords))
				{
					int num = MathF.Min(val2.GetNumberOfHealthyMenOfTier(6), 10);
					((ExplainedNumber)(ref val)).Add((float)num, (TextObject)null, (TextObject)null);
				}
				PerkHelper.AddPerkBonusForParty(Throwing.LastHit, val3, false, ref val, false);
				object obj4;
				if (val2 == null)
				{
					obj4 = null;
				}
				else
				{
					MapEventSide mapEventSide = val2.MapEventSide;
					obj4 = ((mapEventSide != null) ? mapEventSide.LeaderParty : null);
				}
				PartyBase val7 = (PartyBase)obj4;
				if (val7 != null && val2 != val7)
				{
					PerkHelper.AddPerkBonusForParty(Riding.ReliefForce, val3, true, ref val, false);
				}
				if (val2.MapEvent != null)
				{
					float num2 = default(float);
					float num3 = default(float);
					val2.MapEvent.GetStrengthsRelativeToParty(val2.Side, ref num2, ref num3);
					if (num2 < num3)
					{
						PerkHelper.AddPerkBonusForParty(OneHanded.StandUnited, val3, true, ref val, false);
					}
					if (val2.MapEvent.IsSiegeAssault || val2.MapEvent.IsSiegeOutside)
					{
						PerkHelper.AddPerkBonusForParty(Leadership.UpliftingSpirit, val3, true, ref val, false);
					}
					bool flag = false;
					foreach (PartyBase involvedParty in val2.MapEvent.InvolvedParties)
					{
						if (involvedParty.Side != val2.Side && involvedParty.MapFaction != null && ((BasicCultureObject)involvedParty.Culture).IsBandit)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						PerkHelper.AddPerkBonusForParty(Scouting.Patrols, val3, true, ref val, false);
					}
				}
				PerkHelper.AddPerkBonusForParty(OneHanded.LeadByExample, val3, false, ref val, val3.IsCurrentlyAtSea);
			}
			if (val5 != null && val5.GetPerkValue(Leadership.GreatLeader))
			{
				((ExplainedNumber)(ref val)).Add(Leadership.GreatLeader.PrimaryBonus, (TextObject)null, (TextObject)null);
			}
			if (((BasicCharacterObject)val4).IsRanged)
			{
				PerkHelper.AddPerkBonusForParty(Bow.RenownedArcher, val2.MobileParty, true, ref val, false);
				PerkHelper.AddPerkBonusForParty(Crossbow.Marksmen, val2.MobileParty, false, ref val, false);
			}
			if (val3.IsDisorganized && (val3.MapEvent == null || val3.SiegeEvent == null || (int)val3.MapEventSide.MissionSide != 1) && (val6 == null || !val6.GetPerkValue(Tactics.Improviser)))
			{
				((ExplainedNumber)(ref val)).AddFactor(-0.2f, (TextObject)null);
			}
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override bool CanPanicDueToMorale(Agent agent)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		if (agent.IsHuman)
		{
			BasicCharacterObject character = agent.Character;
			CharacterObject val = (CharacterObject)(object)((character is CharacterObject) ? character : null);
			IAgentOriginBase origin = agent.Origin;
			PartyBase val2 = (PartyBase)((origin != null) ? origin.BattleCombatant : null);
			Hero val3 = (((int)val2 != 0) ? val2.LeaderHero : null);
			if (val != null && val3 != null && val.Tier >= (int)Leadership.LoyaltyAndHonor.PrimaryBonus && val3.GetPerkValue(Leadership.LoyaltyAndHonor))
			{
				result = false;
			}
		}
		return result;
	}

	public override float CalculateCasualtiesFactor(BattleSideEnum battleSide)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		if (Mission.Current != null && (int)battleSide != -1)
		{
			float removedAgentRatioForSide = Mission.Current.GetRemovedAgentRatioForSide(battleSide);
			num += removedAgentRatioForSide * 2f;
			num = MathF.Max(0f, num);
		}
		return num;
	}

	public override float GetAverageMorale(Formation formation)
	{
		float num = 0f;
		int num2 = 0;
		if (formation != null)
		{
			foreach (IFormationUnit item in (List<IFormationUnit>)(object)formation.Arrangement.GetAllUnits())
			{
				Agent val;
				if ((val = (Agent)(object)((item is Agent) ? item : null)) != null && val.IsHuman && val.IsAIControlled)
				{
					num2++;
					num += AgentComponentExtensions.GetMorale(val);
				}
			}
		}
		if (num2 > 0)
		{
			return MBMath.ClampFloat(num / (float)num2, 0f, 100f);
		}
		return 0f;
	}

	public override float CalculateMoraleChangeOnShipSunk(IShipOrigin shipOrigin)
	{
		return 0f;
	}

	public override float CalculateMoraleOnRamming(Agent agent, IShipOrigin rammingShip, IShipOrigin rammedShip)
	{
		return AgentComponentExtensions.GetMorale(agent);
	}

	public override float CalculateMoraleOnShipsConnected(Agent agent, IShipOrigin ownerShip, IShipOrigin targetShip)
	{
		return AgentComponentExtensions.GetMorale(agent);
	}
}
