using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("RecruitmentTutorialStep1")]
public class RecruitmentStep1Tutorial : TutorialItemBase
{
	private bool _recruitmentOpened;

	public RecruitmentStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "storymode_tutorial_village_recruit";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _recruitmentOpened;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		_recruitmentOpened = (int)obj.NewContext == 5;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		if (!TutorialHelper.IsCharacterPopUpWindowOpen && (int)TutorialHelper.CurrentContext == 4 && TutorialHelper.PlayerCanRecruit)
		{
			return !Settlement.CurrentSettlement.MapFaction.IsAtWarWith(MobileParty.MainParty.MapFaction);
		}
		return false;
	}
}
