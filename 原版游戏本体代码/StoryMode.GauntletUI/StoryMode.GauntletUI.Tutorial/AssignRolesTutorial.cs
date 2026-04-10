using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("AssignRolesTutorial")]
public class AssignRolesTutorial : TutorialItemBase
{
	private bool _playerAssignedRoleToClanMember;

	public AssignRolesTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)2;
		((TutorialItemBase)this).HighlightedVisualElementID = "RoleAssignmentWidget";
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)6;
	}

	public override void OnClanRoleAssignedThroughClanScreen(ClanRoleAssignedThroughClanScreenEvent obj)
	{
		_playerAssignedRoleToClanMember = true;
	}

	public override bool IsConditionsMetForActivation()
	{
		return TutorialHelper.PlayerHasUnassignedRolesAndMember;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerAssignedRoleToClanMember;
	}
}
