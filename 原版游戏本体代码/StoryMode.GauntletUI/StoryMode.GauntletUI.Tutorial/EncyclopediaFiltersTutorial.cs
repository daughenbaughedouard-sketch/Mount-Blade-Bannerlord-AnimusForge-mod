using System;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("EncyclopediaFiltersTutorial")]
public class EncyclopediaFiltersTutorial : TutorialItemBase
{
	private bool _isActive;

	private bool _isAnyFilterSelected;

	public EncyclopediaFiltersTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "EncyclopediaFiltersContainer";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)9;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		bool isActive = _isActive;
		EncyclopediaPages currentEncyclopediaPage = TutorialHelper.CurrentEncyclopediaPage;
		if (currentEncyclopediaPage - 2 <= 6)
		{
			_isActive = true;
		}
		else
		{
			_isActive = false;
		}
		if (!isActive && _isActive)
		{
			Game.Current.EventManager.RegisterEvent<OnEncyclopediaFilterActivatedEvent>((Action<OnEncyclopediaFilterActivatedEvent>)OnFilterClicked);
		}
		else if (!_isActive && isActive)
		{
			Game.Current.EventManager.UnregisterEvent<OnEncyclopediaFilterActivatedEvent>((Action<OnEncyclopediaFilterActivatedEvent>)OnFilterClicked);
		}
		return _isActive;
	}

	private void OnFilterClicked(OnEncyclopediaFilterActivatedEvent evnt)
	{
		_isAnyFilterSelected = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (_isActive && _isAnyFilterSelected)
		{
			Game.Current.EventManager.UnregisterEvent<OnEncyclopediaFilterActivatedEvent>((Action<OnEncyclopediaFilterActivatedEvent>)OnFilterClicked);
			return true;
		}
		return false;
	}
}
