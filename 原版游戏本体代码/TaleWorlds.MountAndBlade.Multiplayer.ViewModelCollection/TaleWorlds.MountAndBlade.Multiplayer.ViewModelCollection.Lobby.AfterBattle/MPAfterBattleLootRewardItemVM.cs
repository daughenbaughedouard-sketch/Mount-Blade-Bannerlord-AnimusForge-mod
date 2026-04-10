using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.AfterBattle;

public class MPAfterBattleLootRewardItemVM : MPAfterBattleRewardItemVM
{
	public MPAfterBattleLootRewardItemVM(int lootGained, int additionalLootFromBadges)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		base.Type = 0;
		GameTexts.SetVariable("LOOT", lootGained);
		string text = ((object)new TextObject("{=JYIURZLb}+{LOOT} from match", (Dictionary<string, object>)null)).ToString();
		if (additionalLootFromBadges > 0)
		{
			GameTexts.SetVariable("LOOT", additionalLootFromBadges);
			GameTexts.SetVariable("STR1", text);
			GameTexts.SetVariable("STR2", new TextObject("{=erp8X0KD}+{LOOT} from badges", (Dictionary<string, object>)null));
			GameTexts.SetVariable("newline", "\n");
			base.Name = ((object)GameTexts.FindText("str_string_newline_string", (string)null)).ToString();
		}
		else
		{
			base.Name = text;
		}
		((ViewModel)this).RefreshValues();
	}
}
