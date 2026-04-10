using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.GameComponents;

public class NavalDLCVoiceOverModel : VoiceOverModel
{
	private const string NordClass = "nord";

	private const string CultureSouthernPirates = "southern_pirates";

	private const string SouthernPiratesClass = "southern_pirates";

	public override string GetSoundPathForCharacter(CharacterObject character, VoiceObject voiceObject)
	{
		return ((MBGameModel<VoiceOverModel>)this).BaseModel.GetSoundPathForCharacter(character, voiceObject);
	}

	public override string GetAccentClass(CultureObject culture, bool isHighClass)
	{
		if (((MBObjectBase)culture).StringId == "nord")
		{
			return "nord";
		}
		if (((MBObjectBase)culture).StringId == "southern_pirates")
		{
			return "southern_pirates";
		}
		return ((MBGameModel<VoiceOverModel>)this).BaseModel.GetAccentClass(culture, isHighClass);
	}
}
