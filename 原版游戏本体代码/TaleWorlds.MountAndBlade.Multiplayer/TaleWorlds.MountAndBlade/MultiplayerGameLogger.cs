using System.Collections.Generic;
using System.Threading;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace TaleWorlds.MountAndBlade;

public class MultiplayerGameLogger : GameHandler
{
	public const int PreInitialLogId = 0;

	private ChatBox _chatBox;

	private int _lastLogId;

	private List<GameLog> _gameLogs;

	public IReadOnlyList<GameLog> GameLogs => _gameLogs.AsReadOnly();

	public MultiplayerGameLogger()
	{
		_lastLogId = 0;
		_gameLogs = new List<GameLog>();
	}

	public void Log(GameLog log)
	{
		log.Id = Interlocked.Increment(ref _lastLogId);
		_gameLogs?.Add(log);
	}

	protected override void OnGameStart()
	{
	}

	public override void OnBeforeSave()
	{
	}

	public override void OnAfterSave()
	{
	}

	protected override void OnGameNetworkBegin()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		_chatBox = Game.Current.GetGameHandler<ChatBox>();
		NetworkMessageHandlerRegisterer val = new NetworkMessageHandlerRegisterer((RegisterMode)0);
		if (GameNetwork.IsServer)
		{
			val.Register<PlayerMessageAll>((ClientMessageHandlerDelegate<PlayerMessageAll>)HandleClientEventPlayerMessageAll);
			val.Register<PlayerMessageTeam>((ClientMessageHandlerDelegate<PlayerMessageTeam>)HandleClientEventPlayerMessageTeam);
		}
	}

	private bool HandleClientEventPlayerMessageAll(NetworkCommunicator networkPeer, PlayerMessageAll message)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		GameLog val = new GameLog((GameLogType)2, networkPeer.VirtualPlayer.Id, MBCommon.GetTotalMissionTime());
		val.Data.Add("Message", message.Message);
		val.Data.Add("IsTeam", false.ToString());
		Dictionary<string, string> data = val.Data;
		ChatBox chatBox = _chatBox;
		data.Add("IsMuted", ((chatBox != null) ? new bool?(chatBox.IsPlayerMuted(networkPeer.VirtualPlayer.Id)) : ((bool?)null)).ToString());
		val.Data.Add("IsGlobalMuted", networkPeer.IsMuted.ToString());
		Log(val);
		return true;
	}

	private bool HandleClientEventPlayerMessageTeam(NetworkCommunicator networkPeer, PlayerMessageTeam message)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		GameLog val = new GameLog((GameLogType)2, networkPeer.VirtualPlayer.Id, MBCommon.GetTotalMissionTime());
		val.Data.Add("Message", message.Message);
		val.Data.Add("IsTeam", true.ToString());
		Dictionary<string, string> data = val.Data;
		ChatBox chatBox = _chatBox;
		data.Add("IsMuted", ((chatBox != null) ? new bool?(chatBox.IsPlayerMuted(networkPeer.VirtualPlayer.Id)) : ((bool?)null)).ToString());
		val.Data.Add("IsGlobalMuted", networkPeer.IsMuted.ToString());
		Log(val);
		return true;
	}
}
