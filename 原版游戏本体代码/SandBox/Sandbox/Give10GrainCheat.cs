using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace SandBox;

public class Give10GrainCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		ItemObject val = ((IEnumerable<ItemObject>)MBObjectManager.Instance.GetObjectTypeList<ItemObject>())?.FirstOrDefault((Func<ItemObject, bool>)((ItemObject i) => ((MBObjectBase)i).StringId == "grain"));
		if (val == null)
		{
			return;
		}
		PartyBase mainParty = PartyBase.MainParty;
		if (mainParty != null)
		{
			ItemRoster itemRoster = mainParty.ItemRoster;
			if (itemRoster != null)
			{
				itemRoster.AddToCounts(val, 10);
			}
		}
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=Jdc2aaYo}Give 10 Grain", (Dictionary<string, object>)null);
	}
}
