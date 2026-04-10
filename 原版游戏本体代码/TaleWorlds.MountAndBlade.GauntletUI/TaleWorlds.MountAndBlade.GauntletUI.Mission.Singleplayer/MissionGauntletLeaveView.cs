using System;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionLeaveView))]
public class MissionGauntletLeaveView : MissionView
{
	private GauntletLayer _gauntletLayer;

	private MissionLeaveVM _dataSource;

	public override void OnMissionScreenInitialize()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		base.OnMissionScreenInitialize();
		_dataSource = new MissionLeaveVM((Func<float>)((MissionBehavior)this).Mission.GetMissionEndTimerValue, (Func<float>)((MissionBehavior)this).Mission.GetMissionEndTimeInSeconds);
		_gauntletLayer = new GauntletLayer("MissionLeave", 47, false);
		_gauntletLayer.LoadMovie("LeaveUI", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		base.OnMissionScreenFinalize();
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		_dataSource.Tick(dt);
	}

	private void OnEscapeMenuToggled(bool isOpened)
	{
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, !isOpened);
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}
}
