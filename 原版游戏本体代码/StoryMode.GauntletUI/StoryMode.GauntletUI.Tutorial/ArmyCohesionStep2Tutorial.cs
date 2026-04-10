using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("ArmyCohesionStep2")]
public class ArmyCohesionStep2Tutorial : TutorialItemBase
{
	private bool _playerBoostedCohesion;

	public ArmyCohesionStep2Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "ArmyManagementBoostCohesionButton";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerBoostedCohesion;
	}

	public override void OnArmyCohesionByPlayerBoosted(ArmyCohesionBoostedByPlayerEvent obj)
	{
		_playerBoostedCohesion = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)10;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)TutorialHelper.CurrentContext == 10 && Campaign.Current.CurrentMenuContext == null && MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty)
		{
			return MobileParty.MainParty.Army.Cohesion < TutorialHelper.MaxCohesionForCohesionTutorial;
		}
		return false;
	}
}
