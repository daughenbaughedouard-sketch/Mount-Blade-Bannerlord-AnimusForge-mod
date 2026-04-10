using StoryMode.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;

namespace StoryMode.GameComponents;

public class StoryModeGenericXpModel : GenericXpModel
{
	public override float GetXpMultiplier(Hero hero)
	{
		if (((hero != null) ? hero.CurrentSettlement : null) != null && hero.CurrentSettlement.IsTrainingField())
		{
			return 0f;
		}
		return ((MBGameModel<GenericXpModel>)this).BaseModel.GetXpMultiplier(hero);
	}
}
