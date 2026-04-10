using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("InventoryBannerItemTutorial")]
public class InventoryBannerItemTutorial : TutorialItemBase
{
	private bool _inspectedOtherBannerItem;

	public InventoryBannerItemTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)8;
		((TutorialItemBase)this).HighlightedVisualElementID = "InventoryOtherBannerItems";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)2;
	}

	public override void OnInventoryItemInspected(InventoryItemInspectedEvent obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		ItemRosterElement item = obj.Item;
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref item)).EquipmentElement;
		if (((EquipmentElement)(ref equipmentElement)).Item.IsBannerItem && (int)obj.ItemSide == 0)
		{
			_inspectedOtherBannerItem = true;
		}
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (TutorialPhase.Instance.IsCompleted && (int)TutorialHelper.CurrentContext == 2)
		{
			return TutorialHelper.CurrentInventoryScreenIncludesBannerItem;
		}
		return false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _inspectedOtherBannerItem;
	}
}
