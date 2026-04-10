using System.Collections.Generic;
using SandBox.View.Map;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapEscapeMenuView))]
public class GauntletMapEscapeMenuView : MapView
{
	private GauntletLayer _layerAsGauntletLayer;

	private EscapeMenuVM _escapeMenuDatasource;

	private GauntletMovieIdentifier _escapeMenuMovie;

	private readonly List<EscapeMenuItemVM> _menuItems;

	public GauntletMapEscapeMenuView(List<EscapeMenuItemVM> items)
	{
		_menuItems = items;
	}

	protected override void CreateLayout()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		base.CreateLayout();
		_escapeMenuDatasource = new EscapeMenuVM((IEnumerable<EscapeMenuItemVM>)_menuItems, (TextObject)null);
		base.Layer = (ScreenLayer)new GauntletLayer("MapEscapeMenu", 4400, false)
		{
			IsFocusLayer = true
		};
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		_escapeMenuMovie = _layerAsGauntletLayer.LoadMovie("EscapeMenu", (ViewModel)(object)_escapeMenuDatasource);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
		base.MapScreen.PauseAmbientSounds();
		ScreenManager.TrySetFocus(base.Layer);
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		if (base.Layer.Input.IsHotKeyReleased("ToggleEscapeMenu") || base.Layer.Input.IsHotKeyReleased("Exit"))
		{
			MapScreen.Instance.CloseEscapeMenu();
		}
	}

	protected override void OnIdleTick(float dt)
	{
		base.OnIdleTick(dt);
		if (base.Layer.Input.IsHotKeyReleased("ToggleEscapeMenu") || base.Layer.Input.IsHotKeyReleased("Exit"))
		{
			MapScreen.Instance.CloseEscapeMenu();
		}
	}

	protected override bool IsEscaped()
	{
		return base.Layer.Input.IsHotKeyReleased("ToggleEscapeMenu");
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		base.Layer.InputRestrictions.ResetInputRestrictions();
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		base.MapScreen.RestartAmbientSounds();
		ScreenManager.TryLoseFocus(base.Layer);
		base.Layer = null;
		_layerAsGauntletLayer = null;
		_escapeMenuDatasource = null;
		_escapeMenuMovie = null;
	}

	protected override TutorialContexts GetTutorialContext()
	{
		return (TutorialContexts)16;
	}
}
