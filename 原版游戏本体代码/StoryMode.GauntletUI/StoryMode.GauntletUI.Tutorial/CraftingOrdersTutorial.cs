using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Tutorial;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("CraftingOrdersTutorial")]
public class CraftingOrdersTutorial : TutorialItemBase
{
	private bool _craftingCategorySelectionOpened;

	private bool _craftingOrderSelectionOpened;

	private bool _craftingOrderResultOpened;

	private bool _craftingOrderTabOpened;

	public CraftingOrdersTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)2;
		((TutorialItemBase)this).HighlightedVisualElementID = "CraftingOrdersButton";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)14;
	}

	public override void OnCraftingWeaponClassSelectionOpened(CraftingWeaponClassSelectionOpenedEvent obj)
	{
		_craftingCategorySelectionOpened = obj.IsOpen;
	}

	public override void OnCraftingOrderTabOpened(CraftingOrderTabOpenedEvent obj)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		_craftingOrderTabOpened = obj.IsOpen;
		if (_craftingOrderTabOpened)
		{
			((TutorialItemBase)this).HighlightedVisualElementID = "OrderSelectionButton";
		}
		else
		{
			((TutorialItemBase)this).HighlightedVisualElementID = "CraftingOrdersButton";
		}
		Game current = Game.Current;
		if (current != null)
		{
			current.EventManager.TriggerEvent<TutorialNotificationElementChangeEvent>(new TutorialNotificationElementChangeEvent(((TutorialItemBase)this).HighlightedVisualElementID));
		}
	}

	public override void OnCraftingOrderSelectionOpened(CraftingOrderSelectionOpenedEvent obj)
	{
		_craftingOrderSelectionOpened = obj.IsOpen;
	}

	public override void OnCraftingOnWeaponResultPopupOpened(CraftingWeaponResultPopupToggledEvent obj)
	{
		_craftingOrderResultOpened = obj.IsOpen;
	}

	public override bool IsConditionsMetForActivation()
	{
		if (!_craftingCategorySelectionOpened && !_craftingOrderResultOpened)
		{
			return TutorialHelper.IsCurrentTownHaveDoableCraftingOrder;
		}
		return false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _craftingOrderSelectionOpened;
	}
}
