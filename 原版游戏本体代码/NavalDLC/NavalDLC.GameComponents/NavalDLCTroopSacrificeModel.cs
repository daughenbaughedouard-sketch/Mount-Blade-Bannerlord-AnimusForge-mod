using System;
using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCTroopSacrificeModel : TroopSacrificeModel
{
	private const int MinNumberOfShipsForSacrificeShips = 2;

	public override int BreakOutArmyLeaderRelationPenalty => ((MBGameModel<TroopSacrificeModel>)this).BaseModel.BreakOutArmyLeaderRelationPenalty;

	public override int BreakOutArmyMemberRelationPenalty => ((MBGameModel<TroopSacrificeModel>)this).BaseModel.BreakOutArmyMemberRelationPenalty;

	public override ExplainedNumber GetLostTroopCountForBreakingInBesiegedSettlement(MobileParty party, SiegeEvent siegeEvent)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber lostTroopCountForBreakingInBesiegedSettlement = ((MBGameModel<TroopSacrificeModel>)this).BaseModel.GetLostTroopCountForBreakingInBesiegedSettlement(party, siegeEvent);
		if (party.IsCurrentlyAtSea && party.HasPerk(NavalPerks.Shipmaster.GhostShip, false))
		{
			((ExplainedNumber)(ref lostTroopCountForBreakingInBesiegedSettlement)).AddFactor(NavalPerks.Shipmaster.GhostShip.PrimaryBonus * -1f, ((PropertyObject)NavalPerks.Shipmaster.GhostShip).Name);
		}
		return lostTroopCountForBreakingInBesiegedSettlement;
	}

	public override ExplainedNumber GetLostTroopCountForBreakingOutOfBesiegedSettlement(MobileParty party, SiegeEvent siegeEvent, bool isBreakingOutFromPort)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber lostTroopCountForBreakingOutOfBesiegedSettlement = ((MBGameModel<TroopSacrificeModel>)this).BaseModel.GetLostTroopCountForBreakingOutOfBesiegedSettlement(party, siegeEvent, isBreakingOutFromPort);
		if (isBreakingOutFromPort && party.HasPerk(NavalPerks.Shipmaster.GhostShip, false))
		{
			((ExplainedNumber)(ref lostTroopCountForBreakingOutOfBesiegedSettlement)).AddFactor(NavalPerks.Shipmaster.GhostShip.PrimaryBonus * -1f, ((PropertyObject)NavalPerks.Shipmaster.GhostShip).Name);
		}
		return lostTroopCountForBreakingOutOfBesiegedSettlement;
	}

	public override int GetNumberOfTroopsSacrificedForTryingToGetAway(BattleSideEnum battleSide, MapEvent mapEvent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<TroopSacrificeModel>)this).BaseModel.GetNumberOfTroopsSacrificedForTryingToGetAway(battleSide, mapEvent);
	}

	private static bool CanPlayerSideTryToGetAwayWithTheirShipStats(out float totalDamageToApply)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		totalDamageToApply = 0f;
		BattleSideEnum playerSide = PlayerEncounter.Current.PlayerSide;
		MapEvent battle = PlayerEncounter.Battle;
		float num = 0f;
		foreach (MapEventParty item in (List<MapEventParty>)(object)battle.PartiesOnSide(playerSide))
		{
			foreach (Ship item2 in (List<Ship>)(object)item.Ships)
			{
				num += item2.HitPoints;
			}
		}
		float num2 = 0f;
		foreach (MapEventParty item3 in (List<MapEventParty>)(object)battle.PartiesOnSide(Extensions.GetOppositeSide(playerSide)))
		{
			foreach (Ship item4 in (List<Ship>)(object)item3.Ships)
			{
				num2 += item4.HitPoints;
			}
		}
		float num3 = num2 / num;
		totalDamageToApply = num * MathF.Pow(MathF.Min(num3, 3f), 1.3f) * 0.1f;
		if (totalDamageToApply > 0f)
		{
			ExplainedNumber val = default(ExplainedNumber);
			((ExplainedNumber)(ref val))._002Ector(totalDamageToApply, false, (TextObject)null);
			SkillHelper.AddSkillBonusForParty(NavalSkillEffects.ShipDamageReduction, MobileParty.MainParty, ref val);
			float num4 = ((ExplainedNumber)(ref val)).ResultNumber;
			if (MobileParty.MainParty.HasPerk(NavalPerks.Shipmaster.GhostShip, false))
			{
				num4 -= num4 * 0.5f;
			}
			ExplainedNumber val2 = Campaign.Current.Models.PartySpeedCalculatingModel.CalculateBaseSpeed(MobileParty.MainParty, false, 0, 0);
			PartyBase leaderParty = battle.GetLeaderParty(Extensions.GetOppositeSide(playerSide));
			ExplainedNumber val3 = Campaign.Current.Models.PartySpeedCalculatingModel.CalculateBaseSpeed(leaderParty.MobileParty, false, 0, 0);
			if (((ExplainedNumber)(ref val2)).ResultNumber > ((ExplainedNumber)(ref val3)).ResultNumber)
			{
				float num5 = MBMath.ClampFloat(((ExplainedNumber)(ref val2)).ResultNumber / ((ExplainedNumber)(ref val3)).ResultNumber, 1f, 5f) * 0.1f;
				num4 -= num4 * num5;
			}
			totalDamageToApply = num4;
		}
		return totalDamageToApply < num;
	}

	public override void GetShipsToSacrificeForTryingToGetAway(BattleSideEnum playerBattleSide, MapEvent mapEvent, out MBList<Ship> shipsToCapture, out Ship shipToTakeDamage, out float damageToApplyForLastShip)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		damageToApplyForLastShip = float.MinValue;
		shipsToCapture = new MBList<Ship>();
		shipToTakeDamage = null;
		MBReadOnlyList<MapEventParty> obj = mapEvent.PartiesOnSide(playerBattleSide);
		mapEvent.RecalculateStrengthOfSides();
		List<Ship> list = new List<Ship>();
		foreach (MapEventParty item in (List<MapEventParty>)(object)obj)
		{
			foreach (Ship item2 in (List<Ship>)(object)item.Ships)
			{
				list.Add(item2);
			}
		}
		if (CanPlayerSideTryToGetAwayWithTheirShipStats(out var totalDamageToApply))
		{
			float maxHitPoints = Extensions.MaxBy<Ship, float>((IEnumerable<Ship>)list, (Func<Ship, float>)((Ship x) => x.MaxHitPoints)).MaxHitPoints;
			if (totalDamageToApply <= Extensions.MinBy<Ship, float>((IEnumerable<Ship>)list, (Func<Ship, float>)((Ship x) => x.HitPoints)).HitPoints)
			{
				((List<Ship>)(object)shipsToCapture).Add(Extensions.MinBy<Ship, float>((IEnumerable<Ship>)list, (Func<Ship, float>)((Ship x) => x.HitPoints)));
				return;
			}
			while (totalDamageToApply > 0f)
			{
				Ship shipToSacrifice = GetShipToSacrifice(maxHitPoints, list);
				if (totalDamageToApply < shipToSacrifice.HitPoints)
				{
					shipToTakeDamage = shipToSacrifice;
					damageToApplyForLastShip = totalDamageToApply;
					totalDamageToApply = 0f;
					break;
				}
				((List<Ship>)(object)shipsToCapture).Add(shipToSacrifice);
				totalDamageToApply -= shipToSacrifice.HitPoints;
				list.Remove(shipToSacrifice);
			}
		}
		else
		{
			Debug.FailedAssert("This can't be possible anymore (Should already handled in previous menu)", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\GameComponents\\NavalDLCTroopSacrificeModel.cs", "GetShipsToSacrificeForTryingToGetAway", 174);
		}
	}

	private static Ship GetShipToSacrifice(float maxHitPointScore, List<Ship> shipsToSacrifice)
	{
		Dictionary<PartyBase, int> partyShipCounts = new Dictionary<PartyBase, int>();
		foreach (Ship item in shipsToSacrifice)
		{
			if (partyShipCounts.TryGetValue(item.Owner, out var _))
			{
				partyShipCounts[item.Owner]++;
			}
			else
			{
				partyShipCounts.Add(item.Owner, 1);
			}
		}
		int maxOwnedShipCount = Extensions.MaxBy<KeyValuePair<PartyBase, int>, int>((IEnumerable<KeyValuePair<PartyBase, int>>)partyShipCounts, (Func<KeyValuePair<PartyBase, int>, int>)((KeyValuePair<PartyBase, int> x) => x.Value)).Value;
		return Extensions.MinBy<Ship, float>((IEnumerable<Ship>)shipsToSacrifice, (Func<Ship, float>)((Ship x) => GetShipSacrificeScore(x, maxOwnedShipCount, partyShipCounts[x.Owner], maxHitPointScore)));
	}

	private static float GetShipSacrificeScore(Ship shipToConsider, int maxOwnedShipCount, int ownerCurrentShipCount, float maxHitPointScore)
	{
		float hitPoints = shipToConsider.HitPoints;
		hitPoints += (float)(maxOwnedShipCount - ownerCurrentShipCount) * maxHitPointScore;
		if (shipToConsider.Owner.MobileParty.LeaderHero.IsKingdomLeader)
		{
			hitPoints += 50000f;
		}
		else if (shipToConsider.Owner.MobileParty.LeaderHero.IsClanLeader)
		{
			hitPoints += 20000f;
		}
		return hitPoints;
	}

	public override bool CanPlayerGetAwayFromEncounter(out TextObject explanation)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		if (!((MBGameModel<TroopSacrificeModel>)this).BaseModel.CanPlayerGetAwayFromEncounter(ref explanation))
		{
			return false;
		}
		if (MobileParty.MainParty.IsCurrentlyAtSea)
		{
			int num = ((List<Ship>)(object)MobileParty.MainParty.Ships).Count;
			if (MobileParty.MainParty.Army != null && (MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty || MobileParty.MainParty.AttachedTo != null))
			{
				foreach (MobileParty item in (List<MobileParty>)(object)MobileParty.MainParty.Army.LeaderParty.AttachedParties)
				{
					num += ((List<Ship>)(object)item.Ships).Count;
				}
			}
			if (num < 2 || !CanPlayerSideTryToGetAwayWithTheirShipStats(out var _))
			{
				explanation = new TextObject("{=uafBbokT}You don't have enough room on your surviving ships to escape.", (Dictionary<string, object>)null);
				return false;
			}
		}
		return true;
	}
}
