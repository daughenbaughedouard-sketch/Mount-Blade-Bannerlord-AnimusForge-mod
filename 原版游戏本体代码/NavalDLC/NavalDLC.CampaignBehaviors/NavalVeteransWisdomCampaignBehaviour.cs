using System;
using System.Collections.Generic;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace NavalDLC.CampaignBehaviors;

public class NavalVeteransWisdomCampaignBehaviour : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnDailyTickParty);
		CampaignEvents.PerkOpenedEvent.AddNonSerializedListener((object)this, (Action<Hero, PerkObject>)OnPerkOpened);
	}

	private void OnPerkOpened(Hero hero, PerkObject perk)
	{
		if (hero == Hero.MainHero && (perk == NavalPerks.Boatswain.NavalHorde || perk == NavalPerks.Boatswain.Optimization || perk == NavalPerks.Boatswain.GildedPurse))
		{
			MobileParty.MainParty.ItemRoster.UpdateVersion();
		}
	}

	private void OnDailyTickParty(MobileParty party)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (!party.HasPerk(NavalPerks.Boatswain.VeteransWisdom, false))
		{
			return;
		}
		int level = party.GetEffectiveRoleHolder((PartyRole)5).Level;
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)party.MemberRoster.GetTroopRoster())
		{
			if (((BasicCharacterObject)item.Character).IsHero && item.Character.HeroObject.CompanionOf == party.ActualClan)
			{
				float randomFloat = MBRandom.RandomFloat;
				SkillObject val = ((randomFloat < 0.33f) ? NavalSkills.Mariner : ((!(randomFloat < 0.66f)) ? NavalSkills.Shipmaster : NavalSkills.Boatswain));
				item.Character.HeroObject.AddSkillXp(val, NavalPerks.Boatswain.VeteransWisdom.PrimaryBonus * (float)level);
			}
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
