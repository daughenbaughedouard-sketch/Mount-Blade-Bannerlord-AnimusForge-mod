using System.Collections.ObjectModel;
using SandBox.View.Map;
using SandBox.ViewModelCollection.Nameplate;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapPartyNameplateView))]
public class GauntletMapPartyNameplateView : MapView
{
	private GauntletLayer _layerAsGauntletLayer;

	private PartyNameplatesVM _dataSource;

	private GauntletMovieIdentifier _movie;

	protected override void CreateLayout()
	{
		base.CreateLayout();
		_dataSource = new PartyNameplatesVM(base.MapScreen.MapCameraView.Camera, base.MapScreen.FastMoveCameraToMainParty);
		GauntletMapBasicView mapView = base.MapScreen.GetMapView<GauntletMapBasicView>();
		base.Layer = (ScreenLayer)(object)mapView.GauntletNameplateLayer;
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		_movie = _layerAsGauntletLayer.LoadMovie("PartyNameplate", (ViewModel)(object)_dataSource);
		_dataSource.Initialize();
	}

	protected override void OnMapScreenUpdate(float dt)
	{
		base.OnMapScreenUpdate(dt);
		_dataSource.Update();
		bool shouldShowFullName = ((ScreenLayer)base.MapScreen.SceneLayer).Input.IsGameKeyDown(5);
		EncounterModel encounterModel = Campaign.Current.Models.EncounterModel;
		TextObject val = default(TextObject);
		for (int i = 0; i < ((Collection<PartyNameplateVM>)(object)_dataSource.Nameplates).Count; i++)
		{
			PartyNameplateVM partyNameplateVM = ((Collection<PartyNameplateVM>)(object)_dataSource.Nameplates)[i];
			partyNameplateVM.ShouldShowFullName = shouldShowFullName;
			partyNameplateVM.CanParley = partyNameplateVM.ShouldShowFullName && encounterModel.CanMainHeroDoParleyWithParty(partyNameplateVM.Party.Party, ref val);
		}
		if (_dataSource.PlayerNameplate != null)
		{
			_dataSource.PlayerNameplate.ShouldShowFullName = shouldShowFullName;
		}
	}

	protected override void OnResume()
	{
		base.OnResume();
		foreach (PartyNameplateVM item in (Collection<PartyNameplateVM>)(object)_dataSource.Nameplates)
		{
			item.RefreshDynamicProperties(forceUpdate: true);
		}
	}

	protected override void OnFinalize()
	{
		_layerAsGauntletLayer.ReleaseMovie(_movie);
		((ViewModel)_dataSource).OnFinalize();
		_layerAsGauntletLayer = null;
		base.Layer = null;
		_movie = null;
		_dataSource = null;
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
}
