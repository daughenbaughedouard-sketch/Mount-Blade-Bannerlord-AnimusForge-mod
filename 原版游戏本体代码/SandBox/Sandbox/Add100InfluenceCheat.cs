using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace SandBox;

public class Add100InfluenceCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		ChangeClanInfluenceAction.Apply(Clan.PlayerClan, 100f);
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=6TgRwB2Q}Add 100 Influence", (Dictionary<string, object>)null);
	}
}
