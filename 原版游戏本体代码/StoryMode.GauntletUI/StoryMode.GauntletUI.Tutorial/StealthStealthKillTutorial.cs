using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using Storymode.Missions;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("StealthStealthKillTutorial")]
public class StealthStealthKillTutorial : TutorialItemBase
{
	public StealthStealthKillTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}

	public override bool IsConditionsMetForActivation()
	{
		return SneakIntoTheVillaMissionController.IsStealthTutorialReadyForActivation(SneakIntoTheVillaMissionController.MissionState.StealthKill);
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (!SneakIntoTheVillaMissionController.IsStealthTutorialReadyForCompletion(SneakIntoTheVillaMissionController.MissionState.StealthKill))
		{
			if (SneakIntoTheVillaMissionController.Instance != null)
			{
				return SneakIntoTheVillaMissionController.Instance.IsTargetAgentKilled();
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
