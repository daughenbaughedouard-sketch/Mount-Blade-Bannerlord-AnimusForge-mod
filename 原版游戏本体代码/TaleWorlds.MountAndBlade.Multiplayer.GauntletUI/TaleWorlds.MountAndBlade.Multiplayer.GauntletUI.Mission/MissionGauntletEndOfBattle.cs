using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MultiplayerEndOfBattleUIHandler))]
public class MissionGauntletEndOfBattle : MissionView
{
	private MultiplayerEndOfBattleVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private MissionLobbyComponent _lobbyComponent;

	public override void OnMissionScreenInitialize()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		base.ViewOrderPriority = 30;
		_dataSource = new MultiplayerEndOfBattleVM();
		_gauntletLayer = new GauntletLayer("MultiplayerEndOfBattle", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerEndOfBattle", (ViewModel)(object)_dataSource);
		_lobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_lobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		_lobbyComponent.OnPostMatchEnded -= OnPostMatchEnded;
	}

	private void OnPostMatchEnded()
	{
		_dataSource.OnBattleEnded();
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		_dataSource.OnTick(dt);
	}
}
