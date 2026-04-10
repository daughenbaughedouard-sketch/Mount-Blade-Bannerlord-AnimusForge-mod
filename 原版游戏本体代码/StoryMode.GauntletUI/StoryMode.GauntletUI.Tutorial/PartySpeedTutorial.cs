using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("PartySpeed")]
public class PartySpeedTutorial : TutorialItemBase
{
	private bool _isPlayerInspectedPartySpeed;

	private bool _isActivated;

	public PartySpeedTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "PartySpeedLabel";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _isPlayerInspectedPartySpeed;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override void OnPlayerInspectedPartySpeed(PlayerInspectedPartySpeedEvent obj)
	{
		if (_isActivated)
		{
			_isPlayerInspectedPartySpeed = true;
		}
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_isActivated = (int)TutorialHelper.CurrentContext == 4 && Campaign.Current.CurrentMenuContext == null && (int)MobileParty.MainParty.PartyMoveMode != 0 && MobileParty.MainParty.IsActive && MobileParty.MainParty.Speed < TutorialHelper.MaximumSpeedForPartyForSpeedTutorial && (float)MobileParty.MainParty.InventoryCapacity < MobileParty.MainParty.TotalWeightCarried;
		return _isActivated;
	}
}
