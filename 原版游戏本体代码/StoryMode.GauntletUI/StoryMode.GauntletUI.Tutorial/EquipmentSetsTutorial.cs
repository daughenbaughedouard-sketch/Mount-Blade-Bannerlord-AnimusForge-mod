using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("EquipmentSets")]
public class EquipmentSetsTutorial : TutorialItemBase
{
	private bool _playerFilteredToDifferentEquipment;

	public EquipmentSetsTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "EquipmentSetFilters";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerFilteredToDifferentEquipment;
	}

	public override void OnInventoryEquipmentTypeChange(InventoryEquipmentTypeChangedEvent obj)
	{
		_playerFilteredToDifferentEquipment = !obj.IsCurrentlyWarSet;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)2;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (TutorialPhase.Instance.IsCompleted)
		{
			return (int)TutorialHelper.CurrentContext == 2;
		}
		return false;
	}
}
