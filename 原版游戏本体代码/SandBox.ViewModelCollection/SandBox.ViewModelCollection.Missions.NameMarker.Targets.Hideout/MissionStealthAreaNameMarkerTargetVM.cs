using System.Collections.Generic;
using SandBox.Objects.AreaMarkers;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets.Hideout;

public class MissionStealthAreaNameMarkerTargetVM : MissionNameMarkerTargetVM<StealthAreaMarker>
{
	private readonly Vec3 _position;

	public MissionStealthAreaNameMarkerTargetVM(StealthAreaMarker target, Vec3 position)
		: base(target)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
		base.NameType = "Passage";
		base.IconType = "stealth_area";
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
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=WcSky2KB}Stealth Area", (Dictionary<string, object>)null);
	}
}
