using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("EncyclopediaHomeTutorial")]
public class EncyclopediaHomeTutorial : TutorialItemBase
{
	private bool _isActive;

	public EncyclopediaHomeTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)9;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		_isActive = (int)GauntletTutorialSystem.Current.CurrentEncyclopediaPageContext == 1;
		return _isActive;
	}

	public override bool IsConditionsMetForCompletion()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		if (_isActive)
		{
			return (int)GauntletTutorialSystem.Current.CurrentEncyclopediaPageContext != 1;
		}
		return false;
	}
}
