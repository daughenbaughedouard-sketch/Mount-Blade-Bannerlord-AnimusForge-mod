using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("CreateArmyStep2")]
public class CreateArmyStep2Tutorial : TutorialItemBase
{
	private bool _playerAddedPartyToArmy;

	public CreateArmyStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)5;
		((TutorialItemBase)this).HighlightedVisualElementID = "GatherArmyPartiesPanel";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerAddedPartyToArmy;
	}

	public override void OnPartyAddedToArmyByPlayer(PartyAddedToArmyByPlayerEvent obj)
	{
		_playerAddedPartyToArmy = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)10;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)TutorialHelper.CurrentContext == 10 && Campaign.Current.CurrentMenuContext == null && Clan.PlayerClan.Kingdom != null)
		{
			return MobileParty.MainParty.Army == null;
		}
		return false;
	}
}
