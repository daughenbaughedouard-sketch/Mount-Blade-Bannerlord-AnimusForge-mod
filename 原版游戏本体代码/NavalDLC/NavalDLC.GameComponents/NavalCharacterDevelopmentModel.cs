using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;

namespace NavalDLC.GameComponents;

public class NavalCharacterDevelopmentModel : DefaultCharacterDevelopmentModel
{
	public const int AdditionalFocusPointsAtStart = 6;

	public override int MaxAttribute => ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.MaxAttribute;

	public override int MaxFocusPerSkill => ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.MaxFocusPerSkill;

	public override int MaxSkillRequiredForEpicPerkBonus => ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.MaxSkillRequiredForEpicPerkBonus;

	public override int MinSkillRequiredForEpicPerkBonus => ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.MinSkillRequiredForEpicPerkBonus;

	public override int FocusPointsPerLevel => ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.FocusPointsPerLevel;

	public override int FocusPointsAtStart => ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.FocusPointsAtStart + 6;

	public override int AttributePointsAtStart => ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.AttributePointsAtStart;

	public override int LevelsPerAttributePoint => ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.LevelsPerAttributePoint;

	public override ExplainedNumber CalculateLearningLimit(IReadOnlyPropertyOwner<CharacterAttribute> characterAttributes, int focusValue, SkillObject skill, bool includeDescriptions = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.CalculateLearningLimit(characterAttributes, focusValue, skill, includeDescriptions);
	}

	public override ExplainedNumber CalculateLearningRate(IReadOnlyPropertyOwner<CharacterAttribute> characterAttributes, int focusValue, int skillValue, SkillObject skill, bool includeDescriptions = false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.CalculateLearningRate(characterAttributes, focusValue, skillValue, skill, includeDescriptions);
	}

	public override int GetMaxSkillPoint()
	{
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.GetMaxSkillPoint();
	}

	public override CharacterAttribute GetNextAttributeToUpgrade(Hero hero)
	{
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.GetNextAttributeToUpgrade(hero);
	}

	public override PerkObject GetNextPerkToChoose(Hero hero, PerkObject perk)
	{
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.GetNextPerkToChoose(hero, perk);
	}

	public override SkillObject GetNextSkillToAddFocus(Hero hero)
	{
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.GetNextSkillToAddFocus(hero);
	}

	public override int GetSkillLevelChange(Hero hero, SkillObject skill, float skillXp)
	{
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.GetSkillLevelChange(hero, skill, skillXp);
	}

	public override void GetTraitLevelForTraitXp(Hero hero, TraitObject trait, int newValue, out int traitLevel, out int traitXp)
	{
		((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.GetTraitLevelForTraitXp(hero, trait, newValue, ref traitLevel, ref traitXp);
	}

	public override int GetTraitXpRequiredForTraitLevel(TraitObject trait, int traitLevel)
	{
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.GetTraitXpRequiredForTraitLevel(trait, traitLevel);
	}

	public override int GetXpAmountForSkillLevelChange(Hero hero, SkillObject skill, int skillLevelChange)
	{
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.GetXpAmountForSkillLevelChange(hero, skill, skillLevelChange);
	}

	public override int GetXpRequiredForSkillLevel(int skillLevel)
	{
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.GetXpRequiredForSkillLevel(skillLevel);
	}

	public override int SkillsRequiredForLevel(int level)
	{
		return ((MBGameModel<CharacterDevelopmentModel>)this).BaseModel.SkillsRequiredForLevel(level);
	}
}
