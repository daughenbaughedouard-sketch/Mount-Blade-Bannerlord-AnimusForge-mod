using System;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade;

public sealed class LobbyGameStateCommunityClient : LobbyGameState
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static PrivilegeResult _003C_003E9__7_0;

		internal void _003CStartMultiplayer_003Eb__7_0(bool result)
		{
			if (!result)
			{
				PlatformServices.Instance.ShowRestrictedInformation();
			}
		}
	}

	private CommunityClient _communityClient;

	private string _address;

	private int _port;

	private int _peerIndex;

	private int _sessionKey;

	public void SetStartingParameters(CommunityClient communityClient, string address, int port, int peerIndex, int sessionKey)
	{
		_communityClient = communityClient;
		_address = address;
		_port = port;
		_peerIndex = peerIndex;
		_sessionKey = sessionKey;
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (_communityClient != null && !_communityClient.IsInGame)
		{
			((GameState)this).GameStateManager.PopState(0);
		}
	}

	protected override void StartMultiplayer()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		MBDebug.Print("COMMUNITY GAME SERVER ADDRESS: " + _address, 0, (DebugColor)12, 17592186044416uL);
		GameNetwork.StartMultiplayerOnClient(_address, _port, _sessionKey, _peerIndex);
		BannerlordNetwork.StartMultiplayerLobbyMission((LobbyMissionType)2);
		IPlatformServices instance = PlatformServices.Instance;
		if (instance == null)
		{
			return;
		}
		object obj = _003C_003Ec._003C_003E9__7_0;
		if (obj == null)
		{
			PrivilegeResult val = delegate(bool result)
			{
				if (!result)
				{
					PlatformServices.Instance.ShowRestrictedInformation();
				}
			};
			_003C_003Ec._003C_003E9__7_0 = val;
			obj = (object)val;
		}
		instance.CheckPrivilege((Privilege)1, true, (PrivilegeResult)obj);
	}

	protected override void OnDisconnectedFromServer()
	{
		base.OnDisconnectedFromServer();
		if ((object)Game.Current.GameStateManager.ActiveState == this)
		{
			((GameState)this).GameStateManager.PopState(0);
		}
	}
}
