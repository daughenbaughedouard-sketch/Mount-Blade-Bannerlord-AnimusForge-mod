using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.Usables;

public class ShadowingSecureZoneUsePoint : UsableMissionObject
{
	public ShadowingSecureZoneUsePoint()
		: base(false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		TextObject val = new TextObject("{=!}{KEY} Blend in", (Dictionary<string, object>)null);
		val.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		base.ActionMessage = val;
		base.DescriptionMessage = new TextObject("{=!}Blend", (Dictionary<string, object>)null);
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=!}Blend in", (Dictionary<string, object>)null);
	}

	public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
	{
		((UsableMissionObject)this).OnUse(userAgent, agentBoneIndex);
		if (userAgent.IsMainAgent)
		{
			userAgent.SetActionChannel(0, ref ActionIndexCache.act_idle_unarmed_1, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		}
	}

	public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
		((UsableMissionObject)this).OnUseStopped(userAgent, isSuccessful, preferenceIndex);
		if (userAgent.IsMainAgent)
		{
			userAgent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		}
	}

	public override bool IsDisabledForAgent(Agent agent)
	{
		return !agent.IsMainAgent;
	}
}
