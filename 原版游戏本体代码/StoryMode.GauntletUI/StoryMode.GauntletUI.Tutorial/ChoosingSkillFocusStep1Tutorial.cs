using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("ChoosingSkillFocusStep1")]
public class ChoosingSkillFocusStep1Tutorial : TutorialItemBase
{
	private bool _characterWindowOpened;

	public ChoosingSkillFocusStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "character_developer";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _characterWindowOpened;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		_characterWindowOpened = (int)obj.NewContext == 3;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		if (Settlement.CurrentSettlement == null && Hero.MainHero.HeroDeveloper.UnspentFocusPoints > 1 && (TutorialHelper.PlayerIsInAnySettlement || TutorialHelper.PlayerIsSafeOnMap))
		{
			return (int)TutorialHelper.CurrentContext == 4;
		}
		return false;
	}
}
