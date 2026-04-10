using System;
using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

internal class NavalShipAgents
{
	private readonly MBList<Agent> _activeAgents = new MBList<Agent>();

	private readonly MBList<Agent> _activeHeroAgents = new MBList<Agent>();

	private readonly MBList<Agent> _activeNonHeroAgents = new MBList<Agent>();

	private readonly MBSortedMultiList<int, NavalTroopAssignment> _reservedOrderedTroops = new MBSortedMultiList<int, NavalTroopAssignment>(true);

	private readonly Dictionary<IAgentOriginBase, NavalTroopAssignment> _reservedTroops = new Dictionary<IAgentOriginBase, NavalTroopAssignment>();

	private readonly MissionTimer _reinforcementTimer;

	private readonly NavalTeamAgents _teamAgents;

	private TroopTraitsMask _compatibilityTraitsFilter;

	public TroopTraitsMask TroopClassFilter { get; private set; }

	public TroopTraitsMask TroopTraitsFilter { get; private set; }

	internal MBReadOnlyList<Agent> ActiveAgents => (MBReadOnlyList<Agent>)(object)_activeAgents;

	internal MBReadOnlyList<Agent> ActiveHeroAgents => (MBReadOnlyList<Agent>)(object)_activeHeroAgents;

	internal MBReadOnlyList<Agent> ActiveNonHeroAgents => (MBReadOnlyList<Agent>)(object)_activeNonHeroAgents;

	internal MBSortedMultiList<int, NavalTroopAssignment> ReservedTroops => _reservedOrderedTroops;

	internal int ReservedHeroesCount { get; private set; }

	internal int ReservedNonHeroesCount => _reservedTroops.Count - ReservedHeroesCount;

	internal int ReservedTroopsCount => _reservedTroops.Count;

	internal int AllTroopsCount => _reservedTroops.Count + ((List<Agent>)(object)_activeAgents).Count;

	internal MissionShip Ship { get; private set; }

	internal bool CanAddMoreReserves
	{
		get
		{
			if (!IgnoreCapacityChecks)
			{
				return HasMissingTroops;
			}
			return true;
		}
	}

	internal bool CanAddMoreAgents
	{
		get
		{
			if (!IgnoreCapacityChecks)
			{
				if (HasMissingAgentsOnMainDeck)
				{
					return HasMissingTroops;
				}
				return false;
			}
			return true;
		}
	}

	internal bool SpawnReinforcements { get; private set; } = true;

	internal bool IgnoreCapacityChecks { get; private set; }

	internal bool HasPlayerAgent
	{
		get
		{
			if (Agent.Main != null)
			{
				return ((List<Agent>)(object)_activeHeroAgents).Contains(Agent.Main);
			}
			return false;
		}
	}

	internal int DesiredTroopCount { get; private set; }

	internal bool HasMissingTroops => MissingTroopCount > 0;

	internal int MissingTroopCount => MathF.Max(0, MathF.Min(DesiredTroopCount, Ship.TotalCrewCapacity) - AllTroopsCount);

	internal int MissingAgentCountOnMainDeck => MathF.Max(0, MathF.Min(DesiredTroopCount, Ship.CrewSizeOnMainDeck) - ((List<Agent>)(object)ActiveAgents).Count);

	internal bool HasMissingAgentsOnMainDeck => MissingAgentCountOnMainDeck > 0;

	internal float TroopFillRatio
	{
		get
		{
			if (DesiredTroopCount <= 0)
			{
				if (AllTroopsCount != 0)
				{
					return float.MaxValue;
				}
				return 0f;
			}
			return (float)AllTroopsCount / (float)DesiredTroopCount;
		}
	}

