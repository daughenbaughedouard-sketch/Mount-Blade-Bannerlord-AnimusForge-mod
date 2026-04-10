using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.CharacterDevelopment;

public class NavalSkills
{
	private SkillObject _skillMariner;

	private SkillObject _skillBoatswain;

	private SkillObject _skillShipmaster;

	private static NavalSkills Instance => NavalDLCManager.Instance.NavalSkills;

	public static SkillObject Mariner => Instance._skillMariner;

	public static SkillObject Boatswain => Instance._skillBoatswain;

	public static SkillObject Shipmaster => Instance._skillShipmaster;

	private SkillObject Create(string stringId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		return Game.Current.ObjectManager.RegisterPresumedObject<SkillObject>(new SkillObject(stringId));
	}

	private void InitializeAll()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0037: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_006f: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Expected O, but got Unknown
		_skillMariner.Initialize(new TextObject("{=bOhiqquf}Mariner", (Dictionary<string, object>)null), new TextObject("{=JSvE81Iw}Enhances your personal combat prowess during naval engagements and bolsters your effectiveness in leading troops and employing tactics in sea battles.", (Dictionary<string, object>)null), (CharacterAttribute[])(object)new CharacterAttribute[2]
		{
			DefaultCharacterAttributes.Endurance,
			DefaultCharacterAttributes.Cunning
		});
		_skillBoatswain.Initialize(new TextObject("{=olTmdP9j}Boatswain", (Dictionary<string, object>)null), new TextObject("{=SZ0BH8b1}Governs the well-being and discipline of your ship's crew, as well as the vessel's overall combat readiness, including rigging and supplies.", (Dictionary<string, object>)null), (CharacterAttribute[])(object)new CharacterAttribute[2]
		{
			DefaultCharacterAttributes.Control,
			DefaultCharacterAttributes.Social
		});
		_skillShipmaster.Initialize(new TextObject("{=SSLTboWZ}Shipmaster", (Dictionary<string, object>)null), new TextObject("{=CmXMqtcU}Improves your navigational abilities, the effectiveness of naval siege engines under your command, and the speed and quality of ship repairs and upgrades.", (Dictionary<string, object>)null), (CharacterAttribute[])(object)new CharacterAttribute[2]
		{
			DefaultCharacterAttributes.Vigor,
			DefaultCharacterAttributes.Intelligence
		});
	}

	public NavalSkills()
	{
		RegisterAll();
	}

	private void RegisterAll()
	{
		_skillMariner = Create("Mariner");
		_skillBoatswain = Create("Boatswain");
		_skillShipmaster = Create("Shipmaster");
		InitializeAll();
	}
}
