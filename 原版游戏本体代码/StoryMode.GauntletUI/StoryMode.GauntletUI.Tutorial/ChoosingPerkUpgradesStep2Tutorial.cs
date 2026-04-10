using Helpers;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper.PerkSelection;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("ChoosingPerkUpgradesStep2")]
public class ChoosingPerkUpgradesStep2Tutorial : TutorialItemBase
{
	private bool _perkPopupOpened;

	public ChoosingPerkUpgradesStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)5;
		((TutorialItemBase)this).HighlightedVisualElementID = "AvailablePerks";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _perkPopupOpened;
	}

	public override void OnPerkSelectionToggle(PerkSelectionToggleEvent obj)
	{
		_perkPopupOpened = true;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if ((TutorialHelper.PlayerIsInAnySettlement || TutorialHelper.PlayerIsSafeOnMap) && PerkHelper.AvailablePerkCountOfHero(Hero.MainHero) > 1)
		{
			return (int)TutorialHelper.CurrentContext == 3;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)3;
	}
}
