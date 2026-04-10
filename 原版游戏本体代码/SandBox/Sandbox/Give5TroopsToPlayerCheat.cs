using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public class Give5TroopsToPlayerCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		Settlement val = SettlementHelper.FindNearestFortificationToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)null);
		if (Mission.Current != null || MobileParty.MainParty.MapEvent != null || MobileParty.MainParty.SiegeEvent != null || Campaign.Current.ConversationManager.OneToOneConversationCharacter != null || val == null)
		{
			return;
		}
		CultureObject culture = val.Culture;
		Clan randomElementWithPredicate = Extensions.GetRandomElementWithPredicate<Clan>(Clan.All, (Func<Clan, bool>)((Clan x) => x.Culture != null && (culture == null || culture == x.Culture) && !x.IsMinorFaction && !x.IsBanditFaction));
		int num = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
		num = MBMath.ClampInt(num, 0, num);
		int num2 = 5;
		num2 = MBMath.ClampInt(num2, 0, num);
		if (randomElementWithPredicate != null && num2 > 0)
		{
			CharacterObject val2 = randomElementWithPredicate.Culture.BasicTroop;
			if (MBRandom.RandomFloat < 0.3f && randomElementWithPredicate.Culture.EliteBasicTroop != null)
			{
				val2 = randomElementWithPredicate.Culture.EliteBasicTroop;
			}
			CharacterObject randomElementInefficiently = Extensions.GetRandomElementInefficiently<CharacterObject>(CharacterHelper.GetTroopTree(val2, 1f, float.MaxValue));
			MobileParty.MainParty.AddElementToMemberRoster(randomElementInefficiently, num2, false);
		}
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=9FMvBKrV}Give 5 Troops", (Dictionary<string, object>)null);
	}
}
