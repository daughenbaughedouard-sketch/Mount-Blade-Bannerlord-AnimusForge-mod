using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Map;

public class MapEventVisualsVM : ViewModel
{
	private readonly Camera _mapCamera;

	private readonly Dictionary<MapEvent, MapEventVisualItemVM> _eventToVisualMap = new Dictionary<MapEvent, MapEventVisualItemVM>();

	private readonly ParallelForAuxPredicate UpdateMapEventsAuxPredicate;

	private MBBindingList<MapEventVisualItemVM> _mapEvents;

	public MBBindingList<MapEventVisualItemVM> MapEvents
	{
		get
		{
			return _mapEvents;
		}
		set
		{
			if (_mapEvents != value)
			{
				_mapEvents = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MapEventVisualItemVM>>(value, "MapEvents");
			}
		}
	}

	public MapEventVisualsVM(Camera mapCamera)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		_mapCamera = mapCamera;
		MapEvents = new MBBindingList<MapEventVisualItemVM>();
		UpdateMapEventsAuxPredicate = new ParallelForAuxPredicate(UpdateMapEventsAux);
	}

	private void UpdateMapEventsAux(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			((Collection<MapEventVisualItemVM>)(object)MapEvents)[i].ParallelUpdatePosition();
			((Collection<MapEventVisualItemVM>)(object)MapEvents)[i].DetermineIsVisibleOnMap();
		}
	}

	public void Update(float dt)
	{
		TWParallel.For(0, ((Collection<MapEventVisualItemVM>)(object)MapEvents).Count, UpdateMapEventsAuxPredicate, 16);
		for (int i = 0; i < ((Collection<MapEventVisualItemVM>)(object)MapEvents).Count; i++)
		{
			((Collection<MapEventVisualItemVM>)(object)MapEvents)[i].UpdateBindingProperties();
		}
	}

	public void OnMapEventVisibilityChanged(MapEvent mapEvent)
	{
		if (_eventToVisualMap.ContainsKey(mapEvent))
		{
			_eventToVisualMap[mapEvent].UpdateProperties();
		}
	}

	public void OnMapEventStarted(MapEvent mapEvent)
	{
		if (_eventToVisualMap.ContainsKey(mapEvent))
		{
			if (!IsMapEventSettlementRelated(mapEvent))
			{
				_eventToVisualMap[mapEvent].UpdateProperties();
				return;
			}
			MapEventVisualItemVM item = _eventToVisualMap[mapEvent];
			((Collection<MapEventVisualItemVM>)(object)MapEvents).Remove(item);
			_eventToVisualMap.Remove(mapEvent);
		}
		else if (!IsMapEventSettlementRelated(mapEvent))
		{
			MapEventVisualItemVM mapEventVisualItemVM = new MapEventVisualItemVM(_mapCamera, mapEvent);
			_eventToVisualMap.Add(mapEvent, mapEventVisualItemVM);
			((Collection<MapEventVisualItemVM>)(object)MapEvents).Add(mapEventVisualItemVM);
			mapEventVisualItemVM.UpdateProperties();
		}
	}

	public void OnMapEventEnded(MapEvent mapEvent)
	{
		if (_eventToVisualMap.ContainsKey(mapEvent))
		{
			MapEventVisualItemVM item = _eventToVisualMap[mapEvent];
			((Collection<MapEventVisualItemVM>)(object)MapEvents).Remove(item);
			_eventToVisualMap.Remove(mapEvent);
		}
	}

	private bool IsMapEventSettlementRelated(MapEvent mapEvent)
	{
		return mapEvent.MapEventSettlement != null;
	}
}
