using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

public abstract class EncyclopediaPageTutorialBase : TutorialItemBase
{
	private bool _isActive;

	private readonly EncyclopediaPages _activationPage;

	private readonly EncyclopediaPages _alternateActivationPage;

	private EncyclopediaPages _lastActivatedPage;

	public EncyclopediaPageTutorialBase(EncyclopediaPages activationPage, EncyclopediaPages alternateActivationPage)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "";
		((TutorialItemBase)this).MouseRequired = false;
		_activationPage = activationPage;
		_alternateActivationPage = alternateActivationPage;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)9;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		EncyclopediaPages currentEncyclopediaPageContext = GauntletTutorialSystem.Current.CurrentEncyclopediaPageContext;
		bool isActive = _isActive;
		_isActive = currentEncyclopediaPageContext == _activationPage || currentEncyclopediaPageContext == _alternateActivationPage;
		if (!isActive && _isActive)
		{
			_lastActivatedPage = currentEncyclopediaPageContext;
		}
		return _isActive;
	}

	public override bool IsConditionsMetForCompletion()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		if (_isActive)
		{
			EncyclopediaPages currentEncyclopediaPageContext = GauntletTutorialSystem.Current.CurrentEncyclopediaPageContext;
			if (_lastActivatedPage == _alternateActivationPage)
			{
				return currentEncyclopediaPageContext != _alternateActivationPage;
			}
			if ((int)currentEncyclopediaPageContext != 10)
			{
				return (int)currentEncyclopediaPageContext != 2;
			}
			return false;
		}
		return false;
	}
}
