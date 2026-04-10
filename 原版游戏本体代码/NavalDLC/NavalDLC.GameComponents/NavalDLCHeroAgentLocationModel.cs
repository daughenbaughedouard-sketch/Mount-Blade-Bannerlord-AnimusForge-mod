using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCHeroAgentLocationModel : HeroAgentLocationModel
{
	public override Location GetLocationForHero(Hero hero, Settlement settlement, out HeroLocationDetail heroSpawnDetail)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		if (NavalStorylineData.IsNavalStorylineHero(hero))
		{
			if (NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest5) && hero == NavalStorylineData.Gunnar && settlement.IsVillage && (int)hero.Occupation == 31)
			{
				heroSpawnDetail = (HeroLocationDetail)6;
				return settlement.LocationComplex.GetLocationWithId("village_center");
			}
			heroSpawnDetail = (HeroLocationDetail)2;
			if (NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.None) && hero == NavalStorylineData.Purig && !hero.IsDead)
			{
				return settlement.LocationComplex.GetLocationWithId("tavern");
			}
			if (hero == NavalStorylineData.Gunnar && NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.None))
			{
				return null;
			}
			return settlement.LocationComplex.GetLocationWithId("port");
		}
		if (NavalStorylineData.IsNavalStoryLineActive())
		{
			heroSpawnDetail = (HeroLocationDetail)0;
			return null;
		}
		return ((MBGameModel<HeroAgentLocationModel>)this).BaseModel.GetLocationForHero(hero, settlement, ref heroSpawnDetail);
	}

	public override bool WillBeListedInOverlay(LocationCharacter locationCharacter)
	{
		if (NavalStorylineData.IsNavalStoryLineActive() && ((BasicCharacterObject)locationCharacter.Character).IsHero && NavalStorylineData.IsNavalStorylineHero(locationCharacter.Character.HeroObject))
		{
			return true;
		}
		return ((MBGameModel<HeroAgentLocationModel>)this).BaseModel.WillBeListedInOverlay(locationCharacter);
	}
}
