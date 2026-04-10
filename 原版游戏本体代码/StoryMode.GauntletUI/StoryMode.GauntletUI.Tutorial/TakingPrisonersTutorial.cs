using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("TakeAndRescuePrisonerTutorial")]
public class TakingPrisonersTutorial : TutorialItemBase
{
	private bool _playerMovedOtherPrisonerTroop;

	public TakingPrisonersTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)2;
		((TutorialItemBase)this).HighlightedVisualElementID = "TransferButtonOnlyOtherPrisoners";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)1;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		GameState activeState = GameStateManager.Current.ActiveState;
		PartyState val;
		if ((val = (PartyState)(object)((activeState is PartyState) ? activeState : null)) != null && (int)val.PartyScreenMode == 2 && val.PartyScreenLogic.PrisonerRosters[0].Count > 0)
		{
			return (int)TutorialHelper.CurrentContext == 2;
		}
		return false;
	}

	public override void OnPlayerMoveTroop(PlayerMoveTroopEvent obj)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		((TutorialItemBase)this).OnPlayerMoveTroop(obj);
		if (obj.IsPrisoner && (int)obj.ToSide == 1 && obj.Amount > 0)
		{
			_playerMovedOtherPrisonerTroop = true;
		}
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerMovedOtherPrisonerTroop;
	}
}
