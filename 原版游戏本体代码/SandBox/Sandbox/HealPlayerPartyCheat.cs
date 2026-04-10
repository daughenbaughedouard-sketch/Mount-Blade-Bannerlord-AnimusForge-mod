using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public class HealPlayerPartyCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.Current != null || MobileParty.MainParty.MapEvent != null || MobileParty.MainParty.SiegeEvent != null || Campaign.Current.ConversationManager.OneToOneConversationCharacter != null)
		{
			return;
		}
		for (int i = 0; i < PartyBase.MainParty.MemberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = PartyBase.MainParty.MemberRoster.GetElementCopyAtIndex(i);
			if (((BasicCharacterObject)elementCopyAtIndex.Character).IsHero)
			{
				elementCopyAtIndex.Character.HeroObject.Heal(elementCopyAtIndex.Character.HeroObject.MaxHitPoints, false);
			}
			else
			{
				MobileParty.MainParty.Party.AddToMemberRosterElementAtIndex(i, 0, -PartyBase.MainParty.MemberRoster.GetElementWoundedNumber(i));
			}
		}
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=HidEvGr4}Heal Player Party", (Dictionary<string, object>)null);
	}
}
