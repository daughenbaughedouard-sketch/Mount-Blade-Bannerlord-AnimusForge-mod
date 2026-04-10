using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects;

public class TeleportUsePoint : StandingPoint
{
	public enum TeleportType
	{
		Lair,
		Door,
		Gate
	}

	public TeleportType TypeOfTeleport;

	public string TargetPointTag;

	public bool IsLeave;

	private const float LairInteractionDistance = 0.5f;

	private const float GateInteractionDistance = 2.5f;

	public override bool HasAIMovingTo => false;

	public TeleportUsePoint()
	{
		((UsableMissionObject)this).IsInstantUse = true;
		((UsableMissionObject)this).LockUserFrames = false;
		((UsableMissionObject)this).LockUserFrames = false;
	}

	public override bool IsAIMovingTo(Agent agent)
	{
		return false;
	}

	protected override void OnInit()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		((UsableMissionObject)this).DescriptionMessage = TextObject.GetEmpty();
		if (IsLeave)
		{
			((UsableMissionObject)this).ActionMessage = GameTexts.FindText("str_mission_exit", (string)null);
			return;
		}
		switch (TypeOfTeleport)
		{
		case TeleportType.Lair:
			((UsableMissionObject)this).ActionMessage = GameTexts.FindText("str_ui_lair", (string)null);
			break;
		case TeleportType.Door:
			((UsableMissionObject)this).ActionMessage = GameTexts.FindText("str_ui_door", (string)null);
			break;
		case TeleportType.Gate:
			((UsableMissionObject)this).ActionMessage = new TextObject("{=6wZUG0ev}Gate", (Dictionary<string, object>)null);
			break;
		}
	}

	public override bool IsUsableByAgent(Agent userAgent)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (userAgent.IsPlayerControlled && !((UsableMissionObject)this).IsDeactivated)
		{
			WeakGameEntity interactionEntity = ((UsableMissionObject)this).InteractionEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref interactionEntity)).GetGlobalFrame();
			Vec2 asVec = ((Vec3)(ref globalFrame.origin)).AsVec2;
			Vec3 position = userAgent.Position;
			float num = ((Vec2)(ref asVec)).DistanceSquared(((Vec3)(ref position)).AsVec2);
			float interactionDistance = GetInteractionDistance();
			return num <= interactionDistance * interactionDistance;
		}
		return false;
	}

	public override bool IsDisabledForAgent(Agent agent)
	{
		if (agent.IsPlayerControlled && !((UsableMissionObject)this).IsDisabledForPlayers)
		{
			return ((UsableMissionObject)this).IsDeactivated;
		}
		return true;
	}

	protected override void OnTick(float dt)
	{
	}

	public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (!((UsableMissionObject)this).IsDeactivated && ((int)Campaign.Current.GameMode == 1 || userAgent.IsPlayerControlled))
		{
			((StandingPoint)this).OnUse(userAgent, agentBoneIndex);
			GameEntity val = Mission.Current.Scene.FindEntityWithTag(TargetPointTag);
			WorldPosition val2 = ModuleExtensions.ToWorldPosition(val.GetGlobalFrame().origin);
			userAgent.TeleportToPosition(((WorldPosition)(ref val2)).GetGroundVec3());
			userAgent.FadeIn();
		}
	}

	public void Deactivate()
	{
		((UsableMissionObject)this).IsDeactivated = true;
		((UsableMissionObject)this).ActionMessage = TextObject.GetEmpty();
	}

	public void Activate()
	{
		((UsableMissionObject)this).IsDeactivated = false;
		((ScriptComponentBehavior)this).OnInit();
	}

	public override void OnFocusGain(Agent userAgent)
	{
		if (!((UsableMissionObject)this).IsDeactivated)
		{
			((UsableMissionObject)this).OnFocusGain(userAgent);
		}
	}

	private float GetInteractionDistance()
	{
		if (TypeOfTeleport == TeleportType.Lair)
		{
			return 0.5f;
		}
		return 2.5f;
	}
}
