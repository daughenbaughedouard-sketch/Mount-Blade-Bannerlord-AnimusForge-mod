using System;
using System.Threading.Tasks;
using NetworkMessages.FromClient;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.NetworkComponents;

public class BaseNetworkComponent : UdpNetworkComponent
{
	public delegate void WelcomeMessageReceivedDelegate(string messageText);

	public Action OnIntermissionStateUpdated;

	private BaseNetworkComponentData _baseNetworkComponentData;

	public MultiplayerIntermissionState ClientIntermissionState { get; private set; }

	public float CurrentIntermissionTimer { get; private set; }

	public bool DisplayingWelcomeMessage { get; private set; }

	public event WelcomeMessageReceivedDelegate WelcomeMessageReceived = delegate(string messageText)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		InformationManager.DisplayMessage(new InformationMessage(messageText));
	};

	private void EnsureBaseNetworkComponentData()
	{
		if (_baseNetworkComponentData == null)
		{
			_baseNetworkComponentData = GameNetwork.GetNetworkComponent<BaseNetworkComponentData>();
		}
	}

	protected override void AddRemoveMessageHandlers(NetworkMessageHandlerRegistererContainer registerer)
	{
		((UdpNetworkComponent)this).AddRemoveMessageHandlers(registerer);
		if (GameNetwork.IsClientOrReplay)
		{
			registerer.RegisterBaseHandler<AddPeerComponent>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventAddPeerComponent);
			registerer.RegisterBaseHandler<RemovePeerComponent>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventRemovePeerComponent);
			registerer.RegisterBaseHandler<SynchronizingDone>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventSynchronizingDone);
			registerer.RegisterBaseHandler<LoadMission>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventLoadMission);
			registerer.RegisterBaseHandler<UnloadMission>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventUnloadMission);
			registerer.RegisterBaseHandler<InitializeCustomGameMessage>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventInitializeCustomGame);
			registerer.RegisterBaseHandler<MultiplayerOptionsInitial>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventMultiplayerOptionsInitial);
			registerer.RegisterBaseHandler<MultiplayerOptionsImmediate>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventMultiplayerOptionsImmediate);
			registerer.RegisterBaseHandler<MultiplayerIntermissionUpdate>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventMultiplayerIntermissionUpdate);
			registerer.RegisterBaseHandler<MultiplayerIntermissionMapItemAdded>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventIntermissionMapItemAdded);
			registerer.RegisterBaseHandler<MultiplayerIntermissionCultureItemAdded>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventIntermissionCultureItemAdded);
			registerer.RegisterBaseHandler<MultiplayerIntermissionMapItemVoteCountChanged>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventIntermissionMapItemVoteCountChanged);
			registerer.RegisterBaseHandler<MultiplayerIntermissionCultureItemVoteCountChanged>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventIntermissionCultureItemVoteCountChanged);
			registerer.RegisterBaseHandler<MultiplayerIntermissionUsableMapAdded>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventUsableMapAdded);
			registerer.RegisterBaseHandler<UpdateIntermissionVotingManagerValues>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventUpdateIntermissionVotingManagerValues);
			registerer.RegisterBaseHandler<SyncMutedPlayers>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventSyncMutedPlayers);
			registerer.RegisterBaseHandler<SyncPlayerMuteState>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventSyncPlayerMuteState);
		}
		else if (GameNetwork.IsServer)
		{
			registerer.RegisterBaseHandler<FinishedLoading>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleClientEventFinishedLoading);
			registerer.RegisterBaseHandler<SyncRelevantGameOptionsToServer>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleSyncRelevantGameOptionsToServer);
			registerer.RegisterBaseHandler<IntermissionVote>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleIntermissionClientVote);
		}
	}

	public override void OnUdpNetworkHandlerTick(float dt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		((UdpNetworkComponent)this).OnUdpNetworkHandlerTick(dt);
		if (GameNetwork.IsClientOrReplay && ((int)ClientIntermissionState == 3 || (int)ClientIntermissionState == 4 || (int)ClientIntermissionState == 1 || (int)ClientIntermissionState == 2))
		{
			CurrentIntermissionTimer -= dt;
			if (CurrentIntermissionTimer <= 0f)
			{
				CurrentIntermissionTimer = 0f;
			}
		}
	}

	public override void HandleNewClientConnect(PlayerConnectionInfo playerConnectionInfo)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Invalid comparison between Unknown and I4
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Invalid comparison between Unknown and I4
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Expected O, but got Unknown
		EnsureBaseNetworkComponentData();
		NetworkCommunicator networkPeer = playerConnectionInfo.NetworkPeer;
		if (networkPeer.IsServerPeer)
		{
			return;
		}
		GameNetwork.BeginModuleEventAsServer(networkPeer);
		GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerOptionsInitial());
		GameNetwork.EndModuleEventAsServer();
		GameNetwork.BeginModuleEventAsServer(networkPeer);
		GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerOptionsImmediate());
		GameNetwork.EndModuleEventAsServer();
		foreach (IntermissionVoteItem mapVoteItem in MultiplayerIntermissionVotingManager.Instance.MapVoteItems)
		{
			GameNetwork.BeginModuleEventAsServer(networkPeer);
			GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerIntermissionMapItemAdded(mapVoteItem.Id));
			GameNetwork.EndModuleEventAsServer();
			GameNetwork.BeginModuleEventAsServer(networkPeer);
			GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerIntermissionMapItemVoteCountChanged(mapVoteItem.Index, mapVoteItem.VoteCount));
			GameNetwork.EndModuleEventAsServer();
		}
		foreach (IntermissionVoteItem cultureVoteItem in MultiplayerIntermissionVotingManager.Instance.CultureVoteItems)
		{
			GameNetwork.BeginModuleEventAsServer(networkPeer);
			GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerIntermissionCultureItemAdded(cultureVoteItem.Id));
			GameNetwork.EndModuleEventAsServer();
			GameNetwork.BeginModuleEventAsServer(networkPeer);
			GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerIntermissionCultureItemVoteCountChanged(cultureVoteItem.Index, cultureVoteItem.VoteCount));
			GameNetwork.EndModuleEventAsServer();
		}
		if (networkPeer.IsAdmin)
		{
			GameNetwork.BeginModuleEventAsServer(networkPeer);
			GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerOptionsDefault());
			GameNetwork.EndModuleEventAsServer();
			foreach (CustomGameUsableMap usableMap in MultiplayerIntermissionVotingManager.Instance.UsableMaps)
			{
				GameNetwork.BeginModuleEventAsServer(networkPeer);
				GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerIntermissionUsableMapAdded(usableMap.Map, usableMap.IsCompatibleWithAllGameTypes, (!usableMap.IsCompatibleWithAllGameTypes) ? usableMap.CompatibleGameTypes.Count : 0, usableMap.CompatibleGameTypes));
				GameNetwork.EndModuleEventAsServer();
			}
			GameNetwork.BeginModuleEventAsServer(networkPeer);
			GameNetwork.WriteMessage((GameNetworkMessage)new UpdateIntermissionVotingManagerValues());
			GameNetwork.EndModuleEventAsServer();
		}
		GameNetwork.BeginModuleEventAsServer(networkPeer);
		GameNetwork.WriteMessage((GameNetworkMessage)new SyncMutedPlayers(MultiplayerGlobalMutedPlayersManager.MutedPlayers));
		GameNetwork.EndModuleEventAsServer();
		if ((int)BannerlordNetwork.LobbyMissionType == 1 || (int)BannerlordNetwork.LobbyMissionType == 2)
		{
			bool flag = false;
			string text = "";
			string text2 = "";
			if ((GameNetwork.IsDedicatedServer && Mission.Current != null) || !GameNetwork.IsDedicatedServer)
			{
				flag = true;
				MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)13, (MultiplayerOptionsAccessMode)1).GetValue(ref text);
				MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)11, (MultiplayerOptionsAccessMode)1).GetValue(ref text2);
			}
			GameNetwork.BeginModuleEventAsServer(networkPeer);
			GameNetwork.WriteMessage((GameNetworkMessage)new InitializeCustomGameMessage(flag, text2, text, _baseNetworkComponentData.CurrentBattleIndex));
			GameNetwork.EndModuleEventAsServer();
		}
	}

	public override void HandlePlayerDisconnect(NetworkCommunicator networkPeer)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerIntermissionVotingManager.Instance.HandlePlayerDisconnect(networkPeer.VirtualPlayer.Id);
	}

	public void IntermissionCastVote(string itemID, int voteCount)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		GameNetwork.BeginModuleEventAsClient();
		GameNetwork.WriteMessage((GameNetworkMessage)new IntermissionVote(itemID, voteCount));
		GameNetwork.EndModuleEventAsClient();
	}

	public override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
	{
		Mission current = Mission.Current;
		MissionNetworkComponent val = ((current != null) ? current.GetMissionBehavior<MissionNetworkComponent>() : null);
		if (val != null)
		{
			val.OnClientSynchronized(networkPeer);
		}
	}

	public override void OnUdpNetworkHandlerClose()
	{
		((UdpNetworkComponent)this).OnUdpNetworkHandlerClose();
		MultiplayerGlobalMutedPlayersManager.ClearMutedPlayers();
	}

	public void SetDisplayingWelcomeMessage(bool displaying)
	{
		DisplayingWelcomeMessage = displaying;
	}

	private void HandleServerEventMultiplayerOptionsInitial(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected I4, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerOptionsInitial val = (MultiplayerOptionsInitial)baseMessage;
		bool flag = default(bool);
		int num = default(int);
		string text = default(string);
		for (OptionType val2 = (OptionType)0; (int)val2 < 43; val2 = (OptionType)(val2 + 1))
		{
			MultiplayerOptionsProperty optionProperty = MultiplayerOptionsExtensions.GetOptionProperty(val2);
			if ((int)optionProperty.Replication == 1)
			{
				OptionValueType optionValueType = optionProperty.OptionValueType;
				switch ((int)optionValueType)
				{
				case 0:
					val.GetOption(val2).GetValue(ref flag);
					MultiplayerOptionsExtensions.SetValue(val2, flag, (MultiplayerOptionsAccessMode)1);
					break;
				case 1:
				case 2:
					val.GetOption(val2).GetValue(ref num);
					MultiplayerOptionsExtensions.SetValue(val2, num, (MultiplayerOptionsAccessMode)1);
					break;
				case 3:
					val.GetOption(val2).GetValue(ref text);
					MultiplayerOptionsExtensions.SetValue(val2, text, (MultiplayerOptionsAccessMode)1);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		string strValue = MultiplayerOptionsExtensions.GetStrValue((OptionType)1, (MultiplayerOptionsAccessMode)1);
		if (!string.IsNullOrEmpty(strValue))
		{
			this.WelcomeMessageReceived?.Invoke(strValue);
		}
	}

	private void HandleServerEventMultiplayerOptionsImmediate(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected I4, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerOptionsImmediate val = (MultiplayerOptionsImmediate)baseMessage;
		bool flag = default(bool);
		int num = default(int);
		string text = default(string);
		for (OptionType val2 = (OptionType)0; (int)val2 < 43; val2 = (OptionType)(val2 + 1))
		{
			MultiplayerOptionsProperty optionProperty = MultiplayerOptionsExtensions.GetOptionProperty(val2);
			if ((int)optionProperty.Replication == 2)
			{
				OptionValueType optionValueType = optionProperty.OptionValueType;
				switch ((int)optionValueType)
				{
				case 0:
					val.GetOption(val2).GetValue(ref flag);
					MultiplayerOptionsExtensions.SetValue(val2, flag, (MultiplayerOptionsAccessMode)1);
					break;
				case 1:
				case 2:
					val.GetOption(val2).GetValue(ref num);
					MultiplayerOptionsExtensions.SetValue(val2, num, (MultiplayerOptionsAccessMode)1);
					break;
				case 3:
					val.GetOption(val2).GetValue(ref text);
					MultiplayerOptionsExtensions.SetValue(val2, text, (MultiplayerOptionsAccessMode)1);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	private void HandleServerEventMultiplayerIntermissionUpdate(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerIntermissionUpdate val = (MultiplayerIntermissionUpdate)baseMessage;
		CurrentIntermissionTimer = val.IntermissionTimer;
		ClientIntermissionState = val.IntermissionState;
		OnIntermissionStateUpdated?.Invoke();
	}

	private void HandleServerEventIntermissionMapItemAdded(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		MultiplayerIntermissionMapItemAdded val = (MultiplayerIntermissionMapItemAdded)baseMessage;
		MultiplayerIntermissionVotingManager.Instance.AddMapItem(val.MapId);
		OnIntermissionStateUpdated?.Invoke();
	}

	private void HandleServerEventUsableMapAdded(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		MultiplayerIntermissionUsableMapAdded val = (MultiplayerIntermissionUsableMapAdded)baseMessage;
		MultiplayerIntermissionVotingManager.Instance.AddUsableMap(new CustomGameUsableMap(val.MapId, val.IsCompatibleWithAllGameTypes, val.CompatibleGameTypes));
	}

	private void HandleServerEventSyncPlayerMuteState(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		SyncPlayerMuteState val = (SyncPlayerMuteState)baseMessage;
		if (val.IsMuted)
		{
			MultiplayerGlobalMutedPlayersManager.MutePlayer(val.PlayerId);
		}
		else
		{
			MultiplayerGlobalMutedPlayersManager.UnmutePlayer(val.PlayerId);
		}
	}

	private void HandleServerEventSyncMutedPlayers(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		SyncMutedPlayers val = (SyncMutedPlayers)baseMessage;
		MultiplayerGlobalMutedPlayersManager.ClearMutedPlayers();
		if (val.MutedPlayerCount <= 0)
		{
			return;
		}
		foreach (PlayerId mutedPlayerId in val.MutedPlayerIds)
		{
			MultiplayerGlobalMutedPlayersManager.MutePlayer(mutedPlayerId);
		}
	}

	private void HandleServerEventUpdateIntermissionVotingManagerValues(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		UpdateIntermissionVotingManagerValues val = (UpdateIntermissionVotingManagerValues)baseMessage;
		MultiplayerIntermissionVotingManager.Instance.IsAutomatedBattleSwitchingEnabled = val.IsAutomatedBattleSwitchingEnabled;
		MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = val.IsMapVoteEnabled;
		MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = val.IsCultureVoteEnabled;
	}

	private void HandleServerEventIntermissionCultureItemAdded(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		MultiplayerIntermissionCultureItemAdded val = (MultiplayerIntermissionCultureItemAdded)baseMessage;
		MultiplayerIntermissionVotingManager.Instance.AddCultureItem(val.CultureId);
		OnIntermissionStateUpdated?.Invoke();
	}

	private void HandleServerEventIntermissionMapItemVoteCountChanged(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		MultiplayerIntermissionMapItemVoteCountChanged val = (MultiplayerIntermissionMapItemVoteCountChanged)baseMessage;
		MultiplayerIntermissionVotingManager.Instance.SetVotesOfMap(val.MapItemIndex, val.VoteCount);
		OnIntermissionStateUpdated?.Invoke();
	}

	private void HandleServerEventIntermissionCultureItemVoteCountChanged(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		MultiplayerIntermissionCultureItemVoteCountChanged val = (MultiplayerIntermissionCultureItemVoteCountChanged)baseMessage;
		MultiplayerIntermissionVotingManager.Instance.SetVotesOfCulture(val.CultureItemIndex, val.VoteCount);
		OnIntermissionStateUpdated?.Invoke();
	}

	private void HandleServerEventAddPeerComponent(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		AddPeerComponent val = (AddPeerComponent)baseMessage;
		NetworkCommunicator peer = val.Peer;
		uint componentId = val.ComponentId;
		if (PeerExtensions.GetComponent(peer, componentId) == null)
		{
			PeerExtensions.AddComponent(peer, componentId);
		}
	}

	private void HandleServerEventRemovePeerComponent(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		RemovePeerComponent val = (RemovePeerComponent)baseMessage;
		NetworkCommunicator peer = val.Peer;
		uint componentId = val.ComponentId;
		PeerComponent component = PeerExtensions.GetComponent(peer, componentId);
		PeerExtensions.RemoveComponent(peer, component);
	}

	private void HandleServerEventSynchronizingDone(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Invalid comparison between Unknown and I4
		SynchronizingDone val = (SynchronizingDone)baseMessage;
		NetworkCommunicator peer = val.Peer;
		Mission current = Mission.Current;
		MissionNetworkComponent val2 = ((current != null) ? current.GetMissionBehavior<MissionNetworkComponent>() : null);
		if (val2 != null && !peer.IsMine)
		{
			val2.OnClientSynchronized(peer);
			return;
		}
		peer.IsSynchronized = val.Synchronized;
		if (val2 == null || !val.Synchronized)
		{
			return;
		}
		if (PeerExtensions.GetComponent<MissionPeer>(peer) == null)
		{
			LobbyClient gameClient = NetworkMain.GameClient;
			CommunityClient communityClient = NetworkMain.CommunityClient;
			if (communityClient.IsInGame)
			{
				communityClient.QuitFromGame();
			}
			else if ((int)gameClient.CurrentState == 16)
			{
				gameClient.QuitFromCustomGame();
			}
			else if ((int)gameClient.CurrentState == 14)
			{
				gameClient.EndCustomGame();
			}
			else
			{
				gameClient.QuitFromMatchmakerGame();
			}
		}
		else
		{
			val2.OnClientSynchronized(peer);
		}
	}

	private async void HandleServerEventLoadMission(GameNetworkMessage baseMessage)
	{
		LoadMission message = (LoadMission)baseMessage;
		EnsureBaseNetworkComponentData();
		while (GameStateManager.Current.ActiveState is MissionState)
		{
			await Task.Delay(1);
		}
		if (GameNetwork.MyPeer != null)
		{
			GameNetwork.MyPeer.IsSynchronized = false;
		}
		CurrentIntermissionTimer = 0f;
		ClientIntermissionState = (MultiplayerIntermissionState)0;
		_baseNetworkComponentData.UpdateCurrentBattleIndex(message.BattleIndex);
		if (!Module.CurrentModule.StartMultiplayerGame(message.GameType, message.Map))
		{
			Debug.FailedAssert("[DEBUG]Invalid multiplayer game type.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\NetworkComponents\\BaseNetworkComponent.cs", "HandleServerEventLoadMission", 470);
		}
	}

	private void HandleServerEventUnloadMission(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		UnloadMission message = (UnloadMission)baseMessage;
		HandleServerEventUnloadMissionAux(message);
	}

	private void HandleServerEventInitializeCustomGame(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		InitializeCustomGameMessage message = (InitializeCustomGameMessage)baseMessage;
		InitializeCustomGameAux(message);
	}

	private async void InitializeCustomGameAux(InitializeCustomGameMessage message)
	{
		EnsureBaseNetworkComponentData();
		await Task.Delay(200);
		while (!(GameStateManager.Current.ActiveState is LobbyGameStateCustomGameClient) && !(GameStateManager.Current.ActiveState is LobbyGameStateCommunityClient))
		{
			await Task.Delay(1);
		}
		if (message.InMission)
		{
			MBDebug.Print("Client: I have received InitializeCustomGameMessage with mission " + message.GameType + " " + message.Map + ". Loading it...", 0, (DebugColor)12, 17179869184uL);
			_baseNetworkComponentData.UpdateCurrentBattleIndex(message.BattleIndex);
			if (!Module.CurrentModule.StartMultiplayerGame(message.GameType, message.Map))
			{
				Debug.FailedAssert("[DEBUG]Invalid multiplayer game type.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\NetworkComponents\\BaseNetworkComponent.cs", "InitializeCustomGameAux", 507);
			}
		}
		else
		{
			LoadingWindow.DisableGlobalLoadingWindow();
			GameNetwork.SyncRelevantGameOptionsToServer();
		}
	}

	private async void HandleServerEventUnloadMissionAux(UnloadMission message)
	{
		GameNetwork.MyPeer.IsSynchronized = false;
		CurrentIntermissionTimer = 0f;
		ClientIntermissionState = (MultiplayerIntermissionState)0;
		if (Mission.Current != null)
		{
			MissionCustomGameClientComponent missionBehavior = Mission.Current.GetMissionBehavior<MissionCustomGameClientComponent>();
			if (missionBehavior != null)
			{
				missionBehavior.SetServerEndingBeforeClientLoaded(message.UnloadingForBattleIndexMismatch);
			}
			MissionCommunityClientComponent missionBehavior2 = Mission.Current.GetMissionBehavior<MissionCommunityClientComponent>();
			if (missionBehavior2 != null)
			{
				missionBehavior2.SetServerEndingBeforeClientLoaded(message.UnloadingForBattleIndexMismatch);
			}
		}
		BannerlordNetwork.EndMultiplayerLobbyMission();
		Game.Current.GetGameHandler<ChatBox>().ResetMuteList();
		while (Mission.Current != null)
		{
			await Task.Delay(1);
		}
		LoadingWindow.DisableGlobalLoadingWindow();
	}

	private bool HandleClientEventFinishedLoading(NetworkCommunicator networkPeer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		FinishedLoading message = (FinishedLoading)baseMessage;
		HandleClientEventFinishedLoadingAux(networkPeer, message);
		return true;
	}

	private async void HandleClientEventFinishedLoadingAux(NetworkCommunicator networkPeer, FinishedLoading message)
	{
		EnsureBaseNetworkComponentData();
		while (Mission.Current != null && (int)Mission.Current.CurrentState != 2)
		{
			await Task.Delay(1);
		}
		if (!networkPeer.IsServerPeer)
		{
			MBDebug.Print("Server: " + networkPeer.UserName + " has finished loading. From now on, I will include him in the broadcasted messages", 0, (DebugColor)12, 17179869184uL);
			if (Mission.Current == null || _baseNetworkComponentData.CurrentBattleIndex != message.BattleIndex)
			{
				GameNetwork.BeginModuleEventAsServer(networkPeer);
				GameNetwork.WriteMessage((GameNetworkMessage)new UnloadMission(true));
				GameNetwork.EndModuleEventAsServer();
			}
			else
			{
				GameNetwork.ClientFinishedLoading(networkPeer);
			}
		}
	}

	private bool HandleSyncRelevantGameOptionsToServer(NetworkCommunicator networkPeer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SyncRelevantGameOptionsToServer val = (SyncRelevantGameOptionsToServer)baseMessage;
		networkPeer.SetRelevantGameOptions(val.SendMeBloodEvents, val.SendMeSoundEvents);
		return true;
	}

	private bool HandleIntermissionClientVote(NetworkCommunicator networkPeer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		IntermissionVote val = (IntermissionVote)baseMessage;
		int voteCount = val.VoteCount;
		if (voteCount == -1 || voteCount == 1)
		{
			if (((int)MultiplayerIntermissionVotingManager.Instance.CurrentVoteState == 1 && MultiplayerIntermissionVotingManager.Instance.IsMapItem(val.ItemID)) || ((int)MultiplayerIntermissionVotingManager.Instance.CurrentVoteState == 2 && MultiplayerIntermissionVotingManager.Instance.IsCultureItem(val.ItemID)))
			{
				MultiplayerIntermissionVotingManager.Instance.AddVote(networkPeer.VirtualPlayer.Id, val.ItemID, val.VoteCount);
			}
			return true;
		}
		return false;
	}
}
