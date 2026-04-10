using System;
using System.Collections.Generic;
using NavalDLC.Missions.AI.UsableMachineAIs;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipAttachmentPointMachine : UsableMachine
{
	[EditableScriptComponentVariable(true, "")]
	public int RelatedShipNavmeshOffset;

	private GameEntity _focusObject;

	private MBList<GameEntity> _rampPhysicsList;

	public MissionShip OwnerShip { get; private set; }

	public ShipAttachmentMachine.ShipAttachment CurrentAttachment { get; private set; }

	public Vec3 HookAttachLocalPosition { get; private set; }

	public GameEntity ConnectionClipPlaneEntity { get; private set; }

	public GameEntity RampBarrier { get; private set; }

	internal MBReadOnlyList<GameEntity> RampPhysicsList => (MBReadOnlyList<GameEntity>)(object)_rampPhysicsList;

	public GameEntity RampVisualEntity { get; private set; }

	public ShipAttachmentMachine LinkedAttachmentMachine { get; private set; }

	protected override void OnInit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnInit();
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity parent = ((WeakGameEntity)(ref val)).Parent;
		while (OwnerShip == null && ((WeakGameEntity)(ref parent)).IsValid)
		{
			OwnerShip = ((WeakGameEntity)(ref parent)).GetFirstScriptOfType<MissionShip>();
			parent = ((WeakGameEntity)(ref parent)).Parent;
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Parent;
		if (((WeakGameEntity)(ref val)).GetScriptCountOfTypeRecursive<ShipAttachmentMachine>() == 1)
		{
			val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Parent;
			LinkedAttachmentMachine = ((WeakGameEntity)(ref val)).GetFirstScriptOfTypeRecursive<ShipAttachmentMachine>();
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		int childCount = ((WeakGameEntity)(ref val)).ChildCount;
		WeakGameEntity val2 = WeakGameEntity.Invalid;
		for (int i = 0; i < childCount; i++)
		{
			val = ((ScriptComponentBehavior)this).GameEntity;
			WeakGameEntity child = ((WeakGameEntity)(ref val)).GetChild(i);
			if (((WeakGameEntity)(ref child)).Name == "hook_attach_point")
			{
				Vec3 origin = ((WeakGameEntity)(ref child)).GetFrame().origin;
				MatrixFrame frame = ((WeakGameEntity)(ref child)).GetFrame();
				HookAttachLocalPosition = origin + 0.5f * ((Vec3)(ref frame.rotation.u)).NormalizedCopy();
				val2 = child;
			}
			else if (((WeakGameEntity)(ref child)).Name == "focus_object")
			{
				_focusObject = GameEntity.CreateFromWeakEntity(child);
			}
		}
		if (val2 != WeakGameEntity.Invalid)
		{
			((WeakGameEntity)(ref val2)).Remove(78);
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		ConnectionClipPlaneEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("connection_point"));
		val = ((ScriptComponentBehavior)this).GameEntity;
		RampBarrier = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTag("connection_barrier"));
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).GetChildrenWithTagRecursive(list, "step_capsule");
		_rampPhysicsList = new MBList<GameEntity>();
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current = item;
			if (((WeakGameEntity)(ref current)).GetVisibilityExcludeParents())
			{
				GameEntity val3 = GameEntity.CreateFromWeakEntity(current);
				val3.SetVisibilityExcludeParents(false);
				((List<GameEntity>)(object)_rampPhysicsList).Add(val3);
			}
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		RampVisualEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("bridge_target"));
		RampVisualEntity.SetVisibilityExcludeParents(false);
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
		base.EnemyRangeToStopUsing = 5f;
		base.IsDisabledForAttackerAIDueToEnemyInRange = new QueryData<bool>((Func<bool>)(() => OwnerShip != null && OwnerShip.ShipOrder != null && OwnerShip.ShipOrder.IsEnemyOnShip), 1f);
		base.IsDisabledForDefenderAIDueToEnemyInRange = new QueryData<bool>((Func<bool>)(() => OwnerShip != null && OwnerShip.ShipOrder != null && OwnerShip.ShipOrder.IsEnemyOnShip), 1f);
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(2 | ((UsableMachine)this).GetTickRequirement());
	}

	public override void OnDeploymentFinished()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).AddComponent((UsableMissionObjectComponent)new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none, false));
	}

	protected override void OnTick(float dt)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = OwnerShip.FireHitPoints > 0f && (LinkedAttachmentMachine?.CurrentAttachment != null || CurrentAttachment == null || (((UsableMachine)this).PilotAgent == null && (CurrentAttachment.State != ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected || OwnerShip.IsDisconnectionBlocked())));
		((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).SetIsDeactivatedSynched(flag);
		if (_focusObject.GetVisibilityExcludeParents() == flag)
		{
			_focusObject.SetVisibilityExcludeParents(!flag);
		}
		if (((UsableMachine)this).PilotAgent == null)
		{
			return;
		}
		if (OwnerShip.FireHitPoints <= 0f)
		{
			Vec3 f = ((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).GetUserFrameForAgent(((UsableMachine)this).PilotAgent).Rotation.f;
			((Vec3)(ref f)).Normalize();
			if (!(((UsableMachine)this).PilotAgent.GetCurrentAction(0) != ActionIndexCache.act_escape_jump))
			{
				return;
			}
			MatrixFrame frame = ((UsableMachine)this).PilotAgent.Frame;
			if (Vec3.DotProduct(((Vec3)(ref frame.rotation.f)).NormalizedCopy(), f) > 0.95f)
			{
				Agent pilotAgent = ((UsableMachine)this).PilotAgent;
				if (pilotAgent.Formation != null)
				{
					((IDetachment)this).RemoveAgent(pilotAgent);
					pilotAgent.Formation.AttachUnit(pilotAgent);
				}
				else
				{
					((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				}
				Vec3 val = pilotAgent.Position + f * 10f;
				pilotAgent.GetComponent<AgentNavalComponent>().SetupAgentToJumpOffABurningShip();
				pilotAgent.SetActionChannel(0, ref ActionIndexCache.act_escape_jump, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				Vec2 asVec = ((Vec3)(ref val)).AsVec2;
				pilotAgent.SetTargetPositionAndDirection(ref asVec, ref f);
				pilotAgent.ClearTargetFrame();
			}
		}
		else
		{
			if (CurrentAttachment == null || CurrentAttachment.State != ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected)
			{
				return;
			}
			if (((UsableMachine)this).PilotAgent.SetActionChannel(1, ref ActionIndexCache.act_ship_connection_break, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
			{
				if (((UsableMachine)this).PilotAgent.GetCurrentActionProgress(1) > 0.99f)
				{
					CurrentAttachment.AttachmentSource.DisconnectAttachment();
					((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				}
			}
			else
			{
				((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			}
		}
	}

	protected override float GetDetachmentWeightAux(BattleSideEnum side)
	{
		return float.MinValue;
	}

	public override bool ShouldAutoLeaveDetachmentWhenDisabled(BattleSideEnum sideEnum)
	{
		return false;
	}

	public void AssignConnection(ShipAttachmentMachine.ShipAttachment shipAttachment)
	{
		CurrentAttachment = shipAttachment;
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		TextObject val = new TextObject("{=PUbT3s7W}{KEY} Cut Loose", (Dictionary<string, object>)null);
		val.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		return val;
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		if ((CurrentAttachment == null || CurrentAttachment.State != ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected) && (LinkedAttachmentMachine?.CurrentAttachment == null || LinkedAttachmentMachine.CurrentAttachment.State != ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected))
		{
			return new TextObject("{=7zCPG8TR}Hook", (Dictionary<string, object>)null);
		}
		return new TextObject("{=kCMGJl1W}Bridge", (Dictionary<string, object>)null);
	}

	public bool IsShipAttachmentMachinePointBridgeWithEnemy()
	{
		if (CurrentAttachment != null)
		{
			Team val = CurrentAttachment?.AttachmentSource?.OwnerShip?.Team;
			Team val2 = CurrentAttachment?.AttachmentTarget?.OwnerShip?.Team;
			if (val != null && val2 != null && val.IsEnemyOf(val2))
			{
				return CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected;
			}
			return false;
		}
		return false;
	}

	public bool IsShipAttachmentPointBridged()
	{
		if (CurrentAttachment != null)
		{
			if (CurrentAttachment.State != ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected)
			{
				return CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeThrown;
			}
			return true;
		}
		return false;
	}

	public bool IsShipAttachmentPointConnectedToEnemy()
	{
		if (CurrentAttachment != null && (CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.RopesPulling || CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeThrown || CurrentAttachment.State == ShipAttachmentMachine.ShipAttachment.ShipAttachmentState.BridgeConnected) && CurrentAttachment.AttachmentSource.OwnerShip.Team != null && CurrentAttachment.AttachmentTarget.OwnerShip.Team != null && CurrentAttachment.AttachmentSource.OwnerShip.Team.IsEnemyOf(CurrentAttachment.AttachmentTarget.OwnerShip.Team))
		{
			Formation formation = CurrentAttachment.AttachmentSource.OwnerShip.Formation;
			if (formation == null)
			{
				return false;
			}
			return formation.CountOfUnits > 0;
		}
		return false;
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new ShipAttachmentPointAI(this);
	}

	protected override bool OnCheckForProblems()
	{
		return true;
	}
}
