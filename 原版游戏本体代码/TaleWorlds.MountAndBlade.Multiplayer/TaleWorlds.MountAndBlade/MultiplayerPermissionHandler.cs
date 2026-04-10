using System;
using System.Collections.Concurrent;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade;

public class MultiplayerPermissionHandler : UdpNetworkComponent
{
	private ChatBox _chatBox;

	private ConcurrentDictionary<(PlayerId PlayerId, Permission Permission), bool> _registeredEvents = new ConcurrentDictionary<(PlayerId, Permission), bool>();

	public event Action<PlayerId, bool> OnPlayerPlatformMuteChanged;

	public MultiplayerPermissionHandler()
	{
		_chatBox = Game.Current.GetGameHandler<ChatBox>();
	}

	public override void OnUdpNetworkHandlerClose()
	{
		((UdpNetworkComponent)this).OnUdpNetworkHandlerClose();
		HandleClientDisconnect();
	}

	private void HandleClientDisconnect()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		foreach (var key in _registeredEvents.Keys)
		{
			PlatformServices.Instance.UnregisterPermissionChangeEvent(key.PlayerId, key.Permission, new PermissionChanged(VoicePermissionChanged));
			_registeredEvents.TryRemove((key.PlayerId, key.Permission), out var _);
		}
	}

	protected override void AddRemoveMessageHandlers(NetworkMessageHandlerRegistererContainer registerer)
	{
		if (GameNetwork.IsClient)
		{
			registerer.RegisterBaseHandler<InitializeLobbyPeer>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventInitializeLobbyPeer);
		}
	}

	private void HandleServerEventInitializeLobbyPeer(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		InitializeLobbyPeer val = (InitializeLobbyPeer)baseMessage;
		if (GameNetwork.MyPeer != null && val.Peer != GameNetwork.MyPeer)
		{
			if (PlatformServices.Instance.RegisterPermissionChangeEvent(val.ProvidedId, (Permission)1, new PermissionChanged(TextPermissionChanged)))
			{
				_registeredEvents[(val.ProvidedId, (Permission)1)] = true;
			}
			if (PlatformServices.Instance.RegisterPermissionChangeEvent(val.ProvidedId, (Permission)2, new PermissionChanged(VoicePermissionChanged)))
			{
				_registeredEvents[(val.ProvidedId, (Permission)2)] = true;
			}
		}
	}

	public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		((UdpNetworkComponent)this).OnPlayerDisconnectedFromServer(networkPeer);
		bool value;
		if (PlatformServices.Instance.UnregisterPermissionChangeEvent(networkPeer.VirtualPlayer.Id, (Permission)1, new PermissionChanged(TextPermissionChanged)))
		{
			_registeredEvents.TryRemove((networkPeer.VirtualPlayer.Id, (Permission)1), out value);
		}
		if (PlatformServices.Instance.UnregisterPermissionChangeEvent(networkPeer.VirtualPlayer.Id, (Permission)2, new PermissionChanged(VoicePermissionChanged)))
		{
			_registeredEvents.TryRemove((networkPeer.VirtualPlayer.Id, (Permission)2), out value);
		}
	}

	private void TextPermissionChanged(PlayerId targetPlayerId, Permission permission, bool hasPermission)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
		{
			if (!(targetPlayerId != networkPeer.VirtualPlayer.Id))
			{
				PeerExtensions.GetComponent<MissionPeer>(networkPeer);
				bool flag = !hasPermission;
				_chatBox.SetPlayerMutedFromPlatform(targetPlayerId, flag);
				this.OnPlayerPlatformMuteChanged?.Invoke(targetPlayerId, flag);
			}
		}
	}

	private void VoicePermissionChanged(PlayerId targetPlayerId, Permission permission, bool hasPermission)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
		{
			if (!(targetPlayerId != networkPeer.VirtualPlayer.Id))
			{
				MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(networkPeer);
				bool flag = !hasPermission;
				component.SetMutedFromPlatform(flag);
				this.OnPlayerPlatformMuteChanged?.Invoke(targetPlayerId, flag);
			}
		}
	}
}
