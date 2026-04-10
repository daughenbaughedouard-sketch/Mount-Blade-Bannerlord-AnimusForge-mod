using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.ComponentInterfaces;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.GameComponents;

public class NavalDLCShipDeploymentModel : ShipDeploymentModel
{
	private const int BaseShipDeploymentLimit = 3;

	private const int MaxShipDeploymentLimit = 8;

	public override int GetShipDeploymentLimit(MobileParty party)
	{
		int num = (ShipDeploymentModel.IgnoreDeploymentLimits ? 8 : 3);
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector((float)num, false, (TextObject)null);
		PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.PortAuthority, party, true, ref val, false);
		PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.BlessingsOfTheSea, party, true, ref val, false);
		PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.MerchantFleet, party, true, ref val, false);
		PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.Stormrider, party, false, ref val, false);
		PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.MasterAndCommander, party, false, ref val, false);
		return (int)((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override void GetMapEventPartiesOfPlayerTeams(MBReadOnlyList<MapEventParty> playerSideMapEventParties, bool isPlayerSergeant, out MapEventParty playerMapEventParty, out MBList<MapEventParty> playerTeamMapEventParties, out MBList<MapEventParty> playerAllyTeamMapEventParties)
	{
		MobileParty mainParty = MobileParty.MainParty;
		playerMapEventParty = ((IEnumerable<MapEventParty>)playerSideMapEventParties).FirstOrDefault((Func<MapEventParty, bool>)((MapEventParty mep) => !mep.IsNpcParty));
		_ = mainParty.Army;
		playerTeamMapEventParties = new MBList<MapEventParty>();
		playerAllyTeamMapEventParties = new MBList<MapEventParty>();
		IBattleCombatant val = default(IBattleCombatant);
		bool flag = MissionCombatantsLogic.SupportsAllyTeamOnPlayerSide((IEnumerable<IBattleCombatant>)((IEnumerable<MapEventParty>)playerSideMapEventParties).Select((MapEventParty mapEventParty) => mapEventParty.Party), (IBattleCombatant)(object)playerMapEventParty.Party, isPlayerSergeant, ref val);
		foreach (MapEventParty item in (List<MapEventParty>)(object)playerSideMapEventParties)
		{
			if (PartyBase.IsPartyUnderPlayerCommand(item.Party) || !flag)
			{
				((List<MapEventParty>)(object)playerTeamMapEventParties).Add(item);
			}
			else
			{
				((List<MapEventParty>)(object)playerAllyTeamMapEventParties).Add(item);
			}
		}
	}

	public override void GetShipDeploymentLimitsOfPlayerTeams(MBList<MapEventParty> playerTeamMapEventParties, MBList<MapEventParty> playerAllyTeamMapEventParties, out NavalShipDeploymentLimit playerTeamDeploymentLimit, out NavalShipDeploymentLimit playerAllyTeamDeploymentLimit)
	{
		if (!Extensions.IsEmpty<MapEventParty>((IEnumerable<MapEventParty>)playerAllyTeamMapEventParties))
		{
			playerTeamDeploymentLimit = GetTeamShipDeploymentLimit((MBReadOnlyList<MapEventParty>)(object)playerTeamMapEventParties);
			playerAllyTeamDeploymentLimit = GetTeamShipDeploymentLimit((MBReadOnlyList<MapEventParty>)(object)playerAllyTeamMapEventParties);
			int netDeploymentLimit = playerTeamDeploymentLimit.NetDeploymentLimit;
			int netDeploymentLimit2 = playerAllyTeamDeploymentLimit.NetDeploymentLimit;
			int num = netDeploymentLimit + netDeploymentLimit2;
			if (num > 8)
			{
				num = 8;
				float num2 = (float)netDeploymentLimit / (float)(netDeploymentLimit + netDeploymentLimit2);
				int num3 = MathF.Min(MathF.Max(1, MathF.Round(num2 * (float)num)), netDeploymentLimit);
				int num4 = num - num3;
				if (num3 > playerTeamDeploymentLimit.SkeletalCrewLimit)
				{
					int num5 = num3 - playerTeamDeploymentLimit.SkeletalCrewLimit;
					num3 -= num5;
					num4 = MathF.Min(num4 + num5, playerAllyTeamDeploymentLimit.SkeletalCrewLimit);
				}
				if (num4 > playerAllyTeamDeploymentLimit.SkeletalCrewLimit)
				{
					int num6 = num4 - playerAllyTeamDeploymentLimit.SkeletalCrewLimit;
					num4 -= num6;
					num3 = MathF.Min(num3 + num6, playerTeamDeploymentLimit.SkeletalCrewLimit);
				}
				playerTeamDeploymentLimit = new NavalShipDeploymentLimit(playerTeamDeploymentLimit.PartiesLimit, playerTeamDeploymentLimit.SkeletalCrewLimit, num3);
				playerAllyTeamDeploymentLimit = new NavalShipDeploymentLimit(playerAllyTeamDeploymentLimit.PartiesLimit, playerAllyTeamDeploymentLimit.SkeletalCrewLimit, num4);
			}
		}
		else
		{
			playerTeamDeploymentLimit = GetTeamShipDeploymentLimit((MBReadOnlyList<MapEventParty>)(object)playerTeamMapEventParties);
			playerAllyTeamDeploymentLimit = NavalShipDeploymentLimit.Invalid();
		}
	}

	public override NavalShipDeploymentLimit GetTeamShipDeploymentLimit(MBReadOnlyList<MapEventParty> teamMapEventParties)
	{
		int num = 0;
		MBList<Ship> val = new MBList<Ship>();
		int num2 = 0;
		foreach (MapEventParty item in (List<MapEventParty>)(object)teamMapEventParties)
		{
			MobileParty mobileParty = item.Party.MobileParty;
			if (mobileParty != null)
			{
				((List<Ship>)(object)val).AddRange((IEnumerable<Ship>)mobileParty.Ships);
				num += mobileParty.Party.NumberOfHealthyMembers;
				num2 += NavalDLCManager.Instance.GameModels.ShipDeploymentModel.GetShipDeploymentLimit(mobileParty);
			}
		}
		((List<Ship>)(object)val).Sort((Comparison<Ship>)((Ship s1, Ship s2) => s1.SkeletalCrewCapacity.CompareTo(s2.SkeletalCrewCapacity)));
		int num3 = num;
		int num4 = 0;
		foreach (Ship item2 in (List<Ship>)(object)val)
		{
			if (num3 >= item2.SkeletalCrewCapacity)
			{
				num3 -= item2.SkeletalCrewCapacity;
				num4++;
				continue;
			}
			break;
		}
		num4 = MathF.Min(MathF.Max(num4, 1), 8);
		num2 = MathF.Min(num2, 8);
		return new NavalShipDeploymentLimit(num2, num4, MathF.Max(num2, num4));
	}

	public override Ship GetSuitablePlayerShip(MapEventParty playerMapEventParty, MBList<MapEventParty> playerTeamMapEventParties)
	{
		int playerTeamTroopCount = ((IEnumerable<MapEventParty>)playerTeamMapEventParties).Sum((MapEventParty mep) => mep.Party.NumberOfHealthyMembers);
		Ship val = null;
		if (!Extensions.IsEmpty<Ship>((IEnumerable<Ship>)playerMapEventParty.Ships))
		{
			IEnumerable<Ship> enumerable = ((IEnumerable<Ship>)playerMapEventParty.Ships).Where((Ship s1) => s1.SkeletalCrewCapacity <= playerTeamTroopCount);
			if (!Extensions.IsEmpty<Ship>(enumerable))
			{
				return Extensions.MaxBy<Ship, float>(enumerable, (Func<Ship, float>)((Ship ship) => ship.GetCombatFactor()));
			}
			return Extensions.MinBy<Ship, int>((IEnumerable<Ship>)playerMapEventParty.Ships, (Func<Ship, int>)((Ship ship) => ship.SkeletalCrewCapacity));
		}
		MBList<Ship> val2 = new MBList<Ship>();
		foreach (MapEventParty item in (List<MapEventParty>)(object)playerTeamMapEventParties)
		{
			((List<Ship>)(object)val2).AddRange((IEnumerable<Ship>)item.Ships);
		}
		IEnumerable<Ship> enumerable2 = ((IEnumerable<Ship>)val2).Where((Ship s1) => s1.SkeletalCrewCapacity <= playerTeamTroopCount);
		if (!Extensions.IsEmpty<Ship>(enumerable2))
		{
			return Extensions.MinBy<Ship, float>(enumerable2, (Func<Ship, float>)((Ship ship) => ship.GetCombatFactor()));
		}
		return Extensions.MinBy<Ship, int>((IEnumerable<Ship>)val2, (Func<Ship, int>)((Ship ship) => ship.SkeletalCrewCapacity));
	}

	public override void FillShipsOfTeamParties(MBReadOnlyList<MapEventParty> teamMapEventParties, NavalShipDeploymentLimit shipDeploymentLimit, MBList<IShipOrigin> teamShips)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		int netDeploymentLimit = shipDeploymentLimit.NetDeploymentLimit;
		IOrderedEnumerable<MapEventParty> orderedEnumerable = ((IEnumerable<MapEventParty>)teamMapEventParties).OrderByDescending((MapEventParty teamEventParty) => GetNavalPartyPriority(teamEventParty.Party));
		int troopCount = orderedEnumerable.Sum((MapEventParty party) => party.Party.NumberOfHealthyMembers);
		MBList<(Ship ship, MapEventParty party, bool fixedShip)> candidateShips = new MBList<(Ship, MapEventParty, bool)>();
		foreach (IShipOrigin item2 in (List<IShipOrigin>)(object)teamShips)
		{
			foreach (MapEventParty item3 in orderedEnumerable)
			{
				if (((IEnumerable<IShipOrigin>)item3.Ships).Contains(item2))
				{
					((List<(Ship, MapEventParty, bool)>)(object)candidateShips).Add(((Ship)item2, item3, true));
					break;
				}
			}
		}
		((List<IShipOrigin>)(object)teamShips).Clear();
		int num = 0;
		Dictionary<MapEventParty, MBQueue<(Ship, bool)>> dictionary = new Dictionary<MapEventParty, MBQueue<(Ship, bool)>>();
		MBList<(Ship, bool)> val = new MBList<(Ship, bool)>();
		foreach (MapEventParty item4 in orderedEnumerable)
		{
			foreach (Ship item5 in (List<Ship>)(object)item4.Ships)
			{
				((List<(Ship, bool)>)(object)val).Add((item5, false));
			}
			if (!Extensions.IsEmpty<(Ship, MapEventParty, bool)>((IEnumerable<(Ship, MapEventParty, bool)>)candidateShips))
			{
				((List<(Ship, bool)>)(object)val).RemoveAll((Predicate<(Ship, bool)>)(((Ship ship, bool isReplaced) teamShipTuple) => ((IEnumerable<(Ship, MapEventParty, bool)>)candidateShips).Any<(Ship, MapEventParty, bool)>(((Ship ship, MapEventParty party, bool fixedShip) candidateShipTuple) => candidateShipTuple.ship == teamShipTuple.ship)));
			}
			((List<(Ship, bool)>)(object)val).Sort((Comparison<(Ship, bool)>)(((Ship ship, bool isReplaced) firstShipTuple, (Ship ship, bool isReplaced) secondShipTuple) => secondShipTuple.ship.GetCombatFactor().CompareTo(firstShipTuple.ship.GetCombatFactor())));
			num += ((List<(Ship, bool)>)(object)val).Count;
			dictionary[item4] = new MBQueue<(Ship, bool)>((IEnumerable<(Ship, bool)>)val);
			((List<(Ship, bool)>)(object)val).Clear();
		}
		bool flag = true;
		while (flag && ((List<(Ship, MapEventParty, bool)>)(object)candidateShips).Count < netDeploymentLimit)
		{
			flag = false;
			foreach (MapEventParty item6 in orderedEnumerable)
			{
				MBQueue<(Ship, bool)> val2 = dictionary[item6];
				if (!Extensions.IsEmpty<(Ship, bool)>((IEnumerable<(Ship, bool)>)val2))
				{
					(Ship, bool) tuple = ((Queue<(Ship, bool)>)(object)val2).Dequeue();
					num--;
					((List<(Ship, MapEventParty, bool)>)(object)candidateShips).Add((tuple.Item1, item6, false));
					flag = true;
				}
			}
		}
		if (num > 0)
		{
			int firstUnfilledIndex;
			bool flag2 = CanShipsBeFilled(troopCount, 0.65f, (MBReadOnlyList<(Ship ship, MapEventParty party, bool fixedShip)>)(object)candidateShips, out firstUnfilledIndex);
			bool flag3 = true;
			while (flag3 && !flag2)
			{
				flag3 = false;
				for (int num2 = firstUnfilledIndex; num2 >= 0; num2--)
				{
					(Ship, MapEventParty, bool) tuple2 = ((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[num2];
					if (!tuple2.Item3)
					{
						MapEventParty item = tuple2.Item2;
						MBQueue<(Ship, bool)> val3 = dictionary[item];
						if (!Extensions.IsEmpty<(Ship, bool)>((IEnumerable<(Ship, bool)>)val3))
						{
							(Ship, bool) tuple3 = ((Queue<(Ship, bool)>)(object)val3).Peek();
							if (!tuple3.Item2)
							{
								((Queue<(Ship, bool)>)(object)val3).Dequeue();
								((Queue<(Ship, bool)>)(object)val3).Enqueue((tuple2.Item1, true));
								((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[num2] = (tuple3.Item1, item, false);
								flag3 = true;
							}
						}
					}
					flag2 = CanShipsBeFilled(troopCount, 0.65f, (MBReadOnlyList<(Ship ship, MapEventParty party, bool fixedShip)>)(object)candidateShips, out firstUnfilledIndex);
					if (flag2)
					{
						break;
					}
				}
			}
		}
		if (num > 0)
		{
			flag = true;
			while (flag)
			{
				flag = false;
				foreach (MapEventParty item7 in orderedEnumerable)
				{
					MBQueue<(Ship, bool)> val4 = dictionary[item7];
					if (!Extensions.IsEmpty<(Ship, bool)>((IEnumerable<(Ship, bool)>)val4))
					{
						(Ship, bool) tuple4 = ((Queue<(Ship, bool)>)(object)val4).Dequeue();
						num--;
						((List<(Ship, MapEventParty, bool)>)(object)candidateShips).Add((tuple4.Item1, item7, false));
						flag = true;
					}
				}
			}
		}
		dictionary.Clear();
		if (((List<(Ship, MapEventParty, bool)>)(object)candidateShips).Count > netDeploymentLimit)
		{
			bool flag4 = false;
			bool flag5 = true;
			while (!flag4 && flag5)
			{
				flag5 = false;
				flag4 = IsSkeletalCrewLimitationSatisfied(candidateShips, troopCount, netDeploymentLimit);
				if (flag4)
				{
					continue;
				}
				for (int num3 = netDeploymentLimit - 1; num3 >= 0; num3--)
				{
					(Ship, MapEventParty, bool) shipTupleToBeSwapped = ((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[num3];
					if (!shipTupleToBeSwapped.Item3)
					{
						_ = shipTupleToBeSwapped.Item1.SkeletalCrewCapacity;
						int swapIndex = -1;
						if (FindBestSwapShipBelowSkeletalCrewLimit(candidateShips, shipTupleToBeSwapped, netDeploymentLimit, checkTeamMatch: true, out swapIndex))
						{
							(Ship, MapEventParty, bool) value = ((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[num3];
							((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[num3] = ((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[swapIndex];
							((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[swapIndex] = value;
							flag5 = true;
							break;
						}
						if (FindBestSwapShipBelowSkeletalCrewLimit(candidateShips, shipTupleToBeSwapped, netDeploymentLimit, checkTeamMatch: false, out swapIndex))
						{
							(Ship, MapEventParty, bool) value2 = ((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[num3];
							((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[num3] = ((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[swapIndex];
							((List<(Ship, MapEventParty, bool)>)(object)candidateShips)[swapIndex] = value2;
							flag5 = true;
							break;
						}
					}
				}
			}
		}
		if (((List<(Ship, MapEventParty, bool)>)(object)candidateShips).Count > netDeploymentLimit)
		{
			MBList<(Ship, MapEventParty, bool)> val5 = Extensions.ToMBList<(Ship, MapEventParty, bool)>(((IEnumerable<(Ship, MapEventParty, bool)>)candidateShips).Skip(netDeploymentLimit));
			((List<(Ship, MapEventParty, bool)>)(object)candidateShips).RemoveRange(netDeploymentLimit, ((List<(Ship, MapEventParty, bool)>)(object)candidateShips).Count - netDeploymentLimit);
			((List<(Ship, MapEventParty, bool)>)(object)val5).Sort((Comparison<(Ship, MapEventParty, bool)>)(((Ship ship, MapEventParty party, bool fixedShip) s1, (Ship ship, MapEventParty party, bool fixedShip) s2) => s2.ship.TotalCrewCapacity.CompareTo(s1.ship.TotalCrewCapacity)));
			((List<(Ship, MapEventParty, bool)>)(object)candidateShips).AddRange((IEnumerable<(Ship, MapEventParty, bool)>)val5);
		}
		foreach (var item8 in (List<(Ship, MapEventParty, bool)>)(object)candidateShips)
		{
			((List<IShipOrigin>)(object)teamShips).Add((IShipOrigin)(object)item8.Item1);
		}
	}

	public override void GetOrderedCaptainsForPlayerTeamShips(MBReadOnlyList<MapEventParty> playerTeamMapEventParties, MBReadOnlyList<IShipOrigin> playerTeamShips, out List<string> playerTeamCaptainsByPriority)
	{
		List<string> list = HeroHelper.OrderHeroesOnPlayerSideByPriority(true, true);
		playerTeamCaptainsByPriority = new List<string>(((List<IShipOrigin>)(object)playerTeamShips).Count);
		foreach (IShipOrigin ship in (List<IShipOrigin>)(object)playerTeamShips)
		{
			MapEventParty shipParty = ((IEnumerable<MapEventParty>)playerTeamMapEventParties).FirstOrDefault((Func<MapEventParty, bool>)((MapEventParty mep) => ((IEnumerable<IShipOrigin>)mep.Ships).Contains(ship)));
			string text = list.FirstOrDefault(delegate(string heroId)
			{
				Hero leaderHero = shipParty.Party.LeaderHero;
				return leaderHero != null && ((MBObjectBase)leaderHero).StringId.Equals(heroId);
			});
			if (text != null)
			{
				playerTeamCaptainsByPriority.Add(text);
				list.Remove(text);
			}
			else
			{
				playerTeamCaptainsByPriority.Add(string.Empty);
			}
		}
		for (int num = 0; num < playerTeamCaptainsByPriority.Count; num++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			if (Extensions.IsEmpty<char>((IEnumerable<char>)playerTeamCaptainsByPriority[num]))
			{
				playerTeamCaptainsByPriority[num] = list[0];
				list.RemoveAt(0);
			}
		}
		int num2 = -1;
		int num3 = playerTeamCaptainsByPriority.Count - 1;
		while (num3 >= 0 && Extensions.IsEmpty<char>((IEnumerable<char>)playerTeamCaptainsByPriority[num3]))
		{
			num2 = num3;
			num3--;
		}
		if (num2 >= 0)
		{
			playerTeamCaptainsByPriority.RemoveRange(num2, playerTeamCaptainsByPriority.Count - num2);
		}
		int num4 = 0;
		for (int num5 = 0; num5 < playerTeamCaptainsByPriority.Count; num5++)
		{
			if (!Extensions.IsEmpty<char>((IEnumerable<char>)playerTeamCaptainsByPriority[num5]))
			{
				continue;
			}
			for (int num6 = playerTeamCaptainsByPriority.Count - 1 - num4; num6 > num5; num6--)
			{
				if (!Extensions.IsEmpty<char>((IEnumerable<char>)playerTeamCaptainsByPriority[num6]))
				{
					playerTeamCaptainsByPriority[num5] = playerTeamCaptainsByPriority[num6];
					playerTeamCaptainsByPriority[num6] = string.Empty;
					num4++;
					break;
				}
			}
		}
		playerTeamCaptainsByPriority.RemoveAll((string entry) => Extensions.IsEmpty<char>((IEnumerable<char>)entry));
	}

	public override int[] GetMaximumDeployableTroopCountPerTeam(MBList<IShipOrigin> playerTeamShips, MBList<IShipOrigin> playerAllyTeamShips, MBList<IShipOrigin> enemyTeamShips)
	{
		int[] array = new int[3];
		List<IShipOrigin> list = ((IEnumerable<IShipOrigin>)playerTeamShips).OrderByDescending((IShipOrigin ship) => ship.TotalCrewCapacity).ToList();
		int num = MathF.Min(8, list.Count);
		int num2 = 0;
		for (int num3 = 0; num3 < num; num3++)
		{
			num2 += list[num3].TotalCrewCapacity;
		}
		array[0] = num2;
		if (playerAllyTeamShips != null && ((List<IShipOrigin>)(object)playerAllyTeamShips).Count > 0)
		{
			int num4 = MathF.Min(8, ((List<IShipOrigin>)(object)playerAllyTeamShips).Count);
			int num5 = 0;
			for (int num6 = 0; num6 < num4; num6++)
			{
				num5 += ((List<IShipOrigin>)(object)playerAllyTeamShips)[num6].TotalCrewCapacity;
			}
			array[1] = num5;
		}
		int num7 = MathF.Min(8, ((List<IShipOrigin>)(object)enemyTeamShips).Count);
		int num8 = 0;
		for (int num9 = 0; num9 < num7; num9++)
		{
			num8 += ((List<IShipOrigin>)(object)enemyTeamShips)[num9].TotalCrewCapacity;
		}
		array[2] = num8;
		return array;
	}

	private static float GetNavalPartyPriority(PartyBase party)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		float num = 0f;
		IFaction mapFaction = party.MapFaction;
		if (mapFaction != null && mapFaction.IsClan)
		{
			Clan val = (Clan)mapFaction;
			Hero leaderHero = party.LeaderHero;
			Kingdom kingdom = val.Kingdom;
			if (leaderHero != null)
			{
				if (kingdom != null && leaderHero == kingdom.Leader)
				{
					num += 100000f;
				}
				if (leaderHero == val.Leader)
				{
					num += 10000f;
				}
			}
			int maxClanTier = Campaign.Current.Models.ClanTierModel.MaxClanTier;
			int minClanTier = Campaign.Current.Models.ClanTierModel.MinClanTier;
			float num2 = MathF.Clamp((float)(val.Tier - minClanTier) / (float)maxClanTier, 0f, 1f);
			num += num2 * 1000f;
		}
		return num;
	}

	private static bool CanShipsBeFilled(int troopCount, float fillPercentage, MBReadOnlyList<(Ship ship, MapEventParty party, bool fixedShip)> ships, out int firstUnfilledIndex)
	{
		int num = troopCount;
		int num2 = ((List<(Ship, MapEventParty, bool)>)(object)ships).Count - 1;
		while (num2 >= 0)
		{
			int num3 = (int)((float)((List<(Ship, MapEventParty, bool)>)(object)ships)[num2].Item1.TotalCrewCapacity * fillPercentage);
			if (num >= num3)
			{
				num -= num3;
				num2--;
				continue;
			}
			firstUnfilledIndex = num2;
			return false;
		}
		firstUnfilledIndex = -1;
		return true;
	}

	private static bool IsSkeletalCrewLimitationSatisfied(MBList<(Ship ship, MapEventParty party, bool fixedShip)> ships, int troopCount, int shipsToProcessCount)
	{
		int num = MathF.Min(shipsToProcessCount, ((List<(Ship, MapEventParty, bool)>)(object)ships).Count);
		int num2 = troopCount;
		for (int i = 0; i < num; i++)
		{
			(Ship, MapEventParty, bool) tuple = ((List<(Ship, MapEventParty, bool)>)(object)ships)[i];
			if (num2 < tuple.Item1.SkeletalCrewCapacity)
			{
				break;
			}
			num2 -= tuple.Item1.SkeletalCrewCapacity;
		}
		return num2 >= 0;
	}

	private static bool FindBestSwapShipBelowSkeletalCrewLimit(MBList<(Ship ship, MapEventParty party, bool fixedShip)> ships, (Ship ship, MapEventParty party, bool fixedShip) shipTupleToBeSwapped, int startIndex, bool checkTeamMatch, out int swapIndex)
	{
		swapIndex = -1;
		int num = 0;
		int skeletalCrewCapacity = shipTupleToBeSwapped.ship.SkeletalCrewCapacity;
		for (int i = startIndex; i < ((List<(Ship, MapEventParty, bool)>)(object)ships).Count; i++)
		{
			(Ship, MapEventParty, bool) tuple = ((List<(Ship, MapEventParty, bool)>)(object)ships)[i];
			if (!tuple.Item3 && (!checkTeamMatch || tuple.Item2 == shipTupleToBeSwapped.party))
			{
				int skeletalCrewCapacity2 = tuple.Item1.SkeletalCrewCapacity;
				if (skeletalCrewCapacity2 < skeletalCrewCapacity && skeletalCrewCapacity2 > num)
				{
					swapIndex = i;
					num = skeletalCrewCapacity2;
				}
			}
		}
		return swapIndex > -1;
	}
}
