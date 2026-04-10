using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

public class NavalAgentMoraleInteractionLogic : MissionLogic
{
	private NavalShipsLogic _navalShipsLogic;

	public override void OnBehaviorInitialize()
	{
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.ShipsConnectedEvent += OnShipConnected;
	}

	public override void OnRemoveBehavior()
	{
		((MissionBehavior)this).OnRemoveBehavior();
		_navalShipsLogic.ShipsConnectedEvent -= OnShipConnected;
	}

	private void OnShipConnected(MissionShip ownerShip, MissionShip targetShip)
	{
		int num = 0;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ownerShip.ShipAttachmentMachines)
		{
			ShipAttachmentMachine.ShipAttachment currentAttachment = item.CurrentAttachment;
			if (currentAttachment != null && currentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected)
			{
				num++;
			}
			if (num > 1)
			{
				break;
			}
		}
		if (num != 1 || ownerShip.Team == null || targetShip?.Team == null || !ownerShip.Team.IsEnemyOf(targetShip.Team))
		{
			return;
		}
		foreach (Agent unitsWithoutDetachedOne in targetShip.Formation.GetUnitsWithoutDetachedOnes())
		{
			if (unitsWithoutDetachedOne.IsAIControlled)
			{
				float num2 = MissionGameModels.Current.BattleMoraleModel.CalculateMoraleOnShipsConnected(unitsWithoutDetachedOne, ownerShip.ShipOrigin, targetShip.ShipOrigin);
				AgentComponentExtensions.ChangeMorale(unitsWithoutDetachedOne, num2);
			}
		}
	}

	public void OnShipSunk(MissionShip ship)
	{
		float num = MissionGameModels.Current.BattleMoraleModel.CalculateMoraleChangeOnShipSunk(ship.ShipOrigin);
		if (ship.Team == null)
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)ship.Team.ActiveAgents)
		{
			if (item.IsAIControlled)
			{
				AgentComponentExtensions.ChangeMorale(item, num);
			}
		}
	}

	public void OnShipRammed(MissionShip rammingShip, MissionShip rammedShip)
	{
		if (rammingShip?.Team == null || rammedShip.Team == null || !rammingShip.Team.IsEnemyOf(rammedShip.Team))
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)rammingShip.Team.ActiveAgents)
		{
			if (item.IsAIControlled)
			{
				float num = MissionGameModels.Current.BattleMoraleModel.CalculateMoraleOnRamming(item, rammingShip.ShipOrigin, rammedShip.ShipOrigin);
				AgentComponentExtensions.ChangeMorale(item, num);
			}
		}
	}
}
