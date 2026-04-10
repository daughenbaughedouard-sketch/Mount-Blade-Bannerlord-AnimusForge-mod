using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.Credits;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

[OverrideView(typeof(CreditsScreen))]
public class GauntletCreditsScreen : ScreenBase
{
	private CreditsVM _datasource;

	private GauntletLayer _gauntletLayer;

	private GauntletMovieIdentifier _movie;

	private SpriteCategory _creditsCategory;

	protected override void OnInitialize()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		((ScreenBase)this).OnInitialize();
		_creditsCategory = UIResourceManager.LoadSpriteCategory("ui_credits");
		_datasource = new CreditsVM();
		string text = string.Concat(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/", "Credits.xml");
		_datasource.FillFromFile(text);
		_gauntletLayer = new GauntletLayer("CreditsScreen", 100, false);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		_movie = _gauntletLayer.LoadMovie("CreditsScreen", (ViewModel)(object)_datasource);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		InformationManager.HideAllMessages();
	}

	protected override void OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		_creditsCategory.Unload();
		((ViewModel)_datasource).OnFinalize();
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("Exit"))
		{
			ScreenManager.PopScreen();
		}
	}
}
