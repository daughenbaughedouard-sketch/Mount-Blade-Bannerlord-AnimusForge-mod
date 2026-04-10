using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("TalkToNotableTutorialStep2")]
public class TalkToNotableTutorialStep2 : TutorialItemBase
{
	private bool _hasTalkedToNotable;

	public TalkToNotableTutorialStep2()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "OverlayTalkButton";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _hasTalkedToNotable;
	}

	public override void OnPlayerStartTalkFromMenuOverlay(Hero hero)
	{
		_hasTalkedToNotable = hero.IsHeadman;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)TutorialHelper.CurrentContext == 4)
		{
			return TutorialHelper.IsCharacterPopUpWindowOpen;
		}
		return false;
	}
}
