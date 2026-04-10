using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("GetSuppliesTutorialStep2")]
public class BuyingFoodStep2Tutorial : TutorialItemBase
{
	private bool _filterChangedToMisc;

	public BuyingFoodStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "InventoryMicsFilter";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _filterChangedToMisc;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		if (TutorialHelper.BuyingFoodBaseConditions)
		{
			return (int)TutorialHelper.CurrentContext == 2;
		}
		return false;
	}

	public override void OnInventoryFilterChanged(InventoryFilterChangedEvent obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		_filterChangedToMisc = (int)obj.NewFilter == 5;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)2;
	}
}
