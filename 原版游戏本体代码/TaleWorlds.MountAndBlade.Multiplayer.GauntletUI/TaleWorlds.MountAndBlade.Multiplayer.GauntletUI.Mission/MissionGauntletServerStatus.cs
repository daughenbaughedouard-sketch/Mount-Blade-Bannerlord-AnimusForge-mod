using NetworkMessages.FromServer;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerServerStatusUIHandler))]
public class MissionGauntletServerStatus : MissionView
{
	private MultiplayerMissionServerStatusVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private bool IsOptionEnabled => BannerlordConfig.EnableNetworkAlertIcons;

	public override void OnMissionScreenInitialize()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_dataSource = new MultiplayerMissionServerStatusVM();
		_gauntletLayer = new GauntletLayer("MultiplayerServerStatus", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerServerStatus", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		NetworkCommunicator.OnPeerAveragePingUpdated += OnPeerPingUpdated;
	}

	private void OnPeerPingUpdated(NetworkCommunicator obj)
	{
		if (IsOptionEnabled && obj.IsMine)
		{
			_dataSource.UpdatePeerPing(obj.AveragePingInMilliseconds);
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		((MissionView)this).OnMissionScreenTick(dt);
		if (IsOptionEnabled && GameNetwork.IsClient && GameNetwork.IsMyPeerReady)
		{
			_dataSource.UpdatePacketLossRatio((GameNetwork.MyPeer != null) ? ((float)GameNetwork.MyPeer.AverageLossPercent) : 0f);
			MultiplayerMissionServerStatusVM dataSource = _dataSource;
			NetworkCommunicator myPeer = GameNetwork.MyPeer;
			dataSource.UpdateServerPerformanceState((ServerPerformanceState)((myPeer != null) ? ((int)myPeer.ServerPerformanceProblemState) : 0));
		}
		else
		{
			_dataSource.ResetStates();
		}
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		NetworkCommunicator.OnPeerAveragePingUpdated -= OnPeerPingUpdated;
	}
}
