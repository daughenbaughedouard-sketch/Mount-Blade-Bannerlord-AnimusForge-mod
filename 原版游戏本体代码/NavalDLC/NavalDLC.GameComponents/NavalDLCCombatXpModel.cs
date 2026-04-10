using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCCombatXpModel : CombatXpModel
{
	private const float NavalXPBonusForNonHeroTroops = 0.5f;

	public override float CaptainRadius => ((MBGameModel<CombatXpModel>)this).BaseModel.CaptainRadius;

	public override SkillObject GetSkillForWeapon(WeaponComponentData weapon, bool isSiegeEngineHit)
	{
		return ((MBGameModel<CombatXpModel>)this).BaseModel.GetSkillForWeapon(weapon, isSiegeEngineHit);
	}

	public override ExplainedNumber GetXpFromHit(CharacterObject attackerTroop, CharacterObject captain, CharacterObject attackedTroop, PartyBase attackerParty, int damage, bool isFatal, MissionTypeEnum missionType)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber xpFromHit = ((MBGameModel<CombatXpModel>)this).BaseModel.GetXpFromHit(attackerTroop, captain, attackedTroop, attackerParty, damage, isFatal, missionType);
		if (((attackerParty != null) ? attackerParty.MapEvent : null) != null)
		{
			if (attackerParty.MapEvent.IsNavalMapEvent)
			{
				if (!((BasicCharacterObject)attackerTroop).IsHero)
				{
					((ExplainedNumber)(ref xpFromHit)).AddFactor(0.5f, (TextObject)null);
				}
				else if (attackerTroop.HeroObject.CompanionOf != null && attackerParty.IsMobile)
				{
					PerkHelper.AddPerkBonusForParty(NavalPerks.Mariner.NavalFightingTraining, attackerParty.MobileParty, true, ref xpFromHit, false);
				}
			}
			Hero leaderHero = attackerParty.LeaderHero;
			object obj;
			if (leaderHero == null)
			{
				obj = null;
			}
			else
			{
				Clan clan = leaderHero.Clan;
				obj = ((clan != null) ? clan.Kingdom : null);
			}
			if (obj != null && attackerParty.LeaderHero.Clan.Kingdom.HasPolicy(NavalPolicies.FraternalFleetDoctrine))
			{
				((ExplainedNumber)(ref xpFromHit)).AddFactor(-0.15f, ((PropertyObject)NavalPolicies.FraternalFleetDoctrine).Name);
			}
		}
		return xpFromHit;
	}

	public override float GetXpMultiplierFromShotDifficulty(float shotDifficulty)
	{
		return ((MBGameModel<CombatXpModel>)this).BaseModel.GetXpMultiplierFromShotDifficulty(shotDifficulty);
	}
}
