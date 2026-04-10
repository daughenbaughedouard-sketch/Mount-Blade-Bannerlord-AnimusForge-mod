using NavalDLC.Missions;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.View.MissionViews;
using NavalDLC.ViewModelCollection.Missions.CaptureShip;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(NavalMissionCaptureShipView))]
public class MissionGauntletNavalCaptureShipView : MissionView
{
	private GauntletLayer _gauntletLayer;

	private NavalMissionCaptureShipVM _dataSource;

	public ShipControllerMachine ControllerMachine { get; private set; }

	public override void OnMissionScreenInitialize()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_dataSource = new NavalMissionCaptureShipVM(3f);
		_gauntletLayer = new GauntletLayer("NavalMissionCaptureShip", 47, false);
		_gauntletLayer.LoadMovie("NavalMissionCaptureShip", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		Agent main = Agent.Main;
		ControllerMachine = ((main == null) ? null : main.GetComponent<AgentNavalComponent>()?.SteppedShip?.ShipControllerMachine);
		if (ControllerMachine != null && Agent.Main != null && ((UsableMachine)ControllerMachine).PilotAgent == Agent.Main)
		{
			_dataSource.UpdateCaptureTimer(ControllerMachine.CaptureTimer);
		}
		else
		{
			_dataSource.UpdateCaptureTimer(-1f);
		}
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
