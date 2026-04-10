using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.CustomBattle;

namespace TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

public class CustomBattleProvider : ICustomBattleProvider
{
	public void StartCustomBattle()
	{
		MBGameManager.StartNewGame((MBGameManager)(object)new CustomGameManager());
	}

	public TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=RZyk1LZy}Land Custom Battle", (Dictionary<string, object>)null);
	}
}
