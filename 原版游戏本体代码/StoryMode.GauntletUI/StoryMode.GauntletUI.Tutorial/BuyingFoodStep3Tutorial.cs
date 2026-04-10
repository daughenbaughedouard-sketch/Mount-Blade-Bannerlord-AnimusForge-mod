using System.Collections.Generic;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("GetSuppliesTutorialStep3")]
public class BuyingFoodStep3Tutorial : TutorialItemBase
{
	private int _purchasedFoodCount;

	public BuyingFoodStep3Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "TransferButtonOnlyFood";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _purchasedFoodCount >= 2;
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

	public override void OnPlayerInventoryExchange(List<(ItemRosterElement, int)> purchasedItems, List<(ItemRosterElement, int)> soldItems, bool isTrading)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < purchasedItems.Count; i++)
		{
			(ItemRosterElement, int) tuple = purchasedItems[i];
			EquipmentElement equipmentElement = ((ItemRosterElement)(ref tuple.Item1)).EquipmentElement;
			if (((EquipmentElement)(ref equipmentElement)).Item == DefaultItems.Grain)
			{
				_purchasedFoodCount += ((ItemRosterElement)(ref tuple.Item1)).Amount;
			}
		}
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)2;
	}
}
