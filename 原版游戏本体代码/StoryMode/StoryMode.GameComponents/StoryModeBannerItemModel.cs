using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents;

public class StoryModeBannerItemModel : BannerItemModel
{
	public override IEnumerable<ItemObject> GetPossibleRewardBannerItems()
	{
		if (!StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted)
		{
			return new List<ItemObject>();
		}
		return LinQuick.WhereQ<ItemObject>(((MBGameModel<BannerItemModel>)this).BaseModel.GetPossibleRewardBannerItems(), (Func<ItemObject, bool>)((ItemObject i) => !IsItemDragonBanner(i)));
	}

	public override bool CanBannerBeUpdated(ItemObject item)
	{
		if (IsItemDragonBanner(item))
		{
			return false;
		}
		return ((MBGameModel<BannerItemModel>)this).BaseModel.CanBannerBeUpdated(item);
	}

	private bool IsItemDragonBanner(ItemObject item)
	{
		if (!(((MBObjectBase)item).StringId == "dragon_banner") && !(((MBObjectBase)item).StringId == "dragon_banner_center") && !(((MBObjectBase)item).StringId == "dragon_banner_dragonhead"))
		{
			return ((MBObjectBase)item).StringId == "dragon_banner_handle";
		}
		return true;
	}

	public override IEnumerable<ItemObject> GetPossibleRewardBannerItemsForHero(Hero hero)
	{
		return LinQuick.WhereQ<ItemObject>(((MBGameModel<BannerItemModel>)this).BaseModel.GetPossibleRewardBannerItemsForHero(hero), (Func<ItemObject, bool>)((ItemObject b) => !IsItemDragonBanner(b)));
	}

	public override int GetBannerItemLevelForHero(Hero hero)
	{
		return ((MBGameModel<BannerItemModel>)this).BaseModel.GetBannerItemLevelForHero(hero);
	}
}
