using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace SandBox;

public class Add1000GoldCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, 1000, true);
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=KLbeF6gf}Add 1000 Gold", (Dictionary<string, object>)null);
	}
}
