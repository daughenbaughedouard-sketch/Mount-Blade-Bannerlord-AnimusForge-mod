using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerMarkerUIHandler))]
public class MissionGauntletMultiplayerMarkerUIHandler : MissionView
{
	private GauntletLayer _gauntletLayer;

	private MultiplayerMissionMarkerVM _dataSource;

	public override void OnMissionScreenInitialize()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_dataSource = new MultiplayerMissionMarkerVM(((MissionView)this).MissionScreen.CombatCamera);
		_gauntletLayer = new GauntletLayer("MPMissionMarkers", 1, false);
		_gauntletLayer.LoadMovie("MPMissionMarkers", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (((MissionView)this).Input.IsGameKeyDown(5))
		{
			_dataSource.IsEnabled = true;
		}
		else
		{
			_dataSource.IsEnabled = false;
		}
		_dataSource.Tick(dt);
	}
}
