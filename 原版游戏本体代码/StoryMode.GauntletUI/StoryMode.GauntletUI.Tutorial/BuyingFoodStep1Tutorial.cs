using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("GetSuppliesTutorialStep1")]
public class BuyingFoodStep1Tutorial : TutorialItemBase
{
	private bool _contextChangedToInventory;

	public BuyingFoodStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "storymode_tutorial_village_buy";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _contextChangedToInventory;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (!TutorialHelper.IsCharacterPopUpWindowOpen && TutorialHelper.BuyingFoodBaseConditions)
		{
			return (int)TutorialHelper.CurrentContext == 4;
		}
		return false;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		_contextChangedToInventory = (int)obj.NewContext == 2;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}
}
