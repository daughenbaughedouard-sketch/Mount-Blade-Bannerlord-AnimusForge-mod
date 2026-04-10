using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.UsableMachineAIs;

public sealed class ShipAttachmentMachineAI : UsableMachineAIBase
{
	public override bool HasActionCompleted => false;

	protected override MovementOrder NextOrder => MovementOrder.MovementOrderCharge;

	private ShipAttachmentMachine ShipAttachmentMachine => base.UsableMachine as ShipAttachmentMachine;

	public ShipAttachmentMachineAI(ShipAttachmentMachine shipAttachmentMachine)
		: base((UsableMachine)(object)shipAttachmentMachine)
	{
	}
}
