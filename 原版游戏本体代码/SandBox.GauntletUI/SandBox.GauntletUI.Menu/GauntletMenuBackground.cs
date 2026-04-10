using SandBox.View.Menu;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Menu;

[OverrideView(typeof(MenuBackgroundView))]
public class GauntletMenuBackground : MenuView
{
	private GauntletLayer _layerAsGauntletLayer;

	private GauntletMovieIdentifier _movie;

	protected override void OnInitialize()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		base.OnInitialize();
		_layerAsGauntletLayer = base.MenuViewContext.FindLayer<GauntletLayer>("MapMenuView");
		if (_layerAsGauntletLayer == null)
		{
			_layerAsGauntletLayer = new GauntletLayer("MapMenuView", 100, false);
			base.MenuViewContext.AddLayer((ScreenLayer)(object)_layerAsGauntletLayer);
		}
		base.Layer = (ScreenLayer)(object)_layerAsGauntletLayer;
		_movie = _layerAsGauntletLayer.LoadMovie("GameMenuBackground", (ViewModel)null);
		((ScreenLayer)_layerAsGauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
	}

	protected override void OnFinalize()
	{
		GauntletLayer layerAsGauntletLayer = _layerAsGauntletLayer;
		if (layerAsGauntletLayer != null)
		{
			layerAsGauntletLayer.ReleaseMovie(_movie);
		}
		_layerAsGauntletLayer = null;
		base.Layer = null;
		_movie = null;
		base.OnFinalize();
	}

	protected override void OnMapConversationActivated()
	{
		base.OnMapConversationActivated();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, true);
		}
	}

	protected override void OnMapConversationDeactivated()
	{
		base.OnMapConversationDeactivated();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, false);
		}
	}
}
