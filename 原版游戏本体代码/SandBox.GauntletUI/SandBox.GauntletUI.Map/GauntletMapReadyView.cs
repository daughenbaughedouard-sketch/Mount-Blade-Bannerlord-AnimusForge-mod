using System;
using SandBox.View.Map;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapReadyView))]
public class GauntletMapReadyView : MapReadyView
{
	private GauntletLayer _layerAsGauntletLayer;

	private BoolItemWithActionVM _dataSource;

	protected override void CreateLayout()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		base.CreateLayout();
		_dataSource = new BoolItemWithActionVM((Action<object>)null, true, (object)null);
		_layerAsGauntletLayer = new GauntletLayer("MapReadyBlocker", 9999, false);
		_layerAsGauntletLayer.LoadMovie("MapReadyBlocker", (ViewModel)(object)_dataSource);
		base.Layer = (ScreenLayer)(object)_layerAsGauntletLayer;
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		((ViewModel)_dataSource).OnFinalize();
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		base.Layer = null;
		_dataSource = null;
	}

	public override void SetIsMapSceneReady(bool isReady)
	{
		base.SetIsMapSceneReady(isReady);
		_dataSource.IsActive = !isReady;
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
