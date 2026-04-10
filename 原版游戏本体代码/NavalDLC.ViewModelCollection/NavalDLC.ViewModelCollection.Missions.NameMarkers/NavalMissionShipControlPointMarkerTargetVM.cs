using System.Collections.Generic;
using NavalDLC.Missions.Objects.UsableMachines;
using SandBox.ViewModelCollection.Missions.NameMarker;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.ViewModelCollection.Missions.NameMarkers;

public class NavalMissionShipControlPointMarkerTargetVM : MissionNameMarkerTargetVM<ShipControllerMachine>
{
	public NavalMissionShipControlPointMarkerTargetVM(ShipControllerMachine target)
		: base(target)
	{
		((MissionNameMarkerTargetBaseVM)this).NameType = "Normal";
		((MissionNameMarkerTargetBaseVM)this).IconType = "control_point";
		((ViewModel)this).RefreshValues();
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (Agent.Main == null || !((UsableMachine)base.Target).IsStandingPointAvailableForAgent(Agent.Main))
		{
			((MissionNameMarkerTargetBaseVM)this).ScreenPosition = new Vec2(-5000f, -5000f);
			((MissionNameMarkerTargetBaseVM)this).Distance = -1;
			return;
		}
		if (base.Target.HandTargetEntity != (GameEntity)null)
		{
			((MissionNameMarkerTargetBaseVM)this).UpdatePositionWith(missionCamera, base.Target.HandTargetEntity.GlobalPosition + base.Target.HandTargetEntity.GetGlobalFrame().rotation.u * 1.5f);
			return;
		}
		if (base.Target.ControllerEntity != (GameEntity)null)
		{
			((MissionNameMarkerTargetBaseVM)this).UpdatePositionWith(missionCamera, base.Target.ControllerEntity.GlobalPosition + base.Target.ControllerEntity.GetGlobalFrame().rotation.u * 1.5f);
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
		return new TextObject("{=OGY9BKOM}Control the Ship", (Dictionary<string, object>)null);
	}
}
