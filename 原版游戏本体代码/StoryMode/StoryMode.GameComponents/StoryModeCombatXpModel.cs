using StoryMode.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace StoryMode.GameComponents;

public class StoryModeCombatXpModel : CombatXpModel
{
	public override float CaptainRadius => ((MBGameModel<CombatXpModel>)this).BaseModel.CaptainRadius;

	public override SkillObject GetSkillForWeapon(WeaponComponentData weapon, bool isSiegeEngineHit)
	{
		return ((MBGameModel<CombatXpModel>)this).BaseModel.GetSkillForWeapon(weapon, isSiegeEngineHit);
	}

	public override ExplainedNumber GetXpFromHit(CharacterObject attackerTroop, CharacterObject captain, CharacterObject attackedTroop, PartyBase attackerParty, int damage, bool isFatal, MissionTypeEnum missionType)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsTrainingField())
		{
			return new ExplainedNumber(0f, false, (TextObject)null);
		}
		return ((MBGameModel<CombatXpModel>)this).BaseModel.GetXpFromHit(attackerTroop, captain, attackedTroop, attackerParty, damage, isFatal, missionType);
	}

	public override float GetXpMultiplierFromShotDifficulty(float shotDifficulty)
	{
		return ((MBGameModel<CombatXpModel>)this).BaseModel.GetXpMultiplierFromShotDifficulty(shotDifficulty);
	}
}
