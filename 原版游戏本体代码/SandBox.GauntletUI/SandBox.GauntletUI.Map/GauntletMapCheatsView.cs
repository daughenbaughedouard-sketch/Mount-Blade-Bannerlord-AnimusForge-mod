using System.Collections.Generic;
using SandBox.View.Map;
using SandBox.ViewModelCollection.Map.Cheat;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapCheatsView))]
public class GauntletMapCheatsView : MapCheatsView
{
	protected GauntletLayer _layerAsGauntletLayer;

	protected GameplayCheatsVM _dataSource;

	protected override void CreateLayout()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		base.CreateLayout();
		IEnumerable<GameplayCheatBase> mapCheatList = GameplayCheatsManager.GetMapCheatList();
		_dataSource = new GameplayCheatsVM(HandleClose, mapCheatList);
		InitializeKeyVisuals();
		base.Layer = (ScreenLayer)new GauntletLayer("MapCheats", 4500, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		_layerAsGauntletLayer.LoadMovie("MapCheats", (ViewModel)(object)_dataSource);
		((ScreenLayer)_layerAsGauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_layerAsGauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_layerAsGauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_layerAsGauntletLayer);
		((ScreenBase)base.MapScreen).AddLayer((ScreenLayer)(object)_layerAsGauntletLayer);
		base.MapScreen.SetIsMapCheatsActive(isMapCheatsActive: true);
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
		Campaign.Current.SetTimeControlModeLock(true);
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		GameplayCheatsVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_layerAsGauntletLayer = null;
		base.Layer = null;
		_dataSource = null;
		base.MapScreen.SetIsMapCheatsActive(isMapCheatsActive: false);
		Campaign.Current.SetTimeControlModeLock(false);
	}

	private void HandleClose()
	{
		base.MapScreen.CloseGameplayCheats();
	}

	protected override bool IsEscaped()
	{
		return true;
	}

	protected override void OnIdleTick(float dt)
	{
		base.OnIdleTick(dt);
		HandleInput();
	}

	protected override void OnMenuModeTick(float dt)
	{
		base.OnMenuModeTick(dt);
		HandleInput();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		HandleInput();
	}

	private void HandleInput()
	{
		if (base.Layer.Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource?.ExecuteClose();
		}
	}

	private void InitializeKeyVisuals()
	{
		_dataSource.SetCloseInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
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
}
