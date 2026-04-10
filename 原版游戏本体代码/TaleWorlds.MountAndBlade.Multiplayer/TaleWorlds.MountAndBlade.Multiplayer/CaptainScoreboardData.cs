using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public class CaptainScoreboardData : IScoreboardData
{
	public ScoreboardHeader[] GetScoreboardHeaders()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		PeerExtensions.GetComponent<MissionRepresentativeBase>(GameNetwork.MyPeer);
		return (ScoreboardHeader[])(object)new ScoreboardHeader[9]
		{
			new ScoreboardHeader("ping", (Func<MissionPeer, string>)((MissionPeer missionPeer) => MathF.Round(PeerExtensions.GetNetworkPeer((PeerComponent)(object)missionPeer).AveragePingInMilliseconds).ToString()), (Func<BotData, string>)((BotData bot) => "")),
			new ScoreboardHeader("avatar", (Func<MissionPeer, string>)((MissionPeer missionPeer) => ""), (Func<BotData, string>)((BotData bot) => "")),
			new ScoreboardHeader("badge", (Func<MissionPeer, string>)delegate(MissionPeer missionPeer)
			{
				Badge byIndex = BadgeManager.GetByIndex(PeerExtensions.GetPeer((PeerComponent)(object)missionPeer).ChosenBadgeIndex);
				return (byIndex == null) ? null : byIndex.StringId;
			}, (Func<BotData, string>)((BotData bot) => "")),
			new ScoreboardHeader("name", (Func<MissionPeer, string>)((MissionPeer missionPeer) => ((PeerComponent)missionPeer).GetComponent<MissionPeer>().DisplayedName), (Func<BotData, string>)((BotData bot) => ((object)new TextObject("{=hvQSOi79}Bot", (Dictionary<string, object>)null)).ToString())),
			new ScoreboardHeader("kill", (Func<MissionPeer, string>)((MissionPeer missionPeer) => missionPeer.KillCount.ToString()), (Func<BotData, string>)((BotData bot) => bot.KillCount.ToString())),
			new ScoreboardHeader("death", (Func<MissionPeer, string>)((MissionPeer missionPeer) => missionPeer.DeathCount.ToString()), (Func<BotData, string>)((BotData bot) => bot.DeathCount.ToString())),
			new ScoreboardHeader("assist", (Func<MissionPeer, string>)((MissionPeer missionPeer) => missionPeer.AssistCount.ToString()), (Func<BotData, string>)((BotData bot) => bot.AssistCount.ToString())),
			new ScoreboardHeader("soldiers", (Func<MissionPeer, string>)delegate(MissionPeer missionPeer)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Expected O, but got Unknown
				TextObject val = new TextObject("{=4FVIuqsl}{ALIVE}/{TOTAL}", (Dictionary<string, object>)null);
				val.SetTextVariable("ALIVE", missionPeer.BotsUnderControlAlive + (missionPeer.IsControlledAgentActive ? 1 : 0));
				val.SetTextVariable("TOTAL", missionPeer.BotsUnderControlTotal + 1);
				return ((object)val).ToString();
			}, (Func<BotData, string>)delegate(BotData bot)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_002f: Expected O, but got Unknown
				TextObject val = new TextObject("{=4FVIuqsl}{ALIVE}/{TOTAL}", (Dictionary<string, object>)null);
				val.SetTextVariable("ALIVE", bot.AliveCount);
				val.SetTextVariable("TOTAL", 0);
				return ((object)val).ToString();
			}),
			new ScoreboardHeader("score", (Func<MissionPeer, string>)((MissionPeer missionPeer) => missionPeer.Score.ToString()), (Func<BotData, string>)((BotData bot) => bot.Score.ToString()))
		};
	}
}
