using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("UpgradingTroopsStep1")]
public class UpgradingTroopsStep1Tutorial : TutorialItemBase
{
	private bool _partyScreenOpened;

	private bool _playerUpgradedTroop;

	public UpgradingTroopsStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "PartyButton";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (!_partyScreenOpened)
		{
			return _playerUpgradedTroop;
		}
		return true;
	}

	public override void OnPlayerUpgradeTroop(CharacterObject arg1, CharacterObject arg2, int arg3)
	{
		_playerUpgradedTroop = true;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		_partyScreenOpened = (int)obj.NewContext == 1;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (Hero.MainHero.Gold < 100 || (int)TutorialHelper.CurrentContext != 4 || TutorialHelper.PlayerIsInAnySettlement || !TutorialHelper.PlayerIsSafeOnMap)
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
		return (TutorialContexts)4;
	}
}
