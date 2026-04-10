using System.Collections.Generic;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("GettingCompanionsStep3")]
public class GettingCompanionsStep3Tutorial : TutorialItemBase
{
	private bool _startedTalkingWithCompanion;

	public GettingCompanionsStep3Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "OverlayTalkButton";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _startedTalkingWithCompanion;
	}

	public override void OnPlayerStartTalkFromMenuOverlay(Hero hero)
	{
		_startedTalkingWithCompanion = hero.IsWanderer && !hero.IsPlayerCompanion;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		LocationComplex current = LocationComplex.Current;
		Location val = ((current != null) ? current.GetLocationWithId("tavern") : null);
		if (TutorialHelper.PlayerIsInNonEnemyTown && (int)TutorialHelper.CurrentContext == 4 && TutorialHelper.BackStreetMenuIsOpen && TutorialHelper.IsCharacterPopUpWindowOpen && ((List<Hero>)(object)Clan.PlayerClan.Companions).Count == 0 && Clan.PlayerClan.CompanionLimit > 0 && TutorialHelper.IsThereAvailableCompanionInLocation(val) == true)
		{
			return Hero.MainHero.Gold > TutorialHelper.MinimumGoldForCompanion;
		}
		return false;
	}
}
