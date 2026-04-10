using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.AI.UsableMachineAIs;
using NavalDLC.Missions.NavalPhysics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipPullingMachine : UsableMachine
{
	private const string ShipPullPointTag = "ShipPullPoint";

	private const float pullForceMult = 25f;

	private float currentDirection;

	private GameEntity pointToPull;

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	private void RotateMachine(float dt)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(0f - Input.GetMouseMoveX(), 0f - Input.GetMouseMoveY());
		if (((Vec2)(ref val)).IsNonZero())
		{
			float num2 = Math.Min(((Vec2)(ref val)).Normalize(), 5f) * 0.2f;
			num = val.x * num2;
		}
		if (num != 0f)
		{
			currentDirection += 1f * dt * num;
			currentDirection = MBMath.WrapAngle(currentDirection);
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame frame = ((WeakGameEntity)(ref gameEntity)).GetFrame();
		frame.rotation = Mat3.Identity;
		((Mat3)(ref frame.rotation)).RotateAboutUp(currentDirection);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetFrame(ref frame, true);
	}

	protected override void OnFixedTick(float fixedDt)
	{
	}

	protected override void OnTick(float dt)
	{
		if (((UsableMachine)this).UserCountNotInStruckAction <= 0 || ((UsableMachine)this).PilotAgent == null)
		{
			return;
		}
		RotateMachine(dt);
		if (!((UsableMachine)this).PilotAgent.IsInBeingStruckAction && ((UsableMachine)this).PilotAgent.Mission.InputManager.IsGameKeyDown(9))
		{
			if (pointToPull != (GameEntity)null)
			{
				PullOtherShip(pointToPull);
			}
		}
		else
		{
			FindPointToPull();
		}
	}

	private void FindPointToPull()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity pullPointHolderEntity = WeakGameEntity.Invalid;
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref val)).GetChildren())
		{
			WeakGameEntity current = child;
			if (((WeakGameEntity)(ref current)).Name == "pull_point_holder")
			{
				pullPointHolderEntity = current;
				break;
			}
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		Vec3 val2 = ((Vec3)(ref globalFrame.rotation.f)).NormalizedCopy();
		IEnumerable<GameEntity> enumerable = from x in ((ScriptComponentBehavior)this).Scene.FindEntitiesWithTag("ShipPullPoint")
			where x.Parent != pullPointHolderEntity
			select x;
		GameEntity val3 = null;
		float num = -1.1f;
		Vec3 lookDirection = ((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)[0]).UserAgent.LookDirection;
		Vec3 position = ((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)[0]).UserAgent.Position;
		((Vec3)(ref lookDirection)).Normalize();
		foreach (GameEntity item in enumerable)
		{
			MatrixFrame globalFrame2 = item.GetGlobalFrame();
			if (Vec3.DotProduct(globalFrame2.origin - globalFrame.origin, val2) > 0f && Vec3.DotProduct(((Vec3)(ref globalFrame2.rotation.f)).NormalizedCopy(), val2) < 0f)
			{
				Vec3 val4 = globalFrame2.origin - position;
				float num2 = Vec3.DotProduct(((Vec3)(ref val4)).NormalizedCopy(), lookDirection);
				if (num2 > num)
				{
					num = num2;
					val3 = item;
				}
			}
		}
		if (val3 != (GameEntity)null)
		{
			pointToPull = val3;
		}
	}

	private void PullOtherShip(GameEntity otherAttachmentPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		MissionShip firstScriptOfType = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<MissionShip>();
		MissionShip firstScriptOfType2 = otherAttachmentPoint.Root.GetFirstScriptOfType<MissionShip>();
		Vec3 globalPosition = otherAttachmentPoint.GlobalPosition;
		val = ((ScriptComponentBehavior)this).GameEntity;
		Vec3 val2 = globalPosition - ((WeakGameEntity)(ref val)).GlobalPosition;
		((Vec3)(ref val2)).Normalize();
		float num = 25f;
		NavalDLC.Missions.NavalPhysics.NavalPhysics physics = firstScriptOfType.Physics;
		val = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame frame = ((WeakGameEntity)(ref val)).GetFrame();
		physics.ApplyGlobalForceAtLocalPos(in frame.origin, val2 * num, (ForceMode)0);
		NavalDLC.Missions.NavalPhysics.NavalPhysics physics2 = firstScriptOfType2.Physics;
		frame = otherAttachmentPoint.GetFrame();
		physics2.ApplyGlobalForceAtLocalPos(in frame.origin, -val2 * num, (ForceMode)0);
	}

	protected override void OnMissionReset()
	{
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		TextObject val = new TextObject("{=fEQAPJ2e}{KEY} Use", (Dictionary<string, object>)null);
		val.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		return val;
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=5Pf5coO6}Ship Pulling machine", (Dictionary<string, object>)null);
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new ShipPullingMachineAI(this);
	}
}
