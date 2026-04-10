using System.Collections.Generic;
using NavalDLC.Missions.Objects.UsableMachines;
using SandBox.ViewModelCollection.Missions.NameMarker;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.ViewModelCollection.Missions.NameMarkers;

public class AgentBindsMachineMarkerTargetVM : MissionNameMarkerTargetVM<AgentBindsMachine>
{
	public AgentBindsMachineMarkerTargetVM(AgentBindsMachine target)
		: base(target)
	{
		((MissionNameMarkerTargetBaseVM)this).NameType = "Normal";
		((MissionNameMarkerTargetBaseVM)this).IconType = "prisoner";
		((ViewModel)this).RefreshValues();
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (Agent.Main == null || !((UsableMachine)base.Target).IsStandingPointAvailableForAgent(Agent.Main))
		{
			((MissionNameMarkerTargetBaseVM)this).ScreenPosition = new Vec2(-5000f, -5000f);
			((MissionNameMarkerTargetBaseVM)this).Distance = -1;
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)base.Target).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		gameEntity = ((ScriptComponentBehavior)base.Target).GameEntity;
		((MissionNameMarkerTargetBaseVM)this).UpdatePositionWith(missionCamera, globalPosition + ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation.u * 1.5f);
	}

	protected override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=mx9zqEzQ}Unchain", (Dictionary<string, object>)null);
	}
}
