using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

public class NavalAgentsLogic : MissionLogic
{
	public const float MinReinforcementsDuration = 0.5f;

	public const float MaxReinforcementsDuration = 3f;

	private readonly bool[] _ignoreTroopCapacities;

	private readonly MBList<NavalTeamAgents> _teamAgentsData;

	private bool _isDeploymentMode;

	public NavalShipsLogic NavalShipsLogic { get; private set; }

	public bool IsDeploymentMode => _isDeploymentMode;

	public bool IsDeploymentFinished => ((MissionBehavior)this).Mission.IsDeploymentFinished;

	public bool IsMissionEnding => NavalShipsLogic.IsMissionEnding;

	public event Action<IAgentOriginBase, MissionShip> TroopAddedToReserves;

	public event Action<IAgentOriginBase, MissionShip> TroopRemovedFromReserves;

	public event Action<Agent, MissionShip> AgentAddedToShip;

	public event Action<Agent, MissionShip> AgentRemovedFromShip;

	public NavalAgentsLogic()
	{
		_teamAgentsData = new MBList<NavalTeamAgents>();
		_ignoreTroopCapacities = new bool[3];
	}

	public override void OnBehaviorInitialize()
	{
		NavalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		((MissionBehavior)this).Mission.GetAgentTroopClass_Override += GetNavalMissionTroopClass;
		NavalShipsLogic.ShipSpawnedEvent += OnShipSpawned;
		NavalShipsLogic.ShipRemovedEvent += OnShipRemoved;
		NavalShipsLogic.ShipTransferredToFormationEvent += OnShipTransferredToFormation;
		NavalShipsLogic.ShipTransferredToTeamEvent += OnShipTransferredToTeam;
		NavalShipsLogic.ShipCapturedEvent += OnShipCaptured;
		NavalShipsLogic.ShipTeleportedEvent += OnShipTeleported;
		NavalShipsLogic.ShipBurnedEvent += OnShipBurned;
		NavalShipsLogic.MissionEndEvent += OnMissionEnd;
	}

	public override void OnAgentCreated(Agent agent)
	{
		((MissionBehavior)this).OnAgentCreated(agent);
		agent.AddComponent((AgentComponent)(object)new AgentNavalComponent(agent));
		if (agent.IsHuman)
		{
			agent.AddComponent((AgentComponent)(object)new AgentNavalAIComponent(agent));
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (affectedAgent.IsHuman && GetTeamAgents(affectedAgent.Team.TeamSide, out var teamAgents))
		{
			teamAgents.OnAgentRemoved(affectedAgent);
		}
	}

	public override void EarlyStart()
	{
		UpdateTeamAgentsData();
	}

	public void UpdateTeamAgentsData()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		((List<NavalTeamAgents>)(object)_teamAgentsData).Clear();
		foreach (Team item in (List<Team>)(object)((MissionBehavior)this).Mission.Teams)
		{
			((List<NavalTeamAgents>)(object)_teamAgentsData).Add(new NavalTeamAgents(this, item.Side, item.TeamSide));
		}
	}

	public override void OnMissionTick(float dt)
	{
		foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
		{
			if (item.SpawnReinforcementsOnTick)
			{
				item.CheckSpawnReinforcements();
			}
		}
	}

	public void SetSpawnReinforcementsOnTick(bool value, bool resetShips = true)
	{
		foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
		{
			item.SetSpawnReinforcementsOnTick(value, resetShips);
		}
	}

	private void SetSpawnReinforcementsForShip(MissionShip ship, bool value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
		teamAgents.SetSpawnReinforcementsForShip(ship, value);
	}

