using StoryMode.Missions;
using StoryMode.View.Missions;
using StoryMode.ViewModelCollection.Missions;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace StoryMode.GauntletUI.Missions;

[OverrideView(typeof(MissionTrainingFieldObjectiveView))]
public class MissionGauntletTrainingFieldObjectiveView : MissionView
{
	private TrainingFieldObjectivesVM _dataSource;

	private GauntletLayer _layer;

	private float _beginningTime;

	private bool _isTimerActive;

	public override void OnMissionScreenInitialize()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		TrainingFieldMissionController missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<TrainingFieldMissionController>();
		_dataSource = new TrainingFieldObjectivesVM();
		_dataSource.UpdateCurrentObjectiveExplanationText(missionBehavior.InitialCurrentObjective);
		_layer = new GauntletLayer("TrainingFieldObjectives", 2, false);
		_layer.LoadMovie("TrainingFieldObjectives", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_layer);
		missionBehavior.TimerTick = _dataSource.UpdateTimerText;
		missionBehavior.CurrentObjectiveTick = _dataSource.UpdateCurrentObjectiveExplanationText;
		missionBehavior.AllObjectivesTick = _dataSource.UpdateObjectivesWith;
		missionBehavior.UIStartTimer = BeginTimer;
		missionBehavior.UIEndTimer = EndTimer;
		missionBehavior.CurrentMouseObjectiveTick = _dataSource.UpdateCurrentMouseObjective;
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (_isTimerActive)
		{
			_dataSource.UpdateTimerText((((MissionBehavior)this).Mission.CurrentTime - _beginningTime).ToString("0.0"));
		}
	}

	private void BeginTimer()
	{
		_isTimerActive = true;
		_beginningTime = ((MissionBehavior)this).Mission.CurrentTime;
	}

	private float EndTimer()
	{
		_isTimerActive = false;
		_dataSource.UpdateTimerText("");
		return ((MissionBehavior)this).Mission.CurrentTime - _beginningTime;
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_layer);
		_dataSource = null;
		_layer = null;
	}

	public override void OnPhotoModeActivated()
	{
		((MissionView)this).OnPhotoModeActivated();
		if (_layer != null)
		{
			_layer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		((MissionView)this).OnPhotoModeDeactivated();
		if (_layer != null)
		{
			_layer.UIContext.ContextAlpha = 1f;
		}
	}
}
