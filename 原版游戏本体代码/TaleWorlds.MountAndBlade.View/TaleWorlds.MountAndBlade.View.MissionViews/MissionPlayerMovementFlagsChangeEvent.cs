using TaleWorlds.Library.EventSystem;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class MissionPlayerMovementFlagsChangeEvent : EventBase
{
	public MovementControlFlag MovementFlag { get; private set; }

	public MissionPlayerMovementFlagsChangeEvent(MovementControlFlag movementFlag)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		MovementFlag = movementFlag;
	}
}
