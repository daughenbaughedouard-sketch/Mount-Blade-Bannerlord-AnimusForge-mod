using System;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade;

public sealed class LobbyGameStateMatchmakerClient : LobbyGameState
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static PrivilegeResult _003C_003E9__10_0;

		internal void _003CStartMultiplayer_003Eb__10_0(bool result)
		{
			if (!result)
			{
				PlatformServices.Instance.ShowRestrictedInformation();
			}
		}
	}

	private LobbyClient _gameClient;

	private int _playerIndex;

	private int _sessionKey;

	private string _address;

	private int _assignedPort;

	private string _multiplayerGameType;

	private string _scene;

	private LobbyGameClientHandler _lobbyGameClientHandler;

	public void SetStartingParameters(LobbyGameClientHandler lobbyGameClientHandler, int playerIndex, int sessionKey, string address, int assignedPort, string multiplayerGameType, string scene)
	{
		_lobbyGameClientHandler = lobbyGameClientHandler;
		_gameClient = lobbyGameClientHandler.GameClient;
		_playerIndex = playerIndex;
		_sessionKey = sessionKey;
		_address = address;
		_assignedPort = assignedPort;
		_multiplayerGameType = multiplayerGameType;
		_scene = scene;
	}

	protected override void OnActivate()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		base.OnActivate();
		if (_gameClient != null && ((int)_gameClient.CurrentState == 4 || (int)_gameClient.CurrentState == 10 || !_gameClient.Connected))
		{
			((GameState)this).GameStateManager.PopState(0);
		}
	}

	protected override void StartMultiplayer()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		GameNetwork.StartMultiplayerOnClient(_address, _assignedPort, _sessionKey, _playerIndex);
		BannerlordNetwork.StartMultiplayerLobbyMission((LobbyMissionType)0);
		if (!Module.CurrentModule.StartMultiplayerGame(_multiplayerGameType, _scene))
		{
			Debug.FailedAssert("[DEBUG]Invalid multiplayer game type.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\LobbyGameState.cs", "StartMultiplayer", 301);
		}
		IPlatformServices instance = PlatformServices.Instance;
		if (instance == null)
		{
			return;
		}
		object obj = _003C_003Ec._003C_003E9__10_0;
		if (obj == null)
		{
			PrivilegeResult val = delegate(bool result)
			{
				if (!result)
				{
					PlatformServices.Instance.ShowRestrictedInformation();
				}
			};
			_003C_003Ec._003C_003E9__10_0 = val;
			obj = (object)val;
		}
		instance.CheckPrivilege((Privilege)1, true, (PrivilegeResult)obj);
	}
}
