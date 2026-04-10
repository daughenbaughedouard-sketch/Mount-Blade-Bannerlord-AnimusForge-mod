using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace StoryMode.StoryModeObjects;

public class StoryModeHeroes
{
	private const string BrotherStringId = "tutorial_npc_brother";

	private const string LittleBrotherStringId = "storymode_little_brother";

	private const string LittleSisterStringId = "storymode_little_sister";

	private const string TacitusStringId = "tutorial_npc_tacitus";

	private const string RadagosStringId = "tutorial_npc_radagos";

	private const string IstianaStringId = "storymode_imperial_mentor_istiana";

	private const string ArzagosStringId = "storymode_imperial_mentor_arzagos";

	private const string GalterStringId = "radagos_henchman";

	private const string MainHeroMotherId = "main_hero_mother";

	private const string MainHeroFatherId = "main_hero_father";

	private Hero _elderBrother;

	private Hero _littleBrother;

	private Hero _littleSister;

	private Hero _tacitus;

	private Hero _radagos;

	private Hero _imperialMentor;

	private Hero _antiImperialMentor;

	private Hero _radagosHenchman;

	private Hero _mainHeroMother;

	private Hero _mainHeroFather;

	public static Hero ElderBrother => StoryModeManager.Current.StoryModeHeroes._elderBrother;

	public static Hero LittleBrother => StoryModeManager.Current.StoryModeHeroes._littleBrother;

	public static Hero LittleSister => StoryModeManager.Current.StoryModeHeroes._littleSister;

	public static Hero Tacitus => StoryModeManager.Current.StoryModeHeroes._tacitus;

	public static Hero Radagos => StoryModeManager.Current.StoryModeHeroes._radagos;

	public static Hero ImperialMentor => StoryModeManager.Current.StoryModeHeroes._imperialMentor;

	public static Hero AntiImperialMentor => StoryModeManager.Current.StoryModeHeroes._antiImperialMentor;

	public static Hero RadagosHenchman => StoryModeManager.Current.StoryModeHeroes._radagosHenchman;

	public static Hero MainHeroMother => StoryModeManager.Current.StoryModeHeroes._mainHeroMother;

	public static Hero MainHeroFather => StoryModeManager.Current.StoryModeHeroes._mainHeroFather;

	internal StoryModeHeroes()
	{
		RegisterAll();
	}

	private void RegisterAll()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		Clan clan = Campaign.Current.CampaignObjectManager.Find<Clan>("player_faction");
		CharacterObject val = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_mother");
		CharacterObject val2 = Game.Current.ObjectManager.GetObject<CharacterObject>("main_hero_father");
		if (HeroCreator.CreateBasicHero("main_hero_mother", val, ref _mainHeroMother, false))
		{
			_mainHeroMother.Clan = clan;
			CampaignTime birthDay = default(CampaignTime);
			CampaignTime deathDay = default(CampaignTime);
			HeroHelper.GetRandomDeathDayAndBirthDay((int)((BasicCharacterObject)val).Age, ref birthDay, ref deathDay);
			_mainHeroMother.SetBirthDay(birthDay);
			_mainHeroMother.SetDeathDay(deathDay);
		}
		if (HeroCreator.CreateBasicHero("main_hero_father", val2, ref _mainHeroFather, false))
		{
			_mainHeroFather.Clan = clan;
			CampaignTime birthDay2 = default(CampaignTime);
			CampaignTime deathDay2 = default(CampaignTime);
			HeroHelper.GetRandomDeathDayAndBirthDay((int)((BasicCharacterObject)val2).Age, ref birthDay2, ref deathDay2);
			_mainHeroFather.SetBirthDay(birthDay2);
			_mainHeroFather.SetDeathDay(deathDay2);
		}
		if (HeroCreator.CreateBasicHero("tutorial_npc_brother", MBObjectManager.Instance.GetObject<CharacterObject>("tutorial_npc_brother"), ref _elderBrother, true))
		{
			_elderBrother.Clan = clan;
			TextObject val3 = GameTexts.FindText("str_player_brother_name", ((MBObjectBase)val.Culture).StringId);
			_elderBrother.SetName(val3, val3);
			_elderBrother.Mother = val.HeroObject;
			_elderBrother.Father = val2.HeroObject;
			_elderBrother.HeroDeveloper.ResetCharacterStats();
		}
		if (HeroCreator.CreateBasicHero("storymode_little_brother", MBObjectManager.Instance.GetObject<CharacterObject>("storymode_little_brother"), ref _littleBrother, true))
		{
			TextObject val4 = GameTexts.FindText("str_player_little_brother_name", ((MBObjectBase)val.Culture).StringId);
			_littleBrother.SetName(val4, val4);
			_littleBrother.Mother = val.HeroObject;
			_littleBrother.Father = val2.HeroObject;
		}
		if (HeroCreator.CreateBasicHero("storymode_little_sister", MBObjectManager.Instance.GetObject<CharacterObject>("storymode_little_sister"), ref _littleSister, true))
		{
			TextObject val5 = GameTexts.FindText("str_player_little_sister_name", ((MBObjectBase)val.Culture).StringId);
			_littleSister.SetName(val5, val5);
			_littleSister.Mother = val.HeroObject;
			_littleSister.Father = val2.HeroObject;
		}
		HeroCreator.CreateBasicHero("tutorial_npc_tacitus", MBObjectManager.Instance.GetObject<CharacterObject>("tutorial_npc_tacitus"), ref _tacitus, true);
		HeroCreator.CreateBasicHero("tutorial_npc_radagos", MBObjectManager.Instance.GetObject<CharacterObject>("tutorial_npc_radagos"), ref _radagos, true);
		HeroCreator.CreateBasicHero("storymode_imperial_mentor_istiana", MBObjectManager.Instance.GetObject<CharacterObject>("storymode_imperial_mentor_istiana"), ref _imperialMentor, true);
		HeroCreator.CreateBasicHero("storymode_imperial_mentor_arzagos", MBObjectManager.Instance.GetObject<CharacterObject>("storymode_imperial_mentor_arzagos"), ref _antiImperialMentor, true);
		HeroCreator.CreateBasicHero("radagos_henchman", MBObjectManager.Instance.GetObject<CharacterObject>("radagos_henchman"), ref _radagosHenchman, true);
	}
}
