using System;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond.Lobby;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;
using TaleWorlds.MountAndBlade.Multiplayer.NetworkComponents;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace TaleWorlds.MountAndBlade;

public class MissionMatchHistoryComponent : MissionNetwork
{
	private bool _recordedHistory;

	private MatchHistoryData _matchHistoryData;

	public static MissionMatchHistoryComponent CreateIfConditionsAreMet()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		BaseNetworkComponent networkComponent = GameNetwork.GetNetworkComponent<BaseNetworkComponent>();
		if ((networkComponent == null || (int)networkComponent.ClientIntermissionState == 0 || NetworkMain.GameClient.IsInGame) && NetworkMain.GameClient.LastBattleIsOfficial)
		{
			return new MissionMatchHistoryComponent();
		}
		Debug.Print($"Failed to create {typeof(MissionMatchHistoryComponent).Name}. NetworkMain.GameClient.IsInGame: {NetworkMain.GameClient.IsInGame}, NetworkMain.GameClient.LastBattleIsOfficial: {NetworkMain.GameClient.LastBattleIsOfficial}", 0, (DebugColor)12, 17592186044416uL);
		return null;
	}

	private MissionMatchHistoryComponent()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		_recordedHistory = false;
		MatchHistoryData matchHistoryData = default(MatchHistoryData);
		if (MultiplayerLocalDataManager.Instance.MatchHistory.TryGetHistoryData(NetworkMain.GameClient.CurrentMatchId, ref matchHistoryData))
		{
			_matchHistoryData = matchHistoryData;
		}
		else
		{
			_matchHistoryData = new MatchHistoryData();
			_matchHistoryData.MatchId = NetworkMain.GameClient.CurrentMatchId;
		}
		_matchHistoryData.MatchDate = DateTime.Now;
	}

	private static void PrintDebugLog(string text)
	{
		Debug.Print("[MATCH_HISTORY_COMPONTENT]: " + text, 0, (DebugColor)9, 17592186044416uL);
	}

	public override void OnBehaviorInitialize()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		MissionMultiplayerGameModeBaseClient missionBehavior = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		int num = ((missionBehavior != null) ? ((int)missionBehavior.GameType) : 0);
		_matchHistoryData.GameType = ((object)(MultiplayerGameType)num/*cast due to .constrained prefix*/).ToString();
	}

	public override void AfterStart()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).AfterStart();
		string strValue = MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1);
		string strValue2 = MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1);
		string strValue3 = MultiplayerOptionsExtensions.GetStrValue((OptionType)13, (MultiplayerOptionsAccessMode)1);
		_matchHistoryData.Faction1 = strValue;
		_matchHistoryData.Faction2 = strValue2;
		_matchHistoryData.Map = strValue3;
		MissionPeer.OnTeamChanged += new OnTeamChangedDelegate(TeamChange);
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_matchHistoryData.MatchType = ((object)BannerlordNetwork.LobbyMissionType/*cast due to .constrained prefix*/).ToString();
	}

	protected override void AddRemoveMessageHandlers(NetworkMessageHandlerRegistererContainer registerer)
	{
		registerer.RegisterBaseHandler<MissionStateChange>((ServerMessageHandlerDelegate<GameNetworkMessage>)HandleServerEventMissionStateChange);
	}

	private void TeamChange(NetworkCommunicator player, Team oldTeam, Team nextTeam)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected I4, but got Unknown
		_matchHistoryData.AddOrUpdatePlayer(((object)player.VirtualPlayer.Id/*cast due to .constrained prefix*/).ToString(), player.VirtualPlayer.UserName, player.ForcedAvatarIndex, (int)nextTeam.Side);
	}

	private void HandleServerEventMissionStateChange(GameNetworkMessage baseMessage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected I4, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((MissionStateChange)baseMessage).CurrentState != 2)
		{
			return;
		}
		PrintDebugLog("Received mission ending message from server");
		if (_recordedHistory)
		{
			return;
		}
		PrintDebugLog("Match history is eligible for recording after end message");
		MissionScoreboardComponent missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionScoreboardComponent>();
		if (missionBehavior != null && !missionBehavior.IsOneSided)
		{
			int roundScore = missionBehavior.GetRoundScore((BattleSideEnum)1);
			int roundScore2 = missionBehavior.GetRoundScore((BattleSideEnum)0);
			BattleSideEnum matchWinnerSide = missionBehavior.GetMatchWinnerSide();
			_matchHistoryData.WinnerTeam = (int)matchWinnerSide;
			_matchHistoryData.AttackerScore = roundScore;
			_matchHistoryData.DefenderScore = roundScore2;
			MissionScoreboardSide[] sides = missionBehavior.Sides;
			for (int i = 0; i < sides.Length; i++)
			{
				foreach (MissionPeer player in sides[i].Players)
				{
					_matchHistoryData.TryUpdatePlayerStats(((object)((PeerComponent)player).Peer.Id/*cast due to .constrained prefix*/).ToString(), player.KillCount, player.DeathCount, player.AssistCount);
				}
			}
		}
		((MultiplayerLocalDataContainer<MatchHistoryData>)(object)MultiplayerLocalDataManager.Instance.MatchHistory).AddEntry(_matchHistoryData);
		_recordedHistory = true;
		PrintDebugLog("Recorded match history after end message");
	}

	public override void OnRemoveBehavior()
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		PrintDebugLog("Removing match history behavior");
		if (!_recordedHistory)
		{
			PrintDebugLog("Match history was eligible for recording when removing behavior");
			_matchHistoryData.WinnerTeam = -1;
			MissionScoreboardComponent missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionScoreboardComponent>();
			if (missionBehavior != null && !missionBehavior.IsOneSided)
			{
				int roundScore = missionBehavior.GetRoundScore((BattleSideEnum)1);
				int roundScore2 = missionBehavior.GetRoundScore((BattleSideEnum)0);
				_matchHistoryData.AttackerScore = roundScore;
				_matchHistoryData.DefenderScore = roundScore2;
				MissionScoreboardSide[] sides = missionBehavior.Sides;
				for (int i = 0; i < sides.Length; i++)
				{
					foreach (MissionPeer player in sides[i].Players)
					{
						_matchHistoryData.TryUpdatePlayerStats(((object)((PeerComponent)player).Peer.Id/*cast due to .constrained prefix*/).ToString(), player.KillCount, player.DeathCount, player.AssistCount);
					}
				}
			}
			((MultiplayerLocalDataContainer<MatchHistoryData>)(object)MultiplayerLocalDataManager.Instance.MatchHistory).AddEntry(_matchHistoryData);
			PrintDebugLog("Recorded match history after removing behavior");
			_recordedHistory = true;
		}
		MissionPeer.OnTeamChanged -= new OnTeamChangedDelegate(TeamChange);
		((MissionNetwork)this).OnRemoveBehavior();
	}
}
