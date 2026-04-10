using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;

namespace StoryMode.GameComponents;

public class StoryModeHeroDeathProbabilityCalculationModel : HeroDeathProbabilityCalculationModel
{
	public override float CalculateHeroDeathProbability(Hero hero)
	{
		if (hero == StoryModeHeroes.ElderBrother && !StoryModeManager.Current.MainStoryLine.IsCompleted)
		{
			return 0f;
		}
		return ((MBGameModel<HeroDeathProbabilityCalculationModel>)this).BaseModel.CalculateHeroDeathProbability(hero);
	}
}
