using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace SandBox;

public class Add100RenownCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		GainRenownAction.Apply(Hero.MainHero, 100f, true);
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=zXQwb3lj}Add 100 Renown", (Dictionary<string, object>)null);
	}
}
