using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("CreateArmyStep3")]
public class CreateArmyStep3Tutorial : TutorialItemBase
{
	private bool _playerClosedArmyManagement;

	public CreateArmyStep3Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerClosedArmyManagement;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		_playerClosedArmyManagement = (int)obj.NewContext != 10;
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
