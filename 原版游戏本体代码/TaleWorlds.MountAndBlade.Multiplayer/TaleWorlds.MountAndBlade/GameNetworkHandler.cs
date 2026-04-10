using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Multiplayer.NetworkComponents;

namespace TaleWorlds.MountAndBlade;

public class GameNetworkHandler : IGameNetworkHandler
{
	void IGameNetworkHandler.OnNewPlayerConnect(PlayerConnectionInfo playerConnectionInfo, NetworkCommunicator networkPeer)
	{
		if (networkPeer != null)
		{
			GameManagerBase.Current.OnPlayerConnect(networkPeer.VirtualPlayer);
		}
	}

	void IGameNetworkHandler.OnInitialize()
	{
		MultiplayerGameTypes.Initialize();
	}

	void IGameNetworkHandler.OnPlayerConnectedToServer(NetworkCommunicator networkPeer)
	{
		if (Mission.Current == null)
		{
			return;
		}
		foreach (MissionBehavior missionBehavior in Mission.Current.MissionBehaviors)
		{
			MissionNetwork val;
			if ((val = (MissionNetwork)(object)((missionBehavior is MissionNetwork) ? missionBehavior : null)) != null)
			{
				val.OnPlayerConnectedToServer(networkPeer);
			}
		}
	}

	void IGameNetworkHandler.OnDisconnectedFromServer()
	{
		if (Mission.Current != null)
		{
			BannerlordNetwork.EndMultiplayerLobbyMission();
		}
	}

	void IGameNetworkHandler.OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
	{
		GameManagerBase.Current.OnPlayerDisconnect(networkPeer.VirtualPlayer);
		if (Mission.Current == null)
		{
			return;
		}
		foreach (MissionBehavior missionBehavior in Mission.Current.MissionBehaviors)
		{
			MissionNetwork val;
			if ((val = (MissionNetwork)(object)((missionBehavior is MissionNetwork) ? missionBehavior : null)) != null)
			{
				val.OnPlayerDisconnectedFromServer(networkPeer);
			}
		}
	}

	void IGameNetworkHandler.OnStartMultiplayer()
	{
		GameNetwork.AddNetworkComponent<BaseNetworkComponentData>();
		GameNetwork.AddNetworkComponent<BaseNetworkComponent>();
		GameNetwork.AddNetworkComponent<LobbyNetworkComponent>();
		GameNetwork.AddNetworkComponent<MultiplayerPermissionHandler>();
		GameNetwork.AddNetworkComponent<NetworkStatusReplicationComponent>();
		GameManagerBase.Current.OnGameNetworkBegin();
	}

	void IGameNetworkHandler.OnEndMultiplayer()
	{
		GameManagerBase.Current.OnGameNetworkEnd();
		GameNetwork.DestroyComponent((UdpNetworkComponent)(object)GameNetwork.GetNetworkComponent<LobbyNetworkComponent>());
		GameNetwork.DestroyComponent((UdpNetworkComponent)(object)GameNetwork.GetNetworkComponent<NetworkStatusReplicationComponent>());
		GameNetwork.DestroyComponent((UdpNetworkComponent)(object)GameNetwork.GetNetworkComponent<MultiplayerPermissionHandler>());
		GameNetwork.DestroyComponent((UdpNetworkComponent)(object)GameNetwork.GetNetworkComponent<BaseNetworkComponent>());
		GameNetwork.DestroyComponent((UdpNetworkComponent)(object)GameNetwork.GetNetworkComponent<BaseNetworkComponentData>());
	}

	void IGameNetworkHandler.OnStartReplay()
	{
		GameNetwork.AddNetworkComponent<BaseNetworkComponentData>();
		GameNetwork.AddNetworkComponent<BaseNetworkComponent>();
		GameNetwork.AddNetworkComponent<LobbyNetworkComponent>();
	}

	void IGameNetworkHandler.OnEndReplay()
	{
		GameNetwork.DestroyComponent((UdpNetworkComponent)(object)GameNetwork.GetNetworkComponent<LobbyNetworkComponent>());
		GameNetwork.DestroyComponent((UdpNetworkComponent)(object)GameNetwork.GetNetworkComponent<BaseNetworkComponent>());
		GameNetwork.DestroyComponent((UdpNetworkComponent)(object)GameNetwork.GetNetworkComponent<BaseNetworkComponentData>());
	}

	void IGameNetworkHandler.OnHandleConsoleCommand(string command)
	{
		DedicatedServerConsoleCommandManager.HandleConsoleCommand(command);
	}
}
