using System.Linq;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("PressLeaveToReturnFromMissionType1")]
public class PressLeaveToReturnFromMissionTutorial1 : TutorialItemBase
{
	private bool _changedContext;

	public PressLeaveToReturnFromMissionTutorial1()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)5;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _changedContext;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		_changedContext = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		string[] source = new string[6] { "center", "lordshall", "tavern", "prison", "village_center", "arena" };
		if (TutorialHelper.CurrentMissionLocation != null && source.Contains(TutorialHelper.CurrentMissionLocation.StringId) && TutorialHelper.PlayerIsInAnySettlement && !TutorialHelper.PlayerIsInAConversation)
		{
			return (int)TutorialHelper.CurrentContext == 8;
		}
		return false;
	}
}
