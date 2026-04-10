using System;
using System.Collections.ObjectModel;
using SandBox.View.Menu;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Menu;

[OverrideView(typeof(MenuBaseView))]
public class GauntletMenuBaseView : MenuView
{
	private GauntletLayer _layerAsGauntletLayer;

	private GauntletMovieIdentifier _movie;

	public GameMenuVM GameMenuDataSource { get; private set; }

	protected override void OnInitialize()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		base.OnInitialize();
		GameMenuDataSource = new GameMenuVM(base.MenuContext);
		GameKey gameKey = HotKeyManager.GetCategory("Generic").GetGameKey(4);
		GameMenuDataSource.SetLeaveHotKey(gameKey);
		base.Layer = (ScreenLayer)(object)base.MenuViewContext.FindLayer<GauntletLayer>("MapMenuView");
		if (base.Layer == null)
		{
			base.Layer = (ScreenLayer)new GauntletLayer("MapMenuView", 100, false);
			base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			base.MenuViewContext.AddLayer(base.Layer);
		}
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		_movie = _layerAsGauntletLayer.LoadMovie("GameMenu", (ViewModel)(object)GameMenuDataSource);
		ScreenManager.TrySetFocus(base.Layer);
		_layerAsGauntletLayer.UIContext.ContextAlpha = 1f;
		MBInformationManager.HideInformations();
		GainGamepadNavigationAfterSeconds(0.25f);
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		GameMenuDataSource.Refresh(true);
		GameMenuDataSource.SetIdleMode(false);
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		GameMenuDataSource.SetIdleMode(true);
	}

	protected override void OnResume()
	{
		base.OnResume();
		GameMenuDataSource.Refresh(true);
	}

	protected override void OnMenuContextRefreshed()
	{
		base.OnMenuContextRefreshed();
		GameMenuDataSource.Refresh(true);
	}

	protected override void OnFinalize()
	{
		((ViewModel)GameMenuDataSource).OnFinalize();
		GameMenuDataSource = null;
		ScreenManager.TryLoseFocus(base.Layer);
		_layerAsGauntletLayer.ReleaseMovie(_movie);
		_layerAsGauntletLayer = null;
		base.Layer = null;
		_movie = null;
		base.OnFinalize();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		GameMenuDataSource.OnFrameTick();
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

	protected override void OnMenuContextUpdated(MenuContext newMenuContext)
	{
		base.OnMenuContextUpdated(newMenuContext);
		GameMenuDataSource.UpdateMenuContext(newMenuContext);
	}

	protected override void OnBackgroundMeshNameSet(string name)
	{
		base.OnBackgroundMeshNameSet(name);
		GameMenuDataSource.Background = name;
	}

	private void GainGamepadNavigationAfterSeconds(float seconds)
	{
		_layerAsGauntletLayer.UIContext.GamepadNavigation.GainNavigationAfterTime(seconds, (Func<bool>)(() => ((Collection<GameMenuItemVM>)(object)GameMenuDataSource.ItemList).Count > 0));
	}
}
