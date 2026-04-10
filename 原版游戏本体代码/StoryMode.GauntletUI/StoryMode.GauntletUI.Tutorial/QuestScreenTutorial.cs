using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("GetQuestTutorial")]
public class QuestScreenTutorial : TutorialItemBase
{
	private bool _contextChangedToQuestsScreen;

	public QuestScreenTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "QuestsButton";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		if (Mission.Current == null)
		{
			return (int)TutorialHelper.CurrentContext == 11;
		}
		return false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _contextChangedToQuestsScreen;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		_contextChangedToQuestsScreen = (int)obj.NewContext == 11;
	}
}
