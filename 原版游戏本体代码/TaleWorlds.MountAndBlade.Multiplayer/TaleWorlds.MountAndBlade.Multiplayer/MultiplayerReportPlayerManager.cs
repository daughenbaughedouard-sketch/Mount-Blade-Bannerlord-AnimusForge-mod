using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public static class MultiplayerReportPlayerManager
{
	private static Dictionary<PlayerId, int> _reportsPerPlayer = new Dictionary<PlayerId, int>();

	private const int _maxReportsPerPlayer = 3;

	public static event Action<string, PlayerId, string, bool> ReportHandlers;

	public static void RequestReportPlayer(string gameId, PlayerId playerId, string playerName, bool isRequestedFromMission)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerReportPlayerManager.ReportHandlers?.Invoke(gameId, playerId, playerName, isRequestedFromMission);
	}

	public static void OnPlayerReported(PlayerId playerId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		IncrementReportOfPlayer(playerId);
		NetworkCommunicator? obj = GameNetwork.NetworkPeers.Find((NetworkCommunicator x) => x.VirtualPlayer.Id == playerId);
		if (obj != null)
		{
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(obj);
			if (component != null)
			{
				component.SetMuted(true);
			}
		}
		Game.Current.GetGameHandler<ChatBox>().SetPlayerMuted(playerId, true);
	}

	public static bool IsPlayerReportedOverLimit(PlayerId player)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (_reportsPerPlayer.TryGetValue(player, out var value))
		{
			return value == 3;
		}
		return false;
	}

	private static void IncrementReportOfPlayer(PlayerId player)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (_reportsPerPlayer.ContainsKey(player))
		{
			_reportsPerPlayer[player]++;
		}
		else
		{
			_reportsPerPlayer.Add(player, 1);
		}
	}
}
