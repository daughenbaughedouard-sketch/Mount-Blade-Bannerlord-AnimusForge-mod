using System;
using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCCombatSimulationModel : CombatSimulationModel
{
	public override CampaignTime GetSimulationTickInterval(MapEvent mapEvent)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (mapEvent.IsNavalMapEvent)
		{
			return CampaignTime.Minutes(60L);
		}
		return ((MBGameModel<CombatSimulationModel>)this).BaseModel.GetSimulationTickInterval(mapEvent);
	}

	public override void GetBattleAdvantage(MapEvent mapEvent, out ExplainedNumber defenderAdvantage, out ExplainedNumber attackerAdvantage)
	{
		((MBGameModel<CombatSimulationModel>)this).BaseModel.GetBattleAdvantage(mapEvent, ref defenderAdvantage, ref attackerAdvantage);
		if (!mapEvent.IsNavalMapEvent)
		{
			return;
		}
		PartyBase leaderParty = mapEvent.GetLeaderParty((BattleSideEnum)0);
		PartyBase leaderParty2 = mapEvent.GetLeaderParty((BattleSideEnum)1);
		if (leaderParty.IsMobile)
		{
			SkillHelper.AddSkillBonusForParty(NavalSkillEffects.NavalAutoBattleSimulationAdvantage, leaderParty.MobileParty, ref defenderAdvantage);
		}
		if (leaderParty2.IsMobile)
		{
			SkillHelper.AddSkillBonusForParty(NavalSkillEffects.NavalAutoBattleSimulationAdvantage, leaderParty.MobileParty, ref attackerAdvantage);
			if (leaderParty.MobileParty.IsBandit)
			{
				PerkHelper.AddPerkBonusForParty(NavalPerks.Mariner.PirateHunter, leaderParty2.MobileParty, true, ref attackerAdvantage, false);
			}
		}
	}

	public override int GetPursuitRoundCount(MapEvent mapEvent)
	{
		return ((MBGameModel<CombatSimulationModel>)this).BaseModel.GetPursuitRoundCount(mapEvent);
	}

	public override float GetMaximumSiegeEquipmentProgress(Settlement settlement)
	{
		return ((MBGameModel<CombatSimulationModel>)this).BaseModel.GetMaximumSiegeEquipmentProgress(settlement);
	}

	public override int GetNumberOfEquipmentsBuilt(Settlement settlement)
	{
		return ((MBGameModel<CombatSimulationModel>)this).BaseModel.GetNumberOfEquipmentsBuilt(settlement);
	}

	public override float GetSettlementAdvantage(Settlement settlement)
	{
		return ((MBGameModel<CombatSimulationModel>)this).BaseModel.GetSettlementAdvantage(settlement);
	}

	public override float GetShipSiegeEngineHitChance(Ship ship, SiegeEngineType siegeEngineType, BattleSideEnum battleSide)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(0.3f, false, (TextObject)null);
		ShipType type = ship.ShipHull.Type;
		if (!siegeEngineType.IsRanged)
		{
			if ((int)battleSide == 1)
			{
				if ((int)type == 0)
				{
					((ExplainedNumber)(ref val)).Add(0.05f, (TextObject)null, (TextObject)null);
				}
				else if ((int)type == 2)
				{
					((ExplainedNumber)(ref val)).Add(-0.05f, (TextObject)null, (TextObject)null);
				}
			}
			else if ((int)type == 0)
			{
				((ExplainedNumber)(ref val)).Add(-0.05f, (TextObject)null, (TextObject)null);
			}
			else if ((int)type == 2)
			{
				((ExplainedNumber)(ref val)).Add(0.05f, (TextObject)null, (TextObject)null);
			}
		}
		else if ((int)battleSide == 0)
		{
			if ((int)type == 0)
			{
				((ExplainedNumber)(ref val)).Add(-0.1f, (TextObject)null, (TextObject)null);
			}
			else if ((int)type == 2)
			{
				((ExplainedNumber)(ref val)).Add(0.1f, (TextObject)null, (TextObject)null);
			}
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override (int defenderRounds, int attackerRounds) GetSimulationTicksForBattleRound(MapEvent mapEvent)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Invalid comparison between Unknown and I4
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Invalid comparison between Unknown and I4
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Invalid comparison between Unknown and I4
		if (mapEvent.IsNavalMapEvent)
		{
			BattleTypes eventType = mapEvent.EventType;
			Settlement mapEventSettlement = mapEvent.MapEventSettlement;
			int item = 0;
			int item2 = 0;
			int totalCrewCapacity = GetTotalCrewCapacity(mapEvent.DefenderSide);
			int totalCrewCapacity2 = GetTotalCrewCapacity(mapEvent.AttackerSide);
			int num = Math.Min(mapEvent.DefenderSide.NumRemainingSimulationTroops, totalCrewCapacity);
			int num2 = Math.Min(mapEvent.AttackerSide.NumRemainingSimulationTroops, totalCrewCapacity2);
			if (!mapEvent.IsInvulnerable)
			{
				if ((int)eventType == 5 && ((mapEventSettlement.IsTown && num > 100) || (mapEventSettlement.IsCastle && num > 30)))
				{
					float num3 = ((CombatSimulationModel)this).GetSettlementAdvantage(mapEventSettlement) * 0.7f;
					item2 = MathF.Round(1.5f + MathF.Pow((float)num, 0.3f)) * 2;
					item = MathF.Round(0.5f + MathF.Max(1f + MathF.Pow((float)num, 0.3f) * num3, (float)((num + 1) / (num2 + 1)))) * 2;
				}
				else if (num <= 10)
				{
					item = Math.Max(MathF.Round(MathF.Min((float)num2 * 3f, (float)num * 0.3f)), 1);
					item2 = Math.Max(MathF.Round(MathF.Min((float)num * 3f, (float)num2 * 0.3f)), 1);
				}
				else
				{
					item = MathF.Round(MathF.Min((float)num2 * 2f, MathF.Pow((float)num, 0.6f)));
					item2 = MathF.Round(MathF.Min((float)num * 2f, MathF.Pow((float)num2, 0.6f)));
				}
				if ((int)mapEvent.RetreatingSide != -1)
				{
					if ((int)mapEvent.RetreatingSide == 1)
					{
						item2 = 0;
					}
					else
					{
						item = 0;
					}
				}
			}
			return (defenderRounds: item, attackerRounds: item2);
		}
		return ((MBGameModel<CombatSimulationModel>)this).BaseModel.GetSimulationTicksForBattleRound(mapEvent);
	}

	public override ExplainedNumber SimulateHit(CharacterObject strikerTroop, CharacterObject struckTroop, PartyBase strikerParty, PartyBase struckParty, float strikerAdvantage, MapEvent battle, float strikerSideMorale, float struckSideMorale)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<CombatSimulationModel>)this).BaseModel.SimulateHit(strikerTroop, struckTroop, strikerParty, struckParty, strikerAdvantage, battle, strikerSideMorale, struckSideMorale);
		if (battle.IsNavalMapEvent)
		{
			float weightedShipCombatFactor = battle.GetMapEventSide(strikerParty.Side).WeightedShipCombatFactor;
			((ExplainedNumber)(ref result)).AddFactor(weightedShipCombatFactor, (TextObject)null);
		}
		return result;
	}

	public override ExplainedNumber SimulateHit(Ship strikerShip, Ship struckShip, PartyBase strikerParty, PartyBase struckParty, SiegeEngineType siegeEngine, float strikerAdvantage, MapEvent battle, out int troopCasualties)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected I4, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		troopCasualties = 0;
		ExplainedNumber result = default(ExplainedNumber);
		if (siegeEngine.IsRanged)
		{
			((ExplainedNumber)(ref result))._002Ector((float)siegeEngine.Damage, false, (TextObject)null);
			troopCasualties = 1;
		}
		else
		{
			int num = 1;
			ShipType type = strikerShip.ShipHull.Type;
			switch ((int)type)
			{
			case 0:
				num = 1;
				break;
			case 1:
				num = 2;
				break;
			case 2:
				num = 3;
				break;
			default:
				Debug.FailedAssert("Unhandled ship type", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\GameComponents\\NavalDLCCombatSimulationModel.cs", "SimulateHit", 223);
				break;
			}
			((ExplainedNumber)(ref result))._002Ector((float)(siegeEngine.Damage * num), false, (TextObject)null);
			if (struckParty.IsMobile)
			{
				PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.SeaborneFortress, struckParty.MobileParty, true, ref result, false);
			}
		}
		if (strikerParty.IsMobile && !strikerParty.MobileParty.IsCurrentlyAtSea && strikerParty.MobileParty.HasPerk(Crossbow.Terror, false) && RandomOwnerExtensions.RandomFloatWithSeed((IRandomOwner)(object)strikerParty, (uint)battle.UpdateCount) < Crossbow.Terror.PrimaryBonus)
		{
			troopCasualties++;
		}
		return result;
	}

	private int GetTotalCrewCapacity(MapEventSide side)
	{
		int num = 0;
		for (int i = 0; i < ((List<Ship>)(object)side.SimulationShipList).Count; i++)
		{
			Ship val = ((List<Ship>)(object)side.SimulationShipList)[i];
			num += val.MainDeckCrewCapacity;
		}
		return num;
	}

	public override float GetBluntDamageChance(CharacterObject strikerTroop, CharacterObject strikedTroop, PartyBase strikerParty, PartyBase strikedParty, MapEvent battle)
	{
		return ((MBGameModel<CombatSimulationModel>)this).BaseModel.GetBluntDamageChance(strikerTroop, strikedTroop, strikerParty, strikedParty, battle);
	}
}
