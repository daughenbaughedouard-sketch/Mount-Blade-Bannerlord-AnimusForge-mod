using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Diamond.AccessProvider.Test;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Library.NewsManager;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Lobby;
using TaleWorlds.MountAndBlade.Diamond.Ranked;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade;

public class LobbyState : GameState
{
	private const string _newsSourceURLBase = "https://cdn.taleworlds.com/upload/bannerlordnews/NewsFeed_";

	private BannerlordFriendListService _bannerlordFriendListService;

	private RecentPlayersFriendListService _recentPlayersFriendListService;

	private ClanFriendListService _clanFriendListService;

	private readonly object _sessionInvitationDataLock = new object();

	private ILobbyStateHandler _handler;

	private LobbyGameClientHandler _lobbyGameClientManager;

	private ConcurrentDictionary<(PlayerId PlayerId, Permission Permission), bool> _registeredPermissionEvents;

	private List<Func<GameServerEntry, List<CustomServerAction>>> _onCustomServerActionRequestedForServerEntry;

	public Action<bool> OnMultiplayerPrivilegeUpdated;

	public Action<bool> OnCrossplayPrivilegeUpdated;

	public Action<bool> OnUserGeneratedContentPrivilegeUpdated;

	private bool AutoConnect
	{
		get
		{
			if (TestCommonBase.BaseInstance != null && TestCommonBase.BaseInstance.IsTestEnabled)
			{
				return false;
			}
			return true;
		}
	}

	public override bool IsMenuState => true;

	public override bool IsMusicMenuState => false;

	public bool IsLoggingIn { get; private set; }

	public ILobbyStateHandler Handler
	{
		get
		{
			return _handler;
		}
		set
		{
			_handler = value;
		}
	}

	public LobbyClient LobbyClient => NetworkMain.GameClient;

	public NewsManager NewsManager { get; private set; }

	public bool? HasMultiplayerPrivilege { get; private set; }

	public bool? HasCrossplayPrivilege { get; private set; }

	public bool? HasUserGeneratedContentPrivilege { get; private set; }

	public event Action<GameServerEntry> ClientRefusedToJoinCustomServer;

	public LobbyState()
	{
		_registeredPermissionEvents = new ConcurrentDictionary<(PlayerId, Permission), bool>();
		_onCustomServerActionRequestedForServerEntry = new List<Func<GameServerEntry, List<CustomServerAction>>>();
	}

	public void InitializeLogic(ILobbyStateHandler lobbyStateHandler)
	{
		Handler = lobbyStateHandler;
	}

	protected override async void OnInitialize()
	{
		_003C_003En__0();
		MultiplayerLocalDataManager.InitializeManager();
		CommunityClient communityClient = NetworkMain.CommunityClient;
		CommunityClientOnlineLobbyGameHandler handler = new CommunityClientOnlineLobbyGameHandler(this);
		communityClient.Handler = (ICommunityClientHandler)(object)handler;
		LobbyClient.SetLoadedModules(Utilities.GetModulesNames());
		PlatformServices.Instance.OnSignInStateUpdated += OnPlatformSignInStateUpdated;
		PlatformServices.Instance.OnNameUpdated += OnPlayerNameUpdated;
		IFriendListService[] friendListServices = PlatformServices.Instance.GetFriendListServices();
		foreach (IFriendListService val in friendListServices)
		{
			Type type = ((object)val).GetType();
			if (type == typeof(BannerlordFriendListService))
			{
				_bannerlordFriendListService = (BannerlordFriendListService)val;
			}
			else if (type == typeof(RecentPlayersFriendListService))
			{
				_recentPlayersFriendListService = (RecentPlayersFriendListService)val;
			}
			else if (type == typeof(ClanFriendListService))
			{
				_clanFriendListService = (ClanFriendListService)val;
			}
		}
		NewsManager = new NewsManager();
		NewsManager.SetNewsSourceURL(GetApplicableNewsSourceURL());
		RecentPlayersManager.Initialize();
		_onCustomServerActionRequestedForServerEntry = new List<Func<GameServerEntry, List<CustomServerAction>>>();
		_lobbyGameClientManager = new LobbyGameClientHandler();
		_lobbyGameClientManager.LobbyState = this;
		NewsManager.UpdateNewsItems(false);
		if (HasMultiplayerPrivilege == true && AutoConnect)
		{
			await TryLogin();
		}
		else
		{
			SetConnectionState(isAuthenticated: false);
			OnResume();
		}
		if ((int)PlatformServices.SessionInvitationType != 0)
		{
			OnSessionInvitationAccepted(PlatformServices.SessionInvitationType);
		}
		else if (PlatformServices.IsPlatformRequestedMultiplayer)
		{
			OnPlatformRequestedMultiplayer();
		}
		PlatformServices.OnSessionInvitationAccepted = (Action<SessionInvitationType>)Delegate.Combine(PlatformServices.OnSessionInvitationAccepted, new Action<SessionInvitationType>(OnSessionInvitationAccepted));
		PlatformServices.OnPlatformRequestedMultiplayer = (Action)Delegate.Combine(PlatformServices.OnPlatformRequestedMultiplayer, new Action(OnPlatformRequestedMultiplayer));
	}

