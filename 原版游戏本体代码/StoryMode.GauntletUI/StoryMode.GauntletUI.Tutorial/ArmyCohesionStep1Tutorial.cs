using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("ArmyCohesionStep1")]
public class ArmyCohesionStep1Tutorial : TutorialItemBase
{
	private bool _playerOpenedArmyManagement;

	private bool _playerArmyNeedsCohesion;

	public ArmyCohesionStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = "ArmyOverlayArmyManagementButton";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (_playerArmyNeedsCohesion)
		{
			return _playerOpenedArmyManagement;
		}
		return false;
	}

	public override void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		_playerOpenedArmyManagement = _playerArmyNeedsCohesion && (int)obj.NewContext == 10;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		bool playerArmyNeedsCohesion = _playerArmyNeedsCohesion;
		Army army = MobileParty.MainParty.Army;
		_playerArmyNeedsCohesion = playerArmyNeedsCohesion | (((army != null) ? new float?(army.Cohesion) : ((float?)null)) < TutorialHelper.MaxCohesionForCohesionTutorial);
		if ((int)TutorialHelper.CurrentContext == 4 && MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty)
		{
			return MobileParty.MainParty.Army.Cohesion < TutorialHelper.MaxCohesionForCohesionTutorial;
		}
		return false;
	}
}
