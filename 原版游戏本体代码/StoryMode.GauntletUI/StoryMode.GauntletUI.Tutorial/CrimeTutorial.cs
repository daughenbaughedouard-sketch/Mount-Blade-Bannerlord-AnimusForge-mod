using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Events;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("CrimeTutorial")]
public class CrimeTutorial : TutorialItemBase
{
	private bool _inspectedCrimeValueItem;

	public CrimeTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)2;
		((TutorialItemBase)this).HighlightedVisualElementID = "CrimeLabel";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override void OnCrimeValueInspectedInSettlementOverlay(CrimeValueInspectedInSettlementOverlayEvent obj)
	{
		_inspectedCrimeValueItem = true;
	}

	public override bool IsConditionsMetForActivation()
	{
		if (TutorialHelper.TownMenuIsOpen)
		{
			IFaction mapFaction = Settlement.CurrentSettlement.MapFaction;
			if (mapFaction == null)
			{
				return false;
			}
			return mapFaction.MainHeroCrimeRating > 0f;
		}
		return false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _inspectedCrimeValueItem;
	}
}
