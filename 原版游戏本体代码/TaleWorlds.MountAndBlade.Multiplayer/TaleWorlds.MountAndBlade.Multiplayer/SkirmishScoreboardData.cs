using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public class SkirmishScoreboardData : IScoreboardData
{
	public ScoreboardHeader[] GetScoreboardHeaders()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
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
			new ScoreboardHeader("gold", (Func<MissionPeer, string>)((MissionPeer missionPeer) => ((PeerComponent)missionPeer).GetComponent<FlagDominationMissionRepresentative>().GetGoldAmountForVisual().ToString()), (Func<BotData, string>)((BotData bot) => "")),
			new ScoreboardHeader("score", (Func<MissionPeer, string>)((MissionPeer missionPeer) => missionPeer.Score.ToString()), (Func<BotData, string>)((BotData bot) => "".ToString()))
		};
	}
}
