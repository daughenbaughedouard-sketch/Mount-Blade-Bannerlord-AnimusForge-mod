using System.Collections.ObjectModel;
using SandBox.View.Map;
using SandBox.ViewModelCollection.Map.Tracker;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.Tracker;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapTrackersView))]
public class GauntletMapTrackersView : MapTrackersView
{
	private GauntletLayer _layerAsGauntletLayer;

	private GauntletMovieIdentifier _movie;

	private MapTrackerCollectionVM _dataSource;

	protected override void CreateLayout()
	{
		base.CreateLayout();
		_dataSource = new MapTrackerCollectionVM();
		MapTrackerItemVM.OnFastMoveCameraToPosition = FastMoveCameraToPosition;
		GauntletMapBasicView mapView = base.MapScreen.GetMapView<GauntletMapBasicView>();
		base.Layer = (ScreenLayer)(object)mapView.GauntletNameplateLayer;
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		_movie = _layerAsGauntletLayer.LoadMovie("MapTrackers", (ViewModel)(object)_dataSource);
	}

	protected override void OnResume()
	{
		base.OnResume();
		_dataSource.UpdateProperties();
	}

	private void UpdateTrackerPropertiesAux(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			MapTrackerItemVM mapTrackerItemVM = ((Collection<MapTrackerItemVM>)(object)_dataSource.Trackers)[i];
			mapTrackerItemVM.UpdateProperties();
			GetScreenPosition(mapTrackerItemVM.TrackedObject, out var screenX, out var screenY, out var screenW);
			mapTrackerItemVM.UpdatePosition(screenX, screenY, screenW);
		}
	}

	protected override void OnMapScreenUpdate(float dt)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		base.OnMapScreenUpdate(dt);
		TWParallel.For(0, ((Collection<MapTrackerItemVM>)(object)_dataSource.Trackers).Count, new ParallelForAuxPredicate(UpdateTrackerPropertiesAux), 32);
		_dataSource.Tick(dt);
	}

	protected override void OnFinalize()
	{
		MapTrackerItemVM.OnFastMoveCameraToPosition = null;
		((ViewModel)_dataSource).OnFinalize();
		_layerAsGauntletLayer.ReleaseMovie(_movie);
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

	private void GetScreenPosition(ITrackableCampaignObject trackable, out float screenX, out float screenY, out float screenW)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		Vec3 position = ((ITrackableBase)trackable).GetPosition();
		IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
		CampaignVec2 val = new CampaignVec2(((Vec3)(ref position)).AsVec2, true);
		mapSceneWrapper.GetHeightAtPoint(ref val, ref num);
		position.z = MathF.Max(num, 0f);
		screenX = -5000f;
		screenY = -5000f;
		screenW = -1f;
		MBWindowManager.WorldToScreenInsideUsableArea(base.MapScreen.MapCameraView.Camera, position, ref screenX, ref screenY, ref screenW);
	}

	private void FastMoveCameraToPosition(CampaignVec2 target)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		base.MapScreen.FastMoveCameraToPosition(target);
	}
}
