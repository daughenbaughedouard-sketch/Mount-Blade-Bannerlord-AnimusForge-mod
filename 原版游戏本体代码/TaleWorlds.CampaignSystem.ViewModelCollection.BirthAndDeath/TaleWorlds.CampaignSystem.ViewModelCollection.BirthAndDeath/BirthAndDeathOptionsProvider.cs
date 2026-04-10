using System;
using System.Collections.Generic;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.BirthAndDeath;

public class BirthAndDeathOptionsProvider : ICampaignOptionProvider
{
	public IEnumerable<ICampaignOptionData> GetGameplayCampaignOptions()
	{
		yield return (ICampaignOptionData)new BooleanCampaignOptionData("IsLifeDeathCycleEnabled", 890, (CampaignOptionEnableState)2, (Func<float>)(() => (!CampaignOptions.IsLifeDeathCycleDisabled) ? 1f : 0f), (Action<float>)delegate(float value)
		{
			CampaignOptions.IsLifeDeathCycleDisabled = value == 0f;
		}, (Func<CampaignOptionDisableStatus>)null, false, (Func<float, CampaignOptionsDifficultyPresets>)null, (Func<CampaignOptionsDifficultyPresets, float>)null);
	}

	public IEnumerable<ICampaignOptionData> GetCharacterCreationCampaignOptions()
	{
		yield return (ICampaignOptionData)new BooleanCampaignOptionData("IsLifeDeathCycleEnabled", 890, (CampaignOptionEnableState)1, (Func<float>)(() => (!CampaignOptions.IsLifeDeathCycleDisabled) ? 1f : 0f), (Action<float>)delegate(float value)
		{
			CampaignOptions.IsLifeDeathCycleDisabled = value == 0f;
		}, (Func<CampaignOptionDisableStatus>)null, false, (Func<float, CampaignOptionsDifficultyPresets>)null, (Func<CampaignOptionsDifficultyPresets, float>)null);
	}
}
