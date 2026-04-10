using Helpers;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("UpgradingTroopsStep3")]
public class UpgradingTroopsStep3Tutorial : TutorialItemBase
{
	private bool _playerUpgradedTroop;

	public UpgradingTroopsStep3Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "UpgradeButton";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerUpgradedTroop;
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
