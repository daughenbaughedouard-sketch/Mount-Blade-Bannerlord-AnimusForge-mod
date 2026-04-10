using System;
using System.Collections.Generic;
using TaleWorlds.Diamond;
using TaleWorlds.Diamond.ClientApplication;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlatformService;
using TaleWorlds.ServiceDiscovery.Client;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public static class MultiplayerMain
{
	private static ClientApplicationConfiguration _lobbyClientApplicationConfiguration;

	private static DiamondClientApplication _diamondClientApplication;

	public static LobbyClient GameClient => NetworkMain.GameClient;

	public static bool IsInitialized { get; private set; }

	static MultiplayerMain()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		IsInitialized = false;
		ServiceAddressManager.Initalize();
		_lobbyClientApplicationConfiguration = new ClientApplicationConfiguration();
		_lobbyClientApplicationConfiguration.FillFrom("LobbyClient");
		ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo("Multiplayer");
		if (!GameNetwork.IsDedicatedServer && moduleInfo != null)
		{
			_diamondClientApplication = new DiamondClientApplication(moduleInfo.Version);
			_diamondClientApplication.Initialize(_lobbyClientApplicationConfiguration);
			NetworkMain.SetPeers(_diamondClientApplication.GetClient<LobbyClient>("LobbyClient"), new CommunityClient(), (CustomBattleServer)null);
			MachineId.Initialize();
		}
	}

	public static void Initialize(IGameNetworkHandler gameNetworkHandler)
	{
		Debug.Print("Initializing NetworkMain", 0, (DebugColor)12, 17592186044416uL);
		MBCommon.CurrentGameType = (GameType)0;
		GameNetwork.InitializeCompressionInfos();
		if (!IsInitialized)
		{
			IsInitialized = true;
			GameNetwork.Initialize(gameNetworkHandler);
		}
		PermaMuteList.SetPermanentMuteAvailableCallback((Func<bool>)(() => PlatformServices.Instance.IsPermanentMuteAvailable));
		Debug.Print("NetworkMain Initialized", 0, (DebugColor)12, 17592186044416uL);
	}

	public static void InitializeAsDedicatedServer(IGameNetworkHandler gameNetworkHandler)
	{
		MBCommon.CurrentGameType = (GameType)2;
		GameNetwork.InitializeCompressionInfos();
		if (!IsInitialized)
		{
			IsInitialized = true;
			GameNetwork.Initialize(gameNetworkHandler);
			GameStartupInfo startupInfo = Module.CurrentModule.StartupInfo;
			GameNetwork.SetServerBandwidthLimitInMbps(startupInfo.ServerBandwidthLimitInMbps);
			GameNetwork.SetServerTickRate((double)startupInfo.ServerTickRate);
		}
	}

	internal static void Tick(float dt)
	{
		if (IsInitialized)
		{
			if (GameClient != null)
			{
				((Client<LobbyClient>)(object)GameClient).Update();
			}
			if (_diamondClientApplication != null)
			{
				_diamondClientApplication.Update();
			}
			GameNetwork.Tick(dt);
		}
	}

	public static MultiplayerGameType[] GetAvailableRankedGameModes()
	{
		return (MultiplayerGameType[])(object)new MultiplayerGameType[2]
		{
			(MultiplayerGameType)4,
			(MultiplayerGameType)5
		};
	}

	public static MultiplayerGameType[] GetAvailableCustomGameModes()
	{
		return (MultiplayerGameType[])(object)new MultiplayerGameType[2]
		{
			default(MultiplayerGameType),
			(MultiplayerGameType)2
		};
	}

	public static MultiplayerGameType[] GetAvailableQuickPlayGameModes()
	{
		return (MultiplayerGameType[])(object)new MultiplayerGameType[2]
		{
			(MultiplayerGameType)4,
			(MultiplayerGameType)5
		};
	}

	public static string[] GetAvailableMatchmakerRegions()
	{
		return new string[4] { "USE", "USW", "EU", "EA" };
	}

	public static string GetUserDefaultRegion()
	{
		return "None";
	}

	public static string GetUserCurrentRegion()
	{
		LobbyClient gameClient = GameClient;
		if (gameClient != null && gameClient.LoggedIn && GameClient.PlayerData != null)
		{
			return GameClient.PlayerData.LastRegion;
		}
		return GetUserDefaultRegion();
	}

	public static string[] GetUserSelectedGameTypes()
	{
		LobbyClient gameClient = GameClient;
		if (gameClient != null && gameClient.LoggedIn)
		{
			return GameClient.PlayerData.LastGameTypes;
		}
		return new string[0];
	}

	[CommandLineArgumentFunction("gettoken", "customserver")]
	public static string GetDedicatedCustomServerAuthToken(List<string> strings)
	{
		if (!(Common.PlatformFileHelper is PlatformFileHelperPC))
		{
			return "Platform not supported.";
		}
		if (GameClient == null)
		{
			return "Not logged into lobby.";
		}
		GetDedicatedCustomServerAuthToken();
		return string.Empty;
	}

	private static async void GetDedicatedCustomServerAuthToken()
	{
		string text = await GameClient.GetDedicatedCustomServerAuthToken();
		if (text == null)
		{
			MBDebug.EchoCommandWindow("Could not get token.");
			return;
		}
		PlatformDirectoryPath val = default(PlatformDirectoryPath);
		((PlatformDirectoryPath)(ref val))._002Ector((PlatformFileType)0, "Tokens");
		PlatformFilePath val2 = default(PlatformFilePath);
		((PlatformFilePath)(ref val2))._002Ector(val, "DedicatedCustomServerAuthToken.txt");
		FileHelper.SaveFileString(val2, text);
		MBDebug.EchoCommandWindow(text + " (Saved to " + ((PlatformFilePath)(ref val2)).FileFullPath + ")");
	}
}
