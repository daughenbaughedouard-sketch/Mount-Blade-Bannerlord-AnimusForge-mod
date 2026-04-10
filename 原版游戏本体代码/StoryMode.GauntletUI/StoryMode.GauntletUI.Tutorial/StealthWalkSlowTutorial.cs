using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using Storymode.Missions;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("StealthWalkSlowTutorial")]
public class StealthWalkSlowTutorial : TutorialItemBase
{
	public StealthWalkSlowTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}

	public override bool IsConditionsMetForActivation()
	{
		return SneakIntoTheVillaMissionController.IsStealthTutorialReadyForActivation(SneakIntoTheVillaMissionController.MissionState.WalkSlow);
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (Agent.Main != null && Agent.Main.WalkMode)
		{
			return SneakIntoTheVillaMissionController.Instance != null;
		}
		return false;
	}

	public override bool IsConditionsMetForVisibility()
	{
		if (((TutorialItemBase)this).IsConditionsMetForVisibility())
		{
			return SneakIntoTheVillaMissionController.Instance != null;
		}
		return false;
	}
}
