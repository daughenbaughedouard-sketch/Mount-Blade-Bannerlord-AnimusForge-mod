using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using Storymode.Missions;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("StealthCrouchTutorial")]
public class StealthCrouchTutorial : TutorialItemBase
{
	public StealthCrouchTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}

	public override bool IsConditionsMetForActivation()
	{
		return SneakIntoTheVillaMissionController.IsStealthTutorialReadyForActivation(SneakIntoTheVillaMissionController.MissionState.Crouch);
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (Agent.Main != null && Agent.Main.CrouchMode)
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
