using System;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("EncyclopediaTrackTutorial")]
public class EncyclopediaTrackTutorial : TutorialItemBase
{
	private bool _isActive;

	private bool _usedTrackFromEncyclopedia;

	public EncyclopediaTrackTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "EncyclopediaItemTrackButton";
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
		_isActive = (int)TutorialHelper.CurrentEncyclopediaPage == 10;
		if (!isActive && _isActive)
		{
			Game.Current.EventManager.RegisterEvent<PlayerToggleTrackSettlementFromEncyclopediaEvent>((Action<PlayerToggleTrackSettlementFromEncyclopediaEvent>)OnTrackToggledFromEncyclopedia);
		}
		else if (!_isActive && isActive)
		{
			Game.Current.EventManager.UnregisterEvent<PlayerToggleTrackSettlementFromEncyclopediaEvent>((Action<PlayerToggleTrackSettlementFromEncyclopediaEvent>)OnTrackToggledFromEncyclopedia);
		}
		return _isActive;
	}

	public override bool IsConditionsMetForCompletion()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		if (_isActive)
		{
			bool flag = false;
			if (_isActive)
			{
				if ((int)TutorialHelper.CurrentContext != 9)
				{
					flag = true;
				}
				if ((int)TutorialHelper.CurrentEncyclopediaPage != 13 && (int)TutorialHelper.CurrentEncyclopediaPage != 10)
				{
					flag = true;
				}
				if (_usedTrackFromEncyclopedia)
				{
					flag = true;
				}
			}
			if (flag)
			{
				Game.Current.EventManager.UnregisterEvent<PlayerToggleTrackSettlementFromEncyclopediaEvent>((Action<PlayerToggleTrackSettlementFromEncyclopediaEvent>)OnTrackToggledFromEncyclopedia);
				return true;
			}
		}
		return false;
	}

	private void OnTrackToggledFromEncyclopedia(PlayerToggleTrackSettlementFromEncyclopediaEvent callback)
	{
		_usedTrackFromEncyclopedia = true;
	}
}
