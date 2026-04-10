using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Localization;

namespace SandBox;

public class CompleteBuildingProjectCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		if (Settlement.CurrentSettlement == null || !Settlement.CurrentSettlement.IsFortification)
		{
			return;
		}
		foreach (Building item in (List<Building>)(object)Settlement.CurrentSettlement.Town.Buildings)
		{
			if (item.CurrentLevel < 3)
			{
				int currentLevel = item.CurrentLevel;
				item.CurrentLevel = currentLevel + 1;
			}
		}
	}

	public override TextObject GetName()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement != null)
		{
			TextObject val = new TextObject("{=5uXs8pS9}Complete All Building Projects in {SETTLEMENT_NAME}", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT_NAME", ((object)currentSettlement.Name).ToString());
			return val;
		}
		return null;
	}
}
