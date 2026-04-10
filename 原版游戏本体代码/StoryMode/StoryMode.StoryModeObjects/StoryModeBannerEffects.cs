using TaleWorlds.Core;

namespace StoryMode.StoryModeObjects;

public class StoryModeBannerEffects
{
	private const string NotImplementedText = "{=!}Not Implemented.";

	private BannerEffect _dragonBannerEffect;

	public static BannerEffect DragonBannerEffect => StoryModeManager.Current.StoryModeBannerEffects._dragonBannerEffect;

	public StoryModeBannerEffects()
	{
		RegisterAll();
	}

	private void RegisterAll()
	{
		_dragonBannerEffect = Create("dragon_banner_effect");
		InitializeAll();
	}

	private BannerEffect Create(string stringId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		return Game.Current.ObjectManager.RegisterPresumedObject<BannerEffect>(new BannerEffect(stringId));
	}

	private void InitializeAll()
	{
		_dragonBannerEffect.Initialize("{=!}Not Implemented.", "{=!}Not Implemented.", 0f, 0f, 0f, (EffectIncrementType)(-1));
	}
}
