using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Ranked;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade;

public class LobbyGameClientHandler : ILobbyClientSessionHandler
{
	public IChatHandler ChatHandler;

	public LobbyState LobbyState { get; set; }

	public LobbyClient GameClient => NetworkMain.GameClient;

	void ILobbyClientSessionHandler.OnConnected()
	{
	}

	void ILobbyClientSessionHandler.OnCantConnect()
	{
	}

	void ILobbyClientSessionHandler.OnDisconnected(TextObject feedback)
	{
		if (LobbyState != null)
		{
			LobbyState.OnDisconnected(feedback);
		}
	}

	void ILobbyClientSessionHandler.OnPlayerDataReceived(PlayerData playerData)
	{
		if (LobbyState != null)
		{
			LobbyState.OnPlayerDataReceived(playerData);
		}
	}

	void ILobbyClientSessionHandler.OnPendingRejoin()
	{
		LobbyState?.OnPendingRejoin();
	}

	void ILobbyClientSessionHandler.OnBattleResultReceived()
	{
	}

	void ILobbyClientSessionHandler.OnCancelJoiningBattle()
	{
		if (LobbyState != null)
		{
			LobbyState.OnCancelFindingGame();
		}
	}

	void ILobbyClientSessionHandler.OnRejoinRequestRejected()
	{
	}

	void ILobbyClientSessionHandler.OnFindGameAnswer(bool successful, string[] selectedAndEnabledGameTypes, bool isRejoin)
	{
		if (successful && LobbyState != null)
		{
			LobbyState.OnUpdateFindingGame(MatchmakingWaitTimeStats.Empty, selectedAndEnabledGameTypes);
		}
	}

	void ILobbyClientSessionHandler.OnEnterBattleWithPartyAnswer(string[] selectedGameTypes)
	{
		if (LobbyState != null)
		{
			LobbyState.OnUpdateFindingGame(MatchmakingWaitTimeStats.Empty, selectedGameTypes);
		}
	}

	void ILobbyClientSessionHandler.OnWhisperMessageReceived(string fromPlayer, string toPlayer, string message)
	{
		if (ChatHandler != null)
		{
			ChatHandler.ReceiveChatMessage((ChatChannelType)0, fromPlayer, message);
		}
		ChatBox.AddWhisperMessage(fromPlayer, message);
	}

	void ILobbyClientSessionHandler.OnClanMessageReceived(string playerName, string message)
	{
	}

	void ILobbyClientSessionHandler.OnPartyMessageReceived(string playerName, string message)
	{
	}

