using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("ChoosingSkillFocusStep2")]
public class ChoosingSkillFocusStep2Tutorial : TutorialItemBase
{
	private bool _focusAdded;

	public ChoosingSkillFocusStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)5;
		((TutorialItemBase)this).HighlightedVisualElementID = "AddFocusButton";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _focusAdded;
	}

	public override void OnFocusAddedByPlayer(FocusAddedByPlayerEvent obj)
	{
		_focusAdded = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)3;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if (Hero.MainHero.HeroDeveloper.UnspentFocusPoints > 1)
		{
			return (int)TutorialHelper.CurrentContext == 3;
		}
		return false;
	}
}
