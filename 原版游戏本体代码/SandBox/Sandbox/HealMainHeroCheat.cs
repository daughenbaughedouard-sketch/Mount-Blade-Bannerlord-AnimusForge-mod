using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public class HealMainHeroCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		if (Agent.Main != null)
		{
			Agent.Main.Health = Agent.Main.HealthLimit;
		}
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=PsmnVIcb}Heal Main Hero", (Dictionary<string, object>)null);
	}
}
