using SandBox.Objects.AreaMarkers;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets;

public class MissionAnimatedBasicAreaIndicatorMarkerTargetVM : MissionNameMarkerTargetVM<AnimatedBasicAreaIndicator>
{
	public MissionAnimatedBasicAreaIndicatorMarkerTargetVM(AnimatedBasicAreaIndicator target)
		: base(target)
	{
		base.NameType = "Passage";
		base.IconType = (string.IsNullOrEmpty(base.Target.Type) ? "common_area" : base.Target.Type);
		((ViewModel)this).RefreshValues();
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		UpdatePositionWith(missionCamera, ((AreaMarker)base.Target).GetPosition() + MissionNameMarkerHelper.DefaultHeightOffset);
	}

	protected override TextObject GetName()
	{
		return ((AreaMarker)base.Target).GetName();
	}
}
