using System.Collections.Generic;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("GettingCompanionsStep1")]
public class GettingCompanionsStep1Tutorial : TutorialItemBase
{
	private bool _wantedGameMenuOpened;

	public GettingCompanionsStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "town_backstreet";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _wantedGameMenuOpened;
	}

	public override void OnGameMenuOpened(MenuCallbackArgs obj)
	{
		((TutorialItemBase)this).OnGameMenuOpened(obj);
		_wantedGameMenuOpened = obj.MenuContext.GameMenu.StringId == "town_backstreet";
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Invalid comparison between Unknown and I4
		LocationComplex current = LocationComplex.Current;
		Location val = ((current != null) ? current.GetLocationWithId("tavern") : null);
		if (!TutorialHelper.IsCharacterPopUpWindowOpen && TutorialHelper.PlayerIsInNonEnemyTown && TutorialHelper.TownMenuIsOpen && ((List<Hero>)(object)Clan.PlayerClan.Companions).Count == 0 && Clan.PlayerClan.CompanionLimit > 0 && TutorialHelper.IsThereAvailableCompanionInLocation(val) == true && Hero.MainHero.Gold > TutorialHelper.MinimumGoldForCompanion)
		{
			return (int)TutorialHelper.CurrentContext == 4;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}
}
