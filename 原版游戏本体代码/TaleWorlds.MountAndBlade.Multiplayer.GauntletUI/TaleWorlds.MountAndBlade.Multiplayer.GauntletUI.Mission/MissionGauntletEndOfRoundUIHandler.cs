using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.EndOfRound;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MultiplayerEndOfRoundUIHandler))]
public class MissionGauntletEndOfRoundUIHandler : MissionView
{
	private MultiplayerEndOfRoundVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private MissionLobbyComponent _missionLobbyComponent;

	private MissionScoreboardComponent _scoreboardComponent;

	private MissionMultiplayerGameModeBaseClient _mpGameModeBase;

	private IRoundComponent RoundComponent => _mpGameModeBase.RoundComponent;

	public override void OnMissionScreenInitialize()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_missionLobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_scoreboardComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionScoreboardComponent>();
		_mpGameModeBase = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		base.ViewOrderPriority = 23;
		_dataSource = new MultiplayerEndOfRoundVM(_scoreboardComponent, _missionLobbyComponent, RoundComponent);
		_gauntletLayer = new GauntletLayer("MultiplayerAdminPanel", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerEndOfRound", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		if (RoundComponent != null)
		{
			RoundComponent.OnRoundStarted += RoundStarted;
			_scoreboardComponent.OnRoundPropertiesChanged += OnRoundPropertiesChanged;
			RoundComponent.OnPostRoundEnded += ShowEndOfRoundUI;
			_scoreboardComponent.OnMVPSelected += OnMVPSelected;
		}
		_missionLobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		if (RoundComponent != null)
		{
			RoundComponent.OnRoundStarted -= RoundStarted;
			_scoreboardComponent.OnRoundPropertiesChanged -= OnRoundPropertiesChanged;
			RoundComponent.OnPostRoundEnded -= ShowEndOfRoundUI;
			_scoreboardComponent.OnMVPSelected -= OnMVPSelected;
		}
		_missionLobbyComponent.OnPostMatchEnded -= OnPostMatchEnded;
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	private void RoundStarted()
	{
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		_dataSource.IsShown = false;
	}

	private void OnRoundPropertiesChanged()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		if (RoundComponent.RoundCount != 0 && (int)_missionLobbyComponent.CurrentMultiplayerState != 2)
		{
			_dataSource.Refresh();
		}
	}

	private void ShowEndOfRoundUI()
	{
		ShowEndOfRoundUI(isForced: false);
	}

	private void ShowEndOfRoundUI(bool isForced)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if (isForced || (RoundComponent.RoundCount != 0 && (int)_missionLobbyComponent.CurrentMultiplayerState != 2))
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)3);
			_dataSource.IsShown = true;
		}
	}

	private void OnPostMatchEnded()
	{
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		_dataSource.IsShown = false;
	}

	private void OnMVPSelected(MissionPeer mvpPeer, int mvpCount)
	{
		_dataSource.OnMVPSelected(mvpPeer);
	}
}