	protected override void OnActivate()
	{
		((GameState)this).OnActivate();
		MultiplayerIntermissionVotingManager.Instance.UsableMaps.Clear();
	}

	private void OnPlayerNameUpdated(string newName)
	{
		LobbyClient.OnPlayerNameUpdated(newName);
		Handler?.OnPlayerNameUpdated(newName);
	}

	protected override void OnFinalize()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		((GameState)this).OnFinalize();
		MultiplayerLocalDataManager.FinalizeManager();
		PlatformServices.OnPlatformRequestedMultiplayer = (Action)Delegate.Remove(PlatformServices.OnPlatformRequestedMultiplayer, new Action(OnPlatformRequestedMultiplayer));
		PlatformServices.OnSessionInvitationAccepted = (Action<SessionInvitationType>)Delegate.Remove(PlatformServices.OnSessionInvitationAccepted, new Action<SessionInvitationType>(OnSessionInvitationAccepted));
		PlatformServices.Instance.OnSignInStateUpdated -= OnPlatformSignInStateUpdated;
		PlatformServices.Instance.OnNameUpdated -= OnPlayerNameUpdated;
		LobbyClient.RemoveLobbyClientHandler();
		RecentPlayersManager.Serialize();
		NewsManager.OnFinalize();
		NewsManager = null;
		_onCustomServerActionRequestedForServerEntry.Clear();
		_onCustomServerActionRequestedForServerEntry = null;
		foreach (var key in _registeredPermissionEvents.Keys)
		{
			if (PlatformServices.Instance.UnregisterPermissionChangeEvent(key.PlayerId, key.Permission, new PermissionChanged(MultiplayerPermissionWithPlayerChanged)))
			{
				_registeredPermissionEvents.TryRemove((key.PlayerId, key.Permission), out var _);
			}
		}
	}

	protected override void OnTick(float dt)
	{
		((GameState)this).OnTick(dt);
		MultiplayerLocalDataManager.Instance.Tick(dt);
	}

	private string GetApplicableNewsSourceURL()
	{
		bool num = NewsManager.LocalizationID == "zh";
		bool isInPreviewMode = NewsManager.IsInPreviewMode;
		string text = (num ? "zh" : "en");
		if (!isInPreviewMode)
		{
			return "https://cdn.taleworlds.com/upload/bannerlordnews/NewsFeed_" + text + ".json";
		}
		return "https://cdn.taleworlds.com/upload/bannerlordnews/NewsFeed_" + text + "_preview.json";
	}

	private string GetApplicableAnnouncementsURL()
	{
		return "https://cdn.taleworlds.com/bannerlord-ingame/LobbyNewsFeed.json";
	}

	[Conditional("_RGL_KEEP_ASSERTS")]
	private void CheckValidityOfItems()
	{
		foreach (ItemObject item in (List<ItemObject>)(object)MBObjectManager.Instance.GetObjectTypeList<ItemObject>())
		{
			if (!item.IsUsingTeamColor)
			{
				continue;
			}
			MetaMesh copy = MetaMesh.GetCopy(item.MultiMeshName, false, false);
			for (int i = 0; i < copy.MeshCount; i++)
			{
				Material material = copy.GetMeshAtIndex(i).GetMaterial();
				if (material.Name != "vertex_color_lighting_skinned" && material.Name != "vertex_color_lighting" && (NativeObject)(object)material.GetTexture((MBTextureType)1) == (NativeObject)null)
				{
					MBDebug.ShowWarning(string.Concat("Item object(", item.Name, ") has 'Using Team Color' flag but does not have a mask texture in diffuse2 slot. "));
					break;
				}
			}
		}
	}

	public async Task UpdateHasMultiplayerPrivilege()
	{
		TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
		PlatformServices.Instance.CheckPrivilege((Privilege)0, true, (PrivilegeResult)delegate(bool result)
		{
			tsc.SetResult(result);
		});
		HasMultiplayerPrivilege = await tsc.Task;
		OnMultiplayerPrivilegeUpdated?.Invoke(HasMultiplayerPrivilege.Value);
	}

	public async Task UpdateHasCrossplayPrivilege()
	{
		TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
		PlatformServices.Instance.CheckPrivilege((Privilege)2, false, (PrivilegeResult)delegate(bool result)
		{
			tsc.SetResult(result);
		});
		HasCrossplayPrivilege = await tsc.Task;
		OnCrossplayPrivilegeUpdated?.Invoke(HasCrossplayPrivilege.Value);
	}

	public void OnClientRefusedToJoinCustomServer(GameServerEntry serverEntry)
	{
		this.ClientRefusedToJoinCustomServer?.Invoke(serverEntry);
	}

	public async Task UpdateHasUserGeneratedContentPrivilege(bool showResolveUI)
	{
		TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
		PlatformServices.Instance.CheckPrivilege((Privilege)5, showResolveUI, (PrivilegeResult)delegate(bool result)
		{
			tsc.SetResult(result);
		});
		HasUserGeneratedContentPrivilege = await tsc.Task;
		OnUserGeneratedContentPrivilegeUpdated?.Invoke(HasUserGeneratedContentPrivilege.Value);
	}

	public async Task TryLogin()
	{
		IsLoggingIn = true;
		LobbyClient gameClient = LobbyClient;
		if (gameClient.IsIdle)
		{
			await UpdateHasMultiplayerPrivilege();
			if (!HasMultiplayerPrivilege.Value)
			{
				string title = ((object)new TextObject("{=lVfmVHbz}Login Failed", (Dictionary<string, object>)null)).ToString();
				string message = ((object)new TextObject("{=cS0Hafjl}Player does not have access to multiplayer.", (Dictionary<string, object>)null)).ToString();
				ShowFeedback(title, message);
				IsLoggingIn = false;
				return;
			}
			await UpdateHasCrossplayPrivilege();
			await UpdateHasUserGeneratedContentPrivilege(showResolveUI: false);
			ILoginAccessProvider val = await PlatformServices.Instance.CreateLobbyClientLoginProvider();
			string userName = val.GetUserName();
			Func<Task<bool>> func = null;
			if (PlatformServices.InvitationServices != null)
			{
				func = async () => await PlatformServices.InvitationServices.OnLogin();
			}
			LobbyClientConnectResult val2 = await gameClient.Connect((ILobbyClientSessionHandler)(object)_lobbyGameClientManager, val, userName, HasUserGeneratedContentPrivilege == true, PlatformServices.Instance.GetInitParams(), func);
			if (val2.Connected)
			{
				Game.Current.GetGameHandler<ChatBox>().OnLogin();
				OnResume();
			}
			else
			{
				string title2 = ((object)new TextObject("{=lVfmVHbz}Login Failed", (Dictionary<string, object>)null)).ToString();
				ShowFeedback(title2, ((object)val2.Error).ToString());
				SetConnectionState(isAuthenticated: false);
				OnResume();
			}
		}
		IsLoggingIn = false;
	}

	public async Task TryLogin(string userName, string password)
	{
		IsLoggingIn = true;
		LobbyClientConnectResult val = await NetworkMain.GameClient.Connect((ILobbyClientSessionHandler)(object)_lobbyGameClientManager, (ILoginAccessProvider)new TestLoginAccessProvider(), userName, true, PlatformServices.Instance.GetInitParams(), (Func<Task<bool>>)null);
		if (!val.Connected)
		{
			string title = ((object)new TextObject("{=lVfmVHbz}Login Failed", (Dictionary<string, object>)null)).ToString();
			ShowFeedback(title, ((object)val.Error).ToString());
		}
		IsLoggingIn = false;
	}

	public void HostGame()
	{
		if (string.IsNullOrEmpty(MultiplayerOptionsExtensions.GetStrValue((OptionType)0, (MultiplayerOptionsAccessMode)1)))
		{
			MultiplayerOptionsExtensions.SetValue((OptionType)0, NetworkMain.GameClient.Name, (MultiplayerOptionsAccessMode)1);
		}
		string strValue = MultiplayerOptionsExtensions.GetStrValue((OptionType)2, (MultiplayerOptionsAccessMode)1);
		string strValue2 = MultiplayerOptionsExtensions.GetStrValue((OptionType)3, (MultiplayerOptionsAccessMode)1);
		string text = ((!string.IsNullOrEmpty(strValue)) ? Common.CalculateMD5Hash(strValue) : null);
		string text2 = ((!string.IsNullOrEmpty(strValue2)) ? Common.CalculateMD5Hash(strValue2) : null);
		MultiplayerOptionsExtensions.SetValue((OptionType)2, text, (MultiplayerOptionsAccessMode)1);
		MultiplayerOptionsExtensions.SetValue((OptionType)3, text2, (MultiplayerOptionsAccessMode)1);
		string strValue3 = MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1);
		string gameModule = MultiplayerGameTypes.GetGameTypeInfo(strValue3).GameModule;
		string strValue4 = MultiplayerOptionsExtensions.GetStrValue((OptionType)13, (MultiplayerOptionsAccessMode)1);
		string text3 = null;
		UniqueSceneId val = default(UniqueSceneId);
		if (Utilities.TryGetUniqueIdentifiersForScene(strValue4, ref val))
		{
			text3 = val.Serialize();
		}
		NetworkMain.GameClient.RegisterCustomGame(gameModule, strValue3, MultiplayerOptionsExtensions.GetStrValue((OptionType)0, (MultiplayerOptionsAccessMode)1), MultiplayerOptionsExtensions.GetIntValue((OptionType)16, (MultiplayerOptionsAccessMode)1), strValue4, text3, MultiplayerOptionsExtensions.GetStrValue((OptionType)2, (MultiplayerOptionsAccessMode)1), MultiplayerOptionsExtensions.GetStrValue((OptionType)3, (MultiplayerOptionsAccessMode)1), 9999);
	}

	public void CreatePremadeGame()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Invalid comparison between Unknown and I4
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Expected O, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Invalid comparison between Unknown and I4
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		string strValue = MultiplayerOptionsExtensions.GetStrValue((OptionType)0, (MultiplayerOptionsAccessMode)1);
		string strValue2 = MultiplayerOptionsExtensions.GetStrValue((OptionType)10, (MultiplayerOptionsAccessMode)1);
		string strValue3 = MultiplayerOptionsExtensions.GetStrValue((OptionType)13, (MultiplayerOptionsAccessMode)1);
		string strValue4 = MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1);
		string strValue5 = MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1);
		string strValue6 = MultiplayerOptionsExtensions.GetStrValue((OptionType)2, (MultiplayerOptionsAccessMode)1);
		PremadeGameType val = (PremadeGameType)Enum.GetValues(typeof(PremadeGameType)).GetValue(MultiplayerOptionsExtensions.GetIntValue((OptionType)12, (MultiplayerOptionsAccessMode)1));
		if ((int)val == 1)
		{
			bool flag = true;
			foreach (PartyPlayerInLobbyClient partyPlayer in NetworkMain.GameClient.PlayersInParty)
			{
				if (((IEnumerable<ClanPlayer>)NetworkMain.GameClient.PlayersInClan).FirstOrDefault((Func<ClanPlayer, bool>)((ClanPlayer clanPlayer) => clanPlayer.PlayerId == partyPlayer.PlayerId)) == null)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				ShowFeedback(((object)new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=uNrXwGzr}Only practice matches are allowed with your current party. All members should be in the same clan for a clan match.", (Dictionary<string, object>)null)).ToString());
				return;
			}
		}
		if (strValue != null && !Extensions.IsEmpty<char>((IEnumerable<char>)strValue) && (int)val != 2)
		{
			NetworkMain.GameClient.CreatePremadeGame(strValue, strValue2, strValue3, strValue4, strValue5, strValue6, val);
		}
		else if ((int)val == 2)
		{
			ShowFeedback(((object)new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=PfnS8HUd}Premade game type is invalid!", (Dictionary<string, object>)null)).ToString());
		}
		else
		{
			ShowFeedback(((object)new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=EgTUzWUz}Name Can't Be Empty!", (Dictionary<string, object>)null)).ToString());
		}
	}

	public string ShowFeedback(string title, string message)
	{
		if (Handler != null)
		{
			return Handler.ShowFeedback(title, message);
		}
		return null;
	}

	public string ShowFeedback(InquiryData inquiryData)
	{
		if (Handler != null)
		{
			return Handler.ShowFeedback(inquiryData);
		}
		return null;
	}

	public void DismissFeedback(string messageId)
	{
		if (Handler != null)
		{
			Handler.DismissFeedback(messageId);
		}
	}

	public void OnPause()
	{
		if (Handler != null)
		{
			Handler.OnPause();
		}
	}

	public void OnResume()
	{
		if (Handler != null)
		{
			Handler.OnResume();
		}
	}

	public void OnRequestedToSearchBattle()
	{
		if (Handler != null)
		{
			Handler.OnRequestedToSearchBattle();
		}
	}

	public void OnUpdateFindingGame(MatchmakingWaitTimeStats matchmakingWaitTimeStats, string[] gameTypeInfo = null)
	{
		if (Handler != null)
		{
			Handler.OnUpdateFindingGame(matchmakingWaitTimeStats, gameTypeInfo);
		}
	}

	public void OnRequestedToCancelSearchBattle()
	{
		if (Handler != null)
		{
			Handler.OnRequestedToCancelSearchBattle();
		}
	}

	public void OnCancelFindingGame()
	{
		if (Handler != null)
		{
			Handler.OnSearchBattleCanceled();
		}
	}

	public void OnDisconnected(TextObject feedback)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		if (Handler != null)
		{
			Handler.OnDisconnected();
		}
		if (feedback != (TextObject)null)
		{
			string title = ((object)new TextObject("{=MbXatV1Q}Disconnected", (Dictionary<string, object>)null)).ToString();
			ShowFeedback(title, ((object)feedback).ToString());
		}
	}

	public void OnPlayerDataReceived(PlayerData playerData)
	{
		if (Handler != null)
		{
			Handler.OnPlayerDataReceived(playerData);
		}
	}

	public void OnPendingRejoin()
	{
		Handler?.OnPendingRejoin();
	}

	public void OnEnterBattleWithParty(string[] selectedGameTypes)
	{
		if (Handler != null)
		{
			Handler.OnEnterBattleWithParty(selectedGameTypes);
		}
	}

	public async void OnPartyInvitationReceived(string inviterPlayerName, PlayerId playerId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		while (IsLoggingIn)
		{
			Debug.Print("Waiting for logging in to be done..", 0, (DebugColor)12, 17592186044416uL);
			await Task.Delay(100);
		}
		if (PermaMuteList.IsPlayerMuted(playerId))
		{
			LobbyClient.DeclinePartyInvitation();
		}
		else
		{
			if (Handler == null)
			{
				return;
			}
			PermissionResult val2 = default(PermissionResult);
			PlatformServices.Instance.CheckPrivilege((Privilege)3, true, (PrivilegeResult)delegate(bool privilegeResult)
			{
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				//IL_005c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Expected O, but got Unknown
				//IL_0068: Expected O, but got Unknown
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				if (privilegeResult)
				{
					PlayerIdProvidedTypes providedType = ((PlayerId)(ref playerId)).ProvidedType;
					PlayerId playerID = NetworkMain.GameClient.PlayerID;
					if (providedType != ((PlayerId)(ref playerID)).ProvidedType)
					{
						Handler?.OnPartyInvitationReceived(playerId);
					}
					else
					{
						IPlatformServices instance = PlatformServices.Instance;
						PlayerId val = playerId;
						PermissionResult obj = val2;
						if (obj == null)
						{
							PermissionResult val3 = delegate(bool permissionResult)
							{
								//IL_0014: Unknown result type (might be due to invalid IL or missing references)
								if (permissionResult)
								{
									Handler?.OnPartyInvitationReceived(playerId);
								}
								else
								{
									LobbyClient.DeclinePartyInvitation();
								}
							};
							PermissionResult val4 = val3;
							val2 = val3;
							obj = val4;
						}
						instance.CheckPermissionWithUser((Permission)1, val, obj);
					}
				}
				else
				{
					LobbyClient.DeclinePartyInvitation();
				}
			});
		}
	}

	public async void OnPartyJoinRequestReceived(PlayerId joiningPlayerId, PlayerId viaPlayerId, string viaFriendName)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		while (IsLoggingIn)
		{
			Debug.Print("Waiting for logging in to be done..", 0, (DebugColor)12, 17592186044416uL);
			await Task.Delay(100);
		}
		if (PermaMuteList.IsPlayerMuted(joiningPlayerId))
		{
			LobbyClient.DeclinePartyJoinRequest(joiningPlayerId, (PartyJoinDeclineReason)1);
		}
		else
		{
			if (Handler == null)
			{
				return;
			}
			PlayerIdProvidedTypes providedType = ((PlayerId)(ref joiningPlayerId)).ProvidedType;
			PlayerId playerID = NetworkMain.GameClient.PlayerID;
			if (providedType != ((PlayerId)(ref playerID)).ProvidedType)
			{
				Handler?.OnPartyJoinRequestReceived(joiningPlayerId, viaPlayerId, viaFriendName, !LobbyClient.IsInParty);
				return;
			}
			PlatformServices.Instance.CheckPermissionWithUser((Permission)1, joiningPlayerId, (PermissionResult)delegate(bool permissionResult)
			{
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				if (permissionResult)
				{
					Handler?.OnPartyJoinRequestReceived(joiningPlayerId, viaPlayerId, viaFriendName, !LobbyClient.IsInParty);
				}
				else
				{
					LobbyClient.DeclinePartyJoinRequest(joiningPlayerId, (PartyJoinDeclineReason)1);
				}
			});
		}
	}

	public void OnAdminMessageReceived(string message)
	{
		if (Handler != null)
		{
			Handler.OnAdminMessageReceived(message);
		}
	}

	public void OnPartyInvitationInvalidated()
	{
		if (Handler != null)
		{
			Handler.OnPartyInvitationInvalidated();
		}
	}

	public void OnPlayerInvitedToParty(PlayerId playerId)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (Handler != null)
		{
			Handler.OnPlayerInvitedToParty(playerId);
		}
	}

	public void OnPlayerRemovedFromParty(PlayerId playerId, PartyRemoveReason reason)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (((PlayerId)(ref playerId)).Equals(LobbyClient.PlayerID))
		{
			IPlatformInvitationServices invitationServices = PlatformServices.InvitationServices;
			if (invitationServices != null)
			{
				invitationServices.OnLeftParty();
			}
		}
		if (PlatformServices.Instance.UnregisterPermissionChangeEvent(playerId, (Permission)0, new PermissionChanged(MultiplayerPermissionWithPlayerChanged)))
		{
			_registeredPermissionEvents.TryRemove((playerId, (Permission)0), out var _);
		}
		if (Handler != null)
		{
			Handler.OnPlayerRemovedFromParty(playerId, reason);
		}
	}

	public void OnPlayersAddedToParty(List<(PlayerId PlayerId, string PlayerName, bool IsPartyLeader)> addedPlayers, List<(PlayerId PlayerId, string PlayerName)> invitedPlayers)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		PlayerId val;
		foreach (var player in addedPlayers)
		{
			val = player.PlayerId;
			PlayerIdProvidedTypes providedType = ((PlayerId)(ref val)).ProvidedType;
			val = LobbyClient.PlayerID;
			if (providedType != ((PlayerId)(ref val)).ProvidedType)
			{
				Handler?.OnPlayerAddedToParty(player.PlayerId, player.PlayerName, player.IsPartyLeader);
				continue;
			}
			PlatformServices.Instance.CheckPermissionWithUser((Permission)0, player.PlayerId, (PermissionResult)delegate(bool hasPermission)
			{
				//IL_0094: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Expected O, but got Unknown
				//IL_0040: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Unknown result type (might be due to invalid IL or missing references)
				if (hasPermission)
				{
					if (PlatformServices.Instance.RegisterPermissionChangeEvent(player.PlayerId, (Permission)0, new PermissionChanged(MultiplayerPermissionWithPlayerChanged)))
					{
						_registeredPermissionEvents.TryRemove((player.PlayerId, (Permission)0), out var _);
					}
					Handler?.OnPlayerAddedToParty(player.PlayerId, player.PlayerName, player.IsPartyLeader);
				}
				else
				{
					NetworkMain.GameClient.KickPlayerFromParty(NetworkMain.GameClient.PlayerID);
				}
			});
		}
		if (Handler == null)
		{
			return;
		}
		foreach (var invitedPlayer in invitedPlayers)
		{
			PlayerId playerId = invitedPlayer.PlayerId;
			PlayerIdProvidedTypes providedType2 = ((PlayerId)(ref playerId)).ProvidedType;
			val = LobbyClient.PlayerID;
			if (providedType2 != ((PlayerId)(ref val)).ProvidedType)
			{
				Handler?.OnPlayerInvitedToParty(playerId);
				continue;
			}
			PlatformServices.Instance.CheckPermissionWithUser((Permission)0, playerId, (PermissionResult)delegate(bool hasPermission)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				if (hasPermission)
				{
					Handler?.OnPlayerInvitedToParty(playerId);
				}
			});
		}
	}

	private void MultiplayerPermissionWithPlayerChanged(PlayerId targetPlayerId, Permission permission, bool hasPermission)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!hasPermission && ((IEnumerable<PartyPlayerInLobbyClient>)NetworkMain.GameClient.PlayersInParty).FirstOrDefault((Func<PartyPlayerInLobbyClient, bool>)((PartyPlayerInLobbyClient p) => p.PlayerId == targetPlayerId)) != null)
		{
			NetworkMain.GameClient.KickPlayerFromParty(NetworkMain.GameClient.PlayerID);
		}
	}

	public void OnGameClientStateChange(State state)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Invalid comparison between Unknown and I4
		if (!LobbyClient.IsInGame)
		{
			PlatformServices.MultiplayerGameStateChanged(false);
		}
		Handler?.OnGameClientStateChange(state);
		if ((int)state == 3)
		{
			MPPerkSelectionManager.Instance.InitializeForUser(LobbyClient.Name, LobbyClient.PlayerID);
		}
		else if ((int)state == 0)
		{
			MPPerkSelectionManager.FreeInstance();
		}
		else if (!LobbyClient.AtLobby)
		{
			MPPerkSelectionManager.Instance.ResetPendingChanges();
		}
		PlatformServices.LobbyClientStateChanged((int)state == 4, !LobbyClient.IsInParty || LobbyClient.IsPartyLeader);
	}

	public void SetConnectionState(bool isAuthenticated)
	{
		Handler?.SetConnectionState(isAuthenticated);
		PlatformServices.ConnectionStateChanged(isAuthenticated);
	}

	public void OnActivateHome()
	{
		Handler?.OnActivateHome();
	}

	public void OnActivateCustomServer()
	{
		Handler?.OnActivateCustomServer();
	}

	public void OnActivateMatchmaking()
	{
		Handler?.OnActivateMatchmaking();
	}

	public void OnActivateProfile()
	{
		Handler?.OnActivateProfile();
	}

	public void OnClanInvitationReceived(string clanName, string clanTag, bool isCreation)
	{
		Handler?.OnClanInvitationReceived(clanName, clanTag, isCreation);
	}

	public void OnClanInvitationAnswered(PlayerId playerId, ClanCreationAnswer answer)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Handler?.OnClanInvitationAnswered(playerId, answer);
	}

	public void OnClanCreationSuccessful()
	{
		Handler?.OnClanCreationSuccessful();
	}

	public void OnClanCreationFailed()
	{
		Handler?.OnClanCreationFailed();
	}

	public void OnClanCreationStarted()
	{
		Handler?.OnClanCreationStarted();
	}

	public void OnClanInfoChanged()
	{
		ClanFriendListService clanFriendListService = _clanFriendListService;
		if (clanFriendListService != null)
		{
			clanFriendListService.OnClanInfoChanged(LobbyClient.PlayerInfosInClan);
		}
		Handler?.OnClanInfoChanged();
	}

	public void OnPremadeGameEligibilityStatusReceived(bool isEligible)
	{
		Handler?.OnPremadeGameEligibilityStatusReceived(isEligible);
	}

	public void OnPremadeGameCreated()
	{
		Handler?.OnPremadeGameCreated();
	}

	public void OnPremadeGameListReceived()
	{
		Handler?.OnPremadeGameListReceived();
	}

	public void OnPremadeGameCreationCancelled()
	{
		Handler?.OnPremadeGameCreationCancelled();
	}

	public void OnJoinPremadeGameRequested(string clanName, string clanSigilCode, Guid partyId, PlayerId[] challengerPlayerIDs, PlayerId challengerPartyLeaderID, PremadeGameType premadeGameType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Handler?.OnJoinPremadeGameRequested(clanName, clanSigilCode, partyId, challengerPlayerIDs, challengerPartyLeaderID, premadeGameType);
	}

	public void OnJoinPremadeGameRequestSuccessful()
	{
		Handler?.OnJoinPremadeGameRequestSuccessful();
	}

	public void OnActivateArmory()
	{
		Handler?.OnActivateArmory();
	}

	public void OnActivateOptions()
	{
		Handler?.OnActivateOptions();
	}

	public void OnDeactivateOptions()
	{
		Handler?.OnDeactivateOptions();
	}

	public void OnCustomGameServerListReceived(AvailableCustomGames customGameServerList)
	{
		Handler?.OnCustomGameServerListReceived(customGameServerList);
	}

	public void OnMatchmakerGameOver(int oldExp, int newExp, List<string> badgesEarned, int lootGained, RankBarInfo oldRankBarInfo, RankBarInfo newRankBarInfo, BattleCancelReason battleCancelReason)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Handler?.OnMatchmakerGameOver(oldExp, newExp, badgesEarned, lootGained, oldRankBarInfo, newRankBarInfo, battleCancelReason);
	}

	public void OnBattleServerLost()
	{
		Handler?.OnBattleServerLost();
	}

	public void OnRemovedFromMatchmakerGame(DisconnectType disconnectType)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Handler?.OnRemovedFromMatchmakerGame(disconnectType);
	}

	public void OnRemovedFromCustomGame(DisconnectType disconnectType)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Handler?.OnRemovedFromCustomGame(disconnectType);
	}

	public void OnPlayerAssignedPartyLeader(PlayerId partyLeaderId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Handler?.OnPlayerAssignedPartyLeader(partyLeaderId);
	}

	public void OnPlayerSuggestedToParty(PlayerId playerId, string playerName, PlayerId suggestingPlayerId, string suggestingPlayerName)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Handler?.OnPlayerSuggestedToParty(playerId, playerName, suggestingPlayerId, suggestingPlayerName);
	}

	public void OnJoinCustomGameFailureResponse(CustomGameJoinResponse response)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Handler?.OnJoinCustomGameFailureResponse(response);
	}

	public void OnServerStatusReceived(ServerStatus serverStatus)
	{
		Handler?.OnServerStatusReceived(serverStatus);
	}

	public void OnFriendListReceived(FriendInfo[] friends)
	{
		Handler?.OnFriendListUpdated();
		BannerlordFriendListService bannerlordFriendListService = _bannerlordFriendListService;
		if (bannerlordFriendListService != null)
		{
			bannerlordFriendListService.OnFriendListReceived(friends);
		}
	}

	public void OnRecentPlayerStatusesReceived(FriendInfo[] friends)
	{
		RecentPlayersFriendListService recentPlayersFriendListService = _recentPlayersFriendListService;
		if (recentPlayersFriendListService != null)
		{
			((BannerlordFriendListService)recentPlayersFriendListService).OnFriendListReceived(friends);
		}
	}

	public void OnBattleServerInformationReceived(BattleServerInformationForClient battleServerInformation)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Handler?.OnBattleServerInformationReceived(battleServerInformation);
	}

	public void OnRejoinBattleRequestAnswered(bool isSuccessful)
	{
		Handler?.OnRejoinBattleRequestAnswered(isSuccessful);
	}

	internal void OnSigilChanged()
	{
		if (Handler != null)
		{
			Handler.OnSigilChanged();
		}
	}

	public void OnNotificationsReceived(LobbyNotification[] notifications)
	{
		if (Handler != null)
		{
			Handler.OnNotificationsReceived(notifications);
		}
	}

	private void OnPlatformSignInStateUpdated(bool isSignedIn, TextObject message)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!isSignedIn && LobbyClient.Connected)
		{
			LobbyClient.Logout((TextObject)(((object)message) ?? ((object)new TextObject("{=oPOa77dI}Logged out of platform", (Dictionary<string, object>)null))));
		}
	}

	[Conditional("DEBUG")]
	private void PrintCompressionInfoKey()
	{
		try
		{
			List<Type> list = new List<Type>();
			Assembly[] array = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
				where assembly.GetName().Name.StartsWith("TaleWorlds.")
				select assembly).ToArray();
			Assembly[] array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				Type type = Extensions.GetTypesSafe(array2[num], (Func<Type, bool>)null).FirstOrDefault((Type ty) => ty.Name.Contains("CompressionInfo"));
				if (type != null)
				{
					list.AddRange(type.GetNestedTypes());
					break;
				}
			}
			List<FieldInfo> list2 = new List<FieldInfo>();
			array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				foreach (Type item in Extensions.GetTypesSafe(array2[num], (Func<Type, bool>)null))
				{
					FieldInfo[] fields = item.GetFields();
					foreach (FieldInfo fieldInfo in fields)
					{
						if (list.Contains(fieldInfo.FieldType))
						{
							list2.Add(fieldInfo);
						}
					}
				}
			}
			int num3 = 0;
			foreach (FieldInfo item2 in list2)
			{
				object value = item2.GetValue(null);
				MethodInfo method = item2.FieldType.GetMethod("GetHashKey", BindingFlags.Instance | BindingFlags.NonPublic);
				num3 += (int)method.Invoke(value, new object[0]);
			}
			Debug.Print("CompressionInfoKey: " + num3, 0, (DebugColor)7, 17179869184uL);
		}
		catch
		{
			Debug.Print("CompressionInfoKey checking failed.", 0, (DebugColor)7, 17179869184uL);
		}
	}

	public async Task<bool> OnInviteToPlatformSession(PlayerId playerId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (!LobbyClient.Connected)
		{
			return false;
		}
		bool flag = false;
		if ((!LobbyClient.IsInParty || LobbyClient.IsPartyLeader) && LobbyClient.PlayersInParty.Count < Parameters.MaxPlayerCountInParty && PlatformServices.Instance.UsePlatformInvitationService(playerId))
		{
			flag = await PlatformServices.InvitationServices.OnInviteToPlatformSession(playerId);
		}
		if (!flag)
		{
			InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=ljHPjjmX}Could not invite player to the game", (Dictionary<string, object>)null)).ToString()));
		}
		return flag;
	}

	public async void OnPlatformRequestedMultiplayer()
	{
		PlatformServices.OnPlatformMultiplayerRequestHandled();
		await UpdateHasMultiplayerPrivilege();
		if (HasMultiplayerPrivilege.HasValue && HasMultiplayerPrivilege.Value && LobbyClient.IsIdle)
		{
			await TryLogin();
			int waitTime = 0;
			while ((int)LobbyClient.CurrentState != 0 && (int)LobbyClient.CurrentState != 4 && waitTime < 3000)
			{
				await Task.Delay(100);
				waitTime += 100;
			}
		}
	}

	public async void OnSessionInvitationAccepted(SessionInvitationType targetGameType)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)targetGameType != 1)
		{
			return;
		}
		PlatformServices.OnSessionInvitationHandled();
		await UpdateHasMultiplayerPrivilege();
		if (!HasMultiplayerPrivilege.HasValue || !HasMultiplayerPrivilege.Value)
		{
			return;
		}
		await Task.Delay(2000);
		if (LobbyClient.IsIdle)
		{
			await TryLogin();
			int waitTime = 0;
			while ((int)LobbyClient.CurrentState != 0 && (int)LobbyClient.CurrentState != 4 && waitTime < 3000)
			{
				await Task.Delay(100);
				waitTime += 100;
			}
		}
		_ = LobbyClient.CurrentState;
		_ = 4;
	}

	public List<CustomServerAction> GetCustomActionsForServer(GameServerEntry gameServerEntry)
	{
		List<CustomServerAction> list = new List<CustomServerAction>();
		for (int i = 0; i < _onCustomServerActionRequestedForServerEntry.Count; i++)
		{
			List<CustomServerAction> list2 = _onCustomServerActionRequestedForServerEntry[i](gameServerEntry);
			if (list2 != null && list2.Count > 0)
			{
				list.AddRange(list2);
			}
		}
		return list;
	}

	public void RegisterForCustomServerAction(Func<GameServerEntry, List<CustomServerAction>> action)
	{
		if (_onCustomServerActionRequestedForServerEntry != null)
		{
			_onCustomServerActionRequestedForServerEntry.Add(action);
		}
		else
		{
			Debug.FailedAssert("Lobby state is finalized", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\LobbyState.cs", "RegisterForCustomServerAction", 1179);
		}
	}

	public void UnregisterForCustomServerAction(Func<GameServerEntry, List<CustomServerAction>> action)
	{
		if (_onCustomServerActionRequestedForServerEntry != null)
		{
			_onCustomServerActionRequestedForServerEntry.Remove(action);
		}
		else
		{
			Debug.FailedAssert("Lobby state is finalized", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\LobbyState.cs", "UnregisterForCustomServerAction", 1191);
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		((GameState)this).OnInitialize();
	}
}
