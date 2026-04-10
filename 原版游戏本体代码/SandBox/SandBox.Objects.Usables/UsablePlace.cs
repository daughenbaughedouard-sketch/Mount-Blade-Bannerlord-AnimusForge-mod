using SandBox.AI;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.Usables;

public class UsablePlace : UsableMachine
{
	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		return ((UsableMissionObject)(((UsableMachine)this).PilotStandingPoint?)).DescriptionMessage ?? TextObject.GetEmpty();
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		return ((UsableMissionObject)(((UsableMachine)this).PilotStandingPoint?)).ActionMessage;
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new UsablePlaceAI((UsableMachine)(object)this);
	}
}
