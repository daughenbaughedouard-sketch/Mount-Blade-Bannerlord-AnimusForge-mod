using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.Deployment;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

internal class NavalTeamAgents
{
	private struct TroopCountData
	{
		private int _nonHeroOriginsCount;

		private int _heroOriginsCount;

		private int _nonHeroAgentsCount;

		private int _heroAgentsCount;

		public int NonHeroOriginsCount => _nonHeroOriginsCount;

		public int HeroOriginsCount => _heroOriginsCount;

		public int NonHeroAgentsCount => _nonHeroAgentsCount;

		public int HeroAgentsCount => _heroAgentsCount;

		public int OriginsCount => _nonHeroOriginsCount + _heroOriginsCount;

		public int AgentsCount => _nonHeroAgentsCount + _heroAgentsCount;

		public void Add(in NavalTroopAssignment troop)
		{
			if (troop.HasAgent)
			{
				if (troop.Agent.IsHero)
				{
					_heroAgentsCount++;
				}
				else
				{
					_nonHeroAgentsCount++;
				}
			}
			else if (troop.Origin.Troop.IsHero)
			{
				_heroOriginsCount++;
			}
			else
			{
				_nonHeroOriginsCount++;
			}
		}

		public void Remove(in NavalTroopAssignment troop)
		{
			if (troop.HasAgent)
			{
				if (troop.Agent.IsHero)
				{
					_heroAgentsCount--;
				}
				else
				{
					_nonHeroAgentsCount--;
				}
			}
			else if (troop.Origin.Troop.IsHero)
			{
				_heroOriginsCount--;
			}
			else
			{
				_nonHeroOriginsCount--;
			}
		}

		public bool Equals(in TroopCountData other)
		{
			if (_heroOriginsCount == other.HeroOriginsCount && _nonHeroOriginsCount == other.NonHeroOriginsCount && _heroAgentsCount == other.HeroAgentsCount)
			{
				return _nonHeroAgentsCount == other.NonHeroAgentsCount;
			}
			return false;
		}
	}

	internal readonly BattleSideEnum BattleSide;

	internal readonly TeamSideEnum TeamSide;

	internal readonly NavalAgentsLogic AgentsLogic;

	private readonly HashSet<IAgentOriginBase> _allTroopOrigins;

	private readonly HashSet<IAgentOriginBase> _allHeroOrigins;

	private readonly MBList<NavalShipAgents> _allShipAgents;

	private readonly Dictionary<IAgentOriginBase, NavalTroopAssignment> _unassignedTroops;

	private readonly Dictionary<Agent, NavalShipAgents> _agentToShipAgents;

	private readonly MBSortedMultiList<int, NavalTroopAssignment> _unassignedOrderedTroops;

	private TroopCountData _unassignedTroopCountData;

	private readonly Dictionary<Agent, MissionShip> _unassignedReservedAgents;

	private MBList<Agent> _tempSpawnedAgentsList;

	private MBList<NavalTroopAssignment> _tempUnassignedTroops;

	private MBList<NavalShipAgents> _tempShipsWithMissingTroops;

	private MBList<Agent> _tempIncompatibleAgentsList;

	private MBList<IAgentOriginBase> _tempIncompatibleReservesList;

	private MBList<Agent> _tempAgentsNotUsingMachines;

	private MBList<Agent> _recentlySwappedAgents = new MBList<Agent>();

	internal IReadOnlyCollection<IAgentOriginBase> AllTroopOrigins => _allTroopOrigins;

	internal IReadOnlyCollection<IAgentOriginBase> AllHeroOrigins => _allHeroOrigins;

	internal int NumberOfSpawnedAgents { get; private set; }

	internal int NumberOfActiveTroops => _agentToShipAgents.Count;

	internal int NumberOfUnassignedTroops => _unassignedTroops.Count;

	internal bool SpawnReinforcementsOnTick { get; private set; }

	public bool RestrictRecentlySwappedAgentTransfers { get; private set; }