	void ILobbyClientSessionHandler.OnSystemMessageReceived(string message)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		InformationManager.DisplayMessage(new InformationMessage(message));
	}

	void ILobbyClientSessionHandler.OnAdminMessageReceived(string message)
	{
		if (LobbyState != null)
		{
			LobbyState.OnAdminMessageReceived(message);
		}
	}

	void ILobbyClientSessionHandler.OnPartyInvitationReceived(string inviterPlayerName, PlayerId inviterPlayerId)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyState != null)
		{
			LobbyState.OnPartyInvitationReceived(inviterPlayerName, inviterPlayerId);
		}
	}

	void ILobbyClientSessionHandler.OnPartyJoinRequestReceived(PlayerId joiningPlayerId, PlayerId viaPlayerId, string viaFriendName)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyState != null)
		{
			LobbyState.OnPartyJoinRequestReceived(joiningPlayerId, viaPlayerId, viaFriendName);
		}
	}

	void ILobbyClientSessionHandler.OnPartyInvitationInvalidated()
	{
		if (LobbyState != null)
		{
			LobbyState.OnPartyInvitationInvalidated();
		}
	}

	void ILobbyClientSessionHandler.OnPlayerInvitedToParty(PlayerId playerId)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyState != null)
		{
			LobbyState.OnPlayerInvitedToParty(playerId);
		}
	}

	void ILobbyClientSessionHandler.OnPlayersAddedToParty(List<(PlayerId PlayerId, string PlayerName, bool IsPartyLeader)> addedPlayers, List<(PlayerId PlayerId, string PlayerName)> invitedPlayers)
	{
		if (LobbyState != null)
		{
			LobbyState.OnPlayersAddedToParty(addedPlayers, invitedPlayers);
		}
	}

	void ILobbyClientSessionHandler.OnPlayerRemovedFromParty(PlayerId playerId, PartyRemoveReason reason)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyState != null)
		{
			LobbyState.OnPlayerRemovedFromParty(playerId, reason);
		}
	}

	void ILobbyClientSessionHandler.OnPlayerAssignedPartyLeader(PlayerId partyLeaderId)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyState != null)
		{
			LobbyState.OnPlayerAssignedPartyLeader(partyLeaderId);
		}
	}

	void ILobbyClientSessionHandler.OnPlayerSuggestedToParty(PlayerId playerId, string playerName, PlayerId suggestingPlayerId, string suggestingPlayerName)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyState != null)
		{
			LobbyState.OnPlayerSuggestedToParty(playerId, playerName, suggestingPlayerId, suggestingPlayerName);
		}
	}

	void ILobbyClientSessionHandler.OnServerStatusReceived(ServerStatus serverStatus)
	{
		if (LobbyState != null)
		{
			LobbyState.OnServerStatusReceived(serverStatus);
		}
	}

	void ILobbyClientSessionHandler.OnFriendListReceived(FriendInfo[] friends)
	{
		if (LobbyState != null)
		{
			LobbyState.OnFriendListReceived(friends);
		}
	}

	void ILobbyClientSessionHandler.OnRecentPlayerStatusesReceived(FriendInfo[] friends)
	{
		if (LobbyState != null)
		{
			LobbyState.OnRecentPlayerStatusesReceived(friends);
		}
	}

	void ILobbyClientSessionHandler.OnClanInvitationReceived(string clanName, string clanTag, bool isCreation)
	{
		if (LobbyState != null)
		{
			LobbyState.OnClanInvitationReceived(clanName, clanTag, isCreation);
		}
	}

	void ILobbyClientSessionHandler.OnClanInvitationAnswered(PlayerId playerId, ClanCreationAnswer answer)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyState != null)
		{
			LobbyState.OnClanInvitationAnswered(playerId, answer);
		}
	}

	void ILobbyClientSessionHandler.OnClanCreationSuccessful()
	{
		if (LobbyState != null)
		{
			LobbyState.OnClanCreationSuccessful();
		}
	}

	void ILobbyClientSessionHandler.OnClanCreationFailed()
	{
		if (LobbyState != null)
		{
			LobbyState.OnClanCreationFailed();
		}
	}

	void ILobbyClientSessionHandler.OnClanCreationStarted()
	{
		if (LobbyState != null)
		{
			LobbyState.OnClanCreationStarted();
		}
	}

	void ILobbyClientSessionHandler.OnClanInfoChanged()
	{
		if (LobbyState != null)
		{
			LobbyState.OnClanInfoChanged();
		}
	}

	void ILobbyClientSessionHandler.OnPremadeGameEligibilityStatusReceived(bool isEligible)
	{
		if (LobbyState != null)
		{
			LobbyState.OnPremadeGameEligibilityStatusReceived(isEligible);
		}
	}

	void ILobbyClientSessionHandler.OnPremadeGameCreated()
	{
		if (LobbyState != null)
		{
			LobbyState.OnPremadeGameCreated();
		}
	}

	void ILobbyClientSessionHandler.OnPremadeGameListReceived()
	{
		if (LobbyState != null)
		{
			LobbyState.OnPremadeGameListReceived();
		}
	}

	void ILobbyClientSessionHandler.OnPremadeGameCreationCancelled()
	{
		if (LobbyState != null)
		{
			LobbyState.OnPremadeGameCreationCancelled();
		}
	}

	void ILobbyClientSessionHandler.OnJoinPremadeGameRequested(string clanName, string clanSigilCode, Guid partyId, PlayerId[] challengerPlayerIDs, PlayerId challengerPartyLeaderID, PremadeGameType premadeGameType)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyState != null)
		{
			LobbyState.OnJoinPremadeGameRequested(clanName, clanSigilCode, partyId, challengerPlayerIDs, challengerPartyLeaderID, premadeGameType);
		}
	}

	void ILobbyClientSessionHandler.OnJoinPremadeGameRequestSuccessful()
	{
		if (LobbyState != null)
		{
			LobbyState.OnJoinPremadeGameRequestSuccessful();
		}
	}

	void ILobbyClientSessionHandler.OnSigilChanged()
	{
		if (LobbyState != null)
		{
			LobbyState.OnSigilChanged();
		}
	}

	void ILobbyClientSessionHandler.OnNotificationsReceived(LobbyNotification[] notifications)
	{
		if (LobbyState != null)
		{
			LobbyState.OnNotificationsReceived(notifications);
		}
	}

	void ILobbyClientSessionHandler.OnGameClientStateChange(State oldState)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		HandleGameClientStateChange(oldState);
	}

	private async void HandleGameClientStateChange(State oldState)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		LobbyClient gameClient = NetworkMain.GameClient;
		Debug.Print(string.Concat("[][] New MBGameClient State: ", gameClient.CurrentState, " old state:", oldState), 0, (DebugColor)12, 17592186044416uL);
		State currentState = gameClient.CurrentState;
		switch ((int)currentState)
		{
		case 0:
			if ((int)oldState == 9 || (int)oldState == 14 || (int)oldState == 16)
			{
				if (Mission.Current != null && !(Game.Current.GameStateManager.ActiveState is MissionState))
				{
					Game.Current.GameStateManager.PopState(0);
				}
				if (Game.Current.GameStateManager.ActiveState is LobbyGameStateCustomGameClient)
				{
					Game.Current.GameStateManager.PopState(0);
				}
				if (Game.Current.GameStateManager.ActiveState is MissionState)
				{
					MissionState missionSystem = (MissionState)Game.Current.GameStateManager.ActiveState;
					while ((int)missionSystem.CurrentMission.CurrentState == 0 || (int)missionSystem.CurrentMission.CurrentState == 1)
					{
						await Task.Delay(1);
					}
					for (int i = 0; i < 3; i++)
					{
						await Task.Delay(1);
					}
					BannerlordNetwork.EndMultiplayerLobbyMission();
				}
				while (Mission.Current != null)
				{
					await Task.Delay(1);
				}
				LobbyState.SetConnectionState(isAuthenticated: false);
			}
			else if ((int)oldState == 4 || (int)oldState == 8)
			{
				LobbyState.SetConnectionState(isAuthenticated: false);
			}
			else if ((int)oldState == 15)
			{
				LobbyState.SetConnectionState(isAuthenticated: false);
			}
			else if ((int)oldState == 1)
			{
				LobbyState.SetConnectionState(isAuthenticated: false);
			}
			else if ((int)oldState == 3)
			{
				LobbyState.SetConnectionState(isAuthenticated: false);
			}
			else if ((int)oldState == 2)
			{
				LobbyState.SetConnectionState(isAuthenticated: false);
			}
			else
			{
				Debug.FailedAssert("Unexpected old state:" + oldState, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\LobbyGameClientHandler.cs", "HandleGameClientStateChange", 414);
			}
			break;
		case 4:
			LobbyState.SetConnectionState(isAuthenticated: true);
			break;
		case 6:
			LobbyState.OnRequestedToSearchBattle();
			break;
		case 7:
			LobbyState.OnRequestedToCancelSearchBattle();
			break;
		}
		LobbyState.OnGameClientStateChange(gameClient.CurrentState);
	}

	void ILobbyClientSessionHandler.OnCustomGameServerListReceived(AvailableCustomGames customGameServerList)
	{
		LobbyState.OnCustomGameServerListReceived(customGameServerList);
	}

	void ILobbyClientSessionHandler.OnMatchmakerGameOver(int oldExperience, int newExperience, List<string> badgesEarned, int lootGained, RankBarInfo oldRankBarInfo, RankBarInfo newRankBarInfo, BattleCancelReason battleCancelReason)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		GameStateManager gameStateManager = Game.Current.GameStateManager;
		if (!(gameStateManager.ActiveState is LobbyState))
		{
			if (gameStateManager.ActiveState is MissionState)
			{
				BannerlordNetwork.EndMultiplayerLobbyMission();
			}
			else
			{
				gameStateManager.PopState(0);
			}
		}
		LobbyState.OnMatchmakerGameOver(oldExperience, newExperience, badgesEarned, lootGained, oldRankBarInfo, newRankBarInfo, battleCancelReason);
	}

	void ILobbyClientSessionHandler.OnQuitFromMatchmakerGame()
	{
		GameStateManager gameStateManager = Game.Current.GameStateManager;
		if (!(gameStateManager.ActiveState is LobbyState))
		{
			if (gameStateManager.ActiveState is MissionState)
			{
				BannerlordNetwork.EndMultiplayerLobbyMission();
			}
			else
			{
				gameStateManager.PopState(0);
			}
		}
	}

	void ILobbyClientSessionHandler.OnBattleServerInformationReceived(BattleServerInformationForClient battleServerInformation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		HandleBattleJoining(battleServerInformation);
	}

	private async void HandleBattleJoining(BattleServerInformationForClient battleServerInformation)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (LobbyState != null)
		{
			LobbyState.OnBattleServerInformationReceived(battleServerInformation);
		}
		while (GameStateManager.Current.LastOrDefault<LobbyPracticeState>() != null)
		{
			await Task.Delay(5);
		}
		LobbyGameStateMatchmakerClient lobbyGameStateMatchmakerClient = Game.Current.GameStateManager.CreateState<LobbyGameStateMatchmakerClient>();
		lobbyGameStateMatchmakerClient.SetStartingParameters(this, ((BattleServerInformationForClient)(ref battleServerInformation)).PeerIndex, ((BattleServerInformationForClient)(ref battleServerInformation)).SessionKey, ((BattleServerInformationForClient)(ref battleServerInformation)).ServerAddress, ((BattleServerInformationForClient)(ref battleServerInformation)).ServerPort, ((BattleServerInformationForClient)(ref battleServerInformation)).GameType, ((BattleServerInformationForClient)(ref battleServerInformation)).SceneName);
		Game.Current.GameStateManager.PushState((GameState)(object)lobbyGameStateMatchmakerClient, 0);
	}

	void ILobbyClientSessionHandler.OnBattleServerLost()
	{
		GameStateManager gameStateManager = Game.Current.GameStateManager;
		if (!(gameStateManager.ActiveState is LobbyState))
		{
			if (gameStateManager.ActiveState is MissionState)
			{
				BannerlordNetwork.EndMultiplayerLobbyMission();
			}
			else
			{
				gameStateManager.PopState(0);
			}
		}
		LobbyState.OnBattleServerLost();
	}

	void ILobbyClientSessionHandler.OnRemovedFromMatchmakerGame(DisconnectType disconnectType)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		GameStateManager gameStateManager = Game.Current.GameStateManager;
		if (!(gameStateManager.ActiveState is LobbyState))
		{
			if (gameStateManager.ActiveState is MissionState)
			{
				BannerlordNetwork.EndMultiplayerLobbyMission();
			}
			else
			{
				gameStateManager.PopState(0);
			}
		}
		LobbyState.OnRemovedFromMatchmakerGame(disconnectType);
	}

	void ILobbyClientSessionHandler.OnRejoinBattleRequestAnswered(bool isSuccessful)
	{
		LobbyState.OnRejoinBattleRequestAnswered(isSuccessful);
	}

	void ILobbyClientSessionHandler.OnRegisterCustomGameServerResponse()
	{
		if (!GameNetwork.IsSessionActive)
		{
			LobbyGameStatePlayerBasedCustomServer lobbyGameStatePlayerBasedCustomServer = Game.Current.GameStateManager.CreateState<LobbyGameStatePlayerBasedCustomServer>();
			lobbyGameStatePlayerBasedCustomServer.SetStartingParameters(this);
			Game.Current.GameStateManager.PushState((GameState)(object)lobbyGameStatePlayerBasedCustomServer, 0);
		}
	}

	void ILobbyClientSessionHandler.OnCustomGameEnd()
	{
		if (Game.Current == null)
		{
			return;
		}
		GameStateManager gameStateManager = Game.Current.GameStateManager;
		if (!(gameStateManager.ActiveState is LobbyState))
		{
			if (Game.Current.GameStateManager.ActiveState is MissionState)
			{
				BannerlordNetwork.EndMultiplayerLobbyMission();
			}
			else
			{
				gameStateManager.PopState(0);
			}
		}
	}

	PlayerJoinGameResponseDataFromHost[] ILobbyClientSessionHandler.OnClientWantsToConnectCustomGame(PlayerJoinGameData[] playerJoinData)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		Debug.Print("Game join request with party received", 0, (DebugColor)4, 17592186044416uL);
		CustomGameJoinResponse val = (CustomGameJoinResponse)11;
		List<PlayerJoinGameResponseDataFromHost> list = new List<PlayerJoinGameResponseDataFromHost>();
		if (Mission.Current != null && (int)Mission.Current.CurrentState == 2)
		{
			PlayerJoinGameData[] array = playerJoinData;
			for (int i = 0; i < array.Length; i++)
			{
				if (CustomGameBannedPlayerManager.IsUserBanned(array[i].PlayerId))
				{
					val = (CustomGameJoinResponse)8;
				}
			}
			if ((int)val != 8)
			{
				bool flag = MultiplayerOptionsExtensions.GetIntValue((OptionType)16, (MultiplayerOptionsAccessMode)1) < GameNetwork.NetworkPeerCount + playerJoinData.Length;
				if (flag)
				{
					val = ((!flag) ? ((CustomGameJoinResponse)11) : ((CustomGameJoinResponse)2));
				}
				else
				{
					List<PlayerConnectionInfo> list2 = new List<PlayerConnectionInfo>();
					array = playerJoinData;
					foreach (PlayerJoinGameData val2 in array)
					{
						PlayerConnectionInfo val3 = new PlayerConnectionInfo(val2.PlayerId);
						Dictionary<int, List<int>> usedIndicesFromIds = CosmeticsManagerHelper.GetUsedIndicesFromIds(val2.UsedCosmetics);
						val3.AddParameter("PlayerData", (object)val2.PlayerData);
						val3.AddParameter("UsedCosmetics", (object)usedIndicesFromIds);
						val3.AddParameter("IsAdmin", (object)val2.IsAdmin);
						val3.AddParameter("IpAddress", (object)val2.IpAddress);
						val3.Name = val2.Name;
						list2.Add(val3);
					}
					AddPlayersResult val4 = GameNetwork.HandleNewClientsConnect(list2.ToArray(), false);
					if (val4.Success)
					{
						for (int j = 0; j < playerJoinData.Length; j++)
						{
							PlayerJoinGameData val5 = playerJoinData[j];
							NetworkCommunicator val6 = val4.NetworkPeers[j];
							PlayerJoinGameResponseDataFromHost item = new PlayerJoinGameResponseDataFromHost
							{
								PlayerId = val5.PlayerId,
								PeerIndex = val6.Index,
								SessionKey = val6.SessionKey,
								CustomGameJoinResponse = (CustomGameJoinResponse)0,
								IsAdmin = val6.IsAdmin
							};
							list.Add(item);
						}
						val = (CustomGameJoinResponse)0;
					}
					else
					{
						val = (CustomGameJoinResponse)3;
					}
				}
			}
		}
		else
		{
			val = (CustomGameJoinResponse)5;
		}
		if ((int)val != 0)
		{
			PlayerJoinGameData[] array = playerJoinData;
			foreach (PlayerJoinGameData val7 in array)
			{
				PlayerJoinGameResponseDataFromHost item2 = new PlayerJoinGameResponseDataFromHost
				{
					PlayerId = val7.PlayerId,
					PeerIndex = -1,
					SessionKey = -1,
					CustomGameJoinResponse = val
				};
				list.Add(item2);
			}
		}
		Debug.Print("Responding game join request with " + val, 0, (DebugColor)12, 17592186044416uL);
		return list.ToArray();
	}

	void ILobbyClientSessionHandler.OnJoinCustomGameResponse(bool success, JoinGameData joinGameData, CustomGameJoinResponse failureReason, bool isAdmin)
	{
		if (!success)
		{
			return;
		}
		Module.CurrentModule.GetMultiplayerGameMode(joinGameData.GameServerProperties.GameType).JoinCustomGame(joinGameData);
		Task.Run(async delegate
		{
			while (!GameNetwork.IsMyPeerReady)
			{
				await Task.Delay(1);
			}
			GameNetwork.MyPeer.UpdateForJoiningCustomGame(isAdmin);
		});
		Debug.Print("Join game successful", 0, (DebugColor)4, 17592186044416uL);
	}

	void ILobbyClientSessionHandler.OnJoinCustomGameFailureResponse(CustomGameJoinResponse response)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		LobbyState.OnJoinCustomGameFailureResponse(response);
	}

	void ILobbyClientSessionHandler.OnQuitFromCustomGame()
	{
		GameStateManager gameStateManager = Game.Current.GameStateManager;
		if (!(gameStateManager.ActiveState is LobbyState))
		{
			if (gameStateManager.ActiveState is MissionState)
			{
				BannerlordNetwork.EndMultiplayerLobbyMission();
			}
			else
			{
				gameStateManager.PopState(0);
			}
		}
	}

	void ILobbyClientSessionHandler.OnRemovedFromCustomGame(DisconnectType disconnectType)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected I4, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		GameStateManager gameStateManager = Game.Current.GameStateManager;
		if (!(gameStateManager.ActiveState is LobbyState))
		{
			if (gameStateManager.ActiveState is MissionState)
			{
				BannerlordNetwork.EndMultiplayerLobbyMission();
			}
			else
			{
				gameStateManager.PopState(0);
			}
		}
		LobbyState.OnRemovedFromCustomGame(disconnectType);
		if (LobbyState.LobbyClient.IsInParty)
		{
			switch ((int)disconnectType)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 9:
			case 10:
				LobbyState.LobbyClient.KickPlayerFromParty(LobbyState.LobbyClient.PlayerID);
				break;
			case 7:
			case 8:
				break;
			}
		}
	}

	void ILobbyClientSessionHandler.OnEnterCustomBattleWithPartyAnswer()
	{
	}

	void ILobbyClientSessionHandler.OnClientQuitFromCustomGame(PlayerId playerId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.Current == null || (int)Mission.Current.CurrentState != 2)
		{
			return;
		}
		NetworkCommunicator val = ((IEnumerable<NetworkCommunicator>)GameNetwork.NetworkPeers).FirstOrDefault((Func<NetworkCommunicator, bool>)((NetworkCommunicator x) => x.VirtualPlayer.Id == playerId));
		if (val != null && !val.IsServerPeer)
		{
			if (PeerExtensions.GetComponent<MissionPeer>(val) != null)
			{
				val.QuitFromMission = true;
			}
			GameNetwork.AddNetworkPeerToDisconnectAsServer(val);
			MBDebug.Print(string.Concat("player with id ", playerId, " quit from game"), 0, (DebugColor)12, 17592186044416uL);
		}
	}

	void ILobbyClientSessionHandler.OnAnnouncementReceived(Announcement announcement)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (Mission.Current != null && (int)Mission.Current.CurrentState == 2)
		{
			if ((int)announcement.Type == 0)
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject(announcement.Text, (Dictionary<string, object>)null)).ToString(), Color.FromUint(4292235858u)));
			}
			else if ((int)announcement.Type == 1)
			{
				InformationManager.AddSystemNotification(((object)new TextObject(announcement.Text, (Dictionary<string, object>)null)).ToString());
			}
		}
	}

	async Task<bool> ILobbyClientSessionHandler.OnInviteToPlatformSession(PlayerId playerId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return await LobbyState.OnInviteToPlatformSession(playerId);
	}
}
