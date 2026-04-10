using Helpers;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("ChoosingPerkUpgradesStep1")]
public class ChoosingPerkUpgradesStep1Tutorial : TutorialItemBase
{
	private bool _contextChangedToCharacterScreen;

	public ChoosingPerkUpgradesStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "character_developer";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _contextChangedToCharacterScreen;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if ((TutorialHelper.PlayerIsInAnySettlement || TutorialHelper.PlayerIsSafeOnMap) && PerkHelper.AvailablePerkCountOfHero(Hero.MainHero) > 1)
		{
			return (int)TutorialHelper.CurrentContext == 4;
		}
		return false;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		_contextChangedToCharacterScreen = (int)obj.NewContext == 3;
	}
}
