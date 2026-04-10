using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using Storymode.Missions;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("StealthDistractionTutorial")]
public class StealthDistractionTutorial : TutorialItemBase
{
	public StealthDistractionTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}

	public override bool IsConditionsMetForActivation()
	{
		return SneakIntoTheVillaMissionController.IsStealthTutorialReadyForActivation(SneakIntoTheVillaMissionController.MissionState.Distraction);
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (!SneakIntoTheVillaMissionController.IsStealthTutorialReadyForCompletion(SneakIntoTheVillaMissionController.MissionState.Distraction))
		{
			if (SneakIntoTheVillaMissionController.Instance != null)
			{
				return SneakIntoTheVillaMissionController.Instance.IsTargetAgentDistracted();
			}
			return false;
		}
		return true;
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
