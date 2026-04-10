using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade;

public sealed class LobbyGameStatePlayerBasedCustomServer : LobbyGameState
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static PrivilegeResult _003C_003E9__4_0;

		internal void _003CHandleServerStartMultiplayer_003Eb__4_0(bool result)
		{
			if (!result)
			{
				PlatformServices.Instance.ShowRestrictedInformation();
			}
		}
	}

	private LobbyClient _gameClient;

	public void SetStartingParameters(LobbyGameClientHandler lobbyGameClientHandler)
	{
		_gameClient = lobbyGameClientHandler.GameClient;
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (_gameClient != null && (_gameClient.AtLobby || !_gameClient.Connected))
		{
			((GameState)this).GameStateManager.PopState(0);
		}
	}

	protected override void StartMultiplayer()
	{
		HandleServerStartMultiplayer();
	}

	private async void HandleServerStartMultiplayer()
	{
		GameNetwork.PreStartMultiplayerOnServer();
		BannerlordNetwork.StartMultiplayerLobbyMission((LobbyMissionType)1);
		if (!Module.CurrentModule.StartMultiplayerGame(_gameClient.CustomGameType, _gameClient.CustomGameScene))
		{
			Debug.FailedAssert("[DEBUG]Invalid multiplayer game type.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\LobbyGameState.cs", "HandleServerStartMultiplayer", 346);
		}
		while (Mission.Current == null || (int)Mission.Current.CurrentState != 2)
		{
			await Task.Delay(1);
		}
		GameNetwork.StartMultiplayerOnServer(9999);
		if (_gameClient.IsInGame)
		{
			BannerlordNetwork.CreateServerPeer();
			MBDebug.Print("Server: I finished loading and I am now visible to clients in the server list.", 0, (DebugColor)12, 17179869184uL);
			if (!GameNetwork.IsDedicatedServer)
			{
				GameNetwork.ClientFinishedLoading(GameNetwork.MyPeer);
			}
		}
		IPlatformServices instance = PlatformServices.Instance;
		if (instance == null)
		{
			return;
		}
		object obj = _003C_003Ec._003C_003E9__4_0;
		if (obj == null)
		{
			PrivilegeResult val = delegate(bool result)
			{
				if (!result)
				{
					PlatformServices.Instance.ShowRestrictedInformation();
				}
			};
			_003C_003Ec._003C_003E9__4_0 = val;
			obj = (object)val;
		}
		instance.CheckPrivilege((Privilege)1, false, (PrivilegeResult)obj);
	}
}
