using System;
using System.Collections.Generic;
using Helpers;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class StoryModeCharacterCreationCampaignBehavior : CampaignBehaviorBase, ICharacterCreationContentHandler
{
	private const string BrotherNarrativeCharacterStringId = "brother_character";

	private const string PlayerEscapeNarrativeCharacterStringId = "player_escape_character";

	private int _focusToAdd = 1;

	private int _skillLevelToAdd = 10;

	private int _attributeLevelToAdd = 1;

	private CharacterCreationManager _characterCreationManager
	{
		get
		{
			GameState activeState = GameStateManager.Current.ActiveState;
			GameState obj = ((activeState is CharacterCreationState) ? activeState : null);
			if (obj == null)
			{
				return null;
			}
			return ((CharacterCreationState)obj).CharacterCreationManager;
		}
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnCharacterCreationInitializedEvent.AddNonSerializedListener((object)this, (Action<CharacterCreationManager>)OnCharacterCreationInitialized);
		CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener((object)this, (Action)OnCharacterCreationIsOver);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
	}

	private void OnGameLoadFinished()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		ApplicationVersion lastLoadedGameVersion = MBSaveLoad.LastLoadedGameVersion;
		if (!((ApplicationVersion)(ref lastLoadedGameVersion)).IsOlderThan(ApplicationVersion.FromString("v1.3.1.52060", 0)) || !(((MBObjectBase)Hero.MainHero).StringId == "main_hero"))
		{
			return;
		}
		if (Hero.MainHero.Father == null)
		{
			Hero.MainHero.Father = StoryModeHeroes.MainHeroFather;
		}
		if (Hero.MainHero.Mother == null)
		{
			Hero.MainHero.Mother = StoryModeHeroes.MainHeroMother;
		}
		if (!Hero.MainHero.Father.IsDead && !Hero.MainHero.Mother.IsDead)
		{
			if (Hero.MainHero.Father.Spouse == null)
			{
				Hero.MainHero.Father.Spouse = Hero.MainHero.Mother;
			}
			if (Hero.MainHero.Mother.Spouse == null)
			{
				Hero.MainHero.Mother.Spouse = Hero.MainHero.Father;
			}
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnCharacterCreationIsOver()
	{
		UpdateHomeSettlementsOfFamily();
		FinalizeFamilyStory();
	}

	private void UpdateHomeSettlementsOfFamily()
	{
		Settlement homeSettlement = Hero.MainHero.HomeSettlement;
		StoryModeHeroes.MainHeroFather.BornSettlement = homeSettlement;
		StoryModeHeroes.MainHeroFather.UpdateHomeSettlement();
		StoryModeHeroes.MainHeroMother.BornSettlement = homeSettlement;
		StoryModeHeroes.MainHeroMother.UpdateHomeSettlement();
		StoryModeHeroes.LittleBrother.BornSettlement = homeSettlement;
		StoryModeHeroes.LittleBrother.UpdateHomeSettlement();
		StoryModeHeroes.LittleSister.BornSettlement = homeSettlement;
		StoryModeHeroes.LittleSister.UpdateHomeSettlement();
		StoryModeHeroes.ElderBrother.BornSettlement = homeSettlement;
		StoryModeHeroes.ElderBrother.UpdateHomeSettlement();
	}

	private void FinalizeFamilyStory()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		TextObject val = new TextObject("{=h68qCoz3}{PLAYER_LITTLE_BROTHER.NAME} is the little brother of {PLAYER.LINK}. He has been abducted by bandits, who intend to sell him into slavery.", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("PLAYER_LITTLE_BROTHER", StoryModeHeroes.LittleBrother.CharacterObject, val, false);
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
		StoryModeHeroes.LittleBrother.EncyclopediaText = val;
		TextObject val2 = GameTexts.FindText("little_sister_encyclopedia_text", (string)null);
		StringHelpers.SetCharacterProperties("PLAYER_LITTLE_SISTER", StoryModeHeroes.LittleSister.CharacterObject, val2, false);
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val2, false);
		StoryModeHeroes.LittleSister.EncyclopediaText = val2;
		TextObject val3 = new TextObject("{=XmvaRfLM}{PLAYER_FATHER.NAME} was the father of {PLAYER.LINK}. He was slain when raiders attacked the inn at which his family was staying.", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("PLAYER_FATHER", StoryModeHeroes.MainHeroFather.CharacterObject, val3, false);
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val3, false);
		StoryModeHeroes.MainHeroFather.EncyclopediaText = val3;
		TextObject val4 = new TextObject("{=hrhvEWP8}{PLAYER_MOTHER.NAME} was the mother of {PLAYER.LINK}. She was slain when raiders attacked the inn at which her family was staying.", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("PLAYER_MOTHER", StoryModeHeroes.MainHeroMother.CharacterObject, val4, false);
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val4, false);
		StoryModeHeroes.MainHeroMother.EncyclopediaText = val4;
		TextObject val5 = new TextObject("{=bsWSecYa}{PLAYER_BROTHER.NAME} is the elder brother of {PLAYER.LINK}. He has gone in search of the family's two youngest siblings, {PLAYER_LITTLE_BROTHER.NAME} and {PLAYER_LITTLE_SISTER.NAME}.", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("PLAYER_BROTHER", StoryModeHeroes.ElderBrother.CharacterObject, val5, false);
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val5, false);
		StringHelpers.SetCharacterProperties("PLAYER_LITTLE_BROTHER", StoryModeHeroes.LittleBrother.CharacterObject, val5, false);
		StringHelpers.SetCharacterProperties("PLAYER_LITTLE_SISTER", StoryModeHeroes.LittleSister.CharacterObject, val5, false);
		StoryModeHeroes.ElderBrother.EncyclopediaText = val5;
	}

	private void OnCharacterCreationInitialized(CharacterCreationManager characterCreationManager)
	{
		_focusToAdd = characterCreationManager.CharacterCreationContent.FocusToAdd;
		_skillLevelToAdd = characterCreationManager.CharacterCreationContent.SkillLevelToAdd;
		_attributeLevelToAdd = characterCreationManager.CharacterCreationContent.AttributeLevelToAdd;
		characterCreationManager.RegisterCharacterCreationContentHandler((ICharacterCreationContentHandler)(object)this, 900);
	}

	public void InitializeCharacterCreationStages(CharacterCreationManager characterCreationManager)
	{
		characterCreationManager.RemoveStage<CharacterCreationBannerEditorStage>();
		characterCreationManager.RemoveStage<CharacterCreationClanNamingStage>();
	}

	public void InitializeData(CharacterCreationManager characterCreationManager)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		Hero.MainHero.Mother = StoryModeHeroes.MainHeroMother;
		Hero.MainHero.Father = StoryModeHeroes.MainHeroFather;
		characterCreationManager.CharacterCreationContent.ChangeReviewPageDescription(new TextObject("{=wbhKgpmr}You prepare to set off with your brother on a mission of vengeance and rescue. Here is your character. Continue if you are ready, or go back to make changes.", (Dictionary<string, object>)null));
		characterCreationManager.DeleteNarrativeMenuWithId("narrative_age_selection_menu");
		AddEscapeMenu(characterCreationManager);
	}

	void ICharacterCreationContentHandler.InitializeContent(CharacterCreationManager characterCreationManager)
	{
		InitializeCharacterCreationStages(characterCreationManager);
		InitializeData(characterCreationManager);
	}

	void ICharacterCreationContentHandler.AfterInitializeContent(CharacterCreationManager characterCreationManager)
	{
		ModifyParentMenu(characterCreationManager);
	}

	void ICharacterCreationContentHandler.OnStageCompleted(CharacterCreationStageBase stage)
	{
		if (stage is CharacterCreationFaceGeneratorStage)
		{
			FaceGenUpdated();
		}
	}

	void ICharacterCreationContentHandler.OnCharacterCreationFinalize(CharacterCreationManager characterCreationManager)
	{
		ApplyCulture(_characterCreationManager.CharacterCreationContent.SelectedCulture);
	}

	private void ApplyCulture(CultureObject culture)
	{
		StoryModeHeroes.LittleBrother.Culture = culture;
		StoryModeHeroes.LittleSister.Culture = culture;
	}

	private void FaceGenUpdated()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		NarrativeMenu narrativeMenuWithId = _characterCreationManager.GetNarrativeMenuWithId("narrative_parent_menu");
		BodyProperties val = BodyProperties.Default;
		BodyProperties val2 = BodyProperties.Default;
		foreach (NarrativeMenuCharacter character in narrativeMenuWithId.Characters)
		{
			if (character.StringId == "mother_character")
			{
				val = character.BodyProperties;
			}
			if (character.StringId == "father_character")
			{
				val2 = character.BodyProperties;
			}
		}
		Hero elderBrother = StoryModeHeroes.ElderBrother;
		uint hashCode = (uint)((object)Hero.MainHero.BodyProperties/*cast due to .constrained prefix*/).GetHashCode();
		string text = ((object)Hero.MainHero.Culture).ToString().ToLower() + ",";
		string text2 = ((object)Hero.MainHero.Culture).ToString().ToLower() + ",";
		int num = RandomOwnerExtensions.RandomIntWithSeed((IRandomOwner)(object)Hero.MainHero, hashCode, 1, 100);
		CreateSibling(StoryModeHeroes.LittleBrother, val, val2, hashCode + 1);
		CreateSibling(StoryModeHeroes.LittleSister, val, val2, hashCode + 2);
		BodyProperties randomBodyProperties = BodyProperties.GetRandomBodyProperties(((BasicCharacterObject)elderBrother.CharacterObject).Race, elderBrother.IsFemale, val, val2, 1, num, text, text2, ((BasicCharacterObject)Hero.MainHero.Father.CharacterObject).BodyPropertyRange.TattooTags, 0f);
		((BodyProperties)(ref randomBodyProperties))._002Ector(new DynamicBodyProperties(elderBrother.Age, 0.5f, 0.5f), ((BodyProperties)(ref randomBodyProperties)).StaticProperties);
		elderBrother.StaticBodyProperties = ((BodyProperties)(ref randomBodyProperties)).StaticProperties;
		elderBrother.Weight = ((BodyProperties)(ref randomBodyProperties)).Weight;
		elderBrother.Build = ((BodyProperties)(ref randomBodyProperties)).Build;
		foreach (NarrativeMenu item in (List<NarrativeMenu>)(object)_characterCreationManager.NarrativeMenus)
		{
			foreach (NarrativeMenuCharacter character2 in item.Characters)
			{
				if (character2.StringId.Equals("player_escape_character"))
				{
					character2.UpdateBodyProperties(((BasicCharacterObject)CharacterObject.PlayerCharacter).GetBodyProperties((Equipment)null, -1), ((BasicCharacterObject)CharacterObject.PlayerCharacter).Race, false);
				}
				if (character2.StringId.Equals("brother_character"))
				{
					character2.UpdateBodyProperties(elderBrother.BodyProperties, ((BasicCharacterObject)CharacterObject.PlayerCharacter).Race, false);
				}
			}
		}
	}

	private void ModifyParentMenu(CharacterCreationManager characterCreationManager)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		foreach (NarrativeMenuOption item in (List<NarrativeMenuOption>)(object)characterCreationManager.GetNarrativeMenuWithId("narrative_parent_menu").CharacterCreationMenuOptions)
		{
			item.SetOnConsequence(new NarrativeMenuOptionOnConsequenceDelegate(FinalizeParentsAndLittleSiblings));
		}
	}

	private List<NarrativeMenuCharacterArgs> GetEscapeMenuNarrativeMenuCharacterArgs(CultureObject culture, string occupationType, CharacterCreationManager characterCreationManager)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		List<NarrativeMenuCharacterArgs> list = new List<NarrativeMenuCharacterArgs>();
		string text = "brother_char_creation_" + ((MBObjectBase)characterCreationManager.CharacterCreationContent.SelectedCulture).StringId;
		list.Add(new NarrativeMenuCharacterArgs("brother_character", (int)StoryModeHeroes.ElderBrother.Age, text, "act_childhood_schooled", "spawnpoint_brother_brother_stage", "", "", (MountCreationKey)null, true, false));
		string text2 = string.Concat(str3: characterCreationManager.CharacterCreationContent.SelectedTitleType.ToString().ToLower(), str0: "player_char_creation_", str1: ((MBObjectBase)characterCreationManager.CharacterCreationContent.SelectedCulture).StringId, str2: "_");
		text2 += (Hero.MainHero.IsFemale ? "_f" : "_m");
		list.Add(new NarrativeMenuCharacterArgs("player_escape_character", (int)((BasicCharacterObject)CharacterObject.PlayerCharacter).Age, text2, "act_childhood_schooled", "spawnpoint_player_brother_stage", "", "", (MountCreationKey)null, true, ((BasicCharacterObject)CharacterObject.PlayerCharacter).IsFemale));
		return list;
	}

	private void AddEscapeMenu(CharacterCreationManager characterCreationManager)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Expected O, but got Unknown
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Expected O, but got Unknown
		//IL_0245: Expected O, but got Unknown
		//IL_0245: Expected O, but got Unknown
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		MBTextManager.SetTextVariable("EXP_VALUE", _skillLevelToAdd);
		List<NarrativeMenuCharacter> list = new List<NarrativeMenuCharacter>();
		NarrativeMenu narrativeMenuWithId = characterCreationManager.GetNarrativeMenuWithId("narrative_parent_menu");
		BodyProperties val = BodyProperties.Default;
		BodyProperties val2 = BodyProperties.Default;
		foreach (NarrativeMenuCharacter character in narrativeMenuWithId.Characters)
		{
			if (character.StringId == "mother_character")
			{
				val = character.BodyProperties;
			}
			if (character.StringId == "father_character")
			{
				val2 = character.BodyProperties;
			}
		}
		Hero elderBrother = StoryModeHeroes.ElderBrother;
		uint hashCode = (uint)((object)Hero.MainHero.BodyProperties/*cast due to .constrained prefix*/).GetHashCode();
		string text = ((object)Hero.MainHero.Culture).ToString().ToLower() + ",";
		string text2 = ((object)Hero.MainHero.Culture).ToString().ToLower() + ",";
		int num = RandomOwnerExtensions.RandomIntWithSeed((IRandomOwner)(object)Hero.MainHero, hashCode, 1, 100);
		CreateSibling(StoryModeHeroes.LittleBrother, val, val2, hashCode + 1);
		CreateSibling(StoryModeHeroes.LittleSister, val, val2, hashCode + 2);
		BodyProperties randomBodyProperties = BodyProperties.GetRandomBodyProperties(((BasicCharacterObject)elderBrother.CharacterObject).Race, elderBrother.IsFemale, val, val2, 1, num, text, text2, ((BasicCharacterObject)Hero.MainHero.Father.CharacterObject).BodyPropertyRange.TattooTags, 0f);
		((BodyProperties)(ref randomBodyProperties))._002Ector(new DynamicBodyProperties(elderBrother.Age, 0.5f, 0.5f), ((BodyProperties)(ref randomBodyProperties)).StaticProperties);
		elderBrother.StaticBodyProperties = ((BodyProperties)(ref randomBodyProperties)).StaticProperties;
		elderBrother.Weight = ((BodyProperties)(ref randomBodyProperties)).Weight;
		elderBrother.Build = ((BodyProperties)(ref randomBodyProperties)).Build;
		NarrativeMenuCharacter item = new NarrativeMenuCharacter("brother_character", randomBodyProperties, ((BasicCharacterObject)elderBrother.CharacterObject).Race, ((BasicCharacterObject)elderBrother.CharacterObject).IsFemale);
		list.Add(item);
		NarrativeMenuCharacter item2 = new NarrativeMenuCharacter("player_escape_character", Hero.MainHero.BodyProperties, ((BasicCharacterObject)CharacterObject.PlayerCharacter).Race, ((BasicCharacterObject)CharacterObject.PlayerCharacter).IsFemale);
		list.Add(item2);
		NarrativeMenu val3 = new NarrativeMenu("narrative_escape_menu", "narrative_adulthood_menu", "", new TextObject("{=peNBA0WW}Story Background", (Dictionary<string, object>)null), new TextObject("{=jg3T5AyE}Like many families in Calradia, your life was upended by war. Your home was ravaged by the passage of army after army. Eventually, you sold your property and set off with your father, mother, brother, and your two younger siblings to a new town you'd heard was safer. But you did not make it. Along the way, the inn at which you were staying was attacked by raiders. Your parents were slain and your two youngest siblings seized, but you and your brother survived because...", (Dictionary<string, object>)null), list, new GetNarrativeMenuCharacterArgsDelegate(GetEscapeMenuNarrativeMenuCharacterArgs));
		AddEscapeNarrativeMenuOptions(val3);
		characterCreationManager.AddNewMenu(val3);
	}

	private void AddEscapeNarrativeMenuOptions(NarrativeMenu narrativeMenu)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a8: Expected O, but got Unknown
		//IL_00a8: Expected O, but got Unknown
		//IL_00a8: Expected O, but got Unknown
		//IL_00a8: Expected O, but got Unknown
		//IL_00a8: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Expected O, but got Unknown
		//IL_0100: Expected O, but got Unknown
		//IL_0100: Expected O, but got Unknown
		//IL_0100: Expected O, but got Unknown
		//IL_0100: Expected O, but got Unknown
		//IL_0100: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Expected O, but got Unknown
		//IL_0158: Expected O, but got Unknown
		//IL_0158: Expected O, but got Unknown
		//IL_0158: Expected O, but got Unknown
		//IL_0158: Expected O, but got Unknown
		//IL_0158: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_01b0: Expected O, but got Unknown
		//IL_01b0: Expected O, but got Unknown
		//IL_01b0: Expected O, but got Unknown
		//IL_01b0: Expected O, but got Unknown
		//IL_01b0: Expected O, but got Unknown
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Expected O, but got Unknown
		//IL_020a: Expected O, but got Unknown
		//IL_020a: Expected O, but got Unknown
		//IL_020a: Expected O, but got Unknown
		//IL_020a: Expected O, but got Unknown
		//IL_020a: Expected O, but got Unknown
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Expected O, but got Unknown
		NarrativeMenuOption val = new NarrativeMenuOption("escape_subdued_raider_option", new TextObject("{=6vCHovVH}you subdued a raider.", (Dictionary<string, object>)null), new TextObject("{=CvBoRaFv}You were able to grab a knife in the confusion of the attack. You stabbed a raider blocking your way.", (Dictionary<string, object>)null), new GetNarrativeMenuOptionArgsDelegate(GetEscapeSubduedRaiderNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(EscapeSubduedRaiderNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(EscapeSubduedRaiderNarrativeOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(FinalizeMainHeroAndElderBrother));
		narrativeMenu.AddNarrativeMenuOption(val);
		NarrativeMenuOption val2 = new NarrativeMenuOption("escape_arrow_option", new TextObject("{=2XhW49TX}you drove them off with arrows.", (Dictionary<string, object>)null), new TextObject("{=ccf67J3J}You grabbed a bow and sent a few arrows the raiders' way. They took cover, giving you the opportunity to flee with your brother.", (Dictionary<string, object>)null), new GetNarrativeMenuOptionArgsDelegate(GetEscapeArrowNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(EscapeArrowNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(EscapeArrowNarrativeOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(FinalizeMainHeroAndElderBrother));
		narrativeMenu.AddNarrativeMenuOption(val2);
		NarrativeMenuOption val3 = new NarrativeMenuOption("escape_horse_option", new TextObject("{=gOI8lKcl}you rode off on a fast horse.", (Dictionary<string, object>)null), new TextObject("{=cepWNzEA}Jumping on the two remaining horses in the inn's burning stable, you and your brother broke out of the encircling raiders and rode off.", (Dictionary<string, object>)null), new GetNarrativeMenuOptionArgsDelegate(GetEscapeHorseNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(EscapeHorseNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(EscapeHorseNarrativeOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(FinalizeMainHeroAndElderBrother));
		narrativeMenu.AddNarrativeMenuOption(val3);
		NarrativeMenuOption val4 = new NarrativeMenuOption("escape_tricked_option", new TextObject("{=EdUppdLZ}you tricked the raiders.", (Dictionary<string, object>)null), new TextObject("{=ZqOvtLBM}In the confusion of the attack you shouted that someone had found treasure in the back room. You then made your way out of the undefended entrance with your brother.", (Dictionary<string, object>)null), new GetNarrativeMenuOptionArgsDelegate(GetEscapeTrickedNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(EscapeTrickedNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(EscapeTrickedNarrativeOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(FinalizeMainHeroAndElderBrother));
		narrativeMenu.AddNarrativeMenuOption(val4);
		NarrativeMenuOption val5 = new NarrativeMenuOption("escape_breakout_option", new TextObject("{=qhAhPWdp}you organized the travelers to break out.", (Dictionary<string, object>)null), new TextObject("{=Lmfi0cYk}You encouraged the few travellers in the inn to break out in a coordinated fashion. Raiders killed or captured most but you and your brother were able to escape.", (Dictionary<string, object>)null), new GetNarrativeMenuOptionArgsDelegate(GetEscapeBreakOutNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(EscapeBreakOutNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(EscapeBreakOutNarrativeOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(FinalizeMainHeroAndElderBrother));
		narrativeMenu.AddNarrativeMenuOption(val5);
		NarrativeMenuOption val6 = new NarrativeMenuOption("escape_makeshift_fortification_option", new TextObject("{=7AEw4RbK}you threw up makeshift fortifications.", (Dictionary<string, object>)null), new TextObject("{=Lmfi0cYk}You encouraged the few travellers in the inn to break out in a coordinated fashion. Raiders killed or captured most but you and your brother were able to escape.", (Dictionary<string, object>)null), new GetNarrativeMenuOptionArgsDelegate(GetMakeshiftFortificationNarrativeOptionArgs), new NarrativeMenuOptionOnConditionDelegate(MakeshiftFortificationNarrativeOptionOnCondition), new NarrativeMenuOptionOnSelectDelegate(MakeshiftFortificationNarrativeOptionOnSelect), new NarrativeMenuOptionOnConsequenceDelegate(FinalizeMainHeroAndElderBrother));
		narrativeMenu.AddNarrativeMenuOption(val6);
	}

	private void GetEscapeSubduedRaiderNarrativeOptionArgs(NarrativeMenuOptionArgs args)
	{
		SkillObject[] affectedSkills = (SkillObject[])(object)new SkillObject[2]
		{
			DefaultSkills.OneHanded,
			DefaultSkills.Athletics
		};
		args.SetAffectedSkills(affectedSkills);
		args.SetFocusToSkills(_focusToAdd);
		args.SetLevelToSkills(_skillLevelToAdd);
		args.SetLevelToAttribute(DefaultCharacterAttributes.Vigor, _attributeLevelToAdd);
	}

	private bool EscapeSubduedRaiderNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
	{
		return true;
	}

	private void EscapeSubduedRaiderNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
	{
		string animationId = "act_childhood_fierce";
		string animationId2 = "act_childhood_athlete";
		foreach (NarrativeMenuCharacter character in characterCreationManager.CurrentMenu.Characters)
		{
			if (character.StringId.Equals("player_escape_character"))
			{
				character.SetAnimationId(animationId);
			}
			if (character.StringId.Equals("brother_character"))
			{
				character.SetAnimationId(animationId2);
			}
		}
	}

	private void GetEscapeArrowNarrativeOptionArgs(NarrativeMenuOptionArgs args)
	{
		SkillObject[] affectedSkills = (SkillObject[])(object)new SkillObject[2]
		{
			DefaultSkills.Bow,
			DefaultSkills.Tactics
		};
		args.SetAffectedSkills(affectedSkills);
		args.SetFocusToSkills(_focusToAdd);
		args.SetLevelToSkills(_skillLevelToAdd);
		args.SetLevelToAttribute(DefaultCharacterAttributes.Control, _attributeLevelToAdd);
	}

	private bool EscapeArrowNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
	{
		return true;
	}

	private void EscapeArrowNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
	{
		string animationId = "act_childhood_athlete";
		string animationId2 = "act_childhood_sharp";
		foreach (NarrativeMenuCharacter character in characterCreationManager.CurrentMenu.Characters)
		{
			if (character.StringId.Equals("player_escape_character"))
			{
				character.SetAnimationId(animationId);
			}
			if (character.StringId.Equals("brother_character"))
			{
				character.SetAnimationId(animationId2);
			}
		}
	}

	private void GetEscapeHorseNarrativeOptionArgs(NarrativeMenuOptionArgs args)
	{
		SkillObject[] affectedSkills = (SkillObject[])(object)new SkillObject[2]
		{
			DefaultSkills.Riding,
			DefaultSkills.Scouting
		};
		args.SetAffectedSkills(affectedSkills);
		args.SetFocusToSkills(_focusToAdd);
		args.SetLevelToSkills(_skillLevelToAdd);
		args.SetLevelToAttribute(DefaultCharacterAttributes.Endurance, _attributeLevelToAdd);
	}

	private bool EscapeHorseNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
	{
		return true;
	}

	private void EscapeHorseNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
	{
		string animationId = "act_childhood_tough";
		string animationId2 = "act_childhood_decisive";
		foreach (NarrativeMenuCharacter character in characterCreationManager.CurrentMenu.Characters)
		{
			if (character.StringId.Equals("player_escape_character"))
			{
				character.SetAnimationId(animationId);
			}
			if (character.StringId.Equals("brother_character"))
			{
				character.SetAnimationId(animationId2);
			}
		}
	}

	private void GetEscapeTrickedNarrativeOptionArgs(NarrativeMenuOptionArgs args)
	{
		SkillObject[] affectedSkills = (SkillObject[])(object)new SkillObject[2]
		{
			DefaultSkills.Roguery,
			DefaultSkills.Tactics
		};
		args.SetAffectedSkills(affectedSkills);
		args.SetFocusToSkills(_focusToAdd);
		args.SetLevelToSkills(_skillLevelToAdd);
		args.SetLevelToAttribute(DefaultCharacterAttributes.Cunning, _attributeLevelToAdd);
	}

	private bool EscapeTrickedNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
	{
		return true;
	}

	private void EscapeTrickedNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
	{
		string animationId = "act_childhood_ready_handshield";
		string animationId2 = "act_aserai_aserai_mp_archer_idle";
		foreach (NarrativeMenuCharacter character in characterCreationManager.CurrentMenu.Characters)
		{
			if (character.StringId.Equals("player_escape_character"))
			{
				character.SetAnimationId(animationId);
			}
			if (character.StringId.Equals("brother_character"))
			{
				character.SetAnimationId(animationId2);
			}
		}
	}

	private void GetEscapeBreakOutNarrativeOptionArgs(NarrativeMenuOptionArgs args)
	{
		SkillObject[] affectedSkills = (SkillObject[])(object)new SkillObject[2]
		{
			DefaultSkills.Leadership,
			DefaultSkills.Charm
		};
		args.SetAffectedSkills(affectedSkills);
		args.SetFocusToSkills(_focusToAdd);
		args.SetLevelToSkills(_skillLevelToAdd);
		args.SetLevelToAttribute(DefaultCharacterAttributes.Social, _attributeLevelToAdd);
	}

	private bool EscapeBreakOutNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
	{
		return true;
	}

	private void EscapeBreakOutNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
	{
		string animationId = "act_childhood_manners";
		string animationId2 = "act_childhood_tough";
		foreach (NarrativeMenuCharacter character in characterCreationManager.CurrentMenu.Characters)
		{
			if (character.StringId.Equals("player_escape_character"))
			{
				character.SetAnimationId(animationId);
			}
			if (character.StringId.Equals("brother_character"))
			{
				character.SetAnimationId(animationId2);
			}
		}
	}

	private void GetMakeshiftFortificationNarrativeOptionArgs(NarrativeMenuOptionArgs args)
	{
		SkillObject[] affectedSkills = (SkillObject[])(object)new SkillObject[2]
		{
			DefaultSkills.Engineering,
			DefaultSkills.TwoHanded
		};
		args.SetAffectedSkills(affectedSkills);
		args.SetFocusToSkills(_focusToAdd);
		args.SetLevelToSkills(_skillLevelToAdd);
		args.SetLevelToAttribute(DefaultCharacterAttributes.Intelligence, _attributeLevelToAdd);
	}

	private bool MakeshiftFortificationNarrativeOptionOnCondition(CharacterCreationManager characterCreationManager)
	{
		return true;
	}

	private void MakeshiftFortificationNarrativeOptionOnSelect(CharacterCreationManager characterCreationManager)
	{
		string animationId = "act_childhood_ready_handshield";
		string animationId2 = "act_khuzait_mp_rabble_idle";
		foreach (NarrativeMenuCharacter character in characterCreationManager.CurrentMenu.Characters)
		{
			if (character.StringId.Equals("player_escape_character"))
			{
				character.SetAnimationId(animationId);
			}
			if (character.StringId.Equals("brother_character"))
			{
				character.SetAnimationId(animationId2);
			}
		}
	}

	private void FinalizeParentsAndLittleSiblings(CharacterCreationManager characterCreationManager)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		CharacterObject val = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_mother");
		CharacterObject val2 = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_father");
		CharacterObject characterObject = StoryModeHeroes.ElderBrother.CharacterObject;
		NarrativeMenuCharacter val3 = null;
		NarrativeMenuCharacter val4 = null;
		foreach (NarrativeMenuCharacter character in characterCreationManager.GetNarrativeMenuWithId("narrative_parent_menu").Characters)
		{
			if (character.StringId.Equals("mother_character"))
			{
				val3 = character;
			}
			if (character.StringId.Equals("father_character"))
			{
				val4 = character;
			}
		}
		Hero heroObject = val.HeroObject;
		BodyProperties bodyProperties = val3.BodyProperties;
		heroObject.StaticBodyProperties = ((BodyProperties)(ref bodyProperties)).StaticProperties;
		Hero heroObject2 = val2.HeroObject;
		bodyProperties = val4.BodyProperties;
		heroObject2.StaticBodyProperties = ((BodyProperties)(ref bodyProperties)).StaticProperties;
		Hero heroObject3 = val.HeroObject;
		bodyProperties = val3.BodyProperties;
		heroObject3.Weight = ((BodyProperties)(ref bodyProperties)).Weight;
		Hero heroObject4 = val.HeroObject;
		bodyProperties = val3.BodyProperties;
		heroObject4.Build = ((BodyProperties)(ref bodyProperties)).Build;
		Hero heroObject5 = val2.HeroObject;
		bodyProperties = val4.BodyProperties;
		heroObject5.Weight = ((BodyProperties)(ref bodyProperties)).Weight;
		Hero heroObject6 = val2.HeroObject;
		bodyProperties = val4.BodyProperties;
		heroObject6.Build = ((BodyProperties)(ref bodyProperties)).Build;
		if (val3.Equipment != null)
		{
			EquipmentHelper.AssignHeroEquipmentFromEquipment(val.HeroObject, val3.Equipment.DefaultEquipment);
		}
		if (val4.Equipment != null)
		{
			EquipmentHelper.AssignHeroEquipmentFromEquipment(val2.HeroObject, val4.Equipment.DefaultEquipment);
		}
		if (((BasicCharacterObject)characterObject).Equipment != null)
		{
			EquipmentHelper.AssignHeroEquipmentFromEquipment(characterObject.HeroObject, ((BasicCharacterObject)characterObject).Equipment);
		}
		val.HeroObject.Culture = Hero.MainHero.Culture;
		val2.HeroObject.Culture = Hero.MainHero.Culture;
		characterObject.HeroObject.Culture = Hero.MainHero.Culture;
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
		TextObject val5 = GameTexts.FindText("str_player_little_brother_name", ((MBObjectBase)Hero.MainHero.Culture).StringId);
		StoryModeHeroes.LittleBrother.SetName(val5, val5);
		StoryModeHeroes.LittleBrother.SetHasMet();
		TextObject val6 = GameTexts.FindText("str_player_little_sister_name", ((MBObjectBase)Hero.MainHero.Culture).StringId);
		StoryModeHeroes.LittleSister.SetName(val6, val6);
		StoryModeHeroes.LittleSister.SetHasMet();
		TextObject val7 = GameTexts.FindText("str_player_father_name", ((MBObjectBase)Hero.MainHero.Culture).StringId);
		val2.HeroObject.SetName(val7, val7);
		TextObject val8 = GameTexts.FindText("str_player_mother_name", ((MBObjectBase)Hero.MainHero.Culture).StringId);
		val.HeroObject.SetName(val8, val8);
		TextObject val9 = GameTexts.FindText("str_player_brother_name", ((MBObjectBase)Hero.MainHero.Culture).StringId);
		characterObject.HeroObject.SetName(val9, val9);
		val.HeroObject.Spouse = val2.HeroObject;
		val2.HeroObject.Spouse = val.HeroObject;
		val.HeroObject.UpdateHomeSettlement();
		val2.HeroObject.UpdateHomeSettlement();
		characterObject.HeroObject.UpdateHomeSettlement();
		val.HeroObject.SetHasMet();
		val2.HeroObject.SetHasMet();
		characterObject.HeroObject.SetHasMet();
	}

	private void FinalizeMainHeroAndElderBrother(CharacterCreationManager characterCreationManager)
	{
		NarrativeMenu narrativeMenuWithId = characterCreationManager.GetNarrativeMenuWithId("narrative_escape_menu");
		NarrativeMenuCharacter val = null;
		NarrativeMenuCharacter val2 = null;
		foreach (NarrativeMenuCharacter character in narrativeMenuWithId.Characters)
		{
			if (character.StringId.Equals("player_escape_character"))
			{
				val = character;
			}
			if (character.StringId.Equals("brother_character"))
			{
				val2 = character;
			}
		}
		((BasicCharacterObject)CharacterObject.PlayerCharacter).Equipment.FillFrom(val.Equipment.DefaultEquipment, true);
		((BasicCharacterObject)CharacterObject.PlayerCharacter).FirstCivilianEquipment.FillFrom(MBEquipmentRosterExtensions.GetRandomCivilianEquipment(val.Equipment), true);
		Hero elderBrother = StoryModeHeroes.ElderBrother;
		((BasicCharacterObject)elderBrother.CharacterObject).Equipment.FillFrom(val2.Equipment.DefaultEquipment, true);
		((BasicCharacterObject)elderBrother.CharacterObject).FirstCivilianEquipment.FillFrom(MBEquipmentRosterExtensions.GetRandomCivilianEquipment(val2.Equipment), true);
	}

	protected void CreateSibling(Hero hero, BodyProperties motherBodyProperties, BodyProperties fatherBodyProperties, uint seed)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		string text = ((object)Hero.MainHero.Culture).ToString().ToLower() + ",";
		string text2 = ((object)Hero.MainHero.Culture).ToString().ToLower() + ",";
		int num = RandomOwnerExtensions.RandomIntWithSeed((IRandomOwner)(object)Hero.MainHero, seed, 1, 100);
		BodyProperties randomBodyProperties = BodyProperties.GetRandomBodyProperties(((BasicCharacterObject)hero.CharacterObject).Race, hero.IsFemale, motherBodyProperties, fatherBodyProperties, 1, num, text, text2, hero.IsFemale ? ((BasicCharacterObject)Hero.MainHero.Mother.CharacterObject).BodyPropertyRange.TattooTags : ((BasicCharacterObject)Hero.MainHero.Father.CharacterObject).BodyPropertyRange.TattooTags, 0f);
		((BodyProperties)(ref randomBodyProperties))._002Ector(new DynamicBodyProperties(hero.Age, 0.5f, 0.5f), ((BodyProperties)(ref randomBodyProperties)).StaticProperties);
		hero.StaticBodyProperties = ((BodyProperties)(ref randomBodyProperties)).StaticProperties;
		hero.Weight = ((BodyProperties)(ref randomBodyProperties)).Weight;
		hero.Build = ((BodyProperties)(ref randomBodyProperties)).Build;
	}
}
