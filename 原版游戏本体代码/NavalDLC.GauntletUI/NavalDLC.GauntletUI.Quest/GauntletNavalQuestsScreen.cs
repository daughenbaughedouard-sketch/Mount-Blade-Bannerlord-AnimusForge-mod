using System.Collections.ObjectModel;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.MountAndBlade.View.Screens;

namespace NavalDLC.GauntletUI.Quest;

[GameStateScreen(typeof(QuestsState))]
public class GauntletNavalQuestsScreen : GauntletQuestsScreen
{
	public GauntletNavalQuestsScreen(QuestsState questsState)
		: base(questsState)
	{
	}

	protected override void OnFrameTick(float dt)
	{
		((GauntletQuestsScreen)this).OnFrameTick(dt);
		if (base._dataSource == null)
		{
			return;
		}
		for (int i = 0; i < ((Collection<QuestItemVM>)(object)base._dataSource.ActiveQuestsList).Count; i++)
		{
			QuestItemVM val = ((Collection<QuestItemVM>)(object)base._dataSource.ActiveQuestsList)[i];
			if (val.Quest != null)
			{
				val.IsNavalQuest = val.Quest.SpecialQuestType == "NavalStoryline";
			}
		}
	}
}
