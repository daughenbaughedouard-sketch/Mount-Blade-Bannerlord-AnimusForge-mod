using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace NavalDLC.GameComponents;

public class NavalBattleMoraleModel : BattleMoraleModel
{
	private NavalShipsLogic GetNavalShipsLogic()
	{
		return Mission.Current.GetMissionBehavior<NavalShipsLogic>();
	}

	public override (float affectedSideMaxMoraleLoss, float affectorSideMaxMoraleGain) CalculateMaxMoraleChangeDueToAgentIncapacitated(Agent affectedAgent, AgentState affectedAgentState, Agent affectorAgent, in KillingBlow killingBlow)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		var (num, num2) = ((MBGameModel<BattleMoraleModel>)this).BaseModel.CalculateMaxMoraleChangeDueToAgentIncapacitated(affectedAgent, affectedAgentState, affectorAgent, ref killingBlow);
		if (Mission.Current.IsNavalBattle)
		{
			ExplainedNumber val = default(ExplainedNumber);
			((ExplainedNumber)(ref val))._002Ector(num2, false, (TextObject)null);
			ExplainedNumber val2 = default(ExplainedNumber);
			((ExplainedNumber)(ref val2))._002Ector(num, false, (TextObject)null);
			if (((affectorAgent != null) ? affectorAgent.Character : null) is CharacterObject)
			{
				object obj;
				if (affectorAgent == null)
				{
					obj = null;
				}
				else
				{
					Formation formation = affectorAgent.Formation;
					if (formation == null)
					{
						obj = null;
					}
					else
					{
						Agent captain = formation.Captain;
						obj = ((captain != null) ? captain.Character : null);
					}
				}
				CharacterObject val3 = (CharacterObject)((obj is CharacterObject) ? obj : null);
				if (val3 != null && val3.GetPerkValue(NavalPerks.Mariner.TerrorOfTheSeas))
				{
					((ExplainedNumber)(ref val2)).AddFactor(NavalPerks.Mariner.TerrorOfTheSeas.PrimaryBonus, (TextObject)null);
				}
			}
			return (affectedSideMaxMoraleLoss: ((ExplainedNumber)(ref val2)).ResultNumber, affectorSideMaxMoraleGain: ((ExplainedNumber)(ref val)).ResultNumber);
		}
		return (affectedSideMaxMoraleLoss: num, affectorSideMaxMoraleGain: num2);
	}

	public override (float affectedSideMaxMoraleLoss, float affectorSideMaxMoraleGain) CalculateMaxMoraleChangeDueToAgentPanicked(Agent agent)
	{
		return ((MBGameModel<BattleMoraleModel>)this).BaseModel.CalculateMaxMoraleChangeDueToAgentPanicked(agent);
	}

	public override float CalculateMoraleChangeToCharacter(Agent agent, float maxMoraleChange)
	{
		return ((MBGameModel<BattleMoraleModel>)this).BaseModel.CalculateMoraleChangeToCharacter(agent, maxMoraleChange);
	}

	public override float GetEffectiveInitialMorale(Agent agent, float baseMorale)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		float effectiveInitialMorale = ((MBGameModel<BattleMoraleModel>)this).BaseModel.GetEffectiveInitialMorale(agent, baseMorale);
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(effectiveInitialMorale, false, (TextObject)null);
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
		bool flag = false;
		Ship val5 = null;
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
			CharacterObject val6 = (CharacterObject)obj3;
			Hero leaderHero2 = val3.LeaderHero;
			CharacterObject val7 = ((leaderHero2 != null) ? leaderHero2.CharacterObject : null);
			Formation formation = agent.Formation;
			object obj4;
			if (formation == null)
			{
				obj4 = null;
			}
			else
			{
				Agent captain = formation.Captain;
				obj4 = ((captain != null) ? captain.Character : null);
			}
			CharacterObject val8 = (CharacterObject)((obj4 is CharacterObject) ? obj4 : null);
			if (val4 == val8)
			{
				val8 = null;
			}
			if (val2 != null && ((List<Ship>)(object)val2.Ships)?.Count > 0)
			{
				val5 = val2.FlagShip;
				Figurehead val9 = ((val5 != null) ? val5.Figurehead : null);
				flag = val6 != null && val6.GetPerkValue(NavalPerks.Shipmaster.Commodore) && val5 != null && val9 != null;
				if (flag && val9 == DefaultFigureheads.Lion)
				{
					((ExplainedNumber)(ref val)).Add(val9.EffectAmount, (TextObject)null, (TextObject)null);
				}
			}
			val6 = ((val6 != val4) ? val6 : null);
			val7 = ((val7 != val4) ? val7 : null);
			if (val7 != null)
			{
				if (Mission.Current.IsNavalBattle)
				{
					PerkHelper.AddPerkBonusForParty(NavalPerks.Mariner.RallyingCry, val3, true, ref val, false);
					if (val4.IsMariner)
					{
						PerkHelper.AddPerkBonusForParty(NavalPerks.Mariner.AxeOfTheNorthwind, val3, false, ref val, false);
					}
					else
					{
						PerkHelper.AddPerkBonusForParty(NavalPerks.Mariner.SunnyDisposition, val3, false, ref val, false);
					}
				}
				if (((BasicCharacterObject)val7).IsHero)
				{
					Clan clan = val7.HeroObject.Clan;
					if (((clan != null) ? clan.Kingdom : null) != null && val7.HeroObject.Clan.Kingdom.HasPolicy(NavalPolicies.FraternalFleetDoctrine))
					{
						((ExplainedNumber)(ref val)).AddFactor(0.2f, ((PropertyObject)NavalPolicies.FraternalFleetDoctrine).Name);
					}
				}
			}
		}
		NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		if (missionBehavior != null)
		{
			foreach (MissionShip item in (List<MissionShip>)(object)missionBehavior.AllShips)
			{
				IShipOrigin shipOrigin = item.ShipOrigin;
				Ship val10 = (Ship)(object)((shipOrigin is Ship) ? shipOrigin : null);
				if (!flag || val10 != val5)
				{
					IShipOrigin shipOrigin2 = item.ShipOrigin;
					IShipOrigin obj5 = ((shipOrigin2 is Ship) ? shipOrigin2 : null);
					Figurehead val11 = ((obj5 != null) ? ((Ship)obj5).Figurehead : null);
					if (val11 != null && val11 == DefaultFigureheads.Lion && item.GetIsAgentOnShip(agent))
					{
						((ExplainedNumber)(ref val)).Add(val11.EffectAmount, (TextObject)null, (TextObject)null);
					}
				}
			}
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override bool CanPanicDueToMorale(Agent agent)
	{
		return ((MBGameModel<BattleMoraleModel>)this).BaseModel.CanPanicDueToMorale(agent);
	}

	public override float CalculateCasualtiesFactor(BattleSideEnum battleSide)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<BattleMoraleModel>)this).BaseModel.CalculateCasualtiesFactor(battleSide);
	}

	public override float GetAverageMorale(Formation formation)
	{
		return ((MBGameModel<BattleMoraleModel>)this).BaseModel.GetAverageMorale(formation);
	}

	public CharacterObject GetEnemyArmyLeaderCharacter(IShipOrigin shipOrigin)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		GetNavalShipsLogic().FindAssignmentOfShipOrigin(shipOrigin, out var shipAssignment);
		object obj;
		if (shipAssignment == null)
		{
			obj = null;
		}
		else
		{
			Formation formation = shipAssignment.Formation;
			obj = ((formation != null) ? formation.GetFirstUnit() : null);
		}
		Agent val = (Agent)obj;
		if (val != null)
		{
			foreach (Team item in (List<Team>)(object)Mission.Current.Teams)
			{
				if (!item.IsEnemyOf(val.Team) || ((List<Agent>)(object)item.ActiveAgents).Count <= 0)
				{
					continue;
				}
				Agent obj2 = ((List<Agent>)(object)item.ActiveAgents)[0];
				object obj3;
				if (obj2 == null)
				{
					obj3 = null;
				}
				else
				{
					IAgentOriginBase origin = obj2.Origin;
					obj3 = ((origin != null) ? origin.BattleCombatant : null);
				}
				PartyBase val2 = (PartyBase)obj3;
				MobileParty obj4 = ((val2 != null && val2.IsMobile) ? val2.MobileParty : null);
				object result;
				if (obj4 == null)
				{
					result = null;
				}
				else
				{
					Army army = obj4.Army;
					if (army == null)
					{
						result = null;
					}
					else
					{
						MobileParty leaderParty = army.LeaderParty;
						if (leaderParty == null)
						{
							result = null;
						}
						else
						{
							Hero leaderHero = leaderParty.LeaderHero;
							result = ((leaderHero != null) ? leaderHero.CharacterObject : null);
						}
					}
				}
				return (CharacterObject)result;
			}
		}
		return null;
	}

	public override float CalculateMoraleChangeOnShipSunk(IShipOrigin shipOrigin)
	{
		float num = ((MBGameModel<BattleMoraleModel>)this).BaseModel.CalculateMoraleChangeOnShipSunk(shipOrigin);
		CharacterObject enemyArmyLeaderCharacter = GetEnemyArmyLeaderCharacter(shipOrigin);
		if (enemyArmyLeaderCharacter != null && enemyArmyLeaderCharacter.GetPerkValue(NavalPerks.Mariner.EnemyOfTheWood))
		{
			num += NavalPerks.Mariner.EnemyOfTheWood.PrimaryBonus;
		}
		return num;
	}

	public override float CalculateMoraleOnRamming(Agent agent, IShipOrigin rammingShip, IShipOrigin rammedShip)
	{
		float num = ((MBGameModel<BattleMoraleModel>)this).BaseModel.CalculateMoraleOnRamming(agent, rammingShip, rammedShip);
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
		if ((object)((agent != null) ? agent.Character : null) == val2)
		{
			val2 = null;
		}
		PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Shipmaster.ShockAndAwe, val2, ref val);
		Figurehead figurehead = ((Ship)((rammingShip is Ship) ? rammingShip : null)).Figurehead;
		if (figurehead != null && figurehead == DefaultFigureheads.Ram)
		{
			((ExplainedNumber)(ref val)).AddFactor(figurehead.EffectAmount, (TextObject)null);
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override float CalculateMoraleOnShipsConnected(Agent agent, IShipOrigin ownerShip, IShipOrigin targetShip)
	{
		float num = ((MBGameModel<BattleMoraleModel>)this).BaseModel.CalculateMoraleOnShipsConnected(agent, ownerShip, targetShip);
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(num, false, (TextObject)null);
		Figurehead figurehead = ((Ship)((ownerShip is Ship) ? ownerShip : null)).Figurehead;
		if (figurehead != null && figurehead == DefaultFigureheads.Dragon)
		{
			((ExplainedNumber)(ref val)).Add(figurehead.EffectAmount, (TextObject)null, (TextObject)null);
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}
}
