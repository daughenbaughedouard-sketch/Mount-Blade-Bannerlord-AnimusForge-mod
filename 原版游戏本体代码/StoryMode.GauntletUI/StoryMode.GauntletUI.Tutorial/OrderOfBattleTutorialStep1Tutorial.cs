using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("OrderOfBattleTutorialStep1")]
public class OrderOfBattleTutorialStep1Tutorial : TutorialItemBase
{
	private bool _playerAssignedACaptainToFormationInOoB;

	public OrderOfBattleTutorialStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)8;
		((TutorialItemBase)this).HighlightedVisualElementID = "AssignCaptain";
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

	public override void OnOrderOfBattleHeroAssignedToFormation(OrderOfBattleHeroAssignedToFormationEvent obj)
	{
		_playerAssignedACaptainToFormationInOoB = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerAssignedACaptainToFormationInOoB;
	}
}
