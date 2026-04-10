using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("NavigateOnMapTutorialStep2")]
public class NavigateOnMapTutorialStep2 : TutorialItemBase
{
	private const string TargetQuestVillage = "village_ES3_2";

	public NavigateOnMapTutorialStep2()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)5;
		((TutorialItemBase)this).HighlightedVisualElementID = "village_ES3_2";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		return (int)TutorialHelper.CurrentContext == 4;
	}

	public override bool IsConditionsMetForCompletion()
	{
		MobileParty mainParty = MobileParty.MainParty;
		object obj;
		if (mainParty == null)
		{
			obj = null;
		}
		else
		{
			Settlement targetSettlement = mainParty.TargetSettlement;
			obj = ((targetSettlement != null) ? ((MBObjectBase)targetSettlement).StringId : null);
		}
		return (string?)obj == "village_ES3_2";
	}
}
