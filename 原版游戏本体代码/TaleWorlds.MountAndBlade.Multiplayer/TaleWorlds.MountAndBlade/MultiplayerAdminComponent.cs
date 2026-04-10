using System;
using System.Collections.Generic;
using NetworkMessages.FromClient;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade;

public class MultiplayerAdminComponent : MissionNetwork
{
	public delegate void OnSelectPlayerToKickDelegate(bool banPlayer);

	public delegate void OnSetAdminMenuActiveStateDelegate(bool showMenu);

	private MissionLobbyComponent _missionLobbyComponent;

	private static MultiplayerAdminComponent _multiplayerAdminComponent;

	public event OnSetAdminMenuActiveStateDelegate OnSetAdminMenuActiveState;

	public MultiplayerAdminComponent()
	{
		if (string.IsNullOrEmpty(MultiplayerIntermissionVotingManager.Instance.InitialGameType))
		{
			MultiplayerIntermissionVotingManager.Instance.InitialGameType = MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1);
		}
	}

	public override void OnMissionStateActivated()
	{
		((MissionBehavior)this).OnMissionStateActivated();
		MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = !MultiplayerIntermissionVotingManager.Instance.IsDisableMapVoteOverride;
		MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = !MultiplayerIntermissionVotingManager.Instance.IsDisableCultureVoteOverride;
	}

	public void ChangeAdminMenuActiveState(bool isActive)
	{
		this.OnSetAdminMenuActiveState?.Invoke(isActive);
	}

	public void KickPlayer(NetworkCommunicator peerToKick, bool banPlayer)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (GameNetwork.IsServer)
		{
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(peerToKick);
			if (!peerToKick.IsMine && component != null && !peerToKick.IsAdmin)
			{
				DisconnectInfo val = (DisconnectInfo)(((object)peerToKick.PlayerConnectionInfo.GetParameter<DisconnectInfo>("DisconnectInfo")) ?? ((object)new DisconnectInfo()));
				val.Type = (DisconnectType)2;
				peerToKick.PlayerConnectionInfo.AddParameter("DisconnectInfo", (object)val);
				GameNetwork.AddNetworkPeerToDisconnectAsServer(peerToKick);
				if (banPlayer)
				{
					CustomGameBannedPlayerManager.AddBannedPlayer(peerToKick.VirtualPlayer.Id, int.MaxValue);
				}
			}
		}
		else if (GameNetwork.IsClient && !peerToKick.IsMine)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new KickPlayer(peerToKick, banPlayer));
			GameNetwork.EndModuleEventAsClient();
		}
	}

	public void GlobalMuteUnmutePlayer(NetworkCommunicator peerToMute, bool unmute)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		if (GameNetwork.IsServer)
		{
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(peerToMute);
			if (peerToMute.IsMine || component == null || peerToMute.IsAdmin)
			{
				return;
			}
			PlayerId id = peerToMute.VirtualPlayer.Id;
			if (MultiplayerGlobalMutedPlayersManager.IsUserMuted(id) == unmute)
			{
				if (unmute)
				{
					MultiplayerGlobalMutedPlayersManager.UnmutePlayer(peerToMute.VirtualPlayer.Id);
				}
				else
				{
					MultiplayerGlobalMutedPlayersManager.MutePlayer(peerToMute.VirtualPlayer.Id);
				}
				GameNetwork.BeginBroadcastModuleEvent();
				GameNetwork.WriteMessage((GameNetworkMessage)new SyncPlayerMuteState(id, !unmute));
				GameNetwork.EndBroadcastModuleEvent((EventBroadcastFlags)0, (NetworkCommunicator)null);
			}
		}
		else if (GameNetwork.IsClient && !peerToMute.IsMine)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new AdminMuteUnmutePlayer(peerToMute, unmute));
			GameNetwork.EndModuleEventAsClient();
		}
	}

	public void EndWarmup()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		if (GameNetwork.IsServer)
		{
			if (Mission.Current != null)
			{
				MultiplayerWarmupComponent missionBehavior = Mission.Current.GetMissionBehavior<MultiplayerWarmupComponent>();
				if (missionBehavior != null)
				{
					missionBehavior.EndWarmupProgress();
				}
			}
		}
		else
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new AdminRequestEndWarmup());
			GameNetwork.EndModuleEventAsClient();
		}
	}

	public void ChangeWelcomeMessage(string newWelcomeMessage)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		if (GameNetwork.IsServer)
		{
			MultiplayerOptionsExtensions.SetValue((OptionType)1, newWelcomeMessage, (MultiplayerOptionsAccessMode)1);
			SyncImmediateOptions();
		}
		else if (MultiplayerOptionsExtensions.GetStrValue((OptionType)1, (MultiplayerOptionsAccessMode)1) != newWelcomeMessage)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new ChangeWelcomeMessage(newWelcomeMessage));
			GameNetwork.EndModuleEventAsClient();
		}
	}

	public void AdminAnnouncement(string message, bool isBroadcast)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (GameNetwork.IsServer)
		{
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage((GameNetworkMessage)new ServerAdminMessage(message, isBroadcast));
			GameNetwork.EndBroadcastModuleEvent((EventBroadcastFlags)0, (NetworkCommunicator)null);
		}
		else
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new AdminRequestAnnouncement(message, isBroadcast));
			GameNetwork.EndModuleEventAsClient();
		}
	}

	public void ChangeClassRestriction(FormationClass classToChangeRestriction, bool newValue)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		if (GameNetwork.IsServer)
		{
			_missionLobbyComponent.ChangeClassRestriction(classToChangeRestriction, newValue);
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage((GameNetworkMessage)new ChangeClassRestrictions(classToChangeRestriction, newValue));
			GameNetwork.EndBroadcastModuleEvent((EventBroadcastFlags)0, (NetworkCommunicator)null);
		}
		else if (!_missionLobbyComponent.IsClassAvailable(classToChangeRestriction) != newValue)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new AdminRequestClassRestrictionChange(classToChangeRestriction, newValue));
			GameNetwork.EndModuleEventAsClient();
		}
	}

	public void AdminEndMission()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		if (GameNetwork.IsServer)
		{
			_missionLobbyComponent.SetStateEndingAsServer();
			return;
		}
		GameNetwork.BeginModuleEventAsClient();
		GameNetwork.WriteMessage((GameNetworkMessage)new AdminRequestEndMission());
		GameNetwork.EndModuleEventAsClient();
	}

	public override void OnBehaviorInitialize()
	{
		((MissionNetwork)this).OnBehaviorInitialize();
		_missionLobbyComponent = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();
		_missionLobbyComponent.OnAdminMessageRequested += AdminAnnouncement;
	}

	protected override void AddRemoveMessageHandlers(NetworkMessageHandlerRegistererContainer registerer)
	{
		if (GameNetwork.IsServer)
		{
			registerer.RegisterBaseHandler<KickPlayer>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleClientEventKickPlayer);
			registerer.RegisterBaseHandler<ChangeWelcomeMessage>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleClientEventChangeWelcomeMessage);
			registerer.RegisterBaseHandler<AdminRequestAnnouncement>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleClientEventAdminRequestAnnouncement);
			registerer.RegisterBaseHandler<AdminRequestClassRestrictionChange>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleClientEventAdminRequestClassRestrictionChange);
			registerer.RegisterBaseHandler<AdminRequestEndMission>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleClientEventAdminRequestEndMission);
			registerer.RegisterBaseHandler<AdminUpdateMultiplayerOptions>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleAdminUpdateMultiplayerOptions);
			registerer.RegisterBaseHandler<AdminMuteUnmutePlayer>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleClientEventMuteUnmutePlayer);
			registerer.RegisterBaseHandler<AdminRequestEndWarmup>((ClientMessageHandlerDelegate<GameNetworkMessage>)HandleClientEventAdminRequestEndWarmup);
		}
	}

	private bool HandleAdminUpdateMultiplayerOptions(NetworkCommunicator peer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Invalid comparison between Unknown and I4
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Invalid comparison between Unknown and I4
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Invalid comparison between Unknown and I4
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Expected O, but got Unknown
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected I4, but got Unknown
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		AdminUpdateMultiplayerOptions val = (AdminUpdateMultiplayerOptions)baseMessage;
		if (peer.IsAdmin && val.Options != null)
		{
			bool flag = false;
			bool flag2 = false;
			string text = MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1);
			string text2 = null;
			string text3 = null;
			bool flag3 = false;
			bool flag4 = false;
			for (int i = 0; i < val.Options.Count; i++)
			{
				AdminMultiplayerOptionInfo val2 = val.Options[i];
				bool flag5 = true;
				if ((int)val2.OptionType == 11)
				{
					flag5 = !string.IsNullOrEmpty(val2.StringValue);
					if (val2.StringValue == MultiplayerIntermissionVotingManager.Instance.InitialGameType || (!flag5 && MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1) == MultiplayerIntermissionVotingManager.Instance.InitialGameType))
					{
						flag = true;
					}
					if (flag5)
					{
						text = val2.StringValue;
					}
				}
				if ((int)val2.OptionType == 13)
				{
					flag5 = !string.IsNullOrEmpty(val2.StringValue);
					flag2 = !flag5;
				}
				if ((int)val2.OptionType == 14)
				{
					flag3 = !string.IsNullOrEmpty(val2.StringValue);
					text2 = (flag3 ? val2.StringValue : null);
				}
				else if ((int)val2.OptionType == 15)
				{
					flag4 = !string.IsNullOrEmpty(val2.StringValue);
					text3 = (flag4 ? val2.StringValue : null);
				}
				else if (flag5)
				{
					OptionValueType optionValueType = MultiplayerOptionsExtensions.GetOptionProperty(val2.OptionType).OptionValueType;
					switch ((int)optionValueType)
					{
					case 0:
						MultiplayerOptionsExtensions.SetValue(val2.OptionType, val2.BoolValue, val2.AccessMode);
						break;
					case 1:
					case 2:
						MultiplayerOptionsExtensions.SetValue(val2.OptionType, val2.IntValue, val2.AccessMode);
						break;
					case 3:
						MultiplayerOptionsExtensions.SetValue(val2.OptionType, val2.StringValue, val2.AccessMode);
						break;
					}
				}
			}
			if (flag2)
			{
				MultiplayerIntermissionVotingManager.Instance.IsMapSelectedByAdmin = false;
				if (flag)
				{
					if (MultiplayerIntermissionVotingManager.Instance.IsDisableMapVoteOverride)
					{
						MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;
						string id = Extensions.GetRandomElement<IntermissionVoteItem>((IReadOnlyList<IntermissionVoteItem>)MultiplayerIntermissionVotingManager.Instance.MapVoteItems).Id;
						MultiplayerOptionsExtensions.SetValue((OptionType)13, id, (MultiplayerOptionsAccessMode)2);
						Debug.Print("[Admin] game type was default and map was undecided. Voting was disabled. Selected map randomly from automated map pool: " + id, 0, (DebugColor)12, 17592186044416uL);
					}
					else
					{
						MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = true;
						Debug.Print("[Admin] game type was default and map was undecided. Maps will be voted from automated map pool", 0, (DebugColor)12, 17592186044416uL);
					}
				}
				else
				{
					MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;
					string randomElement = Extensions.GetRandomElement<string>((IReadOnlyList<string>)MultiplayerIntermissionVotingManager.Instance.GetUsableMaps(text));
					MultiplayerOptionsExtensions.SetValue((OptionType)13, randomElement, (MultiplayerOptionsAccessMode)2);
					Debug.Print("[Admin] game type wasn't default and map was undecided. Selected map randomly from usable maps: " + randomElement + ".", 0, (DebugColor)12, 17592186044416uL);
				}
			}
			else
			{
				MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;
				MultiplayerIntermissionVotingManager.Instance.IsMapSelectedByAdmin = true;
				Debug.Print("[Admin] next game type: " + text + " next map: " + MultiplayerOptionsExtensions.GetStrValue((OptionType)13, (MultiplayerOptionsAccessMode)2), 0, (DebugColor)12, 17592186044416uL);
			}
			if (flag3 && flag4)
			{
				MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
				MultiplayerOptionsExtensions.SetValue((OptionType)14, text2, (MultiplayerOptionsAccessMode)2);
				MultiplayerOptionsExtensions.SetValue((OptionType)15, text3, (MultiplayerOptionsAccessMode)2);
				Debug.Print("[Admin] Both cultures were valid. Setting " + text2 + " vs " + text3 + " for next game.", 0, (DebugColor)12, 17592186044416uL);
			}
			else if (MultiplayerIntermissionVotingManager.Instance.IsDisableCultureVoteOverride)
			{
				MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
				MultiplayerIntermissionVotingManager.Instance.SelectRandomCultures((MultiplayerOptionsAccessMode)2);
				string strValue = MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)2);
				string strValue2 = MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)2);
				Debug.Print("[Admin] Cultures weren't valid. Randomly setting " + strValue + " vs " + strValue2 + " for next game.", 0, (DebugColor)12, 17592186044416uL);
			}
			else
			{
				MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = true;
				Debug.Print("[Admin] Cultures weren't valid. Culture voting is enabled", 0, (DebugColor)12, 17592186044416uL);
			}
			GameNetwork.BeginBroadcastModuleEvent();
			GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerOptionsImmediate());
			GameNetwork.EndBroadcastModuleEvent((EventBroadcastFlags)0, (NetworkCommunicator)null);
			GameNetwork.BeginModuleEventAsServer(peer);
			GameNetwork.WriteMessage((GameNetworkMessage)new UpdateIntermissionVotingManagerValues());
			GameNetwork.EndModuleEventAsServer();
		}
		return true;
	}

	private bool HandleClientEventKickPlayer(NetworkCommunicator peer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		KickPlayer val = (KickPlayer)baseMessage;
		if (peer.IsAdmin)
		{
			KickPlayer(val.PlayerPeer, val.BanPlayer);
		}
		return true;
	}

	private bool HandleClientEventMuteUnmutePlayer(NetworkCommunicator peer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		AdminMuteUnmutePlayer val = (AdminMuteUnmutePlayer)baseMessage;
		if (peer.IsAdmin)
		{
			GlobalMuteUnmutePlayer(val.PlayerPeer, val.Unmute);
		}
		return true;
	}

	private bool HandleClientEventChangeWelcomeMessage(NetworkCommunicator peer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		ChangeWelcomeMessage val = (ChangeWelcomeMessage)baseMessage;
		if (peer.IsAdmin)
		{
			ChangeWelcomeMessage(val.NewWelcomeMessage);
		}
		return true;
	}

	private bool HandleClientEventAdminRequestClassRestrictionChange(NetworkCommunicator peer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		AdminRequestClassRestrictionChange val = (AdminRequestClassRestrictionChange)baseMessage;
		if (peer.IsAdmin)
		{
			ChangeClassRestriction(val.ClassToChangeRestriction, val.NewValue);
		}
		return true;
	}

	private bool HandleClientEventAdminRequestAnnouncement(NetworkCommunicator peer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		AdminRequestAnnouncement val = (AdminRequestAnnouncement)baseMessage;
		if (peer.IsAdmin)
		{
			AdminAnnouncement(val.Message, val.IsAdminBroadcast);
		}
		return true;
	}

	private bool HandleClientEventAdminRequestEndMission(NetworkCommunicator peer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		_ = (AdminRequestEndMission)baseMessage;
		if (peer.IsAdmin)
		{
			AdminEndMission();
		}
		return true;
	}

	[CommandLineArgumentFunction("announcement", "mp_admin")]
	public static string MPAdminAnnouncement(List<string> strings)
	{
		if (strings.Count == 0)
		{
			return "Wrong format! Usage: mp_admin.announcement {TEXT}";
		}
		if (Mission.Current == null)
		{
			return "Mission is not running!";
		}
		MultiplayerAdminComponent missionBehavior = Mission.Current.GetMissionBehavior<MultiplayerAdminComponent>();
		if (missionBehavior == null)
		{
			return "Admin component could not be found!";
		}
		missionBehavior.AdminAnnouncement(string.Join(" ", strings), isBroadcast: true);
		return "Success";
	}

	private bool HandleClientEventAdminRequestEndWarmup(NetworkCommunicator peer, GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		_ = (AdminRequestEndWarmup)baseMessage;
		if (peer.IsAdmin)
		{
			EndWarmup();
		}
		return true;
	}

	public override void OnRemoveBehavior()
	{
		_multiplayerAdminComponent = null;
		((MissionNetwork)this).OnRemoveBehavior();
	}

	private void SyncImmediateOptions()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		GameNetwork.BeginBroadcastModuleEvent();
		GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerOptionsImmediate());
		GameNetwork.EndBroadcastModuleEvent((EventBroadcastFlags)0, (NetworkCommunicator)null);
	}

	[CommandLineArgumentFunction("kick_player", "mp_admin")]
	public static string MPAdminKickPlayer(List<string> strings)
	{
		if (_multiplayerAdminComponent == null)
		{
			return "Failed: MultiplayerAdminComponent has not been created.";
		}
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		if (myPeer == null || !myPeer.IsAdmin)
		{
			return "Failed: Only admins can use mp_admin commands.";
		}
		if (strings.Count != 1)
		{
			return "Failed: Input is incorrect.";
		}
		string text = strings[0];
		foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
		{
			if (networkPeer.UserName == text)
			{
				_multiplayerAdminComponent.KickPlayer(networkPeer, banPlayer: false);
				return "Player " + text + " has been kicked from the server.";
			}
		}
		return text + " could not be found.";
	}

	[CommandLineArgumentFunction("ban_player", "mp_admin")]
	public static string MPAdminBanPlayer(List<string> strings)
	{
		if (_multiplayerAdminComponent == null)
		{
			return "Failed: MultiplayerAdminComponent has not been created.";
		}
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		if (myPeer == null || !myPeer.IsAdmin)
		{
			return "Failed: Only admins can use mp_admin commands.";
		}
		if (strings.Count != 1)
		{
			return "Failed: Input is incorrect.";
		}
		string text = strings[0];
		foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
		{
			if (networkPeer.UserName == text)
			{
				_multiplayerAdminComponent.KickPlayer(networkPeer, banPlayer: true);
				return "Player " + text + " has been banned from the server.";
			}
		}
		return text + " could not be found.";
	}

	[CommandLineArgumentFunction("change_welcome_message", "mp_admin")]
	public static string MPAdminChangeWelcomeMessage(List<string> strings)
	{
		if (_multiplayerAdminComponent == null)
		{
			return "Failed: MultiplayerAdminComponent has not been created.";
		}
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		if (myPeer == null || !myPeer.IsAdmin)
		{
			return "Failed: Only admins can use mp_host commands.";
		}
		string text = "";
		foreach (string @string in strings)
		{
			text = text + @string + " ";
		}
		_multiplayerAdminComponent.ChangeWelcomeMessage(text);
		return "Changed welcome message to: " + text;
	}

	[CommandLineArgumentFunction("change_class_restriction", "mp_admin")]
	public static string MPAdminChangeClassRestriction(List<string> strings)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (strings.Count != 2 || !Enum.TryParse<FormationClass>(strings[0], out FormationClass result) || !bool.TryParse(strings[1], out var result2))
		{
			return "Wrong format! Usage: mp_admin.change_class_restriction {FormationClass} {true/false}";
		}
		if (Mission.Current == null)
		{
			return "Mission is not running!";
		}
		MultiplayerAdminComponent missionBehavior = Mission.Current.GetMissionBehavior<MultiplayerAdminComponent>();
		if (missionBehavior == null)
		{
			return "Admin component could not be found!";
		}
		missionBehavior.ChangeClassRestriction(result, result2);
		return "Success";
	}

	[CommandLineArgumentFunction("restart_game", "mp_admin")]
	public static string MPHostRestartGame(List<string> strings)
	{
		if (Mission.Current == null)
		{
			return "Mission is not running!";
		}
		MultiplayerAdminComponent missionBehavior = Mission.Current.GetMissionBehavior<MultiplayerAdminComponent>();
		if (missionBehavior == null)
		{
			return "Admin component could not be found!";
		}
		missionBehavior.AdminEndMission();
		return "Success";
	}

	[CommandLineArgumentFunction("change_server_slots", "mp_admin")]
	public static string MPAdminChangeServerSlots(List<string> strings)
	{
		if (Mission.Current == null)
		{
			return "Mission is not running!";
		}
		if (Mission.Current.GetMissionBehavior<MultiplayerAdminComponent>() == null)
		{
			return "Admin component could not be found!";
		}
		if (strings.Count != 1)
		{
			return "Wrong format! Usage: mp_admin.change_server_slots {NUMBER}";
		}
		if (int.TryParse(strings[0], out var result))
		{
			MultiplayerOptionsExtensions.SetValue((OptionType)16, result, (MultiplayerOptionsAccessMode)2);
			return "Success";
		}
		return "Wrong format! Usage: mp_admin.change_server_slots {NUMBER}";
	}
}
