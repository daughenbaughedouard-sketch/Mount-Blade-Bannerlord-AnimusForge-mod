using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("OrderOfBattleTutorialStep2")]
public class OrderOfBattleTutorialStep2Tutorial : TutorialItemBase
{
	private bool _playerChangedAFormationType;

	private bool _playerChangedAFormationWeight;

	public OrderOfBattleTutorialStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)2;
		((TutorialItemBase)this).HighlightedVisualElementID = "CreateFormation";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}

	public override bool IsConditionsMetForActivation()
	{
		if (TutorialHelper.IsOrderOfBattleOpenAndReady && TutorialHelper.IsPlayerEncounterLeader)
		{
			return !TutorialHelper.IsNavalMission;
		}
		return false;
	}

	public override void OnOrderOfBattleFormationClassChanged(OrderOfBattleFormationClassChangedEvent obj)
	{
		_playerChangedAFormationType = true;
	}

	public override void OnOrderOfBattleFormationWeightChanged(OrderOfBattleFormationWeightChangedEvent obj)
	{
		_playerChangedAFormationWeight = _playerChangedAFormationType;
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (_playerChangedAFormationType)
		{
			return _playerChangedAFormationWeight;
		}
		return false;
	}
}
