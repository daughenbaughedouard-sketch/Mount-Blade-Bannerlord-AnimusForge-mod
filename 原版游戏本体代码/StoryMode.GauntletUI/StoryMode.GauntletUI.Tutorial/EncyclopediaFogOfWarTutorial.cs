using System;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("EncyclopediaFogOfWarTutorial")]
public class EncyclopediaFogOfWarTutorial : TutorialItemBase
{
	private EncyclopediaPages _activatedPage;

	private bool _registeredEvents;

	private bool _lastActiveState;

	private bool _isActive;

	public EncyclopediaFogOfWarTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		if (!_registeredEvents && (int)TutorialHelper.CurrentContext == 9)
		{
			Game.Current.EventManager.RegisterEvent<EncyclopediaPageChangedEvent>((Action<EncyclopediaPageChangedEvent>)OnLimitedInformationPageOpened);
			_registeredEvents = true;
		}
		else if (_registeredEvents && (int)TutorialHelper.CurrentContext != 9)
		{
			Game.Current.EventManager.UnregisterEvent<EncyclopediaPageChangedEvent>((Action<EncyclopediaPageChangedEvent>)OnLimitedInformationPageOpened);
			_registeredEvents = false;
		}
		return (TutorialContexts)9;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent evnt)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		((TutorialItemBase)this).OnTutorialContextChanged(evnt);
		if (_registeredEvents && (int)evnt.NewContext != 9)
		{
			Game.Current.EventManager.UnregisterEvent<EncyclopediaPageChangedEvent>((Action<EncyclopediaPageChangedEvent>)OnLimitedInformationPageOpened);
			_registeredEvents = false;
		}
	}

	public override bool IsConditionsMetForActivation()
	{
		if (!_registeredEvents)
		{
			Game.Current.EventManager.RegisterEvent<EncyclopediaPageChangedEvent>((Action<EncyclopediaPageChangedEvent>)OnLimitedInformationPageOpened);
			_registeredEvents = true;
		}
		return _isActive;
	}

	public override bool IsConditionsMetForCompletion()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!_lastActiveState && _isActive)
		{
			_activatedPage = TutorialHelper.CurrentEncyclopediaPage;
		}
		if (_lastActiveState && _isActive && _activatedPage != TutorialHelper.CurrentEncyclopediaPage)
		{
			Game.Current.EventManager.UnregisterEvent<EncyclopediaPageChangedEvent>((Action<EncyclopediaPageChangedEvent>)OnLimitedInformationPageOpened);
			return true;
		}
		_lastActiveState = _isActive;
		return false;
	}

	private void OnLimitedInformationPageOpened(EncyclopediaPageChangedEvent evnt)
	{
		if (evnt.NewPageHasHiddenInformation)
		{
			_isActive = true;
		}
	}
}
