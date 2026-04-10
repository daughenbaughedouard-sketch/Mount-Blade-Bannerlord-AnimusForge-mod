using SandBox.Objects;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets;

public class MissionPassageUsePointNameMarkerTargetVM : MissionNameMarkerTargetVM<PassageUsePoint>
{
	public MissionPassageUsePointNameMarkerTargetVM(PassageUsePoint target)
		: base(target)
	{
		base.NameType = "Passage";
		base.IconType = ((base.Target.ToLocation == null && base.Target.IsMissionExit) ? "center" : base.Target.ToLocation.StringId);
		base.Quests = new MBBindingList<QuestMarkerVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)base.Target).GameEntity;
		UpdatePositionWith(missionCamera, ((WeakGameEntity)(ref gameEntity)).GlobalPosition + MissionNameMarkerHelper.DefaultHeightOffset);
	}

	protected override TextObject GetName()
	{
		if (base.Target.ToLocation == null && base.Target.IsMissionExit)
		{
			return GameTexts.FindText("str_mission_exit", (string)null);
		}
		return base.Target.ToLocation.Name;
	}
}
