using SandBox.Missions.MissionLogics.Arena;
using SandBox.View.Missions;
using SandBox.ViewModelCollection.Missions;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(MissionArenaPracticeFightView))]
public class MissionGauntletArenaPracticeFightView : MissionView
{
	private MissionArenaPracticeFightVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private GauntletMovieIdentifier _movie;

	public override void OnMissionScreenInitialize()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		ArenaPracticeFightMissionController missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<ArenaPracticeFightMissionController>();
		_dataSource = new MissionArenaPracticeFightVM(missionBehavior);
		_gauntletLayer = new GauntletLayer("MissionArenaPracticeFight", base.ViewOrderPriority, false);
		_movie = _gauntletLayer.LoadMovie("ArenaPracticeFight", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		_dataSource.Tick();
	}

	public override void OnMissionScreenFinalize()
	{
		((ViewModel)_dataSource).OnFinalize();
		_gauntletLayer.ReleaseMovie(_movie);
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override void OnPhotoModeActivated()
	{
		((MissionView)this).OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		((MissionView)this).OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}
}
