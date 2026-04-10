using System;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade;

public sealed class LobbyGameStateCustomGameClient : LobbyGameState
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

	private string _address;

	private int _port;

	private int _peerIndex;

	private int _sessionKey;

	private Timer _inactivityTimer;

	private static readonly float InactivityThreshold = 2f;

	public void SetStartingParameters(LobbyClient gameClient, string address, int port, int peerIndex, int sessionKey)
	{
		_gameClient = gameClient;
		_address = address;
		_port = port;
		_peerIndex = peerIndex;
		_sessionKey = sessionKey;
	}

	protected override void OnActivate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		base.OnActivate();
		_inactivityTimer = new Timer(MBCommon.GetApplicationTime(), InactivityThreshold, true);
		if (_gameClient != null && (_gameClient.AtLobby || !_gameClient.Connected))
		{
			((GameState)this).GameStateManager.PopState(0);
		}
	}

	protected override void OnTick(float dt)
	{
		((GameState)this).OnTick(dt);
		if (GameNetwork.IsClient && _inactivityTimer.Check(MBCommon.GetApplicationTime()) && _gameClient != null)
		{
			((Client<LobbyClient>)(object)_gameClient).IsInCriticalState = GameNetwork.ElapsedTimeSinceLastUdpPacketArrived() > (double)InactivityThreshold;
		}
	}

	protected override void StartMultiplayer()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		MBDebug.Print("CUSTOM GAME SERVER ADDRESS: " + _address, 0, (DebugColor)12, 17592186044416uL);
		GameNetwork.StartMultiplayerOnClient(_address, _port, _sessionKey, _peerIndex);
		BannerlordNetwork.StartMultiplayerLobbyMission((LobbyMissionType)1);
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
