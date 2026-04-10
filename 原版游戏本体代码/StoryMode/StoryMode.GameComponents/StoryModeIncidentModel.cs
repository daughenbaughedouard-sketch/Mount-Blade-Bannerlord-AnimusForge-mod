using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;

namespace StoryMode.GameComponents;

public class StoryModeIncidentModel : IncidentModel
{
	public override CampaignTime GetMinGlobalCooldownTime()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetMinGlobalCooldownTime();
	}

	public override CampaignTime GetMaxGlobalCooldownTime()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetMaxGlobalCooldownTime();
	}

	public override float GetIncidentTriggerGlobalProbability()
	{
		if (!TutorialPhase.Instance.IsCompleted)
		{
			return 0f;
		}
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetIncidentTriggerGlobalProbability();
	}

	public override float GetIncidentTriggerProbabilityDuringSiege()
	{
		if (!TutorialPhase.Instance.IsCompleted)
		{
			return 0f;
		}
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetIncidentTriggerProbabilityDuringSiege();
	}

	public override float GetIncidentTriggerProbabilityDuringWait()
	{
		if (!TutorialPhase.Instance.IsCompleted)
		{
			return 0f;
		}
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetIncidentTriggerProbabilityDuringWait();
	}
}