	public override void OnMissionStateFinalized()
	{
		((MissionBehavior)this).Mission.GetAgentTroopClass_Override -= GetNavalMissionTroopClass;
		NavalShipsLogic.ShipSpawnedEvent -= OnShipSpawned;
		NavalShipsLogic.ShipRemovedEvent -= OnShipRemoved;
		NavalShipsLogic.ShipTransferredToFormationEvent -= OnShipTransferredToFormation;
		NavalShipsLogic.ShipTransferredToTeamEvent -= OnShipTransferredToTeam;
		NavalShipsLogic.ShipCapturedEvent -= OnShipCaptured;
		NavalShipsLogic.ShipTeleportedEvent -= OnShipTeleported;
		NavalShipsLogic.ShipBurnedEvent -= OnShipBurned;
		NavalShipsLogic.MissionEndEvent -= OnMissionEnd;
	}

	public void SetSpawnReinforcementsOnTick(TeamSideEnum teamSide, bool value, bool resetShips = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.SetSpawnReinforcementsOnTick(value, resetShips);
	}

	public bool GetSpawnReinforcementsOnTick(TeamSideEnum teamSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.SpawnReinforcementsOnTick;
	}

	public void SetIgnoreTroopCapacities(bool value)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
		{
			_ignoreTroopCapacities[item.TeamSide] = value;
			item.SetIgnoreTroopCapacities(value);
		}
	}

	public void SetIgnoreTroopCapacities(MissionShip ship, bool value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
		teamAgents.SetIgnoreTroopCapacities(ship, value);
	}

	public void SetRestrictRecentlySwappedAgentTransfers(TeamSideEnum teamSide, bool value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.SetRestrictRecentlySwappedAgentTransfers(value);
	}

	public bool GetRestrictRecentlySwappedAgentTransfers(TeamSideEnum teamSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.RestrictRecentlySwappedAgentTransfers;
	}

	public void ClearRecentlySwappedAgentsData(TeamSideEnum teamSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.ClearRecentlySwappedAgents();
	}

	public IAgentOriginBase FindTroopOrigin(TeamSideEnum teamSide, Predicate<IAgentOriginBase> predicate)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.FindTroopOrigin(predicate);
	}

	public int FindTroopOrigins(TeamSideEnum teamSide, Predicate<IAgentOriginBase> predicate, ref MBList<IAgentOriginBase> foundOrigins)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.FindTroopOrigins(predicate, ref foundOrigins);
	}

	public IReadOnlyCollection<IAgentOriginBase> GetTeamTroopOrigins(TeamSideEnum teamSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.AllTroopOrigins;
	}

	public IReadOnlyCollection<IAgentOriginBase> GetTeamHeroOrigins(TeamSideEnum teamSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.AllHeroOrigins;
	}

	public int GetNumberOfSpawnedAgents(BattleSideEnum side)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
		{
			if (item.BattleSide == side)
			{
				num += item.NumberOfSpawnedAgents;
			}
		}
		return num;
	}

	public int GetNumberOfActiveAgents(BattleSideEnum side)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
		{
			if (item.BattleSide == side)
			{
				num += item.NumberOfActiveTroops;
			}
		}
		return num;
	}

	internal int GetNumberOfReservedTroops(BattleSideEnum side, bool spawnableOnly = false)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
		{
			if (item.BattleSide == side)
			{
				num += item.GetNumberOfReservedTroops(spawnableOnly);
			}
		}
		return num;
	}

	public MBReadOnlyList<Agent> GetActiveAgentsOfShip(MissionShip ship)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (ship.Team != null)
		{
			GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
			return teamAgents.GetActiveAgentsOfShip(ship);
		}
		return null;
	}

	public int GetReservedTroopsCountOfShip(MissionShip ship)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (ship.Team != null)
		{
			GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
			return teamAgents.GetReservedTroopsCountOfShip(ship);
		}
		return 0;
	}

	internal int GetTotalTroopCountOfShip(MissionShip ship, bool spawnableReservesOnly = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (ship.Team != null)
		{
			GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
			return teamAgents.GetTotalTroopsCountOfShip(ship, spawnableReservesOnly);
		}
		return 0;
	}

	public FormationClass GetNavalMissionTroopClass(BattleSideEnum battleSide, BasicCharacterObject agentCharacter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return TroopClassExtensions.DismountedClass(agentCharacter.GetFormationClass());
	}

	public void FillReservedTroopsOfShip(MissionShip ship, MBList<IAgentOriginBase> reservedTroops)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
		teamAgents.FillReservedTroopsOfShip(ship, reservedTroops);
	}

	public MBReadOnlyList<Agent> GetActiveHeroesOfShip(MissionShip ship)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (ship.Team != null)
		{
			GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
			return teamAgents.GetActiveHeroesOfShip(ship);
		}
		return null;
	}

	public bool IsAgentOnAnyShip(Agent agent, out MissionShip onShip, TeamSideEnum teamSide = (TeamSideEnum)(-1))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if ((int)teamSide == -1)
		{
			foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
			{
				if (item.IsAgentOnAnyShip(agent, out onShip))
				{
					return true;
				}
			}
			onShip = null;
			return false;
		}
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.IsAgentOnAnyShip(agent, out onShip);
	}

	public int GetActiveHeroCountOfShip(MissionShip ship)
	{
		return ((List<Agent>)(object)GetActiveHeroesOfShip(ship)).Count;
	}

	public bool IsTroopOriginInShipReserves(TeamSideEnum teamSide, IAgentOriginBase troopOrigin, out MissionShip onShip)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.IsTroopInShipReserves(troopOrigin, out onShip);
	}

	public void AddAgentToShip(Agent agent, MissionShip targetShip)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = targetShip.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.AddAgentToShip(agent, targetShip);
	}

	public void RemoveAgentFromShip(Agent agent, MissionShip ship)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = ship.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.RemoveAgentFromShip(agent, ship);
	}

	public bool AddReservedTroopToShip(IAgentOriginBase troopOrigin, MissionShip ship)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = ship.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.AddReservedTroopToShip(troopOrigin, ship);
	}

	public bool IsAgentOnAnyShip(IAgentOriginBase agentOrigin, out Agent foundAgent, out MissionShip onShip, TeamSideEnum teamSide = (TeamSideEnum)(-1))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)teamSide == -1)
		{
			foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
			{
				if (item.IsAgentOnAnyShip(agentOrigin, out foundAgent, out onShip))
				{
					return true;
				}
			}
			foundAgent = null;
			onShip = null;
			return false;
		}
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.IsAgentOnAnyShip(agentOrigin, out foundAgent, out onShip);
	}

	public int GetActiveAgentCountOfShip(MissionShip ship)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (ship.Team != null)
		{
			GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
			return teamAgents.GetActiveTroopsCountOfShip(ship);
		}
		return 0;
	}

	public void RemoveAllReservedTroopsFromShip(MissionShip ship)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = ship.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.RemoveAllReservedTroopsFromShip(ship);
	}

	public bool TransferAgentToShip(Agent agent, MissionShip ship)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = ship.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.TransferAgentToShip(agent, ship, swapAgents: false);
	}

	public int SpawnNextBatch(TeamSideEnum teamSide, bool isReinforcement = false, MBList<Agent> spawnedAgents = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.SpawnNextBatch(isReinforcement, spawnedAgents);
	}

	public int CheckSpawnReinforcements(TeamSideEnum teamSide, MBList<Agent> spawnedAgents = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.CheckSpawnReinforcements(spawnedAgents);
	}

	public void InitializeReinforcementTimers(TeamSideEnum teamSide, bool randomizeTimers = true, bool autoComputeDurations = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.InitializeReinforcementTimers(randomizeTimers, autoComputeDurations);
	}

	internal void AssignCaptainToShip(Agent agent, MissionShip ship, MissionShip captainsCurrentShip = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = ship.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.AssignCaptainToShip(agent, ship, swapOnTransfer: false, captainsCurrentShip);
	}

	internal void AssignCaptainToShipForDeploymentMode(Agent agent, MissionShip targetShip, MissionShip captainsCurrentShip = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = targetShip.Team.TeamSide;
		MissionShip formationShip = agent.GetComponent<AgentNavalComponent>().FormationShip;
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.AssignCaptainToShip(agent, targetShip, swapOnTransfer: true, captainsCurrentShip);
		teamAgents.AssignAndTeleportCrewToShipMachines(targetShip);
		if (formationShip != targetShip)
		{
			teamAgents.AssignAndTeleportCrewToShipMachines(formationShip);
		}
	}

	internal void UnassignCaptainOfShip(MissionShip targetShip)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = targetShip.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.UnassignCaptainOfShip(targetShip);
	}

	internal void UnassignCaptainOfShipForDeploymentMode(MissionShip targetShip)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = targetShip.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.UnassignCaptainOfShip(targetShip);
		teamAgents.AssignAndTeleportCrewToShipMachines(targetShip);
	}

	public void SetDeploymentMode(bool value)
	{
		if (_isDeploymentMode == value)
		{
			return;
		}
		if (!value)
		{
			foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
			{
				item.OnEndDeploymentMode();
			}
		}
		_isDeploymentMode = value;
	}

	public void AddTroopOrigin(TeamSideEnum teamSide, IAgentOriginBase troopOrigin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.AddTroopOrigin(troopOrigin);
	}

	public void AddTroopOrigins(TeamSideEnum teamSide, MBList<IAgentOriginBase> troopOrigins)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		foreach (IAgentOriginBase item in (List<IAgentOriginBase>)(object)troopOrigins)
		{
			teamAgents.AddTroopOrigin(item);
		}
	}

	public void UnassignTroops(TeamSideEnum teamSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.UnassignTroops();
	}

	public bool SpawnExistingHero(IAgentOriginBase heroOrigin, MissionShip ship, out Agent spawnedHero)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = ship.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.SpawnExistingHero(heroOrigin, ship, out spawnedHero);
	}

	public void AutoComputeDesiredTroopCountsPerShip(TeamSideEnum teamSide, bool loadBalanceShips = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		int troopLimitFromBattleSize = ComputeTeamTroopLimitAccordingToBattleSize(teamSide);
		teamAgents.AutoComputeDesiredTroopCountsPerShip(loadBalanceShips, troopLimitFromBattleSize);
	}

	public void AssignTroops(TeamSideEnum teamSide, bool useDynamicTroopTraits = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.AssignTroops(useDynamicTroopTraits);
	}

	internal void InvokeAgentRemovedFromShip(Agent agent, MissionShip ship)
	{
		ship.InvalidateActiveFormationTroopOnShipCache();
		this.AgentRemovedFromShip?.Invoke(agent, ship);
	}

	internal void InvokeAgentAddedToShip(Agent agent, MissionShip ship)
	{
		ship.InvalidateActiveFormationTroopOnShipCache();
		this.AgentAddedToShip?.Invoke(agent, ship);
	}

	internal void InvokeTroopRemovedFromReserves(IAgentOriginBase troop, MissionShip ship)
	{
		this.TroopRemovedFromReserves?.Invoke(troop, ship);
	}

	internal void InvokeTroopAddedToReserves(IAgentOriginBase troop, MissionShip ship)
	{
		this.TroopAddedToReserves?.Invoke(troop, ship);
	}

	public bool IsAgentUnassigned(Agent agent, TeamSideEnum teamSide = (TeamSideEnum)(-1))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if ((int)teamSide == -1)
		{
			foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
			{
				if (item.IsTroopUnassigned(agent.Origin))
				{
					return true;
				}
			}
			return false;
		}
		GetTeamAgents(teamSide, out var teamAgents);
		return teamAgents.IsTroopUnassigned(agent.Origin);
	}

	private int ComputeTeamTroopLimitAccordingToBattleSize(TeamSideEnum teamSide)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		int realBattleSizeForNaval = BannerlordConfig.GetRealBattleSizeForNaval();
		int num = 0;
		int num2 = 0;
		foreach (NavalTeamAgents item in (List<NavalTeamAgents>)(object)_teamAgentsData)
		{
			num += item.AllTroopOrigins.Count;
			if (item.TeamSide == teamSide)
			{
				num2 = item.AllTroopOrigins.Count;
			}
		}
		float num3 = (float)num2 * (float)realBattleSizeForNaval / (float)num;
		int num4 = (int)num3;
		float num5 = (float)(num - num2) * (float)realBattleSizeForNaval / (float)num;
		int num6 = (int)num5;
		if (num4 + num6 < realBattleSizeForNaval)
		{
			float num7 = num3 - (float)num4;
			float num8 = num5 - (float)num6;
			if (num7 > num8)
			{
				num4++;
			}
		}
		return num4;
	}

	public void SetDesiredTroopCountOfShip(MissionShip ship, int desiredTroopCount)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
		teamAgents.SetDesiredTroopCountOfShip(ship, desiredTroopCount);
	}

	public void SetTroopClassFilter(MissionShip ship, TroopTraitsMask troopClassFilter)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
		teamAgents.SetTroopClassFilter(ship, troopClassFilter);
	}

	public void SetTroopTraitsFilter(MissionShip ship, TroopTraitsMask troopTraitsFilter)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
		teamAgents.SetTroopTraitsFilter(ship, troopTraitsFilter);
	}

	internal void AssignAndTeleportCrewToShipMachines(MissionShip ship)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		TeamSideEnum teamSide = ship.Team.TeamSide;
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.AssignAndTeleportCrewToShipMachines(ship);
	}

	internal void AssignAndTeleportCrewToShipMachines(TeamSideEnum teamSide)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(teamSide, out var teamAgents);
		teamAgents.AssignAndTeleportCrewToShipMachines();
	}

	private bool GetTeamAgents(TeamSideEnum teamSide, out NavalTeamAgents teamAgents)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		teamAgents = ((IEnumerable<NavalTeamAgents>)_teamAgentsData).FirstOrDefault((NavalTeamAgents mts) => mts.TeamSide == teamSide);
		return teamAgents != null;
	}

	internal void OnAgentSteppedShipChanged(Agent agent, MissionShip newShip)
	{
		agent.UpdateAgentStats();
	}

	private void OnShipSpawned(MissionShip ship)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (GetTeamAgents(ship.Team.TeamSide, out var teamAgents))
		{
			teamAgents.OnShipSpawned(ship, _ignoreTroopCapacities[teamAgents.TeamSide]);
		}
	}

	public static float ComputeReinforcementSpawnDuration(int reservedTroopCount)
	{
		return 0.5f + 2.5f / (float)(1 + reservedTroopCount / 50);
	}

	private void OnShipRemoved(MissionShip ship)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (ship.Team != null && GetTeamAgents(ship.Team.TeamSide, out var teamAgents))
		{
			teamAgents.OnShipRemoved(ship);
		}
	}

	private void OnShipTransferredToFormation(MissionShip ship, Formation oldFormation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GetTeamAgents(ship.Team.TeamSide, out var teamAgents);
		teamAgents.OnShipTransferredToFormation(ship, oldFormation);
	}

	private void OnShipTransferredToTeam(MissionShip ship, Team oldTeam, Formation oldFormation)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (oldTeam != null)
		{
			GetTeamAgents(oldTeam.TeamSide, out var teamAgents);
			teamAgents.OnShipRemoved(ship);
		}
		GetTeamAgents(ship.Team.TeamSide, out var teamAgents2);
		teamAgents2.OnShipSpawned(ship, _ignoreTroopCapacities[teamAgents2.TeamSide]);
	}

	private void OnShipCaptured(MissionShip ship, MissionShip ship2, Formation formation, Formation formation2)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (formation != null)
		{
			GetTeamAgents(formation.Team.TeamSide, out var teamAgents);
			teamAgents.OnShipCaptured(ship, ship2);
			formation.ApplyActionOnEachUnit((Action<Agent>)delegate(Agent agent)
			{
				agent.GetComponent<AgentNavalComponent>().OnShipCaptured();
			}, (Agent)null);
		}
		GetTeamAgents(formation2.Team.TeamSide, out var teamAgents2);
		teamAgents2.OnShipCaptured(ship2, ship);
		formation2.ApplyActionOnEachUnit((Action<Agent>)delegate(Agent agent)
		{
			agent.GetComponent<AgentNavalComponent>().OnShipCaptured();
		}, (Agent)null);
	}

	private void OnShipTeleported(MissionShip ship, MatrixFrame oldFrame, MatrixFrame targetFrame)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		MBReadOnlyList<Agent> activeAgentsOfShip = GetActiveAgentsOfShip(ship);
		if (activeAgentsOfShip == null)
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)activeAgentsOfShip)
		{
			WorldFrame worldFrame = item.GetWorldFrame();
			MatrixFrame val = ((WorldFrame)(ref worldFrame)).ToGroundMatrixFrame();
			MatrixFrame val2 = ((MatrixFrame)(ref oldFrame)).TransformToLocal(ref val);
			TeleportAgentToFrame(item, ((MatrixFrame)(ref targetFrame)).TransformToParent(ref val2));
		}
	}

	private void OnShipBurned(MissionShip ship)
	{
		SetSpawnReinforcementsForShip(ship, value: false);
	}

	internal static float GetAgentPriority(Agent agent)
	{
		if (agent.IsMainAgent)
		{
			return 500f;
		}
		if (agent.IsPlayerTroop)
		{
			return 400f;
		}
		if (agent.Formation != null && agent == agent.Formation.Captain)
		{
			return 300f;
		}
		return (agent.IsHero ? 100f : 0f) + agent.Origin.Troop.GetPower();
	}

	private void OnMissionEnd()
	{
		SetDeploymentMode(value: false);
	}

	internal static void TeleportAgentToFrame(Agent agent, in MatrixFrame teleportFrame)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = agent.Position;
		Vec2 val2;
		if (((Vec3)(ref val)).NearlyEquals(ref teleportFrame.origin, 0.001f))
		{
			val2 = agent.GetMovementDirection();
			val = teleportFrame.rotation.f;
			if (((Vec2)(ref val2)).NearlyEquals(((Vec3)(ref val)).AsVec2, 0.001f))
			{
				return;
			}
		}
		agent.TeleportToPosition(teleportFrame.origin);
		agent.LookDirection = teleportFrame.rotation.f;
		val = teleportFrame.rotation.f;
		val2 = ((Vec3)(ref val)).AsVec2;
		val2 = ((Vec2)(ref val2)).Normalized();
		agent.SetMovementDirection(ref val2);
	}

	internal static void TeleportAndAssignAgentToMachine(Agent agent, NavalShipAgents agentShip, UsableMachine shipMachine)
	{
		TryStopMachineUseAndReattachAgent(agent);
		TryUseMachineAndDetachAgent(agent, agentShip, shipMachine, teleportAndUseInstantly: true, out var _);
	}

	internal static void TryStopMachineUseAndReattachAgent(Agent agent)
	{
		if (agent.IsDetachedFromFormation)
		{
			agent.TryAttachToFormation();
		}
		if (agent.InteractingWithAnyGameObject() && !(agent.CurrentlyUsedGameObject is SpawnedItemEntity))
		{
			agent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
		}
	}

	internal static bool TryUseMachineAndDetachAgent(Agent agent, NavalShipAgents ownerShipAgents, UsableMachine machine, bool teleportAndUseInstantly, out bool isDetached)
	{
		isDetached = false;
		if (machine.PilotAgent == null && !((UsableMissionObject)machine.PilotStandingPoint).IsAIMovingTo(agent))
		{
			if (agent.IsAIControlled && agent.IsDetachableFromFormation && ownerShipAgents.Ship.Formation != null && ownerShipAgents.Ship.Formation == agent.Formation)
			{
				machine.AddAgentAtSlotIndex(agent, 0);
				isDetached = true;
			}
			agent.UseGameObject((UsableMissionObject)(object)machine.PilotStandingPoint, -1);
			if (teleportAndUseInstantly)
			{
				machine.OnPilotAssignedDuringSpawn();
			}
			return true;
		}
		return false;
	}
}
