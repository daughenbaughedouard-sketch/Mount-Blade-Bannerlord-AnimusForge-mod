using System.Collections.Generic;
using System.Linq;
using SandBox.Objects;
using SandBox.ViewModelCollection.Missions.NameMarker;
using SandBox.ViewModelCollection.Missions.NameMarker.Targets;
using Storymode.Missions;
using TaleWorlds.MountAndBlade;

namespace StoryMode.View.MarkerProviders;

public class StealthTutorialMarkerProvider : MissionNameMarkerProvider
{
	private SneakIntoTheVillaMissionController _controller;

	private SneakIntoTheVillaMissionController Controller
	{
		get
		{
			if (_controller == null)
			{
				Mission current = Mission.Current;
				_controller = ((current != null) ? current.GetMissionBehavior<SneakIntoTheVillaMissionController>() : null);
			}
			return _controller;
		}
	}

	public override void CreateMarkers(List<MissionNameMarkerTargetBaseVM> markers)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		foreach (PassageUsePoint item in MBExtensions.FindAllWithType<PassageUsePoint>((IEnumerable<MissionObject>)Mission.Current.ActiveMissionObjects).ToList())
		{
			if (item.IsMissionExit && !((UsableMissionObject)item).IsDeactivated)
			{
				markers.Add((MissionNameMarkerTargetBaseVM)new MissionPassageUsePointNameMarkerTargetVM(item));
			}
		}
		if (Controller != null)
		{
			markers.Add((MissionNameMarkerTargetBaseVM)new MissionAgentMarkerTargetVM(Controller.HeadmanAgent));
		}
	}

	protected override void OnTick(float dt)
	{
		if (Controller != null && Controller.AreVisualsDirty)
		{
			((MissionNameMarkerProvider)this).SetMarkersDirty();
			Controller.AreVisualsDirty = false;
		}
	}
}
