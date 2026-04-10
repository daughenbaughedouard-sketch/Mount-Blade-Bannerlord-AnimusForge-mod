using SandBox.View.Map;
using SandBox.View.Map.Navigation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapBarView))]
public class GauntletMapBarView : MapView
{
	protected GauntletMapBarGlobalLayer _mapBarGlobalLayer;

	protected override void OnMapConversationStart()
	{
		base.OnMapConversationStart();
		_mapBarGlobalLayer.OnMapConversationStarted();
	}

	protected override void OnMapConversationOver()
	{
		base.OnMapConversationOver();
		_mapBarGlobalLayer.OnMapConversationOver();
	}

	protected override void CreateLayout()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		base.CreateLayout();
		_mapBarGlobalLayer = new GauntletMapBarGlobalLayer(base.MapScreen, (INavigationHandler)(object)new MapNavigationHandler(), 8.5f);
		_mapBarGlobalLayer.Initialize(new MapBarVM());
		ScreenManager.AddGlobalLayer((GlobalLayer)(object)_mapBarGlobalLayer, true);
	}

	protected override void OnFinalize()
	{
		_mapBarGlobalLayer.OnFinalize();
		ScreenManager.RemoveGlobalLayer((GlobalLayer)(object)_mapBarGlobalLayer);
		base.OnFinalize();
	}

	protected override void OnResume()
	{
		base.OnResume();
		_mapBarGlobalLayer.Refresh();
	}

	protected override bool IsEscaped()
	{
		return _mapBarGlobalLayer.IsEscaped();
	}

	protected override TutorialContexts GetTutorialContext()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (_mapBarGlobalLayer.IsInArmyManagement)
		{
			return (TutorialContexts)10;
		}
		return base.GetTutorialContext();
	}
}
