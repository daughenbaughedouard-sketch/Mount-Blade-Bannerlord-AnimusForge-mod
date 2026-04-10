using SandBox.Missions.MissionLogics;
using SandBox.View.Missions;
using SandBox.ViewModelCollection.Missions;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(MissionQuestBarView))]
public class MissionGauntletQuestBarView : MissionQuestBarView
{
	private const float MinProgressValue = 0f;

	private const float MaxProgressValue = 1f;

	private GauntletLayer _gauntletLayer;

	private MissionQuestBarVM _dataSource;

	private IMissionProgressTracker _missionProgressTracker;

	public override void OnMissionScreenInitialize()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_dataSource = new MissionQuestBarVM();
		_gauntletLayer = new GauntletLayer("MissionQuestBar", 10, false);
		_gauntletLayer.LoadMovie("MissionQuestBar", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		foreach (MissionBehavior missionBehavior in ((MissionBehavior)this).Mission.MissionBehaviors)
		{
			if (missionBehavior is IMissionProgressTracker)
			{
				_missionProgressTracker = missionBehavior as IMissionProgressTracker;
				break;
			}
		}
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		((ViewModel)_dataSource).OnFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		_dataSource = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (_missionProgressTracker != null)
		{
			_dataSource.UpdateQuestValues(0f, 1f, _missionProgressTracker.CurrentProgress);
		}
	}
}
