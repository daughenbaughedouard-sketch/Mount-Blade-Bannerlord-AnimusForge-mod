using System;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapOverlayView))]
public class GauntletMapOverlayView : MapView
{
	private GauntletLayer _layerAsGauntletLayer;

	private GameMenuOverlay _overlayDataSource;

	private readonly MapScreen.MapOverlayType _type;

	private GauntletMovieIdentifier _movie;

	private bool _isContextMenuEnabled;

	private GauntletLayer _armyManagementLayer;

	private SpriteCategory _armyManagementCategory;

	private ArmyManagementVM _armyManagementDatasource;

	private GauntletMovieIdentifier _gauntletArmyManagementMovie;

	private CampaignTimeControlMode _timeControlModeBeforeArmyManagementOpened;

	public bool IsInArmyManagement
	{
		get
		{
			if (_armyManagementLayer != null)
			{
				return _armyManagementDatasource != null;
			}
			return false;
		}
	}

	public GauntletMapOverlayView(MapScreen.MapOverlayType type)
	{
		_type = type;
	}

	protected override void CreateLayout()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		base.CreateLayout();
		_overlayDataSource = GetOverlay(_type);
		_overlayDataSource.SetExitInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		base.Layer = (ScreenLayer)new GauntletLayer("MapArmyOverlay", 201, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer.InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		MapScreen.MapOverlayType type = _type;
		if (type == MapScreen.MapOverlayType.Army)
		{
			_movie = _layerAsGauntletLayer.LoadMovie("ArmyOverlay", (ViewModel)(object)_overlayDataSource);
			GameMenuOverlay overlayDataSource = _overlayDataSource;
			((ArmyMenuOverlayVM)((overlayDataSource is ArmyMenuOverlayVM) ? overlayDataSource : null)).OpenArmyManagement = OpenArmyManagement;
		}
		else
		{
			Debug.FailedAssert("This kind of overlay not supported in gauntlet", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Map\\GauntletMapOverlayView.cs", "CreateLayout", 62);
		}
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
	}

	public GameMenuOverlay GetOverlay(MapScreen.MapOverlayType mapOverlayType)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		if (mapOverlayType == MapScreen.MapOverlayType.Army)
		{
			return (GameMenuOverlay)new ArmyMenuOverlayVM();
		}
		Debug.FailedAssert("Game menu overlay: " + mapOverlayType.ToString() + " could not be found", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Map\\GauntletMapOverlayView.cs", "GetOverlay", 75);
		return null;
	}

	protected override void OnArmyLeft()
	{
		base.MapScreen.RemoveArmyOverlay();
	}

	protected override TutorialContexts GetTutorialContext()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (IsInArmyManagement)
		{
			return (TutorialContexts)10;
		}
		return base.GetTutorialContext();
	}

	protected override void OnFinalize()
	{
		if (_armyManagementLayer != null)
		{
			CloseArmyManagement();
		}
		_layerAsGauntletLayer.ReleaseMovie(_movie);
		if (_gauntletArmyManagementMovie != null)
		{
			_layerAsGauntletLayer.ReleaseMovie(_gauntletArmyManagementMovie);
		}
		base.MapScreen.SetIsOverlayContextMenuActive(isOverlayContextMenuEnabled: false);
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		_movie = null;
		_overlayDataSource = null;
		base.Layer = null;
		_layerAsGauntletLayer = null;
		SpriteCategory armyManagementCategory = _armyManagementCategory;
		if (armyManagementCategory != null)
		{
			armyManagementCategory.Unload();
		}
		base.OnFinalize();
	}

