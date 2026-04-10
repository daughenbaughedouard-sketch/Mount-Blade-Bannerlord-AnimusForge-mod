using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.TeamSelection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MultiplayerCultureSelectUIHandler))]
public class MissionGauntletCultureSelection : MissionView
{
	private GauntletLayer _gauntletLayer;

	private MultiplayerCultureSelectVM _dataSource;

	private MissionLobbyComponent _missionLobbyComponent;

	private bool _toOpen;

	public MissionGauntletCultureSelection()
	{
		base.ViewOrderPriority = 22;
	}

	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		_missionLobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_missionLobbyComponent.OnCultureSelectionRequested += OnCultureSelectionRequested;
	}

	public override void OnMissionScreenFinalize()
	{
		_missionLobbyComponent.OnCultureSelectionRequested -= OnCultureSelectionRequested;
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (_toOpen && ((MissionView)this).MissionScreen.SetDisplayDialog(true))
		{
			_toOpen = false;
			OnOpen();
		}
	}

	private void OnOpen()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		_dataSource = new MultiplayerCultureSelectVM(OnCultureSelected, OnClose);
		_gauntletLayer = new GauntletLayer("MultiplayerCultureSelection", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerCultureSelection", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	private void OnClose()
	{
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((MissionView)this).MissionScreen.SetDisplayDialog(false);
		((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	private void OnCultureSelectionRequested()
	{
		_toOpen = true;
	}

	private void OnCultureSelected(BasicCultureObject culture)
	{
		_missionLobbyComponent.OnCultureSelected(culture);
		OnClose();
	}
}
