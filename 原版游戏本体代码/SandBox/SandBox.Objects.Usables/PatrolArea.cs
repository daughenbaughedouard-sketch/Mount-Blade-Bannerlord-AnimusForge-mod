using System.Collections.Generic;
using SandBox.AI;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.Usables;

public class PatrolArea : UsableMachine
{
	public int AreaIndex;

	private int _activeIndex;

	private int ActiveIndex
	{
		get
		{
			return _activeIndex;
		}
		set
		{
			if (_activeIndex != value)
			{
				((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)[value]).IsDeactivated = false;
				((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)[_activeIndex]).IsDeactivated = true;
				_activeIndex = value;
			}
			if (((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints).Count == 1 && _activeIndex == 0 && ((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)[_activeIndex]).IsDeactivated)
			{
				((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)[_activeIndex]).IsDeactivated = false;
			}
		}
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		return ((UsableMissionObject)(((UsableMachine)this).PilotStandingPoint?)).ActionMessage;
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		return ((UsableMissionObject)(((UsableMachine)this).PilotStandingPoint?)).DescriptionMessage ?? TextObject.GetEmpty();
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new UsablePlaceAI((UsableMachine)(object)this);
	}

	protected override void OnInit()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnInit();
		foreach (StandingPoint item in (List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)
		{
			((UsableMissionObject)item).IsDeactivated = true;
		}
		ActiveIndex = ((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints).Count - 1;
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(2 | ((UsableMachine)this).GetTickRequirement());
	}

	protected override void OnTick(float dt)
	{
		((UsableMachine)this).OnTick(dt);
		if (((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)[ActiveIndex]).HasAIUser)
		{
			ActiveIndex = ((ActiveIndex == 0) ? (((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints).Count - 1) : (ActiveIndex - 1));
		}
	}
}
