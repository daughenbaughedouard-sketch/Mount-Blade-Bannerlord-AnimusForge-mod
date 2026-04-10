using SandBox.Objects.AreaMarkers;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets;

public class MissionBasicAreaIndicatorMarkerTargetVM : MissionNameMarkerTargetVM<BasicAreaIndicator>
{
	private readonly Vec3 _position;

	public MissionBasicAreaIndicatorMarkerTargetVM(BasicAreaIndicator target, Vec3 position)
		: base(target)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.NameType = "Passage";
		base.IconType = (string.IsNullOrEmpty(base.Target.Type) ? "common_area" : base.Target.Type);
		_position = position;
		((ViewModel)this).RefreshValues();
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		UpdatePositionWith(missionCamera, _position + MissionNameMarkerHelper.DefaultHeightOffset);
	}

	protected override TextObject GetName()
	{
		return ((AreaMarker)base.Target).GetName();
	}
}
