using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets.Hideout;

public class MissionStealthSentryNameMarkerTargetVM : MissionNameMarkerTargetVM<Agent>
{
	public MissionStealthSentryNameMarkerTargetVM(Agent target)
		: base(target)
	{
		base.IconType = "sentry";
		base.NameType = "Enemy";
		base.IsEnemy = true;
		((ViewModel)this).RefreshValues();
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		UpdatePositionWith(missionCamera, base.Target.GetEyeGlobalPosition() + MissionNameMarkerHelper.AgentHeightOffset);
	}

	protected override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=KdT0PM8Y}Sentry", (Dictionary<string, object>)null);
	}
}
