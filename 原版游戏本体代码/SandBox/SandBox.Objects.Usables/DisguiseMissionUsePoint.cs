using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.Usables;

public class DisguiseMissionUsePoint : UsableMissionObject
{
	public const float InteractionPointDistance = 2f;

	public DisguiseMissionUsePoint()
		: base(false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		TextObject val = new TextObject("{=!}Steal", (Dictionary<string, object>)null);
		val.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		base.ActionMessage = val;
		base.DescriptionMessage = new TextObject("{=!}Information.", (Dictionary<string, object>)null);
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=!}Steal the information", (Dictionary<string, object>)null);
	}

	public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
	{
		((UsableMissionObject)this).OnUse(userAgent, agentBoneIndex);
		_ = userAgent.IsMainAgent;
	}

	public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
		((UsableMissionObject)this).OnUseStopped(userAgent, isSuccessful, preferenceIndex);
		if (((UsableMissionObject)this).LockUserFrames || ((UsableMissionObject)this).LockUserPositions)
		{
			userAgent.ClearTargetFrame();
		}
	}

	public override bool IsDisabledForAgent(Agent agent)
	{
		return !agent.IsMainAgent;
	}

	public override bool IsUsableByAgent(Agent userAgent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = userAgent.Position;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		return ((Vec3)(ref position)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) < 2f;
	}

	public override WorldFrame GetUserFrameForAgent(Agent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return agent.GetWorldFrame();
	}
}
