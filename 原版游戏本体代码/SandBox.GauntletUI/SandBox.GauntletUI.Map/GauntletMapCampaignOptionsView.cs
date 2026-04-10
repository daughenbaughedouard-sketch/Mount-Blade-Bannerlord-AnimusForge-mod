using System;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapCampaignOptionsView))]
public class GauntletMapCampaignOptionsView : MapCampaignOptionsView
{
	private CampaignOptionsVM _dataSource;

	private GauntletLayer _layerAsGauntletLayer;

	protected override void CreateLayout()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		base.CreateLayout();
		_dataSource = new CampaignOptionsVM((Action)OnClose);
		base.Layer = (ScreenLayer)new GauntletLayer("MapCampaignOptions", 4401, false)
		{
			IsFocusLayer = true
		};
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		_layerAsGauntletLayer.LoadMovie("CampaignOptions", (ViewModel)(object)_dataSource);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
		base.MapScreen.PauseAmbientSounds();
		ScreenManager.TrySetFocus(base.Layer);
	}

	private void OnClose()
	{
		MapScreen.Instance.CloseCampaignOptions();
	}

	protected override void OnIdleTick(float dt)
	{
		base.OnFrameTick(dt);
		if (base.Layer.Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteDone();
		}
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		base.Layer.InputRestrictions.ResetInputRestrictions();
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		base.MapScreen.RestartAmbientSounds();
		ScreenManager.TryLoseFocus(base.Layer);
		base.Layer = null;
		_dataSource = null;
		_layerAsGauntletLayer = null;
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
