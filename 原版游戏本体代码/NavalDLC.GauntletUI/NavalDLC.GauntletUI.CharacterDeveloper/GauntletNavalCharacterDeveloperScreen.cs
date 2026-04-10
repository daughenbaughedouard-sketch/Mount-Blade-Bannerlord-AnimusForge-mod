using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI.CharacterDeveloper;

[GameStateScreen(typeof(CharacterDeveloperState))]
public class GauntletNavalCharacterDeveloperScreen : GauntletCharacterDeveloperScreen
{
	private SpriteCategory _navalSpriteCategory;

	public GauntletNavalCharacterDeveloperScreen(CharacterDeveloperState clanState)
		: base(clanState)
	{
		_navalSpriteCategory = UIResourceManager.GetSpriteCategory("ui_naval_character_developer");
	}

	protected override void OnActivate()
	{
		((ScreenBase)this).OnActivate();
		Extensions.Load(_navalSpriteCategory);
	}

	protected override void OnDeactivate()
	{
		((ScreenBase)this).OnDeactivate();
		_navalSpriteCategory.Unload();
	}
}
