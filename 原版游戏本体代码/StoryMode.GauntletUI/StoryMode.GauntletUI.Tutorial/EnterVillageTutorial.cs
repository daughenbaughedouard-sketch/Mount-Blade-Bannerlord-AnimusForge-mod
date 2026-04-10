using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("EnterVillageTutorial")]
public class EnterVillageTutorial : TutorialItemBase
{
	private bool _isEnterOptionSelected;

	private const string _enterGameMenuOptionId = "storymode_tutorial_village_enter";

	public EnterVillageTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "storymode_tutorial_village_enter";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		if (!TutorialHelper.IsCharacterPopUpWindowOpen && (int)TutorialHelper.CurrentContext == 4)
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			return ((currentSettlement != null) ? ((MBObjectBase)currentSettlement).StringId : null) == "village_ES3_2";
		}
		return false;
	}

	public override void OnGameMenuOptionSelected(GameMenuOption obj)
	{
		((TutorialItemBase)this).OnGameMenuOptionSelected(obj);
		_isEnterOptionSelected = obj.IdString == "storymode_tutorial_village_enter";
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _isEnterOptionSelected;
	}
}
