using SandBox.View.Map;
using SandBox.View.Menu;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Menu;

[OverrideView(typeof(MenuOverlayBaseView))]
public class GauntletMenuOverlayBaseView : MenuView
{
	private GameMenuOverlay _overlayDataSource;

	private GauntletLayer _layerAsGauntletLayer;

	private bool _isContextMenuEnabled;

	protected override void OnInitialize()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		MenuOverlayType menuOverlayType = Campaign.Current.GameMenuManager.GetMenuOverlayType(base.MenuContext);
		_overlayDataSource = GameMenuOverlayFactory.GetOverlay(menuOverlayType);
		_overlayDataSource.SetExitInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		base.Layer = (ScreenLayer)new GauntletLayer("MapMenuOverlay", 202, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		((ScreenLayer)_layerAsGauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer.InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		base.MenuViewContext.AddLayer(base.Layer);
		if (_overlayDataSource is EncounterMenuOverlayVM)
		{
			_layerAsGauntletLayer.LoadMovie("EncounterOverlay", (ViewModel)(object)_overlayDataSource);
		}
		else if (_overlayDataSource is SettlementMenuOverlayVM)
		{
			_layerAsGauntletLayer.LoadMovie("SettlementOverlay", (ViewModel)(object)_overlayDataSource);
		}
		else if (_overlayDataSource is ArmyMenuOverlayVM)
		{
			Debug.FailedAssert("Trying to open army overlay in menu. Should be opened in map overlay", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Menu\\GauntletMenuOverlayBaseView.cs", "OnInitialize", 49);
		}
		else
		{
			Debug.FailedAssert("Game menu overlay not supported in gauntlet overlay", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Menu\\GauntletMenuOverlayBaseView.cs", "OnInitialize", 53);
		}
		base.OnInitialize();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		GameMenuOverlay overlayDataSource = _overlayDataSource;
		if (overlayDataSource != null)
		{
			overlayDataSource.OnFrameTick(dt);
		}
		if (ScreenManager.TopScreen is MapScreen && _overlayDataSource != null)
		{
			_overlayDataSource.IsInfoBarExtended = (ScreenManager.TopScreen as MapScreen)?.IsBarExtended ?? false;
		}
		if (!_isContextMenuEnabled && _overlayDataSource.IsContextMenuEnabled)
		{
			_isContextMenuEnabled = true;
			MapScreen.Instance?.SetIsOverlayContextMenuActive(isOverlayContextMenuEnabled: true);
			base.Layer.IsFocusLayer = true;
			ScreenManager.TrySetFocus(base.Layer);
		}
		else if (_isContextMenuEnabled && !_overlayDataSource.IsContextMenuEnabled)
		{
			_isContextMenuEnabled = false;
			MapScreen.Instance?.SetIsOverlayContextMenuActive(isOverlayContextMenuEnabled: false);
			base.Layer.IsFocusLayer = false;
			ScreenManager.TryLoseFocus(base.Layer);
		}
		if (_isContextMenuEnabled && base.Layer.Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_overlayDataSource.IsContextMenuEnabled = false;
		}
	}

	protected override void OnHourlyTick()
	{
		base.OnHourlyTick();
		GameMenuOverlay overlayDataSource = _overlayDataSource;
		if (overlayDataSource != null)
		{
			overlayDataSource.Refresh();
		}
	}

	protected override void OnOverlayTypeChange(MenuOverlayType newType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.OnOverlayTypeChange(newType);
		GameMenuOverlay overlayDataSource = _overlayDataSource;
		if (overlayDataSource != null)
		{
			overlayDataSource.UpdateOverlayType(newType);
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		GameMenuOverlay overlayDataSource = _overlayDataSource;
		if (overlayDataSource != null)
		{
			overlayDataSource.Refresh();
		}
	}

	protected override void OnFinalize()
	{
		MapScreen.Instance?.SetIsOverlayContextMenuActive(isOverlayContextMenuEnabled: false);
		base.MenuViewContext.RemoveLayer(base.Layer);
		((ViewModel)_overlayDataSource).OnFinalize();
		_overlayDataSource = null;
		base.Layer = null;
		_layerAsGauntletLayer = null;
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