	protected override void OnMapConversationStart()
	{
		base.OnMapConversationStart();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, true);
		}
	}

	protected override void OnMapConversationOver()
	{
		base.OnMapConversationOver();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, false);
		}
	}

	protected override void OnHourlyTick()
	{
		base.OnHourlyTick();
		GameMenuOverlay overlayDataSource = _overlayDataSource;
		if (overlayDataSource != null)
		{
			overlayDataSource.HourlyTick();
		}
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		if (ScreenManager.TopScreen is MapScreen mapScreen && _overlayDataSource != null)
		{
			_overlayDataSource.IsInfoBarExtended = mapScreen.IsBarExtended;
		}
		GameMenuOverlay overlayDataSource = _overlayDataSource;
		if (overlayDataSource != null)
		{
			overlayDataSource.OnFrameTick(dt);
		}
	}

	protected override bool IsEscaped()
	{
		if (_armyManagementDatasource != null)
		{
			_armyManagementDatasource.ExecuteCancel();
			return true;
		}
		return false;
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

	protected override void OnResume()
	{
		base.OnResume();
		GameMenuOverlay overlayDataSource = _overlayDataSource;
		if (overlayDataSource != null)
		{
			overlayDataSource.Refresh();
		}
	}

	protected override void OnMapScreenUpdate(float dt)
	{
		base.OnMapScreenUpdate(dt);
		if (!_isContextMenuEnabled && _overlayDataSource.IsContextMenuEnabled)
		{
			_isContextMenuEnabled = true;
			base.MapScreen.SetIsOverlayContextMenuActive(isOverlayContextMenuEnabled: true);
			base.Layer.IsFocusLayer = true;
			ScreenManager.TrySetFocus(base.Layer);
		}
		else if (_isContextMenuEnabled && !_overlayDataSource.IsContextMenuEnabled)
		{
			_isContextMenuEnabled = false;
			base.MapScreen.SetIsOverlayContextMenuActive(isOverlayContextMenuEnabled: false);
			base.Layer.IsFocusLayer = false;
			ScreenManager.TryLoseFocus(base.Layer);
		}
		if (_isContextMenuEnabled && base.Layer.Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_overlayDataSource.IsContextMenuEnabled = false;
		}
		HandleArmyManagementInput();
	}

	protected override void OnMenuModeTick(float dt)
	{
		base.OnMenuModeTick(dt);
		if (ScreenManager.TopScreen is MapScreen mapScreen && _overlayDataSource != null)
		{
			_overlayDataSource.IsInfoBarExtended = mapScreen.IsBarExtended;
		}
		GameMenuOverlay overlayDataSource = _overlayDataSource;
		if (overlayDataSource != null)
		{
			overlayDataSource.OnFrameTick(dt);
		}
	}

	private void OpenArmyManagement()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		_armyManagementDatasource = new ArmyManagementVM((Action)CloseArmyManagement);
		_armyManagementDatasource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_armyManagementDatasource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_armyManagementDatasource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_armyManagementDatasource.SetRemoveInputKey(HotKeyManager.GetCategory("ArmyManagementHotkeyCategory").GetHotKey("RemoveParty"));
		_armyManagementCategory = UIResourceManager.LoadSpriteCategory("ui_armymanagement");
		_armyManagementLayer = new GauntletLayer("MapArmyManagement", 300, false);
		_gauntletArmyManagementMovie = _armyManagementLayer.LoadMovie("ArmyManagement", (ViewModel)(object)_armyManagementDatasource);
		((ScreenLayer)_armyManagementLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_armyManagementLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_armyManagementLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ArmyManagementHotkeyCategory"));
		((ScreenLayer)_armyManagementLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_armyManagementLayer);
		((ScreenBase)base.MapScreen).AddLayer((ScreenLayer)(object)_armyManagementLayer);
		_timeControlModeBeforeArmyManagementOpened = Campaign.Current.TimeControlMode;
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
		Campaign.Current.SetTimeControlModeLock(true);
		if (ScreenManager.TopScreen is MapScreen mapScreen)
		{
			mapScreen.SetIsInArmyManagement(isInArmyManagement: true);
		}
	}

	private void CloseArmyManagement()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (_armyManagementLayer != null && _gauntletArmyManagementMovie != null)
		{
			((ScreenLayer)_armyManagementLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_armyManagementLayer);
			((ScreenLayer)_armyManagementLayer).InputRestrictions.ResetInputRestrictions();
			_armyManagementLayer.ReleaseMovie(_gauntletArmyManagementMovie);
			((ScreenBase)base.MapScreen).RemoveLayer((ScreenLayer)(object)_armyManagementLayer);
		}
		ArmyManagementVM armyManagementDatasource = _armyManagementDatasource;
		if (armyManagementDatasource != null)
		{
			((ViewModel)armyManagementDatasource).OnFinalize();
		}
		_gauntletArmyManagementMovie = null;
		_armyManagementDatasource = null;
		_armyManagementLayer = null;
		GameMenuOverlay overlayDataSource = _overlayDataSource;
		if (overlayDataSource != null)
		{
			overlayDataSource.Refresh();
		}
		if (ScreenManager.TopScreen is MapScreen mapScreen)
		{
			mapScreen.SetIsInArmyManagement(isInArmyManagement: false);
		}
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)4));
		Campaign.Current.SetTimeControlModeLock(false);
		Campaign.Current.TimeControlMode = _timeControlModeBeforeArmyManagementOpened;
	}

	private void HandleArmyManagementInput()
	{
		if (_armyManagementLayer != null && _armyManagementDatasource != null)
		{
			if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_armyManagementDatasource.ExecuteCancel();
			}
			else if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("Confirm"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_armyManagementDatasource.ExecuteDone();
			}
			else if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("Reset"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_armyManagementDatasource.ExecuteReset();
			}
			else if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("RemoveParty") && _armyManagementDatasource.FocusedItem != null)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_armyManagementDatasource.FocusedItem.ExecuteAction();
			}
		}
	}
}
