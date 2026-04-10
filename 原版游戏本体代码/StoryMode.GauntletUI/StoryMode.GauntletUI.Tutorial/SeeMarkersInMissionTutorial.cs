using System.Linq;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Missions.NameMarker;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("SeeMarkersInMissionTutorial")]
public class SeeMarkersInMissionTutorial : TutorialItemBase
{
	private bool _playerEnabledNameMarkers;

	public SeeMarkersInMissionTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)0;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _playerEnabledNameMarkers;
	}

	public override void OnMissionNameMarkerToggled(MissionNameMarkerToggleEvent obj)
	{
		_playerEnabledNameMarkers = obj.NewState;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		string[] source = new string[5] { "center", "lordshall", "tavern", "prison", "village_center" };
		if (TutorialHelper.PlayerIsInAnySettlement && (int)TutorialHelper.CurrentContext == 8 && TutorialHelper.CurrentMissionLocation != null && source.Contains(TutorialHelper.CurrentMissionLocation.StringId))
		{
			return !TutorialHelper.PlayerIsInAConversation;
		}
		return false;
	}
}