	internal NavalTeamAgents(NavalAgentsLogic agentsLogic, BattleSideEnum battleSide, TeamSideEnum teamSide)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		AgentsLogic = agentsLogic;
		BattleSide = battleSide;
		TeamSide = teamSide;
		_allTroopOrigins = new HashSet<IAgentOriginBase>();
		_allHeroOrigins = new HashSet<IAgentOriginBase>();
		_unassignedTroops = new Dictionary<IAgentOriginBase, NavalTroopAssignment>();
		_unassignedOrderedTroops = new MBSortedMultiList<int, NavalTroopAssignment>(true);
		_unassignedTroopCountData = default(TroopCountData);
		_unassignedReservedAgents = new Dictionary<Agent, MissionShip>();
		_allShipAgents = new MBList<NavalShipAgents>();
		_agentToShipAgents = new Dictionary<Agent, NavalShipAgents>();
		_tempSpawnedAgentsList = new MBList<Agent>();
		_tempShipsWithMissingTroops = new MBList<NavalShipAgents>();
		_tempUnassignedTroops = new MBList<NavalTroopAssignment>();
		_tempAgentsNotUsingMachines = new MBList<Agent>();
		_tempIncompatibleAgentsList = new MBList<Agent>();
		_tempIncompatibleReservesList = new MBList<IAgentOriginBase>();
	}

	internal void AddAgentToShip(Agent agent, MissionShip targetShip)
	{
		MissionShip ship;
		bool num = IsAgentOnAnyShip(agent, out ship);
		bool flag = _unassignedTroops.ContainsKey(agent.Origin);
		if (!num && !flag)
		{
			TryGetShipAgents(targetShip, out var shipAgents);
			AddTroopOriginAux(agent.Origin);
			if (AgentsLogic.IsDeploymentMode)
			{
				MakeSpaceForOneAgent(shipAgents);
			}
			AddAgentAux(agent, shipAgents);
		}
	}

	internal void RemoveAgentFromShip(Agent agent, MissionShip ship)
	{
		IsAgentOnAnyShip(agent, out var _);
		TryGetShipAgents(ship, out var shipAgents);
		RemoveAgentAux(agent, shipAgents);
		RemoveTroopOriginAux(agent.Origin);
	}

	internal int GetNumberOfReservedTroops(bool spawnableOnly)
	{
		int num = 0;
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			if (item.SpawnReinforcements)
			{
				num += item.ReservedTroopsCount;
			}
		}
		return num;
	}

	internal bool AddReservedTroopToShip(IAgentOriginBase troopOrigin, MissionShip ship)
	{
		TryGetShipAgents(ship, out var shipAgents);
		return AddReservedTroopToShipAux(troopOrigin, shipAgents);
	}

	internal int AddReservedTroopsToShip(MBList<IAgentOriginBase> troopOrigins, MissionShip ship)
	{
		int num = 0;
		TryGetShipAgents(ship, out var shipAgents);
		foreach (IAgentOriginBase item in (List<IAgentOriginBase>)(object)troopOrigins)
		{
			if (AddReservedTroopToShipAux(item, shipAgents))
			{
				num++;
			}
		}
		return num;
	}

	internal void RemoveReservedTroopFromShip(IAgentOriginBase troopOrigin, MissionShip ship)
	{
		TryGetShipAgents(ship, out var shipAgents);
		RemoveReservedTroopFromShipAux(troopOrigin, shipAgents);
	}

	internal void RemoveReservedTroopsFromShip(MBList<IAgentOriginBase> troopOrigins, MissionShip ship)
	{
		TryGetShipAgents(ship, out var shipAgents);
		foreach (IAgentOriginBase item in (List<IAgentOriginBase>)(object)troopOrigins)
		{
			RemoveReservedTroopFromShipAux(item, shipAgents);
		}
	}

	internal int RemoveReservedTroopsFromShip(MissionShip ship, int count)
	{
		int i = 0;
		TryGetShipAgents(ship, out var shipAgents);
		for (count = ((count > 0) ? MathF.Min(shipAgents.ReservedTroopsCount, count) : shipAgents.ReservedTroopsCount); i < count; i++)
		{
			if (!RemoveReservedTroopFromShipAux(shipAgents))
			{
				break;
			}
		}
		return i;
	}

	internal void RemoveAllReservedTroopsFromShip(MissionShip ship)
	{
		TryGetShipAgents(ship, out var shipAgents);
		int reservedTroopsCount = shipAgents.ReservedTroopsCount;
		RemoveReservedTroopsFromShip(ship, reservedTroopsCount);
	}

	internal bool TransferAgentToShip(Agent agent, MissionShip targetShip, bool swapAgents)
	{
		_agentToShipAgents.TryGetValue(agent, out var value);
		TryGetShipAgents(targetShip, out var shipAgents);
		bool flag = false;
		if (value == shipAgents)
		{
			flag = true;
		}
		else
		{
			if (swapAgents && AgentsLogic.IsDeploymentMode && ((List<Agent>)(object)shipAgents.ActiveAgents).Count > 0)
			{
				Agent minimumPriorityActiveAgent = shipAgents.GetMinimumPriorityActiveAgent(_recentlySwappedAgents);
				RemoveAgentAux(minimumPriorityActiveAgent, shipAgents);
				MakeSpaceForOneAgent(shipAgents);
				TransferAgentAux(agent, value, shipAgents);
				AddAgentAux(minimumPriorityActiveAgent, value);
				if (RestrictRecentlySwappedAgentTransfers && !((List<Agent>)(object)_recentlySwappedAgents).Contains(minimumPriorityActiveAgent))
				{
					((List<Agent>)(object)_recentlySwappedAgents).Add(minimumPriorityActiveAgent);
				}
				flag = true;
			}
			else if (shipAgents.CanAddMoreAgents || AgentsLogic.IsDeploymentMode)
			{
				if (AgentsLogic.IsDeploymentMode)
				{
					MakeSpaceForOneAgent(shipAgents);
				}
				TransferAgentAux(agent, value, shipAgents);
				flag = true;
			}
			if (flag)
			{
				Formation formation = value.Ship.Formation;
				if (((formation != null) ? formation.Captain : null) == agent)
				{
					SetManagedCaptainOfFormation(null, value.Ship.Formation);
				}
			}
		}
		return flag;
	}

	internal void AssignCaptainToShip(Agent captainAgent, MissionShip targetShip, bool swapOnTransfer, MissionShip captainsCurrentShip)
	{
		TryGetShipAgents(targetShip, out var shipAgents);
		Formation formation = shipAgents.Ship.Formation;
		if (targetShip.Captain == captainAgent)
		{
			return;
		}
		if (targetShip.Captain != null)
		{
			UnassignCaptainOfShip(targetShip);
		}
		if (captainAgent != null)
		{
			if (captainsCurrentShip == null)
			{
				IsAgentOnAnyShip(captainAgent, out captainsCurrentShip);
			}
			if (captainsCurrentShip != targetShip)
			{
				TransferAgentToShip(captainAgent, targetShip, swapOnTransfer);
			}
			SetManagedCaptainOfFormation(captainAgent, formation);
		}
	}

	internal void UnassignCaptainOfShip(MissionShip targetShip)
	{
		SetManagedCaptainOfFormation(null, targetShip.Formation);
	}

	internal IAgentOriginBase FindTroopOrigin(Predicate<IAgentOriginBase> predicate)
	{
		foreach (IAgentOriginBase allTroopOrigin in _allTroopOrigins)
		{
			if (predicate(allTroopOrigin))
			{
				return allTroopOrigin;
			}
		}
		return null;
	}

	internal int FindTroopOrigins(Predicate<IAgentOriginBase> predicate, ref MBList<IAgentOriginBase> foundOrigins)
	{
		if (foundOrigins == null)
		{
			foundOrigins = new MBList<IAgentOriginBase>();
		}
		((List<IAgentOriginBase>)(object)foundOrigins).Clear();
		foreach (IAgentOriginBase allTroopOrigin in _allTroopOrigins)
		{
			if (predicate(allTroopOrigin))
			{
				((List<IAgentOriginBase>)(object)foundOrigins).Add(allTroopOrigin);
			}
		}
		return ((List<IAgentOriginBase>)(object)foundOrigins).Count;
	}

	internal bool IsTroopUnassigned(IAgentOriginBase troopOrigin)
	{
		return _unassignedTroops.ContainsKey(troopOrigin);
	}

	internal bool IsTroopInShipReserves(IAgentOriginBase origin, out MissionShip ship)
	{
		ship = null;
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			if (item.IsOriginInReserves(origin))
			{
				ship = item.Ship;
				return true;
			}
		}
		return false;
	}

	internal bool IsAgentOnAnyShip(IAgentOriginBase origin, out Agent agent, out MissionShip ship)
	{
		agent = null;
		ship = null;
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			MBReadOnlyList<Agent> source = (origin.Troop.IsHero ? item.ActiveHeroAgents : item.ActiveNonHeroAgents);
			agent = ((IEnumerable<Agent>)source).FirstOrDefault((Func<Agent, bool>)((Agent agnt) => agnt.Origin == origin));
			if (agent != null)
			{
				ship = item.Ship;
				break;
			}
		}
		return agent != null;
	}

	internal bool IsAgentOnAnyShip(Agent agent, out MissionShip ship)
	{
		if (_agentToShipAgents.TryGetValue(agent, out var value))
		{
			ship = value.Ship;
			return true;
		}
		ship = null;
		return false;
	}

	internal bool IsAgentOnShip(Agent agent, MissionShip ship)
	{
		if (_agentToShipAgents.TryGetValue(agent, out var value))
		{
			return value.Ship == ship;
		}
		return false;
	}

	internal MBReadOnlyList<Agent> GetActiveAgents()
	{
		MBList<Agent> val = new MBList<Agent>();
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			((List<Agent>)(object)val).AddRange((IEnumerable<Agent>)item.ActiveAgents);
		}
		return (MBReadOnlyList<Agent>)(object)val;
	}

	internal int GetActiveTroopsCountOfShip(MissionShip ship)
	{
		return ((List<Agent>)(object)GetActiveAgentsOfShip(ship)).Count;
	}

	internal MBReadOnlyList<Agent> GetActiveAgentsOfShip(MissionShip ship)
	{
		TryGetShipAgents(ship, out var shipAgents);
		return shipAgents?.ActiveAgents;
	}

	internal int GetTotalTroopsCountOfShip(MissionShip ship, bool spawnableReservesOnly)
	{
		TryGetShipAgents(ship, out var shipAgents);
		int num = ((List<Agent>)(object)shipAgents.ActiveAgents).Count;
		if (!spawnableReservesOnly || shipAgents.SpawnReinforcements)
		{
			num += shipAgents.ReservedTroopsCount;
		}
		return num;
	}

	internal int GetReservedTroopsCountOfShip(MissionShip ship)
	{
		TryGetShipAgents(ship, out var shipAgents);
		return shipAgents.ReservedTroopsCount;
	}

	internal void FillReservedTroopsOfShip(MissionShip ship, MBList<IAgentOriginBase> reservedTroops)
	{
		TryGetShipAgents(ship, out var shipAgents);
		shipAgents.FillReservedTroops(reservedTroops);
	}

	internal MBReadOnlyList<Agent> GetActiveHeroesOfShip(MissionShip ship)
	{
		TryGetShipAgents(ship, out var shipAgents);
		return shipAgents.ActiveHeroAgents;
	}

	internal void AutoComputeDesiredTroopCountsPerShip(bool loadBalanceShips, int troopLimitFromBattleSize)
	{
		if (loadBalanceShips)
		{
			int num = 0;
			foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
			{
				num += item.Ship.TotalCrewCapacity;
			}
			int num2 = Math.Min(troopLimitFromBattleSize, _allTroopOrigins.Count);
			float num3 = (float)num2 / (float)num;
			float num4 = (float)troopLimitFromBattleSize / (float)((List<NavalShipAgents>)(object)_allShipAgents).Count;
			int num5 = 0;
			foreach (NavalShipAgents item2 in (List<NavalShipAgents>)(object)_allShipAgents)
			{
				float num6 = MathF.Min((float)item2.Ship.TotalCrewCapacity * num3, (float)item2.Ship.TotalCrewCapacity);
				if (num6 < (float)item2.Ship.ShipOrigin.SkeletalCrewCapacity)
				{
					num6 = item2.Ship.ShipOrigin.SkeletalCrewCapacity;
				}
				if (num6 > num4)
				{
					num6 = num4;
				}
				int num7 = (int)num6;
				item2.SetDesiredTroopCount(num7);
				num5 += num7;
			}
			int num8 = Math.Min(num2, num) - num5;
			bool flag = true;
			while (flag && num8 > 0)
			{
				flag = false;
				float num9 = float.MaxValue;
				int num10 = -1;
				for (int i = 0; i < ((List<NavalShipAgents>)(object)_allShipAgents).Count; i++)
				{
					NavalShipAgents navalShipAgents = ((List<NavalShipAgents>)(object)_allShipAgents)[i];
					if (navalShipAgents.DesiredTroopCount < navalShipAgents.Ship.TotalCrewCapacity)
					{
						float num11 = (float)navalShipAgents.DesiredTroopCount / (float)navalShipAgents.Ship.ShipOrigin.SkeletalCrewCapacity;
						if (num9 > num11)
						{
							num9 = num11;
							num10 = i;
						}
					}
				}
				if (num10 != -1)
				{
					NavalShipAgents navalShipAgents2 = ((List<NavalShipAgents>)(object)_allShipAgents)[num10];
					navalShipAgents2.SetDesiredTroopCount(navalShipAgents2.DesiredTroopCount + 1);
					num5++;
					num8--;
					flag = true;
				}
			}
			return;
		}
		foreach (NavalShipAgents item3 in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			item3.SetDesiredTroopCount(item3.Ship.TotalCrewCapacity);
		}
	}

	internal void SetDesiredTroopCountOfShip(MissionShip ship, int desiredTroopCount)
	{
		TryGetShipAgents(ship, out var shipAgents);
		shipAgents.SetDesiredTroopCount(desiredTroopCount);
	}

	internal int GetDesiredTroopCountOfShip(MissionShip ship)
	{
		TryGetShipAgents(ship, out var shipAgents);
		return shipAgents.DesiredTroopCount;
	}

	internal void SetIgnoreTroopCapacities(bool value)
	{
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			item.SetIgnoreCapacityChecks(value);
		}
	}

	internal void SetIgnoreTroopCapacities(MissionShip ship, bool value)
	{
		TryGetShipAgents(ship, out var shipAgents);
		shipAgents.SetIgnoreCapacityChecks(value);
	}

	internal int SpawnNextBatch(bool isReinforcement, MBList<Agent> spawnedAgents = null)
	{
		int num = 0;
		foreach (NavalShipAgents item3 in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			(int spawnedCount, int reassignedCount) tuple = item3.SpawnNextBatch(isReinforcement, _tempSpawnedAgentsList);
			int item = tuple.spawnedCount;
			int item2 = tuple.reassignedCount;
			num += item + item2;
			NumberOfSpawnedAgents += item;
			foreach (Agent item4 in (List<Agent>)(object)_tempSpawnedAgentsList)
			{
				_agentToShipAgents[item4] = item3;
			}
			((List<Agent>)(object)spawnedAgents)?.AddRange((IEnumerable<Agent>)_tempSpawnedAgentsList);
			((List<Agent>)(object)_tempSpawnedAgentsList).Clear();
		}
		return num;
	}

	internal void SetSpawnReinforcementsOnTick(bool value, bool resetShips)
	{
		SpawnReinforcementsOnTick = value;
		if (!resetShips)
		{
			return;
		}
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			item.SetSpawnReinforcements(value);
		}
	}

	internal void SetSpawnReinforcementsForShip(MissionShip ship, bool value)
	{
		TryGetShipAgents(ship, out var shipAgents);
		shipAgents.SetSpawnReinforcements(value);
	}

	internal bool GetSpawnReinforcementsForShip(MissionShip ship)
	{
		TryGetShipAgents(ship, out var shipAgents);
		return shipAgents.SpawnReinforcements;
	}

	internal int CheckSpawnReinforcements(MBList<Agent> spawnedAgents = null)
	{
		int num = 0;
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			if (!item.SpawnReinforcements)
			{
				continue;
			}
			int num2 = item.CheckSpawnReinforcements(_tempSpawnedAgentsList);
			num += num2;
			NumberOfSpawnedAgents += num2;
			foreach (Agent item2 in (List<Agent>)(object)_tempSpawnedAgentsList)
			{
				_agentToShipAgents[item2] = item;
			}
			((List<Agent>)(object)spawnedAgents)?.AddRange((IEnumerable<Agent>)_tempSpawnedAgentsList);
			((List<Agent>)(object)_tempSpawnedAgentsList).Clear();
		}
		return num;
	}

	internal void InitializeReinforcementTimers(bool randomizeTimers, bool autoComputeDurations)
	{
		if (autoComputeDurations)
		{
			foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
			{
				item.SetReinforcementSpawnDuration();
			}
		}
		foreach (NavalShipAgents item2 in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			item2.InitializeReinforcementTimer(randomizeTimers);
		}
	}

	internal void SetReinforcementSpawnDurationOfShip(MissionShip ship, float duration)
	{
		TryGetShipAgents(ship, out var shipAgents);
		shipAgents.SetReinforcementSpawnDuration(duration);
	}

	internal void AutoComputeReinforcementSpawnDurations()
	{
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			item.SetReinforcementSpawnDuration();
		}
	}

	internal void ClearRecentlySwappedAgents()
	{
		((List<Agent>)(object)_recentlySwappedAgents).Clear();
	}

	internal void OnAgentRemoved(Agent agent)
	{
		if (_agentToShipAgents.TryGetValue(agent, out var value))
		{
			RemoveAgentAux(agent, value);
			RemoveTroopOriginAux(agent.Origin);
		}
	}

	internal void OnShipSpawned(MissionShip ship, bool ignoreTroopCapacities)
	{
		TryGetShipAgents(ship, out var shipAgents);
		shipAgents = new NavalShipAgents(ship, this);
		shipAgents.SetIgnoreCapacityChecks(ignoreTroopCapacities);
		((List<NavalShipAgents>)(object)_allShipAgents).Add(shipAgents);
	}

	internal void OnShipRemoved(MissionShip ship)
	{
		if (!TryGetShipAgents(ship, out var shipAgents))
		{
			return;
		}
		if (AgentsLogic.IsDeploymentMode && !AgentsLogic.IsMissionEnding)
		{
			while (((List<Agent>)(object)shipAgents.ActiveAgents).Count > 0)
			{
				Agent agent = ((IEnumerable<Agent>)shipAgents.ActiveAgents).Last();
				UnassignAgentAux(shipAgents, agent);
			}
			while (shipAgents.ReservedTroopsCount > 0)
			{
				EnqueueUnassignedTroop(DequeueReservedTroop(shipAgents));
			}
		}
		else
		{
			while (((List<Agent>)(object)shipAgents.ActiveAgents).Count > 0)
			{
				Agent val = ((IEnumerable<Agent>)shipAgents.ActiveAgents).Last();
				RemoveAgentAux(val, shipAgents);
				RemoveTroopOriginAux(val.Origin);
				if (val != Agent.Main)
				{
					val.FadeOut(true, true);
				}
			}
			while (shipAgents.ReservedTroopsCount > 0)
			{
				RemoveTroopOriginAux(DequeueReservedTroop(shipAgents).Origin);
			}
		}
		((List<NavalShipAgents>)(object)_allShipAgents).RemoveAll((Predicate<NavalShipAgents>)((NavalShipAgents sAgentsData) => sAgentsData.Ship == ship));
	}

	internal void OnShipCaptured(MissionShip ship, MissionShip ship2)
	{
		TryGetShipAgents(ship, out var shipAgents);
		shipAgents.OnShipCaptured(ship2);
	}

	internal void OnShipTransferredToFormation(MissionShip ship, Formation oldFormation)
	{
		TryGetShipAgents(ship, out var shipAgents);
		foreach (Agent item in (List<Agent>)(object)shipAgents.ActiveAgents)
		{
			bool num = item == oldFormation.Captain;
			SetManagedAgentFormation(item, ship.Formation);
			if (num)
			{
				SetManagedCaptainOfFormation(item, ship.Formation);
			}
		}
	}

	internal void OnEndDeploymentMode()
	{
		int num = 0;
		while (NumberOfUnassignedTroops > 0)
		{
			DequeueUnassignedTroop(out var dequeuedTroop);
			IAgentOriginBase origin = dequeuedTroop.Origin;
			if (dequeuedTroop.HasAgent)
			{
				dequeuedTroop.Agent.FadeOut(true, true);
				num++;
			}
			RemoveTroopOriginAux(origin);
		}
		foreach (KeyValuePair<Agent, MissionShip> unassignedReservedAgent in _unassignedReservedAgents)
		{
			unassignedReservedAgent.Key.FadeOut(true, true);
			num++;
		}
		_unassignedReservedAgents.Clear();
		NumberOfSpawnedAgents -= num;
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			item.OnEndDeploymentMode();
		}
	}

	internal void SetManagedAgentFormation(Agent agent, Formation formation)
	{
		Formation formation2 = agent.Formation;
		if (formation2 != formation)
		{
			if (formation2 != null && formation2.Captain == agent)
			{
				SetManagedCaptainOfFormation(null, formation2);
			}
			agent.Formation = formation;
		}
	}

	internal void SetManagedCaptainOfFormation(Agent captain, Formation formation)
	{
		if (formation.Captain != captain)
		{
			formation.Captain = captain;
		}
	}

	internal void AddTroopOrigin(IAgentOriginBase origin)
	{
		AddTroopOriginAux(origin);
		EnqueueUnassignedTroop(NavalTroopAssignment.Create(origin));
	}

	internal bool SpawnExistingHero(IAgentOriginBase heroOrigin, MissionShip ship, out Agent spawnedHero)
	{
		spawnedHero = null;
		if (IsAgentOnAnyShip(heroOrigin, out var _, out var _))
		{
			return false;
		}
		TryGetShipAgents(ship, out var shipAgents);
		if (AgentsLogic.IsDeploymentMode)
		{
			MakeSpaceForOneAgent(shipAgents);
		}
		NavalTroopAssignment value;
		bool flag = _unassignedTroops.TryGetValue(heroOrigin, out value);
		if (flag && value.HasAgent)
		{
			spawnedHero = ReassignAgentAux(shipAgents, value.Agent);
		}
		else
		{
			bool flag2 = false;
			NavalTroopAssignment dequeuedTroop = NavalTroopAssignment.Invalid();
			if (flag)
			{
				DequeueUnassignedTroop(value.Origin, out dequeuedTroop);
				flag2 = true;
			}
			else
			{
				NavalShipAgents navalShipAgents = null;
				foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
				{
					if (item.IsOriginInReserves(heroOrigin))
					{
						navalShipAgents = item;
						break;
					}
				}
				DequeueReservedTroop(heroOrigin, navalShipAgents, out dequeuedTroop);
				if (navalShipAgents != shipAgents)
				{
					NavalTroopAssignment dequeuedTroop2;
					if (shipAgents.ReservedTroopsCount > 0)
					{
						TransferReservedTroop(shipAgents, navalShipAgents);
					}
					else if (AgentsLogic.IsDeploymentMode && DequeueUnassignedTroop(out dequeuedTroop2))
					{
						EnqueueReservedTroop(in dequeuedTroop2, navalShipAgents);
					}
				}
				flag2 = true;
			}
			if (flag2)
			{
				EnqueueReservedTroop(in dequeuedTroop, shipAgents);
				spawnedHero = shipAgents.SpawnHeroFromReserve(heroOrigin, out var isReassigned);
				_agentToShipAgents[spawnedHero] = shipAgents;
				if (!isReassigned)
				{
					NumberOfSpawnedAgents++;
				}
			}
		}
		return spawnedHero != null;
	}

	internal void AssignAndTeleportCrewToShipMachines(MissionShip targetShip)
	{
		TryGetShipAgents(targetShip, out var shipAgents);
		shipAgents.AssignAndTeleportCrewToShipMachines();
	}

	internal void AssignAndTeleportCrewToShipMachines()
	{
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			item.AssignAndTeleportCrewToShipMachines();
		}
	}

	internal void UnassignTroops()
	{
		UnassignIncompatibleTroops();
		UnassignExcessTroopsFromShips();
	}

	internal void SetTroopTraitsFilter(MissionShip ship, TroopTraitsMask troopTraitsFilter)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		TryGetShipAgents(ship, out var shipAgents);
		shipAgents.SetTroopTraitsFilter(troopTraitsFilter);
	}

	private void UnassignIncompatibleTroops()
	{
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			foreach (Agent item2 in (List<Agent>)(object)item.ActiveAgents)
			{
				if (!item.IsAgentCompatibleWithShip(item2))
				{
					((List<Agent>)(object)_tempIncompatibleAgentsList).Add(item2);
					item2.Formation.OnBatchUnitRemovalStart();
				}
			}
			foreach (Agent item3 in (List<Agent>)(object)_tempIncompatibleAgentsList)
			{
				UnassignAgentAux(item, item3);
			}
			foreach (Team item4 in (List<Team>)(object)Mission.Current.Teams)
			{
				foreach (Formation item5 in (List<Formation>)(object)item4.FormationsIncludingSpecialAndEmpty)
				{
					item5.OnBatchUnitRemovalEnd();
				}
			}
			((List<Agent>)(object)_tempIncompatibleAgentsList).Clear();
			foreach (NavalTroopAssignment reservedTroop in item.ReservedTroops)
			{
				IAgentOriginBase origin = reservedTroop.Origin;
				if (!item.IsTroopCompatibleWithShip(origin))
				{
					((List<IAgentOriginBase>)(object)_tempIncompatibleReservesList).Add(origin);
				}
			}
			foreach (IAgentOriginBase item6 in (List<IAgentOriginBase>)(object)_tempIncompatibleReservesList)
			{
				DequeueReservedTroop(item6, item, out var dequeuedTroop);
				EnqueueUnassignedTroop(in dequeuedTroop);
			}
			((List<IAgentOriginBase>)(object)_tempIncompatibleReservesList).Clear();
		}
	}

	private void UnassignExcessTroopsFromShips()
	{
		int num = 0;
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			num += item.MissingTroopCount;
		}
		int num2 = 0;
		bool flag = true;
		while (num2 < num && flag)
		{
			flag = false;
			float num3 = 0f;
			NavalShipAgents navalShipAgents = null;
			foreach (NavalShipAgents item2 in (List<NavalShipAgents>)(object)_allShipAgents)
			{
				if (item2.TroopFillRatio >= num3)
				{
					num3 = item2.TroopFillRatio;
					navalShipAgents = item2;
				}
			}
			if (navalShipAgents == null)
			{
				continue;
			}
			if (((List<Agent>)(object)navalShipAgents.ActiveAgents).Count > 0)
			{
				Agent val = null;
				if (!Extensions.IsEmpty<Agent>((IEnumerable<Agent>)navalShipAgents.ActiveNonHeroAgents))
				{
					val = Extensions.MinBy<Agent, float>((IEnumerable<Agent>)navalShipAgents.ActiveNonHeroAgents, (Func<Agent, float>)((Agent a2) => NavalAgentsLogic.GetAgentPriority(a2)));
				}
				if (val == null)
				{
					val = Extensions.MinBy<Agent, float>((IEnumerable<Agent>)navalShipAgents.ActiveHeroAgents, (Func<Agent, float>)((Agent a) => NavalAgentsLogic.GetAgentPriority(a)));
				}
				if (!val.IsMainAgent && !val.IsPlayerTroop && val != navalShipAgents.Ship.Formation.Captain)
				{
					UnassignAgentAux(navalShipAgents, val);
					num2++;
					flag = true;
				}
			}
			if (!flag && navalShipAgents.ReservedTroopsCount > 0)
			{
				EnqueueUnassignedTroop(DequeueReservedTroop(navalShipAgents));
				num2++;
				flag = true;
			}
		}
	}

	internal void SetTroopClassFilter(MissionShip ship, TroopTraitsMask troopClassFilter)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		TryGetShipAgents(ship, out var shipAgents);
		shipAgents.SetTroopClassFilter(troopClassFilter);
	}

	private void AddTroopOriginAux(IAgentOriginBase troopOrigin)
	{
		_allTroopOrigins.Add(troopOrigin);
		if (troopOrigin.Troop.IsHero)
		{
			_allHeroOrigins.Add(troopOrigin);
		}
	}

	public void RemoveTroopOriginAux(IAgentOriginBase troopOrigin)
	{
		_allTroopOrigins.Remove(troopOrigin);
		if (troopOrigin.Troop.IsHero)
		{
			_allHeroOrigins.Remove(troopOrigin);
		}
	}

	private bool AddReservedTroopToShipAux(IAgentOriginBase agentOrigin, NavalShipAgents shipAgentsData)
	{
		if (shipAgentsData.IsOriginInReserves(agentOrigin))
		{
			return true;
		}
		if (AgentsLogic.IsDeploymentMode || shipAgentsData.CanAddMoreReserves)
		{
			NavalTroopAssignment dequeuedTroop = NavalTroopAssignment.Invalid();
			if (shipAgentsData.CanAddMoreReserves && !_allTroopOrigins.Contains(agentOrigin))
			{
				AddTroopOriginAux(agentOrigin);
				dequeuedTroop = NavalTroopAssignment.Create(agentOrigin);
			}
			else if (AgentsLogic.IsDeploymentMode)
			{
				DequeueUnassignedTroop(agentOrigin, out dequeuedTroop);
			}
			if (dequeuedTroop.IsValid)
			{
				EnqueueReservedTroop(in dequeuedTroop, shipAgentsData);
				return true;
			}
		}
		return false;
	}

	private bool RemoveReservedTroopFromShipAux(NavalShipAgents shipAgentsData)
	{
		if (shipAgentsData.ReservedTroopsCount > 0)
		{
			NavalTroopAssignment troop = DequeueReservedTroop(shipAgentsData);
			if (AgentsLogic.IsDeploymentMode)
			{
				EnqueueUnassignedTroop(in troop);
			}
			else
			{
				RemoveTroopOriginAux(troop.Origin);
			}
			return true;
		}
		return false;
	}

	private void UpdateTemporaryShipsWithMissingTroopsAux(int shipIndex, NavalShipAgents shipAgentsData)
	{
		if (shipAgentsData.HasMissingTroops)
		{
			int num = shipIndex;
			while (num > 0 && ((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops)[num - 1].TroopFillRatio < shipAgentsData.TroopFillRatio)
			{
				((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops)[num] = ((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops)[num - 1];
				num--;
			}
			if (num != shipIndex)
			{
				((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops)[num] = shipAgentsData;
			}
		}
		else
		{
			((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops).RemoveAt(shipIndex);
		}
	}

	private bool TryGetShipAgents(MissionShip ship, out NavalShipAgents shipAgents)
	{
		shipAgents = null;
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			if (item.Ship == ship)
			{
				shipAgents = item;
				return true;
			}
		}
		return false;
	}

	private void EnqueueUnassignedTroop(in NavalTroopAssignment troop)
	{
		_unassignedTroops.Add(troop.Origin, troop);
		_unassignedOrderedTroops.Add(troop.Priority, troop);
		_unassignedTroopCountData.Add(in troop);
	}

	private bool DequeueUnassignedTroop(IAgentOriginBase troopOrigin, out NavalTroopAssignment dequeuedTroop)
	{
		dequeuedTroop = NavalTroopAssignment.Invalid();
		if (_unassignedOrderedTroops.Count > 0)
		{
			int num = _unassignedOrderedTroops.FindIndex((Predicate<KeyValuePair<int, NavalTroopAssignment>>)((KeyValuePair<int, NavalTroopAssignment> tuple) => tuple.Value.Origin == troopOrigin), !troopOrigin.Troop.IsHero);
			if (num >= 0)
			{
				dequeuedTroop = _unassignedOrderedTroops[num];
				_unassignedOrderedTroops.RemoveAt(num);
				_unassignedTroops.Remove(dequeuedTroop.Origin);
				_unassignedTroopCountData.Remove(in dequeuedTroop);
				return true;
			}
		}
		return false;
	}

	private bool DequeueUnassignedTroop(out NavalTroopAssignment dequeuedTroop)
	{
		dequeuedTroop = NavalTroopAssignment.Invalid();
		if (_unassignedOrderedTroops.Count > 0)
		{
			dequeuedTroop = _unassignedOrderedTroops.LastValue;
			_unassignedOrderedTroops.RemoveLast();
			_unassignedTroops.Remove(dequeuedTroop.Origin);
			_unassignedTroopCountData.Remove(in dequeuedTroop);
			return true;
		}
		return false;
	}

	internal void AssignTroops(bool useDynamicTroopTraits = false)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops).Clear();
		foreach (NavalShipAgents item in (List<NavalShipAgents>)(object)_allShipAgents)
		{
			if (item.HasMissingTroops)
			{
				int i;
				for (i = 0; i < ((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops).Count && item.TroopFillRatio < ((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops)[i].TroopFillRatio; i++)
				{
				}
				((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops).Insert(i, item);
			}
		}
		while (NumberOfUnassignedTroops > 0)
		{
			int num = -1;
			int num2 = -1;
			DequeueUnassignedTroop(out var dequeuedTroop);
			TroopTraitsMask val = (TroopTraitsMask)0;
			val = ((!useDynamicTroopTraits || dequeuedTroop.Agent == null) ? dequeuedTroop.Origin.GetTraitsMask() : dequeuedTroop.Agent.GetTraitsMask());
			for (int num3 = ((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops).Count - 1; num3 >= 0; num3--)
			{
				NavalShipAgents navalShipAgents = ((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops)[num3];
				if (navalShipAgents.IsTroopCompatibleWithClassFilter(val))
				{
					int traitsFilterPriority = navalShipAgents.GetTraitsFilterPriority(dequeuedTroop);
					if (traitsFilterPriority > num)
					{
						num = traitsFilterPriority;
						num2 = num3;
					}
				}
			}
			if (num2 >= 0)
			{
				NavalShipAgents shipAgentsData = ((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops)[num2];
				EnqueueReservedTroop(in dequeuedTroop, shipAgentsData);
				UpdateTemporaryShipsWithMissingTroopsAux(num2, shipAgentsData);
			}
			else
			{
				((List<NavalTroopAssignment>)(object)_tempUnassignedTroops).Add(dequeuedTroop);
			}
		}
		for (int j = 0; j < ((List<NavalTroopAssignment>)(object)_tempUnassignedTroops).Count; j++)
		{
			if (((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops).Count <= 0)
			{
				break;
			}
			NavalTroopAssignment troop = ((List<NavalTroopAssignment>)(object)_tempUnassignedTroops)[j];
			int num4 = ((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops).Count - 1;
			NavalShipAgents shipAgentsData2 = ((List<NavalShipAgents>)(object)_tempShipsWithMissingTroops)[num4];
			EnqueueReservedTroop(in troop, shipAgentsData2);
			UpdateTemporaryShipsWithMissingTroopsAux(num4, shipAgentsData2);
			((List<NavalTroopAssignment>)(object)_tempUnassignedTroops)[j] = NavalTroopAssignment.Invalid();
		}
		if (((List<NavalTroopAssignment>)(object)_tempUnassignedTroops).Count <= 0)
		{
			return;
		}
		foreach (NavalTroopAssignment item2 in (List<NavalTroopAssignment>)(object)_tempUnassignedTroops)
		{
			NavalTroopAssignment troop2 = item2;
			if (troop2.IsValid)
			{
				EnqueueUnassignedTroop(in troop2);
			}
		}
		((List<NavalTroopAssignment>)(object)_tempUnassignedTroops).Clear();
	}

	private bool DequeueUnassignedAgent(out NavalTroopAssignment dequeuedTroop)
	{
		dequeuedTroop = NavalTroopAssignment.Invalid();
		if (_unassignedOrderedTroops.Count > 0)
		{
			int num = _unassignedOrderedTroops.FindIndex((Predicate<KeyValuePair<int, NavalTroopAssignment>>)((KeyValuePair<int, NavalTroopAssignment> tuple) => tuple.Value.HasAgent), false);
			if (num >= 0)
			{
				dequeuedTroop = _unassignedOrderedTroops[num];
				_unassignedOrderedTroops.RemoveAt(num);
				_unassignedTroops.Remove(dequeuedTroop.Origin);
				_unassignedTroopCountData.Remove(in dequeuedTroop);
				return true;
			}
		}
		return false;
	}

	private void EnqueueReservedTroop(in NavalTroopAssignment troop, NavalShipAgents shipAgentsData)
	{
		shipAgentsData.EnqueueReservedTroop(in troop);
		if (troop.HasAgent)
		{
			_unassignedReservedAgents.Add(troop.Agent, shipAgentsData.Ship);
		}
	}

	private bool RemoveReservedTroopFromShipAux(IAgentOriginBase troopOrigin, NavalShipAgents shipAgentsData)
	{
		if (shipAgentsData.ReservedTroopsCount > 0 && DequeueReservedTroop(troopOrigin, shipAgentsData, out var dequeuedTroop))
		{
			if (AgentsLogic.IsDeploymentMode)
			{
				EnqueueUnassignedTroop(in dequeuedTroop);
			}
			else
			{
				RemoveTroopOriginAux(dequeuedTroop.Origin);
			}
			return true;
		}
		return false;
	}

	private NavalTroopAssignment DequeueReservedTroop(NavalShipAgents shipAgentsData)
	{
		shipAgentsData.DequeueReservedTroop(out var dequeuedTroop);
		if (dequeuedTroop.HasAgent)
		{
			_unassignedReservedAgents.Remove(dequeuedTroop.Agent);
		}
		return dequeuedTroop;
	}

	private bool DequeueReservedTroop(IAgentOriginBase troopOrigin, NavalShipAgents shipAgentsData, out NavalTroopAssignment dequeuedTroop)
	{
		dequeuedTroop = NavalTroopAssignment.Invalid();
		if (shipAgentsData.DequeueReservedTroop(troopOrigin, out dequeuedTroop))
		{
			if (dequeuedTroop.HasAgent)
			{
				_unassignedReservedAgents.Remove(dequeuedTroop.Agent);
			}
			return true;
		}
		return false;
	}

	private void TransferReservedTroop(NavalShipAgents fromShipAgentsData, NavalShipAgents toShipAgentsData, IAgentOriginBase troopOrigin = null)
	{
		NavalTroopAssignment dequeuedTroop = NavalTroopAssignment.Invalid();
		if (troopOrigin != null)
		{
			fromShipAgentsData.DequeueReservedTroop(troopOrigin, out dequeuedTroop);
		}
		else
		{
			fromShipAgentsData.DequeueReservedTroop(out dequeuedTroop);
		}
		toShipAgentsData.EnqueueReservedTroop(in dequeuedTroop);
		if (dequeuedTroop.HasAgent)
		{
			_unassignedReservedAgents[dequeuedTroop.Agent] = toShipAgentsData.Ship;
		}
	}

	private void UnassignAgentAux(NavalShipAgents shipAgentsData, Agent agent)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		RemoveAgentAux(agent, shipAgentsData);
		_ = shipAgentsData.Ship;
		agent.SetDetachableFromFormation(false);
		agent.SetRenderCheckEnabled(false);
		agent.AgentVisuals.SetVisible(false);
		agent.SetIsPhysicsForceClosed(true);
		AgentNavalComponent component = agent.GetComponent<AgentNavalComponent>();
		agent.RemoveComponent((AgentComponent)(object)component);
		AgentNavalAIComponent component2 = agent.GetComponent<AgentNavalAIComponent>();
		agent.RemoveComponent((AgentComponent)(object)component2);
		NavalMissionDeploymentPlanningLogic navalMissionDeploymentPlanningLogic = default(NavalMissionDeploymentPlanningLogic);
		Mission.Current.GetDeploymentPlan<NavalMissionDeploymentPlanningLogic>(ref navalMissionDeploymentPlanningLogic);
		navalMissionDeploymentPlanningLogic.GetMeanBoundaryPosition(agent.Team, out var meanPosition);
		agent.TeleportToPosition(((Vec2)(ref meanPosition)).ToVec3(500f));
		EnqueueUnassignedTroop(NavalTroopAssignment.Create(agent.Origin, agent));
	}

	internal Agent ReassignAgentAux(NavalShipAgents shipAgentsData, Agent agent = null)
	{
		NavalTroopAssignment dequeuedTroop = NavalTroopAssignment.Invalid();
		if (agent == null)
		{
			DequeueUnassignedAgent(out dequeuedTroop);
		}
		else if (_unassignedReservedAgents.ContainsKey(agent))
		{
			_unassignedReservedAgents.Remove(agent);
			dequeuedTroop = NavalTroopAssignment.Create(agent.Origin, agent);
		}
		else
		{
			DequeueUnassignedTroop(agent.Origin, out dequeuedTroop);
		}
		agent = dequeuedTroop.Agent;
		AgentNavalComponent component = agent.GetComponent<AgentNavalComponent>();
		AgentNavalAIComponent component2 = agent.GetComponent<AgentNavalAIComponent>();
		component = new AgentNavalComponent(agent);
		agent.AddComponent((AgentComponent)(object)component);
		component2 = new AgentNavalAIComponent(agent);
		agent.AddComponent((AgentComponent)(object)component2);
		((AgentComponent)component).Initialize();
		agent.AgentVisuals.SetVisible(true);
		agent.SetRenderCheckEnabled(true);
		agent.SetIsPhysicsForceClosed(false);
		if (!agent.IsPlayerTroop)
		{
			agent.SetDetachableFromFormation(true);
		}
		AddAgentAux(agent, shipAgentsData);
		return agent;
	}

	internal void SetRestrictRecentlySwappedAgentTransfers(bool value)
	{
		if (RestrictRecentlySwappedAgentTransfers && !value)
		{
			ClearRecentlySwappedAgents();
		}
		RestrictRecentlySwappedAgentTransfers = value;
	}

	private void AddAgentAux(Agent agent, NavalShipAgents shipAgentsData)
	{
		shipAgentsData.AddAgent(agent);
		_agentToShipAgents[agent] = shipAgentsData;
	}

	private void RemoveAgentAux(Agent agent, NavalShipAgents targetShipAgentsData)
	{
		targetShipAgentsData.RemoveAgent(agent);
		_agentToShipAgents.Remove(agent);
		if (((List<Agent>)(object)_recentlySwappedAgents).Count > 0)
		{
			((List<Agent>)(object)_recentlySwappedAgents).Remove(agent);
		}
	}

	private void TransferAgentAux(Agent agent, NavalShipAgents originShipAgentsData, NavalShipAgents targetShipAgentsData)
	{
		originShipAgentsData?.RemoveAgent(agent);
		targetShipAgentsData.AddAgent(agent);
		_agentToShipAgents[agent] = targetShipAgentsData;
	}

	private void MakeSpaceForOneAgent(NavalShipAgents shipAgentsData, bool ignorePlayerTroop = true)
	{
		while (shipAgentsData.MissingAgentCountOnMainDeck == 0 && ((List<Agent>)(object)shipAgentsData.ActiveAgents).Count > 0)
		{
			Agent minimumPriorityActiveAgent = shipAgentsData.GetMinimumPriorityActiveAgent();
			if (ignorePlayerTroop && minimumPriorityActiveAgent.IsPlayerTroop)
			{
				break;
			}
			UnassignAgentAux(shipAgentsData, minimumPriorityActiveAgent);
		}
		MakeSpaceInReserves(shipAgentsData);
	}

	private void MakeSpaceInReserves(NavalShipAgents shipAgentsData)
	{
		while (shipAgentsData.MissingTroopCount == 0 && shipAgentsData.ReservedTroopsCount > 0)
		{
			RemoveReservedTroopFromShipAux(shipAgentsData);
		}
	}
}
