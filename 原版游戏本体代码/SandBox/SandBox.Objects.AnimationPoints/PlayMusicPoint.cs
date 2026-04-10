using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.AnimationPoints;

public class PlayMusicPoint : AnimationPoint
{
	private InstrumentData _instrumentData;

	private SoundEvent _trackEvent;

	private bool _hasInstrumentAttached;

	protected override void OnInit()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		base.OnInit();
		((UsableMissionObject)this).IsDisabledForPlayers = true;
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	public void StartLoop(SoundEvent trackEvent)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		_trackEvent = trackEvent;
		if (((UsableMissionObject)this).HasUser && MBActionSet.CheckActionAnimationClipExists(((UsableMissionObject)this).UserAgent.ActionSet, ref LoopStartActionCode))
		{
			((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref LoopStartActionCode, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		}
	}

	public void EndLoop()
	{
		if (_trackEvent != null)
		{
			_trackEvent = null;
			ChangeInstrument(null);
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (((UsableMissionObject)this).HasUser)
		{
			return (TickRequirement)(2 | base.GetTickRequirement());
		}
		return base.GetTickRequirement();
	}

	protected override void OnTick(float dt)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.OnTick(dt);
		if (_trackEvent != null && ((UsableMissionObject)this).HasUser && MBActionSet.CheckActionAnimationClipExists(((UsableMissionObject)this).UserAgent.ActionSet, ref LoopStartActionCode))
		{
			((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref LoopStartActionCode, _hasInstrumentAttached, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		}
	}

	public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
		DefaultActionCode = ActionIndexCache.act_none;
		EndLoop();
	}

	public void ChangeInstrument(Tuple<InstrumentData, float> instrument)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		InstrumentData instrumentData = instrument?.Item1;
		if (_instrumentData == instrumentData)
		{
			return;
		}
		_instrumentData = instrumentData;
		if (!((UsableMissionObject)this).HasUser || !((UsableMissionObject)this).UserAgent.IsActive())
		{
			return;
		}
		if (((UsableMissionObject)this).UserAgent.IsSitting())
		{
			LoopStartAction = ((instrumentData == null) ? "act_sit_1" : instrumentData.SittingAction);
		}
		else
		{
			LoopStartAction = ((instrumentData == null) ? "act_stand_1" : instrumentData.StandingAction);
			ArriveAction = "";
		}
		ActionSpeed = instrument?.Item2 ?? 1f;
		SetActionCodes();
		ClearAssignedItems();
		((UsableMissionObject)this).UserAgent.SetActionChannel(0, ref LoopStartActionCode, false, (AnimFlags)Math.Min(((UsableMissionObject)this).UserAgent.GetCurrentActionPriority(0), 73), 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		if (_instrumentData == null)
		{
			return;
		}
		foreach (var item in (List<(HumanBone, string)>)(object)_instrumentData.InstrumentEntities)
		{
			ItemForBone newItem = new ItemForBone(item.Item1, item.Item2, isVisible: true);
			AssignItemToBone(newItem);
		}
		AddItemsToAgent();
		_hasInstrumentAttached = !_instrumentData.IsDataWithoutInstrument;
	}
}
