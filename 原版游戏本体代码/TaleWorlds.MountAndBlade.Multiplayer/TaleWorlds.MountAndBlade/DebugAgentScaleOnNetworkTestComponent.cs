using System;
using System.Collections.Generic;
using NetworkMessages.FromServer;
using TaleWorlds.MountAndBlade.Missions;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace TaleWorlds.MountAndBlade;

internal sealed class DebugAgentScaleOnNetworkTestComponent : UdpNetworkComponent
{
	private float _lastTestSendTime;

	public override void OnUdpNetworkHandlerTick(float dt)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		if (!GameNetwork.IsServer)
		{
			return;
		}
		float totalMissionTime = MBCommon.GetTotalMissionTime();
		if (_lastTestSendTime < totalMissionTime + 10f)
		{
			AgentReadOnlyList agents = Mission.Current.Agents;
			int count = ((List<Agent>)(object)agents).Count;
			_lastTestSendTime = totalMissionTime;
			int index = (int)(new Random().NextDouble() * (double)count);
			Agent val = ((List<Agent>)(object)agents)[index];
			if (val.IsActive())
			{
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage((GameNetworkMessage)new DebugAgentScaleOnNetworkTest(val.Index, val.AgentScale));
				GameNetwork.EndBroadcastModuleEventUnreliable((EventBroadcastFlags)0, (NetworkCommunicator)null);
			}
		}
	}

	public DebugAgentScaleOnNetworkTestComponent()
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

	private static void HandleServerMessageDebugAgentScaleOnNetworkTest(DebugAgentScaleOnNetworkTest message)
	{
		Agent agentFromIndex = MissionNetworkHelper.GetAgentFromIndex(message.AgentToTestIndex, true);
		if (agentFromIndex != null && agentFromIndex.IsActive())
		{
			((Float)(ref CompressionMission.DebugScaleValueCompressionInfo)).GetPrecision();
			_ = agentFromIndex.AgentScale;
		}
	}

	private static void AddRemoveMessageHandlers(RegisterMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		new NetworkMessageHandlerRegisterer(mode).Register<DebugAgentScaleOnNetworkTest>((ServerMessageHandlerDelegate<DebugAgentScaleOnNetworkTest>)HandleServerMessageDebugAgentScaleOnNetworkTest);
	}
}
