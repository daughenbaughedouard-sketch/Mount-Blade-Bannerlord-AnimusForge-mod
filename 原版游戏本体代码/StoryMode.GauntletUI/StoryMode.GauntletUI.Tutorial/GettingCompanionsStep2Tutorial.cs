using System.Collections.Generic;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("GettingCompanionsStep2")]
public class GettingCompanionsStep2Tutorial : TutorialItemBase
{
	private bool _wantedCharacterPopupOpened;

	public GettingCompanionsStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "ApplicapleCompanion";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _wantedCharacterPopupOpened;
	}

	public override void OnCharacterPortraitPopUpOpened(CharacterObject obj)
	{
		_wantedCharacterPopupOpened = obj != null && ((BasicCharacterObject)obj).IsHero && obj.HeroObject.IsWanderer;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		LocationComplex current = LocationComplex.Current;
		Location val = ((current != null) ? current.GetLocationWithId("tavern") : null);
		if (!TutorialHelper.IsCharacterPopUpWindowOpen && (int)TutorialHelper.CurrentContext == 4 && TutorialHelper.PlayerIsInNonEnemyTown && TutorialHelper.BackStreetMenuIsOpen && ((List<Hero>)(object)Clan.PlayerClan.Companions).Count == 0 && Clan.PlayerClan.CompanionLimit > 0 && TutorialHelper.IsThereAvailableCompanionInLocation(val) == true)
		{
			return Hero.MainHero.Gold > TutorialHelper.MinimumGoldForCompanion;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}
}
