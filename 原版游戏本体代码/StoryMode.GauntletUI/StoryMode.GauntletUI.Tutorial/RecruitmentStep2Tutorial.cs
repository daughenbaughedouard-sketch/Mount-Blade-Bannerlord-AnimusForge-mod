using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("RecruitmentTutorialStep2")]
public class RecruitmentStep2Tutorial : TutorialItemBase
{
	private int _recruitedTroopCount;

	public RecruitmentStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "AvailableTroops";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _recruitedTroopCount >= 4;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)5;
	}

	public override void OnPlayerRecruitedUnit(CharacterObject obj, int count)
	{
		_recruitedTroopCount += count;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		if (TutorialHelper.PlayerCanRecruit)
		{
			return (int)TutorialHelper.CurrentContext == 5;
		}
		return false;
	}
}