	internal NavalShipAgents(MissionShip ship, NavalTeamAgents teamAgents)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		Ship = ship;
		_teamAgents = teamAgents;
		DesiredTroopCount = Ship.TotalCrewCapacity;
		_reinforcementTimer = new MissionTimer(0f);
		SetTroopClassFilter((TroopTraitsMask)3);
		SetTroopTraitsFilter((TroopTraitsMask)0);
	}

	internal void InitializeReinforcementTimer(bool randomizeTimers)
	{
		if (randomizeTimers)
		{
			_reinforcementTimer.Set(MBRandom.RandomFloat * _reinforcementTimer.GetTimerDuration());
		}
		else
		{
			_reinforcementTimer.Reset();
		}
	}

	internal void SetReinforcementSpawnDuration(float duration = 0f)
	{
		if (duration <= 0f)
		{
			duration = NavalAgentsLogic.ComputeReinforcementSpawnDuration(ReservedTroopsCount);
		}
		_reinforcementTimer.SetDuration(duration);
	}

	internal void SetIgnoreCapacityChecks(bool value)
	{
		IgnoreCapacityChecks = value;
	}

	internal void SetSpawnReinforcements(bool value)
	{
		if (SpawnReinforcements != value)
		{
			SpawnReinforcements = value;
		}
	}

	internal void SetTroopClassFilter(TroopTraitsMask troopClassFilter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		TroopClassFilter = troopClassFilter;
	}

	internal void SetTroopTraitsFilter(TroopTraitsMask troopTraitsFilter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		TroopTraitsFilter = troopTraitsFilter;
		_compatibilityTraitsFilter = (TroopTraitsMask)(troopTraitsFilter & 0xFE7F);
	}

	internal bool IsAgentCompatibleWithShip(Agent agent, bool checkDynamicCompatibility = false)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (agent == Ship.Captain || agent.IsPlayerTroop)
		{
			return true;
		}
		TroopTraitsMask val = (checkDynamicCompatibility ? agent.GetTraitsMask() : agent.Origin.GetTraitsMask());
		if (IsTroopCompatibleWithClassFilter(val))
		{
			return IsTroopCompatibleWithTraitsFilter(val, agent.Character.GetBattleTier());
		}
		return false;
	}

	internal bool IsTroopCompatibleWithShip(IAgentOriginBase troopOrigin)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (troopOrigin.Troop.IsPlayerCharacter)
		{
			return true;
		}
		TroopTraitsMask traitsMask = troopOrigin.GetTraitsMask();
		bool num = IsTroopCompatibleWithClassFilter(traitsMask);
		bool flag = IsTroopCompatibleWithTraitsFilter(traitsMask, troopOrigin.Troop.GetBattleTier());
		return num && flag;
	}

	internal int GetTraitsFilterPriority(NavalTroopAssignment troopAssignment, bool checkDynamicCompatibility = false)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (troopAssignment.Agent != null && checkDynamicCompatibility)
		{
			Agent agent = troopAssignment.Agent;
			return TroopFilteringUtilities.GetTroopPriority(agent.GetTraitsMask(), agent.Origin.Troop.GetBattleTier(), TroopTraitsFilter);
		}
		IAgentOriginBase origin = troopAssignment.Origin;
		return TroopFilteringUtilities.GetTroopPriority(origin.GetTraitsMask(), origin.Troop.GetBattleTier(), TroopTraitsFilter);
	}

	internal bool IsTroopCompatibleWithClassFilter(TroopTraitsMask troopClassMask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		return (troopClassMask & TroopClassFilter) > 0;
	}

	internal bool IsTroopCompatibleWithTraitsFilter(TroopTraitsMask troopTraitsMask, int troopBattleTier)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((int)TroopTraitsFilter == 0)
		{
			return true;
		}
		if ((TroopTraitsMask)(troopTraitsMask & _compatibilityTraitsFilter) == _compatibilityTraitsFilter)
		{
			float num = troopBattleTier;
			float num2 = 3.5f;
			if ((TroopTraitsFilter & 0x100) != 0 && num >= num2)
			{
				return true;
			}
			if ((TroopTraitsFilter & 0x80) != 0 && num <= num2)
			{
				return true;
			}
		}
		return false;
	}

	public void OnShipCaptured(MissionShip newShip)
	{
		int num = newShip?.TotalCrewCapacity ?? 0;
		int num2 = newShip?.CrewSizeOnMainDeck ?? 0;
		DesiredTroopCount = ((DesiredTroopCount == Ship.TotalCrewCapacity) ? num : Math.Min(DesiredTroopCount, num));
		Ship = newShip;
		int num3 = num - num2;
		while (ReservedTroopsCount > num3)
		{
			DequeueReservedTroop(out var dequeuedTroop);
			_teamAgents.RemoveTroopOriginAux(dequeuedTroop.Origin);
		}
	}

	internal void SetDesiredTroopCount(int value)
	{
		DesiredTroopCount = value;
	}

	internal bool IsOriginInReserves(IAgentOriginBase origin)
	{
		return _reservedTroops.ContainsKey(origin);
	}

	internal void EnqueueReservedTroop(in NavalTroopAssignment troop)
	{
		_reservedOrderedTroops.Add(troop.Priority, troop);
		_reservedTroops.Add(troop.Origin, troop);
		if (troop.Origin.Troop.IsHero)
		{
			ReservedHeroesCount++;
		}
		_teamAgents.AgentsLogic.InvokeTroopAddedToReserves(troop.Origin, Ship);
	}

	internal bool DequeueReservedTroop(out NavalTroopAssignment dequeuedTroop)
	{
		dequeuedTroop = NavalTroopAssignment.Invalid();
		if (_reservedOrderedTroops.Count > 0)
		{
			dequeuedTroop = _reservedOrderedTroops.LastValue;
			_reservedOrderedTroops.RemoveLast();
			_reservedTroops.Remove(dequeuedTroop.Origin);
			if (dequeuedTroop.Origin.Troop.IsHero)
			{
				ReservedHeroesCount--;
			}
		}
		if (dequeuedTroop.IsValid)
		{
			_teamAgents.AgentsLogic.InvokeTroopRemovedFromReserves(dequeuedTroop.Origin, Ship);
		}
		return dequeuedTroop.IsValid;
	}

	internal bool DequeueReservedTroop(IAgentOriginBase origin, out NavalTroopAssignment dequeuedTroop)
	{
		dequeuedTroop = NavalTroopAssignment.Invalid();
		if (_reservedTroops.TryGetValue(origin, out dequeuedTroop))
		{
			int priority = NavalTroopAssignment.GetPriority(dequeuedTroop.Origin, dequeuedTroop.Agent);
			for (int i = _reservedOrderedTroops.FirstIndexOf(priority); i <= _reservedOrderedTroops.Count; i++)
			{
				if (_reservedOrderedTroops[i].Origin == origin)
				{
					_reservedOrderedTroops.RemoveAt(i);
					_reservedTroops.Remove(origin);
					if (origin.Troop.IsHero)
					{
						ReservedHeroesCount--;
					}
					break;
				}
			}
		}
		if (dequeuedTroop.IsValid)
		{
			_teamAgents.AgentsLogic.InvokeTroopRemovedFromReserves(origin, Ship);
		}
		return dequeuedTroop.IsValid;
	}

	internal void FillReservedTroops(MBList<IAgentOriginBase> reservedTroops)
	{
		foreach (KeyValuePair<IAgentOriginBase, NavalTroopAssignment> reservedTroop in _reservedTroops)
		{
			((List<IAgentOriginBase>)(object)reservedTroops).Add(reservedTroop.Key);
		}
	}

	internal void AddAgent(Agent agent)
	{
		_teamAgents.SetManagedAgentFormation(agent, Ship.Formation);
		((List<Agent>)(object)_activeAgents).Add(agent);
		if (agent.IsHero)
		{
			((List<Agent>)(object)_activeHeroAgents).Add(agent);
		}
		else
		{
			((List<Agent>)(object)_activeNonHeroAgents).Add(agent);
		}
		_teamAgents.AgentsLogic.InvokeAgentAddedToShip(agent, Ship);
	}

	internal void RemoveAgent(Agent agent)
	{
		NavalAgentsLogic.TryStopMachineUseAndReattachAgent(agent);
		agent.TryRemoveAllDetachmentScores();
		if (agent.IsHero)
		{
			((List<Agent>)(object)_activeHeroAgents).Remove(agent);
		}
		else
		{
			((List<Agent>)(object)_activeNonHeroAgents).Remove(agent);
		}
		((List<Agent>)(object)_activeAgents).Remove(agent);
		_teamAgents.SetManagedAgentFormation(agent, null);
		_teamAgents.AgentsLogic.InvokeAgentRemovedFromShip(agent, Ship);
	}

	internal (int spawnedCount, int reassignedCount) SpawnNextBatch(bool isReinforcement, MBList<Agent> spawnedAgents)
	{
		int num = 0;
		int num2 = 0;
		int num3 = MathF.Min(ReservedTroopsCount, MissingAgentCountOnMainDeck);
		if (isReinforcement && num3 > 0)
		{
			num3 = Math.Min(((List<MatrixFrame>)(object)Ship.CrewSpawnLocalFrames).Count, num3);
		}
		if (num3 > 0)
		{
			while (num < num3)
			{
				DequeueReservedTroop(out var dequeuedTroop);
				Agent item;
				if (dequeuedTroop.Agent != null)
				{
					item = ReassignFromUnassignedReserves(dequeuedTroop);
				}
				else
				{
					item = SpawnAgentAux(dequeuedTroop.Origin, isReinforcement);
					num2++;
				}
				num++;
				((List<Agent>)(object)spawnedAgents).Add(item);
			}
		}
		int item2 = num - num2;
		return (spawnedCount: num2, reassignedCount: item2);
	}

	internal int CheckSpawnReinforcements(MBList<Agent> spawnedAgents)
	{
		int num = 0;
		if (_reinforcementTimer.Check(true) && Ship?.Team != null && !Ship.IsShipNavmeshDisabled)
		{
			num = SpawnNextBatch(isReinforcement: true, spawnedAgents).spawnedCount;
			if (num > 0)
			{
				float duration = NavalAgentsLogic.ComputeReinforcementSpawnDuration(ReservedTroopsCount);
				_reinforcementTimer.SetDuration(duration);
			}
		}
		return num;
	}

	internal Agent SpawnHeroFromReserve(IAgentOriginBase heroOrigin, out bool isReassigned)
	{
		DequeueReservedTroop(heroOrigin, out var dequeuedTroop);
		isReassigned = false;
		Agent result;
		if (dequeuedTroop.Agent != null)
		{
			result = ReassignFromUnassignedReserves(dequeuedTroop);
			isReassigned = true;
		}
		else
		{
			result = SpawnAgentAux(heroOrigin, isReinforcement: false);
		}
		return result;
	}

	internal void AssignAndTeleportCrewToShipMachines()
	{
		Mission mission = ((MissionBehavior)_teamAgents.AgentsLogic).Mission;
		ShipControllerMachine shipControllerMachine = Ship.ShipControllerMachine;
		Ship.ShipOrder.ManageShipDetachments();
		if (Ship.ShipPlacementDetachment.IsUsedByFormation(Ship.Formation))
		{
			do
			{
				Ship.ShipPlacementDetachment.Tick();
			}
			while (Ship.ShipPlacementDetachment.IsTickRequired);
		}
		bool isTeleportingAgents = mission.IsTeleportingAgents;
		mission.IsTeleportingAgents = true;
		if (((UsableMissionObject)((UsableMachine)shipControllerMachine).PilotStandingPoint).HasAIMovingTo)
		{
			((UsableMissionObject)((UsableMachine)shipControllerMachine).PilotStandingPoint).MovingAgent.UseGameObject((UsableMissionObject)(object)((UsableMachine)shipControllerMachine).PilotStandingPoint, -1);
			((UsableMachine)shipControllerMachine).OnPilotAssignedDuringSpawn();
		}
		if (Ship.Captain != null && ((UsableMachine)shipControllerMachine).PilotAgent != Ship.Captain && (!Ship.Captain.IsAIControlled || !((UsableMachine)shipControllerMachine).IsDisabledForAI))
		{
			Ship.Captain.UseGameObject((UsableMissionObject)(object)((UsableMachine)shipControllerMachine).PilotStandingPoint, -1);
			((UsableMachine)shipControllerMachine).OnPilotAssignedDuringSpawn();
		}
		foreach (ShipOarMachine item in (List<ShipOarMachine>)(object)Ship.ShipOarMachines)
		{
			if (((UsableMissionObject)((UsableMachine)item).PilotStandingPoint).HasAIMovingTo)
			{
				((UsableMissionObject)((UsableMachine)item).PilotStandingPoint).MovingAgent.UseGameObject((UsableMissionObject)(object)((UsableMachine)item).PilotStandingPoint, -1);
				((UsableMachine)item).OnPilotAssignedDuringSpawn();
			}
		}
		if (Ship.ShipSiegeWeapon != null)
		{
			RangedSiegeWeapon shipSiegeWeapon = Ship.ShipSiegeWeapon;
			if (((UsableMissionObject)((UsableMachine)shipSiegeWeapon).PilotStandingPoint).HasAIMovingTo)
			{
				((UsableMissionObject)((UsableMachine)shipSiegeWeapon).PilotStandingPoint).MovingAgent.UseGameObject((UsableMissionObject)(object)((UsableMachine)shipSiegeWeapon).PilotStandingPoint, -1);
				((UsableMachine)shipSiegeWeapon).OnPilotAssignedDuringSpawn();
			}
		}
		foreach (Agent item2 in (List<Agent>)(object)_activeAgents)
		{
			if (item2.IsAIControlled && !item2.InteractingWithAnyGameObject())
			{
				item2.ForceUpdateCachedAndFormationValues(true, false);
			}
		}
		mission.IsTeleportingAgents = isTeleportingAgents;
	}

	internal void OnEndDeploymentMode()
	{
		List<KeyValuePair<int, NavalTroopAssignment>> list = new List<KeyValuePair<int, NavalTroopAssignment>>();
		foreach (NavalTroopAssignment reservedOrderedTroop in _reservedOrderedTroops)
		{
			if (reservedOrderedTroop.HasAgent)
			{
				KeyValuePair<int, NavalTroopAssignment> item = new KeyValuePair<int, NavalTroopAssignment>(reservedOrderedTroop.Priority, NavalTroopAssignment.Create(reservedOrderedTroop.Origin));
				list.Add(item);
			}
		}
		_reservedOrderedTroops.RemoveAll((Predicate<KeyValuePair<int, NavalTroopAssignment>>)((KeyValuePair<int, NavalTroopAssignment> tuple) => tuple.Value.HasAgent));
		_reservedOrderedTroops.AddRange((IEnumerable<KeyValuePair<int, NavalTroopAssignment>>)list);
		foreach (KeyValuePair<int, NavalTroopAssignment> item2 in list)
		{
			IAgentOriginBase origin = item2.Value.Origin;
			_reservedTroops[origin] = NavalTroopAssignment.Create(origin);
		}
	}

	private Agent SpawnAgentAux(IAgentOriginBase agentOrigin, bool isReinforcement)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Invalid comparison between Unknown and I4
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Invalid comparison between Unknown and I4
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Invalid comparison between Unknown and I4
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame crewSpawnGlobalFrame;
		if (isReinforcement)
		{
			Ship.GetNextCrewSpawnGlobalFrame(out crewSpawnGlobalFrame);
		}
		else
		{
			crewSpawnGlobalFrame = Ship.GetNextOuterInnerSpawnGlobalFrame();
		}
		Vec2 asVec = ((Vec3)(ref crewSpawnGlobalFrame.rotation.f)).AsVec2;
		Vec2 value = ((Vec2)(ref asVec)).Normalized();
		bool flag = Ship.BattleSide == Mission.Current.PlayerTeam.Side;
		Agent val = Mission.Current.SpawnTroop(agentOrigin, flag, true, false, false, 0, 0, true, true, true, (Vec3?)crewSpawnGlobalFrame.origin, (Vec2?)value, (string)null, (ItemObject)null, Ship.FormationIndex, false);
		bool flag2 = false;
		foreach (ShipVisualSlotInfo shipVisualSlotInfo in Ship.ShipOrigin.GetShipVisualSlotInfos())
		{
			if (shipVisualSlotInfo.VisualSlotTag == "side" && shipVisualSlotInfo.VisualPieceId == "brazier")
			{
				flag2 = true;
			}
		}
		if (val != null && flag2)
		{
			EquipmentElement val5 = default(EquipmentElement);
			MissionWeapon val6 = default(MissionWeapon);
			EquipmentElement val8 = default(EquipmentElement);
			MissionWeapon val9 = default(MissionWeapon);
			for (EquipmentIndex val2 = (EquipmentIndex)0; (int)val2 < 4; val2 = (EquipmentIndex)(val2 + 1))
			{
				EquipmentElement val3 = val.SpawnEquipment[val2];
				ItemObject item = ((EquipmentElement)(ref val3)).Item;
				if (item != null && (int)item.ItemType == 5)
				{
					ItemObject val4 = Game.Current.ObjectManager.GetObject<ItemObject>("burning_arrows");
					((EquipmentElement)(ref val5))._002Ector(val4, (ItemModifier)null, (ItemObject)null, false);
					val.SpawnEquipment[val2] = val5;
					((MissionWeapon)(ref val6))._002Ector(((EquipmentElement)(ref val5)).Item, ((EquipmentElement)(ref val5)).ItemModifier, agentOrigin.Banner);
					val.RemoveEquippedWeapon(val2);
					val.EquipWeaponWithNewEntity(val2, ref val6);
					continue;
				}
				ItemObject item2 = ((EquipmentElement)(ref val3)).Item;
				if (item2 != null && (int)item2.ItemType == 6)
				{
					ItemObject val7 = Game.Current.ObjectManager.GetObject<ItemObject>("burning_bolts");
					((EquipmentElement)(ref val8))._002Ector(val7, (ItemModifier)null, (ItemObject)null, false);
					val.SpawnEquipment[val2] = val8;
					((MissionWeapon)(ref val9))._002Ector(((EquipmentElement)(ref val8)).Item, ((EquipmentElement)(ref val8)).ItemModifier, agentOrigin.Banner);
					val.RemoveEquippedWeapon(val2);
					val.EquipWeaponWithNewEntity(val2, ref val9);
				}
			}
		}
		HumanAIComponent component = val.GetComponent<HumanAIComponent>();
		if (component != null)
		{
			component.ForceDisablePickUpForAgent();
		}
		AddAgent(val);
		return val;
	}

	private Agent ReassignFromUnassignedReserves(NavalTroopAssignment suppliedTroop)
	{
		_teamAgents.ReassignAgentAux(this, suppliedTroop.Agent);
		return suppliedTroop.Agent;
	}

	internal Agent GetMinimumPriorityActiveAgent(MBList<Agent> agentsToIgnore = null)
	{
		Agent result = null;
		float num = float.MaxValue;
		bool flag = agentsToIgnore != null && ((List<Agent>)(object)agentsToIgnore).Count > 0;
		foreach (Agent item in (List<Agent>)(object)ActiveAgents)
		{
			if (!flag || !((List<Agent>)(object)agentsToIgnore).Contains(item))
			{
				float agentPriority = NavalAgentsLogic.GetAgentPriority(item);
				if (agentPriority <= num)
				{
					result = item;
					num = agentPriority;
				}
			}
		}
		return result;
	}
}
