using NavalDLC.View.Map.Navigation;
using NavalDLC.ViewModelCollection.Map.MapBar;
using SandBox.GauntletUI.Map;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace NavalDLC.GauntletUI.Map;

[OverrideView(typeof(MapBarView))]
public class GauntletNavalMapBarView : GauntletMapBarView
{
	protected override void CreateLayout()
	{
		base._mapBarGlobalLayer = (GauntletMapBarGlobalLayer)(object)new GauntletNavalMapBarGlobalLayer(((MapView)this).MapScreen, (INavigationHandler)(object)new NavalMapNavigationHandler(), 8.5f);
		base._mapBarGlobalLayer.Initialize((MapBarVM)(object)new NavalMapBarVM());
		ScreenManager.AddGlobalLayer((GlobalLayer)(object)base._mapBarGlobalLayer, true);
	}
}
