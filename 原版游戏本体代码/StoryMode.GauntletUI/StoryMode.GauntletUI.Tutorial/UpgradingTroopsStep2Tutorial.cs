using Helpers;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("UpgradingTroopsStep2")]
public class UpgradingTroopsStep2Tutorial : TutorialItemBase
{
	private bool _playerUpgradedTroop;

	private bool _playerOpenedUpgradePopup;

	public UpgradingTroopsStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)0;
		((TutorialItemBase)this).HighlightedVisualElementID = "UpgradePopupButton";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (!_playerUpgradedTroop)
		{
			return _playerOpenedUpgradePopup;
		}
		return true;
	}

	public override void OnPlayerToggledUpgradePopup(PlayerToggledUpgradePopupEvent obj)
	{
		if (obj.IsOpened)
		{
			_playerOpenedUpgradePopup = true;
		}
	}

	public override void OnPlayerUpgradeTroop(CharacterObject arg1, CharacterObject arg2, int arg3)
	{
		_playerUpgradedTroop = true;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (Hero.MainHero.Gold <= 100 || (int)TutorialHelper.CurrentContext != 1)
		{
			return false;
		}
		PartyState activePartyState = PartyScreenHelper.GetActivePartyState();
		if (activePartyState != null && (int)activePartyState.PartyScreenMode != 0)
		{
			return false;
		}
		if (!TutorialHelper.AreTroopUpgradesDisabled)
		{
			return TutorialHelper.PlayerHasAnyUpgradeableTroop;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)1;
	}
}
