using System.Collections.Generic;
using TaleWorlds.Engine;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipAttachmentMachineConnectionLogic : ScriptComponentBehavior
{
	private MissionShip _ownerShip;

	private void FillAttachmentMachinesList()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		_ownerShip = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<MissionShip>();
	}

	protected override void OnInit()
	{
		((ScriptComponentBehavior)this).OnInit();
		FillAttachmentMachinesList();
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnTick(float dt)
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_ownerShip.AttachmentMachines)
		{
			if (item.CurrentAttachment == null || item.CurrentAttachment.State != ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.RopesPulling || !item.CurrentAttachment.ShouldLookForBetterConnections())
			{
				continue;
			}
			ShipAttachmentPointMachine attachmentTarget = item.CurrentAttachment.AttachmentTarget;
			MissionShip ownerShip = attachmentTarget.OwnerShip;
			float num = ShipAttachmentMachine.ComputePotentialAttachmentValue(item, attachmentTarget, checkInteractionDistance: false, checkConnectionBlock: false, allowWiderAngleBetweenConnections: true);
			if (!(num > 0f))
			{
				continue;
			}
			float num2 = num * 1.2f;
			ShipAttachmentPointMachine shipAttachmentPointMachine = null;
			foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)ownerShip.AttachmentPointMachines)
			{
				if (attachmentTarget != item2 && item2.CurrentAttachment == null && item2.LinkedAttachmentMachine?.CurrentAttachment == null)
				{
					float num3 = ShipAttachmentMachine.ComputePotentialAttachmentValue(item, attachmentTarget, checkInteractionDistance: true, checkConnectionBlock: true, allowWiderAngleBetweenConnections: false);
					if (num3 > num2)
					{
						num2 = num3;
						shipAttachmentPointMachine = item2;
					}
				}
			}
			if (shipAttachmentPointMachine != null)
			{
				item.CurrentAttachment.Destroy();
				item.ConnectWithAttachmentPointMachine(shipAttachmentPointMachine);
			}
		}
	}
}
