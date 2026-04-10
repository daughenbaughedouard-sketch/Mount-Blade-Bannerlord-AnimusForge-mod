using System.Collections.Generic;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.HeirSelectionPopup;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(HeirSelectionPopupView))]
public class GauntletHeirSelectionPopupView : MapView
{
	private GauntletLayer _layerAsGauntletLayer;

	private HeirSelectionPopupVM _dataSource;

	private GauntletMovieIdentifier _movie;

	private readonly Dictionary<Hero, int> _heirApparents;

	private SpriteCategory _gameOverCategory;

	public GauntletHeirSelectionPopupView(Dictionary<Hero, int> heirApparents)
	{
		_heirApparents = heirApparents;
	}

	protected override void CreateLayout()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		base.CreateLayout();
		_gameOverCategory = UIResourceManager.LoadSpriteCategory("ui_gameover");
		_dataSource = new HeirSelectionPopupVM(_heirApparents);
		InitializeKeyVisuals();
		base.Layer = (ScreenLayer)new GauntletLayer("HeirSelectionPopup", 203, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		base.Layer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(base.Layer);
		_movie = _layerAsGauntletLayer.LoadMovie("HeirSelectionPopup", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
		base.MapScreen.SetIsHeirSelectionPopupActive(isHeirSelectionPopupActive: true);
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
		Campaign.Current.SetTimeControlModeLock(true);
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		HeirSelectionPopupVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.Update();
		}
		HandleInput();
	}

	protected override void OnMenuModeTick(float dt)
	{
		base.OnMenuModeTick(dt);
		HeirSelectionPopupVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.Update();
		}
		HandleInput();
	}

	protected override void OnIdleTick(float dt)
	{
		base.OnIdleTick(dt);
		HeirSelectionPopupVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.Update();
		}
		HandleInput();
	}

	protected override void OnFinalize()
	{
		_layerAsGauntletLayer.ReleaseMovie(_movie);
		_gameOverCategory.Unload();
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		_movie = null;
		_dataSource = null;
		base.Layer = null;
		_layerAsGauntletLayer = null;
		base.MapScreen.SetIsHeirSelectionPopupActive(isHeirSelectionPopupActive: false);
		Campaign.Current.SetTimeControlModeLock(false);
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

	protected override bool IsEscaped()
	{
		return true;
	}

	protected override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return false;
	}

	private void HandleInput()
	{
		if (_dataSource != null && base.Layer.Input.IsHotKeyReleased("Confirm"))
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			_dataSource.ExecuteSelectHeir();
		}
	}

	private void InitializeKeyVisuals()
	{
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
	}
}
