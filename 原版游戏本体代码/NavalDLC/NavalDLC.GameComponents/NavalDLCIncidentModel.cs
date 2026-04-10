using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCIncidentModel : IncidentModel
{
	public override float GetIncidentTriggerGlobalProbability()
	{
		if (NavalStorylineData.IsNavalStoryLineActive())
		{
			return 0f;
		}
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetIncidentTriggerGlobalProbability();
	}

	public override float GetIncidentTriggerProbabilityDuringSiege()
	{
		if (NavalStorylineData.IsNavalStoryLineActive())
		{
			return 0f;
		}
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetIncidentTriggerProbabilityDuringSiege();
	}

	public override float GetIncidentTriggerProbabilityDuringWait()
	{
		if (NavalStorylineData.IsNavalStoryLineActive())
		{
			return 0f;
		}
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetIncidentTriggerProbabilityDuringWait();
	}

	public override CampaignTime GetMaxGlobalCooldownTime()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetMaxGlobalCooldownTime();
	}

	public override CampaignTime GetMinGlobalCooldownTime()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<IncidentModel>)this).BaseModel.GetMinGlobalCooldownTime();
	}
}
