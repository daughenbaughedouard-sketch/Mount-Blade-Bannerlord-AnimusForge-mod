using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.GameComponents;

public class NavalDLCCaravanModel : CaravanModel
{
	public override int MaxNumberOfItemsToBuyFromSingleCategory => ((MBGameModel<CaravanModel>)this).BaseModel.MaxNumberOfItemsToBuyFromSingleCategory;

	public override bool CanHeroCreateCaravan(Hero hero)
	{
		return ((MBGameModel<CaravanModel>)this).BaseModel.CanHeroCreateCaravan(hero);
	}

	public override int GetCaravanFormingCost(bool eliteCaravan, bool navalCaravan)
	{
		if (navalCaravan)
		{
			int num = (eliteCaravan ? 45000 : 30000);
			if (CharacterObject.PlayerCharacter.Culture.HasFeat(DefaultCulturalFeats.AseraiTraderFeat))
			{
				return MathF.Round((float)num * DefaultCulturalFeats.AseraiTraderFeat.EffectBonus);
			}
			return num;
		}
		return ((MBGameModel<CaravanModel>)this).BaseModel.GetCaravanFormingCost(eliteCaravan, navalCaravan);
	}

	public override float GetEliteCaravanSpawnChance(Hero hero)
	{
		return ((MBGameModel<CaravanModel>)this).BaseModel.GetEliteCaravanSpawnChance(hero);
	}

	public override int GetInitialTradeGold(Hero owner, bool navalCaravan, bool largeCaravan)
	{
		if (navalCaravan)
		{
			int num = 30000;
			int num2 = ((owner == Hero.MainHero) ? 5000 : 0);
			if (largeCaravan)
			{
				num = 40000;
			}
			return num + num2;
		}
		return ((MBGameModel<CaravanModel>)this).BaseModel.GetInitialTradeGold(owner, navalCaravan, largeCaravan);
	}

	public override int GetMaxGoldToSpendOnOneItemCategory(MobileParty caravan, ItemCategory itemCategory)
	{
		if (caravan.HasNavalNavigationCapability)
		{
			return 3000;
		}
		return ((MBGameModel<CaravanModel>)this).BaseModel.GetMaxGoldToSpendOnOneItemCategory(caravan, itemCategory);
	}

	public override int GetPowerChangeAfterCaravanCreation(Hero hero, MobileParty caravanParty)
	{
		return ((MBGameModel<CaravanModel>)this).BaseModel.GetPowerChangeAfterCaravanCreation(hero, caravanParty);
	}
}
