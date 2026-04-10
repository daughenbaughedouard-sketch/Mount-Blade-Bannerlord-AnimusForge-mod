using System;
using SandBox.View.Map;
using SandBox.ViewModelCollection;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(BattleSimulationMapView))]
public class GauntletMapBattleSimulationView : MapView
{
	private GauntletLayer _layerAsGauntletLayer;

	private readonly SPScoreboardVM _dataSource;

	public GauntletMapBattleSimulationView(SPScoreboardVM dataSource)
	{
		_dataSource = dataSource;
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

	protected override void CreateLayout()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		base.CreateLayout();
		((ScoreboardBaseVM)_dataSource).Initialize((IMissionScreen)null, (Mission)null, (Action)base.MapState.EndBattleSimulation, (Action<bool>)null);
		((ScoreboardBaseVM)_dataSource).SetShortcuts(new ScoreboardHotkeys
		{
			ShowMouseHotkey = null,
			ShowScoreboardHotkey = null,
			DoneInputKey = HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"),
			FastForwardKey = HotKeyManager.GetCategory("ScoreboardHotKeyCategory").GetHotKey("ToggleFastForward"),
			PauseInputKey = HotKeyManager.GetCategory("ScoreboardHotKeyCategory").GetHotKey("TogglePause")
		});
		base.Layer = (ScreenLayer)new GauntletLayer("MapBattleSimulation", 101, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ScoreboardHotKeyCategory"));
		_layerAsGauntletLayer.LoadMovie("SPScoreboard", (ViewModel)(object)_dataSource);
		((ScoreboardBaseVM)_dataSource).ExecutePlayAction();
		base.Layer.IsFocusLayer = true;
		base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
		ScreenManager.TrySetFocus(base.Layer);
	}

	protected override void OnFinalize()
	{
		((ViewModel)_dataSource).OnFinalize();
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		base.Layer.IsFocusLayer = false;
		base.Layer.InputRestrictions.ResetInputRestrictions();
		ScreenManager.TryLoseFocus(base.Layer);
		_layerAsGauntletLayer = null;
		base.Layer = null;
	}

	protected override void OnMapScreenUpdate(float dt)
	{
		base.OnMapScreenUpdate(dt);
		if (_dataSource != null && base.Layer != null)
		{
			((ScoreboardBaseVM)_dataSource).Tick(dt);
			if (!((ScoreboardBaseVM)_dataSource).IsOver && base.Layer.Input.IsHotKeyReleased("ToggleFastForward"))
			{
				((ScoreboardBaseVM)_dataSource).IsFastForwarding = !((ScoreboardBaseVM)_dataSource).IsFastForwarding;
				((ScoreboardBaseVM)_dataSource).ExecuteFastForwardAction();
			}
			else if (!((ScoreboardBaseVM)_dataSource).IsOver && ((ScoreboardBaseVM)_dataSource).IsSimulation && ((ScoreboardBaseVM)_dataSource).ShowScoreboard && base.Layer.Input.IsHotKeyReleased("TogglePause"))
			{
				((ScoreboardBaseVM)_dataSource).IsPaused = !((ScoreboardBaseVM)_dataSource).IsPaused;
				((ScoreboardBaseVM)_dataSource).ExecutePauseSimulationAction();
			}
			else if (((ScoreboardBaseVM)_dataSource).IsOver && ((ScoreboardBaseVM)_dataSource).ShowScoreboard && base.Layer.Input.IsHotKeyPressed("Confirm"))
			{
				((ScoreboardBaseVM)_dataSource).ExecuteQuitAction();
			}
		}
	}
}
