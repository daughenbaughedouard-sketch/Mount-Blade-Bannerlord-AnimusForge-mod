using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets;

public class MissionWorkshopNameMarkerTargetVM : MissionNameMarkerTargetVM<Workshop>
{
	private readonly Vec3 _signPosition;

	public MissionWorkshopNameMarkerTargetVM(Workshop target, Vec3 signPosition)
		: base(target)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.NameType = "Passage";
		base.IconType = ((MBObjectBase)target.WorkshopType).StringId;
		_signPosition = signPosition;
		((ViewModel)this).RefreshValues();
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		UpdatePositionWith(missionCamera, _signPosition + MissionNameMarkerHelper.DefaultHeightOffset);
	}

	protected override TextObject GetName()
	{
		return base.Target.WorkshopType.Name;
	}
}
