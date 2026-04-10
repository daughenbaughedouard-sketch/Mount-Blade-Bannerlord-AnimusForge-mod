using System.Collections.Generic;
using System.Linq;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents;

public class StoryModeVoiceOverModel : VoiceOverModel
{
	private const string Male = "male";

	private const string Female = "female";

	public override string GetSoundPathForCharacter(CharacterObject character, VoiceObject voiceObject)
	{
		if (voiceObject == null)
		{
			return "";
		}
		if (!TutorialPhase.Instance.IsCompleted && TutorialPhase.Instance.TutorialVillageHeadman.CharacterObject == character)
		{
			string text = ((IEnumerable<string>)voiceObject.VoicePaths).First();
			Debug.Print("[VOICEOVER]Sound path found: " + BasePath.Name + text, 0, (DebugColor)12, 17592186044416uL);
			text = text.Replace("$PLATFORM", "PC");
			return text + ".ogg";
		}
		if (StoryModeHeroes.ElderBrother.CharacterObject == character)
		{
			string text2 = "";
			string value = ((MBObjectBase)character).StringId + "_" + (((BasicCharacterObject)CharacterObject.PlayerCharacter).IsFemale ? "female" : "male");
			foreach (string item in (List<string>)(object)voiceObject.VoicePaths)
			{
				if (item.Contains(value))
				{
					text2 = item;
					break;
				}
				if (item.Contains(((MBObjectBase)character).StringId + "_"))
				{
					text2 = item;
				}
			}
			if (string.IsNullOrEmpty(text2))
			{
				return text2;
			}
			Debug.Print("[VOICEOVER]Sound path found: " + BasePath.Name + text2, 0, (DebugColor)12, 17592186044416uL);
			text2 = text2.Replace("$PLATFORM", "PC");
			return text2 + ".ogg";
		}
		return ((MBGameModel<VoiceOverModel>)this).BaseModel.GetSoundPathForCharacter(character, voiceObject);
	}

	public override string GetAccentClass(CultureObject culture, bool isHighClass)
	{
		return ((MBGameModel<VoiceOverModel>)this).BaseModel.GetAccentClass(culture, isHighClass);
	}
}
