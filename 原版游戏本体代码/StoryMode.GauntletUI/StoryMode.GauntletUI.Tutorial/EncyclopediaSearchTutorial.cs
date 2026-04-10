using System;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("EncyclopediaSearchTutorial")]
public class EncyclopediaSearchTutorial : TutorialItemBase
{
	private bool _isActive;

	private bool _isSearchButtonPressed;

	public EncyclopediaSearchTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "EncyclopediaSearchButton";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)9;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		bool isActive = _isActive;
		_isActive = (int)TutorialHelper.CurrentContext == 9;
		if (!isActive && _isActive)
		{
			Game.Current.EventManager.RegisterEvent<OnEncyclopediaSearchActivatedEvent>((Action<OnEncyclopediaSearchActivatedEvent>)OnEncyclopediaSearchBarUsed);
		}
		else if (!_isActive && isActive)
		{
			Game.Current.EventManager.UnregisterEvent<OnEncyclopediaSearchActivatedEvent>((Action<OnEncyclopediaSearchActivatedEvent>)OnEncyclopediaSearchBarUsed);
		}
		return _isActive;
	}

	private void OnEncyclopediaSearchBarUsed(OnEncyclopediaSearchActivatedEvent evnt)
	{
		_isSearchButtonPressed = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (_isActive && _isSearchButtonPressed)
		{
			Game.Current.EventManager.UnregisterEvent<OnEncyclopediaSearchActivatedEvent>((Action<OnEncyclopediaSearchActivatedEvent>)OnEncyclopediaSearchBarUsed);
			return true;
		}
		return false;
	}
}
