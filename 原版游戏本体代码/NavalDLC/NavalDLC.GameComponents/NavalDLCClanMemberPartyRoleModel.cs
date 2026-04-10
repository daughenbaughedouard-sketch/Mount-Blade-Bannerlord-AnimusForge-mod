using System.Collections.Generic;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalDLCClanMemberPartyRoleModel : ClanMemberPartyRoleModel
{
	public override int MaximumPartyRoleAssignmentCount => 2;

	public override IEnumerable<PartyRole> GetAssignablePartyRoles()
	{
		yield return (PartyRole)10;
		yield return (PartyRole)9;
		yield return (PartyRole)7;
		yield return (PartyRole)8;
		yield return (PartyRole)14;
		yield return (PartyRole)15;
	}

	public override SkillObject GetRelevantSkillForPartyRole(PartyRole role)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)role == 14)
		{
			return NavalSkills.Boatswain;
		}
		if ((int)role == 15)
		{
			return NavalSkills.Shipmaster;
		}
		return ((MBGameModel<ClanMemberPartyRoleModel>)this).BaseModel.GetRelevantSkillForPartyRole(role);
	}

	public override bool IsHeroAssignableForPartyRole(Hero hero, PartyRole role, MobileParty party)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<ClanMemberPartyRoleModel>)this).BaseModel.IsHeroAssignableForPartyRole(hero, role, party);
	}

	public override bool DoesHeroHaveEnoughSkillForPartyRole(Hero hero, PartyRole role, MobileParty party)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (party.GetHeroPartyRoles(hero).Contains(role))
		{
			return true;
		}
		if ((int)role == 14 || (int)role == 15)
		{
			return Campaign.Current.Models.ClanMemberPartyRoleModel.IsHeroAssignableForPartyRoleInParty(role, hero, party);
		}
		return ((MBGameModel<ClanMemberPartyRoleModel>)this).BaseModel.DoesHeroHaveEnoughSkillForPartyRole(hero, role, party);
	}

	public override bool IsHeroAssignableForPartyRoleInParty(PartyRole role, Hero hero, MobileParty party)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<ClanMemberPartyRoleModel>)this).BaseModel.IsHeroAssignableForPartyRoleInParty(role, hero, party);
	}
}
