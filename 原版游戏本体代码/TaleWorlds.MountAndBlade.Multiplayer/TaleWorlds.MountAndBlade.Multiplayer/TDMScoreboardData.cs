using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public class TDMScoreboardData : IScoreboardData
{
	public ScoreboardHeader[] GetScoreboardHeaders()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		PeerExtensions.GetComponent<MissionRepresentativeBase>(GameNetwork.MyPeer);
		return (ScoreboardHeader[])(object)new ScoreboardHeader[8]
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
			new ScoreboardHeader("score", (Func<MissionPeer, string>)((MissionPeer missionPeer) => missionPeer.Score.ToString()), (Func<BotData, string>)((BotData bot) => bot.Score.ToString()))
		};
	}
}
