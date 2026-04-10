using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace SandBox;

public class UnlockFogOfWarCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		foreach (Hero item in (List<Hero>)(object)Hero.AllAliveHeroes)
		{
			item.IsKnownToPlayer = true;
		}
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=jPtG0Pu1}Unlock Fog of War", (Dictionary<string, object>)null);
	}
}
