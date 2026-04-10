using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;
using TaleWorlds.Localization;

namespace NavalDLC.Encyclopedia.Pages;

[OverrideEncyclopediaModel(new Type[] { typeof(CharacterObject) })]
public class NavalEncyclopediaUnityPage : DefaultEncyclopediaUnitPage
{
	protected override List<EncyclopediaFilterItem> GetTypeFilterItems()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		List<EncyclopediaFilterItem> typeFilterItems = ((DefaultEncyclopediaUnitPage)this).GetTypeFilterItems();
		typeFilterItems.Add(new EncyclopediaFilterItem(new TextObject("{=bOhiqquf}Mariner", (Dictionary<string, object>)null), (Predicate<object>)((object s) => ((CharacterObject)s).IsMariner)));
		return typeFilterItems;
	}
}
