using SandBox.View.Map;
using SandBox.ViewModelCollection.SaveLoad;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapSaveView))]
public class GauntletMapSaveView : MapView
{
	private GauntletLayer _layerAsGauntletLayer;

	private MapSaveVM _dataSource;

	protected override void CreateLayout()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		base.CreateLayout();
		_dataSource = new MapSaveVM(OnStateChange);
		_layerAsGauntletLayer = new GauntletLayer("MapSave", 10000, false);
		_layerAsGauntletLayer.LoadMovie("MapSave", (ViewModel)(object)_dataSource);
		base.Layer = (ScreenLayer)(object)_layerAsGauntletLayer;
		base.Layer.InputRestrictions.SetInputRestrictions(false, (InputUsageMask)5);
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
	}

	private void OnStateChange(bool isActive)
	{
		if (isActive)
		{
			base.Layer.IsFocusLayer = true;
			ScreenManager.TrySetFocus(base.Layer);
			base.Layer.InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		}
		else
		{
			base.Layer.IsFocusLayer = false;
			ScreenManager.TryLoseFocus(base.Layer);
			base.Layer.InputRestrictions.ResetInputRestrictions();
		}
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		((ViewModel)_dataSource).OnFinalize();
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		base.Layer = null;
		_dataSource = null;
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
