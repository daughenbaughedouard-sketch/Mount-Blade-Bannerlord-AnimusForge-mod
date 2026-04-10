using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Decisions;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("KingdomDecisionVotingTutorial")]
public class KingdomDecisionVotingTutorial : TutorialItemBase
{
	private bool _playerSelectedAnOption;

	public KingdomDecisionVotingTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)0;
		((TutorialItemBase)this).HighlightedVisualElementID = "DecisionOptions";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)7;
	}

	public override void OnPlayerSelectedAKingdomDecisionOption(PlayerSelectedAKingdomDecisionOptionEvent obj)
	{
		_playerSelectedAnOption = true;
	}

	public override bool IsConditionsMetForActivation()
	{
		return TutorialHelper.IsKingdomDecisionPanelActiveAndHasOptions;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerSelectedAnOption;
	}
}
