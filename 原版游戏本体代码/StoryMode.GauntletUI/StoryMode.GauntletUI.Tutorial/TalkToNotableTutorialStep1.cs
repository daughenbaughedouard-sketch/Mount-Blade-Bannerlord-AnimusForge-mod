using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("TalkToNotableTutorialStep1")]
public class TalkToNotableTutorialStep1 : TutorialItemBase
{
	private bool _wantedCharacterPopupOpened;

	public TalkToNotableTutorialStep1()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)5;
		((TutorialItemBase)this).HighlightedVisualElementID = "ApplicableNotable";
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
		if (!TutorialHelper.IsCharacterPopUpWindowOpen && (int)TutorialHelper.CurrentContext == 4 && TutorialHelper.VillageMenuIsOpen)
		{
			return ((MBObjectBase)Settlement.CurrentSettlement).StringId == "village_ES3_2";
		}
		return false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _wantedCharacterPopupOpened;
	}

	public override void OnCharacterPortraitPopUpOpened(CharacterObject obj)
	{
		int wantedCharacterPopupOpened;
		if (obj == null)
		{
			wantedCharacterPopupOpened = 0;
		}
		else
		{
			Hero heroObject = obj.HeroObject;
			wantedCharacterPopupOpened = ((((heroObject != null) ? new bool?(heroObject.IsHeadman) : ((bool?)null)) == true) ? 1 : 0);
		}
		_wantedCharacterPopupOpened = (byte)wantedCharacterPopupOpened != 0;
	}
}
