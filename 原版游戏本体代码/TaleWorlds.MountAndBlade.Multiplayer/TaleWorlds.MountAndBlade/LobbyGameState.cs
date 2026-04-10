using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public abstract class LobbyGameState : GameState, IUdpNetworkHandler
{
	public override bool IsMusicMenuState => true;

	protected override void OnInitialize()
	{
		((GameState)this).OnInitialize();
		StartMultiplayer();
		GameNetwork.AddNetworkHandler((IUdpNetworkHandler)(object)this);
	}

	protected override void OnActivate()
	{
		((GameState)this).OnActivate();
	}

	protected override void OnFinalize()
	{
		((GameState)this).OnFinalize();
		GameNetwork.RemoveNetworkHandler((IUdpNetworkHandler)(object)this);
		GameNetwork.EndMultiplayer();
	}

	void IUdpNetworkHandler.OnUdpNetworkHandlerClose()
	{
	}

	void IUdpNetworkHandler.OnUdpNetworkHandlerTick(float dt)
	{
	}

	void IUdpNetworkHandler.HandleNewClientConnect(PlayerConnectionInfo clientConnectionInfo)
	{
	}

	void IUdpNetworkHandler.HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
	{
	}

	void IUdpNetworkHandler.HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
	{
	}

	void IUdpNetworkHandler.HandleLateNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
	{
	}

	void IUdpNetworkHandler.HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
	{
	}

	void IUdpNetworkHandler.HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
	{
	}

	void IUdpNetworkHandler.HandleEarlyPlayerDisconnect(NetworkCommunicator networkPeer)
	{
	}

	void IUdpNetworkHandler.HandlePlayerDisconnect(NetworkCommunicator networkPeer)
	{
	}

	void IUdpNetworkHandler.OnEveryoneUnSynchronized()
	{
	}

	void IUdpNetworkHandler.OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
	{
	}

	void IUdpNetworkHandler.OnDisconnectedFromServer()
	{
		OnDisconnectedFromServer();
	}

	protected virtual void OnDisconnectedFromServer()
	{
	}

	protected abstract void StartMultiplayer();
}
