using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("RansomingPrisonersStep2")]
public class RansomingPrisonersStep2Tutorial : TutorialItemBase
{
	private bool _sellPrisonersOptionsSelected;

	public RansomingPrisonersStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "sell_all_prisoners";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _sellPrisonersOptionsSelected;
	}

	public override void OnGameMenuOptionSelected(GameMenuOption obj)
	{
		_sellPrisonersOptionsSelected = obj.IdString == "sell_all_prisoners";
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		if (!TutorialHelper.IsCharacterPopUpWindowOpen && (int)TutorialHelper.CurrentContext == 4 && TutorialHelper.PlayerIsInNonEnemyTown && TutorialHelper.BackStreetMenuIsOpen && !Hero.MainHero.IsPrisoner)
		{
			return MobileParty.MainParty.PrisonRoster.TotalManCount > 0;
		}
		return false;
	}
}
