using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace StoryMode;

public abstract class StoryModeQuestBase : QuestBase
{
	public override string SpecialQuestType => "MainStoryline";

	public override bool IsRemainingTimeHidden => true;

	protected StoryModeQuestBase(string questId, Hero questGiver, CampaignTime duration)
		: base(questId, questGiver, duration, 0)
	{
	}//IL_0003: Unknown result type (might be due to invalid IL or missing references)


	protected override void OnTimedOut()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		((QuestBase)this).OnTimedOut();
		TextObject val = new TextObject("{=JTPmw3cb}You couldn't complete the quest in time.", (Dictionary<string, object>)null);
		((QuestBase)this).AddLog(val, false);
	}
}
