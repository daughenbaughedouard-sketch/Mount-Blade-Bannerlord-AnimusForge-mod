using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.UsableMachineAIs;

public sealed class ShipOarMachineAI : UsableMachineAIBase
{
	public override bool HasActionCompleted => false;

	protected override MovementOrder NextOrder => MovementOrder.MovementOrderCharge;

	private ShipOarMachine ShipOarMachine => base.UsableMachine as ShipOarMachine;

	public ShipOarMachineAI(ShipOarMachine shipOarMachine)
		: base((UsableMachine)(object)shipOarMachine)
	{
	}

	protected override void HandleAgentStopUsingStandingPoint(Agent agent, StandingPoint standingPoint)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (agent == ((UsableMachine)ShipOarMachine).PilotAgent)
		{
			ShipOarMachine.StartDelayedPilotRemoval(((UsableMachineAIBase)this).GetStopUsingStandingPointFlags(agent, standingPoint));
		}
		else
		{
			((UsableMachineAIBase)this).HandleAgentStopUsingStandingPoint(agent, standingPoint);
		}
	}
}
