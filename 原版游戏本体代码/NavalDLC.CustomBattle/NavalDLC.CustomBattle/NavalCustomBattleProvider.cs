using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.CustomBattle;

namespace NavalDLC.CustomBattle;

public class NavalCustomBattleProvider : ICustomBattleProvider
{
	public void StartCustomBattle()
	{
		MBGameManager.StartNewGame((MBGameManager)(object)new NavalCustomGameManager());
	}

	public TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=Q8gbZIiM}Naval Custom Battle", (Dictionary<string, object>)null);
	}
}
