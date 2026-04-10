using System.Collections.Generic;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace TaleWorlds.MountAndBlade;

internal sealed class NetworkStatusReplicationComponent : UdpNetworkComponent
{
	private class NetworkStatusData
	{
		public float NextPingForceSendTime;

		public float NextPingTrySendTime;

		public int LastSentPingValue = -1;

		public float NextLossTrySendTime;

		public int LastSentLossValue;
	}

	private List<NetworkStatusData> _peerData = new List<NetworkStatusData>();

	private float _nextPerformanceStateTrySendTime;

	private ServerPerformanceState _lastSentPerformanceState;

	public override void OnUdpNetworkHandlerTick(float dt)
	{
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected O, but got Unknown
		if (!GameNetwork.IsServer)
		{
			return;
		}
		float totalMissionTime = MBCommon.GetTotalMissionTime();
		foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
		{
			if (!networkPeer.IsSynchronized)
			{
				continue;
			}
			while (_peerData.Count <= networkPeer.Index)
			{
				NetworkStatusData item = new NetworkStatusData();
				_peerData.Add(item);
			}
			double num = networkPeer.RefreshAndGetAveragePingInMilliseconds();
			NetworkStatusData networkStatusData = _peerData[networkPeer.Index];
			bool flag = networkStatusData.NextPingForceSendTime <= totalMissionTime;
			if (flag || networkStatusData.NextPingTrySendTime <= totalMissionTime)
			{
				int num2 = MathF.Round(num);
				if (flag || networkStatusData.LastSentPingValue != num2)
				{
					networkStatusData.LastSentPingValue = num2;
					networkStatusData.NextPingForceSendTime = totalMissionTime + 10f + MBRandom.RandomFloatRanged(1.5f, 2.5f);
					GameNetwork.BeginBroadcastModuleEvent();
					GameNetwork.WriteMessage((GameNetworkMessage)new PingReplication(networkPeer, num2));
					GameNetwork.EndBroadcastModuleEventUnreliable((EventBroadcastFlags)0, (NetworkCommunicator)null);
				}
				networkStatusData.NextPingTrySendTime = totalMissionTime + MBRandom.RandomFloatRanged(1.5f, 2.5f);
			}
			if (!networkPeer.IsServerPeer && networkStatusData.NextLossTrySendTime <= totalMissionTime)
			{
				networkStatusData.NextLossTrySendTime = totalMissionTime + MBRandom.RandomFloatRanged(1.5f, 2.5f);
				int num3 = (int)networkPeer.RefreshAndGetAverageLossPercent();
				if (networkStatusData.LastSentLossValue != num3)
				{
					networkStatusData.LastSentLossValue = num3;
					GameNetwork.BeginModuleEventAsServer(networkPeer);
					GameNetwork.WriteMessage((GameNetworkMessage)new LossReplicationMessage(num3));
					GameNetwork.EndModuleEventAsServer();
				}
			}
		}
		if (_nextPerformanceStateTrySendTime <= totalMissionTime)
		{
			_nextPerformanceStateTrySendTime = totalMissionTime + MBRandom.RandomFloatRanged(1.5f, 2.5f);
			ServerPerformanceState serverPerformanceState = GetServerPerformanceState();
			if (serverPerformanceState != _lastSentPerformanceState)
			{
				_lastSentPerformanceState = serverPerformanceState;
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage((GameNetworkMessage)new ServerPerformanceStateReplicationMessage(serverPerformanceState));
				GameNetwork.EndBroadcastModuleEventUnreliable((EventBroadcastFlags)0, (NetworkCommunicator)null);
			}
		}
	}

	public NetworkStatusReplicationComponent()
	{
		if (GameNetwork.IsClientOrReplay)
		{
			AddRemoveMessageHandlers((RegisterMode)0);
		}
	}

	public override void OnUdpNetworkHandlerClose()
	{
		((UdpNetworkComponent)this).OnUdpNetworkHandlerClose();
		if (GameNetwork.IsClientOrReplay)
		{
			AddRemoveMessageHandlers((RegisterMode)1);
		}
	}

	private static void HandleServerMessagePingReplication(PingReplication message)
	{
		NetworkCommunicator peer = message.Peer;
		if (peer != null)
		{
			peer.SetAveragePingInMillisecondsAsClient((double)message.PingValue);
		}
	}

	private static void HandleServerMessageLossReplication(LossReplicationMessage message)
	{
		if (GameNetwork.IsMyPeerReady)
		{
			GameNetwork.MyPeer.SetAverageLossPercentAsClient((double)message.LossValue);
		}
	}

	private static void HandleServerMessageServerPerformanceStateReplication(ServerPerformanceStateReplicationMessage message)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (GameNetwork.IsMyPeerReady)
		{
			GameNetwork.MyPeer.SetServerPerformanceProblemStateAsClient(message.ServerPerformanceProblemState);
		}
	}

	private static void AddRemoveMessageHandlers(RegisterMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		NetworkMessageHandlerRegisterer val = new NetworkMessageHandlerRegisterer(mode);
		val.Register<PingReplication>((ServerMessageHandlerDelegate<PingReplication>)HandleServerMessagePingReplication);
		val.Register<LossReplicationMessage>((ServerMessageHandlerDelegate<LossReplicationMessage>)HandleServerMessageLossReplication);
		val.Register<ServerPerformanceStateReplicationMessage>((ServerMessageHandlerDelegate<ServerPerformanceStateReplicationMessage>)HandleServerMessageServerPerformanceStateReplication);
	}

	private ServerPerformanceState GetServerPerformanceState()
	{
		if (Mission.Current != null)
		{
			float averageFps = Mission.Current.GetAverageFps();
			if (!(averageFps >= 50f))
			{
				if (!(averageFps >= 30f))
				{
					return (ServerPerformanceState)2;
				}
				return (ServerPerformanceState)1;
			}
			return (ServerPerformanceState)0;
		}
		return (ServerPerformanceState)0;
	}
}
